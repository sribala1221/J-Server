using System.Collections.Generic;
using ServerAPI.ViewModels;
using System.Threading.Tasks;
using System;

namespace ServerAPI.Services
{
    public interface IFacilityHeadCountService
    {
        HeadCountViewerDetails LoadHeadCountMaster(HeadCountFilter headCountFilter);

        List<HeadCountDetail> LoadHeadCountReconcile(int facilityId, int cellLogHeadcountId = 0,
            int housingUnitListId = 0, int cellLogLocationId = 0, bool checkClearBy = false);

        CellHeadCountDetails LoadCellHeadCountMaster(int facilityId, int cellLogId, int housingUnitListId = 0,
            int cellLogLocationId = 0);

        Task<int> InsertOrUpdateCellLog(CellHeadCount cellHeadCount);
        List<HistoryVm> LoadHeadCountSaveHistories(int cellLogId);

        HeadCountDetail GetActiveCellHeadCountString(int facilityId, int housingUnitListId = 0,
            int cellLogLocationId = 0, int housingGroupId = 0, int cellLogId = 0);

        HeadCountViewDetail LoadHeadCountViewDetail(int cellLogHeadCountId);
        Task<int> UpdateHeadCountClear(HeadCountViewDetail headCountViewDetail);
        Task<int> InsertCellLogHeadCount(HeadCountStart headCountStart);
        HeadCountHistoryDetails LoadHeadCountHistory(HeadCountHistoryDetails headCountHistoryMaster);
        List<CellHousingCount> LoadAdminHousingLocationCount(int facilityId);
        HeadCountReport LoadHeadCountReport(HeadCountFilter headCountFilter);
        List<KeyValuePair<int, string>> GetlocationList(int facilityId);
    }
}
