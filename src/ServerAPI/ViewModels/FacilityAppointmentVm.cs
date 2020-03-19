using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServerAPI.ViewModels
{
   public class FacilityApptVm
    {
        public InmateApptFlag InmateApptFlag { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public TimeSpan? FromTime { get; set; }
        public TimeSpan? ToTime { get; set; }
        public int? CourtAgencyId { get; set; }
        public int? CourtAgencyDeptId { get; set; }
        public int? ApptTypeIndex { get; set; }
        public int? FacilityId { get; set; }
        public int? LocationId { get; set; }
        public string HousingLocation { get; set; }
        public string HousingNumber { get; set; }
        public int? HousingUnitListId { get; set; }
        public int? InmateId { get; set; }
        public string ApptReason { get; set; }
        public bool? IsReleased { get; set; }
        public bool DeleteFlag { get; set; }
        public int? ProgramId { get; set; }
        public string FacilityEvent { get; set; }
        public int? PersonnelId { get; set; }
        public bool IsInmateSearch { get; set; }
        public bool IsCourtSearch { get; set; }
        public bool IsProgramSearch { get; set; }
        public bool IsFacilityEventSearch { get; set; }
        public string VisitorLocation { get; set; }
        public string VisitationDay { get; set; }
        public bool ScheduleFlag { get; set; }
        public List<string> PrivilegsList { get; set; }
        public List<PrivilegeDetailsVm> PrivilegsDetails { get; set; }
        public string LastNameChar { get; set; }
    }

    public class AppointmentOverallList
    {
        public string ApptTime { get; set; }
        public string Duration { get; set; }
        public int ApptId { get; set; }
        public int? ProgramId { get; set; }
        public DateTime? ApptDate { get; set; }
        public DateTime? ApptEndDate { get; set; }
        public int? ApptDurationMin { get; set; }
        public string ApptReason { get; set; }
        public int? ApptReasonId { get; set; }
        public string ApptNotes { get; set; }
        public string ApptType { get; set; }
        public int? ApptTypeId { get; set; }
        public int? CourtAgenyId { get; set; }
        public string CourtAgencyName { get; set; }
        public int? CourtAgencyDeptId { get; set; }
        public string CourtAgencyDeptName { get; set; }
        public List<string> ArrestBookingNo { get; set; }
        public string AgencyAbbreviation { get; set; }
        public InmateDetailsList InmateDetails { get; set; }
        public InmateApptFlag InmateApptFlag { get; set; }
        public int ApptDeleteFlag { get; set; }
        public int? ApptOccurId { get; set; }
        public int CreatedBy { get; set; }
        public int? ApptLocationId { get; set; }
        public string ApptLocation { get; set; }
        public int? Count { get; set; }
        public int InmateId { get; set; }
        public bool ApptRequireCourtLink { get; set; }

        public int? FacilityId { get; set; }
        public string FacilityEventName { get; set; }
        public string HousingLocation { get; set; }
        public string HousingNumber { get; set; }
        public string AppointmentPlace { get; set; }
        public int? FacilityEventId { get; set; }
        public List<AoAppointmentVm> AoappointmentList { get; set; }
    }


    public class AppointmentFilterVm {
        public List<string> HousingUnitLoc { get; set; }
        public List<HousingDropDown> HousingUnitNum { get; set; }
        public List<KeyValuePair<int,string>> Location{ get; set; }
        public List<KeyValuePair<int, string>> AppointmentReason { get; set; }
        public List<KeyValuePair<int, string>> AppointmentType { get; set; }
        public List<KeyValuePair<int, string>> CourtList { get; set; }
        public List<ProgramVm> Program { get; set; }
        public List<CourtDepartmentList> CourtDepartmentList { get; set; }
        public List<KeyValuePair<int, string>> FacilityEvent { get; set; }
    }

   
}
