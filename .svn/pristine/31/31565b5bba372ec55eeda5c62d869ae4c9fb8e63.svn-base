using GenerateTables.Models;
using ServerAPI.ViewModels;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace ServerAPI.Services
{
    public class InmateIncidentAppointmentService: IInmateIncidentAppointmentService
    {
        private readonly AAtims _context;
        private readonly int _personnelId;

        public InmateIncidentAppointmentService(AAtims context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _personnelId = Convert.ToInt32(httpContextAccessor.HttpContext.User.FindFirst("personnelId")?.Value);
        }

        public async Task<int> InsertSchedule(InmateApptDetailsVm inmateApptDetails)
        {
            ScheduleInvolvedParty appointment = _context.ScheduleIncident.SingleOrDefault(ii =>
                ii.ScheduleId == inmateApptDetails.ScheduleId);
            if (appointment is null)
            {
                appointment = new ScheduleInvolvedParty();
                Debug.Assert(inmateApptDetails.AoScheduleDetails.DispInmateId != null, 
                    "AppointmentService: inmateApptDetails.AoScheduleDetails.DispInmateId != null");
                appointment.DisciplinaryInmateId = inmateApptDetails.AoScheduleDetails.DispInmateId.Value;
                appointment.CreateDate = DateTime.Now;
                appointment.CreateBy = _personnelId;
                appointment.LocationId = inmateApptDetails.AoScheduleDetails.LocationId;
                appointment.StartDate = inmateApptDetails.AoScheduleDetails.StartDate;
                appointment.EndDate = inmateApptDetails.AoScheduleDetails.EndDate;
                appointment.Duration = inmateApptDetails.AoScheduleDetails.Duration;
                appointment.IsSingleOccurrence = true;

                _context.ScheduleIncident.Add(appointment);
            }
            else
            {
                appointment.LocationId = inmateApptDetails.AoScheduleDetails.LocationId;
                appointment.StartDate = inmateApptDetails.AoScheduleDetails.StartDate;
                appointment.EndDate = inmateApptDetails.AoScheduleDetails.EndDate;
                appointment.Duration = inmateApptDetails.AoScheduleDetails.Duration;
            }

            return await _context.SaveChangesAsync();
        }

        public async Task<int> DeleteIncidentAppointment(DeleteUndoInmateAppt deleteInmateAppt)
        {
            ScheduleInvolvedParty appt = _context.ScheduleIncident.Single(x => x.ScheduleId == deleteInmateAppt.ScheduleId);

            appt.DeleteReason = deleteInmateAppt.DeleteReason;
            appt.DeleteDate = DateTime.Now;
            appt.DeleteBy = _personnelId;
            appt.DeleteFlag = true;

            return await _context.SaveChangesAsync();
        }
    }
}
