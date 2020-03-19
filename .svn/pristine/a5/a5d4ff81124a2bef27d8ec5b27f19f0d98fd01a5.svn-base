using ServerAPI.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using ServerAPI.Tests;
using Xunit;
using Microsoft.AspNetCore.Http;
using ServerAPI.ViewModels;
using System.Threading.Tasks;
using GenerateTables.Models;
using Microsoft.Extensions.Configuration;
using ServerAPI.JMS.Tests.Helper;
using Moq;
using Microsoft.AspNetCore.Identity;
using JwtDb.Models;
using Microsoft.Extensions.Caching.Memory;

// ReSharper disable once CheckNamespace
namespace ServerAPI.JMS.Tests
{
    [Collection("Database collection")]
    public class AltSentServiceTest
    {
        private readonly AltSentService _altSentService;
        private readonly DbInitialize _fixture;
        private readonly IConfigurationRoot _configuration = MockHelpers.GetIConfigurationRoot();
        private readonly Mock<IMemoryCache> _memory = new Mock<IMemoryCache>();

        public AltSentServiceTest(DbInitialize fixture)
        {
            _fixture = fixture;
            PersonService personService = new PersonService(fixture.Db);
            PhotosService photosService = new PhotosService(_fixture.Db, _configuration);
            
            AppletsSavedService appletsSavedService = new AppletsSavedService(_fixture.Db, _configuration);
            Mock<UserManager<AppUser>> userMgr = MockHelpers.MockUserManager<AppUser>();
            HttpContextAccessor httpContext = new HttpContextAccessor
            { HttpContext = fixture.Context.HttpContext };
            InterfaceEngineService interfaceEngineService = new InterfaceEngineService(_fixture.Db,
                _configuration,_memory.Object);
            CommonService commonService = new CommonService(_fixture.Db, _configuration,httpContext,personService,appletsSavedService,interfaceEngineService);
            
            _altSentService = new AltSentService(_fixture.Db, commonService, httpContext, photosService, personService);
        }

        [Fact]
        public void AltSent_LoadAltSentRequestDetails()
        {
            //Act
            AltSentRequestDetails altSentRequest = _altSentService.LoadAltSentRequestDetails(2, DateTime.Now.Date);
            //Assert
            AltSentenceRequest altSentenceRequests = altSentRequest.RequestGrid
                .Single(r => r.OfficerBadgeNumber == "1475");

            Assert.Equal("NASRIN", altSentenceRequests.OfficerMiddleName);
        }

        [Fact]
        public async Task AltSent_InsertUpdateAltSentRequest()
        {
            //Arrange
            List<KeyValuePair<int, string>> lsPairs = new List<KeyValuePair<int, string>>
            {
                new KeyValuePair<int, string>(5, "FESTIVAL WORKS"),
                new KeyValuePair<int, string>(2, "GARDEN WORK")
            };
            AltSentenceRequest altSentence = new AltSentenceRequest
            {
                InmateId = 105,
                FacilityId = 2,
                RequestDate = DateTime.Now,
                RequestNote = "BASED UPON INMATE NAME",
                PersonnelId = 1,
                Programs = lsPairs,
                HistoryList = 
                @"{'FACILITY':'ALT SENT', 'INMATE':'MADDY', 'REQUEST DATE':'10/11/2017 10:26:04 AM', 'PROGRAM':'VISITOR PROGRAM'}"
            };

            AltSentRequest altSentRequest = _fixture.Db.AltSentRequest
                .SingleOrDefault(a => a.InmateId == 105);
            Assert.Null(altSentRequest);
            //Act
            await _altSentService.InsertUpdateAltSentRequest(altSentence);
            //Assert
            altSentRequest = _fixture.Db.AltSentRequest.Single(a => a.InmateId == 105);
            Assert.NotNull(altSentRequest);
        }

        [Fact]
        public async Task AltSent_DeleteAltSentActiveRequest()
        {
            //Arrange

            AltSentRequest altSentRequest = _fixture.Db.AltSentRequest.Single(a => a.AltSentRequestId == 16);
            Assert.Equal(1, altSentRequest.DeleteFlag);
            //Act
            await _altSentService.DeleteAltSentActiveRequest(16, @"{ 'Business fax':'748457457','Email':'RAJ','First name':'ANU','Middle':'ABI','Suffix':'NR'}  ",false);
            //Assert
            altSentRequest = _fixture.Db.AltSentRequest
                .Single(a => a.AltSentRequestId == 16);
            Assert.Null(altSentRequest.DeleteFlag);
        }

        [Fact]
        public void AltSent_GetAltSentHistories()
        {
            //Act
            List<HistoryVm> lstHistoryVm = _altSentService.GetAltSentHistories(15);
            //Assert
            HistoryVm historyVm = lstHistoryVm.Single(h => h.HistoryId == 11);
            Assert.Equal(60, historyVm.PersonId);
        }

        [Fact]
        public async Task AltSent_ApproveRequest()
        {
            //Arrange
            List<string> historyList = new List<string>
            {
                "{'FACILITY':'NCJ', 'INMATE':'SANGEETHA','REQUEST DATE':'10/10/2017 10:59:36 AM', 'PERSONNEL':'VARUN' }",
                "{'FACILITY':'MCJ', 'INMATE':'VINOTH','REQUEST DATE':'07/15/2017 10:59:36 AM', 'PERSONNEL':'NARESH' }"
            };

            ApproveRequest approveRequest = new ApproveRequest
            {
                HistoryList = historyList,
                Approveflag = true,
                RejectFlag = false,
                ApprovalNote = "APPROVE MY TWO REQUEST"
            };
            AltSentRequestApprovalHistory altSentRequest = _fixture.Db.AltSentRequestApprovalHistory
                .SingleOrDefault(a => a.AltSentRequestId == 16);

            Assert.Null(altSentRequest);
            //Act
            await _altSentService.ApproveAltSentRequest(16, approveRequest);
            //Assert
            altSentRequest = _fixture.Db.AltSentRequestApprovalHistory
                .Single(a => a.AltSentRequestId == 16);

            Assert.NotNull(altSentRequest);
        }

        [Fact]
        public void AltSent_GetScheduleDetails()
        {
            //Arrange
            ScheduleDetails schedule = new ScheduleDetails
            {
                ScheduleDateTime = DateTime.Now,
                FacilityId = 1,
                RequestId = 18,
                IsPreviousNextDate = true,
                InmateId = 110
            };
            //Act
            ScheduleDetails details = _altSentService.GetScheduleDetails(schedule);
            //Assert
            Assert.Equal("DEVA", details.PersonLastName);
            Assert.Equal("TR74414", details.Number);
        }

        [Fact]
        public void AltSent_GetApproveRequestInfo()
        {
            //Act
            ApproveRequestDetails approveRequestDetails = _altSentService.GetApproveRequestInfo(19);
            //Assert
            Assert.Equal("Tihar Jail", approveRequestDetails.RequestInfo.FacilityName);
        }

        [Fact]
        public void AltSent_GetAltSentRequestInfo()
        {
            //Act
            AltSentenceRequest sentRequest = _altSentService.GetAltSentRequestInfo(2, 10);
            Assert.NotNull(sentRequest);
        }

        [Fact]
        public async Task AltSent_SaveOrUpdateSchedule()
        {
            //Arrange
            SaveSchedule saveSchedule = new SaveSchedule
            {
                ScheduleDateTime = DateTime.Now,
                ScheduleNote = "SCHEDULED TWO REQUEST"
            };
            AltSentRequest altSentRequest = _fixture.Db.AltSentRequest
                .Single(a => a.AltSentRequestId == 15);

            Assert.Null(altSentRequest.SchedReqBookNotes);
            //Act
            await _altSentService.ScheduleAltSentRequest(15,saveSchedule);
            //Assert
            altSentRequest = _fixture.Db.AltSentRequest.Single(a => a.AltSentRequestId == 15);
            Assert.Equal("SCHEDULED TWO REQUEST", altSentRequest.SchedReqBookNotes);
        }
    }
}
