using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using ServerAPI.Services;
using ServerAPI.Tests;
using ServerAPI.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GenerateTables.Models;
using Xunit;
using ServerAPI.JMS.Tests.Helper;
using Moq;
using Microsoft.AspNetCore.Identity;
using JwtDb.Models;
using Microsoft.Extensions.Caching.Memory;

// ReSharper disable once CheckNamespace
namespace ServerAPI.JMS.Tests
{
    [Collection("Database collection")]
    public class CourtCommitServiceTest
    {
        private readonly CourtCommitService _courtCommitService;
        private readonly IConfigurationRoot _configuration = MockHelpers.GetIConfigurationRoot();
        private readonly DbInitialize _fixture;
        private readonly Mock<IMemoryCache> _memory = new Mock<IMemoryCache>();

        public CourtCommitServiceTest(DbInitialize fixture)
        {
            _fixture = fixture;
            Mock<UserManager<AppUser>> userMgr = MockHelpers.MockUserManager<AppUser>();
            AtimsHubService atimsHubService = new AtimsHubService(_fixture.HubContext.Object);
            PersonService personService = new PersonService(_fixture.Db);
            AppletsSavedService appletsSavedService = new AppletsSavedService(_fixture.Db, _configuration);
            PhotosService photosService = new PhotosService(fixture.Db, _configuration);
            HttpContextAccessor httpContext = new HttpContextAccessor { HttpContext = fixture.Context.HttpContext };
            InterfaceEngineService interfaceEngineService = new InterfaceEngineService(_fixture.Db,
                _configuration, _memory.Object);
            CommonService commonService = new CommonService(fixture.Db, _configuration,
                httpContext,personService,appletsSavedService,interfaceEngineService);
            WizardService wizardService = new WizardService(_fixture.Db,atimsHubService);

            _courtCommitService = new CourtCommitService(fixture.Db, httpContext, commonService, wizardService);
        }

        [Fact]
        public void CourtCommitHistoryDetails()
        {
            //Arrange

            CourtCommitHistorySearchVm courtCommitHistorySearch = new CourtCommitHistorySearchVm
            {
                FacilityId = 1,
                ActiveFlag = true,
                CompleteFlag = true,
                OfficerId = 11,
                FromDate = DateTime.Now.AddDays(-4),
                ToDate = DateTime.Now.AddDays(1),
                InterviewFromDate = DateTime.Now.AddDays(-1),
                InterviewToDate = DateTime.Now.AddDays(1),
                LastName = "DEVA"
            };

            //Act
            List<CourtCommitHistoryVm> lstCommitHistoryVms = _courtCommitService.CourtCommitHistoryDetails(courtCommitHistorySearch);

            //Assert
            CourtCommitHistoryVm coutCommitHistory = lstCommitHistoryVms.Single(d => d.ArrestingOfficerId == 11);
            Assert.Equal("B-4478503", coutCommitHistory.PrebookNumber);
            Assert.Equal(100, coutCommitHistory.PersonId);
            Assert.Equal("MCJ", coutCommitHistory.FacilityName);
        }

        [Fact]
        public async Task CourtCommitUpdateDelete()
        {
            //Before Update
            InmatePrebook inmatePrebook = _fixture.Db.InmatePrebook.Single(i => i.InmatePrebookId == 22);
            Assert.Equal(0, inmatePrebook.DeleteFlag);

            //Act
            await _courtCommitService.CourtCommitUpdateDelete(22);

            //Assert
            //After Update
            inmatePrebook = _fixture.Db.InmatePrebook.Single(i => i.InmatePrebookId == 22);
            Assert.Equal(1, inmatePrebook.DeleteFlag);

        }

        [Fact]
        public void GetIncidentWizard()
        {
            //Act
            AoWizardFacilityVm aoWizard = _courtCommitService.GetIncidentWizard();

            //Assert
            Assert.Equal("Madras Central Jail", aoWizard.Facility.FacilityName);
            Assert.Equal("MCJ", aoWizard.Facility.FacilityAbbr);
        }

        [Fact]
        public async Task WizardComplete()
        {
            //Arrange
            CourtCommitHistoryVm courtCommitHistory = new CourtCommitHistoryVm
            {
                FacilityId = 1,
                InmatePrebookId = 22,
                CompleteFlag = true
            };
            InmatePrebook inmatePrebook = _fixture.Db.InmatePrebook.Single(i => i.InmatePrebookId == 22);
            Assert.Null(inmatePrebook.CompleteBy);

            //Act
            await _courtCommitService.WizardComplete(courtCommitHistory);

            //Assert
            inmatePrebook = _fixture.Db.InmatePrebook.Single(i => i.InmatePrebookId == 22);
            Assert.Equal(11, inmatePrebook.CompleteBy);
        }
    }
}
