using ServerAPI.ViewModels;

// ReSharper disable once CheckNamespace
namespace ServerAPI.Services
{
    public interface IEnrouteService
    {
        EnrouteLocationVm GetEnrouteLocations(int facilityId);
    }
}
