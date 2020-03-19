using System;
using System.Collections.Generic;

namespace ServerAPI.ViewModels
{
    public class CourtCommitHistoryVm
    {
        public int FacilityId { get; set; }
        public int InmatePrebookId { get; set; }
        public PersonInfo InmateName { get; set; }
        public DateTime? PrebookDate { get; set; }
        public string Elapsed { get; set; }
        public PersonnelVm Officer { get; set; }
        public int PersonId { get; set; }
        public bool CompleteFlag { get; set; }
        public string Disposition { get; set; }
        public int? WizardLastStepId { get; set; }
        public int? InmateId { get; set; }
        public int PersonnelId { get; set; }
        public bool DeletedFlag { get; set; }
        public DateTime? Dob { get; set; }
        public int FormCount { get; set; }
        public DateTime? BookedDate { get; set; }
        public DateTime? SchedBookDate { get; set; }
        public int ArrestingOfficerId { get; set; }
        public int MedPrescreenStartFlag { get; set; }
        public int MedPrescreenStatusFlag { get; set; }
        public string DocketNumber { get; set; }
        public string PreBookNotes { get; set; }
        public string FacilityName { get; set; }
        public string AgencyAbbreviation { get; set; }
        public string CaseNumber { get; set; }
        public string PrebookNumber { get; set; }
        public DateTime? ArresstDate { get; set; }
        public DateTime? OverAllFinalReleaseDate { get; set; }
        public int ArrestId { get; set; }
        public int? IncarcerationId { get; set; }
        public int? AoWizardProgressId { get; set; }
        public List<WizardStep> WizardProgress { get; set; }
        public List<LookupVm> CommitType { get; set; }
        public bool TempHold { get; set; }
        public bool InmateActive { get; set; }

        public int InmatePrebookCaseId { get; set; }

        // below property will be used in AltSent -> In/Out -> Interview
        public DateTime? SchedInterviewDate { get; set; }
        public bool InterviewCompleteFlag { get; set; }
        public string CourtCommitStatus { get; set; }
        public string CourtCommitDenyReason { get; set; }
        public List<EnrollmentFeesVm> EnrollmentFeesVms { get; set; }
    }

    public class CourtCommitHistorySearchVm
    {
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int FacilityId { get; set; }
        public bool ActiveFlag { get; set; }
        public int OfficerId { get; set; }
        public bool CompleteFlag { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public DateTime? Dob { get; set; }
        public DateTime? ScheduleDate { get; set; }
        public bool DeleteFlag { get; set; }
        public DateTime? InterviewFromDate { get; set; }
        public DateTime? InterviewToDate { get; set; }
        public int InterviewOfficerId { get; set; }
        public string CommitStatus { get; set; }
        public string DenyReason { get; set; }
        public string CaseNumber { get; set; }
        public bool ActiveInmate { get; set; }

        public string CommitType { get; set; }

        //below property is used to identify whether altsent -> interview or schedule booking
        public AltSentInOutType AltSentInOutType { get; set; }
    }

    public class CourtCommitBookingVm
    {
        public int ArrestId { get; set; }
        public string ArrestBookingNo { get; set; }
        public string ArrestCourtDocket { get; set; }
    }

    public class AgencyDetailsVm
    {
        public int AgencyId { get; set; }
        public string AgencyName { get; set; }
        public bool AgencyCourtFlag { get; set; }
    }

    public class CourtCommitSentEntryVm
    {
        public string ArrestCourtDocket { get; set; }
        public int? ArrestCourtJurisdictionId { get; set; }
        public double? ArrestType { get; set; }
        public string ArrestSentenceDescription { get; set; }
        public Boolean ArrestSentenceAmended { get; set; }
        public Boolean ArrestSentencePenalInstitution { get; set; }
        public Boolean ArrestSentenceOptionsRec { get; set; }
        public Boolean ArrestSentenceAltSentNotAllowed { get; set; }
        public Boolean ArrestSentenceNoEarlyRelease { get; set; }
        public Boolean ArrestSentenceNoLocalParole { get; set; }
        public DateTime? ArrestSentenceDateInfo { get; set; }
        public string ArrestsentenceType { get; set; }
        public string ArrestSentenceFindings { get; set; }
        public int? ArrestSentenceJudgeId { get; set; }
        public Boolean ArrestSentenceConsecutiveFlag { get; set; }
        public DateTime? ArrestSentenceStartDate { get; set; }
        public string ArrestSentenceDaysInterval { get; set; }
        public int? ArrestSentenceDaysAmount { get; set; }
        public int? ArrestSentenceFineDays { get; set; }
        public decimal? ArrestSentenceFineAmount { get; set; }
        public decimal? ArrestSentenceFinePaid { get; set; }
        public string ArrestSentenceFineType { get; set; }
        public decimal? ArrestSentenceFinePerDay { get; set; }
        public int? ArrestSentenceDaysStayed { get; set; }
        public int? ArrestTimeServedDays { get; set; }
        public bool ArrestSentenceForthwith { get; set; }
        public string Court { get; set; }
        public double? ArrestSentenceFineTypeid { get; set; }
        public string CourtCommitType { get; set; }
    }

    public class CourtCommitAgencyVm
    {
        public List<AgencyDetailsVm> AgencyDetails { get; set; }
        public List<PersonnelVm> PersonnelDetails { get; set; }
        public List<CourtCommitBookingVm> CourtCommitBooking { get; set; }
        public List<LookupVm> LookupDetails { get; set; }
        public PersonVm InmateDetails { get; set; }
        public List<CourtCommitSentEntryVm> PrebookDetails { get; set; }
    }

    public class CourtCommitPersonnelConstants
    {
        public const string PERSONNELJUDGEFLAG = "1";
    }

    public enum AltSentInOutType
    {
        Interview,
        SchedBook
    }

    public class EnrollmentFeesVm
    {
        public int Id { get; set; }
        public string Receipt { get; set; }
        public DateTime? TransactionDate { get; set; }
        public decimal? Amount { get; set; }
        public decimal? Balance { get; set; }
        public string Description { get; set; }
        public string ReceiveFrom { get; set; }
        public int VoidId { get; set; }
        public string VoidNote { get; set; }
        public PersonnelVm Officer { get; set; }
    }
}