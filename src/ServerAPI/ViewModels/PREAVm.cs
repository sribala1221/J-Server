using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServerAPI.ViewModels {
    public class PreaDetailsVm {
        public List<string> ActiveAssociation { get; set; }
        public DateTime? LastReview {get; set; }
        public List<PersonClassificationDetails> AssociationsList { get; set; }
        public List<PreaFlagListVm> PreaFlags { get; set; }
        public List<IncarcerationForms> PreaForms { get; set; }
        public List<PrebookAttachment> PreaAttachments { get; set; }
        public List<PeraInvestigation> InvolvedInvestigations { get; set; }
        public List<PeraIncidentsOrGrievance> InvolvedIncidents { get; set; }
        public List<PeraIncidentsOrGrievance> InvolvedGrievances { get; set; }
        public List<PreaReviewVm> PreaReview { get; set; }
        public List<PreaNotesVm> PreaNotes { get; set; }
    }
   
    public class PreaFlagListVm
    {
        public int PersonId { get; set; }
        public int PersonFlagId { get; set; }
        public int? PreaFlagIndex { get; set; }
        public string PreaFlagType { get; set; }
        public DateTime? PreaFlagDate { get; set; }
        public bool DeleteFlag { get; set; }
        public DateTime? CreateDate { get; set; }
        public string Personlastname { get; set; }
        public string Officerbadgenumber { get; set; }
    }
    public class PreaReviewVm
    {
        public int PreaReviewId { get; set; }
        public int InmateId { get; set; }
        public DateTime PreaReviewDate { get; set; }
        public int PreaReviewFlagId { get; set; }
        public string PreaFlagType { get; set; }
        public string PreaReviewNote { get; set; }
        public DateTime CreateDate { get; set; }        
        public PersonnelVm CreatedBy { get; set; }
        public bool DeleteFlag { get; set; }
        public DateTime UpdateDate { get; set; }
        public PersonnelVm UpdatedBy { get; set; }
        public DateTime DeleteDate { get; set; }
        public int DeleteBy { get; set; }
        }
    public class PreaNotesVm
    {
        public int PreaNotesId { get; set; }
        public int InmateId { get; set; }
        public int InvestigationFlagIndex { get; set; }
        public string PreaNoteType { get; set; }
        public string PreaNote { get; set; }
        public DateTime CreateDate { get; set; }
        public PersonnelVm CreatedBy { get; set; }
        public DateTime? UpadateDate { get; set; }
        public int? UpdateBy { get; set; }
        public bool DeleteFlag { get; set; }
    }

    public class PreaQueueDetailsVm {
        public List<QueueAssocVm> Association { get; set; }
        public List<PreaFlagsCountVm> FlagReview { get; set; }
        public List<PreaFlagsCountVm> Flags { get; set; }
        public List<QueueDetailsVm> QueueDetails { get; set; }
    }

    public class QueueAssocVm
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Count { get; set; }
        public List<int> PersonIds { get; set; }
    }

    public class FlagReviewVm {
        public int FlagReviewType { get; set; }
        public DateTime LastReview { get; set; }
        public int Count { get; set; }
        public List<int> PersonIds { get; set; }
    }

    public class PreaFlagsCountVm
    {
        public int? PreaFlagIndex { get; set; }
        public int Count { get; set; }
        public List<int> PersonIds { get; set; }
        public Dictionary<int, DateTime?> ReviewLast { get; set; }
    }

    public class QueueDetailsVm
    {
        public string InmateNumber { get; set; }
        public int InmateId { get; set; }
        public int IncarcerationId { get; set; }
        public int FacilityId { get; set; }
        public string FacilityAbbr { get; set; }
        public int InmateClassificationId { get; set; }
        public string ClassificationReason { get; set; }
        public int PersonId { get; set; }
        public bool InactiveFlag { get; set; }
        public DateTime? OverallSentStartDate { get; set; }
        public DateTime? OverallFinalReleaseDate { get; set; }
        public DateTime? DateIn { get; set; }
        public HousingDetail HousingDetail { get; set; }
        public PersonInfoVm PersonDetails { get; set; }
        public int PersonClassificationId { get; set; }
        public string ClassificationType { get; set; }
        public DateTime? PreaReviewDate { get; set; }
        public List<PreaReviewVm> PreaReviewLst { get; set; }
        public List<PreaNotesVm> PreaNotesLst { get; set; }
        public List<PersonClassificationDetails> PreaAssocList { get; set; }
    }

    public class PreaQueueSearch
    {
        public int FacilityId { get; set; }
        public string AssocType { get; set; }
        public int? PreaTypeId { get; set; } 
        public List<int> PersonIds { get; set; }
    }

    public class PeraInvestigation
    {
        public string Type { get; set; }
        public DateTime StartDate { get; set; }
        public string InvestigationName { get; set; }
        public string InvestigationNumber { get; set; }
        public string Notes { get; set; }
        public string NoteType { get; set; }
    }

    public class PeraIncidentsOrGrievance
    {
        public int Id { get; set; }
        public string Number { get; set; }
        public string Type { get; set; }
        public DateTime? Date { get; set; }
        public string InvolvedType { get; set; }
    }

}
