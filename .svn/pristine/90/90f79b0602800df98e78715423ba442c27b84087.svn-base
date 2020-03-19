using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using jsreport.Client;
using jsreport.Types;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ServerAPI.Authorization;
using ServerAPI.Services;
using ServerAPI.ViewModels;

namespace ServerAPI.Controllers
{
    [Route("[controller]")]
    public class IncidentActiveController : ControllerBase
    {
        private readonly IIncidentActiveService _incidentActiveService;
        private readonly IIncidentWizardService _incidentWizardService;
        private readonly IConfiguration _configuration;
        private readonly ICommonService _commonService;

        public IncidentActiveController(IIncidentActiveService incidentActiveService,
            IIncidentWizardService incidentWizardService, IConfiguration configuration,
            ICommonService commonService)
        {
            _incidentActiveService = incidentActiveService;
            _incidentWizardService = incidentWizardService;
            _configuration = configuration;
            _commonService = commonService;
        }

        // Get active incident grid details.
        [HttpGet("GetIncidentViewerDetails")]
        public IActionResult GetIncidentViewerDetails([FromQuery] IncidentFilters incidentFilters)
        {
            return Ok(_incidentActiveService.LoadActiveIncidentViewerDetails(incidentFilters));
        }

        // Get incident details.
        [HttpGet("GetIncidentDetails")]
        public IActionResult GetIncidentDetails(int incidentId, int dispInmateId, bool deleteFlag = false)
        {
            return Ok(_incidentActiveService.LoadIncidentDetails(incidentId, dispInmateId, deleteFlag));
        }

        [HttpGet("GetIncidentBasicDetails")]
        public IActionResult GetIncidentBasicDetails(int incidentId)
        {
            return Ok(_incidentActiveService.GetIncidentBasicDetails(incidentId));
        }

        // Insert or Edit narrative information.
        [HttpPost("InsertNarrative")]
        // [FuncPermission(1128, FuncPermissionType.Add)]        
        public async Task<IActionResult> InsertNarrative([FromBody] IncidentNarrativeDetailVm narrative)
        {
            return Ok(await _incidentActiveService.InsertOrUpdateNarrativeInfo(narrative));
        }

        // Insert or Edit narrative information.
        // [FuncPermission(1128, FuncPermissionType.Edit)]
        [HttpPut("UpdateNarrative")]
        public async Task<IActionResult> UpdateNarrative([FromBody] IncidentNarrativeDetailVm narrative)
        {
            return Ok(await _incidentActiveService.InsertOrUpdateNarrativeInfo(narrative));
        }

        // Update incident active status.
        [HttpPost("UpdateIncidentActiveStatus")]
        public async Task<IActionResult> UpdateIncidentActiveStatus(int incidentId, bool status)
        {
            return Ok(await _incidentActiveService.UpdateIncidentActiveStatus(incidentId, status));
        }
        //For Permission added
        [FuncPermission(1126,FuncPermissionType.Add)]
        [HttpPost("AddPrimaryNarrative")]
        public IActionResult AddPrimaryNarrative()
        {
            return Ok();
        }

        // Get incident active status.
        [HttpGet("GetIncidentActiveStatus")]
        public IActionResult GetIncidentActiveStatus(int incidentId)
        {
            return Ok(_incidentActiveService.GetIncidentActiveStatus(incidentId));
        }

        // Delete narrative information.
        [FuncPermission(1128, FuncPermissionType.Delete)]
        [HttpDelete("DeleteNarrativeInfo")]
        public async Task<IActionResult> DeleteNarrativeInfo(int narrativeId)
        {
            return Ok(await _incidentActiveService.DeleteNarrativeInfo(narrativeId));
        }

        // Get Forms based on form category
        [HttpGet("GetForms")]
        public IActionResult GetForms(string categoryName)
        {
            return Ok(_incidentActiveService.LoadForms(categoryName));
        }

        // Insert form record details based on incident or grievance.
        [HttpPost("InsertFormRecord")]
        public async Task<IActionResult> InsertFormRecord([FromQuery] int formTemplateId, int grievanceOrIncidentId,
            bool isIncident, int inmateId = 0)
        {
            return Ok(await _incidentActiveService.InsertFormRecord(formTemplateId, grievanceOrIncidentId, isIncident,
                inmateId));
        }

        // Get disciplinary narrative details based on incident id.
        [HttpGet("LoadDisciplinaryNarrativeDetails")]
        public IActionResult LoadDisciplinaryNarrativeDetails(int incidentId)
        {
            return Ok(_incidentActiveService.LoadDisciplinaryNarrativeDetails(incidentId));
        }

        [HttpGet("GetIncidentHistoryDetails")]
        public IActionResult GetIncidentHistoryDetails([FromQuery] IncidentFilters historyInput)
        {
            return Ok(_incidentActiveService.GetIncidentHistoryDetails(historyInput));
        }

        // Get Attachment details based on incident id.
        [HttpGet("GetIncidentAttachments")]
        public IActionResult GetIncidentAttachments(int incidentId, bool deleteFlag = false)
        {
            return Ok(_incidentActiveService.LoadIncidentAttachments(incidentId, deleteFlag));
        }

        // Get photos based on incident id.
        [HttpGet("GetIncidentPhotos")]
        public IActionResult GetIncidentPhotos(int incidentId, bool deleteFlag = false)
        {
            return Ok(_incidentActiveService.LoadIncidentPhotos(incidentId, deleteFlag));
        }

        // Get forms details based on incident id.
        [HttpGet("GetIncidentForms")]
        public IActionResult GetIncidentForms(int incidentId)
        {
            return Ok(_incidentActiveService.LoadIncidentForms(incidentId));
        }

        // Get Involved party details based on incident id.
        [HttpGet("GetInvolvedPartyDetails")]
        public IActionResult GetInvolvedPartyDetails(int incidentId, int dispInmateId = 0, bool isWizardSteps = false)
        {
            return Ok(_incidentActiveService.LoadInvolvedPartyDetails(incidentId, dispInmateId, isWizardSteps));
        }

        // Delete involved party details.
        [FuncPermission(1132, FuncPermissionType.Delete)]
        [HttpDelete("DeleteInvolvedParty")]
        public async Task<IActionResult> DeleteInvolvedParty(int disciplinaryInmateId)
        {
            return Ok(await _incidentActiveService.DeleteInvolvedPartyDetails(disciplinaryInmateId));
        }

        [HttpGet("GetIncidentReport")]
        public async Task<IActionResult> GetIncidentReport(int incidentId, int inmateId, int dispInmateId, int reportType, string reportName)
        {
            IncidentReport incidentReport = _incidentActiveService
                .GetIncidentReportDetails(incidentId, inmateId, dispInmateId, reportType);
            incidentReport.ReportName = reportName;
            JObject customLabel= _commonService.GetCustomMapping();
            incidentReport.CustomLabel = customLabel;
            foreach (IncidentViewer obj in incidentReport.InvolvedPartyDetails)
            {
                obj.ViolationDetails = incidentReport.ViolationDetails
                    .Where(a => a.DispInmateId == obj.DisciplinaryInmateId).ToList();
            }
            string json = JsonConvert.SerializeObject(incidentReport);
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

        [HttpGet("GetRecommendedSanction")]
        public IActionResult GetRecommendedSanction(int dispInmateId, int incidentId = 0)
        {
            return Ok(_incidentActiveService.GetViolationDetails(incidentId, new[] { dispInmateId }, true));
        }

        [HttpPost("DeleteUndoInmateForm")]
        public async Task<IActionResult> DeleteUndoInmateForm([FromBody] KeyValuePair<int, bool> inmateFormDetail)
        {
            return Ok(await _incidentActiveService.DeleteUndoInmateForm(inmateFormDetail));
        }

        [HttpGet("GetInmateFormDetails")]
        public IActionResult GetInmateFormDetails(int dispInmateId)
        {
            return Ok(_incidentActiveService.LoadInmateFormDetails(dispInmateId));
        }

        [HttpPost("AddNewAttachment")]
        
        public IActionResult AddNewAttachment()
        {
            return Ok(); //Here we are returning empty (200 ok) response,
            //Because attachment screen is common screen for almost every module.
            //Http actions for attachment screen has been written at PrebookWizard controller.
            //Hence we are not able to check access rights for all module at the same action.
            //Because every module has the unique functionality id,
            //That's why we are creating duplicate http actions,
            //To check permission access for each and every module for attachment screen.
        }

        [HttpPut("UpdateAttachment")]
        [FuncPermission(1126, FuncPermissionType.Edit)]
        public IActionResult UpdateAttachment()
        {
            return Ok();
        }
//As per update functionality permission ,delete atachment not exist
        // [HttpDelete("DeleteAttachment")]
        
        // public IActionResult DeleteAttachment()
        // {
        //     return Ok();
        // }

        [HttpGet("OpenAttachment")]
        
        public IActionResult OpenAttachment()
        {
            return Ok();
        }

        [HttpPost("AddInvolvedParty")]
        
        public async Task<IActionResult> AddInvolvedParty([FromBody] ClassifyInvPartyDetailsVm invParty)
        {
            return Ok(await _incidentWizardService.InsertUpdateInvolvedParty(invParty));
        }
        
        [HttpPut("EditIncidentSensitiveOrPrea")]  
        [FuncPermission(1120, FuncPermissionType.Edit)]              
        public IActionResult EditIncidentSensitiveOrPrea()
        {
            return Ok();
        }

        [HttpPut("AccessIncidentSensitiveMaterial")]
        [FuncPermission(1121, FuncPermissionType.Access)]
        public IActionResult AccessIncidentSensitiveMaterial()
        {
            return Ok();
        }

        [FuncPermission(1121, FuncPermissionType.Edit)]   
        [HttpPut("EditIncidentSensitiveMaterial")]
             
        public IActionResult EditIncidentSensitiveMaterial()
        {
            return Ok();
        }
        [FuncPermission(1121, FuncPermissionType.Add)]   
        [HttpPut("AddIncidentSensitiveMaterial")]
             
        public IActionResult AddIncidentSensitiveMaterial()
        {
            return Ok();
        }
        
        [HttpPut("AccessIncidentPREAMaterial")]
        [FuncPermission(1122, FuncPermissionType.Access)]
        public IActionResult AccessIncidentPREAMaterial()
        {
            return Ok();
        }

        [HttpPut("EditIncidentPREAMaterial")]
        [FuncPermission(1122, FuncPermissionType.Edit)]        
        public IActionResult EditIncidentPREAMaterial()
        {
            return Ok();
        }

        [HttpPut("EditIncidentMaterial")]
        [FuncPermission(1126, FuncPermissionType.Edit)]        
        public IActionResult EditIncidentMaterial()
        {
            return Ok();
        }

        [HttpPut("ViewIncident")]
        [FuncPermission(1141,FuncPermissionType.Access)]
        public IActionResult ViewIncident()
        {
            return Ok();
        }

        [HttpGet("GetIncidentLocations")]
        public IActionResult GetIncidentLocations(int facilityId)
        {
            return Ok(_incidentActiveService.GetIncidnetLocations(facilityId));
        }

        [HttpGet("GetIncidentOtherLocations")]
        public IActionResult GetIncidentOtherLocations(int facilityId)
        {
            return Ok(_incidentActiveService.GetIncidentOtherLocations(facilityId));
        }

        //Newly Added For Permission
        [FuncPermission(1149,FuncPermissionType.Edit)]
        [HttpPut("EditIncidentNotice")]
        public IActionResult EditIncidentNotice()
        {
            return Ok();
        }

        [FuncPermission(740,FuncPermissionType.Edit)]
        [HttpPut("EditIncidentSupervisor")]
        public IActionResult EditIncidentSupervisor()
        {
            return Ok();
        }

        [HttpGet("GetOverAllGtandDiscDays")]
        public IActionResult GetOverAllGtandDiscDays(int inmateId, DateTime incidentDate)
        {
            return Ok(_incidentActiveService.GetOverAllGtandDiscDays(inmateId, incidentDate));
        }
        [FuncPermission(1122,FuncPermissionType.Add)]
        [HttpGet("AddPreaIncident")]
        public IActionResult AddPreaIncident()
        {
            return Ok();
        }
        [FuncPermission(1128,FuncPermissionType.Add)]
        [HttpGet("AddAdditionalNarrative")]
        public IActionResult AddAdditionalNarrative()
        {
            return Ok();
        }
        [FuncPermission(1128,FuncPermissionType.Edit)]
        [HttpGet("EditAdditionalNarrative")]
        public IActionResult EditAdditionalNarrative()
        {
            return Ok();
        }
    }
}