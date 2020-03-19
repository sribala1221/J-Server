using ServerAPI.Services;
using ServerAPI.ViewModels;
using System.Collections.Generic;
using System.Linq;
using ServerAPI.Tests;
using Xunit;

// ReSharper disable once CheckNamespace
namespace ServerAPI.JMS.Tests
{
    [Collection("Database collection")]
    public class InmateNotesServiceTest
    {
        private readonly InmateNotesService _inmateNotesService;
        public InmateNotesServiceTest(DbInitialize fixture)
        {
            _inmateNotesService = new InmateNotesService(fixture.Db);
        }

        [Fact]
        public void InmateNotes_GetInmateNotes()
        {
            //Act
            IEnumerable<FloorNotesVm> lstFloorNotes = _inmateNotesService.GetInmateNotes(130);
            //Assert
            FloorNotesVm floorNotes = lstFloorNotes.Single(f => f.FloorNoteType == "NO TYPE");
            Assert.Equal("CHENNAI", floorNotes.FloorNoteLocation);
            Assert.Equal("KEEPSEPERATE THE INMATE", floorNotes.FloorNoteNarrative);
        }

        [Fact]
        public void InmateNotes_GetFloorNoteTypeCount()
        {
             //Act
            IEnumerable<FloorNoteTypeCount> typeCount = _inmateNotesService.GetFloorNoteTypeCount(110);
            //Assert
            Assert.True(typeCount.Any());
        }

        [Fact]
        public void InmateNotes_GetInmateNotesByType()
        {
            //Act
            IEnumerable<FloorNotesVm> floorNotes = _inmateNotesService.GetInmateNotesByType(120, "ALL");
            //Assert
            Assert.True(floorNotes.Any());
        }
    }
}
