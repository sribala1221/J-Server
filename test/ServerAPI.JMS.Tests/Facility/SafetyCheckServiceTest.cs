using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using ServerAPI.Services;
using ServerAPI.Tests;
using System.Collections.Generic;
using Xunit;
using ServerAPI.ViewModels;
using ServerAPI.JMS.Tests.Helper;
using Moq;
using JwtDb.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;

// ReSharper disable once CheckNamespace
namespace ServerAPI.JMS.Tests
{

    [Collection("Database collection")]
    public class SafetyCheckServiceTest
    {
        private readonly SafetyCheckService _safetyCheckService;
        private readonly IConfigurationRoot _configuration = MockHelpers.GetIConfigurationRoot();
        private readonly Mock<IMemoryCache> _memory = new Mock<IMemoryCache>();

        public SafetyCheckServiceTest(DbInitialize fixture)
        {
            DbInitialize fixture1 = fixture;
            PersonService personService = new PersonService(fixture.Db);
            PhotosService photosService = new PhotosService(fixture.Db, _configuration);
            AtimsHubService atimsHubService = new AtimsHubService(fixture.HubContext.Object);
            AppletsSavedService appletsSavedService = new AppletsSavedService(fixture.Db, _configuration);
            Mock<UserManager<AppUser>> userMgr = MockHelpers.MockUserManager<AppUser>();
            HttpContextAccessor httpContext = new HttpContextAccessor { HttpContext = fixture.Context.HttpContext };
            InterfaceEngineService interfaceEngineService = new InterfaceEngineService(fixture1.Db,
                _configuration, _memory.Object);
            CommonService commonService = new CommonService(fixture1.Db, _configuration,
                httpContext, personService,appletsSavedService,interfaceEngineService);
            FacilityPrivilegeService facilityPrivilegeService = new FacilityPrivilegeService(fixture.Db, httpContext,
                personService);
            FacilityHousingService facilityHousingService = new FacilityHousingService(fixture1.Db, _configuration,
                httpContext);
            CellService cellService = new CellService(fixture.Db, commonService, httpContext,
                facilityPrivilegeService, facilityHousingService, fixture1.JwtDb, userMgr.Object);
            _safetyCheckService = new SafetyCheckService(fixture.Db, commonService, cellService, httpContext,
                facilityPrivilegeService, atimsHubService);
        }

        [Fact]
        public void GetSafetyCheckList()
        {
            //Get safetycheck Housing Details
            //Arrange
            ConsoleInputVm consoleInput = new ConsoleInputVm
            {
                ListConsoleLocationId = new List<int>(),
                ListConsoleHousingId = new List<int>
                { 5,6,9},
                HousingUnitListId = 5,
                FacilityId = 1
            };

            //Act
            List<SafetyCheckVm> lstSafetyCheckList = _safetyCheckService.GetSafetyCheckList(consoleInput);
            //Assert
            Assert.True(lstSafetyCheckList.Count > 3);


            //Get safetycheck Location Details
            //Arrange
            consoleInput = new ConsoleInputVm
            {
                ListConsoleLocationId = new List<int>
                    { 12,11,10},
                ListConsoleHousingId = new List<int>(),
                FacilityId = 2,
                ListConsoleMyLocationId = new List<int>()

            };
            //Act
            lstSafetyCheckList = _safetyCheckService.GetSafetyCheckList(consoleInput);
            //Assert
            Assert.True(lstSafetyCheckList.Count > 1);
        }

        [Fact]
        public void LoadSafetyCheckHousingDetails()
        {
            //Act
            SafetyCheckVm safetyCheck = _safetyCheckService.LoadSafetyCheckHousingDetails(1, 11);
            //Arrange
            Assert.Equal("MCJ", safetyCheck.FacilityAbbr);
            Assert.Equal("DOWN-A", safetyCheck.HousingUnitNumber);
            Assert.Equal(11, safetyCheck.HousingUnitListId);

        }


        [Fact]
        public void LoadSafetyCheckLocationDetails()
        {
            //Act
            SafetyCheckVm safetyCheck = _safetyCheckService.LoadSafetyCheckLocationDetails(7);
            //Arrange
            Assert.Equal("TRICHY", safetyCheck.PrivilegesDescription);
        }

        [Fact]
        public void LoadSafetyCheckHistoryList()
        {
            HeadCountHistoryDetails headCountHistory = new HeadCountHistoryDetails
            {
                FacilityId = 1,
                HousingUnitListId = 5,
                IsPageInitialize = true,
                CellLogLocationId = 6
            };
            HeadCountHistoryDetails headCount = _safetyCheckService.LoadSafetyCheckHistoryList(headCountHistory);
            Assert.True(headCount.SafetyCheckHistoryList.Count > 1);
            Assert.True(headCount.LocationList.Count > 3);
        }

        //Development is not complete
        //[Fact]
        //public async Task InsertSafetyCheck()
        //{
        //    SafetyCheckBedVm safetyCheckBed = new SafetyCheckBedVm
        //    {
        //        HousingBedNumber = "",
        //        BedChecked = true,
        //        BedRequiredNotes = "NOT BOOKED"

        //    };
        //    SafetyCheckVm safetyCheck = new SafetyCheckVm
        //    {
        //        FacilityId = 1,
        //        Mode = "Add",
        //        HousingUnitListId = 12,
        //        PrivilegesId = 6,
        //        PrivilegesDescription = "MADURAI",
        //        BedListItems = new List<SafetyCheckBedVm>
        //        {
        //          safetyCheckBed

        //        },
        //    };
        //    await _safetyCheckService.InsertSafetyCheck(safetyCheck);

        //}

    }
}
