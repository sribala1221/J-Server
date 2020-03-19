using Microsoft.AspNetCore.Mvc;
using ServerAPI.Interfaces;
using ServerAPI.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ServerAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ProgramController : ControllerBase
    {
        private readonly IProgramClassService _programClassService;
        private readonly IProgramInstructorService _programInstructorService;

        public ProgramController(IProgramClassService programClassService,
        IProgramInstructorService programInstructorService)
        {
            _programClassService = programClassService;
            _programInstructorService = programInstructorService;
        }

        //ProgramClass Schedules
        [HttpGet("GetProgramClassSchedules")]
        public IActionResult GetProgramClassSchedules(int? facilityId) => 
            Ok(_programClassService.GetProgramClassSchedules(facilityId));

        //Insert ProgramClass 
        [HttpPost("InsertProgramClass")]
        public IActionResult InsertProgramClass(ProgramClassScheduleVm pgmClassDetails) => 
            Ok(_programClassService.InsertProgramClass(pgmClassDetails));

        //Insert ProgramClass Schedules
        [HttpPost("InsertProgramClassSchedule")]
        public async Task<IActionResult> InsertProgramClassSchedule(ProgramClassScheduleVm pgmClassDetails) => 
            Ok(await _programClassService.InsertProgramClassSchedule(pgmClassDetails));

        //Update ProgramClass 
        [HttpPost("UpdateProgramClass")]
        public IActionResult UpdateProgramClass(ProgramClassScheduleVm pgmClassDetails) => 
            Ok(_programClassService.UpdateProgramClass(pgmClassDetails));

        //Update ProgramClass Schedules
        [HttpPost("UpdateProgramClassSchedule")]
        public async Task<IActionResult> UpdateProgramClassSchedule(ProgramClassScheduleVm pgmClassDetails) => 
            Ok(await _programClassService.UpdateProgramClassSchedule(pgmClassDetails));

        [HttpGet("GetCurrentCourse")]
        public IActionResult GetCurrentCourse(int facilityId, DateTime fromDate, DateTime toDate) => 
            Ok(_programClassService.GetCurrentCourse(facilityId, fromDate, toDate));


        [HttpGet("GetCurrentCourseList")]
        public IActionResult GetCurrentCourseList(int facilityId, DateTime fromDate, DateTime toDate) => 
            Ok(_programClassService.GetCurrentCourseList(facilityId, fromDate, toDate));

        [HttpPost("DeleteScheduleProgram")]
        public async Task<IActionResult> DeleteScheduleProgram(ProgramAppointmentVm programRequest) => 
            Ok(await _programClassService.DeleteScheduleProgram(programRequest));

        [HttpGet("GetInstructorsList")]
        public IActionResult GetInstructorsList() => Ok(_programClassService.GetInstructorsList());

        //To get Inmate details for Class enrollment
        [HttpGet("GetClassEnrollmentInmateDetails")]
        public IActionResult GetClassEnrollmentInmateDetails(int inmateId) => 
            Ok(_programClassService.GetClassEnrollmentInmateDetails(inmateId));

        [HttpGet("GetClassListDetails")]
        public IActionResult GetClassListDetails(int? inmateId) => Ok(_programClassService.GetClassListDetails(inmateId));

        [HttpGet("GetEnrollmentRequest")]
        public IActionResult GetEnrollmentRequest(int programClassId, int locationId, int scheduleId) => 
            Ok(_programClassService.GetEnrollmentRequest(programClassId, locationId, scheduleId));

        [HttpPost("InsertEnrollmentDetails")]
        public IActionResult InsertEnrollmentDetails(EnrollmentRequest enrollmentRequest) => 
            Ok(_programClassService.InsertEnrollmentDetails(enrollmentRequest));
        [HttpGet("GetProgramInstructor")]

        public IActionResult GetProgramInstructor() => Ok(_programInstructorService.GetProgramInstructor());

        #region Class Management
        
        [HttpGet("GetClassManagementDetails")]
        public IActionResult GetClassManagementDetails(int scheduleId, int programClassId) => 
            Ok(_programClassService.GetClassManagement(scheduleId, programClassId));

        [HttpPost("UpdateClassManagement")]
        public IActionResult UpdateClassManagement(List<ClassManagement> classManagementDetails) => 
            Ok(_programClassService.UpdateClassManagement(classManagementDetails));
        #endregion

        [HttpPost("InsertInstructorCertificate")]
        public IActionResult InsertInstructorCertificate(List<ProgramInstructorCertVm> certificateList) => 
            Ok(_programInstructorService.InsertInstructorCertificate(certificateList));
        [HttpGet("GetScheduleDetailsList")]
        public IActionResult GetScheduleDetailsList(int programClassId) => 
            Ok(_programClassService.GetScheduleDetailsList(programClassId));
        [HttpPost("DeleteProgramClassSchedule")]
        public IActionResult DeleteProgramClassSchedule(int scheduleId) => 
            Ok(_programClassService.DeleteProgramClassSchedule(scheduleId));
    }
}
