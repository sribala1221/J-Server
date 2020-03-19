using System.Collections.Generic;
using ServerAPI.ViewModels;
using System.Threading.Tasks;
using GenerateTables.Models;

namespace ServerAPI.Services
{
    public interface IBookingReleaseService
    {
        BookingReleaseVm GetBookingRelease(BookingInputsVm iBooking);
        BookingReleaseDetailsVm GetBookingReleaseDetails(BookingInputsVm iBooking);
        void CalculateBailTotalAmount(int arrestId, int personId, bool doNotDoSaveHistory, bool bailTransaction);
        OverallIncarceration getOverallIncarceration(int incarcerationId, int userControlId);
        Task<int> UpdateOverallIncarceration(OverallIncarceration overallIncarceration);
        BookingTransportDetailsVm GetBookingTransportDetails(int incarcerationId, bool deleteFlag);
        TransportManageDetailVm GetTransportManageDetails(int incarcerationId);
        int UpdateManageDetails(TransportManageVm transportManageVm);
        int InsertTransportNote(TransportNote transportNote);
        bool GetSupervisorValidate(int incarcerationId);
        int UpdateTransportNote(TransportNote transportNote);
		InmateReleaseVm GetInmateRelease(int incarcerationId, int personId, int inmateId);
        BailDetails BailAmount(int arrestId, int personId, int siteOptionId);
        List<BookingNumberList> GetInmateByBooking(string bookingNumber, int inmateId);
        Task<int> UpdateDoReleaseAsync(int inmateId, int incarcerationId);
        Task<int> UpdateUndoReleaseAsync(int inmateId, int incarcerationId);
        BookingOverviewDetails GetInmateReleaseValidation(int inmateId);
        bool UpdateOverAllChargeLevel(int incarcerationId);
        int UpdateClearFlag(int incarcerationId);
    }
}

