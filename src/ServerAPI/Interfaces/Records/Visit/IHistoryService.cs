using System.Collections.Generic;
using ServerAPI.ViewModels;

// ReSharper disable once CheckNamespace
namespace ServerAPI.Services
{
    public interface IHistoryService
    {
        List<VisitHistoryDetail> GetVisitHistoryDetails(SearchVisitHistoryVm searchValue);
        List<PrivilegesVm> GetVisitLocation(int facilityId);
    }
}
