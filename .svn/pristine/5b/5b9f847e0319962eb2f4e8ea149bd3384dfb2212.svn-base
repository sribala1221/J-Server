using Microsoft.AspNetCore.Http;
using ServerAPI.Services;
using ServerAPI.Tests;
using ServerAPI.ViewModels;
using Xunit;
using Microsoft.Extensions.Configuration;
using System.Linq;
using ServerAPI.JMS.Tests.Helper;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using Microsoft.AspNetCore.Identity;
using JwtDb.Models;

// ReSharper disable once CheckNamespace
namespace ServerAPI.JMS.Tests
{
    [Collection("Database collection")]
    public class InmateSummaryPdfServiceTest
    {
        private readonly InmateSummaryPdfService _inmateSummaryPdfService;
        private readonly IConfigurationRoot _configuration = MockHelpers.GetIConfigurationRoot();
        private readonly Mock<IMemoryCache> _memory = new Mock<IMemoryCache>();

        public InmateSummaryPdfServiceTest(DbInitialize fixture)
        {
            DbInitialize fixture1 = fixture;
            PhotosService photosService = new PhotosService(fixture1.Db, _configuration);
            AppletsSavedService appletsSavedService = new AppletsSavedService(fixture.Db, _configuration);
            Mock<UserManager<AppUser>> userMgr = MockHelpers.MockUserManager<AppUser>();
            HttpContextAccessor httpContext = new HttpContextAccessor { HttpContext = fixture1.Context.HttpContext };
            PersonService personService = new PersonService(fixture1.Db);
            InterfaceEngineService interfaceEngineService = new InterfaceEngineService(fixture1.Db, _configuration,
                _memory.Object);
            CommonService commonService = new CommonService(fixture1.Db, _configuration,
                httpContext, personService,appletsSavedService,interfaceEngineService);

            PersonCharService personCharService = new PersonCharService(fixture1.Db, commonService,
                httpContext, personService,interfaceEngineService);
            PersonDnaService persnoDnaService = new PersonDnaService(fixture1.Db, httpContext, personService);
            FormsService formsService = new FormsService(fixture1.Db, httpContext, commonService, personService,
                interfaceEngineService);
            _inmateSummaryPdfService = new InmateSummaryPdfService(
                fixture1.Db, commonService, formsService, httpContext, personCharService,
                photosService, persnoDnaService);
        }

        [Fact]
        public void InmateSummaryPdf_GetInmateSummaryPdf()
        {
            //Act
            InmateSummaryPdfVm inmateSummaryPdf =
                _inmateSummaryPdfService.GetInmateSummaryPdf(100, InmateSummaryType.ACTIVEBAILSUMMARY);
            //Assert
            Assert.NotNull(inmateSummaryPdf);
            Assert.Equal("VIJAYA", inmateSummaryPdf.InmatePdfHeaderDetails.OfficerName);
            Assert.Equal("ON", inmateSummaryPdf.SiteOption);
            ObservationScheduleVm observationSchedule =
                inmateSummaryPdf.ObservationDetails.Single(o => o.ObservationScheduleId == 5);
            Assert.Equal("SUICIDE ATTEMPT", observationSchedule.Note);
        }
    }
}
