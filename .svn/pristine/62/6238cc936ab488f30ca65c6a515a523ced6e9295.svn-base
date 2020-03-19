using ServerAPI.ViewModels;
using System.Collections.Generic;

namespace ServerAPI.Services
{
    public interface IDataSealService
    {
        List<RecordsDataVm> GetPersonSeal(RecordsDataVm searchValue);
        string DoSeal(DoSeal doSeal);
        string DoUnSeal(DoSeal doUnSeal);
        List<KeyValuePair<int, string>> LoadSealLookUp();
        List<DataHistoryVm> SealHistory(DataHistoryVm searchValue);
    }
}
