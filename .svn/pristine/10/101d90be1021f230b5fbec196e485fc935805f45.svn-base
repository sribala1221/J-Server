using Microsoft.AspNetCore.Mvc;
using ServerAPI.Services;
using ServerAPI.ViewModels;

namespace ServerAPI.Controllers
{
    [Route("[controller]")]
    public class FacilityAppointmentController : ControllerBase
    {
        private readonly IFacilityAppointmentService _facilityAppointmentService;

        public FacilityAppointmentController(IFacilityAppointmentService facilityAppointmentService)
        {
            _facilityAppointmentService = facilityAppointmentService;
        }
        
        [HttpGet("GetAppointmentList")]
        public IActionResult GetAppointmentList([FromQuery] FacilityApptVm lsthousingUnitListId)
        {
            return Ok(_facilityAppointmentService.GetAppointmentList(lsthousingUnitListId));
        }

        [HttpGet("LoadAppointmentFilter")]
        public IActionResult LoadAppointmentFilter([FromQuery] int facilityId)
        {
            return Ok(_facilityAppointmentService.LoadApptFilterlist(facilityId));
        }
    }
}
