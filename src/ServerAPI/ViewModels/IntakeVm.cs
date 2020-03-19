using System;
using System.Collections.Generic;

namespace ServerAPI.ViewModels
{
	//using PersonVm getintake takes 1800ms to 2300ms
	//using PersonInfoVm getintake takes 854ms to 930ms
	public class IntakeVm : PersonInfoVm
	{
		public int IncarcerationId { get; set; }
		public int ArrestId { get; set; }
		public string PrebookNumber { get; set; }
		public DateTime? DateIn { get; set; }
		public int BookandReleaseFlag { get; set; }
		public int? LocationId { get; set; }
		public int PreBookId { get; set; }
		public AoWizardProgressVm WizardProgress { get; set; }
		//public int? LastStepId { get; set; }
		//public WizardStepVm LastStep { get; set; }
		public int PreBookCount { get; set; }
		public WizardType WizardType { get; set; }
	    public List<TasksCountVm> BookingTasks { get; set; }
        public string BookingNo { get; set; }
	    public bool ExpediteBookingFlag { get; set; }
	    public string ExpediteBookingReason { get; set; }
	    public string ExpediteBookingNote { get; set; }
	    public DateTime? ExpediteDate { get; set; }
        public PersonnelVm ExpediteBy { get; set; }
	    public string[] CaseType { get; set; }
    }

    public class IntakeInmate
    {
        public List<IntakeVm> IntakeDetails { get; set; }
        public List<TasksCountVm> TaskDetails { get; set; }
    }

	public class IntakeInmatePrebookVm
	{
		public int InmatePrebookId { get; set; }
		public int? IncarcerationId { get; set; }
		public int? PersonId { get; set; }
		public string PreBookNumber { get; set; }
	}
	public enum WizardType
	{
		None,
		Intake,
		Booking,
		Release,
		BookandRelease,
		TempHold,
		Registrant,
		BookingOverall,
		ReleaseOverall,
		SupervisorReview,
		SupervisorRelease
	}

    //public class WizardDetails
    //{
    //    public string PrebookNumber { get; set; }
    //    public string InmateNumber { get; set; }
    //    public string BookingNumber { get; set; }
    //    public int BookingCount { get; set; }

    //}

    //public class WizardCompleteVm
    //{
    //    public List<KeyValuePair<int, string>> RadioCompleteList { get; set; }
    //    public bool CompleteFlag { get; set; }
    //    public int NextStep { get; set; }
    //    public List<RequiredFields> RequiredFields { get; set; }
    //    public string ArrestType { get; set; }
    //    public string ChargeRequireSiteOption { get; set; }
    //    public string WarrantRequireSiteOption { get; set; }
    //    public string OnlyOneWarrantSiteOption { get; set; }
    //    public int CrimeCount { get; set; }
    //    public int CrimeForceCount { get; set; }
    //    public int WarrantCount { get; set; }
    //    public bool BookingCompleteFlag { get; set; }
    //}

    public class IntakeEntryVm : PersonVm
    {
        //public int InmatePrebookId { get; set; }
        public DateTime? ArrestDate { get; set; }
        public int ReceivingOfficerId { get; set; }
        public PersonnelVm ReceivingOfficer { get; set; }
        public int ArrestOfficerId { get; set; }
        public PersonnelVm ArrestingOfficer { get; set; }
        public string ArrestOfficerName { get; set; }
        public int? ReactivateArrestId { get; set; }
        public int? TransportOfficerId { get; set; }
        public string TransportOfficerLastName { get; set; }
        public string TransportOfficerFirstName { get; set; }
        public string TransportOfficerNumber { get; set; }
        public string TransportOfficerName { get; set; }
        public int? SearchOfficerId { get; set; }
        public PersonnelVm SearchOfficer { get; set; }
        public string BookingType { get; set; }
        public string ReceiveMethod { get; set; }
        public bool BookAndRelease { get; set; }
        public string ReactivateBookingFlag { get; set; }
        public int AltSentFacilityId { get; set; }
        public int ArrestAgencyId { get; set; }
        public int BookingAgencyId { get; set; }
        public string ArrestCaseNumber { get; set; }
        public string ArrestLocation { get; set; }
        public int? ArrestBookingOfficerId { get; set; }
        public int ReactivateSiteOptions { get; set; }
        public int ActiveBooking { get; set; }
        public string OriginalBookingNumber { get; set; }
        public DateTime? OverallFinalReleaseDate { get; set; }
        public bool IsSupplementalFlag { get; set; }
        public bool IsActiveIncarceration { get; set; }
        public bool IsCourtCommit { get; set; }
        public string ArrestCourtDocket { get; set; }
        public int? ArrestCourtJurisdictionId { get; set; }

        public bool ExpediteBookingFlag { get; set; }
        public string ExpediteBookingReason { get; set; }
        public string ExpediteBookingNote { get; set; }
        public string ContactNumber { get; set; }
    }

    public class IntakeTempHoldVm
    {
        public List<PersonnelSearchVm> LstPersonnel { get; set; }
        public List<KeyValuePair<int, string>> LstTempHoldType { get; set; }
        public List<KeyValuePair<int, string>> LstTempHoldLocation { get; set; }
    }

    public class IntakeTempHoldParam
    {
        public int InmatePrebookId { get; set; }
        public int FacilityId { get; set; }
        public int ReceivingOfficerId { get; set; }
        public int TempHoldTypeId { get; set; }
        public int? TempHoldLocationId { get; set; }
        public string TempHoldLocation { get; set; }
        public string TempHoldNote { get; set; }
        public int CreateBy { get; set; }
    }

	public class TempHoldVm
	{
		public List<TempHoldDetailsVm> TempHoldDetails { get; set; }
		public List<RequestTypes> TempHoldTypeCnt { get; set; }
		public List<RequestTypes> DispositionCnt { get; set; }

	}

	public class TempHoldDetailsVm
	{
		public int TempHoldId { get; set; }
		public int PrebookId { get; set; }
		public int PersonId { get; set; }
        public int FacilityId { get; set; }
		public int PersonnelId { get; set; }
		public int OfficerId { get; set; }
		public string Location { get; set; }
		public int LocationId { get; set; }
		public int Disposition { get; set; }
		public int TempHoldType { get; set; }
		public int FormCount { get; set; }
		public string TempHoldTypeDesc { get; set; }
		public string DispositionDesc { get; set; }
		public string TempHoldNote { get; set; }
		public string TempHoldCompleteNote { get; set; }
		public int? CompleteFlag { get; set; }
		public int CompleteBy { get; set; }
		public int WizardLastStepId { get; set; }
		public bool IsDeleted { get; set; }
		public bool IsActive { get; set; }
		public PersonDetailVM PersonDetails { get; set; }
		public PersonDetailVM OfficerDetails { get; set; }
		public PersonDetailVM CompletedByDetails { get; set; }
		public AoWizardProgressVm TempHoldProgress { get; set; }
		public List<AoWizardFacilityStepVm> WizardFacilitySteps { get; set; }
		public DateTime? FromDate { get; set; }
		public DateTime? ToDate { get; set; }
		public DateTime? TempHoldDateIn { get; set; }
		public DateTime? TempHoldCompleteDate { get; set; }
	}

    public class TempHoldCompleteStepLookup
    {
        public List<KeyValuePair<int, string>> ListType { get; set; }
        public List<KeyValuePair<int, string>> ListLocation { get; set; }
        public List<KeyValuePair<int, string>> ListDisposition { get; set; }
        public PrebookCompleteVm PrebookComplete { get; set; }
    }

    public class PrebookCompleteVm
    {
        public int TempHoldId { get; set; }
        public int? TempHoldTypeId { get; set; }
        public int? TempHoldLocationId { get; set; }
        public string TempHoldLocation { get; set; }
        public string TempHoldNote { get; set; }
        public int? TempHoldCompleteFlag { get; set; }
        public int? TempHoldDisposition { get; set; }
        public string TempHoldCompleteNote { get; set; }
        public DateTime? TempHoldCompleteDate { get; set; }
        public int? TempHoldCompleteBy { get; set; }
        public string TempHoldCompleteByLast { get; set; }
        public string TempHoldCompleteByFirst { get; set; }
        public string TempHoldCompleteByBadgeNumber { get; set; }
        public bool BookingRequired { get; set; }
        public int InmatePrebookId { get; set; }
    }

    public class InmatePrebookStagingVm
    {
        public int InmatePrebookStagingId { get; set; }
        public string CaseNumber { get; set; }
        public DateTime CreateDate { get; set; }
        public string StateNumber { get; set; }
        public string InmateNumber { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string Gender { get; set; }
        public string DOB { get; set; }
        public string RawData { get; set; }
        //below property will be used to filter
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string DeleteReason { get; set; }
        public bool DeleteFlag { get; set; }
        public InmatePrebookStagingAlert SelectFlag { get; set; }
        public int? WizardProgressId { set; get; }
        public List<WizardStep> LastStep { get; set; }
         public int? InmatePrebookId { set; get; }
    }

    public enum InmatePrebookStagingReason
    {
        Replaced,
        Consumed
    }

    public enum InmatePrebookStagingAlert
    {
        Active,
        Deleted,
        Consumed
    }
}
