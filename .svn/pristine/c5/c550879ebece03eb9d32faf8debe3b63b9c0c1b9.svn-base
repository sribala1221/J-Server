using GenerateTables.Models;
using Microsoft.AspNetCore.Http;
using ServerAPI.Interfaces;
using ServerAPI.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;

// ReSharper disable once CheckNamespace
namespace ServerAPI.Services
{
    public class ProgramInstructorService : IProgramInstructorService
    {
        private readonly AAtims _context;
        private readonly int _personnelId;

        public ProgramInstructorService(AAtims context,
            IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _personnelId = Convert.ToInt32(httpContextAccessor.HttpContext.User
                .FindFirst("personnelId")?.Value);
        }

        public ProgramInstructionCerificationVm GetProgramInstructor()
        {

            // List<ScheduleVm> assignedCourses = _context.ProgramInstructorAssign.Where(a =>
            //     !(a.DeleteFlag.HasValue) || a.DeleteFlag == 0).Select(s => new ScheduleVm
            //     {
            //         ScheduleId = s.ScheduleId,
            //         PersonnelId = s.PersonnelId,
            //         DeleteFlag = s.DeleteFlag == 1
            //     }).ToList();

            List<ScheduleVm> scheduleList = _context.ScheduleProgram.Where(
                a => !a.ProgramClass.DeleteFlag && !a.ProgramClass.InactiveFlag && a.ProgramClassId > 0
            ).Select(s => new ScheduleVm
            {
                ScheduleId = s.ScheduleId,
                ProgramClassId = s.ProgramClassId,
                ProgramCourseId = s.ProgramClass.ProgramCourseId,
                ProgramCourseNumber = s.ProgramClass.ProgramCourseIdNavigation.CourseNumber,
                ProgramCourseName = s.ProgramClass.ProgramCourseIdNavigation.CourseName,
                ProgramCategory = s.ProgramClass.ProgramCourseIdNavigation.ProgramCategory,
                Crn = s.ProgramClass.CRN,
                PersonnelId = s.ProgramClass.ProgramInstructorString
            }).ToList();

            ProgramInstructionCerificationVm program = new ProgramInstructionCerificationVm
            {
                CourseInstructor = _context.Personnel.Where(p => p.ProgramInstructorFlag && !p.PersonnelTerminationFlag)
                    .Select(pf => new ProgramCourseInstructorVm
                    {
                        PersonnelId = pf.PersonnelId,
                        Personnel = new PersonnelVm
                        {
                            PersonFirstName = pf.PersonNavigation.PersonFirstName,
                            PersonLastName = pf.PersonNavigation.PersonLastName,
                            OfficerBadgeNumber = pf.OfficerBadgeNumber,
                            PersonnelNumber = pf.OfficerBadgeNum
                        },
                        PersonId = pf.PersonId,
                        PersonDlNumber = pf.PersonNavigation.PersonDlNumber,
                        PersonOtherIdNumber = pf.PersonNavigation.PersonOtherIdNumber
                    }).ToList()
            };
            program.Instructor = _context.ProgramClass.Where(pc =>
                    program.CourseInstructor.Any(a => a.PersonnelId == pc.ProgramInstructorString))
                .Select(s => new ScheduleVm
                {
                    ProgramClassId = s.ProgramClassId,
                    ProgramCourseId = s.ProgramCourseId,
                    ProgramCourseNumber = s.ProgramCourseIdNavigation.CourseNumber,
                    ProgramCourseName = s.ProgramCourseIdNavigation.CourseName,
                    ProgramCategory = s.ProgramCourseIdNavigation.ProgramCategory,
                    Crn = s.CRN,
                    PersonnelId = s.ProgramInstructorString
                }).ToList();
            program.ProgamClassCertificateList = _context.ProgramInstructorCert.Where(a => program.Instructor
                                                                                               .Any(b =>
                                                                                                   b.ProgramClassId ==
                                                                                                   a.ProgramClassId) &&
                                                                                           (!(a.DeleteFlag.HasValue) ||
                                                                                            a.DeleteFlag == 0))
                .ToList();

            List<ScheduleVm> certificate = program.Instructor.Where(a => program.ProgamClassCertificateList.Any(b =>
                b.ProgramClassId == a.ProgramClassId)).ToList();

            if (program.CourseInstructor.Any())
            {
                program.CourseInstructor.ForEach(a =>
                {
                    a.Certified = certificate.Where(b => b.PersonnelId == a.PersonnelId).ToList();
                    a.AssignedCourses =scheduleList.Where(b => b.PersonnelId == a.PersonnelId)
                        .ToList();
                });

            }

            return program;
        }

        public int InsertInstructorCertificate(List<ProgramInstructorCertVm> certificateList)
        {
            certificateList.ForEach(c =>
            {
                if (c.ProgramInstructorCertId > 0)
                {
                    if (!c.IsChecked)
                    {
                        ProgramInstructorCert cert = _context.ProgramInstructorCert.Find(c.ProgramInstructorCertId);
                        cert.CertNote = c.CertNote;
                        cert.DeleteBy = _personnelId;
                        cert.ProgramClassId = c.ProgramClassId;
                        cert.DeleteDate = DateTime.Now;
                        cert.DeleteFlag = 1;
                        cert.PersonnelId = c.PersonnelId;
                        _context.SaveChanges();
                    }
                    else
                    {
                        ProgramInstructorCert cert = _context.ProgramInstructorCert.Find(c.ProgramInstructorCertId);
                        cert.CertNote = c.CertNote;
                        cert.ProgramClassId = c.ProgramClassId;
                        cert.PersonnelId = c.PersonnelId;
                        _context.SaveChanges();
                    }
                }
                else
                {
                    ProgramInstructorCert cert = new ProgramInstructorCert
                    {
                        CertNote = c.CertNote,
                        CreateBy = _personnelId,
                        ProgramClassId = c.ProgramClassId,
                        CreateDate = DateTime.Now,
                        DeleteFlag = 0,
                        PersonnelId = c.PersonnelId
                    };
                    _context.ProgramInstructorCert.Add(cert);
                    _context.SaveChanges();
                }
            });
            return _context.SaveChanges();
        }

      

      
    }
}