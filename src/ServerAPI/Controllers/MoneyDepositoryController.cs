using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using ServerAPI.Services;
using ServerAPI.ViewModels;
namespace ServerAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class MoneyDepositoryController : ControllerBase
    {
        private readonly IMoneyDepositVerifyService _moneyDepositVerifyService;
        private readonly IMoneyDepositoryAdjustService _moneyDepositoryAdjustService;
        private readonly IMoneyDepositoryHistoryService _moneyDepositoryHistoryService;

        public MoneyDepositoryController(IMoneyDepositVerifyService moneyDepositVerifyService,
            IMoneyDepositoryAdjustService moneyDepositoryAdjustService, 
            IMoneyDepositoryHistoryService moneyDepositoryHistoryService)
        {
            _moneyDepositVerifyService = moneyDepositVerifyService;
            _moneyDepositoryAdjustService = moneyDepositoryAdjustService;
            _moneyDepositoryHistoryService = moneyDepositoryHistoryService;
        }

        [HttpGet("GetMoneyDepositoryDetails")]
        public IActionResult GetMoneyDepositoryDetails()
        {
            return Ok(_moneyDepositVerifyService.GetMoneyDepositoryDetails());
        }

        //To Get the Depository Verify details 
        [HttpGet("GetMoneyDepositoryVerifyDetails")]
        public IActionResult GetMoneyDepositoryVerifyDetails(int bankId,
            int accountAoDepositoryId, int fundId, int personnelId)
        {
            return Ok(_moneyDepositVerifyService.GetMoneyDepositoryVerifyDetails(bankId,
                accountAoDepositoryId, fundId, personnelId));
        }
        [HttpPost("MoneyDepositoryVerify")]
        public IActionResult MoneyDepositoryVerify(List<MoneyAccountTransactionVm> feeList)
        {
            return Ok(_moneyDepositVerifyService.MoneyDepositoryVerify(feeList));
        }
       
        [HttpGet("MoneyAdjustTransaction")]
        public IActionResult MoneyAdjustTransaction([FromQuery] MoneyAdjustInputVm adjustTransaction)
        {
            return Ok(_moneyDepositoryAdjustService.MoneyAdjustTransaction(adjustTransaction));
        }

        [HttpGet("GetMoneyDepositoryHistory")]
        public IActionResult GetMoneyDepositoryHistory()
        {
            return Ok(_moneyDepositoryHistoryService.GetMoneyDepositoryHistory());
        }

        [HttpGet("GetMoneyDepositoryHistoryDetails")]
        public IActionResult GetMoneyDepositoryHistoryDetails(int bankId,
            int accountAoDepositoryId, int fundId, int personnelId,DateTime? transactionDate)
        {
            return Ok(_moneyDepositoryHistoryService.GetMoneyDepositoryHistoryDetails(bankId,
                accountAoDepositoryId, fundId, personnelId,transactionDate));
        }
    }
}







