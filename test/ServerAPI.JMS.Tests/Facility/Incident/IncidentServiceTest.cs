using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Moq;
using ServerAPI.Services;
using ServerAPI.Tests;
using ServerAPI.ViewModels;
using System.Collections.Generic;
using Xunit;
using System.Linq;
using Microsoft.Extensions.Caching.Memory;
using ServerAPI.JMS.Tests.Helper;
using JwtDb.Models;
using Microsoft.AspNetCore.Identity;

// ReSharper disable once CheckNamespace
namespace ServerAPI.JMS.Tests
{

    [Collection("Database collection")]
    public class IncidentServiceTest
    {

        private readonly IncidentService _incidentService;
        private readonly IConfigurationRoot _configuration = MockHelpers.GetIConfigurationRoot();
        private readonly Mock<IMemoryCache> _memory = new Mock<IMemoryCache>();

        public IncidentServiceTest(DbInitialize fixture)
        {
            DbInitialize fixture1 = fixture;
            PhotosService photosService = new PhotosService(fixture.Db, _configuration);
            Mock<UserManager<AppUser>> userMgr = MockHelpers.MockUserManager<AppUser>();
            HttpContextAccessor httpContext = new HttpContextAccessor {HttpContext = fixture.Context.HttpContext};
            PersonService personService = new PersonService(fixture1.Db);
            InterfaceEngineService interfaceEngineService = new InterfaceEngineService(fixture.Db, _configuration, 
                _memory.Object);
            AppletsSavedService appletsSavedService = new AppletsSavedService(fixture.Db, _configuration);
            CommonService commonService = new CommonService(fixture1.Db, _configuration, httpContext,personService,appletsSavedService,interfaceEngineService);
            
            FacilityPrivilegeService facilityPrivilegeService = new FacilityPrivilegeService(fixture.Db, httpContext,
                personService);
            FacilityHousingService facilityHousingService = new FacilityHousingService(fixture1.Db, _configuration,
                httpContext);
            RecordsCheckService recordsCheckService = new RecordsCheckService(fixture1.Db, commonService, httpContext);
            InmateService inmateService = new InmateService(fixture1.Db, commonService, personService, httpContext,
                facilityPrivilegeService,photosService);
            BookingReleaseService bookingReleaseService =
                new BookingReleaseService(fixture1.Db, commonService, httpContext, personService, inmateService,
                    recordsCheckService,facilityHousingService,interfaceEngineService); 
            AtimsHubService atimsHubService = new AtimsHubService(fixture.HubContext.Object);

            CellService cellService = new CellService(fixture1.Db, commonService, httpContext,
                facilityPrivilegeService, facilityHousingService,fixture1.JwtDb ,userMgr.Object);
            AltSentService altSentService = new AltSentService(fixture.Db, commonService, httpContext, photosService, 
                personService);
            PrebookWizardService prebookWizardService = new PrebookWizardService(
                fixture1.Db, _configuration, commonService, cellService, httpContext,atimsHubService,
                personService,inmateService,bookingReleaseService,altSentService,interfaceEngineService,
                photosService,appletsSavedService);

            PrebookActiveService prebookActiveService = new PrebookActiveService(fixture1.Db,
                httpContext,atimsHubService);

            SentenceService sentenceService = new SentenceService(fixture1.Db, commonService, httpContext,
                personService,interfaceEngineService);
            WizardService wizardService = new WizardService(fixture1.Db,atimsHubService);
           
            BookingService bookingService = new BookingService(
                fixture1.Db, commonService, httpContext, bookingReleaseService, prebookWizardService,
                prebookActiveService, sentenceService,wizardService,personService,inmateService, photosService
                ,interfaceEngineService);

            _incidentService =
                new IncidentService(fixture1.Db, wizardService, commonService, bookingService, httpContext, photosService);
        }


        [Fact]
        public void GetHearingLocation()
        {
            //Act
            List<KeyValuePair<int, string>> lstLocationDetails = _incidentService.GetHearingLocation();
            //Assert
            KeyValuePair<int, string> location = lstLocationDetails.Single(s => s.Key == 5);
            Assert.Equal("CHENNAI", location.Value);
        }

        //[Fact]
        //public void GetIncidentCalendarDetails()
        //{
        //    //Act
        //    List<IncidentCalendarVm> lstIncidentCalendars =
        //        _incidentService.GetIncidentCalendarDetails(DateTime.Now.Date, 2);
        //    //Assert
        //    IncidentCalendarVm incidentCalendar = lstIncidentCalendars.Single(i => i.PersonId == 65);
        //    Assert.Equal("PUDUKKOTTAI", incidentCalendar.AppointmentLocation);
        //}

        [Fact]
        public void GetIncidentWizard()
        {
            //Act
            AoWizardFacilityVm aoWizardFacility = _incidentService.GetIncidentWizard();
            //Assert
            Assert.Equal("MCJ", aoWizardFacility.Facility.FacilityAbbr);
        }

        [Fact]
        public void NarrativeCommonDetail()
        {
            //Act
            List<IncidentNarrativeDetailVm> narrative =
                _incidentService.NarrativeDetails(1);
            //Assert
            IncidentNarrativeDetailVm incidentNarrative =
                narrative.Single(n => n.DisciplinaryIncidentId == 5);

            Assert.Equal("DIS_100101", incidentNarrative.IncidentNumber);
        }

        [Fact]
        public void GetInvPartyDetails()
        {
            //Act
            List<ClassifyInvPartyDetailsVm> lsyClassifyInvPartyDetails =
                _incidentService.GetInvPartyDetails(1,3);

            //Assert
            ClassifyInvPartyDetailsVm classifyInvParty =
                lsyClassifyInvPartyDetails.Single(i => i.DisciplinaryInmateId == 101);
            Assert.Equal("ATTEMPT SUICIDE", classifyInvParty.IncidentType);
        }
    }
}

