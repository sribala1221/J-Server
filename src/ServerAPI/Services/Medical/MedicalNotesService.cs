using System;
using System.Collections.Generic;
using System.Linq;
using GenerateTables.Models;
using Microsoft.AspNetCore.Http;
using ServerAPI.ViewModels;
using System.Threading.Tasks;
using ServerAPI.Utilities;

// ReSharper disable once CheckNamespace
namespace ServerAPI.Services
{
    public class MedicalNotesService : IMedicalNotesService
    {
        private readonly AAtims _context;
        private readonly int _personnelId;

        public MedicalNotesService(AAtims context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _personnelId = Convert.ToInt32(httpContextAccessor.HttpContext.User
                .FindFirst(AuthDetailConstants.PERSONNELID)?.Value);
        }

        public MedicalNotesVm GetMedicalNotes(MedicalNotes medicalNoteSearch)
        {
            MedicalNotesVm lstMedicalNotesDetails = new MedicalNotesVm();
            lstMedicalNotesDetails.lstMedicalNotes = _context.InmateMedicalNote.Where(f => f.InmateMedicalNoteId > 0
               && (medicalNoteSearch.ActiveOnly == null || medicalNoteSearch.ActiveOnly == 0 || f.Inmate.InmateActive == medicalNoteSearch.ActiveOnly)
               && (medicalNoteSearch.DeletedOnly == 1 || f.DeleteFlag == null)
               && (medicalNoteSearch.InmateId == 0 || f.InmateId == medicalNoteSearch.InmateId)
               && (medicalNoteSearch.NoteType == null || f.MedicalNoteType == medicalNoteSearch.NoteType)
               && (f.Inmate.FacilityId == medicalNoteSearch.FacilityId)
               && (medicalNoteSearch.PersonnelId == 0 || f.CreateBy == medicalNoteSearch.PersonnelId)
               && (medicalNoteSearch.FromDate == null || f.CreateDate.Value.Date >= medicalNoteSearch.FromDate.Value.Date
               && f.CreateDate.Value.Date <= medicalNoteSearch.ToDate.Value.Date))
            .Select(s => new MedicalNotes
            {
                InmateId = s.InmateId,
                InmateNumber = s.Inmate.InmateNumber,
                InmateMedicalNoteId = s.InmateMedicalNoteId,
                PersonId = s.Inmate.Person.PersonId,
                Note = s.Note,
                MedicalNoteType = s.MedicalNoteType,
                InmateActive = s.Inmate.InmateActive,
                FacilityId = s.Inmate.FacilityId,
                DeleteFlag = s.DeleteFlag ?? 0,
                CreateDate = s.CreateDate,
                UpdatedDate = s.UpdateDate,
                DeletedDate = s.DeleteDate,
                PersonFirstName = s.Inmate.Person.PersonFirstName,
                PersonMiddleName = s.Inmate.Person.PersonMiddleName,
                PersonLastName = s.Inmate.Person.PersonLastName,
                PersonSuffix = s.Inmate.Person.PersonSuffix,
                CreatedByPersonSuffix = s.Inmate.Person.PersonSuffix,
                CreatedByPersonFirstName = s.CreateByNavigation.PersonNavigation.PersonFirstName,
                CreatedByPersonMiddleName = s.CreateByNavigation.PersonNavigation.PersonMiddleName,
                CreatedByPersonLastName = s.CreateByNavigation.PersonNavigation.PersonLastName,
                UpdatedOfficerBadgeNumber = s.UpdateByNavigation.OfficerBadgeNumber,
                DeletedOfficerBadgeNumber = s.DeleteByNavigation.OfficerBadgeNumber,
                OfficerBadgeNumber = s.CreateByNavigation.OfficerBadgeNum,
            }).OrderBy(o => o.PersonLastName).ThenBy(t => t.PersonFirstName).ThenBy(t => t.PersonMiddleName).ToList();
            return lstMedicalNotesDetails;
        }

        public List<MedicalNotes> GetMedicalTypes()
        {
            List<MedicalNotes> lstMedicalNoteTypes = _context.Lookup.Where(w => w.LookupType ==
            LookupConstants.MEDNOTETYPE).Select(s => new MedicalNotes
            {
                LookupDescription = s.LookupDescription,
                LookupIndex = s.LookupIndex,
                LookupColor = s.LookupColor
            }).ToList();
            return lstMedicalNoteTypes;
        }

        public async Task<int> InsertUpdateMedicalNotes(MedicalNotes value)
        {
            if (value.InmateMedicalNoteId == 0)
            {
                InmateMedicalNote medicalNoteDetails = new InmateMedicalNote
                {
                    InmateId = value.InmateId,
                    Note = value.Note,
                    MedicalNoteType = value.MedicalNoteType,
                    CreateBy = _personnelId,
                    CreateDate = DateTime.Now
                };
                _context.InmateMedicalNote.Add(medicalNoteDetails);
            }
            else
            {
                InmateMedicalNote medicalNoteDetails = _context.InmateMedicalNote.
                Single(s => s.InmateMedicalNoteId == value.InmateMedicalNoteId);
                medicalNoteDetails.InmateId=value.InmateId;
                medicalNoteDetails.Note = value.Note;
                medicalNoteDetails.MedicalNoteType = value.MedicalNoteType;
                medicalNoteDetails.UpdateBy = _personnelId;
                medicalNoteDetails.UpdateDate = DateTime.Now;
            }
            return await _context.SaveChangesAsync();
        }

        public async Task<int> DeleteUndoMedicalNotes(MedicalNotes value)
        {
            InmateMedicalNote medicalNoteDetails = _context.InmateMedicalNote.
            Single(s => s.InmateMedicalNoteId == value.InmateMedicalNoteId);
            medicalNoteDetails.DeleteBy = _personnelId;
            medicalNoteDetails.DeleteDate = DateTime.Now;
            medicalNoteDetails.DeleteFlag = value.DeleteFlag == 0 ? 1 : default(int?);
            return await _context.SaveChangesAsync();
        }
    }
}