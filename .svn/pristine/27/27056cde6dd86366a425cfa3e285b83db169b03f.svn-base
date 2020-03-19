using System;
using System.Collections.Generic;

namespace ServerAPI.ViewModels
{
    //Output
    public class FacilityViewerVm
    {
        public ViewerCount GetCount { get; set; }
        public ViewerFilter FilterSetting { get; set; }
        public List<ViewerDetails> ListViewerDetails { get; set; }
        public string Option { get; set; }

        public List<string> HousingUnitLoc { get; set; }
        public List<HousingDropDown> HousingUnitNum { get; set; }
        public List<HousingDropDown> HousingUnitBed { get; set; }
        public List<Privilege> LocationNoteType1 { get; set; }

        public List<KeyValuePair<int, string>> CellType { get; set; }
        public List<KeyValuePair<int, string>> SafetyChkHousing { get; set; }
        public List<KeyValuePair<int, string>> SafetyChkLocation { get; set; }
        public List<KeyValuePair<int, string>> InmateNoteType { get; set; }
        public List<KeyValuePair<int, string>> LocationNoteType2 { get; set; }
        public List<KeyValuePair<int, string>> GeneralNoteType { get; set; }
    }

    public class ViewerDetails
    {
        public int Id { get; set; }
        public string ViewerType { get; set; }
        public DateTime? Date { get; set; }
        public DateTime? UpdateDate { get; set; }

        public PersonDetailVM Inmate { get; set; }

        //narrative
        public HousingDetail HousingUnits { get; set; }

        public string LocTrack { get; set; }
        public string Reason { get; set; }
        public string NoteType { get; set; }
        public string Comment { get; set; }
        public string FloorLocation { get; set; }
        public int LocationId { get; set; }
        public string Location { get; set; }
        public DateTime? SafetyCheckOccured { get; set; }
        public int Count { get; set; }
        public string Narrative { get; set; }
        public string Number { get; set; }
        public string LocDesc { get; set; }
        public int Status { get; set; }

        public string StatusNote { get; set; }
        //narrative

        public bool DeleteFlag { get; set; }
        public PersonnelVm Officer { get; set; }
        public int OfficerId { get; set; }
        public PersonnelVm UpdatedBy { get; set; }
        public int UpdatedById { get; set; }

        public int InmateId { get; set; }
        public int PersonId { get; set; }
        public string InmateNumber { get; set; }
        public int ClearedBy { get; set; }
    }

    //Input
    public class ViewerParameter
    {
        public int Hours { get; set; }
        public bool All { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int FacilityId { get; set; }
        public string HousingUnitLocation { get; set; }
        public int HousingUnitListId { get; set; }
        public int HousingUnitGroupId { get; set; }
        public string HousingUnitBedNumber { get; set; }
        public int LocationId { get; set; }
        public int InmateId { get; set; }
        public int OfficerId { get; set; }
        public string Keyword { get; set; }
        public bool IsOperationViewer { get; set; }
        public bool IsCellViewerInit { get; set; }
        public bool DeleteFlag { get; set; }

        public string CellNoteType { get; set; }
        public int SafetyCkkHousingUnitListId { get; set; }
        public string SafetyCheckLocation { get; set; }
        public string InmateNoteType { get; set; }
        public string LocationNoteType { get; set; }
        public string GeneralNoteType { get; set; }

        public ViewerFilter FilterSetting { get; set; }

        public int AttendanceId { get; set; }

    }

    public class HousingDropDown
    {
        public int HousingUnitListId { get; set; }
        public string HousingUnitLocation { get; set; }
        public string HousingUnitNumber { get; set; }
        public string HousingUnitBedNumber { get; set; }
    }

    public class Privilege
    {
        public int Id { get; set; }
        public string Description { get; set; }
    }

    public class ViewerCount
    {
        public int ClockInCount { get; set; }
        public int ClockOutCount { get; set; }
        public int SetHousingCount { get; set; }
        public int SetLocationCount { get; set; }
        public int SetStatusCount { get; set; }
        public int OfficerLogCount { get; set; }

        public int CellLogCount { get; set; }
        public int HeadCount { get; set; }
        public int SafetyCheckCount { get; set; }
        public int TrackingInCount { get; set; }
        public int TrackingOutCount { get; set; }
        public int HousingInCount { get; set; }
        public int HousingOutCount { get; set; }
        public int InmateNoteCount { get; set; }
        public int LocationNoteCount { get; set; }
        public int GeneralNoteCount { get; set; }
    }

    public class ViewerFilter
    {
        public bool ClockIn { get; set; }
        public bool ClockOut { get; set; }
        public bool SetHousing { get; set; }
        public bool SetLocation { get; set; }
        public bool SetStatus { get; set; }
        public bool OfficerLog { get; set; }

        public bool CellLog { get; set; }
        public bool HeadCount { get; set; }
        public bool SafetyCheck { get; set; }
        public bool HousingIn { get; set; }
        public bool HousingOut { get; set; }
        public bool TrackingIn { get; set; }
        public bool TrackingOut { get; set; }
        public bool InmateNote { get; set; }
        public bool LocationNote { get; set; }
        public bool GeneralNote { get; set; }

    }
}
