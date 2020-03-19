﻿using GenerateTables.Models;
using Microsoft.AspNetCore.Http;
using ServerAPI.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServerAPI.Services
{
    public class SentenceAttendeeService : ISentenceAttendeeService
    {
        private readonly AAtims _context;
        private readonly int _personnelId;

        public SentenceAttendeeService(AAtims context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _personnelId = Convert.ToInt32(httpContextAccessor.HttpContext.User
                .FindFirst("personnelId")?.Value);
        }

        public List<AttendanceVm> GetAttendanceList(DateTime dateValue)
        {
            List<AttendanceVm> attendanceList = new List<AttendanceVm>();

            ArrestSentenceAttendanceDay arrestSentenceAttendanceDay =
                _context.ArrestSentenceAttendanceDay
                    .SingleOrDefault(a => a.AttendanceDate.Value.Date == dateValue.Date);

            if (arrestSentenceAttendanceDay != null)
            {
                attendanceList = _context.ArrestSentenceAttendance
                    .Where(a => a.ArrestSentenceAttendanceDayId ==
                                arrestSentenceAttendanceDay.ArrestSentenceAttendanceDayId)
                    .OrderBy(o => o.DeleteFlag).ThenBy(o => o.ArrestId).ThenBy(o => o.NoDayDayFlag).Select(a =>
                        new AttendanceVm
                        {
                            ArrestSentenceAttendanceId = a.ArrestSentenceAttendanceId,
                            ArrestId = a.ArrestId,
                            ProgramFlag = a.ProgramFlag == 1,
                            WorkCrewFlag = a.WorkCrewFlag == 1,
                            AttendanceFlag = a.AttendFlag == 1,
                            NoDayForDayFlag = a.NoDayDayFlag == 1,
                            ReCalcComplete = a.ReCalcComplete == 1,
                            AttendanceCredit = a.AttendCredit??0,
                            Note = a.AttendNote,
                            InmateId = a.InmateId,
                            PersonId=a.Inmate.PersonId,
                            AttendanceDay = _context.ArrestSentenceAttendanceDay
                                .Where(x => x.ArrestSentenceAttendanceDayId == a.ArrestSentenceAttendanceDayId)
                                .Select(d => new AttendanceDayVm
                                {
                                    ArrestSentenceAttendanceDayId = d.ArrestSentenceAttendanceDayId,
                                    AttendanceDate = d.AttendanceDate
                                }).SingleOrDefault(),
                            AgencyAbbreviation=a.Arrest.ArrestCourtJurisdiction.AgencyAbbreviation,
                            CaseNumber = a.Arrest.ArrestCaseNumber,
                            DeleteFlag = a.DeleteFlag == 1
                        }).ToList();

                int[] inmateIds = attendanceList.Select(s => s.InmateId).ToArray();

                List<Incarceration> lstIncarcerations = _context.Incarceration
                    .Where(x => inmateIds.Contains(x.InmateId ?? 0)).ToList();

                attendanceList.ForEach(item =>
                {
                    item.IncarcerationId = lstIncarcerations
                        .SingleOrDefault(s => s.InmateId == item.InmateId && !s.ReleaseOut.HasValue)?.IncarcerationId;
                    item.BookingNumber = lstIncarcerations
                        .SingleOrDefault(s => s.InmateId == item.InmateId && !s.ReleaseOut.HasValue)?.BookingNo;
                    item.InmateDetails = LoadInmateDetails(item.InmateId);
                    item.NoDayForDaySentence =
                        GetNoDayForDaySentence(item.IncarcerationId ?? 0, item.InmateId);
                });
            }

            return attendanceList.ToList();
        }

        public AttendanceVm GetAttendanceValue(int arrestSentenceAttendanceId)
        {
            AttendanceVm attendanceList =
                _context.ArrestSentenceAttendance.Where(a => a.ArrestSentenceAttendanceId == arrestSentenceAttendanceId)
                    .OrderBy(o => o.DeleteFlag).ThenBy(o => o.ArrestId).ThenBy(o => o.NoDayDayFlag).Select(a =>
                        new AttendanceVm
                        {
                            ArrestSentenceAttendanceId = a.ArrestSentenceAttendanceId,
                            ProgramFlag = a.ProgramFlag == 1,
                            WorkCrewFlag = a.WorkCrewFlag == 1,
                            AttendanceFlag = a.AttendFlag == 1,
                            NoDayForDayFlag = a.NoDayDayFlag == 1,
                            AttendanceCredit = a.AttendCredit??0,
                            Note = a.AttendNote,
                            InmateId = a.InmateId,
                            AttendanceDay = _context.ArrestSentenceAttendanceDay
                                .Where(x => x.ArrestSentenceAttendanceDayId == a.ArrestSentenceAttendanceDayId)
                                .Select(d => new AttendanceDayVm
                                {
                                    ArrestSentenceAttendanceDayId = d.ArrestSentenceAttendanceDayId,
                                    AttendanceDate = d.AttendanceDate
                                }).SingleOrDefault()
                        }).Single();

            attendanceList.InmateDetails = LoadInmateDetails(attendanceList.InmateId);
            return attendanceList;
        }

        public List<RecentAttendanceVm> GetRecentAttendanceValue(DateTime dateTime)
        {
            DateTime dateTimeFrom = dateTime.AddDays(-15);
            List<AttendanceVm> attendanceList =
                _context.ArrestSentenceAttendance.Where(a =>
                        a.ArrestSentenceAttendanceDay.AttendanceDate.Value.Date >= dateTimeFrom.Date
                        && a.ArrestSentenceAttendanceDay.AttendanceDate.Value.Date <= dateTime.Date)
                    .Select(a => new AttendanceVm
                    {
                        ArrestSentenceAttendanceId = a.ArrestSentenceAttendanceId,
                        ArrestId = a.ArrestId ?? 0,
                        DeleteFlag = a.DeleteFlag == 1,
                        AttendanceFlag = a.AttendFlag == 1,
                        NoDayForDayFlag = a.NoDayDayFlag == 1,
                        ReCalcComplete = a.ReCalcComplete == 1,
                        AttendanceDay = new AttendanceDayVm
                        {
                            ArrestSentenceAttendanceDayId = a.ArrestSentenceAttendanceDay.ArrestSentenceAttendanceDayId,
                            AttendanceDate = a.ArrestSentenceAttendanceDay.AttendanceDate
                        },
                        IncarcerationId = _context.Incarceration
                            .SingleOrDefault(w => w.InmateId == a.InmateId && !w.ReleaseOut.HasValue)
                            .IncarcerationId,
                        InmateId =a.InmateId
                    }).ToList();

            attendanceList.ForEach(item =>
            {
                item.NoDayForDaySentence =
                    GetNoDayForDaySentence(item.IncarcerationId ?? 0, item.InmateId);
            });

            List<RecentAttendanceVm> recentAttendanceList = attendanceList
                .GroupBy(g => new { g.AttendanceDay.AttendanceDate })
                .Select(s => new RecentAttendanceVm
                {
                    AttendanceDate = s.Key.AttendanceDate,
                    Pending = s.Count(c => !c.DeleteFlag && c.ArrestId==0 && !c.NoDayForDayFlag && c.NoDayForDaySentence),
                    SentenceAdjacent = s.Count(c => !c.DeleteFlag && c.ArrestId>0 && !c.NoDayForDayFlag && !c.ReCalcComplete),
                    NoDayForDayFlag = s.Count(c => !c.DeleteFlag && c.ArrestId==0 && (c.NoDayForDayFlag || !c.NoDayForDaySentence)),
                    Recalculate = s.Count(c => !c.DeleteFlag && c.ArrestId>0 && !c.NoDayForDayFlag && c.ReCalcComplete)
                }).ToList();

            return recentAttendanceList;
        }

        public int GetDuplicateAttendance(DateTime dateValue, int inmateId) =>
            _context.ArrestSentenceAttendance.Count(a => a.InmateId == inmateId &&
                                                         a.ArrestSentenceAttendanceDay.AttendanceDate.Value.Date ==
                                                         dateValue.Date);

        public async Task<int> InsertAttendance(AttendanceVm value)
        {
            if (value.ArrestSentenceAttendanceId > 0)
            {
                ArrestSentenceAttendance arrestSentenceAttendance =
                    _context.ArrestSentenceAttendance.Single(s =>
                        s.ArrestSentenceAttendanceId == value.ArrestSentenceAttendanceId);
                {
                    if (value.DeleteFlag)
                    {
                        arrestSentenceAttendance.DeleteFlag = 1;
                        arrestSentenceAttendance.DeleteBy = _personnelId;
                        arrestSentenceAttendance.DeleteDate = DateTime.Now;
                    }
                    else
                    {
                        arrestSentenceAttendance.CreateBy = _personnelId;
                        arrestSentenceAttendance.CreateDate = DateTime.Now;
                        arrestSentenceAttendance.ProgramFlag = value.ProgramFlag ? 1 : 0;
                        arrestSentenceAttendance.WorkCrewFlag = value.WorkCrewFlag ? 1 : 0;
                        arrestSentenceAttendance.AttendFlag = value.AttendanceFlag ? 1 : 0;
                        arrestSentenceAttendance.AttendCredit = value.AttendanceCredit;
                        arrestSentenceAttendance.NoDayDayFlag = value.NoDayForDayFlag ? 1 : 0;
                        arrestSentenceAttendance.AttendNote = value.Note;
                        arrestSentenceAttendance.UpdateBy = _personnelId;
                        arrestSentenceAttendance.UpdateDate = DateTime.Now;
                        arrestSentenceAttendance.DeleteFlag = 0;
                        arrestSentenceAttendance.DeleteBy = null;
                        arrestSentenceAttendance.DeleteDate = null;
                    }
                }
            }
            else
            {
                ArrestSentenceAttendanceDay arrestSentenceAttendanceDay =
                    _context.ArrestSentenceAttendanceDay
                        .SingleOrDefault(a =>
                            a.AttendanceDate.Value.Date == value.AttendanceDay.AttendanceDate.Value.Date);

                if (arrestSentenceAttendanceDay == null)
                {
                    arrestSentenceAttendanceDay = new ArrestSentenceAttendanceDay();
                    {
                        arrestSentenceAttendanceDay.AttendanceDate = value.AttendanceDay.AttendanceDate;
                        arrestSentenceAttendanceDay.CreateBy = _personnelId;
                        arrestSentenceAttendanceDay.CreateDate = DateTime.Now;
                    }
                    _context.Add(arrestSentenceAttendanceDay);
                    _context.SaveChanges();
                }

                ArrestSentenceAttendance arrestSentenceAttendance = new ArrestSentenceAttendance();
                {
                    arrestSentenceAttendance.ArrestSentenceAttendanceDayId =
                        arrestSentenceAttendanceDay.ArrestSentenceAttendanceDayId;
                    arrestSentenceAttendance.InmateId = value.InmateId;
                    arrestSentenceAttendance.CreateBy = _personnelId;
                    arrestSentenceAttendance.CreateDate = DateTime.Now;
                    arrestSentenceAttendance.ProgramFlag = value.ProgramFlag ? 1 : 0;
                    arrestSentenceAttendance.AttendCredit = value.AttendanceCredit;
                    arrestSentenceAttendance.WorkCrewFlag = value.WorkCrewFlag ? 1 : 0;
                    arrestSentenceAttendance.AttendFlag = value.AttendanceFlag ? 1 : 0;
                    arrestSentenceAttendance.NoDayDayFlag = value.NoDayForDayFlag ? 1 : 0;
                    arrestSentenceAttendance.AttendNote = value.Note;
                }

                _context.Add(arrestSentenceAttendance);
            }

            return await _context.SaveChangesAsync();
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
                    PersonDob = i.Person.PersonDob.HasValue ? i.Person.PersonDob : null,
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

        private bool GetNoDayForDaySentence(int incarcerationId, int inmateId)
        {
            List<NoDayForDayFlagVm> lstArrestSentence = _context.IncarcerationArrestXref
                .Where(w => w.IncarcerationId == incarcerationId)
                .Select(a => new NoDayForDayFlagVm
                {
                    InmateId = a.Incarceration.Inmate.InmateId,
                    DayForDayAllowed = a.Arrest.ArrestSentenceDayForDayAllowed > 0,
                    ReleaseDate = a.ReleaseDate,
                    NoDayForDay = a.Arrest.ArrestSentenceNoDayForDay > 0,
                    DayForDayFixed = a.Arrest.ArrestSentenceMethod.ArrestSentenceDdFixed ?? 0,
                    DayForDayFactor = a.Arrest.ArrestSentenceMethod.ArrestSentenceDdFactor ?? 0,
                    DayForDaySql = a.Arrest.ArrestSentenceMethod.ArrestSentenceDdSql
                }).ToList();

            bool isDayForDay =
                lstArrestSentence.Count(w => w.InmateId == inmateId && w.DayForDayAllowed
                                                                    && !w.ReleaseDate.HasValue && !w.NoDayForDay
                                                                    && (w.DayForDayFixed > 0 || w.DayForDayFactor > 0 || (!string.IsNullOrEmpty(w.DayForDaySql) &&
                                                                                                                          w.DayForDaySql.Length > 0))) > 0;

            return isDayForDay;
        }

        public WorkAttendanceVm GetRunAttendanceList(DateTime dateValue)
        {
            WorkAttendanceVm attendance = new WorkAttendanceVm();

            List<AttendanceVm> workAttendance = _context.WorkCrewTrackXref
                .Where(w => w.WorkCrew.WorkCrewLookup.ApplyWorkSentenceCredit == 1
                            && w.InmateTrak.InmateTrakDateOut.Value.Date == dateValue.Date
                            && w.WorkCrew.WorkCrewLookup.ApplyWorkSentenceCheckoutDuration
                            <= (w.InmateTrak.InmateTrakDateOut - (w.InmateTrak.InmateTrakDateIn ?? DateTime.Now)).Value.Minutes)
                .GroupBy(g => new { g.InmateTrak.InmateId })
                .Select(s => new AttendanceVm
                {
                    InmateId = s.Key.InmateId,
                    ProgramFlag = false,
                    WorkCrewFlag = true,
                    AttendCredit = (int)s.Max(m => m.WorkCrew.WorkCrewLookup.ApplyWorkSentenceCreditFactor??0)
                }).ToList();

            List<AttendanceSchedule> schedules = _context.ScheduleProgram.Where(w => w.ProgramClassId > 0)
                .Select(s => new AttendanceSchedule()
                {
                    ScheduleId = s.ScheduleId,
                  //  ApplyAttendSentenceCredit = s.Program.ApplyAttendSentenceCredit ?? 0,
                   // ApplyAttendSentenceCreditFactor = (int)s.Program.ApplyAttendSentenceCreditFactor

                }).ToList();

            workAttendance.AddRange(_context.ProgramAttendInmate
                .Where(w => w.AttendFlag == 1
              && w.ProgramAttend.OccuranceDate.Value.Date == dateValue.Date
              && schedules.Where(we => we.ApplyAttendSentenceCredit == 1).Select(s => s.ScheduleId).Contains(w.ProgramAttend.ScheduleId))
                .GroupBy(g => new { g.InmateId })
                .Select(s => new AttendanceVm
                {
                    InmateId = s.Key.InmateId,
                    ProgramFlag = true,
                    WorkCrewFlag = false,
                    AttendCredit = s.Max(m => schedules.FirstOrDefault(w => w.ScheduleId == m.ProgramAttend.ScheduleId).ApplyAttendSentenceCreditFactor ?? 0)
                }));

            workAttendance = workAttendance.GroupBy(g => g.InmateId).Select(s => new AttendanceVm
            {
                InmateId = s.Key,
                ProgramFlag = s.Max(m => m.ProgramFlag),
                WorkCrewFlag = s.Max(m => m.WorkCrewFlag),
                AttendCredit = s.Sum(su => su.AttendCredit) > 1 ? 1 : s.Sum(su => su.AttendCredit)
            }).ToList();

            List<InmateDetailsList> inmateDetails = _context.Inmate
                .Where(w => workAttendance.Select(se => se.InmateId).Contains(w.InmateId))
                .Select(s => new InmateDetailsList
                {
                    InmateId = s.InmateId,
                    HousingLocation = s.HousingUnit.HousingUnitLocation,
                    HousingNumber = s.HousingUnit.HousingUnitNumber,
                    HousingUnitBedLocation = s.HousingUnit.HousingUnitBedLocation,
                    HousingUnitBedNumber = s.HousingUnit.HousingUnitBedNumber,
                    PersonFirstName = s.Person.PersonFirstName,
                    PersonLastName = s.Person.PersonLastName,
                    PersonMiddleName = s.Person.PersonMiddleName,
                    InmateNumber = s.InmateNumber,
                    FacilityAbbr = s.Facility.FacilityAbbr
                }).ToList();


            workAttendance.ForEach(f =>
            {
                f.InmateDetails = inmateDetails.FirstOrDefault(s => s.InmateId == f.InmateId);
                f.AttendanceFlag = true;
                f.NoDayForDaySentence = !GetNoDayForDaySentence(f.InmateId);
            });

            if (workAttendance.Count > 0)
            {
                attendance.ArrestSentenceAttendanceDayId = _context.ArrestSentenceAttendanceDay
                    .SingleOrDefault(s => s.AttendanceDate == dateValue.Date)?.ArrestSentenceAttendanceDayId ?? 0;

            }

            attendance.Attendance = workAttendance.ToList();
            return attendance;
        }
        private bool GetNoDayForDaySentence(int inmateId)
        {
            bool isDayForDay = _context.IncarcerationArrestXref
                .Count(w => w.Incarceration.Inmate.InmateId == inmateId && !w.Incarceration.ReleaseOut.HasValue
                    && !w.ReleaseDate.HasValue && (w.Arrest.ArrestSentenceNoDayForDay == 0 || !w.Arrest.ArrestSentenceNoDayForDay.HasValue)
                    && (w.Arrest.ArrestSentenceMethod.ArrestSentenceDdFixed > 0 || !w.Arrest.ArrestSentenceMethod.ArrestSentenceDdFactor.HasValue || (!string.IsNullOrEmpty(w.Arrest.ArrestSentenceMethod.ArrestSentenceDdSql) &&
                     w.Arrest.ArrestSentenceMethod.ArrestSentenceDdSql.Length > 0))) > 0;
            return isDayForDay;
        }

        public async Task<int> InsertArrestSentenceAttendanceDay(WorkAttendanceVm value)
        {
            if (value.ArrestSentenceAttendanceDayId == 0)
            {
                if (value.AttendanceDate != null)
                {
                    ArrestSentenceAttendanceDay attendanceDay = new ArrestSentenceAttendanceDay
                    {
                        AttendanceDate = value.AttendanceDate.Value.Date,
                        CreateBy = _personnelId,
                        CreateDate = DateTime.Now,
                    };
                    _context.ArrestSentenceAttendanceDay.Add(attendanceDay);
                    _context.SaveChanges();

                    if (attendanceDay.ArrestSentenceAttendanceDayId > 0)
                    {
                        value.Attendance.ForEach(item =>
                        {
                            ArrestSentenceAttendance arrestSentence = new ArrestSentenceAttendance
                            {
                                ArrestSentenceAttendanceDayId = attendanceDay.ArrestSentenceAttendanceDayId,
                                InmateId = item.InmateId,
                                ProgramFlag = item.ProgramFlag ? 1 : 0,
                                WorkCrewFlag = item.WorkCrewFlag ? 1 : 0,
                                AttendFlag = item.AttendFlag ? 1 : 0,
                                AttendCredit = item.AttendCredit,
                                NoDayDayFlag = item.NoDayForDayFlag ? 1 : 0,
                                CreateBy = _personnelId,
                                CreateDate = DateTime.Now
                            };
                            _context.ArrestSentenceAttendance.Add(arrestSentence);
                            _context.SaveChanges();

                        });
                    }
                }
            }
            else
            {
                value.Attendance.ForEach(item =>
                {
                    if (!(_context.ArrestSentenceAttendance.Any(a => a.InmateId == item.InmateId &&
                   a.ArrestSentenceAttendanceDayId == item.ArrestSentenceAttendanceId)))
                    {
                        ArrestSentenceAttendance arrestSentence = new ArrestSentenceAttendance
                        {
                            ArrestSentenceAttendanceDayId = value.ArrestSentenceAttendanceDayId,
                            InmateId = item.InmateId,
                            ProgramFlag = item.ProgramFlag ? 1 : 0,
                            WorkCrewFlag = item.WorkCrewFlag ? 1 : 0,
                            AttendFlag = item.AttendFlag ? 1 : 0,
                            AttendCredit = item.AttendCredit,
                            NoDayDayFlag = item.NoDayForDayFlag ? 1 : 0,
                            CreateBy = _personnelId,
                            CreateDate = DateTime.Now
                        };
                        _context.ArrestSentenceAttendance.Add(arrestSentence);
                        _context.SaveChanges();
                    }

                });
            }
            return await _context.SaveChangesAsync();
        }
    }
}