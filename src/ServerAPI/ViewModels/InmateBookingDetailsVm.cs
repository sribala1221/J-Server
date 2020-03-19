using System;
using System.Collections.Generic;

namespace ServerAPI.ViewModels
{
    public class CompleteFlagDetails
    {
        public DateTime? CompleteDate { get; set; }
        public string PersonLastName { get; set; }
        public string OfficerBadgeNumber { get; set; }
    }

    public class ActiveCommitDetails
    {
        public string PreBookNumber { get; set; }
        public string PreBookLastName { get; set; }
        public string PreBookFirstName { get; set; }
        public string PreBookMiddleName { get; set; }
        public DateTime? PreBookDob { get; set; }
        public string PersonLastName { get; set; }
        public string PersonFirstName { get; set; }
        public string PersonMiddleName { get; set; }
        public DateTime? PersonDob { get; set; }
        public string CaseNumber { get; set; }
        public DateTime? ArrestDate { get; set; }
        public DateTime? PreBookDate { get; set; }
        public string ArrestAgency { get; set; }
        public string Facility { get; set; }
        public string Notes { get; set; }
        public int? DeleteFlag { get; set; }
        public int PreBookId { get; set; }
        public int? PersonId { get; set; }
        public int? CourtCommitFlag { get; set; }
        public int? InmateId { get; set; }
        public PersonnelVm ArrestingOfficer { get; set; }
        public bool SiteOptions { get; set; }
    }

    public class BailDetails
    {
        public decimal? BailAmount { get; set; }
        public string BailType { get; set; }
        public bool BailFlag { get; set; }
        public int ArrestId { get; set; }
        public decimal? CalculatedBailAmount { get; set; }
        public string BailNote { get; set; }
        public bool IsforSentencing {get;set;}
    }

    public class InmateBookingDetailsVm
    {
        public List<BookingSearchSubData> ListChargesDetails { get; set; }
        public CompleteFlagDetails IntakeCompleteFlag { get; set; }
        public CompleteFlagDetails BookingCompleteFlag { get; set; }
        public CompleteFlagDetails SupervisorCompleteFlag { get; set; }
        public CompleteFlagDetails ReleaseCompleteFlag { get; set; }
        public List<InmateIncarcerationDetails> BookingInmateDetails { get; set; }
        public int SiteOptions { get; set; }
        public List<int> ArrestIds { get; set; }
        public List<WizardLastStepDetails> IntakeWizardLastStep { get; set; }
        public List<WizardLastStepDetails> BookingWizardLastStep { get; set; }
        public List<WizardLastStepDetails> BookingDataWizardLastStep { get; set; }
        public List<WizardLastStepDetails> BookingReleaseWizardLastStep { get; set; }
        public List<WizardLastStepDetails> SupervisorWizardLastStep { get; set; }
        public List<WizardLastStepDetails> ReleaseWizardLastStep { get; set; }
        public int ActiveCommitDetailsCnt { get; set; }
        public int?[] ReactivateArrestId { get; set; }
    }

    public class WizardLastStepDetails
    {
        public int IncarcerationId { get; set; }
        public int? WizardCompleteFlag { get; set; }
        public int? LastStepId { get; set; }
        public int? BookandReleaseFlag { get; set; }
        public int PreBookId { get; set; }
        public int ArrestId { get; set; }
        public int? WizardProgressId { get; set; }
        public List<WizardStep> LastStep { get; set; }
    }

    public class InmateBookingData
    {
        public int ArrestId { get; set; }
        public int IncarcerationId { get; set; }
        public int IncarcerationArrestXrefId { get; set; }
        public int PersonId { get; set; }
        public bool DoNotDoSaveHistory { get; set; }
        public bool BailTransaction { get; set; }
        public int InmateId { get; set; }
        public int FacilityId { get; set; }
        public bool BookingCompleteFlag { get; set; }
        public bool CompleteFlag { get; set; }
    }

    public enum CrimeCode
    {
        F = 1,
        M,
        X,
        C,
        I,
        O
    }

    public class BailTransactionDetails : BailTransactionVm
    {
        public string BailCompanyName { get; set; }
        public PersonnelVm CreatedPerson { get; set; }
        public BailAgentVm BailAgent { get; set; }
        public Form FormDetails { get; set; }
    }

    public class BookingBailDetails
    {
        public List<BailTransactionDetails> BailTransactionDetails { get; set; }
        public BailDetails BailDetails { get; set; }
    }

    public class BailAgentVm
    {
        public int BailAgentId { get; set; }
        public string BailAgentLastName { get; set; }
        public string BailAgentFirstName { get; set; }
        public string BailAgentMiddleName { get; set; }
        public string BailAgentLicenseNum { get; set; }
        public DateTime? BailAgentLicenseExpire { get; set; }
        public string BailAgentHistoryList { get; set; }
        public List<KeyValuePair<int, bool>> BailCompanyIds { get; set; }
    }

    public class BailCompanyVm
    {
        public int BailCompanyId { get; set; }
        public string BailCompanyName { get; set; }
        public decimal? BailCompanyBondLimit { get; set; }
        public string BailCompanyAddressNum { get; set; }
        public string BailCompanyStreetName { get; set; }
        public string BailCompanyAddress { get; set; }
        public string BailCompanyCity { get; set; }
        public string BailCompanyState { get; set; }
        public string BailCompanyPhone { get; set; }
        public string BailCompanyFax { get; set; }
    }
    public class BailTransactionVm
    {
        public int BailTransactionId { get; set; }
        public int ArrestId { get; set; }
        public string BailReceiptNumber { get; set; }
        public int? BailCompanyId { get; set; }
        public string PaymentTypeLookup { get; set; }
        public int BailAgentId { get; set; }
        public string BailPaymentNumber { get; set; }
        public string BailPostedBy { get; set; }
        public string BailTransactionNotes { get; set; }
        public decimal? AmountPosted { get; set; }
        public DateTime? CreateDate { get; set; }
        public Boolean VoidFlag { get; set; }
    }

    public class BailSaveHistory2Vm
    {
        public int BailSaveHistory2Id { get; set; }
        public string BailType { get; set; }

        public string BailNote { get; set; }

        public decimal? BailAmount { get; set; }
        public int CreateBy { get; set; }
        public DateTime? CreateDate { get; set; }
        public PersonnelVm OfficerDetails { get; set; }
    }

    public class BailCompanyDetails
    {
        public List<BailCompanyVm> BailCompanies { get; set; }
        public List<BailAgentVm> BailAgencies { get; set; }
    }
    public class GetSentenceCode
    {
        public string SentenceCode {get;set;}
       
    }
    public class ArrestSentenceHistoryList
    {
        public int ArrestSentenceHistoryId {get;set;}
        public string ArrestSentenceHistory {get;set;}
    }

}
