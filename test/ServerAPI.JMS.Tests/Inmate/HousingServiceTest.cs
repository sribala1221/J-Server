using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using ServerAPI.Services;
using ServerAPI.Tests;
using ServerAPI.ViewModels;
using Xunit;
using System.Linq;
using System.Collections.Generic;
using ServerAPI.JMS.Tests.Helper;
using Moq;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.AspNetCore.Identity;
using JwtDb.Models;

// ReSharper disable once CheckNamespace
namespace ServerAPI.JMS.Tests
{

    [Collection("Database collection")]
    public class HousingServiceTest
    {
        private readonly HousingService _housingService;
        private readonly HousingStatsService _housingStatsService;
        private readonly IConfigurationRoot _configuration = MockHelpers.GetIConfigurationRoot();
        private readonly Mock<IMemoryCache> _memory = new Mock<IMemoryCache>();

        public HousingServiceTest(DbInitialize fixture)
        {
            DbInitialize fixture1 = fixture;
            Mock<UserManager<AppUser>> userMgr = MockHelpers.MockUserManager<AppUser>();
            HttpContextAccessor httpContext = new HttpContextAccessor { HttpContext = fixture1.Context.HttpContext };
            PhotosService photosService = new PhotosService(fixture1.Db, _configuration);
            PersonService personService = new PersonService(fixture.Db);
            InterfaceEngineService interfaceEngineService = new InterfaceEngineService(fixture.Db, _configuration,
                _memory.Object);
            AppletsSavedService appletsSavedService = new AppletsSavedService(fixture.Db, _configuration);
            CommonService commonService = new CommonService(fixture1.Db, _configuration, httpContext, personService,appletsSavedService,interfaceEngineService);
            FacilityPrivilegeService facilityPrivilegeService = new FacilityPrivilegeService(fixture.Db, httpContext,
                personService);
            InmateService inmateService = new InmateService(fixture1.Db, commonService, personService,
                httpContext, facilityPrivilegeService, photosService);
            HousingConflictService housingConflictService = new HousingConflictService(fixture1.Db,
                inmateService, photosService);
            _housingStatsService = new HousingStatsService(fixture1.Db, photosService);
            LockdownService lockdownService = new LockdownService(fixture1.Db, httpContext);
            _housingService = new HousingService(fixture1.Db, housingConflictService, httpContext,
                photosService, facilityPrivilegeService, lockdownService,interfaceEngineService);
        }

        [Fact]
        public void Housing_GetInmateHousingDetails()
        {
            //Act
            InmateHousingVm inmateHousing = _housingService.GetInmateHousingDetails(140);

            //Assert
            Assert.NotNull(inmateHousing);
            HousingIncarcerationHistory incarcerationHistory = inmateHousing
                .IncarcerationFacilityHistoryList.First(i => i.IncarcerationId == 23);

            Assert.Equal("MCJ", incarcerationHistory.FacilityName);
        }

        [Fact]
        public void Housing_GetFacilityHousingDetails()
        {
            //Act
            HousingVm housing = _housingService.GetFacilityHousingDetails(105, 2);
            //Assert
            Assert.NotNull(housing.HousingCapacityList);
            Assert.Equal("TJ", housing.FacilityAbbr);

        }

        [Fact]
        public void Housing_GetHousingDetails()
        {
            //Arrange
            HousingInputVm housingInputVm = new HousingInputVm
            {
                FacilityId = 1,
                InmateId = 100,
                HousingUnitListId = 9,
                HousingBedNumber = "UPA02",
                HousingBedLocation = "UPPER BED LOC2",
                HousingType = HousingType.BedNumber
            };
            //Act
            HousingVm housing = _housingService.GetHousingDetails(housingInputVm);

            //Assert
            InmateSearchVm inmateSearch = housing.InmateDetailsList.Single(i => i.InmateId == 108);
            Assert.Equal("MADURAI", inmateSearch.Location);
        }

        [Fact]
        public void Housing_GetStatsInmateDetails()
        {
            //Arrange
            HousingStatsInputVm housingStats = new HousingStatsInputVm
            {
                HousingUnit = new HousingDetail
                {
                    HousingUnitBedNumber = "DOWNA01",
                    HousingUnitBedLocation = "DOWN BED LOC1",
                    HousingUnitListId = 11
                },
                HousingStatsCount = new HousingStatsCount
                {
                    FlagId = 2,
                    FlagName = "GANG DROPOUT",
                    Type = "PERSONCAUTION"
                },
                FacilityId = 2
            };

            //Act
            List<HousingStatsDetails> lstHousingstats = _housingStatsService.GetStatsInmateDetails(housingStats);

            //Assert
            HousingStatsDetails housingStatsDetails = lstHousingstats.Single(h => h.InmateId == 107);
            Assert.Equal("CHS0410", housingStatsDetails.InmateNumber);
            Assert.Equal("CHENNAI", housingStatsDetails.Location);
        }

        [Fact]
        public void Housing_GetRecommendHousing()
        {
            //Act
            HousingVm housing = _housingService.GetRecommendHousing(2, 104);

            //Assert
            Assert.NotNull(housing.HousingCapacityList);
            HousingCapacityVm housingCapacity = housing.HousingCapacityList.Single(h => h.HousingUnitListId == 11);
            Assert.Equal("DOWNA01", housingCapacity.HousingBedNumber);
        }

        [Fact]
        public void Housing_GetHousingFacility()
        {
            //Act
            HousingVm housing = _housingService.GetHousingFacility(2);

            //Assert
            HousingPrivilegesVm housingPrivileges = housing.InternalLocationList.First(h => h.PrivilegesId == 8);
            Assert.Equal("COIMBATORE", housingPrivileges.PrivilegesDescription);
        }

        [Fact]
        public void Housing_GetNoHousingDetails()
        {
            //Act
            HousingVm housing = _housingService.GetNoHousingDetails(1);

            //Assert
            InmateSearchVm inmateSearch = housing.InmateDetailsList.Single(i => i.InmateId == 142);
            Assert.Equal("IN3744", inmateSearch.InmateNumber);
        }

        [Fact]
        public void Housing_GetBuildingDetails()
        {
            //Act
            HousingVm housing = _housingService.GetBuildingDetails(2);

            //Assert
            //Checked ExternalLocation
            HousingPrivilegesVm housingPrivileges = housing.ExternalLocationList.Single(e => e.PrivilegesId == 10);
            Assert.Equal("VELLORE", housingPrivileges.PrivilegesDescription);
        }
    }

}

