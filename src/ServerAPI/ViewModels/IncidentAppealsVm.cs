using System;

namespace ServerAPI.ViewModels
{
    public enum AppealType
    {
        AppealableIncidents,
        ActiveAppeals,
        AppealsForReview,
        AppealHistory
    }

    public enum DateSelection
    {
        Last100,
        ByDateRange,
        AllDays
    }

    public class AppealsParam
    {
        public int? FacilityId { get; set; }
        public AppealType AppealType { get; set; }
        public int? InmateId { get; set; }
        public int? OfficerId { get; set; }
        public int? AppealReason { get; set; }
        public DateSelection DateSelection { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public int? AppealDisposition { get; set; }
        public string IncidentNumber { get; set; }
    }

    public class Appeals
    {
        public int? DisciplinaryInmateAppealId { get; set; }
        public int IncidentId { get; set; }
        public int? DisciplinaryActive { get; set; }
        public int DisciplinaryInmateId { get; set; }
        public DateTime? AppealDateTime { get; set; }
        public int? AppealReasonId { get; set; }
        public string AppealReason { get; set; }
        public string AppealNote { get; set; }
        public DateTime? CreateDate { get; set; }
        public int? InmateId { get; set; }
        public string IncidentNumber { get; set; }
        public int? SendForReview { get; set; }
        public int? ReviewComplete { get; set; }
        public PersonInfoVm PersonInfo { get; set; }
        public DateTime? IncidentDateTime { get; set; }
        public DateTime? DispoDateTime { get; set; }
        public int? ReviewDispoId { get; set; }
        public string ReviewDispo { get; set; }
        public string ReviewNote { get; set; }
        public string ReviewInmateResponse { get; set; }
        public int? ReviewDiscDaysPrior { get; set; }
        public int? ReviewDiscDaysNew { get; set; }
        public int ReportedBy { get; set; }
        public string ReportedByLastName { get; set; }
        public string ReportedByFirstName { get; set; }
        public string ReportedByBadgeNumber { get; set; }
        public int? ReviewBy { get; set; }
        public string ReviewerLastName { get; set; }
        public string ReviewerFirstName { get; set; }
        public string ReviewerBadgeNumber { get; set; }
        public string IncidentType { get; set; }
        public string DisciplinaryAppealRoute { get; set; }
    }

    public class DispInmateAppeal
    {
        public int DisciplinaryInmateId { get; set; }
        public DateTime AppealDate { get; set; }
        public int? AppealReason { get; set; }
        public string AppealNote { get; set; }
        public int ReportedBy { get; set; }
        public int SendForReview { get; set; }
        public int ReviewComplete { get; set; }
        public int? ReviewDiscDaysPrior { get; set; }
        public int? ReviewDiscDaysNew { get; set; }
        public int? ReviewDispo { get; set; }
        public string ReviewNote { get; set; }
        public string ReviewInmateResponse { get; set; }  
          public string DisciplinaryAppealRoute { get; set; }     
    }
}
