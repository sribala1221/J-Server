using System;
using System.Collections.Generic;

namespace ServerAPI.ViewModels {
    public class MoneyDebitCheckVm {
        public int InmateId { get; set; }
        public int FeeFlag { get; set; }
        public int DepositFlag { get; set; }
        public int FeeCheckFlag { get; set; }
        public int AccountAoFeeId { get; set; }
        public decimal ThersholdAmount { get; set; }
        public bool ReturnExitFlag { get; set; }
        public decimal TotalPayAmount { get; set; }
        public int StatusCode { get; set; }
        public string ErrorMessage { get; set; }
        public decimal PartialPayAmount { get; set; }
        public decimal NewFee { get; set; }
    }
    public class MoneyAccountAoInmateVm {
        public int AccountAoInmateId { get; set; }
        public int AccountAoBankId { get; set; }
        public decimal BalanceInmate { get; set; }
        public decimal BalanceInmateFee { get; set; }
        public decimal BalanceInmatePending { get; set; }
        public int? InmateId { get; set; }
        public PersonVm Inmate { get; set; }
        public int FundId { get; set; }
    }
    public class MoneyAccountAoFeeVm {
        public decimal TransactionFee { get; set; }
        public int FeeWaiveDebtFlag { get; set; }
        public decimal FundAllowFeePayDebtNotToExceed { get; set; }
        public double FundAllowFeePayDebtPercentage { get; set; }
        public int InmateId { get; set; }
        public string FundName { get; set; }
        public int AccountAOFundId { get; set; }
        public decimal FundInmateMinBalance { get; set; }
        public DateTime? CreatedDate { get; set; }
        public int CreatedBy { get; set; }
        public DateTime? TransactionDate { get; set; }
        public int TransactionOfficerId { get; set; }

    }
    public class MoneyAccountAoFeeTypeVm {
        public int AccountAoFeeTypeId { get; set; }
        public int AccountAoFundId { get; set; }
        public string FeeTypeName { get; set; }
        public decimal FeeTypeDefault { get; set; }
    }
    public class MoneyAccountAoFundVm {
        public int AccountAOFundId { get; set; }
        public string FundName { get; set; }
        public string FundDescription { get; set; }
        public int Inactive { get; set; }
        public int FundInmateOnlyFlag { get; set; }
        public int FundWelfareOnlyFlag { get; set; }
        public int FundBailOnlyFlag { get; set; }
        public int FundCommissaryOnlyFlag { get; set; }
        public string FundOptionOnly { get; set; }
        public int FundAllowTransfer { get; set; }
        public int FundAllowFee { get; set; }
        public int FundAllowFeeOrder { get; set; }
        public decimal FundInmateMinBalance { get; set; }
        public decimal BalanceFund { get; set; }
        public int FundBlindVerify { get; set; }
        public int FundAllowBypassDepository { get; set; }
        public int AccountAoBankId { get; set; }
        public decimal? BalanceFundFee { get; set; }
        public decimal? BalanceFundPending { get; set; }
        public List<MoneyAccountAoFeeTypeVm> FeeTypes { get; set; }
    }
    public class MoneyTransactionCalculateVm {
        public int InmateId { get; set; }
        public int BankId { get; set; }
        public int FundId { get; set; }
        public decimal TransactionAmount { get; set; }
        public MoneyTransactionType TransactionType { get; set; }
        public MoneyTransactionTable Table { get; set; }
        public DateTime? TransactionDate { get; set; }
    }
    public enum MoneyTransactionType {
        DEBIT = 1,
        CREDIT = 2
    }
    public enum MoneyTransactionTable {
        ACCOUNTAO_TRANSACTION = 1,
        ACCOUNTAO_RECEIVE = 2,
        ACCOUNTAO_FEE = 3,
        ACCOUNTAO_CASHDRAWER = 4
    }
    public class MoneyBalanceInmateVm {
        public InmateDetail Inmate { get; set; }
        public decimal? BalanceInmate { get; set; }
        public decimal? BalanceInmateFee { get; set; }
        public decimal? BalanceInmatePending { get; set; }
    }
    public class MoneyBalanceVm {
        public decimal InmateBalance { get; set; }
        public decimal FundBalance { get; set; }
        public decimal BankBalance { get; set; }
        public int InmateId { get; set; }
        public int BankId { get; set; }
        public int FundId { get; set; }
        public MoneyTransactionTable Table { get; set; }
        public int StatusCode { get; set; }
        public DateTime? TransactionDate { get; set; }
    }
    public class MoneyCashCheckVm : MoneyReceiptReportVm
    {
        public int InmateId { get; set; }
        public int FundId { get; set; }
        public int BankId { get; set; }
        public decimal TransAmount { get; set; }
        public string TransDescription { get; set; }
        public MoneyTransactionType TransType { get; set; }
        public int CashFlag { get; set; }
        public int? DepositId { get; set; }
        public int? cashBalanceId { get; set; }
        public string RexedFrom { get; set; }
        public string ReceiptNo { get; set; }
        public string ReceiveNo { get; set; }
        public string TransactionNotes { get; set; }
        public string PayToOrder { get; set; }
        public string DebitMemo { get; set; }
        public int TransFlag { get; set; }
        public int StatusCode { get; set; }
        public int ToFundId { get; set; }
        public IntakeCurrencyVm Currency { get; set; }
        public bool IsDepository { get; set; }
        public int? FeeTypeId { get; set; }
        public int TransactionNumber { get; set; }
        public decimal PartialPayAmount { get; set; }
        public decimal NewFee { get; set; }
        public string FundName { get; set; }
        public string CashBalanceName { get; set; }
        public string FacilityAbbr { get; set; }       
        public DateTime? CreateDate { get; set; }
        public PersonnelVm CreateBy {get;set;}
    }
    public class MoneyLedgerVm {
        public int AccountAoReceiveId { get; set; }
        public int AccountAoFeeId { get; set; }
        public int AccountAoTransactionId { get; set; }
        public DateTime? TransactionDate { get; set; }
        public decimal TransactionAmount { get; set; }
        public string TransactionDesc { get; set; }
        public string ReceiptNo { get; set; }
        public string CheckNo { get; set; }
        public int CashFlag { get; set; }
        public int VoidFlag { get; set; }
        public string TransNotes { get; set; }
        public int InmateId { get; set; }
        public decimal PayAmount { get; set; }
        public string FundName { get; set; }
    }
    public class MoneyTransactionVm {
        public List<KeyValuePair<string, int>> BankList { get; set; }
        public List<KeyValuePair<string, int>> DepositoryList { get; set; }
        public List<KeyValuePair<string, int>> CashDrawerList { get; set; }
        public List<KeyValuePair<string, int>> FundList { get; set; }
        public List<MoneyAccountAoFundVm> ToFundList { get; set; }
        public MoneyBalanceInmateVm InmateBalance { get; set; }
    }

    public class PrintInmateLedger {
        public int? SealedPersonId { get; set; }
        public int FacilityId { get; set; }
        public string FacilityAbbr { get; set; }
        public int? HousingUnitToId { get; set; }
        public int? StatusCountHousing { get; set; }
        public string PersonFirstName { get; set; }
        public string PersonLastName { get; set; }
        public string PersonMiddleName { get; set; }
        public string InmateNumber { get; set; }
        public PersonnelVm Officer { get; set; }
        public List<MoneyAccountTransactionVm> PrintInmateDetail { get; set; }

    }
    public class MoneyReceiptReportVm {
        public string PersonFullName { get; set; }
        public string HousingUnits { get; set; }
        public string ReportName { get; set; }
        public decimal? BalanceInmate { get; set; }
        public decimal? BalanceInmatePending { get; set; }
        public decimal? BalanceInmateFee { get; set; }
        public DateTime? TransactionDate { get; set; }
        public string TransactionType { get; set; }
        public bool VoidFlag { get; set; }
        public string VoidName { get; set; }
        public string FeeNumber { get; set; }
        public string TransactionReceiveCash { get; set; }
        public string fromFundName { get; set; }
        public string toFundName { get; set; }
        public int Type { get; set; }
        public string CashDrawerName { get; set; }
        public string TransactionPayToTheOrderOf { get; set; }
        public int CheckNo { get; set; }
        public DateTime? VoidDate { get; set; }
        public string CashOrReceiveNumber { get; set; }
    }
}