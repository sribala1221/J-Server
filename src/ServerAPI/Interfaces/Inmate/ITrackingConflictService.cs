using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ServerAPI.ViewModels;

namespace ServerAPI.Services
{
    public interface ITrackingConflictService
    {
        List<TrackingConflictVm> GetTrackingConflict(TrackingVm obj);
        KeyValuePair<int, string> GetFacilityLocation(int inmateId);
    }
}
