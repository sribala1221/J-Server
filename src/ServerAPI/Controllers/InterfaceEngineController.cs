using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using ServerAPI.Services;
using ServerAPI.ViewModels;

namespace ServerAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class InterfaceEngineController : ControllerBase
    {
        private readonly IInterfaceEngineService _interfaceService;

        public InterfaceEngineController(IInterfaceEngineService interfaceService)
        {
            _interfaceService = interfaceService;
        }

        [AllowAnonymous]
        [HttpPost("InboundRequest")]
        public IActionResult InboundRequest([FromBody] InboundRequestVM values)
        {
            return Ok( _interfaceService.Inbound(values));
        }

        [AllowAnonymous]
        [HttpPost("TestExportRequest")]
        public ActionResult<string> TestExportRequest(ExportRequestVm values)
        {
            Object result = _interfaceService.TestExportRequest(values);
            return Ok(result);
        }

        [AllowAnonymous]
        [HttpPost("SaveInmatePrebook")]
        public IActionResult SaveInmatePrebook([FromBody] InmatePrebookSacramento values)
        {
            return Ok(_interfaceService.SaveInmatePrebookSacramento(values));
        }

        [HttpGet("GetInmatePrebook")]
        public IActionResult GetInmatePrebook(string inmateNumber)
        {
            return Ok(_interfaceService.GetInmatePrebookSacramento(inmateNumber));
        }
    }
}