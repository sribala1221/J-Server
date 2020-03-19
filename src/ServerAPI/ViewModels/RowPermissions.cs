using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServerAPI.ViewModels
{
    public class RowPermissions
    {
        public bool Released { get; set; } = false;
        public int FacilityId { get; set; }
    }

    public enum FuncPermissionType
    {
        Access,
        Print,
        Add,
        Edit,
        Delete,
        ReleaseEdit
    }


}
