using System;
using System.Collections.Generic;

namespace ServerAPI.ViewModels
{
    public class MoneyFeeCollectionVm
    {
        public int AccountAoFeeId { get; set; }
        public string TransactionDescription { get; set; }
        public PersonnelVm PersonBy { get; set; }
        public PersonnelVm Person { get; set; }
        public DateTime TransactionDate { get; set; }
        public int FundId { get; set; }
        public int TransactionVoidFlag { get; set; }
        public int TransactionVoidId { get; set; }
        public string InmateNumber { get; set; }
        public decimal TransactionAmount { get; set; }
        public int InmateId { get; set; }
        public string CollectionName { get; set; }
        public string TransactionType { get; set; }
        public string BankAccountAbbr { get; set; }
        public string FundName { get; set; }
        public decimal BalanceFund { get; set; }
        public decimal BalanceInmate { get; set; }
        public decimal BalanceAccount { get; set; }
        public decimal BalanceAccountFee { get; set; }
        public HousingDetail HousingUnit { get; set; }
        public string TransactionNotes { get; set; }
    }

    public class MoneyFeeCollectionsVm
    {
        public List<MoneyFeeCollectionVm> MoneyFeeCollection { get; set; }
        public string CollectionName { get; set; }
    }

}