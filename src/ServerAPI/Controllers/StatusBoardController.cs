using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ServerAPI.Services;
using ServerAPI.ViewModels;

namespace ServerAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class StatusBoardController : ControllerBase
    {
        private readonly IStatusBoardOverviewService _statusBoardOverviewService;
        public StatusBoardController(IStatusBoardOverviewService statusBoardOverviewService)
        {
            _statusBoardOverviewService = statusBoardOverviewService;
        }
        
        [HttpGet("GetStatusBoardOverview")]
        public IActionResult GetStatusBoardOverview(int facilityId)
        {
            return Ok(_statusBoardOverviewService.GetStatusBoardOverview(facilityId));
        }

        [HttpGet("GetMonitorPreScreenDetail")]
        public IActionResult GetMonitorPreScreenDetail([FromQuery] OverviewDetailInputVm value)
        {
            return Ok(_statusBoardOverviewService.GetMonitorPreScreenDetail(value));
        }

        [HttpGet("GetInmateDetail")]
        public IActionResult GetInmateDetail([FromQuery] OverviewDetailInputVm value)
        {
            return Ok(_statusBoardOverviewService.GetInmateDetail(value));
        }

        [HttpGet("GetGrivanceDetail")]
        public IActionResult GetGrivanceDetail([FromQuery] OverviewDetailInputVm value)
        {
            return Ok(_statusBoardOverviewService.GetGrivanceDetail(value));
        }

        [HttpGet("GetIncidentDetail")]
        public IActionResult GetIncidentDetail([FromQuery] OverviewDetailInputVm value)
        {
            return Ok(_statusBoardOverviewService.GetIncidentDetail(value));
        }

        [HttpGet("GetSupervisorReviewDetail")]
        public IActionResult GetSupervisorReviewDetail([FromQuery] OverviewDetailInputVm value)
        {
            return Ok(_statusBoardOverviewService.GetSupervisorReviewDetail(value));
        }

        [HttpGet("GetPreScreeningCount")]
        public IActionResult GetPreScreeningCount(int facilityId)
        {
            return Ok(_statusBoardOverviewService.GetPreScreeningCount(facilityId));
        }

        [HttpGet("GetWizardSentenceCount")]
        public IActionResult GetWizardSentenceCount(int facilityId)
        {
            return Ok(_statusBoardOverviewService.GetWizardSentenceCount(facilityId));
        }

        [HttpGet("GetIncidentCount")]
        public IActionResult GetIncidentCount(int facilityId)
        {
            return Ok(_statusBoardOverviewService.GetIncidentCount(facilityId));
        }
        [HttpGet("GetInmateCount")]
        public IActionResult GetInmateCount(int facilityId)
        {
            return Ok(_statusBoardOverviewService.GetInmateCount(facilityId));
        }

        [HttpGet("GetStatsCount")]
        public IActionResult GetStatsCount(int facilityId)
        {
            return Ok(_statusBoardOverviewService.GetStatsCount(facilityId));
        }

        [HttpGet("GetGrievanceCount")]
        public IActionResult GetGrievanceCount(int facilityId)
        {
            return Ok(_statusBoardOverviewService.GetGrievanceCount(facilityId));
        }
    }
}
