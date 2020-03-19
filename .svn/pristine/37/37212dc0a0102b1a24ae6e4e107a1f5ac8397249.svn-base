using Microsoft.AspNetCore.Mvc;
using ServerAPI.Services;
using ServerAPI.ViewModels;
using System.Threading.Tasks;
using Newtonsoft.Json;
using jsreport.Client;
using jsreport.Types;
using System;
using ServerAPI.Utilities;
using Microsoft.Extensions.Configuration;

namespace ServerAPI.Controllers
{
    [Route("[controller]")]
    public class IntakeCurrencyController : ControllerBase
    {
        private readonly IIntakeCurrencyService _intakeCurrencyService;
        private readonly ICommonService _commonService;
        private readonly IConfiguration _configuration;

        public IntakeCurrencyController(IIntakeCurrencyService intakeCurrencyService, IConfiguration configuration,
            ICommonService commonService)
        {
            _intakeCurrencyService = intakeCurrencyService;
            _configuration = configuration;
            _commonService = commonService;
        }

        [HttpGet("GetIntakeCurrencyViewer")]
        public IActionResult GetIntakeCurrencyViewer(int incarcerationId)
        {
            return Ok(_intakeCurrencyService.GetIntakeCurrencyViewer(incarcerationId));
        }

        [HttpPost("InsertIntakeCurrency")]
        public async Task<IActionResult> InsertIntakeCurrency([FromBody] IntakeCurrencyVm obj)
        {
            return Ok(await _intakeCurrencyService.InsertIntakeCurrency(obj));
        }

        [HttpGet("GetIntakeCurrencyPdfViewer")]
        public async Task<IActionResult> GetIntakeCurrencyPdfViewer(int incarcerationId)
        {
            IntakeCurrencyPdfViewerVm summaryPdfDetails =
                _intakeCurrencyService.GetIntakeCurrencyPdfViewer(incarcerationId);
            string json = JsonConvert.SerializeObject(summaryPdfDetails);
            ReportingService rs = new ReportingService(_configuration.GetSection("SiteVariables")["ReportUrl"]);
            //try catch used for to handle 2 exceptions (10061 & 404) from jsreport.
            try
            {
                _commonService.atimsReportsContentLog(InventoryQueueConstants.INTAKECURRENCY, json);
                Report report = await rs.RenderByNameAsync(InventoryQueueConstants.INTAKECURRENCY, json);
                FileContentResult result =
                    new FileContentResult(_commonService.ConvertStreamToByte(report.Content), "application/pdf");
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