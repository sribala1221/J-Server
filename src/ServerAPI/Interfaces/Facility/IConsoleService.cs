﻿using Microsoft.AspNetCore.Identity;
using ServerAPI.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ServerAPI.Services
{
    public interface IConsoleService
    {
        ConsoleVm GetConsoleDetails(ConsoleInputVm value,string hostName);
        Task<IdentityResult> InsertMyLocationList(List<ConsoleLocationVm> locationlist);
        List<ConsoleLocationVm> GetMyLocationList(int facilityId);
        List<InmateVm> LoadInmateList(int facilityId, int housingUnitListId, int inmateCurrentTrackId);
        List<ReleaseQueueVm> GetReleaseQueueDetails(int facilityId, string releaseDescription, List<int> housingUnitListId,bool operationFlag);
        IncarcerationFormListVm LoadIncarcerationFormDetails(ConsoleInputVm value);
        Task<int> ClearFormRecord(IncarcerationForms incarcerationForms);
        BatchSafetyCheckVm LoadBatchSafetyCheckList(ConsoleInputVm value);
        Task<int> InsertBatchSafetyCheck(List<SafetyCheckVm> value);
        //List<SafetyCheckVm> LoadBatchSafetyCheckList1(ConsoleInputVm value);
        //List<SafetyCheckVm> LoadBatchSafetyCheckDetails1(int facilityId, int housingUnitListId);
        ConsoleVm GetLocationId(int facilityId, int housingUnitListId, int housingGroupId, string hostName);
    }
}