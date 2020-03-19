﻿using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using ServerAPI.Services;
using ServerAPI.Tests;
using System;
using Xunit;
using ServerAPI.ViewModels;
using System.Threading.Tasks;
using GenerateTables.Models;
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
    public class OperationViewerServiceTest
    {
        private readonly OperationViewerService _operationViewerService;
        private readonly DbInitialize _fixture;
        private readonly JwtDbContext _jwtDbContext;
        private readonly IConfigurationRoot _configuration = MockHelpers.GetIConfigurationRoot();
        private readonly Mock<IMemoryCache> _memory = new Mock<IMemoryCache>();
        public OperationViewerServiceTest(DbInitialize fixture)
        {
            _fixture = fixture;
            Mock<JwtDbContext> jwtDbContext = new Mock<JwtDbContext>();
            Mock<UserManager<AppUser>> userMgr = MockHelpers.MockUserManager<AppUser>();
            PersonService personService = new PersonService(_fixture.Db);
            AppletsSavedService appletsSavedService = new AppletsSavedService(fixture.Db, _configuration);
            PhotosService photosService = new PhotosService(fixture.Db, _configuration);
            InterfaceEngineService interfaceEngineService = new InterfaceEngineService(_fixture.Db,
                _configuration, _memory.Object);

            HttpContextAccessor httpContext = new HttpContextAccessor { HttpContext = fixture.Context.HttpContext };
<<<<<<< .mine
            CommonService commonService = new CommonService(_fixture.Db, _configuration,httpContext,personService,appletsSavedService,interfaceEngineService);
            // _operationViewerService = new OperationViewerService(fixture.Db, commonService, httpContext,personService);
||||||| .r13927
            CommonService commonService = new CommonService(_fixture.Db, _configuration,httpContext,personService,appletsSavedService,interfaceEngineService);
            _operationViewerService = new OperationViewerService(fixture.Db, commonService, httpContext,personService);
=======
            CommonService commonService = new CommonService(_fixture.Db, _configuration, httpContext, personService, appletsSavedService, interfaceEngineService);
            _operationViewerService = new OperationViewerService(fixture.Db, commonService, httpContext, personService,jwtDbContext.Object,userMgr.Object);
>>>>>>> .r13948
        }

<<<<<<< .mine
        [Fact]
        public void OperationViewer_GetFacilityViewerDetails()
        {
            //Act
            // FacilityViewerVm facilityViewer = _operationViewerService.GetFacilityViewerInit(1, false);
||||||| .r13927
        [Fact]
        public void OperationViewer_GetFacilityViewerDetails()
        {
            //Act
            FacilityViewerVm facilityViewer = _operationViewerService.GetFacilityViewerInit(1, false);
=======
        // Due to JwtDbContext
>>>>>>> .r13948

<<<<<<< .mine
            //Assert
            // Assert.NotNull(facilityViewer.InmateNoteType);
            // Assert.NotNull(facilityViewer.HousingUnitLoc);
        }
||||||| .r13927
            //Assert
            Assert.NotNull(facilityViewer.InmateNoteType);
            Assert.NotNull(facilityViewer.HousingUnitLoc);
        }
=======
        //[Fact]
        //public async Task OperationViewer_GetFacilityViewerDetails()
        //{
        //    //Act
        //    await _operationViewerService.GetFacilityViewerInit(1, false);
>>>>>>> .r13948

<<<<<<< .mine
        [Fact]
        public void OperationViewer_GetFacilityViewer()
        {
            //Arrange
            ViewerParameter viewerParameter = new ViewerParameter
            {
                FacilityId = 2,
                FromDate = DateTime.Now,
                InmateId = 101,
                ToDate = DateTime.Now,
                HousingUnitLocation = "FLOOR2",
                AttendanceId = 10,
                FilterSetting = new ViewerFilter
                {
                    CellLog = true,
                    HousingIn = true,
                    SafetyCheck = true,
                    GeneralNote = false
                }
            };
            //Act
            // FacilityViewerVm facilityViewer = _operationViewerService.GetFacilityViewerRefresh(viewerParameter);
||||||| .r13927
        [Fact]
        public void OperationViewer_GetFacilityViewer()
        {
            //Arrange
            ViewerParameter viewerParameter = new ViewerParameter
            {
                FacilityId = 2,
                FromDate = DateTime.Now,
                InmateId = 101,
                ToDate = DateTime.Now,
                HousingUnitLocation = "FLOOR2",
                AttendanceId = 10,
                FilterSetting = new ViewerFilter
                {
                    CellLog = true,
                    HousingIn = true,
                    SafetyCheck = true,
                    GeneralNote = false
                }
            };
            //Act
            FacilityViewerVm facilityViewer = _operationViewerService.GetFacilityViewerRefresh(viewerParameter);
=======
        //    //Assert
        //    //Assert.NotNull(facilityViewer.InmateNoteType);
        //    //Assert.NotNull(facilityViewer.HousingUnitLoc);
        //}
>>>>>>> .r13948

<<<<<<< .mine
            // //Assert
            // Assert.NotNull(facilityViewer.SafetyChkHousing);
            // Assert.NotNull(facilityViewer.CellType);
            // Assert.NotNull(facilityViewer.LocationNoteType1);
        }
||||||| .r13927
            //Assert
            Assert.NotNull(facilityViewer.SafetyChkHousing);
            Assert.NotNull(facilityViewer.CellType);
            Assert.NotNull(facilityViewer.LocationNoteType1);
        }
=======
        //[Fact]
        //public async Task OperationViewer_GetFacilityViewer()
        //{
        //    //Arrange
        //    ViewerParameter viewerParameter = new ViewerParameter
        //    {
        //        FacilityId = 2,
        //        FromDate = DateTime.Now,
        //        InmateId = 101,
        //        ToDate = DateTime.Now,
        //        HousingUnitLocation = "FLOOR2",
        //        AttendanceId = 10,
        //        FilterSetting = new ViewerFilter
        //        {
        //            CellLog = true,
        //            HousingIn = true,
        //            SafetyCheck = true,
        //            GeneralNote = false
        //        }
        //    };
        //    //Act
        //    await _operationViewerService.GetFacilityViewerRefresh(viewerParameter);
>>>>>>> .r13948

        //    //Assert
        //    //Assert.NotNull(facilityViewer.SafetyChkHousing);
        //    //Assert.NotNull(facilityViewer.CellType);
        //    //Assert.NotNull(facilityViewer.LocationNoteType1);
        //}

        //[Fact]
        //public async Task OperationViewer_OperationDelete()
        //{
        //    //Arrange
        //    DeleteParams deleteParams = new DeleteParams
        //    {
        //        Type = "LOCATION NOTE",
        //        Id = 27,
        //        DeleteFlag = 1,
        //        InmateId = 140
        //    };
        //    FloorNotes floorNotes = _fixture.Db.FloorNotes.Single(f => f.FloorNoteId == 27);
        //    Assert.Equal(0, floorNotes.FloorDeleteFlag);
        //    //Act
        //    await _operationViewerService.OperationDelete(deleteParams);
        //    //Assert
        //    floorNotes = _fixture.Db.FloorNotes.Single(f => f.FloorNoteId == 27);
        //    Assert.Equal(1, floorNotes.FloorDeleteFlag);
        //}

        //[Fact]
        //public async Task OperationViewer_UpdateUserSettings()
        //{
        //    //Arrange
        //    ViewerFilter viewerFilter = new ViewerFilter
        //    {
        //        CellLog = true,
        //        HousingIn = true,
        //        ClockOut = true,
        //        SafetyCheck = false,
        //        InmateNote = true,
        //        HeadCount = false,
        //        GeneralNote = true,
        //        SetLocation = true
        //    };

        //    UserAccess userAccess = _fixture.Db.UserAccess.Single(u => u.UserId == 5);
        //    Assert.Null(userAccess.DefaultCheckboxMyLogSetLocation);
        //    Assert.Null(userAccess.DefaultCheckboxViewerGeneralNote);
        //    //Act
        //    await _operationViewerService.UpdateUserSettings(viewerFilter);
        //    //Assert
        //    userAccess = _fixture.Db.UserAccess.Single(u => u.UserId == 5);
        //    Assert.Equal(1, userAccess.DefaultCheckboxMyLogSetLocation);
        //    Assert.Equal(1, userAccess.DefaultCheckboxViewerGeneralNote);
        //}

    }


}
