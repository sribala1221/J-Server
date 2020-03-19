using System;
using System.Collections.Generic;

namespace ServerAPI.ViewModels
{
    public enum AltSentStatus
    {
        All,
        Pending,
        Approved,
        Scheduled
    }
    public class AltSentProgramGrid
    {
        public int AltSentProgramId { get; set; }
        public string AltSentProgramAbbr { get; set; }
        public int Cnt { get; set; }
    }
    public class AltSentRequestDetails
    {
        public List<AltSentenceRequest> RequestGrid { get; set; }
        public List<KeyValuePair<AltSentStatus, int>> StatusGrid { get; set; }
        public List<KeyValuePair<AltSentElapsed, int>> ElapseGrid { get; set; }
        public List<AltSentProgramGrid> ProgramGrid { get; set; }
        public List<KeyValuePair<string, int>> ScheduledGrid { get; set; }
    }
    public class AltSentenceRequest
    {
        public IEnumerable<KeyValuePair<int, string>> FacilityList { get; set; }
        public IEnumerable<KeyValuePair<int, string>> ProgramList { get; set; }
        public int? AltSentRequestId { get; set; }
        public int PersonnelId { get; set; }
        public int FacilityId { get; set; }
        public string FacilityName { get; set; }
        public DateTime? RequestDate { get; set; }
        public string RequestNote { get; set; }
        public int InmateId { get; set; }
        public PersonVm InmateInfo { get; set; }
        public string ApprovalNote { get; set; }
        public DateTime? ScheduleDate { get; set; }
        public string ScheduleNote { get; set; }
        public bool ApproveFlag { get; set; }
        public bool DeleteFlag { get; set; }
        public bool RejectFlag { get; set; }
        public int CreatedById { get; set; }
        public string CreatedByLastName { get; set; }
        public string CreatedByFirstName { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int? UpdatedById { get; set; }
        public string UpdatedByLastName { get; set; }
        public string UpdatedByFirstName { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public int? DeletedById { get; set; }
        public string DeletedByLastName { get; set; }
        public string DeletedPersonnelNumber { get; set; }
        public DateTime? DeletedDate { get; set; }
        public int? RejectedById { get; set; }
        public string RejectedByLastName { get; set; }
        public string RejectedPersonnelNumber { get; set; }
        public DateTime? RejectedDate { get; set; }
        public int? ApprovedById { get; set; }
        public string ApprovedByLastName { get; set; }
        public string ApprovedPersonnelNumber { get; set; }
        public DateTime? ApprovedDate { get; set; }
        public List<KeyValuePair<int, string>> Programs { get; set; }
        public HousingDetail HousingDetail { get; set; }
        public string PhotoFilePath { get; set; }
        public string OfficerLastName { get; set; }
        public string OfficerFirstName { get; set; }
        public string OfficerMiddleName { get; set; }
        public string OfficerBadgeNumber { get; set; }
        public string PersonnelNumber { get; set; }
        public string HistoryList { get; set; }
    }
    public enum AltSentElapsed
    {
        // Commented text will be a output text of the corresponding property
        All,
        Hours0124, //01 - 24 Hours
        Hours2548, //25 - 48 Hours
        Hours4972, //49 - 72 Hours
        Hours73 //73 + Hours
    }
    public class ApproveRequestDetails
    {
        public AltSentenceRequest RequestInfo { get; set; }
        public List<AppealHistory> AppealHistory { get; set; }
    }
    public class ApproveRequest
    {
        public bool Approveflag { get; set; }
        public bool RejectFlag { get; set; }
        public bool IsDelete { get; set; }
        public string ApprovalNote { get; set; }
        public string StrPersonnelDelete { get; set; }
        public List<string> HistoryList { get; set; }
    }
    public class ScheduleDetails
    {
        public int FacilityId { get; set; }
        public int InmateId { get; set; }
        public int RequestId { get; set; }
        public DateTime? ScheduleDateTime { get; set; }
        public bool IsPreviousNextDate { get; set; }
        public IEnumerable<KeyValuePair<int, string>> FacilityList { get; set; }
        public List<KeyValuePair<TimeSpan, int>> ScheduledTimes { get; set; }
        public string PersonLastName { get; set; }
        public string PersonFirstName { get; set; }
        public string Number { get; set; }
    }
    public class SaveSchedule
    {
        public DateTime? ScheduleDateTime { get; set; }
        public string ScheduleNote { get; set; }
        public string HistoryList { get; set; }
    }
    public class AppealHistory
    {
        public int AltSentRequestAppealId { get; set; }
        public DateTime? Date { get; set; }
        public string Status { get; set; }
        public int PersonnelId { get; set; }
        public string PersonnelLastName { get; set; }
        public string PersonnelNumber { get; set; }
        public string Notes { get; set; }
    }
    public class DeleteRequest
    {
        public int RequestId { get; set; }
        public bool DeleteFlag { get; set; }
        public string HistoryList { get; set; }
        public int DispSentDayXrefId { get; set; }
        public int ArrestId { get; set; }
        public int CrimeId { get; set; }
    }
    public class PersonnelSearchVm
    {
        public bool PersonActive { get; set; }
        public bool CheckPhoto { get; set; }
        public int PersonnelId { get; set; }
        public PersonVm PersonDetail { get; set; }
        public string PhotoFilePath { get; set; }
        public string OfficerFlag { get; set; }
        public bool AgencyGroupFlag { get; set; }
        public bool ArrestTransportOfficerFlag { get; set; }
        public string[] PersonnelSearchText{ get; set; }
        public List<IdentifierVm> PersonPhoto { get; set; }
        public int AgencyId { get; set; }
    }

    public class PrimaryProgram
    {
        public int AltsentProgramId { get; set; }
        public string FacilityAbbr { get; set; }
        public string AltsentProgramabbr { get; set; }
    }

    public class AltSentSites
    {
        public int SiteId { get; set; }
        public string SiteName { get; set; }
        public int ProgramId { get; set; }
    }

    public class ProgramAndSite
    {
        public List<PrimaryProgram> ProgramLst { get; set; }
        public List<AltSentSites> SitesLst { get; set; }
    }

    public class SiteScheduleDetails
    {
        public int SiteId { get; set; }
        public string SiteName { get; set; }
        public int SiteSchdId { get; set; }
        public int? SiteSchdDayOfWeek { get; set; }
        public TimeSpan? SiteSchdTimeFrom { get; set; }
        public TimeSpan? SiteSchdTimeThru { get; set; }
        public int? SiteSchdCapacity { get; set; }
        public string SiteSchdDescription { get; set; }
        public int? ASNCountOld { get; set; }
        public int? ASNCount { get; set; }
    }

    public class SiteAssignedInmates
    {
        public PersonInfo InmateDetail { get; set; }
        public string InmateNumber { get; set; }
        public int InmateId { get; set; }
        public int IncarcerationId { get; set; }
        public int AltsentId { get; set; }
        public bool? Availablesun { get; set; }
        public bool? AvailableMon { get; set; }
        public bool? AvailableTue { get; set; }
        public bool? AvailableWed { get; set; }
        public bool? AvailableThu { get; set; }
        public bool? AvailableFri { get; set; }
        public bool? AvailableSat { get; set; }
        public string AltsentSitename { get; set; }
        public int? AltsentSiteId { get; set; }
        public string FacilityAbbr { get; set; }
        public string ProgramAbbr { get; set; }
        public int FacilityId { get; set; }
        public int AltsentProgramId { get; set; }
        public DateTime? AltsentStart { get; set; }
        public int? AltsentAdts { get; set; }
        public decimal? Totalowed { get; set; }
        public decimal? TotalCollected { get; set; }
        public decimal? TotalBalance { get; set; }
        public int? TotalAttend { get; set; }

    }

    public class PrimarySite
    {
        public int AltsentId { get; set; }
        public bool? Availablesun { get; set; }
        public bool? AvailableMon { get; set; }
        public bool? AvailableTue { get; set; }
        public bool? AvailableWed { get; set; }
        public bool? AvailableThu { get; set; }
        public bool? AvailableFri { get; set; }
        public bool? AvailableSat { get; set; }
        public int? PrimaryAltsentSiteId { get; set; }
        public int? DefaultSunSiteAssignId { get; set; }
        public int? DefaultMonSiteAssignId { get; set; }
        public int? DefaultTueSiteAssignId { get; set; }
        public int? DefaultWedSiteAssignId { get; set; }
        public int? DefaultThuSiteAssignId { get; set; }
        public int? DefaultFriSiteAssignId { get; set; }
        public int? DefaultSatSiteAssignId { get; set; }

    }

    public class AltSentQueue
    {
        public string PersonLastName { get; set; }
        public string PersonFirstName { get; set; }
        public string Number { get; set; }
        public DateTime? DateIn { get; set; }
        public HousingDetail HousingDetail { get; set; }
        public int HousingUnitId { get; set; }
        public int IncarcerationId { get; set; }
        public int InmateId { get; set; }
        public int PersonId { get; set; }
        public string Location { get; set; }
        public string AltSentNotes { get; set; }
        public DateTime? AltSentStartDate { get; set; }
    }
}
