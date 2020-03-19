using System.Collections.Generic;
using ServerAPI.ViewModels;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace ServerAPI.Services
{
    public interface IFacilityOperationsService
    {
        RosterMaster GetRosterMasterDetails(RosterFilters rosterFilters);
        List<RosterInmateInfo> GetFilterRoster(RosterFilters rosterFilters);
        List<RosterInmateInfo> getAllRosterInmate(RosterFilters rosterFilters);
        List<FormTemplate> GetPersonFormTemplates();
        PrintOverLay GetRosterOverlay(PrintOverLay printOverLay);
        List<InmateBookings> GetInmateBookings(int[] inmateId);
        Task<IActionResult> PrintOverlayReport(string ids, int templateId);
    }
}
