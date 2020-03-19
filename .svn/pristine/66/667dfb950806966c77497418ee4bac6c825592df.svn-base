using System;
using System.Collections.Generic;

namespace ServerAPI.ViewModels
{
    public class RequestVm
    {
        public int? PersonId { get; set; }
        public int? InmateId { get; set; }
        public int RequestId { get; set; }
        public int? ActionLookupId { get; set; }
        public int? ShowInFlag { get; set; }
        public DateTime? RequestDate { get; set; }
        public string Action { get; set; }
        public string PersonFirstName { get; set; }
        public string PersonLastName { get; set; }
        public string InmateNumber { get; set; }
        public string Note { get; set; }
        public int? ClearedBy { get; set; }
        public int CreatedBy { get; set; }
        public int? PendingBy { get; set; }
        public DateTime? PendingDate { get; set; }
        public int? FacilityId { get; set; }
        public string FacilityAbbr { get; set; }
        public int? InactiveFlag { get; set; }
        public int? PendingFacFlag { get; set; }
        public int? ReqResInmateFlag { get; set; }
        public Int64 Elapsed { get; set; }
        public string RequestCategory { get; set; }
        public string Disposition { get; set; }
        public int? TrackBy { get; set; }
        public int? ReadFlag { get; set; }
        public int InmateCount { get; set; }
        public string HousingLocation { get; set; }
        public string HousingNumber { get; set; }
        public int HousingUnitId{ get; set; }
        public int? ReqHousingUnitListId { get; set; }
        public int? AllowActionTransfer { get; set; }
        public string PersonnelNumber { get; set; }
        public string PersonnelBadgeNumber { get; set; }
        public List<HousingDetail> LstHousingDetail { get; set; }
        public int? AppSubModuleId { get; set; }
        public string RequestNote { get; set; }
        public string RequestDisposition { get; set; }
        public DateTime? CreateDate { get; set; }
        public int? RequestedById { get; set; }
        public PersonnelVm RequestBy { get; set; }
        public PersonVm PersonDetails { get; set; }
        public string RequestDepartment { get; set; }
        public string RequestPosition { get; set; }
        public DateTime? ClearedDate { get; set; }
        public HousingDetail InmateHousingDetail { get; set; }
        public PersonnelVm PendingPerson { get; set; }
        public PersonnelVm ClearedPerson { get; set; }
		public bool RequestProgram { get; set; }
		public bool RequestWorkCrew { get; set; }
		public string RequestLocation { get; set; }
		public int? NewActionRequest { get; set; }
        public int? CreateAppointment { get; set; }
        public int? CreateProgramRequest { get; set; }
        public int? SendInternalEmail { get; set; }
        public int? SendEmail { get; set; }
        public int? OpenInmateFile { get; set; }
        public bool RequestByInmate { get; set; }
		public bool ReturnDispoToInmate { get; set; }
		public bool IsRequestForm { get; set; }
		public bool CopyNote { get; set; }
		public int? UpdatedBy { get; set; }
		public bool IsView { get; set; }
        public ElapsedHours ElapsedStatus { get; set; }
	}

	public class RequestTrackVm
    {
        public RequestVm RequestDetails { get; set; }
        public List<RequestVm> LstRequestDetails { get; set; }
    }

    public class RequestDetails
    {
        public int? PersonId { get; set; }
        public int? InmateId { get; set; }
        public int RequestId { get; set; }
        public int? ActionLookupId { get; set; }
        public DateTime? RequestDate { get; set; }
        public DateTime? CreateDate { get; set; }
        public string Action { get; set; }
        public string PersonFirstName { get; set; }
        public string PersonLastName { get; set; }
        public string InmateNumber { get; set; }
        public string Note { get; set; }
        public int CreatedBy { get; set; }
        public int? FacilityId { get; set; }
        public string FacilityAbbr { get; set; }
        public int? InactiveFlag { get; set; }
        public string Disposition { get; set; }
        public int InmateCount { get; set; }
        public string HousingLocation { get; set; }
        public string HousingNumber { get; set; }
		public RequestTypeEnum RequestType { get; set; }
	}

    public class RequestOperations
    {
        public int RequestId { get; set; }
        public string ActionLookup { get; set; }
        public RequestStatus RequestStatus { get; set; }

    }

    public enum RequestStatus
    {
        None,
        Assigned,
        Cleared,
        Pending,
        Requested,
		Accepted,
        UndoCleared
    }

    public enum RequestDetailsStatus
    {
		
        Transfer = 1,
        Note,
        Response,
        Dispo
    }

	public enum RequestTypeEnum
	{
		None,
		Inmate,
		Visit,	
		Housing,
		Location,
		WorkCrew,
		Program,
		Department,
		Position
	}

	public class RequestTransfer
    {
        public int RequestId { get; set; }
        public int? PersonnelId { get; set; }
        public string Note { get; set; }
        public int? ReqActionLookupId { get; set; }
        public int ReqTransferchk { get; set; }
        public RequestDetailsStatus RequestStatus { get; set; }
        public string DispoCategory { get; set; }
        public bool ClearFlag { get; set; }
    }

    public class RequestClear
    {
        public int RequestId { get; set; }
        public bool IsPendingType { get; set; }
    }

    public class RequestValues
    {
        public int ActionLookupId { get; set; }
        public int? RequestedBy { get; set; }
        public int? InmateId { get; set; }
        public int FacilityId { get; set; }
        public int Top { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string Disposition { get; set; }
        public RequestStatus RequestStatus { get; set; }
        public RequestStatus RequestPersonnel { get; set; }

    }

	public class ReqResponsibilityVm	{
		public List<FloorNoteTypeCount> Responsibilities { get; set; }
		public List<FloorNoteTypeCount> AssignedLst { get; set; }
		public List<RequestVm> RequestDetailLst { get; set; }
		public List<RequestStatusVm> RequestStatusLst { get; set; }
	}

	public class RequestStatusVm
	{
		public int Count { get; set; }
		public int ActionId { get; set; }
		public string ActionName { get; set; }
		public ElapsedHours Elapsed { get; set; }
		public PersonnelVm Assigned { get; set; }
	}

	public enum ElapsedHours
	{
		TWELVEHOURS,
		TWENTYFOURHOURS,
		THIRTYSIXHOURS,
		NONE, //more then 36 hrs
	}
}
