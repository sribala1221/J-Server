using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GenerateTables.Models;
using Microsoft.WindowsAzure.Storage.Blob;

namespace ServerAPI.ViewModels
{
    public class AOScheduleEntryVm
    {
        DateTime InputTime { get; set; }
        TimeSpan Duration { get; set; }
        int AOScheduleGroupId { get; set; }
    }

    public class AoScheduleWorkCrew
    {
        public WorkCrew WorkCrew { get; set; }
        public Schedule AoSchedule { get; set; }
    }

    public class AoScheduleReturnConflicts
    {
        public int? AoScheduleGroupId { get; set; }
        public int? ScheduleId { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan Duration { get; set; }
    }

    public class AoScheduleFromTo
    {
        public int? GroupId { get; set; }
        public DateTime From { get; set; }
        public DateTime To { get; set; }
    }

    public class AoScheduleOfGroup
    {
        public int GroupId { get; set; }
        public Schedule AoSchedule { get; set;}
    }

    public class AoScheduleCreateException
    {
        public int GroupId { get; set; }
        public ScheduleExclude AoScheduleExclude { get; set; }
    }

}
