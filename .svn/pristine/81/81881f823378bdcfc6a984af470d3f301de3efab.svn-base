using System.Collections.Generic;

namespace ServerAPI.Models
{
    public class UserPermissions
    {
        public int UserId { get; set; }
        public Dictionary<int, PermissionVector> permissions { get; set; }
    }

    public class UserPermissionsZip
    {
        public int UserId { get; set; }
        public Dictionary<int, int[]> shortPermissions { get; set; }
    }

    public class PermissionVector {
        public bool Access { get; set; }
        public bool Add { get; set; }
        public bool Delete { get; set; }
        public bool Edit { get; set; }
        public bool Print { get; set; }
        public bool RDelete { get; set; }
        public bool REdit { get; set; }
    }
}
