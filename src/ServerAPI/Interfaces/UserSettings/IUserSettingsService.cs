﻿using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using JwtDb.Models;
using Microsoft.AspNetCore.Identity;
using ServerAPI.ViewModels;

namespace ServerAPI.Services
{
    public interface IUserSettingsService
    {
        Task<object> GetUserSettings(string settingsId, ClaimsPrincipal userPrincipal);
        Task<object> GetUserSettingsFromAppUser(string settingsId, AppUser user);
        Task<IdentityResult> SaveUserSettings(string settingsId, string settings, ClaimsPrincipal userPrincipal);
        WorkStationSettings GetWorkstationSettings(string workStationName);
        List<KeyValuePair<int, string>> GetLiveScanLocation();
        Task<int> InsertWorkstationSettings(WorkStationSettings workStationSettings);
        Task<int> UpdateWorkstationSettings(WorkStationSettings workStationSettings);
        Task<string> UpdateChangePassword(UserVm value, ClaimsPrincipal userPrincipal);
    }
}
