using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using ServerAPI.Services;
using ServerAPI.ViewModels;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using jsreport.Client;
using jsreport.Types;
using System;
using ServerAPI.Authorization;
using ServerAPI.Utilities;
using System.Net;

namespace ServerAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class PrebookWizardController : ControllerBase
    {
        private readonly IPrebookWizardService _prebookWizardService;
        private readonly IWizardService _wizardService;
        private readonly ICommonService _commonService;
        private readonly IConfiguration _configuration;

        public PrebookWizardController(IPrebookWizardService prebookWizardService, IWizardService wizardService,
            ICommonService commonService, IConfiguration configuration)
        {
            _prebookWizardService = prebookWizardService;
            _wizardService = wizardService;
            _commonService = commonService;
            _configuration = configuration;
        }

        //For creating new prebookinfo 
        [HttpPost("CreatePrebook")]
        public async Task<IActionResult> InsertPrebook([FromBody] InmatePrebookVm value) => 
            Ok(await _prebookWizardService.InsertPrebook(value));

        [FuncPermission(400, FuncPermissionType.Add)]
        [HttpPost("CreatePrebookJms")]
       public async Task<IActionResult> InsertPrebookJms([FromBody] InmatePrebookVm value) => 
           Ok(await _prebookWizardService.InsertPrebook(value));

       //For updating prebookinfo 
        [FuncPermission(602, FuncPermissionType.Edit)]
        [HttpPost("UpdatePrebook")]
        public async Task<IActionResult> UpdatePrebook([FromBody] InmatePrebookVm value) => 
            Ok(await _prebookWizardService.UpdatePrebook(value));

        [HttpPost("UpdatePrebookJms")]
        public async Task<IActionResult> UpdatePrebookJms([FromBody] InmatePrebookVm value) => 
            Ok(await _prebookWizardService.UpdatePrebook(value));

        //For updating PrebookLaststep
        [HttpPost("UpdatePrebookLastStep")]
        public async Task<IActionResult> UpdatePrebookLastStep([FromBody] InmatePrebookVm value) => 
            Ok(await _prebookWizardService.UpdatePrebookLastStep(value));

        #region Prebook Charges

        //Get prebook charges
        [HttpGet("GetPrebookCharges")]
        public IActionResult GetPrebookCharges(int inmatePrebookId, bool deleteFlag, int incarcerationId, int arrestId) =>
            Ok(_prebookWizardService.GetPrebookCharges(inmatePrebookId, deleteFlag, 0, incarcerationId, arrestId));

        //Create a new prebook charge
        [FuncPermission(467, FuncPermissionType.Add)]
        [HttpPost("InsertPrebookCharge")]
        public async Task<IActionResult> InsertPrebookCharge([FromBody] PrebookCharge prebookCharge)
        {
            if (prebookCharge.CrimeLookupId.HasValue)
            {
                await _prebookWizardService.InsertInmatePrebookCharge(prebookCharge);
            }
            else
            {
                await _prebookWizardService.InsertForceCharge(prebookCharge);
            }

            return Ok(prebookCharge.InmatePrebookId > 0
                ? _prebookWizardService.GetPrebookCharges(prebookCharge.InmatePrebookId ?? 0, false, 0)
                : new List<PrebookCharge>());
        }

        //Update Prebook Charge
        [HttpPost("UpdatePrebookCharge")]
        public async Task<IActionResult> UpdatePrebookCharge([FromBody] PrebookCharge prebookCharge)
        {
            if (prebookCharge.InmatePrebookChargeId.HasValue)
            {
                await _prebookWizardService.UpdateInmatePrebookCharge(prebookCharge);
            }

            if (prebookCharge.CrimeForceId.HasValue)
            {
                await _prebookWizardService.UpdateForceCharge(prebookCharge);
            }

            return Ok(prebookCharge.InmatePrebookId > 0
                ? _prebookWizardService.GetPrebookCharges(prebookCharge.InmatePrebookId ?? 0, false, 0)
                : new List<PrebookCharge>());
        }

        //Delete Prebook charge
        [HttpPost("DeleteInmatePrebookCharge")]
        public async Task<IActionResult> DeleteInmatePrebookCharges([FromBody] int inmatePrebookChargeId) => inmatePrebookChargeId <= 0
                ? BadRequest("InmatePrebookChargeId is required")
                : (IActionResult)Ok(await _prebookWizardService.DeleteInmatePrebookCharges(inmatePrebookChargeId));

        //Undo Prebook Deleted charges
        [HttpPost("UndoDeleteInmatePrebookCharge")]
        public async Task<IActionResult> UndoDeleteInmatePrebookCharges([FromBody] int inmatePrebookChargeId) => inmatePrebookChargeId <= 0
                ? BadRequest("InmatePrebookChargeId is required")
                : (IActionResult)Ok(await _prebookWizardService.UndoDeleteInmatePrebookCharges(inmatePrebookChargeId));

        //Get Crime details
        [HttpPost("GetCrimeSearch")]
        public IActionResult GetCrimeSearch([FromBody] CrimeLookupVm crimeDetails) => 
            Ok(_prebookWizardService.GetCrimeSearch(crimeDetails));

        //Get Crime History
        [HttpGet("GetCrimeHistory")]
        public IActionResult GetCrimeHistory(int? inmatePrebookChargeId = 0, int? crimeForceId = 0) =>
            inmatePrebookChargeId == 0 && crimeForceId == 0
                ? BadRequest("Server Error")
                : (IActionResult)Ok(_prebookWizardService.GetCrimeHistory(inmatePrebookChargeId));

        //Insert Crime History
        [HttpPost("InsertCrimeHistory")]
        public async Task<IActionResult> InsertCrimeHistory(CrimeHistoryVm crime) => 
            Ok(await _prebookWizardService.InsertCrimeHistory(crime));

        //Delete Crime force details
        [HttpPost("DeleteCrimeForce")]
        public async Task<IActionResult> DeleteCrimeForces([FromBody] int crimeForceId) =>
            crimeForceId <= 0 ? BadRequest("CrimeForceId is required") : 
                (IActionResult)Ok(await _prebookWizardService.DeleteCrimeForces(crimeForceId));

        //Undo deleted Crime force
        [HttpPost("UndoDeleteCrimeForce")]
        public async Task<IActionResult> UndoDeleteCrimeForces([FromBody] int crimeForceId) =>
            crimeForceId <= 0 ? (IActionResult) BadRequest("CrimeForceId is required")
                : Ok(await _prebookWizardService.UndoDeleteCrimeForces(crimeForceId));

        #endregion

        #region Prebook Warrants

        //Get list of Prebook Warrants
        [HttpGet("GetPrebookWarrants")]
        public IActionResult GetPrebookWarrant(int inmatePrebookId, bool deleteFlag = false, int incarcerationId = 0,
            int arrestId = 0) =>
            Ok(_prebookWizardService.GetPrebookWarrant(inmatePrebookId, deleteFlag, incarcerationId, arrestId));

        //Get a single prebook warrant based on Id
        [HttpGet("GetPrebookWarrantById")]
        public IActionResult GetPrebookWarrantById(int inmatePrebookWarrantId) => 
            Ok(_prebookWizardService.GetPrebookWarrantById(inmatePrebookWarrantId));

        //Create a new prebook warrant
        [HttpPost("CreatePrebookWarrant")]
        public async Task<IActionResult> CreatePrebookWarrant([FromBody] InmatePrebookWarrantVm warrant) => 
            Ok(await _prebookWizardService.InsertPrebookWarrant(warrant));

        //Update Prebbok Warrant
        [HttpPost("UpdatePrebookWarrant")]
        public async Task<IActionResult> UpdatePrebookWarrant([FromBody] InmatePrebookWarrantVm warrant) => 
            Ok(await _prebookWizardService.UpdatePrebookWarrant(warrant));

        //Delete Prebook Warrant
        [FuncPermission(465, FuncPermissionType.Delete)]
        [HttpPost("DeletePrebookWarrant")]
        public async Task<IActionResult> DeletePrebookWarrant([FromBody] int inmatePrebookWarrantId) =>
            inmatePrebookWarrantId <= 0 ? (IActionResult) BadRequest("InmatePrebookWarrantId is required")
                : Ok(await _prebookWizardService.DeletePrebookWarrant(inmatePrebookWarrantId));

        //Undo deleted Prebook Warrant
        [HttpPost("UndoDeletePrebookWarrant")]
        public async Task<IActionResult> UndoDeleteInmatePrebookWarrant([FromBody] int inmatePrebookWarrantId) =>
            inmatePrebookWarrantId <= 0 ? (IActionResult) BadRequest("InmatePrebookWarrantId is required")
                : Ok(await _prebookWizardService.UndoDeletePrebookWarrant(inmatePrebookWarrantId));

        #endregion

        //Get all Personal inventory
        [HttpGet("GetPersonalInventoryPrebook")]
        public IActionResult GetPersonalInventoryPrebook(int preBookId, bool deleteFlag = false) => 
            Ok(_prebookWizardService.GetPersonalInventoryPrebook(preBookId, deleteFlag));

        //Create new inventory

        [HttpPost("CreatePersonalInventoryPrebook")]
        public async Task<IActionResult> CreatePersonalInventoryPrebook([FromBody] PersonalInventoryVm[] properties) => 
            Ok(await _prebookWizardService.InsertPersonalInventoryPrebook(properties));

        //Delete personal inventory
        [HttpPost("DeletePersonalInventoryPrebook")]
        public async Task<IActionResult> DeletePersonalInventoryPrebook([FromBody] int personalInventoryPreBookId) =>
            personalInventoryPreBookId <= 0 ? (IActionResult) BadRequest("PersonalInventoryPreBookId is required") : 
                Ok(await _prebookWizardService.DeletePersonalInventoryPrebook(personalInventoryPreBookId));

        //Undo deleted personal inventory
        [HttpPost("UndoDeletePersonalInventoryPrebook")]
        public async Task<IActionResult> UndoDeletePersonalInventoryPrebook([FromBody] int personalInventoryPreBookId) =>
            personalInventoryPreBookId <= 0 ? (IActionResult) BadRequest("PersonalInventoryPreBookId is required") : 
                Ok(await _prebookWizardService.UndoDeletePersonalInventoryPrebook(personalInventoryPreBookId));

        //property receipt
        [HttpPost("GetPropertyReceipt")]
        public async Task<IActionResult> GetPropertyReceipt([FromBody] PrebookProperty prebookProperty)
        {
            PrebookProperty prebookPropertyDetails = _prebookWizardService.GetPropertyDetails(prebookProperty);
            string json = JsonConvert.SerializeObject(prebookPropertyDetails);
            ReportingService rs = new ReportingService(_configuration.GetSection("SiteVariables")["ReportUrl"]);
            //try catch used for to handle 2 exceptions (10061 & 404) from jsreport.
            try
            {
                _commonService.atimsReportsContentLog(InventoryQueueConstants.PREBOOKPROPERTYNAME, json);
                Report report = await rs.RenderByNameAsync(InventoryQueueConstants.PREBOOKPROPERTYNAME, json);
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

        #region Prebook Forms

        //Get list of prebook forms
        [HttpGet("GetPreBookForms")]
        public IActionResult GetPreBookForms(int inmatePrebookId, FormScreen formScreenFlag) =>
            inmatePrebookId <= 0 ? (IActionResult) BadRequest("InmatePrebookId DeleteFlag required")
                : Ok(_prebookWizardService.GetPreBookForms(inmatePrebookId, formScreenFlag));

        //Get list of saved forms
        [HttpGet("LoadSavedForms")]
        public IActionResult LoadSavedForms(int inmatePrebookId, int formTemplateId, FormScreen formScreenFlag,
            bool deleteFlag) =>
            inmatePrebookId <= 0 ? (IActionResult) BadRequest("InmatePrebookId and DeleteFlag required") : 
                Ok(_prebookWizardService.LoadSavedForms(inmatePrebookId, formTemplateId, formScreenFlag, deleteFlag));

        //Delete prebook form
        [HttpPost("DeletePreBookForm")]
        public async Task<IActionResult> DeletePreBookForm([FromBody] int formRecordId) =>
            formRecordId == 0 ? (IActionResult) BadRequest("FormRecordId and DeleteFlag required")
                : Ok(await _prebookWizardService.DeletePreBookForm(formRecordId));

        //Adding new form
        [HttpGet("AddForm")]
        public IActionResult AddForm(int inmatePrebookId, int formTemplateId) =>
            inmatePrebookId == 0 || formTemplateId == 0
                ? (IActionResult) BadRequest("PrebookId and FormTemplateId are required")
                : Ok(_prebookWizardService.AddForm(inmatePrebookId, formTemplateId));

        //Show the Form list
        [HttpGet("ListForm")]
        public IActionResult ListForm(int formRecordId) =>
            formRecordId == 0 ? (IActionResult) BadRequest("FormRecordId is required")
                : Ok(_prebookWizardService.ListForm(formRecordId));

        //Saving Form
        [HttpPost("SaveForm")]
        public async Task<IActionResult> SaveForm(LoadSavedForms formdata) => 
            Ok(await _prebookWizardService.UpdateForm(formdata));

        #endregion

        #region Prebook Attachment

        //Get Prebook attachment
        [HttpPost("GetPrebookAttachment")]
        public IActionResult GetPrebookAttachment([FromBody] AttachmentSearch attachmentSearch) => 
            Ok(_prebookWizardService.GetPrebookAttachment(attachmentSearch));

        //Create new prebook attachement
        [HttpPost("CreatePrebookAttachment")]
        public async Task<IActionResult> CreatePrebookAttachment([FromBody] PrebookAttachment applets) => 
            Ok(await _prebookWizardService.InsertPrebookAttachment(applets));

        //Update Prebook attachment
        [HttpPost("UpdatePrebookAttachment")]
        public async Task<IActionResult> UpdatePrebookAttachment([FromBody] PrebookAttachment applets) => 
            Ok(await _prebookWizardService.UpdatePrebookAttachment(applets));

        //Get prebook attachment entry

        [HttpGet("LoadPrebookAttachmentEntry")]
        public IActionResult LoadPrebookAttachmentEntry(string attachType, int inmateId = 0, int facilityId = 0) => 
            Ok(_prebookWizardService.LoadPrebookAttachmentEntry(attachType, inmateId, facilityId));

        //Get attach history
        [HttpGet("LoadAttachHistory")]
        public IActionResult LoadAttachHistory(int appletsavedId) => 
            Ok(_prebookWizardService.LoadAttachHistory(appletsavedId));

        //Open prebook attachment
        [HttpGet("OpenPrebookAttachment")]
        public FileResult OpenPrebookAttachment(int appletSavedId) => new FileContentResult(
                System.IO.File.ReadAllBytes(_prebookWizardService.OpenPrebookAttachment(appletSavedId)), "application/pdf")
            {
                FileDownloadName = "test.pdf"
            };

        //Delete prebook attachment
        [HttpGet("DeletePrebookAttachment")]
        public async Task<IActionResult> DeletePrebookAttachment(int appletSavedId) => 
            Ok(await _prebookWizardService.DeletePrebookAttachment(appletSavedId));

        //Undo prebook deleted attachment
        [HttpGet("UndoDeletePrebookAttachment")]
        public async Task<IActionResult> UndoDeletePrebookAttachment(int appletSavedId, string history) => 
            Ok(await _prebookWizardService.UndoDeletePrebookAttachment(appletSavedId, history));

        #endregion

        [HttpPost("SaveMedPrescreenStatusStartComplete")]
        public async Task<IActionResult> PostUpdateStatusStartComplete([FromBody] MedPrescreenStatus medPrescreenStatus) => 
            Ok(await _prebookWizardService.UpdateMedPrescreenStatusStartComplete(medPrescreenStatus));

        [HttpPost("SaveMedPrescreenStatus")]
        public async Task<IActionResult> PostUpdateStatus([FromBody] MedPrescreenStatus medPrescreenStatus) => 
            Ok(await _prebookWizardService.UpdateMedPrescreenStatus(medPrescreenStatus));

        [HttpGet("GetAttachmentHistory")]
        public IActionResult GetAttachmentHistory(int prebookId) => Ok(_prebookWizardService.GetAttachmentHistory(prebookId));

        //Complete prebook record
        [HttpPost("PostPrebookComplete")]
        public async Task<IActionResult> PostPrebookComplete([FromBody] int inmatePrebookId) => 
            Ok(await _prebookWizardService.UpdatePrebookComplete(inmatePrebookId));

        //Undo prebook complete
        [HttpPost("PostPrebookUndoComplete")]
        public async Task<IActionResult> PostPrebookUndoComplete([FromBody] int inmatePrebookId) => 
            Ok(await _prebookWizardService.UpdatePrebookUndoComplete(inmatePrebookId));

        [HttpGet("GetAgencies")]
        public IActionResult GetAgencies() => Ok(_prebookWizardService.GetAgencies());

        [HttpGet("GetPrebookWizard")]
        [AllowAnonymous]
        public IActionResult GetPrebookWizard()
        {
            //temporary hard coded WizardId and WizardFacilityId
            AoWizardVm wizardDetails = _wizardService.GetWizardSteps(9)[0];
            AoWizardFacilityVm wizardFacilitySteps =
                wizardDetails.WizardFacilities.SingleOrDefault(wf => wf.Facility?.FacilityId == 1);
            return Ok(wizardFacilitySteps);
        }

        [HttpGet("GetFacilities")]
        public IActionResult GetFacilities() => Ok(_prebookWizardService.GetFacilities());

        [HttpGet("GetVehicleMake")]
        public IActionResult GetVehicleMake() => Ok(_prebookWizardService.GetVehicleMake());

        [HttpGet("GetVehicleModel")]
        public IActionResult GetVehicleModel(int vehicleMakeId) => Ok(_prebookWizardService.GetVehicleModel(vehicleMakeId));

        //temporary
        [HttpGet("GetPersonnel")]
        public IActionResult GetPersonnel(string type) => Ok(_commonService.GetPersonnel(type));

        [HttpPost("Upload")]
        public IActionResult Upload() => HttpContext.Request.Form.Files.Count == 0 ? 
            (IActionResult) BadRequest("file not uploaded!")
            : Ok(_commonService.UploadFile(HttpContext.Request.Form.Files[0]));

        [HttpGet("DeleteFile")]
        public IActionResult DeleteFile(string fileName)
        {
            if (string.IsNullOrEmpty(fileName))
                return BadRequest("fileName Required!");
            _commonService.DeleteFile(fileName);
            return Ok();
        }

        [HttpGet("GetAttachment")]
        public IActionResult GetAttachment(int attachmentId)
        {
            PrebookAttachment attachmentFile = _commonService.GetAttachment(attachmentId);
            string filePath = attachmentFile.AttachmentFile;
            if (string.IsNullOrEmpty(filePath))
                return Ok("File Not Found!");
            if (!attachmentFile.AppletsSavedIsExternal && !System.IO.File.Exists(filePath))
                return Ok("File Not Found!");
            if (attachmentFile.AppletsSavedIsExternal)
            {
                WebRequest req = WebRequest.Create(filePath);
                WebResponse response = req.GetResponse();
                Stream stream = response.GetResponseStream();
                return Ok(stream);
            }
            return File(new FileStream(filePath, FileMode.Open), "application/octet-stream",
                Path.GetFileName(filePath));
        }

        [HttpGet("CheckForceCharge")]
        public IActionResult CheckForceCharge(string siteOptionName) => 
            Ok(_commonService.GetSiteOptionValue(siteOptionName));

        [HttpGet("GetBookingNumbers")]
        public IActionResult GetBookingNumbers(int inmateId) => Ok(_prebookWizardService.GetBookingNumbers(inmateId));

        [HttpGet("GetAttachmentDetail")]
        public IActionResult GetAttachmentDetail(int appletsSavedId) => 
            Ok(_prebookWizardService.GetAttachmentDetail(appletsSavedId));

        [HttpPost("CreateVehicleModel")]
        public async Task<IActionResult> CreateVehicleModel([FromBody] VehicleModelVm vehicleModel) => 
            Ok(await _prebookWizardService.CreateVehicleModel(vehicleModel));

        [FuncPermission(212,FuncPermissionType.Delete)]
        [HttpPost("UpdateVehicleModel")]
        public async Task<IActionResult> UpdateVehicleModel([FromBody] VehicleModelVm vehicleModel) => 
            Ok(await _prebookWizardService.UpdateVehicleModel(vehicleModel));

        [HttpPost("CreateVehicleMake")]
        public async Task<IActionResult> CreateVehicleMake([FromBody] VehicleMakeVm vehicleMake) => 
            Ok(await _prebookWizardService.CreateVehicleMake(vehicleMake));

        [HttpPost("UpdateVehicleMake")]
        public async Task<IActionResult> UpdateVehicleMake([FromBody] VehicleMakeVm vehicleMake) => 
            Ok(await _prebookWizardService.UpdateVehicleMake(vehicleMake));

        #region InmatePrebookCase

        //Get prebook cases
        [HttpGet("GetPrebookCases")]
        public IActionResult GetPrebookCases(int inmatePrebookId, bool deleteFlag) => 
            Ok(_prebookWizardService.GetPrebookCases(inmatePrebookId, deleteFlag));

        //Insert/Update a prebook case
        [HttpPost("InsertUpdatePrebookCase")]
        public IActionResult InsertUpdatePrebookCase([FromBody] List<InmatePrebookCaseVm> prebookCaseVms,
            [FromQuery] bool deleteFlag) =>
            Ok(_prebookWizardService.InsertUpdatePrebookCase(prebookCaseVms, deleteFlag));

        //Delete / Undo Prebook Case
        [HttpPost("DeleteUndoPrebookCase")]
        public async Task<IActionResult> DeleteUndoPrebookCase([FromBody] InmatePrebookCaseVm prebookCaseVm) =>
            prebookCaseVm.InmatePrebookCaseId <= 0
                ? BadRequest("InmatePrebookCaseId is required")
                : (IActionResult)Ok(await _prebookWizardService.DeleteUndoPrebookCase(prebookCaseVm));

        #endregion
        [HttpGet("GetPrebookProperty")]
        public IActionResult GetPrebookProperty(int preBookId, bool deleteFlag = false) => 
            Ok(_prebookWizardService.GetPersonalInventoryPrebook(preBookId, deleteFlag));

        [HttpGet("GetDefaultAgency")]
        public IActionResult GetDefaultAgency() => Ok(_prebookWizardService.GetDefaultAgencyId());

        //Newly added for vechle 
        [FuncPermission(212,FuncPermissionType.Add)]
        [HttpPut("AddVehicleModel")]
        public IActionResult AddVehicleModel() => Ok();

        //Newly added for New Prebook in Prebook->Active
        [FuncPermission(602,FuncPermissionType.Add)]
        [HttpPut("AddNewPrebook")]
        public IActionResult AddNewPrebook() => Ok();

        //As per updated functionality permission,467 edit permission not exist
        // [FuncPermission(467, FuncPermissionType.Edit)]
        [HttpPut("ForceChargePermission")]
        public IActionResult ForceChargePermission() => Ok();

        //Permission For Booking Attachment
        [FuncPermission(483, FuncPermissionType.Add)]
        [HttpPut("AddBookingAttachment")]
        public IActionResult AddBookingAttachment() => Ok();

        [FuncPermission(483, FuncPermissionType.Delete)]
        [HttpDelete("DeleteBookingAttachment")]
        public IActionResult DeleteBookingAttachment() => Ok();

        [FuncPermission(484, FuncPermissionType.Add)]
        [HttpPut("AddCaseAttachment")]
        public IActionResult AddCaseAttachment() => Ok();

        [FuncPermission(483, FuncPermissionType.Access)]
        [HttpPut("AccessBookingAttachment")]
        public IActionResult AccessBookingAttachment() => Ok();

        [FuncPermission(483, FuncPermissionType.Edit)]
        [HttpPut("EditBookingAttachment")]
        public IActionResult EditBookingAttachment() => Ok();

        [FuncPermission(484, FuncPermissionType.Delete)]
        [HttpDelete("DeleteCaseAttachment")]
        public IActionResult DeleteCaseAttachment() => Ok();

        [FuncPermission(484, FuncPermissionType.Access)]
        [HttpPut("AccessCaseAttachment")]
        public IActionResult AccessCaseAttachment() => Ok();

        [FuncPermission(484, FuncPermissionType.Edit)]
        [HttpPut("EditCaseAttachment")]
        public IActionResult EditCaseAttachment() => Ok();
    }
}
