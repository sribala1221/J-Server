using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ServerAPI.Authorization;
using ServerAPI.Services;
using ServerAPI.ViewModels;
using System.Threading.Tasks;

namespace ServerAPI.Controllers
{
    [Route("[controller]")]
    public class IntakePrebookController : ControllerBase
    {
        private readonly IIntakePrebookService _intakePrebookService;
        private readonly IPersonService _personService;

        public IntakePrebookController(IIntakePrebookService intakePrebookService, IPersonService personService)
        {
            _intakePrebookService = intakePrebookService;
            _personService = personService;
        }
#region permission for prebook

        [FuncPermission(408, FuncPermissionType.Print)]
        [HttpGet("GetIntakeExport")]
        public IActionResult GetIntakeExport()
        {
            return NoContent();
        }

        //reject medical permisssion
        [FuncPermission(404, FuncPermissionType.Add)]
        [HttpPost("InsertIntakeMedical")]
        public IActionResult InsertIntakeMedical()
        {
            return Ok();
        }

        [FuncPermission(405, FuncPermissionType.Edit)]
        [HttpPut("EditIntakeMedical")]
        public IActionResult EditIntakeMedical()
        {
            return Ok();
        }

        [FuncPermission(400, FuncPermissionType.Delete)]
        [HttpDelete("DeleteIntakeMedical")]
        public IActionResult DeleteIntakeMedical()
        {
            return Ok();
        }

        [FuncPermission(405, FuncPermissionType.Delete)]
        [HttpDelete("DeleteIntakeMedicalProgress")]
        public IActionResult DeleteIntakeMedicalProgress()
        {
            return Ok();
        }

        [FuncPermission(412, FuncPermissionType.Edit)]
        [HttpPut("EditIntakeByPassed")]
        public IActionResult EditIntakeByPassed()
        {
            return Ok();
        }


        [FuncPermission(401, FuncPermissionType.Add)]
        [HttpPost("InsertReadyIntake")]
        public IActionResult InsertReadyIntake()
        {
            return Ok();
        }

        [FuncPermission(401, FuncPermissionType.Delete)]
        [HttpDelete("DeleteReadyIntake")]
        public IActionResult DeleteReadyIntake()
        {
            return Ok();
        }
        [FuncPermission(423, FuncPermissionType.Access)]
        [HttpPost("showPrebookdata")]
        public IActionResult showPrebookdata()
        {
            return Ok();
        }
        [FuncPermission(420, FuncPermissionType.Access)]
        [HttpDelete("showPrebookForm")]
        public IActionResult showPrebookForm()
        {
            return Ok();
        }

        #endregion
        //#region permissionpolicy add
        //[FuncPermission(401, FuncPermissionType.Add)]
        //[HttpPost("GetIntakePrebookReady")]
        //public IActionResult GetIntakePrebookReady()
        //{
        //    return Ok();
        //}
        //[FuncPermission(402, FuncPermissionType.Add)]
        //[HttpPost("GetCourtCommitSchd")]
        //public IActionResult GetCourtCommitSchd()
        //{
        //    return Ok();
        //}
        //[FuncPermission(403, FuncPermissionType.Add)]
        //[HttpPost("GetCourtCommitOverdue")]
        //public IActionResult GetCourtCommitOverdue()
        //{
        //    return Ok();
        //}
        //[FuncPermission(404, FuncPermissionType.Add)]
        //[HttpPost("GetMedicallyRejected")]
        //public IActionResult GetMedicallyRejected()
        //{
        //    return Ok();
        //}
        //[FuncPermission(404, FuncPermissionType.Add)]
        //[HttpPost("GetIntakeInProgress")]
        //public IActionResult GetIntakeInProgress()
        //{
        //    return Ok();
        //}
        [FuncPermission(406, FuncPermissionType.Add)]
        [HttpPost("GetReadyIntake")]
        public IActionResult GetReadyIntake()
        {
            return Ok();
        }
        [FuncPermission(407, FuncPermissionType.Add)]
        [HttpPost("GetTempHold")]
        public IActionResult GetTempHold()
        {
           return Ok();
        }
        //#endregion

        #region permissionpolicy edit
        //[FuncPermission(405, FuncPermissionType.Edit)]
        //[HttpPut("GetIntakeInProgressEdit")]
        //public IActionResult GetIntakeInProgressEdit()
        //{
        //    return Ok();
        //}

        [FuncPermission(412, FuncPermissionType.Edit)]
        [HttpPut("GetIntakeByPassedEdit")]
        public IActionResult GetIntakeByPassedEdit()
        {
            return Ok();
        }
        [FuncPermission(413, FuncPermissionType.Edit)]
        [HttpPut("GetIntakeNotRequiredEdit")]
        public IActionResult GetIntakeNotRequiredEdit()
        {
            return Ok();
        }
        #endregion
        #region 
        [FuncPermission(414, FuncPermissionType.Edit)]
        [HttpPut("GetIntakeIdentityEdit")]
        public IActionResult GetIntakeIdentityEdit()
        {
            return Ok();
        }
        #endregion

        #region permissionpolicy delete
        [FuncPermission(401, FuncPermissionType.Delete)]
        [HttpDelete("GetIntakePrebookReadyDel")]
        public IActionResult GetIntakePrebookReadyDel()
        {
            return Ok();
        }
        [FuncPermission(402, FuncPermissionType.Delete)]
        [HttpDelete("GetCourtCommitSchdDel")]
        public IActionResult GetCourtCommitSchdDel()
        {
            return Ok();
        }
        [FuncPermission(403, FuncPermissionType.Delete)]
        [HttpDelete("GetCourtCommitOverdueDel")]
        public IActionResult GetCourtCommitOverdueDel()
        {
            return Ok();
        }
        [FuncPermission(404, FuncPermissionType.Delete)]
        [HttpDelete("GetMedicallyRejectedDel")]
        public IActionResult GetMedicallyRejectedDel()
        {
            return Ok();
        }
        [FuncPermission(405, FuncPermissionType.Delete)]
        [HttpDelete("GetIntakeInProgressDel")]
        public IActionResult GetIntakeInProgressDel()
        {
            return Ok();
        }

        //[FuncPermission(412, FuncPermissionType.Delete)]
        //[HttpDelete("GetIntakeByPassedDel")]
        //public IActionResult GetIntakeByPassedDel()
        //{
        //    return Ok();
        //}
        [FuncPermission(406, FuncPermissionType.Delete)]
        [HttpDelete("GetReadyIntakeDel")]
        public IActionResult GetReadyIntakeDel()
        {
            return Ok();
        }
        [FuncPermission(407, FuncPermissionType.Delete)]
        [HttpDelete("GetTempHoldDel")]
        public IActionResult GetTempHoldDel()
        {
            return Ok();
        }
        #endregion

        #region LoadGrid

        #region GetIntakePrebookCount

        /// <summary>
        /// To get the intake prebook count based on Facility
        /// </summary>
        /// <param name="facilityId"></param>
        /// <param name="inmatePrebookId"></param>
        /// <param name="queueName"></param>
        /// <param name="active"></param>
        /// <returns></returns>
        [HttpGet("GetIntakePrebookCount")]
        public IActionResult GetIntakePrebookCount(int facilityId, int inmatePrebookId, string queueName, bool active)
        {
            return Ok(_intakePrebookService.GetIntakePrebookCount(facilityId, inmatePrebookId, queueName, active));
        }

        [HttpGet("GetListPrebookDetails")]
        public IActionResult GetListPrebookDetails(int facilityId, PrebookDetails prebookSelected, string courtcommitoverdueDate)
        {
            return Ok(_intakePrebookService.GetListPrebookDetails(facilityId, prebookSelected, courtcommitoverdueDate));
        }

        #endregion

        #endregion

        #region GridFunctionality

        //Set Delete Flag in InmatePrebook
        [HttpPost("DeleteInmatePrebook")]
        public async Task<IActionResult> DeleteInmatePrebook([FromBody] int inmatePrebookId)
        {
            return Ok(await _intakePrebookService.DeleteInmatePrebook(inmatePrebookId));
        }

        //Remove Delete Flag in InmatePrebook
        [HttpPost("UndoInmatePrebook")]
        public async Task<IActionResult> UndoInmatePrebook([FromBody] int inmatePrebookId)
        {
            return Ok(await _intakePrebookService.UndoInmatePrebook(inmatePrebookId));
        }

        //Set Temporary Hold Flag in InmatePrebook
        [FuncPermission(407,FuncPermissionType.Edit)]
        [HttpPost("UpdateTemporaryHold")]
        public async Task<IActionResult> UpdateTemporaryHold([FromBody] int inmatePrebookId)
        {
            return Ok(await _intakePrebookService.UpdateTemporaryHold(inmatePrebookId));
        }

        //Remove Temporary Hold Flag in InmatePrebook
        [FuncPermission(406,FuncPermissionType.Edit)]
        [HttpPost("RemoveTemporaryHold")]
        public async Task<IActionResult> RemoveTemporaryHold([FromBody] int inmatePrebookId)
        {
            return Ok(await _intakePrebookService.RemoveTemporaryHold(inmatePrebookId));
        }

        //Set Intake Prebook Person Name
        [HttpPost("UpdateInmatePrebookPersonDetails")]
        public async Task<IActionResult> UpdateInmatePrebookPersonDetails([FromBody] InmatePrebookVm intakePrebook)
        {
            Debug.Assert(intakePrebook.PersonId != null, "intakePrebook.PersonId != null");
            return Ok(await _intakePrebookService.UpdateInmatePrebookPersonDetails(intakePrebook.InmatePrebookId,
                intakePrebook.PersonId.Value));
        }


        //Set Intake Prebook Medical screen
        [HttpPost("UpdateMedicalPrescreenPrebook")]
        public async Task<IActionResult> UpdateMedicalPrescreenPrebook([FromBody] int inmatePrebookId)
        {
            return Ok(await _intakePrebookService.UpdateMedicalPrescreenPrebook(inmatePrebookId));
        }

        //Get Intake Prebook
        [HttpGet("GetInmatePrebookDetails")]
        public IActionResult GetInmatePrebookDetails(int inmatePrebookId)
        {
            return Ok(_intakePrebookService.GetInmatePrebookDetails(inmatePrebookId));
        }

        //Get Person Details
        [HttpGet("GetPersonDetails")]
        public IActionResult GetPersonDetails(int personId)
        {
            return Ok(_personService.GetPersonDetails(personId));
        }

        #endregion

        #region NewIntake

        //Creating New Intake
        [HttpPost("InsertIntakeProcess")]
        public async Task<IActionResult> InsertIntakeProcess([FromBody]IntakeEntryVm obj)
        {
            return Ok(await _intakePrebookService.InsertIntakeProcess(obj));
        }

        #endregion

        #region Set Complete
        [FuncPermission(411, FuncPermissionType.Edit)]
        [HttpPost("SetIntakeProcessComplete")]
        public async Task<IActionResult> SetIntakeProcessComplete([FromBody]int incarcerationId)
        {
            return Ok(await _intakePrebookService.SetIntakeProcessComplete(incarcerationId));
        }
        [HttpPost("SetIntakeProcessUndoComplete")]
        public async Task<IActionResult> SetIntakeProcessUndoComplete([FromBody]int incarcerationId)
        {
            return Ok(await _intakePrebookService.SetIntakeProcessUndoComplete(incarcerationId));
        }

        #endregion

        #region InmatePrebookCase
        [HttpGet("GetIntakePrebookCase")]
        public IActionResult GetIntakePrebookCase(int inmatePrebookId)
        {
            return Ok(_intakePrebookService.GetIntakePrebookCase(inmatePrebookId));
        }
        #endregion

        #region ApprovePrebook
        [HttpPut("ApprovePrebook")]
        public async Task<IActionResult> ApprovePrebook([FromBody]InmatePrebookReviewVm inmatePrebookReview)
        {
            return Ok(await _intakePrebookService.ApprovePrebook(inmatePrebookReview));
        }
        #endregion

        #region IdentityAccept
        [HttpPut("IdentityAccept")]
        public async Task<IActionResult> SetIdentityAccept([FromBody] InmatePrebookVm inmatePrebook)
        {
            return Ok(await _intakePrebookService.SetIdentityAccept(inmatePrebook));
        }
        
        [HttpPut("UpdateByPassedAndNotRequiredMedical")]
        public async Task<IActionResult> UpdateByPassedAndNotRequiredMedical(int inmatePrebookId, int medPrescreenStatusFlag)
        {
            return Ok(await _intakePrebookService.UpdateByPassedAndNotRequiredMedical(inmatePrebookId, medPrescreenStatusFlag));
        }
        #endregion

        [HttpGet("GetPrebookChargesAndWarrantCount")]
        public IActionResult GetPrebookChargesAndWarrantCount(int incarcerationId, int arrestId)
        {
            return Ok(_intakePrebookService.GetPrebookChargesAndWarrantCount(incarcerationId, arrestId));
        } 
        [FuncPermission(415,FuncPermissionType.Edit)]
        [HttpPut("EditIntakeReview")]
        public IActionResult EditIntakeReview()
        {
            return Ok();
        }     

    }
}
