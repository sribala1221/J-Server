using System;
using System.Collections.Generic;
using GenerateTables.Models;

namespace ServerAPI.ViewModels
{
    public class MailBinVm
    {
         public int MailBinid { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime? UpdateDate { get; set; }
        public DateTime? DeleteDate { get; set; }
        public int CreateBy { get; set; }
        public int? UpdateBy { get; set; }
        public int? DeleteBy { get; set; }
        public bool DeleteFlag { get; set; }
        public int FacilityId { get; set; }
        public string MailBinName { get; set; }
        public bool InmateFlag { get; set; }
        public bool RefusalBinFlag { get; set; }
        public bool HoldBinFlag { get; set; }
        public bool OutgoingBinFlag { get; set; }
        public string AssignedHousingString { get; set; }
        public int Destination { get; set; }
        public bool StaffFlag { get; set; }
    }

//  public class InmateHousingBinVm
// {
//     public HousingDetail HousingDetail { get; set; }

//      public List<MailBin> MailBinList { get; set; }

// }
}
