using System.Collections.Generic;
using ServerAPI.ViewModels;

namespace ServerAPI.Services
{
    public interface IFacilityAppointmentService
    {
        List<AppointmentOverallList> GetAppointmentList(FacilityApptVm value);
        AppointmentFilterVm LoadApptFilterlist(int facilityId);
    }
}
