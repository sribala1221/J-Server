using System;
using System.Threading.Tasks;
using ServerAPI.ViewModels;
using System.Collections.Generic;

namespace ServerAPI.Services
{
    public interface IIncidentActiveService
    {
        IncidentViewerDetails LoadActiveIncidentViewerDetails(IncidentFilters incidentActiveFilters);
        IncidentDetails LoadIncidentDetails(int incidentId,int dispInmateId, bool deleteFlag);
        int GetIncidentActiveStatus(int incidentId);
        Task<int> InsertOrUpdateNarrativeInfo(IncidentNarrativeDetailVm narrative);
        Task<int> UpdateIncidentActiveStatus(int incidentId, bool status);
        Task<int> DeleteNarrativeInfo(int narrativeId);
        List<LoadSavedForms> LoadForms(string categoryName);
        Task<int> InsertFormRecord(int formTemplateId, int grievanceOrIncidentId, bool isIncident, int inmateId);
        List<IncidentNarrativeDetailVm> LoadDisciplinaryNarrativeDetails(int incidentId);
        IncidentHistoryDetails GetIncidentHistoryDetails(IncidentFilters value);
        List<PrebookAttachment> LoadIncidentAttachments(int incidentId, bool deleteFlag);
        List<IdentifierVm> LoadIncidentPhotos(int incidentId, bool deleteFlag);
        List<IncarcerationForms> LoadIncidentForms(int incidentId);
        List<IncidentViewer> LoadInvolvedPartyDetails(int incidentId, int dispInmateId, bool isWizardSteps);
        Task<int> DeleteInvolvedPartyDetails(int disciplinaryInmateId);      
        IncidentReport GetIncidentReportDetails(int incidentId, int inmateId, int dispInmateId, int reportType);
		IncidentViewer GetIncidentBasicDetails(int incidentId);
        List<ViolationDetails> GetViolationDetails(int incidentId, int[] dispInmateId, bool isActive = false);
        Task<int> DeleteUndoInmateForm(KeyValuePair<int, bool> inmateFormDetail);
        List<InmateFormDetails> LoadInmateFormDetails(int dispInmateId);
        List<KeyValuePair<int, string>> GetIncidnetLocations(int facilityId);
        List<KeyValuePair<int, string>> GetIncidentOtherLocations(int facilityId);
        KeyValuePair<int, int> GetOverAllGtandDiscDays(int inmateId, DateTime incidentDate);
    }
}
