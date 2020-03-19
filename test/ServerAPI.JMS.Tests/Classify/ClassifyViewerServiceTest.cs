using Xunit;
using ServerAPI.Services;
using ServerAPI.ViewModels;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;
using ServerAPI.Tests;
using Moq;
using Microsoft.Extensions.Configuration;
using JwtDb.Models;
using Microsoft.AspNetCore.Identity;
using ServerAPI.JMS.Tests.Helper;
using Microsoft.Extensions.Caching.Memory;

// ReSharper disable once CheckNamespace
namespace ServerAPI.JMS.Tests
{
    [Collection("Database collection")]
    public class ClassifyViewerServiceTest
    {
        private readonly ClassifyViewerService _classifyViewer;
        private readonly IConfigurationRoot _configuration = MockHelpers.GetIConfigurationRoot();
        private readonly Mock<IMemoryCache> _memory = new Mock<IMemoryCache>();
        public ClassifyViewerServiceTest(DbInitialize fixture)
        {
            PersonService personService = new PersonService(fixture.Db);
            PhotosService photosService = new PhotosService(fixture.Db, _configuration);
            Mock<JwtDbContext> jwtMock = new Mock<JwtDbContext>();
            AppletsSavedService appletsSavedService = new AppletsSavedService(fixture.Db, _configuration);
            Mock<UserManager<AppUser>> userMgr = MockHelpers.MockUserManager<AppUser>();
            HttpContextAccessor httpContext = new HttpContextAccessor { HttpContext = fixture.Context.HttpContext };
            InterfaceEngineService interfaceEngineService = new InterfaceEngineService(fixture.Db,
                _configuration, _memory.Object);

            CommonService commonService = new CommonService(fixture.Db, _configuration,
                httpContext,personService,appletsSavedService,interfaceEngineService);
            FacilityHousingService facilityHousingService = new FacilityHousingService(fixture.Db, _configuration,
                httpContext);

            _classifyViewer = new ClassifyViewerService(fixture.Db, commonService, httpContext, personService,jwtMock.Object, 
                userMgr.Object, appletsSavedService);
        }

        //comment these line due to JwtDbContext is not implement
        //[Fact]
        //public void ClassifyViewerServiceTest_GetClassLog()
        //{
        //    //Arrange
        //    LogParameters logParameters = new LogParameters
        //    {
        //        Assoc = true,
        //        Initial = true,
        //        ReClassify = true,
        //        Form = true,
        //        Link = true,
        //        Attach = true,
        //        ClassNote = true,
        //        Flag = true,
        //        Housing = true,
        //        Incident = true,
        //        Message = true,
        //        Release = true,
        //        Review = true,
        //        Privileges = true
        //    };
        //    ClassLogInputs classLogInputs = new ClassLogInputs
        //    {
        //        FacilityId = 2,
        //        InmateId = 103,
        //        Active = true,
        //        PersonnelId = 12,
        //        HousingUnitListId = 12,
        //        Hours = 12,
        //        ClassType = "PENDING",
        //        LogParameters = logParameters
        //    };
        //    //Act
        //    ClassLogDetails classLogDetails = _classifyViewer.GetClassLog(classLogInputs);
        //    //Assert
        //    Assert.NotNull(classLogDetails);
        //    ClassLog classLog = classLogDetails.ClassLog.Single(c => c.ClassType == "RE-CLASSIFICATION");
        //    Assert.Equal("PENDING FILE", classLog.ClassNarrative);
        //    Assert.Equal(103, classLog.PersonDetails.InmateId);
        //    Assert.Equal(12, classLog.OfficerId);
        //    Assert.Equal("1729", classLog.OfficerDetails.OfficerBadgeNumber);
        //}

        //[Fact]
        //public void ClassifyViewerServiceTest_GetInmateClassify()
        //{
        //    //Act
        //    List<ClassLog> lstClassLogs = _classifyViewer.GetInmateClassify(104);
        //    //Assert
        //    ClassLog classLog = lstClassLogs.Single(c => c.ClassType == "INITIAL");
        //    Assert.Equal("MINIMUM", classLog.ClassNarrative);
        //    Assert.Equal("PALANI", classLog.OfficerDetails.PersonMiddleName);
        //}

       
    }
}
