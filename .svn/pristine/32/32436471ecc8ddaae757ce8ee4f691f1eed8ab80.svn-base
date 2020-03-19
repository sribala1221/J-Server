using ServerAPI.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ServerAPI.Services
{
    public interface IObservationLogService
    {
        ObservationLogsVm GetObservationLogList(ConsoleInputVm value);
        List<ObservationLogItemDetails> LoadObservationHistoryList(int observationActionId, int inmateId);
        ObservationLogsVm LoadObservationLogList(ConsoleInputVm value);
        ObservationScheduleVm LoadObservationScheduleEntry(int observationScheduleId);
        ObservationScheduleActionVm LoadObservationActionEntry(int observationScheduleActionId);
        Task<int> InsertObservationLog(ObservationLogItemDetails logItem);
    }
}
