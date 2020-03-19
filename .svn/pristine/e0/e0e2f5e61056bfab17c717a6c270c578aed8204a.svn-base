using System.Security.Claims;
using GenerateTables.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Moq;
using Xunit;
using ServerAPI.Tests;
using ServerAPI.Hubs;
using Microsoft.AspNetCore.SignalR;
using JwtDb.Models;

namespace ServerAPI.Tests
{
    public partial class DbInitialize
    {
        public AAtims Db { get; }
        public JwtDbContext JwtDb { get; }
        public readonly ControllerContext Context;
        public readonly Mock<IHubContext<AtimsHub>> HubContext = new Mock<IHubContext<AtimsHub>>();


        public DbInitialize()
        {
            ClaimsPrincipal user = new ClaimsPrincipal(new ClaimsIdentity(new[]
            {
                new Claim(ClaimTypes.Name, "atims"),
                new Claim(ClaimTypes.NameIdentifier, "dssiadmin"),
                new Claim("personnelId", "11"),
                new Claim("personnel_id", "11"),
                new Claim("personId", "50"),
                new Claim("facilityId", "1"),
                new Claim("id","5")

            }));
            Context = new ControllerContext
            {
                HttpContext = new DefaultHttpContext
                {
                    User = user
                }
            };


            //SingnalR Added 
            Mock<IHubClients> mockClients = new Mock<IHubClients>();
            Mock<IClientProxy> mockClientProxy = new Mock<IClientProxy>();
            mockClients.Setup(clients => clients.All).Returns(mockClientProxy.Object);
            HubContext.Setup(hub => hub.Clients).Returns(mockClients.Object);

            DbContextOptionsBuilder<AAtims> optionsBuilder = new DbContextOptionsBuilder<AAtims>();
            optionsBuilder.UseInMemoryDatabase("testdb");

            DbContextOptionsBuilder<JwtDbContext> jwtBuilder = new DbContextOptionsBuilder<JwtDbContext>();
            jwtBuilder.UseInMemoryDatabase("jwtdb");

            Db = new AAtims(optionsBuilder.Options);
            JwtDb = new JwtDbContext(jwtBuilder.Options);
            InmateDetails();
            PersonDetails();
            AppletsDetails();
            FormDetails();
            LookUpDetails();
            HousingDetails();
            WorkCrewDetails();
            IncarcerationDetails();
            AppAoDetails();
            ObservationDetails();
            Warrantdetails();
            KeepSepDetails();
            AppointmentDetails();
            ArrestDetails();
            ProgramDetails();
            VisitorDetails();
            DisciplinaryDetails();
            CrimeDetails();
            BailDetails();
            GrievanceDetails();
            SiteDetails();
            AtimsDetails();
            RecordDetails();
            AltSentdetails();
            WebserviceDetails();
            RequestDetails();
            CellLogDetails();
            UserDetails();
            SafetyDetails();
            VehicleDetails();
            CustomDetails();
            PREADetails();
            InvestigationDetails();
            Db.SaveChanges();
            JwtDb.SaveChanges();
        }
    }
}

[CollectionDefinition("Database collection")]
public class DatabaseCollection : ICollectionFixture<DbInitialize>
{
    // This class has no code, and is never created. Its purpose is simply
    // to be the place to apply [CollectionDefinition] and all the
    // ICollectionFixture<> interfaces.
}

