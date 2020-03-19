﻿using System;
using System.Collections.Generic;
using GenerateTables.Models;
using ScheduleWidget.Common;

namespace ServerAPI.ViewModels
{
    public class InmateAppointmentCount
    {
        public int ApptLocationId { get; set; }
        public string ApptLocation { get; set; }
        public int? Count { get; set; }
        public int InmateId { get; set; }
        public bool ApptRequireCourtLink { get; set; }
    }

    public class InmateAppointmentList : InmateAppointmentCount
    {
        public string ApptTime { get; set; }
        public string Duration { get; set; }
        public int ApptId { get; set; }
        public DateTime ApptDate { get; set; }
        public DateTime? ApptEndDate { get; set; }
        public int? ApptDurationMin { get; set; }
        public string ApptReason { get; set; }
        public string ApptNotes { get; set; }
        public string ApptType { get; set; }
        public int? AppointmentTypeId { get; set; }
        public string AgencyName { get; set; }
        public string AgencyDept { get; set; }
        public List<string> ArrestBookingNo { get; set; }
        public string AgencyAbbreviation { get; set; }
        public InmateDetailsList InmateDetails { get; set; }
        public InmateApptFlag InmateApptFlag { get; set; }
        public int ApptDeleteFlag { get; set; }
        public int? ApptOccurId { get; set; }
        public TimeSpan ApptDuration { get; set; }
    }

    public class ScheduleList
    {
        public int InmateId { get; set; }
        public int ApptId { get; set; }
        public DateTime ApptDate { get; set; }
        public bool IsRec { get; set; }
    }

    public class InmateAppointmentDetails : InmateAppointmentList
    {
        public string ApptPlace { get; set; }
        public int? ApptTypeId { get; set; }
        public int ApptReoccurFlag { get; set; }
        public int ApptReoccurSunday { get; set; }
        public int ApptReoccurMonday { get; set; }
        public int ApptReoccurTuesday { get; set; }
        public int ApptReoccurWednesday { get; set; }
        public int ApptReoccurThursday { get; set; }
        public int ApptReoccurFriday { get; set; }
        public int ApptmentReoccurSaturday { get; set; }
        public int ApptReoccurMonthday1 { get; set; }
        public int ApptReoccurMonthday2 { get; set; }
        public DateTime? AppointmentReoccurEnddate { get; set; }
        public int? ProgramId { get; set; }
        public string ProgramInstructor { get; set; }
        public int? FacilityEventId { get; set; }
        public int? FacilityEventFacilityId { get; set; }
        public string FacilityEventHousingLocation { get; set; }
        public string FacilityEventHousingNumber { get; set; }
        public int ApptReoccurYearday1 { get; set; }
        public int ApptReoccurYearday2 { get; set; }
        public int ApptReoccurYearday3 { get; set; }
        public int ApptReoccurYearday4 { get; set; }
        public int? HousingUnitListId { get; set; }
        public List<DateTime> ApptOccuranceDate { get; set; }
        public string AppointmentHistoryList { get; set; }
        public InmateApptArrestXref InmateApptArrestXref { get; set; }
        public bool ApptRecheckConflict { get; set; }
        public bool AllDayAppt { get; set; }
        public int? DisciplinaryInmateId { get; set; }
        public DayInterval DayInterval { get; set; }
        public WeekInterval WeekInterval { get; set; }
        public MonthOfQuarterInterval MonthOfQuarterInterval { get; set; }
        public QuarterInterval QuarterInterval { get; set; }
        public FrequencyType Frequency { get; set; }
    }

    public class InmatePgmApptCount
    {
        public int? ProgramId { get; set; }
        public int Count { get; set; }
        public string DayOfWeek { get; set; }
        public int ReoccurDay { get; set; }
        public string ApptLocation { get; set; }
        public InmateApptFlag InmateApptFlag { get; set; }
        public ProgramReoccurEventFlag ReoccurFlag { get; set; }
    }

    public class InmatePgmApptList : InmatePgmApptCount
    {
        public List<string> DayOfWeekList { get; set; }
        public string ProgramCategory { get; set; }
        public string ProgramCategoryId { get; set; }
        public DateTime ReoccurDate { get; set; }
        public DateTime? ApptDate { get; set; }
        public DateTime? ApptEndDate { get; set; }
        public List<int> RecYearList { get; set; }
        public List<int> RecMonthList { get; set; }
        public List<ProgramReoccurEventFlag> AddtionalDateList { get; set; }
        public bool IsAppointmentReoccurDay { get; set; }
        public bool IsAppointmentReoccurMonthDay { get; set; }
        public bool IsAppointmentReoccurYearDay { get; set; }
        public bool IsAppointmentReoccurAddOn { get; set; }
    }

    public class InmateAppointmentVm
    {
        public List<InmateAppointmentList> InmateAppointmentList { get; set; }
        public List<InmateAppointmentCount> InmateAppointmentCount { get; set; }
        public List<InmatePgmApptCount> InmatePgmApptCount { get; set; }
        public List<InmatePgmApptList> InmatePgmApptList { get; set; }
        public List<InmateCourtApptCount> InmateCourtApptCount { get; set; }
    }

    public class DeleteUndoInmateAppt
    {
        public int ScheduleId { get; set; }
        public bool IsRecurrence { get; set; }
        public DateTime? ScheduleDate { get; set; }
        public int? ApptOccurId { get; set; }
        public string DeleteReason { get; set; }
        public string DeleteNote { get; set; }
    }

    public class AppointmentLocationVm
    {
        public List<LocationList> LocationList { get; set; }
        public List<KeyValuePair<int, string>> AppointmentReason { get; set; }
        public List<KeyValuePair<int, string>> AppointmentType { get; set; }
        public List<KeyValuePair<int, string>> CourtLocation { get; set; }
        public List<CourtDepartmentList> CourtDepartmentList { get; set; }
        public List<BookingDetails> BookingDetails { get; set; }
        public List<ProgramAndClass> Programs { get; set; }
    }

    public enum ApptLocationFlag
    {
        None,
        Visitation,
        Transfer,
        Schedule
    }

    public class LocationList
    {
        public int ApptLocationId { get; set; }
        public bool ApptRequireCourtLink { get; set; }
        public string ApptLocation { get; set; }
        public bool ApptAllowFutureEndDate { get; set; }
        public bool ApptAlertEndDate { get; set; }
        public int? DefaultCourtId { get; set; }
    }

    public class BookingDetails
    {
        public int ArrestId { get; set; }
        public string ArrestBookingNo { get; set; }
        public string ArrestTypeId { get; set; }
        public string ArrestType { get; set; }
        public string ArrestCourtDocket { get; set; }
        public int? ArrestCourtJurisdictionId { get; set; }
        public int? ArrestActive { get; set; }
        public string ArrestCaseNumber { get; set; }
        public string AgencyName { get; set; }
        public string AgencyAbbr { get; set; }
        public string ArrestingAgencyAbbr { get; set; }
        public DateTime? ArrestDate { get; set; }
        public bool ActiveIncarceration { get; set; }
        public DateTime? ReleaseOut { get; set; }

    }

    public class AppointmentHistoryList
    {
        public int AppointmentHistoryId { get; set; }
        public DateTime? CreateDate { get; set; }
        public string PersonLastName { get; set; }
        public string OfficerBadgeNumber { get; set; }
        public string ApptHistoryList { get; set; }
        public List<AppointmentHeader> Header { get; set; }
        public int PersonId { get; set; }
    }

    public class InmateAppointmentHistoryVm
    {
        public List<AppointmentHistoryList> ApptHistoryList { get; set; }
        public InmateDetailsList InmateDetails { get; set; }
    }

    public class InmateDetailsList
    {
        public string InmateNumber { get; set; }
        public int PersonId { get; set; }
        public int InmateId { get; set; }
        public int FacilityId { get; set; }
        public string FacilityAbbr { get; set; }
        public int HousingUnitId { get; set; }
        public string PersonFirstName { get; set; }
        public string PersonLastName { get; set; }
        public string PersonMiddleName { get; set; }
        public int? HousingUnitCapacity { get; set; }
        public DateTime? PersonDob { get; set; }
        public string HousingLocation { get; set; }
        public string HousingNumber { get; set; }
        public string HousingUnitBedNumber { get; set; }
        public string HousingUnitBedLocation { get; set; }
        public string ClassificationReason { get; set; }
        public int? InmateActive { get; set; }
        public string TrackLocation { get; set; }
        public string PhotoPath { get; set; }
        public int HousingUnitListId { get; set; }
        public int? PersonSexLast { get; set; }
    }

    public class AppointmentConflictDetails
    {
        public int ApptId { get; set; }
        public int ApptReoccurFlag { get; set; }
        public DateTime? ApptStartDate { get; set; }
        public DateTime? ApptEndDate { get; set; }
        public int? SchdAnytime { get; set; }
        public string ApptDayOfWeek { get; set; }
        public string ConflictApptLocation { get; set; }
        public string ConflictType { get; set; }
        public string ProgramCategoryDesc { get; set; }
        public string ProgramCategoryId { get; set; }
        public int InmateId { get; set; }
        public AppointmentBump ConfBumpList { get; set; }
        public PersonInfoVm Person { get; set; }
        public string Description { get; set; }
        public string Notes { get; set; }
    }

    public class AppointmentBump
    {
        public int? HierarchyId { get; set; }
        public int? AllowBump { get; set; }
        public string HierarchyName { get; set; }
    }

    public class AppointmentHeader
    {
        public string Header { get; set; }
        public string Detail { get; set; }
    }

    public enum InmateApptFlag
    {
        Appointment,
        Visitation,
        Program,
        FacilityEvent,
        Inmate,
        Court
    }

    public enum ProgramReoccurEventFlag
    {
        ReoccurDay,
        ReoccurMonthDay,
        ReoccurYearDay,
        ReoccurAddOn
    }

    public class CourtDepartmentList
    {
        public int AgencyId { get; set; }
        public int AgencyCourtDeptId { get; set; }
        public string DepartmentName { get; set; }
    }

    public class InmateApptArrestXref
    {
        public List<int?> ArrestId { get; set; }
        public string ApptLocation { get; set; }
        public int AgencyId { get; set; }
        public int? AgencyCourtDeptId { get; set; }
        public string AgencyName { get; set; }
        public string AgencyDept { get; set; }
        public string AgencyAbbreviation { get; set; }
        public string ArrestBookingNo { get; set; }
    }

    public class InmateCourtApptCount
    {
        public string ArrestBookingNo { get; set; }
        public string AgencyAbbreviation { get; set; }
        public string AgencyName { get; set; }
        public int BookingCount { get; set; }
        public string AgencyDeptId { get; set; }
        public string ApptLocation { get; set; }
    }

    public class AppointmentConflictCheck
    {
        public int? ApptLocationId { get; set; }
        public string ApptLocation { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? ApptDate { get; set; }
        public AppointmentBump ApptBumpList { get; set; }
        public List<AppointmentConflictDetails> ApptConflictDetails { get; set; }

        public int ScheduleId { get; set; }
    }

    // This class will be used for global i.e. left grid info
    public class GridInfoVm
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Cnt { get; set; }
    }

    // BumpQueue Classes
    public class BumpQueue
    {
        public int ApptId { get; set; }
        public int? InmateId { get; set; }
        public bool InmateActive { get; set; }
        public bool ClearFlag { get; set; }
        public bool BumpClearFlag { get; set; }
        public int? NewAppointmentId { get; set; }
        public DateTime? ApptDate { get; set; }
        public DateTime? ApptEndDate { get; set; }
        public int? ApptDuration { get; set; }
        public string InmateLastName { get; set; }
        public string InmateFirstName { get; set; }
        public string InmateMiddleName { get; set; }
        public string ApptNotes { get; set; }
        public string ApptLocation { get; set; }
        public int? BumpBy { get; set; }
        public DateTime? BumpDate { get; set; }
        public string BumpLastName { get; set; }
        public string BumpBadgeNumber { get; set; }
        public int? ClearBy { get; set; }
        public DateTime? ClearDate { get; set; }
        public string ClearLastName { get; set; }
        public string ClearBadgeNumber { get; set; }

        public string ClearNote { get; set; }

        public int? HousingUnitId { get; set; }

        public bool DeleteFlag { get; set; }
        public bool BumpFlag { get; set; }
        public int PersonId { get; set; }
        public string VisitorTimeIn { get; set; }
        public string VisitorTimeOut { get; set; }
        public int VisitToVisitorId { get; set; }
        public bool VisitorBefore { get; set; }
        public int VisitorType { get; set; }
        public PersonInfoVm PrimaryVisitor { get; set; }
        public bool FrontDeskFlag { get; set; }
        public string VisitorIdType { get; set; }
        public int? BumpVisitId { get; set; }
        public int VisitorId { get; set; }
        public HousingDetail HousingUnit { get; set; }
        public string InmateNumber { get; set; }
        public AoWizardProgressVm RegistrationProgress { get; set; }
       
    }

    public class BumpQueueVm
    {
        public string[] LstBuilding { get; set; }
        public List<GridInfoVm> LstGridInfo { get; set; }
        public List<BumpQueue> LstBumpQueue { get; set; }
    }

    public class AppointmentClass
    {
        public int ApptId { get; set; }
        public string ApptLocation { get; set; }
        public int? ApptLocationId { get; set; }
        public DateTime ApptDate { get; set; }
        public DateTime? ApptEndDate { get; set; }
        public DateTime? ApptReoccurDate { get; set; }
        public int? ApptDurationMin { get; set; }
        public int? ApptRecurFlag { get; set; }
        public int? ProgramId { get; set; }
        public string ClassName { get; set; }
        public string ProgramCategory { get; set; }
        public string StartDateHours { get; set; }
        public string EndDateHours { get; set; }
        public int? InmateId { get; set; }
        public bool IsSingleOccurrence { get; set; }
        public FrequencyType FrequencyType { get; set; }
        public QuarterInterval QuarterInterval { get; set; }
        public MonthOfYear MonthOfYear { get; set; }
        public int? DayOfMonth { get; set; }
        public DayInterval DayInterval { get; set; }
        public WeekInterval WeekInterval { get; set; }
        public MonthOfQuarterInterval MonthOfQuarterInterval { get; set; }
        public string Discriminator { get; set; }
        public TimeSpan Duration { get; internal set; }
        public bool DeleteFlag { get; set; }
    }

    public class IncidentAppointmentVm
    {

        public int AppointmentId { get; set; }
        public int DisciplinaryInmateId { get; set; }
        public int Duration { get; set; }
        public string Location { get; set; }
        public DateTime? ApptStartDate { get; set; }
        public DateTime? ApptEndDate { get; set; }
        public DateTime? ScheduleHearingDate { get; set; }
        public PersonDetailVM PersonDetail { get; set; }
        public string Description { get; set; }
        public string DisciplinaryNumber { get; set; }
        public int? DisciplinaryType { get; set; }
        public bool DisciplinaryHearingHold { get; set; }
        public string DisciplinaryHearingHoldReason { get; set; }
    }

    public class ApptSchedule
    {
        public int ScheduleId { get; set; }
        public int InmateId { get; set; }
        public int LocationId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public TimeSpan Duration { get; set; }
        public bool IsSingleOccurrence { get; set; }
        public DayInterval DayInterval { get; set; }
        public int? DayIntervalValue { get; set; }
        public FrequencyType FrequencyType { get; set; }
    }

    public class AoAppointmentVm
    {
        public int ScheduleId { get; set; }
        public int? InmateId { get; set; }
        public int LocationId { get; set; }
        public int? ReasonId { get; set; }
        public int? TypeId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public TimeSpan Duration { get; set; }
        public InmateDetailsList InmateDetails { get; set; }
        public string Location { get; set; }
        public string Reason { get; set; }
        public string DeleteReason { get; set; }
        public string DeleteReasonNote { get; set; }
        public bool DeleteFlag { get; set; }
        public int? DeletedBy { get; set; }
        public DateTime? DeleteDate { get; set; }
        public PersonnelVm DeletedOfficer { get; set; }
        public string Notes { get; set; }
        public bool IsSingleOccurrence { get; set; }
        public int? ProgramClassId { get; set; }
        public string LocationDetail { get; set; }
        public ApptOccurance ApptOccurance { get; set; }
        public int? AgencyId { get; set; }
        public int? DeptId { get; set; }
        public string ApptType { get; set; }
        public string Court { get; set; }
        public string Dept { get; set; }
        public List<string> BookingNo { get; set; }
        public int FacilityId { get; set; }
        public List<HousingCapacityVm> HousingVmDetails { get; set; }
        public List<KeepSeparateVm> KeepSepDetails { get; set; }
        public int? DispInmateId { get; set; }
        public string Discriminator { get; set; }



        public bool InmateTraked { get; set; }
        public DayInterval DayInterval { get; set; }
        public WeekInterval WeekInterval { get; set; }
        public FrequencyType FrequencyType { get; set; }
        public MonthOfQuarterInterval MonthOfQuarterInterval { get; set; }
        public QuarterInterval QuarterInterval { get; set; }
        public int? DayOfMonth { get; set; }
        public MonthOfYear MonthOfYear { get; set; }
        public bool EndDateNull { get; set; }

        public List<VisitDetails> VisitToAdult { get; set; }
        public List<VisitToChild> VisitToChild { get; set; }
        public VisitDetails PrimaryVisitorDetails { get; set; }
        public int VisitAdditionAdultCount { get; set; }
        public int VisitAdditionChildCount { get; set; }
        public int VisitType { get; set; }
        public string ProfessionalType { get; set; }

        public int? CreateBy { get; set; }
        public PersonnelVm personnelInfo { get; set; }
    }

    public class ApptOccurance
    {
        public int OccuranceId { get; set; }
        public int? DeleteFlag { get; set; }
        public string DeleteReason { get; set; }
        public string DeleteReasonNote { get; set; }
        public DateTime OccuranceDate { get; set; }
        public int? DeleteBy { get; set; }
        public DateTime? DeleteDate { get; set; }
        public int ScheduleId { get; set; }
    }

    public class ScheduleVm
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
        public int? ClassOrServiceNumber { get; set; }
        public string ProgramCategory { get; set; }
        public string ClassOrServiceName { get; set; }
        public string Location { get; set; }
        public string Court { get; set; }
        public string Dept { get; set; }
        public int? TrackId { get; set; }
        public int FacilityId { get; set; }
        public DateTime? DeleteDate { get; set; }
        public bool DeleteFlag { get; set; }
        public int? DeleteBy { get; set; }
        public int? DispInmateId { get; set; }
        public int? ProgramId { get; set; }
        public string Instructor { get; set; }
        public string Discriminator { get; set; }
        public InmateDetailsList InmateDetails { get; set; }
        public string Crn { get; set; }
        public int CourseCapacity { get; set; }

        public int ProgramCourseId { get; set; }
        public string ProgramCourseName { get; set; }
        public string ProgramCourseNumber { get; set; }

        public int? ProgramClassId { get; set; }
        public int PersonnelId { get;  set; }
        public int? CreateBy { get; set; }
    }

    public class ApptDetailsVm : ScheduleVm
    {
        public List<int> InmateIds { get; set; }
        public string AppointmentHistoryList { get; set; }
        public List<int> ArrestId { get; set; }
        public bool ApptRequireCourtLink { get; set; }
        public bool AllDayAppt { get; set; }
        public AppointmentConflictCheck AppointmentConflictCheck { get; set; }
        public bool ApptRecheckConflict { get; set; }

    }

    public class InmateApptDetailsVm
    {
        public List<int> InmateId { get; set; }
        public ScheduleVm AoScheduleDetails { get; set; }

        public string AppointmentHistoryList { get; set; }
        public List<int> ArrestId { get; set; }
        public bool ApptRequireCourtLink { get; set; }
        public bool AllDayAppt { get; set; }
        public AppointmentConflictCheck AppointmentConflictCheck { get; set; }
        public bool ApptRecheckConflict { get; set; }
        public int ScheduleId { get; set; }
        public int? FacilityId { get; set; }
    }

    public class CreateAppointmentRequest
    {
        public int ReasonId { get; set; }
        public int TypeId { get; set; }
        public string LocationDetails { get; set; }
        public string Notes { get; set; }
        public int InmateId { get; set; }
        public Schedule Schedule { get; set; }
    }

    public class ProgramClassScheduleVm
    {
        public List<KeyValuePair<int, string>> LocationList { get; set; }
        public List<ProgramCourseVm> CourseList { get; set; }

        public List<Instructor> InstructorList { get; set; }
        public List<LookupVm> CategoryList { get; set; }

        public ProgramClass ProgramClass { get; set; }

        public List<ProgramClassDetailsVm> ProgramClassList { get; set; }

        public ScheduleVm Schedule { get; set; }
        public bool AllDayAppt { get; set; }
        public int ScheduleId { get; set; }
    }

    public class ProgramCourseVm
    {
        public bool Restrict1Course { get; set; }
        public bool IssueGradeFlag { get; set; }
        public bool GradeFlag { get; set; }

        public string CertificateName { get; set; }
        public bool CertificateFlag { get; set; }

        public string CourseDescription { get; set; }

        public string CourseName { get; set; }

        public string CourseNumber { get; set; }

        public string ProgramCategory { get; set; }
        public int ProgramCourseId { get; set; }
        public bool PassNotPassFlag { get; set; }

        public int ProgramClassId { get; set; }
    }

    public class ProgramClassDetailsVm
    {
        public int ProgramClassId { get; set; }

        public string Crn { get; set; }

        public bool InactiveFlag { get; set; }

        public int ProgramCourseId { get; set; }

        public string CourseName { get; set; }
        public string CourseNumber { get; set; }

        public List<ScheduleVm> ScheduleList { get; set; }

        public int ProgramRequestId { get; set; }

    }

    public class ScheduleSaveHistoryVm
    {
        public int ScheduleId { get; set; }
        public string ScheduleHistoryList { get; set; }
        public DateTime CreatedDate { get; set; }
        public int CreatedBy { get; set; }
        public PersonnelVm CreatedByDetails { get; set; }
    }

    public class ClassEnrollmentInmateDetails
    {
        public InmateEligibilityCount InmateEligibilityCnt { set; get; }
        public List<ClassLog> InmateEligibilityDetails { get; set; }

        public List<ProgramClassDetailsVm> ClassDetailsList { get; set; }
    }

    public class InmateEligibilityCount
    {
        public int Charges { get; set; }
        public int Incident { get; set; }
        public int Privileges { get; set; }
        public int KeepSep { get; set; }
        public int Assoc { get; set; }
        public int Housing { get; set; }
        public int Classify { get; set; }
    }
}
