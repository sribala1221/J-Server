using GenerateTables.Models;
using JwtDb.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Moq;
using ServerAPI.JMS.Tests.Helper;
using ServerAPI.Services;
using ServerAPI.Tests;
using ServerAPI.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

// ReSharper disable once CheckNamespace
namespace ServerAPI.JMS.Tests
{
    [Collection("Database collection")]
    public class GrievanceServiceTest
    {
        private readonly GrievanceService _grievanceService;
        private readonly DbInitialize _fixture;
        private readonly IConfigurationRoot _configuration = MockHelpers.GetIConfigurationRoot();
        private readonly Mock<IMemoryCache> _memory = new Mock<IMemoryCache>();

        public GrievanceServiceTest(DbInitialize fixture)
        {
            _fixture = fixture;
            PersonService personService = new PersonService(_fixture.Db);
            AtimsHubService atimsHubService = new AtimsHubService(_fixture.HubContext.Object);
            Mock<UserManager<AppUser>> userMgr = MockHelpers.MockUserManager<AppUser>();
            AppletsSavedService appletsSavedService = new AppletsSavedService(fixture.Db, _configuration);
            PhotosService photosService = new PhotosService(fixture.Db, _configuration);
            HttpContextAccessor httpContext = new HttpContextAccessor { HttpContext = fixture.Context.HttpContext };
            InterfaceEngineService interfaceEngineService = new InterfaceEngineService(_fixture.Db,
                _configuration, _memory.Object);
            CommonService commonService = new CommonService(_fixture.Db, _configuration,
                 httpContext, personService,appletsSavedService,interfaceEngineService);
            WizardService wizardService = new WizardService(_fixture.Db,atimsHubService);
            _grievanceService = new GrievanceService(_fixture.Db, commonService, httpContext, wizardService, personService,
                photosService);
        }

        [Fact]
        public void GetGrievanceCount()
        {
            //Arrange
            GrievanceCountVm grievanceCount = new GrievanceCountVm
            {
                PersonnelId = 12,
                FacilityId = 1,
                PersonnelType = "CREATED BY",
                Hours = 6,
                FromDate = DateTime.Now
            };
            //Act
            GrievanceCountVm grievance = _grievanceService.GetGrievanceCount(grievanceCount);
            //Assert
            Assert.NotNull(grievance);
            Assert.True(grievance.LstAgainst.Count > 0);
            Assert.Equal("CREATED BY", grievance.PersonnelType);
        }

        [Fact]
        public void GetGrievanceCountPopupDetails()
        {
            //Arrange
            GrievanceCountVm grievanceCounts = new GrievanceCountVm
            {
                FacilityId = 2,
                Hours = 4,
                Type = GrievanceType.Personnel
            };
            //Act
            List<GrievanceCountDetails> grievanceCountDetails = _grievanceService.
                GetGrievanceCountPopupDetails(grievanceCounts);
            //Assert
            GrievanceCountDetails grievanceCount = grievanceCountDetails.Single(g => g.InmateId == 102);
            Assert.Equal("KUMAR", grievanceCount.LastName);
            Assert.Equal("PKS000", grievanceCount.Number);
        }

        [Fact]
        public void GetGrievanceDetails()
        {
            //Arrange
            GrievanceInputs grievanceInputs = new GrievanceInputs
            {
                FacilityId = 1,
                ActionFlag = 2
            };
            //Act
            GrievanceDetailsVm grievanceDetails = _grievanceService.GetGrievanceDetails(grievanceInputs);
            //Assert
            GrievanceVm grievance = grievanceDetails.GrievanceList.Single(g => g.InmateId == 105);
            Assert.Equal("ELECTRIC", grievance.Department);
        }

        [Fact]
        public void GetGrievanceHistory()
        {
            //Arrange
            GrievanceInputs grievanceInputs = new GrievanceInputs
            {
                FacilityId = 1
            };
            //Act
            GrievanceHistoryDetails grievanceHistory = _grievanceService.GetGrievanceHistory(grievanceInputs);
            //Assert
            Assert.True(grievanceHistory.HousingDetails.Count > 2);

        }
        [Fact]
        public async Task InsertAdditionalInmate()
        {
            //Arrange
            GrievanceVm grievance = new GrievanceVm
            {
                InmateId = 100,
                GrievanceId = 12
            };
            GrievanceInmate grievanceInmate = _fixture.Db.GrievanceInmate.SingleOrDefault(g => g.InmateId == 100);
            Assert.Null(grievanceInmate);
            //Act
            await _grievanceService.InsertAdditionalInmate(grievance);
            //Assert
            grievanceInmate = _fixture.Db.GrievanceInmate.Single(g => g.InmateId == 100);
            Assert.NotNull(grievanceInmate);
        }

        [Fact]
        public async Task DeleteAdditionalInmate()
        {
            //Arrange
            GrievanceInmate grievanceInmate = _fixture.Db.GrievanceInmate.Single(g => g.GrievanceInmateId == 10);
            Assert.NotNull(grievanceInmate);
            //Act
            await _grievanceService.DeleteAdditionalInmate(10);
            //Assert
            grievanceInmate = _fixture.Db.GrievanceInmate.SingleOrDefault(g => g.GrievanceInmateId == 10);
            Assert.Null(grievanceInmate);

        }
        [Fact]
        public async Task InsertAdditionalPersonnel()
        {
            //Arrange
            GrievanceVm grievance = new GrievanceVm
            {
                GrievanceId = 5,
                PersonnelId = 13
            };
            GrievancePersonnel grievancePersonnel = _fixture.Db.GrievancePersonnel.SingleOrDefault(g => g.PersonnelId == 13);
            Assert.Null(grievancePersonnel);
            //Act
            await _grievanceService.InsertAdditionalPersonnel(grievance);

            //Assert
            grievancePersonnel = _fixture.Db.GrievancePersonnel.Single(g => g.PersonnelId == 13);
            Assert.NotNull(grievancePersonnel);
        }

        [Fact]
        public async Task DeleteAdditionalPersonnel()
        {
            //Arrange
            GrievancePersonnel grievancePersonnel = _fixture.Db.GrievancePersonnel.Single(g => g.GrievancePersonnelId == 12);
            Assert.NotNull(grievancePersonnel);
            //Act
            await _grievanceService.DeleteAdditionalPersonnel(12);
            //Assert
            grievancePersonnel = _fixture.Db.GrievancePersonnel.SingleOrDefault(g => g.GrievancePersonnelId == 12);
            Assert.Null(grievancePersonnel);
        }

        [Fact]
        public async Task UpdateGrievanceDepartment()
        {
            //Arrange
            GrievanceVm grievanceValues = new GrievanceVm
            {
                Department = "SOCIAL",
                ReadyForReview = 2,
                GrievanceId = 8
            };

            Grievance grievance = _fixture.Db.Grievance.Single(g => g.GrievanceId == 8);
            Assert.Null(grievance.Department);

            //Act
            await _grievanceService.UpdateGrievanceDepartment(grievanceValues);

            //Assert
            grievance = _fixture.Db.Grievance.Single(g => g.GrievanceId == 8);
            Assert.Equal("SOCIAL", grievance.Department);
        }

        [Fact]
        public async Task UpdateGrievanceReview()
        {
            //Arrange
            GrievanceReview grievanceReview = new GrievanceReview
            {
                GrievanceId = 9,
                ReviewedBy = 12,
                ReviewedDate = DateTime.Now,
                LstGrievanceFlag = new List<string>()
            };
            Grievance grievance = _fixture.Db.Grievance.Single(x => x.GrievanceId == 9);
            Assert.Null(grievance.ReviewedBy);
            //Act
            await _grievanceService.UpdateGrievanceReview(grievanceReview);
            //Assert
            grievance = _fixture.Db.Grievance.Single(x => x.GrievanceId == 9);
            Assert.Equal(12, grievance.ReviewedBy);
        }

        [Fact]
        public void GetAdditionalDetails()
        {
            //Act
            GrievanceDetails grievanceDetails = _grievanceService.GetAdditionalDetails(6);

            //Assert
            Assert.Equal("CBI", grievanceDetails.Department);
            Assert.True(grievanceDetails.AdditionalInmateLst.Count > 0);
            Assert.True(grievanceDetails.AppealDetails.Count > 0);
        }
        [Fact]
        public void GetHousingDetails()
        {
            //Act
            GrievanceHousingDetails grievanceHousing = _grievanceService.GetHousingDetails(2);
            //Assert
            Assert.True(grievanceHousing.LocationList.Count > 2);
            Assert.True(grievanceHousing.HousingBedNumberList.Count > 0);
        }

        [Fact]
        public async Task UpdateGrvSetReview()
        {
            //Arrange
            GrievanceVm values = new GrievanceVm
            {
                FacilityId = 2,
                GrievanceId = 12,
                SetReview = 1
            };
            Grievance grievance = _fixture.Db.Grievance.Single(g => g.GrievanceId == 12);
            Assert.Null(grievance.SetReview);

            //Act
            await _grievanceService.UpdateGrvSetReview(values);

            //Assert
            grievance = _fixture.Db.Grievance.Single(g => g.GrievanceId == 12);
            Assert.Equal(1, grievance.SetReview);
        }

        [Fact]
        public void LoadGrievanceAttachments()
        {
            //Act
            List<PrebookAttachment> lstPrebookAttachments = _grievanceService.LoadGrievanceAttachments(7);

            //Assert
            PrebookAttachment prebookAttachment = lstPrebookAttachments.Single(p => p.AttachmentId == 16);
            Assert.Equal("BELT", prebookAttachment.AttachmentKeyword1);
            Assert.Equal("STORE ALL THINGS", prebookAttachment.AttachmentType);
        }

    }
}
