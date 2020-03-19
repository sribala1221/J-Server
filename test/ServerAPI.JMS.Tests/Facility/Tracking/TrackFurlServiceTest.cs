using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using ServerAPI.Services;
using ServerAPI.Tests;
using ServerAPI.ViewModels;
using Xunit;
using System.Linq;
using ServerAPI.JMS.Tests.Helper;
using Moq;
using Microsoft.AspNetCore.Identity;
using JwtDb.Models;
using Microsoft.Extensions.Caching.Memory;

// ReSharper disable once CheckNamespace
namespace ServerAPI.JMS.Tests
{
    [Collection("Database collection")]
    public class TrackFurlServiceTest
    {
        private readonly TrackFurlService _trackFurlService;
        private readonly IConfigurationRoot _configuration = MockHelpers.GetIConfigurationRoot();
        private readonly Mock<IMemoryCache> _memory = new Mock<IMemoryCache>();

        public TrackFurlServiceTest(DbInitialize fixture1)
        {
            DbInitialize fixture = fixture1;
            PhotosService photosService = new PhotosService(fixture.Db, _configuration);
            AppletsSavedService appletsSavedService = new AppletsSavedService(fixture.Db, _configuration);
            PersonService personService = new PersonService(fixture.Db);
            Mock<UserManager<AppUser>> userMgr = MockHelpers.MockUserManager<AppUser>();
            HttpContextAccessor httpContext = new HttpContextAccessor { HttpContext = fixture.Context.HttpContext };
            InterfaceEngineService interfaceEngineService = new InterfaceEngineService(fixture.Db,
                _configuration, _memory.Object);
            CommonService commonService = new CommonService(fixture.Db, _configuration, httpContext,personService,appletsSavedService,interfaceEngineService);
            FacilityPrivilegeService facilityPrivilegeService = new FacilityPrivilegeService(fixture.Db, httpContext,
                personService);
            InmateService inmateService = new InmateService(fixture.Db, commonService, personService, httpContext,
                facilityPrivilegeService,photosService);
            FacilityHousingService facilityHousingService = new FacilityHousingService(fixture.Db, _configuration, httpContext);
            _trackFurlService = new TrackFurlService(fixture.Db, commonService, inmateService, facilityPrivilegeService,
                photosService,facilityHousingService);
        }

        [Fact]
        public void TrackFurl_GetFurloughDetails()
        {
            //Act
            WorkCrewFurloughTracking workCrewFurlough = _trackFurlService.GetFurloughDetails(2);

            //Assert
            // WorkCrewFurl Details
            WorkCrewFurlVm workCrewFurl = workCrewFurlough.LstFurloughDetails.Single(w => w.InmateId == 108);
            Assert.Equal("WAREHOUSE", workCrewFurl.WorkCrewName);
            //PrivilegeDetails
            PrivilegeDetailsVm privilegeDetails = workCrewFurlough.LstPrivileges.Single(w => w.PrivilegeId == 10);
            Assert.Equal("VELLORE", privilegeDetails.PrivilegeDescription);
        }
    }
}
