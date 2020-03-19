using System.Collections.Generic;
using ServerAPI.ViewModels;

namespace ServerAPI.Services
{
    public interface IAccountTransactionService
    {

        List<AccountTransactionVm> GetAccountTransactionList(AccountTransactionVm accTransaction);
       List<MoneyInmateLedgerVm> GetMoneyLedgerList(MoneyInmateLedgerVm ledger);
    }
}
