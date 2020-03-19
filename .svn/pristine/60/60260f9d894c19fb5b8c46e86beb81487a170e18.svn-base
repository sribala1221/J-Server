using System.Security.Claims;
using System.Threading.Tasks;

namespace ServerAPI.Authentication
{
    public interface IJwtFactory
    {
        Task<string> GenerateEncodedToken(ClaimsIdentity identity);
        ClaimsIdentity GenerateClaimsIdentity(string userName);
    }
}
