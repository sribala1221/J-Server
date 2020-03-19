using ServerAPI.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Microsoft.AspNetCore.Http;
using ServerAPI.ViewModels;
using GenerateTables.Models;
using ServerAPI.Tests;
using Moq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Caching.Memory;
using ServerAPI.JMS.Tests.Helper;
using Microsoft.AspNetCore.Identity;
using JwtDb.Models;

namespace ServerAPI.JMS.Tests.Inmate
{
    [Collection("Database collection")]
    public class InmateIncidentServiceTest
    {
        private readonly DbInitialize _fixture;
        private readonly InmateIncidentService _inmateIncidentService;
        private readonly IConfigurationRoot _configuration = MockHelpers.GetIConfigurationRoot();
        private readonly Mock<IMemoryCache> _memory = new Mock<IMemoryCache>();

        public InmateIncidentServiceTest(DbInitialize fixture)
        {
            _fixture = fixture;
            HttpContextAccessor httpContext = new HttpContextAccessor { HttpContext = fixture.Context.HttpContext };
            AppletsSavedService appletsSavedService = new AppletsSavedService(fixture.Db, _configuration);
            AtimsHubService atimsHubService = new AtimsHubService(_fixture.HubContext.Object);
            PersonService personService = new PersonService(_fixture.Db);
            PhotosService photosService = new PhotosService(fixture.Db, _configuration);
            Mock<UserManager<AppUser>> userMgr = MockHelpers.MockUserManager<AppUser>();
            InterfaceEngineService interfaceEngineService = new InterfaceEngineService(fixture.Db, _configuration,
                _memory.Object);
            CommonService commonService = new CommonService(fixture.Db, _configuration, httpContext, personService,appletsSavedService,interfaceEngineService);
            FacilityPrivilegeService facilityPrivilegeService = new FacilityPrivilegeService(_fixture.Db, httpContext,
                personService);
           

            InmateService inmateService = new InmateService(_fixture.Db, commonService, personService, httpContext,
                facilityPrivilegeService, photosService);
            FacilityHousingService facilityHousingService = new FacilityHousingService(fixture.Db, _configuration, httpContext);
            RecordsCheckService recordsCheckService = new RecordsCheckService(fixture.Db, commonService, httpContext);
            BookingReleaseService bookingReleaseService =
               new BookingReleaseService(_fixture.Db, commonService, httpContext, personService, inmateService,
                   recordsCheckService, facilityHousingService, interfaceEngineService);
            CellService cellService = new CellService(fixture.Db, commonService, httpContext,
                facilityPrivilegeService, facilityHousingService, _fixture.JwtDb, userMgr.Object);
            PrebookActiveService prebookActiveService = new PrebookActiveService(_fixture.Db, httpContext, atimsHubService);
            AltSentService altSentService = new AltSentService(_fixture.Db, commonService, httpContext, photosService,
                personService);

            SentenceService sentenceService = new SentenceService(fixture.Db, commonService, httpContext, personService,
                interfaceEngineService);
            PrebookWizardService prebookWizardService = new PrebookWizardService(_fixture.Db, _configuration,
                commonService, cellService, httpContext, atimsHubService, personService, inmateService, bookingReleaseService,
                altSentService, interfaceEngineService,photosService,appletsSavedService);

            WizardService wizardService = new WizardService(_fixture.Db,atimsHubService);

            BookingService bookingService = new BookingService(
            _fixture.Db, commonService, httpContext, bookingReleaseService, prebookWizardService,
            prebookActiveService, sentenceService, wizardService, personService, inmateService, photosService,
            interfaceEngineService);

            IncidentService incidentService = new IncidentService(_fixture.Db, wizardService, commonService,
                bookingService, httpContext, photosService);

            _inmateIncidentService = new InmateIncidentService(fixture.Db, commonService, httpContext, incidentService,
                interfaceEngineService);
        }

        [Fact]
        public void InmateIncident_GetInmateIncidentList()
        {
            //Act
            List<IncidentViewer> inmateIncident = _inmateIncidentService.GetInmateIncidentList(101);
            //Assert
            InmateIncidentInfo incidentInfo =
                inmateIncident.Single(i => i.DispInmateId == 103);
            Assert.Equal("UP-A", incidentInfo.DispHousingUnitNumber);
            Assert.Equal("DIS_100102", incidentInfo.IncidentNumber);
        }

        [Fact]
        public void InmateIncident_GetInmateIncidentDropdownList()
        {
            //Act
            InmateIncidentDropdownList inmateIncident = _inmateIncidentService.GetInmateIncidentDropdownList(true,1);
            //Assert
            KeyValuePair<int, string> listLocation = inmateIncident.IncidentHousingLocationList
                .Single(h => h.Key == 5);
            Assert.Equal("CHENNAI", listLocation.Value);
        }

        [Fact]
        public async Task InmateIncident_InsertInmateIncident()
        {
            //Arrange
            InmateIncidentInfo incidentInfo = new InmateIncidentInfo
            {
                DispIncidentFlag = new[]
                {
                    "FORCE USED", "OFFICER INJURY"
                },
                DisciplinaryTypeId = 20,
                ReportDate = DateTime.Now,
                FacilityId = 1,
                DispLocationId = 7,
                IncidentDate = DateTime.Now.AddDays(-3),
                DispHousingUnitBedNumber = "DOWN001",
                DispHousingUnitNumber = "DOWN-B",
                DispHousingUnitLocation = "FLOOR2"
            };
            DisciplinaryIncident disciplinaryIncident =
                _fixture.Db.DisciplinaryIncident.SingleOrDefault(d => d.DisciplinaryNumber == "IN201NUM");
            Assert.Null(disciplinaryIncident);

            //Act
            await _inmateIncidentService.InsertInmateIncident(incidentInfo);

            //Assert
            disciplinaryIncident = _fixture.Db.DisciplinaryIncident
                .Single(d => d.DisciplinaryNumber == "IN201NUM");
            Assert.NotNull(disciplinaryIncident);
        }
    }
}
