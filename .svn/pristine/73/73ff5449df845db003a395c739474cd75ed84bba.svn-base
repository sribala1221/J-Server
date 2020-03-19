using System.Collections.Generic;
using ServerAPI.ViewModels;
using System.Threading.Tasks;

namespace ServerAPI.Services
{
    public interface IInventoryInmateService
    {
        InventoryVm GetInventoryGrid(int inmateId, Disposition disposition, int showDeleteFlag);
        List<InventoryHistoryVm> GetInventoryHistory(int inventoryId, int inmateId, Disposition disposition,bool inventoryFlag);
        List<PropGroupHistoryDetails> PropertyGroupHistory(int propGroupId);
        InventoryChangeGroupVm LoadChangeInventoryDetails(int inmateId, int inventoryBinId, int inventoryGroupId,
            Disposition disposition);
        List<BinInmateLoadDetails> BinInmateDetails(int personalInventoryBinId);
        Task<int> UpdateInventoryInmate(InventoryChangeGroupVm value);
        BinInventoryVm AvailableBinItems(int facilityId);
        BinDeleteVm DeleteInventoryDetails();
        Task<int> DeleteandUndoInventory(InventoryDetails obj);
        List<PersonalInventoryPreBookVm> GetPreBookInventoryItem(int inmateId, int personalInventoryId);
        Task<int> InsertInventoryAddItems(PersonalInventoryPreBookVm value);
        Task<int> UpdatePropertyGroupNotesEntry(InventoryItemDetails value);
        MoveBinVm MoveBinInmateDetails(int inmateId, int inventoryBinId);
        Task<int> InsertInventoryMove(MoveBinVm value);
        InventoryVm GetReleaseItems(int inmateId, Disposition disposition, int showDeleteFlag);
        InventoryLookupVm GetLookupDetails();
        PersonAddressVm ReleaseItemAddressDetails(int personId);
        InventoryInStorage GetInventoryInStorage(InventoryVm inventoryDetails, bool isRelease);
        Task<int> InsertPropertyPhoto(PersonPhoto personPhoto);
        Task<int> DeletePropertyPhoto(int identifiersId);
        InventoryInStorage GetPropertyGroupDetails(InventoryDetails inventoryDetails);
        Task<int> InsertInventoryMisplacedValues(InventoryDetails inventoryDetails);
    }
}