using Microsoft.Extensions.Configuration;
using Moq;
using ServerAPI.Services;
using ServerAPI.Tests;
using ServerAPI.ViewModels;
using System.Linq;
using Xunit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using JwtDb.Models;
using ServerAPI.JMS.Tests.Helper;
using Microsoft.Extensions.Caching.Memory;
using ServerAPI.Policies;

// ReSharper disable once CheckNamespace
namespace ServerAPI.JMS.Tests
{
    [Collection("Database collection")]
    public class PersonHeaderServiceTest
    {
        private readonly PersonHeaderService _personHeaderService;
        private readonly IConfigurationRoot _configuration = MockHelpers.GetIConfigurationRoot();
        private readonly Mock<IMemoryCache> _memory = new Mock<IMemoryCache>();

        public PersonHeaderServiceTest(DbInitialize fixture)
        {
            DbInitialize fixture1 = fixture;
            Mock<UserManager<AppUser>> userMgr = MockHelpers.MockUserManager<AppUser>();
            Mock<RoleManager<IdentityRole>> roleMgr = MockHelpers.MockRoleManager<IdentityRole>();
            AppletsSavedService appletsSavedService = new AppletsSavedService(fixture1.Db, _configuration);
            HttpContextAccessor httpContext = new HttpContextAccessor { HttpContext = fixture1.Context.HttpContext };
            UserPermissionPolicy userePermissionPolicy =
                new UserPermissionPolicy(fixture1.Db, userMgr.Object, roleMgr.Object, httpContext);
            AtimsHubService atimsHubService = new AtimsHubService(fixture1.HubContext.Object);
            PersonService personService = new PersonService(fixture1.Db);
            PhotosService photosService = new PhotosService(fixture1.Db, _configuration);
            FacilityPrivilegeService facilityPrivilegeService = new FacilityPrivilegeService(fixture1.Db, httpContext,
                personService);
            InterfaceEngineService interfaceEngineService = new InterfaceEngineService(fixture1.Db, _configuration, _memory.Object);
            CommonService commonService = new CommonService(fixture1.Db, _configuration,
                httpContext, personService, appletsSavedService, interfaceEngineService);
            FacilityHousingService facilityHousingService = new FacilityHousingService(fixture1.Db, _configuration, httpContext);
            RecordsCheckService recordsCheckService = new RecordsCheckService(fixture1.Db, commonService, httpContext);
            InmateService inmateService = new InmateService(fixture1.Db, commonService, personService, httpContext,
                facilityPrivilegeService, photosService);
            PersonPhotoService personPhotoService = new PersonPhotoService(fixture1.Db, _configuration,
                httpContext, personService, photosService);
            PersonCharService personCharService = new PersonCharService(fixture1.Db, commonService, httpContext, personService,
                interfaceEngineService);
            PrebookActiveService prebookActiveService = new PrebookActiveService(fixture1.Db,
                httpContext, atimsHubService);
            PersonAkaService personAkaService = new PersonAkaService(fixture1.Db, commonService, httpContext, personService,
                interfaceEngineService);
            PersonIdentityService personIdentityService = new PersonIdentityService(fixture1.Db, httpContext,
            prebookActiveService, personAkaService, personService,interfaceEngineService);
            CellService cellService = new CellService(fixture1.Db, commonService, httpContext,
                facilityPrivilegeService, facilityHousingService, fixture1.JwtDb, userMgr.Object);
            AlertService alertService = new AlertService(fixture1.Db, httpContext, commonService, personService,
                interfaceEngineService, userePermissionPolicy);
            KeepSepAlertService keepSepAlertService = new KeepSepAlertService(fixture1.Db, httpContext,
                facilityHousingService,interfaceEngineService);
            BookingReleaseService bookingReleaseService = new BookingReleaseService(
                fixture1.Db, commonService, httpContext, personService, inmateService,
                recordsCheckService, facilityHousingService, interfaceEngineService);
            PersonProfileService personProfileService = new PersonProfileService(fixture1.Db, personIdentityService, httpContext);
            AltSentService altSentService = new AltSentService(fixture1.Db, commonService, httpContext, photosService, personService);
            PrebookWizardService prebookWizardService = new PrebookWizardService(fixture1.Db,
                _configuration, commonService, cellService, httpContext, atimsHubService, personService,
                inmateService, bookingReleaseService, altSentService, interfaceEngineService, photosService, appletsSavedService);
            WizardService wizardService = new WizardService(fixture1.Db,atimsHubService);
            SentenceService sentenceService = new SentenceService(fixture1.Db, commonService, httpContext, personService,
                interfaceEngineService);
            BookingService bookingService = new BookingService(fixture1.Db, commonService, httpContext,
                bookingReleaseService, prebookWizardService, prebookActiveService, sentenceService, wizardService,
                personService, inmateService, photosService, interfaceEngineService);
            _personHeaderService = new PersonHeaderService(fixture1.Db, userMgr.Object, roleMgr.Object,
                personProfileService, personCharService, personPhotoService, alertService, keepSepAlertService, bookingService);
        }


        [Fact]
        public void PersonHeaderService_GetPersonDetails()
        {
            //Act
            PersonDetail personDetail = _personHeaderService.GetPersonDetails(60, true, 0);
            //Assert
            Assert.NotNull(personDetail);
            Assert.Equal("NAVEEN", personDetail.FknMiddleName);
            Assert.Equal("YC", personDetail.DlClass);
            //Aka
            AkaVm aka = personDetail.Aka.Single(a => a.AkaFirstName == "VIJAI");
            Assert.Equal("LATE PIC UP", aka.PersonGangName);
            //Person Association
            PersonClassificationDetails personClassification =
                personDetail.LstAssociation.Single(a => a.ClassificationStatus == "LEADER");
            Assert.Equal("SUPPLY ALL METIRIALS", personClassification.ClassificationNotes);
            //Person Privilege Alerts
            PrivilegeDetailsVm privilegeDetails = personDetail.LstPrivilegesAlerts.Single(p => p.PrivilegeId == 5);
            Assert.Equal("REVOKE", privilegeDetails.PrivilegeType);
        }
    }
}
