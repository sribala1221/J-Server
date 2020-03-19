using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServerAPI.Services;

namespace ServerAPI.Controllers
{
    [Route("[controller]")]
    public class CustomClientMappingController : ControllerBase
    {
        private readonly ICustomClientMappingService _iCustomClientMapping;

        public CustomClientMappingController(ICustomClientMappingService iCustomClientMapping)
        {
            _iCustomClientMapping = iCustomClientMapping;
        }

        [HttpGet("GetClientCustomLabels")]
        [AllowAnonymous]
        public IActionResult GetClientCustomLabels()
        {
            return Ok(_iCustomClientMapping.GetClientCustomLabels());
        }

        [HttpGet("GetCustomFieldLookups")]
        [AllowAnonymous]
        public IActionResult GetCustomFieldLookups(int appAoUserControlId)
        {
            return Ok(_iCustomClientMapping.GetCustomFieldLookups(appAoUserControlId));
        }
    }
}