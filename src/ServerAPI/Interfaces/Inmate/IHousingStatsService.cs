using ServerAPI.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServerAPI.Services
{
    public interface IHousingStatsService
    {
        List<HousingStatsDetails> GetStatsInmateDetails(HousingStatsInputVm housingStatsInputs);
    }
}
