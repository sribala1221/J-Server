using GenerateTables.Models;
using ServerAPI.Interfaces;
using ServerAPI.Utilities;
using ServerAPI.ViewModels;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace ServerAPI.Services
{
    public class ScheduleService : IScheduleService
    {

        private readonly AAtims _context;
        private VisitScheduleVm _visitScheduleDetailsVm;
        private readonly IAppointmentService _appointmentService;

        public ScheduleService(AAtims context,IAppointmentService appointmentService)
        {
            _context = context;
            _appointmentService = appointmentService;
        }

        public VisitScheduleVm GetVisitScheduleDetails(FacilityApptVm scheduleApptDetails)
        {
            //Get Privilege details
            scheduleApptDetails.PrivilegsDetails = GetPrivilegeList(scheduleApptDetails.FacilityId);

            //scheduleApptDetails.PrivilegsList = _listPrivileges.Select(p => p.PrivilegeDescription).ToList();

            _visitScheduleDetailsVm =
                new VisitScheduleVm
                {
                    //get Daily visitation details
                    DailyVisitationDetails =
                        GetDailyVisitationList(scheduleApptDetails.FacilityId, scheduleApptDetails.VisitationDay),

                    PrivilegeList = scheduleApptDetails.PrivilegsDetails,

                    //Get Daily Appointment details
                    VisitationApptList = GetAppointmentList(scheduleApptDetails)
                };

            return _visitScheduleDetailsVm;
        }

        private List<AoAppointmentVm> GetAppointmentList(FacilityApptVm scheduleApptDetails)
        {
            Debug.Assert(scheduleApptDetails.FacilityId != null, "ScheduleService: scheduleApptDetails.FacilityId != null");
            Debug.Assert(scheduleApptDetails.FromDate != null, "ScheduleService: scheduleApptDetails.FromDate != null");
            Debug.Assert(scheduleApptDetails.ToDate != null, "ScheduleService: scheduleApptDetails.ToDate != null");
            List<AoAppointmentVm> aoAppointmentList = _appointmentService.ListAoAppointments(scheduleApptDetails.FacilityId.Value, 
                scheduleApptDetails.InmateId, scheduleApptDetails.FromDate.Value, scheduleApptDetails.ToDate.Value, false);

            //To get VisitorList Appointment for Schedule
            List<int> lstVisitorList =
                _context.VisitToVisitor//.Where(v => !v.InactiveFlag)
                    .Select(s => s.PersonId).ToList();

			List<AoAppointmentVm> visitList = _context.VisitToVisitor.Where(v =>
					 v.Visit.Location.ShowInVisitation && !v.Visit.VisitSecondaryFlag.HasValue &&
					v.Visit.LocationDetail == scheduleApptDetails.VisitorLocation &&
					!v.Visit.EndDate.HasValue && v.Visit.Inmate.FacilityId == scheduleApptDetails.FacilityId &&
					v.Visit.StartDate.Date >= scheduleApptDetails.FromDate && v.Visit.StartDate.Date <= scheduleApptDetails.ToDate
					&& v.Visit.Location.TransferFlag == 1
					&& lstVisitorList.Contains(v.PersonId)).Select(y =>
				new AoAppointmentVm
				{
					//ApptDate = y.VisitorDateIn.Value,
					//ApptTime = y.VisitorDateIn.Value.ToString(DateConstants.HOURSMINUTES),
					//Reason = y.Visit.ReasonId,
					Location = y.Visit.LocationDetail,
					InmateId = y.Visit.InmateId ?? 0,
					//ApptId = y.VisitorId,
					//ApptType = y.VisitorType,
					Notes = y.Visit.Notes,
					LocationId = y.Visit.LocationId ?? 0,
					//InmateApptFlag = InmateApptFlag.Visitation
				}).ToList();

			aoAppointmentList.AddRange(visitList);

            List<string> lstLocation = scheduleApptDetails.PrivilegsDetails.Select(p => p.PrivilegeDescription).ToList();

            //Get Privileges details  
            aoAppointmentList = aoAppointmentList
                .Where(a => lstLocation.Contains(a.Location)).ToList();

            if (aoAppointmentList.Count > 0 && scheduleApptDetails.VisitorLocation != null)
            {
                aoAppointmentList = aoAppointmentList
                .Where(a => a.Location == scheduleApptDetails.VisitorLocation).ToList();
            }

            return aoAppointmentList;
        }

        //Get Privileges details
        private List<PrivilegeDetailsVm> GetPrivilegeList(int? facilityId) =>
           _context.Privileges
               .Where(p => p.InactiveFlag == 0 && p.ShowInVisitation
                           && (p.FacilityId == facilityId || p.FacilityId == 0 || !p.FacilityId.HasValue))
               .Select(p => new PrivilegeDetailsVm
               {
                   PrivilegeId = p.PrivilegeId,
                   PrivilegeDescription = p.PrivilegeDescription
               }).ToList();

        private List<DailyVisitationDetailsVm> GetDailyVisitationList(int? facilityId, string visitationDay)
        {
            //To get Schedule details
            List<DailyVisitationDetailsVm> dailyVisitationDetails = _context.HousingUnitVisitation
                .Where(huv => huv.FacilityId == facilityId && huv.VisitationDay == visitationDay
                              && (!huv.DeleteFlag.HasValue || huv.DeleteFlag == 0)).Select(huv =>
                    new DailyVisitationDetailsVm
                    {
                        VisitationFrom = huv.VisitationFrom,
                        VisitationTo = huv.VisitationTo,
                        HousingUnitLocation = huv.HousingUnitLocation,
                        HousingUnitNumber = huv.HousingUnitNumber,
                        Location = huv.Location,
                        LastNameChar = huv.LastNameChar,
                        FacilityAbbr = huv.Facility.FacilityAbbr,
                        VisitationDay = huv.VisitationDay,
                        Type = NumericConstants.ZERO
                    }).Distinct().ToList();

            dailyVisitationDetails.AddRange(_context.HousingUnitVisitationCell
                .Where(huc => huc.FacilityId == facilityId && huc.VisitationDay == visitationDay &&
                              (!huc.DeleteFlag.HasValue || huc.DeleteFlag == 0))
                .Select(huc => new DailyVisitationDetailsVm
                {
                    VisitationFrom = huc.VisitationFrom,
                    VisitationTo = huc.VisitationTo,
                    HousingUnitLocation = huc.HousingUnit.HousingUnitLocation,
                    HousingUnitNumber = huc.HousingUnit.HousingUnitNumber,
                    LastNameChar = huc.LastNameChar,
                    FacilityAbbr = huc.Facility.FacilityAbbr,
                    VisitationDay = huc.VisitationDay,
                    Type = NumericConstants.ONE
                }).Distinct().ToList());

            dailyVisitationDetails = dailyVisitationDetails.OrderBy(o => o.FacilityAbbr)
                .ThenBy(o => o.HousingUnitLocation)
                .ThenBy(o => o.HousingUnitNumber).ToList();

            return dailyVisitationDetails;
        }

        public List<InmateDetailsList> GetVisitationInmateDetails(FacilityApptVm scheduleApptDetails)
        {
            //Taking Inmate details for Visit schedule
            List<InmateDetailsList> visitInmateDetails = _context.Inmate
                .Where(i => i.FacilityId == scheduleApptDetails.FacilityId
                            && i.HousingUnitId.HasValue
                            && i.HousingUnit.HousingUnitLocation == scheduleApptDetails.HousingLocation
                            && i.HousingUnit.HousingUnitNumber == scheduleApptDetails.HousingNumber
                            && i.InmateActive == 1)
                .Select(i => new InmateDetailsList
                {
                    InmateId = i.InmateId,
                    InmateNumber = i.InmateNumber,
                    HousingUnitBedNumber = i.HousingUnit.HousingUnitBedNumber,
                    HousingUnitBedLocation = i.HousingUnit.HousingUnitBedLocation,
                    PersonLastName = i.Person.PersonLastName,
                    PersonMiddleName = i.Person.PersonMiddleName,
                    PersonFirstName = i.Person.PersonFirstName
                }).ToList();

            if (!string.IsNullOrEmpty(scheduleApptDetails.LastNameChar))
            {
                visitInmateDetails = visitInmateDetails.Where(w =>
                    w.PersonLastName.StartsWith(scheduleApptDetails.LastNameChar)).ToList();
            }

            if (!string.IsNullOrEmpty(scheduleApptDetails.VisitorLocation))
            {
                //List<int> arrInmate = _context.Visit
                //    .Where(i => i.VisitLocation == scheduleApptDetails.VisitorLocation
                //                && i.VisitAoschedule.StartDate == scheduleApptDetails.FromDate)
                //    .Select(s => s.InmateId).ToList();

                ////Take visitation Inmate List based in VisitLocation
                //visitInmateDetails = visitInmateDetails.Where(i => arrInmate.Contains(i.InmateId)).ToList();
            }

            return visitInmateDetails;
        }       
    }
}
