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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

// ReSharper disable once CheckNamespace
namespace ServerAPI.JMS.Tests
{
    [Collection("Database collection")]
    public class IncidentWizardServiceTest
    {
        private readonly IncidentWizardService _incidentWizardService;
        private readonly DbInitialize _fixture;
        private readonly IConfigurationRoot _configuration = MockHelpers.GetIConfigurationRoot();
        private readonly Mock<IMemoryCache> _memory = new Mock<IMemoryCache>();

        public IncidentWizardServiceTest(DbInitialize fixture)
        {
            _fixture = fixture;
            HttpContextAccessor httpContext = new HttpContextAccessor { HttpContext = fixture.Context.HttpContext };
            AtimsHubService atimsHubService = new AtimsHubService(_fixture.HubContext.Object);
            AppletsSavedService appletsSavedService = new AppletsSavedService(fixture.Db, _configuration);
            Mock<UserManager<AppUser>> userMgr = MockHelpers.MockUserManager<AppUser>();
            Mock<RoleManager<IdentityRole>> roleMgr = MockHelpers.MockRoleManager<IdentityRole>();
            UserPermissionPolicy userPermission =
                new UserPermissionPolicy(_fixture.Db, userMgr.Object, roleMgr.Object, httpContext);
            PhotosService photosService = new PhotosService(_fixture.Db, _configuration);
            PersonService personService = new PersonService(_fixture.Db);
            InterfaceEngineService interfaceEngineService = new InterfaceEngineService(fixture.Db, _configuration,
                _memory.Object);
            CommonService commonService = new CommonService(fixture.Db, _configuration, httpContext,personService,appletsSavedService,
            interfaceEngineService);
            FacilityPrivilegeService facilityPrivilegeService = new FacilityPrivilegeService(_fixture.Db, httpContext,
                personService);

            FacilityHousingService facilityHousingService = new FacilityHousingService(_fixture.Db, _configuration,
                httpContext);
            RecordsCheckService recordsCheckService = new RecordsCheckService(fixture.Db, commonService, httpContext);
            InmateService inmateService = new InmateService(_fixture.Db, commonService, personService, httpContext,
                facilityPrivilegeService, photosService);
            WizardService wizardService = new WizardService(_fixture.Db,atimsHubService);
          
            CellService cellService = new CellService(fixture.Db, commonService, httpContext,
                facilityPrivilegeService, facilityHousingService, _fixture.JwtDb, userMgr.Object);
            AltSentService altSentService = new AltSentService(_fixture.Db, commonService, httpContext, photosService,
                personService);

            BookingReleaseService bookingReleaseService = new BookingReleaseService(_fixture.Db, commonService,
                httpContext, personService, inmateService, recordsCheckService, facilityHousingService,
                interfaceEngineService);
            PrebookWizardService prebookWizardService = new PrebookWizardService(_fixture.Db,
                _configuration, commonService, cellService, httpContext, atimsHubService,
                personService, inmateService, bookingReleaseService, altSentService, interfaceEngineService,
                photosService,appletsSavedService);
            PrebookActiveService prebookActiveService = new PrebookActiveService(_fixture.Db,
                httpContext, atimsHubService);
            SentenceService sentenceService = new SentenceService(fixture.Db, commonService, httpContext, personService,
                interfaceEngineService);
            BookingService bookingService = new BookingService(_fixture.Db, commonService, httpContext, bookingReleaseService,
                prebookWizardService, prebookActiveService, sentenceService, wizardService, personService,
                inmateService, photosService, interfaceEngineService);

            IncidentService incidentService = new IncidentService(_fixture.Db, wizardService, commonService, bookingService,
                httpContext, photosService);
            KeepSepAlertService keepSepAlertService = new KeepSepAlertService(_fixture.Db, httpContext,
                facilityHousingService,interfaceEngineService);
            PersonAkaService personAkaService = new PersonAkaService(_fixture.Db, commonService, httpContext, personService,
                interfaceEngineService);
            PersonCharService personCharService = new PersonCharService(_fixture.Db, commonService, httpContext, personService,
                interfaceEngineService);
            PersonIdentityService personIdentityService = new PersonIdentityService(_fixture.Db,
                httpContext, prebookActiveService, personAkaService, personService,interfaceEngineService);

            RegisterService registerService = new RegisterService(_fixture.Db, httpContext, commonService, personCharService,
                photosService, wizardService, personIdentityService, keepSepAlertService, userPermission);
            AppointmentViewerService appointmentViewerService = new AppointmentViewerService(_fixture.Db, commonService, photosService);

            AppointmentService appointmentService = new AppointmentService(fixture.Db, commonService, registerService,
                httpContext, personService, keepSepAlertService, interfaceEngineService, appointmentViewerService);
            _incidentWizardService = new IncidentWizardService(_fixture.Db, httpContext, commonService,
                incidentService, interfaceEngineService,appointmentViewerService);
        }

        [Fact]
        public void DisciplinaryLookUpDetails()
        {
            //Act
            List<IncidentViolationVm> incidentViolatiom = _incidentWizardService.DisciplinaryLookUpDetails("WORK", "SS");

            //Assert
            IncidentViolationVm incidentViolations = incidentViolatiom.Single(i => i.DisciplinaryControlLookupId == 11);
            Assert.Equal("WORK", incidentViolations.DisciplinaryControlLookupName);
            Assert.Equal((int?)DisciplinaryLookup.DISCWAIV, incidentViolations.DisciplinaryControlLookupType);
        }

        [Fact]
        public void ViolationDetails()
        {
            //Act
            List<IncicentViolationSaveVm> lstIncidentViolation = _incidentWizardService.ViolationDetails(102);

            //Assert
            IncicentViolationSaveVm incicentViolationSave = lstIncidentViolation.Single(i => i.DisciplinaryControlXrefId == 15);
            Assert.Equal("102 GAMBLING IS PROHIBITED", incicentViolationSave.Violation);
        }

        [Fact]
        public async Task DisciplinaryControlInsertUpdate()
        {
            //Arrange
            //For Update
            IncicentViolationSaveVm incicentViolationSave = new IncicentViolationSaveVm
            {
                DisciplinaryControlFindingId = 10,
                Operation = DisciplinaryOperation.Edit,
                DisciplinaryControlXrefId = 16,
                DisciplinaryControlPleaId = 11,
                DisciplinaryControlSanctionDays = 6,
                DisciplinaryControlViolationId = 12,
                DisciplinaryControlNotes = "DISCIPLINARY IS NOT REQUIRED"
            };
            DisciplinaryControlXref disciplinaryControlXref = _fixture.Db.DisciplinaryControlXref.Single(d => d.DisciplinaryControlXrefId == 16);
            Assert.Null(disciplinaryControlXref.DisciplinaryControlViolationId);
            Assert.Null(disciplinaryControlXref.DisciplinaryControlNotes);

            //Act
            await _incidentWizardService.DisciplinaryControlInsertUpdate(incicentViolationSave);

            //Assert
            disciplinaryControlXref = _fixture.Db.DisciplinaryControlXref.Single(d => d.DisciplinaryControlXrefId == 16);
            Assert.Equal(12, disciplinaryControlXref.DisciplinaryControlViolationId);
            Assert.Equal("DISCIPLINARY IS NOT REQUIRED", disciplinaryControlXref.DisciplinaryControlNotes);
        }

        [Fact]
        public async Task DisciplinaryInmateUpdate()
        {
            //Arrange
            IncidentInmateVm incidentInmateVm = new IncidentInmateVm
            {
                InmateId = 102,
                ReviewDate = DateTime.Now,
                ReviewNotes = "REVIEWED BY ANOTHER USER",
                ReviewOfficerId = 13
            };

            DisciplinaryInmate disciplinaryInmate = _fixture.Db.DisciplinaryInmate.Single(i => i.DisciplinaryInmateId == 102);
            Assert.Null(disciplinaryInmate.DisciplinaryReviewNotes);

            //Act
            await _incidentWizardService.DisciplinaryInmateUpdate(incidentInmateVm);

            //Assert            
            disciplinaryInmate = _fixture.Db.DisciplinaryInmate.Single(i => i.DisciplinaryInmateId == 102);
            Assert.Equal("REVIEWED BY ANOTHER USER", disciplinaryInmate.DisciplinaryReviewNotes);
        }

        [Fact]
        public void GetScheduleHearing()
        {
            //Act
            DisciplinaryHearing disciplinary = _incidentWizardService.GetScheduleHearing(105);

            //Assert
            Assert.Equal("SANGEETHA", disciplinary.HearingOfficer1.PersonFirstName);
            Assert.Equal("KRISHNA", disciplinary.HearingOfficer2.PersonLastName);
        }

        [Fact]
        public async Task InsertUpdateInvolvedParty()
        {
            //Arrange
            ClassifyInvPartyDetailsVm classifyInvPartyDetails = new ClassifyInvPartyDetailsVm
            {
                DisciplinaryInmateId = 100,
                InvolvedType = 5,
                DisciplinaryDamage = "4.0",
                PersonnelId = 13,
                InmateId = 104,
                DisciplinaryIncidentId = 6,
                NarrativeFlag = true
            };
            DisciplinaryInmate disciplinaryInmate = _fixture.Db.DisciplinaryInmate.Single(d => d.DisciplinaryInmateId == 100);
            Assert.Null(disciplinaryInmate.InmateId);
            Assert.Null(disciplinaryInmate.DisciplinaryDamage);

            //Act
            await _incidentWizardService.InsertUpdateInvolvedParty(classifyInvPartyDetails);
            //Assert
            Assert.Equal(104, disciplinaryInmate.InmateId);
            Assert.Equal("4.0", disciplinaryInmate.DisciplinaryDamage);


            // 8472 SVN Revision issues fixed checked

            classifyInvPartyDetails = new ClassifyInvPartyDetailsVm
            {
                DisciplinaryInmateId = 123,
                InvolvedType = 4,
                PersonnelId = 13,
                InmateId = 105,
                DisciplinaryIncidentId = 10,
                NarrativeFlag = true
            };
            DisciplinaryIncident disciplinaryIncident = _fixture.Db.DisciplinaryIncident.Single(
                d => d.DisciplinaryIncidentId == 10);
            Assert.Null(disciplinaryIncident.ExpectedNarrativeCount);
            await _incidentWizardService.InsertUpdateInvolvedParty(classifyInvPartyDetails);
            disciplinaryIncident = _fixture.Db.DisciplinaryIncident.Single(
                d => d.DisciplinaryIncidentId == 10);
            Assert.Equal(1, disciplinaryIncident.ExpectedNarrativeCount);
        }

        [Fact]
        public void GetInvPartyEntryDetails()
        {
            //Act
            ClassifyInvPartyDetailsVm classifyInvParty = _incidentWizardService.GetInvPartyEntryDetails(104);

            //Assert
            Assert.Equal(7, classifyInvParty.DisciplinaryIncidentId);
            Assert.Equal("WAJBAI", classifyInvParty.DisciplinaryOtherName);
        }

        [Fact]
        public async Task DisciplinaryControlXrefUpdate()
        {
            //Arrange
            IncicentViolationSaveVm incicentViolationSaveVm = new IncicentViolationSaveVm
            {
                DisciplinaryControlXrefId = 17,
                DisciplinaryControlSanctionDays = 10,
                DisciplinaryControlSysRecSanctionDays = 15,
                DisciplinaryControlSysRecPriorCount = 5,
                DisciplinaryControlPleaId = 22,
                DisciplinaryControlNotes = "CHECK DURING COUNT THE INMATES ARE MISSING",
                DisciplinaryControlWaiverId = 10,
                DisciplinaryControlFindingId = 20
            };
            DisciplinaryControlXref disciplinaryControlXref = _fixture.Db.DisciplinaryControlXref.Single(d => d.DisciplinaryControlXrefId == 17);
            Assert.Null(disciplinaryControlXref.DisciplinaryControlWaiverId);
            Assert.Null(disciplinaryControlXref.DisciplinaryControlSanctionDays);

            //Act
            await _incidentWizardService.DisciplinaryControlXrefUpdate(incicentViolationSaveVm);

            //Assert
            disciplinaryControlXref = _fixture.Db.DisciplinaryControlXref.Single(d => d.DisciplinaryControlXrefId == 17);
            Assert.Equal(10, disciplinaryControlXref.DisciplinaryControlWaiverId);
            Assert.Equal(22, disciplinaryControlXref.DisciplinaryControlPleaId);

        }
        [Fact]
        public void CheckIncidentComplete()
        {
            bool result = _incidentWizardService.CheckIncidentComplete(9);
            Assert.True(result);
        }


        //[Fact]
        //public void GetAppointmentSchedule()
        //{
        //    //Act
        //    List<IncidentAppointmentVm> lstIncidentAppointments = _incidentWizardService.GetAppointmentSchedule(104);

        //    //Assert
        //    IncidentAppointmentVm incidentAppointment = lstIncidentAppointments.Single(
        // => a.AppointmentId == 8);
        //    Assert.Equal("TRICHY", incidentAppointment.Location);
        //}

        //      [Fact]
        //      public void GetAppointmentRooms()
        //      {
        //	//Act
        //	List<IncidentAppointmentVm> lstIncidentAppointments = _incidentWizardService.GetAppointmentRooms(103, null, 5);

        //	//Assert
        //	IncidentAppointmentVm incidentAppointment = lstIncidentAppointments.Single(l => l.DisciplinaryNumber == "DIS_100102");
        //	Assert.Equal("ATTEMPT SUICIDE", incidentAppointment.Description);
        //}

        [Fact]
        public async Task UpdateScheduleHearingLocation()
        {
            //Arrange
            IncidentAppointmentVm incidentAppointment = new IncidentAppointmentVm
            {
                PersonDetail = new PersonDetailVM
                {
                    PersonId = 55
                },
                DisciplinaryInmateId = 105,
                ScheduleHearingDate = DateTime.Now,
                Location = "PUDUKKOTTAI"
            };

            DisciplinaryInmate disciplinary = _fixture.Db.DisciplinaryInmate.Single(d => d.DisciplinaryInmateId == 105);
            Assert.Null(disciplinary.DisciplinaryScheduleHearingLocation);
            //Act
            await _incidentWizardService.UpdateScheduleHearingLocation(incidentAppointment);

            //Assert
            disciplinary = _fixture.Db.DisciplinaryInmate.Single(d => d.DisciplinaryInmateId == 105);
            Assert.Equal("PUDUKKOTTAI", disciplinary.DisciplinaryScheduleHearingLocation);
        }

        [Fact]
        public void GetAppliedBookings()
        {
            //Act
            List<AppliedBooking> lstAppliedBookings = _incidentWizardService.GetAppliedBookings(104);

            //Assert
            AppliedBooking appliedBooking = lstAppliedBookings.Single(a => a.ArrestId == 6);
            Assert.Equal("160001191", appliedBooking.BookingNumber);
        }

        [Fact]
        public void GetAppliedCharges()
        {
            //Act
            List<AppliedCharge> appliedCharges = _incidentWizardService.GetAppliedCharges(102);

            //Assert
            Assert.NotNull(appliedCharges);
        }

        [Fact]
        public async Task ReviewComplete()
        {
            //Arrange
            ReviewComplete reviewComplete = new ReviewComplete
            {
                DisciplinaryInmateId = 120,
                CompleteDate = DateTime.Now,
                InmateId = 120,
                CompleteOfficer = 12,
                DisciplinaryDays = 10,
                Sanction = "5 DAYS BENDING WORK DETAIL",
                AppliedBookings = new List<AppliedBooking>
                {
                    new AppliedBooking
                    {
                        DispSentDayXrefid = 11
                    }
                },
                AppliedCharges = new List<AppliedCharge>
                {
                    new AppliedCharge()
                }
            };

            DisciplinaryInmate disciplinaryInmate = _fixture.Db.DisciplinaryInmate.Single(d => d.DisciplinaryInmateId == 120);
            Assert.Null(disciplinaryInmate.DisciplinarySanction);

            //Act
            await _incidentWizardService.ReviewComplete(reviewComplete);

            //Assert
            disciplinaryInmate = _fixture.Db.DisciplinaryInmate.Single(d => d.DisciplinaryInmateId == 120);
            Assert.Equal("5 DAYS BENDING WORK DETAIL", disciplinaryInmate.DisciplinarySanction);
        }

        [Fact]
        public void GetIncidentWizardCompleteDetails()
        {
            //Act
            IncidentWizardCompleteDetails incidentWizardComplete = _incidentWizardService.GetIncidentWizardCompleteDetails(5);

            //Assert
            ClassifyInvPartyDetailsVm classifyInvPartyDetails = incidentWizardComplete.DisciplinaryInmateDetails
                .Single(d => d.DisciplinaryInmateId == 101);
            Assert.Equal("ATTEMPT SUICIDE", classifyInvPartyDetails.DisciplinaryType);
            Assert.Equal("SVK661", classifyInvPartyDetails.InmateNumber);
        }

        [Fact]
        public void UpdateDisciplinaryInmateNotice()
        {
            //Act
            DisciplinaryInmateNotice disciplinaryInmateNotice = new DisciplinaryInmateNotice
            {
                NoticeDate = DateTime.Now,
                NoticeFlag = false,
                NoticeNote = "Diwali is celebrated by Hindus",
                NoticePersonnelId = 12,
                NoticeWavierId = 102
            };

            DisciplinaryInmate disciplinaryInmate = _fixture.Db.DisciplinaryInmate.Single(x => x.DisciplinaryInmateId == 121);
            Assert.Null(disciplinaryInmate.NoticeWavierId);
            Assert.Null(disciplinaryInmate.PersonnelId);

            //Act
            _incidentWizardService.UpdateDisciplinaryInmateNotice(121, disciplinaryInmateNotice);

            //Assert
            Assert.Equal(102, disciplinaryInmate.NoticeWavierId);
            Assert.Equal(12, disciplinaryInmate.NoticePersonnelId);

        }

        [Fact]
        public void GetDisciplinaryInmateNotice()
        {
            //Act
            DisciplinaryInmateNotice disciplinaryInmate = _incidentWizardService.GetDisciplinaryInmateNotice(122);

            //Assert
            Assert.Equal("CHANGE INTO NEW CELL", disciplinaryInmate.NoticeNote);
        }

    }
}

