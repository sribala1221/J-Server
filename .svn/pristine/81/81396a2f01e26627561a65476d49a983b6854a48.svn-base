using ServerAPI.ViewModels;
using System.Collections.Generic;
using System.Linq;
using GenerateTables.Models;
using System;
using Microsoft.AspNetCore.Http;
using System.IO;

namespace ServerAPI.Services
{
    public interface IFacilityHousingService
    {
        HousingDetail GetHousingDetails(int housingUnitId);
        List<HousingDetail> GetHousingList(List<int> housingUnitIds);
        List<HousingDetail> GetHousing(int facilityId);
    }
}
