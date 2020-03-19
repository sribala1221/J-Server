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
using System.Linq;
using ServerAPI.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace ServerAPI.JMS.Tests
{
    [Collection("Database collection")]
    public class IntakeCurrencyServiceTest
    {

        private readonly IntakeCurrencyService _intakeCurrencyService;
        private readonly DbInitialize _fixture;
        private readonly IConfigurationRoot _configuration = MockHelpers.GetIConfigurationRoot();
        private readonly Mock<IMemoryCache> _memory = new Mock<IMemoryCache>();

        public IntakeCurrencyServiceTest(DbInitialize fixture)
        {
            _fixture = fixture;
            AtimsHubService atimsHubService = new AtimsHubService(_fixture.HubContext.Object);
            AppletsSavedService appletsSavedService = new AppletsSavedService(_fixture.Db, _configuration);
            HttpContextAccessor httpContext = new HttpContextAccessor
            { HttpContext = _fixture.Context.HttpContext };
            PersonService personService = new PersonService(_fixture.Db);
            InterfaceEngineService interfaceEngineService = new InterfaceEngineService(fixture.Db,
                         _configuration, _memory.Object);
            CommonService commonService = new CommonService(_fixture.Db, _configuration, httpContext, personService,
                appletsSavedService, interfaceEngineService);
            _intakeCurrencyService = new IntakeCurrencyService(_fixture.Db, commonService, httpContext,
                  personService, interfaceEngineService);
        }

        [Fact]
        public void GetIntakeCurrencyViewer()
        {
            //Act
            IntakeCurrencyViewerVm intakeCurrencyViewer = _intakeCurrencyService.GetIntakeCurrencyViewer(15);

            //Assert
            Assert.True(intakeCurrencyViewer.CurrencyModifyReason.Count > 0);
            Assert.True(intakeCurrencyViewer.IntakeCurrency.Count > 0);
        }

        [Fact]
        public async Task InsertIntakeCurrency()
        {
            //Arrange
            IntakeCurrencyVm intakeCurrency = new IntakeCurrencyVm
            {
                IncarcerationId = 16,
                OtherAmount = 500,
                ModifyReason = "OTHER MONEY",
                Currency10000Count = 100,
                Currency100Count = 10,
                Currency1000Count = 15,
                Currency25Count = 20,
                Currency01Count = 25,
                Currency5000Count = 3
            };
            //Before Insert
            IncarcerationIntakeCurrency incarcerationIntakeCurrency =
                _fixture.Db.IncarcerationIntakeCurrency.SingleOrDefault(i => i.IncarcerationId == 16);

            Assert.Null(incarcerationIntakeCurrency);

            //Act
            await _intakeCurrencyService.InsertIntakeCurrency(intakeCurrency);

            //Assert
            //After Insert
            incarcerationIntakeCurrency =
                _fixture.Db.IncarcerationIntakeCurrency.SingleOrDefault(i => i.IncarcerationId == 16);

            Assert.NotNull(incarcerationIntakeCurrency);
        }

        [Fact]
        public void GetIntakeCurrencyPdfViewer()
        {
            //Act
            IntakeCurrencyPdfViewerVm intakeCurrencyPdf = _intakeCurrencyService.GetIntakeCurrencyPdfViewer(18);

            //Assert
            Assert.True(intakeCurrencyPdf.IntakeCurrencyList.Count > 0);
            Assert.Equal("PUBLIC DEFENDER", intakeCurrencyPdf.SentencePdfDetails.AgencyName);
            Assert.Equal("NEW CURRENCY",intakeCurrencyPdf.IntakeCurrency.ModifyReason);
        }


    }
}
