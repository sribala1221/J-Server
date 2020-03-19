﻿using Microsoft.AspNetCore.Mvc;
using ServerAPI.Services;
using System.Threading.Tasks;
using ServerAPI.Authorization;

namespace ServerAPI.Controllers
{
    [Route("[controller]")]
    public class InmateDetailsController : ControllerBase
    {
        private readonly IInmateDetailsService _inmateDetailsService;

        public InmateDetailsController(IInmateDetailsService inmateDetailsService)
        {
            _inmateDetailsService = inmateDetailsService;
        }

        //To get Inmate details Parent SubModule 
        [HttpGet("GetInmateDetailParent")]
        public IActionResult GetInmateDetailParent(int subModuleId)
        {
            return Ok(_inmateDetailsService.GetInmateDetailParent(subModuleId));
        }

        [HttpGet("GetInmateDetailsByInmateNumber")]
        public IActionResult GetInmateDetailsByInmateNumber(string inmateNumber)
        {
            return Ok(_inmateDetailsService.GetInmateDetailsCountByInmateNumber(inmateNumber));
        }

        //To get Inmate details Parent SubModule 
        [HttpGet("GetInmateFileCount")]
        public async Task<IActionResult> GetInmateFileCount(int inmateId)
        {
            return Ok(await _inmateDetailsService.GetInmateFileCount(inmateId));
        }

        [HttpGet("GetBookingHistory")]
        public IActionResult GetBookingHistory(int inmateId)
        {
            return Ok(_inmateDetailsService.GetBookingHistory(inmateId));
        }

        [HttpPost("UpdateLastXInmate")]
        public async Task<IActionResult> UpdateLastXInmate([FromBody]int inmateId)
        {
            return Ok(await _inmateDetailsService.UpdateLastXInmate(inmateId));
        }

        [HttpGet("GetLastXInmates")]
        public async Task<IActionResult> GetLastXInmates()
        {
            return Ok(await _inmateDetailsService.GetLastXInmates());
        }
        //Function Permission
        [FuncPermission(731,ViewModels.FuncPermissionType.Access)]
        [HttpPost("AccessSupervisorWorkflow")]
        public IActionResult AccessSupervisorWorkflow()
        {
            return Ok();
        }
        [FuncPermission(731,ViewModels.FuncPermissionType.Edit)]
        [HttpPost("EditSupervisorWorkflow")]
        public IActionResult EditSupervisorWorkflow()
        {
            return Ok();
        }
    }
}