using ServerAPI.Services;
using ServerAPI.Tests;
using System;
using System.Collections.Generic;
using Xunit;
using System.Linq;
using ServerAPI.ViewModels;
using GenerateTables.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using ServerAPI.JMS.Tests.Helper;
using Moq;
using Microsoft.Extensions.Caching.Memory;

// ReSharper disable once CheckNamespace
namespace ServerAPI.JMS.Tests
{

    [Collection("Database collection")]
    public class PersonTestingServiceTest
    {
        private readonly PersonTestingService _personTestingService;
        private readonly DbInitialize _fixture;
        private readonly IConfigurationRoot _configuration = MockHelpers.GetIConfigurationRoot();
        private readonly Mock<IMemoryCache> _memory = new Mock<IMemoryCache>();

        public PersonTestingServiceTest(DbInitialize fixture)
        {
            _fixture = fixture;
         
            PersonService personService = new PersonService(fixture.Db);
            HttpContextAccessor httpContext = new HttpContextAccessor {HttpContext = fixture.Context.HttpContext};
            InterfaceEngineService interfaceEngineService = new InterfaceEngineService(fixture.Db, _configuration,
                _memory.Object);
            _personTestingService = new PersonTestingService(fixture.Db,personService,httpContext,interfaceEngineService);
        }

        [Fact]
        public void PersonTesting_GetTestingDetails()
        {
            //Act
            List<TestingVm> lsttesting = _personTestingService.GetTestingDetails(55);
            //Assert
            TestingVm testingVm = lsttesting.Single(t => t.OfficerBadgeNumber == "1729");
            Assert.Equal("SUGIR", testingVm.CreateByPersonFirstName);
            Assert.Equal("BLOOD PRESURE", testingVm.TypeText);
        }

        [Fact]
        public void PersonTesting_GetTestingHistoryDetails()
        {
            //Act
            List<HistoryVm> lsthistory = _personTestingService.GetTestingHistoryDetails(6);
            //Assert
            HistoryVm historyVm = lsthistory.Single(x => x.PersonId == 50);
            Assert.Equal("1728", historyVm.OfficerBadgeNumber);
            Assert.Equal("Performed By", historyVm.Header[3].Header);
            Assert.Equal("SAKTHI VEL", historyVm.Header[3].Detail);
        }

        [Fact]
        public void PersonTesting_InsertUpdateTestingDetails()
        {
            //Insert Method
            //Arrange
            TestingVm testingVm = new TestingVm
            {
                CreateDate = DateTime.Now.AddDays(-2),
                UpdateDate = DateTime.Now,
                PersonId = 70,
                PersonnelId = 12,
                GatheredDate = DateTime.Now.AddDays(-1),
                DateProcessed = DateTime.Now.AddDays(-1),
                RequestDate = DateTime.Now.AddDays(-2),
                UpdatePersonnelId = 11,
                Notes = "COMPLETED ALL TESTS",
                ProcessedDisposition = null,
                Type = 3,
                Requested = 11,
                PerformedBy = "RAJIV GANDHI"
            };
            //Before Insert
            PersonTesting personTesting = _fixture.Db.PersonTesting.SingleOrDefault(x => x.PersonId == 70);
            Assert.Null(personTesting);
            //Act
            _personTestingService.InsertUpdateTestingDetails(testingVm);

            //Assert
            //After Insert
            personTesting = _fixture.Db.PersonTesting.Single(p => p.PersonId == 70);
            Assert.NotNull(personTesting);

            //Update Method
            //Arrange
            testingVm = new TestingVm
            {
                PerformedBy = "MUGESH",
                PersonId = 75,
                PersonnelId = 12,
                TestingId = 7,
                GatheredDate = DateTime.Now.AddDays(-1),
                DateProcessed = DateTime.Now.AddDays(-1),
                RequestDate = DateTime.Now.AddDays(-2),
                UpdatePersonnelId = 11,
                Notes = "GATHERED ECC,X-RAY,BLOOD TEST",
                ProcessedDisposition = null,
                Type = 2,
                Requested = 11
            };
            //Before Update
            personTesting = _fixture.Db.PersonTesting.Single(p => p.PersonId == 75);
            Assert.Equal("SAMPLE REPORT", personTesting.TestingNotes);
            //Act
            _personTestingService.InsertUpdateTestingDetails(testingVm);

            //Assert
            //After Update
            personTesting = _fixture.Db.PersonTesting.Single(p => p.PersonId == 75);
            Assert.Equal("GATHERED ECC,X-RAY,BLOOD TEST", personTesting.TestingNotes);
        }
    }
}
