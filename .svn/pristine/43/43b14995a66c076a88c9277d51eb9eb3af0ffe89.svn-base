using GenerateTables.Models;
using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace ServerAPI.ViewModels
{
    public class InventoryDetails: LostFoundAddVm
    {
        public string GroupNumber { get; set; }
        public int PersonalInventoryId { get; set; }
        public int? PersonalInventoryBinId { get; set; }
        public int PersonalInventoryGroupId { get; set; }
        public string PersonalBinName { set; get; }
        public string PersonalGroupName { set; get; }
        public int? InventoryDispositionCode { set; get; }
        public int Count { get; set; }
        public int InventoryItemCount { get; set; }
        public string PropertyGroupNotes { get; set; }
        public int? InmateId { set; get; }
        public int DeleteFlag { set; get; }
        public string DeleteReason { set; get; }
        public string DeleteReasonNote { set; get; }
        public DateTime? InventoryReturnDate { get; set; }
        public string PersonName { set; get; }
        public string PersonAddress { set; get; }
        public string CityStateZip { set; get; }
        public string PersonIdType { set; get; }
        public string InventoryEvidenceCaseNumber { set; get; }
        public int? InventoryEvidencePersonnelId { set; get; }
        public int? InventoryEvidenceAgencyId { set; get; }
        public string DispoNotes { set; get; }
        public DateTime? UpdateDate { get; set; }
        public bool UpdateFlag { get; set; }
        public List<InventoryItemDetails> InventoryItemDetails { get; set; }
        public string PhotoWithAllItem { get; set; }
        public string FilePath { get; set; }
        public FoundBy FoundBy { get; set; }
        public FoundIn FoundIn { get; set; }
        public DateTime? LostFountDate { get; set; }
        public int Itemscount { get; set; }
        public bool InventoryDamageFlag { get; set; }      
        public string InventoryDamageDescription { get; set; }
        public bool InventoryMisplacedFlag {get; set;}
        public string InventoryMisplacedNote {get;set;}
        public int? ReceivingFlag {get;set;}
        public int? FacilityTransferFlag {get;set;}
    }

    public class InventoryItemDetails : InventoryDetails
    {
        public int? InventoryQuantity { set; get; }
        public int InventoryArticles { set; get; }
        public string InventoryDescription { set; get; }
        public string InventoryColor { set; get; }
        public double? InventoryValue { set; get; }
        public string PersonalInventoryGroupNumber { set; get; }
        public List<AgencyVm> AgencyDetails { get; set; }
        public List<PersonnelVm> PersonalDetails { get; set; }  
        public DateTime? CreatedDate { get; set; }
        public List<IdentifierVm> ListIdentifiers { get; set; }
        public string InventoryArticlesName { set; get; }
        //public new int PersonalInventoryId { get; set; }
    }
    public class LostFoundAddVm
    {
        public string LostFoundCircumstance { get; set; }
        public DateTime? LostFoundDate { get; set; }
        public int? LostFoundByPersonnelId { get; set; }
        public int? LostFoundByInmateId { get; set; }
        public string LostFoundLocHousingUnitNumber { get; set; }
        public string LostFoundLocHousingUnitLocation { get; set; }
        public int? LostFoundLocFacilityId { get; set; }
        public int? LostFoundLocPrivilegeId { get; set; }
        public string LostFoundByOther { get; set; }
        public string LostFoundLocOther { get; set; }
    }
    public class InventoryHistoryVm : PersonalInventoryHistory
    {
        public string LookupDescription { get; set; }
        public string PersonalInventoryBinNumber { set; get; }
        public string PersonalInventoryGroupNumber { set; get; }
        public DateTime? PersonalInventoryUpdateDate { get; set; }
        public string FacilityAbbr { get; set; }
        public string CreatedPersonFirstName { get; set; }
        public string CreatedPersonLastName { get; set; }
        public string CreatedPersonMiddleName { get; set; }
        public string UpdatedPersonFirstName { get; set; }
        public string UpdatedPersonLastName { get; set; }
        public string UpdatedPersonMiddleName { get; set; }
        public string DeletedPersonFirstName { get; set; }
        public string DeletedPersonLastName { get; set; }
        public string DeletedPersonMiddleName { get; set; }
        public string InventoryMailPersonFirstName { get; set; }
        public string InventoryMailPersonLastName { get; set; }
        public string InventoryMailPersonMiddleName { get; set; }
        public string InventoryMailAddressPersonFirstName { get; set; }
        public string InventoryMailAddressPersonLastName { get; set; }
        public string InventoryMailAddressPersonMiddleName { get; set; }
        public string InventoryEvidencePersonFirstName { get; set; }
        public string InventoryEvidencePersonLastName { get; set; }
        public string InventoryEvidencePersonMiddleName { get; set; }
        public string AgencyName { get; set; }
        public string OfficerBadgeNumber { get; set; }
        public string InventoryArticlesName { get; set; }
    }

    public class InventoryPropGroupDetails
    {
        public int PropGroupId { get; set; }
        public string PropGroupName { get; set; }
        public string PropGroupNotes { get; set; }
        public int? DeleteFlag { get; set; }
    }

    public class PropGroupHistoryDetails:LostFoundAddVm
    {
        public int Personalgroupid { get; set; }
        public DateTime? CreateDate { get; set; }
        public string InventoryNotes { get; set; }
        public int? DeleteFlag { get; set; }
        public int? CreatedBy { get; set; }
        public string PropGrpPersonFirstName { get; set; }
        public string PropGrpPersonLastName { get; set; }
        public string PropGrpPersonMiddleName { get; set; }
        public string Personalgroup { get; set; }
        public string OfficerBadgeNumber { get; set; }
        public int PersonalGroupHistoryId { get; set; }
        public FoundBy FoundBy { get; set; }
        public FoundIn FoundIn { get; set; }
    }

    public class InventoryVm
    {
        public List<InventoryDetails> InventoryDetails { get; set; }
        public List<InventoryItemDetails> InventoryItemDetails { get; set; }
        public List<InventoryPropGroupDetails> InventoryPropGroup { get; set; }
        public List<MoveBinDetails> ReleaseBinDetails { get; set; }
        public ReleaseDetails ReleaseDetails { get; set; }
        
        public List<Incarceration> ReleaseDetail{get;set;}

    }

    public class InventoryChangeDropDownDetails
    {
        public int? InventoryChangeGroupBinId { get; set; }
        public int InventoryChangeGroupGroupId { get; set; }
        public string InventoryChangeGroupBinName { set; get; }
        public string InventoryChangeGroupGroupName { set; get; }
    }

    public class InventoryChangeGroupVm
    {
        public List<InventoryDetails> InventoryChangeGroupDetails { get; set; }
        public List<InventoryItemDetails> InventoryChangeGroupItemDetails { get; set; }
        public List<InventoryChangeDropDownDetails> InventoryDropDownDetails { get; set; }
        public InventoryCheckActive InventoryCheckActive { get; set; }
        public InventoryDetails InventoryDetails { get; set; }

    }

    public enum InventoryCheckActive
    {
        SplitGroup,
        MoveBinGroup,
        NewBinGroup
    }   

    public enum Disposition
    {
        KeptInPossesion = 1,
        Donated,
        Mail,
        Storage,
        ReleasedToPerson,
        Evidence,
        Lost,
        Destroy           
    }

    public class BinInmateDetails
    {
        public int? ReceivingFlag { get; set; }
        public int? FacilityTransferFlag { get; set; }
        public int? FacilityId { get; set; }
        public int PersonalInventoryBinId { get; set; }      
        public string PersonalBinName { set; get; }
        public int? InmateCount { set; get; }
        public int? ItemsCount { set; get; }
        public int? LostFound { set; get; }
        public int PersonalInventoryId { get; set; }

    }

    public class BinInmateLoadDetails
    {
        public int PersonalInventoryBinId { get; set; }
        public string PersonalBinName { set; get; }
        public int? InmateId { get; set; }
        public PersonVm PersonInfoDetails { get; set; }
        public HousingDetail HousingDetails { get; set; }
        public int PersonalInventoryId { get; set; }
    }

    public class BinViewerDetails
    {
        public int PersonalInventoryBinId { get; set; }
        public string BinName { get; set; }
        public int? BinInmateCount { get; set; }
        public int? BinItemsCount { get; set; }
        public int? BinLostFound { get; set; }
        public int? FacilityId { get; set; }
        public int PersonalInventoryId { get; set; }
        public string FacilityAbbr { get; set; }
        public int? ReceivingFlag { get; set; }
        public int? FacilityTransferFlag { get; set; }
    }

    public class BinInventoryVm
    {
        public List<BinViewerDetails> BinAvailableDetails { get; set; }
        public List<BinViewerDetails> BinAssignedeDetails { get; set; }
        public List<BinViewerDetails> BinReceivingDetails { get; set; }
        public List<BinViewerDetails> BinFacilityTransferDetails { get; set; }
    }

    public class BinDeleteVm
    {
        public List<KeyValuePair<int, string>> ListInventoryLookUpDetails { get; set; }
    }

    public class PersonalInventoryPreBookVm:LostFoundAddVm
    {
        public int PersonalInventoryPreBookId { get; set; }
        public int InmatePrebookId { set; get; }
        public int InventoryArticles { set; get; }
        public int? InventoryQuantity { set; get; }
        public string InventoryDescription { set; get; }
        public string InventoryColor { set; get; }
        public int? ImportFlag { set; get; }
        public int? FacilityId { get; set; }
        public int? DeleteFlag { set; get; }
        public int? IncarcerationId { get; set; }
        public List<PersonalInventoryAddItems> PersonalInventoryAddItemsList { get; set; }
        public InventoryDetails InventoryDetails { get; set; }
        public InventoryAddItems InventoryAddItems { get; set; }
        public PersonalInventoryAddItems PersonalInventoryAddItems { get; set; }
        public string InventoryArticlesName { get; set; }
    }

    public class PersonalInventoryAddItems : PersonalInventory 
    {
        public int? InventoryAddItemsQuantity { set; get; }
        public int InventoryAddItemsArticles { set; get; }
        public string InventoryAddItemsDescription { set; get; }
        public string InventoryAddItemsColor { set; get; }
        public string PersonalBinName { get; set; }
        public int PersonalInventoryPrebookId { set; get; }
    }

    public enum InventoryAddItems
    {
        UseExistingGroup,
        UseNewGroup
    }

    public class MoveBinDetails : ReleaseDetails
    {
        public int MoveBinInventoryId { get; set; }
        public List<int> ListPersonalInventoryId { get; set; }
        public int? MoveBinInventoryBinId { get; set; }
        public int? MoveBinInmateId { get; set; }
        public string MoveBinName { get; set; }
        public string MoveBinInmateNumber { get; set; }
        public int? MoveBinInventoryQuantity { set; get; }
        public int MoveBinInventoryArticles { set; get; }
        public string MoveBinInventoryDescription { set; get; }
        public string MoveBinInventoryColor { set; get; }
        public double? MoveBinInventoryValue { set; get; }
        public int? InventoryDispositionCode { set; get; }
        public string InventoryDispositionName { set; get; }
        public int MoveBinItemCount { get; set; }
        public string MoveBinFirstName { get; set; }
        public string MoveBinLastName { get; set; }
        public string MoveBinMiddleName { get; set; }
        public int DeleteFlag { get; set; }
    }

    public class MoveBinVm
    {
        public List<MoveBinDetails> MoveBinDetails { get; set; }
        public List<MoveBinDetails> MoveBinDetailsItems { get; set; }
        public InmateBinEvent InmateBinEvents { get; set; }
        public ReleaseDetails ReleaseDetails { get; set; }
    }

    public class ReleaseDetails
    {
        public string PersonIdType { set; get; }
        public string PersonAddress { set; get; }
        public string CityStateZip { set; get; }
        public string PersonName { set; get; }
        public string DispoNotes { set; get; }
        public int? EvidencePersonnel { set; get; }
        public int? EvidenceAgencyId { set; get; }
        public int UpdatedBy { get; set; }
        public string EvidenceCaseNo { set; get; }
        public bool InventoryDamageFlag { get; set; }
        public string InventoryDamageDescription { get; set; }
        public string EvidenceOfficer { get; set; }
        public string EvidenceAgency { get; set; }
        public string ReleasedBy { get; set; }
        public string Disposition { get; set; }
        public PersonnelVm ReleasedByDetails { get; set; }
        public DateTime? InventoryReturnDate { get; set; }
    }

    public enum InmateBinEvent
    {
        Move,
        Release,
        Mail,
        Donate,
        Keep,
        Evidence, 
        Destroy,
        Lost
    }

    public class InventoryLookupVm
    {
        public List<KeyValuePair<int, string>> ListInventoryArticle { get; set; }
        public List<KeyValuePair<int, string>> ListInventoryColor { get; set; }
        public List<KeyValuePair<int, string>> ListInventoryDiposition { get; set; }
    }

    public class ReleaseAddressVm
    {
        public string AddressNumber { get; set; }
        public string AddressDirection { get; set; }
        public string AddressStreet { get; set; }
        public string AddressSuffix { get; set; }
        public string AddressDirectionSuffix { get; set; }
        public string AddressUnitType { get; set; }
        public string AddressUnitNumber { get; set; }
        public string AddressLine2 { get; set; }
        public string AddressCity { get; set; }
        public string AddressState { get; set; }
        public string AddressZip { get; set; }
    }

    public class InventoryInStorage
    {
        public int IncarcerationId { get; set; }
        public InmatePdfHeader InmateHeaderDetails { get; set; }
		public InventoryReceiptPersonDetails PersonDetails { get; set; }
        public List<InventoryDetails> InventoryDetails { get; set; }
        public Form FormData { get; set; }
        public bool IsReleased { get; set; }
        public ReleaseDetails ReleaseDetails { get; set; }
        public InventoryDetails PropertyGroupDetails { get; set; }
        public JObject CustomLabel { get; set; }
    }

    public class InventoryReceiptPersonDetails
    {
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public DateTime? Dob { get; set; }
        public string InmateNumber { get; set; }
        public string BookingNumber { get; set; }
        public string Gender { get; set; }
        public string Race { get; set; }
        public int Age { get; set; }
        public decimal Balance { get; set; }
        public int PersonId { get; set; }
        public string PropertyGroupPhotoPath { get; set; }
    }
}

