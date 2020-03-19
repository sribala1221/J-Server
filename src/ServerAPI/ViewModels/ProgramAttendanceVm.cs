using System;
using System.Collections.Generic;
using GenerateTables.Models;

namespace ServerAPI.ViewModels
{
    public class ProgramAttendanceCourseVm : ScheduleVm
    {

        public bool AttendFlag { get; set; }
        public bool NotAttendFlag { get; set; }

        public DateTime? OccuranceDate { get; set; }

        public int ProgramAttendId { get; set; }

        public int ProgramAttendInmateId { get; set; }

        public List<ProgramAttendanceCourseVm> ProgramAttendList { get; set; }

        public List<ProgramAttendanceCourseVm> ProgramAttendInmateList { get; set; }
    }

    public class ProgramAttendanceDetails
    {
        public PersonInfoVm InmateInfo { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public HousingDetail HousingDetails { get; set; }
        public int RequestId { get; set; }
        public int ScheduleId { get; set; }
        public int InmateId { get; set; }
        public int ProgramClassAssignId { get; set; }
        public List<ProgramAttendanceCourseVm> ProgramAttendInmateList { get; set; }
        public List<ProgramAttendanceCourseVm> ProgramAttendList { get; set; }

    }

}