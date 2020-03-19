using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;

namespace ServerAPI.ViewModels
{
    public class InmateSummaryPdfVm
    {
        public HousingDetail HousingDetails { get; set; }
        public PersonVm PersonDetails { get; set; }
        public PersonCharVm PersonCharDetails { get; set; }
        public PersonDetailVM IncNameDetails { get; set; }
        public ArrestBookingDetails CaseDetails { get; set; }
        public List<IncarcerationDetail> IncarcerationDetails { get; set; }
        public List<KeepSeparateVm> KeepSeparateDetails { get; set; }
        public List<ObservationScheduleVm> ObservationDetails { get; set; }
        public List<InmateAlertVm> PersonAlertDetailLst { get; set; }
        public List<PersonClassificationDetails> PersonClassificationDetailLst { get; set; }
        public List<PersonDescriptorVm> PersonDescriptorDetailLst { get; set; }
        public InmatePdfHeader InmatePdfHeaderDetails { get; set; }
        public Form FormData { get; set; }
        public List<AkaVm> AkaDetailLst { get; set; }
        public List<string> MonikerLst { get; set; }
        public string PhotoFilePath { get; set; }
        public string ClassificationReason { get; set; }
        public string SiteOption { get; set; }
        public string SentenceByCharge { get; set; } // for SiteOption value
        public string Employer { get; set; }
        public string Occupation { get; set; }
        public string InmateSiteNumber { get; set; }
        public string TxtInmateNumber { get; set; }
        public string TxtSiteInmate { get; set; }
        public string TxtAKAInmateNum { get; set; }
        public string TxtAKASiteInmateNum { get; set; }
        public string TxtOtherPhoneNum { get; set; }
        public string TxtAFISNumber { get; set; }
        public string TxtAKAFBI { get; set; }
        public List<DnaVm> PersonDNA { get; set; }
        public JObject CustomLabel { get; set; }
    }

    public class InmatePdfHeader
    {
        public string SummaryHeader { get; set; }
        public string AgencyName { get; set; }
        public string OfficerName { get; set; }
        public string PersonnelNumber { get; set; }
        public string OfficerBadgeNumber { get; set; }
        public string InmateNumber { get; set; }
        public DateTime StampDate { get; set; }
        public string ArrestingAgencyName { get; set; }
        public string PbpcPath { get; set; }
    }


    public class AltSentDetail
    {
        public int AltSentId { get; set; }
        public int ProgramId { get; set; }
        public int? IncarcerationId { get; set; }
        public string ProgramAbbr { get; set; }
        public string FacilityAbbr { get; set; }
        public int TotalDaysServed { get; set; }
        public int Adts { get; set; }
        public int TotalAttend { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? ThruDate { get; set; } //End date
    }

    public class ArrestBookingDetails
    {
        public int ArrestId { get; set; }
        public int ArrestingAgencyId { get; set; }
        public int ArrestOfficerId { get; set; }
        public int OriginatingAgencyId { get; set; }
        public int BillingAgencyId { get; set; }
        public int ArrestReceivingOfficerId { get; set; }
        public int ArrestCourtJurisdictionId { get; set; }
        public int? ReleaseOfficerId { get; set; }
        public int? WeekEnder { get; set; }
        public int? ArrestSentenceIndefiniteHold { get; set; }
        public int? ArrestSentenceCode { get; set; }
        public int? ArrestBookingStatus { get; set; }
        public int? LenDays { get; set; }
        public decimal? BailAmount { get; set; }
        public string ArrestType { get; set; }
        public string BookingNo { get; set; }
        public string ArrestLocation { get; set; }
        public string ArrestOfficerText { get; set; }
        public string ArrestOfficerLastName { get; set; }
        public string ArrestOfficerNumber { get; set; }
        public string ArrestOfficerBadgeNumber { get; set; }
        public string ArrestAgency { get; set; }
        public string ArrestAbbr { get; set; }
        public string ArrestAgencyOriNumber { get; set; }
        public string OrginAgency { get; set; }
        public string BillingAgency { get; set; }
        public string CaseNumber { get; set; }
        public string BookType { get; set; }
        public string RecOfficerLastName { get; set; }
        public string RecOfficerNumber { get; set; }
        public string RecOfficerBadgeNumber { get; set; }
        public string Status { get; set; }
        public string Court { get; set; }
        public string CourtName { get; set; }
        public string Docket { get; set; }
        public int Sentence { get; set; }
        public string OrginAbbr { get; set; }
        public string CondOfClear { get; set; }
        public string CondOfClearanceNote { get; set; }
        public string ReleaseReason { get; set; }
        public string ReleaseNotes { get; set; }
        public string ArrestNotes { get; set; }
        public string BailType { get; set; }
        public bool BailNoBailFlag { get; set; }
        public bool BookingCompleteFlag { get; set; }
        public DateTime? ArraignmentDate { get; set; }
        public DateTime? ArrestDate { get; set; }
        public DateTime? BookDate { get; set; }
        public DateTime? ReleaseDate { get; set; }
        public SentenceDetailsVm SentenceDetails { get; set; }
        //public List<ScheduleCourtDetail> ScheduleCourtDetails { get; set; }
        public ScheduleCourtDetail ScheduleCourtDetails { get; set; }
        public List<WarrantDetails> WarrantDetailLst { get; set; }
        public List<PrebookCharge> ChargeDetails { get; set; }
        // Inmate Header Booking info
        public DateTime? SentReleaseDate { get; set; }
        public int? SentenceDaysToServe { get; set; }
        // both crime and warrant info charges for MISC -> NCIC - Incarceration History details
        public List<ChargesVm> LstCharges { get; set; }
        public DateTime? ScheduleClearDate { get; set; }
        public PersonnelVm ReleaseOfficer { get; set; }
        public int IncarcerationArrestXrefId { get; set; }
        public int IncarcerationArrestId { get; set; }
        public int? IncarcerationId { get; set; }
        public string ArrestDisposition { get; set; }
        public DateTime? SentenceStartDate { get; set; }
        public int? ActualDaysToServe { get; set; }

    }

    public class ScheduleCourtDetail
    {
        public int AgencyCourtId { get; set; }
        public int AgencyCourtDeptId { get; set; }
        public string AppointmentLocation { get; set; }
        public string AgencyAbbreviation { get; set; }
        public string SchdCourtName { get; set; }
        public string DepartmentName { get; set; }
        public string AppointmentReason { get; set; }
        public DateTime? AppointmentDate { get; set; }
        public int AppointmentLocationId { get; set; }
    }

    public class ObservationScheduleVm
    {
        public int ObservationScheduleId { get; set; }
        public int ObservationType { get; set; }
        public string Note { get; set; }
        public string TypeName { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int InmateId { get; set; }
        public List<KeyValuePair<int, string>> ObservationTypeList { get; set; }
        public string HistoryList { get; set; }
        public DateTime? LastReminderEntry { get; set; }
        public int PersonId { get; set; }
    }

    public class UserControlFieldTags
    {
        public int ControlId { get; set; }
        public string FieldTag { get; set; }
        public string FieldLabel { get; set; }
    }

    public class FileContent
    {
        public FileContentResult ContentResult { get; set; }
        public InmateSummaryPdfVm ContentDetails { get; set; }
        public byte[] Bytes { get; set; }
    }
}
