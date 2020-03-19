using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Moq;
using ServerAPI.Services;
using ServerAPI.Tests;
using ServerAPI.ViewModels;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using System.Threading.Tasks;
using GenerateTables.Models;
using Microsoft.Extensions.Caching.Memory;
using ServerAPI.JMS.Tests.Helper;
using JwtDb.Models;
using Microsoft.AspNetCore.Identity;

// ReSharper disable once CheckNamespace
namespace ServerAPI.JMS.Tests
{
    [Collection("Database collection")]
    public class IncidentActiveServiceTest
    {
        private readonly IncidentActiveService _incidentActiveService;
        private readonly DbInitialize _fixture;
        private readonly IConfigurationRoot _configuration = MockHelpers.GetIConfigurationRoot();
        private readonly Mock<IMemoryCache> _memory = new Mock<IMemoryCache>();


        public IncidentActiveServiceTest(DbInitialize fixture)
        {
            _fixture = fixture;
            PhotosService photosService = new PhotosService(_fixture.Db, _configuration);
            AppletsSavedService appletsSavedService = new AppletsSavedService(fixture.Db, _configuration);
            HttpContextAccessor httpContext = new HttpContextAccessor { HttpContext = fixture.Context.HttpContext };
            PersonService personService = new PersonService(_fixture.Db);
            InterfaceEngineService interfaceEngineService = new InterfaceEngineService(fixture.Db, _configuration,
                _memory.Object);
            Mock<UserManager<AppUser>> userMgr = MockHelpers.MockUserManager<AppUser>();
            CommonService commonService = new CommonService(_fixture.Db, _configuration, 
                httpContext, personService,appletsSavedService,interfaceEngineService);
            
            AtimsHubService atimsHubService = new AtimsHubService(_fixture.HubContext.Object);
            FacilityPrivilegeService facilityPrivilegeService = new FacilityPrivilegeService(_fixture.Db, httpContext,
                personService);
            FacilityHousingService facilityHousingService = new FacilityHousingService(fixture.Db, _configuration,
                httpContext);
            RecordsCheckService recordsCheckService = new RecordsCheckService(fixture.Db, commonService, httpContext);
            CellService cellService = new CellService(fixture.Db, commonService, httpContext,
                facilityPrivilegeService, facilityHousingService, _fixture.JwtDb, userMgr.Object);
            InmateService inmateService = new InmateService(_fixture.Db, commonService, personService,
                httpContext, facilityPrivilegeService, photosService);
            BookingReleaseService bookingReleaseService =
                new BookingReleaseService(_fixture.Db, commonService, httpContext, personService, inmateService,
                    recordsCheckService, facilityHousingService, interfaceEngineService);

            WizardService wizardService = new WizardService(_fixture.Db,atimsHubService);
            AltSentService altSentService = new AltSentService(fixture.Db, commonService, httpContext, photosService,
                personService);
            PrebookWizardService prebookWizardService = new PrebookWizardService(_fixture.Db, _configuration,
                commonService, cellService, httpContext, atimsHubService, personService, inmateService,
                bookingReleaseService, altSentService, interfaceEngineService,photosService,appletsSavedService);

            PrebookActiveService prebookActiveService = new PrebookActiveService(_fixture.Db, httpContext, atimsHubService);

            SentenceService sentenceService = new SentenceService(fixture.Db, commonService, httpContext, personService,
                interfaceEngineService);

            BookingService bookingService = new BookingService(
                _fixture.Db, commonService, httpContext, bookingReleaseService, prebookWizardService,
                prebookActiveService, sentenceService, wizardService, personService, inmateService,
                photosService, interfaceEngineService);

            IncidentService incidentService = new IncidentService(
                _fixture.Db, wizardService, commonService, bookingService, httpContext, photosService);

            _incidentActiveService = new IncidentActiveService(fixture.Db, commonService, httpContext, incidentService,
                personService, photosService);
        }

        [Fact]
        public void IncidentActive_LoadActiveIncidentViewerDetails()
        {
            //Arrange
            IncidentFilters incidentFilters = new IncidentFilters
            {
                FacilityId = 1,
                Skip = 0,
                RowsPerPage = 1,
                Hours = 0,
                SortColumn = "DisciplinaryReportDate",
                SortOrder = "asc"
            };
            //Act
            IncidentViewerDetails incidentViewer =
                _incidentActiveService.LoadActiveIncidentViewerDetails(incidentFilters);
            //Assert
            Assert.NotNull(incidentViewer.IncidentViewerList);
            IncidentViewer incident = incidentViewer.IncidentViewerList.Single(i => i.DisciplinaryTypeId == 10);
            Assert.Equal("ATTEMPT SUICIDE", incident.DisciplinaryType);
            Assert.Equal("DIS_100101", incident.IncidentNumber);
        }

        [Fact]
        public void IncidentActive_LoadIncidentDetails()
        {
            //Act
            IncidentDetails incidentDetails = _incidentActiveService.LoadIncidentDetails(6, 0, false);
            //Assert
            Assert.True(incidentDetails.IncidentAttachments.Count > 0);
            Assert.True(incidentDetails.InvolvedPartyDetails.Count > 1);
        }

        [Fact]
        public async Task IncidentActive_InsertOrUpdateNarrativeInfo()
        {
            //Arrange

            IncidentNarrativeDetailVm incidentNarrativeDetail = new IncidentNarrativeDetailVm
            {
                DisciplinaryIncidentNarrativeId = 10,
                DisciplinaryIncidentNarrative = "EXCESSIVE NOISE/NO POWER"

            };
            DisciplinaryIncidentNarrative disciplinaryIncident = _fixture.Db.DisciplinaryIncidentNarrative.Single
                (d => d.DisciplinaryIncidentNarrativeId == 10);
            Assert.Null(disciplinaryIncident.DisciplinaryIncidentNarrative1);
            //Act
            await _incidentActiveService.InsertOrUpdateNarrativeInfo(incidentNarrativeDetail);
            //Assert
            disciplinaryIncident =
                _fixture.Db.DisciplinaryIncidentNarrative.Single(d => d.DisciplinaryIncidentNarrativeId == 10);

            Assert.Equal("EXCESSIVE NOISE/NO POWER", disciplinaryIncident.DisciplinaryIncidentNarrative1);
        }

        [Fact]
        public async Task IncidentActive_UpdateIncidentActiveStatus()
        {
            DisciplinaryIncident disciplinaryIncident =
                _fixture.Db.DisciplinaryIncident.Single(d => d.DisciplinaryIncidentId == 7);
            Assert.Equal(1, disciplinaryIncident.DisciplinaryActive);
            //Act
            await _incidentActiveService.UpdateIncidentActiveStatus(7, false);
            //Assert
            disciplinaryIncident = _fixture.Db.DisciplinaryIncident.Single(d => d.DisciplinaryIncidentId == 7);
            Assert.Equal(0, disciplinaryIncident.DisciplinaryActive);
        }

        [Fact]
        public async Task IncidentActive_DeleteNarrativeInfo()
        {

            DisciplinaryIncidentNarrative disciplinaryIncident =
                _fixture.Db.DisciplinaryIncidentNarrative.Single(d => d.DisciplinaryIncidentNarrativeId == 11);

            Assert.Null(disciplinaryIncident.DeleteFlag);
            //Act
            await _incidentActiveService.DeleteNarrativeInfo(11);
            //Assert
            disciplinaryIncident =
                _fixture.Db.DisciplinaryIncidentNarrative.Single(d => d.DisciplinaryIncidentNarrativeId == 11);
            Assert.Equal(1, disciplinaryIncident.DeleteFlag);
        }

        [Fact]
        public void IncidentActive_GetIncidentActiveStatus()
        {
            //Act
            int returnValues = _incidentActiveService.GetIncidentActiveStatus(8);
            //Assert
            Assert.Equal(1, returnValues);
        }

        [Fact]
        public void IncidentActive_LoadForms()
        {
            //Act
            List<LoadSavedForms> lstLoadSaved = _incidentActiveService.LoadForms("BOOKING FORMS");

            //Assert
            LoadSavedForms loadSaved = lstLoadSaved.Single(l => l.FormTemplatesId == 12);
            Assert.Equal("BOOKING FORMS", loadSaved.DisplayName);
        }

        [Fact]
        public async Task IncidentActive_InsertFormRecord()
        {
            FormRecord formRecord = _fixture.Db.FormRecord.SingleOrDefault(f => f.DisciplinaryControlId == 12);
            Assert.Null(formRecord);
            //Act
            await _incidentActiveService.InsertFormRecord(12, 12, true, 105);
            //Assert
            formRecord = _fixture.Db.FormRecord.Single(f => f.DisciplinaryControlId == 12);
            Assert.NotNull(formRecord);
        }

        // 8472 SVN Revision issues fixed
        //Here Checked ExpectedNarrativeCount in DisciplinaryIncident Table.
        [Fact]
        public async Task DeleteInvolvedPartyDetails()
        {
            DisciplinaryIncident disciplinaryIncident =
                _fixture.Db.DisciplinaryIncident.Single(d => d.DisciplinaryIncidentId == 5);
            Assert.Equal(4, disciplinaryIncident.ExpectedNarrativeCount);
            await _incidentActiveService.DeleteInvolvedPartyDetails(122);
            disciplinaryIncident =
                _fixture.Db.DisciplinaryIncident.Single(d => d.DisciplinaryIncidentId == 5);
            Assert.Equal(3, disciplinaryIncident.ExpectedNarrativeCount);

        }
    }
}
