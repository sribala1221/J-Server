using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GenerateTables.Models;
using ServerAPI.ViewModels; 

namespace ServerAPI.Services
{
    public interface IRecordsCheckService
    {
        RecordsCheckHistoryVm GetRecordHistroy(int personId);
        List<KeyValuePair<int, string>> GetActionList(string lookupType);
        Task<RecordsCheckVm> InsertRecordsCheck(RecordsCheckVm obSaveRecordsCheck);
        Task<int> InsertBypass(RecordsCheckVm obSaveRecordsCheck);
        Task<RecordsCheckVm> SendResponseRecordsCheck(RecordsCheckVm obSaveRecordsCheck);
        Task<int> DeleteRecordsCheck(int recordCheckId);
        Task<int> InsertFormRecords(FormRecordVm obInsert);
        List<FormRecordVm> FormRecordHist(int formRecordId);
        RecordsCheckVm GetRecordCheck(int formRecordId);
        IQueryable<RecordsCheckRequest> GetRecordsCheckResponseCount(int facilityId);
        List<RecordsCheckVm> GetRecordsCheckResponse(int facilityId);
    }
}
