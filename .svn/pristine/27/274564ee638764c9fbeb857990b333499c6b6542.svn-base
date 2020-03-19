using System.Collections.Generic;
using System.Threading.Tasks;
using ServerAPI.ViewModels;

namespace ServerAPI.Services
{
    public interface IMoveService
    {
        List<RecordsDataVm> GetMovePersonSearch(RecordsDataVm searchValue);
        List<DataHistoryVm> GetDataHistory(DataHistoryVm searchValue);
        List<DataHistoryVm> GetDataInfo(DataHistoryVm searchValue, List<int?> personIds, bool sealPerson);
        List<DataHistoryFieldVm> GetDataHistoryFields(int historyId);
        Task<int> DoMove(DoMoveParam objParam);
    }
}
