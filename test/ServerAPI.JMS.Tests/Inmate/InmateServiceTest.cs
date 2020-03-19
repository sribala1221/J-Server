using Xunit;
using ServerAPI.Services;
using ServerAPI.ViewModels;
using System.Collections.Generic;
using GenerateTables.Models;
using System.Linq;
using System.Threading.Tasks;
using ServerAPI.Tests;
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
    public class InmateServiceTest
    {
        private readonly InmateService _inmateService;
        private readonly DbInitialize _fixture;
        private readonly IConfigurationRoot _configuration = MockHelpers.GetIConfigurationRoot();
        private readonly Mock<IMemoryCache> _memory = new Mock<IMemoryCache>();
        public InmateServiceTest(DbInitialize fixture)
        {
            _fixture = fixture;
            PersonService personService = new PersonService(fixture.Db);
            AppletsSavedService appletsSavedService = new AppletsSavedService(fixture.Db, _configuration);
            Mock<UserManager<AppUser>> userMgr = MockHelpers.MockUserManager<AppUser>();
            PhotosService photosService = new PhotosService(fixture.Db, _configuration);
            InterfaceEngineService interfaceEngineService = new InterfaceEngineService(_fixture.Db,
                _configuration, _memory.Object);
            HttpContextAccessor httpContext = new HttpContextAccessor { HttpContext = fixture.Context.HttpContext };
            CommonService commonService = new CommonService(_fixture.Db, _configuration, httpContext, personService,appletsSavedService,interfaceEngineService);
            FacilityPrivilegeService facilityPrivilegeService = new FacilityPrivilegeService(_fixture.Db, httpContext,
                personService);
            _inmateService = new InmateService(_fixture.Db, commonService, personService, httpContext,
                facilityPrivilegeService, photosService);
        }

        [Fact]
        public void InmateService_GetInmateNote()
        {
            //Act
            InmateNoteVm result = _inmateService.GetInmateNote(1, 100);
            //Asserts
            GenerateTables.Models.Facility facility = result.FacilityList.Single(f => f.FacilityAbbr == "MCJ");
            Assert.Equal("Madras Central Jail", facility.FacilityName);

            List<KeyValuePair<int, string>> locationLit = result.LocationList;
            Assert.Equal(5, locationLit[0].Key);
            Assert.Equal("CHENNAI", locationLit[0].Value);
        }

        [Fact]
        public async Task InmateService_InsertInmateNote()
        {
            //Arrange
            FloorNotesVm floorNoteValue = new FloorNotesVm
            {
                FloorNoteType = "MISC",
                FloorNoteNarrative = "ALREADY ASSIGNED THE INMATE",
                FloorNoteLocation = "CHENNAI",
                FloorNoteLocationId = 5,
                FloorNoteOfficerId = 11,
                InmateId = 101
            };
            FloorNotes floorNote = _fixture.Db.FloorNotes.SingleOrDefault(f => f.FloorNoteType == "MISC");
            Assert.Null(floorNote);
            //Act
            await _inmateService.InsertInmateNote(floorNoteValue);
            //Assert
            floorNote = _fixture.Db.FloorNotes.Single(f => f.FloorNoteType == "MISC");
            Assert.Equal("CHENNAI", floorNote.FloorNoteLocation);
        }

        [Fact]
        public async Task InmateService_UpdateInmateNote()
        {
            //Arrange
            FloorNotesVm floorNotes = new FloorNotesVm
            {
                FloorNoteId = 10,
                FacilityId = 2,
                FloorNoteNarrative = "KEEP SEPARATE IN SAME LOCATION",
                FloorNoteLocation = "MADURAI",
                FloorNoteLocationId = 6,
                FloorNoteType = "NO TYPE"
            };
            FloorNotes floorNote = _fixture.Db.FloorNotes.Single(f => f.FloorNoteId == 10);
            Assert.Equal("INMATE GENDER CONFLICT", floorNote.FloorNoteNarrative);
            Assert.Equal("GENDER CHECK", floorNote.FloorNoteType);
            //Act
            await _inmateService.UpdateInmateNote(floorNotes);
            //Updated FloorNotes Details
            //Assert
            floorNote = _fixture.Db.FloorNotes.Single(f => f.FloorNoteId == 10);
            Assert.Equal("KEEP SEPARATE IN SAME LOCATION", floorNote.FloorNoteNarrative);
            Assert.Equal("NO TYPE", floorNote.FloorNoteType);
        }
    }
}
