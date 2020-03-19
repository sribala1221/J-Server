using System;
using System.Collections.Generic;
using ScheduleWidget.Common;

namespace ServerAPI.ViewModels
{
    public class AppointmentViewerVm
    {
        public int ScheduleId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public int? UpdatedBy { get; set; }
        public DateTime? EndDate { get; set; }
        public int? InmateId { get; set; }
        public int LocationId { get; set; }
        public int? TypeId { get; set; }
        public int? ReasonId { get; set; }
        public string LocationDetail { get; set; }
        public string Notes { get; set; }
        public TimeSpan Duration { get; set; }
        public bool IsSingleOccurrence { get; set; }
        public int Hour { get; set; }
        public DayInterval DayInterval { get; set; }
        public WeekInterval WeekInterval { get; set; }
        public FrequencyType FrequencyType { get; set; }
        public MonthOfQuarterInterval MonthOfQuarterInterval { get; set; }
        public QuarterInterval QuarterInterval { get; set; }
        public TimeSpan Time { get; set; }
        public MonthOfYear MonthOfYear { get; set; }
        public int? DayOfMonth { get; set; }
        public int? AgencyCourtDeptId { get; set; }
        public int? AgencyId { get; set; }
        public string DeleteReason { get; set; }
        public string Location { get; set; }
        public string Court { get; set; }
        public string Dept { get; set; }
        public int? TrackId { get; set; }
        public int FacilityId { get; set; }
        public DateTime? DeleteDate { get; set; }
        public bool DeleteFlag { get; set; }
        public int? DeleteBy { get; set; }
        public int? DispInmateId { get; set; }
        public InmateHousing InmateDetails { get; set; }
    }

    public class DarkDayInfo
    {
        public int DarkDayId { get; set; }
        public string Description { get; set; }
        public DateTime DarkDayDate { get; set; }
        public string Notes { get; set; }
        public bool DeleteFlag { get; set; }
    }

    public class DarkDayVm
    {
        public List<DarkDayInfo> LstDarkDays { get; set; }
        public List<PrivilegesVm> LstLocation { get; set; }
    }
}
