using ServerAPI.ViewModels;

namespace ServerAPI.Services
{
    public interface IAoScheduleAppointmentService
    {
        int CreateInmateAppointment(CreateAppointmentRequest appointmentRequest, int personnelId);
    }
}
