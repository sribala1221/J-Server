using System;
using GenerateTables.Models;
using ServerAPI.ViewModels;
using System.Linq;
using System.Collections.Generic;
using ServerAPI.Utilities;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using ServerAPI.Interfaces;
using Newtonsoft.Json;
using System.Security.Claims;
using JwtDb.Models;
using Microsoft.AspNetCore.Identity;

namespace ServerAPI.Services
{
    public class ConsoleService : IConsoleService
    {
        private readonly AAtims _context;
        private readonly int _personnelId;
        private readonly ITrackFurlService _trackFurlService;
        private readonly IWorkCrewService _workCrewService;
        private readonly IApptService _apptService;
        private readonly IRegisterService _registerService;
        private readonly IObservationLogService _observationLogService;
        private readonly ISafetyCheckService _safetyCheckService;
        private readonly IHeadCountService _headCountService;
        private List<int> _listConsoleHousingId;
        private List<int> _listConsoleMyLocationId;
        private readonly IPhotosService _photos;
        private readonly UserManager<AppUser> _userManager;

        public ConsoleService(AAtims context, IHttpContextAccessor httpContextAccessor,
            ITrackFurlService trackFurlService,
            IWorkCrewService workCrewService, UserManager<AppUser> userManager, IApptService apptService, IRegisterService registerService,
            IObservationLogService observationLogService, ISafetyCheckService safetyCheckService,
            IHeadCountService headCountService, IPhotosService photosService)
        {
            _context = context;
            _trackFurlService = trackFurlService;
            _workCrewService = workCrewService;
            _safetyCheckService = safetyCheckService;
            _headCountService = headCountService;
            _apptService = apptService;
            _registerService = registerService;
            _observationLogService = observationLogService;
            _personnelId = Convert.ToInt32(httpContextAccessor.HttpContext.User.FindFirst("personnelId")?.Value);
            _photos = photosService;
            _userManager = userManager;
        }

        //console grid load methods
        public ConsoleVm GetConsoleDetails(ConsoleInputVm value, string hostName)
        {
            ConsoleVm console = new ConsoleVm
            {
                HousingList = new List<ConsoleHousingVm>(),
                LocationList = new List<ConsoleLocationVm>(),
                MyLocationList = new List<ConsoleLocationVm>()
            };

            if (value.IsRefresh)
            {
                _listConsoleHousingId = value.ListConsoleHousingId;
                _listConsoleMyLocationId = value.ListConsoleMyLocationId;

            }
            else
            {
                HousingLocationVm housingLocationVm =
                    LoadHousingLocationDetails(value.FacilityId, value.HousingUnitListId,
                        value.HousingUnitGroupId, hostName); //Housinglist grid

                console.HousingList = housingLocationVm.HousingList;
                console.LocationList = housingLocationVm.HousingLocationList;

                console.MyLocationList = GetMyLocationList(value.FacilityId).Where(i => i.Selected).ToList(); // MyLocationlist grid

                _listConsoleMyLocationId = console.MyLocationList.Select(s => s.PrivilegeId).ToList();

                _listConsoleHousingId = console.HousingList.Where(w => w.Selected).Select(s => s.HousingUnitListId).ToList();

                value.ListConsoleHousingId = _listConsoleHousingId;
                value.ListConsoleLocationId = console.LocationList.Count > 0 ? console.LocationList
                        .Where(w => w.Selected).Select(s => s.PrivilegeId).ToList()
                    : new List<int>();
                value.ListConsoleMyLocationId = _listConsoleMyLocationId;
            }

            ApptParameter apptParameter = new ApptParameter
            {
                HousingUnitListId = _listConsoleHousingId,
                FacilityId = value.FacilityId,
                HousingNumber = value.HousingUnitListId,
                HousingGroup = value.HousingUnitGroupId
            };

            console.SafetyCheckHousingList =
                _safetyCheckService.LoadSafetyCheckHousingList(value); // safety check housing grid

            console.SafetyCheckLocationList =
                _safetyCheckService.LoadSafetyCheckLocationList(value); // safety check location grid

            console.HeadCountHousingList = _headCountService.LoadHeadCountHousingList(value); // head count housing grid

            console.HeadCountLocationList =
                _headCountService.LoadHeadCountLocationList(value); // head count location grid

            console.ObservationLogList = _observationLogService.LoadObservationLogList(value); // observation list grid

            console.ReleaseQueueList = GetReleaseQueue(value.FacilityId, value.OperationFlag); // release list grid

            console.RequestQueueList = GetRequestQueue(value.FacilityId, value.OperationFlag); // request list grid             

            console.ConsoleEventList = FacilityEventDetails(); // facility events - currently empty!

            console.VisitDetails = GetScheduleVisitDetails(value);
          
            // console.WorkCrewList = GetWorkCrewConsoleList(value.FacilityId, value.OperationFlag); // work crew list grid
            //console.WorkCrewFurloughList = GetFurloughCount(value.FacilityId, value.OperationFlag); // work furlough list grid

            console.ProgramConsoleList = GetProgramCourseList(value); // programs list grid; for now - empty

            console.ApptConsoleList = GetApptCourtAndLocation(apptParameter); // appointment list grid

            console.FormCount = _context.FormRecord
                .Count(f => f.DeleteFlag == 0 && f.FormHousingClear == 0
                            && (f.FormTemplates.Inactive == 0 || !f.FormTemplates.Inactive.HasValue) && f.FormHousingRoute == 1 &&
                            f.Incarceration.Inmate.FacilityId == value.FacilityId
                            && _listConsoleHousingId.Contains(
                                f.Incarceration.Inmate.HousingUnit.HousingUnitListId)); // incarceration list grid details

            if (!value.OperationFlag)
            {
                console.TransferInOutList = GetTransferInOutList(value);
            }

            return console;
        }

        // load program console
        private List<ProgramAppointmentVm> GetProgramCourseList(ConsoleInputVm objParam)
        {
            DateTime todayDate = DateTime.Now;
            List<ProgramAppointmentVm> programAppList = _context.ScheduleProgram.Where(a =>!a.DeleteFlag &&
                          (a.LocationId > 0 && objParam.ListConsoleLocationId.Contains(a.LocationId??0) ||
                           objParam.ListConsoleMyLocationId.Contains(a.LocationId ?? 0)) && 
                          (a.StartDate >= todayDate || (a.EndDate > todayDate && a.StartDate < todayDate)))
                .Select(sch => new ProgramAppointmentVm
                 {
                     StartDate = sch.StartDate,
                     EndDate = sch.EndDate,
                     LocationId = sch.LocationId??0,
                     LocationDetail = sch.LocationDetail,
                     CRN = sch.ProgramClass.CRN,
                     CourseName = sch.ProgramClass.ProgramCourseIdNavigation.CourseName,
                     ProgramCategory = sch.ProgramClass.ProgramCourseIdNavigation.ProgramCategory
                 }).ToList();

            List<int> locationIds = programAppList.Select(s => s.LocationId).ToList();
            IQueryable<Privileges> lstPrivileges = _context.Privileges
                        .Where(l => locationIds.Contains(l.PrivilegeId));

            programAppList.ForEach(item =>
            {
                if (item.LocationId > 0)
                {
                    item.Location = lstPrivileges.Single(c => c.PrivilegeId == item.LocationId).PrivilegeDescription;
                }
                if (item.LocationId > 0)
                {
                    // i have doubt in program inmate count.
                    item.InmateCount = lstPrivileges.Count(c => c.PrivilegeId == item.LocationId);
                }
            });

            return programAppList;
        }
        
        // load visitation console
        private List<VisitDetails> GetScheduleVisitDetails(ConsoleInputVm objParam)
        {

            IQueryable<Privileges> lstPrivilegeses = _context.Privileges.Where(h =>
                h.FacilityId == objParam.FacilityId && objParam.ListConsoleMyLocationId.Contains(h.PrivilegeId));
               
            List<VisitDetails> lstRegisterationDetails = _context.VisitToVisitor
                .Where(w => objParam.ListConsoleHousingId.Count > 0 && w.Visit.VisitDenyFlag == 0 && !w.Visit.CompleteVisitFlag
                            && w.Visit.StartDate.Date == DateTime.Now.Date
                            && (objParam.ListConsoleHousingId.Count == 0 || w.Visit.Inmate.HousingUnit.HousingUnitListId > 0 &&
                                objParam.ListConsoleHousingId.Contains(w.Visit.Inmate.HousingUnit.HousingUnitListId)))
                .OrderBy(o => o.Visit.CreateDate)
                .Select(s => new VisitDetails
                {
                    InmateInfo = new PersonInfoVm
                    {
                        InmateId = s.Visit.InmateId,
                        InmateNumber = s.Visit.Inmate.InmateNumber,
                        PersonLastName = s.Visit.Inmate.Person.PersonLastName,
                        PersonFirstName = s.Visit.Inmate.Person.PersonFirstName
                    },
                    Location = s.Visit.Location.PrivilegeDescription,
                    ScheduleDateTime = s.Visit.StartDate,
                    CountAsVisit = s.Visit.CountAsVisit,
                    AdultVisitorsCount = s.Visit.VisitAdditionAdultCount,
                    ChildVisitorsCount = s.Visit.VisitAdditionChildCount,
                    CompleteRegistration = s.Visit.CompleteRegistration,
                    InmateTrackingStart = s.Visit.InmateTrackingStart,
                    StartDate = s.Visit.StartDate,
                    EndDate = s.Visit.EndDate,
                    Time = s.Visit.Time,
                    Duration = s.Visit.Duration,
                    LocationId = s.Visit.LocationId ?? 0,
                    HousingDetails = new HousingDetail()
                    {
                        HousingUnitLocation = s.Visit.Inmate.HousingUnit.HousingUnitLocation
                    },
                    InmateCurrentTrack = s.Visit.Inmate.InmateCurrentTrack
                }).ToList();

            lstRegisterationDetails.ForEach(item =>
                {
                    if (item.LocationId > 0)
                    {
                        item.HousingCount = lstPrivilegeses.Count(c => c.PrivilegeId == item.LocationId);
                    }
                });
            
            return lstRegisterationDetails;
        }

        //housingUnitId && housingGroupId check for the both facility operations and cell module to loaded the same housing and location grid list       
        private HousingLocationVm LoadHousingLocationDetails(int facilityId, int housingUnitListId, int housingGroupId, string hostName)
        {
            HousingLocationVm consoleOperation = new HousingLocationVm();

            IQueryable<UserAccess> lstUserAccess = _context.UserAccess
                .Where(ucs => ucs.PersonnelId == _personnelId);

            IQueryable<HousingUnitList> lstHousingDetails =
                _context.HousingUnitList.Where(h => h.FacilityId == facilityId);

            if (housingUnitListId > 0)
            {
                consoleOperation.HousingList = lstHousingDetails.Where(h => h.HousingUnitListId == housingUnitListId)
                    .Select(i => new ConsoleHousingVm
                    {
                        HousingUnitListId = i.HousingUnitListId,
                        HousingUnitLocation = i.HousingUnitLocation,
                        HousingUnitNumber = i.HousingUnitNumber,
                        Selected = true
                    }).ToList();

                consoleOperation.HousingLocationList = _context.Privileges.Where(h =>
                        h.FacilityId == facilityId && h.HousingUnitListId == housingUnitListId)
                    .Select(p => new ConsoleLocationVm
                    {
                        PrivilegeId = p.PrivilegeId,
                        PrivilegeDescription = p.PrivilegeDescription,
                        Selected = true
                    }).ToList();
            }
            else if (housingGroupId > 0)
            {
                IQueryable<HousingGroupAssign> lstHousingGroup =
                    _context.HousingGroupAssign.Where(h => h.HousingGroupId == housingGroupId
                                                           && h.DeleteFlag == 0 &&
                                                           h.HousingGroup.FacilityId == facilityId);

                consoleOperation.HousingList = lstHousingGroup.Where(w =>
                        w.HousingGroupId > 0 && w.HousingUnitLocation != null && w.HousingUnitNumber != null
                        || w.HousingUnitLocation == "NO HOUSING")
                    .Select(i => new ConsoleHousingVm
                    {
                        HousingUnitListId = i.HousingUnitListId ?? 0,
                        HousingUnitLocation =
                            i.HousingUnitLocation == "NO HOUSING" ? "NO HOUSING" : i.HousingUnitLocation,
                        HousingUnitNumber = i.HousingUnitNumber == "NO HOUSING" ? "" : i.HousingUnitNumber,
                        AllowBatchSafetyCheck = i.HousingGroup.AllowBatchSafetyCheck,
                        Selected = true
                    }).ToList();

                consoleOperation.HousingLocationList = lstHousingGroup.Where(w => w.Location != null)
                    .Select(i => new ConsoleLocationVm
                    {
                        PrivilegeDescription = i.Location,
                        PrivilegeId = i.LocationId ?? 0,
                        Selected = true
                    }).ToList();
            }
            else
            {
                IQueryable<PrintWorkstationMaster> printWorkstation = _context.PrintWorkstationMaster
                    .Where(w => w.WorkStationName == hostName);

                List<int> housingUnitListIds = printWorkstation.Any(a => a.WorkStationDefaultHousingGroupId > 0)
                    ? lstUserAccess.Where(w =>
                            w.UserDefaultHousingGroup.HousingGroupAssign.Any(a => a.HousingUnitListId > 0))
                        .SelectMany(s =>
                            s.UserDefaultHousingGroup.HousingGroupAssign.Select(sm => sm.HousingUnitListId ?? 0))
                        .ToList()
                    : printWorkstation.Select(s => s.HousingUnitListId ?? 0).ToList();

                consoleOperation.HousingList = lstHousingDetails
                    .Select(i => new ConsoleHousingVm
                    {
                        HousingUnitListId = i.HousingUnitListId,
                        HousingUnitLocation = i.HousingUnitLocation,
                        HousingUnitNumber = i.HousingUnitNumber,
                        Selected = housingUnitListIds.Any(a => a == i.HousingUnitListId)
                    }).ToList(); // opeartion housing load
                consoleOperation.HousingLocationList = new List<ConsoleLocationVm>();
            }
            return consoleOperation;
        }

        #region Get and Set Checkbox Checks

        private async Task<List<int>> GetUserSettingDetails()
        {
            Claim claim = new Claim(CustomConstants.PERSONNELID, _personnelId.ToString());

            AppUser appUser = _userManager.GetUsersForClaimAsync(claim).Result.FirstOrDefault();

            IList<Claim> claims = await _userManager.GetClaimsAsync(appUser);

            Claim settings = claims.FirstOrDefault(f => f.Type == "ConsoleMyLocationSettings");

            List<int> loadSettings = settings == null
                ? new List<int>() : JsonConvert.DeserializeObject<List<int>>(settings.Value).ToList();

            return loadSettings;
        }

        //insert the my location list into grid
        public async Task<IdentityResult> InsertMyLocationList(List<ConsoleLocationVm> locationList)
        {
            IEnumerable<string> userConsoleSettingList =
                locationList.Where(w => w.Selected).Select(s => s.PrivilegeId.ToString()).AsEnumerable();
            
            Claim claim = new Claim("personnel_id", _personnelId.ToString());
            AppUser appUser = _userManager.GetUsersForClaimAsync(claim).Result.FirstOrDefault();
            IList<Claim> claims = await _userManager.GetClaimsAsync(appUser);
            Claim inmateIdClaim = claims.FirstOrDefault(f => f.Type == "ConsoleMyLocationSettings");

            IdentityResult result = inmateIdClaim == null ? await _userManager.AddClaimAsync(appUser,
                    new Claim("ConsoleMyLocationSettings", JsonConvert.SerializeObject(userConsoleSettingList)))
                : await _userManager.ReplaceClaimAsync(appUser, inmateIdClaim,
                    new Claim("ConsoleMyLocationSettings", JsonConvert.SerializeObject(userConsoleSettingList)));
            return result;
        }

        #endregion

        //operation && cell my location load
        private List<ConsoleLocationVm> GetMyLocation() => _context
            .UserConsoleSetting
            .Where(ucs => ucs.UserId == _personnelId)
            .OrderBy(o => o.Location)
            .Select(ucs => new ConsoleLocationVm
            { PrivilegeId = ucs.LocationId ?? 0, PrivilegeDescription = ucs.Location, Selected = true }
            ).ToList();

        //load the my location grid
        public List<ConsoleLocationVm> GetMyLocationList(int facilityId)
        {
            List<int> locationList = GetUserSettingDetails().Result.Count > 0 ? GetUserSettingDetails().Result.ToList() : new List<int>();

            List<ConsoleLocationVm> lstPrivilegesLocation = _context.Privileges
                .Where(pv => pv.InactiveFlag == 0 && (pv.FacilityId == facilityId
                                                      || !pv.FacilityId.HasValue ||
                                                      pv.HeadCountAssignFacilityId == facilityId))
                .Select(pv => new ConsoleLocationVm
                {
                    PrivilegeId = pv.PrivilegeId,
                    PrivilegeDescription = pv.PrivilegeDescription,
                    Selected = locationList.Any(a => a == pv.PrivilegeId)
                }).ToList();

            IEnumerable<ConsoleLocationVm> lstInmateLocation = _context.Inmate
                .Where(inm => inm.FacilityId == facilityId
                              && inm.InmateCurrentTrackId.HasValue
                              && !lstPrivilegesLocation.Select(s => s.PrivilegeId)
                                  .Contains(inm.InmateCurrentTrackId ?? 0))
                .Select(inm => new ConsoleLocationVm
                {
                    PrivilegeId = inm.InmateCurrentTrackId ?? 0,
                    PrivilegeDescription = inm.InmateCurrentTrack,
                    Selected = locationList.Any(a => a == inm.InmateCurrentTrackId)
                });

            lstPrivilegesLocation.AddRange(lstInmateLocation);

            return lstPrivilegesLocation.OrderBy(o => o.PrivilegeDescription).ToList();
        }

        //load inmate details housing and location  list
        public List<InmateVm> LoadInmateList(int facilityId, int housingUnitListId, int inmateCurrentTrackId)
        {
            IQueryable<Inmate> lstInmateDetails = _context.Inmate.Where(inc => inc.InmateActive == 1 &&
                                                                               inc.FacilityId == facilityId);

            lstInmateDetails = housingUnitListId > 0
                ? lstInmateDetails.Where(inc => inc.HousingUnit.HousingUnitListId == housingUnitListId)
                : lstInmateDetails.Where(c => !c.HousingUnitId.HasValue);

            if (inmateCurrentTrackId > 0)
            {
                lstInmateDetails = lstInmateDetails.Where(inc => inc.InmateCurrentTrackId == inmateCurrentTrackId);
            }

            List<InmateVm> lstHeadInmateDetails = lstInmateDetails
                .Select(s => new InmateVm
                {
                    InmateId = s.InmateId,
                    InmateNumber = s.InmateNumber,
                    InmateCurrentTrack = s.InmateCurrentTrack,
                    InmateActive = s.InmateActive == 1,
                    Person = new PersonVm
                    {
                        PersonLastName = s.Person.PersonLastName,
                        PersonFirstName = s.Person.PersonFirstName,
                        PersonMiddleName = s.Person.PersonMiddleName
                    },
                    HousingUnit = new HousingDetail
                    {
                        HousingUnitLocation = s.HousingUnit.HousingUnitLocation,
                        HousingUnitNumber = s.HousingUnit.HousingUnitNumber,
                        HousingUnitBedNumber = s.HousingUnit.HousingUnitBedNumber,
                        HousingUnitBedLocation = s.HousingUnit.HousingUnitBedLocation
                    },
                    PersonPhoto = _photos.GetPhotoByCollectionIdentifier(s.Person.Identifiers)
                }).Distinct().ToList();

            return lstHeadInmateDetails;
        }

        //release queue list grid
        private List<ReleaseQueueVm> GetReleaseQueueList(int facilityId, List<int> housingUnitListId, bool operationFlag)
        {
            List<ReleaseQueueVm> lstReleaseQueue = new List<ReleaseQueueVm>();
            //housingUnitListId = housingUnitListId.Where(w => w > 0).ToList();
            IQueryable<Incarceration> incarceration = _context.Incarceration
                .Where(s => !s.ReleaseOut.HasValue && s.Inmate.FacilityId == facilityId
                            && s.Inmate.InmateActive == 1);

            incarceration = !operationFlag
                ? incarceration.Where(s => housingUnitListId.Count == 0 ||
                             housingUnitListId.Contains(s.Inmate.HousingUnit.HousingUnitListId))
                : housingUnitListId.Count > 0 ? incarceration.Where(s => housingUnitListId.Count == 0 ||
                                                        housingUnitListId.Contains(s.Inmate.HousingUnit.HousingUnitListId)) :
                    incarceration;

            ReleaseQueueVm releaseSchdQueue = new ReleaseQueueVm
            {
                ReleaseConsoleType = ConsoleReleaseConstants.SCHEDULEDTODAY,
                ReleaseConsoleCount = incarceration.Count(s =>
                    s.OverallFinalReleaseDate.HasValue && s.OverallFinalReleaseDate <= DateTime.Now
                    && (!s.ReleaseClearFlag.HasValue || s.ReleaseClearFlag == 0))
            };
            lstReleaseQueue.Add(releaseSchdQueue);

            ReleaseQueueVm releaseClearQueue = new ReleaseQueueVm
            {
                ReleaseConsoleType = ConsoleReleaseConstants.CLEAREDFORRELEASE,
                ReleaseConsoleCount = incarceration.Count(s => s.ReleaseClearFlag == 1
                                                               && s.TransportFlag.GetValueOrDefault() == 0)
            };
            lstReleaseQueue.Add(releaseClearQueue);

            ReleaseQueueVm releaseTransQueue = new ReleaseQueueVm
            {
                ReleaseConsoleType = ConsoleReleaseConstants.AWAITINGTRANSPORT,
                ReleaseConsoleCount = incarceration.Count(s => s.ReleaseClearFlag == 1
                                                               && s.TransportFlag == 1)
            };
            lstReleaseQueue.Add(releaseTransQueue);

            return lstReleaseQueue;

        }

        //release click history 
        public List<ReleaseQueueVm> GetReleaseQueueDetails(int facilityId, string selectReleaseQueue, List<int> housingUnitListId, bool operationFlag)
        {
            housingUnitListId = housingUnitListId.Where(w => w > 0).ToList();
            List<ReleaseQueueVm> listReleaseInmate = new List<ReleaseQueueVm>();

            List<ReleaseQueueVm> lstReleasedetails = _context.Incarceration
                .Where(i => i.Inmate.InmateActive == 1
                            && !i.ReleaseOut.HasValue && i.Inmate.FacilityId == facilityId)
                .Select(i => new ReleaseQueueVm
                {
                    InmateId = i.Inmate.InmateId,
                    InmateNumber = i.Inmate.InmateNumber,
                    OverallFinalReleaseDate = i.OverallFinalReleaseDate,
                    ReleaseClearDate = i.ReleaseClearDate,
                    TransportScheduleDate = i.TransportScheduleDate,
                    PersonDetail = new PersonnelVm
                    {
                        PersonLastName = i.Inmate.Person.PersonLastName,
                        PersonMiddleName = i.Inmate.Person.PersonMiddleName,
                        PersonFirstName = i.Inmate.Person.PersonFirstName
                    },
                    HousingDetail = new HousingDetail
                    {
                        HousingUnitLocation = i.Inmate.HousingUnit.HousingUnitLocation,
                        HousingUnitNumber = i.Inmate.HousingUnit.HousingUnitNumber,
                        HousingUnitBedNumber = i.Inmate.HousingUnit.HousingUnitBedNumber,
                        HousingUnitListId = i.Inmate.HousingUnit.HousingUnitListId
                    },
                    PersonPhoto = _photos.GetPhotoByCollectionIdentifier(i.Inmate.Person.Identifiers),
                    InmateCurrentTrak = i.Inmate.InmateCurrentTrack,
                    ReleaseClearFlag = i.ReleaseClearFlag,
                    TransportFlag = i.TransportFlag
                }).OrderBy(s => s.PersonDetail.PersonLastName).ThenBy(t => t.PersonDetail.PersonFirstName)
                .ThenBy(t => t.PersonDetail.PersonMiddleName).ToList();

            if (!operationFlag)
            {
                lstReleasedetails = lstReleasedetails.Where(i => housingUnitListId.Count == 0 ||
                               housingUnitListId.Contains(i.HousingDetail.HousingUnitListId ?? 0)).ToList();
            }

            if (operationFlag && housingUnitListId.Count > 0)
            {
                lstReleasedetails = lstReleasedetails.Where(i => housingUnitListId.Contains(i.HousingDetail.HousingUnitListId ?? 0)).ToList();
            }

            listReleaseInmate = selectReleaseQueue switch
            {
                ConsoleReleaseConstants.SCHEDULEDTODAY => lstReleasedetails.Where(s =>
                        s.OverallFinalReleaseDate.HasValue && s.OverallFinalReleaseDate <= DateTime.Now &&
                        (!s.ReleaseClearFlag.HasValue || s.ReleaseClearFlag == 0))
                    .ToList(),
                ConsoleReleaseConstants.CLEAREDFORRELEASE => lstReleasedetails.Where(s =>
                        s.ReleaseClearFlag == 1 && s.TransportFlag.GetValueOrDefault() == 0)
                    .ToList(),
                ConsoleReleaseConstants.AWAITINGTRANSPORT => lstReleasedetails
                    .Where(s => s.ReleaseClearFlag == 1 && s.TransportFlag == 1)
                    .ToList(),
                _ => listReleaseInmate
            };

            return listReleaseInmate;

        }

        //request queue
        private List<RequestQueueVm> GetRequestQueuelist(int facilityId, List<int> housingUnitListId)
        {
            //  housingUnitListId = housingUnitListId.Where(w => w > 0).ToList();
            List<RequestQueueVm> lstRequestQueue = new List<RequestQueueVm>();

            int[] lstRequestLookupIds = _context.RequestActionUserGroup
                .Where(s => s.DeleteFlag == 0 && s.GroupId == 1)
                .Select(s => s.RequestActionLookupId).ToArray();

            List<RequestQueueVm> requestActionLookup = _context.Request.Where(rl =>
                    rl.RequestActionLookup.FacilityId == facilityId
                    && lstRequestLookupIds.Contains(rl.RequestActionLookupId)
                    //&& rl.Inmate.InmateActive == 1
                    && rl.RequestActionLookup.ShowInFlag == 1
                    && (housingUnitListId.Count == 0 ||
                              (housingUnitListId.Contains(rl.HousingUnitListId ?? 0))))
                //   && _listConsoleHousingId.Contains(rl.HousingUnitListId ?? 0))
                .Select(rl => new RequestQueueVm
                {
                    RequestId = rl.RequestId,
                    ClearedBy = rl.ClearedBy,
                    PendingBy = rl.PendingBy,
                    InactiveFlag = rl.RequestActionLookup.InactiveFlag ? 1 : 0
                }).Distinct().ToList();

            RequestQueueVm releasePendingQueue = new RequestQueueVm
            {
                RequestConsoleType = RequestType.PENDINGREQUEST,
                RequestConsoleCount = requestActionLookup.Count(rl => rl.ClearedBy.GetValueOrDefault() == 0
                    && rl.PendingBy.GetValueOrDefault() == 0 && rl.InactiveFlag == 0)
            };
            lstRequestQueue.Add(releasePendingQueue);

            RequestQueueVm releaseAssgnQueue = new RequestQueueVm
            {
                RequestConsoleType = RequestType.ASSIGNEDREQUEST,
                RequestConsoleCount = requestActionLookup.Count(rl => rl.ClearedBy.GetValueOrDefault() == 0
                    && rl.PendingBy.GetValueOrDefault() == _personnelId)
            };
            lstRequestQueue.Add(releaseAssgnQueue);

            int[] lstRequesTraktId = _context.RequestTrack
                .Where(rl => rl.ResponseInmateReadFlag.GetValueOrDefault() == 0 && rl.ResponseInmateFlag == 1)
                .Select(s => s.RequestId).ToArray();

            requestActionLookup = requestActionLookup.Where(rl => lstRequesTraktId.Contains(rl.RequestId)).ToList();

            RequestQueueVm releaseRespQueue = new RequestQueueVm
            {
                RequestConsoleType = RequestType.RESPONSES,
                RequestConsoleCount = requestActionLookup.Count
            };
            lstRequestQueue.Add(releaseRespQueue);

            return lstRequestQueue;
        }

        //form routing grid list details
        public IncarcerationFormListVm LoadIncarcerationFormDetails(ConsoleInputVm value)
        {
            IncarcerationFormListVm incarcerationFormDetails = new IncarcerationFormListVm();

            IQueryable<FormRecord> lstRecordsDetails = _context.FormRecord
                .Where(f => f.DeleteFlag == 0 && f.FormHousingClear == 0 &&
                            f.Incarceration.Inmate.FacilityId == value.FacilityId
                            && (f.FormTemplates.Inactive ?? 0) == 0 && f.FormHousingRoute == 1
                            && value.ListConsoleHousingId.Contains(
                                f.Incarceration.Inmate.HousingUnit.HousingUnitListId));

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
                    XmlData = s.XmlData,
                    FormCategoryFolderName = s.FormTemplates.FormCategory.FormCategoryFolderName,
                    HtmlFileName = s.FormTemplates.HtmlFileName,
                    FormTemplatesId = s.FormTemplatesId,
                    FormInterfaceFlag = s.FormTemplates.FormInterfaceFlag,
                    FormInterfaceSent = s.FormInterfaceSent,
                    FormInterfaceByPassed = s.FormInterfaceBypassed,
                    CreateDate = s.CreateDate,
                    CreatedBy = lstPersonnel.SingleOrDefault(w => w.PersonnelId == s.CreateBy),
                    UpdateDate = s.UpdateDate,
                    FormCategoryFilterId = s.FormTemplates.FormCategoryFilterId,
                    FilterName = s.FormTemplates.FormCategoryFilter.FilterName,
                    HousingDetail = new HousingDetail
                    {
                        HousingUnitLocation = s.Incarceration.Inmate.HousingUnit.HousingUnitLocation,
                        HousingUnitBedNumber = s.Incarceration.Inmate.HousingUnit.HousingUnitBedNumber,
                        HousingUnitNumber = s.Incarceration.Inmate.HousingUnit.HousingUnitNumber
                    },
                    InmateNumber = s.Incarceration.Inmate.InmateNumber,
                    PersonnelDetails = new PersonnelVm
                    {
                        PersonLastName = s.Incarceration.Inmate.Person.PersonLastName,
                        PersonFirstName = s.Incarceration.Inmate.Person.PersonFirstName
                    }
                }).ToList();

            List<FormTemplateCount> formTemplateCounts = new List<FormTemplateCount>
            {
                //All filter grid list           
                new FormTemplateCount
                {
                    CategoryName = CommonConstants.ALL.ToString(),
                    Count = lstRecordsDetails.Count()
                },
                //No filters grid list            
                new FormTemplateCount
                {
                    CategoryName = ConsoleFormConstants.NOFILTER,
                    Count = lstRecordsDetails.Count(f => f.FormTemplates.FormCategoryFilterId == 0
                                                         || !f.FormTemplates.FormCategoryFilterId.HasValue)
                }
            };

            //Another filters grid list
            formTemplateCounts.AddRange(_context.FormTemplates
                .Where(w => w.FormCategoryFilterId.HasValue)
                .Select(s => new FormTemplateCount
                {
                    CategoryId = s.FormCategoryFilter.FormCategoryFilterId,
                    CategoryName = s.FormCategoryFilter.FilterName,
                    Count = lstRecordsDetails.Count(i => i.FormTemplates.FormCategoryFilterId == s.FormCategoryFilterId)
                }).Distinct().ToList());

            incarcerationFormDetails.FormTemplateCountList = formTemplateCounts;

            return incarcerationFormDetails;
        }

        private List<object> FacilityEventDetails()
        {
            return null;
        }

        //in progress list grid
        private List<RegisterDetails> GetConsoleVisitor(SearchRegisterDetails searchDetails)
        {
            List<RegisterDetails> consoleVisitorDetails = new List<RegisterDetails>();
            if (!(_listConsoleHousingId is null))
            {
                consoleVisitorDetails = _registerService.GetVisitorList(searchDetails).Where(s =>
                    _listConsoleHousingId.Contains(s.HousingUnitListId ?? 0)
                    && s.VisitorStatus == VisitorStatus.InProgress).ToList();
            }

            return consoleVisitorDetails;
        }

        //release grid list
        private List<ReleaseQueueVm> GetReleaseQueue(int facilityId, bool operationFlag) => operationFlag
                ? GetReleaseQueueList(facilityId, _listConsoleHousingId, true)
                : GetReleaseQueueList(facilityId, _listConsoleHousingId, false);

        //request grid list
        private List<RequestQueueVm> GetRequestQueue(int facilityId, bool operationFlag)
        {
            List<int> housingUnitListId = new List<int>();
            List<RequestQueueVm> getReleseDetails = operationFlag
                    ? GetRequestQueuelist(facilityId, housingUnitListId)
                    : GetRequestQueuelist(facilityId, _listConsoleHousingId);

            return getReleseDetails;
        }

        //work Crew list grid
        private List<WorkCrewVm> GetWorkCrewConsoleList(int facilityId, bool operationFlag)
        {
            List<int> housingUnitListId = new List<int>();
            List<WorkCrewVm> workCrewEntriesDetails =
                operationFlag
                    ? _workCrewService.GetWorkCrewEntriesCount(facilityId, housingUnitListId)
                    : _workCrewService.GetWorkCrewEntriesCount(facilityId, _listConsoleHousingId);

            return workCrewEntriesDetails;
        }

        //work Furlough list grid
        private List<WorkCrewFurloughCountVm> GetFurloughCount(int facilityId, bool operationFlag)
        {

            List<WorkCrewFurloughCountVm> workFurloughDetails =
                operationFlag
                    ? _trackFurlService.GetFurloughCount(facilityId, null)
                    : _trackFurlService.GetFurloughCount(facilityId, _listConsoleHousingId);

            return workFurloughDetails;
        }

        //appointment court and location 
        private ApptTrackingVm GetApptCourtAndLocation(ApptParameter objApptParameter) =>
            _apptService.GetConsoleAppointmentCourtAndLocation(objApptParameter);

        //clear form record
        public async Task<int> ClearFormRecord(IncarcerationForms incarcerationForms)
        {
            FormRecord formRecord = _context.FormRecord.Single(s =>
                    s.FormRecordId == incarcerationForms.FormRecordId &&
                    s.FormTemplatesId == incarcerationForms.FormTemplatesId);

            formRecord.FormHousingClear = 1;
            formRecord.FormHousingClearBy = _personnelId;
            formRecord.FormHousingClearDate = DateTime.Now;

            return await _context.SaveChangesAsync();
        }

        //batch safety check grid
        public BatchSafetyCheckVm LoadBatchSafetyCheckList(ConsoleInputVm value)
        {
            BatchSafetyCheckVm batchSafetyCheck = new BatchSafetyCheckVm();

            List<HousingDetail> lstSafetyCheckHousing = _context.HousingUnit.Where(w =>
                    w.FacilityId == value.FacilityId
                    && w.Inmate.Where(we => we.InmateActive == 1).Select(s => s.HousingUnitId).Contains(w.HousingUnitId)
                    && value.ListConsoleHousingId.Contains(w.HousingUnitListId))
                .Select(s => new HousingDetail
                {
                    FacilityAbbr = s.Facility.FacilityAbbr,
                    FacilityId = value.FacilityId,
                    HousingUnitListId = s.HousingUnitListId,
                    HousingUnitLocation = s.HousingUnitLocation,
                    HousingUnitNumber = s.HousingUnitNumber
                }).Distinct().ToList();


            batchSafetyCheck.SafetyCheckHousing = _context.HousingUnitScheduleSafteyCheck.Where(w =>
                    w.FacilityId == value.FacilityId
                    && lstSafetyCheckHousing.Select(s => s.HousingUnitListId).Contains(w.HousingUnitListId ?? 0))
                .Select(s => new SafetyCheckVm
                {
                    FacilityAbbr = s.Facility.FacilityAbbr,
                    HousingUnitLocation = s.HousingUnitLocation,
                    HousingUnitNumber = s.HousingUnitNumber,
                    FacilityId = value.FacilityId,
                    HousingUnitListId = s.HousingUnitListId,
                    LastEntry = s.LastEntry,
                    LateEntryMaxMin = s.LateEntryMaxMin,
                    IntervalMinutes = s.IntervalMinutes,
                    LateEntryNoteRequired = s.LateEntryNoteRequired,
                    PersonNameDetails = new PersonnelVm
                    {
                        PersonLastName = s.LastEntryByNavigation.PersonNavigation.PersonLastName,
                        OfficerBadgeNumber = s.LastEntryByNavigation.OfficerBadgeNum
                    },
                    InmateCount = s.HousingUnitList.HousingUnit.Count,
                    HideSafetyHousingFlag = true,
                    RequiredEndTime = s.RequireEndTime
                }).ToList();


            batchSafetyCheck.SafetyCheckLocation = _context.Privileges
                .Where(s => s.SafetyCheckFlag == 1 && s.FacilityId.HasValue
                            && (value.ListConsoleMyLocationId.Contains(s.PrivilegeId) ||
                                value.ListConsoleLocationId.Contains(s.PrivilegeId)))
                .OrderBy(s => s.PrivilegeDescription)
                .Select(s => new SafetyCheckVm
                {
                    PrivilegesId = s.PrivilegeId,
                    PrivilegesDescription = s.PrivilegeDescription,
                    SafetyCheckLateEntryMax = s.SafetyCheckLateEntryMax,
                    SafetyCheckBatchNotAllowed = s.SafetyCheckBatchNotAllowed,
                    SafetyCheckIntervalMinutes = s.SafetyCheckIntervalMinutes,
                    FacilityName = s.Facility1.FacilityAbbr,
                    FacilityId = value.FacilityId,
                    SafetyCheckFlag = s.SafetyCheckFlag ?? 0,
                    PersonNameDetails = new PersonnelVm
                    {
                        PersonLastName = s.SafetyCheckLastEntryByNavigation.PersonNavigation.PersonLastName,
                        OfficerBadgeNumber = s.SafetyCheckLastEntryByNavigation.OfficerBadgeNum
                    },
                    LastEntry = s.SafetyCheckLastEntry,
                    InmateCount = s.Inmate.Count,
                    HideSafetyLocationFlag = true,
                    RequiredEndTime = s.SafetyCheckRequireEndTime
                }).Where(s => s.InmateCount > 0).Distinct().ToList();

            return batchSafetyCheck;

        }

        // insert batch safety check
        public async Task<int> InsertBatchSafetyCheck(List<SafetyCheckVm> safetyCheckInput)
        {
            // housing batch safety check insert 
            List<SafetyCheck> listSafetyCheck = safetyCheckInput.Select(s => new SafetyCheck
            {
                FacilityId = s.FacilityId,
                HousingUnitLocation = !string.IsNullOrEmpty(s.HousingUnitLocation) ? s.HousingUnitLocation : null,
                HousingUnitNumber = !string.IsNullOrEmpty(s.HousingUnitNumber) ? s.HousingUnitNumber : null,
                SafetyCheckLateEntryFlag = s.LateEntryNoteRequired,
                SafetyCheckNote = s.SafetyCheckNote,
                SafetyCheckCheckCount = s.SafetyCheckCount,
                SafetyCheckNoCheckCount = s.SafetyNoCheckCount,
                CreateBy = _personnelId,
                CreateDate = DateTime.Now,
                Location = !string.IsNullOrEmpty(s.PrivilegesDescription) ? s.PrivilegesDescription : null,
                LocationId = s.PrivilegesId > 0 ? s.PrivilegesId : default(int?),
                SafetyCheckOccured = s.SafetyCheckOccured,
                HousingUnitListId = s.HousingUnitListId > 0 ? s.HousingUnitListId : default,
                SafetyCheckEnded = s.EndDateTime
            }).ToList();

            _context.SafetyCheck.AddRange(listSafetyCheck);

            foreach (SafetyCheckVm safetyCheckIn in safetyCheckInput.Where(a => a.HousingUnitListId > 0))
            {
                // select the housing safety check list using housing unit list id
                HousingUnitScheduleSafteyCheck lstHousingUnitScheduleSafteyCheck = _context
                    .HousingUnitScheduleSafteyCheck.SingleOrDefault(h => h.FacilityId == safetyCheckIn.FacilityId &&
                        h.HousingUnitListId == safetyCheckIn.HousingUnitListId && (!h.DeleteFlag.HasValue || h.DeleteFlag == 0));

                // insert the bed no in db
                List<SafetyCheckBed> lstsafetyCheckBed = _context.HousingUnit
                    .OrderBy(h => h.HousingUnitBedNumber)
                    .Where(h => h.FacilityId == safetyCheckIn.FacilityId &&
                                h.HousingUnitListId == safetyCheckIn.HousingUnitListId
                                && (!h.HousingUnitInactive.HasValue || h.HousingUnitInactive == 0) && h.HousingUnitBedNumber != null)
                    .Select(s => new SafetyCheckBed
                    {
                        SafetyCheckId = listSafetyCheck.Select(s1 => s1.SafetyCheckId).FirstOrDefault(),
                        HousingUnitBedNumber = s.HousingUnitBedNumber,
                        CheckFlag = 1,
                        CheckNote = string.Empty,
                        CreateDate = DateTime.Now,
                        CreateBy = _personnelId
                    }).ToList();

                _context.SafetyCheckBed.AddRange(lstsafetyCheckBed);

                if (lstHousingUnitScheduleSafteyCheck?.LastEntry == null) continue;
                if (lstHousingUnitScheduleSafteyCheck.LastEntry.Value.Date >= safetyCheckIn.TotalCurrentDate) continue;
                lstHousingUnitScheduleSafteyCheck.LastEntry = safetyCheckIn.TotalCurrentDate;
                lstHousingUnitScheduleSafteyCheck.LastEntryBy = _personnelId;
                await _context.SaveChangesAsync();
            }

            // location batch safety check insert 
            List<Privileges> privileges = _context.Privileges
                .Where(w => listSafetyCheck.Where(s => s.LocationId > 0).Select(s => s.LocationId)
                    .Contains(w.PrivilegeId)).ToList();

            privileges.ForEach(p =>
            {
                p.SafetyCheckLastEntry = listSafetyCheck.Single(s => s.LocationId == p.PrivilegeId)
                    .SafetyCheckOccured;
                p.SafetyCheckLastEntryBy = _personnelId;
            });

            return await _context.SaveChangesAsync();

        }

        private List<KeyValuePair<int, string>> GetTransferInOutList(ConsoleInputVm value) => new List<KeyValuePair<int, string>>
        {
            new KeyValuePair<int, string>(_context.Inmate.Count(w => w.InmateCurrentTrackNavigation.TransferFlag == 1
                 && w.InmateCurrentTrackNavigation.InactiveFlag == 0 && w.InmateActive == 1 && w.FacilityId == value.FacilityId
                 && value.ListConsoleHousingId.Contains(w.HousingUnit.HousingUnitListId)),"Transfer Out"),

            new KeyValuePair<int, string>(_context.Inmate.Count(w => w.InmateCurrentTrackNavigation.TransferFlag == 1
                 && w.InmateCurrentTrackNavigation.InactiveFlag == 0 && w.InmateActive == 1 && w.FacilityId == value.FacilityId
                 && value.ListConsoleLocationId.Contains(w.InmateCurrentTrackId??0)),"Transfer In")

        };

        public ConsoleVm GetLocationId(int facilityId, int housingUnitListId, int housingGroupId,string hostName) =>
            new ConsoleVm
            {
                LocationList = LoadHousingLocationDetails(
                    facilityId, housingUnitListId, housingGroupId, hostName).HousingLocationList,
                MyLocationList = GetMyLocation()
            };
    }
}