using System.Collections.Generic;
using System.Threading.Tasks;
using ServerAPI.ViewModels;

namespace ServerAPI.Services
{
    public interface IMoneyClearService
    {
        MoneyClearVm GetDropDownDetails();
        List<MoneyAccountTransactionVm> GetMoneyClearClearDetails(MoneyAccountFilterVm searchValue);
        Task<int> UpdateMoneyClearClearTransaction(List<int> checkedTransIdList);
        List<MoneyAccountTransactionVm> GetMoneyClearHistoryDetails(MoneyAccountFilterVm searchValue);
        Task<int> InsertReturnCheck(List<int> value);
    }
}