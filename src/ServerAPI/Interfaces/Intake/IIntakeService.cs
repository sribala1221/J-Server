using System.Collections.Generic;
using System.Threading.Tasks;
using ServerAPI.ViewModels;

namespace ServerAPI.Services
{
    public interface IIntakeService
    {
        IntakeInmate GetIntake(int facilityId, bool isShowAll);
        IntakeVm GetInmateIdFromPrebook(int inmatePrebookId);
        List<InmatePrebookStagingVm> GetInmatePrebookStaging(InmatePrebookStagingVm obj);
        PersonInfoVm GetPersonByRms(string sid);
        string InsertRmsChargesAndWarrants(int inmatePrebookStagingId, int inmatePrebookId, int personId, int facilityId);
        Task<int> CompleteRmsPrebook(int inmatePrebookStagingId, int inmatePrebookId);
        Task<int> DeleteInmatePrebookStaging(int inmatePrebookStagingId);
    }
}
