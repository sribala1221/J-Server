using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServerAPI.ViewModels
{
    public class PrintRefusalNotificationVm
    {

        public string MailBinName { get; set; }
        public string CurrentDate { get; set; }
        public string DestinationName { get; set; }
        public string MailTypeName { get; set; }
        public string SenderTypeName { get; set; }
        public string HousingData { get; set; }
        public string InmateName { get; set; }
        public string InmateNumber { get; set; }
        public string Name { get; set; }
        public string RefusalName { get; set; }
        public string Type { get; set; }
        public int? MailRecordid { get; set; }

        public int? LookupFlag6 { get; set; }
        public int? LookupFlag7 { get; set; }

        public string SendOther { get; set; }
        public bool Flag { get; set; }
        public string MoneyAmount { get; set; }
         public string MoneyType { get; set; }
          public string MoneyNumber { get; set; }
    }
    
}