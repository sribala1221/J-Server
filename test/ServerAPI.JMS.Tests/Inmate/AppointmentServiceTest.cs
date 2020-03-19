using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using ServerAPI.Services;
using ServerAPI.Tests;
using ServerAPI.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GenerateTables.Models;
using Xunit;
using ScheduleWidget.Common;
using System.Linq;
using ScheduleWidget.TemporalExpressions.Base;
using ScheduleWidget.TemporalExpressions;
using ServerAPI.JMS.Tests.Helper;
using Moq;
using Microsoft.AspNetCore.Identity;
using JwtDb.Models;
using ServerAPI.Policies;
using Microsoft.Extensions.Caching.Memory;

// ReSharper disable once CheckNamespace
namespace ServerAPI.JMS.Tests
{

    [Collection("Database collection")]
    public class AppointmentServiceTest
    {
        private readonly AppointmentService _appointmentService;
        private readonly IConfigurationRoot _configuration = MockHelpers.GetIConfigurationRoot();
        private readonly DbInitialize _fixture;
        private readonly Mock<IMemoryCache> _memory = new Mock<IMemoryCache>();
        
        public AppointmentServiceTest(DbInitialize fixture)
        {
            _fixture = fixture;
            PersonService personService = new PersonService(fixture.Db);
            AppletsSavedService appletsSavedService = new AppletsSavedService(fixture.Db, _configuration);
            Mock<UserManager<AppUser>> userMgr = MockHelpers.MockUserManager<AppUser>();
            PhotosService photosService = new PhotosService(_fixture.Db, _configuration);
            AtimsHubService atimsHubService = new AtimsHubService(_fixture.HubContext.Object);
            Mock<RoleManager<IdentityRole>> roleMgr = MockHelpers.MockRoleManager<IdentityRole>();
            HttpContextAccessor httpContext = new HttpContextAccessor { HttpContext = fixture.Context.HttpContext };
            UserPermissionPolicy userPermission =
                new UserPermissionPolicy(_fixture.Db, userMgr.Object, roleMgr.Object, httpContext);
            InterfaceEngineService interfaceEngineService = new InterfaceEngineService(_fixture.Db,
                _configuration, _memory.Object);
            CommonService commonService = new CommonService(fixture.Db, _configuration, httpContext,personService,appletsSavedService,interfaceEngineService);
            PrebookActiveService prebookActiveService = new PrebookActiveService(_fixture.Db,
                httpContext, atimsHubService);
            WizardService wizardService = new WizardService(_fixture.Db,atimsHubService);
            FacilityHousingService facilityHousingService = new FacilityHousingService(_fixture.Db,_configuration,httpContext); 
            KeepSepAlertService keepSepAlertService = new KeepSepAlertService(_fixture.Db, httpContext,
                facilityHousingService,interfaceEngineService);
            PersonAkaService personAkaService = new PersonAkaService(_fixture.Db, commonService, httpContext, personService,
                interfaceEngineService);
            PersonCharService personCharService = new PersonCharService(_fixture.Db, commonService, httpContext, personService,interfaceEngineService);
            PersonIdentityService personIdentityService = new PersonIdentityService(_fixture.Db,
                httpContext, prebookActiveService, personAkaService, personService,interfaceEngineService);
            AppointmentViewerService appointmentViewerService = new AppointmentViewerService(_fixture.Db, commonService, photosService);

            RegisterService registerService = new RegisterService(_fixture.Db, httpContext, commonService, personCharService,
                photosService, wizardService, personIdentityService, keepSepAlertService, userPermission);
            _appointmentService = new AppointmentService(fixture.Db, commonService, registerService, httpContext,personService,
                keepSepAlertService,interfaceEngineService,appointmentViewerService);
        }

        //Due to under processing development


        //Create single Recurring

      //  [Fact]
      //  public async Task SingleAppointment()
      //  {
      //      //Arrange
      //      ScheduleInmate apptSchedule = new ScheduleInmate
      //      {
      //          InmateId = 104,
      //          StartDate = new DateTime(2018, 10, 1, 8, 0, 0),
      //          EndDate = new DateTime(2018, 10, 1, 9, 0, 0),
      //          LocationId = 7,
      //          IsSingleOccurrence = true,
      //          DayInterval = DayInterval.None
      //      };
      //      InmateApptDetailsVm inmateApptDetails = new InmateApptDetailsVm
      //      {
      //          AoScheduleDetails = apptSchedule

      //      };
      //      //Act
      //      await _appointmentService.InsertSchedule(inmateApptDetails);

      //      //Assert
      //      List<AoAppointmentVm> inmateApptList = _appointmentService.ListAoApointments(104, new DateTime(2018, 10, 1, 8, 0, 0), new DateTime(2018, 10, 2, 9, 0, 0), false);
      //      Assert.Single(inmateApptList);
      //      Assert.Equal(104, inmateApptList[0].InmateId);
      //  }

      //  [Fact]
      //  public async Task ListSchedule()
      //  {
      //      //Arrange
      //      ScheduleInmate apptSchedule = new ScheduleInmate
      //      {
      //          InmateId = 106,
      //          StartDate = new DateTime(2018, 10, 1, 8, 0, 0),
      //          EndDate = new DateTime(2018, 10, 31, 0, 0, 0),
      //          Duration = TimeSpan.FromHours(1),
      //          LocationId = 7,
      //          IsSingleOccurrence = false,
      //          DayInterval = DayInterval.Mon | DayInterval.Wed | DayInterval.Fri,
      //          FrequencyType = FrequencyType.Weekly
      //      };
      //      InmateApptDetailsVm inmateApptDetails = new InmateApptDetailsVm
      //      {
      //          AoScheduleDetails = apptSchedule
      //      };

      //      //Act
      //      await _appointmentService.InsertSchedule(inmateApptDetails);

      //      //Assert 
      //      List<AoAppointmentVm> inmateApptList = _appointmentService.ListAoApointments(106, new DateTime(2018, 10, 1, 8, 0, 0), new DateTime(2018, 10, 31, 0, 0, 0), false);
      //      Assert.True(inmateApptList.Count > 0);

      //      AoAppointmentVm appt2 = inmateApptList[0];
      //      Assert.Equal(new DateTime(2018, 10, 1, 20, 0, 0), appt2.StartDate);
      //      Assert.Equal(new DateTime(2018, 10, 1, 21, 0, 0), appt2.EndDate);
      //  }

      //  [Fact]
      //  public async Task InsertScheduleSingle()
      //  {
      //      //Single Appointment
      //      ScheduleInmate singleAppt = new ScheduleInmate
      //      {
      //          InmateId = 107,
      //          StartDate = new DateTime(2018, 10, 1, 10, 0, 0),
      //          EndDate = new DateTime(2018, 10, 1, 11, 0, 0),
      //          LocationId = 7,
      //          IsSingleOccurrence = true,
      //          DayInterval = DayInterval.None
      //      };
      //      InmateApptDetailsVm inmateApptDetails = new InmateApptDetailsVm
      //      {
      //          AoScheduleDetails = singleAppt

      //      };
      //      //Act
      //      await _appointmentService.InsertSchedule(inmateApptDetails);

      //      // Multiple Appointment
      //      ScheduleInmate multiAppt = new ScheduleInmate
      //      {
      //          InmateId = 107,
      //          StartDate = new DateTime(2018, 10, 1, 13, 0, 0),
      //          EndDate = new DateTime(2018, 10, 31, 0, 0, 0),
      //          Duration = TimeSpan.FromHours(1),
      //          LocationId = 7,
      //          IsSingleOccurrence = false,
      //          DayInterval =DayInterval.Mon | DayInterval.Wed | DayInterval.Fri,
      //          FrequencyType = FrequencyType.Weekly
      //      };
      //      InmateApptDetailsVm inmateApptDetail = new InmateApptDetailsVm
      //      {
      //          AoScheduleDetails = multiAppt

      //      };

      //      //Act
      //      await _appointmentService.InsertSchedule(inmateApptDetail);

      //      //Schedule Count
      //      int scheduleCount = _appointmentService.AoScheduleCount(107);
      //      Assert.Equal(2, scheduleCount);

      //      //Appointment Count 
      //      List<AoAppointmentVm> inmateApptList = _appointmentService.ListAoApointments(107, new DateTime(2018, 10, 1, 13, 0, 0), new DateTime(2018, 10, 31, 0, 0, 0), false);
      //      Assert.True(inmateApptList.Count > 1);

      //      AoAppointmentVm appt1 = inmateApptList[0];
      //      Assert.Equal(new DateTime(2018, 10, 1, 10, 0, 0), appt1.StartDate);
      //      Assert.Equal(new DateTime(2018, 10, 1, 11, 0, 0), appt1.EndDate);


      //      AoAppointmentVm appt2 = inmateApptList[1];
      //      Assert.Equal(new DateTime(2018, 10, 2, 1, 0, 0), appt2.StartDate);
      //      Assert.Equal(new DateTime(2018, 10, 2, 2, 0, 0), appt2.EndDate);
      //  }

      ////  Passing different DateTime
      //  [Fact]
      //  public async Task InsertSchedule()
      //  {
      //      //Arrange
      //      ScheduleInmate apptSchedule = new ScheduleInmate
      //      {
      //          InmateId = 120,
      //          StartDate = new DateTime(2018, 10, 3, 8, 0, 0),
      //          EndDate = new DateTime(2018, 10, 3, 9, 0, 0),
      //          LocationId = 7,
      //          IsSingleOccurrence = true,
      //          DayInterval = DayInterval.None
      //      };
      //      InmateApptDetailsVm inmateApptDetail = new InmateApptDetailsVm
      //      {
      //          AoScheduleDetails = apptSchedule

      //      };

      //      //Act
      //      await _appointmentService.InsertSchedule(inmateApptDetail);

      //      //Arrange
      //      ScheduleInmate apptSchedule1 = new ScheduleInmate
      //      {
      //          InmateId = 120,
      //          StartDate = new DateTime(2018, 10, 4, 8, 0, 0),
      //          EndDate = new DateTime(2018, 10, 4, 9, 0, 0),
      //          LocationId = 7,
      //          IsSingleOccurrence = true,
      //          DayInterval = DayInterval.None
      //      };
      //      InmateApptDetailsVm inmateApptDetails = new InmateApptDetailsVm
      //      {
      //          AoScheduleDetails = apptSchedule1

      //      };

      //      //Act
      //      await _appointmentService.InsertSchedule(inmateApptDetails);

      //      //Assert
      //      List<AoAppointmentVm> inmateApptList = _appointmentService.ListAoApointments(120, new DateTime(2018, 10, 4, 8, 0, 0), new DateTime(2018, 10, 4, 9, 0, 0), false);

      //      AoAppointmentVm appt1 = inmateApptList[0];
      //      Assert.Equal(new DateTime(2018, 10, 4, 8, 0, 0), appt1.StartDate);
      //      Assert.Equal(new DateTime(2018, 10, 4, 9, 0, 0), appt1.EndDate);
      //  }

      //  [Fact]
      //  public void AppointmentService_GetAppointmentLocation()
      //  {
      //      //Act
      //      AppointmentLocationVm appointmentLocation = _appointmentService.GetAppointmentLocation(102, 1, ApptLocationFlag.None);

      //      //Assert
      //      List<KeyValuePair<int, string>> appointmenttype = appointmentLocation.AppointmentType;
      //      Assert.Equal(10, appointmenttype[1].Key);
      //      Assert.Equal("PRELIMINARY EXAMINATION", appointmenttype[1].Value);
      //      List<KeyValuePair<int, string>> appointmentreason = appointmentLocation.AppointmentReason;
      //      Assert.Equal(12, appointmentreason[0].Key);
      //      Assert.Equal("MENTAL HEALTH", appointmentreason[0].Value);
      //  }

      //  [Fact]
      //  public async Task UpdateSchedule()
      //  {
      //      ScheduleInmate aOSchedule = _fixture.Db.ScheduleInmate.Single(x => x.ScheduleId == 13);

      //      //Before Insertion
      //      Assert.Equal(7, aOSchedule.LocationId);

      //      //Arrange
      //      aOSchedule.LocationId = 6;
      //      aOSchedule.StartDate = DateTime.Now;
      //      aOSchedule.EndDate = DateTime.Now;

      //      InmateApptDetailsVm inmateApptDetails = new InmateApptDetailsVm
      //      {
      //          AoScheduleDetails = aOSchedule
      //      };

      //      //Act
      //      await _appointmentService.UpdateSchedule(inmateApptDetails);

      //      //After Insertion
      //      aOSchedule = _fixture.Db.ScheduleInmate.Single(x => x.ScheduleId == 13);
      //      //Assert
      //      Assert.Equal(6, aOSchedule.LocationId);
      //  }

      //  [Fact]
      //  public async Task ListAoApointmentsExcludingholidays()
      //  {
      //      //Arrange
      //      ScheduleInmate apptSchedule = new ScheduleInmate
      //      {
      //          InmateId = 109,
      //          StartDate = new DateTime(2018, 10, 1, 8, 0, 0),
      //          EndDate = new DateTime(2018, 10, 31, 0, 0, 0),
      //          Duration = TimeSpan.FromHours(1),
      //          LocationId = 7,
      //          IsSingleOccurrence = false,
      //          DayInterval = DayInterval.Mon | DayInterval.Wed | DayInterval.Fri,
      //          FrequencyType = FrequencyType.Weekly
      //      };
      //      InmateApptDetailsVm inmateApptDetails = new InmateApptDetailsVm
      //      {
      //          AoScheduleDetails = apptSchedule

      //      };

      //      //Act
      //      await _appointmentService.InsertSchedule(inmateApptDetails);

      //      //Check for holidays
      //      TemporalExpressionUnion holidays = new TemporalExpressionUnion();
      //      ScheduleFixedHoliday holiday1 = new ScheduleFixedHoliday(10, 03);
      //      ScheduleFixedHoliday holiday2 = new ScheduleFixedHoliday(10, 12);
      //      holidays.Add(holiday1);
      //      holidays.Add(holiday2);

      //      //Act
      //      List<AoAppointmentVm> inmateApptList = _appointmentService.ListAoApointmentsExcludingholidays(109, new DateTime(2018, 10, 1, 8, 0, 0), new DateTime(2018, 10, 31, 0, 0, 0), holidays);
      //      Assert.Equal(12, inmateApptList.Count);
      //  }

    }
}


