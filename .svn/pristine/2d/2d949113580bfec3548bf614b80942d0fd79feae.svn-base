using System;
namespace ServerAPI.ViewModels
{
    public class BookingDataVm : BookingStatusVm
    {
        public DateTime? BookedDate { get; set; }
        public int? IncarcerationId { get; set; }
        public bool ReleasedOut { get; set; }
        public bool InmateActive { get; set; }
    }

    public class BookingVerifyDataVm
    {
        public int InmateId { get; set; }
        public int PersonId { get; set; }
        public int IncarcerationId { get; set; }
        public string BookingNumber { get; set; }
        public int VerifyIdFlag { get; set; }
        public PersonVm Person {get;set;}
        public HousingDetail HousingUnit { get; set; }
        public string CurrentStateNumber { get; set; }
        public string CurrentFbiNumber { get; set; }
        public string LiveScanStateNumber { get; set; }
        public string LiveScanFbiNumber { get; set; }
        public bool IsBypass { get; set; }
        public bool IsCreatePerson { get; set; }
        public string PhotographRelativePath { get; set; }
        public int IdentifierId { get; set; }
        public string Payload { get; set; }
        public int VerifyExternalId { get; set; }
        public bool InmateActive { get; set; }
    }

    public enum BookingVerifyType
    {
        NotVerified,
        Verified,
        Move,
        Merge,
        ByPass
    }
}
