using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServerAPI.Services;
using ServerAPI.ViewModels;
using System.Collections.Generic;
using Newtonsoft.Json;
using ServerAPI.Authorization;
using ServerAPI.Policies;

namespace ServerAPI.Controllers
{
    [Route("[controller]")]
    public class CommonController : ControllerBase
    {
        private readonly ICommonService _commonService;
        private readonly IBookingService _bookingService;
        private readonly IBookingDetailService _bookingDetailService;
        private readonly IUserPermissionPolicy _userPermissionPolicy;

        public CommonController(ICommonService commonService, IBookingService bookingService, 
            IBookingDetailService iBookingDetailService, IUserPermissionPolicy userPermissionPolicy)
        {
            _commonService = commonService;
            _bookingService = bookingService;
            _bookingDetailService = iBookingDetailService;
            _userPermissionPolicy = userPermissionPolicy;
        }

        [HttpPost("GetLookups")]
        public IActionResult GetLookups([FromBody] string[] value) => Ok(_commonService.GetLookups(value));

        [HttpGet("GetDropdownValues")]
        public IActionResult GetDropdownValues(string[] lookupTypes, string[] lookupReferences) => 
            Ok(_commonService.GetDropdownValues(lookupTypes, lookupReferences));

        [HttpGet("GetAgencies")]
        public IActionResult GetAgenciesByType(string type) => Ok(_commonService.GetAgenciesByType(type));

        [AllowAnonymous]
        [HttpGet("GetAgencyByOri")]
        public IActionResult GetAgencyByOri(string oriNumber) => Ok(_commonService.GetAgencyByOri(oriNumber));

        [HttpGet("GetOfficerDetails")]
        public IActionResult GetPersonnelDetails() => Ok(_commonService.GetPersonnelDetails());

        [HttpGet("GetPersonnelByUserName")]
        public IActionResult GetPersonnelByUsername(string username) => 
            Ok(_commonService.GetPersonnelByUsername(username));

        //Get User Control Field Names
        [HttpPost("GetFieldNames")]
        public IActionResult GetFieldNames([FromBody] List<UserControlFieldTags> controlFields) => 
            Ok(_commonService.GetFieldNames(controlFields));

        [HttpGet("GetEvidenceAgency")]
        public IActionResult GetEvidenceAgency(bool arrestFlag, bool courtFlag) => 
            Ok(_commonService.GetEvidenceAgency(arrestFlag, courtFlag));

        [HttpGet("GetEvidencePersonnel")]
        public IActionResult GetEvidencePersonnel(string type) => Ok(_commonService.GetPersonnel(type));

        /// <summary>
        /// To search personnel or officer details
        /// </summary>
        /// <param name="personnelSearchVm"></param>
        /// <returns></returns>
        [HttpPost("LoadPersonnelSearch")]
        public IActionResult LoadPersonnelSearch([FromBody] PersonnelSearchVm personnelSearchVm) => 
            Ok(_commonService.GetPersonnelSearchDetails(personnelSearchVm));

        [HttpGet("GetDbDetails")]
        [AllowAnonymous]
        public IActionResult GetDdDetails() => Ok(_commonService.GetDbDetails());

        /// <summary>
        /// Get site options value
        /// </summary>
        /// <param name="optionName"></param>
        /// <param name="optionVariable"></param>
        /// <returns></returns>
        [HttpGet("GetSiteOptions")]
        public IActionResult GetSiteOptions(string optionName = "", string optionVariable = "") => 
            Ok(_commonService.GetSiteOptionValue(optionName, optionVariable));

        [HttpPost("GetSiteOptionList")]
        public IActionResult GetSiteOptionList([FromBody] string[] siteOptionNames) => 
            Ok(_commonService.GetSiteOptions(siteOptionNames));

        [HttpGet("GetIncarcerationFormsDetails")]
        public IActionResult GetIncarcerationFormsDetails(int incarcerationId, string filterName) => 
            Ok(_bookingDetailService.GetIncarcerationFormsDetails(incarcerationId, filterName));

        [HttpPost("DeleteUndoIncarcerationForm")]
        public async Task<IActionResult> DeleteUndoIncarcerationForm([FromBody] IncarcerationForms incFrom) => 
            Ok(await _bookingDetailService.DeleteUndoIncarcerationForm(incFrom));

        [HttpPost("UpdateFormInterfaceBypassed")]
        public async Task<IActionResult> UpdateFormInterfaceBypassed([FromBody] IncarcerationForms incFrom) => 
            Ok(await _bookingDetailService.UpdateFormInterfaceBypassed(incFrom));

        [FuncPermission(492, FuncPermissionType.Edit)]
        [HttpPost("InsertInmateTracking")]
        public IActionResult InsertInmateTracking([FromBody] InmateTrackingVm ob) => Ok(_commonService.InsertInmateTracking(ob));
        [FuncPermission(532, FuncPermissionType.Edit)]
        [HttpPost("CheckOutInsertInmateTracking")]
        public IActionResult CheckOutInsertInmateTracking([FromBody] InmateTrackingVm ob) => Ok(_commonService.InsertInmateTracking(ob));
        [FuncPermission(533, FuncPermissionType.Edit)]
        [HttpPost("ConflictEditTracking")]
        public IActionResult ConflictEditTracking() => Ok();

        [AllowAnonymous]
        [HttpGet("GetExecSp")]
        public IActionResult GetExecSp(string spName, string parameters) => 
            Ok(_commonService.GetExecSp(spName, JsonConvert.DeserializeObject<KeyValuePair<string, string>[]>(parameters)));

        [HttpGet("GetBookings")]
        public IActionResult GetBookings(int incarcerationId, bool toBindCharge) => 
            Ok(_bookingService.GetBookings(incarcerationId, toBindCharge));

        [HttpGet("GetCourtCommitFlag")]
        public IActionResult GetCourtCommitFlag(int arrestId) => Ok(arrestId > 0 && _commonService.GetCourtCommitFlag(arrestId));

        [HttpGet("GetIdleTimeOut")]
        public IActionResult GetIdleTimeOut() => Ok(_commonService.GetIdleTimeOut());

        [HttpGet("GetAppDetails")]
        public IActionResult GetAppDetails() => Ok(_userPermissionPolicy.GetAppPermission());

        [HttpGet("GetHousingUnitListIds")]
        public IActionResult GetHousingUnitListIds(int housingGroupId) => 
            Ok(_commonService.GetHousingUnitListIds(housingGroupId));

        [HttpPost("GetIncidentLookups")]
        public IActionResult GetIncidentLookups([FromBody] string[] value) => Ok(_commonService.GetIncidentLookups(value));
    }
}
