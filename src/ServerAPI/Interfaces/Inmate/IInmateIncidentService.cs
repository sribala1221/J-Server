using ServerAPI.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ServerAPI.Services
{
    public interface IInmateIncidentService
    {
        List<IncidentViewer> GetInmateIncidentList(int inmateId);
        InmateIncidentDropdownList GetInmateIncidentDropdownList(bool presetFlag, int facilityId);
        List<string> GetHousingUnitNumber(int facilityId, string housingUnitLocation);
        List<string> GetHousingUnitBedNumber(int facilityId, string housingUnitLocation, string housingUnitNumber);
        List<KeyValuePair<int, string>> GetLocation(int facilityId);
        Task<int> InsertInmateIncident(InmateIncidentInfo incidentDetails);
        InmateIncidentInfo GetInmateIncident(int incidentId);
        Task<int> UpdateInmateIncident(InmateIncidentInfo inmateIncidentInfo);
        List<DisciplinaryPresetVm> GetDisciplinaryPresetDetails();
        Task<int> InsertPresetIncident(DisciplinaryPresetVm presetDetails);
        List<IncarcerationForms> LoadIncidentForms(int incidentId, int dispInmateId);
        IncidentCellGroupVm GetCellGroupDetails(int facilityId, string housingUnitLocation, string housingUnitNumber);
        List<LastHousingSearchVm> GetLastSearchDetails(int facilityId, string searchFlag);
        List<CountHousingSearchVm> GetCountSearchList(int facilityId, string searchFlag, DateTime fromDate, string disposition, string lookupType);
        List<LastHousingSearchVm> GetLastSearchHistoryList(LastHousingSearchVm value, int facilityId, string searchFlag);
        List<CountHousingSearchVm> GetCountSearchHistoryList(CountHousingSearchVm value, int facilityId, string searchFlag);
    }
}
