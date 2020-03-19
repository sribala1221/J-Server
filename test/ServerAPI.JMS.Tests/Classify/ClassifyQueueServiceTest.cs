using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Moq;
using ServerAPI.Services;
using ServerAPI.Tests;
using ServerAPI.ViewModels;
using System;
using System.Collections.Generic;
using Xunit;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using JwtDb.Models;
using ServerAPI.JMS.Tests.Helper;
using Microsoft.Extensions.Caching.Memory;

// ReSharper disable once CheckNamespace
namespace ServerAPI.JMS.Tests
{
    [Collection("Database collection")]
    public class ClassifyQueueServiceTest
    {

        private readonly ClassifyQueueService _classifyQueueService;
        private readonly IConfigurationRoot _configuration = MockHelpers.GetIConfigurationRoot();
        private readonly DbInitialize _fixture;
        private readonly Mock<IMemoryCache> _memory = new Mock<IMemoryCache>();
        public ClassifyQueueServiceTest(DbInitialize fixture)
        {
            _fixture = fixture;
            PhotosService photosService = new PhotosService(_fixture.Db, _configuration);
            Mock<UserManager<AppUser>> userMgr = MockHelpers.MockUserManager<AppUser>();
            Mock<RoleManager<IdentityRole>> roleMgr = MockHelpers.MockRoleManager<IdentityRole>();
            AtimsHubService atimsHubService = new AtimsHubService(_fixture.HubContext.Object);
            HttpContextAccessor httpContext = new HttpContextAccessor { HttpContext = _fixture.Context.HttpContext };
            PersonService personService = new PersonService(_fixture.Db);
            AppletsSavedService appletsSavedService = new AppletsSavedService(fixture.Db, _configuration);
            InterfaceEngineService interfaceEngineService = new InterfaceEngineService(_fixture.Db,
                _configuration, _memory.Object);

            CommonService commonService = new CommonService(_fixture.Db, _configuration, httpContext, personService, appletsSavedService,interfaceEngineService);
            FacilityPrivilegeService facilityPrivilegeService = new FacilityPrivilegeService(_fixture.Db, httpContext,
                personService);
            InmateService inmateService = new InmateService(_fixture.Db, commonService, personService,
                httpContext, facilityPrivilegeService, photosService);
            FacilityHousingService facilityHousingService = new FacilityHousingService(_fixture.Db, _configuration, httpContext);

            RequestService requestService = new RequestService(_fixture.Db, httpContext, userMgr.Object, roleMgr.Object, personService, inmateService,
                facilityHousingService, atimsHubService);

            _classifyQueueService = new ClassifyQueueService(_fixture.Db, requestService, httpContext, personService,
                inmateService, facilityHousingService, commonService);
        }

        //comment these line due to RequestService module is not completed
        //[Fact]
        //public void ClassifyQueue_GetClassifyQueue()
        //{
        //    //Act
        //    ClassifyQueueVm classifyQueue = _classifyQueueService.GetClassifyQueue(2);
        //    //Assert

        //    //AssignQueue
        //    RequestTypes requestTypes = classifyQueue.AssignQueue.Single(a => a.RequestLookupId == 19);
        //    Assert.Equal("INITIAL PROGRAM", requestTypes.RequestLookupName);
        //}

        //[Fact]

        //public void ClassifyQueue_ManageSpecialQueueHistory()
        //{
        //    //Arrange
        //    SpecialQueueInputs specialQueue = new SpecialQueueInputs
        //    {
        //        FacilityId = 1,
        //        InmateId = 100,
        //        OfficerId = 12,
        //        Count = 95,
        //        FromDate = DateTime.Now.AddDays(-1),
        //        ToDate = DateTime.Now.AddDays(1)
        //    };
        //    //Act
        //    List<SpecialQueueHistory> lstSpecialQueueHistory = _classifyQueueService.ManageSpecialQueueHistory(specialQueue);

        //    //Assert
        //    SpecialQueueHistory specialQueueHistory = lstSpecialQueueHistory.Single(s => s.InmateId == 100);
        //    Assert.Equal("CHENNAI", specialQueueHistory.InmateDetails.InmateCurrentTrack);
        //    Assert.Equal("SANGEETHA", specialQueueHistory.InmateDetails.PersonFirstName);
        //    Assert.Equal(12, specialQueueHistory.OfficerId);
        //}

        //[Fact]
        //public async Task ClassifyQueue_InsertSpecialClassQueue()
        //{
        //    //Arrange           
        //    SpecialQueueInputs specialQueue = new SpecialQueueInputs
        //    {
        //        InmateId = 140,
        //        Interval = 4,
        //        FacilityId = 1
        //    };
        //    GenerateTables.Models.Inmate inmate = _fixture.Db.Inmate.Single(i => i.InmateId == 140);
        //    Assert.Null(inmate.SpecialClassQueueInterval);
        //    //Act
        //    await _classifyQueueService.InsertSpecialClassQueue(specialQueue);

        //    //Assert
        //    inmate = _fixture.Db.Inmate.Single(i => i.InmateId == 140);
        //    Assert.Equal(4, inmate.SpecialClassQueueInterval);
        //}

        //[Fact]
        //public async Task ClassifyQueue_InsertClassificationNarrative()
        //{
        //    //Arrange
        //    QueueInputs queueInputs = new QueueInputs
        //    {
        //        LstInmateIds = new[]
        //        {
        //            107, 141, 125
        //        }
        //    };
        //    GenerateTables.Models.Inmate inmate = _fixture.Db.Inmate.Single(i => i.InmateId == 141);
        //    Assert.Null(inmate.LastClassReviewDate);
        //    Assert.Null(inmate.LastClassReviewBy);

        //    //Act
        //    await _classifyQueueService.InsertClassificationNarrative(queueInputs);

        //    //Assert
        //    inmate = _fixture.Db.Inmate.Single(i => i.InmateId == 141);
        //    Assert.NotNull(inmate.LastClassReviewDate);
        //    Assert.NotNull(inmate.LastClassReviewBy);
        //}
    }
}
