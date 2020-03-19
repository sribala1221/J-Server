using System;
using System.Collections.Generic;

namespace ServerAPI.ViewModels
{
    public class IncarcerationDetail
    {
        public int IncarcerationId { get; set; }
        public int InmateId { get; set; }
        public DateTime? DateIn { get; set; }
        public DateTime? ReleaseOut { get; set; }
        public DateTime? SchdRelsDate { get; set; }
        public DateTime? OverallFinalReleaseDate { get; set; }
        public string OrigBookNumber { get; set; }
        public string PrebookNumber { get; set; }
        public bool BookCompleteFlag { get; set; }
        public bool BookReleaseFlag { get; set; }
        public bool IntakeCompleteFlag { get; set; }
        public InmateDetailVm Inmate { get; set; }
        public DateTime? ReleaseIn { get; set; }
        public string TransportHoldName { get; set; }
        public int? TransportHoldTypeId { get; set; }
        public string TransportHoldType { get; set; }
        public string TransportInstructions { get; set; }
        public DateTime? TransportScheduleDate { get; set; }
        public string TransportInmateCautions { get; set; }
        public string TransportInmateBail { get; set; }
        public string UsedPersonLast { get; set; }
        public string UsedPersonFirst { get; set; }
        public string UsedPersonMiddle { get; set; }
        public string UsedPersonSuffix { get; set; }
        public DateTime? UsedPersonDob { get; set; }
        public int PersonId { get; set; }
        public string PresentIncarceration { get; set; }
        public DateTime? DateOut { get; set; }
        public List<AltSentDetail> AltSentDetails { get; set; }
        public List<ArrestBookingDetails> ArrestBookingDetails { get; set; }
        public List<InmateTrackingHistroyVm> InmateTrackingHistory { get; set; }
        // Inmate Header incarceration info
        public int? UnSentCnt { get; set; }
        public int SentCnt { get; set; }
        public int AltSentCnt { get; set; }
        public int HoldCnt { get; set; }
        public int HoldCCnt { get; set; }
        public int HoldCdCnt { get; set; }
        public int HoldICnt { get; set; }
        public int ClearCnt { get; set; }
        public int XbailCnt { get; set; }
        public int YbailCnt { get; set; }
        public bool IsNoBail { get; set; }
        public decimal BailAmount { get; set; }
        public DateTime? OverallStartDate { get; set; }
        public DateTime? OverallReleaseDate { get; set; }
        public int? ActualDaysToServe { get; set; }
        public DateTime? IncarStartDate { get; set; }
        public DateTime? IncarEndDate { get; set; }
        public string DefaultBookingNumber { get; set; }
        public bool NoKeeper { get; set; }
        public bool AssessmentCompleteFlag { get; set; }
        public int DaysDiff{get;set;}
        public string BookingNo{get;set;}
        public string ReceiveMethod{get;set;}
        public DateTime? OverallSentStartDate{get;set;}
        public int TotSentDays{get;set;}
        public int ReleaseCompleteBy{get;set;}
        public DateTime? ReleaseClearDate{get;set;}
        public int InOfficerId{get;set;}
        public int TransportFlag{get;set;}
        public int ReleaseClearBy{get;set;}
    }

}
