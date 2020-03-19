using System.Collections.Generic;
using System;

namespace ServerAPI.ViewModels
{
    public class InmateBookingInfoVm
    {
        public List<KeyValuePair<int, string>> ArrestingAgency { get; set; }
        public List<KeyValuePair<int, string>> BillingAgency { get; set; }
        public List<KeyValuePair<int, string>> BookingAgency { get; set; }
        public List<KeyValuePair<int, string>> OriginatingAgency { get; set; }
        public List<KeyValuePair<int, string>> Court { get; set; }
        public BookingInfoArrestDetails ArrestDetails { get; set; }
        public string[] CondReleaseFlag { get; set; }
        public List<ClearanceFlag> ArrestCondClear { get; set; }
        public List<SiteOptionProp> SiteOptions { get; set; }
    }

    public class BookingInfoArrestDetails
    {
        public int ArrestId { get; set; }
        public DateTime? ArrestDate { get; set; }
        public string SiteBookingNo { get; set; }
        public string Pcn { get; set; }
        public bool FingerprintByDoj { get; set; }
        public string Location { get; set; }
        public string Type { get; set; }
        public string LawEnforcementDisposition { get; set; }
        public string NonCompliance { get; set; }
        public string ConditionsOfRelease { get; set; }
        public int? CourtJurisdictionId { get; set; }
        public DateTime? ArraignmentDate { get; set; }
        public string CourtDocket { get; set; }
        public string CaseNumber { get; set; }
        public int? BookingSupervisorId { get; set; }
        public int? BookingOfficerId { get; set; }
        public int ArrestOfficerId { get; set; }
        public string ArrestOfficerText { get; set; }
        public int ReceivingOfficerId { get; set; }
        public int? SearchOfficerId { get; set; }
        public int? TransportingOfficerId { get; set; }
        public string TransportingOfficerText { get; set; }
        public int ArrestingAgencyId { get; set; }
        public int? OriginatingAgencyId { get; set; }
        public int? BillingAgencyId { get; set; }
        public int BookingAgencyId { get; set; }
        public string Notes { get; set; }
        public string BookingNumber { get; set; }
        public int InmateId { get; set; }
        public string InmateSiteNumber { get; set; }
        public DateTime? BookingDate { get; set; }
        public string InmateNumber { get; set; }
        public List<ClearanceFlag> ClearanceList { get; set; }
        public string ArrestInfoHistoryList { get; set; }
        public int IncArrestXrefId { set; get; }
        public string ArrestSearchOfficerAddList { get; set; }
        public int FacilityId { get; set; }
        public DateTime ConfictArraignmentDate { get; set; }
    }

    public class ClearanceFlag
    {
        public bool Flag { get; set; }
        public string CondOfClearance { get; set; }
        public string CondOfClearanceNote { get; set; }
    }
}
