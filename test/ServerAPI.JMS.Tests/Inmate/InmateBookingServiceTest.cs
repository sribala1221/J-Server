using Xunit;
using System.Linq;
using ServerAPI.Services;
using ServerAPI.Tests;
using Microsoft.AspNetCore.Http;
using ServerAPI.ViewModels;
using System.Threading.Tasks;
using GenerateTables.Models;
using Microsoft.Extensions.Configuration;
using ServerAPI.JMS.Tests.Helper;
using Moq;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.AspNetCore.Identity;
using JwtDb.Models;
using ServerAPI.Policies;

// ReSharper disable once CheckNamespace
namespace ServerAPI.JMS.Tests
{
    [Collection("Database collection")]
    public class InmateBookingServiceTest
    {
        private readonly InmateBookingService _inmateBookingService;
        private readonly DbInitialize _fixture;
        private readonly IConfigurationRoot _configuration = MockHelpers.GetIConfigurationRoot();
        private readonly Mock<IMemoryCache> _memory = new Mock<IMemoryCache>();

        public InmateBookingServiceTest(DbInitialize fixture)
        {
            _fixture = fixture;
            Mock<UserManager<AppUser>> userMgr = MockHelpers.MockUserManager<AppUser>();
            AtimsHubService atimsHubService = new AtimsHubService(fixture.HubContext.Object);
            AppletsSavedService appletsSavedService = new AppletsSavedService(fixture.Db, _configuration);
            HttpContextAccessor httpContext = new HttpContextAccessor { HttpContext = fixture.Context.HttpContext };
            Mock<RoleManager<IdentityRole>> roleMgr = MockHelpers.MockRoleManager<IdentityRole>();
            UserPermissionPolicy userPermission =
                new UserPermissionPolicy(fixture.Db, userMgr.Object, roleMgr.Object, httpContext);
            PersonService personService = new PersonService(fixture.Db);
            PhotosService photosService = new PhotosService(_fixture.Db, _configuration);
            FacilityPrivilegeService facilityPrivilegeService = new FacilityPrivilegeService(fixture.Db, httpContext,
                personService);
            InterfaceEngineService interfaceEngineService = new InterfaceEngineService(fixture.Db, _configuration,
                _memory.Object);
            CommonService commonService = new CommonService(fixture.Db, _configuration, httpContext, personService,
                appletsSavedService, interfaceEngineService);
            FacilityHousingService facilityHousingService = new FacilityHousingService(fixture.Db, _configuration,
                httpContext);
            RecordsCheckService recordsCheckService = new RecordsCheckService(fixture.Db, commonService, httpContext);

            InmateService inmateService = new InmateService(fixture.Db, commonService, personService, httpContext,
                facilityPrivilegeService, photosService);
            SearchService searchService = new SearchService(fixture.Db, photosService);

            KeepSepAlertService keepSepAlertService = new KeepSepAlertService(fixture.Db, httpContext,
                facilityHousingService,interfaceEngineService);
            WizardService wizardService = new WizardService(fixture.Db, atimsHubService);
            PersonAkaService personAkaService =
                new PersonAkaService(fixture.Db, commonService, httpContext, personService,
                    interfaceEngineService);
            PrebookActiveService prebookActiveService =
                new PrebookActiveService(fixture.Db, httpContext, atimsHubService);
            PersonCharService personCharService = new PersonCharService(fixture.Db, commonService, httpContext,
                personService,interfaceEngineService);
            PersonIdentityService personIdentityService = new PersonIdentityService(fixture.Db,
                httpContext, prebookActiveService, personAkaService, personService,interfaceEngineService);
            AppointmentViewerService appointmentViewerService =
                new AppointmentViewerService(fixture.Db, commonService, photosService);

            RegisterService registerService = new RegisterService(fixture.Db, httpContext, commonService,
                personCharService, photosService, wizardService, personIdentityService, keepSepAlertService,
                userPermission);
            AppointmentService appointmentService = new AppointmentService(fixture.Db,
                commonService, registerService, httpContext, personService, keepSepAlertService,
                interfaceEngineService,appointmentViewerService);
            BookingReleaseService bookingReleaseService = new BookingReleaseService(_fixture.Db,
                commonService, httpContext, personService, inmateService, recordsCheckService, facilityHousingService, interfaceEngineService);
<<<<<<< .mine
            // _inmateBookingService = new InmateBookingService(
            //     _fixture.Db, httpContext, bookingReleaseService, commonService, searchService,
            //     interfaceEngineService);
||||||| .r13927
            _inmateBookingService = new InmateBookingService(
                _fixture.Db, httpContext, bookingReleaseService, commonService, searchService,
                interfaceEngineService);
=======
            _inmateBookingService = new InmateBookingService(
                _fixture.Db, httpContext, bookingReleaseService, commonService, searchService,
                interfaceEngineService, appointmentService);
>>>>>>> .r13948
        }

        [Fact]
        public void InmateBooking_GetInmateBookingDetails()
        {
            InmateBookingData inmateBooking = new InmateBookingData
            {
                InmateId = 107,
                IncarcerationId = 18,
                PersonId = 99
            };
            //Act
            InmateBookingDetailsVm inmateBookingDetails = _inmateBookingService.GetInmateBookingDetails(inmateBooking);
            //Assert
            InmateIncarcerationDetails inmateIncarceration = inmateBookingDetails.BookingInmateDetails
                .Single(l => l.IncarcerationId == 18);
            Assert.Equal("SENTHIL", inmateIncarceration.UsedPersonFirst);
        }

        [Fact]
        public async Task InmateBooking_DeleteInmateActiveCommit()
        {
            //Before Update
            InmatePrebook inmatePrebook = _fixture.Db.InmatePrebook.Single(i => i.InmatePrebookId == 11);
            Assert.Null(inmatePrebook.DeletedBy);
            //Act
            await _inmateBookingService.DeleteInmateActiveCommit(11);

            //After Update
            //Assert
            inmatePrebook = _fixture.Db.InmatePrebook.Single(i => i.InmatePrebookId == 11);
            Assert.Equal(11, inmatePrebook.DeletedBy);
        }

        [Fact]
        public async Task InmateBooking_UndoInmateBooking()
        {
            //Before Update
            InmateBookingData undoBookingDetails = new InmateBookingData
            {
                ArrestId = 6,
                PersonId = 60,
                BailTransaction = false,
                DoNotDoSaveHistory = false,
                IncarcerationArrestXrefId = 5,
                IncarcerationId = 11
            };

            IncarcerationArrestXref arrestXref =
                _fixture.Db.IncarcerationArrestXref.Single(i => i.IncarcerationArrestXrefId == 5);
            Assert.Equal("BAIL", arrestXref.ReleaseReason);
            Incarceration incarceration = _fixture.Db.Incarceration.Single(i => i.IncarcerationId == 11);
            Assert.Equal(11, incarceration.ReleaseClearBy);

            BailSaveHistory bailSaveHistory = _fixture.Db.BailSaveHistory
                .SingleOrDefault(b => b.ArrestId == 6);
            Assert.Null(bailSaveHistory);
            //Act
            await _inmateBookingService.UndoInmateBooking(undoBookingDetails);

            //After Update
            //Assert
            arrestXref = _fixture.Db.IncarcerationArrestXref.Single(i => i.IncarcerationArrestXrefId == 5);
            Assert.Null(arrestXref.ReleaseReason);
            incarceration = _fixture.Db.Incarceration.Single(i => i.IncarcerationId == 11);
            Assert.Null(incarceration.ReleaseClearBy);
            //Insert BailSaveHistory values
            bailSaveHistory = _fixture.Db.BailSaveHistory.Single(b => b.ArrestId == 6);
            Assert.NotNull(bailSaveHistory);
        }

        [Fact]
        public async Task InmateBooking_UndoReleaseInmateBooking()
        {
            //Before Update
            InmateBookingData undoBookingDetails = new InmateBookingData
            {
                IncarcerationId = 20,
                InmateId = 120
            };

            GenerateTables.Models.Inmate incarceration = _fixture.Db.Inmate.Single(i => i.InmateId == 120);
            Assert.Equal(0, incarceration.InmateActive);
            //Act
            await _inmateBookingService.UndoReleaseInmateBooking(undoBookingDetails);
            //Assert
            incarceration = _fixture.Db.Inmate.Single(i => i.InmateId == 120);
            Assert.Equal(1, incarceration.InmateActive);
        }
    }
}
