
using Microsoft.AspNetCore.Mvc;
using ServerAPI.Services;
using System.Threading.Tasks;
using ServerAPI.ViewModels;

namespace ServerAPI.Controllers

{
    [Route("[controller]")]
    public class LogCountController : Controller
    {
        private readonly ILogCountService _logCountService;
        public LogCountController(ILogCountService logCountService)
        {
            _logCountService = logCountService;
        }

        [HttpGet("GetLogCountDetails")]
        public IActionResult GetLogCountDetails(LogCountVm value)
        {
            return Ok(_logCountService.GetLogCountDetails(value));
        }

    }
}

