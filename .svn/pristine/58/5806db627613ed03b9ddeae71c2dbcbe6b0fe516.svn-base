using System;
using System.Collections.Generic;
using System.Linq;
using GenerateTables.Models;
using ServerAPI.ViewModels;
using ServerAPI.Utilities;

// ReSharper disable once CheckNamespace
namespace ServerAPI.Services
{
    public class ApptService : IApptService
    {
        private readonly AAtims _context;
        private readonly ICommonService _commonService;
        private List<ApptTracking> _apptTracking;
        private readonly IAppointmentService _appointmentService;
        private readonly IPhotosService _photo;
        private readonly IFacilityPrivilegeService _facilityPrivilegeService;

        List<KeyValuePair<int, string>> CourtLocation { get; set; }

        public ApptService(AAtims context, ICommonService commonService, IAppointmentService appointmentService,
            IPhotosService photoService, IFacilityPrivilegeService facilityPrivilegeService)
        {
            _context = context;
            _commonService = commonService;
            _appointmentService = appointmentService;
            _photo = photoService;
            _facilityPrivilegeService = facilityPrivilegeService;
        }

        public ApptTrackingVm GetApptCourtAndLocation(ApptParameter objApptParameter)
        {
            //To get Appointment Details
            GetTodaysAppointments(objApptParameter);

            ApptTrackingVm apptTrackingVm = new ApptTrackingVm
            {
                //To get Location List
                LocationList = _facilityPrivilegeService.GetTrackingLocationList(objApptParameter.FacilityId).ToList(),

                //To get Court List
                CourtList = GetCourtList(),

                //To get Appt Type Lookup List
                TypeLookupList = GetTypeLookupList(),

                //Appointment Details by Location
                CntLocApptTracking = GetAppointmentListByLocation(),

                //Appointment Details by Court
                CntCourtApptTracking = GetAppointmentListByCourt(),

                //To Get Refusal Reason
                RefusalReason = _commonService.GetLookupList(LookupConstants.TRACKREFUSALREAS)
                    .Select(l => new LookupVm { LookupDescription = l.LookupDescription, LookupIndex = l.LookupIndex })
                    .ToList()
            };

            apptTrackingVm.LocApptTracking = GetApptHousingList(apptTrackingVm.CntLocApptTracking, 0, 0, false);

            apptTrackingVm.CourtApptTracking = GetApptHousingList(apptTrackingVm.CntCourtApptTracking);

            return apptTrackingVm;
        }

        private List<KeyValuePair<int, string>> GetCourtList()
        {
            return _context.Agency.Where(w =>
                    w.AgencyCourtFlag && (!w.AgencyInactiveFlag.HasValue || w.AgencyInactiveFlag.Value == 0))
                .Select(s => new KeyValuePair<int, string>(s.AgencyId, s.AgencyName))
                .OrderBy(o => o.Value).ToList();
        }

        private List<KeyValuePair<int, string>> GetTypeLookupList()
        {
            return _commonService.GetLookupList(LookupConstants.APPTYPE)
                .Select(s => new KeyValuePair<int, string>(s.LookupIndex, s.LookupDescription))
                .ToList();
        }

        private void GetTodaysAppointments(ApptParameter objApptParameter)
        {
            //Taking the list of appointment details from appointment service
            List<AoAppointmentVm> lstApptDetails =
                _appointmentService.ListAoAppointments(objApptParameter.FacilityId, null, DateTime.Now, DateTime.Now, objApptParameter.ShowDeleted);

            // Appointment Details
            _apptTracking = lstApptDetails.Where(w =>
                w.InmateDetails.FacilityId == objApptParameter.FacilityId &&
                (!objApptParameter.LocationId.HasValue || w.LocationId == objApptParameter.LocationId)
                && (!objApptParameter.ApptTypeId.HasValue || w.TypeId == objApptParameter.ApptTypeId)
                && w.InmateDetails.InmateActive == 1 && w.StartDate.Date <= DateTime.Now.Date
                && !w.ProgramClassId.HasValue
            ).Select(s =>
                new ApptTracking
                {
                    ScheduleId = s.ScheduleId,
                    ApptStartDate = s.StartDate.ToString("HH:mm"),
                    ApptEndDate = s.EndDate?.ToString("HH:mm"),
                    ApptLocation = s.Location,
                    ApptLocationId = s.LocationId,
                    ReoccurFlag = s.IsSingleOccurrence ? 1 : 0,
                    DeleteFlag = s.DeleteFlag,
                    AppointmentEndDate = s.EndDate,
                    AppointmentStartDate = s.StartDate,
                    AgencyId = s.AgencyId,
                    DeptId = s.DeptId
                }).ToList();

            //To get Court Agency
            CourtLocation = _context.Agency
                .Where(x => x.AgencyCourtFlag
                            && (x.AgencyInactiveFlag == 0 || !x.AgencyInactiveFlag.HasValue))
                .Select(x => new KeyValuePair<int, string>(x.AgencyId, x.AgencyName))
                .OrderBy(x => x.Value).ToList();

            _apptTracking.ForEach(item =>
            {
                if (item.AgencyId > 0)
                {
                    KeyValuePair<int, string> courtAppt = CourtLocation.LastOrDefault(w => w.Key == item.AgencyId);

                    item.AgencyName = courtAppt.Value;
                    item.AgencyId = courtAppt.Key;
                }

            });
        }

        private List<ApptTracking> GetAppointmentListByLocation(bool isFromTransfer = false)
        {
            List<Privileges> lstPrivileges = _context.Privileges.Select(p => new Privileges
            {
                PrivilegeId = p.PrivilegeId,
                PrivilegeDescription = p.PrivilegeDescription,
                TrackingAllowRefusal = p.TrackingAllowRefusal,
                AppointmentAlertApptEndDate = p.AppointmentAlertApptEndDate,
                FacilityId = p.FacilityId,
                TrackEnrouteFlag = p.TrackEnrouteFlag,
                HousingUnitLocation = p.HousingUnitLocation,
                HousingUnitNumber = p.HousingUnitNumber,
                HousingUnitListId = p.HousingUnitListId,
                HousingGroupId = p.HousingGroupId,
                AppointmentRequireCourtLink = p.AppointmentRequireCourtLink,
                TransferFlag = p.TransferFlag ?? 0
            }).ToList();

            List<ApptTracking> lstLocationApptTracking = _apptTracking.GroupBy(g => new
            {
                g.ApptStartDate,
                g.ApptEndDate,
                //g.AppointmentStartDate,
                //g.AppointmentEndDate,
                g.ApptLocation,
                g.ApptLocationId
            }).Select(s => new ApptTracking
            {
                ApptStartDate = s.Key.ApptStartDate,
                ApptEndDate = s.Key.ApptEndDate,
                ApptLocation = s.Key.ApptLocation,
                //AppointmentStartDate = s.Key.AppointmentStartDate,
                //AppointmentEndDate = s.Key.AppointmentEndDate,
                ApptLocationId = s.Key.ApptLocationId,
                Count = s.Count(),
                ReoccurFlag = s.Select(x => x.ReoccurFlag).FirstOrDefault(),
                LstAppointmentId = s.Select(x => x.ScheduleId).ToArray()
            }).OrderBy(o => o.ApptStartDate).ToList();

            int[] locationIds = lstLocationApptTracking
                .Select(i => i.ApptLocationId ?? 0).ToArray();

            if (isFromTransfer)
            {
                int[] transferLocationIds = lstPrivileges
                    .Where(p => p.TransferFlag.Value == 1 && p.FacilityId > 0
                                && locationIds.Contains(p.PrivilegeId))
                    .Select(p => p.PrivilegeId).ToArray();

                lstLocationApptTracking = lstLocationApptTracking
                    .Where(a => transferLocationIds.Contains(a.ApptLocationId ?? 0))
                    .ToList();
            }

            //skip court appointment
            int[] courtLocationIds = lstPrivileges
                .Where(p => p.AppointmentRequireCourtLink && p.FacilityId > 0
                            && locationIds.Contains(p.PrivilegeId))
                .Select(p => p.PrivilegeId).ToArray();

            lstLocationApptTracking = lstLocationApptTracking
                .Where(a => !courtLocationIds.Contains(a.ApptLocationId ?? 0))
                .ToList();

            lstLocationApptTracking.ForEach(itm =>
            {
                itm.DisplayFlag = _apptTracking.Count(w =>
                                      w.ApptStartDate == itm.ApptStartDate
                                      && w.ReoccurFlag == 1 && w.ApptEndDate == itm.ApptEndDate
                                      && w.ApptLocationId == itm.ApptLocationId &&
                                      itm.ApptAlertEndDate
                                      && w.AppointmentEndDate.HasValue &&
                                      w.AppointmentEndDate <= DateTime.Now
                                      && w.ApptEndDate != "23:59") > 0;
                itm.EndDateFlag = _context.InmateAppointmentTrack.Any(w =>
                    itm.LstAppointmentId.Contains(w.ScheduleId) && w.InmateTrakDateIn == null);
                itm.VisitFlag = _context.Visit.Any(w =>
                    itm.LstAppointmentId.Contains(w.ScheduleId));
                itm.Privileges = lstPrivileges.Where(w => w.PrivilegeId == itm.ApptLocationId)
                    .Select(p => new PrivilegeDetailsVm
                    {
                        PrivilegeId = p.PrivilegeId,
                        TrackingAllowRefusal = p.TrackingAllowRefusal,
                        FacilityId = p.FacilityId,
                        TrackEnrouteFlag = p.TrackEnrouteFlag,
                        PrivilegeDescription = p.PrivilegeDescription
                    }).SingleOrDefault();
                itm.HousingUnitListId = lstPrivileges.Where(w => w.PrivilegeId == itm.ApptLocationId)
                    .Select(p => p.HousingUnitListId).SingleOrDefault();
                itm.HousingGroupId = lstPrivileges.Where(w => w.PrivilegeId == itm.ApptLocationId)
                    .Select(p => p.HousingGroupId).SingleOrDefault();
                itm.RefusalFlag = lstPrivileges.Where(w => w.PrivilegeId == itm.ApptLocationId)
                     .Select(p => p.TrackingAllowRefusal == 1).SingleOrDefault();
                itm.ApptAlertEndDate = lstPrivileges.Where(w => w.PrivilegeId == itm.ApptLocationId)
                    .Select(p => p.AppointmentAlertApptEndDate == 1).SingleOrDefault();
                itm.InmateCurrentTrackFacilityId = lstPrivileges
                    .Where(w => w.PrivilegeId == itm.ApptLocationId)
                    .Select(p => p.FacilityId).SingleOrDefault();
                itm.HousingUnitLocation = lstPrivileges
                    .Where(w => w.PrivilegeId == itm.ApptLocationId)
                    .Select(p => p.HousingUnitLocation).SingleOrDefault();
                itm.HousingUnitNumber = lstPrivileges
                    .Where(w => w.PrivilegeId == itm.ApptLocationId)
                    .Select(p => p.HousingUnitNumber).SingleOrDefault();
                itm.EnrouteFlag = _context.Privileges.Find(itm.ApptLocationId).TrackEnrouteFlag == 1;
            });

            return lstLocationApptTracking;
        }

        private List<ApptTracking> GetAppointmentListByCourt(bool isFromTransfer = false)
        {
            List<Privileges> lstPrivileges = _context.Privileges.Select(p => new Privileges
            {
                PrivilegeId = p.PrivilegeId,
                PrivilegeDescription = p.PrivilegeDescription,
                TrackingAllowRefusal = p.TrackingAllowRefusal,
                AppointmentAlertApptEndDate = p.AppointmentAlertApptEndDate,
                FacilityId = p.FacilityId,
                TrackEnrouteFlag = p.TrackEnrouteFlag,
                HousingUnitLocation = p.HousingUnitLocation,
                HousingUnitNumber = p.HousingUnitNumber,
                HousingUnitListId = p.HousingUnitListId,
                HousingGroupId = p.HousingGroupId,
                TransferFlag = p.TransferFlag ?? 0
            }).ToList();

            List<ApptTracking> lstCourtApptTracking = _apptTracking
                .Where(w => w.AgencyId.HasValue).GroupBy(g => new
                {
                    g.ApptStartDate,
                    g.ApptEndDate,
                    // g.AgencyName,
                    // g.AgencyId,
                    //g.AppointmentStartDate,
                    //g.AppointmentEndDate,
                    g.ApptLocationId
                }).Select(s => new ApptTracking
                {
                    ApptStartDate = s.Key.ApptStartDate,
                    ApptEndDate = s.Key.ApptEndDate,
                    // AgencyName = s.Key.AgencyName,
                    // AgencyId = s.Key.AgencyId,
                    //AppointmentStartDate = s.Key.AppointmentStartDate,
                    //AppointmentEndDate = s.Key.AppointmentEndDate,
                    ApptLocation = s.Select(x => x.ApptLocation).FirstOrDefault(),
                    ApptLocationId = s.Select(x => x.ApptLocationId).FirstOrDefault(),
                    Count = s.Count(),
                    ReoccurFlag = s.Select(x => x.ReoccurFlag).FirstOrDefault(),
                    LstAppointmentId = s.Select(x => x.ScheduleId).ToArray()
                }).OrderBy(o => o.ApptStartDate).ToList();

            if (isFromTransfer)
            {
                int[] locationIds = lstCourtApptTracking
                    .Select(i => i.ApptLocationId ?? 0).ToArray();

                int[] transferLocationIds = _context.Privileges
                    .Where(p => p.TransferFlag.Value == 1 && p.FacilityId > 0
                                && locationIds.Contains(p.PrivilegeId))
                    .Select(p => p.PrivilegeId).ToArray();

                lstCourtApptTracking = lstCourtApptTracking
                    .Where(a => transferLocationIds.Contains(a.ApptLocationId ?? 0))
                    .ToList();
            }

            lstCourtApptTracking.ForEach(itm =>
            {
                itm.DisplayFlag = _apptTracking.Count(w =>
                                      w.ApptStartDate == itm.ApptStartDate
                                      && w.ReoccurFlag == 1 && w.ApptEndDate == itm.ApptEndDate
                                      && w.ApptLocationId == itm.ApptLocationId && itm.ApptAlertEndDate
                                      && w.AppointmentEndDate.HasValue && w.AppointmentEndDate <= DateTime.Now
                                      && w.ApptEndDate != "23:59") > 0;
            itm.EndDateFlag = _context.InmateAppointmentTrack.Any(w =>
                    itm.LstAppointmentId.Contains(w.ScheduleId) && w.InmateTrakDateIn == null);
                itm.VisitFlag = _context.Visit.Any(w =>
                    itm.LstAppointmentId.Contains(w.ScheduleId));
                itm.Privileges = lstPrivileges.Where(w => w.PrivilegeId == itm.ApptLocationId)
                    .Select(p => new PrivilegeDetailsVm
                    {
                        PrivilegeId = p.PrivilegeId,
                        TrackingAllowRefusal = p.TrackingAllowRefusal,
                        FacilityId = p.FacilityId,
                        TrackEnrouteFlag = p.TrackEnrouteFlag,
                        PrivilegeDescription = p.PrivilegeDescription
                    }).SingleOrDefault();
                itm.HousingUnitListId = lstPrivileges.Where(w => w.PrivilegeId == itm.ApptLocationId)
                    .Select(p => p.HousingUnitListId).SingleOrDefault();
                itm.HousingGroupId = lstPrivileges.Where(w => w.PrivilegeId == itm.ApptLocationId)
                    .Select(p => p.HousingGroupId).SingleOrDefault();
                itm.RefusalFlag = lstPrivileges.Where(w => w.PrivilegeId == itm.ApptLocationId)
                     .Select(p => p.TrackingAllowRefusal == 1).SingleOrDefault();
                itm.ApptAlertEndDate = lstPrivileges.Where(w => w.PrivilegeId == itm.ApptLocationId)
                    .Select(p => p.AppointmentAlertApptEndDate == 1).SingleOrDefault();
                itm.InmateCurrentTrackFacilityId = lstPrivileges
                    .Where(w => w.PrivilegeId == itm.ApptLocationId)
                    .Select(p => p.FacilityId).SingleOrDefault();
                itm.HousingUnitLocation = lstPrivileges
                    .Where(w => w.PrivilegeId == itm.ApptLocationId)
                    .Select(p => p.HousingUnitLocation).SingleOrDefault();
                itm.HousingUnitNumber = lstPrivileges
                    .Where(w => w.PrivilegeId == itm.ApptLocationId)
                    .Select(p => p.HousingUnitNumber).SingleOrDefault();
                itm.EnrouteFlag = _context.Privileges.Find(itm.ApptLocationId).TrackEnrouteFlag == 1;
            });

            return lstCourtApptTracking;
        }

        private List<ApptInmateDetails> GetApptHousingList(List<ApptTracking> lstApptTracking,
            int? housingNumber = 0, int? housingGroup = 0, bool isCourt = true)
        {
            List<ApptInmateDetails> appTracDet = new List<ApptInmateDetails>();

            List<KeyValuePair<int, int>> lstFilteredInmateHousing = new List<KeyValuePair<int, int>>();
            if (housingNumber > 0 || housingGroup > 0)
            {
                int[] schduleIds = lstApptTracking
                    .Where(t => !isCourt || t.AgencyId > 0)
                    .SelectMany(a => a.LstAppointmentId).ToArray();

                int[] lstInmateIds = _context.ScheduleInmate
                    .Where(a => schduleIds.Contains(a.ScheduleId))
                    .Select(s => s.InmateId ?? 0).ToArray();

                List<KeyValuePair<int, int>> lstInmateHousing = _context.Inmate
                    .Where(a => lstInmateIds.Contains(a.InmateId))
                    .Select(a => new KeyValuePair<int, int>(a.InmateId, a.HousingUnitId > 0
                        ? a.HousingUnit.HousingUnitListId
                        : 0))
                    .Where(a => a.Value > 0).ToList();

                if (housingNumber > 0)
                {
                    lstFilteredInmateHousing = lstInmateHousing.Where(a => a.Value == housingNumber).ToList();
                }

                if (housingGroup > 0)
                {
                    int[] housingUnitListIds = _context.HousingGroupAssign.Where(a => a.HousingGroupId == housingGroup)
                        .Select(a => a.HousingUnitListId ?? 0)
                        .Where(a => a > 0).ToArray();

                    lstFilteredInmateHousing = lstInmateHousing.Where(a => housingUnitListIds.Contains(a.Value)).ToList();
                }
            }

            lstApptTracking.ForEach(item =>
            {
                foreach (int itemApptId in item.LstAppointmentId)
                {
                    int[] lstApptInmate = { };
                    if (!isCourt)
                    {
                        ScheduleCourt scheduleCourt =
                            _context.ScheduleCourt.SingleOrDefault(a => a.ScheduleId == itemApptId && a.AgencyId > 0);
                        if(scheduleCourt == null)
                        {
                            lstApptInmate = _context.ScheduleInmate
                                .Where(a => a.ScheduleId == itemApptId)
                                .Select(s => s.InmateId ?? 0).ToArray();
                        }
                    }
                    else
                    {
                        lstApptInmate = _context.ScheduleCourt
                            .Where(a => a.ScheduleId == itemApptId && a.AgencyId > 0)
                            .Select(s => s.InmateId ?? 0).ToArray();
                    }

                    if(lstApptInmate.Length > 0)
                    {
                        foreach (int itemInmateId in lstApptInmate)
                        {
                            bool isAvailable = true;
                            if (housingGroup > 0 || housingNumber > 0)
                            {
                                if (lstFilteredInmateHousing.Count == 0)
                                {
                                    isAvailable = false;
                                }
                                else if (lstFilteredInmateHousing.Count(a => a.Key == itemInmateId) == 0)
                                {
                                    isAvailable = false;
                                }
                            }

                            if (isAvailable)
                            {
                                ApptInmateDetails lstApp = _context.Inmate
                                    .Where(i => i.InmateId == itemInmateId)
                                    .Select(x => new ApptInmateDetails
                                    {
                                        ScheduleId = itemApptId,
                                        HousingUnitId = x.HousingUnitId,
                                        HousingUnitLocation = x.HousingUnit != null ? x.HousingUnit.HousingUnitLocation : null,
                                        HousingUnitNumber = x.HousingUnit != null ? x.HousingUnit.HousingUnitNumber : null,
                                        HousingUnitListId = x.HousingUnit.HousingUnitListId,
                                        InmateDetail = new PersonTracking
                                        {
                                            PersonId = x.PersonId,
                                            InmateNumber = x.InmateNumber,
                                            InmateCurrentTrack = x.InmateCurrentTrack,
                                            InmateCurrentTrackId = x.InmateCurrentTrackId,
                                            InmateId = x.InmateId,
                                            InmateTrack = GetInmateTrack(itemApptId),
                                            HousingUnitBedLocation =
                                                x.HousingUnit != null ? x.HousingUnit.HousingUnitBedLocation : null,
                                            HousingUnitBedNumber =
                                                x.HousingUnit != null ? x.HousingUnit.HousingUnitBedNumber : null
                                        }
                                    }).FirstOrDefault();

                                if (lstApp != null)
                                {
                                    PersonVm personVm = _context.Person.Where(p => p.PersonId == lstApp.InmateDetail.PersonId)
                                        .Select(a => new PersonVm{ 
                                            PersonFirstName = a.PersonFirstName,
                                            PersonLastName = a.PersonLastName,
                                            PersonMiddleName = a.PersonMiddleName
                                        }).Single();
                                    List<Identifiers> identifiers = _context.Identifiers
                                        .Where(i => i.PersonId == lstApp.InmateDetail.PersonId).ToList();
                                    Person person = new Person
                                    { 
                                        PersonId = personVm.PersonId,
                                        Identifiers = identifiers
                                    };
                                    lstApp.InmateDetail.PhotoFilePath = _photo.GetPhotoByPerson(person);
                                    lstApp.ImageName = _photo.GetPhotoByCollectionIdentifier(identifiers);
                                    lstApp.InmateDetail.PersonFirstName = personVm.PersonFirstName;
                                    lstApp.InmateDetail.PersonLastName = personVm.PersonLastName;
                                    lstApp.InmateDetail.PersonMiddleName = personVm.PersonMiddleName;

                                    lstApp.ApptStartDate = item.ApptStartDate;
                                    lstApp.ApptEndDate = item.ApptEndDate;
                                    lstApp.ApptLocation = item.ApptLocation;
                                    lstApp.ApptLocationId = item.ApptLocationId;
                                    lstApp.AgencyId = item.AgencyId;
                                    lstApp.AgencyName = item.AgencyName;
                                    lstApp.RefusalFlag = item.RefusalFlag;
                                    lstApp.DeleteFlag = _apptTracking.First(app => app.ScheduleId == itemApptId).DeleteFlag;
                                    lstApp.ApptAlertEndDate = item.ApptAlertEndDate;
                                    lstApp.AppointmentEndDate = _apptTracking.First(app => app.ScheduleId == itemApptId)
                                        .AppointmentEndDate;
                                    lstApp.ReoccurFlag = item.ReoccurFlag;
                                    lstApp.VisitFlag = item.VisitFlag;
                                    lstApp.HistoryScheduleId = itemApptId;
                                    appTracDet.Add(lstApp);
                                }
                            }
                        }
                    }
                }
            });

            int[] inmateIds = appTracDet.Select(a => a.InmateDetail.InmateId ?? 0).ToArray();

            List<InmateTrak> lstInmateTracks = _context.InmateTrak.Where(i =>
                inmateIds.Contains(i.InmateId)).ToList();

            appTracDet.ForEach(item =>
            {
                List<InmateTrak> lstEachInmateTracks = lstInmateTracks
                    .Where(i => i.InmateId == item.InmateDetail.InmateId)
                    .ToList();
                int inmateCurrentTrackId = item.InmateDetail.InmateCurrentTrackId ?? 0;

                item.InmateDetail.InmateCurrentTrakDateOut = lstEachInmateTracks.LastOrDefault(w =>
                        w.InmateTrakLocationId == inmateCurrentTrackId && !w.InmateTrakDateIn.HasValue)
                    ?.InmateTrakDateOut;
                item.InmateDetail.EnrouteStartOut = lstEachInmateTracks.LastOrDefault(w =>
                        w.InmateTrakLocationId == inmateCurrentTrackId && !w.InmateTrakDateIn.HasValue)
                    ?.EnrouteStartOut;
                item.InmateDetail.InmateCurrentTrakDateIn = lstEachInmateTracks.LastOrDefault(w =>
                        w.InmateTrakLocationId == inmateCurrentTrackId)
                    ?.InmateTrakDateIn;
                item.EnrouteLocationId = lstEachInmateTracks.LastOrDefault(w =>
                        w.InmateTrakLocationId == inmateCurrentTrackId)
                    ?.EnrouteFinalLocationId;
                item.DateOut = lstEachInmateTracks.Select(se => se.InmateTrakDateOut).LastOrDefault();
            });

            return appTracDet.OrderBy(app => app.HousingUnitId).ToList();
        }

        private InmateTrakVm GetInmateTrack(int appId)
        {
            return _context.InmateAppointmentTrack
                .Where(w => w.ScheduleId == appId && w.OccurenceDate.Date == DateTime.Now.Date)
                .Select(s =>
                    new InmateTrakVm
                    {
                        InmateTrackId = s.InmateTrakId,
                        InmateCurrentTrack = s.InmateTrakLocation,
                        InmateCurrentTrackId = s.InmateTrakLocationId,
                        InmateTrackDateIn = s.InmateTrakDateIn,
                        DateOut = s.InmateTrakDateOut,
                        DateIn = s.InmateTrakDateIn,
                        InmateRefused = s.InmateRefused
                    }).OrderByDescending(w => w.InmateTrackId)
                .FirstOrDefault();
        }

        public ApptTrackingVm GetConsoleAppointmentCourtAndLocation(ApptParameter objApptParameter)
        {
            //To get Appointment Details
            GetTodaysAppointments(objApptParameter);

            ApptTrackingVm apptTrackingVm = new ApptTrackingVm
            {
                //Appointment Details by Location
                CntLocApptTracking = GetAppointmentListByLocation(),

                //Appointment Details by Court
                CntCourtApptTracking = _apptTracking
                .Where(w => w.AgencyId.HasValue).GroupBy(g => new
                {
                    g.ApptStartDate,
                    g.ApptEndDate,
                    g.AgencyName,
                    g.AgencyId
                }).Select(s => new ApptTracking
                {
                    ApptStartDate = s.Key.ApptStartDate,
                    ApptEndDate = s.Key.ApptEndDate,
                    AgencyName = s.Key.AgencyName,
                    AgencyId = s.Key.AgencyId,
                    Count = s.Count(),
                    ReoccurFlag = s.Select(x => x.ReoccurFlag).FirstOrDefault(),
                    LstAppointmentId = s.Select(x => x.ScheduleId).ToArray()
                }).OrderBy(o => o.ApptStartDate).ToList()
            };

            if (objApptParameter.HousingNumber > 0 || objApptParameter.HousingGroup > 0)
            {
                List<ApptInmateDetails> lstHousing = GetApptHousingList(apptTrackingVm.CntLocApptTracking, objApptParameter.HousingNumber,
                    objApptParameter.HousingGroup, false);

                List<ApptInmateDetails> lstCourt = GetApptHousingList(apptTrackingVm.CntCourtApptTracking, objApptParameter.HousingNumber,
                    objApptParameter.HousingGroup);

                if (lstHousing.Count > 0 || lstCourt.Count > 0)
                {
                    apptTrackingVm.CntLocApptTracking.ForEach(item =>
                    {
                        item.Count = lstHousing.Count(a => a.ApptLocationId == item.ApptLocationId);
                    });

                    apptTrackingVm.CntCourtApptTracking.ForEach(item =>
                    {
                        item.Count = lstCourt.Count(a => a.AgencyId == item.AgencyId);
                    });

                    apptTrackingVm.CntLocApptTracking = apptTrackingVm.CntLocApptTracking.Where(a => a.Count > 0).ToList();
                    apptTrackingVm.CntCourtApptTracking = apptTrackingVm.CntCourtApptTracking.Where(a => a.Count > 0).ToList();
                }
                else
                {
                    apptTrackingVm.CntLocApptTracking = new List<ApptTracking>();
                    apptTrackingVm.CntCourtApptTracking = new List<ApptTracking>();
                }
            }

            return apptTrackingVm;
        }
        public TrackingSchedule GetTrackingSchedule(TrackingSchedule trackingSchedule, bool isFromTransfer = false)
        {
            ApptParameter objApptParameter = new ApptParameter()
            {
                ShowDeleted = false,
                FacilityId = trackingSchedule.FacilityId
            };
            
            //To get Appointment Details
            GetTodaysAppointments(objApptParameter);

            TrackingSchedule model = new TrackingSchedule()
            {
                //Appointment Details by Location
                CntLocApptTracking = GetAppointmentListByLocation(isFromTransfer),

                //Appointment Details by Court
                CntCourtApptTracking = GetAppointmentListByCourt(isFromTransfer),
            };

            model.LocApptTracking = GetApptHousingList(model.CntLocApptTracking, trackingSchedule.HousingUnitListId,
                trackingSchedule.HousingGroupId, false);

            model.CourtApptTracking = GetApptHousingList(model.CntCourtApptTracking, trackingSchedule.HousingUnitListId,
                trackingSchedule.HousingGroupId);

            IQueryable<PrivilegeDetailsVm> lstAllPrivilege = _context.Privileges
                .Where(w => w.RemoveFromTrackingFlag == 0 && w.InactiveFlag == 0)
                .Select(s => new PrivilegeDetailsVm
                {
                    PrivilegeId = s.PrivilegeId,
                    PrivilegeDescription = s.PrivilegeDescription,
                    FacilityId = s.FacilityId,
                    TrackingAllowRefusal = s.TrackingAllowRefusal,
                    TrackEnrouteFlag = s.TrackEnrouteFlag,
                    ApptAlertEndDate = s.AppointmentAlertApptEndDate == 1
                });

            model.LocApptTracking.ForEach(item =>
            {
                if (item.EnrouteLocationId > 0)
                {
                    item.Destination = lstAllPrivilege.Single(w => w.PrivilegeId == item.EnrouteLocationId)
                        .PrivilegeDescription;
                }
            });

            model.CourtApptTracking.ForEach(item =>
            {
                if (item.EnrouteLocationId > 0)
                {
                    item.Destination = lstAllPrivilege.Single(w => w.PrivilegeId == item.EnrouteLocationId)
                        .PrivilegeDescription;
                }
            });

            model.LstPrivileges = lstAllPrivilege.Where(w => w.FacilityId == trackingSchedule.FacilityId).ToList();
            model.LstPrivileges.AddRange(lstAllPrivilege.Where(w => w.FacilityId == 0 ||
                                                                    !w.FacilityId.HasValue).ToList());
            model.LstPrivileges =
                model.LstPrivileges.OrderBy(o => o.PrivilegeDescription).ToList();

            model.RefusalReasonList = _commonService.GetLookupKeyValuePairs(LookupConstants.TRACKREFUSALREAS);
            model.Event = trackingSchedule.Event;
            model.Housing = trackingSchedule.Housing;
            model.Location = trackingSchedule.Location;
            model.Time = trackingSchedule.Time;
            model.AllLocations = _facilityPrivilegeService.GetLocationList(trackingSchedule.FacilityId);
            model.HousingNumberList = _context.HousingUnit
                   .Where(w => (w.HousingUnitInactive == 0 || !w.HousingUnitInactive.HasValue) && w.FacilityId == trackingSchedule.FacilityId)
                   .GroupBy(x => new { x.HousingUnitLocation, x.FacilityId, x.HousingUnitListId, x.HousingUnitNumber })
                   .Select(s => new HousingDetail
                   {
                       HousingUnitLocation = s.Key.HousingUnitLocation,
                       HousingUnitNumber = s.Key.HousingUnitNumber,
                       HousingUnitListId = s.Key.HousingUnitListId,
                   }).Distinct().ToList();
            
            if(trackingSchedule.HousingGroupId > 0)
            {
                int[] housingUnitListIds = _context.HousingGroupAssign.Where(a => a.HousingGroupId == trackingSchedule.HousingGroupId)
                        .Select(a => a.HousingUnitListId ?? 0)
                        .Where(a => a > 0).ToArray();

                model.HousingNumberList = model.HousingNumberList.Where(h => housingUnitListIds.Contains(h.HousingUnitListId ?? 0))
                    .ToList();
            }

            if(trackingSchedule.HousingUnitListId > 0)
            {
                model.HousingNumberList = model.HousingNumberList
                    .Where(h => h.HousingUnitListId == trackingSchedule.HousingUnitListId).ToList();
            }

            model.HousingNumberList = model.HousingNumberList.OrderBy(o => o.HousingUnitLocation)
                .ThenBy(o => o.HousingUnitNumber).ToList();
            
            return model;
        }
    }
}