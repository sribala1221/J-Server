using System.Collections.Generic;
using ServerAPI.ViewModels;
using System.Threading.Tasks;

namespace ServerAPI.Services
{
    public interface IMedicalNotesService
    {        
        MedicalNotesVm GetMedicalNotes(MedicalNotes medicalNoteSearch); 
        List<MedicalNotes> GetMedicalTypes();       
        Task<int> InsertUpdateMedicalNotes(MedicalNotes value);
        Task<int> DeleteUndoMedicalNotes(MedicalNotes value);
    }
}