using GenerateTables.Models;
using JwtDb.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Moq;
using ServerAPI.JMS.Tests.Helper;
using ServerAPI.Policies;
using ServerAPI.Services;
using ServerAPI.Tests;
using ServerAPI.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

// ReSharper disable once CheckNamespace
namespace ServerAPI.JMS.Tests
{
    [Collection("Database collection")]
    public class TransferServiceTest
    {
        private readonly TransferService _transferService;
        private readonly IConfigurationRoot _configuration = MockHelpers.GetIConfigurationRoot();
        private readonly DbInitialize _fixture;
        private readonly Mock<IMemoryCache> _memory = new Mock<IMemoryCache>();

        public TransferServiceTest(DbInitialize fixture)
        {
            _fixture = fixture;
            PersonService personService = new PersonService(_fixture.Db);
            Mock<UserManager<AppUser>> userMgr = MockHelpers.MockUserManager<AppUser>();
            AtimsHubService atimsHubService = new AtimsHubService(fixture.HubContext.Object);
            InterfaceEngineService interfaceEngineService = new InterfaceEngineService(fixture.Db, _configuration,
                _memory.Object);
            AppletsSavedService appletsSavedService = new AppletsSavedService(_fixture.Db, _configuration);
            HttpContextAccessor httpContext = new HttpContextAccessor { HttpContext = _fixture.Context.HttpContext };
            Mock<RoleManager<IdentityRole>> roleMgr = MockHelpers.MockRoleManager<IdentityRole>();
            UserPermissionPolicy userPermission =
                new UserPermissionPolicy(_fixture.Db, userMgr.Object, roleMgr.Object, httpContext);
            PhotosService photosService = new PhotosService(_fixture.Db, _configuration);
            CommonService commonService = new CommonService(_fixture.Db, _configuration, httpContext, personService
                ,appletsSavedService,interfaceEngineService);
            FacilityHousingService facilityHousingService = new FacilityHousingService(_fixture.Db, _configuration,
                httpContext);
            KeepSepAlertService keepSepAlertService = new KeepSepAlertService(_fixture.Db, httpContext,
                facilityHousingService,interfaceEngineService);
            WizardService wizardService = new WizardService(fixture.Db,atimsHubService);
            PersonAkaService personAkaService = new PersonAkaService(fixture.Db, commonService, httpContext, personService,
                interfaceEngineService);
            PrebookActiveService prebookActiveService = new PrebookActiveService(fixture.Db, httpContext, atimsHubService);
            PersonCharService personCharService = new PersonCharService(fixture.Db, commonService, httpContext, personService,
                interfaceEngineService);
            PersonIdentityService personIdentityService = new PersonIdentityService(fixture.Db,
                httpContext, prebookActiveService, personAkaService, personService,
                interfaceEngineService);
            AppointmentViewerService appointmentViewerService =
                new AppointmentViewerService(fixture.Db, commonService, photosService);
            RegisterService registerService = new RegisterService(fixture.Db, httpContext, commonService, personCharService,
                photosService, wizardService, personIdentityService, keepSepAlertService, userPermission);
            AppointmentService appointmentService = new AppointmentService(fixture.Db, commonService, registerService,
                httpContext, personService, keepSepAlertService,interfaceEngineService,appointmentViewerService);
            FacilityPrivilegeService facilityPrivilegeService = new FacilityPrivilegeService(_fixture.Db, httpContext,
                personService);
            InmateService inmateService = new InmateService(_fixture.Db, commonService, personService, httpContext,
                facilityPrivilegeService, photosService);
            HousingConflictService housingConflictService = new HousingConflictService(_fixture.Db,
                inmateService, photosService);
            LockdownService lockdownService = new LockdownService(_fixture.Db, httpContext);
            HousingService housingService = new HousingService(fixture.Db, housingConflictService,
                httpContext, photosService, facilityPrivilegeService, lockdownService, interfaceEngineService);
            _transferService = new TransferService(_fixture.Db, httpContext, commonService, photosService,
                appointmentService, inmateService, housingService, keepSepAlertService, interfaceEngineService);
        }

        #region Transfer Eligibles
        [Fact]
        public void GetTransferEligibles()
        {
            //Arrange
            //Without filter
            EligibleSearchVm eligibleSearch = new EligibleSearchVm
            {
                IsFlag = true,
                FacilityId = 1
            };

            //Act
            TransEligibleDetailsVm transEligibleDetails = _transferService.GetTransferEligibles(eligibleSearch);

            //Assert
            Assert.True(transEligibleDetails.TransEligibleDetails.Count > 0);
            Assert.True(transEligibleDetails.LstHousingDetails.Count > 2);


            //Arrange
            //With filter
            eligibleSearch = new EligibleSearchVm
            {
                IsFlag = true,
                FacilityId = 2,
                InmateId = 105,
                PersonnelId = 12
            };

            //Act
            transEligibleDetails = _transferService.GetTransferEligibles(eligibleSearch);

            //Assert
            Assert.True(transEligibleDetails.LstHousingDetails.Count > 0);
            Assert.True(transEligibleDetails.LstLocation.Count > 1);
        }

        [Fact]
        public void UpdateIncarceration()
        {
            //Arrange
            TransferEligibleVm transferEligible = new TransferEligibleVm
            {
                IncarcerationId = 28,
                EligibleLookup = 1,
                EligibleNote = "SUPPLY ITEM",
                ApprovalLookup = 0,
                EligibleDate = DateTime.Now.Date,
                ApprovalDate = DateTime.Now
            };

            //Before Update
            Incarceration incarceration = _fixture.Db.Incarceration.Single(i => i.IncarcerationId == 28);
            Assert.Null(incarceration.TransferEligibleNote);

            //Before Insert 
            IncarcerationTransferSaveHistory history = _fixture.Db.IncarcerationTransferSaveHistory.SingleOrDefault(i =>
                i.Incarcerationid == 28);
            Assert.Null(history);

            //Act
            _transferService.UpdateIncarceration(transferEligible);

            //Assert
            //After Update
            incarceration = _fixture.Db.Incarceration.Single(i => i.IncarcerationId == 28);
            Assert.NotNull(incarceration.TransferEligibleNote);

            //After Insert
            history = _fixture.Db.IncarcerationTransferSaveHistory.SingleOrDefault(i =>
                i.Incarcerationid == 28);
            Assert.NotNull(history);
        }

        [Fact]
        public void GetTransferHistoryDetails()
        {
            //Act
            List<TransferHistoryVm> lsTransferHistory = _transferService.GetTransferHistoryDetails(15);

            //Assert
            Assert.True(lsTransferHistory.Count > 1);
        }

        [Fact]
        public void GetInmateCount()
        {
            //Act
            EligibleInmateCountVm eligibleInmate = _transferService.GetInmateCount(105, 60);

            //Assert
            Assert.True(eligibleInmate.Counts.Count > 5);

        }

        #endregion

        #region External Transfer

        [Fact]
        public void GetInternalTransfer()
        {
            //Act
            ExternalTransferVm externalTransfer = _transferService.GetLocationCountDetails(2, DateTime.Now);

            //Assert
            Assert.True(externalTransfer.LocationDetails.Count > 0);
        }


        //due to appointment not implemented

        //[Fact]
        //public void GetLocationInmateDetails()
        //{
        //    //Act
        //    _transferService.GetLocationInmateDetails(7, 2, false, DateTime.Now);

        //}

        [Fact]
        public void GetExternalLocations()
        {
            //Act
            List<KeyValuePair<string, int>> lstKeyValuePairs = _transferService.GetExternalLocations();

            //Assert
            Assert.True(lstKeyValuePairs.Count > 0);

        }

        [Fact]
        public void GetInventoryBinList()
        {
            //Act
            List<KeyValuePair<string, int>> lstKeyValuePairs = _transferService.GetInventoryBinList();

            //Assert
            Assert.True(lstKeyValuePairs.Count > 0);

        }

        [Fact]
        public async Task IssuedProperty()
        {
            //Arrange
            List<ExternalSearchVm> externalSearch = new List<ExternalSearchVm>
            {
                new ExternalSearchVm
                {
                    InmateId = 142,
                    BinId = 6,
                    BinNumber = "BIN002"
                }
            };

            //Before Update
            PersonalInventory personalInventory = _fixture.Db.PersonalInventory.Single(p => p.PersonalInventoryId == 10);
            Assert.Null(personalInventory.InventoryBinNumber);

            //Act
            await _transferService.IssuedProperty(externalSearch, 2);

            //Assert
            //After Update
            personalInventory = _fixture.Db.PersonalInventory.Single(p => p.PersonalInventoryId == 10);
            Assert.NotNull(personalInventory.InventoryBinNumber);

        }



        [Fact]
        public async Task UpdateCheckInLibBook()
        {
            //Arrange
            List<int> inmateIds = new List<int>
                { 110};

            LibraryBook libraryBook = _fixture.Db.LibraryBook.Single(l => l.CurrentCheckoutInmateId == 110);
            Assert.NotNull(libraryBook);

            //Act
            await _transferService.UpdateCheckInLibBook(inmateIds);

            //Assert
            libraryBook = _fixture.Db.LibraryBook.SingleOrDefault(l => l.CurrentCheckoutInmateId == 110);
            Assert.Null(libraryBook);

        }

        [Fact]
        public void GetCounts()
        {
            //Act
            List<LibraryBook> lstBooks = _transferService.GetCounts();

            //Assert
            Assert.True(lstBooks.Count > 1);
        }



        #endregion



    }
}
