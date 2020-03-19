using System;
using System.Collections.Generic;

namespace ServerAPI.ViewModels
{
    public class BookingVm : Expedite
    {
        public DateTime? DateIn { get; set; }
        public int IncarcerationId { get; set; }
        public int InmateId { get; set; }
        public InmateHousing PersonDetails { get; set; }
        public bool IntakeCompleteFlag { get; set; }
        public bool BookReleaseFlag { get; set; }
        public string OriginalBookingNumber { get; set; }
        public string[] CaseType { get; set; }
        public string PreBookNumber { get; set; }
        public int? BookingWizardLastStepId { get; set; }
        public bool BookCompleteFlag { get; set; }
        public List<WizardStep> LastStep { get; set; }
        public int? WizardProgressId { get; set; }
        public List<TasksCountVm> BookingTasks { get; set; }
        public bool NoKeeper { get; set; }
    }

    //public class BookingSupervisorVm : BookingVm
    //{
    //    public int? ArrestId { get; set; }
    //    public bool AssessmentCompleteFlag { get; set; }
    //    public bool BookingSupervisorCompleteFlag { get; set; }
    //    public bool ReleaseClearFlag { get; set; }
    //    public bool ReleaseSupervisorCompleteFlag { get; set; }
    //    public DateTime? OverallFinalReleaseDate { get; set; }
    //}

    public class WizardStep
    {
        public int WizardProgressId { get; set; }
        public int WizardStepProgressId { get; set; }
        public int ComponentId { get; set; }
        public bool StepComplete { get; set; }
        public int? AoWizardFacilityStepId { get; set; }
    }

    public class BookingNoteDetailsVm
    {
        public DateTime? NoteDate { get; set; }
        public string NoteType { get; set; }
        public string NoteText { get; set; }
        public string Personlastname { get; set; }
        public string Officerbadgenumber { get; set; }
        public int DeleteFlag { get; set; }
        public int ArrestNoteId { get; set; }
        public int ArrestId { get; set; }
    }


    public class BookingNoteVm
    {
        public List<BookingNoteDetailsVm> ListBookingNoteDetails { get; set; }
        public List<KeyValuePair<string, int>> ListBookingNoteCount { get; set; }
    }

    public class BookNoteEntryVm
    {
        public List<BookingDetails> ListNoteEntryDetails { get; set; }
        public List<KeyValuePair<int, string>> ListNoteType { get; set; }
    }

    public class BookingClearVm
    {
        public string BookingNo { get; set; }
        public string BookingType { get; set; }
        public string CourtDocket { get; set; }
        public DateTime? ReleaseDate { get; set; }
        public string ClearReason { get; set; }
        public string ClearNotes { get; set; }
        public DateTime? ClearDate { get; set; }
        public int? Personnel { get; set; }
        public DateTime? ReleaseOut { get; set; }
        public int? ArrestSentenceCode { get; set; }
        public int? BailNoBailFlag { get; set; }
        public int? ActualDays { get; set; }
        public int? ArrestId { get; set; }
        public PersonnelVm ReleaseOfficer { get; set; }
        public int ArrestType { get; set; }
        public decimal? BailAmount { get; set; }
        public int ReleaseOfficerId { get; set; }
        public int? IncarcerationId { get; set; }
        public string ArrestHistoryList { get; set; }
        public int IncarcerationArrestXrefId { get; set; }
        public int PersonId { get; set; }
        public List<SentenceVm> Sentence { get; set; }
    }

    public class ClearHistoryVm
    {
        public int ArrestClearHistoryId { get; set; }
        public int ArrestId { get; set; }
        public int PersonnelId { get; set; }
        public DateTime? CreateDate { get; set; }
        public PersonnelVm ReleaseOfficer { get; set; }
    }

    public class BookingClearHeader
    {
        public string Header { get; set; }
        public string Detail { get; set; }
        public int ArrestClearHistoryValueId { get; set; }
    }

    public class WarningClearVm
    {
        public string Warning { get; set; }
        public string Warning1 { get; set; }
        public string Warning2 { get; set; }
        public int RecordOrder { get; set; }
    }

    public class ClearChargesVm
    {
        public int? ArrestId { get; set; }
        public int? Count { get; set; }
        public string Type { get; set; }
        public string Qualifier { get; set; }
        public string Section { get; set; }
        public string Description { get; set; }
        public string Statute { get; set; }
        public decimal? Bail { get; set; }
        public string Status { get; set; }
        public string Note { get; set; }
        public string WarrantNumber { get; set; }
        public int? CrimeNumber { get; set; }
        public DateTime? StartDate { get; set; }
        public int? ArrestSentenceConsecutiveFlag { get; set; }
        public DateTime? UseStartDate { get; set; }
        public int? ArrestSentenceDays { get; set; }
        public string Method { get; set; }
        public int? ArrestSentenceDaysToServe { get; set; }
        public int? ArrestSentenceActualDaysToServe { get; set; }
        public DateTime? ClearDate { get; set; }
        public int CrimeId { get; set; }
        public string ArrestBookingNumber { get; set; }
        public int? ArrestSentenceMethodId { get; set; }
    }

    public class ClearBailVm
    {
        public string ReceiptNumber { get; set; }
        public string BailCompanyName { get; set; }
        public int? BailCompanyId { get; set; }
        public string PostedBy { get; set; }
        public decimal AmountPosted { get; set; }
        public string PaymentType { get; set; }
        public string PaymentNumber { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int BailAgentId { get; set; }
        public PersonnelVm CreatedBy { get; set; }
        public PersonVm BailPerson { get; set; }
        public int BailTransactionId { get; set; }
    }

    public class SentenceVm
    {
        public int? IncarcerationId { get; set; }
        public int? ArrestId { get; set; }
        public int DisplayOrder { get; set; }
        public int? ArrestSentenceGroup { get; set; }
        public string ArrestBookingNo { get; set; }
        public string Type { get; set; }
        public string ArrestCourtDocket { get; set; }
        public Boolean ArrestSentenceConsecutiveFlag { get; set; }
        public DateTime? ArrestSentenceStartDate { get; set; }
        public DateTime? ArrestSentenceUseStartDate { get; set; }
        public int? ArrestSentenceDays { get; set; }
        public string MethodName { get; set; }
        public int? ArrestSentenceDaysToServe { get; set; }
        public int? ArrestSentenceActualDaysToServe { get; set; }
        public DateTime? ArrestSentenceReleaseDate { get; set; }
        public string ArrestSentenceDescription { get; set; }
        public DateTime? ArrestSentenceExpirationDate { get; set; }
        public int? ArrestSentenceCode { get; set; }
        public int? ArrestSentenceConsecutiveTo { get; set; }
        public string ArrestBookingNo1 { get; set; }
        public int? WeekEnder { get; set; }
        public string Abbr { get; set; }
        public DateTime? ArrestClearScheduleDate { get; set; }
        public string ReleaseReason { get; set; }
        public int? ArrestSentenceDaysAmount { get; set; }
        //public List<ChargeSentenceVm> ChargeSentences { get; set; }
        public SentenceGapFound SentenceGapFound { get; set; }
        public int IncarcerationArrestXrefId { get; set; }
        public bool ArrestSentenceManual { get; set; }
        public bool ArrestSentenceForthwith { get; set; }
        public bool ArrestSentenceIndefiniteHold { get; set; }
        public int? ArrestSupSeqNumber { get; set; }
        public int? ArrestSentenceDaysStayed { get; set; }
        public int? ArrestTimeServedDays { get; set; }
        public string ArrestSentenceDaysStayedInterval { get; set; }
        public int? ArrestSentenceDaysStayedAmount { get; set; }
        public DateTime? ArrestDate { get; set; }
        public bool? ArrestSentenceStopDaysFlag { get; set; }
    }

    public class BookingCountDetails
    {
        public int? IncarcerationId { get; set; }
        public int? SentenceCode { get; set; }
        public DateTime? SentenceReleaseDate { get; set; }
        public DateTime? SentenceClearDate { get; set; }
        public int? SentenceIndefiniteHold { get; set; }
        public DateTime? ReleaseDate { get; set; }
        public string ActiveBookingHold { get; set; }
        public int? SentenceWeekender { get; set; }
    }

    public class BookClearVm
    {
		public PersonVm PersonDetails { get; set; }
		public PersonCharVm PersonCharDetails { get; set; }
		public List<AkaVm> AkaDetailLst { get; set; }

		public List<KeyValuePair<int, string>> ReleaseReason { get; set; }	
		public List<WarningClearVm> WarningClearList { get; set; }
        public List<ClearChargesVm> Chargeslist { get; set; }
        public List<ClearBailVm> ClearBaillist { get; set; }
        //public List<SentenceVm> Sentencelist { get; set; }
        public List<ClearWarrant> OverallWarrant { get; set; }
        public SentenceViewerVm SentenceViewer { get; set; }
    }

    public class OverallSentvm
    {
        public Overall Overall { get; set; }
        public List<BookingCountDetails> BookingCountDetails { get; set; }
    }

    public class Overall
    {
        public string ArrestBookingNo { get; set; }
        public DateTime? ReleaseDate { get; set; }
        public int? TotalSentDays { get; set; }
        public int SentCount { get; set; }
        public int UnSentCount { get; set; }
    }

    public class CurrentStatus
    {
        public DateTime? OverAllSentStartDate { get; set; }
        public DateTime? OverAllSentReleaseDate { get; set; }
        public int? ActualDaysToServe { get; set; }
        public int? OverAllSentManual { get; set; }
        public int? ManualOverride { get; set; }
        public int? OverAllSentErc { get; set; }
        public int? OverAllSentErcClear { get; set; }
    }
    public enum SentenceCode
    {
        Cleared = -1,
        Unsentenced,
        Sentenced,
        AltSent,
        Hold = 4
    }


    public class OverallIncarceration : OverallInOut
    {
        public List<KeyValuePair<string, string>> RealeaseCondtions { get; set; }
        public List<KeyValuePair<string, string>> ConditionRelease { get; set; }
        public List<OverallWizard> ListOverallWizard { get; set; }
        public OverallUserControls OverallUserControls { get; set; }
    }

    public class OverallInOut : Expedite
    {
        public DateTime? DateIn { get; set; }
        public DateTime? DateOut { get; set; }
        public int InmateActive { get; set; }
        public string ChargeType { get; set; }
        public bool ChargeFlag { get; set; }
        public string FacilityAbbr { get; set; }
        public decimal? BailAmountTotal { get; set; }
        public int? BailNoBailFlagTotal { get; set; }
        public DateTime? OverallSentStartDate { get; set; }
        public DateTime? OverallFinalReleaseDate { get; set; }
        public string OverallConditionOfRelease { get; set; }
        public bool BookAndReleaseFlag { get; set; }
        public int? IncarcerationId { get; set; }
        public string ReceiveMethod { get; set; }
        public bool IntakeCompleteFlag { get; set; }
    }

    public class OverallWizard
    {
        public string WizardName { get; set; }
        public DateTime? CompleteDate { get; set; }
        public PersonnelVm Personnel { get; set; }
        public int Order { get; set; }
    }

    public class OverallUserControls
    {
        public string FieldName { get; set; }
        public int? FieldRequired { get; set; }
        public int? FieldVisible { get; set; }
    }

    public class ClearWarrant
    {
        public int? WarrantId { get; set; }
        public int PersonId { get; set; }
        public int? ArrestId { get; set; }
        public string WarrantNumber { get; set; }
        public string WarrantType { get; set; }
        public DateTime? WarrantDate { get; set; }
        public string WarrantCountry { get; set; }
        public string WarrantDescription { get; set; }
        public string WarrantBailType { get; set; }
        public decimal? WarrantBailAmount { get; set; }
        public string WarrantChargeType { get; set; }
        public DateTime? CreatedDate { get; set; }
        public List<CrimeGroup> CrimeGroups { get; set; }
    }

    public class CrimeGroup
    {
        public int? CrimeNumber { get; set; }
        public int CrimeId { get; set; }
        public int CrimeLookUpId { get; set; }
        public string CrimeStatus { get; set; }
        public int? Count { get; set; }
        public int DeleteFlag { get; set; }
        public string CodeType { get; set; }
        public string CrimeSection { get; set; }
        public string CrimeDescription { get; set; }
        public string StatuteCode { get; set; }
        public decimal? BailAmount { get; set; }
        public int? BailNoFlag { get; set; }
        public string Status { get; set; }
        public string StatusId { get; set; }
        public string CrimeNotes { get; set; }
        public int CrimeForceId { get; set; }
        public string CrimeGroups { get; set; }
        public string Qualifier { get; set; }
        public string QualifierId { get; set; }
        public DateTime? CreatedDate { get; set; }

    }

    public enum Order
    {
        INTAKECOMPLETED = 1,
        BOOKINGCOMPLETED,
        SUPERVISORCOMPLETED,
        CLEARCOMPLETED,
        RELEASECOMPLETED
    }

    public class ProgramTypes
    {
        public int AltSentId { get; set; }
        public DateTime? AltSentStart { get; set; }
        public int? IncarcerationId { get; set; }
        public string FacilityAbbr { get; set; }
        public string AltSentProgramAbbr { get; set; }
        public DateTime? AltSentThru { get; set; }
        public int AltSentProgramId { get; set; }
        public int? AltSentTotalAttend { get; set; }
        public string AltSentNote { get; set; }
    }

    public class OercMethod
    {
        public int ArrestSentenceMethodOERCid { get; set; }
        public int OERCVisible { get; set; }
        public int OERCCredit { get; set; }
        public int OERCDaysRange { get; set; }
        public int OERCDaysRangeUseMaxDTS { get; set; }
        public int OERCDaysRangeUseMaxD { get; set; }

    }

    public class OverallSentence : EventVm
    {
        public int? SentManual { get; set; }
        public DateTime? SentManualDate { get; set; }
        public int? SentManualby { get; set; }
        public DateTime? SentStartDate { get; set; }
        public DateTime? FinalReleaseDate { get; set; }
        public int? DaysToServe { get; set; }
        public int SentERC { get; set; }
        public int SentERCClear { get; set; }
        public int? TotSentDays { get; set; }
        public int IncarcerationId { get; set; }
        public int AltSentId { get; set; }
        public int FacilityId { get; set; }
    }

    public class UndoClearBook
    {
        public int ArrestId { get; set; }
        public int IncarcerationArrestXrefId { get; set; }
        public int IncarcerationId { get; set; }
        public int PersonId { get; set; }
        public string HistoryList { get; set; }
    }
    public class BookingActive
    {
        public int InmateId { get; set; }
        public int ArrestId { get; set; }
        public int FacilityId { get; set; }
        public string FacilityAbbr { get; set; }
        public PersonInfoVm PersonDetails { get; set; }
        public DateTime? DOB { get; set; }
        public string InmateNumber { get; set; }
        public string PhotoFilePath { get; set; }
        public int InmateActive { get; set; }
        public HousingUnitVm HousingUnit { get; set; }
        public int PrebookFlag { get; set; }
        public int IntakeFlag { get; set; }
        public int BookCompleteFlag { get; set; }
        public int HouseFlag { get; set; }
        public string TrackNotesLocation { get; set; }
        public string InmateCurrentTrack { get; set; }
        public string CurrentTrackLocation { get; set; }
        public int ClassFlag { get; set; }
        public DateTime? IncarcerationDate { get; set; }
        public int Elapsed { get; set; }
        public DateTime? SchedRelOld { get; set; }
        public DateTime? SchedRel { get; set; }
        public int SentFlag { get; set; }
        public string Classify { get; set; }
        public DateTime? OverallFinalRelDate { get; set; }
        public int? ActualDaystoServe { get; set; }
        public  string BookingNumber { get; set; }
    }

    public enum BookingActiveStatus
    {
        All = 1,
        IntakeOnly = 2,
        BookingOnly = 3,
        NoClassify = 4,
        NoHousing = 5,
        NoSent = 6,
        SentOnly = 7
    }

    public class BookingPrebook
    {
        public string PrebookNumber { get; set; }
        public List<LoadSavedForms> FormTemplates { get; set; }
        public InmatePrebookVm InmatePrebook { get; set; }
        public List<PrebookCharge> PrebookCharges { get; set; }
        public List<InmatePrebookWarrantVm> PrebookWarrant { get; set; }
        public List<PersonalInventoryVm> PrebookProperty { get; set; }
        public List<PrebookAttachment> PrebookAttachment { get; set; }

    }
}
