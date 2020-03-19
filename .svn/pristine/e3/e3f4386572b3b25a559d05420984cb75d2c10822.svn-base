using System.Collections.Generic;
using ServerAPI.ViewModels;
using System.Threading.Tasks;

namespace ServerAPI.Services
{
    public interface IInventoryQueueService
    {
        InventoryQueueVm GetInventoryQueue(int facilityId,int value,int scheduled);
        InventoryQueueDetailsVm GetInventoryInmateDetails(int facilityId, InventoryQueue value,
            int personalInventoryBinId, int selected,int schSelected);
       List<InventoryFormsDetails> GetInventoryProperyFormsDetails();
       Task<int> ClearInventoryQueue(int recordId);
    }
}
