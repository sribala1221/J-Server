using ServerAPI.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ServerAPI.Services
{
    public interface ILockdownService
    {
        List<LockdownVm> GetActiveLockdownDetails(int facilityId);
        LockdownDetailsVm GetLockdownEntryDetails(int housingLockdownId);
        Task<int> InsertUpdateLockdown(LockdownVm lockdown);
        Task<int> DeleteUndoLockdown(LockdownVm value);
        List<LockdownVm> GetLockdownHistoryDetails(DateTime? fromDate, DateTime? toDate,int facilityId);
        List<LockdownVm> GetLockdownDetails(int regionId, LockDownType region, string housingInfo);
        List<HousingDetail> GetHousingLockdownNotifications(HousingLockInputVm value);
    }
}
