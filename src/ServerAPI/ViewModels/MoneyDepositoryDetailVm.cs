using System;
using System.Collections.Generic;

namespace ServerAPI.ViewModels
{
    public class PersonnelCountVm
    {
        public int PersonnelId { get; set; }
        public string PersonnelFirstName { get; set; }
        public string PersonnelLastName { get; set; }
        public string OfficerBadgeNumber { get; set; }
        public int Count { get; set; }
    }

    public class MoneyDepositoryDetailVm
    {
        public List<MoneyAccountTransactionVm> MoneyDepositoryDetail { get; set; }
        public List<KeyValuePair<string, int>> BankDetails { get; set; }
        public List<MoneyAccountAoFundVm> FundDetails { get; set; }
        public List<KeyValuePair<string, int>> DepositoryDetails { get; set; }
        public List<MoneyCashBalanceVm> VaultDetails { get; set; }
        public List<PersonnelCountVm> PersonnelCountDetails { get; set; }
        public List<DateTime?> TrasnsactionDateList { get; set; }
        public List<MoneyDepositVm> MoneyDetails { get; set; }
    }
    public class MoneyAdjustCountVm
    {
        public PersonnelVm OriginalOfficer { get; set; }
        public int OriginalCount { get; set; }
        public int TransactionOfficerId { get; set; }
    }

    public class MoneyAdjustInputVm
    {
        public int BankId { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int TransactionOfficerId { get; set; }
    }

    public class MoneyDepositVm
    {
        public int AccountTransactionFromId { get; set; }
        public decimal BankDeposit { get; set; }
        public decimal VaultDeposit { get; set; }
    }

}
