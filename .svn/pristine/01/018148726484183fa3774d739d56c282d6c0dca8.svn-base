using Microsoft.AspNetCore.Mvc;
using ServerAPI.Services;

namespace ServerAPI.Controllers
{
    [Route("[controller]")]
    public class FieldSettingsController : ControllerBase
    {
        private readonly IFieldSettingsService _fieldSettingsService;
        public FieldSettingsController(IFieldSettingsService fieldSettingsService)
        {
            _fieldSettingsService = fieldSettingsService;
        }

        [HttpPost("GetFieldLabelSettings")]
        public IActionResult GetFieldLabel([FromBody]int[] fieldLabelId)
        {                                  
            return Ok(_fieldSettingsService.GetFieldLabels(fieldLabelId));
        }

        [HttpGet("GetFieldSettings")]
        public IActionResult GetFieldSettings()
        {
            return Ok(_fieldSettingsService.GetFieldSettings());
        }

        [HttpGet("GetFieldSettings/{appAoUserControlId}")]
        public IActionResult GetFieldSettings(int appAoUserControlId)
        {
            return Ok(_fieldSettingsService.GetFieldSettings(appAoUserControlId));
        }

    }
}
