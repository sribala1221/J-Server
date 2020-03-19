using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using ServerAPI.Services;
using ServerAPI.Tests;
using ServerAPI.ViewModels;
using System;
using System.Collections.Generic;
using Xunit;
using System.Linq;
using System.Threading.Tasks;
using GenerateTables.Models;
using ServerAPI.JMS.Tests.Helper;
using Moq;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.AspNetCore.Identity;
using JwtDb.Models;

// ReSharper disable once CheckNamespace
namespace ServerAPI.JMS.Tests
{
    [Collection("Database collection")]
    public class InmateVisitationServiceTest
    {
        private readonly DbInitialize _fixture;
        private readonly InmateVisitationService _inmateVisitationService;
        private readonly IConfigurationRoot _configuration = MockHelpers.GetIConfigurationRoot();
        private readonly Mock<IMemoryCache> _memory = new Mock<IMemoryCache>();
        public InmateVisitationServiceTest(DbInitialize fixture)
        {
            _fixture = fixture;
            Mock<UserManager<AppUser>> userMgr = MockHelpers.MockUserManager<AppUser>();
            HttpContextAccessor httpContext = new HttpContextAccessor { HttpContext = fixture.Context.HttpContext };
            AtimsHubService atimsHubService = new AtimsHubService(_fixture.HubContext.Object);
            AppletsSavedService appletsSavedService = new AppletsSavedService(_fixture.Db, _configuration);

            PrebookActiveService prebookActiveService = new PrebookActiveService(fixture.Db, httpContext, 
                atimsHubService);
            InterfaceEngineService interfaceEngineService = new InterfaceEngineService(fixture.Db, _configuration,
                _memory.Object);
            PhotosService photosService = new PhotosService(_fixture.Db, _configuration);
            PersonService personService = new PersonService(_fixture.Db);
            CommonService commonService = new CommonService(fixture.Db, _configuration,httpContext,personService
                ,appletsSavedService,interfaceEngineService);
            PersonAddressService personAddressService = new PersonAddressService(fixture.Db, commonService,
                httpContext,personService,interfaceEngineService);
            PersonAkaService personAkaService = new PersonAkaService(fixture.Db, commonService, httpContext,personService,
                interfaceEngineService);
            PersonIdentityService personIdentityService = new PersonIdentityService(
                fixture.Db, httpContext, prebookActiveService, personAkaService,personService,interfaceEngineService);
            _inmateVisitationService = new InmateVisitationService(
                fixture.Db, commonService, personIdentityService, personAddressService, httpContext);
        }

        [Fact]
        public void InmateVisitation_GetPersonalVisitorList()
        {
            //Arrange
            SearchVisitorList searchVisitorList = new SearchVisitorList
            {
                PersonId = 55,
                InmateId = 110,
                VisitorFirstName = "SUGIR"
            };
            //Act
            VisitorListVm personalVisitorList = _inmateVisitationService
                .GetVisitorList(searchVisitorList);

            //Assert
            Assert.NotNull(personalVisitorList);
        }


        [Fact]
        public void InmateVisitation_GetAssignedInmateList()
        {
            //Act
            List<AssignedInmateList> lstAssignedInmate = _inmateVisitationService
                .GetAssignedInmateList(10, true);
            //Assert
            AssignedInmateList inmateList = lstAssignedInmate.Single(a => a.VisitorListToInmateId == 16);
            Assert.Equal(100, inmateList.PersonId);
            Assert.Equal("TR74414", inmateList.InmateNumber);
        }


        //[Fact]
        //public async Task InmateVisitation_InsertUpdatePersonDetails()
        //{
        //    //Arrange
        //    PersonalVisitorDetails personalVisitorDetails = new PersonalVisitorDetails
        //    {
        //        PersonIdentity = new PersonIdentity
        //        {
        //            PersonFirstName = "UVAN",
        //            PersonMiddleName = "SHANKAR",
        //            PersonLastName = "RAJA",
        //            InmateNumber = "CS4151",
        //            CreateBy = 11,
        //            UpdateBy = 12,
        //            CreatedDate = DateTime.Now,
        //            UpdatedDate = DateTime.Now,
        //            CreateByOfficerBadgeNumber = "BDS14011",
        //            FacilityId = 2,
        //            FknFirstName = "RAJA",
        //            FknLastName = "EL",
        //            FknSuffixName = null,
        //            InmateId = 170,
        //            PersonContactId = 5,
        //            PersonDob = DateTime.Now.AddYears(-33),
        //            PersonPlaceOfBirth = "KANCHIPURAM",
        //            PersonDlState = "TN"
        //        },
        //        PersonAddress = new PersonAddressVm
        //        {
        //            PersonCreateDate = DateTime.Now,
        //            AddressId = 11,
        //            IsRefused = false,
        //            IsHomeless = false
        //        },
        //        VisitorInfo = new SearchVisitorList
        //        {
        //            VisitorListId = 11,
        //            InmateId = 120,
        //            VisitorIdType = "VI",
        //            VisitorType = 2,
        //            VisitorNotes = "FAMILY MEMBER",
        //            VisitorIdNumber = "5"
        //        },
        //        VisitorRejectDetails = new VisitorRejectDetails
        //        {
        //            RejectAll = 1
        //        },
        //        PersonOfInterestDetails = new PersonOfInterestDetails
        //        {
        //            PersonOfInterestNote = "PERSON IS ALLOWED"
        //        },
        //        PersonAddressDetails = new PersonAddressDetails
        //        {
        //            ResAddress = new PersonAddressVm
        //            {
        //                AddressType = "RES"
        //            }
        //        }
        //    };
        //   GenerateTables.Models.Person person = _fixture.Db.Person.SingleOrDefault(p => p.PersonPlaceOfBirth == "KANCHIPURAM");
        //    Assert.Null(person);
        //    Visitor visitor = _fixture.Db.Visitor
        //        .SingleOrDefault(v => v.VisitorNotes == "FAMILY MEMBER");
        //    Assert.Null(visitor);

        //    //Act
        //    await _inmateVisitationService.InsertUpdatePersonDetails(personalVisitorDetails);

        //    //Assert
        //    person = _fixture.Db.Person.Single(p => p.PersonPlaceOfBirth == "KANCHIPURAM");
        //    Assert.NotNull(person);
        //    visitor = _fixture.Db.Visitor.Single(v => v.VisitorNotes == "FAMILY MEMBER");
        //    Assert.NotNull(visitor);
        //}

        [Fact]
        public async Task InmateVisitation_InsertInmateToVisitor()
        {
            //Arrange
            InmateToVisitorInfo inmateToVisitorInfo = new InmateToVisitorInfo
            {
                VisitorListId = 12,
                InmateId = 110,
                VisitorRelationshipId = 5,
                RejectSpecificInmate = true,
                VisitorRejectDetails = new VisitorRejectDetails
                {
                    VisitorNotAllowedExpireDate = DateTime.Now,
                    VisitorNotAllowedNote = "REJECTED FOR ID CARD",
                    VisitorNotAllowedReason = "OTHERS",
                    RejectAll = 0
                }
            };
            VisitorToInmate visitorListToInmate = _fixture.Db.VisitorToInmate
                .SingleOrDefault(v => v.VisitorRelationship == 5);

            Assert.Null(visitorListToInmate);
            VisitorRejectSaveHistory visitorRejectSaveHistory = _fixture.Db.VisitorRejectSaveHistory
                .SingleOrDefault(v => v.VisitorId == 12);

            Assert.Null(visitorRejectSaveHistory);

            //Act
            await _inmateVisitationService.InsertInmateToVisitor(inmateToVisitorInfo);

            //Assert
            visitorRejectSaveHistory = _fixture.Db.VisitorRejectSaveHistory.Single(v => v.VisitorId == 12);
            Assert.NotNull(visitorRejectSaveHistory);
            visitorListToInmate = _fixture.Db.VisitorToInmate.Single(v => v.VisitorRelationship == 5);
            Assert.NotNull(visitorListToInmate);
        }

        //[Fact]
        //public void InmateVisitation_GetPersonalVisitorDetails()
        //{
        //    //Act
        //    PersonalVisitorDetails personalVisitor = _inmateVisitationService.GetPersonalVisitorDetails(7);
        //    //Assert
        //    Assert.NotNull(personalVisitor);
        //    Assert.Equal(50, personalVisitor.PersonAddressDetails.PersonId);
        //    Assert.Equal("PC2015", personalVisitor.PersonIdentity.PersonCii);
        //}

        //[Fact]
        //public async Task InmateVisitation_DeletePersonalVisitorDetails()
        //{
        //    //Delete personal
        //    Visitor visitorList = _fixture.Db.Visitor.Single(v => v.VisitorId == 13);
        //   // Assert.Null(visitorList.InactiveFlag);

        //    //Act
        //    await _inmateVisitationService.DeletePersonalVisitorDetails(13);

        //    //Assert
        //    visitorList = _fixture.Db.Visitor.Single(v => v.VisitorId == 13);
        //   // Assert.Equal(1, visitorList.InactiveFlag);

        //    //Undo-personal
        //    visitorList = _fixture.Db.Visitor.Single(v => v.VisitorId == 12);
        //    Assert.Equal(13, visitorList.InactiveBy);

        //    //Act
        //    await _inmateVisitationService.UndoPersonalVisitorDetails(12);

        //    //Assert
        //    visitorList = _fixture.Db.Visitor.Single(v => v.VisitorId == 12);
        //    Assert.Null(visitorList.InactiveBy);
        //}

        [Fact]
        public void InmateVisitation_GetVisitorListSaveHistory()
        {
            //Act
            List<VisitorListSaveHistory> lstVisitorListHistory = _inmateVisitationService.GetVisitorListSaveHistory(06);

            //Assert
            VisitorListSaveHistory visitorListHistory = lstVisitorListHistory
                .Single(v => v.VisitorListHistoryId == 10);

            Assert.Equal("KRISHNA", visitorListHistory.PersonLastName);
            Assert.Equal("THANGA", visitorListHistory.VisitationHeader[0].Detail);
            Assert.Equal("personFirstName", visitorListHistory.VisitationHeader[0].Header);
        }

        //[Fact]
        //public void InmateVisitation_GetHistoryList()
        //{
        //    //Arrange
        //    SearchVisitorHistoryList visitorHistoryList = new SearchVisitorHistoryList
        //    {
        //        InmateId = 110,
        //        DateFrom = DateTime.Now.AddDays(-2),
        //        DateTo = DateTime.Now,
        //        VisitFlag = null
        //    };
        //    //Act
        //    HistoryList historyList = _inmateVisitationService.GetHistoryList(visitorHistoryList);

        //    //Assert
        //    HistoryInfo historyInfo = historyList.HistoryInfoList.Single(s => s.InmateId == 110);
        //    Assert.Equal("CHENNAI", historyInfo.VisitorLocation);
        //}

        //[Fact]
        //public void InmateVisitation_GetVisitorHistory()
        //{
        //    //Act
        //    List<VisitationHistory> lstVisitaion = _inmateVisitationService.GetVisitorHistory(107, 99);

        //    //Assert
        //    VisitationHistory visitationHistory = lstVisitaion.Single(v => v.VisitorIdNumber == "VID74100");
        //    Assert.Equal("RELATION", visitationHistory.VisitorReason);
        //}

        [Fact]
        public async Task InmateVisitation_DeleteAssignedInmateDetails()
        {
            VisitorToInmate visitorListToInmate = _fixture.Db.VisitorToInmate
                .Single(v => v.VisitorToInmateId == 18);
            Assert.Null(visitorListToInmate.DeleteFlag);

            //Act
            await _inmateVisitationService
                .DeleteAssignedInmateDetails(18);

            //Arrange
            visitorListToInmate = _fixture.Db.VisitorToInmate.Single(v => v.VisitorToInmateId == 18);
            Assert.Equal(1, visitorListToInmate.DeleteFlag);
        }

        [Fact]
        public async Task UndoAssignedInmateDetails()
        {
            VisitorToInmate visitorListToInmate = _fixture.Db.VisitorToInmate
                .Single(v => v.VisitorToInmateId == 19);
            Assert.Equal(1, visitorListToInmate.DeleteFlag);
            //Act
            await _inmateVisitationService.UndoAssignedInmateDetails(19);
            //Assert
            visitorListToInmate = _fixture.Db.VisitorToInmate.Single(v => v.VisitorToInmateId == 19);
            Assert.Equal(0, visitorListToInmate.DeleteFlag);
        }

        //[Fact]
        //public async Task DeleteVisitationHistory()
        //{
        //    Visit visit = _fixture.Db.Visit.Single(v => v.VisitId == 9);
        //    Assert.Null(visit.VisitDeleteFlag);

        //    //Act
        //    await _inmateVisitationService.DeleteVisitationHistory(9);

        //    //Assert
        //    visit = _fixture.Db.Visit.Single(v => v.VisitId == 9);
        //    Assert.Equal(1, visit.VisitDeleteFlag);
        //}
    }
}




