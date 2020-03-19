using Microsoft.Extensions.Configuration;
using ServerAPI.JMS.Tests.Helper;
using ServerAPI.Services;
using ServerAPI.Tests;
using ServerAPI.ViewModels;
using System.Collections.Generic;
using System.Linq;
using Xunit;

// ReSharper disable once CheckNamespace
namespace ServerAPI.JMS.Tests
{
    [Collection("Database collection")]
    public class HousingStatsServiceTest
    {
        private readonly HousingStatsService _housingStatsService;
        private readonly IConfigurationRoot _configuration = MockHelpers.GetIConfigurationRoot();

        public HousingStatsServiceTest(DbInitialize fixture)
        {
            var fixture1 = fixture;
            PhotosService photoService = new PhotosService(fixture1.Db, _configuration);
            _housingStatsService = new HousingStatsService(fixture1.Db, photoService);
        }

        // To get list of inmates based on Gender Flag
        [Fact]
        public void GetStatsInmateDetails()
        {
            HousingStatsInputVm housingStatsInput = new HousingStatsInputVm
            {
                FacilityId = 1,
                HousingStatsCount = new HousingStatsCount
                {
                    EventFlag = CellsEventFlag.Gender
                },
                HousingType = HousingType.BedLocation,
                HousingUnit = new HousingDetail
                {

                    HousingUnitBedLocation = null,
                    HousingUnitBedNumber = "UPA011",
                    HousingUnitListId = 10,
                    HousingUnitId = 15
                }
            };

            List<HousingStatsDetails> lsthHousingStatsDetails = _housingStatsService.GetStatsInmateDetails(housingStatsInput);

            HousingStatsDetails housingStatsDetails = lsthHousingStatsDetails.Single(h => h.InmateId == 143);
            Assert.Equal("IN3745", housingStatsDetails.InmateNumber);
            Assert.Equal("VELLORE", housingStatsDetails.Location);
            Assert.Equal("VIJAYA", housingStatsDetails.PersonDetail.PersonFirstName);
        }

        // To get list of inmates based on Alert Flag
        [Fact]
        public void GetFlagDetails()
        {
            HousingStatsInputVm housingStatsInput = new HousingStatsInputVm
            {
                FacilityId = 2,
                HousingStatsCount = new HousingStatsCount
                {
                    EventFlag = CellsEventFlag.Flag,
                    Type = "PERSONCAUTION",
                    FlagId = 2,
                    FlagName = "GANG DROPOUT"
                },
                HousingType = HousingType.BedNumber,
                HousingUnit = new HousingDetail
                {

                    HousingUnitBedLocation = "DOWN BED LOC1",
                    HousingUnitBedNumber = "DOWNA01",
                    HousingUnitListId = 11,
                    HousingUnitId = 14
                }
            };

            List<HousingStatsDetails> lsthHousingStatsDetails = _housingStatsService.GetStatsInmateDetails(housingStatsInput);

            HousingStatsDetails housingStatsDetails = lsthHousingStatsDetails.Single(h => h.InmateId == 107);
            Assert.Equal(2, housingStatsDetails.FlagId);
            Assert.Equal("GANG DROPOUT", housingStatsDetails.Flags);
        }


        // To get list of inmates based on Race Flag
        [Fact]
        public void GetRaceDetails()
        {
            HousingStatsInputVm housingStatsInput = new HousingStatsInputVm
            {
                FacilityId = 2,
                HousingStatsCount = new HousingStatsCount
                {
                    EventFlag = CellsEventFlag.Race,
                    Type = "RACE",
                    FlagId = 10
                },
                HousingType = HousingType.HousingLocation,
                HousingUnit = new HousingDetail
                {

                    HousingUnitLocation = "FLOOR2",
                    HousingUnitId = 12
                }
            };

            List<HousingStatsDetails> lsthHousingStatsDetails = _housingStatsService.GetStatsInmateDetails(housingStatsInput);

            HousingStatsDetails housingStatsDetails = lsthHousingStatsDetails.Single(h => h.InmateId == 141);
            Assert.Equal(126, housingStatsDetails.PersonId);
            Assert.Equal("SAMOAN", housingStatsDetails.Flags);
        }

        // Based on Association Flag
        [Fact]
        public void GetClassifyDetails()
        {
            HousingStatsInputVm housingStatsInput = new HousingStatsInputVm
            {
                FacilityId = 1,
                HousingStatsCount = new HousingStatsCount
                {
                    EventFlag = CellsEventFlag.Association,
                    FlagName = "EIGHT-EASTERN EUROPEANS",
                    FlagId = 6
                },
                HousingType = HousingType.NoHousing
            };

            List<HousingStatsDetails> lsthHousingStatsDetails = _housingStatsService.GetStatsInmateDetails(housingStatsInput);
            Assert.True(lsthHousingStatsDetails.Count>0);
        }

        // To get list of inmates based on Classify Flag
        [Fact]
        public void Classification()
        {
            HousingStatsInputVm housingStatsInput = new HousingStatsInputVm
            {
                FacilityId = 1,
                HousingGroupId = 12,
                HousingStatsCount = new HousingStatsCount
                {
                    EventFlag = CellsEventFlag.Classification,
                    FlagName = "PENDING"
                },
                HousingUnit = new HousingDetail
                {
                    HousingUnitListId = 6
                },
                HousingType = HousingType.Number
            };

            List<HousingStatsDetails> lsthHousingStatsDetails = _housingStatsService.GetStatsInmateDetails(housingStatsInput);

            HousingStatsDetails housingStatsDetails = lsthHousingStatsDetails.Single(h => h.InmateId == 110);
            Assert.Equal(100, housingStatsDetails.PersonId);

        }



    }
}
