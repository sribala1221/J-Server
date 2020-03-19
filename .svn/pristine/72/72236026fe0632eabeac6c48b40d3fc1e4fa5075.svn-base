using System;
using ServerAPI.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ServerAPI.Services
{
    public interface IIncidentService
    {
        List<KeyValuePair<int, string>> GetHearingLocation();
        List<IncidentCalendarVm> GetIncidentCalendarDetails(DateTime appointmentDate, int facilityId);
        List<ClassifyInvPartyDetailsVm> GetInvPartyDetails(int facilityId, int filterCategory);
        AoWizardFacilityVm GetIncidentWizard();
        List<IncidentNarrativeDetailVm> NarrativeDetails(int facilityId);
        NarrativeCommonDetailVm NarrativeReview(int incidentId);
        Task<int> UpdateIncident(IncidentNarrativeDetailVm value);
        IncidentCountVm GetIncidentLocation(IncidentLocationVm incidentLocation);
        Task<int> UpdateApproveDeny(IncidentNarrativeDetailVm value);      
        List<AoWizardProgressVm> GetIncidentWizardProgress(int[] dispInmateIds);
        KeyValuePair<int, int> GetAcceptLogic(int dispInmateId);
        ViolationDetails CheckSanction(int dispCtrlXrefId);
        List<DiscDaysHistory> GetDiscDaysHistories(int inmateId);
        List<SentenceVm> GetSentenceDetails(int inmateId);
        Task<int> SaveAppliedBookings(List<AppliedCharge> applieds);
        List<ClearChargesVm> GetSentenceCharges(int inmateId);
        Task<int> DeleteApplyCharges(DeleteRequest deleteParams);
        Task<int> UpdateDispDaySentFlag(ReviewComplete review);
        Task<int> DeleteApplyBookings(DeleteRequest deleteRequest);
        List<IncidentNarrativeDetailVm> SupervisorStatusDetails(int facilityId, List<LookupVm> lookups);
        List<IncidentNarrativeDetailVm> SupervisorNarrativesDetails(int facilityId);
        NarrativeCommonDetailVm GetNarrativeCommonDetail(int facilityId);
        List<IncidentNarrativeDetailVm> GetHearingQueueNarratives(int facilityId, int queueType);
        List<IncidentNarrativeDetailVm> SupervisorAppealQueue(int facilityId);
        bool IsInvolvedPartyExists(int incidentId, int inmateId, int personnelId);
        List<DisciplinaryWTStopVm> GetDisciplinaryWTStops(int dispInmateId, int incarcerationId);
        IncarcerationDetail GetActiveBooking(int inmateId);
        List<WtStoppageHistory> GetWtStoppageHistories(int dispInmateId, int incarcerationId);
        KeyValuePair<int?, int?> GetGtAndWtFieldVisibility();
        List<DisciplinaryWTStopVm> GetWTStoppageDetails(int inmateId);
        Task<int> UpdateWTStoppage(DisciplinaryWTStopVm disciplinaryWTStop);
        Task<int> DeleteWTStoppage(int wtStopId);
    }
}