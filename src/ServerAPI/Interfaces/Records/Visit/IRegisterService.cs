using GenerateTables.Models;
using ServerAPI.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace ServerAPI.Interfaces
{
    public interface IRegisterService
    {
        AoWizardFacilityVm GetVisitRegistrationWizard();
        List<VisitDetails> GetVisitDetails(VisitParam objVisitParam);
        List<VisitDetails> GetScheduleVisitDetails(VisitParam objVisitParam);
        IdentifyVisitorVm GetIdentifiedVisitorDetails(VisitParam objVisitParam);
        List<ProfessionalSearchDetails> GetProfessionalDetails(ProfessionalSearchDetails objParam);
        Task<int> InsertUpdateVisitorPersonDetails(PersonIdentity visitor);
        Task<KeyValuePair<int, int>> InsertIdentifiedVisitorDetails(IdentifyVisitorVm objParam);
        Task<int> UpdateIdentifiedVisitorDetails(IdentifyVisitorVm objParam);
        Task<int> UpdateVisitDenyDetails(VisitDetails objParam);
        Task<int> DenyVisitor(VisitDetails objParam);
        // Task<int> InsertChildVisitors(List<VisitToChild> objChildList);
        //Task<int> UpdateChildVisitors(List<VisitToChild> objChildList);
        SavedVisitorDetailsVm GetSavedVisitorDetails(int scheduleId);
        SelectInmateVm GetSelectedInmateVisitDetails(int inmateId, List<int> lstVisitorId, int scheduleId);
        Task<int> InsertVisitorToInmate(List<VisitorToInmateDetails> lstVisitorToInmateDetail);
        Task<int> UpdateVisitorToInmate(List<VisitorToInmateDetails> lstVisitorToInmateDetail);
        Task<int> UpdateScheduleInmate(int scheduleId, int inmateId);
        InmateDetail GetVisitInmateDetails(int inmateId);
        PrimaryVisitorVm GetSchedulePrimaryDetail(PrimaryVisitorVm objParam);
        VisitScheduledDetail GetVisitRoomDetails(VisitScheduledVm objParam);
        VisitScheduledBoothDetail GetVisitBoothDetails(VisitScheduledBoothVm objParam);
        Task<int> UpdateRoomBoothDetails(VisitSchedule visitSchedule);
        List<TrackingConflictVm> SchedulingConflict(VisitSchedule visitSchedule);
        ConfirmVisitorIdVm GetConfirmVisitorIdDetails(int inmateId, int scheduleId);
        Task<int> UpdateConfirmIdDetails(int scheduleId);
        Task<int> UpdateAcknowledgementBackground(int scheduleId, bool backgroundFlag);
        Task<int> UpdateCompleteRegistration(int scheduleId);
        Task<int> DeleteVisitRegistration(int scheduleId);
        
        List<AoWizardProgressVm> GetVisitRegistrationWizardProgress(List<int> lstScheduleId);

        #region Old Code
        List<RegisterDetails> GetVisitorList(SearchRegisterDetails searchDetails);


        //InmateScheduleDetails GetScheduleList(int inmateId);

        //ExistingVisitDetails GetExistingVisitDetails(int inmateId);

        //FutureVisitDetails GetFutureVisitDetails(int inmateId, DateTime currentDate);

        //List<VisitorPersonList> GetSelectedVisitor(int inmateId, bool isProfFlag);

        //VisitorPersonDetails GetVisitorDetails(int visitorListId, int inmateId);

        //List<VisitorInfo> GetRecentVisitorInfo(int inmateId, bool isPersonalFlag);

        //int GetDuplicateVisitor(int personId, int inmateId, DateTime currentDate);

        //Task<int> InsertVisitationConflict(KeyValuePair<int, string> floorNoteNarrative);

        //Task<int> InsertUpdateVisitorDetails(RecordsVisitationVm recordsVisitation);

        //Task<int> DeleteVisitorDetails(int visitorId);

        //VisitationConflict GetVisitationConflict(int inmateId, int locationId, DateTime visitorDate);
        //int GetExitVisitor(int inmateId);
        Task<int> UpdateExitVisitor(KeyValuePair<int, int> exitVisitor);
        //AppointmentConflictCheck CheckScheduleConflict(int inmateId, DateTime? startDate, DateTime? endDate, int locationId);

        //List<VisitBoothDetailsVm> GetBoothConflict(RecordsVisitationVm visitReq);

        //Task<int> UpdateConsoleVisit(RegisterDetails value);

        List<CompleteVisitReason> GetCompleteVisits();
        Task<int> UpdateScheduleVisits(ScheduledVisits scheduledVisits);
        List<TrackingConflictVm> GetInmateConflict(int inmateId, int personId);
        bool GetVisitationPermissionRights();
        List<VisitToChild> GetChildVisitors(int scheduleId, int personId);
        List<VisitToAdultDetail> GetAdultVisitorDetails(int scheduleId);

        List<VisitorIdentityAndRelationship> GetInmateRelationShipDetails(int scheduleId, VisitorIdentityAndRelationship inmateId);
        #endregion
    }
}
