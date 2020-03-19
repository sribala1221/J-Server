using ServerAPI.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ServerAPI.Services
{
    public interface ISentenceAttendeeService
    {
        List<AttendanceVm> GetAttendanceList(DateTime dateValue);
        AttendanceVm GetAttendanceValue(int arrestSentenceAttendanceId);
        List<RecentAttendanceVm> GetRecentAttendanceValue(DateTime dateTime);
        int GetDuplicateAttendance(DateTime dateValue, int inmateId);
        Task<int> InsertAttendance(AttendanceVm value);
        WorkAttendanceVm GetRunAttendanceList(DateTime dateValue);
        Task<int> InsertArrestSentenceAttendanceDay(WorkAttendanceVm value);
    }
}
