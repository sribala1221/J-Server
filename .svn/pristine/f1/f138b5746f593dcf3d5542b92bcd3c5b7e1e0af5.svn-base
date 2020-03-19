using JwtDb.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Moq;
using ServerAPI.JMS.Tests.Helper;
using ServerAPI.Services;
using ServerAPI.Tests;
using System;
using System.Collections.Generic;
using System.Linq;
using GenerateTables.Models;
using ServerAPI.ViewModels;
using Xunit;

// ReSharper disable once CheckNamespace
namespace ServerAPI.JMS.Tests
{
    [Collection("Database collection")]
    public class InvestigationServiceTest
    {
        private readonly InvestigationService _investigationService;
        private readonly IConfigurationRoot _configuration = MockHelpers.GetIConfigurationRoot();
        private readonly DbInitialize _fixture;
        private readonly Mock<IMemoryCache> _memory = new Mock<IMemoryCache>();

        public InvestigationServiceTest(DbInitialize fixture)
        {
            _fixture = fixture;
            Mock<UserManager<AppUser>> userMgr = MockHelpers.MockUserManager<AppUser>();
            HttpContextAccessor httpContext = new HttpContextAccessor { HttpContext = fixture.Context.HttpContext };
            PersonService personService = new PersonService(fixture.Db);
            PhotosService photosService = new PhotosService(_fixture.Db, _configuration);
            AppletsSavedService appletsSavedService = new AppletsSavedService(_fixture.Db, _configuration);
            InterfaceEngineService interfaceEngineService = new InterfaceEngineService(fixture.Db, _configuration,
                _memory.Object);
            CommonService commonService = new CommonService(_fixture.Db, _configuration, httpContext, personService, 
                appletsSavedService, interfaceEngineService);
            FacilityPrivilegeService facilityPrivilegeService = new FacilityPrivilegeService(fixture.Db, httpContext,
                personService);
            AtimsHubService atimsHubService = new AtimsHubService(_fixture.HubContext.Object);

            InmateService inmateService = new InmateService(_fixture.Db, commonService, personService, httpContext,
                facilityPrivilegeService, photosService);
            FacilityHousingService facilityHousingService = new FacilityHousingService(_fixture.Db, _configuration,
                httpContext);
            RecordsCheckService recordsCheckService = new RecordsCheckService(_fixture.Db, commonService, httpContext);

            BookingReleaseService bookingReleaseService = new BookingReleaseService(
                _fixture.Db, commonService, httpContext, personService, inmateService, recordsCheckService,
                facilityHousingService, interfaceEngineService);
            AltSentService altSentService = new AltSentService(_fixture.Db, commonService, httpContext, photosService,
                personService);
            CellService cellService = new CellService(fixture.Db, commonService, httpContext,
                facilityPrivilegeService, facilityHousingService, _fixture.JwtDb, userMgr.Object);

            PrebookWizardService prebookWizardService = new PrebookWizardService(_fixture.Db,
                _configuration, commonService, cellService, httpContext, atimsHubService,
                personService, inmateService, bookingReleaseService, altSentService, interfaceEngineService,
                photosService, appletsSavedService);
            WizardService wizardService = new WizardService(_fixture.Db, atimsHubService);
            SentenceService sentenceService = new SentenceService(_fixture.Db, commonService, httpContext, personService,
                interfaceEngineService);
            PrebookActiveService prebookActiveService = new PrebookActiveService(_fixture.Db, httpContext, atimsHubService);
            BookingService bookingService = new BookingService(_fixture.Db, commonService, httpContext,
                bookingReleaseService, prebookWizardService, prebookActiveService, sentenceService, wizardService,
                personService, inmateService, photosService, interfaceEngineService);
            _investigationService = new InvestigationService(_fixture.Db, httpContext, commonService, personService,
                bookingService);
        }

        [Fact]
        public void GetInvestigations()
        {
            //Arrange
            InvestigationInputs investigationInputs = new InvestigationInputs
            {
                FromDate = DateTime.Now,
                ToDate = DateTime.Now,
                Range = InvstRange.active

            };

            //Act
            InvestigationDataVm investigationData = _investigationService.GetInvestigations(investigationInputs);

            //Assert
            Assert.True(investigationData.Investigation.Count > 1);
            Assert.True(investigationData.Lookups.Count > 5);
            Assert.True(investigationData.Personnels.Count > 3);
        }


        [Fact]
        public void InsertUpdateInvestigation()
        {
            //Arrange
            InvestigationVm investigationValuse = new InvestigationVm
            {
                DeleteFlag = false,
                InvestigationName = "KARTHICK",
                InvestigationNumber = "1990",
                InvestigationStatus = 2,
                OfficerId = 12,
                CompleteFlag = false,
                StartDate = DateTime.Now,
                InvestigationType = 5,
                InvestigationSummary = "NEW INVESTIGATION"
            };
            //Before Insert
            Investigation investigation = _fixture.Db.Investigation.SingleOrDefault(i => i.InvestigationName == "KARTHICK");
            Assert.Null(investigation);

            //Act
            _investigationService.InsertUpdateInvestigation(investigationValuse);

            //Assert
            //After Insert
            investigation = _fixture.Db.Investigation.Single(i => i.InvestigationName == "KARTHICK");
            Assert.NotNull(investigation);

        }

        [Fact]
        public void InsertUpdateInvestigationFlags()
        {
            //Arrange
            InvestigationFlag investigationFlag = new InvestigationFlag
            {
                InvestigationFlagIndex = 5,
                InvestigationId = 11
            };
            //Before Update
            InvestigationFlags flags = _fixture.Db.InvestigationFlags.SingleOrDefault(i => i.InvestigationId == 11);
            Assert.Null(flags);

            //Act
            _investigationService.InsertUpdateInvestigationFlags(investigationFlag);

            //Assert
            //After Update
            flags = _fixture.Db.InvestigationFlags.Single(i => i.InvestigationId == 11);
            Assert.NotNull(flags);
        }

        [Fact]
        public void InsertUpdateInvestigationPersonnel()
        {
            //Arrange
            InvestigationPersonnelVm personnel = new InvestigationPersonnelVm
            {
                InvestigationId = 12,
                PersonnelId = 12,
                DeleteFlag = false,
                ContributerFlag = false,
                NamedOnlyFlag = true,
                ViewerFlag = false,
                PersonnelNote = "OUT OF CUSTODY"
            };
            //Before Update
            InvestigationPersonnel investigationPersonnel = _fixture.Db.InvestigationPersonnel.SingleOrDefault(
                i => i.InvestigationId == 12);
            Assert.Null(investigationPersonnel);

            //Act
            _investigationService.InsertUpdateInvestigationPersonnel(personnel);

            //Assert
            //After Update
            investigationPersonnel = _fixture.Db.InvestigationPersonnel.Single(
                i => i.InvestigationId == 12);
            Assert.NotNull(investigationPersonnel);
        }

        [Fact]
        public void InsertUpdateInvestigationNotes()
        {
            //For Insert
            //Arrange
            InvestigationNotesVm notes = new InvestigationNotesVm
            {
                InmateId = 106,
                InvestigationFlagId = 12,
                DeleteFlag = false,
                InvestigationId = 10
            };
            //Before Insert
            InvestigationNotes investigationNotes =
                _fixture.Db.InvestigationNotes.SingleOrDefault(i => i.InmateId == 106);
            Assert.Null(investigationNotes);

            //Act
            _investigationService.InsertUpdateInvestigationNotes(notes);

            //Assert
            //After Insert
            investigationNotes =
                _fixture.Db.InvestigationNotes.Single(i => i.InmateId == 106);
            Assert.NotNull(investigationNotes);

            //For Update 
            //Arrange
            notes = new InvestigationNotesVm
            {
                InvestigationNotesId = 11,
                DeleteFlag = true,
                InvestigationId = 11,
                InmateId = 102,
                InvestigationFlagId = 11,
                InvestigationNotes = "NEW REPORT"
            };
            //Before Update
            investigationNotes =
               _fixture.Db.InvestigationNotes.Single(i => i.InvestigationNotesId == 11);
            Assert.False(investigationNotes.DeleteFlag);
            Assert.Null(investigationNotes.InvestigationNote);

            //Act
            _investigationService.InsertUpdateInvestigationNotes(notes);

            //Assert
            //After Update
            investigationNotes =
                _fixture.Db.InvestigationNotes.Single(i => i.InvestigationNotesId == 11);
            Assert.True(investigationNotes.DeleteFlag);
            Assert.Equal("NEW REPORT", investigationNotes.InvestigationNote);
        }


        [Fact]
        public void InsertUpdateInvestigationIncident()
        {
            //For Insert
            //Arrange
            InvestigationIncident investigationIncident = new InvestigationIncident
            {
                InvestigationId = 10,
                DeleteFlag = false,
                DisciplinaryIncidentId = 8
            };

            //Before Insert
            InvestigationToIncident investigationToIncident =
                _fixture.Db.InvestigationToIncident.SingleOrDefault(i => i.DisciplinaryIncidentId == 8);
            Assert.Null(investigationToIncident);

            //Act
            _investigationService.InsertUpdateInvestigationIncident(investigationIncident);

            //Assert
            //After Insert
            investigationToIncident =
                _fixture.Db.InvestigationToIncident.Single(i => i.DisciplinaryIncidentId == 8);
            Assert.NotNull(investigationToIncident);


            //For Update
            //Arrange
            investigationIncident = new InvestigationIncident
            {
                InvestigationId = 10,
                DeleteFlag = false,
                InvestigationToIncidentId = 21,
                DisciplinaryIncidentId = 9
            };
            //Before Update
            investigationToIncident =
               _fixture.Db.InvestigationToIncident.Single(i => i.InvestigationToIncidentId == 21);
            Assert.Equal(10, investigationToIncident.DisciplinaryIncidentId);

            //Act
            _investigationService.InsertUpdateInvestigationIncident(investigationIncident);

            //Assert
            //After Update
            investigationToIncident =
                _fixture.Db.InvestigationToIncident.Single(i => i.InvestigationToIncidentId == 21);
            Assert.Equal(9, investigationToIncident.DisciplinaryIncidentId);
        }

        [Fact]
        public void InsertUpdateInvestigationLink()
        {
            //Arrange
            InvestigationLinkVm investigationLink = new InvestigationLinkVm
            {
                InmateIds = new[] { 102, 105 },
                LinkTypeId = 3,
                InvestigationId = 10
            };
            //Before Insert
            InvestigationLinkXref linkXref = _fixture.Db.InvestigationLinkXref.SingleOrDefault(
                i => i.InmateId == 105);
            Assert.Null(linkXref);

            //Act
            _investigationService.InsertUpdateInvestigationLink(investigationLink);

            //Assert
            //After Insert
            linkXref = _fixture.Db.InvestigationLinkXref.SingleOrDefault(
                i => i.InmateId == 105);
            Assert.NotNull(linkXref);
        }

        [Fact]
        public void GetInvestigationIncidents()
        {
            //Act
            List<KeyValuePair<int, string>> result = _investigationService.GetInvestigationIncidents();

            //Assert
            Assert.True(result.Count > 2);
        }

        [Fact]
        public void GetInvestigationGrievance()
        {
            //Act
            List<KeyValuePair<int, string>> result = _investigationService.GetInvestigationGrievance();

            //Assert
            Assert.True(result.Count > 5);
        }

        [Fact]
        public void InsertUpdateInvestigationGrievance()
        {
            //Arrange
            InvestigationIncident investigationIncident = new InvestigationIncident
            {
                DisciplinaryIncidentId = 10,
                InvestigationToIncidentId = 16,
                InvestigationId = 10
            };
            //Before Update
            InvestigationToGrievance grievance =
                _fixture.Db.InvestigationToGrievance.Single(i => i.InvestigationToGrievanceId == 16);
            Assert.Equal(5, grievance.GrievanceId);

            //Act
            _investigationService.InsertUpdateInvestigationGrievance(investigationIncident);

            //Assert
            //After Update
            grievance = _fixture.Db.InvestigationToGrievance.Single(i => i.InvestigationToGrievanceId == 16);
            Assert.Equal(10, grievance.GrievanceId);
        }

        [Fact]
        public void GetInvestigationAllDetails()
        {
            //Act
            InvestigationAllDetails allDetails = _investigationService.GetInvestigationAllDetails(11);

            //Assert
            Assert.True(allDetails.InvestigationLink.Count > 0);
            Assert.True(allDetails.InvestigationNotes.Count > 0);
            Assert.True(allDetails.InvestigationPersonnel.Count > 0);

        }

        [Fact]
        public void GetInvestigationForms()
        {
            //Act
            List<IncarcerationForms> lstInvestigationForms = _investigationService.GetInvestigationForms(10);

            //Assert
            Assert.True(lstInvestigationForms.Count > 1);
        }

        [Fact]
        public void DeleteInvestigationAttachment()
        {
            //Before Delete
            AppletsSaved appletsSaved = _fixture.Db.AppletsSaved.Single(a => a.AppletsSavedId == 21);
            Assert.Null(appletsSaved.DeletedBy);
            Assert.Equal(0, appletsSaved.AppletsDeleteFlag);

            //Act
            _investigationService.DeleteInvestigationAttachment(21);

            //Assert
            //After Delete
            appletsSaved = _fixture.Db.AppletsSaved.Single(a => a.AppletsSavedId == 21);
            Assert.NotNull(appletsSaved.DeletedBy);
            Assert.Equal(1, appletsSaved.AppletsDeleteFlag);
        }

        [Fact]
        public void DeleteInvestigationForms()
        {
            //Before Delete
            FormRecord formRecord = _fixture.Db.FormRecord.Single(f => f.FormRecordId == 15);
            Assert.Equal(0, formRecord.DeleteFlag);

            //Act
            _investigationService.DeleteInvestigationForms(15);


            //Assert
            //After Delete
            formRecord = _fixture.Db.FormRecord.Single(f => f.FormRecordId == 15);
            Assert.Equal(1, formRecord.DeleteFlag);
        }

        [Fact]
        public void UpdateInvestigationComplete()
        {
            //Arrange
            InvestigationVm investigationValues = new InvestigationVm
            {
                InvestigationId = 13,
                CompleteFlag = false,
                CompleteDisposition = 3,
                CompleteReason = "NO DATA",
                InvestigationHistory =
                @"{'Investigation name':'Material icons are delightful, beautifully crafted','Source referral':null,'Source department':'ABC','Investigation type':'ADMINITRATIVE','Investigation status':'ACTIVE','Investigation summary':'Material icons are delightful, beautifully crafted symbols for common actions and items. Download on desktop to use them in your digital products for Android, iOS, and webMaterial icons are delightful, beautifully crafted symbols for common actions and items. Download on desktop to use them in your digital products for Android, iOS, and webMaterial icons are delightful, beautifully crafted symbols for common actions and items. Download on desktop to use them in your digital products for Android,','Start date':'2019-10-01T01:00:30','Manager':1,'Complete flag':false,'Complete date':null,'Complete reason':'Deletedddd','Receive date':'2019-09-04T12:00:37','Delete flag':null,'Delete date':null}"
            };
            //Before Insert history table
            InvestigationHistory investigationHistory = _fixture.Db.InvestigationHistory.SingleOrDefault(
                i => i.InvestigationId == 13);
            Assert.Null(investigationHistory);

            //Before Update
            Investigation investigation = _fixture.Db.Investigation.Single(i => i.InvestigationId == 13);
            Assert.Null(investigation.CompleteDisposition);

            //Act
            _investigationService.UpdateInvestigationComplete(investigationValues);

            //Assert
            //After Insert history table
            investigationHistory = _fixture.Db.InvestigationHistory.SingleOrDefault(
                i => i.InvestigationId == 13);
            Assert.NotNull(investigationHistory);

            //After Update
            investigation = _fixture.Db.Investigation.Single(i => i.InvestigationId == 13);
            Assert.Equal(3, investigation.CompleteDisposition);
        }

        [Fact]
        public void DeleteInvestigation()
        {
            //Arrange
            InvestigationVm values = new InvestigationVm
            {
                InvestigationId = 14,
                InvestigationHistory =
                    @"{'Investigation name':'Material icons are delightful, beautifully crafted','Source referral':null,'Source department':'ABC','Investigation type':'ADMINITRATIVE','Investigation status':'ACTIVE','Investigation summary':'Material icons are delightful, beautifully crafted symbols for common actions and items. Download on desktop to use them in your digital products for Android, iOS, and webMaterial icons are delightful, beautifully crafted symbols for common actions and items. Download on desktop to use them in your digital products for Android, iOS, and webMaterial icons are delightful, beautifully crafted symbols for common actions and items. Download on desktop to use them in your digital products for Android,','Start date':'2019-10-01T01:00:30','Manager':1,'Complete flag':false,'Complete date':null,'Complete reason':'Deletedddd','Receive date':'2019-09-04T12:00:37','Delete flag':null,'Delete date':null}"

            };
            //Before Delete
            Investigation investigation = _fixture.Db.Investigation.Single(i => i.InvestigationId == 14);
            Assert.False(investigation.DeleteFlag);

            //Before Insert history table
            InvestigationHistory investigationHistory = _fixture.Db.InvestigationHistory.SingleOrDefault(
                i => i.InvestigationId == 14);
            Assert.Null(investigationHistory);

            //Act
            _investigationService.DeleteInvestigation(values);

            //Assert
            //After Delete
            investigation = _fixture.Db.Investigation.Single(i => i.InvestigationId == 14);
            Assert.True(investigation.DeleteFlag);

            //After Insert history table
            investigationHistory = _fixture.Db.InvestigationHistory.Single(
                i => i.InvestigationId == 14);
            Assert.NotNull(investigationHistory);
        }


        [Fact]
        public void GetInvestigationHistoryDetails()
        {
            //Act
            List<HistoryVm> lstHistory = _investigationService.GetInvestigationHistoryDetails(10);

            //Assert
            Assert.True(lstHistory.Count > 1);
        }


    }
}
