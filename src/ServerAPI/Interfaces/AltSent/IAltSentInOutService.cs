using ServerAPI.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace ServerAPI.Services
{
    public interface IAltSentInOutService
    {
        List<CourtCommitHistoryVm> GetInterviewOrScheduleBook(CourtCommitHistorySearchVm searchValue);
        // ReSharper disable once IdentifierTypo
        CourtCommitHistoryVm GetInterviewOrScheduleBookCompleteDetails(int inmatePrebookId);
        Task<int> UpdateInterviewOrScheduleBookCompleteDetails(CourtCommitHistoryVm courtCommitHistoryVm);
        // ReSharper disable once IdentifierTypo
        CourtCommitHistoryVm GetScheduleAlternativeSentence(int inmatePrebookId);
        Task<int> UpdateScheduleAlternativeSentence(CourtCommitHistoryVm courtCommitHistoryVm);
    }
}
