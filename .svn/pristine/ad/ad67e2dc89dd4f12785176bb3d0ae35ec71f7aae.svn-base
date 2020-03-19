using Microsoft.AspNetCore.Mvc;
using ServerAPI.Authorization;
using ServerAPI.Services;
using ServerAPI.ViewModels;
using System.Threading.Tasks;

namespace ServerAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class MedicalNotesController : ControllerBase
    {
        private IMedicalNotesService _medicalNotesService;

        public MedicalNotesController(IMedicalNotesService medicalNotesService)
        {
            _medicalNotesService = medicalNotesService;
        }

        [HttpGet("GetMedicalNotes")]
        public IActionResult GetMedicalNotes([FromQuery] MedicalNotes medicalNoteSearch)
        {
            return Ok(_medicalNotesService.GetMedicalNotes(medicalNoteSearch));
        }
        [HttpGet("GetMedicalTypes")]
        public IActionResult GetMedicalTypes()
        {
            return Ok(_medicalNotesService.GetMedicalTypes());
        }
        [HttpPut("EditMedicalNotes")]
        [FuncPermission(6060, FuncPermissionType.Edit)]
        public IActionResult EditMedicalNotes()
        {
            return Ok();
        }

        [HttpPut("AddMedicalNotes")]
        [FuncPermission(6061, FuncPermissionType.Add)]
        public IActionResult AddMedicalNotes()
        {
            return Ok();
        }


        [HttpPost("InsertUpdateMedicalNotes")]
        public async Task<IActionResult> InsertUpdateMedicalNotes(MedicalNotes medicalNotes)
        {
            return Ok(await _medicalNotesService.InsertUpdateMedicalNotes(medicalNotes));
        }

        [FuncPermission(6060, FuncPermissionType.Delete)]
        [HttpPost("DeleteUndoMedicalNotes")]
        public async Task<IActionResult> DeleteUndoMedicalNotes(MedicalNotes medicalNotes)
        {
            return Ok(await _medicalNotesService.DeleteUndoMedicalNotes(medicalNotes));
        }
    }
}