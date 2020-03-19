using System.Threading.Tasks;
using System.Collections.Generic;
using ServerAPI.ViewModels;

namespace ServerAPI.Services
{
    public interface ITempHoldService
    {
        IntakeTempHoldVm GetIntakeTempHoldDetails(PersonnelSearchVm personnelSearchVm);
        Task<TempHoldDetailsVm> SaveIntakeTempHold(IntakeTempHoldParam objTempHoldParam);
        Task<int> UpdateTempHold(PrebookCompleteVm objParam);
		TempHoldVm GetTempHoldDetails(TempHoldDetailsVm tempHoldReq);
        TempHoldCompleteStepLookup GetTempHoldCompleteStepLookup(int tempHoldId);
    }
}