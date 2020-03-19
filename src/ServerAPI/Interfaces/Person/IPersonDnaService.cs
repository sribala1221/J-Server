using ServerAPI.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ServerAPI.Services
{
    public interface IPersonDnaService
    {
        List<DnaVm> GetDnaDetails(int personId);
        Task<int> InsertUpdatePersonDna(DnaVm dna);
        List<HistoryVm> GetDnaHistoryDetails(int dnaId);
    }
}
