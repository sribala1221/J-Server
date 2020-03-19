using GenerateTables.Models;
using ServerAPI.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace ServerAPI.Interfaces
{
    public interface IVisitorsService
    {
        List<VisitorsVisitDetails> GetVisitorsVisitDetails(VisitParam objVisitParam);
        List<InmateAssignmentDetails> GetInmateAssignment(int visitorId);
        Task<int> InsertInmateVisitorDetails(InmateAssignmentDetails objParam);
        List<OpenScheduleDetails> GetOpenScheduleDetails(OpenScheduleDetails paramList);
        List<BumpedVisitList> GetBumpedVisitList(BumpedVisitList paramList);
        Task<int> ClearBumpedVisit(ClearBumpedVisit clearBumpedVisit);
        Task<RevisitReturnParams> InsertRevisitVisitorDetails(KeyValuePair<int, int> previousScheduleDetails);
        int DeleteUndoVisitorInmate(int VisitorinmateId, bool DeleteFlag);
        int DeleteUndoVisitors(int scheduleId, bool DeleteFlag);
    }
}
