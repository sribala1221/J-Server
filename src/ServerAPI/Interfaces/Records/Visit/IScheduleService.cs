using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ServerAPI.ViewModels;

namespace ServerAPI.Interfaces
{
    public interface IScheduleService
    {
        VisitScheduleVm GetVisitScheduleDetails(FacilityApptVm scheduleApptDetails);       

        List<InmateDetailsList> GetVisitationInmateDetails(FacilityApptVm scheduleApptDetails);
    }
}
