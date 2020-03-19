﻿using GenerateTables.Models;
using ServerAPI.Utilities;
using ServerAPI.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using ScheduleWidget.Schedule;
using ScheduleWidget.Common;
using ServerAPI.Interfaces;
using Microsoft.EntityFrameworkCore;

// ReSharper disable once CheckNamespace
namespace ServerAPI.Services
{
    public class AppointmentService : IAppointmentService
    {
        private readonly AAtims _context;
        private readonly ICommonService _commonService;
        private readonly IRegisterService _visitRegisterService;
        private readonly int _personnelId;
        private AppointmentConflictCheck _apptConflictCheck;
        private readonly IPersonService _personService;
        private readonly IKeepSepAlertService _iKeepSepAlertService;
        private readonly IAppointmentViewerService _iAppointmentViewerService;
        private readonly IInterfaceEngineService _interfaceEngine;        
        public AppointmentService(AAtims context, ICommonService commonService, IRegisterService visitRegisterService,
            IHttpContextAccessor httpContextAccessor, IPersonService personService,
            IKeepSepAlertService keepSepAlertService, IInterfaceEngineService interfaceEngine,
            IAppointmentViewerService iAppointmentViewerService)
        {
            _context = context;
            _commonService = commonService;
            _visitRegisterService = visitRegisterService;
            _personnelId = Convert.ToInt32(httpContextAccessor.HttpContext.User
                .FindFirst("personnelId")?.Value);
            _personService = personService;
            _personService = personService;
            _iKeepSepAlertService = keepSepAlertService;
            _iAppointmentViewerService = iAppointmentViewerService;
            _interfaceEngine = interfaceEngine;           
        }

        public List<AoAppointmentVm> ListAoAppointments(int facilityId, int? inmateId, DateTime fromDate,
            DateTime toDate,
            bool isActive, bool inProgress = false, bool checkReoccurence = false)
        {
            List<AoAppointmentVm> inmateApptList = new List<AoAppointmentVm>();
            List<int> lstInmate = new List<int>();

            if (facilityId > 0 && (!inmateId.HasValue || inmateId == 0))
            {
                lstInmate = _context.Inmate.Where(inm => inm.FacilityId == facilityId).Select(inm => inm.InmateId)
                    .ToList();
            }

            List<ScheduleVm> schList = _context.Schedule.OfType<ScheduleInmate>().AsNoTracking().Where(a =>
                !a.DeleteFlag &&
                (!inmateId.HasValue || inmateId == 0 || a.InmateId == inmateId) &&
                (a.IsSingleOccurrence && a.StartDate.Date == a.EndDate.Value.Date &&
                 fromDate.Date <= a.StartDate.Date && toDate.Date >= a.StartDate.Date ||

                 !a.IsSingleOccurrence &&
                 (fromDate.Date >= a.StartDate.Date &&
                  toDate.Date <= (a.EndDate.HasValue ? a.EndDate.Value.Date : toDate.Date) ||
                  fromDate.Date <= (a.EndDate.HasValue ? a.EndDate.Value.Date : toDate.Date) &&
                  toDate > a.StartDate.Date)
                 || (a.IsSingleOccurrence && a.StartDate.Date != a.EndDate.Value.Date && a.EndDate.Value.Date >= fromDate.Date
                 &&
                  fromDate.Date <= a.StartDate.Date && toDate.Date >= a.StartDate.Date
                  )

                 )).Select(sch => new ScheduleVm
                 {
                     StartDate = sch.StartDate,
                     EndDate = sch.EndDate,
                     LocationId = sch.LocationId ?? 0,
                     ScheduleId = sch.ScheduleId,
                     InmateId = sch.InmateId ?? 0,
                     ReasonId = sch.ReasonId,
                     TypeId = sch.TypeId,
                     Duration = sch.Duration,
                     DeleteReason = sch.DeleteReason,
                     Notes = sch.Notes,
                     IsSingleOccurrence = sch.IsSingleOccurrence,
                     LocationDetail = sch.LocationDetail,
                     DayInterval = sch.DayInterval,
                     WeekInterval = sch.WeekInterval,
                     FrequencyType = sch.FrequencyType,
                     QuarterInterval = sch.QuarterInterval,
                     MonthOfQuarterInterval = sch.MonthOfQuarterInterval,
                     MonthOfYear = sch.MonthOfYear,
                     DayOfMonth = sch.DayOfMonth,
                     Discriminator = sch is ScheduleCourt ? "COURT" : sch is Visit ? "VISIT" : "APPT",
                     Location =  sch.Location.PrivilegeDescription,
                     FacilityId = sch.Location.FacilityId ?? 0
                 }).ToList();

            schList = facilityId > 0 && (!inmateId.HasValue || inmateId == 0)
                ? schList.Where(ii => lstInmate.Contains(ii.InmateId ?? 0)).ToList()
                : schList.Where(ii => ii.InmateId == inmateId).ToList();

            List<KeyValuePair<int, int>> keyValuePairs = schList
                .Select(a => new KeyValuePair<int, int>(a.ScheduleId, a.LocationId)).ToList();
            int[] scheduleIds = (from a in schList select a.ScheduleId).ToArray();
            int[] locationIds = (from a in schList select a.LocationId).ToArray();

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
                    .DuringMonthOfQuarter(sch.MonthOfQuarterInterval)
                    .DuringQuarter(sch.QuarterInterval)
                    .HavingFrequency(sch.FrequencyType)
                    .Create();

                Debug.Assert(materializedSchedule.EndDate != null, "AppointmentService: materializedSchedule.EndDate != null");
                DateRange during = new DateRange(materializedSchedule.StartDate,
                    sch.EndDate.HasValue ? materializedSchedule.EndDate.Value : DateTime.Now.AddYears(5));

                if (materializedSchedule.IsSingleOccurrence || sch.Discriminator == "VISIT")
                {
                    inmateApptList.Add(new AoAppointmentVm
                    {
                        StartDate = materializedSchedule.StartDate,
                        EndDate = materializedSchedule.EndDate,
                        LocationId = sch.LocationId,
                        ScheduleId = sch.ScheduleId,
                        InmateId = sch.InmateId,
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
                        Discriminator = sch.Discriminator,
                        //TrackId = sch.TrackId,
                        FacilityId = sch.FacilityId
                    });
                }
                else
                {
                    inmateApptList.AddRange(materializedSchedule.Occurrences(during)
                        .Select(date => new AoAppointmentVm
                        {
                            StartDate = date.Date.Add(during.StartDateTime.TimeOfDay),
                            EndDate = sch.EndDate.HasValue
                                ? date.Date.Add(during.EndDateTime.TimeOfDay)
                                : date.Date.Add(during.StartDateTime.TimeOfDay).Add(sch.Duration),
                            LocationId = sch.LocationId,
                            ScheduleId = sch.ScheduleId,
                            InmateId = sch.InmateId,
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
                            Discriminator = sch.Discriminator,
                            //TrackId = sch.TrackId,
                            FacilityId = sch.FacilityId
                        }));
                }
            }

            inmateApptList = inmateApptList.Where(w =>
            {
                Debug.Assert(w.EndDate != null, "AppointmentService: w.EndDate != null");
                return fromDate.Date <= w.StartDate.Date && toDate.Date >= w.StartDate.Date ||
                       w.IsSingleOccurrence && w.StartDate.Date != w.EndDate.Value.Date;
            }).ToList();

            //To get Personnel details for deleted Appointment list
            List<PersonnelVm> lstPersonnel = _context.Personnel
                .Select(s => new PersonnelVm
                {
                    PersonnelId = s.PersonnelId,
                    PersonLastName = s.PersonNavigation.PersonLastName,
                    OfficerBadgeNumber = s.OfficerBadgeNum
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

            if (checkReoccurence)
            {
                inmateApptList = inmateApptList.GroupBy(a => new { a.ScheduleId, a.StartDate }).Select(a => a.First())
                    .ToList();
                IList<int> locationId = inmateApptList.GroupBy(g => g.LocationId).Select(aos => aos.Key).ToList();

                IList<int> transLocationIds = _context.Privileges
                    .Where(pri => locationId.Contains(pri.PrivilegeId) && pri.TransferFlag == 1)
                    .Select(pri => pri.PrivilegeId).ToList();

                inmateApptList = inmateApptList.Where(app => transLocationIds.Contains(app.LocationId)).ToList();
            }

            //To get the Inactive Occurance list 
            inmateApptList = inmateApptList.Where(i => isActive ||
               !apptOccur.Any(a => a.ScheduleId == i.ScheduleId && a.OccuranceDate.Date == i.StartDate.Date)).ToList();

            //Get lookup list for APPTREAS and APPTYPE
            List<Lookup> lstLookUp = _context.Lookup.Where(x => x.LookupInactive == 0 &&
                     (x.LookupType == LookupConstants.APPTREAS || x.LookupType == LookupConstants.APPTYPE)).ToList();

            if (inProgress) //Filter schedule based on Tracking
            {
                inmateApptList = inmateApptList.SelectMany(apl => _context.InmateAppointmentTrack.Where(aoa =>
                    apl.ScheduleId == aoa.ScheduleId && aoa.InmateTrakId > 0 &&
                    aoa.InmateTrakDateIn.HasValue), (apl, aop) => apl).ToList();
            }

            foreach (int schId in keyValuePairs.Select(a => a.Key))
            {
                List<AoAppointmentVm> lstAppt = inmateApptList.Where(ii => ii.ScheduleId == schId).ToList();

                lstAppt.ForEach(item =>
                {
                    AoAppointmentVm apptInm = lstAppt.First(ii => ii.InmateId == item.InmateId);
                    if (apptInm.InmateDetails == null)
                    {
                        item.InmateDetails = LoadInmateDetails(item.InmateId ?? 0);

                        item.ApptOccurance = apptOccur.SingleOrDefault(a =>
                                a.DeleteFlag == 1 && a.OccuranceDate.Date == item.StartDate.Date &&
                                a.ScheduleId == item.ScheduleId);

                        item.DeletedOfficer = item.ApptOccurance != null
                            ? lstPersonnel.SingleOrDefault(p => p.PersonnelId == item.ApptOccurance.DeleteBy)
                            : lstPersonnel.SingleOrDefault(p => p.PersonnelId == item.DeletedBy);

                        item.ApptType = lstLookUp.FirstOrDefault(lkp =>
                                (int?)lkp.LookupIndex == item.TypeId && lkp.LookupType == LookupConstants.APPTYPE)
                            ?.LookupDescription;

                        item.Reason = lstLookUp.SingleOrDefault(lkp =>
                                (int?)lkp.LookupIndex == item.ReasonId && lkp.LookupType == LookupConstants.APPTREAS)
                            ?.LookupDescription;

                        ScheduleCourt scheduleCourt = _context.ScheduleCourt.Include(ii => ii.ScheduleCourtArrest)
                            .Include(ii => ii.Agency).Include(ii => ii.AgencyCourtDept)
                            .SingleOrDefault(aoa => aoa.ScheduleId == item.ScheduleId);

                        if (scheduleCourt is null) return;
                        item.AgencyId = scheduleCourt.AgencyId;
                        item.DeptId = scheduleCourt.AgencyCourtDeptId;
                        item.Court = scheduleCourt.Agency?.AgencyName;
                        item.Dept = scheduleCourt.AgencyCourtDept?.DepartmentName;

                        item.BookingNo = _context.ScheduleCourt.Where(w => w.ScheduleId == item.ScheduleId)
                            .SelectMany(apl => apl.ScheduleCourtArrest.Where(aoa =>
                                    apl.ScheduleId == aoa.ScheduleId), (apl, aop) => aop)
                            .Select(ii => ii.Arrest.ArrestBookingNo)
                            .ToList();
                    }
                    else
                    {
                        item.InmateDetails = apptInm.InmateDetails;
                        item.ApptOccurance = apptInm.ApptOccurance;
                        item.DeletedOfficer = apptInm.DeletedOfficer;
                        item.ApptType = apptInm.ApptType;
                        item.Reason = apptInm.Reason;
                        item.AgencyId = apptInm.AgencyId;
                        item.DeptId = apptInm.DeptId;
                        item.Court = apptInm.Court;
                        item.Dept = apptInm.Dept;
                        item.BookingNo = apptInm.BookingNo;
                        //item.IsVisit = apptInm.IsVisit;
                    }
                });
            }

            return inmateApptList;
        }

        public AppointmentLocationVm GetAppointmentLocation(int inmateId, int? facilityId, ApptLocationFlag flag)
        {
            List<LocationList> LocationList = new List<LocationList>();
            if (inmateId > 0 && (facilityId == 0 || facilityId == null))
            {
                //To get facilityid based on inmate 
                facilityId = _context.Inmate.Single(x => x.InmateId == inmateId).FacilityId;
            }

            if (flag == ApptLocationFlag.Transfer)
            {
                LocationList = _context.Privileges
                                                     .Where(p =>
                                                         p.RemoveFromPrivilegeFlag == 1 && p.RemoveFromTrackingFlag == 0 &&
                                                         p.InactiveFlag == 0 && (p.TransferFlag > 0) && (p.FacilityId == facilityId || !p.FacilityId.HasValue))
                                                     .Select(p =>
                                                         new LocationList
                                                         {
                                                             ApptLocationId = p.PrivilegeId,
                                                             ApptRequireCourtLink = p.AppointmentRequireCourtLink,
                                                             ApptLocation = p.PrivilegeDescription,
                                                             ApptAllowFutureEndDate = p.AppointmentAllowFutureEndDate == 1,
                                                             ApptAlertEndDate = p.AppointmentAlertApptEndDate == 1,
                                                             DefaultCourtId = p.DefaultCourtId
                                                         }).OrderBy(x => x.ApptLocation).ToList();
            }
            else
            {
                LocationList = _context.Privileges
                                     .Where(p =>
                                         p.RemoveFromPrivilegeFlag == 1 && p.RemoveFromTrackingFlag == 0 &&
                                         p.InactiveFlag == 0 && (flag == ApptLocationFlag.Visitation
                                         ? p.ShowInVisitation : flag == ApptLocationFlag.Transfer ? p.TransferFlag > 0
                                         : flag == ApptLocationFlag.Schedule ? p.ShowInAppointments && p.TransferFlag == 1
                                         : p.ShowInVisitation || p.ShowInAppointments)
                                     && (facilityId > 0 && flag == ApptLocationFlag.Transfer ? p.FacilityId == facilityId
                                         : p.FacilityId == facilityId || !p.FacilityId.HasValue))
                                     .Select(p =>
                                         new LocationList
                                         {
                                             ApptLocationId = p.PrivilegeId,
                                             ApptRequireCourtLink = p.AppointmentRequireCourtLink,
                                             ApptLocation = p.PrivilegeDescription,
                                             ApptAllowFutureEndDate = p.AppointmentAllowFutureEndDate == 1,
                                             ApptAlertEndDate = p.AppointmentAlertApptEndDate == 1,
                                             DefaultCourtId = p.DefaultCourtId
                                         }).OrderBy(x => x.ApptLocation).ToList();
            }

            //To get Appointment Location for New Appointment Entry
            AppointmentLocationVm apptLocationList = new AppointmentLocationVm
            {
                //To get Appointment Location
                LocationList = LocationList,
                //To get Appointment Reason
                AppointmentReason = _commonService.GetLookupKeyValuePairs(LookupConstants.APPTREAS),

                //To get Appointment Type
                AppointmentType = _commonService.GetLookupKeyValuePairs(LookupConstants.APPTYPE),

                //To get Court Agency
                CourtLocation = _context.Agency
                    .Where(x => x.AgencyCourtFlag
                                && (x.AgencyInactiveFlag == 0 || !x.AgencyInactiveFlag.HasValue))
                    .Select(x => new KeyValuePair<int, string>(x.AgencyId, x.AgencyName))
                    .OrderBy(x => x.Value).ToList(),

                //To get Court Department
                CourtDepartmentList =
                    _context.AgencyCourtDept.Where(x => x.DeleteFlag == 0 || !x.DeleteFlag.HasValue)
                        .Select(x => new CourtDepartmentList
                        {
                            AgencyId = x.AgencyId,
                            AgencyCourtDeptId = x.AgencyCourtDeptId,
                            DepartmentName = x.DepartmentName
                        }).ToList(),

                // To get Booking Details
                BookingDetails = inmateId > 0 ? GetBookingDetails(inmateId) : null

            };
            return apptLocationList;
        }

        private List<BookingDetails> GetBookingDetails(int inmateId)
        {
            //To get Booking Details for Inmate
            int incarcerationId = _context.Incarceration.Where(x => x.InmateId == inmateId)
                .OrderByDescending(x => x.ReleaseOut ?? DateTime.Now)
                .Select(x => x.IncarcerationId).FirstOrDefault();

            int[] incarcerationIds = _context.Incarceration.Where(x => x.InmateId == inmateId)
                .Select(x => x.IncarcerationId).ToArray();

            List<BookingDetails> bookingDetails = _context.IncarcerationArrestXref
                .Where(ax => ax.IncarcerationId.HasValue && incarcerationIds.Contains(ax.IncarcerationId.Value)).Select(ax => new BookingDetails
                {
                    ArrestId = ax.Arrest.ArrestId,
                    ArrestBookingNo = ax.Arrest.ArrestBookingNo,
                    ArrestTypeId = ax.Arrest.ArrestType,
                    ArrestCourtDocket = ax.Arrest.ArrestCourtDocket,
                    ArrestCourtJurisdictionId = ax.Arrest.ArrestCourtJurisdictionId,
                    ArrestActive = ax.Arrest.ArrestActive,
                    AgencyName = ax.Arrest.ArrestCourtJurisdiction.AgencyName,
                    ArrestCaseNumber = ax.Arrest.ArrestCaseNumber,
                    AgencyAbbr = ax.Arrest.ArrestCourtJurisdiction.AgencyAbbreviation,
                    ArrestingAgencyAbbr = ax.Arrest.ArrestingAgency.AgencyAbbreviation,
                    ArrestDate = ax.Arrest.ArrestDate,
                    ActiveIncarceration = ax.IncarcerationId == incarcerationId,
                    ReleaseOut = ax.Incarceration.ReleaseOut
                }).OrderBy(x => x.ArrestId).ToList();

            List<Lookup> lookupList = _context.Lookup.Where(
                x => x.LookupType == LookupConstants.ARRTYPE && x.LookupInactive == 0).ToList();

            bookingDetails.ForEach(item =>
            {
                item.ArrestType = lookupList.Where(x => item.ArrestTypeId == Convert.ToString(x.LookupIndex))
                    .Select(y => y.LookupDescription).FirstOrDefault();
            });
            return bookingDetails;
        }

        public async Task<AppointmentConflictCheck> InsertSchedule(InmateApptDetailsVm inmateApptDetails)
        {
            ////To Check Appointment Conflict 
            if (inmateApptDetails.AoScheduleDetails.IsSingleOccurrence &&
                !inmateApptDetails.ApptRecheckConflict &&
                inmateApptDetails.AoScheduleDetails.StartDate.Date == inmateApptDetails.AoScheduleDetails.EndDate.Value.Date)
            {
                _apptConflictCheck = CheckAppointmentConflict(inmateApptDetails);

                if (_apptConflictCheck.ApptConflictDetails.Any())
                {
                    return _apptConflictCheck;
                }
            }

            if (inmateApptDetails.AoScheduleDetails.AgencyId.HasValue)
            {
                InsertCourtSchedule(inmateApptDetails);
            }
            else
            {
                InsertApptSchedule(inmateApptDetails);
            }
            //To add inmate schedule details in schedule history
            LoadInsertScheduleHistory(inmateApptDetails.ScheduleId, inmateApptDetails.AppointmentHistoryList);
            await _context.SaveChangesAsync();
            return _apptConflictCheck;
        }

        private void InsertApptSchedule(InmateApptDetailsVm inmateApptDetails)
        {
            ScheduleInmate appointment = new ScheduleInmate();
            if (inmateApptDetails.ScheduleId > 0)
            {
                ScheduleCourt appointmentCourt = _context.ScheduleCourt
                    .SingleOrDefault(ii => ii.ScheduleId == inmateApptDetails.ScheduleId);
                if (!(appointmentCourt is null))
                {
                    inmateApptDetails.ScheduleId = 0;
                    _context.ScheduleCourt.Remove(appointmentCourt);
                }
                else
                {
                    appointment = _context.ScheduleInmate
                        .Single(ii => ii.ScheduleId == inmateApptDetails.ScheduleId);
                }
            }

            appointment.InmateId = inmateApptDetails.AoScheduleDetails.InmateId;
            appointment.ReasonId = inmateApptDetails.AoScheduleDetails.ReasonId;
            appointment.TypeId = inmateApptDetails.AoScheduleDetails.TypeId;
            appointment.LocationDetail = inmateApptDetails.AoScheduleDetails.LocationDetail;
            appointment.Notes = inmateApptDetails.AoScheduleDetails.Notes;
            appointment.CreateDate = DateTime.Now;
            appointment.CreateBy = _personnelId;
            appointment.LocationId = inmateApptDetails.AoScheduleDetails.LocationId;
            appointment.StartDate = inmateApptDetails.AoScheduleDetails.StartDate;
            appointment.EndDate = inmateApptDetails.AoScheduleDetails.EndDate;
            appointment.Duration = inmateApptDetails.AoScheduleDetails.Duration;
            appointment.IsSingleOccurrence = inmateApptDetails.AoScheduleDetails.IsSingleOccurrence;
            appointment.Time = inmateApptDetails.AoScheduleDetails.Time;
            appointment.DayInterval = inmateApptDetails.AoScheduleDetails.DayInterval;
            appointment.WeekInterval = inmateApptDetails.AoScheduleDetails.WeekInterval;
            appointment.FrequencyType = inmateApptDetails.AoScheduleDetails.FrequencyType;
            appointment.MonthOfYear = inmateApptDetails.AoScheduleDetails.MonthOfYear;
            appointment.DayOfMonth = inmateApptDetails.AoScheduleDetails.DayOfMonth;
            appointment.QuarterInterval = inmateApptDetails.AoScheduleDetails.QuarterInterval;
            appointment.MonthOfQuarterInterval = inmateApptDetails.AoScheduleDetails.MonthOfQuarterInterval;

            if (inmateApptDetails.ScheduleId == 0)
            {
                _context.ScheduleInmate.Add(appointment);
            }

            _context.SaveChanges();

            _interfaceEngine.Export(new ExportRequestVm
            {
                EventName = EventNameConstants.APPOINTMENT,
                PersonnelId = _personnelId,
                Param1 = _context.Inmate.SingleOrDefault(a => a.InmateId == appointment.InmateId)?
                .PersonId.ToString(),
                Param2 = appointment.ScheduleId.ToString()
            });

            inmateApptDetails.ScheduleId = inmateApptDetails.ScheduleId == 0 ?
                appointment.ScheduleId : inmateApptDetails.ScheduleId;
        }

        private void InsertCourtSchedule(InmateApptDetailsVm inmateApptDetails)
        {
            ScheduleCourt appointment = new ScheduleCourt();
            if (inmateApptDetails.ScheduleId > 0)
            {
                appointment = _context.ScheduleCourt.SingleOrDefault(ii => ii.ScheduleId == inmateApptDetails.ScheduleId);
                if (appointment is null)
                {
                    ScheduleInmate appointmentInm = _context.ScheduleInmate.Single(ii => ii.ScheduleId == inmateApptDetails.ScheduleId);
                    inmateApptDetails.ScheduleId = 0;
                    _context.ScheduleInmate.Remove(appointmentInm);
                    appointment = new ScheduleCourt();
                }
            }

            appointment.InmateId = inmateApptDetails.AoScheduleDetails.InmateId;
            appointment.ReasonId = inmateApptDetails.AoScheduleDetails.ReasonId;
            appointment.TypeId = inmateApptDetails.AoScheduleDetails.TypeId;
            appointment.LocationDetail = inmateApptDetails.AoScheduleDetails.LocationDetail;
            appointment.Notes = inmateApptDetails.AoScheduleDetails.Notes;
            appointment.CreateDate = DateTime.Now;
            appointment.CreateBy = _personnelId;
            appointment.LocationId = inmateApptDetails.AoScheduleDetails.LocationId;
            appointment.StartDate = inmateApptDetails.AoScheduleDetails.StartDate;
            appointment.EndDate = inmateApptDetails.AoScheduleDetails.EndDate;
            appointment.Duration = inmateApptDetails.AoScheduleDetails.Duration;
            appointment.IsSingleOccurrence = inmateApptDetails.AoScheduleDetails.IsSingleOccurrence;
            appointment.Time = inmateApptDetails.AoScheduleDetails.Time;
            appointment.DayInterval = inmateApptDetails.AoScheduleDetails.DayInterval;
            appointment.WeekInterval = inmateApptDetails.AoScheduleDetails.WeekInterval;
            appointment.FrequencyType = inmateApptDetails.AoScheduleDetails.FrequencyType;
            appointment.MonthOfYear = inmateApptDetails.AoScheduleDetails.MonthOfYear;
            appointment.DayOfMonth = inmateApptDetails.AoScheduleDetails.DayOfMonth;
            appointment.AgencyId = inmateApptDetails.AoScheduleDetails.AgencyId ?? 0;
            appointment.AgencyCourtDeptId = inmateApptDetails.AoScheduleDetails.AgencyCourtDeptId == 0
                ? null
                : inmateApptDetails.AoScheduleDetails.AgencyCourtDeptId;
            appointment.QuarterInterval = inmateApptDetails.AoScheduleDetails.QuarterInterval;
            appointment.MonthOfQuarterInterval = inmateApptDetails.AoScheduleDetails.MonthOfQuarterInterval;
            if (inmateApptDetails.ScheduleId == 0)
            {
                _context.ScheduleCourt.Add(appointment);
            }

            _context.SaveChanges();
            //To get schedule id 
            inmateApptDetails.ScheduleId = inmateApptDetails.ScheduleId == 0 ? appointment.ScheduleId : inmateApptDetails.ScheduleId;

            List<ScheduleCourtArrest> lstScheduleCourtArrest = _context.ScheduleCourtArrest
                    .Where(ii => ii.ScheduleId == appointment.ScheduleId).ToList();

            _context.ScheduleCourtArrest.RemoveRange(lstScheduleCourtArrest);
            _context.SaveChanges();

            appointment.ScheduleCourtArrest = new List<ScheduleCourtArrest>();
            if (inmateApptDetails.ArrestId != null && inmateApptDetails.ArrestId.Count > 0)
            {
                inmateApptDetails.ArrestId.ForEach(arr =>
                {
                    ScheduleCourtArrest scheduleArrest = new ScheduleCourtArrest
                    {
                        ScheduleId = appointment.ScheduleId,
                        ArrestId = arr,
                        ScheduleCourt = appointment
                    };
                    appointment.ScheduleCourtArrest.Add(scheduleArrest);
                });
            }

            _context.SaveChanges();
        }

        public async Task<AppointmentConflictCheck> UpdateSchedule(InmateApptDetailsVm inmateApptDetails)
        {

            ////To Check Appointment Conflict 
            if (inmateApptDetails.AoScheduleDetails.IsSingleOccurrence && !inmateApptDetails.ApptRecheckConflict)
            {
                _apptConflictCheck = CheckAppointmentConflict(inmateApptDetails);

                if (_apptConflictCheck.ApptConflictDetails.Any())
                {
                    return _apptConflictCheck;
                }
            }

            if (inmateApptDetails.AoScheduleDetails.AgencyId.HasValue)
            {
                InsertCourtSchedule(inmateApptDetails);
            }
            else
            {
                InsertApptSchedule(inmateApptDetails);
            }
            //To add inmate schedule details in schedule history
            LoadInsertScheduleHistory(inmateApptDetails.ScheduleId, inmateApptDetails.AppointmentHistoryList);

            int? visitToVisitorId = _context.VisitToVisitor.SingleOrDefault(vst =>
                vst.ScheduleId == inmateApptDetails.AoScheduleDetails.ScheduleId)?.VisitToVisitorId;
            if (visitToVisitorId > 0)
            {
                await _visitRegisterService.UpdateExitVisitor(
                    new KeyValuePair<int, int>(inmateApptDetails.AoScheduleDetails.InmateId ?? 0, (int)visitToVisitorId));
            }

            _context.SaveChanges();
            return _apptConflictCheck;
        }

        public AppointmentConflictCheck CheckAppointmentConflict(InmateApptDetailsVm inmateApptDetails)
        {
            ScheduleVm aoSchedule = inmateApptDetails.AoScheduleDetails;
            //To get keep separate inmate details
            List<KeepSeparateVm> keepSeparateVms = _iKeepSepAlertService.GetInmateKeepSep(aoSchedule.InmateId ?? 0, false);

            //To Check Appointment Conflict 
            Debug.Assert(aoSchedule.EndDate != null, "AppointmentService: aoSchedule.EndDate != null");
            DateTime endDate = aoSchedule.EndDate.Value;
            endDate = inmateApptDetails.AllDayAppt ? endDate.Date.AddHours(23).AddMinutes(59) : endDate;

            CalendarInputs inputs = new CalendarInputs
            {
                CalendarTypes = "1,2,3",
                InmateId = aoSchedule.InmateId,
                FromDate = aoSchedule.StartDate,
                ToDate = endDate
            };

            List<AoAppointmentVm> lstApptDetails = _iAppointmentViewerService.CheckAppointment(inputs);

            List<AppointmentConflictDetails> lstofReoccurAppt = new List<AppointmentConflictDetails>();

            IQueryable<Privileges> privilegesList = _context.Privileges;

            foreach (DateTime day in EachDay(aoSchedule.StartDate, endDate))
            {
                //Appointment Conflict Check for Reoccurence
                lstofReoccurAppt = lstApptDetails.Where(app =>
                        app.InmateId == aoSchedule.InmateId
                        && app.EndDate.HasValue
                        && (app.EndDate == null ||
                            app.EndDate.Value.Date >= day.Date) &&
                        (aoSchedule.StartDate < day.Date.Add(app.EndDate.Value.TimeOfDay) &&
                         aoSchedule.EndDate > day.Date.Add(app.StartDate.TimeOfDay) ||
                         app.StartDate.ToString(DateConstants.HOURSMINUTES)
                         == DateConstants.STARTHRSMINUTES
                         && app.EndDate.Value.ToString(DateConstants.HOURSMINUTES)
                         == DateConstants.STARTHRSMINUTES)
                    )
                    .Select(app => new AppointmentConflictDetails
                    {
                        ApptId = app.ScheduleId,
                        ConflictType = ConflictTypeConstants.APPOINTMENTOVERLAPCONFLICT,
                        ConflictApptLocation = privilegesList
                            .SingleOrDefault(x => x.PrivilegeId == app.LocationId)?.PrivilegeDescription,
                        ApptStartDate = app.StartDate,
                        ApptReoccurFlag = app.IsSingleOccurrence ? 0 : 1
                    }).ToList();

                lstofReoccurAppt.AddRange(lstApptDetails.Where(app =>
                        app.InmateId == aoSchedule.InmateId &&
                        (app.EndDate == null ||
                         app.EndDate.Value.Date >= day.Date) &&
                        app.EndDate.HasValue &&
                        (day.Date.Add(app.EndDate.Value.TimeOfDay) <= aoSchedule.StartDate &&
                         day.Date.Add(app.EndDate.Value.TimeOfDay).AddHours(1) >= aoSchedule.StartDate &&
                         app.StartDate.ToString(DateConstants.HOURSMINUTES)
                         != DateConstants.STARTHRSMINUTES && app.EndDate.Value.ToString(DateConstants.HOURSMINUTES)
                         != DateConstants.STARTHRSMINUTES ||
                         aoSchedule.EndDate <= day.Date.Add(app.StartDate.TimeOfDay) &&
                         day.Date.Add(app.StartDate.TimeOfDay).AddHours(-1) <= endDate &&
                         app.StartDate.ToString(DateConstants.HOURSMINUTES)
                         != DateConstants.STARTHRSMINUTES && app.EndDate.Value.ToString(DateConstants.HOURSMINUTES)
                         != DateConstants.STARTHRSMINUTES)
                    )
                    .Select(app => new AppointmentConflictDetails
                    {
                        ApptId = app.ScheduleId,
                        ConflictType = ConflictTypeConstants.APPOINTMENTWARNINGBACKTOBACK,
                        ApptStartDate = app.StartDate,
                        ConflictApptLocation = privilegesList
                            .SingleOrDefault(x => x.PrivilegeId == app.LocationId)?.PrivilegeDescription,
                        ApptReoccurFlag = app.IsSingleOccurrence ? 0 : 1
                    }));
            }

            int[] personIds = keepSeparateVms.Select(p => p.PersonId).ToArray();
            List<Person> lstKeepPerson = _context.Person.Where(p => personIds.Contains(p.PersonId))
                .Select(p => new Person
                {
                    PersonLastName = p.PersonLastName,
                    PersonFirstName = p.PersonFirstName,
                    PersonMiddleName = p.PersonMiddleName,
                    PersonId = p.PersonId
                }).ToList();
            foreach (KeepSeparateVm item in keepSeparateVms)
            {
                //Taking the list of appointment details from appointment service
                List<AoAppointmentVm> lstKeepSepApptDetails =
                    ListAoAppointments(aoSchedule.FacilityId, item.KeepSepInmateId, aoSchedule.StartDate, endDate,
                        false);
                lstKeepSepApptDetails =
                    lstKeepSepApptDetails.Where(l => l.LocationId == aoSchedule.LocationId).ToList();
                if (lstKeepSepApptDetails.Count <= 0) continue;
                foreach (AoAppointmentVm aoAppointmentVm in lstKeepSepApptDetails)
                {
                    AppointmentConflictDetails appointmentConflictDetails = new AppointmentConflictDetails
                    {
                        ApptId = aoAppointmentVm.ScheduleId,
                        ConflictType = ConflictTypeConstants.APPOINTMENTKEEPSEPERATECONFLICT,
                        ApptStartDate = aoAppointmentVm.StartDate,
                        ConflictApptLocation = privilegesList
                            .SingleOrDefault(x => x.PrivilegeId == aoAppointmentVm.LocationId)
                            ?.PrivilegeDescription,
                        ApptReoccurFlag = aoAppointmentVm.IsSingleOccurrence ? 0 : 1
                    };

                    PersonInfoVm person = lstKeepPerson.Where(p => p.PersonId == item.PersonId).Select(p =>
                        new PersonInfoVm
                        {
                            InmateNumber = item.InmateNumber,
                            PersonLastName = p.PersonLastName,
                            PersonFirstName = p.PersonFirstName,
                            PersonMiddleName = p.PersonMiddleName
                        }).Single();
                    appointmentConflictDetails.Person = person;

                    lstofReoccurAppt.Add(appointmentConflictDetails);
                }
            }

            // dark day conflicts
            Privileges selectedLocation = privilegesList
                        .SingleOrDefault(x => x.PrivilegeId == aoSchedule.LocationId
                            && x.DarkDaysFlag == 1);
            if (selectedLocation != null)
            {
                lstofReoccurAppt.AddRange(
                    _context.DarkDay.Where(d => d.DarkDayDate.Value.Date == aoSchedule.StartDate.Date
                        && d.DeleteFlag != 1)
                    .Select(a => new AppointmentConflictDetails
                    {
                        ApptId = a.DarkDayId,
                        ConflictType = ConflictTypeConstants.DARKDAY,
                        ApptStartDate = a.DarkDayDate,
                        Description = a.DarkDayDescription,
                        Notes = a.DarkDayNotes,
                        ConflictApptLocation = selectedLocation.PrivilegeDescription
                    }).ToList());
            }

            string appointmentLocation =
                privilegesList.Single(p => p.PrivilegeId == aoSchedule.LocationId).PrivilegeDescription;

            AppointmentConflictCheck apptConflictCheck = new AppointmentConflictCheck
            {
                ApptLocation = appointmentLocation,
                ApptLocationId = aoSchedule.LocationId,
                EndDate = inmateApptDetails.AllDayAppt ? aoSchedule.StartDate : endDate,
                ApptDate = aoSchedule.StartDate,
                ApptConflictDetails = lstofReoccurAppt
            };

            if (inmateApptDetails.ScheduleId > 0)
            {
                apptConflictCheck.ApptConflictDetails.ToList().ForEach(item =>
                {
                    if (item.ApptId == inmateApptDetails.ScheduleId)
                    {
                        apptConflictCheck.ApptConflictDetails.Remove(item);
                    }
                });
            }

            return apptConflictCheck;
        }

        private static IEnumerable<DateTime> EachDay(DateTime from, DateTime thru)
        {
            for (DateTime day = from.Date; day.Date <= thru.Date; day = day.AddDays(1))
            {
                yield return day;
            }
        }

        public InmateApptDetailsVm GetAppointmentSchedule(int scheduleId)
        {
            InmateApptDetailsVm inmateApptDetailsVm = new InmateApptDetailsVm { ScheduleId = scheduleId };
            ScheduleCourt scheduleCourt = _context.ScheduleCourt.Include(ii => ii.ScheduleCourtArrest)
                .SingleOrDefault(ii => ii.ScheduleId == scheduleId);

            if (scheduleCourt is null)
            {
                inmateApptDetailsVm.AoScheduleDetails =
                    _context.ScheduleInmate.Where(ii => ii.ScheduleId == scheduleId).Select(ii => new ScheduleVm
                    {
                        InmateId = ii.InmateId ?? 0,
                        ReasonId = ii.ReasonId,
                        TypeId = ii.TypeId,
                        LocationDetail = ii.LocationDetail,
                        Notes = ii.Notes,
                        LocationId = ii.LocationId ?? 0,
                        StartDate = ii.StartDate,
                        EndDate = ii.EndDate,
                        Duration = ii.Duration,
                        IsSingleOccurrence = ii.IsSingleOccurrence,
                        Time = ii.Time,
                        DayInterval = ii.DayInterval,
                        WeekInterval = ii.WeekInterval,
                        FrequencyType = ii.FrequencyType,
                        MonthOfYear = ii.MonthOfYear,
                        DayOfMonth = ii.DayOfMonth
                    }).Single();
            }
            else
            {
                inmateApptDetailsVm.AoScheduleDetails =
                    _context.ScheduleCourt.Where(ii => ii.ScheduleId == scheduleId).Select(ii => new ScheduleVm
                    {
                        InmateId = ii.InmateId ?? 0,
                        ReasonId = ii.ReasonId,
                        TypeId = ii.TypeId,
                        LocationDetail = ii.LocationDetail,
                        Notes = ii.Notes,
                        LocationId = ii.LocationId ?? 0,
                        StartDate = ii.StartDate,
                        EndDate = ii.EndDate,
                        Duration = ii.Duration,
                        IsSingleOccurrence = ii.IsSingleOccurrence,
                        Time = ii.Time,
                        DayInterval = ii.DayInterval,
                        WeekInterval = ii.WeekInterval,
                        FrequencyType = ii.FrequencyType,
                        MonthOfYear = ii.MonthOfYear,
                        DayOfMonth = ii.DayOfMonth,
                        AgencyId = ii.AgencyId,
                        AgencyCourtDeptId = ii.AgencyCourtDeptId
                    }).Single();

                if (!(scheduleCourt.ScheduleCourtArrest is null))
                {
                    inmateApptDetailsVm.ArrestId = scheduleCourt.ScheduleCourtArrest.Select(ii => ii.ArrestId).ToList();
                }
            }

            if (!inmateApptDetailsVm.AoScheduleDetails.EndDate.HasValue &&
                inmateApptDetailsVm.AoScheduleDetails.IsSingleOccurrence)
            {
                inmateApptDetailsVm.AoScheduleDetails.EndDate = inmateApptDetailsVm.AoScheduleDetails.StartDate.Date
                    .AddHours((inmateApptDetailsVm.AoScheduleDetails.StartDate.TimeOfDay +
                               inmateApptDetailsVm.AoScheduleDetails.Duration).Hours)
                    .AddMinutes((inmateApptDetailsVm.AoScheduleDetails.StartDate.TimeOfDay +
                                 inmateApptDetailsVm.AoScheduleDetails.Duration).Minutes);
            }

            return inmateApptDetailsVm;

        }

        private InmateDetailsList LoadInmateDetails(int inmateId)
        {
            //To get Inmate Details
            InmateDetailsList inmateDetails = _context.Inmate.Where(i => i.InmateId == inmateId).Select(i =>
                new InmateDetailsList
                {
                    FacilityId = i.FacilityId,
                    FacilityAbbr = i.Facility.FacilityAbbr,
                    InmateNumber = i.InmateNumber,
                    PersonFirstName = i.Person.PersonFirstName,
                    PersonLastName = i.Person.PersonLastName,
                    PersonMiddleName = i.Person.PersonMiddleName,
                    HousingUnitId = i.HousingUnitId ?? 0,
                    PersonDob = i.Person.PersonDob,
                    HousingLocation = i.HousingUnit.HousingUnitLocation,
                    HousingNumber = i.HousingUnit.HousingUnitNumber,
                    HousingUnitBedLocation = i.HousingUnit.HousingUnitBedLocation,
                    HousingUnitBedNumber = i.HousingUnit.HousingUnitBedNumber,
                    HousingUnitListId = i.HousingUnit.HousingUnitId > 0 ? i.HousingUnit.HousingUnitListId : 0,
                    PersonId = i.PersonId,
                    InmateActive = i.InmateActive,
                    ClassificationReason = i.InmateClassification.InmateClassificationReason,
                    TrackLocation = i.InmateCurrentTrackNavigation.PrivilegeDescription
                }).Single();
            return inmateDetails;
        }

        public async Task<int> InsertMultiInmateAppt(InmateApptDetailsVm inmateApptDetails, List<int> inmateIds)
        {
            inmateIds.ForEach(item =>
            {
                inmateApptDetails.AoScheduleDetails.InmateId = item;
                inmateApptDetails.AoScheduleDetails.UpdatedDate = DateTime.Now;
                inmateApptDetails.ScheduleId = 0;
                if (inmateApptDetails.AoScheduleDetails.AgencyId.HasValue)
                {
                    InsertCourtSchedule(inmateApptDetails);
                }
                else
                {
                    InsertApptSchedule(inmateApptDetails);
                }
                //To add inmate schedule details in schedule history
                LoadInsertScheduleHistory(inmateApptDetails.ScheduleId, inmateApptDetails.AppointmentHistoryList);
            });
            return await _context.SaveChangesAsync();
        }


        #region Appt Bump Queue Details

        public BumpQueueVm GetBumpQueueDetails(int facilityId, string fromModule)
        {
            BumpQueueVm bumpQueueVm = new BumpQueueVm
            {
                // To get building number (3ms)
                LstBuilding = _context.HousingUnit.Where(h =>
                        (!(facilityId > 0) || h.FacilityId == facilityId)
                        && (h.HousingUnitInactive == 0 || h.HousingUnitInactive == null) && h.HousingUnitLocation != "")
                    .OrderBy(h => h.HousingUnitLocation)
                    .Select(h => h.HousingUnitLocation).Distinct().ToArray(),
                LstBumpQueue = fromModule == CommonConstants.Appt.ToString() ? null
                    : GetVisitBumpQueueMain(facilityId, true, null, null, null, null, null)
            };

            // To get left grid count 
            bumpQueueVm.LstGridInfo = bumpQueueVm.LstBumpQueue is null ?
                new List<GridInfoVm>() : GetBumpQueueCount(bumpQueueVm.LstBumpQueue);

            return bumpQueueVm;
        }

        public List<HousingDetail> GetBumpQueueHousingDetails(int? facilityId, string housingUnitLocation)
        {
            // To get housing details
            return _context.HousingUnit.Where(h => ((h.HousingUnitInactive ?? 0) == 0 || h.HousingUnitInactive == null)
                                                   && h.HousingUnitLocation == housingUnitLocation &&
                                                   ((facilityId ?? 0) <= 0 || h.FacilityId == facilityId))
                .GroupBy(g => new { g.HousingUnitLocation, g.HousingUnitNumber, g.HousingUnitListId })
                .Select(h => new HousingDetail
                {
                    HousingUnitLocation = h.Key.HousingUnitLocation,
                    HousingUnitNumber = h.Key.HousingUnitNumber,
                    HousingUnitListId = h.Key.HousingUnitListId
                }).ToList();
        }

        public BumpQueueVm GetBumpQueueInfo(int facilityId, bool isActiveBump, DateTime? startDate, DateTime? endDate,
            string housingUnitLocation, string housingUnitNumber, int? inmateId, string fromModule)
        {
            BumpQueueVm bumpQueueVm = new BumpQueueVm();
            if (fromModule == CommonConstants.Appt.ToString())
            {
                // To load appointment - bumpqueue details
                bumpQueueVm.LstBumpQueue = null;
            }
            else
            {
                // To load visit - bumpqueue details
                bumpQueueVm.LstBumpQueue = GetVisitBumpQueueMain(facilityId, isActiveBump, startDate, endDate,
                    housingUnitLocation,
                    housingUnitNumber, inmateId);
            }

            // To get left grid count 
            if (!(bumpQueueVm.LstBumpQueue is null))
            {
                bumpQueueVm.LstGridInfo = GetBumpQueueCount(bumpQueueVm.LstBumpQueue);
            }

            return bumpQueueVm;
        }

        private List<BumpQueue> GetVisitBumpQueueMain(int facilityId, bool isActiveBump, DateTime? startDate,
            DateTime? endDate, string housingUnitLocation, string housingUnitNumber, int? inmateId)
        {
            // Took 4ms
            List<BumpQueue> lstBumpQueue = _context.VisitToVisitor.Where(a =>
                a.Visit.DeleteFlag && a.Visit.BumpFlag == 1 && (isActiveBump
                    ? a.Visit.BumpClearFlag == 0 || a.Visit.BumpClearFlag == null
                    : !a.Visit.BumpDate.HasValue ||
                      a.Visit.BumpDate.Value.Date >= (startDate ?? DateTime.Today).Date &&
                      a.Visit.BumpDate.Value.Date <= (endDate ?? DateTime.Today).Date)
                && (facilityId <= 0 || a.Visit.Inmate.FacilityId == facilityId)
                && (housingUnitLocation == "" || housingUnitLocation == null ||
                    a.Visit.Inmate.HousingUnit.HousingUnitLocation == housingUnitLocation)
                && ((inmateId ?? 0) <= 0 || a.Visit.Inmate.InmateId == inmateId)).Select(a => new BumpQueue
                {
                    ApptId = a.ScheduleId,
                    ApptDate = a.Visit.StartDate,
                    ApptEndDate = a.Visit.EndDate,                    
                    InmateId = a.Visit.Inmate.InmateId,
                    InmateActive = a.Visit.Inmate.InmateActive > 0,
                    BumpClearFlag = (a.Visit.BumpClearFlag ?? 0) > 0,
                    NewAppointmentId = a.Visit.BumpNewVisitId,
                    InmateLastName = a.Visit.Inmate.Person.PersonLastName,
                    InmateFirstName = a.Visit.Inmate.Person.PersonFirstName,
                    InmateMiddleName = a.Visit.Inmate.Person.PersonMiddleName,                    
                    BumpDate = a.Visit.BumpDate,
                    BumpBy = a.Visit.BumpBy,
                    ClearDate = a.Visit.BumpClearDate,
                    ClearBy = a.Visit.BumpClearBy,
                    ClearNote = a.Visit.BumpClearNote,
                    HousingUnitId = a.Visit.Inmate.HousingUnitId,
                    DeleteFlag = a.Visit.DeleteFlag,
                    BumpFlag = a.Visit.BumpFlag > 0,
                    PersonId = a.Visit.Inmate.PersonId,
                    VisitorId = a.PersonId,
                    FrontDeskFlag = a.Visit.FrontDeskFlag,
                    VisitorType = a.Visit.VisitorType ?? 0,
                    VisitorBefore = a.Visitor.VisitorBefore,
                    VisitorIdType = a.VisitorIdType,
                    VisitToVisitorId = a.VisitToVisitorId,
                    InmateNumber = a.Visit.Inmate.InmateNumber,
                    PrimaryVisitor = new PersonInfoVm
                    {
                        PersonId = a.PersonId,
                        PersonLastName = a.Visitor.PersonLastName,
                        PersonFirstName = a.Visitor.PersonFirstName,
                        PersonDob = a.Visitor.PersonDob
                    },
                    HousingUnit = a.Visit.Inmate.HousingUnitId > 0 ? new HousingDetail {                       
                        HousingUnitLocation = a.Visit.Inmate.HousingUnit.HousingUnitLocation,
                        HousingUnitNumber = a.Visit.Inmate.HousingUnit.HousingUnitNumber,
                        HousingUnitBedNumber = a.Visit.Inmate.HousingUnit.HousingUnitBedNumber,
                        HousingUnitBedLocation = a.Visit.Inmate.HousingUnit.HousingUnitBedLocation,
                    } : new HousingDetail()                    
                }).ToList();

            return SimplifyBumpQueue(lstBumpQueue, housingUnitLocation, housingUnitNumber);
        }

        private List<BumpQueue> SimplifyBumpQueue(List<BumpQueue> lstBumpQueue, string housingUnitLocation,
            string housingUnitNumber)
        {
            if (housingUnitLocation != null &&
                housingUnitNumber != null)
            {
                // Took 9ms
                lstBumpQueue = (from h in _context.HousingUnit
                                from a in lstBumpQueue
                                where h.HousingUnitId == a.HousingUnitId &&
                                      h.HousingUnitLocation == housingUnitLocation
                                      && h.HousingUnitNumber == housingUnitNumber
                                select a).ToList();
            }

            // Add BumpBy to list
            List<int> lstPersonnelIds = lstBumpQueue.Select(a => a.BumpBy).Where(p => p.HasValue)
                .Select(p => p.Value).ToList();
            // Append ClearBy to list
            lstPersonnelIds.AddRange(
                lstBumpQueue.Select(a => a.ClearBy).Where(p => p.HasValue).Select(p => p.Value)
                    .ToList());

            // getting the person details (1ms)
            List<PersonnelVm> lstPersonDetails = _personService.GetPersonNameList(lstPersonnelIds);

            List<int> lstNewApptId = lstBumpQueue.Select(a => a.NewAppointmentId).Where(a => a.HasValue)
                .Select(a => a.Value).ToList();

            // Took 3ms
            var lstAppts = (from a in _context.Schedule
                            where lstNewApptId.Contains(a.ScheduleId)
                            select new
                            {
                                a.ScheduleId,
                                a.StartDate,
                                a.EndDate
                            }).ToList();

            List<int> lstScheduleId = lstBumpQueue.Select(s => s.ApptId).ToList();

            List<AoWizardProgressVm> lstVisitRegistrationWizardProgress = _visitRegisterService.GetVisitRegistrationWizardProgress(lstScheduleId);

            lstBumpQueue.ForEach(item =>
            {
                if ((item.NewAppointmentId ?? 0) > 0)
                {
                    item.ApptDate = lstAppts.Single(a => a.ScheduleId == item.NewAppointmentId).StartDate;
                    item.ApptEndDate = lstAppts.Single(a => a.ScheduleId == item.NewAppointmentId).EndDate;
                }

                if ((item.BumpBy ?? 0) > 0)
                {
                    item.BumpLastName = lstPersonDetails.Single(p => p.PersonnelId == item.BumpBy).PersonLastName;
                    item.BumpBadgeNumber =
                        lstPersonDetails.Single(p => p.PersonnelId == item.BumpBy).OfficerBadgeNumber;
                }

                item.RegistrationProgress =
                    lstVisitRegistrationWizardProgress.FirstOrDefault(w => w.ScheduleId == item.ApptId);

                if ((item.ClearBy ?? 0) <= 0) return;
                item.ClearLastName = lstPersonDetails.Single(p => p.PersonnelId == item.ClearBy).PersonLastName;
                item.ClearBadgeNumber = lstPersonDetails.Single(p => p.PersonnelId == item.ClearBy).OfficerBadgeNumber;
              
            });
            
            return lstBumpQueue.ToList();
        }

        private List<GridInfoVm> GetBumpQueueCount(List<BumpQueue> lstBumpQueue)
        {
            List<GridInfoVm> lstGridInfo = lstBumpQueue.GroupBy(a => new { a.ApptLocation })
                .Select(a => new GridInfoVm
                {
                    Id = 0,
                    Name = a.Key.ApptLocation,
                    Cnt = a.Count()
                }).ToList();

            lstGridInfo.Insert(0,
                new GridInfoVm { Id = 0, Name = CommonConstants.ALL.ToString(), Cnt = lstGridInfo.Sum(a => a.Cnt) });

            return lstGridInfo;
        }

        public async Task<int> ClearBumpQueue(KeyValuePair<int, string> clearBumpQueue)
        {
            // TODO
            Visit scheduleDetails = _context.Visit.Single(s => s.ScheduleId == clearBumpQueue.Key);
            scheduleDetails.BumpClearFlag = 1;
            scheduleDetails.BumpClearNote = clearBumpQueue.Value;
            scheduleDetails.BumpClearBy = _personnelId;
            scheduleDetails.BumpClearDate = DateTime.Now;
            return await _context.SaveChangesAsync();
            //return await Task.FromResult(1);
        }

        public List<KeyValuePair<int, string>> GetApptDeleteReason() =>
            _commonService.GetLookupKeyValuePairs(LookupConstants.APPTDELREAS);

        public async Task<int> DeleteInmateAppointment(DeleteUndoInmateAppt deleteInmateAppt)
        {
            ScheduleInmate appt = _context.ScheduleInmate
                .Single(x => x.ScheduleId == deleteInmateAppt.ScheduleId);

            if (deleteInmateAppt.IsRecurrence || (appt is Visit))
            {
                appt.DeleteReason = deleteInmateAppt.DeleteReason;
                appt.Notes = deleteInmateAppt.DeleteNote;
                appt.DeleteDate = DateTime.Now;
                appt.DeleteBy = _personnelId;
                appt.DeleteFlag = true;
            }
            else if (!deleteInmateAppt.IsRecurrence && deleteInmateAppt.ScheduleDate.HasValue)
            {
                ScheduleExclude apptOccur = new ScheduleExclude
                {
                    ScheduleId = deleteInmateAppt.ScheduleId,
                    ExcludedDate = deleteInmateAppt.ScheduleDate.Value,
                    ExcludeReason = deleteInmateAppt.DeleteReason,
                    ExcludeNote = deleteInmateAppt.DeleteNote,
                    CreateDate = DateTime.Now,
                    CreateBy = _personnelId
                };
                _context.ScheduleExclude.Add(apptOccur);
            }
            _interfaceEngine.Export(new ExportRequestVm
            {
                EventName = EventNameConstants.APPOINTMENTDELETE,
                PersonnelId = _personnelId,
                Param1 = _context.Inmate.SingleOrDefault(a => a.InmateId == appt.InmateId)?
                .PersonId.ToString(),
                Param2 = appt.ScheduleId.ToString()
            });
            return await _context.SaveChangesAsync();
        }

        public async Task<int> UndoInmateAppointment(DeleteUndoInmateAppt undoInmateAppt)
        {
            ScheduleInmate appt =
                _context.ScheduleInmate.Single(x => x.ScheduleId == undoInmateAppt.ScheduleId);

            if (undoInmateAppt.IsRecurrence)
            {
                appt.DeleteReason = null;
                appt.Notes = null;
                appt.DeleteDate = null;
                appt.DeleteBy = null;
                appt.DeleteFlag = false;
            }

            if (!undoInmateAppt.IsRecurrence && undoInmateAppt.ApptOccurId.HasValue)
            {
                ScheduleExclude apptOccur =
                    _context.ScheduleExclude.FirstOrDefault(x =>
                        x.ScheduleExcludeId == undoInmateAppt.ApptOccurId.Value);
                if (apptOccur != null)
                {
                    _context.Remove(apptOccur);
                }
            }
            _interfaceEngine.Export(new ExportRequestVm
            {
                EventName = EventNameConstants.APPOINTMENTDELETE,
                PersonnelId = _personnelId,
                Param1 = _context.Inmate.SingleOrDefault(a => a.InmateId == appt.InmateId)?
                .PersonId.ToString(),
                Param2 = appt.ScheduleId.ToString()
            });
            return await _context.SaveChangesAsync();

        }

        #endregion

        public AppointmentLocationVm ProgramAppointmentDropDowns(int facilityId)
        {
            AppointmentLocationVm progmApptDropDowns = new AppointmentLocationVm()
            {
                LocationList = _context.Privileges
                    .Where(p => p.InactiveFlag == 0 && p.ShowInProgram == 1 &&
                    (p.FacilityId == facilityId || !p.FacilityId.HasValue || p.FacilityId == 0))
                    .Select(p =>
                        new LocationList
                        {
                            ApptLocationId = p.PrivilegeId,
                            ApptRequireCourtLink = p.AppointmentRequireCourtLink,
                            ApptLocation = p.PrivilegeDescription,
                            ApptAllowFutureEndDate = p.AppointmentAllowFutureEndDate == 1,
                            ApptAlertEndDate = p.AppointmentAlertApptEndDate == 1,
                            DefaultCourtId = p.DefaultCourtId
                        }).OrderBy(x => x.ApptLocation).ToList(),
                AppointmentReason = _commonService.GetLookupKeyValuePairs(LookupConstants.APPTREAS),
                //To get Appointment Type
                AppointmentType = _commonService.GetLookupKeyValuePairs(LookupConstants.APPTYPE),
                Programs = _context.Program.Where(a => a.DeleteFlag == 0 && (!a.ProgramCategory.DeleteFlag.HasValue
                  || a.ProgramCategory.DeleteFlag == 0 && a.ProgramCategory.FacilityId == facilityId))
                .Select(a => new ProgramAndClass
                {
                    ProgramId = a.ProgramId,
                    ProgramCategory = a.ProgramCategory.ProgramCategory1,
                    ClassOrServiceNumber = a.ClassOrServiceNumber,
                    ClassOrServiceName = a.ClassOrServiceName
                }).ToList()
            };
            return progmApptDropDowns;
        }

        public ProgramInstructor GetProgramInstructorDetails(int programId, int appointmentId)
        {
            List<ProgramInstructorCert> programInstructorCerts = _context.ProgramInstructorCert
                .Where(pi => pi.ProgramClassId == programId).ToList();
            int[] personnelId = _context.ProgramInstructorAssign
                .Where(a => a.ScheduleId == appointmentId && (a.DeleteFlag == 0 || !a.DeleteFlag.HasValue))
                .Select(a => a.PersonnelId).ToArray();
            ProgramInstructor programInstructor = new ProgramInstructor
            {
                Assigned = _context.Personnel
                .Where(a => personnelId.Any(p => p == a.PersonnelId))
                .Select(a => new Instructor
                {
                    Personnel = new PersonnelVm
                    {
                        PersonnelId = a.PersonnelId,
                        PersonLastName = a.PersonNavigation.PersonLastName,
                        PersonFirstName = a.PersonNavigation.PersonFirstName,
                        OfficerBadgeNumber = a.OfficerBadgeNum
                    },
                    DeleteFlag = programInstructorCerts.Where(p => p.ProgramClassId == programId
                     && p.PersonnelId == a.PersonnelId).Select(p => p.DeleteFlag)
                    .FirstOrDefault(),
                    ProgramInstructorCertId = programInstructorCerts.Where(p => p.ProgramClassId == programId
                      && p.PersonnelId == a.PersonnelId).Select(p => p.ProgramInstructorCertId)
                    .FirstOrDefault()
                }).OrderBy(a => a.Personnel.PersonLastName)
                .ThenBy(a => a.Personnel.PersonFirstName)
                .ThenBy(a => a.Personnel.OfficerBadgeNumber).ToList()
            };
            List<Instructor> instructors = _context.Personnel
                .Where(a => !personnelId.Any(p => p == a.PersonnelId) && a.ProgramInstructorFlag == true)
                .Select(a => new Instructor
                {
                    Personnel = new PersonnelVm
                    {
                        PersonnelId = a.PersonnelId,
                        PersonLastName = a.PersonNavigation.PersonLastName,
                        PersonFirstName = a.PersonNavigation.PersonFirstName,
                        OfficerBadgeNumber = a.OfficerBadgeNumber
                    },
                    DeleteFlag = programInstructorCerts.Where(p => p.ProgramClassId == programId
                     && p.PersonnelId == a.PersonnelId).Select(p => p.DeleteFlag)
                    .FirstOrDefault(),
                    ProgramInstructorCertId = programInstructorCerts.Where(p => p.ProgramClassId == programId
                      && p.PersonnelId == a.PersonnelId).Select(p => p.ProgramInstructorCertId)
                    .FirstOrDefault()
                }).OrderBy(a => a.Personnel.PersonLastName)
                .ThenBy(a => a.Personnel.PersonFirstName)
                .ThenBy(a => a.Personnel.OfficerBadgeNumber).ToList();
            programInstructor.Certified = instructors.Where(a => (a.DeleteFlag == 0 || !a.DeleteFlag.HasValue)
            && a.ProgramInstructorCertId > 0).ToList();
            programInstructor.NotCertified = instructors.Where(a => a.DeleteFlag == 1 ||
            a.ProgramInstructorCertId == 0 || !a.ProgramInstructorCertId.HasValue).ToList();
            return programInstructor;
        }

        // to insert schedule save history
        private void LoadInsertScheduleHistory(int scheduleId, string scheduleHistoryList)
        {
            //To save Appointment History Details
            ScheduleSaveHistory scheduleHistory = new ScheduleSaveHistory
            {
                CreateDate = DateTime.Now,
                ScheduleId = scheduleId,
                CreatedBy = _personnelId,
                ScheduleHistoryList = scheduleHistoryList
            };
            _context.ScheduleSaveHistory.Add(scheduleHistory);
            _context.SaveChanges();
        }
        public List<ScheduleSaveHistoryVm> GetScheduleSaveHistory(int scheduleId) => _context.ScheduleSaveHistory
            .Where(w => w.ScheduleId == scheduleId).Select(s => new ScheduleSaveHistoryVm
            {
                ScheduleId = s.ScheduleId,
                ScheduleHistoryList = s.ScheduleHistoryList,
                CreatedDate = s.CreateDate,
                CreatedByDetails = new PersonnelVm
                {
                    PersonFirstName = s.CreateByNavigation.PersonNavigation.PersonFirstName,
                    PersonLastName = s.CreateByNavigation.PersonNavigation.PersonLastName,
                    OfficerBadgeNumber = s.CreateByNavigation.OfficerBadgeNumber
                },
                CreatedBy = s.CreatedBy
            }).OrderByDescending(o => o.CreatedDate).ToList();
    }
}