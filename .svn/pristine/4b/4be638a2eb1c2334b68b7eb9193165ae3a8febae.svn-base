using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;

namespace ServerAPI.ViewModels
{
    public class AgencyVm
    {
        public int AgencyId { get; set; }
        public string AgencyName { get; set; }
        public string AgencyAbbreviation { get; set; }
        public bool AgencyArrestingFlag { get; set; }
        public bool AgencyCourtFlag { get; set; }
        public int? AgencyInactiveFlag { get; set; }
        public bool AgencyOriginatingFlag { get; set; }
    }

    public class FacilityVm
    {
        public int FacilityId { get; set; }
        public string FacilityAbbr { get; set; }
        public string FacilityName { get; set; }
    }

    public class InmatePrebookVm : CourtCommitSentEntryVm
    {
        //public string InmateNumber { get; set; }
        public int? InmateId { get; set; }
        public int InmatePrebookId { get; set; }
        public int? IncarcerationId { get; set; }
        public int? TempHoldId { get; set; }
        public int? PersonId { get; set; }
        public string PersonFirstName { get; set; }
        public string PersonLastName { get; set; }
        public string PersonMiddleName { get; set; }
        public string PersonSuffix { get; set; }
        public DateTime? PersonDob { get; set; }
        public string PrebookNumber { get; set; }
        public DateTime? ArrestDate { get; set; }
        public DateTime? PrebookDate { get; set; }
        public DateTime? ArrestFromDate { get; set; }
        public DateTime? ArrestToDate { get; set; }
        public DateTime? PrebookFromDate { get; set; }
        public DateTime? PrebookToDate { get; set; }
        public PersonVm IdentifiedPerson { get; set; }
        public double[] CaseType { get; set; }
        public int WizardFixedStepsId { get; set; }
        public int ArrestOfficerId { get; set; }
        public PersonnelVm ArrestingOfficer { get; set; }
        public string ArrestOfficerName { get; set; }
        public int? TransportOfficerId { get; set; }
        public string TransportOfficerLastName { get; set; }
        public string TransportOfficerFirstName { get; set; }
        public string TransportOfficerNumber { get; set; }
        public string TransportOfficerName { get; set; }
        public int ArrestAgencyId { get; set; }
        public string ArrestAgencyName { get; set; }
        public string ArrestAgencyAbbr { get; set; }
        public int FacilityId { get; set; }
        public string FacilityAbbr { get; set; }
        public string CaseNumber { get; set; }
        public string ContactNumber { get; set; }
        public string ArrestLocation { get; set; }
        public string PrebookNotes { get; set; }
        public bool DeleteFlag { get; set; }
        public bool TemporaryHold { get; set; }
        public bool CourtCommitFlag { get; set; }
        public bool IntakeReviewAccepted { get; set; }
        public bool IntakeReviewDenied { get; set; }
        public string IntakeReviewComment { get; set; }
        public bool PrebookComplete { get; set; }
        public int PersonnelId { get; set; }
        public string PrebookType { get; set; }
        public bool MedPrescreenStartFlag { get; set; }
        public MedPrescreenStatusFlag? MedPrescreenStatusFlag { get; set; }
        public DateTime? MedPrescreenStatusDate { get; set; }
        public int CreateBy { get; set; }
        public DateTime? CreateDate { get; set; }
        public int? UpdateBy { get; set; }
        public DateTime? UpdateDate { get; set; }
        public string VehicleColor1 { get; set; }
        public string VehicleColor2 { get; set; }
        public string VehicleDescription { get; set; }
        public string VehicleDisposition { get; set; }
        public bool VehicleInvolvedFlag { get; set; }
        public string VehicleLicense { get; set; }
        public string VehicleLocation { get; set; }
        public int? VehicleMakeid { get; set; }
        public int? VehicleModelid { get; set; }
        public string VehicleState { get; set; }
        public string VehicleType { get; set; }
        public string VehicleVIN { get; set; }
        public int? VehicleYear { get; set; }

        public string[] VehicleFlagString { get; set; }

        //public bool ActiveInmate { get; set; }
        public int? InmateCount { set; get; }
        public int[] LstInmatePreBookId { get; set; }
        public PrebookDetails LstPrebookDetails { get; set; }
        public InmateDetail InmateDetail { get; set; }
        public bool ActiveInmate { get; set; }
        public int CompleteFlag { get; set; }
        public int? WizardProgressId { get; set; }
        public List<AoWizardStepProgressVm> WizardStepProgress { get; set; }
        public int InmatePrebookCaseId { get; set; }
        public string CaseNote { get; set; }
        public string MedPrescreenStatusNote { get; set; }
        public bool PrebookReviewed { get; set; }
        public bool PersonIdentified { get; set; }
        public bool IdentificationAccepted { get; set; }
        public string CcQueueName { get; set; }
        public double? ArresteeBAC1{get; set;}
        public double? ArresteeBAC2 { get; set; }

        public int[] DisplayArresteeCondition { get; set; }
        public int[] DisplayArresteeBAC { get; set; }
        public int[] DisplayArresteeBehavior { get; set; }

        public List<InmatePreBookCondition> ArresteeCondition { get; set; }
        public List<InmatePreBookCondition> ArresteeBAC { get; set; }
        public List<InmatePreBookCondition> ArresteeBehavior { get; set; }
    }

    public enum MedPrescreenStatusFlag
    {
        Reject = -1,
        Accept = 1,
        Bypass = 2,
        NotRequired = 3
    }

    public class Module
    {
        public List<SubModule> SubModuleList { get; set; }
        public string ModuleName { get; set; }
        public int ModuleId { get; set; }
        public string AppAoModuleIcon { get; set; }

    }

    public class SubModule
    {
        public string SubModuleName { get; set; }
        public string Route { get; set; }
        public int SubModuleId { get; set; }
        public int? SubModuleOrder { get; set; }
        public List<SubModuleDetail> SubModuleDetailList { get; set; }
    }

    public class SubModuleDetail
    {
        public string DetailName { get; set; }
        public string DetailRoute { get; set; }
        public string DetailTooltip { get; set; }
        public int DetailOrder { get; set; }
        public int DetailId { get; set; }
    }

    public class PrebookAttachment
    {
        public int AttachmentId { get; set; }
        public DateTime? AttachmentDate { get; set; }
        public string AttachType { get; set; }
        public string AttachmentType { get; set; }
        public string AttachmentTitle { get; set; }
        public string AttachmentDescription { get; set; }
        public string AttachmentKeyword1 { get; set; }
        public string AttachmentKeyword2 { get; set; }
        public string AttachmentKeyword3 { get; set; }
        public string AttachmentKeyword4 { get; set; }
        public string AttachmentKeyword5 { get; set; }
        public bool AttachmentDeleted { get; set; }
        public string AttachmentFile { get; set; }
        public int? InmatePrebookId { get; set; }
        public int? AltSentRequestId { get; set; }
        public int? InmateId { get; set; }
        public string History { get; set; }
        public int? DisciplinaryIncidentId { get; set; }
        public PersonnelVm CreatedBy { get; set; }
        public DateTime? UpdateDate { get; set; }
        public PersonnelVm UpdatedBy { get; set; }
        public int? MedicalInmateId { get; set; }
        public int? ArrestId { get; set; }
        public int? IncarcerationId { get; set; }
        public int? ProgramCaseInmateId { get; set; }
        public int? GrievanceId { get; set; }
        public int? PREAInmateId { get; set; }
        public int? RegistrantRecordId { get; set; }
        public int? FacilityId { get; set; }
        public string HousingUnitLocation { get; set; }
        public string HousingUnitNumber { get; set; }
        public int? AltSentId { get; set; }
        public int? ExternalInmateId { get; set; }
        public PersonVm PersonInfo { get; set; }
        public List<KeyValuePair<string, string>> HousingGroups { get; set; }
        public string[] Histories { get; set; }
        public int? InvestigationId { get; set; }
        public int? MailRecordid  { get; set; }
        public bool AppletsSavedIsExternal { get; set; }
    }

    public class AttachmentSearch
    {
        public int ActiveFlag { get; set; }
        public int FacilityId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int InmateId { get; set; }
        public int PersonnelId { get; set; }
        public int AppletsSavedID { get; set; }
        public int DeleteFlag { get; set; }
        public string Flag { get; set; }
        public int ArrestId { get; set; }
        public int RegistrantPersonId { get; set; }
        public string Building { get; set; }
        public string Number { get; set; }
        public List<KeyValuePair<string, string>> HousingList { get; set; }
        public int IncarcerationId { get; set; }
        public int InmateprebookId { get; set; }
        public int AltSentRequestId { get; set; }
        public string FlagFilterForIncar { get; set; }
        public string FlagFilterForBooking { get; set; }
        public int GrievanceID { get; set; }
        public string Keyword { get; set; }
    }

    public class PrebookCharge
    {
        public int? InmatePrebookId { get; set; }
        public int? InmatePrebookChargeId { get; set; }
        public int? CrimeForceId { get; set; }
        public string CrimeLookupForceString { get; set; }
        public int? InmatePrebookWarrantId { get; set; }
        public int? WarrantId { get; set; }
        public int? CrimeLookupId { get; set; }
        public int? CrimeId { get; set; }
        public int? ArrestId { get; set; }
        public int ReplicateCount { get; set; }
        public int CrimeCount { get; set; }
        public int CrimeNumber { get; set; }
        public string CrimeCodeType { get; set; }
        public string CrimeSection { get; set; }
        public string CrimeDescription { get; set; }
        public string CrimeStatuteCode { get; set; }
        public string CrimeStatuteId { get; set; }
        public string CrimeSubSection { get; set; }
        public decimal? BailAmount { get; set; }
        public bool BailNoBailFlag { get; set; }
        public string CrimeType { get; set; }
        public int? CrimeStatusLookup { get; set; }
        public string CrimeStatus { get; set; }
        public string CrimeStatusAcronms { get; set; }
        public string CrimeNotes { get; set; }
        public int? CrimeGroupId { get; set; }
        public string CrimeGroup { get; set; }
        public int? ChargeQualifierId { get; set; }
        public string CrimeQualifierLookup { get; set; }
        public string CrimeQualifier { get; set; }
        public int CreateBy { get; set; }
        public DateTime? CreateDate { get; set; }
        public int? UpdateBy { get; set; }
        public DateTime? UpdateDate { get; set; }
        public bool DeleteFlag { get; set; }
        public bool IsForceCharge { get; set; }
        public int? DeleteBy { get; set; }
        public DateTime? DeleteDate { get; set; }
        public PersonnelVm CreatedPersonnel { get; set; }
        public PersonnelVm UpdatedPersonnel { get; set; }
        public int? InmatePrebookCaseId { get; set; }
        public DateTime? OffenceDate { get; set; }
        public int EventPersonId { get; set; }
    }

    public class InmatePrebookWarrantVm
    {
        public int InmatePrebookWarrantId { get; set; }
        public int InmatePrebookId { get; set; }
        public string WarrantNumber { get; set; }
        public string WarrantType { get; set; }
        public string WarrantCounty { get; set; }
        public string WarrantDescription { get; set; }
        public decimal? WarrantBailAmount { get; set; }
        public int? WarrantAgencyId { get; set; }
        public int? ArrestId { get; set; }
        public int PersonId { get; set; }
        public string WarrantAgencyText { get; set; }
        public string AgencyName { get; set; }
        public DateTime? WarrantIssueDate { get; set; }
        public string WarrantChargeType { get; set; }
        public bool WarrantNoBail { get; set; }
        public DateTime? CreateDate { get; set; }
        public int CreateBy { get; set; }
        public DateTime? UpdateDate { get; set; }
        public int? UpdateBy { get; set; }
        public bool DeleteFlag { get; set; }
        public DateTime? DeleteDate { get; set; }
        public int? DeletedBy { get; set; }
        public int? WarrantId { get; set; }
        public string WarrantBailType { get; set; }
        public List<PrebookCharge> WarrantPrebookCharges { get; set; }
        public int InmatePrebookCaseId { get; set; }
        public string OriginatingAgency { get; set; }
        public string OriginatingAgencyText { get; set; }
        public bool LocalWarrant { get; set; }
    }

    public class CrimeHistoryVm
    {
        public int CrimeHistoryId { get; set; }
        public int? InmatePrebookChargeId { get; set; }
        public int? CrimeId { get; set; }
        public int? CrimeForceId { get; set; }
        public int? CrimeLookupId { get; set; }
        public string CrimeLookupForceString { get; set; }
        public string CrimeType { get; set; }
        public string CrimeNotes { get; set; }
        public int? CrimeNumber { get; set; }
        public int? CrimeCount { get; set; }
        public bool CrimeDeleteFlag { get; set; }
        public decimal? BailAmount { get; set; }
        public bool BailNoBailFlag { get; set; }
        public DateTime? CreatDate { get; set; }
        public int CreatedBy { get; set; }
        public string ChargeQualifierLookup { get; set; }
        public int? CrimeStatusLookup { get; set; }
        public int? CrimeQualifierLookup { get; set; }
    }

    public class AppletsSavedVm
    {
        public IFormFile File { get; set; }
        public int AppletsSavedId { get; set; }
        public string AppletsSavedTitle { get; set; }
        public string AppletsSavedPath { get; set; }
        public string AppletsSavedDescription { get; set; }
        public string AppletsSavedKeyword1 { get; set; }
        public string AppletsSavedKeyword2 { get; set; }
        public string AppletsSavedKeyword3 { get; set; }
        public string AppletsSavedKeyword4 { get; set; }
        public string AppletsSavedKeyword5 { get; set; }
        public int? AppletsNumber { get; set; }
        public int? AppletsDeleteFlag { get; set; }
        public int? IncidentId { get; set; }
        public int? CaseId { get; set; }
        public int? ProbationId { get; set; }
        public int? GangId { get; set; }
        public int? ArrestId { get; set; }
        public int? InmateId { get; set; }
        public int? CallId { get; set; }
        public int? EvidenceId { get; set; }
        public int? CivilId { get; set; }
        public DateTime? CreateDate { get; set; }
        public DateTime? UpdateDate { get; set; }
        public int CreatedBy { get; set; }
        public int? LastAccessBy { get; set; }
        public DateTime? LastAccessDate { get; set; }
        public string AppletsSavedType { get; set; }
        public int? MedicalInmateId { get; set; }
        public int? DeletedBy { get; set; }
        public DateTime? DeleteDate { get; set; }
        public int? UpdatedBy { get; set; }
        public int? DisciplinaryIncidentId { get; set; }
        public int? FacilityId { get; set; }
        public string HousingUnitLocation { get; set; }
        public string HousingUnitNumber { get; set; }
        public int? GeneralInmateId { get; set; }
        public int? RegistrantRecordId { get; set; }
        public int? IncarcerationId { get; set; }
        public int? InmatePrebookId { get; set; }
        public int? RequestId { get; set; }
        public string AppletsSavedDocType { get; set; }
        public int? AltSentRequestId { get; set; }
        public int? AltSentId { get; set; }
        public int? GrievanceId { get; set; }
        public int? ExternalInmateId { get; set; }
        public int? ExternalAcceptFlag { get; set; }
        public DateTime? ExternalAcceptDate { get; set; }
        public int? ExternalAcceptBy { get; set; }
        public string BookingNumber { get; set; }
        public PersonVm PersonName { get; set; }
        public PersonVm CreatedByName { get; set; }
        public PersonVm UpdatedByName { get; set; }
    }

    public class PersonalInventoryVm
    {
        public int PersonalInventoryPreBookId { get; set; }
        public int InmatePrebookId { set; get; }
        public int InventoryArticles { set; get; }
        public int InventoryQuantity { get; set; }
        public string InventoryDescription { set; get; }
        public string InventoryColor { set; get; }
        public DateTime CreateDate { set; get; }
        public int CreateBy { set; get; }
        public bool DeleteFlag { set; get; }
        public DateTime? DeleteDate { set; get; }
        public int? DeleteBy { set; get; }
        public int? ImportFlag { set; get; }
        public DateTime? ImportDate { set; get; }
        public int? ImportBy { set; get; }
        public string InventoryArticlesName { get; set; }
    }

    public class GetFormTemplates
    {
        public string DisplayName { set; get; }
        public string HtmlPath { set; get; }
        public string HtmlFileName { set; get; }
        public int FormTemplatesId { set; get; }
        public int CategoryId { set; get; }
        public int Cnt { set; get; }
        public int FormInterfaceFlag { set; get; }
        public int FormRecordId { get; set; }
        public string FormNotes { get; set; }
        public bool DeleteFlag { get; set; }
        public DateTime? CreateDate { get; set; }
        //below property will be mapped with either incarcerationId or arrestId
        public int? Id { get; set; }
        //below property will be used in repositary place
        public string RequireUponCompleteBookTypeString { get; set; }
        public string RequireUponCompleteFacilityString { get; set; }
        public int? RequireUponComplete { get; set; }
    }

    public class LoadSavedForms
    {
        public string DisplayName { set; get; }
        public string Notes { set; get; }
        public int FormRecordId { set; get; }
        public int FormTemplatesId { set; get; }
        public int CategoryId { set; get; }
        public string HtmlPath { set; get; }
        public string HtmlFileName { set; get; }
        public string XmlStr { set; get; }
        public int? FormInterfaceSent { set; get; }
        public int? InterfaceBypassed { set; get; }
        public int InterfaceFlag { set; get; }
        public DateTime? Date { set; get; }
        public int DeleteFlag { set; get; }
        public int CreatedPersonnel { set; get; }
        public int InmatePrebookId { get; set; }
        public int UpdateBy { set; get; }
        public string FormCategoryFolderName { get; set; }
    }

    public class AddForm
    {
        public string DisplayName { set; get; }
        public string PersonFirstName { set; get; }
        public string PersonLastName { set; get; }
        public DateTime? PersonDob { set; get; }
        public string File { set; get; }

    }

    public class ListForm
    {
        public string DisplayName { set; get; }
        public string PersonFirstName { set; get; }
        public string PersonLastName { set; get; }
        public DateTime? PersonDob { set; get; }
        public string File { set; get; }
        public string Notes { set; get; }
        public string FormData { set; get; }
        public int InmatePrebookId { get; set; }
    }

    public class GlobalNumber
    {
        public string AtimsNumberPrefix { get; set; }
        public string AtimsNumberSuffix { get; set; }
        public int AtimsNumberCounter { get; set; }
        public int? AtimsNumberPadding { get; set; }
        public int AtimsNumberPaddingLen { get; set; }
        public int? AtimsNumberYearResetCounter { get; set; }
        public int? AtimsNumberAllowSequence { get; set; }
        public int? AtimsNumberUseYear { get; set; }
        public string AtimsNumberYearFormat { get; set; }
        public string AtimsNumberYearCompare { get; set; }

    }

    public enum FormScreen
    {
        PrebookIntake,
        Courtcommit
    }

    public class VehicleMakeVm
    {
        public int VehicleMakeId { get; set; }
        public string VehicleMakeName { get; set; }
        public string MakeCode { get; set; }
        public int DeleteFlag { get; set; }
    }

    public class VehicleModelVm
    {
        public int VehicleModelId { get; set; }
        public string VehicleModelName { get; set; }
        public string ModelCode { get; set; }
        public int VehicleMakeId { get; set; }
        public int DeleteFlag { get; set; }
    }

    public class MedPrescreenStatus
    {
        public int PrebookId { get; set; }
        public MedPrescreenStatusFlag MedPrescreenStatusFlag { get; set; }
        public string MedPrescreenStatusNote { get; set; }
        public int? MedStartFlag { get; set; }
        public int? MedCompleteFlag { get; set; }
    }

    public enum AtimsGlobalNumber
    {
        BookingNumber = 1,
        InmateNumber = 2,
        IncidentNumber = 3,
        RegNumber = 4,
        GrievanceNumber = 5,
        BailReceiptNumber = 6,
        PrebookNumber = 7,
        PropertyGroup = 8,
        BookNumber = 9,
        AltSentReceipt = 10,
        InvestigationNumber = 11,
    }

    public class PrebookCountVm
    {
        public Dictionary<string, int> PrebookCounts { get; set; }
        public string PrebookDetails { get; set; }
        public string IntakeReviewOption { get; set; }
        public int DefaultFacilitySelected { get; set; }
        public List<InmatePrebookVm> CommitoverdueList { get; set; }
        public List<InmatePrebookVm> CommitschList { get; set; }
    }

    public enum PrebookDetails
    {
        AllInProgress,
        PrebookReady,
        CourtCommitSchToday,
        CourtCommitSch,
        CourtCommitOverdue,
        MedicallyRejected,
        InProgress,
        ByPassed,
        Identification,
        Intake,
        TempHold,
        NotReviewed,
        DeniedReviewAgain
    }

    public class AttachmentComboBoxes
    {
        public List<LookupVm> LookupTypes { get; set; }
        public List<KeyValuePair<string, int>> BookingNumers { get; set; }
        public List<ProgramTypes> Programs { get; set; }
        public List<KeyValuePair<string, int>> Facilities { get; set; }
        public List<HousingUnitListVm> HousingList { get; set; }
        public List<HousingUnitGroupVm> HousingGroupList { get; set; }
    }

    public class InmatePrebookCaseVm
    {
        public int InmatePrebookCaseId { get; set; }
        public decimal? CaseTypeId { get; set; } // arrest type id
        public string CaseType { get; set; } // arrest type value
        public string CaseNumber { get; set; }
        public string CaseNote { get; set; }
        public List<PrebookCharge> LstCharges { get; set; }
        public List<InmatePrebookWarrantVm> LstWarrants { get; set; }
        public int InmatePrebookId { get; set; }
        public bool DeleteFlag { get; set; }
        public string DeleteReason { get; set; }
    }

    public class PrebookValidateConfirmData
    {
        public List<PrebookValidateConfirmRecord> PrebookValidateConfirmRecords { get; set; }
    }

    public class PrebookValidateConfirmRecord
    {
        public int FormRecordId { get; set; }
        public int InmatePrebookId { get; set; }
        public string TagName { get; set; }
        public string FieldMapping { get; set; }
        public int FieldOrder { get; set; }
        public string Entered { get; set; }
        public string StoredValue { get; set; }
        public string ControlType { get; set; }
    }

    public class PrebookProperty
    {
        public IEnumerable<PersonalInventoryVm> ListPersonalInventory { get; set; }
        public InmatePdfHeader InmateHeaderDetails { get; set; }
        public string PersonName { get; set; }
        public string PrebookNumber { get; set; }
        public int prebookId { get; set; }
        public string logoPath { get; set; }
         public bool client { get; set; }
    }

    public class IntakePrebookSelectVm
    {
        public int InmatePrebookId { get; set; }
        public KeyValuePair<int, string> FacilityValueDetail { get; set; }
    }

    public class GetPrebookSearchVm
    {
        public string PreBookfromDate { get; set; }
        public string PreBooktoDate { get; set; }
        public int? TransOfficerId { get; set; }
        public int? ArrestingAgencyId { get; set; }
        public string ArrestfromDate { get; set; }
        public string ArrestOfficerName { get; set; }
        public string TransportOfficerName { get; set; }
        public string ArresttoDate { get; set; }
        public int? ArrestingOfficer { get; set; }
        public int FacilityId { get; set; }
        public string ArrestLocation { get; set; }
        public string CaseNumber { get; set; }
        public string PersonLastName { get; set; }
        public string PersonFirstName { get; set; }
        public string PersonSuffix { get; set; }
        public string Dob { get; set; }
        public string Active { get; set; }
        public string Deleted { get; set; }
        public string TempHold { get; set; }
        public string MyPreBooks { get; set; }
        public int? CourtCommitFlag { get; set; }
        public bool IsSearch { get; set; }

        //duplicate
        public int? TempHoldSearch { get; set; }
        public int? DeletedSearch { get; set; }
        public int? MyPreBooksSearch { get; set; }
        public int? ActiveSearch { get; set; }

        public int? FacilitySearch { get; set; }

    }

    public class PdfData
    {
        public object JsonData { get; set; }
        public string FormName { get; set; }
    }
    public class MonitorPreScreenVm
    {
        public int InmatePrebookId { get; set; }       
        public string PersonFirstName { get; set; }
        public string PersonLastName { get; set; }
        public string PersonMiddleName { get; set; }
        public string PersonSuffix { get; set; }
        public DateTime? PersonDob { get; set; }       
        public DateTime? ArrestDate { get; set; }
        public DateTime? PrebookDate { get; set; }
        public int? PersonId { get; set; }
        public string PhotoFilePath { get; set; }
    }

      public class InmatePreBookCondition
    {
        public int InmatePrebookId { get; set; }
        public int ArresteeConditionLookupIndex { get; set; }
        public int BACMethodLookupIndex { get; set; }
        public int ArresteeBehaviorLookupIndex { get; set; }

        public bool DeleteFlag{get; set;}
        public double? BAC1 { get; set; }
        public double? BAC2 { get; set; }
       
    }

}