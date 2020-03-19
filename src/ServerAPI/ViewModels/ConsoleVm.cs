using System;
using System.Collections.Generic;
using System.Linq;

namespace ServerAPI.ViewModels
{
    public class ConsoleInputVm
    {
        public int FacilityId { get; set; }
        public int HousingUnitListId { get; set; }
        public int HousingUnitGroupId { get; set; }
        public List<int> ListConsoleHousingId { get; set; }
        public List<int> ListConsoleLocationId { get; set; }
        public List<int> ListConsoleMyLocationId { get; set; }
        public bool IsRefresh { get; set; }
        public ApptParameter ApptParameter { get; set; }
        public bool OperationFlag { get; set; }

    }

    public class ConsoleVm
    {
        public List<ConsoleLocationVm> MyLocationList { get; set; }
        public HousingLocationVm HousingLocation { get; set; }
        public List<SafetyCheckVm> SafetyCheckHousingList { get; set; }
        public List<SafetyCheckVm> SafetyCheckLocationList { get; set; }
        public List<HeadCountVm> HeadCountHousingList { get; set; }
        public List<HeadCountVm> HeadCountLocationList { get; set; }
        public List<ConsoleHousingVm> HousingList { get; set; }
        public List<ConsoleLocationVm> LocationList { get; set; }
        public ObservationLogsVm ObservationLogList { get; set; }
        public List<ReleaseQueueVm> ReleaseQueueList { get; set; }
        public List<RequestQueueVm> RequestQueueList { get; set; }
        public List<object> ConsoleEventList { get; set; }
        public List<WorkCrewVm> WorkCrewList { get; set; }
        public List<WorkCrewFurloughCountVm> WorkCrewFurloughList { get; set; }
        public IEnumerable<object> ProgramConsoleList { get; set; }
        public ApptTrackingVm ApptConsoleList { get; set; }
        public List<RegisterDetails> VisitorInprogressList { get; set; }
        public List<VisitDetails> VisitDetails { get; set; }
        public int FormCount { get; set; }
        public List<KeyValuePair<int, string>> TransferInOutList { get; set; }
    }

    public class SafetyCheckVm : HousingDetail
    {
        public DateTime? LastEntry { get; set; }
        public int? IntervalMinutes { get; set; }
        public string Mode { get; set; }
        public int SafetyCheckId { get; set; }
        public bool SafetyCheckLateEntryFlag { get; set; }
        public string SafetyCheckNote { get; set; }
        public int? SafetyCheckCount { get; set; }
        public int? SafetyNoCheckCount { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? SafetyCheckOccured { get; set; }
        public int? LateEntryMaxMin { get; set; }
        public int? LateEntryNoteRequired { get; set; }
        public int? DeleteFlag { get; set; }
        public int? InmateInActive { get; set; }
        public List<string> BedNumberList { get; set; }
        public int PrivilegesId { get; set; }
        public string PrivilegesDescription { get; set; }
        public int? SafetyCheckLateEntryMax { get; set; }
        public int? SafetyCheckIntervalMinutes { get; set; }
        public int? SafetyCheckFlag { get; set; }
        public int? SafetyCheckBatchNotAllowed { get; set; }
        public PersonnelVm PersonNameDetails { get; set; }
        public int InmateCount { get; set; }
        public List<SafetyCheckBedVm> BedListItems { get; set; }
        public List<SafetyCheckVm> SafetyCheckItems { get; set; }
        public DateTime CheckDate { get; set; }
        public DateTime CheckTime { get; set; }
        public bool UnCheckBedNoteRequired { get; set; }
        public bool HideSafetyHousingFlag { get; set; }
        public bool HideSafetyLocationFlag { get; set; }
        public DateTime CurrentDate { get; set; }
        public DateTime TotalCurrentDate { get; set; }
        public int? RequiredEndTime { get; set; }
        public DateTime? EndDateTime { get; set; }
        public int? PrivilegeFacilityId { get; set; }
        public string PrivilegeFacilityAbbr { get; set; }
        public string PrivilegeFacilityName { get; set; }
        public int? HousingUnitScheduleSafteyCheckId { get; set; }

    }

    public class SafetyCheckBedVm
    {
        public bool BedChecked { get; set; }
        public string BedRequiredNotes { get; set; }
        public string HousingBedNumber { get; set; }
        public string HousingBedLocation{ get; set; }
    }

    public class BatchSafetyCheckVm
    {
        public List<SafetyCheckVm> SafetyCheckHousing { get; set; }
        public List<SafetyCheckVm> SafetyCheckLocation { get; set; }
    }

    public class HeadCountVm
    {
        public int? PrivilegesId { get; set; }
        public string PrivilegesDescription { get; set; }
        public int? CellHeadCountId { get; set; }
        public int CellLogId { get; set; }
        public DateTime? CellLogDate { get; set; }
        public int? Count { get; set; }
        public DateTime? ClearedDate { get; set; }
        public DateTime? UpdateDate { get; set; }
        public DateTime? StartDate { get; set; }
        public int SafetyMins { get; set; }
        public int? HousingInactive { get; set; }
        public int? InmateCurrentTrackId { get; set; }
        public string InmateCurrentTrack { get; set; }
        public int HousingUnitGroupId { get; set; }
        public DateTime? CellLogSchdule { get; set; }
        public HousingDetail HousingDetail { get; set; }
        public int? HousingUnitListId { get; set; }
        public string HousingLocation { get; set; }
        public string HousingNumber { get; set; }
        public int FacilityId { get; set; }
        public string FacilityAbbr { get; set; }
        public int Assigned { get; set; }
        public int Out { get; set; }
        public int Actual { get; set; }

        public int? UpdatedBy { get; set; }
    }

    public class ConsoleHousingVm
    {
        public int HousingUnitListId { get; set; }
        public string HousingUnitLocation { get; set; }
        public string HousingUnitNumber { get; set; }
        public int? AllowBatchSafetyCheck { get; set; }
        public bool Selected { get; set; }
    }

    public class ConsoleLocationVm
    {
        public int PrivilegeId { get; set; }
        public string PrivilegeDescription { get; set; }
        public bool Selected { get; set; }
    }

    public class HousingLocationVm
    {
        public List<ConsoleHousingVm> HousingList { get; set; }
        public List<ConsoleLocationVm> HousingLocationList { get; set; }
    }

    public class ObservationLogDetails
    {
        public int? ObservationScheduleId { get; set; }
        public int? ObservationTypeId { get; set; }
        public string ObservationType { get; set; }
        public DateTime? ObservationStartDate { get; set; }
        public DateTime? ObservationEndDate { get; set; }
        public string ObservationNotes { get; set; }
        public string InmateCurrentTrack { get; set; }
        public int InmateId { get; set; }
        public int? HousingUnitId { get; set; }
        public PersonInfoVm PersonDetail { get; set; }
        public HousingDetail HousingDetail { get; set; }
        public List<KeyValuePair<int, string>> LookupDetails { get; set; }        
    }

    public class ObservationLogItemDetails : ObservationLogDetails
    {
        public int? ObservationScheduleActionId { get; set; }
        public string ObservationActionName { get; set; }
        public string ObservationAct { get; set; }
        public int? ObservationActionId { get; set; }
        public int? ObservationLogId { get; set; }
        public DateTime? ObservationDate { get; set; }
        public bool ObservationLateEntryFlag { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string ObservationScheduleNote { get; set; }
        public string ObservationActionNote { get; set; }
        public int? ObservationScheduleInterval { get; set; }
        public DateTime? LastReminderEntry { get; set; }
        public int? LastReminderEntryBy { get; set; }
        public int? ObservationLateEntryMax { get; set; }
        public PersonnelVm PersonDetails { get; set; }
    }

    public class ObservationLogsVm
    {
        public IQueryable<ObservationLogDetails> ObservationLogDetails { get; set; }
        public List<ObservationLogItemDetails> ObservationLogItemDetails { get; set; }
    }

    public class ReleaseQueueVm
    {
        public string InmateNumber { get; set; }
        public string ReleaseConsoleType { get; set; }
        public int? ReleaseConsoleCount { get; set; }
        public HousingDetail HousingDetail { get; set; }
        public PersonnelVm PersonDetail { get; set; }
        public string PersonPhoto { get; set; }
        public string InmateCurrentTrak { get; set; }
        public int InmateId { get; set; }
        public DateTime? OverallFinalReleaseDate { get; set; }
        public int? ReleaseClearFlag { get; set; }
        public int? TransportFlag { get; set; }
        public DateTime? ReleaseClearDate { get; set; }
        public DateTime? TransportScheduleDate { get; set; }
    }

    public class RequestQueueVm
    {
        public int RequestId { get; set; }
        public int? ClearedBy { get; set; }
        public int? PendingBy { get; set; }
        public int? ShowInFlag { get; set; }
        public int? InactiveFlag { get; set; }
        public string RequestConsoleType { get; set; }
        public int? RequestConsoleCount { get; set; }
        public List<int> ListRequestHousingId { get; set; }
        public int? ResponseInmateFlag { get; set; }
        public int? ResponseInmateReadFlag { get; set; }
        public int? PendingAllFacilityFlag { get; set; }
        public int FacilityId { get; set; }
    }

    public class ObservationScheduleActionVm
    {
        public int ObservationScheduleActionId { get; set; }
        public int ObservationActionId { get; set; }
        public int? ObservationScheduleInterval { get; set; }
        public int? ObservationLateEntryMax { get; set; }
        public string ObservationScheduleNote { get; set; }
        public List<KeyValuePair<int, string>> ActionList { get; set; }
    }

    public class IncarcerationFormListVm
    {
        public List<FormTemplateCount> FormTemplateCountList { get; set; }
        public List<IncarcerationForms> IncarcerationForms { get; set; }
        public List<IncarcerationForms> BookingForms { get; set; }
    }
}

