﻿
using System.Collections.Generic;
using System.Linq;
using GenerateTables.Models;
using ServerAPI.ViewModels;

// ReSharper disable once CheckNamespace
namespace ServerAPI.Services
{
    public class MoneyService : IMoneyService
    {
        private readonly AAtims _context;
        private MoneyAccountTransactionInfoVm _accountTransaction;
        private MoneyAccountTransactionDetailVm _moneyAccountTransaction;
        public MoneyService(AAtims context)
        {
            _context = context;
        }
        public List<KeyValuePair<string, int>> MoneyGetBank()
        {
            List<KeyValuePair<string, int>> bankList = _context.AccountAoBank
                .Where(w => w.Inactive == 0 && !string.IsNullOrEmpty(w.BankAccountAbbr))
                .Select(s => new KeyValuePair<string, int>(s.BankAccountAbbr, s.AccountAoBankId))
                .OrderBy(o => o.Key).ToList();
            return bankList;
        }
        public List<MoneyAccountAoFundVm> MoneyGetFund()
        {
            List<MoneyAccountAoFundVm> fundList = _context.AccountAoFund
                .Where(w => (!w.Inactive.HasValue || w.Inactive == 0))
                .Select(s =>
                   new MoneyAccountAoFundVm
                   {
                       AccountAOFundId = s.AccountAoFundId,
                       FundName = s.FundName,
                       FundDescription = s.FundDescription,
                       Inactive = s.Inactive ?? 0,
                       FundInmateOnlyFlag = s.FundInmateOnlyFlag ?? 0,
                       FundWelfareOnlyFlag = s.FundWelfareOnlyFlag ?? 0,
                       FundBailOnlyFlag = s.FundBailOnlyFlag ?? 0,
                       FundCommissaryOnlyFlag = s.FundCommissaryOnlyFlag ?? 0,
                       FundOptionOnly = s.FundOptionOnly,
                       FundAllowTransfer = s.FundAllowTransfer ?? 0,
                       FundAllowFee = s.FundAllowFee ?? 0,
                       FundAllowFeeOrder = s.FundAllowFeeOrder ?? 0,
                       FundInmateMinBalance = s.FundInmateMinBalance ?? 0,
                       BalanceFund = s.BalanceFund ?? 0,
                       FundBlindVerify = s.FundBlindVerify ?? 0,
                       FundAllowBypassDepository = s.FundAllowBypassDepository ?? 0,
                       AccountAoBankId = s.AccountAoBankId,
                       BalanceFundFee = s.BalanceFundFee,
                       BalanceFundPending = s.BalanceFundPending
                   }).ToList();
            // To add fee type for each fund
            List<MoneyAccountAoFeeTypeVm> feeTypeList = _context.AccountAoFeeType
                .Where(w => !w.DeleteFlag.HasValue || w.DeleteFlag == 0)
                .Select(s => new MoneyAccountAoFeeTypeVm
                {
                    AccountAoFeeTypeId = s.AccountAoFeeTypeId,
                    AccountAoFundId = s.AccountAoFundId,
                    FeeTypeName = s.FeeTypeName,
                    FeeTypeDefault = s.FeeTypeDefault ?? 0
                }).ToList();
            fundList.ForEach(f =>
            {
                f.FeeTypes = feeTypeList.Where(w => w.AccountAoFundId == f.AccountAOFundId).ToList();
            });
            return fundList;
        }
        public List<KeyValuePair<string, int>> MoneyGetDeposit()
        {
            List<KeyValuePair<string, int>> depositoryList = _context.AccountAoDepository
                .Select(s => new KeyValuePair<string, int>(
                   s.DepositoryName,
                   s.AccountAoDepositoryId
               )).ToList();
            return depositoryList;
        }
        public List<KeyValuePair<string, int>> MoneyGetCashDrawer(int facilityId, bool addVault)
        {
            List<KeyValuePair<string, int>> cashDrawerList = _context.AccountAoCashBalance.Where(w =>
               w.FacilityId == facilityId 
               && (addVault || !w.VaultFlag)
               && !w.InactiveFlag).Select(s => new KeyValuePair<string, int>(
              s.Name,
              s.AccountAoCashBalanceId
          )).ToList();
            return cashDrawerList;
        }
        public MoneyAccountTransactionDetailVm GetMoneyAccountTransaction(MoneyAccountFilterVm searchValue)
        {
            _moneyAccountTransaction = new MoneyAccountTransactionDetailVm();
            GetMoneyPendingTransaction(searchValue);
            GetMoneyUnPaidTransaction(searchValue);
            GetMoneyLedgerTransaction(searchValue);
            return _moneyAccountTransaction;
        }
        //pending transaction
        private void GetMoneyPendingTransaction(MoneyAccountFilterVm searchValue)
        {
            _moneyAccountTransaction.PendingDetails = _context.AccountAoReceive
                .Where(x => ((x.AccountAoFund.AccountAoBankId == searchValue.BankId) &&
                   ((!searchValue.FromDate.HasValue || !searchValue.ToDate.HasValue) ||
                       (searchValue.FromDate.Value.Date <= x.TransactionDate.Value.Date &&
                           searchValue.ToDate.Value.Date >= x.TransactionDate.Value.Date)) &&
                   (!x.TransactionVerified.HasValue || x.TransactionVerified == 0) &&
                   (!x.TransactionVoidFlag.HasValue || x.TransactionVoidFlag == 0))).Select(s => new MoneyAccountTransactionVm
                   {
                       TransactionDate = s.TransactionDate,
                       TransactionAmount = s.TransactionAmount ?? 0,
                       TransactionDescription = s.TransactionDescription,
                       TransactionReceipt = s.TransactionReceipt,
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
                       InmateId = s.InmateId ?? 0,
                       FacilityAbbr = s.Inmate.Facility.FacilityAbbr,
                       InmateNumber = s.Inmate.InmateNumber,
                       FundName = s.AccountAoFund.FundName,
                       BankId = s.AccountAoFund.AccountAoBankId,
                       FundId = s.AccountAoFund.AccountAoFundId,
                       DepositoryName = s.AccountAoDepository.DepositoryName,
                       VoidFlag = s.TransactionVoidFlag == 1,
                       TransactionId = s.AccountAoReceiveId,
                       TransactionVerified = s.TransactionVerified == 1,
                       ReceiveCashFlag = s.TransactionReceiveCashFlag == 1,
                       TransactionType = s.TransactionType,
                       BankAccountAbbr = s.AccountAoFund.AccountAoBank.BankAccountAbbr,
                       ReceiveNumber = s.TransactionReceiveNumber,
                       ReceiveFrom = s.TransactionReceiveFrom,
                       ClearDate = s.TransactionVerifiedDate,
                       BalanceInmate = s.BalanceInmatePending,
                       BalanceFund = s.BalanceAccountPending,
                       BalanceAccount = s.BalanceAccountPending,
                       TransactionNotes = s.TransactionNotes,
                       VoidDate = s.TransactionVoidDate,
                       ClearBy = new PersonnelVm
                       {
                           PersonFirstName = s.TransactionVerifiedByNavigation.PersonNavigation.PersonFirstName,
                           PersonLastName = s.TransactionVerifiedByNavigation.PersonNavigation.PersonLastName,
                           OfficerBadgeNumber = s.TransactionVerifiedByNavigation.OfficerBadgeNumber
                       },
                       VoidBy = new PersonnelVm
                       {
                           PersonFirstName = s.TransactionVoidByNavigation.PersonNavigation.PersonFirstName,
                           PersonLastName = s.TransactionVoidByNavigation.PersonNavigation.PersonLastName,
                           OfficerBadgeNumber = s.TransactionVoidByNavigation.OfficerBadgeNumber
                       },
                       PersonnelBy = new PersonnelVm
                       {
                           PersonFirstName = s.CreatedByNavigation.PersonNavigation.PersonFirstName,
                           PersonLastName = s.CreatedByNavigation.PersonNavigation.PersonLastName,
                           OfficerBadgeNumber = s.CreatedByNavigation.OfficerBadgeNumber
                       },
                       AccountAoDepositoryId = s.AccountAoDepositoryId
                   }).OrderByDescending(o => o.TransactionId).ToList();
            if (_moneyAccountTransaction.PendingDetails.Any())
            {
                _moneyAccountTransaction.PendingBalanceAmount = _context.AccountAoBank.First(b =>
                       _moneyAccountTransaction.PendingDetails.Any(a => a.BankId == b.AccountAoBankId))?
                    .BalanceAccountPending;
            }

        }
        //unpaid transaction
        private void GetMoneyUnPaidTransaction(MoneyAccountFilterVm searchValue)
        {
            _moneyAccountTransaction.UnpaidDetails = _context.AccountAoFee.Where(x =>
                   ((x.AccountAoFund.AccountAoBankId == searchValue.BankId) &&
                       ((!searchValue.FromDate.HasValue || !searchValue.ToDate.HasValue) ||
                           (searchValue.FromDate.Value.Date <= x.TransactionDate.Value.Date &&
                               searchValue.ToDate.Value.Date >= x.TransactionDate.Value.Date)) &&
                       (!x.TransactionCleared.HasValue || x.TransactionCleared == 0) &&
                       (!x.TransactionVoidFlag.HasValue || x.TransactionVoidFlag == 0)))
                .Select(s => new MoneyAccountTransactionVm
                {
                    TransactionId = s.AccountAoFeeId,
                    TransactionDate = s.TransactionDate,
                    TransactionAmount = s.TransactionFee ?? 0,
                    TransactionDescription = s.TransactionDescription,
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
                    InmateId = s.InmateId ?? 0,
                    FacilityAbbr = s.Inmate.Facility.FacilityAbbr,
                    InmateNumber = s.Inmate.InmateNumber,
                    FundName = s.AccountAoFund.FundName,
                    FundId = s.AccountAoFund.AccountAoFundId,
                    BankId = s.AccountAoFund.AccountAoBankId,
                    TransactionType = s.TransactionType,
                    BankAccountAbbr = s.AccountAoFund.AccountAoBank.BankAccountAbbr,
                    TransactionNotes = s.TransactionNotes,
                    VoidDate = s.TransactionVoidDate,
                    ClearDate = s.TransactionClearedDate,
                    BalanceInmate = s.BalanceInmateFee,
                    BalanceFund = s.BalanceFundFee,
                    BalanceAccount = s.BalanceAccountFee,
                    ClearFlag = s.TransactionCleared == 1,
                    ClearBy = new PersonnelVm
                    {
                        PersonFirstName = s.TransactionClearedByNavigation.PersonNavigation.PersonFirstName,
                        PersonLastName = s.TransactionClearedByNavigation.PersonNavigation.PersonLastName,
                        OfficerBadgeNumber = s.TransactionClearedByNavigation.OfficerBadgeNumber
                    },
                    VoidBy = new PersonnelVm
                    {
                        PersonFirstName = s.TransactionVoidByNavigation.PersonNavigation.PersonFirstName,
                        PersonLastName = s.TransactionVoidByNavigation.PersonNavigation.PersonLastName,
                        OfficerBadgeNumber = s.TransactionVoidByNavigation.OfficerBadgeNumber
                    },
                    PersonnelBy = new PersonnelVm
                    {
                        PersonFirstName = s.CreatedByNavigation.PersonNavigation.PersonFirstName,
                        PersonLastName = s.CreatedByNavigation.PersonNavigation.PersonLastName,
                        OfficerBadgeNumber = s.CreatedByNavigation.OfficerBadgeNumber
                    },
                    VoidFlag = s.TransactionVoidFlag == 1,
                }).OrderByDescending(o => o.TransactionId).ToList();
            if (_moneyAccountTransaction.UnpaidDetails.Any())
            {
                _moneyAccountTransaction.UnpaidFeeBalanceAmount = _context.AccountAoBank.First(b =>
                   _moneyAccountTransaction.UnpaidDetails.Any(a => a.BankId == b.AccountAoBankId))?.BalanceAccountFee;
            }

        }
        //Account ledger transaction
        private void GetMoneyLedgerTransaction(MoneyAccountFilterVm searchValue)
        {
            _moneyAccountTransaction.AccountLedgerDetails = _context.AccountAoTransaction
                .Where(x => x.AccountAoFund.AccountAoBankId == searchValue.BankId &&
                   !searchValue.FromDate.HasValue || !searchValue.ToDate.HasValue ||
                   (searchValue.FromDate.Value.Date <= x.TransactionDate.Value.Date &&
                       searchValue.ToDate.Value.Date >= x.TransactionDate.Value.Date))
                .Select(s => new MoneyAccountTransactionVm
                {
                    AccountAoReceiveId = s.AccountAoReceiveId ?? 0,
                    BalanceFund = s.BalanceFund,
                    TransactionCheckNumber = s.TransactionCheckNumber,
                    TransactionReceiveCashFlag = s.TransactionReceiveCashFlag == 1,
                    TransactionDate = s.TransactionDate,
                    TransactionAmount = s.TransactionAmount,
                    TransactionDescription = s.TransactionDescription,
                    TransactionReceipt = s.TransactionReceipt,
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
                    InmateId = s.InmateId ?? 0,
                    FacilityAbbr = s.Inmate.Facility.FacilityAbbr,
                    InmateNumber = s.Inmate.InmateNumber,
                    FundName = s.AccountAoFund.FundName,
                    FundId = s.AccountAoFund.AccountAoFundId,
                    BankId = s.AccountAoFund.AccountAoBankId,
                    AccountAoTransactionId = s.AccountAoTransactionId,
                    BalanceAccount = s.BalanceAccount,
                    TransactionId = s.AccountAoTransactionId,
                    VoidFlag = s.TransactionVoidFlag == 1,
                    TransactionType = s.TransactionType,
                    BankAccountAbbr = s.AccountAoFund.AccountAoBank.BankAccountAbbr,
                    TransactionNotes = s.TransactionNotes,
                    VoidDate = s.TransactionVoidDate,
                    ClearDate = s.TransactionClearedDate,
                    BalanceInmate = s.BalanceInmate,
                    ClearFlag = s.TransactionCleared == 1,
                    ReturnCashFlag = s.TransactionReturnCashFlag == 1,
                    AccountAoFeeId = s.AccountAoFeeId ?? 0,
                    ReceiveFrom = s.TransactionReceiveFrom,
                    TransactionPayToTheOrderOf = s.TransactionPayToTheOrderOf,
                    ReceiveNumber = s.TransactionReceiveNumber,
                    TransactionDebitMemo = s.TransactionDebitMemo,
                    TransactionFromId = s.AccountAoTransactionFromId ?? 0,
                    TransactionToId = s.AccountAoTransactionToId ?? 0,
                    ClearBy = new PersonnelVm
                    {
                        PersonFirstName = s.TransactionClearedByNavigation.PersonNavigation.PersonFirstName,
                        PersonLastName = s.TransactionClearedByNavigation.PersonNavigation.PersonLastName,
                        OfficerBadgeNumber = s.TransactionClearedByNavigation.OfficerBadgeNumber
                    },
                    VoidBy = new PersonnelVm
                    {
                        PersonFirstName = s.TransactionVoidByNavigation.PersonNavigation.PersonFirstName,
                        PersonLastName = s.TransactionVoidByNavigation.PersonNavigation.PersonLastName,
                        OfficerBadgeNumber = s.TransactionVoidByNavigation.OfficerBadgeNumber

                    },
                    PersonnelBy = new PersonnelVm
                    {
                        PersonFirstName = s.CreatedByNavigation.PersonNavigation.PersonFirstName,
                        PersonLastName = s.CreatedByNavigation.PersonNavigation.PersonLastName,
                        OfficerBadgeNumber = s.CreatedByNavigation.OfficerBadgeNumber
                    }
                }).OrderByDescending(o => o.TransactionId).ToList();
            if (_moneyAccountTransaction.AccountLedgerDetails.Any())
            {
                _moneyAccountTransaction.AccountLedgerBalanceAmount = _context.AccountAoBank.First(b =>
                       _moneyAccountTransaction.AccountLedgerDetails.Any(a => a.BankId == b.AccountAoBankId))?
                    .BalanceAccount;
            }
        }

        //get Transaction Detail
        public MoneyAccountTransactionInfoVm GetMoneyTransactionDetails(int transId, int inmateId, int bankId,
            MoneyAccountTransactionFlagType pFlag)
        {
            _accountTransaction = new MoneyAccountTransactionInfoVm
            {
                MoneyAccountRelatedTranscations = _context.AccountAoTransaction
                .Where(x => (pFlag == MoneyAccountTransactionFlagType.PENDING && x.AccountAoReceiveId == transId) ||
               (pFlag == MoneyAccountTransactionFlagType.UNPAID && x.AccountAoFeeId == transId) ||
               (pFlag == MoneyAccountTransactionFlagType.ACCOUNTTRANSACTION &&
               (x.AccountAoTransactionId == transId || x.AccountAoTransactionFromId == transId ||
               x.AccountAoTransactionToId == transId)) && x.AccountAoFund.AccountAoBankId == bankId)
                .Select(s => new MoneyAccountTransactionVm
                {
                    AccountAoReceiveId = s.AccountAoReceiveId ?? 0,
                    TransactionDate = s.TransactionDate,
                    TransactionAmount = s.TransactionAmount,
                    TransactionDescription = s.TransactionDescription,
                    TransactionReceipt = s.TransactionReceipt,
                    FundId = s.AccountAoFund.AccountAoFundId,
                    BankId = s.AccountAoFund.AccountAoBankId,
                    AccountAoTransactionId = s.AccountAoTransactionId,
                    TransactionId = s.AccountAoTransactionId,
                    VoidFlag = s.TransactionVoidFlag == 1,
                    FundName = s.AccountAoFund.FundName,
                    ReturnCashFlag = s.TransactionReturnCashFlag == 1,
                    AccountAoFeeId = s.AccountAoFeeId ?? 0,
                    ClearDate = s.TransactionClearedDate,
                    TransactionPayToTheOrderOf = s.TransactionPayToTheOrderOf,
                    ReceiveFrom = s.TransactionReceiveFrom,
                    TransactionCheckNumber = s.TransactionCheckNumber,
                    ReceiveNumber = s.TransactionReceiveNumber,
                    BalanceAccount = s.BalanceAccount,
                    BalanceFund = s.BalanceFund,
                    DepositoryName = s.AccountAoReceive.AccountAoDepository.DepositoryName,
                    TransactionType = s.TransactionType,
                    TransactionNotes = s.TransactionNotes,
                    BalanceInmate = s.BalanceInmate,
                    Person = s.InmateId > 0 ? new PersonVm
                    {
                        PersonLastName = s.Inmate.Person.PersonLastName,
                        PersonFirstName = s.Inmate.Person.PersonFirstName,
                        PersonMiddleName = s.Inmate.Person.PersonMiddleName,
                        PersonSuffix = s.Inmate.Person.PersonSuffix,
                        PersonId = s.Inmate.PersonId,
                        FacilityId = s.Inmate.FacilityId,

                    } : new PersonVm(),
                    InmateId = s.InmateId ?? 0,
                    InmateNumber = s.Inmate.InmateNumber,
                    BankAccountAbbr = s.AccountAoFund.AccountAoBank.BankAccountAbbr,
                    ClearBy = new PersonnelVm
                    {
                        PersonFirstName = s.TransactionClearedByNavigation.PersonNavigation.PersonFirstName,
                        PersonLastName = s.TransactionClearedByNavigation.PersonNavigation.PersonLastName,
                        OfficerBadgeNumber = s.TransactionClearedByNavigation.OfficerBadgeNumber
                    },
                    PersonnelBy = new PersonnelVm
                    {
                        PersonFirstName = s.CreatedByNavigation.PersonNavigation.PersonFirstName,
                        PersonLastName = s.CreatedByNavigation.PersonNavigation.PersonLastName,
                        OfficerBadgeNumber = s.CreatedByNavigation.OfficerBadgeNumber
                    },
                    TransactionDebitMemo = s.TransactionDebitMemo,
                }).OrderByDescending(o => o.AccountAoTransactionId).ToList()
            };

            if (_accountTransaction.MoneyAccountRelatedTranscations.Count > 0)
            {
                GetReceiveTransactionDetails(bankId);
                UnPaidTransactionDetails(bankId);
                AccountAoCashDrawerDetails();
            }

            return _accountTransaction;
        }
        public void GetReceiveTransactionDetails(int bankId)
        {
            int[] accountAoReceiveIds = _accountTransaction.MoneyAccountRelatedTranscations.Where(a =>
                   a.AccountAoReceiveId > 0 && (a.AccountAoFeeId == 0)).Select(t => t.AccountAoReceiveId).ToArray();
            int[] accountAoAdjustmentReceiveIds = _context.AccountAoReceive.Where(a =>
                    accountAoReceiveIds.Any(r => r == a.AccountAoReceiveId)).Select(t => t.AdjustmentAccountAoReceiveId ?? 0)
                .ToArray();
            if (accountAoReceiveIds.Any())
            {
                _accountTransaction.SubReceiveTransactions = _context.AccountAoReceive.Where(x => x.AccountAoFund.AccountAoBankId == bankId &&
                        (accountAoReceiveIds.Any(a => a == x.AccountAoReceiveId) || accountAoAdjustmentReceiveIds.Any(ad => ad == x.AccountAoReceiveId)))
                    .Select(s => new MoneyAccountTransactionVm
                    {
                        TransactionDate = s.TransactionDate,
                        TransactionAmount = s.TransactionAmount ?? 0,
                        TransactionDescription = s.TransactionDescription,
                        Person = s.InmateId > 0 ? new PersonVm
                        {
                            PersonLastName = s.Inmate.Person.PersonLastName,
                            PersonFirstName = s.Inmate.Person.PersonFirstName,
                            PersonMiddleName = s.Inmate.Person.PersonMiddleName,
                            PersonSuffix = s.Inmate.Person.PersonSuffix,
                            PersonId = s.Inmate.PersonId,
                            FacilityId = s.Inmate.FacilityId,

                        } : new PersonVm(),
                        InmateId = s.InmateId ?? 0,
                        InmateNumber = s.Inmate.InmateNumber,
                        VoidFlag = s.TransactionVoidFlag == 1,
                        TransactionId = s.AccountAoReceiveId,
                        ReceiveCashFlag = s.TransactionReceiveCashFlag == 1,
                        TransactionType = s.TransactionType,
                        ReceiveFrom = s.TransactionReceiveFrom,
                        ClearDate = s.TransactionVerifiedDate,
                        ClearFlag = s.TransactionVerified == 1,
                         VoidDate = s.TransactionVoidDate,
                        ClearBy = new PersonnelVm
                        {
                            PersonFirstName = s.TransactionVerifiedByNavigation.PersonNavigation.PersonFirstName,
                            PersonLastName = s.TransactionVerifiedByNavigation.PersonNavigation.PersonLastName,
                            OfficerBadgeNumber = s.TransactionVerifiedByNavigation.OfficerBadgeNumber
                        },
                        VoidBy = new PersonnelVm
                        {
                            PersonFirstName = s.TransactionVoidByNavigation.PersonNavigation.PersonFirstName,
                            PersonLastName = s.TransactionVoidByNavigation.PersonNavigation.PersonLastName,
                            OfficerBadgeNumber = s.TransactionVoidByNavigation.OfficerBadgeNumber
                        },
                        PersonnelBy = new PersonnelVm
                        {
                            PersonFirstName = s.CreatedByNavigation.PersonNavigation.PersonFirstName,
                            PersonLastName = s.CreatedByNavigation.PersonNavigation.PersonLastName,
                            OfficerBadgeNumber = s.CreatedByNavigation.OfficerBadgeNumber
                        },
                        AccountAoDepositoryId = s.AccountAoDepositoryId
                    }).OrderByDescending(o => o.TransactionId).ToList();
            }
        }

        private void UnPaidTransactionDetails(int bankId)
        {
            int[] accountAoFeeIds = _accountTransaction.MoneyAccountRelatedTranscations.Where(a => a.AccountAoFeeId > 0)
                .Select(t => t.AccountAoFeeId).ToArray();
            if (accountAoFeeIds.Any())
            {
                _accountTransaction.SubFeeTransactions = _context.AccountAoFee.Where(x =>
                       (x.AccountAoFund.AccountAoBankId == bankId &&
                           accountAoFeeIds.Any(a => a == x.AccountAoFeeId)))
                    .Select(s => new MoneyAccountTransactionVm
                    {
                        TransactionId = s.AccountAoFeeId,
                        TransactionDate = s.TransactionDate,
                        TransactionAmount = s.TransactionFee ?? 0,
                        TransactionDescription = s.TransactionDescription,
                        InmateId = s.InmateId ?? 0,
                        TransactionType = s.TransactionType,
                        TransactionNotes = s.TransactionNotes,
                        VoidDate = s.TransactionVoidDate,
                        ClearDate = s.TransactionClearedDate,
                        ClearFlag = s.TransactionCleared == 1,
                        ClearBy = new PersonnelVm
                        {
                            PersonFirstName = s.TransactionClearedByNavigation.PersonNavigation.PersonFirstName,
                            PersonLastName = s.TransactionClearedByNavigation.PersonNavigation.PersonLastName,
                            OfficerBadgeNumber = s.TransactionClearedByNavigation.OfficerBadgeNumber
                        },
                        VoidBy = new PersonnelVm
                        {
                            PersonFirstName = s.TransactionVoidByNavigation.PersonNavigation.PersonFirstName,
                            PersonLastName = s.TransactionVoidByNavigation.PersonNavigation.PersonLastName,
                            OfficerBadgeNumber = s.TransactionVoidByNavigation.OfficerBadgeNumber
                        },
                        PersonnelBy = new PersonnelVm
                        {
                            PersonFirstName = s.CreatedByNavigation.PersonNavigation.PersonFirstName,
                            PersonLastName = s.CreatedByNavigation.PersonNavigation.PersonLastName,
                            OfficerBadgeNumber = s.CreatedByNavigation.OfficerBadgeNumber
                        },
                        VoidFlag = s.TransactionVoidFlag == 1,
                    }).OrderByDescending(o => o.TransactionId).ToList();
            }
        }

        public void AccountAoCashDrawerDetails()
        {
            int[] accountTransIds = _accountTransaction.MoneyAccountRelatedTranscations
                .Where(a => a.ReturnCashFlag && (a.AccountAoFeeId == 0) && (a.AccountAoReceiveId == 0))
                .Select(t => t.TransactionId).ToArray();
            if (accountTransIds.Any())
            {
                _accountTransaction.CashDrawerTransactions = _context.AccountAoCashDrawer
                    .Where(x => accountTransIds.Any(aq => aq == x.AccountAoTransactionId))
                    .Select(s => new MoneyAccountTransactionVm
                    {
                        TransactionDate = s.TransactionDate,
                        TransactionAmount = s.TransactionAmount,
                        TransactionDescription = s.TransactionDescription,
                        InmateId = s.InmateId ?? 0,
                        VoidDate = s.TransactionVoidDate,
                        TransactionId = s.AccountAoCashDrawerId,
                        VoidBy = new PersonnelVm
                        {
                            PersonFirstName = s.TransactionVoidByNavigation.PersonNavigation.PersonFirstName,
                            PersonLastName = s.TransactionVoidByNavigation.PersonNavigation.PersonLastName,
                            OfficerBadgeNumber = s.TransactionVoidByNavigation.OfficerBadgeNumber
                        },
                        PersonnelBy = new PersonnelVm
                        {
                            PersonFirstName = s.CreatedByNavigation.PersonNavigation.PersonFirstName,
                            PersonLastName = s.CreatedByNavigation.PersonNavigation.PersonLastName,
                            OfficerBadgeNumber = s.CreatedByNavigation.OfficerBadgeNumber
                        },
                    }).OrderByDescending(o => o.TransactionId).ToList();
            }
        }
        #region  Get bank details for print and receipt 
        public MoneyBankVm GetBankDetails() => _context.AccountAoBank.Select(s => new MoneyBankVm
        {
            BankId = s.AccountAoBankId,
            Balance = s.BalanceAccount,
            BankName = s.BankAccountAbbr,
            ReceiptShowBalance = s.ReceiptShowBalance == 1,
            ReceiptShowOwed = s.ReceiptShowOwed == 1,
            ReceiptShowPending = s.ReceiptShowPending == 1,
            ReceiptCopies = s.ReceiptCopies ?? 0,
            ClearCheckAfterDays = s.ClearCheckAfterDays
        }).FirstOrDefault();

        #endregion

        #region  Get Inmate detail
        public MoneyAccountAoInmateVm GetAccountAoInmate(int inmateId) => _context.AccountAoInmate.Where(
            w => w.InmateId == inmateId
        ).Select(s => new MoneyAccountAoInmateVm
        {
            AccountAoInmateId = s.AccountAoInmateId,
            AccountAoBankId = s.AccountAoBankId,
            BalanceInmate = s.BalanceInmate,
            BalanceInmateFee = s.BalanceInmateFee,
            BalanceInmatePending = s.BalanceInmatePending,
            InmateId = s.InmateId
        }).SingleOrDefault();
        #endregion

    }
}