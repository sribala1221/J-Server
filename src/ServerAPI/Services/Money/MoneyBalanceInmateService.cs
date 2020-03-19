﻿using GenerateTables.Models;
using Microsoft.AspNetCore.Http;
using ServerAPI.Utilities;
using ServerAPI.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;

// ReSharper disable once CheckNamespace
namespace ServerAPI.Services
{
    public class MoneyBalanceInmateService : IMoneyBalanceInmateService
    {
        private readonly AAtims _context;
        private readonly int _personnelId;
        private readonly IMoneyBalanceDebitCheck _moneyBalanceDebitCheck;
        private readonly IMoneyService _moneyService;
        private readonly MoneyCashCheckVm _response = new MoneyCashCheckVm();
        public MoneyBalanceInmateService(AAtims context,
        IHttpContextAccessor httpContextAccessor,
        IMoneyBalanceDebitCheck moneyBalanceDebitCheck,
        IMoneyService moneyService)
        {
            _context = context;
            _personnelId = Convert.ToInt32(httpContextAccessor.HttpContext.User
                .FindFirst("personnelId")?.Value);
            _moneyBalanceDebitCheck = moneyBalanceDebitCheck;
            _moneyService = moneyService;
        }
        // Money -> Balance -> Inmate
        #region Main grid
        public List<MoneyBalanceInmateVm> MoneyBalanceInmateLoad(int bankId, int inmateStatus, int accountBalanceType, int facilityId)
        {
            List<HousingUnit> housingList = _context.HousingUnit.ToList();
            List<MoneyBalanceInmateVm> balanceInmateDetails = _context.AccountAoInmate
            .Where(w => w.AccountAoBankId == bankId
            && (!(facilityId > 0) || w.Inmate.FacilityId == facilityId))
            .Select(s => new MoneyBalanceInmateVm
            {
                Inmate = new InmateDetail{
                    Person =  new PersonVm{
                        PersonLastName = s.Inmate.Person.PersonLastName,
                        PersonFirstName = s.Inmate.Person.PersonFirstName,
                        PersonMiddleName = s.Inmate.Person.PersonMiddleName,
                        PersonSuffix = s.Inmate.Person.PersonSuffix,
                        PersonId = s.Inmate.PersonId,
                        FacilityId = s.Inmate.FacilityId
                    },
                    InmateActive = s.Inmate.InmateActive == 1,
                    InmateId = s.InmateId,
                    InmateNumber = s.Inmate.InmateNumber,
                    InmateCurrentTrack = s.Inmate.InmateCurrentTrack,
                    HousingUnit = housingList.Where(y => y.HousingUnitId == s.Inmate.HousingUnitId)
                                            .Select(z=> new HousingDetail{
                                                HousingUnitLocation = z.HousingUnitLocation,
                                                HousingUnitNumber = z.HousingUnitNumber,
                                                HousingUnitBedLocation = z.HousingUnitBedLocation,
                                                HousingUnitBedNumber = z.HousingUnitBedNumber,
                                                FacilityAbbr = z.Facility.FacilityAbbr
                                            }).SingleOrDefault()
                },
                BalanceInmate = s.BalanceInmate,
                BalanceInmateFee = s.BalanceInmateFee,
                BalanceInmatePending = s.BalanceInmatePending
            }).ToList();
            if (inmateStatus == 1)
            {
                balanceInmateDetails = balanceInmateDetails.Where(w => w.Inmate.InmateActive).ToList();
            } else if(inmateStatus == 2)
            {
                balanceInmateDetails = balanceInmateDetails.Where(w => !w.Inmate.InmateActive).ToList();
            }
            switch (accountBalanceType)
            {
                case 1:
                    balanceInmateDetails = balanceInmateDetails.Where(w => w.BalanceInmate != 0).ToList();
                    break;
                case 2:
                    balanceInmateDetails = balanceInmateDetails.Where(w => w.BalanceInmateFee != 0).ToList();
                    break;
                case 3:
                    balanceInmateDetails = balanceInmateDetails.Where(w => w.BalanceInmatePending != 0).ToList();
                    break;
            }
            return balanceInmateDetails.OrderBy(o => o.Inmate.Person.PersonLastName).ToList();
        }
        #endregion

        #region Transaction popup

        public MoneyBalanceInmateVm MoneyGetInmateCurrentBalance(int inmateId, int bankId) => _context.Inmate.Where(w => w.InmateId == inmateId)
            .Select(s=> new MoneyBalanceInmateVm{
                Inmate = new InmateDetail {
                    Person =  new PersonVm {
                        PersonLastName = s.Person.PersonLastName,
                        PersonFirstName = s.Person.PersonFirstName,
                        PersonMiddleName = s.Person.PersonMiddleName,
                        PersonSuffix = s.Person.PersonSuffix,
                        PersonId = s.PersonId,
                        FacilityId = s.FacilityId
                    },
                    InmateNumber = s.InmateNumber,
                    HousingUnit = new HousingDetail{
                        HousingUnitLocation = s.HousingUnit != null ? s.HousingUnit.HousingUnitLocation : default,
                        HousingUnitNumber = s.HousingUnit != null ? s.HousingUnit.HousingUnitNumber : default,
                        HousingUnitBedLocation = s.HousingUnit != null ? s.HousingUnit.HousingUnitBedLocation : default,
                        HousingUnitBedNumber = s.HousingUnit != null ? s.HousingUnit.HousingUnitBedNumber : default,
                        FacilityAbbr = s.Facility.FacilityAbbr
                    }
                },
                BalanceInmate = s.AccountAoInmate.Where(w => w.InmateId == inmateId).Select(aai => aai.BalanceInmate).SingleOrDefault(),
                BalanceInmateFee = s.AccountAoInmate.Where(w => w.InmateId == inmateId).Select(aai => aai.BalanceInmateFee).SingleOrDefault(),
                BalanceInmatePending = s.AccountAoInmate.Where(w => w.InmateId == inmateId).Select(aai => aai.BalanceInmatePending).SingleOrDefault()
            }).Single();

        public MoneyTransactionVm MoneyTransactionList(int facilityId, int bankId)
        {
            MoneyTransactionVm objMoneyDetails = new MoneyTransactionVm();
            objMoneyDetails.BankList = _moneyService.MoneyGetBank();
            objMoneyDetails.DepositoryList = _moneyService.MoneyGetDeposit();
            objMoneyDetails.CashDrawerList = _moneyService.MoneyGetCashDrawer(facilityId, false);
            objMoneyDetails.FundList = _moneyService.MoneyGetFund().Where(w => w.FundInmateOnlyFlag == 1)
            .Select(s => new KeyValuePair<string, int>(s.FundName, s.AccountAOFundId)).ToList();
            objMoneyDetails.ToFundList = _moneyService.MoneyGetFund().ToList();
            return objMoneyDetails;
        }

        #endregion

        #region Receive Cash && Receive check  
        public MoneyCashCheckVm MoneyReceiptCashCheckInsert(MoneyCashCheckVm param)
        {
            // Pass Eight 0's as string value in param.ReceiptNo to set prefix
            AccountAoInmate aoInmate = new AccountAoInmate();
            param.ReceiptNo = param.ReceiptNo + MoneyGetNextReciptNo(param.BankId).ToString();
            param.ReceiptNo = param.ReceiptNo.Substring(param.ReceiptNo.Length - 8);
            aoInmate = _context.AccountAoInmate.SingleOrDefault(w =>
                     w.InmateId == param.InmateId && w.AccountAoBankId == param.BankId);
            decimal inmateMaxBalance = _context.AccountAoBank.Find(param.BankId).InmateMaxBalance ?? 0;
            if (aoInmate == null && param.InmateId > 0)
            {
                if(param.TransAmount > inmateMaxBalance)
                {
                    // Inmate maximum balance exceeds;
                    _response.StatusCode = 10;
                    return _response;
                }
                aoInmate.InmateId = param.InmateId;
                aoInmate.AccountAoBankId = param.BankId;
                _context.AccountAoInmate.Add(aoInmate);
                _context.SaveChanges();
            } else if(inmateMaxBalance > 0 
            && (aoInmate.BalanceInmate + aoInmate.BalanceInmatePending + param.TransAmount) > inmateMaxBalance
            && param.InmateId > 0)
            {
                // Inmate maximum balance exceeds;
                _response.StatusCode = 10;
                return _response;
            }
            if (param.IsDepository)
            {
                MoneyTransactionCalculateVm balanceParam = new MoneyTransactionCalculateVm();
                balanceParam.InmateId = param.InmateId;
                balanceParam.TransactionAmount = param.TransAmount;
                balanceParam.BankId = param.BankId;
                balanceParam.FundId = param.FundId;
                balanceParam.TransactionType = MoneyTransactionType.CREDIT;
                balanceParam.Table = MoneyTransactionTable.ACCOUNTAO_RECEIVE;
                MoneyBalanceVm balanceDetails = _moneyBalanceDebitCheck.MoneyTransactionCalculateBalance(balanceParam);
                balanceDetails.InmateId = param.InmateId;
                balanceDetails.BankId = param.BankId;
                balanceDetails.FundId = param.FundId;
                balanceDetails.Table = MoneyTransactionTable.ACCOUNTAO_RECEIVE;
                _moneyBalanceDebitCheck.MoneyUpdateBalance(balanceDetails);
                AccountAoBank aoBank = _context.AccountAoBank.Find(param.BankId);
                aoBank.NextReceiptNum = aoBank.NextReceiptNum + 1;
                AccountAoReceive aoReceive = new AccountAoReceive();
                aoReceive.AccountAoFundId = param.FundId;
                aoReceive.TransactionType = param.TransType.ToString();
                aoReceive.InmateId = param.InmateId != 0 ? param.InmateId : default(int?);
                aoReceive.TransactionReceiveCashFlag = param.CashFlag;
                aoReceive.TransactionDescription = param.TransDescription;
                aoReceive.AccountAoDepositoryId = param.DepositId ?? 0;
                aoReceive.TransactionAmount = param.TransAmount;
                aoReceive.TransactionReceiveFrom = param.RexedFrom;
                aoReceive.TransactionReceipt = param.ReceiptNo;
                aoReceive.CreatedBy = _personnelId;
                aoReceive.CreateDate = DateTime.Now;
                aoReceive.TransactionDate = DateTime.Now;
                aoReceive.BalanceInmatePending = balanceDetails.InmateBalance;
                aoReceive.BalanceFundPending = balanceDetails.FundBalance;
                aoReceive.BalanceAccountPending = balanceDetails.BankBalance;
                aoReceive.TransactionOfficerId = _personnelId;
                aoReceive.TransactionReceiveNumber = param.ReceiveNo;
                aoReceive.UpdatedBy = _personnelId;
                aoReceive.UpdateDate = DateTime.Now;
                aoReceive.TransactionNotes = param.TransactionNotes;
                aoReceive.Currency01Count = param.Currency.Currency01Count;
                aoReceive.Currency05Count = param.Currency.Currency05Count;
                aoReceive.Currency10Count = param.Currency.Currency10Count;
                aoReceive.Currency25Count = param.Currency.Currency25Count;
                aoReceive.Currency50Count = param.Currency.Currency50Count;
                aoReceive.Currency100Count = param.Currency.Currency100Count;
                aoReceive.Currency200Count = param.Currency.Currency200Count;
                aoReceive.Currency500Count = param.Currency.Currency500Count;
                aoReceive.Currency1000Count = param.Currency.Currency1000Count;
                aoReceive.Currency2000Count = param.Currency.Currency2000Count;
                aoReceive.Currency5000Count = param.Currency.Currency5000Count;
                aoReceive.Currency10000Count = param.Currency.Currency10000Count;
                _context.AccountAoReceive.Add(aoReceive);
                _context.SaveChanges();
                _response.ReceiptNo = param.ReceiptNo;
                _response.TransAmount = param.TransAmount;
                _response.TransDescription = param.TransDescription;
                _response.TransactionNumber = aoReceive.AccountAoReceiveId;
                _response.RexedFrom = param.RexedFrom;
                _response.TransactionNotes = param.TransactionNotes;
                _response.ReceiveNo = param.ReceiveNo;
                _response.InmateId = param.InmateId;
                } else
            {
                // Update cash balance
                AccountAoCashBalance aoCashBalance = _context.AccountAoCashBalance.Find(param.cashBalanceId);
                aoCashBalance.CurrentBalance = aoCashBalance.CurrentBalance != null
                ? aoCashBalance.CurrentBalance + param.TransAmount :
                param.TransAmount;
                _context.SaveChanges();
                // Credit cash drawer
                AccountAoCashDrawer aoCash = new AccountAoCashDrawer();
                aoCash.BalanceCashDrawer = aoCashBalance.CurrentBalance??0 + param.TransAmount;
                aoCash.CashBalanceId = aoCashBalance.AccountAoCashBalanceId;
                aoCash.InmateId = param.InmateId != 0 ? param.InmateId : default(int?);
                aoCash.TransactionAmount = param.TransAmount;
                aoCash.TransactionType = MoneyTransactionType.CREDIT.ToString();
                aoCash.TransactionDescription = param.TransDescription;
                aoCash.TransactionNotes = param.TransactionNotes;
                aoCash.TransactionDate = DateTime.Now;
                aoCash.TransactionOfficerId = _personnelId;
                aoCash.CreatedBy = _personnelId;
                aoCash.CreateDate = DateTime.Now;
                aoCash.UpdatedBy = _personnelId;
                aoCash.UpdateDate = DateTime.Now;
                aoCash.Currency01Count = param.Currency.Currency01Count;
                aoCash.Currency05Count = param.Currency.Currency05Count;
                aoCash.Currency10Count = param.Currency.Currency10Count;
                aoCash.Currency25Count = param.Currency.Currency25Count;
                aoCash.Currency50Count = param.Currency.Currency50Count;
                aoCash.Currency100Count = param.Currency.Currency100Count;
                aoCash.Currency200Count = param.Currency.Currency200Count;
                aoCash.Currency500Count = param.Currency.Currency500Count;
                aoCash.Currency1000Count = param.Currency.Currency1000Count;
                aoCash.Currency2000Count = param.Currency.Currency2000Count;
                aoCash.Currency5000Count = param.Currency.Currency5000Count;
                aoCash.Currency10000Count = param.Currency.Currency10000Count;
                _context.AccountAoCashDrawer.Add(aoCash);
                _context.SaveChanges();
                int welfareFundId = _context.AccountAoFund.Single(s => s.FundWelfareOnlyFlag == 1 && s.Inactive == 0
                && s.AccountAoBankId == param.BankId).AccountAoFundId;
                // if welfareFund balance is less than trans amount or zero or null then we have to show error messge.
                MoneyTransactionCalculateVm balanceParam = new MoneyTransactionCalculateVm();
                balanceParam.InmateId = param.InmateId;
                balanceParam.BankId = param.BankId;
                balanceParam.FundId = welfareFundId;
                balanceParam.TransactionAmount = param.TransAmount;
                balanceParam.TransactionType = MoneyTransactionType.DEBIT;
                balanceParam.Table = MoneyTransactionTable.ACCOUNTAO_TRANSACTION;
                MoneyBalanceVm balanceDetails = _moneyBalanceDebitCheck.MoneyTransactionCalculateBalance(balanceParam);
                balanceDetails.InmateId = 0;
                balanceDetails.BankId = param.BankId;
                balanceDetails.FundId = welfareFundId;
                balanceDetails.Table = MoneyTransactionTable.ACCOUNTAO_TRANSACTION;
                _moneyBalanceDebitCheck.MoneyUpdateBalance(balanceDetails);
                AccountAoTransaction aoTransDebit = new AccountAoTransaction();
                aoTransDebit.AccountAoFundId = welfareFundId;
                aoTransDebit.BalanceAccount = balanceDetails.BankBalance;
                aoTransDebit.BalanceFund = balanceDetails.FundBalance;
                aoTransDebit.BalanceInmate = balanceDetails.InmateBalance;
                aoTransDebit.TransactionAmount = -1 * Math.Abs(param.TransAmount);
                aoTransDebit.TransactionType = MoneyTransactionType.DEBIT.ToString();
                aoTransDebit.TransactionDescription = param.TransDescription;
                aoTransDebit.CreatedBy = _personnelId;
                aoTransDebit.CreateDate = DateTime.Now;
                aoTransDebit.TransactionDate = DateTime.Now;
                aoTransDebit.TransactionOfficerId = _personnelId;
                aoTransDebit.UpdatedBy = _personnelId;
                aoTransDebit.UpdateDate = DateTime.Now;
                aoTransDebit.TransactionNotes = param.TransactionNotes;
                aoTransDebit.TransactionReceiveCashFlag = 1;
                _context.AccountAoTransaction.Add(aoTransDebit);
                _context.SaveChanges();
                MoneyTransactionCalculateVm balanceParamCredit = new MoneyTransactionCalculateVm();
                balanceParamCredit.InmateId = param.InmateId;
                balanceParamCredit.BankId = param.BankId;
                balanceParamCredit.FundId = param.FundId;
                balanceParamCredit.TransactionAmount = param.TransAmount;
                balanceParamCredit.TransactionType = MoneyTransactionType.CREDIT;
                balanceParamCredit.Table = MoneyTransactionTable.ACCOUNTAO_TRANSACTION;
                MoneyBalanceVm balanceDetailsCredit = _moneyBalanceDebitCheck.MoneyTransactionCalculateBalance(balanceParamCredit);
                balanceDetailsCredit.InmateId = param.InmateId;
                balanceDetailsCredit.BankId = param.BankId;
                balanceDetailsCredit.FundId = param.FundId;
                balanceDetailsCredit.Table = MoneyTransactionTable.ACCOUNTAO_TRANSACTION;
                _moneyBalanceDebitCheck.MoneyUpdateBalance(balanceDetailsCredit);
                AccountAoTransaction aoTransCredit = new AccountAoTransaction();
                aoTransCredit.AccountAoFundId = param.FundId;
                aoTransCredit.InmateId = param.InmateId == 0 ? default(int?) : param.InmateId;
                aoTransCredit.BalanceAccount = balanceDetailsCredit.BankBalance;
                aoTransCredit.BalanceFund = balanceDetailsCredit.FundBalance;
                aoTransCredit.BalanceInmate = balanceDetailsCredit.InmateBalance;
                aoTransCredit.TransactionAmount = Math.Abs(param.TransAmount);
                aoTransCredit.TransactionType = MoneyTransactionType.CREDIT.ToString();
                aoTransCredit.TransactionDescription = param.TransDescription;
                aoTransCredit.CreatedBy = _personnelId;
                aoTransCredit.CreateDate = DateTime.Now;
                aoTransCredit.TransactionDate = DateTime.Now;
                aoTransCredit.TransactionOfficerId = _personnelId;
                aoTransCredit.UpdatedBy = _personnelId;
                aoTransCredit.UpdateDate = DateTime.Now;
                aoTransCredit.TransactionNotes = param.TransactionNotes;
                aoTransCredit.AccountAoTransactionFromId = aoTransDebit.AccountAoTransactionId;
                aoTransCredit.TransactionReceiveCashFlag = 1;
                _context.AccountAoTransaction.Add(aoTransCredit);
                _context.SaveChanges();
                // Update To id in debit transaction
                aoTransDebit.AccountAoTransactionToId = aoTransCredit.AccountAoTransactionId;
                // Update cash drawer
                aoCash.AccountAoTransactionId = aoTransDebit.AccountAoTransactionId;
                _context.SaveChanges();
                // Before receive cash complete run debt check 
                // Receive Transaction Created
                List<int> feeIds = _context.AccountAoFee
                .Where(w => !w.TransactionCleared.HasValue
                && !w.TransactionVoidFlag.HasValue
                && w.InmateId == param.InmateId)
                .OrderByDescending(od => od.AccountAoFund.FundAllowFeeOrder)
                .ThenBy(o => o.TransactionDate)
                .Select(s => s.AccountAoFeeId)
                .ToList();
                if (feeIds.Count > 0)
                {
                    MoneyDebitCheckVm debitCheck = new MoneyDebitCheckVm();
                    debitCheck.TotalPayAmount = 0;
                    foreach (int f in feeIds)
                    {
                        debitCheck.InmateId = param.InmateId;
                        debitCheck.FeeFlag = 0;
                        debitCheck.DepositFlag = 1;
                        debitCheck.FeeCheckFlag = 0;
                        debitCheck.AccountAoFeeId = f;
                        debitCheck.ThersholdAmount = param.TransAmount;
                        debitCheck = _moneyBalanceDebitCheck.MoneyDebitCheck(debitCheck);
                        if (debitCheck.ReturnExitFlag) break;
                    }
                }
                _response.ReceiptNo = param.ReceiptNo;
                _response.TransAmount = param.TransAmount;
                _response.TransDescription = param.TransDescription;
                _response.TransactionNumber = aoTransCredit.AccountAoTransactionId;
                _response.RexedFrom = param.RexedFrom;
                _response.TransactionNotes = param.TransactionNotes;
                _response.ReceiveNo = param.ReceiveNo;
                _response.InmateId = param.InmateId;
            }
            return _response;
        }
        private int MoneyGetNextReciptNo(int bankId)
        {
            int? nextReceiptNum = _context.AccountAoBank.Find(bankId).NextReceiptNum;
            return nextReceiptNum ?? 0;
        }
        #endregion

        #region Write check && Final write check
        public MoneyCashCheckVm MoneyTransactionWriteCheckInsert(MoneyCashCheckVm param)
        {
            MoneyTransactionCalculateVm balanceParam = new MoneyTransactionCalculateVm();
            balanceParam.InmateId = param.InmateId;
            balanceParam.TransactionAmount = param.TransAmount;
            balanceParam.BankId = param.BankId;
            balanceParam.FundId = param.FundId;
            balanceParam.TransactionType = MoneyTransactionType.DEBIT;
            balanceParam.Table = MoneyTransactionTable.ACCOUNTAO_TRANSACTION;
            MoneyBalanceVm balanceDetails = _moneyBalanceDebitCheck.MoneyTransactionCalculateBalance(balanceParam);
            balanceDetails.InmateId = param.InmateId;
            balanceDetails.BankId = param.BankId;
            balanceDetails.FundId = param.FundId;
            balanceDetails.Table = MoneyTransactionTable.ACCOUNTAO_TRANSACTION;
            _moneyBalanceDebitCheck.MoneyUpdateBalance(balanceDetails);
            AccountAoTransaction aoTransaction = new AccountAoTransaction();
            aoTransaction.InmateId = param.InmateId != 0 ? param.InmateId : default(int?);
            aoTransaction.AccountAoFundId = param.FundId;
            aoTransaction.TransactionType = param.TransType.ToString();
            aoTransaction.TransactionCheckNumber = param.ReceiveNo;
            aoTransaction.TransactionDescription = param.TransDescription;
            aoTransaction.TransactionAmount = -1 * Math.Abs(param.TransAmount);
            aoTransaction.TransactionPayToTheOrderOf = param.PayToOrder;
            aoTransaction.TransactionDebitMemo = param.DebitMemo;
            aoTransaction.BalanceInmate = balanceDetails.InmateBalance;
            aoTransaction.BalanceFund = balanceDetails.FundBalance;
            aoTransaction.BalanceAccount = balanceDetails.BankBalance;
            aoTransaction.CreatedBy = _personnelId;
            aoTransaction.CreateDate = DateTime.Now;
            aoTransaction.TransactionDate = DateTime.Now;
            aoTransaction.TransactionOfficerId = _personnelId;
            aoTransaction.UpdatedBy = _personnelId;
            aoTransaction.UpdateDate = DateTime.Now;
            aoTransaction.TransactionNotes = param.TransactionNotes;
            _context.AccountAoTransaction.Add(aoTransaction);
            _context.SaveChanges();
            // Debit Transaction Success,while create in Transaction table
            _response.TransAmount = param.TransAmount;
            _response.TransDescription = param.TransDescription;
            _response.TransactionNumber = aoTransaction.AccountAoTransactionId;
            _response.TransactionNotes = param.TransactionNotes;
            _response.ReceiveNo = param.ReceiveNo;
            _response.InmateId = param.InmateId;
            _response.TransType = param.TransType;
            _response.PayToOrder = param.PayToOrder;
            _response.DebitMemo = param.DebitMemo;
            _response.CreateDate = aoTransaction.CreateDate;
            _response.CreateBy = _context.Personnel.Where(w => w.PersonnelId == _personnelId).Select(s =>
             new PersonnelVm
             {
                 PersonFirstName = s.PersonNavigation.PersonFirstName,
                 PersonLastName = s.PersonNavigation.PersonLastName,
             }).Single();

            return _response;
        }
        #endregion

        #region  Return cash && Final return cash
        public MoneyCashCheckVm MoneyReturnCash(MoneyCashCheckVm param)
        {
            // debit cash drawer
            AccountAoCashBalance aoCash = _context.AccountAoCashBalance.Find(param.cashBalanceId);
            if (aoCash.CurrentBalance < param.TransAmount)
            {
                _response.StatusCode = 2;
                // Insufficient Fund
                return _response;
            }
            aoCash.CurrentBalance = aoCash.CurrentBalance - param.TransAmount;
            AccountAoCashDrawer aoCashDrawer = new AccountAoCashDrawer();
            aoCashDrawer.InmateId = param.InmateId;
            aoCashDrawer.CashBalanceId = param.cashBalanceId;
            aoCashDrawer.BalanceCashDrawer = aoCash.CurrentBalance ?? 0;
            aoCashDrawer.CreateDate = DateTime.Now;
            aoCashDrawer.CreatedBy = _personnelId;
            aoCashDrawer.UpdateDate = DateTime.Now;
            aoCashDrawer.UpdatedBy = _personnelId;
            aoCashDrawer.TransactionOfficerId = _personnelId;
            aoCashDrawer.TransactionDate = DateTime.Now;
            aoCashDrawer.TransactionAmount = -1 * Math.Abs(param.TransAmount);
            aoCashDrawer.TransactionType = MoneyTransactionType.DEBIT.ToString();
            aoCashDrawer.TransactionDescription = param.TransDescription;
            aoCashDrawer.TransactionNotes = param.TransactionNotes;
            _context.AccountAoCashDrawer.Add(aoCashDrawer);
            _context.SaveChanges();
            // Debit inmate account
            MoneyTransactionCalculateVm balanceParam = new MoneyTransactionCalculateVm();
            balanceParam.InmateId = param.InmateId;
            balanceParam.TransactionAmount = param.TransAmount;
            balanceParam.BankId = param.BankId;
            balanceParam.FundId = param.FundId;
            balanceParam.TransactionType = MoneyTransactionType.DEBIT;
            balanceParam.Table = MoneyTransactionTable.ACCOUNTAO_TRANSACTION;
            MoneyBalanceVm balanceDetails = _moneyBalanceDebitCheck.MoneyTransactionCalculateBalance(balanceParam);
            balanceDetails.InmateId = param.InmateId;
            balanceDetails.BankId = param.BankId;
            balanceDetails.FundId = param.FundId;
            balanceDetails.Table = MoneyTransactionTable.ACCOUNTAO_TRANSACTION;
            _moneyBalanceDebitCheck.MoneyUpdateBalance(balanceDetails);
            AccountAoTransaction aoDebitTransaction = new AccountAoTransaction();
            aoDebitTransaction.InmateId = param.InmateId;
            aoDebitTransaction.AccountAoFundId = param.FundId;
            aoDebitTransaction.TransactionType = MoneyTransactionType.DEBIT.ToString();
            aoDebitTransaction.TransactionDescription = param.TransDescription;
            aoDebitTransaction.TransactionAmount = -1 * Math.Abs(param.TransAmount);
            aoDebitTransaction.BalanceInmate = balanceDetails.InmateBalance;
            aoDebitTransaction.BalanceFund = balanceDetails.FundBalance;
            aoDebitTransaction.BalanceAccount = balanceDetails.BankBalance;
            aoDebitTransaction.CreatedBy = _personnelId;
            aoDebitTransaction.CreateDate = DateTime.Now;
            aoDebitTransaction.TransactionDate = DateTime.Now;
            aoDebitTransaction.TransactionOfficerId = _personnelId;
            aoDebitTransaction.TransactionReturnCashFlag = 1;
            aoDebitTransaction.UpdatedBy = _personnelId;
            aoDebitTransaction.UpdateDate = DateTime.Now;
            aoDebitTransaction.TransactionNotes = param.TransactionNotes;
            _context.AccountAoTransaction.Add(aoDebitTransaction);
            _context.SaveChanges();
            // From Transaction Created Successfully
            aoCashDrawer.AccountAoTransactionId = aoDebitTransaction.AccountAoTransactionId;
            _context.SaveChanges();
            int toFundId = _context.AccountAoFund.Single(w =>
            w.FundWelfareOnlyFlag == 1
            && w.Inactive == 0
            && w.AccountAoBankId == param.BankId).AccountAoFundId;
            // credit welfare fund
            MoneyTransactionCalculateVm balanceCreditParam = new MoneyTransactionCalculateVm();
            balanceCreditParam.InmateId = param.InmateId;
            balanceCreditParam.TransactionAmount = param.TransAmount;
            balanceCreditParam.BankId = param.BankId;
            balanceCreditParam.FundId = toFundId;
            balanceCreditParam.TransactionType = MoneyTransactionType.CREDIT;
            balanceCreditParam.Table = MoneyTransactionTable.ACCOUNTAO_TRANSACTION;
            MoneyBalanceVm balanceCreditDetails = _moneyBalanceDebitCheck.MoneyTransactionCalculateBalance(balanceCreditParam);
            balanceCreditDetails.BankId = param.BankId;
            balanceCreditDetails.FundId = toFundId;
            balanceCreditDetails.Table = MoneyTransactionTable.ACCOUNTAO_TRANSACTION;
            _moneyBalanceDebitCheck.MoneyUpdateBalance(balanceCreditDetails);
            AccountAoTransaction aoCreditTransaction = new AccountAoTransaction();
            aoCreditTransaction.InmateId = param.InmateId;
            aoCreditTransaction.AccountAoFundId = toFundId;
            aoCreditTransaction.TransactionType = MoneyTransactionType.CREDIT.ToString();
            aoCreditTransaction.TransactionDescription = param.TransDescription;
            aoCreditTransaction.TransactionAmount = Math.Abs(param.TransAmount);
            aoCreditTransaction.BalanceInmate = 0;
            aoCreditTransaction.BalanceFund = balanceCreditDetails.FundBalance;
            aoCreditTransaction.BalanceAccount = balanceCreditDetails.BankBalance;
            aoCreditTransaction.CreatedBy = _personnelId;
            aoCreditTransaction.CreateDate = DateTime.Now;
            aoCreditTransaction.TransactionDate = DateTime.Now;
            aoCreditTransaction.TransactionOfficerId = _personnelId;
            aoCreditTransaction.AccountAoTransactionFromId = aoDebitTransaction.AccountAoTransactionId;
            aoCreditTransaction.TransactionReturnCashFlag = 1;
            aoCreditTransaction.UpdatedBy = _personnelId;
            aoCreditTransaction.UpdateDate = DateTime.Now;
            aoCreditTransaction.TransactionNotes = param.TransactionNotes;
            _context.AccountAoTransaction.Add(aoCreditTransaction);
            _context.SaveChanges();
            // To Transaction Created Successfully
            aoDebitTransaction.AccountAoTransactionToId = aoCreditTransaction.AccountAoTransactionId;
            _context.SaveChanges();
            aoCashDrawer.AccountAoTransactionId = aoCreditTransaction.AccountAoTransactionId;
            _context.SaveChanges();
            _response.TransAmount = param.TransAmount;
            _response.TransDescription = param.TransDescription;
            _response.TransactionNumber = aoCreditTransaction.AccountAoTransactionId;
            _response.TransactionNotes = param.TransactionNotes;
            _response.CashBalanceName = aoCash.Name;
            _response.FacilityAbbr = _context.Facility.Find(aoCash.FacilityId).FacilityAbbr;
            return _response;
        }
        #endregion

        #region Purchase
        public MoneyCashCheckVm MoneyPurchase(MoneyCashCheckVm param)
        {
            // Debit inmate account
            MoneyTransactionCalculateVm balanceParam = new MoneyTransactionCalculateVm();
            balanceParam.InmateId = param.InmateId;
            balanceParam.TransactionAmount = param.TransAmount;
            balanceParam.BankId = param.BankId;
            balanceParam.FundId = param.FundId;
            balanceParam.TransactionType = MoneyTransactionType.DEBIT;
            balanceParam.Table = MoneyTransactionTable.ACCOUNTAO_TRANSACTION;
            MoneyBalanceVm balanceDetails = _moneyBalanceDebitCheck.MoneyTransactionCalculateBalance(balanceParam);
            balanceDetails.InmateId = param.InmateId;
            balanceDetails.BankId = param.BankId;
            balanceDetails.FundId = param.FundId;
            balanceDetails.Table = MoneyTransactionTable.ACCOUNTAO_TRANSACTION;
            _moneyBalanceDebitCheck.MoneyUpdateBalance(balanceDetails);
            AccountAoTransaction aoDebitTransaction = new AccountAoTransaction();
            aoDebitTransaction.AccountAoFundId = param.FundId;
            aoDebitTransaction.InmateId = param.InmateId;
            aoDebitTransaction.TransactionType = MoneyTransactionType.DEBIT.ToString();
            aoDebitTransaction.TransactionDescription = param.TransDescription;
            aoDebitTransaction.TransactionAmount = -1 * Math.Abs(param.TransAmount);
            aoDebitTransaction.BalanceInmate = balanceDetails.InmateBalance;
            aoDebitTransaction.BalanceFund = balanceDetails.FundBalance;
            aoDebitTransaction.BalanceAccount = balanceDetails.BankBalance;
            aoDebitTransaction.CreateDate = DateTime.Now;
            aoDebitTransaction.CreatedBy = _personnelId;
            aoDebitTransaction.TransactionDate = DateTime.Now;
            aoDebitTransaction.TransactionOfficerId = _personnelId;
            aoDebitTransaction.UpdateDate = DateTime.Now;
            aoDebitTransaction.UpdatedBy = _personnelId;
            aoDebitTransaction.TransactionNotes = param.TransactionNotes;
            _context.AccountAoTransaction.Add(aoDebitTransaction);
            _context.SaveChanges();
            //  from transaction successful
            int toBankId = _context.AccountAoFund.Find(param.ToFundId).AccountAoBankId;
            // Credit purchase fund
            MoneyTransactionCalculateVm balanceCreditParam = new MoneyTransactionCalculateVm();
            balanceCreditParam.InmateId = param.InmateId;
            balanceCreditParam.TransactionAmount = param.TransAmount;
            balanceCreditParam.BankId = toBankId;
            balanceCreditParam.FundId = param.ToFundId;
            balanceCreditParam.TransactionType = MoneyTransactionType.CREDIT;
            balanceCreditParam.Table = MoneyTransactionTable.ACCOUNTAO_TRANSACTION;
            MoneyBalanceVm balanceCreditDetails = _moneyBalanceDebitCheck.MoneyTransactionCalculateBalance(balanceCreditParam);
            balanceCreditDetails.BankId = toBankId;
            balanceCreditDetails.FundId = param.ToFundId;
            balanceCreditDetails.Table = MoneyTransactionTable.ACCOUNTAO_TRANSACTION;
            _moneyBalanceDebitCheck.MoneyUpdateBalance(balanceCreditDetails);
            AccountAoTransaction aoCreditTransaction = new AccountAoTransaction();
            aoCreditTransaction.AccountAoFundId = param.ToFundId;
            aoCreditTransaction.InmateId = param.InmateId;
            aoCreditTransaction.TransactionType = MoneyTransactionType.CREDIT.ToString();
            aoCreditTransaction.TransactionDescription = param.TransDescription;
            aoCreditTransaction.TransactionAmount = Math.Abs(param.TransAmount);
            aoCreditTransaction.BalanceInmate = 0;
            aoCreditTransaction.BalanceFund = balanceCreditDetails.FundBalance;
            aoCreditTransaction.BalanceAccount = balanceCreditDetails.BankBalance;
            aoCreditTransaction.CreateDate = DateTime.Now;
            aoCreditTransaction.CreatedBy = _personnelId;
            aoCreditTransaction.TransactionDate = DateTime.Now;
            aoCreditTransaction.TransactionOfficerId = _personnelId;
            aoCreditTransaction.UpdateDate = DateTime.Now;
            aoCreditTransaction.UpdatedBy = _personnelId;
            aoCreditTransaction.TransactionNotes = param.TransactionNotes;
            aoCreditTransaction.AccountAoTransactionFromId = aoDebitTransaction.AccountAoTransactionId;
            _context.AccountAoTransaction.Add(aoCreditTransaction);
            _context.SaveChanges();
            // to transaction successfull              
            // to trsanction id updated
            aoDebitTransaction.AccountAoTransactionToId = aoCreditTransaction.AccountAoTransactionId;
            _context.SaveChanges();
            return _response;
        }
        #endregion

        #region Apply Fee
        public MoneyCashCheckVm MoneyAppFee(MoneyCashCheckVm param)
        {
            AccountAoInmate aoInmate = _context.AccountAoInmate
            .Where(w => w.InmateId == param.InmateId && w.AccountAoBankId == param.BankId)
            .SingleOrDefault();
            if (aoInmate == null)
            {
                AccountAoInmate aoNewInmate = new AccountAoInmate();
                aoNewInmate.InmateId = param.InmateId;
                aoNewInmate.AccountAoBankId = param.BankId;
                _context.AccountAoInmate.Add(aoNewInmate);
                _context.SaveChanges();
            }
            MoneyTransactionCalculateVm objAoFeeCreditParam = new MoneyTransactionCalculateVm();
            objAoFeeCreditParam.InmateId = param.InmateId;
            objAoFeeCreditParam.TransactionAmount = param.TransAmount;
            objAoFeeCreditParam.BankId = param.BankId;
            objAoFeeCreditParam.FundId = param.ToFundId;
            objAoFeeCreditParam.TransactionType = MoneyTransactionType.CREDIT;
            objAoFeeCreditParam.Table = MoneyTransactionTable.ACCOUNTAO_FEE;
            MoneyBalanceVm objAoFeeCreditBalance = _moneyBalanceDebitCheck.MoneyTransactionCalculateBalance(objAoFeeCreditParam);
            objAoFeeCreditBalance.InmateId = param.InmateId;
            objAoFeeCreditBalance.BankId = param.BankId;
            objAoFeeCreditBalance.FundId = param.ToFundId;
            objAoFeeCreditBalance.Table = MoneyTransactionTable.ACCOUNTAO_FEE;
            _moneyBalanceDebitCheck.MoneyUpdateBalance(objAoFeeCreditBalance);
            AccountAoFee aoNewFee = new AccountAoFee();
            aoNewFee.InmateId = param.InmateId;
            aoNewFee.AccountAoFundId = param.ToFundId;
            aoNewFee.CreateDate = DateTime.Now;
            aoNewFee.CreatedBy = _personnelId;
            aoNewFee.TransactionDate = DateTime.Now;
            aoNewFee.TransactionOfficerId = _personnelId;
            aoNewFee.TransactionDescription = param.TransDescription;
            aoNewFee.TransactionFee = param.TransAmount;
            aoNewFee.BalanceInmateFee = objAoFeeCreditBalance.InmateBalance;
            aoNewFee.BalanceFundFee = objAoFeeCreditBalance.FundBalance;
            aoNewFee.BalanceAccountFee = objAoFeeCreditBalance.BankBalance;
            aoNewFee.UpdateDate = DateTime.Now;
            aoNewFee.UpdatedBy = _personnelId;
            aoNewFee.AccountAoFeeTypeId = param.FeeTypeId;
            aoNewFee.TransactionNotes = param.TransactionNotes;
            _context.AccountAoFee.Add(aoNewFee);
            _context.SaveChanges();
            // Saved Successfully
            // Debt check
            MoneyDebitCheckVm debitCheckParam = new MoneyDebitCheckVm();
            debitCheckParam.InmateId = param.InmateId;
            debitCheckParam.FeeFlag = 1;
            debitCheckParam.AccountAoFeeId = aoNewFee.AccountAoFeeId;
            MoneyDebitCheckVm debtResponse = _moneyBalanceDebitCheck.MoneyDebitCheck(debitCheckParam);
            _response.StatusCode = debtResponse.StatusCode;
            _response.PartialPayAmount = debtResponse.PartialPayAmount;
            _response.NewFee = debtResponse.NewFee;
            return _response;
        }
        #endregion

        #region  Donate
        public MoneyCashCheckVm MoneyDonate(MoneyCashCheckVm param)
        {
            MoneyTransactionCalculateVm balanceParam = new MoneyTransactionCalculateVm();
            balanceParam.InmateId = param.InmateId;
            balanceParam.TransactionAmount = param.TransAmount;
            balanceParam.BankId = param.BankId;
            balanceParam.FundId = param.FundId;
            balanceParam.TransactionType = MoneyTransactionType.DEBIT;
            balanceParam.Table = MoneyTransactionTable.ACCOUNTAO_TRANSACTION;
            MoneyBalanceVm balanceDetails = _moneyBalanceDebitCheck.MoneyTransactionCalculateBalance(balanceParam);
            balanceDetails.InmateId = param.InmateId;
            balanceDetails.BankId = param.BankId;
            balanceDetails.FundId = param.FundId;
            balanceDetails.Table = MoneyTransactionTable.ACCOUNTAO_TRANSACTION;
            _moneyBalanceDebitCheck.MoneyUpdateBalance(balanceDetails);
            AccountAoTransaction aoTransaction = new AccountAoTransaction();
            aoTransaction.AccountAoFundId = param.FundId;
            aoTransaction.InmateId = param.InmateId;
            aoTransaction.TransactionType = MoneyTransactionType.DEBIT.ToString();
            aoTransaction.TransactionDescription = param.TransDescription;
            aoTransaction.TransactionAmount = -1 * Math.Abs(param.TransAmount);
            aoTransaction.BalanceInmate = balanceDetails.InmateBalance;
            aoTransaction.BalanceFund = balanceDetails.FundBalance;
            aoTransaction.BalanceAccount = balanceDetails.BankBalance;
            aoTransaction.CreateDate = DateTime.Now;
            aoTransaction.CreatedBy = _personnelId;
            aoTransaction.TransactionDate = DateTime.Now;
            aoTransaction.TransactionOfficerId = _personnelId;
            aoTransaction.UpdateDate = DateTime.Now;
            aoTransaction.UpdatedBy = _personnelId;
            aoTransaction.TransactionNotes = param.TransactionNotes;
            _context.AccountAoTransaction.Add(aoTransaction);
            _context.SaveChanges();
            int toBankId = _context.AccountAoFund.Find(param.ToFundId).AccountAoBankId;
            MoneyTransactionCalculateVm balanceCreditParam = new MoneyTransactionCalculateVm();
            balanceCreditParam.InmateId = param.InmateId;
            balanceCreditParam.TransactionAmount = param.TransAmount;
            balanceCreditParam.BankId = toBankId;
            balanceCreditParam.FundId = param.ToFundId;
            balanceCreditParam.TransactionType = MoneyTransactionType.CREDIT;
            balanceCreditParam.Table = MoneyTransactionTable.ACCOUNTAO_TRANSACTION;
            MoneyBalanceVm balanceCreditDetails = _moneyBalanceDebitCheck.MoneyTransactionCalculateBalance(balanceCreditParam);
            balanceCreditDetails.BankId = toBankId;
            balanceCreditDetails.FundId = param.ToFundId;
            balanceCreditDetails.Table = MoneyTransactionTable.ACCOUNTAO_TRANSACTION;
            _moneyBalanceDebitCheck.MoneyUpdateBalance(balanceCreditDetails);
            AccountAoTransaction objAoTrans = new AccountAoTransaction();
            objAoTrans.AccountAoFundId = param.ToFundId;
            objAoTrans.InmateId = param.InmateId;
            objAoTrans.TransactionType = MoneyTransactionType.CREDIT.ToString();
            objAoTrans.TransactionDescription = param.TransDescription;
            objAoTrans.TransactionAmount = Math.Abs(param.TransAmount);
            objAoTrans.BalanceInmate = 0;
            objAoTrans.BalanceFund = balanceCreditDetails.FundBalance;
            objAoTrans.BalanceAccount = balanceCreditDetails.BankBalance;
            objAoTrans.CreateDate = DateTime.Now;
            objAoTrans.CreatedBy = _personnelId;
            objAoTrans.TransactionDate = DateTime.Now;
            objAoTrans.TransactionOfficerId = _personnelId;
            objAoTrans.UpdateDate = DateTime.Now;
            objAoTrans.UpdatedBy = _personnelId;
            objAoTrans.TransactionNotes = param.TransactionNotes;
            objAoTrans.AccountAoTransactionFromId = aoTransaction.AccountAoTransactionId;
            _context.AccountAoTransaction.Add(objAoTrans);
            _context.SaveChanges();
            AccountAoTransaction updateObjAoTrans = _context.AccountAoTransaction.Find(aoTransaction.AccountAoTransactionId);
            updateObjAoTrans.AccountAoTransactionToId = objAoTrans.AccountAoTransactionId;
            _context.SaveChanges();
            return _response;
        }
        #endregion

        #region  Refund
        public MoneyCashCheckVm MoneyRefund(MoneyCashCheckVm param)
        {
            // From client side we pass inmate fund id in param.FundId(To fund) 
            // and purchase fund id in param.ToFundId(From fund)
            int fromFundId = param.ToFundId;
            int toFundId = param.FundId;

            AccountAoFund aoFund = _context.AccountAoFund
            .Single(w => w.AccountAoFundId == fromFundId && w.AccountAoBankId == param.BankId);

            if (!aoFund.FundWelfareOnlyFlag.HasValue || aoFund.FundWelfareOnlyFlag == 0)
            {
                decimal minBalance = aoFund.BalanceFund ?? 0;
                if (minBalance < param.TransAmount)
                {
                    _response.StatusCode = 2;
                    // Insufficient Fund
                    return _response;
                }
            }
            MoneyTransactionCalculateVm aoTransParam = new MoneyTransactionCalculateVm();
            aoTransParam.InmateId = param.InmateId;
            aoTransParam.TransactionAmount = param.TransAmount;
            aoTransParam.BankId = param.BankId;
            aoTransParam.FundId = fromFundId;
            aoTransParam.TransactionType = MoneyTransactionType.DEBIT;
            aoTransParam.Table = MoneyTransactionTable.ACCOUNTAO_TRANSACTION;
            MoneyBalanceVm aoTransBalance = _moneyBalanceDebitCheck.MoneyTransactionCalculateBalance(aoTransParam);
            aoTransBalance.BankId = param.BankId;
            aoTransBalance.FundId = fromFundId;
            aoTransBalance.Table = MoneyTransactionTable.ACCOUNTAO_TRANSACTION;
            _moneyBalanceDebitCheck.MoneyUpdateBalance(aoTransBalance);
            AccountAoTransaction aoTransaction = new AccountAoTransaction();
            aoTransaction.AccountAoFundId = fromFundId;
            aoTransaction.InmateId = param.InmateId;
            aoTransaction.TransactionType = MoneyTransactionType.DEBIT.ToString();
            aoTransaction.TransactionDescription = param.TransDescription;
            aoTransaction.TransactionAmount = -1 * Math.Abs(param.TransAmount);
            aoTransaction.BalanceInmate = 0;
            aoTransaction.BalanceFund = aoTransBalance.FundBalance;
            aoTransaction.BalanceAccount = aoTransBalance.BankBalance;
            aoTransaction.CreateDate = DateTime.Now;
            aoTransaction.CreatedBy = _personnelId;
            aoTransaction.TransactionDate = DateTime.Now;
            aoTransaction.TransactionOfficerId = _personnelId;
            aoTransaction.UpdateDate = DateTime.Now;
            aoTransaction.UpdatedBy = _personnelId;
            aoTransaction.TransactionNotes = param.TransactionNotes;
            _context.AccountAoTransaction.Add(aoTransaction);
            _context.SaveChanges();
            AccountAoInmate aoInmate = _context.AccountAoInmate
            .Where(w => w.InmateId == param.InmateId && w.AccountAoBankId == param.BankId)
            .SingleOrDefault();
            if (aoInmate == null)
            {
                AccountAoInmate aoNewInmate = new AccountAoInmate();
                aoNewInmate.InmateId = param.InmateId;
                aoNewInmate.AccountAoBankId = param.BankId;
                _context.AccountAoInmate.Add(aoNewInmate);
                _context.SaveChanges();
            }
            int toBankId = _context.AccountAoFund.Find(toFundId).AccountAoBankId;
            MoneyTransactionCalculateVm aoTransCreditParam = new MoneyTransactionCalculateVm();
            aoTransCreditParam.InmateId = param.InmateId;
            aoTransCreditParam.TransactionAmount = param.TransAmount;
            aoTransCreditParam.BankId = toBankId;
            aoTransCreditParam.FundId = toFundId;
            aoTransCreditParam.TransactionType = MoneyTransactionType.CREDIT;
            aoTransCreditParam.Table = MoneyTransactionTable.ACCOUNTAO_TRANSACTION;
            MoneyBalanceVm aoTransCreditBalance = _moneyBalanceDebitCheck.MoneyTransactionCalculateBalance(aoTransCreditParam);
            aoTransCreditBalance.InmateId = param.InmateId;
            aoTransCreditBalance.BankId = toBankId;
            aoTransCreditBalance.FundId = toFundId;
            aoTransCreditBalance.Table = MoneyTransactionTable.ACCOUNTAO_TRANSACTION;
            _moneyBalanceDebitCheck.MoneyUpdateBalance(aoTransCreditBalance);
            AccountAoTransaction aoCreditTransaction = new AccountAoTransaction();
            aoCreditTransaction.AccountAoFundId = toFundId;
            aoCreditTransaction.InmateId = param.InmateId;
            aoCreditTransaction.TransactionType = MoneyTransactionType.CREDIT.ToString();
            aoCreditTransaction.TransactionDescription = param.TransDescription;
            aoCreditTransaction.TransactionAmount = Math.Abs(param.TransAmount);
            aoCreditTransaction.BalanceInmate = aoTransCreditBalance.InmateBalance;
            aoCreditTransaction.BalanceFund = aoTransCreditBalance.FundBalance;
            aoCreditTransaction.BalanceAccount = aoTransCreditBalance.BankBalance;
            aoCreditTransaction.CreateDate = DateTime.Now;
            aoCreditTransaction.CreatedBy = _personnelId;
            aoCreditTransaction.TransactionDate = DateTime.Now;
            aoCreditTransaction.TransactionOfficerId = _personnelId;
            aoCreditTransaction.AccountAoTransactionFromId = aoTransaction.AccountAoTransactionId;
            aoCreditTransaction.UpdateDate = DateTime.Now;
            aoCreditTransaction.UpdatedBy = _personnelId;
            aoCreditTransaction.TransactionNotes = param.TransactionNotes;
            _context.AccountAoTransaction.Add(aoCreditTransaction);
            _context.SaveChanges();
            AccountAoTransaction objAoTrans = _context.AccountAoTransaction.Find(aoTransaction.AccountAoTransactionId);
            objAoTrans.AccountAoTransactionToId = aoCreditTransaction.AccountAoTransactionId;
            _context.SaveChanges();

            List<int> feeIds = _context.AccountAoFee
            .Where(w => !w.TransactionCleared.HasValue && !w.TransactionVoidFlag.HasValue && w.InmateId == param.InmateId)
            .OrderByDescending(od => od.AccountAoFund.FundAllowFeeOrder)
            .ThenBy(o => o.TransactionDate)
            .Select(s => s.AccountAoFeeId)
            .ToList();
            MoneyDebitCheckVm debitCheck = new MoneyDebitCheckVm();
            debitCheck.TotalPayAmount = 0;
            foreach (int f in feeIds)
            {
                debitCheck.InmateId = param.InmateId;
                debitCheck.FeeFlag = 0;
                debitCheck.DepositFlag = 1;
                debitCheck.FeeCheckFlag = 0;
                debitCheck.AccountAoFeeId = f;
                debitCheck.ThersholdAmount = param.TransAmount;
                debitCheck = _moneyBalanceDebitCheck.MoneyDebitCheck(debitCheck);
                if (debitCheck.ReturnExitFlag) break;
            }

            _response.StatusCode = debitCheck.StatusCode;
            return _response;
        }
        #endregion

        #region Journal
        public MoneyCashCheckVm MoneyJournal(MoneyCashCheckVm param)
        {
            AccountAoInmate aoInmate = _context.AccountAoInmate
                    .Where(w => w.InmateId == param.InmateId && w.AccountAoBankId == param.BankId)
                    .SingleOrDefault();
            if (aoInmate == null && param.InmateId > 0)
            {
                AccountAoInmate aoNewInmate = new AccountAoInmate();
                aoNewInmate.InmateId = param.InmateId;
                aoNewInmate.AccountAoBankId = param.BankId;
                _context.AccountAoInmate.Add(aoNewInmate);
                _context.SaveChanges();
            }
            MoneyTransactionCalculateVm aoTransParam = new MoneyTransactionCalculateVm();
            aoTransParam.InmateId = param.InmateId;
            aoTransParam.TransactionAmount = param.TransAmount;
            aoTransParam.BankId = param.BankId;
            aoTransParam.FundId = param.FundId;
            aoTransParam.TransactionType = param.TransType;
            aoTransParam.Table = MoneyTransactionTable.ACCOUNTAO_TRANSACTION;
            MoneyBalanceVm aoTransBalance = _moneyBalanceDebitCheck.MoneyTransactionCalculateBalance(aoTransParam);
            aoTransBalance.InmateId = param.InmateId;
            aoTransBalance.BankId = param.BankId;
            aoTransBalance.FundId = param.FundId;
            aoTransBalance.Table = MoneyTransactionTable.ACCOUNTAO_TRANSACTION;
            _moneyBalanceDebitCheck.MoneyUpdateBalance(aoTransBalance);
            AccountAoTransaction aoTransaction = new AccountAoTransaction();
            aoTransaction.InmateId = param.InmateId != 0 ? param.InmateId : default(int?);
            aoTransaction.AccountAoFundId = param.FundId;
            aoTransaction.BalanceInmate = aoTransBalance.InmateBalance;
            aoTransaction.BalanceFund = aoTransBalance.FundBalance;
            aoTransaction.BalanceAccount = aoTransBalance.BankBalance;
            aoTransaction.CreateDate = DateTime.Now;
            aoTransaction.CreatedBy = _personnelId;
            aoTransaction.TransactionDate = DateTime.Now;
            aoTransaction.TransactionOfficerId = _personnelId;
            aoTransaction.TransactionType = MoneyConstants.JOURNAL;
            aoTransaction.TransactionDescription = param.TransDescription;
            aoTransaction.TransactionAmount = param.TransAmount;
            aoTransaction.UpdateDate = DateTime.Now;
            aoTransaction.UpdatedBy = _personnelId;
            aoTransaction.TransactionNotes = param.TransactionNotes;
            _context.AccountAoTransaction.Add(aoTransaction);
            _context.SaveChanges();
            List<int> feeIds = _context.AccountAoFee
            .Where(w => !w.TransactionCleared.HasValue && !w.TransactionVoidFlag.HasValue && w.InmateId == param.InmateId)
            .OrderByDescending(od => od.AccountAoFund.FundAllowFeeOrder)
            .ThenBy(o => o.TransactionDate)
            .Select(s => s.AccountAoFeeId)
            .ToList();
            MoneyDebitCheckVm debitCheckParam = new MoneyDebitCheckVm();
            debitCheckParam.TotalPayAmount = 0;
            foreach (int f in feeIds)
            {
                debitCheckParam.InmateId = param.InmateId;
                debitCheckParam.FeeFlag = 0;
                debitCheckParam.DepositFlag = 1;
                debitCheckParam.FeeCheckFlag = 0;
                debitCheckParam.AccountAoFeeId = f;
                debitCheckParam.ThersholdAmount = param.TransAmount;
                debitCheckParam = _moneyBalanceDebitCheck.MoneyDebitCheck(debitCheckParam);
                if (debitCheckParam.ReturnExitFlag) break;
            }

            return _response;
        }
        #endregion

        #region RunFeeCheck
        public List<MoneyLedgerVm> MoneyInmateLedgerTransaction(int flag, int bankId, int inmateId)
        {
            List<MoneyLedgerVm> accountAoList = new List<MoneyLedgerVm>();
            switch (flag)
            {
                case 0:
                    accountAoList = _context.AccountAoReceive
                        .Where(w =>
                        w.AccountAoFund.FundInmateOnlyFlag == 1
                        && (!w.TransactionVerified.HasValue || w.TransactionVerified == 0)
                        && (!w.TransactionVoidFlag.HasValue || w.TransactionVoidFlag == 0)
                        && w.AccountAoFund.AccountAoBankId == bankId
                        && w.InmateId == inmateId)
                    .Select(s=> new MoneyLedgerVm{
                            AccountAoReceiveId = s.AccountAoReceiveId,
                            TransactionDate = s.TransactionDate,
                            TransactionAmount = s.TransactionAmount ?? 0,
                            TransactionDesc = s.TransactionDescription,
                            ReceiptNo = s.TransactionReceipt,
                            VoidFlag = s.TransactionVoidFlag ?? 0,
                            TransNotes = s.TransactionNotes,
                            InmateId = s.InmateId ?? 0
                        }).OrderByDescending(o => o.AccountAoReceiveId).ToList();
                    break;
                case 1:
                    accountAoList = _context.AccountAoFee
                        .Where(w =>
                        (!w.TransactionCleared.HasValue || w.TransactionCleared == 0)
                        && (!w.TransactionVoidFlag.HasValue || w.TransactionVoidFlag == 0)
                        && w.AccountAoFund.AccountAoBankId == bankId
                        && w.InmateId == inmateId)
                    .Select(s=> new MoneyLedgerVm{
                            AccountAoFeeId = s.AccountAoFeeId,
                            TransactionDate = s.TransactionDate,
                            TransactionAmount = s.TransactionFee ?? 0,
                            TransactionDesc = s.TransactionDescription,
                            VoidFlag = s.TransactionVoidFlag ?? 0,
                            TransNotes = s.TransactionNotes,
                            InmateId = s.InmateId ?? 0,
                            FundName = s.AccountAoFund.FundName
                        }).OrderByDescending(o => o.AccountAoFeeId).ToList();
                    break;
                case 2:
                    accountAoList = _context.AccountAoTransaction
                        .Where(w =>
                        w.AccountAoFund.FundInmateOnlyFlag == 1
                        && w.AccountAoFund.AccountAoBankId == bankId
                        && w.InmateId == inmateId)
                    .Select(s=> new MoneyLedgerVm{
                            AccountAoTransactionId = s.AccountAoTransactionId,
                            TransactionDate = s.TransactionDate,
                            TransactionAmount = s.TransactionAmount,
                            TransactionDesc = s.TransactionDescription,
                            ReceiptNo = s.TransactionReceipt,
                            CheckNo = s.TransactionCheckNumber,
                            AccountAoReceiveId = s.AccountAoReceiveId ?? 0,
                            CashFlag = s.TransactionReceiveCashFlag ?? 0,
                            VoidFlag = s.TransactionVoidFlag ?? 0,
                            TransNotes = s.TransactionNotes,
                            InmateId = s.InmateId ?? 0
                        }).OrderByDescending(o => o.AccountAoTransactionId).ToList();
                    break;
            }
            return accountAoList;
        }

        public List<MoneyCashCheckVm> MoneyRunFeeCheck(List<MoneyLedgerVm> aoFeeList)
        {
            List<MoneyCashCheckVm> responseList = new List<MoneyCashCheckVm>();
            MoneyDebitCheckVm debitCheckParam = new MoneyDebitCheckVm();
            debitCheckParam.TotalPayAmount = 0;
            foreach (MoneyLedgerVm f in aoFeeList)
            {
                MoneyCashCheckVm debtResponse = new MoneyCashCheckVm();
                debitCheckParam.InmateId = f.InmateId;
                debitCheckParam.FeeFlag = 0;
                debitCheckParam.DepositFlag = 0;
                debitCheckParam.FeeCheckFlag = 1;
                debitCheckParam.AccountAoFeeId = f.AccountAoFeeId;
                debitCheckParam.ThersholdAmount = f.PayAmount;
                debitCheckParam = _moneyBalanceDebitCheck.MoneyDebitCheck(debitCheckParam);
                debtResponse.TransAmount = f.PayAmount;
                debtResponse.PartialPayAmount = debitCheckParam.PartialPayAmount;
                debtResponse.NewFee = debitCheckParam.NewFee;
                debtResponse.FundName = f.FundName;
                debtResponse.StatusCode = debitCheckParam.StatusCode;
                responseList.Add(debtResponse);
                if (debitCheckParam.ReturnExitFlag) break;
            }

            return responseList;
        }
        #endregion

        #region workstation
        public MoneyTransactionVm WorkstationMoneyDetailList(int facilityId)
        {
            MoneyTransactionVm objMoneyDetails = new MoneyTransactionVm();
            objMoneyDetails.BankList = _moneyService.MoneyGetBank();
            objMoneyDetails.DepositoryList = _moneyService.MoneyGetDeposit();
            objMoneyDetails.CashDrawerList = _moneyService.MoneyGetCashDrawer(facilityId, true);
            return objMoneyDetails;
        }
        #endregion
    }
}
