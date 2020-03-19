using ServerAPI.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ServerAPI.Services
{
    public interface IProgramAttendanceService
    {
        List<ProgramAttendanceDetails> GetProgramAttendanceRequest (int scheduleId);
        Task<int> InsertProgramAttendanceRequest(List<ProgramAttendanceCourseVm> programAttend);
    }
}