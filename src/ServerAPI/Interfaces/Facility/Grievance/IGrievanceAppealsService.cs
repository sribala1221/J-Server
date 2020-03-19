using System.Collections.Generic;
using System.Threading.Tasks;
using ServerAPI.ViewModels;

namespace ServerAPI.Services
{
    public interface IGrievanceAppealsService
    {
        GrievanceAppealsVm GetGrievanceAppealDetails(int facilityId, bool showReviewed, bool showDeleted);
        List<GrievanceAppealDetails> GetGrievanceDetails(int? grievanceId);
        Task<int> InsertGrievanceAppeal(GrievanceAppealParam grivAppealParam);
        Task<int> UpdateGrievanceAppeal(int grievanceAppealId, GrievanceAppealParam grivAppealParam);
        Task<int> DeleteGrievanceAppeal(int grievanceAppealId, bool deleteFlag);
        GrievanceAppealDetails GetAppealDetailsForPdf(int grievanceId);
        List<GrievanceAppealSearch> SearchGrievances(string grievanceNumber, int inmateId);
        bool CheckReviewFlagAndClear(int grievanceId, int facilityId, int grievanceAppealId);
        GrievanceAppealsReport GetGrievanceAppealsReport(int grievanceId);
    }
}
