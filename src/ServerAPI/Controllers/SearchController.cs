using Microsoft.AspNetCore.Mvc;
using ServerAPI.Services;
using ServerAPI.ViewModels;

namespace ServerAPI.Controllers
{
    [Route("[controller]")]
    public class SearchController : ControllerBase
    {
        private readonly ISearchInmateService _iSearchInmateService;
        private readonly ISearchInmateCaseService _iSearchInmateCaseService;
        private readonly ISearchInmateBookingService _iSearchInmateBookingService;
        private readonly ISearchInmateChargeService _iSearchInmateChargeService;

        public SearchController(ISearchInmateService iSearchInmateService,
            ISearchInmateCaseService iSearchInmateCaseService, ISearchInmateBookingService iSearchInmateBookingService,
            ISearchInmateChargeService iSearchInmateChargeService)
        {
            _iSearchInmateService = iSearchInmateService;
            _iSearchInmateCaseService = iSearchInmateCaseService;
            _iSearchInmateBookingService = iSearchInmateBookingService;
            _iSearchInmateChargeService = iSearchInmateChargeService;
        }

        [HttpGet("GetBuildingDetails")]
        public IActionResult GetBuildingDetails(int facilityId)
        {
            return Ok(_iSearchInmateService.GetHousing(facilityId));
        }

        [HttpGet("GetPodDetails")]
        public IActionResult GetPodDetails(string buildingId)
        {
            return Ok(_iSearchInmateService.PodDetails(buildingId));
        }

        [HttpGet("GetClassificationDetails")]
        public IActionResult GetClassificationDetails()
        {
            return Ok(_iSearchInmateService.GetClassificationDetails());
        }

        [HttpGet("GetFlagAlertDetails")]
        public IActionResult GetFlagAlertDetails(bool isPermission)
        {
            return Ok(_iSearchInmateService.GetFlagAlertDetails(isPermission));
        }

        [HttpGet("GetLocationDetails")]
        public IActionResult GetLocationDetails()
        {
            return Ok(_iSearchInmateService.GetLocationDetails());
        }

        [HttpGet("GetPersonnel")]
        public IActionResult GetPersonnel(string type, int agencyId)
        {
            return Ok(_iSearchInmateService.GetPersonnel(type, agencyId));
        }

        [HttpGet("GetInmateBookingInfo")]
        public IActionResult GetInmateBookingInfo()
        {
            return Ok(_iSearchInmateService.GetInmateBookingInfo());
        }

        [HttpGet("GetSentenceMethodInfo")]
        public IActionResult GetSentenceMethodInfo()
        {
            return Ok(_iSearchInmateService.GetSentenceMethodInfo());
        }

        [HttpGet("GetCaseType")]
        public IActionResult GetCaseType(string arrestType)
        {
            return Ok(_iSearchInmateService.GetCaseType(arrestType));
        }

        [HttpGet("GetAgencies")]
        public IActionResult GetAgencies()
        {
            return Ok(_iSearchInmateService.GetAgencies());
        }

        [HttpGet("GetChargeFlag")]
        public IActionResult GetChargeFlag()
        {
            return Ok(_iSearchInmateService.GetChargeFlag());
        }

        [HttpGet("GetCaseSearch")]
        public IActionResult GetCaseSearch([FromQuery] SearchRequestVm searchDetails)
        {
            return Ok(_iSearchInmateCaseService.GetBookingSearchList(searchDetails));
        }

        [HttpGet("GetFacilityDetails")]
        public IActionResult GetFacilityDetails()
        {
            return Ok(_iSearchInmateCaseService.GetFacilityDetails());
        }

        [HttpGet("GetInmateSearch")]
        public IActionResult GetInmateSearch([FromQuery] SearchRequestVm searchDetails)
        {
            return Ok(_iSearchInmateService.GetBookingSearchList(searchDetails));
        }

        [HttpGet("GetInmateBookingSearch")]
        public IActionResult GetInmateBookingSearch([FromQuery] SearchRequestVm searchDetails)
        {
            return Ok(_iSearchInmateBookingService.GetBookingSearchList(searchDetails));
        }

        [HttpGet("GetChargeSearch")]
        public IActionResult GetChargeSearch([FromQuery] SearchRequestVm searchDetails)
        {
            return Ok(_iSearchInmateChargeService.GetBookingSearchList(searchDetails));
        }

        [HttpGet("GetLookups")]
        public IActionResult GetLookups()
        {
            return Ok(_iSearchInmateService.GetLookups());
        }
    }
}