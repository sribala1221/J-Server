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
using ServerAPI.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

// ReSharper disable once CheckNamespace
namespace ServerAPI.JMS.Tests
{
    [Collection("Database collection")]
    public class ScheduleServiceTest
    {

        private readonly ScheduleService _scheduleService;
        private readonly IConfigurationRoot _configuration = MockHelpers.GetIConfigurationRoot();
        private readonly DbInitialize _fixture;
        private readonly Mock<IMemoryCache> _memory = new Mock<IMemoryCache>();

        public ScheduleServiceTest(DbInitialize fixture)
        {
            _fixture = fixture;
            PersonService personService = new PersonService(fixture.Db);
            Mock<UserManager<AppUser>> userMgr = MockHelpers.MockUserManager<AppUser>();
            AppletsSavedService appletsSavedService = new AppletsSavedService(_fixture.Db, _configuration);
            AtimsHubService atimsHubService = new AtimsHubService(_fixture.HubContext.Object);
            PhotosService photosService = new PhotosService(fixture.Db, _configuration);
            HttpContextAccessor httpContext = new HttpContextAccessor { HttpContext = fixture.Context.HttpContext };
            Mock<RoleManager<IdentityRole>> roleMgr = MockHelpers.MockRoleManager<IdentityRole>();
            UserPermissionPolicy userPermission =
                new UserPermissionPolicy(_fixture.Db, userMgr.Object, roleMgr.Object, httpContext);
            InterfaceEngineService interfaceEngineService = new InterfaceEngineService(_fixture.Db,
                _configuration, _memory.Object);
            CommonService commonService = new CommonService(_fixture.Db, _configuration,
                httpContext, personService, appletsSavedService, interfaceEngineService);
            FacilityHousingService facilityHousingService = new FacilityHousingService(_fixture.Db, _configuration,
                httpContext);
            KeepSepAlertService keepSepAlertService = new KeepSepAlertService(_fixture.Db, httpContext,
                facilityHousingService,interfaceEngineService);
            WizardService wizardService = new WizardService(_fixture.Db, atimsHubService);
            PersonAkaService personAkaService = new PersonAkaService(_fixture.Db, commonService, httpContext,
                personService,interfaceEngineService);
            PrebookActiveService prebookActiveService = new PrebookActiveService(_fixture.Db, httpContext,
                atimsHubService);
            PersonCharService personCharService = new PersonCharService(_fixture.Db, commonService, httpContext,
                personService,interfaceEngineService);
            PersonIdentityService personIdentityService = new PersonIdentityService(_fixture.Db,
                httpContext, prebookActiveService, personAkaService, personService,interfaceEngineService);

            RegisterService registerService = new RegisterService(_fixture.Db, httpContext, commonService,
                personCharService, photosService, wizardService, personIdentityService, keepSepAlertService,
                userPermission);
            AppointmentViewerService appointmentViewerService = new AppointmentViewerService(_fixture.Db, commonService, photosService);

            AppointmentService appointmentService = new AppointmentService(fixture.Db, commonService, registerService,
                httpContext, personService, keepSepAlertService,interfaceEngineService, appointmentViewerService);

            _scheduleService = new ScheduleService(_fixture.Db, appointmentService);
        }

        //no use this service

        [Fact]
        public void GetVisitScheduleDetails()
        {
           
            FacilityApptVm facilityAppt = new FacilityApptVm
            {
                FacilityId = 1,
                VisitationDay = DateTime.Now.Date.DayOfWeek.ToString(),
                FromDate = DateTime.Now.Date.AddDays(-1),
                VisitorLocation = "TRICHY",
                ToDate = DateTime.Now
            };

            VisitScheduleVm visitSchedule = _scheduleService.GetVisitScheduleDetails(facilityAppt);
            Assert.True(visitSchedule.PrivilegeList.Count > 0);
            Assert.True(visitSchedule.DailyVisitationDetails.Count > 0);
        }

        [Fact]
        public void GetVisitationInmateDetails()
        {
            FacilityApptVm facilityAppt = new FacilityApptVm
            {
                FacilityId = 2,
                HousingLocation = "FLOOR1",
                HousingNumber = "UP-B",
                LastNameChar = "KU"
            };
            List<InmateDetailsList> lstDetailsLists = _scheduleService.GetVisitationInmateDetails(facilityAppt);
            Assert.True(lstDetailsLists.Count > 1);


        }
    }
}
