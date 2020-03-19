using Microsoft.AspNetCore.Mvc;
using ServerAPI.Services;
using ServerAPI.ViewModels;
using System.Threading.Tasks;
using System.Collections.Generic;
// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ServerAPI.Controllers
{
    [Route("[controller]")]
      [ApiController]
    public class ProgramRequestController : ControllerBase
    {
        private readonly IProgramRequestService _programRequestService;
        public ProgramRequestController(IProgramRequestService programRequestService)
        {
            _programRequestService = programRequestService;
        }
       
        [HttpGet("GetProgramRequestEligibility")]
        public IActionResult GetProgramRequestEligibility(int facilityId, int inmateId, int programId)
        {
            return Ok(_programRequestService.GetProgramRequestEligibility(facilityId, inmateId, programId));
        }
       
        [HttpGet("GetProgramClassNameList")]
        public IActionResult GetProgramClassNameList(int facilityId, int inmateId)
        {
            return Ok(_programRequestService.GetProgramClassNameList(facilityId, inmateId));
        }

        [HttpGet("GetProgramClassList")]
        public IActionResult GetProgramClassList(int facilityId, int inmateId)
        {
            return Ok(_programRequestService.GetProgramClassList(facilityId, inmateId));
        }

        [HttpPost("SaveProgramRequestDetails")]
        public async Task<IActionResult> SaveProgramRequestDetails([FromBody] ProgramRequestInputVm value)
        {
            return Ok(await _programRequestService.SaveProgramRequestDetails(value));
        }
        
        [HttpGet("ProgramRequestValid")]
        public IActionResult ProgramRequestValid(int programId, int inmateId)
        {
            return Ok(_programRequestService.ProgramRequestValid(programId, inmateId));
        }
              
        [HttpPost("SaveDenyandSentProgramRequest")]
        public async Task<IActionResult> SaveDenyandSentProgramRequest(  ProgramCatogoryVm Inputs)
        {
            return Ok(await _programRequestService.SaveDenyandSentProgramRequest(Inputs));
        }

        [HttpPost("SaveAppealProgramRequest")]
        public async Task<IActionResult> SaveAppealProgramRequest(  ProgramCatogoryVm Inputs)
        {
            return Ok(await _programRequestService.SaveAppealProgramRequest(Inputs));
        }
         [HttpPost("SaveAssignRequest")]
        public async Task<IActionResult> SaveAssignRequest(  List<ProgramEligibility> Inputs)
        {
            return Ok(await _programRequestService.SaveAssignRequest(Inputs));
        }

        [HttpPut("DeleteDenied")]
        public async Task<IActionResult> DeleteDenied(int Inputs, bool DeleteFlag)
        {
            return Ok(await _programRequestService.DeleteDenied(Inputs, DeleteFlag));
        }

        [HttpPost("DeleteProgramRequest")]
        public async Task<IActionResult> DeleteProgramRequest(  ProgramCatogoryVm ProgramRequest)
        {
            return Ok(await _programRequestService.DeleteProgramRequest(ProgramRequest));
        }

        [HttpGet("GetProgramRequestAppeal")]
        public IActionResult GetProgramRequestAppeal(int facilityId)
        {
            return Ok(_programRequestService.GetProgramRequestAppeal(facilityId));
        }

        [HttpPost("SaveAppealAssignRequest")]
        public async Task<IActionResult> SaveAppealAssignRequest(ProgramEligibility Inputs)
        {
            return Ok(await _programRequestService.SaveAppealAssignRequest(Inputs));
        }

         [HttpGet("GetRequestByCourse")]
        public IActionResult GetRequestByCourse(int programCourseId)
        {
            return Ok(_programRequestService.GetRequestByCourse(programCourseId));
        }
        
          [HttpGet("GetRequestByInmate")]
        public IActionResult GetRequestByInmate(int inmateId)
        {
            return Ok(_programRequestService.GetRequestByInmate(inmateId));
        }

           [HttpPost("SaveApproveRequest")]
        public async Task<IActionResult> SaveApproveRequest( ProgramCatogoryVm Inputs)
        {
            return Ok(await _programRequestService.SaveApproveRequest(Inputs));
        }

              [HttpGet("GetRequestCount")]
            public IActionResult GetRequestCount(int programCourseId)
        {
            return Ok(_programRequestService.GetRequestCount(programCourseId));
        }
    }
}
