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
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

// ReSharper disable once CheckNamespace
namespace ServerAPI.JMS.Tests
{
    [Collection("Database collection")]
    public class InmatePhoneServiceTest
    {
        private readonly InmatePhoneService _inmatePhone;
        private readonly DbInitialize _fixture;
        private readonly IConfigurationRoot _configuration = MockHelpers.GetIConfigurationRoot();
        private readonly Mock<IMemoryCache> _memory = new Mock<IMemoryCache>();
        public InmatePhoneServiceTest(DbInitialize fixture)
        {
            _fixture = fixture;
            PersonService personService = new PersonService(fixture.Db);
            AppletsSavedService appletsSavedService = new AppletsSavedService(fixture.Db, _configuration);
            Mock<UserManager<AppUser>> userMgr = MockHelpers.MockUserManager<AppUser>();
            PhotosService photosService = new PhotosService(fixture.Db, _configuration);
            HttpContextAccessor httpContext = new HttpContextAccessor { HttpContext = fixture.Context.HttpContext };
            InterfaceEngineService interfaceEngineService = new InterfaceEngineService(_fixture.Db,
                _configuration, _memory.Object);
            CommonService commonService = new CommonService(_fixture.Db,_configuration,httpContext,personService,appletsSavedService,interfaceEngineService);
            _inmatePhone = new InmatePhoneService(fixture.Db, commonService, httpContext);
        }

        [Fact]
        public void InmatePhoneService_GetCallLogHistroy()
        {
            //Act
            InmatePhoneHistoryVm inmatePhoneHistory = _inmatePhone.GetCallLogHistroy(100);
            //Assert
            PhoneDetailsVm phoneDetails = inmatePhoneHistory.LstCallLogDetails
                .Single(c => c.CallLogType == "MISC");

            Assert.Equal("VIJAYA", phoneDetails.CreateByPersonName);
            Assert.Equal("1728", phoneDetails.UpdateByOfficerBadgeNumber);
        }

        [Fact]
        public void InmatePhoneService_GetPinHistory()
        {
            //Act
            InmatePhoneHistoryVm inmatePhoneHistory = _inmatePhone.GetPinHistroy(102);
            //Assert
            Assert.Equal("1432", inmatePhoneHistory.CurrentPinId);
            Assert.Equal(60, inmatePhoneHistory.PersonSeal);
            PhoneDetailsVm phonedetail =
                inmatePhoneHistory.LstPinHistory.Single(p => p.CreateByOfficerBadgeNumber == "1729");

            Assert.Equal("KRISHNA", phonedetail.CreateByPersonName);
            Assert.Equal("7486", phonedetail.Pin);
        }

        [Fact]
        public async Task InmatePhoneService_InsertUpdateCallLog()
        {
            // 1.Insert 
            //Arrange
            PhoneDetailsVm phoneDetails = new PhoneDetailsVm
            {
                PhoneLogStatus = PhoneLogStatus.Insert,
                PhoneNumber = "9685741245",
                CallLogType = "STAR",
                Duration = "40",
                ContactName = "ASWIN",
                InmateId = 102,
                CallLogDate = DateTime.Now.AddDays(5)
            };
            PhoneCallLog phoneCallLog = _fixture.Db.PhoneCallLog
                .SingleOrDefault(p => p.InmateId == 102);
            Assert.Null(phoneCallLog);
            //Act
            await _inmatePhone.InsertUpdateCallLog(phoneDetails);
            //Assert
            phoneCallLog = _fixture.Db.PhoneCallLog.Single(p => p.InmateId == 102);
            Assert.Equal("STAR", phoneCallLog.CallLogType);

            // 2.Update
            //Arrange
            phoneDetails = new PhoneDetailsVm
            {
                PhoneLogStatus = PhoneLogStatus.Update,
                PhoneNumber = "7485968745",
                Duration = "30",
                ContactName = "ANIRUDH",
                PhoneCallLogId = 11
            };
            phoneCallLog = _fixture.Db.PhoneCallLog.Single(p => p.InmateId == 101);
            Assert.Equal("HARIHARAN", phoneCallLog.ContactName);
            //Act
            await _inmatePhone.InsertUpdateCallLog(phoneDetails);
            //Assert
            phoneCallLog = _fixture.Db.PhoneCallLog.Single(p => p.InmateId == 101);
            Assert.Equal("ANIRUDH", phoneCallLog.ContactName);

            // 3.Delete
            //Arrange
            phoneDetails = new PhoneDetailsVm
            {
                PhoneLogStatus = PhoneLogStatus.Delete,
                PhoneCallLogId = 10
            };
            phoneCallLog = _fixture.Db.PhoneCallLog.Single(p => p.InmateId == 100);
            Assert.Equal(0, phoneCallLog.DeleteFlag);
            //Act
            await _inmatePhone.InsertUpdateCallLog(phoneDetails);
            //Assert
            phoneCallLog = _fixture.Db.PhoneCallLog.Single(p => p.InmateId == 100);
            Assert.Equal(1, phoneCallLog.DeleteFlag);

            // 4.Undo 
            //Arrange
            phoneDetails = new PhoneDetailsVm
            {
                PhoneLogStatus = PhoneLogStatus.Undo,
                PhoneCallLogId = 12
            };
            phoneCallLog = _fixture.Db.PhoneCallLog.Single(p => p.InmateId == 104);
            Assert.Equal(1, phoneCallLog.DeleteFlag);
            //Act
            await _inmatePhone.InsertUpdateCallLog(phoneDetails);
            //Assert
            phoneCallLog = _fixture.Db.PhoneCallLog.Single(p => p.InmateId == 104);
            Assert.Equal(0, phoneCallLog.DeleteFlag);
        }

        [Fact]
        public async Task InmatePhoneService_InsertDeletePhonePin()
        {
            //Arrange
            //Deleted flag is 0
            PhoneDetailsVm phoneDetails = new PhoneDetailsVm
            {
                InmateId = 100,
                Pin = "1222",
                DeleteFlag = 0
            };
          GenerateTables.Models.Inmate inmate = _fixture.Db.Inmate.Single(i => i.InmateId == 100);

            Assert.Equal("4512", inmate.PhonePin);
            //Act
            await _inmatePhone.InsertDeletePhonePin(phoneDetails);
            //Assert
            inmate = _fixture.Db.Inmate.Single(i => i.InmateId == 100);
            Assert.Equal("1222", inmate.PhonePin);

            //Arrange
            //Deleted flag is 0
            phoneDetails = new PhoneDetailsVm
            {
                InmateId = 105,
                Pin = "4578",
                DeleteFlag = 1
            };
            PhonePinHistory phonePinHistory = _fixture.Db.PhonePinHistory
                .SingleOrDefault(i => i.InmateId == 105);

            Assert.Null(phonePinHistory);
            //Act
            await _inmatePhone.InsertDeletePhonePin(phoneDetails);
            //Assert
            phonePinHistory = _fixture.Db.PhonePinHistory.Single(i => i.InmateId == 105);
            Assert.Equal("DELETED", phonePinHistory.Note);
        }
    }
}
