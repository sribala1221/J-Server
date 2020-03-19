using System;

namespace ServerAPI.ViewModels
{
    public class AlertFlagsVm
    {
        public string Description { get; set; }
        public string AlertColor { get; set; }
        public string FlagNote { get; set; }
        public int? AlertOrder { get; set; }
        public AlertType AlertType { get; set; }
        // Merged Record Alert
        public DateTime? Date { get; set; }
        public PersonnelVm MergeBy { get; set; }
        public PersonInfoVm KeepPerson { get; set; }
        public PersonInfoVm MergePerson { get; set; }
        public string Reason { get; set; }
        public string AssignmentReason { get; set; }
    }

    public class InmateAlertVm
    {
        public string Flag { get; set; }
        public string FlagNote { get; set; }
        public string LookupDescription { get; set; }
        public int? LookupAlertOrder { get; set; }
    }
    public class ObservationLogVm
    {
        public int ObservationScheduleId { get; set; }
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string ColorCode { get; set; }
    }

    public enum AlertType
    {
        // Commented text will be a output text of the properties
        None,
        LocalHold, // Color="#80C0FF" and Description="LOCAL HOLD"
        IllegalAlien, // Color="#000018" and Description="ILLEGAL ALIEN"
        Weekender, // Color="#C0C0FF" and Description="WEEKENDER"
        InCustody, // Color="#C0C0FF" and Description="IN CUSTODY"
        Message, // Color="16777215"/#FFFFFF and Description = "MESSAGE"
        Merge // Color="12632319"/#C0C0FF and Merge record alert
    }
}
