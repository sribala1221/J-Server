﻿using System;
using System.Collections.Generic;
using System.Linq;
using GenerateTables.Models;
using ServerAPI.ViewModels;
using ServerAPI.Utilities;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Web;

namespace ServerAPI.Services
{
    public class BookingService : IBookingService
    {
        private readonly AAtims _context;
        private readonly ICommonService _commonService;
        private readonly IBookingReleaseService _bookingReleaseService;
        private readonly int _personnelId;
        private List<LookupVm> _lookupList;
        private readonly IPrebookWizardService _preBookWizardService;
        private readonly IPrebookActiveService _preBookActiveService;
        private readonly ISentenceService _sentenceService;
        private readonly IWizardService _wizardService;
        private readonly IPersonService _iPersonService;
        private readonly IInmateService _inmateService;
        private readonly IPhotosService _photos;
        private readonly IInterfaceEngineService _interfaceEngine;

        public BookingService(AAtims context, ICommonService commonService,
            IHttpContextAccessor httpContextAccessor, IBookingReleaseService bookingReleaseService,
            IPrebookWizardService preBookWizardService, IPrebookActiveService preBookActiveService,
            ISentenceService sentenceService, IWizardService wizardService, IPersonService personService,
            IInmateService inmateService, IPhotosService photosService, IInterfaceEngineService interfaceEngineService)
        {
            _context = context;
            _commonService = commonService;
            _preBookWizardService = preBookWizardService;
            _preBookActiveService = preBookActiveService;
            _sentenceService = sentenceService;
            _personnelId = Convert.ToInt32(httpContextAccessor.HttpContext.User.FindFirst("personnelId")?.Value);
            _bookingReleaseService = bookingReleaseService;
            _wizardService = wizardService;
            _lookupList = new List<LookupVm>();
            _iPersonService = personService;
            _inmateService = inmateService;
            _photos = photosService;
            _interfaceEngine = interfaceEngineService;
        }
        
        // update Booking Complete details
        public async Task<int> UpdateBookingComplete(BookingComplete bookingComplete)
        {
            Incarceration incDetails = _context.Incarceration.Single(inc =>
                inc.InmateId == bookingComplete.InmateId && inc.IncarcerationId == bookingComplete.IncarcerationId);

            if (bookingComplete.BookingDataFlag == false)
            {
                incDetails.BookCompleteFlag = bookingComplete.IsComplete ? 1 : (int?) null;
                incDetails.BookCompleteDate = DateTime.Now;
                incDetails.BookCompleteBy = _personnelId;
            }

            int personId = _iPersonService.GetInmateDetails(incDetails.InmateId ?? 0).PersonId;

            _interfaceEngine.Export(new ExportRequestVm
            {
                EventName = EventNameConstants.INCARCERATIONCOMPLETE,
                PersonnelId = _personnelId,
                Param1 = personId.ToString(),
                Param2 = bookingComplete.IncarcerationId.ToString()
            });

            if (!incDetails.BookCompleteFlag.HasValue && bookingComplete.IsComplete)
            {
                string webServiceEventName = bookingComplete.BookingDataFlag
                    ? LookupConstants.CASEDATACOMPLETE
                    : LookupConstants.INCARCERATIONCOMPLETE;

                bool eventQueueFlag = (_context.WebServiceEventSetting.FirstOrDefault(wse => wse.EventQueueFlag == 1)
                    ?.EventQueueFlag).HasValue;

                incDetails.BookCompleteFlag = bookingComplete.IsComplete ? 1 : (int?) null;

                if (eventQueueFlag)
                {
                     _interfaceEngine.Export(new ExportRequestVm
                    {
                        EventName = webServiceEventName,
                        PersonnelId = _personnelId,
                        Param1 = personId.ToString(),
                        Param2 = bookingComplete.IncarcerationId.ToString()
                    });
                }
            }
            else
            {
                if (!bookingComplete.BookingDataFlag)
                {
                    incDetails.BookCompleteFlag = bookingComplete.IsComplete ? 1 : default;
                }
            }

            _inmateService.CreateTask(bookingComplete.InmateId, incDetails.NoKeeper
                ? TaskValidateType.BookingCompleteNonKeeperEvent
                : TaskValidateType.BookingCompleteKeeperEvent);


            // complete assessment workflow automatically if site option in ON for NON KEEPER
            if (incDetails.NoKeeper && _context.SiteOptions.SingleOrDefault(sit =>
                        sit.SiteOptionsName == SiteOptionsConstants.BYPASSASSESSMENTFORNONKEEPER &&
                        sit.SiteOptionsStatus == "1")
                    ?.SiteOptionsValue == SiteOptionsConstants.ON)
            {
                incDetails.AssessmentCompleteFlag = bookingComplete.IsComplete;
                incDetails.AssessmentCompleteDate = DateTime.Now;
                incDetails.AssessmentCompleteBy = _personnelId;
                incDetails.AssessmentCompleteByNavigation =
                    _context.Personnel.Single(per => per.PersonnelId == _personnelId);

                AoWizardProgressVm wizardProgress = new AoWizardProgressVm
                {
                    IncarcerationId = bookingComplete.IncarcerationId,
                    WizardId = 16

                };
                wizardProgress.WizardProgressId = await _wizardService.CreateWizardProgress(wizardProgress);

                AoWizardStepProgressVm wizardStep = new AoWizardStepProgressVm
                {
                    ComponentId = 98,
                    WizardProgressId = wizardProgress.WizardProgressId,
                    StepComplete = true,
                    StepCompleteById = _personnelId,
                    StepCompleteDate = DateTime.Now
                };

                await _wizardService.SetWizardStepComplete(wizardStep);
            }

            //complete assessment workflow automatically if site option in ON for  KEEPER
           if (!incDetails.NoKeeper && _context.SiteOptions.SingleOrDefault(sit =>
                        sit.SiteOptionsName == SiteOptionsConstants.BYPASSASSESSMENTFORKEEPER &&
                        sit.SiteOptionsStatus == "1")
                    ?.SiteOptionsValue == SiteOptionsConstants.ON)
            {
                incDetails.AssessmentCompleteFlag = bookingComplete.IsComplete;
                incDetails.AssessmentCompleteDate = DateTime.Now;
                incDetails.AssessmentCompleteBy = _personnelId;
                incDetails.AssessmentCompleteByNavigation =
                    _context.Personnel.Single(per => per.PersonnelId == _personnelId);

                AoWizardProgressVm wizardProgress = new AoWizardProgressVm
                {
                    IncarcerationId = bookingComplete.IncarcerationId,
                    WizardId = 16

                };
                wizardProgress.WizardProgressId = await _wizardService.CreateWizardProgress(wizardProgress);

                AoWizardStepProgressVm wizardStep = new AoWizardStepProgressVm
                {
                    ComponentId = 98,
                    WizardProgressId = wizardProgress.WizardProgressId,
                    StepComplete = true,
                    StepCompleteById = _personnelId,
                    StepCompleteDate = DateTime.Now
                };

                await _wizardService.SetWizardStepComplete(wizardStep);
            }

            return await _context.SaveChangesAsync();
        }

        public List<InmateHousing> GetInmateDetails(List<int> inmateIds) =>
            _context.Inmate.Where(x => inmateIds.Contains(x.InmateId)).Select(a => new InmateHousing
            {
                InmateId = a.InmateId,
                PersonId = a.PersonId,
                PersonFirstName = a.Person.PersonFirstName,
                PersonMiddleName = a.Person.PersonMiddleName,
                PersonLastName = a.Person.PersonLastName,
                PersonSuffix = a.Person.PersonSuffix,
                PersonDob = a.Person.PersonDob,
                InmateNumber = a.InmateNumber,
                FacilityId = a.FacilityId,
                LastReviewDate = a.LastReviewDate,
                InmateCurrentTrack = a.InmateCurrentTrack,
                HousingUnitId = a.HousingUnitId,
                HousingLocation = a.HousingUnit.HousingUnitLocation,
                HousingNumber = a.HousingUnit.HousingUnitNumber,
                HousingBedLocation = a.HousingUnit.HousingUnitBedLocation,
                HousingBedNumber = a.HousingUnit.HousingUnitBedNumber,
                PersonSexLast = a.Person.PersonSexLast,
                InmateClassificationReason = a.InmateClassification.InmateClassificationReason,
                InmateActive = a.InmateActive == 1,
                InmateClassificationId=a.InmateClassificationId,
                PhotoFilePath = _photos.GetPhotoByCollectionIdentifier(a.Person.Identifiers)
            }).ToList();

        public async Task<int> UpdateRequest(RequestClear requestClear)
        {
            Request req = _context.Request.Single(re => re.RequestId == requestClear.RequestId);
            req.PendingBy = _personnelId;
            req.PendingDate = DateTime.Now;

            RequestTrack reqTrack = new RequestTrack
            {
                RequestId = requestClear.RequestId,
                RequestTrackDate = DateTime.Now,
                RequestTrackBy = _personnelId,
            };

            if (requestClear.IsPendingType)
            {
                reqTrack.RequestTrackCategory = RequestTrackCategory.REQUESTACCEPTED;
            }
            else
            {
                req.PendingBy = null;
                req.PendingDate = null;
                reqTrack.RequestTrackCategory = RequestTrackCategory.UNDOREQUESTACCEPTED;
            }

            _context.RequestTrack.Add(reqTrack);
            return await _context.SaveChangesAsync();
        }

        public async Task<int> UpdateClearRequest(RequestClear requestClear)
        {
            Request req = _context.Request.Single(re => re.RequestId == requestClear.RequestId);
            req.ClearedBy = _personnelId;
            req.ClearedDate = DateTime.Now;

            if (requestClear.IsPendingType)
            {
                req.PendingBy = _personnelId;
                req.PendingDate = DateTime.Now;
            }

            RequestTrack reqTrack = new RequestTrack
            {
                RequestId = requestClear.RequestId,
                RequestTrackDate = DateTime.Now,
                RequestTrackBy = _personnelId,
                RequestTrackCategory = RequestTrackCategory.REQUESTCLEARED
            };

            _context.RequestTrack.Add(reqTrack);
            return await _context.SaveChangesAsync();
        }

        public BookingNoteVm GetBookingNoteDetails(int arrestId, bool deleteFlag)
        {
            BookingNoteVm bookingNoteVm = new BookingNoteVm
            {
                ListBookingNoteDetails = _context.ArrestNote
                    .Where(i => i.ArrestId == arrestId && (deleteFlag ? i.DeleteFlag >= 0 : i.DeleteFlag == 0))
                    .Select(i => new BookingNoteDetailsVm
                    {
                        NoteDate = i.NoteDate,
                        NoteType = i.NoteType,
                        NoteText = i.NoteText,
                        DeleteFlag = i.DeleteFlag,
                        ArrestNoteId = i.ArrestNoteId,
                        Officerbadgenumber = i.NoteByNavigation.OfficerBadgeNum,
                        Personlastname = i.NoteByNavigation.PersonNavigation.PersonLastName
                    }).OrderByDescending(i => i.NoteDate).ToList()
            };

            bookingNoteVm.ListBookingNoteCount = new List<KeyValuePair<string, int>>
            {
                new KeyValuePair<string, int>("ALL", bookingNoteVm.ListBookingNoteDetails.Count)
            };

            bookingNoteVm.ListBookingNoteCount.AddRange(bookingNoteVm.ListBookingNoteDetails
                .GroupBy(s => s.NoteType).Select(s => new KeyValuePair<string, int>(s.Key, s.Count()))
                .OrderBy(o => o.Key).ToList());

            return bookingNoteVm;
        }

        public async Task<int> DeleteBookingNote(BookingNoteDetailsVm value)
        {
            ArrestNote arrestNote = _context.ArrestNote.Single(s => s.ArrestNoteId == value.ArrestNoteId);
            {
                arrestNote.DeleteFlag = value.DeleteFlag;
                arrestNote.DeleteBy = value.DeleteFlag > 0 ?_personnelId : default(int?);
                arrestNote.DeleteDate = value.DeleteFlag > 0 ? DateTime.Now : default(DateTime?);
            }
            return await _context.SaveChangesAsync();
        }

        //Record Check Drop down binding
        public ExtAttachApproveVm GetExternalAttachmentDetails(int inmateId)
        {
            ExtAttachApproveVm eaaVm = new ExtAttachApproveVm
            {
                IncarcerationDetails = _context.Incarceration.Where(inc => inc.InmateId == inmateId).Select(
                    inc => new IncarcerationDetail
                    {
                        IncarcerationId = inc.IncarcerationId,
                        ReleaseOut = inc.ReleaseOut,
                        DateIn = inc.DateIn,
                        UsedPersonFirst = inc.UsedPersonFrist,
                        UsedPersonLast = inc.UsedPersonLast,
                        UsedPersonMiddle = inc.UsedPersonMiddle
                    }).OrderBy(inc => inc.DateIn).ToList()
            };

            int? incarcerationId = _context.Incarceration
                .SingleOrDefault(inc => inc.InmateId == inmateId && !inc.ReleaseOut.HasValue)?.IncarcerationId;

            int[] lstArrestId;
            if (incarcerationId.HasValue)
            {
                lstArrestId = _context.IncarcerationArrestXref
                    .Where(iax => iax.IncarcerationId == incarcerationId && iax.ArrestId.HasValue)
                    .Select(iax => iax.ArrestId.Value).ToArray();
            }
            else
            {
                incarcerationId = _context.Incarceration
                    .OrderByDescending(inc => inc.DateIn).First(inc => inc.InmateId == inmateId).IncarcerationId;

                lstArrestId = _context.IncarcerationArrestXref
                    .Where(iax => iax.IncarcerationId == incarcerationId && iax.ArrestId.HasValue)
                    .Select(iax => iax.ArrestId.Value).ToArray();
            }

            List<string> lookupTypes = new List<string>
            {
                LookupConstants.ARRTYPE,
                LookupConstants.INCARATTACHTYPE,
                LookupConstants.BOOKATTACHTYPE
            };

            IQueryable<Lookup> lstLook =
                _context.Lookup.Where(look => lookupTypes.Contains(look.LookupType) && look.LookupInactive == 0);

            eaaVm.BookingDetails = _context.Arrest.Where(arr => lstArrestId.Contains(arr.ArrestId)).Select(
                inc => new ArrestDetails
                {
                    ArrestId = inc.ArrestId,
                    BookingNumber = inc.ArrestBookingNo,
                    ArrestCourtDocket = inc.ArrestCourtDocket,
                    ArrestTypeId = inc.ArrestType,
                    ArrestType = inc.ArrestType != null
                        ? lstLook.SingleOrDefault(look =>
                            Convert.ToString(look.LookupIndex) == inc.ArrestType.Trim() &&
                            look.LookupType == LookupConstants.ARRTYPE).LookupDescription
                        : null
                }).ToList();

            eaaVm.IncarcerationType = lstLook
                .Where(look => look.LookupDescription != null && look.LookupType == LookupConstants.INCARATTACHTYPE)
                .Select(look => new LookupVm
                {
                    LookupDescription = look.LookupDescription,
                    LookupIndex = look.LookupIndex
                }).ToList();

            eaaVm.BookingType = lstLook
                .Where(look => look.LookupDescription != null && look.LookupType == LookupConstants.BOOKATTACHTYPE)
                .Select(look => new LookupVm
                {
                    LookupDescription = look.LookupDescription,
                    LookupIndex = look.LookupIndex
                }).ToList();

            return eaaVm;
        }

        public Task<int> UpdateExternalAttachment(ExternalAttachmentsVm eaVm)
        {
            Personnel updateBy = _context.Personnel.Single(per => per.PersonnelId == _personnelId);

            AppletsSaved updateRecordsCheck =
                _context.AppletsSaved.Single(r => r.AppletsSavedId == eaVm.AppletsSavedId);

            updateRecordsCheck.AppletsSavedType = eaVm.AppletsSavedType;
            updateRecordsCheck.AppletsSavedTitle = eaVm.AppletsSavedTitle;
            updateRecordsCheck.AppletsSavedDescription = eaVm.AppletsSavedDescription;

            updateRecordsCheck.AppletsSavedKeyword1 = eaVm.AppletsSavedKeyword1;
            updateRecordsCheck.AppletsSavedKeyword2 = eaVm.AppletsSavedKeyword2;
            updateRecordsCheck.AppletsSavedKeyword3 = eaVm.AppletsSavedKeyword3;
            updateRecordsCheck.AppletsSavedKeyword4 = eaVm.AppletsSavedKeyword4;
            updateRecordsCheck.AppletsSavedKeyword5 = eaVm.AppletsSavedKeyword5;

            updateRecordsCheck.ExternalInmateId = eaVm.InmateId;
            updateRecordsCheck.ExternalInmate = _context.Inmate.Single(inm => inm.InmateId == eaVm.InmateId);

            updateRecordsCheck.LastAccessBy = _personnelId;
            updateRecordsCheck.LastAccessDate = DateTime.Now;
            updateRecordsCheck.LastAccessByNavigation = updateBy;

            updateRecordsCheck.UpdatedBy = _personnelId;
            updateRecordsCheck.UpdateDate = DateTime.Now;
            updateRecordsCheck.UpdatedByNavigation = updateBy;

            updateRecordsCheck.ExternalAcceptFlag = 1;
            updateRecordsCheck.ExternalAcceptDate = DateTime.Now;
            updateRecordsCheck.ExternalAcceptBy = _personnelId;
            updateRecordsCheck.ExternalAcceptByNavigation = updateBy;

            if (eaVm.ArrestId > 0)
            {
                updateRecordsCheck.ArrestId = eaVm.ArrestId;
                updateRecordsCheck.Arrest = _context.Arrest.Single(arr => arr.ArrestId == eaVm.ArrestId);
            }

            if (eaVm.IncarcerationId > 0)
            {
                updateRecordsCheck.IncarcerationId = eaVm.IncarcerationId;
                updateRecordsCheck.Incarceration =
                    _context.Incarceration.Single(inc => inc.IncarcerationId == eaVm.IncarcerationId);
            }

            return _context.SaveChangesAsync();
        }

        public BookNoteEntryVm LoadInmateBookings(int inmateId)
        {
            List<KeyValuePair<int, string>> lookup = _context.Lookup
                .Where(w => w.LookupType == LookupConstants.ARRTYPE && w.LookupInactive == 0)
                .Select(s => new KeyValuePair<int, string>(s.LookupIndex, s.LookupDescription)).ToList();

            BookNoteEntryVm bookNoteEntryVm = new BookNoteEntryVm
            {
                ListNoteEntryDetails = _context.Arrest
                    .Where(i => i.InmateId == inmateId)
                    .Select(s => new BookingDetails
                    {
                        ArrestBookingNo = s.ArrestBookingNo,
                        ArrestCourtDocket = s.ArrestCourtDocket,
                        ArrestId = s.ArrestId,
                        ArrestType = lookup.SingleOrDefault(a => a.Key == Convert.ToInt32(s.ArrestType)).Value
                    }).ToList(),
                ListNoteType = _commonService.GetLookupKeyValuePairs(LookupConstants.BOOKNOTETYPE)
            };

            return bookNoteEntryVm;
        }

        public async Task<int> SaveBookingNoteDetails(BookingNoteDetailsVm noteDetails)
        {
            ArrestNote arrestNote = new ArrestNote();
            {
                arrestNote.ArrestId = noteDetails.ArrestId;
                arrestNote.NoteType = noteDetails.NoteType;
                arrestNote.NoteText = noteDetails.NoteText;
                arrestNote.NoteDate = DateTime.Now;
                arrestNote.NoteBy = _personnelId;
            }

            _context.Add(arrestNote);

            return await _context.SaveChangesAsync();
        }

        public List<BookingClearVm> GetBookingClearDetails(int incarcerationId)
        {
            IEnumerable<Lookup> lookup = _context.Lookup
                .Where(w => w.LookupType == LookupConstants.ARRTYPE && w.LookupInactive == 0).ToList();

            return _context.IncarcerationArrestXref
                .Where(w => w.IncarcerationId == incarcerationId)
                .Select(s => new BookingClearVm
                {
                    IncarcerationArrestXrefId = s.IncarcerationArrestXrefId,
                    BookingNo = s.Arrest.ArrestBookingNo,
                    CourtDocket = s.Arrest.ArrestCourtDocket,
                    ReleaseDate = s.Arrest.ArrestSentenceReleaseDate,
                    ClearReason = s.ReleaseReason,
                    ClearNotes = s.ReleaseNotes,
                    ClearDate = s.ReleaseDate,
                    Personnel = s.ReleaseOfficerId,
                    ReleaseOut = s.Incarceration.ReleaseOut,
                    IncarcerationId = s.IncarcerationId,
                    ArrestSentenceCode = s.Arrest.ArrestSentenceCode,
                    BailNoBailFlag = s.Arrest.BailNoBailFlag,
                    BailAmount = s.Arrest.BailAmount,
                    ActualDays = s.Arrest.ArrestSentenceActualDaysToServe,
                    ArrestId = s.ArrestId,
                    ArrestType = !string.IsNullOrEmpty(s.Arrest.ArrestType)
                        ? Convert.ToInt32(s.Arrest.ArrestType) : 0,
                    ReleaseOfficer = s.ReleaseOfficerId > 0
                        ? new PersonnelVm
                        {
                            PersonLastName = s.ReleaseOfficer.PersonNavigation.PersonLastName,
                            OfficerBadgeNumber = s.ReleaseOfficer.OfficerBadgeNum,
                            PersonnelId = s.ReleaseOfficer.PersonnelId
                        }
                        : default,
                    BookingType = lookup.Where(l => l.LookupIndex == Convert.ToInt32(s.Arrest.ArrestType))
                        .Select(l => l.LookupDescription).SingleOrDefault(),
                    Sentence = _sentenceService.GetSentenceDetailsIncarceration(incarcerationId)
                }).OrderBy(o => o.BookingNo).ToList();
        }

        public List<HistoryVm> GetClearHistoryDetails(int arrestId)
        {
            List<HistoryVm> listClearHistory = _context.ArrestClearHistory.Where(a => a.ArrestId == arrestId)
                .OrderByDescending(a => a.CreateDate)
                .Select(history => new HistoryVm
                {
                    HistoryId = history.ArrestClearHistoryId,
                    CreateDate = history.CreateDate,
                    PersonId = history.Personnel.PersonId,
                    OfficerBadgeNumber = history.Personnel.OfficerBadgeNum,
                    PersonLastName = history.Personnel.PersonNavigation.PersonLastName,
                    HistoryList = history.ArrestHistoryList
                }).ToList();
            listClearHistory.ForEach(history =>
            {
                if (string.IsNullOrEmpty(history.HistoryList)) return;
                Dictionary<string, string> histories =
                    JsonConvert.DeserializeObject<Dictionary<string, string>>(history.HistoryList);
                history.Header = histories.Select(a => new PersonHeader
                {
                    Header = a.Key,
                    Detail = a.Value
                }).ToList();
            });
            return listClearHistory;
        }

        public BookClearVm GetBookingClearlist(int arrestId)
        {
            BookClearVm bookClearVm = new BookClearVm
            {
                WarningClearList = _context.Arrest.Where(w => w.ArrestId == arrestId)
                    .Select(s => new WarningClearVm
                    {
                        Warning = s.ArrestSentenceNoEarlyRelease.ToString(),
                        Warning1 = s.ArrestSentenceNoLocalParole.ToString(),
                        Warning2 = s.ArrestConditionsOfRelease,
                        RecordOrder = s.ArrestConditionsOfRelease != null ? 1 : 0
                    }).ToList()
            };

            int incarcerationId =
                _context.IncarcerationArrestXref.First(s => s.ArrestId == arrestId).IncarcerationId ?? 0;

            bookClearVm.WarningClearList.AddRange(
                _context.ArrestCondClear.Where(w => w.ArrestId == arrestId && !w.DeleteFlag.HasValue)
                    .Select(s => new WarningClearVm
                    {
                        Warning = s.CondOfClearance,
                        Warning1 = s.CondOfClearanceNote,
                        RecordOrder = 2
                    }));

            List<Lookup> lookup = _context.Lookup
                .Where(w => w.LookupType == LookupConstants.SENTFLAG && w.LookupInactive == 0)
                .ToList();

            bookClearVm.WarningClearList.AddRange(
                _context.ArrestSentFlag.Where(w => w.ArrestId == arrestId && !w.DeleteFlag.HasValue)
                    .Select(s => new WarningClearVm
                    {
                        Warning = lookup.SingleOrDefault(w => w.LookupIndex == s.SentflagLookupIndex)
                            .LookupDescription,
                        RecordOrder = 3
                    }));

            bookClearVm.WarningClearList.AddRange(_context.Incarceration.Where(w =>
                    w.IncarcerationId == incarcerationId && w.OverallFinalReleaseDate.HasValue
                                                         && w.OverallFinalReleaseDate.Value.Date > DateTime.Now.Date)
                .Select(s => new WarningClearVm
                {
                    RecordOrder = 4
                }));

            bookClearVm.Chargeslist = GetBookingCharges(arrestId);
            bookClearVm.ClearBaillist = GetBookingBail(arrestId);
            bookClearVm.OverallWarrant = GetWarrantDetails(arrestId);

            bookClearVm.SentenceViewer = _sentenceService.GetSentenceViewerList(0, incarcerationId);

            return bookClearVm;
        }

        private List<ClearWarrant> GetWarrantDetails(int? arrestId)
        {
            IEnumerable<Lookup> lookup = _context.Lookup
                .Where(w => w.LookupType == LookupConstants.CRIMETYPE || w.LookupType == LookupConstants.CRIMEGROUP ||
                            w.LookupType == LookupConstants.CHARGEQUALIFIER && w.LookupInactive == 0).ToList();

            List<ClearWarrant> warrantlist = _context.Warrant.Where(w => w.ArrestId == arrestId)
                .Select(s => new ClearWarrant
                {
                    WarrantId = s.WarrantId,
                    PersonId = s.PersonId,
                    ArrestId = s.ArrestId,
                    WarrantNumber = s.WarrantNumber,
                    WarrantType = s.WarrantType,
                    WarrantDate = s.WarrantDate,
                    WarrantCountry = s.WarrantCounty,
                    WarrantDescription = s.WarrantDescription,
                    WarrantBailType = s.WarrantBailType,
                    WarrantBailAmount = s.WarrantBailAmount,
                    WarrantChargeType = s.WarrantChargeType,
                    CreatedDate = s.CreateDate
                }).OrderBy(o => o.CreatedDate).ToList();
            
            warrantlist.ForEach(item =>
            {
                List<CrimeGroup> lstCrimeGroup = _context.Crime
                    .Where(w => w.WarrantId == item.WarrantId && w.ArrestId != 0 && w.CrimeDeleteFlag == 0)
                    .Select(s => new CrimeGroup
                    {
                        CrimeNumber = s.CrimeNumber,
                        CrimeId = s.CrimeId,
                        CrimeLookUpId = s.CrimeLookupId,
                        CrimeStatus = lookup.FirstOrDefault(w =>
                                w.LookupType == LookupConstants.CRIMETYPE &&
                                w.LookupIndex == Convert.ToInt32(s.CrimeType))
                            .LookupDescription,
                        Count = s.CrimeCount,
                        DeleteFlag = s.CrimeDeleteFlag,
                        CodeType = s.CrimeLookup.CrimeCodeType,
                        CrimeSection = s.CrimeLookup.CrimeSection,
                        CrimeDescription = s.CrimeLookup.CrimeDescription,
                        StatuteCode = s.CrimeLookup.CrimeStatuteCode,
                        BailAmount = s.BailAmount,
                        BailNoFlag = s.BailNoBailFlag,
                        Status = lookup.FirstOrDefault(w =>
                                w.LookupType == LookupConstants.CRIMETYPE &&
                                w.LookupIndex == Convert.ToInt32(s.CrimeType))
                            .LookupDescription,
                        StatusId = s.CrimeType,
                        CrimeNotes = s.CrimeNotes,
                        CrimeForceId = 0,
                        CrimeGroups = lookup.FirstOrDefault(w =>
                            w.LookupType == LookupConstants.CRIMEGROUP &&
                            (int?)w.LookupIndex == s.CrimeLookup.CrimeGroupId).LookupDescription,
                        Qualifier = lookup.FirstOrDefault(w =>
                            w.LookupType == LookupConstants.CHARGEQUALIFIER &&
                            w.LookupIndex == Convert.ToInt32(s.ChargeQualifierLookup)).LookupDescription,
                        QualifierId = s.ChargeQualifierLookup,
                        CreatedDate = s.CreateDate
                    }).ToList();


                lstCrimeGroup.AddRange(_context.CrimeForce
                    .Where(w => w.WarrantId == item.WarrantId && w.DeleteFlag == 0 &&
                        !w.ForceSupervisorReviewFlag.HasValue)
                    .Select(s => new CrimeGroup
                    {
                        Count = s.TempCrimeCount,
                        DeleteFlag = s.DeleteFlag,
                        CodeType = s.TempCrimeCodeType,
                        CrimeSection = s.TempCrimeSection,
                        CrimeDescription = s.TempCrimeDescription,
                        StatuteCode = s.TempCrimeStatuteCode,
                        BailAmount = s.BailAmount,
                        BailNoFlag = s.BailNoBailFlag,
                        Status = lookup.Where(w => w.LookupType == LookupConstants.CRIMETYPE &&
                            (int?) w.LookupIndex == (string.IsNullOrEmpty(s.TempCrimeType)
                                ? 0
                                : Convert.ToInt32(s.TempCrimeType))).Select(w => w.LookupDescription).FirstOrDefault(),
                        StatusId = s.TempCrimeType,
                        CrimeNotes = s.TempCrimeNotes,
                        CrimeForceId = s.CrimeForceId,
                        CrimeGroups = s.TempCrimeGroup,
                        Qualifier = lookup.Where(w => w.LookupType == LookupConstants.CHARGEQUALIFIER &&
                                (int?) w.LookupIndex ==
                                (string.IsNullOrEmpty(s.ChargeQualifierLookup)
                                    ? 0
                                    : Convert.ToInt32(s.ChargeQualifierLookup)))
                            .Select(w => w.LookupDescription).FirstOrDefault(),
                        QualifierId = s.ChargeQualifierLookup,
                        CreatedDate = s.CreateDate
                    }));
                item.CrimeGroups = lstCrimeGroup;
            });

            return warrantlist;
        }

        private List<ClearChargesVm> GetBookingCharges(int? arrestId)
        {
            IQueryable<Lookup> lookuplst = _context.Lookup
                .Where(w => w.LookupType == LookupConstants.CRIMETYPE ||
                    w.LookupType == LookupConstants.CHARGEQUALIFIER && w.LookupInactive == 0);

            List<ClearChargesVm> chargeslist = _context.Crime
                .Where(w => w.ArrestId == arrestId && w.CrimeDeleteFlag == 0)
                .Select(s => new ClearChargesVm
                {
                    Count = s.CrimeCount,
                    Type = s.CrimeLookup.CrimeCodeType,
                    Qualifier = lookuplst.FirstOrDefault(w =>
                        w.LookupIndex == Convert.ToInt32(s.ChargeQualifierLookup)
                        && w.LookupType == LookupConstants.CHARGEQUALIFIER).LookupDescription,
                    Section = s.CrimeLookup.CrimeSection,
                    Description = s.CrimeLookup.CrimeDescription,
                    Statute = s.CrimeLookup.CrimeStatuteCode,
                    Bail = s.BailAmount,
                    Status = lookuplst.FirstOrDefault(w => w.LookupIndex == Convert.ToInt32(s.CrimeType)
                            && w.LookupType == LookupConstants.CRIMETYPE)
                        .LookupDescription,
                    Note = s.CrimeNotes
                }).ToList();

            chargeslist.AddRange(_context.CrimeForce
                .Where(w => w.ArrestId == arrestId && w.DeleteFlag == 0 && (w.ForceSupervisorReviewFlag == 0
                    || !w.ForceSupervisorReviewFlag.HasValue))
                .Select(s => new ClearChargesVm
                {
                    Count = s.TempCrimeCount,
                    Type = s.TempCrimeCodeType,
                    Section = s.TempCrimeSection,
                    Description = s.TempCrimeDescription,
                    Statute = s.TempCrimeStatuteCode,
                    Bail = s.BailAmount,
                    Status = lookuplst.SingleOrDefault(w => w.LookupIndex == Convert.ToInt32(s.TempCrimeType)
                            && w.LookupType == LookupConstants.CRIMETYPE)
                        .LookupDescription,
                    Note = s.TempCrimeNotes
                }));
            return chargeslist;
        }

        private List<ClearBailVm> GetBookingBail(int? arrestId)
        {
            List<ClearBailVm> clearBaillist = _context.BailTransaction
                .Where(w => w.ArrestId == arrestId)
                .Select(s => new ClearBailVm
                {
                    ReceiptNumber = s.BailReceiptNumber,
                    BailCompanyName = s.BailCompany.BailCompanyName,
                    PostedBy = s.BailPostedBy,
                    AmountPosted = Convert.ToDecimal(s.AmountPosted),
                    PaymentType = s.PaymentTypeLookup,
                    PaymentNumber = s.BailPaymentNumber,
                    CreatedDate = s.CreateDate,
                    CreatedBy = new PersonnelVm
                    {
                        PersonLastName = s.CreateByNavigation.PersonNavigation.PersonLastName,
                        PersonFirstName = s.CreateByNavigation.PersonNavigation.PersonFirstName,
                        OfficerBadgeNumber = s.CreateByNavigation.OfficerBadgeNum
                    },
                    BailTransactionId = s.BailTransactionId,
                    BailAgentId = s.BailAgentId,
                }).ToList();

            clearBaillist.ForEach(item =>
            {
                item.BailPerson = _context.BailAgent.Where(w => w.BailAgentId == item.BailAgentId).Select(s =>
                    new PersonVm
                    {
                        PersonFirstName = s.BailAgentFirstName,
                        PersonLastName = s.BailAgentLastName
                    }).SingleOrDefault();
            });
            return clearBaillist;
        }

        public List<SentenceVm> GetBookingSentence(int incarcerationid)
        {
            IQueryable<Lookup> lookuplst = _context.Lookup
                .Where(w => (w.LookupType == LookupConstants.ARRTYPE || w.LookupType == LookupConstants.BOOKSTAT) &&
                            w.LookupInactive == 0);

            List<SentenceVm> lstBookingsentence1 = _context.IncarcerationArrestXref
                .Where(w => w.IncarcerationId == incarcerationid &&
                    (w.Arrest.ArrestSentenceCode.Value == (int?) SentenceCode.Sentenced
                        || w.Arrest.ArrestSentenceCode.Value == (int?) SentenceCode.AltSent
                        || w.Arrest.ArrestSentenceCode.Value == 3))
                .Select(s => new SentenceVm
                {
                    ArrestBookingNo = s.Arrest.ArrestBookingNo,
                    Type = lookuplst.SingleOrDefault(w => w.LookupIndex == Convert.ToInt32(s.Arrest.ArrestType)
                        && w.LookupType == LookupConstants.ARRTYPE).LookupDescription,
                    ArrestCourtDocket = s.Arrest.ArrestCourtDocket,
                    ArrestSentenceConsecutiveFlag = s.Arrest.ArrestSentenceConsecutiveFlag == 1,
                    ArrestSentenceConsecutiveTo = s.Arrest.ArrestSentenceConsecutiveTo,
                    ArrestBookingNo1 = _context.Arrest.Where(w => w.ArrestId == s.Arrest.ArrestSentenceConsecutiveTo)
                        .Select(i => i.ArrestBookingNo).SingleOrDefault(),
                    ArrestSentenceStartDate = s.Arrest.ArrestSentenceStartDate,
                    ArrestSentenceUseStartDate = s.Arrest.ArrestSentenceUseStartDate,
                    ArrestSentenceDays = s.Arrest.ArrestSentenceDays,
                    MethodName = s.Arrest.ArrestSentenceMethod.MethodName,
                    ArrestSentenceDaysToServe = s.Arrest.ArrestSentenceDaysToServe,
                    ArrestSentenceActualDaysToServe = s.Arrest.ArrestSentenceActualDaysToServe,
                    ArrestSentenceReleaseDate = s.ReleaseDate,
                    ArrestSentenceCode = s.Arrest.ArrestSentenceCode,
                    ArrestSentenceDescription = s.Arrest.ArrestSentenceDescription,
                    DisplayOrder = 1,
                    ArrestId = s.ArrestId,
                    IncarcerationId = s.IncarcerationId,
                    ArrestSentenceGroup = s.Arrest.ArrestSentenceGroup,
                    WeekEnder = s.Arrest.ArrestSentenceWeekender,
                    Abbr = lookuplst
                        .Where(w => w.LookupType == LookupConstants.BOOKSTAT &&
                            (int?)w.LookupIndex == (s.Arrest.ArrestBookingStatus ?? 0))
                        .Select(a => a.LookupName).SingleOrDefault(),
                    ArrestClearScheduleDate = s.Arrest.ArrestSentenceReleaseDate,
                    ReleaseReason = s.ReleaseReason
                }).ToList();

            lstBookingsentence1.AddRange(_context.IncarcerationArrestXref
                .Where(w => w.IncarcerationId == incarcerationid &&
                    w.Arrest.ArrestSentenceCode.Value == (int?) SentenceCode.Hold)
                .Select(s => new SentenceVm
                {
                    ArrestBookingNo = s.Arrest.ArrestBookingNo,
                    Type = lookuplst.SingleOrDefault(w => w.LookupIndex == Convert.ToInt32(s.Arrest.ArrestType)
                        && w.LookupType == LookupConstants.ARRTYPE).LookupDescription,
                    ArrestCourtDocket = s.Arrest.ArrestCourtDocket,
                    ArrestSentenceConsecutiveFlag = s.Arrest.ArrestSentenceConsecutiveFlag == 1,
                    ArrestSentenceConsecutiveTo = s.Arrest.ArrestSentenceConsecutiveTo,
                    ArrestBookingNo1 = _context.Arrest.Where(w => w.ArrestId == s.Arrest.ArrestSentenceConsecutiveTo)
                        .Select(i => i.ArrestBookingNo).SingleOrDefault(),
                    ArrestSentenceStartDate = s.Arrest.ArrestSentenceStartDate,
                    ArrestSentenceDays = s.Arrest.ArrestSentenceDays,
                    MethodName = s.Arrest.ArrestSentenceMethod.MethodName,
                    ArrestSentenceDaysToServe = s.Arrest.ArrestSentenceDaysToServe,
                    ArrestSentenceActualDaysToServe = s.Arrest.ArrestSentenceActualDaysToServe,
                    ArrestSentenceReleaseDate = s.ReleaseDate,
                    ArrestSentenceCode = s.Arrest.ArrestSentenceCode,
                    ArrestSentenceDescription = s.Arrest.ArrestSentenceDescription,
                    DisplayOrder = 2,
                    ArrestId = s.ArrestId,
                    IncarcerationId = s.IncarcerationId,
                    ArrestSentenceGroup = s.Arrest.ArrestSentenceGroup,
                    WeekEnder = s.Arrest.ArrestSentenceWeekender,
                    Abbr = lookuplst.Where(w => w.LookupType == LookupConstants.BOOKSTAT &&
                            (int?)w.LookupIndex == (s.Arrest.ArrestBookingStatus ?? 0))
                        .Select(a => a.LookupName).SingleOrDefault(),
                    ArrestClearScheduleDate = s.Arrest.ArrestSentenceReleaseDate,
                    ReleaseReason = s.ReleaseReason
                }).ToList());

            lstBookingsentence1.AddRange(_context.IncarcerationArrestXref
                .Where(w => w.IncarcerationId == incarcerationid && !w.Arrest.ArrestSentenceCode.HasValue
                    && w.ReleaseDate.HasValue)
                .Select(s => new SentenceVm
                {
                    ArrestBookingNo = s.Arrest.ArrestBookingNo,
                    Type = lookuplst.SingleOrDefault(w => w.LookupIndex == Convert.ToInt32(s.Arrest.ArrestType)
                        && w.LookupType == LookupConstants.ARRTYPE).LookupDescription,
                    ArrestCourtDocket = s.Arrest.ArrestCourtDocket,
                    ArrestSentenceConsecutiveFlag = s.Arrest.ArrestSentenceConsecutiveFlag == 1,
                    ArrestSentenceConsecutiveTo = s.Arrest.ArrestSentenceConsecutiveTo,
                    ArrestBookingNo1 = _context.Arrest.Where(w => w.ArrestId == s.Arrest.ArrestSentenceConsecutiveTo)
                        .Select(i => i.ArrestBookingNo).SingleOrDefault(),
                    ArrestSentenceStartDate = s.Arrest.ArrestSentenceStartDate,
                    ArrestSentenceDays = s.Arrest.ArrestSentenceDays,
                    MethodName = s.Arrest.ArrestSentenceMethod.MethodName,
                    ArrestSentenceDaysToServe = s.Arrest.ArrestSentenceDaysToServe,
                    ArrestSentenceActualDaysToServe = s.Arrest.ArrestSentenceActualDaysToServe,
                    ArrestSentenceReleaseDate = s.ReleaseDate,
                    ArrestSentenceCode = s.Arrest.ArrestSentenceCode,
                    ArrestSentenceDescription = s.Arrest.ArrestSentenceDescription,
                    DisplayOrder = 3,
                    ArrestId = s.ArrestId,
                    IncarcerationId = s.IncarcerationId,
                    ArrestSentenceGroup = s.Arrest.ArrestSentenceGroup,
                    WeekEnder = s.Arrest.ArrestSentenceWeekender,
                    Abbr = lookuplst.Where(w => w.LookupType == LookupConstants.BOOKSTAT &&
                            (int?) w.LookupIndex == (s.Arrest.ArrestBookingStatus ?? 0))
                        .Select(a => a.LookupName).SingleOrDefault(),
                    ArrestClearScheduleDate = s.Arrest.ArrestSentenceReleaseDate,
                    ReleaseReason = s.ReleaseReason
                }).ToList());

            lstBookingsentence1.AddRange(_context.IncarcerationArrestXref
                .Where(w => w.IncarcerationId == incarcerationid && !w.Arrest.ArrestSentenceCode.HasValue
                    && !w.ReleaseDate.HasValue)
                .Select(s => new SentenceVm
                {
                    ArrestBookingNo = s.Arrest.ArrestBookingNo,
                    Type = lookuplst.SingleOrDefault(w => w.LookupIndex == Convert.ToInt32(s.Arrest.ArrestType)
                        && w.LookupType == LookupConstants.ARRTYPE).LookupDescription,
                    ArrestCourtDocket = s.Arrest.ArrestCourtDocket,
                    ArrestSentenceConsecutiveFlag = s.Arrest.ArrestSentenceConsecutiveFlag == 1,
                    ArrestSentenceConsecutiveTo = s.Arrest.ArrestSentenceConsecutiveTo,
                    ArrestBookingNo1 = _context.Arrest.Where(w => w.ArrestId == s.Arrest.ArrestSentenceConsecutiveTo)
                        .Select(i => i.ArrestBookingNo).SingleOrDefault(),
                    ArrestSentenceStartDate = s.Arrest.ArrestSentenceStartDate,
                    ArrestSentenceDays = s.Arrest.ArrestSentenceDays,
                    MethodName = s.Arrest.ArrestSentenceMethod.MethodName,
                    ArrestSentenceDaysToServe = s.Arrest.ArrestSentenceDaysToServe,
                    ArrestSentenceActualDaysToServe = s.Arrest.ArrestSentenceActualDaysToServe,
                    ArrestSentenceReleaseDate = s.ReleaseDate,
                    ArrestSentenceCode = s.Arrest.ArrestSentenceCode,
                    ArrestSentenceDescription = s.Arrest.ArrestSentenceDescription,
                    DisplayOrder = 4,
                    ArrestId = s.ArrestId,
                    IncarcerationId = s.IncarcerationId,
                    ArrestSentenceGroup = s.Arrest.ArrestSentenceGroup,
                    WeekEnder = s.Arrest.ArrestSentenceWeekender,
                    Abbr = lookuplst.Where(w => w.LookupType == LookupConstants.BOOKSTAT &&
                            (int?)w.LookupIndex == (s.Arrest.ArrestBookingStatus ?? 0))
                        .Select(a => a.LookupName).SingleOrDefault(),
                    ArrestClearScheduleDate = s.Arrest.ArrestSentenceReleaseDate,
                    ReleaseReason = s.ReleaseReason
                }).ToList());

            return lstBookingsentence1.OrderBy(s => s.ArrestId)
                .ThenBy(s => s.IncarcerationId)
                .ThenBy(s => s.DisplayOrder)
                .ThenBy(s => s.ArrestSentenceGroup)
                .ThenBy(s => s.ArrestSentenceConsecutiveFlag).ToList();
        }

        public async Task<int> UpdateClear(BookingClearVm bookingClear)
        {
            IncarcerationArrestXref arrestNote = _context.IncarcerationArrestXref.Single(
                s => s.IncarcerationArrestXrefId == bookingClear.IncarcerationArrestXrefId);
            {
                arrestNote.ReleaseNotes = bookingClear.ClearNotes;
                arrestNote.ReleaseReason = bookingClear.ClearReason;
                arrestNote.ReleaseDate = DateTime.Now;
                arrestNote.ReleaseOfficerId = _personnelId;
                arrestNote.ReleaseSupervisorCompleteFlag = null;
            }
            _context.SaveChanges();
            
             _interfaceEngine.Export(new ExportRequestVm
            {
                EventName = EventNameConstants.CASECLEAR,
                PersonnelId = _personnelId,
                Param1 = bookingClear.PersonId.ToString(),
                Param2 = bookingClear.IncarcerationArrestXrefId.ToString()
            });

            Arrest arrest = _context.Arrest.Single(s => s.ArrestId == bookingClear.ArrestId);
            {
                arrest.UpdateDate = DateTime.Now;
                arrest.ArrestReleaseClearedDate = DateTime.Now;
                arrest.ArrestReleaseClearedBy = _personnelId;
                arrest.ArrestActive = null;
            }

            Incarceration incarceration =
                _context.Incarceration.Single(s => s.IncarcerationId == bookingClear.IncarcerationId);

            int arrestCnt = _context.IncarcerationArrestXref
                .Count(a => a.IncarcerationId == incarceration.IncarcerationId && !a.ReleaseDate.HasValue);

            if (arrestCnt > 0)
            {
                incarceration.ReleaseClearFlag = null;
                incarceration.ReleaseClearBy = null;
                incarceration.ReleaseClearDate = null;
                incarceration.ReleaseWizardLastStepId = null;
                incarceration.ReleaseCompleteFlag = null;
                incarceration.ReleaseSupervisorCompleteFlag = null;
                incarceration.ReleaseSupervisorWizardLastStepId = null;
            }
            else
            {
                incarceration.ReleaseClearFlag = 1;
                incarceration.ReleaseClearBy = _personnelId;
                incarceration.ReleaseClearDate = DateTime.Now;

                _inmateService.CreateTask(incarceration.InmateId ?? 0, TaskValidateType.ClearedForRelease);
            }

            InsertArrestClearHistory(bookingClear.ArrestId ?? 0, bookingClear.ArrestHistoryList);
            _bookingReleaseService.CalculateBailTotalAmount(bookingClear.ArrestId ?? 0,
                bookingClear.PersonId, false, true);
           
            return await _context.SaveChangesAsync();
        }

        public OverallSentvm CheckOverallSentDetails(int arrestId, int incarcerationId)
        {
            OverallSentvm overallsent = new OverallSentvm
            {
                Overall = _context.IncarcerationArrestXref
                    .Where(w => w.ArrestId == arrestId && !w.Arrest.ArrestSentenceCode.HasValue)
                    .Select(s => new Overall
                    {
                        ArrestBookingNo = s.Arrest.ArrestBookingNo,
                        ReleaseDate = s.Incarceration.OverallFinalReleaseDate,
                        TotalSentDays = s.Incarceration.TotSentDays,
                        SentCount = _context.IncarcerationArrestXref.Count(
                            w => w.Arrest.ArrestSentenceCode == 1 && w.IncarcerationId == incarcerationId),
                        UnSentCount = _context.IncarcerationArrestXref.Count(
                            w => w.IncarcerationId == incarcerationId &&
                                 !w.Arrest.ArrestSentenceCode.HasValue && !w.ReleaseDate.HasValue)
                    }).SingleOrDefault(),

                BookingCountDetails = _context.IncarcerationArrestXref.Where(w =>
                        !w.Incarceration.ReleaseOut.HasValue && w.IncarcerationId == incarcerationId)
                    .Select(s => new BookingCountDetails
                    {
                        IncarcerationId = s.Incarceration.IncarcerationId,
                        SentenceCode = s.Arrest.ArrestSentenceCode,
                        ReleaseDate = s.ReleaseDate,
                        SentenceWeekender = s.Arrest.ArrestSentenceWeekender,
                        SentenceIndefiniteHold = s.Arrest.ArrestSentenceIndefiniteHold,
                        SentenceReleaseDate = s.Arrest.ArrestSentenceReleaseDate,
                        SentenceClearDate = s.Arrest.ArrestReleaseClearedDate,
                        ActiveBookingHold = _context.Lookup.SingleOrDefault(w =>
                            w.LookupIndex == Convert.ToInt32(s.Arrest.ArrestType)
                            && w.LookupDescription.Contains(LookupConstants.HOLD) &&
                            w.LookupField == LookupConstants.ARRESTTYPE && !s.ReleaseDate.HasValue &&
                            w.LookupInactive == 0).LookupDescription
                    }).ToList()
            };
            return overallsent;
        }

        private void InsertArrestClearHistory(int arrestId, string historyList)
        {
            ArrestClearHistory arrestClearHistory = new ArrestClearHistory();
            {
                arrestClearHistory.ArrestId = arrestId;
                arrestClearHistory.PersonnelId = _personnelId;
                arrestClearHistory.CreateDate = DateTime.Now;
                arrestClearHistory.ArrestHistoryList = historyList;
            }
            _context.Add(arrestClearHistory);
        }

        public List<ClearChargesVm> GetSentenceCharges(int[] arrestId)
        {
            _lookupList = _commonService.GetLookups(new[]
                {LookupConstants.CRIMETYPE, LookupConstants.CHARGEQUALIFIER});
            return _context.Crime
                .Where(w => arrestId.Contains(w.ArrestId ?? 0) && w.CrimeDeleteFlag == 0)
                .Select(s => new ClearChargesVm
                {
                    ArrestId = s.ArrestId,
                    Count = s.CrimeCount,
                    Type = s.CrimeLookup.CrimeCodeType,
                    Qualifier = _lookupList.SingleOrDefault(w =>
                        w.LookupIndex == Convert.ToInt32(s.ChargeQualifierLookup)
                        && w.LookupType.Contains(LookupConstants.CHARGEQUALIFIER)).LookupDescription,
                    Section = s.CrimeLookup.CrimeSection,
                    Description = s.CrimeLookup.CrimeDescription,
                    Statute = s.CrimeLookup.CrimeStatuteCode,
                    Bail = s.BailAmount,
                    Status = _lookupList.SingleOrDefault(w => w.LookupIndex == Convert.ToInt32(s.CrimeType)
                            && w.LookupType.Contains(LookupConstants.CRIMETYPE))
                        .LookupDescription,
                    Note = s.CrimeNotes,
                    WarrantNumber = s.WarrantId > 0 ? s.Warrant.WarrantNumber : string.Empty,
                    CrimeNumber = s.CrimeNumber,
                    StartDate = s.Arrest.ArrestSentenceStartDate,
                    ArrestSentenceConsecutiveFlag = s.Arrest.ArrestSentenceConsecutiveFlag,
                    UseStartDate = s.Arrest.ArrestSentenceUseStartDate,
                    ArrestSentenceDays = s.Arrest.ArrestSentenceDays,
                    Method = s.Arrest.ArrestSentenceMethod.MethodName,
                    ArrestSentenceDaysToServe = s.Arrest.ArrestSentenceDaysToServe,
                    ArrestSentenceActualDaysToServe = s.Arrest.ArrestSentenceActualDaysToServe,
                    ClearDate = s.ArrestSentenceReleaseDate
                }).ToList();
        }

        public CurrentStatus GetCurrentStatus(int incarcerationId)
        {
            return _context.Incarceration.Where(a => a.IncarcerationId == incarcerationId).Select(a => new CurrentStatus
            {
                OverAllSentStartDate = a.OverallSentStartDate,
                OverAllSentReleaseDate = a.OverallFinalReleaseDate,
                ActualDaysToServe = a.ActualDaysToServe,
                OverAllSentManual = a.OverallSentManual,
                ManualOverride = a.OverallSentManual,
                OverAllSentErc = a.OverallsentErc,
                OverAllSentErcClear = a.OverallsentErcclear
            }).Single();
        }

        public OercMethod GetOercDetails()
        {
            return _context.ArrestSentenceMethodOerc.Where(a => a.ArrestSentenceMethodOercid == 1).Select(a =>
                new OercMethod
                {
                    ArrestSentenceMethodOERCid = a.ArrestSentenceMethodOercid,
                    OERCVisible = a.Oercvisible,
                    OERCCredit = a.Oerccredit,
                    OERCDaysRange = a.OercdaysRange,
                    OERCDaysRangeUseMaxDTS = a.OercdaysRangeUseMaxDts,
                    OERCDaysRangeUseMaxD = a.OercdaysRangeUseMaxD
                }).SingleOrDefault();
        }

        public async Task<int> UpdateOverAllSentence(OverallSentence overallSentence)
        {
            Incarceration incar = _context.Incarceration.Single(
                s => s.IncarcerationId == overallSentence.IncarcerationId);
            {
                incar.OverallSentManual = overallSentence.SentManual;
                incar.OverallSentManualDate = overallSentence.SentManualDate;
                incar.OverallSentManualBy = overallSentence.SentManualby;
                incar.OverallSentStartDate = overallSentence.SentStartDate;
                incar.OverallFinalReleaseDate = overallSentence.FinalReleaseDate;
                incar.ActualDaysToServe = overallSentence.DaysToServe;
                incar.OverallsentErc = overallSentence.SentERC;
                incar.OverallsentErcclear = overallSentence.SentERCClear;
                incar.TotSentDays = overallSentence.TotSentDays;
            }
            IncarcerationSentSaveHistory incarcerationSentSaveHistory = new IncarcerationSentSaveHistory();
            {
                incarcerationSentSaveHistory.IncarcerationId = overallSentence.IncarcerationId;
                incarcerationSentSaveHistory.OverallFinalReleaseDate = overallSentence.FinalReleaseDate;
                incarcerationSentSaveHistory.OverallSentStartDate = overallSentence.SentStartDate;
                incarcerationSentSaveHistory.TotSentDays = overallSentence.TotSentDays;
                incarcerationSentSaveHistory.OverallSentManual = overallSentence.SentManual;
                incarcerationSentSaveHistory.SaveDate = DateTime.Now;
                incarcerationSentSaveHistory.SaveBy = _personnelId;
                incarcerationSentSaveHistory.OverallsentErc = overallSentence.SentERC;
                incarcerationSentSaveHistory.OverallsentErcclear = overallSentence.SentERCClear;
            }
            _context.Add(incarcerationSentSaveHistory);
            int programSentCode = _context.AltSent.FirstOrDefault(a =>
                                          a.IncarcerationId == overallSentence.IncarcerationId
                                          && (a.AltSentClearFlag == 0 || !a.AltSentClearFlag.HasValue))?.AltSentProgram
                                      .AltSentProgramSentCode ?? 0;
            if (_context.Facility.Single(a => a.FacilityId == overallSentence.FacilityId).AltSentFlag > 0 &&
                programSentCode == 1)
            {
                AltSent altSent = _context.AltSent.Single(a => a.AltSentId == overallSentence.AltSentId);
                altSent.AltSentProjectedRelease = overallSentence.FinalReleaseDate;
            }

            _interfaceEngine.Export(new ExportRequestVm
            {
                EventName = overallSentence.EventName,
                PersonnelId = _personnelId,
                Param1 = overallSentence.PersonId.ToString(),
                Param2 = overallSentence.IncarcerationId.ToString()
            });
            return await _context.SaveChangesAsync();
        }

        public List<string> GetCautionflag(int personId)
        {
            IQueryable<Lookup> lookuplst = _context.Lookup.Where(w => w.LookupType == LookupConstants.PERSONCAUTION
                  || w.LookupType == LookupConstants.TRANSCAUTION ||
                  w.LookupType == LookupConstants.DIET && w.LookupInactive == 0);

            IQueryable<PersonFlag> personFlag = _context.PersonFlag.Where(w =>
                w.PersonId == personId && w.DeleteFlag == 0
                && (w.InmateFlagIndex > 0 || w.PersonFlagIndex > 0 || w.DietFlagIndex > 0));

            List<string> flagAlert = personFlag.SelectMany(
                p => lookuplst.Where(w => w.LookupType == LookupConstants.PERSONCAUTION
                    && (int?) w.LookupIndex == p.PersonFlagIndex
                    && p.PersonFlagIndex > 0), (p, l) => l.LookupDescription).ToList();

            flagAlert.AddRange(personFlag.SelectMany(
                p => lookuplst.Where(w => w.LookupType == LookupConstants.TRANSCAUTION
                    && (int?) w.LookupIndex == p.InmateFlagIndex
                    && p.InmateFlagIndex > 0), (p, l) => l.LookupDescription).ToList());

            flagAlert.AddRange(personFlag.SelectMany(
                p => lookuplst.Where(w => w.LookupType == LookupConstants.DIET
                    && (int?) w.LookupIndex == p.DietFlagIndex
                    && p.DietFlagIndex > 0), (p, l) => l.LookupDescription).ToList());

            return flagAlert;
        }

        public async Task<int> UndoClearBook(UndoClearBook undoClearBook)
        {
            // Update Arrest Table
            Arrest arrest = _context.Arrest.Single(a => a.ArrestId == undoClearBook.ArrestId);
            arrest.UpdateDate = null;
            arrest.ArrestReleaseClearedBy = null;
            arrest.ArrestReleaseClearedDate = null;
            arrest.ArrestActive = 1;
            // Update IncarcerationArrestXref Table
            IncarcerationArrestXref incarcerationArrestXref = _context.IncarcerationArrestXref
                .Single(a => a.IncarcerationArrestXrefId == undoClearBook.IncarcerationArrestXrefId);
            incarcerationArrestXref.ReleaseNotes = null;
            incarcerationArrestXref.ReleaseReason = null;
            incarcerationArrestXref.ReleaseDate = null;
            incarcerationArrestXref.ReleaseOfficerId = null;
            incarcerationArrestXref.ReleaseSupervisorCompleteFlag = null;
            incarcerationArrestXref.ReleaseSupervisorWizardLastStepId = null;
            _context.SaveChanges();
            // Inset history details into ArrestClearHistory Table
            InsertArrestClearHistory(undoClearBook.ArrestId, undoClearBook.HistoryList);
            // Calculate Bail Amount
            _bookingReleaseService.CalculateBailTotalAmount(undoClearBook.ArrestId,
                undoClearBook.PersonId, false, true);
            // Set Release Clear Flag
            Incarceration incarceration =
                _context.Incarceration.Single(a => a.IncarcerationId == undoClearBook.IncarcerationId);
            if (_context.IncarcerationArrestXref.Any(a => a.IncarcerationId == undoClearBook.IncarcerationId
                                                          && !a.ReleaseDate.HasValue))
            {
                incarceration.ReleaseClearFlag = null;
                incarceration.ReleaseClearBy = null;
                incarceration.ReleaseClearDate = null;
                incarceration.ReleaseWizardLastStepId = null;
                incarceration.ReleaseCompleteFlag = null;
                incarceration.ReleaseSupervisorCompleteFlag = null;
                incarceration.ReleaseSupervisorWizardLastStepId = null;
            }
            else
            {
                incarceration.ReleaseClearFlag = 1;
                incarceration.ReleaseClearBy = _personnelId;
                incarceration.ReleaseClearDate = DateTime.Now;
            }

            return await _context.SaveChangesAsync();
        }

        public IEnumerable<BookingActive> GetActiveBooking(int facilityId, BookingActiveStatus status)
        {
            List<BookingActive> bookingActive = _context.Incarceration
                .Where(x => x.Inmate.FacilityId == facilityId
                    && x.Inmate.PersonId > 0
                    && x.ReleaseOut == null
                    && x.Inmate.InmateActive == 1
                    && (status != BookingActiveStatus.BookingOnly ||
                        x.IntakeCompleteFlag == 1 && x.BookCompleteFlag != 1 || x.BookAndReleaseFlag == 1)
                    && (status != BookingActiveStatus.IntakeOnly || x.IntakeCompleteFlag != 1)
                    && (status != BookingActiveStatus.NoClassify ||
                        x.BookAndReleaseFlag != 1 && x.Inmate.InmateClassificationId == null)
                    && (status != BookingActiveStatus.NoHousing || x.Inmate.HousingUnitId == null)
                    && (status != BookingActiveStatus.NoSent || x.OverallSentStartDate == null)
                    && (status != BookingActiveStatus.SentOnly || x.OverallSentStartDate != null)).Select(inc =>
                    new BookingActive
                    {
                        InmateId = inc.InmateId ?? 0,
                        IntakeFlag = inc.IntakeCompleteFlag ?? 0,
                        BookCompleteFlag = inc.BookCompleteFlag ?? 0,
                        IncarcerationDate = inc.DateIn,
                        OverallFinalRelDate = inc.OverallFinalReleaseDate,
                        ActualDaystoServe = inc.ActualDaysToServe,
                        SentFlag = inc.OverallSentStartDate.HasValue ? 1 : 0,
                        BookingNumber = inc.BookingNo
                    }).ToList();

            List<InmateHousing> lstPersonDetails = GetInmateDetails(bookingActive.Select(i => i.InmateId).ToList());

            bookingActive.ForEach(inc =>
            {
                InmateHousing inmate = lstPersonDetails.Single(ii => ii.InmateId == inc.InmateId);
                inc.InmateActive = inmate.InmateActive ? 1 : 0;
                inc.FacilityId = inmate.FacilityId;
                inc.InmateNumber = inmate.InmateNumber;
                inc.DOB = inmate.PersonDob;
                inc.PhotoFilePath = inmate.PhotoFilePath;
                inc.HouseFlag = inmate.HousingUnitId.HasValue ? 1 : 0;
                inc.TrackNotesLocation = inmate.InmateCurrentTrack;
                inc.InmateCurrentTrack = inmate.InmateCurrentTrack;
                inc.CurrentTrackLocation = inmate.InmateCurrentTrack;
                inc.ClassFlag = inmate.InmateClassificationId.HasValue ? 1 : 0;
                inc.Classify = inmate.InmateClassificationReason;
                inc.PersonDetails = new PersonInfoVm
                {
                    PersonId = inmate.PersonId,
                    PersonFirstName = inmate.PersonFirstName,
                    PersonLastName = inmate.PersonLastName,
                    PersonMiddleName = inmate.PersonMiddleName
                };

                inc.HousingUnit = new HousingUnitVm
                {
                    HousingUnitLocation = inmate.HousingLocation,
                    HousingUnitNumber = inmate.HousingNumber,
                    HousingUnitBedLocation = inmate.HousingBedLocation,
                    HousingUnitBedNumber = inmate.HousingBedNumber
                };
            });

            return bookingActive.OrderBy(x => x.PersonDetails.PersonLastName);
        }

        List<KeyValuePair<int?, decimal?>> IBookingService.GetTotalBailAmountandNoBailFlag(int incarcerationId) =>
            _context.IncarcerationArrestXref.Where(w => 
                w.Arrest.ArrestActive == 1 && w.Incarceration.IncarcerationId == incarcerationId)
                .Select(s => new KeyValuePair<int?, decimal?>(s.Arrest.BailNoBailFlag, s.Arrest.BailAmount))
                .ToList();

        public BookingPrebook GetBookingPrebookForms(int incarcerationId)
        {
            InmatePrebook inmPrebook = _context.InmatePrebook.First(inm => inm.IncarcerationId == incarcerationId);

            AttachmentSearch attSearch = new AttachmentSearch {InmateprebookId = inmPrebook.InmatePrebookId};

            BookingPrebook bookPreBook = new BookingPrebook
            {
                FormTemplates = _context.FormRecord.Where(fr =>
                        fr.InmatePrebookId == inmPrebook.InmatePrebookId && fr.FormTemplates.FormCategoryId == 1)
                    .Select(fr => new LoadSavedForms
                    {
                        Notes = fr.FormNotes,
                        FormRecordId = fr.FormRecordId,
                        CategoryId = fr.FormTemplates.FormCategoryId,
                        DeleteFlag = fr.DeleteFlag,
                        XmlStr = HttpUtility.HtmlDecode(fr.XmlData),
                        FormInterfaceSent = fr.FormInterfaceSent,
                        InterfaceBypassed = fr.FormInterfaceBypassed,
                        Date = fr.CreateDate,
                        DisplayName = fr.FormTemplates.DisplayName,
                        FormTemplatesId = fr.FormTemplatesId,
                        HtmlFileName = fr.FormTemplates.HtmlFileName,
                        FormCategoryFolderName = fr.FormTemplates.FormCategory.FormCategoryFolderName
                    }).ToList(),
                PrebookNumber = inmPrebook.PreBookNumber,
                InmatePrebook = _preBookActiveService.GetPrebook(inmPrebook.InmatePrebookId),
                PrebookCharges = _preBookWizardService.GetPrebookCharges(inmPrebook.InmatePrebookId, true, 0).ToList(),
                PrebookWarrant = _preBookWizardService.GetPrebookWarrant(inmPrebook.InmatePrebookId, true).ToList(),
                PrebookProperty = _preBookWizardService
                    .GetPersonalInventoryPrebook(inmPrebook.InmatePrebookId, true).ToList(),
                PrebookAttachment = _preBookWizardService.GetPrebookAttachment(attSearch).ToList()
            };
            return bookPreBook;
        }

        public IncarcerationFormListVm LoadFormDetails(int incarcerationId, int incarcerationArrestId, int arrestId, int formTemplateId)
        {
            IncarcerationFormListVm incarcerationFormDetails = new IncarcerationFormListVm();

            IncarcerationArrestXref incarcerationArrestXref = _context.IncarcerationArrestXref.SingleOrDefault(x =>
                    x.IncarcerationArrestXrefId == incarcerationArrestId &&
                    x.IncarcerationId == incarcerationId);
            // 10 - Booking forms category id
            IQueryable<FormRecord> lstRecordsDetails = _context.FormRecord.Where(f => f.FormHousingClear == 0 &&
               f.Incarceration.IncarcerationId == incarcerationId
               && f.FormTemplates.FormCategory.FormCategoryId == 10
               && (f.FormTemplates.Inactive ?? 0) == 0);

            if (formTemplateId > 0)
            {
                lstRecordsDetails = lstRecordsDetails.Where(f => f.FormTemplatesId == formTemplateId);
            }

            IQueryable<PersonnelVm> lstPersonnel = _context.Personnel.Select(s => new PersonnelVm
            {
                PersonLastName = s.PersonNavigation.PersonLastName,
                PersonnelNumber = s.PersonnelNumber,
                PersonnelId = s.PersonnelId
            });

            //Incarceration Form Details
            incarcerationFormDetails.IncarcerationForms = lstRecordsDetails
                .Select(s => new IncarcerationForms
                {
                    FormRecordId = s.FormRecordId,
                    DisplayName = s.FormTemplates.DisplayName,
                    FormNotes = s.FormNotes,
                    ReleaseOut = s.Incarceration.ReleaseOut,
                    DateIn = s.Incarceration.DateIn,
                    DeleteFlag = s.DeleteFlag,
                    XmlData = HttpUtility.HtmlDecode(s.XmlData),
                    FormCategoryFolderName = s.FormTemplates.FormCategory.FormCategoryFolderName,
                    HtmlFileName = s.FormTemplates.HtmlFileName,
                    FormTemplatesId = s.FormTemplatesId,
                    FormInterfaceFlag = s.FormTemplates.FormInterfaceFlag,
                    FormInterfaceSent = s.FormInterfaceSent,
                    FormInterfaceByPassed = s.FormInterfaceBypassed,
                    CreateDate = s.CreateDate,
                    UpdateDate = s.UpdateDate,
                    FormCategoryFilterId = s.FormTemplates.FormCategoryFilterId ?? 0,                                    
                    FilterName = s.FormTemplates.FormCategoryFilter.FilterName ?? "NO FILTER",
                    InmateNumber = s.Incarceration.Inmate.InmateNumber,
                    CreatedBy = lstPersonnel.SingleOrDefault(w => w.PersonnelId == s.CreateBy),
                    UpdatedBy = lstPersonnel.SingleOrDefault(w => w.PersonnelId == s.UpdateBy),
                    NoSignature = s.NoSignatureReason,
                    NoSignatureFlag=s.NoSignatureFlag
                    
                }).ToList();
            if (incarcerationArrestXref != null)
            {
                incarcerationFormDetails.BookingForms = _context.FormRecord
                    .Where(f => f.ArrestId == incarcerationArrestXref.ArrestId
                                && f.FormTemplates.FormCategory.FormCategoryId == 11).Select(s => new IncarcerationForms
                                {
                                    FormRecordId = s.FormRecordId,
                                    DisplayName = s.FormTemplates.DisplayName,
                                    FormNotes = s.FormNotes,
                                    ReleaseOut = s.Incarceration.ReleaseOut,
                                    DateIn = s.Incarceration.DateIn,
                                    DeleteFlag = s.DeleteFlag,
                                    XmlData = HttpUtility.HtmlDecode(s.XmlData),
                                    FormCategoryFolderName = s.FormTemplates.FormCategory.FormCategoryFolderName,
                                    HtmlFileName = s.FormTemplates.HtmlFileName,
                                    FormTemplatesId = s.FormTemplatesId,
                                    FormInterfaceFlag = s.FormTemplates.FormInterfaceFlag,
                                    FormInterfaceSent = s.FormInterfaceSent,
                                    FormInterfaceByPassed = s.FormInterfaceBypassed,
                                    CreateDate = s.CreateDate,
                                    UpdateDate = s.UpdateDate,
                                    FormCategoryFilterId = s.FormTemplates.FormCategoryFilterId,
                                    FilterName = s.FormTemplates.FormCategoryFilter.FilterName,
                                    InmateNumber = s.Incarceration.Inmate.InmateNumber,
                                    CreatedBy = lstPersonnel.SingleOrDefault(w => w.PersonnelId == s.CreateBy),
                                    UpdatedBy = lstPersonnel.SingleOrDefault(w => w.PersonnelId == s.UpdateBy),
                                    NoSignature = s.NoSignatureReason
                                }).ToList();
            }

            List<FormTemplateCount> formTemplateCounts = new List<FormTemplateCount>
            {
                //All filter grid list           
                new FormTemplateCount
                {
                    CategoryName = CommonConstants.ALL.ToString(),
                    LstFormTemplate = lstRecordsDetails.Select(s => s.FormTemplatesId).ToList(),
                    Count = lstRecordsDetails.Count()
                },

                //No filters grid list            
                new FormTemplateCount
                {
                    CategoryName = ConsoleFormConstants.NOFILTER,
                    LstFormTemplate = lstRecordsDetails.Select(s => s.FormTemplatesId).ToList(),
                    Count = lstRecordsDetails.Count(f => f.FormTemplates.FormCategoryFilterId == 0
                        || !f.FormTemplates.FormCategoryFilterId.HasValue)
                }
            };

            //Another filters grid list
            // 10 - Booking forms category id
            formTemplateCounts.AddRange(_context.FormTemplates
                .Where(w => w.FormCategoryFilterId.HasValue && w.Inactive==0 && w.FormCategoryId==10)
                .Select(s => new FormTemplateCount
                {
                    CategoryId = s.FormCategoryFilter.FormCategoryFilterId,
                    CategoryName = s.FormCategoryFilter.FilterName,
                    LstFormTemplate = lstRecordsDetails.Select(t => t.FormTemplatesId).ToList(),
                    Count = lstRecordsDetails.Count(i => i.FormTemplates.FormCategoryFilterId == s.FormCategoryFilterId)
                }).Distinct().ToList());

            //incarcerationFormDetails.FormTemplateCountList = formTemplateCounts;
            incarcerationFormDetails.FormTemplateCountList  = formTemplateCounts.GroupBy(i => i.CategoryName)
            .Select(g => g.First()).ToList();
            return incarcerationFormDetails;
        }

        public List<IncarcerationDetail> GetIncarcerationAndBookings(int inmateId, bool toBindCharge = false,
            bool isActiveIncarceration = false, DateTime? dateIn = null, DateTime? releaseOut = null)
        {
            List<IncarcerationDetail> lstIncarcerations = new List<IncarcerationDetail>();
            if (inmateId == 0) return lstIncarcerations;

            //Get all incarcerations ids
            int[] incarcerationIds = _context.Incarceration
                .OrderByDescending(inc => inc.IncarcerationId)
                .Where(inc => inc.InmateId == inmateId
                              && (!isActiveIncarceration || !inc.ReleaseOut.HasValue)
                              && (!dateIn.HasValue || !releaseOut.HasValue || inc.DateIn.Value <= dateIn.Value
                                  && (!inc.ReleaseOut.HasValue || inc.ReleaseOut.Value >= releaseOut.Value)))
                .Select(inc => inc.IncarcerationId).ToArray();

            //Get all arrest id's for all incarceration id's
            List<KeyValuePair<int?, int?>> allArrestIds = (from arrXref in _context.IncarcerationArrestXref
                   where incarcerationIds.Contains(arrXref.IncarcerationId ?? 0) && !arrXref.ReleaseDate.HasValue
                   select new KeyValuePair<int?, int?>(arrXref.ArrestId, arrXref.IncarcerationId)).ToList();
            int?[] arrestId = allArrestIds.Select(id => id.Key).ToArray();
            //Get all arrest records for all arrest id's
            List<Arrest> lstAllArrest = (from arr in _context.Arrest
                                         where arrestId.Contains(arr.ArrestId)
                                         select new Arrest
                                         {
                                             ArrestId = arr.ArrestId,
                                             ArrestSentenceCode = arr.ArrestSentenceCode,
                                             ArrestSentenceIndefiniteHold = arr.ArrestSentenceIndefiniteHold,
                                             ArrestSentenceReleaseDate = arr.ArrestSentenceReleaseDate
                                         }).ToList();


            //Get all clear count for all incarceration id's
            int?[] lstClearCount = _context.IncarcerationArrestXref
                .Where(iax => incarcerationIds.Contains(iax.IncarcerationId ?? 0) && iax.ReleaseDate.HasValue)
                .Select(iax => iax.IncarcerationId).ToArray();

            Person person = _context.Inmate
                .Where(i => i.InmateId == inmateId)
                .Select(p => new Person
                {
                    PersonLastName = p.Person.PersonLastName,
                    PersonFirstName = p.Person.PersonFirstName,
                    PersonMiddleName = p.Person.PersonMiddleName,
                    PersonDob = p.Person.PersonDob
                }).Single();

            //Incarceration
            List<Incarceration> lstAllIncarceration = (from inc in _context.Incarceration
                where incarcerationIds.Contains(inc.IncarcerationId)
                select new Incarceration
                {
                    IncarcerationId = inc.IncarcerationId,
                    OverallSentStartDate = inc.OverallSentStartDate,
                    OverallFinalReleaseDate = inc.OverallFinalReleaseDate,
                    ActualDaysToServe = inc.ActualDaysToServe,
                    DateIn = inc.DateIn,
                    ReleaseOut = inc.ReleaseOut,
                    UsedPersonLast = inc.UsedPersonLast,
                    UsedPersonFrist = inc.UsedPersonFrist,
                    UsedPersonMiddle = inc.UsedPersonMiddle,
                    UsedDob = inc.UsedDob,
                    BookingNo = inc.BookingNo,
                    AssessmentCompleteFlag = inc.AssessmentCompleteFlag,
                    NoKeeper = inc.NoKeeper,
                    BookAndReleaseFlag = inc.BookAndReleaseFlag,
                    BookCompleteFlag = inc.BookCompleteFlag,
                    BailAmountTotal = inc.BailAmountTotal,
                    BailNoBailFlagTotal = inc.BailNoBailFlagTotal
                }).ToList();

            foreach (int t in incarcerationIds)
            {
                IncarcerationDetail incarcerationDetail = new IncarcerationDetail();
                //IncarcerationArrestXref
                int?[] arrestIds = allArrestIds
                    .Where(arrXref => arrXref.Value == t)
                    .Select(arrXref => arrXref.Key).ToArray();

                List<Arrest> lstArrest = (from arr in lstAllArrest
                    where arrestIds.Contains(arr.ArrestId)
                    select new Arrest
                    {
                        ArrestSentenceCode = arr.ArrestSentenceCode,
                        ArrestSentenceIndefiniteHold = arr.ArrestSentenceIndefiniteHold,
                        ArrestSentenceReleaseDate = arr.ArrestSentenceReleaseDate
                    }).ToList();

                //TODO remove magic numbers
                incarcerationDetail.IncarcerationId = t; // to get incarcerationId for loading seal wizard
                incarcerationDetail.UnSentCnt = lstArrest.Count(arr => !arr.ArrestSentenceCode.HasValue);
                incarcerationDetail.SentCnt = lstArrest.Count(arr => arr.ArrestSentenceCode == 1);
                incarcerationDetail.AltSentCnt = lstArrest.Count(arr => arr.ArrestSentenceCode == 2);
                incarcerationDetail.HoldCnt = lstArrest.Count(arr => arr.ArrestSentenceCode == 4);
                incarcerationDetail.HoldCCnt = lstArrest.Count(arr => arr.ArrestSentenceCode == 4
                    && !arr.ArrestSentenceIndefiniteHold.HasValue && !arr.ArrestSentenceReleaseDate.HasValue);

                incarcerationDetail.HoldCdCnt = lstArrest.Count(arr => arr.ArrestSentenceCode == 4
                    && !arr.ArrestSentenceIndefiniteHold.HasValue && arr.ArrestSentenceReleaseDate.HasValue);

                incarcerationDetail.HoldICnt = lstArrest.Count(arr => arr.ArrestSentenceCode == 4
                    && arr.ArrestSentenceIndefiniteHold.HasValue && !arr.ArrestSentenceReleaseDate.HasValue);

                incarcerationDetail.ClearCnt = lstClearCount.Count(arrXref => arrXref == t);

                //Incarceration
                List<Incarceration> lstIncarceration = (from inc in lstAllIncarceration
                    where inc.IncarcerationId == t
                    select new Incarceration
                    {
                        OverallSentStartDate = inc.OverallSentStartDate,
                        OverallFinalReleaseDate = inc.OverallFinalReleaseDate,
                        ActualDaysToServe = inc.ActualDaysToServe,
                        DateIn = inc.DateIn,
                        ReleaseOut = inc.ReleaseOut,
                        UsedPersonLast = inc.UsedPersonLast,
                        UsedPersonFrist = inc.UsedPersonFrist,
                        UsedPersonMiddle = inc.UsedPersonMiddle,
                        UsedDob = inc.UsedDob,
                        BookingNo = inc.BookingNo,
                        AssessmentCompleteFlag = inc.AssessmentCompleteFlag,
                        NoKeeper = inc.NoKeeper,
                        BookAndReleaseFlag = inc.BookAndReleaseFlag,
                        BookCompleteFlag = inc.BookCompleteFlag,
                        BailAmountTotal = inc.BailAmountTotal,
                        BailNoBailFlagTotal = inc.BailNoBailFlagTotal
                    }).ToList();

                incarcerationDetail.OverallStartDate = lstIncarceration.Select(s => s.OverallSentStartDate).Single();
                incarcerationDetail.OverallReleaseDate =
                    lstIncarceration.Select(s => s.OverallFinalReleaseDate).Single();
                incarcerationDetail.ActualDaysToServe = lstIncarceration.Select(s => s.ActualDaysToServe ?? 0).Single();
                incarcerationDetail.IncarStartDate = lstIncarceration.Select(s => s.DateIn).Single();
                incarcerationDetail.IncarEndDate = lstIncarceration.Select(s => s.ReleaseOut).Single();
                incarcerationDetail.ArrestBookingDetails = GetBookings(t, toBindCharge);
                incarcerationDetail.DefaultBookingNumber = lstIncarceration.Select(s => s.BookingNo).Single();

                incarcerationDetail.AssessmentCompleteFlag = lstIncarceration.Select(s => s.AssessmentCompleteFlag).Single();
                incarcerationDetail.NoKeeper = lstIncarceration.Select(s => s.NoKeeper).Single();
                incarcerationDetail.BookReleaseFlag = lstIncarceration.Single().BookAndReleaseFlag == 1;
                incarcerationDetail.BookCompleteFlag = lstIncarceration.Single().BookCompleteFlag == 1;
                incarcerationDetail.IsNoBail = lstIncarceration.Single().BailNoBailFlagTotal == 1;
                incarcerationDetail.BailAmount = lstIncarceration.Single().BailAmountTotal ?? 0;

                string usedPersonLast = lstIncarceration.Select(s => s.UsedPersonLast).Single();
                string usedPersonFirst = lstIncarceration.Select(s => s.UsedPersonFrist).Single();
                string usedPersonMiddle = lstIncarceration.Select(s => s.UsedPersonMiddle).Single();
                string usedDob = lstIncarceration.Select(s => s.UsedDob).Single();

                bool isDiff = false;
                if (person.PersonLastName != usedPersonLast || person.PersonFirstName != usedPersonFirst
                    || person.PersonMiddleName != usedPersonMiddle)
                {
                    isDiff = true;
                }
                else if (person.PersonDob.ToString() != usedDob && string.IsNullOrEmpty(usedDob))
                {
                    isDiff = true;
                }

                if (isDiff)
                {
                    incarcerationDetail.UsedPersonLast = usedPersonLast;
                    incarcerationDetail.UsedPersonFirst = usedPersonFirst;
                    incarcerationDetail.UsedPersonMiddle = usedPersonMiddle;
                    try
                    {
                        incarcerationDetail.UsedPersonDob = DateTime.Parse(usedDob);
                    }
                    catch
                    {
                        incarcerationDetail.UsedPersonDob = null;
                    }
                }

                lstIncarcerations.Add(incarcerationDetail);
            }

            return lstIncarcerations;
        }

        public List<ArrestBookingDetails> GetBookings(int incarcerationId, bool toBindCharge = false)
        {
            var lstLookup = (from l in _context.Lookup
                             where l.LookupType == LookupConstants.ARRTYPE
                             select new
                             {
                                 l.LookupDescription,
                                 l.LookupIndex
                             }).ToList();

            //Booking
            List<ArrestBookingDetails> lstBooking =
                (from iax in _context.IncarcerationArrestXref
                 where iax.IncarcerationId == incarcerationId
                 orderby iax.Arrest.ArrestId
                 select new ArrestBookingDetails
                 {
                     ArrestId = iax.ArrestId ?? 0,
                     ReleaseDate = iax.ReleaseDate,
                     ReleaseReason = iax.ReleaseReason,
                     BookDate = iax.BookingDate,
                     IncarcerationArrestXrefId = iax.IncarcerationArrestXrefId,
                     IncarcerationArrestId = incarcerationId
                 }).ToList();

            lstBooking.ForEach(item =>
            {
                if (item.ArrestId <= 0) return;
                var arrest = (from ar in _context.Arrest
                              where ar.ArrestId == item.ArrestId
                              select new
                              {
                                  ar.ArrestType,
                                  ar.ArrestBookingNo,
                                  ar.ArrestSentenceCode,
                                  ar.BailNoBailFlag,
                                  ar.BailAmount,
                                  ar.ArrestSentenceReleaseDate,
                                  ar.ArrestSentenceActualDaysToServe,
                                  ar.ArrestingAgencyId,
                                  ar.ArrestCourtDocket,
                                  ar.ArrestCaseNumber,
                                  ar.ArrestCourtJurisdictionId,
                                  ar.ArrestSentenceIndefiniteHold,
                                  ar.ArrestLawEnforcementDisposition
                              }).Single();

                if(arrest.ArrestingAgencyId > 0)
                {
                    Agency agency = _context.Agency.Find(arrest.ArrestingAgencyId);
                    item.ArrestAbbr = agency.AgencyAbbreviation;
                    item.ArrestAgencyOriNumber = agency.AgencyOriNumber;
                }

                if(arrest.ArrestCourtJurisdictionId > 0)
                {
                    Agency agency = _context.Agency.Find(arrest.ArrestCourtJurisdictionId);
                    item.Court = agency.AgencyAbbreviation;
                    item.CourtName = agency.AgencyOriNumber;
                }

                item.BookingNo = arrest.ArrestBookingNo;
                item.BookType =
                    lstLookup.Where(l => l.LookupIndex == Convert.ToInt32(Convert.ToDecimal(arrest.ArrestType)))
                        .Select(l => l.LookupDescription)
                        .SingleOrDefault();
                item.ArrestSentenceCode = arrest.ArrestSentenceCode ?? 0;
                item.BailNoBailFlag = (arrest.BailNoBailFlag ?? 0) > 0;
                item.BailAmount = arrest.BailAmount;
                item.SentReleaseDate = arrest.ArrestSentenceReleaseDate;
                item.SentenceDaysToServe = arrest.ArrestSentenceActualDaysToServe;                
                item.Docket = arrest.ArrestCourtDocket;
                item.CaseNumber = arrest.ArrestCaseNumber;
                item.ArrestSentenceIndefiniteHold = arrest.ArrestSentenceIndefiniteHold;
                if (toBindCharge)
                {
                    item.LstCharges = GetCharges(item.ArrestId);
                }

				if (!string.IsNullOrEmpty(arrest.ArrestLawEnforcementDisposition))
				{
					item.ArrestDisposition = _context.Lookup.SingleOrDefault(lk => lk.LookupType == LookupConstants.LAWDISPO &&
					 lk.LookupIndex == Convert.ToInt32(arrest.ArrestLawEnforcementDisposition))?.LookupDescription;
				}
            });

            return lstBooking;
        }

        private List<ChargesVm> GetCharges(int? arrestId)
        {
            List<ChargesVm> lstChargeVm = new List<ChargesVm>();

            List<ChargesVm> lstCharge =
                (from c in _context.Crime
                 where c.ArrestId == arrestId && c.CrimeDeleteFlag == 0
                 select new ChargesVm
                 {
                     CrimeId = c.CrimeId,
                     WarrantNumber = c.Warrant.WarrantNumber,
                     CrimeSection = c.CrimeLookup.CrimeSection,
                     CrimeSubSection = c.CrimeLookup.CrimeSubSection,
                     Description = c.CrimeLookup.CrimeDescription,
                     Type = c.CrimeLookup.CrimeCodeType,
                     CrimeStatueCode = c.CrimeLookup.CrimeStatuteCode,
                     BailNoBailFlag = c.BailNoBailFlag,
                     BailAmount = c.BailAmount,
                     CreateDate = c.CreateDate,
                     WarrantId = c.WarrantId ?? 0
                 }).ToList();

            lstChargeVm.AddRange(lstCharge);

            List<ChargesVm> lstCrimeForce =
                (from c in _context.CrimeForce
                    where c.ArrestId == arrestId && (c.DropChargeFlag ?? 0) == 0
                        && (c.ForceCrimeLookupId ?? 0) == 0 &&
                        (c.SearchCrimeLookupId ?? 0) == 0 && c.DeleteFlag == 0
                    select new ChargesVm
                    {
                        CrimeForceId = c.CrimeForceId,
                        WarrantNumber = c.Warrant.WarrantNumber,
                        CrimeSection = c.TempCrimeSection,
                        Description = c.TempCrimeDescription,
                        Type = c.TempCrimeCodeType,
                        CrimeStatueCode = c.TempCrimeStatuteCode,
                        WarrantId = c.WarrantId ?? 0,
                        BailNoBailFlag = c.BailNoBailFlag,
                        BailAmount = c.BailAmount,
                        CreateDate = c.CreateDate
                    }).ToList();

            lstChargeVm.AddRange(lstCrimeForce);

            // get the incarceration Id for seal grid
            List<ChargesVm> lstIncarceration =
                (from iax in _context.IncarcerationArrestXref
                    where iax.ArrestId == arrestId
                    orderby iax.Arrest.ArrestId
                    select new ChargesVm
                    {
                        IncarcerationId = iax.IncarcerationId ?? 0
                    }).ToList();

            lstChargeVm.AddRange(lstIncarceration);

            List<ChargesVm> lstWarrant =
                (from c in _context.Warrant
                 where c.ArrestId == arrestId
                 select new ChargesVm
                 {
                     WarrantNumber = c.WarrantNumber,
                     Description = c.WarrantType,
                     WarrantId = c.WarrantId,
                     WarrantBailType = c.WarrantBailType,
                     BailAmount = c.WarrantBailAmount,
                     CreateDate = c.CreateDate
                 }).ToList();

            lstChargeVm.AddRange(lstWarrant);
            lstChargeVm = lstChargeVm.OrderBy(x => x.CreateDate).ThenBy(x => x.WarrantNumber).ToList();

            return lstChargeVm;
        }

        public List<TaskOverview> GetAllCompleteTasks(int inmateId)
        {
            List<TaskOverview> lstAoTaskQueues = _context.AoTaskQueue.Where(t => t.InmateId == inmateId).Select(t => new TaskOverview
            {
                CompleteById = t.CompleteBy ?? 0
            }).ToList();

            return lstAoTaskQueues;
        }
    }
}