using ServerAPI.ViewModels;

namespace ServerAPI.Services
{
    public interface IMoneyBalanceDebitCheck
    {
        MoneyDebitCheckVm MoneyDebitCheck(MoneyDebitCheckVm param);
        MoneyBalanceVm MoneyTransactionCalculateBalance(MoneyTransactionCalculateVm param);
        MoneyBalanceVm MoneyUpdateBalance(MoneyBalanceVm param);
    }
}