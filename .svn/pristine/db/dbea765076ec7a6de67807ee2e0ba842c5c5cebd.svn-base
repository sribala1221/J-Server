using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ServerAPI.Services;
using ServerAPI.ViewModels;
using ServerAPI.Authorization;

namespace ServerAPI.Controllers
{
    [Route("[controller]")]
    public class AltSentController : ControllerBase
    {
        private readonly IAltSentService _altSentService;
        private readonly ISitesService _sitesService;
        private readonly IAltSentInOutService _altSentInOutService;

        public AltSentController(IAltSentService altSentService, ISitesService sitesService,
            IAltSentInOutService altSentInOutService)
        {
            _altSentService = altSentService;
            _sitesService = sitesService;
            _altSentInOutService = altSentInOutService;
        }

        /// <summary>
        /// Get alt sentence request info for new request and existing request
        /// </summary>
        /// <param name="facilityId"></param>
        /// <param name="requestId"></param>
        /// <returns></returns>
        [FuncPermission(5050, FuncPermissionType.Access)]
        [HttpGet("GetAltSentRequest")]
        public IActionResult GetAltSentRequest(int facilityId, int? requestId)
        {
            return Ok(_altSentService.GetAltSentRequestInfo(facilityId, requestId));
        }

        /// <summary>
        /// Get alt sentence request facility list
        /// </summary>
        /// <returns></returns>
        [HttpGet("GetFacilityList")]
        public IActionResult GetFacilityList()
        {
            return Ok(_altSentService.GetFacilityList());
        }

        /// <summary>
        /// Get alt sentence program list according to facility
        /// </summary>
        /// <param name="facilityId"></param>
        /// <returns></returns>
        [HttpGet("GetAllSentProgramList")]
        public IActionResult GetAllSentProgramList(int facilityId)
        {
            return Ok(_altSentService.GetAltSentProgramList(facilityId));
        }

        /// <summary>
        /// Get alt sentence active request grid
        /// </summary>
        /// <param name="facilityId"></param>
        /// <param name="scheduleDate"></param>
        /// <returns></returns>
        [FuncPermission(5050, FuncPermissionType.Access)]
        [HttpGet("GetAltSentRequestGrid")]
        public IActionResult GetAltSentRequestGrid(int facilityId, DateTime? scheduleDate)
        {
            return Ok(_altSentService.LoadAltSentRequestDetails(facilityId, scheduleDate));
        }

        /// <summary>
        /// Insert alt sentence request details
        /// </summary>
        /// <param name="altSentRequest"></param>
        /// <returns></returns>
        [FuncPermission(5054, FuncPermissionType.Add)]
        [HttpPost("InsertAltSentRequest")]
        public async Task<IActionResult> InsertAltSentRequest([FromBody] AltSentenceRequest altSentRequest)
        {
            return Ok(await _altSentService.InsertUpdateAltSentRequest(altSentRequest));
        }

        /// <summary>
        /// Update alt sentence request
        /// </summary>
        /// <param name="altSentRequestId"></param>
        /// <param name="altSentRequest"></param>
        /// <returns></returns>
        [FuncPermission(5050, FuncPermissionType.Edit)]
        [HttpPut("UpdateAltSentRequest")]
        public async Task<IActionResult> UpdateAltSentRequest(int altSentRequestId,
            [FromBody] AltSentenceRequest altSentRequest)
        {
            return Ok(await _altSentService.InsertUpdateAltSentRequest(altSentRequest, altSentRequestId));
        }

        /// <summary>
        /// check altSent request exists or not? based on inmate and facility
        /// </summary>
        /// <param name="inmateId"></param>
        /// <param name="facilityId"></param>
        /// <returns></returns>
        [HttpGet("CheckAltSentRequestExists")]
        public IActionResult CheckAltSentRequestExists(int inmateId, int facilityId)
        {
            return Ok(_altSentService.CheckAltSentRequestExists(inmateId, facilityId));
        }

        /// <summary>
        /// Delete alt sentence active request
        /// </summary>
        /// <param name="altSentRequestId"></param>
        /// <param name="history"></param>
        /// <param name="deleteFlag"></param>
        /// <returns></returns>
        [HttpDelete("DeleteAltSentRequest")]
        public async Task<IActionResult> DeleteAltSentRequest(int altSentRequestId, string history, bool deleteFlag)
        {
            return Ok(await _altSentService.DeleteAltSentActiveRequest(altSentRequestId, history, deleteFlag));
        }

        /// <summary>
        /// Get alt sentence histories
        /// </summary>
        /// <param name="requestId"></param>
        /// <returns></returns>
        [HttpGet("GetAltSentHistories")]
        public IActionResult GetAltSentHistories(int requestId)
        {
            return Ok(_altSentService.GetAltSentHistories(requestId));
        }

        /// <summary>
        /// Get approve request details
        /// </summary>
        /// <param name="requestId"></param>
        /// <returns></returns>
        [FuncPermission(5050, FuncPermissionType.Access)]
        [HttpGet("GetApproveRequestDetails")]
        public IActionResult GetApproveRequestDetails(int requestId)
        {
            return Ok(_altSentService.GetApproveRequestInfo(requestId));
        }

        /// <summary>
        /// Approve altSent request
        /// </summary>
        /// <param name="altSentRequestId"></param>
        /// <param name="approveRequest"></param>
        /// <returns></returns>
        [FuncPermission(5056, FuncPermissionType.Edit)]
        [HttpPut("ApproveRequest")]
        public async Task<IActionResult> ApproveRequest(int altSentRequestId, [FromBody] ApproveRequest approveRequest)
        {
            return Ok(await _altSentService.ApproveAltSentRequest(altSentRequestId, approveRequest));
        }

        /// <summary>
        /// Get schedule details
        /// </summary>
        /// <param name="scheduleDetails"></param>
        /// <returns></returns>
        [HttpGet("GetScheduledDetails")]
        public IActionResult GetScheduledDetails(ScheduleDetails scheduleDetails)
        {
            return Ok(_altSentService.GetScheduleDetails(scheduleDetails));
        }

        /// <summary>
        /// Schedule alt sentence request
        /// </summary>
        /// <param name="altSentRequestId"></param>
        /// <param name="schedule"></param>
        /// <returns></returns>
        [FuncPermission(5058, FuncPermissionType.Edit)]
        [HttpPut("ScheduleRequest")]
        public async Task<IActionResult> ScheduleRequest(int altSentRequestId, [FromBody] SaveSchedule schedule)
        {
            return Ok(await _altSentService.ScheduleAltSentRequest(altSentRequestId, schedule));
        }

        /// <summary>
        /// Check alt sentence program exists
        /// </summary>
        /// <param name="inmateId"></param>
        /// <returns></returns>
        [HttpGet("CheckAltSentPgmExists")]
        public IActionResult CheckAltSentPgmExists(int inmateId)
        {
            return Ok(_altSentService.CheckAltSentPgmExists(inmateId));
        }

        /// <summary>
        /// Get flag value for alt sentence arrest not allowed
        /// </summary>
        /// <param name="arrestId"></param>
        /// <returns></returns>
        [HttpGet("GetAltSentArrestNotAllowedFlag")]
        public IActionResult GetAltSentArrestNotAllowedFlag(int arrestId)
        {
            return Ok(_altSentService.GetAltSentArrestNotAllowedFlag(arrestId));
        }

        [HttpGet("GetProgramList")]
        public IActionResult GetProgramList(int inmateId, int altSentId = 0)
        {
            return Ok(_altSentService.LoadProgramList(inmateId, altSentId));
        }

        [FuncPermission(5055, FuncPermissionType.Edit)]
        [HttpPut("RequestButtonAccess")]
        public async Task<IActionResult> RequestButtonAccess([FromBody] AltSentenceRequest altSentRequest)
        {
            return Ok(await _altSentService.InsertUpdateAltSentRequest(altSentRequest));
        }

        [FuncPermission(5059, FuncPermissionType.Access)]
        [HttpGet("AppointmentButtonAccess")]
        public IActionResult AppointmentButtonAccess()
        {
            return Ok();
        }

        //To get Alt sent Queue
        [HttpGet("GetAltSentQueueDetails")]
        public IActionResult GetAltSentQueueDetails(int facilityId)
        {
            return Ok(_altSentService.GetAltSentQueueDetails(facilityId));
        }

        [FuncPermission(5135, FuncPermissionType.Access)]
        [HttpGet("GetAltSentSchedule")]
        public IActionResult GetAltSentSchedule(int facilityId, int programId)
        {
            return Ok(_altSentService.GetAltSentSchedule(facilityId, programId));
        }

        [HttpGet("LoadPrimaryDetails")]
        public IActionResult LoadPrimaryDetails()
        {
            return Ok(_sitesService.LoadPrimaryDetails());
        }

        [HttpGet("GetScheduleDetails")]
        public IActionResult GetScheduleDetails(int programId, int? day, int? siteId)
        {
            return Ok(_sitesService.GetScheduleDetails(programId, day, siteId));
        }

        /// <summary>
        /// To get AltSent -> Interview details
        /// </summary>
        /// <param name="searchValue"></param>
        /// <returns></returns>
        [FuncPermission(5021, FuncPermissionType.Access)]
        [HttpGet("GetAltSentInterview")]
        public IActionResult GetAltSentInterview(CourtCommitHistorySearchVm searchValue)
        {
            return Ok(_altSentInOutService.GetInterviewOrScheduleBook(searchValue));
        }

        /// <summary>
        /// To get AltSent -> Schedule book details
        /// </summary>
        /// <param name="searchValue"></param>
        /// <returns></returns>
        [FuncPermission(5022, FuncPermissionType.Access)]
        [HttpGet("GetAltSentScheduleBook")]
        public IActionResult GetAltSentScheduleBook(CourtCommitHistorySearchVm searchValue)
        {
            return Ok(_altSentInOutService.GetInterviewOrScheduleBook(searchValue));
        }

        /// <summary>
        /// To get AltSent -> Interview or Schedule book - complete details
        /// </summary>
        /// <param name="inmatePrebookId"></param>
        /// <returns></returns>
        [HttpGet("GetInterviewOrScheduleBookCompleteDetails")]
        // ReSharper disable once IdentifierTypo
        public IActionResult GetInterviewOrScheduleBookCompleteDetails(int inmatePrebookId)
        {
            return Ok(_altSentInOutService.GetInterviewOrScheduleBookCompleteDetails(inmatePrebookId));
        }

        /// <summary>
        /// To update AltSent -> Interview or Schedule book - complete details
        /// </summary>
        /// <param name="courtCommitHistoryVm"></param>
        /// <returns></returns>
        [FuncPermission(5021, FuncPermissionType.Edit)]
        [HttpPut("UpdateInterviewCompleteDetails")]
        public async Task<IActionResult> UpdateInterviewCompleteDetails(
            [FromBody] CourtCommitHistoryVm courtCommitHistoryVm)
        {
            return Ok(await _altSentInOutService.UpdateInterviewOrScheduleBookCompleteDetails(courtCommitHistoryVm));
        }

        /// <summary>
        /// To update AltSent -> Interview or Schedule book - complete details
        /// </summary>
        /// <param name="courtCommitHistoryVm"></param>
        /// <returns></returns>
        [FuncPermission(5022, FuncPermissionType.Edit)]
        [HttpPut("UpdateScheduleBookCompleteDetails")]
        public async Task<IActionResult> UpdateScheduleBookCompleteDetails(
            [FromBody] CourtCommitHistoryVm courtCommitHistoryVm)
        {
            return Ok(await _altSentInOutService.UpdateInterviewOrScheduleBookCompleteDetails(courtCommitHistoryVm));
        }

        /// <summary>
        /// To get AltSent &quot; Court commit or Interview or Schedule book details
        /// </summary>
        /// <param name="inmatePrebookId"></param>
        /// <returns></returns>
        [FuncPermission(5026, FuncPermissionType.Access)]
        [HttpGet("GetScheduleAlternativeSentence")]
        // ReSharper disable once IdentifierTypo
        public IActionResult GetScheduleAlternativeSentence(int inmatePrebookId)
        {
            return Ok(_altSentInOutService.GetScheduleAlternativeSentence(inmatePrebookId));
        }

        /// <summary>
        /// To update &lt; AltSent &gt; Court commit or Interview or Schedule book details
        /// </summary>
        /// <param name="courtCommitHistoryVm">Commit &gt; History</param>
        /// <returns></returns>
        [HttpPut("UpdateScheduleAlternativeSentence")]
        public async Task<IActionResult> UpdateScheduleAlternativeSentence(
            [FromBody] CourtCommitHistoryVm courtCommitHistoryVm)
        {
            return Ok(await _altSentInOutService.UpdateScheduleAlternativeSentence(courtCommitHistoryVm));
        }
    }
}