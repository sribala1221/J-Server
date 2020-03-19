using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServerAPI.ViewModels
{
    public class AOScheduleExcludeVm
    {

        public int ScheduleId { get; set; }
        public DateTime CreateDate { get; set; }
        public int CreatedBy { get; set; }
        public string ExcludeReason { get; set; }
        public string ExcludeNote {get;set;}
        public DateTime ExcludeDate { get; set; }


    }
}
