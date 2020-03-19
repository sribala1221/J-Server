using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using ServerAPI.ViewModels;

namespace ServerAPI.Services
{
    public interface IPermissionsService
    {
        Task<string> FunctionPermissionCheck(FunctionPermissionCheck functionPermissionCheck);
        bool FunctionPermissionConditionCheck(string condition, HttpContext httpContext);
    }
}
