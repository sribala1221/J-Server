using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using ServerAPI.Services;
using ServerAPI.ViewModels;

namespace ServerAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class MoneyBalanceInmateController : ControllerBase
    {
        private IMoneyBalanceInmateService _moneyBalanceInmateService;
        public MoneyBalanceInmateController(IMoneyBalanceInmateService  moneyBalanceInmateService)
        {
            _moneyBalanceInmateService = moneyBalanceInmateService;
        }

        [HttpGet("GetBalanceInmateLoad")]
        public IActionResult MoneyBalanceInmateLoad(int bankId, int inmateStatus, int accountBalanceType, int facilityId)
        {
            return Ok(_moneyBalanceInmateService.MoneyBalanceInmateLoad(bankId, inmateStatus, accountBalanceType, facilityId));
        }

        [HttpGet("GetWorkstationMoneyDetailList")]
        public IActionResult WorkstationMoneyDetailList(int facilityId)
        {
            return Ok(_moneyBalanceInmateService.WorkstationMoneyDetailList(facilityId));
        }

        [HttpGet("GetMoneyTransactionList")]
        public IActionResult MoneyTransactionList(int facilityId, int bankId)
        {
            return Ok(_moneyBalanceInmateService.MoneyTransactionList(facilityId,bankId));
        }
        [HttpGet("GetInmateCurrentBalance")]
        public IActionResult MoneyGetInmateCurrentBalance(int inmateId, int bankId)
        {
            return Ok(_moneyBalanceInmateService.MoneyGetInmateCurrentBalance(inmateId,bankId));
        }
        [HttpPost("MoneyReceiptCashCheckInsert")]
        public IActionResult MoneyReceiptCashCheckInsert(MoneyCashCheckVm param)
        {
            return Ok(_moneyBalanceInmateService.MoneyReceiptCashCheckInsert(param));
        }
        [HttpPost("MoneyTransactionWriteCheckInsert")]
        public IActionResult MoneyTransactionWriteCheckInsert(MoneyCashCheckVm param)
        {
            return Ok(_moneyBalanceInmateService.MoneyTransactionWriteCheckInsert(param));
        }
        [HttpPost("MoneyReturnCash")]
        public IActionResult MoneyReturnCash(MoneyCashCheckVm param)
        {
            return Ok(_moneyBalanceInmateService.MoneyReturnCash(param));
        }

        [HttpPost("MoneyPurchase")]
        public IActionResult MoneyPurchase(MoneyCashCheckVm param)
        {
            return Ok(_moneyBalanceInmateService.MoneyPurchase(param));
        }
        [HttpPost("MoneyAppFee")]
        public IActionResult MoneyAppFee(MoneyCashCheckVm param)
        {
            return Ok(_moneyBalanceInmateService.MoneyAppFee(param));
        }
        [HttpPost("MoneyDonate")]
        public IActionResult MoneyDonate(MoneyCashCheckVm param)
        {
            return Ok(_moneyBalanceInmateService.MoneyDonate(param));
        }
        [HttpPost("MoneyRefund")]
        public IActionResult MoneyRefund(MoneyCashCheckVm param)
        {
            return Ok(_moneyBalanceInmateService.MoneyRefund(param));
        }
        [HttpPost("MoneyJournal")]
        public IActionResult MoneyJournal(MoneyCashCheckVm param)
        {
            return Ok(_moneyBalanceInmateService.MoneyJournal(param));
        }
        [HttpGet("GetInmateLedgerTransaction")]
        public IActionResult MoneyInmateLedgerTransaction(int flag, int bankId, int inmateId)
        {
            return Ok(_moneyBalanceInmateService.MoneyInmateLedgerTransaction(flag,bankId,inmateId));
        }
        [HttpPost("MoneyRunFeeCheck")]
        public IActionResult MoneyRunFeeCheck(List<MoneyLedgerVm> aoFeeList)
        {
            return Ok(_moneyBalanceInmateService.MoneyRunFeeCheck(aoFeeList));
        }
    }
}