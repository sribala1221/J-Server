using System.Threading.Tasks;
using ServerAPI.ViewModels;
using Microsoft.AspNetCore.Identity;

namespace ServerAPI.Services
{
    public interface IOperationViewerService
    {
        Task<FacilityViewerVm> GetFacilityViewerInit(int facilityId, bool isOperationViewer);

        Task<FacilityViewerVm> GetFacilityViewerRefresh(ViewerParameter objViewer);

        Task<IdentityResult> UpdateUserSettings(ViewerFilter objFilter);

        Task<int> OperationDelete(DeleteParams deleteParams);
    }
}
