﻿using ServerAPI.ViewModels;
using Microsoft.AspNetCore.Mvc;
using ServerAPI.Services;
using ServerAPI.Authorization;
using Newtonsoft.Json;
using jsreport.Client;
using Microsoft.Extensions.Configuration;
using jsreport.Types;
using System;
using System.Threading.Tasks;
//using ServerAPI.Utilities;


namespace ServerAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class MoneyController : ControllerBase
    {
        private readonly IMoneyService _moneyService;
        private readonly IMoneyVoidService _moneyVoidService;
        private readonly IMoneyVoidCashDrawerService _moneyVoidCashDrawerService;
        private readonly IConfiguration _configuration;
        private readonly ICommonService _commonService;

        public MoneyController(IMoneyService moneyService,
         IMoneyVoidService moneyVoidService, IMoneyVoidCashDrawerService moneyVoidCashDrawerService,
         IConfiguration configuration, ICommonService commonService)
        {
            _moneyService = moneyService;
            _moneyVoidService = moneyVoidService;
            _moneyVoidCashDrawerService = moneyVoidCashDrawerService;
            _commonService = commonService;
            _configuration = configuration;
        }

        [HttpGet("GetBank")]
        public IActionResult MoneyGetBank() => Ok(_moneyService.MoneyGetBank());
        [HttpGet("GetFund")]
        public IActionResult MoneyGetFund() => Ok(_moneyService.MoneyGetFund());
        [HttpGet("GetDepository")]
        public IActionResult MoneyGetDeposit() => Ok(_moneyService.MoneyGetDeposit());
        [HttpGet("GetCashDrawer")]
        public IActionResult MoneyGetCashDrawer(int facilityId, bool addVault) => 
            Ok(_moneyService.MoneyGetCashDrawer(facilityId, addVault));
        #region Money->Account
        [HttpGet("GetTransactionDetails")]
        public IActionResult GetTransactionDetails([FromQuery]MoneyAccountFilterVm searchValue) => 
            Ok(_moneyService.GetMoneyAccountTransaction(searchValue));
        #endregion
        [HttpGet("GetTransactionDetailsById")]
        public IActionResult GetTransactionDetailsById(int transId, int inmateId, int bankId, MoneyAccountTransactionFlagType pFlag) => 
            Ok(_moneyService.GetMoneyTransactionDetails(transId, inmateId, bankId, pFlag));
        [HttpGet("GetDoVoidDetails")]
        public IActionResult GetDoVoidDetails(int transId, MoneyAccountTransactionFlagType pFlag) => 
            Ok(_moneyVoidService.GetDoVoidDetails(transId, pFlag));
        [HttpPost("InsertDoVoid")]
        public IActionResult InsertDoVoid(SetVoidVm voidDetails) => Ok(_moneyVoidService.InsertDoVoid(voidDetails));

        #region Set Void for Cash Drawer or Vault Void  
        [HttpGet("GetCashDrawerVoidDetails")]
        public IActionResult GetCashDrawerVoidDetails(int transId, bool voidFlag, MoneyAccountTransactionFlagType pFlag) =>
            Ok(_moneyVoidCashDrawerService.GetCashDrawerVoidDetails(transId, voidFlag, pFlag));
        [HttpPost("DoVoidCashDrawerAndVault")]
        public async Task<IActionResult> DoVoidCashDrawerAndVault([FromBody]SetVoidVm setVoid) => 
            Ok(await _moneyVoidCashDrawerService.DoVoidCashDrawerAndVault(setVoid));
        #endregion

        #region Show Detail(Vault or CashDrawer)
        [HttpGet("GetCashDrawerTransactionDetails")]
        public IActionResult GetCashDrawerTransactionDetails(int transId) => 
            Ok(_moneyVoidCashDrawerService.GetCashDrawerTransactionDetails(transId));

        #endregion

        [FuncPermission(5009, FuncPermissionType.Add)]
        [HttpPost("AddFinalReturnCash")]
        public IActionResult AddFinalReturnCash() => Ok();
        [FuncPermission(5010, FuncPermissionType.Add)]
        [HttpPost("AddReceiveCash")]
        public IActionResult AddReceiveCash() => Ok();
        [FuncPermission(5011, FuncPermissionType.Add)]
        [HttpPost("AddReceiveCheck")]
        public IActionResult AddReceiveCheck() => Ok();
        [FuncPermission(5012, FuncPermissionType.Add)]
        [HttpPost("AddWriteCheck")]
        public IActionResult AddWriteCheck() => Ok();
        [FuncPermission(5013, FuncPermissionType.Add)]
        [HttpPost("AddFinalWriteCheck")]
        public IActionResult AddFinalWriteCheck() => Ok();
        [FuncPermission(5014, FuncPermissionType.Add)]
        [HttpPost("AddReturnCash")]
        public IActionResult AddReturnCash() => Ok();
        [FuncPermission(5017, FuncPermissionType.Add)]
        [HttpPost("AddPurchase")]
        public IActionResult AddPurchase() => Ok();
        [FuncPermission(5018, FuncPermissionType.Add)]
        [HttpPost("AddApplyFee")]
        public IActionResult AddApplyFee() => Ok();
        [FuncPermission(5019, FuncPermissionType.Add)]
        [HttpPost("AddDonate")]
        public IActionResult AddDonate() => Ok();
        [FuncPermission(5200, FuncPermissionType.Add)]
        [HttpPost("AddRefund")]
        public IActionResult AddRefund() => Ok();
        [FuncPermission(5201, FuncPermissionType.Add)]
        [HttpPost("AddJournal")]
        public IActionResult AddJournal() => Ok();

        [HttpGet("GetBankDetails")]
        public IActionResult GetBankDetails() => Ok(_moneyService.GetBankDetails());

        [HttpGet("GetAccountAoInmate")]
        public IActionResult GetAccountAoInmate(int inmateId) => Ok(_moneyService.GetAccountAoInmate(inmateId));
        [HttpPost("GetReceiptReport")]
        public async Task<IActionResult> GetReceiptReport([FromBody]MoneyCashCheckVm reportData)
        {
            string json = JsonConvert.SerializeObject(reportData);
            ReportingService rs = new ReportingService(_configuration.GetSection("SiteVariables")["ReportUrl"]);
            //try catch used for to handle 2 exceptions (10061 & 404) from jsreport.
            try
            {
                _commonService.atimsReportsContentLog(reportData.ReportName, json);
                Report report = await rs.RenderByNameAsync(reportData.ReportName, json);
                FileContentResult result = new FileContentResult(_commonService.ConvertStreamToByte(report.Content),
                    "application/pdf");
                return result;
            }

            catch (Exception ex)
            {
                //10061 is status code for no connection could be made because the target machine actively refused it
                //404 for file not found in jsreport
                return ex.InnerException != null ? Ok(10061) : Ok(404);
            }

        }
    }
}