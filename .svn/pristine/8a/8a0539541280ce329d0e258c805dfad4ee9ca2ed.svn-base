using Microsoft.AspNetCore.Mvc;
using ServerAPI.Authorization;
using ServerAPI.Services;
using ServerAPI.ViewModels;
using System.Threading.Tasks;

namespace ServerAPI.Controllers
{
    [Route("[controller]")]
    public class CellController : ControllerBase
    {
        private readonly ICellService _cellService;
        private readonly IHousingCellService _housingCellService;
        private readonly IInmateCellService _inmateCellService;

        public CellController(ICellService cellService, IHousingCellService housingCellService, IInmateCellService inmateCellService)
        {
            _cellService = cellService;
            _housingCellService = housingCellService;
            _inmateCellService = inmateCellService;
        }

        [HttpGet("GetHousingGroup")]
        public IActionResult GetHousingGroup(int facilityId)
        {
            return Ok(_cellService.GetHousingGroup(facilityId));
        }

        [HttpGet("GetHousingUnit")]
        public IActionResult GetHousingUnit(int facilityId)
        {
            return Ok(_cellService.GetHousingUnit(facilityId));
        }

        [HttpGet("GetMyLogSettings")]
        public async Task<IActionResult> GetMyLogSettings()
        {
            return Ok(await _cellService.GetMyLogSettings());
        }

        [HttpPost("DeleteUndoMyLog")]
        public async Task<IActionResult> DeleteUndoMyLog([FromBody] CellLogDetailsVm log)
        {
            return Ok(await _cellService.DeleteUndoMyLog(log));
        }

        [HttpGet("GetCellViewerDetails")]
        public IActionResult GetCellViewerDetails(int facilityId, int housingUnitListId, int housingGroupId)
        {
            return Ok(_housingCellService.GetCellViewerDetails(facilityId, housingUnitListId, housingGroupId));
        }

        [HttpGet("GetBedNumberDetails")]
        public IActionResult GetBedNumberDetails(int facilityId, int housingUnitListId, string housingBedNumber)
        {
            return Ok(_housingCellService.GetBedNumberDetails(facilityId, housingUnitListId, housingBedNumber));
        }
       
        
        [HttpGet("GetOutOfServiceDetails")]
        public IActionResult GetOutOfServiceDetails(int housingUnitListId, int facilityId)
        {
            return Ok(_housingCellService.GetOutOfServiceDetails(housingUnitListId, facilityId));
        }
        [FuncPermission(1093,FuncPermissionType.Edit)]
        [HttpGet("EditOutOfService")]
        public IActionResult EditOutOfService()
        {
            return Ok();
        }
        [HttpGet("GetOutOfServiceHistory")]
        public IActionResult GetOutOfServiceHistory(HousingInputVm housingInput)
        {
            return Ok(_housingCellService.GetOutOfServiceHistory(housingInput));
        }

        [HttpPost("UpdateOutOfService")]
        public async Task<IActionResult> UpdateOutOfService([FromBody] HousingUnitVm housingDet)
        {
            return Ok(await _housingCellService.UpdateOutOfService(housingDet));
        }
       
        [HttpGet("GetPropertyLibraryDetails")]
        public IActionResult GetPropertyLibraryDetails(int housingUnitListId, int housingGroupId, string housingBedNumber)
        {
            return Ok(_housingCellService.GetPropertyLibraryDetails(housingUnitListId, housingGroupId, housingBedNumber));
        }

        [HttpGet("GetInmateCell")]
        public IActionResult GetInmateCellDetails([FromQuery]CellInmateInputs input)
        {
            return Ok(_inmateCellService.GetInmateCell(input));
        }

        [HttpGet("GetInmateCellHistory")]
        public IActionResult GetInmateCellHistoryDetails([FromQuery]CellInmateInputs input)
        {
            return Ok(_inmateCellService.GetInmateCellHistory(input));
        }

        [HttpGet("GetInmateCellLog")]
        public IActionResult ViewInmateCellDetails(int cellLogId,int facilityId)
        {
            return Ok(_inmateCellService.GetCellLog(cellLogId, facilityId));
        }

        [HttpPost("SetInmateCellLog")]
        public async Task<IActionResult> AddEditInmateCellDetails([FromBody]InmateCellLogDetailsVm values)
        {
            return Ok(await _inmateCellService.InmateCellLog(values));
        }
        [HttpPost("UpdateUserSettings")]
        public async Task<IActionResult> UpdateUserSettings([FromBody] LogSettingDetails log)
        {
            return Ok(await _cellService.UpdateUserSettings(log));
        }
        //Newly added for functionality Permission
        [FuncPermission(1040,FuncPermissionType.Edit)]
        [HttpPut("EditCellLog")]
        public IActionResult EditCellLog()
        {
            return Ok();
        }
        [FuncPermission(1040,FuncPermissionType.Delete)]
        [HttpPut("DeleteCellLog")]
        public IActionResult DeleteCellLog()
        {
            return Ok();
        }
        [FuncPermission(1040,FuncPermissionType.Add)]
        [HttpPut("AddCellLog")]
        public IActionResult AddCellLog()
        {
            return Ok();
        }

    }
}
