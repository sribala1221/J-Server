﻿using Microsoft.AspNetCore.Mvc;
using ServerAPI.Authorization;
using ServerAPI.Services;
using ServerAPI.Utilities;
using ServerAPI.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ServerAPI.Controllers
{
	[Route("[controller]")]
	public class ClassifyController : ControllerBase
	{
		private readonly IClassifyService _classifyService;
        private readonly IClassifyClassFileService _classifyClassFileService;
        private readonly ICommonService _commonService;

		public ClassifyController(IClassifyService classifyService, IClassifyClassFileService classifyClassFileService, ICommonService commonService)
		{
			_classifyService = classifyService;
            _classifyClassFileService = classifyClassFileService;
            _commonService = commonService;
		}

		#region Initial

		//Get Inmate classification summary details
		//lastTen is a nullable int because if we want to get the last 10 review history we could pass the count to it.
		[HttpGet("InmateClassification")]
		public IActionResult GetInmateClassificationSummary(int inmateId)
		{
			return Ok(_classifyService.GetInmateClassificationSummary(inmateId));
		}

		//Get Inmate Initial classification details

		[HttpGet("ClassificationInitial")]
		public IActionResult GetInmateInitialClassfication(int inmateId)
		{
			return Ok(_classifyService.GetInitialClassification(inmateId));
		}

		//Get Inmate classification view  details
		[HttpGet("InmateClassficationEntry")]
		public IActionResult GetInmateClassficationEntry(int inmateClassificationId, int inmateId)
		{
			return Ok(_classifyService.GetInmateClassificationDetails(inmateClassificationId, inmateId));
		}

        //Creating classification entry
        [FuncPermission(801, FuncPermissionType.Add)]
        [HttpPost("ClassificationEntry")]
		public async Task<IActionResult> InsertClassificationEntry([FromBody] InmateDetail details)
		{
			return Ok(await _classifyService.InsertInmateClassificationEntry(details));
		}

        //Updating classification entry
        [FuncPermission(801, FuncPermissionType.Edit)]
        [HttpPut("SaveClassificationEntry")]
		public async Task<IActionResult> UpdateClassificationEntry([FromBody] InmateClassificationVm classifydetails)
		{
			return Ok(await _classifyService.UpdateClassification(classifydetails));
		}
        [FuncPermission(801, FuncPermissionType.Edit)]
        [HttpPut("UpdateClassificationEntry")]
        public async Task<IActionResult> SaveClassificationEntry([FromBody] InmateClassificationVm classifydetails)
        {
            return Ok(await _classifyService.SaveClassification(classifydetails));
        }
        #endregion

        //Get Review Lookup details
        [HttpGet("GetLookupReview")]
		public IActionResult GetLookupReview()
		{
			return Ok(_commonService.GetLookupList(LookupConstants.CLASSREVIEWTEXT));
		}

        //Creating classification review entry
        [FuncPermission(802, FuncPermissionType.Add)]
        [HttpPost("ClassificationReviewEntry")]
		public async Task<IActionResult> InsertReviewEntry([FromBody] InmateDetail details)
		{
			return Ok(await _classifyService.InsertReviewEntry(details));
		}

        //Updating classification review 
        [HttpPut("SaveClassificationReview")]
		public async Task<IActionResult> UpdateReviewEntry([FromBody] InmateClassificationVm details)
		{
			return Ok(await _classifyService.UpdateClassificationReview(details));
		}

        //Creating classification note
        [FuncPermission(803, FuncPermissionType.Add)]
        [HttpPost("ClassificationNote")]
        public async Task<IActionResult> InsertNoteEntry([FromBody] InmateDetail details)
        {
            return Ok(await _classifyService.InsertReviewEntry(details));
        }

        //Updating classification note 
        [FuncPermission(803, FuncPermissionType.Edit)]
        [HttpPut("SaveClassificationNote")]
        public async Task<IActionResult> UpdateNoteEntry([FromBody] InmateClassificationVm details)
        {
            return Ok(await _classifyService.UpdateClassificationReview(details));
        }

        [HttpGet("GetLookupGangSubSet")]
	    public IActionResult GetLookupGangSubSet(string gangName)
	    {
	        return Ok(_commonService.GetLookupGangSubSet(gangName));
	    }

        #region Class File
      
        [HttpGet("LoadClassFile")]
        public async Task<IActionResult> GetClassFile([FromQuery] ClassFileInputs classFileInputs)
        {
            return Ok(await _classifyClassFileService.GetClassFile(classFileInputs));
        }
        
        [HttpGet("GetClassFilePermission")]
        public IActionResult GetClassFilePermission()
        {
            return Ok();
        }

        [FuncPermission(802, FuncPermissionType.Delete)]
        [HttpDelete("InmateDeleteUndo")]
        public async Task<IActionResult> DeleteUndoInmate([FromQuery]DeleteParams deleteParams)
        {
            return Ok(await _classifyClassFileService.InmateDeleteUndo(deleteParams));
        }

        [FuncPermission(802, FuncPermissionType.Delete)]
        [HttpDelete("ReviewDeleteUndo")]
        public async Task<IActionResult> ReviewUndoInmate([FromQuery]DeleteParams deleteParams)
        {
            return Ok(await _classifyClassFileService.InmateDeleteUndo(deleteParams));
        }

        [HttpPost("AddNewClassAttachment")]
        [FuncPermission(805, FuncPermissionType.Add)]
        public IActionResult AddNewClassAttachment()
        {
            return Ok(); //Here we are returning empty (200 ok) response,
            //Because attachment screen is common screen for almost every module.
            //Http actions for attachment screen has been written at PrebookWizard controller.
            //Hence we are not able to check access rights for all module at the same action.
            //Because every module has the unique functionality id,
            //That's why we are creating duplicate http actions,
            //To check permission access for each and every module for attachment screen.
        }

        [HttpPut("UpdateClassAttachment")]
        [FuncPermission(805, FuncPermissionType.Edit)]
        public IActionResult UpdateClassAttachment()
        {
            return Ok();
        }

        [FuncPermission(805, FuncPermissionType.Delete)]
        [HttpDelete("AttachDeleteUndo")]
        public async Task<IActionResult> AttachUndoInmate([FromQuery]DeleteParams deleteParams)
        {
            return Ok(await _classifyClassFileService.InmateDeleteUndo(deleteParams));
        }

        [HttpGet("PendingClassifyCheck")]
        public IActionResult PendingClassifyCheck(int inmateId)
        {
            return Ok(_classifyClassFileService.PendingCheck(inmateId));
        }

        #endregion

        #region Last non-pending classification

        [HttpGet("GetLastNonPendingClassification")]
	    public IActionResult GetLastNonPendingClassification(string inmateNumber)
	    {
	        return Ok(_classifyService.GetLastNonPendingClassification(inmateNumber));
	    }

	    #endregion
        
        [HttpGet("GetInmateCount")]
        public int GetInmateCount(int inmateId)
        {
            return _classifyService.GetInmateCount(inmateId);
        }

        [HttpGet("GetClassifySubModules")]
        public List<KeyValuePair<int, string>> GetClassifySubModules()
        {
            return _classifyService.GetClassifySubModules();
        }
        //Newly added for functionality permission
        [FuncPermission(1106,FuncPermissionType.Add)]
        [HttpPut("AddAttachment")]
        public IActionResult AddAttachment()
        {
            return Ok();
        }
        [FuncPermission(1106,FuncPermissionType.Delete)]
        [HttpDelete("DeleteAttachment")]
        public IActionResult DeleteAttachment()
        {
            return Ok();
        }
        [FuncPermission(1106,FuncPermissionType.Access)]
        [HttpPut("AccessAttachment")]
        public IActionResult AccessAttachment()
        {
            return Ok();
        }
        [FuncPermission(1106,FuncPermissionType.Edit)]
        [HttpPut("EditAttachment")]
        public IActionResult EditAttachment()
        {
            return Ok();
        }
        
        [HttpGet("GetClassifyAlertMessage")]
        public IActionResult GetClassifyAlertMessage(int facilityId)
        {
            return Ok(_classifyService.GetClassifyMessageAlerts(facilityId));
        }
    }
}
