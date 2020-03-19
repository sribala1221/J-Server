﻿using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ServerAPI.ViewModels;
using ServerAPI.Services;
using System;
using ServerAPI.Authorization;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace ServerAPI.Controllers
{
    [Route("[controller]")]
    public class InmateTrackingController : ControllerBase
    {
        private readonly IInmateTrackingService _inmateTrackingService;
        private readonly ITrackingConflictService _trackingConflictService;

        public InmateTrackingController(IInmateTrackingService inmateTrackingService, ITrackingConflictService trackingConflictService)
        {
            _inmateTrackingService = inmateTrackingService;
            _trackingConflictService = trackingConflictService;
        }

        [HttpGet("GetInmateTrackingDetails")]
        public IActionResult GetInmateTrackingDetails(int inmateId)
        {
            return Ok(_inmateTrackingService.GetInmateTrackingDetails(inmateId));
        }
        [FuncPermission(492, FuncPermissionType.Edit)]
        [HttpPost("InsertInmateTracking")]
        public async Task<IActionResult> InsertInmateTracking([FromBody] InmateTrackingVm ob)
        {           
            return Ok(await _inmateTrackingService.InsertInmateTracking(ob));
        }
        [FuncPermission(531,FuncPermissionType.Edit)]
        [HttpPut("EditUnscheduledTracking")]
        public IActionResult EditUnscheduledTracking()
        {
            return Ok();
        }

        // To get Intake Inmate Track Details
        [HttpGet("GetInmateTracking")]
        public IActionResult GetInmateTracking(int facilityId, int inmateId)
        {
            return Ok(_inmateTrackingService.GetInmateTracking(facilityId, inmateId));
        }

        // To Get Tracking Conflict
        [HttpGet("GetTrackingConflict")]
        public IActionResult GetTrackingConflict([FromQuery] TrackingVm obj)
        {
            return Ok(_trackingConflictService.GetTrackingConflict(obj));
        }

        [HttpGet("GetTrackingHistory")]
        public IActionResult GetTrackingHistory(int inmateId, DateTime? dateIn, DateTime? dateOut)
        {
            return Ok(_inmateTrackingService.GetTrackingHistory(inmateId, dateIn, dateOut));
        }

        [HttpGet("GetFacilityLocation")]
        public IActionResult GetFacilityLocation(int inmateId)
        {
            return Ok(_trackingConflictService.GetFacilityLocation(inmateId));
        }

    }
}
