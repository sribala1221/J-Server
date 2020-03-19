using System.Collections.Generic;
using System.Threading.Tasks;
using ServerAPI.ViewModels;

namespace ServerAPI.Services
{
    public interface IMedicalFormsService
    {
        MedicalFormVm GetMedicalCategory(MedicalFormsParam inputs);
        Task<int> InsertFormRecord(MedicalFormsVm medicalForms);
        Task<int> UpdateFormRecord(int formRecordId, MedicalFormsVm medicalForms);
        Task<MedicalFormVm> DeleteUndoFormRecord(MedicalFormsParam inputs);
    }
}
