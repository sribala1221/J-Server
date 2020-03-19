using System;
using System.Collections.Generic;

namespace ServerAPI.ViewModels
{
    public class ClassLinkInputs
    {
        public bool Last12hours { get; set; }
        public bool DateRange { get; set; }
        public int InmateId { get; set; }
        public int FacilityId { get; set; }
        public int LinkType { get; set; }
        public int OfficerId { get; set; }
        public bool DeleteFlag { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int Hours { get; set; }
    }
    public class ClassLinkDetails
    {
        public int InmateClassificationLinkId { get; set; }
        public DateTime? CreateDate { get; set; }
        public PersonnelVm Officer { get; set; }
        public int OfficerId { get; set; }
        public string LinkType { get; set; }
        public string LinkNote { get; set; }
        public int? LinkTypeValue { get; set; }
        public int? DeleteFlag { get; set; }
        public List<CLassLinkInmateVm> ClassLinkInmateDetails { get; set; }

    }
    public class ClassLinkAddParam
    {
        public int LinkType { get; set; }
        public string LinkNote { get; set; }
        public int[] InmateIds { get; set; }

    }
    public class ClassLinkUpdateParam
    {
        public int InmateClassificationLinkId { get; set; }
        public int LinkType { get; set; }
        public string LinkNote { get; set; }
        public int[] InmateIds { get; set; }
    }
    public class CLassLinkInmateVm : InmateDetailVm
    {
        public int InmateClassificationLinkId { get; set; }
    }
    public class ClassLinkViewHistoryVm
    {
        public ClassLinkType ClassType { get; set; }
        public string InmateNumber { get; set; }
        public DateTime? ClassDate { get; set; }
        public string ClassNarrative { get; set; }
        public PersonVm PersonInfo { get; set; }
        public PersonnelVm Personnel { get; set; }
        public bool ClassReview { get; set; }
        public int Id { get; set; }
        public int InmateId { get; set; }
        public int ClassOfficerId { get; set; }
        public DateTime? LastReviewDate { get; set; }
        public string Color { get; set; }
        public int DeleteFlag { get; set; }
        public string PathName { get; set; }
        public string PhotoFilePath { get; set; }
        public int PersonId { get; set; }
        public int? LinkType { get; set; }
        public string LinkNote { get; set; }
        public string Reason { get; set; }
        public int? HousingUnitToId { get; set; }
        public string DisciplinaryInmateType { get; set; }
        public int? PersonFlagIndex { get; set; }
        public int? InmateFlagIndex { get; set; }
        public int? DietFlagIndex { get; set; }
        public int InmateActive { get; set; }
        public string PersonClassificationType { get; set; }
        public string PersonClassificationSubSet { get; set; }
        public string PersonClassificationStatus { get; set; }
        public DateTime? PersonClassificationDateThru { get; set; }
        public string PersonClassificationNotes { get; set; }
        public int PrivilegeId { get; set; }
        public DateTime? PrivilegeDate { get; set; }
        public int PrivilegeRemoveOffId { get; set; }
        public string PrivilegeNote { get; set; }
        public DateTime? PrivilegeExpires { get; set; }
        public string PrivilegeDateDesc { get; set; }
        public string PrivilegeExpireDesc { get; set; }
        public string PrivilegeDescription { get; set; }
        public int Count { get; set; }
        public string AppletsSavedType { get; set; }
        public string AppletsSavedTitle { get; set; }
        public string LookupDescription { get; set; }
        public string ArrestBookingNo { get; set; }
        public string ReleaseReason { get; set; }
        public string FacilityAbbr { get; set; }
        public string HousingUnitLocation { get; set; }
        public string HousingUnitNumber { get; set; }
        public string FloorNoteType { get; set; }
        public string FloorNoteNarrative { get; set; }
        public string DisciplinaryNumber { get; set; }
        public string DisciplinaryLocation { get; set; }
        public string DisciplinarySynopsis { get; set; }
        public string PersonLastName { get; set; }
        public string PersonFirstName { get; set; }
        public string PersonMiddleName { get; set; }
        public string HousingUnitBedNumber { get; set; }
        public string HousingUnitBedLocation { get; set; }
        public string KeepSepAssoc1 { get; set; }
        public string KeepSepReason { get; set; }
        public string KeepSepAssoc1Subset { get; set; }
        public int? KeepSepAssoc1Id { get; set; }
        public int? KeepSepAssoc1SubsetId { get; set; }
        public int? PersonClassificationTypeId { get; set; }
        public int? PersonClassificationSubSetId { get; set; }
    }
    public class ClassLinkHousingVm
    {
        public string HousingUnitLocation { get; set; }
        public string HousingUnitNumber { get; set; }
        public int FacilityId { get; set; }
        public string FacilityAbbr { get; set; }
        public int? HousingUnitId { get; set; }

    }
    public class ClassLinkInmateVm
    {
        public int? InmateId { get; set; }
        public string InmateNumber { get; set; }
        public int PersonId { get; set; }
        public DateTime? LastReviewDate { get; set; }
    }
    public enum ClassLinkType
    {
        INITIAL = 1,
        RECLASSIFY,
        NARRATIVE,
        ATTACH,
        LINK,
        INTAKE,
        RELEASE,
        HOUSING,
        NOTE,
        INCIDENT,
        MESSAGE,
        FLAG,
        KEEPSEPINMATE,
        KEEPSEPINMATENEW,
        KEEPSEPASSOC,
        KEEPSEPSUBSET,
        ASSOC,
        PRIVILEGES
    }

}