using Microsoft.AspNetCore.Mvc;
using ServerAPI.Services;
using System.Threading.Tasks;
using ServerAPI.ViewModels;

namespace ServerAPI.Controllers

{
    [Route("[controller]")]
    public class MoneyInmateController : ControllerBase
    {
        private readonly IMoneyInmateService _moneyInmateService;
        public MoneyInmateController(IMoneyInmateService moneyInmateService)
        {
            _moneyInmateService = moneyInmateService;
        }

        [HttpGet("GetMoneyInmateLedger")]
        public IActionResult GetMoneyInmateLedger(int iBankId, int iInmateId)
        {
            return Ok(_moneyInmateService.GetMoneyInmateLedger(iBankId, iInmateId));
        }
        [HttpGet("GetMoneyInmateDeposit")]
        public IActionResult GetMoneyInmateDeposit(int iBankId, int iInmateId)
        {
            return Ok(_moneyInmateService.GetMoneyInmateDeposit(iBankId, iInmateId));
        }


        [HttpGet("GetMoneyInmateFeeDetails")]
        public IActionResult GetMoneyInmateFeeDetails(int bankId, int inmateId)
        {
            return Ok(_moneyInmateService.GetMoneyInmateFeeDetails(bankId, inmateId));
        }

        [HttpGet("GetMoneyInmateReference")]
        public IActionResult GetMoneyInmateReference(int? bankId, int? inmateId)
        {
            return Ok(_moneyInmateService.GetMoneyInmateReference(bankId, inmateId));
        }

        #region Money-> Inmate-> Clear
        [HttpGet("GetMoneyInmateClear")]
        public IActionResult MoneyInmateClear(int bankId, int inmateId)
        {
            return Ok(_moneyInmateService.MoneyInmateClear(bankId, inmateId));
        }

        [HttpPost("UpdateMoneyClearTransaction")]
        public async Task<IActionResult> UpdateMoneyClearTransaction([FromBody]int accountAoTransactionId)
        {
            return Ok(await _moneyInmateService.UpdateMoneyClearTransaction(accountAoTransactionId));
        }

        [HttpPost("InmateMoneyReturnTransaction")]
        public async Task<IActionResult> InmateMoneyReturnTransaction([FromBody]MoneyAccountTransactionVm moneyAccountTransaction)
        {
            return Ok(await _moneyInmateService.InmateMoneyReturnTransaction(moneyAccountTransaction));
        }


        #endregion
        [HttpGet("GetPrintReceive")]
        public IActionResult GetPrintReceive(int receiveId, int inmateId)
        {
            return Ok(_moneyInmateService.GetPrintReceive(receiveId, inmateId));
        }


    }
}

