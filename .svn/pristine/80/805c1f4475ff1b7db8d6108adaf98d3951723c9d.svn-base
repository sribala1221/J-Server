using System.Threading.Tasks;
using System.Collections.Generic;
using ServerAPI.ViewModels;

namespace ServerAPI.Services
{
    public interface IPersonMilitaryService
    {
        List<PersonMilitaryVm> GetPersonMilitaryDetails(int personId);
        Task<int> InsertUpdatePersonMilitary(PersonMilitaryVm personMilitary);
        List<HistoryVm> GetMilitaryHistoryDetails(int militaryId);
        Task<int> DeletePersonMilitary(PersonMilitaryVm personMilitary);
    }
}
