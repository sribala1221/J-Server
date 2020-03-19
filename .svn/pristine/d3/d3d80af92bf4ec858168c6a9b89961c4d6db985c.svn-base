using JwtDb.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Moq;
using ServerAPI.JMS.Tests.Helper;
using ServerAPI.Policies;
using ServerAPI.Services;
using ServerAPI.Tests;
using Xunit;

// ReSharper disable once CheckNamespace
namespace ServerAPI.JMS.Tests
{

    [Collection("Database collection")]
    public class ApptServiceTest
    {
        private readonly IConfigurationRoot _configuration = MockHelpers.GetIConfigurationRoot();
        private readonly ApptService _apptService;
        private readonly Mock<IMemoryCache> _memory = new Mock<IMemoryCache>();

        public ApptServiceTest(DbInitialize fixture)
        {
            DbInitialize fixture1 = fixture;
            Mock<UserManager<AppUser>> userMgr = MockHelpers.MockUserManager<AppUser>();
            Mock<RoleManager<IdentityRole>> roleMgr = MockHelpers.MockRoleManager<IdentityRole>();
            HttpContextAccessor httpContext = new HttpContextAccessor { HttpContext = fixture.Context.HttpContext };
            InterfaceEngineService interfaceEngineService = new InterfaceEngineService(fixture1.Db,
                _configuration, _memory.Object);
            UserPermissionPolicy userPermission =
                new UserPermissionPolicy(fixture1.Db, userMgr.Object, roleMgr.Object, httpContext);
            PhotosService photosService = new PhotosService(fixture.Db, _configuration);
            AtimsHubService atimsHubService = new AtimsHubService(fixture.HubContext.Object);
            PersonService personService = new PersonService(fixture.Db);
            AppletsSavedService appletsSavedService = new AppletsSavedService(fixture.Db, _configuration);
            CommonService commonService = new CommonService(fixture1.Db, _configuration,
                 httpContext, personService,appletsSavedService,interfaceEngineService);
            PrebookActiveService prebookActiveService = new PrebookActiveService(fixture.Db,
                httpContext, atimsHubService);
            FacilityHousingService facilityHousingService = new FacilityHousingService(fixture1.Db, _configuration, httpContext);
            KeepSepAlertService keepSepAlertService = new KeepSepAlertService(fixture1.Db, httpContext,
                facilityHousingService,interfaceEngineService);
            PersonAkaService personAkaService = new PersonAkaService(fixture.Db, commonService, httpContext, personService,
                interfaceEngineService);
            PersonCharService personCharService = new PersonCharService(fixture.Db, commonService, httpContext, personService,
                interfaceEngineService);
            PersonIdentityService personIdentityService = new PersonIdentityService(fixture.Db,
                httpContext, prebookActiveService, personAkaService, personService,interfaceEngineService);
            WizardService wizardService = new WizardService(fixture.Db,atimsHubService);
            RegisterService registerService = new RegisterService(fixture.Db, httpContext, commonService, personCharService,
                photosService, wizardService, personIdentityService, keepSepAlertService, userPermission);
            AppointmentViewerService appointmentViewerService = new AppointmentViewerService(fixture1.Db, commonService, photosService);

            AppointmentService appointmentService = new AppointmentService(fixture.Db, commonService, registerService,
                httpContext, personService, keepSepAlertService,interfaceEngineService,appointmentViewerService);
            FacilityPrivilegeService facilityPrivilegeService = new FacilityPrivilegeService(fixture1.Db, httpContext,
                personService);
            _apptService = new ApptService(fixture1.Db, commonService, appointmentService,
                photosService, facilityPrivilegeService);
        }


        //[Fact]
        //public void GetApptCourtAndLocation()
        //{
        //    //Arrange
        //    ApptParameter apptParameter = new ApptParameter
        //    {
        //        FacilityId = 2,
        //        LocationId = 5,
        //        HousingUnitListId = new List<int>
        //        {5,6,7 },
        //        ShowDeleted = false
        //    };
        //    //Act
        //    ApptTrackingVm apptTracking = _apptService.GetApptCourtAndLocation(apptParameter);

        //    //Assert
        //    Assert.NotNull(apptTracking);
        //    Assert.True(apptTracking.LocationList.Count > 2);

        //   // 9378 SVN Revision issues fixed
        //   //Here added Visit flag 
        //    ApptTracking tracking = apptTracking.CntCourtApptTracking.Single(c => c.ApptLocationId == 5);
        //    Assert.True(tracking.VisitFlag);
        //}

        //[Fact]
        //public void GetConsoleAppointmentCourtAndLocation()
        //{
        //    //Arrange
        //    ApptParameter apptParameter = new ApptParameter
        //    {
        //        FacilityId = 2
        //    };

        //    //Act
        //    ApptTrackingVm apptTracking = _apptService.GetConsoleAppointmentCourtAndLocation(apptParameter);

        //    //Assert
        //    Assert.True(apptTracking.CntCourtApptTracking.Count > 0);
        //    Assert.True(apptTracking.CntLocApptTracking.Count > 0);
        //}
    }
}
