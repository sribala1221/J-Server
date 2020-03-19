using Microsoft.AspNetCore.Mvc;
using ServerAPI.Services;
using ServerAPI.ViewModels;
using GenerateTables.Models;
using ServerAPI.Authorization;

namespace ServerAPI.Controllers
{
    [Route("[controller]")]
    public class InvestigationController : ControllerBase
    {
        private readonly IInvestigationService _investigationService;

        public InvestigationController(IInvestigationService investigationService)
        {
            _investigationService = investigationService;
        }

        [FuncPermission(1166, FuncPermissionType.Access)]
        [HttpGet("GetMyInvestigations")]
        public IActionResult GetMyInvestigations([FromQuery] InvestigationInputs inputs)
        {
            return Ok(_investigationService.GetInvestigations(inputs));
        }

        [HttpGet("GetInvestigations")]
        public IActionResult GetInvestigations([FromQuery] InvestigationInputs inputs)
        {
            return Ok(_investigationService.GetInvestigations(inputs));
        }

        [FuncPermission(1166, FuncPermissionType.Access)]
        [HttpGet("GetAllInvestigations")]
        public IActionResult GetAllInvestigations([FromQuery] InvestigationInputs inputs)
        {
            return Ok(_investigationService.GetInvestigations(inputs));
        }

        [FuncPermission(1166, FuncPermissionType.Add)]
        [HttpPost("InsertUpdateInvestigation")]
        public IActionResult InsertUpdateInvestigation([FromBody] InvestigationVm iInvestigation)
        {
            return Ok(_investigationService.InsertUpdateInvestigation(iInvestigation));
        }

        // [FuncPermission(1166, FuncPermissionType.Edit)]
        [HttpPost("EditInvestigation")]
        public IActionResult EditInvestigation([FromBody] InvestigationVm iInvestigation)
        {
            return Ok(_investigationService.InsertUpdateInvestigation(iInvestigation));
        }
        
        [HttpPost("InsertUpdateInvestigationFlags")]
        public IActionResult InsertUpdateInvestigationFlags([FromBody] InvestigationFlag iInvestigation)
        {
            return Ok(_investigationService.InsertUpdateInvestigationFlags(iInvestigation));
        }

        [HttpPost("InsertUpdateInvestigationPersonnel")]
        public IActionResult InsertUpdateInvestigationPersonnel([FromBody] InvestigationPersonnelVm iInvestigation)
        {
            return Ok(_investigationService.InsertUpdateInvestigationPersonnel(iInvestigation));
        }

        [HttpPost("InsertUpdateInvestigationNotes")]
        public IActionResult InsertUpdateInvestigationNotes([FromBody] InvestigationNotesVm iInvestigation)
        {
            return Ok(_investigationService.InsertUpdateInvestigationNotes(iInvestigation));
        }

        [HttpGet("GetInvestigationAllDetails")]
        public IActionResult GetInvestigationAllDetails([FromQuery] int investigationId)
        {
            return Ok(_investigationService.GetInvestigationAllDetails(investigationId));
        }

        [HttpGet("GetInvestigationIncidents")]
        public IActionResult GetInvestigationIncidents()
        {
            return Ok(_investigationService.GetInvestigationIncidents());
        }

        [HttpPost("InsertUpdateInvestigationIncident")]
        public IActionResult InsertUpdateInvestigationIncident([FromBody] InvestigationIncident iInvestigation)
        {
            return Ok(_investigationService.InsertUpdateInvestigationIncident(iInvestigation));
        }

        [HttpGet("GetInvestigationGrievance")]
        public IActionResult GetInvestigationGrievance()
        {
            return Ok(_investigationService.GetInvestigationGrievance());
        }

        [HttpPost("InsertUpdateInvestigationGrievance")]
        public IActionResult InsertUpdateInvestigationGrievance([FromBody] InvestigationIncident iInvestigation)
        {
            return Ok(_investigationService.InsertUpdateInvestigationGrievance(iInvestigation));
        }

       [FuncPermission(1166, FuncPermissionType.Delete)]
        [HttpPost("DeleteInvestigationAttachment")]
        public IActionResult DeleteInvestigationAttachment([FromBody] int attachmentId)
        {
            return Ok(_investigationService.DeleteInvestigationAttachment(attachmentId));
        }
        [HttpPost("InsertUpdateInvestigationLink")]
        public IActionResult InsertUpdateInvestigationLink([FromBody] InvestigationLinkVm iInvestigation)
        {
            return Ok(_investigationService.InsertUpdateInvestigationLink(iInvestigation));
        }

        [HttpPost("DeleteInvestigationForms")]
        public IActionResult DeleteInvestigationForms([FromBody] int formId)
        {
            return Ok(_investigationService.DeleteInvestigationForms(formId));
        }

        [HttpPut("UpdateInvestigationComplete")]
        public IActionResult UpdateInvestigationComplete([FromBody] InvestigationVm investigation)
        {
            return Ok(_investigationService.UpdateInvestigationComplete(investigation));
        }

        [FuncPermission(1166, FuncPermissionType.Delete)]
        [HttpPut("DeleteInvestigation")]
        public IActionResult DeleteInvestigation([FromBody] InvestigationVm investigation)
        {
            return Ok(_investigationService.DeleteInvestigation(investigation));
        }

        [HttpGet("GetInvestigationHistoryDetails")]
        public IActionResult GetInvestigationHistoryDetails([FromQuery] int investigationId)
        {
            return Ok(_investigationService.GetInvestigationHistoryDetails(investigationId));
        }
        [FuncPermission(1166, FuncPermissionType.Edit)]
        [HttpGet("EditInvestigationDetails")]
        public IActionResult EditInvestigationDetails()
        {
            return Ok();
        }
        [FuncPermission(1166, FuncPermissionType.Add)]
        [HttpGet("AddInvestigationDetails")]
        public IActionResult AddInvestigationDetails()
        {
            return Ok();
        }
         [FuncPermission(1166, FuncPermissionType.Delete)]
        [HttpGet("DeleteInvestigationDetails")]
        public IActionResult DeleteInvestigationDetails()
        {
            return Ok();
        }
        [FuncPermission(1166, FuncPermissionType.Edit)]
        [HttpPost("EditInvestigationDetail")]
        public IActionResult EditInvestigationDetail()
        {
            return Ok();
        }
    }
}