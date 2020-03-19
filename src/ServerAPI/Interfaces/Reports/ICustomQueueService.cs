
using ServerAPI.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ServerAPI.Services
{
    public interface ICustomQueueService
    {
        List<CustomQueueDetailsVm> GetCustomQueue();

        IEnumerable<QueueParameterOptionalVm> GetCustomQueueEntryDetails(int customQueueId);

        Task<string>  GetCustomQueueSearch(CustomQueueSearchInput customQueueSearch);
    }
}
