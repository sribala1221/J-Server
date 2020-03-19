using System;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using ServerAPI.Services;
using ServerAPI.Tests;
using System.Collections.Generic;
using ServerAPI.ViewModels;
using Xunit;
using System.Linq;
using GenerateTables.Models;
using ServerAPI.JMS.Tests.Helper;
using Moq;
using Microsoft.AspNetCore.Identity;
using JwtDb.Models;
using Microsoft.Extensions.Caching.Memory;

// ReSharper disable once CheckNamespace
namespace ServerAPI.JMS.Tests
{
    [Collection("Database collection")]
    public class IncidentAppealsServiceTest
    {
        private readonly IncidentAppealsService _incidentAppealsService;
        private readonly DbInitialize _fixture;
        private readonly IConfigurationRoot _configuration = MockHelpers.GetIConfigurationRoot();
        private readonly Mock<IMemoryCache> _memory = new Mock<IMemoryCache>();

        public IncidentAppealsServiceTest(DbInitialize fixture)
        {
            _fixture = fixture;
            Mock<UserManager<AppUser>> userMgr = MockHelpers.MockUserManager<AppUser>();
            PersonService personService = new PersonService(fixture.Db);
            InterfaceEngineService interfaceEngineService = new InterfaceEngineService(_fixture.Db,
                _configuration, _memory.Object);
            AppletsSavedService appletsSavedService = new AppletsSavedService(fixture.Db, _configuration);
            PhotosService photosService = new PhotosService(fixture.Db, _configuration);
            HttpContextAccessor httpContext = new HttpContextAccessor { HttpContext = fixture.Context.HttpContext };
            CommonService commonService = new CommonService(_fixture.Db, _configuration,
                httpContext, personService,appletsSavedService,interfaceEngineService);
            _incidentAppealsService = new IncidentAppealsService(_fixture.Db, commonService, httpContext);
        }

        [Fact]
        public void GetIncidentAppealsCount()
        {
            //Act
            List<KeyValuePair<string, int>> lstKeyValuePair = _incidentAppealsService.GetIncidentAppealsCount(2);

            //Assert
            Assert.True(lstKeyValuePair.Count > 0);
        }

        [Fact]
        public void GetIncidentAppeals()
        {
            //Arrange
            AppealsParam appealsParam = new AppealsParam
            {
                FacilityId = 2,
                DateSelection = DateSelection.ByDateRange,
                DateFrom = DateTime.Now.AddDays(-1),
                DateTo = DateTime.Now
            };
            //Act
            List<Appeals> lstAppeals = _incidentAppealsService.GetIncidentAppeals(appealsParam);

            //Assert
            Appeals appeals = lstAppeals.Single(a => a.InmateId == 120);
            Assert.Equal("DIS_100102", appeals.IncidentNumber);

        }

        [Fact]
        public async void InsertDisciplinaryInmateAppeal()
        {
            //Arrange
            DispInmateAppeal dispInmate = new DispInmateAppeal
            {
                DisciplinaryInmateId = 124,
                ReportedBy = 12,
                AppealDate = DateTime.Now,
                AppealReason = 5
            };
            //Before Insert
            DisciplinaryInmateAppeal disciplinaryInmateAppeal = _fixture.Db.DisciplinaryInmateAppeal.SingleOrDefault(
                d => d.DisciplinaryInmateId == 124);
            Assert.Null(disciplinaryInmateAppeal);

            //Act
            await _incidentAppealsService.InsertDisciplinaryInmateAppeal(dispInmate);

            //After Insert
            //Assert
            disciplinaryInmateAppeal = _fixture.Db.DisciplinaryInmateAppeal.Single(
                d => d.DisciplinaryInmateId == 124);
            Assert.NotNull(disciplinaryInmateAppeal);

        }
        [Fact]
        public async void UpdateDisciplinaryInmateAppeal()
        {
            //Arrange
            DispInmateAppeal dispInmate = new DispInmateAppeal
            {
                DisciplinaryInmateId = 123,
                ReportedBy = 11,
                AppealDate = DateTime.Now,
                AppealNote = "WRONG APPEAL",
                AppealReason = 4,
                SendForReview = 7
            };
            //Before Update
            DisciplinaryInmateAppeal disciplinaryInmateAppeal = _fixture.Db.DisciplinaryInmateAppeal.Single(
                d => d.DisciplinaryInmateAppealId == 12);
            Assert.Null(disciplinaryInmateAppeal.AppealNote);

            //Act
            await _incidentAppealsService.UpdateDisciplinaryInmateAppeal(12, dispInmate);

            //After Update
            //Assert
            disciplinaryInmateAppeal = _fixture.Db.DisciplinaryInmateAppeal.Single(
                d => d.DisciplinaryInmateAppealId == 12);
            Assert.Equal("WRONG APPEAL", disciplinaryInmateAppeal.AppealNote);

        }
    }
}
