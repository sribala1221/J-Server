using ServerAPI.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ServerAPI.Interfaces
{
    public interface ISafetyCheckService
    {
        List<SafetyCheckVm> GetSafetyCheckList(ConsoleInputVm value);
        List<SafetyCheckVm> LoadSafetyCheckHousingList(ConsoleInputVm value);
        SafetyCheckVm LoadSafetyCheckHousingDetails(int facilityId, int housingUnitListId);
        List<SafetyCheckVm> LoadSafetyCheckLocationList(ConsoleInputVm value);
        SafetyCheckVm LoadSafetyCheckLocationDetails(int locationId);
        Task<int> InsertSafetyCheck(SafetyCheckVm value);
        HeadCountHistoryDetails LoadSafetyCheckHistoryList(HeadCountHistoryDetails headCountHistoryDetails);
    }
}