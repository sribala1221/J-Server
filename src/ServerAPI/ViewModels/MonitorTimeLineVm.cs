using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServerAPI.ViewModels
{
    public class MonitorTimeLineSearchVm
    {
        public int FacilityId { get; set; }
        public DateTime DateOfRecord { get; set; }
        public int PersonnelId { get; set; }
        public int InmateId { get; set; }
        public bool PreBookFlag { get; set; }
        public bool IntakeFlag { get; set; }
        public bool BookingFlag { get; set; }
        public bool IncarcerationFlag { get; set; }
        public bool ReleaseFlag { get; set; }
        public bool ClassificationFlag { get; set; }
        public bool HousingFlag { get; set; }
        public bool GrievanceFlag { get; set; }
        public bool IncidentFlag { get; set; }
        public bool NotesFlag { get; set; }
        public bool CellLogFlag { get; set; }
        public bool AppointmentsFlag { get; set; }
        public bool VisitationFlag { get; set; }
        public bool TrackingFacilityFlag { get; set; }
        public bool TrackingNoFacilityFlag { get; set; }
        public bool MedDistributeFlag { get; set; }
    }

    public class MonitorTimeLineDetailsVm
    {
        public DateTime? DateOfRecord { get; set; }
        public string Category { get; set; }
        public PersonVm InmateInfo { get; set; }
        public PersonnelVm Personnel { get; set; }
        public HousingDetail HousingUnitLoation { get; set; }
        public int? InmateId { get; set; }
        public int ArrestOfficerId { get; set; }
        public string DetailOne { get; set; }
        public string DetailTwo { get; set; }
        public string DetailThree { get; set; }
        public int DetailFour { get; set; }
        public DateTime? DetailInDate { get; set; }
        public int? IncarcerationId { get; set; }
        public int FloorNotesId { get; set; }
        public int DisciplinaryIncidentId { get; set; }
        public int? CreateBy { get; set; }
        public PersonVm VisitBy { get; set; }
        public int? AcceptedFlag { get; set; }
        public int? RejectedFlag { get; set; }
    }

}
