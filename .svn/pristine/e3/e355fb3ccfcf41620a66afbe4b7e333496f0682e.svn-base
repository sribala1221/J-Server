using GenerateTables.Models;
using ServerAPI.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServerAPI.Services
{
    public interface IHousingService
    {
        InmateHousingVm GetInmateHousingDetails(int inmateId);

        HousingVm GetFacilityHousingDetails(int inmateId, int facilityId);        

        HousingVm GetHousingDetails(HousingInputVm housingInput);

        HousingVm GetRecommendHousing(int facilityId, int inmateId);

        HousingVm GetHousingFacility(int facilityId);

        HousingVm GetFacilityLocationDetails(int locationId, int facilityId);

        List<InmateSearchVm> GetHousingInmateHistory(HousingInputVm value);

        HousingVm GetNoHousingDetails(int facilityId);

        HousingVm GetBuildingDetails(int facilityId);

        Task<int> InsertHousingAssign(HousingAssignVm value);

        HousingStatsVm LoadHousingStatsCount(IQueryable<Inmate> inmateList);

        HousingVm GetLocationInmateDetails(HousingInputVm housingInput);

        HousingVm GetFacilityInmateDetails(int facilityId);
        HousingVm GetPodHousingDetails(HousingInputVm housingInput);

        HousingVm GetHousingFacilityDetailsForFacility(int facilityId);
        HousingVm GetHousingFacilityDetailsForBuilding(int facilityId);
        HousingVm GetHousingFacilityDetailsForInmates(int facilityId);
        HousingVm GetHousingFacilityDetailsForInternalExternalLocation(int facilityId);

        HousingVm GetHousingData(int housingUnitListId);

        bool GetHousingNextLevel(int housingUnitListId);

        HousingVm GetHousingNumberDetails(HousingInputVm housingInput);

        InmateCurrentDetails GetInmateCurrentDetails(int inmateId);

    }
}
