using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using GenerateTables.Models;
using ServerAPI.ViewModels;
using ServerAPI.Utilities;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using ScheduleWidget.Schedule;
using ScheduleWidget.Common;


// ReSharper disable once CheckNamespace
namespace ServerAPI.Services
{
    public class InmateSummaryPdfService : IInmateSummaryPdfService
    {
        private readonly AAtims _context;
        private readonly ICommonService _commonService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IFormsService _formsService;
        private readonly IPhotosService _photos;
        private readonly IPersonDnaService _personDnaService;

        private InmateSummaryPdfVm _inmateSummaryPdfDetail = new InmateSummaryPdfVm();
        private IQueryable<Lookup> _dbLookupDetails;
        private bool _isBailSummary;
        private readonly IPersonCharService _iPersonCharService;

        public InmateSummaryPdfService(AAtims context, ICommonService commonService,
            IFormsService formsService, IHttpContextAccessor httpContextAccessor,
            IPersonCharService iPersonCharService, IPhotosService photosService, IPersonDnaService personDnaService)
        {
            _context = context;
            _commonService = commonService;
            _httpContextAccessor = httpContextAccessor;
            _formsService = formsService;
            _iPersonCharService = iPersonCharService;
            _photos = photosService;
            _personDnaService = personDnaService;
        }

        #region Get Inmate-Summary Pdf details

        public InmateSummaryPdfVm GetInmateSummaryPdf(int inmateId, InmateSummaryType summaryType, int? incarcerationId = null)
        {

            if (summaryType == InmateSummaryType.ACTIVEBAILSUMMARY)
            {
                _isBailSummary = true;
            }

            Inmate dbInmateDetails = _context.Inmate.SingleOrDefault(inm => inm.InmateId == inmateId);
            int personnelId =
                Convert.ToInt32(_httpContextAccessor.HttpContext.User.FindFirst(AuthDetailConstants.PERSONNELID)?.Value);
            _dbLookupDetails = _context.Lookup;

            if (dbInmateDetails == null) return _inmateSummaryPdfDetail;
            //Get PDF Header Details
            _inmateSummaryPdfDetail.InmatePdfHeaderDetails = new InmatePdfHeader
            {
                AgencyName = _context.Agency.FirstOrDefault(ag => ag.AgencyJailFlag)?.AgencyName,
                InmateNumber = dbInmateDetails.InmateNumber,
                PersonnelNumber = _context.Personnel.SingleOrDefault(per =>
                    per.PersonnelId == personnelId)?.PersonnelNumber,
                OfficerBadgeNumber = _context.Personnel.SingleOrDefault(per =>
                    per.PersonnelId == personnelId)?.OfficerBadgeNum,
                OfficerName = _context.Person.FirstOrDefault(p =>
                    p.Personnel.FirstOrDefault().PersonnelId == personnelId)?.PersonLastName
            };
            _inmateSummaryPdfDetail.InmateSiteNumber = dbInmateDetails.InmateSiteNumber;

            //Load Person Details
            _inmateSummaryPdfDetail.PersonDetails = GetPersonDetails(dbInmateDetails.PersonId);
            _inmateSummaryPdfDetail.ClassificationReason =
                _context.InmateClassification.SingleOrDefault(
                        incl => incl.InmateClassificationId == dbInmateDetails.InmateClassificationId)?
                    .InmateClassificationReason;

            if (dbInmateDetails.HousingUnitId.HasValue)
            {
                _inmateSummaryPdfDetail.HousingDetails = (from hu in _context.HousingUnit
                                                          where hu.HousingUnitId == dbInmateDetails.HousingUnitId
                                                          select new HousingDetail
                                                          {
                                                              FacilityId = hu.FacilityId,
                                                              FacilityAbbr = hu.Facility.FacilityAbbr,
                                                              HousingUnitLocation = hu.HousingUnitLocation,
                                                              HousingUnitNumber = hu.HousingUnitNumber,
                                                              HousingUnitBedNumber = hu.HousingUnitBedNumber,
                                                              HousingUnitBedLocation = hu.HousingUnitBedLocation
                                                          }).SingleOrDefault();
            }

            _inmateSummaryPdfDetail.PersonCharDetails = _iPersonCharService.GetCharacteristics(dbInmateDetails.PersonId);

            _inmateSummaryPdfDetail.PersonDNA = _personDnaService.GetDnaDetails(dbInmateDetails.PersonId);

            //Get Site options
            _inmateSummaryPdfDetail.SiteOption =
                _commonService.GetSiteOptionValue(SiteOptionsConstants.BAILSUMCHARNO, SiteOptionsConstants.BAILSUMCHARNO);

            _inmateSummaryPdfDetail.SentenceByCharge = _commonService.GetSiteOptionValue(SiteOptionsConstants.SENTENCEBYCHARGE);

            // In Finger-print summary doesn't need this data
            if (summaryType != InmateSummaryType.ACTIVEINMATEFINGERPRINT)
            {
                //HomeAddress && Business Address
                _inmateSummaryPdfDetail.PersonDetails.ResAddress = GetPersonAddressDetails(
                    dbInmateDetails.PersonId, AddressTypeConstants.RES);
                _inmateSummaryPdfDetail.PersonDetails.BusAddress = GetPersonAddressDetails(
                    dbInmateDetails.PersonId, AddressTypeConstants.BUS);

                //Load Contact Address Details
                LoadContactDetails(dbInmateDetails.PersonId);

                //Load Person Description Details
                LoadPersonDescription(dbInmateDetails.PersonId);
            }

            if (summaryType != InmateSummaryType.NONE)
            {
                //Load Incarceration Details
                LoadIncarcerationDetails(inmateId, summaryType, incarcerationId);

                _inmateSummaryPdfDetail.MonikerLst = _context.Aka.Where(aka =>
                    aka.PersonId == _inmateSummaryPdfDetail.PersonDetails.PersonId &&
                    !string.IsNullOrEmpty(aka.PersonGangName)).Select(aka => aka.PersonGangName).ToList();

                LoadObservationSchDetails(inmateId);
                LoadKeepSeperateDetails(inmateId);
                LoadPersonFlagAlerts(_inmateSummaryPdfDetail.PersonDetails.PersonId);

                #region Get Customized Fields From AppAoUserControlFields
                List<UserControlFieldTags> fieldLabList = new List<UserControlFieldTags>
                {
                    new UserControlFieldTags{FieldTag= nameof(_inmateSummaryPdfDetail.TxtInmateNumber),ControlId= 26 },
                    new UserControlFieldTags{FieldTag=nameof(_inmateSummaryPdfDetail.TxtSiteInmate),ControlId= 26 },
                    new UserControlFieldTags
                    {
                        FieldTag = nameof(_inmateSummaryPdfDetail.TxtAKAInmateNum),
                        ControlId = 2
                    },
                    new UserControlFieldTags
                    {
                        FieldTag = nameof(_inmateSummaryPdfDetail.TxtAKASiteInmateNum),
                        ControlId = 2
                    },
                    new UserControlFieldTags
                    {
                        FieldTag = nameof(_inmateSummaryPdfDetail.TxtOtherPhoneNum),
                        ControlId = 2
                    },
                    new UserControlFieldTags
                    {
                        FieldTag = nameof(_inmateSummaryPdfDetail.TxtAFISNumber),
                        ControlId = 2
                    },
                    new UserControlFieldTags { FieldTag = nameof(_inmateSummaryPdfDetail.TxtAKAFBI), ControlId = 2 }
                };

                List<UserControlFieldTags> customizedFieldNames = _commonService.GetFieldNames(fieldLabList);

                _inmateSummaryPdfDetail.TxtInmateNumber = customizedFieldNames
                    .SingleOrDefault(cf => cf.FieldTag == nameof(_inmateSummaryPdfDetail.TxtInmateNumber))?.FieldLabel;
                _inmateSummaryPdfDetail.TxtSiteInmate = customizedFieldNames
                    .SingleOrDefault(cf => cf.FieldTag == nameof(_inmateSummaryPdfDetail.TxtSiteInmate))?.FieldLabel;
                _inmateSummaryPdfDetail.TxtAKAInmateNum = customizedFieldNames
                    .SingleOrDefault(cf => cf.FieldTag == nameof(_inmateSummaryPdfDetail.TxtAKAInmateNum))?.FieldLabel;
                _inmateSummaryPdfDetail.TxtAKASiteInmateNum = customizedFieldNames
                    .SingleOrDefault(cf => cf.FieldTag == nameof(_inmateSummaryPdfDetail.TxtAKASiteInmateNum))?.FieldLabel;
                _inmateSummaryPdfDetail.TxtOtherPhoneNum = customizedFieldNames
                    .SingleOrDefault(cf => cf.FieldTag == nameof(_inmateSummaryPdfDetail.TxtOtherPhoneNum))?.FieldLabel;
                _inmateSummaryPdfDetail.TxtAFISNumber = customizedFieldNames
                    .SingleOrDefault(cf => cf.FieldTag == nameof(_inmateSummaryPdfDetail.TxtAFISNumber))?.FieldLabel;
                _inmateSummaryPdfDetail.TxtAKAFBI = customizedFieldNames
                    .SingleOrDefault(cf => cf.FieldTag == nameof(_inmateSummaryPdfDetail.TxtAKAFBI))?.FieldLabel;

                #endregion
            }

            LoadPersonDescriptorDetails(_inmateSummaryPdfDetail.PersonDetails.PersonId);
            LoadPersonClassification(_inmateSummaryPdfDetail.PersonDetails.PersonId);
            LoadAkaDetails(_inmateSummaryPdfDetail.PersonDetails.PersonId);

            _inmateSummaryPdfDetail.PhotoFilePath = _photos.GetPhotoByPersonId(_inmateSummaryPdfDetail.PersonDetails.PersonId);

            return _inmateSummaryPdfDetail;
        }

        private void LoadIncarcerationDetails(int inmateId, InmateSummaryType summaryType, int? incarcerationId)
        {
            _inmateSummaryPdfDetail.IncarcerationDetails = (from inc in _context.Incarceration
                                                            where inc.InmateId == inmateId
                                                                  && (InmateSummaryType.INMATESUMMARY == summaryType || !inc.ReleaseOut.HasValue)
                                                                  && (!incarcerationId.HasValue || inc.IncarcerationId == incarcerationId.Value)
                                                            select new IncarcerationDetail
                                                            {
                                                                IncarcerationId = inc.IncarcerationId,
                                                                DateIn = inc.DateIn,
                                                                ReleaseOut = inc.ReleaseOut,
                                                                OverallFinalReleaseDate = inc.OverallFinalReleaseDate,
                                                                UsedPersonFirst = inc.UsedPersonFrist,
                                                                UsedPersonLast = inc.UsedPersonLast,
                                                                UsedPersonMiddle = inc.UsedPersonMiddle,
                                                                BookCompleteFlag = inc.BookCompleteFlag == 1
                                                            }).OrderByDescending(d => d.IncarcerationId).Take(15).ToList();

            //Get AltSent Details by using Incarceration Id
            if (!_inmateSummaryPdfDetail.IncarcerationDetails.Any()) return;
            List<int> incarcerationIdLst =
                _inmateSummaryPdfDetail.IncarcerationDetails.Select(de => de.IncarcerationId).ToList();

            IQueryable<AltSent> dbAltSentLst = _context.AltSent.Where(al =>
                    al.IncarcerationId.HasValue && incarcerationIdLst.Contains(al.IncarcerationId.Value));
            IQueryable<Arrest> dbArrestDetails = _context.Arrest;

            IQueryable<IncarcerationArrestXref> dbIncarcerationArrestXref =
                _context.IncarcerationArrestXref.Where(iaxf => iaxf.IncarcerationId.HasValue &&
                                                               incarcerationIdLst.Contains(iaxf.IncarcerationId ?? 0));

            bool isbailSummary = summaryType == InmateSummaryType.ACTIVEBAILSUMMARY ||
                                 summaryType == InmateSummaryType.ACTIVEINMATESUMMARY;
            _inmateSummaryPdfDetail.IncarcerationDetails.ForEach(incd =>
            {
                LoadAltSentDetails(incd, dbAltSentLst);

                //Get Booking Details
                incd.ArrestBookingDetails = GetBookDetails(dbArrestDetails,
                    dbIncarcerationArrestXref, summaryType).Where(w => w.IncarcerationId == incd.IncarcerationId).OrderByDescending(o => o.ArrestId).ToList();

                //Get grand total based on IncarcerationId
                incd.XbailCnt = dbIncarcerationArrestXref.Count(inx => inx.IncarcerationId == incd.IncarcerationId
                                  && (inx.Arrest.BailAmount > 0 || inx.Arrest.BailNoBailFlag == 1) &&
                                  (!isbailSummary || !inx.ReleaseDate.HasValue));

                incd.YbailCnt = dbIncarcerationArrestXref.Count(inx =>
                    inx.IncarcerationId == incd.IncarcerationId && (!isbailSummary || !inx.ReleaseDate.HasValue));

                incd.IsNoBail = dbIncarcerationArrestXref.Count(inx => inx.IncarcerationId == incd.IncarcerationId
                    && inx.Arrest.BailNoBailFlag == 1 && (!isbailSummary || !inx.ReleaseDate.HasValue)) > 0;

                incd.BailAmount = dbIncarcerationArrestXref.Where(inx => inx.IncarcerationId == incd.IncarcerationId
                    && inx.Arrest.BailNoBailFlag != 1 &&
                    (!isbailSummary || !inx.ReleaseDate.HasValue)).Sum(de => de.Arrest.BailAmount) ?? 0;
            });
        }

        #endregion

        #region Get Booking Details

        private List<ArrestBookingDetails> GetBookDetails(IQueryable<Arrest> dbArrestDetails, IQueryable<IncarcerationArrestXref> dbIncarcerationArrestXref,
            InmateSummaryType summaryType)
        {
            List<ArrestBookingDetails> arrestBookingDetailLst = (from iaxf in dbIncarcerationArrestXref
                where InmateSummaryType.ACTIVEBAILSUMMARY != summaryType || !iaxf.ReleaseDate.HasValue
                select new ArrestBookingDetails
                {
                    IncarcerationId = iaxf.IncarcerationId,
                    ArrestId = iaxf.ArrestId ?? 0,
                    ReleaseDate = iaxf.ReleaseDate,
                    ReleaseReason = iaxf.ReleaseReason
                }).ToList();

            List<int> arrestIdLst = arrestBookingDetailLst.Select(de => de.ArrestId).ToList();
            IQueryable<Arrest> filArrestDetails = dbArrestDetails.Where(arr => arrestIdLst.Contains(arr.ArrestId));

            var arrestCondClearLst = _context.ArrestCondClear.Where(acc => arrestIdLst.Contains(acc.ArrestId))
                .Select(accl => new { accl.ArrestId, accl.CondOfClearance, accl.CondOfClearanceNote });

            List<int> jurisdictionIds = filArrestDetails.Where(arr => arr.ArrestCourtJurisdictionId > 0)
                    .Select(arr => arr.ArrestCourtJurisdictionId ?? 0).ToList();
            jurisdictionIds.AddRange(filArrestDetails.Where(arr => arr.OriginatingAgencyId > 0)
                    .Select(arr => arr.OriginatingAgencyId ?? 0).ToList());
            jurisdictionIds.AddRange(filArrestDetails.Select(arr => arr.ArrestingAgencyId).ToList());

            IQueryable<Agency> dbAgencyDetails = _context.Agency.Where(agn => jurisdictionIds.Contains(agn.AgencyId));
            IQueryable<PersonnelVm> dbPersonnelDetails = _context.Personnel.Select(s => new PersonnelVm
            {
                PersonLastName = s.PersonNavigation.PersonLastName,
                PersonFirstName = s.PersonNavigation.PersonFirstName,
                OfficerBadgeNumber = s.OfficerBadgeNum,
                PersonnelNumber = s.PersonNavigation.PersonNumber,
                PersonnelId = s.PersonnelId
            });

            arrestBookingDetailLst.ForEach(iaxf =>
            {
                Arrest arrestDetails = (from arr in filArrestDetails
                                        where arr.ArrestId == iaxf.ArrestId
                                        select arr).SingleOrDefault();
                if (arrestDetails == null) return;
                iaxf.BookingNo = arrestDetails.ArrestBookingNo;
                iaxf.ArraignmentDate = arrestDetails.ArrestArraignmentDate;
                iaxf.ScheduleClearDate = arrestDetails.ArrestSentenceReleaseDate;
                iaxf.ActualDaysToServe = arrestDetails.ArrestSentenceActualDaysToServe;
                iaxf.SentenceStartDate=arrestDetails.ArrestSentenceStartDate;
                iaxf.ArrestDate = arrestDetails.ArrestDate;
                iaxf.ArrestLocation = arrestDetails.ArrestLocation;
                iaxf.CaseNumber = arrestDetails.ArrestCaseNumber;
                iaxf.ArrestNotes = arrestDetails.ArrestNotes;
                iaxf.ArrestingAgencyId = arrestDetails.ArrestingAgencyId;
                iaxf.ArrestOfficerId = arrestDetails.ArrestOfficerId;
                iaxf.OriginatingAgencyId = arrestDetails.OriginatingAgencyId ?? 0;
                iaxf.ArrestType = arrestDetails.ArrestType;
                iaxf.BookDate = arrestDetails.ArrestBookingDate;
                iaxf.ArrestReceivingOfficerId = arrestDetails.ArrestReceivingOfficerId;
                iaxf.ArrestBookingStatus = arrestDetails.ArrestBookingStatus;
                iaxf.ArrestCourtJurisdictionId = arrestDetails.ArrestCourtJurisdictionId ?? 0;
                iaxf.Docket = arrestDetails.ArrestCourtDocket;
                iaxf.BailAmount = arrestDetails.BailNoBailFlag == 0 ? arrestDetails.BailAmount : 0;
                iaxf.BailNoBailFlag = !arrestDetails.BailAmount.HasValue && arrestDetails.BailNoBailFlag == 1;
                iaxf.ArrestSentenceCode = arrestDetails.ArrestSentenceCode;
                iaxf.SentReleaseDate = arrestDetails.ArrestSentenceReleaseDate;
                iaxf.ArrestSentenceIndefiniteHold = arrestDetails.ArrestSentenceIndefiniteHold;
                iaxf.WeekEnder = arrestDetails.ArrestSentenceWeekender;
                iaxf.LenDays = arrestDetails.ArrestSentenceDays;
                iaxf.BookingCompleteFlag = arrestDetails.BookingCompleteFlag == 1;

                iaxf.ArrestAgency =
                    dbAgencyDetails.SingleOrDefault(ag => ag.AgencyId == arrestDetails.ArrestingAgencyId)?.AgencyName;
                iaxf.ArrestAbbr = dbAgencyDetails.SingleOrDefault(ag =>
                    ag.AgencyId == arrestDetails.ArrestingAgencyId)?.AgencyAbbreviation;
                iaxf.OrginAgency = dbAgencyDetails.SingleOrDefault(ag =>
                    ag.AgencyId == arrestDetails.OriginatingAgencyId)?.AgencyName;
                iaxf.OrginAbbr = dbAgencyDetails.SingleOrDefault(agd =>
                    agd.AgencyId == arrestDetails.OriginatingAgencyId)?.AgencyAbbreviation;
                iaxf.Court = dbAgencyDetails.SingleOrDefault(agd =>
                    agd.AgencyId == arrestDetails.ArrestCourtJurisdictionId && agd.AgencyCourtFlag)?.AgencyName;

                iaxf.BookType = _dbLookupDetails.SingleOrDefault(lkp => lkp.LookupType == LookupConstants.ARRTYPE &&
                     lkp.LookupIndex == Convert.ToInt32(arrestDetails.ArrestType))?.LookupDescription;
                iaxf.Status = _dbLookupDetails.SingleOrDefault(lkp => lkp.LookupType == LookupConstants.BOOKSTAT &&
                     lkp.LookupIndex == arrestDetails.ArrestBookingStatus)?.LookupName;

                iaxf.RecOfficerLastName = dbPersonnelDetails
                    .Where(prn => prn.PersonnelId == arrestDetails.ArrestReceivingOfficerId)
                    .Select(prn => prn.PersonLastName).SingleOrDefault();

                iaxf.RecOfficerNumber = dbPersonnelDetails
                    .Where(prn => prn.PersonnelId == arrestDetails.ArrestReceivingOfficerId)
                    .Select(prn => prn.OfficerBadgeNumber).SingleOrDefault();

                iaxf.ArrestOfficerLastName = dbPersonnelDetails.Where(prn => prn.PersonnelId == arrestDetails.ArrestOfficerId)
                    .Select(prn => prn.PersonLastName).SingleOrDefault();

                iaxf.ArrestOfficerNumber = dbPersonnelDetails.Where(prn => prn.PersonnelId == arrestDetails.ArrestOfficerId)
                    .Select(prn => prn.OfficerBadgeNumber).SingleOrDefault();

                iaxf.ArrestOfficerText = dbAgencyDetails.SingleOrDefault(agn =>
                    agn.AgencyId == arrestDetails.ArrestingAgencyId)
                    .AgencyArrestingFlag ? arrestDetails.ArrestOfficerText : string.Empty;

                iaxf.Sentence = GetArrestSentence(arrestDetails);
            });
            arrestBookingDetailLst.ForEach(iaxf =>
             {
                 List<string> condOfClearanceLst = arrestCondClearLst.Where(
                     acc => acc.ArrestId == iaxf.ArrestId && !string.IsNullOrEmpty(acc.CondOfClearance))
                     .Select(acc => acc.CondOfClearance).ToList();
                 List<string> condOfClearanceNoteLst = arrestCondClearLst.Where(
                     acc => acc.ArrestId == iaxf.ArrestId && !string.IsNullOrEmpty(acc.CondOfClearanceNote))
                     .Select(acc => acc.CondOfClearanceNote).ToList();

                 iaxf.CondOfClear = condOfClearanceLst.Count > 0 ? string.Join(", ", condOfClearanceLst) : string.Empty;
                 iaxf.CondOfClearanceNote = condOfClearanceNoteLst.Count > 0
                     ? string.Join(", ", condOfClearanceNoteLst) : string.Empty;


                 //Get Sched_Court Details
                 LoadScheduleCourtDetails( iaxf);
                 LoadWarrantDetails(iaxf);
             });

            return arrestBookingDetailLst;
        }

        #endregion

        // Load Warrant Details 
        private void LoadWarrantDetails(ArrestBookingDetails iaxf)
        {
            iaxf.WarrantDetailLst = (from wa in _context.Warrant
                where wa.ArrestId == iaxf.ArrestId
                select new WarrantDetails
                {
                    WarrantId = wa.WarrantId,
                    Type = wa.WarrantType,
                    County = wa.WarrantCounty,
                    Description = wa.WarrantDescription,
                    WarrantBail = wa.WarrantBailType == BailType.NOBAIL ? wa.WarrantBailType
                        : wa.WarrantBailAmount.ToString(),
                    WarrantNumber = wa.WarrantNumber,
                    WarrantAgencyId = wa.WarrantAgencyId,
                }).ToList();

            if (iaxf.WarrantDetailLst.Any())
            {
                iaxf.WarrantDetailLst.ForEach(wdetails =>
                {
                    if (wdetails?.WarrantId > 0)
                    {
                        //Bail Summary is false
                        wdetails.ChargeDetails = GetChargeDetails(0, wdetails.WarrantId,
             _isBailSummary && _inmateSummaryPdfDetail.SiteOption?.ToUpper() == SiteOptionsConstants.ON);
                        if (!(wdetails.WarrantAgencyId > 0)) return;
                        string agencyName = _context.Agency
                            .SingleOrDefault(age => age.AgencyId == wdetails.WarrantAgencyId)?.AgencyName;
                        wdetails.County = !string.IsNullOrEmpty(agencyName) ? agencyName : wdetails.County;
                    }
                });
            }
            else
            {
                iaxf.ChargeDetails = GetChargeDetails(iaxf.ArrestId, 0,
                    _isBailSummary && _inmateSummaryPdfDetail.SiteOption?.ToUpper() == SiteOptionsConstants.ON);
            }
        }

        // Load Schedule-Court Details
        private void LoadScheduleCourtDetails( ArrestBookingDetails arrest)
        {
            List<AoAppointmentVm> inmateAppList = new List<AoAppointmentVm>();
            List<ScheduleCourtArrest> scheduleCourtArr =
                _context.ScheduleCourtArrest.Where(s => s.ArrestId == arrest.ArrestId).ToList();

            int[] scheduleId = scheduleCourtArr.Select(s => s.ScheduleId).ToArray();

            IQueryable<ScheduleCourt> lstScheduleInmate = _context.Schedule.OfType<ScheduleCourt>();

            lstScheduleInmate = lstScheduleInmate.Where(a =>
                !a.DeleteFlag && a.LocationId.HasValue);

            List<AoAppointmentVm> schList = lstScheduleInmate.Where(s=> scheduleId.Contains(s.ScheduleId)).Select(sch => new AoAppointmentVm
            {
                StartDate = sch.StartDate,
                EndDate = sch.EndDate,
                LocationId = sch.LocationId ?? 0,
                ScheduleId = sch.ScheduleId,
                InmateId = sch.InmateId ?? 0,
                ReasonId = sch.ReasonId,
                TypeId = sch.TypeId,
                Duration = sch.Duration,
                DeleteReason = sch.DeleteReason,
                Notes = sch.Notes,
                LocationDetail = sch.LocationDetail,
                IsSingleOccurrence = sch.IsSingleOccurrence,
                DayInterval = sch.DayInterval,
                WeekInterval = sch.WeekInterval,
                FrequencyType = sch.FrequencyType,
                QuarterInterval = sch.QuarterInterval,
                MonthOfQuarterInterval = sch.MonthOfQuarterInterval,
                MonthOfYear = sch.MonthOfYear,
                DayOfMonth = sch.DayOfMonth,
                Discriminator = "COURT"
            }).ToList();

            if (schList.Count > 0)
            {
                foreach (AoAppointmentVm sch in schList)
                {
                    ScheduleBuilder schBuilder = new ScheduleBuilder();
                    schBuilder.StartDate(sch.StartDate);
                    if (sch.EndDate.HasValue)
                    {
                        schBuilder.EndDate(sch.EndDate.Value);
                    }

                    if (sch.EndDate != null && (sch.IsSingleOccurrence && (sch.StartDate.Date != sch.EndDate.Value.Date)))
                    {
                        sch.IsSingleOccurrence = false;
                        sch.FrequencyType = FrequencyType.Daily;
                    }
                    ISchedule materializedSchedule = schBuilder
                        .Duration(sch.Duration)
                        .SingleOccurrence(sch.IsSingleOccurrence)
                        .OnDaysOfWeek(sch.DayInterval)
                        .DuringMonth(sch.WeekInterval)
                        .HavingFrequency(sch.FrequencyType)
                        .SetMonthOfYear(sch.MonthOfYear)
                        .SetDayOfMonth(sch.DayOfMonth ?? 0)
                        .DuringMonthOfQuarter(sch.MonthOfQuarterInterval)
                        .DuringQuarter(sch.QuarterInterval)
                        .Create();

                    Debug.Assert(materializedSchedule.EndDate != null,
                        "AppointmentService: materializedSchedule.EndDate != null");
                    DateRange during = new DateRange(materializedSchedule.StartDate,
                        sch.EndDate.HasValue ? materializedSchedule.EndDate.Value : DateTime.Now.AddYears(5));

                    
                        ScheduleCourt scheduleCourt = _context.ScheduleCourt
                            .Include(ii => ii.Agency).Include(ii => ii.AgencyCourtDept)
                            .SingleOrDefault(aoa => aoa.ScheduleId == sch.ScheduleId);

                        if (!(scheduleCourt is null))
                        {
                            sch.AgencyId = scheduleCourt.AgencyId;
                            sch.DeptId = scheduleCourt.AgencyCourtDeptId;
                            sch.Court = scheduleCourt.Agency?.AgencyName;
                            sch.Dept = scheduleCourt.AgencyCourtDept?.DepartmentName;

                            sch.BookingNo = _context.ScheduleCourtArrest.Where(sc => sc.ScheduleId == sch.ScheduleId)
                                .Select(ii => ii.Arrest.ArrestBookingNo)
                                .ToList();
                        }

                    if (sch.EndDate != null && (materializedSchedule.IsSingleOccurrence && (sch.StartDate.Date == sch.EndDate.Value.Date)))
                    {
                        AoAppointmentVm app = sch;
                        app.StartDate = materializedSchedule.StartDate;
                        app.EndDate = materializedSchedule.EndDate;
                        inmateAppList.Add(app);
                    }
                    else
                    {
                        inmateAppList.AddRange(materializedSchedule.Occurrences(during)
                            .Select(date => new AoAppointmentVm
                            {
                                StartDate = date.Date.Add(during.StartDateTime.TimeOfDay),
                                Duration = sch.Duration,
                                EndDate = sch.EndDate.HasValue
                                    ? date.Date.Add(during.EndDateTime.TimeOfDay)
                                    : date.Date.Add(during.StartDateTime.TimeOfDay).Add(sch.Duration),
                                EndDateNull = !sch.EndDate.HasValue,
                                LocationId = sch.LocationId,
                                ScheduleId = sch.ScheduleId,
                                InmateId = sch.InmateId,
                                ReasonId = sch.ReasonId,
                                Reason = sch.Reason,
                                ApptType = sch.ApptType,
                                BookingNo = sch.BookingNo,
                                TypeId = sch.TypeId,
                                IsSingleOccurrence = sch.IsSingleOccurrence,
                                FacilityId = sch.FacilityId,
                                InmateDetails = sch.InmateDetails,
                                LocationDetail = sch.LocationDetail,
                                Notes = sch.Notes,
                                Discriminator = sch.Discriminator,
                                AgencyId = sch.AgencyId,
                                DeptId = sch.DeptId,
                                Court = sch.Court,
                                Dept = sch.Dept
                            }));
                    }
                }
            }
          
            inmateAppList = inmateAppList.Where(s => s.StartDate > DateTime.Now).ToList();

            arrest.ScheduleCourtDetails = inmateAppList.Select(cl => new ScheduleCourtDetail
            {
                AppointmentDate = cl.StartDate,
                AgencyCourtDeptId = cl.DeptId ?? 0,
                AgencyCourtId = cl.AgencyId ?? 0,
                DepartmentName = cl.Dept,
                AppointmentLocationId = cl.LocationId
            }).FirstOrDefault();

            if (arrest.ScheduleCourtDetails == null) return;

            Agency agencyLst = _context.Agency.SingleOrDefault(ag =>
                    !string.IsNullOrEmpty(ag.AgencyAbbreviation) && ag.AgencyId == arrest.ScheduleCourtDetails.AgencyCourtId);
                arrest.ScheduleCourtDetails.SchdCourtName = agencyLst?.AgencyName;
                arrest.ScheduleCourtDetails.AgencyAbbreviation = agencyLst?.AgencyAbbreviation;
                arrest.ScheduleCourtDetails.AppointmentLocation = _context.Privileges.SingleOrDefault(prv =>
                   prv.PrivilegeId == arrest.ScheduleCourtDetails.AppointmentLocationId)?.PrivilegeDescription;
        }

        // Load AltSent Details
        private static void LoadAltSentDetails(IncarcerationDetail incd, IQueryable<AltSent> dbAltSentLst) =>
            incd.AltSentDetails = dbAltSentLst.Where(alt => alt.IncarcerationId == incd.IncarcerationId)
                .Select(alts => new AltSentDetail
                {
                    AltSentId = alts.AltSentId,
                    ProgramId = alts.AltSentProgramId,
                    IncarcerationId = alts.IncarcerationId,
                    StartDate = alts.AltSentStart,
                    ThruDate = alts.AltSentThru,
                    Adts = alts.AltSentAdts ?? 0,
                    TotalDaysServed = alts.AltSentTotalDaysServed ?? 0,
                    TotalAttend = alts.AltSentTotalAttend ?? 0,
                    ProgramAbbr = alts.AltSentProgram.AltSentProgramAbbr,
                    FacilityAbbr = alts.AltSentProgram.Facility.FacilityAbbr
                }).ToList();

        // Get Arrest Sentence
        //FR TODO: Why do we need this?!!!
        private static int GetArrestSentence(Arrest arrestDetails) => arrestDetails.ArrestSentenceCode ?? 0;

        // Load Person Description details
        private void LoadPersonDescription(int personId)
        {
            KeyValuePair<string, string> personDescription = _context.PersonDescription.Where(pdsc => pdsc.PersonId == personId)
                .Select(pd => new KeyValuePair<string, string>(pd.PersonEmployer, pd.PersonOccupation)).LastOrDefault();

            _inmateSummaryPdfDetail.Employer = personDescription.Key;
            _inmateSummaryPdfDetail.Occupation = personDescription.Value;
        }

        // Get Person Address Details
        //TODO Shouldn't it all be in Person Service?
        private PersonAddressVm GetPersonAddressDetails(int personId, string addressType) =>
            _context.Address.Where(addr => addr.PersonId == personId && addr.AddressType == addressType)
                .Select(addr => new PersonAddressVm
                {
                    AddressId = addr.AddressId,
                    Number = addr.AddressNumber,
                    Direction = addr.AddressDirection,
                    DirectionSuffix = addr.AddressDirectionSuffix,
                    Street = addr.AddressStreet,
                    Suffix = addr.AddressSuffix,
                    UnitType = addr.AddressUnitType,
                    UnitNo = addr.AddressUnitNumber,
                    Line2 = addr.AddressLine2,
                    City = addr.AddressCity,
                    State = addr.AddressState,
                    Zip = addr.AddressZip,
                    BusinessPhone = addr.PersonBusinessPhone,
                    BusinessFax = addr.PersonBusinessFax
                }).LastOrDefault();

        // Load Person Details
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
                    FknFirstName = per.FknFirstName,
                    FknLastName = per.FknLastName,
                    FknMiddleName = per.FknMiddleName,
                    FknSuffixName = per.FknSuffixName,
                    PersonPhone = per.PersonPhone,
                    PersonPlaceOfBirth = per.PersonPlaceOfBirth,
                    PersonPlaceOfBirthList = per.PersonPlaceOfBirthList,
                    PersonDob = per.PersonDob,
                    PersonDlNumber = per.PersonDlNumber,
                    PersonDlState = per.PersonDlState,
                    PersonSsn = per.PersonSsn,
                    PersonCii = per.PersonCii,
                    PersonFbiNo = per.PersonFbiNo,
                    PersonAlienNo = per.PersonAlienNo,
                    AfisNumber = per.AfisNumber,
                    PersonDoc = per.PersonDoc,
                    PersonOtherIdType = per.PersonOtherIdType,
                    PersonOtherIdNumber = per.PersonOtherIdNumber,
                    PersonAge = _commonService.GetAgeFromDob(per.PersonDob),
                    PersonBusinessPhone = per.PersonBusinessPhone,
                    //InmateCurrentTrack = per.Inmate.Select(s=>s.InmateCurrentTrack).FirstOrDefault()
                    InmateCurrentTrack = per.Inmate.FirstOrDefault().InmateCurrentTrack,
                    NoKeeper = per.Inmate.First().Incarceration.FirstOrDefault(w=>!w.ReleaseOut.HasValue).NoKeeper

                }).Single();
            IQueryable<Lookup> lookuplst = _context.Lookup.Where(w =>
                w.LookupType == LookupConstants.MARITAL && w.LookupInactive == 0);
            personDetails.MaritalStatusName = _context.PersonDescription.OrderByDescending(de => de.PersonDescriptionId)
            .Where(pd => pd.PersonId == personId).SelectMany(pd => lookuplst
            .Where(lk => lk.LookupIndex == pd.PersonMaritalStatus), (pd, lk) => lk.LookupDescription).FirstOrDefault();
            return personDetails;
        }

        // Load Contact Details
        private void LoadContactDetails(int personId)
        {
            IQueryable<Contact> contactDetails = _context.Contact.Where(cnt => cnt.TypePersonId == personId
                  && cnt.ContactRelationship != null &&
                  (!cnt.VictimNotify.HasValue || cnt.VictimNotify == 0) &&
                  cnt.ContactActiveFlag == "1");

            List<string> contactRelationShipLst = contactDetails.Select(cnt => cnt.ContactRelationship.Trim()).ToList();

            if (!contactRelationShipLst.Any()) return;
            List<int> lookupIds = _dbLookupDetails.Where(lkp => lkp.LookupType == LookupConstants.RELATIONS &&
                                                                lkp.LookupName == LookupConstants.VICTIM)
                .Select(lkp => lkp.LookupIndex).ToList();

            int filteredPersonId = contactDetails.FirstOrDefault(cnt =>
                lookupIds.Contains(Convert.ToInt32(cnt.ContactRelationship.Trim())))?.PersonId ?? 0;

            Person contactPerson = contactDetails.Where(ct => ct.PersonId != filteredPersonId).Select(de => de.Person)
                .OrderByDescending(de => de.PersonId).FirstOrDefault();
            if (contactPerson == null) return;
            _inmateSummaryPdfDetail.PersonDetails.RelationShipLastName = contactPerson.FknLastName;
            _inmateSummaryPdfDetail.PersonDetails.RelationShipFirstName = contactPerson.FknFirstName;
            _inmateSummaryPdfDetail.PersonDetails.RelationShipMiddleName = contactPerson.FknMiddleName;

            _inmateSummaryPdfDetail.PersonDetails.RelationShip = _dbLookupDetails.Where(lkp =>
                lkp.LookupType == LookupConstants.RELATIONS &&
                contactRelationShipLst.Contains(lkp.LookupIndex.ToString()))
                .OrderByDescending(lkp => lkp.LookupOrder)
                .ThenBy(lkp => lkp.LookupDescription)
                .Select(lkp => lkp.LookupDescription).LastOrDefault();

            _inmateSummaryPdfDetail.PersonDetails.ContactAddress = GetPersonAddressDetails(
                contactPerson.PersonId, AddressTypeConstants.RES);
            _inmateSummaryPdfDetail.PersonDetails.PersonPhone2 = contactPerson.PersonPhone;
        }

        #region Get Charge Details based on ArrestId or WarrrantId

        private List<PrebookCharge> GetChargeDetails(int arrestId, int warrantId, bool isActiveBail)
        {
            List<PrebookCharge> chargeDetails = new List<PrebookCharge>();
            if (arrestId <= 0 && warrantId <= 0) return chargeDetails;
            chargeDetails = (from cri in _context.Crime
                             where (warrantId > 0 ? cri.WarrantId == warrantId : cri.ArrestId == arrestId)
                                   && cri.CrimeDeleteFlag == 0 &&
                                   cri.CrimeLookupId > 0 && (!isActiveBail || string.IsNullOrEmpty(cri.CrimeType))
                             select new PrebookCharge
                             {
                                 CrimeId = cri.CrimeId,
                                 CrimeLookupId = cri.CrimeLookupId,
                                 CrimeNotes = cri.CrimeNotes,
                                 ChargeQualifierId = cri.ChargeQualifierLookup == null ? (int?)null : int.Parse(cri.ChargeQualifierLookup),
                                 CrimeCount = cri.CrimeCount ?? 0,
                                 CrimeType = cri.CrimeType,
                                 CreateDate = cri.CreateDate ?? DateTime.Now,
                                 CrimeNumber = cri.CrimeNumber ?? 0,
                                 BailNoBailFlag = cri.BailNoBailFlag == 1,
                                 BailAmount = cri.BailAmount,
                                 CrimeStatuteCode = cri.CrimeLookup.CrimeStatuteCode,
                                 CrimeCodeType = cri.CrimeLookup.CrimeCodeType,
                                 CrimeSection = cri.CrimeLookup.CrimeSection,
                                 CrimeDescription = cri.CrimeLookup.CrimeDescription,
                             }).ToList();

            if (arrestId > 0)
            {
                IQueryable<CrimeLookup> crimeLookupDetails = _context.CrimeLookup.Where(
                        clkp => chargeDetails.Select(cde => cde.CrimeLookupId).Contains(clkp.CrimeLookupId));
                chargeDetails.ForEach(cde =>
                {
                    cde.CrimeDescription = crimeLookupDetails.SingleOrDefault(clk =>
                       clk.CrimeLookupId == cde.CrimeLookupId)?.CrimeDescription;
                    cde.CrimeSection = crimeLookupDetails.SingleOrDefault(clk =>
                        clk.CrimeLookupId == cde.CrimeLookupId)?.CrimeSection;
                    cde.CrimeCodeType = crimeLookupDetails.SingleOrDefault(clk =>
                       clk.CrimeLookupId == cde.CrimeLookupId)?.CrimeCodeType;
                    cde.CrimeStatuteCode = crimeLookupDetails.SingleOrDefault(clk =>
                       clk.CrimeLookupId == cde.CrimeLookupId)?.CrimeStatuteCode;
                });
            }

            chargeDetails.AddRange((from crf in _context.CrimeForce
                where crf.DeleteFlag == 0 &&
                    (warrantId > 0 ? crf.WarrantId == warrantId && crf.ForceSupervisorReviewFlag != 1
                        : crf.ArrestId == arrestId && (!crf.DropChargeFlag.HasValue || crf.DropChargeFlag == 0)
                        && (!crf.ForceCrimeLookupId.HasValue || crf.ForceCrimeLookupId == 0) &&
                        (!crf.SearchCrimeLookupId.HasValue || crf.SearchCrimeLookupId == 0)) &&
                    (!isActiveBail || string.IsNullOrEmpty(crf.TempCrimeType))
                select new PrebookCharge
                {
                    CrimeForceId = crf.CrimeForceId,
                    CrimeNotes = crf.TempCrimeNotes,
                    ChargeQualifierId = string.IsNullOrEmpty(crf.ChargeQualifierLookup) ? 0 :
                        int.Parse(crf.ChargeQualifierLookup),
                    CrimeCount = crf.TempCrimeCount ?? 0,
                    CrimeType = crf.TempCrimeType,
                    CreateDate = crf.CreateDate ?? DateTime.Now,
                    BailNoBailFlag = crf.BailNoBailFlag == 1,
                    BailAmount = crf.BailAmount,
                    CrimeStatuteCode = crf.TempCrimeStatuteCode,
                    CrimeDescription = crf.TempCrimeDescription,
                    CrimeSection = crf.TempCrimeSection,
                    CrimeCodeType = crf.TempCrimeCodeType
                }).ToList());

            chargeDetails.ForEach(chg =>
            {
                chg.CrimeStatus = _dbLookupDetails.SingleOrDefault(lkp =>
                    lkp.LookupIndex == Convert.ToInt32(chg.CrimeType)
                    && lkp.LookupType == LookupConstants.CRIMETYPE)?.LookupDescription;
                chg.CrimeStatusAcronms = _dbLookupDetails.SingleOrDefault(lkp =>
                    lkp.LookupIndex == Convert.ToInt32(chg.CrimeType)
                    && lkp.LookupType == LookupConstants.CRIMETYPE)?.LookupDescription;
                chg.CrimeQualifier = _dbLookupDetails.SingleOrDefault(lkp =>
                    lkp.LookupIndex == chg.ChargeQualifierId &&
                    lkp.LookupType == LookupConstants.CHARGEQUALIFIER && lkp.LookupInactive == 0)?.LookupDescription;
            });
            return chargeDetails.OrderBy(ch => ch.CreateDate).ToList();
        }

        #endregion

        // Load Person-Descriptor Details
        private void LoadPersonDescriptorDetails(int personId)
        {
            _inmateSummaryPdfDetail.PersonDescriptorDetailLst = _context.PersonDescriptor
                .Where(pds => pds.PersonId == personId && pds.DeleteFlag == 0
                              && !string.IsNullOrEmpty(pds.Code))
                .Select(pdsc => new PersonDescriptorVm
                {
                    PersonDescriptorId = pdsc.PersonDescriptorId,
                    PersonId = pdsc.PersonId ?? 0,
                    Category = pdsc.Category,
                    CategoryMap = pdsc.CategoryMap,
                    ItemLocation = pdsc.ItemLocation,
                    Code = pdsc.Code,
                    DescriptorText = pdsc.DescriptorText
                }).ToList();
        }

        // Load Person Flag Alerts
        private void LoadPersonFlagAlerts(int personId)
        {
            IQueryable<PersonFlag> dbPersonFlagDetails = _context.PersonFlag.Where(pflg =>
                pflg.PersonId == personId && pflg.DeleteFlag == 0 &&
               (pflg.PersonFlagIndex > 0 || pflg.InmateFlagIndex > 0 || pflg.DietFlagIndex > 0));

            List<Lookup> personLookupDetails = _dbLookupDetails.Where(
                    lkp => lkp.LookupInactive == 0 && (!lkp.LookupNoAlert.HasValue || lkp.LookupNoAlert == 0) &&
                        lkp.LookupType == LookupConstants.PERSONCAUTION).ToList();
            List<Lookup> inmateLookupDetails = _dbLookupDetails.Where(lkp =>
                lkp.LookupInactive == 0 && lkp.LookupType == LookupConstants.TRANSCAUTION).ToList();
            List<Lookup> dietLookupDetails = _dbLookupDetails.Where(
                    lkp => lkp.LookupInactive == 0 && (!lkp.LookupNoAlert.HasValue || lkp.LookupNoAlert == 0) &&
                           lkp.LookupType == LookupConstants.DIET).ToList();
            _inmateSummaryPdfDetail.PersonAlertDetailLst = dbPersonFlagDetails.Where(pfd => pfd.PersonFlagIndex > 0)
                .Select(de => new InmateAlertVm
                {
                    Flag = LookupConstants.PERSON,
                    FlagNote = de.FlagNote,
                    LookupDescription = personLookupDetails.Where(pad =>
                        pad.LookupIndex == de.PersonFlagIndex).Select(pa => pa.LookupDescription).SingleOrDefault(),
                    LookupAlertOrder = personLookupDetails.Where(pad =>
                       pad.LookupIndex == de.PersonFlagIndex).Select(pa => pa.LookupAlertOrder).SingleOrDefault()
                }).ToList();

            _inmateSummaryPdfDetail.PersonAlertDetailLst.AddRange(
                dbPersonFlagDetails.Where(pfd => pfd.InmateFlagIndex > 0)
                    .Select(de => new InmateAlertVm
                    {
                        Flag = LookupConstants.INMATE,
                        FlagNote = de.FlagNote,
                        LookupDescription =
                            inmateLookupDetails.Where(pad => pad.LookupIndex == de.InmateFlagIndex)
                                .Select(pa => pa.LookupDescription).SingleOrDefault(),
                        LookupAlertOrder =
                            inmateLookupDetails.Where(pad => pad.LookupIndex == de.InmateFlagIndex)
                                .Select(pa => pa.LookupAlertOrder).SingleOrDefault()
                    }).ToList());

            _inmateSummaryPdfDetail.PersonAlertDetailLst.AddRange(
                dbPersonFlagDetails.Where(pfd => pfd.DietFlagIndex > 0)
                    .Select(de => new InmateAlertVm
                    {
                        Flag = LookupConstants.DIET,
                        FlagNote = de.FlagNote,
                        LookupDescription =
                            dietLookupDetails.Where(pad => pad.LookupIndex == de.DietFlagIndex)
                                .Select(pa => pa.LookupDescription).SingleOrDefault(),
                        LookupAlertOrder =
                            dietLookupDetails.Where(pad => pad.LookupIndex == de.DietFlagIndex)
                                .Select(pa => pa.LookupAlertOrder).SingleOrDefault()
                    }).ToList());
        }

        //Load Person-Classification details
        private void LoadPersonClassification(int personId)
        {
            List<Lookup> lookupslist = _context.Lookup.Where(w => w.LookupType == LookupConstants.CLASSGROUP).ToList();
            List<Lookup> lookupSubsetlist = _context.Lookup.Where(w => w.LookupType == LookupConstants.CLASSGROUPSUB).ToList();
            _inmateSummaryPdfDetail.PersonClassificationDetailLst = (from pcl in _context.PersonClassification
                where pcl.PersonId == personId && pcl.InactiveFlag == 0
                    && (!pcl.PersonClassificationDateThru.HasValue || pcl.PersonClassificationDateThru > DateTime.Now)
                select new PersonClassificationDetails
                {
                    PersonId = pcl.PersonId,
                    PersonClassificationId = pcl.PersonClassificationId,
                    ClassificationType = lookupslist
                        .SingleOrDefault(f => f.LookupIndex == pcl.PersonClassificationTypeId).LookupDescription,
                    ClassificationSubset = lookupSubsetlist
                        .SingleOrDefault(f => f.LookupIndex == pcl.PersonClassificationSubsetId).LookupDescription,
                    ClassificationTypeId = pcl.PersonClassificationTypeId,
                    ClassificationSubsetId = pcl.PersonClassificationSubsetId,
                    ClassificationStatus = pcl.PersonClassificationStatus,
                    ClassificationNotes = pcl.PersonClassificationNotes,
                    ClassificationCompleteBy = pcl.PersonClassificationCompleteBy ?? 0,
                    DateFrom = pcl.PersonClassificationDateFrom,
                    DateTo = pcl.PersonClassificationDateThru,
                    PersonnelId = pcl.CreatedByPersonnelId,
                    CreateDate = pcl.CreateDate,
                    UpdateDate = pcl.UpdateDate
                }).ToList();
        }

        // Load Observation Schedule Details
        private void LoadObservationSchDetails(int inmateId)
        {
            _inmateSummaryPdfDetail.ObservationDetails = _context.ObservationSchedule
                .Where(obs => obs.InmateId == inmateId && obs.DeleteFlag == 0
                              && obs.StartDate <= DateTime.Now && (!obs.EndDate.HasValue || obs.EndDate > DateTime.Now))
                .Select(obs => new ObservationScheduleVm
                {
                    ObservationScheduleId = obs.ObservationScheduleId,
                    Note = obs.Note,
                    ObservationType = obs.ObservationType ?? 0,
                    StartDate = obs.StartDate,
                    EndDate = obs.EndDate
                }).ToList();

            _inmateSummaryPdfDetail.ObservationDetails.ForEach(obd =>
                obd.TypeName = _dbLookupDetails.SingleOrDefault(lkp => lkp.LookupType == LookupConstants.OBSTYPE
                && lkp.LookupIndex == obd.ObservationType && lkp.LookupInactive == 0)?.LookupDescription);
        }

        // Load Aka Details
        // TODO Why do we need a separate variable?
        private void LoadAkaDetails(int personId)
        {
            List<AkaVm> akaDetailLst = _context.Aka.Where(aka => aka.PersonId == personId && aka.DeleteFlag != 1)
                .Select(aka => new AkaVm
                {
                    AkaFirstName = aka.AkaFirstName,
                    AkaLastName = aka.AkaLastName,
                    AkaMiddleName = aka.AkaMiddleName,
                    AkaSuffix = aka.AkaSuffix,
                    PersonGangName = aka.PersonGangName,
                    AkaInmateNumber = aka.AkaInmateNumber,
                    AkaSiteInmateNumber = aka.AkaSiteInmateNumber,
                    AkaDob = aka.AkaDob,
                    AkaDoc = aka.AkaDoc,
                    AkaFbi = aka.AkaFbi,
                    AkaSsn = aka.AkaSsn,
                    AkaDl = aka.AkaDl,
                    AkaDlState = aka.AkaDlState,
                    AkaCii = aka.AkaCii,
                    AkaAlienNo = aka.AkaAlienNo,
                    AkaAfisNumber = aka.AkaAfisNumber,
                    AkaOtherIdType = aka.AkaOtherIdType,
                    AkaOtherIdNumber = aka.AkaOtherIdNumber,
                    AkaOtherIdDescription = aka.AkaOtherIdDescription,
                    AkaOtherIdState = aka.AkaOtherIdState,
                    AkaOtherPhoneType = aka.AkaOtherPhoneType,
                    AkaOtherPhoneDescription = aka.AkaOtherPhoneDescription,
                    AkaOtherPhoneNumber = aka.AkaOtherPhoneNumber,
                    AkaSocialMediaAccount = aka.SocialMediaAccount,
                    AkaSocialMediaDescription = aka.SocialMediaDescription,
                    AkaSocialMediaType = aka.SocialMediaType
                }).ToList();

            _inmateSummaryPdfDetail.AkaDetailLst = new List<AkaVm>();
            if (akaDetailLst.Any())
            {
                _inmateSummaryPdfDetail.AkaDetailLst.AddRange(akaDetailLst);
            }
        }

        //Load Keep-Separate Details
        private void LoadKeepSeperateDetails(int inmateId)
        {
            IQueryable<KeepSeparate> keepSeparateLst = _context.KeepSeparate.Where(
                ks => (ks.KeepSeparateInmate1Id == inmateId || ks.KeepSeparateInmate2Id == inmateId) &&
                    ks.InactiveFlag == 0);
            List<Lookup> lookupslist = _context.Lookup.Where(w => w.LookupType == LookupConstants.CLASSGROUP).ToList();
            List<Lookup> lookupSubsetlist = _context.Lookup.Where(w => w.LookupType == LookupConstants.CLASSGROUPSUB).ToList();

            _inmateSummaryPdfDetail.KeepSeparateDetails = (from ks in keepSeparateLst
                where ks.KeepSeparateInmate1Id == inmateId && ks.KeepSeparateInmate2.InmateActive == 1
                select new KeepSeparateVm
                {
                    Reason = ks.KeepSeparateReason,
                    KeepSepInmateId = ks.KeepSeparateInmate2Id
                }).ToList();

            _inmateSummaryPdfDetail.KeepSeparateDetails.AddRange((from ks in keepSeparateLst
                where ks.KeepSeparateInmate2Id == inmateId && ks.KeepSeparateInmate1.InmateActive == 1
                select new KeepSeparateVm
                {
                    Reason = ks.KeepSeparateReason,
                    KeepSepInmateId = ks.KeepSeparateInmate1Id
                }).ToList());

            _inmateSummaryPdfDetail.KeepSeparateDetails.AddRange((from kss in _context.KeepSepSubsetInmate
                where kss.KeepSepInmate2Id == inmateId && kss.DeleteFlag == 0
                    && kss.KeepSepInmate2.InmateActive == 1
                select new KeepSeparateVm
                {
                    KeepSepAssoc = lookupslist.SingleOrDefault(f => f.LookupIndex == kss.KeepSepAssoc1Id)
                        .LookupDescription,
                    KeepSepAssocSubset = lookupSubsetlist
                        .SingleOrDefault(f => f.LookupIndex == kss.KeepSepAssoc1SubsetId).LookupDescription,
                    KeepSepAssoc1Id = kss.KeepSepAssoc1Id,
                    KeepSepAssoc1SubsetId = kss.KeepSepAssoc1SubsetId,
                }).ToList());

            _inmateSummaryPdfDetail.KeepSeparateDetails.AddRange((from ksai in _context.KeepSepAssocInmate
                where ksai.DeleteFlag == 0 && ksai.KeepSepInmate2Id == inmateId && ksai.KeepSepInmate2.InmateActive == 1
                select new KeepSeparateVm
                {
                    KeepSepAssoc = lookupslist.SingleOrDefault(f => f.LookupIndex == ksai.KeepSepAssoc1Id)
                        .LookupDescription,
                    KeepSepAssoc1Id = ksai.KeepSepAssoc1Id,
                }).ToList());

            var lstKeepSepActiveAssocSubset = (from pc in _context.PersonClassification
                from i in _context.Inmate
                where pc.PersonId == i.PersonId && pc.InactiveFlag == 0 &&
                    pc.PersonClassificationDateFrom < DateTime.Now
                    && (!pc.PersonClassificationDateThru.HasValue || pc.PersonClassificationDateThru >= DateTime.Now)
                    && i.InmateId == inmateId
                select new
                {
                    i.InmateId,
                    Assoc = pc.PersonClassificationType,
                    Subset = pc.PersonClassificationSubset,
                    AssocId = pc.PersonClassificationTypeId,
                    SubsetId = pc.PersonClassificationSubsetId
                }).ToList();

            //Assoc Keep Sep List : ASSOCIATION KEEP SEPARATE To ASSOCIATION
            _inmateSummaryPdfDetail.KeepSeparateDetails.AddRange((from ksai in _context.KeepSepAssocInmate
                from ksai2 in _context.KeepSepAssocInmate
                from ksaas in lstKeepSepActiveAssocSubset
                where ksai.DeleteFlag == 0 && ksaas.AssocId == ksai.KeepSepAssoc1Id
                    && ksai2.KeepSepInmate2Id == ksai.KeepSepInmate2Id && ksai2.DeleteFlag == 0
                select new KeepSeparateVm
                {
                    KeepSepInmateId = ksai.KeepSepInmate2Id
                }).GroupBy(n => new
            {
                n.KeepSepInmateId
            }).Select(s => s.FirstOrDefault()));

            // Subset Keep Sep List : ASSOCIATION KEEP SEPARATE To SUBSET
            _inmateSummaryPdfDetail.KeepSeparateDetails.AddRange((from kssi in _context.KeepSepSubsetInmate
                from ksaas in lstKeepSepActiveAssocSubset
                from kssi2 in _context.KeepSepSubsetInmate
                where ksaas.AssocId == kssi.KeepSepAssoc1Id
                    && ksaas.SubsetId == kssi.KeepSepAssoc1SubsetId
                    && kssi.DeleteFlag == 0
                    && kssi2.KeepSepInmate2Id == kssi.KeepSepInmate2Id
                    && kssi2.DeleteFlag == 0
                select new KeepSeparateVm
                {
                    KeepSepInmateId = kssi.KeepSepInmate2Id
                }).GroupBy(n => new
            {
                n.KeepSepInmateId
            }).Select(s => s.FirstOrDefault()));

            if (_inmateSummaryPdfDetail.KeepSeparateDetails.Any())
            {
                //KeepSepInmateDetail
                _inmateSummaryPdfDetail.KeepSeparateDetails.ForEach(ks =>
                {
                    if (ks.KeepSepInmateId > 0)
                    {
                        ks.KeepSepInmateDetail = _context.Inmate
                            .Where(x => x.InmateId == ks.KeepSepInmateId)
                            .Select(a => new PersonVm
                            {
                                PersonFirstName = a.Person.PersonFirstName,
                                PersonMiddleName = a.Person.PersonMiddleName,
                                PersonLastName = a.Person.PersonLastName,
                                InmateNumber = a.InmateNumber
                            }).SingleOrDefault();
                    }
                });
            }
        }

        public InmateSummaryPdfVm GetCaseSheetDetails(FormDetail formDetail)
        {
            _inmateSummaryPdfDetail = new InmateSummaryPdfVm();
            FormRecord formRecDetails = _context.FormRecord.SingleOrDefault(fr =>
                fr.DeleteFlag == 0 && fr.SheetArrestId == formDetail.FieldName.ArrestId);

            if (formRecDetails != null && !formDetail.Autofill)
            {
                _inmateSummaryPdfDetail = JsonConvert.DeserializeObject<InmateSummaryPdfVm>(formRecDetails.XmlData);
            }
            else
            {
                Inmate dbInmateDetails = _context.Inmate.Single(inm => inm.InmateId == formDetail.FieldName.InmateId);
                _inmateSummaryPdfDetail.PersonDetails = GetPersonDetails(dbInmateDetails.PersonId);
                GetInmateSummaryPdf(formDetail.FieldName.InmateId, InmateSummaryType.NONE);

                IQueryable<Arrest> dbArrestDetails = _context.Arrest.Where(ar => ar.ArrestId == formDetail.FieldName.ArrestId);
                List<int> jurisdictionIds =
                    dbArrestDetails.Where(arr => arr.ArrestCourtJurisdictionId > 0)
                        .Select(arr => arr.ArrestCourtJurisdictionId ?? 0).ToList();
                jurisdictionIds.AddRange(
                    dbArrestDetails.Where(arr => arr.OriginatingAgencyId > 0)
                        .Select(arr => arr.OriginatingAgencyId ?? 0).ToList());
                jurisdictionIds.AddRange(dbArrestDetails.Select(arr => arr.ArrestingAgencyId).ToList());
                jurisdictionIds.AddRange(dbArrestDetails.Select(arr => arr.ArrestBillingAgencyId ?? 0).ToList());

                IQueryable<Agency> dbAgencyDetails = _context.Agency.Where(agn => jurisdictionIds.Contains(agn.AgencyId));
                IQueryable<PersonnelVm> dbPersonnelDetails = _context.Personnel.Select(s => new PersonnelVm
                {
                    PersonLastName = s.PersonNavigation.PersonLastName,
                    PersonFirstName = s.PersonNavigation.PersonFirstName,
                    OfficerBadgeNumber = s.OfficerBadgeNum,
                    PersonnelNumber = s.PersonNavigation.PersonNumber,
                    PersonnelId = s.PersonnelId
                });
                _inmateSummaryPdfDetail.CaseDetails = dbArrestDetails.Where(arr => arr.ArrestId == formDetail.FieldName.ArrestId)
                    .Select(arr => new ArrestBookingDetails
                    {
                        ArrestId = formDetail.FieldName.ArrestId,
                        BookingNo = arr.ArrestBookingNo,
                        ArrestDate = arr.ArrestDate,
                        ArraignmentDate = arr.ArrestArraignmentDate,
                        ArrestLocation = arr.ArrestLocation,
                        CaseNumber = arr.ArrestCaseNumber,
                        ArrestNotes = arr.ArrestNotes,
                        ArrestOfficerId = arr.ArrestOfficerId,
                        ArrestingAgencyId = arr.ArrestingAgencyId,
                        OriginatingAgencyId = arr.OriginatingAgencyId ?? 0,
                        BillingAgencyId = arr.ArrestBillingAgencyId ?? 0,
                        ArrestType = arr.ArrestType,
                        BookDate = arr.ArrestBookingDate,
                        ArrestReceivingOfficerId = arr.ArrestReceivingOfficerId,
                        ArrestBookingStatus = arr.ArrestBookingStatus,
                        ArrestCourtJurisdictionId = arr.ArrestCourtJurisdictionId ?? 0,
                        Docket = arr.ArrestCourtDocket,
                        BailAmount = arr.BailNoBailFlag == 0 ? arr.BailAmount : 0,
                        BailNoBailFlag = !arr.BailAmount.HasValue && arr.BailNoBailFlag == 1,
                        BailType = arr.BailType,
                        ArrestSentenceCode = arr.ArrestSentenceCode,
                        SentReleaseDate = arr.ArrestSentenceReleaseDate,
                        ArrestSentenceIndefiniteHold = arr.ArrestSentenceIndefiniteHold,
                        WeekEnder = arr.ArrestSentenceWeekender,
                        LenDays = arr.ArrestSentenceDays,
                        BookingCompleteFlag = arr.BookingCompleteFlag == 1,
                        ArrestOfficerText = arr.ArrestOfficerText,

                        ReleaseDate = arr.IncarcerationArrestXref.Select(de => de.ReleaseDate).FirstOrDefault(),
                        ReleaseReason = arr.IncarcerationArrestXref.Select(de => de.ReleaseReason).FirstOrDefault(),
                        ReleaseNotes = arr.IncarcerationArrestXref.Select(de => de.ReleaseNotes).FirstOrDefault(),
                        ReleaseOfficerId = arr.IncarcerationArrestXref.Select(de => de.ReleaseOfficerId).FirstOrDefault(),

                        RecOfficerLastName = dbPersonnelDetails
                    .Where(prn => prn.PersonnelId == arr.ArrestReceivingOfficerId)
                    .Select(prn => prn.PersonLastName).SingleOrDefault(),

                        RecOfficerBadgeNumber = dbPersonnelDetails
                    .Where(prn => prn.PersonnelId == arr.ArrestReceivingOfficerId)
                    .Select(prn => prn.OfficerBadgeNumber).SingleOrDefault(),

                        ArrestOfficerLastName = dbPersonnelDetails.Where(prn => prn.PersonnelId == arr.ArrestOfficerId)
                    .Select(prn => prn.PersonLastName).SingleOrDefault(),

                        ArrestOfficerBadgeNumber = dbPersonnelDetails.Where(prn => prn.PersonnelId == arr.ArrestOfficerId)
                    .Select(prn => prn.OfficerBadgeNumber).SingleOrDefault(),

                        Sentence = GetArrestSentence(arr)
                    }).Single();

                if (_inmateSummaryPdfDetail.CaseDetails != null)
                {
                    _inmateSummaryPdfDetail.CaseDetails.SentenceDetails = _context.Arrest
                        .Where(w => w.ArrestId == _inmateSummaryPdfDetail.CaseDetails.ArrestId)
                        .Select(s => new SentenceDetailsVm
                        {
                            ArrestSentenceStartDate = s.ArrestSentenceStartDate,
                            ArrestSentenceReleaseDate = s.ArrestSentenceReleaseDate,
                            ArrestSentenceAmended = s.ArrestSentenceAmended == 1,
                            ArrestSentenceManual = s.ArrestSentenceManual == 1,
                            ArrestSentenceDays = s.ArrestSentenceDays,
                            ArrestTimeServedDays = s.ArrestTimeServedDays,
                            ArrestSentenceActualDaysToServe = s.ArrestSentenceActualDaysToServe,
                            ArrestSentenceFineDays = s.ArrestSentenceFineDays,
                            ArrestSentenceDaysStayed = s.ArrestSentenceDaysStayed,
                            ArrestSentenceType = s.ArrestSentenceType,
                            ArrestSentenceFindings = s.ArrestSentenceFindings,
                            ArrestSentenceDescription = s.ArrestSentenceDescription
                        }).Single();

                    _inmateSummaryPdfDetail.CaseDetails.ArrestAgency =
                        dbAgencyDetails.SingleOrDefault(ag =>
                            ag.AgencyId == _inmateSummaryPdfDetail.CaseDetails.ArrestingAgencyId)?.AgencyName;
                    _inmateSummaryPdfDetail.CaseDetails.BillingAgency =
                        dbAgencyDetails
                            .SingleOrDefault(ag => ag.AgencyId == _inmateSummaryPdfDetail.CaseDetails.BillingAgencyId)
                            ?.AgencyName;
                    _inmateSummaryPdfDetail.CaseDetails.ArrestAbbr =
                        dbAgencyDetails.SingleOrDefault(ag =>
                                ag.AgencyId == _inmateSummaryPdfDetail.CaseDetails.ArrestingAgencyId)?
                            .AgencyAbbreviation;
                    _inmateSummaryPdfDetail.CaseDetails.OrginAgency =
                        dbAgencyDetails.SingleOrDefault(ag =>
                            ag.AgencyId == _inmateSummaryPdfDetail.CaseDetails.OriginatingAgencyId)?.AgencyName;
                    _inmateSummaryPdfDetail.CaseDetails.OrginAbbr =
                        dbAgencyDetails.SingleOrDefault(agd =>
                        agd.AgencyId == _inmateSummaryPdfDetail.CaseDetails.OriginatingAgencyId)?.AgencyAbbreviation;
                    _inmateSummaryPdfDetail.CaseDetails.Court = dbAgencyDetails.SingleOrDefault(agd =>
                            agd.AgencyId == _inmateSummaryPdfDetail.CaseDetails.ArrestCourtJurisdictionId
                            && agd.AgencyCourtFlag)?.AgencyName;

                    _inmateSummaryPdfDetail.CaseDetails.BookType = _context.Lookup.SingleOrDefault(lkp =>
                        lkp.LookupType == LookupConstants.ARRTYPE &&
                        lkp.LookupIndex == Convert.ToInt32(_inmateSummaryPdfDetail.CaseDetails.ArrestType))
                            ?.LookupDescription;
                    _inmateSummaryPdfDetail.CaseDetails.Status = _context.Lookup.SingleOrDefault(
                        lkp => lkp.LookupType == LookupConstants.BOOKSTAT &&
                        lkp.LookupIndex == _inmateSummaryPdfDetail.CaseDetails.ArrestBookingStatus)?.LookupName;

                    if (_inmateSummaryPdfDetail.CaseDetails.ReleaseOfficerId > 0)
                        _inmateSummaryPdfDetail.CaseDetails.ReleaseOfficer = dbPersonnelDetails.SingleOrDefault(prn =>
                                    prn.PersonnelId == _inmateSummaryPdfDetail.CaseDetails.ReleaseOfficerId);

                    _inmateSummaryPdfDetail.CaseDetails.ArrestOfficerText = dbAgencyDetails.SingleOrDefault(agn =>
                        agn.AgencyId == _inmateSummaryPdfDetail.CaseDetails.ArrestingAgencyId)?.AgencyArrestingFlag ?? false
                            ? _inmateSummaryPdfDetail.CaseDetails.ArrestOfficerText : string.Empty;

                    //Load Warrant or charge details
                    LoadWarrantDetails(_inmateSummaryPdfDetail.CaseDetails);
                }

                //Get Incarceration Name
                _inmateSummaryPdfDetail.IncNameDetails = _context.Incarceration.Where(inc => inc.InmateId == formDetail.FieldName.InmateId)
                    .OrderByDescending(inc => inc.IncarcerationId)
                    .Select(inc => new PersonDetailVM
                    {
                        LastName = inc.UsedPersonLast,
                        FirstName = inc.UsedPersonFrist,
                        MiddleName = inc.UsedPersonMiddle,
                        Suffix = inc.UsedPersonSuffix
                    }).FirstOrDefault();
            }

            if (formRecDetails != null)
            {
                _inmateSummaryPdfDetail.FormData = new Form
                {
                    FormTemplateId = formRecDetails.FormTemplatesId,
                    FormRecordId = formRecDetails.FormRecordId,
                    //Values = formRecordId == 0 ? spModel : xmlModel,
                    SignValues = _formsService.GetSignature(formRecDetails.FormRecordId, formRecDetails.FormTemplatesId),
                    NoSignatureTrack = _context.FormTemplates.FirstOrDefault(ft =>
                        ft.FormCategoryId == (int?)FormCategories.BookingSheet && ft.Inactive != 1)?.NoSignatureTrack ?? true
                };
            }
            else
            {
                FormTemplates formTemplate = _context.FormTemplates.FirstOrDefault(ft =>
                    ft.FormCategoryId == (int?)FormCategories.CaseSheet && ft.Inactive != 1);
                _inmateSummaryPdfDetail.FormData = new Form
                {
                    FormTemplateId = formTemplate?.FormTemplatesId ?? 0,
                    NoSignatureTrack = formTemplate?.NoSignatureTrack ?? true,
                    SignValues = new Signature()
                };
            }

            return _inmateSummaryPdfDetail;
        }

        public InmateSummaryPdfVm GetBookingSheetDetails(int inmateId, InmateSummaryType summaryType,
            int incarcerationId, bool autofill)
        {
            _inmateSummaryPdfDetail = new InmateSummaryPdfVm();
            FormRecord formRecDetails = _context.FormRecord.SingleOrDefault(fr => fr.BooksSheetIncarcerationId == incarcerationId);

            _inmateSummaryPdfDetail = formRecDetails != null && !autofill
                ? JsonConvert.DeserializeObject<InmateSummaryPdfVm>(HttpUtility.HtmlDecode(formRecDetails.XmlData))
                : GetInmateSummaryPdf(inmateId, summaryType, incarcerationId);
            if (formRecDetails != null)
            {
                _inmateSummaryPdfDetail.FormData = new Form
                {
                    FormTemplateId = formRecDetails.FormTemplatesId,
                    FormRecordId = formRecDetails.FormRecordId,
                    //Values = formRecordId == 0 ? spModel : xmlModel,
                    SignValues = _formsService.GetSignature(formRecDetails.FormRecordId, formRecDetails.FormTemplatesId),
                    NoSignatureTrack = _context.FormTemplates.FirstOrDefault(ft =>
                        ft.FormCategoryId == (int?)FormCategories.BookingSheet && ft.Inactive != 1)?.NoSignatureTrack ?? true
                };

            }
            else
            {
                FormTemplates formTemplate = _context.FormTemplates.FirstOrDefault(ft =>
                    ft.FormCategoryId == (int?)FormCategories.BookingSheet && ft.Inactive != 1);
                _inmateSummaryPdfDetail.FormData = new Form
                {
                    FormTemplateId = formTemplate?.FormTemplatesId ?? 0,
                    NoSignatureTrack = formTemplate?.NoSignatureTrack ?? true,
                    SignValues = new Signature()
                };
            }

            return _inmateSummaryPdfDetail;
        }

        public bool GetBookComplete(int inmateId)
        {
            return _context.Incarceration.FirstOrDefault(f => f.InmateId == inmateId && !f.ReleaseOut.HasValue)
                       ?.BookCompleteFlag == 1;
        }

    }
}
