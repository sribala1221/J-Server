using System.Collections.Generic;
using ServerAPI.ViewModels;

namespace ServerAPI.Policies
{
    public interface IUserPermissionPolicy
    {
        List<KeyValuePair<string,int>> GetAppPermission();
        List<Module> GetMenuPermission(List<Module> lstModule, int appAoId);
        List<LookupVm> GetFlagPermission(List<LookupVm> staticFlagLst);
        List<string> GetClaims();
    }
}