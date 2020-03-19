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
    public class HeadCountServiceTest
    {
        private readonly HeadCountService _headCountService;
        private readonly IConfigurationRoot _configuration = MockHelpers.GetIConfigurationRoot();
        private readonly Mock<IMemoryCache> _memory = new Mock<IMemoryCache>();

        public HeadCountServiceTest(DbInitialize fixture)
        {
            DbInitialize fixture1 = fixture;
            Mock<UserManager<AppUser>> userMgr = MockHelpers.MockUserManager<AppUser>();
            PersonService personService = new PersonService(fixture1.Db);
            PhotosService photosService = new PhotosService(fixture.Db, _configuration);
            AppletsSavedService appletsSavedService = new AppletsSavedService(fixture.Db, _configuration);
            HttpContextAccessor httpContext = new HttpContextAccessor { HttpContext = fixture.Context.HttpContext };
            FacilityHousingService facilityHousingService = new FacilityHousingService(fixture1.Db,
                _configuration, httpContext);
            InterfaceEngineService interfaceEngineService = new InterfaceEngineService(fixture1.Db,
                _configuration, _memory.Object);
            CommonService commonService = new CommonService(fixture1.Db, _configuration, httpContext, personService, appletsSavedService,interfaceEngineService);
            FacilityPrivilegeService facilityPrivilegeService = new FacilityPrivilegeService(fixture1.Db, httpContext,
                personService);
            CellService cellService = new CellService(fixture1.Db, commonService, httpContext,
                facilityPrivilegeService, facilityHousingService, fixture1.JwtDb, userMgr.Object);

            _headCountService = new HeadCountService(fixture1.Db, commonService, cellService, httpContext);
        }

        [Fact]
        public void GetHeadCountList()
        {
            //Arrange
            ConsoleInputVm consoleInput = new ConsoleInputVm
            {
                FacilityId = 1,
                ListConsoleHousingId = new List<int>
                {11,12},
                HousingUnitListId = 12
            };
            //Act
            List<HeadCountVm> lstHeadCountList = _headCountService.GetHeadCountList(consoleInput);
            //Assert
            Assert.True(lstHeadCountList.Count > 0);


            //Arrange
            consoleInput = new ConsoleInputVm
            {
                FacilityId = 2,
                ListConsoleHousingId = new List<int>(),
                ListConsoleLocationId = new List<int>

                    { 7,8,9,10 },
                ListConsoleMyLocationId = new List<int>
                    { 5,6},
                HousingUnitListId = 11
            };
            //Act
            lstHeadCountList = _headCountService.GetHeadCountList(consoleInput);
            //Assert
            Assert.True(lstHeadCountList.Count > 2);
        }
    }
}
