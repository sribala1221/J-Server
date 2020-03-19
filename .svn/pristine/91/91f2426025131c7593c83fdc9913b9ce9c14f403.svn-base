using ServerAPI.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServerAPI.Services
{
    public interface IFacilityPrivilegeService
    {
        List<FacilityPrivilegeVm> GetPrivilegeByOfficer(FacilityPrivilegeInput input);

        List<InmatePrivilegeReviewHistoryVm> GetReviewHistory(int inmatePrivilegeXrefId);

        Task<int> InsertReviewHistory(InmatePrivilegeReviewHistoryVm inmatePrivilege);

        List<PrivilegeDetailsVm> GetPrivilegeList(int facilityId);

        List<KeyValuePair<int, string>> GetLocationList(int facilityId);
        IQueryable<PrivilegeDetailsVm> GetTrackingLocationList(int facilityId);
    }
}
