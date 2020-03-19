using System.Collections.Generic;
using ServerAPI.ViewModels;
using System.Threading.Tasks;

namespace ServerAPI.Services
{
    public interface IBookingProgressService
    {
        BookingInProgressCountVm GetBookingInProgress(BookingInputsVm iBooking);
        BookingInProgressDetailsVm GetBookingInProgressDetails(BookingInputsVm iBooking);
        BookingOverviewVm GetBookingOverviewDetails(int facilityId);
        BookingOverviewVm GetBookingOverview(int facilityId,int inmateId);
    }
}
