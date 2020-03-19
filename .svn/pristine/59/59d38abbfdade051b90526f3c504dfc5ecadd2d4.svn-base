using System.Collections.Generic;
using System.Threading.Tasks;
using ServerAPI.ViewModels;

namespace ServerAPI.Services
{
    public interface IPersonnelService
    {       
        PersonnelDetailsVm GetPersonnelDetails(PersonnelFilter personnelSearch);
        PersonnelInputVm GetPersonnelInputDetails(int personnelId);
        Task<int> InsertUserPersonnel(PersonnelInputVm personnelInput);   
        List<KeyValuePair<int, string>> GetDepartmentPosition(string forFlag);
        Task<int> InsertDepartmentPosition(KeyValuePair<string, string> param);
    }
}
