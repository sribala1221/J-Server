using System.Collections.Generic;
using ServerAPI.ViewModels;

namespace ServerAPI.Services
{
    public interface IMoneyDepositVerifyService
    {
        MoneyDepositoryDetailVm GetMoneyDepositoryDetails();
        MoneyDepositoryDetailVm GetMoneyDepositoryVerifyDetails(int bankId, int accountAoDepositoryId,
            int fundId, int personnelId);
        int MoneyDepositoryVerify(List<MoneyAccountTransactionVm> feeList);
    }
}