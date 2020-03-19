using Microsoft.AspNetCore.Mvc;
using ServerAPI.ViewModels;
using ServerAPI.Interfaces;

namespace ServerAPI.Controllers
{
    [Route("[controller]")]
    public class VisitAvailabilityController : ControllerBase
    {
        private readonly IVisitAvailabilityService _visitAvailabilityService;

        public VisitAvailabilityController( IVisitAvailabilityService visitAvailabilityService)
        {
            _visitAvailabilityService = visitAvailabilityService;
        }
        [HttpGet("GetVisitAvailability")]
        public IActionResult GetVisitAvailability([FromQuery]VisitScheduledVm visitationRoomInfo)
        {
            return Ok(_visitAvailabilityService.GetVisitAvailability(visitationRoomInfo));
        }
    }
}
