using System;
using Microsoft.AspNetCore.Http;
using ServerAPI.Services;
using ServerAPI.Tests;
using ServerAPI.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using GenerateTables.Models;


// ReSharper disable once CheckNamespace
namespace ServerAPI.JMS.Tests
{
    [Collection("Database collection")]
    public class MiscAttendeeServiceTest
    {
        private readonly MiscAttendeeService _miscAttendeeService;
        private readonly DbInitialize _fixture;

        public MiscAttendeeServiceTest(DbInitialize fixture)
        {
            _fixture = fixture;
            HttpContextAccessor httpContext = new HttpContextAccessor { HttpContext = _fixture.Context.HttpContext };
            _miscAttendeeService = new MiscAttendeeService(_fixture.Db, httpContext);
        }

        [Fact]
        public void GetInmateAttendeeList()
        {
            //Act
            List<AttendenceVm> lstAttendeeList = _miscAttendeeService.GetInmateAttendeeList(125);

            //Assert
            Assert.True(lstAttendeeList.Count > 0);

        }

        [Fact]
        public async Task InsertAttendee()
        {
            //Arrange
            AttendenceVm attendence = new AttendenceVm
            {
                CreateDate = DateTime.Now,
                AttendanceDate = DateTime.Now.Date.AddDays(-3),
                InmateId = 103,
                ProgramFlag = true,
                AttendFlag = true
            };
            //Before Insert
            ArrestSentenceAttendance arrestSentenceAttendance = _fixture.Db.ArrestSentenceAttendance.SingleOrDefault
                (a => a.InmateId == 103);
            Assert.Null(arrestSentenceAttendance);

            //Act
            await _miscAttendeeService.InsertAttendee(attendence);

            //Assert
            //After Insert
            arrestSentenceAttendance = _fixture.Db.ArrestSentenceAttendance.Single
                 (a => a.InmateId == 103);
            Assert.NotNull(arrestSentenceAttendance);

        }

        [Fact]
        public async Task UpdateAttendee()
        {
            //Arrange
            AttendenceVm attendence = new AttendenceVm
            {
                AttendanceId = 23,
                CreateDate = DateTime.Now,
                InmateId = 105,
                NoDayDayFlag = true,
                AttendFlag = true,
                WorkCrewFlag = true
            };
            //Before Update
            ArrestSentenceAttendance arrestSentenceAttendance = _fixture.Db.ArrestSentenceAttendance.Single
                (a => a.ArrestSentenceAttendanceId == 23);
            Assert.Null(arrestSentenceAttendance.WorkCrewFlag);

            //Act
            await _miscAttendeeService.UpdateAttendee(attendence);

            //Assert
            //After Update
            arrestSentenceAttendance = _fixture.Db.ArrestSentenceAttendance.Single
                (a => a.ArrestSentenceAttendanceId == 23);
            Assert.Equal(1, arrestSentenceAttendance.WorkCrewFlag);

        }

        [Fact]
        public async Task DeleteUndoAttendee()
        {
            //Beofore DeleteUndo 
            ArrestSentenceAttendance arrestSentenceAttendance = _fixture.Db.ArrestSentenceAttendance.Single
                (a => a.ArrestSentenceAttendanceId == 24);
            Assert.Equal(1,arrestSentenceAttendance.DeleteFlag);

            //Act
            await _miscAttendeeService.DeleteUndoAttendee(24);

            //Assert
            //After AfterUndo
            arrestSentenceAttendance = _fixture.Db.ArrestSentenceAttendance.Single
                (a => a.ArrestSentenceAttendanceId == 24);
            Assert.Equal(0, arrestSentenceAttendance.DeleteFlag);

        }

    }
}
