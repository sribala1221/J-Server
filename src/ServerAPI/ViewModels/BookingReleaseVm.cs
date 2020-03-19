using System;
using System.Collections.Generic;

namespace ServerAPI.ViewModels
{
    public class BookingReleaseVm
    {
        public List<KeyValuePair<ReleaseTypeEnum, int>> ReleaseCount { get; set; }
        public List<KeyValuePair<string, int>> TransportCount { get; set; }
        public List<TasksCountVm> TasksCount { get; set; }
    }

    public class BookingNumberList
    {
        public string BookingNumber { get; set; }
        public int InmateId { get; set; }
        public int ArrestId { get; set; }
        public int IncarcerationId { get; set; }
        public bool BookAndReleaseFlag { get; set; }
        public int? WizardProgressId { get; set; }
        public List<WizardStep> LastStep { get; set; }
        public string CaseType { get; set; }
    }

    public class BookingReleaseCount
    {
        public int AllTransports { get; set; }
        public int NoTransportDate { get; set; }
        public List<KeyValuePair<DateTime, int>> TransportScheduleDate { get; set; }
    }

    public class BookingReleaseDetailsVm
    {
        public List<ReleaseVm> ReleaseDetails { get; set; }
        public List<SentenceClearVm> ReleaseClearDetails { get; set; }
        public List<RecordsCheckVm> RecordsCheckResponse { get; set; }
    }

    public class BookingTransportDetailsVm
    {
        public TransportInfo TransportInfo { get; set; }
        public List<TransportNote> TransportNote { get; set; }
        public IncarcerationFormDetails TransportIncarceration { get; set; }
    }

    public class TransportInfo
    {
        public int? TransportFlag { get; set; }
        public int? OtherAgencyId { get; set; }
        public string AgencyName { get; set; }
        public string OtherAgencyName { get; set; }
        public int? CoOperativeFlag { get; set; }
        public int? HoldFlag { get; set; }
        public string TransportRoute { get; set; }
        public string HoldType { get; set; }
        public string HoldName { get; set; }
        public DateTime? ScheduleDate { get; set; }
        public string Instruction { get; set; }
        public string Caution { get; set; }
        public string Bail { get; set; }
        public int? ReturnAgency { get; set; }
    }

    public class TransportNote
    {
        public int NoteId { get; set; }
        public DateTime? NoteDate { get; set; }
        public string NoteType { get; set; }
        public string Note { get; set; }
        public int NoteBy { get; set; }
        public int DeleteFlag { get; set; }
        public PersonnelVm ReleaseOfficer { get; set; }
        public int IncarcerationId { get; set; }
    }

    public class ConditionRelease
    {
        public string Note { get; set; }
        public string ReleaseFlag { get; set; }
        public int? ConReleaseId { get; set; }
        public int? DeleteFlag { get; set; }
    }

    public class TransportManageDetailVm
    {
        public TransportManageVm TransportManage { get; set; }
        public List<ConditionRelease> ConditionRelease { get; set; }
        public List<AgencyVm> Agency { get; set; }
    }

    public class TransportManageVm
    {
        public int IncarcerationId { get; set; }
        public int? IntakeCompleteFlag { get; set; }
        public decimal? BailAmountTotal { get; set; }
        public int? BailNoBailFlagTotal { get; set; }
        public string ChargeLevel { get; set; }
        public string OverallCondition { get; set; }
        public DateTime? FinalReleaseDate { get; set; }
        public DateTime? TransportScheduleDate { get; set; }
        public int? LevelOverrideFlag { get; set; }
        public string HoldName { get; set; }
        public string Instructions { get; set; }
        public string InmateCautions { get; set; }
        public string InmateBail { get; set; }
        public int? InmateReturn { get; set; }
        public int? CoopFlag { get; set; }
        public int? HoldFlag { get; set; }
        public string Route { get; set; }
        public string Type { get; set; }
        public PersonnelVm PersonInName { get; set; }
        public PersonnelVm PersonOutName { get; set; }
        public string AgencyName { get; set; }
        public int? AgencyId { get; set; }
        public string OtherAgencyName { get; set; }
        public int? TransportFlag { get; set; }
        public int? BookAndReleaseFlag { get; set; }
        public DateTime? DateIn { get; set; }
        public DateTime? ReleaseOut { get; set; }
        public DateTime? TransportUpdateDate { get; set; }
        public PersonnelVm TransportUpdateBy { get; set; }
        public int? TransportUpdateById { get; set; }
        public string ReceiveMethod { get; set; }
        public int? CoOpRouteId { get; set; }
        public int? HoldTypeId { get; set; }

    }

    public enum ReleaseTypeEnum
    {
        None,
        OverallReleaseProgress,
        OverallReleaseProgressTransport,
        SentenceClear,
        HoldClear,
        HoldIndefinite,
        RecordsCheckResponses,
        AllTransports,
        NoTransportDate,
        Weekender

    }
}
