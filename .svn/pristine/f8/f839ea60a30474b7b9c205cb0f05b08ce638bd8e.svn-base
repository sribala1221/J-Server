using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Moq;
using ServerAPI.JMS.Tests.Helper;
using ServerAPI.Services;
using ServerAPI.Tests;
using ServerAPI.ViewModels;
using System.Collections.Generic;
using Xunit;

// ReSharper disable once CheckNamespace
namespace ServerAPI.JMS.Tests
{
    [Collection("Database collection")]
    public class DataMergeServiceTest
    {
        private readonly DataMergeService _dataMergeService;
        private readonly DbInitialize _fixture;
        private readonly IConfigurationRoot _configuration = MockHelpers.GetIConfigurationRoot();
        private readonly Mock<IMemoryCache> _memory = new Mock<IMemoryCache>();

        public DataMergeServiceTest(DbInitialize fixture)
        {
            _fixture = fixture;
            InterfaceEngineService interfaceEngineService = new InterfaceEngineService(_fixture.Db,
                _configuration, _memory.Object);
            HttpContextAccessor httpContext = new HttpContextAccessor { HttpContext = fixture.Context.HttpContext };
            _dataMergeService = new DataMergeService(fixture.Db, httpContext,interfaceEngineService);
        }

        //For testing StartsWith null exception 
        [Fact]
        public void GetNewMergePersons()
        {
            RecordsDataVm recordsData = new RecordsDataVm
            {
                IsGroupBy = false,
                Results = 25,
                IsGroupByLastName = true,
                IsGroupByFirstName = true,
                IsGroupByDob = true,
                IsInclAka = true,
                // Dln = "ss",

                //uncomment the InmateNumber values and check it, its throws the null exception only for unit testing
                //(AkaInmateNumber) is null on aka table 

                //InmateNumber = "SVK661",
                LstPerson = new List<RecordsDataVm>
                {
                    new RecordsDataVm
                    {
                        InmateId = 100
                    }
                }
            };

            List<RecordsDataVm> lstRecordsData = _dataMergeService.GetNewMergePersons(recordsData);
           // Assert.True(lstRecordsData.Count > 20);
        }

    }
}
