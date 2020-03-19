using ServerAPI.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServerAPI.ViewModels
{
    public class TasksCountVm
    {
        public int InmateId { get; set; }
        public int TaskLookupId { get; set; }
        public string TaskName { get; set; }
        public int InmateCount { get; set; }
        public string TaskIconPath { get; set; }
        public int TaskPriorityCount { get; set; }
        public string ComponentName { get; set; }
        public int? ComponentId { get; set; }
        public string CompleteNote { get; set; }
        public bool PriorityFlag { get; set; }
        public int? FormTemplateId { get; set; }
        public string FormTemplateName { get; set; }
        public string TaskInstruction { get; set; }
    }

    public enum TaskValidateType
    {
        None,
        IntakeCreateEvent,
        IntakeCompleteEvent,
        BookingCompleteKeeperEvent,
        AssessmentCompleteKeeperEvent,
        FacilityTransferEvent,
        ClearedForRelease,
        BookingCompleteNonKeeperEvent,
        AssessmentCompleteNonKeeperEvent,
        IntakeComplete,
        BookingComplete,
        AssessmentComplete,
        DoRelease,
        HousingAssignFromTransfer
    }

    public class Expedite
    {
        public bool ExpediteBookingFlag { get; set; }
        public string ExpediteBookingReason { get; set; }
        public string ExpediteBookingNote { get; set; }
        public DateTime? ExpediteDate { get; set; }
        public int? ExpediteById { get; set; }
        public PersonnelVm ExpediteBy { get; set; }
        public DateTime? ReleaseOut { get; set; }
        public int VerifyId { get; set; }
    }

    public class TaskOverview : Expedite
    {
        public int InmateId { get; set; }
        public InmateHousing PersonDetails { get; set; }
        public DateTime? CompleteDate { get; set; }
        public int? CompleteById { get; set; }
        public string CompleteNote { get; set; }
        public PersonnelVm CompleteBy { get; set; }
        public int TaskLookupId { get; set; }
        public string TaskName { get; set; }
        public bool PriorityFlag { get; set; }
        public bool NoKeeperFlag { get; set; }
        public DateTime? CreateDate { get; set; }
        public string ComponentName { get; set; }
        public string TaskInstruction { get; set; }
        public int? ComponentId { get; set; }
        public int? ArrestId { get; set; }
        public int? IncarcerationId { get; set; }
        public int IncarcerationArrestXrefId { get; set; }
        public int? FormTemplateId { get; set; }
        public string FormTemplateName { get; set; }
        public int? FormCategoryId { get; set; }
        public List<KeyValuePair<int, string>> FormTemplateDetails { get; set; }
        public string[] TaskValidateLookup { get; set; }
       
    }
    public class AoTaskLookupVm
    {
        public int AoTaskLookupId { get; set; }
        public string TaskName { get; set; }
        public string TaskInstruction { get; set; }
        public int? AoComponentId { get; set; }
    }
}
