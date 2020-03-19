using System;
using System.Collections.Generic;

namespace ServerAPI.ViewModels
{
    public class LiveScanDetail
    {
        public List<KeyValuePair<int,string>> Location {get;set;}
        public List<LoadLiveScanBooking> ArrestBookingDetail {get;set;}
        public bool IsAccessProvided { get; set; }
    }

   public class LoadLiveScanBooking
   {
       public string ArrestBookingNumber{get;set;}
       public DateTime ArrestDate{get;set;}
       public string ArrestCourtDocket{get;set;}
       public Boolean BookingCompleteFlag{get;set;}
       public string ArrestType{get;set;}
       public DateTime? ReleaseDate{get;set;}
       public string ArrestCaseNumber{get;set;}
       public string AgencyAbbreviation{get;set;}
       public int ArrestId {get;set;}
   }
}