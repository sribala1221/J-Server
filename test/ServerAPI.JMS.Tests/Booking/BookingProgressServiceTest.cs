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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace ServerAPI.JMS.Tests
{

    [Collection("Database collection")]
    public class BookingProgressServiceTest
    {
        private readonly BookingProgressService _bookingProgressService;
        private readonly IConfigurationRoot _configuration = MockHelpers.GetIConfigurationRoot();
        private readonly DbInitialize _fixture;
        private readonly Mock<IMemoryCache> _memory = new Mock<IMemoryCache>();

        public BookingProgressServiceTest(DbInitialize fixture)
        {
            _fixture = fixture;
            AtimsHubService atimsHubService = new AtimsHubService(_fixture.HubContext.Object);
            Mock<UserManager<AppUser>> userMgr = MockHelpers.MockUserManager<AppUser>();
            Mock<RoleManager<IdentityRole>> roleMgr = MockHelpers.MockRoleManager<IdentityRole>();
            AppletsSavedService appletsSavedService = new AppletsSavedService(_fixture.Db, _configuration);
            HttpContextAccessor httpContext = new HttpContextAccessor
            { HttpContext = _fixture.Context.HttpContext };
            PhotosService photosService = new PhotosService(_fixture.Db, _configuration);
            PersonService personService = new PersonService(_fixture.Db);

            InterfaceEngineService interfaceEngineService = new InterfaceEngineService(_fixture.Db,
                _configuration, _memory.Object);

            CommonService commonService = new CommonService(_fixture.Db, _configuration,
                    httpContext, personService, appletsSavedService, interfaceEngineService);


            FacilityPrivilegeService facilityPrivilegeService = new FacilityPrivilegeService(fixture.Db, httpContext, personService);
            InmateService inmateService = new InmateService(_fixture.Db, commonService, personService, httpContext,
                facilityPrivilegeService, photosService);
            FacilityHousingService facilityHousingService = new FacilityHousingService(_fixture.Db, _configuration, httpContext);
            RecordsCheckService recordsCheckService = new RecordsCheckService(_fixture.Db, commonService, httpContext);

            BookingReleaseService bookingReleaseService = new BookingReleaseService(
                _fixture.Db, commonService, httpContext, personService, inmateService, recordsCheckService,
                facilityHousingService, interfaceEngineService);
            AltSentService altSentService = new AltSentService(_fixture.Db, commonService, httpContext, photosService, personService);
            CellService cellService = new CellService(fixture.Db, commonService, httpContext,
                facilityPrivilegeService, facilityHousingService, _fixture.JwtDb, userMgr.Object);
            PrebookWizardService prebookWizardService = new PrebookWizardService(_fixture.Db,
                _configuration, commonService, cellService, httpContext, atimsHubService,
                personService, inmateService, bookingReleaseService, altSentService, interfaceEngineService,
                photosService, appletsSavedService);
            WizardService wizardService = new WizardService(_fixture.Db,atimsHubService);
            RequestService requestService = new RequestService(_fixture.Db, httpContext, userMgr.Object, roleMgr.Object, personService, inmateService,
                facilityHousingService, atimsHubService);

            SentenceService sentenceService = new SentenceService(_fixture.Db, commonService, httpContext, personService,
                interfaceEngineService);
            PrebookActiveService prebookActiveService = new PrebookActiveService(_fixture.Db, httpContext, atimsHubService);
            BookingService bookingService = new BookingService(_fixture.Db, commonService, httpContext,
                bookingReleaseService, prebookWizardService, prebookActiveService, sentenceService, wizardService,
                personService, inmateService, photosService, interfaceEngineService);
            _bookingProgressService = new BookingProgressService(_fixture.Db, commonService, personService, recordsCheckService,
                requestService, bookingService, appletsSavedService);
        }

        [Fact]
        public void Booking_GetBookingInProgress()
        {
            //Arrange
            BookingInputsVm bookingInput = new BookingInputsVm
            {
                FacilityId = 2,
                ShowIntake = true,
                BookingType = BookingTypeEnum.OverallBookingProgress
            };
            //Act
            BookingInProgressCountVm count = _bookingProgressService.GetBookingInProgress(bookingInput);

            //Assert
            Assert.True(count.BookingCounts.Count > 2);
            RequestTypes assignedQueue = count.AssignedQueue.Single(a => a.RequestLookupId == 15);
            Assert.Equal("PROGRAM REQUEST", assignedQueue.RequestLookupName);
        }

        [Fact]
        public void Booking_GetBookingInProgressDetails()
        {
            //Arrange
            BookingInputsVm bookingInput = new BookingInputsVm
            {
                ActiveOnly = true,
                FacilityId = 1,
                BookingType = BookingTypeEnum.BookingDataProgress,
                ShowIntake = true
            };
            //Act
            BookingInProgressDetailsVm bookingInProgressDetails = _bookingProgressService
                .GetBookingInProgressDetails(bookingInput);
            //Assert
            ArrestDetailsVm addressDetail = bookingInProgressDetails.BookingDataProgress
                .Single(b => b.IncarcerationId == 15);
            Assert.Equal("SA", addressDetail.ArrestAgencyName);
            Assert.Equal(105, addressDetail.InmateId);
        }


        [Fact]
        public void GetBookingOverviewDetails()
        {
            //Act
            BookingOverviewVm bookingOverview = _bookingProgressService.GetBookingOverviewDetails(2);

            //Assert
            Assert.True(bookingOverview.BookingOverviewDetails.Count > 5);
            Assert.True(bookingOverview.TasksCount.Count > 0);
        }

        [Fact]
        public void GetBookingOverview()
        {
            //Act
            BookingOverviewVm bookingOverview = _bookingProgressService.GetBookingOverview(2, 105);

            //Assert
            BookingOverviewDetails bookingOverviewDetails = bookingOverview.BookingOverviewDetails.
                Single(p => p.PrebookId == 13);
            Assert.Equal("B-4478499", bookingOverviewDetails.PrebookNumber);

        }

    }
}
