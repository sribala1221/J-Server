using ServerAPI.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ServerAPI.Services
{
    public interface IIntakePrebookService
    {
        PrebookCountVm GetIntakePrebookCount(int facilityId, int inmatePrebookId, string queueName, bool active);
        List<InmatePrebookVm> GetListPrebookDetails(int facilityId, PrebookDetails prebookSelected, string courtcommitoverdueDate);
        Task<int> DeleteInmatePrebook(int inmatePrebookId);
        Task<int> UndoInmatePrebook(int inmatePrebookId);
        Task<int> UpdateTemporaryHold(int inmatePrebookId);
        Task<int> RemoveTemporaryHold(int inmatePrebookId);
        InmatePrebookVm GetInmatePrebookDetails(int inmatePrebookId);
        Task<int> UpdateInmatePrebookPersonDetails(int inmatePrebookId, int personId);
        Task<int> UpdateMedicalPrescreenPrebook(int inmatePrebookId);
        Task<int> InsertIntakeProcess(IntakeEntryVm value);
        Task<int> SetIntakeProcessComplete(int incarcerationId);
        List<InmatePrebookCaseVm> GetIntakePrebookCase(int inmatePrebookId);
        Task<int> SetIntakeProcessUndoComplete(int incarcerationId);
        Task<int> ApprovePrebook(InmatePrebookReviewVm imatePrebookReview);
        Task<int> SetIdentityAccept(InmatePrebookVm inmatePrebook);
        //  int GetSupplementalFacilityDetails(int inmateId);
        Task<int> UpdateByPassedAndNotRequiredMedical(int inmatePrebookId, int medPrescreenStatusFlag);
        KeyValuePair<int, int> GetPrebookChargesAndWarrantCount(int incarcerationId, int arrestId);        
    }
}