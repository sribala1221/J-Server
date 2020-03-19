using System.Collections.Generic;
using ServerAPI.ViewModels;

namespace ServerAPI.Services
{
    public interface ITrackFurlService
    {
        WorkCrewFurloughTracking GetFurloughDetails(int facilityId);
        List<WorkCrewFurloughCountVm> GetFurloughCount(int facilityId, List<int> housingUnitListId);

    }
}
