using GenerateTables.Models;
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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

// ReSharper disable once CheckNamespace
namespace ServerAPI.JMS.Tests
{
    [Collection("Database collection")]
    public class IntakePrebookServiceTest
    {
        private readonly IntakePrebookService _intakeprebook;
        private readonly IConfigurationRoot _configuration = MockHelpers.GetIConfigurationRoot();
        private readonly DbInitialize _fixture;
        private readonly Mock<IMemoryCache> _memory = new Mock<IMemoryCache>();

        public IntakePrebookServiceTest(DbInitialize fixture)
        {
            _fixture = fixture;
            Mock<UserManager<AppUser>> userMgr = MockHelpers.MockUserManager<AppUser>();
            AtimsHubService atimsHubService = new AtimsHubService(fixture.HubContext.Object);
            AppletsSavedService appletsSavedService = new AppletsSavedService(_fixture.Db, _configuration);
            PersonService personService = new PersonService(_fixture.Db);
            PhotosService photosService = new PhotosService(_fixture.Db, _configuration);
            HttpContextAccessor httpContext = new HttpContextAccessor { HttpContext = fixture.Context.HttpContext };
            InterfaceEngineService interfaceEngineService = new InterfaceEngineService(fixture.Db, _configuration,
                _memory.Object);
            CommonService commonService = new CommonService(fixture.Db, _configuration,
                httpContext,personService,appletsSavedService,interfaceEngineService);
            FacilityPrivilegeService facilityPrivilegeService = new FacilityPrivilegeService(fixture.Db, httpContext,
                personService);
            WizardService wizardService = new WizardService(_fixture.Db,atimsHubService);
            InmateService inmateService = new InmateService(_fixture.Db, commonService, personService, httpContext,
               facilityPrivilegeService,photosService);
            _intakeprebook = new IntakePrebookService(fixture.Db, commonService, httpContext,personService,inmateService,
                atimsHubService,wizardService,interfaceEngineService);
        }

        [Fact]
        public void IntakePrebook_GetIntakePrebookCount()
        {
            //Act
            PrebookCountVm prebookCount = _intakeprebook.GetIntakePrebookCount(1, 14, "notReviewed",false);

            //Assert
            Assert.Equal("notReviewed", prebookCount.PrebookDetails);
        }

        [Fact]
        public void IntakePrebook_GetListPrebookDetails()
        {
            //Act
            List<InmatePrebookVm> lstInmatePrebook = _intakeprebook
                .GetListPrebookDetails(2, PrebookDetails.CourtCommitSch, "2019-02-07");

            //Assert
            Assert.NotNull(lstInmatePrebook);
        }

        [Fact]
        public async Task IntakePrebook_DeleteInmatePrebook()
        {
            InmatePrebook inmatePrebook = _fixture.Db.InmatePrebook.Single(i => i.InmatePrebookId == 15);
            Assert.Null(inmatePrebook.DeleteFlag);

            //Act
            await _intakeprebook.DeleteInmatePrebook(15);

            //Assert
            inmatePrebook = _fixture.Db.InmatePrebook.Single(i => i.InmatePrebookId == 15);
            Assert.Equal(1, inmatePrebook.DeleteFlag);
        }

        [Fact]
        public async Task IntakePrebook_UpdateInmatePrebookPersonDetails()
        {
            InmatePrebook inmatePrebook = _fixture.Db.InmatePrebook.Single(i => i.InmatePrebookId == 19);
            Assert.Equal(80, inmatePrebook.PersonId);

            //Act   
            await _intakeprebook.UpdateInmatePrebookPersonDetails(19, 80);

            //Assert
            inmatePrebook = _fixture.Db.InmatePrebook.Single(i => i.InmatePrebookId == 19);
            Assert.Null(inmatePrebook.PersonId);
        }


        [Fact]
        public async Task IntakePrebook_UpdateTemporaryHold()
        {
            InmatePrebook inmatePrebook = _fixture.Db.InmatePrebook.Single(i => i.InmatePrebookId == 18);
            Assert.Null(inmatePrebook.TemporaryHold);

            //Act
            await _intakeprebook.UpdateTemporaryHold(18);

            //Assert
            inmatePrebook = _fixture.Db.InmatePrebook.Single(i => i.InmatePrebookId == 18);
            Assert.Equal(1, inmatePrebook.TemporaryHold);
        }

        [Fact]
        public void IntakePrebook_GetInmatePrebookDetails()
        {
            //Using CourtCommitFlag= 1 get records from InmatePrebookCase
            //Act
            InmatePrebookVm inmatePrebook = _intakeprebook.GetInmatePrebookDetails(13);

            //Assert
            Assert.Equal("IC-10003", inmatePrebook.CaseNumber);
            Assert.Equal(65, inmatePrebook.PersonId);

            //Using CourtCommitFlag = 0 get records from InmatePrebookCase
            inmatePrebook = _intakeprebook.GetInmatePrebookDetails(18);
            Assert.Equal("IC-10001,IC-10002", inmatePrebook.CaseNumber);
            Assert.Equal("B-4478500", inmatePrebook.PrebookNumber);

            //Set SiteOptionsValue = 'ON'
            inmatePrebook = _intakeprebook.GetInmatePrebookDetails(18);
            Assert.Equal("IC-10001,IC-10002", inmatePrebook.CaseNumber);
        }

        [Fact]
        public async Task IntakePrebook_UndoInmatePrebook()
        {
            InmatePrebook inmatePrebook = _fixture.Db.InmatePrebook.Single(i => i.InmatePrebookId == 17);
            Assert.Equal(1, inmatePrebook.DeleteFlag);
            Assert.NotNull(inmatePrebook.DeleteDate);

            //Act
            await _intakeprebook.UndoInmatePrebook(17);

            //Assert
            inmatePrebook = _fixture.Db.InmatePrebook.Single(i => i.InmatePrebookId == 17);
            Assert.Null(inmatePrebook.DeleteFlag);
            Assert.Null(inmatePrebook.DeleteDate);
        }

        //This method will fail due to using transaction, when comment that line it will work.
        //[Fact]
        //public async Task IntakePrebook_InsertIntakeProcess()
        //{
        //    IntakeEntryVm intakeEntry = new IntakeEntryVm()
        //    {
        //        PersonId = 126,
        //        InmateId = 140,
        //        FacilityId = 2,
        //        PersonLastName = null,
        //        PersonFirstName = "BHARATH",
        //        PersonMiddleName = "VAJ",
        //        PersonSuffix = "BV",
        //        BookAndRelease = true,
        //        InmatePreBookId = 12
        //    };

        //    Incarceration incarceration = _fixture.Db.Incarceration.Single(i => i.IncarcerationId == 22);
        //    Assert.Null(incarceration.ExternalToDo);


        //    await _intakeprebook.InsertIntakeProcess(intakeEntry);

        //    incarceration = _fixture.Db.Incarceration.Single(i => i.IncarcerationId == 22);
        //    Assert.Equal(1, incarceration.ExternalToDo);
        //}

    }
}
