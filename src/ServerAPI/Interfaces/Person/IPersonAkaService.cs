using ServerAPI.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ServerAPI.Services
{
    public interface IPersonAkaService
    {
        List<AkaVm> GetAkaDetails(int personId);
        Task<int> DeleteUndoPersonAka(AkaVm aka);
        List<HistoryVm> GetPersonAkaHistory(int akaId);
        Task<int> InsertUpdatePersonAka(AkaVm aka);
        void LoadInsertAkaHistory(int akaId, string personAkaList);
    }
}
