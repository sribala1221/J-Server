using Microsoft.AspNetCore.Mvc;
using ServerAPI.Services;
using System.Threading.Tasks;
using ServerAPI.ViewModels;

namespace ServerAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class MonitorTimeLineController : Controller
    {
        private readonly IMonitorTimeLineService _monitorTimeLineService;
        public MonitorTimeLineController(IMonitorTimeLineService monitorTimeLineService)
        {
            _monitorTimeLineService = monitorTimeLineService;
        }

        #region Monitor => TimeLine
        //Load monitor timeline grid details
        [HttpGet("GetMonitorTimeLine")]
        public IActionResult GetMonitorTimeLine([FromQuery]MonitorTimeLineSearchVm searchValues)
        {
            return Ok(_monitorTimeLineService.GetMonitorTimeLine(searchValues));
        }
        #endregion

    }
}