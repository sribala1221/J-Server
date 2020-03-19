using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using ServerAPI.Services;
using ServerAPI.Tests;
using ServerAPI.ViewModels;
using System.Collections.Generic;
using Xunit;
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
    public class ProgramServiceTest
    {
        private readonly ProgramService _programService;
        private readonly IConfigurationRoot _configuration = MockHelpers.GetIConfigurationRoot();
        private readonly Mock<IMemoryCache> _memory = new Mock<IMemoryCache>();
        public ProgramServiceTest(DbInitialize fixture)
        {
            DbInitialize fixture1 = fixture;
            Mock<UserManager<AppUser>> userMgr = MockHelpers.MockUserManager<AppUser>();
            PersonService personService = new PersonService(fixture1.Db);
            PhotosService photosService = new PhotosService(fixture.Db, _configuration);
            AppletsSavedService appletsSavedService = new AppletsSavedService(fixture.Db, _configuration);
            InterfaceEngineService interfaceEngineService = new InterfaceEngineService(fixture1.Db,
                _configuration, _memory.Object);

            HttpContextAccessor httpContext = new HttpContextAccessor { HttpContext = fixture1.Context.HttpContext };
            CommonService commonService = new CommonService(fixture1.Db, _configuration, httpContext, personService,appletsSavedService,interfaceEngineService);
            _programService = new ProgramService(fixture1.Db, commonService, photosService);
        }


        [Fact]
        public void Program_GetLocationList()
        {
            //Act
            IEnumerable<KeyValuePair<int, string>> locationList = _programService.GetLocationList(2);
            //Assert
            Assert.True(locationList.Any());
        }

        [Fact]
        public void Program_GetProgramList()
        {
            //Act
            IEnumerable<ProgramVm> programList = _programService.GetProgramList(1);
            //Assert
            ProgramVm program = programList.Single(p => p.ProgramId == 8);
            Assert.Equal("COMPUTER-MIN", program.ClassOrServiceName);
        }

        //[Fact]
        //public void Program_GetProgramAppts()
        //{
        //    List<int> locationIds = new List<int>
        //    {
        //    7,8};
        //    //Act
        //    IEnumerable<ScheduleEventVm> scheduleEvent = _programService.GetProgramAppts(2, false, locationIds, 8);

        //    //Assert
        //    Assert.NotNull(scheduleEvent);
        //    ScheduleEventVm schedule = scheduleEvent.Single(s => s.LocationId == 6);
        //    Assert.Equal("MIN FEMALE", schedule.EventCategory);
        //}
    }
}
