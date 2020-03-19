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
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

// ReSharper disable once CheckNamespace
namespace ServerAPI.JMS.Tests
{
    [Collection("Database collection")]
    public class SentenceAdjustmentServiceTest
    {
        private readonly SentenceAdjustmentService _sentenceAdjustment;
        private readonly IConfigurationRoot _configuration = MockHelpers.GetIConfigurationRoot();
        private readonly DbInitialize _fixture;
        private readonly Mock<IMemoryCache> _memory = new Mock<IMemoryCache>();

        public SentenceAdjustmentServiceTest(DbInitialize fixture)
        {
            _fixture = fixture;
            PersonService personService = new PersonService(fixture.Db);
            AppletsSavedService appletsSavedService = new AppletsSavedService(_fixture.Db, _configuration);
            Mock<UserManager<AppUser>> userMgr = MockHelpers.MockUserManager<AppUser>();
            PhotosService photosService = new PhotosService(fixture.Db, _configuration);
            InterfaceEngineService interfaceEngineService = new InterfaceEngineService(_fixture.Db,
                _configuration, _memory.Object);

            HttpContextAccessor httpContext = new HttpContextAccessor { HttpContext = fixture.Context.HttpContext };
            CommonService commonService = new CommonService(_fixture.Db, _configuration,
                httpContext, personService,appletsSavedService,interfaceEngineService);
            FacilityPrivilegeService facilityPrivilegeService = new FacilityPrivilegeService(fixture.Db, httpContext, personService);
            InmateService inmateService = new InmateService(fixture.Db, commonService, personService, httpContext, facilityPrivilegeService, photosService);
            _sentenceAdjustment = new SentenceAdjustmentService(_fixture.Db, httpContext, commonService, inmateService);
        }


        [Fact]
        public void GetAttendeeList()
        {
            //Act
            List<AttendanceVm> lstAttendance = _sentenceAdjustment.GetAttendeeList(DateTime.Now.Date.AddDays(-1), 0);

            //Assert
            Assert.True(lstAttendance.Count > 0);
        }

        [Fact]
        public void GetSentenceAdjDetail()
        {
            //Act
            AttendanceParam attendanceParam = _sentenceAdjustment.GetSentenceAdjDetail(110);

            //Assert
            Assert.True(attendanceParam.LstAttendanceSentenceVm.Count > 0);
            Assert.True(attendanceParam.LstAttendanceVms.Count > 1);
        }

        [Fact]
        public async Task UpdateNoDayForDay()
        {
            //Arrange
            int[] attendanceIds = new[]
            {
               20
            };

            //Before Update
            ArrestSentenceAttendance arrestSentenceAttendance = _fixture.Db.ArrestSentenceAttendance.Single(
                a => a.ArrestSentenceAttendanceId == 20);
            Assert.Null(arrestSentenceAttendance.NoDayDayRemoveBy);

            //Act
            await _sentenceAdjustment.UpdateNoDayForDay(attendanceIds);

            //Assert
            //After Update
            arrestSentenceAttendance = _fixture.Db.ArrestSentenceAttendance.Single(
                a => a.ArrestSentenceAttendanceId == 20);
            Assert.NotNull(arrestSentenceAttendance.NoDayDayRemoveBy);
        }

        [Fact]
        public async Task UpdateArrestAttendance()
        {
            //Arrange
            AttendanceParam attendanceParam = new AttendanceParam
            {
                AttendanceIds = new[]
                {
                    22,25
                },
                AttendanceVm = new AttendanceVm
                {
                    ArrestId = 5
                },
                LstAttendanceVms = new List<AttendanceVm>
                {
                   new AttendanceVm
                   {
                       ArrestSentenceAttendanceId = 22,
                       ArrestId = 7
                   }
                }
            };

            //Before Update
            ArrestSentenceAttendanceArrestXref attendanceArrestXref =
                _fixture.Db.ArrestSentenceAttendanceArrestXref.SingleOrDefault(
                    a => a.ArrestSentenceAttendanceId == 22);
            Assert.Null(attendanceArrestXref);

            //Act
            await _sentenceAdjustment.UpdateArrestAttendance(attendanceParam);

            //Assert
            //After Update
            attendanceArrestXref =
                _fixture.Db.ArrestSentenceAttendanceArrestXref.Single(
                    a => a.ArrestSentenceAttendanceId == 22);
            Assert.NotNull(attendanceArrestXref);
        }

        [Fact]
        public void GetDiscDaysDetails()
        {
            //Act
            List<DisciplinaryDays> discDays = _sentenceAdjustment.GetDiscDaysDetails(true, false);

            //Assert
            Assert.True(discDays.Count > 0);
        }

        [Fact]
        public void UpdateDiscInmateRemoveFlag()
        {
            //Act
            int result = _sentenceAdjustment.UpdateDiscInmateRemoveFlag(125, true);

            //Assert
            Assert.Equal(1,result);
        }

        [Fact]
        public void GetIncarcerationForSentence()
        {
            //Act
            int result = _sentenceAdjustment.GetIncarcerationForSentence(102, 8);

            //Assert
            Assert.Equal(13,result);
        }
    }
}
