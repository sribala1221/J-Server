using System;
using System.Collections.Generic;

namespace ServerAPI.ViewModels
{
    public class InmateMailPrivilegeVm
    {

        public List<FacilityPrivilegeVm> FacilityPrivilegeList { get; set; }
         public List<KeyValuePair<string, int>> PriviligeCounts { get; set; }
    }
}