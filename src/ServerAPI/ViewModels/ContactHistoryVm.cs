using System;

namespace ServerAPI.ViewModels
{
    public class ContactHistoryVm
    {
        public int PersonId { get; set; }
        public int InmateId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public bool IsHousingNumber { get; set; }
        public bool IsHousingBed { get; set; }
        public bool IsIntakeDate { get; set; }
        public bool IsVisitorHistory { get; set; }
        public bool IsLocation { get; set; }
        public bool IsIncidentDate { get; set; }
        public bool IsGrievance { get; set; }
        public bool IsClassify { get; set; }
        public bool IsKeepSep { get; set; }
        public bool IsRelease { get; set; }
        public bool IsHousingMove { get; set; }
        public bool IsPhoto { get; set; }
        public bool IsTracking { get; set; }
        public bool IsJmsIncident { get; set; }
        public bool IsFloorNote { get; set; }
        public bool IsDna { get; set; }
        public bool IsTesting { get; set; }
        public int AddHour { get; set; }
        public DateTime? FilterDate { get; set; }
    }

    public class ContactDetail
    {
        public string Detail1 { get; set; }
        public string Detail2 { get; set; }
        public string Detail3 { get; set; }
        public string Detail4 { get; set; }
    }
    public enum ContactTypeEnum
    {
        Arresting,
        Transport,
        Receiving,
        Search,
        Supervisor,
        Booking,
        Reporting
    }
    public class ContactHistoryModelVm    {
        public int Id { get; set; }
        public int PersonId { get; set; }
        public int InmateId { get; set; }
        public string Type { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string Detail1 { get; set; }
        public string Detail2 { get; set; }
        public string Detail3 { get; set; }
        public DateTime? SearchFromDate { get; set; }
        public DateTime? SearchToDate { get; set; }
        public DateTime? FilterDate { get; set; }
        public int? Detail1Id { get; set; }
        public  int SearchInmateId { get; set; }
    }
    public class ContactHistoryDetailVm
    {
        public int Id { get; set; }
        public int? PersonId { get; set; }
        public int? PersonnelId { get; set; }
        public int InmateId { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public DateTime? Dob { get; set; }
        public string Email { get; set; }
        public string HomePhone { get; set; }
        public string BusinessPhone { get; set; }
        public string Type { get; set; }
        public string InmateNumber { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int Count { get; set; }
        public int? UnitId { get; set; }
        public PersonAddressVm AddressDetail { get; set; } 
        public int? Detail1Id { get; set; }
        public ContactDetail Detail { get; set; }
        public string Detail1 { get; set; }
        public string PhotographRelativePath { get; set; }
        public int IdentifierId { get; set; }
        public bool BookingStatus { get; set; }
    }
}
