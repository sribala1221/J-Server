using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Moq;
using ServerAPI.Services;
using ServerAPI.Tests;
using ServerAPI.ViewModels;
using Xunit;
using System.Linq;
using ServerAPI.Utilities;
using GenerateTables.Models;
using System.Threading.Tasks;
using JwtDb.Models;
using Microsoft.AspNetCore.Identity;
using ServerAPI.JMS.Tests.Helper;
using Microsoft.Extensions.Caching.Memory;

// ReSharper disable once CheckNamespace
namespace ServerAPI.JMS.Tests
{
    [Collection("Database collection")]
    public class ClassifyClassFileServiceTest
    {
        private readonly ClassifyClassFileService _classifyClassFileService;
        private readonly DbInitialize _fixture;
        private readonly IConfigurationRoot _configuration = MockHelpers.GetIConfigurationRoot();
        private readonly Mock<IMemoryCache> _memory = new Mock<IMemoryCache>();
        public ClassifyClassFileServiceTest(DbInitialize fixture)
        {
            _fixture = fixture;
            Mock<JwtDbContext> jwtMock = new Mock<JwtDbContext>();
            Mock<UserManager<AppUser>> userMgr = MockHelpers.MockUserManager<AppUser>();
            HttpContextAccessor httpContext = new HttpContextAccessor { HttpContext = fixture.Context.HttpContext };
            PersonService personService = new PersonService(fixture.Db);
            PhotosService photosService = new PhotosService(fixture.Db, _configuration);
            AppletsSavedService appletsSavedService = new AppletsSavedService(_fixture.Db, _configuration);
            InterfaceEngineService interfaceEngineService = new InterfaceEngineService(fixture.Db, _configuration,
                _memory.Object);
            CommonService commonService = new CommonService(_fixture.Db, _configuration, httpContext,personService,appletsSavedService,interfaceEngineService);
            FacilityHousingService facilityHousingService = new FacilityHousingService(_fixture.Db, _configuration, httpContext);

            ClassifyViewerService classifyViewerService = new ClassifyViewerService(_fixture.Db,
                commonService, httpContext,personService,jwtMock.Object,userMgr.Object, appletsSavedService);

            _classifyClassFileService = new ClassifyClassFileService(_fixture.Db, httpContext, classifyViewerService,
                personService,appletsSavedService);
        }


        //comment these line due to JwtDbContext is not implement

        //[Fact]
        //public void ClassifyClassFile_GetClassFile()
        //{
        //    //Arrange
        //    ClassFileInputs classFileInputs = new ClassFileInputs
        //    {
        //        InmateId = 102,
        //        DeleteFlag = true,
        //        LogParameters = new LogParameters
        //        {
        //            Initial = true,
        //            Attach = true,
        //            Form = true,
        //            Housing = true,
        //            Intake = true,
        //            Assoc = true,
        //            KeepSep = true,
        //            Link = true,
        //            Message = true,
        //            Privileges = true,
        //            Release = true
        //        },
        //    };

        //    //Act
        //    ClassFileOutputs classFileOutputs = _classifyClassFileService.GetClassFile(classFileInputs);

        //    //Assert
        //    Assert.NotNull(classFileOutputs.GridValues);
        //    SiteOptionProp siteOption = classFileOutputs.SiteOption
        //        .Single(f => f.SiteOptionVariable == "ALLOW_CLASSIFY_EDIT");
        //    Assert.Equal("ON", siteOption.SiteOptionValue);
        //}

        //[Fact]
        //public async Task ClassifyClassFile_InmateDeleteUndo()
        //{
        //    //Arrange
        //    DeleteParams deleteParams = new DeleteParams
        //    {
        //        Id = 19,
        //        DeleteFlag = 1,
        //        InmateId = 141,
        //        Type = ClassTypeConstants.ATTACH
        //    };
        //    AppletsSaved appletsSaved = _fixture.Db.AppletsSaved.Single(a => a.AppletsSavedId == 19);
        //    Assert.Equal(0, appletsSaved.AppletsDeleteFlag);

        //    //Act
        //    await _classifyClassFileService.InmateDeleteUndo(deleteParams);

        //    //Assert
        //    appletsSaved = _fixture.Db.AppletsSaved.Single(a => a.AppletsSavedId == 19);
        //    Assert.Equal(1, appletsSaved.AppletsDeleteFlag);
        //}

    }
}
