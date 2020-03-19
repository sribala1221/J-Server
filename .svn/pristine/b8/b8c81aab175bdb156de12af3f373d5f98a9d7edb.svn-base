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
    public class FacilityAppointmentServiceTest
    {
        private readonly FacilityAppointmentService _facilityAppointment;
        private readonly IConfigurationRoot _configuration = MockHelpers.GetIConfigurationRoot();
        private readonly Mock<IMemoryCache> _memory = new Mock<IMemoryCache>();
        public FacilityAppointmentServiceTest(DbInitialize fixture)
        {
            DbInitialize fixture1 = fixture;
            Mock<UserManager<AppUser>> userMgr = MockHelpers.MockUserManager<AppUser>();
            AppletsSavedService appletsSavedService = new AppletsSavedService(fixture.Db, _configuration);
            PhotosService photosService = new PhotosService(fixture.Db, _configuration);
            InterfaceEngineService interfaceEngineService = new InterfaceEngineService(fixture1.Db,
                _configuration, _memory.Object);
            HttpContextAccessor httpContext = new HttpContextAccessor { HttpContext = fixture1.Context.HttpContext };
            PersonService personService = new PersonService(fixture1.Db);
            CommonService commonService = new CommonService(fixture1.Db, _configuration, httpContext, personService,appletsSavedService,interfaceEngineService);
            _facilityAppointment = new FacilityAppointmentService(fixture1.Db, commonService);
        }

        [Fact]
        public void GetAppointmentList()
        {
            FacilityApptVm facilityAppointment = new FacilityApptVm
            {
                IsCourtSearch = true,
                IsInmateSearch = true,
                IsFacilityEventSearch = true,
                IsProgramSearch = true,
                IsReleased = false,
                DeleteFlag = false,
                FacilityId = 1,
                FromDate = null,
                ToDate = null
            };

            _facilityAppointment.GetAppointmentList(facilityAppointment);

        }

        [Fact]
        public void LoadApptFilterlist()
        {
            AppointmentFilterVm appointmentFilter = _facilityAppointment.LoadApptFilterlist(2);

            Assert.NotNull(appointmentFilter.AppointmentReason);
            Assert.NotNull(appointmentFilter.HousingUnitLoc);
            Assert.NotNull(appointmentFilter.Location);
        }

    }
}
