using GenerateTables.Models;
using Microsoft.AspNetCore.Http;
using ScheduleWidget.Common;
using ScheduleWidget.Schedule;
using ServerAPI.Interfaces;
using ServerAPI.Utilities;
using ServerAPI.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace ServerAPI.Services
{
    public class ProgramClassService : IProgramClassService
    {

        private readonly AAtims _context;
        private readonly ICommonService _commonService;
        private readonly int _personnelId;

        private ClassFileInmateDetail _inmate;
        private ClassEnrollmentInmateDetails _classEnrollmentDetails;
        private List<Lookup> _lookUp;
        private readonly ISearchService _searchService;
        private readonly IInmateBookingService _inmateBookingService;

        public ProgramClassService(AAtims context, ICommonService commonService,
            ISearchService searchService, IInmateBookingService inmateBookingService, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _commonService = commonService;
            _personnelId = Convert.ToInt32(httpContextAccessor.HttpContext.User
                .FindFirst("personnelId")?.Value);
            _inmateBookingService = inmateBookingService;
            _searchService = searchService;
        }

        //ProgramClass Schedules
        public ProgramClassScheduleVm GetProgramClassSchedules(int? facilityId)
        {
            //To get Appointment Location for New Appointment Entry
            ProgramClassScheduleVm appLocationList = new ProgramClassScheduleVm
            {
                //To get Appointment Location
                LocationList = _context.Privileges
                    .Where(p => p.InactiveFlag == 0 && p.ShowInAppointments
                         && (p.FacilityId == facilityId || p.FacilityId == 0 || !p.FacilityId.HasValue))
                 .OrderBy(x => x.PrivilegeDescription)
                    .Select(p => new KeyValuePair<int, string>(p.PrivilegeId, p.PrivilegeDescription)).ToList(),

                //To get Course List
                CourseList = _context.ProgramCourse
                    .Where(x => !x.DeleteFlag)
                    .OrderBy(x => x.CourseName)
                  .Select(p => new ProgramCourseVm
                  {
                      GradeFlag = p.GradeFlag,
                      IssueGradeFlag = p.IssueGradeFlag,
                      PassNotPassFlag = p.PassNotPassFlag,
                      CertificateFlag = p.CertificateFlag,
                      Restrict1Course = p.Restrict1Course,
                      CertificateName = p.CertificateName,
                      CourseDescription = p.CourseDescription,
                      ProgramCourseId = p.ProgramCourseId,
                      CourseNumber = p.CourseNumber,
                      CourseName = p.CourseName,
                      ProgramCategory = p.ProgramCategory
                  }).ToList(),

                InstructorList = GetProgramInstructorDetails(),

                CategoryList = _commonService.GetLookups(new[] { LookupConstants.PROGRAMCATEGORY }),

                ProgramClassList = _context.ProgramClass
                              .Select(p => new ProgramClassDetailsVm
                              {
                                  ProgramClassId = p.ProgramClassId,
                                  Crn = p.CRN,
                                  InactiveFlag = p.InactiveFlag,
                                  ProgramCourseId = p.ProgramCourseId
                              }).ToList()
            };
            return appLocationList;
        }

        private List<Instructor> GetProgramInstructorDetails()
        {
            List<ProgramInstructorCert> programInstructorCerts = _context.ProgramInstructorCert
                .ToList();
            int[] personnelId = _context.ProgramInstructorAssign
                .Where(a => a.DeleteFlag == 0 || !a.DeleteFlag.HasValue)
                .Select(a => a.PersonnelId).ToArray();

            List<Instructor> instructors = _context.Personnel
                .Where(a => personnelId.All(p => p != a.PersonnelId) && a.ProgramInstructorFlag)
                .Select(a => new Instructor
                {
                    Personnel = new PersonnelVm
                    {
                        PersonnelId = a.PersonnelId,
                        PersonLastName = a.PersonNavigation.PersonLastName,
                        PersonFirstName = a.PersonNavigation.PersonFirstName,
                        OfficerBadgeNumber = a.OfficerBadgeNumber
                    },
                    DeleteFlag = programInstructorCerts.Where(p =>
                       p.PersonnelId == a.PersonnelId).Select(p => p.DeleteFlag)
                    .FirstOrDefault(),
                    ProgramInstructorCertId = programInstructorCerts.Where(p =>
                       p.PersonnelId == a.PersonnelId).Select(p => p.ProgramInstructorCertId)
                    .FirstOrDefault()
                }).OrderBy(a => a.Personnel.PersonLastName)
                .ThenBy(a => a.Personnel.PersonFirstName)
                .ThenBy(a => a.Personnel.OfficerBadgeNumber).ToList();
            return instructors;
        }

        public List<ScheduleVm> GetScheduleDetailsList(int programClassId)
        {
            //To get Class List Details
            List<ScheduleVm> schList = _context.ScheduleProgram
                .Where(n => n.ProgramClassId == programClassId
                 && !n.ProgramClass.DeleteFlag && !n.ProgramClass.InactiveFlag)
                .Select(sch => new ScheduleVm
                {
                    StartDate = sch.StartDate,
                    EndDate = sch.EndDate,
                    LocationId = sch.LocationId ?? 0,
                    ScheduleId = sch.ScheduleId,
                    Duration = sch.Duration,
                    DeleteReason = sch.DeleteReason,
                    IsSingleOccurrence = sch.IsSingleOccurrence,
                    LocationDetail = sch.LocationDetail,
                    DayInterval = sch.DayInterval,
                    WeekInterval = sch.WeekInterval,
                    FrequencyType = sch.FrequencyType,
                    QuarterInterval = sch.QuarterInterval,
                    MonthOfQuarterInterval = sch.MonthOfQuarterInterval,
                    MonthOfYear = sch.MonthOfYear,
                    DayOfMonth = sch.DayOfMonth,
                    ProgramClassId = sch.ProgramClassId,
                    Crn = sch.ProgramClass.CRN,
                    CourseCapacity = sch.ProgramClass.CourseCapacity,
                    ProgramCourseId = sch.ProgramClass.ProgramCourseId,
                    ProgramCourseNumber = sch.ProgramClass.ProgramCourseIdNavigation.CourseNumber,
                    ProgramCourseName = sch.ProgramClass.ProgramCourseIdNavigation.CourseName,
                    ProgramCategory = sch.ProgramClass.ProgramCourseIdNavigation.ProgramCategory,
                    DeleteFlag = sch.DeleteFlag
                }).ToList();
            return schList;
        }

        private AppointmentConflictCheck CheckProgramClassConflict(ProgramClassScheduleVm pgmClassDetails)
        {
            ScheduleVm aoSchedule = pgmClassDetails.Schedule;

            //To Check Appointment Conflict 
            Debug.Assert(aoSchedule.EndDate != null, "AppointmentService: aoSchedule.EndDate != null");
            DateTime endDate = aoSchedule.EndDate.Value;
            endDate = pgmClassDetails.AllDayAppt ? endDate.Date.AddHours(23).AddMinutes(59) : endDate;

            CalendarInputs inputs = new CalendarInputs
            {
                CalendarTypes = "1,2,3",
                FromDate = aoSchedule.StartDate,
                ToDate = endDate,
                ProgramClassId = aoSchedule.ProgramClassId,
                LocationId = aoSchedule.LocationId
            };

            List<AoAppointmentVm> lstApptDetails = CheckProgramClass(inputs);

            List<AppointmentConflictDetails> lstofReoccurAppt = new List<AppointmentConflictDetails>();

            foreach (DateTime day in EachDay(aoSchedule.StartDate, endDate))
            {
                //Appointment Conflict Check for Reoccurence
                lstofReoccurAppt = lstApptDetails.Where(app => app.EndDate.HasValue
                        && (app.EndDate == null || app.EndDate.Value.Date >= day.Date) &&
                        (aoSchedule.StartDate < day.Date.Add(app.EndDate.Value.TimeOfDay) &&
                            aoSchedule.EndDate > day.Date.Add(app.StartDate.TimeOfDay) ||
                            app.StartDate.ToString(DateConstants.HOURSMINUTES) == DateConstants.STARTHRSMINUTES
                            && app.EndDate.Value.ToString(DateConstants.HOURSMINUTES) == DateConstants.STARTHRSMINUTES))
                    .Select(app => new AppointmentConflictDetails
                    {
                        ApptId = app.ScheduleId,
                        ConflictType = ConflictTypeConstants.PROGRAMOVERLAPCONFLICT,
                        ConflictApptLocation = app.Location,
                        ApptStartDate = app.StartDate,
                        ApptEndDate = app.EndDate,
                        ApptReoccurFlag = app.IsSingleOccurrence ? 0 : 1
                    }).ToList();
            }

            AppointmentConflictCheck apptConflictCheck = new AppointmentConflictCheck
            {
                ApptLocation = aoSchedule.Location,
                ApptLocationId = aoSchedule.LocationId,
                EndDate = pgmClassDetails.AllDayAppt ? aoSchedule.StartDate : endDate,
                ApptDate = aoSchedule.StartDate,
                ApptConflictDetails = lstofReoccurAppt
            };

            if (pgmClassDetails.ScheduleId > 0)
            {
                apptConflictCheck.ApptConflictDetails.ToList().ForEach(item =>
                {
                    if (item.ApptId == pgmClassDetails.ScheduleId)
                    {
                        apptConflictCheck.ApptConflictDetails.Remove(item);
                    }
                });
            }

            return apptConflictCheck;
        }

        private List<AoAppointmentVm> CheckProgramClass(CalendarInputs inputs)
        {
            List<AoAppointmentVm> inmateApptList = new List<AoAppointmentVm>();

            //List<ScheduleVm> lstSchedule = new List<ScheduleVm>();

            List<ScheduleVm> lstSchedule = _context.ScheduleProgram
                .Where(a => a.ProgramClassId == inputs.ProgramClassId && !a.DeleteFlag)
                .Select(s => new ScheduleVm
                {
                    ScheduleId = s.ScheduleId,
                    LocationId = s.LocationId ?? 0,
                    Location = s.Location.PrivilegeDescription,
                    StartDate = s.StartDate,
                    EndDate = s.EndDate,
                    IsSingleOccurrence = s.IsSingleOccurrence,
                    DayInterval = s.DayInterval,
                    WeekInterval = s.WeekInterval,
                    FrequencyType = s.FrequencyType,
                    QuarterInterval = s.QuarterInterval,
                    MonthOfQuarterInterval = s.MonthOfQuarterInterval,
                    MonthOfYear = s.MonthOfYear,
                    DayOfMonth = s.DayOfMonth,
                    ProgramClassId = s.ProgramClassId
                }).ToList();

            List<AoAppointmentVm> lstScheduleProgram = lstSchedule.Where(a =>
                    a.LocationId == inputs.LocationId &&
                    (a.IsSingleOccurrence && a.EndDate.HasValue &&
                        a.StartDate.Date == a.EndDate.Value.Date &&
                        inputs.FromDate.Date <= a.StartDate.Date && inputs.ToDate.Date >= a.StartDate.Date ||

                     !a.IsSingleOccurrence &&
                     (inputs.FromDate.Date >= a.StartDate.Date &&
                         inputs.ToDate.Date <= (a.EndDate ?? inputs.ToDate.Date) ||
                         inputs.FromDate.Date <= (a.EndDate ?? inputs.ToDate.Date) &&
                         inputs.ToDate.Date > a.StartDate.Date)

                     || a.IsSingleOccurrence && a.EndDate.HasValue && a.StartDate.Date != a.EndDate.Value.Date &&
                        a.EndDate.Value.Date >= inputs.FromDate.Date
                    )).Select(sch => new AoAppointmentVm
                    {
                        StartDate = sch.StartDate,
                        EndDate = sch.EndDate,
                        LocationId = sch.LocationId,
                        Location = sch.Location,
                        ScheduleId = sch.ScheduleId,
                        IsSingleOccurrence = sch.IsSingleOccurrence,
                        DayInterval = sch.DayInterval,
                        WeekInterval = sch.WeekInterval,
                        FrequencyType = sch.FrequencyType,
                        QuarterInterval = sch.QuarterInterval,
                        MonthOfQuarterInterval = sch.MonthOfQuarterInterval,
                        MonthOfYear = sch.MonthOfYear,
                        DayOfMonth = sch.DayOfMonth
                    }).ToList();


            int[] scheduleIds = lstScheduleProgram.Select(ii => ii.ScheduleId).ToArray();

            foreach (AoAppointmentVm sch in lstScheduleProgram)
            {
                ScheduleBuilder schBuilder = new ScheduleBuilder();
                schBuilder.StartDate(sch.StartDate);
                if (sch.EndDate.HasValue)
                {
                    schBuilder.EndDate(sch.EndDate.Value);
                }

                ISchedule materializedSchedule = schBuilder
                    .Duration(sch.Duration)
                    .SingleOccurrence(sch.IsSingleOccurrence)
                    .OnDaysOfWeek(sch.DayInterval)
                    .DuringMonth(sch.WeekInterval)
                    .HavingFrequency(sch.FrequencyType)
                    .SetMonthOfYear(sch.MonthOfYear)
                    .SetDayOfMonth(sch.DayOfMonth ?? 0)
                    .DuringMonthOfQuarter(sch.MonthOfQuarterInterval)
                    .DuringQuarter(sch.QuarterInterval)
                    .Create();

                Debug.Assert(materializedSchedule.EndDate != null,
                    "AppointmentService: materializedSchedule.EndDate != null");
                DateRange during = new DateRange(materializedSchedule.StartDate,
                    sch.EndDate.HasValue ? materializedSchedule.EndDate.Value : DateTime.Now.AddYears(5));

                if (materializedSchedule.IsSingleOccurrence)
                {
                    AoAppointmentVm app = sch;
                    app.StartDate = materializedSchedule.StartDate;
                    app.EndDate = materializedSchedule.EndDate;
                    app.Duration = materializedSchedule.EndDate - materializedSchedule.StartDate ??
                                 new TimeSpan(0, 0, 0);
                    inmateApptList.Add(app);
                }
                else
                {
                    inmateApptList.AddRange(materializedSchedule.Occurrences(during)
                        .Select(date => new AoAppointmentVm
                        {
                            StartDate = date.Date.Add(during.StartDateTime.TimeOfDay),
                            Duration = sch.Duration,
                            EndDate = sch.EndDate.HasValue
                                    ? date.Date.Add(during.EndDateTime.TimeOfDay)
                                    : date.Date.Add(during.StartDateTime.TimeOfDay).Add(sch.Duration),
                            EndDateNull = !sch.EndDate.HasValue,
                            LocationId = sch.LocationId,
                            Location = sch.Location,
                            ScheduleId = sch.ScheduleId,
                            IsSingleOccurrence = sch.IsSingleOccurrence
                        }));
                }
            }

            inmateApptList = inmateApptList.Where(w =>
            {
                if (w.EndDate != null && w.IsSingleOccurrence && w.StartDate.Date != w.EndDate.Value.Date)
                {
                    w.EndDate = w.StartDate.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
                }
                return true;
            }).ToList();

            inmateApptList = inmateApptList.Where(w =>
            {
                Debug.Assert(w.EndDate != null, "AppointmentService: w.EndDate != null");
                return inputs.FromDate.Date <= w.StartDate.Date && inputs.ToDate.Date >= w.StartDate.Date ||
                       w.IsSingleOccurrence && w.StartDate.Date != w.EndDate.Value.Date;
            }).ToList();

            //Taking Appointment Occurrence for Occurence list
            List<ApptOccurance> apptOccur = _context.ScheduleExclude.Where(oc =>
                scheduleIds.Contains(oc.ScheduleId)).Select(a => new ApptOccurance
                {
                    OccuranceId = a.ScheduleExcludeId,
                    OccuranceDate = a.ExcludedDate,
                    DeleteReason = a.ExcludeReason,
                    DeleteReasonNote = a.ExcludeNote,
                    ScheduleId = a.ScheduleId
                }).ToList();

            inmateApptList = inmateApptList.Where(i => !apptOccur.Any(a =>
                    a.ScheduleId == i.ScheduleId &&
                    a.OccuranceDate.Date == i.StartDate.Date)).ToList();

            return inmateApptList;
        }

        // TODO Really? Here?
        private static IEnumerable<DateTime> EachDay(DateTime from, DateTime thru)
        {
            for (DateTime day = from.Date; day.Date <= thru.Date; day = day.AddDays(1))
            {
                yield return day;
            }
        }

        public int DeleteProgramClassSchedule(int scheduleId)
        {
            //Delete the appointment
            ScheduleProgram appointment =
                _context.ScheduleProgram.SingleOrDefault(s => s.ScheduleId == scheduleId);

            if (appointment == null) return -1;
            appointment.DeleteFlag = true;
            appointment.DeleteDate = DateTime.Now;
            appointment.DeleteBy = _personnelId;

            _context.ScheduleProgram.Update(appointment);

            return _context.SaveChanges();
        }

        public int InsertProgramClass(ProgramClassScheduleVm pgmClassDetails)
        {
            ProgramClass programClass = new ProgramClass
            {
                ProgramCourseId = pgmClassDetails.ProgramClass.ProgramCourseId,
                CourseCapacity = pgmClassDetails.ProgramClass.CourseCapacity,
                CourseClassificationFilter = pgmClassDetails.ProgramClass.CourseClassificationFilter,
                CourseGenderFilter = pgmClassDetails.ProgramClass.CourseGenderFilter,
                CRN = pgmClassDetails.ProgramClass.CRN,
                ProgramInstructorString = pgmClassDetails.ProgramClass.ProgramInstructorString
            };

            int programClassId = pgmClassDetails.ProgramClass.ProgramClassId;

            if (programClassId > 0) return programClassId;
            _context.ProgramClass.Add(programClass);
            _context.SaveChanges();
            programClassId = programClass.ProgramClassId;

            return programClassId;
        }

        public async Task<AppointmentConflictCheck> InsertProgramClassSchedule(ProgramClassScheduleVm pgmClassDetails)
        {
            AppointmentConflictCheck apptConflictCheck = new AppointmentConflictCheck();
            ////To Check Appointment Conflict 
            if (pgmClassDetails.Schedule.EndDate != null && pgmClassDetails.Schedule.IsSingleOccurrence &&
                pgmClassDetails.Schedule.StartDate.Date == pgmClassDetails.Schedule.EndDate.Value.Date)
            {
                apptConflictCheck = CheckProgramClassConflict(pgmClassDetails);

                if (apptConflictCheck.ApptConflictDetails.Any())
                {
                    return apptConflictCheck;
                }
            }

            ScheduleProgram appointment = new ScheduleProgram();
            if (pgmClassDetails.Schedule.ProgramClassId > 0)
            {
                appointment.CreateDate = DateTime.Now;
                appointment.CreateBy = _personnelId;
                appointment.LocationId = pgmClassDetails.Schedule.LocationId;
                appointment.StartDate = pgmClassDetails.Schedule.StartDate;
                appointment.EndDate = pgmClassDetails.Schedule.EndDate;
                appointment.Duration = pgmClassDetails.Schedule.Duration;
                appointment.IsSingleOccurrence = pgmClassDetails.Schedule.IsSingleOccurrence;
                appointment.Time = pgmClassDetails.Schedule.Time;
                appointment.DayInterval = pgmClassDetails.Schedule.DayInterval;
                appointment.WeekInterval = pgmClassDetails.Schedule.WeekInterval;
                appointment.FrequencyType = pgmClassDetails.Schedule.FrequencyType;
                appointment.MonthOfYear = pgmClassDetails.Schedule.MonthOfYear;
                appointment.DayOfMonth = pgmClassDetails.Schedule.DayOfMonth;
                appointment.QuarterInterval = pgmClassDetails.Schedule.QuarterInterval;
                appointment.MonthOfQuarterInterval = pgmClassDetails.Schedule.MonthOfQuarterInterval;
                appointment.ProgramClassId = pgmClassDetails.Schedule.ProgramClassId;

                _context.ScheduleProgram.Add(appointment);
                _context.SaveChanges();
                apptConflictCheck.ScheduleId = appointment.ScheduleId;
            }

            await _context.SaveChangesAsync();
            return apptConflictCheck;

        }

        public int UpdateProgramClass(ProgramClassScheduleVm pgmClassDetails)
        {
            int programClassId = 0;
            ProgramClass programClass =
                _context.ProgramClass.SingleOrDefault(p =>
                    p.ProgramClassId == pgmClassDetails.ProgramClass.ProgramClassId);

            if (programClass != null && pgmClassDetails.ProgramClass.ProgramClassId > 0)
            {
                programClass.ProgramCourseId = pgmClassDetails.ProgramClass.ProgramCourseId;
                programClass.CourseCapacity = pgmClassDetails.ProgramClass.CourseCapacity;
                programClass.CourseClassificationFilter = pgmClassDetails.ProgramClass.CourseClassificationFilter;
                programClass.CourseGenderFilter = pgmClassDetails.ProgramClass.CourseGenderFilter;
                programClass.CRN = pgmClassDetails.ProgramClass.CRN;
                programClass.ProgramInstructorString = pgmClassDetails.ProgramClass.ProgramInstructorString;

                _context.ProgramClass.Update(programClass);
                _context.SaveChanges();
                programClassId = programClass.ProgramClassId;
            }

            return programClassId;
        }

        public async Task<AppointmentConflictCheck> UpdateProgramClassSchedule(ProgramClassScheduleVm pgmClassDetails)
        {
            AppointmentConflictCheck apptConflictCheck = new AppointmentConflictCheck();
            ////To Check Appointment Conflict 
            if (pgmClassDetails.Schedule.EndDate != null && pgmClassDetails.Schedule.IsSingleOccurrence &&
                pgmClassDetails.Schedule.StartDate.Date == pgmClassDetails.Schedule.EndDate.Value.Date)
            {
                apptConflictCheck = CheckProgramClassConflict(pgmClassDetails);

                if (apptConflictCheck.ApptConflictDetails.Any())
                {
                    return apptConflictCheck;
                }
            }

            ScheduleProgram appointment =
                _context.ScheduleProgram.SingleOrDefault(s => s.ScheduleId == pgmClassDetails.Schedule.ScheduleId);

            if (pgmClassDetails.Schedule.ProgramClassId > 0 && appointment != null)
            {
                appointment.CreateDate = DateTime.Now;
                appointment.CreateBy = _personnelId;
                appointment.LocationId = pgmClassDetails.Schedule.LocationId;
                appointment.StartDate = pgmClassDetails.Schedule.StartDate;
                appointment.EndDate = pgmClassDetails.Schedule.EndDate;
                appointment.Duration = pgmClassDetails.Schedule.Duration;
                appointment.IsSingleOccurrence = pgmClassDetails.Schedule.IsSingleOccurrence;
                appointment.Time = pgmClassDetails.Schedule.Time;
                appointment.DayInterval = pgmClassDetails.Schedule.DayInterval;
                appointment.WeekInterval = pgmClassDetails.Schedule.WeekInterval;
                appointment.FrequencyType = pgmClassDetails.Schedule.FrequencyType;
                appointment.MonthOfYear = pgmClassDetails.Schedule.MonthOfYear;
                appointment.DayOfMonth = pgmClassDetails.Schedule.DayOfMonth;
                appointment.QuarterInterval = pgmClassDetails.Schedule.QuarterInterval;
                appointment.MonthOfQuarterInterval = pgmClassDetails.Schedule.MonthOfQuarterInterval;

                _context.ScheduleProgram.Update(appointment);
                _context.SaveChanges();
                apptConflictCheck.ScheduleId = appointment.ScheduleId;
            }

            await _context.SaveChangesAsync();
            return apptConflictCheck;
        }
        public List<ProgramAppointmentVm> GetCurrentCourse(int facilityId, DateTime fromDate, DateTime toDate)
        {
            List<ProgramAppointmentVm> programAptList = new List<ProgramAppointmentVm>();

            List<ScheduleVm> schList = _context.ScheduleProgram.Where(a =>
                !a.DeleteFlag &&
                (a.IsSingleOccurrence && a.StartDate.Date == a.EndDate.Value.Date &&
                    fromDate.Date <= a.StartDate.Date && toDate.Date >= a.StartDate.Date ||

                    !a.IsSingleOccurrence &&
                    (fromDate.Date >= a.StartDate.Date &&
                        toDate.Date <= (a.EndDate.HasValue ? a.EndDate.Value.Date : toDate.Date) ||
                        fromDate.Date <= (a.EndDate.HasValue ? a.EndDate.Value.Date : toDate.Date) &&
                        toDate > a.StartDate.Date)

                    || a.IsSingleOccurrence && a.StartDate.Date != a.EndDate.Value.Date &&
                    a.EndDate.Value.Date >= fromDate.Date
                )).Select(sch => new ScheduleVm
            {
                StartDate = sch.StartDate,
                EndDate = sch.EndDate,
                LocationId = sch.LocationId ?? 0,
                ScheduleId = sch.ScheduleId,
                Duration = sch.Duration,
                DeleteReason = sch.DeleteReason,
                IsSingleOccurrence = sch.IsSingleOccurrence,
                LocationDetail = sch.LocationDetail,
                DayInterval = sch.DayInterval,
                WeekInterval = sch.WeekInterval,
                FrequencyType = sch.FrequencyType,
                QuarterInterval = sch.QuarterInterval,
                MonthOfQuarterInterval = sch.MonthOfQuarterInterval,
                MonthOfYear = sch.MonthOfYear,
                DayOfMonth = sch.DayOfMonth,
                ProgramId = sch.ProgramClassId,
                Crn = sch.ProgramClass.CRN,
                CourseCapacity = sch.ProgramClass.CourseCapacity,
                ProgramCourseId = sch.ProgramClass.ProgramCourseId,
                ProgramCourseNumber = sch.ProgramClass.ProgramCourseIdNavigation.CourseNumber,
                ProgramCourseName = sch.ProgramClass.ProgramCourseIdNavigation.CourseName,
                ProgramCategory = sch.ProgramClass.ProgramCourseIdNavigation.ProgramCategory,
                Location = sch.Location.PrivilegeDescription
            }).ToList();

            foreach (ScheduleVm sch in schList)
            {
                ScheduleBuilder schBuilder = new ScheduleBuilder();
                schBuilder.StartDate(sch.StartDate);
                if (sch.EndDate.HasValue)
                {
                    schBuilder.EndDate(sch.EndDate.Value);
                }
                else if (!sch.EndDate.HasValue && sch.IsSingleOccurrence)
                {
                    schBuilder.EndDate(sch.StartDate.Date.AddHours((sch.StartDate.TimeOfDay + sch.Duration).Hours)
                        .AddMinutes((sch.StartDate.TimeOfDay + sch.Duration).Minutes));
                }

                ISchedule materializedSchedule = schBuilder
                    .Duration(sch.Duration)
                    .SingleOccurrence(sch.IsSingleOccurrence)
                    .OnDaysOfWeek(sch.DayInterval)
                    .DuringMonth(sch.WeekInterval)
                    .HavingFrequency(sch.FrequencyType)
                    .Create();

                Debug.Assert(materializedSchedule.EndDate != null, "AppointmentService: materializedSchedule.EndDate != null");
                DateRange during = new DateRange(materializedSchedule.StartDate,
                    sch.EndDate.HasValue ? materializedSchedule.EndDate.Value : DateTime.Now.AddYears(5));

                if (materializedSchedule.IsSingleOccurrence)
                {
                    programAptList.Add(new ProgramAppointmentVm
                    {
                        StartDate = materializedSchedule.StartDate,
                        EndDate = materializedSchedule.EndDate,
                        LocationId = sch.LocationId,
                        ScheduleId = sch.ScheduleId,
                        ReasonId = sch.ReasonId,
                        TypeId = sch.TypeId,
                        Duration = materializedSchedule.EndDate - materializedSchedule.StartDate ??
                                   new TimeSpan(0, 0, 0),
                        DeleteReason = sch.DeleteReason,
                        DeleteFlag = sch.DeleteFlag,
                        DeleteDate = sch.DeleteDate,
                        DeletedBy = sch.DeleteBy,
                        Notes = sch.Notes,
                        IsSingleOccurrence = sch.IsSingleOccurrence,
                        LocationDetail = sch.LocationDetail,
                        Location = sch.Location,
                        //FacilityId = sch.FacilityId,
                        CourseName = sch.ProgramCourseName,
                        CourseNumber = sch.ProgramCourseNumber,
                        CRN = sch.Crn,
                        CourseCapacity = sch.CourseCapacity,
                        ProgramCourseId = sch.ProgramCourseId,
                        DayInterval = sch.DayInterval,
                        WeekInterval = sch.WeekInterval,
                        ProgramCategory = sch.ProgramCategory,
                        FrequencyType = sch.FrequencyType,
                        ProgramClassId = sch.ProgramId,

                    });
                }
                else
                {
                    programAptList.AddRange(materializedSchedule.Occurrences(during)
                        .Select(date => new ProgramAppointmentVm
                        {
                            StartDate = date.Date.Add(during.StartDateTime.TimeOfDay),
                            EndDate = sch.EndDate.HasValue
                                ? date.Date.Add(during.EndDateTime.TimeOfDay)
                                : date.Date.Add(during.StartDateTime.TimeOfDay).Add(sch.Duration),
                            LocationId = sch.LocationId,
                            ScheduleId = sch.ScheduleId,
                            ReasonId = sch.ReasonId,
                            TypeId = sch.TypeId,
                            Duration = materializedSchedule.Duration,
                            DeleteReason = sch.DeleteReason,
                            DeleteFlag = sch.DeleteFlag,
                            DeleteDate = sch.DeleteDate,
                            DeletedBy = sch.DeleteBy,
                            Notes = sch.Notes,
                            IsSingleOccurrence = sch.IsSingleOccurrence,
                            LocationDetail = sch.LocationDetail,
                            Location = sch.Location,
                            //FacilityId = sch.FacilityId,
                            CourseName = sch.ProgramCourseName,
                            CourseNumber = sch.ProgramCourseNumber,
                            CRN = sch.Crn,
                            CourseCapacity = sch.CourseCapacity,
                            ProgramCourseId = sch.ProgramCourseId,
                            DayInterval = sch.DayInterval,
                            WeekInterval = sch.WeekInterval,
                            ProgramCategory = sch.ProgramCategory,
                            FrequencyType = sch.FrequencyType,
                            ProgramClassId = sch.ProgramId
                        }));
                }
            }
            return programAptList;
        }

        public List<ProgramAppointmentVm> GetCurrentCourseList(int facilityId, DateTime fromDate, DateTime toDate)
        {
            DateTime todayDate = DateTime.Now;
            List<ProgramAppointmentVm> programAppList = _context.ScheduleProgram.Where(a =>
                 !a.DeleteFlag && (a.StartDate >= todayDate || a.EndDate > todayDate && a.StartDate < todayDate)
                 ).Select(sch => new ProgramAppointmentVm
                 {
                     StartDate = sch.StartDate,
                     EndDate = sch.EndDate,
                     LocationId = sch.LocationId ?? 0,
                     ScheduleId = sch.ScheduleId,
                     Duration = sch.Duration,
                     DeleteReason = sch.DeleteReason,
                     IsSingleOccurrence = sch.IsSingleOccurrence,
                     LocationDetail = sch.LocationDetail,
                     DayInterval = sch.DayInterval,
                     WeekInterval = sch.WeekInterval,
                     FrequencyType = sch.FrequencyType,
                     QuarterInterval = sch.QuarterInterval,
                     MonthOfQuarterInterval = sch.MonthOfQuarterInterval,
                     MonthOfYear = sch.MonthOfYear,
                     DayOfMonth = sch.DayOfMonth,
                     ProgramClassId = sch.ProgramClassId,
                     CRN = sch.ProgramClass.CRN,
                     CourseCapacity = sch.ProgramClass.CourseCapacity,
                     ProgramCourseId = sch.ProgramClass.ProgramCourseId,
                     CourseNumber = sch.ProgramClass.ProgramCourseIdNavigation.CourseNumber,
                     CourseName = sch.ProgramClass.ProgramCourseIdNavigation.CourseName,
                     ProgramCategory = sch.ProgramClass.ProgramCourseIdNavigation.ProgramCategory,
                     ProgramInstructorString = sch.ProgramClass.ProgramInstructorString,
                     CourseClassificationFilter = sch.ProgramClass.CourseClassificationFilter,
                     CourseGenderFilter = sch.ProgramClass.CourseGenderFilter,
                     Location = sch.Location.PrivilegeDescription
                 }).ToList();

            return programAppList;
        }
        public async Task<int> DeleteScheduleProgram(ProgramAppointmentVm program)
        {
            ScheduleProgram courseDetails = _context.ScheduleProgram.SingleOrDefault(p =>
                        p.ScheduleId == program.ScheduleId);
            if (program == null || courseDetails == null) return -1;
            courseDetails.DeleteFlag = program.DeleteFlag;
            courseDetails.DeleteBy = _personnelId;
            courseDetails.DeleteDate = DateTime.Now;

            return await _context.SaveChangesAsync();
        }

        public List<InstructorsVm> GetInstructorsList()
        {
            List<InstructorsVm> lstInstructor = _context.ProgramClass.Select(s => new InstructorsVm
            {

                ProgramClassId = s.ProgramClassId,
                Instructors = s.ProgramInstructorString,
                AssignedCourses = s.ProgramCourseIdNavigation.CourseName,
                Certification = s.ProgramCourseIdNavigation.CertificateName,

            }).ToList();

            int?[] lstOfficerIds = lstInstructor.Select(s => s.Instructors).ToArray();

            List<PersonnelVm> lstByPersonnel = _context.Personnel.Where(a => lstOfficerIds.Contains(a.PersonnelId))
          .Select(a => new PersonnelVm
          {
              PersonLastName = a.PersonNavigation.PersonLastName,
              PersonFirstName = a.PersonNavigation.PersonFirstName,
              PersonMiddleName = a.PersonNavigation.PersonMiddleName,
              PersonnelId = a.PersonnelId,
              OfficerBadgeNumber = a.OfficerBadgeNum,
              PersonnelNumber = a.PersonnelNumber,

          }).ToList();
            lstInstructor.ForEach(item =>
           {
               PersonnelVm instructor = lstByPersonnel.SingleOrDefault(x => x.PersonnelId == item.ProgramClassId);
               item.InstructorsInfo = instructor;
           });
            return lstInstructor;
        }

        //To get inmate details depend on FacilityId
        private void GetInmate(int inmateId) =>
            _inmate = _context.Inmate.Where(i => i.InmateId == inmateId)
                .Select(a => new ClassFileInmateDetail
                {
                    InmateId = a.InmateId,
                    PersonId = a.PersonId,
                    FacilityId = a.FacilityId,
                    HousingUnitId = a.HousingUnitId ?? 0,

                    InmateNumber = a.InmateNumber,
                    InmateActive = a.InmateActive == 1,

                    LastReviewBy = a.LastClassReviewBy ?? 0,
                    LastReviewDate = a.LastClassReviewDate.GetValueOrDefault(),

                    InmateClassificationId = a.InmateClassificationId ?? 0,
                    InmateClassificationReason = a.InmateClassificationId.HasValue
                        ? a.InmateClassification.InmateClassificationReason : string.Empty,

                    HousingUnit = new HousingDetail
                    {
                        HousingUnitId = a.HousingUnitId ?? 0,
                        FacilityId = a.HousingUnitId.HasValue ? a.HousingUnit.FacilityId : 0,
                        HousingUnitListId = a.HousingUnitId.HasValue ? a.HousingUnit.HousingUnitListId : 0,
                        HousingUnitNumber = a.HousingUnitId.HasValue ? a.HousingUnit.HousingUnitNumber : string.Empty,
                        HousingUnitLocation = a.HousingUnitId.HasValue ? a.HousingUnit.HousingUnitLocation : string.Empty,
                        HousingUnitBedNumber = a.HousingUnitId.HasValue ? a.HousingUnit.HousingUnitBedNumber : string.Empty,
                        HousingUnitBedLocation = a.HousingUnitId.HasValue ? a.HousingUnit.HousingUnitBedLocation : string.Empty
                    },
                    Person = new PersonDetail
                    {
                        PersonId = a.PersonId,
                        PersonFirstName = a.Person.PersonFirstName,
                        PersonMiddleName = a.Person.PersonMiddleName,
                        PersonLastName = a.Person.PersonLastName
                    }
                }).SingleOrDefault();

        //Get Lookup List
        private void GetLookUpList() =>
            _lookUp = _context.Lookup.Where(x =>
                x.LookupType == LookupConstants.CLASSLINKTYPE ||
                x.LookupType == LookupConstants.ARRTYPE ||
                x.LookupType == LookupConstants.DISCINTYPE ||
                x.LookupType == LookupConstants.DISCTYPE ||
                x.LookupType == LookupConstants.CLASSGROUP
                ).Select(s => new Lookup
                {
                    LookupType = s.LookupType ?? string.Empty,
                    LookupDescription = s.LookupDescription ?? string.Empty,
                    LookupCategory = s.LookupCategory ?? string.Empty,
                    LookupIndex = s.LookupIndex,
                    LookupFlag10 = s.LookupFlag10,
                    LookupFlag9 = s.LookupFlag9
                }).ToList();

        //To get Inmate details for Class enrollment
        public ClassEnrollmentInmateDetails GetClassEnrollmentInmateDetails(int inmateId)
        {
            //To get data for Enrollment by Inmate
            _classEnrollmentDetails = new ClassEnrollmentInmateDetails
            {
                InmateEligibilityCnt = new InmateEligibilityCount(),
                InmateEligibilityDetails = new List<ClassLog>()
            };
            GetInmate(inmateId);
            GetLookUpList();
            _classEnrollmentDetails.ClassDetailsList = GetClassListDetails(inmateId);
            GetChargesDetails(inmateId);
            GetIncidentDetails(inmateId);
            GetPrivilegeDetails(inmateId);
            GetKeepSeparateDetails(inmateId);
            GetAssociationDetails();
            GetHousingDetails(inmateId);
            GetClassifyDetails(inmateId);

            GetKeepSepDetails();

            return _classEnrollmentDetails;
        }

        private void GetChargesDetails(int inmateId)
        {
            List<int> arrestIds = new List<int>();

            List<InmateIncarcerationDetails> bookingInmateDetails =
                _searchService.GetIncarcerationDetails(inmateId);

            //To get Inmate details from Incarceration table
            bookingInmateDetails.ForEach(item =>
            {
                arrestIds.AddRange(item.IncarcerationArrestXrefDetailLSt
                    .Select(i => i.ArrestId));
            });

            List<BookingSearchSubData> chargesDetails = _inmateBookingService.GetInmateCrimeCharges(arrestIds, BookingChargeType.NONE, false);

            _classEnrollmentDetails.InmateEligibilityDetails.AddRange(bookingInmateDetails
                .Select(id => new ClassLog
                {
                    InmateIncDetails = id,
                    ChargesDetails = chargesDetails
                }).ToList());

            _classEnrollmentDetails.InmateEligibilityCnt.Charges = chargesDetails.Count;
        }

        private void GetIncidentDetails(int inmateId)
        {
            IQueryable<DisciplinaryInmate> inmateDisciplinary =
                _context.DisciplinaryInmate.Where(id => id.InmateId == inmateId && (!id.DeleteFlag.HasValue || id.DeleteFlag == 0));

            //Incident Details
            _classEnrollmentDetails.InmateEligibilityDetails.AddRange(inmateDisciplinary
                .Select(id => new ClassLog
                {
                    Id = id.DisciplinaryIncident.DisciplinaryIncidentId,
                    InmateId = id.InmateId.Value,
                    ClassType = ClassTypeConstants.INCIDENT,
                    ClassDate = id.DisciplinaryIncident.DisciplinaryIncidentDate,
                    IncidentNarrative = new IncidentNarrative
                    {
                        DisciplinaryNumber = id.DisciplinaryIncident.DisciplinaryNumber,
                        DisciplinarySynopsis = id.DisciplinaryIncident.DisciplinarySynopsis,
                        DisciplinaryInmateType = id.DisciplinaryInmateType ?? 0,
                        DisciplinaryType = id.DisciplinaryIncident.DisciplinaryType ?? 0,
                        DisciplinaryTypeValue = _lookUp.SingleOrDefault(l =>
                            l.LookupIndex == id.DisciplinaryIncident.DisciplinaryType &&
                            l.LookupType == LookupConstants.DISCTYPE).LookupDescription,
                        DisciplinaryInmateTypeValue = _lookUp.SingleOrDefault(l =>
                                l.LookupIndex == id.DisciplinaryInmateType &&
                                l.LookupType == LookupConstants.DISCINTYPE)
                            .LookupDescription,

                    },
                    OfficerId = id.DisciplinaryIncident.DisciplinaryOfficerId,
                    DeleteFlag = id.DeleteFlag == 1,
                    HousingDetails = new HousingDetail
                    {
                        HousingLocation = id.DisciplinaryIncident.DisciplinaryLocation,
                        HousingUnitLocation = id.DisciplinaryIncident.DisciplinaryHousingUnitLocation,
                        HousingUnitNumber = id.DisciplinaryIncident.DisciplinaryHousingUnitNumber,
                        HousingUnitBedLocation = id.DisciplinaryIncident.DisciplinaryHousingUnitBed
                    },
                    ReportDate = id.DisciplinaryIncident.DisciplinaryReportDate,
                    ScheduleHearingDate = id.DisciplinaryScheduleHearingDate,
                    DisciplinaryHearingDate = id.DisciplinaryHearingDate,
                    ReviewDate = id.DisciplinaryReviewDate,
                    Sanction = id.DisciplinarySanction,
                    HearingComplete = id.HearingComplete,
                    ByPassHearing = id.DisciplinaryInmateBypassHearing,
                    PersonInfo = _inmate.Person
                }).ToList());

            //Incident Count Details
            _classEnrollmentDetails.InmateEligibilityCnt.Incident =
                _classEnrollmentDetails.InmateEligibilityDetails.Count(c => c.ClassType == ClassTypeConstants.INCIDENT);

        }

        private void GetPrivilegeDetails(int inmateId)
        {
            IQueryable<InmatePrivilegeXref> inmatePrivilegeXref =
                _context.InmatePrivilegeXref.Where(pref => pref.InmateId == inmateId && !pref.PrivilegeRemoveDatetime.HasValue
                                                           && (!pref.PrivilegeExpires.HasValue || pref.PrivilegeExpires >= DateTime.Now));

            //Privilege details
            _classEnrollmentDetails.InmateEligibilityDetails.AddRange(inmatePrivilegeXref
                .Select(xref => new ClassLog
                {
                    Id = xref.InmatePrivilegeXrefId,
                    InmateId = xref.InmateId,
                    ClassDate = xref.CreateDate,
                    ClassType = ClassTypeConstants.PRIVILEGES,
                    OfficerId = xref.PrivilegeOfficerId,
                    PrivilegesNarrative = new PrivilegesNarrative
                    {
                        PrivilegeRemoveOfficerId = xref.PrivilegeRemoveOfficerId,
                        PrivilegeDate = xref.PrivilegeDate,
                        PrivilegeNote = xref.PrivilegeNote,
                        PrivilegeExpires = xref.PrivilegeExpires,
                        PrivilegeId = xref.PrivilegeId,
                        PrivilegeDescription = xref.Privilege.PrivilegeDescription,
                        PrivilegeType = xref.Privilege.PrivilegeType
                    },
                    DeleteFlag = xref.PrivilegeRemoveDatetime.HasValue
                }).ToList());

            //Privilege Count
            _classEnrollmentDetails.InmateEligibilityCnt.Privileges =
                _classEnrollmentDetails.InmateEligibilityDetails.Count(c => c.ClassType == ClassTypeConstants.PRIVILEGES);

        }

        #region Keep Seperate

        private void GetKeepSeparateDetails(int inmateId)
        {
            //Keep Sep Details
            IQueryable<KeepSeparate> keepSep = _context.KeepSeparate.Where(ks1 =>
                ks1.KeepSeparateInmate1Id == inmateId ||
                ks1.KeepSeparateInmate2Id == inmateId);
            IQueryable<KeepSeparate> keepSep1 =
                keepSep.Where(ks1 => ks1.KeepSeparateInmate1Id == inmateId);
            IQueryable<KeepSeparate> keepSep2 =
                keepSep.Where(ks2 => ks2.KeepSeparateInmate2Id == inmateId);
            IQueryable<KeepSepAssocInmate> keepSepAssoc =
                _context.KeepSepAssocInmate.Where(ksa => ksa.KeepSepInmate2Id == inmateId);
            IQueryable<KeepSepSubsetInmate> keepSepSubset =
                _context.KeepSepSubsetInmate.Where(kss => kss.KeepSepInmate2Id == inmateId);

            if (!keepSep.Any() && !keepSepAssoc.Any() && !keepSepSubset.Any())
            {
                return;
            }

            //count
            if (keepSep1.Any())
            {
                List<KeepSeparate> keepSep1Lst = keepSep1.Where(c => c.InactiveFlag == 0).ToList();
                _classEnrollmentDetails.InmateEligibilityCnt.KeepSep = keepSep1Lst.Count;

                KeepSep1(keepSep1Lst);

            }
            if (keepSep2.Any())
            {
                List<KeepSeparate> keepSep2Lst = keepSep2.Where(c => c.InactiveFlag == 0).ToList();
                _classEnrollmentDetails.InmateEligibilityCnt.KeepSep += keepSep2Lst.Count;

                KeepSep2(keepSep2Lst);

            }
            if (keepSepAssoc.Any())
            {
                List<KeepSepAssocInmate> keepSepAssocLst = keepSepAssoc.Where(c => c.DeleteFlag == 0).ToList();
                _classEnrollmentDetails.InmateEligibilityCnt.KeepSep += keepSepAssocLst.Count;

                KeepAssociate(keepSepAssocLst);

            }

            if (!keepSepSubset.Any()) return;
            List<KeepSepSubsetInmate> keepSepSubsetLst = keepSepSubset.Where(c => c.DeleteFlag == 0).ToList();
            _classEnrollmentDetails.InmateEligibilityCnt.KeepSep += keepSepSubsetLst.Count;

            KeepSubSet(keepSepSubsetLst);

        }

        private void GetKeepSepDetails()
        {
            List<ClassFileInmateDetail> keepSepInmate = _context.Inmate.Where(w =>
                    _classEnrollmentDetails.InmateEligibilityDetails.Where(c => c.ClassType == ClassTypeConstants.KEEPSEPINMATE)
                     .Select(s => s.KeepSeparateNarrative.KeepSepInmate2Id).Contains(w.InmateId))
                .Select(a => new ClassFileInmateDetail
                {
                    InmateId = a.InmateId,
                    InmateActive = a.InmateActive == 1,
                    InmateNumber = a.InmateNumber,
                    HousingUnitId = a.HousingUnitId ?? 0,
                    HousingUnit = new HousingDetail
                    {
                        HousingUnitId = a.HousingUnitId ?? 0,
                        FacilityId = a.HousingUnitId.HasValue ? a.HousingUnit.FacilityId : 0,
                        HousingUnitListId = a.HousingUnitId.HasValue ? a.HousingUnit.HousingUnitListId : 0,
                        HousingUnitNumber = a.HousingUnitId.HasValue ? a.HousingUnit.HousingUnitNumber : string.Empty,
                        HousingUnitLocation = a.HousingUnitId.HasValue ? a.HousingUnit.HousingUnitLocation : string.Empty,
                        HousingUnitBedNumber =
                        a.HousingUnitId.HasValue ? a.HousingUnit.HousingUnitBedNumber : string.Empty,
                        HousingUnitBedLocation =
                        a.HousingUnitId.HasValue ? a.HousingUnit.HousingUnitBedLocation : string.Empty
                    },
                    Person = new PersonDetail
                    {
                        PersonId = a.PersonId,
                        PersonFirstName = a.Person.PersonFirstName,
                        PersonMiddleName = a.Person.PersonMiddleName,
                        PersonLastName = a.Person.PersonLastName
                    }
                }).ToList();

            if (keepSepInmate.Count > 0)
            {
                _classEnrollmentDetails.InmateEligibilityDetails.Where(c => c.ClassType == ClassTypeConstants.KEEPSEPINMATE).ToList()
                    .ForEach(record =>
                    {
                        record.KeepSeparateNarrative.InmateActive = keepSepInmate.Single(w =>
                            w.InmateId == record.KeepSeparateNarrative.KeepSepInmate2Id).InmateActive;
                        record.HousingDetails = keepSepInmate.Single(w =>
                            w.InmateId == record.KeepSeparateNarrative.KeepSepInmate2Id).HousingUnit;

                        record.PersonDetails = new PersonInfo
                        {
                            InmateNumber = keepSepInmate
                                .Single(w => w.InmateId == record.KeepSeparateNarrative.KeepSepInmate2Id).InmateNumber,
                            PersonFirstName = keepSepInmate
                                .Single(w => w.InmateId == record.KeepSeparateNarrative.KeepSepInmate2Id).Person
                                .PersonFirstName,
                            PersonLastName = keepSepInmate
                                .Single(w => w.InmateId == record.KeepSeparateNarrative.KeepSepInmate2Id).Person
                                .PersonLastName,
                            PersonMiddleName = keepSepInmate
                                .Single(w => w.InmateId == record.KeepSeparateNarrative.KeepSepInmate2Id).Person
                                .PersonMiddleName
                        };
                    });
            }
        }

        private void KeepSubSet(List<KeepSepSubsetInmate> keepSepSubset)
        {
            //To get SubSet details
            List<Lookup> lookuplist = _context.Lookup.Where(w => w.LookupType == LookupConstants.CLASSGROUP).ToList();
            List<Lookup> lookupSubsetlist = _context.Lookup.Where(w => w.LookupType == LookupConstants.CLASSGROUPSUB).ToList();
            _classEnrollmentDetails.InmateEligibilityDetails.AddRange(keepSepSubset.Select(kss => new ClassLog
            {
                Id = kss.KeepSepSubsetInmateId,
                InmateId = kss.KeepSepInmate2Id,
                ClassType = ClassTypeConstants.KEEPSEPSUBSET,
                ClassDate = kss.CreateDate,
                KeepSeparateNarrative = new KeepSeparateNarrative
                {
                    KeepSepAssoc = lookuplist.SingleOrDefault(k => k.LookupIndex == kss.KeepSepAssoc1Id)?.LookupDescription,
                    KeepSepAssocSubset = lookupSubsetlist.SingleOrDefault(k => k.LookupIndex == kss.KeepSepAssoc1SubsetId)?.LookupDescription,
                    KeepSepAssoc1Id = kss.KeepSepAssoc1Id,
                    KeepSepAssoc1SubsetId = kss.KeepSepAssoc1SubsetId,
                    KeepSepType = kss.KeepSeparateType,
                    KeepSepReason = kss.KeepSepReason,
                    KeepSepNote = kss.KeepSeparateNote
                },
                OfficerId = kss.KeepSepOfficerId,
                DeleteFlag = kss.DeleteFlag == 1

            }).ToList());
        }

        private void KeepAssociate(List<KeepSepAssocInmate> keepSepAssoc)
        {
            //To get KeepAssociate details
            List<Lookup> lookuplist = _context.Lookup.Where(w => w.LookupType == LookupConstants.CLASSGROUP).ToList();
            _classEnrollmentDetails.InmateEligibilityDetails.AddRange(keepSepAssoc.Select(ksa => new ClassLog
            {
                Id = ksa.KeepSepAssocInmateId,
                InmateId = ksa.KeepSepInmate2Id,
                ClassType = ClassTypeConstants.KEEPSEPASSOC,
                ClassDate = ksa.CreateDate,

                KeepSeparateNarrative = new KeepSeparateNarrative
                {
                    KeepSepAssoc = lookuplist.SingleOrDefault(k => k.LookupIndex == ksa.KeepSepAssoc1Id)?.LookupDescription,
                    KeepSepAssoc1Id = ksa.KeepSepAssoc1Id,
                    KeepSepType = ksa.KeepSeparateType,
                    KeepSepReason = ksa.KeepSepReason,
                    KeepSepNote = ksa.KeepSeparateNote
                },
                OfficerId = ksa.KeepSepOfficerId,
                DeleteFlag = ksa.DeleteFlag == 1
            }).ToList());
        }

        private void KeepSep2(List<KeepSeparate> keepSep2) =>
            _classEnrollmentDetails.InmateEligibilityDetails.AddRange(keepSep2.Select(ks2 => new ClassLog
            {
                Id = ks2.KeepSeparateId,
                InmateId = ks2.KeepSeparateInmate2Id,
                ClassType = ClassTypeConstants.KEEPSEPINMATE,
                ClassDate = ks2.KeepSeparateDate,
                KeepSeparateNarrative = new KeepSeparateNarrative
                {
                    KeepSepInmate2Id = ks2.KeepSeparateInmate1Id,
                    KeepSepType = ks2.KeepSeparateType,
                    KeepSepReason = ks2.KeepSeparateReason,
                    KeepSepNote = ks2.KeepSeparateNote
                },
                OfficerId = ks2.KeepSeparateOfficerId,
                DeleteFlag = ks2.InactiveFlag == 1
            }).ToList());

        private void KeepSep1(List<KeepSeparate> keepSep1) =>
            _classEnrollmentDetails.InmateEligibilityDetails.AddRange(keepSep1.Select(ks1 => new ClassLog
            {
                Id = ks1.KeepSeparateId,
                InmateId = ks1.KeepSeparateInmate1Id,
                ClassType = ClassTypeConstants.KEEPSEPINMATE,
                ClassDate = ks1.KeepSeparateDate,
                KeepSeparateNarrative = new KeepSeparateNarrative
                {
                    KeepSepInmate2Id = ks1.KeepSeparateInmate2Id,
                    KeepSepType = ks1.KeepSeparateType,
                    KeepSepReason = ks1.KeepSeparateReason,
                    KeepSepNote = ks1.KeepSeparateNote
                },
                OfficerId = ks1.KeepSeparateOfficerId,
                DeleteFlag = ks1.InactiveFlag == 1
            }).ToList());

        #endregion

        private void GetAssociationDetails()
        {
            List<int> lookupDescLst = _lookUp.Where(w => w.LookupType == LookupConstants.CLASSGROUP && (w.LookupFlag9 == 1 || w.LookupFlag10 == 1))
                .Select(x => Convert.ToInt32(x.LookupIndex)).ToList();

            IQueryable<PersonClassification> personClassification =
                _context.PersonClassification.Where(pc => pc.PersonId == _inmate.PersonId && pc.InactiveFlag == 0 &&
                                                          (!pc.PersonClassificationDateThru.HasValue ||
                                                           pc.PersonClassificationDateThru.Value.Date >= DateTime.Now.Date) &&
                                                          lookupDescLst.Contains(pc.PersonClassificationTypeId ?? 0));
            //Association Count
            _classEnrollmentDetails.InmateEligibilityCnt.Assoc = personClassification.Count();

            //Association Details
            _classEnrollmentDetails.InmateEligibilityDetails.AddRange(personClassification
                .Select(person => new ClassLog
                {
                    Id = person.PersonClassificationId,
                    InmateId = _inmate.InmateId,
                    PersonId = person.PersonId,
                    ClassDate = person.PersonClassificationDateFrom,
                    OfficerId = person.CreatedByPersonnelId,
                    ClassType = ClassTypeConstants.ASSOC,
                    DeleteFlag = person.InactiveFlag == 1,
                    AssocNarrative = new AssocNarrative
                    {
                        PersonClassificationType = person.PersonClassificationType,
                        PersonClassificationSubset = person.PersonClassificationSubset,
                        PersonClassificationNotes = person.PersonClassificationNotes,
                        PersonClassificationStatus = person.PersonClassificationStatus,
                        PersonClassificationDateThru = person.PersonClassificationDateThru,
                        PersonClassificationTypeId = person.PersonClassificationTypeId,
                        PersonClassificationSubsetId = person.PersonClassificationSubsetId,
                    }
                }).ToList());
        }

        private void GetHousingDetails(int inmateId)
        {
            //To get Housing details
            IQueryable<HousingUnitMoveHistory> housingUnitMoveHistory =
                _context.HousingUnitMoveHistory.Where(hmv =>
                    (hmv.HousingUnitToId.HasValue || !hmv.HousingUnitToId.HasValue) && hmv.InmateId == inmateId);

            if (!housingUnitMoveHistory.Any())
            {
                return;
            }

            //record
            _classEnrollmentDetails.InmateEligibilityDetails.AddRange(housingUnitMoveHistory
                .Select(hmh => new ClassLog
                {
                    Id = hmh.HousingUnitMoveHistoryId,
                    InmateId = hmh.InmateId,
                    ClassType = ClassTypeConstants.HOUSING,
                    ClassDate = hmh.MoveDate,
                    HousingDateThru = hmh.MoveDateThru,
                    HousingDetails = new HousingDetail
                    {
                        HousingUnitNumber = hmh.HousingUnitTo.HousingUnitNumber,
                        HousingUnitLocation = hmh.HousingUnitTo.HousingUnitLocation,
                        HousingUnitBedNumber = hmh.HousingUnitTo.HousingUnitBedNumber,
                        HousingUnitBedLocation = hmh.HousingUnitTo.HousingUnitBedLocation,
                        FacilityAbbr = hmh.HousingUnitTo.FacilityId > 0
                            ? hmh.HousingUnitTo.Facility.FacilityAbbr
                            : string.Empty
                    },
                    Reason = hmh.MoveReason,
                    OfficerId = hmh.MoveOfficerId,
                    MoveThruBy = hmh.MoveThruBy
                }).ToList());
            _classEnrollmentDetails.InmateEligibilityCnt.Housing =
               _classEnrollmentDetails.InmateEligibilityDetails.Count(c => c.ClassType == ClassTypeConstants.HOUSING);
        }

        private void GetClassifyDetails(int inmateId)
        {
            //To get Classify details
            List<InmateClassification> inmateClassification = _context.InmateClassification.Where(ic =>
                ic.InmateId == inmateId &&
                (ic.InmateClassificationType == ClassTypeConstants.INITIAL ||
                    ic.InmateClassificationType == ClassTypeConstants.RECLASSIFICATION)).ToList();

            List<InmateClassificationNarrative> inmateClassificationNarratives =
                _context.InmateClassificationNarrative.Where(ic => ic.InmateId == inmateId && ic.ReviewFlag == 1).ToList();

            //Count
            _classEnrollmentDetails.InmateEligibilityCnt.Classify = inmateClassification.Count + inmateClassificationNarratives.Count;

            //Record
            _classEnrollmentDetails.InmateEligibilityDetails.AddRange(inmateClassification.Where(ic =>
                    ic.InmateClassificationType == ClassTypeConstants.INITIAL ||
                    ic.InmateClassificationType == ClassTypeConstants.RECLASSIFICATION)
                .Select(ic => new ClassLog
                {
                    Id = ic.InmateClassificationId,
                    InmateId = ic.InmateId,
                    ClassType = ic.InmateClassificationType,
                    ClassDate = ic.InmateDateAssigned,
                    DateUnassigned = ic.InmateDateUnassigned,
                    ClassNarrative = ic.InmateOverrideNarrative,
                    Reason = ic.InmateClassificationReason,
                    OfficerId = ic.ClassificationOfficerId,
                    PersonId = _inmate.PersonId
                })
                .ToList());

            _classEnrollmentDetails.InmateEligibilityDetails.AddRange(inmateClassificationNarratives
                .Select(ic => new ClassLog
                {
                    Id = ic.InmateClassificationNarrativeId,
                    InmateId = ic.InmateId,
                    ClassType = ClassTypeConstants.REVIEW,
                    ClassDate = ic.CreateDate,
                    //DateUnassigned = ic.InmateDateUnassigned,
                    ClassNarrative = ic.Narrative,
                    //Reason = ic.InmateClassificationReason,
                    OfficerId = ic.CreatedBy,
                    PersonId = _inmate.PersonId
                })
                .ToList());
        }

        #region Class Details
        //To get Class List Details
        public List<ProgramClassDetailsVm> GetClassListDetails(int? inmateId)
        {
            List<ScheduleVm> schList = _context.ScheduleProgram.Where(n => n.ProgramClassId.HasValue &&
                !n.DeleteFlag && !n.ProgramClass.DeleteFlag
                && !n.ProgramClass.InactiveFlag).Select(sch => new ScheduleVm
            {
                StartDate = sch.StartDate,
                EndDate = sch.EndDate,
                LocationId = sch.LocationId ?? 0,
                ScheduleId = sch.ScheduleId,
                Duration = sch.Duration,
                DeleteReason = sch.DeleteReason,
                IsSingleOccurrence = sch.IsSingleOccurrence,
                LocationDetail = sch.LocationDetail,
                DayInterval = sch.DayInterval,
                WeekInterval = sch.WeekInterval,
                FrequencyType = sch.FrequencyType,
                QuarterInterval = sch.QuarterInterval,
                MonthOfQuarterInterval = sch.MonthOfQuarterInterval,
                MonthOfYear = sch.MonthOfYear,
                DayOfMonth = sch.DayOfMonth,
                ProgramClassId = sch.ProgramClassId,
                Location = sch.Location.PrivilegeDescription,
                CourseCapacity = sch.ProgramClass.CourseCapacity,
            }).ToList();
            //Taking unAssign Schedules For Inmate
            List<int> scheduleIds = new List<int>();
            if (inmateId > 0)
            {
                 scheduleIds = _context.ProgramClassAssign.Where(p => p.InmateId == inmateId)
                    .Select(p => p.ScheduleId).ToList();
            }

            List<ProgramClassDetailsVm> pgmList = _context.ProgramRequest
                .Where(p => p.DeleteFlag == 0 && p.ApprovedFlag && (inmateId == 0 || p.InmateId == inmateId))
                .Select(p => new ProgramClassDetailsVm
                {
                    ProgramRequestId = p.ProgramRequestId,
                    ProgramClassId = p.ProgramClassId,
                    Crn = p.ProgramClass.CRN,
                    ProgramCourseId = p.ProgramClass.ProgramCourseId,
                    CourseName = p.ProgramClass.ProgramCourseIdNavigation.CourseName,
                    CourseNumber = p.ProgramClass.ProgramCourseIdNavigation.CourseNumber,
                    ScheduleList = schList.Where(s =>
                        s.ProgramClassId == p.ProgramClassId && (inmateId == 0 || scheduleIds.Count == 0 ||
                            !scheduleIds.Contains(s.ScheduleId))).ToList()
                }).ToList();

            return pgmList;
        }

        #endregion


        #region Enrollment By Request
        //Programs -> Course Management -> Enrollment By Request
        public EnrollmentRequest GetEnrollmentRequest(int programClassId, int locationId, int scheduleId)
        {
            List<int> requestIds = _context.ProgramClassAssign.Where(p => p.ScheduleId == scheduleId)
                .Select(p => p.ProgramRequestId).ToList();

            EnrollmentRequest enrollmentRequests = new EnrollmentRequest()
            {
                EnrollmentRequestDetails = _context.ProgramRequest
                    .Where(w => w.ProgramClassId == programClassId && !requestIds.Contains(w.ProgramRequestId))
                    .Select(s => new EnrollmentRequestDetails
                    {
                        RequestId = s.ProgramRequestId,
                        ApprovedFlag = s.ApprovedFlag,
                        Capacity = s.ProgramClass.CourseCapacity,
                        RequestDate = s.CreateDate,
                        InmateId = s.Inmate.InmateId,
                        InmateInfo = new PersonInfoVm
                        {
                            PersonLastName = s.Inmate.Person.PersonLastName,
                            PersonFirstName = s.Inmate.Person.PersonFirstName,
                            PersonMiddleName = s.Inmate.Person.PersonMiddleName,
                            InmateNumber = s.Inmate.InmateNumber,
                            PersonSexLast = s.Inmate.Person.PersonSexLast,
                            Sex = _context.Lookup.FirstOrDefault(w =>
                                    w.LookupIndex == s.Inmate.Person.PersonSexLast &&
                                    w.LookupType == LookupConstants.SEX)
                                .LookupDescription
                        },
                        HousingDetails = new HousingDetail
                        {
                            HousingUnitLocation = s.Inmate.HousingUnit.HousingUnitLocation,
                            HousingUnitNumber = s.Inmate.HousingUnit.HousingUnitNumber,
                            HousingUnitBedNumber = s.Inmate.HousingUnit.HousingUnitBedNumber,
                            HousingUnitBedLocation = s.Inmate.HousingUnit.HousingUnitBedLocation,
                            HousingUnitListId = s.Inmate.HousingUnit.HousingUnitListId
                        }
                    }).ToList(),
                EnrollmentCapacity = _context.ScheduleProgram
                    .Where(w => w.LocationId == locationId && w.ScheduleId == scheduleId)
                    .Select(s => s.Location.Capacity).FirstOrDefault()
            };

            return enrollmentRequests;
        }

        public int InsertEnrollmentDetails(EnrollmentRequest enrollmentRequest)
        {
            List<ProgramClassAssign> programClassAssigns = enrollmentRequest.EnrollmentRequestDetails
                .Select(s => new ProgramClassAssign
                {
                    ScheduleId = s.ScheduleId,
                    InmateId = s.InmateId,
                    CreateBy = _personnelId,
                    CreateDate = DateTime.Now,
                    ProgramRequestId = s.RequestId,
                    AssignedBy = _personnelId,
                    AssignedDate = DateTime.Now,
                    AssignNote = s.AssignNote
                }).ToList();

            _context.ProgramClassAssign.AddRange(programClassAssigns);
            return _context.SaveChanges();
        }
        #endregion

       
        //Programs -> Course Management -> Class Management
        public List<ClassManagement> GetClassManagement(int scheduleId, int programClassId) => _context.ProgramClassAssign
          .Where(w => w.ScheduleId == scheduleId).Select(s => new ClassManagement
          {
              ProgramClassAssignId = s.ProgramClassAssignId,
              ScheduleId = s.ScheduleId,
              AssignedDate = s.AssignedDate,
              CompleteNotPass = s.CompleteNotPass,
              CompletePass = s.CompletePass,
              CourseComplete = s.CourseComplete,
              CourseNotComplete = s.CourseNotComplete,
              InmateInfo = new PersonInfoVm
              {
                  PersonLastName = s.InmateIdNavigation.Person.PersonLastName,
                  PersonFirstName = s.InmateIdNavigation.Person.PersonFirstName,
                  PersonMiddleName = s.InmateIdNavigation.Person.PersonMiddleName,
                  InmateNumber = s.InmateIdNavigation.InmateNumber,
                  PersonSexLast = s.InmateIdNavigation.Person.PersonSexLast,
                  InmateId = s.InmateId
              },
              CertificateName = s.CompleteCertName,
              CourseUnassignReason = s.CourseUnassignReason,
              //CompleteGrade = s.CompleteGrade,
              Location = _context.Privileges
                  .SingleOrDefault(w => w.PrivilegeId == s.ProgramClassIdNavigation.LocationId).PrivilegeDescription,
              HousingDetails = new HousingDetail
              {
                  HousingUnitLocation = s.InmateIdNavigation.HousingUnit.HousingUnitLocation,
                  HousingUnitNumber = s.InmateIdNavigation.HousingUnit.HousingUnitNumber,
                  HousingUnitBedNumber = s.InmateIdNavigation.HousingUnit.HousingUnitBedNumber,
                  HousingUnitBedLocation = s.InmateIdNavigation.HousingUnit.HousingUnitBedLocation,
                  HousingUnitListId = s.InmateIdNavigation.HousingUnit.HousingUnitListId
              },
              ProgramClassId = programClassId,
              ProgramCourseId = s.ProgramRequest.ProgramClass.ProgramCourseId,
              GradeFlag = s.ProgramRequest.ProgramClass.ProgramCourseIdNavigation.GradeFlag,
              PassNotPassFlag = s.ProgramRequest.ProgramClass.ProgramCourseIdNavigation.PassNotPassFlag,
              CertificateFlag = s.ProgramRequest.ProgramClass.ProgramCourseIdNavigation.CertificateFlag,
          }).ToList();

        public int UpdateClassManagement(List<ClassManagement> classManagementDetails)
        {
            classManagementDetails.ForEach(item =>
            {
                ProgramClassAssign programClassAssign =
                    _context.ProgramClassAssign.Single(w => w.ProgramClassAssignId == item.ProgramClassAssignId);
                //programClassAssign.CompleteGrade = item.CompleteGrade;
                programClassAssign.AssignedDate = item.AssignedDate;
                programClassAssign.CompletePass = item.CompletePass;
                programClassAssign.CompleteNotPass = item.CompleteNotPass;
                programClassAssign.CourseComplete = item.CourseComplete;
                programClassAssign.CourseNotComplete = item.CourseNotComplete;
                programClassAssign.ScheduleId = item.ScheduleId;
                programClassAssign.CompleteCertFlag = item.CertificateFlag;
                programClassAssign.CourseUnassignReason = item.CourseUnassignReason;
                programClassAssign.AssignNote = item.Note;
                programClassAssign.CompleteCertName = item.CertificateName;
                programClassAssign.UpdateBy = _personnelId;
                programClassAssign.UpdateDate = DateTime.Now;
            });
            return _context.SaveChanges();
        }
    }
}
