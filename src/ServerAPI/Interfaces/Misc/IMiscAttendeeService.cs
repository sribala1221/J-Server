using System;
using ServerAPI.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ServerAPI.Services
{
    public interface IMiscAttendeeService
    {
        List<AttendenceVm> GetInmateAttendeeList(int inmateId);
        Task<bool> InsertAttendee(AttendenceVm obAttendace);
        Task<int> UpdateAttendee(AttendenceVm obAttendance);
        Task<int> DeleteUndoAttendee(int attendanceId);
    }
}
