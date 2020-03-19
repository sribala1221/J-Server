using ServerAPI.ViewModels;

namespace ServerAPI.Services
{
    public interface IApptService
    {
        ApptTrackingVm GetApptCourtAndLocation(ApptParameter objApptParameter);
        ApptTrackingVm GetConsoleAppointmentCourtAndLocation(ApptParameter objApptParameter);
        TrackingSchedule GetTrackingSchedule(TrackingSchedule trackingSchedule, bool isFromTransfer = false);
    }
}