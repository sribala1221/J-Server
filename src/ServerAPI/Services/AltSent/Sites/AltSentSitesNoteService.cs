using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using GenerateTables.Models;
using ServerAPI.ViewModels;
using ServerAPI.Utilities;

namespace ServerAPI.Services
{
    public class AltSentSitesNoteService : IAltSentSitesNoteService
    {
        private readonly AAtims _context;
        private readonly int _personnelId;

        public AltSentSitesNoteService(AAtims context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _personnelId = Convert.ToInt32(httpContextAccessor.HttpContext.User
                .FindFirst(AuthDetailConstants.PERSONNELID)?.Value);
        }

        public List<AltSentProgramDetails> GetAltSentProgramDetails() =>
            _context.AltSentProgram.Where(w => (!w.InactiveFlag.HasValue || w.InactiveFlag == 0)
                                               && w.Facility.AltSentFlag == 1)
                .OrderBy(o => o.AltSentProgramId)
                .Select(s => new AltSentProgramDetails
                {
                    AltSentProgramId = s.AltSentProgramId,
                    AltSentProgramAbbr = s.AltSentProgramAbbr,
                    FacilityAbbr = s.Facility.FacilityAbbr
                }).ToList();

        public List<KeyValuePair<int, string>> GetAltSentSiteDetails(int altSentProgramId) =>
            _context.AltSentSite.Where(w => w.AltSentProgramId == altSentProgramId)
                .OrderByDescending(o => o.AltSentSiteId)
                .Select(s => new KeyValuePair<int, string>(s.AltSentSiteId, s.AltSentSiteName)).ToList();

        public List<SiteApptDetails> GetSiteApptDetails(SiteApptParam objParam)
        {
            List<int> altSentInmateId = _context.AltSent.Where(w => w.AltSentProgramId == objParam.AltSentProgramId)
                .Select(s => s.Incarceration.InmateId ?? 0).ToList();

            List<SiteApptDetails> lstSiteApptDetails = _context.AltSentSiteNote
                .Where(w => altSentInmateId.Contains(w.InmateId)
                            && (!objParam.InmateId.HasValue || w.InmateId == objParam.InmateId)
                            && (!objParam.CreateBy.HasValue || w.CreatedBy == objParam.CreateBy)
                            && (!objParam.SiteId.HasValue || w.AltSentSiteId == objParam.SiteId)
                            && (objParam.NoteKeyword == null || objParam.NoteKeyword.Contains(w.AltSentSiteNote1))

                            && (w.AltSentDay.AltSentDate.HasValue && objParam.NotesToday
                                ? w.AltSentDay.AltSentDate.Value.Date == DateTime.Now.Date
                                : !objParam.NotesDateFrom.HasValue || !objParam.NotesDateTo.HasValue ||
                                  !w.AltSentDay.AltSentDate.HasValue ||
                                  (w.AltSentDay.AltSentDate.Value.Date >= objParam.NotesDateFrom.Value.Date &&
                                   w.AltSentDay.AltSentDate.Value.Date <= objParam.NotesDateTo.Value.Date))

                            && (!objParam.ReminderDateFrom.HasValue || !objParam.ReminderDateTo.HasValue ||
                                !w.RemainderDate.HasValue ||
                                (w.RemainderDate.Value.Date >= objParam.ReminderDateFrom.Value.Date &&
                                 w.RemainderDate.Value.Date <= objParam.ReminderDateTo.Value.Date)))
                .Select(s => new SiteApptDetails
                {
                    SiteNoteId = s.AltSentSiteNoteId,
                    NoteTime = s.AltSentNoteTime,
                    NoteType = _context.Lookup.SingleOrDefault(w => w.LookupInactive == 0
                                                        && w.LookupType == LookupConstants.NOTETYPESITE
                                                        && w.LookupIndex == s.AltSentNoteType).LookupDescription,
                    DeleteFlag = s.DeleteFlag,
                    SiteNote = s.AltSentSiteNote1,
                    SiteId = s.AltSentSiteId,
                    SiteName = s.AltSentSite.AltSentSiteName,
                    ReminderDate = s.RemainderDate,
                    CreateDate = s.CreateDate,
                    CreateBy = s.CreatedBy,
                    AltSentDate = s.AltSentDay.AltSentDate,
                    PersonInfo = new PersonInfoVm
                    {
                        InmateId = s.InmateId,
                        InmateNumber = s.Inmate.InmateNumber,
                        PersonLastName = s.Inmate.Person.PersonLastName,
                        PersonFirstName = s.Inmate.Person.PersonFirstName,
                        PersonMiddleName = s.Inmate.Person.PersonMiddleName
                    }
                }).ToList();

            return lstSiteApptDetails;
        }

        public async Task<int> InsertSiteNote(SiteApptDetails siteNoteParam)
        {
            //To get AltSentDayId from AltSentDate
            int altSentDayId = _context.AltSentDay.Where(w => w.AltSentDate == siteNoteParam.AltSentDate)
                .Select(s => s.AltSentDayId).Single();

            AltSentSiteNote altSentSiteNote = new AltSentSiteNote
            {
                AltSentSiteId = siteNoteParam.SiteId,
                AltSentDayId = altSentDayId,
                AltSentNoteTime = siteNoteParam.NoteTime,
                InmateId = siteNoteParam.InmateId,
                AltSentSiteNote1 = siteNoteParam.SiteNote,
                AltSentNoteType = siteNoteParam.NoteTypeId,
                RemainderFlag = siteNoteParam.ReminderFlag,
                RemainderClear = false,
                CreatedBy = _personnelId,
                CreateDate = DateTime.Now
            };

            _context.AltSentSiteNote.Add(altSentSiteNote);

            return await _context.SaveChangesAsync();
        }

        public async Task<int> UpdateSiteNote(int siteNoteId, SiteApptDetails siteNoteParam)
        {
            AltSentSiteNote altSentSiteNote = _context.AltSentSiteNote.Single(s => s.AltSentSiteNoteId == siteNoteId);

            //To get AltSentDayId from AltSentDate
            int altSentDayId = _context.AltSentDay.Where(w => w.AltSentDate == siteNoteParam.AltSentDate)
                .Select(s => s.AltSentDayId).Single();

            altSentSiteNote.AltSentSiteId = siteNoteParam.SiteId;
            altSentSiteNote.AltSentDayId = altSentDayId;
            altSentSiteNote.AltSentNoteTime = siteNoteParam.NoteTime;
            altSentSiteNote.InmateId = siteNoteParam.InmateId;
            altSentSiteNote.AltSentSiteNote1 = siteNoteParam.SiteNote;
            altSentSiteNote.AltSentNoteType = siteNoteParam.NoteTypeId;
            altSentSiteNote.RemainderFlag = siteNoteParam.ReminderFlag;
            altSentSiteNote.RemainderDate = siteNoteParam.ReminderDate;
            altSentSiteNote.UpdatedBy = _personnelId;
            altSentSiteNote.UpdateDate = DateTime.Now;

            return await _context.SaveChangesAsync();
        }

        public async Task<int> DeleteSiteNote(int siteNoteId)
        {
            AltSentSiteNote altSentSiteNote = _context.AltSentSiteNote.Single(s => s.AltSentSiteNoteId == siteNoteId);

            altSentSiteNote.DeleteFlag = 1;
            altSentSiteNote.DeleteDate = DateTime.Now;
            altSentSiteNote.DeleteBy = _personnelId;

            return await _context.SaveChangesAsync();
        }
    }
}