using ServerAPI.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ServerAPI.Interfaces
{
    public interface IProgramClassService
    {
        ProgramClassScheduleVm GetProgramClassSchedules(int? facilityId);
        int InsertProgramClass(ProgramClassScheduleVm pgmClassDetails);

        Task<AppointmentConflictCheck> InsertProgramClassSchedule(ProgramClassScheduleVm pgmClassDetails);
        int UpdateProgramClass(ProgramClassScheduleVm pgmClassDetails);
        Task<AppointmentConflictCheck> UpdateProgramClassSchedule(ProgramClassScheduleVm pgmClassDetails);
        List<ProgramAppointmentVm> GetCurrentCourse(int facilityId, DateTime fromDate, DateTime toDate);
        List<ProgramAppointmentVm> GetCurrentCourseList(int facilityId, DateTime fromDate, DateTime toDate);
        Task<int> DeleteScheduleProgram(ProgramAppointmentVm pgmClassDetails);
        List<InstructorsVm> GetInstructorsList();
        List<ProgramClassDetailsVm> GetClassListDetails(int? inmateId);
        EnrollmentRequest GetEnrollmentRequest(int programClassId, int locationId, int scheduleId);
        //To get Inmate details for Class enrollment
        ClassEnrollmentInmateDetails GetClassEnrollmentInmateDetails(int inmateId);
        int InsertEnrollmentDetails(EnrollmentRequest enrollmentRequest);
        List<ScheduleVm> GetScheduleDetailsList(int programClassId);
        int DeleteProgramClassSchedule(int scheduleId);
        // Class Management
        List<ClassManagement> GetClassManagement(int scheduleId, int programClassId);
        int UpdateClassManagement(List<ClassManagement> classManagementDetails);
    }
}
