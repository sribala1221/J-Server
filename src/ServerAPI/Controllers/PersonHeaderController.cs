﻿using Microsoft.AspNetCore.Mvc;
using ServerAPI.Services;
using System.Threading.Tasks;
using ServerAPI.Authorization;
using ServerAPI.ViewModels;
using System.Security.Claims;

namespace ServerAPI.Controllers
{
    [Route("[controller]")]
    public class PersonHeaderController : ControllerBase
    {
        private readonly IPersonHeaderService _iPersonHeaderService;
        private readonly IBookingService _iBookingService;

        public PersonHeaderController(IPersonHeaderService iPersonHeaderService, IBookingService iBookingService)
        {
            _iPersonHeaderService = iPersonHeaderService;
            _iBookingService = iBookingService;
        }

        [HttpGet("GetPersonDetails")]
        public IActionResult GetPersonDetails(int personId, bool isInmate = true, int personnelId = 0)
        {
            return Ok(_iPersonHeaderService.GetPersonDetails(personId, isInmate, personnelId));
        }       
       
        [FuncPermission(8100,FuncPermissionType.Access)]
        [HttpGet("ShowIncarcerationName")]
        public IActionResult ShowIncarcerationName()
        {
            return Ok();
        }
        [FuncPermission(8110, FuncPermissionType.Access)]
        [HttpGet("ShowFKNName")]
        public IActionResult ShowFKNName()
        {
            return Ok();
        }
        [FuncPermission(8120, FuncPermissionType.Access)]
        [HttpGet("ShowChar")]
        public IActionResult ShowChar()
        {
            return Ok();
        }
        [FuncPermission(8130, FuncPermissionType.Access)]
        [HttpGet("ShowStatus")]
        public IActionResult ShowStatus()
        {
            return Ok();
        }
        [FuncPermission(8140, FuncPermissionType.Access)]
        [HttpGet("ShowHousing")]
        public IActionResult ShowHousing()
        {
            return Ok();
        }
        [FuncPermission(8150, FuncPermissionType.Access)]
        [HttpGet("ShowLocation")]
        public IActionResult ShowLocation()
        {
            return Ok();
        }
        [FuncPermission(8160, FuncPermissionType.Access)]
        [HttpGet("ShowClassify")]
        public IActionResult ShowClassify()
        {
            return Ok();
        }
        [FuncPermission(8170, FuncPermissionType.Access)]
        [HttpGet("ShowBalance")]
        public IActionResult ShowBalance()
        {
            return Ok();
        }
        [FuncPermission(8180, FuncPermissionType.Access)]
        [HttpGet("ShowProperty")]
        public IActionResult ShowProperty()
        {
            return Ok();
        }
        [FuncPermission(8190, FuncPermissionType.Access)]
        [HttpGet("ShowVisitSchedule")]
        public IActionResult ShowVisitSchedule()
        {
            return Ok();
        }
        // [FuncPermission(8200, FuncPermissionType.Access)]
        [HttpGet("ShowFrontViewPhoto")]
        public IActionResult ShowFrontViewPhoto()
        {
            return Ok();
        }
        // [FuncPermission(8210, FuncPermissionType.Access)]
        [HttpGet("ShowOtherPhoto")]
        public IActionResult ShowOtherPhoto()
        {
            return Ok();
        }
        [FuncPermission(8310, FuncPermissionType.Access)]
        [HttpGet("ShowMessageAlert")]
        public IActionResult ShowMessageAlert()
        {
            return Ok();
        }
        // [FuncPermission(8300, FuncPermissionType.Access)]
        [HttpGet("OverallAlertAccess")]
        public IActionResult OverallAlertAccess()
        {
            return Ok();
        }
        [FuncPermission(8320, FuncPermissionType.Access)]
        [HttpGet("ShowFlagAlwaysAlert")]
        public IActionResult ShowFlagAlwaysAlert()
        {
            return Ok();
        }
        [FuncPermission(8340, FuncPermissionType.Access)]
        [HttpGet("ShowFlagAlertDiet")]
        public IActionResult ShowFlagAlertDiet()
        {
            return Ok();
        }
        [FuncPermission(8350, FuncPermissionType.Access)]
        [HttpGet("ShowKeepSeps")]
        public IActionResult ShowKeepSeps()
        {
            return Ok();
        }
        [FuncPermission(8360, FuncPermissionType.Access)]
        [HttpGet("ShowAssociation")]
        public IActionResult ShowAssociation()
        {
            return Ok();
        }
        [FuncPermission(8400, FuncPermissionType.Access)]
        [HttpGet("OverallInOutAccess")]
        public IActionResult  OverallInOutAccess()
        {
            return Ok();
        }
        [FuncPermission(8410, FuncPermissionType.Access)]
        [HttpGet("ShowIncarcerationLevel")]
        public IActionResult ShowIncarcerationLevel()
        {
            return Ok();
        }
        [FuncPermission(8420, FuncPermissionType.Access)]
        [HttpGet("ShowBookingLevel")]
        public IActionResult ShowBookingLevel()
        {
            return Ok();
        }

        [HttpGet("GetPersonInOut")]
        public IActionResult GetPersonInOut(int inmateId, bool toBindCharge = false,
            bool isActiveIncarceration = false)
        {
            return Ok(_iBookingService.GetIncarcerationAndBookings(inmateId, toBindCharge, isActiveIncarceration));
        }
        
        //fetch inmate information like balance, inventory, aka, visit
        [HttpGet("GetPersonInfo")]
        public IActionResult GetPersonInfo(int inmateId)
        {
            return Ok(_iPersonHeaderService.GetPersonInfo(inmateId));
        }

        [HttpGet("GetPersonHeader")]
        public async Task<IActionResult> GetPersonHeader(int personId)
        {
            return Ok(await _iPersonHeaderService.GetPersonHeader(personId, User));
        }
        //Added For functionality Permisison
        [FuncPermission(8370,FuncPermissionType.Access)]
        [HttpGet("ShowPrivilegeRevoke")]
        public IActionResult ShowPrivilegeRevoke()
        {
            return Ok();
        }
        [FuncPermission(8380,FuncPermissionType.Access)]
        [HttpGet("ShowPrivilegeAuth")]
        public IActionResult ShowPrivilegeAuth()
        {
            return Ok();
        }
        [FuncPermission(8390,FuncPermissionType.Access)]
        [HttpGet("ShowObservationAlert")]
        public IActionResult ShowObservationAlert()
        {
            return Ok();
        }
    }
}