using jsreport.Client;
using jsreport.Types;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using ServerAPI.Services;
using ServerAPI.ViewModels;
using System;
using System.Threading.Tasks;
using GenerateTables.Models;
using Newtonsoft.Json;
using ServerAPI.Authorization;

namespace ServerAPI.Controllers
{
    [Route("[controller]")]
    public class FormsController : ControllerBase
    {

        private readonly IFormsService _formsService;
        private readonly IConfiguration _configuration;
        private readonly ICommonService _commonService;

        public FormsController(IFormsService formsService, IConfiguration configuration, ICommonService commonService)
        {
            _formsService = formsService;
            _configuration = configuration;
            _commonService = commonService;
        }

        [HttpPost("GetForm")]
        public IActionResult GetForm([FromBody] FormDetail details)
        {
            return Ok(_formsService.GetFormDetails(details));
        }
        
        [HttpPost("SaveForm")]
        public async Task<IActionResult> SaveForm([FromBody] FormSaveData value)
        {
            return Ok(await _formsService.SaveForm(value));
        }
        
        [HttpPost("InsertSaveForm")]
        public async Task<IActionResult> InsertSaveForm([FromBody] FormSaveData value)
        {
            return Ok(await _formsService.SaveForm(value));
        }
        
        [HttpPost("UpdateSaveForm")]
        public async Task<IActionResult> UpdateSaveForm([FromBody] FormSaveData value)
        {
            return Ok(await _formsService.SaveForm(value));
        }

        [HttpGet("GetFormHistory")]
        public IActionResult GetFormHistory(int formRecordId)
        {
            return Ok(_formsService.GetFormHistory(formRecordId));
        }

        [HttpGet("GetFormSavedXML")]
        public IActionResult GetFormSavedXml(int formSaveHistoryId)
        {
            return Ok(_formsService.GetSaveHistoryXml(formSaveHistoryId));
        }

        [HttpGet("GetSignature")]
        public IActionResult GetSignature(int formRecordId, int formTemplateId)
        {
            return Ok(_formsService.GetSignature(formRecordId, formTemplateId));
        }

        [HttpPost("SaveSignature")]
        public async Task<IActionResult> SaveSignature([FromBody] Signature signDetails)
        {
            return Ok(await _formsService.SaveSignature(signDetails));
        }
        // [HttpPost("SaveFormTemplateXref")]
        // public async Task<IActionResult> SaveFormTemplateXref([FromBody] Form form)
        // {
        //     return Ok(await _formsService.InsertBookMarkXref(form.FormTemplateId, form.Html));
        // }

        [HttpGet("GetFormTemplateDetails")]
        public IActionResult GetFormTemplateDetails(int categoryId, string filterName)
        {
            return Ok(_formsService.GetFormTemplateDetails(categoryId, filterName));
        }

        [HttpGet("GetFormTemplates")]
        public IActionResult GetFormTemplates(int categoryId)
        {
            return Ok(_formsService.GetFormTemplates(categoryId));
        }

        [HttpPost("GetFormsPDF")]
        public async Task<IActionResult> GetFormsPDF([FromBody] PdfData pdfData)
        {
            ReportingService rs = new ReportingService(_configuration.GetSection("SiteVariables")["ReportUrl"]);
            try
            {
                _commonService.atimsReportsContentLog(pdfData.FormName, JsonConvert.SerializeObject(pdfData.JsonData));
                Report report = await rs.RenderByNameAsync(pdfData.FormName, pdfData.JsonData);
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

        [FuncPermission(6040, FuncPermissionType.Access)]
        [HttpGet("GetInmateMedicalForms")]
        public IActionResult GetInmateForms(int inmateId)
        {
            return Ok(_formsService.GetInmateMedicalForms(inmateId));
        }

        [HttpGet("GetLastIncarcerationFormRecord")]
        public IActionResult GetLastIncarcerationFormRecord(string inmateNumber, int formTemplateId)
        {
            return Ok(_formsService.GetLastIncarcerationFormRecord(inmateNumber, formTemplateId));
        }

        [HttpGet("GetFormRecordByRms")]
        public IActionResult GetFormRecordByRms(int inmatePrebookStagingId)
        {
            return Ok(_formsService.GetFormRecordByRms(inmatePrebookStagingId));
        }

        //Get list of required forms
        [HttpGet("GetRequiredForms")]
        public IActionResult GetRequiredForms(FormCategories categoryName, string facilityAbbr, int incarcerationId,
            int arrestId, int inmatePrebookId, bool isOptionalForms = false, string caseTypeId = "")
        {
            return Ok(_formsService.GetRequiredForms(categoryName, facilityAbbr, incarcerationId,
                arrestId, inmatePrebookId, isOptionalForms, caseTypeId));
        }

        //Get required arrest type
        [HttpGet("GetRequiredCaseType")]
        public IActionResult GetRequiredCaseType(FormCategories categoryName, string facilityAbbr, int incarcerationId,
            int arrestId, int inmatePrebookId, string caseTypeId = "")
        {
            return Ok(_formsService.GetRequiredCaseType(categoryName, facilityAbbr, incarcerationId, arrestId, inmatePrebookId, caseTypeId));
        }

        [HttpGet("GetFacilityFormDetails")]
        public IActionResult GetFacilityFormDetails(int facilityId, int formTemplateId)
        {
            return Ok(_formsService.LoadFacilityFormDetails(facilityId, formTemplateId));
        }

        [HttpGet("checkFormTemplate")]
        public IActionResult CheckFormTemplate(int templateId)
        {
            return Ok(_formsService.CheckFormTemplate(templateId));
        }
        //Menthod For Permissions
        [FuncPermission(480, FuncPermissionType.Add)]
        [HttpPut("AddBookingForm")]
        public IActionResult AddBookingForm()
        {
            return Ok();
        } 
        [FuncPermission(480, FuncPermissionType.Edit)]
        [HttpPut("EditBookingForm")]
        public IActionResult EditBookingForm()
        {
            return Ok();
        } 
         [FuncPermission(480, FuncPermissionType.Delete)]
        [HttpPut("DeleteBookingForm")]
        public IActionResult DeleteBookingForm()
        {
            return Ok();
        } 
        [FuncPermission(481, FuncPermissionType.Add)]
        [HttpPut("AddCaseForm")]
        public IActionResult AddCaseForm()
        {
            return Ok();
        } 
        [FuncPermission(481, FuncPermissionType.Edit)]
        [HttpPut("EditCaseForm")]
        public IActionResult EditCaseForm()
        {
            return Ok();
        } 
         [FuncPermission(481, FuncPermissionType.Delete)]
        [HttpPut("DeleteCaseForm")]
        public IActionResult DeleteCaseForm()
        {
            return Ok();
        } 
        
        [HttpGet("IsPBPCFormsExists")]
        public IActionResult IsPBPCFormsExists(int inmatePrebookId)
        {
            return Ok(_formsService.IsPBPCFormsExists(inmatePrebookId));
        }
    }
}