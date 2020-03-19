using System;
using System.Collections.Generic;

namespace ServerAPI.ViewModels
{
    public class FoundBy
    {
        public string InmateNumber { get; set; }
        public string LostFoundByOther { get; set; }
        public PersonnelVm PersonVm { get; set; }
        public PersonnelVm InmateVm { get; set; }
    }

    public class FoundIn
    {
        public string FacilityAbbr { get; set; }
        public string HousingUnitLocation { get; set; }
        public string HousingUnitNumber { get; set; }
        public string PrivilegeDescription { get; set; }
        public string LostFoundLocOther { get; set; }
    }

    public class LostFoundBinItems
    {
        public int? InventoryQuantity { set; get; }
        public int InventoryArticles { set; get; }
        public string InventoryDescription { set; get; }
        public string InventoryColor { set; get; }
        public int PersonalInventoryId { get; set; }
        public int DeleteFlag { get; set; }
    }
    public class IdentitySave
    {
        public int PersonalInventoryGroupId { get; set; }
        public int AppliedInmateId { get; set; }
        public List<int> PersonalInventoryId { get; set; }
    }
    public class LostFoundInventory
    {
        public List<InventoryDetails> InventoryGrid { get; set; } //Details main grid 
        public List<LostFoundBin> LostFoundBins { get; set; }//Left top grid
        public List<KeyValuePair<int,int>> PropertyType { get; set; } //Bottom left grid
        public InventoryLookupVm InventoryLookupVm { get; set; }
        public List<HousingDetail> GetHousingDetail { get; set; }
        public List<KeyValuePair<int, string>> Location { get; set; }
        public List<Privilege> LocationDropdown { get; set; }
        public List<string> HousingUnitLoc { get; set; }
        public List<HousingDropDown> HousingUnitNum { get; set; }
    }

    public class LostFoundBin
    {
        public string BinName { get; set; }
        public int ItemsCount { get; set; }
        public int LostFound { get; set; }
    }

    public class SearchOptionsView
    {
        public int GlobalFacilityId { get; set; }
        public int? DispositionCode { get; set; }
        public int FacilityId { get; set; }
        public int FoundInmateId { get; set; }
        public int FoundPersonalId { get; set; }
        public int PrivilegeId { get; set; }
        public int AppliedInmateId { get; set; }
        public int GroupId { get; set; }
        public int BinId { get; set; }
        public int PersonnalInventoryId { get; set; }
        public bool DeleteFlag { get; set; }
        public bool IsIdentify { get; set; }
        public string InGroupId { get; set; }
        public DateTime? FoundFromDate { get; set; }
        public DateTime? FoundToDate { get; set; }
        public string Building { get; set; }
        public string Number { get; set; }
        public string Color { get; set; }
        public string DiscriptionKey1 { get; set; }
        public string DiscriptionKey2 { get; set; }
        public string DiscriptionKey3 { get; set; }
        public int Artical { get; set; }
        public string OtherLocation { get; set; }
        public string Circumstance1 { get; set; }
        public string Circumstance2 { get; set; }
        public DateTime? IdentifiedFromDate { get; set; }
        public DateTime? IdentifiedToDate { get; set; }
        public string InventoryBin { get; set; }
        public string PropertyGroup { get; set; }
        public int PropertyType { get; set; }
        public string GroupNumber { get; set; }
        public string GroupNum { get; set; }
        public bool IsAdvanceOption { get; set; }
        public bool PageIntialize { get; set; }
    }

    public class InventoryLostFoundVm : FoundIn
    {
        public DateTime? LostFountDate { get; set; }
        public string PersonalBinName { get; set; }
        public string GroupNumber { get; set; }
        public string PropertyGroupNotes { get; set; }
        public string LostFoundCircumstance { get; set; }
        public int Count { get; set; }
        public int? LostFoundByPersonnelId { get; set; }
        public int? LostFoundByInmateId { get; set; }
        public string LostFoundByOther { get; set; }
        public int? LostFoundLocFacilityId { get; set; }
        public int? LostFoundLocPrivilegeId { get; set; }
        public FoundBy FoundBy { get; set; }
        public FoundIn FoundIn { get; set; }
        public int? PersonalInventoryBinId { get; set; }
        public int Itemscount { get; set; }
        public int PersonalInventoryGroupId { get; set; }
        public List<LostFoundBinItems> LostFoundBinItems { get; set; }
        public string PhotoFilePath { get; set; }
        public List <InventoryPdfLostFoundVm> InventoryPdfLostFound { get; set; }
        public int PersonalInventoryId { get; set; }
        public int DeleteFlag { get; set; }
        public string InventoryBinNumber { get; set; }        
    }
    public class InventoryPdfLostFoundVm
    {
        public string SummaryHeader { get; set; }
        public string AgencyName { get; set; }
        public DateTime StampDate { get; set; }
        public PersonnelVm Officer { get; set; }
    }

    public class InventoryLostFoundHistory
    {        
        public int DispositionCode { get; set; }
        public int? Qty { get; set; }
        public int? InmateId { get; set; }
        public string Description { get; set; }
        public string Bin { get; set; }
        public int Type { get; set; }
        public string GroupNumber { get; set; }
        public int? Damaged { get; set; }
        public PersonVm InmateDetails { get; set; }
        public PersonnelVm CreatedBy { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string EvidCase { get; set; }
        public PersonnelVm DispodBy { get; set; }
        public DateTime? DispoDate { get; set; }
        public Boolean DeletedFlag { get; set; }
        public int PersonalInventoryId { get; set; }
        public string InventoryDamagedDescription { get; set; }
        public string ReleaseToPerson { get; set; }
        public int? PersonalInventoryBinId { get; set; }
        public string DispositionDetail { get; set; }
    }

    public class HistorySearch
    {
        public int FacilityId { get; set; }
        public int DispositionCode { get; set; }
        public string BinName { get; set; }
        public string PropertyGroup { get; set; }
        public int Type { get; set; }
        public string Color { get; set; }
        public string Keyword1 { get; set; }
        public string Keyword2 { get; set; }
        public string Keyword3 { get; set; }
        public int BelongsInmate { get; set; }
        public Boolean Damaged { get; set; }
        public Boolean Misplaced { get; set; }
        public Boolean FoundBy { get; set; }
        public Boolean Deleted { get; set; }
        public string ReleaseToPerson { get; set; }
        public string EvidCase { get; set; }
        public int CreatedPersonnalId { get; set; }
        public int DisposePersonnalId { get; set; }
        public bool CreatedFlag { get; set; }
        public DateTime CreatedFromDate { get; set; }
        public DateTime CreatedToDate { get; set; }
        public bool DisposeFlag { get; set; }
        public DateTime DisposeFromDate { get; set; }
        public DateTime DisposeToDate { get; set; }
        public int TopRecords { get; set; } = 1000;
    }
}
