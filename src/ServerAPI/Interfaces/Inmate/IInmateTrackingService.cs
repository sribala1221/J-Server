using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GenerateTables.Models;
using ServerAPI.ViewModels;
using Microsoft.Extensions.Configuration;

namespace ServerAPI.Services
{
    public interface IInmateTrackingService
    {
        InmateTrackingVm GetInmateTrackingDetails(int inmateId);
        Task<InmateTrackingVm> InsertInmateTracking(InmateTrackingVm ob);
        InmateTrackDetailsVm GetInmateTracking(int facilityId, int inmateId);
        List<InmateTrackingHistroyVm> GetTrackingHistory(int inmateId, DateTime? dateIn, DateTime? dateOut);
    }
}
