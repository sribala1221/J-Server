using JwtDb.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Moq;
using ServerAPI.JMS.Tests.Helper;
using ServerAPI.Services;
using ServerAPI.Tests;
using Xunit;

// ReSharper disable once CheckNamespace
namespace ServerAPI.JMS.Tests
{
    [Collection("Database collection")]
    public class CommonServiceTest
    {
        private readonly CommonService _commonservice;
        private readonly IConfigurationRoot _configuration = MockHelpers.GetIConfigurationRoot();
        private readonly Mock<IMemoryCache> _memory = new Mock<IMemoryCache>();


        public CommonServiceTest(DbInitialize fixture)
        {
            HttpContextAccessor httpContext = new HttpContextAccessor { HttpContext = fixture.Context.HttpContext };
            Mock<UserManager<AppUser>> userMgr = MockHelpers.MockUserManager<AppUser>();
            AppletsSavedService appletsSavedService = new AppletsSavedService(fixture.Db, _configuration);
            PersonService personService = new PersonService(fixture.Db);
            InterfaceEngineService interfaceEngineService = new InterfaceEngineService(fixture.Db,
                  _configuration, _memory.Object);
            PhotosService photosService = new PhotosService(fixture.Db, _configuration);
            _commonservice = new CommonService(fixture.Db, _configuration, httpContext, personService, appletsSavedService, interfaceEngineService);
        }


    }

}
