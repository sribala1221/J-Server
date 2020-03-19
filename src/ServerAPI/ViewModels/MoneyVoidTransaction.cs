using System;
namespace ServerAPI.ViewModels
{
    public class MoneyVoidTransactionFee
    {
        public int AffectedRows { get; set; }
        public string AffectedTable { get; set; }
        public string TransactionType { get; set; }
        public decimal TransactionAmount { get; set; }
        public int IdentityValue { get; set; }
        public int StatusCode { get; set; }
        public string ErrorMessage { get; set; }
        public string AffectingTable { get; set; }
        public int BankId { get; set; }
        public int FundId { get; set; }
        public int CreatedBy { get; set; }
        public DateTime CreatedDate { get; set; }
        public int InmateId { get; set; }
        public decimal BalanceInmate { get; set; }
        public decimal BalanceAccount { get; set; }
        public decimal BalanceFund { get; set; }
        public int CurrentTransactionId { get; set; }
        public int VoidTransactionId { get; set; }
        public int FeeId { get; set; }
        public bool TransactionClearedBy { get; set; }
        public DateTime? TransactionClearedDate { get; set; }
        public decimal TransactionClearedAmount { get; set; }
        public string Type { get; set; }
        public int ReceiveId { get; set; }
        public string ReceiveNumber { get; set; }
        public string ReceiveFrm { get; set; }
        public string PayToOrder { get; set; }
        public string CheckNo { get; set; }
        public string DebitMemo { get; set; }
        public string TransactionDescription { get; set; }
        public string CurrentTransactionDescription { get; set; }
      //  public string ReceiptNo { get; set; }
        public decimal AbsTotalAmount { get; set; }
        public int FromTransactionId { get; set; }
        public int ToTransactionId { get; set; }
        public bool ReturnCashFlag { get; set; }
        public int AccountAoFeeTypeId { get; set; }
        public bool TransactionVoidBy { get; set; }
        public int CashId  { get; set; }
        public DateTime? TransactionDate { get; set; }
         public int AccountAoDepositoryId { get; set; }
         public string NextReceiptNum { get; set; }
         public int CashBalanceId { get; set; }
          public bool AdjustFlag {get ;set;}
        public int AccountAoCashDrawerId { get; set; }
    }
}
