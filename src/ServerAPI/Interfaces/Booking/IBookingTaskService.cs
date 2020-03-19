using System;
using System.Collections.Generic;
using ServerAPI.ViewModels;
using System.Threading.Tasks;

namespace ServerAPI.Services
{
    public interface IBookingTaskService
    {

        List<TaskOverview> GetTaskInmates(int taskLookupId, int facilityId);
        Task<int> AssignTaskAsync(TaskOverview taskOverview);
        List<TaskOverview> GetInmateAllTasks(int inmateId, int taskLookupId = 0);
        Task<int> UpdateTaskPriority(TaskOverview taskOverview);
        AoTaskLookupVm GetCompleteTask(int aoTaskLookupId);
        int SaveNoKeeperValues(NoKeeperHistory noKeeperDetails);
        int UpdateCompleteTasks(TasksCountVm taskDetails);
        List<KeyValuePair<int, string>> GetAllTasks(int facilityId);
        KeeperNoKeeperDetails GetKeeperNoKeeper(int incarcerationId);
        BookingOverviewVm GetAssessmentDetails(int facilityId);
        Task<int> UpdateAssessmentComplete(BookingComplete assessmentComplete);
    }
}
