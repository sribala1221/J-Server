using System;
using System.Collections.Generic;

namespace ServerAPI.ViewModels
{
    public class InmateIncidentDropdownList
    {
        public List<LookupVm> LookupIncidentType { get; set; }
        public List<LookupVm> LookupDiscInmateType { get; set; }
        public List<LookupVm> LookupCategorizationType { get; set; }
        public List<KeyValuePair<int, string>> LookupIncidentFlag { get; set; }
        public List<IncidentAltSentSite> IncidentAltSentSiteList { get; set; }
        public List<KeyValuePair<int, string>> IncidentHousingBuildingList { get; set; }
        public List<KeyValuePair<int, string>> IncidentHousingLocationList { get; set; }
        public List<KeyValuePair<int, string>> IncidentOtherLocationList { get; set; }
    }

    public class InmateIncident
    {
        public List<InmateIncidentInfo> InmateIncidentList { get; set; }
    }

    public class InmateIncidentInfo
    {
        public int DispIncidentId { get; set; }
        public string IncidentNumber { get; set; }
        public int? DisciplinaryTypeId { get; set; }
        public string DisciplinaryType { get; set; }
        public DateTime? IncidentDate { get; set; }
        public string DispLocation { get; set; }
        public string DispHousingUnitLocation { get; set; }
        public string DispHousingUnitNumber { get; set; }
        public string DispHousingUnitBedNumber { get; set; }
        public int DispOfficerId { get; set; }
        public DateTime? ReportDate { get; set; }
        public int? InmateTypeId { get; set; }
        public string InmateType { get; set; }
        public int? BypassHearing { get; set; }
        public DateTime? ScheduleHearingDate { get; set; }
        public DateTime? HearingDate { get; set; }
        public DateTime? ReviewDate { get; set; }
        public int? HearingComplete { get; set; }
        public int? DisciplinaryActive { get; set; }
        public string DispSynopsis { get; set; }
        public int FacilityId { get; set; }
        public int? InmateId { get; set; }
        public int? DispLocationId { get; set; }
        public string DispOtherName { get; set; }
        public string DispOtherLocation { get; set; }
        public string DispOtherLocationName { get; set; }
        public int? DispOtherLocationId { get; set; }
        public int? DispAltSentSiteId { get; set; }
        public string DispAltSentSiteName { get; set; }
        public int? DispInmateType { get; set; }
        public string[] DispIncidentFlag { get; set; }
        public string SupervisorAction { get; set; }
        public int DispInmateId { get; set; }
        public int? PersonnelId { get; set; }
        public bool NarrativeFlag { get; set; }
        public bool DispOfficerNarrativeFlag { get; set; }
        public AoWizardProgressVm ActiveIncidentProgress { get; set; }
        public int Count { get; set; }
        public int ExpectedCount { get; set; }
        public int RecordsCount { get; set; }
        public bool SensitiveMaterial { get; set; }
        public bool PreaOnly { get; set; }
        public bool SupervisorReviewFlag { get; set; }
        public string LookupDescription { get; set; }
        public int CategorizationIndex { get; set; }
        public bool AllowHearingFlag { get; set; }
        public string IncidentLocation { get; set; }
        public string DispFreeForm { get; set; }
        public List<cellListVm> CellList { get; set; }
        public bool ContrabandFound { get; set; }
        public string ContrabandNote { get; set; }
        public int SelectedIncidentType { get; set; }
        public string NarrativeFlagNote { get; set; }
        public int LookupFlag { get; set; }

    }

    public class IncidentAltSentSite
    {
        public string AltSentSiteName { get; set; }
        public int? AltSentSiteId { get; set; }
        public int? FacilityId { get; set; }
    }

    public class DisciplinaryPresetVm
    {
        public int PresetId { get; set; }
        public string PresetName { get; set; }
        public string DisciplinaryType { get; set; }
        public int DisciplinaryTypeId { get; set; }
        public bool HousingFlag { get; set; }
        public string DisciplinarySynopsis { get; set; }
        public string Narrative { get; set; }
        public int ViolationId { get; set; }
        public string DisciplinaryInmateType { get; set; }
        public int? DisciplinaryInmateTypeId { get; set; }
        public bool BypassHearing { get; set; }
        public bool AllowHearingFlag { get; set; }
        public KeyValuePair<string, string> Violation { get; set; }
        public DateTime IncidentDate { get; set; }
        public DateTime ReportDate { get; set; }
        public int FacilityId { get; set; }
        public string DispLocation { get; set; }
        public int? DispLocationId { get; set; }
        public string DispHousingUnitLocation { get; set; }
        public string DispHousingUnitNumber { get; set; }
        public string DispHousingUnitBedNumber { get; set; }
        public int[] InmateIds { get; set; }
        public string Categorization { get; set; }
        public int? CategorizationIndex { get; set; }


    }

    public class ReviewComplete
    {
        public int DisciplinaryInmateId { get; set; }
        public int InmateId { get; set; }
        public DateTime CompleteDate { get; set; }
        public DateTime? AppealDueDate { get; set; }
        public int CompleteOfficer { get; set; }
        public string Sanction { get; set; }
        public int DisciplinaryDays { get; set; }
        public List<AppliedBooking> AppliedBookings { get; set; }
        public List<AppliedCharge> AppliedCharges { get; set; }
        public DateTime StopDate { get; set; }
        public string StopNote { get; set; }
        public bool IsStoppage { get; set; }
        public int IncarcerationId { get; set; }
    }

    public class DisciplinaryHearing
    {
        public string ScheduleHearingLoc { get; set; }
        public string FindingNotes { get; set; }
        public string DisciplinaryReviewNotes { get; set; }
        public string DisciplinaryRecommedations { get; set; }
        public PersonnelVm HearingOfficer1 { get; set; }
        public PersonnelVm HearingOfficer2 { get; set; }
        public PersonnelVm ReviewOfficer { get; set; }
        public PersonnelVm ReviewCompleteOfficer { get; set; }
        public DateTime? ScheduleHearingDate { get; set; }
        public DateTime? DisciplinaryIncidentDate { get; set; }
        public DateTime? DisciplinaryHearingDate { get; set; }
        public DateTime? DisciplinaryFindingDate { get; set; }
        public DateTime? DisciplinaryReviewDate { get; set; }
        public DateTime? DisciplinaryReviewCompleteDate { get; set; }
        public string DisciplinarySanction { get; set; }
        public string InmateInterview { get; set; }
        public DateTime? AppealDueDate { get; set; }
        public bool HearingComplete { get; set; }
        public bool InmatePresent { get; set; }
        public int DisciplinaryInmateId { get; set; }
        public int HearingOfficerId1 { get; set; }
        public int HearingOfficerId2 { get; set; }
        public int ReviewOfficerId { get; set; }
        public bool DisciplinaryHearingHold { get; set; }
        public string DisciplinaryHearingHoldReason { get; set; }
    }

    public class IncidentCellGroupVm
    {
        public List<cellListVm> CellList { get; set; }
        public List<cellGroupListVm> CellGroupList { get; set; }

    }
    public class cellGroupListVm
    {
        public int HousingUnitListBedGroupId { get; set; }
        public int HousingUnitListId { get; set; }
        public string BedGroupName { get; set; }
        public bool Selected { get; set; }
    }
    public class cellListVm
    {
        public int HousingUnitListId { get; set; }
        public string HousingUnitBedNumber { get; set; }
        public bool Selected { get; set; }
        public bool ContrabandFound { get; set; }
        public string ContrabandNote { get; set; }
    }
    public class InmateHousingSearchInfo
    {
        public int DispIncidentId { get; set; }
        public int HousingUnitId { get; set; }
        public int LocationId { get; set; }
        public int OtherlocationId { get; set; }
        public bool ContrabandFound { get; set; }
        public string ContrabandNote { get; set; }
        public DateTime SearchDate { get; set; }
        public bool DeleteFlag { get; set; }
    }
    public class LastHousingSearchVm
    {
        public string HousingUnitLocation { get; set; }
        public string HousingUnitNumber { get; set; }
        public string HousingUnitBedNumber { get; set; }
        public string FacilityAbbr { get; set; }
        public string LocationName { get; set; }
        public int LocationId { get; set; }
        public string OtherLocationName { get; set; }
        public int OtherLocationId { get; set; }
        public string IncidentNumber { get; set; }
        public DateTime? LastSearchDate { get; set; }
        public DateTime? CreateDate { get; set; }

        public double Elapsed { get; set; }
        public List<string> Contraband { get; set; }
        public string LocContraband { get; set; }
        public List<string> SearchNote { get; set; }
        public string LocSearchNote { get; set; }

    }
    public class CountHousingSearchVm
    {
        public string HousingUnitLocation { get; set; }
        public string HousingUnitNumber { get; set; }
        public string HousingUnitBedNumber { get; set; }
        public string FacilityAbbr { get; set; }
        public string LocationName { get; set; }
        public int LocationId { get; set; }
        public string OtherLocationName { get; set; }
        public int OtherLocationId { get; set; }
        public List<KeyValuePair<int, string>> IncidentList { get; set; }
        public string DisplinaryNumber { get; set; }
        public int SearchCount { get; set; }
        public string DisplinaryIncidentId { get; set; }

    }

}
