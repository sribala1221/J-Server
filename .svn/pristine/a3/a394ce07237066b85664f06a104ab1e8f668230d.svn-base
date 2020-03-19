using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using ServerAPI.Hubs;

namespace ServerAPI.Services
{
    public class AtimsHubService : IAtimsHubService
    {
        private readonly IHubContext<AtimsHub> _hubContext;

        public AtimsHubService(IHubContext<AtimsHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task StartHeadCount()
        {
            await _hubContext.Clients.All.SendAsync("headCount", "Begin Headcount");
        }

        public async Task GetIntakePrebookCount()
        {
            await _hubContext.Clients.All.SendAsync("getIntakePrebookCount", "update");
        }

        public async Task GetPrebook()
        {
            await _hubContext.Clients.All.SendAsync("getPrebook","Get Prebook");
        }
        public async Task GetRequestCount()
        {
            await _hubContext.Clients.All.SendAsync("getRequestCount","Get Request Count");
        }
        public async Task GetSafetyCheck()
        {
            await _hubContext.Clients.All.SendAsync("getSafetyCheck","Get Safety Check");
        }
        public async Task GetHeadCount()
        {
            await _hubContext.Clients.All.SendAsync("getHeadCount","Get head count");
        }

        public async Task WizardStepChanged(int facilityId, int wizardId, string wizardName)
        {
            await _hubContext.Clients.All.SendAsync("wizardStepChanged", new { facilityId = facilityId, wizardId = wizardId, wizardName = wizardName });
        }
    }
}
