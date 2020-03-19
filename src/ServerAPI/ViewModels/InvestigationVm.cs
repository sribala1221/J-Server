using System;
using System.Collections.Generic;

namespace ServerAPI.ViewModels
{
    public class InvestigationDataVm
    {
        public List<InvestigationVm> Investigation { get; set; }
        public List<LookupVm> Lookups { get; set; }
        public List<PersonnelVm> Personnels { get; set; }
    }

    public class InvestigationVm
    {
        public int? CompleteDisposition { get; set; }
        public bool CompleteFlag { get; set; }
        public string InvestigationSummary { get; set; }
        public int InvestigationStatus { get; set; }
        public int InvestigationType { get; set; }
        public int OfficerId { get; set; }
        public DateTime StartDate { get; set; }
        public string InvestigationName { get; set; }
        public string InvestigationNumber { get; set; }
        public int? InvestigationId { get; set; }
        public bool DeleteFlag { get; set; }
        public bool ManagerFlag { get; set; }
        public List<KeyValuePair<int, string>> PersonnelList { get; set; }
        public string SourceReferral { get; set; }
        public int? SourceDepartment { get; set; }
        public DateTime ReceiveDate { get; set; }
        public PersonnelVm Officer { get; set; }
        public DateTime? CompleteDate { get; set; }
        public string InvestigationHistory { get; set; }
        public string CompleteReason { get; set; }
    }

    public class InvestigationAllDetails
    {
        public List<InvestigationFlag> InvestigationFlag { get; set; }
        public List<InvestigationPersonnelVm> InvestigationPersonnel { get; set; }
        public List<InvestigationNotesVm> InvestigationNotes { get; set; }
        public List<PrebookAttachment> InvestigationAttachments { get; set; }
        public List<InvestigationIncident> InvestigationIncident { get; set; }
        public List<InvestigationIncident> InvestigationGrievance { get; set; }
        public List<InvestigationLinkVm> InvestigationLink { get; set; }
        public List<IncarcerationForms> InvestigationForms { get; set; }
        
    }
    
    public class InvestigationFlag
    {
        public int InvestigationId { get; set; }
        public int InvestigationFlagIndex { get; set; }
        public int? InvestigationFlagsId { get; set; }
        public string FlagNotes { get; set; }
        public bool DeleteFlag { get; set; }
    }

    public class InvestigationPersonnelVm
    {
        public int InvestigationId { get; set; }
        public int PersonnelId { get; set; }
        public int? InvestigationPersonnelId { get; set; }
        public string PersonnelNote { get; set; }
        public bool DeleteFlag { get; set; }
        public bool NamedOnlyFlag { get; set; }
        public bool ContributerFlag { get; set; }
        public bool ViewerFlag { get; set; }
        public PersonnelVm PersonnelOfficer { get; set; }
    }

    public class InvestigationNotesVm
    {
        public int InvestigationId { get; set; }
        public int? InmateId { get; set; }
        public int? InvestigationNotesId { get; set; }
        public string InvestigationNotes { get; set; }
        public bool DeleteFlag { get; set; }
        public int InvestigationFlagId { get; set; }
        public InmateHousing InmateDetail { get; set; }
        
    }

    public class InvestigationIncident
    {
        public int? InvestigationToIncidentId { get; set; }
        public int InvestigationId { get; set; }
        public int DisciplinaryIncidentId { get; set; }
        public string IncidentReferenceNote { get; set; }
        public bool DeleteFlag { get; set; }
        public string IncidentNumber { get; set; }
        public int? IncidentType { get; set; }
    }

    public enum InvstRange
    {
        active,
        start,
        complete,
        delete,
        receive
    }

    public class InvestigationInputs
    {
        public InvstRange Range { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int InvestigationId { get; set; }
    }

    public class InvestigationLinkVm
    {
        public int InvestigationId { get; set; }
        public int? InvestigationLinkId { get; set; }
        public int LinkTypeId { get; set; }
        public int[] InmateIds { get; set; }
        public DateTime? CreateDate { get; set; }
        public string LinkNotes { get; set; }
        public bool DeleteFlag { get; set; }
        public List<InmateHousing> InmateDetails { get; set; }
    }
}
