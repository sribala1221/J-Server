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
using System.Collections.Generic;
using Xunit;

// ReSharper disable once CheckNamespace
namespace ServerAPI.JMS.Tests
{
    [Collection("Database collection")]
    public class HousingConflictServiceTest
    {
        private readonly HousingConflictService _housingConflictService;
        private readonly IConfigurationRoot _configuration = MockHelpers.GetIConfigurationRoot();
        private readonly Mock<IMemoryCache> _memory = new Mock<IMemoryCache>();
        public HousingConflictServiceTest(DbInitialize fixture)
        {
            DbInitialize fixture1 = fixture;
            Mock<UserManager<AppUser>> userMgr = MockHelpers.MockUserManager<AppUser>();
            PersonService personService = new PersonService(fixture1.Db);
            InterfaceEngineService interfaceEngineService = new InterfaceEngineService(fixture1.Db,
                _configuration, _memory.Object);
            HttpContextAccessor httpContext = new HttpContextAccessor { HttpContext = fixture.Context.HttpContext };
            PhotosService photosService = new PhotosService(fixture1.Db, _configuration);
            AppletsSavedService appletsSavedService = new AppletsSavedService(fixture.Db, _configuration);
            CommonService commonService = new CommonService(fixture1.Db, _configuration, httpContext,personService,appletsSavedService,interfaceEngineService);
            FacilityPrivilegeService facilityPrivilegeService = new FacilityPrivilegeService(fixture.Db, httpContext,
                personService);
                
            InmateService inmateService = new InmateService(fixture1.Db, commonService, personService, 
                httpContext,facilityPrivilegeService,photosService);
            _housingConflictService = new HousingConflictService(fixture1.Db,inmateService,photosService);
        }

        [Fact]
        public void HousingConflic_GetHousingKeepSeperate()
        {
            //Act
            List<HousingConflictVm> lstHousingUnit = _housingConflictService.GetHousingKeepSeparate(102, 2);
            //Assert
            Assert.NotNull(lstHousingUnit);
        }


        [Fact]
        public void GetInmateHousingConflictVm()
        {
            //Arrange
            HousingInputVm housingInput = new HousingInputVm
            {
                FacilityId = 2,
                HousingBedNumber = "UPA02",
                HousingUnitListId = 9,
                InmateId = 100
            };
            //Act
            List<HousingConflictVm> housingConflict = _housingConflictService.GetInmateHousingConflictVm(housingInput);
            //Assert
            Assert.NotNull(housingConflict);
        }

    }
}
