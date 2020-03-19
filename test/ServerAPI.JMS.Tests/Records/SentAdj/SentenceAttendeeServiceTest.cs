using Microsoft.AspNetCore.Http;
using ServerAPI.Services;
using ServerAPI.Tests;
using System;
using Xunit;
using ServerAPI.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GenerateTables.Models;

// ReSharper disable once CheckNamespace
namespace ServerAPI.JMS.Tests
{
    [Collection("Database collection")]
    public class SentenceAttendeeServiceTest
    {
        private readonly SentenceAttendeeService _sentenceAttendee;
        private readonly DbInitialize _fixture;

        public SentenceAttendeeServiceTest(DbInitialize fixture)
        {
            _fixture = fixture;
            HttpContextAccessor httpContext = new HttpContextAccessor { HttpContext = fixture.Context.HttpContext };
            _sentenceAttendee = new SentenceAttendeeService(_fixture.Db, httpContext);

        }

        [Fact]
        public void GetAttendanceList()
        {
            //Act
            List<AttendanceVm> lstAttendance = _sentenceAttendee.GetAttendanceList(DateTime.Now);

            //Assert
            AttendanceVm attendance = lstAttendance.First(a => a.InmateId == 100);
            Assert.True(attendance.NoDayForDaySentence);
            Assert.Equal(20, attendance.ArrestSentenceAttendanceId);
        }

        [Fact]
        public void GetAttendanceValue()
        {
            //Act
            AttendanceVm attendance = _sentenceAttendee.GetAttendanceValue(21);
           
            //Assert
            Assert.NotNull(attendance);
            Assert.Equal(101, attendance.InmateId);
        }

        [Fact]

        public void GetRecentAttendanceValue()
        {
            //Act
            List<RecentAttendanceVm> lstRecentAttendance = _sentenceAttendee.GetRecentAttendanceValue(DateTime.Now);

            //Assert
            Assert.True(lstRecentAttendance.Count > 2);
        }

        [Fact]
        public void GetDuplicateAttendance()
        {
            //Act
            int result = _sentenceAttendee.GetDuplicateAttendance(DateTime.Now.AddDays(-1), 105);

            //Assert
            Assert.Equal(1, result);
        }

        [Fact]
        public async Task InsertAttendance()
        {
            //Arrange
            AttendanceVm attendance = new AttendanceVm
            {
                ArrestSentenceAttendanceId = 25,
                NoDayForDayFlag = true,
                AttendanceCredit = 1
            };

            //Before Update
            ArrestSentenceAttendance arrestSentenceAttendance = _fixture.Db.ArrestSentenceAttendance
                .Single(a => a.ArrestSentenceAttendanceId == 25);
            Assert.Null(arrestSentenceAttendance.NoDayDayFlag);
            Assert.Null(arrestSentenceAttendance.AttendCredit);

            //Act
            await _sentenceAttendee.InsertAttendance(attendance);

            //Assert
            //After Update
            arrestSentenceAttendance = _fixture.Db.ArrestSentenceAttendance.Single(
                a => a.ArrestSentenceAttendanceId == 25);
            Assert.NotNull(arrestSentenceAttendance.NoDayDayFlag);
            Assert.NotNull(arrestSentenceAttendance.AttendCredit);
        }

        [Fact]
        public void GetRunAttendanceList()
        {
            //Act
            WorkAttendanceVm workAttendance = _sentenceAttendee.GetRunAttendanceList(DateTime.Now);

            //Assert
            Assert.Equal(10, workAttendance.ArrestSentenceAttendanceDayId);
            Assert.True(workAttendance.Attendance.Count > 0);
        }

        [Fact]
        public async Task InsertArrestSentenceAttendanceDay()
        {
            //Arrange
            WorkAttendanceVm workAttendance = new WorkAttendanceVm
            {
                AttendanceDate = DateTime.Now.Date.AddDays(-20),
                Attendance = new List<AttendanceVm>
                {
                    new AttendanceVm
                    {
                        InmateId = 104,
                        DeleteFlag = false,
                        AttendCredit = 1,
                        AttendFlag = true
                    }
                }
            };

            //Before Insert
            ArrestSentenceAttendance arrestSentenceAttendance = _fixture.Db.ArrestSentenceAttendance.
                SingleOrDefault(a => a.InmateId == 104);
            Assert.Null(arrestSentenceAttendance);

            //Act
            await _sentenceAttendee.InsertArrestSentenceAttendanceDay(workAttendance);

            //Assert
            //After Insert
            arrestSentenceAttendance = _fixture.Db.ArrestSentenceAttendance.Single(
                a => a.InmateId == 104);
            Assert.NotNull(arrestSentenceAttendance);

            // Temp on hold due to development


            //workAttendance = new WorkAttendanceVm
            //{
            //    AttendanceDate = DateTime.Now.Date.AddDays(-20),
            //    ArrestSentenceAttendanceDayId = 13,
            //    Attendance = new List<AttendanceVm>
            //    {
            //        new AttendanceVm
            //        {
            //            InmateId = 111,
            //            ArrestSentenceAttendanceId = 27,
            //            DeleteFlag = false,
            //            AttendCredit = 1,
            //            AttendFlag = true
            //        }
            //    }
            //};

            //arrestSentenceAttendance = _fixture.Db.ArrestSentenceAttendance.
            //   Single(a => a.ArrestSentenceAttendanceId == 27);
            //Assert.Equal(12,arrestSentenceAttendance.ArrestSentenceAttendanceDayId);

            //await _sentenceAttendee.InsertArrestSentenceAttendanceDay(workAttendance);

            //arrestSentenceAttendance = _fixture.Db.ArrestSentenceAttendance.Single(
            //    a => a.ArrestSentenceAttendanceId == 27);
            //Assert.Equal(13, arrestSentenceAttendance.ArrestSentenceAttendanceDayId);
            
        }


    }
}
