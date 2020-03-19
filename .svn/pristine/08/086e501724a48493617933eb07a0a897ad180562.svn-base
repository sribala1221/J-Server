using Microsoft.AspNetCore.Mvc;
using ServerAPI.Authorization;
using ServerAPI.Services;
using ServerAPI.ViewModels;
using System.Threading.Tasks;

namespace ServerAPI.Controllers
{
    [Route("[controller]")]
    public class CourtCommitController : ControllerBase
    {
        private readonly ICourtCommitService _courtCommitService;
        public CourtCommitController(ICourtCommitService courtCommitService)
        {
            _courtCommitService = courtCommitService;
        }

        [HttpGet("CourtCommitHistoryDetails")]
        public IActionResult CourtCommitHistoryDetails([FromQuery]CourtCommitHistorySearchVm searchvalue) => 
            Ok(_courtCommitService.CourtCommitHistoryDetails(searchvalue));

        [HttpDelete("CourtCommitUpdateDelete")]
        public async Task<IActionResult> CourtCommitUpdateDelete(int inmatePreBookId) => 
            Ok(await _courtCommitService.CourtCommitUpdateDelete(inmatePreBookId));

        [HttpGet("CourtCommitSentDetails")]
        public IActionResult CourtCommitSentDetails(int incarcerationId, int arrestId, int inmateId, int inmatePrebookId) => 
            Ok(_courtCommitService.CourtCommitSentDetails(incarcerationId, arrestId, inmateId, inmatePrebookId));

        [HttpGet("GetIncidentWizard")]
        public IActionResult GetIncidentWizard() => Ok(_courtCommitService.GetIncidentWizard());

        [HttpPost("WizardComplete")]
        public async Task<IActionResult> WizardComplete([FromBody]CourtCommitHistoryVm courtCommitVm) => 
            Ok(await _courtCommitService.WizardComplete(courtCommitVm));

        [FuncPermission(900, FuncPermissionType.Add)]
        [HttpPost("AddNewCommit")]
        public IActionResult AddNewCommit() => Ok();
        [FuncPermission(900, FuncPermissionType.Edit)]
        [HttpPost("EditNewCommit")]
        public IActionResult EditNewCommit() => Ok();

        [FuncPermission(900, FuncPermissionType.Delete)]
        [HttpDelete("CourtCommitDelete")]
        public IActionResult CourtCommitDelete() => Ok();
        [FuncPermission(900, FuncPermissionType.Access)]
        [HttpPut("CourtCommitAccess")]
        public IActionResult CourtCommitAccess() => Ok();

        [FuncPermission(901, FuncPermissionType.Edit)]
        [HttpGet("CourtCommitWizard")]
        public IActionResult CourtCommitWizard() => Ok();

        [FuncPermission(902, FuncPermissionType.Add)]
        [HttpGet("CourtCommitForms")]
        public IActionResult CourtCommitForms() => Ok();

        [FuncPermission(902, FuncPermissionType.Edit)]
        [HttpGet("CourtCommitFormsEdit")]
        public IActionResult CourtCommitFormsEdit() => Ok();

        [FuncPermission(902, FuncPermissionType.Delete)]
        [HttpGet("CourtCommitFormsDelete")]
        public IActionResult CourtCommitFormsDelete() => Ok();

        [FuncPermission(402, FuncPermissionType.Delete)]
        [HttpDelete("CourtCommitScheduleQueue")]
        public IActionResult CourtCommitScheduleQueue() => Ok();
        [FuncPermission(402, FuncPermissionType.Delete)]
        [HttpDelete("CourtCommitOverdueQueue")]
        public IActionResult CourtCommitOverdueQueue() => Ok();

        [FuncPermission(422, FuncPermissionType.Edit)]
        [HttpGet("CourtCommitUndoIdentity")]
        public IActionResult CourtCommitUndoIdentity() => Ok();

        [FuncPermission(903, FuncPermissionType.Add)]
        [HttpPost("AddNewAttachment")]
        public IActionResult AddNewAttachment() => Ok();

        [FuncPermission(903, FuncPermissionType.Edit)]
        [HttpPut("EditAttachment")]
        public IActionResult EditAttachment() => Ok();

        [FuncPermission(903, FuncPermissionType.Delete)]
        [HttpDelete("DeleteAttachment")]
        public IActionResult DeleteAttachment() => Ok();
    }
}