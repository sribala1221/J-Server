using System;
using System.Collections.Generic;

namespace ServerAPI.ViewModels
{
    public class IntakeCurrencyVm
    {
        public int IncarcerationIntakeCurrencyId { get; set; }
        public int IncarcerationId { get; set; }
        public int CreateBy { get; set; }
        public DateTime CreateDate { get; set; }
        public int Currency01Count { get; set; }
        public int Currency05Count { get; set; }
        public int Currency10Count { get; set; }
        public int Currency25Count { get; set; }
        public int Currency50Count { get; set; }
        public int Currency100Count { get; set; }
        public int Currency200Count { get; set; }
        public int Currency500Count { get; set; }
        public int Currency1000Count { get; set; }
        public int Currency2000Count { get; set; }
        public int Currency5000Count { get; set; }
        public int Currency10000Count { get; set; }
        public Decimal OtherAmount { get; set; }
        public string OtherDescription { get; set; }
        public Decimal TotalAmount { get; set; }
        public bool ModifyFlag { get; set; }
        public string ModifyReason { get; set; }
        public string ModifyNote { get; set; }
        public PersonnelVm Personnel { get; set; }
        public int PersonId { get; set; }
    }

    public class IntakeCurrencyViewerVm
    {
        public List<IntakeCurrencyVm> IntakeCurrency { get; set; }
        public List<KeyValuePair<int, string>> CurrencyModifyReason { get; set; }
    }

    public class IntakeCurrencyPdfViewerVm
    {
        public List<IntakeCurrencyVm> IntakeCurrencyList { get; set; }
        public IntakeCurrencyVm IntakeCurrency { get; set; }
        public SentencePdfDetailsVm SentencePdfDetails { get; set; }
    }
}
