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
    public class TrackHousingServiceTest
    {
        private readonly TrackHousingService _trackHousingService;
        private readonly IConfigurationRoot _configuration = MockHelpers.GetIConfigurationRoot();
        private readonly Mock<IMemoryCache> _memory = new Mock<IMemoryCache>();
        public TrackHousingServiceTest(DbInitialize fixture)
        {
            var fixture1 = fixture;
            PersonService personService = new PersonService(fixture.Db);
            AppletsSavedService appletsSavedService = new AppletsSavedService(fixture.Db, _configuration);
            Mock<UserManager<AppUser>> userMgr = MockHelpers.MockUserManager<AppUser>();
            PhotosService photosService = new PhotosService(fixture1.Db, _configuration);
            HttpContextAccessor httpContext = new HttpContextAccessor { HttpContext = fixture.Context.HttpContext };
            InterfaceEngineService interfaceEngineService = new InterfaceEngineService(fixture1.Db,
                _configuration, _memory.Object);
            CommonService commonService = new CommonService(fixture.Db, _configuration,
                httpContext, personService,appletsSavedService,interfaceEngineService);
            FacilityPrivilegeService facilityPrivilegeService = new FacilityPrivilegeService(fixture1.Db, httpContext,
                personService);
            InmateService inmateService = new InmateService(fixture1.Db, commonService, personService, httpContext,
                facilityPrivilegeService,photosService);
            LockdownService lockdownService = new LockdownService(fixture1.Db, httpContext);
            _trackHousingService = new TrackHousingService(fixture.Db, commonService, inmateService, photosService,
                lockdownService);
        }

        //[Fact]
        //public void GetAllLocations()
        //{
        //    //Act
        //    TrackHousingDetailVm trackHousingDetail = _trackHousingService.GetTrackingLocation(2);

        //    //Assert
        //    Assert.True(trackHousingDetail.LstPrivileges.Count > 2);
        //    Assert.True(trackHousingDetail.LstTrackCheckedOut.Count > 1);
        //    Assert.True(trackHousingDetail.LstTrackTransfer.Count > 1);
        //}


    }
}
