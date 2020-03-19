using System;
using System.Collections.Generic;
using System.Linq;
using GenerateTables.Models;
using ServerAPI.ViewModels;

namespace ServerAPI.Services
{
    public class MoneyTransactionService : IMoneyTransactionService
    {
        private readonly AAtims _context;

        public MoneyTransactionService(AAtims context)
        {
            _context = context;
        }
        #region Get Money Transaction Details
        // Money -> Transaction
        public List<MoneyAccountTransactionVm> GetMoneyTransactionDetails(MoneyAccountTransactionVm searchValue)
        {
            List<MoneyAccountTransactionVm> moneyTransaction = _context.AccountAoTransaction
            .Where(w => w.AccountAoFund.AccountAoBankId == searchValue.BankId
            && (searchValue.FundId == 0 || w.AccountAoFundId == searchValue.FundId)
            && (searchValue.InmateId == 0 || w.InmateId == searchValue.InmateId)
            && (!searchValue.FromDate.HasValue || w.TransactionDate.HasValue &&
            w.TransactionDate.Value.Date >= searchValue.FromDate.Value.Date) &&
            (!searchValue.ToDate.HasValue || w.TransactionDate.HasValue &&
             w.TransactionDate.Value.Date <= searchValue.ToDate.Value.Date) &&
            (string.IsNullOrEmpty(searchValue.TransactionType) ||
            (w.TransactionType == searchValue.TransactionType))
            && (string.IsNullOrEmpty(searchValue.TransactionReceipt) ||
            w.TransactionReceipt == searchValue.TransactionReceipt) &&
            (string.IsNullOrEmpty(searchValue.TransactionCheckNumber) ||
            w.TransactionCheckNumber == searchValue.TransactionCheckNumber)
            && (searchValue.TransactionId == 0 ||
            w.AccountAoTransactionFromId == searchValue.TransactionId ||
            w.AccountAoTransactionToId == searchValue.TransactionId))
            .Select(s => new MoneyAccountTransactionVm
            {
                AccountAoTransactionId = s.AccountAoTransactionId,
                TransactionDate = s.TransactionDate,
                TransactionAmount = s.TransactionAmount,
                TransactionDescription = s.TransactionDescription,
                TransactionReceipt = s.TransactionReceipt,
                TransactionCheckNumber = s.TransactionCheckNumber,
                TransactionId = s.AccountAoTransactionId,
                FacilityAbbr = s.InmateId > 0 ? s.Inmate.Facility.FacilityAbbr : default,
                BalanceInmate = s.BalanceInmate,
                BalanceAccount = s.BalanceAccount,
                BalanceFund = s.BalanceFund,
                VoidFlag = s.TransactionVoidFlag == 1,
                InmateId = s.InmateId > 0 ? s.Inmate.InmateId : default,
                InmateNumber = s.InmateId > 0 ? s.Inmate.InmateNumber : default,
                FundName = s.AccountAoFund.FundName,
                BankId = s.AccountAoFund.AccountAoBankId,
                FundId = s.AccountAoFundId,
                ReceiveCashFlag = s.TransactionReceiveCashFlag == 1,
                TransactionType = s.TransactionType,
                BankAccountAbbr = s.AccountAoFund.AccountAoBank.BankAccountAbbr,
                ReceiveNumber = s.TransactionReceiveNumber,
                ReceiveFrom = s.TransactionReceiveFrom,
                TransactionNotes = s.TransactionNotes,
                VoidDate = s.TransactionVoidDate,
                TransactionDebitMemo = s.TransactionDebitMemo,
                TransactionFromId = s.AccountAoTransactionFromId ?? 0,
                TransactionToId = s.AccountAoTransactionToId ?? 0,
                Person = s.InmateId > 0 ? new PersonVm
                {
                    PersonLastName = s.Inmate.Person.PersonLastName,
                    PersonFirstName = s.Inmate.Person.PersonFirstName,
                    PersonMiddleName = s.Inmate.Person.PersonMiddleName,
                    PersonSuffix = s.Inmate.Person.PersonSuffix,
                    PersonId = s.Inmate.PersonId,
                    FacilityId = s.Inmate.FacilityId
                } : default,
                HousingUnit = s.InmateId > 0 && s.Inmate.HousingUnitId > 0 ? new HousingDetail
                {
                    HousingUnitId = s.Inmate.HousingUnit.HousingUnitId,
                    HousingUnitLocation = s.Inmate.HousingUnit.HousingUnitLocation,
                    HousingUnitNumber = s.Inmate.HousingUnit.HousingUnitNumber,
                    HousingUnitBedNumber = s.Inmate.HousingUnit.HousingUnitBedNumber,
                    HousingUnitBedLocation = s.Inmate.HousingUnit.HousingUnitBedLocation
                } : default,

                VoidBy = s.TransactionVoidBy > 0 ? new PersonnelVm
                {
                    PersonFirstName = s.TransactionVoidByNavigation.PersonNavigation.PersonFirstName,
                    PersonLastName = s.TransactionVoidByNavigation.PersonNavigation.PersonLastName,
                    OfficerBadgeNumber = s.TransactionVoidByNavigation.OfficerBadgeNum
                } : default

            }).OrderByDescending(o => o.AccountAoTransactionId).ToList();
            return moneyTransaction;
        }
        #endregion
    }
}