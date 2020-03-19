using System.Collections.Generic;
using System.Threading.Tasks;
using ServerAPI.ViewModels;

// ReSharper disable once CheckNamespace
namespace ServerAPI.Services
{
    public interface IMoneyVoidCashDrawerService
    {
        MoneyAccountVoidDetailsVm GetCashDrawerVoidDetails(int transId,bool voidFlag, MoneyAccountTransactionFlagType pFlag);
        Task<int> DoVoidCashDrawerAndVault(SetVoidVm setVoid);
        List<MoneyAccountTransactionVm> GetCashDrawerTransactionDetails(int transId);
    }
}
