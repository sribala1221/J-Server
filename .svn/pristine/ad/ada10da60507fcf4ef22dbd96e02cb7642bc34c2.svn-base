using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using ServerAPI.Hubs;
using ServerAPI.Services;
using ServerAPI.Tests;
using Xunit;

// ReSharper disable once CheckNamespace
namespace ServerAPI.JMS.Tests
{
    [Collection("Database collection")]
    public class PrebookActiveServiceTest
    {

        private readonly PrebookActiveService _prebookActiveService;
       

        public PrebookActiveServiceTest(DbInitialize fixture, IHubContext<AtimsHub> hubContext)
        {
            DbInitialize fixture1 = fixture;
         
            HttpContextAccessor httpContext = new HttpContextAccessor { HttpContext = fixture.Context.HttpContext };
            AtimsHubService atimsHubService = new AtimsHubService(hubContext);
            _prebookActiveService = new PrebookActiveService(fixture1.Db, httpContext,atimsHubService);
        }
     
    }
}

