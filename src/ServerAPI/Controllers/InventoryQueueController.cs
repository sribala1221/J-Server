using Microsoft.AspNetCore.Mvc;
using ServerAPI.Services;
using ServerAPI.ViewModels;
using ServerAPI.Authorization;
using System.Threading.Tasks;
// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace ServerAPI.Controllers
{
    [Route("[controller]")]
    public class InventoryQueueController : ControllerBase
    {
        private readonly IInventoryQueueService _inventoryQueueservice;


        public InventoryQueueController(IInventoryQueueService invq)
        {
            _inventoryQueueservice = invq;
        }

        [HttpGet("GetInventoryQueue")]
        public IActionResult GetInventoryQueue(int facilityId,int value,int scheduled)
        {
            return Ok(_inventoryQueueservice.GetInventoryQueue(facilityId,value,scheduled));
        }

        [HttpGet("GetInventoryInmateDetails")]
        public IActionResult GetInventoryInmateDetails(int facilityId, InventoryQueue value, int personalInventoryBinId, int selected, int schSelected)
        {
            return Ok(_inventoryQueueservice.GetInventoryInmateDetails(facilityId, value, personalInventoryBinId, selected, schSelected));
        }

        [HttpGet("GetInventoryProperyFormsDetails")]
        public IActionResult GetInventoryProperyFormsDetails()
        {
            return Ok(_inventoryQueueservice.GetInventoryProperyFormsDetails());
        }
        
        [HttpPost("ClearInventoryQueue")]
        public async Task<IActionResult> ClearInventoryQueue([FromBody] int recordId)
        {
            return Ok(await _inventoryQueueservice.ClearInventoryQueue(recordId));
        }
    }
}