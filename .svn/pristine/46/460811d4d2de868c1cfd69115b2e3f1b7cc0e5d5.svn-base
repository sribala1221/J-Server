using GenerateTables.Models;
using JwtDb.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Moq;
using ServerAPI.JMS.Tests.Helper;
using ServerAPI.Services;
using ServerAPI.Tests;
using ServerAPI.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using JetBrains.Annotations;
using Xunit;
using Microsoft.Extensions.Caching.Memory;

// ReSharper disable once CheckNamespace
namespace ServerAPI.JMS.Tests
{

    [Collection("Database collection")]
    public class SearchInmateServiceTest
    {

        private readonly SearchInmateService _searchInmate;
        private readonly IConfigurationRoot _configuration = MockHelpers.GetIConfigurationRoot();
        private readonly DbInitialize _fixture;
        private readonly Mock<IMemoryCache> _memory = new Mock<IMemoryCache>();
        public SearchInmateServiceTest(DbInitialize fixture)
        {
            _fixture = fixture;
            HttpContextAccessor httpContext = new HttpContextAccessor { HttpContext = _fixture.Context.HttpContext };
            AppletsSavedService appletsSavedService = new AppletsSavedService(_fixture.Db, _configuration);
            PhotosService photosService = new PhotosService(_fixture.Db, _configuration);
            Mock<UserManager<AppUser>> userMgr = MockHelpers.MockUserManager<AppUser>();
            Mock<RoleManager<IdentityRole>> roleMgr = MockHelpers.MockRoleManager<IdentityRole>();
            AtimsHubService atimsHubService = new AtimsHubService(_fixture.HubContext.Object);
            PersonService personService = new PersonService(_fixture.Db);
            FacilityPrivilegeService facilityPrivilegeService = new FacilityPrivilegeService(_fixture.Db, httpContext,
                personService);
            InterfaceEngineService interfaceEngineService = new InterfaceEngineService(_fixture.Db,
                _configuration, _memory.Object);

            CommonService commonService = new CommonService(_fixture.Db, _configuration,
                httpContext, personService,appletsSavedService,interfaceEngineService);
            _searchInmate = new SearchInmateService(_fixture.Db, photosService,
                commonService, httpContext, _fixture.JwtDb, userMgr.Object, roleMgr.Object);
        }

        //For Startwith null exception 
        //[Fact]
        //public void GetBookingSearchList()
        //{
        //    SearchRequestVm searchRequest = new SearchRequestVm
        //    {
        //        IsInmateSearch = true,
        //        ActiveOnly = true,
        //        InmateNumber = "SVK661",
        //        IsPersonSearch = true,
        //        IsCharSearch = true,
        //        isInmateRelatedSearch = true,
        //        Eyecolor = 20,
        //        Moniker = "CHAIN SNATCHER"
        //    };

        //    _searchInmate.GetBookingSearchList(searchRequest);

        //}


        [Fact]
        public void GetInmateBookingInfo()
        {
            //Act
            InmateBookingInfoVm inmateBooking = _searchInmate.GetInmateBookingInfo();

            //Assert
            Assert.True(inmateBooking.ArrestingAgency.Count > 0);
            Assert.True(inmateBooking.OriginatingAgency.Count > 0);

        }

        [Fact]
        public void GetSentenceMethodInfo()
        {
            //Act
            List<ArrestSentenceMethod> lstArrestSentenceMethods = _searchInmate.GetSentenceMethodInfo();

            //Assert
            Assert.True(lstArrestSentenceMethods.Count > 0);
        }

        [Fact]
        public void GetAgencies()
        {
            //Act
            List<AgencyVm> lstAgency = _searchInmate.GetAgencies();

            //Assert
            Assert.True(lstAgency.Count > 1);

        }

        [Fact]
        public void GetChargeFlag()
        {
            //Act
            List<CrimeLookupFlag> lstCrimeLookupFlag = _searchInmate.GetChargeFlag();

            //Assert
            Assert.True(lstCrimeLookupFlag.Count > 1);
        }

        [Fact]
        public void GetCaseType()
        {
            //Act
            string result = _searchInmate.GetCaseType("5");
        }


        [Fact]
        public void GetPersonnel()
        {
            //Act
            List<PersonnelVm> lstPersonnel = _searchInmate.GetPersonnel(String.Empty, 7);

            //Assert
            PersonnelVm personnel = lstPersonnel.Single(p => p.PersonId == 60);
            Assert.Equal("KUMAR, SANKAR 1730", personnel.PersonnelFullDisplayName);

        }

        [Fact]
        public void GetLookups()
        {
            //Act
            IQueryable<Lookup> lstLookups = _searchInmate.GetLookups();

            //Assert
            Lookup lookup = lstLookups.Single(l => l.LookupDescription == "SAMOAN");
            Assert.Equal("RACE", lookup.LookupType);
            Assert.Equal("S", lookup.LookupName);
        }

        //For Startwith null exception 
        //[Fact]
        //public void GetFlagAlertDetails()
        //{

        //    List<FlagAlertVm> lstFlagAlert = _searchInmate.GetFlagAlertDetails(true);

        //    Assert.True(lstFlagAlert.Count>0);

        //}

        [Fact]
        public void PodDetails()
        {
            //Act
            List<HousingUnitList> lstHousingUnitList = _searchInmate.PodDetails("FLOOR1");

            //Assert
            Assert.True(lstHousingUnitList.Count > 2);

        }

    }
}
