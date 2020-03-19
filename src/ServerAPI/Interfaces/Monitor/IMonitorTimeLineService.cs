using System.Collections.Generic;
using System.Threading.Tasks;
using ServerAPI.ViewModels;

namespace ServerAPI.Services
{
    public interface IMonitorTimeLineService
    {
        List<MonitorTimeLineDetailsVm> GetMonitorTimeLine(MonitorTimeLineSearchVm searchValues);
    }
}
