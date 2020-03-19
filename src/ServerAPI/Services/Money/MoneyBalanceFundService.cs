using System;
using System.Collections.Generic;
using System.Linq;
using GenerateTables.Models;
using ServerAPI.ViewModels;

namespace ServerAPI.Services {
    public class MoneyBalanceFundService : IMoneyBalanceFundService {

        private readonly AAtims _context;

        public MoneyBalanceFundService (AAtims context) {
            _context = context;
        }

        #region For Money Balance Account
        public List<MoneyAccountVm> GetMoneyAccount () {
            List<MoneyAccountVm> lstAccountDetails = _context.AccountAoBank.Where (x => x.Inactive == 0)
                .Select (s => new MoneyAccountVm {
                    BankAccountAbbr = s.BankAccountAbbr,
                        BalanceAccount = s.BalanceAccount,
                        BalanceAccountFee = s.BalanceAccountFee,
                        BalanceAccountPending = s.BalanceAccountPending,
                        AccountAoBankId = s.AccountAoBankId,
                        Inactive = s.Inactive
                }).OrderByDescending (o => o.BankAccountAbbr).ToList ();
            return lstAccountDetails;
        }
        #endregion
    }
}