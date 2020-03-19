using System.Collections.Generic;
using System.Linq;
using GenerateTables.Models;
using JetBrains.Annotations;
using ServerAPI.Utilities;
using ServerAPI.ViewModels;

namespace ServerAPI.Services
{
    [UsedImplicitly]
    public class MedicalAlertsFlagViewerService : IMedicalAlertsFlagViewerService
    {
        private readonly AAtims _context;

        public MedicalAlertsFlagViewerService(AAtims context) => _context = context;


        // AO_MedicalAlertsInmateDetails
        public MedicalAlertInmateVm GetMedicalAlertsFlagViewers(MedicalAlertInmateVm inputs)
        {
            string[] buildingInfo = { };
            List<LookupVm> medAlertinfo = new List<LookupVm>();
            List<KeyValuePair<string, string>> buildingNumberInfo = new List<KeyValuePair<string, string>>();
            if (!inputs.RefreshFlag)
            {
                buildingInfo = _context.HousingUnit.Where(i => i.FacilityId == inputs.FacilityId || inputs.FacilityId == 0)
                .Select(s => s.HousingUnitLocation).Distinct().ToArray();
                medAlertinfo = _context.Lookup.Where(x => x.LookupType == LookupConstants.MEDFLAG)
                 .Select(a => new LookupVm
                 {
                     LookupDescription = a.LookupDescription,
                     
                     LookupIndex = a.LookupIndex
                 }).ToList();
                buildingNumberInfo = _context.HousingUnit
                  .Where(i => i.FacilityId == inputs.FacilityId && buildingInfo.Contains(i.HousingUnitLocation))
                  .Select(i => new KeyValuePair<string, string>(i.HousingUnitLocation, i.HousingUnitNumber)).Distinct().ToList();
            }

            List<MedicalAlertInmateDataVm> lstMedicalFlagViewerVm = _context.PersonFlag.Where(w => 
               w.DeleteFlag == 0 && w.MedicalFlagIndex.HasValue)
               .Select(s => new MedicalAlertInmateDataVm
               {
                   MedicalFlagIndex = s.MedicalFlagIndex,
                   PersonId = s.PersonId,
                   FlagExpire = s.FlagExpire,
                   FlagNote = s.FlagNote
               }).ToList();

            int[] personIds = lstMedicalFlagViewerVm.Select(s => s.PersonId).ToArray();
            List<PersonInfoVm> lstPersonInfoVm = _context.Inmate.Where(w => personIds.Contains(w.PersonId)
             && w.InmateActive == 1 && (inputs.FacilityId == 0 || w.FacilityId == inputs.FacilityId))
            .Select(s => new PersonInfoVm
            {
                InmateId = s.InmateId,
                InmateActive = s.InmateActive == 1,
                InmateNumber = s.InmateNumber,
                FacilityId = s.FacilityId,
                PersonId = s.PersonId,
                HousingUnitId = s.HousingUnitId ?? 0
            }).ToList();
            lstMedicalFlagViewerVm.ForEach(item =>
            {
                PersonInfoVm inmateIds = lstPersonInfoVm.FirstOrDefault(f => f.PersonId == item.PersonId);
                if (inmateIds != null)
                {
                    item.InmateId = inmateIds.InmateId ?? 0;
                }           
            });
            lstMedicalFlagViewerVm = lstMedicalFlagViewerVm.Where(w => w.InmateId != 0).ToList();
            List<Person> lstPerson = _context.Person.Where(i => lstPersonInfoVm.Select(s => s.PersonId).Contains(i.PersonId))
           .Select(s => new Person
           {
               PersonId = s.PersonId,
               PersonLastName = s.PersonLastName,
               PersonFirstName = s.PersonFirstName,
               PersonMiddleName = s.PersonMiddleName,
               PersonSuffix = s.PersonSuffix
           }).ToList();
            lstPersonInfoVm.ForEach(item =>
            {
                Person person = lstPerson.FirstOrDefault(i => i.PersonId == item.PersonId);
                if (person == null) return;
                item.PersonId = person.PersonId;
                item.PersonLastName = person.PersonLastName;
                item.PersonFirstName = person.PersonFirstName;
                item.PersonMiddleName = person.PersonMiddleName;
                item.PersonSuffix = person.PersonSuffix;
            });

            int[] housingUnitIds = lstPersonInfoVm.Select(s => s.HousingUnitId).ToArray();
            List<HousingUnit> lsthousingunit = _context.HousingUnit.Where(w => housingUnitIds.Contains(w.HousingUnitId))
            .Select(s => new HousingUnit
            {
                HousingUnitId = s.HousingUnitId,
                HousingUnitNumber = s.HousingUnitNumber,
                HousingUnitLocation = s.HousingUnitLocation,
                HousingUnitBedLocation = s.HousingUnitBedLocation,
                HousingUnitBedNumber = s.HousingUnitBedNumber
            }).ToList();

            int[] medicalflagindexs = lstMedicalFlagViewerVm.Where(w => w.MedicalFlagIndex > 0).Select(s => s.MedicalFlagIndex ?? 0).ToArray();
            List<Lookup> lstlookup = _context.Lookup.Where(w => medicalflagindexs.Contains(w.LookupIndex)
                && w.LookupType == LookupConstants.MEDFLAG)
                .Select(s => new Lookup
                {
                    LookupDescription = s.LookupDescription,
                    LookupIndex = s.LookupIndex
                }).ToList();

            lstMedicalFlagViewerVm.ForEach(item =>
            {
                PersonInfoVm personInfoVm = lstPersonInfoVm
                .SingleOrDefault(s => s.PersonId == item.PersonId);
                item.PersonInfo = personInfoVm ?? new PersonInfoVm();
                HousingUnit housingUnit = lsthousingunit.SingleOrDefault(s => s.HousingUnitId == item.PersonInfo.HousingUnitId);
                if (housingUnit != null)
                {
                    item.HousingUnitNumber = housingUnit.HousingUnitNumber;
                    item.HousingUnitLocation = housingUnit.HousingUnitLocation;
                    item.HousingUnitBedNumber = housingUnit.HousingUnitBedNumber;
                    item.HousingUnitBedLocation = housingUnit.HousingUnitBedLocation;
                }
                Lookup lookupInfo = lstlookup.SingleOrDefault(s => s.LookupIndex == item.MedicalFlagIndex);
                item.MedAlertDescription = lookupInfo?.LookupDescription;
            });
            lstMedicalFlagViewerVm = lstMedicalFlagViewerVm.Where(w => w.MedAlertDescription != null 
            && (string.IsNullOrEmpty(inputs.Building) || w.HousingUnitLocation == inputs.Building)).OrderBy(o => o.PersonInfo.PersonFirstName).ToList();

            MedicalAlertInmateVm medicalAlertInmateVm = new MedicalAlertInmateVm
            {
                MedicalAlertDetails = lstMedicalFlagViewerVm,
                BuildingInfo = buildingInfo,
                MedAlertinfo = medAlertinfo,
                BuildingNumberInfo = buildingNumberInfo
            };

            return medicalAlertInmateVm;

        }
    }
}