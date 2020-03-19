using ServerAPI.ViewModels;
using System.Threading.Tasks;

namespace ServerAPI.Services
{
    public interface IClassCountService
    {
        ClassCountHousing GetHousing(int facilityId);
        HousingDetails GetHousingCountDetails(ClassCountInputs countInputs);
        Task<int> InsertFloorNote(FloorNotesVm values);
    }
}
