using ServerAPI.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ServerAPI.Services
{
    public interface ISentenceService
    {
        SentenceViewerVm GetSentenceViewerList(int arrestId, int incarcerationId);
        ChargeSentenceViewerVm GetChargeSentenceViewerList(int arrestId, int incarcerationId);
        List<BookingStatusHistoryVm> GetBookingStatusHistory(int arrestId);
        Task<int> UpdateArrestBookingStatus(BookingStatusVm value);
        SentenceCreditServedVm GetSentenceCreditServed(int inmateId, 
            int incarcerationArrestXrefId, string courtDocket);
        List<SentenceAdditionalFlagsVm> GetSentenceAdditionalFlags(int arrestId);
        Task<List<SentenceAdditionalFlagsVm>> InsertArrestSentFlag(List<SentenceAdditionalFlagsVm> value);
        Task<int> DeleteArrestSentFlag(int arrestSentFlagId);
        SentenceDetailsVm GetSentenceDetailsArrest(int arrestId);
        SentenceMethodVm GetSentenceMethod(int sentenceMethodId);
        Task<int> ClearArrestSentence(int arrestId);
        Task<int> ClearCrimeSentence(int crimeId);
        Task<int> UpdateArrestSentence(SentenceDetailsVm value);
        List<DisciplinaryDays> GetDisciplinaryDays(int arrestId);
        Task<int> UpdateArrestSentenceGap(List<SentenceDetailsVm> value);
        Task<int> UpdateCrimeSentenceGap(List<ChargeSentenceVm> value);
        Task<int> UpdateChargeSentence(SentenceDetailsVm value);
        Task<int> UpdateChargeSentenceList(SentenceDetailsVm value, List<int> crimeIds);
        List<ChargeSentenceVm> GetAllArrestSentenceDetailsCrime(int arrestId);
        SentenceDetailsVm GetSentenceDetailsCrime(int crimeId);
        Task<int> UpdateOverallSentence(OverallSentenceVm value);
        OverallSentenceVm GetOverallSentence(int incarcerationId);
        List<ArrestSentenceSettingVm> GetArrestSentenceSetting();
        List<SentenceVm> GetSentenceDetailsIncarceration(int incarcerationId);
        List<OverallSentenceVm> GetOverallIncarcerationHistory(int incarcerationId);
        List<HistoryVm> GetArrestSentenceHistory(int arrestId, int crimeId);
        SentencePdfViewerVm GetSentencePdfViewerList(int incarcerationId, int arrestId, 
            SentenceSummaryType reportType, int crimeId);

        BookingStatusVm GetCaseConviction(int arrestId);
    }
}