using System.Collections.Generic;
using GenerateTables.Models;
using ServerAPI.Models;

namespace ServerAPI.Services
{
    public interface IUserPermissionsService {
        UserAccess GetUser(string userName);
        UserPermissions BuildFlatPermissions(int userId);
        UserPermissionsZip BuildZipPermissions(int userId);
        IList<int> BuildPermissionNumbers(int userId);
    }
}
