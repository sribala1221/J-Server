using ServerAPI.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServerAPI.Services
{
    public interface IStatusBoardOverviewService
    {
        StatusBoardOveriviewVm GetStatusBoardOverview(int facilityId);
        List<MonitorPreScreenVm> GetMonitorPreScreenDetail(OverviewDetailInputVm value);
        List<HousingStatsDetails> GetInmateDetail(OverviewDetailInputVm value);
        List<MonitorGrivanceVm> GetGrivanceDetail(OverviewDetailInputVm value);
        List<IncidentDescription> GetIncidentDetail(OverviewDetailInputVm value);
        List<SupervisiorReviewVm> GetSupervisorReviewDetail(OverviewDetailInputVm value);

        StatusBoardOveriviewVm GetPreScreeningCount(int facilityId);

        StatusBoardOveriviewVm GetWizardSentenceCount(int facilityId);

        StatusBoardOveriviewVm GetInmateCount(int facilityId);

        StatusBoardOveriviewVm GetIncidentCount(int facilityId);

        StatusBoardOveriviewVm GetStatsCount(int facilityId);

        StatusBoardOveriviewVm GetGrievanceCount(int facilityId);
    }
}
