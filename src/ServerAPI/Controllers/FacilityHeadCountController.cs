﻿using Microsoft.AspNetCore.Mvc;
using ServerAPI.Services;
using ServerAPI.ViewModels;
using System.Threading.Tasks;
using ServerAPI.Authorization;
using System;
using Newtonsoft.Json;
using jsreport.Client;
using jsreport.Types;
using Microsoft.Extensions.Configuration;
using System.Linq;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace ServerAPI.Controllers
{
    [Route("[controller]")]
    public class FacilityHeadCountController : ControllerBase
    {
        private readonly IFacilityHeadCountService _facilityHeadCountService;
        private readonly ICommonService _commonService;
        private readonly IConfiguration _configuration;

        public FacilityHeadCountController(IFacilityHeadCountService facilityHeadCountService, IConfiguration configuration, ICommonService commonService)
        {
            _facilityHeadCountService = facilityHeadCountService;
            _commonService = commonService;
            _configuration = configuration;

        }

        [HttpGet("GetHeadCountMaster")]
        public IActionResult GetHeadCountMaster([FromQuery] HeadCountFilter headCountFilter)
        {
            return Ok(_facilityHeadCountService.LoadHeadCountMaster(headCountFilter));
        }

        [HttpGet("GetCellHeadCountReconcile")]
        public IActionResult GetCellHeadCountReconcile(int facilityId, int cellLogHeadcountId)
        {
            return Ok(_facilityHeadCountService.LoadHeadCountReconcile(facilityId, cellLogHeadcountId));
        }     
        
        [HttpGet("GetCellHeadCountMaster")]
        public IActionResult GetCellHeadCountMaster(int facilityId, int cellLogId, int housingUnitListId = 0,
            int cellLogLocationId = 0)
        {
            return Ok(_facilityHeadCountService.LoadCellHeadCountMaster(facilityId, cellLogId, housingUnitListId,
                cellLogLocationId));
        }

        [FuncPermission(1030, FuncPermissionType.Edit)]
        [HttpGet("GetConsoleHeadCountMaster")]
        public IActionResult GetConsoleHeadCountMaster(int facilityId, int cellLogId, int housingUnitListId = 0,
            int cellLogLocationId = 0)
        {
            return Ok(_facilityHeadCountService.LoadCellHeadCountMaster(facilityId, cellLogId, housingUnitListId,
                cellLogLocationId));
        }

        [HttpPost("InsertOrUpdateCellLog")]
        public async Task<IActionResult> InsertOrUpdateCellLog([FromBody] CellHeadCount cellHeadCount)
        {
            return Ok(await _facilityHeadCountService.InsertOrUpdateCellLog(cellHeadCount));
        }

        [HttpGet("GetHeadCountSaveHistories")]
        public IActionResult GetHeadCountSaveHistories(int cellLogId)
        {
            return Ok(_facilityHeadCountService.LoadHeadCountSaveHistories(cellLogId));
        }

        [HttpGet("GetActiveCellHeadCountString")]
        public IActionResult GetActiveCellHeadCountString(int facilityId, int housingUnitListId = 0,
            int cellLogLocationId = 0, int housingGroupId = 0, int cellLogId = 0)
        {
            return Ok(_facilityHeadCountService.GetActiveCellHeadCountString(facilityId, housingUnitListId,
                cellLogLocationId, housingGroupId, cellLogId));
        }

        [FuncPermission(1032,FuncPermissionType.Edit)]
        [HttpGet("GetHeadCountViewDetail")]
        public IActionResult GetHeadCountViewDetail(int cellLogHeadCountId)
        {
            return Ok(_facilityHeadCountService.LoadHeadCountViewDetail(cellLogHeadCountId));
        }

        [FuncPermission(1032,FuncPermissionType.Edit)]
        [HttpPost("UpdateHeadCountClear")]
        public async Task<IActionResult> UpdateHeadCountClear([FromBody] HeadCountViewDetail headCountViewDetail)
        {
            return Ok(await _facilityHeadCountService.UpdateHeadCountClear(headCountViewDetail));
        }

        [FuncPermission(1031,FuncPermissionType.Add)]
        [HttpPost("InsertCellLogHeadCount")]
        public async Task<IActionResult> InsertCellLogHeadCount([FromBody] HeadCountStart headCountStart)
        {
            return Ok(await _facilityHeadCountService.InsertCellLogHeadCount(headCountStart));
        }

        [HttpGet("GetHeadCountHistories")]
        public IActionResult GetHeadCountHistories([FromQuery] HeadCountHistoryDetails headCountHistoryDetails)
        {
            return Ok(_facilityHeadCountService.LoadHeadCountHistory(headCountHistoryDetails));
        }

        [HttpGet("GetLocationList")]
        public IActionResult GetLocationList(int facilityId)
        {
            return Ok(_facilityHeadCountService.GetlocationList(facilityId));
        }

        [HttpGet("GetAdminHousingLocationCounts")]
        public IActionResult GetAdminHousingLocationCounts(int facilityId)
        {
            return Ok(_facilityHeadCountService.LoadAdminHousingLocationCount(facilityId));
        }

        [HttpPost("GetHeadCountReport")]
        public async Task<IActionResult> GetHeadCountReportAsync([FromBody] HeadCountFilter headCountFilter, string reportName)
        {
            HeadCountReport headCountReportData = _facilityHeadCountService.LoadHeadCountReport(headCountFilter);
            //Get Enum Displayname using 'DisplayAttribute'
            string json = JsonConvert.SerializeObject(headCountReportData);
            ReportingService rs = new ReportingService(_configuration.GetSection("SiteVariables")["ReportUrl"]);
            //try catch used for to handle 2 exceptions (10061 & 404) from jsreport.
            try
            {
                _commonService.atimsReportsContentLog(reportName, json);
                Report report = await rs.RenderByNameAsync(reportName, json);
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