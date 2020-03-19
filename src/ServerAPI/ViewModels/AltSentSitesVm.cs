using System;

namespace ServerAPI.ViewModels
{
    public class AltSentProgramDetails
    {
        public int AltSentProgramId { get; set; }
        public string AltSentProgramAbbr { get; set; }
        public string FacilityAbbr { get; set; }
    }

    public class SiteApptDetails
    {
        public int SiteNoteId { get; set; }
        public TimeSpan? NoteTime { get; set; }
        public string NoteType { get; set; }
        public int NoteTypeId { get; set; }
        public int? DeleteFlag { get; set; }
        public string SiteNote { get; set; }
        public int? SiteId { get; set; }
        public string SiteName { get; set; }
        public DateTime? ReminderDate { get; set; }
        public DateTime? CreateDate { get; set; }
        public int CreateBy { get; set; }
        public DateTime? AltSentDate { get; set; }
        public PersonInfoVm PersonInfo { get; set; }
        public int InmateId { get; set; }
        public bool ReminderFlag { get; set; }
    }

    public class SiteApptParam
    {
        public int AltSentProgramId { get; set; }
        public bool NotesToday { get; set; }
        public DateTime? NotesDateFrom { get; set; }
        public DateTime? NotesDateTo { get; set; }
        public int? InmateId { get; set; }
        public int? CreateBy { get; set; }
        public int? SiteId { get; set; }
        public string NoteKeyword { get; set; }
        public DateTime? ReminderDateFrom { get; set; }
        public DateTime? ReminderDateTo { get; set; }
    }
}
