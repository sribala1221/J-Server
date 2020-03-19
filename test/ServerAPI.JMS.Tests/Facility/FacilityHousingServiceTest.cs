using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using ServerAPI.JMS.Tests.Helper;
using ServerAPI.Services;
using ServerAPI.Tests;
using ServerAPI.ViewModels;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace ServerAPI.JMS.Tests.Facility
{
    [Collection("Database collection")]
    public class FacilityHousingServiceTest
    {
        private readonly FacilityHousingService _facilityHousingService;
        private readonly IConfigurationRoot _configuration = MockHelpers.GetIConfigurationRoot();
        public FacilityHousingServiceTest(DbInitialize fixture)
        {
            DbInitialize fixture1 = fixture;
            HttpContextAccessor httpContext = new HttpContextAccessor { HttpContext = fixture1.Context.HttpContext };
            _facilityHousingService = new FacilityHousingService(fixture1.Db, _configuration, httpContext);
        }

        [Fact]
        public void GetHousingDetails()
        {
            //Act
            HousingDetail housingDetails = _facilityHousingService.GetHousingDetails(11);
            //Assert
            Assert.NotNull(housingDetails);
        }


        [Fact]
        public void GetHousing()
        {
            //Act
            List<HousingDetail> lstHousingDetails = _facilityHousingService.GetHousing(2);
            //Assert
            HousingDetail housingDetail = lstHousingDetails.Single(h => h.HousingUnitListId == 11);
            Assert.Equal("DOWNA01", housingDetail.HousingUnitBedNumber);
            Assert.Equal("FLOOR2", housingDetail.HousingUnitLocation);
        }

        [Fact]
        public void GetHousingList()
        {
            //Arrange
            List<int> lstInts = new List<int>
                {12, 11, 15};

            //Act
            List<HousingDetail> lstHousingDetails = _facilityHousingService.GetHousingList(lstInts);
            //Assert
            Assert.True(lstHousingDetails.Count>2);
            HousingDetail housingDetail = lstHousingDetails.Single(h => h.HousingUnitId == 12);
            Assert.Equal("Madras Central Jail",housingDetail.FacilityName);
            Assert.Equal("UPB01",housingDetail.HousingUnitBedNumber);
        }
    }
}
