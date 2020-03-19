﻿using GenerateTables.Models;
using ServerAPI.Utilities;
using ServerAPI.ViewModels;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using ScheduleWidget.Schedule;
using ScheduleWidget.Common;
using Microsoft.EntityFrameworkCore;

namespace ServerAPI.Services
{
    public class AppointmentViewerService : IAppointmentViewerService
    {
        private readonly AAtims _context;
        private readonly IPhotosService _photos;
        private readonly ICommonService _commonService;

        public AppointmentViewerService(AAtims context, ICommonService commonService,
            IPhotosService photosService)
        {
            _context = context;
            _photos = photosService;
            _commonService = commonService;
        }

        private List<InmateDetailsList> GetInmateDetails(List<int> inmateIds) =>
            _context.Inmate.Where(x => inmateIds.Contains(x.InmateId)).Select(a => new InmateDetailsList
            {
                InmateId = a.InmateId,
                PersonId = a.PersonId,
                PersonFirstName = a.Person.PersonFirstName,
                PersonMiddleName = a.Person.PersonMiddleName,
                PersonLastName = a.Person.PersonLastName,
                PersonDob = a.Person.PersonDob,
                InmateNumber = a.InmateNumber,
                FacilityId = a.FacilityId,
                TrackLocation = a.InmateCurrentTrack,
                HousingUnitListId = a.HousingUnitId ?? 0,
                HousingLocation = a.HousingUnit.HousingUnitLocation,
                HousingNumber = a.HousingUnit.HousingUnitNumber,
                HousingUnitBedLocation = a.HousingUnit.HousingUnitBedLocation,
                HousingUnitBedNumber = a.HousingUnit.HousingUnitBedNumber,
                ClassificationReason = a.InmateClassification.InmateClassificationReason,
                InmateActive = a.InmateActive,
                PhotoPath = _photos.GetPhotoByCollectionIdentifier(a.Person.Identifiers)
            }).ToList();

        // Appointment check with inmateid, from date and to date
        public List<AoAppointmentVm> AppointmentViewer(CalendarInputs inputs)
        {
            List<CalendarType> lstCalendarType = new List<CalendarType>();
            inputs.CalendarTypes.Split(',').ToList().ForEach(ii =>
            {
                lstCalendarType.Add((CalendarType)Enum.Parse(typeof(CalendarType), ii));
            });

            List<int> housingUnitListIds = new List<int>();
            if (inputs.HousingGroupId > 0)
            {
                housingUnitListIds = _context.HousingGroupAssign
                    .Where(w => w.HousingGroupId == inputs.HousingGroupId && w.HousingUnitListId.HasValue &&
                        w.DeleteFlag != 1)
                    .Select(s => s.HousingUnitListId ?? 0).ToList();
            }

            List<int> lstInmate = _context.Inmate.Where(inm => inm.FacilityId == inputs.FacilityId &&
                    inm.InmateActive == 1 &&
                    (inputs.HousingLocation == null || inm.HousingUnit.HousingUnitLocation == inputs.HousingLocation &&
                        (inputs.HousingNumber == null || inm.HousingUnit.HousingUnitNumber == inputs.HousingNumber)) &&
                    (!inputs.HousingGroupId.HasValue || inputs.HousingGroupId == 0 ||
                        inm.HousingUnitId.HasValue && housingUnitListIds.Contains(inm.HousingUnit.HousingUnitListId)) &&
                    (!inputs.InmateId.HasValue || inputs.InmateId == 0 || inm.InmateId == inputs.InmateId))
                .Select(inm => inm.InmateId).ToList();

            List<InmateDetailsList> lstPersonDetails = GetInmateDetails(lstInmate);

            List<AoAppointmentVm> inmateApptList = new List<AoAppointmentVm>();
            List<AoAppointmentVm> schList = new List<AoAppointmentVm>();

            if (lstCalendarType.Contains(CalendarType.CourtAppointment) ||
                lstCalendarType.Contains(CalendarType.Appointment))
            {
                schList = _context.ScheduleInmate.Where(a => !a.DeleteFlag && a.LocationId.HasValue &&
                    !(a is Visit) && (a.IsSingleOccurrence && a.StartDate.Date == a.EndDate.Value.Date &&
                        inputs.FromDate <= a.StartDate && inputs.ToDate >= a.StartDate ||
                        !a.IsSingleOccurrence && (inputs.FromDate >= a.StartDate &&
                            inputs.ToDate <= (a.EndDate ?? inputs.ToDate) ||
                            inputs.FromDate <= (a.EndDate ?? inputs.ToDate) &&
                            inputs.ToDate > a.StartDate.Date)
                        || a.IsSingleOccurrence && a.StartDate.Date != a.EndDate.Value.Date &&
                        a.EndDate.Value >= inputs.FromDate
                    )).Select(sch => new AoAppointmentVm
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
                    LocationDetail = sch.LocationDetail,
                    IsSingleOccurrence = sch.IsSingleOccurrence,
                    DayInterval = sch.DayInterval,
                    WeekInterval = sch.WeekInterval,
                    FrequencyType = sch.FrequencyType,
                    QuarterInterval = sch.QuarterInterval,
                    MonthOfQuarterInterval = sch.MonthOfQuarterInterval,
                    MonthOfYear = sch.MonthOfYear,
                    DayOfMonth = sch.DayOfMonth,
                    Discriminator = sch is ScheduleCourt ? "COURT" :
                        sch is Visit ? "VISIT" :
                        "APPT",
                    InmateDetails = lstPersonDetails.SingleOrDefault(inm => inm.InmateId == sch.InmateId),
                    CreateBy = sch.CreateBy
                }).ToList();

                schList = schList.Where(sch => lstInmate.Contains(sch.InmateId ?? 0)).ToList();

                if (!lstCalendarType.Contains(CalendarType.CourtAppointment))
                {
                    schList = schList.Where(sch => sch.Discriminator != "COURT").ToList();
                }

                if (!lstCalendarType.Contains(CalendarType.Appointment) &&
                    lstCalendarType.Contains(CalendarType.CourtAppointment))
                {
                    schList = schList.Where(sch => sch.Discriminator == "COURT").ToList();
                }

                if (inputs.ApptReasonIndex.HasValue && inputs.ApptReasonIndex > 0)
                {
                    schList = schList.Where(sch => sch.ReasonId == inputs.ApptReasonIndex).ToList();
                }

                if (inputs.ApptTypeIndex.HasValue && inputs.ApptTypeIndex > 0)
                {
                    schList = schList.Where(sch => sch.TypeId == inputs.ApptTypeIndex).ToList();
                }
            }

            if (lstCalendarType.Contains(CalendarType.Visitation))
            {
                schList.AddRange(GetVisitAppointment(inputs, lstPersonDetails, lstInmate));
            }

            if (inputs.LocationId.HasValue && inputs.LocationId > 0)
            {
                schList = schList.Where(sch => sch.LocationId == inputs.LocationId).ToList();
            }

            int[] scheduleId = schList.Select(i => i.ScheduleId).ToArray();

            List<KeyValuePair<int, string>> lstReasonLookup = _commonService.GetLookupKeyValuePairs(LookupConstants.APPTREAS);
            List<KeyValuePair<int, string>> lstTypeLookup = _commonService.GetLookupKeyValuePairs(LookupConstants.APPTYPE);

            if (schList.Count <= 0) return inmateApptList;
            foreach (AoAppointmentVm sch in schList)
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

                if (lstCalendarType.Contains(CalendarType.CourtAppointment))
                {
                    ScheduleCourt scheduleCourt = _context.ScheduleCourt
                        .Include(ii => ii.Agency).Include(ii => ii.AgencyCourtDept)
                        .SingleOrDefault(aoa => aoa.ScheduleId == sch.ScheduleId);

                    if (!(scheduleCourt is null))
                    {
                        sch.AgencyId = scheduleCourt.AgencyId;
                        sch.DeptId = scheduleCourt.AgencyCourtDeptId;
                        sch.Court = scheduleCourt.Agency?.AgencyName;
                        sch.Dept = scheduleCourt.AgencyCourtDept?.DepartmentName;

                        sch.BookingNo = _context.ScheduleCourtArrest.Where(sc => sc.ScheduleId == sch.ScheduleId)
                            .Select(ii => ii.Arrest.ArrestBookingNo)
                            .ToList();
                    }
                }
                if (sch.ReasonId.HasValue && sch.ReasonId > 0)
                {
                    sch.Reason = lstReasonLookup.SingleOrDefault(ii => ii.Key == sch.ReasonId).Value;
                }
                if (sch.TypeId.HasValue && sch.TypeId > 0)
                {
                    sch.ApptType = lstTypeLookup.SingleOrDefault(ii => ii.Key == sch.TypeId).Value;
                }

                if (materializedSchedule.IsSingleOccurrence || sch.Discriminator == "VISIT")
                {
                    AoAppointmentVm app = sch;
                    app.StartDate = materializedSchedule.StartDate;
                    app.EndDate = materializedSchedule.EndDate;
                    inmateApptList.Add(app);
                }
                else
                {
                    inmateApptList.AddRange(materializedSchedule.Occurrences(during).Select(date => new AoAppointmentVm
                    {
                        StartDate = date.Date.Add(during.StartDateTime.TimeOfDay),
                        Duration = sch.Duration,
                        EndDate = sch.EndDate.HasValue
                            ? date.Date.Add(during.EndDateTime.TimeOfDay)
                            : date.Date.Add(during.StartDateTime.TimeOfDay).Add(sch.Duration),
                        EndDateNull = !sch.EndDate.HasValue,
                        LocationId = sch.LocationId,
                        ScheduleId = sch.ScheduleId,
                        InmateId = sch.InmateId,
                        ReasonId = sch.ReasonId,
                        Reason = sch.Reason,
                        ApptType = sch.ApptType,
                        BookingNo = sch.BookingNo,
                        TypeId = sch.TypeId,
                        IsSingleOccurrence = sch.IsSingleOccurrence,
                        FacilityId = sch.FacilityId,
                        InmateDetails = sch.InmateDetails,
                        LocationDetail = sch.LocationDetail,
                        Notes = sch.Notes,
                        Discriminator = sch.Discriminator,
                        AgencyId = sch.AgencyId,
                        DeptId = sch.DeptId,
                        Court = sch.Court,
                        Dept = sch.Dept,
                        CreateBy = sch.CreateBy
                    }));
                }
            }

            List<Privileges> lstPrivileges = _context.Privileges
                .Where(pri => inmateApptList.Select(ii => ii.LocationId).Contains(pri.PrivilegeId)).ToList();

            inmateApptList = inmateApptList.Where(w =>
            {
                Debug.Assert(w.EndDate != null, "AppointmentService: materializedSchedule.EndDate != null");
                if (w.IsSingleOccurrence && w.StartDate.Date != w.EndDate.Value.Date)
                {
                    w.EndDate = w.StartDate.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
                }
                return true;
            }).ToList();

            inmateApptList = inmateApptList.Where(w =>
            {
                Debug.Assert(w.EndDate != null, "AppointmentService: w.EndDate != null");
                w.Location = w.LocationId != 0
                    ? lstPrivileges.Single(pri => pri.PrivilegeId == w.LocationId).PrivilegeDescription : "";
                return inputs.FromDate <= w.StartDate && inputs.ToDate >= w.StartDate ||
                    w.IsSingleOccurrence && w.StartDate.Date != w.EndDate.Value.Date;
            }).ToList();

            //Taking Appointment Occurance for Occurance list
            List<ApptOccurance> apptOccur = _context.ScheduleExclude.Where(oc =>
                scheduleId.Contains(oc.ScheduleId)).Select(a => new ApptOccurance
            {
                OccuranceId = a.ScheduleExcludeId,
                OccuranceDate = a.ExcludedDate,
                DeleteReason = a.ExcludeReason,
                DeleteReasonNote = a.ExcludeNote,
                ScheduleId = a.ScheduleId
            }).ToList();

            //To get the Inactive Occurance list 
            inmateApptList = inmateApptList.Where(i => !apptOccur.Any(a =>
                a.ScheduleId == i.ScheduleId &&
                a.OccuranceDate.Date == i.StartDate.Date)).ToList();

            if (!inputs.IsFromViewer) return inmateApptList;
            int[] inmateIds = inmateApptList.Select(ii => ii.InmateId ?? 0).ToArray();

            List<InmateTrak> lstInmateTracks = _context.InmateTrak.Where(i =>
                inmateIds.Contains(i.InmateId)).ToList();

            inmateApptList = inmateApptList.Where(w =>
            {
                Debug.Assert(w.EndDate != null, "AppointmentService: materializedSchedule.EndDate != null");
                if (w.StartDate.Date <= DateTime.Now.Date && w.EndDate.Value.Date >= DateTime.Now.Date)
                {
                    w.InmateTraked = lstInmateTracks.Count(
                            ii => ii.InmateTrakLocationId == w.LocationId && ii.InmateId == w.InmateId) == 0
                        || lstInmateTracks.Count(ii =>
                            ii.InmateTrakLocationId == w.LocationId && !ii.InmateTrakDateIn.HasValue &&
                            ii.InmateId == w.InmateId) > 0;
                }
                return true;
            }).ToList();

            return inmateApptList;
        }

        private List<AoAppointmentVm> GetVisitAppointment(CalendarInputs inputs,
            List<InmateDetailsList> lstPersonDetails, List<int> lstInmate)
        {
            List<AoAppointmentVm> lstAppointment = _context.Visit.Where(a =>
                !a.DeleteFlag && a.LocationId.HasValue && a.InmateId.HasValue && a.EndDate.HasValue &&
                inputs.FromDate <= a.StartDate && inputs.ToDate >= a.StartDate &&
                a.CompleteRegistration && a.RegFacilityId == inputs.FacilityId &&
                a.VisitDenyFlag != 1 && !a.CompleteVisitFlag &&
                (inputs.InmateId.HasValue && inputs.InmateId != 0 ? a.InmateId == inputs.InmateId
                    : lstInmate.Contains(a.InmateId ?? 0))
            ).Select(ii => new AoAppointmentVm
            {
                StartDate = ii.StartDate,
                CreateBy = ii.CreateBy,
                EndDate = ii.EndDate,
                LocationId = ii.LocationId ?? 0,
                ScheduleId = ii.ScheduleId,
                InmateId = ii.InmateId ?? 0,
                ReasonId = ii.ReasonId,
                TypeId = ii.TypeId,
                Duration = ii.Duration,
                DeleteReason = ii.DeleteReason,
                Notes = ii.Notes,
                IsSingleOccurrence = ii.IsSingleOccurrence,
                DayInterval = ii.DayInterval,
                WeekInterval = ii.WeekInterval,
                FrequencyType = ii.FrequencyType,
                QuarterInterval = ii.QuarterInterval,
                MonthOfQuarterInterval = ii.MonthOfQuarterInterval,
                MonthOfYear = ii.MonthOfYear,
                DayOfMonth = ii.DayOfMonth,
                InmateDetails = lstPersonDetails.Single(inm => inm.InmateId == ii.InmateId),
                FacilityId = ii.RegFacilityId,
                LocationDetail = ii.LocationDetail,
                VisitType = ii.VisitorType ?? 0,
                // TODO Why are we retrieving Discriminator?
                Discriminator = "VISIT",
                VisitAdditionAdultCount = ii.VisitAdditionAdultCount,
                VisitAdditionChildCount = ii.VisitAdditionChildCount
            }).ToList();
            List<int> scheduleIds = lstAppointment.Select(ii => ii.ScheduleId).ToList();
            List<VisitDetails> lstVisitToVisitor = _context.VisitToVisitor.Where(ii =>
                scheduleIds.Any(sId => sId == ii.ScheduleId) &&
                !ii.PrimaryVisitorId.HasValue).Select(s => new VisitDetails
            {
                VisitToVisitorId = s.VisitToVisitorId,
                ScheduleId = s.ScheduleId,
                PrimaryVisitor = new PersonInfoVm
                {
                    PersonLastName = s.Visitor.PersonLastName,
                    PersonFirstName = s.Visitor.PersonFirstName
                },
                PersonId = s.PersonId
            }).ToList();

            IQueryable<VisitorToInmate> visitorToInmate = _context.VisitorToInmate.Where(a => !a.DeleteFlag.HasValue
                || a.DeleteFlag == 0);

            List<LookupVm> lstLookup = _commonService.GetLookups(new[]
            {
                LookupConstants.RELATIONS,
                LookupConstants.VISTYPE
            });

            lstAppointment.ForEach(appt =>
            {
                appt.VisitToAdult = new List<VisitDetails>();
                appt.VisitToChild = new List<VisitToChild>();
                appt.PrimaryVisitorDetails = new VisitDetails();
                if (appt.VisitType > 0)
                {
                    appt.ProfessionalType = lstLookup.SingleOrDefault(l => l.LookupIndex == appt.VisitType &&
                        l.LookupType == LookupConstants.VISTYPE)?.LookupDescription;
                }

                appt.PrimaryVisitorDetails = lstVisitToVisitor.Single(a => a.ScheduleId == appt.ScheduleId);

                int? relationship = visitorToInmate.Last(v =>
                    v.VisitorId == appt.PrimaryVisitorDetails.PersonId
                    && lstInmate.Any(a => a == v.InmateId)).VisitorRelationship;

                if (relationship > 0)
                {
                    appt.PrimaryVisitorDetails.Relationship = lstLookup.Single(l =>
                        l.LookupIndex == relationship
                        && l.LookupType == LookupConstants.RELATIONS).LookupDescription;
                }

                if (appt.VisitAdditionAdultCount > 0)
                {
                    List<VisitToVisitor> adultVisitor = _context.VisitToVisitor.Where(w => w.PrimaryVisitorId ==
                        appt.PrimaryVisitorDetails.VisitToVisitorId).ToList();

                    adultVisitor.ForEach(adult =>
                    {
                        VisitDetails visitDetails = visitorToInmate.Where(v => v.VisitorId == adult.PersonId).Select(
                            s => new VisitDetails
                            {
                                Relationship = lstLookup.SingleOrDefault(l => l.LookupIndex == s.VisitorRelationship
                                    && l.LookupType == LookupConstants.RELATIONS).LookupDescription,
                                PrimaryVisitor = new PersonInfoVm
                                {
                                    PersonLastName = s.Visitor.PersonLastName,
                                    PersonFirstName = s.Visitor.PersonFirstName
                                }
                            }).Last();
                        appt.VisitToAdult.Add(visitDetails);
                    });
                }

                if (appt.VisitAdditionChildCount > 0)
                {
                    appt.VisitToChild = _context.VisitToChild.Where(c => c.VisitScheduleId ==
                        appt.ScheduleId).Select(s => new VisitToChild
                    {
                        VisitChildName = s.VisitChildName
                    }).ToList();
                }

            });
            return lstAppointment;
        }

        private List<ApptInmateDetails> GetApptHousingList(List<ApptTracking> lstApptTracking, List<ApptTracking> appTraking)
        {
            List<ApptInmateDetails> appTracDet = new List<ApptInmateDetails>();

            lstApptTracking.ForEach(item =>
            {
                foreach (int itemApptId in item.LstAppointmentId)
                {
                    int? inmateId = (_context.ScheduleCourt
                        .SingleOrDefault(a => a.ScheduleId == itemApptId && a.AgencyId > 0)?.InmateId ?? _context.ScheduleInmate
                        .SingleOrDefault(a => a.ScheduleId == itemApptId)?.InmateId) ?? _context.Visit
                        .SingleOrDefault(a => a.ScheduleId == itemApptId)?.InmateId;

                    ApptInmateDetails lstApp = _context.Inmate
                        .Where(i => i.InmateId == inmateId)
                        .Select(x => new ApptInmateDetails
                        {
                            ScheduleId = itemApptId,
                            HousingUnitId = x.HousingUnitId,
                            HousingUnitLocation = x.HousingUnit != null ? x.HousingUnit.HousingUnitLocation : null,
                            HousingUnitNumber = x.HousingUnit != null ? x.HousingUnit.HousingUnitNumber : null,
                            HousingUnitListId = x.HousingUnit.HousingUnitListId,
                            InmateDetail = new PersonTracking
                            {
                                PersonId = x.PersonId,
                                InmateNumber = x.InmateNumber,
                                InmateCurrentTrack = x.InmateCurrentTrack,
                                InmateCurrentTrackId = x.InmateCurrentTrackId,
                                InmateId = x.InmateId,
                                InmateTrack = GetInmateTrack(itemApptId),
                                HousingUnitBedLocation =
                                    x.HousingUnit != null ? x.HousingUnit.HousingUnitBedLocation : null,
                                HousingUnitBedNumber =
                                    x.HousingUnit != null ? x.HousingUnit.HousingUnitBedNumber : null
                            }
                        }).FirstOrDefault();

                    if (lstApp == null) continue;
                    PersonVm personVm = _context.Person.Where(p => p.PersonId == lstApp.InmateDetail.PersonId)
                        .Select(a => new PersonVm
                        {
                            PersonFirstName = a.PersonFirstName,
                            PersonLastName = a.PersonLastName,
                            PersonMiddleName = a.PersonMiddleName
                        }).Single();
                    List<Identifiers> identifiers = _context.Identifiers
                        .Where(i => i.PersonId == lstApp.InmateDetail.PersonId).ToList();
                    Person person = new Person
                    {
                        PersonId = personVm.PersonId,
                        Identifiers = identifiers
                    };
                    lstApp.InmateDetail.PhotoFilePath = _photos.GetPhotoByPerson(person);
                    lstApp.ImageName = _photos.GetPhotoByCollectionIdentifier(identifiers);
                    lstApp.InmateDetail.PersonFirstName = personVm.PersonFirstName;
                    lstApp.InmateDetail.PersonLastName = personVm.PersonLastName;
                    lstApp.InmateDetail.PersonMiddleName = personVm.PersonMiddleName;

                    lstApp.ApptStartDate = item.ApptStartDate;
                    lstApp.ApptEndDate = item.ApptEndDate;
                    lstApp.ApptLocation = item.ApptLocation;
                    lstApp.ApptLocationId = item.ApptLocationId;
                    lstApp.AgencyId = item.AgencyId;
                    lstApp.AgencyName = item.AgencyName;
                    lstApp.RefusalFlag = item.RefusalFlag;
                    lstApp.ApptAlertEndDate = item.ApptAlertEndDate;
                    lstApp.AppointmentEndDate = appTraking.First(app => app.ScheduleId == itemApptId).AppointmentEndDate;
                    lstApp.ReoccurFlag = item.ReoccurFlag;
                    lstApp.VisitFlag = item.VisitFlag;
                    lstApp.HistoryScheduleId = itemApptId;
                    appTracDet.Add(lstApp);
                }
            });

            int[] inmateIds = appTracDet.Select(a => a.InmateDetail.InmateId ?? 0).ToArray();

            List<InmateTrak> lstInmateTracks = _context.InmateTrak.Where(i =>
                inmateIds.Contains(i.InmateId)).ToList();

            appTracDet.ForEach(item =>
            {
                List<InmateTrak> lstEachInmateTracks = lstInmateTracks
                    .Where(i => i.InmateId == item.InmateDetail.InmateId)
                    .ToList();
                int inmateCurrentTrackId = item.InmateDetail.InmateCurrentTrackId ?? 0;

                item.InmateDetail.InmateCurrentTrakDateOut = lstEachInmateTracks.LastOrDefault(w =>
                        w.InmateTrakLocationId == inmateCurrentTrackId && !w.InmateTrakDateIn.HasValue)
                    ?.InmateTrakDateOut;
                item.InmateDetail.EnrouteStartOut = lstEachInmateTracks.LastOrDefault(w =>
                        w.InmateTrakLocationId == inmateCurrentTrackId && !w.InmateTrakDateIn.HasValue)
                    ?.EnrouteStartOut;
                item.InmateDetail.InmateCurrentTrakDateIn = lstEachInmateTracks.LastOrDefault(w =>
                        w.InmateTrakLocationId == inmateCurrentTrackId)
                    ?.InmateTrakDateIn;
                item.EnrouteLocationId = lstEachInmateTracks.LastOrDefault(w =>
                        w.InmateTrakLocationId == inmateCurrentTrackId)
                    ?.EnrouteFinalLocationId;
                item.DateOut = lstEachInmateTracks.Select(se => se.InmateTrakDateOut).LastOrDefault();
            });

            return appTracDet.OrderBy(app => app.HousingUnitId).ToList();
        }

        public TrackingSchedule GetTrackingSchedule(CalendarInputs inputs)
        {
            TrackingSchedule model = new TrackingSchedule();

            List<Privileges> lstPrivileges = _context.Privileges.ToList();

            List<ApptTracking> lstApptDetails = AppointmentViewer(inputs).Select(s =>
                new ApptTracking
                {
                    ScheduleId = s.ScheduleId,
                    ApptStartDate = s.StartDate.ToString("HH:mm"),
                    ApptEndDate = s.EndDate?.ToString("HH:mm"),
                    ApptLocation = s.Location,
                    ApptLocationId = s.LocationId,
                    ReoccurFlag = s.IsSingleOccurrence ? 1 : 0,
                    DeleteFlag = s.DeleteFlag,
                    AppointmentEndDate = s.EndDate,
                    AppointmentStartDate = s.StartDate,
                    AgencyId = s.AgencyId,
                    DeptId = s.DeptId
                }).ToList();

            List<ApptTracking> lstLocationApptTracking = lstApptDetails.GroupBy(g => new
            {
                g.ApptStartDate,
                g.ApptEndDate,
                g.ApptLocation,
                g.ApptLocationId
            }).Select(s => new ApptTracking
            {
                ApptStartDate = s.Key.ApptStartDate,
                ApptEndDate = s.Key.ApptEndDate,
                ApptLocation = s.Key.ApptLocation,
                ApptLocationId = s.Key.ApptLocationId,
                Count = s.Count(),
                ReoccurFlag = s.Select(x => x.ReoccurFlag).FirstOrDefault(),
                LstAppointmentId = s.Select(x => x.ScheduleId).ToArray()
            }).OrderBy(o => o.ApptStartDate).ToList();

            int[] locationIds = lstLocationApptTracking
                .Select(i => i.ApptLocationId ?? 0).ToArray();

            if (inputs.IsFromTransfer)
            {
                int[] transferLocationIds = lstPrivileges
                    .Where(p => p.TransferFlag == 1 && p.FacilityId > 0 && locationIds.Contains(p.PrivilegeId))
                    .Select(p => p.PrivilegeId).ToArray();

                lstLocationApptTracking = lstLocationApptTracking
                    .Where(a => transferLocationIds.Contains(a.ApptLocationId ?? 0)).ToList();
            }

            lstLocationApptTracking.ForEach(itm =>
            {
                itm.DisplayFlag = lstApptDetails.Count(w =>
                    w.ApptStartDate == itm.ApptStartDate
                    && w.ReoccurFlag == 1 && w.ApptEndDate == itm.ApptEndDate
                    && w.ApptLocationId == itm.ApptLocationId && itm.ApptAlertEndDate
                    && w.AppointmentEndDate.HasValue && w.AppointmentEndDate <= DateTime.Now
                    && w.ApptEndDate != "23:59") > 0;
                itm.EndDateFlag = _context.InmateAppointmentTrack.Any(w =>
                    itm.LstAppointmentId.Contains(w.ScheduleId) && w.InmateTrakDateIn == null);
                itm.VisitFlag = _context.Visit.Any(w => itm.LstAppointmentId.Contains(w.ScheduleId));

                Privileges privilege = lstPrivileges.Single(w => w.PrivilegeId == itm.ApptLocationId);

                itm.HousingUnitListId = privilege.HousingUnitListId;
                itm.HousingGroupId = privilege.HousingGroupId;
                itm.RefusalFlag = privilege.TrackingAllowRefusal == 1;
                itm.ApptAlertEndDate = privilege.AppointmentAlertApptEndDate == 1;
                itm.InmateCurrentTrackFacilityId = privilege.FacilityId;
                itm.HousingUnitLocation = privilege.HousingUnitLocation;
                itm.HousingUnitNumber = privilege.HousingUnitNumber;
                itm.EnrouteFlag = privilege.TrackEnrouteFlag == 1;
                itm.Privileges = lstPrivileges.Where(w => w.PrivilegeId == itm.ApptLocationId)
                    .Select(p => new PrivilegeDetailsVm
                    {
                        PrivilegeId = p.PrivilegeId,
                        TrackingAllowRefusal = p.TrackingAllowRefusal,
                        FacilityId = p.FacilityId,
                        TrackEnrouteFlag = p.TrackEnrouteFlag,
                        PrivilegeDescription = p.PrivilegeDescription
                    }).SingleOrDefault();
            });

            int[] courtLocationIds = lstPrivileges
                .Where(p => p.AppointmentRequireCourtLink && locationIds.Contains(p.PrivilegeId))
                .Select(p => p.PrivilegeId).ToArray();

            model.CntLocApptTracking = lstLocationApptTracking
                .Where(a => !courtLocationIds.Contains(a.ApptLocationId ?? 0)).ToList();
            model.CntCourtApptTracking = lstLocationApptTracking
                .Where(a => courtLocationIds.Contains(a.ApptLocationId ?? 0)).ToList();

            model.LocApptTracking = GetApptHousingList(model.CntLocApptTracking, lstApptDetails);

            model.CourtApptTracking = GetApptHousingList(model.CntCourtApptTracking, lstApptDetails);

            model.LocApptTracking.ForEach(item => {
                if (item.EnrouteLocationId > 0)
                {
                    item.Destination = lstPrivileges.Single(w => w.PrivilegeId == item.EnrouteLocationId)
                        .PrivilegeDescription;
                }
            });

            model.CourtApptTracking.ForEach(item =>
            {
                if (item.EnrouteLocationId > 0)
                {
                    item.Destination = lstPrivileges.Single(w => w.PrivilegeId == item.EnrouteLocationId)
                        .PrivilegeDescription;
                }
            });

            model.LstPrivileges = lstPrivileges.Where(w =>
                w.FacilityId == inputs.FacilityId && w.RemoveFromTrackingFlag == 0
                && w.InactiveFlag == 0).Select(s => new PrivilegeDetailsVm
            {
                PrivilegeId = s.PrivilegeId,
                PrivilegeDescription = s.PrivilegeDescription,
                FacilityId = s.FacilityId,
                TrackingAllowRefusal = s.TrackingAllowRefusal,
                TrackEnrouteFlag = s.TrackEnrouteFlag,
                ApptAlertEndDate = s.AppointmentAlertApptEndDate == 1
            }).ToList();

            model.LstPrivileges.AddRange(lstPrivileges.Where(w => (w.FacilityId == 0 || !w.FacilityId.HasValue) &&
                    w.RemoveFromTrackingFlag == 0 && w.InactiveFlag == 0)
                .Select(s => new PrivilegeDetailsVm
                {
                    PrivilegeId = s.PrivilegeId,
                    PrivilegeDescription = s.PrivilegeDescription,
                    FacilityId = s.FacilityId,
                    TrackingAllowRefusal = s.TrackingAllowRefusal,
                    TrackEnrouteFlag = s.TrackEnrouteFlag,
                    ApptAlertEndDate = s.AppointmentAlertApptEndDate == 1
                }).ToList());

            model.LstPrivileges = model.LstPrivileges.OrderBy(o => o.PrivilegeDescription).ToList();

            model.RefusalReasonList = _commonService.GetLookupKeyValuePairs(LookupConstants.TRACKREFUSALREAS);

            model.AllLocations = lstPrivileges.Where(w =>
                (w.FacilityId == inputs.FacilityId || w.FacilityId == 0 || !w.FacilityId.HasValue)
                && w.RemoveFromTrackingFlag == 0 && w.InactiveFlag == 0).Select(p =>
                new KeyValuePair<int, string>(p.PrivilegeId, p.PrivilegeDescription)).OrderBy(i => i.Value).ToList();

            return model;
        }

        private InmateTrakVm GetInmateTrack(int appId) => _context.InmateAppointmentTrack
                .Where(w => w.ScheduleId == appId && w.OccurenceDate.Date == DateTime.Now.Date)
                .Select(s =>
                    new InmateTrakVm
                    {
                        InmateTrackId = s.InmateTrakId,
                        InmateCurrentTrack = s.InmateTrakLocation,
                        InmateCurrentTrackId = s.InmateTrakLocationId,
                        InmateTrackDateIn = s.InmateTrakDateIn,
                        DateOut = s.InmateTrakDateOut,
                        DateIn = s.InmateTrakDateIn,
                        InmateRefused = s.InmateRefused,
                        InmateTrackNote = s.InmateTrakNote
                    }).OrderByDescending(w => w.InmateTrackId).FirstOrDefault();


        // Appointment check with inmateid, from date and to date
        public List<AoAppointmentVm> CheckAppointment(CalendarInputs inputs)
        {
            List<AoAppointmentVm> inmateApptList = new List<AoAppointmentVm>();

            List<AoAppointmentVm> lstScheduleInmate = _context.ScheduleInmate.Where(a =>
                    !a.DeleteFlag && a.LocationId.HasValue && a.InmateId.HasValue &&
                    (!inputs.InmateId.HasValue || a.InmateId == inputs.InmateId) &&
                    (inputs.FacilityId <= 0 || a.Inmate.FacilityId == inputs.FacilityId) &&
                    (!inputs.LocationId.HasValue || a.LocationId == inputs.LocationId) &&
                    ((a.IsSingleOccurrence || a is Visit) && a.EndDate.HasValue &&
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
                        LocationId = sch.LocationId ?? 0,
                        ScheduleId = sch.ScheduleId,
                        InmateId = sch.InmateId ?? 0,
                        IsSingleOccurrence = sch.IsSingleOccurrence,
                        DayInterval = sch.DayInterval,
                        WeekInterval = sch.WeekInterval,
                        FrequencyType = sch.FrequencyType,
                        QuarterInterval = sch.QuarterInterval,
                        MonthOfQuarterInterval = sch.MonthOfQuarterInterval,
                        MonthOfYear = sch.MonthOfYear,
                        DayOfMonth = sch.DayOfMonth,
                        Discriminator = sch is ScheduleCourt ? "COURT" : sch is Visit ? "VISIT" : "APPT"
                    }).ToList();
            
            int[] scheduleIds = lstScheduleInmate.Select(ii => ii.ScheduleId).ToArray();
            int[] locationIds = lstScheduleInmate.Select(ii => ii.LocationId).ToArray();

            List<Privileges> lstPrivileges = _context.Privileges
                .Where(l => locationIds.Contains(l.PrivilegeId)).ToList();

            foreach (AoAppointmentVm sch in lstScheduleInmate)
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

                if (materializedSchedule.IsSingleOccurrence || sch.Discriminator == "VISIT")
                {
                    AoAppointmentVm app = sch;
                    app.StartDate = materializedSchedule.StartDate;
                    app.EndDate = materializedSchedule.EndDate;
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
                            ScheduleId = sch.ScheduleId,
                            InmateId = sch.InmateId,
                            IsSingleOccurrence = sch.IsSingleOccurrence
                        }));
                }
            }

            List<int> inmIds = inmateApptList.Select(ii => ii.InmateId.Value).Distinct().ToList();
            List<InmateDetailsList> inmateList = GetInmateDetails(inmIds);

            inmateApptList = inmateApptList.Where(w =>
            {
                Debug.Assert(w.EndDate != null, "AppointmentService: materializedSchedule.EndDate != null");
                if (w.IsSingleOccurrence && w.StartDate.Date != w.EndDate.Value.Date)
                {
                    w.EndDate = w.StartDate.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
                }
                return true;
            }).ToList();

            inmateApptList = inmateApptList.Where(w =>
            {
                Debug.Assert(w.EndDate != null, "AppointmentService: w.EndDate != null");
                w.Location = w.LocationId != 0
                        ? lstPrivileges.Single(pri => pri.PrivilegeId == w.LocationId).PrivilegeDescription : "";
                w.InmateDetails = inmateList.Single(ii => ii.InmateId == w.InmateId);
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
    }
}
