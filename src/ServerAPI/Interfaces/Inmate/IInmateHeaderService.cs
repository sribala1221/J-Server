using ServerAPI.ViewModels;

// ReSharper disable once CheckNamespace
namespace ServerAPI.Services
{
    public interface IInmateHeaderService
    {
        InmateHeaderVm GetInmateHeaderDetail(int inmateId, bool showMedicalAlerts = true);
        InmateHeaderVm GetInmateBasicInfo(int inmateId);
    }
}
