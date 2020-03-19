﻿using Microsoft.AspNetCore.Mvc;
using ServerAPI.Authorization;
using ServerAPI.Services;
using ServerAPI.ViewModels;
using System.Threading.Tasks;

namespace ServerAPI.Controllers
{
	[Route("[controller]")]
	public class RequestController : ControllerBase
	{

		private readonly IRequestService _requestService;

		public RequestController(IRequestService requestService)
		{
			_requestService = requestService;
		}

		// Save a new Inmate Request 
		
		[HttpPost("SaveRequestDetails")]
		public async Task<IActionResult> SaveRequestDetails([FromBody] RequestVm requestDetails)
		{
			return Ok(await _requestService.SaveRequestDetail(requestDetails));
		}
		[FuncPermission(495,FuncPermissionType.Add)]
		[HttpPut("AddRequest")]
		public IActionResult AddRequest()
		{return Ok();}
		[FuncPermission(495,FuncPermissionType.Edit)]
		[HttpPut("EditRequest")]
		public IActionResult EditRequest()
		{return Ok();}
		[FuncPermission(495,FuncPermissionType.Access)]
		[HttpPut("AccessRequest")]
		public IActionResult AccessRequest()
		{return Ok();}

		// To Assign,Undo and Clear Request Details
		[FuncPermission(495,FuncPermissionType.Edit)]
		[HttpPut("UpdateRequestDetails")]
		public async Task<IActionResult> UpdateRequestDetails([FromBody] RequestOperations requests)
		{
			return Ok(await _requestService.UpdateRequestDetails(requests));
		}

		// To Transfer,Disposition ,Note and Response for that Request
		[HttpPut("SaveRequestTransfer")]
		public async Task<IActionResult> SaveRequestTransferAccept([FromBody] RequestTransfer transNote)
		{
			return Ok(await _requestService.SaveRequestTransfer(transNote));
		}

		[HttpGet("GetRequestActionList")]
		public IActionResult GetRequestActionDropdownList(RequestDetails request)
		{
			return Ok(_requestService.GetRequestActionList(request));
		}

		[HttpGet("GetAssignRequest")]
		public async Task<IActionResult> GetAssignRequestAsync(int inmateId, int facilityId, string fromScreen)
		{
			return Ok(await _requestService.GetAssignRequest(inmateId, facilityId, fromScreen));
		}

		[HttpGet("GetPenRequestLst")]
		public async Task<IActionResult> GetPenRequestLstAsync(int facilityId, int[] housingGroup, string fromScreen)
		{
			return Ok(await _requestService.GetPenRequestDetails(facilityId, housingGroup, fromScreen));
		}

		[HttpGet("GetRequestStatus")]
		public async Task<IActionResult> GetRequestStatus(int facilityId)
		{
			return Ok(await _requestService.GetRequestStatus(facilityId));
		}

		[HttpGet("RequestAction")]
        public async Task<IActionResult> RequestAction(int facilityId)
        {
            return Ok(await _requestService.RequestAction(facilityId));
        }

        [HttpGet("GetRequestSearch")]
        public async Task<IActionResult> GetRequestSearch(RequestValues values)
        {
            return Ok(await _requestService.GetRequestSearch(values));
        }

		[HttpGet("GetRequestCount")]
		public async Task<IActionResult> GetRequestCount(int facilityId)
		{
			return Ok(await _requestService.GetRequestCount(facilityId));
		}

		//Get Request details by RequestId
		[HttpGet("GetRequestDetails")]
		public IActionResult GetRequestDetails(int requestId)
		{
			return Ok(_requestService.GetRequestDetailsById(requestId));
		}

		#region Inmate Request

		//Get Inmate Request details
		[HttpGet("GetInmateRequest")]
		public IActionResult GetInmateRequestSummary(int inmateId, int? requestId, int? actionId)
		{
			return Ok(_requestService.GetInmateRequest(inmateId, requestId, actionId));
		}

		//Get Inmate Request Count
		[HttpGet("GetInmateRequestCount")]
		public IActionResult GetInmateRequestCount(int inmateId, int? requestId)
		{
			return Ok(_requestService.GetInmateRequestCount(inmateId, requestId));
		}

		#endregion
	}
}