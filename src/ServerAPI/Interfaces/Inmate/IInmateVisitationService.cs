using ServerAPI.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ServerAPI.Services
{
    public interface IInmateVisitationService
    {
        VisitorListVm GetDefaultVisitorInfo(SearchVisitorList searchVisitorList);

        VisitorListVm GetVisitorList(SearchVisitorList searchVisitorList);

        List<VisitorInfo> GetVisitationSearchList(SearchVisitorList searchVisitorList);

        List<AssignedInmateList> GetAssignedInmateList(int visitorListId,bool isActiveInmate);

        Task<int> InsertUpdatePersonDetails(PersonalVisitorDetails personalVisitor);

        Task<bool> InsertInmateToVisitor(InmateToVisitorInfo inmateToVisitorInfo);

        List<KeyValuePair<int, string>> GetRejectReason();

        List<KeyValuePair<int, string>> GetPersonOfInterest();

        PersonalVisitorDetails GetPersonalVisitorDetails(int visitorListId);

        Task<int> DeletePersonalVisitorDetails(int visitorListId);

        Task<int> UndoPersonalVisitorDetails(int visitorListId);

        List<VisitorListSaveHistory> GetVisitorListSaveHistory(int visitorListId);

        HistoryList GetHistoryList(SearchVisitorHistoryList searchHistoryList);

        List<HistoryInfo> GetVisitationHistorySearchList(SearchVisitorHistoryList searchHistoryList);

        List<VisitationHistory> GetVisitorHistory(int inmateId,int personId);

        Task<int> DeleteAssignedInmateDetails(int visitorListId);

        Task<int> UndoAssignedInmateDetails(int visitorListId);

        Task<int> DeleteVisitationHistory(int visitorId);

        List<VisitorListSaveHistory> GetVisitorAssignedListSaveHistory(int visitorId);

        InmateToVisitorInfo GetInmateVisitorDetails(int visitorId);

        List<RejectInmateHistory> GetVisitorRejectHistoryList(int visitorId, VisitorRejectFlag visitorFlag, int visitorListId);
    }
}
