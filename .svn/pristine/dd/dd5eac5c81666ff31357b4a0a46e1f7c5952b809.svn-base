using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ServerAPI.ViewModels;

namespace ServerAPI.Services
{

    public interface IIncidentWizardService
    {
        bool CheckIncidentComplete(int incidentId);
        List<AoAppointmentVm> GetAppointmentSchedule(int facilityId, int locationId, DateTime fromDate, DateTime toDate);
        List<IncidentAppointmentVm> GetAppointmentRooms(DateTime? date, int locationId);
        bool CheckIncidentHearing(int incidentId);
		List<int> GetDuplicateAppointments(int inmateId, string location, DateTime fromDate, DateTime todate);
		Task<int> UpdateScheduleHearingLocation(IncidentAppointmentVm incidentAppointment);
		
        List<IncidentViolationVm> DisciplinaryLookUpDetails(string code = "", string description = "");
        List<IncicentViolationSaveVm> ViolationDetails(int diciplinaryInmateId);
        Task<int> DisciplinaryControlInsertUpdate(IncicentViolationSaveVm details);
        Task<int> DisciplinaryInmateUpdate(IncidentInmateVm value);
        DisciplinaryHearing GetScheduleHearing(int disciplinaryInmateId);
        ClassifyInvPartyDetailsVm GetInvPartyEntryDetails(int disciplinaryInmateId);
        Task<int> InsertUpdateInvolvedParty(ClassifyInvPartyDetailsVm invParty);
        Task<int> InsertFlagEntry(IncidentViewer incidentViewer);
        Task<int> DisciplinaryControlXrefUpdate(IncicentViolationSaveVm value);
        Task<int> IncidentHearingUpdate(DisciplinaryHearing value);
        List<AppliedBooking> GetAppliedBookings(int dispInmateId, int arrestId = 0);
        List<AppliedCharge> GetAppliedCharges(int dispInmateId, int crimeId = 0);
        Task<int> ReviewComplete(ReviewComplete reviewComplete);
        IncidentWizardCompleteDetails GetIncidentWizardCompleteDetails(int incidentId);
        Task<int> UpdateIncidentWizardComplete(IncidentWizardCompleteDetails incidentDetails);
        void UpdateDisciplinaryInmateNotice(int disciplinaryInmateId, DisciplinaryInmateNotice notice);
        DisciplinaryInmateNotice GetDisciplinaryInmateNotice(int disciplinaryInmateId);
        bool CheckRecommendedFlag(int violationId, int findingId);
        int GetPriorCount(int inmateId, int findingId, int violationId, int incidentId);
        KeyValuePair<int, int> GetSanction(int violationId, bool isRecSancOne);
    }
}
