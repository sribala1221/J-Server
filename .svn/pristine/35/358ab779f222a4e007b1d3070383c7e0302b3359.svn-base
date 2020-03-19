
using GenerateTables.Models;
using ServerAPI.ViewModels;
using System.Collections.Generic;
using System.Linq;
// ReSharper disable once CheckNamespace
namespace ServerAPI.Services
{
    public class MoneyDepositoryAdjustService : IMoneyDepositoryAdjustService
    {
        private readonly AAtims _context;

        public MoneyDepositoryAdjustService(AAtims context)
        {
            _context = context;
        }
        /// <summary>
        /// transaction officer get the transaction details 
        /// </summary>
        /// <param name="adjustTransaction"></param>
        /// <returns></returns>
        public List<MoneyAccountTransactionVm> MoneyAdjustTransaction(
            MoneyAdjustInputVm adjustTransaction)
        {
            List<MoneyAccountTransactionVm> transaction = _context.AccountAoTransaction.Where(at =>
                at.AccountAoReceive.AdjustmentAccountAoReceiveId > 0
                && at.AccountAoFund.AccountAoBankId == adjustTransaction.BankId
                && (adjustTransaction.FromDate.Date <= at.TransactionDate.Value.Date &&
                    adjustTransaction.ToDate.Date >= at.TransactionDate.Value.Date) && at.InmateId>0
            ).Select(s => new MoneyAccountTransactionVm
            {
                BalanceFund = s.BalanceFund,
                TransactionCheckNumber = s.TransactionCheckNumber,
                TransactionReceiveCashFlag = s.TransactionReceiveCashFlag == 1,
                TransactionDate = s.TransactionDate,
                TransactionDescription = s.TransactionDescription,
                Person =new PersonVm
                {
                    PersonLastName = s.Inmate.Person.PersonLastName,
                    PersonFirstName = s.Inmate.Person.PersonFirstName,
                    PersonMiddleName = s.Inmate.Person.PersonMiddleName,
                    PersonSuffix = s.Inmate.Person.PersonSuffix,
                    PersonId = s.Inmate.PersonId,
                    FacilityId = s.Inmate.FacilityId,

                },
                HousingUnit =s.Inmate.HousingUnitId > 0 ? new HousingDetail
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
                TransactionId = s.AccountAoReceiveId ?? 0,
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
                DepositoryName = s.AccountAoReceive.AccountAoDepository.DepositoryName,
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
                },
                AdjustmentAccountAoReceiveId = s.AccountAoReceive.AdjustmentAccountAoReceiveId ?? 0,
                TransactionAmount = s.TransactionAmount,
            }).OrderByDescending(a => a.AccountAoTransactionId).ToList();
            List<MoneyAccountTransactionVm> receive = _context.AccountAoReceive
                .Where(ar => transaction.Any(t => t.AdjustmentAccountAoReceiveId == ar.AccountAoReceiveId)
                             && ar.TransactionVoidFlag == 1).Select(of => new MoneyAccountTransactionVm
                             {
                                 Officer = new PersonnelVm
                                 {
                                     PersonFirstName = of.TransactionOfficer.PersonNavigation.PersonFirstName,
                                     PersonLastName = of.TransactionOfficer.PersonNavigation.PersonLastName,
                                     OfficerBadgeNumber = of.TransactionOfficer.OfficerBadgeNumber
                                 },
                                 TransactionAmount = of.TransactionAmount ?? 0,
                                 AccountAoReceiveId = of.AccountAoReceiveId,
                                 TransactionOfficerId = of.TransactionOfficerId,
                                 TransactionReceipt=of.TransactionReceipt
                             }).ToList();
            transaction.ForEach(f =>
            {
                MoneyAccountTransactionVm adjustmentReceive =
                    receive.FirstOrDefault(w => w.AccountAoReceiveId == f.AdjustmentAccountAoReceiveId);
                if (adjustmentReceive == null) return;
                f.VerifiedAmount = adjustmentReceive.TransactionAmount;
                f.TransactionReceipt = adjustmentReceive.TransactionReceipt;
                f.TransactionOfficerId = adjustmentReceive.TransactionOfficerId;
                f.Officer = adjustmentReceive.Officer;
            });
            return transaction;
        }

    }
}