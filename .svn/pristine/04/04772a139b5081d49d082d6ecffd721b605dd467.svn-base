using System.Collections.Generic;
using ServerAPI.Tests;
using Xunit;
using ServerAPI.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using ServerAPI.ViewModels;
using GenerateTables.Models;
using System.Linq;
using ServerAPI.JMS.Tests.Helper;
using Moq;
using Microsoft.AspNetCore.Identity;
using JwtDb.Models;
using Microsoft.Extensions.Caching.Memory;

// ReSharper disable once CheckNamespace
namespace ServerAPI.JMS.Tests
{
    [Collection("Database collection")]
    public class PersonProfileServiceTest
    {
        private readonly IConfigurationRoot _configuration = MockHelpers.GetIConfigurationRoot();
        private readonly PersonProfileService _personProfileService;
        private readonly DbInitialize _fixture;
        private readonly Mock<IMemoryCache> _memory = new Mock<IMemoryCache>();

        public PersonProfileServiceTest(DbInitialize fixture)
        {
            _fixture = fixture;
            Mock<UserManager<AppUser>> userMgr = MockHelpers.MockUserManager<AppUser>();
            AppletsSavedService appletsSavedService = new AppletsSavedService(_fixture.Db, _configuration);
            AtimsHubService atimsHubService = new AtimsHubService(_fixture.HubContext.Object);
            PersonService personService = new PersonService(_fixture.Db);
            PhotosService photosService = new PhotosService(fixture.Db, _configuration);
            HttpContextAccessor httpContext = new HttpContextAccessor { HttpContext = fixture.Context.HttpContext };

            InterfaceEngineService interfaceEngineService = new InterfaceEngineService(_fixture.Db,
                _configuration, _memory.Object);
            CommonService commonService = new CommonService(fixture.Db, _configuration,
                httpContext, personService, appletsSavedService, interfaceEngineService);
            PrebookActiveService prebookActiveService = new PrebookActiveService(fixture.Db, httpContext,
                atimsHubService);
            PersonAkaService personAkaService = new PersonAkaService(fixture.Db, commonService, httpContext, personService,
                interfaceEngineService);
            PersonIdentityService personIdentityService = new PersonIdentityService(
                fixture.Db, httpContext, prebookActiveService, personAkaService, personService, interfaceEngineService);
            _personProfileService = new PersonProfileService(fixture.Db, personIdentityService, httpContext);
        }

        [Fact]
        public void PersonProfile_GetProfileDetails()
        {
            //Act
            PersonProfileVm personProfile = _personProfileService.GetProfileDetails(126, 141);

            //Assert
            Assert.NotNull(personProfile);
            Assert.Equal(1, personProfile.MaritalStatus);

        }

        [Fact]
        public void PersonProfile_InsertUpdatePersonProfile()
        {
            //Arrange
            PersonProfileVm personProfile = new PersonProfileVm
            {
                PersonId = 127,
                MaritalStatus = 1,
                USCitizen = true,
                PrimLang = 1,
                Interpreter = "1",
                Ethnicity = 5,
                IllegalAlien = true,
                Religion = "CRISTIAN",
                Citizen = "UNITED STATES",
                GenderIdentity = "MALE",
                EduDegree = "M.S COMPUTER SCIENCE",
                EduGrade = "HIGH SCHOOL"
            };

            //Before Insert
            Person person = _fixture.Db.Person.Single(p => p.PersonId == 127);
            Assert.Null(person.PersonReligion);
            PersonDescription personDescription = _fixture.Db.PersonDescription
                .Single(p => p.PersonDescriptionId == 13);

            Assert.Null(personDescription.PersonMaritalStatus);

            //Act
            _personProfileService.InsertUpdatePersonProfile(personProfile);

            //After Insert
            //Assert
            Assert.Equal("CRISTIAN", person.PersonReligion);
            Assert.Equal(1, personDescription.PersonMaritalStatus);
        }

        [Fact]
        public void PersonProfile_GetSkillAndTradedetails()
        {
            //Act
            List<PersonSkillTradeVm> lstPersonSkill = _personProfileService.GetSkillAndTradedetails(65);

            //Assert
            PersonSkillTradeVm personSkillTrade = lstPersonSkill.Single(p => p.SkillAndTradeId == 10);
            Assert.Equal("STEAMFITTER", personSkillTrade.SkillTrade);

        }

        [Fact]
        public void PersonProfile_InsertSkillAndTradeDetails()
        {
            //Arrange
            PersonProfileVm personProfile = new PersonProfileVm
            {
                PersonId = 75,
                SkillTrade = new List<PersonSkillTradeVm>
                    {
                       new PersonSkillTradeVm
                       {
                           SkillTrade="AGRICULTURAL - FRUIT GROWER",
                           IsSkillTrade= true
                       }
                    }
            };

            PersonSkillAndTrade personSkillAndTrade = _fixture.Db.PersonSkillAndTrade
                .SingleOrDefault(p => p.PersonId == 75);

            Assert.Null(personSkillAndTrade);
            //Act
            _personProfileService.InsertSkillAndTradeDetails(personProfile);
            //Assert
            personSkillAndTrade = _fixture.Db.PersonSkillAndTrade.Single(p => p.PersonId == 75);
            Assert.NotNull(personSkillAndTrade);

            //Delete Function
            //Arrange
            personProfile = new PersonProfileVm
            {
                PersonId = 55,
                SkillTrade = new List<PersonSkillTradeVm>
                    {
                       new PersonSkillTradeVm
                       {
                           SkillTrade="COOK",
                           SkillAndTradeId = 11
                       }
                    }
            };
            personSkillAndTrade = _fixture.Db.PersonSkillAndTrade.Single(p => p.PersonSkillAndTradeId == 11);
            Assert.NotNull(personSkillAndTrade);

            //Act
            _personProfileService.InsertSkillAndTradeDetails(personProfile);

            //Assert
            personSkillAndTrade = _fixture.Db.PersonSkillAndTrade
                .SingleOrDefault(p => p.PersonSkillAndTradeId == 11);
            Assert.Null(personSkillAndTrade);
        }

        [Fact]
        public void PersonProfile_GetWorkCrewRequestDetails()
        {
            //Act
            WorkCrowAndFurloughRequest workCrowAndFurlough = _personProfileService.GetWorkCrewRequestDetails(2, 103);

            //Assert
            Assert.Equal("PAINT CREW", workCrowAndFurlough.AssignedCrewName);
            Assert.Equal("FEMALE", workCrowAndFurlough.Gender);
        }

        [Fact]
        public void PersonProfile_InsertWorkCrowAndFurloughRequest()
        {
            //Arrange
            WorkCrewRequestVm workCrew = new WorkCrewRequestVm
            {
                WorkCrewLookupId = 7
            };
            WorkCrowAndFurloughRequest workCrowAndFurlough = new WorkCrowAndFurloughRequest
            {
                LstWorkCrowAndFurlough = new List<WorkCrewRequestVm> { workCrew },
                InmateId = 105,
                RequestNote = "PENDING LIST"
            };
            WorkCrewRequest workCrewRequest = _fixture.Db.WorkCrewRequest
                .SingleOrDefault(w => w.InmateId == 105);

            Assert.Null(workCrewRequest);
            //Act
            _personProfileService.InsertWorkCrowAndFurloughRequest(workCrowAndFurlough);

            //Assert
            workCrewRequest = _fixture.Db.WorkCrewRequest.Single(w => w.InmateId == 105);
            Assert.NotNull(workCrewRequest);
        }

        [Fact]
        public void PersonProfile_GetProgramAndClass()
        {
            //Act
            ProgramDetails lstProgramAndClass = _personProfileService.GetProgramAndClass(104, 2);
            //Assert
            Assert.NotNull(lstProgramAndClass);
        }

        [Fact]
        public void PersonProfile_ValidateProgram()
        {
            //Arrange
            ProgramAndClass programAndClass = new ProgramAndClass
            {
                ProgramId = 6
            };
            ProgramDetails programDetails = new ProgramDetails
            {
                InmateId = 125,
                PriorityLevel = 4,
                RequestNote = "REQUEST FOR NEXT PROGRAM",
                LstProgramAndClass = new List<ProgramAndClass> { programAndClass }
            };

            ProgramRequest programRequest = _fixture.Db.ProgramRequest.SingleOrDefault(p => p.InmateId == 125);
            Assert.Null(programRequest);
            //Act
            _personProfileService.ValidateProgam(programDetails);

            //Assert
            programRequest = _fixture.Db.ProgramRequest.Single(p => p.InmateId == 125);
            Assert.NotNull(programRequest);
        }
    }
}
