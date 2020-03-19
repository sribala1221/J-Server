using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GenerateTables.Models;
using ServerAPI.ViewModels;
using ServerAPI.Utilities;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace ServerAPI.Services
{
    public class InmateBookingService : IInmateBookingService
    {
        private readonly AAtims _context;

        private InmateBookingDetailsVm _inmateBookingDetailsVm;

        private readonly int _personnelId;

        private readonly IBookingReleaseService _bookingReleaseService;
        private readonly ICommonService _commonService;
        private readonly ISearchService _searchService;
        private readonly List<int> _arrestIds = new List<int>();
        private readonly IInterfaceEngineService _interfaceEngineService;
        private readonly IAppointmentService _appointmentService;

        public InmateBookingService(AAtims context, IHttpContextAccessor httpContextAccessor,
            IBookingReleaseService bookingReleaseService, ICommonService commonService,
            ISearchService searchService, IInterfaceEngineService interfaceEngineService,
            IAppointmentService appointmentService)
        {
            _context = context;
            _personnelId = Convert.ToInt32(httpContextAccessor.HttpContext
                .User.FindFirst("personnelId")?.Value);
            _bookingReleaseService = bookingReleaseService;
            _commonService = commonService;
            _searchService = searchService;
            _interfaceEngineService = interfaceEngineService;
            _appointmentService = appointmentService;
        }

        #region Get Inmate Bookingdetails

        public InmateBookingDetailsVm GetInmateBookingDetails(InmateBookingData inmateData)
        {
            _inmateBookingDetailsVm = new InmateBookingDetailsVm
            {
                BookingInmateDetails = _searchService.GetIncarcerationDetails(inmateData.InmateId)
            };

            //To get Inmate details from Incarceration table

            _inmateBookingDetailsVm.BookingInmateDetails.ForEach(item =>
            {
                _arrestIds.AddRange(item.IncarcerationArrestXrefDetailLSt
                    .Select(i => i.ArrestId));
            });

            //To get Charges details
            _inmateBookingDetailsVm.ArrestIds = _arrestIds;

            //To get Wizard Flag details
            GetWizardFlagDetails(inmateData.IncarcerationId);

            //To get WizardStep Details
            GetWizardStepDetails(inmateData.IncarcerationId, inmateData.PersonId, inmateData.FacilityId, _arrestIds);

            //To get SiteOptions
            GetSiteOptions();

            //To get ActiveCommitDetailsCnt
            _inmateBookingDetailsVm.ActiveCommitDetailsCnt = GetActiveCommitDetails(inmateData.PersonId).Count;

            //To get GetReactivateArrestId
            GetReactivateArrestId(inmateData.InmateId);

            return _inmateBookingDetailsVm;
        }

        private void GetReactivateArrestId(int inmateId)
        {
            //Get Reactivate ArrestId 
            _inmateBookingDetailsVm.ReactivateArrestId = _context.IncarcerationArrestXref
                .Where(i => i.Incarceration.InmateId == inmateId && !i.ReleaseDate.HasValue).Select(i => i.ArrestId).ToArray();

        }

        public List<IncarcerationArrestXrefDetails> GetBookingDataDetails(int inmateId, int facilityId)
        {
            //To get IncarcerationArrestXref details for booking data grid
            List<IncarcerationArrestXrefDetails> incarceratinArrestXrefLst = _context.IncarcerationArrestXref
                .Where(i => i.IncarcerationId.HasValue
                            && i.Arrest.InmateId == inmateId)
                .Select(inax => new IncarcerationArrestXrefDetails
                {
                    ArrestId = inax.ArrestId ?? 0,
                    IncarcerationId = inax.IncarcerationId ?? 0,
                    IncarcerationArrestXrefId = inax.IncarcerationArrestXrefId,
                    InmateId = inax.Arrest.InmateId ?? 0,
                    BailAmount = inax.Arrest.BailAmount,
                    ArrestSentenceStartDate = inax.Arrest.ArrestSentenceStartDate,
                    ArrestSentenceReleaseDate = inax.Arrest.ArrestSentenceReleaseDate,
                    BookingNumber = inax.Arrest.ArrestBookingNo,
                    CourtDocket = inax.Arrest.ArrestCourtDocket,
                    ArrestType = Convert.ToInt32(inax.Arrest.ArrestType),
                    BookingStatus = inax.ReleaseDate.HasValue
                            ? BookingStatus.INACTIVE.ToString()
                            : BookingStatus.ACTIVE.ToString(),
                    ReleaseDate = inax.ReleaseDate,
                    BookingDate = inax.BookingDate,
                    ReleaseReason = inax.ReleaseReason,
                    CaseNumber = inax.Arrest.ArrestCaseNumber,
                    BookingActive = inax.Arrest.ArrestActive,
                    ClearFlag = inax.ReleaseDate.HasValue ? 1 : 0,
                    ReactivateFlag = inax.ReactivateFlag,
                    Arrest = inax.Arrest.ArrestingAgency.AgencyAbbreviation,
                    Court = inax.Arrest.ArrestCourtJurisdiction.AgencyAbbreviation,
                    WeekEnder = inax.Arrest.ArrestSentenceWeekender,
                    wizardCompleteFlag = inax.Arrest.BookingCompleteFlag,
                    BookingSupervisorCompleteFlag = inax.BookingSupervisorCompleteFlag == 1,
                    ReleaseSupervisorCompleteFlag = inax.ReleaseSupervisorCompleteFlag == 1
                }).ToList();

            //To get LookUp details
            List<Lookup> lookupLst = (from lkp in _context.Lookup
                                      where lkp.LookupType == LookupConstants.ARRTYPE && lkp.LookupInactive == 0
                                      select new Lookup
                                      {
                                          LookupIndex = lkp.LookupIndex,
                                          LookupDescription = lkp.LookupDescription
                                      }).Distinct().ToList();

            incarceratinArrestXrefLst.ForEach(item =>
            {
                //To add ArrestIds for getting WizardSteps
                _arrestIds.Add(item.ArrestId);

                item.AoWizardProgressId = _context.AoWizardProgressArrest
                    .SingleOrDefault(w => w.ArrestId == item.ArrestId && w.AoWizardId == 14)
                    ?.AoWizardProgressId;

                item.ReviewBookingWizardId = _context.AoWizardProgressArrest
                    .SingleOrDefault(w => w.ArrestId == item.ArrestId && w.AoWizardId == 17)
                    ?.AoWizardProgressId;

                item.ReviewClearWizardId = _context.AoWizardProgressArrest
                    .SingleOrDefault(w => w.ArrestId == item.ArrestId && w.AoWizardId == 18)
                    ?.AoWizardProgressId;

                item.BookingType = lookupLst.SingleOrDefault
                    (lkp => lkp.LookupIndex == item.ArrestType)?.LookupDescription;
                if (item.AoWizardProgressId.HasValue)
                {
                    item.BookingDataWizardStep = _context.AoWizardStepProgress
                        .Where(w => w.AoWizardProgressId == item.AoWizardProgressId.Value)
                        .Select(s => new AoWizardStepProgressVm
                        {
                            ComponentId = s.AoComponentId,
                            StepComplete = s.StepComplete,
                            WizardStepProgressId = s.AoWizardStepProgressId,
                            AoWizardFacilityStepId = s.AoWizardFacilityStepId,
                            StepCompleteById = s.StepCompleteById,
                            StepCompleteDate = s.StepCompleteDate,
                        }).ToList();

                }

                if (item.ReviewBookingWizardId.HasValue)
                {
                    item.ReviewBookingWizard = _context.AoWizardStepProgress
                        .Where(w => w.AoWizardProgressId == item.ReviewBookingWizardId.Value)
                        .Select(s => new AoWizardStepProgressVm
                        {
                            ComponentId = s.AoComponentId,
                            StepComplete = s.StepComplete,
                            WizardStepProgressId = s.AoWizardStepProgressId,
                            AoWizardFacilityStepId = s.AoWizardFacilityStepId,
                            StepCompleteById = s.StepCompleteById,
                            StepCompleteDate = s.StepCompleteDate,
                        }).ToList();

                }

                if (item.ReviewClearWizardId.HasValue)
                {
                    item.ReviewClearWizard = _context.AoWizardStepProgress
                        .Where(w => w.AoWizardProgressId == item.ReviewClearWizardId.Value)
                        .Select(s => new AoWizardStepProgressVm
                        {
                            ComponentId = s.AoComponentId,
                            StepComplete = s.StepComplete,
                            WizardStepProgressId = s.AoWizardStepProgressId,
                            AoWizardFacilityStepId = s.AoWizardFacilityStepId,
                            StepCompleteById = s.StepCompleteById,
                            StepCompleteDate = s.StepCompleteDate,
                        }).ToList();

                }
            });

            return incarceratinArrestXrefLst;
        }

        private void GetSiteOptions()
        {
            //SiteOptions value for Booking clearflag
            _inmateBookingDetailsVm.SiteOptions = _context.SiteOptions
                .Count(s => (s.SiteOptionsName == SiteOptionsConstants.ALLOWWEEKENDERSENTENCE
                             || s.SiteOptionsName == SiteOptionsConstants.ALLOWBOOKINGREACTIVATES)
                            && s.SiteOptionsValue == SiteOptionsConstants.ON
                            && s.SiteOptionsStatus == "1");
        }

        //To get IsActiveBooking
        private int GetActiveBooking(int arrestId, string bookingNumber) =>
            _context.Arrest.Where(a => a.ArrestId != arrestId && a.ArrestBookingNo == bookingNumber)
                .OrderByDescending(a => a.ArrestId)
                .Select(a => a.ArrestActive ?? 0).FirstOrDefault();

        private int GetReactivateSiteoptions(int weekEnder) => weekEnder == 1
            ? _context.SiteOptions.Count(s => s.SiteOptionsName == SiteOptionsConstants.ALLOWWEEKENDERSENTENCE
                && s.SiteOptionsValue == SiteOptionsConstants.ON
                && s.SiteOptionsStatus == "1")
            : _context.SiteOptions.Count(s => s.SiteOptionsName == SiteOptionsConstants.ALLOWBOOKINGREACTIVATES
                && s.SiteOptionsValue == SiteOptionsConstants.ON
                && s.SiteOptionsStatus == "1");

        public IntakeEntryVm GetSiteOptionsReactivate(int weekEnder, int arrestId, string bookingNumber)
        {
            //get Lookup details
            List<Lookup> lstLookUp = _context.Lookup.Where(
                x => x.LookupType == LookupConstants.ARRTYPE && x.LookupInactive == 0).ToList();

            //To get details for insert person intake
            IntakeEntryVm personIntakeDetails = _context.Arrest
                .Where(iax => iax.ArrestId == arrestId)
                .Select(iax => new IntakeEntryVm
                {
                    BookingType = Convert.ToString(lstLookUp.SingleOrDefault(lkp =>
                        lkp.LookupIndex == Convert.ToInt32(iax.ArrestType)).LookupIndex),
                    ArrestAgencyId = iax.ArrestingAgencyId,
                    BookingAgencyId = iax.BookingAgencyId,
                    ArrestLocation = iax.ArrestLocation
                }).SingleOrDefault();

            if (personIntakeDetails == null) return null;
            //Get Reactivate Site Options
            personIntakeDetails.ReactivateSiteOptions = GetReactivateSiteoptions(weekEnder);

            //Get Active Booking 
            personIntakeDetails.ActiveBooking = GetActiveBooking(arrestId, bookingNumber);
            return personIntakeDetails;
        }

        public List<ActiveCommitDetails> GetActiveCommitDetails(int personId)
        {
            //To get Personnel details
            List<PersonnelVm> lstPersonnel = _context.Personnel.Select(s => new PersonnelVm
            {
                PersonnelId = s.PersonnelId,
                PersonFirstName = s.PersonNavigation.PersonFirstName,
                PersonLastName = s.PersonNavigation.PersonLastName,
                OfficerBadgeNumber = s.OfficerBadgeNum
            }).ToList();

            //SiteOptions value for PreBook Number
            bool siteOptions = _context.SiteOptions
                .Any(s => s.SiteOptionsName == SiteOptionsConstants.TURNONPREBOOKNUMBERING
                          && s.SiteOptionsValue == SiteOptionsConstants.ON
                          && s.SiteOptionsStatus == "1");

            //To get ActiveCommit Details
            List<ActiveCommitDetails> activeCommitDetails = _context.InmatePrebook
                .Where(i => i.CourtCommitFlag == 1
                            && (i.DeleteFlag == 0 || !i.DeleteFlag.HasValue)
                            && i.PersonId == personId)
                .OrderBy(i => i.PrebookDate)
                .Select(i => new ActiveCommitDetails
                {
                    PreBookNumber = i.PreBookNumber,
                    PersonFirstName = i.Person.PersonFirstName, // Case Cont For Name 
                    PersonLastName = i.Person.PersonLastName,
                    PersonMiddleName = i.Person.PersonMiddleName,
                    PersonDob = i.Person.PersonDob,
                    PreBookLastName = i.PersonLastName,
                    PreBookFirstName = i.PersonFirstName,
                    PreBookMiddleName = i.PersonMiddleName,
                    PreBookDob = i.PersonDob,
                    CaseNumber = i.CaseNumber,
                    ArrestDate = i.ArrestDate,
                    PreBookDate = i.PrebookDate, // function for Elapsed Column
                    ArrestingOfficer = lstPersonnel.Find(p => p.PersonnelId == i.ArrestingOfficerId),
                    ArrestAgency = i.ArrestAgency.AgencyAbbreviation,
                    Facility = i.Facility.FacilityAbbr,
                    Notes = i.PrebookNotes,
                    DeleteFlag = i.DeleteFlag,
                    PreBookId = i.InmatePrebookId,
                    PersonId = i.PersonId,
                    CourtCommitFlag = i.CourtCommitFlag,
                    SiteOptions = siteOptions
                }).ToList();

            activeCommitDetails.ForEach(item =>
            {
                item.InmateId = _context.Inmate.SingleOrDefault(i => i.PersonId == item.PersonId)?.InmateId;
            });

            return activeCommitDetails;
        }

        private void GetWizardFlagDetails(int activeIncarcerationId)
        {
            //To get Incarceration details
            IQueryable<Incarceration> lstincarcerationDetails = _context.Incarceration
                .Where(i => !i.ReleaseOut.HasValue && i.IncarcerationId == activeIncarcerationId);

            //To get Flag details for Intake
            _inmateBookingDetailsVm.IntakeCompleteFlag = lstincarcerationDetails
                .Where(i => i.IntakeCompleteFlag == 1)
                .Select(i => new CompleteFlagDetails
                {
                    CompleteDate = i.IntakeCompleteDate,
                    OfficerBadgeNumber = i.IntakeCompleteByNavigation.OfficerBadgeNum,
                    PersonLastName = i.IntakeCompleteByNavigation.PersonNavigation.PersonLastName
                }).SingleOrDefault();

            //To get Flag details for Booking
            _inmateBookingDetailsVm.BookingCompleteFlag = lstincarcerationDetails
                .Where(i => i.BookCompleteFlag == 1)
                .Select(i => new CompleteFlagDetails
                {
                    CompleteDate = i.BookCompleteDate,
                    OfficerBadgeNumber = i.BookCompleteByNavigation.OfficerBadgeNum,
                    PersonLastName = i.BookCompleteByNavigation.PersonNavigation.PersonLastName
                }).SingleOrDefault();

            //To get Flag details for BookingSupervisor
            _inmateBookingDetailsVm.SupervisorCompleteFlag = lstincarcerationDetails
                .Where(i => i.BookingSupervisorCompleteFlag == 1)
                .Select(i => new CompleteFlagDetails
                {
                    CompleteDate = i.BookingSupervisorCompleteDate,
                    OfficerBadgeNumber = i.BookingSupervisorCompleteByNavigation.OfficerBadgeNum,
                    PersonLastName = i.BookingSupervisorCompleteByNavigation.PersonNavigation.PersonLastName
                }).SingleOrDefault();

            //To get Flag details for Release
            _inmateBookingDetailsVm.ReleaseCompleteFlag = lstincarcerationDetails
                .Where(i => i.ReleaseCompleteFlag == 1)
                .Select(i => new CompleteFlagDetails
                {
                    CompleteDate = i.ReleaseCompleteDate,
                    OfficerBadgeNumber = i.ReleaseCompleteByNavigation.OfficerBadgeNum,
                    PersonLastName = i.ReleaseCompleteByNavigation.PersonNavigation.PersonLastName
                }).SingleOrDefault();
        }

        private void GetWizardStepDetails(int activeIncarcerationId, int personId, int facilityId,
            List<int> arrestIds)
        {
            //To get Intake WizardStep
            _inmateBookingDetailsVm.IntakeWizardLastStep = GetInkateWizardsdetails(facilityId,
                activeIncarcerationId, personId);

            //To get Booking WizardStep
            _inmateBookingDetailsVm.BookingWizardLastStep =
                GetBookingWizardsdetails(facilityId, activeIncarcerationId);

            _inmateBookingDetailsVm.BookingReleaseWizardLastStep =
                GetBookingReleaseWizardsdetails(facilityId, activeIncarcerationId);

            //To get SupervisorWizardStep
            _inmateBookingDetailsVm.SupervisorWizardLastStep =
                GetSupervisorWizardsdetails(facilityId, activeIncarcerationId);

            //To get ReleaseWizardStep
            _inmateBookingDetailsVm.ReleaseWizardLastStep =
                GetReleaseWizardsdetails(facilityId, activeIncarcerationId);

            //To get BookingDataWizardStep
            _inmateBookingDetailsVm.BookingDataWizardLastStep =
                GetBookingDataWizardsdetails(facilityId, arrestIds);
        }

        private List<WizardLastStepDetails> GetInkateWizardsdetails(int facilityId, int incarcerationId, int personId)
        {
            // To get Inmate Prebook details
            int inmatePrebookId = 0;
            if (incarcerationId > 0)
            {
                inmatePrebookId = _context.InmatePrebook
                .First(w => w.PersonId == personId && w.IncarcerationId.HasValue && w.IncarcerationId == incarcerationId)
                .InmatePrebookId;
            }

            // To get Intake based on facility
            List<WizardLastStepDetails> intakeVm = _context.Incarceration
                .Where(i => i.Inmate.InmateActive == 1
                            && i.Inmate.FacilityId == facilityId
                            && !i.ReleaseOut.HasValue
                            && i.IncarcerationId == incarcerationId
                )
                .Select(i => new WizardLastStepDetails
                {
                    IncarcerationId = i.IncarcerationId,
                    LastStepId = i.IntakeWizardLastStepId,
                    WizardCompleteFlag = i.IntakeCompleteFlag
                }).ToList();

            intakeVm.ForEach(ivm =>
            {
                ivm.PreBookId = inmatePrebookId;

                ivm.WizardProgressId = _context.AoWizardProgressIncarceration.SingleOrDefault(awp =>
                    awp.IncarcerationId == incarcerationId && awp.AoWizardId == 1)?.AoWizardProgressId;

                if (ivm.WizardProgressId.HasValue)
                {
                    ivm.LastStep = _context.AoWizardStepProgress
                        .Where(aws => aws.AoWizardProgressId == ivm.WizardProgressId.Value).Select(aw => new WizardStep
                        {
                            ComponentId = aw.AoComponentId,
                            WizardProgressId = aw.AoWizardProgressId,
                            WizardStepProgressId = aw.AoWizardStepProgressId,
                            StepComplete = aw.StepComplete
                        }).ToList();
                }
            });

            return intakeVm;
        }

        private List<WizardLastStepDetails> GetBookingWizardsdetails(int facilityId, int incarcerationId)
        {
            //To get Incarceration details for taking Booking wizard details
            IQueryable<Incarceration> lstIncarceration =
                _context.Incarceration.Where(inc => !inc.ReleaseOut.HasValue
                                                    && inc.Inmate.InmateActive == 1 &&
                                                    inc.Inmate.FacilityId == facilityId &&
                                                    inc.IncarcerationId == incarcerationId);

            List<WizardLastStepDetails> overallBookings = lstIncarceration.Select(inc => new WizardLastStepDetails
            {
                IncarcerationId = inc.IncarcerationId,
                BookandReleaseFlag = inc.BookAndReleaseFlag,
                WizardCompleteFlag = inc.BookCompleteFlag,
                LastStepId = inc.BookingWizardLastStepId
            }).ToList();

            overallBookings.ForEach(ovp =>
            {
                int wizardId = ovp.BookandReleaseFlag == 1 ? NumericConstants.FOUR : NumericConstants.TWO;
                ovp.WizardProgressId = _context.AoWizardProgressIncarceration.FirstOrDefault(awp =>
                    awp.IncarcerationId == ovp.IncarcerationId &&
                    awp.AoWizardId == wizardId)?.AoWizardProgressId;

                if (ovp.WizardProgressId.HasValue)
                {
                    ovp.LastStep = _context.AoWizardStepProgress
                        .Where(aws => aws.AoWizardProgressId == ovp.WizardProgressId.Value).Select(aw => new WizardStep
                        {
                            ComponentId = aw.AoComponentId,
                            WizardProgressId = aw.AoWizardProgressId,
                            WizardStepProgressId = aw.AoWizardStepProgressId,
                            StepComplete = aw.StepComplete
                        }).ToList();
                }
            });

            return overallBookings;
        }

        private List<WizardLastStepDetails> GetBookingDataWizardsdetails(int facilityId, List<int> arrestIds)
        {
            //To get Arrest details for taking Booking Data wizard details
            IQueryable<Arrest> lstArrest = _context.Arrest.Where(a =>
                    a.Inmate.InmateActive == 1
                    && a.Inmate.FacilityId == facilityId
                    && arrestIds.Contains(a.ArrestId));

            List<WizardLastStepDetails> overallBookingsData = lstArrest.Select(a => new WizardLastStepDetails
            {
                WizardCompleteFlag = a.BookingCompleteFlag,
                LastStepId = a.BookingWizardLastStepId,
                ArrestId = a.ArrestId
            }).ToList();

            List<AoWizardProgressArrest> lstAoWizardProgress = _context.AoWizardProgressArrest.Where(awp =>
                overallBookingsData.Select(i => i.ArrestId).Contains(awp.ArrestId) &&
                awp.AoWizardId == NumericConstants.FOURTEEN).ToList();

            List<WizardStep> lstAoWizardStepProgress = _context.AoWizardStepProgress
                .Where(aws => lstAoWizardProgress.Select(i => i.AoWizardProgressId).Contains(aws.AoWizardProgressId))
                .Select(aw => new WizardStep
                {
                    ComponentId = aw.AoComponentId,
                    WizardProgressId = aw.AoWizardProgressId,
                    WizardStepProgressId = aw.AoWizardStepProgressId,
                    StepComplete = aw.StepComplete
                }).ToList();

            //IEnumerable<WizardStepVm> lstAppAoWizardSteps = _commonService.GetWizardStep().ToList();
            overallBookingsData.ForEach(ovp =>
            {
                ovp.WizardProgressId = lstAoWizardProgress.FirstOrDefault(awp => awp.ArrestId == ovp.ArrestId)
                    ?.AoWizardProgressId;

                if (ovp.WizardProgressId.HasValue)
                {
                    ovp.LastStep = lstAoWizardStepProgress
                        .Where(aws => aws.WizardProgressId == ovp.WizardProgressId.Value).ToList();
                }
            });

            return overallBookingsData;
        }

        private List<WizardLastStepDetails> GetBookingReleaseWizardsdetails(int facilityId, int incarcerationId)
        {
            //To get Incarceration details for taking Booking Release wizard details
            List<WizardLastStepDetails> lstRelease = _context.Incarceration.Where(inc => !inc.ReleaseOut.HasValue
                 && inc.Inmate.FacilityId == facilityId && inc.IncarcerationId == incarcerationId).Select(lstInc =>
                new WizardLastStepDetails
                {
                    IncarcerationId = lstInc.IncarcerationId,
                    BookandReleaseFlag = lstInc.BookAndReleaseFlag,
                    LastStepId = lstInc.BookAndReleaseWizardLastStepId,
                    WizardCompleteFlag = lstInc.Inmate.InmateActive == 1 ? 0 : 1
                }).ToList();

            lstRelease.ForEach(rel =>
            {
                rel.WizardProgressId = _context.AoWizardProgressIncarceration.SingleOrDefault(awp =>
                    awp.IncarcerationId == incarcerationId && awp.AoWizardId == 1)?.AoWizardProgressId;

                if (rel.WizardProgressId.HasValue)
                {
                    rel.LastStep = _context.AoWizardStepProgress.Where(aws =>
                       aws.AoWizardProgressId == rel.WizardProgressId.Value).Select(aw => new WizardStep
                       {
                           ComponentId = aw.AoComponentId,
                           WizardProgressId = aw.AoWizardProgressId,
                           WizardStepProgressId = aw.AoWizardStepProgressId,
                           StepComplete = aw.StepComplete
                       }).ToList();
                }
            });

            return lstRelease;
        }

        private List<WizardLastStepDetails> GetSupervisorWizardsdetails(int facilityId, int incarcerationId)
        {
            //To get Incarceration details for taking Suoervisor wizard details
            List<WizardLastStepDetails> lstSupervisor = _context.Incarceration.Where(inc =>
                !inc.ReleaseOut.HasValue && inc.Inmate.InmateActive == 1 && inc.Inmate.FacilityId == facilityId &&
                inc.IncarcerationId == incarcerationId).Select(lstInc => new WizardLastStepDetails
                {
                    IncarcerationId = lstInc.IncarcerationId,
                    LastStepId = lstInc.BookingSupervisorWizardLastStepId,
                    WizardCompleteFlag = lstInc.BookingSupervisorCompleteFlag
                }).ToList();

            lstSupervisor.ForEach(rel =>
            {
                rel.WizardProgressId = _context.AoWizardProgressIncarceration.SingleOrDefault(awp =>
                    awp.IncarcerationId == incarcerationId &&
                    awp.AoWizardId == 1)?.AoWizardProgressId;

                if (rel.WizardProgressId.HasValue)
                {
                    rel.LastStep = _context.AoWizardStepProgress
                        .Where(aws => aws.AoWizardProgressId == rel.WizardProgressId.Value).Select(aw => new WizardStep
                        {
                            ComponentId = aw.AoComponentId,
                            WizardProgressId = aw.AoWizardProgressId,
                            WizardStepProgressId = aw.AoWizardStepProgressId,
                            StepComplete = aw.StepComplete
                        }).ToList();
                }
            });
            return lstSupervisor;
        }

        private List<WizardLastStepDetails> GetReleaseWizardsdetails(int facilityId, int incarcerationId)
        {
            //To get Incarceration details for taking Release wizard details
            List<WizardLastStepDetails> lstRelease = _context.Incarceration.Where(inc => !inc.ReleaseOut.HasValue &&
                inc.Inmate.InmateActive == 1 && inc.Inmate.FacilityId == facilityId &&
                inc.IncarcerationId == incarcerationId).Select(lstInc => new WizardLastStepDetails
                {
                    IncarcerationId = lstInc.IncarcerationId,
                    LastStepId = lstInc.ReleaseWizardLastStepId,
                    WizardCompleteFlag = lstInc.ReleaseCompleteFlag
                }).ToList();

            lstRelease.ForEach(rel =>
            {
                rel.WizardProgressId = _context.AoWizardProgressIncarceration.SingleOrDefault(awp =>
                    awp.IncarcerationId == incarcerationId &&
                    awp.AoWizardId == 1)?.AoWizardProgressId;

                if (rel.WizardProgressId.HasValue)
                {
                    rel.LastStep = _context.AoWizardStepProgress
                        .Where(aws => aws.AoWizardProgressId == rel.WizardProgressId.Value).Select(aw => new WizardStep
                        {
                            ComponentId = aw.AoComponentId,
                            WizardProgressId = aw.AoWizardProgressId,
                            WizardStepProgressId = aw.AoWizardStepProgressId,
                            StepComplete = aw.StepComplete
                        }).ToList();
                }
            });
            return lstRelease;
        }

        public async Task<int> DeleteInmateActiveCommit(int inmatePrebookId)
        {
            //Update InmatePrebook
            InmatePrebook updateInmatePrebook =
                _context.InmatePrebook.Single(r => r.InmatePrebookId == inmatePrebookId);
            updateInmatePrebook.DeleteFlag = 1;
            updateInmatePrebook.DeletedBy = _personnelId;
            updateInmatePrebook.DeleteDate = DateTime.Now;

            return await _context.SaveChangesAsync();
        }

        public async Task<int> UndoInmateBooking(InmateBookingData inmateData)
        {
            UndoClearBooking(inmateData.ArrestId, inmateData.IncarcerationArrestXrefId);

            UpdateReleaseClearFlag(inmateData.IncarcerationId);

            //Updating the Bail amount
            if (inmateData.ArrestId > 0)
            {
                _bookingReleaseService.CalculateBailTotalAmount(inmateData.ArrestId, inmateData.PersonId,
                    inmateData.DoNotDoSaveHistory, inmateData.BailTransaction);
            }

            return await _context.SaveChangesAsync();
        }

        public async Task<int> UndoInmateBookingPopup(InmateBookingData inmateData)
        {
            //Undo Inmate Booking for Booking master popup
            UndoClearBooking(inmateData.ArrestId, inmateData.IncarcerationArrestXrefId);

            UpdateReleaseClearFlag(inmateData.IncarcerationId);

            return await _context.SaveChangesAsync();
        }

        private void UpdateReleaseClearFlag(int incarcerationId)
        {
            //To get Incarceration table for updating the columns
            Incarceration updateIncarceration =
                _context.Incarceration.Single(i => i.IncarcerationId == incarcerationId);

            if (_context.IncarcerationArrestXref
                .Any(i => i.IncarcerationId == incarcerationId && !i.ReleaseDate.HasValue))
            {
                updateIncarceration.ReleaseClearFlag = null;
                updateIncarceration.ReleaseClearBy = null;
                updateIncarceration.ReleaseClearDate = null;
                updateIncarceration.ReleaseWizardLastStepId = null;
                updateIncarceration.ReleaseCompleteFlag = null;
                updateIncarceration.ReleaseSupervisorCompleteFlag = null;
                updateIncarceration.ReleaseSupervisorWizardLastStepId = null;
            }
            else
            {
                updateIncarceration.ReleaseClearFlag = 1;
                updateIncarceration.ReleaseClearBy = _personnelId;
                updateIncarceration.ReleaseClearDate = DateTime.Now;
            }
        }

        private void UndoClearBooking(int arrestId, int incarcerationArrestXrefId)
        {
            //To get Arrest table for updating the columns
            Arrest updateArrest = _context.Arrest.Single(a => a.ArrestId == arrestId);

            updateArrest.UpdateDate = null;
            updateArrest.ArrestReleaseClearedBy = null;
            updateArrest.ArrestReleaseClearedDate = null;
            updateArrest.ArrestActive = 1;

            //To get IncarcerationArrestXref table for updating the columns
            IncarcerationArrestXref updateIncarcerationArrestXref = _context.IncarcerationArrestXref.Single(i =>
                i.IncarcerationArrestXrefId == incarcerationArrestXrefId);

            updateIncarcerationArrestXref.ReleaseNotes = null;
            updateIncarcerationArrestXref.ReleaseReason = null;
            updateIncarcerationArrestXref.ReleaseDate = null;
            updateIncarcerationArrestXref.ReleaseOfficerId = null;
            updateIncarcerationArrestXref.ReleaseSupervisorCompleteFlag = null;
            updateIncarcerationArrestXref.ReleaseSupervisorWizardLastStepId = null;
        }

        public async Task<int> UndoReleaseInmateBooking(InmateBookingData inmateData)
        {
            //To get Incarceration table for updating the columns
            Incarceration updateIncarceration =
                _context.Incarceration.Single(i => i.IncarcerationId == inmateData.IncarcerationId);

            updateIncarceration.ReleaseOut = null;
            updateIncarceration.ReleaseSupervisorCompleteFlag = null;
            updateIncarceration.ReleaseSupervisorWizardLastStepId = null;
            updateIncarceration.ReleaseCompleteFlag = null;
            updateIncarceration.ReleaseWizardLastStepId = null;
            updateIncarceration.ReleaseClearFlag = null;
            updateIncarceration.ReleaseCompleteDate = null;
            updateIncarceration.ReleaseCompleteBy = null;
            updateIncarceration.OutOfficerId = null;

            //To update InmateActive from Inmate table
            Inmate updateInmate = _context.Inmate.Single(i => i.InmateId == inmateData.InmateId);
            updateInmate.InmateActive = 1;

            return await _context.SaveChangesAsync();
        }

        #endregion

        #region Inmate Booking Info

        public InmateBookingInfoVm GetInmateBookingInfo(int arrestId, int incarcerationArrestXrefId)
        {
            //Agency List
            List<Agency> agencyList = _context.Agency.OrderBy(a => a.AgencyName).
                Select(s => new Agency
                {
                    AgencyId = s.AgencyId,
                    AgencyName = s.AgencyName,
                    AgencyArrestingFlag = s.AgencyArrestingFlag,
                    AgencyBillingExceptionFlag = s.AgencyBillingExceptionFlag,
                    AgencyInactiveFlag = s.AgencyInactiveFlag,
                    AgencyBillingFlag = s.AgencyBillingFlag,
                    AgencyBookingFlag = s.AgencyBookingFlag,
                    AgencyCourtFlag = s.AgencyCourtFlag
                }).ToList();

            InmateBookingInfoVm bookingInfo = new InmateBookingInfoVm
            {
                //Arresting Agency
                ArrestingAgency = agencyList.Where(a => a.AgencyArrestingFlag && !a.AgencyCourtFlag)
                    .Select(a => new KeyValuePair<int, string>(a.AgencyId, a.AgencyName)).ToList(),
                //Billing Agency
                BillingAgency = agencyList.Where(a =>
                        !a.AgencyBillingExceptionFlag && (!a.AgencyInactiveFlag.HasValue || a.AgencyInactiveFlag == 0) &&
                        a.AgencyBillingFlag)
                    .Select(a => new KeyValuePair<int, string>(a.AgencyId, a.AgencyName)).ToList(),
                //Booking Agency
                BookingAgency = agencyList.Where(a => a.AgencyBookingFlag)
                    .Select(a => new KeyValuePair<int, string>(a.AgencyId, a.AgencyName)).ToList(),
                //Originating Agency
                OriginatingAgency = agencyList
                    .Where(a => !a.AgencyCourtFlag)
                    .Select(a => new KeyValuePair<int, string>(a.AgencyId, a.AgencyName)).ToList(),
                //court
                Court = agencyList.Where(a => a.AgencyCourtFlag &&
                        (!a.AgencyInactiveFlag.HasValue || a.AgencyInactiveFlag == 0))
                    .Select(a => new KeyValuePair<int, string>(a.AgencyId, a.AgencyName)).ToList()
            };
            //Lookup List
            List<Lookup> lstLookUp = _context.Lookup.Where(l =>
                (l.LookupType == LookupConstants.ARRTYPE || l.LookupType == LookupConstants.BOOKCONDCLEAR) && l.LookupInactive == 0).ToList();
            //clearance flags
            bookingInfo.CondReleaseFlag = lstLookUp.Where(l => l.LookupType == LookupConstants.BOOKCONDCLEAR)
                .Select(l => l.LookupDescription).ToArray();

            bookingInfo.ArrestCondClear = _context.ArrestCondClear
                .Where(a => a.ArrestId == arrestId && a.DeleteFlag != 1).Select(i => new ClearanceFlag
                {
                    CondOfClearance = i.CondOfClearance,
                    CondOfClearanceNote = i.CondOfClearanceNote,
                    Flag = true
                }).ToList();

            if (bookingInfo.ArrestCondClear.Count != bookingInfo.CondReleaseFlag.Length)
            {
                foreach (string s in bookingInfo.CondReleaseFlag)
                {
                    if (bookingInfo.ArrestCondClear.Select(i => i.CondOfClearance).Contains(s)) continue;
                    bookingInfo.ArrestCondClear.Add(new ClearanceFlag
                    {
                        CondOfClearance = s,
                        CondOfClearanceNote = "",
                        Flag = false
                    });
                }
            }

            //arrest details
            bookingInfo.ArrestDetails = _context.IncarcerationArrestXref.Where(i =>
                i.ArrestId == arrestId && i.IncarcerationArrestXrefId == incarcerationArrestXrefId).Select(a =>
                    new BookingInfoArrestDetails
                    {
                        ArrestId = a.Arrest.ArrestId,
                        BookingNumber = a.Arrest.ArrestBookingNo,
                        BookingDate = a.BookingDate,
                        InmateNumber = a.Arrest.Inmate.InmateNumber,
                        ArrestDate = a.Arrest.ArrestDate,
                        Pcn = a.Arrest.ArrestPcn,
                        FingerprintByDoj = a.Arrest.ArrestFingerprintByDoj == "1",
                        Type = a.Arrest.ArrestType.Trim(),
                        LawEnforcementDisposition = a.Arrest.ArrestLawEnforcementDisposition,
                        NonCompliance = a.Arrest.ArrestNonCompliance,
                        ConditionsOfRelease = a.Arrest.ArrestConditionsOfRelease,
                        ArraignmentDate = a.Arrest.ArrestArraignmentDate,
                        CaseNumber = a.Arrest.ArrestCaseNumber,
                        CourtDocket = a.Arrest.ArrestCourtDocket,
                        CourtJurisdictionId = a.Arrest.ArrestCourtJurisdictionId,
                        ArrestingAgencyId = a.Arrest.ArrestingAgencyId,
                        BillingAgencyId = a.Arrest.ArrestBillingAgencyId,
                        BookingAgencyId = a.Arrest.BookingAgencyId,
                        OriginatingAgencyId = a.Arrest.OriginatingAgencyId,
                        BookingSupervisorId = a.Arrest.ArrestBookingSupervisorId,
                        BookingOfficerId = a.Arrest.ArrestBookingOfficerId,
                        ArrestOfficerId = a.Arrest.ArrestOfficerId,
                        SearchOfficerId = a.Arrest.ArrestSearchOfficerId,
                        ReceivingOfficerId = a.Arrest.ArrestReceivingOfficerId,
                        TransportingOfficerId = a.Arrest.ArrestTransportingOfficerId,
                        Notes = a.Arrest.ArrestNotes,
                        ArrestOfficerText = a.Arrest.ArrestOfficerText,
                        TransportingOfficerText = a.Arrest.ArrestTransportingOfficerText,
                        SiteBookingNo = a.Arrest.ArrestSiteBookingNo.Trim(),
                        InmateSiteNumber = a.Arrest.Inmate.InmateSiteNumber.Trim(),
                        Location = a.Arrest.ArrestLocation
                    }).SingleOrDefault();
            if (bookingInfo.ArrestDetails != null)
            {
                bookingInfo.ArrestDetails.Type = lstLookUp.SingleOrDefault(l =>
                        Convert.ToString(l.LookupIndex) == bookingInfo.ArrestDetails.Type &&
                        l.LookupType == LookupConstants.ARRTYPE)?.LookupIndex.ToString();
                //If ArrestingAgencyId is not in ArrestingAgency list=>defaultly we will take as below
                KeyValuePair<int, string> arrAgency =
                    bookingInfo.ArrestingAgency.Find(i => i.Key == bookingInfo.ArrestDetails.ArrestingAgencyId);
                if (arrAgency.Value == null)
                {
                    int personId = _context.Arrest.Where(w => w.ArrestId == arrestId).Select(i => i.Inmate.PersonId)
                        .SingleOrDefault();
                    if (personId > 0)
                    {
                        int[] prebook = _context.InmatePrebook.Where(i => i.PersonId == personId)
                            .Select(s => s.ArrestAgencyId).ToArray();
                        bookingInfo.ArrestDetails.ArrestingAgencyId = agencyList.Where(a =>
                                prebook.Contains(a.AgencyId) && a.AgencyArrestingFlag &&
                                (!a.AgencyInactiveFlag.HasValue || a.AgencyInactiveFlag == 0) &&
                                !a.AgencyCourtFlag)
                            .Select(a => a.AgencyId).FirstOrDefault();
                    }
                }
                //If BookingAgencyId is not in BookingAgency list=>defaultly we will take as below
                KeyValuePair<int, string> bookingAgency =
                    bookingInfo.BookingAgency.Find(i => i.Key == bookingInfo.ArrestDetails.BookingAgencyId);
                if (bookingAgency.Value == null)
                {
                    int agencyId = _context.Personnel.Single(w => w.PersonnelId == _personnelId).AgencyId;
                    if (agencyId > 0)
                    {
                        bookingInfo.ArrestDetails.BookingAgencyId = agencyList.Where(a => a.AgencyId == agencyId &&
                            (!a.AgencyInactiveFlag.HasValue || a.AgencyInactiveFlag == 0) &&
                            a.AgencyBookingFlag).Select(a => a.AgencyId).FirstOrDefault();
                    }
                }
            }
            bookingInfo.SiteOptions = SiteOptions();
            return bookingInfo;
        }

        public List<HistoryVm> GetBookingInfoHistory(int arrestId)
        {
            List<HistoryVm> bookingInfoHistory = _context.ArrestInfoHistory.Where(a => a.ArrestId == arrestId).Select(
                a => new HistoryVm
                {
                    HistoryId = a.ArrestInfoHistoryId,
                    CreateDate = a.CreateDate,
                    OfficerBadgeNumber = a.Personnel.OfficerBadgeNum,
                    PersonId = a.Personnel.PersonId,
                    HistoryList = a.ArrestInfoHistoryList
                }).OrderByDescending(i => i.CreateDate).ToList();

            int[] personIds = bookingInfoHistory.Select(p => p.PersonId).ToArray();

            List<Person> lstPersonDetails = _context.Person.Where(p => personIds.Contains(p.PersonId)).Select(p =>
                new Person
                {
                    PersonId = p.PersonId,
                    PersonLastName = p.PersonLastName
                }).ToList();

            bookingInfoHistory.ForEach(item =>
            {
                item.PersonLastName =
                    lstPersonDetails.SingleOrDefault(p => p.PersonId == item.PersonId)?.PersonLastName;

                if (item.HistoryList == null) return;
                Dictionary<string, string> personHistoryList =
                    JsonConvert.DeserializeObject<Dictionary<string, string>>(item.HistoryList);
                item.Header = personHistoryList.Select(ph => new PersonHeader { Header = ph.Key, Detail = ph.Value })
                    .ToList();
            });
            return bookingInfoHistory;
        }

        public int PostUpdateSearchOfficiers(string search, int arrestId)
        {
            Arrest arrest = _context.Arrest.Single(a => a.ArrestId == arrestId);
            {
                arrest.UpdateBy = _personnelId;
                arrest.UpdateDate = DateTime.Now;
                arrest.ArrestSearchOfficerAddList = search;
            }
            return _context.SaveChanges();
        }

        public async Task<int> UpdateBookingInfo(BookingInfoArrestDetails arrDetails)
        {
            //update arrest table
            Arrest arrest = _context.Arrest.Single(a => a.ArrestId == arrDetails.ArrestId);
            {
                arrest.ArrestDate = arrDetails.ArrestDate;
                arrest.ArrestSiteBookingNo = arrDetails.SiteBookingNo;
                arrest.ArrestPcn = arrDetails.Pcn;
                arrest.ArrestFingerprintByDoj = arrDetails.FingerprintByDoj ? "1" : "0";
                arrest.ArrestLocation = arrDetails.Location;
                arrest.ArrestType = arrDetails.Type;
                arrest.ArrestLawEnforcementDisposition = arrDetails.LawEnforcementDisposition;
                arrest.ArrestNonCompliance = arrDetails.NonCompliance;
                arrest.ArrestConditionsOfRelease = arrDetails.ConditionsOfRelease;
                arrest.ArrestCourtJurisdictionId = arrDetails.CourtJurisdictionId;
                arrest.ArrestArraignmentDate = arrDetails.ArraignmentDate;
                arrest.ArrestCourtDocket = arrDetails.CourtDocket;
                arrest.ArrestCaseNumber = arrDetails.CaseNumber;
                arrest.ArrestBookingSupervisorId = arrDetails.BookingSupervisorId;
                arrest.ArrestBookingOfficerId = arrDetails.BookingOfficerId;
                arrest.ArrestOfficerId = arrDetails.ArrestOfficerId;
                arrest.ArrestOfficerText = arrDetails.ArrestOfficerText;
                arrest.ArrestReceivingOfficerId = arrDetails.ReceivingOfficerId;
                arrest.ArrestSearchOfficerId = arrDetails.SearchOfficerId;
                arrest.ArrestTransportingOfficerId = arrDetails.TransportingOfficerId;
                arrest.ArrestTransportingOfficerText = arrDetails.TransportingOfficerText;
                arrest.ArrestingAgencyId = arrDetails.ArrestingAgencyId;
                arrest.OriginatingAgencyId = arrDetails.OriginatingAgencyId;
                arrest.ArrestBillingAgencyId = arrDetails.BillingAgencyId;
                arrest.BookingAgencyId = arrDetails.BookingAgencyId;
                arrest.ArrestNotes = arrDetails.Notes;
                arrest.UpdateBy = _personnelId;
                arrest.UpdateDate = DateTime.Now;
                arrest.ArrestSearchOfficerAddList = arrDetails.ArrestSearchOfficerAddList;
            }

            Inmate inmate = _context.Inmate.Single(i => i.InmateId == arrDetails.InmateId);
            {
                inmate.InmateSiteNumber = arrDetails.InmateSiteNumber;
            }
            _interfaceEngineService.Export(new ExportRequestVm
            {
                EventName = EventNameConstants.CASEINFORMATIONSAVE,
                PersonnelId = _personnelId,
                Param1 = inmate.PersonId.ToString(),
                Param2 = arrDetails.ArrestId.ToString()
            });
            //insert data to arrest info history 
            ArrestInfoHistory infoHistory = new ArrestInfoHistory
            {
                ArrestId = arrDetails.ArrestId,
                PersonnelId = _personnelId,
                CreateDate = DateTime.Now,
                ArrestInfoHistoryList = arrDetails.ArrestInfoHistoryList
            };
            _context.ArrestInfoHistory.Add(infoHistory);

            arrDetails.ClearanceList.ForEach(i =>
            {
                ArrestCondClear cond = _context.ArrestCondClear.SingleOrDefault(c =>
                    c.ArrestId == arrDetails.ArrestId && c.CondOfClearance == i.CondOfClearance);
                if (cond == null)
                {
                    if (!i.Flag) return;
                    //insert data to ArrestCondClear table
                    ArrestCondClear arrestCond = new ArrestCondClear
                    {
                        ArrestId = arrDetails.ArrestId,
                        CreateBy = _personnelId,
                        CreateDate = DateTime.Now,
                        CondOfClearance = i.CondOfClearance,
                        CondOfClearanceNote = i.CondOfClearanceNote
                    };
                    _context.ArrestCondClear.Add(arrestCond);
                }
                else
                {
                    cond.CondOfClearanceNote = i.CondOfClearanceNote;
                    cond.DeleteBy = i.Flag ? (int?)null : _personnelId; //if we put default instead of (int?)null it return 0
                    cond.DeleteDate = i.Flag ? (DateTime?)null : DateTime.Now; //if we put default instead of (DateTime?)null it return default datetime
                    cond.DeleteFlag = i.Flag ? (int?)null : 1;
                }
            });

            return await _context.SaveChangesAsync();
        }

        private List<SiteOptionProp> SiteOptions() => _context.SiteOptions.Where(sp =>
            sp.SiteOptionsStatus == "1" && (sp.SiteOptionsName == SiteOptionsConstants.REQUIREDBILLINGAGENCY ||
                  sp.SiteOptionsName == SiteOptionsConstants.REQUIREDCOURT ||
                  sp.SiteOptionsName == SiteOptionsConstants.JMSREQUIRESEARCHOFFICER ||
                  sp.SiteOptionsName == SiteOptionsConstants.JMSREQUIRERECEIVEOFFICER ||
                  sp.SiteOptionsName == SiteOptionsConstants.JMSREQUIRETRANSPORTOFFICER ||
                  sp.SiteOptionsName == SiteOptionsConstants.JMSDONOTREQUIREBOOKINGOFFICER))
                .Select(s => new SiteOptionProp
                {
                    SiteOptionVariable = s.SiteOptionsName,
                    SiteOptionValue = s.SiteOptionsValue
                }).ToList();

        #endregion

        //Get Inmate CrimeCharges and CrimeForce Details
        public List<BookingSearchSubData> GetInmateCrimeCharges(List<int> arrestIds, BookingChargeType chargeType,
            bool showDeleted)
        {
            List<BookingSearchSubData> crimeChargeDetailsLst = new List<BookingSearchSubData>();
            if (chargeType != BookingChargeType.WARRANT)
            {
                //To get Inmate Charge details from Crime table
                crimeChargeDetailsLst = _context.Crime
                    .Where(c => (showDeleted || c.CrimeDeleteFlag == 0) && arrestIds.Contains(c.ArrestId ?? 0))
                    .Select(c => new BookingSearchSubData
                    {
                        CrimeId = c.CrimeId,
                        CrimeLookupId = c.CrimeLookupId,
                        CrimeNumber = c.CrimeNumber ?? 0,
                        CrimeCount = c.CrimeCount ?? 0,
                        CrimeSection = c.CrimeLookup.CrimeSection,
                        CrimeSubSection = c.CrimeLookup.CrimeSubSection,
                        CrimeStatueCode = c.CrimeLookup.CrimeStatuteCode,
                        ArrestId = c.ArrestId ?? 0,
                        WarrantNumber = c.Warrant.WarrantNumber,
                        Type = c.CrimeLookup.CrimeCodeType,
                        ChargeQualifierLookup = c.ChargeQualifierLookup,
                        Description = c.CrimeLookup.CrimeDescription,
                        Note = c.CrimeNotes,
                        BailAmount = c.BailAmount,
                        BailFlag = c.BailNoBailFlag == 1,
                        CrimeType = c.CrimeType,
                        DeleteFlag = c.CrimeDeleteFlag,
                        CreateDate = c.CreateDate,
                        CreatedBy = c.CreatedBy,
                        CreatedPersonnel = new PersonnelVm
                        {
                            PersonLastName = c.CreatedByNavigation.PersonNavigation.PersonLastName,
                            OfficerBadgeNumber = c.CreatedByNavigation.OfficerBadgeNum
                        },
                        UpdateDate = c.UpdateDate,
                        UpdatedBy = c.UpdateBy,
                        UpdatedPersonnel = new PersonnelVm
                        {
                            PersonLastName = c.UpdateByNavigation.PersonNavigation.PersonLastName,
                            OfficerBadgeNumber = c.UpdateByNavigation.OfficerBadgeNum
                        },
                        WarrantId = c.WarrantId ?? 0,
                        BookingChargeType = BookingChargeType.CHARGE,
                        WarrantCounty = c.Warrant.WarrantCounty,
                        WarrantAgencyId = c.Warrant.WarrantAgencyId,
                        OffenceDate = c.OffenceDate
                    }).ToList();

                if (crimeChargeDetailsLst.Any() && showDeleted) // showDeleted should be true, When we call from JMS
                { // Get CrimeGroupId from CrimeLookup.
                    int[] crimeLookupIds = crimeChargeDetailsLst.Select(cc => cc.CrimeLookupId).ToArray();
                    List<KeyValuePair<int, int>> dbCrimeLookup = _context.CrimeLookup.Where(cl => crimeLookupIds.Contains(cl.CrimeLookupId))
                            .Select(cl => new KeyValuePair<int, int>(cl.CrimeLookupId, cl.CrimeGroupId ?? 0)).ToList();
                    crimeChargeDetailsLst.ForEach(ccd =>
                    {
                        ccd.CrimeGroupId = dbCrimeLookup.Single(cl => cl.Key == ccd.CrimeLookupId).Value;
                    });
                }

                //To get Inmate Charge details from CrimeForce table
                crimeChargeDetailsLst.AddRange(_context.CrimeForce
                    .Where(c => (!c.DropChargeFlag.HasValue || c.DropChargeFlag == 0) &&
                                (!c.ForceCrimeLookupId.HasValue || c.ForceCrimeLookupId == 0) &&
                                (!c.SearchCrimeLookupId.HasValue || c.SearchCrimeLookupId == 0)
                                && (showDeleted || c.DeleteFlag == 0) && arrestIds.Contains(c.ArrestId ?? 0))
                    .Select(c => new BookingSearchSubData
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
                        CreatedPersonnel = new PersonnelVm
                        {
                            PersonLastName = c.CreateByNavigation.PersonNavigation.PersonLastName,
                            OfficerBadgeNumber = c.CreateByNavigation.OfficerBadgeNum
                        },
                        UpdateDate = c.UpdateDate,
                        UpdatedBy = c.UpdateBy,
                        UpdatedPersonnel = new PersonnelVm
                        {
                            PersonLastName = c.UpdateByNavigation.PersonNavigation.PersonLastName,
                            OfficerBadgeNumber = c.UpdateByNavigation.OfficerBadgeNum
                        },
                        CrimeForceId = c.CrimeForceId,
                        DeleteFlag = c.DeleteFlag,
                        CrimeGroupId = int.Parse(c.TempCrimeGroup),
                        ChargeQualifierLookup = c.ChargeQualifierLookup,
                        WarrantId = c.WarrantId ?? 0,
                        BookingChargeType = BookingChargeType.CHARGE,
                        WarrantCounty = c.Warrant.WarrantCounty,
                        OffenceDate = c.OffenceDate,
                        WarrantAgencyId = c.Warrant.WarrantAgencyId
                    }).ToList());

                //Adding the Status & Qualifier column for InmateBookingChargesDetails from CrimeForce table
                if (crimeChargeDetailsLst.Count > 0)
                {
                    //To get LookUp details
                    List<Lookup> lstLookUp = _context.Lookup.Where(
                        x => (x.LookupType.Contains(LookupConstants.CHARGEQUALIFIER) ||
                              x.LookupType.Contains(LookupConstants.CRIMETYPE)) && x.LookupInactive == 0).ToList();

                    crimeChargeDetailsLst.ForEach(cf =>
                    {
                        cf.Status = lstLookUp
                            .Where(l => l.LookupType == LookupConstants.CRIMETYPE && l.LookupIndex == Convert.ToInt32(cf.CrimeType))
                            .Select(l => l.LookupDescription).SingleOrDefault();

                        cf.Qualifier = !string.IsNullOrEmpty(cf.ChargeQualifierLookup)
                            ? lstLookUp.Where(l => l.LookupType == LookupConstants.CHARGEQUALIFIER &&
                                            l.LookupIndex == Convert.ToInt32(cf.ChargeQualifierLookup))
                                .Select(l => l.LookupDescription).FirstOrDefault()
                            : string.Empty;
                    });
                }
            }

            if (chargeType != BookingChargeType.CHARGE)
            {
                //To get Booking Charge details from Warrant table 
                List<BookingSearchSubData> warrantDetailLst = _context.Warrant
                    .Where(w => arrestIds.Contains(w.ArrestId ?? 0))
                    .Select(w => new BookingSearchSubData
                    {
                        WarrantId = w.WarrantId,
                        ArrestId = w.ArrestId ?? 0,
                        AgencyId = w.WarrantAgencyId ?? 0,
                        WarrantNumber = w.WarrantNumber,
                        Type = w.WarrantType,
                        Description = w.WarrantDescription,
                        WarrantDate = w.WarrantDate,
                        WarrantChargeLevel = w.WarrantChargeType,
                        County = w.WarrantCounty,
                        BailAmount = w.WarrantBailAmount,
                        BailFlag = w.WarrantBailType == BailType.NOBAIL,
                        Note = w.WarrantNotes,
                        CreateDate = w.CreateDate,
                        BookingChargeType = BookingChargeType.WARRANT,
                        WarrantCounty = w.WarrantCounty,
                        WarrantAgencyId = w.WarrantAgencyId
                    }).ToList();

                if (warrantDetailLst.Count > 0 && chargeType == BookingChargeType.WARRANT)
                    warrantDetailLst.ForEach(wa =>
                    {
                        wa.ChargeDetails = GetWarrantChargeDetails(wa.WarrantId);
                    });
                crimeChargeDetailsLst.AddRange(warrantDetailLst);
            }

            crimeChargeDetailsLst.ForEach(item =>
            {
                item.WarrantAgencyText = _context.Agency.SingleOrDefault(s => s.AgencyId == item.WarrantAgencyId)?.AgencyName;
            });

            return crimeChargeDetailsLst;
        }

        //Get Crime and CrimeForce History Details
        public List<BookingSearchSubData> GetCrimeHistory(int crimeId, int crimeForceId, int prebookChargeId)
        {
            List<BookingSearchSubData> crimeHistoryLst = new List<BookingSearchSubData>();
            if (crimeId > 0 || prebookChargeId > 0) //Crime History Details
            {
                crimeHistoryLst = _context.CrimeHistory.Where(x => crimeId > 0
                    ? x.CrimeId == crimeId
                    : x.InmatePrebookChargeId == prebookChargeId)
                .Select(y => new BookingSearchSubData
                {
                    CrimeHistoryId = y.CrimeHistoryId,
                    CrimeNumber = y.Crime.CrimeNumber ?? 0,
                    CrimeCount = y.CrimeCount ?? 0,
                    CrimeType = y.CrimeType,
                    Note = y.CrimeNotes,
                    CreateDate = y.CreatDate,
                    CreatedBy = y.CreatedBy,
                    DeleteFlag = y.CrimeDeleteFlag ?? 0,
                    BailAmount = y.BailAmount,
                    BailFlag = y.BailNoBailFlag == 1,
                    CrimeLookupId = y.CrimeLookupId ?? 0,
                    ChargeQualifierLookup = y.ChargeQualifierLookup,
                    CrimeStatusLookup = y.CrimeStatusLookup,
                    Type = y.CrimeLookup.CrimeCodeType,
                    CrimeSection = y.CrimeLookup.CrimeSection,
                    Description = y.CrimeLookup.CrimeDescription,
                    CrimeStatueCode = y.CrimeLookup.CrimeStatuteCode,
                    CrimeStatuteId = y.CrimeLookup.CrimeStatuteId
                }).ToList();
            }
            else if (crimeForceId > 0) //CrimeForce History Details
            {
                crimeHistoryLst = _context.CrimeHistory.Where(x => x.CrimeForceId == crimeForceId)
                .Select(y => new BookingSearchSubData
                {
                    CrimeHistoryId = y.CrimeHistoryId,
                    CrimeForceId = y.CrimeForce.CrimeForceId,
                    CrimeNumber = y.Crime.CrimeNumber ?? 0,
                    CrimeCount = y.CrimeCount ?? 0,
                    CrimeType = y.CrimeType,
                    Note = y.CrimeNotes,
                    CreateDate = y.CreatDate,
                    CreatedBy = y.CreatedBy,
                    DeleteFlag = y.CrimeDeleteFlag ?? 0,
                    BailAmount = y.BailAmount,
                    BailFlag = y.BailNoBailFlag == 1,
                    CrimeLookupId = y.CrimeLookupId ?? 0,
                    CrimeLookupForceString = y.CrimeLookupForceString,
                    ChargeQualifierLookup = y.ChargeQualifierLookup,
                    CrimeStatusLookup = y.CrimeStatusLookup,
                    CrimeStatueCode = y.CrimeForce.TempCrimeStatuteCode
                }).ToList();
            }

            List<int> personnelIds = crimeHistoryLst.Select(cr => cr.CreatedBy).ToList();
            List<PersonnelVm> dbPersonDetails = _context.Personnel.Where(per => personnelIds.Contains(per.PersonnelId))
                    .Select(p => new PersonnelVm
                    {
                        PersonnelId = p.PersonnelId,
                        PersonFirstName = p.PersonNavigation.PersonFirstName,
                        PersonLastName = p.PersonNavigation.PersonLastName,
                        OfficerBadgeNumber = p.OfficerBadgeNum
                    }).ToList();
            List<Lookup> crimeTypeLookupLst = _commonService.GetLookupList(LookupConstants.CRIMETYPE);
            List<Lookup> qualifierLookupLst = _commonService.GetLookupList(LookupConstants.CHARGEQUALIFIER);

            crimeHistoryLst.ForEach(a =>
            {
                a.Status = !string.IsNullOrEmpty(a.CrimeType)
                    ? crimeTypeLookupLst
                        .FirstOrDefault(z => z.LookupIndex == Convert.ToInt32(a.CrimeType))?
                        .LookupDescription : string.Empty;

                a.Qualifier = !string.IsNullOrEmpty(a.ChargeQualifierLookup)
                    ? qualifierLookupLst
                        .FirstOrDefault(z => z.LookupIndex == Convert.ToInt32(a.ChargeQualifierLookup))?
                        .LookupDescription : string.Empty;
                a.PersonnelDetail = dbPersonDetails.SingleOrDefault(per => per.PersonnelId == a.CreatedBy);
            });

            return crimeHistoryLst.OrderByDescending(c => c.CrimeHistoryId).ToList();
        }

        public List<InmatePrebookWarrantVm> GetInmateWarrantDetails(int arrestId, int personId)
        {
            List<InmatePrebookWarrantVm> warrantDetailsLst =
                _context.Warrant.Where(wa => wa.ArrestId == arrestId && wa.PersonId == personId)
                    .Select(war => new InmatePrebookWarrantVm
                    {
                        WarrantId = war.WarrantId,
                        WarrantAgencyId = war.WarrantAgencyId,
                        WarrantNumber = war.WarrantNumber,
                        WarrantType = war.WarrantType,
                        WarrantIssueDate = war.WarrantDate,
                        WarrantDescription = war.WarrantDescription,
                        WarrantBailAmount = war.WarrantBailAmount,
                        WarrantNoBail = war.WarrantBailType == BailType.NOBAIL,
                        WarrantChargeType = war.WarrantChargeType,
                        WarrantCounty = war.WarrantCounty,
                        CreateDate = war.CreateDate,
                        OriginatingAgency = war.OriginatingAgency,
                        LocalWarrant = war.LocalWarrant
                    }).ToList();

            if (warrantDetailsLst.Count > 0)
            {
                warrantDetailsLst.ForEach(wdetail =>
                {
                    wdetail.WarrantPrebookCharges = GetWarrantChargeDetails(wdetail.WarrantId ?? 0);
                    wdetail.WarrantAgencyText = _context.Agency
                        .SingleOrDefault(age => age.AgencyId == wdetail.WarrantAgencyId)?.AgencyName;
                });
            }
            return warrantDetailsLst;
        }

        private List<PrebookCharge> GetWarrantChargeDetails(int warrantId)
        {
            List<PrebookCharge> warrantChargeLst = _context.Crime.Where(cr => cr.WarrantId == warrantId && cr.ArrestId > 0)
                .Select(cr => new PrebookCharge
                {
                    CrimeId = cr.CrimeId,
                    CrimeLookupId = cr.CrimeLookupId,
                    WarrantId = cr.WarrantId,
                    CrimeNumber = cr.CrimeNumber ?? 0,
                    CrimeCount = cr.CrimeCount ?? 0,
                    CrimeSection = cr.CrimeLookup.CrimeSection,
                    CrimeSubSection = cr.CrimeLookup.CrimeSubSection,
                    CrimeCodeType = cr.CrimeLookup.CrimeCodeType,
                    CrimeQualifierLookup = cr.ChargeQualifierLookup,
                    CrimeStatusLookup = cr.CrimeStatusLookup,
                    ChargeQualifierId = cr.ChargeQualifierLookup == null ? (int?)null : int.Parse(cr.ChargeQualifierLookup),
                    CrimeDescription = cr.CrimeLookup.CrimeDescription,
                    CrimeStatuteCode = cr.CrimeLookup.CrimeStatuteCode,
                    CrimeNotes = cr.CrimeNotes,
                    BailAmount = cr.BailAmount,
                    BailNoBailFlag = cr.BailNoBailFlag == 1,
                    CrimeType = cr.CrimeType,
                    DeleteFlag = cr.CrimeDeleteFlag == 1,
                    CreateDate = cr.CreateDate,
                    OffenceDate = cr.OffenceDate
                }).ToList();

            warrantChargeLst.AddRange(_context.CrimeForce.Where(cf => cf.WarrantId == warrantId &&
                (!cf.ForceSupervisorReviewFlag.HasValue || cf.ForceSupervisorReviewFlag == 0))
                .Select(cfe => new PrebookCharge
                {
                    InmatePrebookId = cfe.InmatePrebookId ?? 0,
                    CrimeForceId = cfe.CrimeForceId,
                    InmatePrebookWarrantId = cfe.InmatePrebookWarrantId,
                    WarrantId = cfe.WarrantId,
                    DeleteFlag = cfe.DeleteFlag == 1,
                    CrimeLookupId = cfe.ForceCrimeLookupId,
                    CrimeCount = cfe.TempCrimeCount ?? 0,
                    CrimeCodeType = cfe.TempCrimeCodeType,
                    CrimeSection = cfe.TempCrimeSection,
                    CrimeDescription = cfe.TempCrimeDescription,
                    CrimeStatuteCode = cfe.TempCrimeStatuteCode,
                    BailAmount = cfe.BailAmount,
                    BailNoBailFlag = cfe.BailNoBailFlag == 1,
                    CrimeType = cfe.TempCrimeType,
                    CrimeQualifierLookup = cfe.ChargeQualifierLookup,
                    CrimeNotes = cfe.TempCrimeNotes,
                    ChargeQualifierId = !string.IsNullOrEmpty(cfe.ChargeQualifierLookup)
                        ? int.Parse(cfe.ChargeQualifierLookup)
                        : 0,
                    CrimeStatusLookup = cfe.TempCrimeStatusLookup,
                    CrimeGroupId = int.Parse(cfe.TempCrimeGroup),
                    CreateDate = cfe.CreateDate,
                    IsForceCharge = true,
                    OffenceDate = cfe.OffenceDate
                }).ToList());

            warrantChargeLst.ForEach(a =>
            {
                a.CrimeStatus = !string.IsNullOrEmpty(a.CrimeType)
                    ? _commonService.GetLookupList(LookupConstants.CRIMETYPE)
                        .FirstOrDefault(z => z.LookupIndex == Convert.ToDouble(a.CrimeType))?.LookupDescription
                    : string.Empty;

                a.CrimeQualifier = string.IsNullOrEmpty(a.CrimeQualifierLookup) ? null
                    : _commonService.GetLookupList(LookupConstants.CHARGEQUALIFIER)
                        .FirstOrDefault(z => z.LookupIndex == Convert.ToDouble(a.CrimeQualifierLookup))?
                        .LookupDescription;
            });
            return warrantChargeLst;
        }

        public async Task<int> InsertUpdateCrimeDetails(PrebookCharge chargeDetails)
        {
            bool isExist = false;
            Crime dbCrimeDetails = _context.Crime.SingleOrDefault(cr => cr.CrimeId == chargeDetails.CrimeId);
            if (dbCrimeDetails is null)
            {
                dbCrimeDetails = new Crime
                {
                    ArrestId = chargeDetails.ArrestId,
                    CreateDate = DateTime.Now,
                    CreatedBy = _personnelId
                };
            }
            else
            {
                dbCrimeDetails.UpdateDate = DateTime.Now;
                dbCrimeDetails.UpdateBy = _personnelId;
                isExist = true;
                if (dbCrimeDetails.BailAmount != chargeDetails.BailAmount)
                {
                    _interfaceEngineService.Export(new ExportRequestVm
                    {
                        EventName = EventNameConstants.CASEBAIL,
                        PersonnelId = _personnelId,
                        Param1 = chargeDetails.EventPersonId.ToString(),
                        Param2 = dbCrimeDetails.ArrestId?.ToString()
                    });
                }
                if (dbCrimeDetails.CrimeType != chargeDetails.CrimeType)
                {
                    _interfaceEngineService.Export(new ExportRequestVm
                    {
                        EventName = EventNameConstants.CHARGESTATUS,
                        PersonnelId = _personnelId,
                        Param1 = chargeDetails.EventPersonId.ToString(),
                        Param2 = dbCrimeDetails.CrimeId.ToString()
                    });
                }
            }
            if (chargeDetails.WarrantId > 0)
            {
                dbCrimeDetails.WarrantId = chargeDetails.WarrantId;
                dbCrimeDetails.Warrant = _context.Warrant.Single(waar => waar.WarrantId == chargeDetails.WarrantId);
            }

            dbCrimeDetails.CrimeNumber = chargeDetails.CrimeNumber;
            dbCrimeDetails.CrimeLookupId = chargeDetails.CrimeLookupId ?? 0;
            dbCrimeDetails.CrimeNotes = chargeDetails.CrimeNotes;
            dbCrimeDetails.CrimeCount = chargeDetails.CrimeCount;
            dbCrimeDetails.CrimeType = chargeDetails.CrimeType;
            dbCrimeDetails.CrimeStatusLookup = chargeDetails.CrimeStatusLookup;
            dbCrimeDetails.BailAmount = chargeDetails.BailAmount;
            dbCrimeDetails.BailNoBailFlag = chargeDetails.BailNoBailFlag ? 1 : 0;
            dbCrimeDetails.BailType = chargeDetails.BailNoBailFlag ? BailType.NOBAIL : default;
            dbCrimeDetails.ChargeQualifierLookup =
                chargeDetails.ChargeQualifierId.HasValue ? chargeDetails.ChargeQualifierId.ToString() : default;
            dbCrimeDetails.CrimeQualifierLookup = chargeDetails.ChargeQualifierId;
            dbCrimeDetails.OffenceDate = chargeDetails.OffenceDate;
            if (!isExist)
            {
                _context.Crime.Add(dbCrimeDetails);
            }
            //Crime History details are added
            chargeDetails.CrimeId = dbCrimeDetails.CrimeId;
            InsertCrimeHistory(chargeDetails);

            if (!isExist)
            {
                List<Crime> crimes = _context.Crime.Where(w => w.ArrestId == chargeDetails.ArrestId).OrderBy(o => o.CrimeId).ToList();
                int maxCrimeNumber = crimes.Max(m => m.CrimeNumber ?? 0);
                crimes.ForEach(item =>
                {
                    if (item.CrimeNumber == 0)
                    {
                        maxCrimeNumber += 1;
                        item.CrimeNumber = maxCrimeNumber;
                    }
                });
            }

            await _context.SaveChangesAsync();
            _interfaceEngineService.Export(new ExportRequestVm
            {
                EventName = EventNameConstants.CASECHARGECHANGE,
                PersonnelId = _personnelId,
                Param1 = chargeDetails.EventPersonId.ToString(),
                Param2 = dbCrimeDetails.CrimeId.ToString()
            });
            //Update OverAll ChargeLevel
            Incarceration incarceration = _context.IncarcerationArrestXref
                .Where(i => i.Arrest.ArrestId == chargeDetails.ArrestId)
                .OrderByDescending(i => i.Incarceration.IncarcerationId)
                .Select(i => i.Incarceration).FirstOrDefault();
            if (!(incarceration is null))
            {
                _bookingReleaseService.UpdateOverAllChargeLevel(incarceration.IncarcerationId);
                if (!incarceration.ReleaseOut.HasValue)
                {
                    Inmate inmate = _context.Inmate.Single(a => a.InmateId == incarceration.InmateId);
                    _interfaceEngineService.Export(new ExportRequestVm
                    {
                        EventName = "Review Inmate Housing - Charges",
                        PersonnelId = _personnelId,
                        Param1 = inmate.PersonId.ToString(),
                        Param2 = dbCrimeDetails.CrimeId.ToString()
                    });
                    await _context.SaveChangesAsync();
                }
            }
            return dbCrimeDetails.CrimeId;
        }

        public int InsertUpdateWarrantDetails(InmatePrebookWarrantVm warrant)
        {
            bool isExist = false;
            Warrant dbWarrantDetails = _context.Warrant.SingleOrDefault(wa => wa.WarrantId == warrant.WarrantId);
            if (dbWarrantDetails is null)
            {
                dbWarrantDetails = new Warrant
                {
                    CreateDate = DateTime.Now,
                    WarrantEntered = DateTime.Now,
                    WarrantUpdatedBy = _personnelId
                };
            }
            else
            {
                isExist = true;
            }

            dbWarrantDetails.ArrestId = warrant.ArrestId;
            dbWarrantDetails.PersonId = warrant.PersonId;
            dbWarrantDetails.WarrantCounty =
                string.IsNullOrWhiteSpace(warrant.WarrantCounty)
                    ? _context.Agency.SingleOrDefault(a => a.AgencyId == warrant.WarrantAgencyId)?.AgencyName
                    : warrant.WarrantCounty;
            dbWarrantDetails.WarrantAgencyId = warrant.WarrantAgencyId;
            dbWarrantDetails.WarrantNumber = warrant.WarrantNumber;
            dbWarrantDetails.WarrantType = warrant.WarrantType;
            dbWarrantDetails.WarrantDescription = warrant.WarrantDescription;
            dbWarrantDetails.WarrantBailAmount = !warrant.WarrantNoBail ? warrant.WarrantBailAmount : 0;
            dbWarrantDetails.WarrantBailType = warrant.WarrantNoBail ? BailType.NOBAIL : string.Empty;
            dbWarrantDetails.WarrantDate = warrant.WarrantIssueDate;
            dbWarrantDetails.WarrantChargeType = warrant.WarrantChargeType;
            dbWarrantDetails.OriginatingAgency = warrant.OriginatingAgency;
            dbWarrantDetails.LocalWarrant = warrant.LocalWarrant;

            if (warrant.WarrantId > 0)
            {
                dbWarrantDetails.UpdateDate = DateTime.Now;
                dbWarrantDetails.WarrantUpdated = DateTime.Now;
                dbWarrantDetails.WarrantUpdatedBy = _personnelId;
            }

            dbWarrantDetails.WarrantEnteredBy = _personnelId;
            if (!isExist)
            {
                _context.Warrant.Add(dbWarrantDetails);
            }

            _context.SaveChanges();

            //EventVm eventVm = new EventVm
            //{
            //    PersonId = warrant.PersonId,
            //    CorresId = dbWarrantDetails.WarrantId,
            //    EventName = EventNameConstants.CASEWARRANTCHANGE
            //};
            //_commonService.EventHandle(eventVm);
            _interfaceEngineService.Export(new ExportRequestVm
            {
                EventName = EventNameConstants.CASEWARRANTCHANGE,
                PersonnelId = _personnelId,
                Param1 = warrant.PersonId.ToString(),
                Param2 = dbWarrantDetails.WarrantId.ToString()
            });
            _context.SaveChanges();

            //Update OverAll ChargeLevel
            int? incarcerationId = _context.IncarcerationArrestXref
                .Where(i => i.Arrest.ArrestId == warrant.ArrestId)
                .OrderByDescending(i => i.Incarceration.IncarcerationId)
                .Select(i => i.Incarceration.IncarcerationId).FirstOrDefault();
            if (incarcerationId > 0)
                _bookingReleaseService.UpdateOverAllChargeLevel((int)incarcerationId);
            return dbWarrantDetails.WarrantId;
        }

        public bool DeleteWarrantDetails(int warrantId)
        {
            //In here, Hard delete the warrant and warrant Reference by warrant_id(Based on Version 1.0)
            Warrant warrantDetails = _context.Warrant.SingleOrDefault(wa => wa.WarrantId == warrantId);
            if (warrantDetails == null) return true;
            int personId = warrantDetails.PersonId;
            //Delete Warrant References
            DeleteWarrantReferences(warrantId);

            // delete Warrant table records
            _context.Warrant.Remove(warrantDetails);
            _context.SaveChanges();

            //EventVm eventVm = new EventVm
            //{
            //    PersonId = personId,
            //    CorresId = warrantId,
            //    EventName = EventNameConstants.CASEWARRANTCHANGE
            //};
            //_commonService.EventHandle(eventVm);
            _interfaceEngineService.Export(new ExportRequestVm
            {
                EventName = EventNameConstants.CASEWARRANTCHANGE,
                PersonnelId = _personnelId,
                Param1 = personId.ToString(),
                Param2 = warrantId.ToString()
            });
            _context.SaveChanges();

            return true;
        }

        private void DeleteWarrantReferences(int warrantId)
        {
            //  delete crime_history table by crime_id
            List<Crime> crimeLst = _context.Crime.Where(cr => cr.WarrantId == warrantId).ToList();
            if (crimeLst.Any())
            {
                List<CrimeHistory> crimeHistoryList = _context.CrimeHistory
                    .Where(ch => crimeLst.Select(cr => cr.CrimeId).Contains(ch.CrimeId ?? 0)).ToList();
                _context.CrimeHistory.RemoveRange(crimeHistoryList);
                // delete crime table by warrant_id
                _context.Crime.RemoveRange(crimeLst);
            }

            List<InmatePrebookWarrant> inmatePrebookWarrants =
                _context.InmatePrebookWarrant.Where(ipw => ipw.WarrantId == warrantId).ToList();
            if (inmatePrebookWarrants.Any())
            {
                List<InmatePrebookCharge> inmatePrebookCharges = _context.InmatePrebookCharge.Where(ipc =>
                    inmatePrebookWarrants.Select(ipw => ipw.InmatePrebookWarrantId).Contains(ipc.InmatePrebookWarrantId ?? 0))
                    .ToList();
                if (inmatePrebookCharges.Any())
                {
                    // delete crime_history table by inmate_prebook_charge_id
                    int[] InmPreChargeIds = inmatePrebookCharges.Select(cr => cr.InmatePrebookChargeId).ToArray();

                    List<CrimeHistory> prebookCrimeHistoryList = _context.CrimeHistory.Where(ch =>
                        InmPreChargeIds.Contains((int)(ch.InmatePrebookChargeId ?? 0))).ToList();

                    _context.CrimeHistory.RemoveRange(prebookCrimeHistoryList);

                    // delete inmate_prebook_charge table by inmate_prebook_warrant_id
                    _context.InmatePrebookCharge.RemoveRange(inmatePrebookCharges);
                }
                // delete inmate_prebook_warrant where warrant_id
                _context.InmatePrebookWarrant.RemoveRange(inmatePrebookWarrants);
            }

            List<CrimeForce> crimeForces = _context.CrimeForce.Where(cf => cf.WarrantId == warrantId).ToList();
            if (crimeForces.Any())
            {
                // delete Crime_History where Crime_Force_Id
                int[] CrimeForceIds = crimeForces.Select(cr => cr.CrimeForceId).ToArray();

                List<CrimeHistory> crimeForceHistoryList = _context.CrimeHistory
                    .Where(ch => CrimeForceIds.Contains(ch.CrimeForceId ?? 0)).ToList();
                _context.CrimeHistory.RemoveRange(crimeForceHistoryList);

                // delete Crime_Force where warrant_id
                _context.CrimeForce.RemoveRange(crimeForces);
            }

            _context.SaveChanges();
        }

        public async Task<bool> DeleteUndoCrimeDetails(PrebookCharge crimeDetailReq)
        {
            Crime crimeDetails = _context.Crime.SingleOrDefault(cr => cr.CrimeId == crimeDetailReq.CrimeId);
            if (crimeDetails != null)
            {
                crimeDetails.UpdateDate = DateTime.Now;
                crimeDetails.UpdateBy = _personnelId;
                crimeDetails.CrimeDeleteFlag = crimeDetailReq.DeleteFlag ? 1 : 0;

                //Crime History details are added
                PrebookCharge chargeDetails = new PrebookCharge
                {
                    CrimeId = crimeDetails.CrimeId,
                    CrimeLookupId = crimeDetails.CrimeLookupId,
                    CrimeType = crimeDetails.CrimeType,
                    CrimeNotes = crimeDetails.CrimeNotes,
                    CrimeCount = crimeDetails.CrimeCount ?? 0,
                    DeleteFlag = crimeDetails.CrimeDeleteFlag == 1,
                    BailAmount = crimeDetails.BailAmount,
                    BailNoBailFlag = crimeDetails.BailNoBailFlag == 1,
                    ChargeQualifierId = crimeDetails.CrimeQualifierLookup,
                    CrimeStatusLookup = crimeDetails.CrimeStatusLookup
                };
                InsertCrimeHistory(chargeDetails);

                //EventVm eventVm = new EventVm
                //{
                //    PersonId = crimeDetailReq.EventPersonId,
                //    CorresId = crimeDetails.CrimeId,
                //    EventName = EventNameConstants.CASECHARGECHANGE
                //};
                //_commonService.EventHandle(eventVm);
                _interfaceEngineService.Export(new ExportRequestVm
                {
                    EventName = EventNameConstants.CASECHARGECHANGE,
                    PersonnelId = _personnelId,
                    Param1 = crimeDetailReq.EventPersonId.ToString(),
                    Param2 = crimeDetails.CrimeId.ToString()
                });
            }
            await _context.SaveChangesAsync();
            //Update OverAll ChargeLevel
            Incarceration incarceration = _context.IncarcerationArrestXref
                .Where(i => i.Arrest.ArrestId == crimeDetailReq.ArrestId)
                .OrderByDescending(i => i.Incarceration.IncarcerationId)
                .Select(i => i.Incarceration).FirstOrDefault();
            if (!(incarceration is null))
            {
                _bookingReleaseService.UpdateOverAllChargeLevel(incarceration.IncarcerationId);
                if (!incarceration.ReleaseOut.HasValue && crimeDetailReq.DeleteFlag)
                {
                    Inmate inmate = _context.Inmate.Single(a => a.InmateId == incarceration.InmateId);
                    //EventVm eventVm = new EventVm
                    //{
                    //    PersonId = inmate.PersonId,
                    //    CorresId = crimeDetails?.CrimeId ?? 0,
                    //    EventName = "Review Inmate Housing - Charges"
                    //};
                    //_commonService.EventHandle(eventVm);
                    _interfaceEngineService.Export(new ExportRequestVm
                    {
                        EventName = "Review Inmate Housing - Charges",
                        PersonnelId = _personnelId,
                        Param1 = inmate.PersonId.ToString(),
                        Param2 = (crimeDetails?.CrimeId ?? 0).ToString()
                    });
                    await _context.SaveChangesAsync();
                }
            }

            return true;
        }

        public async Task<bool> DeleteUndoCrimeForce(PrebookCharge chargeDetails)
        {
            CrimeForce crimeForce = _context.CrimeForce.SingleOrDefault(crf => crf.CrimeForceId == chargeDetails.CrimeForceId);
            if (crimeForce != null)
            {
                crimeForce.DeleteDate = DateTime.Now;
                crimeForce.DeleteBy = _personnelId;
                crimeForce.DeleteFlag = chargeDetails.DeleteFlag ? 1 : 0;

                //Crime History details are added
                PrebookCharge crimeForceDetails = new PrebookCharge
                {
                    CrimeForceId = crimeForce.CrimeForceId,
                    CrimeLookupId = crimeForce.ForceCrimeLookupId,
                    CrimeType = crimeForce.TempCrimeType,
                    CrimeNotes = crimeForce.TempCrimeNotes,
                    CrimeCount = crimeForce.TempCrimeCount ?? 0,
                    DeleteFlag = crimeForce.DeleteFlag == 1,
                    BailAmount = crimeForce.BailAmount,
                    BailNoBailFlag = crimeForce.BailNoBailFlag == 1,
                    ChargeQualifierId = crimeForce.TempCrimeQualifierLookup,
                    CrimeStatusLookup = crimeForce.TempCrimeStatusLookup
                };
                InsertCrimeHistory(crimeForceDetails);
                await _context.SaveChangesAsync();
            }
            //Update OverAll ChargeLevel
            int? incarcerationId = _context.IncarcerationArrestXref
                .Where(i => i.Arrest.ArrestId == chargeDetails.ArrestId)
                .OrderByDescending(i => i.Incarceration.IncarcerationId)
                .Select(i => i.Incarceration.IncarcerationId).FirstOrDefault();
            if (incarcerationId > 0)
                _bookingReleaseService.UpdateOverAllChargeLevel((int)incarcerationId);
            return true;
        }

        private void InsertCrimeHistory(PrebookCharge chargeDetails)
        {
            CrimeHistory crimeHistory = new CrimeHistory
            {
                CrimeId = chargeDetails.CrimeId,
                CrimeForceId = chargeDetails.CrimeForceId,
                CrimeLookupId = chargeDetails.CrimeLookupId,
                CrimeType = chargeDetails.CrimeType,
                CrimeNotes = chargeDetails.CrimeNotes,
                CrimeCount = chargeDetails.CrimeCount,
                CrimeDeleteFlag = chargeDetails.DeleteFlag ? 1 : 0,
                BailAmount = chargeDetails.BailAmount,
                BailNoBailFlag = chargeDetails.BailNoBailFlag ? 1 : 0,
                CreatDate = DateTime.Now,
                CreatedBy = _personnelId,
                ChargeQualifierLookup =
                    chargeDetails.ChargeQualifierId.HasValue ? chargeDetails.ChargeQualifierId.ToString() : default,
                CrimeQualifierLookup = chargeDetails.ChargeQualifierId,
                CrimeStatusLookup = chargeDetails.CrimeStatusLookup,
                OffenceDate = chargeDetails.OffenceDate
            };
            _context.Add(crimeHistory);
            _context.SaveChanges();
        }

        public async Task<bool> ReplicateChargeDetails(PrebookCharge chargeDetails)
        {
            for (int cnt = 1; cnt <= chargeDetails.ReplicateCount; cnt++)
            {
                await InsertUpdateCrimeDetails(chargeDetails);
            }
            return true;
        }

        //Update Max_CrimeNumber in Crime based on ArrestId and 'Current Max_Crime_Number'
        private void UpdateCrimeNumber(int arrestId)
        {
            List<KeyValuePair<int, int>> crimeNumberDetails = _context.Crime.Where(cr => cr.ArrestId == arrestId)
                .Select(cr => new KeyValuePair<int, int>(cr.CrimeId, cr.CrimeNumber ?? 0)).ToList();
            int maxCrimeNumber = crimeNumberDetails.Max(de => de.Value);
            crimeNumberDetails = crimeNumberDetails.Where(cr => cr.Value == 0).OrderBy(c => c.Key).ToList();

            crimeNumberDetails.ForEach(cr =>
            {
                Crime crime = _context.Crime.Single(c => c.CrimeId == cr.Key);
                crime.CrimeNumber = maxCrimeNumber++;
                _context.SaveChanges();
            });
        }

        //Insert InmatePrebookCharges Details To Crime
        public bool InsertPrebookChargesToCrime(List<PrebookCharge> prebookChargeLst, bool isWarrantCharge)
        {
            if (prebookChargeLst.Any())
            {
                prebookChargeLst.ForEach(pch =>
                {
                    if (pch.CrimeForceId > 0)
                    {
                        CrimeForce crimeForce = _context.CrimeForce.Single(cf => cf.CrimeForceId == pch.CrimeForceId);
                        if (crimeForce is null) return;

                        crimeForce.ArrestId = pch.ArrestId;
                        crimeForce.WarrantId = pch.WarrantId;
                        if (_commonService.GetSiteOptionValue(SiteOptionsConstants.BAILFROMPREBOOK) == SiteOptionsConstants.OFF)
                            crimeForce.BailAmount = null;
                        _context.SaveChanges();
                    }
                    else
                    {
                        int crimeId = InsertCrimeDetails(pch);
                        InmatePrebookCharge inmatePrebookCharge =
                            _context.InmatePrebookCharge.Single(ipc => ipc.InmatePrebookChargeId == pch.InmatePrebookChargeId);
                        if (inmatePrebookCharge == null) return;
                        inmatePrebookCharge.CrimeId = crimeId;
                        _context.SaveChanges();
                    }
                });

                if (!isWarrantCharge &&
                    prebookChargeLst.Any(pc => (pc.CrimeForceId == 0 || !pc.CrimeForceId.HasValue) && pc.ArrestId > 0))
                {
                    UpdateCrimeNumber(prebookChargeLst.First(pc => pc.ArrestId > 0).ArrestId ?? 0);
                }
            }
            else
            {
                return false;
            }
            return true;
        }

        private int InsertCrimeDetails(PrebookCharge chargeDetails)
        {
            bool isExist = false;
            Crime dbCrimeDetails = _context.Crime.SingleOrDefault(cr => cr.CrimeId == chargeDetails.CrimeId);
            if (dbCrimeDetails is null)
            {
                dbCrimeDetails = new Crime
                {
                    ArrestId = chargeDetails.ArrestId,
                    CreateDate = DateTime.Now,
                    CreatedBy = _personnelId
                };
            }
            else
            {
                isExist = true;
            }
            if (chargeDetails.WarrantId > 0)
            {
                dbCrimeDetails.WarrantId = chargeDetails.WarrantId;
                dbCrimeDetails.Warrant = _context.Warrant.Single(waar => waar.WarrantId == chargeDetails.WarrantId);
            }

            dbCrimeDetails.CrimeNumber = chargeDetails.CrimeNumber;
            dbCrimeDetails.CrimeLookupId = chargeDetails.CrimeLookupId ?? 0;
            dbCrimeDetails.CrimeNotes = chargeDetails.CrimeNotes;
            dbCrimeDetails.CrimeCount = chargeDetails.CrimeCount;
            dbCrimeDetails.CrimeType = chargeDetails.CrimeType;
            dbCrimeDetails.CrimeStatusLookup = chargeDetails.CrimeStatusLookup;
            dbCrimeDetails.BailAmount = chargeDetails.BailAmount;
            dbCrimeDetails.BailNoBailFlag = chargeDetails.BailNoBailFlag ? 1 : 0;
            dbCrimeDetails.BailType = chargeDetails.BailNoBailFlag ? BailType.NOBAIL : default;
            dbCrimeDetails.ChargeQualifierLookup =
                chargeDetails.ChargeQualifierId.HasValue ? chargeDetails.ChargeQualifierId.ToString() : default;
            dbCrimeDetails.CrimeQualifierLookup = chargeDetails.ChargeQualifierId;
            dbCrimeDetails.UpdateDate = DateTime.Now;
            dbCrimeDetails.UpdateBy = _personnelId;
            dbCrimeDetails.OffenceDate = chargeDetails.OffenceDate;

            if (!isExist)
            {
                _context.Crime.Add(dbCrimeDetails);
            }
            //Crime History details are added
            chargeDetails.CrimeId = dbCrimeDetails.CrimeId;
            InsertCrimeHistory(chargeDetails);
            _context.SaveChanges();

            //EventVm caseChargeEventVm = new EventVm
            //{
            //    PersonId = chargeDetails.EventPersonId,
            //    CorresId = dbCrimeDetails.CrimeId,
            //    EventName = EventNameConstants.CASECHARGECHANGE
            //};
            //_commonService.EventHandle(caseChargeEventVm);
            _interfaceEngineService.Export(new ExportRequestVm
            {
                EventName = EventNameConstants.CASECHARGECHANGE,
                PersonnelId = _personnelId,
                Param1 = chargeDetails.EventPersonId.ToString(),
                Param2 = dbCrimeDetails.CrimeId.ToString()
            });
            _context.SaveChanges();

            //Update OverAll ChargeLevel
            int? incarcerationId = _context.IncarcerationArrestXref
                .Where(i => i.Arrest.ArrestId == chargeDetails.ArrestId)
                .OrderByDescending(i => i.Incarceration.IncarcerationId)
                .Select(i => i.Incarceration.IncarcerationId).FirstOrDefault();
            if (incarcerationId > 0)
                _bookingReleaseService.UpdateOverAllChargeLevel((int)incarcerationId);
            return dbCrimeDetails.CrimeId;
        }

        //Insert InmatePrebookWarrant Details into Warrant and Charges(Crime & Crime Force)
        public bool InsertPrebookWarrantToWarrant(List<InmatePrebookWarrantVm> prebookWarrants)
        {
            if (!prebookWarrants.Any()) return false;

            prebookWarrants.ForEach(pw =>
            {
                //Update WarrantId in InmatePrebookCharge
                InmatePrebookWarrant inmatePrebookWarrant = _context.InmatePrebookWarrant
                    .Single(ipc => ipc.InmatePrebookWarrantId == pw.InmatePrebookWarrantId);
                if (inmatePrebookWarrant == null) return;
                int warrantId = InsertUpdateWarrantDetails(pw);
                inmatePrebookWarrant.WarrantId = warrantId;
                _context.SaveChanges();
                //Insert InmatePrebookCharges Details To Crime
                pw.WarrantPrebookCharges.ForEach(pc => pc.WarrantId = warrantId);
                InsertPrebookChargesToCrime(pw.WarrantPrebookCharges, true);
            });
            return true;
        }

        public async Task<int> UpdateBookSupervisorClearFlag(BookingComplete bookingComplete)
        {
            IncarcerationArrestXref updateArrest = _context.IncarcerationArrestXref.Single(i =>
                i.ArrestId == bookingComplete.ArrestId &&
                i.IncarcerationArrestXrefId == bookingComplete.IncarcerationArrestXrefId);

            updateArrest.ReleaseSupervisorCompleteFlag = bookingComplete.IsComplete ? (int?)1 : null;
            updateArrest.ReleaseSupervisorCompleteBy = _personnelId;
            updateArrest.ReleaseSupervisorCompleteDate = DateTime.Now;

            return await _context.SaveChangesAsync();
        }
        public async Task<int> UpdateBookSupervisorCompleteFlag(BookingComplete bookingComplete)
        {
            IncarcerationArrestXref updateArrest = _context.IncarcerationArrestXref.Single(i =>
                i.ArrestId == bookingComplete.ArrestId &&
                i.IncarcerationArrestXrefId == bookingComplete.IncarcerationArrestXrefId);

            updateArrest.BookingSupervisorCompleteFlag = bookingComplete.IsComplete ? (int?)1 : null;
            updateArrest.BookingSupervisorCompleteBy = _personnelId;
            updateArrest.BookingSupervisorCompleteDate = DateTime.Now;

            int result = await _context.SaveChangesAsync();
            int? bookAndReleaseFlag = _context.Incarceration
                .Single(s => s.IncarcerationId == bookingComplete.IncarcerationId).BookAndReleaseFlag;
            if (bookAndReleaseFlag == 1)
            {
                await UpdateBookSupervisorClearFlag(bookingComplete);
            };
            _interfaceEngineService.Export(new ExportRequestVm
            {
                EventName = EventNameConstants.CASEDATACOMPLETE,
                PersonnelId = _personnelId,
                Param1 = bookingComplete.PersonId.ToString(),
                Param2 = bookingComplete.ArrestId.ToString()
            });
            return result;
        }

        public async Task<int> UpdateBookDataCompleteFlag(BookingComplete bookingComplete)
        {
            //To get Arrest table for updating the columns
            Arrest updateArrest = _context.Arrest.Single(i => i.ArrestId == bookingComplete.ArrestId);

            updateArrest.BookingCompleteFlag = bookingComplete.IsComplete ? (int?)1 : null;
            updateArrest.BookingCompleteBy = _personnelId;
            updateArrest.BookingCompleteDate = DateTime.Now;

            return await _context.SaveChangesAsync();
        }

        public async Task<int> UpdateBookingCompleteFlag(BookingComplete bookingComplete)
        {
            //To get Incarceration table for updating the columns
            Incarceration updateIncarceration =
                _context.Incarceration.Single(i => i.IncarcerationId == bookingComplete.IncarcerationId);

            updateIncarceration.BookBookDataFlag = bookingComplete.IsComplete ? (int?)1 : null;
            updateIncarceration.BookBookDataBy = _personnelId;
            updateIncarceration.BookBookDataDate = DateTime.Now;

            //Updating the incarceration table and webServiceEventType
            if (bookingComplete.BookingDataFlag)
            {
                _interfaceEngineService.Export(new ExportRequestVm
                {
                    EventName = EventNameConstants.CASEDATACOMPLETE,
                    PersonnelId = _personnelId,
                    Param1 = bookingComplete.PersonId.ToString(),
                    Param2 = bookingComplete.ArrestId.ToString()
                });
            }

            return await _context.SaveChangesAsync();
        }

        public InmateBookingData GetBookingDataComplete(int inmateId, int incarcerationId, int arrestId) =>
            //getting the Booking CompleteDetails
            new InmateBookingData
            {
                BookingCompleteFlag = _context.IncarcerationArrestXref.Any(a => a.Arrest.InmateId == inmateId
                    && a.IncarcerationId == incarcerationId
                    && (a.Arrest.BookingCompleteFlag == 0 || a.Arrest.BookingCompleteFlag == null)),
                CompleteFlag = _context.Arrest.Any(a => a.ArrestId == arrestId && a.BookingCompleteFlag == 1)
            };

        public IntakeEntryVm GetSupplementalBookingDetails(int inmateId, int incarcerationId)
        {
            int arrestId = _context.IncarcerationArrestXref.OrderBy(i => i.IncarcerationArrestXrefId)
                .FirstOrDefault(i => i.Incarceration.InmateId == inmateId
                && !i.Incarceration.ReleaseOut.HasValue)?.ArrestId ?? 0;

            IntakeEntryVm supplementalBookingDetails = _context.Arrest.Where(iax => iax.ArrestId == arrestId)
                .Select(iax => new IntakeEntryVm
                {
                    ArrestAgencyId = iax.ArrestingAgencyId,
                    BookingAgencyId = iax.BookingAgencyId,
                    ArrestDate = iax.ArrestDate,
                    ArrestLocation = iax.ArrestLocation
                }).SingleOrDefault();

            if (supplementalBookingDetails == null) return null;
            supplementalBookingDetails.OverallFinalReleaseDate = _context.Incarceration
                .SingleOrDefault(i => i.IncarcerationId == incarcerationId
                && i.BookCompleteFlag.HasValue && i.OverallFinalReleaseDate.HasValue)?.OverallFinalReleaseDate;

            supplementalBookingDetails.OriginalBookingNumber = _context.Incarceration
                .Where(i => !i.ReleaseOut.HasValue && i.IncarcerationId == incarcerationId)
                .Select(i => i.BookingNo).FirstOrDefault();

            supplementalBookingDetails.IsActiveIncarceration = _context.Incarceration.Any(i =>
                i.InmateId == inmateId && !i.ReleaseOut.HasValue);
            return supplementalBookingDetails;
        }

        public bool GetCourtdateArraignment(BookingInfoArrestDetails arrestDetails)
        {
            bool status = _context.Arrest.Count(aa => aa.InmateId == arrestDetails.InmateId 
            && aa.ArrestBookingNo != arrestDetails.BookingNumber &&
            aa.ArrestArraignmentDate >= arrestDetails.ConfictArraignmentDate &&
                     aa.ArrestArraignmentDate <= arrestDetails.ConfictArraignmentDate) > 0;
            if (!status)
            {
                status = _appointmentService.ListAoAppointments(arrestDetails.FacilityId, arrestDetails.InmateId, arrestDetails.ConfictArraignmentDate,
                    arrestDetails.ConfictArraignmentDate, true).Count() > 0;
            }
            return status;
        }
    }
}
