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
using System.Linq;
using Xunit;

// ReSharper disable once CheckNamespace
namespace ServerAPI.JMS.Tests
{
    [Collection("Database collection")]
    public class InventoryQueueServiceTest
    {
        private readonly InventoryQueueService _inventoryQueueService;
        private readonly IConfigurationRoot _configuration = MockHelpers.GetIConfigurationRoot();
        private readonly Mock<IMemoryCache> _memory = new Mock<IMemoryCache>();

        public InventoryQueueServiceTest(DbInitialize fixture)
        {
            DbInitialize fixture1 = fixture;
            AtimsHubService atimsHubService = new AtimsHubService(fixture.HubContext.Object);
            PersonService personService = new PersonService(fixture1.Db);
            Mock<UserManager<AppUser>> userMgr = MockHelpers.MockUserManager<AppUser>();
            PhotosService photosService = new PhotosService(fixture1.Db, _configuration);
            HttpContextAccessor httpContext = new HttpContextAccessor { HttpContext = fixture.Context.HttpContext };
            FacilityPrivilegeService facilityPrivilegeService = new FacilityPrivilegeService(fixture1.Db, httpContext,
                personService);
            InterfaceEngineService interfaceEngineService = new InterfaceEngineService(fixture.Db, _configuration, _memory.Object);
            AppletsSavedService appletsSavedService = new AppletsSavedService(fixture1.Db, _configuration);
            CommonService commonService = new CommonService(fixture1.Db, _configuration, httpContext, personService, appletsSavedService, interfaceEngineService);
            FacilityHousingService facilityHousingService = new FacilityHousingService(fixture.Db, _configuration, httpContext);
            RecordsCheckService recordsCheckService = new RecordsCheckService(fixture.Db, commonService, httpContext);
            InmateService inmateService = new InmateService(fixture1.Db, commonService, personService, httpContext,
                facilityPrivilegeService, photosService);
            FormsService formsService = new FormsService(fixture1.Db, httpContext, commonService, personService,
                interfaceEngineService);
            InventoryInmateService inventoryinmateservice =
                new InventoryInmateService(fixture1.Db, commonService, httpContext, formsService, _configuration,
                    personService, photosService, facilityHousingService, interfaceEngineService);

            BookingReleaseService bookingReleaseService = new BookingReleaseService(
                fixture.Db, commonService, httpContext, personService, inmateService, recordsCheckService,
                facilityHousingService, interfaceEngineService);
            AltSentService altSentService = new AltSentService(fixture.Db, commonService, httpContext, photosService,
                personService);

            CellService cellService = new CellService(fixture.Db, commonService, httpContext,
                facilityPrivilegeService, facilityHousingService, fixture1.JwtDb, userMgr.Object);
            PrebookWizardService prebookWizardService = new PrebookWizardService(fixture.Db,
                _configuration, commonService, cellService, httpContext, atimsHubService, personService,
                inmateService, bookingReleaseService, altSentService, interfaceEngineService, photosService,
                appletsSavedService);
            WizardService wizardService = new WizardService(fixture.Db, atimsHubService);
            SentenceService sentenceService = new SentenceService(fixture.Db, commonService, httpContext,
                personService, interfaceEngineService);

            PrebookActiveService prebookActiveService = new PrebookActiveService(fixture.Db, httpContext, atimsHubService);
            BookingService bookingService = new BookingService(fixture.Db, commonService, httpContext,
                bookingReleaseService, prebookWizardService, prebookActiveService, sentenceService, wizardService, personService
                , inmateService, photosService, interfaceEngineService);
            BookingTaskService bookingTaskService = new BookingTaskService(fixture1.Db, commonService, httpContext,
                personService, inmateService, bookingService, interfaceEngineService);
            _inventoryQueueService = new InventoryQueueService(fixture.Db, commonService, inventoryinmateservice, 
                bookingTaskService,httpContext,formsService);
        }

        [Fact]
        public void InventoryQueue_GetInventoryInDetails()

        {
            //Act
            InventoryQueueVm inventoryQueue = _inventoryQueueService.GetInventoryQueue(2, 2, 0);
            //Assert
            Assert.True(inventoryQueue.BinFacilityTransferDetails.Count > 0);
            Assert.True(inventoryQueue.BinReceivingDetails.Count > 0);
            Assert.True(inventoryQueue.QueueInProgress.Count > 0);
            Assert.True(inventoryQueue.QueueInRelease.Count > 0);
        }

        [Fact]
        public void InventoryQueue_GetIntakeInmateDetails()
        {
            //Act
            InventoryQueueDetailsVm inventoryQueueDetails =
                _inventoryQueueService.GetInventoryInmateDetails(2, InventoryQueue.Intake, 7, 36, 0);
            //Assert
            InventoryQueueIntakeDetails inventoryQueueIntake =
                inventoryQueueDetails.InventoryQueueIntakeDetails.Single(i => i.InmateId == 101);
            Assert.Equal("PJS001", inventoryQueueIntake.InmateNumber);
            
           

        }

        [Fact]
        public void GetIntakeInmateRecevingBin()
        {
            //Act
            List<InventoryQueueIntakeDetails> lstiInventoryQueueIntakeDetails = _inventoryQueueService
                .GetIntakeInmateRecevingBin();

            //Assert
            Assert.True(lstiInventoryQueueIntakeDetails.Count > 1);

        }
    }
}
