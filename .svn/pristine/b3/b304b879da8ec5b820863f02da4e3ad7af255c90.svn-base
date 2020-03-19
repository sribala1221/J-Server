using System;
using System.Collections.Generic;
using Xunit;
using ServerAPI.ViewModels;
using ServerAPI.Services;
using System.Linq;
using ServerAPI.Tests;
using System.Threading.Tasks;
using GenerateTables.Models;
using Microsoft.AspNetCore.Http;

// ReSharper disable once CheckNamespace
namespace ServerAPI.JMS.Tests
{
    [Collection("Database collection")]
    public class PersonDnaServiceTest
    {
        private readonly PersonDnaService _personDna;
        private readonly DbInitialize _fixture;

        public PersonDnaServiceTest(DbInitialize fixture)
        {
            _fixture = fixture;
            PersonService personService = new PersonService(fixture.Db);
            HttpContextAccessor httpContext = new HttpContextAccessor { HttpContext = fixture.Context.HttpContext };
            _personDna = new PersonDnaService(_fixture.Db,  httpContext,personService);
        }

        [Fact]
        public void PersonDna_GetDnaDetails()
        {
            //Act
            List<DnaVm> lstdna = _personDna.GetDnaDetails(70);
            //Assert
            DnaVm dna = lstdna.Single(d => d.DnaId == 8);
            Assert.Equal("COLLECTED THE DNA REPORT DOCUMENTS", dna.Notes);
            Assert.Equal("SANGEETHA", dna.CreateByPersonFirstName);
            Assert.Equal("MISC", dna.ProcessedDisposition);
        }


        [Fact]
        public void PersonDna_GetDnaHistoryDetails()
        {
            //Act
            List<HistoryVm> lsthistory = _personDna.GetDnaHistoryDetails(8);
            //Assert
            HistoryVm history = lsthistory.Single(h => h.HistoryId == 5);
            Assert.Equal("KRISHNA", history.PersonLastName);
            Assert.Equal("1729", history.OfficerBadgeNumber);
        }
      
        [Fact]
        public async Task PersonDna_InsertUpdatePersonDna()
        {
            //InsertPersonDna
            //Arrange
            DnaVm createdna = new DnaVm {
                PersonId = 65,
                CreateDate = DateTime.Now,
                UpdateDate = DateTime.Now,
                RequestDate = DateTime.Now,
                GatheredDate = DateTime.Now,
                Notes = "FILE STORED IN JAIL",
                ProcessedDisposition = "MISC",
                ProcessedBy = "SURYA",
                PersonDnaHistoryList =
                    @"{'DNA Requested':'True','Date Requested':'09/09/2017','Date Gathered':null,'Gathered By':'11','Disposition':'1','Date Processed':'09/10/2017','Processed By':null,'Disposition ':'2','Notes':'DNA FILE'} "
            };

            //Before Insert
            PersonDna personDna = _fixture.Db.PersonDna.SingleOrDefault(p => p.PersonId == 65);
            Assert.Null(personDna);

            //Act
            await _personDna.InsertUpdatePersonDna(createdna);
            //Assert
            //After Insert
            personDna = _fixture.Db.PersonDna.Single(p => p.PersonId == 65);
            Assert.NotNull(personDna);

            //UpdatePersonDna
            //Arrange
            DnaVm updatedna = new DnaVm {
                PersonId = 75,
                Disposition = 3,
                ProcessedDisposition = "MISC",
                ProcessedBy = "VARUN",
                DnaId = 10,
                CreateDate = DateTime.Now.AddDays(-1),
                UpdateDate = DateTime.Now,
                GatheredDate = DateTime.Now.AddDays(-1),
                RequestDate = DateTime.Now,
                DateProcessed = DateTime.Now
            };
            //Before Update
            personDna = _fixture.Db.PersonDna.Single(p => p.PersonDnaId == 10);
            Assert.Equal(2, personDna.DnaDisposition);
            Assert.Null(personDna.DnaProcessedBy);
            //Act
            await _personDna.InsertUpdatePersonDna(updatedna);
            //Assert
            //After Update
            personDna = _fixture.Db.PersonDna.Single(p => p.PersonDnaId == 10);
            Assert.Equal(3, personDna.DnaDisposition);
            Assert.Equal("VARUN", personDna.DnaProcessedBy);
        }

       
    }
}
