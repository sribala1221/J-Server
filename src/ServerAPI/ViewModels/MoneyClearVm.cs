using System;
using System.Collections.Generic;

namespace ServerAPI.ViewModels
{
    public class MoneyClearVm
    {
        public List<KeyValuePair<string,int>> LoadBankDetails { get; set; }
        public List<MoneyAccountAoFundVm> LoadFundDetails { get; set; }
    }
}
