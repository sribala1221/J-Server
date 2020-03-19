using ServerAPI.Services;
using ServerAPI.Tests;
using ServerAPI.ViewModels;
using System.Collections.Generic;
using Xunit;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using ServerAPI.JMS.Tests.Helper;
using Moq;
using Microsoft.AspNetCore.Identity;
using JwtDb.Models;
using Microsoft.Extensions.Caching.Memory;

// ReSharper disable once CheckNamespace
namespace ServerAPI.JMS.Tests
{

    [Collection("Database collection")]
    public class WorkCrewServiceTest
    {
        private readonly WorkCrewService _workCrewService;
        private readonly IConfigurationRoot _configuration = MockHelpers.GetIConfigurationRoot();
        private readonly Mock<IMemoryCache> _memory = new Mock<IMemoryCache>();
        public WorkCrewServiceTest(DbInitialize fixture)
        {
            DbInitialize fixture1 = fixture;
            AppletsSavedService appletsSavedService = new AppletsSavedService(fixture.Db, _configuration);
            Mock<UserManager<AppUser>> userMgr = MockHelpers.MockUserManager<AppUser>();
            PersonService personService = new PersonService(fixture.Db);
            PhotosService photosService = new PhotosService(fixture.Db, _configuration);
            HttpContextAccessor httpContext = new HttpContextAccessor { HttpContext = fixture1.Context.HttpContext };
            InterfaceEngineService interfaceEngineService = new InterfaceEngineService(fixture1.Db,
                _configuration, _memory.Object);
            CommonService commonService = new CommonService(fixture1.Db, _configuration, httpContext, personService,appletsSavedService,interfaceEngineService);
            FacilityPrivilegeService facilityPrivilegeService = new FacilityPrivilegeService(fixture1.Db, httpContext,
                personService);
            _workCrewService = new WorkCrewService(fixture1.Db, commonService, facilityPrivilegeService, photosService);
        }

        [Fact]
        public void WorkCrew_GetWorkCrewEntriesCount()
        {
            List<int> housingUnitIds = new List<int>();
            //Act
            List<WorkCrewVm> lstWorkcrew = _workCrewService.GetWorkCrewEntriesCount(2, housingUnitIds);
            //Assert
            WorkCrewVm workCrew = lstWorkcrew.Single(w => w.WorkCrewLookupId == 6);
            Assert.Equal("OFFICE ROOM CREW", workCrew.WorkCrewName);
        }

        //[Fact]
        //public void WorkCrew_GetWorkcrewInmateDetails()
        //{
        //    //Act
        //    workCrewDetailsVm lstWorkCrewFurl = _workCrewService.GetWorkcrewInmateDetails(2);

        //    //Assert
        //    WorkCrewFurlVm workCrewFurl = lstWorkCrewFurl.LstWorkCrewFurl.Single(w => w.InmateId == 101);
        //    Assert.Equal("OFFICE ROOM CREW", workCrewFurl.WorkCrewName);
        //    Assert.Equal("http://localhost:5151/atims_dir/IDENTIFIERS\\PERSON\\FRONT_VIEW\\201509\\1_FACE.jpg", workCrewFurl.Photofilepath);
        //}
    }
}
