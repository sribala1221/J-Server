using Microsoft.AspNetCore.Mvc;
using ServerAPI.Services;
using ServerAPI.ViewModels;
using ServerAPI.Authorization;

namespace ServerAPI.Controllers
{
    [Route("[controller]")]
    public class BookingReleaseController : ControllerBase
    {
        private readonly IBookingReleaseService _bookingReleaseService;

        public BookingReleaseController(IBookingReleaseService bookingReleaseService)
        {
            _bookingReleaseService = bookingReleaseService;
        }

        [FuncPermission(720, FuncPermissionType.Access)]
        // Get Booking In Release counts to bind Left Side Grid values.
        [HttpGet("GetBookingRelease")]
        public IActionResult GetBookingRelease([FromQuery] BookingInputsVm iRelease)
        {
            return Ok(_bookingReleaseService.GetBookingRelease(iRelease));
        }

        // Get Booking In Release counts to bind Left Side Grid values.
        [FuncPermission(721, FuncPermissionType.Add)]
        [HttpGet("NewRelease")]
        public IActionResult NewRelease()
        {
            return NoContent();
        }
        //Edit Release
         [FuncPermission(721, FuncPermissionType.Edit)]
        [HttpGet("EditRelease")]
        public IActionResult EditRelease()
        {
            return NoContent();
        }

        [FuncPermission(720, FuncPermissionType.Access)]
        // Get Booking Release to bind Right Side Grid values.
        [HttpGet("GetBookingReleaseDetails")]
        public IActionResult GetBookingReleaseDetails([FromQuery] BookingInputsVm iRelease)
        {
            return Ok(_bookingReleaseService.GetBookingReleaseDetails(iRelease));
        }

        // Get Booking Complete details.
        [HttpGet("GetInmateRelease")]
        public IActionResult GetInmateRelease(int inmateId, int incarcerationId, int personId)
        {
            return Ok(_bookingReleaseService.GetInmateRelease(inmateId, incarcerationId, personId));
        }

        
        [HttpGet("GetBookingTransportDetails")]
        public IActionResult GetBookingTransportDetails(int incarcerationId, bool deleteFlag)
        {
            return Ok(_bookingReleaseService.GetBookingTransportDetails(incarcerationId, deleteFlag));
        }

        
        [HttpGet("GetTransportManageDetails")]
        public IActionResult GetTransportManageDetails(int incarcerationId)
        {
            return Ok(_bookingReleaseService.GetTransportManageDetails(incarcerationId));
        }

        [HttpPost("UpdateTransportDetails")]
        public IActionResult UpdateTransportDetails([FromBody] TransportManageVm transportManageVm)
        {
            return Ok(_bookingReleaseService.UpdateManageDetails(transportManageVm));
        }

        [HttpPost("GetSupervisorValidate")]
        public IActionResult GetSupervisorValidate([FromBody] int incarcerationId)
        {
            return Ok(_bookingReleaseService.GetSupervisorValidate(incarcerationId));
        }

        [FuncPermission(8422, FuncPermissionType.Add)]
        [HttpPost("InsertTransportNote")]
        public IActionResult InsertTransportNote([FromBody] TransportNote transportNote)
        {
            return Ok(_bookingReleaseService.InsertTransportNote(transportNote));
        }
        [FuncPermission(8422, FuncPermissionType.Delete)]
        [HttpPost("UpdateTransportNote")]
        public IActionResult UpdateTransportNote([FromBody] TransportNote transportNote)
        {
            return Ok(_bookingReleaseService.UpdateTransportNote(transportNote));
        }
        
        [HttpGet("GetInmateByBooking")]
        public IActionResult GetInmateByBooking(string bookingNumber, int inmateId)
        {
            return Ok(_bookingReleaseService.GetInmateByBooking(bookingNumber, inmateId));
        }

        [FuncPermission(721, FuncPermissionType.Edit)]
        [HttpPut("UpdateDoRelease")]
        public IActionResult UpdateDoRelease([FromBody] BookingComplete bookingComplete)
        {
            _bookingReleaseService.UpdateDoReleaseAsync(bookingComplete.InmateId, bookingComplete.IncarcerationId);
            return NoContent();
        }

        [FuncPermission(722, FuncPermissionType.ReleaseEdit)]
        [HttpPost("UpdateUndoRelease")]
        public IActionResult UpdateUndDoRelease([FromBody]BookingComplete bookingComplete)
        {
            _bookingReleaseService.UpdateUndoReleaseAsync(bookingComplete.InmateId, bookingComplete.IncarcerationId);
            return NoContent();
        }
        [HttpGet("GetInmateReleaseValidation")]
        public IActionResult GetInmateReleaseValidation(int inmateId)
        {
            return Ok(_bookingReleaseService.GetInmateReleaseValidation(inmateId));
        }

        [HttpPost("UpdateClearFlag")]
        public IActionResult UpdateClearFlag([FromBody] int incarcerationId)
        {
            return Ok(_bookingReleaseService.UpdateClearFlag(incarcerationId));
        }

        //Added for Functionality Permission
        [FuncPermission(8421,FuncPermissionType.Edit)]
        [HttpGet("EditManagetransportAfterRelease")]
        public IActionResult EditManagetransportAfterRelease()
        {
            return Ok();
        }
        [FuncPermission(8421,FuncPermissionType.Delete)]
        [HttpDelete("DeleteManageAfterTransportRelease")]
        public IActionResult DeleteManageAfterTransportRelease()
        {
            return Ok();
        }
        [FuncPermission(8421,FuncPermissionType.Add)]
        [HttpGet("AddManageAfterTransportRelease")]
        public IActionResult AddManageAfterTransportRelease()
        {
            return Ok();
        }

    }
}