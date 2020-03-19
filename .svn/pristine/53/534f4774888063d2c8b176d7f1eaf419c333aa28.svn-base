using Microsoft.AspNetCore.Mvc;
using ServerAPI.Services;
using ServerAPI.ViewModels;
using System.Threading.Tasks;
using ServerAPI.Authorization;

namespace ServerAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class HousingController : ControllerBase
    {
        private readonly IHousingService _housingService;
        private readonly IHousingStatsService _housingStatsService;
        private readonly IHousingConflictService _housingConflictService;
        private readonly IFacilityPrivilegeService _facilityPrivilegeService;

        public HousingController(IHousingService housingService, IHousingStatsService housingStatsService,
            IHousingConflictService housingConflictService, IFacilityPrivilegeService facilityPrivilegeService)
        {
            _housingService = housingService;
            _housingStatsService = housingStatsService;
            _housingConflictService = housingConflictService;
            _facilityPrivilegeService = facilityPrivilegeService;
        }
      
        [HttpGet("GetInmateHousingDetails")]
        public IActionResult GetInmateHousingDetails(int inmateId)
        {
            return Ok(_housingService.GetInmateHousingDetails(inmateId));
        }

        [HttpGet("GetFacilityHousingDetails")]
        public IActionResult GetFacilityHousingDetails(int inmateId, int facilityId)
        {
            return Ok(_housingService.GetFacilityHousingDetails(inmateId, facilityId));
        }

        [HttpGet("GetHousingDetails")]
        public IActionResult GetHousingDetails([FromQuery] HousingInputVm housingInput)
        {
            return Ok(_housingService.GetHousingDetails(housingInput));
        }

        [HttpGet("GetStatsInmateDetails")]
        public IActionResult GetStatsInmateDetails([FromQuery] HousingStatsInputVm housingStatsInput)
        {
            return Ok(_housingStatsService.GetStatsInmateDetails(housingStatsInput));
        }
        [HttpGet("GetRecommendHousing")]
        public IActionResult GetRecommendHousing(int facilityId, int inmateId)
        {
            return Ok(_housingService.GetRecommendHousing(facilityId, inmateId));
        }

        [HttpGet("GetHousingFacility")]
        public IActionResult GetHousingFacility(int facilityId)
        {
            return Ok(_housingService.GetHousingFacility(facilityId));
        }

        [HttpGet("GetHousingFacilityDetailsForFacility")]
        public IActionResult GetHousingFacilityDetailsForFacility(int facilityId)
        {
            return Ok(_housingService.GetHousingFacilityDetailsForFacility(facilityId));
        }

        [HttpGet("GetHousingFacilityDetailsForBuilding")]
        public IActionResult GetHousingFacilityDetailsForBuilding(int facilityId)
        {
            return Ok(_housingService.GetHousingFacilityDetailsForBuilding(facilityId));
        }

        [HttpGet("GetHousingFacilityDetailsForInmates")]
        public IActionResult GetHousingFacilityDetailsForInmates(int facilityId)
        {
            return Ok(_housingService.GetHousingFacilityDetailsForInmates(facilityId));
        }

        [HttpGet("GetHousingFacilityDetailsForInternalExternalLocation")]
        public IActionResult GetHousingFacilityDetailsForInternalExternalLocation(int facilityId)
        {
            return Ok(_housingService.GetHousingFacilityDetailsForInternalExternalLocation(facilityId));
        }

        [HttpGet("GetFacilityLocationDetails")]
        public IActionResult GetFacilityLocationDetails(int locationId, int facilityId)
        {
            return Ok(_housingService.GetFacilityLocationDetails(locationId, facilityId));
        }

        [FuncPermission(770, FuncPermissionType.Access)]
        [HttpGet("GetHousingInmateHistory")]
        public IActionResult GetHousingInmateHistory([FromQuery] HousingInputVm value)
        {
            return Ok(_housingService.GetHousingInmateHistory(value));
        }
        [HttpGet("GetNoHousingDetails")]
        public IActionResult GetNoHousingDetails(int facilityId)
        {
            return Ok(_housingService.GetNoHousingDetails(facilityId));
        }

        [HttpGet("GetBuildingDetails")]
        public IActionResult GetBuildingDetails(int facilityId)
        {
            return Ok(_housingService.GetBuildingDetails(facilityId));
        }

        [FuncPermission(499, FuncPermissionType.Edit)]
        [HttpPost("InsertHousingAssign")]
        public async Task<IActionResult> InsertHousingAssign( HousingAssignVm housingAssign)
        {
            return Ok(await _housingService.InsertHousingAssign(housingAssign));
        }

        [FuncPermission(498, FuncPermissionType.Edit)]
        [HttpPost("InsertHousingUnAssign")]
        public async Task<IActionResult> InsertHousingUnAssign( HousingAssignVm housingAssign)
        {
            return Ok(await _housingService.InsertHousingAssign(housingAssign));
        }

        [HttpGet("GetKeepSepDetails")]
        public IActionResult GetKeepSepDetails(int inmateId, int facilityId,string housingUnitLocation,int housingUnitListId)
        {
            return Ok(_housingConflictService.GetKeepSepDetails(inmateId, facilityId, housingUnitLocation, housingUnitListId));
        }

        [HttpGet("GetLocationInmateDetails")]
        public IActionResult GetLocationInmateDetails([FromQuery] HousingInputVm housingInputVm)
        {
            return Ok(_housingService.GetLocationInmateDetails(housingInputVm));
        }

        [HttpGet("GetFacilityInmateDetails")]
        public IActionResult GetFacilityInmateDetails(int facilityId)
        {
            return Ok(_housingService.GetFacilityInmateDetails(facilityId));
        }

        [FuncPermission(492, FuncPermissionType.Edit)]
        [HttpGet("GetHousingInmateTrack")]
        public IActionResult GetHousingInmateTrack()
        {
            return Ok();
        }

        [FuncPermission(494, FuncPermissionType.Add)]
        [HttpGet("GetHousingInmateNote")]
        public IActionResult GetHousingInmateNote()
        {
            return Ok();
        }
       

        [HttpGet("GetHousingInmateLabel")]
        public IActionResult GetHousingInmateLabel()
        {
            return Ok();
        }

        [HttpGet("GetHousingInmateIssuedProperty")]
        public IActionResult GetHousingInmateIssuedProperty()
        {
            return Ok();
        }

        [HttpGet("GetHousingInmateDetail")]
        public IActionResult GetHousingInmateDetail()
        {
            return Ok();
        }
        [HttpGet("GetPodHousingDetails")]
        public IActionResult GetPodHousingDetails([FromQuery] HousingInputVm housingInput)
        {
            return Ok(_housingService.GetPodHousingDetails(housingInput));
        }
        [HttpGet("GetTrackingLocationList")]
        public IActionResult GetTrackingLocationList(int facilityId)
        {
            return Ok(_facilityPrivilegeService.GetTrackingLocationList(facilityId));
        }

        [HttpGet("GetInmateHousingConflictVm")]
        public IActionResult GetInmateHousingConflictVm([FromQuery] HousingInputVm value)
        {
            return Ok(_housingConflictService.GetInmateHousingConflictVm(value));
        }

        [FuncPermission(499, FuncPermissionType.Edit)]
        [HttpGet("GetHousingAssignDetail")]
        public IActionResult GetHousingAssignDetail()
        {
            return Ok();
        }

        [FuncPermission(498, FuncPermissionType.Edit)]
        [HttpGet("GetHousingUnAssignDetail")]
        public IActionResult GetHousingUnAssignDetail()
        {
            return Ok();
        }

        [HttpGet("GetHousingConflictNotification")]
        public IActionResult GetHousingConflictNotification([FromQuery] HousingInputVm value)
        {
            return Ok(_housingConflictService.GetHousingConflictNotification(value));
        }

        [HttpGet("GetHousingData")]
        public IActionResult GetHousingData(int housingUnitListId)
        {
            return Ok(_housingService.GetHousingData(housingUnitListId));
        }

        [HttpGet("GetHousingNextLevel")]
        public IActionResult GetHousingNextLevel(int housingUnitListId)
        {
            return Ok(_housingService.GetHousingNextLevel(housingUnitListId));
        }

        [HttpGet("GetHousingNumberDetails")]
        public IActionResult GetHousingNumberDetails([FromQuery] HousingInputVm housingInput)
        {
            return Ok(_housingService.GetHousingNumberDetails(housingInput));
        }

        [HttpGet("GetInmateCurrentDetails")]
        public IActionResult GetInmateCurrentDetails(int inmateId)
        {
            return Ok(_housingService.GetInmateCurrentDetails(inmateId));
        }
    }
}
