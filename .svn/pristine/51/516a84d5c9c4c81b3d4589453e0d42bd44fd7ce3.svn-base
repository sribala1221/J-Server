using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ServerAPI.ViewModels;

namespace ServerAPI.Services
{
    public interface IPersonCharService
    {
        PersonCharVm GetCharDetails(int personId);
        Task<int> InsertUpdatePersonChar(PersonCharVm personchar);
        Task<int> InsertUpdatePersonCharFromInterfaceEngineStartPrebookFromKPF(PersonCharVm personchar);
        List<HistoryVm> GetPersonCharHistory(int charId);
        PersonCharVm GetCharacteristics(int personId);
    }
}
