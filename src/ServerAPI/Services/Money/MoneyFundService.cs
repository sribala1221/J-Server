
using System.Collections.Generic;
using System.Linq;
using GenerateTables.Models;
using ServerAPI.ViewModels;
using System;

// ReSharper disable once CheckNamespace
namespace ServerAPI.Services
{
    public class MoneyFundService : IMoneyFundService
    {
        private readonly AAtims _context;
        private readonly IMoneyInmateService _moneyInmateService;
        private readonly IMoneyService _moneyService;

        public MoneyFundService(AAtims context, IMoneyInmateService moneyInmateService, IMoneyService moneyService)
        {
            _context = context;
            _moneyInmateService = moneyInmateService;
            _moneyService = moneyService;
        }

        public MoneyFundVm GetMoneyFundTransactionDetails(MoneyFund searchValue)
        {             
            MoneyFundVm moneyFundLedger = new MoneyFundVm();
            if (!searchValue.IsSearch)
            {
                moneyFundLedger.bankList = _moneyService.MoneyGetBank();
                moneyFundLedger.fundList = _moneyService.MoneyGetFund();
                if (searchValue.BankId == 0 && searchValue.FundId == 0)
                {
                    searchValue.BankId = moneyFundLedger.bankList[0].Value;
                    searchValue.FundId = moneyFundLedger.fundList[0].AccountAOFundId;
                }
            }
            moneyFundLedger.PendingDetails = _context.AccountAoReceive
            .Where(x => (x.AccountAoFund.AccountAoBankId == searchValue.BankId) &&
           (x.AccountAoFundId == searchValue.FundId) &&
           (x.TransactionDate.Value.Date >= searchValue.FromDate.Value.Date &&
           x.TransactionDate.Value.Date <= searchValue.ToDate.Value.Date) &&
           (x.TransactionVerified == 0 || !x.TransactionVerified.HasValue)).Select(s =>
          new MoneyAccountTransactionVm
          {
              TransactionId = s.AccountAoReceiveId,
              AccountAoReceiveId = s.AccountAoReceiveId,
              TransactionDate = s.TransactionDate,
              TransactionAmount = s.TransactionAmount ?? 0,
              TransactionDescription = s.TransactionDescription,
              TransactionReceipt = s.TransactionReceipt,
              TransactionReceiveNumber  =s.TransactionReceiveNumber,
              InmateId = s.InmateId ?? 0,
              InmateNumber = s.Inmate.InmateNumber,
              Person = s.InmateId > 0 ? new PersonVm
              {
                  PersonLastName = s.Inmate.Person.PersonLastName,
                  PersonFirstName = s.Inmate.Person.PersonFirstName,
                  PersonMiddleName = s.Inmate.Person.PersonMiddleName,
                  PersonSuffix = s.Inmate.Person.PersonSuffix,
                  PersonId = s.Inmate.PersonId,
                  FacilityId = s.Inmate.FacilityId,

              } : new PersonVm(),
              HousingUnit = s.InmateId > 0 && s.Inmate.HousingUnitId > 0 ? new HousingDetail
              {
                  HousingUnitId = s.Inmate.HousingUnit.HousingUnitId,
                  HousingUnitLocation = s.Inmate.HousingUnit.HousingUnitLocation,
                  HousingUnitNumber = s.Inmate.HousingUnit.HousingUnitNumber,
                  HousingUnitBedNumber = s.Inmate.HousingUnit.HousingUnitBedNumber,
                  HousingUnitBedLocation = s.Inmate.HousingUnit.HousingUnitBedLocation,
              } : new HousingDetail(),
              FundName = s.AccountAoFund.FundName,
              FacilityAbbr = s.Inmate.Facility.FacilityAbbr,
              FundId = s.AccountAoFund.AccountAoFundId,
              BankId = s.AccountAoFund.AccountAoBankId,
              TransactionType = s.TransactionType,
              BankAccountAbbr = s.AccountAoFund.AccountAoBank.BankAccountAbbr,
              DepositoryName = s.AccountAoDepository.DepositoryName,
              // PersonnelBy = s.CreatedByNavigation.PersonNavigation,
              PersonnelBy = new PersonnelVm
              {
                  PersonFirstName = s.CreatedByNavigation.PersonNavigation.PersonFirstName,
                  PersonLastName = s.CreatedByNavigation.PersonNavigation.PersonLastName,
                  OfficerBadgeNumber = s.CreatedByNavigation.OfficerBadgeNum
              },
              VoidFlag = s.TransactionVoidFlag == 1,
              ReceiveFrom = s.TransactionReceiveFrom,
              BalanceInmate = s.BalanceInmatePending,
              AccountAoDepositoryId = s.AccountAoDepositoryId,
              TransactionNotes = s.TransactionNotes,
              TransactionReceiveCashFlag = s.TransactionReceiveCashFlag == 1,
              ClearBy = new PersonnelVm
              {
                  PersonFirstName = s.TransactionVerifiedByNavigation.PersonNavigation.PersonFirstName,
                  PersonLastName = s.TransactionVerifiedByNavigation.PersonNavigation.PersonLastName,
                  OfficerBadgeNumber = s.TransactionVerifiedByNavigation.OfficerBadgeNum
              },
              VoidBy = new PersonnelVm
              {
                  PersonFirstName = s.TransactionVoidByNavigation.PersonNavigation.PersonFirstName,
                  PersonLastName = s.TransactionVoidByNavigation.PersonNavigation.PersonLastName,
                  OfficerBadgeNumber = s.TransactionVoidByNavigation.OfficerBadgeNum
              }
          }).OrderByDescending(o => o.AccountAoReceiveId).ToList();

            moneyFundLedger.UnpaidDetails = _context.AccountAoFee.Where(x =>
            (x.AccountAoFund.AccountAoBankId == searchValue.BankId) &&
            (x.AccountAoFundId == searchValue.FundId) &&
            (x.TransactionDate.Value.Date >= searchValue.FromDate.Value.Date &&
            x.TransactionDate.Value.Date <= searchValue.ToDate.Value.Date) &&
            (x.TransactionCleared == 0 || !x.TransactionCleared.HasValue))
             .Select(s => new MoneyAccountTransactionVm
             {
                 TransactionId = s.AccountAoFeeId,
                 AccountAoFeeId = s.AccountAoFeeId,
                 TransactionDate = s.TransactionDate,
                 TransactionAmount = s.TransactionFee ?? 0,
                 TransactionDescription = s.TransactionDescription,
                 InmateId = s.InmateId ?? 0,
                 InmateNumber = s.Inmate.InmateNumber,
                 Person = s.InmateId > 0 ? new PersonVm
                 {
                     PersonLastName = s.Inmate.Person.PersonLastName,
                     PersonFirstName = s.Inmate.Person.PersonFirstName,
                     PersonMiddleName = s.Inmate.Person.PersonMiddleName,
                     PersonSuffix = s.Inmate.Person.PersonSuffix,
                     PersonId = s.Inmate.PersonId,
                     FacilityId = s.Inmate.FacilityId,
                 } : new PersonVm(),
                 HousingUnit = s.InmateId > 0 && s.Inmate.HousingUnitId > 0 ? new HousingDetail
                 {
                     HousingUnitId = s.Inmate.HousingUnit.HousingUnitId,
                     HousingUnitLocation = s.Inmate.HousingUnit.HousingUnitLocation,
                     HousingUnitNumber = s.Inmate.HousingUnit.HousingUnitNumber,
                     HousingUnitBedNumber = s.Inmate.HousingUnit.HousingUnitBedNumber,
                     HousingUnitBedLocation = s.Inmate.HousingUnit.HousingUnitBedLocation,
                 } : new HousingDetail(),
                 FundName = s.AccountAoFund.FundName,

                 FundId = s.AccountAoFund.AccountAoFundId,
                 BankId = s.AccountAoFund.AccountAoBankId,
                 FacilityAbbr = s.Inmate.Facility.FacilityAbbr,
                 TransactionNotes = s.TransactionNotes,
                 TransactionType = s.TransactionType,
                 PersonnelBy = new PersonnelVm
                 {
                     PersonFirstName = s.CreatedByNavigation.PersonNavigation.PersonFirstName,
                     PersonLastName = s.CreatedByNavigation.PersonNavigation.PersonLastName,
                     OfficerBadgeNumber = s.CreatedByNavigation.OfficerBadgeNum
                 },
                 ClearBy = new PersonnelVm
                 {
                     PersonFirstName = s.TransactionClearedByNavigation.PersonNavigation.PersonFirstName,
                     PersonLastName = s.TransactionClearedByNavigation.PersonNavigation.PersonLastName,
                     OfficerBadgeNumber = s.TransactionClearedByNavigation.OfficerBadgeNum
                 },
                 VoidBy = new PersonnelVm
                 {
                     PersonFirstName = s.TransactionVoidByNavigation.PersonNavigation.PersonFirstName,
                     PersonLastName = s.TransactionVoidByNavigation.PersonNavigation.PersonLastName,
                     OfficerBadgeNumber = s.TransactionVoidByNavigation.OfficerBadgeNum
                 },
                 // PersonnelBy = s.CreatedByNavigation.PersonNavigation,
                 VoidFlag = s.TransactionVoidFlag == 1
             }).OrderByDescending(o => o.AccountAoFeeId).ToList();

            moneyFundLedger.FundLedgerDetails = _context.AccountAoTransaction.Where(x =>
            (x.AccountAoFund.AccountAoBankId == searchValue.BankId) &&
            (x.AccountAoFund.AccountAoFundId == searchValue.FundId) &&
            (x.TransactionDate.Value.Date >= searchValue.FromDate.Value.Date &&
            x.TransactionDate.Value.Date <= searchValue.ToDate.Value.Date) &&
            (x.TransactionCleared == 0 || !x.TransactionCleared.HasValue)).Select(s => new MoneyAccountTransactionVm
            {
                TransactionId = s.AccountAoTransactionId,
                AccountAoReceiveId = s.AccountAoReceiveId ?? 0,
                BalanceFund = s.BalanceFund,
                TransactionCheckNumber = s.TransactionCheckNumber,
                TransactionReceiveCashFlag = s.TransactionReceiveCashFlag == 1,
                TransactionDate = s.TransactionDate,
                TransactionAmount = s.TransactionAmount,
                TransactionDescription = s.TransactionDescription,
                TransactionReceipt = s.TransactionReceipt,
                InmateId = s.InmateId ?? 0,
                InmateNumber = s.Inmate.InmateNumber,
                Person = s.InmateId > 0 ? new PersonVm
                {
                    PersonLastName = s.Inmate.Person.PersonLastName,
                    PersonFirstName = s.Inmate.Person.PersonFirstName,
                    PersonMiddleName = s.Inmate.Person.PersonMiddleName,
                    PersonSuffix = s.Inmate.Person.PersonSuffix,
                    PersonId = s.Inmate.PersonId,
                    FacilityId = s.Inmate.FacilityId,

                } : new PersonVm(),
                HousingUnit = s.InmateId > 0 && s.Inmate.HousingUnitId > 0 ? new HousingDetail
                {
                    HousingUnitId = s.Inmate.HousingUnit.HousingUnitId,
                    HousingUnitLocation = s.Inmate.HousingUnit.HousingUnitLocation,
                    HousingUnitNumber = s.Inmate.HousingUnit.HousingUnitNumber,
                    HousingUnitBedNumber = s.Inmate.HousingUnit.HousingUnitBedNumber,
                    HousingUnitBedLocation = s.Inmate.HousingUnit.HousingUnitBedLocation,
                } : new HousingDetail(),
                FundName = s.AccountAoFund.FundName,
                FacilityAbbr = s.Inmate.Facility.FacilityAbbr,
                FundId = s.AccountAoFund.AccountAoFundId,
                BankId = s.AccountAoFund.AccountAoBankId,
                AccountAoTransactionId = s.AccountAoTransactionId,
                TransactionReceiveNumber = s.TransactionReceiveNumber,
                TransactionDebitMemo = s.TransactionDebitMemo,
                DepositoryName = s.AccountAoReceive.AccountAoDepository.DepositoryName,
                TransactionFromId = s.AccountAoTransactionFromId ?? 0,
                TransactionToId = s.AccountAoTransactionToId ?? 0,
                TransactionPayToTheOrderOf = s.TransactionPayToTheOrderOf,
                TransactionNotes = s.TransactionNotes,
                TransactionType = s.TransactionType,
                PersonnelBy = new PersonnelVm
                {
                    PersonFirstName = s.CreatedByNavigation.PersonNavigation.PersonFirstName,
                    PersonLastName = s.CreatedByNavigation.PersonNavigation.PersonLastName,
                    OfficerBadgeNumber = s.CreatedByNavigation.OfficerBadgeNum
                },
                ClearBy = new PersonnelVm
                {
                    PersonFirstName = s.TransactionClearedByNavigation.PersonNavigation.PersonFirstName,
                    PersonLastName = s.TransactionClearedByNavigation.PersonNavigation.PersonLastName,
                    OfficerBadgeNumber = s.TransactionClearedByNavigation.OfficerBadgeNum
                },
                VoidBy = new PersonnelVm
                {
                    PersonFirstName = s.TransactionVoidByNavigation.PersonNavigation.PersonFirstName,
                    PersonLastName = s.TransactionVoidByNavigation.PersonNavigation.PersonLastName,
                    OfficerBadgeNumber = s.TransactionVoidByNavigation.OfficerBadgeNum

                },
                //  PersonnelBy = s.CreatedByNavigation.PersonNavigation,
                VoidFlag = s.TransactionVoidFlag == 1,
                ReceiveFrom = s.TransactionReceiveFrom
            }).OrderByDescending(o => o.AccountAoTransactionId).ToList();

            moneyFundLedger.MoneyFundDetails = GetMoneyFundDetails(searchValue);
            moneyFundLedger.FundLedgerDetails.ForEach(
                item =>
                {
                    if (!item.ReturnCashFlag && (item.TransactionToId > 0 || item.TransactionFromId > 0))
                    {
                        item.FromFundName = (item.TransactionFromId > 0) ?
                                   _moneyInmateService.GetAccountTransactionId(item.TransactionFromId) : _moneyInmateService.GetAccountTransactionId(item.TransactionId);
                        item.ToFundName = (item.TransactionToId > 0) ?
                         _moneyInmateService.GetAccountTransactionId(item.TransactionToId) : _moneyInmateService.GetAccountTransactionId(item.TransactionId);
                    }

                });
            return moneyFundLedger;
        }

	private List<KeyValuePair<string, decimal>> GetMoneyFundDetails (MoneyFund searchValue) {
            List<KeyValuePair<string, decimal>> fundDetails = new List<KeyValuePair<string, decimal>> ();
            decimal? pendingDetails = _context.AccountAoReceive.Where (w =>
                w.AccountAoFundId == searchValue.FundId &&
                (!w.TransactionVoidFlag.HasValue || w.TransactionVoidFlag == 0) &&
                w.AccountAoFund.AccountAoBankId == searchValue.BankId &&
                ((searchValue.FromDate.HasValue && searchValue.ToDate.HasValue) &&
                    (searchValue.FromDate.Value.Date <= w.TransactionDate.Value.Date &&
                        searchValue.ToDate.Value.Date >= w.TransactionDate.Value.Date)
                )
            ).Select (s => s.AccountAoFund.BalanceFundPending).FirstOrDefault ();
            fundDetails.Add (new KeyValuePair<string, decimal> ((Enum.GetName(typeof(MoneyAccountTransactionFlagType),0)), pendingDetails ?? 0));
            decimal? unpaidDetails = _context.AccountAoFee.Where (w =>
                w.AccountAoFundId == searchValue.FundId &&
                (!w.TransactionVoidFlag.HasValue || w.TransactionVoidFlag == 0) &&
                w.AccountAoFund.AccountAoBankId == searchValue.BankId &&
                ((searchValue.FromDate.HasValue && searchValue.ToDate.HasValue) &&
                    (searchValue.FromDate.Value.Date <= w.TransactionDate.Value.Date &&
                        searchValue.ToDate.Value.Date >= w.TransactionDate.Value.Date)
                )
            ).Select (s => s.AccountAoFund.BalanceFundFee).FirstOrDefault ();
             fundDetails.Add (new KeyValuePair<string, decimal> ((Enum.GetName(typeof(MoneyAccountTransactionFlagType),0)), unpaidDetails ?? 0));
            decimal? fundLedgerDetails = _context.AccountAoTransaction.Where (w =>
                w.AccountAoFundId == searchValue.FundId &&
                w.AccountAoFund.AccountAoBankId == searchValue.BankId &&
                ((searchValue.FromDate.HasValue && searchValue.ToDate.HasValue) &&
                    (searchValue.FromDate.Value.Date <= w.TransactionDate.Value.Date &&
                        searchValue.ToDate.Value.Date >= w.TransactionDate.Value.Date)
                )
            ).Select (s => s.AccountAoFund.BalanceFund).FirstOrDefault ();
              fundDetails.Add (new KeyValuePair<string, decimal> ((Enum.GetName(typeof(MoneyAccountTransactionFlagType),0)), fundLedgerDetails ?? 0));
            return fundDetails.ToList ();
        }
    }
}