using System;
using GenerateTables.Models;
using ServerAPI.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using ServerAPI.Utilities;


// ReSharper disable once CheckNamespace
namespace ServerAPI.Services
{
    public class MoneyInmateService : IMoneyInmateService
    {
        private readonly AAtims _context;
        private readonly int _personnelId;
        private readonly IMoneyBalanceDebitCheck _moneyBalanceDebitCheck;
        public MoneyInmateService(AAtims context, IHttpContextAccessor httpContextAccessor, IMoneyBalanceDebitCheck moneyBalanceDebitCheck)
        {
            _context = context;
            _personnelId = Convert.ToInt32(httpContextAccessor.HttpContext.User.FindFirst("personnelId")?.Value);
            _moneyBalanceDebitCheck = moneyBalanceDebitCheck;
        }

        #region Main  inmate ledger grid
        // Money -> Inmate -> Ledger
        public MoneyAccountTransactionDetailVm GetMoneyInmateLedger(int iBankId, int iInmateId)
        {
            List<AccountAoCashDrawer> accountAoCashDrawers = _context.AccountAoCashDrawer.Where(w => w.InmateId == iInmateId).ToList();
            MoneyAccountTransactionDetailVm inmateLedger = new MoneyAccountTransactionDetailVm
            {
                PendingDetails = _context.AccountAoReceive
                    .Where(w => (!w.TransactionVerified.HasValue || w.TransactionVerified == 0) &&
                                (!w.TransactionVoidFlag.HasValue || w.TransactionVoidFlag == 0) &&
                                w.InmateId == iInmateId && w.AccountAoFund.FundInmateOnlyFlag == 1 &&
                                w.AccountAoFund.AccountAoBankId == iBankId)
                    .Select(s => new MoneyAccountTransactionVm
                    {
                        TransactionDate = s.TransactionDate,
                        TransactionAmount = s.TransactionAmount ?? 0,
                        TransactionDescription = s.TransactionDescription,
                        TransactionReceipt = s.TransactionReceipt,
                        Person = new PersonVm
                        {
                            PersonLastName = s.Inmate.Person.PersonLastName,
                            PersonFirstName = s.Inmate.Person.PersonFirstName,
                            PersonMiddleName = s.Inmate.Person.PersonMiddleName,
                            PersonSuffix = s.Inmate.Person.PersonSuffix,
                            PersonId = s.Inmate.PersonId,
                            FacilityId = s.Inmate.FacilityId,

                        },
                        HousingUnit = s.Inmate.HousingUnitId > 0 ? new HousingDetail
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
                        ClearBy = s.TransactionVerifiedBy > 0 ?
                            new PersonnelVm
                            {
                                PersonFirstName =
                                    s.TransactionVerifiedByNavigation.PersonNavigation.PersonFirstName,
                                PersonLastName =
                                    s.TransactionVerifiedByNavigation.PersonNavigation.PersonLastName,
                                OfficerBadgeNumber = s.TransactionVerifiedByNavigation.OfficerBadgeNum
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
                        AccountAoDepositoryId = s.AccountAoDepositoryId
                    }).OrderByDescending(o => o.TransactionId).ToList(),
                UnpaidDetails = _context.AccountAoFee
                    .Where(w => (!w.TransactionCleared.HasValue || w.TransactionCleared == 0) &&
                                (!w.TransactionVoidFlag.HasValue || w.TransactionVoidFlag == 0) &&
                                w.InmateId == iInmateId && w.AccountAoFund.AccountAoBankId == iBankId)
                    .Select(s => new MoneyAccountTransactionVm
                    {
                        TransactionId = s.AccountAoFeeId,
                        TransactionDate = s.TransactionDate,
                        TransactionAmount = s.TransactionFee ?? 0,
                        TransactionDescription = s.TransactionDescription,
                        InmateId = s.InmateId ?? 0,
                        InmateNumber = s.Inmate.InmateNumber,
                        Person = new PersonVm
                        {
                            PersonLastName = s.Inmate.Person.PersonLastName,
                            PersonFirstName = s.Inmate.Person.PersonFirstName,
                            PersonMiddleName = s.Inmate.Person.PersonMiddleName,
                            PersonSuffix = s.Inmate.Person.PersonSuffix,
                            PersonId = s.Inmate.PersonId,
                            FacilityId = s.Inmate.FacilityId,

                        },
                        HousingUnit = s.Inmate.HousingUnitId > 0 ? new HousingDetail
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
                        TransactionNotes = s.TransactionNotes,
                        VoidDate = s.TransactionVoidDate,
                        ClearDate = s.TransactionClearedDate,
                        BalanceInmate = s.BalanceInmateFee,
                        BalanceFund = s.BalanceFundFee,
                        BalanceAccount = s.BalanceAccountFee,
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
                        ClearFlag = s.TransactionCleared == 1,
                        VoidFlag = s.TransactionVoidFlag == 1,
                    }).OrderByDescending(o => o.TransactionId).ToList(),

                AccountLedgerDetails = _context.AccountAoTransaction
                    .Where(w =>
                        w.InmateId == iInmateId && w.AccountAoFund.FundInmateOnlyFlag == 1 &&
                        w.AccountAoFund.AccountAoBankId == iBankId)
                    .Select(s => new MoneyAccountTransactionVm
                    {
                        AccountAoReceiveId = s.AccountAoReceiveId ?? 0,
                        TransactionCheckNumber = s.TransactionCheckNumber,
                        ReceiveCashFlag = s.TransactionReceiveCashFlag == 1,
                        TransactionDate = s.TransactionDate,
                        TransactionAmount = s.TransactionAmount,
                        TransactionDescription = s.TransactionDescription,
                        TransactionReceipt = s.TransactionReceipt,
                        InmateId = s.InmateId ?? 0,
                        InmateNumber = s.Inmate.InmateNumber,
                        Person = new PersonVm
                        {
                            PersonLastName = s.Inmate.Person.PersonLastName,
                            PersonFirstName = s.Inmate.Person.PersonFirstName,
                            PersonMiddleName = s.Inmate.Person.PersonMiddleName,
                            PersonSuffix = s.Inmate.Person.PersonSuffix,
                            PersonId = s.Inmate.PersonId,
                            FacilityId = s.Inmate.FacilityId,

                        },
                        HousingUnit = s.Inmate.HousingUnitId > 0 ? new HousingDetail
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
                        BalanceAccount = s.BalanceAccount,
                        TransactionId = s.AccountAoTransactionId,
                        VoidFlag = s.TransactionVoidFlag == 1,
                        DepositoryName = s.AccountAoReceive.AccountAoDepository.DepositoryName,
                        TransactionType = s.TransactionType,
                        BankAccountAbbr = s.AccountAoFund.AccountAoBank.BankAccountAbbr,
                        TransactionNotes = s.TransactionNotes,
                        VoidDate = s.TransactionVoidDate,
                        ClearDate = s.TransactionClearedDate,
                        BalanceInmate = s.BalanceInmate,
                        BalanceFund = s.BalanceFund,
                        ClearFlag = s.TransactionCleared == 1,
                        ReturnCashFlag = s.TransactionReturnCashFlag == 1,
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
                        TransactionCashFlag = accountAoCashDrawers.Any(w => w.AccountAoTransactionId == s.AccountAoTransactionFromId),
                        CreatedDate = s.CreateDate
                    }).OrderByDescending(o => o.TransactionId).ToList()
            };

            inmateLedger.AccountLedgerDetails.ForEach(
                item =>
                        {
                            if (!item.ReturnCashFlag && (item.TransactionToId > 0 || item.TransactionFromId > 0))
                            {
                                item.FromFundName = item.TransactionFromId > 0 ?
                                            GetAccountTransactionId(item.TransactionFromId) : GetAccountTransactionId(item.TransactionId);
                                item.ToFundName = item.TransactionToId > 0 ?
                         GetAccountTransactionId(item.TransactionToId) : GetAccountTransactionId(item.TransactionId);
                            }
                            if (item.AccountAoFeeId > 0)
                            {
                                item.TransactionNotes = _context.AccountAoFee.Single(s =>
                                    s.AccountAoFeeId == item.AccountAoFeeId).TransactionNotes;
                            }

                        });
            AccountAoInmate accountAoInmate = _context.AccountAoInmate.
            SingleOrDefault(s => s.InmateId == iInmateId && s.AccountAoBankId == iBankId);
            if (accountAoInmate != null)
            {
                inmateLedger.PendingBalanceAmount = accountAoInmate.BalanceInmatePending;
                inmateLedger.UnpaidFeeBalanceAmount = accountAoInmate.BalanceInmateFee;
                inmateLedger.AccountLedgerBalanceAmount = accountAoInmate.BalanceInmate;

            }
            return inmateLedger;
        }

        #endregion

        public string GetAccountTransactionId(int transId)
        {
            string value = string.Empty;
            if (transId > 0)
            {
                AccountAoTransaction aoTransaction = _context.AccountAoTransaction.Find(transId);
                if (aoTransaction.AccountAoFundId > 0 && aoTransaction.InmateId > 0)
                {
                    value = _context.AccountAoFund.Find(aoTransaction.AccountAoFundId).FundName;
                }
            }
            return value;
        }
        #region Main inmate deposit grid
        // load the money inmate deposit grid details
        public MoneyAccountTransactionDetailVm GetMoneyInmateDeposit(int iBankId, int iInmateId)
        {
            IQueryable<AccountAoInmate> accountAoInmate = _context.AccountAoInmate;

            List<MoneyAccountTransactionVm> inmateDeposit = _context.AccountAoReceive.Where(w =>
                    w.InmateId == iInmateId && w.AccountAoFund.FundInmateOnlyFlag == 1
                    && w.AccountAoFund.AccountAoBankId == iBankId)
                .Select(s => new MoneyAccountTransactionVm
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
                        OfficerBadgeNumber = s.TransactionVerifiedByNavigation.OfficerBadgeNum
                    },
                    VoidBy = new PersonnelVm
                    {
                        PersonFirstName = s.TransactionVoidByNavigation.PersonNavigation.PersonFirstName,
                        PersonLastName = s.TransactionVoidByNavigation.PersonNavigation.PersonLastName,
                        OfficerBadgeNumber = s.TransactionVoidByNavigation.OfficerBadgeNum
                    },
                    PersonnelBy = new PersonnelVm
                    {
                        PersonFirstName = s.CreatedByNavigation.PersonNavigation.PersonFirstName,
                        PersonLastName = s.CreatedByNavigation.PersonNavigation.PersonLastName,
                        OfficerBadgeNumber = s.CreatedByNavigation.OfficerBadgeNum
                    },
                    AccountAoDepositoryId = s.AccountAoDepositoryId
                }).OrderByDescending(o => o.TransactionId).ToList();

            MoneyAccountTransactionDetailVm depositDetailsVm = new MoneyAccountTransactionDetailVm
            {
                // load the pending details for money inmate deposit.
                PendingDetails = inmateDeposit.Where(w => !w.TransactionVerified || w.TransactionVerified)
                    .Select(s => s).ToList(),
                // load the history details for money inmate deposit.
                DepositDetails = inmateDeposit.Where(w => w.TransactionVerified).Select(s => s).ToList(),

                PendingBalanceAmount = accountAoInmate.Where(w =>
                        inmateDeposit.Any(a => a.InmateId == w.InmateId) && w.AccountAoBankId == iBankId)
                    .Select(s => s.BalanceInmatePending).FirstOrDefault(),

                DepositBalanceAmount = accountAoInmate.Where(w =>
                        inmateDeposit.Any(a => a.InmateId == w.InmateId) && w.AccountAoBankId == iBankId)
                    .Select(s => s.BalanceInmatePending).FirstOrDefault()
            };

            return depositDetailsVm;
        }

        #endregion

        #region Money Inmate Clear
        // Money -> Inmate -> Clear
        //Load The Inmate Clear Grid Details
        public MoneyAccountTransactionDetailVm MoneyInmateClear(int bankId, int inmateId)
        {
            IQueryable<AccountAoInmate> accountAoInmate = _context.AccountAoInmate;

            List<MoneyAccountTransactionVm> unCleared = _context.AccountAoTransaction
               .Where(w => w.TransactionCheckNumber != null && w.AccountAoFund.AccountAoBankId == bankId &&
                w.Inmate.AccountAoInmate.Any(a => a.InmateId == inmateId && a.AccountAoBankId == bankId))
          .Select(s => new MoneyAccountTransactionVm
          {
              BalanceInmate = s.BalanceInmate,
              TransactionId = s.AccountAoTransactionId,
              TransactionDate = s.TransactionDate,
              TransactionAmount = s.TransactionAmount,
              TransactionReceipt = s.TransactionReceipt,
              TransactionDescription = s.TransactionDescription,
              TransactionCheckNumber = s.TransactionCheckNumber,
              VoidFlag = s.TransactionVoidFlag == 1,
              ClearFlag = s.TransactionCleared == 1,
              TransactionVoidId = s.TransactionVoidTransactionId ?? 0,
              BankId = s.AccountAoFund.AccountAoBankId,
              TransactionFromId = s.AccountAoTransactionFromId ?? 0,
              TransactionToId = s.AccountAoTransactionToId ?? 0,
              VoidDate = s.TransactionVoidDate,
              TransactionReturnCheckFlag = s.TransactionReturnCheckFlag ?? 0,
              CreatedDate = s.CreateDate,
              VoidBy = new PersonnelVm
              {
                  PersonFirstName = s.TransactionVoidByNavigation.PersonNavigation.PersonFirstName,
                  PersonLastName = s.TransactionVoidByNavigation.PersonNavigation.PersonLastName,
                  PersonMiddleName = s.TransactionVoidByNavigation.PersonNavigation.PersonMiddleName,
                  OfficerBadgeNumber = s.TransactionVoidByNavigation.OfficerBadgeNumber,
              },
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
              HousingUnit = s.Inmate.HousingUnitId > 0 ? new HousingDetail
              {
                  HousingUnitId = s.Inmate.HousingUnit.HousingUnitId,
                  HousingUnitLocation = s.Inmate.HousingUnit.HousingUnitLocation,
                  HousingUnitNumber = s.Inmate.HousingUnit.HousingUnitNumber,
                  HousingUnitBedNumber = s.Inmate.HousingUnit.HousingUnitBedNumber,
                  HousingUnitBedLocation = s.Inmate.HousingUnit.HousingUnitBedLocation,
              } : new HousingDetail(),
              Person = new PersonVm
              {
                  PersonLastName = s.Inmate.Person.PersonLastName,
                  PersonFirstName = s.Inmate.Person.PersonFirstName,
                  PersonMiddleName = s.Inmate.Person.PersonMiddleName,
                  PersonSuffix = s.Inmate.Person.PersonSuffix,
                  PersonId = s.Inmate.PersonId,
                  FacilityId = s.Inmate.FacilityId,
              }
          }).OrderByDescending(o => o.AccountAoTransactionId).ToList();

            MoneyAccountTransactionDetailVm clearDetails = new MoneyAccountTransactionDetailVm
            {
                unclearedDetails = unCleared,
                UnClearedBalanceAmount = accountAoInmate
                    .Where(w => w.InmateId == inmateId && w.AccountAoBankId == bankId)
                    .Select(s => s.BalanceInmate).SingleOrDefault(),

                HistoryBalanceAmount = accountAoInmate.Where(w => w.InmateId == inmateId && w.AccountAoBankId == bankId)
                    .Select(s => s.BalanceInmate).SingleOrDefault()
            };

            return clearDetails;
        }


        //Update the Cleared Field
        public async Task<int> UpdateMoneyClearTransaction(int accountAoTransactionId)
        {
            AccountAoTransaction accountAoTransaction =
           _context.AccountAoTransaction.Single(w => w.AccountAoTransactionId == accountAoTransactionId);
            accountAoTransaction.TransactionCleared = 1;
            accountAoTransaction.TransactionClearedBy = _personnelId;
            accountAoTransaction.TransactionClearedDate = DateTime.Now;
            accountAoTransaction.UpdatedBy = _personnelId;
            accountAoTransaction.UpdateDate = DateTime.Now;
            return await _context.SaveChangesAsync();
        }


        //Update the Returned Field
        public async Task<int> InmateMoneyReturnTransaction(MoneyAccountTransactionVm moneyAccountTransaction)
        {
            AccountAoTransaction accountAoTransaction =
           _context.AccountAoTransaction.Single(w => w.AccountAoTransactionId == moneyAccountTransaction.TransactionId);
            accountAoTransaction.TransactionCleared = 1;
            accountAoTransaction.TransactionClearedBy = _personnelId;
            accountAoTransaction.TransactionClearedDate = DateTime.Now;
            accountAoTransaction.UpdatedBy = _personnelId;
            accountAoTransaction.UpdateDate = DateTime.Now;
            accountAoTransaction.TransactionReturnCheckFlag = 1;
            _context.SaveChanges();

            AccountAoFund fund = _context.AccountAoFund.First(f => f.FundWelfareOnlyFlag == 1);
            decimal totalamount = Math.Abs(moneyAccountTransaction.TransactionAmount);

            MoneyTransactionCalculateVm param = new MoneyTransactionCalculateVm
            {
                BankId = moneyAccountTransaction.BankId,
                FundId = fund.AccountAoFundId,
                TransactionAmount = totalamount,
                TransactionType = MoneyTransactionType.CREDIT,
                Table = MoneyTransactionTable.ACCOUNTAO_TRANSACTION
            };

            MoneyBalanceVm balanceDetails = _moneyBalanceDebitCheck.MoneyTransactionCalculateBalance(param);
            balanceDetails.InmateId = param.InmateId;
            balanceDetails.FundId = param.FundId;
            balanceDetails.BankId = param.BankId;
            balanceDetails.Table = param.Table;
            balanceDetails.TransactionDate = param.TransactionDate;
            _moneyBalanceDebitCheck.MoneyUpdateBalance(balanceDetails);

            AccountAoTransaction transaction = new AccountAoTransaction
            {
                InmateId = accountAoTransaction.InmateId,
                AccountAoFundId = fund.AccountAoFundId,
                TransactionType = MoneyConstants.CREDIT,
                TransactionOfficerId = _personnelId,
                TransactionDate = DateTime.Now,
                TransactionAmount = totalamount,
                TransactionDescription = MoneyConstants.RETURNCHECK,
                BalanceAccount = balanceDetails.BankBalance,
                BalanceFund = balanceDetails.FundBalance,
                CreatedBy = _personnelId,
                CreateDate = DateTime.Now,
                UpdateDate = DateTime.Now,
                UpdatedBy = _personnelId,
                AccountAoTransactionFromId = moneyAccountTransaction.TransactionId
            };
            _context.AccountAoTransaction.Add(transaction);
            _context.SaveChanges();
            accountAoTransaction.AccountAoTransactionToId = transaction.AccountAoTransactionId;
            return await _context.SaveChangesAsync();
        }

        #endregion

        #region Money > inmate > fee         
        //This method used to get details of fee history and unpaid details
        public MoneyAccountTransactionDetailVm GetMoneyInmateFeeDetails(int bankId, int inmateId)
        {
            MoneyAccountTransactionDetailVm objInmateFee = new MoneyAccountTransactionDetailVm();

            List<MoneyAccountTransactionVm> inmateFeeDetails = _context.AccountAoFee
                .Where(w => w.InmateId == inmateId && w.AccountAoFundId == w.AccountAoFund.AccountAoFundId
                    && w.Inmate.AccountAoInmate.Any(k => k.AccountAoBankId == bankId))
                .Select(s => new MoneyAccountTransactionVm
                {
                    TransactionId = s.AccountAoFeeId,
                    TransactionDate = s.TransactionDate,
                    TransactionAmount = s.TransactionFee ?? 0,
                    TransactionDescription = s.TransactionDescription,
                    InmateId = s.InmateId ?? 0,
                    InmateNumber = s.Inmate.InmateNumber,
                    FundName = s.AccountAoFund.FundName,
                    FacilityAbbr = s.Inmate.Facility.FacilityAbbr,
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
                    VoidFlag = s.TransactionVoidFlag == 1,
                    Person = new PersonVm
                    {
                        PersonLastName = s.Inmate.Person.PersonLastName,
                        PersonFirstName = s.Inmate.Person.PersonFirstName,
                        PersonMiddleName = s.Inmate.Person.PersonMiddleName,
                        InmateNumber = s.Inmate.InmateNumber,
                        PersonSuffix = s.Inmate.Person.PersonSuffix,
                        PersonId = s.Inmate.PersonId,
                        FacilityId = s.Inmate.FacilityId,
                    },
                    HousingUnit = s.Inmate.HousingUnitId > 0 ? new HousingDetail
                    {
                        HousingUnitId = s.Inmate.HousingUnit.HousingUnitId,
                        HousingUnitLocation = s.Inmate.HousingUnit.HousingUnitLocation,
                        HousingUnitNumber = s.Inmate.HousingUnit.HousingUnitNumber,
                        HousingUnitBedNumber = s.Inmate.HousingUnit.HousingUnitBedNumber,
                        HousingUnitBedLocation = s.Inmate.HousingUnit.HousingUnitBedLocation,
                    } : new HousingDetail(),
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
                    PersonnelBy = new PersonnelVm
                    {
                        PersonFirstName = s.CreatedByNavigation.PersonNavigation.PersonFirstName,
                        PersonLastName = s.CreatedByNavigation.PersonNavigation.PersonLastName,
                        OfficerBadgeNumber = s.CreatedByNavigation.OfficerBadgeNum
                    }
                })
                .OrderByDescending(o => o.TransactionId).ToList();
            objInmateFee.InmateFeeDetails = inmateFeeDetails;
            objInmateFee.UnpaidFeeBalanceAmount = _context.AccountAoInmate
                .LastOrDefault(i => i.AccountAoBankId == bankId && i.InmateId == inmateId)?.BalanceInmateFee ?? 0;
            return objInmateFee;
        }
        #endregion

        #region Money > inmate > reference
        //This method is used to get inmate reference details
        public MoneyAccountTransactionDetailVm GetMoneyInmateReference(int? bankId, int? inmateId)
        {
            MoneyAccountTransactionDetailVm objInmateReference = new MoneyAccountTransactionDetailVm();

            List<MoneyAccountTransactionVm> inmateLedgerAmount = _context.AccountAoTransaction
                .Where(w => w.AccountAoFund.AccountAoBankId == bankId
                    && (w.AccountAoFund.FundInmateOnlyFlag ?? 0) == 0
                    && w.Inmate.AccountAoInmate.Any(t => t.AccountAoBankId == bankId
                        && w.Inmate.AccountAoInmate.Any(a => a.InmateId == inmateId)))
                .Select(s => new MoneyAccountTransactionVm
                {
                    BalanceInmate = s.Inmate.AccountAoInmate.FirstOrDefault().BalanceInmate,
                    AccountAoTransactionId = s.AccountAoTransactionId
                }).OrderByDescending(o => o.AccountAoTransactionId).ToList();

            List<MoneyAccountTransactionVm> lstMoneyInmateReferenceTrans = new List<MoneyAccountTransactionVm>();
            var moneyInmateReferenceTrans = _context.AccountAoTransaction.Where(w =>
                    w.AccountAoFund.AccountAoBankId == bankId)
                .Select(s => new
                {
                    s.TransactionDate,
                    s.TransactionAmount,
                    s.TransactionDescription,
                    s.TransactionReceipt,
                    s.TransactionCheckNumber,
                    s.AccountAoFund.FundName,
                    VoidFlag = s.TransactionVoidFlag == 1,
                    InmateId = s.InmateId ?? 0,
                    AccountAoFeeId = s.AccountAoFeeId ?? 0,
                    s.AccountAoTransactionId,
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
                    } : new PersonVm(),
                    HousingUnit = s.Inmate.HousingUnitId > 0 ? new HousingDetail
                    {
                        HousingUnitId = s.Inmate.HousingUnit.HousingUnitId,
                        HousingUnitLocation = s.Inmate.HousingUnit.HousingUnitLocation,
                        HousingUnitNumber = s.Inmate.HousingUnit.HousingUnitNumber,
                        HousingUnitBedNumber = s.Inmate.HousingUnit.HousingUnitBedNumber,
                        HousingUnitBedLocation = s.Inmate.HousingUnit.HousingUnitBedLocation
                    } : new HousingDetail(),
                    s.Inmate.Facility.FacilityAbbr,
                    FundId = s.AccountAoFund.AccountAoFundId,
                    BankId = s.AccountAoFund.AccountAoBankId,
                    s.BalanceAccount,
                    TransactionId = s.AccountAoTransactionId,
                    s.AccountAoReceive.AccountAoDepository.DepositoryName,
                    s.TransactionType,
                    s.AccountAoFund.AccountAoBank.BankAccountAbbr,
                    s.TransactionNotes,
                    VoidDate = s.TransactionVoidDate,
                    ClearDate = s.TransactionClearedDate,
                    s.BalanceInmate,
                    s.BalanceFund,
                    ClearFlag = s.TransactionCleared == 1,
                    ReturnCashFlag = s.TransactionReturnCashFlag == 1,
                    ReceiveFrom = s.TransactionReceiveFrom,
                    s.TransactionPayToTheOrderOf,
                    ReceiveNumber = s.TransactionReceiveNumber,
                    s.TransactionDebitMemo,
                    s.TransactionClearedBy,
                    s.TransactionVoidBy,
                    s.CreatedBy,
                    ClearBy =
                        new PersonnelVm
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
                    PersonnelBy = new PersonnelVm
                    {
                        PersonFirstName = s.CreatedByNavigation.PersonNavigation.PersonFirstName,
                        PersonLastName = s.CreatedByNavigation.PersonNavigation.PersonLastName,
                        OfficerBadgeNumber = s.CreatedByNavigation.OfficerBadgeNum
                    },
                    FundInmateOnlyFlag = s.AccountAoFund.FundInmateOnlyFlag == 1
                }).Distinct().OrderByDescending(a => a.TransactionId);
            List<MoneyAccountTransactionVm> lstMoneyTransactions = moneyInmateReferenceTrans.Select(s => new MoneyAccountTransactionVm
            {
                TransactionDate = s.TransactionDate,
                TransactionAmount = s.TransactionAmount,
                TransactionDescription = s.TransactionDescription,
                TransactionReceipt = s.TransactionReceipt,
                TransactionCheckNumber = s.TransactionCheckNumber,
                FundName = s.FundName,
                VoidFlag = s.VoidFlag,
                InmateId = s.InmateId,
                AccountAoFeeId = s.AccountAoFeeId,
                AccountAoTransactionId = s.AccountAoTransactionId,
                TransactionFromId = s.TransactionFromId,
                TransactionToId = s.TransactionToId,
                Person = s.Person,
                HousingUnit = s.HousingUnit,
                FacilityAbbr = s.FacilityAbbr,
                FundId = s.FundId,
                BankId = s.BankId,
                BalanceAccount = s.BalanceAccount,
                TransactionId = s.AccountAoTransactionId,
                DepositoryName = s.DepositoryName,
                TransactionType = s.TransactionType,
                BankAccountAbbr = s.BankAccountAbbr,
                TransactionNotes = s.TransactionNotes,
                VoidDate = s.VoidDate,
                ClearDate = s.ClearDate,
                BalanceInmate = s.BalanceInmate,
                BalanceFund = s.BalanceFund,
                ClearFlag = s.ClearFlag,
                ReturnCashFlag = s.ReturnCashFlag,
                ReceiveFrom = s.ReceiveFrom,
                TransactionPayToTheOrderOf = s.TransactionPayToTheOrderOf,
                ReceiveNumber = s.ReceiveNumber,
                TransactionDebitMemo = s.TransactionDebitMemo,
                TransactionClearedBy = s.TransactionClearedBy,
                TransactionVoidBy = s.TransactionVoidBy,
                CreatedBy = s.CreatedBy,
                ClearBy = s.ClearBy,

                VoidBy = s.VoidBy,
                PersonnelBy = s.PersonnelBy,
                FundInmateOnlyFlag = s.FundInmateOnlyFlag
            }).ToList();
            List<MoneyAccountTransactionVm> lstMoneyInmateReference = lstMoneyTransactions.Where(w => w.FundInmateOnlyFlag == false
              && w.InmateId == inmateId).ToList();
            lstMoneyInmateReference.ForEach(f =>
            {
                if (f.AccountAoFeeId > 0)
                {
                    lstMoneyInmateReferenceTrans.AddRange(lstMoneyTransactions.Where(w =>
                        w.AccountAoFeeId == f.AccountAoFeeId
                        && w.AccountAoTransactionId != f.AccountAoTransactionId));
                }
                else
                {
                    lstMoneyInmateReferenceTrans.AddRange(lstMoneyTransactions.Where(w =>
                        w.TransactionToId == f.AccountAoTransactionId
                        || w.TransactionFromId == f.AccountAoTransactionId));
                }
            });
            lstMoneyInmateReferenceTrans.AddRange(lstMoneyInmateReference);

            if (inmateLedgerAmount.Count > 0)
            {
                objInmateReference.AccountLedgerBalanceAmount = inmateLedgerAmount.First().BalanceInmate;
            }

            objInmateReference.AccountLedgerDetails = lstMoneyInmateReferenceTrans.Distinct()
                .OrderByDescending(o => o.TransactionId).ToList();

            return objInmateReference;
        }

        #endregion


        public MoneyAccountTransactionVm GetPrintReceive(int receiveId, int inmateId)
        {
            MoneyAccountTransactionVm PendingDetails = new MoneyAccountTransactionVm();
            int accountReceiveId = _context.AccountAoReceive.Single(w =>
                w.InmateId == inmateId && w.TransactionVoidReceiveId == receiveId).AccountAoReceiveId;
            if (accountReceiveId > 0)
            {
                PendingDetails = _context.AccountAoReceive.Where(w => w.AccountAoReceiveId == accountReceiveId).Select(
                    s => new MoneyAccountTransactionVm
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
                            OfficerBadgeNumber = s.TransactionVerifiedByNavigation.OfficerBadgeNum
                        },
                        VoidBy = new PersonnelVm
                        {
                            PersonFirstName = s.TransactionVoidByNavigation.PersonNavigation.PersonFirstName,
                            PersonLastName = s.TransactionVoidByNavigation.PersonNavigation.PersonLastName,
                            OfficerBadgeNumber = s.TransactionVoidByNavigation.OfficerBadgeNum
                        },
                        PersonnelBy = new PersonnelVm
                        {
                            PersonFirstName = s.CreatedByNavigation.PersonNavigation.PersonFirstName,
                            PersonLastName = s.CreatedByNavigation.PersonNavigation.PersonLastName,
                            OfficerBadgeNumber = s.CreatedByNavigation.OfficerBadgeNum
                        },
                        AccountAoDepositoryId = s.AccountAoDepositoryId

                    }
                ).FirstOrDefault();
            }
            return PendingDetails;
        }

    }
}
