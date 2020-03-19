using ServerAPI.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;
namespace ServerAPI.Interfaces
{
    public interface IMiscIssuedPropertyService
    {
        List<IssuedPropertyLookupFacilityVm> GetIssuedPropertyLookup(int facilityId);
        List<IssuedPropertyMethod> GetIssuedProperty(int inmateId);
        Task<int> InsertAndUpdateIssuedProperty(IssuedPropertyMethod objIssued);
        Task<int> DeleteIssuedProperty(List<IssuedPropertyMethod> issued);
        List<HistoryVm> GetIssuedPropertyHistory(int issuedPropertyId);
    }
}
