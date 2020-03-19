using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ServerAPI.ViewModels;

namespace ServerAPI.Services    
{
    public interface IMoneyBalanceInmateService
    {
        List<MoneyBalanceInmateVm> MoneyBalanceInmateLoad(int bankId, int inmateStatus, int accountBalanceType, int facilityId);
        MoneyTransactionVm WorkstationMoneyDetailList(int facilityId);
        MoneyTransactionVm MoneyTransactionList(int facilityId, int bankId);
        MoneyBalanceInmateVm MoneyGetInmateCurrentBalance(int inmateId, int bankId);
        MoneyCashCheckVm MoneyReceiptCashCheckInsert(MoneyCashCheckVm param);
        MoneyCashCheckVm MoneyTransactionWriteCheckInsert(MoneyCashCheckVm param);
        MoneyCashCheckVm MoneyReturnCash(MoneyCashCheckVm param);
        MoneyCashCheckVm MoneyPurchase(MoneyCashCheckVm param);
        MoneyCashCheckVm MoneyAppFee(MoneyCashCheckVm param);
        MoneyCashCheckVm MoneyDonate(MoneyCashCheckVm param);
        MoneyCashCheckVm MoneyRefund(MoneyCashCheckVm param);
        MoneyCashCheckVm MoneyJournal(MoneyCashCheckVm param);
        List<MoneyLedgerVm> MoneyInmateLedgerTransaction(int flag, int bankId, int inmateId);
        List<MoneyCashCheckVm> MoneyRunFeeCheck(List<MoneyLedgerVm> aoFeeList);
    }
}
