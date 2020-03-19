using System;
using System.Collections.Generic;

namespace ServerAPI.ViewModels
{
    public class ReleaseVm : Expedite
    {
        public DateTime? DateIn { get; set; }
        public int IncarcerationId { get; set; }
        public int? IncarcerationArrestXrefId { get; set; }
        public int InmateId { get; set; }
        public InmateHousing PersonDetails { get; set; }
        public DateTime? OverallFinalReleaseDate { get; set; }
        public bool ReleaseCompleteFlag { get; set; }
        public DateTime? TransportScheduleDate { get; set; }
        public string TransportHoldName { get; set; }
        public bool BookAndReleaseFlag { get; set; }
        public bool TransportFlag { get; set; }
        public HousingDetail HousingDetails { get; set; }
        public string LastClearReason { get; set; }
        public string AgencyName { get; set; }
        public string TransportInmateCaution { get; set; }
        public string TransportInstructions { get; set; }
        public string TransRoute { get; set; }
        public string TransType { get; set; }
        public int? WizardProgressId { get; set; }
        public List<WizardStep> LastStep { get; set; }
        public List<TasksCountVm> BookingTasks { get; set; }
        public string[] CaseType { get; set; }
    }

    public class SentenceClearVm : BookingClearVm
    {
        public string CourtName { get; set; }
        public int InmateId { get; set; }
        public InmateHousing PersonDetails { get; set; }
        public int? ArrestCourtJurisdictionId { get; set; }
        public int? ArrestReleaseClearedById { get; set; }
        public DateTime? OverallFinalReleaseDate { get; set; }
        public string ReleaseReason { get; set; }
        public string ReleaseNotes { get; set; }
        public bool ArrestActive { get; set; }
        public string ArrestSentenceDescription { get; set; }
        public PersonnelVm ArrestReleaseClearedBy { get; set; }
        public int? ArrestBookingStatus { get; set; }
        public string Abbr { get; set; }
    }
}
