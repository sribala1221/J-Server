using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GenerateTables.Models;
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
using Xunit;

// ReSharper disable once CheckNamespace
namespace ServerAPI.JMS.Tests
{
    [Collection("Database collection")]
    public class RegisterServiceTest
    {

        private readonly RegisterService _registerService;
        private readonly IConfigurationRoot _configuration = MockHelpers.GetIConfigurationRoot();
        private readonly DbInitialize _fixture;
        private readonly Mock<IMemoryCache> _memory = new Mock<IMemoryCache>();
        public RegisterServiceTest(DbInitialize fixture)
        {
            _fixture = fixture;
            HttpContextAccessor httpContext = new HttpContextAccessor { HttpContext = fixture.Context.HttpContext };
            AppletsSavedService appletsSavedService = new AppletsSavedService(_fixture.Db, _configuration);
            Mock<UserManager<AppUser>> userMgr = MockHelpers.MockUserManager<AppUser>();
            Mock<RoleManager<IdentityRole>> roleMgr = MockHelpers.MockRoleManager<IdentityRole>();
            UserPermissionPolicy userPermission =
                new UserPermissionPolicy(_fixture.Db, userMgr.Object, roleMgr.Object, httpContext);
            PersonService personService = new PersonService(fixture.Db);
            AtimsHubService atimsHubService = new AtimsHubService(_fixture.HubContext.Object);
            PhotosService photosService = new PhotosService(fixture.Db, _configuration);
            InterfaceEngineService interfaceEngineService = new InterfaceEngineService(_fixture.Db,
                _configuration, _memory.Object);
            CommonService commonService = new CommonService(_fixture.Db, _configuration,
                httpContext, personService, appletsSavedService, interfaceEngineService);
            WizardService wizardService = new WizardService(_fixture.Db,atimsHubService);
            FacilityHousingService facilityHousingService = new FacilityHousingService(_fixture.Db, _configuration,
                httpContext);
            KeepSepAlertService keepSepAlertService = new KeepSepAlertService(_fixture.Db, httpContext, 
                facilityHousingService,interfaceEngineService);
            PersonAkaService personAkaService = new PersonAkaService(_fixture.Db, commonService, httpContext, personService,
                interfaceEngineService);
            PrebookActiveService prebookActiveService = new PrebookActiveService(_fixture.Db, httpContext, atimsHubService);
            PersonCharService personCharService = new PersonCharService(_fixture.Db, commonService, httpContext, personService,
                interfaceEngineService);
            PersonIdentityService personIdentityService = new PersonIdentityService(_fixture.Db,
                httpContext, prebookActiveService, personAkaService, personService,interfaceEngineService);
            _registerService = new RegisterService(_fixture.Db, httpContext, commonService, personCharService,
                photosService, wizardService, personIdentityService, keepSepAlertService, userPermission);
        }


        //AppointmentConflictCheck 
        //[Fact]
        //public void CheckScheduleConflict()
        //{
        //    //Arrange
        //    DateTime? startDate = DateTime.Now.Date.AddHours(2).AddMinutes(15);
        //    DateTime? endDate = DateTime.Now.Date.AddHours(3).AddMinutes(15);

        //    //Act
        //    AppointmentConflictCheck appointmentConflictCheck = _registerService.CheckScheduleConflict
        //        (103, startDate, endDate, 7);

        //    //Assert
        //    //Getting conflict
        //    Assert.True(appointmentConflictCheck.ApptConflictDetails.Count > 0);
        //    AppointmentConflictDetails conflictDetails = appointmentConflictCheck.ApptConflictDetails.Single(a => a.ApptId == 8);
        //    Assert.Equal("VISIT SOCIAL CONFLICT", conflictDetails.ConflictType);
        //    Assert.Equal("HIERACHY LEVEL 1", appointmentConflictCheck.ApptBumpList.HierarchyName);
        //    Assert.Equal("TRICHY", appointmentConflictCheck.ApptLocation);
        //}


        //[Fact]
        //public async Task InsertVisitationConflict()
        //{
        //    //Act
        //    KeyValuePair<int, string> keyValuePair = new KeyValuePair<int, string>(140, "NMATE GENDER CONFLICT");

        //    //Before insert
        //    FloorNoteXref floorNoteXref = _fixture.Db.FloorNoteXref.SingleOrDefault(f => f.InmateId == 140);
        //    Assert.Null(floorNoteXref);

        //    //Act
        //    await _registerService.InsertVisitationConflict(keyValuePair);

        //    //Assert
        //    //After Insert
        //    floorNoteXref = _fixture.Db.FloorNoteXref.Single(f => f.InmateId == 140);
        //    Assert.NotNull(floorNoteXref);
        //}

        //[Fact]
        //public async Task InsertUpdateVisitorDetails()
        //{
        //    //Arrange
        //    RecordsVisitationVm recordsVisitation = new RecordsVisitationVm
        //    {
        //        InmateId = 109,
        //        VisitorId = 11,
        //        VisitorLocationId = 5,
        //        PersonId = 110,
        //        VisitorListId = 12,
        //        VisitorDenyReason = "NO IDENTIFICATION",
        //        VisitorBadgenumber = 0,
        //        VisitorDenyFlag = 0,
        //        VisitorIdNumber = "PV104210",
        //        VisitorIdState = "TN",
        //        VisitorIdType = "OTHER",
        //        VisitorHistoryValue = "{'Inmate':'ABILA, WE ADD227','Personal ID':'','Reject all':'No','Person of interest':'No','Personal visitor':'test, teat'}"
        //    };

        //    //Before Update
        //    Person person = _fixture.Db.Person.Single(p => p.PersonId == 95);
        //    Assert.Null(person.PersonOtherIdType);

        //    //Before Insert
        //    VisitorHistory visitorHistory = _fixture.Db.VisitorHistory.SingleOrDefault(v => v.VisitorId == 12);
        //    Assert.Null(visitorHistory);

        //    //Before Update
        //  // Visitor visit = _fixture.Db.Visitor.Single(v => v.VisitorId == 12);
        //    //Assert.Null(visit.VisitorIdState);

        //    //Act
        //    await _registerService.InsertUpdateVisitorDetails(recordsVisitation);

        //    //Assert
        //    //After Update
        //    person = _fixture.Db.Person.Single(p => p.PersonId == 110);
        //    Assert.Equal("OTHER", person.PersonOtherIdType);

        //    //After Insert
        //    visitorHistory = _fixture.Db.VisitorHistory.SingleOrDefault(v => v.VisitorId == 12);
        //    Assert.NotNull(visitorHistory);

        //    //After Update
        //   // visit = _fixture.Db.Visitor.Single(v => v.VisitorId == 12);
        //  //  Assert.Equal("TN", visit.VisitorIdState);

        //}

        //[Fact]
        //public async Task DeleteVisitorDetails()
        //{
        //Before delete
        //    Visit visit = _fixture.Db.Visit.Single(v => v.VisitId == 12);
        //    Assert.Null(visit.VisitDeleteFlag);

        //    //Act
        //    await _registerService.DeleteVisitorDetails(12);

        //    //Assert
        //    //After delete
        //    visit = _fixture.Db.Visit.Single(v => v.VisitId == 12);
        //    Assert.Equal(1, visit.VisitDeleteFlag);
        // }

        //[Fact]
        //public void GetVisitorList()
        //{
        //    //Arrange
        //    SearchRegisterDetails searchRegisterDetails = new SearchRegisterDetails
        //    {
        //        FacilityId = 2
        //    };

        //    //Act
        //    List<RegisterDetails> lstRegisterDetails = _registerService.GetVisitorList(searchRegisterDetails);

        //    //Assert
        //    RegisterDetails registerDetails = lstRegisterDetails.Single(r => r.PersonId == 60);
        //    Assert.Equal("TRICHY", registerDetails.VisitorLocation);
        //    Assert.Equal(6, registerDetails.VisitorListId);
        //}

        //[Fact]
        //public void GetScheduleList()
        //{
        //    //Act
        //    InmateScheduleDetails inmateSchedule = _registerService.GetScheduleList(104);

        //    //Assert
        //    Assert.Equal(12, inmateSchedule.HousingDetail.HousingUnitId);
        //    Assert.Equal("Madras Central Jail", inmateSchedule.HousingDetail.FacilityName);
        //    Assert.True(inmateSchedule.ScheduleList.Count > 1);
        //}

        //[Fact]
        //public void GetExistingVisitDetails()
        //{
        //    _registerService.GetExistingVisitDetails(145);
        //}

    }
}
