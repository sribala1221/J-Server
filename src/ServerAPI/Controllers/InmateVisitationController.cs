using Microsoft.AspNetCore.Mvc;
using ServerAPI.Authorization;
using ServerAPI.Services;
using ServerAPI.ViewModels;
using System.Threading.Tasks;

namespace ServerAPI.Controllers
{
    [Route("[controller]")]
    public class InmateVisitationController:Controller
    {
        private readonly IInmateVisitationService _inmateVisitationService;

        public InmateVisitationController(IInmateVisitationService inmateVisitationService)
        {
            _inmateVisitationService = inmateVisitationService;
        }

        [HttpGet("GetDefaultVisitorInfo")]
        public IActionResult GetDefaultVisitorInfo(SearchVisitorList searchVisitorList)
        {
            return Ok(_inmateVisitationService.GetDefaultVisitorInfo(searchVisitorList));
        }

        [HttpGet("GetVisitorList")]
        public IActionResult GetVisitorList(SearchVisitorList searchVisitorList)
        {
            return Ok(_inmateVisitationService.GetVisitorList(searchVisitorList));
        }

        [HttpGet("GetVisitationSearchList")]
        public IActionResult GetVisitationSearchList(SearchVisitorList searchVisitorList)
        {
            return Ok(_inmateVisitationService.GetVisitationSearchList(searchVisitorList));
        }

        [HttpGet("GetAssignedInmateList")]
        public IActionResult GetAssignedInmateList(int visitorListId, bool isActiveInmate)
        {
            return Ok(_inmateVisitationService.GetAssignedInmateList(visitorListId, isActiveInmate));
        }

        [HttpPost("InsertUpdatePersonDetails")]
        public async Task<IActionResult> InsertUpdatePersonDetails
            ([FromBody]PersonalVisitorDetails personalVisitor)
        {
            return Ok(await _inmateVisitationService.InsertUpdatePersonDetails(personalVisitor));
        }

        [HttpPost("InsertInmateToVisitor")]
        public async Task<IActionResult> InsertInmateToVisitor
            ([FromBody]InmateToVisitorInfo inmateToVisitorInfo)
        {
            return Ok(await _inmateVisitationService.InsertInmateToVisitor(inmateToVisitorInfo));
        }

        [HttpGet("GetRejectReason")]
        public IActionResult GetRejectReason()
        {
            return Ok(_inmateVisitationService.GetRejectReason());
        }

        [HttpGet("GetPersonOfInterest")]
        public IActionResult GetPersonOfInterest()
        {
            return Ok(_inmateVisitationService.GetPersonOfInterest());
        }

        [HttpGet("GetPersonalVisitorDetails")]
        public IActionResult GetPersonalVisitorDetails(int visitorListId)
        {
            return Ok(_inmateVisitationService.GetPersonalVisitorDetails(visitorListId));
        }

        [HttpPost("DeletePersonalVisitorDetails")]
        public async Task<IActionResult> DeletePersonalVisitorDetails([FromBody]int visitorListId)
        {
            return Ok(await _inmateVisitationService.DeletePersonalVisitorDetails(visitorListId));
        }

        [HttpPost("UndoPersonalVisitorDetails")]
        public async Task<IActionResult> UndoPersonalVisitorDetails([FromBody]int visitorListId)
        {
            return Ok(await _inmateVisitationService.UndoPersonalVisitorDetails(visitorListId));
        }
                
        [HttpGet("GetVisitorListSaveHistory")]
        public IActionResult GetVisitorListSaveHistory(int visitorListId)
        {
            return Ok(_inmateVisitationService.GetVisitorListSaveHistory(visitorListId));
        }

        [FuncPermission(926, FuncPermissionType.Access)]
        [HttpGet("GetHistoryList")]
        public IActionResult GetHistoryList(SearchVisitorHistoryList searchHistoryList)
        {
            return Ok(_inmateVisitationService.GetHistoryList(searchHistoryList));
        }

        [HttpGet("GetHistorySearchList")]
        public IActionResult GetHistorySearchList(SearchVisitorHistoryList searchHistoryList)
        {
            return Ok(_inmateVisitationService.GetVisitationHistorySearchList(searchHistoryList));
        }

        [HttpGet("GetVisitorHistory")]
        public IActionResult GetVisitorHistory(int inmateId,int personId)
        {
            return Ok(_inmateVisitationService.GetVisitorHistory(inmateId, personId));
        }

        [HttpPost("DeleteAssignedInmateDetails")]
        public async Task<IActionResult> DeleteAssignedInmateDetails
            ([FromBody]int visitorListId)
        {
            return Ok(await _inmateVisitationService.DeleteAssignedInmateDetails
                (visitorListId));
        }

        [HttpPost("UndoAssignedInmateDetails")]
        public async Task<IActionResult> UndoAssignedInmateDetails
            ([FromBody]int visitorListId)
        {
            return Ok(await _inmateVisitationService.UndoAssignedInmateDetails
                (visitorListId));
        }

        [HttpPost("DeleteVisitationHistory")]
        public async Task<IActionResult> DeleteVisitationHistory([FromBody]int visitorId)
        {
            return Ok(await _inmateVisitationService.DeleteVisitationHistory
                (visitorId));
        }

        [HttpGet("GetVisitorAssignedListSaveHistory")]
        public IActionResult GetVisitorAssignedListSaveHistory(int visitorId)
        {
            return Ok(_inmateVisitationService.GetVisitorAssignedListSaveHistory(visitorId));
        }

        [HttpGet("GetInmateVisitorDetails")]
        public IActionResult GetInmateVisitorDetails(int visitorId)
        {
            return Ok(_inmateVisitationService.GetInmateVisitorDetails(visitorId));
        }

        [HttpGet("GetVisitorRejectHistoryList")]
        public IActionResult GetVisitorRejectHistoryList(int visitorId, VisitorRejectFlag visitorFlag, int visitorListId)
        {
            return Ok(_inmateVisitationService.GetVisitorRejectHistoryList(visitorId, visitorFlag, visitorListId));
        }
    }
}
