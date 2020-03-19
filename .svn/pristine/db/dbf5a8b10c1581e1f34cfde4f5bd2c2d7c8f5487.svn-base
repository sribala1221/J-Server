using System;
using ServerAPI.ViewModels;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace ServerAPI.Services
{
    public interface ISupervisorService
    {
        BookingSupervisorVm GetBookingSupervisor(int facilityId);
        int CompleteForceCharge(PrebookCharge charges, int option);
        Task<int> UpdateReviewComplete(BookingComplete assessmentComplete);
        bool GetReleaseValidation(int incarcerationId);
        List<BookingSearchSubData> GetForceCharge(int facilityId);
        List<RecordsCheckVm> GetRecordsCheckResponse(int facilityId);
        List<ArrestReview> GetReviewBooking(int iFacilityId, bool isClear);
        List<BookingReview> GetOverallReview(int iFacilityId);
    }
}
