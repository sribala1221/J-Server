using Microsoft.AspNetCore.Mvc;
using ServerAPI.Services;
using ServerAPI.ViewModels;
using System.Threading.Tasks;

namespace ServerAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class FacilityOperationsController : ControllerBase
    {
        private readonly IFacilityOperationsService _facilityOperationsService;
        private readonly IOperationViewerService _operationViewerService;

        public FacilityOperationsController(IFacilityOperationsService facilityOperationsService, IOperationViewerService operationViewerService)
        {
            _facilityOperationsService = facilityOperationsService;
            _operationViewerService = operationViewerService;
        }

        [HttpPost("GetRosterMaster")]
        public IActionResult GetRosterMaster(RosterFilters rosterFilters)
        {
            return Ok(_facilityOperationsService.GetRosterMasterDetails(rosterFilters));
        }

        [HttpPost("GetFilterRoster")]
        public IActionResult GetFilterRoster(RosterFilters rosterFilters)
        {
            return Ok(_facilityOperationsService.GetFilterRoster(rosterFilters));
        }

        [HttpPost("getAllRosterInmate")]
        public IActionResult getAllRosterInmate(RosterFilters rosterFilters)
        {
            return Ok(_facilityOperationsService.getAllRosterInmate(rosterFilters));
        }

        [HttpGet("LoadPersonFormTemplates")]
        public IActionResult LoadPersonFormTemplates()
        {
            return Ok(_facilityOperationsService.GetPersonFormTemplates());
        }

        [HttpPost("LoadInmateBookings")]
        public IActionResult LoadInmateBookings(int[] inmateId)
        {
            return Ok(_facilityOperationsService.GetInmateBookings(inmateId));
        }

        [HttpGet("LoadPrintOverLayScreen")]
        public IActionResult LoadPrintOverLayScreen([FromQuery] PrintOverLay printOverLay)
        {
            return Ok(_facilityOperationsService.GetRosterOverlay(printOverLay));
        }

        //Operation-Viewer
        [HttpGet("InitOperationViewer")]
        public async  Task<IActionResult> InitiateOperationViewer(int facilityId, bool isOperationViewer)
        {
            return Ok(await _operationViewerService.GetFacilityViewerInit(facilityId, isOperationViewer));
        }

        //Cell-Viewer
        [HttpGet("InitCellViewer")]
        public async  Task<IActionResult> InitiateCellViewer(int facilityId, bool isOperationViewer)
        {
            return Ok(await _operationViewerService.GetFacilityViewerInit(facilityId, isOperationViewer));
        }

        [HttpGet("FilterOperationViewer")]
        public async  Task<IActionResult> FilterOperationViewer([FromQuery] ViewerParameter objviewer)
        {
            return Ok(await _operationViewerService.GetFacilityViewerRefresh(objviewer));
        }

        [HttpPut("UpdateUserSettings")]
        public async Task<IActionResult> UpdateUserSettings(ViewerFilter objfilter)
        {
            return Ok(await _operationViewerService.UpdateUserSettings(objfilter));
        }

        [HttpDelete("DeleteUndoOperations")]
        public async Task<IActionResult> DeleteUndoOperations([FromQuery]DeleteParams deleteParams)
        {
            return Ok(await _operationViewerService.OperationDelete(deleteParams));
        }

        [HttpPost("PrintOverlayReport")]
        public async Task<IActionResult> PrintOverlayReport(string ids,[FromQuery]int templateId)
        {
            return Ok(await _facilityOperationsService.PrintOverlayReport(ids,templateId));
        }
    }
}