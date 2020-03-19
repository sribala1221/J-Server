using ServerAPI.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ServerAPI.Services {
    public interface IPREAService
    {
        PreaDetailsVm GetClassFileDetails(int personId, int inmateId);
        PreaQueueDetailsVm GetPreaQueue(int facilityId);
        List<QueueDetailsVm> GetQueueDetails(PreaQueueSearch input);
        Task<int> InsertPreaReview(PreaReviewVm review);
        Task<int> InsertPreaNotes(PreaNotesVm notes);
        Task<int> UpdatePreaNotes(PreaNotesVm review);
        Task<int> UpdatePreaReview(PreaReviewVm notes);
        Task<AssocAlertValidation> InsertUpdateAssocDetails(PersonClassificationDetails personAssocDetails);
        Task<AssocAlertValidation> DeleteAssocDetails(PersonClassificationDetails personAssocDetails);
    }
}
