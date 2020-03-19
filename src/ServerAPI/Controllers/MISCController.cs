using Microsoft.AspNetCore.Mvc;
using ServerAPI.Interfaces;
using System.Threading.Tasks;
using ServerAPI.ViewModels;
using ServerAPI.Services;
using ServerAPI.Authorization;
using System.Collections.Generic;

namespace ServerAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class MiscController : ControllerBase
    {
        private readonly IMiscNcicService _iMiscNcicService;
        private readonly IMiscAttendeeService _iMiscAttendeeService;
        private readonly IMiscLabelService _iMiscLabelService;

        private readonly IMiscIssuedPropertyService _iMiscIssuedPropertyService;

        public MiscController(IMiscNcicService miscNcicService, IMiscAttendeeService miscAttendeeService,
            IMiscLabelService miscLabelService, IMiscIssuedPropertyService miscIssuedPropertyService)
        {
            _iMiscNcicService = miscNcicService;
            _iMiscAttendeeService = miscAttendeeService;
            _iMiscLabelService = miscLabelService;
            _iMiscIssuedPropertyService = miscIssuedPropertyService;
        }

        [FuncPermission(8425, FuncPermissionType.Access)]
        [HttpGet("GetNcicDetails")]
        public IActionResult GetNcicDetails(int inmateId, int personId)
        {
            return Ok(_iMiscNcicService.GetNcicDetails(inmateId, personId));
        }

        [FuncPermission(485, FuncPermissionType.Delete)]
        [HttpPut("DeleteExternalAttachment")]
        public async Task<IActionResult> DeleteExternalAttachment(int appletSavedId)
        {
            return Ok(await _iMiscNcicService.DeleteExternalAttachment(appletSavedId));
        }

        [HttpGet("GetInmateAttendee")]
        public IActionResult GetInmateAttendee(int inmateId)
        {
            return Ok(_iMiscAttendeeService.GetInmateAttendeeList(inmateId));
        }

        [HttpPost("InsertAttendee")]
        public async Task<IActionResult> InsertAttendee(AttendenceVm objAttendee)
        {
            return Ok(await _iMiscAttendeeService.InsertAttendee(objAttendee));
        }

        [HttpPut("UpdateAttendee")]
        public async Task<IActionResult> UpdateAttendee(AttendenceVm objAttendee)
        {
            return Ok(await _iMiscAttendeeService.UpdateAttendee(objAttendee));
        }

        [HttpPost("DeleteUndoAttendee")]
        public async Task<IActionResult> DeleteUndoAttendee(DeleteParams obj)
        {
            return Ok(await _iMiscAttendeeService.DeleteUndoAttendee(obj.Id));
        }

        [HttpGet("GetMisclabel")]
        public IActionResult GetMisclabel(int inmateId, string fromScreen, int wizardStepId, int arrestId,
            int personnelId)
        {
            return Ok(_iMiscLabelService.GetMisclabel(inmateId, fromScreen, wizardStepId, arrestId, personnelId));
        }

        [HttpGet("GetMiscPdflabel")]
        public async Task<IActionResult> GetMiscPdflabel(int inmateId, string fromScreen, int wizardStepId, int arrestId,
            int personnelId)
        {
            return Ok(await _iMiscLabelService.GetMiscPdfLabel(inmateId, fromScreen, wizardStepId, arrestId, personnelId));
        }


        [HttpGet("GetIssuedPropertyLookup")]

        public IActionResult GetIssuedPropertyLookup(int facilityId)
        {
            return Ok(_iMiscIssuedPropertyService.GetIssuedPropertyLookup(facilityId));
        }

        [HttpGet("GetIssuedProperty")]
        public IActionResult GetIssuedProperty(int inmateId)
        {
            return Ok(_iMiscIssuedPropertyService.GetIssuedProperty(inmateId));
        }
        
        [FuncPermission(571, FuncPermissionType.Delete)]
        [HttpDelete("GetDeleteIssuedProperty")]
        public IActionResult GetDeleteIssuedProperty()
        {
            return Ok();
        }
        
        [FuncPermission(571, FuncPermissionType.Edit)]
        [HttpPut("GetUpdateIssuedProperty")]
        public IActionResult GetUpdateIssuedProperty()
        {
            return Ok();
        }
        [FuncPermission(571, FuncPermissionType.Access)]
        [HttpGet("GetIssuedPropertyAccess")]
        public IActionResult GetIssuedPropertyAccess()
        {
            return Ok();
        }

        [HttpPost("InsertAndUpdateIssuedProperty")]
        public async Task<IActionResult> InsertAndUpdateIssuedProperty(IssuedPropertyMethod objIssued)
        {
            return Ok(await _iMiscIssuedPropertyService.InsertAndUpdateIssuedProperty(objIssued));
        }

        [HttpPut("DeleteIssuedProperty")]
        public async Task<IActionResult> DeleteIssuedProperty(List<IssuedPropertyMethod> objIssued)
        {
            return Ok(await _iMiscIssuedPropertyService.DeleteIssuedProperty(objIssued));
        }
        [HttpGet("GetIssuedPropertyHistory")]
        public IActionResult GetIssuedPropertyHistory(int issuedPropertyId)
        {
            return Ok(_iMiscIssuedPropertyService.GetIssuedPropertyHistory(issuedPropertyId));
        }

    }
}