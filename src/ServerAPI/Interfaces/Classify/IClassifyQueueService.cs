using ServerAPI.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ServerAPI.Services
{
    public interface IClassifyQueueService
    {
        ClassifyQueueVm GetClassifyQueue(int facilityId);
        List<QueueDetails> GetQueueDetails(QueueInputs inputs);
        List<SpecialQueueHistory> ManageSpecialQueueHistory(SpecialQueueInputs inputs);
        Task<int> InsertSpecialClassQueue(SpecialQueueInputs inputs);
        Task<int> InsertClassificationNarrative(QueueInputs inputs);
        List<QueueReviewDetails> GetReview(string siteOption,int facilityId);
        KeyValuePair<int, int> GetSpecialQueue(int facilityId);
        List<QueueDetails> GetQueueDetailsFromFacilityId(int facilityId);
    }
}
