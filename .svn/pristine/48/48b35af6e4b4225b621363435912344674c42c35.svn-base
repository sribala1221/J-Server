using Microsoft.AspNetCore.Mvc;
using ServerAPI.ViewModels;
using ServerAPI.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ServerAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class MoneyClearController:ControllerBase
    {
        private readonly IMoneyClearService _moneyClearService;
        public MoneyClearController(IMoneyClearService moneyClearService)
        {
            _moneyClearService=moneyClearService;
        }

        //To get the Bank & Fund dropdown details
        [HttpGet("GetDropDownDetails")]
        public IActionResult GetDropDownDetails()
        {
            return Ok(_moneyClearService.GetDropDownDetails());
        }

        //To get the Money-Clear-Clear-List Details
        [HttpGet("GetMoneyClearClearDetails")]
        public IActionResult GetMoneyClearClearDetails([FromQuery] MoneyAccountFilterVm searchValue)
        {
            return Ok(_moneyClearService.GetMoneyClearClearDetails(searchValue));
        }

        //To do the DoTransaction in Money-Clear-Clear-List Details 
        [HttpPost("UpdateMoneyClearClearTransaction")]
        public async Task<IActionResult> UpdateMoneyClearClearTransaction(List<int> checkedTransIdList)
        {
            return Ok(await _moneyClearService.UpdateMoneyClearClearTransaction(checkedTransIdList));
        }

        [HttpPost("InsertReturnCheck")]
        public async Task<IActionResult> InsertReturnCheck(List<int> param)
        {
            return Ok(await _moneyClearService.InsertReturnCheck(param));
        }

        //To get the Money-Clear-History-List Details
        [HttpGet("GetMoneyClearHistoryDetails")]
        public IActionResult GetMoneyClearHistoryDetails([FromQuery] MoneyAccountFilterVm searchValue)
        {
            return Ok(_moneyClearService.GetMoneyClearHistoryDetails(searchValue));
        }

    }
}