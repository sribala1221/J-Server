using System;

namespace ServerAPI.ViewModels
{
    public class ChargesVm
    {
        public int CrimeId { get; set; }
        public int CrimeForceId { get; set; }
        public string CrimeSection { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }
        public DateTime? CreateDate { get; set; }
        public string CrimeSubSection { get; set; }
        public string CrimeStatueCode { get; set; } // alise Statue
        public int WarrantId { get; set; }
        public string WarrantNumber { get; set; }
        public string WarrantBailType { get; set; }
        public decimal? BailAmount { get; set; }
        public int? BailNoBailFlag { get; set; }

        public int IncarcerationId { get; set; }

    }
}
