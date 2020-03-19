using ServerAPI.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ServerAPI.Services
{
    public interface IGrievanceService
    {
        Task<int> InsertUpdateGrievance(GrievanceVm grievance);
        GrievanceCountVm GetGrievanceCount(GrievanceCountVm grievanceCountVm);
        List<GrievanceCountDetails> GetGrievanceCountPopupDetails(GrievanceCountVm grievanceCountVm);
        GrievanceDetailsVm GetGrievanceDetails(GrievanceInputs inputs);
        GrievanceHistoryDetails GetGrievanceHistory(GrievanceInputs inputs);
        Task<int> InsertAdditionalInmate(GrievanceVm grievance);
        Task<int> DeleteAdditionalInmate(int grievanceInmateId);
        Task<int> InsertAdditionalPersonnel(GrievanceVm grievance);
        Task<int> DeleteAdditionalPersonnel(int grievancePersonnelId);
        Task<int> UpdateGrievanceDepartment(GrievanceVm grievance);
        Task<int> UpdateGrievanceReview(GrievanceReview review);
        GrievanceDetails GetAdditionalDetails(int grievanceId);
        List<IncarcerationForms> GetGrievanceForms(int grievanceId);
        GrievanceHousingDetails GetHousingDetails(int facilityId);
        Task<int> UpdateGrvSetReview(GrievanceVm grvcDetail);
        List<PrebookAttachment> LoadGrievanceAttachments(int grievanceId);
        Task<int> DeleteUndoByGrievanceId(GrievanceVm grvcDetail);
       // Task<int> DeleteGrievance(int grievanceId, string disposition, string dispositionNote);
        Task<int> DeleteGrievance(CancelGrievance CancelGrievance);
        GrievanceReport GetGrievanceReport(int grievanceId);
        GrievanceVm GetGrievance(int grievanceId);
    }
}
