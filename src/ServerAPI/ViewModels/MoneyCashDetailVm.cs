﻿using System;
using System.Collections.Generic;

namespace ServerAPI.ViewModels {
    public class MoneyCashDetailVm {
        public int FacilityId { get; set; }
        public int DrawerId { get; set; }
        public int InmateId { get; set; }
        public int PersonnelId { get; set; }
        public string DrawerName { get; set; }
        public int Receipt { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; }
        public decimal Balance { get; set; }
        public int FromDrawerId { get; set; }
        public int ToDrawerId { get; set; }
        public PersonInfoVm Inmate { get; set; }
        public PersonnelVm Personnel { get; set; }
        public bool VoidFlag { get; set; }
        public int TransactionFromId { get; set; }
        public int TransactionToId { get; set; }
        public int BankId { get; set; }
        public string TransactionType { get; set; }
        public string BankName { get; set; }
        public string TransactionNotes { get; set; }
        public PersonnelVm VoidPersonnel { get; set; }
        public decimal? FromAmount { get; set; }
        public decimal? ToAmount { get; set; }
        public DateTime? VoidDate { get; set; }
    }

    public class MoneyCashBalanceVm {
        public int MoneyCashBalanceId { get; set; }
        public string CashDrawerName { get; set; }
        public bool IsVault { get; set; }
        public decimal BalanceAmount { get; set; }
        public decimal MinimumBalance { get; set; }
        public int FacilityId { get; set; }
        public string FacilityAbbr { get; set; }
    }

    public class MoneyCashDrawerDetailVm {
        public List<MoneyCashDetailVm> MoneyCashDetail { get; set; }
        public List<MoneyCashBalanceVm> CashDrawerDetail { get; set; }
    }
    public class MoneyBankVm {
        public int BankId { get; set; }
        public decimal Balance { get; set; }
        public string BankName { get; set; }
        public bool ReceiptShowPending { get; set; }
        public bool ReceiptShowOwed { get; set; }
        public bool ReceiptShowBalance { get; set; }
        public int? ReceiptCopies { get; set; }
        public int? ClearCheckAfterDays { get; set; }
    }

    public class MoneyAccountFundVm {
        public int AccountAoFundId { get; set; }
        public string FundName { get; set; }
        public decimal BalanceFund { get; set; }
        public decimal BalanceFundPending { get; set; }
        public decimal BalanceFundFee { get; set; }

    }
    public class MoneyAccountDetailVm {
        public MoneyBankVm MoneyBankDetail { get; set; }
        public List<MoneyCashBalanceVm> CashDrawerDetail { get; set; }
        public MoneyAccountFundVm AccountAoFundDetail { get; set; }
    }

    public class CashTransactionDetailVm {
        public int BankId { get; set; }
        public int FundId { get; set; }
        public int FacilityId { get; set; }
        public int DrawerId { get; set; }
        public int VaultId { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; }
        public string Notes { get; set; }
        public IntakeCurrencyVm Currency { get; set; }
    }
    public class MoneyCashInsertVm {
        public int VerifyId { get; set; }
        public int BankId { get; set; }
        public decimal CashCount { get; set; }
        public decimal CashBalance { get; set; }
        public int Cent1 { get; set; }
        public int Cent5 { get; set; }
        public int Cent10 { get; set; }
        public int Cent25 { get; set; }
        public int Cent50 { get; set; }
        public int Dollar1Coin { get; set; }
        public int Dollar1Bill { get; set; }
        public int Dollar2 { get; set; }
        public int Dollar5 { get; set; }
        public int Dollar10 { get; set; }
        public int Dollar20 { get; set; }
        public int Dollar50 { get; set; }
        public int Dollar100 { get; set; }
        public int Dollar500 { get; set; }
        public int Dollar1000 { get; set; }
        public int CashBalanceId { get; set; }
        public string CashBalanceName { get; set; }
        public DateTime? CreateDateTime { get; set; }
        public decimal? Difference { get; set; }
        public PersonnelVm Personnel { get; set; }
        public int FacilityId { get; set; }
    }

}