using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ServerAPI.Services;
using ServerAPI.ViewModels;

namespace ServerAPI.Controllers
{
    [Route("[controller]")]
    public class RecordsSpecialHandlingController : ControllerBase
    {
        private readonly IWhosInCustodyService _whosInCustodyService;

        public RecordsSpecialHandlingController(IWhosInCustodyService whosInCustodyService)
        {
            _whosInCustodyService = whosInCustodyService;
        }

        [HttpGet("GetWhosInCustody")]
        public IActionResult GetWhosInCustody()
        {
            return Ok(_whosInCustodyService.GetWhosInCustody());
        }

        [HttpPost("InsertWhosInCustody")]
        public async Task<IActionResult> InsertWhosInCustody([FromBody] WhosInCustodyRemoveVm obj)
        {
            return Ok(await _whosInCustodyService.InsertWhosInCustody(obj));
        }

        [HttpPost("UpdateWhosInCustody")]
        public async Task<IActionResult> UpdateWhosInCustody([FromBody] WhosInCustodyRemoveVm obj)
        {
            return Ok(await _whosInCustodyService.UpdateWhosInCustody(obj));
        }

        [HttpPut("DeleteWhosInCustody")]
        public IActionResult DeleteWhosInCustody([FromBody] WhosInCustodyRemoveVm obj)
        {
            return Ok(_whosInCustodyService.DeleteWhosInCustody(obj));
        }

        [HttpPut("UpdateWhosInCustody")]
        public IActionResult UndoWhosInCustody([FromBody] WhosInCustodyRemoveVm obj)
        {
            return Ok(_whosInCustodyService.UndoWhosInCustody(obj));
        }
    }
}