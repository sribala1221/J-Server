﻿using ServerAPI.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;
using GenerateTables.Models;

// ReSharper disable once CheckNamespace
namespace ServerAPI.Services
{
    public interface IFormsService
    {
        Task<int> SaveForm(FormSaveData value);
        Form GetFormDetails(FormDetail detail);
        IEnumerable<FormHistory> GetFormHistory(int formRecordId);
        FormSavedHistoryObj GetSaveHistoryXml(int formSaveHistoryId);
        Signature GetSignature(int formRecordId, int formTemplateId);
        Task<int> SaveSignature(Signature details);
        Task<int> InsertBookMarkXref(int templateId, string html);
        List<KeyValuePair<string, int>> GetFormTemplateDetails(int categoryId, string filterName);
        List<GetFormTemplates> GetFormTemplates(int categoryId);
        MedicalFormVm GetInmateMedicalForms(int inmateId);
        string GetLastIncarcerationFormRecord(string inmateNumber, int formTemplatesId);
        KeyValuePair<int, int> GetFormRecordByRms(int inmatePrebookStagingId);
        List<GetFormTemplates> GetRequiredForms(FormCategories categoryName, string facilityAbbr, int incarcerationId,
            int arrestId, int inmatePrebookId, bool isOptionalForms, string caseTypeId);

        string[] GetRequiredCaseType(FormCategories categoryName, string facilityAbbr, int incarcerationId,
            int arrestId, int inmatePrebookId, string caseTypeId);
        FacilityFormListVm LoadFacilityFormDetails(int facilityId,int formTemplateId);
        bool CheckFormTemplate(int templateId);
        bool IsPBPCFormsExists(int inmatePrebookId);
        Task<int> SavePdfForm(FormSaveData value);
    }
}