using GenerateTables.Models;
using ServerAPI.ViewModels;
using System;
using System.Linq;

namespace ServerAPI.Services
{
    public class AoScheduleAppointmentService : IAoScheduleAppointmentService
    {
        private readonly AAtims _context;

        public AoScheduleAppointmentService(AAtims context)
        {
            _context = context;
        }

        //we must add the conflict check logic 
        //we must add inmate exclusion 
        //we must add schedule exclusion 
        public int CreateInmateAppointment(CreateAppointmentRequest appointmentRequest, int personnelId)
        {
            //handle exceptions here
            //handle inmate exceptions here

            ScheduleInmate appointment = new ScheduleInmate
            {
                InmateId = appointmentRequest.InmateId,
                ReasonId = appointmentRequest.ReasonId,
                TypeId = appointmentRequest.TypeId,
                LocationDetail = appointmentRequest.LocationDetails,
                Notes = appointmentRequest.Notes,
                CreateDate = DateTime.Now,
                CreateBy = personnelId,
                LocationId = appointmentRequest.Schedule.LocationId,
                StartDate = appointmentRequest.Schedule.StartDate,
                EndDate = appointmentRequest.Schedule.EndDate,
                Duration = appointmentRequest.Schedule.Duration,
                IsSingleOccurrence = appointmentRequest.Schedule.IsSingleOccurrence,
                Time = appointmentRequest.Schedule.Time,
                DayInterval = appointmentRequest.Schedule.DayInterval,
                WeekInterval = appointmentRequest.Schedule.WeekInterval,
                FrequencyType = appointmentRequest.Schedule.FrequencyType,
                MonthOfYear = appointmentRequest.Schedule.MonthOfYear,
                DayOfMonth = appointmentRequest.Schedule.DayOfMonth
            };

            _context.ScheduleInmate.Add(appointment);
            _context.SaveChanges();

            return appointment.ScheduleId;
        }

    }

}
