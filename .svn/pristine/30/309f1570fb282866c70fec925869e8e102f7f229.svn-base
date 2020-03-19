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
using Moq;
using Microsoft.Extensions.Caching.Memory;

// ReSharper disable once CheckNamespace
namespace ServerAPI.JMS.Tests
{
    [Collection("Database collection")]
    public class InmateTrackingServiceTest
    {
        private readonly InmateTrackingService _inmateTrackingService;
        private readonly DbInitialize _fixture;
        private readonly IConfigurationRoot _configuration = MockHelpers.GetIConfigurationRoot();
        private readonly Mock<IMemoryCache> _memory = new Mock<IMemoryCache>();
        public InmateTrackingServiceTest(DbInitialize fixture)
        {
            _fixture = fixture;
            PersonService personService = new PersonService(_fixture.Db);
            AppletsSavedService appletsSavedService = new AppletsSavedService(_fixture.Db, _configuration);
            InterfaceEngineService interfaceEngineService = new InterfaceEngineService(fixture.Db, _configuration,
                _memory.Object);
            PhotosService photosService = new PhotosService(_fixture.Db, _configuration);
            HttpContextAccessor httpContext = new HttpContextAccessor { HttpContext = fixture.Context.HttpContext };
            CommonService commonService = new  CommonService(_fixture.Db, _configuration,
                httpContext,personService,appletsSavedService,interfaceEngineService);
            FacilityPrivilegeService facilityPrivilegeService = new FacilityPrivilegeService(_fixture.Db, httpContext,
                personService);
           
            _inmateTrackingService = new InmateTrackingService(_fixture.Db, commonService, 
                httpContext,personService,facilityPrivilegeService,photosService,interfaceEngineService);
        }

        [Fact]
        public void InmateTrackingService_GetInmateTrackingDetails()
        {
            //Act
            InmateTrackingVm inmateTracking = _inmateTrackingService.GetInmateTrackingDetails(100);
            //Assert
            Assert.Equal("SVK661", inmateTracking.InmateNumber);

            //Personinfo
            Assert.Equal(50, inmateTracking.InmateTrackDetailsVm.PersonInfo.PersonId);
            Assert.Equal("SANGEETHA", inmateTracking.InmateTrackDetailsVm.PersonInfo.PersonFirstName);

            //Privilege
            IQueryable<PrivilegeDetailsVm> privilegeList = inmateTracking.InmateTrackDetailsVm.LocationList;
            PrivilegeDetailsVm privilegeDetails = privilegeList.Single(p => p.PrivilegeId == 5);
            Assert.Equal("CHENNAI", privilegeDetails.PrivilegeDescription);
            List<KeyValuePair<int, string>> expectedValues = inmateTracking.InmateTrackDetailsVm.RefusalReasonList;
            Dictionary<int, string> lookupValue = new Dictionary<int, string> { {5, "NOT AVAILABLE"} };
            Assert.Equal(expectedValues, lookupValue);
        }

        [Fact]
        public async Task InmateTrackingService_InsertInmateTracking()
        {
            //Arrange
            InmateTrackingVm inmateTrack = new InmateTrackingVm{
                InmateId = 103,
                SelectedLocationId = 5,
                SelectedLocation = "CHENNAI",
                FacilityId = 1,
                Person = new PersonVm
                {
                    PersonId = 60
                },
                ConflictDetails = new List<TrackingConflictVm> {
                    new TrackingConflictVm
                    {
                        InmateId = 103,
                        ConflictType = "GENDER CONFLICT",
                        Description = "GENDER LOCATION WARNING"
                    }
                }
            };
            FloorNotes floorNote = _fixture.Db.FloorNotes
                .SingleOrDefault(f => f.FloorNoteType == "CONFLICT CHECK");

            Assert.Null(floorNote);
            FloorNoteXref floorNoteXref = _fixture.Db.FloorNoteXref.SingleOrDefault(f => f.InmateId == 103);
            Assert.Null(floorNoteXref);
            //Act
            await _inmateTrackingService.InsertInmateTracking(inmateTrack);
            //Assert
            floorNote = _fixture.Db.FloorNotes.Single(f => f.FloorNoteType == "CONFLICT CHECK");

            floorNoteXref = _fixture.Db.FloorNoteXref.Single(f => f.InmateId == 103);

            Assert.NotNull(floorNote);
            Assert.NotNull(floorNoteXref);
        }
    }
}
