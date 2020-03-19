using System;

namespace ServerAPI.ViewModels
{
    public class MoneyInmateClearVm
    {
        public int AccountAoTransactionId { get; set; }
        public decimal? BalanceInmate { get; set; }
        public decimal TransactionAmount { get; set; }
        public string TransactionCheckNumber { get; set; }
        public DateTime? TransactionDate { get; set; }
        public string TransactionDescription { get; set; }
        public string TransactionReceipt { get; set; }
        public bool TransactionReturnCheckFlag { get; set; }
        public bool TransactionVoidFlag { get; set; }
    }
}
