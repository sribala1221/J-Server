using Microsoft.AspNetCore.Mvc;
using ServerAPI.Services;
using ServerAPI.ViewModels;
namespace ServerAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class MoneyTransactionController : Controller
    {
        private readonly IMoneyTransactionService _moneyTransactionService;
        public MoneyTransactionController(IMoneyTransactionService moneyTransactionService)
        {
            _moneyTransactionService = moneyTransactionService;
        }       

        [HttpGet("GetMoneyTransactionDetails")]

        public IActionResult GetMoneyTransactionDetails([FromQuery] MoneyAccountTransactionVm searchValue)
        {
            return Ok(_moneyTransactionService.GetMoneyTransactionDetails(searchValue));
        }
    }
}

