using GenerateTables.Models;
using Microsoft.AspNetCore.Http;
using ServerAPI.Utilities;
using ServerAPI.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServerAPI.Services
{
    public class LogCountService : ILogCountService
    {

        private readonly AAtims _context;
        private readonly int _personnelId;
        private readonly IPersonService _personService;
        private List<Lookup> _lookUp;

        public LogCountService(AAtims context, IHttpContextAccessor httpContextAccessor, IPersonService personService)
        {
            _context = context;
            _personnelId =
                Convert.ToInt32(httpContextAccessor.HttpContext.User.FindFirst(AuthDetailConstants.PERSONNELID)?.Value);
            _personService = personService;
        }

        public LogCountDetails GetLogCountDetails(LogCountVm value)
        {
            _lookUp = _context.Lookup.Where(w => w.LookupInactive != 1 && (w.LookupType == LookupConstants.DEPARTMENT
            || w.LookupType == LookupConstants.POSITION || w.LookupType == "ATTENDSTAT") && w.LookupDescription != null).ToList();

            List<AttendanceAoHistory> attendanceAOList = _context.AttendanceAoHistory.
            Where(w => w.AttendanceAoId == w.AttendanceAo.AttendanceAoId).ToList();
            LogCountDetails logCountDetails = new LogCountDetails();

            IQueryable<Personnel> lstPersonnel = _context.Personnel.Where(w => !string.IsNullOrEmpty(w.PersonnelDepartment));

            //To get logcount details
            logCountDetails.LogCountList = lstPersonnel.Where(w =>
              !w.PersonnelTerminationFlag)
            .Select(s => new LogCountVm
            {
                PersonnelDetails = new PersonnelVm
                {
                    PersonId = s.PersonId,
                    PersonFirstName = s.PersonNavigation.PersonFirstName,
                    PersonLastName = s.PersonNavigation.PersonLastName,
                    PersonMiddleName = s.PersonNavigation.PersonMiddleName,
                    OfficerBadgeNumber = s.OfficerBadgeNumber
                },
                PersonnelId = s.PersonnelId,
                PersonnelTerminationFlag = s.PersonnelTerminationFlag,
                PersonnelDepartment = s.PersonnelDepartment,
                PersonnelPosition = s.PersonnelPosition,
                ClockIn = GetClockInCount(s.PersonnelId, value, attendanceAOList),
                ClockOut = GetClockOutCount(s.PersonnelId, value, attendanceAOList),
                SetHousing = GetHousingCount(s.PersonnelId, value, attendanceAOList),
                SetLocation = GetLocationCount(s.PersonnelId, value, attendanceAOList),
                SetStatus = GetStatusCount(s.PersonnelId, value, attendanceAOList),
                Logs = GetLogCount(s.PersonnelId, value, attendanceAOList)
            }).OrderBy(o => o.PersonnelDetails.PersonFirstName).ThenBy(t => t.PersonnelDetails.PersonLastName).ToList();

            //To get department list
            logCountDetails.DepartmentList = logCountDetails.LogCountList
             .GroupBy(g => new
             {
                 g.PersonnelDepartment
             }).Select(s => new LookupVm
             {
                 LookupDescription = s.Key.PersonnelDepartment,
                 Count = s.Count()
             }).ToList();
            //To get position list
            logCountDetails.PositionList = logCountDetails.LogCountList.Where(w => !string.IsNullOrEmpty(w.PersonnelPosition))
            .GroupBy(g => new
            {
                g.PersonnelPosition
            }).Select(s => new LookupVm
            {
                LookupDescription = s.Key.PersonnelPosition,
                Count = s.Count()
            }).ToList();

            return logCountDetails;
        }
        private List<AttendanceAoVm> GetClockInCount(int personnelId, LogCountVm value, List<AttendanceAoHistory> attendanceAOList)
        {
            List<AttendanceAoVm> getClockInLogDetails = attendanceAOList.Where(w => w.AttendanceAoStatus == 1 && w.AttendanceAoStatusNote == "CLOCK IN"
                    && ((w.AttendanceAoStatusDate >= DateTime.Now.AddHours(-12)
                    && w.AttendanceAoStatusDate <= DateTime.Now.AddHours(-12)) ||
                      (w.AttendanceAoStatusDate >= value.FromDate
                    && w.AttendanceAoStatusDate <= value.ToDate)) && w.AttendanceAo.PersonnelId == personnelId)
                    .Select(s => new AttendanceAoVm
                    {
                        AttendanceAOHistoryId = s.AttendanceAoHistoryId,
                        AttendanceAoStatusDate = s.AttendanceAoStatusDate
                    }).ToList();

            return getClockInLogDetails;
        }
        private List<AttendanceAoVm> GetClockOutCount(int personnelId, LogCountVm value, List<AttendanceAoHistory> attendanceAOList)
        {
            List<AttendanceAoVm> getClockOutDetails = attendanceAOList.Where(w => w.AttendanceAoStatus == 2 && w.AttendanceAoStatusNote == "CLOCK OUT"
                       && ((w.AttendanceAoStatusDate >= DateTime.Now.AddHours(-12)
                       && w.AttendanceAoStatusDate <= DateTime.Now.AddHours(-12)) ||
                       (w.AttendanceAoStatusDate >= value.FromDate
                       && w.AttendanceAoStatusDate <= value.ToDate)) && w.AttendanceAo.PersonnelId == personnelId)
                       .Select(s => new AttendanceAoVm
                       {
                           AttendanceAOHistoryId = s.AttendanceAoHistoryId,
                           AttendanceAoStatusDate = s.AttendanceAoStatusDate
                       }).ToList();
            return getClockOutDetails;
        }

        private List<AttendanceAoVm> GetHousingCount(int personnelId, LogCountVm value, List<AttendanceAoHistory> attendanceAOList)
        {

            List<AttendanceAoVm> getSetHousingList = attendanceAOList.Where(w => w.AttendanceAoLastHousingLocation != null
                      && w.AttendanceAoLastHousingNumber != null && ((w.AttendanceAoLastHousingDate >= DateTime.Now.AddHours(-12)
                      && w.AttendanceAoLastHousingDate <= DateTime.Now.AddHours(-12)) ||
                    (w.AttendanceAoLastHousingDate >= value.FromDate && w.AttendanceAoLastHousingDate <= value.ToDate))
                    && w.AttendanceAo.PersonnelId == personnelId)
                    .Select(s => new AttendanceAoVm
                    {
                        AttendanceAOHistoryId = s.AttendanceAoHistoryId,
                        AttendanceAoLastHousingDate = s.AttendanceAoLastHousingDate,
                        AttendanceAoLastHousingLocation = s.AttendanceAoLastHousingLocation,
                        AttendanceAoLastHousingNumber = s.AttendanceAoLastHousingNumber,
                        AttendanceAoLastHousingNote = s.AttendanceAoLastHousingNote
                    }).ToList();

            return getSetHousingList;
        }
        private List<AttendanceAoVm> GetLocationCount(int personnelId, LogCountVm value, List<AttendanceAoHistory> attendanceAOList)
        {
            List<AttendanceAoVm> getSetLocationDetails = attendanceAOList.Where(w => w.AttendanceAoLastLocTrack != null
                      && ((w.AttendanceAoLastLocDate >= DateTime.Now.AddHours(-12)
                      && w.AttendanceAoLastLocDate <= DateTime.Now.AddHours(-12)) ||
                    (w.AttendanceAoLastLocDate >= value.FromDate && w.AttendanceAoLastLocDate <= value.ToDate))
                    && w.AttendanceAo.PersonnelId == personnelId)
                     .Select(s => new AttendanceAoVm
                     {
                         AttendanceAOHistoryId = s.AttendanceAoHistoryId,
                         AttendanceAoLastLocDate = s.AttendanceAoLastLocDate,
                         AttendanceAoLastLocTrack = s.AttendanceAoLastLocTrack,
                         AttendanceAoLastLocDesc = s.AttendanceAoLastLocDesc
                     }).ToList();
            return getSetLocationDetails;
        }
        private List<AttendanceAoVm> GetStatusCount(int personnelId, LogCountVm value, List<AttendanceAoHistory> attendanceAOList)
        {
            List<AttendanceAoVm> getStatusDetails = attendanceAOList.Where(w => w.AttendanceAoStatus > 0
                       && w.AttendanceAoStatusNote != "CLOCK IN"
                       && w.AttendanceAoStatusNote != "CLOCK OUT" &&
                       ((w.AttendanceAoStatusDate >= DateTime.Now.AddHours(-12)
                      && w.AttendanceAoStatusDate <= DateTime.Now.AddHours(-12)) ||
                    (w.AttendanceAoStatusDate >= value.FromDate && w.AttendanceAoStatusDate <= value.ToDate))
                    && w.AttendanceAo.PersonnelId == personnelId)
                     .Select(s => new AttendanceAoVm
                     {
                         AttendanceAOHistoryId = s.AttendanceAoHistoryId,
                         AttendanceAoStatusDate = s.AttendanceAoStatusDate,
                         AttendanceAoStatusNote = s.AttendanceAoStatusNote,
                         AttendanceAoLastLocTrack = s.AttendanceAoLastLocTrack,
                         Status = _lookUp.FirstOrDefault(w => w.LookupIndex == s.AttendanceAoStatus
                         && w.LookupType == "ATTENDSTAT").LookupDescription
                     }).ToList();

            return getStatusDetails;
        }
        private List<AttendanceAoVm> GetLogCount(int personnelId, LogCountVm value, List<AttendanceAoHistory> attendanceAOList)
        {
            List<AttendanceAoVm> getLogDetails = attendanceAOList.Where(w => w.AttendanceAoOfficerLog != null
                       && ((w.AttendanceAoOfficerLogDate >= DateTime.Now.AddHours(-12)
                      && w.AttendanceAoOfficerLogDate <= DateTime.Now.AddHours(-12)) ||
                    (w.AttendanceAoOfficerLogDate >= value.FromDate && w.AttendanceAoOfficerLogDate <= value.ToDate))
                    && w.AttendanceAo.PersonnelId == personnelId)
                    .Select(s => new AttendanceAoVm
                    {
                        AttendanceAOHistoryId = s.AttendanceAoHistoryId,
                        AttendanceAoStatusDate = s.AttendanceAoStatusDate,
                        AttendanceAoOfficerLog = s.AttendanceAoOfficerLog
                    }).ToList();

            return getLogDetails;
        }
    }
}
