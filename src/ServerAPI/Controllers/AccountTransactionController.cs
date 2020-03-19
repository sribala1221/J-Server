using ServerAPI.Services;
using Microsoft.AspNetCore.Mvc;
using ServerAPI.ViewModels;

namespace ServerAPI.Controllers
{
    [Route("[controller]")]
    public class AccountTransactionController : Controller
    {
        private readonly IAccountTransactionService _accountTransactionService;
        public AccountTransactionController(IAccountTransactionService accountTransactionService)
        {
            _accountTransactionService = accountTransactionService;
        }

        [HttpGet("GetAccountTransactionList")]
        public IActionResult GetAccountTransactionList(AccountTransactionVm transaction)
        {
            return Ok(_accountTransactionService.GetAccountTransactionList(transaction));
        }
        [HttpGet("GetLedgerAmount")]
        public IActionResult GetLedgerAmount(MoneyInmateLedgerVm ledger)
        {
            return Ok(_accountTransactionService.GetMoneyLedgerList(ledger));
        }
    }
}