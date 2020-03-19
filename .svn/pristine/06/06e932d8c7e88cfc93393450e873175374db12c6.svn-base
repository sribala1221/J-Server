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
    public class MedicalPreScreenService : IMedicalPreScreenService
    {
        private readonly AAtims _context;
        private readonly int _personnelId;

        public MedicalPreScreenService(AAtims context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _personnelId = Convert.ToInt32(httpContextAccessor.HttpContext.User
                .FindFirst(AuthDetailConstants.PERSONNELID)?.Value);
        }

        public List<MedicalPrescreenVm> GetMedicalPrescreen(MedicalPrescreenVm medicalPreSearch)
        {
            List<MedicalPrescreenVm> lstMedicalPreScreenVm = _context.FormRecord
                .Where(f => f.MedInmatePrebookId > 0
                            && f.FormTemplates.FormCategoryId == (int?) FormCategories.MedicalPrescreening
                            && (!medicalPreSearch.FromDate.HasValue ||
                                (f.CreateDate.Value.Date >= medicalPreSearch.FromDate.Value.Date 
                                && f.CreateDate.Value.Date <= medicalPreSearch.ToDate.Value.Date))
                            && (medicalPreSearch.PersonnelId == 0 || f.CreateBy == medicalPreSearch.PersonnelId)
                            && (medicalPreSearch.DeleteFlag || f.DeleteFlag == 0))
                .Select(s => new MedicalPrescreenVm
                {
                    MedInmatePreBookId = s.MedInmatePrebookId,
                    MedScreenArrestId = s.MedScreenArrestId,
                    FormRecordId = s.FormRecordId,
                    DeleteFlag = s.DeleteFlag == 1,
                    Createdate = s.CreateDate,
                    FormDate = s.CreateDate,
                    Updatedate = s.UpdateDate,
                    InmateId = s.InmateId,
                    DisplayName = s.FormTemplates.DisplayName,
                    CreatedBy = s.CreateBy,
                    UpdatedBy = s.UpdateBy,
                    FormNotes = s.FormNotes
                }).OrderByDescending(o => o.FormDate).ToList();

            int[] inmatePrebookiIds = lstMedicalPreScreenVm.Select(a => a.MedInmatePreBookId ?? 0).ToArray();

            List<InmatePrebook> lstInmatePrebooks = _context.InmatePrebook
                .Where(a => inmatePrebookiIds.Contains(a.InmatePrebookId)
                && (medicalPreSearch.DeleteFlag || a.DeleteFlag == 0 || !a.DeleteFlag.HasValue))
                .ToList();

            int[] statusByIds = lstInmatePrebooks.Select(i => i.MedPrescreenStatusBy ?? 0).ToArray();
            List<PersonnelVm> lstStatusByPersonnels = _context.Personnel.Where(a => statusByIds.Contains(a.PersonnelId))
                .Select(a => new PersonnelVm
                {
                    PersonLastName = a.PersonNavigation.PersonLastName,
                    PersonFirstName = a.PersonNavigation.PersonFirstName,
                    PersonMiddleName = a.PersonNavigation.PersonMiddleName,
                    PersonnelId = a.PersonnelId,
                    PersonnelNumber = a.OfficerBadgeNum
                }).ToList();

            lstMedicalPreScreenVm.ForEach(item =>
            {
                InmatePrebook inmatePrebook = lstInmatePrebooks
                    .SingleOrDefault(a => a.InmatePrebookId == item.MedInmatePreBookId);
                if (inmatePrebook != null)
                {
                    item.IncarcerationId = inmatePrebook.IncarcerationId;
                    item.PreBookInfo = new PersonInfoVm
                    {
                        PersonFirstName = inmatePrebook.PersonFirstName,
                        PersonMiddleName = inmatePrebook.PersonMiddleName,
                        PersonLastName = inmatePrebook.PersonLastName,
                        PersonDob = inmatePrebook.PersonDob
                    };
                    item.TempHoldFlag = inmatePrebook.TemporaryHold == 1;
                    item.TempHoldId = inmatePrebook.TempHoldId;
                    item.PrebookNumber = inmatePrebook.PreBookNumber;
                    item.StatusFlag = inmatePrebook.MedPrescreenStatusFlag;
                    item.StatusDate = inmatePrebook.MedPrescreenStatusDate;
                    item.PrebookDeleteFlag = inmatePrebook.DeleteFlag == 1;
                    item.StatusBy = inmatePrebook.MedPrescreenStatusBy;
                    if (item.StatusBy > 0)
                    {
                        PersonnelVm statusBy = lstStatusByPersonnels
                            .SingleOrDefault(x => x.PersonnelId == item.StatusBy);
                        item.StatusByInfo = statusBy;
                    }
                    item.FacilityId = inmatePrebook.FacilityId;
                }
            });

            lstMedicalPreScreenVm = lstMedicalPreScreenVm.Where(f =>
                (medicalPreSearch.FacilityId == 0 || f.FacilityId == medicalPreSearch.FacilityId) && 
                (!medicalPreSearch.TempHoldFlag || f.TempHoldId > 0 && f.TempHoldFlag)
                && (medicalPreSearch.DeleteFlag || !f.PrebookDeleteFlag)
                && (string.IsNullOrEmpty(medicalPreSearch.PrebookNumber)
                    || f.PrebookNumber.ToLower().StartsWith(medicalPreSearch.PrebookNumber.ToLower()))
            ).ToList();

            int[] incarcerationIds = lstMedicalPreScreenVm.Where(f => f.IncarcerationId.HasValue)
                .Select(i => i.IncarcerationId ?? 0).ToArray();
            List<KeyValuePair<int, int>> lstKeyValuePairs = _context.Incarceration
                .Where(i => incarcerationIds.Contains(i.IncarcerationId))
                .Select(i => new KeyValuePair<int, int>(i.IncarcerationId, i.InmateId ?? 0)).ToList();

            int[] inmateIds = lstKeyValuePairs.Select(a => a.Value).ToArray();
            List<Inmate> lstInmates = _context.Inmate.Where(i =>
                    inmateIds.Contains(i.InmateId))
                .Select(a => new Inmate
                {
                    InmateId = a.InmateId,
                    InmateNumber = a.InmateNumber,
                    PersonId = a.PersonId,
                    InmateActive = a.InmateActive
                }).ToList();

            int[] personIds = lstInmates.Select(a => a.PersonId).ToArray();
            List<PersonInfoVm> lstPersonInfoVms = _context.Person.Where(i =>
                    personIds.Contains(i.PersonId))
                .Select(s => new PersonInfoVm
                {
                    PersonId = s.PersonId,
                    PersonLastName = s.PersonLastName,
                    PersonFirstName = s.PersonFirstName,
                    PersonMiddleName = s.PersonMiddleName,
                    PersonDob = s.PersonDob
                }).ToList();

            lstPersonInfoVms.ForEach(item =>
            {
                Inmate inmate = lstInmates.FirstOrDefault(a => a.PersonId == item.PersonId);
                if (inmate != null)
                {
                    item.InmateId = inmate.InmateId;
                    item.InmateNumber = inmate.InmateNumber;
                    item.InmateActive = inmate.InmateActive == 1;
                }
            });

            lstMedicalPreScreenVm.ForEach(item =>
            {
                item.PersonInfo = new PersonInfoVm();
                KeyValuePair<int, int> keyValuePair = lstKeyValuePairs.Find(a => a.Key == item.IncarcerationId);
                if (keyValuePair.Key > 0)
                {
                    PersonInfoVm personInfoVm = lstPersonInfoVms.SingleOrDefault(p => p.InmateId == keyValuePair.Value);
                    if(personInfoVm != null) item.PersonInfo = personInfoVm;
                    else item.PersonInfo = new PersonInfoVm();
                }
            });

            if (medicalPreSearch.ActiveFlag
                || !string.IsNullOrEmpty(medicalPreSearch.PersonLastName)
                || !string.IsNullOrEmpty(medicalPreSearch.PersonFirstName)
                || !string.IsNullOrEmpty(medicalPreSearch.PersonMiddleName)
                || !string.IsNullOrEmpty(medicalPreSearch.PersonDob.ToString())
                || medicalPreSearch.InmateId > 0)
            {
                lstMedicalPreScreenVm = lstMedicalPreScreenVm.Where(f=> f.PersonInfo.PersonId > 0).ToList();
                lstMedicalPreScreenVm = lstMedicalPreScreenVm.Where(f =>
                        (!medicalPreSearch.ActiveFlag || f.PersonInfo.InmateActive)
                        && (!medicalPreSearch.InmateId.HasValue || f.PersonInfo.InmateId == medicalPreSearch.InmateId)
                        && (string.IsNullOrEmpty(medicalPreSearch.PersonLastName) ||
                            (f.PersonInfo.PersonLastName.ToLower().StartsWith(medicalPreSearch.PersonLastName.ToLower()) ||
                             f.PreBookInfo.PersonLastName.ToLower().StartsWith(medicalPreSearch.PersonLastName.ToLower())))
                        && (string.IsNullOrEmpty(medicalPreSearch.PersonFirstName) ||
                            (f.PersonInfo.PersonLastName.ToLower().StartsWith(medicalPreSearch.PersonFirstName.ToLower()) ||
                             f.PreBookInfo.PersonFirstName.ToLower().StartsWith(medicalPreSearch.PersonFirstName.ToLower())))
                        && (string.IsNullOrEmpty(medicalPreSearch.PersonMiddleName) ||
                            (f.PersonInfo.PersonLastName.ToLower().StartsWith(medicalPreSearch.PersonMiddleName.ToLower()) ||
                             f.PreBookInfo.PersonFirstName.ToLower().StartsWith(medicalPreSearch.PersonMiddleName.ToLower())))
                        && (!medicalPreSearch.PersonDob.HasValue ||
                            (f.PersonInfo.PersonDob == medicalPreSearch.PersonDob ||
                             f.PreBookInfo.PersonDob == medicalPreSearch.PersonDob)))
                    .ToList();
            }

            if (medicalPreSearch.TopSearch > 0)
            {
                lstMedicalPreScreenVm = lstMedicalPreScreenVm.Take(medicalPreSearch.TopSearch).ToList();
            }

            int[] personnelIds = lstMedicalPreScreenVm.Select(i => new[] {i.CreatedBy, i.UpdatedBy})
                .SelectMany(i => i).Where(i => i.HasValue)
                .Select(i => i.Value).ToArray();
            List<PersonnelVm> lstPersonnels = _context.Personnel.Where(a => personnelIds.Contains(a.PersonnelId))
                .Select(a => new PersonnelVm
                {
                    PersonLastName = a.PersonNavigation.PersonLastName,
                    PersonFirstName = a.PersonNavigation.PersonFirstName,
                    PersonMiddleName = a.PersonNavigation.PersonMiddleName,
                    PersonnelId = a.PersonnelId,
                    PersonnelNumber = a.OfficerBadgeNum
                }).ToList();

            lstMedicalPreScreenVm.ForEach(item =>
            {
                PersonnelVm createdBy = lstPersonnels.SingleOrDefault(x => x.PersonnelId == item.CreatedBy);
                item.CreatedByInfo = createdBy;

                PersonnelVm updatedBy = lstPersonnels.SingleOrDefault(x => x.PersonnelId == item.UpdatedBy);
                item.UpdatedByInfo = updatedBy;
            });

            return lstMedicalPreScreenVm;
        }

        public async Task<int> UpdateMedicalPreScreenFormRecord(int formRecordId)
        {
            FormRecord formRecord = _context.FormRecord.Find(formRecordId);                       
            FormRecordSaveHistory formRecordSaveHistory = new FormRecordSaveHistory
            {
                FormRecordId = formRecord.FormRecordId,
                Sig = formRecord.Sig,
                Sig2 = formRecord.Sig2,
                Sig3 = formRecord.Sig3,
                Sig4 = formRecord.Sig4,
                Sig5 = formRecord.Sig5,
                Sig6 = formRecord.Sig6,
                Sig7 = formRecord.Sig7,
                FormNotes = formRecord.FormNotes,
                XmlData = formRecord.XmlData
            };
            _context.FormRecordSaveHistory.Add(formRecordSaveHistory);
            return await _context.SaveChangesAsync();
        }

        public int DeleteMedicalPrescreen(int formRecordId, bool deleteflag)
        {
            FormRecord formRecord = _context.FormRecord.Find(formRecordId);
            formRecord.DeleteFlag = deleteflag ? 1 : 0;
            formRecord.DeleteDate = deleteflag ? (DateTime?)DateTime.Now : null;
            formRecord.DeleteBy = deleteflag ? (int?)_personnelId : null;            
            return _context.SaveChanges();
        }
    }
}