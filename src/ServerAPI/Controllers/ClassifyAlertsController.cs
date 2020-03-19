using Microsoft.AspNetCore.Mvc;
using ServerAPI.Services;
using ServerAPI.ViewModels;
using ServerAPI.Interfaces;
using System.Threading.Tasks;

namespace ServerAPI.Controllers
{

    [Route("[controller]")]
    [ApiController]
    public class ClassifyAlertsController : ControllerBase
    {
        private readonly IClassifyAlertsPrivilegeService _classifyAlertsPrivilegeService;
        private readonly IClassifyAlertsAssociationService _classifyAlertsAssociationService;
        private readonly IClassifyAlertFlagService _classifyAlertFlagService;
        private readonly IClassifyAlertsKeepsepService _classifyAlertsKeepsepService;
        public ClassifyAlertsController(IClassifyAlertsPrivilegeService classifyAlertsPrivilegeService,
        IClassifyAlertsAssociationService classifyAlertsAssociationService,
        IClassifyAlertFlagService classifyAlertFlagService, IClassifyAlertsKeepsepService classifyAlertsKeepsepService)
        {
            _classifyAlertsPrivilegeService = classifyAlertsPrivilegeService;
            _classifyAlertsAssociationService = classifyAlertsAssociationService;
            _classifyAlertFlagService = classifyAlertFlagService;
            _classifyAlertsKeepsepService = classifyAlertsKeepsepService;

        }

        #region Classify->Alerts->Privilege
        [HttpGet("GetClassifyPrivilegeDetails")]
        public IActionResult GetClassifyPrivilegeDetails([FromQuery] ClassifyPrivilegeSearchVm search)
        {
            return Ok(_classifyAlertsPrivilegeService.GetClassifyPrivilegeDetails(search));
        }
        #endregion

        #region Classify->Alerts->Association
        [HttpGet("GetClassifyAssociationDetails")]
        public IActionResult GetClassifyAssociationDetails([FromQuery] KeepSepSearchVm value)
        {
            return Ok(_classifyAlertsAssociationService.GetClassifyAssociationDetails(value));
        }
        #endregion

        [HttpGet("GetClassifyAlert")]
        public IActionResult GetClassifyAlert([FromQuery] KeepSepSearchVm keepSepSearch)
        {
            return Ok(_classifyAlertFlagService.GetClassifyAlert(keepSepSearch));
        }

        #region Classify->Alerts->KeepSeparate

        [HttpGet("GetAlertKeepInmateList")]
        public IActionResult GetAlertKeepInmateList([FromQuery]KeepSepSearchVm keepSepSearch)
        {
            return Ok(_classifyAlertsKeepsepService.GetAlertKeepInmateList(keepSepSearch));
        }
        [HttpGet("GetKeepSeparateAssocSubsetList")]
        public IActionResult GetKeepSeparateAssocSubsetList([FromQuery]KeepSepSearchVm keepSepSearch)
        {
            return Ok(_classifyAlertsKeepsepService.GetKeepSeparateAssocSubsetList(keepSepSearch));
        }
        [HttpGet("GetHousingBuildingDetails")]
        public IActionResult GetHousingBuildingDetails(int facilityId)
        {
            return Ok(_classifyAlertsKeepsepService.GetHousingBuildingDetails(facilityId));
        }

        [HttpPost("DeleteUndoKeepSeparate")]
        public async Task<IActionResult> DeleteUndoKeepSeparate(KeepSeparateVm keepSepSearch)
        {
            return Ok( await _classifyAlertsKeepsepService.DeleteUndoKeepSeparate(keepSepSearch));
        }

        #endregion





    }
}


