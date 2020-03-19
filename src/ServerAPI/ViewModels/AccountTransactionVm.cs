using System;

namespace ServerAPI.ViewModels
{
    public class AccountTransactionVm
    {
        public int AccountTransactionId { get; set; }
        public DateTime? TransactionDate { get; set; }
        public Decimal TransactionAmount { get; set; }
        public string TransactionDescription { get; set; }
        public string TransactionReceipt { get; set; }
        public string TransactionCheckNumber { get; set; }
        public Decimal TransactionBalanceInmate { get; set; }
        public Decimal TransactionBalanceAccount { get; set; }
        public Decimal TransactionBalanceFund { get; set; }
        public int InmateId { get; set; }
        public string InmateNumber { get; set; }
        public bool VoidFlag { get; set; }
        public PersonVm Person { get; set; }
        public int BankId { get; set; }
        public int FundId { get; set; }
        public string TransactionType { get; set; }
        public string TransactionReceiveNumber { get; set; }
        public int TransactionFromId { get; set; }
        public int TransactionToId { get; set; }
        public DateTime? TransactionFromDate { get; set; }
        public DateTime? TransactionToDate { get; set; }
    }

    public class MoneyInmateLedgerVm
    {
        public int BankId { get; set; }
        public int? InmateId { get; set; }
        public string Sample { get; set; }
        public Decimal PendingBalance { get; set; }
        public Decimal UnPaidBalance { get; set; }
        public Decimal LedgerBalance { get; set; }

    }
}