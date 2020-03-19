using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using ServerAPI.Services;
using ServerAPI.Tests;
using System.Collections.Generic;
using System.Linq;
using ServerAPI.ViewModels;
using Xunit;
using System.Threading.Tasks;
using GenerateTables.Models;
using ServerAPI.JMS.Tests.Helper;
using JwtDb.Models;
using Moq;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;

// ReSharper disable once CheckNamespace
namespace ServerAPI.JMS.Tests
{
    [Collection("Database collection")]
    public class GrievanceAppealsServiceTest
    {
        private readonly GrievanceAppealsService _grievanceAppealsService;
        private readonly IConfigurationRoot _configuration = MockHelpers.GetIConfigurationRoot();
        private readonly DbInitialize _fixture;
        private readonly Mock<IMemoryCache> _memory = new Mock<IMemoryCache>();

        public GrievanceAppealsServiceTest(DbInitialize fixture)
        {
            _fixture = fixture;
            AtimsHubService atimsHubService = new AtimsHubService(_fixture.HubContext.Object);
            HttpContextAccessor httpContext = new HttpContextAccessor { HttpContext = _fixture.Context.HttpContext };
            Mock<UserManager<AppUser>> userMgr = MockHelpers.MockUserManager<AppUser>();
            AppletsSavedService appletsSavedService = new AppletsSavedService(_fixture.Db, _configuration);
            PhotosService photosService = new PhotosService(fixture.Db, _configuration);
            WizardService wizardService = new WizardService(_fixture.Db,atimsHubService);
            PersonService personService = new PersonService(fixture.Db);
            InterfaceEngineService interfaceEngineService = new InterfaceEngineService(_fixture.Db,
                _configuration, _memory.Object);
            CommonService commonService = new CommonService(_fixture.Db, _configuration,
                httpContext, personService,appletsSavedService,interfaceEngineService);
            GrievanceService grievanceService = new GrievanceService(_fixture.Db,commonService,httpContext,wizardService,
                personService,photosService);
            _grievanceAppealsService = new GrievanceAppealsService(_fixture.Db, commonService,
                grievanceService,photosService,httpContext,appletsSavedService);
        }


        [Fact]
        public void GetGrievanceAppealDetails()
        {
            //Act
            GrievanceAppealsVm grievanceAppeal = _grievanceAppealsService.GetGrievanceAppealDetails(2, true, true);

            //Assert
            Assert.True(grievanceAppeal.CategoryCount.Count > 0);
            Assert.True(grievanceAppeal.GrievanceAppealList.Count > 0);
            Assert.True(grievanceAppeal.LocationCount.Count > 0);
            Assert.True(grievanceAppeal.TypeCount.Count > 0);
        }

        [Fact]
        public void GetGrievanceDetails()
        {
            //Act
            List<GrievanceAppealDetails> lstGrievanceAppealDetails = _grievanceAppealsService.GetGrievanceDetails(9);

            //Assert
            GrievanceAppealDetails grievanceAppeal = lstGrievanceAppealDetails.Single(g => g.GrievanceId == 9);
            Assert.Equal("GR50", grievanceAppeal.GrievanceNumber);

            //Check with null condition 
            //Act
            lstGrievanceAppealDetails = _grievanceAppealsService.GetGrievanceDetails(null);

            //Assert
            //Check the AppealCount 
            grievanceAppeal = lstGrievanceAppealDetails.Single(g => g.GrievanceId == 11);
            Assert.True(grievanceAppeal.AppealCount > 0);
        }


        [Fact]
        public async Task InsertGrievanceAppeal()
        {
            //Arrange
            GrievanceAppealParam grievanceAppealParm = new GrievanceAppealParam
            {
                AppealCategory = 5,
                GrievanceId = 7,
                ReviewNote = "Testing"
            };
            //Before Insert
            GrievanceAppeal grievanceAppeal = _fixture.Db.GrievanceAppeal.SingleOrDefault(g => g.GrievanceId == 7);
            Assert.Null(grievanceAppeal);

            //Act
            await _grievanceAppealsService.InsertGrievanceAppeal(grievanceAppealParm);
            //After Insert
            //Assert
            grievanceAppeal = _fixture.Db.GrievanceAppeal.Single(g => g.GrievanceId == 7);
            Assert.NotNull(grievanceAppeal);

        }

        [Fact]
        public async Task UpdateGrievanceAppeal()
        {
            //Arrange
            GrievanceAppealParam grievanceAppealParam = new GrievanceAppealParam
            {
                GrievanceId = 10,
                ReviewNote = "NOT FOUNDED"
            };
            //Before Update
            GrievanceAppeal grievanceAppeal = _fixture.Db.GrievanceAppeal.Single(g => g.GrievanceAppealId == 13);
            Assert.Null(grievanceAppeal.ReviewNote);

            //Act
            await _grievanceAppealsService.UpdateGrievanceAppeal(13, grievanceAppealParam);

            //Assert
            //After Update
            grievanceAppeal = _fixture.Db.GrievanceAppeal.Single(g => g.GrievanceAppealId == 13);
            Assert.Equal("NOT FOUNDED", grievanceAppeal.ReviewNote);
        }

        [Fact]
        public async Task DeleteGrievanceAppeal()
        {
            //Before delete
            GrievanceAppeal grievanceAppeal = _fixture.Db.GrievanceAppeal.Single(g => g.GrievanceAppealId == 14);
            Assert.Equal(0, grievanceAppeal.DeleteFlag);
            Assert.Null(grievanceAppeal.DeleteBy);

            //Act
            await _grievanceAppealsService.DeleteGrievanceAppeal(14, true);

            //After Delete
            //Assert
            grievanceAppeal = _fixture.Db.GrievanceAppeal.Single(g => g.GrievanceAppealId == 14);
            Assert.Equal(1, grievanceAppeal.DeleteFlag);
            Assert.Equal(11, grievanceAppeal.DeleteBy);

            //Undo function
            grievanceAppeal = _fixture.Db.GrievanceAppeal.Single(g => g.GrievanceAppealId == 15);
            Assert.Equal(1, grievanceAppeal.DeleteFlag);
            Assert.Equal(12, grievanceAppeal.DeleteBy);

            //Act
            await _grievanceAppealsService.DeleteGrievanceAppeal(15, false);

            //Assert
            grievanceAppeal = _fixture.Db.GrievanceAppeal.Single(g => g.GrievanceAppealId == 15);
            Assert.Null(grievanceAppeal.DeleteFlag);
            Assert.Null(grievanceAppeal.DeleteBy);
        }


    }
}
