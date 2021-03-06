﻿using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ServerAPI.ViewModels;
using ServerAPI.Services;
using ServerAPI.Authorization;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;
using jsreport.Client;
using jsreport.Types;
using System;

namespace ServerAPI.Controllers
{
    [Route("[controller]")]
    public class GrievanceAppealsController : ControllerBase
    {
        private readonly IGrievanceAppealsService _appealsService;
        private readonly IConfiguration _configuration;
        private readonly ICommonService _commonService;

        public GrievanceAppealsController(IGrievanceAppealsService appealsService,
            IConfiguration configuration, ICommonService commonService)
        {
            _appealsService = appealsService;
            _configuration = configuration;
            _commonService = commonService;
        }

        [HttpGet("GetGrievanceAppealDetails")]
        public IActionResult GetGrievanceAppealDetails(int facilityId, bool showReviewed, bool showDeleted)
        {
            return Ok(_appealsService.GetGrievanceAppealDetails(facilityId, showReviewed, showDeleted));
        }

        [HttpGet("GetGrievanceDetails")]
        public IActionResult GetGrievanceDetails(int? grievanceId)
        {
            return Ok(_appealsService.GetGrievanceDetails(grievanceId));
        }

        [FuncPermission(1162, FuncPermissionType.Add)]
        [HttpPost("InsertGrievanceAppeal")]
        public async Task<IActionResult> InsertGrievanceAppeal([FromBody]GrievanceAppealParam grivAppealParam)
        {
            return Ok(await _appealsService.InsertGrievanceAppeal(grivAppealParam));
        }

        [HttpPut("UpdateGrievanceAppeal")]
        public async Task<IActionResult> UpdateGrievanceAppeal(int grievanceAppealId, [FromBody]GrievanceAppealParam grivAppealParam)
        {
            return Ok(await _appealsService.UpdateGrievanceAppeal(grievanceAppealId, grivAppealParam));
        }

        [HttpDelete("DeleteGrievanceAppeal")]
        public async Task<IActionResult> DeleteGrievanceAppeal(int grievanceAppealId, bool deleteFlag)
        {
            return Ok(await _appealsService.DeleteGrievanceAppeal(grievanceAppealId, deleteFlag));
        }

        [HttpGet("SearchGrievances")]
        public IActionResult SearchGrievances(string grievanceNumber, int inmateId)
        {
            return Ok(_appealsService.SearchGrievances(grievanceNumber, inmateId));
        }

        [HttpGet("CheckReviewFlagAndClear")]
        public IActionResult CheckReviewFlagAndClear(int grievanceId, int facilityId, int grievanceAppealId)
        {
            return Ok(_appealsService.CheckReviewFlagAndClear(grievanceId, facilityId, grievanceAppealId));
        }

        [HttpGet("GetGrievanceAppealReport")]
        public async Task<IActionResult> GetGrievanceAppealReport(int grievanceId, string reportName)
        {
            GrievanceAppealsReport PdfDetails = _appealsService.GetGrievanceAppealsReport(grievanceId);
            PdfDetails.ReportName = reportName;
            string json = JsonConvert.SerializeObject(PdfDetails);
            ReportingService rs = new ReportingService(_configuration.GetSection("SiteVariables")["ReportUrl"]);
            //try catch used for to handle 2 exceptions (10061 & 404) from jsreport.
            try
            {
                _commonService.atimsReportsContentLog(reportName, json);
                Report report = await rs.RenderByNameAsync(reportName, json);
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
        //Newly added for functionality permission
        [FuncPermission(1162,FuncPermissionType.Edit)]
        [HttpGet("EditAppealDetails")]
        public IActionResult EditAppealDetails()
        {
            return Ok();
        }
    }
}
