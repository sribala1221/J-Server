using System.Collections.Generic;

namespace ServerAPI.ViewModels
{
    public class OperationsVm
    {
    }

    public class ActiveInmates
    {
        public int FacilityId { get; set; }
        public string Facility { get; set; }
        public int? InmateCurrentTrackId { get; set; }
        public string InmateCurrentTrack { get; set; }
        public int? HousingUnitListId { get; set; }
        public string HousingUnitLocation { get; set; }
        public string HousingUnitNumber { get; set; }
    }

    public class CheckedOutLocation
    {
        public int LocationId { get; set; }
        public string Location { get; set; }
        public int CheckedOutCount { get; set; }
    }

    public class FacilityHousing
    {
        public int? HousingUnitListId { get; set; }
        public string HousingUnitLocation { get; set; }
        public string HousingUnitNumber { get; set; }
        public string Facility { get; set; }
        public int InmateCount { get; set; }
        public int CheckedOutCount { get; set; }
    }

    public class HousingMoveParameter
    {
        public int InmateId { get; set; }
        public int? InmateClassificationId { get; set; }
        public int? HousingUnitFromId { get; set; }
        public int HousingUnitToId { get; set; }
        public int FacilityId { get; set; }
        public string MoveReason { get; set; }
        public bool CheckIn { get; set; }
    }
    
    public class BatchHousingVm
    {
        public List<HousingDetail> LstHousing { get; set; }
        public List<CheckedOutLocation> LstCheckedOutLocation { get; set; }
        public List<CheckedOutLocation> LstTransferHousing { get; set; }
        public List<FacilityHousing> LstFacilityHousing { get; set; }
    }
}
