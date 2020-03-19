using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using ServerAPI.Services;
using ServerAPI.Tests;
using ServerAPI.ViewModels;
using Xunit;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using GenerateTables.Models;
using ServerAPI.JMS.Tests.Helper;
using Moq;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.AspNetCore.Identity;
using JwtDb.Models;

// ReSharper disable once CheckNamespace
namespace ServerAPI.JMS.Tests
{
    [Collection("Database collection")]
    public class HousingCellServiceTest
    {
        private readonly HousingCellService _housingCellService;
        private readonly IConfigurationRoot _configuration = MockHelpers.GetIConfigurationRoot();
        private readonly DbInitialize _fixture;
        private readonly Mock<IMemoryCache> _memory = new Mock<IMemoryCache>();
        public HousingCellServiceTest(DbInitialize fixture)
        {
            _fixture = fixture;
            PersonService personService = new PersonService(_fixture.Db);
            Mock<UserManager<AppUser>> userMgr = MockHelpers.MockUserManager<AppUser>();
            PhotosService photosService = new PhotosService(_fixture.Db, _configuration);
            AppletsSavedService appletsSavedService = new AppletsSavedService(fixture.Db, _configuration);
            HttpContextAccessor httpContext = new HttpContextAccessor { HttpContext = fixture.Context.HttpContext };
           
            InterfaceEngineService interfaceEngineService = new InterfaceEngineService(fixture.Db, _configuration,
                _memory.Object);
            CommonService commonService = new CommonService(_fixture.Db, _configuration,
                httpContext, personService, appletsSavedService,interfaceEngineService);
            FacilityPrivilegeService facilityPrivilegeService = new FacilityPrivilegeService(_fixture.Db, httpContext,
                personService);
            InmateService inmateService = new InmateService(_fixture.Db, commonService, personService, httpContext,
                facilityPrivilegeService, photosService);
            HousingConflictService housingConflictService = new HousingConflictService(_fixture.Db, inmateService,
                photosService);

            LockdownService lockdownService = new LockdownService(_fixture.Db, httpContext);
            HousingService housingService = new HousingService(_fixture.Db, housingConflictService,
                httpContext, photosService, facilityPrivilegeService, lockdownService, interfaceEngineService);

            _housingCellService = new HousingCellService(_fixture.Db, housingService, httpContext, photosService, lockdownService);
        }


        [Fact]
        public void HousingCell_GetCellViewerDetails()
        {
            //Act
            CellViewerDetails cellViewer = _housingCellService.GetCellViewerDetails(1, 6, 5);

            //Assert
            Assert.NotNull(cellViewer.BedNumberCapacityList);
            Assert.NotNull(cellViewer.HousingHeaderDetails);
            Assert.NotNull(cellViewer.NumberCapacityList);
        }

        [Fact]
        public void HousingCell_GetBedNumberDetails()
        {
            //Act
            CellViewerDetails cellViewer = _housingCellService.GetBedNumberDetails(1, 6, "UPB02");

            //Assert
            Assert.NotNull(cellViewer.HousingStatsDetails.FlagCountList);
            InmateSearchVm inmateSearch = cellViewer.InmateDetailsList.Single(i => i.InmateId == 110);
            Assert.Equal("TRICHY", inmateSearch.Location);
        }

        [Fact]
        public void HousingCell_GetOutOfServiceDetails()
        {
            //Act   
            List<HousingUnitVm> housingUnit = _housingCellService.GetOutOfServiceDetails(11, 2);
            Assert.NotNull(housingUnit);
        }

        [Fact]
        public void HousingCell_GetOutOfServiceHistory()
        {
            //Arrange
            HousingInputVm housingInput = new HousingInputVm
            {
                FacilityId = 2,
                HousingUnitListId = 9,
                HousingBedNumber = "UPA01",
                HousingLocation = "UP-A"
            };

            //Act
            OutOfServiceHistory outOfService = _housingCellService.GetOutOfServiceHistory(housingInput);
            Assert.NotNull(outOfService.HousintUnitList);
        }

        [Fact]
        public async Task HousingCell_UpdateOutOfService()
        {
            //Arrange
            HousingUnitVm housingUnit = new HousingUnitVm
            {
                HousingUnitId = 12,
                HousingUnitOutOfService = 5,
                HousingUnitOutOfserviceReason = "MAINTENANCE",
                HousingUnitOutOfServiceNote = "OOS CLEARED"
            };
            HousingUnit housing = _fixture.Db.HousingUnit.Single(h => h.HousingUnitId == 12);
            Assert.Null(housing.HousingUnitOutOfServiceNote);

            HousingUnitOosHistory housingUnitOos = _fixture.Db.HousingUnitOosHistory
                .SingleOrDefault(h => h.HousingUnitId == 12);
            Assert.Null(housingUnitOos);

            //Act
            await _housingCellService.UpdateOutOfService(housingUnit);

            //Assert
            Assert.Equal("OOS CLEARED", housing.HousingUnitOutOfServiceNote);

            housingUnitOos = _fixture.Db.HousingUnitOosHistory
                .Single(h => h.HousingUnitId == 12);
            Assert.NotNull(housingUnitOos);

        }

        [Fact]
        public void GetPropertyLibraryDetails()
        {
            //Act
            CellPropertyLibraryDetails cellProperty = _housingCellService.GetPropertyLibraryDetails(9, 10, "UPA02");
            //Assert
            IssuedPropertyVm issuedProperty = cellProperty.IssuedPropertyList.Single(i => i.PropertyLookupId == 5);
            Assert.Equal("THE QURAN – SPANISH", issuedProperty.PropertyName);
            Assert.NotNull(cellProperty.LibraryList);
        }

    }
}
