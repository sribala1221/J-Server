using System;
using System.Linq;
using System.Threading.Tasks;
using GenerateTables.Models;
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
using Xunit;

// ReSharper disable once CheckNamespace
namespace ServerAPI.JMS.Tests
{
    [Collection("Database collection")]
    public class SupervisorServiceTest
    {
        private readonly SupervisorService _supervisorService;
        private readonly IConfigurationRoot _configuration = MockHelpers.GetIConfigurationRoot();
        private readonly DbInitialize _fixture;
        private readonly Mock<IMemoryCache> _memory = new Mock<IMemoryCache>();

        public SupervisorServiceTest(DbInitialize fixture)
        {
            _fixture = fixture;
            Mock<UserManager<AppUser>> userMgr = MockHelpers.MockUserManager<AppUser>();
            Mock<RoleManager<IdentityRole>> roleMgr = MockHelpers.MockRoleManager<IdentityRole>();
            HttpContextAccessor httpContext = new HttpContextAccessor
            { HttpContext = _fixture.Context.HttpContext };

            AppletsSavedService appletsSavedService = new AppletsSavedService(fixture.Db, _configuration);

            AtimsHubService atimsHubService = new AtimsHubService(_fixture.HubContext.Object);
            PhotosService photosService = new PhotosService(_fixture.Db, _configuration);
            PersonService personService = new PersonService(_fixture.Db);
            InterfaceEngineService interfaceEngineService = new InterfaceEngineService(fixture.Db,
                _configuration, _memory.Object);
            CommonService commonService = new CommonService(_fixture.Db, _configuration,
                httpContext, personService,appletsSavedService,interfaceEngineService);
            FacilityPrivilegeService facilityPrivilegeService = new FacilityPrivilegeService(fixture.Db,
                httpContext, personService);
            InmateService inmateService = new InmateService(_fixture.Db, commonService, personService, httpContext,
                facilityPrivilegeService, photosService);
            FacilityHousingService facilityHousingService = new FacilityHousingService(_fixture.Db,
                _configuration, httpContext);
            RecordsCheckService recordsCheckService = new RecordsCheckService(_fixture.Db, commonService, httpContext);
           
            BookingReleaseService bookingReleaseService = new BookingReleaseService(
                _fixture.Db, commonService, httpContext, personService, inmateService, recordsCheckService,
                facilityHousingService,interfaceEngineService);
            AltSentService altSentService = new AltSentService(_fixture.Db, commonService, httpContext, photosService, 
                personService);
            CellService cellService = new CellService(fixture.Db, commonService, httpContext,
                facilityPrivilegeService, facilityHousingService, _fixture.JwtDb, userMgr.Object);
            PrebookWizardService prebookWizardService = new PrebookWizardService(_fixture.Db,
                _configuration, commonService, cellService, httpContext, atimsHubService,
                personService, inmateService, bookingReleaseService, altSentService,interfaceEngineService,
                photosService,appletsSavedService);
            WizardService wizardService = new WizardService(_fixture.Db,atimsHubService);
            SentenceService sentenceService = new SentenceService(_fixture.Db, commonService, httpContext,
                personService,interfaceEngineService);
            PrebookActiveService prebookActiveService = new PrebookActiveService(_fixture.Db, httpContext,
                atimsHubService);
            BookingService bookingService = new BookingService(_fixture.Db, commonService, httpContext,
                bookingReleaseService, prebookWizardService, prebookActiveService, sentenceService, wizardService,
                personService, inmateService, photosService, interfaceEngineService);
            IncidentService incidentService = new IncidentService(_fixture.Db, wizardService, commonService, 
                bookingService,httpContext, photosService);

            RequestService requestService = new RequestService(_fixture.Db, httpContext, userMgr.Object, roleMgr.Object, 
                personService, inmateService,facilityHousingService, atimsHubService);
            _supervisorService = new SupervisorService(_fixture.Db, personService, httpContext, requestService,
                incidentService);
        }

        [Fact]
        public void GetBookingSupervisor()
        {
            //Act
            BookingSupervisorVm bookingSupervisor = _supervisorService.GetBookingSupervisor(1);

            //Assert
            Assert.Equal(1,bookingSupervisor.RecordsCheckCount);
        }


        [Fact]
        public void CompleteForceCharge()
        {
            //Arrange
            PrebookCharge prebookCharge = new PrebookCharge
            {
                ArrestId = 10,
                CrimeForceId = 8,
                CreateDate = DateTime.Now,
                CrimeSection = "10250.52",
                CrimeDescription = "PROPERTY FRAUD",
                CrimeGroupId = 25
            };

            CrimeLookup crimeLookup = _fixture.Db.CrimeLookup.SingleOrDefault(c => c.CrimeGroupId == 25);
            Assert.Null(crimeLookup);

            //Act
            int result = _supervisorService.CompleteForceCharge(prebookCharge, 1);

            //Assert
            Assert.Equal(1, result);
            crimeLookup = _fixture.Db.CrimeLookup.SingleOrDefault(c => c.CrimeGroupId == 25);
            Assert.NotNull(crimeLookup);
            Assert.Equal("PROPERTY FRAUD", crimeLookup.CrimeDescription);
        }


        [Fact]
        public async Task UpdateReviewComplete()
        {
            //Arrange
            BookingComplete bookingComplete = new BookingComplete
            {
                InmateId = 108,
                IncarcerationId = 15,
                IsComplete = true
            };
            //Before Update
            Incarceration incarceration = _fixture.Db.Incarceration.Single(i => i.IncarcerationId == 15);
            Assert.Null(incarceration.BookingSupervisorCompleteFlag);

            //Act
            await _supervisorService.UpdateReviewComplete(bookingComplete);

            //Assert
            //After Update
            incarceration = _fixture.Db.Incarceration.Single(i => i.IncarcerationId == 15);
            Assert.Equal(1, incarceration.BookingSupervisorCompleteFlag);
        }

    }
}
