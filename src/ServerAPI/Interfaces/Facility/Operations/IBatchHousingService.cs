using System.Threading.Tasks;
using System.Collections.Generic;
using ServerAPI.ViewModels;

namespace ServerAPI.Services
{
    public interface IBatchHousingService
    {
        BatchHousingVm GetBatchHousingDetails(bool externalOnly, int facilityId);
        List<InmateVm> GetBatchHousingInmateDetails(int facilityId, int? locationId, int? housingUnitListId);
        List<HousingCapacityVm> GetHousingBedDetails(int housingUnitListId);
        List<HousingConflictVm> GetHousingConflict(List<HousingInputVm> objHousingInput);
        Task<int> CreateHousingUnitMoveHistory(List<HousingMoveParameter> objHousingMoveParameter);
    }
}
