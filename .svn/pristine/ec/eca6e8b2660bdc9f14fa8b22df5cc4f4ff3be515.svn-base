using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ServerAPI.Services;
using ServerAPI.ViewModels;

namespace ServerAPI.Controllers
{
    [Route("[controller]")]
    public class CustomQueueController : ControllerBase
    {
        private readonly ICustomQueueService _customQueueService;

        public CustomQueueController(ICustomQueueService customQueueService)
        {
            _customQueueService = customQueueService;
        }

        [HttpGet("GetCustomQueue")]
        public IActionResult GetCustomQueue() => Ok(_customQueueService.GetCustomQueue());

        [HttpGet("GetCustomQueueEntryDetails")]
        public IActionResult GetCustomQueueEntryDetails(int customQueueId) => 
            Ok(_customQueueService.GetCustomQueueEntryDetails(customQueueId));

        [HttpPost("GetCustomQueueSearch")]
        public async Task<IActionResult> GetCustomQueueSearch([FromBody] CustomQueueSearchInput customQueueSearch) => 
            Ok(await _customQueueService.GetCustomQueueSearch(customQueueSearch));
    }
}