﻿using System;
using System.Collections.Generic;
using ServerAPI.ViewModels;

// ReSharper disable once CheckNamespace
namespace ServerAPI.Services
{
    public interface IMoneyCashService
    {
        List<MoneyCashBalanceVm> MoneyGetCashDrawer(int facilityId);
        MoneyCashDrawerDetailVm GetCashDetails(MoneyCashDetailVm cashDetail);
        MoneyAccountDetailVm GetAccountDetail(int facilityId);
        int DepositTransaction(CashTransactionDetailVm detail);
        int WithdrawTransaction(CashTransactionDetailVm detail);
        int BankToCashTransaction(CashTransactionDetailVm detail);
        int CashToBankTransaction(CashTransactionDetailVm detail);
        int VaultToCashTransaction(CashTransactionDetailVm detail);
        int CashToVaultTransaction(CashTransactionDetailVm detail);
        int CashToCashTransaction(CashTransactionDetailVm detail);
        int Journal(CashTransactionDetailVm detail);
        int MoneyCashInsert(MoneyCashInsertVm param);
        List<MoneyCashInsertVm> MoneyCashVerify(int bankId, int cashBalanceId,DateTime? fromDate, DateTime? toDate);
        decimal MoneyCashBalance(int cashBalanceId);
        MoneyAccountTransactionVm GetTransactionDetail(int transId, bool voidFlag);
        MoneyCashDrawerDetailVm GetCashSearchDetails(MoneyCashDetailVm searchValue);
    }
}
