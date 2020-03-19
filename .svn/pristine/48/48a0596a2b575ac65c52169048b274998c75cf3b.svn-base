using ServerAPI.Services;
using ServerAPI.Tests;
using ServerAPI.ViewModels;
using System;
using Xunit;

// ReSharper disable once CheckNamespace
namespace ServerAPI.JMS.Tests
{
    [Collection("Database collection")]
    public class VisitAvailabilityServiceTest
    {
        private readonly VisitAvailabilityService _visitAvailability;
        private readonly DbInitialize _fixture;
        public VisitAvailabilityServiceTest(DbInitialize fixture)
        {
            _fixture = fixture;
            _visitAvailability = new VisitAvailabilityService(_fixture.Db);
        }


        [Fact]
        public void GetVisitAvailability()
        {
            //Arrange
            VisitScheduledVm visitScheduled = new VisitScheduledVm
            {
                InmateId = 100,
                FacilityId = 1,
                VisitDate = DateTime.Now.AddDays(-1)
            };
            //Act
            VisitScheduledDetail visitScheduledDetail = _visitAvailability.GetVisitAvailability(visitScheduled);

            //Assert
            Assert.True(visitScheduledDetail.SlotProfRoomInfo.Count > 0);
            Assert.True(visitScheduledDetail.VisitationRoomInfo.Count > 0);
        }
    }
}
