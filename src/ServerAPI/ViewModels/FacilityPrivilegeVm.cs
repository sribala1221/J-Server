using System;

namespace ServerAPI.ViewModels
{
    public class FacilityPrivilegeVm : PrivilegeAlertVm
    {
        public FacilityPrivilegeIncidentLinkVm Incident { get; set; }
        public int PersonId { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string Number { get; set; }
        public bool MailPrivilegeFlag { get; set; }
         public bool MailPrivilegeCoverFlag { get; set; }
    }

    public class FacilityPrivilegeIncidentLinkVm
    {
        public int? PrivilegeDiscLinkId { get; set; }
        public string IncidentNumber { get; set; }
        public int? IncidentTypeId { get; set; }
        public string IncidentType { get; set; }
        public DateTime? IncidentDate { get; set; }
        public string InvolvedPartType { get; set; }
        public string ShortSnopsis { get; set; }
        public string ViolationNote { get; set; }
        public string SanctionNote { get; set; }
    }

    public class FacilityPrivilegeInput
    {
        public int FacilityId { get; set; }
        public int OfficerId { get; set; }
        public bool ActiveToday { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
    }

    public class InmatePrivilegeReviewHistoryVm
    {
        public int InmatePrivilegeReviewHistoryId { get; set; }
        public int InmatePrivilegeXrefId { get; set; }
        public DateTime ScheduledReview { get; set; }
        public string ReviewNote { get; set; }
        public int? ReviewInterval { get; set; }
        public DateTime? ReviewNext { get; set; }
        public DateTime? ReviewActual { get; set; }
        public int ReviewBy { get; set; }
        public string PersonLastName { get; set; }
        public string OfficerBadgeNumber { get; set; }
        public string RemovalNote { get; set; }
        public bool ReviewReauthorize { get; set; }
        public bool IsReauthorizeOrUnassign { get; set; }
    }
}
