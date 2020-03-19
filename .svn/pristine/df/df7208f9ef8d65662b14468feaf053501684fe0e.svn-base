using System;
using ServerAPI.ViewModels;
using System.Linq;
using GenerateTables.Models;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading.Tasks;
using ServerAPI.Utilities;

namespace ServerAPI.Services
{
    public class MoneyClearService : IMoneyClearService
    {
        private readonly AAtims _context;
        private readonly int _personnelId;
        private readonly IMoneyService _moneyService;

        private readonly IMoneyBalanceDebitCheck _moneyBalanceDebitCheck;
        public MoneyClearService(AAtims context, IHttpContextAccessor httpContextAccessor, IMoneyService moneyService,
          IMoneyBalanceDebitCheck moneyBalanceDebitCheck)
        {
            _context = context;
            _personnelId = Convert.ToInt32(httpContextAccessor.HttpContext.User
                .FindFirst("personnelId")?.Value);
            _moneyService = moneyService;
            _moneyBalanceDebitCheck = moneyBalanceDebitCheck;
        }

        //To get the Bank & Fund dropdown details
        public MoneyClearVm GetDropDownDetails() =>
            new MoneyClearVm
            {
                LoadBankDetails = _moneyService.MoneyGetBank(), LoadFundDetails = _moneyService.MoneyGetFund()
            };

        //To get the Money-Clear-Clear-List Details
        public List<MoneyAccountTransactionVm> GetMoneyClearClearDetails(MoneyAccountFilterVm searchValue) =>
            _context.AccountAoTransaction
                .Where(w => w.AccountAoFund.AccountAoBankId == searchValue.BankId
                    && (searchValue.FundId == 0 || w.AccountAoFundId == searchValue.FundId)
                    && !w.TransactionCleared.HasValue
                    && !string.IsNullOrEmpty(w.TransactionCheckNumber)
                    && (string.IsNullOrEmpty(searchValue.TransactionCheckNumber)
                        ? w.TransactionDate.Value.Date >= searchValue.FromDate.Value.Date &&
                        w.TransactionDate.Value.Date <= searchValue.ToDate.Value.Date
                        : w.TransactionCheckNumber == searchValue.TransactionCheckNumber))
                .Select(s => new MoneyAccountTransactionVm
                {
                    AccountAoTransactionId = s.AccountAoTransactionId,
                    TransactionDate = s.TransactionDate,
                    TransactionDescription = s.TransactionDescription,
                    TransactionCheckNumber = s.TransactionCheckNumber,
                    TransactionReceipt = s.TransactionReceipt,
                    InmateId = s.InmateId ?? 0,
                    InmateNumber = s.Inmate.InmateNumber,
                    Person = s.InmateId > 0 ? new PersonVm
                    {
                        PersonLastName = s.Inmate.Person.PersonLastName,
                        PersonFirstName = s.Inmate.Person.PersonFirstName,
                        PersonMaidenName = s.Inmate.Person.PersonMaidenName,
                        PersonSuffix = s.Inmate.Person.PersonSuffix,
                    } : new PersonVm(),
                    HousingUnit = s.InmateId > 0 && s.Inmate.HousingUnitId > 0 ? new HousingDetail
                    {
                        HousingUnitId = s.Inmate.HousingUnit.HousingUnitId,
                        HousingUnitLocation = s.Inmate.HousingUnit.HousingUnitLocation,
                        HousingUnitNumber = s.Inmate.HousingUnit.HousingUnitNumber,
                        HousingUnitBedNumber = s.Inmate.HousingUnit.HousingUnitBedNumber,
                        HousingUnitBedLocation = s.Inmate.HousingUnit.HousingUnitBedLocation,
                    } : new HousingDetail(),
                    TransactionAmount = s.TransactionAmount,
                    VoidFlag = s.TransactionVoidFlag == 1,
                    BalanceInmate = s.BalanceInmate,
                    BalanceFund = s.BalanceFund,
                    BalanceAccount = s.BalanceAccount,
                    ClearBy = s.TransactionClearedBy > 0 ? new PersonnelVm
                    {
                        PersonFirstName = s.TransactionClearedByNavigation.PersonNavigation.PersonFirstName,
                        PersonLastName = s.TransactionClearedByNavigation.PersonNavigation.PersonLastName,
                        OfficerBadgeNumber = s.TransactionClearedByNavigation.OfficerBadgeNum
                    } : new PersonnelVm(),
                    VoidBy = s.TransactionVoidBy > 0 ? new PersonnelVm
                    {
                        PersonFirstName = s.TransactionVoidByNavigation.PersonNavigation.PersonFirstName,
                        PersonLastName = s.TransactionVoidByNavigation.PersonNavigation.PersonLastName,
                        OfficerBadgeNumber = s.TransactionVoidByNavigation.OfficerBadgeNum
                    } : new PersonnelVm(),
                    PersonnelBy = s.CreatedBy > 0 ? new PersonnelVm
                    {
                        PersonFirstName = s.CreatedByNavigation.PersonNavigation.PersonFirstName,
                        PersonLastName = s.CreatedByNavigation.PersonNavigation.PersonLastName,
                        OfficerBadgeNumber = s.CreatedByNavigation.OfficerBadgeNum
                    } : new PersonnelVm(),
                    AccountAoReceiveId = s.AccountAoReceiveId ?? 0,
                    ReceiveFrom = s.TransactionReceiveFrom,
                    DepositoryName = s.AccountAoReceive.AccountAoDepository.DepositoryName,
                    TransactionPayToTheOrderOf = s.TransactionPayToTheOrderOf,
                    ReceiveNumber = s.TransactionReceiveNumber,
                    TransactionDebitMemo = s.TransactionDebitMemo,
                    TransactionType = s.TransactionType,
                    ClearDate = s.TransactionClearedDate,
                    BankAccountAbbr = s.AccountAoFund.AccountAoBank.BankAccountAbbr,
                    TransactionNotes = s.TransactionNotes,
                    TransactionId = s.AccountAoTransactionId,
                    BankId = s.AccountAoFund.AccountAoBankId,
                    ReceiveCashFlag = s.TransactionReceiveCashFlag == 1,
                    FundName = s.AccountAoFund.FundName,
                    FacilityAbbr = s.Inmate.Facility.FacilityAbbr,
                    FundId = s.AccountAoFund.AccountAoFundId,
                    VoidDate = s.TransactionVoidDate,
                    ClearFlag = s.TransactionCleared == 1,
                    ReturnCashFlag = s.TransactionReturnCashFlag == 1,
                    TransactionFromId = s.AccountAoTransactionFromId ?? 0,
                    TransactionToId = s.AccountAoTransactionToId ?? 0,
                }).OrderByDescending(o => o.AccountAoTransactionId).ToList();

        //To get the Money-Clear-History-List Details
        public List<MoneyAccountTransactionVm> GetMoneyClearHistoryDetails(MoneyAccountFilterVm searchValue) =>
            _context.AccountAoTransaction
                .Where(w => w.AccountAoFund.AccountAoBankId == searchValue.BankId
                    && (searchValue.FundId == 0 || w.AccountAoFundId == searchValue.FundId) &&
                    w.TransactionCleared == 1
                    && !string.IsNullOrEmpty(w.TransactionCheckNumber) &&
                    w.TransactionDate.Value.Date >= searchValue.FromDate.Value.Date &&
                    w.TransactionDate.Value.Date <= searchValue.ToDate.Value.Date)
                .Select(s => new MoneyAccountTransactionVm
                {
                    AccountAoTransactionId = s.AccountAoTransactionId,
                    TransactionDate = s.TransactionDate,
                    TransactionDescription = s.TransactionDescription,
                    TransactionCheckNumber = s.TransactionCheckNumber,
                    InmateNumber = s.Inmate.InmateNumber,
                    Person = s.InmateId.HasValue ? new PersonVm
                    {
                        PersonId = s.Inmate.Person.PersonId,
                        PersonLastName = s.Inmate.Person.PersonLastName,
                        PersonFirstName = s.Inmate.Person.PersonFirstName,
                        PersonMiddleName = s.Inmate.Person.PersonMiddleName,
                        PersonSuffix = s.Inmate.Person.PersonSuffix,
                    } : new PersonVm(),
                    HousingUnit = s.Inmate.HousingUnitId > 0 ? new HousingDetail
                    {
                        HousingUnitId = s.Inmate.HousingUnit.HousingUnitId,
                        HousingUnitLocation = s.Inmate.HousingUnit.HousingUnitLocation,
                        HousingUnitNumber = s.Inmate.HousingUnit.HousingUnitNumber,
                        HousingUnitBedNumber = s.Inmate.HousingUnit.HousingUnitBedNumber,
                        HousingUnitBedLocation = s.Inmate.HousingUnit.HousingUnitBedLocation,
                    } : new HousingDetail(),
                    TransactionAmount = s.TransactionAmount,
                    ReturnCashFlag = s.TransactionReturnCheckFlag == 1,
                    ClearFlag = s.TransactionCleared == 1,
                    ClearDate = s.TransactionClearedDate,
                    VoidFlag = s.TransactionVoidFlag == 1,
                    BalanceInmate = s.BalanceInmate,
                    BalanceFund = s.BalanceFund,
                    BalanceAccount = s.BalanceAccount,
                    FundName = s.AccountAoFund.FundName,
                    FacilityAbbr = s.Inmate.Facility.FacilityAbbr,
                    FundId = s.AccountAoFund.AccountAoFundId,
                    BankId = s.AccountAoFund.AccountAoBankId,
                    DepositoryName = s.AccountAoReceive.AccountAoDepository.DepositoryName,
                    TransactionType = s.TransactionType,
                    BankAccountAbbr = s.AccountAoFund.AccountAoBank.BankAccountAbbr,
                    TransactionNotes = s.TransactionNotes,
                    VoidDate = s.TransactionVoidDate,
                    AccountAoFeeId = s.AccountAoFeeId ?? 0,
                    ReceiveFrom = s.TransactionReceiveFrom,
                    TransactionPayToTheOrderOf = s.TransactionPayToTheOrderOf,
                    ReceiveNumber = s.TransactionReceiveNumber,
                    TransactionDebitMemo = s.TransactionDebitMemo,
                    TransactionFromId = s.AccountAoTransactionFromId ?? 0,
                    TransactionToId = s.AccountAoTransactionToId ?? 0,
                    ClearBy = s.TransactionClearedBy > 0 ?
                        new PersonnelVm
                        {
                            PersonFirstName = s.TransactionClearedByNavigation.PersonNavigation.PersonFirstName,
                            PersonLastName = s.TransactionClearedByNavigation.PersonNavigation.PersonLastName,
                            OfficerBadgeNumber = s.TransactionClearedByNavigation.OfficerBadgeNum
                        } : new PersonnelVm(),
                    VoidBy = s.TransactionVoidBy > 0 ? new PersonnelVm
                    {
                        PersonFirstName = s.TransactionVoidByNavigation.PersonNavigation.PersonFirstName,
                        PersonLastName = s.TransactionVoidByNavigation.PersonNavigation.PersonLastName,
                        OfficerBadgeNumber = s.TransactionVoidByNavigation.OfficerBadgeNum
                    } : new PersonnelVm(),
                    PersonnelBy = s.CreatedBy > 0 ? new PersonnelVm
                    {
                        PersonFirstName = s.CreatedByNavigation.PersonNavigation.PersonFirstName,
                        PersonLastName = s.CreatedByNavigation.PersonNavigation.PersonLastName,
                        OfficerBadgeNumber = s.CreatedByNavigation.OfficerBadgeNum
                    } : new PersonnelVm(),
                }).OrderByDescending(o => o.AccountAoTransactionId).ToList();

        //To do the DoTransaction in Money-Clear-Clear-List Details
        public async Task<int> UpdateMoneyClearClearTransaction(List<int> checkedTransIdList)
        {
            if (checkedTransIdList.Count == 0) return -1;
            List<AccountAoTransaction> accountAoTransaction = _context.AccountAoTransaction
                .Where(w => checkedTransIdList.Contains(w.AccountAoTransactionId)
                    && (!w.TransactionCleared.HasValue || w.TransactionCleared == 0)).ToList();

            //Update the AccountAOTransaction table
            accountAoTransaction.ForEach(f =>
            {
                f.TransactionCleared = 1;
                f.TransactionClearedBy = _personnelId;
                f.TransactionClearedDate = DateTime.Now;
                f.UpdateDate = DateTime.Now;
                f.UpdatedBy = _personnelId;
            });
            return await _context.SaveChangesAsync(); ;
        }

        public async Task<int> InsertReturnCheck(List<int> value)
        {
            if (value.Count == 0) return -1;
            List<AccountAoTransaction> aoTransactions = _context.AccountAoTransaction
                .Where(w => value.Contains(w.AccountAoTransactionId) && (w.TransactionCleared == 0
                    || !w.TransactionCleared.HasValue)).ToList();
            aoTransactions.ForEach(item =>
            {
                int BankId = _context.AccountAoFund.Single(s => s.AccountAoFundId == item.AccountAoFundId).AccountAoBankId;
                AccountAoTransaction accountAoTransaction = _context.AccountAoTransaction.Find(item.AccountAoTransactionId);
                accountAoTransaction.TransactionCleared = 1;
                accountAoTransaction.TransactionClearedBy = _personnelId;
                accountAoTransaction.TransactionClearedDate = DateTime.Now;
                accountAoTransaction.TransactionReturnCheckFlag = 1;
                accountAoTransaction.UpdateDate = DateTime.Now;
                accountAoTransaction.UpdatedBy = _personnelId;
                _context.SaveChanges();
                int welfareFundId = _context.AccountAoFund
                    .Single(s => s.FundWelfareOnlyFlag == 1 && s.AccountAoBankId == BankId).AccountAoFundId;
                MoneyTransactionCalculateVm balanceParam = new MoneyTransactionCalculateVm
                {
                    InmateId = item.InmateId ?? 0,
                    BankId = BankId,
                    FundId = welfareFundId,
                    TransactionAmount = item.TransactionAmount,
                    TransactionType = MoneyTransactionType.CREDIT,
                    Table = MoneyTransactionTable.ACCOUNTAO_TRANSACTION
                };
                MoneyBalanceVm balanceDetails = _moneyBalanceDebitCheck.MoneyTransactionCalculateBalance(balanceParam);
                balanceDetails.InmateId = 0;
                balanceDetails.BankId = BankId;
                balanceDetails.FundId = welfareFundId;
                balanceDetails.Table = MoneyTransactionTable.ACCOUNTAO_TRANSACTION;
                _moneyBalanceDebitCheck.MoneyUpdateBalance(balanceDetails);
                AccountAoTransaction aoTransaction = new AccountAoTransaction
                {
                    AccountAoFundId = welfareFundId,
                    InmateId = item.InmateId == 0 ? default : item.InmateId,
                    BalanceAccount = balanceDetails.BankBalance,
                    BalanceFund = balanceDetails.FundBalance,
                    BalanceInmate = 0,
                    TransactionAmount = -1 * Math.Abs(item.TransactionAmount),
                    TransactionType = MoneyTransactionType.CREDIT.ToString(),
                    TransactionDescription = MoneyConstants.RETURNCHECK,
                    CreatedBy = _personnelId,
                    CreateDate = DateTime.Now,
                    TransactionDate = DateTime.Now,
                    TransactionOfficerId = _personnelId,
                    UpdatedBy = _personnelId,
                    UpdateDate = DateTime.Now,
                    TransactionNotes = item.TransactionNotes,
                    AccountAoTransactionFromId = item.AccountAoTransactionId
                };
                _context.AccountAoTransaction.Add(aoTransaction);
                _context.SaveChanges();
                int toId = aoTransaction.AccountAoTransactionId;
                AccountAoTransaction updateObjAoTrans = _context.AccountAoTransaction.Find(item.AccountAoTransactionId);
                updateObjAoTrans.AccountAoTransactionToId = toId;
            });
            return await _context.SaveChangesAsync();
        }
    }
}