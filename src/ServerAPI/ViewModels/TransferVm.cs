using System;
using System.Collections.Generic;

namespace ServerAPI.ViewModels
{
    public class TransEligibleDetailsVm
    {
        public List<TransferEligibleVm> TransEligibleDetails { get; set; }
        public List<PrivilegeDetailsVm> LstLocation { get; set; }
        public List<HousingDetail> LstHousingDetails { get; set; }
    }

    public class TransferEligibleVm
    {
        public string InmateNumber { get; set; }
        public int InmateId { get; set; }
        public int IncarcerationId { get; set; }
        public int FacilityId { get; set; }
        public string FacilityAbbr { get; set; }
        public int PersonId { get; set; }
        public DateTime? OverallSentStartDate { get; set; }
        public DateTime? OverallFinalReleaseDate { get; set; }
        public DateTime? DateIn { get; set; }
        public int? EligibleLookup { get; set; }
        public int? ApprovalLookup { get; set; }
        public DateTime? EligibleDate { get; set; }
        public DateTime? ApprovalDate { get; set; }
        public string EligibleNote { get; set; }
        public string ApprovalNote { get; set; }
        public HousingDetail HousingDetail { get; set; }
        public PersonInfoVm PersonDetails { get; set; }
        public FlagAlertVm FlagAlers { get; set; }
        public List<AppointmentClass> Appointment { get; set; }
        public string PersonFlagName { get; set; }
        public string InmateFlagName { get; set; }
        public string DietMedFlagName { get; set; }
        public int InmateClassificationId { get; set; }
        public string ClassificationReason { get; set; }
    }

    public class EligibleSearchVm
    {
        public bool IsFlag { get; set; }
        public int? Eligiblility { get; set; }
        public int? Approval { get; set; }
        public int? Housing { get; set; }
        public string Classify { get; set; }
        public int? Gender { get; set; }
        public int InmateId { get; set; }
        public int PersonnelId { get; set; }
        public bool None { get; set; }
        public int InternalId { get; set; }
        public int ExternalId { get; set; }
        public int? FacilityId { get; set; }
        public bool AlldateFlag { get; set; }
        public DateTime? Date { get; set; }
        public int? PersonFlagId { get; set; }
        public int? InmateFlagId { get; set; }
        public int? DietFlagId { get; set; }
        public int? MedFlagId { get; set; }
        public FutureAppt FutureAppt { get; set; }
    }

    public class TransferHistoryVm
    {
        public int IncarcerationId { get; set; }
        public int? EligibleLookup { get; set; }
        public int? ApprovalLookup { get; set; }
        public DateTime? EligibleDate { get; set; }
        public string EligibleNote { get; set; }
        public DateTime? EligibleSaveDate { get; set; }
        public int? EligibleSaveBy { get; set; }
        public DateTime? ApprovalDate { get; set; }
        public string ApprovalNote { get; set; }
        public DateTime? ApprovalSaveDate { get; set; }
        public int? ApprovalSaveBy { get; set; }
        public PersonnelVm ApprovalDetails { get; set; }
        public PersonnelVm EligibleDetails { get; set; }
    }

    public class EligibleInmateCountVm
    {
        public List<KeyValuePair<int, string>> Counts { get; set; }
    }

    public enum FutureAppt
    {
        All,
        Today,
        Tomorrow
    }

    public class PersnlInventoryVm
    {
        public int PersonalInventoryId { get; set; }
        public int? InmateId { get; set; }
        public DateTime? InventoryDate { get; set; }
        public int InventoryArticles { get; set; }
        public int? InventoryQuantity { get; set; }
        public string InventoryUom { get; set; }
        public string InventoryDescription { get; set; }
        public int? InventoryDispositionCode { get; set; }
        public float InventoryValue { get; set; }
        public string InventoryDestroyed { get; set; }
        public string InventoryMailed { get; set; }
        public int? InventoryMailPersonId { get; set; }
        public int? InventoryMailAddressId { get; set; }
        public int InventoryOfficeId { get; set; }
        public string InventoryColor { get; set; }
        public string InventoryBinNumber { get; set; }
        public DateTime InventoryReturnDate { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime UpdateDate { get; set; }
        public int PersonalInventoryBinId { get; set; }
        public string PersonName { get; set; }
        public string PersonIdType { get; set; }
        public string PersonAddress { get; set; }
        public string DispoNotes { get; set; }
        public int CreatedBy { get; set; }
        public int? UpdatedBy { get; set; }
        public int? DeleteFlag { get; set; }
        public DateTime? DeleteDate { get; set; }
        public int? DeletedBy { get; set; }
        public string CityStateZip { get; set; }
        public int? PersonalInventoryGroupId { get; set; }
        public string DeleteReason { get; set; }
        public string DeleteReasonNote { get; set; }
        public int? InventoryEvidencePersonnelId { get; set; }
        public int? InventoryEvidenceAgencyId { get; set; }
        public string InventoryEvidenceCaseNumber { get; set; }
    }

    public class ExternalSearchVm
    {
        public int InmateId { get; set; }
        public int? BinId { get; set; }
        public string BinNumber { get; set; }
    }

    public class ExtTransferFacilityVm
    {
        public List<InmateTransferDetails> TransferInmateDetails { get; set; }
        public int FacilityId { get;set;}
        public int NewFacilityId { get; set; }
        public string ExtLocation { get; set; }
        public int ExtLocationId { get; set; }
        public bool IsExtenalLocation { get; set; }
        public InmateTransferDetails TransferDetail { get; set; }

    }
    public class NewFacilityTransferVm
    {
        public int IncarcerationId { get; set; }
        public int InmateId { get; set; }
        public int IncarcerationFacilityHistoryId { get; set; }
        public int FacilityId { get; set; }
    }

    public class InmateSupplyVm
    {
        public int InmateSupplySizeLookupId { get; set; }
        public int? CheckoutCount { get; set; }
        public int? CheckInCount { get; set; }
        public int? CheckBy { get; set; }
        public int InmateId { get; set; }
        public DateTime? CheckInDate { get; set; }
        public int? UpdatedBy { get; set; }
        public int InmateSupplyLookupId { get; set; }
        public int? DoNotTransferFlag { get; set; }
        public int? QtyTotal { get; set; }
        public int? QtyCheckedOut { get; set; }
        public int? QtyOnHand { get; set; }
    }

    public class InventoryBinVm
    {
        public int PersonalInventoryId { get; set; }
        public int PersonalInventoryBinId { get; set; }
        public int? DoNotMoveDuringTransfer { get; set; }
        public int? InmateId { get; set; }
        public string InventoryBinNumber { get; set; }
        public int? InventoryDispositionCode { get; set; }
        public string BinName { get; set; }
        public string InmatePersonalInventory { get; set; }
    }

    public class TransferCountDetails
    {
        public int Count { get; set; }
        public int LocationId { get; set; }
        public int HousingUnitId { get; set; }
        public string FacilityAbbr { get; set; }
        public string Description { get; set; }
        public string HousingLocation { get; set; }
        public string HousingNumber { get; set; }      
        public DateTime? AppointmentDate { get; set; }
    }

    public class InternalTransferVm
    {
        public List<TransferCountDetails> LocationList { get; set; }
        public List<TransferCountDetails> HousingList { get; set; }
        public List<AoAppointmentVm> ScheduleDetails { get; set; }
    }

    public class ExternalTransferVm
    {
        public List<TransferCountDetails> LocationDetails { get; set; }
        public List<TransferCountDetails> ScheduleDetails { get; set; }
        public List<InmateTransferDetails>  InmateDetails { get; set;}
        public List<InmateTransferDetails>  AllInmateDetails { get; set;}
    }
}
