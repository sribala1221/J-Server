using System;
using System.Collections.Generic;
using System.Linq;
using GenerateTables.Models;
using ServerAPI.ViewModels;
using ServerAPI.Utilities;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace ServerAPI.Services
{
    public class SupervisorService : ISupervisorService
    {
        private readonly AAtims _context;
        private readonly IRequestService _requestService;
        private readonly IPersonService _iPersonService;
        private readonly IIncidentService _incidentService;
        private readonly int _personnelId;

        public SupervisorService(AAtims context, IPersonService personService,
            IHttpContextAccessor httpContextAccessor, IRequestService requestService, IIncidentService incidentService)
        {
            _context = context;
            _requestService = requestService;
            _iPersonService = personService;
            _personnelId = Convert.ToInt32(httpContextAccessor.HttpContext.User.FindFirst("personnelId")?.Value);
            _incidentService = incidentService;
        }

        private List<InmateHousing> GetInmateDetails(List<int> inmateIds) =>
            _context.Inmate.Where(x => inmateIds.Contains(x.InmateId)).Select(a => new InmateHousing
            {
                InmateId = a.InmateId,
                PersonId = a.PersonId,
                PersonFirstName = a.Person.PersonFirstName,
                PersonMiddleName = a.Person.PersonMiddleName,
                PersonLastName = a.Person.PersonLastName,
                PersonSuffix = a.Person.PersonSuffix,
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
                InmateActive = a.InmateActive == 1
            }).ToList();

        public BookingSupervisorVm GetBookingSupervisor(int iFacilityId)
        {
            var lstIncarcerationArrestXrefDs =
                _context.IncarcerationArrestXref.Where(iax =>
                    !iax.Incarceration.ReleaseOut.HasValue
                    && iax.Incarceration.Inmate.InmateActive == 1
                    && iax.Incarceration.Inmate.FacilityId == iFacilityId
                    && (iax.Incarceration.BookCompleteFlag == 1 || iax.Incarceration.BookAndReleaseFlag == 1)
                    && iax.ArrestId.HasValue).Select(iax => new
                {
                    BookingComplete = iax.Incarceration.BookCompleteFlag == 1,
                    ArrestBookingComplete = iax.Arrest.BookingCompleteFlag == 1,
                    IaxBookingSupervisorCompleteFlag = iax.BookingSupervisorCompleteFlag == 1,
                    IaxReleaseSupervisorCompleteFlag = iax.ReleaseSupervisorCompleteFlag == 1,
                    iax.ReleaseDate,
                    BookAndReleaseFlag = iax.Incarceration.BookAndReleaseFlag == 1,
                    ReleaseClearFlag = iax.Incarceration.ReleaseClearFlag == 1,
                    IncBookingSupervisorCompleteFlag = iax.Incarceration.BookingSupervisorCompleteFlag == 1,
                    IncReleaseSupervisorCompleteFlag = iax.Incarceration.ReleaseSupervisorCompleteFlag == 1,
                    iax.IncarcerationId
                }).ToList();

            var lstIncarceration = lstIncarcerationArrestXrefDs
                .Select(inc => new
                {
                    inc.BookAndReleaseFlag,
                    inc.ReleaseClearFlag,
                    inc.IncBookingSupervisorCompleteFlag,
                    inc.IncReleaseSupervisorCompleteFlag,
                    inc.BookingComplete,
                    inc.IncarcerationId
                }).Distinct().ToList();

            BookingSupervisorVm bookingSupervisor = new BookingSupervisorVm
            {
                ReviewOverallCount = lstIncarceration.Count(inc =>
                    !inc.IncBookingSupervisorCompleteFlag && inc.BookingComplete),
                ReviewOverallBookCount = lstIncarceration.Count(inc =>
                    !inc.IncBookingSupervisorCompleteFlag && inc.BookingComplete && inc.BookAndReleaseFlag),
                ReviewReleaseCount = lstIncarceration.Count(inc =>
                    !inc.IncReleaseSupervisorCompleteFlag && inc.ReleaseClearFlag),
                ReviewReleaseBookCount = lstIncarceration.Count(inc =>
                    !inc.IncReleaseSupervisorCompleteFlag && inc.ReleaseClearFlag && inc.BookAndReleaseFlag),
                ReviewClearCount = lstIncarcerationArrestXrefDs
                    .Count(xr => xr.ReleaseDate.HasValue && !xr.IaxReleaseSupervisorCompleteFlag),
                ReviewBookingCount = lstIncarcerationArrestXrefDs
                    .Count(xr => xr.BookingComplete && xr.ArrestBookingComplete && !xr.IaxBookingSupervisorCompleteFlag)
            };

            bookingSupervisor.ForceChargeCount = _context.CrimeForce
                .Count(crime =>
                    crime.DeleteFlag != 1 && crime.ArrestId.HasValue &&
                    crime.ForceSupervisorReviewFlag != 1
                    && crime.Arrest.IncarcerationArrestXref
                        .Any(iax => !iax.Incarceration.ReleaseOut.HasValue
                                    && iax.Incarceration.Inmate.InmateActive == 1
                                    && iax.Incarceration.Inmate.FacilityId == iFacilityId));

            bookingSupervisor.RecordsCheckCount = _context.RecordsCheckRequest.Count(rec =>
                rec.ClearFlag == 1 &&
                rec.RequestFacilityId == iFacilityId &&
                rec.SupervisorReviewFlag != 1);

            //Get narrative list
            //bookingSupervisor.LstNarratives = _incidentService.SupervisorNarrativesDetails(iFacilityId);

            //Get pending request count
            bookingSupervisor.PendingRequest = _requestService.GetBookingPendingReq(iFacilityId, 2)
                .OrderBy(x => x.RequestCount).ThenBy(y => y.RequestLookupName).ToList();
            //Get assigned request count
            bookingSupervisor.AssignedRequest = _requestService.GetBookingAssignedReq(2).OrderBy(x => x.RequestCount)
                .ThenBy(y => y.RequestLookupName).ToList();

            return bookingSupervisor;
        }

        public List<BookingReview> GetOverallReview(int iFacilityId)
        {
            List<BookingReview> bookingReviewOverall =
                _context.Incarceration.Where(inc =>
                        !inc.ReleaseOut.HasValue &&
                        inc.Inmate.InmateActive == 1 &&
                        inc.Inmate.FacilityId == iFacilityId &&
                        (inc.BookCompleteFlag == 1 || inc.BookAndReleaseFlag == 1))
                    .Select(inc => new BookingReview
                    {
                        BookAndReleaseFlag = inc.BookAndReleaseFlag == 1,
                        IncarcerationId = inc.IncarcerationId,
                        BookingSupervisorCompleteFlag = inc.BookingSupervisorCompleteFlag == 1,
                        OverallFinalReleaseDate = inc.OverallFinalReleaseDate,
                        ReleaseClearDate = inc.ReleaseClearDate,
                        InmateId = inc.InmateId.Value,
                        ReleaseSupervisorCompleteFlag = inc.ReleaseSupervisorCompleteFlag == 1,
                        ReleaseClearFlag = inc.ReleaseClearFlag == 1,
                        ExpediteBookingReason = inc.ExpediteBookingReason,
                        ExpediteBookingFlag = inc.ExpediteBookingFlag,
                        ExpediteBookingNote = inc.ExpediteBookingNote,
                        ExpediteDate = inc.ExpediteBookingDate,
                        ExpediteById = inc.ExpediteBookingBy,
                        NoKeeper = inc.NoKeeper,
                        BookCompleteFlag = inc.BookCompleteFlag == 1,
                        PersonDetails = new InmateHousing(){
                            InmateId = inc.InmateId ?? 0,
                            PersonId = inc.Inmate.PersonId,
                            PersonFirstName = inc.Inmate.Person.PersonFirstName,
                            PersonMiddleName = inc.Inmate.Person.PersonMiddleName,
                            PersonLastName = inc.Inmate.Person.PersonLastName,
                            PersonSuffix = inc.Inmate.Person.PersonSuffix,
                            InmateNumber = inc.Inmate.InmateNumber,
                            FacilityId = inc.Inmate.FacilityId,
                            LastReviewDate = inc.Inmate.LastReviewDate,
                            InmateCurrentTrack = inc.Inmate.InmateCurrentTrack,
                            HousingUnitId = inc.Inmate.HousingUnitId,
                            HousingLocation = inc.Inmate.HousingUnit.HousingUnitLocation,
                            HousingNumber = inc.Inmate.HousingUnit.HousingUnitNumber,
                            HousingBedLocation = inc.Inmate.HousingUnit.HousingUnitBedLocation,
                            HousingBedNumber = inc.Inmate.HousingUnit.HousingUnitBedNumber,
                            PersonSexLast = inc.Inmate.Person.PersonSexLast,
                            InmateClassificationReason = inc.Inmate.InmateClassification.InmateClassificationReason,
                            InmateActive = inc.Inmate.InmateActive == 1
                        }
                    }).OrderByDescending(over => over.ExpediteBookingFlag)
                    .ThenByDescending(over => over.OverallFinalReleaseDate.HasValue)
                    .ThenBy(over => over.OverallFinalReleaseDate.Value)
                    .ToList();

            List<int> officerIds = bookingReviewOverall.Where(i => i.ExpediteById.HasValue)
                .Select(i => i.ExpediteById.Value).ToList();

            List<PersonnelVm> expediteOfficer = _iPersonService.GetPersonNameList(officerIds.ToList());

            int[] lstIncIds = bookingReviewOverall.Select(inc => inc.IncarcerationId).ToArray();

            List<AoWizardProgressIncarceration> lstAoWizardProgressIncarceration =
                _context.AoWizardProgressIncarceration.Where(awp =>
                    lstIncIds.Contains(awp.IncarcerationId)).ToList();

            int[] lstWizardProgressIds =
                lstAoWizardProgressIncarceration.Select(inc => inc.AoWizardProgressId).ToArray();

            List<AoWizardStepProgress> lstAoWizardStepProgress = _context.AoWizardStepProgress
                .Where(aws => lstWizardProgressIds.Contains(aws.AoWizardProgressId)).ToList();

            var lstIncarcerationArrestXrefDs =
                _context.IncarcerationArrestXref
                    .Where(iax =>
                        !iax.Incarceration.ReleaseOut.HasValue &&
                        iax.Incarceration.Inmate.InmateActive == 1 &&
                        iax.Incarceration.Inmate.FacilityId == iFacilityId &&
                        (iax.Incarceration.BookCompleteFlag == 1 || iax.Incarceration.BookAndReleaseFlag == 1) &&
                        iax.Incarceration.BookingSupervisorCompleteFlag != 1 && iax.Incarceration.BookCompleteFlag == 1
                        && iax.ArrestId.HasValue).Select(iax => new
                    {
                        IncarcerationId = iax.IncarcerationId.Value,
                        iax.ReleaseReason,
                        ArrestTypeId = iax.Arrest.ArrestType
                    }).ToList();

            bookingReviewOverall.ForEach(book =>
            {
                book.BookingSupervisorId = lstAoWizardProgressIncarceration.FirstOrDefault(awp =>
                    awp.IncarcerationId == book.IncarcerationId &&
                    awp.AoWizardId == (int?) Wizards.bookingSupervisor)?.AoWizardProgressId;

                book.ReleaseSupervisorId = lstAoWizardProgressIncarceration.FirstOrDefault(awp =>
                    awp.IncarcerationId == book.IncarcerationId &&
                    awp.AoWizardId == (int?) Wizards.releaseSupervisor)?.AoWizardProgressId;

                if (book.BookingSupervisorId.HasValue)
                {
                    book.ReviewBookingWizards = LoadWizards(lstAoWizardStepProgress, book.BookingSupervisorId.Value);
                }

                if (book.ReleaseSupervisorId.HasValue)
                {
                    book.ReviewReleaseWizards = LoadWizards(lstAoWizardStepProgress, book.ReleaseSupervisorId.Value);
                }

                book.CaseType = lstIncarcerationArrestXrefDs.Where(
                    iax => book.IncarcerationId == iax.IncarcerationId).Select(iax => iax.ArrestTypeId).ToArray();

                book.LastClearReason = lstIncarcerationArrestXrefDs.Where(
                    iax => book.IncarcerationId == iax.IncarcerationId).Max(iax => iax.ReleaseReason);

                book.ExpediteBy = book.ExpediteById.HasValue
                    ? expediteOfficer.Single(ao => ao.PersonnelId == book.ExpediteById.Value)
                    : null;
            });

            return bookingReviewOverall;
        }

        public List<ArrestReview> GetReviewBooking(int iFacilityId, bool isClear)
        {
            List<LookupVm> lstLook = _context.Lookup.Where(look =>
                look.LookupType == LookupConstants.ARRTYPE &&
                look.LookupInactive == 0).Select(look => new LookupVm
            {
                LookupType = look.LookupType,
                LookupIndex = look.LookupIndex,
                LookupDescription = look.LookupDescription
            }).ToList();

            List<ArrestReview> lstReviewBooking =
                _context.IncarcerationArrestXref.Where(iax =>
                    !iax.Incarceration.ReleaseOut.HasValue &&
                    iax.Incarceration.Inmate.InmateActive == 1 &&
                    iax.Incarceration.Inmate.FacilityId == iFacilityId &&
                    (iax.Incarceration.BookCompleteFlag == 1 || iax.Incarceration.BookAndReleaseFlag == 1)
                    && iax.ArrestId.HasValue).Select(iax => new ArrestReview
                {
                    BookingComplete = iax.Incarceration.BookCompleteFlag == 1,
                    BookingSupervisorCompleteFlag = iax.BookingSupervisorCompleteFlag == 1,
                    ReleaseSupervisorCompleteFlag = iax.ReleaseSupervisorCompleteFlag == 1,
                    IncarcerationArrestXrefId = iax.IncarcerationArrestXrefId,
                    IncarcerationId = iax.IncarcerationId.Value,
                    ReleaseDate = iax.ReleaseDate,
                    ReleaseReason = iax.ReleaseReason,
                    ArrestId = iax.ArrestId.Value,
                    ReleaseClearDate = iax.Incarceration.ReleaseClearDate,
                    InmateId = iax.Incarceration.InmateId
                }).ToList();

            lstReviewBooking = lstReviewBooking.Where(iax => isClear
                ? iax.ReleaseDate.HasValue && !iax.ReleaseSupervisorCompleteFlag
                : iax.BookingComplete &&
                  !iax.BookingSupervisorCompleteFlag).ToList();

            List<Arrest> lstArrest = _context.Arrest.Where(arr =>
                arr.IncarcerationArrestXref
                    .Any(iax => !iax.Incarceration.ReleaseOut.HasValue
                                && iax.Incarceration.Inmate.InmateActive == 1
                                && iax.Incarceration.Inmate.FacilityId == iFacilityId
                                && (iax.Incarceration.BookCompleteFlag == 1 ||
                                    iax.Incarceration.BookAndReleaseFlag == 1))
            ).Select(a => new Arrest
            {
                ArrestId = a.ArrestId,
                BookingCompleteFlag = a.BookingCompleteFlag,
                ArrestBookingNo = a.ArrestBookingNo,
                BookingCompleteDate = a.BookingCompleteDate,
                ArrestType = a.ArrestType,
                ArrestBookingDate = a.ArrestBookingDate,
                ArrestCaseNumber = a.ArrestCaseNumber,
                ArrestActive = a.ArrestActive,
                InmateId = a.InmateId
            }).ToList();

            List<int> inmateIds = lstReviewBooking.Select(i => i.InmateId ?? 0).Distinct().ToList();
            List<int> notExistInmateIds = lstArrest.Where(arr => 
                !lstReviewBooking.Select(i => i.InmateId ?? 0).Any(b => b == arr.InmateId))
                .Select(a => a.InmateId ?? 0).Distinct().ToList();
            inmateIds.AddRange(notExistInmateIds);
            List<InmateHousing> lstPersonDetails = GetInmateDetails(inmateIds);

            List<AoWizardProgressArrest> lstAoWizardProgressArrest = _context.AoWizardProgressArrest
                .Where(awp =>
                    awp.Arrest.IncarcerationArrestXref
                        .Any(iax => !iax.Incarceration.ReleaseOut.HasValue
                                    && iax.Incarceration.Inmate.InmateActive == 1
                                    && iax.Incarceration.Inmate.FacilityId == iFacilityId
                                    && (iax.Incarceration.BookCompleteFlag == 1 ||
                                        iax.Incarceration.BookAndReleaseFlag == 1))
                ).Select(a => new AoWizardProgressArrest
                {
                    AoWizardProgressId = a.AoWizardProgressId,
                    ArrestId = a.ArrestId
                }).ToList();

            int[] lstArrestWizardProgressIds =
                lstAoWizardProgressArrest.Select(inc => inc.AoWizardProgressId).ToArray();

            List<AoWizardStepProgress> lstAoArrestWizardStepProgress = _context.AoWizardStepProgress
                .Where(aws => lstArrestWizardProgressIds.Contains(aws.AoWizardProgressId)).ToList();

            lstReviewBooking.ForEach(book =>
            {
                Arrest arrest = lstArrest.Find(arr => arr.ArrestId == book.ArrestId);
                book.ArrestBookingComplete = arrest.BookingCompleteFlag == 1;
                book.BookingNumber = arrest.ArrestBookingNo;
                book.BookingCompleteDate = arrest.BookingCompleteDate;
                book.ArrestTypeId = arrest.ArrestType;
                book.ArrestBookingDate = arrest.ArrestBookingDate;
                book.ArrestCaseNumber = arrest.ArrestCaseNumber;
                book.ArrestActive = arrest.ArrestActive == 1;
                book.PersonDetails = lstPersonDetails.SingleOrDefault(per => per.InmateId == arrest.InmateId);
                book.ArrestType = arrest.ArrestType != null
                    ? lstLook.SingleOrDefault(look =>
                        Convert.ToString(look.LookupIndex) == arrest.ArrestType.Trim() &&
                        look.LookupType == LookupConstants.ARRTYPE)?.LookupDescription
                    : null;

                book.ReviewBookingWizardId = lstAoWizardProgressArrest.FirstOrDefault(awp =>
                    awp.ArrestId == book.ArrestId &&
                    awp.AoWizardId == (int?) Wizards.bookingDataSupervisorReview)?.AoWizardProgressId;

                book.ReviewClearWizardId = lstAoWizardProgressArrest.FirstOrDefault(awp =>
                    awp.ArrestId == book.ArrestId &&
                    awp.AoWizardId == (int?) Wizards.bookingDataSupervisorClear)?.AoWizardProgressId;

                if (book.ReviewBookingWizardId.HasValue)
                {
                    book.ReviewBookingWizard =
                        LoadWizards(lstAoArrestWizardStepProgress, book.ReviewBookingWizardId.Value);
                }

                if (book.ReviewClearWizardId.HasValue)
                {
                    book.ReviewClearWizard = LoadWizards(lstAoArrestWizardStepProgress, book.ReviewClearWizardId.Value);
                }
            });

            lstReviewBooking = lstReviewBooking.Where(iax => isClear || iax.ArrestBookingComplete).ToList();
            return lstReviewBooking;
        }

        public List<BookingSearchSubData> GetForceCharge(int facilityId)
        {
            List<LookupVm> lstLook = _context.Lookup.Where(look =>
                look.LookupType == LookupConstants.CRIMETYPE &&
                look.LookupInactive == 0).Select(look => new LookupVm
            {
                LookupType = look.LookupType,
                LookupIndex = look.LookupIndex,
                LookupDescription = look.LookupDescription
            }).ToList();

            List<BookingSearchSubData> lstCharges = _context.CrimeForce.Where(crime =>
                crime.DeleteFlag != 1 && crime.ArrestId.HasValue &&
                crime.ForceSupervisorReviewFlag != 1
                && crime.Arrest.IncarcerationArrestXref
                    .Any(iax => !iax.Incarceration.ReleaseOut.HasValue
                                && iax.Incarceration.Inmate.InmateActive == 1
                                && iax.Incarceration.Inmate.FacilityId == facilityId)
            ).Select(c => new BookingSearchSubData
            {
                CrimeLookupId = c.ForceCrimeLookupId ?? 0,
                CrimeSection = c.TempCrimeSection,
                CrimeStatueCode = c.TempCrimeStatuteCode,
                ArrestId = c.ArrestId ?? 0,
                WarrantNumber = c.Warrant.WarrantNumber,
                Type = c.TempCrimeCodeType,
                Description = c.TempCrimeDescription,
                Note = c.TempCrimeNotes,
                CrimeCount = c.TempCrimeCount ?? 0,
                BailAmount = c.BailAmount,
                BailFlag = c.BailNoBailFlag == 1,
                CrimeType = c.TempCrimeType,
                CreateDate = c.CreateDate,
                CreatedBy = c.CreateBy,
                UpdateDate = c.UpdateDate,
                UpdatedBy = c.UpdateBy,
                CrimeForceId = c.CrimeForceId,
                DeleteFlag = c.DeleteFlag,
                CrimeGroupId = int.Parse(c.TempCrimeGroup),
                ChargeQualifierLookup = c.ChargeQualifierLookup,
                WarrantId = c.WarrantId ?? 0,
                BookingChargeType = BookingChargeType.CHARGE,
                WarrantCounty = c.Warrant.WarrantCounty,
                WarrantAgencyId = c.Warrant.WarrantAgencyId,
                InmateId = c.Arrest.InmateId,
                BookingNumber = c.Arrest.ArrestCaseNumber,
                Status = lstLook.SingleOrDefault(w =>
                        w.LookupIndex == Convert.ToInt32(c.TempCrimeType) &&
                        w.LookupType == LookupConstants.CRIMETYPE)
                    .LookupDescription,
                CrimeStatusLookup = c.TempCrimeStatusLookup
            }).ToList();

            List<int> officerIds =
                lstCharges.Select(i => new[] {i.CreatedBy, i.UpdatedBy})
                    .SelectMany(i => i)
                    .Where(i => i.HasValue)
                    .Select(i => i.Value)
                    .ToList();

            List<PersonnelVm> personnelOfficers = _iPersonService.GetPersonNameList(officerIds.ToList());

            int[] inmateIds = lstCharges.Where(ii => ii.InmateId.HasValue).Select(ii => ii.InmateId.Value).ToArray();

            List<InmateHousing> lstPersonDetails = GetInmateDetails(inmateIds.ToList());

            lstCharges.ForEach(
                ii =>
                {
                    ii.PersonDetails = lstPersonDetails.Single(per => per.InmateId == ii.InmateId);
                    ii.CreatedPersonnel = personnelOfficers.Single(per => per.PersonnelId == ii.CreatedBy);
                    if (ii.UpdatedBy.HasValue)
                    {
                        ii.UpdatedPersonnel = personnelOfficers.Single(per => per.PersonnelId == ii.UpdatedBy);
                    }
                });

            return lstCharges;
        }

        public List<RecordsCheckVm> GetRecordsCheckResponse(int facilityId)
        {
            List<RecordsCheckVm> lstRecordsCheck = _context.RecordsCheckRequest.Where(rec =>
                rec.ClearFlag == 1 &&
                rec.RequestFacilityId == facilityId &&
                rec.SupervisorReviewFlag != 1).Select(rcr => new RecordsCheckVm
            {
                RecordsCheckRequestId = rcr.RecordsCheckRequestId,
                RequestType = rcr.RequestType,
                RequestNote = rcr.RequestNote,

                PersonDetails = new InmateHousing
                {
                    PersonLastName = rcr.Person.PersonLastName,
                    PersonFirstName = rcr.Person.PersonFirstName,
                    PersonMiddleName = rcr.Person.PersonMiddleName,
                    InmateNumber = _context.Inmate.Single(i => i.PersonId == rcr.PersonId).InmateNumber,
                    PersonSuffix = rcr.Person.PersonSuffix
                },

                RequestDate = rcr.RequestDate,
                ResponseDate = rcr.ResponseDate,
                ClearDate = rcr.ClearDate,

                RequestOfficer = new PersonnelVm
                {
                    PersonLastName = rcr.RequestByNavigation.PersonNavigation.PersonLastName,
                    OfficerBadgeNumber = rcr.RequestByNavigation.OfficerBadgeNum
                },

                ResponseOfficer = new PersonnelVm
                {
                    PersonLastName = rcr.ResponseByNavigation.PersonNavigation.PersonLastName,
                    OfficerBadgeNumber = rcr.ResponseByNavigation.OfficerBadgeNum
                },

                ClearOfficer = new PersonnelVm
                {
                    PersonLastName = rcr.ClearByNavigation.PersonNavigation.PersonLastName,
                    OfficerBadgeNumber = rcr.ClearByNavigation.OfficerBadgeNum
                },

                RequestBy = rcr.RequestBy,
                ResponseBy = rcr.ResponseBy,
                ClearBy = rcr.ClearBy,
                ByPassFlag = rcr.BypassFlag.HasValue,
                ResponseFlag = rcr.ResponseFlag.HasValue,
                ClearFlag = rcr.ClearFlag.HasValue,
                SupervisorReviewFlag = rcr.SupervisorReviewFlag.HasValue,

                BypassNote = rcr.BypassNotes,
                ResponseNote = rcr.ResponseNote,
                ClearNote = rcr.ClearNote,
                DeleteFlag = rcr.DeleteFlag == 1,
                ByPassDate = rcr.BypassDate,
                RequestFlag = rcr.RequestFlag.HasValue,
                SupervisorReviewDate = rcr.SupervisorReviewDate,
                SupervisorBy = rcr.SupervisorReviewBy,
                ByPassBy = rcr.BypassBy,
                RequestFacilityId = rcr.RequestFacilityId
            }).ToList();

            return lstRecordsCheck;
        }

        private List<WizardStep> LoadWizards(List<AoWizardStepProgress> wizardStepProgresses, int wizardId) =>
            wizardStepProgresses.Where(aws => aws.AoWizardProgressId == wizardId).Select(aw =>
                new WizardStep
                {
                    ComponentId = aw.AoComponentId,
                    WizardProgressId = aw.AoWizardProgressId,
                    AoWizardFacilityStepId = aw.AoWizardFacilityStepId,
                    WizardStepProgressId = aw.AoWizardStepProgressId,
                    StepComplete = aw.StepComplete
                }).ToList();

        public int CompleteForceCharge(PrebookCharge charges, int option)
        {
            int result = 0;
            if (option != 3)
            {
                int crimeLookUpId = option == 1 ? InsertCrimeLookup(charges) : charges.CrimeLookupId ?? 0;
                int crimeId = InsertCrime(charges, crimeLookUpId);
                UpdateCrimeForce(charges, crimeLookUpId, option);
                UpdateCrimeHistory(charges, crimeId);
                result = InsertCrimeHistory(charges, crimeId);
            }
            else if (option == 3)
            {
                result = UpdateCrimeForce(charges, 0, option);
            }

            return result;
        }

        private int InsertCrimeLookup(PrebookCharge charges)
        {
            CrimeLookup crimeLookup = new CrimeLookup
            {
                CrimeCodeType = charges.CrimeCodeType,
                CrimeSection = charges.CrimeSection,
                CrimeDescription = charges.CrimeDescription,
                CrimeGroupId = charges.CrimeGroupId,
                BailAmountDefault = charges.BailAmount,
                CrimeStatuteCode = charges.CrimeStatuteCode
            };
            _context.CrimeLookup.Add(crimeLookup);
            _context.SaveChanges();
            return crimeLookup.CrimeLookupId;
        }

        private int InsertCrime(PrebookCharge charges, int crimeLookUpId)
        {
            Crime crime = new Crime
            {
                CrimeLookupId = crimeLookUpId,
                CrimeCount = charges.CrimeCount,
                BailAmount = charges.BailAmount,
                BailNoBailFlag = charges.BailNoBailFlag ? 1 : default,
                CrimeType = charges.CrimeType,
                ArrestId = charges.ArrestId,
                WarrantId = charges.WarrantId,
                CrimeNotes = charges.CrimeNotes,
                CreateDate = DateTime.Now,
                CreatedBy = _personnelId,
                CrimeStatusLookup = charges.CrimeStatusLookup
            };
            _context.Crime.Add(crime);
            _context.SaveChanges();
            return crime.CrimeId;
        }

        private int UpdateCrimeForce(PrebookCharge charges, int crimeLookUpId, int option)
        {
            CrimeForce crimeForce = _context.CrimeForce.Single(w => w.CrimeForceId == charges.CrimeForceId);
            crimeForce.ForceSupervisorReviewFlag = 1;
            crimeForce.ForceSupervisorCompleteDate = DateTime.Now;
            crimeForce.ForceSupervisorCompleteBy = _personnelId;
            switch (option)
            {
                case 1:
                    crimeForce.ForceCrimeLookupId = crimeLookUpId;
                    break;
                case 2:
                    crimeForce.SearchCrimeLookupId = crimeLookUpId;
                    break;
                case 3:
                    crimeForce.DropChargeFlag = 1;
                    break;
            }

            return _context.SaveChanges();
        }

        private void UpdateCrimeHistory(PrebookCharge charges, int crimeId)
        {
            List<CrimeHistory> crimeHistory =
                _context.CrimeHistory.Where(w => w.CrimeForceId == charges.CrimeForceId).ToList();
            foreach (CrimeHistory e in crimeHistory)
            {
                e.CrimeId = crimeId;
            }

            _context.SaveChanges();
        }

        private int InsertCrimeHistory(PrebookCharge charges, int crimeId)
        {
            CrimeHistory crimeHistory = new CrimeHistory
            {
                CrimeId = crimeId,
                CrimeCount = charges.CrimeCount,
                BailAmount = charges.BailAmount,
                BailNoBailFlag = charges.BailNoBailFlag ? 1 : default,
                CrimeType = charges.CrimeType,
                CrimeLookupId = charges.CrimeLookupId,
                CrimeNotes = charges.CrimeNotes,
                CreatDate = DateTime.Now,
                CreatedBy = _personnelId,
                ChargeQualifierLookup = charges.ChargeQualifierId.ToString(),
                CrimeStatusLookup = charges.CrimeStatusLookup,
                CrimeQualifierLookup = charges.ChargeQualifierId > 0 ? charges.ChargeQualifierId : default
            };
            _context.CrimeHistory.Add(crimeHistory);
            return _context.SaveChanges();
        }

        public async Task<int> UpdateReviewComplete(BookingComplete supervisorComplete)
        {
            Incarceration incDetails = _context.Incarceration.Single(inc =>
                inc.InmateId == supervisorComplete.InmateId &&
                inc.IncarcerationId == supervisorComplete.IncarcerationId);

            if (supervisorComplete.ReviewReleaseFlag)
            {
                incDetails.ReleaseSupervisorCompleteFlag = supervisorComplete.IsComplete ? 1 : 0;
                incDetails.ReleaseSupervisorCompleteDate = DateTime.Now;
                incDetails.ReleaseSupervisorCompleteBy = _personnelId;
                incDetails.ReleaseSupervisorCompleteByNavigation =
                    _context.Personnel.Single(per => per.PersonnelId == _personnelId);
            }
            else
            {
                incDetails.BookingSupervisorCompleteFlag = supervisorComplete.IsComplete ? 1 : 0;
                incDetails.BookingSupervisorCompleteDate = DateTime.Now;
                incDetails.BookingSupervisorCompleteBy = _personnelId;
                incDetails.BookingSupervisorCompleteByNavigation =
                    _context.Personnel.Single(per => per.PersonnelId == _personnelId);
            }


            return await _context.SaveChangesAsync();
        }

        public bool GetReleaseValidation(int incarcerationId) =>
            _context.Incarceration.Find(incarcerationId).ReleaseSupervisorCompleteFlag == 1;
    }
}
