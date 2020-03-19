using Microsoft.AspNetCore.Http;
using ServerAPI.Services;
using ServerAPI.Tests;
using ServerAPI.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using System.Threading.Tasks;
using GenerateTables.Models;

namespace ServerAPI.JMS.Tests.Facility
{
    [Collection("Database collection")]
    public class ObservationLogServiceTest
    {
        private readonly ObservationLogService _observationLog;
        private readonly DbInitialize _fixture;

        public ObservationLogServiceTest(DbInitialize fixture)
        {
            _fixture = fixture;
            HttpContextAccessor httpContext = new HttpContextAccessor { HttpContext = fixture.Context.HttpContext };
            _observationLog = new ObservationLogService(_fixture.Db, httpContext);
        }


        [Fact]
        public void GetObservationLogList()
        {
            //Arrange
            ConsoleInputVm consoleInput = new ConsoleInputVm
            {
                FacilityId = 1,
                HousingUnitListId = 15,
                ListConsoleHousingId = new List<int>
                { 7,5 }
            };

            //Act
            ObservationLogsVm observationLogs = _observationLog.GetObservationLogList(consoleInput);

            //Assert
            ObservationLogItemDetails observationLogItem = observationLogs.ObservationLogItemDetails
                .Single(o => o.ObservationScheduleId == 5);
            Assert.Equal("SUICIDE ATTEMPT", observationLogItem.ObservationScheduleNote);
        }

        [Fact]
        public void LoadObservationHistoryList()
        {
            //Act
            List<ObservationLogItemDetails> lstObservationLogItems = _observationLog.LoadObservationHistoryList(10, 101);
            
            //Assert
            ObservationLogItemDetails observationLogItem = lstObservationLogItems.Single(o => o.ObservationLogId == 7);
            Assert.Equal("FOR MEDICAL CHECKUP", observationLogItem.ObservationActionNote);
        }

        [Fact]
        public void LoadObservationScheduleEntry()
        {
            //Act
            ObservationScheduleVm observationSchedule = _observationLog.LoadObservationScheduleEntry(8);

            //Assert
            Assert.Equal(110, observationSchedule.InmateId);
        }
        [Fact]
        public void LoadObservationActionEntry()
        {
            //Act
            ObservationScheduleActionVm observationScheduleAction = _observationLog.LoadObservationActionEntry(10);

            //Assert
            Assert.Equal("SCHEDULE NOTE", observationScheduleAction.ObservationScheduleNote);
            Assert.Equal(120, observationScheduleAction.ObservationScheduleInterval);
        }

        [Fact]
        public async Task InsertObservationLog()
        {
            //Arrange
            ObservationLogItemDetails observationLogItemDetails = new ObservationLogItemDetails
            {
                ObservationScheduleActionId = 11,
                HousingUnitId = 7,
                ObservationLateEntryFlag = true,
                ObservationNotes = "HEALTH ISSUES",
                ObservationDate = DateTime.Now
            };

            //Act
            await _observationLog.InsertObservationLog(observationLogItemDetails);

            //Assert
            ObservationLog observationLog = _fixture.Db.ObservationLog
                .Single(o => o.ObservationScheduleActionId == 11);
            Assert.Equal("HEALTH ISSUES", observationLog.ObservationNote);
            Assert.Equal(11, observationLog.CreateBy);
        }
    }
}
