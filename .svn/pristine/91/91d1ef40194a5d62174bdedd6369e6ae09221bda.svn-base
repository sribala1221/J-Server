using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using ServerAPI.ViewModels;
using GenerateTables.Models;
using ServerAPI.Utilities;

namespace ServerAPI.Services
{
    public class MedicalAlertsService : IMedicalAlertsService
    {

        private readonly AAtims _context;
        private readonly int _personnelId;

        public MedicalAlertsService(AAtims context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _personnelId = Convert.ToInt32(httpContextAccessor.HttpContext.User
                .FindFirst(AuthDetailConstants.PERSONNELID)?.Value);
        }

        public MedicalAlertInmateVm GetMedicalAlertInmate(MedicalAlertInmateVm inputs)
        {
            string[] buildingInfo = new string[] {};
            List<LookupVm> MedAlertinfo = new List<LookupVm> {};
            List<KeyValuePair<string, string>> buildingNumberInfo = new List<KeyValuePair<string, string>> {};

             if (!inputs.RefreshFlag)
             {
                buildingInfo = _context.HousingUnit.Where(i => i.FacilityId == inputs.FacilityId ||inputs.FacilityId == 0).Select(s => s.HousingUnitLocation).Distinct().ToArray();
                MedAlertinfo = _context.Lookup.Where(x => x.LookupType == LookupConstants.MEDFLAG)
                 .Select(a => new LookupVm
                 {
                     LookupDescription = a.LookupDescription,
                     LookupIndex = a.LookupIndex
                 }).ToList();
                buildingNumberInfo = _context.HousingUnit
                  .Where(i => i.FacilityId == inputs.FacilityId && buildingInfo.Contains(i.HousingUnitLocation))
                  .Select(i => new KeyValuePair<string, string>(i.HousingUnitLocation, i.HousingUnitNumber)).Distinct().ToList();

            }
             List<MedicalAlertInmateDataVm> medIndex = _context.PersonFlag.Where(w => w.MedicalFlagIndex > 0 && w.DeleteFlag == 0)
            .Select(a => new MedicalAlertInmateDataVm
            {
                MedicalFlagIndex = a.MedicalFlagIndex,
                PersonId = a.PersonId
            }).ToList();
           int[] medIndexpersonIds = medIndex.Select(a => a.PersonId).ToArray();
            List<MedicalAlertInmateDataVm> lstGetAlertInmate = _context.Inmate.Where(i =>
                     (i.FacilityId == inputs.FacilityId)
                      && (inputs.Building == null || i.HousingUnit.HousingUnitLocation == inputs.Building)
                      && (inputs.BuildingNumber == null || i.HousingUnit.HousingUnitNumber == inputs.BuildingNumber) && i.InmateActive == 1).Where
                      (w=>medIndexpersonIds.Contains(w.PersonId))
                .Select(s =>
                new MedicalAlertInmateDataVm
                {
                    HousingUnitLocation = s.HousingUnit.HousingUnitLocation,
                    HousingUnitNumber = s.HousingUnit.HousingUnitNumber,
                    HousingUnitBedLocation = s.HousingUnit.HousingUnitBedLocation,
                    HousingUnitBedNumber = s.HousingUnit.HousingUnitBedNumber,
                    PersonId = s.PersonId,
                    InmateNumber = s.InmateNumber,
                    InmateId = s.InmateId,
                 
                }).ToList();
            int[] personIds = lstGetAlertInmate.Select(a => a.PersonId).ToArray();
            
              medIndex.ForEach(item =>
            {
              MedicalAlertInmateDataVm inmateIds = lstGetAlertInmate.FirstOrDefault(x => x.PersonId == item.PersonId);
                    if(inmateIds!=null){
                    item.InmateId=inmateIds.InmateId;                     
                    }

            });
            medIndex=medIndex.Where(w=>w.InmateId!=0).ToList();
     
            List<PersonInfoVm> personInfolst = _context.Person.Where(a => personIds.Contains(a.PersonId))
            .Select(a => new PersonInfoVm
            {
                PersonId = a.PersonId,
                PersonFirstName = a.PersonFirstName,
                PersonLastName = a.PersonLastName,
                PersonMiddleName = a.PersonMiddleName,

            }).ToList();
         
            int?[] LookupindexIds = medIndex.Where(w => w.MedicalFlagIndex != null).Select(a => a.MedicalFlagIndex).ToArray();
            List<LookupVm> lstAlerts = _context.Lookup.Where(a => LookupindexIds.Contains((int?)a.LookupIndex) && a.LookupType == LookupConstants.MEDFLAG)
            .Select(a => new LookupVm
            {
                LookupDescription = a.LookupDescription,
                LookupIndex = a.LookupIndex
            }).ToList();
            medIndex.ForEach(item =>
            
                {
                    MedicalAlertInmateDataVm lst = lstGetAlertInmate.FirstOrDefault(x => x.PersonId == item.PersonId);
                    if(lst!=null){
                    item.PersonId =  lst.PersonId;
                    item.InmateId=lst.InmateId;
                    item.HousingUnitLocation = lst.HousingUnitLocation;
                    item.HousingUnitNumber = lst.HousingUnitNumber;
                    item.HousingUnitBedLocation = lst.HousingUnitBedLocation;
                    item.HousingUnitBedNumber = lst.HousingUnitBedNumber;
                    item.InmateNumber = lst.InmateNumber;
                          
                    }
                  
                    LookupVm lookupInfo = lstAlerts.SingleOrDefault(x => x.LookupIndex == item.MedicalFlagIndex);
                    item.MedAlertDescription = lookupInfo?.LookupDescription;
                    PersonInfoVm personInfo = personInfolst.SingleOrDefault(x => x.PersonId == item.PersonId);
                    item.PersonInfo = personInfo;
                });
             if(inputs.MedAlert>0 &&inputs.MedAlert!=null)
            {
                medIndex = medIndex.Where(w=>w.MedicalFlagIndex==inputs.MedAlert).ToList();
            }
         

            MedicalAlertInmateVm GetMedicalAlertDetails = new MedicalAlertInmateVm
            {
                 MedicalAlertDetails = medIndex,
                BuildingInfo = buildingInfo,
                BuildingNumberInfo = buildingNumberInfo,
                MedAlertinfo = MedAlertinfo,
                Building=inputs.Building,
                BuildingNumber=inputs.BuildingNumber,
                MedAlert=inputs.MedAlert,
                RefreshFlag=inputs.RefreshFlag
            };
            return GetMedicalAlertDetails;
        }
    }
}