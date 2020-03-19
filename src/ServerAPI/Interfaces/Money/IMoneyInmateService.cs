using System.Collections.Generic;
using System.Threading.Tasks;
using ServerAPI.ViewModels;

namespace ServerAPI.Services
{
    public interface IMoneyInmateService
    {
        MoneyAccountTransactionDetailVm GetMoneyInmateLedger(int iBankId, int iInmateId);
        MoneyAccountTransactionDetailVm GetMoneyInmateDeposit(int iBankId, int iInmateId);
        MoneyAccountTransactionDetailVm GetMoneyInmateFeeDetails(int bankId, int inmateId);
        MoneyAccountTransactionDetailVm GetMoneyInmateReference(int? bankId, int? inmateId);
        string GetAccountTransactionId(int transId);
        MoneyAccountTransactionVm GetPrintReceive(int receiveId, int inmateId);
        MoneyAccountTransactionDetailVm MoneyInmateClear(int bankId, int inmateId);
        Task<int> UpdateMoneyClearTransaction(int accountAoTransactionId);
        Task<int> InmateMoneyReturnTransaction(MoneyAccountTransactionVm moneyAccountTransaction);
    }
}
