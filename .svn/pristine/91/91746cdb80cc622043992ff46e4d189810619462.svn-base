using System.Collections.Generic;
using ServerAPI.ViewModels;
using System.Threading.Tasks;

namespace ServerAPI.Services
{
    public interface IHousingCellService
    {
        CellViewerDetails GetCellViewerDetails(int facilityId, int housingUnitListId, int housingGroupId);
        CellViewerDetails GetBedNumberDetails(int facilityId, int housingUnitListId, string housingBedNumber);
        List<HousingUnitVm> GetOutOfServiceDetails(int housingUnitListId, int facilityId);
        OutOfServiceHistory GetOutOfServiceHistory(HousingInputVm housingInput);
        Task<int> UpdateOutOfService(HousingUnitVm housingDet);
        CellPropertyLibraryDetails GetPropertyLibraryDetails(int housingUnitListId, int housingGroupId,string housingBedNumber);
    }
}
