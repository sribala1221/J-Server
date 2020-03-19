using ServerAPI.Services;
using System;
using Xunit;
using ServerAPI.Tests;
using ServerAPI.ViewModels;
using System.Linq;
using GenerateTables.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using ServerAPI.JMS.Tests.Helper;

// ReSharper disable once CheckNamespace
namespace ServerAPI.JMS.Tests
{
    [Collection("Database collection")]
    public class PersonDescriptorServiceTest
    {
        private readonly PersonDescriptorService _personDescriptorService;
        private readonly DbInitialize _fixture;
        private readonly IConfigurationRoot _configuration = MockHelpers.GetIConfigurationRoot();

        public PersonDescriptorServiceTest(DbInitialize fixture)
        {
            _fixture = fixture;
            HttpContextAccessor httpContext = new HttpContextAccessor { HttpContext = fixture.Context.HttpContext };
            PhotosService photosService = new PhotosService(_fixture.Db, _configuration);
            _personDescriptorService = new PersonDescriptorService(fixture.Db,  _configuration, httpContext,photosService);
        }
    
        [Fact]
        public void PersonDescriptor_GetIdentifierDetails()
        {
            //Act
            PersonDescriptorDetails descriptorDetails = _personDescriptorService.GetIdentifierDetails(65,false);
            //Assert
            PersonDescriptorVm personDescriptor =
                descriptorDetails.LstDescriptorPhotoGraphs.Single(d => d.IdentifierId == 70);
            Assert.Equal("ARM,RIGHT", personDescriptor.ItemLocation);
            Assert.Equal("TATTOOS", personDescriptor.Category);
        }

        [Fact]
        public void PersonDescriptor_GetDescriptorLookupDetails()
        {
            string[] bodyMap = 
            { "R_ARM",""}; 

            //Act
            PersonBodyDescriptor bodyDescriptor = _personDescriptorService.GetDescriptorLookupDetails(bodyMap, 60);
            //Assert
            PersonDescriptorVm personDescriptor =
                bodyDescriptor.LstCurrentDescriptor.Single(c => c.PersonDescriptorId == 13);
            Assert.Equal("TAT R ARM", personDescriptor.Code);
            Assert.Equal("WORDS", personDescriptor.DescriptorText);
        }

        [Fact]
        public void PersonDescriptor_DeleteUndoDescriptor()
        {
            //Arrange
            PersonDescriptorVm personDescriptorVm = new PersonDescriptorVm
            {
                DeleteFlag = 1,
                PersonDescriptorId = 14
            };

            PersonDescriptor personDescriptor = _fixture.Db.PersonDescriptor.Single(p => p.PersonDescriptorId == 14);
            Assert.Equal(1, personDescriptor.DeleteFlag);
            //Act
            _personDescriptorService.DeleteUndoDescriptor(personDescriptorVm);
            //Assert
            personDescriptor = _fixture.Db.PersonDescriptor.Single(p => p.PersonDescriptorId == 14);
            Assert.Equal(0, personDescriptor.DeleteFlag);
        }

        [Fact]
        public void PersonDescriptor_InsertUpdateDescriptor()
        {
            //Arrange
            PersonDescriptorVm personDescriptorvalues = new PersonDescriptorVm
            {
                CreateDate = DateTime.Now.AddDays(-1),
                UpdateDate = DateTime.Now.AddHours(-5),
                PersonId = 75,
                DeleteFlag = 0,
                Code = "SC RT ARM",
                Category = "SCARS",
                ItemLocation = "FOREARM, RIGHT",
                DescriptorText = "PERSON"
            };
            PersonDescriptor personDescriptor = _fixture.Db.PersonDescriptor.SingleOrDefault(p => p.PersonId == 75);
            Assert.Null(personDescriptor);
            //Act
            _personDescriptorService.InsertUpdateDescriptor(personDescriptorvalues);
            //Assert
            personDescriptor = _fixture.Db.PersonDescriptor.Single(p => p.PersonId == 75);
            Assert.NotNull(personDescriptor);
        }
    }
}
