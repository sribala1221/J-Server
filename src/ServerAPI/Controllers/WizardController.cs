using Microsoft.AspNetCore.Mvc;
using ServerAPI.Services;
using ServerAPI.ViewModels;
using System.Threading.Tasks;
using ServerAPI.Authorization;
using Microsoft.AspNetCore.Authorization;

namespace ServerAPI.Controllers
{
    [Route("[controller]")]
    public class WizardController : ControllerBase
    {
        private readonly IWizardService _wizardService;

        public WizardController(IWizardService wizardService)
        {
            _wizardService = wizardService;
        }

        [HttpGet("GetWizardSteps")]
        public IActionResult GetWizardSteps()
        {

            return Ok(_wizardService.GetWizardSteps());

        }

        [HttpGet("GetAoWizardFacility")]
        public IActionResult GetAoWizardFacility(int wizardId, int facilityId)
        {
            return Ok(_wizardService.GetAoWizardFacility(wizardId, facilityId));
        }

        // [FuncPermission(415, FuncPermissionType.Edit)]
        [HttpGet("GetWizardProgress")]
        public IActionResult GetWizardProgress(int aoWizardProgressId)
        {
            return Ok(_wizardService.GetWizardProgress(aoWizardProgressId));
        }

        [HttpPost("CreateWizardProgress")]
        public IActionResult CreateWizardProgress([FromBody] AoWizardProgressVm wizardProgress)
        {
            return Ok(_wizardService.CreateWizardProgress(wizardProgress).Result);
        }

        [HttpPost("SetWizardStepComplete")]
        public IActionResult SetWizardStepComplete([FromBody] AoWizardStepProgressVm stepProgress)
        {
            return Ok(_wizardService.SetWizardStepComplete(stepProgress).Result);
        }
        //Permissions For Wizard
        [FuncPermission(477, FuncPermissionType.Edit)]
        [HttpPut("EditBookingWorkflow")]
        public IActionResult EditBookingWorkflow(int aoWizardProgressId)
        {
            return Ok();
        }

        [AllowAnonymous]
        [HttpGet("WizardStepChanged")]
        public IActionResult WizardStepChanged(int facilityId, int wizardId, string wizardName)
        {
            return Ok(_wizardService.WizardStepChanged(facilityId, wizardId, wizardName));
        }
        ////To get Wizard Step details
        //[HttpGet("GetWizardStepDetails")]
        //public IActionResult GetWizardStepDetails(int facilityId, WizardType wizardType, bool childWizard)
        //{
        //	return Ok(_wizardService.GetWizardStepDetails(facilityId, wizardType, childWizard));
        //}

        ////To get Wizard parent details
        //[HttpGet("GetWizardDetails")]
        //public IActionResult GetWizardDetails(int incarcerationId)
        //{
        //    return Ok(_wizardService.GetWizardDetails(incarcerationId));
        //}

        ////Updating the Wizard Step Id 
        //[HttpPost("UpdateIncarcerationWizardStep")]
        //public async Task<IActionResult> UpdateIncarcerationWizardStep([FromBody] IntakeVm intakeVm)
        //{
        //    return Ok(await _wizardService.UpdateIncarcerationWizardStep(intakeVm));
        //}

        ////To get Intake Wizard Complete details
        //[HttpGet("GetIntakeWizardCompleteDetails")]
        //public IActionResult GetIntakeWizardCompleteDetails(int incarcerationId)
        //{
        //    return Ok(_wizardService.GetIntakeWizardCompleteDetails(incarcerationId));
        //}

        ////Updating the Intake Wizard Complete
        //[HttpPost("UpdateIntakeWizardComplete")]
        //public async Task<IActionResult> UpdateIntakeWizardComplete(bool completeFlag, int incarcerationId)
        //{
        //    return Ok(await _wizardService.UpdateIntakeWizardComplete(completeFlag, incarcerationId));
        //}

        ////Updating the Intake Wizard Complete
        //[HttpPost("UpdateBookingDataWizard")]
        //public async Task<IActionResult> UpdateBookingDataWizard([FromBody] IntakeVm intakeVm)
        //{
        //    return Ok(await _wizardService.UpdateBookingDataWizard(intakeVm));
        //}
    }
}
