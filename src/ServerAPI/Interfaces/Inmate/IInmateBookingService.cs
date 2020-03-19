using System.Collections.Generic;
using System.Threading.Tasks;
using ServerAPI.ViewModels;


namespace ServerAPI.Services
{
    public interface IInmateBookingService
    {
        InmateBookingDetailsVm GetInmateBookingDetails(InmateBookingData inmateData);
        List<IncarcerationArrestXrefDetails> GetBookingDataDetails(int inmateId, int facilityId);
        List<ActiveCommitDetails> GetActiveCommitDetails(int personId);
        Task<int> DeleteInmateActiveCommit(int inmatePrebookId);
        Task<int> UndoInmateBooking(InmateBookingData inmateData);
        Task<int> UndoInmateBookingPopup(InmateBookingData inmateData);
        Task<int> UndoReleaseInmateBooking(InmateBookingData inmateData);
        InmateBookingInfoVm GetInmateBookingInfo(int arrestId, int incarcerationArrestXrefId);
        IntakeEntryVm GetSiteOptionsReactivate(int weekEnder, int arrestId, string bookingNumber);
        List<HistoryVm> GetBookingInfoHistory(int arrestId);
        Task<int> UpdateBookingInfo(BookingInfoArrestDetails arrDetails);
        int PostUpdateSearchOfficiers(string search, int arrestId);
        InmateBookingData GetBookingDataComplete(int inmateId, int incarcerationId, int arrestId);
        Task<int> UpdateBookingCompleteFlag(BookingComplete bookingComplete);
        Task<int> UpdateBookDataCompleteFlag(BookingComplete bookingComplete);
        List<BookingSearchSubData> GetCrimeHistory(int crimeId, int crimeForceId, int prebookChargeId);
        List<BookingSearchSubData> GetInmateCrimeCharges(List<int> arrestIds, BookingChargeType chargeType, bool showDeleted);
        List<InmatePrebookWarrantVm> GetInmateWarrantDetails(int arrestId, int personId);
        Task<int> InsertUpdateCrimeDetails(PrebookCharge chargeDetails);
        int InsertUpdateWarrantDetails(InmatePrebookWarrantVm warrant);
        bool DeleteWarrantDetails(int warrantId);
        Task<bool> DeleteUndoCrimeForce(PrebookCharge chargeDetails);
        Task<bool> DeleteUndoCrimeDetails(PrebookCharge crimeDetailReq);
        Task<bool> ReplicateChargeDetails(PrebookCharge chargeDetails);
        bool InsertPrebookChargesToCrime(List<PrebookCharge> prebookChargeLst, bool isWarrantChage = false);
        bool InsertPrebookWarrantToWarrant(List<InmatePrebookWarrantVm> prebookWarrants);
        IntakeEntryVm GetSupplementalBookingDetails(int inmateId, int incarcerationId);

        Task<int> UpdateBookSupervisorClearFlag(BookingComplete bookingComplete);
        Task<int> UpdateBookSupervisorCompleteFlag(BookingComplete bookingComplete);
        bool GetCourtdateArraignment(BookingInfoArrestDetails arrestDetails);
    }
}
