using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GenerateTables.Models;
using Microsoft.AspNetCore.Http;
using ServerAPI.Utilities;
using ServerAPI.ViewModels;

// ReSharper disable once CheckNamespace
namespace ServerAPI.Services
{
    public class DarkDaysService : IDarkDaysService
    {
        private readonly AAtims _context;
        private readonly int _personnelId;

        public DarkDaysService(AAtims context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _personnelId = Convert.ToInt32(httpContextAccessor.HttpContext.User
                .FindFirst(AuthDetailConstants.PERSONNELID)?.Value);
        }
        
        public DarkDayVm GetDarkDays() 
        { 
            DarkDayVm darkDayVm = new DarkDayVm()
            {
                LstDarkDays = _context.DarkDay
                    .Select(w => new DarkDayInfo{
                        DarkDayId = w.DarkDayId,
                        Description = w.DarkDayDescription,
                        Notes = w.DarkDayNotes,
                        DarkDayDate = w.DarkDayDate ?? DateTime.Now,
                        DeleteFlag = w.DeleteFlag == 1
                    }).ToList(),
                LstLocation = _context.Privileges
                    .Where(p => p.InactiveFlag == 0 
                        && p.DarkDaysFlag == 1)
                    .Select(a => new PrivilegesVm{
                        PrivilegesId = a.PrivilegeId,
                        PrivilegesDescription = a.PrivilegeDescription,
                        FacilityId = a.FacilityId
                    }).OrderBy(a => a.PrivilegesDescription).ToList()
            };

            return darkDayVm;
        }

        public async Task<int> InsertDarkDays(DarkDayInfo model)
        {
            DarkDay darkDay = new DarkDay();
            darkDay.DarkDayDate = model.DarkDayDate;
            darkDay.DarkDayDescription = model.Description;
            darkDay.DarkDayNotes = model.Notes;
            darkDay.CreatedBy = _personnelId;
            darkDay.CreatedDate = DateTime.Now;
            _context.Add(darkDay);
            return await _context.SaveChangesAsync();
        }

        public async Task<int> UpdateDarkDays(DarkDayInfo model)
        {
            DarkDay darkDay = _context.DarkDay
                .Where(w => w.DarkDayId == model.DarkDayId).Single();
            darkDay.DarkDayDate = model.DarkDayDate;
            darkDay.DarkDayDescription = model.Description;
            darkDay.DarkDayNotes = model.Notes;
            darkDay.UpdatedBy = _personnelId;
            darkDay.UpdatedDate = DateTime.Now;
            return await _context.SaveChangesAsync();
        }

        public DarkDayVm DeleteDarkDays(DarkDayInfo model)
        {
            DeleteUndoDarkDays(model);
            return GetDarkDays();
        }

        public DarkDayVm UndoDarkDays(DarkDayInfo model)
        {
            DeleteUndoDarkDays(model);
            return GetDarkDays();
        }

        private void DeleteUndoDarkDays(DarkDayInfo model)
        {
            DarkDay darkDay = _context.DarkDay
                .Where(w => w.DarkDayId == model.DarkDayId).Single();
            if(model.DeleteFlag)
            {
                darkDay.DeleteBy = _personnelId;
                darkDay.DeleteDate = DateTime.Now;
            }
            else
            {
                darkDay.DeleteBy = null;
                darkDay.DeleteDate = null;
            }
            darkDay.DeleteFlag = model.DeleteFlag ? 1 : 0;
            _context.SaveChanges();
        }
    }
}