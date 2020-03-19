using System.Collections.Generic;
using System.Threading.Tasks;
using ServerAPI.ViewModels;

// ReSharper disable once CheckNamespace
namespace ServerAPI.Services
{
    public interface IDarkDaysService
    {
        DarkDayVm GetDarkDays();
        Task<int> InsertDarkDays(DarkDayInfo model);
        Task<int> UpdateDarkDays(DarkDayInfo model);
        DarkDayVm DeleteDarkDays(DarkDayInfo model);
        DarkDayVm UndoDarkDays(DarkDayInfo model);
    }
}