using ServerAPI.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;
using GenerateTables.Models;

// ReSharper disable once CheckNamespace
namespace ServerAPI.Services
{
    public interface IDataMergeService
    {
        List<RecordsDataVm> GetNewMergePersons(RecordsDataVm searchValue);
        List<RecordsDataVm> GetPersonsToAddMerge(RecordsDataVm searchValue);
        List<BookingDataVm> GetRecordsDataInmateBooking(int[] inmateIds);
        List<AkaVm> GetRecordsDataAkaDetails(int[] personIds);
        List<RecordsDataReferenceVm> GetPersonReferences(List<KeyValuePair<int, int>> lstIds, RecordsDataType type, 
            int incarcerationId = 0);
        List<RecordsDataReferenceVm> GetPersonReferenceDetails(int dataAoLookupId, int inmateId, int personId, 
            int incarcerationId = 0);
        List<RecordsDataReferenceVm> GetReferenceDetail(DataAoLookup dataAoLookup, int? inmateId, int personId,int incarcerationId = 0);
        void MergeRecords(DataAoLookup dataAo, int? toId, int? refId);
        Task<int> DoMerge(DoMergeParam doMergeParam);
    }
}
