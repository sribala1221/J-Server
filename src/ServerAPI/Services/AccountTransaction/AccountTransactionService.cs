using System;
using GenerateTables.Models;
using Microsoft.AspNetCore.Http;
using ServerAPI.ViewModels;
using System.Collections.Generic;
using System.Linq;

namespace ServerAPI.Services
{
    public class AccountTransactionService : IAccountTransactionService
    {
        private readonly AAtims _context;
        private readonly int _personnelId;
        public AccountTransactionService(AAtims context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _personnelId = Convert.ToInt32(httpContextAccessor.HttpContext.User
            .FindFirst("personnelId")?.Value);
        }
        public List<AccountTransactionVm> GetAccountTransactionList(AccountTransactionVm accTransaction)
        {
            List<AccountTransactionVm> AccountTransaction = _context.AccountAoTransaction
            .Where(s => (accTransaction.BankId <= 0
            || s.AccountAoFund.AccountAoBankId == accTransaction.BankId) &&
             (accTransaction.FundId <= 0 || s.AccountAoFundId == accTransaction.FundId) &&
             (accTransaction.InmateId == 0 || s.InmateId == accTransaction.InmateId) &&
             (string.IsNullOrEmpty(accTransaction.TransactionType)
             || s.TransactionType == accTransaction.TransactionType) &&
             (string.IsNullOrEmpty(accTransaction.TransactionReceiveNumber)
             || s.TransactionReceiveNumber == accTransaction.TransactionReceiveNumber) &&
             (string.IsNullOrEmpty(accTransaction.TransactionCheckNumber)
             || s.TransactionCheckNumber == accTransaction.TransactionCheckNumber) &&
             ((accTransaction.TransactionFromId == 0)
              || s.AccountAoTransactionFromId == accTransaction.TransactionFromId) &&
             ((accTransaction.TransactionToId == 0)
             || s.AccountAoTransactionToId == accTransaction.TransactionToId) &&
             (!accTransaction.TransactionFromDate.HasValue
             || (s.TransactionDate >= accTransaction.TransactionFromDate
             && s.TransactionDate <= accTransaction.TransactionToDate)))
            .Select(s => new AccountTransactionVm
            {
                AccountTransactionId = s.AccountAoTransactionId,
                TransactionDate = s.TransactionDate,
                TransactionAmount = s.TransactionAmount,
                TransactionDescription = s.TransactionDescription,
                TransactionReceipt = s.TransactionReceipt,
                TransactionCheckNumber = s.TransactionCheckNumber,
                TransactionBalanceInmate = s.BalanceInmate,
                TransactionBalanceAccount = s.BalanceAccount,
                TransactionBalanceFund = s.BalanceFund,
                InmateId = s.Inmate.InmateId,
                InmateNumber = s.Inmate.InmateNumber,
                Person = new PersonVm
                {
                    PersonFirstName = s.Inmate.Person.PersonFirstName,
                    PersonMiddleName = s.Inmate.Person.PersonMiddleName,
                    PersonLastName = s.Inmate.Person.PersonLastName,
                },
                VoidFlag = s.TransactionVoidFlag == 1,
            }).OrderByDescending(a => a.AccountTransactionId).ToList();
            return AccountTransaction;
        }

        public List<MoneyInmateLedgerVm> GetMoneyLedgerList(MoneyInmateLedgerVm ledger)
        {
        //     w.TransactionVerified == 0 &&
        //   w.TransactionVoidFlag == 0 && w.AccountAoFund.FundInmateOnlyFlag == 1 &&  w.AccountAoFund.AccountAoBankId == ledger.BankId &&
            List<MoneyInmateLedgerVm> ledgerList = _context.AccountAoReceive.Where(w =>
            w.InmateId == ledger.InmateId
         )
            .Select(s => new MoneyInmateLedgerVm
            {
                Sample = "Pending :",
                InmateId = s.InmateId
            })
            // .Union(_context.AccountAoFee.Where(w => w.InmateId == ledger.InmateId &&
            //  w.AccountAoFund.FundInmateOnlyFlag == 1 && w.TransactionCleared == 0 &&
            //  w.TransactionVoidFlag == 0 && w.AccountAoFund.AccountAoBankId == ledger.BankId)
            // .Select(s => new MoneyInmateLedgerVm
            // {
            //     Sample = "Unpaid Fee :",
            //     InmateId = s.InmateId

            // })).Union(_context.AccountAoTransaction.Where(w => w.AccountAoFund.FundInmateOnlyFlag == 1 &&
            // w.InmateId == ledger.InmateId && w.AccountAoFund.AccountAoBankId == ledger.BankId)
            // .Select(s => new MoneyInmateLedgerVm
            // {
            //     Sample = "Inmate Ledger :",
            //     InmateId = s.InmateId
            // }))
            .ToList();
            int?[] accountAOReceiveInmateIds = ledgerList.Select(s => s.InmateId).ToArray();
            List<MoneyInmateLedgerVm> inmateList = _context.AccountAoInmate
            .Where(w => accountAOReceiveInmateIds.Contains(w.InmateId)).Select(s => new MoneyInmateLedgerVm
            {
                InmateId = s.InmateId,
                BankId = s.AccountAoBankId,
                PendingBalance = s.BalanceInmatePending,
                UnPaidBalance = s.BalanceInmateFee,
                LedgerBalance = s.BalanceInmate,


            }).ToList();
            ledgerList.ForEach(f =>
            {
                MoneyInmateLedgerVm bank = inmateList.SingleOrDefault(w => w.InmateId == f.InmateId);
                if (bank != null)
                {
                    f.BankId = bank.BankId;
                    f.PendingBalance = bank.PendingBalance;
                    f.UnPaidBalance = bank.UnPaidBalance;
                    f.LedgerBalance = bank.LedgerBalance;
                }
            });
            ledgerList.Where(w => w.BankId == ledger.BankId);
            return ledgerList;
        }
    }
}
