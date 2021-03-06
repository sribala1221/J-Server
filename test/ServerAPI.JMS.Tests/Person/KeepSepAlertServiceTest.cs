﻿using Microsoft.AspNetCore.Http;
using ServerAPI.Services;
using ServerAPI.Tests;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using Xunit;
using ServerAPI.ViewModels;
using System.Linq;
using System.Threading.Tasks;
using GenerateTables.Models;
using ServerAPI.JMS.Tests.Helper;
using Microsoft.AspNetCore.Identity;
using Moq;
using JwtDb.Models;
using Microsoft.Extensions.Caching.Memory;

// ReSharper disable once CheckNamespace
namespace ServerAPI.JMS.Tests
{
    [Collection("Database collection")]
    public class KeepSepAlertServiceTest
    {
        private readonly KeepSepAlertService _keepSepAlertService;
        private readonly DbInitialize _fixture;
        private readonly IConfigurationRoot _configuration = MockHelpers.GetIConfigurationRoot();
        private readonly Mock<IMemoryCache> _memory = new Mock<IMemoryCache>();

        public KeepSepAlertServiceTest(DbInitialize fixture)
        {
            _fixture = fixture;
            Mock<UserManager<AppUser>> userMgr = MockHelpers.MockUserManager<AppUser>();
            PersonService personService = new PersonService(_fixture.Db);
            AppletsSavedService appletsSavedService = new AppletsSavedService(_fixture.Db, _configuration);
            PhotosService photosService = new PhotosService(fixture.Db, _configuration);
            HttpContextAccessor httpContext = new HttpContextAccessor { HttpContext = fixture.Context.HttpContext };
            InterfaceEngineService interfaceEngineService = new InterfaceEngineService(_fixture.Db,
                _configuration, _memory.Object);
            CommonService commonService = new CommonService(_fixture.Db, _configuration,
                httpContext, personService, appletsSavedService, interfaceEngineService);
            FacilityHousingService facilityHousingService = new FacilityHousingService(fixture.Db, _configuration, httpContext);
            _keepSepAlertService = new KeepSepAlertService(fixture.Db, httpContext, facilityHousingService, interfaceEngineService);
        }


        [Fact]
        public void KeepSepAlert_GetKeepSeparateAlertDetails()
        {
            //Act
            KeepSeparateAlertVm keepSeparateAlert = _keepSepAlertService.GetKeepSeparateAlertDetails(106);

            //Assert
            Assert.NotNull(keepSeparateAlert);
            KeepSeparateVm keepSeparateAssocList =
                keepSeparateAlert.KeepSeparateAssocList.Single(k => k.PersonId == 55);
            Assert.Equal("REG.ARSON", keepSeparateAssocList.Assoc);
            Assert.Equal("DEFRAUDED GANG", keepSeparateAssocList.KeepSepReason);
            KeepSeparateVm keepSeparateInmateList =
                keepSeparateAlert.KeepSeparateInmateList.Single(k => k.KeepSeparateId == 10);
            Assert.Equal("TR74414", keepSeparateInmateList.InmateNumber);
        }

        [Fact]
        public void KeepSepAlert_GetKeepSeparateHistory()
        {
            //Act
            List<HistoryVm> lstHistory = _keepSepAlertService.GetKeepSeparateHistory(15, KeepSepType.Association);

            //Assert
            HistoryVm history = lstHistory.Single(h => h.PersonId == 55);
            Assert.Equal("1729", history.OfficerBadgeNumber);
            Assert.Equal("MUKESH", history.Header[0].Detail);
            Assert.Equal("Inmate", history.Header[0].Header);
        }


        [Fact]
        public void KeepSepAlert_GetKeepSepAssocSubsetDetails()
        {
            //Act
            List<KeepSepInmateDetailsVm> lstKeepSepInmateDetails = _keepSepAlertService
                .GetKeepSepAssocSubsetDetails("LOCAL BOYS", null, KeepSepType.Inmate);

            //Assert
            Assert.NotNull(lstKeepSepInmateDetails);
            KeepSepInmateDetailsVm keepSepInmate = lstKeepSepInmateDetails.Single(k => k.HousingUnitId == 11);
            Assert.Equal("UPA02", keepSepInmate.Housing.HousingUnitBedNumber);
            Assert.Equal("SUGIR", keepSepInmate.FirstName);
        }

        [Fact]
        public void KeepSepAlert_GetKeepSepInmateConflictDetails()
        {
            //Act
            List<KeepSepInmateDetailsVm> lstKeepSepInmateConflict =
                _keepSepAlertService.GetKeepSepInmateConflictDetails(125);

            //Assert
            //I-I
            KeepSepInmateDetailsVm keepSepInmateDetails = lstKeepSepInmateConflict.First(k => k.PeronId == 75);
            Assert.Equal("I-I", keepSepInmateDetails.ConflictType);
            Assert.Equal("SASVITHA", keepSepInmateDetails.LastName);

        }

        [Fact]
        public async Task KeepSepAlert_InsertUpdateKeepSepInmateDetails()
        {
            //Arrange
            KeepSeparateVm keepSeparate = new KeepSeparateVm
            {
                KeepSeparateId = 7,
                KeepSepInmateId = 102,
                KeepSepInmate2Id = 135
            };

            //Act
            string message = await _keepSepAlertService.InsertUpdateKeepSepInmateDetails(keepSeparate);
            //Assert
            //Error Message
            Assert.Equal("Keep separate record already exists", message);

            //Insert values into KeepSeparate 
            keepSeparate = new KeepSeparateVm
            {
                KeepSepInmateId = 130,
                KeepSepReason = "ASSUALT",
                KeepSeparateNote = "FIGHT",
                KeepSepType = "CONFIDENTIAL"

            };
            //Before Insertion
            KeepSeparate seperate = _fixture.Db.KeepSeparate.SingleOrDefault(k => k.KeepSeparateInmate1Id == 130);
            Assert.Null(seperate);

            //Act
            await _keepSepAlertService.InsertUpdateKeepSepInmateDetails(keepSeparate);

            //After Insertion
            seperate = _fixture.Db.KeepSeparate.Single(k => k.KeepSeparateInmate1Id == 130);
            Assert.NotNull(seperate);

        }


        [Fact]
        public async Task KeepSepAlert_InsertUpdateKeepSepAssocDetails()
        {
            //Arrange
            KeepSeparateVm keepSeparate = new KeepSeparateVm
            {
                KeepSepType = "DROP OUT",
                KeepSepAssoc = "NEW ROD",
                KeepSepReason = null,
                KeepSeparateNote = "NO ISSUES",
                KeepSepAssocInmateId = 18,
                KeepSepInmateId = 125,
                KeepSepAssoc1Id = 7

            };

            KeepSepAssocInmate keepSepAssoc =
                _fixture.Db.KeepSepAssocInmate.SingleOrDefault(i => i.KeepSepAssoc1 == "NEW ROD");
            Assert.Null(keepSepAssoc);
            //Act
            await _keepSepAlertService.InsertUpdateKeepSepAssocDetails(keepSeparate);

            //Assert
            keepSepAssoc = _fixture.Db.KeepSepAssocInmate.Single(i => i.KeepSepAssoc1 == "NEW ROD");
            Assert.NotNull(keepSepAssoc);
        }

        [Fact]
        public async Task KeepSepAlert_DeleteUndoKeepSeparateDetails()
        {
            //Arrange
            KeepSeparateVm keepSeparate = new KeepSeparateVm
            {
                KeepSepLabel = "INMATE",
                KeepSeparateId = 14,
                KeepSepHistoryList =
                    @"{'Inmate':'GOBI',  'Keep Sep Type':'CONFIDENTIAL', 'Keep Sep Reason':'ENEMY', 'Keep Sep Notes':'CLOSED SECTION' }"
            };
            KeepSeparate separate = _fixture.Db.KeepSeparate.Single(k => k.KeepSeparateId == 14);
            Assert.Equal(1, separate.InactiveFlag);
            //Act
            await _keepSepAlertService.DeleteUndoKeepSeparateDetails(keepSeparate);
            //Assert
            separate = _fixture.Db.KeepSeparate.Single(k => k.KeepSeparateId == 14);
            Assert.Equal(0, separate.InactiveFlag);
        }
    }


}
