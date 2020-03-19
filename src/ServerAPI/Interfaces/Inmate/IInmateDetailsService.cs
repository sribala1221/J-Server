using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using ServerAPI.ViewModels;

// ReSharper disable once CheckNamespace
namespace ServerAPI.Services
{
    public interface IInmateDetailsService
    {
        List<DetailVm> GetInmateDetailParent(int subModuleId);
        Task<InmateFileDetailVm> GetInmateDetailsCountByInmateNumber(string inmateNumber);
        Task<InmateFileDetailVm> GetInmateFileCount(int inmateId);

        BookingHistoryVm GetBookingHistory(int inmateId);
        Task<IdentityResult> UpdateLastXInmate(int inmateId);
        Task<List<InmateSearchVm>> GetLastXInmates();

    }
}