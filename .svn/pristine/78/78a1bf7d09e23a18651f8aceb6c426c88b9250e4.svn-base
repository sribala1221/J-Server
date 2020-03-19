using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GenerateTables.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ScheduleWidget.Common;
using ServerAPI.Services;
using ServerAPI.ViewModels;

namespace ServerAPI.Controllers
{

    [Route("[controller]")]
    public class AOScheduleController : ControllerBase
    {
        private readonly IAoScheduleService _aoScheduleService;
      

        public AOScheduleController(IAoScheduleService aoScheduleService)
        {
            _aoScheduleService = aoScheduleService;
        }
        // example of inmate appointment 

        [HttpPost("createScheduleException")]
        public IActionResult CreateScheduleException([FromBody] ScheduleExclude aoScheduleExclude)
        {
            return Ok(_aoScheduleService.InsertScheduleExclusion(aoScheduleExclude, Convert.ToInt32(User.FindFirst("personnelId")?.Value)));

        }

        [AllowAnonymous]
        [HttpPost("createInmateExclusion")]
        public IActionResult CreateInmateSchedule([FromBody] ScheduleExcludeInmate aoScheduleExcludeInmate)
        {
            return Ok(_aoScheduleService.InsertInmateExclusion(aoScheduleExcludeInmate, Convert.ToInt32(User.FindFirst("personnelId")?.Value)));
        }

    }

}
        






