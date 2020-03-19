using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServerAPI.ViewModels
{
    public class AOScheduleGroupVm
    {

        public int AOScheduleGroupId { get; set; }
        public int DefaultLocationId { get; set; }
        public DateTime InputDate { get; set; }
        public String Reason { get; set; }

    }

}
