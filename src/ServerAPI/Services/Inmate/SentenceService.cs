using GenerateTables.Models;
using Microsoft.AspNetCore.Http;
using System;
using ServerAPI.Utilities;
using ServerAPI.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace ServerAPI.Services
{
    public class SentenceService : ISentenceService
    {
        private readonly AAtims _context;
        private readonly ICommonService _commonService;
        private readonly int _personnelId;
        private SentenceDetailsVm _sentenceDetails;
        private readonly bool _sentenceByCharge;
        private readonly IPersonService _personService;
        private readonly IInterfaceEngineService _interfaceEngineService;

        public SentenceService(AAtims context, ICommonService commonService, IHttpContextAccessor httpContextAccessor,
            IPersonService personService, IInterfaceEngineService interfaceEngineService)
        {
            _context = context;
            _commonService = commonService;
            _personnelId = Convert.ToInt32(httpContextAccessor.HttpContext.User
                .FindFirst("personnelId")?.Value);
            _sentenceByCharge = _commonService.GetSiteOptionValue(SiteOptionsConstants.SENTENCEBYCHARGE) == "ON";
            _personService = personService;
            _interfaceEngineService = interfaceEngineService;
        }

        public SentenceViewerVm GetSentenceViewerList(int arrestId, int incarcerationId)
        {
            SentenceViewerVm sentenceViewer = new SentenceViewerVm
            {
                ////To get Arrest Booking Status
                //BookingStatus = GetBookingStatus(arrestId),

                //To get Sentence Details
                SentenceList = GetSentenceDetailsIncarceration(incarcerationId),

                ChargeSentence = GetAllArrestSentenceDetailsCrime(incarcerationId),

                ////To get Overall Sentence Details
                //OverallSentence = GetOverallSentence(incarcerationId),

                SentenceByCharge = _sentenceByCharge
            };
            return sentenceViewer;
        }

        #region BookingStatus

        public BookingStatusVm GetBookingStatus(int arrestId)
        {
            BookingStatusVm bookingStatus = new BookingStatusVm();

            if (arrestId == 0)
            {
                return bookingStatus;
            }

            IQueryable<Lookup> lookups = _context.Lookup
                .Where(w => (w.LookupType == LookupConstants.BOOKSTAT || w.LookupType == LookupConstants.ARRTYPE));

            bookingStatus = _context.Arrest.Where(a => a.ArrestId == arrestId).Select(a =>
                new BookingStatusVm
                {
                    ArrestId = a.ArrestId,
                    InmateId = a.InmateId,
                    ArrestBookingStatus = lookups.SingleOrDefault(
                            w => w.LookupIndex == a.ArrestBookingStatus &&
                                 w.LookupType == LookupConstants.BOOKSTAT)
                        .LookupDescription,
                    ArrestBookingStatusId = a.ArrestBookingStatus,
                    ArrestBookingStatusList =
                        lookups.Where(w => w.LookupType == LookupConstants.BOOKSTAT && w.LookupInactive == 0)
                            .OrderByDescending(o => o.LookupOrder).ThenBy(o => o.LookupDescription)
                            .Select(t =>
                                new KeyValuePair<int, string>(t.LookupIndex, t.LookupDescription))
                            .ToList(),
                    ArrestBookingAbbr = lookups.SingleOrDefault(
                            w => w.LookupIndex == a.ArrestBookingStatus &&
                                 w.LookupType == LookupConstants.BOOKSTAT)
                        .LookupName,
                    ArrestConvictionDate = a.ArrestConvictionDate,
                    ArrestConvictionNote = a.ArrestConvictionNote,
                    ArrestBookingNumber = a.ArrestBookingNo,
                    ArrestBookingType = lookups.SingleOrDefault(
                            w => w.LookupIndex == Convert.ToInt32(a.ArrestType) &&
                                 w.LookupType == LookupConstants.ARRTYPE)
                        .LookupDescription,
                    CourtDocket = a.ArrestCourtDocket,
                    Court = _context.Agency
                        .SingleOrDefault(w => w.AgencyId == a.ArrestCourtJurisdictionId)
                        .AgencyAbbreviation
                }).Single();

            return bookingStatus;
        }
        public BookingStatusVm GetCaseConviction(int arrestId)
        {
            BookingStatusVm bookingStatus = new BookingStatusVm();

            if (arrestId == 0)
            {
                return bookingStatus;
            }

            IQueryable<Lookup> lookups = _context.Lookup
                .Where(w => (w.LookupType == LookupConstants.BOOKSTAT || w.LookupType == LookupConstants.ARRTYPE));
            //line: 121, Changed single to singleordefault to fix object reference issue.
            bookingStatus = _context.Arrest.Where(a => a.ArrestId == arrestId).Select(a =>
                new BookingStatusVm
                {
                    ArrestId = a.ArrestId,
                    InmateId = a.InmateId,
                    ArrestBookingStatus = lookups.SingleOrDefault(w => w.LookupIndex == a.ArrestBookingStatus &&
                                 w.LookupType == LookupConstants.BOOKSTAT).LookupDescription,
                    ArrestBookingStatusId = a.ArrestBookingStatus,
                    ArrestBookingStatusList =
                        lookups.Where(w => w.LookupType == LookupConstants.BOOKSTAT && w.LookupInactive == 0)
                            .OrderByDescending(o => o.LookupOrder).ThenBy(o => o.LookupDescription)
                            .Select(t =>
                                new KeyValuePair<int, string>(t.LookupIndex, t.LookupDescription))
                            .ToList(),
                    ArrestBookingAbbr = lookups.SingleOrDefault(
                            w => w.LookupIndex == a.ArrestBookingStatus &&
                                 w.LookupType == LookupConstants.BOOKSTAT)
                        .LookupName,
                    ArrestConvictionDate = a.ArrestConvictionDate,
                    ArrestConvictionNote = a.ArrestConvictionNote,
                    ArrestBookingNumber = a.ArrestBookingNo,
                    ArrestBookingType = lookups.SingleOrDefault(
                            w => w.LookupIndex == Convert.ToInt32(a.ArrestType) &&
                                 w.LookupType == LookupConstants.ARRTYPE)
                        .LookupDescription,
                    CourtDocket = a.ArrestCourtDocket,
                    Court = _context.Agency
                        .SingleOrDefault(w => w.AgencyId == a.ArrestCourtJurisdictionId)
                        .AgencyAbbreviation
                }).Single();

            return bookingStatus;
        }

        public List<BookingStatusHistoryVm> GetBookingStatusHistory(int arrestId)
        {
            IQueryable<Lookup> lookups = _context.Lookup.Where(w => w.LookupType == LookupConstants.BOOKSTAT);

            List<BookingStatusHistoryVm> bookingStatusHistory =
                _context.ArrestBookingStatusHistory.Where(a => a.ArrestId == arrestId)
                    .OrderByDescending(o => o.ArrestBookingStatusChange).Select(a =>
                        new BookingStatusHistoryVm
                        {
                            ArrestBookingStatus =
                                lookups.SingleOrDefault(l => l.LookupIndex == a.ArrestBookingStatus).LookupDescription,
                            ArrestBookingAbbr =
                                lookups.SingleOrDefault(l => l.LookupIndex == a.ArrestBookingStatus).LookupName,
                            SavedBy = a.ArrestBookingStatusBy,
                            SavedDate = a.ArrestBookingStatusChange,
                            ArrestConvictionDate = a.ArrestConvictionDate,
                            ArrestConvictionNote = a.ArrestConvictionNote
                        }).ToList();

            List<int> lstPersonnelIds = bookingStatusHistory.Where(a => a.SavedBy.HasValue)
                .Select(a => a.SavedBy.Value)
                .ToList();

            List<PersonnelVm> lstPersonDetails = _personService.GetPersonNameList(lstPersonnelIds);

            bookingStatusHistory.ForEach(item =>
            {
                PersonnelVm personInfo = lstPersonDetails
                    .SingleOrDefault(a => a.PersonnelId == item.SavedBy);
                if (personInfo != null)
                {
                    item.SavedPersonFirstName = personInfo.PersonFirstName;
                    item.SavedPersonLastName = personInfo.PersonLastName;
                    item.SavedPersonMiddleName = personInfo.PersonMiddleName;
                    item.OfficerBadgeNumber = personInfo.OfficerBadgeNumber;
                }
            });

            return bookingStatusHistory.ToList();
        }

        public async Task<int> UpdateArrestBookingStatus(BookingStatusVm value)
        {
            Arrest arrest = _context.Arrest.Single(s => s.ArrestId == value.ArrestId);
            {
                arrest.ArrestBookingStatus = value.ArrestBookingStatusId;
                arrest.ArrestBookingStatusChange = DateTime.Now;
                arrest.ArrestConvictionDate = value.ArrestConvictionDate;
                arrest.ArrestBookingStatusBy = _personnelId;
                arrest.ArrestConvictionNote = value.ArrestConvictionNote;
                arrest.UpdateBy = _personnelId;
            }

            //Event Handle
            //EventVm evenHandle = new EventVm
            //{
            //    PersonId = value.PersonId,
            //    CorresId = value.ArrestId,
            //    EventName = EventNameConstants.CASESTATUSCHANGE
            //};
            // INSERT  EVENT HANDLE
            //_commonService.EventHandle(evenHandle);
            _interfaceEngineService.Export(new ExportRequestVm
            {
                EventName = EventNameConstants.CASESTATUSCHANGE,
                PersonnelId = _personnelId,
                Param1 = value.PersonId.ToString(),
                Param2 = value.ArrestId.ToString()
            });

            ArrestBookingStatusHistory arrestBookingStatusHistory = new ArrestBookingStatusHistory();
            {
                arrestBookingStatusHistory.ArrestId = value.ArrestId;
                arrestBookingStatusHistory.ArrestBookingStatus = value.ArrestBookingStatusId;
                arrestBookingStatusHistory.ArrestBookingStatusChange = DateTime.Now;
                arrestBookingStatusHistory.ArrestBookingStatusBy = _personnelId;
                arrestBookingStatusHistory.ArrestConvictionDate = value.ArrestConvictionDate;
                arrestBookingStatusHistory.ArrestConvictionNote = value.ArrestConvictionNote;
            }
            _context.Add(arrestBookingStatusHistory);

            return await _context.SaveChangesAsync();
        }

        #endregion

        #region SentenceList

        public List<SentenceVm> GetSentenceDetailsIncarceration(int incarcerationId)
        {
            IQueryable<Lookup> lookups = _context.Lookup
                .Where(w => w.LookupType == LookupConstants.ARRTYPE || w.LookupType == LookupConstants.BOOKSTAT
                                                                    || w.LookupType ==
                                                                    LookupConstants.CHARGEQUALIFIER ||
                                                                    w.LookupType == LookupConstants.CRIMETYPE);

            List<SentenceVm> lstBookingSentence = _context.IncarcerationArrestXref
                .Where(w => w.IncarcerationId == incarcerationId)
                .Select(s => new SentenceVm
                {
                    ArrestBookingNo = s.Arrest.ArrestBookingNo,
                    Type = lookups.SingleOrDefault(w => w.LookupIndex == Convert.ToInt32(s.Arrest.ArrestType)
                                                        && w.LookupType == LookupConstants.ARRTYPE).LookupDescription,
                    ArrestCourtDocket = s.Arrest.ArrestCourtDocket,
                    ArrestSentenceConsecutiveFlag = s.Arrest.ArrestSentenceConsecutiveFlag == 1,
                    ArrestSentenceConsecutiveTo = s.Arrest.ArrestSentenceConsecutiveTo,
                    ArrestSentenceStartDate = s.Arrest.ArrestSentenceStartDate,
                    ArrestSentenceUseStartDate = s.Arrest.ArrestSentenceUseStartDate,
                    ArrestSentenceDays = s.Arrest.ArrestSentenceDays,
                    ArrestSentenceForthwith = s.Arrest.ArrestSentenceForthwith == 1,
                    MethodName = s.Arrest.ArrestSentenceMethod.MethodName,
                    ArrestSentenceDaysToServe = s.Arrest.ArrestSentenceDaysToServe,
                    ArrestSentenceActualDaysToServe = s.Arrest.ArrestSentenceActualDaysToServe,
                    ArrestSentenceReleaseDate = s.Arrest.ArrestSentenceReleaseDate,
                    ArrestSentenceCode = s.Arrest.ArrestSentenceCode ?? 0,
                    ArrestSentenceDescription = s.Arrest.ArrestSentenceDescription,
                    ArrestId = s.ArrestId,
                    IncarcerationId = s.IncarcerationId,
                    IncarcerationArrestXrefId = s.IncarcerationArrestXrefId,
                    ArrestSentenceGroup = s.Arrest.ArrestSentenceGroup,
                    WeekEnder = s.Arrest.ArrestSentenceWeekender,
                    Abbr = lookups
                        .Where(w => w.LookupType == LookupConstants.BOOKSTAT &&
                                    w.LookupIndex == (s.Arrest.ArrestBookingStatus ?? 0))
                        .Select(a => a.LookupName).SingleOrDefault(),
                    ArrestClearScheduleDate = s.ReleaseDate,
                    ReleaseReason = s.ReleaseReason,
                    ArrestSentenceDaysAmount = s.Arrest.ArrestSentenceDaysAmount,
                    SentenceGapFound = new SentenceGapFound
                    {
                        ProcessFlag = false
                    },
                    ArrestSentenceIndefiniteHold = s.Arrest.ArrestSentenceIndefiniteHold == 1,
                    ArrestSupSeqNumber = s.Arrest.ArrestSupSeqNumber,
                    ArrestSentenceDaysStayed = s.Arrest.ArrestSentenceDaysStayed,
                    ArrestSentenceDaysStayedInterval = s.Arrest.ArrestSentenceDaysStayedInterval,
                    ArrestSentenceDaysStayedAmount = s.Arrest.ArrestSentenceDaysStayedAmount,
                    ArrestTimeServedDays = s.Arrest.ArrestTimeServedDays,
                    ArrestDate = s.Arrest.ArrestDate,
                    ArrestSentenceStopDaysFlag = s.Arrest.ArrestSentenceStopDaysFlag
                }).ToList();

            return lstBookingSentence.OrderBy(s => s.ArrestId)
                .ThenBy(s => s.IncarcerationId)
                .ThenBy(s => s.ArrestSentenceGroup)
                .ThenBy(s => s.ArrestSentenceConsecutiveFlag).ToList();
        }

        public SentenceCreditServedVm GetSentenceCreditServed(int inmateId, int incarcerationArrestXrefId,
            string courtDocket)
        {
            IQueryable<Lookup> lookups = _context.Lookup
                .Where(w => w.LookupType == LookupConstants.BOOKSTAT || w.LookupType == LookupConstants.ARRTYPE);

            IQueryable<IncarcerationArrestXref> sentenceCreditServedList =
                _context.IncarcerationArrestXref.Where(w => w.Incarceration.InmateId == inmateId);

            SentenceCreditServedVm sentenceCreditServed = new SentenceCreditServedVm
            {
                CurrentDetails = sentenceCreditServedList
                    .Where(w => w.IncarcerationArrestXrefId == incarcerationArrestXrefId)
                    .Select(s => new SentenceCreditServed
                    {
                        BookingDate = s.BookingDate,
                        ReleaseDate = s.Arrest.ArrestSentenceStartDate ?? DateTime.Now,
                        ArrestBookingNo = s.Arrest.ArrestBookingNo,
                        ArrestType = lookups.SingleOrDefault(
                                w => w.LookupIndex == Convert.ToInt32(s.Arrest.ArrestType) &&
                                     w.LookupType == LookupConstants.ARRTYPE)
                            .LookupDescription,
                        ArrestCourtDocket = s.Arrest.ArrestCourtDocket,
                        ArrestBookingStatus = lookups.SingleOrDefault(
                                w => w.LookupIndex == s.Arrest.ArrestBookingStatus &&
                                     w.LookupType == LookupConstants.BOOKSTAT).LookupName,
                        ArrestConvictionDate = s.Arrest.ArrestConvictionDate,
                        IsDayDifference = true,
                        ArrestCourtJurisdictionAbbr = _context.Agency
                                .SingleOrDefault(w => w.AgencyId == s.Arrest.ArrestCourtJurisdictionId)
                                .AgencyAbbreviation
                    }).Single(),

                SameDocketDetails = sentenceCreditServedList
                    .Where(w => w.Arrest.ArrestCourtDocket == courtDocket &&
                                !string.IsNullOrEmpty(w.Arrest.ArrestCourtDocket))
                    .Select(s => new SentenceCreditServed
                    {
                        BookingDate = s.BookingDate,
                        ReleaseDate = s.ReleaseDate,
                        ArrestBookingNo = s.Arrest.ArrestBookingNo,
                        ArrestType = lookups.SingleOrDefault(w => w.LookupIndex == Convert.ToInt32(s.Arrest.ArrestType) &&
                                     w.LookupType == LookupConstants.ARRTYPE).LookupDescription,
                        ArrestCourtDocket = s.Arrest.ArrestCourtDocket,
                        ArrestBookingStatus = lookups.SingleOrDefault(w => w.LookupIndex == s.Arrest.ArrestBookingStatus &&
                                     w.LookupType == LookupConstants.BOOKSTAT).LookupName,
                        ArrestConvictionDate = s.Arrest.ArrestConvictionDate,
                        ArrestCourtJurisdictionAbbr = _context.Agency
                                .SingleOrDefault(w => w.AgencyId == s.Arrest.ArrestCourtJurisdictionId)
                                .AgencyAbbreviation
                    }).OrderByDescending(o => o.BookingDate).ToList(),

                OtherDocketDetails = sentenceCreditServedList
                    .Where(w => w.Arrest.ArrestCourtDocket != courtDocket && !string.IsNullOrWhiteSpace(w.Arrest.ArrestCourtDocket))
                    .Select(s => new SentenceCreditServed
                    {
                        BookingDate = s.BookingDate,
                        ReleaseDate = s.ReleaseDate,
                        ArrestBookingNo = s.Arrest.ArrestBookingNo,
                        ArrestType = lookups.SingleOrDefault(w => w.LookupIndex == Convert.ToInt32(s.Arrest.ArrestType) &&
                                     w.LookupType == LookupConstants.ARRTYPE).LookupDescription,
                        ArrestCourtDocket = s.Arrest.ArrestCourtDocket,
                        ArrestBookingStatus = lookups.SingleOrDefault(w => w.LookupIndex == s.Arrest.ArrestBookingStatus &&
                                     w.LookupType == LookupConstants.BOOKSTAT).LookupName,
                        ArrestConvictionDate = s.Arrest.ArrestConvictionDate,
                        ArrestCourtJurisdictionAbbr = _context.Agency
                                .SingleOrDefault(w => w.AgencyId == s.Arrest.ArrestCourtJurisdictionId)
                                .AgencyAbbreviation
                    }).OrderByDescending(o => o.BookingDate).ToList()
            };

            return sentenceCreditServed;
        }

        #endregion

        #region OverallSentence

        public OverallSentenceVm GetOverallSentence(int incarcerationId)
        {
            List<ArrestSentenceSetting> arrestSentenceSettings = _context.ArrestSentenceSetting.
                                             Where(w => w.FieldTable == SentenceSettingConstants.INCARCERATION).ToList();
            OverallSentenceVm overallSentence =
                _context.Incarceration.Where(i => i.IncarcerationId == incarcerationId).Select(i =>
                    new OverallSentenceVm
                    {
                        IncarcerationId = i.IncarcerationId,
                        InmateId = i.InmateId,
                        OverallSentStartDate = i.OverallSentStartDate,
                        OverallFinalReleaseDate = i.OverallFinalReleaseDate,
                        ActualDaysToServe = i.ActualDaysToServe,
                        SentenceByCharge = _sentenceByCharge,
                        //ListLookup = _commonService.GetLookups(new[] {LookupConstants.CRIMETYPE}),
                        Manual = i.OverallSentManual == 1,
                        Erc = i.OverallsentErc ?? 0,
                        ErcClear = i.OverallsentErcclear == 1,
                        AltSentProgramList = _context.AltSent
                            .Where(w => w.Incarceration.IncarcerationId == i.IncarcerationId)
                            .Select(x => new SentenceAltSentProgramVm
                            {
                                AltSentId = x.AltSentId,
                                IncarcerationId = x.IncarcerationId,
                                AltSentProgramId = x.AltSentProgramId,
                                AltSentProgramAbbr = x.AltSentProgram.AltSentProgramAbbr,
                                AltSentStart = x.AltSentStart,
                                AltSentThru = x.AltSentThru,
                                FacilityAbbr = x.AltSentProgram.Facility.FacilityAbbr,
                                Adts = x.AltSentAdts,
                                DaysAttend = x.AltSentTotalAttend
                            }).OrderByDescending(o => o.AltSentId).FirstOrDefault(),
                        OercDetails = _context.ArrestSentenceMethodOerc.Where(w => w.ArrestSentenceMethodOercid == 1)
                            .Select(x => new OercDetailsVm
                            {
                                Visible = x.Oercvisible == 1,
                                DaysRange = x.OercdaysRange,
                                Credit = x.Oerccredit,
                                UseMaxDts = x.OercdaysRangeUseMaxDts,
                                UseMaxD = x.OercdaysRangeUseMaxD
                            }).FirstOrDefault(),
                        BookCompleteFlag = i.BookCompleteFlag == 1,
                        TransportFlag = i.TransportFlag == 1,
                        OverAllErcFlag = arrestSentenceSettings.Where(w => w.FieldName == SentenceSettingConstants.OVERALLERC)
                                        .Select(s => s.InvisibleFlag == 0).SingleOrDefault(),
                        OverAllErcClearFlag = arrestSentenceSettings.Where(w => w.FieldName ==  SentenceSettingConstants.OVERALLERCCLEAR)
                                        .Select(s => s.InvisibleFlag == 0).SingleOrDefault(),
                        ArrestDate=i.IncarcerationArrestXref.Select(s=>s.Arrest.ArrestDate).FirstOrDefault()

                    }).Single();

            return overallSentence;
        }

        #endregion

        #region SentenceEntry

        public List<SentenceAdditionalFlagsVm> GetSentenceAdditionalFlags(int arrestId)
        {
            IQueryable<Lookup> lookups = _context.Lookup.Where(w => w.LookupType == LookupConstants.SENTFLAG && w.LookupInactive == 0);

            List<SentenceAdditionalFlagsVm> arrestSentFlag =
                _context.ArrestSentFlag.Where(w => w.ArrestId == arrestId && (!w.DeleteFlag.HasValue || w.DeleteFlag == 0))
                .Select(s => new SentenceAdditionalFlagsVm
                {
                    ArrestId = s.ArrestId,
                    ArrestSentFlagId = s.ArrestSentflagId,
                    LookupIndex = s.SentflagLookupIndex,
                    LookupDescription = lookups.SingleOrDefault(w => w.LookupIndex == s.SentflagLookupIndex)
                            .LookupDescription,
                    Selected = true
                }).ToList();

            arrestSentFlag.AddRange(lookups.Where(w => arrestSentFlag.All(a => a.LookupIndex != w.LookupIndex))
                .Select(s =>
                    new SentenceAdditionalFlagsVm
                    {
                        ArrestId = arrestId,
                        LookupIndex = s.LookupIndex,
                        LookupDescription = s.LookupDescription,
                        Selected = false
                    }).OrderBy(o => o.LookupDescription));

            return arrestSentFlag.ToList();
        }

        public async Task<List<SentenceAdditionalFlagsVm>> InsertArrestSentFlag(List<SentenceAdditionalFlagsVm> value)
        {
            value.ForEach(f =>
            {
                ArrestSentFlag getArrestSentFlag;
                if (f.ArrestSentFlagId > 0)
                {
                    getArrestSentFlag = _context.ArrestSentFlag.Single(w => w.ArrestSentflagId == f.ArrestSentFlagId);

                    getArrestSentFlag.DeleteFlag = f.Selected ? 0 : 1;
                    getArrestSentFlag.DeleteDate = DateTime.Now;
                    getArrestSentFlag.DeletedBy = _personnelId;
                }
                else
                {
                    if (f.Selected)
                    {
                        getArrestSentFlag = new ArrestSentFlag
                        {
                            ArrestId = f.ArrestId,
                            SentflagLookupIndex = f.LookupIndex,
                            CreateBy = _personnelId,
                            CreateDate = DateTime.Now
                        };
                        _context.ArrestSentFlag.Add(getArrestSentFlag);
                    }
                }
            });

            await _context.SaveChangesAsync();


            List<SentenceAdditionalFlagsVm> arrestSentFlagList = GetSentenceAdditionalFlags(value[0].ArrestId);
            return arrestSentFlagList;
        }

        public async Task<int> DeleteArrestSentFlag(int arrestSentFlagId)
        {
            ArrestSentFlag arrestSentFlag =
                _context.ArrestSentFlag.SingleOrDefault(w => w.ArrestSentflagId == arrestSentFlagId);

            if (arrestSentFlag != null)
            {
                arrestSentFlag.DeleteFlag = 1;
                arrestSentFlag.DeletedBy = _personnelId;
                arrestSentFlag.DeleteDate = DateTime.Now;
            }

            return await _context.SaveChangesAsync();
        }

        private static int NullableTryParseInt32(string text)
        {
            return int.TryParse(text, out int value) ? value : 0;
        }

        public SentenceDetailsVm GetSentenceDetailsArrest(int arrestId)
        {
            IQueryable<Lookup> lookuplst = _context.Lookup
                .Where(w => w.LookupType == LookupConstants.ARRTYPE || w.LookupType == LookupConstants.BOOKSTAT ||
                            w.LookupType == LookupConstants.SENTFLAG || w.LookupType == LookupConstants.SENTTYPE ||
                            w.LookupType == LookupConstants.SENTFIND || w.LookupType == LookupConstants.SENTDURATION ||
                            w.LookupType == LookupConstants.SENTFINETYPE);

            IQueryable<ArrestSentenceMethod> sentenceMethodLst = _context.ArrestSentenceMethod;

            _sentenceDetails = _context.Arrest
                .Where(w => w.ArrestId == arrestId)
                .Select(s => new SentenceDetailsVm
                {
                    ArrestBookingNo = s.ArrestBookingNo,
                    ArrestDate = s.ArrestDate,
                    Type = lookuplst.Where(w => w.LookupIndex == Convert.ToInt32(s.ArrestType)
                                                          && w.LookupType == LookupConstants.ARRTYPE)
                                                          .Select(w => w.LookupDescription).SingleOrDefault(),
                    ArrestCourtDocket = s.ArrestCourtDocket,
                    ArrestSentenceConsecutiveFlag = s.ArrestSentenceConsecutiveFlag == 1,
                    ArrestSentenceConsecutiveTo = s.ArrestSentenceConsecutiveTo,
                    ArrestBookingNo1 = _context.Arrest.Where(w => w.ArrestId == s.ArrestSentenceConsecutiveTo)
                        .Select(i => i.ArrestBookingNo).SingleOrDefault(),
                    ArrestSentenceStartDate = s.ArrestSentenceStartDate,
                    ArrestSentenceUseStartDate = s.ArrestSentenceUseStartDate,
                    ArrestSentenceDays = s.ArrestSentenceDays,
                    MethodName = s.ArrestSentenceMethod.MethodName,
                    ArrestSentenceDaysToServe = s.ArrestSentenceDaysToServe,
                    ArrestSentenceActualDaysToServe = s.ArrestSentenceActualDaysToServe,
                    ArrestSentenceCode = s.ArrestSentenceCode ?? 0,
                    ArrestSentenceDescription = s.ArrestSentenceDescription,
                    ArrestId = s.ArrestId,
                    ArrestSentenceGroup = s.ArrestSentenceGroup,
                    WeekEnder = s.ArrestSentenceWeekender,
                    Abbr = lookuplst
                        .Where(w => w.LookupType == LookupConstants.BOOKSTAT &&
                                    w.LookupIndex == (s.ArrestBookingStatus ?? 0))
                        .Select(a => a.LookupName).SingleOrDefault(),
                    ArrestSentenceReleaseDate = s.ArrestSentenceReleaseDate,
                    ArrestSentenceAmended = s.ArrestSentenceAmended == 1,
                    ArrestSentencePenalInstitution = s.ArrestSentencePenalInstitution == 1,
                    ArrestSentenceOptionsRec = s.ArrestSentenceOptionsRec == 1,
                    ArrestSentenceAltSentNotAllowed = s.ArrestSentenceAltSentNotAllowed == 1,
                    ArrestSentenceNoEarlyRelease = s.ArrestSentenceNoEarlyRelease == 1,
                    ArrestSentenceNoLocalParole = s.ArrestSentenceNoLocalParole == 1,
                    ArrestSentenceNoDayForDay = s.ArrestSentenceNoDayForDay == 1,
                    ArrestSentenceWeekender = s.ArrestSentenceWeekender == 1,
                    AdditionalFlagsList = _context.ArrestSentFlag
                        .Where(w => w.ArrestId == s.ArrestId && (!w.DeleteFlag.HasValue || w.DeleteFlag == 0))
                        .Select(x => new SentenceAdditionalFlagsVm
                        {
                            ArrestSentFlagId = x.ArrestSentflagId,
                            ArrestId = x.ArrestId,
                            LookupDescription = lookuplst.Where(w => w.LookupIndex == x.SentflagLookupIndex
                                                                               && w.LookupType ==
                                                                               LookupConstants.SENTFLAG)
                                                                               .Select(w => w.LookupDescription)
                                .SingleOrDefault(),
                            LookupIndex = x.SentflagLookupIndex,
                            Selected = true
                        }).ToList(),
                    ArrestSentenceDateInfo = s.ArrestSentenceDateInfo,
                    ArrestSentenceType = s.ArrestSentenceType,
                    ArrestSentenceTypeList = lookuplst.Where(w => w.LookupType == LookupConstants.SENTTYPE).OrderByDescending
                            (o => o.LookupOrder).ThenBy(o => o.LookupDescription).Select
                            (l => new KeyValuePair<int, string>(l.LookupIndex, l.LookupDescription))
                        .ToList(),
                    ArrestSentenceFindings = s.ArrestSentenceFindings,
                    ArrestSentenceFindingsList = lookuplst.Where(w => w.LookupType == LookupConstants.SENTFIND).OrderByDescending
                            (o => o.LookupOrder).ThenBy(o => o.LookupDescription).Select
                            (l => new KeyValuePair<int, string>(l.LookupIndex, l.LookupDescription))
                        .ToList(),
                    ArrestSentenceJudgeId = s.ArrestSentenceJudgeId,
                    ArrestSentenceJudgeList = LoadJudgeDetails(null),
                    ArrestSentenceDaysInterval = s.ArrestSentenceDaysInterval,
                    ArrestSentenceDaysIntervalList = lookuplst.Where(w => w.LookupType == LookupConstants.SENTDURATION)
                        .OrderByDescending(o => o.LookupOrder).ThenBy(o => o.LookupDescription).Select
                        (l => new KeyValuePair<int, string>(NullableTryParseInt32(l.LookupMap),
                            l.LookupDescription)).ToList(),
                    ArrestSentenceFineDays = s.ArrestSentenceFineDays,
                    ArrestSentenceDaysStayed = s.ArrestSentenceDaysStayed,
                    ArrestSentenceDaysStayedInterval = s.ArrestSentenceDaysStayedInterval,
                    ArrestSentenceDaysStayedAmount = s.ArrestSentenceDaysStayedAmount,
                    ArrestTimeServedDays = s.ArrestTimeServedDays,
                    ArrestSentenceForthwith = s.ArrestSentenceForthwith == 1,
                    ArrestSentenceFlatTime = s.ArrestSentenceFlatTime,
                    ArrestSentenceManual = s.ArrestSentenceManual == 1,
                    ArrestSentenceMethodId = s.ArrestSentenceMethodId,
                    ArrestSentenceMethodList =
                        sentenceMethodLst.Where(w => w.InactiveFlag != 1).Select
                            (a => new KeyValuePair<int, string>(a.ArrestSentenceMethodId,
                                a.MethodName))
                            .ToList(),
                    DefaultSentenceMethodId = sentenceMethodLst.FirstOrDefault(f => f.DefaultManualSent).ArrestSentenceMethodId,
                    ArrestSentenceDaysAmount = s.ArrestSentenceDaysAmount,
                    ArrestSentenceHours = s.ArrestSentenceHours ?? 0,
                    ArrestSentenceExpirationDate = s.ArrestSentenceExpirationDate,
                    ArrestSentenceDayForDayAllowedOverride = s.ArrestSentenceDayForDayAllowedOverride == 1,
                    ArrestSentenceGtDaysOverride = s.ArrestSentenceGtDaysOverride == 1,
                    ArrestSentenceWtDaysOverride = s.ArrestSentenceWtDaysOverride == 1,
                    ArrestSentenceErcDays = s.ArrestSentenceErcdays,
                    ArrestSentenceDayForDayDaysOverride = s.ArrestSentenceDayForDayDaysOverride == 1,
                    ArrestSentenceDayForDayAllowed = s.ArrestSentenceDayForDayAllowed,
                    ArrestSentenceGtDays = s.ArrestSentenceGtDays ?? 0,
                    ArrestSentenceWtDays = s.ArrestSentenceWtDays ?? 0,
                    ArrestSentenceGwGtAdjust = s.ArrestSentenceGwGtAdjust ?? 0,
                    ArrestSentenceDayForDayDays = s.ArrestSentenceDayForDayDays,
                    ArrestSentenceDisciplinaryDaysSum = s.ArrestSentenceDisciplinaryDaysSum,
                    ArrestSentenceIndefiniteHold = s.ArrestSentenceIndefiniteHold == 1,
                    SentenceFineDays = new SentenceFineDaysVm
                    {
                        ArrestSentenceFinePaid = s.ArrestSentenceFinePaid,
                        ArrestSentenceFineToServe = s.ArrestSentenceFineToServe,
                        ArrestSentenceFineType = s.ArrestSentenceFineType,
                        ArrestSentenceFineAmount = s.ArrestSentenceFineAmount,
                        ArrestSentenceFinePerDay = s.ArrestSentenceFinePerDay,
                        ArrestSentenceFineTypeList = lookuplst.Where(a => a.LookupType == LookupConstants.SENTFINETYPE)
                            .Select(a => new LookupVm
                            {
                                LookupIndex = a.LookupIndex,
                                LookupName = a.LookupName,
                                LookupDescription = a.LookupDescription,
                                LookupCategory = a.LookupCategory,
                                LookupOrder = a.LookupOrder
                            }).OrderByDescending(o => o.LookupOrder).ThenBy(o => o.LookupDescription).ToList()
                    },
                    AllowWeekEnderSentence =
                        _commonService.GetSiteOptionValue(SiteOptionsConstants.ALLOWWEEKENDERSENTENCE) == "ON",
                    NoDayForDayVisible =
                        _commonService.GetSiteOptionValue(SiteOptionsConstants.NODAYFORDAYVISIBLE) == "ON",
                    SameDayUseStart =
                        _commonService.GetSiteOptionValue(SiteOptionsConstants.CONSECUTIVESENTENCESAMEDAYUSESTART) == "ON",
                    InmateId = s.InmateId ?? 0,

                    NoBailSiteOptionValue = _commonService.GetSiteOptionValue(SiteOptionsConstants.SETNOBAILAFTERSENTENCED) == "ON",

                    IsNoBail = s.BailNoBailFlag ?? 0

                    //ListLookup = _commonService.GetLookups(new[] { LookupConstants.CRIMETYPE })
                }).Single();

            _sentenceDetails.SentenceMethod = GetSentenceMethod(_sentenceDetails.ArrestSentenceMethodId ?? 0);

            Inmate inmate = _context.Inmate.Single(s => s.InmateId == _sentenceDetails.InmateId);
            bool inmateActive = inmate.InmateActive == 1;
            int facilityId = inmate.FacilityId;
            if (inmateActive)
            {
                _sentenceDetails.SentenceAltSentDetails = GetAltSentDetails(_sentenceDetails.InmateId);
            }

            _sentenceDetails.IsAltSentFacility =
                _context.Facility.Count(w => w.FacilityId == facilityId && w.AltSentFlag == 1) > 0;
            _sentenceDetails.FacilityId = facilityId;

            return _sentenceDetails;
        }

        private List<PersonnelVm> LoadJudgeDetails(int? personnelId)
        {
            List<int> officerIds =
                _context.Personnel.Where(w => w.PersonnelJudgeFlag).Select(i => i.PersonnelId)
                    .Distinct().ToList();

            List<PersonnelVm> judgeDetails = _personService.GetPersonNameList(officerIds.ToList());

            if (personnelId > 0)
            {
                judgeDetails = judgeDetails.Where(w => w.PersonnelId == personnelId).ToList();
            }

            return judgeDetails;
        }

        public SentenceMethodVm GetSentenceMethod(int sentenceMethodId)
        {
            SentenceMethodVm sentenceMethodVm = _context.ArrestSentenceMethod
                .Where(w => w.ArrestSentenceMethodId == sentenceMethodId)
                .Select(s => new SentenceMethodVm
                {
                    ArrestSentenceMethodId = s.ArrestSentenceMethodId,
                    MethodName = s.MethodName,
                    MethodDescription = s.MethodDescription,

                    GtFixed = s.ArrestSentenceGtFixed,
                    GtDFactor = s.ArrestSentenceGtDFactor,
                    GtDtsFactor = s.ArrestSentenceGtDtsFactor,
                    GtSql = s.ArrestSentenceGtSql,
                    GtDtsPercent = s.ArrestSentenceGtDtsPercent,
                    GtDPercent = s.ArrestSentenceGtDPercent,
                    GtAllowOverride = s.ArrestSentenceGtAllowOverride == 1,
                    GtPostMultiply = s.ArrestSentenceGtPostMultiply,
                    GtTable = s.ArrestSentenceGtTable,
                    GtTableDays = s.ArrestSentenceGtTableDays,
                    GtTabularLookup = _context.ArrestSentenceMethodGtTable
                        .Where(w => w.ArrestSentenceMethodId == s.ArrestSentenceMethodId)
                        .Select(t => new TabularLookupVm
                        {
                            ArrestSentenceMethodId = t.ArrestSentenceMethodId,
                            CreditDays = t.CreditDays,
                            DaysFrom = t.DaysFrom,
                            DaysThru = t.DaysThru
                        }).ToList(),

                    DdaFixed = s.ArrestSentenceDdaFixed,
                    DdaDFactor = s.ArrestSentenceDdaDFactor,
                    DdaDtsFactor = s.ArrestSentenceDdaFactor,
                    DdaSql = s.ArrestSentenceDdaSql,
                    DdaDtsPercent = s.ArrestSentenceDdaDtsPercent,
                    DdaDPercent = s.ArrestSentenceDdaDPercent,
                    DdaAllowOverride = s.ArrestSentenceDdaAllowOverride == 1,
                    DdaSubtractGt = s.ArrestSentenceDdaSubtractGt == 1,

                    WtFixed = s.ArrestSentenceWtFixed,
                    WtDFactor = s.ArrestSentenceWtDFactor,
                    WtDtsFactor = s.ArrestSentenceWtDtsFactor,
                    WtSql = s.ArrestSentenceWtSql,
                    WtDtsPercent = s.ArrestSentenceWtDtsPercent,
                    WtDPercent = s.ArrestSentenceWtDPercent,
                    WtAllowOverride = s.ArrestSentenceWtAllowOverride == 1,
                    WtPostMultiply = s.ArrestSentenceWtPostMultiply,
                    WtTable = s.ArrestSentenceWtTable,
                    WtTableDays = s.ArrestSentenceWtTableDays,
                    WtTabularLookup = _context.ArrestSentenceMethodWtTable
                        .Where(w => w.ArrestSentenceMethodId == s.ArrestSentenceMethodId)
                        .Select(t => new TabularLookupVm
                        {
                            ArrestSentenceMethodId = t.ArrestSentenceMethodId,
                            CreditDays = t.CreditDays,
                            DaysFrom = t.DaysFrom,
                            DaysThru = t.DaysThru
                        }).ToList(),

                    DdFixed = s.ArrestSentenceDdFixed,
                    DdDFactor = s.ArrestSentenceDdDFactor,
                    DdDtsFactor = s.ArrestSentenceDdFactor,
                    DdSql = s.ArrestSentenceDdSql,
                    DdDtsPercent = s.ArrestSentenceDdDtsPercent,
                    DdDPercent = s.ArrestSentenceDdDPercent,
                    DdAllowOverride = s.ArrestSentenceDdAllowOverride == 1,

                    InactiveFlag = s.InactiveFlag == 1,

                    ErcTable = s.ArrestSentenceErctable == 1,
                    ErcTableDays = s.ArrestSentenceErctableDays,
                    ErcTabularLookup = _context.ArrestSentenceMethodErctable
                        .Where(w => w.ArrestSentenceMethodId == s.ArrestSentenceMethodId)
                        .Select(t => new TabularLookupVm
                        {
                            ArrestSentenceMethodId = t.ArrestSentenceMethodId,
                            CreditDays = t.CreditDays,
                            DaysFrom = t.DaysFrom,
                            DaysThru = t.DaysThru
                        }).ToList(),

                    SiteOptionValue = _context.SiteOptions
                        .SingleOrDefault(w => w.SiteOptionsVariable == SiteOptionsConstants.SENTENCEDONOTROUNDCREDITS)
                        .SiteOptionsValue
                }).SingleOrDefault();

            return sentenceMethodVm;
        }

        private SentenceAltSentDetailsVm GetAltSentDetails(int inmateId)
        {
            int incarcerationId = _context.Incarceration
                .Single(w => w.InmateId == inmateId && !w.ReleaseOut.HasValue).IncarcerationId;
            SentenceAltSentDetailsVm sentenceAltSentDetailsVm = new SentenceAltSentDetailsVm();
            if (incarcerationId > 0)
            {
                sentenceAltSentDetailsVm = _context.AltSent
                    .Where(w => w.IncarcerationId == incarcerationId && !w.AltSentThru.HasValue)
                    .Select(s => new SentenceAltSentDetailsVm
                    {
                        AltSentId = s.AltSentId,
                        AltSentProgramId = s.AltSentProgramId,
                        AltSentProgramSentCode = s.AltSentProgram.AltSentProgramSentCode
                    }).SingleOrDefault();
            }

            return sentenceAltSentDetailsVm;
        }

        public async Task<int> ClearArrestSentence(int arrestId)
        {
            Arrest arrest = _context.Arrest.Single(s => s.ArrestId == arrestId);
            arrest.ArrestSentenceConsecutiveFlag = null;
            arrest.ArrestSentenceConsecutiveTo = null;
            arrest.ArrestSentenceGroup = null;
            arrest.ArrestSentenceStartDate = null;
            arrest.ArrestSentenceForthwith = null;
            arrest.ArrestSentenceFlatTime = null;
            arrest.ArrestSentenceUseStartDate = null;
            arrest.ArrestSentenceDaysInterval = null;
            arrest.ArrestSentenceDaysAmount = null;
            arrest.ArrestSentenceDays = null;
            arrest.ArrestSentenceHours = null;
            arrest.ArrestSentenceFineDays = null;
            arrest.ArrestSentenceDaysStayed = null;
            arrest.ArrestSentenceDaysStayedInterval = null;
            arrest.ArrestSentenceDaysStayedAmount = null;
            arrest.ArrestTimeServedDays = null;
            arrest.ArrestSentenceDaysToServe = null;
            arrest.ArrestSentenceExpirationDate = null;
            arrest.ArrestSentenceManual = null;
            arrest.ArrestSentenceGwGtFactor = null;
            arrest.ArrestSentenceGtDays = null;
            arrest.ArrestSentenceGtDaysOverride = null;
            arrest.ArrestSentenceWtDays = null;
            arrest.ArrestSentenceWtDaysOverride = null;
            arrest.ArrestSentenceEarlyRelease = null;
            arrest.ArrestSentenceGwGtAdjust = null;
            arrest.ArrestSentenceActualDaysToServe = null;
            arrest.ArrestSentenceMethodId = null;
            arrest.ArrestSentenceReleaseDate = null;
            arrest.ArrestSentenceErcdays = null;

            return await _context.SaveChangesAsync();
        }

        public async Task<int> UpdateArrestSentence(SentenceDetailsVm value)
        {
            Arrest arrest = _context.Arrest.Single(s => s.ArrestId == value.ArrestId);
            arrest.ArrestSentenceCode = value.ArrestSentenceCode;
            arrest.ArrestSentenceDescription = value.ArrestSentenceDescription;

            arrest.ArrestSentenceIndefiniteHold = value.ArrestSentenceIndefiniteHold ? 1 : 0;
            arrest.ArrestSentenceReleaseDate = value.ArrestSentenceReleaseDateStr;

            arrest.ArrestSentenceAmended = value.ArrestSentenceAmended ? 1 : 0;
            arrest.ArrestSentencePenalInstitution = value.ArrestSentencePenalInstitution ? 1 : 0;
            arrest.ArrestSentenceOptionsRec = value.ArrestSentenceOptionsRec ? 1 : 0;
            arrest.ArrestSentenceAltSentNotAllowed = value.ArrestSentenceAltSentNotAllowed ? 1 : 0;
            arrest.ArrestSentenceNoEarlyRelease = value.ArrestSentenceNoEarlyRelease ? 1 : 0;
            arrest.ArrestSentenceNoLocalParole = value.ArrestSentenceNoLocalParole ? 1 : 0;
            arrest.ArrestSentenceNoDayForDay = value.ArrestSentenceNoDayForDay ? 1 : 0;
            arrest.ArrestSentenceStartDate = value.ArrestSentenceStartDate;
            arrest.ArrestSentenceUseStartDate = value.ArrestSentenceUseStartDateStr;
            arrest.ArrestSentenceActualDaysToServe = value.ArrestSentenceActualDaysToServe;
            arrest.ArrestSentenceWeekender = value.WeekEnder;
            arrest.ArrestSentenceDisciplinaryDaysFlag = 0;

            if (!_sentenceByCharge)
            {
                arrest.ArrestSentenceWeekender = value.ArrestSentenceWeekender ? 1 : 0;
                arrest.ArrestSentenceDateInfo = value.ArrestSentenceDateInfo;
                arrest.ArrestSentenceType = value.ArrestSentenceType;
                arrest.ArrestSentenceFindings = value.ArrestSentenceFindings;
                arrest.ArrestSentenceJudgeId = value.ArrestSentenceJudgeId;
                arrest.ArrestSentenceConsecutiveFlag = value.ArrestSentenceConsecutiveFlag ? 1 : 0;
                arrest.ArrestSentenceConsecutiveTo = value.ArrestSentenceConsecutiveTo;
                arrest.ArrestSentenceDaysInterval = value.ArrestSentenceDaysInterval;
                arrest.ArrestSentenceDays = value.ArrestSentenceDays;
                arrest.ArrestSentenceFineDays = value.ArrestSentenceFineDays;
                arrest.ArrestSentenceDaysStayed = value.ArrestSentenceDaysStayed;
                arrest.ArrestSentenceDaysStayedInterval = value.ArrestSentenceDaysStayedInterval;
                arrest.ArrestSentenceDaysStayedAmount = value.ArrestSentenceDaysStayedAmount;
                arrest.ArrestSentenceForthwith = value.ArrestSentenceForthwith ? 1 : 0;
                arrest.ArrestSentenceFlatTime = value.ArrestSentenceFlatTime;
                arrest.ArrestTimeServedDays = value.ArrestTimeServedDays;
                arrest.ArrestSentenceMethodId = value.ArrestSentenceMethodId;
                arrest.ArrestSentenceDaysAmount = value.ArrestSentenceDaysAmount;
                arrest.ArrestSentenceHours = value.ArrestSentenceHours;
                arrest.ArrestSentenceDaysToServe = value.ArrestSentenceDaysToServe;
                arrest.ArrestSentenceExpirationDate = value.ArrestSentenceExpirationDateStr;
                arrest.ArrestSentenceManual = value.ArrestSentenceManual ? 1 : 0;
                arrest.ArrestSentenceGtDays = value.ArrestSentenceGtDays;
                arrest.ArrestSentenceGtDaysOverride = value.ArrestSentenceGtDaysOverride ? 1 : 0;
                arrest.ArrestSentenceWtDays = value.ArrestSentenceWtDays;
                arrest.ArrestSentenceWtDaysOverride = value.ArrestSentenceWtDaysOverride ? 1 : 0;
                arrest.ArrestSentenceGwGtAdjust = value.ArrestSentenceGwGtAdjust;
                arrest.ArrestSentenceDayForDayAllowed = value.ArrestSentenceDayForDayAllowed;
                arrest.ArrestSentenceDayForDayAllowedOverride = value.ArrestSentenceDayForDayAllowedOverride ? 1 : 0;
                arrest.ArrestSentenceDayForDayDays = value.ArrestSentenceDayForDayDays;
                arrest.ArrestSentenceDayForDayDaysOverride = value.ArrestSentenceDayForDayDaysOverride ? 1 : 0;

                arrest.ArrestSentenceFinePaid = value.SentenceFineDays?.ArrestSentenceFinePaid;
                arrest.ArrestSentenceFineToServe = value.SentenceFineDays?.ArrestSentenceFineToServe;
                arrest.ArrestSentenceFineType = value.SentenceFineDays?.ArrestSentenceFineType;
                arrest.ArrestSentenceFineAmount = value.SentenceFineDays?.ArrestSentenceFineAmount;
                arrest.ArrestSentenceFinePerDay = value.SentenceFineDays?.ArrestSentenceFinePerDay;

                arrest.ArrestSentenceIndefiniteHold = value.ArrestSentenceIndefiniteHold ? 1 : 0;
                arrest.ArrestSentenceDisciplinaryDaysSum = value.ArrestSentenceDisciplinaryDaysSum ?? 0;
                arrest.ArrestSentenceErcdays = value.ArrestSentenceErcDays;
            }

            if (value.ArrestSentenceCode == 0 || value.ArrestSentenceCode == 4)
            {
                List<ArrestSentFlag> arrestSentFlags = _context.ArrestSentFlag.Where(w => w.ArrestId == value.ArrestId).ToList();

                if (arrestSentFlags.Count > 0)
                {
                    arrestSentFlags.ForEach(f =>
                    {
                        f.DeleteFlag = 1;
                        f.DeleteDate = DateTime.Now;
                        f.DeletedBy = _personnelId;
                    });
                }
            }

            ArrestSentenceHistory dbArrestSentenceHistory = new ArrestSentenceHistory
            {
                ArrestId = arrest.ArrestId,
                PersonnelId = _personnelId,
                CreateDate = DateTime.Now,
                ArrestSentenceHistoryList = value.HistoryList
            };
            _context.ArrestSentenceHistory.Add(dbArrestSentenceHistory);
            if (value.IsCalculated)
            {
                ArrestSentenceAttendance arrSentObj = _context.ArrestSentenceAttendance
                    .SingleOrDefault(a => a.ArrestSentenceAttendanceId == value.ArrestSentenceAttendanceId);
                if (!(arrSentObj is null))
                {
                    arrSentObj.ReCalcComplete = 1;
                    arrSentObj.ReCalcBy = _personnelId;
                    arrSentObj.ReCalcDate = DateTime.Now;
                }
                ArrestSentenceAttendanceArrestXref arrSentXrefObj = _context.ArrestSentenceAttendanceArrestXref
                    .FirstOrDefault(a => a.ArrestSentenceAttendanceId == value.ArrestSentenceAttendanceId
                        && (!a.ReCalcComplete.HasValue || a.ReCalcComplete == 0));
                if (!(arrSentXrefObj is null))
                {
                    arrSentXrefObj.ReCalcComplete = 1;
                    arrSentXrefObj.ReCalcBy = _personnelId;
                    arrSentXrefObj.ReCalcDate = DateTime.Now;
                }
                arrest.ArrestSentenceStopWT = value.ArrestSentenceStopWT;
                arrest.ArrestSentenceStopDaysFlag = false;
                List<DisciplinaryWTStop> disciplinaryWTStops=_context.DisciplinaryWTStop
                .Where(w=>w.DisciplinaryInmate.InmateId==arrest.InmateId).ToList();
                disciplinaryWTStops.ForEach(f=>{
                    f.CalculateFlag=false;
                });
            }
            _interfaceEngineService.Export(new ExportRequestVm
            {
                EventName = EventNameConstants.SENTENCE,
                PersonnelId = _personnelId,
                Param1 = _context.Inmate.SingleOrDefault(x => x.InmateId == arrest.InmateId)?
                .PersonId.ToString(),
                Param2 = arrest.ArrestId.ToString()
            });
            return await _context.SaveChangesAsync();
        }

        public List<DisciplinaryDays> GetDisciplinaryDays(int arrestId)
        {
            IQueryable<Lookup> lookuplst = _context.Lookup.Where(w => w.LookupType == LookupConstants.DISCTYPE);

            int[] disciplinaryInmateId =
                _context.DisciplinarySentDayXref.Where(x => x.ArrestId == arrestId)
                    .Select(x => x.DisciplinaryInmateId ?? 0).ToArray();

            List<DisciplinaryDays> disciplinaryDays =
                _context.DisciplinaryInmate.Where(a => disciplinaryInmateId.Contains(a.DisciplinaryInmateId)).Select(
                    a =>
                        new DisciplinaryDays
                        {
                            IncidentNumber = a.DisciplinaryIncident.DisciplinaryNumber,
                            IncidentDate = a.DisciplinaryIncident.DisciplinaryIncidentDate,
                            IncidentType = lookuplst
                                .SingleOrDefault(w => w.LookupIndex == a.DisciplinaryIncident.DisciplinaryType)
                                .LookupDescription,
                            DiscDays = a.DisciplinaryDays
                        }).ToList();

            return disciplinaryDays;
        }

        public async Task<int> UpdateArrestSentenceGap(List<SentenceDetailsVm> value)
        {
            value.ForEach(item =>
            {
                Arrest arrest = _context.Arrest.Single(s => s.ArrestId == item.ArrestId);

                arrest.ArrestSentenceUseStartDate = item.SentenceGapFound.NewUseStartDate;
                arrest.ArrestSentenceExpirationDate = item.SentenceGapFound.NewUseExpirationDate;
                arrest.ArrestSentenceReleaseDate = item.SentenceGapFound.NewUseReleaseClearDate;

            });
            return await _context.SaveChangesAsync();
        }

        public async Task<int> UpdateCrimeSentenceGap(List<ChargeSentenceVm> value)
        {
            value.ForEach(item =>
            {
                Crime crime = _context.Crime.Single(s => s.CrimeId == item.CrimeId);

                crime.ArrestSentenceUseStartDate = item.SentenceGapFound.NewUseStartDate;
                crime.ArrestSentenceExpirationDate = item.SentenceGapFound.NewUseExpirationDate;
                crime.ArrestSentenceReleaseDate = item.SentenceGapFound.NewUseReleaseClearDate;
            });

            return await _context.SaveChangesAsync();
        }

        #region CrimeSentence

        public SentenceDetailsVm GetSentenceDetailsCrime(int crimeId)
        {
            IQueryable<Lookup> lookuplst = _context.Lookup
                .Where(w => w.LookupType == LookupConstants.ARRTYPE || w.LookupType == LookupConstants.CRIMETYPE ||
                            w.LookupType == LookupConstants.CHARGEQUALIFIER ||
                            w.LookupType == LookupConstants.SENTFLAG ||
                            w.LookupType == LookupConstants.SENTTYPE || w.LookupType == LookupConstants.SENTFIND ||
                            w.LookupType == LookupConstants.SENTDURATION ||
                            w.LookupType == LookupConstants.SENTFINETYPE);

            IQueryable<ArrestSentenceMethod> sentenceMethodLst = _context.ArrestSentenceMethod;

            //List<LookupVm> crimeTypeLookups = _commonService.GetLookups(new[] { LookupConstants.CRIMETYPE });

            _sentenceDetails = _context.Crime.Where(w => w.CrimeId == crimeId).Select(s => new SentenceDetailsVm
            {
                SentenceCharge = new SentenceChargeVm
                {
                    CrimeId = s.CrimeId,
                    CrimeNumber = s.CrimeNumber ?? 0, //Sequence
                    CrimeCount = s.CrimeCount,
                    Qualifier = lookuplst.SingleOrDefault(w => w.LookupType == LookupConstants.CHARGEQUALIFIER &&
                        w.LookupIndex == Convert.ToInt32(s.ChargeQualifierLookup)).LookupDescription,
                    CrimeStatus = lookuplst.SingleOrDefault(w =>
                            w.LookupType == LookupConstants.CRIMETYPE && w.LookupIndex == Convert.ToInt32(s.CrimeType))
                        .LookupDescription,
                    CrimeStatusId = lookuplst.SingleOrDefault(w =>
                            w.LookupType == LookupConstants.CRIMETYPE && w.LookupIndex == Convert.ToInt32(s.CrimeType))
                        .LookupIndex,
                    CrimeStatusList = lookuplst.Where(w => w.LookupType == LookupConstants.CRIMETYPE).OrderByDescending
                            (o => o.LookupOrder).ThenBy(o => o.LookupDescription).Select
                            (l => new KeyValuePair<int, string>(l.LookupIndex, l.LookupDescription))
                        .ToList(),
                    CrimeLookupId = s.CrimeLookupId
                },

                ArrestBookingNo = s.Arrest.ArrestBookingNo,
                ArrestDate = s.Arrest.ArrestDate,
                Type = lookuplst.SingleOrDefault(w => w.LookupIndex == Convert.ToInt32(s.Arrest.ArrestType)
                                                      && w.LookupType == LookupConstants.ARRTYPE).LookupDescription,
                ArrestCourtDocket = s.Arrest.ArrestCourtDocket,
                ArrestSentenceConsecutiveFlag = s.ArrestSentenceConsecutiveFlag == 1,
                ArrestSentenceConsecutiveTo = s.ArrestSentenceConsecutiveTo,
                ArrestSentenceStartDate = s.ArrestSentenceStartDate,
                ArrestSentenceUseStartDate = s.ArrestSentenceUseStartDate,
                ArrestSentenceDays = s.ArrestSentenceDays,
                ArrestSentenceDaysToServe = s.ArrestSentenceDaysToServe,
                ArrestSentenceActualDaysToServe = s.ArrestSentenceActualDaysToServe,
                ArrestSentenceCode = s.Arrest.ArrestSentenceCode ?? 0,
                ArrestSentenceDescription = s.Arrest.ArrestSentenceDescription,
                ArrestId = s.ArrestId,
                WeekEnder = s.Arrest.ArrestSentenceWeekender,
                ArrestSentenceAmended = s.Arrest.ArrestSentenceAmended == 1,
                ArrestSentencePenalInstitution = s.Arrest.ArrestSentencePenalInstitution == 1,
                ArrestSentenceOptionsRec = s.Arrest.ArrestSentenceOptionsRec == 1,
                ArrestSentenceAltSentNotAllowed = s.Arrest.ArrestSentenceAltSentNotAllowed == 1,
                ArrestSentenceNoEarlyRelease = s.Arrest.ArrestSentenceNoEarlyRelease == 1,
                ArrestSentenceNoLocalParole = s.Arrest.ArrestSentenceNoLocalParole == 1,
                ArrestSentenceNoDayForDay = s.Arrest.ArrestSentenceNoDayForDay == 1,
                ArrestSentenceWeekender = s.Arrest.ArrestSentenceWeekender == 1,
                AdditionalFlagsList = _context.ArrestSentFlag
                    .Where(w => w.ArrestId == s.ArrestId && (!w.DeleteFlag.HasValue || w.DeleteFlag == 0))
                    .Select(x => new SentenceAdditionalFlagsVm
                    {
                        ArrestSentFlagId = x.ArrestSentflagId,
                        ArrestId = x.ArrestId,
                        LookupDescription = lookuplst.SingleOrDefault(w => w.LookupIndex == x.SentflagLookupIndex
                                                                           && w.LookupType == LookupConstants.SENTFLAG)
                            .LookupDescription,
                        LookupIndex = x.SentflagLookupIndex,
                        Selected = true
                    }).ToList(),
                ArrestSentenceDateInfo = s.ArrestSentenceDateInfo,
                ArrestSentenceType = s.ArrestSentenceType,
                ArrestSentenceTypeList = lookuplst.Where(w => w.LookupType == LookupConstants.SENTTYPE).Select
                    (l => new KeyValuePair<int, string>(l.LookupIndex, l.LookupDescription)).ToList(),
                ArrestSentenceFindings = s.ArrestSentenceFindings,
                ArrestSentenceFindingsList = lookuplst.Where(w => w.LookupType == LookupConstants.SENTFIND).Select
                    (l => new KeyValuePair<int, string>(l.LookupIndex, l.LookupDescription)).ToList(),
                ArrestSentenceJudgeId = s.ArrestSentenceJudgeId,
                ArrestSentenceJudgeList = LoadJudgeDetails(null),
                ArrestSentenceDaysInterval = s.ArrestSentenceDaysInterval,
                ArrestSentenceDaysIntervalList = lookuplst.Where(w => w.LookupType == LookupConstants.SENTDURATION)
                    .Select
                    (l => new KeyValuePair<int, string>(NullableTryParseInt32(l.LookupMap),
                        l.LookupDescription))
                    .OrderBy(o => o.Value).ToList(),
                ArrestSentenceFineDays = s.ArrestSentenceFineDays,
                ArrestSentenceDaysStayed = s.ArrestSentenceDaysStayed,
                ArrestSentenceDaysStayedInterval = s.ArrestSentenceDaysStayedInterval,
                ArrestSentenceDaysStayedAmount = s.ArrestSentenceDaysStayedAmount,
                ArrestTimeServedDays = s.ArrestTimeServedDays,
                ArrestSentenceForthwith = s.ArrestSentenceForthwith == 1,
                ArrestSentenceFlatTime = s.ArrestSentenceFlatTime,
                ArrestSentenceManual = s.ArrestSentenceManual == 1,
                ArrestSentenceMethodId = s.ArrestSentenceMethodId,
                ArrestSentenceMethodList =
                    sentenceMethodLst.Where(w => w.InactiveFlag != 1).Select
                        (a => new KeyValuePair<int, string>(a.ArrestSentenceMethodId,
                            a.MethodName))
                        .ToList(),
                DefaultSentenceMethodId = sentenceMethodLst.FirstOrDefault(f => f.DefaultManualSent).ArrestSentenceMethodId,
                ArrestSentenceDaysAmount = s.ArrestSentenceDaysAmount,
                ArrestSentenceHours = s.ArrestSentenceHours ?? 0,
                ArrestSentenceExpirationDate = s.ArrestSentenceExpirationDate,
                ArrestSentenceDayForDayAllowedOverride = s.ArrestSentenceDayForDayAllowedOverride == 1,
                ArrestSentenceGtDaysOverride = s.ArrestSentenceGtDaysOverride == 1,
                ArrestSentenceWtDaysOverride = s.ArrestSentenceWtDaysOverride == 1,
                ArrestSentenceErcDays = s.ArrestSentenceErcdays,
                ArrestSentenceDayForDayDaysOverride = s.ArrestSentenceDayForDayDaysOverride == 1,
                ArrestSentenceDayForDayAllowed = s.ArrestSentenceDayForDayAllowed,
                ArrestSentenceGtDays = s.ArrestSentenceGtDays ?? 0,
                ArrestSentenceWtDays = s.ArrestSentenceWtDays ?? 0,
                ArrestSentenceGwGtAdjust = s.ArrestSentenceGwGtAdjust ?? 0,
                ArrestSentenceDayForDayDays = s.ArrestSentenceDayForDayDays,
                ArrestSentenceDisciplinaryDaysSum = s.ArrestSentenceDisciplinaryDaysSum ?? 0,
                ArrestSentenceReleaseDate = s.ArrestSentenceReleaseDate,
                SentenceFineDays = new SentenceFineDaysVm
                {
                    ArrestSentenceFinePaid = s.ArrestSentenceFinePaid,
                    ArrestSentenceFineToServe = s.ArrestSentenceFineToServe,
                    ArrestSentenceFineType = s.ArrestSentenceFineType,
                    ArrestSentenceFineAmount = s.ArrestSentenceFineAmount,
                    ArrestSentenceFinePerDay = s.ArrestSentenceFinePerDay,
                    ArrestSentenceFineTypeList = lookuplst.Where(a => a.LookupType == LookupConstants.SENTFINETYPE)
                        .Select(a => new LookupVm
                        {
                            LookupIndex = a.LookupIndex,
                            LookupName = a.LookupName,
                            LookupDescription = a.LookupDescription,
                            LookupCategory = a.LookupCategory
                        }).ToList()
                },
                NoDayForDayVisible = _commonService.GetSiteOptionValue(SiteOptionsConstants.NODAYFORDAYVISIBLE) == "ON",
                SameDayUseStart = _commonService.GetSiteOptionValue(SiteOptionsConstants.CONSECUTIVESENTENCESAMEDAYUSESTART) == "ON",
                ListLookup = _commonService.GetLookups(new[] { LookupConstants.CRIMETYPE },false)
            }).Single();

            CrimeLookup crimeLookups =
                _context.CrimeLookup.Single(w => w.CrimeLookupId == _sentenceDetails.SentenceCharge.CrimeLookupId);
            _sentenceDetails.SentenceCharge.CrimeCodeType = crimeLookups.CrimeCodeType;
            _sentenceDetails.SentenceCharge.CrimeDescription = crimeLookups.CrimeDescription;
            _sentenceDetails.SentenceCharge.CrimeStatute = crimeLookups.CrimeStatuteCode;
            _sentenceDetails.SentenceMethod = GetSentenceMethod(_sentenceDetails.ArrestSentenceMethodId ?? 0);

            return _sentenceDetails;
        }

        public ChargeSentenceViewerVm GetChargeSentenceViewerList(int arrestId, int incarcerationId)
        {
            ChargeSentenceViewerVm chargeSentenceViewer = new ChargeSentenceViewerVm
            {
                //To get Arrest Booking Status
                SentenceDetails = GetSentenceDetailsArrest(arrestId),

                //To get Sentence Details
                ChargeSentence = GetAllArrestSentenceDetailsCrime(incarcerationId),
            };
            return chargeSentenceViewer;
        }

        public List<ChargeSentenceVm> GetAllArrestSentenceDetailsCrime(int incarcerationId)
        {
            List<ChargeSentenceVm> lstCrimeSentence = new List<ChargeSentenceVm>();
            if (!_sentenceByCharge)
            {
                return lstCrimeSentence;
            }

            int[] arrestIds =
                _context.IncarcerationArrestXref.Where(x => x.IncarcerationId == incarcerationId)
                    .Select(x => x.ArrestId ?? 0).ToArray();

            IQueryable<Lookup> lookuplst = _context.Lookup
                .Where(w => w.LookupType == LookupConstants.CHARGEQUALIFIER ||
                            w.LookupType == LookupConstants.CRIMETYPE);

            IQueryable<ArrestSentenceMethod> arrestSentenceMethodList = _context.ArrestSentenceMethod;

            lstCrimeSentence = _context.Crime.Where(w => arrestIds.Contains(w.ArrestId ?? 0) && w.CrimeDeleteFlag == 0)
                .Select(c => new ChargeSentenceVm
                {
                    ArrestId = c.ArrestId,
                    ArrestBookingNo = c.Arrest.ArrestBookingNo,
                    ArrestSentenceCode = c.Arrest.ArrestSentenceCode,
                    CrimeId = c.CrimeId,
                    CrimeNumber = c.CrimeNumber,
                    CrimeCount = c.CrimeCount,
                    CreateDate = c.CreateDate,
                    CrimeLookupId = c.CrimeLookupId,
                    Qualifier =
                        lookuplst.SingleOrDefault(w => w.LookupIndex == Convert.ToInt32(c.ChargeQualifierLookup)
                                     && w.LookupType == LookupConstants.CHARGEQUALIFIER).LookupDescription,
                    CrimeStatus = lookuplst.SingleOrDefault(w => w.LookupIndex == Convert.ToInt32(c.CrimeType)
                         && w.LookupType == LookupConstants.CRIMETYPE).LookupDescription,
                    CrimeStatusId = Convert.ToInt32(c.CrimeType),
                    ArrestSentenceStartDate = c.ArrestSentenceStartDate,
                    ArrestSentenceConsecutiveTo = c.ArrestSentenceConsecutiveTo,
                    ArrestSentenceUseStartDate = c.ArrestSentenceUseStartDate,
                    ArrestSentenceDays = c.ArrestSentenceDays,
                    MethodName = arrestSentenceMethodList.SingleOrDefault(
                                w => w.ArrestSentenceMethodId == c.ArrestSentenceMethodId).MethodName,
                    ArrestSentenceDaysToServe = c.ArrestSentenceDaysToServe,
                    ArrestSentenceActualDaysToServe = c.ArrestSentenceActualDaysToServe,
                    ArrestSentenceReleaseDate = c.ArrestSentenceReleaseDate,
                    ArrestSentenceDateInfo = c.ArrestSentenceDateInfo,
                    ArrestSentenceType = c.ArrestSentenceType,
                    ArrestSentenceFindings = c.ArrestSentenceFindings,
                    ArrestSentenceJudgeId = c.ArrestSentenceJudgeId,
                    ArrestSentenceDaysInterval = c.ArrestSentenceDaysInterval,
                    ArrestSentenceFineDays = c.ArrestSentenceFineDays,
                    ArrestSentenceDaysStayed = c.ArrestSentenceDaysStayed,
                    ArrestSentenceDaysStayedInterval = c.ArrestSentenceDaysStayedInterval,
                    ArrestSentenceDaysStayedAmount = c.ArrestSentenceDaysStayedAmount,
                    ArrestTimeServedDays = c.ArrestTimeServedDays,
                    ArrestSentenceForthwith = c.ArrestSentenceForthwith == 1,
                    ArrestSentenceFlatTime = c.ArrestSentenceFlatTime,
                    ArrestSentenceManual = c.ArrestSentenceManual == 1,
                    ArrestSentenceMethodId = c.ArrestSentenceMethodId,
                    ArrestSentenceDaysAmount = c.ArrestSentenceDaysAmount,
                    ArrestSentenceHours = c.ArrestSentenceHours,
                    ArrestSentenceExpirationDate = c.ArrestSentenceExpirationDate,
                    ArrestSentenceDayForDayAllowedOverride = c.ArrestSentenceDayForDayAllowedOverride == 1,
                    ArrestSentenceGtDaysOverride = c.ArrestSentenceGtDaysOverride == 1,
                    ArrestSentenceWtDaysOverride = c.ArrestSentenceWtDaysOverride == 1,
                    ArrestSentenceErcDays = c.ArrestSentenceErcdays,
                    ArrestSentenceDayForDayDaysOverride = c.ArrestSentenceDayForDayDaysOverride == 1,
                    ArrestSentenceDayForDayAllowed = c.ArrestSentenceDayForDayAllowed,
                    ArrestSentenceGtDays = c.ArrestSentenceGtDays,
                    ArrestSentenceWtDays = c.ArrestSentenceWtDays,
                    ArrestSentenceGwGtAdjust = c.ArrestSentenceGwGtAdjust,
                    ArrestSentenceDayForDayDays = c.ArrestSentenceDayForDayDays,
                    ArrestSentenceDisciplinaryDaysSum = c.ArrestSentenceDisciplinaryDaysSum ?? 0,
                    ArrestClearScheduleDate = c.Arrest.ArrestSentenceReleaseDate,
                    SentenceGapFound = new SentenceGapFound
                    {
                        ProcessFlag = false
                    },
                    SentenceCharge = new SentenceChargeVm
                    {
                        CrimeId = c.CrimeId,
                        CrimeStatusId = lookuplst.SingleOrDefault(w =>
                                w.LookupType == LookupConstants.CRIMETYPE &&
                                w.LookupIndex == Convert.ToInt32(c.CrimeType)).LookupIndex,
                    },
                    SentenceFineDays = new SentenceFineDaysVm
                    {
                        ArrestSentenceFinePaid = c.ArrestSentenceFinePaid,
                        ArrestSentenceFineToServe = c.ArrestSentenceFineToServe,
                        ArrestSentenceFineType = c.ArrestSentenceFineType,
                        ArrestSentenceFineAmount = c.ArrestSentenceFineAmount,
                        ArrestSentenceFinePerDay = c.ArrestSentenceFinePerDay
                    },
                }).ToList();

            int[] crimeLookupId = lstCrimeSentence.Select(x => x.CrimeLookupId ?? 0).ToArray();
            IQueryable<CrimeLookup> crimeLookuplst = _context.CrimeLookup
                .Where(w => crimeLookupId.Contains(w.CrimeLookupId));

            lstCrimeSentence.ForEach(item =>
            {
                item.CrimeCodeType =
                    crimeLookuplst.SingleOrDefault(w => w.CrimeLookupId == item.CrimeLookupId)
                        ?.CrimeCodeType;
                item.CrimeSection =
                    crimeLookuplst.SingleOrDefault(w => w.CrimeLookupId == item.CrimeLookupId)
                        ?.CrimeSection;
                item.CrimeDescription =
                    crimeLookuplst.SingleOrDefault(w => w.CrimeLookupId == item.CrimeLookupId)
                        ?.CrimeDescription;
                item.CrimeStatute =
                    crimeLookuplst.SingleOrDefault(w => w.CrimeLookupId == item.CrimeLookupId)
                        ?.CrimeStatuteCode;
            });

            return lstCrimeSentence.OrderBy(o => o.CrimeNumber).ThenBy(o => o.CreateDate).ToList();
        }

        public async Task<int> ClearCrimeSentence(int crimeId)
        {
            Crime crime = _context.Crime.Single(s => s.CrimeId == crimeId);
            crime.ArrestSentenceConsecutiveFlag = null;
            crime.ArrestSentenceConsecutiveTo = null;
            crime.ArrestSentenceStartDate = null;
            crime.ArrestSentenceForthwith = null;
            crime.ArrestSentenceFlatTime = null;
            crime.ArrestSentenceUseStartDate = null;
            crime.ArrestSentenceDaysInterval = null;
            crime.ArrestSentenceDaysAmount = null;
            crime.ArrestSentenceDays = null;
            crime.ArrestSentenceHours = null;
            crime.ArrestSentenceFineDays = null;
            crime.ArrestSentenceDaysStayed = null;
            crime.ArrestSentenceDaysStayedInterval = null;
            crime.ArrestSentenceDaysStayedAmount = null;
            crime.ArrestTimeServedDays = null;
            crime.ArrestSentenceDaysToServe = null;
            crime.ArrestSentenceExpirationDate = null;
            crime.ArrestSentenceManual = null;
            crime.ArrestSentenceGtDays = null;
            crime.ArrestSentenceGtDaysOverride = null;
            crime.ArrestSentenceWtDays = null;
            crime.ArrestSentenceWtDaysOverride = null;
            crime.ArrestSentenceGwGtAdjust = null;
            crime.ArrestSentenceActualDaysToServe = null;
            crime.ArrestSentenceMethodId = null;
            crime.ArrestSentenceReleaseDate = null;
            crime.ArrestSentenceErcdays = null;

            return await _context.SaveChangesAsync();
        }

        public async Task<int> UpdateChargeSentence(SentenceDetailsVm value)
        {
            Crime crime = _context.Crime.Single(s => s.CrimeId == value.SentenceCharge.CrimeId);

            crime.CrimeType = Convert.ToString(value.SentenceCharge.CrimeStatusId);
            crime.ArrestSentenceDateInfo = value.ArrestSentenceDateInfo;
            crime.ArrestSentenceType = value.ArrestSentenceType;
            crime.ArrestSentenceFindings = value.ArrestSentenceFindings;
            crime.ArrestSentenceJudgeId = value.ArrestSentenceJudgeId;
            crime.ArrestSentenceConsecutiveFlag = value.ArrestSentenceConsecutiveFlag ? 1 : 0;
            crime.ArrestSentenceConsecutiveTo = value.ArrestSentenceConsecutiveTo;
            crime.ArrestSentenceStartDate = value.ArrestSentenceStartDate;
            crime.ArrestSentenceDaysInterval = value.ArrestSentenceDaysInterval;
            crime.ArrestSentenceDays = value.ArrestSentenceDays;
            crime.ArrestSentenceDaysStayed = value.ArrestSentenceDaysStayed;
            crime.ArrestSentenceDaysStayedInterval = value.ArrestSentenceDaysStayedInterval;
            crime.ArrestSentenceDaysStayedAmount = value.ArrestSentenceDaysStayedAmount;
            crime.ArrestSentenceFineDays = value.ArrestSentenceFineDays;
            crime.ArrestSentenceForthwith = value.ArrestSentenceForthwith ? 1 : 0;
            crime.ArrestSentenceFlatTime = value.ArrestSentenceFlatTime;
            crime.ArrestTimeServedDays = value.ArrestTimeServedDays;
            crime.ArrestSentenceMethodId = value.ArrestSentenceMethodId;
            crime.ArrestSentenceUseStartDate = value.ArrestSentenceUseStartDateStr;
            crime.ArrestSentenceDaysAmount = value.ArrestSentenceDaysAmount;
            crime.ArrestSentenceHours = value.ArrestSentenceHours;
            crime.ArrestSentenceDaysToServe = value.ArrestSentenceDaysToServe;
            crime.ArrestSentenceExpirationDate = value.ArrestSentenceExpirationDateStr;
            crime.ArrestSentenceManual = value.ArrestSentenceManual ? 1 : 0;
            crime.ArrestSentenceGtDays = value.ArrestSentenceGtDays;
            crime.ArrestSentenceGtDaysOverride = value.ArrestSentenceGtDaysOverride ? 1 : 0;
            crime.ArrestSentenceWtDays = value.ArrestSentenceWtDays;
            crime.ArrestSentenceWtDaysOverride = value.ArrestSentenceWtDaysOverride ? 1 : 0;
            crime.ArrestSentenceGwGtAdjust = value.ArrestSentenceGwGtAdjust;
            crime.ArrestSentenceDayForDayAllowed = value.ArrestSentenceDayForDayAllowed;
            crime.ArrestSentenceDayForDayAllowedOverride = value.ArrestSentenceDayForDayAllowedOverride ? 1 : 0;
            crime.ArrestSentenceDayForDayDays = value.ArrestSentenceDayForDayDays;
            crime.ArrestSentenceDayForDayDaysOverride = value.ArrestSentenceDayForDayDaysOverride ? 1 : 0;
            crime.ArrestSentenceActualDaysToServe = value.ArrestSentenceActualDaysToServe;
            crime.ArrestSentenceReleaseDate = value.ArrestSentenceReleaseDateStr;

            crime.ArrestSentenceFinePaid = value.SentenceFineDays.ArrestSentenceFinePaid;
            crime.ArrestSentenceFineToServe = value.SentenceFineDays.ArrestSentenceFineToServe;
            crime.ArrestSentenceFineType = value.SentenceFineDays.ArrestSentenceFineType;
            crime.ArrestSentenceFineAmount = value.SentenceFineDays.ArrestSentenceFineAmount;
            crime.ArrestSentenceFinePerDay = value.SentenceFineDays.ArrestSentenceFinePerDay;

            crime.ArrestSentenceDisciplinaryDaysSum = value.ArrestSentenceDisciplinaryDaysSum;
            crime.ArrestSentenceErcdays = value.ArrestSentenceErcDays;

            CrimeSentenceHistory dbCrimeSentenceHistory = new CrimeSentenceHistory
            {
                CrimeId = crime.CrimeId,
                PersonnelId = _personnelId,
                CreateDate = DateTime.Now,
                CrimeSentenceHistoryList = value.HistoryList
            };
            _context.CrimeSentenceHistory.Add(dbCrimeSentenceHistory);
            _interfaceEngineService.Export(new ExportRequestVm
            {
                EventName = EventNameConstants.SENTENCE,
                PersonnelId = _personnelId,
                Param1 = _context.Inmate.SingleOrDefault(i => i.InmateId == value.InmateId)?
                .PersonId.ToString(),
                Param2 = crime.ArrestId?.ToString()
            });
            return await _context.SaveChangesAsync();
        }

        public async Task<int> UpdateChargeSentenceList(SentenceDetailsVm value, List<int> crimeIds)
        {
            foreach (int crimeId in crimeIds)
            {
                value.SentenceCharge.CrimeId = crimeId;
                await UpdateChargeSentence(value);
            }

            return await _context.SaveChangesAsync();
        }

        #endregion

        #region Overall

        public async Task<int> UpdateOverallSentence(OverallSentenceVm value)
        {
            Incarceration incarceration =
                _context.Incarceration.Single(s => s.IncarcerationId == value.IncarcerationId);

            incarceration.OverallSentManual = value.Manual ? 1 : 0;
            if (value.Manual)
            {
                incarceration.OverallSentManualDate = DateTime.Now;
                incarceration.OverallSentManualBy = _personnelId;
            }

            incarceration.ActualDaysToServe = value.ActualDaysToServe;
            incarceration.OverallFinalReleaseDate = value.OverallFinalReleaseDate;
            incarceration.OverallSentStartDate = value.OverallSentStartDate;
            incarceration.OverallsentErc = value.Erc;
            incarceration.OverallsentErcclear = value.ErcClear ? 1 : 0;
            incarceration.TotSentDays = value.TotalSentDays;

            IncarcerationSentSaveHistory incarcerationSentSaveHistory = new IncarcerationSentSaveHistory();
            {
                incarcerationSentSaveHistory.IncarcerationId = value.IncarcerationId;
                incarcerationSentSaveHistory.OverallFinalReleaseDate = value.OverallFinalReleaseDate;
                incarcerationSentSaveHistory.OverallSentManual = value.Manual ? 1 : 0;
                incarcerationSentSaveHistory.OverallSentStartDate = value.OverallSentStartDate;
                incarcerationSentSaveHistory.SaveBy = _personnelId;
                incarcerationSentSaveHistory.SaveDate = DateTime.Now;
                incarcerationSentSaveHistory.TotSentDays = value.ActualDaysToServe;
                incarcerationSentSaveHistory.OverallsentErc = value.Erc ?? 0;
                incarcerationSentSaveHistory.OverallsentErcclear = value.ErcClear ? 1 : 0;
            }
            _context.Add(incarcerationSentSaveHistory);
            _interfaceEngineService.Export(new ExportRequestVm
            {
                EventName = EventNameConstants.OVERALLSENTENCE,
                PersonnelId = _personnelId,
                Param1 = _context.Inmate.SingleOrDefault(i => i.InmateId == value.InmateId)?
                .PersonId.ToString(),
                Param2 = incarceration.IncarcerationId.ToString()
            });
            return await _context.SaveChangesAsync();
        }

        public List<OverallSentenceVm> GetOverallIncarcerationHistory(int incarcerationId)
        {
            List<OverallSentenceVm> overallSentenceHistory =
                _context.IncarcerationSentSaveHistory.Where(a => a.IncarcerationId == incarcerationId)
                    .OrderByDescending(o => o.SaveDate).Select(a => new OverallSentenceVm
                    {
                        SavedDate = a.SaveDate,
                        SavedBy = a.SaveBy,
                        Manual = a.OverallSentManual == 1,
                        OverallSentStartDate = a.OverallSentStartDate,
                        OverallFinalReleaseDate = a.OverallFinalReleaseDate,
                        ActualDaysToServe = a.TotSentDays,
                        Erc = a.OverallsentErc,
                        ErcClear = a.OverallsentErcclear == 1,
                    }).ToList();

            List<int> lstPersonnelIds = overallSentenceHistory.Where(a => a.SavedBy.HasValue)
                .Select(a => a.SavedBy.Value)
                .ToList();

            List<PersonnelVm> lstPersonDetails = _personService.GetPersonNameList(lstPersonnelIds);

            overallSentenceHistory.ForEach(item =>
            {
                PersonnelVm personInfo = lstPersonDetails
                    .SingleOrDefault(a => a.PersonnelId == item.SavedBy);
                if (personInfo != null)
                {
                    item.SavedPersonFirstName = personInfo.PersonFirstName;
                    item.SavedPersonLastName = personInfo.PersonLastName;
                    item.SavedPersonMiddleName = personInfo.PersonMiddleName;
                    item.OfficerBadgeNumber = personInfo.OfficerBadgeNumber;
                }
            });

            return overallSentenceHistory.ToList();
        }

        public List<HistoryVm> GetArrestSentenceHistory(int arrestId, int crimeId)
        {
            List<HistoryVm> sentenceHistory = crimeId > 0 ? _context.CrimeSentenceHistory
                .Where(w => w.CrimeId == crimeId)
                .OrderByDescending(ph => ph.CreateDate)
                .Select(ph => new HistoryVm
                {
                    HistoryId = ph.CrimeId,
                    CreateDate = ph.CreateDate,
                    PersonId = ph.Personnel.PersonId,
                    OfficerBadgeNumber = ph.Personnel.OfficerBadgeNumber,
                    HistoryList = ph.CrimeSentenceHistoryList
                }).ToList()
                : _context.ArrestSentenceHistory
                .Where(w => w.ArrestId == arrestId)
                .OrderByDescending(ph => ph.CreateDate)
                .Select(ph => new HistoryVm
                {
                    HistoryId = ph.ArrestId,
                    CreateDate = ph.CreateDate,
                    PersonId = ph.Personnel.PersonId,
                    OfficerBadgeNumber = ph.Personnel.OfficerBadgeNumber,
                    HistoryList = ph.ArrestSentenceHistoryList
                }).ToList();
            if (sentenceHistory.Count <= 0) return sentenceHistory;

            //To Improve Performence All Person Details Loaded By Single Hit Before Looping
            int[] personIds = sentenceHistory.Select(x => x.PersonId).ToArray();
            //get person list
            List<Person> lstPersonDet = _context.Person.Where(per => personIds.Contains(per.PersonId)).ToList();

            sentenceHistory.ForEach(item =>
            {
                item.PersonLastName = lstPersonDet.SingleOrDefault(p => p.PersonId == item.PersonId)?.PersonLastName;
                //To GetJson Result Into Dictionary
                if (item.HistoryList == null) return;
                Dictionary<string, string> personHistoryList =
                    JsonConvert.DeserializeObject<Dictionary<string, string>>(item.HistoryList);
                item.Header =
                    personHistoryList.Select(ph => new PersonHeader { Header = ph.Key, Detail = ph.Value })
                        .ToList();
            });
            return sentenceHistory;
        }

        #endregion

        #endregion

        private List<BookingStatusVm> GetBookingStatusPdf(int incarcerationId, int arrestId)
        {
            List<BookingStatusVm> bookingStatus = new List<BookingStatusVm>();

            if (incarcerationId == 0)
            {
                return bookingStatus;
            }

            IQueryable<Lookup> lookups = _context.Lookup
                .Where(w => (w.LookupType == LookupConstants.BOOKSTAT || w.LookupType == LookupConstants.ARRTYPE));

            bookingStatus = _context.IncarcerationArrestXref.Where(a => a.IncarcerationId == incarcerationId).Select(
                a => new BookingStatusVm
                {
                    ArrestId = a.Arrest.ArrestId,
                    InmateId = a.Arrest.InmateId,
                    ArrestBookingStatus = lookups.SingleOrDefault(w => w.LookupIndex == a.Arrest.ArrestBookingStatus &&
                                 w.LookupType == LookupConstants.BOOKSTAT).LookupDescription,
                    ArrestBookingStatusId = a.Arrest.ArrestBookingStatus,
                    ArrestBookingStatusList =
                            lookups.Where(w => w.LookupType == LookupConstants.BOOKSTAT)
                                .OrderByDescending(o => o.LookupOrder).ThenBy(o => o.LookupDescription)
                                .Select(t =>
                                    new KeyValuePair<int, string>(t.LookupIndex, t.LookupDescription))
                                .ToList(),
                    ArrestBookingAbbr = lookups.SingleOrDefault(
                                w => w.LookupIndex == a.Arrest.ArrestBookingStatus &&
                                     w.LookupType == LookupConstants.BOOKSTAT)
                            .LookupName,
                    ArrestConvictionDate = a.Arrest.ArrestConvictionDate,
                    ArrestConvictionNote = a.Arrest.ArrestConvictionNote,
                    ArrestBookingNumber = a.Arrest.ArrestBookingNo,
                    ArrestBookingType = lookups.SingleOrDefault(
                                w => w.LookupIndex == Convert.ToInt32(a.Arrest.ArrestType) &&
                                     w.LookupType == LookupConstants.ARRTYPE)
                            .LookupDescription,
                    CourtDocket = a.Arrest.ArrestCourtDocket,
                    Court = _context.Agency
                            .SingleOrDefault(w => w.AgencyId == a.Arrest.ArrestCourtJurisdictionId)
                            .AgencyAbbreviation
                }).ToList();

            if (arrestId > 0)
            {
                bookingStatus = bookingStatus.Where(a => a.ArrestId == arrestId).ToList();
            }

            return bookingStatus;
        }

        private List<SentenceDetailsVm> GetSentenceDetailsPdf(int incarcerationId, int arrestId,
            SentenceSummaryType reportType)
        {
            IQueryable<Lookup> lookuplst = _context.Lookup
                .Where(w => w.LookupType == LookupConstants.ARRTYPE || w.LookupType == LookupConstants.BOOKSTAT ||
                            w.LookupType == LookupConstants.SENTFLAG || w.LookupType == LookupConstants.SENTTYPE ||
                            w.LookupType == LookupConstants.SENTFIND || w.LookupType == LookupConstants.SENTDURATION ||
                            w.LookupType == LookupConstants.SENTFINETYPE);

            List<SentenceDetailsVm> sentenceDetailsList = _context.IncarcerationArrestXref
                .Where(w => w.IncarcerationId == incarcerationId)
                .Select(s => new SentenceDetailsVm
                {
                    IncarcerationId = s.IncarcerationId,
                    ArrestBookingNo = s.Arrest.ArrestBookingNo,
                    ArrestDate = s.Arrest.ArrestDate,
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
                    ArrestSentenceCode = s.Arrest.ArrestSentenceCode ?? 0,
                    ArrestSentenceDescription = s.Arrest.ArrestSentenceDescription,
                    ArrestId = s.ArrestId,
                    ArrestSentenceGroup = s.Arrest.ArrestSentenceGroup,
                    WeekEnder = s.Arrest.ArrestSentenceWeekender,
                    Abbr = lookuplst
                        .Where(w => w.LookupType == LookupConstants.BOOKSTAT &&
                                    w.LookupIndex == (s.Arrest.ArrestBookingStatus ?? 0))
                        .Select(a => a.LookupName).SingleOrDefault(),
                    ArrestSentenceReleaseDate = s.Arrest.ArrestSentenceReleaseDate,
                    ArrestSentenceAmended = s.Arrest.ArrestSentenceAmended == 1,
                    ArrestSentencePenalInstitution = s.Arrest.ArrestSentencePenalInstitution == 1,
                    ArrestSentenceOptionsRec = s.Arrest.ArrestSentenceOptionsRec == 1,
                    ArrestSentenceAltSentNotAllowed = s.Arrest.ArrestSentenceAltSentNotAllowed == 1,
                    ArrestSentenceNoEarlyRelease = s.Arrest.ArrestSentenceNoEarlyRelease == 1,
                    ArrestSentenceNoLocalParole = s.Arrest.ArrestSentenceNoLocalParole == 1,
                    ArrestSentenceNoDayForDay = s.Arrest.ArrestSentenceNoDayForDay == 1,
                    ArrestSentenceWeekender = s.Arrest.ArrestSentenceWeekender == 1,
                    AdditionalFlagsList = _context.ArrestSentFlag
                        .Where(w => w.ArrestId == s.ArrestId && (!w.DeleteFlag.HasValue || w.DeleteFlag == 0))
                        .Select(x => new SentenceAdditionalFlagsVm
                        {
                            ArrestSentFlagId = x.ArrestSentflagId,
                            ArrestId = x.ArrestId,
                            LookupDescription = lookuplst.SingleOrDefault(w => w.LookupIndex == x.SentflagLookupIndex
                                                                               && w.LookupType ==
                                                                               LookupConstants.SENTFLAG)
                                .LookupDescription,
                            LookupIndex = x.SentflagLookupIndex,
                            Selected = true
                        }).ToList(),
                    ArrestSentenceDateInfo = s.Arrest.ArrestSentenceDateInfo,
                    ArrestSentenceType = s.Arrest.ArrestSentenceType,
                    ArrestSentenceTypeList = lookuplst.Where(w => w.LookupType == LookupConstants.SENTTYPE).Select
                            (l => new KeyValuePair<int, string>(l.LookupIndex, l.LookupDescription))
                        .ToList(),
                    ArrestSentenceFindings = s.Arrest.ArrestSentenceFindings,
                    ArrestSentenceFindingsList = lookuplst.Where(w => w.LookupType == LookupConstants.SENTFIND).Select
                            (l => new KeyValuePair<int, string>(l.LookupIndex, l.LookupDescription))
                        .ToList(),
                    ArrestSentenceJudgeId = s.Arrest.ArrestSentenceJudgeId,
                    //ArrestSentenceJudgeList = LoadJudgeDetails(s.Arrest.ArrestSentenceJudgeId),
                    ArrestSentenceDaysInterval = s.Arrest.ArrestSentenceDaysInterval,
                    ArrestSentenceDaysIntervalList = lookuplst.Where(w => w.LookupType == LookupConstants.SENTDURATION)
                        .Select
                        (l => new KeyValuePair<int, string>(NullableTryParseInt32(l.LookupMap),
                            l.LookupDescription))
                        .OrderBy(o => o.Value).ToList(),
                    ArrestSentenceFineDays = s.Arrest.ArrestSentenceFineDays,
                    ArrestSentenceDaysStayed = s.Arrest.ArrestSentenceDaysStayed,
                    ArrestSentenceDaysStayedInterval = s.Arrest.ArrestSentenceDaysStayedInterval,
                    ArrestSentenceDaysStayedAmount = s.Arrest.ArrestSentenceDaysStayedAmount,
                    ArrestTimeServedDays = s.Arrest.ArrestTimeServedDays,
                    ArrestSentenceForthwith = s.Arrest.ArrestSentenceForthwith == 1,
                    ArrestSentenceFlatTime = s.Arrest.ArrestSentenceFlatTime,
                    ArrestSentenceManual = s.Arrest.ArrestSentenceManual == 1,
                    ArrestSentenceMethodId = s.Arrest.ArrestSentenceMethodId,
                    ArrestSentenceMethodList =
                        _context.ArrestSentenceMethod.Where(w => w.InactiveFlag != 1).Select
                            (a => new KeyValuePair<int, string>(a.ArrestSentenceMethodId,
                                a.MethodName))
                            .ToList(),
                    ArrestSentenceDaysAmount = s.Arrest.ArrestSentenceDaysAmount,
                    ArrestSentenceHours = s.Arrest.ArrestSentenceHours ?? 0,
                    ArrestSentenceExpirationDate = s.Arrest.ArrestSentenceExpirationDate,
                    ArrestSentenceDayForDayAllowedOverride = s.Arrest.ArrestSentenceDayForDayAllowedOverride == 1,
                    ArrestSentenceGtDaysOverride = s.Arrest.ArrestSentenceGtDaysOverride == 1,
                    ArrestSentenceWtDaysOverride = s.Arrest.ArrestSentenceWtDaysOverride == 1,
                    ArrestSentenceErcDays = s.Arrest.ArrestSentenceErcdays,
                    ArrestSentenceDayForDayDaysOverride = s.Arrest.ArrestSentenceDayForDayDaysOverride == 1,
                    ArrestSentenceDayForDayAllowed = s.Arrest.ArrestSentenceDayForDayAllowed,
                    ArrestSentenceGtDays = s.Arrest.ArrestSentenceGtDays ?? 0,
                    ArrestSentenceWtDays = s.Arrest.ArrestSentenceWtDays ?? 0,
                    ArrestSentenceGwGtAdjust = s.Arrest.ArrestSentenceGwGtAdjust ?? 0,
                    ArrestSentenceDayForDayDays = s.Arrest.ArrestSentenceDayForDayDays,
                    ArrestSentenceDisciplinaryDaysSum = s.Arrest.ArrestSentenceDisciplinaryDaysSum,
                    ArrestSentenceIndefiniteHold = s.Arrest.ArrestSentenceIndefiniteHold == 1,
                    SentenceFineDays = new SentenceFineDaysVm
                    {
                        ArrestSentenceFinePaid = s.Arrest.ArrestSentenceFinePaid,
                        ArrestSentenceFineToServe = s.Arrest.ArrestSentenceFineToServe,
                        ArrestSentenceFineType = s.Arrest.ArrestSentenceFineType,
                        ArrestSentenceFineAmount = s.Arrest.ArrestSentenceFineAmount,
                        ArrestSentenceFinePerDay = s.Arrest.ArrestSentenceFinePerDay,
                        ArrestSentenceFineTypeList = lookuplst.Where(a =>
                                a.LookupType == LookupConstants.SENTFINETYPE &&
                                a.LookupDescription == s.Arrest.ArrestSentenceFineType)
                            .Select(a => new LookupVm
                            {
                                LookupIndex = a.LookupIndex,
                                LookupName = a.LookupName,
                                LookupDescription = a.LookupDescription,
                                LookupCategory = a.LookupCategory
                            }).ToList()
                    },
                    AllowWeekEnderSentence =
                        _commonService.GetSiteOptionValue(SiteOptionsConstants.ALLOWWEEKENDERSENTENCE) == "ON",
                    NoDayForDayVisible =
                        _commonService.GetSiteOptionValue(SiteOptionsConstants.NODAYFORDAYVISIBLE) == "ON",
                    InmateId = s.Arrest.InmateId ?? 0,
                    ArrestClearScheduleDate = s.ReleaseDate,
                    ArrestSupSeqNumber = s.Arrest.ArrestSupSeqNumber ?? 0,
                    ArrestSentenceErcFlag = s.Arrest.ArrestSentenceMethod.ArrestSentenceErctable == 1
                }).ToList();

            sentenceDetailsList.ForEach(item =>
            {
                item.ConsecutiveArrestSupSeqNumber = sentenceDetailsList.FirstOrDefault(w => w.ArrestId == item.ArrestSentenceConsecutiveTo)?.ArrestSupSeqNumber ?? 0;
                item.ConsecutiveArrestCourtDocket = sentenceDetailsList.FirstOrDefault(f => f.ArrestId == item.ArrestSentenceConsecutiveTo)?.ArrestCourtDocket;
                item.ConsecutiveArrestSentenceCode = sentenceDetailsList.FirstOrDefault(f => f.ArrestId == item.ArrestSentenceConsecutiveTo)?.ArrestSentenceCode ?? 0;
                item.ConsecutiveArrestSentenceReleaseDate = sentenceDetailsList.FirstOrDefault(f => f.ArrestId == item.ArrestSentenceConsecutiveTo)?.ArrestSentenceReleaseDate;
                item.ConsecutiveArrestSentenceIndefiniteHold = sentenceDetailsList.FirstOrDefault(f => f.ArrestId == item.ArrestSentenceConsecutiveTo)?.ArrestSentenceIndefiniteHold ?? false;
                item.ArrestSentenceJudgeList = LoadJudgeDetails(item.ArrestSentenceJudgeId);
            });

            if (arrestId > 0)
            {
                sentenceDetailsList = sentenceDetailsList.Where(a => a.ArrestId == arrestId).ToList();
            }

            if (reportType == SentenceSummaryType.ACTIVE)
            {
                sentenceDetailsList = sentenceDetailsList.Where(w => !w.ArrestClearScheduleDate.HasValue).ToList();
            }

            return sentenceDetailsList.OrderBy(s => s.ArrestId)
                .ThenBy(s => s.IncarcerationId)
                .ThenBy(s => s.ArrestSentenceGroup)
                .ThenBy(s => s.ArrestSentenceConsecutiveFlag).ToList();
        }

        private List<ChargeSentenceVm> GetAllArrestSentenceDetailsCrimePdf(int incarcerationId,
            int arrestId, int crimeId)
        {
            List<ChargeSentenceVm> lstCrimeSentence = new List<ChargeSentenceVm>();
            if (!_sentenceByCharge)
            {
                return lstCrimeSentence;
            }

            int[] arrestIds =
                _context.IncarcerationArrestXref.Where(x => x.IncarcerationId == incarcerationId)
                    .Select(x => x.ArrestId ?? 0).ToArray();

            IQueryable<Lookup> lookuplst = _context.Lookup
                .Where(w => w.LookupType == LookupConstants.CHARGEQUALIFIER ||
                            w.LookupType == LookupConstants.CRIMETYPE);

            IQueryable<ArrestSentenceMethod> arrestSentenceMethodList = _context.ArrestSentenceMethod;

            lstCrimeSentence = _context.Crime.Where(w => arrestIds.Contains(w.ArrestId ?? 0) && w.CrimeDeleteFlag == 0)
                .Select(c => new ChargeSentenceVm
                {
                    ArrestId = c.ArrestId,
                    ArrestBookingNo = c.Arrest.ArrestBookingNo,
                    ArrestSentenceCode = c.Arrest.ArrestSentenceCode,
                    CrimeId = c.CrimeId,
                    CrimeNumber = c.CrimeNumber,
                    CrimeCount = c.CrimeCount,
                    CreateDate = c.CreateDate,
                    CrimeLookupId = c.CrimeLookupId,
                    Qualifier = lookuplst.SingleOrDefault(w => w.LookupIndex == Convert.ToInt32(c.ChargeQualifierLookup)
                                     && w.LookupType == LookupConstants.CHARGEQUALIFIER).LookupDescription,
                    CrimeStatus = lookuplst.SingleOrDefault(w => w.LookupIndex == Convert.ToInt32(c.CrimeType)
                        && w.LookupType == LookupConstants.CRIMETYPE).LookupDescription,
                    CrimeStatusId = Convert.ToInt32(c.CrimeType),
                    ArrestSentenceStartDate = c.ArrestSentenceStartDate,
                    ArrestSentenceConsecutiveTo = c.ArrestSentenceConsecutiveTo,
                    ArrestSentenceUseStartDate = c.ArrestSentenceUseStartDate,
                    ArrestSentenceDays = c.ArrestSentenceDays,
                    MethodName = arrestSentenceMethodList.Single(
                        w => w.ArrestSentenceMethodId == c.ArrestSentenceMethodId).MethodName,
                    ArrestSentenceDaysToServe = c.ArrestSentenceDaysToServe,
                    ArrestSentenceActualDaysToServe = c.ArrestSentenceActualDaysToServe,
                    ArrestSentenceReleaseDate = c.ArrestSentenceReleaseDate,
                    ArrestSentenceDateInfo = c.ArrestSentenceDateInfo,
                    ArrestSentenceType = c.ArrestSentenceType,
                    ArrestSentenceFindings = c.ArrestSentenceFindings,
                    ArrestSentenceJudgeId = c.ArrestSentenceJudgeId,
                    ArrestSentenceDaysInterval = c.ArrestSentenceDaysInterval,
                    ArrestSentenceFineDays = c.ArrestSentenceFineDays,
                    ArrestSentenceDaysStayed = c.ArrestSentenceDaysStayed,
                    ArrestSentenceDaysStayedInterval = c.ArrestSentenceDaysStayedInterval,
                    ArrestSentenceDaysStayedAmount = c.ArrestSentenceDaysStayedAmount,
                    ArrestTimeServedDays = c.ArrestTimeServedDays,
                    ArrestSentenceForthwith = c.ArrestSentenceForthwith == 1,
                    ArrestSentenceFlatTime = c.ArrestSentenceFlatTime,
                    ArrestSentenceManual = c.ArrestSentenceManual == 1,
                    ArrestSentenceMethodId = c.ArrestSentenceMethodId,
                    ArrestSentenceDaysAmount = c.ArrestSentenceDaysAmount,
                    ArrestSentenceHours = c.ArrestSentenceHours,
                    ArrestSentenceExpirationDate = c.ArrestSentenceExpirationDate,
                    ArrestSentenceDayForDayAllowedOverride = c.ArrestSentenceDayForDayAllowedOverride == 1,
                    ArrestSentenceGtDaysOverride = c.ArrestSentenceGtDaysOverride == 1,
                    ArrestSentenceWtDaysOverride = c.ArrestSentenceWtDaysOverride == 1,
                    ArrestSentenceErcDays = c.ArrestSentenceErcdays,
                    ArrestSentenceDayForDayDaysOverride = c.ArrestSentenceDayForDayDaysOverride == 1,
                    ArrestSentenceDayForDayAllowed = c.ArrestSentenceDayForDayAllowed,
                    ArrestSentenceGtDays = c.ArrestSentenceGtDays,
                    ArrestSentenceWtDays = c.ArrestSentenceWtDays,
                    ArrestSentenceGwGtAdjust = c.ArrestSentenceGwGtAdjust,
                    ArrestSentenceDayForDayDays = c.ArrestSentenceDayForDayDays,
                    ArrestSentenceDisciplinaryDaysSum = c.ArrestSentenceDisciplinaryDaysSum ?? 0,
                    ArrestClearScheduleDate = c.Arrest.ArrestSentenceReleaseDate,
                    SentenceGapFound = new SentenceGapFound
                    {
                        ProcessFlag = false
                    },
                    SentenceCharge = new SentenceChargeVm
                    {
                        CrimeId = c.CrimeId,
                        CrimeStatusId = lookuplst.SingleOrDefault(w => w.LookupType == LookupConstants.CRIMETYPE &&
                                w.LookupIndex == Convert.ToInt32(c.CrimeType)).LookupIndex
                    },
                    SentenceFineDays = new SentenceFineDaysVm
                    {
                        ArrestSentenceFinePaid = c.ArrestSentenceFinePaid,
                        ArrestSentenceFineToServe = c.ArrestSentenceFineToServe,
                        ArrestSentenceFineType = c.ArrestSentenceFineType,
                        ArrestSentenceFineAmount = c.ArrestSentenceFineAmount,
                        ArrestSentenceFinePerDay = c.ArrestSentenceFinePerDay
                    },
                }).ToList();

            if (arrestId > 0)
            {
                lstCrimeSentence = lstCrimeSentence.Where(a => a.ArrestId == arrestId).ToList();
            }

            if (crimeId > 0)
            {
                lstCrimeSentence = lstCrimeSentence.Where(a => a.CrimeId == crimeId).ToList();
            }

            int[] crimeLookupId = lstCrimeSentence.Select(x => x.CrimeLookupId ?? 0).ToArray();
            IQueryable<CrimeLookup> crimeLookups = _context.CrimeLookup
                .Where(w => crimeLookupId.Contains(w.CrimeLookupId));

            lstCrimeSentence.ForEach(item =>
            {
                item.SentenceCharge.CrimeCodeType =
                    crimeLookups.SingleOrDefault(w => w.CrimeLookupId == item.CrimeLookupId)
                        ?.CrimeCodeType;
                item.SentenceCharge.CrimeSection =
                    crimeLookups.SingleOrDefault(w => w.CrimeLookupId == item.CrimeLookupId)
                        ?.CrimeSection;
                item.SentenceCharge.CrimeDescription =
                    crimeLookups.SingleOrDefault(w => w.CrimeLookupId == item.CrimeLookupId)
                        ?.CrimeDescription;
                item.SentenceCharge.CrimeStatute =
                    crimeLookups.SingleOrDefault(w => w.CrimeLookupId == item.CrimeLookupId)
                        ?.CrimeStatuteCode;
            });

            return lstCrimeSentence.OrderBy(o => o.CrimeNumber).ToList();
        }

        public SentencePdfViewerVm GetSentencePdfViewerList(int incarcerationId, int arrestId,
            SentenceSummaryType reportType, int crimeId)
        {
            SentencePdfViewerVm sentenceViewer = new SentencePdfViewerVm
            {
                //To get Arrest Booking Status
                BookingStatus = GetBookingStatusPdf(incarcerationId, arrestId),

                //To get Sentence Details
                SentenceList = GetSentenceDetailsPdf(incarcerationId, arrestId, reportType),

                ChargeSentence = GetAllArrestSentenceDetailsCrimePdf(incarcerationId, arrestId, crimeId),

                //To get Overall Sentence Details
                OverallSentence = GetOverallSentence(incarcerationId),

                SentencePdfDetails = GetSentenceSummaryPdf(incarcerationId, arrestId, reportType),

                ArrestSentenceSetting = GetArrestSentenceSetting()
            };
            return sentenceViewer;
        }

        private SentencePdfDetailsVm GetSentenceSummaryPdf(int incarcerationId, int arrestId,
            SentenceSummaryType summaryType)
        {
            SentencePdfDetailsVm sentencePdfDetails = new SentencePdfDetailsVm();

            int inmateId = _context.Incarceration.Single(inm => inm.IncarcerationId == incarcerationId)
                               .InmateId ?? 0;
            Inmate dbInmateDetails = _context.Inmate.SingleOrDefault(inm => inm.InmateId == inmateId);

            if (dbInmateDetails == null) return sentencePdfDetails;

            //Get PDF Header Details
            sentencePdfDetails = new SentencePdfDetailsVm
            {
                AgencyName = _context.Agency.FirstOrDefault(ag => ag.AgencyJailFlag)?.AgencyName,
                StampDate = DateTime.Now,
                InmateNumber = dbInmateDetails.InmateNumber,
                PersonnelNumber = _context.Personnel.SingleOrDefault(per =>
                    per.PersonnelId == _personnelId)?.PersonnelNumber,
                OfficerName = _context.Person.FirstOrDefault(p =>
                    p.Personnel.FirstOrDefault().PersonnelId == _personnelId)?.PersonLastName
            };

            if (summaryType == SentenceSummaryType.ARRESTONLY)
            {
                string caseNo = _context.Arrest.SingleOrDefault(arr => arr.ArrestId == arrestId)?.ArrestBookingNo;
                sentencePdfDetails.SummaryHeader = caseNo;
                sentencePdfDetails.DisplayOverall = false;
            }
            else if (summaryType == SentenceSummaryType.ACTIVE)
            {
                sentencePdfDetails.SummaryHeader = SentenceSummaryType.ACTIVE.ToString();
                sentencePdfDetails.DisplayOverall = true;
            }
            else if (summaryType == SentenceSummaryType.ALL)
            {
                sentencePdfDetails.SummaryHeader = SentenceSummaryType.ALL.ToString();
                sentencePdfDetails.DisplayOverall = true;
            }

            //Get Person Details
            sentencePdfDetails.PersonDetails = GetPersonDetails(dbInmateDetails.PersonId);
            sentencePdfDetails.PersonDetails.InmateNumber = dbInmateDetails.InmateNumber;

            return sentencePdfDetails;
        }

        private PersonVm GetPersonDetails(int personId)
        {
            PersonVm personDetails = _context.Person.Where(pr => pr.PersonId == personId)
                .Select(per => new PersonVm
                {
                    PersonId = per.PersonId,
                    PersonFirstName = per.PersonFirstName,
                    PersonLastName = per.PersonLastName,
                    PersonMiddleName = per.PersonMiddleName,
                    PersonSuffix = per.PersonSuffix,
                    PersonDob = per.PersonDob
                }).Single();

            return personDetails;
        }

        public List<ArrestSentenceSettingVm> GetArrestSentenceSetting()
        {
            List<ArrestSentenceSettingVm> arrestSentenceSetting =
                _context.ArrestSentenceSetting.Where(w => w.FieldTable == "Arrest")
                    .Select(a =>
                        new ArrestSentenceSettingVm
                        {
                            FieldTable = a.FieldTable,
                            FieldName = a.FieldName.Trim(),
                            FieldOrder = a.FieldOrder,
                            FieldDisplay = a.FieldDisplay,
                            FieldDescription = a.FieldDescription,
                            FieldSettingMethodFlag = a.FieldSettingMethodFlag ?? 0,
                            FieldCalcInputRequired = a.FieldCalcInputRequired == 1,
                            FieldCalcFlag = a.FieldCalcFlag ?? 0,
                            FieldEntryFlag = a.FieldEntryFlag ?? 0,
                            FieldAllowDefault = a.FieldAllowDefault ?? 0,
                            DisplayOverride = a.DisplayOverride,
                            DisableFlag = a.DisableFlag == 1,
                            InvisibleFlag = a.InvisibleFlag == 1,
                            RequiredForCalc = a.RequiredForCalc == 1,
                            RequiredForSave = a.RequiredForSave == 1,
                            DefaultValue = a.DefaultValue ?? 0,
                            RequireForCalc = a.RequiredForCalc == 1 || a.FieldCalcInputRequired == 1,
                            FieldLabel = a.DisplayOverride.Length > 0 ? a.DisplayOverride : a.FieldDisplay
                        }).OrderBy(o => o.FieldOrder).ToList();

            return arrestSentenceSetting;
        }
    }
}