using ServerAPI.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ServerAPI.Services
{
    public interface IHousingSupplyService
    {
        List<ClosetVm> GetClosetDetails(int facilityId);
        List<CheckListVm> GetSupplyCheckList(HousingSupplyInput input);
        HousingSupplyVm GetAvailableSupplyItems(int facilityId, int[] moduleId);
        List<SupplyItemsVm> GetAvailableHistoryLst(SupplyItemsVm supplyItemsVm);
        Task<int> UpdateCheckInTo(List<SupplyVm> supplyItems);
        Task<int> InsertSupplyDetails(List<SupplyVm> supplyItems);
        Task<int> UpdateDamage(List<SupplyVm> supplyItems);
        List<SupplyItemsVm> GetManageSupplyItem(int facilityId, List<int> housingSupplyModuleId);
        List<SupplyItemsVm> GetSupplyItemLookup();
        Task<int> InsertSupplyItem(SupplyItemsVm supplyItems);
        Task<int> UpdateSupplyItem(SupplyItemsVm supplyItems);
        Task<int> DeleteSupplyItem(SupplyItemsVm supplyItems);
        WareHouseLookup GetWareHouseLookup(int facilityId);
        List<WareHouseItemVm> GetHousingActiveRequestDetail(WareHouseItemVm wareHouseItem);
        HousingSupplyVm GetSupplyItems(int facilityId, int housingSupplyModuleId, int checkListId);
        Task<int> DeleteUndoCheckList(CheckListVm checkList);
        Task<int> InsertCheckOutItem(HousingSupplyVm housingSupply, string note);
        Task<int> WareHouseInsert(List<WareHouseItemVm> wareHouseItem);
        List<HousingSupplyItemHistoryVm> GetHousingSupplyHistory(SupplyItemsVm supplyItems);
        List<HousingSupplyHistoryLocation> GetSupplyHistoryLocation(int facilityId,int[] moduleId);
    }
}
