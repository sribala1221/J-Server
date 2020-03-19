using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using ServerAPI.ViewModels;
using GenerateTables.Models;
using ServerAPI.Utilities;

// ReSharper disable once CheckNamespace
namespace ServerAPI.Services
{
    public class MedicalFormsService : IMedicalFormsService
    {
        private readonly AAtims _context;
        private readonly int _personnelId;

        public MedicalFormsService(AAtims context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _personnelId = Convert.ToInt32(httpContextAccessor.HttpContext.User
                .FindFirst(AuthDetailConstants.PERSONNELID)?.Value);
        }

        public MedicalFormVm GetMedicalCategory(MedicalFormsParam inputs)
        {
            MedicalFormVm medForm = new MedicalFormVm
            {
                lstGetFormTemplates = _context.FormTemplates.Where(fTemp =>
                    fTemp.FormCategoryId == 4 && fTemp.Inactive != 1).Select(fTemp =>
                    new GetFormTemplates
                    {
                        FormTemplatesId = fTemp.FormTemplatesId,
                        DisplayName = fTemp.DisplayName,
                        HtmlFileName = fTemp.HtmlFileName,
                        DeleteFlag = fTemp.Inactive == 1
                    }).ToList()
            };

            medForm.lstMedicalForms = GetMedicalFormsDetails(inputs, medForm);

            return medForm;
        }

        public List<MedicalForms> GetMedicalFormsDetails(MedicalFormsParam inputs, MedicalFormVm medForm)
        {
            int[] templatesId = medForm.lstGetFormTemplates.Select(tem => tem.FormTemplatesId).ToArray();

            if (inputs.DateTo.HasValue)
            {
                inputs.DateTo = inputs.DateTo.Value.AddDays(1);
            }

            List<MedicalForms> lstMedicalFormsDetails = _context.FormRecord
                .Where(w =>
                    (!inputs.InmateId.HasValue || w.InmateId == inputs.InmateId) &&
                    (!inputs.DateFrom.HasValue || w.CreateDate >= inputs.DateFrom) &&
                    (!inputs.DateTo.HasValue || w.CreateDate < inputs.DateTo) &&
                    (inputs.PersonnelId == null || w.CreateBy == inputs.PersonnelId) &&
                    templatesId.Contains(w.FormTemplatesId))
                .Select(s => new MedicalForms
                {
                    FormRecordId = s.FormRecordId,
                    FormTemplateId = s.FormTemplatesId,
                    CreateDate = s.CreateDate.Value,
                    DisplayName = s.FormTemplates.DisplayName,
                    FormNotes = s.FormNotes,
                    InmateDetails = new InmateHousing
                    {
                        InmateId = s.InmateId ?? 0,
                        PersonId = s.InmateId.HasValue ? s.Inmate.PersonId : 0,
                        PersonLastName = s.InmateId.HasValue ? s.Inmate.Person.PersonLastName : null,
                        PersonFirstName = s.InmateId.HasValue ? s.Inmate.Person.PersonFirstName : null,
                        PersonMiddleName = s.InmateId.HasValue ? s.Inmate.Person.PersonMiddleName : null,
                        InmateNumber = s.InmateId.HasValue ? s.Inmate.InmateNumber : null,
                        InmateActive = s.InmateId.HasValue && s.Inmate.InmateActive == 1,
                    },
                    CreateBy = new PersonnelVm
                    {
                        PersonLastName = s.CreateByNavigation.PersonNavigation.PersonLastName,
                        PersonFirstName = s.CreateByNavigation.PersonNavigation.PersonFirstName,
                        PersonMiddleName = s.CreateByNavigation.PersonNavigation.PersonMiddleName,
                        OfficerBadgeNumber = s.CreateByNavigation.OfficerBadgeNum
                    },
                    DeleteFlag = s.DeleteFlag == 1
                }).ToList();

            return lstMedicalFormsDetails;
        }

        public async Task<int> InsertFormRecord(MedicalFormsVm medicalForms)
        {
            FormRecord formRecord = new FormRecord
            {
                MedScreenIncarcerationId = medicalForms.IncarcerationId,
                InmateId = medicalForms.InmateId,
                FormTemplatesId = medicalForms.FormTemplatesId,
                FormNotes = medicalForms.FormNotes,
                XmlData = medicalForms.XmlData
            };

            _context.FormRecord.Add(formRecord);

            return await _context.SaveChangesAsync();
        }

        public async Task<int> UpdateFormRecord(int formRecordId, MedicalFormsVm medicalForms)
        {
            FormRecord formRecord = _context.FormRecord.Single(s => s.FormRecordId == formRecordId);

            formRecord.UpdateDate = DateTime.Now;
            formRecord.UpdateBy = _personnelId;
            formRecord.FormNotes = medicalForms.FormNotes;
            formRecord.XmlData = medicalForms.XmlData;

            return await _context.SaveChangesAsync();
        }

        public async Task<MedicalFormVm> DeleteUndoFormRecord(MedicalFormsParam inputs)
        {
            FormRecord formRecord = _context.FormRecord.Single(s => s.FormRecordId == inputs.FormRecordId);

            DateTime? deleteDate = DateTime.Now;
            formRecord.DeleteFlag = formRecord.DeleteFlag == 1 ? 0 : 1;
            formRecord.DeleteDate = formRecord.DeleteFlag == 1 ? null : deleteDate;
            formRecord.DeleteBy = formRecord.DeleteFlag == 1 ? new int() : _personnelId;
            await _context.SaveChangesAsync();
            return GetMedicalCategory(inputs);
        }
    }
}
