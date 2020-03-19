using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServerAPI.ViewModels
{
    public class BookingOverviewDetails : Expedite
    {
        public string PrebookNumber { get; set; }
        public int PrebookId { get; set; }
        public DateTime? CreateDate { get; set; }
        public bool NoKeeper { get; set; }
        public bool TransportFlag { get; set; }
        public int InmateId { get; set; }
        public int? PersonId { get; set; }
        public int IncarcerationId { get; set; }
        public bool IntakeCompleteFlag { get; set; }
        public bool BookCompleteFlag { get; set; }
        public bool BookAndReleaseFlag { get; set; }
        public bool ReleaseCompleteFlag { get; set; }
        public bool AssessmentCompleteFlag { get; set; }
        public DateTime? OverallFinalReleaseDate { get; set; }
        public DateTime? TransportDate { get; set; }
        public InmateHousing PersonDetails { get; set; }
        public List<TasksCountVm> InmateTasks { get; set; }
        public int? IntakeWizardProgressId { get; set; }
        public List<WizardStep> IntakeWizards { get; set; }
        public int? BookingWizardProgressId { get; set; }
        public List<WizardStep> BookingWizards { get; set; }
        public int? ReleaseWizardProgressId { get; set; }
        public List<WizardStep> ReleaseWizards { get; set; }
        public bool PrebookCompleteFlag { get; set; }
        public bool ReleaseClearFlag { get; set; }
        public int? AssessmentWizardProgressId { get; set; }
        public List<WizardStep> AssessmentWizards { get; set; }
        public string[] CaseType { get; set; }


        
    }

    public class BookingOverviewVm
    {
        public List<BookingOverviewDetails> BookingOverviewDetails { get; set; }
        public List<TasksCountVm> TasksCount { get; set; }
    }

    public class BookingReview : BookingOverviewDetails
    {
        public bool ReleaseSupervisorCompleteFlag { get; set; }
        public bool BookingSupervisorCompleteFlag { get; set; }
        public DateTime? ReleaseClearDate { get; set; }
        public int? BookingSupervisorId { get; set; }
        public int? ReleaseSupervisorId { get; set; }
        public string LastClearReason { get; set; }
        public List<WizardStep> ReviewBookingWizards { get; set; }
        public List<WizardStep> ReviewReleaseWizards { get; set; }
    }
}
