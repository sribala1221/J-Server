using ServerAPI.ViewModels;
using System.Collections.Generic;

namespace ServerAPI.Services
{
    public interface IHousingConflictService
    {
        List<HousingConflictVm> GetInmateHousingConflictVm(HousingInputVm housingInput);

        List<HousingConflictVm> GetHousingKeepSeparate(int inmateId, int facilityId);

        List<HousingConflictVm> GetHousingConflictVm(HousingInputVm housingInput);

        List<HousingConflictVm> GetHousingRuleAndClassifyFlags(int inmateId, int facilityId);

        List<HousingConflictVm> GetKeepSepDetails(int inmateId, int facilityId, string housingUnitLocation, int housingUnitListId);

        List<HousingConflictVm> GetHousingConflictNotification(HousingInputVm housingInput);

        List<HousingConflictVm> GetRecommendHousingConflictVm(int inmateId, int facilityId);
    }
}
