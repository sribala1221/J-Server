using System.Threading.Tasks;

namespace ServerAPI.Services
{
    public interface IAtimsHubService
    {
        Task StartHeadCount();
        Task GetIntakePrebookCount();
        Task GetPrebook();
        Task GetRequestCount();
        Task GetSafetyCheck();
        Task GetHeadCount();
        Task WizardStepChanged(int facilityId, int wizardId, string wizardName);
    }
    
}
