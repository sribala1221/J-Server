using GenerateTables.Models;
using System;
using System.Linq;
using System.Collections.Generic;

namespace ServerAPI.ViewModels
{
    public class InmateVm
    {
        public int InmateId { get; set; }
        public string InmateNumber { get; set; }
        public string InmateCurrentTrack { get; set; }
        public bool InmateActive { get; set; }
        public int? FacilityId { get; set; }
        public int? InmateCurrentTrackId { get; set; }
        public string PersonPhoto { get; set; }
        public PersonVm Person { get; set; }
        public HousingDetail HousingUnit { get; set; }
        public InmateClassificationVm InmateClassification { get; set; }
        public int HousingUnitId { get; set; }
        public string Destination { get; set; }
        public int? DestinationId { get; set; }
        public DateTime? EnrouteOutDate { get; set; }
        public DateTime? EnrouteStartOut { get; set; }
    }

    public class InmateDetail
    {
        public int InmateId { get; set; }
        public int ArrestId { get; set; }
        public string InmateNumber { get; set; }
        public string InmateCurrentTrack { get; set; }
        public bool InmateActive { get; set; }
        public PersonVm Person { get; set; }
        public PersonVm PersonDetails { get; set; }
        public FacilityVm Facility { get; set; }
        public HousingDetail HousingUnit { get; set; }
        public InmateClassificationVm InmateClassification { get; set; }
        public IEnumerable<LookupVm> Lookup { get; set; }
        public int PersonId { get; set; }
        public bool? ValidationFlag {get;set;}
        public bool IsSaveDraft {get;set;}

    }

    public class InmateClassificationDetail
    {
        public int InmateId { get; set; }
        public int InmateClassificationId { get; set; }
        public DateTime? DateAssigned { get; set; }
        public DateTime? DateUnAssigned { get; set; }
        public string ClassificationType { get; set; }
        public string ClassificationReason { get; set; }
    }

    public class InmateHeaderVm
    {
        public InmateDetailVm MyInmateDetail { get; set; }
        public List<IdentifierVm> InmatePhoto { get; set; }
        public List<AlertFlagsVm> InmateAlerts { get; set; }
        public List<ObservationLogVm> LstObservationLog { get; set; }
        public List<PersonClassificationDetails> LstAssociation { get; set; }
        public List<PrivilegeDetailsVm> LstPrivilegesAlerts { get; set; }
        public List<KeepSeparateVm> LstKeepSep { get; set; }
        public List<IncarcerationDetail> IncarcerationAndBooking { get; set; }
    }

    public class InmateDetailVm
    {
        public bool InmateActive { get; set; }
        public string InmateNumber { get; set; }
        public string IncLastName { get; set; }
        public string IncFirstName { get; set; }
        public string IncMiddleName { get; set; }
        public string IncSuffix { get; set; }
        public int InmateId { get; set; }
        public int FacilityId { get; set; }
        public int? InmateClassificationId { get; set; }
        public int? HousingUnitId { get; set; }
        public int? HousingUnitListId { get; set; }
        public string PhotoFilePath { get; set; }
        public string Facility { get; set; }
        public HousingDetail HousingDetail { get; set; }
        public string CurrentTrack { get; set; }
        public int? InmateCurrentTrackId { get; set; }
        public string Moniker { get; set; }
        public string Classification { get; set; }
        public DateTime? SchdRelease { get; set; }
        public string WorkCrew { get; set; }
        public string WorkFurlough { get; set; }
        public int? IncarcerationId { get; set; }
        public PersonCharVm Characteristics { get; set; }
        public InmateFeeVm InmateFee { get; set; }
        public string AltSentProgramAbbr { get; set; }
        public bool? AltSentProgramManageSites { get; set; }
        public string AltSentSiteName { get; set; }
        public string PersonalInventory { get; set; }
        public string PhonePin { get; set; }
        public decimal? InmateBalance { get; set; }
        public decimal? InmateDepositeBalance { get; set; }
        public decimal? InmateDept { get; set; }
        public PersonVm PersonDetail { get; set; }
        public int PersonId { get; set; }
        public List<VisitationDetails> VisitationDetails { get; set; }
    }

    public class InmateFeeVm
    {
        public decimal? Balance { get; set; }
        public decimal? Pending { get; set; }
        public decimal? Fee { get; set; }
    }

    public class IdentifierVm
    {
        public int PersonId { get; set; }
        public int DeleteFlag { get; set; }
        public int? IdentifierId { get; set; }
        public string IdentifierTypeName { get; set; }
        public string PhotographDate { get; set; }
        public string PhotographRelativePath { get; set; }
        public string IdentifierType { get; set; }
        public DateTime? CreatedDate { get; set; }
        public string IdentifierNarrative { get; set; }
        public PersonnelVm Officer { get; set; }
        public int PersonalInventoryId { get; set; }
        public int? DeleteBy { get; set; }
        public DateTime? DeleteDate { get; set; }
        public string IdentifierDescription { get; set; }
        public string IdentifierLocation { get; set; }
        public int PhotoTakenBy { get; set; }
        public PersonnelVm TakenByOfficer { get; set; }
        public bool? PhotoGraphPathAbsolute { get; set; }
        public string PhotoGraphPath { get; set; }
    }

    public enum IdentifierType
    {
        FrontView = 1,
        SideView,
        Tattoo,
        Piercing,
        Composite,
        IdMark,
        Property,
        Evidence,
        Other,
        Incident,
    }

    public class PersonCharVm
    {
        public int DescriptionId { get; set; }
        public int? PersonId { get; set; }
        public int? SexLast { get; set; }
        public int? RaceLast { get; set; }
        public int? HairColorLast { get; set; }
        public int? EyeColorLast { get; set; }
        public int? Sex { get; set; }
        public int? Race { get; set; }
        public int? PrimaryHeight { get; set; }
        public int? SecondaryHeight { get; set; }
        public int? Weight { get; set; }
        public int? HairColor { get; set; }
        public int? EyeColor { get; set; }
        public string HairColorName  { get; set; }
        public string EyeColorName  { get; set; }
        public string SexName{ get; set; }
        public string RaceName{ get; set; }
        public int? MaritalStatus { get; set; }
        public string Occupation { get; set; }
        public string Employer { get; set; }
        public int? HairLength { get; set; }
        public int? HairStyle { get; set; }
        public int? HairType { get; set; }
        public int? FacialHair { get; set; }
        public int? Appearance { get; set; }
        public int? Build { get; set; }
        public int? Glasses { get; set; }
        public int? Complexion { get; set; }
        public int? Handed { get; set; }
        public int? Teeth { get; set; }
        public int? Speech { get; set; }
        public string NeckSize { get; set; }
        public string Sexuality { get; set; }
        public int? Clothing { get; set; }
        public string Clothes { get; set; }
        public int? Ethnicity { get; set; }
        public int? ResidentStatus { get; set; }
        public long? PrimaryLanguage { get; set; }
        public string InterpreterNeeded { get; set; }
        public DateTime? CreateDate { get; set; }
        public DateTime? UpdateDate { get; set; }
        public int CreatedBy { get; set; }
        public int? UpdatedBy { get; set; }
        public string CreateByPersonLastName { get; set; }
        public string CreateByPersonFirstName { get; set; }
        public string CreateByPersonMiddleName { get; set; }
        public string CreateByOfficerBadgeNumber { get; set; }
        public string UpdateByPersonLastName { get; set; }
        public string UpdateByPersonFirstName { get; set; }
        public string UpdateByPersonMiddleName { get; set; }
        public string UpdatedByOfficerBadgeNumber { get; set; }
        public string CharHistoryList { get; set; }
         public string PersonFirstName { get; set; }
        public string PersonMiddleName { get; set; }
        public string PersonLastName { get; set; }
        public DateTime? PersonDob { get; set; }
        public string PersonDlNumber {get;set;}
        public string PersonCii {get;set;}
        public string AfisNumber {get;set;}
        public string Placeofbirth {get;set;}
        public string BookingType{get;set;}
        public string Gender{get;set;}

    }

    public class InmateClassificationVm : InmateClassificationDetail
    {
        public string Note { get; set; }
        public int ClassificationOfficerId { get; set; }
        public DateTime? ClassDate { get; set; }
        public string ReviewDate { get; set; }
        public int ClassificationNarrativeId { get; set; }
        public string ClassificationNarration { get; set; }
        public string NoteType { get; set; }
		public virtual Form FormTemplate { get; set; }
        public virtual PersonnelVm ClassificationOfficer { get; set; }
        public virtual PersonnelVm ReviewOfficer { get; set; }
        public virtual FormSaveData ClassifyFormData { get; set; }
        public int ReviewFlag { get; set; }
        public bool IsReview { get; set; }
        public bool isSaveDraft {get;set;}
    }

    public class ClassificationVm
    {
        public IEnumerable<InmateClassificationVm> LstInmateClassification { get; set; }
        public bool ClassificationInitial { get; set; }
    }

    public class InmateNoteVm
    {
        public List<Facility> FacilityList { get; set; }
        public PersonVm PersonInfo { get; set; }
        public List<KeyValuePair<int, string>> LocationList { get; set; }
        public List<KeyValuePair<int, string>> NoteTypeList { get; set; }
    }

    public class InmateTrackDetailsVm
    {
        public PersonVm PersonInfo { get; set; }
        public TrackingFlag TrackingFlag { get; set; }
        public IQueryable<PrivilegeDetailsVm> LocationList { get; set; }
        public List<KeyValuePair<int, string>> RefusalReasonList { get; set; }
        public DateTime? InmateTrackDateOut { get; set; }
        public DateTime? EnrouteStartOut { get; set; }
        public string InmateTrackNote { get; set; }
    }

    public enum TrackingFlag
    {
        Checkout,
        Checkin,
        Move
    }

    public class InmateTrackingVm : InmateTrackingHistroyVm
    {
        public InmateTrackDetailsVm InmateTrackDetailsVm { get; set; }
        public List<IncarcerationDetail> IncarcerationDetails { get; set; }
        public List<InmateTrackingHistroyVm> TrackingHistory { get; set; }
        public ScheduleEventFlag ScheduleFlag { get; set; }
        public int ScheduleId { get; set; }
        public int TrackCount { get; set; }
        public bool IsVisitRefused { get; set; }
    }

    public class FloorNotesVm : InmateVm
    {
        public int FloorNoteId { get; set; }
        public int FloorNoteOfficerId { get; set; }
        public string FloorNoteNarrative { get; set; }
        public string FloorNoteLocation { get; set; }
        public string FloorNoteType { get; set; }
        public int? FloorNoteLocationId { get; set; }
        public DateTime? FloorNoteDate { get; set; }
        public string BadgeNumber { get; set; }
        public int FloorNoteWatchFlag { get; set; }
        public int FloorNoteDeleteFlag { get; set; }
        public PersonnelVm Personnel { get; set; }
        public string FloorNoteTime { get; set; }
    }

    public class FloorNoteTypeCount
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Count { get; set; }
        public string Number { get; set; }
    }

    public class InmateRequestCount
    {
        public int AllType { get; set; }
        public IEnumerable<FloorNoteTypeCount> ListInmateRequestType { get; set; }
    }

    public class VisitationDetails
    {
        public string VisitDay { get; set; }
        public DateTime? VisitFromDate { get; set; }
        public DateTime? VisitToDate { get; set; }
    }

    public class InmateTrackingHistroyVm : InmateVm
    {
        public DateTime? CreateDate { get; set; }
        public bool InmateRefused { get; set; }
        public string InmateRefusedNote { get; set; }
        public string InmateRefusedReason { get; set; }
        public int? InmateTrakLocationId { get; set; }
        public string InmateTrakLocation { get; set; }
        public string InmateTrakConflictNote { get; set; }
        public string InmateTrackNote { get; set; }
        public DateTime? InmateTrakDateOut { get; set; }
        public DateTime? InmateTrakDateIn { get; set; }
        public TrackingFlag TrackingStatus { get; set; }
        public int OutPersonnelId { get; set; }
        public int? InPersonnelId { get; set; }
        public int? SelectedLocationId { get; set; }
        public string SelectedLocation { get; set; }
        public int? EnrouteFinalLocationId { get; set; }
        public string OutPersonLastName { get; set; }
        public string OutOfficerBadgeNumber { get; set; }
        public string InPersonLastName { get; set; }
        public string InOfficerBadgeNumber { get; set; }
        public string PhotoPath { get; set; }
        public int WorkCrewId { get; set; }
        public string HistoryList { get; set; }
        public string CreateByLastName { get; set; }
        public string CreateByFirstName { get; set; }
        public string CreateByOfficerBadgeNumber { get; set; }
        public string UpdateByLastName { get; set; }
        public string UpdateByFirstName { get; set; }
        public string UpdateByOfficerBadgeNumber { get; set; }
        public int VisitorId { get; set; }
        public int HistoryScheduleId { get; set; }
        public string FromHousingUnitLocation { get; set; }
        public string FromHousingUnitNumber { get; set; }
        public string FromLocation { get; set; }
        public string FinalDestination { get; set; }
        public List<TrackingConflictVm> ConflictDetails { get; set; }
        public int InmateTrackId { get; set; }
    }

    public class PrivilegeDetailsVm
    {
        public string PrivilegeDescription { get; set; }
        public string PrivilegeType { get; set; }
        public int PrivilegeId { get; set; }
        public int? TrackingAllowRefusal { get; set; }
        public int? TrackEnrouteFlag { get; set; }
        public DateTime? ExpireDate { get; set; }
        public bool Selected { get; set; }
        public int? FacilityId { get; set; }
        public bool ApptAlertEndDate { get; set; }
        public bool MailPrivilegeFlag { get; set; }
        public bool MailCoverFlag { get; set; }
    }

    public class AppointmentVm
    {
        public int ApptId { get; set; }
        public string ApptLocation { get; set; }
        public int? ApptLocationId { get; set; }
        public int? InmateId { get; set; }
        public int PersonId { get; set; }
        public DateTime? ApptDate { get; set; }
        public string InmateNumber { get; set; }
        public int? ProgramId { get; set; }
        public string ProgramCategory { get; set; }
        public bool VisitType { get; set; }

    }

    public class InmateRequestVm
    {
        public int RequestId { get; set; }
        public int RequestActionLookupId { get; set; }
        public DateTime? Date { get; set; }
        public string Action { get; set; }
        public string Note { get; set; }
        public DateTime? AssignedDate { get; set; }
        public DateTime? ClearedDate { get; set; }
        public string Facility { get; set; }
    }
    public class PhoneDetailsVm
    {
        public int PhoneCallLogId { get; set; }
        public int InmateId { get; set; }
        public DateTime? CallLogDate { get; set; }
        public DateTime? CreateDate { get; set; }
        public DateTime? UpdateDate { get; set; }
        public DateTime? DeleteDate { get; set; }
        public int CreateBy { get; set; }
        public int? UpdateBy { get; set; }
        public int? DeleteBy { get; set; }
        public string CallLogType { get; set; }
        public string PersonLastName { get; set; }
        public string OfficerBadgeNumber { get; set; }
        public int Personneld { get; set; }
        public string CreateByPersonName { get; set; }
        public string UpdateByPersonName { get; set; }
        public string DeleteByPersonName { get; set; }
        public string CreateByOfficerBadgeNumber { get; set; }
        public string UpdateByOfficerBadgeNumber { get; set; }
        public string DeleteByOfficerBadgeNumber { get; set; }
        public string PhoneNumber { get; set; }
        public string ContactName { get; set; }
        public string Duration { get; set; }
        public string Note { get; set; }
        public PhoneLogStatus PhoneLogStatus { get; set; }
        public int DeleteFlag { get; set; }
        public int PhonePinHistoryId { get; set; }
        public string Pin { get; set; }
    }

    public enum PhoneLogStatus
    {
        Insert,
        Update,
        Delete,
        Undo
    }

    public class InmatePhoneHistoryVm : PhoneDetailsVm
    {
        public List<PhoneDetailsVm> LstCallLogDetails { get; set; }
        public List<Lookup> LookUp { get; set; }
        public List<PhoneDetailsVm> LstPinHistory { get; set; }
        public string PersonSsn { get; set; }
        public string CurrentPinId { get; set; }
        public int? PersonSeal { get; set; }
    }

    public class InmateSearchVm
    {
        public int InmateActive { get; set; }
        public bool CheckPhoto { get; set; }
        public string InmateNumber { get; set; }
        public PersonInfoVm PersonDetail { get; set; }
        public string PhotoFilePath { get; set; }
        public int? IncarcerationId { get; set; }
        public int? HousingUnitId { get; set; }
        public int PersonId { get; set; }
        public int InmateId { get; set; }
        public int FacilityId { get; set; }
        public HousingDetail HousingDetail { get; set; }
        public int? LocationId { get; set; }
        public string Location { get; set; }
        public string Classify { get; set; }
        public DateTime? Sentenced { get; set; }
        public string FacilityAbbr { get; set; }
        public string[] SearchText { get; set; }
        public string BookingNo { get; set; }
		public DateTime? FromDate { get; set; }
		public DateTime? ThruDate { get; set; }
		public bool Active { get; set; }
        public int? PersonFlagIndex { get; set; }
        public int? InmateFlagIndex { get; set; }
        public int? DietFlagIndex { get; set; }
        public int? HousingUnitGroupId { get; set; }
        public int? HousingUnitListId { get; set; }
        public bool FacilityInmate { get; set; }
    }

    public class BookingSearchVm
    {
        public int? ArrestId { get; set; }
        public string ArrestBookingNumber { get; set; }
        public int InmateId { get; set; }
        public string InmateNumber { get; set; }
        //  public int PersonId { get; set; }
        public PersonVm PersonDetail { get; set; }
    }

    public class InmateHousingVm
    {
        public InmateCurrentDetails InmateCurrentDetails { get; set; }
        public List<InmateHousingMoveHistory> InmateHousingMoveHistoryList { get; set; }
        public List<HousingIncarcerationHistory> IncarcerationHistoryList { get; set; }
        public List<HousingIncarcerationHistory> IncarcerationFacilityHistoryList { get; set; }

    }
    public class InmateCurrentDetails: FacilityVm
    {
        public String Location { get; set; }
        public int? LocationId { get; set; }        
        public string InmateClassification { get; set; }
        public HousingDetail HousingDetails { get; set; }
        public List<string> HousingFlags { get; set; }
        public PersonInfoVm Person { get; set; }
    }
    public class InmateSupplyDetails
    {
        public int InmateId { get; set; }
        public string ItemDescription { get; set; }
        public int CheckOutCount { get; set; }
        public int? DoNotTransferFlag { get; set; }
    }

    public class InmateSupplyLookupDetails
    {
        public int InmateSupplySizelookupId { get; set; }
        public string ItemDescription { get; set; }
        public int? ShirtFlag { get; set; }
        public int? ShoeFlag { get; set; }
        public int? OtherFlag { get; set; }
        public int? BraFlag { get; set; }
        public int? PantFlag { get; set; }
        public int? UnderwearFlag { get; set; }
        public int? DoNotTransferFlag  { get; set; }
    }

    public class InmateTransferDetails
    {
        public string InmateNumber { get; set; }
        public int InmateId { get; set; }
        public string InmLastName { get; set; }
        public string InmFirstName { get; set; }
        public string InmMiddleName { get; set; }
        public int FacilityId { get; set; }
        public string CurrentTrack { get; set; }
        public int? InmateCurrentTrackId { get; set; }
        public string PersonalInventory { get; set; }
        public int PersonId { get; set; }
        public int IssuedPropertyCnt { get; set; }
        public decimal? SupplyCnt { get; set; }
        public int BookCnt { get; set; }
        public List<KeyValuePair<int?,string>> TransferSupply { get; set; }
        public List<KeyValuePair<int?,string>> NoTransferSupply { get; set; }
        public int Count { get; set; }
        public int LateCount { get; set; }
        public DateTime ScheduleTime { get; set; }
        public string ScheduleLocation { get; set; }
        public int ScheduleLocationId { get; set; }
        public int ScheduleId { get; set; }
        public int InmateTrakId { get; set; }
        public HousingDetail Housing { get; set;}
        public string ConflictNote { get; set; }
        public bool IsIncompleteTasks { get; set; }
    }
}
