﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using GenerateTables.Models;
using JwtDb.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;
using ServerAPI.Utilities;
using ServerAPI.ViewModels;

namespace ServerAPI.Services {
    public class CellService : ICellService {
        private readonly AAtims _context;
        private readonly ICommonService _commonService;
        private readonly IFacilityPrivilegeService _facilityPrivilegeService;
        private readonly IFacilityHousingService _facilityHousingService;
        private readonly JwtDbContext _jwtDbContext;
        private readonly UserManager<AppUser> _userManager;

        public MyLogCountDetailsVm MyLogCount { get; set; }
        private readonly int _personnelId;

        public CellService (AAtims context, ICommonService commonService, IHttpContextAccessor ihHttpContextAccessor,
            IFacilityPrivilegeService facilityPrivilegeService, IFacilityHousingService facilityHousingService, JwtDbContext jwtDbContext,
            UserManager<AppUser> userManager) {
            _context = context;
            _commonService = commonService;
            _personnelId =
                Convert.ToInt32 (
                    ihHttpContextAccessor.HttpContext.User.FindFirst (AuthDetailConstants.PERSONNELID)?.Value);
            _facilityPrivilegeService = facilityPrivilegeService;
            _facilityHousingService = facilityHousingService;
            _jwtDbContext = jwtDbContext;
            _userManager = userManager;
        }
        public async Task<LogSettingDetails> GetMyLogSettings () {

            string classifyDefault = DefaultClassifySettings ();

            Claim claim = new Claim (CustomConstants.PERSONNELID, _personnelId.ToString ());

            AppUser appUser = _userManager.GetUsersForClaimAsync (claim).Result.FirstOrDefault ();

            IList<Claim> claims = _userManager.GetClaimsAsync (appUser).Result;

            Claim classifyClaim = claims.FirstOrDefault (f => f.Type == "Celllog_Settings");

            if (classifyClaim == null) {
                await _userManager.AddClaimAsync (appUser, new Claim ("Celllog_Settings", classifyDefault));
            }

            IList<Claim> claims1 = _userManager.GetClaimsAsync (appUser).Result;

            string classifyClaimLog = claims1.First (f => f.Type == "Celllog_Settings").Value;

            LogSettingDetails logParameters = JsonConvert.DeserializeObject<LogSettingDetails> (classifyClaimLog);

            return logParameters;
        }

        public List<HousingUnitGroupVm> GetHousingGroup (int facilityId) {
            List<HousingUnitGroupVm> housingGroup = (from hg in _context.HousingGroup where hg.FacilityId == facilityId && hg.DeleteFlag == 0 select new HousingUnitGroupVm {
                HousingUnitGroupId = hg.HousingGroupId,
                    GroupName = hg.GroupName,
                    GroupString = hg.GroupString,
                    LocationString = hg.LocationString,
                    HousingList = hg.HousingGroupAssign.Where (a => a.HousingUnitListId > 0)
                    .Select (hga => new KeyValuePair<string, string> (hga.HousingUnitLocation,
                        hga.HousingUnitNumber)).OrderBy (a => a.Key).ThenBy (a => a.Value).ToList ()
            }).OrderBy (o => o.GroupName).ToList ();
            return housingGroup;
        }

        public List<HousingUnitListVm> GetHousingUnit (int facilityId) {
            List<HousingUnitListVm> housingUnit = (from hu in _context.HousingUnit where hu.FacilityId == facilityId && (hu.HousingUnitInactive == 0 || !hu.HousingUnitInactive.HasValue) select new HousingUnitListVm {
                HousingUnitLocation = hu.HousingUnitLocation,
                    HousingUnitNumber = hu.HousingUnitNumber,
                    HousingUnitListId = hu.HousingUnitListId
            }).OrderBy (o => o.HousingUnitLocation).ThenBy (o => o.HousingUnitNumber).Distinct ().ToList ();
            return housingUnit;
        }

        public MyLogCountDetailsVm GetMyLogDetailsCount (MyLogRequestVm logDetails) {
            MyLogCount = new MyLogCountDetailsVm {
                CellLogDetailsLst = new List<CellLogDetailsVm> (),
                GetCount = new ViewerCount ()
            };

            logDetails.PersonnelId = _personnelId;
            logDetails.AttendanceId = GetAttendanceId (_personnelId);

            IQueryable<AttendanceAoHistory> dbAttendanceHistory = _context.AttendanceAoHistory;

            LoadClockInOutCount (logDetails, ViewerType.CLOCKIN, dbAttendanceHistory);
            LoadClockInOutCount (logDetails, ViewerType.CLOCKOUT, dbAttendanceHistory);
            LoadSetHousingCount (logDetails, dbAttendanceHistory);
            LoadSetLocationCount (logDetails, dbAttendanceHistory);
            LoadClockInOutCount (logDetails, ViewerType.SETSTATUS, dbAttendanceHistory);
            LoadOfficerLogCount (logDetails, dbAttendanceHistory);
            LoadCellCount (logDetails);
            LoadHeadCount (logDetails);
            LoadSaftyCheckCount (logDetails);
            LoadHousingInCount (logDetails);
            LoadHousingOutCount (logDetails);
            LoadTrackingInOutCount (logDetails, ViewerType.TRACKINGIN);
            LoadTrackingInOutCount (logDetails, ViewerType.TRACKINGOUT);
            LoadInmateNoteCount (logDetails);
            LoadLocationNoteCount (logDetails);
            LoadGeneralNoteCount (logDetails);
            return MyLogCount;
        }

        private void LoadClockInOutCount (MyLogRequestVm logReqDetails, string flag,
            IQueryable<AttendanceAoHistory> dbAttendanceHistory) {
            if (flag == ViewerType.SETSTATUS) {
                dbAttendanceHistory = dbAttendanceHistory
                    .Where (ah => ah.AttendanceAoStatus > 0 &&
                        ah.AttendanceAoStatusNote != ViewerType.CLOCKIN &&
                        ah.AttendanceAoStatusNote != ViewerType.CLOCKOUT);
            } else {
                int status = flag == ViewerType.CLOCKIN ? 1 : 2;
                dbAttendanceHistory = dbAttendanceHistory.Where (ah => ah.AttendanceAoStatus == status &&
                    ah.AttendanceAoStatusNote == flag);
            }
            if (logReqDetails.Hours > 0) {
                dbAttendanceHistory =
                    dbAttendanceHistory.Where (ah => ah.AttendanceAoStatusDate.HasValue &&
                        ah.AttendanceAoStatusDate >=
                        DateTime.Now.AddHours (-logReqDetails.Hours ?? 0) &&
                        ah.AttendanceAoStatusDate <= DateTime.Now);
            }
            if (logReqDetails.FromDate.HasValue) {
                dbAttendanceHistory =
                    dbAttendanceHistory.Where (
                        ah => ah.AttendanceAoStatusDate.HasValue && ah.AttendanceAoStatusDate >= logReqDetails.FromDate &&
                        ah.AttendanceAoStatusDate <= logReqDetails.ToDate);
            }

            if (logReqDetails.AttendanceId > 0) {
                dbAttendanceHistory = dbAttendanceHistory.Where (ah => ah.AttendanceAoId == logReqDetails.AttendanceId);
            }
            if (!string.IsNullOrEmpty (logReqDetails.Keyword)) {
                dbAttendanceHistory =
                    dbAttendanceHistory.Where (ah => ah.AttendanceAoStatusNote.Contains (logReqDetails.Keyword));
            }
            if (logReqDetails.PersonnelId > 0) {
                dbAttendanceHistory =
                    dbAttendanceHistory.Where (ah => ah.AttendanceAoStatusOfficerId == logReqDetails.PersonnelId);
            }

            List<CellLogDetailsVm> lstCellLog = (from ah in dbAttendanceHistory select new CellLogDetailsVm {
                CellLogId = ah.AttendanceAoHistoryId,
                    Type = flag,
                    CellDate = ah.AttendanceAoStatusDate,
                    NoteType =
                    (flag == ViewerType.CLOCKIN) ? ah.AttendanceAo.ClockInNote : ah.AttendanceAo.ClockOutNote,
                    AttendanceStatusNote = (flag == ViewerType.SETSTATUS) ? ah.AttendanceAoStatusNote : string.Empty,
                    AttendanceStatus = ah.AttendanceAoStatus,
                    LastLocTrack = ah.AttendanceAoLastLocTrack,
                    // ClearedBy = 1
            }).OrderByDescending (de => de.CellDate).Take (logReqDetails.IsMyLog ? 100 : dbAttendanceHistory.Count ()).ToList ();

            if (flag == ViewerType.SETSTATUS) {
                IQueryable<Lookup> dbLookUp = _context.Lookup.Where (l => l.LookupType == LookupCategory.ATTENDSTAT);
                lstCellLog.ForEach (item => item.LookupDescription =
                    dbLookUp.SingleOrDefault (l => l.LookupIndex == item.AttendanceStatus)?.LookupDescription);

                if (logReqDetails.IsLogSearch.IsSetStatus) {
                    MyLogCount.CellLogDetailsLst.AddRange (lstCellLog);
                }
                MyLogCount.GetCount.SetStatusCount = lstCellLog.Count;
            } else if (flag == ViewerType.CLOCKIN && logReqDetails.IsLogSearch.IsClockin) {
                MyLogCount.CellLogDetailsLst.AddRange (lstCellLog);
            } else if (flag == ViewerType.CLOCKOUT && logReqDetails.IsLogSearch.IsClockOut) {
                MyLogCount.CellLogDetailsLst.AddRange (lstCellLog);
            }

            switch (flag) {
                case ViewerType.CLOCKIN:
                    MyLogCount.GetCount.ClockInCount = lstCellLog.Count;
                    break;
                case ViewerType.CLOCKOUT:
                    MyLogCount.GetCount.ClockOutCount = lstCellLog.Count;
                    break;
            }
        }
        private void LoadClockInOutCountModify (MyLogRequestVm logReqDetails, string flag) {
            int status = flag == ViewerType.CLOCKIN ? 1 : 2;
            List<AttendanceAoHistory> lstAttendanceAoHistory = _context.AttendanceAoHistory
                .Where (ah => flag == ViewerType.SETSTATUS || (ah.AttendanceAoStatus == status) &&
                    ah.AttendanceAoStatusNote == flag).ToList ();
        }
        private void LoadSetHousingCount (MyLogRequestVm logReqDetails,
            IQueryable<AttendanceAoHistory> dbAttendanceHistory) {
            dbAttendanceHistory =
                dbAttendanceHistory.Where (ah => !string.IsNullOrEmpty (ah.AttendanceAoLastHousingLocation));

            if (logReqDetails.Hours > 0) {
                dbAttendanceHistory = dbAttendanceHistory.Where (ah => ah.AttendanceAoLastHousingDate.HasValue &&
                    ah.AttendanceAoLastHousingDate >=
                    DateTime.Now.AddHours (-logReqDetails.Hours ?? 0) &&
                    ah.AttendanceAoLastHousingDate <= DateTime.Now);
            }
            if (logReqDetails.FromDate.HasValue) {
                dbAttendanceHistory = dbAttendanceHistory.Where (ah => ah.AttendanceAoLastHousingDate.HasValue &&
                    ah.AttendanceAoLastHousingDate >=
                    logReqDetails.FromDate &&
                    ah.AttendanceAoLastHousingDate <=
                    logReqDetails.ToDate);
            }

            if (logReqDetails.AttendanceId > 0) {
                dbAttendanceHistory = dbAttendanceHistory.Where (ah => ah.AttendanceAoId == logReqDetails.AttendanceId);
            }
            if (!string.IsNullOrEmpty (logReqDetails.Keyword)) {
                dbAttendanceHistory = dbAttendanceHistory.Where (ah =>
                    string.Join (' ', ah.AttendanceAoLastHousingLocation ?? string.Empty,
                        ah.AttendanceAoLastHousingNumber ??
                        string.Empty, ah.AttendanceAoLastHousingNote ?? string.Empty).Contains (logReqDetails.Keyword));
            }
            if (logReqDetails.PersonnelId > 0) {
                dbAttendanceHistory =
                    dbAttendanceHistory.Where (ah => ah.AttendanceAoLastHousingOfficerId == logReqDetails.PersonnelId);
            }
            List<CellLogDetailsVm> lstCellLogDetails = dbAttendanceHistory.Where (ah => logReqDetails.IsLogSearch.IsSetHousing)
                .Select (ah => new CellLogDetailsVm {
                    CellLogId = ah.AttendanceAoHistoryId,
                        Type = ViewerType.SETHOUSING,
                        CellDate = ah.AttendanceAoLastHousingDate,
                        LastHousingNote = ah.AttendanceAoLastHousingNote,
                        LastHousingLocation = ah.AttendanceAoLastHousingLocation,
                        LastHousingNumber = ah.AttendanceAoLastHousingNumber,
                        //  ClearedBy = 1
                }).OrderByDescending (de => de.CellDate).Take (logReqDetails.IsMyLog ? 100 : dbAttendanceHistory.Count ()).ToList ();
            MyLogCount.GetCount.SetHousingCount = lstCellLogDetails.Count ();
            if (logReqDetails.IsLogSearch.IsSetHousing) {
                MyLogCount.CellLogDetailsLst.AddRange (lstCellLogDetails);
            }
        }

        private void LoadSetLocationCount (MyLogRequestVm logReqDetails,
            IQueryable<AttendanceAoHistory> dbAttendanceHistory) {
            dbAttendanceHistory = dbAttendanceHistory.Where (ah => !string.IsNullOrEmpty (ah.AttendanceAoLastLocTrack));
            if (logReqDetails.Hours > 0) {
                dbAttendanceHistory =
                    dbAttendanceHistory.Where (ah => ah.AttendanceAoLastLocDate.HasValue &&
                        ah.AttendanceAoLastLocDate >=
                        DateTime.Now.AddHours (-logReqDetails.Hours ?? 0) &&
                        ah.AttendanceAoLastLocDate <= DateTime.Now);
            }
            if (logReqDetails.FromDate.HasValue) {
                dbAttendanceHistory = dbAttendanceHistory
                    .Where (ah => ah.AttendanceAoLastLocDate.HasValue &&
                        ah.AttendanceAoLastLocDate >= logReqDetails.FromDate &&
                        ah.AttendanceAoLastLocDate <= logReqDetails.ToDate);
            }
            if (logReqDetails.AttendanceId > 0) {
                dbAttendanceHistory = dbAttendanceHistory.Where (ah => ah.AttendanceAoId == logReqDetails.AttendanceId);
            }
            if (logReqDetails.PersonnelId > 0) {
                dbAttendanceHistory =
                    dbAttendanceHistory.Where (ah => ah.AttendanceAoLastLocOfficerId == logReqDetails.PersonnelId);
            }
            if (!string.IsNullOrEmpty (logReqDetails.Keyword)) {
                dbAttendanceHistory = dbAttendanceHistory.Where (
                    ah => string.Join (' ', ah.AttendanceAoLastLocTrack ?? string.Empty,
                        ah.AttendanceAoLastLocDesc ?? string.Empty).Contains (logReqDetails.Keyword));
            }
            List<CellLogDetailsVm> lstCellLogDetails = dbAttendanceHistory.Where (ah => logReqDetails.IsLogSearch.IsSetLocation)
                .Select (ah => new CellLogDetailsVm {
                    CellLogId = ah.AttendanceAoHistoryId,
                        Type = ViewerType.SETLOCATION,
                        CellDate = ah.AttendanceAoLastLocDate,
                        LastLocDesc = ah.AttendanceAoLastLocDesc,
                        LastLocTrack = ah.AttendanceAoLastLocTrack,
                }).OrderByDescending (de => de.CellDate).Take (logReqDetails.IsMyLog ? 100 : dbAttendanceHistory.Count ()).ToList ();
            MyLogCount.GetCount.SetLocationCount = lstCellLogDetails.Count ();
            if (logReqDetails.IsLogSearch.IsSetLocation) {
                MyLogCount.CellLogDetailsLst.AddRange (lstCellLogDetails);
            }
        }

        private void LoadOfficerLogCount (MyLogRequestVm mylogDetails,
            IQueryable<AttendanceAoHistory> dbAttendanceHistory) {
            dbAttendanceHistory = dbAttendanceHistory.Where (ah => !string.IsNullOrEmpty (ah.AttendanceAoOfficerLog));
            if (mylogDetails.Hours > 0) {
                dbAttendanceHistory = dbAttendanceHistory.Where (ah => ah.AttendanceAoOfficerLogDate.HasValue &&
                    ah.AttendanceAoOfficerLogDate >=
                    DateTime.Now.AddHours (-mylogDetails.Hours ?? 0) &&
                    ah.AttendanceAoOfficerLogDate <= DateTime.Now);
            }
            if (mylogDetails.FromDate.HasValue) {
                dbAttendanceHistory = dbAttendanceHistory
                    .Where (ah => ah.AttendanceAoOfficerLogDate.HasValue && ah.AttendanceAoOfficerLogDate >=
                        mylogDetails.FromDate && ah.AttendanceAoOfficerLogDate <= mylogDetails.ToDate);
            }
            if (mylogDetails.AttendanceId > 0) {
                dbAttendanceHistory = dbAttendanceHistory.Where (ah => ah.AttendanceAoId == mylogDetails.AttendanceId);
            }
            if (!string.IsNullOrEmpty (mylogDetails.Keyword)) {
                dbAttendanceHistory =
                    dbAttendanceHistory.Where (ah => ah.AttendanceAoOfficerLog.Contains (mylogDetails.Keyword));
            }
            if (mylogDetails.PersonnelId > 0) {
                dbAttendanceHistory =
                    dbAttendanceHistory.Where (ah => ah.AttendanceAoOfficerLogOfficerId == mylogDetails.PersonnelId);
            }
            List<CellLogDetailsVm> lstCellLogDetails = dbAttendanceHistory.Where (ah => mylogDetails.IsLogSearch.IsLog)
                .Select (ah => new CellLogDetailsVm {
                    CellLogId = ah.AttendanceAoHistoryId,
                        Type = ViewerType.OFFICERLOG,
                        CellDate = ah.AttendanceAoOfficerLogDate,
                        Comments = ah.AttendanceAoOfficerLog,
                }).OrderByDescending (de => de.CellDate).Take (mylogDetails.IsMyLog ? 100 : dbAttendanceHistory.Count ()).ToList ();
            MyLogCount.GetCount.OfficerLogCount = lstCellLogDetails.Count ();
            if (mylogDetails.IsLogSearch.IsLog) {
                MyLogCount.CellLogDetailsLst.AddRange (lstCellLogDetails);
            }
        }

        private void LoadCellCount (MyLogRequestVm cellLogDetails) {
            IQueryable<CellLogDetailsVm> lstCellLogDetails = _context.CellLog.Where (cl =>
                    (!cl.CellLogHeadcountId.HasValue || cl.CellLogHeadcountId == 0) &&
                    ((cellLogDetails.Hours == 0 || !cellLogDetails.Hours.HasValue) || cl.CellLogDate.HasValue &&
                        cl.CellLogDate >= DateTime.Now.AddHours (-cellLogDetails.Hours.Value) &&
                        cl.CellLogDate <= DateTime.Now) &&
                    (!cellLogDetails.FromDate.HasValue || cl.CellLogDate.HasValue &&
                        cl.CellLogDate >= cellLogDetails.FromDate &&
                        cl.CellLogDate <= cellLogDetails.ToDate) &&
                    (cellLogDetails.PersonnelId == 0 || cl.CellLogOfficerId == cellLogDetails.PersonnelId) &&
                    (cellLogDetails.DeleteFlag == 0 || cl.CellLogDeleteFlag == cellLogDetails.DeleteFlag) &&
                    (cellLogDetails.FacilityId == 0 || cl.FacilityId == cellLogDetails.FacilityId) &&
                    (string.IsNullOrEmpty (cellLogDetails.Keyword) ||
                        cl.CellLogComments.Contains (cellLogDetails.Keyword)) &&
                    (string.IsNullOrEmpty (cellLogDetails.Location) ||
                        cl.HousingUnitLocation == cellLogDetails.Location) &&
                    (string.IsNullOrEmpty (cellLogDetails.Number) ||
                        cl.HousingUnitNumber == cellLogDetails.Number) &&
                    (string.IsNullOrEmpty (cellLogDetails.GroupString) ||
                        !string.IsNullOrEmpty (cl.HousingUnitLocation) &&
                        !string.IsNullOrEmpty (cl.HousingUnitNumber) &&
                        cellLogDetails.GroupString.Contains (cl.HousingUnitLocation + ' ' + cl.HousingUnitNumber))
                )
                .Select (s => new CellLogDetailsVm {
                    CellLogId = s.CellLogId,
                        Type = ViewerType.CELL,
                        UpdateDate = s.UpdateDate,
                        CellDate = s.CreateDate,
                        HousingUnitLocation = s.HousingUnitLocation,
                        HousingUnitNumber = s.HousingUnitNumber,
                        NoteType = s.CellLogNoteType,
                        Comments = s.CellLogComments,
                        DeleteFlag = s.CellLogDeleteFlag
                }).OrderByDescending (de => de.CellDate);
            List<CellLogDetailsVm> lstCellLogDetail = cellLogDetails.IsMyLog ? lstCellLogDetails.Take (100).ToList () : lstCellLogDetails.ToList ();
            MyLogCount.GetCount.CellLogCount = lstCellLogDetail.Count;
            if (cellLogDetails.IsLogSearch.Iscelllog) {
                MyLogCount.CellLogDetailsLst.AddRange (lstCellLogDetail);
            }
        }

        private void LoadHeadCount (MyLogRequestVm headCountReq) {
            IQueryable<CellLogDetailsVm> LogHeadCountDetailsLst = _context.CellLog.Where (cl =>
                    cl.CellLogHeadcountId > 0 &&
                    ((headCountReq.Hours == 0 || !headCountReq.Hours.HasValue) || cl.CellLogDate.HasValue &&
                        cl.CellLogDate >= DateTime.Now.AddHours (-headCountReq.Hours.Value) &&
                        cl.CellLogDate <= DateTime.Now) &&
                    (!headCountReq.FromDate.HasValue || cl.CellLogDate.HasValue &&
                        cl.CellLogDate >= headCountReq.FromDate &&
                        cl.CellLogDate <= headCountReq.ToDate) &&
                    (headCountReq.PersonnelId == 0 || cl.CellLogOfficerId == headCountReq.PersonnelId) &&
                    (headCountReq.DeleteFlag == 0 || cl.CellLogDeleteFlag == headCountReq.DeleteFlag) &&
                    (headCountReq.FacilityId == 0 || cl.FacilityId == headCountReq.FacilityId) &&
                    (string.IsNullOrEmpty (headCountReq.Keyword) ||
                        cl.CellLogComments.Contains (headCountReq.Keyword)) &&
                    (string.IsNullOrEmpty (headCountReq.Location) ||
                        cl.HousingUnitLocation == headCountReq.Location) &&
                    (string.IsNullOrEmpty (headCountReq.Number) ||
                        cl.HousingUnitNumber == headCountReq.Number) &&
                    (string.IsNullOrEmpty (headCountReq.GroupString) ||
                        !string.IsNullOrEmpty (cl.HousingUnitLocation) &&
                        !string.IsNullOrEmpty (cl.HousingUnitNumber) &&
                        headCountReq.GroupString.Contains (cl.HousingUnitLocation + ' ' + cl.HousingUnitNumber))
                )
                .Select (s => new CellLogDetailsVm {
                    CellLogId = s.CellLogId,
                        Type = ViewerType.HEADCOUNT,
                        CellDate = s.UpdateDate,
                        HousingUnitLocation = s.HousingUnitLocation,
                        HousingUnitNumber = s.HousingUnitNumber,
                        CellLogCount = s.CellLogCount,
                        CellLogLocation = s.CellLogLocation,
                        NoteType = s.CellLogNoteType,
                        Comments = s.CellLogComments,
                        DeleteFlag = s.CellLogDeleteFlag,
                        ClearedBy = s.CellLogHeadcount.ClearedBy
                }).OrderByDescending (de => de.CellDate);
            List<CellLogDetailsVm> LogHeadCountDetailsLsts = headCountReq.IsMyLog? LogHeadCountDetailsLst.Take (100).ToList ():
                LogHeadCountDetailsLst.ToList ();
            MyLogCount.GetCount.HeadCount = LogHeadCountDetailsLsts.Count;
            if (headCountReq.IsLogSearch.IsHeadCount) {
                MyLogCount.CellLogDetailsLst.AddRange (LogHeadCountDetailsLsts);
            }
        }

        private void LoadSaftyCheckCount (MyLogRequestVm saftyCheckReq) {
            IQueryable<CellLogDetailsVm> lstCellLog = _context.SafetyCheck.Where (w =>
                    w.FacilityId == saftyCheckReq.FacilityId && w.CreateBy == saftyCheckReq.PersonnelId &&
                    ((saftyCheckReq.Hours == 0 || !saftyCheckReq.Hours.HasValue) || (w.CreateDate.HasValue &&
                        w.CreateDate >= DateTime.Now.AddHours (-saftyCheckReq.Hours.Value) &&
                        w.CreateDate <= DateTime.Now)) &&
                    (!saftyCheckReq.FromDate.HasValue || !saftyCheckReq.ToDate.HasValue ||
                        w.CreateDate.HasValue && w.CreateDate >= saftyCheckReq.FromDate &&
                        w.CreateDate <= saftyCheckReq.ToDate) &&
                    (string.IsNullOrEmpty (saftyCheckReq.Location) ||
                        w.HousingUnitLocation == saftyCheckReq.Location) &&
                    (string.IsNullOrEmpty (saftyCheckReq.Number) ||
                        w.HousingUnitNumber == saftyCheckReq.Number) &&
                    (string.IsNullOrEmpty (saftyCheckReq.GroupString) ||
                        !string.IsNullOrEmpty (w.HousingUnitLocation) &&
                        !string.IsNullOrEmpty (w.HousingUnitNumber) &&
                        saftyCheckReq.GroupString.Contains (w.HousingUnitLocation + ' ' + w.HousingUnitNumber)) &&
                    !w.LocationId.HasValue)
                .Select (s => new CellLogDetailsVm {
                    CellLogId = s.SafetyCheckId,
                        Type = ViewerType.SAFETYCHECK,
                        CellDate = s.CreateDate,
                        HousingUnitLocation = s.HousingUnitLocation,
                        HousingUnitNumber = s.HousingUnitNumber,
                        CellLogLocation = s.Location,
                        NoteType = s.SafetyCheckNote,
                        DeleteFlag = s.DeleteFlag
                }).Distinct ();
            List<CellLogDetailsVm> lstCellLogs = saftyCheckReq.IsMyLog ? lstCellLog.Take (100).ToList () : lstCellLog.ToList ();
            MyLogCount.GetCount.SafetyCheckCount = lstCellLogs.Count;
            if (saftyCheckReq.IsLogSearch.IsSafetyCheck) {
                MyLogCount.CellLogDetailsLst.AddRange (lstCellLogs);
            }
        }

        private void LoadHousingInCount (MyLogRequestVm myLogReq) {
            IQueryable<CellLogDetailsVm> lstHousingInMoveHistory = _context.HousingUnitMoveHistory.Where (w =>
                    ((myLogReq.Hours == 0 || !myLogReq.Hours.HasValue) || w.MoveDate.HasValue &&
                        w.MoveDate >= DateTime.Now.AddHours (-myLogReq.Hours.Value) &&
                        w.MoveDate <= DateTime.Now) && (!myLogReq.FromDate.HasValue ||
                        w.MoveDate >= myLogReq.FromDate && w.MoveDate <= myLogReq.ToDate) &&
                    (myLogReq.PersonnelId == 0 || w.MoveOfficerId == myLogReq.PersonnelId) &&
                    w.HousingUnitTo.HousingUnitId > 0 &&
                    myLogReq.FacilityId == w.HousingUnitTo.FacilityId &&
                    (string.IsNullOrEmpty (myLogReq.Location) ||
                        w.HousingUnitTo.HousingUnitLocation == myLogReq.Location) &&
                    (string.IsNullOrEmpty (myLogReq.Number) ||
                        w.HousingUnitTo.HousingUnitNumber == myLogReq.Number) &&
                    (string.IsNullOrEmpty (myLogReq.GroupString) ||
                        !string.IsNullOrEmpty (w.HousingUnitTo.HousingUnitLocation) &&
                        !string.IsNullOrEmpty (w.HousingUnitTo.HousingUnitNumber) &&
                        myLogReq.GroupString.Contains (w.HousingUnitTo.HousingUnitLocation + ' ' +
                            w.HousingUnitTo.HousingUnitNumber)))
                .Select (s => new CellLogDetailsVm {
                    CellLogId = s.HousingUnitToId ?? 0,
                        Type = ViewerType.HOUSINGIN,
                        CellDate = s.MoveDate,
                        HousingUnitLocation = s.HousingUnitTo.HousingUnitLocation,
                        HousingUnitNumber = s.HousingUnitTo.HousingUnitNumber,
                        HousingUnitBedNumber = s.HousingUnitTo.HousingUnitBedNumber,
                        HousingUnitBedLocation = s.HousingUnitTo.HousingUnitBedLocation,
                        InmateId = s.Inmate.InmateId,
                        PersonId = s.Inmate.Person.PersonId,
                        InmateNumber = s.Inmate.InmateNumber,
                        LastName = s.Inmate.Person.PersonLastName,
                        FirstName = s.Inmate.Person.PersonFirstName,
                        MiddleName = s.Inmate.Person.PersonMiddleName
                }).OrderByDescending (de => de.CellDate);
            List<CellLogDetailsVm> lstHousingInMoveHistorys = myLogReq.IsMyLog ? lstHousingInMoveHistory.Take (100).ToList () :
                lstHousingInMoveHistory.ToList ();
            MyLogCount.GetCount.HousingInCount = lstHousingInMoveHistorys.Count;
            if (myLogReq.IsLogSearch.IsHousingOut) {
                MyLogCount.CellLogDetailsLst.AddRange (lstHousingInMoveHistorys);
            }

        }

        private void LoadHousingOutCount (MyLogRequestVm housingOutReq) {
            IQueryable<CellLogDetailsVm> lstHousingOutMoveHistory = _context.HousingUnitMoveHistory.Where (w =>
                    w.MoveDateThru.HasValue && ((housingOutReq.Hours == 0 || !housingOutReq.Hours.HasValue) ||
                        w.MoveDateThru >= DateTime.Now.AddHours (-housingOutReq.Hours.Value) &&
                        w.MoveDateThru <= DateTime.Now) && (!housingOutReq.FromDate.HasValue ||
                        w.MoveDateThru >= housingOutReq.FromDate &&
                        w.MoveDateThru <= housingOutReq.ToDate) &&
                    (housingOutReq.PersonnelId == 0 || w.MoveThruBy == housingOutReq.PersonnelId) &&
                    housingOutReq.FacilityId == w.HousingUnitTo.FacilityId &&
                    (string.IsNullOrEmpty (housingOutReq.Location) ||
                        w.HousingUnitTo.HousingUnitLocation == housingOutReq.Location) &&
                    (string.IsNullOrEmpty (housingOutReq.Number) ||
                        w.HousingUnitTo.HousingUnitNumber == housingOutReq.Number) &&
                    (string.IsNullOrEmpty (housingOutReq.GroupString) ||
                        !string.IsNullOrEmpty (w.HousingUnitTo.HousingUnitLocation) &&
                        !string.IsNullOrEmpty (w.HousingUnitTo.HousingUnitNumber) &&
                        housingOutReq.GroupString.Contains (w.HousingUnitTo.HousingUnitLocation + ' ' +
                            w.HousingUnitTo.HousingUnitNumber)))
                .Select (s => new CellLogDetailsVm {
                    CellLogId = s.HousingUnitToId ?? 0,
                        Type = ViewerType.HOUSINGOUT,
                        CellDate = s.MoveDate,
                        Location = s.Inmate.InmateCurrentTrack,
                        HousingUnitLocation = s.HousingUnitTo.HousingUnitLocation,
                        HousingUnitNumber = s.HousingUnitTo.HousingUnitNumber,
                        HousingUnitBedNumber = s.HousingUnitTo.HousingUnitBedNumber,
                        HousingUnitBedLocation = s.HousingUnitTo.HousingUnitBedLocation,
                        InmateId = s.Inmate.InmateId,
                        PersonId = s.Inmate.Person.PersonId,
                        InmateNumber = s.Inmate.InmateNumber,
                        LastName = s.Inmate.Person.PersonLastName,
                        FirstName = s.Inmate.Person.PersonFirstName,
                        MiddleName = s.Inmate.Person.PersonMiddleName
                }).OrderByDescending (de => de.CellDate);
            List<CellLogDetailsVm> lstHousingOutMoveHistorys = housingOutReq.IsMyLog ? lstHousingOutMoveHistory.Take (100).ToList () :
                lstHousingOutMoveHistory.ToList ();
            MyLogCount.GetCount.HousingOutCount = lstHousingOutMoveHistorys.Count;

            if (housingOutReq.IsLogSearch.IsHousingOut) {
                MyLogCount.CellLogDetailsLst.AddRange (lstHousingOutMoveHistorys);
            }
        }

        public List<CellLogDetailsVm> GetInmateTrackInOut (IQueryable<InmateTrak> inmateTraks, MyLogRequestVm trackingReq, bool isFromMylog, string flag) {
            List<CellLogDetailsVm> dbInmateTrack;
            // Get Last 100 MyLog Details
            if (isFromMylog) {
                dbInmateTrack = inmateTraks.OrderByDescending (a => a.InmateTrakId)
                    .Select (tk => new CellLogDetailsVm {
                        CellLogId = tk.InmateTrakId,
                            Type = flag,
                            CellDate = flag == ViewerType.TRACKINGIN ? tk.InmateTrakDateIn : tk.InmateTrakDateOut,
                            Location = tk.Inmate.InmateCurrentTrack,
                            TrakLocation = tk.InmateTrakLocation,
                            InmateRefused = tk.InmateRefused,
                            RefusedReason = tk.InmateRefusedReason,
                            NoteType = tk.InmateTrakNote,
                            InmateId = tk.InmateId,
                            PersonId = tk.Inmate.Person.PersonId,
                            InmateNumber = tk.Inmate.InmateNumber,
                            LastName = tk.Inmate.Person.PersonLastName,
                            FirstName = tk.Inmate.Person.PersonFirstName,
                            MiddleName = tk.Inmate.Person.PersonMiddleName
                    }).OrderByDescending (de => de.CellDate).Take (100).ToList ();
            }
            // Get All MyLog Details
            else {
                dbInmateTrack = inmateTraks.Where (tk => flag == ViewerType.TRACKINGIN ?
                        (trackingReq.Hours == 0 || !trackingReq.Hours.HasValue ||
                            tk.InmateTrakDateIn >= DateTime.Now.AddHours (-trackingReq.Hours.Value)) &&
                        (!trackingReq.FromDate.HasValue || (tk.InmateTrakDateIn >= trackingReq.FromDate &&
                            tk.InmateTrakDateIn <= trackingReq.ToDate)) : (trackingReq.Hours <= 0 || !trackingReq.Hours.HasValue ||
                            tk.InmateTrakDateOut >= DateTime.Now.AddHours (-trackingReq.Hours.Value)) &&
                        (!trackingReq.FromDate.HasValue || (tk.InmateTrakDateOut >= trackingReq.FromDate &&
                            tk.InmateTrakDateOut <= trackingReq.ToDate)))
                    .Select (tk => new CellLogDetailsVm {
                        CellLogId = tk.InmateTrakId,
                            Type = flag,
                            CellDate = flag == ViewerType.TRACKINGIN ? tk.InmateTrakDateIn : tk.InmateTrakDateOut,
                            Location = tk.Inmate.InmateCurrentTrack,
                            TrakLocation = tk.InmateTrakLocation,
                            InmateRefused = tk.InmateRefused,
                            RefusedReason = tk.InmateRefusedReason,
                            NoteType = tk.InmateTrakNote,
                            InmateId = tk.InmateId,
                            PersonId = tk.Inmate.Person.PersonId,
                            InmateNumber = tk.Inmate.InmateNumber,
                            LastName = tk.Inmate.Person.PersonLastName,
                            FirstName = tk.Inmate.Person.PersonFirstName,
                            MiddleName = tk.Inmate.Person.PersonMiddleName
                    }).OrderByDescending (de => de.CellDate).ToList ();
            }

            return dbInmateTrack;
        }

        private void LoadTrackingInOutCount (MyLogRequestVm trackingReq, string flag) {
            bool isFromMylog = (trackingReq.Hours == 0 || !trackingReq.Hours.HasValue) &&
                !trackingReq.FromDate.HasValue;
            List<CellLogDetailsVm> dbInmateTrack;
            // Get TRACKINGIN MyLog Details
            if (flag == ViewerType.TRACKINGIN) {
                IQueryable<InmateTrak> inmateTraks = _context.InmateTrak
                    .Where (tk =>
                        tk.InmateTrakDateIn.HasValue &&
                        (trackingReq.PersonnelId == 0 || tk.InPersonnelId == trackingReq.PersonnelId) &&
                        tk.Inmate.FacilityId == trackingReq.FacilityId &&
                        (string.IsNullOrEmpty (trackingReq.Location) ||
                            tk.InmateTrakFromLocation.HousingUnitLocation == trackingReq.Location) &&
                        (string.IsNullOrEmpty (trackingReq.Number) ||
                            tk.InmateTrakFromLocation.HousingUnitNumber == trackingReq.Number) &&
                        (string.IsNullOrEmpty (trackingReq.GroupString) ||
                            (tk.InmateTrakFromLocation.HousingUnitLocation + " " +
                                tk.InmateTrakFromLocation.HousingUnitNumber).Contains (trackingReq.GroupString)));
                if (isFromMylog) {
                    dbInmateTrack = GetInmateTrackInOut (inmateTraks, trackingReq, isFromMylog, flag);
                } else {
                    dbInmateTrack = GetInmateTrackInOut (inmateTraks, trackingReq, isFromMylog, flag);
                }
            }
            // Get TRACKINGOUT MyLog Details
            else {
                IQueryable<InmateTrak> inmateTraks = _context.InmateTrak
                    .Where (tk =>
                        tk.InmateTrakDateOut.HasValue &&
                        (trackingReq.PersonnelId == 0 || tk.OutPersonnelId == trackingReq.PersonnelId) &&
                        tk.Inmate.FacilityId == trackingReq.FacilityId &&
                        (string.IsNullOrEmpty (trackingReq.Location) ||
                            tk.InmateTrakFromLocation.HousingUnitLocation == trackingReq.Location) &&
                        (string.IsNullOrEmpty (trackingReq.Number) ||
                            tk.InmateTrakFromLocation.HousingUnitNumber == trackingReq.Number) &&
                        (string.IsNullOrEmpty (trackingReq.GroupString) ||
                            (tk.InmateTrakFromLocation.HousingUnitLocation + " " +
                                tk.InmateTrakFromLocation.HousingUnitNumber).Contains (trackingReq.GroupString)));
                if (isFromMylog) {
                    dbInmateTrack = GetInmateTrackInOut (inmateTraks, trackingReq, isFromMylog, flag);
                } else {
                    dbInmateTrack = GetInmateTrackInOut (inmateTraks, trackingReq, isFromMylog, flag);
                }
            }
            if (flag == ViewerType.TRACKINGIN) {
                MyLogCount.GetCount.TrackingInCount = dbInmateTrack.Count;
            } else if (flag == ViewerType.TRACKINGOUT) {
                MyLogCount.GetCount.TrackingOutCount = dbInmateTrack.Count;
            }

            if ((flag == ViewerType.TRACKINGIN && trackingReq.IsLogSearch.IsTrackingIn) ||
                (flag == ViewerType.TRACKINGOUT && trackingReq.IsLogSearch.IsTrackingOut)) {
                MyLogCount.CellLogDetailsLst.AddRange (dbInmateTrack);
            }
        }

        public IQueryable<Privileges> GetPrivilegesDetails (MyLogRequestVm privileges) {
            IQueryable<Privileges> dbPrivileges = _context.Privileges;

            if (!string.IsNullOrEmpty (privileges.Location)) {
                dbPrivileges = dbPrivileges.Where (p => p.HousingUnitLocation == privileges.Location);
            }
            if (!string.IsNullOrEmpty (privileges.Number)) {
                dbPrivileges = dbPrivileges.Where (p => p.HousingUnitNumber == privileges.Number);
            }
            if (!string.IsNullOrEmpty (privileges.GroupString)) {
                dbPrivileges = dbPrivileges.Where (
                    p => !string.IsNullOrEmpty (p.HousingUnitLocation) && !string.IsNullOrEmpty (p.HousingUnitNumber) &&
                    privileges.GroupString.Contains (p.HousingUnitLocation + ' ' + p.HousingUnitNumber));
            }
            return dbPrivileges;
        }

        public IQueryable<FloorNotes> GetFloorNotesDetails (MyLogRequestVm floorReq) {
            IQueryable<FloorNotes> lstFloorNotes = _context.FloorNotes.Where (fn =>
                (floorReq.PersonnelId == 0 || fn.FloorNoteOfficerId == floorReq.PersonnelId) &&
                (!floorReq.FromDate.HasValue ||
                    fn.FloorNoteDate.HasValue && fn.FloorNoteDate >= floorReq.FromDate &&
                    fn.FloorNoteDate <= floorReq.ToDate) &&
                ((floorReq.Hours == 0 || !floorReq.Hours.HasValue) || fn.FloorNoteDate.HasValue &&
                    fn.FloorNoteDate >= DateTime.Now.AddHours (-floorReq.Hours.Value) &&
                    fn.FloorNoteDate <= DateTime.Now) &&
                (string.IsNullOrEmpty (floorReq.Keyword) ||
                    fn.FloorNoteNarrative.Contains (floorReq.Keyword)));

            return lstFloorNotes;
        }

        private void LoadInmateNoteCount (MyLogRequestVm inmateNoteReq) {
            IQueryable<CellLogDetailsVm> lstFloorNoteXref = _context.FloorNoteXref.Where (fnx =>
                    fnx.FloorNoteId == fnx.FloorNote.FloorNoteId &&
                    (inmateNoteReq.FacilityId == 0 || fnx.Inmate.FacilityId == inmateNoteReq.FacilityId) &&
                    (string.IsNullOrEmpty (inmateNoteReq.Location) ||
                        fnx.Inmate.HousingUnit.HousingUnitLocation == inmateNoteReq.Location) &&
                    (string.IsNullOrEmpty (inmateNoteReq.Number) ||
                        fnx.Inmate.HousingUnit.HousingUnitNumber == inmateNoteReq.Number) &&
                    (string.IsNullOrEmpty (inmateNoteReq.GroupString) ||
                        !string.IsNullOrEmpty (fnx.Inmate.HousingUnit.HousingUnitLocation) &&
                        !string.IsNullOrEmpty (fnx.Inmate.HousingUnit.HousingUnitNumber) &&
                        inmateNoteReq.GroupString.Contains (fnx.Inmate.HousingUnit.HousingUnitLocation + ' ' +
                            fnx.Inmate.HousingUnit.HousingUnitNumber)) &&
                    (inmateNoteReq.PersonnelId == 0 || fnx.FloorNote.FloorNoteOfficerId == inmateNoteReq.PersonnelId) &&
                    (!inmateNoteReq.FromDate.HasValue || fnx.FloorNote.FloorNoteDate.HasValue &&
                        fnx.FloorNote.FloorNoteDate >= inmateNoteReq.FromDate &&
                        fnx.FloorNote.FloorNoteDate <= inmateNoteReq.ToDate) &&
                    ((inmateNoteReq.Hours == 0 || !inmateNoteReq.Hours.HasValue) || fnx.FloorNote.FloorNoteDate.HasValue &&
                        fnx.FloorNote.FloorNoteDate >= DateTime.Now.AddHours (-inmateNoteReq.Hours.Value) &&
                        fnx.FloorNote.FloorNoteDate <= DateTime.Now) && (string.IsNullOrEmpty (inmateNoteReq.Keyword) ||
                        fnx.FloorNote.FloorNoteNarrative.Contains (inmateNoteReq.Keyword)))
                .Select (s => new CellLogDetailsVm {
                    CellLogId = s.FloorNoteId,
                        Type = ViewerType.INMATENOTE,
                        CellDate = s.FloorNote.FloorNoteDate,
                        Location = s.Inmate.InmateCurrentTrack,
                        FloorNoteLocation = s.FloorNote.FloorNoteLocation,
                        LastName = s.Inmate.Person.PersonLastName,
                        FirstName = s.Inmate.Person.PersonFirstName,
                        MiddleName = s.Inmate.Person.PersonMiddleName,
                        NoteType = s.FloorNote.FloorNoteType,
                        Comments = s.FloorNote.FloorNoteNarrative,
                        DeleteFlag = s.FloorNote.FloorDeleteFlag,
                        InmateId = s.InmateId,
                        InmateNumber = s.Inmate.InmateNumber,
                        PersonId = s.Inmate.Person.PersonId
                }).OrderByDescending (de => de.CellDate);
            List<CellLogDetailsVm> lstFloorNoteXrefs = inmateNoteReq.IsMyLog ? lstFloorNoteXref.Take (100).ToList () :
                lstFloorNoteXref.ToList ();
            MyLogCount.GetCount.InmateNoteCount = lstFloorNoteXrefs.Count;

            if (inmateNoteReq.IsLogSearch.IsNote) {
                MyLogCount.CellLogDetailsLst.AddRange (lstFloorNoteXrefs);
            }
        }

        private void LoadLocationNoteCount (MyLogRequestVm locationNote) {
            string[] privilegesDescLst = _context.Privileges
                .Where (p => !(locationNote.FacilityId > 0) ||
                    (p.FacilityId == locationNote.FacilityId || !p.FacilityId.HasValue))
                .Select (s => s.PrivilegeDescription).ToArray ();

            List<FloorNotes> dbFloorNote = GetFloorNotesDetails (locationNote).Where (
                fn => !fn.FloorNoteXref.Any () && !string.IsNullOrEmpty (fn.FloorNoteLocation) &&
                (!string.IsNullOrEmpty (fn.FloorNoteNarrative) ||
                    !string.IsNullOrEmpty (fn.FloorNoteType))).ToList ();

            List<CellLogDetailsVm> lstCellLog =
                dbFloorNote.Where (p => privilegesDescLst.Contains (p.FloorNoteLocation))
                .Select (f => new CellLogDetailsVm {
                    CellLogId = f.FloorNoteId,
                        Type = ViewerType.LOCATIONNOTE,
                        CellDate = f.FloorNoteDate,
                        Location = f.FloorNoteLocation,
                        NoteType = f.FloorNoteType,
                        Comments = f.FloorNoteNarrative,
                        DeleteFlag = f.FloorDeleteFlag,
                }).OrderByDescending (de => de.CellDate).Take (locationNote.IsMyLog ? 100 : dbFloorNote.Count).ToList ();

            MyLogCount.GetCount.LocationNoteCount = lstCellLog.Count;

            if (locationNote.IsLogSearch.IsLocationNote) {
                MyLogCount.CellLogDetailsLst.AddRange (lstCellLog);
            }
        }

        private void LoadGeneralNoteCount (MyLogRequestVm general) {
            List<FloorNotes> lstFloorNotes = GetFloorNotesDetails (general).Where (fn =>
                !fn.FloorNoteXref.Any () && string.IsNullOrEmpty (fn.FloorNoteLocation) &&
                (!string.IsNullOrEmpty (fn.FloorNoteNarrative) || !string.IsNullOrEmpty (fn.FloorNoteType))).ToList ();

            List<CellLogDetailsVm> lstCellLog = lstFloorNotes.Where (g =>
                    general.IsLogSearch.IsGeneralNote)
                .Select (f => new CellLogDetailsVm {
                    CellLogId = f.FloorNoteId,
                        Type = ViewerType.GENERALNOTE,
                        CellDate = f.FloorNoteDate,
                        FloorNoteLocation = f.FloorNoteLocation,
                        NoteType = f.FloorNoteType,
                        Comments = f.FloorNoteNarrative,
                        DeleteFlag = f.FloorDeleteFlag,
                }).OrderByDescending (de => de.CellDate).Take (general.IsMyLog ? 100 : lstFloorNotes.Count).ToList ();
            MyLogCount.GetCount.GeneralNoteCount = lstCellLog.Count ();
            if (general.IsLogSearch.IsGeneralNote) {
                MyLogCount.CellLogDetailsLst.AddRange (lstCellLog);
            }
        }

        public async Task<int> SetHousingDetails (MyLogRequestVm setHousingReq) {
            setHousingReq.PersonnelId = _personnelId;
            setHousingReq.AttendanceId = GetAttendanceId (setHousingReq.PersonnelId);

            if (setHousingReq.FacilityId > 0 && setHousingReq.Location != null && setHousingReq.Number != null) {
                setHousingReq.HousingUnitListId =
                    _context.HousingUnitList.SingleOrDefault (
                        h => h.FacilityId == setHousingReq.FacilityId &&
                        h.HousingUnitLocation == setHousingReq.Location &&
                        h.HousingUnitNumber == setHousingReq.Number)?.HousingUnitListId ?? 0;
            }

            if (setHousingReq.AttendanceId > 0) {
                await ClearAttendanceDetails (setHousingReq);

                //To update attendance details based on CellLog type
                AttendanceAo dbAttendance =
                    _context.AttendanceAo.FirstOrDefault (a => a.AttendanceAoId == setHousingReq.AttendanceId);
                if (dbAttendance != null) {
                    if (setHousingReq.IsHousing) {
                        dbAttendance.AttendanceAoLastHousingFacilityId = setHousingReq.FacilityId;
                        dbAttendance.AttendanceAoLastHousingLocation = setHousingReq.Location;
                        dbAttendance.AttendanceAoLastHousingNumber = setHousingReq.Number;
                        dbAttendance.AttendanceAoLastHousingNote = setHousingReq.Note;
                        dbAttendance.AttendanceAoLastHousingDate = DateTime.Now;
                        dbAttendance.AttendanceAoLastHousingOfficerId = setHousingReq.PersonnelId;
                        dbAttendance.HousingUnitListId = setHousingReq.HousingUnitListId;

                        AddAttendanceHidtory (setHousingReq, false);
                    } else {
                        dbAttendance.AttendanceAoLastLocTrack = setHousingReq.LastLocTrack;
                        dbAttendance.AttendanceAoLastHousingFacilityId = setHousingReq.FacilityId;
                        dbAttendance.AttendanceAoLastLocDesc = setHousingReq.Note;
                        dbAttendance.AttendanceAoLastLocDate = DateTime.Now;
                        dbAttendance.AttendanceAoLastLocOfficerId = setHousingReq.PersonnelId;
                        dbAttendance.AttendanceAoLastLocTrackId = setHousingReq.PrivilegeId;

                        AddAttendanceHidtory (setHousingReq, true);
                    }
                }
            }
            return await _context.SaveChangesAsync ();
        }

        public async Task<int> ClearAttendanceDetails (MyLogRequestVm attendance) {
            attendance.PersonnelId = _personnelId;
            attendance.AttendanceId =
                _context.AttendanceAo.Where (a => a.PersonnelId == attendance.PersonnelId)
                .Select (i => i.AttendanceAoId).Max ();
            AttendanceAo dbAttendance =
                _context.AttendanceAo.FirstOrDefault (a => a.AttendanceAoId == attendance.AttendanceId);

            if (dbAttendance != null) {
                if (!attendance.IsHousing) {
                    dbAttendance.AttendanceAoLastHousingFacilityId = null;
                    dbAttendance.AttendanceAoLastHousingLocation = null;
                    dbAttendance.AttendanceAoLastHousingNumber = null;
                    dbAttendance.AttendanceAoLastHousingNote = null;
                    dbAttendance.AttendanceAoLastHousingDate = null;
                    dbAttendance.AttendanceAoLastHousingOfficerId = null;
                    dbAttendance.HousingUnitListId = null;
                    AddAttendanceHidtory (attendance, false);
                } else {
                    dbAttendance.AttendanceAoLastLocTrack = null;
                    dbAttendance.AttendanceAoLastHousingFacilityId = null;
                    dbAttendance.AttendanceAoLastLocDesc = null;
                    dbAttendance.AttendanceAoLastLocDate = null;
                    dbAttendance.AttendanceAoLastLocOfficerId = null;
                    dbAttendance.AttendanceAoLastLocTrackId = null;
                    AddAttendanceHidtory (attendance, true);
                }
            }
            return await _context.SaveChangesAsync ();
        }

        private void AddAttendanceHidtory (MyLogRequestVm log, bool isDeleted) {
            AttendanceAoHistory dbAttendanceHistory = new AttendanceAoHistory {
                AttendanceAoId = log.AttendanceId,
                AttendanceAoLastHousingFacilityId = log.FacilityId
            };
            if (isDeleted) {
                dbAttendanceHistory.AttendanceAoLastLocTrack = log.LastLocTrack;
                dbAttendanceHistory.AttendanceAoLastLocDesc = log.Note;
                dbAttendanceHistory.AttendanceAoLastLocDate = DateTime.Now;
                dbAttendanceHistory.AttendanceAoLastLocOfficerId = log.PersonnelId;
                dbAttendanceHistory.AttendanceAoHistoryLastLocTrackId = log.PrivilegeId;
            } else {
                dbAttendanceHistory.AttendanceAoLastHousingLocation = log.Location;
                dbAttendanceHistory.AttendanceAoLastHousingNumber = log.Number;
                dbAttendanceHistory.AttendanceAoLastHousingNote = log.Note;
                dbAttendanceHistory.AttendanceAoLastHousingDate = DateTime.Now;
                dbAttendanceHistory.AttendanceAoLastHousingOfficerId = log.PersonnelId;
                if (log.HousingUnitListId > 0) {
                    dbAttendanceHistory.HousingUnitListId = log.HousingUnitListId;
                }
            }
            _context.AttendanceAoHistory.Add (dbAttendanceHistory);
        }

        public MylogDetailsVm GetMyLogDetails (MyLogRequestVm logReqDetails) {
            logReqDetails.PersonnelId = _personnelId;
            logReqDetails.AttendanceId = GetAttendanceId (_personnelId);

            MylogDetailsVm myLog = new MylogDetailsVm ();
            if (logReqDetails.AttendanceId > 0) {
                myLog = (from a in _context.AttendanceAo where a.AttendanceAoId == logReqDetails.AttendanceId select new MylogDetailsVm {
                    ClockInEnter = a.ClockInEnter,
                        ClockOutEnter = a.ClockOutEnter,
                        Note = a.ClockInNote,
                        LastLocTrack = a.AttendanceAoLastLocTrack,
                        LastHousingLocation = a.AttendanceAoLastHousingLocation,
                        LastHousingNumber = a.AttendanceAoLastHousingNumber,
                        LastHousingNote = a.AttendanceAoLastHousingNote,
                        LastLocDesc = a.AttendanceAoLastLocDesc,
                        Status = a.AttendanceAoStatus,
                        StatusNote = a.AttendanceAoStatusNote
                }).SingleOrDefault ();
            }

            if (myLog != null) {
                // myLog.HousingLst = _facilityHousingService.GetHousing(logReqDetails.FacilityId ?? 0);
                myLog.HousingLst = _context.HousingUnit.Where (w => w.FacilityId == logReqDetails.FacilityId &&
                        (w.HousingUnitInactive == 0 || w.HousingUnitInactive == null))
                    .GroupBy (g => new {
                        g.HousingUnitLocation,
                            g.HousingUnitNumber
                    }).Select (s => new HousingDetail {
                        HousingUnitLocation = s.Key.HousingUnitLocation,
                            HousingUnitNumber = s.Key.HousingUnitNumber,
                            HousingUnitListId = s.First ().HousingUnitListId,
                            // FacilityAbbr = lstFacilityAbbr.SingleOrDefault(w => w.Key == facilityId).Value,
                    }).ToList ();
                myLog.LocationLst = _facilityPrivilegeService.GetLocationList (logReqDetails.FacilityId ?? 0);

                logReqDetails.IsMyLog = true;

                myLog.LstCelldetails =
                    GetMyLogDetailsCount (logReqDetails)
                    .CellLogDetailsLst?.OrderByDescending (de => de.CellDate).Take (100).ToList ();

                KeyValuePair<string, string> personnelDetails =
                    _context.Personnel.Where (pr => pr.PersonnelId == logReqDetails.PersonnelId)
                    .Select (de => new KeyValuePair<string, string> (de.PersonnelPosition, de.PersonnelDepartment))
                    .SingleOrDefault ();

                myLog.LstOfficerLog =
                    _context.OfficerLogLookup.Where (
                        ol => ol.Inactive == null &&
                        (string.IsNullOrEmpty (ol.FilterDepartment) && string.IsNullOrEmpty (ol.FilterPosition)) ||
                        (!string.IsNullOrEmpty (ol.FilterDepartment) &&
                            ol.FilterDepartment == personnelDetails.Value) ||
                        (!string.IsNullOrEmpty (ol.FilterPosition) &&
                            ol.FilterPosition == personnelDetails.Key)).Select (s => s.OfficerLog).ToList ();
            }
            return myLog;
        }

        public int GetAttendanceId (int? personnelId) {
            int[] attendanceAoIdLst =
                _context.AttendanceAo.Where (a => a.PersonnelId == personnelId)
                .Select (i => i.AttendanceAoId).ToArray ();
            return attendanceAoIdLst.Any () ? attendanceAoIdLst.Max () : 0;
            // Conditional operator used to handle 'Sequence contains no elements' exception.
        }

        public Task<int> SetCurrentStatus (MyLogRequestVm statusReq) {
            statusReq.PersonnelId = _personnelId;
            statusReq.AttendanceId = GetAttendanceId (statusReq.PersonnelId);

            if (statusReq.AttendanceId > 0) {
                AttendanceAo dbAttendance =
                    _context.AttendanceAo.SingleOrDefault (a => a.AttendanceAoId == statusReq.AttendanceId);
                if (dbAttendance != null) {
                    if (statusReq.IsClear) {
                        dbAttendance.AttendanceAoStatus = null;
                        dbAttendance.AttendanceAoStatusNote = null;
                        dbAttendance.AttendanceAoStatusDate = null;
                        dbAttendance.AttendanceAoStatusOfficerId = null;
                    } else {
                        dbAttendance.AttendanceAoStatus = statusReq.StatusId;
                        dbAttendance.AttendanceAoStatusNote = statusReq.Note;
                        dbAttendance.AttendanceAoStatusDate = DateTime.Now;
                        dbAttendance.AttendanceAoStatusOfficerId = statusReq.PersonnelId;
                    }
                }

                AttendanceAoHistory dbAttendanceHistory = new AttendanceAoHistory {
                    AttendanceAoId = statusReq.AttendanceId,
                    AttendanceAoStatus = statusReq.StatusId,
                    AttendanceAoStatusNote = statusReq.Note,
                    AttendanceAoStatusDate = DateTime.Now,
                    AttendanceAoStatusOfficerId = statusReq.PersonnelId,
                    AttendanceAoLastHousingFacilityId = statusReq.FacilityId

                };
                _context.AttendanceAoHistory.Add (dbAttendanceHistory);
            }

            return _context.SaveChangesAsync ();
        }

        public Task<int> AddLogDetails (MyLogRequestVm logDetails) {
            logDetails.PersonnelId = _personnelId;
            logDetails.AttendanceId = GetAttendanceId (logDetails.PersonnelId);

            if (logDetails.AttendanceId <= 0) {
                AttendanceAo dbAttandance = new AttendanceAo {
                PersonnelId = logDetails.PersonnelId ?? 0,
                FacilityId = logDetails.FacilityId ?? 0
                };
                _context.AttendanceAo.Add (dbAttandance);
                _context.SaveChanges ();

                logDetails.AttendanceId =
                    _context.AttendanceAo.OrderByDescending (a => a.AttendanceAoId)
                    .Select (s => s.AttendanceAoId).LastOrDefault ();
            }

            if (logDetails.AttendanceId > 0) {
                if (logDetails.AttendanceHistoryid > 0) {
                    AttendanceAoHistory dbHistory =
                        _context.AttendanceAoHistory.Single (
                            a => a.AttendanceAoHistoryId == logDetails.AttendanceHistoryid);
                    dbHistory.AttendanceAoOfficerLog = logDetails.Note;
                } else {
                    AttendanceAoHistory dbAttendanceHistory = new AttendanceAoHistory {
                        AttendanceAoId = logDetails.AttendanceId,
                        AttendanceAoOfficerLog = logDetails.Note,
                        AttendanceAoOfficerLogDate = DateTime.Now,
                        AttendanceAoOfficerLogOfficerId = logDetails.PersonnelId,
                        AttendanceAoLastHousingFacilityId = logDetails.FacilityId
                    };
                    _context.AttendanceAoHistory.Add (dbAttendanceHistory);
                }
            }
            return _context.SaveChangesAsync ();
        }

        public Task<int> DeleteUndoMyLog (CellLogDetailsVm log) {
            DateTime? deleteDate = DateTime.Now;
            if (log.Type == ViewerType.CELL || log.Type == ViewerType.HEADCOUNT) {
                CellLog dbCellLog = _context.CellLog.Single (c => c.CellLogId == log.CellLogId);
                if (dbCellLog != null) {
                    dbCellLog.DeletedBy = (dbCellLog.CellLogDeleteFlag == 1) ? new int?() : _personnelId;
                    dbCellLog.DeletedDate = dbCellLog.CellLogDeleteFlag == 1 ? null : deleteDate;
                    dbCellLog.CellLogDeleteFlag = dbCellLog.CellLogDeleteFlag == 1 ? 0 : 1;
                }

            } else if (log.Type == ViewerType.LOCATIONNOTE || log.Type == ViewerType.GENERALNOTE ||
                log.Type == ViewerType.INMATENOTE) {
                FloorNotes dbNotes = _context.FloorNotes.Single (f => f.FloorNoteId == log.CellLogId);
                if (dbNotes != null) {
                    dbNotes.DeletedBy = dbNotes.FloorDeleteFlag == 1 ? new int?() : _personnelId;
                    dbNotes.DeletedDate = dbNotes.FloorDeleteFlag == 1 ? null : deleteDate;
                    dbNotes.FloorDeleteFlag = dbNotes.FloorDeleteFlag == 1 ? 0 : 1;
                }
            } else if (log.Type == ViewerType.SAFETYCHECK) {
                SafetyCheck dbSafetyCheck = _context.SafetyCheck.Single (s => s.SafetyCheckId == log.CellLogId);
                dbSafetyCheck.DeleteBy = dbSafetyCheck.DeleteFlag == 1 ? new int?() : _personnelId;
                dbSafetyCheck.DeleteDate = dbSafetyCheck.DeleteFlag == 1 ? null : deleteDate;
                dbSafetyCheck.DeleteFlag = dbSafetyCheck.DeleteFlag == 1 ? 0 : 1;
            }
            return _context.SaveChangesAsync ();
        }

        public string DefaultClassifySettings () {
            LogSettingDetails lstClaimClassifySettings = new LogSettingDetails () {

                Iscelllog = false,
                IsClockin = false,
                IsClockOut = false,
                IsSetHousing = false,
                IsSetLocation = false,
                IsSetStatus = false,
                IsLog = false,
                IsHeadCount = false,
                IsSafetyCheck = false,
                IsHousingIn = false,
                IsHousingOut = false,
                IsTrackingOut = false,
                IsTrackingIn = false,
                IsNote = false,
                IsLocationNote = false,
                IsGeneralNote = false,

            };
            string claimClassify = JsonConvert.SerializeObject (lstClaimClassifySettings);
            return claimClassify;
        }

        public async Task<IdentityResult> UpdateUserSettings (LogSettingDetails objfilter) {

            string classifyDefault = DefaultClassifySettings ();
            string userId = _jwtDbContext.UserClaims.Where (w => w.ClaimType == "personnel_id" &&
                    w.ClaimValue == _personnelId.ToString ())
                .Select (s => s.UserId).FirstOrDefault ();

            AppUser appUser = _userManager.FindByIdAsync (userId).Result;

            IList<Claim> claims = await _userManager.GetClaimsAsync (appUser);

            Claim classifyClaim = claims.FirstOrDefault (f => f.Type == "Celllog_Settings");
            IdentityResult result;

            if (classifyClaim == null) {
                result = await _userManager.AddClaimAsync (appUser, new Claim ("Celllog_Settings", classifyDefault));
            } else {
                result = await _userManager.ReplaceClaimAsync (appUser, classifyClaim,
                    new Claim ("Celllog_Settings", JsonConvert.SerializeObject (objfilter)));
            }
            return result;
        }
    }

}