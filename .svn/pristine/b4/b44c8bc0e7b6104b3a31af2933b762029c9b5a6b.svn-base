using ServerAPI.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ServerAPI.Services
{
    public interface IInmateService
    {
        InmateNoteVm GetInmateNote(int facilityId, int inmateId);
        Task<int> InsertInmateNote(FloorNotesVm value);
        Task<int> UpdateInmateNote(FloorNotesVm value);
        Task<int> DeleteInmateNote(FloorNotesVm value);
        FloorNotesVm GetInmateNoteEdit(int floorNoteId);
        List<InmateSearchVm> GetInmateSearchDetails(InmateSearchVm inmate); 
        List<BookingSearchVm> GetBookingNumber(string searchText, int inmateActive);
        Task<InmateVm> InsertUpdateInmate(InmateVm inmate);
        void CreateTask(int inmateId, TaskValidateType eventName);
        List<KeyValuePair<int, string>> GetInmateTasks(int inmateId, TaskValidateType validateType);
        List<PersonVm> GetInmateDetails(List<int> inmateId);
        bool IsActiveInmate(int inmateId, int juvenileFlag);
        List<InmateSearchVm> GetDashboardInmateSearchDetails(InmateSearchVm inmate);
    }
}
