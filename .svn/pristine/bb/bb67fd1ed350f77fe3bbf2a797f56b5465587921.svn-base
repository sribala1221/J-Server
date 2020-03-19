using ServerAPI.Services;
using ServerAPI.Tests;
using Xunit;
using Microsoft.AspNetCore.Http;
using ServerAPI.ViewModels;
using System.Linq;
using GenerateTables.Models;
using Moq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Caching.Memory;
using ServerAPI.JMS.Tests.Helper;
using JwtDb.Models;
using Microsoft.AspNetCore.Identity;

namespace ServerAPI.JMS.Tests
{
    [Collection("Database collection")]
    public class MiscNcicServiceTest
    {
        private readonly MiscNcicService _miscNcicService;
        private readonly DbInitialize _fixture;
        private readonly IConfigurationRoot _configuration = MockHelpers.GetIConfigurationRoot();
        private readonly Mock<IMemoryCache> _memory = new Mock<IMemoryCache>();

        public MiscNcicServiceTest(DbInitialize fixture)
        {
            _fixture = fixture;
            AtimsHubService atimsHubService = new AtimsHubService(_fixture.HubContext.Object);
            Mock<UserManager<AppUser>> userMgr = MockHelpers.MockUserManager<AppUser>();
            HttpContextAccessor httpContext = new HttpContextAccessor { HttpContext = fixture.Context.HttpContext };
            InterfaceEngineService interfaceEngineService = new InterfaceEngineService(fixture.Db, _configuration,
                _memory.Object);
            PersonService personService = new PersonService(_fixture.Db);
            FacilityPrivilegeService facilityPrivilegeService = new FacilityPrivilegeService(fixture.Db, httpContext,
                personService);
            AppletsSavedService appletsSavedService = new AppletsSavedService(_fixture.Db, _configuration);
            PhotosService photosService = new PhotosService(_fixture.Db, _configuration);
            CommonService commonService = new CommonService(_fixture.Db, _configuration,
                httpContext, personService,appletsSavedService,interfaceEngineService);
            FacilityHousingService facilityHousingService = new FacilityHousingService(fixture.Db, _configuration, httpContext);
            RecordsCheckService recordsCheckService = new RecordsCheckService(fixture.Db, commonService, httpContext);

            InmateService inmateService = new InmateService(_fixture.Db, commonService, personService, httpContext,
                facilityPrivilegeService, photosService);
            AltSentService altSentService = new AltSentService(_fixture.Db, commonService, httpContext, photosService, personService);

            CellService cellService = new CellService(fixture.Db, commonService, httpContext,
                facilityPrivilegeService, facilityHousingService, _fixture.JwtDb, userMgr.Object);
            BookingReleaseService bookingReleaseService = new BookingReleaseService(
                _fixture.Db, commonService, httpContext, personService, inmateService, recordsCheckService,
                facilityHousingService, interfaceEngineService);
            PrebookWizardService prebookWizardService = new PrebookWizardService(fixture.Db,
                _configuration, commonService, cellService, httpContext, atimsHubService,
                personService, inmateService, bookingReleaseService, altSentService, interfaceEngineService,
                photosService,appletsSavedService);
            WizardService wizardService = new WizardService(_fixture.Db,atimsHubService);
            SentenceService sentenceService = new SentenceService(_fixture.Db, commonService, httpContext, personService,
                interfaceEngineService);

            PrebookActiveService prebookActiveService = new PrebookActiveService(_fixture.Db, httpContext, atimsHubService);
            BookingService bookingService = new BookingService(_fixture.Db, commonService, httpContext,
                bookingReleaseService, prebookWizardService, prebookActiveService, sentenceService, wizardService,
                personService, inmateService, photosService, interfaceEngineService);

            PersonAkaService personAkaService = new PersonAkaService(_fixture.Db, commonService, httpContext, personService,
                interfaceEngineService);
            PersonCharService personCharService = new PersonCharService(_fixture.Db, commonService, httpContext, personService,
                interfaceEngineService);
            PersonIdentityService personIdentityService = new PersonIdentityService(_fixture.Db, httpContext,
                prebookActiveService, personAkaService, personService,interfaceEngineService);
            _miscNcicService = new MiscNcicService(fixture.Db, commonService, personIdentityService, httpContext,
                bookingService, personCharService);
        }

        [Fact]
        public void MiscNcic_GetNcicDetails()
        {
            //Act
            NcicVm ncicVm = _miscNcicService.GetNcicDetails(103, 65);
            //Assert
            Assert.Equal("ON", ncicVm.SiteOption);
            Assert.Equal("NCICREQTRANS", ncicVm.LstNcicType[0].Value);
            Assert.Equal(15, ncicVm.LstNcicType[0].Key);
        }

        [Fact]
        public void MiscNcic_DeleteExternalAttachment()
        {
            //Arrange
            AppletsSaved applets = _fixture.Db.AppletsSaved.Single(a => a.AppletsSavedId == 17);
            Assert.Equal(0, applets.AppletsDeleteFlag);
            //Act
            _miscNcicService.DeleteExternalAttachment(17);
            //Assert
            applets = _fixture.Db.AppletsSaved.Single(a => a.AppletsSavedId == 17);
            Assert.Equal(1, applets.AppletsDeleteFlag);
        }

    }
}
