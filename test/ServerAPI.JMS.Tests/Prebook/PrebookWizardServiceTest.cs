using Microsoft.Extensions.Configuration;
using ServerAPI.Services;
using ServerAPI.ViewModels;
using Xunit;
using System.Linq;
using GenerateTables.Models;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using ServerAPI.Tests;
using System.Collections.Generic;
using System;
using ServerAPI.JMS.Tests.Helper;
using Moq;
using Microsoft.AspNetCore.Identity;
using JwtDb.Models;
using Microsoft.Extensions.Caching.Memory;

// ReSharper disable once CheckNamespace
namespace ServerAPI.JMS.Tests
{
    [Collection("Database collection")]
    public class PrebookWizardServiceTest
    {
        private readonly PrebookWizardService _prebookWizardservice;
        private readonly IConfigurationRoot _configuration = MockHelpers.GetIConfigurationRoot();
        private readonly DbInitialize _fixture;
        private readonly Mock<IMemoryCache> _memory = new Mock<IMemoryCache>();


        public PrebookWizardServiceTest(DbInitialize fixture)
        {
            _fixture = fixture;
            HttpContextAccessor httpContext = new HttpContextAccessor { HttpContext = fixture.Context.HttpContext };
            PersonService personService = new PersonService(fixture.Db);
            InterfaceEngineService interfaceEngineService = new InterfaceEngineService(fixture.Db, _configuration, _memory.Object);
            AppletsSavedService appletsSavedService = new AppletsSavedService(_fixture.Db, _configuration);
            PhotosService photosService = new PhotosService(_fixture.Db, _configuration);
            Mock<UserManager<AppUser>> userMgr = MockHelpers.MockUserManager<AppUser>();
            CommonService commonService = new CommonService(_fixture.Db, _configuration,
                httpContext, personService,appletsSavedService,interfaceEngineService);
            FacilityHousingService facilityHousingService = new FacilityHousingService(fixture.Db, _configuration, httpContext);
            RecordsCheckService recordsCheckService = new RecordsCheckService(fixture.Db, commonService, httpContext);
            FacilityPrivilegeService facilityPrivilegeService = new FacilityPrivilegeService(fixture.Db, httpContext,
                personService);
            AltSentService altSentService = new AltSentService(fixture.Db, commonService, httpContext, photosService, personService);
            InmateService inmateService = new InmateService(_fixture.Db, commonService, personService, httpContext,
                facilityPrivilegeService, photosService);
            //For singnalR concept it will getting null values from (_hubContext.Clients)
            //So I comment this lines 
            //  AtimsHubService atimsHubService = new AtimsHubService(hubContext.Object);

            //for SingnalR concept
            //So I am added Mock values in DbInitialize class file DI access from it here.
            AtimsHubService atimsHubService = new AtimsHubService(fixture.HubContext.Object);
            BookingReleaseService bookingReleaseService = new BookingReleaseService(
                fixture.Db, commonService, httpContext, personService, inmateService, recordsCheckService,
                facilityHousingService,interfaceEngineService);
            CellService cellService = new CellService(fixture.Db, commonService, httpContext,
                facilityPrivilegeService, facilityHousingService, _fixture.JwtDb, userMgr.Object);
            _prebookWizardservice = new PrebookWizardService(fixture.Db, _configuration, commonService,
                cellService, httpContext, atimsHubService, personService, inmateService, bookingReleaseService,
                altSentService,interfaceEngineService,photosService,appletsSavedService);
        }

        [Fact]
        public async Task PrebookWizardService_CreateInmatePrebookCharge()
        {
            //Arrange
            PrebookCharge prebook = new PrebookCharge
            {
                InmatePrebookId = 15,
                CrimeNotes = "LAND ISSUES",
                CrimeLookupId = 30,
                BailAmount = 5000,
                CrimeType = "CIVIL"
            };
            InmatePrebookCharge inmateprebook =
                _fixture.Db.InmatePrebookCharge.SingleOrDefault(x => x.InmatePrebookId == 15);
            Assert.Null(inmateprebook);
            //Act
            await _prebookWizardservice.InsertInmatePrebookCharge(prebook);
            //Assert
            inmateprebook = _fixture.Db.InmatePrebookCharge.Single(i => i.InmatePrebookId == 15);
            Assert.Equal("LAND ISSUES", inmateprebook.CrimeNotes);
            Assert.Equal(5000, inmateprebook.BailAmount);
        }


        //SignalR concept Using this method.
        [Fact]
        public async Task UpdatePrebookComplete()
        {
            //Before Update
            InmatePrebook inmatePrebook = _fixture.Db.InmatePrebook.Single(i => i.InmatePrebookId == 19);
            Assert.Equal(0, inmatePrebook.CompleteFlag);

            //Act
            await _prebookWizardservice.UpdatePrebookComplete(19);

            //Assert
            //After Update
            inmatePrebook = _fixture.Db.InmatePrebook.Single(i => i.InmatePrebookId == 19);
            Assert.Equal(1, inmatePrebook.CompleteFlag);
        }

        [Fact]
        public void GetAgencies()
        {
            //Act
            List<AgencyVm> lstAgency = _prebookWizardservice.GetAgencies();
            //Assert
            Assert.True(lstAgency.Count > 3);

        }

        [Fact]
        public void GetFacilities()
        {
            //Act
            List<FacilityVm> lstFacility = _prebookWizardservice.GetFacilities();
            //Assert
            FacilityVm facility = lstFacility.Single(f => f.FacilityId == 1);
            Assert.Equal("MCJ", facility.FacilityAbbr);
            Assert.Equal("Madras Central Jail", facility.FacilityName);
        }

        [Fact]
        public async Task InsertPrebook()
        {
            //Arrange
            InmatePrebookVm inmatePrebook = new InmatePrebookVm
            {
                PersonId = 100,
                CreateBy = 12,
                UpdateBy = 12,
                CreateDate = DateTime.Now.AddDays(-1),
                UpdateDate = DateTime.Now.AddDays(-1),
                PrebookDate = DateTime.Now,
                CourtCommitFlag = false,
                FacilityId = 1,
                CompleteFlag = 1,
                ArrestAgencyId = 7,
                CaseNumber = "CASE_NUM10",
                ArrestType = 5,
                InmatePrebookCaseId = 12,
                VehicleColor1 = "RED",
                VehicleColor2 = "BLACK",
                VehicleModelid = 1232,
                VehicleType = "PULSER_120"
            };
            //Before Insert on InmatePrebookCase table
            InmatePrebookCase inmatePrebookCase = _fixture.Db.InmatePrebookCase.SingleOrDefault(i => i.CaseNumber == "CASE_NUM10");
            Assert.Null(inmatePrebookCase);
            //Before Insert on InmatePrebook table
            InmatePrebook prebook = _fixture.Db.InmatePrebook.SingleOrDefault(i => i.VehicleType == "PULSER_120");
            Assert.Null(prebook);

            await _prebookWizardservice.InsertPrebook(inmatePrebook);
            //Assert
            //After Insert on InmatePrebookCase table
            inmatePrebookCase = _fixture.Db.InmatePrebookCase.Single(i => i.CaseNumber == "CASE_NUM10");
            Assert.NotNull(inmatePrebookCase);
            //After Insert on InmatePrebook table
            prebook = _fixture.Db.InmatePrebook.Single(i => i.VehicleType == "PULSER_120");
            Assert.NotNull(prebook);
        }

        [Fact]
        public async Task UpdatePrebook()
        {
            //Arrange
            InmatePrebookVm inmatePrebook = new InmatePrebookVm
            {
                PersonId = 110,
                CreateBy = 12,
                UpdateBy = 12,
                CreateDate = DateTime.Now.AddDays(-1),
                UpdateDate = DateTime.Now.AddDays(-1),
                PrebookDate = DateTime.Now,
                CourtCommitFlag = false,
                FacilityId = 1,
                CompleteFlag = 1,
                ArrestAgencyId = 7,
                CaseNumber = "IC-10004",
                ArrestType = 5,
                InmatePrebookCaseId = 12,
                VehicleColor1 = "BLACK",
                VehicleColor2 = "BLACK",
                VehicleModelid = 1232,
                VehicleType = "PULSER_180",
                PrebookNotes = "B-4478506",
                DeleteFlag = false,
                VehicleLicense = "ICR-4101210",
                InmatePrebookId = 23,
                DisplayArresteeCondition = new []
                {
                    11
                },
                DisplayArresteeBAC = new []
                {
                    12
                },
                DisplayArresteeBehavior = new []
                {
                    10
                }
            };
            InmatePrebookCase inmatePrebookCase = _fixture.Db.InmatePrebookCase.
                Single(i => i.InmatePrebookId == 23);
            Assert.Null(inmatePrebookCase.CaseNumber);
            InmatePrebook prebook = _fixture.Db.InmatePrebook.Single(i => i.InmatePrebookId == 23);
            Assert.Null(prebook.VehicleLicense);

            //Act
            await _prebookWizardservice.UpdatePrebook(inmatePrebook);

            //Assert
            inmatePrebookCase = _fixture.Db.InmatePrebookCase.Single(i => i.InmatePrebookId == 23);
            Assert.Equal("IC-10004", inmatePrebookCase.CaseNumber);
            prebook = _fixture.Db.InmatePrebook.Single(i => i.InmatePrebookId == 23);
            Assert.Equal("ICR-4101210", prebook.VehicleLicense);
        }

        //
        //[Fact]
        //public void GetPrebookCharges()
        //{
        //    List<PrebookCharge> lstPrebookCharge = _prebookWizardservice.GetPrebookCharges(24, true, 10, 0, 7);



        //}

        [Fact]
        public async Task InsertInmatePrebookCharge()
        {
            //Arrange
            PrebookCharge prebook = new PrebookCharge
            {
                InmatePrebookChargeId = 11,
                BailAmount = 45000,
                InmatePrebookId = 12,
                CrimeCount = 1,
                CrimeLookupId = 6,
                InmatePrebookWarrantId = 12,
                CreateBy = 11,
                UpdateBy = 12,
                CreateDate = DateTime.Now.AddDays(-9),
                UpdateDate = DateTime.Now,
                InmatePrebookCaseId = 10,
                CrimeType = "5",
                CrimeNotes = "5000 BALANCE",
                CrimeId = 10,
                CrimeForceId = 7,
                DeleteFlag = false,
                DeleteDate = null,
                DeleteBy = null
            };
            //Before Insert  InmatePrebookCharge Table
            InmatePrebookCharge inmatePrebookCharge = _fixture.Db.InmatePrebookCharge.SingleOrDefault(i =>
                i.InmatePrebookWarrantId == 12);
            Assert.Null(inmatePrebookCharge);

            //Before Insert  CrimeHistory Table
            CrimeHistory crimeHistory = _fixture.Db.CrimeHistory.SingleOrDefault(c => c.CrimeId == 10);
            Assert.Null(crimeHistory);
            //Act
            await _prebookWizardservice.InsertInmatePrebookCharge(prebook);

            //Assert
            //After Insert  InmatePrebookCharge Table
            inmatePrebookCharge = _fixture.Db.InmatePrebookCharge.Single(i =>
                i.InmatePrebookWarrantId == 12);
            Assert.NotNull(inmatePrebookCharge);
            Assert.Equal(10, inmatePrebookCharge.InmatePrebookCaseId);

            //After Insert  CrimeHistory Table
            crimeHistory = _fixture.Db.CrimeHistory.Single(c => c.CrimeId == 10);

            Assert.NotNull(crimeHistory);

        }

        [Fact]
        public async Task InsertForceCharge()
        {
            //Arrange
            PrebookCharge prebookCharge = new PrebookCharge
            {
                CreateDate = DateTime.Now,
                DeleteFlag = false,
                UpdateDate = DateTime.Now,
                CrimeId = 15,
                InmatePrebookCaseId = 10,
                WarrantId = 11,
                CreateBy = 11,
                BailAmount = 4100,
                DeleteDate = null,
                DeleteBy = null,
                CrimeDescription = "NEW CASE INSTRUCTION",
                CrimeGroupId = 10,
                InmatePrebookChargeId = 11
            };
            //Before Insert Crime Table
            Crime crime = _fixture.Db.Crime.Single(c => c.CrimeId == 15);
            Assert.NotNull(crime);

            //Before Insert CrimeHistory Table
            CrimeHistory crimeisHistory = _fixture.Db.CrimeHistory.Single(c => c.CrimeId == 15);
            Assert.Null(crimeisHistory.InmatePrebookChargeId);

            //Before Insert CrimeForce Table
            CrimeForce crimeForce = _fixture.Db.CrimeForce.SingleOrDefault(c =>
                c.TempCrimeGroup == "10");

            Assert.Null(crimeForce);
            //Act
            await _prebookWizardservice.InsertForceCharge(prebookCharge);

            //Assert
            //After Insert Crime Table
            crime = _fixture.Db.Crime.SingleOrDefault(c => c.CrimeId == 15);
            Assert.Null(crime);

            //After Insert CrimeHistory Table
            crimeisHistory = _fixture.Db.CrimeHistory.Single(c => c.CrimeId == 15);
            Assert.Equal(11, crimeisHistory.InmatePrebookChargeId);

            //After Insert CrimeForce Table
            crimeForce = _fixture.Db.CrimeForce.Single(c =>
                c.TempCrimeGroup == "10");
            Assert.Equal("NEW CASE INSTRUCTION", crimeForce.TempCrimeDescription);
        }

        [Fact]

        public async Task DeleteInmatePrebookCharges()
        {
            //Before Delete
            InmatePrebookCharge inmatePrebookCharge = _fixture.Db.InmatePrebookCharge.Single(
                i => i.InmatePrebookChargeId == 14);
            Assert.Equal(0, inmatePrebookCharge.DeleteFlag);
            //Before Insert History table
            CrimeHistory crimeisHistory = _fixture.Db.CrimeHistory.SingleOrDefault(c => c.InmatePrebookChargeId == 14);
            Assert.Null(crimeisHistory);

            //Act
            await _prebookWizardservice.DeleteInmatePrebookCharges(14);

            //Assert
            //After Delete
            inmatePrebookCharge = _fixture.Db.InmatePrebookCharge.Single(
                i => i.InmatePrebookChargeId == 14);
            Assert.Equal(1, inmatePrebookCharge.DeleteFlag);
            //After Insert History table
            crimeisHistory = _fixture.Db.CrimeHistory.Single(c => c.InmatePrebookChargeId == 14);
            Assert.NotNull(crimeisHistory);

        }

        [Fact]
        public async Task UndoDeleteInmatePrebookCharges()
        {
            //Before UndoDelete
            InmatePrebookCharge inmatePrebookCharge = _fixture.Db.InmatePrebookCharge.Single(
                i => i.InmatePrebookChargeId == 15);
            Assert.Equal(1, inmatePrebookCharge.DeleteFlag);
            //Before Insert History table
            CrimeHistory crimeisHistory = _fixture.Db.CrimeHistory.SingleOrDefault(c => c.InmatePrebookChargeId == 15);
            Assert.Null(crimeisHistory);

            //Act
            await _prebookWizardservice.UndoDeleteInmatePrebookCharges(15);

            //Assert
            //After UndoDelete
            inmatePrebookCharge = _fixture.Db.InmatePrebookCharge.Single(
                i => i.InmatePrebookChargeId == 15);
            Assert.Null(inmatePrebookCharge.DeleteFlag);
            //After Insert History table
            crimeisHistory = _fixture.Db.CrimeHistory.Single(c => c.InmatePrebookChargeId == 15);
            Assert.NotNull(crimeisHistory);

        }

        [Fact]
        public async Task UpdateInmatePrebookCharge()
        {
            //Act
            PrebookCharge prebookCharge = new PrebookCharge
            {
                InmatePrebookChargeId = 16,
                BailAmount = 2500.15m,
                CrimeLookupId = 8,
                InmatePrebookId = 19,
                InmatePrebookWarrantId = 10,
                CrimeType = "5",
                CrimeNotes = "FULL BAIL AMOUNT"
            };
            //Before Update
            InmatePrebookCharge inmaePrebookCharge =
                _fixture.Db.InmatePrebookCharge.Single(i => i.InmatePrebookChargeId == 16);
            Assert.Equal(1500, inmaePrebookCharge.BailAmount);
            Assert.Null(inmaePrebookCharge.CrimeNotes);

            //Act
            await _prebookWizardservice.UpdateInmatePrebookCharge(prebookCharge);

            //Assert
            //After Update
            inmaePrebookCharge =
                _fixture.Db.InmatePrebookCharge.Single(i => i.InmatePrebookChargeId == 16);
            Assert.Equal(2500.15m, inmaePrebookCharge.BailAmount);
            Assert.Equal("FULL BAIL AMOUNT", inmaePrebookCharge.CrimeNotes);

        }

        [Fact]
        public async Task UpdateForceCharge()
        {
            //Arrange
            PrebookCharge prebook = new PrebookCharge
            {
                CrimeForceId = 10,
                InmatePrebookId = 11,
                ArrestId = 10,
                CrimeDescription = "MURDER CASE NOT COMPLETED",
                BailAmount = 500000,
                CrimeId = 12
            };
            //Before Update Crimeforce Table
            CrimeForce crimeForce = _fixture.Db.CrimeForce.Single(
                c => c.CrimeForceId == 10);
            Assert.Null(crimeForce.InmatePrebookId);

            //Before Update Incarceration Table
            Incarceration incarceration = _fixture.Db.Incarceration.Single(i => i.IncarcerationId == 20);
            Assert.Null(incarceration.ChargeLevel);

            //Before Insert CrimeHistory Table
            CrimeHistory crimeHistory = _fixture.Db.CrimeHistory.SingleOrDefault(c => c.CrimeId == 12);
            Assert.Null(crimeHistory);

            //Act
            await _prebookWizardservice.UpdateForceCharge(prebook);

            //Assert
            //After Update Crimeforce Table
            crimeForce = _fixture.Db.CrimeForce.Single(
                c => c.CrimeForceId == 10);
            Assert.Equal(11, crimeForce.InmatePrebookId);
            //After Update Incarceration Table
            incarceration = _fixture.Db.Incarceration.Single(i => i.IncarcerationId == 20);
            Assert.Equal("M", incarceration.ChargeLevel);
            //After Insert CrimeHistory Table
            crimeHistory = _fixture.Db.CrimeHistory.Single(c => c.CrimeId == 12);
            Assert.NotNull(crimeHistory);
        }

        [Fact]
        public void GetCrimeSearch()
        {
            //Arrange
            CrimeLookupVm crimeLookup = new CrimeLookupVm
            {
                CrimeCodeType = "F",
                CrimeSection = "IRSR101",
                CrimeDescription = "MEDICINE",
                Descriptions = new[]
                {
                   "MEDICINE","ROOM VIOLATION"
                }
            };
            //Act
            IEnumerable<CrimeLookupVm> lstCrimeLookup = _prebookWizardservice.GetCrimeSearch(crimeLookup);

            //Assert
            CrimeLookupVm crime = lstCrimeLookup.Single(c => c.CrimeLookupId == 8);
            Assert.Equal("ROOM VIOLATION", crime.CrimeDescription);
            Assert.Equal("IRSR101", crime.CrimeSection);
        }

        [Fact]
        public async Task InsertCrimeHistory()
        {
            //Arrange
            CrimeHistoryVm crimeHistory = new CrimeHistoryVm
            {
                CreatedBy = 11,
                BailAmount = 4500,
                CrimeDeleteFlag = false,
                CrimeCount = 1,
                CreatDate = DateTime.Now,
                CrimeLookupId = 11,
                CrimeForceId = 11

            };
            //Before Insert CrimeHistory Table
            CrimeHistory history = _fixture.Db.CrimeHistory.SingleOrDefault(c => c.CrimeForceId == 11);
            Assert.Null(history);

            //Act
            await _prebookWizardservice.InsertCrimeHistory(crimeHistory);

            //Assert
            //After Insert CrimeHistory Table
            history = _fixture.Db.CrimeHistory.Single(c => c.CrimeForceId == 11);
            Assert.NotNull(history);
            Assert.Equal(4500, history.BailAmount);
        }

        [Fact]
        public async Task DeleteCrimeForces()
        {
            //Delete Function
            //Before Delete CrimeForce Table
            CrimeForce crimeForce = _fixture.Db.CrimeForce.Single(c => c.CrimeForceId == 12);
            Assert.Equal(0, crimeForce.DeleteFlag);

            //Act
            await _prebookWizardservice.DeleteCrimeForces(12);

            //Assert
            //After Delete CrimeForce Table
            crimeForce = _fixture.Db.CrimeForce.Single(c => c.CrimeForceId == 12);
            Assert.Equal(1, crimeForce.DeleteFlag);

            //Added new row in  CrimeHistory.
            CrimeHistory history = _fixture.Db.CrimeHistory.Last(c => c.CrimeForceId == 12);
            Assert.Equal(11, history.CreatedBy);
            Assert.Equal(1, history.CrimeDeleteFlag);

            //UndoDelete Function
            //Before Delete CrimeForce Table
            crimeForce = _fixture.Db.CrimeForce.Single(c => c.CrimeForceId == 12);
            Assert.Equal(1, crimeForce.DeleteFlag);

            await _prebookWizardservice.UndoDeleteCrimeForces(12);

            //After Delete CrimeForce Table
            crimeForce = _fixture.Db.CrimeForce.Single(c => c.CrimeForceId == 12);
            Assert.Equal(0, crimeForce.DeleteFlag);

            //Added new row in  CrimeHistory.
            history = _fixture.Db.CrimeHistory.Last(c => c.CrimeForceId == 12);
            Assert.Equal(11, history.CreatedBy);
            Assert.Equal(0, history.CrimeDeleteFlag);

        }

        [Fact]
        public void GetCrimeHistory()
        {
            //Act
            IEnumerable<CrimeHistoryVm> crimeHistory = _prebookWizardservice.GetCrimeHistory(12);

            //Assert
            CrimeHistoryVm history = crimeHistory.Single(c => c.CrimeHistoryId == 1502);
            Assert.Equal(4000, history.BailAmount);
        }

        [Fact]
        public void GetVehicleMake()
        {
            //Act
            IEnumerable<VehicleMakeVm> lstVehicleMake = _prebookWizardservice.GetVehicleMake();

            //Assert
            VehicleMakeVm vehicleMake = lstVehicleMake.Single(v => v.VehicleMakeId == 10);
            Assert.Equal("YAMAHA", vehicleMake.VehicleMakeName);
        }

        [Fact]
        public void GetVehicleModel()
        {
            //Act
            IEnumerable<VehicleModelVm> lstVehicleModel = _prebookWizardservice.GetVehicleModel(11);

            //Assert
            VehicleModelVm vehicleModel = lstVehicleModel.Single(v => v.VehicleModelId == 11);
            Assert.Equal("C-CLASS", vehicleModel.VehicleModelName);
            Assert.Equal("BENZ", vehicleModel.ModelCode);
        }

        [Fact]
        public void GetPrebookWarrantById()
        {
            //Act
            InmatePrebookWarrantVm inmatePrebookWarrant = _prebookWizardservice.GetPrebookWarrantById(12);

            //Assert
            Assert.Equal("WARNUM4100", inmatePrebookWarrant.WarrantNumber);
            Assert.Equal("ARREST", inmatePrebookWarrant.WarrantType);
            Assert.Equal(1500, inmatePrebookWarrant.WarrantBailAmount);
        }

        [Fact]
        public async Task InsertPrebookWarrant()
        {
            //Arrange
            InmatePrebookWarrantVm inmatePrebookWarrant = new InmatePrebookWarrantVm
            {
                //Don't give Primary Key Values.
                //InmatePrebookWarrantId = 3,
                InmatePrebookId = 19,
                CreateDate = DateTime.Now.AddDays(-9),
                CreateBy = 11,
                UpdateDate = DateTime.Now.AddDays(-9),
                UpdateBy = 12,
                WarrantId = 8,
                WarrantAgencyId = 5,
                WarrantNumber = "WARNU7845",
                WarrantBailAmount = 4000,
                WarrantDescription = "NO FIR FILE BOOKED",
                WarrantType = "RELEASE",
                WarrantChargeType = "I",
                WarrantIssueDate = DateTime.Now.AddDays(-5),
                WarrantNoBail = true
            };
            //Before Insert
            InmatePrebookWarrant prebookWarrant =
                _fixture.Db.InmatePrebookWarrant.SingleOrDefault(i => i.InmatePrebookId == 19);
            Assert.Null(prebookWarrant);

            //Act
            await _prebookWizardservice.InsertPrebookWarrant(inmatePrebookWarrant);

            //Assert
            //After Insert
            prebookWarrant =
                _fixture.Db.InmatePrebookWarrant.Single(i => i.InmatePrebookId == 19);
            Assert.NotNull(prebookWarrant);
            Assert.Equal("NO FIR FILE BOOKED", prebookWarrant.WarrantDescription);
            Assert.Equal("RELEASE", prebookWarrant.WarrantType);
        }

        [Fact]
        public async Task UpdatePrebookWarrant()
        {
            //Arrange
            InmatePrebookWarrantVm inmatePrebookWarrant = new InmatePrebookWarrantVm
            {
                InmatePrebookWarrantId = 13,
                InmatePrebookId = 18,
                CreateDate = DateTime.Now.AddDays(-9),
                CreateBy = 11,
                UpdateDate = DateTime.Now.AddDays(-9),
                UpdateBy = 12,
                WarrantId = 9,
                WarrantAgencyId = 5,
                ArrestId = 8,
                WarrantNumber = "WARNU7846",
                WarrantBailAmount = 4000,
                WarrantDescription = "DRINK AND DRIVE",
                WarrantType = "HOLD",
                WarrantChargeType = "I",
                WarrantIssueDate = DateTime.Now.AddDays(-5),
                WarrantNoBail = true
            };
            //Before Update
            InmatePrebookWarrant prebookWarrant =
                _fixture.Db.InmatePrebookWarrant.Single(i => i.InmatePrebookWarrantId == 13);
            Assert.Null(prebookWarrant.WarrantId);
            Assert.Null(prebookWarrant.WarrantBailAmount);
            //Act
            await _prebookWizardservice.UpdatePrebookWarrant(inmatePrebookWarrant);

            //Asssert
            //After Update
            prebookWarrant =
                _fixture.Db.InmatePrebookWarrant.Single(i => i.InmatePrebookWarrantId == 13);
            Assert.Equal(9, prebookWarrant.WarrantId);
            Assert.Equal("WARNU7846", prebookWarrant.WarrantNumber);

            //Bail Amount 0 due to WarrantNoBail is true
            Assert.Equal(0, prebookWarrant.WarrantBailAmount);
        }

        [Fact]
        public async Task DeletePrebookWarrant()
        {
            //Delete Function
            //Before Delete
            InmatePrebookWarrant prebookWarrant =
                _fixture.Db.InmatePrebookWarrant.Single(i => i.InmatePrebookWarrantId == 14);
            Assert.Null(prebookWarrant.DeleteFlag);
            Assert.Null(prebookWarrant.DeletedBy);

            //Act
            await _prebookWizardservice.DeletePrebookWarrant(14);

            //Assert
            //After Delete
            prebookWarrant =
                _fixture.Db.InmatePrebookWarrant.Single(i => i.InmatePrebookWarrantId == 14);
            Assert.Equal(1, prebookWarrant.DeleteFlag);
            Assert.Equal(11, prebookWarrant.DeletedBy);


            //Undo Function
            //Before UndoDelete
            prebookWarrant =
                _fixture.Db.InmatePrebookWarrant.Single(i => i.InmatePrebookWarrantId == 14);
            Assert.Equal(1, prebookWarrant.DeleteFlag);

            //Act
            await _prebookWizardservice.UndoDeletePrebookWarrant(14);

            //After UndoDelete
            //Assert
            prebookWarrant =
                _fixture.Db.InmatePrebookWarrant.Single(i => i.InmatePrebookWarrantId == 14);
            Assert.Null(prebookWarrant.DeleteFlag);
        }

        [Fact]
        public void GetPersonalInventoryPrebook()
        {
            //Act
            IEnumerable<PersonalInventoryVm> lstPersonalInventory = _prebookWizardservice.GetPersonalInventoryPrebook(8, true);

            //Assert
            PersonalInventoryVm personalInventory = lstPersonalInventory.Single(p => p.PersonalInventoryPreBookId == 8);
            Assert.Equal("WATCH", personalInventory.InventoryArticlesName);

        }

        [Fact]
        public async Task InsertPersonalInventoryPrebook()
        {
            //Arange
            PersonalInventoryVm[] lstPersonalInventory = new[]
            {
                new PersonalInventoryVm
                {
                   InmatePrebookId = 9,
                    InventoryArticles= 12,
                    InventoryQuantity = 10,
                    InventoryDescription="MONEY FOR CLOTHES",
                   InventoryColor="BLUE"
                },
                new PersonalInventoryVm
                {
                    InmatePrebookId = 10,
                    InventoryArticles= 22,
                    InventoryQuantity = 15,
                    InventoryDescription="FOR MEDICINE",
                    InventoryColor="BLUE"
                }
            };

            //Before Insert Multiple rows in PersonalInventoryPreBook Table
            PersonalInventoryPreBook personalInventoryPreBook = _fixture.Db.PersonalInventoryPreBook.SingleOrDefault(
                p => p.InmatePrebookId == 9);
            Assert.Null(personalInventoryPreBook);

            personalInventoryPreBook = _fixture.Db.PersonalInventoryPreBook.SingleOrDefault(
                p => p.InmatePrebookId == 10);
            Assert.Null(personalInventoryPreBook);

            //Act
            await _prebookWizardservice.InsertPersonalInventoryPrebook(lstPersonalInventory);

            //Assert
            //After Insert Multiple rows in PersonalInventoryPreBook Table
            personalInventoryPreBook = _fixture.Db.PersonalInventoryPreBook.Single(
                p => p.InmatePrebookId == 9);
            Assert.NotNull(personalInventoryPreBook);
            Assert.Equal("MONEY FOR CLOTHES", personalInventoryPreBook.InventoryDescription);

            personalInventoryPreBook = _fixture.Db.PersonalInventoryPreBook.Single(
                p => p.InmatePrebookId == 10);
            Assert.NotNull(personalInventoryPreBook);
            Assert.Equal("BLUE", personalInventoryPreBook.InventoryColor);
        }

        [Fact]
        public async Task DeletePersonalInventoryPrebook()
        {
            //Delete Function
            //Before Delete
            PersonalInventoryPreBook personalInventoryPreBook = _fixture.Db.PersonalInventoryPreBook.Single(
                p => p.InmatePrebookId == 11);
            Assert.Equal(0, personalInventoryPreBook.DeleteFlag);

            //Act
            await _prebookWizardservice.DeletePersonalInventoryPrebook(9);

            //Assert
            //After Delete
            personalInventoryPreBook = _fixture.Db.PersonalInventoryPreBook.Single(
                p => p.InmatePrebookId == 11);
            Assert.Equal(1, personalInventoryPreBook.DeleteFlag);


            //UndoDelete Function
            //Before Undo
            personalInventoryPreBook = _fixture.Db.PersonalInventoryPreBook.Single(
                           p => p.InmatePrebookId == 11);
            Assert.Equal(1, personalInventoryPreBook.DeleteFlag);

            //Act
            await _prebookWizardservice.UndoDeletePersonalInventoryPrebook(9);

            //Assert
            //After Undo
            personalInventoryPreBook = _fixture.Db.PersonalInventoryPreBook.Single(
                p => p.InmatePrebookId == 11);
            Assert.Null(personalInventoryPreBook.DeleteFlag);
        }

        [Fact]
        public void GetPreBookForms()
        {
            //Act
            IEnumerable<GetFormTemplates> lsFormTemplateses = _prebookWizardservice.GetPreBookForms(6, FormScreen.Courtcommit);

            //Assert
            GetFormTemplates formTemplates = lsFormTemplateses.Single(f => f.FormTemplatesId == 14);
            Assert.Equal("\\FORMTEMPLATES\\COURTCOMMIT", formTemplates.HtmlPath);
        }

        [Fact]
        public void LoadSavedForms()
        {
            //Act
            IEnumerable<LoadSavedForms> lstLoadSavedFormses = _prebookWizardservice.LoadSavedForms(5, 10, FormScreen.PrebookIntake, false);

            //Assert
            LoadSavedForms loadSaved = lstLoadSavedFormses.Single(l => l.FormRecordId == 13);
            Assert.Equal("BOOKING", loadSaved.DisplayName);
            Assert.Equal(5, loadSaved.InmatePrebookId);
        }

        [Fact]
        public async Task DeletePreBookForm()
        {
            //Before Detele
            FormRecord formRecord = _fixture.Db.FormRecord.Single(f => f.FormRecordId == 14);
            Assert.Equal(0, formRecord.DeleteFlag);
            Assert.Null(formRecord.DeleteBy);

            //Act
            await _prebookWizardservice.DeletePreBookForm(14);

            //Assert
            //After Delete
            formRecord = _fixture.Db.FormRecord.Single(f => f.FormRecordId == 14);
            Assert.Equal(1, formRecord.DeleteFlag);
            Assert.Equal(11, formRecord.DeleteBy);
        }

        [Fact]
        public void AddForm()
        {
            //Act
            AddForm addForm = _prebookWizardservice.AddForm(12, 15);

            //Assert
            Assert.Equal("MEDICAL PRE-SCREENING", addForm.DisplayName);
            Assert.Equal("MANI", addForm.PersonFirstName);
        }

        [Fact]
        public void ListForm()
        {
            //Act
            ListForm listForm = _prebookWizardservice.ListForm(13);

            //Assert
            Assert.Equal("BOOKING", listForm.DisplayName);
            Assert.Equal("VIJAYA", listForm.PersonLastName);
        }

        [Fact]
        public async Task UpdateForm()
        {
            //For Insert
            //Assert
            LoadSavedForms forms = new LoadSavedForms
            {
                UpdateBy = 11,
                FormTemplatesId = 11,
                XmlStr = "<Form_Details>< person_name > ABINAYA</ person_name ></ Form_Details > ",
                InmatePrebookId = 12
            };
            //Before Insert
            FormRecord formRecord = _fixture.Db.FormRecord.SingleOrDefault(f => f.InmatePrebookId == 12);
            Assert.Null(formRecord);

            //Act
            await _prebookWizardservice.UpdateForm(forms);

            //Assert
            //After Insert
            formRecord = _fixture.Db.FormRecord.Single(f => f.InmatePrebookId == 12);
            Assert.NotNull(formRecord);


            //For Update
            forms = new LoadSavedForms
            {
                FormRecordId = 15,
                Notes = "GATHERED PERSON XML DATA",
                UpdateBy = 12,
                XmlStr = "< person_name_lfm > MANI </ person_name_lfm >< person_AKA > YOGI </ person_AKA >< stampdate > 10 / 10 / 2017 </ stampdate > < person_doc > 784754579 </ person_doc > < person_dob > 03 / 06 / 1992 </ person_dob > "

            };
            //Before Update
            formRecord = _fixture.Db.FormRecord.Single(f => f.FormRecordId == 15);
            Assert.Null(formRecord.FormNotes);
            Assert.Equal(11, formRecord.UpdateBy);

            //Act
            await _prebookWizardservice.UpdateForm(forms);

            //Assert
            //After Update
            formRecord = _fixture.Db.FormRecord.Single(f => f.FormRecordId == 15);
            Assert.NotNull(formRecord.FormNotes);
            Assert.Equal(12, formRecord.UpdateBy);

        }

        [Fact]
        public void GetPrebookAttachment()
        {
            //Arrange
            AttachmentSearch attachmentSearch = new AttachmentSearch
            {
                InmateprebookId = 6,
                ActiveFlag = 0,
                Flag = "INCARATTACHTYPE",
                FacilityId = 2
            };

            //Assert
            IEnumerable<PrebookAttachment> lstPrebookAttachments = _prebookWizardservice.GetPrebookAttachment(attachmentSearch);
            PrebookAttachment attachment = lstPrebookAttachments.Single(p => p.InmateId == 105);
            Assert.Equal("BELT", attachment.AttachmentKeyword1);
        }

        //Temporary hold this method
        //[Fact]
        //public async Task InsertPrebookAttachment()
        //{
        //    PrebookAttachment inmatePrebook = new PrebookAttachment
        //    {
        //        ArrestId = 5,
        //        AttachType = "BOOKATTACHTYPE",
        //        AttachmentTitle = "UNIT TEST",
        //        AttachmentType = "OTHER",
        //        AttachmentFile = "Class file_201905161742.png",
        //        History =
        //            @"{ 'Inmate':'Automate, Test, AT, 1900101590','Booking':'19000055.1',Type:'BOOKING SHEET','Title':'TEST','Keyword1':'','Keyword5':''}"
        //    };
        //    await _prebookWizardservice.InsertPrebookAttachment(inmatePrebook);
        //}

        [Fact]
        public void LoadPrebookAttachmentEntry()
        {
            //Act
            AttachmentComboBoxes attachmentCombo = _prebookWizardservice.LoadPrebookAttachmentEntry("BOOKATTACHTYPE", 105, 2);

            //Assert
            Assert.True(attachmentCombo.BookingNumers.Count > 1);
            Assert.True(attachmentCombo.LookupTypes.Count > 0);


        }
        [Fact]
        public void LoadAttachHistory()
        {
            //Act
            List<HistoryVm> lstHistory = _prebookWizardservice.LoadAttachHistory(15);

            //Assert
            HistoryVm history = lstHistory.Single(h => h.PersonId == 55);
            Assert.Equal("1729", history.OfficerBadgeNumber);
            Assert.Equal("KRISHNA", history.PersonLastName);
            Assert.EndsWith("InmateId", history.Header[0].Header);
            Assert.EndsWith("100", history.Header[0].Detail);
            Assert.EndsWith("InmateCurrentTrack", history.Header[2].Header);
            Assert.EndsWith("CHENNAI", history.Header[2].Detail);
        }
        [Fact]
        public async Task DeletePrebookAttachment()
        {
            //Before Delete
            AppletsSaved appletsSaved = _fixture.Db.AppletsSaved.Single(s => s.AppletsSavedId == 18);
            Assert.Equal(0, appletsSaved.AppletsDeleteFlag);

            //Before Insert History table
            AppletsSavedHistory history = _fixture.Db.AppletsSavedHistory.SingleOrDefault
                (a => a.AppletsSavedId == 18);
            Assert.Null(history);

            //Act
            await _prebookWizardservice.DeletePrebookAttachment(18);

            //Assert
            //After Delete
            appletsSaved = _fixture.Db.AppletsSaved.Single(s => s.AppletsSavedId == 18);
            Assert.Equal(1, appletsSaved.AppletsDeleteFlag);

            //After Insert History table
            history = _fixture.Db.AppletsSavedHistory.Single
                (a => a.AppletsSavedId == 18);
            Assert.NotNull(history);

        }

        [Fact]
        public async Task UndoDeletePrebookAttachment()
        {
            //Arrange
            string history = "@{ 'InmateId':'103', 'InmateSiteNumber':'CHS000', 'InmateCurrentTrack':'CHENNAI', 'Type':'STORE ROOM', 'AppletsSavedKeyword1' : 'BELT','AppletsSavedKeyword4' : 'CHAIN'}";

            //Before Undo 
            AppletsSaved appletsSaved = _fixture.Db.AppletsSaved.Single(s => s.AppletsSavedId == 20);
            Assert.Equal(1, appletsSaved.AppletsDeleteFlag);

            //Before Insert  history  Table
            AppletsSavedHistory appletsSavedHistory = _fixture.Db.AppletsSavedHistory.SingleOrDefault
           (a => a.AppletsSavedId == 20);
            Assert.Null(appletsSavedHistory);

            //Act
            await _prebookWizardservice.UndoDeletePrebookAttachment(20, history);

            //Assert
            //After Undo
            appletsSaved = _fixture.Db.AppletsSaved.Single(s => s.AppletsSavedId == 20);
            Assert.Equal(0, appletsSaved.AppletsDeleteFlag);

            //After Insert History Table
            appletsSavedHistory = _fixture.Db.AppletsSavedHistory.SingleOrDefault
                (a => a.AppletsSavedId == 20);
            Assert.NotNull(appletsSavedHistory);

        }
        [Fact]
        public void GetAttachmentDetail()
        {
            //Act
            PrebookAttachment prebookAttachment = _prebookWizardservice.GetAttachmentDetail(17);

            //Assert
            Assert.Equal("MOBILE PHONE", prebookAttachment.AttachmentKeyword2);
            Assert.Equal("PHANTS", prebookAttachment.AttachmentKeyword5);
        }

        [Fact]
        public async Task UpdateMedPrescreenStatusStartComplete()
        {
            //Arrange
            MedPrescreenStatus medPrescreen = new MedPrescreenStatus
            {
                PrebookId = 25,
                MedCompleteFlag = 1
            };

            //Before Update
            InmatePrebook inmatePrebook = _fixture.Db.InmatePrebook.Single(i => i.InmatePrebookId == 25);
            Assert.Equal(0, inmatePrebook.CompleteFlag);

            //Act
            await _prebookWizardservice.UpdateMedPrescreenStatusStartComplete(medPrescreen);

            //Assert
            //After Update
            inmatePrebook = _fixture.Db.InmatePrebook.Single(i => i.InmatePrebookId == 25);
            Assert.Equal(1, inmatePrebook.CompleteFlag);
        }

        [Fact]
        public async Task UpdateMedPrescreenStatus()
        {
            //Arrange
            MedPrescreenStatus medPrescreenStatus = new MedPrescreenStatus
            {
                PrebookId = 26,
                MedPrescreenStatusFlag = MedPrescreenStatusFlag.Bypass,
                MedCompleteFlag = 1
            };

            //Before Update
            InmatePrebook inmatePrebook = _fixture.Db.InmatePrebook.Single(i => i.InmatePrebookId == 26);
            Assert.Equal(0, inmatePrebook.CompleteFlag);

            //Before Insert
            InmatePrebookMedPrescreenHistory prescreenHistory = _fixture.Db.InmatePrebookMedPrescreenHistory.SingleOrDefault(
                i => i.InmatePrebookId == 26);
            Assert.Null(prescreenHistory);

            //Act
            await _prebookWizardservice.UpdateMedPrescreenStatus(medPrescreenStatus);

            //Assert
            //Before Update
            inmatePrebook = _fixture.Db.InmatePrebook.Single(i => i.InmatePrebookId == 26);
            Assert.Equal(1, inmatePrebook.CompleteFlag);

            //After Insert
            prescreenHistory = _fixture.Db.InmatePrebookMedPrescreenHistory.Single(
               i => i.InmatePrebookId == 26);
            Assert.NotNull(prescreenHistory);
        }

        [Fact]
        public void GetAttachmentHistory()
        {
            //Act
            IEnumerable<AppletsSavedVm> lstAppletsSaved = _prebookWizardservice.GetAttachmentHistory(6);

            //Assert
            Assert.True(lstAppletsSaved.Count() > 0);

        }

        [Fact]
        public async Task UpdatePrebookUndoComplete()
        {
            //Before Update
            InmatePrebook inmatePrebook = _fixture.Db.InmatePrebook.Single(i => i.InmatePrebookId == 25);
            Assert.Equal(1, inmatePrebook.CompleteFlag);

            //Act
            await _prebookWizardservice.UpdatePrebookUndoComplete(25);

            //Assert
            //After Update
            inmatePrebook = _fixture.Db.InmatePrebook.Single(i => i.InmatePrebookId == 25);
            Assert.Equal(0, inmatePrebook.CompleteFlag);
        }

        [Fact]
        public async Task CreateVehicleModel()
        {
            //Arrange
            VehicleModelVm vehicleModelvm = new VehicleModelVm
            {
                VehicleMakeId = 11,
                DeleteFlag = 0,
                ModelCode = "BENZ",
                VehicleModelName = "G-CLASS"
            };
            //Before Insert
            VehicleModel vehicleModel = _fixture.Db.VehicleModel.SingleOrDefault(v => v.VehicleModelName == "G-CLASS");
            Assert.Null(vehicleModel);

            //Act
            await _prebookWizardservice.CreateVehicleModel(vehicleModelvm);

            //Assert
            //After Insert
            vehicleModel = _fixture.Db.VehicleModel.Single(v => v.VehicleModelName == "G-CLASS");
            Assert.NotNull(vehicleModel);
        }

        [Fact]
        public async Task UpdateVehicleModel()
        {
            //Arrange
            VehicleModelVm vehicleModelvm = new VehicleModelVm
            {
                DeleteFlag = 1,
                VehicleModelId = 13
            };
            //Before Update
            VehicleModel vehicleModel = _fixture.Db.VehicleModel.Single(v => v.VehicleModelId == 13);
            Assert.Equal(0, vehicleModel.DeleteFlag);

            //Act
            await _prebookWizardservice.UpdateVehicleModel(vehicleModelvm);

            //Assert
            //After Update
            vehicleModel = _fixture.Db.VehicleModel.Single(v => v.VehicleModelId == 13);
            Assert.Equal(1, vehicleModel.DeleteFlag);

        }

        [Fact]
        public async Task CreateVehicleMake()
        {
            //Arrange
            VehicleMakeVm vehicleMakeVm = new VehicleMakeVm
            {
                VehicleMakeName = "HONDA CITY",
                MakeCode = "HONDA"
            };
            //Before Insert
            VehicleMake vehicleMake = _fixture.Db.VehicleMake.SingleOrDefault(v => v.MakeCode == "HONDA");
            Assert.Null(vehicleMake);

            //Act
            await _prebookWizardservice.CreateVehicleMake(vehicleMakeVm);

            //Assert
            //After Insert
            vehicleMake = _fixture.Db.VehicleMake.Single(v => v.MakeCode == "HONDA");
            Assert.NotNull(vehicleMake);

        }

        [Fact]
        public async Task UpdateVehicleMake()
        {
            //Arrange
            VehicleMakeVm vehicleMakeVm = new VehicleMakeVm
            {
                VehicleMakeId = 12,
                DeleteFlag = 1
            };
            //Before Update
            VehicleMake vehicleMake = _fixture.Db.VehicleMake.Single(v => v.VehicleMakeId == 12);
            Assert.Equal(0, vehicleMake.DeleteFlag);

            //Act
            await _prebookWizardservice.UpdateVehicleMake(vehicleMakeVm);

            //Assert
            //After Update
            vehicleMake = _fixture.Db.VehicleMake.Single(v => v.VehicleMakeId == 12);
            Assert.Equal(1, vehicleMake.DeleteFlag);
        }

        [Fact]
        public void GetPrebookCases()
        {
            //Act
            List<InmatePrebookCaseVm> lstInmatePrebookCases = _prebookWizardservice.GetPrebookCases(13, true);

            //Assert
            Assert.True(lstInmatePrebookCases.Count > 0);
        }

        [Fact]
        public void InsertUpdatePrebookCase()
        {
            //arrange
            List<InmatePrebookCaseVm> inmatePrebooks = new List<InmatePrebookCaseVm>
            {
                new InmatePrebookCaseVm
                {
                    InmatePrebookId = 9,
                    InmatePrebookCaseId = 15,
                    DeleteFlag = false,
                    CaseTypeId = 34.5m,
                    CaseNumber = "IC-10010",
                    CaseNote = null
                }
            };
            //Before Update
            InmatePrebookCase inmatePrebookCase = _fixture.Db.InmatePrebookCase.Single(i => i.InmatePrebookCaseId == 15);
            Assert.Null(inmatePrebookCase.CaseNumber);

            //Act
            List<InmatePrebookCaseVm> lstInmatePrebookCases = _prebookWizardservice.InsertUpdatePrebookCase(inmatePrebooks, true);

            //Assert
            //After Update
            Assert.True(lstInmatePrebookCases.Count > 0);
            inmatePrebookCase = _fixture.Db.InmatePrebookCase.Single(i => i.InmatePrebookCaseId == 15);
            Assert.Equal("IC-10010", inmatePrebookCase.CaseNumber);

        }

        [Fact]
        public async Task DeleteUndoPrebookCase()
        {
            //Arrange
            InmatePrebookCaseVm inmatePrebookCase = new InmatePrebookCaseVm
            {
                DeleteFlag = true,
                InmatePrebookCaseId = 16,
                DeleteReason = "OTHER"
            };
            //Before Delete
            InmatePrebookCase inmatePrebook = _fixture.Db.InmatePrebookCase.Single(i => i.InmatePrebookCaseId == 16);
            Assert.False(inmatePrebook.DeleteFlag);

            //Act
            await _prebookWizardservice.DeleteUndoPrebookCase(inmatePrebookCase);

            //Assert
            //After Delete
            inmatePrebook = _fixture.Db.InmatePrebookCase.Single(i => i.InmatePrebookCaseId == 16);
            Assert.True(inmatePrebook.DeleteFlag);

        }

        //[Fact]
        //public void GetPropertyDetails()
        //{
        //    //Arrange
        //    PrebookProperty prebookProperty = new PrebookProperty();

        //    //Act
        //    PrebookProperty property = _prebookWizardservice.GetPropertyDetails(prebookProperty);

        //    //Assert
        //    Assert.Equal("SENIOR ADVOCATE", property.InmateHeaderDetails.AgencyName);
        //    Assert.Equal("PREBOOK PROPERTY", property.InmateHeaderDetails.SummaryHeader);

        //}

        [Fact]
        public void GetDefaultAgencyId()
        {
            //Act
            int agencyId = _prebookWizardservice.GetDefaultAgencyId();

            //Assert
            Assert.Equal(5, agencyId);
        }

    }
}

