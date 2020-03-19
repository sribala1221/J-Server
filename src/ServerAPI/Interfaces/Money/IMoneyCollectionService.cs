using System.Collections.Generic;
using ServerAPI.ViewModels;
using System.Threading.Tasks;
using System;

// ReSharper disable once CheckNamespace
namespace ServerAPI.Services
{
    public interface IMoneyCollectionService
    {
        List<MoneyFeeCollectionVm> GetMoneyFeeCollection(MoneyAccountFilterVm values);
        Task<int> UpdateMoneyFeeTransaction(MoneyFeeCollectionsVm values);
        List<MoneyAccountTransactionVm> GetFeeCollectionHistory(int bankId, DateTime? transactionClearedDate);
        List<DateTime> GetMoneyFeeClearedDate(bool flag, int month);
    }
}