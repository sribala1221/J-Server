using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace ServerAPI.ViewModels
{
    public class HeadCountFilter
    {
        public int FacilityId { get; set; }
        public int HeadCountId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int PersonnelId { get; set; }
        public int? SelectTop { get; set; }
    }

    public class HeadCountViewerDetails
    {
        public List<DateTime> HeadCountSchedules { get; set; }
        public List<HeadCountViewDetail> HeadCountViewDetails { get; set; }
        public DateTime? ActiveHeadCount { get; set; }
    }

    public class HeadCountViewDetail
    {
        public int FacilityId { get; set; }
        public int CellLogHeadCountId { get; set; }
        public DateTime? HeadCountLog { get; set; }
        public int? Assigned { get; set; }
        public int? CheckedOut { get; set; }
        public int? Actual { get; set; }
        public int? Count { get; set; }
        public int? Location { get; set; }
        public int? LocationCount { get; set; }
        public string HousingNote { get; set; }
        public string LocationNote { get; set; }
        public int? ClearedBy { get; set; }
        public DateTime? StartedDate { get; set; }
        public PersonnelVm StartedPersonnel { get; set; }
        public DateTime? ClearedDate { get; set; }
        public PersonnelVm ClearedPersonnel { get; set; }
        public DateTime? CellLogDate { get; set; }
        public PersonnelVm OffierName { get; set; }
        public List<HeadCountDetail> HeadCountHousingDetails { get; set; }
    }

    public class HeadCountDetail
    {
        public int CellLogId { get; set; }
        public int? CellLogHeadCountId { get; set; }
        public int? Assigned { get; set; }
        public int? CheckedOut { get; set; }
        public int? Actual { get; set; }
        public int? Count { get; set; }
        public string Note { get; set; }
        public string HousingUnitLocation { get; set; }
        public string HousingUnitNumber { get; set; }
        public int? HousingUnitListId { get; set; }
        public string CellLogLocation { get; set; }
        public int? CellLogLocationId { get; set; }
        public DateTime? CellLogDate { get; set; }
        public DateTime? CellLogSchedule { get; set; }
        public int FacilityId { get; set; }
        public string FacilityAbbr { get; set; }
        public DateTime? CreateDate { get; set; }
        public PersonnelVm CreateOfficerName { get; set; }
        public DateTime? UpdateDate { get; set; }
        public PersonnelVm UpdateOfficerName { get; set; }
        public int ByInmateFlag { get; set; }
    }

    public class CellHeadCountDetails
    {
        public List<KeyValuePair<int, string>> FacilityList { get; set; }
        public List<PrivilegesVm> LocationList { get; set; }
        public HeadCountDetail HeadCountDetail { get; set; }
        public List<CellLogInmateInfo> CellLogInmateDetails { get; set; }
    }

    public class CellHeadCount
    {
        public int CellLogId { get; set; }
        public int? CellLogHeadcountId { get; set; }
        public int FacilityId { get; set; }
        public int? CellLogLocationId { get; set; }
        public string Location { get; set; }
        public int? CellLogCount { get; set; }
        public string Notes { get; set; }
        public DateTime? CellLogDate { get; set; }
        public string CellLogTime { get; set; }
        public string CellLogSaveHistoryList { get; set; }
        public List<int> InmateIdList { get; set; }
    }

    public class CellLogInmateInfo
    {
        public int CellLogInmateId { get; set; }
        public int CellLogId { get; set; }
        public int InmateId { get; set; }
        public int FacilityId { get; set; }
        public string InmateLastName { get; set; }
        public string InmateFirstName { get; set; }
        public string InmateMiddleName { get; set; }
        public int? HousingUnitId { get; set; }
        public int? HousingUnitListId { get; set; }
        public string HousingUnitLocation { get; set; }
        public int? InmateCheckoutLocationId { get; set; }
        public string HousingUnitNumber { get; set; }
        public string HousingUnitBedNumber { get; set; }
        public string PhotoGraphPath { get; set; }
        public string InmateIdCheckoutLocation { get; set; }
        public string Number { get; set; }
        public int ByInmateCountFlag { get; set; }
        public int? ByInmateCountPersonnelId { get; set; }
        public DateTime? ByInmateCountDateTime { get; set; }
        public int PersonId { get; set; }

    }

    public class HeadCountStart
    {
        public int FacilityId { get; set; }
        public string FacilityName { get; set; }
        public DateTime HeadCountShedule { get; set; }
        public bool Unscheduled { get; set; }
        public int Type { get; set; }
    }

    public class CellHousingCount
    {
        public int? FacilityId { get; set; }
        public string FacilityAbbr { get; set; }
        public string HousingUnitLocation { get; set; }
        public string HousingUnitNumber { get; set; }
        public int? HousingUnitListId { get; set; }
        public int? HousingUnitInactive { get; set; }
        public string Location { get; set; }
        public int? InmateCount { get; set; }
        public int? AssignedCount { get; set; }
        public int? CheckOutCount { get; set; }
        public int? PrivilegeId { get; set; }
        public int ByInmateFlagDefault { get; set; }
    }

    public class HeadCountSchedule
    {
        public int InmateId { get; set; }
        public int? InmateCurrentTrackId { get; set; }
        public string InmateCurrentTrack { get; set; }
        public int FacilityId { get; set; }
        public string FacilityAbbr { get; set; }
        public int? HousingUnitId { get; set; }
        public string HousingUnitLocation { get; set; }
        public string HousingUnitNumber { get; set; }
        public int? HousingUnitListId { get; set; }
        public string HousingUnitBedNumber { get; set; }
    }

    public class HeadCountHistoryDetails
    {
        public int FacilityId { get; set; }
        public int? HousingUnitListId { get; set; }
        public int CellLogLocationId { get; set; }
        public int PersonnelId { get; set; }
        public int? LastHundred { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public bool ActualCountOnly { get; set; }
        public bool IsPageInitialize { get; set; }
        public List<KeyValuePair<int, string>> LocationList { get; set; }
        public List<HousingUnitListVm> HousingList { get; set; }
        public List<KeyValuePair<int, string>> FacilityList { get; set; }
        public List<HeadCountHistory> HeadCountHistoryList { get; set; }
        public List<SafetyCheckVm> SafetyCheckHistoryList { get; set; }
    }

    public class HeadCountHistory
    {
        public DateTime? CellLogSchedule { get; set; }
        public string FacilityAbbr { get; set; }
        public string HousingUnitLocation { get; set; }
        public string HousingUnitNumber { get; set; }
        public string CellLogLocation { get; set; }
        public string CellLogComments { get; set; }
        public int? CellLogAssigned { get; set; }
        public int? CellLogCheckOut { get; set; }
        public int? CellLogActual { get; set; }
        public int? CellLogCount { get; set; }
        public KeyValuePair<string, string> Personnel { get; set; }
        public DateTime? CreateDate { get; set; }
        public DateTime? UpdateDate { get; set; }
        public KeyValuePair<string, string> UpdatedBy { get; set; }
        public int CellLogId { get; set; }
    }

    public class HeadCountReport
    {
        public HeaderDetails HeaderDetails { get; set; }
        public HeadCountFilter HeadCountFilter { get; set; }
        public List<HeadCountViewDetail> CellLogHeadCounts { get; set; }
    }

    public class HeadCountNewLocationVm
    {
        public int? CellLogActual { get; set; }
        public List<CellLogInmateInfo> CellLogInmateDetails { get; set; }
    }

    public class HeaderDetails
    {   public string FacilityAbbr { get; set; }
        public PersonnelVm Officer { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ThruDate { get; set; }
        public PersonnelVm SearchOfficer { get; set; }
    }

}
