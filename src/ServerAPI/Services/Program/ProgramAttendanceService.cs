using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GenerateTables.Models;
using Microsoft.AspNetCore.Http;
using ServerAPI.Interfaces;
using ServerAPI.ViewModels;

namespace ServerAPI.Services {
    public class ProgramAttendanceService : IProgramAttendanceService {

        private readonly AAtims _context;
        private readonly int _personnelId;

        public ProgramAttendanceService (AAtims context,
            IHttpContextAccessor httpContextAccessor) {
            _context = context;
            _personnelId = Convert.ToInt32 (httpContextAccessor.HttpContext.User
                .FindFirst ("personnelId")?.Value);
        }

        public List<ProgramAttendanceDetails> GetProgramAttendanceRequest(int scheduleId)
        {
            List<ProgramAttendInmate> programAttendInmates = _context.ProgramAttendInmate.Where(w =>
                    w.ProgramAttend.ScheduleId == scheduleId && w.ProgramAttendId == w.ProgramAttend.ProgramAttendId)
                .ToList();

            List<ProgramAttendanceDetails> programAttendance = _context.ProgramClassAssign
                .Where(w => w.ScheduleId == scheduleId)
                .Select(
                    s => new ProgramAttendanceDetails
                    {
                        RequestId = s.ProgramRequestId,
                        InmateId = s.InmateId,
                        InmateInfo = new PersonInfoVm
                        {
                            PersonLastName = s.InmateIdNavigation.Person.PersonLastName,
                            PersonFirstName = s.InmateIdNavigation.Person.PersonFirstName,
                            PersonMiddleName = s.InmateIdNavigation.Person.PersonMiddleName,
                            InmateNumber = s.InmateIdNavigation.InmateNumber
                        },
                        ScheduleId = s.ScheduleId,
                        StartDate = s.ProgramClassIdNavigation.StartDate,
                        EndDate = s.ProgramClassIdNavigation.EndDate,
                        ProgramClassAssignId = s.ProgramClassAssignId,
                        ProgramAttendInmateList = programAttendInmates.Where(j => j.InmateId == s.InmateId).Select(k =>
                            new ProgramAttendanceCourseVm
                            {
                                ProgramAttendInmateId = k.ProgramAttendInmateId,
                                ProgramAttendId = k.ProgramAttendId,
                                AttendFlag = k.AttendFlag == 1,
                                NotAttendFlag = k.NotAttendFlag == 1,
                                ScheduleId = k.ProgramAttend.ScheduleId,
                                OccuranceDate = k.ProgramAttend.OccuranceDate,
                                InmateId = k.InmateId
                            }).ToList(),
                        ProgramAttendList = programAttendInmates.Where(j => j.InmateId == s.InmateId).Select(k =>
                            new ProgramAttendanceCourseVm
                            {
                                ScheduleId = k.ProgramAttend.ScheduleId,
                                OccuranceDate = k.ProgramAttend.OccuranceDate,
                                ProgramAttendId = k.ProgramAttendId,
                            }).ToList()
                    }).ToList();

            return programAttendance;
        }

        public async Task<int> InsertProgramAttendanceRequest (List<ProgramAttendanceCourseVm> programAttendValue) {
            if (programAttendValue.Any())
            {
                programAttendValue.ForEach(program =>
                {
                    program.ProgramAttendList.ForEach(item =>
                    {
                        ProgramAttend programAttend = _context.ProgramAttend.Find(item.ProgramAttendId);
                        if (programAttend is null)
                        {
                            InsertProgramAttendance(item);
                        }
                        else
                        {
                            ProgramAttendInmate programAttendInmates =
                                _context.ProgramAttendInmate.Find(item.ProgramAttendInmateId);
                            if (programAttendInmates is null)
                            {
                                ProgramAttendInmate programAttendInmate = new ProgramAttendInmate
                                {
                                    AttendFlag = item.AttendFlag ? 1 : 0,
                                    CreateBy = _personnelId,
                                    CreateDate = DateTime.Now,
                                    InmateId = item.InmateId ?? default,
                                    NotAttendFlag = item.NotAttendFlag ? 1 : 0,
                                    ProgramAttendId = item.ProgramAttendId,
                                    ProgramHours = item.Duration
                                };
                                _context.ProgramAttendInmate.AddRange(programAttendInmate);
                            }
                            else
                            {
                                programAttendInmates.AttendFlag = item.AttendFlag ? 1 : 0;
                                programAttendInmates.NotAttendFlag = item.NotAttendFlag ? 1 : 0;
                                programAttendInmates.UpdateBy = _personnelId;
                                programAttendInmates.UpdateDate = DateTime.Now;
                            }
                        }
                    });
                });
            }

            return await _context.SaveChangesAsync();
        }

        private int InsertProgramAttendance(ProgramAttendanceCourseVm programAttendValue)
        {
            ProgramAttend programAttend = new ProgramAttend
            {
                ScheduleId = programAttendValue.ScheduleId,
                CreateBy = _personnelId,
                CreateDate = DateTime.Now,
                OccuranceDate = programAttendValue.OccuranceDate
            };
            _context.ProgramAttend.Add(programAttend);
            _context.SaveChanges();

            programAttendValue.ProgramAttendInmateList.ForEach(item =>
            {
                ProgramAttendInmate programAttendInmate = new ProgramAttendInmate
                {
                    AttendFlag = programAttendValue.AttendFlag ? 1 : 0,
                    CreateBy = _personnelId,
                    CreateDate = DateTime.Now,
                    InmateId = programAttendValue.InmateId ?? default,
                    NotAttendFlag = programAttendValue.NotAttendFlag ? 1 : 0,
                    ProgramAttendId = programAttend.ProgramAttendId,
                    ProgramHours = programAttendValue.Duration,
                };
                _context.ProgramAttendInmate.AddRange(programAttendInmate);
            });

            return _context.SaveChanges();
        }
    }
}