using System;
using GenerateTables.Models;
using ServerAPI.ViewModels;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;

// ReSharper disable once CheckNamespace
namespace ServerAPI.Services
{
    public class MoneyDepositVerifyService : IMoneyDepositVerifyService
    {
        private readonly AAtims _context;
        private readonly int _personnelId;
        private readonly IMoneyBalanceDebitCheck _moneyBalanceDebitCheck;
        private readonly IMoneyService _moneyService;

        public MoneyDepositVerifyService(AAtims context, IHttpContextAccessor httpContextAccessor,
            IMoneyBalanceDebitCheck moneyBalanceDebitCheck, IMoneyService moneyService)
        {
            _context = context;
            _personnelId = Convert.ToInt32(httpContextAccessor.HttpContext.User
                .FindFirst("personnelId")?.Value);
            _moneyBalanceDebitCheck = moneyBalanceDebitCheck;
            _moneyService = moneyService;
        }

        public MoneyDepositoryDetailVm GetMoneyDepositoryDetails()
        {
            MoneyDepositoryDetailVm moneyDepositoryVerifyDetailVm = new MoneyDepositoryDetailVm();
            {
                moneyDepositoryVerifyDetailVm.BankDetails = _moneyService.MoneyGetBank();
                moneyDepositoryVerifyDetailVm.FundDetails = _moneyService.MoneyGetFund();
                moneyDepositoryVerifyDetailVm.DepositoryDetails = _moneyService.MoneyGetDeposit();
            }

            moneyDepositoryVerifyDetailVm.VaultDetails = _context.AccountAoCashBalance
                .Where(w => w.VaultFlag && !w.InactiveFlag)
                .Select(s => new MoneyCashBalanceVm
                {
                    MoneyCashBalanceId = s.AccountAoCashBalanceId,
                    CashDrawerName = s.Name,
                    BalanceAmount = s.CurrentBalance ?? default,
                    MinimumBalance = s.MinimumBalance ?? default,
                    FacilityId = s.FacilityId
                }).ToList();

            return moneyDepositoryVerifyDetailVm;
        }

        //To  Get MoneyDepositoryVerifyDetails List//
        public MoneyDepositoryDetailVm GetMoneyDepositoryVerifyDetails(int bankId, int accountAoDepositoryId, int fundId, int personnelId)
        {
            MoneyDepositoryDetailVm moneyDepositoryVerifyDetailVm = new MoneyDepositoryDetailVm();
            {
                moneyDepositoryVerifyDetailVm.MoneyDepositoryDetail = _context.AccountAoReceive
                .Where(w => w.AccountAoFund.AccountAoBankId == bankId &&
                           w.AccountAoDepositoryId == accountAoDepositoryId
                           && !w.TransactionVerified.HasValue &&
                           (w.TransactionVoidFlag == 0 || !w.TransactionVoidFlag.HasValue) &&
                            (fundId == 0 || w.AccountAoFundId == fundId) &&
                            (personnelId == 0 || w.TransactionOfficerId == personnelId))
                   .Select(s => new MoneyAccountTransactionVm
                   {
                       AccountAoReceiveId = s.AccountAoReceiveId,
                       TransactionDate = s.TransactionDate,
                       TransactionDescription = s.TransactionDescription,
                       TransactionReceipt = s.TransactionReceipt,
                       InmateNumber = s.Inmate.InmateNumber,
                       TransactionAmount = s.TransactionAmount ?? 0,
                       VerifyFlag = s.AccountAoFund.FundBlindVerify == 1,
                       FundId = s.AccountAoFund.AccountAoFundId,
                       InmateId = s.InmateId ?? 0,
                       VoidFlag = s.TransactionVoidFlag == 1,
                       TransactionReceiveCashFlag = s.TransactionReceiveCashFlag == 1,
                       BalanceInmate = s.BalanceInmatePending,
                       BalanceAccount = s.BalanceAccountPending,
                       BalanceFund = s.BalanceFundPending,
                       FundName = s.AccountAoFund.FundName,
                       FacilityAbbr = s.Inmate.Facility.FacilityAbbr,
                       BankId = s.AccountAoFund.AccountAoBankId,
                       DepositoryName = s.AccountAoDepository.DepositoryName,
                       TransactionType = s.TransactionType,
                       BankAccountAbbr = s.AccountAoFund.AccountAoBank.BankAccountAbbr,
                       TransactionNotes = s.TransactionNotes,
                       VoidDate = s.TransactionVoidDate,
                       ReceiveFrom = s.TransactionReceiveFrom,
                       ReceiveNumber = s.TransactionReceiveNumber,
                       Person = s.InmateId > 0 ? new PersonVm
                       {
                           PersonId = s.Inmate.Person.PersonId,
                           PersonLastName = s.Inmate.Person.PersonLastName,
                           PersonFirstName = s.Inmate.Person.PersonFirstName,
                           PersonMiddleName = s.Inmate.Person.PersonMiddleName,
                           PersonSuffix = s.Inmate.Person.PersonSuffix,
                       } : new PersonVm(),
                       HousingUnit = s.Inmate.HousingUnitId > 0 ? new HousingDetail()
                       {
                           HousingUnitId = s.Inmate.HousingUnit.HousingUnitId,
                           HousingUnitLocation = s.Inmate.HousingUnit.HousingUnitLocation,
                           HousingUnitNumber = s.Inmate.HousingUnit.HousingUnitNumber,
                           HousingUnitBedNumber = s.Inmate.HousingUnit.HousingUnitBedNumber,
                           HousingUnitBedLocation = s.Inmate.HousingUnit.HousingUnitBedLocation,
                       } : new HousingDetail(),
                       PersonnelBy = new PersonnelVm
                       {
                           OfficerBadgeNumber = s.TransactionOfficer.OfficerBadgeNum,
                           PersonLastName = s.TransactionOfficer.PersonNavigation.PersonLastName,
                           PersonFirstName = s.TransactionOfficer.PersonNavigation.PersonFirstName,
                           PersonMiddleName = s.TransactionOfficer.PersonNavigation.PersonMiddleName,
                           PersonnelId = s.TransactionOfficerId
                       }
                   }).OrderByDescending(o => o.AccountAoReceiveId).ToList();
                  
              
               
 
                moneyDepositoryVerifyDetailVm.PersonnelCountDetails = moneyDepositoryVerifyDetailVm.MoneyDepositoryDetail
                .GroupBy(g => new
                {
                    g.PersonnelBy.PersonnelId,
                    g.PersonnelBy.PersonFirstName,
                    g.PersonnelBy.PersonLastName,
                    g.PersonnelBy.OfficerBadgeNumber
                })
                .Select(s => new PersonnelCountVm
                {
                    PersonnelId = s.Key.PersonnelId,
                    PersonnelFirstName = s.Key.PersonFirstName,
                    PersonnelLastName = s.Key.PersonLastName,
                    OfficerBadgeNumber = s.Key.OfficerBadgeNumber,
                    Count = s.Count()
                }).ToList();

                return moneyDepositoryVerifyDetailVm;
            }
        }

        public int MoneyDepositoryVerify(List<MoneyAccountTransactionVm> feeList)
        {
            foreach (MoneyAccountTransactionVm item in feeList)
            {
                MoneyDepository(item);
            }
            return _context.SaveChanges();
        }

        public void MoneyDepository(MoneyAccountTransactionVm fee)
        {
            AccountAoReceive accountAo = _context.AccountAoReceive.Find(fee.AccountAoReceiveId);
            int receiveId;
            if (!fee.AdjustFlag)
            {
                accountAo.TransactionVerified = 1;
                accountAo.TransactionVerifiedDate = DateTime.Now;
                accountAo.TransactionVerifiedBy = _personnelId;
                accountAo.UpdatedBy = _personnelId;
                accountAo.UpdateDate = DateTime.Now;
                _context.SaveChanges();
                receiveId = fee.AccountAoReceiveId;
            }
            else
            {
                accountAo.TransactionVoidFlag = 1;
                accountAo.TransactionVoidBy = _personnelId;
                accountAo.TransactionVoidDate = DateTime.Now;
                accountAo.UpdatedBy = _personnelId;
                accountAo.UpdateDate = DateTime.Now;
                _context.SaveChanges();
                MoneyTransactionCalculateVm objParam = new MoneyTransactionCalculateVm
                {
                    BankId = fee.BankId,
                    FundId = fee.FundId,
                    InmateId = fee.InmateId,
                    TransactionAmount = accountAo.TransactionAmount ?? 0,
                    TransactionType = MoneyTransactionType.DEBIT,
                    Table = MoneyTransactionTable.ACCOUNTAO_RECEIVE
                };
                MoneyBalanceVm objUpdate = _moneyBalanceDebitCheck.MoneyTransactionCalculateBalance(objParam);
                objUpdate.InmateId = fee.InmateId;
                objUpdate.BankId = fee.BankId;
                objUpdate.FundId = fee.FundId;
                objUpdate.Table = MoneyTransactionTable.ACCOUNTAO_RECEIVE;
                _moneyBalanceDebitCheck.MoneyUpdateBalance(objUpdate);
                MoneyTransactionCalculateVm objParamCredit = new MoneyTransactionCalculateVm
                {
                    BankId = fee.BankId,
                    FundId = fee.FundId,
                    InmateId = fee.InmateId,
                    TransactionAmount = fee.VerifiedAmount,
                    TransactionType = MoneyTransactionType.CREDIT,
                    Table = MoneyTransactionTable.ACCOUNTAO_RECEIVE
                };
                MoneyBalanceVm objParamUpdateCredit = _moneyBalanceDebitCheck.MoneyTransactionCalculateBalance(objParamCredit);
                objParamUpdateCredit.InmateId = fee.InmateId;
                objParamUpdateCredit.BankId = fee.BankId;
                objParamUpdateCredit.FundId = fee.FundId;
                objParamUpdateCredit.Table = MoneyTransactionTable.ACCOUNTAO_RECEIVE;
                _moneyBalanceDebitCheck.MoneyUpdateBalance(objParamUpdateCredit);
                // Insert new record in AccountAO_Receive with verified amount
                AccountAoReceive accountAoReceiveObj = new AccountAoReceive
                {
                    InmateId = accountAo.InmateId,
                    AccountAoFundId = accountAo.AccountAoFundId,
                    AccountAoDepositoryId = accountAo.AccountAoDepositoryId,
                    CreateDate = accountAo.CreateDate,
                    UpdateDate = accountAo.UpdateDate,
                    CreatedBy = accountAo.CreatedBy,
                    UpdatedBy = accountAo.UpdatedBy,
                    TransactionOfficerId = accountAo.TransactionOfficerId,
                    TransactionDate = accountAo.TransactionDate,
                    TransactionType = accountAo.TransactionType,
                    TransactionDescription = accountAo.TransactionDescription,
                    TransactionReceiveCashFlag = accountAo.TransactionReceiveCashFlag,
                    TransactionReceiveNumber = accountAo.TransactionReceiveNumber,
                    TransactionReceiveFrom = accountAo.TransactionReceiveFrom,
                    TransactionReceipt = accountAo.TransactionReceipt,
                    TransactionVerifiedBy = _personnelId,
                    TransactionVerified = 1,
                    TransactionVerifiedDate = DateTime.Now,
                    TransactionAmount = fee.VerifiedAmount
                };
                accountAoReceiveObj.UpdatedBy = _personnelId;
                accountAoReceiveObj.UpdateDate = DateTime.Now;
                accountAoReceiveObj.AdjustmentAccountAoReceiveId = fee.AccountAoReceiveId;
                accountAoReceiveObj.BalanceInmatePending = objParamUpdateCredit.InmateBalance;
                accountAoReceiveObj.BalanceFundPending = objParamUpdateCredit.FundBalance;
                accountAoReceiveObj.BalanceAccountPending = objParamUpdateCredit.BankBalance;
                _context.AccountAoReceive.Add(accountAoReceiveObj);
                _context.SaveChanges();
                receiveId = accountAoReceiveObj.AccountAoReceiveId;
            }

            MoneyTransactionCalculateVm objParamDebit = new MoneyTransactionCalculateVm
            {
                BankId = fee.BankId,
                FundId = fee.FundId,
                InmateId = fee.InmateId,
                TransactionAmount = fee.VerifiedAmount,
                TransactionType = MoneyTransactionType.DEBIT,
                Table = MoneyTransactionTable.ACCOUNTAO_RECEIVE
            };
            MoneyBalanceVm balanceDetails = _moneyBalanceDebitCheck.MoneyTransactionCalculateBalance(objParamDebit);
            balanceDetails.InmateId = fee.InmateId;
            balanceDetails.BankId = fee.BankId;
            balanceDetails.FundId = fee.FundId;
            balanceDetails.Table = MoneyTransactionTable.ACCOUNTAO_RECEIVE;
            _moneyBalanceDebitCheck.MoneyUpdateBalance(balanceDetails);
            AccountAoTransaction aoTransDebit = new AccountAoTransaction();
            if (fee.VaultId > 0 && fee.TransactionReceiveCashFlag)
            {
                AccountAoCashBalance aoCashBalance = _context.AccountAoCashBalance.Find(fee.VaultId);
                aoCashBalance.CurrentBalance = aoCashBalance.CurrentBalance != null
                ? aoCashBalance.CurrentBalance + fee.VerifiedAmount :
                fee.VerifiedAmount;
                _context.SaveChanges();
                AccountAoCashDrawer aoCash = new AccountAoCashDrawer();
                aoCash.BalanceCashDrawer = aoCash.BalanceCashDrawer + fee.VerifiedAmount;
                aoCash.CashBalanceId = aoCashBalance.AccountAoCashBalanceId;
                aoCash.InmateId = fee.InmateId;
                aoCash.TransactionAmount = fee.VerifiedAmount;
                aoCash.TransactionDescription=fee.TransactionDescription;
                aoCash.TransactionType = MoneyTransactionType.CREDIT.ToString();
                aoCash.TransactionDate = DateTime.Now;
                aoCash.TransactionOfficerId = _personnelId;
                aoCash.CreatedBy = _personnelId;
                aoCash.CreateDate = DateTime.Now;
                aoCash.UpdatedBy = _personnelId;
                aoCash.UpdateDate = DateTime.Now;
                _context.AccountAoCashDrawer.Add(aoCash);
                _context.SaveChanges();
                int welfareFundId = _context.AccountAoFund.Single(s => s.FundWelfareOnlyFlag == 1 && s.Inactive == 0
                && s.AccountAoBankId == fee.BankId).AccountAoFundId;
                MoneyTransactionCalculateVm balanceParam = new MoneyTransactionCalculateVm
                {
                    InmateId = fee.InmateId,
                    BankId = fee.BankId,
                    FundId = welfareFundId,
                    TransactionAmount = fee.VerifiedAmount,
                    TransactionType = MoneyTransactionType.DEBIT,
                    Table = MoneyTransactionTable.ACCOUNTAO_TRANSACTION
                };
                MoneyBalanceVm objDebit = _moneyBalanceDebitCheck.MoneyTransactionCalculateBalance(balanceParam);
                objDebit.InmateId = 0;
                objDebit.BankId = fee.BankId;
                objDebit.FundId = welfareFundId;
                objDebit.Table = MoneyTransactionTable.ACCOUNTAO_TRANSACTION;
                _moneyBalanceDebitCheck.MoneyUpdateBalance(objDebit);

                aoTransDebit.AccountAoFundId = welfareFundId;
                aoTransDebit.BalanceAccount = objDebit.BankBalance;
                aoTransDebit.BalanceFund = objDebit.FundBalance;
                aoTransDebit.BalanceInmate = objDebit.InmateBalance;
                aoTransDebit.TransactionDescription=accountAo.TransactionDescription;
                aoTransDebit.TransactionAmount = -1 * Math.Abs(fee.VerifiedAmount);
                aoTransDebit.TransactionType = MoneyTransactionType.DEBIT.ToString();
                aoTransDebit.CreatedBy = _personnelId;
                aoTransDebit.CreateDate = DateTime.Now;
                aoTransDebit.TransactionDate = DateTime.Now;
                aoTransDebit.TransactionOfficerId = _personnelId;
                aoTransDebit.UpdatedBy = _personnelId;
                aoTransDebit.UpdateDate = DateTime.Now;
                _context.AccountAoTransaction.Add(aoTransDebit);
                _context.SaveChanges();
                aoCash.AccountAoTransactionId = aoTransDebit.AccountAoTransactionId;
                _context.SaveChanges();
            }

            MoneyTransactionCalculateVm objCredit = new MoneyTransactionCalculateVm
            {
                BankId = fee.BankId,
                FundId = fee.FundId,
                InmateId = fee.InmateId,
                TransactionAmount = fee.VerifiedAmount,
                TransactionType = MoneyTransactionType.CREDIT,
                Table = MoneyTransactionTable.ACCOUNTAO_TRANSACTION
            };
            MoneyBalanceVm balance = _moneyBalanceDebitCheck.MoneyTransactionCalculateBalance(objCredit);
            balance.InmateId = fee.InmateId;
            balance.BankId = fee.BankId;
            balance.FundId = fee.FundId;
            balance.Table = MoneyTransactionTable.ACCOUNTAO_TRANSACTION;
            _moneyBalanceDebitCheck.MoneyUpdateBalance(balance);
            AccountAoTransaction aoTransCredit = new AccountAoTransaction
            {
                AccountAoFundId = fee.FundId,
                TransactionType = MoneyTransactionType.CREDIT.ToString(),
                InmateId = fee.InmateId > 0 ? fee.InmateId : default(int?),
                TransactionAmount = fee.VerifiedAmount,
                TransactionDate = DateTime.Now,
                TransactionDescription = accountAo.TransactionDescription,
                TransactionReceiveCashFlag = accountAo.TransactionReceiveCashFlag,
                TransactionReceiveNumber = accountAo.TransactionReceiveNumber,
                TransactionReceiveFrom = accountAo.TransactionReceiveFrom,
                TransactionReceipt = accountAo.TransactionReceipt,
                TransactionNotes = accountAo.TransactionNotes,
                BalanceInmate = balance.InmateBalance,
                BalanceFund = balance.FundBalance,
                BalanceAccount = balance.BankBalance,
                TransactionOfficerId = accountAo.TransactionOfficerId,
                UpdateDate = DateTime.Now,
                UpdatedBy = _personnelId,
                CreateDate = DateTime.Now,
                CreatedBy = _personnelId,
                AccountAoReceiveId = receiveId,
                AccountAoTransactionFromId = aoTransDebit.AccountAoTransactionId > 0
                    ? aoTransDebit.AccountAoTransactionId
                    : default(int?)
            };
            _context.AccountAoTransaction.Add(aoTransCredit);
            _context.SaveChanges();
            if (aoTransDebit.AccountAoTransactionId > 0)
            {
                aoTransDebit.AccountAoTransactionToId = aoTransCredit.AccountAoTransactionId;
                _context.SaveChanges();
            }
            // Receive Transaction Created
            List<int> feeIds = _context.AccountAoFee
           .Where(w => !w.TransactionCleared.HasValue
               && !w.TransactionVoidFlag.HasValue
               && w.InmateId == fee.InmateId)
           .OrderByDescending(od => od.AccountAoFund.FundAllowFeeOrder)
           .ThenBy(o => o.TransactionDate)
           .Select(s => s.AccountAoFeeId)
           .ToList();
            if (feeIds.Count > 0)
            {
                MoneyDebitCheckVm debitCheck = new MoneyDebitCheckVm { TotalPayAmount = 0 };
                foreach (int f in feeIds)
                {
                    debitCheck.InmateId = fee.InmateId;
                    debitCheck.FeeFlag = 0;
                    debitCheck.DepositFlag = 1;
                    debitCheck.FeeCheckFlag = 0;
                    debitCheck.AccountAoFeeId = f;
                    debitCheck.ThersholdAmount = fee.VerifiedAmount;
                    debitCheck = _moneyBalanceDebitCheck.MoneyDebitCheck(debitCheck);
                    if (debitCheck.ReturnExitFlag) break;
                }
            }
        }
    }
}


















