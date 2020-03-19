using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ServerAPI.Authorization;
using ServerAPI.Services;
using ServerAPI.ViewModels;

namespace ServerAPI.Controllers
{
    [Route("[controller]")]
    public class FacilityController : ControllerBase
    {
        private readonly ICellService _cellService;
        private readonly IFacilityPrivilegeService _facilityPrivilegeService;
        private readonly IFacilityHousingService _facilityHousingService;

        public FacilityController(ICellService cellService, IFacilityPrivilegeService facilityPrivilegeService,
            IFacilityHousingService facilityHousingService)
        {
            _cellService = cellService;
            _facilityPrivilegeService = facilityPrivilegeService;
            _facilityHousingService = facilityHousingService;
        }

        [HttpPost("GetMyLogDetailsCount")]
        public IActionResult GetMyLogDetailsCount([FromBody] MyLogRequestVm mylog)
        {
            return Ok(_cellService.GetMyLogDetailsCount(mylog));
        }

        [FuncPermission(1050, FuncPermissionType.Access)]
        [HttpPost("GetMyLogDetails")]
        public IActionResult GetMyLogDetails([FromBody] MyLogRequestVm logReqDetails)
        {
            return Ok(_cellService.GetMyLogDetails(logReqDetails));
        }

        [HttpPost("SetCurrentStatus")]
        public async Task<IActionResult> SetCurrentStatus([FromBody] MyLogRequestVm statusReq)
        {
            return Ok(await _cellService.SetCurrentStatus(statusReq));
        }

        [FuncPermission(1050, FuncPermissionType.Add)]
        [HttpPost("AddLogDetails")]
        public async Task<IActionResult> AddLogDetails([FromBody] MyLogRequestVm logDetails)
        {
            return Ok(await _cellService.AddLogDetails(logDetails));
        }

        [HttpPost("ClearAttendanceDetails")]
        public async Task<IActionResult> ClearAttendanceDetails([FromBody] MyLogRequestVm attendance)
        {
            return Ok(await _cellService.ClearAttendanceDetails(attendance));
        }

        [HttpPost("SetHousingDetails")]
        public async Task<IActionResult> SetHousingDetails([FromBody] MyLogRequestVm setHousingReq)
        {
            return Ok(await _cellService.SetHousingDetails(setHousingReq));
        }

        //Facility - Privilege
        [HttpGet("getPrivilege")]
        public IActionResult GetPrivilege(FacilityPrivilegeInput input)
        {
            return Ok(_facilityPrivilegeService.GetPrivilegeByOfficer(input));
        }

        [HttpGet("LoadReviewHistory")]
        public IActionResult GetReviewHistory(int inmatePrivilegeXrefId)
        {
            return Ok(_facilityPrivilegeService.GetReviewHistory(inmatePrivilegeXrefId));
        }

        [HttpPost("SaveReviewHistory")]
        public async Task<IActionResult> InsertReviewHistory([FromBody]InmatePrivilegeReviewHistoryVm inmatePrivilege)
        {
            return Ok(await _facilityPrivilegeService.InsertReviewHistory(inmatePrivilege));
        }

        [HttpGet("GetLocationList")]
        public IActionResult GetLocationList(int facilityId)
        {
            return Ok(_facilityPrivilegeService.GetLocationList(facilityId));
        }

        [HttpGet("GetHousing")]
        public IActionResult GetHousing(int facilityId)
        {
            return Ok(_facilityHousingService.GetHousing(facilityId));
        }
    }
}
