using Microsoft.AspNetCore.Http;
using ServerAPI.Services;
using ServerAPI.Tests;
using System.Collections.Generic;
using Xunit;
using System.Linq;
using GenerateTables.Models;
using ServerAPI.ViewModels;
using System.Threading.Tasks;
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
    public class RecordsCheckServiceTest
    {
        private readonly RecordsCheckService _recordsCheck;
        private readonly DbInitialize _fixture;
        private readonly IConfigurationRoot _configuration = MockHelpers.GetIConfigurationRoot();
        private readonly Mock<IMemoryCache> _memory = new Mock<IMemoryCache>();

        public RecordsCheckServiceTest(DbInitialize fixture)
        {
            _fixture = fixture;
            Mock<UserManager<AppUser>> userMgr = MockHelpers.MockUserManager<AppUser>();
            AppletsSavedService appletsSavedService = new AppletsSavedService(_fixture.Db, _configuration);
            PersonService personService = new PersonService(_fixture.Db);
            PhotosService photosService = new PhotosService(fixture.Db, _configuration);
            InterfaceEngineService interfaceEngineService = new InterfaceEngineService(_fixture.Db,
                _configuration, _memory.Object);

            HttpContextAccessor httpContext = new HttpContextAccessor { HttpContext = fixture.Context.HttpContext };
            CommonService commonService = new CommonService(_fixture.Db, _configuration,httpContext, personService,appletsSavedService,interfaceEngineService);
            _recordsCheck = new RecordsCheckService(_fixture.Db, commonService, httpContext);
        }

        [Fact]
        public void RecordsCheck_GetRecordHistory()
        {
            //Act
            RecordsCheckHistoryVm recordsCheckHistory = _recordsCheck.GetRecordHistroy(80);
            //Assert
            GenerateTables.Models.Facility facility = recordsCheckHistory.FacilityList.Single(f => f.FacilityId == 2);
            Assert.Equal("Tihar Jail", facility.FacilityName);
        }

        [Fact]
        public void FormRecordHist()
        {
            //Act
            List<FormRecordVm> lstFormRecord = _recordsCheck.FormRecordHist(5);
            //Assert
            FormRecordVm formRecord = lstFormRecord.FirstOrDefault(f => f.FormRecordId == 5);
            Assert.Equal("COLLECT ADDRESS DETAILS", formRecord?.FormNotes);
        }

        [Fact]
        public async Task RecordsCheck_InsertRecordsCheck()
        {
            //Insert Function
            //Arrange
            RecordsCheckVm recordsCheck = new RecordsCheckVm
            {
                PersonId = 65,
                RequestNote = "NO CASE FILE",
                RequestType = "REQUEST",
                RequestFacilityId = 1,
                Action = new[]
                { "REQUEST ACTION" }
            };
            RecordsCheckRequest recordsCheckRequest =
                _fixture.Db.RecordsCheckRequest.SingleOrDefault(r => r.PersonId == 65);
            Assert.Null(recordsCheckRequest);
            //Act
            await _recordsCheck.InsertRecordsCheck(recordsCheck);
            //Assert
            recordsCheckRequest = _fixture.Db.RecordsCheckRequest.Single(r => r.PersonId == 65);
            Assert.NotNull(recordsCheckRequest);

            //Update Function
            //Arrange
            recordsCheck = new RecordsCheckVm
            {
                RecordsCheckRequestId = 12,
                RequestFacilityId = 2,
                Note = "GIVE ANOTHER FACILITY",
                Action = new[] { "RESPONSE", "REQUEST ACTION" }
            };
            recordsCheckRequest = _fixture.Db.RecordsCheckRequest.Single(r => r.RecordsCheckRequestId == 12);
            Assert.Equal(1, recordsCheckRequest.RequestFacilityId);
            //Act
            await _recordsCheck.InsertRecordsCheck(recordsCheck);
            //Assert
            recordsCheckRequest = _fixture.Db.RecordsCheckRequest.Single(r => r.RecordsCheckRequestId == 12);
            Assert.Equal(2, recordsCheckRequest.RequestFacilityId);
        }

        [Fact]
        public async Task RecordsCheck_SendResponseRecordsCheckAsync()
        {
            //Arrange
            RecordsCheckVm recordsCheckVm = new RecordsCheckVm
            {
                RecordsStatus = RecordsCheckStatus.Clear,
                RecordsCheckRequestId = 11
            };
            RecordsCheckRequest recordsCheckRequest =
                _fixture.Db.RecordsCheckRequest.Single(r => r.RecordsCheckRequestId == 11);
            Assert.Null(recordsCheckRequest.ClearFlag);
            //Act
            await _recordsCheck.SendResponseRecordsCheck(recordsCheckVm);
            //Assert
            recordsCheckRequest = _fixture.Db.RecordsCheckRequest
                .Single(r => r.RecordsCheckRequestId == 11);

            Assert.Equal(1, recordsCheckRequest.ClearFlag);
        }

        [Fact]
        public void RecordsCheck_DeleteRecordsCheck()
        {
            //Arrange
            RecordsCheckRequest recordsCheck =
                _fixture.Db.RecordsCheckRequest.Single(r => r.RecordsCheckRequestId == 11);
            Assert.Equal(0, recordsCheck.DeleteFlag);
            //Act
            _recordsCheck.DeleteRecordsCheck(11);
            //Assert
            recordsCheck = _fixture.Db.RecordsCheckRequest.Single(r => r.RecordsCheckRequestId == 11);
            Assert.Equal(1, recordsCheck.DeleteFlag);
        }

        [Fact]
        public async Task RecordsCheck_InsertFormRecords()
        {
            //Arrange
            FormRecordVm formRecord = new FormRecordVm
            {
                FormRecordId = 8,
                XmlData =
                    @"{'person_name':'RAISA','person_dob':'30/27/1994','currentpersonnellast':'GANESH','currentpersonnelnumber':'87457215412','stampdate':'10/11/2017','stamptime':'15.15.00','person_sex':'FEMALE','address_state':'AP'}"
            };

            FormRecordSaveHistory formRecordHistory =
                _fixture.Db.FormRecordSaveHistory.SingleOrDefault(f => f.FormRecordId == 8);
            Assert.Null(formRecordHistory);
            //Act
            await _recordsCheck.InsertFormRecords(formRecord);
            //Assert
            formRecordHistory = _fixture.Db.FormRecordSaveHistory.Single(f => f.FormRecordId == 8);
            Assert.NotNull(formRecordHistory);
        }

        [Fact]
        public void GetRecordCheck()
        {
            RecordsCheckVm recordsCheck = _recordsCheck.GetRecordCheck(14);


            Assert.Equal("CHS0022", recordsCheck.InmateNumber);

            Assert.Equal("VIJAYA", recordsCheck.RequestOfficer.PersonLastName);
            Assert.Equal("1728", recordsCheck.RequestOfficer.OfficerBadgeNumber);

        }

        [Fact]
        public void InsertBypass()
        {
            //Arrange
            RecordsCheckVm recordsCheck = new RecordsCheckVm
            {
                PersonId = 90,
                Note = "SAME RECORD"
            };
            //Before Insert
            RecordsCheckRequest recordsCheckRequest = _fixture.Db.RecordsCheckRequest.SingleOrDefault(
                r => r.PersonId == 90);
            Assert.Null(recordsCheckRequest);

            //Act
            _recordsCheck.InsertBypass(recordsCheck);

            //After Insert
            //Assert
            recordsCheckRequest = _fixture.Db.RecordsCheckRequest.Single(
                r => r.PersonId == 90);
            Assert.NotNull(recordsCheckRequest);
        }

        [Fact]
        public void GetRecordsCheckResponseCount()
        {
            //Act
            IQueryable<RecordsCheckRequest> recordsCheckRequests = _recordsCheck.GetRecordsCheckResponseCount(1);
            //Assert
            RecordsCheckRequest recordsCheck = recordsCheckRequests.Single(s => s.PersonId == 50);
            Assert.Equal(10, recordsCheck.RecordsCheckRequestId);
        }

        [Fact]
        public void GetRecordsCheckResponse()
        {
            //Act
            List<RecordsCheckVm> lstRecordsCheck = _recordsCheck.GetRecordsCheckResponse(2);
            //Assert
            Assert.True(lstRecordsCheck.Count > 0);
        }
    }
}
