using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServerAPI.ViewModels
{
    public class BookingSupervisorVm
    {
        public List<BookingReview> BookingReviewOverall { get; set; }

        public List<RecordsCheckVm> LstRecordsCheckRequest { get; set; }
        public List<ArrestReview> LstReviewBooking { get; set; }
        public List<BookingSearchSubData> LstForceCharges { get; set; }
        public List<IncidentNarrativeDetailVm> LstNarratives { get; set; }

        public List<RequestTypes> PendingRequest { get; set; }
        public List<RequestTypes> AssignedRequest { get; set; }

        public int ReviewOverallCount { get; set; }
        public int ReviewOverallBookCount { get; set; }
        public int ReviewReleaseCount { get; set; }
        public int ReviewReleaseBookCount { get; set; }

        public int ForceChargeCount { get; set; }
        public int RecordsCheckCount { get; set; }
        public int ReviewBookingCount { get; set; }
        public int ReviewClearCount { get; set; }

    }

    public class ArrestReview : ArrestDetailsVm
    {
        public Boolean BookingComplete { get; set; }
        public Boolean ArrestBookingComplete { get; set; }
        public Boolean BookingSupervisorCompleteFlag { get; set; }
        public Boolean ReleaseSupervisorCompleteFlag { get; set; }
        public int? ReviewBookingWizardId { set; get; }
        public List<WizardStep> ReviewBookingWizard { get; set; }
        public int? ReviewClearWizardId { set; get; }
        public List<WizardStep> ReviewClearWizard { get; set; }
        public DateTime? BookingCompleteDate { get; set; }
        public DateTime? ReleaseClearDate { get; set; }
    }
}
