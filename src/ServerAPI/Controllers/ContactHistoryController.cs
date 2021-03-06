﻿using Microsoft.AspNetCore.Mvc;
using ServerAPI.Services;
using ServerAPI.ViewModels;

namespace ServerAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ContactHistoryController : ControllerBase
    {
        private readonly IContactHistoryService _contactService;
        public ContactHistoryController(IContactHistoryService contactService)
        {
            _contactService = contactService;

        }

        #region Person History
        [HttpGet("GetContacts")]
        public IActionResult GetContacts([FromQuery]ContactHistoryVm searchValue)
        {
            return Ok(_contactService.GetContacts(searchValue));
        }
        [HttpGet("GetContactDetail")]
        public IActionResult GetContactDetail([FromQuery]ContactHistoryModelVm modelVm)
        {
            return Ok(_contactService.GetContactDetail(modelVm));
        }
        #endregion

        #region Personnel History
        [HttpGet("GetPersonnelContacts")]
        public IActionResult GetPersonnelContacts([FromQuery]ContactHistoryVm searchValue)
        {
            return Ok(_contactService.GetPersonnelContacts(searchValue));
        }
        [HttpGet("GetPersonnelDetail")]
        public IActionResult GetPersonnelDetail([FromQuery]ContactHistoryDetailVm contacts)
        {
            return Ok(_contactService.GetPersonnelDetail(contacts));
        }
        #endregion
    }
}
