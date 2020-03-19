using System.Collections.Generic;
using ServerAPI.ViewModels;
using System.Threading.Tasks;
using System;

namespace ServerAPI.Services
{
    public interface IAppointmentService
    {
        AppointmentLocationVm GetAppointmentLocation(int inmateId, int? facilityId, ApptLocationFlag flag);

        Task<AppointmentConflictCheck> InsertSchedule(InmateApptDetailsVm inmateApptDetails);

        List<AoAppointmentVm> ListAoAppointments(int facilityId, int? inmateId, DateTime fromDate, DateTime toDate, bool isActive,
            bool inProgress = false, bool checkReOccurence = false);

        Task<AppointmentConflictCheck> UpdateSchedule(InmateApptDetailsVm inmateApptDetails);

        InmateApptDetailsVm GetAppointmentSchedule(int scheduleId);
        BumpQueueVm GetBumpQueueDetails(int facilityId, string fromModule);
        List<HousingDetail> GetBumpQueueHousingDetails(int? facilityId, string housingUnitLocation);

        BumpQueueVm GetBumpQueueInfo(int facilityId, bool isActiveBump, DateTime? startDate, DateTime? endDate,
            string housingUnitLocation, string housingUnitNumber, int? inmateId, string fromModule);

        Task<int> ClearBumpQueue(KeyValuePair<int, string> clearBump);
        Task<int> InsertMultiInmateAppt(InmateApptDetailsVm inmateApptDetails, List<int> inmateIds);

        List<KeyValuePair<int, string>> GetApptDeleteReason();

        Task<int> DeleteInmateAppointment(DeleteUndoInmateAppt deleteInmateAppt);
        Task<int> UndoInmateAppointment(DeleteUndoInmateAppt undoInmateAppt);

        AppointmentConflictCheck CheckAppointmentConflict(InmateApptDetailsVm inmateApptDetails);

        AppointmentLocationVm ProgramAppointmentDropDowns(int facilityId);
        ProgramInstructor GetProgramInstructorDetails(int programId, int appointmentId);
        List<ScheduleSaveHistoryVm> GetScheduleSaveHistory(int scheduleId); 
    }
}
