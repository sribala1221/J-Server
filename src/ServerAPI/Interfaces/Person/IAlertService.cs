using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ServerAPI.ViewModels;

namespace ServerAPI.Services
{
    public interface IAlertService
    {
        Task<bool> InsertUpdateMessageAlertDetails(PersonAlertVm personAlertDetails);
        List<PersonAlertVm> GetMessageAlertDetailLst(int personId);
        List<HistoryVm> GetMessageAlertHistoryLst(int alertId);

        List<FlagAlertVm> GetFlagAlertDetails(int personId, bool isPermission);
        List<FlagAlertHistoryVm> GetFlagAlertHistoryDetails(int personId, int flagIndex, string type);
        Task<int> InsertUpdateFlagAlert(FlagAlertVm flagAlertDetails);
        List<PersonClassificationDetails> GetAssocAlertDetails(int personId);
        List<PrivilegeAlertVm> GetPrivilegeDetails(int inmateId, int incidentId);
        Task<int> InsertOrUpdatePrivilegeInfo(PrivilegeAlertVm privilege);
        PrivilegeIncidentDetailsVm GetPrivilegeIncidentDetails(int inmateId, int facilityId,bool isMailComponent);
        List<DisciplinaryIncidentDetails> GetDisciplinaryIncidentList(int inmateId);
        List<HistoryVm> GetPrivilegeHistoryDetails(int privilegeXrefId);
        List<AssocAlertHistoryVm> GetAssocHistoryDetails(int personClassificationId);
        Task<AssocAlertValidation> InsertUpdateAssocDetails(PersonClassificationDetails personAssocDetails);
        Task<AssocAlertValidation> DeleteAssocDetails(PersonClassificationDetails personAssocDetails);
        ObservationVm GetObservationLog(int inmateId, int deleteFlag);
        List<HistoryVm> GetObservationHistory(int observationScheduleId);
        FlagAlertHistoryByPersonVm GetFlagAlertHistoryByPerson(int personId);
        List<FlagAlertHistoryVm> GetFlagAlertHistoryByPerson(int personId, FlagAlert flagAlert);
        Task<int> InsertObservationScheduleEntryDetails(ObservationScheduleVm scheduleDetails);
        Task<int> UpdateObservationScheduleEntryDetails(ObservationScheduleVm scheduleDetails);
        Task<int> DeleteUndoObservationScheduleEntry(ObservationScheduleVm scheduleDetails);
        ObservationLookupDetails GetObservationLookupDetails();
        Task<int> UpdateScheduleActionNote(ObservationScheduleActionVm observationScheduleAction);
        ObservationLogItemDetails LoadObservationLogDetail(int observationLogId);
        List<KeyValuePair<int, string>> GetLookupSubList(int LookupIdentity);
        List<AlertFlagsVm> GetAlerts(int personId, int inmateId);
        List<ObservationLogVm> GetObservationLog(int inmateId);
        List<AlertFlagsVm> GetMedicalAlerts(int personId);
        List<PrivilegeDetailsVm> GetPrivilegesAlert(int inmateId);
        bool CheckScheduleOverlap(DateTime dateTime, int inmateId);
    }
}
