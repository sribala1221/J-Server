using Microsoft.AspNetCore.Mvc;
using ServerAPI.ViewModels;
using ServerAPI.Services;
using System.Threading.Tasks;
using ServerAPI.Authorization;

namespace ServerAPI.Controllers
{
    [Route("[controller]")]
    public class ClassifyViewerController : ControllerBase
    {
        private readonly IClassifyViewerService _iClassifyService;
        private readonly IClassifyQueueService _classifyQueueService;
        private readonly IClassCountService _iclassCountService;
        private readonly IClassLinkService _iclassLinkService;

        public ClassifyViewerController(IClassifyViewerService iClassifyService,
            IClassifyQueueService classifyQueueService, IClassCountService classCountService, IClassLinkService classLinkService)
        {
            _iClassifyService = iClassifyService;
            _classifyQueueService = classifyQueueService;
            _iclassCountService = classCountService;
            _iclassLinkService = classLinkService;
        }

        // To get classification class log details.
        [HttpGet("GetClassLog")]
        public IActionResult GetClassLog([FromQuery] ClassLogInputs classLogInputs)
        {
            return Ok(_iClassifyService.GetClassLog(classLogInputs));
        }

        // To Update Classify Settings Details
        [HttpPost("UpdateClassifySettings")]
        public async Task<IActionResult> UpdateClassifySettings([FromBody] LogParameters inputs)
        {
            return Ok(await _iClassifyService.UpdateClassify(inputs));
        }

        // To get Details of Inmate Classification for tree plus click
        [HttpGet("GetInmateClassify")]
        public IActionResult GetInmateClassify(int inmateId)
        {
            return Ok(_iClassifyService.GetInmateClassify(inmateId));
        }

        // For classlog page load        
        [HttpGet("GetClassifyLog")]
        public async Task<IActionResult> GetClassifyLog(int facilityId)
        {
            return Ok(await _iClassifyService.GetClassifyLog(facilityId));
        }
        #region Classification Queue

        //For class queue page load
        [FuncPermission(807, FuncPermissionType.Access)]
        [HttpGet("GetClassifyQueue")]
        public IActionResult GetClassifyQueue(int facilityId)
        {
            return Ok(_classifyQueueService.GetClassifyQueue(facilityId));
        }

        //getting queue details
        [FuncPermission(807, FuncPermissionType.Access)]
        [HttpGet("GetQueueDetails")]
        public IActionResult GetQueueDetails([FromQuery] QueueInputs inputs)
        {
            return Ok(_classifyQueueService.GetQueueDetails(inputs));
        }

        [HttpGet("GetQueueDetailsFromFacilityId")]
        public IActionResult GetQueueDetailsFromFacilityId([FromQuery] int facilityId)
        {
            return Ok(_classifyQueueService.GetQueueDetailsFromFacilityId(facilityId));
        }

        [FuncPermission(807, FuncPermissionType.Access)]
        [HttpGet("GetQueueDetailsManage")]
        public IActionResult GetQueueDetailsManage([FromQuery] QueueInputs inputs)
        {
            return Ok(_classifyQueueService.GetQueueDetails(inputs));
        }

        //Getting Manage Special Queue History Details
        [HttpGet("GetManageSpecialQueueHistory")]
        public IActionResult GetManageSpecialQueue([FromQuery] SpecialQueueInputs inputs)
        {
            return Ok(_classifyQueueService.ManageSpecialQueueHistory(inputs));
        }

        //Insert/Update/Delete Functionality For SpecialClassQueueSaveHistory Grid
        [FuncPermission(807, FuncPermissionType.Add)]
        [HttpPost("InsertSpecialClassQueue")]
        public async Task<IActionResult> InsertSpecialClassQueue([FromBody] SpecialQueueInputs inputs)
        {
            return Ok(await _classifyQueueService.InsertSpecialClassQueue(inputs));
        }

        //Update/Delete Functionality For SpecialClassQueueSaveHistory Grid
        [FuncPermission(807, FuncPermissionType.Edit)]
        [HttpPost("InsertSpecialClassQueueEdit")]
        public async Task<IActionResult> InsertSpecialClassQueueEdit([FromBody] SpecialQueueInputs inputs)
        {
            return Ok(await _classifyQueueService.InsertSpecialClassQueue(inputs));
        }

        //Delete Functionality For SpecialClassQueueSaveHistory Grid
        [FuncPermission(807, FuncPermissionType.Delete)]
        [HttpPost("InsertSpecialClassQueueDelete")]
        public async Task<IActionResult> InsertSpecialClassQueueDelete([FromBody] SpecialQueueInputs inputs)
        {
            return Ok(await _classifyQueueService.InsertSpecialClassQueue(inputs));
        }

        //Save Method For Classification Batch Review Popup
        [HttpPost("InsertClassificationNarrative")]
        public async Task<IActionResult> InsertClassificationNarrative([FromBody] QueueInputs inputs)
        {
            return Ok(await _classifyQueueService.InsertClassificationNarrative(inputs));
        }

        //For Getting Review List
        [HttpGet("GetReview")]
        public IActionResult GetReview(string siteOption, int facilityId)
        {
            return Ok(_classifyQueueService.GetReview(siteOption, facilityId));
        }

        //For Getting Special Queue Count
        [HttpGet("GetSpecialQueue")]
        public IActionResult GetSpecialQueue(int facilityId)
        {
            return Ok(_classifyQueueService.GetSpecialQueue(facilityId));
        }

        #endregion

        #region Class Count

        //Page loading at first time
        [HttpGet("GetHousing")]
        public IActionResult GetHousing(int facilityId)
        {
            return Ok(_iclassCountService.GetHousing(facilityId));
        }

        //Refreshing time
        [HttpGet("GetHousingCountDetails")]
        public IActionResult GetHousingCountDetails([FromQuery] ClassCountInputs countInputs)
        {
            return Ok(_iclassCountService.GetHousingCountDetails(countInputs));
        }

        //Insert floor note details
        [HttpPost("InsertFloorNote")]
        public async Task<IActionResult> InsertFloorNote([FromBody] FloorNotesVm value)
        {
            return Ok(await _iclassCountService.InsertFloorNote(value));
        }
        #endregion

        #region Class Link
        //Getting class link details
        [HttpGet("GetClassLinkDetails")]
        public IActionResult GetClassLinkDetails([FromQuery] ClassLinkInputs inputs)
        {
            return Ok(_iclassLinkService.GetClassLinkDetails(inputs));
        }
        //Insert class link details
        [HttpPost("InsertClassifyViewerClassLink")]
        public async Task<IActionResult> InsertClassifyViewerClassLink([FromBody] ClassLinkAddParam inputs)
        {
            return Ok(await _iclassLinkService.InsertClassifyViewerClassLink(inputs));
        }
        //Update class link details
        [HttpPut("UpdateClassifyViewerClassLink")]
        public async Task<IActionResult> UpdateClassifyViewerClassLink([FromBody] ClassLinkUpdateParam inputs)
        {
            return Ok(await _iclassLinkService.UpdateClassifyViewerClassLink(inputs));
        }
        //Delete class link details
        [HttpPut("DeleteClassifyViewerClassLink")]
        public IActionResult DeleteClassifyViewerClassLink(int inmateClassificationLinkId, bool isUndo)
        {
            return Ok(_iclassLinkService.DeleteClassifyViewerClassLink(inmateClassificationLinkId, isUndo));
        }
        //Getting class link histrory details
        [HttpGet("GetClassLinkViewHistory")]
        public IActionResult GetClassLinkViewHistory(int facilityId, int inmateId)
        {
            return Ok(_iclassLinkService.GetClassLinkViewHistory(facilityId, inmateId));
        }
        #endregion
        //Newly added for Functionality Permission
        [FuncPermission(1070,FuncPermissionType.Add)]
        [HttpPut("AddGeneralNote")]
        public IActionResult AddGeneralNote()
        {
            return Ok();
        }
        [FuncPermission(1070,FuncPermissionType.Edit)]
        [HttpPut("EditGeneralNote")]
        public IActionResult EditGeneralNote()
        {
            return Ok();
        }
        [FuncPermission(1070,FuncPermissionType.Delete)]
        [HttpDelete("DeleteGeneralNote")]
        public IActionResult DeleteGeneralNote()
        {
            return Ok();
        }
        [FuncPermission(1060,FuncPermissionType.Add)]
        [HttpPut("addLocationNote")]
        public IActionResult addLocationNote()
        {
            return Ok();
        }
        [FuncPermission(1060,FuncPermissionType.Edit)]
        [HttpPut("EditLocationNote")]
        public IActionResult EditLocationNote()
        {
            return Ok();
        }
        [FuncPermission(1060,FuncPermissionType.Delete)]
        [HttpDelete("DeleteLocationNote")]
        public IActionResult DeleteLocationNote()
        {
            return Ok();
        }
    }
}
