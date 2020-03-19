using System.Collections.Generic;
using System.Threading.Tasks;
using ServerAPI.ViewModels;

namespace ServerAPI.Services
{
    public interface ILogCountService
    {       
       LogCountDetails GetLogCountDetails(LogCountVm value);
    }
}
