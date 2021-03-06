﻿using System;
using System.Collections.Generic;

namespace ServerAPI.ViewModels
{
    public class MyLogCountDetailsVm
    {
        public List<CellLogDetailsVm> CellLogDetailsLst { get; set; }
        public ViewerCount GetCount { get; set; }
    }

    public class MyLogRequestVm
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int? Hours { get; set; }
        public int? PersonnelId { get; set; }
        public int? DeleteFlag { get; set; }
        public int? FacilityId { get; set; }
        public string Keyword { get; set; }
        public int? MyLog { get; set; }
        public string Location { get; set; }
        public string Number { get; set; }
        public string GroupString { get; set; }
        public string LastLocTrack { get; set; }
        public string Note { get; set; }
        public int? PrivilegeId { get; set; }
        public int HousingUnitListId { get; set; }
        public int AttendanceId { get; set; }
        public bool IsHousing { get; set; }
        public int? StatusId { get; set; }
        public bool IsClear { get; set; }
        public LogSettingDetails IsLogSearch { get; set; }
        public int? AttendanceHistoryid { get; set; }
        public bool IsMyLog { get; set; }
    }

    public class LogSettingDetails
    {
        public bool Iscelllog { get; set; }
        public bool IsClockin { get; set; }
        public bool IsClockOut { get; set; }
        public bool IsSetHousing { get; set; }
        public bool IsSetLocation { get; set; }
        public bool IsSetStatus { get; set; }
        public bool IsLog { get; set; }
        public bool IsHeadCount { get; set; }
        public bool IsSafetyCheck { get; set; }
        public bool IsHousingIn { get; set; }
        public bool IsHousingOut { get; set; }
        public bool IsTrackingOut { get; set; }
        public bool IsTrackingIn { get; set; }
        public bool IsNote { get; set; }
        public bool IsLocationNote { get; set; }
        public bool IsGeneralNote { get; set; }
    }

    public class CellLogDetailsVm
    {
        public int CellLogId { get; set; }
        public string Type { get; set; }
        public DateTime? UpdateDate { get; set; }
        public DateTime? CellDate { get; set; }
        public string FloorNoteLocation { get; set; }
        public string Location { get; set; }
        public string HousingUnitLocation { get; set; }
        public string HousingUnitNumber { get; set; }
        public string NoteType { get; set; }
        public string Comments { get; set; }
        public int? DeleteFlag { get; set; }
        public int InmateId { get; set; }
        public string InmateNumber { get; set; }
        public int? ClearedBy { get; set; }
        public string HeadCountType { get; set; }
        public int? CellLogCount { get; set; }
        public string CellLogLocation { get; set; }
        public string HousingUnitBedNumber { get; set; }
        public string HousingUnitBedLocation { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string TrakLocation { get; set; }
        public Boolean InmateRefused { get; set; }
        public string RefusedReason { get; set; }
        public string LastHousingNote { get; set; }
        public string LastHousingLocation { get; set; }
        public string LastHousingNumber { get; set; }
        public string LastLocDesc { get; set; }
        public string LastLocTrack { get; set; }
        public string LookupDescription { get; set; }
        public string AttendanceStatusNote { get; set; }
        public int? AttendanceStatus { get; set; }

        public int PersonId { get; set; }
    }

    public class MylogDetailsVm
    {
        public List<HousingDetail> HousingLst { get; set; }
        public List<KeyValuePair<int, string>> LocationLst { get; set; }
        public List<CellLogDetailsVm> LstCelldetails { get; set; }
        public DateTime? ClockInEnter { get; set; }
        public DateTime? ClockOutEnter { get; set; }
        public string Note { get; set; }
        public string LastLocTrack { get; set; }
        public string LastHousingLocation { get; set; }
        public string LastHousingNumber { get; set; }
        public string LastHousingNote { get; set; }
        public string LastLocDesc { get; set; }
        public int? Status { get; set; }
        public string StatusNote { get; set; }
        public List<string> LstOfficerLog { get; set; }
    }

    public class InmateTrakObj
    {
        public DateTime? InmateTrakDateOut { get; set; }
        public DateTime? InmateTrakDateIn { get; set; }
        public int? InPersonnelId { get; set; }
        public int? OutPersonnelId { get; set; }
        public int? FacilityId { get; set; }

        public string InmateTrakLocation { get; set; }
        public string InmateCurrentTrack { get; set; }
        public bool InmateRefused { get; set; }
        public string InmateRefusedReason { get; set; }
        public string InmateTrakNote { get; set; }

        public int InmateId { get; set; }
        public int PersonId { get; set; }
        public int InmateTrakId { get; set; }

        public string InmateNumber { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }

        //tk.InmateTrakDateOut,
        //tk.InmateTrakDateIn,
        //tk.InPersonnelId,
        //tk.OutPersonnelId,
        //tk.Inmate.FacilityId,
        //tk.InmateTrakLocation,
        //tk.Inmate.InmateCurrentTrack,
        //tk.InmateRefused,
        //tk.InmateRefusedReason,
        //tk.InmateTrakNote,
        //tk.InmateId,
        //tk.InmateTrakId
    }
}
