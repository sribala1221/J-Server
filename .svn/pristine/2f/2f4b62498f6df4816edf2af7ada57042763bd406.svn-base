using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServerAPI.Services;
using ServerAPI.ViewModels;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using jsreport.Client;
using jsreport.Types;
using System;
using ServerAPI.Authorization;
using System.Net;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;

namespace ServerAPI.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class BookingController : ControllerBase
    {

        private readonly IBookingService _bookingService;
        private readonly ICommonService _commonService;
        private readonly IConfiguration _configuration;
        private readonly IBookingReleaseService _bookingReleaseService;
        private readonly ISupervisorService _supervisorService;
        private readonly IInmateSummaryPdfService _inmateSummaryPdfService;
        private readonly IBookingProgressService _bookingProgressService;
        private readonly IRequestService _requestService;
        private readonly IBookingTaskService _bookingTaskService;
        private readonly IBookingVerifyIdService _bookingVerify;
        private readonly ILiveScanService _liveScanService;

        public BookingController(IBookingService bookingService, IInmateSummaryPdfService inmateSummaryPdfService,
            ICommonService commonService, IConfiguration configuration, IBookingReleaseService bookingReleaseService,
            ISupervisorService supervisorService, IBookingProgressService bookingProgressService,
            IRequestService requestService, IBookingTaskService bookingTaskService, 
            IBookingVerifyIdService bookingVerify ,ILiveScanService liveScanService)
        {
            _bookingService = bookingService;
            _commonService = commonService;
            _configuration = configuration;
            _bookingReleaseService = bookingReleaseService;
            _inmateSummaryPdfService = inmateSummaryPdfService;
            _supervisorService = supervisorService;
            _bookingProgressService = bookingProgressService;
            _requestService = requestService;
            _bookingTaskService = bookingTaskService;
            _bookingVerify = bookingVerify;
            _liveScanService =liveScanService;
        }

        // Get Booking In Progress counts to bind Left Side Grid values.
        [FuncPermission(700, FuncPermissionType.Access)]
        [HttpGet("GetBookingInProgress")]
        public IActionResult GetBookingInProgress([FromQuery] BookingInputsVm iBooking) =>
            Ok(_bookingProgressService.GetBookingInProgress(iBooking));

        [FuncPermission(700, FuncPermissionType.Access)]
        // Get Booking In Progress details to bind right Side values.
        [HttpGet("GetBookingInProgressDetails")]
        public IActionResult GetBookingInProgressDetails([FromQuery] BookingInputsVm iBooking) => 
            Ok(_bookingProgressService.GetBookingInProgressDetails(iBooking));

        //Get Booking Note Details        
        [HttpGet("GetBookingNoteDetails")]
        public IActionResult GetBookingNoteDetails(int arrestId, bool deleteFlag) => 
            Ok(_bookingService.GetBookingNoteDetails(arrestId, deleteFlag));

        //Get User Control required Fields.
        [HttpGet("GetExternalAttachmentDetails")]
        public IActionResult GetExternalAttachmentDetails(int inmateId) => 
            Ok(_bookingService.GetExternalAttachmentDetails(inmateId));

        [FuncPermission(485, FuncPermissionType.Edit)]
        [HttpPost("UpdateExternalAttachment")]
        public async Task<IActionResult> UpdateExternalAttachment([FromBody] ExternalAttachmentsVm eaAvm) => 
            Ok(await _bookingService.UpdateExternalAttachment(eaAvm));

        [HttpGet("GetExternalAttachment")]
        public IActionResult GetExternalAttachment(int attachmentId)
        {
            PrebookAttachment attachmentFile = _commonService.GetAttachment(attachmentId);
            string filePath = attachmentFile.AttachmentFile;
            if (string.IsNullOrEmpty(filePath))
                return Ok("File Not Found!");
            if (!attachmentFile.AppletsSavedIsExternal && !System.IO.File.Exists(filePath))
                return Ok("File Not Found!");
            if (!System.IO.File.Exists(filePath))
                return Ok("File Not Found!");
            if (attachmentFile.AppletsSavedIsExternal)
            {
                WebRequest req = WebRequest.Create(filePath);
                WebResponse response = req.GetResponse();
                Stream stream = response.GetResponseStream();
                return Ok(stream);
            }
            return File(new FileStream(filePath, FileMode.Open), "application/octet-stream", Path.GetFileName(filePath));

        }

        [FuncPermission(465, FuncPermissionType.Delete)]
        [HttpPost("UpdateBookingNote")]
        public async Task<IActionResult> UpdateBookingNote([FromBody] BookingNoteDetailsVm value) => 
            Ok(await _bookingService.DeleteBookingNote(value));

        [FuncPermission(465, FuncPermissionType.Add)]
        [HttpGet("LoadInmateBookings")]
        public IActionResult LoadInmateBookings(int inmateId) => Ok(_bookingService.LoadInmateBookings(inmateId));

        [HttpPost("SaveBookingNoteDetails")]
        public async Task<IActionResult> SaveBookingNoteDetails([FromBody] BookingNoteDetailsVm noteDetails) => 
            Ok(await _bookingService.SaveBookingNoteDetails(noteDetails));

        [HttpGet("GetBookingClearDetails")]
        public IActionResult GetBookingClearDetails(int incarcerationId) => 
            Ok(_bookingService.GetBookingClearDetails(incarcerationId));

        [FuncPermission(470, FuncPermissionType.Add)]
        [HttpGet("GetBookingClearDetailsBail")]
        public IActionResult GetBookingClearDetailsBail(int incarcerationId) => 
            Ok(_bookingService.GetBookingClearDetails(incarcerationId));

        [HttpGet("GetClearHistoryDetails")]
        public IActionResult GetClearHistoryDetails(int arrestId) => Ok(_bookingService.GetClearHistoryDetails(arrestId));

        [HttpGet("GetBookingClearlist")]
        public IActionResult GetBookingClearlist(int arrestId) => Ok(_bookingService.GetBookingClearlist(arrestId));

        [FuncPermission(476, FuncPermissionType.Edit)]
        [HttpPost("UpdateClear")]
        public async Task<IActionResult> UpdateClear([FromBody] BookingClearVm bookingClear) => 
            Ok(await _bookingService.UpdateClear(bookingClear));

        [HttpGet("GetOverallIncarceration")]
        public IActionResult GetOverallIncarceration(int incarcerationId, int userControlId) => 
            Ok(_bookingReleaseService.getOverallIncarceration(incarcerationId, userControlId));
        [FuncPermission(477, FuncPermissionType.Edit)]
        [HttpPost("UpdateOverallIncarceration")]
        public async Task<IActionResult> UpdateOverallIncarceration(
            [FromBody] OverallIncarceration overallIncarceration) => 
            Ok(await _bookingReleaseService.UpdateOverallIncarceration(overallIncarceration));

        [FuncPermission(477, FuncPermissionType.Access)]
        [HttpPut("AccessBookingOverall")]
        public IActionResult AccessBookingOverall() => Ok();

        [HttpGet("CheckOverallSentDetails")]
        public IActionResult CheckOverallSentDetails(int arrestId, int incarcerationId) => 
            Ok(_bookingService.CheckOverallSentDetails(arrestId, incarcerationId));

        [HttpPost("GetSentenceChargeDetails")]
        public IActionResult GetSentenceChargeDetails([FromBody] int[] arrestId) => 
            Ok(_bookingService.GetSentenceCharges(arrestId));

        [HttpGet("GetCurrentStatus")]
        public IActionResult GetCurrentStatus(int incarcerationId) => Ok(_bookingService.GetCurrentStatus(incarcerationId));

        [HttpGet("GetOERCDetails")]
        public IActionResult GetOercDetails() => Ok(_bookingService.GetOercDetails());

        [HttpGet("GetCautionflag")]
        public IActionResult GetCautionflag(int personId) => Ok(_bookingService.GetCautionflag(personId));

        [HttpGet("GetBail")]
        public IActionResult GetBail(int incarcerationId) => Ok(_bookingService.GetTotalBailAmountandNoBailFlag(incarcerationId));

        [FuncPermission(488, FuncPermissionType.Edit)]
        [HttpPost("UpdateOverallSentence")]
        public async Task<IActionResult> UpdateOverallSentence([FromBody] OverallSentence overallSentence) => 
            Ok(await _bookingService.UpdateOverAllSentence(overallSentence));

        [FuncPermission(476, FuncPermissionType.Edit)]
        [HttpPost("UndoClearBook")]
        public async Task<IActionResult> UndoClearBook([FromBody] UndoClearBook undoClearBook) =>
            Ok(await _bookingService.UndoClearBook(undoClearBook));

        [HttpGet("GetActiveBooking")]
        public IActionResult GetActiveBooking(int facilityId, BookingActiveStatus status) => 
            Ok(_bookingService.GetActiveBooking(facilityId, status));

        [FuncPermission(477, FuncPermissionType.Edit)]
        [HttpPut("UpdateBookingComplete")]
        public async Task<IActionResult> UpdateBookingComplete([FromBody] BookingComplete bookingComplete) => 
            Ok(await _bookingService.UpdateBookingComplete(bookingComplete));

        [HttpGet("GetBookingSentence")]
        public IActionResult GetBookingSentence(int incarcerationId) => Ok(_bookingService.GetBookingSentence(incarcerationId));

        [HttpGet("GetPendingRequestDetails")]
        public IActionResult GetPendingRequestDetails(int facilityId, int requestLookupId, int showInFlag) => 
            Ok(_requestService.GetBookingPendingReqDetail(facilityId, requestLookupId, showInFlag));

        [HttpGet("GetAssignedRequestDetails")]
        public IActionResult GetAssignedRequestDetails(int requestLookupId, int showInFlag) => 
            Ok(_requestService.GetBookingAssignedReqDetail(requestLookupId, showInFlag));

        [HttpPut("UpdateRequest")]
        public async Task<IActionResult> UpdateRequest([FromBody] RequestClear requestClear) => 
            Ok(await _bookingService.UpdateRequest(requestClear));

        [HttpPost("UpdateClearRequest")]
        public async Task UpdateClearRequest([FromBody] RequestClear requestClear) => 
            Ok(await _bookingService.UpdateClearRequest(requestClear));

        [HttpGet("GetBookingPrebookForms")]
        public IActionResult GetBookingPrebookForms(int incarcerationId) => 
            Ok(_bookingService.GetBookingPrebookForms(incarcerationId));

        [HttpGet("GetFormDetails")]
        public IActionResult GetFormDetails(int incarcerationId, int incarcerationArrestId, int arrestId, int formTemplateId) => 
            Ok(_bookingService.LoadFormDetails(incarcerationId, incarcerationArrestId, arrestId, formTemplateId));

        // Get Case Sheet details for jsreport	
        [HttpGet("GetCaseSheetDetails")]
        public IActionResult GetCaseSheetDetails([FromQuery] FormDetail formDetail) => 
            Ok(_inmateSummaryPdfService.GetCaseSheetDetails(formDetail));

        // Generate Case Sheet details jsreport	
        [HttpPost("CreateSheetReport")]
        public async Task<IActionResult> CreateSheetReport([FromBody] InmateSummaryPdfVm details,
            [FromQuery] string reportName)
        {
            details.InmatePdfHeaderDetails.SummaryHeader = reportName;
            details.CustomLabel = _commonService.GetCustomMapping(); ;
            ReportingService rs = new ReportingService(_configuration.GetSection("SiteVariables")["ReportUrl"]);
            string jsonData = JsonConvert.SerializeObject(details);
            //try catch used for to handle 2 exceptions (10061 & 404) from jsreport.
            try
            {
                _commonService.atimsReportsContentLog(reportName, jsonData);
                Report report = await rs.RenderByNameAsync(reportName, jsonData);
                FileContentResult result =
                    new FileContentResult(_commonService.ConvertStreamToByte(report.Content), "application/pdf");

                return result;
            }
            catch (Exception ex)
            {
                //10061 is status code for no connection could be made because the target machine actively refused it
                //404 for file not found in jsreport
                return ex.InnerException != null ? Ok(10061) : Ok(404);
            }
        }

        [HttpGet("GetBookingOverviewDetails")]
        public IActionResult GetBookingOverviewDetails(int facilityId) => 
            Ok(_bookingProgressService.GetBookingOverviewDetails(facilityId));

        [HttpGet("GetAllTasks")]
        public IActionResult GetAllTasks(int facilityId) => Ok(_bookingTaskService.GetAllTasks(facilityId));

        [HttpGet("GetTaskInmates")]
        public IActionResult GetTaskInmates(int taskLookupId, int facilityId) => 
            Ok(_bookingTaskService.GetTaskInmates(taskLookupId, facilityId));

        [HttpPost("AssignTask")]
        public async Task<IActionResult> AssignTask([FromBody] TaskOverview taskOverview) => 
            Ok(await _bookingTaskService.AssignTaskAsync(taskOverview));

        [HttpGet("GetInmateAllTasks")]
        public IActionResult GetInmateAllTasks(int inmateId, int taskLookupId = 0) => 
            Ok(_bookingTaskService.GetInmateAllTasks(inmateId, taskLookupId));

        [HttpGet("GetKeeperNoKeeper")]
        public IActionResult GetKeeperNoKeeper(int incarcerationId) => Ok(_bookingTaskService.GetKeeperNoKeeper(incarcerationId));

        [HttpPost("SaveNoKeeperValues")]
        public IActionResult SaveNoKeeperValues([FromBody] NoKeeperHistory noKeeperHistory) => 
            Ok(_bookingTaskService.SaveNoKeeperValues(noKeeperHistory));

        [HttpPost("UpdateCompleteTasks")]
        public IActionResult UpdateCompleteTasks([FromBody] TasksCountVm taskDetails) => 
            Ok(_bookingTaskService.UpdateCompleteTasks(taskDetails));

        [HttpGet("GetBookingSheet")]
        public IActionResult GetBookingSheetDetails(int inmateId, InmateSummaryType summaryType, int incarcerationId,
            bool autofill) => Ok(_inmateSummaryPdfService.GetBookingSheetDetails(inmateId, summaryType, incarcerationId,
                autofill));

        [HttpGet("GetAssessmentDetails")]
        public IActionResult GetAssessmentDetails(int facilityId) => Ok(_bookingTaskService.GetAssessmentDetails(facilityId));

        [HttpPost("UpdateAssessmentComplete")]
        public async Task<IActionResult> UpdateAssessmentComplete([FromBody] BookingComplete bookingComplete) => 
            Ok(await _bookingTaskService.UpdateAssessmentComplete(bookingComplete));

        [HttpPost("UpdateTaskPriority")]
        public async Task<IActionResult> UpdateTaskPriority([FromBody] TaskOverview taskOverview) => 
            Ok(await _bookingTaskService.UpdateTaskPriority(taskOverview));

        [HttpGet("GetBookClearAccess")]
        public IActionResult GetBookClearAccess(int incarcerationId) => Ok(_bookingService.GetBookingClearDetails(incarcerationId));

        [FuncPermission(730, FuncPermissionType.Access)]
        [HttpGet("GetBookingSupervisor")]
        public IActionResult GetBookingSupervisor(int facilityId) => Ok(_supervisorService.GetBookingSupervisor(facilityId));

        [HttpGet("GetForceCharge")]
        public IActionResult GetForceCharge(int facilityId) => Ok(_supervisorService.GetForceCharge(facilityId));

        [HttpGet("GetRecordsCheckResponse")]
        public IActionResult GetRecordsCheckResponse(int facilityId) => Ok(_supervisorService.GetRecordsCheckResponse(facilityId));

        [HttpGet("GetReviewBooking")]
        public IActionResult GetReviewBooking(int facilityId, bool isClear) => Ok(_supervisorService.GetReviewBooking(facilityId, isClear));

        [HttpGet("GetOverallReview")]
        public IActionResult GetOverallReview(int facilityId) => Ok(_supervisorService.GetOverallReview(facilityId));

        [HttpGet("GetCompleteTask")]
        public IActionResult GetCompleteTask(int aoTaskLookupId) => Ok(_bookingTaskService.GetCompleteTask(aoTaskLookupId));

        [HttpPost("setCompleteForceCharge")]
        public IActionResult CompleteForceCharge([FromBody] PrebookCharge charges, int option) => 
            Ok(_supervisorService.CompleteForceCharge(charges, option));

        [FuncPermission(731, FuncPermissionType.Edit)]
        [HttpPost("UpdateSupervisorComplete")]
        public async Task<IActionResult> UpdateSupervisorComplete([FromBody] BookingComplete bookingComplete) =>
            Ok(await _supervisorService.UpdateReviewComplete(bookingComplete));

        [HttpGet("GetReleaseValidation")]
        public IActionResult GetReleaseValidation(int incarcerationId) => 
            Ok(_supervisorService.GetReleaseValidation(incarcerationId));

        [HttpGet("GetBookingOverview")]
        public IActionResult GetBookingOverview(int facilityId, int inmateId = 0) => 
            Ok(_bookingProgressService.GetBookingOverview(facilityId, inmateId));

        [HttpGet("GetAllCompleteTasks")]
        public IActionResult GetAllCompleteTasks(int inmateId) => Ok(_bookingService.GetAllCompleteTasks(inmateId));

        [HttpGet("GetVerifyInmateDetail")]
        public IActionResult GetVerifyInmateDetail(int facilityId) => Ok(_bookingVerify.GetVerifyInmateDetail(facilityId));

        [HttpGet("GetDuplicateRecords")]
        public IActionResult GetDuplicateRecords(BookingVerifyDataVm verifyDataVm) => 
            Ok(_bookingVerify.GetDuplicateRecords(verifyDataVm));
        
        [HttpPost("UpdateVerifyDetail")]
        public async Task<IActionResult> UpdateVerifyDetail([FromBody] BookingVerifyDataVm verifyDataVm) => 
            Ok(await _bookingVerify.UpdateVerifyDetail(verifyDataVm));

        [HttpGet("GetVerifyInmate")]
        public IActionResult GetVerifyInmate(int incarcerationId) => Ok(_bookingVerify.GetVerifyInmate(incarcerationId));
        [HttpPost("CreateInmate")]
        public async Task<IActionResult> CreateInmate([FromBody] PersonVm personVm) => 
            Ok(await _bookingVerify.CreateInmate(personVm));

        [HttpGet("GetParticularPersonnel")]
        public IActionResult GetParticularPersonnel(string personnelSearch) => 
            Ok(_bookingVerify.GetParticularPersonnel(personnelSearch));


        [HttpGet("EditSearchOfficierDetails")]
        public IActionResult EditSearchOfficierDetails(int arrestId) => 
            Ok(_bookingVerify.EditSearchOfficierDetails(arrestId));

        [FuncPermission(465, FuncPermissionType.Access)]
        [HttpGet("EditNoteType")]
        public IActionResult EditNoteType() => Ok();

        [FuncPermission(476, FuncPermissionType.Access)]
        [HttpGet("ClearBookHistory")]
        public IActionResult ClearBookHistory() => Ok();

        //Live service
        [HttpGet("GetLiveScan")]
        public IActionResult GetLiveScan(int inmateId, int userControlId) => 
            Ok(_liveScanService.GetLiveScan(inmateId, userControlId));

        [HttpGet("PreviewLiveScanPayLoad")]
        public IActionResult PreviewLiveScanPayLoad(string arrestIds, int exportId) =>
            Ok(_liveScanService.PreviewLiveScanPayLoad(arrestIds, exportId));

        [FuncPermission(961, FuncPermissionType.Edit)]
        [HttpPut("EditUpdateVerifyDetail")]
        public IActionResult EditUpdateVerifyDetail() => Ok();

        [FuncPermission(940, FuncPermissionType.Edit)]
        [HttpPut("EditVerifyIdDetail")]
        public IActionResult EditVerifyIdDetail() => Ok();

        [FuncPermission(962, FuncPermissionType.Edit)]
        [HttpPut("EditVerifyLiveScanDetails")]
        public IActionResult EditVerifyLiveScanDetails() => Ok();

        [FuncPermission(476, FuncPermissionType.Delete)]
        [HttpDelete("UndoClearBooking")]
        public IActionResult UndoClearBooking() => Ok();
    }
}
