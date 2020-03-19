using ServerAPI.ViewModels;
using System.Security.Claims;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace ServerAPI.Services
{
    public interface IPersonHeaderService
    {

        PersonDetail GetPersonDetails(int personId, bool isInmate, int personnelId);
        Task<PersonDetail> GetPersonHeader(int personId, ClaimsPrincipal userPrincipal);
        InmateHeaderInfoVm GetPersonInfo(int inmateId);
    }
}
