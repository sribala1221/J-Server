using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Linq;
using System.Net;
using Newtonsoft.Json;
using System;
using GenerateTables.Models;
using ServerAPI.Services;
using ServerAPI.ViewModels;

namespace ServerAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class FulcrumController : ControllerBase
    {
        private readonly IFulcrumService _fulcrumServices;

        public FulcrumController(IFulcrumService fulcrumService)
        {
            _fulcrumServices = fulcrumService;
        }

        //ENROLL

        [AllowAnonymous]
        [HttpPost("enroll")] 
        public Object Enroll([FromBody] FulcrumEnrollRequest value)
        {
            return _fulcrumServices.Enroll(value);
        }

        //IDENTIFY
        [AllowAnonymous]
        [HttpPost("identify")]
        public Object Identify([FromBody] FulcrumIdentifyRequest value)
        {
            return _fulcrumServices.GetInmateNumber(value);
        }

        //VERIFY
        [AllowAnonymous]
        [HttpPost("verify")]
        public Object Verify([FromBody] FulcrumVerifyRequest value)
        {
            return _fulcrumServices.Verify(value);
        }

        //DELETE
        [AllowAnonymous]
        [HttpPost("delete")]
        public Object Delete([FromBody] FulcrumDeleteRequest value)
        {
            return _fulcrumServices.Delete(value);
        }

    }

    
}
