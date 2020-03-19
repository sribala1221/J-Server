using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ServerAPI.Authorization;
using ServerAPI.Services;
using ServerAPI.ViewModels;

namespace ServerAPI.Controllers
{
    [Route("[controller]")]
    public class MedicalChartController : ControllerBase
    {
        private readonly IMedicalFormsService _medicalFormsService;

        public MedicalChartController(IMedicalFormsService medicalFormsService)
        {
            _medicalFormsService = medicalFormsService;
        }

        [HttpGet("GetMedicalCategory")]
        public IActionResult GetMedicalCategory([FromQuery] MedicalFormsParam inputs)
        {
            return Ok(_medicalFormsService.GetMedicalCategory(inputs));
        }

        [FuncPermission(6041, FuncPermissionType.Add)]
        [HttpPost("InsertFormRecord")]
        public async Task<IActionResult> InsertFormRecord([FromBody] MedicalFormsVm medicalForms)
        {
            return Ok(await _medicalFormsService.InsertFormRecord(medicalForms));
        }

        [FuncPermission(6040, FuncPermissionType.Edit)]
        [HttpPut("UpdateFormRecord")]
        public async Task<IActionResult> UpdateFormRecord([FromBody] int formRecordId,
            [FromBody] MedicalFormsVm medicalForms)
        {
            return Ok(await _medicalFormsService.UpdateFormRecord(formRecordId, medicalForms));
        }

        [FuncPermission(6040, FuncPermissionType.Delete)]
        [HttpPost("DeleteUndoFormRecord")]
        public async Task<IActionResult> DeleteUndoFormRecord([FromBody] MedicalFormsParam inputs)
        {
            return Ok(await _medicalFormsService.DeleteUndoFormRecord(inputs));
        }
    }
}
