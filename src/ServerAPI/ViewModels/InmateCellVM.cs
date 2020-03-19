using System;
using System.Collections.Generic;

namespace ServerAPI.ViewModels
{

    public class CellInmateInputs
    {
        public int TabId { get; set; }
        public int FacilityId { get; set; }
        public int HousingUnitListId { get; set; }
        public int HousingUnitGroupId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ThruDate { get; set; }
    }

    public class InmateCellVm
    {
        public HousingCapacityVm HousingCapacity { get; set; }
        public HousingHeaderVm HousingHeaderDetails { get; set; }        
        public List<HousingVisitationVm> HousingVisitationDetails { get; set; }
        public HousingStatsVm HousingStatsDetails { get; set; }
        public List<InmateSearchVm> InmateDetailsList { get; set; }
        public List<HousingInmateHistory> HousingInmateHistoryList { get; set; }
    }

    public class CellLogVm
    {
        //Drop Down's
        public List<HousingDetail> HousingDetail { get; set; }
        public List<KeyValuePair<int,string>> HousingBedNumberList { get; set; }
        public List<string> NoteList { get; set; }

        public InmateCellLogDetailsVm CellLogDetail { get; set; }
    }

    public class InmateCellLogDetailsVm
    {
        public int FacilityId { get; set; }
        public int HousingUnitListId { get; set; }
        public string HousingNumber { get; set; }
        public string HousingLocation { get; set; }
        public string HousingBedNumber { get; set; }
        public DateTime? LogDate { get; set; }
        public string LogTime { get; set; }
        public int Count { get; set; }
        public string Note { get; set; }
        public string NoteType { get; set; }
        public InputMode Mode { get; set; }
        public int CellLogId { get; set; }
        public PersonnelVm CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public PersonnelVm UpdatedBy { get; set; }
        public DateTime? UpdatedDate { get; set; }
    }

    public enum InputMode
    {
        Add = 1,
        Edit,
        View
    }

}
