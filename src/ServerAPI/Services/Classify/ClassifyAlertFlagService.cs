using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GenerateTables.Models;
using ServerAPI.ViewModels;
using ServerAPI.Utilities;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using ServerAPI.Policies;

namespace ServerAPI.Services
{
    public class ClassifyAlertFlagService : IClassifyAlertFlagService
    {
        private readonly AAtims _context;

        public ClassifyAlertFlagService(AAtims context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
        }

        public ClassifyAlertVm GetClassifyAlert(KeepSepSearchVm keepSepSearch)
        {

            ClassifyAlertVm classifyAlert = new ClassifyAlertVm();

            List<Lookup> lookupList = _context.Lookup
                          .Where(l => (l.LookupType == LookupConstants.PERSONCAUTION
                                || l.LookupType == LookupConstants.TRANSCAUTION
                                || l.LookupType == LookupConstants.DIET)
                                && l.LookupInactive == 0)
                                .OrderByDescending(o => o.LookupOrder)
                                .ThenBy(t => t.LookupDescription).ToList();

            IQueryable<PersonFlag> inmateList = _context.PersonFlag
                     .Where(w => w.Person.Inmate.Any(a => a.InmateActive == 1
                            && a.FacilityId == keepSepSearch.FacilityId)
                            && w.DeleteFlag == 0
                            && (keepSepSearch.GenderId == 0 || w.Person.PersonSexLast == keepSepSearch.GenderId)
                            && (keepSepSearch.AssociationId == 0 || w.Person.PersonClassification
                            .Any(a => a.PersonClassificationTypeId == keepSepSearch.AssociationId))
                            && (keepSepSearch.RaceId == 0 || w.Person.PersonRaceLast == keepSepSearch.RaceId)
                            && (string.IsNullOrEmpty(keepSepSearch.InmateCurrentTrack) || w.Person.Inmate
                            .Any(a => a.InmateCurrentTrack == keepSepSearch.InmateCurrentTrack))
                            && (keepSepSearch.HousingUnit.HousingUnitId == 0
                            || w.Person.Inmate.Any(a => a.HousingUnitId == keepSepSearch.HousingUnit.HousingUnitId))
                            && (keepSepSearch.IllegalAlienOnly == false
                            || w.Person.IllegalAlienFlag == keepSepSearch.IllegalAlienOnly)
                            && (keepSepSearch.AlertFLag != AlertFLag.AllFlags
                            || (w.PersonFlagIndex > 0 || w.DietFlagIndex > 0 || w.InmateFlagIndex > 0))
                            && (keepSepSearch.AlertFLag != AlertFLag.AlwaysAlerts || (w.PersonFlagIndex > 0))
                            && (keepSepSearch.AlertFLag != AlertFLag.ActiveInmateAlerts || (w.InmateFlagIndex > 0))
                            && (keepSepSearch.AlertFLag != AlertFLag.Diet || (w.DietFlagIndex > 0)));

            classifyAlert.ClassifyAlertList = inmateList
                     .SelectMany(inm => lookupList
                     .Where(w => (w.LookupIndex == inm.PersonFlagIndex
                            && w.LookupType == LookupConstants.PERSONCAUTION
                            || w.LookupIndex == inm.InmateFlagIndex
                            && w.LookupType == LookupConstants.TRANSCAUTION
                            || w.LookupIndex == inm.DietFlagIndex
                            && w.LookupType == LookupConstants.DIET)),
                            (inm, look) => new ClassifyAlertVm
                            {
                                InmateId = inm.Person.Inmate.Any() ? inm.Person.Inmate.FirstOrDefault().InmateId : 0,
                                InmateNumber = inm.Person.Inmate.FirstOrDefault().InmateNumber,
                                LookupDescription = look.LookupDescription,
                                LookupType = look.LookupType,
                                Person = new PersonVm
                                {
                                    PersonId = inm.PersonId,
                                    PersonLastName = inm.Person.PersonLastName,
                                    PersonFirstName = inm.Person.PersonFirstName,
                                    PersonMiddleName = inm.Person.PersonMiddleName,
                                    PersonSuffix = inm.Person.PersonSuffix,
                                },
                                HousingUnit = inm.Person.Inmate.Any() ? inm.Person.Inmate.Select(s => new HousingUnitVm
                                {
                                    HousingUnitLocation = s.HousingUnit.HousingUnitLocation,
                                    HousingUnitNumber = s.HousingUnit.HousingUnitNumber,
                                    HousingUnitBedNumber = s.HousingUnit.HousingUnitBedNumber,
                                    HousingUnitBedLocation = s.HousingUnit.HousingUnitBedLocation,
                                }).FirstOrDefault() : new HousingUnitVm(),
                                InmateCurrentTrack = inm.Person.Inmate.Select(s => s.InmateCurrentTrack).FirstOrDefault(),
                                Flag = look.LookupIndex,
                                FlagNote = inm.FlagNote
                            }).ToList();

            classifyAlert.HousingList = GetClassifyAlertHousingCount(classifyAlert.ClassifyAlertList);
            classifyAlert.Lookup = GetFlagCount(classifyAlert.ClassifyAlertList);
            return classifyAlert;

        }
        public List<HousingUnitVm> GetClassifyAlertHousingCount(List<ClassifyAlertVm> classifyAlerts) => classifyAlerts
               .GroupBy(g => new
               {
                   g.HousingUnit.HousingUnitLocation,
                   g.HousingUnit.HousingUnitNumber
               })
               .Select(s => new HousingUnitVm
               {
                   HousingUnitLocation = s.Key.HousingUnitLocation,
                   HousingUnitNumber = s.Key.HousingUnitNumber,
                   Count = s.Count()
               }).ToList();

        public List<LookupVm> GetFlagCount(List<ClassifyAlertVm> classifyAlerts) => classifyAlerts
                .GroupBy(g => new
                {
                    g.LookupDescription,
                    g.LookupType,
                    g.LookupIndex
                })
                .Select(s => new LookupVm
                {
                    LookupDescription = s.Key.LookupDescription,
                    LookupType = s.Key.LookupType,
                    LookupIndex = s.Key.LookupIndex,
                    Count = s.Count()
                }).ToList();

    }

}

























