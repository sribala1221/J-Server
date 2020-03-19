using System.Collections.Generic;
using ServerAPI.ViewModels;

namespace ServerAPI.Services
{
    public interface IWorkCrewService
    {
        List<WorkCrewVm> GetWorkCrewEntriesCount(int facilityId, List<int> housingUnitListId);
        WorkCrewDetailsVm GetWorkcrewInmateDetails(int facilityId);
    }
}
