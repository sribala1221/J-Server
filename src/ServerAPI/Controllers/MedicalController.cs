using Microsoft.AspNetCore.Mvc;
using ServerAPI.Authorization;
using ServerAPI.Services;
using ServerAPI.ViewModels;
using System.Threading.Tasks;

namespace ServerAPI.Controllers
{
    [Route("[controller]")]
    public class MedicalController : ControllerBase
    {
        private IMedicalPreScreenService _medicalPreScreenService;
       private IMedicalAlertsFlagViewerService _medicalAlertsFlagViewerService;
        private IMedicalAlertsService _medicalAlertService;
        public MedicalController(IMedicalPreScreenService medicalPreScreenService,IMedicalAlertsService medicalAlertService,
        IMedicalAlertsFlagViewerService medicalAlertsFlagViewerService)
        {
            _medicalPreScreenService = medicalPreScreenService;
            _medicalAlertService=medicalAlertService;
            _medicalAlertsFlagViewerService=medicalAlertsFlagViewerService;
        }

        [HttpGet("GetMedicalPrescreen")]
        public IActionResult GetMedicalPrescreen(MedicalPrescreenVm medicalPreSearch)
        {
            return Ok(_medicalPreScreenService.GetMedicalPrescreen(medicalPreSearch));
        }

        [FuncPermission(6030, FuncPermissionType.Edit)]
        [HttpPost("UpdateMedicalPreScreenFormRecord")]
        public async Task<IActionResult> UpdateMedicalPreScreenFormRecord([FromBody] int formRecordId)
        {
            return Ok(await _medicalPreScreenService.UpdateMedicalPreScreenFormRecord(formRecordId));
        }

        [FuncPermission(6030, FuncPermissionType.Delete)]
        [HttpPut("DeleteMedicalPrescreen")]
        public IActionResult DeleteMedicalPrescreen([FromBody]int formRecordId, [FromQuery]bool deleteflag)
        {
            return Ok(_medicalPreScreenService.DeleteMedicalPrescreen(formRecordId, deleteflag));
        }

        [FuncPermission(6071, FuncPermissionType.Access)]
        [HttpGet("GetMedicalAlertInmate")]
        public IActionResult GetMedicalAlertInmate([FromQuery] MedicalAlertInmateVm inputs)
        {
            return Ok(_medicalAlertService.GetMedicalAlertInmate(inputs));
        }

        [HttpGet("GetMedicalAlertsFlagViewers")]
        public IActionResult GetMedicalAlertsFlagViewers(MedicalAlertInmateVm inputs)
        {
            return Ok(_medicalAlertsFlagViewerService.GetMedicalAlertsFlagViewers(inputs));
        }
    }
}