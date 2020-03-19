using System.Collections.Generic;
using System.Threading.Tasks;
using ServerAPI.ViewModels;

namespace ServerAPI.Services
{
    public interface IInventoryLostFoundService
    {
        LostFoundInventory GetLostFoundInventory(SearchOptionsView lostFoundFilters);
        InventoryLostFoundVm ItemList(int groupId);
        Task<int> InsertInventory(IdentitySave value);
        Task<int> DeleteOrUndoLostFound(InventoryDetails inventoryDetails);
        InventoryLostFoundVm InventoryPdfGet(int groupId);
        List<HousingDetail> GetHousingUnitNumberDetails(int facilityId, string housingUnitLocation);
        List<KeyValuePair<int, string>> GetBinType(int facilityId);
        List<InventoryLostFoundHistory> HistoryDetails(HistorySearch value);
        Task<int> Disposition(InventoryDetails inventoryDetails);
    }   
}
