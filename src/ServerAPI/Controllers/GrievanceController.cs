﻿using System;
using System.Threading.Tasks;
using jsreport.Client;
using jsreport.Types;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using ServerAPI.Services;
using ServerAPI.ViewModels;
using ServerAPI.Authorization;
using Newtonsoft.Json;


namespace ServerAPI.Controllers
{
    [Route("[controller]")]
    public class GrievanceController : ControllerBase
    {
        private readonly IGrievanceService _grievanceService;
        private readonly IConfiguration _configuration;
        private readonly ICommonService _commonService;

        public GrievanceController(IGrievanceService grievanceService, ICommonService commonService, IConfiguration configuration)
        {
            _grievanceService = grievanceService;
            _commonService = commonService;
            _configuration = configuration;
        }

        [HttpGet("GetGrievanceCount")]
        public IActionResult GetGrievanceCount([FromQuery] GrievanceCountVm grievanceCountVm)
        {
            return Ok(_grievanceService.GetGrievanceCount(grievanceCountVm));
        }

        [HttpGet("GetGrievanceCountDetails")]
        public IActionResult GetGrievanceCountDetails([FromQuery] GrievanceCountVm grievanceCountVm)
        {
            return Ok(_grievanceService.GetGrievanceCountPopupDetails(grievanceCountVm));
        }

        [HttpPost("InsertGrievance")]
        public async Task<IActionResult> InsertGrievance([FromBody] GrievanceVm grievance)
        {
            return Ok(await _grievanceService.InsertUpdateGrievance(grievance));
        }

        [HttpPut("UpdateGrievance")]
        public async Task<IActionResult> UpdateGrievance([FromBody] GrievanceVm grievance)
        {
            return Ok(await _grievanceService.InsertUpdateGrievance(grievance));
        }

        //For Grievance Active and History pages
        [HttpGet("GetGrievanceDetails")]
        public IActionResult GetGrievanceDetails([FromQuery]GrievanceInputs inputs)
        {
            return Ok(_grievanceService.GetGrievanceDetails(inputs));
        }

        //For Grievance History Search
        [HttpGet("GetGrievanceHistory")]
        public IActionResult GetGrievanceHistory([FromQuery]GrievanceInputs inputs)
        {
            return Ok(_grievanceService.GetGrievanceHistory(inputs));
        }

        [HttpPost("InsertAdditionalInmate")]
        public async Task<IActionResult> InsertAdditionalInmate([FromBody] GrievanceVm grievance)
        {
            return Ok(await _grievanceService.InsertAdditionalInmate(grievance));
        }

        [HttpDelete("DeleteAdditionalInmate")]
        public async Task<IActionResult> DeleteAdditionalInmate([FromQuery] int grievanceInmateId)
        {
            return Ok(await _grievanceService.DeleteAdditionalInmate(grievanceInmateId));
        }

        [HttpPost("InsertAdditionalPersonnel")]
        public async Task<IActionResult> InsertAdditionalPersonnel([FromBody] GrievanceVm grievance)
        {
            return Ok(await _grievanceService.InsertAdditionalPersonnel(grievance));
        }

        [HttpDelete("DeleteAdditionalPersonnel")]
        public async Task<IActionResult> DeleteAdditionalPersonnel([FromQuery] int grievancePersonnelId)
        {
            return Ok(await _grievanceService.DeleteAdditionalPersonnel(grievancePersonnelId));
        }

        [HttpPut("UpdateGrievanceDepartment")]
        public async Task<IActionResult> UpdateGrievanceDepartment([FromBody] GrievanceVm grievance)
        {
            return Ok(await _grievanceService.UpdateGrievanceDepartment(grievance));
        }

        [FuncPermission(1160, FuncPermissionType.Edit)]
        [HttpPut("UpdateGrievanceReview")]
        public async Task<IActionResult> UpdateGrievanceReview([FromBody]GrievanceReview review)
        {
            return Ok(await _grievanceService.UpdateGrievanceReview(review));
        }

        [HttpGet("GetAdditionalDetails")]
        public IActionResult GetAdditionalDetails(int grievanceId)
        {
            return Ok(_grievanceService.GetAdditionalDetails(grievanceId));
        }

        [HttpGet("GetGrievanceForms")]
        public IActionResult GetGrievanceForms(int grievanceId)
        {
            return Ok(_grievanceService.GetGrievanceForms(grievanceId));

        }

        [HttpGet("GetHousingDetails")]
        public IActionResult GetHousingDetails(int facilityId)
        {
            return Ok(_grievanceService.GetHousingDetails(facilityId));
        }

        
        [HttpPut("UpdateGrvSetReview")]
        public async Task<IActionResult> UpdateGrvSetReview([FromBody]GrievanceVm grvcDetail)
        {
            return Ok(await _grievanceService.UpdateGrvSetReview(grvcDetail));
        }

        [HttpGet("LoadGrievanceAttachments")]
        public IActionResult LoadGrievanceAttachments(int grievanceId)
        {
            return Ok(_grievanceService.LoadGrievanceAttachments(grievanceId));
        }
        [HttpPut("DeleteUndoByGrievanceId")]
        public async Task<IActionResult> DeleteByGrievanceId([FromBody]GrievanceVm grvcDetail)
        {
            return Ok(await _grievanceService.DeleteUndoByGrievanceId(grvcDetail));
        }

        // [HttpPut("DeleteGrievance")]
        // public async Task<IActionResult> DeleteGrievance(int grievanceId, string disposition, string dispositionNote)
        // {
        //     return Ok(await _grievanceService.DeleteGrievance(grievanceId, disposition, dispositionNote));
        // }
        [HttpPut("DeleteGrievance")]
        public async Task<IActionResult> DeleteGrievance([FromBody]CancelGrievance CancelGrievance)
        {
            return Ok(await _grievanceService.DeleteGrievance(CancelGrievance));
        }

  
        [HttpGet("GetGrievanceReport")]
        public async Task<IActionResult> GetGrievanceReport(int grievanceId, string reportName)
        {
            GrievanceReport grievancePdfDetails = _grievanceService.GetGrievanceReport(grievanceId);
            grievancePdfDetails.ReportName = reportName;
            string json = JsonConvert.SerializeObject(grievancePdfDetails);
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

       //Create the Grievance Sensitive Material
        [HttpPost("AddGrivanceSensitiveMaterial")]  
        [FuncPermission(1164 , FuncPermissionType.Add)]              
        public IActionResult AddGrivanceSensitiveMaterial()
        {
            return Ok();
        }
        [FuncPermission(510,FuncPermissionType.Edit)]
        [HttpGet("EditGrievance")]
        public IActionResult EditGrievance()
        {
            return Ok();
        }
        [FuncPermission(510,FuncPermissionType.Delete)]
        [HttpDelete("DeleteGrievances")]
        public IActionResult DeleteGrievances()
        {
            return Ok();
        }
         [FuncPermission(510,FuncPermissionType.Access)]
        [HttpPut("AccessGrievance")]
        public IActionResult AccessGrievance()
        {
            return Ok();
        }
        [FuncPermission(1160,FuncPermissionType.Edit)]
        [HttpPut("EditGrievanceReview")]
        public IActionResult EditGrievanceReview()
        {
            return Ok();
        }

        //Modify the Grievance Sensitive Material
        [HttpPut ("EditGrivanceSensitiveMaterial")]  
        [FuncPermission(1164, FuncPermissionType.Edit)]              
        public IActionResult EditGrivanceSensitiveMaterial()
        {
            return Ok();
        }
        
        //View the Grievance Sensitive Material
        [HttpPut("AccessGrievanceSensitiveMaterial")]
        [FuncPermission(1164, FuncPermissionType.Access)]
        public IActionResult AccessGrievanceSensitiveMaterial()
        {
            return Ok();
        }

        //Delete the Grievance Sensitive Material
        [HttpDelete("DeleteGrievanceSensitiveMaterial")]
        [FuncPermission(1164, FuncPermissionType.Delete)]
        public IActionResult DeleteGrievanceSensitiveMaterial()
        {
            return Ok();
        }
        
        //Print of Grievance Sensitive Material
        [FuncPermission(1164, FuncPermissionType.Print)]
        [HttpPost("PrintGrievanceSensitiveMaterial")]
        public IActionResult PrintGrievanceSensitiveMaterial()
        {
            return Ok();
        }

        [HttpGet("GetGrievanceById")]
        public IActionResult GetGrievanceById(int grievanceId)
        {
            return Ok(_grievanceService.GetGrievance(grievanceId));
        }
        [FuncPermission(510,FuncPermissionType.Add)]
        [HttpPut("AddGrievance")]
        public IActionResult AddGrievance()
        {
            return Ok();
        }
    }
}