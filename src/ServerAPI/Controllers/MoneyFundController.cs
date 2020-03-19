using Microsoft.AspNetCore.Mvc;
using ServerAPI.Services;
using ServerAPI.ViewModels;
namespace ServerAPI.Controllers
{
    [Route("[controller]")]
    public class MoneyFundController : ControllerBase
    {
        private readonly IMoneyFundService _moneyFundService;
        public MoneyFundController(IMoneyFundService moneyFundService)
        {
            _moneyFundService = moneyFundService;
        }

        [HttpGet("GetMoneyFundTransactionDetails")]

        public IActionResult GetMoneyFundTransactionDetails(MoneyFund searchValue)
        {
            return Ok(_moneyFundService.GetMoneyFundTransactionDetails(searchValue));
        }
    }
}

