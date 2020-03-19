using System;
using System.Collections.Generic;
using ServerAPI.ViewModels;
using System.Threading.Tasks;

namespace ServerAPI.Services
{
    public interface IBookingService
    {
        ExtAttachApproveVm GetExternalAttachmentDetails(int inmateId);
        BookingNoteVm GetBookingNoteDetails(int arrestId, bool deleteFlag);
        Task<int> UpdateExternalAttachment(ExternalAttachmentsVm eaVm);
        Task<int> DeleteBookingNote(BookingNoteDetailsVm value);
        BookNoteEntryVm LoadInmateBookings(int inmateId);
        Task<int> SaveBookingNoteDetails(BookingNoteDetailsVm noteDetails);
        List<BookingClearVm> GetBookingClearDetails(int incarcerationId);
        List<HistoryVm> GetClearHistoryDetails(int arrestId);
        BookClearVm GetBookingClearlist(int arrestId);
        Task<int> UpdateClear(BookingClearVm bookingClear);
        OverallSentvm CheckOverallSentDetails(int arrestId, int incarcerationId);
        List<ClearChargesVm> GetSentenceCharges(int[] arrestId);
        CurrentStatus GetCurrentStatus(int incarcerationId);
        OercMethod GetOercDetails();
        List<string> GetCautionflag(int personId);
        List<KeyValuePair<int?, decimal?>> GetTotalBailAmountandNoBailFlag(int incarcerationId);
        Task<int> UpdateOverAllSentence(OverallSentence overallSentence);
        Task<int> UndoClearBook(UndoClearBook undoClearBook);
        IEnumerable<BookingActive> GetActiveBooking(int facilityId, BookingActiveStatus status);
        Task<int> UpdateBookingComplete(BookingComplete bookingComplete);
        List<SentenceVm> GetBookingSentence(int incarcerationId);
        Task<int> UpdateRequest(RequestClear requestClear);
        Task<int> UpdateClearRequest(RequestClear requestClear);

        BookingPrebook GetBookingPrebookForms(int incarcerationId);

        IncarcerationFormListVm LoadFormDetails(int incarcerationId, int incarcerationArrestId, int arrestId,
            int formTemplateId);

        List<IncarcerationDetail> GetIncarcerationAndBookings(int inmateId, bool toBindCharge = false,
            bool isActiveIncarceration = false, DateTime? dateIn = null, DateTime? releaseOut = null);

        List<ArrestBookingDetails> GetBookings(int incarcerationId, bool toBindCharge = false);
        List<InmateHousing> GetInmateDetails(List<int> inmateIds);

        List<TaskOverview> GetAllCompleteTasks(int inmateId);
    }
}

