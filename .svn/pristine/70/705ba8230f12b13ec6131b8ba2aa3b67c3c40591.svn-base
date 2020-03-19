using Microsoft.AspNetCore.Mvc;
using ServerAPI.Authorization;
using ServerAPI.Services;
using ServerAPI.ViewModels;

namespace ServerAPI.Controllers
{
    [Route("[controller]")]
    public class InmateHeaderController : ControllerBase
    {
        private readonly IInmateHeaderService _iInmateHeaderService;

        public InmateHeaderController(IInmateHeaderService iInmateHeaderService)
        {
            _iInmateHeaderService = iInmateHeaderService;
        }

        [HttpGet("GetInmateHeaderDetails")]
        public IActionResult GetInmateHeaderDetails(int inmateId)
        {
            return Ok(_iInmateHeaderService.GetInmateHeaderDetail(inmateId));
        }
    }
}
