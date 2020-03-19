  using System.Collections.Generic;
using ServerAPI.ViewModels;

namespace ServerAPI.Services
{
    public interface IMoneyVoidService
    {
        MoneyAccountVoidDetailsVm GetDoVoidDetails(int transId, MoneyAccountTransactionFlagType pFlag);
        int InsertDoVoid(SetVoidVm voidDetails);

    }
}