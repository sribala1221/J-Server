using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using ServerAPI.Services;
using ServerAPI.Tests;
using ServerAPI.ViewModels;
using System.Collections.Generic;
using Xunit;
using System.Linq;
using System.Threading.Tasks;
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
    public class BatchHousingServiceTest
    {
        private readonly BatchHousingService _batchHousing;
        private readonly IConfigurationRoot _configuration = MockHelpers.GetIConfigurationRoot();
        private readonly DbInitialize _fixture;
        private readonly Mock<IMemoryCache> _memory = new Mock<IMemoryCache>();

        public BatchHousingServiceTest(DbInitialize fixture)
        {
            _fixture = fixture;
            PersonService personService = new PersonService(fixture.Db);
            Mock<UserManager<AppUser>> userMgr = MockHelpers.MockUserManager<AppUser>();
            AppletsSavedService appletsSavedService = new AppletsSavedService(fixture.Db, _configuration);

            PhotosService photosService = new PhotosService(_fixture.Db, _configuration);
            HttpContextAccessor httpContext = new HttpContextAccessor { HttpContext = fixture.Context.HttpContext };
            InterfaceEngineService interfaceEngineService = new InterfaceEngineService(_fixture.Db,
                _configuration, _memory.Object);

            CommonService commonService = new CommonService(fixture.Db, _configuration,
                httpContext, personService,appletsSavedService,interfaceEngineService);
            FacilityPrivilegeService facilityPrivilegeService = new FacilityPrivilegeService(_fixture.Db, httpContext,
                personService);
            FacilityHousingService facilityHousingService = new FacilityHousingService(_fixture.Db, _configuration, 
                httpContext);
            InmateService inmateService = new InmateService(_fixture.Db, commonService, personService, httpContext,
                facilityPrivilegeService, photosService);

            HousingConflictService housingConflictService = new HousingConflictService(fixture.Db, inmateService,
                photosService);
            _batchHousing = new BatchHousingService(fixture.Db, housingConflictService, httpContext,
                facilityHousingService);
        }

        [Fact]
        public void GetBatchHousingDetails()
        {
            //Act
            BatchHousingVm batchHousing = _batchHousing.GetBatchHousingDetails(false, 1);

            //Assert
            CheckedOutLocation checkedOutLocation = batchHousing.LstCheckedOutLocation.Single(l => l.LocationId == 5);
            Assert.Equal("CHENNAI", checkedOutLocation.Location);
            FacilityHousing facilityHousing = batchHousing.LstFacilityHousing.Single(f => f.HousingUnitListId == 5);
            Assert.Equal("MCJ", facilityHousing.Facility);
        }

        [Fact]
        public void GetBatchHousingInmateDetails()
        {
            //Act
            List<InmateVm> lstInmates = _batchHousing.GetBatchHousingInmateDetails(1, 8, null);
            //Assert
            InmateVm inmate = lstInmates.Single(i => i.InmateId == 110);
            Assert.Equal("TRICHY", inmate.InmateCurrentTrack);
        }
        [Fact]
        public void GetHousingBedDetails()
        {
            //Act
            List<HousingCapacityVm> lstHousingCapacities = _batchHousing.GetHousingBedDetails(6);

            //Assert
            HousingCapacityVm housingCapacity = lstHousingCapacities.Single(h => h.HousingId == 12);
            Assert.Equal("UPB01", housingCapacity.HousingBedNumber);

        }
        [Fact]
        public void GetHousingConflict()
        {
            //Check null condition
            //Act
            List<HousingConflictVm> lstHousingConflicts = _batchHousing.GetHousingConflict(null);
            //Assert
            Assert.False(lstHousingConflicts.Count > 0);

            //Arrange
            List<HousingInputVm> housingInputs = new List<HousingInputVm>
            {
                new HousingInputVm
                {
                    FacilityId = 2,
                    HousingUnitListId = 12,
                    HousingBedLocation= "DOWNUP01",
                    InmateId = 107
                }
            };

            //Act
            lstHousingConflicts = _batchHousing.GetHousingConflict(housingInputs);

            //Assert
            Assert.True(lstHousingConflicts.Count > 0);
        }

        [Fact]
        public async Task CreateHousingUnitMoveHistoryAsync()
        {
            //Arrange
            List<HousingMoveParameter> lstHousingMoveParameters = new List<HousingMoveParameter>
            {
                new HousingMoveParameter
                {
                    InmateId = 200,
                    HousingUnitFromId= null,
                    HousingUnitToId= 11,
                    MoveReason = "FRIENDS"
                }
            };
            HousingUnitMoveHistory housingUnitMoveHistory = _fixture.Db.HousingUnitMoveHistory.
                SingleOrDefault(h => h.InmateId == 200);
            Assert.Null(housingUnitMoveHistory);

            //Act
            await _batchHousing.CreateHousingUnitMoveHistory(lstHousingMoveParameters);

            //Assert
            housingUnitMoveHistory = _fixture.Db.HousingUnitMoveHistory.Single(h => h.InmateId == 200);
            Assert.NotNull(housingUnitMoveHistory);
        }
    }
}
