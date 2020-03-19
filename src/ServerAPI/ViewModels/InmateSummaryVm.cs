using System;
using System.Collections.Generic;

namespace ServerAPI.ViewModels
{
    public class InmateSummaryVm
    {
        public int InmateId { get; set; }
        public bool InmateHeader { get; set; }
        public bool InmateAlerts { get; set; }
        public bool ActiveBookings { get; set; }
        public bool ReleasedBookings { get; set; }
        public bool InOut { get; set; }
        public bool FloorNote { get; set; }
        public bool Incident { get; set; }
        public bool Track { get; set; }
        public bool Visitors { get; set; }
        public bool InmateClassification { get; set; }
        public bool Appointments { get; set; }
        public bool Housinghistory { get; set; }
        public bool KeepSeparate { get; set; }
        public bool Association { get; set; }
    }

    public class InmateSummaryDetailsVm
    {
        public InmateHeaderVm InmateHeader { get; set; }
        public PersonVm PersonDetails { get; set; }
        public InmateAlerts InmateAlerts { get; set; }
        public List<ArrestDetails> ActiveBookings { get; set; }
        public List<ArrestDetails> ReleasedBookings { get; set; }
        public List<IncarcerationDetail> InOut { get; set; }
        public List<FloorNoteXrefDetails> FloorNotes { get; set; }
        public List<DisciplinaryIncidentDetails> Incident { get; set; }
        public List<InmateTrackDetails> Track { get; set; }
        public List<VisitorDetails> Visitors { get; set; }
        public List<InmateClassificationVm> InmateClassification { get; set; }
        public List<InmateAppointmentList> InmateAppointment { get; set; }
        public List<InmateHousingHistory> HousingHistory { get; set; }
        public List<KeepSeparateVm> KeepSeparate { get; set; }
        public List<PersonClassificationDetails> Association { get; set; }

    }

    public class PersonClassificationDetails
    {
        public int PersonId { get; set; }
        public int PersonClassificationId { get; set; }
        public int PersonnelId { get; set; }
        public string ClassificationType { get; set; }
        public string ClassificationSubset { get; set; }
        public string ClassificationStatus { get; set; }
        public string ClassificationNotes { get; set; }
        public int? ClassificationCompleteBy { get; set; }
        public int CreateById { get; set; }
        public int? UpdateById { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public DateTime? CreateDate { get; set; }
        public DateTime? UpdateDate { get; set; }
        public bool InactiveFlag { get; set; }
        public int[] ClassificationFlag { get; set; }
        public string ClassifyHistoryList { get; set; }
        public PersonnelVm CreatedBy { get; set; }
        public PersonnelVm UpdatedBy { get; set; }
        public int? ClassificationTypeId { get; set; }
        public int? ClassificationSubsetId { get; set; }
        public PersonVm PersonDetails { get; set; }
        public HousingDetail HousingDetails { get; set; }
        public InmateVm InmateDetails { get; set; }
        public int? HousingUnitListId { get; set; }
    }

    public class ClassifyAlertAssociationVm
    {
        public List<PersonClassificationDetails> AlertAssociation { get; set; }
        public List<AlertAssociationVm> AlertAssociationCount { get; set; }
        public List<HousingUnitListVm> HousingBuilding { get; set; }
        public List<HousingUnitListVm> HousingNumber { get; set; }
        public List<HousingGroupVm> HousingGroup { get; set; }
    }

    public class AlertAssociationVm
    {
        public int Count { get; set; }
        public int SubsetCount { get; set; }
        public int ClassificationTypeId { get; set; }
        public int? ClassificationSubsetId { get; set; }
        public string ClassificationType { get; set; }
        public string ClassificationSubset { get; set; }
    }

    public class InmateAlerts
    {
        public int InmateId { get; set; }
        public int PersonId { get; set; }
        public List<string> PersonAlerts { get; set; }
        public List<string> PersonFlagAlerts { get; set; }
        public List<PersonClassificationDetails> PersonClassificationDetails { get; set; }
        public List<KeepSeparateVm> KeepSepDetails { get; set; }
        public int WarrantDetails { get; set; }
        public int WarrantHold { get; set; }
        public bool IllegalAlien { get; set; }
        public bool InmateActive { get; set; }
        public bool License { set; get; }

    }

    public class ArrestDetails
    {
        public string BookingNumber { set; get; }
        public int? ArrestSentenceCode { get; set; }
        public DateTime? ArrestSentenceReleaseDate { get; set; }
        public int? ArrestSentenceIndefiniteHold { get; set; }
        public decimal? BailAmount { get; set; }
        public string ArrestSiteBookingNumber { get; set; }
        public string ArrestTypeId { get; set; }
        public string ArrestType { get; set; }
        public int? ArrestExamineBooking { get; set; }
        public string ArrestCaseNumber { get; set; }
        public string ArrestPcn { get; set; }
        public DateTime? ArrestBookingDate { get; set; }
        public int? ArrestBookingStatusId { get; set; }
        public string ArrestBookingStatus { get; set; }
        public DateTime? ArrestDate { get; set; }
        public string ArrestLocation { get; set; }
        public int ArrestOfficerId { get; set; }
        public string ArrestOfficerText { get; set; }
        public int? ArrestBookingOfficerId { get; set; }
        public string ArrestAgencyName { get; set; }
        public string BookingAgencyName { get; set; }
        public int? ArrestArraignmentCourtId { get; set; }
        public DateTime? ArrestArraignmentDate { get; set; }
        public string ArrestCourtDocket { get; set; }
        public string ArrestNonCompliance { get; set; }
        public string ArrestConditionsOfRelease { get; set; }
        public int ArrestId { get; set; }
        public int IncarcerationArrestXrefId { get; set; }
        public int? ArrestSentenced { get; set; }
        public string ArrestSentenceByHour { get; set; }
        public DateTime? ArrestSentenceStartDate { get; set; }
        public int? ArrestTimeServedDays { get; set; }
        public int? ArrestSentenceDaysStayed { get; set; }
        public int? ArrestSentenceForThwith { get; set; }
        public int? ArrestSentenceGwGtDays { get; set; }
        public int? ArrestSentenceGwGtAdjust { get; set; }
        public int? ArrestSentenceEarlyRelease { get; set; }
        public int? ArrestSentenceDays { get; set; }
        public string ArrestSentenceFineDays { get; set; }
        public int? ArrestSentenceWeekender { get; set; }
        public int? InmateId { get; set; }
        public bool ArrestActive { get; set; }
        public List<WarrantDetails> WarrantDetails { get; set; }
        public List<CrimeDetails> CrimeDetails { get; set; }
        public IncarcerationDetail IncarcerationDetails { get; set; }
        public string Court { get; set; }
        public PersonnelVm ArrestOfficer { get; set; }
        public PersonnelVm BookingOfficer { get; set; }
        public int? CourtId { get; set; }
        public int ArrestAgencyId { get; set; }
        public int BookingAgencyId { get; set; }
        public List<int> DisciplinaryDays { get; set; }
        public List<BookingHistory> ReactiveHistory { get; set; }
        public int ArrestSentenceDisciplinaryDaysFlag { get; set; }
    }

    public class ArrestDetailsVm : ArrestDetails
    {
        public int IncarcerationId { set; get; }
        public int? BillingAgencyId { set; get; }
        public int? ArrestCourtJurisdictionId { set; get; }
        public DateTime? ReleaseDate { set; get; }
        public string ReleaseReason { set; get; }
        public string BillingAgencyName { set; get; }
        public string ArrestCourtJurisdictionName { set; get; }
        public bool ArrestCompleteFlag { set; get; }
        public InmateHousing PersonDetails { set; get; }
        public int? WizardProgressId { set; get; }
        public List<WizardStep> LastStep { get; set; }
        public string[] CaseType { get; set; }
    }

    public class BookingHistory
    {
        public DateTime? BookingDate { get; set; }
        public DateTime? ReleaseDate { get; set; }
        public string BookingReason { get; set; }
    }

    public class WarrantDetails
    {
        public int WarrantId { get; set; }
        public int AgencyId { get; set; }
        public string WarrantNumber { get; set; }
        public DateTime? WarrantDate { get; set; }
        public string WarrantChargeLevel { get; set; }
        public string Type { get; set; }
        public string Number { get; set; }
        public string Description { get; set; }
        public int? WarrantAgencyId { get; set; }
        public int? LocalWarrantFlag { get; set; }
        public int? Complete { get; set; }
        public string County { get; set; }
        public string WarrantBail { get; set; }
        public List<PrebookCharge> ChargeDetails { get; set; }
        public DateTime? OffenceDate { get; set; }
    }

    public class CrimeDetails
    {
        public int CrimeId { get; set; }
        public int? CrimeCount { get; set; }
        public string CrimeType { get; set; }
        public string CrimeSection { get; set; }
        public string CrimeDescription { get; set; }
        public string CrimeSubSection { get; set; }
        public string CrimeStatueCode { get; set; }
        public string CrimeNcicCode { get; set; }
        public string CrimeUrcOffenceCode { get; set; }
        public string CrimeNotes { get; set; }
        public string CrimeCodeType { get; set; }
        public decimal? BailAmount { get; set; }
        public int? CrimeNumber { get; set; }
        public int? CrimeGroupId { get; set; }
        public DateTime? CreateDate { get; set; }
        public bool IsJmsSearch { get; set; } //Identify function call from Prebook or Jms

    }

    public class FloorNoteXrefDetails
    {
        public DateTime? FloorNoteDate { get; set; }
        public string FloorNoteTime { get; set; }
        public string FloorNoteDescription { get; set; }
    }

    public class DisciplinaryIncidentDetails
    {
        public DateTime? DisciplinaryDate { get; set; }
        public string DisciplinaryDescription { get; set; }
        public string DisciplinaryNumber { get; set; }
        public int IncidentId { get; set; }
        public int? DisciplinaryType { get; set; }
    }

    public class InmateTrackDetails
    {
        public int InmateTrackId { get; set; }
        public DateTime? InmateTrackDateIn { get; set; }
        public string InmateTrackTimeIn { get; set; }
        public DateTime? InmateTrackDateOut { get; set; }
        public string InmateTrackTimeOut { get; set; }
        public string InmateTrackLocation { get; set; }
    }

    public class VisitorDetails
    {
        public DateTime? VisitorDateIn { get; set; }
        public string VisitorTimeIn { get; set; }
        public DateTime? VisitorDateOut { get; set; }
        public string VisitorTimeOut { get; set; }
        public string VisitorReason { get; set; }
        public string VisitorFirstName { get; set; }
        public string VisitorLastName { get; set; }
    }

    public enum KeepSepType
    {
        Inmate,
        Association,
        Subset
    }

    public class InmateHousingHistory
    {
        public int? FromHousingUnitId { get; set; }
        public int? ToHousingUnitId { get; set; }
        public HousingDetail FromHousing { get; set; }
        public HousingDetail ToHousing { get; set; }
        public DateTime? HistoryMoveDate { get; set; }
        public string Reason { get; set; }
        public int MoveOfficerId { get; set; }
        public PersonnelVm MoveOfficer { get; set; }

    }
}
