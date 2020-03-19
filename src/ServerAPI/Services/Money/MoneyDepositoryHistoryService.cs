using GenerateTables.Models;
using Microsoft.AspNetCore.Http;
using ServerAPI.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ServerAPI.Services
{
    public class MoneyDepositoryHistoryService : IMoneyDepositoryHistoryService
    {
        private readonly AAtims _context;
        private readonly IMoneyService _moneyService;

        public MoneyDepositoryHistoryService(AAtims context, IHttpContextAccessor httpContextAccessor,
            IMoneyService moneyService)
        {
            _context = context;
            _moneyService = moneyService;
        }

        public MoneyDepositoryDetailVm GetMoneyDepositoryHistory()
        {
            MoneyDepositoryDetailVm moneyDepositoryDetail = new MoneyDepositoryDetailVm()
            {
                BankDetails = _moneyService.MoneyGetBank(),
                FundDetails = _moneyService.MoneyGetFund(),
                DepositoryDetails = _moneyService.MoneyGetDeposit()
            };

            moneyDepositoryDetail.TrasnsactionDateList = _context.AccountAoTransaction.Where(w => w.AccountAoReceiveId > 0
            && w.TransactionDate.Value.Year == DateTime.Now.Year)
                .Select(s => s.TransactionDate).OrderByDescending(o => o).Distinct().ToList();

            return moneyDepositoryDetail;
        }

        public MoneyDepositoryDetailVm GetMoneyDepositoryHistoryDetails(int bankId,
            int accountAoDepositoryId, int fundId, int personnelId, DateTime? transactionDate)
        {
            MoneyDepositoryDetailVm moneyDepositoryVerifyDetail = new MoneyDepositoryDetailVm();
            {
                moneyDepositoryVerifyDetail.MoneyDepositoryDetail = _context.AccountAoReceive
                .Where(w => w.AccountAoFund.AccountAoBankId == bankId &&
                           w.AccountAoDepositoryId == accountAoDepositoryId
                           && w.TransactionVerified == 1 &&
                            (fundId == 0 || w.AccountAoFundId == fundId) &&
                            (personnelId == 0 || w.TransactionOfficerId == personnelId)
                            && ((w.TransactionVerifiedDate.Value.Date == transactionDate.Value.Date
                            && w.TransactionVerifiedDate.Value.Hour == transactionDate.Value.Hour
                            && w.TransactionVerifiedDate.Value.Minute == transactionDate.Value.Minute && w.TransactionVerifiedDate.Value.Second == transactionDate.Value.Second)
                            || (w.TransactionVoidDate.Value.Date == transactionDate.Value.Date
                            && w.TransactionVoidDate.Value.Hour == transactionDate.Value.Hour
                            && w.TransactionVoidDate.Value.Minute == transactionDate.Value.Minute && w.TransactionVoidDate.Value.Second == transactionDate.Value.Second))
                            //&& (w.TransactionVerifiedDate == transactionDate || w.TransactionVoidDate == transactionDate)
                            )
                            .OrderByDescending(o => o.AccountAoReceiveId)
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
                       AdjustmentAccountAoReceiveId = s.AdjustmentAccountAoReceiveId ?? 0,
                       VerifiedAmount = s.AdjustmentAccountAoReceiveId > 0 ?
                       _context.AccountAoReceive.Where(f => f.AccountAoReceiveId == s.AdjustmentAccountAoReceiveId)
                       .Select(se => se.TransactionAmount ?? 0).FirstOrDefault() : 0,
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
                   }).ToList();

                moneyDepositoryVerifyDetail.MoneyDetails = _context.AccountAoTransaction
                .Where(at => moneyDepositoryVerifyDetail.MoneyDepositoryDetail.Any(t => t.AccountAoReceiveId == at.AccountAoReceiveId))
                .Select(s => new MoneyDepositVm
                {

                    AccountTransactionFromId = s.AccountAoTransactionFromId ?? 0,
                    BankDeposit = s.AccountAoTransactionFromId > 0 ? 0 : s.TransactionAmount,
                    VaultDeposit = s.AccountAoTransactionFromId > 0 ? _context.AccountAoCashDrawer
                               .Where(x => x.AccountAoTransactionId == s.AccountAoTransactionFromId)
                               .Select(f => f.TransactionAmount).SingleOrDefault() : 0
                }).ToList();           

            return moneyDepositoryVerifyDetail;
            }
        }
    }
}
