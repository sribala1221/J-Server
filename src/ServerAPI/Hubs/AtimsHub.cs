using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace ServerAPI.Hubs
{
    public class AtimsHub : Hub
    {
        public async Task StartHeadCount()
        {
            await Clients.All.SendAsync("headCount", new {status = 0, message = "Begin Headcount"});
        }

        public async Task GetIntakePrebookCount()
        {
            await Clients.All.SendAsync("getIntakePrebookCount", new {stateus = 0, message = "Update"});
        }
        public async Task GetPrebook()
        {
            await Clients.All.SendAsync("getPrebook", new {status = 0, message = "Get Prebook"});
        }
        
        public async Task GetRequestCount()
        {
            await Clients.All.SendAsync("getRequestCount", new {status = 0, message = "Get Request Count"});
        }

        public async Task GetSafetyCheck()
        {
            await Clients.All.SendAsync("getSafetyCheck", new {status = 0, message = "Get Safety Check"});
        }
        public async Task GetHeadCount()
        {
            await Clients.All.SendAsync("getHeadCount", new {status = 0, message = "Get Head Count"});
        }
    }
}
