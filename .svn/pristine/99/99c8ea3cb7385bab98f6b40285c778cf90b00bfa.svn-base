using System;
using System.Collections.Generic;
using GenerateTables.Models;

namespace ServerAPI.ViewModels
{

    public class VisitorsVisitDetails
    {
        public int VisitToVisitorId { get; set; }
        public int FacilityId { get; set; }
        public int? InmateId { get; set; }
        public int ScheduleId { get; set; }
        public int PersonId { get; set; }
        public int VisitorType { get; set; }
        public PersonInfoVm PrimaryVisitor { get; set; }
        public PersonInfoVm InmateInfo { get; set; }

        public string VisitorIdNumber { get; set; }
        public string VisitorIdState { get; set; }
        public string VisitorIdType { get; set; }
        public string Relationship { get; set; }
        public int VisitorsCount { get; set; }
        public string VisitorNotes { get; set; }
        public string VisitorInmateNotes { get; set; }
        public string visitType { get; set; }
        public string Visitor { get; set; }
        public string Inamte { get; set; }
        public string visitorId { get; set; }
        public bool DeleteFlag { get; set; }

    }

    public class InmateAssignmentDetails
    {
        public int VisitorToInmateId { get; set; }
        public int InmateId { get; set; }
        public int? VisitorId { get; set; }
        public PersonInfoVm InmateInfo { get; set; }
        public PersonInfoVm VisitInfo { get; set; }
        public int? VisitorRelation { get; set; }
        public string VisitorNote { get; set; }
        public DateTime? VisitorNotAllowedExpire { get; set; }
        public string VisitorNotAllowedNote { get; set; }
        public Boolean VisitorNotAllowedFlag { get; set; }
        public int PersonId { get; set; }
        public int InmateActive { get; set; }
        public string HousingUnitLocation { get; set; }
        public string VisitorNotAllowedReson { get; set; }
        public PersonVm InmateDetails { get; set; }
        public int? DeleteFlag { get; set; }


    }

    public class InmateAssignmentinput
    {
        public int InmateId { get; set; }
        public int? visitorId { get; set; }
        public int? VisitorRelation { get; set; }
        public string VisitorNote { get; set; }
        public int VisitorNotAllowedFlag { get; set; }
        public DateTime? VisitorNotAllowedExpire { get; set; }
        public string VisitorNotAllowedNote { get; set; }
        public string VisitorNotAllowedReson { get; set; }

    }
    public class OpenScheduleDetails
    {
        public string VisitationRoom { get; set; }
        public int? RoomCapacity { get; set; }
        public bool OpenScheduleFlag { get; set; }
        public int PendingCount { get; set; }
        public int InProgressCount { get; set; }
        public int PersonId { get; set; }
        public DateTime? VisitAvailDate { get; set; }
    }

    public class BumpedVisitList
    {
        public int VisitToVisitorId { get; set; }
        public bool VisitorBefore { get; set; }
        public int VisitorType { get; set; }
        public DateTime? ScheduledDate { get; set; }
        public DateTime ScheduledStartTime { get; set; }
        public DateTime? ScheduledEndTime { get; set; }
        public TimeSpan ScheduledDuration { get; set; }
        public string BumpedNote { get; set; }
        public string Location { get; set; }
        public string BumpedBy { get; set; }
        public int IsCleared { get; set; }
        public DateTime? BumpedDate { get; set; }
        public int ScheduleId { get; set; }
        public PersonInfoVm PrimaryVisitor { get; set; }
        public PersonInfoVm InmateInfo { get; set; }
        public DateTime ScheduleDateTime { get; set; }
        public DateTime CreateDate { get; set; }
        public int FacilityId { get; set; }
        public bool FrontDeskFlag { get; set; }
        public int? BumpVisitId { get; set; }
        public AoWizardProgressVm RegistrationProgress { get; set; }

        public string VisitorIdType { get; set; }
        public int VisitorId { get; set; }
    }
    public class ClearBumpedVisit
    {
        public string ClearNote { get; set; }
        public int ScheduledId { get; set; }
    }
    public class RevisitReturnParams
    {
        public int VisitToVisitorId { get; set; }
        public int ScheduleId { get; set; }
        public int WizardProgressId { get; set; }
        public AoWizardProgressVm RegistrationProgress { get; set; }
    }
}