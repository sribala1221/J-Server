using System;
using System.Collections.Generic;
using System.Linq;
using GenerateTables.Models;
using ServerAPI.ViewModels;
using ServerAPI.Utilities;
using Microsoft.AspNetCore.Http;
using JwtDb.Models;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace ServerAPI.Services
{
    public class InmateDetailsService : IInmateDetailsService
    {
        private readonly AAtims _context;
        private readonly ICommonService _commonService;
        private readonly IInmateBookingService _inmateBookingService;
        private readonly IAppointmentViewerService _appointmentViewerService;
        private readonly int _personnelId;
        private readonly JwtDbContext _jwtDbContext;
        private readonly UserManager<AppUser> _userManager;
        private readonly IPhotosService _photos;

        public InmateDetailsService(AAtims context, ICommonService commonService,
            IInmateBookingService inmateBookingService, IHttpContextAccessor httpContextAccessor, 
            JwtDbContext jwtDbContext, UserManager<AppUser> userManager,
            IAppointmentViewerService appointmentViewerService, IPhotosService photosService)
        {
            _context = context;
            _commonService = commonService;
            _inmateBookingService = inmateBookingService;
            _personnelId = Convert.ToInt32(httpContextAccessor.HttpContext.User
                .FindFirst(AuthDetailConstants.PERSONNELID)?.Value);
            _jwtDbContext = jwtDbContext;
            _userManager = userManager;
            _appointmentViewerService = appointmentViewerService;
            _photos = photosService;
        }

        //To get Inmate details Parent SubModule 
        public List<DetailVm> GetInmateDetailParent(int subModuleId)
        {
            List<DetailVm> listDetailParent = _context.AppAoDetailParentXref.Where(w => 
                w.AppAoSubModuleId == subModuleId && w.AppAoDetailParentVisible == 1).Select(s => new DetailVm
                {
                    DetailId = s.AppAoDetailParentId,
                    DetailName = s.AppAoDetailParent.AppAoDetailParentName,
                    DetailVisible = s.AppAoDetailParentVisible == 1,
                    DetailToolTip = s.AppAoDetailParent.AppAoDetailParentToolTip
                }).ToList();
            return listDetailParent;
        }

        public async Task<InmateFileDetailVm> GetInmateDetailsCountByInmateNumber(string inmateNumber)
        {
            int? inmateId = _context.Inmate.SingleOrDefault(w => w.InmateNumber == inmateNumber)?.InmateId;
            InmateFileDetailVm inmateFileDetailvm = new InmateFileDetailVm();

            return inmateId.HasValue ? await GetInmateFileCount(inmateId.Value) : inmateFileDetailvm;
        }

        public async Task<InmateFileDetailVm> GetInmateFileCount(int inmateId)
        {
            InmateFileDetailVm inmateFileDetail = new InmateFileDetailVm
            {
                PersonDetails = _context.Inmate
                    .Where(x => x.InmateId == inmateId)
                    .Select(a => new FilePersonVm
                    {
                        PersonId = a.PersonId,
                        InmateId = a.InmateId != 0 ? a.InmateId : default(int?),
                        PersonFirstName = a.Person.PersonFirstName,
                        PersonMiddleName = a.Person.PersonMiddleName,
                        PersonLastName = a.Person.PersonLastName,
                        PersonDob = a.Person.PersonDob,
                        PersonSuffix = a.Person.PersonSuffix,
                        InmateNumber = a.InmateNumber,
                        FacilityId = a.FacilityId,
                        InmateActive = a.InmateActive > 0
                    }).Single()
            };

            //KEEPSEP
            //To get keepsepInmate count for KeepSeparation
            int keepsepInmate = _context.KeepSeparate.Count(ks => 
                (ks.KeepSeparateInmate1Id == inmateId || ks.KeepSeparateInmate2Id == inmateId) && ks.InactiveFlag == 0 
                && ks.KeepSeparateInmate1.InmateActive == 1 && ks.KeepSeparateInmate2.InmateActive == 1);

            //To get keepsepAssoc count for KeepSeparation
            int keepsepAssoc = _context.KeepSepAssocInmate.Count(a => 
                a.KeepSepInmate2Id == inmateId && a.DeleteFlag == 0);

            //To get keepsepSubset count for KeepSeparation
            int keepsepSubset = _context.KeepSepSubsetInmate.Count(s => 
                s.KeepSepInmate2Id == inmateId && s.DeleteFlag == 0);

            //ASSOCIATION
            //To get LookupDescription in Lookup for Association count
            int[] lookupClassGroup = _commonService.GetLookupList(LookupConstants.CLASSGROUP)
                .Where(a => a.LookupFlag9 == 1 || a.LookupFlag10 == 1).Select(a => a.LookupIndex).ToArray();

            //To get list from Appointment
            // TODO Huh?
            DateTime startDate = DateTime.Now.Date.AddHours(0).AddMinutes(0).AddSeconds(0);
            DateTime endDate = DateTime.Now.AddMonths(6).Date.AddHours(23).AddMinutes(59).AddSeconds(59);

            CalendarInputs inputs = new CalendarInputs
            {
                CalendarTypes = "1,2,3",
                InmateId = inmateId,
                FromDate = startDate,
                ToDate = endDate
            };
            int apptCnt = _appointmentViewerService.CheckAppointment(inputs).Count;

            //To get list from WorkCrew for Workcrew count & Workfurlough count
            IQueryable<WorkCrew> lstWorkCrew = _context.WorkCrew.Where(w => 
                w.InmateId == inmateId && w.DeleteFlag == 0);

            List<Lookup> lstLookUp = _context.Lookup.Where(
                x => x.LookupType == LookupConstants.LIBBOOKCAT
                     || x.LookupType == LookupConstants.LIBBOOKCOND
                     || x.LookupType == LookupConstants.LIBBOOKTYPE
                     || x.LookupType == LookupConstants.ARRTYPE
                     || x.LookupType == LookupConstants.LAWDISPO).ToList();

            //Taking Book Category,Condition and Type LookupId from lookup table
            int[] categoryId = lstLookUp
                .Where(l => l.LookupType.Contains(LookupConstants.LIBBOOKCAT) && l.LookupInactive == 0)
                .Select(l => l.LookupIndex).ToArray();
            int[] conditionId = lstLookUp
                .Where(l => l.LookupType.Contains(LookupConstants.LIBBOOKCOND) && l.LookupInactive == 0)
                .Select(l => l.LookupIndex).ToArray();
            int[] typeId = lstLookUp
                .Where(l => l.LookupType.Contains(LookupConstants.LIBBOOKTYPE) && l.LookupInactive == 0)
                .Select(l => l.LookupIndex).ToArray();

            //Taking the Case status lookup MISC
            string[] lstLookUpsMisc = _context.Lookup.Where(x => 
                x.LookupType.Contains(LookupConstants.PROGCASESTATUS) && x.LookupInactive == 0)
                .Select(x => x.LookupDescription).ToArray();

            inmateFileDetail.ListBookingNumber = _context.Incarceration.Where(w => w.InmateId == inmateId)
                .Select(s => new BookingDetailCountVm
                {
                    IncarcerationId = s.IncarcerationId,
                    ArrestBookingNumber = s.BookingNo,
                    ReleaseOut = s.ReleaseOut,
                    DateIn = s.DateIn,
                    VerifyId = s.VerifyIDFlag
                }).OrderByDescending(o => o.IncarcerationId).ToList();

            //Getting Person details for Inmate Info & Clear 
            int personCount = _context.Person.Count(p =>
                    p.PersonId == inmateFileDetail.PersonDetails.PersonId &&
                    !inmateFileDetail.PersonDetails.InmateNumber.Contains("PURGED_%"));

            //To get list from FormRecord for Forms count
            IQueryable<FormRecord> lstFormRecord = _context.FormRecord.Where(f => f.DeleteFlag == 0);

            //To get list from AppletsSaved for Attach count
            IQueryable<AppletsSaved> lstAppletsSaved = _context.AppletsSaved.Where(i => i.AppletsDeleteFlag == 0);

            int incarcerationId;
            if (inmateFileDetail.ListBookingNumber.Any())
            {
                incarcerationId = inmateFileDetail.ListBookingNumber.FirstOrDefault()?.IncarcerationId ?? 0;

                inmateFileDetail.PersonDetails.InmatePreBookId = _context.InmatePrebook
                    .First(ip => incarcerationId == ip.IncarcerationId.Value).InmatePrebookId;

                //To get count from FormRecord for PreBook count
                int inmatePreBookCnt = lstFormRecord
                    .Count(f => f.DeleteFlag == 0 && f.FormTemplates.FormCategoryId == 1 &&
                                f.InmatePrebook.PersonId == inmateFileDetail.PersonDetails.PersonId
                                && f.InmatePrebook.IncarcerationId == incarcerationId &&
                                f.InmatePrebook.IncarcerationId > 0);

                //To get count from AppletsSaved for PreBook count
                int appletsSavedCnt = lstAppletsSaved
                    .Count(a => !a.InmatePrebook.DeleteFlag.HasValue && a.AppletsDeleteFlag == 0 &&
                                a.InmatePrebook.PersonId == inmateFileDetail.PersonDetails.PersonId);

                inmateFileDetail.ListBookingNumber.ForEach(a =>
                {
                    //FORMS
                    int incarcerationFormsCnt = lstFormRecord
                        .Count(f => f.FormTemplates.FormCategory.FormCategoryId == 10 //INCARCERATION FORMS
                                    && f.Incarceration.IncarcerationId == a.IncarcerationId);

                    //Booking Sheet forms
                    int bookingSheetFormsCnt = lstFormRecord
                        .Count(f => f.FormTemplates.FormCategory.FormCategoryId == 22 //Booking Sheet FORMS
                                    && f.BooksSheetIncarcerationId == a.IncarcerationId);

                    int incarcerationAttachCnt = lstAppletsSaved.Count(i => i.IncarcerationId == a.IncarcerationId);

                    a.BookingCountVm = new Dictionary<SubmoduleDetail, int>
                    {
                        [SubmoduleDetail.BookingsOverall] = personCount,
                        [SubmoduleDetail.BookingsTransport] = _context.Incarceration.Count(i =>
                            i.IncarcerationId == incarcerationId && i.TransportFlag == 1 &&
                            i.Inmate.PersonId == inmateFileDetail.PersonDetails.PersonId),
                        [SubmoduleDetail.BookingsPrebook] = inmatePreBookCnt + appletsSavedCnt,
                        [SubmoduleDetail.BookingsForms] = incarcerationFormsCnt,
                        [SubmoduleDetail.BookingsAttach]  = incarcerationAttachCnt,
                        [SubmoduleDetail.BookingsIntakeCurrency]  = _context.IncarcerationIntakeCurrency.Count(inc =>
                            inc.IncarcerationId == incarcerationId),
                        [SubmoduleDetail.BookingsHistory] = inmateFileDetail.ListBookingNumber.Count,
                        [SubmoduleDetail.BookingsKeeper]  = _context.IncarcerationNoKeeperHistory.Count(inc =>
                            inc.IncarcerationId == incarcerationId && inc.Keeper),
                        [SubmoduleDetail.BookingsSheet] = bookingSheetFormsCnt,
                    };
                    //Keeper - No Keeper Flag
                    inmateFileDetail.PersonDetails.NoKeeper = _context.Incarceration.Find(incarcerationId).NoKeeper;
                });
            }

            //To get Inmate BookingNo details for Booking dropdown
            inmateFileDetail.ListBookingCasesNumber =
                _context.IncarcerationArrestXref.Where(i => i.Incarceration.InmateId == inmateId)
                    .Select(i => new InmateBookingNumberDetails
                    {
                        ArrestBookingNumber = i.Arrest.ArrestBookingNo,
                        BookingDate = i.BookingDate,
                        ArrestId = i.ArrestId,
                        ArrestType = i.Arrest.ArrestType,
                        IncarcerationId = i.IncarcerationId,
                        IncarcerationArrestXrefId = i.IncarcerationArrestXrefId,
                        CourtDocket = i.Arrest.ArrestCourtDocket,
                        ArrestAbbrevation = i.Arrest.ArrestingAgency.AgencyAbbreviation,
                        ArrestBookingType = i.Arrest.ArrestType,
                        ClearDate = i.ReleaseDate,
                        ClearReason = i.ReleaseReason,
                        Court = i.Arrest.ArrestCourtJurisdiction.AgencyName
                    }).OrderBy(o => o.ArrestId).ToList();

            inmateFileDetail.ListBookingCasesNumber?.ForEach(a =>
            {
                if (!string.IsNullOrEmpty(a.ArrestType))
                {
                    a.BookingType = lstLookUp.Single(lkp => lkp.LookupIndex == Convert.ToInt32(Convert.ToDecimal(a.ArrestType))
                            && lkp.LookupType == LookupConstants.ARRTYPE).LookupDescription;
                }

                //To get list from Crime for Charges count
                int crimeCnt = _context.Crime.Count(c => c.CrimeLookupId > 0 && c.CrimeDeleteFlag == 0 && 
                    !c.WarrantId.HasValue && c.Arrest.ArrestId == a.ArrestId);

                //To get list from CrimeForce for Charges count
                int crimeForceCnt = _context.CrimeForce.Count(c => c.DeleteFlag == 0 && !c.DropChargeFlag.HasValue 
                    && !c.ForceCrimeLookupId.HasValue && !c.WarrantId.HasValue && !c.ForceSupervisorReviewFlag.HasValue &&
                    c.ArrestId == a.ArrestId);

                int bookingFormsCnt = lstFormRecord.Count(f => f.FormTemplates.FormCategory.FormCategoryId == 11 //BOOKING FORMS
                    && f.ArrestId == a.ArrestId);

                //ATTACH
                int bookingAttachCnt = lstAppletsSaved.Count(i => i.ArrestId > 0 && i.ArrestId == a.ArrestId);

                a.InmateBookingDetailsCountVm = new Dictionary<SubmoduleDetail, int>
                {
                    [SubmoduleDetail.CasesAssigned] = inmateFileDetail.ListBookingCasesNumber.Count(c=>c.IncarcerationId==a.IncarcerationId),
                    [SubmoduleDetail.CasesInfo] = personCount,
                    [SubmoduleDetail.CasesSheet] = personCount,
                    [SubmoduleDetail.CasesClear] = inmateFileDetail.ListBookingCasesNumber.Count(x=>x.ClearDate.HasValue),
                    [SubmoduleDetail.CasesSentence] = _context.IncarcerationArrestXref.Count(
                        s => (s.Arrest.ArrestSentenceCode == 1 || s.Arrest.ArrestSentenceCode == 2)
                             && !s.Incarceration.ReleaseOut.HasValue &&
                             s.Incarceration.IncarcerationId == a.IncarcerationId &&
                             s.Arrest.Inmate.PersonId == inmateFileDetail.PersonDetails.PersonId),
                    [SubmoduleDetail.CasesCharges] = crimeCnt + crimeForceCnt,
                    //WARRANT
                    //[SubmoduleDetail.CasesCharges] = _context.Warrant.Count(w => w.Arrest.ArrestId == a.ArrestId),
                    [SubmoduleDetail.CasesBail] = _context.BailTransaction
                        .Count(b => !b.VoidFlag.HasValue && b.ArrestId == a.ArrestId &&
                                    b.Arrest.Inmate.PersonId == inmateFileDetail.PersonDetails.PersonId),
                    [SubmoduleDetail.CasesNote] = _context.ArrestNote.Count(an => an.ArrestId == a.ArrestId && an.DeleteFlag == 0),
                    [SubmoduleDetail.CasesForms] = bookingFormsCnt,
                    [SubmoduleDetail.CasesAttach] = bookingAttachCnt,
                    [SubmoduleDetail.CasesHistory] = inmateFileDetail.ListBookingCasesNumber.Count,
                };
            });

            inmateFileDetail.InmateFileCount = new Dictionary<SubmoduleDetail, int>
            {
                [SubmoduleDetail.PersonIdentity] = 1, //Hardcoded
                [SubmoduleDetail.PersonAka] = _context.Aka.Count(a =>
                    a.PersonId == inmateFileDetail.PersonDetails.PersonId && a.PersonId > 0
                    && (!a.DeleteFlag.HasValue || a.DeleteFlag == 0)),
                [SubmoduleDetail.PersonAddress] = _context.Address.Count(a =>
                    a.PersonId == inmateFileDetail.PersonDetails.PersonId),
                [SubmoduleDetail.PersonChar] = _context.PersonDescription.Count(a => a.PersonId == inmateFileDetail.PersonDetails.PersonId),
                [SubmoduleDetail.PersonDescriptor] = _context.PersonDescriptor.Count(
                    a => a.PersonId == inmateFileDetail.PersonDetails.PersonId && a.DeleteFlag == 0 && a.Code != null),
                [SubmoduleDetail.PersonPhoto] = _context.Identifiers.Count(
                    a => a.PersonId == inmateFileDetail.PersonDetails.PersonId && a.DeleteFlag == 0 &&
                         (a.PhotographRelativePath != null || a.PhotographPath != null)),
                [SubmoduleDetail.PersonContact] = _context.Contact.Count(
                    a => a.TypePersonId == inmateFileDetail.PersonDetails.PersonId && a.PersonId > 0 &&
                         a.ContactActiveFlag == "1"),
                [SubmoduleDetail.PersonProfile] = 1, //Hardcoded
                [SubmoduleDetail.PersonDna] = _context.PersonDna.Count(a => a.PersonId == inmateFileDetail.PersonDetails.PersonId),
                [SubmoduleDetail.PersonTesting] = _context.PersonTesting.Count(a =>
                    a.PersonId == inmateFileDetail.PersonDetails.PersonId),
                [SubmoduleDetail.PersonMilitary] = _context.PersonMilitary.Count(a => a.PersonId == inmateFileDetail.PersonDetails.PersonId),
                // BioMetrics = _context.BiometricsFingerprint.Count(a => a.PersonId == inmateFileDetail.PersonDetails.PersonId && a.DeleteFlag == 0),
                [SubmoduleDetail.AlertsMessage] = _context.PersonAlert.Count(a =>
                    a.PersonId == inmateFileDetail.PersonDetails.PersonId && a.ActiveAlertFlag == 1 &&
                    (!a.ExpireDate.HasValue || a.ExpireDate.Value.Date >= DateTime.Now.Date)),

                [SubmoduleDetail.AlertsFlag] = _context.PersonFlag.Count(a =>
                     a.PersonId == inmateFileDetail.PersonDetails.PersonId && a.DeleteFlag == 0 &&
                     (a.PersonFlagIndex > 0 || a.InmateFlagIndex > 0 || a.DietFlagIndex > 0)),

                [SubmoduleDetail.AlertsKeepSep] = keepsepInmate + keepsepAssoc + keepsepSubset,
                [SubmoduleDetail.AlertsAssoc] = _context.PersonClassification.Count(pc =>
                    pc.InactiveFlag == 0 && pc.PersonId == inmateFileDetail.PersonDetails.PersonId &&
                    (!pc.PersonClassificationDateThru.HasValue ||
                        pc.PersonClassificationDateThru.Value.Date >= DateTime.Now.Date) &&
                    lookupClassGroup.Contains(pc.PersonClassificationTypeId ?? 0)),

                [SubmoduleDetail.AlertsPrivilege] = _context.InmatePrivilegeXref.Count(ip =>
                    ip.Privilege.InactiveFlag == 0 && !ip.PrivilegeRemoveOfficerId.HasValue &&
                    (ip.PrivilegeExpires >= DateTime.Now || !ip.PrivilegeExpires.HasValue) && ip.InmateId == inmateId),

                [SubmoduleDetail.AlertsObservation] = _context.ObservationSchedule
                    .Count(o => o.InmateId == inmateId && o.DeleteFlag == 0 && o.StartDate.Value <= DateTime.Now &&
                                (!o.EndDate.HasValue || o.EndDate.Value >= DateTime.Now)),

                [SubmoduleDetail.OperationsHousing] = _context.HousingUnitMoveHistory.Count(h => h.InmateId == inmateId),
                [SubmoduleDetail.OperationsTracking] = _context.InmateTrak.Count(i => i.InmateId == inmateId),
                [SubmoduleDetail.OperationsAppointment] = apptCnt,
                [SubmoduleDetail.OperationsVisit] = _context.VisitToVisitor.Count(w => w.Visit.InmateId == inmateId &&
                    !w.Visit.CompleteRegistration && w.Visit.VisitDenyFlag == 0
                    && !w.Visit.DeleteFlag && !w.Visit.VisitSecondaryFlag.HasValue && !w.Visit.CompleteVisitFlag),
                [SubmoduleDetail.OperationsClassify] = _context.InmateClassification.Count(i => i.InmateId == inmateId),
                [SubmoduleDetail.OperationsNotes] = _context.FloorNoteXref.Count(fxref => fxref.InmateId == inmateId),
                [SubmoduleDetail.OperationsRequest] = _context.Request.Count(r => r.InmateId == inmateId &&
                    r.RequestActionLookup.RequestByInmate && !r.RequestActionLookup.InactiveFlag),
                [SubmoduleDetail.OperationsIncident] = _context.DisciplinaryInmate.Count(d => d.InmateId == inmateId),
                [SubmoduleDetail.OperationsGrievance] = _context.Grievance.Count(g =>
                    g.InmateId == inmateId && g.DeleteFlag == 0),
                [SubmoduleDetail.OperationsWorkCrew] = lstWorkCrew.Count(w => !w.WorkCrewLookup.WorkFurloughFlag.HasValue),
                [SubmoduleDetail.OperationsWorkFurlough] = lstWorkCrew.Count(w => w.WorkCrewLookup.WorkFurloughFlag.HasValue),

                [SubmoduleDetail.MiscellaneousInventory] = _context.PersonalInventory.Count(
                    i => i.InventoryDispositionCode == 4 && i.DeleteFlag == 0 && i.InmateId == inmateId),
                [SubmoduleDetail.MiscellaneousPhone] = _context.PhoneCallLog.Count(p => p.DeleteFlag == 0 && p.InmateId == inmateId),
                [SubmoduleDetail.MiscellaneousLabel] = _context.PersonFormTemplate.Count(p => p.ShowInLabel == true && p.DeleteFlag != true
                            && p.Inactive != true && !string.IsNullOrEmpty(p.ShortId)),
                // Ncic = 1

                [SubmoduleDetail.MiscellaneousRecChk] = _context.RecordsCheckRequest.Count(r =>
                    r.PersonId == inmateFileDetail.PersonDetails.PersonId && r.DeleteFlag == 0),
                [SubmoduleDetail.MiscellaneousAttendee] = _context.ArrestSentenceAttendance.Count(a =>
                    a.DeleteFlag == 0 && a.InmateId == inmateId),
                [SubmoduleDetail.MiscellaneousIssuedProp] = _context.IssuedProperty.Count(i =>
                    i.DeleteFlag == 0 && i.ActiveFlag == 1 && i.InmateId == inmateId),
                [SubmoduleDetail.MiscellaneousLibrary] = _context.LibraryBook.Count(l => l.DeleteFlag == 0 &&
                    l.LibraryRoom.DeleteFlag == 0 && l.CurrentCheckoutInmateId == inmateId &&
                    categoryId.Contains(l.BookCategoryId) && typeId.Contains(l.BookTypeId) &&
                    conditionId.Contains(l.CurrentConditionId ?? 0) || l.LibraryRoomLocation.DeleteFlag == 0),
                [SubmoduleDetail.MiscellaneousTasks] = _context.AoTaskQueue.Count(queue =>
                    queue.InmateId == inmateId && !queue.CompleteFlag),
                [SubmoduleDetail.MiscellaneousMail] = _context.MailRecord.Count(r => r.InmateId == inmateId && !r.DeleteFlag),

                [SubmoduleDetail.MiscellaneousAltSent] = _context.AltSent.Count(a =>
                    a.Incarceration.InmateId == inmateId && a.Incarceration.IncarcerationId > 0 &&
                    !a.Incarceration.ReleaseOut.HasValue),
                [SubmoduleDetail.ProgramCaseManagement] = _context.Inmate.Count(
                    i => i.InmateId == inmateId && lstLookUpsMisc.Contains(i.ProgramCaseStatus)),
                [SubmoduleDetail.ProgramClassAttendance] = 0,
                [SubmoduleDetail.ProgramClassEnroll] = 0,
                // This program table need to change based on programClass
                [SubmoduleDetail.ProgramRequestHistory] = _context.ProgramRequest.Count(p =>
                    p.InmateId == inmateId && p.DeleteFlag == 0 && p.ProgramClassId > 0
                    && !p.AppointmentProgramAssignId.HasValue),
                [SubmoduleDetail.MoneyLedger] = _context.AccountAoTransaction.Count(a => a.InmateId == inmateId),
            };

            //Unrestricted PHI medical flag count
            List<Lookup> lstLookup = _context.Lookup.Where(l => !l.LookupNoAlert.HasValue &&
                l.LookupInactive == 0 && l.LookupType == LookupConstants.MEDFLAG && l.LookupFlag6 == 1).ToList();

            if (lstLookup.Count > 0)
            {
                int medicalFlagCnt = _context.PersonFlag.Count(a =>
                    a.PersonId == inmateFileDetail.PersonDetails.PersonId && a.DeleteFlag == 0 &&
                    a.MedicalFlagIndex > 0 && lstLookup.Any(d => d.LookupIndex == a.MedicalFlagIndex));
                inmateFileDetail.InmateFileCount[SubmoduleDetail.AlertsFlag] += medicalFlagCnt;
            }

            inmateFileDetail.CourtCommitCnt = _inmateBookingService
                .GetActiveCommitDetails(inmateFileDetail.PersonDetails.PersonId).Count;
            if (inmateFileDetail.PersonDetails.InmateActive)
            {
                await UpdateLastXInmate(inmateId);
            }

            return inmateFileDetail;
        }
        
        public BookingHistoryVm GetBookingHistory(int inmateId)
        {
            BookingHistoryVm inmateBookingCount = new BookingHistoryVm();

            List<Lookup> lookups = _context.Lookup.Where(x =>
                x.LookupType == LookupConstants.ARRTYPE || x.LookupType == LookupConstants.LAWDISPO).ToList();

            inmateBookingCount.BookingDetailCount = _context.Incarceration.Where(w => w.InmateId == inmateId)
                .Select(s => new BookingDetailCountVm
                {
                    IncarcerationId = s.IncarcerationId,
                    ArrestBookingNumber = s.BookingNo,
                    DateIn = s.DateIn,
                    ReleaseOut = s.ReleaseOut,
                    InmateBookingNumberDetails = s.IncarcerationArrestXref
                    .OrderBy(i => i.ArrestId)
                    .ThenByDescending(i => i.Incarceration.DateIn)
                    .Select(i => new InmateBookingNumberDetails {
                        ArrestBookingNumber = i.Arrest.ArrestBookingNo,
                        BookingDate = i.BookingDate,
                        ArrestId = i.ArrestId,
                        IncarcerationId = i.IncarcerationId,
                        IncarcerationArrestXrefId = i.IncarcerationArrestXrefId,
                        CourtDocket = i.Arrest.ArrestCourtDocket,
                        ArrestAbbrevation = i.Arrest.ArrestingAgency.AgencyAbbreviation,
                        BookingType = lookups.Where(lkp =>
                            lkp.LookupIndex == Convert.ToInt32(i.Arrest.ArrestType) &&
                            lkp.LookupType == LookupConstants.ARRTYPE).Select(lkp => lkp.LookupDescription)
                            .SingleOrDefault(),
                        ArrestDate = i.Arrest.ArrestDate,
                        ArrestCaseNumber = i.Arrest.ArrestCaseNumber,
                        ClearDate = i.ReleaseDate,
                        ClearReason = i.ReleaseReason,
                        ArrestDisposition = lookups.Where(lk => lk.LookupType == LookupConstants.LAWDISPO &&
                            lk.LookupIndex == Convert.ToInt32(i.Arrest.ArrestLawEnforcementDisposition))
                            .Select(lk => lk.LookupDescription).SingleOrDefault()
                    }).ToList()
                }).OrderByDescending(o => o.IncarcerationId).ToList();
            return inmateBookingCount;

        }

        public async Task<IdentityResult> UpdateLastXInmate(int inmateId)
        {
            string userId = _jwtDbContext.UserClaims.Where(w => w.ClaimType == "personnel_id"
                   && w.ClaimValue == _personnelId.ToString())
                  .Select(s => s.UserId).FirstOrDefault();

            AppUser appUser = _userManager.FindByIdAsync(userId).Result;

            IList<Claim> claims = await _userManager.GetClaimsAsync(appUser);

            Claim inmateIdClaim = claims.FirstOrDefault(f => f.Type == "inmate_id");
            IdentityResult result;

            if (inmateIdClaim == null)
            {
                result = await _userManager.AddClaimAsync(appUser, new Claim("inmate_id", inmateId.ToString()));
            }
            else
            {
                List<string> lstLastInmateId = inmateIdClaim.Value.Split(',').Where(val => val != inmateId.ToString()).ToList();
                lstLastInmateId.Insert(0, inmateId.ToString());
                if (lstLastInmateId.Count > 10)
                {
                    lstLastInmateId.RemoveAt(10);
                }

                result = await _userManager.ReplaceClaimAsync(appUser, inmateIdClaim,
                    new Claim("inmate_id", string.Join(",", lstLastInmateId.Select(x => x.ToString()).ToArray())));
            }
            return result;

        }

        public async Task<List<InmateSearchVm>> GetLastXInmates()
        {
            string userId = _jwtDbContext.UserClaims.Where(w => w.ClaimType == "personnel_id"
                   && w.ClaimValue == _personnelId.ToString())
                  .Select(s => s.UserId).FirstOrDefault();

            AppUser appUser = _userManager.FindByIdAsync(userId).Result;

            IList<Claim> claims = await _userManager.GetClaimsAsync(appUser);

            Claim inmateIdClaim = claims.FirstOrDefault(f => f.Type == "inmate_id");

            List<InmateSearchVm> inmateList = new List<InmateSearchVm>();
            if (inmateIdClaim == null) return inmateList;
            List<int> lstLastInmateId = inmateIdClaim.Value.Split(',').Select(int.Parse).ToList();

            List<InmateSearchVm> listIncar = _context.Incarceration.Where(i => lstLastInmateId.Contains(i.Inmate.InmateId))
                .Select(f =>
                    new InmateSearchVm
                    {
                        InmateId = f.InmateId ?? 0,
                        IncarcerationId = f.IncarcerationId,
                        BookingNo = f.BookingNo
                    }).ToList();

            inmateList = _context.Inmate.Where(w => lstLastInmateId.Contains(w.InmateId) && w.InmateActive == 1)
                .Select(i => new InmateSearchVm
                {
                    InmateNumber = i.InmateNumber,
                    InmateId = i.InmateId,
                    PersonId = i.PersonId,
                    HousingUnitId = i.HousingUnitId,
                    InmateActive = i.InmateActive,
                    FacilityId = i.FacilityId,
                    FacilityAbbr = i.Facility.FacilityAbbr,
                    HousingDetail = new HousingDetail
                    {
                        HousingUnitLocation = i.HousingUnit.HousingUnitLocation,
                        HousingUnitNumber = i.HousingUnit.HousingUnitNumber,
                        HousingUnitBedNumber = i.HousingUnit.HousingUnitBedNumber,
                        HousingUnitBedLocation = i.HousingUnit.HousingUnitBedLocation,
                        HousingUnitListId = i.HousingUnit.HousingUnitListId
                    },
                    PersonDetail = new PersonInfoVm
                    {
                        PersonFirstName = i.Person.PersonFirstName,
                        PersonLastName = i.Person.PersonLastName,
                        PersonMiddleName = i.Person.PersonMiddleName,
                        PersonSuffix = i.Person.PersonSuffix,
                        PersonId = i.PersonId,
                        PersonDob = i.Person.PersonDob,
                        InmateClassificationId = i.InmateClassificationId
                    },
                    PhotoFilePath = _photos.GetPhotoByCollectionIdentifier(i.Person.Identifiers),
                }).ToList();
            inmateList.ForEach(inm =>
            {
                inm.IncarcerationId = listIncar.OrderByDescending(o => o.IncarcerationId)
                    .FirstOrDefault(w => w.InmateId == inm.InmateId)?.IncarcerationId;
                inm.BookingNo = listIncar.OrderByDescending(o => o.BookingNo)
                    .FirstOrDefault(w => w.InmateId == inm.InmateId)?.BookingNo;
            });
            inmateList = inmateList.OrderBy(i => lstLastInmateId.IndexOf(i.InmateId)).ToList();

            return inmateList;
        }
    }
}