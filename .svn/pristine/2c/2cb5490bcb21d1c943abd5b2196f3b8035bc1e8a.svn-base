using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ServerAPI.Services;
using ServerAPI.ViewModels;

namespace ServerAPI.Controllers {

    [Route("[controller]")]
    [ApiController]
    public class ProgramAttendanceController : ControllerBase
    {
        private readonly IProgramAttendanceService _programAttendanceService;

        public ProgramAttendanceController(IProgramAttendanceService programAttendanceService)
        {
            _programAttendanceService = programAttendanceService;
        }

        [HttpGet("GetProgramAttendanceRequest")]
        public IActionResult GetProgramAttendanceRequest(int scheduleId) =>
            Ok(_programAttendanceService.GetProgramAttendanceRequest(scheduleId));


        [HttpPost("InsertProgramAttendanceRequest")]
        public async Task<IActionResult>
            InsertProgramAttendanceRequest(List<ProgramAttendanceCourseVm> programAttend) =>
            Ok(await _programAttendanceService.InsertProgramAttendanceRequest(programAttend));


    }
}