using ServerAPI.Services;
using ServerAPI.Tests;
using Xunit;
using ServerAPI.ViewModels;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using ServerAPI.JMS.Tests.Helper;
using Moq;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.AspNetCore.Identity;
using JwtDb.Models;

// ReSharper disable once CheckNamespace
namespace ServerAPI.JMS.Tests
{
    [Collection("Database collection")]
    public class BookingReleaseServiceTest
    {
        private readonly BookingReleaseService _bookingReleaseService;
        private readonly IConfigurationRoot _configuration = MockHelpers.GetIConfigurationRoot();
        private readonly Mock<IMemoryCache> _memory = new Mock<IMemoryCache>();

        public BookingReleaseServiceTest(DbInitialize fixture)
        {
            var fixture1 = fixture;
            Mock<UserManager<AppUser>> userMgr = MockHelpers.MockUserManager<AppUser>();
            AppletsSavedService appletsSavedService = new AppletsSavedService(fixture1.Db, _configuration);
            HttpContextAccessor httpContext = new HttpContextAccessor { HttpContext = fixture.Context.HttpContext };
            PersonService personService = new PersonService(fixture1.Db);
            PhotosService photosService = new PhotosService(fixture1.Db, _configuration);
            InterfaceEngineService interfaceEngineService = new InterfaceEngineService(fixture.Db, _configuration,
                _memory.Object);
            CommonService commonService = new CommonService(fixture1.Db, _configuration, httpContext, personService,appletsSavedService,interfaceEngineService);
           

            FacilityHousingService facilityHousingService = new FacilityHousingService(fixture1.Db, _configuration, httpContext);
            RecordsCheckService recordsCheckService = new RecordsCheckService(fixture1.Db,commonService,httpContext);

            FacilityPrivilegeService facilityPrivilegeService = new FacilityPrivilegeService(fixture1.Db, httpContext,
                personService);
            InmateService inmateService = new InmateService(fixture1.Db, commonService, personService, httpContext,
                facilityPrivilegeService,photosService);
            _bookingReleaseService = new BookingReleaseService(fixture1.Db, commonService, httpContext, personService,
                inmateService,recordsCheckService,facilityHousingService,interfaceEngineService);
        }

        [Fact]
        public void BookingRelease_GetBookingReleaseDetails()
        {
            //Arrange
            BookingInputsVm bookingInputs = new BookingInputsVm
            {
                FacilityId = 2,
                ReleaseType = ReleaseTypeEnum.OverallReleaseProgress
            };
            //Act
            BookingReleaseDetailsVm bookingReleaseDetails = _bookingReleaseService
                .GetBookingReleaseDetails(bookingInputs);
            //Assert
            Assert.NotNull(bookingReleaseDetails.ReleaseDetails);
        }
    }
}
