﻿using GenerateTables.Models;
using System;

// ReSharper disable once CheckNamespace
namespace ServerAPI.Tests
{
    public partial class DbInitialize
    {
        private void ProgramDetails()
        {
            Db.ProgramClass.AddRange(
                new ProgramClass
                {
                    ProgramClassId = 10,
                    DeleteFlag = false,
                    DeleteDate = null,
                    DeleteBy = null,
                    InactiveFlag = false,
                    ProgramInstructorString = 0,
                    CRN = "NEW CLASS",
                    DeleteReason = null,
                    InactiveBy = null,
                    InactiveDate = null,
                    ProgramCourseId = 10
                },
                new ProgramClass
                {
                    ProgramClassId = 11,
                    DeleteFlag = false,
                    DeleteDate = null,
                    DeleteBy = null,
                    InactiveFlag = false,
                    ProgramInstructorString = 0,
                    CRN = "GENERAL",
                    DeleteReason = null,
                    InactiveBy = null,
                    InactiveDate = null,
                    ProgramCourseId = 10
                },
                new ProgramClass
                {
                    ProgramClassId = 12,
                    DeleteFlag = false,
                    DeleteDate = null,
                    DeleteBy = null,
                    InactiveFlag = false,
                    ProgramInstructorString = 0,
                    CRN = null,
                    DeleteReason = null,
                    InactiveBy = null,
                    InactiveDate = null,
                    ProgramCourseId = 11
                }


                );
            Db.ProgramCourse.AddRange(
                new ProgramCourse
                {
                    ProgramCourseId = 10,
                    CreateDate = DateTime.Now.AddDays(-10),
                    DeleteFlag = false,
                    CreateBy = 11,
                    UpdateDate = DateTime.Now.AddDays(-10),
                    UpdateBy = 11,
                    DeleteDate = null,
                    DeleteBy = null,
                    ProgramCategory = "LIFE SKILS"
                },
                new ProgramCourse
                {
                    ProgramCourseId = 11,
                    CreateDate = DateTime.Now.AddDays(-10),
                    DeleteFlag = false,
                    CreateBy = 11,
                    UpdateDate = DateTime.Now.AddDays(-10),
                    UpdateBy = 12,
                    DeleteDate = null,
                    DeleteBy = null,
                    ProgramCategory = "EDUCATION"
                }
                );

            Db.Program.AddRange(
                new GenerateTables.Models.Program
                {
                    ProgramId = 5,
                    ProgramCategoryId = 10,
                    DeleteFlag = 0,
                    CreateDate = DateTime.Now.AddDays(-5),
                    UpdateDate = DateTime.Now,
                    CreateBy = 12,
                    UpdateBy = 12,
                    ClassOrServiceName = "MIN MALE",
                    CertificateName = "10 th FAIL"
                },
                new GenerateTables.Models.Program
                {
                    ProgramId = 6,
                    ProgramCategoryId = 11,
                    DeleteFlag = 0,
                    CreateDate = DateTime.Now.AddDays(-4),
                    UpdateDate = DateTime.Now,
                    CreateBy = 11,
                    UpdateBy = 12,
                    ClassOrServiceName = "MIN FEMALE",
                    CertificateName = "DIPLOMO",
                    ClassOrServiceNumber = 145000
                },
                new GenerateTables.Models.Program
                {
                    ProgramId = 7,
                    ProgramCategoryId = 10,
                    DeleteFlag = 0,
                    CreateDate = DateTime.Now.AddDays(-3),
                    UpdateDate = DateTime.Now.AddDays(-3),
                    CreateBy = 11,
                    UpdateBy = 12,
                    ClassOrServiceName = "MAX DADS",
                    CertificateName = "DEGREE",
                    ClassOrServiceGenderFilter = "MAX PENDING",
                    ClassOrServiceNumber = 145005
                },
                new GenerateTables.Models.Program
                {
                    ProgramId = 8,
                    ProgramCategoryId = 12,
                    DeleteFlag = 0,
                    CreateDate = DateTime.Now.AddDays(-3),
                    UpdateDate = DateTime.Now,
                    CreateBy = 11,
                    UpdateBy = 12,
                    ClassOrServiceName = "COMPUTER-MIN",
                    CertificateName = null,
                    ClassOrServiceGenderFilter = "MINI MALE",
                    ClassOrServiceDescription = "EXCELLENT MALE",
                    ClassOrServiceNumber = 145010
                },
                new GenerateTables.Models.Program
                {
                    ProgramId = 9,
                    ProgramCategoryId = 12,
                    DeleteFlag = 0,
                    CreateDate = DateTime.Now.AddDays(-3),
                    UpdateDate = DateTime.Now,
                    CreateBy = 12,
                    UpdateBy = 11,
                    ClassOrServiceName = "AD SEG/PROTECTIVE CUSTODY",
                    CertificateName = null,
                    ClassOrServiceGenderFilter = "FEMALE",
                    ClassOrServiceDescription = null,
                    ClassOrServiceNumber = 145012,
                    ApplyAttendSentenceCreditFactor = 1,
                    ApplyAttendSentenceCredit = 1
                });


            Db.ProgramRequest.AddRange(
                new ProgramRequest
                {
                    ProgramRequestId = 10,
                    InmateId = 120,
                    CreateBy = 12,
                    CreateDate = DateTime.Now.AddDays(-3),
                    UpdateDate = DateTime.Now.AddDays(-3),
                    UpdateBy = 11,
                    ProgramClassId = 10
                },
                new ProgramRequest
                {
                    ProgramRequestId = 11,
                    InmateId = 110,
                    CreateBy = 13,
                    CreateDate = DateTime.Now.AddDays(-3),
                    UpdateDate = DateTime.Now.AddDays(-3),
                    UpdateBy = 12,
                    ProgramClassId = 11

                }
            );

            Db.ProgramCategory.AddRange(
                new ProgramCategory
                {
                    ProgramCategoryId = 10,
                    CreateDate = DateTime.Now.AddDays(-4),
                    UpdateBy = 12,
                    CreateBy = 11,
                    UpdateDate = DateTime.Now.AddDays(-3),
                    DeleteFlag = 0,
                    FacilityId = 2,
                    ProgramCategory1 = "105"
                },
                new ProgramCategory
                {
                    ProgramCategoryId = 11,
                    CreateDate = DateTime.Now.AddDays(-4),
                    UpdateBy = 11,
                    CreateBy = 12,
                    UpdateDate = DateTime.Now.AddDays(-3),
                    DeleteFlag = null,
                    FacilityId = 2,
                    ProgramCategory1 = "120"
                },
                new ProgramCategory
                {
                    ProgramCategoryId = 12,
                    CreateDate = DateTime.Now.AddDays(-3),
                    UpdateBy = 11,
                    CreateBy = 11,
                    UpdateDate = DateTime.Now.AddDays(-3),
                    DeleteFlag = 0,
                    FacilityId = 1,
                    ProgramCategory1 = "205"
                }
            );
            Db.ProgramAttend.AddRange(
                new ProgramAttend
                {
                    ProgramAttendId = 10,
                    CreateDate = DateTime.Now.AddDays(-10),
                    CreateBy = 12,
                    UpdateDate = DateTime.Now.AddDays(-9),
                    UpdateBy = 12,
                    CompleteBy = 12,
                    CompleteFlag = 1,
                    CompleteDate = DateTime.Now,
                    OccuranceDate = DateTime.Now,
                    ScheduleId = 500

                },
                new ProgramAttend
                {
                    ProgramAttendId = 11,
                    CreateDate = DateTime.Now.AddDays(-10),
                    CreateBy = 12,
                    UpdateDate = DateTime.Now.AddDays(-9),
                    UpdateBy = 12,
                    CompleteBy = null,
                    CompleteFlag = null,
                    CompleteDate = null
                }
            );
            Db.ProgramAttendInmate.AddRange(
                new ProgramAttendInmate
                {
                    ProgramAttendInmateId = 20,
                    ProgramAttendId = 10,
                    CreateDate = DateTime.Now.AddDays(-10),
                    InmateId = 100,
                    NotAttendFlag = 0,
                    ProgramHours = null,
                    AttendFlag = 1
                },
                new ProgramAttendInmate
                {
                    ProgramAttendInmateId = 21,
                    ProgramAttendId = 10,
                    CreateDate = DateTime.Now.AddDays(-10),
                    InmateId = 101,
                    NotAttendFlag = 1,
                    ProgramHours = null
                }
        );


        }

    }
}
