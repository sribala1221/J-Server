using GenerateTables.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using ServerAPI.ViewModels;

// ReSharper disable once CheckNamespace
namespace ServerAPI.Services
{
    public class InmateHeaderService : IInmateHeaderService
    {
        #region Properties

        private readonly AAtims _context;
        private readonly ICommonService _commonService;
        private readonly IPersonCharService _iPersonCharService;
        private readonly IPersonPhotoService _iPersonPhotoService;
        private readonly IPersonService _personService;
        private readonly IAlertService _iAlertService;
        private readonly IKeepSepAlertService _iKeepSepAlertService;
        private readonly IBookingService _iBookingService;
        private readonly IPhotosService _photos;
        private readonly IFacilityHousingService _facilityHousingService;

        #endregion

        #region Constructor

        public InmateHeaderService(AAtims context, ICommonService commonService, IPersonCharService iPersonCharService,
            IPersonPhotoService iPersonPhotoService, IAlertService iAlertService,
            IKeepSepAlertService iKeepSepAlertService, IBookingService iBookingService, IPersonService personService, IPhotosService photosService,
            IFacilityHousingService facilityHousingService)
        {
            _context = context;
            _commonService = commonService;
            _iPersonCharService = iPersonCharService;
            _iPersonPhotoService = iPersonPhotoService;
            _iAlertService = iAlertService;
            _iKeepSepAlertService = iKeepSepAlertService;
            _iBookingService = iBookingService;
            _personService = personService;
            _photos = photosService;
            _facilityHousingService = facilityHousingService;
        }

        #endregion

        #region Methods

        public InmateHeaderVm GetInmateHeaderDetail(int inmateId, bool showMedicalAlerts = true)
        {
            InmateHeaderVm ihvm = GetInmateBasicInfo(inmateId);

            //Moniker           //MONIKER
            LoadMoniker(ihvm);

            //Classification    //CLASSIFY
            LoadClassification(ihvm);

            //Inmate Fee details
            LoadFee(ihvm);

            //To get workcrew details   //WORK CREW
            LoadWorkCrewAndFurlough(ihvm);

            //Inmate Photo File Path
            LoadPhotoFilePath(ihvm);

            //Inmate Photo
            ihvm.InmatePhoto = _iPersonPhotoService.GetIdentifier(ihvm.MyInmateDetail.PersonId);

            //Inmate ObservationLog
            ihvm.LstObservationLog = _iAlertService.GetObservationLog(ihvm.MyInmateDetail.InmateId);

            //Inmate Association
            ihvm.LstAssociation = _iKeepSepAlertService.GetAssociation(ihvm.MyInmateDetail.PersonId);

            //Inmate Active Booking         //STATUS OVERALL
            //Active Incarceration
            LoadActiveIncarceration(ihvm);

            //Inmate Alerts
            ihvm.InmateAlerts = _iAlertService.GetAlerts(ihvm.MyInmateDetail.PersonId,
                ihvm.MyInmateDetail.InmateId);

            //Medical Alerts
            if (showMedicalAlerts)
            {
                ihvm.InmateAlerts.AddRange(_iAlertService.GetMedicalAlerts(ihvm.MyInmateDetail.PersonId));
            }

            //Privilege Alerts
            ihvm.LstPrivilegesAlerts = _iAlertService.GetPrivilegesAlert(ihvm.MyInmateDetail.InmateId);

            //Inmate KeepSep
            if (ihvm.MyInmateDetail.InmateActive)
                ihvm.LstKeepSep = _iKeepSepAlertService.GetInmateKeepSep(ihvm.MyInmateDetail.InmateId);

            //Inmate All Incarcerations and Bookings
            ihvm.IncarcerationAndBooking = _iBookingService.GetIncarcerationAndBookings(ihvm.MyInmateDetail.InmateId);

            //Visitation Details
            LoadVisitation(ihvm);

            return ihvm;
        }

        #endregion

        #region Moniker

        private void LoadMoniker(InmateHeaderVm ihvm)
        {
            List<string> lstMonicker = _context.Aka.Where(w => w.PersonId == ihvm.MyInmateDetail.PersonId
                                                               && w.PersonGangName != null)
                .Select(s => s.PersonGangName).ToList();

            ihvm.MyInmateDetail.Moniker = string.Join(", ", lstMonicker);
        }

        #endregion

        #region Classification

        private void LoadClassification(InmateHeaderVm ihvm)
        {
            if (ihvm.MyInmateDetail.InmateClassificationId.HasValue)
            {
                ihvm.MyInmateDetail.Classification = _context.InmateClassification.
                    Single(w => w.InmateId == ihvm.MyInmateDetail.InmateId &&
                                         w.InmateClassificationId == ihvm.MyInmateDetail.InmateClassificationId)
                    ?.InmateClassificationReason;
            }
        }

        #endregion

        #region Fee

        private void LoadFee(InmateHeaderVm ihvm)
        {
            ihvm.MyInmateDetail.InmateFee = _context.AccountAoInmate
                .Where(w => w.InmateId == ihvm.MyInmateDetail.InmateId)
                .Select(w => new InmateFeeVm
                {
                    Balance = w.BalanceInmate,
                    Pending = w.BalanceInmatePending,
                    Fee = w.BalanceInmateFee
                }).SingleOrDefault();
        }

        #endregion

        #region WorkCrew And WorkFurlough

        private void LoadWorkCrewAndFurlough(InmateHeaderVm ihvm)
        {
            IQueryable<WorkCrewLookup> lstWorkCrews =
                _context.WorkCrew.Where(wc => (!wc.EndDate.HasValue || wc.EndDate > DateTime.Today)
                    && wc.DeleteFlag == 0 && ihvm.MyInmateDetail.InmateActive
                    && wc.InmateId == ihvm.MyInmateDetail.InmateId).Select(wcl => wcl.WorkCrewLookup);

            if (!lstWorkCrews.Any()) return;
            //Work Crew
            ihvm.MyInmateDetail.WorkCrew = lstWorkCrews
                .LastOrDefault(wc => !wc.WorkFurloughFlag.HasValue && wc.CrewName != "") ? .CrewName;

            //Work Furlough
            ihvm.MyInmateDetail.WorkFurlough = lstWorkCrews
                .LastOrDefault(wc => wc.WorkFurloughFlag.HasValue && wc.CrewName != "") ? .CrewName;
        }

        #endregion

        #region Photo File Path

        private void LoadPhotoFilePath(InmateHeaderVm ihvm)
        {
            ihvm.MyInmateDetail.PhotoFilePath = _photos.GetPhotoByPersonId(ihvm.MyInmateDetail.PersonId);
        }

        #endregion

        #region Active Incarceration

        private void LoadActiveIncarceration(InmateHeaderVm ihvm)
        {
            var myIncarceration = (from inc in _context.Incarceration
                where !inc.ReleaseOut.HasValue && inc.InmateId == ihvm.MyInmateDetail.InmateId
                select new
                {
                    inc.IncarcerationId,
                    FinalRelease = inc.OverallFinalReleaseDate
                }).SingleOrDefault();

            ihvm.MyInmateDetail.IncarcerationId = myIncarceration?.IncarcerationId;
            ihvm.MyInmateDetail.SchdRelease = myIncarceration?.FinalRelease; //RELEASE
        }

        #endregion

        #region Visitation Details

        private void LoadVisitation(InmateHeaderVm ihvm)
        {
            if (ihvm.MyInmateDetail.HousingDetail == null) return;
            //Visitation Details        //VISIT.SCHD
            List<VisitationDetails> lstVisitationDetails = (from v in _context.HousingUnitVisitation
                where v.FacilityId == ihvm.MyInmateDetail.FacilityId
                      && v.HousingUnitLocation == ihvm.MyInmateDetail.HousingDetail.HousingUnitLocation
                      && v.ExcludeClassString != ihvm.MyInmateDetail.Classification
                      && v.HousingUnitNumber == ihvm.MyInmateDetail.HousingDetail.HousingUnitNumber
                select new VisitationDetails
                {
                    VisitDay = v.VisitationDay,
                    VisitFromDate = v.VisitationFrom,
                    VisitToDate = v.VisitationTo
                }).ToList();

            lstVisitationDetails.AddRange((from v in _context.HousingUnitVisitation
                where v.FacilityId == ihvm.MyInmateDetail.FacilityId
                      && v.HousingUnitLocation == ihvm.MyInmateDetail.HousingDetail.HousingUnitLocation
                      && v.ExcludeClassString != ihvm.MyInmateDetail.Classification
                      && v.HousingUnitNumber == string.Empty
                select new VisitationDetails
                {
                    VisitDay = v.VisitationDay,
                    VisitFromDate = v.VisitationFrom,
                    VisitToDate = v.VisitationTo
                }).ToList());

            // Order by VisitDay starting from Monday to Sunday
            ihvm.MyInmateDetail.VisitationDetails =
                lstVisitationDetails.Distinct().OrderBy(v => (int?) ((DayOfWeek) Enum.Parse(typeof(DayOfWeek),
                    v.VisitDay.Substring(0, 1).ToUpper() + v.VisitDay.Substring(1).ToLower()) + 6)%7)
                .ThenBy(v => v.VisitFromDate).ToList();
        }

        #endregion

        //Get Inmate Basic Information
        public InmateHeaderVm GetInmateBasicInfo(int inmateId)
        {
            InmateDetailVm ih =
                (from inm in _context.Inmate
                 where inm.InmateId == inmateId
                 select new InmateDetailVm
                 {
                     PersonId = inm.PersonId,
                     InmateId = inmateId,
                     InmateActive = inm.InmateActive == 1,
                     InmateNumber = inm.InmateNumber,
                     CurrentTrack = inm.InmateCurrentTrack,
                     SchdRelease = inm.InmateScheduledReleaseDate,
                     InmateBalance = inm.InmateBalance,
                     InmateDepositeBalance = inm.InmateDepositedBalance,
                     InmateDept = inm.InmateDebt,
                     WorkCrew = inm.WorkCrew.WorkCrewLookup.CrewName,
                     HousingUnitId = inm.HousingUnitId,
                     PersonalInventory = inm.InmatePersonalInventory,
                     InmateClassificationId = inm.InmateClassificationId,
                     InmateCurrentTrackId = inm.InmateCurrentTrackId,
                     FacilityId = inm.FacilityId,
                     PhonePin = inm.PhonePin,
                     Facility = inm.Facility.FacilityAbbr
                 }).Single();

            Incarceration incarceration = _context.Incarceration.SingleOrDefault(inca =>
                inca.InmateId == inmateId && !inca.ReleaseOut.HasValue);
            if (incarceration != null)
            {
                ih.IncFirstName = incarceration.UsedPersonFrist;
                ih.IncMiddleName = incarceration.UsedPersonMiddle;
                ih.IncLastName = incarceration.UsedPersonLast;
                ih.IncSuffix = incarceration.UsedPersonSuffix;
            }

            ih.PersonDetail = _personService.GetPersonDetails(ih.PersonId);

            ih.Characteristics = _iPersonCharService.GetCharacteristics(ih.PersonId);

            if (ih.InmateActive && ih.HousingUnitId.HasValue)
            {
                ih.HousingDetail = _facilityHousingService.GetHousingDetails(ih.HousingUnitId.Value);
            }

            InmateHeaderVm inmateHeader = new InmateHeaderVm
            {
                MyInmateDetail = ih
            };

            return inmateHeader;
        }
    }
}
