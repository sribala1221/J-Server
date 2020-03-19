using System;
using System.Collections.Generic;

namespace ServerAPI.ViewModels
{
    public class VisitorListVm
    {
        public List<KeyValuePair<int,string>> VisitorIdTypeList { get; set; }
        public List<KeyValuePair<int,string>> VisitorStateList { get; set; }
        public List<KeyValuePair<int, string>> VisitorRelationshipList { get; set; }
        public List<KeyValuePair<int, string>> ProfessionalTypeList { get; set; }
        public List<VisitorInfo> VisitorInfo { get; set; }
        public VisitorCountDetails VisitorCountDetails { get; set; }
    }

    public class VisitorCountDetails
    {
        public int ActiveCount { get; set; }
        public int InActiveCount { get; set; }
        public int RejectAllCount { get; set; }
        public int RejectSpecificCount { get; set; }
    }

    public class VisitorInfo
    {
        public int? VisitorType { get; set; }
        public string VisitorIdType { get; set; }
        public string VisitorIdNumber { get; set; }
        public string VisitorState { get; set; }
        public int? VisitorRelationshipId { get; set; }
        public int VisitorInmateCount { get; set; }
        public int VisitorCount { get; set; }
        public string VisitorNotes { get; set; }
        public string VisitorGender { get; set; }
        public int PersonId { get; set; }
        public int? AddressId { get; set; }
        public int? VisitorListId { get; set; }
        public string VisitorHisitoryList { get; set; }
        public int? VistorRejectAll { get; set; }
        public int? VistorRejectSpecificInmate { get; set; }
        public int? VistorPersonofInterest { get; set; }
        public int? VisitorDeleteFlag { get; set; }
        public VisitorAddress VisitorAddress { get; set; }
        public PersonInfo PersonDetails { get; set; }
        public int? ProfessionalTypeId { get; set; }
        public string ProfessionalType { get; set; }
    }

    public class VisitorAddress
    {
        public string AddressNumber { get; set; }
        public string AddressDirection { get; set; }
        public string AddressStreet { get; set; }
        public string AddressSuffix { get; set; }
        public string AddressDirectionSuffix { get; set; }
        public string AddressUnitType { get; set; }
        public string AddressUnitNumber { get; set; }
    }

    public class AssignedInmateList
    {
        public int VisitorCount { get; set; }
        public int VisitorListToInmateId { get; set; }
        public string InmateNumber { get; set; }
        public string InmateNotes { get; set; }
        public string RelationShip { get; set; }
        public int? HousingUnitId { get; set; }
        public string HousingUnitLocation { get; set; }
        public string HousingUnitNumber { get; set; }
        public string Location { get; set; }
        public int PersonId { get; set; }
        public int InmateId { get; set; }
        public int? InmateDeleteFlag { get; set; }
        public int? RejectSpecificInmate { get; set; }
        public PersonVm PersonDetails { get; set; }
    }

    public class SearchVisitorList: VisitorInfo
    {
        public int InmateId { get; set; }
        public string VisitorFirstName { get; set; }
        public string VisitorLastName { get; set; }
        public string VisitorMiddleName { get; set; }
        public DateTime? VisitorDob { get; set; }
        public bool RejectAll { get; set; }
        public bool RejectSpecificInmate { get; set; }
        public bool PersonofInterest { get; set; }
        public bool IsActiveVisitor { get; set; }
        public DateTime? ExpireDate { get; set; }
        public bool IsPersonnel { get; set; }
    }

    public class InmateToVisitorInfo : VisitorInfo
    {
        public int InmateId { get; set; }
        public int? VisitorListToInmateId { get; set; }
        public bool RejectSpecificInmate { get; set; }
        public string VisitorInmateToHisitoryList { get; set; }
        public VisitorRejectDetails VisitorRejectDetails { get; set; }
        public bool IsPersonnel { get; set; }
    }

    public class VisitorRejectDetails
    {
        public string VisitorNotAllowedNote { get; set; }
        public string VisitorNotAllowedReason { get; set; }
        public DateTime? VisitorNotAllowedExpireDate { get; set; }
        public int? RejectAll { get; set; }
    }

    public class PersonOfInterestDetails
    {
        public string PersonOfInterestNote { get; set; }
        public string PersonOfInterestReason { get; set; }
        public DateTime? PersonOfInterestExpire { get; set; }
        public int? PersonOfInterest { get; set; }
    }

    public class PersonalVisitorDetails
    {
        public PersonIdentity PersonIdentity { get; set; }
        public PersonAddressVm PersonAddress { get; set; }
        public PersonAddressDetails PersonAddressDetails { get; set; }
        public VisitorRejectDetails VisitorRejectDetails { get; set; }
        public PersonOfInterestDetails PersonOfInterestDetails { get; set; }
        public SearchVisitorList VisitorInfo { get; set; }
    }

    public class VisitorListSaveHistory
    {
        public int  VisitorListHistoryId { get; set; }
        public DateTime? CreateDate { get; set; }
        public string PersonLastName { get; set; }
        public string OfficerBadgeNumber { get; set; }
        public string VisitorHisitoryList { get; set; }
        public List<VisitationHeader> VisitationHeader { get; set; }
    }

    public class VisitationHeader
    {
        public string Header { get; set; }
        public string Detail { get; set; }
    }

    public class HistoryList
    {
       public List<HistoryInfo> HistoryInfoList { get; set; }
       public List<string> VisitorFlagList { get; set; }
        public List<KeyValuePair<int,string>> VisitorDenyReasonList { get; set; }
    }

    public class HistoryInfo
    {
        public int? InmateId { get; set; }
        public int VisitorId { get; set; }
        public int PersonId { get; set; }
        public DateTime? VisitorDate { get; set; }
        public string VisitorTimeIn { get; set; }
        public string VisitorTimeOut { get; set; }
        public string VisitorFirstName { get; set; }
        public string VisitorLastName { get; set; }
        public string VisitorMiddleName { get; set; }
        public string VisitorSuffix { get; set; }
        public string VisitorLocation { get; set; }
        public string Reason { get; set; }
        public string VisitorType { get; set; }
        public string VisitorNotes { get; set; }
        public int VisitorDenyFlag { get; set; }
        public string VisitorDenyReason { get; set; }
        public string VisitorSystemAlerts { get; set; }
        public string VisitorDenyNotes { get; set; }
        public int? VisitorProfFlag { get; set; }
        public int? VistorNotAllowedFlag { get; set; }
        public int? VisitorDeleteFlag { get; set; }
    }

    public class SearchVisitorHistoryList
    {
        public int InmateId { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public bool DeniedVisit { get; set; }
        public string DenyReason { get; set; }
        public string VisitFlag { get; set; }
    }

    public class VisitationHistory
    {
        public DateTime? VisitorDateIn { get; set; }
        public DateTime? VisitorDateOut { get; set; }
        public PersonInfo VisitorInfo { get; set; }
        public PersonInfo InmateInfo { get; set; }
        public PersonInfo PersonnelInfo { get; set; }
        public string OfficerBadgeNumber { get; set; }
        public string VisitorRelationship { get; set; }
        public string VisitorReason { get; set; }
        public string VisitorNotes { get; set; }
        public string VisitorIdType { get; set; }
        public string VisitorIdNumber { get; set; }
        public string VisitorIdState { get; set; }
        public int VisitorDenyFlag { get; set; }
        public string VisitorDenyReason { get; set; }
        public string VisitorSystemFlagString { get; set; }
        public string VisitorDenyNote { get; set; }
        public DateTime? VisitorCreatedDate { get; set; }
    }

    public class PersonInfo
    {
        public string PersonFirstName { get; set; }
        public string PersonLastName { get; set; }
        public string PersonMiddleName { get; set; }
        public string PersonSuffix { get; set; }
        public int InmateId { get; set; }
        public int PersonId { get; set; }
        public int FacilityId { get; set; }
        public int? HousingUnitId { get; set; }
        public string InmateNumber { get; set; }
        public DateTime? LastReviewDate { get; set; }
        public string InmateCurrentTrack { get; set; }
        public int? PersonSexLast { get; set; }
        public string PersonDlNumber { get; set; }
        public string PersonPhone { get; set; }
        public DateTime? PersonDob { get; set; }
        public int? InmateClassificationId { get; set; }
        public short? PersonAge { get; set; }
        public string sex { get; set; }
        


    }

    public class InmateHousing : PersonInfo
    {
        public string HousingLocation { get; set; }
        public string HousingNumber { get; set; }
        public string HousingBedNumber { get; set; }
        public string HousingBedLocation { get; set; }
        public bool InmateActive { get; set; }
        public string InmateClassificationReason { get; set; }
        public string PhotoFilePath { get; set; }
    }

    public class RejectInmateHistory
    {
        public DateTime? SaveDate { get; set; }
        public int? VisitorRejectFlag { get; set; }
        public string VisitorRejectNote { get; set; }
        public string VisitorRejectReason { get; set; }
        public DateTime? VisitorRejectExpireDate { get; set; }
        public string PersonLastName { get; set; }
        public string OfficerBadgeNumber { get; set; }
    }

    public enum VisitorRejectFlag
    {
        RejectAll,
        RejectSpecificInmate,
        PersonOfInterest
    }
}
