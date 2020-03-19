using GenerateTables.Models;
using Microsoft.AspNetCore.Http;
using ServerAPI.Services;
using ServerAPI.Tests;
using ServerAPI.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace ServerAPI.JMS.Tests
{
    [Collection("Database collection")]
    public class FacilityPrivilegeServiceTest
    {
        private readonly FacilityPrivilegeService _facilityPrivilegeService;
        private readonly DbInitialize _fixture;

        public FacilityPrivilegeServiceTest(DbInitialize fixture)
        {
            _fixture = fixture;
            PersonService personService = new PersonService(_fixture.Db);
            HttpContextAccessor httpContext = new HttpContextAccessor { HttpContext = fixture.Context.HttpContext };
            _facilityPrivilegeService = new FacilityPrivilegeService(_fixture.Db, httpContext, personService);
        }

        [Fact]
        public void GetPrivilegeByOfficer()
        {
            //Arrange
            FacilityPrivilegeInput facilityPrivilegeInput = new FacilityPrivilegeInput
            {
                FacilityId = 2,
                ActiveToday = true
            };

            //Act
            List<FacilityPrivilegeVm> lstFacilityPrivilege = _facilityPrivilegeService.
                GetPrivilegeByOfficer(facilityPrivilegeInput);
            //Arrange
            FacilityPrivilegeVm facilityPrivilege = lstFacilityPrivilege.Single(f => f.InmatePrivilegeXrefId == 10);
            Assert.Equal("CHS0010", facilityPrivilege.Number);
            Assert.Equal("AUTH", facilityPrivilege.PrivilegeType);
            Assert.Equal("ATTEMPT SUICIDE", facilityPrivilege.Incident.IncidentType);
        }

        [Fact]
        public void GetReviewHistory()
        {
            //Act
            List<InmatePrivilegeReviewHistoryVm> lstInmatePrivilegeReviewHistory = _facilityPrivilegeService
                .GetReviewHistory(7);
            //Assert
            Assert.True(lstInmatePrivilegeReviewHistory.Count > 0);
        }

        [Fact]
        public async Task InsertReviewHistory()
        {
            //Arrange
            InmatePrivilegeReviewHistoryVm inmatePrivilegeReview = new InmatePrivilegeReviewHistoryVm()
            {
                InmatePrivilegeXrefId = 9,
                ReviewInterval = 5,
                ReviewActual = DateTime.Now,
                ReviewNext = DateTime.Now,
                RemovalNote = "PHONE PRIVILEGE",
                IsReauthorizeOrUnassign = true
            };
            //Before Add
            InmatePrivilegeReviewHistory inmatePrivilegeReviewHistory = _fixture.Db.InmatePrivilegeReviewHistory.SingleOrDefault
                (i => i.InmatePrivilegeXrefId == 9);
            Assert.Null(inmatePrivilegeReviewHistory);

            //Before Update
            InmatePrivilegeXref inmatePrivilegeXref =
                _fixture.Db.InmatePrivilegeXref.Single(i => i.InmatePrivilegeXrefId == 9);
            Assert.Null(inmatePrivilegeXref.PrivilegeRemoveNote);

            //Act
            await _facilityPrivilegeService.InsertReviewHistory(inmatePrivilegeReview);

            //Assert
            // After Add
            //Added values in InmatePrivilegeReviewHistory table
            inmatePrivilegeReviewHistory = _fixture.Db.InmatePrivilegeReviewHistory.Single
               (i => i.InmatePrivilegeXrefId == 9);
            Assert.NotNull(inmatePrivilegeReviewHistory);

            //After Updated values in InmatePrivilegeXref table
            inmatePrivilegeXref =
                _fixture.Db.InmatePrivilegeXref.Single(i => i.InmatePrivilegeXrefId == 9);
            Assert.Equal("PHONE PRIVILEGE", inmatePrivilegeXref.PrivilegeRemoveNote);

        }

        [Fact]
        public void GetPrivilegeList()
        {
            //Act
            List<PrivilegeDetailsVm> lstPrivilegeDetails = _facilityPrivilegeService.GetPrivilegeList(2);
            //Assert
            PrivilegeDetailsVm privilegeDetails = lstPrivilegeDetails.Single(p => p.PrivilegeId == 10);
            Assert.Equal("VELLORE", privilegeDetails.PrivilegeDescription);

        }
        [Fact]
        public void GetLocationList()
        {
            //Act
            List<KeyValuePair<int, string>> lstPrivilegeDetails = _facilityPrivilegeService.GetLocationList(1);
            //Assert
            Assert.True(lstPrivilegeDetails.Count > 2);
        }

        [Fact]
        public void GetTrackingLocationList()
        {
            //Act
            IQueryable<PrivilegeDetailsVm> lstPrivilegeDetails = _facilityPrivilegeService.GetTrackingLocationList(1);
            //Assert
            PrivilegeDetailsVm privilegeDetails = lstPrivilegeDetails.Single(p => p.PrivilegeId == 5);
            Assert.Equal("CHENNAI", privilegeDetails.PrivilegeDescription);
        }

    }
}
