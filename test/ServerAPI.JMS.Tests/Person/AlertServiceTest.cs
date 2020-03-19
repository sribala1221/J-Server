using System;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Moq;
using ServerAPI.Services;
using ServerAPI.Tests;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GenerateTables.Models;
using Xunit;
using ServerAPI.ViewModels;
using ServerAPI.JMS.Tests.Helper;
using JwtDb.Models;
using Microsoft.AspNetCore.Identity;
using ServerAPI.Policies;

// ReSharper disable once CheckNamespace
namespace ServerAPI.JMS.Tests
{
    [Collection("Database collection")]
    public class AlertServiceTest
    {
        private readonly AlertService _alertService;
        private readonly DbInitialize _fixture;
        private readonly IConfigurationRoot _configuration = MockHelpers.GetIConfigurationRoot();
        private readonly Mock<IMemoryCache> _memory = new Mock<IMemoryCache>();

        public AlertServiceTest(DbInitialize fixture)
        {
            _fixture = fixture;
            Mock<UserManager<AppUser>> userMgr = MockHelpers.MockUserManager<AppUser>();
            Mock<RoleManager<IdentityRole>> roleMgr = MockHelpers.MockRoleManager<IdentityRole>();
            AppletsSavedService appletsSavedService = new AppletsSavedService(_fixture.Db, _configuration);
            HttpContextAccessor httpContext = new HttpContextAccessor { HttpContext = fixture.Context.HttpContext };
            PhotosService photosService = new PhotosService(fixture.Db, _configuration);
            UserPermissionPolicy userPermissionPolicy =
                new UserPermissionPolicy(fixture.Db, userMgr.Object, roleMgr.Object, httpContext);
            PersonService personService = new PersonService(_fixture.Db);
            InterfaceEngineService interfaceEngineService =
                new InterfaceEngineService(_fixture.Db, _configuration, _memory.Object);
            CommonService commonService = new CommonService(_fixture.Db, _configuration,
                httpContext, personService, appletsSavedService, interfaceEngineService);

            _alertService = new AlertService(fixture.Db, httpContext, commonService, personService,
                interfaceEngineService,
                userPermissionPolicy);
        }

        [Fact]
        public async Task InsertUpdateMessageAlertDetails()
        {
            //Arrange
            PersonAlertVm personAlert = new PersonAlertVm
            {
                PersonId = 70,
                AlertId = 8,
                AlertMessage = "CASE CLOSED"
            };
            //Before Update
            PersonAlert alert = _fixture.Db.PersonAlert.Single(a => a.PersonAlertId == 8);
            Assert.Equal("VOICE ALERT", alert.Alert);

            //Before Insert
            PersonAlertHistory alertHistory = _fixture.Db.PersonAlertHistory.SingleOrDefault(p =>
                p.PersonAlertId == 8);
            Assert.Null(alertHistory);

            //Act
            await _alertService.InsertUpdateMessageAlertDetails(personAlert);

            //Assert
            //After Update
            alert = _fixture.Db.PersonAlert.Single(a => a.PersonAlertId == 8);
            Assert.Equal("CASE CLOSED", alert.Alert);

            //After Insert
            alertHistory = _fixture.Db.PersonAlertHistory.Single(p =>
                p.PersonAlertId == 8);
            Assert.NotNull(alertHistory);
        }

        [Fact]
        public void GetMessageAlertDetailLst()
        {
            //Act
            List<PersonAlertVm> personAlert = _alertService.GetMessageAlertDetailLst(55);

            //Assert
            Assert.True(personAlert.Count > 0);

        }

        [Fact]
        public void GetMessageAlertHistoryLst()
        {
            //Act
            List<HistoryVm> lstHistory = _alertService.GetMessageAlertHistoryLst(7);

            //Assert
            Assert.True(lstHistory.Count > 0);

        }

        [Fact]
        public void GetAssocAlertDetails()
        {
            //Act
            List<PersonClassificationDetails> lstPersonClassification = _alertService.GetAssocAlertDetails(75);

            //Assert
            PersonClassificationDetails personClassification =
                lstPersonClassification.Single(p => p.PersonClassificationId == 9);
            Assert.Equal("LEADERSHIP", personClassification.ClassificationStatus);
            Assert.Equal("ALL LOCAL AREAS", personClassification.ClassificationNotes);
        }

        [Fact]
        public void GetAssocHistoryDetails()
        {
            //Act
            List<AssocAlertHistoryVm> lstHistoryVms = _alertService.GetAssocHistoryDetails(9);

            //Assert
            Assert.True(lstHistoryVms.Count > 1);
        }

        [Fact]
        public async Task InsertUpdateAssocDetails()
        {
            //Arrange
            PersonClassificationDetails personClassificationDetails = new PersonClassificationDetails
            {
                CreateDate = DateTime.Now.AddDays(-10),
                PersonId = 70,
                PersonClassificationId = 7,
                ClassificationFlag = new[]
                {
                    10, 11
                },
                ClassifyHistoryList =
                    "{ 'Association':'BLOOD','Association status':'ASSOCIATE','Start date':'05/20/2019'}"
            };

            //Before Update
            PersonClassification personClassification = _fixture.Db.PersonClassification.SingleOrDefault(
                p => p.PersonId == 70);
            Assert.Null(personClassification);

            //Act
            await _alertService.InsertUpdateAssocDetails(personClassificationDetails);

            //Assert
            //After Update
            personClassification = _fixture.Db.PersonClassification.SingleOrDefault(
                p => p.PersonId == 70);

            Assert.NotNull(personClassification);

        }

        [Fact]
        public async Task DeleteAssocDetails()
        {
            //Arrange
            PersonClassificationDetails personClassificationDetails = new PersonClassificationDetails
            {
                CreateDate = DateTime.Now,
                PersonId = 130,
                ClassificationType = "FIVE-ASIANS",
                ClassificationTypeId = 8
            };

            //Act
            AssocAlertValidation assocAlertValidation =
                await _alertService.DeleteAssocDetails(personClassificationDetails);

            //Assert
            Assert.True(assocAlertValidation.ErrorMessage == AssocErrorMessage.AssocAssigned);

        }

        [Fact]
        public void GetPrivilegeDetails()
        {
            //Act
            List<PrivilegeAlertVm> lstPrivilegeAlert = _alertService.GetPrivilegeDetails(105, 0);

            //Assert
            Assert.True(lstPrivilegeAlert.Count > 0);
            PrivilegeAlertVm privilegeAlert = lstPrivilegeAlert.Single(p => p.InmatePrivilegeXrefId == 10);
            Assert.Equal("TRICHY", privilegeAlert.PrivilegeDescription);
        }

        [Fact]
        public async Task InsertOrUpdatePrivilegeInfo()
        {
            //Arrange
            PrivilegeAlertVm privilegeAlert = new PrivilegeAlertVm
            {
                InmatePrivilegeXrefId = 11,
                PrivilegeId = 8,
                PrivilegeDate = DateTime.Now,
                PrivilegeXrefHistoryList =
                    @"{'INMATE':'ACOSTA,ANTONIO NMN AWB321','START DATE':'06/19/2018 14:06','NEVER':'','THRU DATE':'06/27/2018 23:59','LINK TO INCIDENT':'17001029 NaN ESCAPE','NOTES':'second'}",
                PrivilegeExpires = DateTime.Now
            };
            //Before update InmatePrivilegeXref table
            InmatePrivilegeXref inmatePrivilegeXref =
                _fixture.Db.InmatePrivilegeXref.Single(i => i.InmatePrivilegeXrefId == 11);
            Assert.Equal(7, inmatePrivilegeXref.PrivilegeId);

            //Before insert history table
            InmatePrivilegeXrefHistory inmatePrivilegeXrefHistory =
                _fixture.Db.InmatePrivilegeXrefHistory.SingleOrDefault(i => i.InmatePrivilegeXrefId == 11);
            Assert.Null(inmatePrivilegeXrefHistory);

            //Act
            await _alertService.InsertOrUpdatePrivilegeInfo(privilegeAlert);

            //Assert
            //After update InmatePrivilegeXref table
            inmatePrivilegeXref =
                _fixture.Db.InmatePrivilegeXref.Single(i => i.InmatePrivilegeXrefId == 11);
            Assert.Equal(8, inmatePrivilegeXref.PrivilegeId);

            //After insert history table
            inmatePrivilegeXrefHistory =
                _fixture.Db.InmatePrivilegeXrefHistory.Single(i => i.InmatePrivilegeXrefId == 11);
            Assert.NotNull(inmatePrivilegeXrefHistory);

        }

        [Fact]
        public void GetPrivilegeIncidentDetails()
        {
            //Act
            PrivilegeIncidentDetailsVm privilegeIncidentDetails = _alertService.GetPrivilegeIncidentDetails(103, 1, false);

            //Assert
            Assert.True(privilegeIncidentDetails.DisciplinaryIncidentList.Count > 0);
            Assert.True(privilegeIncidentDetails.PrivilegeList.Count > 0);
            Assert.True(privilegeIncidentDetails.PrivilegeLookupList.Count > 1);
        }

        [Fact]
        public void GetDisciplinaryIncidentList()
        {
            //Act
            List<DisciplinaryIncidentDetails> lstDisciplinaryIncidentDetails =
                _alertService.GetDisciplinaryIncidentList(100);

            //Assert
            Assert.True(lstDisciplinaryIncidentDetails.Count > 0);
        }

        [Fact]
        public void GetPrivilegeHistoryDetails()
        {
            //Act
            List<HistoryVm> lstHistory = _alertService.GetPrivilegeHistoryDetails(9);

            //Assert
            Assert.True(lstHistory.Count > 0);
        }

        [Fact]
        public void GetObservationLog()
        {
            //Act
            ObservationVm observation = _alertService.GetObservationLog(120, 1);

            //Assert
            Assert.True(observation.AlertObservationLogs.Count > 0);
            Assert.True(observation.LstActiveSchedule.Count > 0);
            Assert.True(observation.LstInactiveSchedule.Count > 0);
        }

        [Fact]
        public void GetObservationHistory()
        {
            //Act
            List<HistoryVm> lstHistory = _alertService.GetObservationHistory(10);

            //Assert
            Assert.True(lstHistory.Count > 0);

        }

        [Fact]
        public async Task InsertObservationScheduleEntryDetails()
        {
            //Arrange
            ObservationScheduleVm observationSchedule = new ObservationScheduleVm
            {
                InmateId = 105,
                StartDate = DateTime.Now.AddDays(-15),
                EndDate = DateTime.Now,
                Note = "NEW SCHEDULE",
                ObservationType = 5,
                HistoryList =
                    @"{'Inmate':'AMAYA, AMAYA F BDS428','Undo Deleted':'Observation Schedule Record Undo Deleted'}"
            };

            //Before Insert
            ObservationSchedule schedule = _fixture.Db.ObservationSchedule.SingleOrDefault(o => o.InmateId == 105);

            Assert.Null(schedule);

            //Act
            await _alertService.InsertObservationScheduleEntryDetails(observationSchedule);

            //Assert
            //After Insert
            schedule = _fixture.Db.ObservationSchedule.Single(o => o.InmateId == 105);

            Assert.NotNull(schedule);

        }

        [Fact]
        public async Task UpdateObservationScheduleEntryDetails()
        {
            //Arrange
            ObservationScheduleVm observationSchedule = new ObservationScheduleVm
            {
                ObservationScheduleId = 12,
                Note = "MEDICAL REPORT",
                HistoryList =
                    @"{ 'Inmate':'task, task test IX 0010014','Type':'MENTAL HEALTH','Start date':'08/20/2019 13:58','End date':'08/20/2019 23:59'}"
            };

            //Before Update
            ObservationSchedule schedule = _fixture.Db.ObservationSchedule.Single(o =>
                o.ObservationScheduleId == 12);
            Assert.Equal("REPORT", schedule.Note);

            //Before Insert
            ObservationScheduleHistory observationScheduleHistory =
                _fixture.Db.ObservationScheduleHistory.SingleOrDefault(
                    o => o.ObservationScheduleId == 12);
            Assert.Null(observationScheduleHistory);

            //Act
            await _alertService.UpdateObservationScheduleEntryDetails(observationSchedule);

            //Assert
            //After Update
            schedule = _fixture.Db.ObservationSchedule.Single(o =>
                o.ObservationScheduleId == 12);
            Assert.Equal("MEDICAL REPORT", schedule.Note);

            //After Insert
            observationScheduleHistory = _fixture.Db.ObservationScheduleHistory.
                Single(o => o.ObservationScheduleId == 12);
            Assert.NotNull(observationScheduleHistory);

        }

        [Fact]
        public async Task DeleteUndoObservationScheduleEntry()
        {
            //Arrange
            ObservationScheduleVm observationSchedule = new ObservationScheduleVm
            {
                ObservationScheduleId = 10,
                HistoryList = @"{'Inmate':'Dinesh, Dinesh 1900102840','Type':'MEDICAL','Start date':'08/09/2019 09:44'}"
            };

            //Before Delete
            ObservationSchedule schedule = _fixture.Db.ObservationSchedule.Single(o => o.ObservationScheduleId == 10);
            Assert.Null(schedule.DeleteBy);

            //Act
            await _alertService.DeleteUndoObservationScheduleEntry(observationSchedule);

            //Assert
            //After Delete
            schedule = _fixture.Db.ObservationSchedule.Single(o => o.ObservationScheduleId == 10);
            Assert.Equal(11, schedule.DeleteBy);
        }

        [Fact]
        public void GetObservationLookupDetails()
        {
            //Act
            ObservationLookupDetails observationLookupDetails = _alertService.GetObservationLookupDetails();

            //Assert
            Assert.True(observationLookupDetails.ListLookup.Count > 0);
            Assert.True(observationLookupDetails.ListObservationPolicy.Count > 0);
        }

        [Fact]
        public async Task UpdateScheduleActionNote()
        {
            //Arrange
            ObservationScheduleActionVm observationScheduleAction = new ObservationScheduleActionVm
            {
                ObservationScheduleActionId = 12,
                ObservationScheduleNote = "DUE TO ANOTHER SCHEDULE"
            };

            //Before Update
            ObservationScheduleAction observationSchedule = _fixture.Db.ObservationScheduleAction.Single(
                a => a.ObservationScheduleActionId == 12);
            Assert.Equal("NEW SCHEDULE", observationSchedule.ObservationScheduleNote);


            //Act
            await _alertService.UpdateScheduleActionNote(observationScheduleAction);

            //Assert
            //After Update
            observationSchedule = _fixture.Db.ObservationScheduleAction.Single(
                a => a.ObservationScheduleActionId == 12);
            Assert.Equal("DUE TO ANOTHER SCHEDULE", observationSchedule.ObservationScheduleNote);

        }

        [Fact]
        public void LoadObservationLogDetail()
        {
            //Act
            ObservationLogItemDetails observationLog = _alertService.LoadObservationLogDetail(9);

            //Assert
            Assert.Equal("HEALTH CHECK", observationLog.ObservationActionName);

        }

        [Fact]
        public void GetAlerts()
        {
            //Act
            List<AlertFlagsVm> lstAlertFlags = _alertService.GetAlerts(50, 105);

            //Assert
            Assert.True(lstAlertFlags.Count > 3);
        }

        [Fact]
        public void GetMedicalAlerts()
        {
            //Act
            List<AlertFlagsVm> lstAlertFlags = _alertService.GetMedicalAlerts(100);

            //Assert
            Assert.True(lstAlertFlags.Count > 0);
        }

        [Fact]
        public void GetPrivilegesAlert()
        {
            //Act
            List<PrivilegeDetailsVm> lstPrivilegeDetails = _alertService.GetPrivilegesAlert(130);

            //Assert
            PrivilegeDetailsVm privilegeAlert = lstPrivilegeDetails.Single(p => p.PrivilegeId == 7);
            Assert.Equal("AUTH", privilegeAlert.PrivilegeType);
        }

    }
}
