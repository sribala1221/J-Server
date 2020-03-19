using System;
using System.Collections.Generic;
using GenerateTables.Models;

namespace ServerAPI.ViewModels
{
    public class RecordsCheckVm
    {
        public int RecordsCheckRequestId { get; set; }
        public string RequestType { get; set; }
        public int? RequestFacilityId { get; set; }
        public string RequestNote { get; set; }
        public string[] RequestAction { get; set; }
        public string[] ResponseAction { get; set; }
        public string[] ClearAction { get; set; }
        public string BypassNote { get; set; }
        public string ResponseNote { get; set; }
        public string ClearNote { get; set; }
        public DateTime? RequestDate { get; set; }
        public bool ByPassFlag { get; set; }
        public bool DeleteFlag { get; set; }
        public DateTime? ResponseDate { get; set; }
        public int? PersonId { get; set; }
        public DateTime? ByPassDate { get; set; }
        public bool ResponseFlag { get; set; }
        public bool ClearFlag { get; set; }
        public bool RequestFlag { get; set; }
        public bool SupervisorReviewFlag { get; set; }
        public DateTime? ClearDate { get; set; }
        public DateTime? SupervisorReviewDate { get; set; }
        public string InmateNumber { get; set; }
        public int? RequestBy { get; set; }
        public int? ResponseBy { get; set; }
        public int? SupervisorBy { get; set; }
        public int? ByPassBy { get; set; }
        public int? ClearBy { get; set; }
        public string PersonLastName { get; set; }
        public string PersonFirstName { get; set; }
        public string OfficerBadgeNumber { get; set; }
        public int Personneld { get; set; }       
        public string Note { get; set; }
        public string[] Action { get; set; }
        public RecordsCheckStatus RecordsStatus { get; set; }
        public PersonnelVm RequestOfficer { get; set; }
        public PersonnelVm ResponseOfficer { get; set; }
        public PersonnelVm ClearOfficer { get; set; }
        public PersonnelVm SuperviseOfficer { get; set; }
        public PersonnelVm ByPassOfficer { get; set; }
        public InmateHousing PersonDetails { get; set; }
        public string PersonSuffix { get; set; }
        public string PersonMiddleName { get; set; }
        public string BypassReason { get; set; }
        public DateTime? BypassDate { get; set; }
    }

    public enum RecordsCheckStatus
    {
        Request,
        Response,
        Clear,
        ByePass,
        Supervisor
    }

    public class RecordsCheckHistoryVm
    {
        public List<RecordsCheckVm> LstRecordHisty { get; set; }
        public List<Lookup> RecordCheckType { get; set; }
        public List<KeyValuePair<int, string>> ActionList { get; set; }
        public List<Facility> FacilityList { get; set; }
    }
}
