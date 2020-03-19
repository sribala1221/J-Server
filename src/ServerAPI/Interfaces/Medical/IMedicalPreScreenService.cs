using System.Collections.Generic;
using ServerAPI.ViewModels;
using System.Threading.Tasks;

namespace ServerAPI.Services
{
    public interface IMedicalPreScreenService
    {
        List<MedicalPrescreenVm> GetMedicalPrescreen(MedicalPrescreenVm medicalPreSearch);
        Task<int> UpdateMedicalPreScreenFormRecord(int formRecordId);
        int DeleteMedicalPrescreen(int formRecordId, bool deleteflag);
    }
}