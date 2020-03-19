using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServerAPI.ViewModels
{
    public enum CalendarType
    {
        CourtAppointment= 1,
        Appointment,
        Visitation,
        Programs,
        Services,
        Workcrew,
        Furlough,
        FacilityEvents
    }

    public class CalendarInputs
    {
        public string CalendarTypes { get; set; }
        public int? LocationId { get; set; }
        public string HousingLocation { get; set; }
        public string HousingNumber { get; set; }
        public int? ApptTypeIndex { get; set; }
        public int? ApptReasonIndex { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int? InmateId { get; set; }
        public int FacilityId { get; set; }
        public int? HousingGroupId { get; set; }
        public bool IsFromTransfer { get; set; }
        public bool IsFromViewer { get; set; }

        public int? ProgramClassId { get; set; }
    }
}
