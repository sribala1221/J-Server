using System;
using System.Collections.Generic;

namespace ServerAPI.ViewModels
{

    public class BookingInProgressCountVm
    {
        public List<KeyValuePair<BookingTypeEnum, int>> BookingCounts { get; set; }
        public List<RequestTypes> PendingQueue { get; set; }
        public List<RequestTypes> AssignedQueue { get; set; }
        public List<TasksCountVm> TasksCount { get; set; }
        public BookingInProgressDetailsVm BookingDetails { get; set; }
    }
    
    public class BookingInProgressDetailsVm
    {
        public List<BookingVm> OverallBookingProgress { get; set; }
        public List<ArrestDetailsVm> BookingDataProgress { get; set; }
        public List<RecordsCheckVm> RecordsCheckResponse { get; set; }
        public List<ExternalAttachmentsVm> ExternalAttachments { get; set; }
        public List<RequestVm> PendingRequest { get; set; }
        public List<RequestVm> AssignedRequest { get; set; }
    }
    
    public class BookingInputsVm
    {
        public int FacilityId { get; set; }
        public bool ShowIntake { get; set; }
        public bool ActiveOnly { get; set; }
        //public RequestTypeEnum RequestType { get; set; }
        public BookingTypeEnum BookingType { get; set; }
        public int RequestLookupId { get; set; }
        public ReleaseTypeEnum ReleaseType { get; set; }
        public DateTime? TransportDate { get; set; }
        public int TaskLookupId { get; set; }
    }

    //public enum RequestTypeEnum
    //{
    //    None,
    //    Pending,
    //    Assigned
    //}

    public class RequestTypes
    {
        public int RequestCount { get; set; }
        public int RequestLookupId { get; set; }
        public string RequestLookupName { get; set; }
    }

    public class RequiredFields
    {
        public int UserControlId { get; set; }
        public string FieldName { get; set; }
        public bool FieldVisible { get; set; }
        public bool FieldRequired { get; set; }
        public string Fieldlabel { get; set; }
        public int FieldId { get; set; }
        public bool RestrictFlag { get; set; }
        public bool ProtectFlag { get; set; }
        public int? GroupId{ get; set; }
        public bool CompleteRequired { get; set; }
        public bool CompleteWarning { get; set; }
        public string ValidateCompleteBookTypeString { get; set; }
    }

    public class BookingComplete
    {
        public int InmateId { get; set; }
        public int IncarcerationId { get; set; }
        public int IncarcerationArrestXrefId { get; set; }
        public bool IsComplete { get; set; }
        public int ArrestId { get; set; }
        public bool BookingDataFlag { get; set; }
        public bool ReviewReleaseFlag { get; set; }
        public int PersonId { get; set; }
    }

    public enum BookingTypeEnum
    {
        None,
        OverallBookingProgress,
        BookingDataProgress,
        RecordsCheckResponsess,
        ExternalAttachments,
        Pending,
        Assigned,
        Tasks
    }

    public class KeeperNoKeeperDetails
    {
        public bool NoKeeperFlag { get; set; }
        //public List<KeyValuePair<string, string>> lstNoKeeperReason { get; set; }
        public List<NoKeeperHistory> lstNoKeeperHistory { get; set; }
    }

    public class NoKeeperHistory
    {
        public int incarcerationId { get; set; }
        public bool KeeperFlag { get; set; }
        public bool NoKeeperFlag { get; set; }
        public string NoKeeperReason { get; set; }
        public string NoKeeperNote { get; set; }
        public int NoKeeperById { get; set; }
        public PersonnelVm NoKeeperBy { get; set; }
        public DateTime NoKeeperDate { get; set; }
    }
}
