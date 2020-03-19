using System;
using System.Collections.Generic;
namespace ServerAPI.ViewModels
{
    public class LockdownVm : HousingDetail
    {
        public int HousingLockdownId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Reason { get; set; }
        public string Region { get; set; }
        public string Note { get; set; }
        public int SourceId { get; set; }
        public string Duration { get; set; }
        public bool DeleteFlag { get; set; }
        public string GroupName { get; set; }
        public DateTime? CreateDate { get; set; }
        public PersonnelVm CreatedPersonnel { get; set; }
        public DateTime? UpdateDate { get; set; }
        public PersonnelVm UpdatedPersonnel { get; set; }
        public DateTime? DeleteDate { get; set; }
        public PersonnelVm DeletedPersonnel { get; set; }
    }

    public class LockdownDetailsVm
    {
        public LockdownVm LockdownInfo { get; set; }
        public List<FacilityVm> FacilityList { get; set; }
        public List<HousingDetail> BuildingList { get; set; }
        public List<HousingGroupVm> GroupList { get; set; }
        public List<HousingDetail> PodList { get; set; }
        public List<HousingDetail> CellGroupList { get; set; }
        public List<HousingDetail> CellList { get; set; }
    }

    public enum LockDownType
    {
        Facility,
        Building,
        Pod,
        Cell,
        CellGroup,
        HousingGroup
    }
}
