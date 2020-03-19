using Microsoft.AspNetCore.Mvc;
using ServerAPI.Services;
using ServerAPI.ViewModels;
using System.Threading.Tasks;
using ServerAPI.Authorization;
using System.Collections.Generic;
using System;

namespace ServerAPI.Controllers
{
    [Route("[controller]")]
    public class InmateIncidentController : ControllerBase
    {
        private readonly IInmateIncidentService _inmateIncidentService;

        public InmateIncidentController(IInmateIncidentService inmateIncidentService)
        {
            _inmateIncidentService = inmateIncidentService;
        }

        [HttpGet("GetInmateIncidentList")]
        public IActionResult GetInmateIncidentList(int inmateId)
        {
            return Ok(_inmateIncidentService.GetInmateIncidentList(inmateId));
        }

        [HttpGet("GetInmateIncidentDropdownList")]
        public IActionResult GetInmateIncidentDropdownList(bool presetFlag,int facilityId)
        {
            return Ok(_inmateIncidentService.GetInmateIncidentDropdownList(presetFlag, facilityId));
        }

        [HttpGet("GetHousingUnitNumber")]
        public IActionResult GetHousingUnitNumber(int facilityId, string housingUnitLocation)
        {
            return Ok(_inmateIncidentService.GetHousingUnitNumber(facilityId, housingUnitLocation));
        }

        [HttpGet("GetHousingUnitBedNumber")]
        public IActionResult GetHousingUnitBedNumber(int facilityId, string housingUnitLocation,
            string housingUnitNumber)
        {
            return Ok(_inmateIncidentService.GetHousingUnitBedNumber(facilityId, housingUnitLocation, housingUnitNumber));
        }

        [HttpGet("GetLocation")]
        public IActionResult GetLocation(int facilityId)
        {
            return Ok(_inmateIncidentService.GetLocation(facilityId));
        }

        [FuncPermission(1126, FuncPermissionType.Add)]
        [HttpPost("InsertInmateIncident")]
        public async Task<IActionResult> InsertInmateIncident([FromBody] InmateIncidentInfo incidentDetails)
        {
            return Ok(await _inmateIncidentService.InsertInmateIncident(incidentDetails));
        }

        [HttpGet("LoadInmateIncident")]
        public IActionResult LoadInmateIncident(int incidentId)
        {
            return Ok(_inmateIncidentService.GetInmateIncident(incidentId));
        }

        [HttpPost("UpdateInmateIncident")]
        public async Task<IActionResult> UpdateInmateIncident([FromBody] InmateIncidentInfo inmateIncidentInfo)
        {
            return Ok(await _inmateIncidentService.UpdateInmateIncident(inmateIncidentInfo));
        }

        //To Get Disciplinary Preset details
        [HttpGet("GetDisciplinaryPresetDetails")]
        public IActionResult GetDisciplinaryPresetDetails()
        {
            return Ok(_inmateIncidentService.GetDisciplinaryPresetDetails());
        }

        //Insert method for preset incident
        [HttpPost("InsertPresetIncident")]
        public async Task<IActionResult> InsertPresetIncident([FromBody] DisciplinaryPresetVm presetDetails)
        {
            return Ok(await _inmateIncidentService.InsertPresetIncident(presetDetails));
        }

        [HttpGet("GetIncidentForms")]
        public IActionResult GetIncidentForms(int incidentId, int dispInmateId)
        {
            return Ok(_inmateIncidentService.LoadIncidentForms(incidentId, dispInmateId));
        }

        [HttpGet("GetCellGroupDetails")]
        public IActionResult GetCellGroupDetails(int facilityId, string housingUnitLocation, string housingUnitNumber)
        {
            return Ok(_inmateIncidentService.GetCellGroupDetails(facilityId, housingUnitLocation, housingUnitNumber));
        }
        [HttpGet("GetLastSearchList")]
        public IActionResult GetLastSearchList(int facilityId, string searchFlag)
        {
            return Ok(_inmateIncidentService.GetLastSearchDetails(facilityId, searchFlag));
        }
        [HttpGet("GetCountSearchList")]
        public IActionResult GetCountSearchList(int facilityId, string searchFlag, DateTime fromDate, string disposition, string lookupType)
        {
            return Ok(_inmateIncidentService.GetCountSearchList(facilityId, searchFlag, fromDate, disposition, lookupType));
        }
        [HttpGet("GetLastSearchHistoryList")]
        public IActionResult GetLastSearchHistoryList(LastHousingSearchVm value, int facilityId, string searchFlag)
        {
            return Ok(_inmateIncidentService.GetLastSearchHistoryList(value, facilityId, searchFlag));
        }

         [HttpGet("GetCountSearchHistoryList")]
        public IActionResult GetCountSearchHistoryList(CountHousingSearchVm value, int facilityId, string searchFlag)
        {
            return Ok(_inmateIncidentService.GetCountSearchHistoryList(value, facilityId, searchFlag));
        }
    }
}
