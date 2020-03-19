using ServerAPI.ViewModels;
using Microsoft.AspNetCore.Mvc;
using ServerAPI.Services;
using System;

namespace ServerAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class MoneyCashController : ControllerBase
    {
        private readonly IMoneyCashService _moneyCashService;
        public MoneyCashController(IMoneyCashService moneyCashService)
        {
            _moneyCashService = moneyCashService;
        }

        #region Money->Cash->Ledger
        [HttpGet("GetCashDrawers")]
        public IActionResult GetCashDrawers(int facilityId) => Ok(_moneyCashService.MoneyGetCashDrawer(facilityId));
        [HttpGet("GetCashDetails")]
        public IActionResult GetCashDetails([FromQuery]MoneyCashDetailVm searchValue) => Ok(_moneyCashService.GetCashDetails(searchValue));
        [HttpGet("GetAccountDetail")]
        public IActionResult GetAccountDetail(int facilityId) => Ok(_moneyCashService.GetAccountDetail(facilityId));
        [HttpPost("DepositTransaction")]
        public IActionResult DepositTransaction(CashTransactionDetailVm detail) => 
            Ok(_moneyCashService.DepositTransaction(detail));
        [HttpPost("WithdrawTransaction")]
        public IActionResult WithdrawTransaction(CashTransactionDetailVm detail) => 
            Ok(_moneyCashService.WithdrawTransaction(detail));
        [HttpPost("BankToCashTransaction")]
        public IActionResult BankToCashTransaction(CashTransactionDetailVm detail) => 
            Ok(_moneyCashService.BankToCashTransaction(detail));
        [HttpPost("CashToBankTransaction")]
        public IActionResult CashToBankTransaction(CashTransactionDetailVm detail) => 
            Ok(_moneyCashService.CashToBankTransaction(detail));
        [HttpPost("VaultToCashTransaction")]
        public IActionResult VaultToCashTransaction(CashTransactionDetailVm detail) => 
            Ok(_moneyCashService.VaultToCashTransaction(detail));
        [HttpPost("CashToVaultTransaction")]
        public IActionResult CashToVaultTransaction(CashTransactionDetailVm detail) => 
            Ok(_moneyCashService.CashToVaultTransaction(detail));
        [HttpPost("CashToCashTransaction")]
        public IActionResult CashToCashTransaction(CashTransactionDetailVm detail) => 
            Ok(_moneyCashService.CashToCashTransaction(detail));
        [HttpPost("Journal")]
        public IActionResult Journal(CashTransactionDetailVm detail) => Ok(_moneyCashService.Journal(detail));
        [HttpGet("GetTransactionDetail")]
        public IActionResult GetTransactionDetail(int transId, bool voidFlag) => Ok(_moneyCashService.GetTransactionDetail(transId, voidFlag));
        #endregion

        #region Money->Cash->Verify

        [HttpPost("MoneyCashInsert")]
        public IActionResult MoneyCashInsert(MoneyCashInsertVm param) => Ok(_moneyCashService.MoneyCashInsert(param));
        [HttpGet("MoneyCashVerify")]
        public IActionResult MoneyCashVerify(int bankId, int cashBalanceId, DateTime? fromDate, DateTime? toDate) => 
            Ok(_moneyCashService.MoneyCashVerify(bankId, cashBalanceId, fromDate, toDate));

        [HttpGet("MoneyCashBalance")]
        public IActionResult MoneyCashBalance(int cashBalanceId) => Ok(_moneyCashService.MoneyCashBalance(cashBalanceId));
        #endregion

        #region Money->Cash->Search
        [HttpGet("GetCashSearchDetails")]
        public IActionResult GetCashSearchDetails(MoneyCashDetailVm searchValue) => 
            Ok(_moneyCashService.GetCashSearchDetails(searchValue));
        #endregion
    }
}
