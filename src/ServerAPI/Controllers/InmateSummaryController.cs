﻿using jsreport.Client;
using jsreport.Types;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ServerAPI.Authorization;
using ServerAPI.Services;
using ServerAPI.ViewModels;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace ServerAPI.Controllers
{
    [Route("[controller]")]
    public class InmateSummaryController : ControllerBase
    {
        private readonly IInmateSummaryService _iiInmateSummaryService;
        private readonly IInmateSummaryPdfService _iInmateSummaryPdfService;
        private readonly ICommonService _commonService;
        private readonly IConfiguration _configuration;

        public InmateSummaryController(IInmateSummaryService iinmateSummaryService, IInmateSummaryPdfService iInmateSummaryPdfService,
            ICommonService commonService, IConfiguration configuration)
        {
            _iiInmateSummaryService = iinmateSummaryService;
            _iInmateSummaryPdfService = iInmateSummaryPdfService;
            _commonService = commonService;
            _configuration = configuration;
        }

        // To get the inmate Summary details of inmate by inmateId
        [HttpGet("GetInmateSummary")]
        public IActionResult GetInmateSummary([FromQuery]InmateSummaryVm isvm)
        {
            return Ok(_iiInmateSummaryService.GetInmateSummaryDetails(isvm));
        }

        // Get Inmate Summary PDF details based on Summary Type
        [HttpGet("GetInmateSummaryPdf")]
        public async Task<IActionResult> GetInmateSummaryPdf(int inmateId, InmateSummaryType summaryType, int? incarcerationId)
        {
            InmateSummaryPdfVm summaryPdfDetails = _iInmateSummaryPdfService.GetInmateSummaryPdf(inmateId, summaryType, incarcerationId);

            //Get Enum Displayname using 'DisplayAttribute'
            summaryPdfDetails.InmatePdfHeaderDetails.SummaryHeader = summaryType.GetType().GetMember(summaryType.ToString()).First()
                   .GetCustomAttribute<DisplayAttribute>()?.Name;
            JObject customLabel = _commonService.GetCustomMapping();
            summaryPdfDetails.CustomLabel = customLabel;
            string json = JsonConvert.SerializeObject(summaryPdfDetails);
            ReportingService rs = new ReportingService(_configuration.GetSection("SiteVariables")["ReportUrl"]);
            //try catch used for to handle 2 exceptions (10061 & 404) from jsreport.
            try
            {
                _commonService.atimsReportsContentLog(summaryPdfDetails.InmatePdfHeaderDetails.SummaryHeader, json);
                Report report = await rs.RenderByNameAsync(summaryPdfDetails.InmatePdfHeaderDetails.SummaryHeader, json);
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

        //Access to File Summary Module
        [HttpGet("AccessInmateSummary")]
        public IActionResult AccessInmateSummary()
        {
            return NoContent();
        }

        //Print of Active Inmate Summary
        [FuncPermission(631, FuncPermissionType.Access)]
        [HttpGet("PrintActiveInmateSummary")]
        public IActionResult AccessActiveInmateSummary()
        {
            return NoContent();
        }

        //Print of Active Inmate Fingerprint
        [FuncPermission(632, FuncPermissionType.Print)]
        [HttpGet("PrintInmateFingerprint")]
        public IActionResult PrintInmateFingerprint()
        {
            return NoContent();
        }
        //Print of Inmate Summary
        [FuncPermission(633, FuncPermissionType.Print)]
        [HttpGet("PrintInmateSummary")]
        public IActionResult PrintInmateSummary()
        {
            return NoContent();
        }
        //Print of Active Inmate Public Summary
        [FuncPermission(634, FuncPermissionType.Access)]
        [HttpGet("PrintInmatePublicSummary")]
        public IActionResult PrintInmatePublicSummary()
        {
            return NoContent();
        }
        //Print of Active Bail Summary
        [FuncPermission(635, FuncPermissionType.Print)]
        [HttpGet("PrintBailSummary")]
        public IActionResult PrintBailSummary()
        {
            return NoContent();
        }
        [HttpGet("GetBookComplete")]
        public Boolean GetBookComplete(int inmateID)
        {
            return _iInmateSummaryPdfService.GetBookComplete(inmateID);
        }
    }
}
