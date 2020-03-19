using System.Collections.Generic;
using System.Threading.Tasks;
using ServerAPI.ViewModels;

namespace ServerAPI.Services
{
    public interface IPersonTestingService
    {
        List<TestingVm> GetTestingDetails(int personId);
        Task<int> InsertUpdateTestingDetails(TestingVm testingDet);
        List<HistoryVm> GetTestingHistoryDetails(int testingId);
    }
}
