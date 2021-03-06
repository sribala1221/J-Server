﻿using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ServerAPI.Services;
using ServerAPI.ViewModels;
using System.Collections.Generic;
using ServerAPI.Authorization;

namespace ServerAPI.Controllers
{
    [Route("[controller]")]
    public class InmateBookingController : ControllerBase
    {
        private readonly IInmateBookingService _inmateBookingService;
        private readonly IInmateBookingCaseService _inmateBookingCaseService;

        public InmateBookingController(IInmateBookingService inmateBookingService,
            IInmateBookingCaseService inmateBookingCaseService)
        {
            _inmateBookingService = inmateBookingService;
            _inmateBookingCaseService = inmateBookingCaseService;
        }

        //To get Inmate Booking details
        [HttpGet("GetInmateBookingDetails")]
        public IActionResult GetInmateBookingDetails(InmateBookingData inmateData)
        {
            return Ok(_inmateBookingService.GetInmateBookingDetails(inmateData));
        }

        //To get Booking Arrest details
        [HttpGet("GetBookingDataDetails")]
        public IActionResult GetBookingDataDetails(int inmateId, int facilityId)
        {
            return Ok(_inmateBookingService.GetBookingDataDetails(inmateId, facilityId));
        }

        //To get Active Commit details
        [HttpGet("GetActiveCommitDetails")]
        public IActionResult GetActiveCommitDetails(int personId)
        {
            return Ok(_inmateBookingService.GetActiveCommitDetails(personId));
        }

        [HttpGet("GetBookingDataComplete")]
        public IActionResult GetBookingDataComplete(int inmateId, int incarcerationId, int arrestId)
        {
            return Ok(_inmateBookingService.GetBookingDataComplete(inmateId, incarcerationId, arrestId));
        }

        [HttpPost("DeleteInmateActiveCommit")]
        public async Task<IActionResult> DeleteInmateActiveCommit([FromBody] int inmatePrebookId)
        {
            return Ok(await _inmateBookingService.DeleteInmateActiveCommit(inmatePrebookId));
        }

        [HttpPost("UndoInmateBooking")]
        public async Task<IActionResult> UndoInmateBooking([FromBody] InmateBookingData inmateData)
        {
            return Ok(await _inmateBookingService.UndoInmateBooking(inmateData));
        }

        [HttpPost("UndoInmateBookingPopup")]
        public async Task<IActionResult> UndoInmateBookingPopup([FromBody] InmateBookingData inmateData)
        {
            return Ok(await _inmateBookingService.UndoInmateBookingPopup(inmateData));
        }

        [HttpPost("UndoReleaseInmateBooking")]
        public async Task<IActionResult> UndoReleaseInmateBooking([FromBody] InmateBookingData inmateData)
        {
            return Ok(await _inmateBookingService.UndoReleaseInmateBooking(inmateData));
        }

        [HttpGet("GetSiteOptionsReactivate")]
        public IActionResult GetSiteOptionsReactivate(int weekEnder, int arrestId, string bookingNumber)
        {
            return Ok(_inmateBookingService.GetSiteOptionsReactivate(weekEnder, arrestId, bookingNumber));
        }

        #region Inmate Booking Info

        [HttpGet("GetInmateBookingInfo")]
        public IActionResult GetInmateBookingInfo(int arrestId, int incarcerationArrestXrefId)
        {
            return Ok(_inmateBookingService.GetInmateBookingInfo(arrestId, incarcerationArrestXrefId));
        }

        [HttpGet("GetBookingInfoHistory")]
        public IActionResult GetBookingInfoHistory(int arrestId)
        {
            return Ok(_inmateBookingService.GetBookingInfoHistory(arrestId));
        }
        [FuncPermission(465, FuncPermissionType.Edit)]
        [HttpPost("UpdateBookingInfo")]
        public async Task<IActionResult> UpdateBookingInfo([FromBody] BookingInfoArrestDetails arrestDetails)
        {
            return Ok(await _inmateBookingService.UpdateBookingInfo(arrestDetails));
        }

        [HttpPost("PostUpdateSearchOfficiers")]
        public IActionResult PostUpdateSearchOfficiers([FromBody]  string search, int arrestId)
        {
            return Ok(_inmateBookingService.PostUpdateSearchOfficiers(search, arrestId));
        }


        #endregion
        [FuncPermission(701, FuncPermissionType.Edit)]
        [HttpPost("UpdateBookingCompleteFlag")]
        public async Task<IActionResult> UpdateBookingCompleteFlag([FromBody] BookingComplete bookingComplete)
        {
            return Ok(await _inmateBookingService.UpdateBookingCompleteFlag(bookingComplete));
        }

        [HttpPost("UpdateBookDataCompleteFlag")]
        public async Task<IActionResult> UpdateBookDataCompleteFlag([FromBody] BookingComplete bookingComplete)
        {
            return Ok(await _inmateBookingService.UpdateBookDataCompleteFlag(bookingComplete));
        }

        [HttpGet("GetInmateCrimeChargeLst")]
        public IActionResult GetInmateCrimeChargeLst([FromQuery] BookingChargeType chargeType, List<int> arrestIds,
            bool showDeleted)
        {
            return Ok(_inmateBookingService.GetInmateCrimeCharges(arrestIds, chargeType, showDeleted));
        }

        [HttpGet("GetInmateWarrantDetails")]
        public IActionResult GetInmateWarrantDetails(int arrestId, int personId)
        {
            return Ok(_inmateBookingService.GetInmateWarrantDetails(arrestId, personId));
        }

        //Get Crime and CrimeForce History  Details
        [HttpGet("GetCrimeHistory")]
        public IActionResult GetCrimeHistory(int crimeId, int crimeForceId, int prebookChargeId)
        {
            return Ok(_inmateBookingService.GetCrimeHistory(crimeId, crimeForceId, prebookChargeId));
        }

        [FuncPermission(465, FuncPermissionType.Add)]
        [HttpPost("InsertUpdateCrimeDetails")]
        public async Task<IActionResult> InsertUpdateCrimeDetails([FromBody] PrebookCharge chargeDetails)
        {
            return Ok(await _inmateBookingService.InsertUpdateCrimeDetails(chargeDetails));
        }

        [FuncPermission(465, FuncPermissionType.Add)]
        [HttpPost("InsertUpdateWarrantDetails")]
        public IActionResult InsertUpdateWarrantDetails([FromBody] InmatePrebookWarrantVm warrant)
        {
            return Ok(_inmateBookingService.InsertUpdateWarrantDetails(warrant));
        }

        [HttpPost("ReplicateCharges")]
        public async Task<IActionResult> ReplicateChargeDetails([FromBody] PrebookCharge chargeDetails)
        {
            return Ok(await _inmateBookingService.ReplicateChargeDetails(chargeDetails));
        }

        [FuncPermission(465, FuncPermissionType.Delete)]
        [HttpPost("DeleteWarrantDetails")]
        public IActionResult DeleteWarrantDetails([FromBody] int warrantId)
        {
            return Ok(_inmateBookingService.DeleteWarrantDetails(warrantId));
        }

        [FuncPermission(465, FuncPermissionType.Delete)]
        [HttpPost("DeleteUndoCrimeDetails")]
        public async Task<IActionResult> DeleteUndoCrimeDetails([FromBody] PrebookCharge chargeDetails)
        {
            return Ok(await _inmateBookingService.DeleteUndoCrimeDetails(chargeDetails));
        }

        [FuncPermission(465, FuncPermissionType.Delete)]
        [HttpPost("DeleteUndoCrimeForce")]
        public async Task<IActionResult> DeleteUndoCrimeForce([FromBody] PrebookCharge chargeDetails)
        {
            return Ok(await _inmateBookingService.DeleteUndoCrimeForce(chargeDetails));
        }

        [HttpPost("InsertPrebookChargesToCrime")]
        public IActionResult InsertPrebookChargesToCrime([FromBody] List<PrebookCharge> prebookChargeLst)
        {
            return Ok(_inmateBookingService.InsertPrebookChargesToCrime(prebookChargeLst));
        }

        [HttpPost("InsertPrebookWarrantToWarrant")]
        public IActionResult InsertPrebookWarrantToWarrant([FromBody] List<InmatePrebookWarrantVm> prebookWarrants)
        {
            return Ok(_inmateBookingService.InsertPrebookWarrantToWarrant(prebookWarrants));
        }

        #region Inmate Booking Bail

        //Get Inmate Booking Bail Details For Page Load
        [HttpGet("GetInmateBookingBailDetails")]
        public IActionResult GetInmateBookingBailDetails(int arrestId)
        {
            return Ok(_inmateBookingCaseService.GetInmateBookingBailDetails(arrestId));
        }

        //To Get Bail Transaction Details
        [HttpGet("GetBookingBailTransactionDetails")]
        public IActionResult GetBookingBailTransactionDetails(int arrestId)
        {
            return Ok(_inmateBookingCaseService.GetBookingBailTransactionDetails(arrestId));
        }

        //To Get Inmate Booking Bail Save History Details
        [HttpGet("GetBailSaveHistory")]
        public IActionResult GetBailSaveHistory(int arrestId)
        {
            return Ok(_inmateBookingCaseService.GetBailSaveHistory(arrestId));
        }

        //Delete Or Undo For Bail Transaction Table
        [FuncPermission(470, FuncPermissionType.Delete)]
        [HttpPost("DeleteUndoBailTransaction")]
        public async Task<IActionResult> DeleteUndoBailTransaction([FromBody] int bailTransactionId)
        {
            return Ok(await _inmateBookingCaseService.DeleteUndoBailTransaction(bailTransactionId));
        }

        //To Get Bail Companies And Bail Agencies Details
        [HttpGet("GetBailCompanyDetails")]
        public IActionResult GetBailCompanyDetails()
        {
            return Ok(_inmateBookingCaseService.GetBailCompanyDetails());
        }

        //Update Bail Details in Arrest Table
        [FuncPermission(465, FuncPermissionType.Edit)]
        [HttpPost("UpdateBail")]
        public async Task<IActionResult> UpdateBail([FromBody] BailDetails bailDetails)
        {
            return Ok(await _inmateBookingCaseService.UpdateBail(bailDetails));
        }


        //To Get BailAgentHistory Details
        [HttpGet("GetBailAgentHistoryDetails")]
        public IActionResult GetBailAgentHistoryDetails(int bailAgentId)
        {
            return Ok(_inmateBookingCaseService.GetBailAgentHistoryDetails(bailAgentId));
        }

        //Insert and Update For Bail Agent
        [HttpPost("InsertUpdateBailAgent")]
        public async Task<IActionResult> InsertUpdateBailAgent([FromBody] BailAgentVm agent)
        {
            return Ok(await _inmateBookingCaseService.InsertUpdateBailAgent(agent));
        }

        //Delete For Bail Agent Table
        [HttpPost("DeleteBailAgent")]
        public async Task<IActionResult> DeleteBailAgent([FromBody] int bailAgentId)
        {
            return Ok(await _inmateBookingCaseService.DeleteBailAgent(bailAgentId));
        }

        //Insert For Bail Transaction Table
        [HttpPost("InsertBailTransaction")]
        public async Task<IActionResult> InsertBailTransaction([FromBody] BailTransactionDetails bailTransaction)
        {
            return Ok(await _inmateBookingCaseService.InsertBailTransaction(bailTransaction));
        }

        //For getting Booking Bail Amount Details
        [HttpGet("GetBookingBailAmount")]
        public IActionResult GetBookingBailAmount(int arrestId, int personId)
        {
            return Ok(_inmateBookingCaseService.GetBookingBailAmount(arrestId, personId));
        }

        #endregion

        [FuncPermission(465, FuncPermissionType.Add)]
        [HttpGet("GetSupplementalBookingDetails")]
        public IActionResult GetSupplementalBookingDetails(int inmateId, int incarcerationId)
        {
            return Ok(_inmateBookingService.GetSupplementalBookingDetails(inmateId, incarcerationId));
        }

        [HttpPost("UpdateBookSupervisorClearFlag")]
        public async Task<IActionResult> UpdateBookSupervisorClearFlag([FromBody] BookingComplete bookingComplete)
        {
            return Ok(await _inmateBookingService.UpdateBookSupervisorClearFlag(bookingComplete));
        }

        [FuncPermission(731, FuncPermissionType.Edit)]
        [HttpPost("UpdateBookSupervisorCompleteFlag")]
        public async Task<IActionResult> UpdateBookSupervisorCompleteFlag([FromBody] BookingComplete bookingComplete)
        {
            return Ok(await _inmateBookingService.UpdateBookSupervisorCompleteFlag(bookingComplete));
        }

        [FuncPermission(465, FuncPermissionType.Edit)]
        [HttpGet("GetBookingChargeWarrantEdit")]
        public IActionResult GetBookingChargeWarrantEdit()
        {
            return Ok();
        }

        [FuncPermission(465, FuncPermissionType.Delete)]
        [HttpGet("GetBookingChargeWarrantDelete")]
        public IActionResult GetBookingChargeWarrantDelete()
        {
            return Ok();
        }

        [FuncPermission(465, FuncPermissionType.Add)]
        [HttpGet("CourtActiveCommit")]
        public IActionResult CourtActiveCommit()
        {
            return Ok();
        }

        [FuncPermission(465, FuncPermissionType.Add)]
        [HttpPost("ReplicateCharge")]
        public IActionResult ReplicateCharge()
        {
            return Ok();
        }

        [FuncPermission(465, FuncPermissionType.Access)]
        [HttpGet("HistoryCharge")]
        public IActionResult HistoryCharge()
        {
            return Ok();
        }

        [FuncPermission(465, FuncPermissionType.Add)]
        [HttpPost("OpenChargePrebook")]
        public IActionResult OpenChargePrebook()
        {
            return Ok();
        }

        [FuncPermission(465, FuncPermissionType.Edit)]
        [HttpGet("GetBail")]
        public IActionResult GetBail()
        {
            return Ok();
        }

        [FuncPermission(463, FuncPermissionType.Add)]
        [HttpPost("InsertReactiveCase")]
        public IActionResult InsertReactiveCase()
        {
            return Ok();
        }

        [HttpGet("CheckAppointmentConflictForInfo")]
        public IActionResult CheckAppointmentConflictForInfo([FromQuery]BookingInfoArrestDetails arrestDetails)
        {
            return Ok(_inmateBookingService.GetCourtdateArraignment(arrestDetails));
        }

    }
}
