using System;
using System.Linq;
using GenerateTables.Models;
using Microsoft.AspNetCore.Http;
using ServerAPI.Utilities;
using ServerAPI.ViewModels;

namespace ServerAPI.Services
{
    public class MoneyBalanceDebitCheck:IMoneyBalanceDebitCheck
    {
        private readonly AAtims _context;
        private readonly IPersonService _personService;
        private readonly int _personnelId;
        private MoneyDebitCheckVm _response = new MoneyDebitCheckVm();

        public MoneyBalanceDebitCheck(AAtims context,IPersonService personService, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _personService = personService;
            _personnelId = Convert.ToInt32(httpContextAccessor.HttpContext.User
                .FindFirst("personnelId")?.Value);
        }  
        #region Debit check
        public MoneyDebitCheckVm MoneyDebitCheck(MoneyDebitCheckVm param)
        {
            _response = new MoneyDebitCheckVm();
            //Get AccountAo Fee
            MoneyAccountAoFeeVm accountAoFee = _context.AccountAoFee.Where(s => s.AccountAoFeeId == param.AccountAoFeeId)
                    .Select(s => new MoneyAccountAoFeeVm
                    {
                        TransactionFee = s.TransactionFee ?? 0,
                        FeeWaiveDebtFlag = s.AccountAoFund.FundAllowFeeWaiveDebt ?? 0,
                        FundAllowFeePayDebtNotToExceed = s.AccountAoFund.FundAllowFeePayDebtNotToExceed ?? 0,
                        FundAllowFeePayDebtPercentage = s.AccountAoFund.FundAllowFeePayDebtPercentage ?? 0,
                        InmateId = s.InmateId ?? 0,
                        FundName = s.AccountAoFund.FundName,
                        AccountAOFundId = s.AccountAoFundId,
                        FundInmateMinBalance = s.AccountAoFund.FundInmateMinBalance ?? 0,
                        CreatedBy = s.CreatedBy,
                        CreatedDate = s.CreateDate,
                        TransactionDate = s.TransactionDate,
                        TransactionOfficerId = s.TransactionOfficerId
                    }).Single();
                    //Get AccountAo Inmate
                    MoneyAccountAoInmateVm accountAoInmate = _context.AccountAoInmate
                    .Where(w => w.InmateId == param.InmateId)
                    .Select(s => new MoneyAccountAoInmateVm
                    {
                        AccountAoInmateId = s.AccountAoInmateId,
                        AccountAoBankId = s.AccountAoBankId,
                        BalanceInmate = s.BalanceInmate,
                        BalanceInmateFee = s.BalanceInmateFee,
                        BalanceInmatePending = s.BalanceInmatePending,
                        InmateId = s.InmateId
                    }).Single();
                    accountAoInmate.FundId = _context.AccountAoFund
                    .Single(w=> (w.FundInmateOnlyFlag.HasValue && w.FundInmateOnlyFlag == 1)).AccountAoFundId;
                    accountAoInmate.Inmate = _personService.GetInmateDetails(param.InmateId);
                    // BEGIN for FeeFlag = 1
                    if(param.FeeFlag == 1)
                    {
                        MoneyFeeFlag(param,accountAoFee,accountAoInmate);
                    }
                    // END for FeeFlag = 1
                    // BEGIN for DepositFlag = 1 
                    if(param.DepositFlag ==1 )
                    {
                        MoneyDepositFlag(param,accountAoFee,accountAoInmate);
                    }
                    // END for DepositFlag = 1
                    // BEGIN for FeeCheckFlag = 1 
                    if(param.FeeCheckFlag == 1)
                    {
                        MoneyFeeCheckFlag(param,accountAoFee,accountAoInmate);
                    }
                    // END for FeeCheckFlag = 1
                    return _response;
        }

        private void MoneyFeeFlag(MoneyDebitCheckVm param,MoneyAccountAoFeeVm accountAoFee,MoneyAccountAoInmateVm accountAoInmate)
        {
                    //  BEGIN for 1ST CONDITION - FEE FLAG 
                    // INSUFFICIENT FUND
                        if(accountAoInmate.BalanceInmate <= accountAoFee.FundInmateMinBalance)
                        {
                            if(accountAoFee.TransactionFee > 0 && accountAoFee.FeeWaiveDebtFlag == 1)
                            {
                                    AccountAoFee feeDetail = _context.AccountAoFee.Find(param.AccountAoFeeId);
                                    feeDetail.TransactionCleared = 1;
                                    feeDetail.TransactionClearedDate = DateTime.Now;
                                    feeDetail.TransactionClearedBy = _personnelId;
                                    feeDetail.TransactionDescription = feeDetail.TransactionDescription + MoneyConstants.FEEWAIVED;
                                    feeDetail.TransactionClearedAmount = accountAoFee.TransactionFee;
                                    feeDetail.UpdateDate = DateTime.Now;
                                    feeDetail.UpdatedBy = _personnelId;
                                    _context.SaveChanges();
                                    //Money Transaction Calculation
                                    MoneyTransactionCalculateVm requestParam = new MoneyTransactionCalculateVm();
                                    requestParam.InmateId = param.InmateId;
                                    requestParam.TransactionAmount = accountAoFee.TransactionFee;
                                    requestParam.BankId = accountAoInmate.AccountAoBankId;
                                    requestParam.FundId = accountAoFee.AccountAOFundId;
                                    requestParam.TransactionType = MoneyTransactionType.DEBIT;
                                    requestParam.Table = MoneyTransactionTable.ACCOUNTAO_FEE;
                                    MoneyBalanceVm balanceDetails = MoneyTransactionCalculateBalance(requestParam);
                                    //Money Update Balance
                                    balanceDetails.InmateId = param.InmateId;
                                    balanceDetails.BankId = accountAoInmate.AccountAoBankId;
                                    balanceDetails.FundId = accountAoFee.AccountAOFundId;
                                    balanceDetails.Table = MoneyTransactionTable.ACCOUNTAO_FEE;
                                    MoneyUpdateBalance(balanceDetails);
                            }
                            //Insufficient fund return message
                            _response.StatusCode = 3;
                            return;
                        }
                        // END for 1ST CONDITION - FEE FLAG
                        // BEGIN for 2ND CONDITION - FEE FLAG  
                        // PARTIAL PAYMENT
                        if((accountAoInmate.BalanceInmate - accountAoFee.TransactionFee) < accountAoFee.FundInmateMinBalance)
                        {
                            //To Update Debit Balances  
                            MoneyTransactionCalculateVm debitBalanceParam = new MoneyTransactionCalculateVm();
                            debitBalanceParam.InmateId = param.InmateId;
                            debitBalanceParam.TransactionAmount = accountAoInmate.BalanceInmate - accountAoFee.FundInmateMinBalance;
                            debitBalanceParam.BankId = accountAoInmate.AccountAoBankId;
                            debitBalanceParam.FundId = accountAoInmate.FundId;
                            debitBalanceParam.TransactionType = MoneyTransactionType.DEBIT;
                            debitBalanceParam.Table = MoneyTransactionTable.ACCOUNTAO_TRANSACTION;
                            MoneyBalanceVm debitBalanceDetails = MoneyTransactionCalculateBalance(debitBalanceParam);
                            //Money Update Balance
                            debitBalanceDetails.InmateId = param.InmateId;
                            debitBalanceDetails.BankId = accountAoInmate.AccountAoBankId;
                            debitBalanceDetails.FundId = accountAoInmate.FundId;
                            debitBalanceDetails.Table = MoneyTransactionTable.ACCOUNTAO_TRANSACTION;
                            MoneyUpdateBalance(debitBalanceDetails);
                            // To Create Debit Tranaction in the Transaction Table
                            AccountAoTransaction aoDebitTransaction = new AccountAoTransaction();
                            aoDebitTransaction.InmateId = param.InmateId;
                            aoDebitTransaction.AccountAoFundId = accountAoInmate.FundId;
                            aoDebitTransaction.AccountAoFeeId = param.AccountAoFeeId;
                            aoDebitTransaction.TransactionOfficerId = _personnelId;
                            aoDebitTransaction.TransactionDate = DateTime.Now;
                            aoDebitTransaction.TransactionAmount = -1 * Math.Abs(accountAoInmate.BalanceInmate - accountAoFee.FundInmateMinBalance);
                            aoDebitTransaction.TransactionType = MoneyTransactionType.DEBIT.ToString();
                            aoDebitTransaction.TransactionDescription = MoneyConstants.AUTOPAYFEE;
                            aoDebitTransaction.BalanceInmate = debitBalanceDetails.InmateBalance;
                            aoDebitTransaction.BalanceFund = debitBalanceDetails.FundBalance;
                            aoDebitTransaction.BalanceAccount = debitBalanceDetails.BankBalance;
                            aoDebitTransaction.CreatedBy = _personnelId;
                            aoDebitTransaction.UpdatedBy = _personnelId;
                            aoDebitTransaction.CreateDate = DateTime.Now;
                            aoDebitTransaction.UpdateDate = DateTime.Now;
                            _context.AccountAoTransaction.Add(aoDebitTransaction);
                            _context.SaveChanges();
                            // To Update Fee Pending Balances  
                            MoneyTransactionCalculateVm feeBalanceParam = new MoneyTransactionCalculateVm();
                            feeBalanceParam.InmateId = param.InmateId;
                            feeBalanceParam.TransactionAmount = accountAoInmate.BalanceInmate - accountAoFee.FundInmateMinBalance;
                            feeBalanceParam.BankId = accountAoInmate.AccountAoBankId;
                            feeBalanceParam.FundId = accountAoFee.AccountAOFundId;
                            feeBalanceParam.TransactionType = MoneyTransactionType.DEBIT;
                            feeBalanceParam.Table = MoneyTransactionTable.ACCOUNTAO_FEE;
                            MoneyBalanceVm feeBalanceDetails = MoneyTransactionCalculateBalance(feeBalanceParam);
                            //Money Update Balance
                            feeBalanceDetails.InmateId = param.InmateId;
                            feeBalanceDetails.BankId = accountAoInmate.AccountAoBankId;
                            feeBalanceDetails.FundId = accountAoFee.AccountAOFundId;
                            feeBalanceDetails.Table = MoneyTransactionTable.ACCOUNTAO_FEE;
                            MoneyUpdateBalance(feeBalanceDetails);
                            // To Update Credit Balances
                            MoneyTransactionCalculateVm creditBalanceParam = new MoneyTransactionCalculateVm();
                            creditBalanceParam.InmateId = param.InmateId;
                            creditBalanceParam.TransactionAmount = accountAoInmate.BalanceInmate - accountAoFee.FundInmateMinBalance;
                            creditBalanceParam.BankId = accountAoInmate.AccountAoBankId;
                            creditBalanceParam.FundId = accountAoFee.AccountAOFundId;
                            creditBalanceParam.TransactionType = MoneyTransactionType.CREDIT;
                            creditBalanceParam.Table = MoneyTransactionTable.ACCOUNTAO_TRANSACTION;
                            MoneyBalanceVm creditBalanceDetails = MoneyTransactionCalculateBalance(creditBalanceParam);
                            //Money Update Balance
                            creditBalanceDetails.BankId = accountAoInmate.AccountAoBankId;
                            creditBalanceDetails.FundId = accountAoFee.AccountAOFundId;
                            creditBalanceDetails.Table = MoneyTransactionTable.ACCOUNTAO_TRANSACTION;
                            MoneyUpdateBalance(creditBalanceDetails);
                                // To Create Credit Tranaction in the Transaction Table 
                                AccountAoTransaction aoCreditTransaction = new AccountAoTransaction();
                                aoCreditTransaction.InmateId = param.InmateId;
                                aoCreditTransaction.AccountAoFundId = accountAoFee.AccountAOFundId;
                                aoCreditTransaction.AccountAoFeeId = param.AccountAoFeeId;
                                aoCreditTransaction.TransactionOfficerId = _personnelId;
                                aoCreditTransaction.TransactionDate = DateTime.Now;
                                aoCreditTransaction.TransactionAmount = Math.Abs(accountAoInmate.BalanceInmate - accountAoFee.FundInmateMinBalance);
                                aoCreditTransaction.TransactionType = MoneyTransactionType.CREDIT.ToString();
                                aoCreditTransaction.TransactionDescription = MoneyConstants.AUTOPAYFEE;
                                aoCreditTransaction.BalanceFund = creditBalanceDetails.FundBalance;
                                aoCreditTransaction.BalanceAccount = creditBalanceDetails.BankBalance;
                                aoCreditTransaction.AccountAoTransactionFromId = aoDebitTransaction.AccountAoTransactionId;
                                aoCreditTransaction.CreatedBy = _personnelId;
                                aoCreditTransaction.UpdatedBy = _personnelId;
                                aoCreditTransaction.CreateDate = DateTime.Now;
                                aoCreditTransaction.UpdateDate = DateTime.Now;
                                _context.AccountAoTransaction.Add(aoCreditTransaction);
                                _context.SaveChanges();
                                // To Update Transaction To Id
                                AccountAoTransaction aoTransactionUpdate = _context.AccountAoTransaction.Find(aoDebitTransaction.AccountAoTransactionId);
                                aoTransactionUpdate.AccountAoTransactionToId = aoCreditTransaction.AccountAoTransactionId;
                                _context.SaveChanges();
                                // To Update AccountAo_Fee Table
                                AccountAoFee objAoFee = _context.AccountAoFee.Find(param.AccountAoFeeId);
                                objAoFee.TransactionCleared = 1;
                                objAoFee.TransactionClearedDate = DateTime.Now;
                                objAoFee.TransactionClearedBy = _personnelId;
                                objAoFee.TransactionClearedAmount = accountAoInmate.BalanceInmate - accountAoFee.FundInmateMinBalance;
                                objAoFee.UpdateDate = DateTime.Now;
                                objAoFee.UpdatedBy = _personnelId;
                                _context.SaveChanges();
                                _response.PartialPayAmount = accountAoInmate.BalanceInmate - accountAoFee.FundInmateMinBalance;
                                // To Insert New Amount in the AccountAo_Fee table 
                                if(accountAoFee.TransactionFee - (accountAoInmate.BalanceInmate - accountAoFee.FundInmateMinBalance) > 0)
                                {
                                    AccountAoFee newAmount = new AccountAoFee();
                                    newAmount.InmateId = accountAoFee.InmateId;
                                    newAmount.AccountAoFundId = accountAoFee.AccountAOFundId;
                                    newAmount.CreateDate = accountAoFee.CreatedDate;
                                    newAmount.CreatedBy = accountAoFee.CreatedBy;
                                    newAmount.TransactionDate = accountAoFee.TransactionDate;
                                    newAmount.TransactionOfficerId = accountAoFee.TransactionOfficerId;
                                    newAmount.TransactionDescription = MoneyConstants.PARTIALPAID;
                                    newAmount.TransactionFee = accountAoFee.TransactionFee - (accountAoInmate.BalanceInmate - accountAoFee.FundInmateMinBalance);
                                    newAmount.BalanceInmateFee = feeBalanceDetails.InmateBalance;
                                    newAmount.BalanceFundFee = feeBalanceDetails.FundBalance;
                                    newAmount.BalanceAccountFee = feeBalanceDetails.BankBalance;
                                    newAmount.AdjustmentAccountAoFeeId = param.AccountAoFeeId;
                                    newAmount.UpdateDate = DateTime.Now;
                                    newAmount.UpdatedBy = _personnelId;
                                    _context.AccountAoFee.Add(newAmount);
                                    _context.SaveChanges();
                                    // Successfully Saved!;
                                    _response.StatusCode = 4;
                                    _response.NewFee = accountAoFee.TransactionFee - (accountAoInmate.BalanceInmate - accountAoFee.FundInmateMinBalance);
                                    if(accountAoFee.FeeWaiveDebtFlag == 1)
                                    {
                                        AccountAoFee objAoFee1 = _context.AccountAoFee.Find(newAmount.AccountAoFeeId);
                                        objAoFee1.TransactionCleared = 1;
                                        objAoFee1.TransactionClearedDate = DateTime.Now;
                                        objAoFee1.TransactionClearedBy = _personnelId;
                                        objAoFee1.TransactionDescription = objAoFee1.TransactionDescription + MoneyConstants.FEEWAIVED;
                                        objAoFee1.TransactionClearedAmount = accountAoFee.TransactionFee - (accountAoInmate.BalanceInmate - accountAoFee.FundInmateMinBalance);
                                        objAoFee1.UpdateDate = DateTime.Now;
                                        objAoFee1.UpdatedBy = _personnelId;
                                        _context.SaveChanges();
                                        // NewTransAmount Credited in the Fee Pending Balance(FeeAmount -CurrentBalance)
                                        MoneyTransactionCalculateVm objCalculationParam = new MoneyTransactionCalculateVm();
                                        objCalculationParam.InmateId = param.InmateId;
                                        objCalculationParam.TransactionAmount = accountAoFee.TransactionFee - (accountAoInmate.BalanceInmate - accountAoFee.FundInmateMinBalance);
                                        objCalculationParam.BankId = accountAoInmate.AccountAoBankId;
                                        objCalculationParam.FundId = accountAoFee.AccountAOFundId;
                                        objCalculationParam.TransactionType = MoneyTransactionType.DEBIT;
                                        objCalculationParam.Table = MoneyTransactionTable.ACCOUNTAO_FEE;
                                        MoneyBalanceVm objBalance =  MoneyTransactionCalculateBalance(objCalculationParam);
                                        //Money Update Balance
                                        objBalance.InmateId = param.InmateId;
                                        objBalance.BankId = accountAoInmate.AccountAoBankId;
                                        objBalance.FundId = accountAoFee.AccountAOFundId;
                                        objBalance.Table = MoneyTransactionTable.ACCOUNTAO_FEE;
                                        MoneyUpdateBalance(objBalance);
                                        _response.StatusCode = 5;
                                    }
                                    // Parital payment and Fee Has Been Waived For This Fund.
                                }
                                return;
                        }
                        // END for 2ND CONDITION - FEE FLAG  
                        // BEGIN for 3RD CONDITION - FEE FLAG
                        // FULL PAYMENT
                        if((accountAoInmate.BalanceInmate - accountAoFee.TransactionFee) >= accountAoFee.FundInmateMinBalance)
                        {
                            AccountAoFee objAoFee = _context.AccountAoFee.Find(param.AccountAoFeeId);
                            objAoFee.TransactionCleared = 1;
                            objAoFee.TransactionClearedDate = DateTime.Now;
                            objAoFee.TransactionClearedBy = _personnelId;
                            objAoFee.TransactionClearedAmount = accountAoFee.TransactionFee;
                            objAoFee.UpdateDate = DateTime.Now;
                            objAoFee.UpdatedBy = _personnelId;
                            // To Update Fee Pending Balances
                            MoneyTransactionCalculateVm feePendingParam = new MoneyTransactionCalculateVm();
                            feePendingParam.InmateId = param.InmateId;
                            feePendingParam.TransactionAmount = accountAoFee.TransactionFee;
                            feePendingParam.BankId = accountAoInmate.AccountAoBankId;
                            feePendingParam.FundId = accountAoInmate.FundId;
                            feePendingParam.TransactionType = MoneyTransactionType.DEBIT;
                            feePendingParam.Table = MoneyTransactionTable.ACCOUNTAO_TRANSACTION;
                            MoneyBalanceVm feePendingBalance =  MoneyTransactionCalculateBalance(feePendingParam);
                            feePendingBalance.InmateId = param.InmateId;
                            feePendingBalance.BankId = accountAoInmate.AccountAoBankId;
                            feePendingBalance.FundId = accountAoInmate.FundId;
                            feePendingBalance.Table = MoneyTransactionTable.ACCOUNTAO_TRANSACTION;
                            MoneyUpdateBalance(feePendingBalance);
                                // To Create Debit Transaction in the Transaction Table
                                AccountAoTransaction aoDebitTransaction = new AccountAoTransaction();
                                aoDebitTransaction.InmateId = param.InmateId;
                                aoDebitTransaction.AccountAoFundId = accountAoInmate.FundId;
                                aoDebitTransaction.AccountAoFeeId = param.AccountAoFeeId;
                                aoDebitTransaction.TransactionOfficerId = _personnelId;
                                aoDebitTransaction.TransactionDate = DateTime.Now;
                                aoDebitTransaction.TransactionAmount = -1 * Math.Abs(accountAoFee.TransactionFee);
                                aoDebitTransaction.TransactionType = MoneyTransactionType.DEBIT.ToString();
                                aoDebitTransaction.TransactionDescription = MoneyConstants.AUTOPAYFEE;
                                aoDebitTransaction.BalanceInmate = feePendingBalance.InmateBalance;
                                aoDebitTransaction.BalanceFund = feePendingBalance.FundBalance;
                                aoDebitTransaction.BalanceAccount = feePendingBalance.BankBalance;
                                aoDebitTransaction.CreatedBy = _personnelId;
                                aoDebitTransaction.UpdatedBy = _personnelId;
                                aoDebitTransaction.CreateDate = DateTime.Now;
                                aoDebitTransaction.UpdateDate = DateTime.Now;
                                _context.AccountAoTransaction.Add(aoDebitTransaction);
                                _context.SaveChanges();
                                    // To Update Fee Pending Balances
                                    MoneyTransactionCalculateVm feePendingParam1 = new MoneyTransactionCalculateVm();
                                    feePendingParam1.InmateId = param.InmateId;
                                    feePendingParam1.TransactionAmount = accountAoFee.TransactionFee;
                                    feePendingParam1.BankId = accountAoInmate.AccountAoBankId;
                                    feePendingParam1.FundId = accountAoFee.AccountAOFundId;
                                    feePendingParam1.TransactionType = MoneyTransactionType.DEBIT;
                                    feePendingParam1.Table = MoneyTransactionTable.ACCOUNTAO_FEE;
                                    MoneyBalanceVm feePendingBalance1 =  MoneyTransactionCalculateBalance(feePendingParam1);
                                    feePendingBalance1.InmateId = param.InmateId;
                                    feePendingBalance1.BankId = accountAoInmate.AccountAoBankId;
                                    feePendingBalance1.FundId = accountAoFee.AccountAOFundId;
                                    feePendingBalance1.Table = MoneyTransactionTable.ACCOUNTAO_FEE;
                                    MoneyUpdateBalance(feePendingBalance1);
                                    // To Update Credit Balances
                                    MoneyTransactionCalculateVm feePendingParam2 = new MoneyTransactionCalculateVm();
                                    feePendingParam2.InmateId = param.InmateId;
                                    feePendingParam2.TransactionAmount = accountAoFee.TransactionFee;
                                    feePendingParam2.BankId = accountAoInmate.AccountAoBankId;
                                    feePendingParam2.FundId = accountAoFee.AccountAOFundId;
                                    feePendingParam2.TransactionType = MoneyTransactionType.CREDIT;
                                    feePendingParam2.Table = MoneyTransactionTable.ACCOUNTAO_TRANSACTION;
                                    MoneyBalanceVm feePendingBalance2 =  MoneyTransactionCalculateBalance(feePendingParam2);
                                    feePendingBalance2.BankId = accountAoInmate.AccountAoBankId;
                                    feePendingBalance2.FundId = accountAoFee.AccountAOFundId;
                                    feePendingBalance2.Table = MoneyTransactionTable.ACCOUNTAO_TRANSACTION;
                                    MoneyUpdateBalance(feePendingBalance2);
                                        // To Create Credit Tranaction in the Transaction Table
                                        AccountAoTransaction aoCreditTransaction = new AccountAoTransaction();
                                        aoCreditTransaction.InmateId = param.InmateId;
                                        aoCreditTransaction.AccountAoFundId = accountAoFee.AccountAOFundId;
                                        aoCreditTransaction.AccountAoFeeId = param.AccountAoFeeId;
                                        aoCreditTransaction.TransactionOfficerId = _personnelId;
                                        aoCreditTransaction.TransactionDate = DateTime.Now;
                                        aoCreditTransaction.TransactionAmount = Math.Abs(accountAoFee.TransactionFee);
                                        aoCreditTransaction.TransactionType = MoneyTransactionType.CREDIT.ToString();
                                        aoCreditTransaction.TransactionDescription = MoneyConstants.AUTOPAYFEE;
                                        aoCreditTransaction.BalanceFund = feePendingBalance.FundBalance;
                                        aoCreditTransaction.BalanceAccount = feePendingBalance.BankBalance;
                                        aoCreditTransaction.AccountAoTransactionFromId = aoDebitTransaction.AccountAoTransactionId;
                                        aoCreditTransaction.CreatedBy = _personnelId;
                                        aoCreditTransaction.UpdatedBy = _personnelId;
                                        aoCreditTransaction.CreateDate = DateTime.Now;
                                        aoCreditTransaction.UpdateDate = DateTime.Now;
                                        _context.AccountAoTransaction.Add(aoCreditTransaction);
                                        _context.SaveChanges();
                                        // Full Payment of Fee Applied
                                        _response.StatusCode = 6;
                        }
                    // END for 3RD CONDITION - FEE FLAG
        }
        private void MoneyDepositFlag(MoneyDebitCheckVm param,MoneyAccountAoFeeVm accountAoFee,MoneyAccountAoInmateVm accountAoInmate)
        {
                        decimal payAmount = accountAoFee.TransactionFee;
                        decimal newPayAmount = 0;
                        if(accountAoFee.FundAllowFeePayDebtNotToExceed > 0)
                        {
                            // LOGIC FOR PAY DEBT NOT TO EXCEED THE THRESHOLD.  (originally the deposit amount).  If > then reset threshold 
                            if(accountAoFee.FundAllowFeePayDebtNotToExceed > param.ThersholdAmount)
                            {
                                param.ThersholdAmount = accountAoFee.FundAllowFeePayDebtNotToExceed;
                            }
                        }
                        if(accountAoFee.FundAllowFeePayDebtPercentage > 0)
                        {
                            // LOGIC FOR PAY DEBT PERCENTAGE FOR FUND – reduce it by the % 
                            param.ThersholdAmount =  param.ThersholdAmount * ((decimal)accountAoFee.FundAllowFeePayDebtPercentage/100);
                        }
                        param.ThersholdAmount = param.ThersholdAmount - param.TotalPayAmount;
                        // DO THRESHOLD CHECK FOR THE PAYMENT 
                        // THE CURRENT FEE IS MORE THEN WHAT IS ALLOWED BY THE DEPOSIT
                        if(accountAoFee.TransactionFee > param.ThersholdAmount)
                        {
                            payAmount = param.ThersholdAmount;
                            // DO NOT PAY ANY ADDITIONAL FEES
                            _response.ReturnExitFlag = true; 
                        }
                        // BEGIN for 1ST CONDITION - DEPOSIT FLAG 
                        if(accountAoInmate.BalanceInmate <= accountAoFee.FundInmateMinBalance)
                        {
                            // Insufficient Funds To Pay Fee
                            _response.ReturnExitFlag = true;
                            _response.StatusCode = 1;
                            return ;
                        }
                        // END for 1ST CONDITION - DEPOSIT FLAG
                        // BEGIN for 2ND CONDITION - DEPOSIT FLAG
                        // PARTIAL PAYMENT 
                        if(((accountAoInmate.BalanceInmate - payAmount)  < accountAoFee.FundInmateMinBalance)
                        || (((accountAoInmate.BalanceInmate - payAmount) >= accountAoFee.FundInmateMinBalance)
                        && payAmount < accountAoFee.TransactionFee))
                        {
                            // BELOW INMATES ALLOWED BALANCE FOR THE FUND
                            if((accountAoInmate.BalanceInmate - payAmount) < accountAoFee.FundInmateMinBalance)
                            {
                                // DO TRANSFER INMATE TO FUND of New PayAmount
                                newPayAmount = accountAoInmate.BalanceInmate - accountAoFee.FundInmateMinBalance;
                                // DO NOT PAY ANY ADDITIONAL FEES
                                _response.ReturnExitFlag = true;
                            }
                            if(((accountAoInmate.BalanceInmate - payAmount) >= accountAoFee.FundInmateMinBalance)
                            && payAmount < accountAoFee.TransactionFee)
                            {
                                // INMATE HAS ENOUGH FUNDS BUT PAYMENT AMOUNT IS LESS THEN TRANSACTION FEE, HENCE PARTIAL PAYMENT
                                newPayAmount = payAmount;
                            }
                            if(newPayAmount < 0)
                            {
                                // Payment Cannot Be Done
                                _response.StatusCode = 0;
                                return ;
                            }
                            // To Update Debit Balances
                            MoneyTransactionCalculateVm objDebitParam = new MoneyTransactionCalculateVm();
                            objDebitParam.InmateId = param.InmateId;
                            objDebitParam.TransactionAmount = newPayAmount;
                            objDebitParam.BankId = accountAoInmate.AccountAoBankId;
                            objDebitParam.FundId = accountAoInmate.FundId;
                            objDebitParam.TransactionType = MoneyTransactionType.DEBIT;
                            objDebitParam.Table = MoneyTransactionTable.ACCOUNTAO_TRANSACTION;
                            MoneyBalanceVm objDebitBalance =  MoneyTransactionCalculateBalance(objDebitParam);
                            objDebitBalance.InmateId = param.InmateId;
                            objDebitBalance.BankId = accountAoInmate.AccountAoBankId;
                            objDebitBalance.FundId = accountAoInmate.FundId;
                            objDebitBalance.Table = MoneyTransactionTable.ACCOUNTAO_TRANSACTION;
                            MoneyUpdateBalance(objDebitBalance);
                                // To Create Debit Tranaction in the Transaction Table
                                AccountAoTransaction aoDebitTransaction = new AccountAoTransaction();
                                aoDebitTransaction.InmateId = param.InmateId;
                                aoDebitTransaction.AccountAoFundId = accountAoInmate.FundId;
                                aoDebitTransaction.AccountAoFeeId = param.AccountAoFeeId;
                                aoDebitTransaction.TransactionOfficerId = _personnelId;
                                aoDebitTransaction.TransactionDate = DateTime.Now;
                                aoDebitTransaction.TransactionAmount = -1 * Math.Abs(newPayAmount);
                                aoDebitTransaction.TransactionType = MoneyTransactionType.DEBIT.ToString();
                                aoDebitTransaction.TransactionDescription = MoneyConstants.AUTOPAYFEE;
                                aoDebitTransaction.BalanceInmate = objDebitBalance.InmateBalance;
                                aoDebitTransaction.BalanceFund = objDebitBalance.FundBalance;
                                aoDebitTransaction.BalanceAccount = objDebitBalance.BankBalance;
                                aoDebitTransaction.CreatedBy = _personnelId;
                                aoDebitTransaction.UpdatedBy = _personnelId;
                                aoDebitTransaction.CreateDate = DateTime.Now;
                                aoDebitTransaction.UpdateDate = DateTime.Now;
                                _context.AccountAoTransaction.Add(aoDebitTransaction);
                                _context.SaveChanges();
                                // To Update Fee Pending Balances
                                MoneyTransactionCalculateVm objDebitPendingParam = new MoneyTransactionCalculateVm
                                {
                                    InmateId = param.InmateId,
                                    TransactionAmount = newPayAmount,
                                    BankId = accountAoInmate.AccountAoBankId,
                                    FundId = accountAoFee.AccountAOFundId,
                                    TransactionType = MoneyTransactionType.DEBIT,
                                    Table = MoneyTransactionTable.ACCOUNTAO_FEE
                                };
                                MoneyBalanceVm objDebitPendingBalance =  MoneyTransactionCalculateBalance(objDebitPendingParam);
                                objDebitPendingBalance.InmateId = param.InmateId;
                                objDebitPendingBalance.BankId = accountAoInmate.AccountAoBankId;
                                objDebitPendingBalance.FundId = accountAoFee.AccountAOFundId;
                                objDebitPendingBalance.Table = MoneyTransactionTable.ACCOUNTAO_FEE;
                                MoneyUpdateBalance(objDebitPendingBalance);
                                // To Update Credit Balances
                                MoneyTransactionCalculateVm objCreditParam = new MoneyTransactionCalculateVm();
                                objCreditParam.InmateId = param.InmateId;
                                objCreditParam.TransactionAmount = newPayAmount;
                                objCreditParam.BankId = accountAoInmate.AccountAoBankId;
                                objCreditParam.FundId = accountAoFee.AccountAOFundId;
                                objCreditParam.TransactionType = MoneyTransactionType.CREDIT;
                                objCreditParam.Table = MoneyTransactionTable.ACCOUNTAO_TRANSACTION;
                                MoneyBalanceVm objCreditBalance =  MoneyTransactionCalculateBalance(objCreditParam);
                                objCreditBalance.BankId = accountAoInmate.AccountAoBankId;
                                objCreditBalance.FundId = accountAoFee.AccountAOFundId;
                                objCreditBalance.Table = MoneyTransactionTable.ACCOUNTAO_TRANSACTION;
                                MoneyUpdateBalance(objCreditBalance);
                                        // To Create Credit Tranaction in the Transaction Table  
                                        AccountAoTransaction aoCreditTransaction = new AccountAoTransaction();
                                        aoCreditTransaction.InmateId = param.InmateId;
                                        aoCreditTransaction.AccountAoFundId = accountAoFee.AccountAOFundId;
                                        aoCreditTransaction.AccountAoFeeId = param.AccountAoFeeId;
                                        aoCreditTransaction.TransactionOfficerId = _personnelId;
                                        aoCreditTransaction.TransactionDate = DateTime.Now;
                                        aoCreditTransaction.TransactionAmount = Math.Abs(newPayAmount);
                                        aoCreditTransaction.TransactionType = MoneyTransactionType.CREDIT.ToString();
                                        aoCreditTransaction.TransactionDescription = MoneyConstants.AUTOPAYFEE;
                                        aoCreditTransaction.BalanceFund = objCreditBalance.FundBalance;
                                        aoCreditTransaction.BalanceAccount = objCreditBalance.BankBalance;
                                        aoCreditTransaction.AccountAoTransactionFromId = aoDebitTransaction.AccountAoTransactionId;
                                        aoCreditTransaction.CreatedBy = _personnelId;
                                        aoCreditTransaction.UpdatedBy = _personnelId;
                                        aoCreditTransaction.CreateDate = DateTime.Now;
                                        aoCreditTransaction.UpdateDate = DateTime.Now;
                                        _context.AccountAoTransaction.Add(aoCreditTransaction);
                                        _context.SaveChanges();
                                        // To Update Transaction To Id
                                        AccountAoTransaction objAoTransaction  = 
                                        _context.AccountAoTransaction.Find(aoDebitTransaction.AccountAoTransactionId);
                                        objAoTransaction.AccountAoTransactionToId = aoCreditTransaction.AccountAoTransactionId;
                                        _context.SaveChanges();
                                        // To Update AccountAo_Fee Table [Clearing the Old Fee]
                                        AccountAoFee objAoFee = _context.AccountAoFee.Find(param.AccountAoFeeId);
                                        objAoFee.TransactionCleared = 1;
                                        objAoFee.TransactionClearedDate = DateTime.Now;
                                        objAoFee.TransactionClearedBy = _personnelId;
                                        objAoFee.TransactionClearedAmount = newPayAmount;
                                        objAoFee.UpdateDate = DateTime.Now;
                                        objAoFee.UpdatedBy = _personnelId;
                                        _context.SaveChanges();
                                        decimal newFee = accountAoFee.TransactionFee - newPayAmount;
                                        // To Insert New Amount in the AccountAo_Fee table [Inserting the New Fee]
                                        if(newFee > 0)
                                        {
                                            AccountAoFee objNewAoFee = new AccountAoFee();
                                            objNewAoFee.InmateId = objAoFee.InmateId;
                                            objNewAoFee.AccountAoFundId = objAoFee.AccountAoFundId;
                                            objNewAoFee.CreateDate = objAoFee.CreateDate;
                                            objNewAoFee.CreatedBy = objAoFee.CreatedBy;
                                            objNewAoFee.TransactionDate = objAoFee.TransactionDate;
                                            objNewAoFee.TransactionOfficerId = objAoFee.TransactionOfficerId;
                                            objNewAoFee.TransactionDescription = MoneyConstants.PARTIALPAID;
                                            objNewAoFee.TransactionFee = newFee;
                                            objNewAoFee.BalanceInmateFee = objDebitPendingBalance.InmateBalance;
                                            objNewAoFee.BalanceFundFee = objDebitPendingBalance.FundBalance;
                                            objNewAoFee.BalanceAccountFee = objDebitPendingBalance.BankBalance;
                                            objNewAoFee.AdjustmentAccountAoFeeId = param.AccountAoFeeId;
                                            objNewAoFee.UpdateDate = DateTime.Now;
                                            objNewAoFee.UpdatedBy = _personnelId;
                                            _context.AccountAoFee.Add(objNewAoFee);
                                            _context.SaveChanges();
                                        }
                                        // Partial payment applied and new fee created
                                        _response.TotalPayAmount = _response.TotalPayAmount + newFee;
                                        _response.StatusCode = 1;
                                        return ;
                        }
                        // END for 2ND CONDITION - DEPOSIT FLAG  
                        // BEGIN for 3RD CONDITION - DEPOSIT FLAG
                        // FULL PAYMENT  
                        if((accountAoInmate.BalanceInmate - payAmount) >= accountAoFee.FundInmateMinBalance)
                        {
                            AccountAoFee objAoFee = _context.AccountAoFee.Find(param.AccountAoFeeId);
                            objAoFee.TransactionCleared = 1;
                            objAoFee.TransactionClearedDate = DateTime.Now;
                            objAoFee.TransactionClearedBy = _personnelId;
                            objAoFee.TransactionClearedAmount = payAmount;
                            objAoFee.UpdateDate = DateTime.Now;
                            objAoFee.UpdatedBy = _personnelId;
                            _context.SaveChanges();
                            newPayAmount = payAmount;
                            // To Update Fee Pending Balances
                            MoneyTransactionCalculateVm objAoTransParam = new MoneyTransactionCalculateVm();
                            objAoTransParam.InmateId = param.InmateId;
                            objAoTransParam.TransactionAmount = newPayAmount;
                            objAoTransParam.BankId = accountAoInmate.AccountAoBankId;
                            objAoTransParam.FundId = accountAoInmate.FundId;
                            objAoTransParam.TransactionType = MoneyTransactionType.DEBIT;
                            objAoTransParam.Table = MoneyTransactionTable.ACCOUNTAO_TRANSACTION;
                            MoneyBalanceVm objAoTransBalance =  MoneyTransactionCalculateBalance(objAoTransParam);
                            objAoTransBalance.InmateId = param.InmateId;
                            objAoTransBalance.BankId = accountAoInmate.AccountAoBankId;
                            objAoTransBalance.FundId = accountAoInmate.FundId;
                            objAoTransBalance.Table = MoneyTransactionTable.ACCOUNTAO_TRANSACTION;
                            MoneyUpdateBalance(objAoTransBalance);
                                // To Create Debit Tranaction in the Transaction Table
                                AccountAoTransaction objAoDebitTransaction = new AccountAoTransaction();
                                objAoDebitTransaction.InmateId = param.InmateId;
                                objAoDebitTransaction.AccountAoFundId = accountAoInmate.FundId;
                                objAoDebitTransaction.AccountAoFeeId = param.AccountAoFeeId;
                                objAoDebitTransaction.TransactionOfficerId = _personnelId;
                                objAoDebitTransaction.TransactionDate = DateTime.Now;
                                objAoDebitTransaction.TransactionAmount = -1 * Math.Abs(newPayAmount);
                                objAoDebitTransaction.TransactionType = MoneyTransactionType.DEBIT.ToString();
                                objAoDebitTransaction.TransactionDescription = MoneyConstants.AUTOPAYFEE;
                                objAoDebitTransaction.BalanceInmate = objAoTransBalance.InmateBalance;
                                objAoDebitTransaction.BalanceFund = objAoTransBalance.FundBalance;
                                objAoDebitTransaction.BalanceAccount = objAoTransBalance.BankBalance;
                                objAoDebitTransaction.CreatedBy = _personnelId;
                                objAoDebitTransaction.UpdatedBy = _personnelId;
                                objAoDebitTransaction.CreateDate = DateTime.Now;
                                objAoDebitTransaction.UpdateDate = DateTime.Now;
                                _context.AccountAoTransaction.Add(objAoDebitTransaction);
                                _context.SaveChanges();
                                    // To Update Fee Pending Balances
                                    MoneyTransactionCalculateVm objAoFeeParam = new MoneyTransactionCalculateVm();
                                    objAoFeeParam.InmateId = param.InmateId;
                                    objAoFeeParam.TransactionAmount = newPayAmount;
                                    objAoFeeParam.BankId = accountAoInmate.AccountAoBankId;
                                    objAoFeeParam.FundId = accountAoFee.AccountAOFundId;
                                    objAoFeeParam.TransactionType = MoneyTransactionType.DEBIT;
                                    objAoFeeParam.Table = MoneyTransactionTable.ACCOUNTAO_FEE;
                                    MoneyBalanceVm objAoFeeBalance =  MoneyTransactionCalculateBalance(objAoFeeParam);
                                    objAoFeeBalance.InmateId = param.InmateId;
                                    objAoFeeBalance.BankId = accountAoInmate.AccountAoBankId;
                                    objAoFeeBalance.FundId = accountAoFee.AccountAOFundId;
                                    objAoFeeBalance.Table = MoneyTransactionTable.ACCOUNTAO_FEE;
                                    MoneyUpdateBalance(objAoFeeBalance);
                                    // To Update Credit Balances
                                    MoneyTransactionCalculateVm objAoTransCreditParam = new MoneyTransactionCalculateVm();
                                    objAoTransCreditParam.InmateId = param.InmateId;
                                    objAoTransCreditParam.TransactionAmount = newPayAmount;
                                    objAoTransCreditParam.BankId = accountAoInmate.AccountAoBankId;
                                    objAoTransCreditParam.FundId = accountAoFee.AccountAOFundId;
                                    objAoTransCreditParam.TransactionType = MoneyTransactionType.CREDIT;
                                    objAoTransCreditParam.Table = MoneyTransactionTable.ACCOUNTAO_TRANSACTION;
                                    MoneyBalanceVm objAoTransCreditBalance = MoneyTransactionCalculateBalance(objAoTransCreditParam);
                                    objAoTransCreditBalance.BankId = accountAoInmate.AccountAoBankId;
                                    objAoTransCreditBalance.FundId = accountAoFee.AccountAOFundId;
                                    objAoTransCreditBalance.Table = MoneyTransactionTable.ACCOUNTAO_TRANSACTION;
                                    MoneyUpdateBalance(objAoTransCreditBalance);
                                        // To Create Credit Tranaction in the Transaction Table
                                        AccountAoTransaction objAoCreditTransaction = new AccountAoTransaction();
                                        objAoCreditTransaction.InmateId = param.InmateId;
                                        objAoCreditTransaction.AccountAoFundId = accountAoFee.AccountAOFundId;
                                        objAoCreditTransaction.AccountAoFeeId = param.AccountAoFeeId;
                                        objAoCreditTransaction.TransactionOfficerId = _personnelId;
                                        objAoCreditTransaction.TransactionDate = DateTime.Now;
                                        objAoCreditTransaction.TransactionAmount = Math.Abs(newPayAmount);
                                        objAoCreditTransaction.TransactionType = MoneyTransactionType.CREDIT.ToString();
                                        objAoCreditTransaction.TransactionDescription = MoneyConstants.AUTOPAYFEE;
                                        objAoCreditTransaction.BalanceFund = objAoTransBalance.FundBalance;
                                        objAoCreditTransaction.BalanceAccount = objAoTransBalance.BankBalance;
                                        objAoCreditTransaction.AccountAoTransactionFromId = objAoDebitTransaction.AccountAoTransactionId;
                                        objAoCreditTransaction.CreatedBy = _personnelId;
                                        objAoCreditTransaction.UpdatedBy = _personnelId;
                                        objAoCreditTransaction.CreateDate = DateTime.Now;
                                        objAoCreditTransaction.UpdateDate = DateTime.Now;
                                        _context.AccountAoTransaction.Add(objAoCreditTransaction);
                                        _context.SaveChanges();
                                        // Full Payment of Fee Applied.
                                        _response.TotalPayAmount = _response.TotalPayAmount + newPayAmount;
                                        _response.StatusCode = 1;
                        }
                        // END for 3RD CONDITION - DEPOSIT FLAG
        }
        private void MoneyFeeCheckFlag(MoneyDebitCheckVm param,MoneyAccountAoFeeVm accountAoFee,MoneyAccountAoInmateVm accountAoInmate)
        {
                        decimal payAmount = accountAoFee.TransactionFee;
                        decimal newPayAmount = 0;
                        payAmount = (param.ThersholdAmount < payAmount) 
                        ? param.ThersholdAmount
                        : payAmount;
                        // BEGIN for 1ST CONDITION - FEE CHECK FLAG
                        if(accountAoInmate.BalanceInmate <= accountAoFee.FundInmateMinBalance)
                        {
                            // Insufficient Funds To Pay Fee.
                            _response.ReturnExitFlag = true;
                            _response.StatusCode = 3;
                            return;
                        }
                        // END for 1ST CONDITION - FEE CHECK FLAG  
                        // BEGIN for 2ND CONDITION - FEE CHECK FLAG
                        // PARTIAL PAYMENT 
                        if(((accountAoInmate.BalanceInmate - payAmount) < accountAoFee.FundInmateMinBalance)
                        || (((accountAoInmate.BalanceInmate - payAmount) >= accountAoFee.FundInmateMinBalance)
                        && payAmount < accountAoFee.TransactionFee))
                        {
                            // BELOW INMATES ALLOWED BALANCE FOR THE FUND
                            if((accountAoInmate.BalanceInmate - payAmount) < accountAoFee.FundInmateMinBalance)
                            {
                                // DO TRANSFER INMATE TO FUND of New PayAmount
                                newPayAmount = accountAoInmate.BalanceInmate - accountAoFee.FundInmateMinBalance;
                            }
                            if(((accountAoInmate.BalanceInmate - payAmount) >= accountAoFee.FundInmateMinBalance)
                            && payAmount < accountAoFee.TransactionFee)
                            {
                                // INMATE HAS ENOUGH FUNDS BUT PAYMENT AMOUNT IS LESS THEN TRANSACTION FEE, HENCE PARTIAL PAYMENT
                                newPayAmount = payAmount;
                            }
                            if(newPayAmount < 0)
                            {
                                // Payment Cannot Be Done
                                _response.StatusCode = 1;
                                return;
                            }
                            // To Update Debit Balances 
                            MoneyTransactionCalculateVm objAoTransParam = new MoneyTransactionCalculateVm();
                            objAoTransParam.InmateId = param.InmateId;
                            objAoTransParam.TransactionAmount = newPayAmount;
                            objAoTransParam.BankId = accountAoInmate.AccountAoBankId;
                            objAoTransParam.FundId = accountAoInmate.FundId;
                            objAoTransParam.TransactionType = MoneyTransactionType.DEBIT;
                            objAoTransParam.Table = MoneyTransactionTable.ACCOUNTAO_TRANSACTION;
                            MoneyBalanceVm objAoTransBalance =  MoneyTransactionCalculateBalance(objAoTransParam);
                            objAoTransBalance.InmateId = param.InmateId;
                            objAoTransBalance.BankId = accountAoInmate.AccountAoBankId;
                            objAoTransBalance.FundId = accountAoInmate.FundId;
                            objAoTransBalance.Table = MoneyTransactionTable.ACCOUNTAO_TRANSACTION;
                            MoneyUpdateBalance(objAoTransBalance);           
                                // To Create Debit Tranaction in the Transaction Table
                                AccountAoTransaction objAoDebitTransaction = new AccountAoTransaction();
                                objAoDebitTransaction.InmateId = param.InmateId;
                                objAoDebitTransaction.AccountAoFundId = accountAoInmate.FundId;
                                objAoDebitTransaction.AccountAoFeeId = param.AccountAoFeeId;
                                objAoDebitTransaction.TransactionOfficerId = _personnelId;
                                objAoDebitTransaction.TransactionDate = DateTime.Now;
                                objAoDebitTransaction.TransactionAmount = -1 * Math.Abs(newPayAmount);
                                objAoDebitTransaction.TransactionType = MoneyTransactionType.DEBIT.ToString();
                                objAoDebitTransaction.TransactionDescription = MoneyConstants.AUTOPAYFEE;
                                objAoDebitTransaction.BalanceInmate = objAoTransBalance.InmateBalance;
                                objAoDebitTransaction.BalanceFund = objAoTransBalance.FundBalance;
                                objAoDebitTransaction.BalanceAccount = objAoTransBalance.BankBalance;
                                objAoDebitTransaction.CreatedBy = _personnelId;
                                objAoDebitTransaction.UpdatedBy = _personnelId;
                                objAoDebitTransaction.CreateDate = DateTime.Now;
                                objAoDebitTransaction.UpdateDate = DateTime.Now;
                                _context.AccountAoTransaction.Add(objAoDebitTransaction);
                                _context.SaveChanges();
                                // Update Fee Pending Balances
                                    MoneyTransactionCalculateVm objAoFeeParam = new MoneyTransactionCalculateVm();
                                    objAoFeeParam.InmateId = param.InmateId;
                                    objAoFeeParam.TransactionAmount = newPayAmount;
                                    objAoFeeParam.BankId = accountAoInmate.AccountAoBankId;
                                    objAoFeeParam.FundId = accountAoFee.AccountAOFundId;
                                    objAoFeeParam.TransactionType = MoneyTransactionType.DEBIT;
                                    objAoFeeParam.Table = MoneyTransactionTable.ACCOUNTAO_FEE;
                                    MoneyBalanceVm objAoFeeBalance =  MoneyTransactionCalculateBalance(objAoFeeParam);
                                    objAoFeeBalance.InmateId = param.InmateId;
                                    objAoFeeBalance.BankId = accountAoInmate.AccountAoBankId;
                                    objAoFeeBalance.FundId =  accountAoFee.AccountAOFundId;
                                    objAoFeeBalance.Table = MoneyTransactionTable.ACCOUNTAO_FEE;
                                    MoneyUpdateBalance(objAoFeeBalance);
                                    // Update Credit Balances
                                    MoneyTransactionCalculateVm objAoFeeCreditParam = new MoneyTransactionCalculateVm();
                                    objAoFeeCreditParam.InmateId = param.InmateId;
                                    objAoFeeCreditParam.TransactionAmount = newPayAmount;
                                    objAoFeeCreditParam.BankId = accountAoInmate.AccountAoBankId;
                                    objAoFeeCreditParam.FundId = accountAoFee.AccountAOFundId;
                                    objAoFeeCreditParam.TransactionType = MoneyTransactionType.CREDIT;
                                    objAoFeeCreditParam.Table = MoneyTransactionTable.ACCOUNTAO_TRANSACTION;
                                    MoneyBalanceVm objAoFeeCreditBalance =  MoneyTransactionCalculateBalance(objAoFeeCreditParam);
                                    objAoFeeCreditBalance.BankId = accountAoInmate.AccountAoBankId;
                                    objAoFeeCreditBalance.FundId =  accountAoFee.AccountAOFundId;
                                    objAoFeeCreditBalance.Table = MoneyTransactionTable.ACCOUNTAO_TRANSACTION;
                                    MoneyUpdateBalance(objAoFeeCreditBalance);
                                        // To Create Credit Tranaction in the Transaction Table
                                        AccountAoTransaction objAoCreditTransaction = new AccountAoTransaction();
                                        objAoCreditTransaction.InmateId = param.InmateId;
                                        objAoCreditTransaction.AccountAoFundId = accountAoFee.AccountAOFundId;
                                        objAoCreditTransaction.AccountAoFeeId = param.AccountAoFeeId;
                                        objAoCreditTransaction.TransactionOfficerId = _personnelId;
                                        objAoCreditTransaction.TransactionDate = DateTime.Now;
                                        objAoCreditTransaction.TransactionAmount = Math.Abs(newPayAmount);
                                        objAoCreditTransaction.TransactionType = MoneyTransactionType.CREDIT.ToString();
                                        objAoCreditTransaction.TransactionDescription = MoneyConstants.AUTOPAYFEE;
                                        objAoCreditTransaction.BalanceFund = objAoTransBalance.FundBalance;
                                        objAoCreditTransaction.BalanceAccount = objAoTransBalance.BankBalance;
                                        objAoCreditTransaction.AccountAoTransactionFromId = 
                                        objAoDebitTransaction.AccountAoTransactionId;
                                        objAoCreditTransaction.CreatedBy = _personnelId;
                                        objAoCreditTransaction.UpdatedBy = _personnelId;
                                        objAoCreditTransaction.CreateDate = DateTime.Now;
                                        objAoCreditTransaction.UpdateDate = DateTime.Now;
                                        _context.AccountAoTransaction.Add(objAoCreditTransaction);
                                        _context.SaveChanges();
                                        // To Update Transaction To Id
                                            AccountAoTransaction objAoTransaction = 
                                            _context.AccountAoTransaction.Find(objAoDebitTransaction.AccountAoTransactionId);
                                            objAoTransaction.AccountAoTransactionToId = objAoCreditTransaction.AccountAoTransactionId;
                                            _context.SaveChanges();
                                            // To Update AccountAo_Fee Table [Clearing the Old Fee]
                                                AccountAoFee objAoFee = _context.AccountAoFee.Find(param.AccountAoFeeId);
                                                objAoFee.TransactionCleared =1;
                                                objAoFee.TransactionClearedDate = DateTime.Now;
                                                objAoFee.TransactionClearedBy = _personnelId;
                                                objAoFee.TransactionClearedAmount = newPayAmount;
                                                objAoFee.UpdateDate = DateTime.Now;
                                                objAoFee.UpdatedBy = _personnelId;
                                                _context.SaveChanges();
                                                _response.PartialPayAmount = newPayAmount;
                                                decimal newFee = accountAoFee.TransactionFee - newPayAmount;
                                                _response.NewFee = newFee;
                                                // To Insert New Amount in the AccountAo_Fee table [Inserting the New Fee]
                                                    if(newFee > 0)
                                                    {
                                                        AccountAoFee objNewAoFee = new AccountAoFee();
                                                        objNewAoFee.InmateId = objAoFee.InmateId;
                                                        objNewAoFee.AccountAoFundId = objAoFee.AccountAoFundId;
                                                        objNewAoFee.CreateDate = objAoFee.CreateDate;
                                                        objNewAoFee.CreatedBy = objAoFee.CreatedBy;
                                                        objNewAoFee.TransactionDate = objAoFee.TransactionDate;
                                                        objNewAoFee.TransactionOfficerId = objAoFee.TransactionOfficerId;
                                                        objNewAoFee.TransactionDescription = MoneyConstants.PARTIALPAID;
                                                        objNewAoFee.TransactionFee = newFee;
                                                        objNewAoFee.BalanceInmateFee = objAoFeeBalance.InmateBalance;
                                                        objNewAoFee.BalanceFundFee = objAoFeeBalance.FundBalance;
                                                        objNewAoFee.BalanceAccountFee = objAoFeeBalance.BankBalance;
                                                        objNewAoFee.AdjustmentAccountAoFeeId = param.AccountAoFeeId;
                                                        objNewAoFee.UpdateDate = DateTime.Now;
                                                        objNewAoFee.UpdatedBy = _personnelId;
                                                        _context.AccountAoFee.Add(objNewAoFee);
                                                        _context.SaveChanges();
                                                    }
                                                    // Partial Payment Applied. New Fee Created
                                                    _response.TotalPayAmount = _response.TotalPayAmount + newFee;
                                                    _response.StatusCode = 4;
                                                    return;
                        }
                        // END for 2ND CONDITION - FEE CHECK FLAG
                        // BEGIN for 3RD CONDITION - FEE CHECK FLAG
                        // FULL PAYMENT 
                        if((accountAoInmate.BalanceInmate - payAmount) >= accountAoFee.FundInmateMinBalance)
                        {
                            AccountAoFee objAoFee = _context.AccountAoFee.Find(param.AccountAoFeeId);
                            objAoFee.TransactionCleared = 1;
                            objAoFee.TransactionClearedDate = DateTime.Now;
                            objAoFee.TransactionClearedBy = _personnelId;
                            objAoFee.TransactionClearedAmount = payAmount;
                            objAoFee.UpdateDate = DateTime.Now;
                            objAoFee.UpdatedBy = _personnelId;
                            _context.SaveChanges();
                            newPayAmount = payAmount;
                            // To Update Fee Pending Balances  
                            MoneyTransactionCalculateVm objAoTransParam = new MoneyTransactionCalculateVm();
                            objAoTransParam.InmateId = param.InmateId;
                            objAoTransParam.TransactionAmount = newPayAmount;
                            objAoTransParam.BankId = accountAoInmate.AccountAoBankId;
                            objAoTransParam.FundId = accountAoInmate.FundId;
                            objAoTransParam.TransactionType = MoneyTransactionType.DEBIT;
                            objAoTransParam.Table = MoneyTransactionTable.ACCOUNTAO_TRANSACTION;
                            MoneyBalanceVm objAoTransBalance =  MoneyTransactionCalculateBalance(objAoTransParam);
                            objAoTransBalance.InmateId = param.InmateId;
                            objAoTransBalance.BankId = accountAoInmate.AccountAoBankId;
                            objAoTransBalance.FundId = accountAoInmate.FundId;
                            objAoTransBalance.Table = MoneyTransactionTable.ACCOUNTAO_TRANSACTION;
                            MoneyUpdateBalance(objAoTransBalance);
                                // To Create Debit Tranaction in the Transaction Table
                                AccountAoTransaction objAoDebitTransaction = new AccountAoTransaction();
                                objAoDebitTransaction.InmateId = param.InmateId;
                                objAoDebitTransaction.AccountAoFundId = accountAoInmate.FundId;
                                objAoDebitTransaction.AccountAoFeeId = param.AccountAoFeeId;
                                objAoDebitTransaction.TransactionOfficerId = _personnelId;
                                objAoDebitTransaction.TransactionDate = DateTime.Now;
                                objAoDebitTransaction.TransactionAmount = -1 * Math.Abs(newPayAmount);
                                objAoDebitTransaction.TransactionType = MoneyTransactionType.DEBIT.ToString();
                                objAoDebitTransaction.TransactionDescription = MoneyConstants.AUTOPAYFEE;
                                objAoDebitTransaction.BalanceInmate = objAoTransBalance.InmateBalance;
                                objAoDebitTransaction.BalanceFund = objAoTransBalance.FundBalance;
                                objAoDebitTransaction.BalanceAccount = objAoTransBalance.BankBalance;
                                objAoDebitTransaction.CreatedBy = _personnelId;
                                objAoDebitTransaction.UpdatedBy = _personnelId;
                                objAoDebitTransaction.CreateDate = DateTime.Now;
                                objAoDebitTransaction.UpdateDate = DateTime.Now;
                                _context.AccountAoTransaction.Add(objAoDebitTransaction);
                                _context.SaveChanges();
                                // To Update Fee Pending Balances
                                    MoneyTransactionCalculateVm objAoFeeParam = new MoneyTransactionCalculateVm();
                                    objAoFeeParam.InmateId = param.InmateId;
                                    objAoFeeParam.TransactionAmount = newPayAmount;
                                    objAoFeeParam.BankId = accountAoInmate.AccountAoBankId;
                                    objAoFeeParam.FundId = accountAoFee.AccountAOFundId;
                                    objAoFeeParam.TransactionType = MoneyTransactionType.DEBIT;
                                    objAoFeeParam.Table = MoneyTransactionTable.ACCOUNTAO_FEE;
                                    MoneyBalanceVm objAoFeeBalance =  MoneyTransactionCalculateBalance(objAoFeeParam);
                                    objAoFeeBalance.InmateId = param.InmateId;
                                    objAoFeeBalance.BankId = accountAoInmate.AccountAoBankId;
                                    objAoFeeBalance.FundId = accountAoFee.AccountAOFundId;
                                    objAoFeeBalance.Table = MoneyTransactionTable.ACCOUNTAO_FEE;
                                    MoneyUpdateBalance(objAoFeeBalance);
                                    // To Update Credit Balances
                                    MoneyTransactionCalculateVm objAoTransCreditParam = new MoneyTransactionCalculateVm();
                                    objAoTransCreditParam.InmateId = param.InmateId;
                                    objAoTransCreditParam.TransactionAmount = newPayAmount;
                                    objAoTransCreditParam.BankId = accountAoInmate.AccountAoBankId;
                                    objAoTransCreditParam.FundId = accountAoFee.AccountAOFundId;
                                    objAoTransCreditParam.TransactionType = MoneyTransactionType.CREDIT;
                                    objAoTransCreditParam.Table = MoneyTransactionTable.ACCOUNTAO_TRANSACTION;
                                    MoneyBalanceVm objAoTransCreditBalance =  MoneyTransactionCalculateBalance(objAoTransCreditParam);
                                    objAoTransCreditBalance.InmateId = param.InmateId;
                                    objAoTransCreditBalance.BankId = accountAoInmate.AccountAoBankId;
                                    objAoTransCreditBalance.FundId = accountAoFee.AccountAOFundId;
                                    objAoTransCreditBalance.Table = MoneyTransactionTable.ACCOUNTAO_TRANSACTION;
                                    MoneyUpdateBalance(objAoTransCreditBalance);
                                        // To Create Credit Tranaction in the Transaction Table
                                        AccountAoTransaction objAoCreditTransaction = new AccountAoTransaction();
                                        objAoCreditTransaction.InmateId = param.InmateId;
                                        objAoCreditTransaction.AccountAoFundId = accountAoFee.AccountAOFundId;
                                        objAoCreditTransaction.AccountAoFeeId = param.AccountAoFeeId;
                                        objAoCreditTransaction.TransactionOfficerId = _personnelId;
                                        objAoCreditTransaction.TransactionDate = DateTime.Now;
                                        objAoCreditTransaction.TransactionAmount = Math.Abs(newPayAmount);
                                        objAoCreditTransaction.TransactionType = MoneyTransactionType.CREDIT.ToString();
                                        objAoCreditTransaction.TransactionDescription = MoneyConstants.AUTOPAYFEE;
                                        objAoCreditTransaction.BalanceFund = objAoTransBalance.FundBalance;
                                        objAoCreditTransaction.BalanceAccount = objAoTransBalance.BankBalance;
                                        objAoCreditTransaction.AccountAoTransactionFromId = objAoDebitTransaction.AccountAoTransactionId;
                                        objAoCreditTransaction.CreatedBy = _personnelId;
                                        objAoCreditTransaction.UpdatedBy = _personnelId;
                                        objAoCreditTransaction.CreateDate = DateTime.Now;
                                        objAoCreditTransaction.UpdateDate = DateTime.Now;
                                        _context.AccountAoTransaction.Add(objAoCreditTransaction);
                                        _context.SaveChanges();
                                        // Full Payment of Fee Applied.
                                        _response.TotalPayAmount = _response.TotalPayAmount+newPayAmount;
                                        _response.StatusCode = 6;
                        }
                        // END for 3RD CONDITION - FEE CHECK FLAG
        }
        #endregion

        #region  Transaction calculation
        public MoneyBalanceVm MoneyTransactionCalculateBalance(MoneyTransactionCalculateVm param)
        {
            MoneyBalanceVm balanceDetails = new  MoneyBalanceVm();
            switch(param.Table)
            {
                case MoneyTransactionTable.ACCOUNTAO_TRANSACTION:
                    if (param.InmateId > 0)
                    {
                        balanceDetails.InmateBalance = _context.AccountAoInmate
                            .Single(s => s.InmateId == param.InmateId && s.AccountAoBankId == param.BankId)
                            .BalanceInmate;
                    }

                    balanceDetails.BankBalance = _context.AccountAoBank
                    .Single(s=>s.AccountAoBankId == param.BankId).BalanceAccount;
                    decimal? balanceFund = _context.AccountAoFund
                        .Single(s => s.AccountAoBankId == param.BankId && s.AccountAoFundId == param.FundId)
                        .BalanceFund;
                    balanceDetails.FundBalance =  balanceFund ?? 0;
                    break;
                case MoneyTransactionTable.ACCOUNTAO_RECEIVE:
                    if (param.InmateId > 0)
                    {
                        balanceDetails.InmateBalance = _context.AccountAoInmate
                                          .Single(s => s.InmateId == param.InmateId && s.AccountAoBankId == param.BankId).BalanceInmatePending;
                    }
                      
                    decimal? balanceAccountPending = _context.AccountAoBank
                        .Single(s => s.AccountAoBankId == param.BankId).BalanceAccountPending;
                    balanceDetails.BankBalance = balanceAccountPending ?? 0;
                    decimal? balanceFundPending = _context.AccountAoFund
                        .Single(s => s.AccountAoBankId == param.BankId && s.AccountAoFundId == param.FundId)
                        .BalanceFundPending;
                    balanceDetails.FundBalance = balanceFundPending ?? 0;
                    break;
                case MoneyTransactionTable.ACCOUNTAO_FEE:
                    if (param.InmateId > 0)
                    {
                        balanceDetails.InmateBalance = _context.AccountAoInmate
                    .Single(s => s.InmateId == param.InmateId && s.AccountAoBankId == param.BankId).BalanceInmateFee;
                    }
                    decimal? balanceAccountFee = _context.AccountAoBank
                        .Single(s => s.AccountAoBankId == param.BankId).BalanceAccountFee;
                    balanceDetails.BankBalance = balanceAccountFee ?? 0;
                    decimal? balanceFundFee = _context.AccountAoFund
                        .Single(s => s.AccountAoBankId == param.BankId && s.AccountAoFundId == param.FundId)
                        .BalanceFundFee;
                    balanceDetails.FundBalance = balanceFundFee ?? 0;
                    break;
            }
            if(param.TransactionType == MoneyTransactionType.DEBIT)
            {
                balanceDetails.InmateBalance = balanceDetails.InmateBalance > 0 
                ? balanceDetails.InmateBalance - param.TransactionAmount : 0;
                balanceDetails.BankBalance = balanceDetails.BankBalance > 0
                ? balanceDetails.BankBalance - param.TransactionAmount : 0;
                balanceDetails.FundBalance = balanceDetails.FundBalance > 0 
                ? balanceDetails.FundBalance - param.TransactionAmount : 0;
            } else 
            {
                balanceDetails.InmateBalance = balanceDetails.InmateBalance + param.TransactionAmount;
                balanceDetails.BankBalance = balanceDetails.BankBalance + param.TransactionAmount;
                balanceDetails.FundBalance = balanceDetails.FundBalance + param.TransactionAmount;
            }
            return balanceDetails;
        }
        #endregion
        
        #region  Money update balance
        public MoneyBalanceVm MoneyUpdateBalance(MoneyBalanceVm param)
        {
            MoneyBalanceVm objResponse = new MoneyBalanceVm();
            switch(param.Table)
            {
                case MoneyTransactionTable.ACCOUNTAO_TRANSACTION:
                {
                    if(param.InmateId > 0)
                    {
                        AccountAoInmate aoInmate = _context.AccountAoInmate
                        .Single(s=>s.InmateId == param.InmateId && s.AccountAoBankId == param.BankId);
                        aoInmate.BalanceInmate = param.InmateBalance;
                    }
                    AccountAoFund aoFund = _context.AccountAoFund.Find(param.FundId);
                    aoFund.BalanceFund = param.FundBalance;
                    aoFund.UpdateBy = _personnelId;
                    aoFund.UpdateDate = DateTime.Now;
                    AccountAoBank aoBank = _context.AccountAoBank.Find(param.BankId);
                    aoBank.BalanceAccount = param.BankBalance;
                    aoBank.UpdateBy = _personnelId;
                    _context.SaveChanges();
                    objResponse.StatusCode = 1;
                }
                break;
                case MoneyTransactionTable.ACCOUNTAO_RECEIVE:
                {
                    if(param.InmateId > 0)
                    {
                        AccountAoInmate aoInmate = _context.AccountAoInmate
                        .Single(s=>s.InmateId == param.InmateId && s.AccountAoBankId == param.BankId);
                        aoInmate.BalanceInmatePending = param.InmateBalance;
                    }
                    AccountAoFund aoFund = _context.AccountAoFund.Find(param.FundId);
                    aoFund.BalanceFundPending = param.FundBalance;
                    aoFund.UpdateBy = _personnelId;
                    aoFund.UpdateDate = DateTime.Now;
                    AccountAoBank aoBank = _context.AccountAoBank.Find(param.BankId);
                    aoBank.BalanceAccountPending = param.BankBalance;
                    aoBank.UpdateBy = _personnelId;
                    aoBank.UpdateDate = DateTime.Now;
                    _context.SaveChanges();
                    objResponse.StatusCode = 1;
                }
                break;
                case MoneyTransactionTable.ACCOUNTAO_FEE:
                {
                    if(param.InmateId > 0)
                    {
                        AccountAoInmate aoInmate = _context.AccountAoInmate
                        .Single(s=>s.InmateId == param.InmateId && s.AccountAoBankId == param.BankId);
                        aoInmate.BalanceInmateFee = param.InmateBalance;
                    }
                    AccountAoFund aoFund = _context.AccountAoFund.Find(param.FundId);
                    aoFund.BalanceFundFee = param.FundBalance;
                    aoFund.UpdateBy = _personnelId;
                    aoFund.UpdateDate = param.TransactionDate ?? DateTime.Now;
                    AccountAoBank aoBank = _context.AccountAoBank.Find(param.BankId);
                    aoBank.BalanceAccountFee = param.BankBalance;
                    aoBank.UpdateBy = _personnelId;
                    aoBank.UpdateDate = param.TransactionDate ?? DateTime.Now;
                    _context.SaveChanges();
                    objResponse.StatusCode = 1;
                }
                break;
            }
            return objResponse;
        }
        #endregion
    }
}