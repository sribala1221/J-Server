using ServerAPI.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ServerAPI.Services
{
    public interface ICourtCommitService
    {
        List<CourtCommitHistoryVm> CourtCommitHistoryDetails(CourtCommitHistorySearchVm Searchvalue);
        Task<int> CourtCommitUpdateDelete(int inmatePreBookId);
        CourtCommitAgencyVm CourtCommitSentDetails(int incarcerationId, int arrestId,int inmateId,int inmatePrebookId);
        AoWizardFacilityVm GetIncidentWizard();
        Task<int> WizardComplete(CourtCommitHistoryVm courtCommitVm);
    }
}
