using Microsoft.Extensions.Configuration;
using ServerAPI.Services;
using ServerAPI.Tests;
using ServerAPI.ViewModels;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Microsoft.AspNetCore.Http;
using ServerAPI.JMS.Tests.Helper;
using Microsoft.AspNetCore.Identity;
using Moq;
using JwtDb.Models;
using Microsoft.Extensions.Caching.Memory;

// ReSharper disable once CheckNamespace
namespace ServerAPI.JMS.Tests
{
    [Collection("Database collection")]
    public class TrackingConflictServiceTest
    {

        private readonly TrackingConflictService _trackingconflict;
        private readonly IConfigurationRoot _configuration = MockHelpers.GetIConfigurationRoot();
        private readonly Mock<IMemoryCache> _memory = new Mock<IMemoryCache>();

        public TrackingConflictServiceTest(DbInitialize fixture)
        {
            PersonService personService = new PersonService(fixture.Db);
            AppletsSavedService appletsSavedService = new AppletsSavedService(fixture.Db, _configuration);
            Mock<UserManager<AppUser>> userMgr = MockHelpers.MockUserManager<AppUser>();
            PhotosService photosService = new PhotosService(fixture.Db, _configuration);
            HttpContextAccessor httpContext = new HttpContextAccessor { HttpContext = fixture.Context.HttpContext };
            InterfaceEngineService interfaceEngineService = new InterfaceEngineService(fixture.Db,
                _configuration, _memory.Object);
            CommonService commonService = new CommonService(fixture.Db, _configuration, httpContext, personService,appletsSavedService,interfaceEngineService);
            _trackingconflict = new TrackingConflictService(fixture.Db, commonService);
        }

        //[Fact]
        //public void TrackingConflictService_GetTrackingConflict()
        //{
        //    //Arrange
        //    TrackingVm trackvm = new TrackingVm
        //    {
        //        MoveLocationId = 5,
        //        MoveInmateList = new List<int> { 100, 101, 102 }
        //    };
        //    //Act
        //    List<TrackingConflictVm> lsttrackconflictvm = _trackingconflict.GetTrackingConflict(trackvm);
        //    //Assert
        //    //Visitor conflict
        //    //TrackingConflictVm trackconflictvm =
        //    //    lsttrackconflictvm.Single(t => t.ConflictType == "VISITOR REGISTERED CONFLICT");
        //    //Assert.Equal("TRICHY", trackconflictvm.Location);
        //    //Assert.Equal("BROTHER", trackconflictvm.Relationship);
        //    //Privilege conflict
        //    TrackingConflictVm trackconflictvm = lsttrackconflictvm.FirstOrDefault(t => t.ConflictType == "PRIVILEGE");
        //    if (trackconflictvm != null)
        //    {
        //        Assert.Equal("SVK661", trackconflictvm.InmateNumber);
        //    }
        //    //Location conflict
        //    trackconflictvm = lsttrackconflictvm.Single(t => t.ConflictType == "LOCATION OUT OF SERVICE");
        //    Assert.Equal("CHENNAI", trackconflictvm.Location);
        //    Assert.Equal("ELECTRICAL WORK IS GOING ON", trackconflictvm.ConflictDescription);
        //}
    }
}
