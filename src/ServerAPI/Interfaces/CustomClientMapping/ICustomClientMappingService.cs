using System.Collections.Generic;
using ServerAPI.ViewModels;

namespace ServerAPI.Services
{
    public interface ICustomClientMappingService
    {
        List<CustomMappingVm> GetClientCustomLabels();
        List<CustomFieldVm> GetCustomFieldLookups(int appAoUserControlId);
    }
}
