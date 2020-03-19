using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ServerAPI.ViewModels;

// ReSharper disable once CheckNamespace
namespace ServerAPI.Services
{
    public interface IAltSentService
    {
        AltSentRequestDetails LoadAltSentRequestDetails(int facilityId, DateTime? scheduleDate);
        AltSentenceRequest GetAltSentRequestInfo(int facilityId, int? requestId);
        IEnumerable<KeyValuePair<int, string>> GetFacilityList();
        IEnumerable<KeyValuePair<int, string>> GetAltSentProgramList(int facilityId);
        Task<int> InsertUpdateAltSentRequest(AltSentenceRequest altSentenceRequest, int altSentRequestId = 0);
        bool CheckAltSentRequestExists(int inmateId, int facilityId);
        Task<int> DeleteAltSentActiveRequest(int altSentRequestId, string history, bool deleteFlag);
        List<HistoryVm> GetAltSentHistories(int altSentRequestId);
        Task<int> ApproveAltSentRequest(int altSentRequestId, ApproveRequest approveRequest);
        ScheduleDetails GetScheduleDetails(ScheduleDetails scheduleDetails);
        Task<int> ScheduleAltSentRequest(int altSentRequestId, SaveSchedule schedule);
        ApproveRequestDetails GetApproveRequestInfo(int requestId);
        bool CheckAltSentPgmExists(int inmateId);
        bool GetAltSentArrestNotAllowedFlag(int arrestId);
        List<ProgramTypes> LoadProgramList(int inmateId, int altSentId = 0);
        AltSentScheduleVm GetAltSentSchedule(int facilityId, int programId);
        List<AltSentQueue> GetAltSentQueueDetails(int facilityId);
    }
}