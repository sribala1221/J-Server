using System;
using System.Collections.Generic;
using GenerateTables.Models;

namespace ServerAPI.ViewModels {
    public class MoneyFund {

        public string FundName { get; set; }
        public decimal? BalanceFund { get; set; }
        public int BankId { get; set; }
        public int FundId { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public decimal? BalanceFundFee { get; set; }
        public decimal? BalanceFundPending { get; set; }
        public string BankName { get; set; }
        public Boolean IsSearch { get; set; }
    }

    public class MoneyFundVm {
        public List<MoneyAccountTransactionVm> PendingDetails;
        public List<MoneyAccountTransactionVm> UnpaidDetails;
        public List<MoneyAccountTransactionVm> FundLedgerDetails;
        public List<KeyValuePair<string, decimal>> MoneyFundDetails;
        public List<KeyValuePair<string, int>> bankList;
        public List<MoneyAccountAoFundVm> fundList;
    }
}