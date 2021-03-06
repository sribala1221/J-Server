﻿using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ServerAPI.ViewModels;
using ServerAPI.Services;
using GenerateTables.Models;
using ServerAPI.Authorization;

namespace ServerAPI.Controllers
{
    [Route("[controller]")]
    public class IncidentAppealsController : ControllerBase
    {
        private readonly IIncidentAppealsService _appealsService;

        public IncidentAppealsController(IIncidentAppealsService appealsService)
        {
            _appealsService = appealsService;
        }

        [HttpGet("GetIncidentAppealsCount")]
        public IActionResult GetIncidentAppealsCount(int facilityId)
        {
            return Ok(_appealsService.GetIncidentAppealsCount(facilityId));
        }

        [HttpGet("GetIncidentAppeals")]
        public IActionResult GetIncidentAppeals(AppealsParam objAppealsParam)
        {
            return Ok(_appealsService.GetIncidentAppeals(objAppealsParam));
        }

        [FuncPermission(1146,FuncPermissionType.Add)]
        [HttpPost("InsertDisciplinaryInmateAppeal")]
        public async Task<IActionResult> InsertDisciplinaryInmateAppeal([FromBody] DispInmateAppeal inmateAppeal)
        {
            return Ok(await _appealsService.InsertDisciplinaryInmateAppeal(inmateAppeal));
        }

        [FuncPermission(1146,FuncPermissionType.Edit)]
        [HttpPut("UpdateDisciplinaryInmateAppeal")]
        public async Task<IActionResult> UpdateDisciplinaryInmateAppeal(int dispInmateAppealId,
            [FromBody] DispInmateAppeal inmateAppeal)
        {
            return Ok(await _appealsService.UpdateDisciplinaryInmateAppeal(dispInmateAppealId, inmateAppeal));
        }

        [FuncPermission(1147,FuncPermissionType.Edit)]
        [HttpPut("ReviewDisciplinaryInmateAppeal")]
        public async Task<IActionResult> ReviewDisciplinaryInmateAppeal(int dispInmateAppealId,
            [FromBody] DispInmateAppeal inmateAppeal)
        {
            return Ok(await _appealsService.UpdateDisciplinaryInmateAppeal(dispInmateAppealId, inmateAppeal));
        }

		//Get Incident AppealsCount by Incident Id
        [HttpGet("GetAppealsCount")]
		public IActionResult GetAppealsCountByIncidentId(int incidentId)
		{
			return Ok(_appealsService.GetAppealsCountByIncidentId(incidentId));
		}
	}
}
