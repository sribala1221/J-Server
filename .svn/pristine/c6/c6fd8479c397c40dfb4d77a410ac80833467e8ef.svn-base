using System.Collections.Generic;
using ServerAPI.ViewModels;

namespace ServerAPI.Services
{
    public interface IMoneyService
    {
        List<KeyValuePair<string, int>> MoneyGetBank();
        List<MoneyAccountAoFundVm> MoneyGetFund();
        List<KeyValuePair<string, int>> MoneyGetDeposit();
        List<KeyValuePair<string, int>> MoneyGetCashDrawer(int facilityId, bool addVault);
        MoneyAccountTransactionDetailVm GetMoneyAccountTransaction(MoneyAccountFilterVm searchValue);
        MoneyAccountTransactionInfoVm GetMoneyTransactionDetails(int transId, int inmateId, int bankId,
            MoneyAccountTransactionFlagType pFlag);
        MoneyBankVm GetBankDetails();
        MoneyAccountAoInmateVm GetAccountAoInmate(int inmateId) ;
    }
}
