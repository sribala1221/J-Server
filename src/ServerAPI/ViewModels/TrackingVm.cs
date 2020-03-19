﻿using System;
using System.Collections.Generic;

namespace ServerAPI.ViewModels
{
    public class TrackingVm
    {
        public int MoveLocationId { get; set; }
        public List<int> MoveInmateList { get; set; }
        public int MoveDestinationId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public bool IsVisit { get; set; }
        public int ScheduleId { get; set; }
    }

    public class TrackingConflictVm
    {
        public int InmateId { get; set; }
        public string InmateNumber { get; set; }
        public int PersonId { get; set; }
        public int VisitorId { get; set; }
        public string VisitorFirstName { get; set; }
        public string VisitorLastName { get; set; }
        public string VisitorMiddleName { get; set; }
        public string ConflictType { get; set; }
        public string ConflictDescription { get; set; }
        public string Note { get; set; }
        public string Reason { get; set; }
        public string Relationship { get; set; }
        public string Location { get; set; }
        public DateTime? ConflictDate { get; set; }
        public int InmateCount { get; set; }
        public int? Capacity { get; set; }
        public int? OutofService { get; set; }
        public int LocationId { get; set; }
        public string Description { get; set; }
        public int? PersonSexLast { get; set; }
        public string InmateCurrentTrack { get; set; }
        public int? InmateCurrentTrackId { get; set; }
        public int FacilityId { get; set; }
        public string LookupDescription { get; set; }
        public double? LookupIndex { get; set; }
        public string PersonClassificationType { get; set; }
        public string PersonClassificationSubset { get; set; }
        public int KeepSepPersonId { get; set; }
        public int KeepSepInmateId { get; set; }
        public string KeepSepInmateNumber { get; set; }
        public string KeepSepAssoc1 { get; set; }
        public string KeepSepAssoc2 { get; set; }
        public string KeepSepReason { get; set; }
        public string KeepSepType { get; set; }
        public string KeepSepAssocSubset2 { get; set; }
        public string KeepSepAssocSubset1 { get; set; }
        public string KeepSepAssoc2Subset { get; set; }
        public string KeepSepAssoc1Subset { get; set; }
        public string PersonFirstName { get; set; }
        public string PersonLastName { get; set; }
        public string PersonMiddleName { get; set; }
        public string KeepSepPersonLastName { get; set; }
        public string KeepSepPersonFirstName { get; set; }
        public string KeepSepPersonMiddleName { get; set; }
        public string FacilityAbbr { get; set; }
        public HousingDetail HousingInfo { get; set; }
        public LockDownType LockDown { get; set; }
        public DateTime StartLockdown { get; set; }
        public DateTime EndLockdown { get; set; }
        public DateTime? FromDate { get; set; }
        public List<string> LocationDetail { get; set; }
        public int? KeepSepAssoc1Id { get; set; }
        public int? KeepSepAssoc1SubsetId { get; set; }
        public int? KeepSepAssoc2Id { get; set; }
        public int? KeepSepAssoc2SubsetId { get; set; }
        public int? KeepSepAssocSubset1Id { get; set; }
        public int? KeepSepAssocSubset2Id { get; set; }
        public int? PersonClassificationTypeId { get; set; }
        public int? PersonClassificationSubsetId { get; set; }
        public string InmateHousingRuleClassifyInfo { get; set; }
        public string InmateHousingRuleFlagInfo { get; set; }
    }

    public class WorkCrewDetailsVm
    {
        public List<PrivilegeDetailsVm> LstPrivileges { get; set; }
        public List<WorkCrewFurlVm> LstWorkCrewFurl { get; set; }
    }

    public class WorkCrewFurlVm
    {
        public int WorkCrewId { get; set; }
        public string WorkCrewName { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string Comment { get; set; }
        public string LockerName { get; set; }
        public int? WorkCrewLockerId { get; set; }
        public string InmateNumber { get; set; }
        public int DeleteFlag { get; set; }
        public int InmateId { get; set; }
        public int InmateTrackId { get; set; }
        public InmateTrakVm InmateTrak { get; set; }
        public WorkCrewIdentifier WorkCrewIdentifier { get; set; }
        public int WorkCrewLookupId { get; set; }
        public int? Capacity { get; set; }
        public WorkFurlRequestVm WorkCrewRequest { get; set; }
        public HousingDetail HousingDetail { get; set; }
        public PersonVm Person { get; set; }
        public int FacilityId { get; set; }
        public TimeSpan? TodayStart { get; set; }
        public TimeSpan? TodayEnd { get; set; }
        public string Photofilepath { get; set; }
        public bool SchdAnytime { get; set; }
        public string ContactName { get; set; }
        public string ContactNumber { get; set; }
        public string ContactInfo { get; set; }
        public List<PersonAddressVm> PersonAddress { get; set; }
        public TimeSpan? SunStart { get; set; }
        public TimeSpan? SunEnd { get; set; }
        public TimeSpan? MonStart { get; set; }
        public TimeSpan? MonEnd { get; set; }
        public TimeSpan? TueStart { get; set; }
        public TimeSpan? TueEnd { get; set; }
        public TimeSpan? WedStart { get; set; }
        public TimeSpan? WedEnd { get; set; }
        public TimeSpan? ThuStart { get; set; }
        public TimeSpan? ThuEnd { get; set; }
        public TimeSpan? FriStart { get; set; }
        public TimeSpan? FriEnd { get; set; }
        public TimeSpan? SatStart { get; set; }
        public TimeSpan? SatEnd { get; set; }
        public TrackingHistory TrackingHistory { get; set; }
        public string PersonOccupation { get; set; }
        public string PersonEmployer { get; set; }
        public int PersonId { get; set; }
    }

    public class WorkCrewFurlHistoryVm
    {
        public string WorkCrew { get; set; }
        public string StartDate { get; set; }
        public string StartTime { get; set; }
        public string EndDate { get; set; }
        public string EndTime { get; set; }
        public string Comment { get; set; }
        public string LockerName { get; set; }
        public string ContactName { get; set; }
        public string ContactNumber { get; set; }
        public string ContactInfo { get; set; }
        public string Address { get; set; }
        public string Sun { get; set; }
        public string Mon { get; set; }
        public string Tue { get; set; }
        public string Wed { get; set; }
        public string Thu { get; set; }
        public string Fri { get; set; }
        public string Sat { get; set; }
        public string TrackLocation { get; set; }
        public string Employer { get; set; }
        public string Occupation { get; set; }
        public string BusPhone { get; set; }
        public string CellPhone { get; set; }
        public string HomePhone { get; set; }
    }

    public class InmateTrakVm
    {
        public string InmateCurrentTrack { get; set; }
        public string InmateTrackNote { get; set; }
        public int InmateId { get; set; }
        public int? InmateTrackId { get; set; }
        public DateTime? InmateTrackDateIn { get; set; }
        public int? InmateCurrentTrackId { get; set; }
        public string InmateRefusedNote { get; set; }
        public string InmateRefusedReason { get; set; }
        public bool InmateRefused { get; set; }
        public DateTime? DateIn { get; set; }
        public DateTime? DateOut { get; set; }
    }

    public class WorkCrewIdentifier
    {
        public bool? PhotoGraphPathAbsolute { get; set; }
        public string PhotoGraphPath { get; set; }
        public string PhotoGraphRelativePath { get; set; }
        public int IdentifierId { get; set; }
        public int PersonId { get; set; }
    }

    public class WorkCrewVm
    {
        public int WorkCrewCount { get; set; }
        public int WorkCrewCapacity { get; set; }
        public string WorkCrewName { get; set; }
        public int WorkCrewLookupId { get; set; }
        public int RequestCount { get; set; }
        public string StartDateHours { get; set; }
        public string EndDateHours { get; set; }
        public string ContactName { get; set; }
        public string ContactNumber { get; set; }
        public string ContactInfo { get; set; }
        public List<PersonAddressVm> PersonAddress { get; set; }
        public int HousingUnitListId { get; set; }
        public int InmateId { get; set; }
        public bool SchdAnytime { get; set; }
    }

    public class WorkFurlRequestVm
    {
        public int WorkCrewRequestId { get; set; }
        public int? DeleteFlag { get; set; }
    }

    public class WorkCrewFurloughCountVm
    {
        public string WorkCrewName { get; set; }
        public int WorkCrewLookupId { get; set; }
        public int WorkCrewFurloughCount { get; set; }
        public TimeSpan? TodayStart { get; set; }
        public TimeSpan? TodayEnd { get; set; }
        public int WorkCrewRequestCount { get; set; }
        public int? CrewCapacity { get; set; }
        public int DeleteFlag { get; set; }
    }

    public class ProgramVm
    {
        public int ProgramId { get; set; }
        public string ProgramCategory { get; set; }
        public int? ClassOrServiceNumber { get; set; }
        public string ClassOrServiceDescription { get; set; }
        public string ClassOrServiceName { get; set; }
    }

    public class WorkCrewTrackXrefVm
    {
        public int WorkCrewId { get; set; }
        public int WorkCrewTrackId { get; set; }
    }

    public class TrackingHistory
    {
        public int InmateId { get; set; }
        public int WorkCrewId { get; set; }
        public string HistoryList { get; set; }
        public string CreateByLastName { get; set; }
        public string CreateByFirstName { get; set; }
        public string CreateByOfficerBadgeNumber { get; set; }
        public string UpdateByLastName { get; set; }
        public string UpdateByFirstName { get; set; }
        public string UpdateByOfficerBadgeNumber { get; set; }
        public InmateTrakVm InmateTrakDetails { get; set; }
    }

    public class ApptTracking
    {
        public int ScheduleId { get; set; }
        public string ApptStartDate { get; set; }
        public string ApptEndDate { get; set; }
        public string ApptLocation { get; set; }
        public int? ApptLocationId { get; set; }
        public int ReoccurFlag { get; set; }
        public string AgencyName { get; set; }
        public int? AgencyId { get; set; }
        public int? InmateTrackId { get; set; }
        public int? Count { get; set; }
        public bool? RefusalFlag { get; set; }
        public bool DeleteFlag { get; set; }
        public int[] LstAppointmentId { get; set; }
        public bool ApptAlertEndDate { get; set; }
        public DateTime? AppointmentEndDate { get; set; }
        public DateTime AppointmentStartDate { get; set; }
        public int? DeptId { get; set; }
        public bool EndDateFlag { get; set; }
        public bool DisplayFlag { get; set; }
        public bool VisitFlag { get; set; }

        public int? InmateCurrentTrackFacilityId { get; set; }

        // below properties are used in Facility -> Tracking -> Schedule screen
        public string HousingUnitLocation { get; set; }
        public string HousingUnitNumber { get; set; }
        public PrivilegeDetailsVm Privileges { get; set; }
        public bool EnrouteFlag { get; set; }
        public int? HousingUnitListId { get; set; }
        public int? HousingGroupId { get; set; }
    }

    public class ApptInmateDetails : ApptTracking
    {
        public int? HousingUnitId { get; set; }

        //below property for schedule
        public int? HistoryScheduleId { get; set; }
        public PersonTracking InmateDetail { get; set; }
        public DateTime? DateOut { get; set; }
        public int? EnrouteLocationId { get; set; }
        public string Destination { get; set; }
        public string ImageName { get; set; }
        public string InmateTrackNote { get; set; }
    }

    public class ApptParameter
    {
        public int FacilityId { get; set; }
        public int? LocationId { get; set; }
        public List<int> HousingUnitListId { get; set; }
        public int? CourtId { get; set; }
        public int? ApptTypeId { get; set; }
        public bool ShowDeleted { get; set; }
        public bool IsFromTracking { get; set; }
        public int? HousingNumber { get; set; }
        public int? HousingGroup { get; set; }
    }

    public class ApptTrackingVm
    {
        public List<PrivilegeDetailsVm> LocationList { get; set; }
        public List<KeyValuePair<int, string>> CourtList { get; set; }
        public List<KeyValuePair<int, string>> TypeLookupList { get; set; }
        public List<ApptTracking> CntLocApptTracking { get; set; }
        public List<ApptTracking> CntCourtApptTracking { get; set; }
        public List<LookupVm> RefusalReason { get; set; }
        public List<ApptInmateDetails> LocApptTracking { get; set; }
        public List<ApptInmateDetails> CourtApptTracking { get; set; }
    }

    public class WorkCrewFurloughTracking
    {
        public List<WorkCrewFurlVm> LstFurloughDetails { get; set; }
        public List<WorkCrewFurloughCountVm> LstFurloughCountDetails { get; set; }
        public List<PrivilegeDetailsVm> LstPrivileges { get; set; }
    }

    public class TrackingSchedule
    {
        public List<KeyValuePair<int, string>> AllLocations { get; set; }
        public int FacilityId { get; set; }
        public string Event { get; set; }
        public string Location { get; set; }
        public string Time { get; set; }
        public string Housing { get; set; }
        public List<ApptTracking> CntLocApptTracking { get; set; }
        public List<ApptTracking> CntCourtApptTracking { get; set; }
        public List<ApptInmateDetails> LocApptTracking { get; set; }
        public List<ApptInmateDetails> CourtApptTracking { get; set; }
        public List<KeyValuePair<int, string>> RefusalReasonList { get; set; }
        public List<PrivilegeDetailsVm> LstPrivileges { get; set; }
        public int? HousingUnitListId { get; set; }
        public int? HousingGroupId { get; set; }
        public List<HousingDetail> HousingNumberList { get; set; }
        public bool IsFromTransfer { get; set; }
    }

    public class PersonTracking : PersonVm
    {
        public InmateTrakVm InmateTrack { get; set; }
        public List<LookupVm> RefusalReason { get; set; }
        public int ProgramDeleteFlag { get; set; }
    }
}