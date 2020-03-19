using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Moq;
using ServerAPI.Services;
using ServerAPI.Tests;
using ServerAPI.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;

namespace ServerAPI.JMS.Tests
{
    [Collection("Database collection")]
    public class PersonServiceTest
    {
        private readonly PersonService _personService;
        private readonly DbInitialize _fixture;

        public PersonServiceTest(DbInitialize fixture)
        {
            _fixture = fixture;
            _personService = new PersonService(_fixture.Db);
        }

        [Fact]
        public void GetPersonNameList()
        {
            //Arrange
            List<int> personnelIds = new List<int>
            {
               12,13
            };

            //Act
            List<PersonnelVm> lstPersonnel = _personService.GetPersonNameList(personnelIds);
            //Assert
            PersonnelVm personnel = lstPersonnel.Single(p => p.PersonnelId == 12);
            Assert.Equal("KRISHNA", personnel.PersonLastName);
            Assert.Equal("1729", personnel.OfficerBadgeNumber);
        }

        [Fact]
        public void GetInmateDetails()
        {
            //Act
            PersonVm inmatedetails = _personService.GetInmateDetails(101);

            //Assert
            //Get inmate details
            Assert.NotNull(inmatedetails);
            Assert.Equal("MADURAI", inmatedetails.InmateCurrentTrack);
            Assert.Equal("PJS001", inmatedetails.InmateNumber);
        }

        [Fact]
        public void GetPersonDetailsFromPersonId()
        {
            //Act
            PersonVm person = _personService.GetPersonDetails(70);

            //Assert
            //Get person details
            Assert.NotNull(person);
            Assert.Equal("CHS0010", person.InmateNumber);
            Assert.Equal("RAHIMA", person.PersonLastName);
            Assert.Equal("NASRIN", person.PersonMiddleName);
        }

        [Fact]
        public void IsPersonSealed()
        {
            //Act
            bool personSeal = _personService.IsPersonSealed(60);

            //Assert
            Assert.True(personSeal);
        }
    }
}
