using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using ServerAPI.Services;
using ServerAPI.Tests;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using ServerAPI.ViewModels;
using ServerAPI.JMS.Tests.Helper;
using Microsoft.AspNetCore.Identity;
using Moq;
using JwtDb.Models;
using Microsoft.Extensions.Caching.Memory;

// ReSharper disable once CheckNamespace
namespace ServerAPI.JMS.Tests
{
    [Collection("Database collection")]
    public class FacilityOperationsServiceTest
    {

        private readonly FacilityOperationsService _facilityOperationsService;
        private readonly IConfigurationRoot _configuration = MockHelpers.GetIConfigurationRoot();
        private readonly Mock<IMemoryCache> _memory = new Mock<IMemoryCache>();
        public FacilityOperationsServiceTest(DbInitialize fixture)
        {
            DbInitialize fixture1 = fixture;
            PersonService personService = new PersonService(fixture1.Db);
            Mock<UserManager<AppUser>> userMgr = MockHelpers.MockUserManager<AppUser>();
            AppletsSavedService appletsSavedService = new AppletsSavedService(fixture.Db, _configuration);
            HttpContextAccessor httpContext = new HttpContextAccessor { HttpContext = fixture1.Context.HttpContext };
            PhotosService photosService = new PhotosService(fixture1.Db, _configuration);
            InterfaceEngineService interfaceEngineService = new InterfaceEngineService(fixture1.Db,
                _configuration, _memory.Object);
            CommonService commonService = new CommonService(fixture1.Db, _configuration, httpContext, personService,appletsSavedService,interfaceEngineService);
            MiscLabelService miscLabelService = new MiscLabelService(fixture1.Db,photosService,_configuration,
                httpContext,commonService);
           _facilityOperationsService = new FacilityOperationsService(fixture1.Db, commonService,
               photosService,miscLabelService,_configuration);
        }


        [Fact]
        public void FacilityOperations_GetRosterMasterDetails()
        {
            //Arrange
            RosterFilters rosterFilters = new RosterFilters
            {
                FacilityId = 2,
                Gender = 2,
                Balance = "POSITIVE > 0.00",
                Association = "LOCAL BOYS",
                Race = 5,
                Status = "Sent",
                IsPageInitialize = true,
                HousingUnitListId = 9
            };
            //Act
            RosterMaster rosterMaster = _facilityOperationsService.GetRosterMasterDetails(rosterFilters);

            //Assert
            //Roster
            Assert.NotNull(rosterMaster.RosterDetails);

            //Location
            Assert.NotNull(rosterMaster.LocationDetails);

        }

        [Fact]
        public void FacilityOperations_GetPersonFormTemplates()
        {
            //Act
            List<FormTemplate> lstFormTemplate = _facilityOperationsService.GetPersonFormTemplates();
            //Assert
            FormTemplate formTemplate = lstFormTemplate.Single(f => f.PersonFormTemplateId == 5);
            Assert.Equal("PROPERTY LABEL", formTemplate.TemplateName);
        }

        [Fact]
        public void FacilityOperations_GetInmateBookings()
        {
            //Arrange
            int[] lstinmate = { 101, 105, 102, 155, 160 };

            //Act
            List<InmateBookings> lstInmateBooking = _facilityOperationsService.GetInmateBookings(lstinmate);

            //Assert
            InmateBookings inmateBookings = lstInmateBooking.Single(i => i.ArrestId == 8);
            Assert.Equal("160001132", inmateBookings.ArrestBookingNo);
        }
    }
}
