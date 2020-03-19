using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServerAPI.ViewModels
{
    public class AltSentScheduleVm
    {
        public List<KeyValuePair<int, string>> FacilityList { get; set; }
        public List<KeyValuePair<int, string>> ProgramList { get; set; }
        public List<AltSentSchedule> lstAltSentScheduleSites { get; set; }
    }

    public class AltSentSchedule
    {
        public int AltSentSiteId { get; set; }
        public string AltSentSiteName { get; set; }
        public int? AltSentWorkSite { get; set; }
        public int? AltSentTrainingSite { get; set; }
        public int? AltSentReportingSite { get; set; }
        public List<AltSentSiteSchedule> lstAltSentSiteSchedule { get; set; }
    }

    public class AltSentSiteSchedule
    {
        public int AltSentSiteId { get; set; }
        public int AltSentSiteScheduleId { get; set; }
        public int? ScheduleDay { get; set; }
        public TimeSpan? ScheduleTimeFrom { get; set; }
        public TimeSpan? ScheduleTimeThru { get; set; }
        public int? ScheduleCapacity { get; set; }
        public string ScheduleDescription { get; set; }
        public string ScheduleAdditionalEmail { get; set; }
        public string ScheduleAdditionalContact { get; set; }
        public string ScheduleInmateInstructions { get; set; }
        public bool InactiveFlag { get; set; }
        public double AssignCount { get; set; }

    }
}
