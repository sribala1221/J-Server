using ServerAPI.Services;
using ServerAPI.Tests;
using Xunit;
using ServerAPI.ViewModels;
using System.Threading.Tasks;
using GenerateTables.Models;
using System.Linq;
using System;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using ServerAPI.JMS.Tests.Helper;
using Microsoft.AspNetCore.Identity;
using Moq;
using JwtDb.Models;
using Microsoft.Extensions.Caching.Memory;

// ReSharper disable once CheckNamespace
namespace ServerAPI.JMS.Tests
{
    [Collection("Database collection")]
    public class ClassCountServiceTest
    {
        private readonly ClassCountService _classCountService;
        private readonly DbInitialize _fixture;
        private readonly IConfigurationRoot _configuration = MockHelpers.GetIConfigurationRoot();
        private readonly Mock<IMemoryCache> _memory = new Mock<IMemoryCache>();
        public ClassCountServiceTest(DbInitialize fixture)
        {
            _fixture = fixture;
            PersonService personService = new PersonService(fixture.Db);
            Mock<UserManager<AppUser>> userMgr = MockHelpers.MockUserManager<AppUser>();
            PhotosService photosService = new PhotosService(fixture.Db, _configuration);
            InterfaceEngineService interfaceEngineService = new InterfaceEngineService(_fixture.Db,
                _configuration, _memory.Object);

            AppletsSavedService appletsSavedService = new AppletsSavedService(fixture.Db, _configuration);
            HttpContextAccessor httpContext = new HttpContextAccessor { HttpContext = fixture.Context.HttpContext };
            CommonService commonService = new CommonService(_fixture.Db, _configuration,
                httpContext, personService, appletsSavedService, interfaceEngineService);
            _classCountService = new ClassCountService(_fixture.Db, commonService);
        }

        [Fact]
        public void ClassCount_GetHousing()
        {
            //Act
            ClassCountHousing classCountHousing = _classCountService.GetHousing(2);
            Assert.NotNull(classCountHousing);
        }

        [Fact]
        public async Task ClassCount_InsertFloorNote()
        {
            //Arrange
            FloorNotesVm floorNotes = new FloorNotesVm
            {
                FloorNoteLocationId = 8,
                FloorNoteLocation = "COIMBATORE",
                FloorNoteWatchFlag = 0,
                FloorNoteNarrative = "SAME AGE INMATES NOT ACCEPTABLE",
                InmateId = 105,
                FloorNoteDate = DateTime.Now
            };
            //Before Insert
            FloorNotes notes = _fixture.Db.FloorNotes.SingleOrDefault(f => f.FloorNoteLocation == "COIMBATORE");
            Assert.Null(notes);
            //Act
            await _classCountService.InsertFloorNote(floorNotes);
            //Assert
            //After Insert
            notes = _fixture.Db.FloorNotes.Single(f => f.FloorNoteLocation == "COIMBATORE");
            Assert.NotNull(notes);
        }

        [Fact]
        public void GetHousingCountDetails()
        {
            //Arrange
            ClassCountInputs classCount = new ClassCountInputs
            {
                FacilityId = 2,
                CountFlag = false,
                CountRefreshFlag = true
            };
            //Act
            HousingDetails result = _classCountService.GetHousingCountDetails(classCount);

            //Assert
            Assert.True(result.ParentClassDayCount.Count > 1);
            HousingCount housingCount = result.ParentClassDayCount.Single(p => p.DayCount == 5);
            Assert.Equal("5", housingCount.Days);
        }
    }
}
