using GenerateTables.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using ServerAPI.Services;
using ServerAPI.Tests;
using ServerAPI.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using System.Linq;
using ServerAPI.JMS.Tests.Helper;
using Moq;
using Microsoft.AspNetCore.Identity;
using JwtDb.Models;
using Microsoft.Extensions.Caching.Memory;

// ReSharper disable once CheckNamespace
namespace ServerAPI.JMS.Tests
{
    [Collection("Database collection")]
    public class FacilityHeadCountServiceTest
    {
        private readonly FacilityHeadCountService _facilityHeadCount;
        private readonly DbInitialize _fixture;
        private readonly IConfigurationRoot _configuration = MockHelpers.GetIConfigurationRoot();
        private readonly Mock<IMemoryCache> _memory = new Mock<IMemoryCache>();
        
        public FacilityHeadCountServiceTest(DbInitialize fixture)
        {
            _fixture = fixture;
            Mock<UserManager<AppUser>> userMgr = MockHelpers.MockUserManager<AppUser>();
            PersonService personService = new PersonService(_fixture.Db);
            PhotosService photosService = new PhotosService(fixture.Db, _configuration);
            InterfaceEngineService interfaceEngineService = new InterfaceEngineService(fixture.Db, _configuration,
                _memory.Object);
            AppletsSavedService appletsSavedService = new AppletsSavedService(fixture.Db, _configuration);
            HttpContextAccessor httpContext = new HttpContextAccessor { HttpContext = fixture.Context.HttpContext };
            AtimsHubService atimsHubService = new AtimsHubService(_fixture.HubContext.Object);
            CommonService commonService = new CommonService(_fixture.Db, _configuration, httpContext, personService,appletsSavedService,interfaceEngineService);
            FacilityPrivilegeService facilityPrivilegeService = new FacilityPrivilegeService(_fixture.Db, httpContext,
                personService);
            FacilityHousingService facilityHousingService = new FacilityHousingService(_fixture.Db, _configuration,
                httpContext);
            CellService cellService = new CellService(fixture.Db, commonService, httpContext,
                facilityPrivilegeService, facilityHousingService, _fixture.JwtDb, userMgr.Object);
            _facilityHeadCount = new FacilityHeadCountService(fixture.Db, commonService, cellService,
                httpContext, atimsHubService, photosService,interfaceEngineService);
        }


        [Fact]
        public void FacilityHeadCount_LoadHeadCountMaster()
        {
            //Arrange
            HeadCountFilter headCountFilter = new HeadCountFilter
            {
                FacilityId = 1
            };
            //Act
            HeadCountViewerDetails headCountViewer = _facilityHeadCount.LoadHeadCountMaster(headCountFilter);
            //Assert
            Assert.NotNull(headCountViewer.HeadCountSchedules);
        }

        [Fact]
        public async Task FacilityHeadCount_InsertOrUpdateCellLog()
        {
            //Arrange
            CellHeadCount cellHeadCount = new CellHeadCount
            {
                CellLogHeadcountId = 11,
                Location = "NEW BOOK - MALE",
                Notes = "LIBRARY BOOKS ONLY",
                CellLogCount = 25,
                CellLogDate = DateTime.Now,
                CellLogTime = "12:20",
                InmateIdList = new List<int>()
            };
            //Act
            CellLog cellLog = _fixture.Db.CellLog.SingleOrDefault(c => c.CellLogHeadcountId == 11);
            Assert.Null(cellLog);
            await _facilityHeadCount.InsertOrUpdateCellLog(cellHeadCount);
            //Assert
            cellLog = _fixture.Db.CellLog.Single(c => c.CellLogHeadcountId == 11);
            Assert.NotNull(cellLog);
        }

        [Fact]
        public void FacilityHeadCount_LoadCellHeadCountMaster()
        {
            //Act
            CellHeadCountDetails cellHeadCount = _facilityHeadCount.LoadCellHeadCountMaster(2, 21);

            //Assert
            CellLogInmateInfo cellLogInmate = cellHeadCount.CellLogInmateDetails.Single(c => c.CellLogInmateId == 200);
            Assert.Equal(105, cellLogInmate.InmateId);
            Assert.Equal("CHS0010", cellLogInmate.Number);
            Assert.NotNull(cellHeadCount.HeadCountDetail);
        }

        [Fact]
        public void FacilityHeadCount_LoadHeadCountSaveHistories()
        {
            //Act
            List<HistoryVm> lstHistory = _facilityHeadCount.LoadHeadCountSaveHistories(22);
            //Assert
            HistoryVm history = lstHistory.Single(h => h.HistoryId == 20);
            Assert.Equal("KRISHNA", history.PersonLastName);
            Assert.Equal("1729", history.OfficerBadgeNumber);
        }

        [Fact]
        public void FacilityHeadCount_LoadHeadCountViewDetail()
        {
            //Act
            HeadCountViewDetail headCountView = _facilityHeadCount.LoadHeadCountViewDetail(12);
            //Assert
            Assert.NotNull(headCountView);

        }

        [Fact]
        public async Task FacilityHeadCount_UpdateHeadCountClear()
        {
            //Assert
            HeadCountViewDetail headCountView = new HeadCountViewDetail
            {
                CellLogHeadCountId = 13,
                Assigned = 160,
                CheckedOut = 20,
                Actual = 140,
                LocationCount = 5,
                Location = 15,
                HousingNote = "FOR HOUSING UNIT",
                LocationNote = "CLEARED"
            };
            CellLogHeadcount cellLogHeadcount = _fixture.Db.CellLogHeadcount.Single(c => c.CellLogHeadcountId == 13);
            Assert.Null(cellLogHeadcount.ClearNote);
            Assert.Null(cellLogHeadcount.ClearLocationNote);

            //Act
            await _facilityHeadCount.UpdateHeadCountClear(headCountView);

            //Assert
            cellLogHeadcount = _fixture.Db.CellLogHeadcount.Single(c => c.CellLogHeadcountId == 13);
            Assert.Equal("CLEARED", cellLogHeadcount.ClearLocationNote);
            Assert.Equal("FOR HOUSING UNIT", cellLogHeadcount.ClearNote);
        }

        [Fact]
        public async Task FacilityHeadCount_InsertCellLogHeadCount()
        {
            //Arrange
            HeadCountStart headCountStart = new HeadCountStart
            {
                //given without seconds
                HeadCountShedule = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, DateTime.Now.Hour, DateTime.Now.Minute, 0),
                FacilityId = 1,
                Unscheduled = true
            };
            //Act
           
            await _facilityHeadCount.InsertCellLogHeadCount(headCountStart);
            //Inserted values in CellLogInmate tables.
        }

        [Fact]
        public void FacilityHeadCount_LoadHeadCountHistory()
        {
            //Arrange
            HeadCountHistoryDetails headCountHistory = new HeadCountHistoryDetails
            {
                FacilityId = 1,
                IsPageInitialize = true
            };
            //Act
            HeadCountHistoryDetails headCountHistoryDetails = _facilityHeadCount.LoadHeadCountHistory(headCountHistory);
            //Assert
            Assert.NotNull(headCountHistoryDetails.LocationList);
            Assert.NotNull(headCountHistoryDetails.FacilityList);
            Assert.NotNull(headCountHistoryDetails.HousingList);
        }

        [Fact]
        public void FacilityHeadCount_LoadAdminHousingLocationCount()
        {
            //Act
            List<CellHousingCount> lstCellHousingCount = _facilityHeadCount.LoadAdminHousingLocationCount(2);

            //Assert
            Assert.NotNull(lstCellHousingCount);
        }

        [Fact]
        public void FacilityHeadCount_LoadHeadCountReport()
        {
            //Arrange
            HeadCountFilter headCountFilter = new HeadCountFilter
            {
                FacilityId = 2
            };

            //Act
            HeadCountReport headCountReport = _facilityHeadCount.LoadHeadCountReport(headCountFilter);

            //Assert
            Assert.Equal("JUNIOR BAIL LEAF MANAGER", headCountReport.HeaderDetails.Officer.AgencyName);
        }

    }
}
