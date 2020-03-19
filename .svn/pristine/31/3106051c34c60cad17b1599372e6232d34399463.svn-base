using GenerateTables.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using GenerateTables.ScheduleWidget.Common;
using ServerAPI.ViewModels;

namespace ServerAPI.Services
{
    public class AoScheduleService : IAoScheduleService
    {

        private readonly AAtims _context;

        public AoScheduleService(AAtims context)
        {
            _context = context;
        }

        

        //Create Exception Against Schedule
        public int InsertScheduleExclusion(ScheduleExclude aoScheduleExclude, int personnelId)
        {
            ScheduleExclude exclude = new ScheduleExclude
            {
                CreateBy = personnelId,
                CreateDate = DateTime.Today,
                ExcludeReason = aoScheduleExclude.ExcludeReason,
                ExcludeNote = aoScheduleExclude.ExcludeNote,
                ExcludedDate = aoScheduleExclude.ExcludedDate,
                ScheduleId = aoScheduleExclude.ScheduleId
            };

            Schedule s = _context.Schedule.FirstOrDefault(w => w.ScheduleId == exclude.ScheduleId);
            if (s != null)
            {
                s.ScheduleExclude.Add(exclude);
            }
            _context.SaveChanges();
            return exclude.ScheduleExcludeId;
        }

        public int InsertInmateExclusion(ScheduleExcludeInmate aoScheduleExcludeInmate, int personnelId)
        {
            ScheduleExcludeInmate exclude = new ScheduleExcludeInmate
            {
                CreateBy = personnelId,
                CreateDate = DateTime.Today,
                ExcludeReason = aoScheduleExcludeInmate.ExcludeReason,
                ExcludeNote = aoScheduleExcludeInmate.ExcludeNote,
                ExcludedDate = aoScheduleExcludeInmate.ExcludedDate,
                ScheduleId = aoScheduleExcludeInmate.ScheduleId,
                InmateId = aoScheduleExcludeInmate.InmateId
            };

            Schedule s = _context.Schedule.FirstOrDefault(w => w.ScheduleId == exclude.ScheduleId);
            if (s != null)
            {
                _context.ScheduleExcludeInmate.Add(exclude);
            }
            _context.SaveChanges();
            return exclude.ScheduleId;
        }

        
    }
}









