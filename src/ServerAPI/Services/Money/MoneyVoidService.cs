using System;
using GenerateTables.Models;
using ServerAPI.ViewModels;
using System.Collections.Generic;
using System.Linq;
using ServerAPI.Utilities;
using Microsoft.AspNetCore.Http;
// ReSharper disable once CheckNamespace
namespace ServerAPI.Services
{
    public class MoneyVoidService : IMoneyVoidService
    {
        private readonly AAtims _context;
        private readonly int _personnelId;
        private IQueryable<MoneyAccountTransactionVm> _moneyTransaction;
        private IQueryable<MoneyVoidTransactionFee> _moneyVoidTransactionFee;
        private MoneyAccountVoidDetailsVm _moneyAccountVoidDetails;
        private MoneyAccountTransactionVm _moneyAccountTransaction;
        private readonly IMoneyBalanceDebitCheck _moneyBalanceDebitCheck;

        public MoneyVoidService(AAtims context, IHttpContextAccessor httpContextAccessor,
            IMoneyBalanceDebitCheck moneyBalanceDebitCheck)
        {
            _context = context;
            _personnelId = Convert.ToInt32(httpContextAccessor.HttpContext.User.FindFirst("personnelId")?.Value);
            _moneyBalanceDebitCheck = moneyBalanceDebitCheck;
        }

        //get void details for Transaction
        public MoneyAccountVoidDetailsVm GetDoVoidDetails(int transId, MoneyAccountTransactionFlagType pFlag)
        {
            _moneyAccountVoidDetails = new MoneyAccountVoidDetailsVm();
            switch (pFlag)
            {
                case MoneyAccountTransactionFlagType.PENDING:
                    PendingAccountTransaction(transId);
                    break;
                case MoneyAccountTransactionFlagType.UNPAID:
                    UnpaidTransaction(transId);
                    break;
                case MoneyAccountTransactionFlagType.ACCOUNTTRANSACTION:
                    AccountTransactionLedger(transId);
                    break;
            }

            if ((pFlag == MoneyAccountTransactionFlagType.PENDING || pFlag == MoneyAccountTransactionFlagType.UNPAID)
                && _moneyAccountTransaction != null)
            {
                _moneyAccountVoidDetails.Flag = _moneyAccountTransaction.Flag;
                if (_moneyAccountTransaction.ClearFlag)
                {
                    _moneyAccountVoidDetails.ReturnCashList.Add(_moneyAccountTransaction);
                }
                else if (!_moneyAccountTransaction.VoidFlag)
                {
                    _moneyAccountVoidDetails.ReturnCashList.Add(_moneyAccountTransaction);
                }
                else if (_moneyAccountTransaction.VoidFlag)
                {
                    MoneyAccountTransactionVm childInOutTrans =
                        _moneyTransaction.SingleOrDefault(t => t.TransactionId == transId);
                    MoneyAccountTransactionVm childVoidTrans =
                        _moneyTransaction.SingleOrDefault(t => t.TransactionVoidId == transId);

                    if (childInOutTrans != null && childVoidTrans != null)
                    {
                        if (childInOutTrans.TransactionId < childVoidTrans.TransactionId)
                        {
                            _moneyAccountVoidDetails.ChildTransactionList.Add(childInOutTrans);
                            _moneyAccountVoidDetails.VoidChildTransactionList.Add(childVoidTrans);
                        }
                        else
                        {
                            _moneyAccountVoidDetails.ChildTransactionList.Add(childVoidTrans);
                            _moneyAccountVoidDetails.VoidChildTransactionList.Add(childInOutTrans);
                        }
                    } else{
                          _moneyAccountVoidDetails.VoidChildTransactionList.Add(childInOutTrans);
                    }
                }
            }
            return _moneyAccountVoidDetails;
        }
        //Do void insert for transaction
        public int InsertDoVoid(SetVoidVm voidDetails)
        {
            int statusCode = 0;
            MoneyBalanceVm balanceDetails = new MoneyBalanceVm();
            MoneyVoidTransactionFee returnTransDetail;
            if (voidDetails.Flag == MoneyAccountTransactionFlagType.PENDING)
            {
                _moneyVoidTransactionFee = _context.AccountAoReceive
                    .Select(a =>
                        new MoneyVoidTransactionFee
                        {
                            BankId = a.AccountAoFund.AccountAoBankId,
                            FundId = a.AccountAoFundId,
                            TransactionDescription = a.TransactionDescription.ToUpper(),
                            ReturnCashFlag = a.TransactionReceiveCashFlag == 1,
                            ReceiveFrm = a.TransactionReceiveFrom,
                            ReceiveNumber = a.TransactionReceiveNumber,
                            AccountAoDepositoryId = a.AccountAoDepositoryId,
                            CurrentTransactionId = a.AccountAoReceiveId,
                            InmateId = a.InmateId ?? 0
                        });
                voidDetails.TransactionDetail =
                    _moneyVoidTransactionFee.SingleOrDefault(a => a.CurrentTransactionId == voidDetails.TransactionId);
                returnTransDetail =
                    _moneyVoidTransactionFee.SingleOrDefault(a => a.CurrentTransactionId == voidDetails.ReturnTransactionId);
            }
            else if (voidDetails.Flag == MoneyAccountTransactionFlagType.UNPAID)
            {
                _moneyVoidTransactionFee = _context.AccountAoFee.Select(a =>
                    new MoneyVoidTransactionFee
                    {
                        BankId = a.AccountAoFund.AccountAoBankId,
                        FundId = a.AccountAoFundId,
                        TransactionDescription = a.TransactionDescription.ToUpper(),
                        CurrentTransactionId = a.AccountAoFeeId,
                        InmateId = a.InmateId ?? 0
                    });
                voidDetails.TransactionDetail =
                    _moneyVoidTransactionFee.SingleOrDefault(a => a.CurrentTransactionId == voidDetails.TransactionId);
                returnTransDetail =
                    _moneyVoidTransactionFee.SingleOrDefault(a => a.CurrentTransactionId == voidDetails.ReturnTransactionId);
            }
            else
            {
                _moneyVoidTransactionFee = _context.AccountAoTransaction.Select(a =>
                    new MoneyVoidTransactionFee
                    {
                        FundId = a.AccountAoFundId,
                        BankId = a.AccountAoFund.AccountAoBankId,
                        TransactionDescription = a.TransactionDescription.ToUpper(),
                        ReturnCashFlag = a.TransactionReceiveCashFlag == 1,
                        ReceiveFrm = a.TransactionReceiveFrom,
                        ReceiveNumber = a.TransactionReceiveNumber,
                        CurrentTransactionId = a.AccountAoTransactionId,
                        FeeId = a.AccountAoFeeId ?? 0,
                        ReceiveId = a.AccountAoReceiveId ?? 0,
                        PayToOrder = a.TransactionPayToTheOrderOf,
                        CheckNo = a.TransactionCheckNumber,
                        FromTransactionId = a.AccountAoTransactionFromId ?? 0,
                        ToTransactionId = a.AccountAoTransactionToId ?? 0,
                        DebitMemo = a.TransactionDebitMemo,
                        InmateId = a.InmateId ?? 0
                    });
                voidDetails.TransactionDetail =
                    _moneyVoidTransactionFee.SingleOrDefault(a => a.CurrentTransactionId == voidDetails.TransactionId);
                returnTransDetail = _moneyVoidTransactionFee.SingleOrDefault(a => a.CurrentTransactionId == voidDetails.ReturnTransactionId);
            }

            if (voidDetails.TransactionDetail != null && returnTransDetail != null)
            {
                returnTransDetail.BankId = voidDetails.TransactionDetail.BankId;
                returnTransDetail.ReturnCashFlag = voidDetails.Flag != MoneyAccountTransactionFlagType.MOVETRANS && returnTransDetail.ReturnCashFlag;
                returnTransDetail.TransactionAmount = voidDetails.ReturnAmount;
                returnTransDetail.TransactionType = voidDetails.ReturnTransactionType;
                returnTransDetail.TransactionDescription = voidDetails.ReturnTransactionDescription;
                returnTransDetail.CurrentTransactionDescription = voidDetails.TransactionDetail.TransactionDescription;
                if (voidDetails.Flag == MoneyAccountTransactionFlagType.PENDING ||
                    voidDetails.Flag == MoneyAccountTransactionFlagType.UNPAID)
                {
                    AccountAoInmate newInmateId = _context.AccountAoInmate.SingleOrDefault(a =>
                        a.InmateId == returnTransDetail.InmateId
                        && a.AccountAoBankId == returnTransDetail.BankId);
                    if (newInmateId == null && returnTransDetail.InmateId > 0)
                    {
                        AccountAoInmate accountAoInmate = new AccountAoInmate
                        {
                            AccountAoBankId = returnTransDetail.BankId,
                            InmateId = returnTransDetail.InmateId
                        };
                        _context.AccountAoInmate.Add(accountAoInmate);
                        _context.SaveChanges();
                    }

                    MoneyTransactionCalculateVm param = new MoneyTransactionCalculateVm
                    {
                        BankId = returnTransDetail.BankId,
                        FundId = returnTransDetail.FundId,
                        InmateId = returnTransDetail.InmateId,
                        TransactionAmount = voidDetails.Flag == MoneyAccountTransactionFlagType.PENDING
                            ? Math.Abs(returnTransDetail.TransactionAmount)
                            : returnTransDetail.TransactionAmount,
                        TransactionType = returnTransDetail.TransactionType == MoneyConstants.DEBIT ?
                            MoneyTransactionType.DEBIT : MoneyTransactionType.CREDIT,
                        Table = voidDetails.Flag == MoneyAccountTransactionFlagType.PENDING
                            ? MoneyTransactionTable.ACCOUNTAO_RECEIVE
                            : MoneyTransactionTable.ACCOUNTAO_FEE
                    };
                    balanceDetails = CashUpdateBalance(param);
                }

                switch (voidDetails.Flag)
                {
                    case MoneyAccountTransactionFlagType.PENDING:
                        {
                            returnTransDetail.ReturnCashFlag = voidDetails.TransactionDetail.ReturnCashFlag;
                            returnTransDetail.ReceiveFrm = voidDetails.TransactionDetail.ReceiveFrm;
                            returnTransDetail.ReceiveNumber = voidDetails.TransactionDetail.ReceiveNumber;
                            returnTransDetail.AccountAoDepositoryId =
                                voidDetails.TransactionDetail.AccountAoDepositoryId;
                            int num = _context.AccountAoBank.Single(b => b.AccountAoBankId == returnTransDetail.BankId)
                                          .NextReceiptNum ?? 0;
                            returnTransDetail.NextReceiptNum = voidDetails.ReceiptNumber + num;
                            returnTransDetail.NextReceiptNum = returnTransDetail.NextReceiptNum
                                .Substring(returnTransDetail.NextReceiptNum.Length - 8);
                            returnTransDetail.CurrentTransactionId = voidDetails.TransactionDetail.CurrentTransactionId;
                            statusCode = PendingReceiveDoVoid(returnTransDetail, balanceDetails);
                            break;
                        }
                    case MoneyAccountTransactionFlagType.UNPAID:
                        {
                            returnTransDetail.CurrentTransactionId = voidDetails.TransactionId;
                            statusCode = UnpaidDoVoid(returnTransDetail, balanceDetails);
                            break;
                        }
                    case MoneyAccountTransactionFlagType.INOUTTRANS:
                        {
                            returnTransDetail.FeeId = voidDetails.TransactionDetail.FeeId;
                            returnTransDetail.ReceiveId = voidDetails.TransactionDetail.ReceiveId;
                            returnTransDetail.ReceiveFrm = voidDetails.TransactionDetail.ReceiveFrm;
                            returnTransDetail.PayToOrder = voidDetails.TransactionDetail.PayToOrder;
                            returnTransDetail.CheckNo = voidDetails.TransactionDetail.CheckNo;
                            returnTransDetail.DebitMemo = voidDetails.TransactionDetail.DebitMemo;
                            returnTransDetail.CurrentTransactionId = voidDetails.TransactionDetail.CurrentTransactionId;
                            if (voidDetails.ReturnTransactionType == MoneyConstants.DEBIT)
                            {
                                int num = _context.AccountAoBank
                                              .Single(b => b.AccountAoBankId == returnTransDetail.BankId)
                                              .NextReceiptNum ?? 0;

                                returnTransDetail.NextReceiptNum = voidDetails.ReceiptNumber + num;
                                returnTransDetail.NextReceiptNum = returnTransDetail.NextReceiptNum
                                    .Substring(returnTransDetail.NextReceiptNum.Length - 8);
                            }

                            bool fundChk = false;
                            if (voidDetails.TransactionDetail.TransactionDescription == MoneyConstants.AUTOPAYFEE
                                && voidDetails.ReturnTransactionType == MoneyConstants.DEBIT)
                            {
                                SaveInOutFundVoidTransaction(voidDetails, false);
                                fundChk = true;
                            }

                            if (returnTransDetail.InmateId > 0)
                            {
                                statusCode = fundChk ? SaveFundVoidTransaction(returnTransDetail) : SaveInOutVoidTransaction(returnTransDetail);
                                if (voidDetails.TransactionDetail.TransactionDescription == MoneyConstants.AUTOPAYFEE
                                    && voidDetails.ReturnTransactionType == MoneyConstants.CREDIT)
                                {
                                    statusCode = SaveInOutFundVoidTransaction(voidDetails, true);
                                }
                            }
                            else
                            {
                                returnTransDetail.ReceiveNumber = returnTransDetail.CurrentTransactionDescription ==
                                                                  MoneyConstants.TransferCashDrawerToBank ? ""
                                    : returnTransDetail.ReceiveNumber;
                
                                statusCode = SaveFundVoidTransaction(returnTransDetail);
                            }

                            break;
                        }
                    case MoneyAccountTransactionFlagType.MOVETRANS:
                    case MoneyAccountTransactionFlagType.RETURNCASHVOID:
                    case MoneyAccountTransactionFlagType.APPLYFEE:
                        {
                            statusCode = SaveReturnCashMoveVoidTransaction(returnTransDetail, voidDetails);
                            if (statusCode > 0 && voidDetails.Flag == MoneyAccountTransactionFlagType.RETURNCASHVOID
                            || voidDetails.Flag == MoneyAccountTransactionFlagType.MOVETRANS)
                            {
                                statusCode = voidDetails.CashDrawerTransactionId > 0 ? CashDrawerVoidTransaction(voidDetails) : 1;
                            }
                            if (voidDetails.Flag == MoneyAccountTransactionFlagType.APPLYFEE)
                            {
                                AccountAoFee adjustFee = _context.AccountAoFee.FirstOrDefault(s =>
                                    s.AdjustmentAccountAoFeeId
                                    == voidDetails.TransactionDetail.FeeId);
                                if (adjustFee != null && adjustFee.AccountAoFeeId > 0)
                                {
                                    MoneyVoidTransactionFee adjustmentTransaction = _context.AccountAoFee
                                        .Where(x => x.AccountAoFeeId == adjustFee.AccountAoFeeId||
                                                    (x.AdjustmentAccountAoFeeId == adjustFee.AccountAoFeeId &&
                                                     (!x.TransactionVoidFlag.HasValue || x.TransactionVoidFlag == 0)
                                                     && (!x.TransactionCleared.HasValue || x.TransactionCleared == 0))).Select(s =>
                                            new MoneyVoidTransactionFee
                                            {
                                                FeeId = s.AccountAoFeeId,
                                                TransactionAmount =
                                                    ((s.TransactionCleared.HasValue) || (s.TransactionCleared == 0))
                                                        ? s.TransactionFee ?? 0
                                                        : s.TransactionClearedAmount ?? 0,
                                                TransactionDescription = s.TransactionDescription,
                                                InmateId = s.InmateId ?? 0,
                                                FundId = s.AccountAoFund.AccountAoFundId,
                                                TransactionClearedBy = s.TransactionCleared == 1,
                                                TransactionVoidBy = s.TransactionVoidFlag == 1,
                                            }).FirstOrDefault();
                                    if (adjustmentTransaction != null)
                                    {
                                        adjustmentTransaction.BankId = voidDetails.TransactionDetail.BankId;
                                        adjustmentTransaction.TransactionType = MoneyConstants.DEBIT;
                                        statusCode = ClearVoidTransactions(adjustmentTransaction);
                                    }
                                }
                                bool waveFlag = false;
                                if (statusCode > 0 && voidDetails.Flag == MoneyAccountTransactionFlagType.APPLYFEE &&
                                    voidDetails.CashDrawerTransactionId > 0)
                                {
                                    MoneyVoidTransactionFee cashDrawerFee = _context.AccountAoFee.Where(ao =>
                                        ao.AccountAoFeeId == voidDetails.CashDrawerTransactionId).Select(a =>
                                        new MoneyVoidTransactionFee
                                        {
                                            FundId = a.AccountAoFundId,
                                            InmateId = a.InmateId ?? 0,
                                            FeeId = a.AccountAoFeeId
                                        }).SingleOrDefault();
                                    if (cashDrawerFee != null)
                                    {
                                        cashDrawerFee.BankId = voidDetails.TransactionDetail.BankId;
                                        cashDrawerFee.TransactionType = MoneyConstants.CREDIT;
                                        cashDrawerFee.TransactionAmount = voidDetails.CashDrawerAmount;
                                        cashDrawerFee.TransactionDescription =
                                            voidDetails.CashDrawerTransactionDescription;
                                        cashDrawerFee.CurrentTransactionDescription =
                                            voidDetails.TransactionDetail.TransactionDescription;
                                        if (cashDrawerFee.FundId > 0)
                                        {
                                            AccountAoFund accountFund =
                                                _context.AccountAoFund.SingleOrDefault(af =>
                                                    af.AccountAoFundId == cashDrawerFee.FundId);
                                            if (accountFund != null)
                                                waveFlag = accountFund.FundAllowFeeWaiveDebt == 1;
                                        }

                                        if (!waveFlag)
                                        {
                                            statusCode = FeeTransaction(voidDetails, cashDrawerFee);
                                        }
                                    }
                                }
                            }
                            break;
                        }
                }
            }
            return statusCode;
        }
        // Inmate update balance
        private MoneyBalanceVm CashUpdateBalance(MoneyTransactionCalculateVm param)
        {
            MoneyBalanceVm balanceDetails = _moneyBalanceDebitCheck.MoneyTransactionCalculateBalance(param);
            balanceDetails.InmateId = param.InmateId;
            balanceDetails.FundId = param.FundId;
            balanceDetails.BankId = param.BankId;
            balanceDetails.Table = param.Table;
            balanceDetails.TransactionDate = param.TransactionDate;
            _moneyBalanceDebitCheck.MoneyUpdateBalance(balanceDetails);
            return balanceDetails;
        }
        // Updation for Clear flag
        private int ClearVoidTransactions(MoneyVoidTransactionFee fee)
        {
            //if (fee.TransactionVoidBy || fee.TransactionClearedBy)
            //{
            //    return 0;
            //}
            //else
            //{
                MoneyTransactionCalculateVm param = new MoneyTransactionCalculateVm
                {
                    BankId = fee.BankId,
                    FundId = fee.FundId,
                    InmateId = fee.InmateId,
                    TransactionAmount = fee.TransactionAmount,
                    TransactionType = MoneyTransactionType.DEBIT,
                    Table = MoneyTransactionTable.ACCOUNTAO_FEE
                };
                CashUpdateBalance(param);
                AccountAoFee accountAoFee = _context.AccountAoFee.Find(fee.FeeId);
                accountAoFee.TransactionClearedBy = _personnelId;
                accountAoFee.TransactionCleared = 1;
                accountAoFee.TransactionClearedDate = DateTime.Now;
                accountAoFee.TransactionClearedAmount = fee.TransactionAmount;
                accountAoFee.UpdatedBy = _personnelId;
                accountAoFee.UpdateDate = DateTime.Now;
                _context.SaveChanges();
                fee.StatusCode = 1;
           // }
            return fee.StatusCode;
        }
        // insert for Void Cash drawer
        private int CashDrawerVoidTransaction(SetVoidVm voidDetails)
        {
            int statusCode = 0;
            MoneyVoidTransactionFee cashDrawer = _context.AccountAoCashDrawer.Where(ao =>
                ao.AccountAoCashDrawerId == voidDetails.CashDrawerTransactionId).Select(a => new MoneyVoidTransactionFee
                {
                    AccountAoCashDrawerId=a.AccountAoCashDrawerId,
                    FundId = a.AccountAoTransaction.AccountAoFundId,
                    InmateId = a.InmateId ?? 0,
                    CashBalanceId = a.CashBalanceId ?? 0
                }).SingleOrDefault();
            if (cashDrawer != null)
            {
                cashDrawer.AccountAoCashDrawerId = cashDrawer.AccountAoCashDrawerId;
                cashDrawer.BankId = voidDetails.TransactionDetail.BankId;
                cashDrawer.CashId = voidDetails.TransactionDetail.ToTransactionId > 0
                    ? voidDetails.TransactionDetail.ToTransactionId
                    : voidDetails.TransactionDetail.CurrentTransactionId;
                cashDrawer.TransactionAmount = voidDetails.CashDrawerAmount;
                cashDrawer.TransactionType = voidDetails.CashDrawerTransactionType;
                cashDrawer.TransactionDescription = voidDetails.CashDrawerTransactionDescription;
                cashDrawer.CurrentTransactionDescription = voidDetails.TransactionDetail.TransactionDescription;
                cashDrawer.CurrentTransactionId = voidDetails.CashDrawerTransactionId;
                statusCode = CashDrawerVoid(cashDrawer);
            }
            return statusCode;
        }
        // insert for void Account_ledger transaction
        private int AccountAoTransactionInsert(MoneyVoidTransactionFee transaction, int voidTransactionId,
            MoneyBalanceVm param)
        {
            AccountAoTransaction trans = new AccountAoTransaction
            {
                InmateId = transaction.InmateId > 0 ? transaction.InmateId : default(int?),
                AccountAoFundId = transaction.FundId,
                AccountAoFeeId = transaction.FeeId > 0 ? transaction.FeeId : default(int?),
                AccountAoReceiveId = transaction.ReceiveId > 0 ? transaction.ReceiveId : default(int?),
                TransactionAmount = transaction.AbsTotalAmount,
                TransactionType = transaction.TransactionType,
                TransactionDescription = transaction.TransactionDescription,
                TransactionReceiveNumber = transaction.ReceiveNumber,
                TransactionReceipt = transaction.NextReceiptNum,
                TransactionReceiveFrom = transaction.ReceiveFrm,
                TransactionPayToTheOrderOf = transaction.PayToOrder,
                TransactionCheckNumber = transaction.CheckNo,
                TransactionDebitMemo = transaction.DebitMemo,
                CreatedBy = _personnelId,
                CreateDate = DateTime.Now,
                UpdatedBy = _personnelId,
                UpdateDate = DateTime.Now,
                TransactionDate = DateTime.Now,
                TransactionOfficerId = _personnelId,
                TransactionVoidBy = _personnelId,
                TransactionVoidFlag = 1,
                TransactionVoidDate = DateTime.Now,
                TransactionVoidTransactionId = voidTransactionId,
                BalanceAccount = param.BankBalance,
                BalanceInmate = param.InmateBalance,
                BalanceFund = param.FundBalance,
                AccountAoTransactionFromId = transaction.FromTransactionId > 0 ? transaction.FromTransactionId : default(int?),
                AccountAoTransactionToId = transaction.ToTransactionId > 0 ? transaction.ToTransactionId : default(int?),
            };
            _context.AccountAoTransaction.Add(trans);
            _context.SaveChanges();
            AccountAoTransaction accountTransaction = _context.AccountAoTransaction.Find(voidTransactionId);
            accountTransaction.TransactionVoidBy = _personnelId;
            accountTransaction.TransactionVoidFlag = 1;
            accountTransaction.TransactionVoidDate = DateTime.Now;
            accountTransaction.TransactionVoidTransactionId = trans.AccountAoTransactionId;
            _context.SaveChanges();
            return trans.AccountAoTransactionId;
        }
        private int AccountAoCashDrawerInsert(MoneyVoidTransactionFee cashDrawerVoidFee, decimal balanceCashDrawer)
        {
            AccountAoCashDrawer accountAoCashDrawer = new AccountAoCashDrawer
            {
                AccountAoTransactionId = cashDrawerVoidFee.CashId,
                BalanceCashDrawer = balanceCashDrawer,
                CreatedBy = _personnelId,
                CreateDate = DateTime.Now,
                UpdateDate = DateTime.Now,
                UpdatedBy = _personnelId,
                TransactionOfficerId = _personnelId,
                TransactionDate = DateTime.Now,
                TransactionAmount = cashDrawerVoidFee.AbsTotalAmount,
                TransactionDescription = cashDrawerVoidFee.TransactionDescription,
                TransactionType = cashDrawerVoidFee.TransactionType,
                CashBalanceId = cashDrawerVoidFee.CashBalanceId,
                TransactionVoidBy = _personnelId,
                TransactionVoidFlag = 1,
                TransactionVoidDate = DateTime.Now,
                TransactionVoidCashDrawerId = cashDrawerVoidFee.CurrentTransactionId,
                InmateId = cashDrawerVoidFee.InmateId>0? cashDrawerVoidFee.InmateId:default(int?)
            };
            _context.AccountAoCashDrawer.Add(accountAoCashDrawer);
            _context.SaveChanges();
            AccountAoCashDrawer updateAccountAoCashDrawer = _context.AccountAoCashDrawer.Find(cashDrawerVoidFee.AccountAoCashDrawerId);
            updateAccountAoCashDrawer.TransactionVoidFlag = 1;
            updateAccountAoCashDrawer.TransactionVoidDate = DateTime.Now;
            updateAccountAoCashDrawer.TransactionVoidBy = _personnelId;
            updateAccountAoCashDrawer.TransactionVoidCashDrawerId = accountAoCashDrawer.AccountAoCashDrawerId;
            _context.SaveChanges();
            return 1;
        }

        private int CashDrawerVoid(MoneyVoidTransactionFee cashDrawerVoidFee)
        {
            AccountAoCashBalance accountAoCashBalance = _context.AccountAoCashBalance.SingleOrDefault(acc =>
                acc.AccountAoCashBalanceId
                == cashDrawerVoidFee.CashBalanceId);
            if (accountAoCashBalance != null)
            {
                decimal balanceCashDrawer = accountAoCashBalance.CurrentBalance ?? 0;
                if (cashDrawerVoidFee.TransactionType == MoneyConstants.DEBIT)
                {
                    balanceCashDrawer = balanceCashDrawer - Math.Abs(cashDrawerVoidFee.TransactionAmount);
                    cashDrawerVoidFee.AbsTotalAmount = -1 * Math.Abs(cashDrawerVoidFee.TransactionAmount);
                }

                if (cashDrawerVoidFee.TransactionType == MoneyConstants.CREDIT)
                {
                    balanceCashDrawer = balanceCashDrawer + Math.Abs(cashDrawerVoidFee.TransactionAmount);
                    cashDrawerVoidFee.AbsTotalAmount = 1 * Math.Abs(cashDrawerVoidFee.TransactionAmount);
                }

                cashDrawerVoidFee.StatusCode = AccountAoCashDrawerInsert(cashDrawerVoidFee, balanceCashDrawer);
                if (cashDrawerVoidFee.TransactionDescription == MoneyConstants.VoidFundCashDrawerForDepository)
                {
                    cashDrawerVoidFee.TransactionType = MoneyConstants.CREDIT;
                    MoneyTransactionCalculateVm param = new MoneyTransactionCalculateVm
                    {
                        BankId = cashDrawerVoidFee.BankId,
                        FundId = cashDrawerVoidFee.FundId,
                        InmateId = cashDrawerVoidFee.InmateId,
                        TransactionAmount = cashDrawerVoidFee.TransactionAmount,
                        TransactionType = MoneyTransactionType.CREDIT,
                        Table = MoneyTransactionTable.ACCOUNTAO_TRANSACTION
                    };
                    MoneyBalanceVm balanceDetails = CashUpdateBalance(param);
                    cashDrawerVoidFee.AbsTotalAmount = Math.Abs(cashDrawerVoidFee.TransactionAmount);
                    int transId = AccountAoTransactionInsert(cashDrawerVoidFee, cashDrawerVoidFee.CashId,
                        balanceDetails);
                    cashDrawerVoidFee.StatusCode = transId > 0 ? 1 : 0;
                    AccountAoCashBalance cashBalance =
                        _context.AccountAoCashBalance.Find(cashDrawerVoidFee.CashBalanceId);
                    cashBalance.CurrentBalance = balanceCashDrawer;
                    _context.SaveChanges();
                }

            }

            return cashDrawerVoidFee.StatusCode;
        }
        //insert for void unpaid transaction
        private int AccountAoFeeInsert(MoneyVoidTransactionFee fee, MoneyBalanceVm param)
        {
            AccountAoFee accountAoFee = new AccountAoFee
            {
                InmateId = fee.InmateId > 0 ? fee.InmateId : default(int?),
                AccountAoFundId = fee.FundId,
                TransactionFee = fee.TransactionAmount,
                TransactionType = fee.TransactionType,
                TransactionDescription = fee.TransactionDescription,
                CreatedBy = _personnelId,
                CreateDate = fee.TransactionDate,
                UpdatedBy = _personnelId,
                UpdateDate = fee.TransactionDate,
                TransactionDate = fee.TransactionDate,
                TransactionOfficerId = _personnelId,
                BalanceAccountFee = param.BankBalance,
                BalanceInmateFee = param.InmateBalance,
                BalanceFundFee = param.FundBalance,
            };
            _context.AccountAoFee.Add(accountAoFee);
            _context.SaveChanges();
            return accountAoFee.AccountAoFeeId;
        }
        // void for Apply Fee
        private int FeeTransaction(SetVoidVm voidDetails, MoneyVoidTransactionFee fee)
        {

            AccountAoInmate newInmateId = _context.AccountAoInmate.SingleOrDefault(a => a.InmateId == fee.InmateId
                                                                                       && a.AccountAoBankId ==
                                                                                       fee.BankId);
            if (newInmateId == null && fee.InmateId > 0)
            {
                AccountAoInmate accountAoInmate = new AccountAoInmate
                {
                    AccountAoBankId = fee.BankId,
                    InmateId = fee.InmateId
                };
                _context.AccountAoInmate.Add(accountAoInmate);
                _context.SaveChanges();
            }
            MoneyTransactionCalculateVm param = new MoneyTransactionCalculateVm
            {
                BankId = fee.BankId,
                FundId = fee.FundId,
                InmateId = fee.InmateId,
                TransactionAmount = fee.TransactionAmount,
                TransactionType = MoneyTransactionType.CREDIT,
                Table = MoneyTransactionTable.ACCOUNTAO_FEE,
                TransactionDate = voidDetails.TransactionDetail.TransactionDate
            };
            MoneyBalanceVm balanceDetails = CashUpdateBalance(param);
            fee.TransactionDate = voidDetails.TransactionDetail.TransactionDate;
            fee.StatusCode = AccountAoFeeInsert(fee, balanceDetails) > 0 ? 1 : 0;
            return fee.StatusCode;
        }
        // void for Inout transaction
        private int SaveInOutVoidTransaction(MoneyVoidTransactionFee moneyVoid)
        {
            if (moneyVoid.InmateId == 0)
            {
                return 0;
            }
            string transactionType;
            if (moneyVoid.TransactionType == MoneyConstants.JOURNAL)
            {
                transactionType = moneyVoid.TransactionAmount > 0 ? MoneyConstants.CREDIT : MoneyConstants.DEBIT;
            }
            else
            {
                transactionType = moneyVoid.TransactionType;
            }
            moneyVoid.TransactionAmount = Math.Abs(moneyVoid.TransactionAmount);
            MoneyTransactionCalculateVm param = new MoneyTransactionCalculateVm
            {
                BankId = moneyVoid.BankId,
                FundId = moneyVoid.FundId,
                InmateId = moneyVoid.InmateId,
                TransactionAmount = moneyVoid.TransactionAmount,
                TransactionType = (transactionType == MoneyConstants.DEBIT)
                    ? MoneyTransactionType.DEBIT
                    : MoneyTransactionType.CREDIT,
                Table = MoneyTransactionTable.ACCOUNTAO_TRANSACTION
            };
            MoneyBalanceVm balanceDetails = CashUpdateBalance(param);
            moneyVoid.AbsTotalAmount = transactionType == MoneyConstants.DEBIT ? -1 * Math.Abs(moneyVoid.TransactionAmount)
                : Math.Abs(moneyVoid.TransactionAmount);
            AccountAoBank ban1 = _context.AccountAoBank.Find(moneyVoid.BankId);
            ban1.NextReceiptNum = ban1.NextReceiptNum + 1;
            _context.SaveChanges();
            int transId = AccountAoTransactionInsert(moneyVoid, moneyVoid.CurrentTransactionId, balanceDetails);
            moneyVoid.StatusCode = transId > 0 ? 1 : 0;
            return moneyVoid.StatusCode;
        }
        // AUTOPAYFEE
        private int SaveInOutFundVoidTransaction(SetVoidVm voidDetails, bool isFundCheck)
        {
            int statusCode = 0;
            MoneyVoidTransactionFee fee =
                _moneyVoidTransactionFee.SingleOrDefault(a => a.CurrentTransactionId == voidDetails.MoveTransactionId);
            if (fee != null)
            {
                fee.TransactionAmount = voidDetails.MoveToCashAmount;
                fee.TransactionType = voidDetails.MoveToCashTransactionType;
                fee.TransactionDescription = voidDetails.MoveToCashTransactionDescription;
                fee.CurrentTransactionDescription = voidDetails.TransactionDetail.TransactionDescription;
                fee.BankId = voidDetails.TransactionDetail.BankId;
                fee.CurrentTransactionId = voidDetails.MoveTransactionId;
                fee.CashId = voidDetails.TransactionDetail.ToTransactionId > 0
                    ? voidDetails.TransactionDetail.ToTransactionId
                    : voidDetails.TransactionDetail.CurrentTransactionId;
                if (voidDetails.MoveToCashTransactionType == MoneyConstants.DEBIT)
                {
                    int num = _context.AccountAoBank.Single(b => b.AccountAoBankId == fee.BankId).NextReceiptNum ?? 0;

                    fee.NextReceiptNum = voidDetails.ReceiptNumber + num;
                    fee.NextReceiptNum = fee.NextReceiptNum
                        .Substring(fee.NextReceiptNum.Length - 8);
                }
                return isFundCheck ? SaveFundVoidTransaction(fee) : SaveInOutVoidTransaction(fee);
            }
            return statusCode;
        }
        //Inout trans
        private int SaveFundVoidTransaction(MoneyVoidTransactionFee fee)
        {
            fee.TransactionAmount = Math.Abs(fee.TransactionAmount);
            MoneyTransactionCalculateVm param = new MoneyTransactionCalculateVm
            {
                BankId = fee.BankId,
                FundId = fee.FundId,
                InmateId = fee.InmateId,
                TransactionAmount = fee.TransactionAmount,
                TransactionType = (fee.TransactionType == MoneyConstants.DEBIT)
                    ? MoneyTransactionType.DEBIT
                    : MoneyTransactionType.CREDIT,
                Table = MoneyTransactionTable.ACCOUNTAO_TRANSACTION
            };
            MoneyBalanceVm balanceDetails = CashUpdateBalance(param);
            fee.AbsTotalAmount = fee.TransactionType == MoneyConstants.DEBIT ? -1 * Math.Abs(fee.TransactionAmount)
                : Math.Abs(fee.TransactionAmount);
            AccountAoBank ban1 = _context.AccountAoBank.Find(fee.BankId);
            ban1.NextReceiptNum = ban1.NextReceiptNum + 1;
            _context.SaveChanges();
            int transId = AccountAoTransactionInsert(fee, fee.CurrentTransactionId, balanceDetails);
              fee.StatusCode= transId > 0 ? 1 : 0;
           
            if (fee.CurrentTransactionDescription == MoneyConstants.FundCashDrawerForDepository
                || fee.CurrentTransactionDescription == MoneyConstants.TransferCashDrawerToBank
                || fee.CurrentTransactionDescription == MoneyConstants.TransferBankToCashDrawer)
            {
                AccountAoCashDrawer cashDrawer = _context.AccountAoCashDrawer
                    .FirstOrDefault(a => a.AccountAoTransactionId == fee.CurrentTransactionId);
                if (cashDrawer != null)
                {
                    fee.CashId = transId;
                    fee.CashBalanceId = cashDrawer.CashBalanceId??0;
                    fee.AccountAoCashDrawerId = cashDrawer.AccountAoCashDrawerId;
                    AccountAoCashBalance accountAoCashBalance = _context.AccountAoCashBalance.SingleOrDefault(acc =>
                        acc.AccountAoCashBalanceId
                        == cashDrawer.CashBalanceId);
                    if (accountAoCashBalance != null)
                    {
                        decimal balanceCashDrawer = accountAoCashBalance.CurrentBalance ?? 0;
                        if (fee.CurrentTransactionDescription != MoneyConstants.TransferCashDrawerToBank)
                        {
                            balanceCashDrawer = balanceCashDrawer - Math.Abs(fee.TransactionAmount);
                        }
                        else
                        {
                            balanceCashDrawer = balanceCashDrawer + Math.Abs(fee.TransactionAmount);
                        }

                        if (fee.CurrentTransactionDescription == MoneyConstants.TransferBankToCashDrawer)
                        {
                            fee.TransactionType = MoneyConstants.DEBIT;
                            fee.AbsTotalAmount = -1 * Math.Abs(fee.TransactionAmount);
                        }

                        if (fee.CurrentTransactionDescription == MoneyConstants.TransferCashDrawerToBank)
                        {
                            fee.TransactionType = MoneyConstants.CREDIT;
                            fee.AbsTotalAmount = 1 * Math.Abs(fee.TransactionAmount);
                        }
                        fee.StatusCode = AccountAoCashDrawerInsert(fee, balanceCashDrawer);
                        AccountAoCashBalance cashBalance = _context.AccountAoCashBalance.Find(cashDrawer.CashBalanceId);
                        cashBalance.CurrentBalance = balanceCashDrawer;
                        _context.SaveChanges();
                    }
                }
            }

            return fee.StatusCode;
        }
        // common for Move Trans && Return cash void && Apply
        private int SaveReturnCashMoveVoidTransaction(MoneyVoidTransactionFee returnTransactionDetail,
            SetVoidVm voidDetails)
        {
            int statusCode = 0;
            if (returnTransactionDetail != null)
            {
                returnTransactionDetail.ReturnCashFlag = voidDetails.Flag != MoneyAccountTransactionFlagType.MOVETRANS && returnTransactionDetail.ReturnCashFlag;
                returnTransactionDetail.TransactionAmount = voidDetails.ReturnAmount;
                returnTransactionDetail.TransactionType = voidDetails.ReturnTransactionType;
                returnTransactionDetail.TransactionDescription = voidDetails.ReturnTransactionDescription;
                returnTransactionDetail.CurrentTransactionDescription = voidDetails.TransactionDetail.TransactionDescription;
                returnTransactionDetail.BankId = voidDetails.TransactionDetail.BankId;
                AccountAoTransaction balanceCheck = _context.AccountAoTransaction.SingleOrDefault(at =>
                    at.AccountAoTransactionId == returnTransactionDetail.CurrentTransactionId
                    && at.BalanceInmate > 0);
                if (balanceCheck != null)
                {
                    AccountAoInmate inmateBalance = _context.AccountAoInmate.SingleOrDefault(at =>
                        at.InmateId == balanceCheck.InmateId && at.AccountAoBankId == returnTransactionDetail.BankId);
                    if (returnTransactionDetail.TransactionType == MoneyConstants.DEBIT && inmateBalance != null
                    && inmateBalance.BalanceInmate < returnTransactionDetail.TransactionAmount)
                    {
                        return 0;
                    }
                }
                returnTransactionDetail.TransactionAmount = Math.Abs(returnTransactionDetail.TransactionAmount);
                MoneyTransactionCalculateVm param = new MoneyTransactionCalculateVm
                {
                    BankId = returnTransactionDetail.BankId,
                    FundId = returnTransactionDetail.FundId,
                    InmateId = balanceCheck != null ? balanceCheck.InmateId ?? 0 : 0,
                    TransactionAmount = returnTransactionDetail.TransactionAmount,
                    TransactionType = (returnTransactionDetail.TransactionType == MoneyConstants.DEBIT)
                        ? MoneyTransactionType.DEBIT
                        : MoneyTransactionType.CREDIT,
                    Table = MoneyTransactionTable.ACCOUNTAO_TRANSACTION
                };
                MoneyBalanceVm balanceDetails = CashUpdateBalance(param);
                returnTransactionDetail.AbsTotalAmount = returnTransactionDetail.TransactionType == MoneyConstants.DEBIT ?
                    -1 * Math.Abs(returnTransactionDetail.TransactionAmount) : Math.Abs(returnTransactionDetail.TransactionAmount);
                AccountAoTransactionInsert(returnTransactionDetail, returnTransactionDetail.CurrentTransactionId, balanceDetails);
                statusCode = 1;
            }
            MoneyVoidTransactionFee moneyVoid =
                _moneyVoidTransactionFee.SingleOrDefault(a => a.CurrentTransactionId == voidDetails.MoveTransactionId);
            //bottom grid
            if (moneyVoid != null)
            {
                moneyVoid.TransactionAmount = voidDetails.MoveToCashAmount;
                moneyVoid.TransactionType = voidDetails.MoveToCashTransactionType;
                moneyVoid.TransactionDescription = voidDetails.MoveToCashTransactionDescription;
                moneyVoid.CurrentTransactionDescription = voidDetails.TransactionDetail.TransactionDescription;
                moneyVoid.BankId = voidDetails.TransactionDetail.BankId;
                moneyVoid.CurrentTransactionId = voidDetails.MoveTransactionId;
                AccountAoTransaction balanceCheck = _context.AccountAoTransaction.SingleOrDefault(at =>
                    at.AccountAoTransactionId == moneyVoid.CurrentTransactionId
                    && at.BalanceInmate > 0);
                if (balanceCheck != null)
                {
                    AccountAoInmate inmateBalance = _context.AccountAoInmate.SingleOrDefault(at =>
                        at.InmateId == balanceCheck.InmateId && at.AccountAoBankId == moneyVoid.BankId);
                    if (moneyVoid.TransactionType == MoneyConstants.DEBIT && inmateBalance != null
                    && inmateBalance.BalanceInmate < moneyVoid.TransactionAmount)
                    {
                        return 0;
                    }
                }
                moneyVoid.TransactionAmount = Math.Abs(moneyVoid.TransactionAmount);
                MoneyTransactionCalculateVm param = new MoneyTransactionCalculateVm
                {
                    BankId = moneyVoid.BankId,
                    FundId = moneyVoid.FundId,
                    InmateId = balanceCheck != null ? balanceCheck.InmateId ?? 0 : 0,
                    TransactionAmount = moneyVoid.TransactionAmount,
                    TransactionType = (moneyVoid.TransactionType == MoneyConstants.DEBIT)
                        ? MoneyTransactionType.DEBIT
                        : MoneyTransactionType.CREDIT,
                    Table = MoneyTransactionTable.ACCOUNTAO_TRANSACTION
                };
                MoneyBalanceVm balanceDetails = CashUpdateBalance(param);
                moneyVoid.AbsTotalAmount = moneyVoid.TransactionType == MoneyConstants.DEBIT ? -1 * Math.Abs(moneyVoid.TransactionAmount)
                    : Math.Abs(moneyVoid.TransactionAmount);
                AccountAoTransactionInsert(moneyVoid, moneyVoid.CurrentTransactionId, balanceDetails);
                statusCode = 1;
            }
            return statusCode;
        }
        private int UnpaidDoVoid(MoneyVoidTransactionFee fee, MoneyBalanceVm balanceDetails)
        {
            int feeId = AccountAoFeeInsert(fee, balanceDetails);
            AccountAoFee newTransaction = _context.AccountAoFee.Find(feeId);
            newTransaction.TransactionVoidFlag = 1;
            newTransaction.TransactionVoidDate = DateTime.Now;
            newTransaction.TransactionVoidBy = _personnelId;
            newTransaction.TransactionVoidFeeId = fee.CurrentTransactionId;
            AccountAoFee currentTransaction = _context.AccountAoFee.Find(fee.CurrentTransactionId);
            currentTransaction.TransactionVoidFlag = 1;
            currentTransaction.TransactionVoidDate = DateTime.Now;
            currentTransaction.TransactionVoidBy = _personnelId;
            currentTransaction.TransactionVoidFeeId = feeId;
            return _context.SaveChanges();
        }
        //insert for void pending transaction
        private int PendingReceiveDoVoid(MoneyVoidTransactionFee receive, MoneyBalanceVm balanceDetails)
        {
            AccountAoBank ban1 = _context.AccountAoBank.Find(receive.BankId);
            ban1.NextReceiptNum = ban1.NextReceiptNum + 1;
            _context.SaveChanges();
            AccountAoReceive accountAoReceive = new AccountAoReceive
            {
                AccountAoFundId = balanceDetails.FundId,
                InmateId = balanceDetails.InmateId,
                TransactionType = receive.TransactionType,
                TransactionReceiveCashFlag = receive.ReturnCashFlag ? 1 : 0,
                TransactionDescription = receive.TransactionDescription,
                AccountAoDepositoryId = receive.AccountAoDepositoryId,
                TransactionAmount = receive.TransactionAmount,
                TransactionReceiveFrom = receive.ReceiveFrm,
                TransactionReceiveNumber = receive.ReceiveNumber,
                TransactionReceipt = receive.NextReceiptNum,
                TransactionDate = DateTime.Now,
                TransactionOfficerId = _personnelId,
                BalanceInmatePending = balanceDetails.InmateBalance,
                BalanceFundPending = balanceDetails.FundBalance,
                BalanceAccountPending = balanceDetails.BankBalance,
                CreateDate = DateTime.Now,
                CreatedBy = _personnelId,
                UpdateDate = DateTime.Now,
                UpdatedBy = _personnelId,
                TransactionVoidReceiveId = receive.CurrentTransactionId,
                TransactionVoidFlag = 1,
                TransactionVoidDate = DateTime.Now,
                TransactionVoidBy = _personnelId,
            };
            _context.AccountAoReceive.Add(accountAoReceive);
            _context.SaveChanges();
            AccountAoReceive currentTransaction = _context.AccountAoReceive.Find(receive.CurrentTransactionId);
            currentTransaction.TransactionVoidFlag = 1;
            currentTransaction.TransactionVoidBy = _personnelId;
            currentTransaction.TransactionVoidDate = DateTime.Now;
            currentTransaction.TransactionVoidReceiveId = accountAoReceive.AccountAoReceiveId;
            return _context.SaveChanges();
        }
        //pending void details
        private void PendingAccountTransaction(int transId)
        {
            _moneyTransaction = _context.AccountAoReceive
                .Select(s => new MoneyAccountTransactionVm
                {
                    AccountAoReceiveId = s.AccountAoReceiveId,
                    TransactionDate = s.TransactionDate,
                    TransactionAmount = s.TransactionAmount ?? 0,
                    TransactionDescription = s.TransactionDescription.ToUpper(),
                    TransactionReceipt = s.TransactionReceipt,
                    InmateId = s.InmateId ?? 0,
                    VoidFlag = s.TransactionVoidFlag == 1,
                    ReceiveCashFlag = s.TransactionReceiveCashFlag == 1,
                    TransactionType = s.TransactionType,
                    ReceiveNumber = s.TransactionReceiveNumber,
                    TransactionId = s.AccountAoReceiveId,
                    Flag = MoneyAccountTransactionFlagType.PENDING,
                    ClearFlag = s.TransactionVerified == 1,
                    TransactionVoidId = s.TransactionVoidReceiveId ?? 0,
                    Officer = new PersonnelVm
                    {
                        PersonFirstName = s.TransactionOfficer.PersonNavigation.PersonFirstName,
                        PersonLastName = s.TransactionOfficer.PersonNavigation.PersonLastName,
                        OfficerBadgeNumber = s.TransactionOfficer.OfficerBadgeNumber
                    }
                }).OrderByDescending(s => s.AccountAoReceiveId);
            _moneyAccountTransaction =
                _moneyTransaction.Single(t => t.AccountAoReceiveId == transId);
        }
        //unpaid void details
        private void UnpaidTransaction(int transId)
        {
            _moneyTransaction = _context.AccountAoFee.Select(s =>
                new MoneyAccountTransactionVm
                {
                    AccountAoFeeId = s.AccountAoFeeId,
                    TransactionDate = s.TransactionDate,
                    TransactionAmount = (!s.TransactionCleared.HasValue || s.TransactionCleared == 0)
                        ? s.TransactionFee ?? 0
                        : s.TransactionClearedAmount ?? 0,
                    TransactionDescription = s.TransactionDescription.ToUpper(),
                    InmateId = s.InmateId ?? 0,
                    VoidFlag = s.TransactionVoidFlag == 1,
                    TransactionType = s.TransactionType,
                    ClearFlag = s.TransactionCleared == 1,
                    TransactionId = s.AccountAoFeeId,
                    Flag = MoneyAccountTransactionFlagType.UNPAID,
                    TransactionVoidId = s.TransactionVoidFeeId ?? 0,
                    AdjustmentAccountAoFeeId = s.AdjustmentAccountAoFeeId,
                    Officer = new PersonnelVm
                    {
                        PersonFirstName = s.TransactionOfficer.PersonNavigation.PersonFirstName,
                        PersonLastName = s.TransactionOfficer.PersonNavigation.PersonLastName,
                        OfficerBadgeNumber = s.TransactionOfficer.OfficerBadgeNumber
                    }
                }).OrderByDescending(o => o.TransactionId);
            _moneyAccountTransaction = _moneyTransaction
                .FirstOrDefault(x => x.AccountAoFeeId == transId ||
                                     (x.AdjustmentAccountAoFeeId == transId && !x.ClearFlag && !x.VoidFlag));
        }

        //get account ledger details
        private void AccountTransactionLedger(int transId)
        {
            _moneyTransaction = _context.AccountAoTransaction
                .Select(s => new MoneyAccountTransactionVm
                {
                    AccountAoReceiveId = s.AccountAoReceiveId ?? 0,
                    TransactionDate = s.TransactionDate,
                    TransactionAmount = s.TransactionAmount,
                    TransactionDescription = s.TransactionDescription.ToUpper(),
                    TransactionReceipt = s.TransactionReceipt,
                    InmateId = s.InmateId ?? 0,
                    TransactionId = s.AccountAoTransactionId,
                    ReturnCashFlag = s.TransactionReturnCashFlag == 1,
                    AccountAoFeeId = s.AccountAoFeeId ?? 0,
                    TransactionFromId = s.AccountAoTransactionFromId ?? 0,
                    TransactionToId = s.AccountAoTransactionToId ?? 0,
                    TransactionVoidId = s.TransactionVoidTransactionId ?? 0,
                    VoidFlag = s.TransactionVoidFlag == 1,
                    TransactionType = s.TransactionType,
                    Officer = new PersonnelVm
                    {
                        PersonFirstName = s.TransactionOfficer.PersonNavigation.PersonFirstName,
                        PersonLastName = s.TransactionOfficer.PersonNavigation.PersonLastName,
                        OfficerBadgeNumber = s.TransactionOfficer.OfficerBadgeNumber
                    }
                }).OrderByDescending(o => o.TransactionId);
            MoneyAccountTransactionVm moneyInoutTransaction =
                _moneyTransaction.SingleOrDefault(x =>
                    x.TransactionFromId == 0 && x.TransactionToId == 0 && x.TransactionId == transId);
            if (moneyInoutTransaction != null)
            {
                moneyInoutTransaction.Flag = MoneyAccountTransactionFlagType.INOUTTRANS;
                if (moneyInoutTransaction.ReturnCashFlag)
                {
                    moneyInoutTransaction.ReceiveNumber = MoneyConstants.CASH;
                }
                _moneyAccountTransaction = moneyInoutTransaction;
            }

            MoneyAccountTransactionVm moveTransactionVms = _moneyTransaction.SingleOrDefault(x =>
                x.TransactionId == transId && (x.TransactionFromId > 0 || x.TransactionToId > 0)
                                     && (x.AccountAoFeeId == 0) && !x.ReturnCashFlag);
            if (moveTransactionVms != null)
            {
                moveTransactionVms.Flag = MoneyAccountTransactionFlagType.MOVETRANS;
                _moneyAccountTransaction = moveTransactionVms;
            }

            MoneyAccountTransactionVm moneyReturnCashSummaryTransaction = _moneyTransaction.SingleOrDefault(x =>
                x.TransactionId == transId &&
                (x.TransactionFromId > 0 || x.TransactionToId > 0)
                && x.ReturnCashFlag
                && x.TransactionDate
                < (DateTime.Now.Date.AddDays(-7)));
            if (moneyReturnCashSummaryTransaction != null)
            {
                moneyReturnCashSummaryTransaction.Flag = MoneyAccountTransactionFlagType.RETURNCASHSUMMARY;
                if (moneyReturnCashSummaryTransaction.ReturnCashFlag)
                {
                    moneyReturnCashSummaryTransaction.ReceiveNumber = MoneyConstants.CASH;
                }
                _moneyAccountTransaction = moneyReturnCashSummaryTransaction;
            }

            MoneyAccountTransactionVm moneyReturnCashVoidTransaction = _moneyTransaction.SingleOrDefault(x =>
                x.TransactionId == transId &&
                (x.TransactionFromId > 0 || x.TransactionToId > 0)
                && x.ReturnCashFlag && x.TransactionDate > (DateTime.Now.Date.AddDays(-7)));
            if (moneyReturnCashVoidTransaction != null)
            {
                moneyReturnCashVoidTransaction.Flag = MoneyAccountTransactionFlagType.RETURNCASHVOID;
                if (moneyReturnCashVoidTransaction.ReturnCashFlag)
                {
                    moneyReturnCashVoidTransaction.ReceiveNumber = MoneyConstants.CASH;
                }
                _moneyAccountTransaction = moneyReturnCashVoidTransaction;
            }

            MoneyAccountTransactionVm moneyApplyFeeVoidTransaction = _moneyTransaction.SingleOrDefault(x =>
                x.TransactionId == transId && (x.TransactionFromId > 0 || x.TransactionToId > 0)
                                     && x.AccountAoFeeId > 0);
            if (moneyApplyFeeVoidTransaction != null)
            {
                moneyApplyFeeVoidTransaction.Flag = MoneyAccountTransactionFlagType.APPLYFEE;
                if (moneyApplyFeeVoidTransaction.ReturnCashFlag)
                {
                    moneyApplyFeeVoidTransaction.ReceiveNumber = MoneyConstants.CASH;
                }
                _moneyAccountTransaction = moneyApplyFeeVoidTransaction;
            }

            if (_moneyAccountTransaction != null)
            {
                _moneyAccountVoidDetails.Flag = _moneyAccountTransaction.Flag;
                switch (_moneyAccountTransaction.Flag)
                {
                    case MoneyAccountTransactionFlagType.INOUTTRANS:
                        InoutTransactionDetails(transId);
                        break;
                    case MoneyAccountTransactionFlagType.RETURNCASHSUMMARY:
                        ReturnSummaryTransactionDetails();
                        break;
                    case MoneyAccountTransactionFlagType.MOVETRANS:
                    case MoneyAccountTransactionFlagType.RETURNCASHVOID:
                    case MoneyAccountTransactionFlagType.APPLYFEE:
                        MoveTransactionDetails();
                        break;
                }
            }
        }
        private List<MoneyAccountTransactionVm> CashDrawerTransactionDetails(int transId)
        {
            return _context.AccountAoCashDrawer.Where(x =>
                    x.AccountAoTransactionId == transId)
                .Select(s => new MoneyAccountTransactionVm
                {
                    AccountAoReceiveId = s.AccountAoTransaction.AccountAoReceiveId ?? 0,
                    TransactionDate = s.TransactionDate,
                    TransactionAmount = s.TransactionAmount,
                    TransactionDescription = s.TransactionDescription,
                    VoidFlag = s.TransactionVoidFlag == 1,
                    InmateId = s.InmateId ?? 0,
                    FacilityAbbr = s.AccountAoCashBalance.Facility.FacilityAbbr,
                    AccountAoTransactionId = s.AccountAoTransactionId ?? 0,
                    TransactionId = s.AccountAoCashDrawerId,
                    TransactionType = s.TransactionType,
                    Officer = new PersonnelVm
                    {
                        PersonFirstName = s.TransactionOfficer.PersonNavigation.PersonFirstName,
                        PersonLastName = s.TransactionOfficer.PersonNavigation.PersonLastName,
                        OfficerBadgeNumber = s.TransactionOfficer.OfficerBadgeNumber
                    }
                }).ToList();
        }

        private void ReturnSummaryTransactionDetails()
        {
            if (_moneyAccountTransaction.TransactionFromId > 0)
            {
                _moneyAccountVoidDetails.ReturnCashList.AddRange(_moneyTransaction.Where(x =>
                    x.TransactionId == _moneyAccountTransaction.TransactionFromId));
                _moneyAccountVoidDetails.MoveToCashList.AddRange(_moneyTransaction.Where(x =>
                    x.TransactionId == _moneyAccountTransaction.TransactionId));
            }

            if (_moneyAccountTransaction.TransactionToId > 0)
            {
                _moneyAccountVoidDetails.MoveToCashList.AddRange(_moneyTransaction.Where(x =>
                    x.TransactionId == _moneyAccountTransaction.TransactionToId));
                _moneyAccountVoidDetails.ReturnCashList.AddRange(_moneyTransaction.Where(x =>
                    x.TransactionId == _moneyAccountTransaction.TransactionId));
            }

            if (_moneyAccountTransaction.Flag == MoneyAccountTransactionFlagType.RETURNCASHSUMMARY)
            {
                int cashId = _moneyAccountTransaction.TransactionFromId > 0 ? _moneyAccountTransaction.TransactionFromId
                    : _moneyAccountTransaction.TransactionToId;
                _moneyAccountVoidDetails.CashDrawerList = CashDrawerTransactionDetails(cashId);
            }
        }
        private void InoutTransactionDetails(int transId)
        {
            if (_moneyAccountTransaction.VoidFlag)
            {
                MoneyAccountTransactionVm childInOutTrans =
                    _moneyTransaction.SingleOrDefault(t => t.TransactionId == transId);
                MoneyAccountTransactionVm childVoidTrans =
                    _moneyTransaction.SingleOrDefault(t => t.TransactionVoidId == transId);
                if (childInOutTrans != null && childVoidTrans != null)
                {
                    if (childInOutTrans.TransactionId < childVoidTrans.TransactionId)
                    {
                        _moneyAccountVoidDetails.ChildTransactionList.Add(childInOutTrans);
                        _moneyAccountVoidDetails.VoidChildTransactionList.Add(childVoidTrans);
                    }
                    else
                    {
                        _moneyAccountVoidDetails.ChildTransactionList.Add(childVoidTrans);
                        _moneyAccountVoidDetails.VoidChildTransactionList.Add(childInOutTrans);
                    }
                } else {
                          _moneyAccountVoidDetails.VoidChildTransactionList.Add(childInOutTrans);
                    }
            }
            else
            {
                if (_moneyAccountTransaction.TransactionDescription ==
                    MoneyConstants.FundCashDrawerForDepository
                    || _moneyAccountTransaction.TransactionDescription ==
                    MoneyConstants.TransferCashDrawerToBank ||
                    _moneyAccountTransaction.TransactionDescription == MoneyConstants.TransferBankToCashDrawer)
                {
                    _moneyAccountVoidDetails.CashDrawerList = CashDrawerTransactionDetails(transId);
                    _moneyAccountVoidDetails.ReturnCashList.Add(_moneyAccountTransaction);
                }
                else if (_moneyAccountTransaction.TransactionDescription == MoneyConstants.AUTOPAYFEE)
                {
                    _moneyAccountVoidDetails.MoveToCashList.AddRange(_moneyTransaction.Where(x =>
                        x.TransactionId != transId &&
                        (x.AccountAoFeeId == _moneyAccountTransaction.AccountAoFeeId)));
                    _moneyAccountVoidDetails.ReturnCashList.Add(_moneyAccountTransaction);
                }
                else
                {
                    _moneyAccountVoidDetails.ReturnCashList.Add(_moneyAccountTransaction);
                }
            }
        }
        private List<MoneyAccountTransactionVm> ApplyFeeTransactionDetails(int transId)
        {
            List<MoneyAccountTransactionVm> fundApply = _context.AccountAoFee
                .Where(x => x.AccountAoFeeId == transId || (x.AdjustmentAccountAoFeeId == transId &&
                                                            (!x.TransactionVoidFlag.HasValue || x.TransactionVoidFlag == 0)
                                                            && (!x.TransactionCleared.HasValue || x.TransactionCleared == 0))).Select(s =>
                    new MoneyAccountTransactionVm
                    {
                        AccountAoFeeId = s.AccountAoFeeId,
                        TransactionDate = s.TransactionDate,
                        TransactionAmount = s.TransactionFee ?? 0,
                        TransactionDescription = s.TransactionDescription,
                        InmateId = s.InmateId ?? 0,
                        FacilityAbbr = s.Inmate.HousingUnit.Facility.FacilityAbbr,
                        VoidFlag = s.TransactionVoidFlag == 1,
                        TransactionType = s.TransactionType,
                        TransactionId = s.AccountAoFeeId,
                        VoidBy = new PersonnelVm
                        {
                            PersonFirstName = s.TransactionVoidByNavigation.PersonNavigation.PersonFirstName,
                            PersonLastName = s.TransactionVoidByNavigation.PersonNavigation.PersonLastName,
                            OfficerBadgeNumber = s.TransactionVoidByNavigation.OfficerBadgeNumber
                        }
                    }).OrderByDescending(o => o.AccountAoFeeId).ToList();
            return fundApply;
        }
        //common for move,Returncash,applyfee
        private void MoveTransactionDetails()
        {
            if (!_moneyAccountTransaction.VoidFlag)
            {
                ReturnSummaryTransactionDetails();
                switch (_moneyAccountTransaction.Flag)
                {
                    case MoneyAccountTransactionFlagType.RETURNCASHVOID:
                        int cash = _moneyAccountTransaction.TransactionToId > 0 ? _moneyAccountTransaction.TransactionToId
                            : _moneyAccountTransaction.TransactionId;
                        _moneyAccountVoidDetails.CashDrawerList = CashDrawerTransactionDetails(cash);
                        break;
                    case MoneyAccountTransactionFlagType.MOVETRANS:
                        {
                            int cashId = _moneyAccountTransaction.TransactionFromId > 0 ? _moneyAccountTransaction.TransactionFromId
                                : _moneyAccountTransaction.TransactionId;
                            _moneyAccountVoidDetails.CashDrawerList = CashDrawerTransactionDetails(cashId);
                            break;
                        }
                    case MoneyAccountTransactionFlagType.APPLYFEE:
                        {
                            MoneyAccountTransactionVm fundApply =
                                ApplyFeeTransactionDetails(_moneyAccountTransaction.AccountAoFeeId).FirstOrDefault();
                            if (fundApply != null)
                            {
                                if (fundApply.FundId > 0)
                                {
                                    AccountAoFund accountFund = _context.AccountAoFund.SingleOrDefault(af =>
                                          af.AccountAoFundId == fundApply.FundId);
                                    if (accountFund != null)
                                        if (!accountFund.FundAllowFeeWaiveDebt.HasValue ||
                                            accountFund.FundAllowFeeWaiveDebt == 0)
                                            _moneyAccountVoidDetails.CashDrawerList.Add(fundApply);
                                }
                                else
                                {
                                    _moneyAccountVoidDetails.CashDrawerList.Add(fundApply);
                                }
                            }

                            break;
                        }
                }
            }
            else
            {
                MoneyAccountTransactionVm childVoidTransaction =
                    _moneyTransaction.SingleOrDefault(
                        t => t.TransactionVoidId == _moneyAccountTransaction.TransactionId);
                MoneyAccountTransactionVm childTransaction =
                    _moneyTransaction.SingleOrDefault(t =>
                        t.TransactionId == _moneyAccountTransaction.TransactionId);
                MoneyAccountTransactionVm childMoveFromTransaction = null;
                MoneyAccountTransactionVm childMoveVoidFromTransaction = null;
                MoneyAccountTransactionVm childMoveToTransaction = null;
                MoneyAccountTransactionVm childMoveVoidToTransaction = null;
                if (_moneyAccountTransaction.TransactionFromId > 0)
                {
                    childMoveVoidFromTransaction =
                        _moneyTransaction.FirstOrDefault(t =>
                            t.TransactionVoidId == _moneyAccountTransaction.TransactionFromId);
                    childMoveFromTransaction =
                        _moneyTransaction.FirstOrDefault(t =>
                            t.TransactionId == _moneyAccountTransaction.TransactionFromId);
                }

                if (_moneyAccountTransaction.TransactionToId > 0)
                {
                    childMoveVoidToTransaction =
                        _moneyTransaction.FirstOrDefault(t =>
                            t.TransactionVoidId == _moneyAccountTransaction.TransactionToId);
                    childMoveToTransaction =
                        _moneyTransaction.FirstOrDefault(t =>
                            t.TransactionId == _moneyAccountTransaction.TransactionToId);
                }

                if (_moneyAccountTransaction.Flag == MoneyAccountTransactionFlagType.MOVETRANS ||
                    _moneyAccountTransaction.Flag == MoneyAccountTransactionFlagType.RETURNCASHVOID)
                {
                    if (_moneyAccountTransaction.TransactionFromId > 0)
                    {
                        if (childMoveVoidFromTransaction != null && childMoveFromTransaction != null)
                        {
                            if (childMoveFromTransaction.TransactionId < childMoveVoidFromTransaction.TransactionId)
                            {
                                _moneyAccountVoidDetails.ChildTransactionList.Add(childMoveFromTransaction);
                                _moneyAccountVoidDetails.VoidChildTransactionList.Add(childMoveVoidFromTransaction);
                            }
                            else
                            {
                                _moneyAccountVoidDetails.ChildTransactionList.Add(childMoveVoidFromTransaction);
                                _moneyAccountVoidDetails.VoidChildTransactionList.Add(childMoveFromTransaction);
                            }
                        }

                        if (childVoidTransaction != null && childTransaction != null)
                        {
                            if (childTransaction.TransactionId < childVoidTransaction.TransactionId)
                            {
                                _moneyAccountVoidDetails.ChildMoveTransactionList.Add(childTransaction);
                                _moneyAccountVoidDetails.VoidMoveChildTransactionList.Add(childVoidTransaction);
                            }
                            else
                            {
                                _moneyAccountVoidDetails.ChildMoveTransactionList.Add(childVoidTransaction);
                                _moneyAccountVoidDetails.VoidMoveChildTransactionList.Add(childTransaction);
                            }
                        }
                    }

                    if (_moneyAccountTransaction.TransactionToId > 0)
                    {
                        if (childTransaction != null && childVoidTransaction != null)
                        {
                            if (childTransaction.TransactionId < childVoidTransaction.TransactionId)
                            {
                                _moneyAccountVoidDetails.ChildTransactionList.Add(childTransaction);
                                _moneyAccountVoidDetails.VoidChildTransactionList.Add(childVoidTransaction);
                            }
                            else
                            {
                                _moneyAccountVoidDetails.ChildTransactionList.Add(childVoidTransaction);
                                _moneyAccountVoidDetails.VoidChildTransactionList.Add(childTransaction);
                            }
                        }

                        if (childMoveToTransaction != null && childMoveVoidToTransaction != null)
                        {
                            if (childMoveToTransaction.TransactionId < childMoveVoidToTransaction.TransactionId)
                            {
                                _moneyAccountVoidDetails.ChildMoveTransactionList.Add(childMoveToTransaction);
                                _moneyAccountVoidDetails.VoidMoveChildTransactionList.Add(childMoveVoidToTransaction);
                            }
                            else
                            {
                                _moneyAccountVoidDetails.ChildMoveTransactionList.Add(childMoveVoidToTransaction);
                                _moneyAccountVoidDetails.VoidMoveChildTransactionList.Add(childMoveToTransaction);
                            }
                        }
                    }
                }

                switch (_moneyAccountTransaction.Flag)
                {
                    case MoneyAccountTransactionFlagType.RETURNCASHVOID:
                        int cash = _moneyAccountTransaction.TransactionToId > 0
                            ? _moneyAccountTransaction.TransactionToId
                            : _moneyAccountTransaction.TransactionId;
                        MoneyAccountTransactionVm cashDrawer = CashDrawerTransactionDetails(cash).FirstOrDefault();
                        if (cashDrawer != null)
                        {
                            cashDrawer.Flag = MoneyAccountTransactionFlagType.CASHDRAWER;
                            _moneyAccountVoidDetails.ApplyFeeList.Add(cashDrawer);
                            _moneyAccountVoidDetails.NewFeeAppliedList = CashDrawerVoidTransactionDetails(cashDrawer.TransactionId);
                        }
                        break;
                    case MoneyAccountTransactionFlagType.MOVETRANS:
                        {
                            int cashId = _moneyAccountTransaction.TransactionFromId > 0
                                ? _moneyAccountTransaction.TransactionFromId
                                : _moneyAccountTransaction.TransactionId;
                            MoneyAccountTransactionVm accountDrawer = CashDrawerTransactionDetails(cashId).FirstOrDefault();
                            if (accountDrawer != null)
                            {
                                accountDrawer.Flag = MoneyAccountTransactionFlagType.CASHDRAWER;
                                _moneyAccountVoidDetails.ApplyFeeList.Add(accountDrawer);
                                _moneyAccountVoidDetails.NewFeeAppliedList = CashDrawerVoidTransactionDetails(accountDrawer.TransactionId);
                            }
                            break;
                        }
                    case MoneyAccountTransactionFlagType.APPLYFEE:
                        {
                            DateTime? d1 = null;
                            if (_moneyAccountTransaction.TransactionFromId > 0)
                            {

                                if (childTransaction != null && childVoidTransaction != null)
                                {
                                    if (childTransaction.TransactionId < childVoidTransaction.TransactionId)
                                    {
                                        _moneyAccountVoidDetails.ChildTransactionList.Add(childTransaction);
                                        _moneyAccountVoidDetails.VoidChildTransactionList.Add(childVoidTransaction);
                                    }
                                    else
                                    {
                                        _moneyAccountVoidDetails.ChildTransactionList.Add(childVoidTransaction);
                                        _moneyAccountVoidDetails.VoidChildTransactionList.Add(childTransaction);
                                    }
                                }

                                if (childMoveFromTransaction != null && childMoveVoidFromTransaction != null)
                                {
                                    if (childMoveFromTransaction.TransactionId < childMoveVoidFromTransaction.TransactionId)
                                    {
                                        d1 = childMoveFromTransaction.TransactionDate;
                                        _moneyAccountVoidDetails.ChildMoveTransactionList.Add(childMoveFromTransaction);
                                        _moneyAccountVoidDetails.VoidMoveChildTransactionList.Add(childMoveVoidFromTransaction);
                                    }
                                    else
                                    {
                                        d1 = childMoveVoidFromTransaction.TransactionDate;
                                        _moneyAccountVoidDetails.ChildMoveTransactionList.Add(childMoveVoidFromTransaction);
                                        _moneyAccountVoidDetails.VoidMoveChildTransactionList.Add(childMoveFromTransaction);
                                    }
                                }

                            }

                            if (_moneyAccountTransaction.TransactionToId > 0)
                            {

                                if (childMoveToTransaction != null && childMoveVoidToTransaction != null)
                                {
                                    if (childMoveToTransaction.TransactionId < childMoveVoidToTransaction.TransactionId)
                                    {
                                        _moneyAccountVoidDetails.ChildTransactionList.Add(childMoveToTransaction);
                                        _moneyAccountVoidDetails.VoidChildTransactionList.Add(childMoveVoidToTransaction);
                                    }
                                    else
                                    {
                                        _moneyAccountVoidDetails.ChildTransactionList.Add(childMoveVoidToTransaction);
                                        _moneyAccountVoidDetails.VoidChildTransactionList.Add(childMoveToTransaction);
                                    }
                                }

                                MoneyAccountTransactionVm childMoveVoidTrans =
                                    _moneyTransaction.SingleOrDefault(
                                        t => t.TransactionVoidId == _moneyAccountTransaction.TransactionId);
                                MoneyAccountTransactionVm childMoveTrans =
                                    _moneyTransaction.SingleOrDefault(t =>
                                        t.TransactionId == _moneyAccountTransaction.TransactionId);
                                if (childMoveTrans != null && childMoveVoidTrans != null)
                                {
                                    if (childMoveTrans.TransactionId < childMoveVoidTrans.TransactionId)
                                    {
                                        d1 = childMoveTrans.TransactionDate;
                                        _moneyAccountVoidDetails.ChildMoveTransactionList.Add(childMoveTrans);
                                        _moneyAccountVoidDetails.VoidMoveChildTransactionList.Add(childMoveVoidTrans);
                                    }
                                    else
                                    {
                                        d1 = childMoveVoidTrans.TransactionDate;
                                        _moneyAccountVoidDetails.ChildMoveTransactionList.Add(childMoveVoidTrans);
                                        _moneyAccountVoidDetails.VoidMoveChildTransactionList.Add(childMoveTrans);
                                    }
                                }
                            }
                            List<MoneyAccountTransactionVm> fundApply = ApplyFeeTransactionDetails(_moneyAccountTransaction.AccountAoFeeId);
                            if (fundApply.Count > 0)
                            {
                                AccountAoFee fee = d1 != null
                                    ? _context.AccountAoFee.FirstOrDefault(ao => ao.TransactionDate == d1)
                                    : null;
                                if (fee != null)
                                {
                                    List<MoneyAccountTransactionVm> feeIdLst = ApplyFeeTransactionDetails(fee.AccountAoFeeId);
                                    if (feeIdLst.Any())
                                    {
                                        _moneyAccountVoidDetails.NewFeeAppliedList.AddRange(feeIdLst);
                                    }
                                }

                                _moneyAccountVoidDetails.ApplyFeeList.AddRange(fundApply);
                            }

                            break;
                        }
                }
            }
        }
        //get cashdrawer void
        private List<MoneyAccountTransactionVm> CashDrawerVoidTransactionDetails(int transId)
        {
            return _context.AccountAoCashDrawer.Where(x =>
                                 x.TransactionVoidCashDrawerId == transId)
                             .Select(s => new MoneyAccountTransactionVm
                             {
                                 AccountAoReceiveId = s.AccountAoTransaction.AccountAoReceiveId ?? 0,
                                 TransactionDate = s.TransactionDate,
                                 TransactionAmount = s.TransactionAmount,
                                 TransactionDescription = s.TransactionDescription,
                                 VoidFlag = s.TransactionVoidFlag == 1,
                                 InmateId = s.InmateId ?? 0,
                                 FacilityAbbr = s.AccountAoCashBalance.Facility.FacilityAbbr,
                                 AccountAoTransactionId = s.AccountAoTransactionId ?? 0,
                                 TransactionId = s.AccountAoCashDrawerId,
                                 TransactionType = s.TransactionType,
                                 VoidDate = s.TransactionVoidDate,
                                 BalanceInmate = s.BalanceCashDrawer,
                                 Officer = new PersonnelVm
                                 {
                                     PersonFirstName = s.TransactionOfficer.PersonNavigation.PersonFirstName,
                                     PersonLastName = s.TransactionOfficer.PersonNavigation.PersonLastName,
                                     OfficerBadgeNumber = s.TransactionOfficer.OfficerBadgeNumber
                                 }
                             }).ToList();
        }
    }
}