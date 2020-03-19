﻿using GenerateTables.Models;
using ServerAPI.Utilities;
using ServerAPI.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;

// ReSharper disable once CheckNamespace
namespace ServerAPI.Services
{
    public class HousingConflictService : IHousingConflictService
    {
        private readonly AAtims _context;
        private readonly IInmateService _inmateService;
        private readonly IPhotosService _photos;
        private IQueryable<Inmate> _inmateList;

        public HousingConflictService(AAtims context, IInmateService inmateService, IPhotosService photosService)
        {
            _context = context;
            _inmateService = inmateService;
            _photos = photosService;
        }

        public List<HousingConflictVm> GetInmateHousingConflictVm(HousingInputVm value)
        {
            // To get Inmate details. 
            IQueryable<Inmate> lstInmate = _context.Inmate
                .Where(f => f.FacilityId == value.FacilityId
                            && f.InmateActive == 1);

            IEnumerable<HousingDetail> lstInmateHousings = lstInmate
                .Where(w => w.HousingUnitId.HasValue && w.HousingUnit.HousingUnitListId == value.HousingUnitListId)
                .Select(s => new HousingDetail
                {
                    HousingUnitListId = s.HousingUnit.HousingUnitListId,
                    HousingUnitBedNumber = s.HousingUnit.HousingUnitBedNumber,
                    HousingUnitBedLocation = s.HousingUnit.HousingUnitBedLocation
                });

            //To get Housing Inmate details based on bed number
            if (!(value.HousingBedNumber is null))
            {
                // To get Housing Inmate details based on bed location.
                lstInmateHousings = lstInmateHousings.Where(w =>
                    w.HousingUnitListId == value.HousingUnitListId
                    && w.HousingUnitBedNumber == value.HousingBedNumber);
            }

            // To get Housing Inmate details based on bed location.
            if (!(value.HousingBedLocation is null))
            {
                // To get Housing Inmate details based on bed location.
                lstInmateHousings = lstInmateHousings.Where(w =>
                    w.HousingUnitListId == value.HousingUnitListId
                    && w.HousingUnitBedLocation ==
                    value.HousingBedLocation);
            }

            IEnumerable<HousingUnit> lstHousingUnit = _context.HousingUnit
                .Where(h => h.FacilityId == value.FacilityId
                            && h.HousingUnitListId == value.HousingUnitListId
                            && (!h.HousingUnitInactive.HasValue ||
                                h.HousingUnitInactive == 0));

            if (!(value.HousingBedNumber is null))
            {
                // To get Housing Unit details based on bed location.
                lstHousingUnit =
                    lstHousingUnit.Where(h => h.HousingUnitBedNumber == value.HousingBedNumber);
            }

            if (!(value.HousingBedLocation is null))
            {
                // To get Housing Unit details based on bed location.
                lstHousingUnit =
                    lstHousingUnit.Where(h => h.HousingUnitBedLocation == value.HousingBedLocation);
            }

            List<HousingUnit> housingUnits = lstHousingUnit.ToList();
            HousingCapacityVm housingCapacity = new HousingCapacityVm
            {
                CurrentCapacity = housingUnits.ToList().Sum(c => c.HousingUnitActualCapacity ?? 0),
                OutofService = housingUnits.ToList().Sum(c => c.HousingUnitOutOfService ?? 0),
                Assigned = lstInmateHousings.ToList().Count
            };

            string inmateClassify = _context.Inmate.Where(i => i.InmateId == value.InmateId)
                .Select(s => s.InmateClassification.InmateClassificationReason).SingleOrDefault();

            List<string> classify = housingUnits.Where(w => w.HousingUnitClassifyRecString != null)
                .Select(se => se.HousingUnitClassifyRecString.Split(','))
                .SelectMany(se => se).Distinct().ToList();

            List<HousingConflictVm> lstHousingConflictDetails = new List<HousingConflictVm>();

            if (classify.Any() && !(inmateClassify is null) && !classify.Contains(inmateClassify) &&
                inmateClassify != RequestType.PENDINGREQUEST)
            {
                // To get Housing out of service details.
                lstHousingConflictDetails.Add(new HousingConflictVm
                {
                    ConflictType = ConflictTypeConstants.HOUSINGCLASSIFYCONFLICT,
                    Description = inmateClassify
                });
            }

            List<string> flagString = housingUnits.Where(w => !string.IsNullOrEmpty(w.HousingUnitFlagString))
                .SelectMany(s => s.HousingUnitFlagString.Split(',')).ToList();

            // To get the person flag list
            IQueryable<PersonFlag> personFlag = _context.PersonFlag
                .Where(w => w.Person.Inmate.Any(s => s.InmateId == value.InmateId)
                            && w.DeleteFlag == 0
                            && (w.InmateFlagIndex > 0 ||
                                w.PersonFlagIndex > 0));

            List<string> housingFlags = personFlag.SelectMany(
                    p => _context.Lookup.Where(w =>
                        w.LookupInactive == 0 && w.LookupFlag7 == 1 && w.LookupType == LookupConstants.PERSONCAUTION &&
                        w.LookupIndex == p.PersonFlagIndex && p.PersonFlagIndex > 0 ||
                        w.LookupType == LookupConstants.TRANSCAUTION
                        && w.LookupIndex == p.InmateFlagIndex &&
                        p.InmateFlagIndex > 0 && w.LookupFlag7 == 1), (p, l) => l.LookupDescription.Replace("'", ""))
                .ToList();

            //V2-3727 JMS_Recommend housing_Not displaying entire list
            if (housingFlags.Any() && (!flagString.Any() ||
                                       flagString.Any() && !housingFlags.All(a => flagString.Any(o => o == a))))
            {
                // To get Housing out of service details.
                lstHousingConflictDetails.Add(new HousingConflictVm
                {
                    ConflictType = ConflictTypeConstants.HOUSINGFLAGCONFLICT,
                    ListDescription = flagString.Any() ? housingFlags.Except(flagString).ToList() : housingFlags
                });
            }

            if (housingCapacity.OutofService > 0)
            {
                // To get Housing out of service details.
                lstHousingConflictDetails.Add(new HousingConflictVm
                {
                    ConflictType = ConflictTypeConstants.HOUSINGOUTOFSERVICE,
                });
            }

            // To get site option value of no house class.
            int siteOptionsOverCap = _context.SiteOptions
                .Count(w => w.SiteOptionsName == SiteOptionsConstants.DONOTALLOWOVERCAPACITYHOUSING
                            && w.SiteOptionsStatus == "1" &&
                            w.SiteOptionsValue == SiteOptionsConstants.OFF);

            if (housingCapacity.Assigned >= housingCapacity.CurrentCapacity)
            {
                // To get Housing over capacity details.
                lstHousingConflictDetails.Add(new HousingConflictVm
                {
                    ConflictType = ConflictTypeConstants.HOUSINGOVERCAPACITY,
                    Immovable = siteOptionsOverCap <= 0
                });
            }

            // To get site option value of no house class.
            int siteOptionsNoHouseClass = _context.SiteOptions
                .Count(w => w.SiteOptionsName == SiteOptionsConstants.NOHOUSECLASS
                            && w.SiteOptionsStatus == "1" &&
                            w.SiteOptionsValue == SiteOptionsConstants.ON);

            if (siteOptionsNoHouseClass > 0)
            {
                // To get unclassified inmate details.
                if (inmateClassify is null)
                {
                    lstHousingConflictDetails.Add(new HousingConflictVm
                    {
                        ConflictType = ConflictTypeConstants.UNCLASSIFIEDINMATE,
                        Immovable = true
                    });
                }
            }

            // To get site option value of no house book.
            int siteOptionsNoHouseBook = _context.SiteOptions
                .Count(w => w.SiteOptionsName == SiteOptionsConstants.NOHOUSEBOOK
                            && w.SiteOptionsStatus == "1"
                            && w.SiteOptionsValue == SiteOptionsConstants.ON);

            if (siteOptionsNoHouseBook > 0)
            {
                // To get booking not complete details.
                HousingConflictVm noBookingConflictDetail = _context.IncarcerationArrestXref
                    .Where(i => i.Incarceration.InmateId == value.InmateId
                                && !i.Incarceration.BookCompleteFlag.HasValue && i.Arrest.ArrestActive == 1)
                    .Select(i => new HousingConflictVm
                    {
                        ConflictType = ConflictTypeConstants.BOOKINGNOTCOMPLETE
                    }).FirstOrDefault();

                if (!(noBookingConflictDetail is null))
                {
                    lstHousingConflictDetails.Add(noBookingConflictDetail);
                }
            }

            // To get the unclassified inmate details.
            HousingConflictVm unClassifiedConflict = _context.InmateClassification
                .Where(i => i.InmateId == value.InmateId
                            && i.InmateClassificationReason == RequestType.PENDINGREQUEST
                            && (i.InmateClassificationType == ClassTypeConstants.RECLASSIFICATION
                                || i.InmateClassificationType == ClassTypeConstants.INITIAL))
                .Select(i => new HousingConflictVm
                {
                    ConflictType = ConflictTypeConstants.UNCLASSIFIEDINMATE,
                    Immovable = false
                }).FirstOrDefault();

            if (!(unClassifiedConflict is null))
            {
                lstHousingConflictDetails.Add(unClassifiedConflict);
            }

            // To get the lookup value.
            Dictionary<int, string> dict = _context.Lookup
                .Where(l => l.LookupInactive == 0 && l.LookupType == LookupConstants.SEX)
                .Select(s => new {s.LookupIndex, s.LookupDescription})
                .ToDictionary(p => p.LookupIndex, p => p.LookupDescription);

            // To get the gender details that are based on housing.
            List<KeyValuePair<int, int>> inmateListSexLast = housingUnits
                .Where(s => s.HousingUnitSex.HasValue && s.HousingUnitSex > 0
                                                      && dict.Keys.Contains(s.HousingUnitSex.Value))
                .Select(s => new KeyValuePair<int, int>(s.HousingUnitSex.Value, 1))
                .ToList();

            if (string.IsNullOrEmpty(value.HousingBedNumber) && string.IsNullOrEmpty(value.HousingBedLocation))
            {
                // To get inmate gender details details based on bed number.
                inmateListSexLast.AddRange(lstInmate
                    .Where(h => h.HousingUnitId.HasValue && h.Person.PersonSexLast.HasValue
                                                         && h.Person.PersonSexLast > 0
                                                         && dict.Keys.Contains(h.Person.PersonSexLast.Value)
                                                         && h.HousingUnit.HousingUnitListId == value.HousingUnitListId)
                    .Select(h => new KeyValuePair<int, int>(h.Person.PersonSexLast.Value, 2)).Distinct());
            }
            else if (!string.IsNullOrEmpty(value.HousingBedNumber) && string.IsNullOrEmpty(value.HousingBedLocation))
            {
                // To get inmate gender details details based on bed number.
                inmateListSexLast.AddRange(lstInmate
                    .Where(h => h.HousingUnitId.HasValue && h.Person.PersonSexLast.HasValue
                                                         && h.Person.PersonSexLast > 0
                                                         && dict.Keys.Contains(h.Person.PersonSexLast.Value)
                                                         && h.HousingUnit.HousingUnitListId == value.HousingUnitListId
                                                         && h.HousingUnit.HousingUnitBedNumber ==
                                                         value.HousingBedNumber)
                    .Select(h => new KeyValuePair<int, int>(h.Person.PersonSexLast.Value, 2)).Distinct());
            }
            else
            {
                // To get inmate gender details details based on bed location.
                inmateListSexLast.AddRange(lstInmate
                    .Where(h => h.HousingUnitId.HasValue && h.Person.PersonSexLast.HasValue
                                                         && h.Person.PersonSexLast > 0
                                                         && dict.Keys.Contains(h.Person.PersonSexLast.Value)
                                                         && h.HousingUnit.HousingUnitListId == value.HousingUnitListId
                                                         && h.HousingUnit.HousingUnitBedNumber == value.HousingBedNumber
                                                         && h.HousingUnit.HousingUnitBedLocation ==
                                                         value.HousingBedLocation)
                    .Select(h => new KeyValuePair<int, int>(h.Person.PersonSexLast.Value, 2)).Distinct());
            }

            inmateListSexLast = inmateListSexLast.Distinct().ToList();

            // To get the gender details of current inmate.
            int gender = _context.Inmate.Where(w => w.InmateId == value.InmateId
                                                    && w.Person.PersonSexLast.HasValue)
                .Select(s => s.Person.PersonSexLast.Value).SingleOrDefault();

            if (gender > 0 && inmateListSexLast.Count > 0)
            {
                // To get the gender conflict details.
                lstHousingConflictDetails.AddRange(inmateListSexLast.Where(g => gender != g.Key
                                                                                && g.Value == 2)
                    .Select(g => new HousingConflictVm
                        {
                            ConflictType = ConflictTypeConstants.INMATEGENDERHOUSINGCONFLICT,
                            Description = dict.Where(w => w.Key == g.Key)
                                .Select(s => s.Value).FirstOrDefault()
                        }
                    ));

                lstHousingConflictDetails.AddRange(inmateListSexLast.Where(g => gender != g.Key
                                                                                && g.Value == 1)
                    .Select(g => new HousingConflictVm
                        {
                            ConflictType = ConflictTypeConstants.INMATEGENDERCONFLICT
                        }
                    ));
            }

            List<HousingConflictVm> podHousingConflict = GetHousingKeepSeparate(value.InmateId, value.FacilityId)
                .Where(w => w.Housing.HousingUnitListId == value.HousingUnitListId).ToList();

            List<HousingConflictVm> housingConflictVms = podHousingConflict
                .Where(w => w.Housing.HousingUnitBedNumber == value.HousingBedNumber).Distinct().ToList();

            if (value.HousingBedLocation is null)
            {

                if (podHousingConflict.Count > 0)
                {
                    if (_context.HousingUnit.Any(a => a.HousingUnitListId == value.HousingUnitListId
                                                      && a.HousingUnitBedNumber != null &&
                                                      a.HousingUnitBedNumber == value.HousingBedNumber &&
                                                      a.HousingUnitListBedGroup != null
                                                      && a.HousingUnitListBedGroup.DeleteFlag == 0 &&
                                                      a.HousingUnitListBedGroupId > 0
                                                      && a.HousingUnitBedGroupKeepsepConflictCheck == 1))
                    {
                        int housingUnitListBedGroupId = _context.HousingUnit.Where(a =>
                                a.HousingUnitListId == value.HousingUnitListId
                                && a.HousingUnitBedNumber != null && a.HousingUnitBedNumber == value.HousingBedNumber &&
                                a.HousingUnitListBedGroup != null
                                && a.HousingUnitListBedGroup.DeleteFlag == 0 && a.HousingUnitListBedGroupId > 0
                                && a.HousingUnitBedGroupKeepsepConflictCheck == 1)
                            .Select(s => s.HousingUnitListBedGroupId ?? 0).SingleOrDefault();

                        List<HousingConflictVm> bedGroupHousingConflict = podHousingConflict
                            .Where(w => w.HousingBedGroupId == housingUnitListBedGroupId).Select(s =>
                                new HousingConflictVm
                                {
                                    ConflictType = s.ConflictType == KeepSepLabel.ASSOCKEEPSEPASSOC
                                        ? KeepSepLabel.ASSOCKEEPSEPASSOCBEDGROUP
                                        : s.ConflictType == KeepSepLabel.ASSOCKEEPSEPINMATE
                                            ? KeepSepLabel.ASSOCKEEPSEPINMATEBEDGROUP
                                            : s.ConflictType == KeepSepLabel.ASSOCKEEPSEPSUBSET
                                                ? KeepSepLabel.ASSOCKEEPSEPSUBSETBEDGROUP
                                                : s.ConflictType == KeepSepLabel.INMATEKEEPSEPASSOC
                                                    ? KeepSepLabel.INMATEKEEPSEPASSOCBEDGROUP
                                                    : s.ConflictType == KeepSepLabel.INMATEKEEPSEPINMATE
                                                        ? KeepSepLabel.INMATEKEEPSEPINMATEBEDGROUP
                                                        : s.ConflictType == KeepSepLabel.INMATEKEEPSEPSUBSET
                                                            ? KeepSepLabel.INMATEKEEPSEPSUBSETBEDGROUP
                                                            : s.ConflictType == KeepSepLabel.SUBSETKEEPSEPASSOC
                                                                ? KeepSepLabel.SUBSETKEEPSEPASSOCBEDGROUP
                                                                : s.ConflictType == KeepSepLabel.SUBSETKEEPSEPINMATE
                                                                    ? KeepSepLabel.SUBSETKEEPSEPINMATEBEDGROUP
                                                                    : KeepSepLabel.SUBSETKEEPSEPSUBSETBEDGROUP,
                                    KeepSepPersonId = s.KeepSepPersonId,
                                    KeepSepInmateId = s.KeepSepInmateId,
                                    KeepSepInmateNumber = s.KeepSepInmateNumber,
                                    PersonId = s.PersonId,
                                    InmateId = s.InmateId,
                                    InmateNumber = s.InmateNumber,
                                    KeepSepType = s.KeepSepType,
                                    ConflictDescription = s.ConflictDescription,
                                    Housing = s.Housing,
                                    AssignConflictType = s.AssignConflictType,
                                    KeepSepAssoc1 = s.KeepSepAssoc1,
                                    KeepSepAssoc2 = s.KeepSepAssoc2,
                                    KeepSepAssocSubset1 = s.KeepSepAssocSubset1,
                                    KeepSepAssocSubset2 = s.KeepSepAssocSubset2,
                                    PersonLastName = s.PersonLastName,
                                    PersonMiddleName = s.PersonMiddleName,
                                    PersonFirstName = s.PersonFirstName,
                                    PhotoFilePath = s.PhotoFilePath,
                                    KeepSepPersonLastName = s.KeepSepPersonLastName,
                                    KeepSepPersonFirstName = s.KeepSepPersonFirstName,
                                    KeepSepPersonMiddleName = s.KeepSepPersonMiddleName
                                }).ToList();

                        housingConflictVms.AddRange(bedGroupHousingConflict.OrderBy(o => o.ConflictType));
                    }
                }
            }
            else
            {

                housingConflictVms = housingConflictVms
                    .Where(w => w.Housing.HousingUnitBedNumber == value.HousingBedNumber
                                && w.Housing.HousingUnitBedLocation == value.HousingBedLocation).ToList();

                if (podHousingConflict.Count > 0)
                {
                    if (_context.HousingUnit.Any(a =>
                        a.HousingUnitListId == value.HousingUnitListId
                        && a.HousingUnitBedLocation != null
                        && a.HousingUnitBedNumber != null
                        && a.HousingUnitBedLocation ==
                        value.HousingBedLocation &&
                        a.HousingUnitBedNumber == value.HousingBedNumber
                        && a.HousingUnitListBedGroup != null
                        && a.HousingUnitListBedGroup.DeleteFlag == 0 &&
                        a.HousingUnitListBedGroupId > 0
                        && a.HousingUnitBedGroupKeepsepConflictCheck ==
                        1))
                    {

                        int housingUnitListBedGroupId = _context.HousingUnit.Where(a =>
                                a.HousingUnitListId == value.HousingUnitListId
                                && a.HousingUnitBedLocation != null
                                && a.HousingUnitBedNumber != null &&
                                a.HousingUnitBedLocation == value.HousingBedLocation
                                && a.HousingUnitBedNumber == value.HousingBedNumber && a.HousingUnitListBedGroup != null
                                && a.HousingUnitListBedGroup.DeleteFlag == 0 && a.HousingUnitListBedGroupId > 0
                                && a.HousingUnitBedGroupKeepsepConflictCheck == 1)
                            .Select(s => s.HousingUnitListBedGroupId ?? 0).SingleOrDefault();

                        List<HousingConflictVm> bedGroupHousingConflict = podHousingConflict
                            .Where(w => w.HousingBedGroupId == housingUnitListBedGroupId).Select(s =>
                                new HousingConflictVm
                                {
                                    ConflictType = s.ConflictType == KeepSepLabel.ASSOCKEEPSEPASSOC
                                        ? KeepSepLabel.ASSOCKEEPSEPASSOCBEDGROUP
                                        : s.ConflictType == KeepSepLabel.ASSOCKEEPSEPINMATE
                                            ? KeepSepLabel.ASSOCKEEPSEPINMATEBEDGROUP
                                            : s.ConflictType == KeepSepLabel.ASSOCKEEPSEPSUBSET
                                                ? KeepSepLabel.ASSOCKEEPSEPSUBSETBEDGROUP
                                                : s.ConflictType == KeepSepLabel.INMATEKEEPSEPASSOC
                                                    ? KeepSepLabel.INMATEKEEPSEPASSOCBEDGROUP
                                                    : s.ConflictType == KeepSepLabel.INMATEKEEPSEPINMATE
                                                        ? KeepSepLabel.INMATEKEEPSEPINMATEBEDGROUP
                                                        : s.ConflictType == KeepSepLabel.INMATEKEEPSEPSUBSET
                                                            ? KeepSepLabel.INMATEKEEPSEPSUBSETBEDGROUP
                                                            : s.ConflictType == KeepSepLabel.SUBSETKEEPSEPASSOC
                                                                ? KeepSepLabel.SUBSETKEEPSEPASSOCBEDGROUP
                                                                : s.ConflictType == KeepSepLabel.SUBSETKEEPSEPINMATE
                                                                    ? KeepSepLabel.SUBSETKEEPSEPINMATEBEDGROUP
                                                                    : KeepSepLabel.SUBSETKEEPSEPSUBSETBEDGROUP,
                                    KeepSepPersonId = s.KeepSepPersonId,
                                    KeepSepInmateId = s.KeepSepInmateId,
                                    KeepSepInmateNumber = s.KeepSepInmateNumber,
                                    PersonId = s.PersonId,
                                    InmateId = s.InmateId,
                                    InmateNumber = s.InmateNumber,
                                    KeepSepType = s.KeepSepType,
                                    ConflictDescription = s.ConflictDescription,
                                    Housing = s.Housing,
                                    AssignConflictType = s.AssignConflictType,
                                    KeepSepAssoc1 = s.KeepSepAssoc1,
                                    KeepSepAssoc2 = s.KeepSepAssoc2,
                                    KeepSepAssocSubset1 = s.KeepSepAssocSubset1,
                                    KeepSepAssocSubset2 = s.KeepSepAssocSubset2,
                                    PersonLastName = s.PersonLastName,
                                    PersonMiddleName = s.PersonMiddleName,
                                    PersonFirstName = s.PersonFirstName,
                                    PhotoFilePath = s.PhotoFilePath,
                                    KeepSepPersonLastName = s.KeepSepPersonLastName,
                                    KeepSepPersonFirstName = s.KeepSepPersonFirstName,
                                    KeepSepPersonMiddleName = s.KeepSepPersonMiddleName
                                }).ToList();

                        housingConflictVms.AddRange(bedGroupHousingConflict.OrderBy(o => o.ConflictType));
                    }
                }
            }

            if (podHousingConflict.Count > 0)
            {
                if (_context.HousingUnit.Any(a =>
                    a.HousingUnitListId == value.HousingUnitListId && a.HousingUnitBedNumber != null
                                                                   && a.HousingUnitBedNumber ==
                                                                   value.HousingBedNumber &&
                                                                   a.HousingUnitNumberKeepsepConflictCheck == 1))
                {
                    List<HousingConflictVm> podConflict = podHousingConflict.Select(s =>
                        new HousingConflictVm
                        {
                            ConflictType = s.ConflictType == KeepSepLabel.ASSOCKEEPSEPASSOC
                                ? KeepSepLabel.ASSOCKEEPSEPASSOCBEDPOD
                                : s.ConflictType == KeepSepLabel.ASSOCKEEPSEPINMATE
                                    ? KeepSepLabel.ASSOCKEEPSEPINMATEBEDPOD
                                    : s.ConflictType == KeepSepLabel.ASSOCKEEPSEPSUBSET
                                        ? KeepSepLabel.ASSOCKEEPSEPSUBSETBEDPOD
                                        : s.ConflictType == KeepSepLabel.INMATEKEEPSEPASSOC
                                            ? KeepSepLabel.INMATEKEEPSEPASSOCBEDPOD
                                            : s.ConflictType == KeepSepLabel.INMATEKEEPSEPINMATE
                                                ? KeepSepLabel.INMATEKEEPSEPINMATEBEDPOD
                                                : s.ConflictType == KeepSepLabel.INMATEKEEPSEPSUBSET
                                                    ? KeepSepLabel.INMATEKEEPSEPSUBSETBEDPOD
                                                    : s.ConflictType == KeepSepLabel.SUBSETKEEPSEPASSOC
                                                        ? KeepSepLabel.SUBSETKEEPSEPASSOCBEDPOD
                                                        : s.ConflictType == KeepSepLabel.SUBSETKEEPSEPINMATE
                                                            ? KeepSepLabel.SUBSETKEEPSEPINMATEBEDPOD
                                                            : KeepSepLabel.SUBSETKEEPSEPSUBSETBEDPOD,
                            KeepSepPersonId = s.KeepSepPersonId,
                            KeepSepInmateId = s.KeepSepInmateId,
                            KeepSepInmateNumber = s.KeepSepInmateNumber,
                            PersonId = s.PersonId,
                            InmateId = s.InmateId,
                            InmateNumber = s.InmateNumber,
                            KeepSepType = s.KeepSepType,
                            ConflictDescription = s.ConflictDescription,
                            Housing = s.Housing,
                            AssignConflictType = s.AssignConflictType,
                            KeepSepAssoc1 = s.KeepSepAssoc1,
                            KeepSepAssoc2 = s.KeepSepAssoc2,
                            KeepSepAssocSubset1 = s.KeepSepAssocSubset1,
                            KeepSepAssocSubset2 = s.KeepSepAssocSubset2,
                            PersonLastName = s.PersonLastName,
                            PersonMiddleName = s.PersonMiddleName,
                            PersonFirstName = s.PersonFirstName,
                            PhotoFilePath = s.PhotoFilePath,
                            KeepSepPersonLastName = s.KeepSepPersonLastName,
                            KeepSepPersonFirstName = s.KeepSepPersonFirstName,
                            KeepSepPersonMiddleName = s.KeepSepPersonMiddleName
                        }).ToList();

                    housingConflictVms.AddRange(podConflict.OrderBy(o => o.ConflictType));

                }
            }

            lstHousingConflictDetails.AddRange(housingConflictVms);

            HousingConflictVm housingTransferTasks = _inmateService
                .GetInmateTasks(value.InmateId, TaskValidateType.HousingAssignFromTransfer)
                .Select(i => new HousingConflictVm
                {
                    ConflictType = ConflictTypeConstants.TASKNOTCOMPLETE,
                    ConflictDescription = i.Value
                }).FirstOrDefault();

            if (!(housingTransferTasks is null))
            {
                lstHousingConflictDetails.Add(housingTransferTasks);
            }

            List<InmateSearchVm> lstPersonFlag = lstInmate.SelectMany(s => _context.PersonFlag
                .Where(w => s.HousingUnit.HousingUnitListId == value.HousingUnitListId
                            && w.DeleteFlag == 0 && (w.InmateFlagIndex > 0 || w.PersonFlagIndex > 0)
                            && w.PersonId == s.PersonId), (s, p) => new InmateSearchVm
            {
                PersonFlagIndex = p.PersonFlagIndex,
                InmateFlagIndex = p.InmateFlagIndex,
                FacilityId = s.FacilityId,
                PersonId = s.PersonId,
                HousingDetail = new HousingDetail
                {
                    HousingUnitId = s.HousingUnitId ?? 0,
                    HousingUnitListId = s.HousingUnit.HousingUnitListId,
                    HousingUnitLocation = s.HousingUnit.HousingUnitLocation,
                    HousingUnitNumber = s.HousingUnit.HousingUnitNumber,
                    HousingUnitBedNumber = s.HousingUnit.HousingUnitBedNumber,
                    HousingUnitBedLocation = s.HousingUnit.HousingUnitBedLocation
                }
            }).ToList();


            if (!(value.HousingBedNumber is null))
            {
                lstPersonFlag = lstPersonFlag.Where(w => w.HousingDetail.HousingUnitBedNumber == value.HousingBedNumber)
                    .ToList();
            }

            List<Lookup> lookupList = _context.Lookup.Where(w => w.LookupInactive == 0
                                                                 && w.LookupFlag6 == 1 &&
                                                                 w.LookupType == LookupConstants.PERSONCAUTION
                                                                 || w.LookupType == LookupConstants.TRANSCAUTION
                                                                 && w.LookupFlag6 == 1)
                .OrderByDescending(o => o.LookupOrder).ThenBy(t => t.LookupDescription).ToList();

            List<InmateSearchVm> totalSepDetails = lstPersonFlag.SelectMany(p => lookupList.Where(w =>
                    w.LookupInactive == 0 && w.LookupFlag6 == 1 &&
                    w.LookupType == LookupConstants.PERSONCAUTION && w.LookupIndex == p.PersonFlagIndex &&
                    p.PersonFlagIndex > 0
                    || w.LookupType == LookupConstants.TRANSCAUTION
                    && w.LookupIndex == p.InmateFlagIndex
                    && p.InmateFlagIndex > 0
                    && w.LookupFlag6 == 1
                ), (p, look) => new InmateSearchVm
                {
                    FacilityId = p.FacilityId,
                    PersonId = p.PersonId,
                    HousingDetail = p.HousingDetail
                }
            ).ToList();


            if (totalSepDetails.Any())
            {
                List<HousingConflictVm> conflictTotalSep = totalSepDetails.Select(s => new HousingConflictVm
                {
                    ConflictType = ConflictTypeConstants.TOTALSEPHOUSINGCONFLICT,
                    Housing = s.HousingDetail
                }).ToList();

                lstHousingConflictDetails.AddRange(conflictTotalSep);
            }

            // Total separation inmate conflict
            List<PersonFlag> inmateSepFlag = personFlag.Where(
                w => w.Person.Inmate.Any(a => a.InmateActive == 1)).ToList();

            int inmateSepCount = GetSeparationCount(inmateSepFlag);

            if (inmateSepCount > 0)
            {
                int inmateHouseCount;
                if (value.HousingBedNumber is null)
                {
                    inmateHouseCount = lstInmate.Count(
                        w => w.HousingUnit.HousingUnitListId == value.HousingUnitListId);
                }
                else
                {
                    inmateHouseCount = lstInmate.Count(
                        w => w.HousingUnit.HousingUnitListId == value.HousingUnitListId
                             && w.HousingUnit.HousingUnitBedNumber == value.HousingBedNumber);
                }

                if (inmateHouseCount > 0)
                {
                    lstHousingConflictDetails.Add(new HousingConflictVm
                    {
                        ConflictType = ConflictTypeConstants.TOTALSEPINMATECONFLICT
                    });
                }
                else
                {
                    if (value.ViewerFlag)
                    {
                        lstHousingConflictDetails.Add(new HousingConflictVm
                        {
                            ConflictType = ConflictTypeConstants.TOTALSEPBATCHHOUSINGCONFLICT
                        });
                    }
                }
            }

            // Bind housing rule and classify conflicts
            List<HousingConflictVm> lstHousingRuleAndClassifyConflictVms =
                GetHousingRuleAndClassifyFlagConflicts(value, inmateClassify);
            lstHousingConflictDetails.AddRange(lstHousingRuleAndClassifyConflictVms);

            lstHousingConflictDetails.ForEach(item =>
            {
                PersonInfoVm inmate = _context.Inmate.Where(w => w.InmateActive == 1 & w.InmateId == value.InmateId)
                    .Select(inm => new PersonInfoVm
                    {
                        InmateId = inm.InmateId,
                        InmateNumber = inm.InmateNumber,
                        PersonLastName = inm.Person.PersonLastName,
                        PersonFirstName = inm.Person.PersonFirstName,
                        PersonMiddleName = inm.Person.PersonMiddleName
                    }).SingleOrDefault();

                if (inmate != null)
                {
                    item.InmateId = value.InmateId;
                    item.InmateNumber = inmate.InmateNumber;
                    item.PersonLastName = inmate.PersonLastName;
                    item.PersonFirstName = inmate.PersonFirstName;
                    item.PersonMiddleName = inmate.PersonMiddleName;
                }
            });
            return lstHousingConflictDetails.OrderBy(inmC => inmC.InmateId).ThenBy(d => d.ConflictType).ToList();
        }

        public List<HousingConflictVm> GetHousingKeepSeparate(int inmateId, int facilityId)
        {
            //Active inmate list   
            IQueryable<Inmate> lstInmate =
                _context.Inmate.Where(p => p.InmateActive == 1 && p.FacilityId == facilityId);

            // To get the person classification     
            IEnumerable<HousingConflictVm> lstPersonClassification = _context.PersonClassification
                .SelectMany(pc => lstInmate.Where(w =>
                    pc.PersonId == w.PersonId
                    && pc.InactiveFlag == 0
                    && pc.PersonClassificationDateFrom < DateTime.Now
                    && (!pc.PersonClassificationDateThru.HasValue
                        || pc.PersonClassificationDateThru >= DateTime.Now)
                    && w.InmateActive == 1), (pc, i) => new HousingConflictVm
                {
                    InmateId = i.InmateId,
                    InmateNumber = i.InmateNumber,
                    PersonId = pc.PersonId,
                    KeepSepAssoc1 = pc.PersonClassificationType,
                    KeepSepAssocSubset1 = pc.PersonClassificationSubset,
                    KeepSepAssoc1Id = pc.PersonClassificationTypeId,
                    KeepSepAssocSubset1Id = pc.PersonClassificationSubsetId,
                    Housing = new HousingDetail
                    {
                        HousingUnitBedNumber = i.HousingUnit.HousingUnitBedNumber,
                        HousingUnitBedLocation = i.HousingUnit.HousingUnitBedLocation,
                        HousingUnitListId = i.HousingUnit.HousingUnitListId,
                        HousingUnitLocation = i.HousingUnit.HousingUnitLocation,
                        HousingUnitNumber = i.HousingUnit.HousingUnitNumber,
                        HousingUnitId = i.HousingUnitId ?? 0
                    },
                    FacilityId = i.FacilityId,
                    FacilityAbbr = i.Facility.FacilityAbbr,
                    PersonClassificationType = i.InmateClassification.InmateClassificationReason
                });

            IEnumerable<HousingConflictVm> lstPerClassifyInmates =
                lstPersonClassification.Where(p => p.FacilityId == facilityId && p.Housing.HousingUnitId > 0).ToList();

            //Selected Inmate List
            IEnumerable<HousingConflictVm> lstPerClassifySelectedInmates =
                lstPersonClassification.Where(i => i.InmateId == inmateId).ToList();


            #region Assoc To Assoc

            List<Lookup> lookupslist = _context.Lookup.Where(w => w.LookupType == LookupConstants.CLASSGROUP).ToList();
            List<Lookup> lookupsSublist =
                _context.Lookup.Where(w => w.LookupType == LookupConstants.CLASSGROUPSUB).ToList();
            //Keep Sep Details for Assoc - Assoc
            IEnumerable<KeepSepAssocAssoc> tkeepSepAssocAssoc =
                _context.KeepSepAssocAssoc.Where(ksaa => ksaa.DeleteFlag == 0);

            //Same Assoc Details
            IEnumerable<HousingConflictVm> keepSepAssocAssoc = tkeepSepAssocAssoc
                .SelectMany(ksaa => lstPerClassifyInmates
                        .Where(w => w.KeepSepAssoc1Id == ksaa.KeepSepAssoc1Id),
                    (ksaa, lpcl) => new HousingConflictVm
                    {
                        PersonId = lpcl.PersonId,
                        InmateId = lpcl.InmateId,
                        InmateNumber = lpcl.InmateNumber,
                        Housing = lpcl.Housing,
                        ConflictDescription = ksaa.KeepSepReason,
                        KeepSepAssoc2 = lookupslist.Where(k => k.LookupIndex == ksaa.KeepSepAssoc2Id)
                            .Select(a => a.LookupDescription).SingleOrDefault(),
                        KeepSepAssoc1 = lookupslist.Where(k => k.LookupIndex == ksaa.KeepSepAssoc1Id)
                            .Select(a => a.LookupDescription).SingleOrDefault(),
                        KeepSepAssoc1Id = ksaa.KeepSepAssoc1Id,
                        KeepSepAssoc2Id = ksaa.KeepSepAssoc2Id
                    });

            //Differnt Assoc Details
            IEnumerable<HousingConflictVm> tKeepSepAssocAssoc = tkeepSepAssocAssoc
                .SelectMany(ksaa => lstPerClassifyInmates
                        .Where(w => w.KeepSepAssoc1Id == ksaa.KeepSepAssoc2Id),
                    (ksaa, lpcl) => new HousingConflictVm
                    {
                        PersonId = lpcl.PersonId,
                        InmateId = lpcl.InmateId,
                        InmateNumber = lpcl.InmateNumber,
                        Housing = lpcl.Housing,
                        ConflictDescription = ksaa.KeepSepReason,
                        KeepSepAssoc2 = lookupslist.Where(k => k.LookupIndex == ksaa.KeepSepAssoc2Id)
                            .Select(a => a.LookupDescription).SingleOrDefault(),
                        KeepSepAssoc1 = lookupslist.Where(k => k.LookupIndex == ksaa.KeepSepAssoc1Id)
                            .Select(a => a.LookupDescription).SingleOrDefault(),
                        KeepSepAssoc1Id = ksaa.KeepSepAssoc1Id,
                        KeepSepAssoc2Id = ksaa.KeepSepAssoc2Id

                    });

            List<HousingConflictVm> lstKeepSeps = keepSepAssocAssoc
                .SelectMany(ksaa => lstPerClassifySelectedInmates
                        .Where(w => w.KeepSepAssoc1Id == ksaa.KeepSepAssoc2Id
                                    && ksaa.PersonId != w.PersonId
                        ),
                    (ksaa, lksaas) => new
                    {
                        KeepSepPersonId = ksaa.PersonId,
                        KeepSepInmateId = ksaa.InmateId,
                        KeepSepInmateNumber = ksaa.InmateNumber,
                        ksaa.ConflictDescription,
                        KeepSepAssoc1 = ksaa.KeepSepAssoc2,
                        KeepSepAssoc2 = ksaa.KeepSepAssoc1,
                        ksaa.KeepSepAssoc1Id,
                        ksaa.KeepSepAssoc2Id,
                        lksaas.PersonId,
                        lksaas.InmateId,
                        lksaas.InmateNumber

                    }).Distinct().Select(s => new HousingConflictVm
                {
                    ConflictType = KeepSepLabel.ASSOCKEEPSEPASSOC,
                    KeepSepPersonId = s.KeepSepPersonId,
                    KeepSepInmateId = s.KeepSepInmateId,
                    KeepSepInmateNumber = s.KeepSepInmateNumber,
                    ConflictDescription = s.ConflictDescription,
                    KeepSepAssoc1 = s.KeepSepAssoc1,
                    KeepSepAssoc2 = s.KeepSepAssoc2,
                    KeepSepAssoc1Id = s.KeepSepAssoc1Id,
                    KeepSepAssoc2Id = s.KeepSepAssoc2Id,
                    PersonId = s.PersonId,
                    InmateId = s.InmateId,
                    InmateNumber = s.InmateNumber,
                    AssignConflictType = KeepSepLabel.KEEPSEPASSOCASSOCSAMELOCATION
                }).ToList();

            lstKeepSeps.AddRange(tKeepSepAssocAssoc.SelectMany(ksaa => lstPerClassifySelectedInmates
                .Where(w => w.KeepSepAssoc1Id == ksaa.KeepSepAssoc1Id
                            && ksaa.PersonId != w.PersonId), (ksaa, lksaas) => new
            {

                ksaa.ConflictDescription,
                ksaa.KeepSepAssoc1,
                ksaa.KeepSepAssoc2,
                ksaa.KeepSepAssoc1Id,
                ksaa.KeepSepAssoc2Id,
                lksaas.PersonId,
                lksaas.InmateId,
                lksaas.InmateNumber,
                KeepSepPersonId = ksaa.PersonId,
                KeepSepInmateId = ksaa.InmateId,
                KeepSepInmateNumber = ksaa.InmateNumber
            }).Distinct().Select(s => new HousingConflictVm
            {
                ConflictType = KeepSepLabel.ASSOCKEEPSEPASSOC,
                ConflictDescription = s.ConflictDescription,
                KeepSepAssoc1Id = s.KeepSepAssoc1Id,
                KeepSepAssoc2Id = s.KeepSepAssoc2Id,
                KeepSepAssoc1 = s.KeepSepAssoc1,
                KeepSepAssoc2 = s.KeepSepAssoc2,
                PersonId = s.PersonId,
                InmateId = s.InmateId,
                InmateNumber = s.InmateNumber,
                KeepSepPersonId = s.KeepSepPersonId,
                KeepSepInmateId = s.KeepSepInmateId,
                KeepSepInmateNumber = s.KeepSepInmateNumber,
                AssignConflictType = KeepSepLabel.KEEPSEPASSOCASSOCSAMELOCATION
            }));

            #endregion

            #region Assoc To Inmate               

            // To get the person classification based on Facility
            IEnumerable<HousingConflictVm> tKeepSepAssocInmate = _context.KeepSepAssocInmate.Where(
                    p => p.DeleteFlag == 0 && p.KeepSepInmate2.HousingUnitId.HasValue
                                           && p.KeepSepInmate2.InmateActive == 1 &&
                                           p.KeepSepInmate2.FacilityId == facilityId)
                .Select(s => new HousingConflictVm
                {
                    KeepSepPersonId = s.KeepSepInmate2.PersonId,
                    KeepSepInmateId = s.KeepSepInmate2Id,
                    KeepSepInmateNumber = s.KeepSepInmate2.InmateNumber,
                    KeepSepAssoc1 = lookupslist.Where(k => k.LookupIndex == s.KeepSepAssoc1Id)
                        .Select(a => a.LookupDescription).SingleOrDefault(),
                    KeepSepAssoc1Id = s.KeepSepAssoc1Id,
                    KeepSepType = s.KeepSeparateType,
                    ConflictDescription = s.KeepSepReason,
                    Housing = new HousingDetail
                    {
                        HousingUnitListId = s.KeepSepInmate2.HousingUnit.HousingUnitListId,
                        HousingUnitLocation = s.KeepSepInmate2.HousingUnit.HousingUnitLocation,
                        HousingUnitNumber = s.KeepSepInmate2.HousingUnit.HousingUnitNumber,
                        HousingUnitBedNumber = s.KeepSepInmate2.HousingUnit.HousingUnitBedNumber,
                        HousingUnitBedLocation = s.KeepSepInmate2.HousingUnit.HousingUnitBedLocation
                    },
                });

            lstKeepSeps.AddRange(tKeepSepAssocInmate.SelectMany(ksa => lstPerClassifySelectedInmates
                .Where(w => w.KeepSepAssoc1Id == ksa.KeepSepAssoc1Id
                            && w.InmateId != ksa.KeepSepInmateId), (ksa, lksa) => new
            {
                ksa.KeepSepPersonId,
                ksa.KeepSepInmateId,
                ksa.KeepSepInmateNumber,
                lksa.PersonId,
                lksa.InmateId,
                lksa.InmateNumber,
                ksa.KeepSepAssoc1,
                ksa.KeepSepAssoc1Id,
                ksa.KeepSepType,
                ksa.ConflictDescription,
                AssignConflictType = KeepSepLabel.KEEPSEPASSOCINMATESAMELOCATION
            }).Distinct().Select(s =>
                new HousingConflictVm
                {
                    ConflictType = KeepSepLabel.ASSOCKEEPSEPINMATE,
                    KeepSepPersonId = s.KeepSepPersonId,
                    KeepSepInmateId = s.KeepSepInmateId,
                    KeepSepInmateNumber = s.KeepSepInmateNumber,
                    PersonId = s.PersonId,
                    InmateId = s.InmateId,
                    InmateNumber = s.InmateNumber,
                    KeepSepAssoc1 = s.KeepSepAssoc1,
                    KeepSepAssoc1Id = s.KeepSepAssoc1Id,
                    KeepSepType = s.KeepSepType,
                    ConflictDescription = s.ConflictDescription,
                    AssignConflictType = KeepSepLabel.KEEPSEPASSOCINMATESAMELOCATION
                }));

            #endregion

            #region Assoc To Subset

            IQueryable<KeepSepAssocSubset> tKeepSepAssocSubset =
                _context.KeepSepAssocSubset.Where(ksas => ksas.DeleteFlag == 0);

            IEnumerable<HousingConflictVm> lstKeepSepAssocSubset = tKeepSepAssocSubset
                .SelectMany(ksas => lstPerClassifySelectedInmates
                        .Where(w => ksas.KeepSepAssoc1Id == w.KeepSepAssoc1Id),
                    (ksas, ksaas) => new HousingConflictVm
                    {
                        PersonId = ksaas.PersonId,
                        InmateId = ksaas.InmateId,
                        InmateNumber = ksaas.InmateNumber,
                        KeepSepAssoc1 = lookupslist.Where(s => s.LookupIndex == ksas.KeepSepAssoc1Id)
                            .Select(a => a.LookupDescription).SingleOrDefault(),
                        KeepSepAssoc2 = lookupslist.Where(s => s.LookupIndex == ksas.KeepSepAssoc2Id)
                            .Select(a => a.LookupDescription).SingleOrDefault(),
                        KeepSepAssoc2Subset = lookupsSublist.Where(s => s.LookupIndex == ksas.KeepSepAssoc2SubsetId)
                            .Select(a => a.LookupDescription).SingleOrDefault(),
                        KeepSepAssoc1Id = ksas.KeepSepAssoc1Id,
                        KeepSepAssoc2Id = ksas.KeepSepAssoc2Id,
                        KeepSepAssoc2SubsetId = ksas.KeepSepAssoc2SubsetId,
                    });

            //ASSOC KEEP SEPARATE To SUBSET
            lstKeepSeps.AddRange(lstKeepSepAssocSubset.SelectMany(ksas =>
                    lstPerClassifyInmates.Where(w => ksas.KeepSepAssoc2Id == w.KeepSepAssoc1Id
                                                     && ksas.KeepSepAssoc2SubsetId == w.KeepSepAssocSubset1Id
                                                     && ksas.PersonId != w.PersonId),
                (ksas, lksa) => new
                {
                    ksas.PersonId,
                    ksas.InmateId,
                    ksas.InmateNumber,
                    KeepSepPersonId = lksa.PersonId,
                    KeepSepInmateId = lksa.InmateId,
                    KeepSepInmateNumber = lksa.InmateNumber,
                    ksas.KeepSepAssoc1,
                    ksas.KeepSepAssoc2,
                    KeepSepAssocSubset2 = ksas.KeepSepAssoc2Subset,
                    ksas.KeepSepAssoc1Id,
                    ksas.KeepSepAssoc2Id,
                    KeepSepAssocSubset2Id = ksas.KeepSepAssoc2SubsetId,
                }).Distinct().Select(s => new HousingConflictVm
            {
                ConflictType = KeepSepLabel.ASSOCKEEPSEPSUBSET,
                PersonId = s.PersonId,
                InmateId = s.InmateId,
                InmateNumber = s.InmateNumber,
                KeepSepPersonId = s.KeepSepPersonId,
                KeepSepInmateId = s.KeepSepInmateId,
                KeepSepInmateNumber = s.KeepSepInmateNumber,
                KeepSepAssoc1 = s.KeepSepAssoc1,
                KeepSepAssoc2 = s.KeepSepAssoc2,
                KeepSepAssocSubset2 = s.KeepSepAssocSubset2,
                KeepSepAssoc1Id = s.KeepSepAssoc1Id,
                KeepSepAssoc2Id = s.KeepSepAssoc2Id,
                KeepSepAssocSubset2Id = s.KeepSepAssocSubset2Id,
                AssignConflictType = KeepSepLabel.KEEPSEPASSOCSUBSETSAMELOCATION
            }));


            //ASSOC KEEP SEPARATE To SUBSET
            lstKeepSeps.AddRange(lstKeepSepAssocSubset.SelectMany(ksas =>
                    lstPerClassifySelectedInmates.Where(w => ksas.KeepSepAssoc2Id == w.KeepSepAssoc1Id
                                                             && ksas.KeepSepAssoc2SubsetId == w.KeepSepAssocSubset1Id
                                                             && ksas.PersonId != w.PersonId),
                (ksas, lksa) => new
                {
                    ksas.PersonId,
                    ksas.InmateId,
                    ksas.InmateNumber,
                    KeepSepPersonId = lksa.PersonId,
                    KeepSepInmateId = lksa.InmateId,
                    KeepSepInmateNumber = lksa.InmateNumber,
                    ksas.KeepSepAssoc1,
                    ksas.KeepSepAssoc2,
                    KeepSepAssocSubset2 = ksas.KeepSepAssoc2Subset,
                    ksas.KeepSepAssoc1Id,
                    ksas.KeepSepAssoc2Id,
                    KeepSepAssocSubset2Id = ksas.KeepSepAssoc2SubsetId
                }).Distinct().Select(s => new HousingConflictVm
            {
                ConflictType = KeepSepLabel.ASSOCKEEPSEPSUBSET,
                PersonId = s.PersonId,
                InmateId = s.InmateId,
                InmateNumber = s.InmateNumber,
                KeepSepPersonId = s.KeepSepPersonId,
                KeepSepInmateId = s.KeepSepInmateId,
                KeepSepInmateNumber = s.KeepSepInmateNumber,
                KeepSepAssoc1 = s.KeepSepAssoc1,
                KeepSepAssoc2 = s.KeepSepAssoc2,
                KeepSepAssocSubset2 = s.KeepSepAssocSubset2,
                KeepSepAssoc1Id = s.KeepSepAssoc1Id,
                KeepSepAssoc2Id = s.KeepSepAssoc2Id,
                KeepSepAssocSubset2Id = s.KeepSepAssocSubset2Id,
                AssignConflictType = KeepSepLabel.KEEPSEPASSOCSUBSETSAMELOCATION
            }));

            #endregion

            #region Inmate To Assoc           

            IQueryable<KeepSepAssocInmate> keepSepInmateAssoc = _context.KeepSepAssocInmate.Where(
                ksa => ksa.DeleteFlag == 0 && ksa.KeepSepInmate2Id == inmateId);

            lstKeepSeps.AddRange(keepSepInmateAssoc.SelectMany(ksa =>
                lstPerClassifyInmates.Where(w => w.KeepSepAssoc1Id == ksa.KeepSepAssoc1Id
                                                 && w.InmateId != ksa.KeepSepInmate2Id), (ksa, lksa) =>
                new HousingConflictVm
                {
                    PersonId = ksa.KeepSepInmate2.PersonId,
                    InmateId = ksa.KeepSepInmate2Id,
                    InmateNumber = ksa.KeepSepInmate2.InmateNumber,
                    KeepSepInmateId = lksa.InmateId,
                    KeepSepPersonId = lksa.PersonId,
                    KeepSepInmateNumber = lksa.InmateNumber,
                    KeepSepAssoc1 = lookupslist.Where(s => s.LookupIndex == ksa.KeepSepAssoc1Id)
                        .Select(a => a.LookupDescription).SingleOrDefault(),
                    KeepSepAssoc1Id = ksa.KeepSepAssoc1Id,
                    KeepSepType = ksa.KeepSeparateType,
                    ConflictDescription = ksa.KeepSepReason
                }).GroupBy(g => new
            {
                g.InmateId,
                g.PersonId,
                g.InmateNumber,
                g.KeepSepInmateId,
                g.KeepSepPersonId,
                g.KeepSepInmateNumber,
                g.KeepSepAssoc1,
                g.KeepSepType,
                g.ConflictDescription,
                g.KeepSepAssoc1Id,

            }).Select(s => new HousingConflictVm
            {
                ConflictType = KeepSepLabel.INMATEKEEPSEPASSOC,
                PersonId = s.Key.PersonId,
                InmateId = s.Key.InmateId,
                InmateNumber = s.Key.InmateNumber,
                KeepSepInmateId = s.Key.KeepSepInmateId,
                KeepSepPersonId = s.Key.KeepSepPersonId,
                KeepSepInmateNumber = s.Key.KeepSepInmateNumber,
                KeepSepAssoc1 = s.Key.KeepSepAssoc1,
                KeepSepType = s.Key.KeepSepType,
                ConflictDescription = s.Key.ConflictDescription,
                KeepSepAssoc1Id = s.Key.KeepSepAssoc1Id,
                AssignConflictType = KeepSepLabel.KEEPSEPINMATEASSOCSAMELOCATION
            }));

            #endregion

            #region Inmate To Inmate

            //Keep Sep Inmate 1 List           

            IEnumerable<HousingConflictVm> kepSeparateDetails = _context.KeepSeparate.Where(
                    p => p.InactiveFlag == 0 && p.KeepSeparateInmate1Id == inmateId
                                             && p.KeepSeparateInmate1.InmateActive == 1 &&
                                             p.KeepSeparateInmate2.FacilityId == facilityId)
                .Select(ks => new HousingConflictVm
                {
                    ConflictType = KeepSepLabel.INMATEKEEPSEPINMATE,
                    KeepSepPersonId = ks.KeepSeparateInmate2.PersonId,
                    KeepSepInmateId = ks.KeepSeparateInmate2Id,
                    KeepSepInmateNumber = ks.KeepSeparateInmate2.InmateNumber,
                    PersonId = ks.KeepSeparateInmate1.PersonId,
                    InmateId = ks.KeepSeparateInmate1Id,
                    InmateNumber = ks.KeepSeparateInmate1.InmateNumber,
                    KeepSepType = ks.KeepSeparateType,
                    ConflictDescription = ks.KeepSeparateReason,
                    Housing = new HousingDetail
                    {
                        HousingUnitListId = ks.KeepSeparateInmate2.HousingUnit.HousingUnitListId,
                        HousingUnitLocation = ks.KeepSeparateInmate2.HousingUnit.HousingUnitLocation,
                        HousingUnitNumber = ks.KeepSeparateInmate2.HousingUnit.HousingUnitNumber,
                        HousingUnitBedNumber = ks.KeepSeparateInmate2.HousingUnit.HousingUnitBedNumber,
                        HousingUnitBedLocation = ks.KeepSeparateInmate2.HousingUnit.HousingUnitBedLocation,
                        HousingUnitId = ks.KeepSeparateInmate2.HousingUnitId ?? 0
                    },
                    AssignConflictType = KeepSepLabel.KEEPSEPINMATEINMATESAMELOCATION
                }).ToList();

            lstKeepSeps.AddRange(kepSeparateDetails);

            IEnumerable<HousingConflictVm> kepSeparateInmate2Details = _context.KeepSeparate
                .Where(ks => ks.InactiveFlag == 0 && ks.KeepSeparateInmate2Id == inmateId
                                                  && ks.KeepSeparateInmate2.InmateActive == 1
                                                  && ks.KeepSeparateInmate1.HousingUnitId.HasValue
                                                  && ks.KeepSeparateInmate1.FacilityId == facilityId)
                .Select(ks => new HousingConflictVm
                {
                    ConflictType = KeepSepLabel.INMATEKEEPSEPINMATE,
                    KeepSepPersonId = ks.KeepSeparateInmate1.PersonId,
                    KeepSepInmateId = ks.KeepSeparateInmate1Id,
                    KeepSepInmateNumber = ks.KeepSeparateInmate1.InmateNumber,
                    PersonId = ks.KeepSeparateInmate2.PersonId,
                    InmateId = ks.KeepSeparateInmate2Id,
                    InmateNumber = ks.KeepSeparateInmate2.InmateNumber,
                    KeepSepType = ks.KeepSeparateType,
                    ConflictDescription = ks.KeepSeparateReason,
                    Housing = new HousingDetail
                    {
                        HousingUnitListId = ks.KeepSeparateInmate1.HousingUnit.HousingUnitListId,
                        HousingUnitLocation = ks.KeepSeparateInmate1.HousingUnit.HousingUnitLocation,
                        HousingUnitNumber = ks.KeepSeparateInmate1.HousingUnit.HousingUnitNumber,
                        HousingUnitBedNumber = ks.KeepSeparateInmate1.HousingUnit.HousingUnitBedNumber,
                        HousingUnitBedLocation = ks.KeepSeparateInmate1.HousingUnit.HousingUnitBedLocation,
                        HousingUnitId = ks.KeepSeparateInmate1.HousingUnitId ?? 0
                    },
                    AssignConflictType = KeepSepLabel.KEEPSEPINMATEINMATESAMELOCATION
                });

            lstKeepSeps.AddRange(kepSeparateInmate2Details);

            #endregion

            #region Inmate To Subset

            IEnumerable<HousingConflictVm> keepSepInmateSubset = _context.KeepSepSubsetInmate
                .Where(ksa => ksa.DeleteFlag == 0 && ksa.KeepSepInmate2Id == inmateId)
                .Select(s => new HousingConflictVm
                {
                    PersonId = s.KeepSepInmate2.PersonId,
                    InmateId = s.KeepSepInmate2Id,
                    InmateNumber = s.KeepSepInmate2.InmateNumber,
                    KeepSepAssoc1 = lookupslist.Where(k => k.LookupIndex == s.KeepSepAssoc1Id)
                        .Select(a => a.LookupDescription).SingleOrDefault(),
                    KeepSepAssocSubset1 = lookupsSublist.Where(k => k.LookupIndex == s.KeepSepAssoc1SubsetId)
                        .Select(a => a.LookupDescription).SingleOrDefault(),
                    KeepSepAssoc1Id = s.KeepSepAssoc1Id,
                    KeepSepAssocSubset1Id = s.KeepSepAssoc1SubsetId,
                    KeepSepType = s.KeepSeparateType,
                    ConflictDescription = s.KeepSepReason
                });

            //Keep Sep : INMATE KEEP SEPARATE To SUBSET
            lstKeepSeps.AddRange(keepSepInmateSubset.SelectMany(ksa =>
                lstPerClassifyInmates.Where(w => w.KeepSepAssoc1Id == ksa.KeepSepAssoc1Id
                                                 && w.KeepSepAssocSubset1Id == ksa.KeepSepAssocSubset1Id
                                                 && w.InmateId != ksa.InmateId), (ksa, lpcl) =>
                new
                {
                    ksa.PersonId,
                    ksa.InmateId,
                    ksa.InmateNumber,
                    KeepSepPersonId = lpcl.PersonId,
                    KeepSepInmateId = lpcl.InmateId,
                    KeepSepInmateNumber = lpcl.InmateNumber,
                    ksa.KeepSepAssoc1,
                    ksa.KeepSepAssocSubset1,
                    ksa.KeepSepAssoc1Id,
                    ksa.KeepSepAssocSubset1Id,
                    ksa.KeepSepType,
                    ConflictDescription = ksa.KeepSepReason
                }).Distinct().Select(s => new HousingConflictVm
            {
                ConflictType = KeepSepLabel.INMATEKEEPSEPSUBSET,
                PersonId = s.PersonId,
                InmateId = s.InmateId,
                InmateNumber = s.InmateNumber,
                KeepSepPersonId = s.KeepSepPersonId,
                KeepSepInmateId = s.KeepSepInmateId,
                KeepSepInmateNumber = s.KeepSepInmateNumber,
                KeepSepAssoc1 = s.KeepSepAssoc1,
                KeepSepAssocSubset1 = s.KeepSepAssocSubset1,
                KeepSepType = s.KeepSepType,
                ConflictDescription = s.ConflictDescription,
                KeepSepAssoc1Id = s.KeepSepAssoc1Id,
                KeepSepAssocSubset1Id = s.KeepSepAssocSubset1Id,
                AssignConflictType = KeepSepLabel.KEEPSEPINMATESUBSETSAMELOCATION
            }));

            #endregion

            #region Subset To Assoc

            IEnumerable<HousingConflictVm> selLstKeepSepAssocSubset = tKeepSepAssocSubset
                .SelectMany(ksas => lstPerClassifySelectedInmates
                        .Where(w => ksas.KeepSepAssoc2Id == w.KeepSepAssoc1Id
                                    && ksas.KeepSepAssoc2SubsetId == w.KeepSepAssocSubset1Id),
                    (ksas, ksaas) => new HousingConflictVm
                    {
                        PersonId = ksaas.PersonId,
                        InmateId = ksaas.InmateId,
                        InmateNumber = ksaas.InmateNumber,
                        KeepSepReason = ksas.KeepSepReason,
                        KeepSepAssoc1 = lookupslist.Where(s => s.LookupIndex == ksas.KeepSepAssoc1Id)
                            .Select(a => a.LookupDescription).SingleOrDefault(),
                        KeepSepAssoc2 = lookupslist.Where(s => s.LookupIndex == ksas.KeepSepAssoc2Id)
                            .Select(a => a.LookupDescription).SingleOrDefault(),
                        KeepSepAssoc2Subset = lookupsSublist.Where(s => s.LookupIndex == ksas.KeepSepAssoc2SubsetId)
                            .Select(a => a.LookupDescription).SingleOrDefault(),
                        KeepSepAssoc1Id = ksas.KeepSepAssoc1Id,
                        KeepSepAssoc2Id = ksas.KeepSepAssoc2Id,
                        KeepSepAssocSubset2Id = ksas.KeepSepAssoc2SubsetId,
                    });


            lstKeepSeps.AddRange(selLstKeepSepAssocSubset.SelectMany(ksas =>
                lstPerClassifySelectedInmates.Where(w => w.KeepSepAssoc1Id == ksas.KeepSepAssoc1Id
                                                         && ksas.PersonId != w.PersonId), (ksas, lpcl) =>
                new
                {
                    ksas.PersonId,
                    ksas.InmateId,
                    ksas.InmateNumber,
                    KeepSepPersonId = lpcl.PersonId,
                    KeepSepInmateId = lpcl.InmateId,
                    KeepSepInmateNumber = lpcl.InmateNumber,
                    ksas.KeepSepAssoc1,
                    KeepSepAssocSubset1 = ksas.KeepSepAssoc2,
                    KeepSepAssoc2 = ksas.KeepSepAssoc2Subset,
                    ksas.KeepSepAssoc1Id,
                    KeepSepAssocSubset1Id = ksas.KeepSepAssoc2Id,
                    KeepSepAssoc2Id = ksas.KeepSepAssoc2SubsetId,
                }).Distinct().Select(s =>
                new HousingConflictVm
                {
                    ConflictType = KeepSepLabel.SUBSETKEEPSEPASSOC,
                    PersonId = s.PersonId,
                    InmateId = s.InmateId,
                    InmateNumber = s.InmateNumber,
                    KeepSepPersonId = s.KeepSepPersonId,
                    KeepSepInmateId = s.KeepSepInmateId,
                    KeepSepInmateNumber = s.KeepSepInmateNumber,
                    KeepSepAssoc1 = s.KeepSepAssoc1,
                    KeepSepAssocSubset1 = s.KeepSepAssocSubset1,
                    KeepSepAssoc2 = s.KeepSepAssoc2,
                    KeepSepAssoc1Id = s.KeepSepAssoc1Id,
                    KeepSepAssoc2Id = s.KeepSepAssoc2Id,
                    KeepSepAssocSubset1Id = s.KeepSepAssocSubset1Id,
                    AssignConflictType = KeepSepLabel.KEEPSEPSUBSETASSOCSAMELOCATION
                }));

            lstKeepSeps.AddRange(selLstKeepSepAssocSubset.SelectMany(ksas =>
                lstPerClassifyInmates.Where(w => w.KeepSepAssoc1Id == ksas.KeepSepAssoc1Id
                                                 && ksas.PersonId != w.PersonId), (ksas, lpcl) =>
                new
                {
                    ksas.PersonId,
                    ksas.InmateId,
                    ksas.InmateNumber,
                    KeepSepPersonId = lpcl.PersonId,
                    KeepSepInmateId = lpcl.InmateId,
                    KeepSepInmateNumber = lpcl.InmateNumber,
                    ksas.KeepSepAssoc1,
                    KeepSepAssocSubset1 = ksas.KeepSepAssoc2,
                    KeepSepAssoc2 = ksas.KeepSepAssoc2Subset,
                    ksas.KeepSepAssoc1Id,
                    KeepSepAssocSubset1Id = ksas.KeepSepAssoc2Id,
                    KeepSepAssoc2Id = ksas.KeepSepAssoc2SubsetId,
                }).Distinct().Select(s => new HousingConflictVm
            {
                ConflictType = KeepSepLabel.SUBSETKEEPSEPASSOC,
                PersonId = s.PersonId,
                InmateId = s.InmateId,
                InmateNumber = s.InmateNumber,
                KeepSepPersonId = s.KeepSepPersonId,
                KeepSepInmateId = s.KeepSepInmateId,
                KeepSepInmateNumber = s.KeepSepInmateNumber,
                KeepSepAssoc1 = s.KeepSepAssoc1,
                KeepSepAssocSubset1 = s.KeepSepAssocSubset1,
                KeepSepAssoc2 = s.KeepSepAssoc2,
                KeepSepAssoc1Id = s.KeepSepAssoc1Id,
                KeepSepAssoc2Id = s.KeepSepAssoc2Id,
                KeepSepAssocSubset1Id = s.KeepSepAssocSubset1Id,
                AssignConflictType = KeepSepLabel.KEEPSEPSUBSETASSOCSAMELOCATION
            }));


            //}

            #endregion

            #region Subset To Inmate

            IEnumerable<HousingConflictVm> keepSepSubsetInmate = _context.KeepSepSubsetInmate.Where(
                    p => p.DeleteFlag == 0 && p.KeepSepInmate2.HousingUnitId.HasValue
                                           && p.KeepSepInmate2.InmateActive == 1 &&
                                           p.KeepSepInmate2.FacilityId == facilityId)
                .Select(s => new HousingConflictVm
                {
                    KeepSepPersonId = s.KeepSepInmate2.PersonId,
                    KeepSepInmateId = s.KeepSepInmate2Id,
                    KeepSepInmateNumber = s.KeepSepInmate2.InmateNumber,
                    KeepSepAssoc1 = lookupslist.Where(k => k.LookupIndex == s.KeepSepAssoc1Id)
                        .Select(a => a.LookupDescription).SingleOrDefault(),
                    KeepSepAssocSubset1 = lookupsSublist.Where(k => k.LookupIndex == s.KeepSepAssoc1SubsetId)
                        .Select(a => a.LookupDescription).SingleOrDefault(),
                    KeepSepAssoc1Id = s.KeepSepAssoc1Id,
                    KeepSepAssoc1SubsetId = s.KeepSepAssoc1SubsetId,
                    KeepSepType = s.KeepSeparateType,
                    ConflictDescription = s.KeepSepReason,
                    Housing = new HousingDetail
                    {
                        HousingUnitListId = s.KeepSepInmate2.HousingUnit.HousingUnitListId,
                        HousingUnitLocation = s.KeepSepInmate2.HousingUnit.HousingUnitLocation,
                        HousingUnitNumber = s.KeepSepInmate2.HousingUnit.HousingUnitNumber,
                        HousingUnitBedNumber = s.KeepSepInmate2.HousingUnit.HousingUnitBedNumber,
                        HousingUnitBedLocation = s.KeepSepInmate2.HousingUnit.HousingUnitBedLocation
                    },
                    FacilityId = s.KeepSepInmate2.FacilityId
                }).ToList();

            //Keep Sep : SUBSET SEPARATE To INMATE KEEP
            lstKeepSeps.AddRange(keepSepSubsetInmate.SelectMany(ksa =>
                lstPerClassifySelectedInmates.Where(w => w.KeepSepAssoc1 == ksa.KeepSepAssoc1
                                                         && w.KeepSepAssocSubset1 == ksa.KeepSepAssocSubset1
                                                         && w.InmateId != ksa.KeepSepInmateId), (ksa, lpcl) =>
                new
                {
                    lpcl.PersonId,
                    lpcl.InmateId,
                    lpcl.InmateNumber,
                    ksa.KeepSepPersonId,
                    ksa.KeepSepInmateId,
                    ksa.KeepSepInmateNumber,
                    ksa.KeepSepAssoc1,
                    ksa.KeepSepAssocSubset1,
                    ksa.KeepSepType,
                    ksa.ConflictDescription,
                    ksa.KeepSepAssoc1Id,
                    ksa.KeepSepAssoc1SubsetId
                }).Distinct().Select(s => new HousingConflictVm
            {
                ConflictType = KeepSepLabel.SUBSETKEEPSEPINMATE,
                PersonId = s.PersonId,
                InmateId = s.InmateId,
                InmateNumber = s.InmateNumber,
                KeepSepPersonId = s.KeepSepPersonId,
                KeepSepInmateId = s.KeepSepInmateId,
                KeepSepInmateNumber = s.KeepSepInmateNumber,
                KeepSepAssoc1 = s.KeepSepAssoc1,
                KeepSepAssocSubset1 = s.KeepSepAssocSubset1,
                KeepSepType = s.KeepSepType,
                ConflictDescription = s.ConflictDescription,
                KeepSepAssoc1Id = s.KeepSepAssoc1Id,
                KeepSepAssoc1SubsetId = s.KeepSepAssoc1SubsetId,
                AssignConflictType = KeepSepLabel.KEEPSEPSUBSETINMATESAMELOCATION
            }));

            #endregion

            #region Subset To Subset

            List<HousingConflictVm> keepSepSubsetSubset = _context.KeepSepSubsetSubset.SelectMany(ksss =>
                lstPerClassifyInmates.Where(w =>
                    ksss.KeepSepAssoc1Id == w.KeepSepAssoc1Id
                    && ksss.KeepSepAssoc1SubsetId == w.KeepSepAssocSubset1Id
                    && ksss.DeleteFlag == 0), (ksss, ksaas) => new HousingConflictVm
            {
                PersonId = ksaas.PersonId,
                InmateId = ksaas.InmateId,
                Housing = ksaas.Housing,
                InmateNumber = ksaas.InmateNumber,
                ConflictDescription = ksss.KeepSepReason,
                KeepSepAssoc1 = lookupslist.Where(k => k.LookupIndex == ksaas.KeepSepAssoc1Id)
                    .Select(a => a.LookupDescription).SingleOrDefault(),
                KeepSepAssoc1Subset = lookupsSublist.Where(k => k.LookupIndex == ksaas.KeepSepAssoc1SubsetId)
                    .Select(a => a.LookupDescription).SingleOrDefault(),
                KeepSepAssoc2 = lookupslist.Where(k => k.LookupIndex == ksaas.KeepSepAssoc2Id)
                    .Select(a => a.LookupDescription).SingleOrDefault(),
                KeepSepAssoc2Subset = lookupsSublist.Where(k => k.LookupIndex == ksaas.KeepSepAssoc2SubsetId)
                    .Select(a => a.LookupDescription).SingleOrDefault(),
                KeepSepAssoc1Id = ksss.KeepSepAssoc1Id,
                KeepSepAssoc1SubsetId = ksss.KeepSepAssoc1SubsetId,
                KeepSepAssoc2Id = ksss.KeepSepAssoc2Id,
                KeepSepAssoc2SubsetId = ksss.KeepSepAssoc2SubsetId
            }).ToList();

            lstKeepSeps.AddRange(keepSepSubsetSubset.SelectMany(ksss =>
                lstPerClassifySelectedInmates.Where(w => ksss.KeepSepAssoc2Id == w.KeepSepAssoc1Id
                                                         && ksss.KeepSepAssoc2SubsetId == w.KeepSepAssocSubset1Id
                                                         && w.InmateId != ksss.InmateId), (ksss, lksaas) =>
                new
                {
                    KeepSepPersonId = ksss.PersonId,
                    KeepSepInmateId = ksss.InmateId,
                    KeepSepInmateNumber = ksss.InmateNumber,
                    ksss.ConflictDescription,
                    KeepSepAssoc1 = ksss.KeepSepAssoc2,
                    KeepSepAssocSubset1 = ksss.KeepSepAssoc2Subset,
                    KeepSepAssoc2 = ksss.KeepSepAssoc1,
                    KeepSepAssocSubset2 = ksss.KeepSepAssoc1Subset,
                    KeepSepAssoc1Id = ksss.KeepSepAssoc2Id,
                    KeepSepAssocSubset1Id = ksss.KeepSepAssoc2SubsetId,
                    KeepSepAssoc2Id = ksss.KeepSepAssoc1Id,
                    KeepSepAssocSubset2Id = ksss.KeepSepAssoc1SubsetId,
                    lksaas.PersonId,
                    lksaas.InmateId,
                    lksaas.InmateNumber
                }).Distinct().Select(s => new HousingConflictVm
            {
                ConflictType = KeepSepLabel.SUBSETKEEPSEPSUBSET,
                KeepSepPersonId = s.KeepSepPersonId,
                KeepSepInmateId = s.KeepSepInmateId,
                KeepSepInmateNumber = s.KeepSepInmateNumber,
                ConflictDescription = s.ConflictDescription,
                KeepSepAssoc1 = s.KeepSepAssoc1,
                KeepSepAssocSubset1 = s.KeepSepAssocSubset1,
                KeepSepAssoc2 = s.KeepSepAssoc2,
                KeepSepAssocSubset2 = s.KeepSepAssocSubset2,
                PersonId = s.PersonId,
                InmateId = s.InmateId,
                InmateNumber = s.InmateNumber,
                KeepSepAssoc1Id = s.KeepSepAssoc1Id,
                KeepSepAssocSubset1Id = s.KeepSepAssocSubset1Id,
                KeepSepAssoc2Id = s.KeepSepAssoc2Id,
                KeepSepAssocSubset2Id = s.KeepSepAssocSubset2Id,
                AssignConflictType = KeepSepLabel.KEEPSEPSUBSETSUBSETSAMELOCATION
            }));

            IEnumerable<HousingConflictVm> keepSepSubsetTSubset = _context.KeepSepSubsetSubset
                .SelectMany(ksss => lstPerClassifySelectedInmates.Where(w =>
                    ksss.KeepSepAssoc1Id == w.KeepSepAssoc1Id
                    && ksss.KeepSepAssoc1SubsetId == w.KeepSepAssocSubset1Id
                    && ksss.DeleteFlag == 0), (ksss, lksaas) =>
                    new HousingConflictVm
                    {
                        ConflictDescription = ksss.KeepSepReason,
                        KeepSepAssoc1 = lookupslist.Where(k => k.LookupIndex == ksss.KeepSepAssoc1Id)
                            .Select(a => a.LookupDescription).SingleOrDefault(),
                        KeepSepAssoc1Subset = lookupsSublist.Where(k => k.LookupIndex == ksss.KeepSepAssoc1SubsetId)
                            .Select(a => a.LookupDescription).SingleOrDefault(),
                        KeepSepAssoc2 = lookupslist.Where(k => k.LookupIndex == ksss.KeepSepAssoc2Id)
                            .Select(a => a.LookupDescription).SingleOrDefault(),
                        KeepSepAssoc2Subset = lookupsSublist.Where(k => k.LookupIndex == ksss.KeepSepAssoc2SubsetId)
                            .Select(a => a.LookupDescription).SingleOrDefault(),
                        KeepSepAssoc1Id = ksss.KeepSepAssoc1Id,
                        KeepSepAssoc1SubsetId = ksss.KeepSepAssoc1SubsetId,
                        KeepSepAssoc2Id = ksss.KeepSepAssoc2Id,
                        KeepSepAssoc2SubsetId = ksss.KeepSepAssoc2SubsetId,
                        PersonId = lksaas.PersonId,
                        InmateId = lksaas.InmateId,
                        InmateNumber = lksaas.InmateNumber
                    });

            lstKeepSeps.AddRange(keepSepSubsetTSubset.SelectMany(ksss =>
                lstPerClassifyInmates.Where(w => ksss.KeepSepAssoc2Id == w.KeepSepAssoc1Id
                                                 && ksss.KeepSepAssoc2SubsetId == w.KeepSepAssocSubset1Id
                                                 && w.InmateId != ksss.InmateId), (ksss, ksaas) =>
                new
                {
                    KeepSepPersonId = ksaas.PersonId,
                    KeepSepInmateId = ksaas.InmateId,
                    KeepSepInmateNumber = ksaas.InmateNumber,
                    ksss.ConflictDescription,
                    ksss.KeepSepAssoc1,
                    KeepSepAssocSubset1 = ksss.KeepSepAssoc1Subset,
                    ksss.KeepSepAssoc2,
                    KeepSepAssocSubset2 = ksss.KeepSepAssoc2Subset,
                    ksss.KeepSepAssoc1Id,
                    KeepSepAssocSubset1Id = ksss.KeepSepAssoc1SubsetId,
                    ksss.KeepSepAssoc2Id,
                    KeepSepAssocSubset2Id = ksss.KeepSepAssoc2SubsetId,
                    ksss.PersonId,
                    ksss.InmateId,
                    ksss.InmateNumber
                }).Distinct().Select(s => new HousingConflictVm
            {
                ConflictType = KeepSepLabel.SUBSETKEEPSEPSUBSET,
                KeepSepPersonId = s.KeepSepPersonId,
                KeepSepInmateId = s.KeepSepInmateId,
                KeepSepInmateNumber = s.KeepSepInmateNumber,
                ConflictDescription = s.ConflictDescription,
                KeepSepAssoc1 = s.KeepSepAssoc1,
                KeepSepAssocSubset1 = s.KeepSepAssocSubset1,
                KeepSepAssoc2 = s.KeepSepAssoc2,
                KeepSepAssocSubset2 = s.KeepSepAssocSubset2,
                KeepSepAssoc1Id = s.KeepSepAssoc1Id,
                KeepSepAssocSubset1Id = s.KeepSepAssocSubset1Id,
                KeepSepAssoc2Id = s.KeepSepAssoc2Id,
                KeepSepAssocSubset2Id = s.KeepSepAssocSubset2Id,
                PersonId = s.PersonId,
                InmateId = s.InmateId,
                InmateNumber = s.InmateNumber,
                AssignConflictType = KeepSepLabel.KEEPSEPSUBSETSUBSETSAMELOCATION
            }));

            #endregion

            List<HousingConflictVm> listHousingConflictVm = lstKeepSeps;

            IQueryable<Person> lstPerson = _context.Person.Select(s => new Person
            {
                PersonId = s.PersonId,
                PersonLastName = s.PersonLastName,
                PersonFirstName = s.PersonFirstName,
                PersonMiddleName = s.PersonMiddleName
            });

            listHousingConflictVm.ForEach(item =>
            {
                if (item.KeepSepInmateId > 0 && item.Housing is null)
                {
                    HousingDetail housing = _context.Inmate
                        .Where(w => w.InmateId == item.KeepSepInmateId
                                    && w.HousingUnitId.HasValue)
                        .Select(h => new HousingDetail
                        {
                            HousingUnitLocation = h.HousingUnit.HousingUnitLocation,
                            HousingUnitNumber = h.HousingUnit.HousingUnitNumber,
                            HousingUnitBedNumber = h.HousingUnit.HousingUnitBedNumber,
                            HousingUnitBedLocation = h.HousingUnit.HousingUnitBedLocation,
                            HousingUnitListId = h.HousingUnit.HousingUnitListId,
                            HousingUnitId = h.HousingUnitId ?? 0
                        }).FirstOrDefault();
                    item.Housing = housing;
                }

                if (item.PersonId > 0)
                {
                    Person person = lstPerson.Single(p => p.PersonId == item.PersonId);

                    item.PersonLastName = person.PersonLastName;
                    item.PersonMiddleName = person.PersonMiddleName;
                    item.PersonFirstName = person.PersonFirstName;

                    item.PhotoFilePath = _photos.GetPhotoByCollectionIdentifier(person.Identifiers);
                }

                if (item.KeepSepPersonId > 0)
                {
                    Person person = lstPerson.Single(p => p.PersonId == item.KeepSepPersonId);

                    item.KeepSepPersonLastName = person.PersonLastName;
                    item.KeepSepPersonFirstName = person.PersonFirstName;
                    item.KeepSepPersonMiddleName = person.PersonMiddleName;

                    item.PhotoFilePath = _photos.GetPhotoByCollectionIdentifier(person.Identifiers);
                }

                if (!(item.Housing is null))
                {
                    item.HousingBedGroupId = _context.HousingUnit.Where(a =>
                            a.HousingUnitId == item.Housing.HousingUnitId &&
                            a.HousingUnitListBedGroup.DeleteFlag == 0 &&
                            a.HousingUnitListBedGroupId > 0
                            && a.HousingUnitBedGroupKeepsepConflictCheck == 1)
                        .Select(s => s.HousingUnitListBedGroupId ?? 0)
                        .FirstOrDefault();
                }
            });

            return listHousingConflictVm;
        }

        public List<HousingConflictVm> GetHousingConflictVm(HousingInputVm value)
        {
            // To get Inmate details. 
            IQueryable<Inmate> lstInmate = _context.Inmate
                .Where(f => f.FacilityId == value.FacilityId
                            && f.InmateActive == 1);

            IEnumerable<HousingDetail> lstInmateHousings = lstInmate
                .Where(w => w.HousingUnitId.HasValue && w.HousingUnit.HousingUnitListId == value.HousingUnitListId)
                .Select(s => new HousingDetail
                {
                    HousingUnitListId = s.HousingUnit.HousingUnitListId,
                    HousingUnitBedNumber = s.HousingUnit.HousingUnitBedNumber,
                    HousingUnitBedLocation = s.HousingUnit.HousingUnitBedLocation
                });

            //To get Housing Inmate details based on bed number
            if (!(value.HousingBedNumber is null))
            {
                // To get Housing Inmate details based on bed location.
                lstInmateHousings = lstInmateHousings.Where(w =>
                    w.HousingUnitListId == value.HousingUnitListId
                    && w.HousingUnitBedNumber == value.HousingBedNumber);
            }

            // To get Housing Inmate details based on bed location.
            if (!(value.HousingBedLocation is null))
            {
                // To get Housing Inmate details based on bed location.
                lstInmateHousings = lstInmateHousings.Where(w =>
                    w.HousingUnitListId == value.HousingUnitListId
                    && w.HousingUnitBedLocation ==
                    value.HousingBedLocation);
            }

            IEnumerable<HousingUnit> lstHousingUnit = _context.HousingUnit
                .Where(h => h.FacilityId == value.FacilityId
                            && h.HousingUnitListId == value.HousingUnitListId
                            && (!h.HousingUnitInactive.HasValue ||
                                h.HousingUnitInactive == 0));

            if (!(value.HousingBedNumber is null))
            {
                // To get Housing Unit details based on bed location.
                lstHousingUnit =
                    lstHousingUnit.Where(h => h.HousingUnitBedNumber == value.HousingBedNumber);
            }

            if (!(value.HousingBedLocation is null))
            {
                // To get Housing Unit details based on bed location.
                lstHousingUnit =
                    lstHousingUnit.Where(h => h.HousingUnitBedLocation == value.HousingBedLocation);
            }

            List<HousingUnit> housingUnits = lstHousingUnit.ToList();
            HousingCapacityVm housingCapacity = new HousingCapacityVm
            {
                CurrentCapacity = housingUnits.Sum(c => c.HousingUnitActualCapacity ?? 0),
                OutofService = housingUnits.Sum(c => c.HousingUnitOutOfService ?? 0),
                Assigned = lstInmateHousings.ToList().Count
            };
            List<HousingConflictVm> lstHousingConflictDetails = new List<HousingConflictVm>();

            if (housingCapacity.OutofService > 0)
            {
                // To get Housing out of service details.
                lstHousingConflictDetails.Add(new HousingConflictVm
                {
                    ConflictType = ConflictTypeConstants.HOUSINGOUTOFSERVICE
                });
            }

            // To get site option value of no house class.
            int siteOptionsOverCap = _context.SiteOptions
                .Count(w => w.SiteOptionsName == SiteOptionsConstants.DONOTALLOWOVERCAPACITYHOUSING
                            && w.SiteOptionsStatus == "1" &&
                            w.SiteOptionsValue == SiteOptionsConstants.OFF);

            if (housingCapacity.Assigned >= housingCapacity.CurrentCapacity)
            {
                // To get Housing over capacity details.
                lstHousingConflictDetails.Add(new HousingConflictVm
                {
                    ConflictType = ConflictTypeConstants.HOUSINGOVERCAPACITY,
                    Immovable = siteOptionsOverCap <= 0
                });
            }

            return lstHousingConflictDetails.ToList();
        }

        public List<HousingConflictVm> GetKeepSepDetails(int inmateId, int facilityId, string housingUnitLocation,
            int housingUnitListId)
        {
            List<HousingConflictVm> housingConflictVms;
            if (housingUnitListId > 0)
            {
                housingConflictVms = GetHousingKeepSeparate(inmateId, facilityId)
                    .Where(w => w.Housing.HousingUnitListId == housingUnitListId).ToList();

                if (housingConflictVms.Count > 0)
                {
                    List<HousingConflictVm> podHousingConflictVms = housingConflictVms.ToList();

                    List<HousingCapacityVm> housingBedGroupDetails = _context.HousingUnit
                        .Where(w => w.FacilityId == facilityId &&
                                    (!w.HousingUnitInactive.HasValue || w.HousingUnitInactive == 0) &&
                                    w.HousingUnitListId == housingUnitListId).Select(s =>
                            new HousingCapacityVm
                            {
                                HousingLocation = s.HousingUnitLocation,
                                HousingNumber = s.HousingUnitNumber,
                                HousingBedNumber = s.HousingUnitBedNumber,
                                HousingBedLocation = s.HousingUnitBedLocation,
                                HousingUnitListId = s.HousingUnitListId,
                                HousingId = s.HousingUnitId,
                                HousingUnitBedGroupId = s.HousingUnitListBedGroupId ?? 0,
                                KeepSep = s.HousingUnitBedGroupKeepsepConflictCheck,
                                Gender = s.HousingUnitNumberKeepsepConflictCheck
                            }).ToList();

                    housingBedGroupDetails.Where(w => w.HousingUnitBedGroupId > 0 && w.KeepSep > 0).ToList().ForEach(
                        f =>
                        {
                            if (podHousingConflictVms.Any(a =>
                                a.HousingBedGroupId == f.HousingUnitBedGroupId && a.HousingBedGroupId > 0 &&
                                f.HousingUnitBedGroupId > 0))
                            {
                                housingConflictVms.AddRange(podHousingConflictVms
                                    .Where(w => w.HousingBedGroupId == f.HousingUnitBedGroupId).Select(s =>
                                        new HousingConflictVm
                                        {
                                            ConflictType = s.ConflictType == KeepSepLabel.ASSOCKEEPSEPASSOC
                                                ? KeepSepLabel.ASSOCKEEPSEPASSOCBEDGROUP
                                                : s.ConflictType == KeepSepLabel.ASSOCKEEPSEPINMATE
                                                    ? KeepSepLabel.ASSOCKEEPSEPINMATEBEDGROUP
                                                    : s.ConflictType == KeepSepLabel.ASSOCKEEPSEPSUBSET
                                                        ? KeepSepLabel.ASSOCKEEPSEPSUBSETBEDGROUP
                                                        : s.ConflictType == KeepSepLabel.INMATEKEEPSEPASSOC
                                                            ? KeepSepLabel.INMATEKEEPSEPASSOCBEDGROUP
                                                            : s.ConflictType == KeepSepLabel.INMATEKEEPSEPINMATE
                                                                ? KeepSepLabel.INMATEKEEPSEPINMATEBEDGROUP
                                                                : s.ConflictType == KeepSepLabel.INMATEKEEPSEPSUBSET
                                                                    ? KeepSepLabel.INMATEKEEPSEPSUBSETBEDGROUP
                                                                    : s.ConflictType == KeepSepLabel.SUBSETKEEPSEPASSOC
                                                                        ? KeepSepLabel.SUBSETKEEPSEPASSOCBEDGROUP
                                                                        : s.ConflictType ==
                                                                          KeepSepLabel.SUBSETKEEPSEPINMATE
                                                                            ? KeepSepLabel.SUBSETKEEPSEPINMATEBEDGROUP
                                                                            : KeepSepLabel.SUBSETKEEPSEPSUBSETBEDGROUP,
                                            KeepSepPersonId = s.KeepSepPersonId,
                                            KeepSepInmateId = s.KeepSepInmateId,
                                            KeepSepInmateNumber = s.KeepSepInmateNumber,
                                            PersonId = s.PersonId,
                                            InmateId = s.InmateId,
                                            InmateNumber = s.InmateNumber,
                                            KeepSepType = s.KeepSepType,
                                            ConflictDescription = s.ConflictDescription,
                                            Housing = new HousingDetail
                                            {
                                                HousingUnitLocation = f.HousingLocation,
                                                HousingUnitNumber = f.HousingNumber,
                                                HousingUnitBedNumber = f.HousingBedNumber,
                                                HousingUnitBedLocation = f.HousingBedLocation,
                                                HousingUnitListId = f.HousingUnitListId
                                            },
                                            AssignConflictType = s.AssignConflictType,
                                            KeepSepAssoc1 = s.KeepSepAssoc1,
                                            KeepSepAssoc2 = s.KeepSepAssoc2,
                                            KeepSepAssocSubset1 = s.KeepSepAssocSubset1,
                                            KeepSepAssocSubset2 = s.KeepSepAssocSubset2,
                                            PersonLastName = s.PersonLastName,
                                            PersonMiddleName = s.PersonMiddleName,
                                            PersonFirstName = s.PersonFirstName,
                                            PhotoFilePath = s.PhotoFilePath,
                                            KeepSepPersonLastName = s.KeepSepPersonLastName,
                                            KeepSepPersonFirstName = s.KeepSepPersonFirstName,
                                            KeepSepPersonMiddleName = s.KeepSepPersonMiddleName,
                                            KeepSepAssoc1Id = s.KeepSepAssoc1Id,
                                            KeepSepAssoc2Id = s.KeepSepAssoc2Id,
                                            KeepSepAssocSubset1Id = s.KeepSepAssocSubset1Id,
                                            KeepSepAssocSubset2Id = s.KeepSepAssocSubset2Id,
                                        }).ToList());
                            }
                        });

                    housingBedGroupDetails.Where(w => w.Gender > 0).ToList().ForEach(f =>
                    {
                        housingConflictVms.AddRange(podHousingConflictVms.Select(s =>
                            new HousingConflictVm
                            {
                                ConflictType = s.ConflictType == KeepSepLabel.ASSOCKEEPSEPASSOC
                                    ? KeepSepLabel.ASSOCKEEPSEPASSOCBEDGROUP
                                    : s.ConflictType == KeepSepLabel.ASSOCKEEPSEPINMATE
                                        ? KeepSepLabel.ASSOCKEEPSEPINMATEBEDGROUP
                                        : s.ConflictType == KeepSepLabel.ASSOCKEEPSEPSUBSET
                                            ? KeepSepLabel.ASSOCKEEPSEPSUBSETBEDGROUP
                                            : s.ConflictType == KeepSepLabel.INMATEKEEPSEPASSOC
                                                ? KeepSepLabel.INMATEKEEPSEPASSOCBEDGROUP
                                                : s.ConflictType == KeepSepLabel.INMATEKEEPSEPINMATE
                                                    ? KeepSepLabel.INMATEKEEPSEPINMATEBEDGROUP
                                                    : s.ConflictType == KeepSepLabel.INMATEKEEPSEPSUBSET
                                                        ? KeepSepLabel.INMATEKEEPSEPSUBSETBEDGROUP
                                                        : s.ConflictType == KeepSepLabel.SUBSETKEEPSEPASSOC
                                                            ? KeepSepLabel.SUBSETKEEPSEPASSOCBEDGROUP
                                                            : s.ConflictType == KeepSepLabel.SUBSETKEEPSEPINMATE
                                                                ? KeepSepLabel.SUBSETKEEPSEPINMATEBEDGROUP
                                                                : KeepSepLabel.SUBSETKEEPSEPSUBSETBEDGROUP,
                                KeepSepPersonId = s.KeepSepPersonId,
                                KeepSepInmateId = s.KeepSepInmateId,
                                KeepSepInmateNumber = s.KeepSepInmateNumber,
                                PersonId = s.PersonId,
                                InmateId = s.InmateId,
                                InmateNumber = s.InmateNumber,
                                KeepSepType = s.KeepSepType,
                                ConflictDescription = s.ConflictDescription,
                                Housing = new HousingDetail
                                {
                                    HousingUnitLocation = f.HousingLocation,
                                    HousingUnitNumber = f.HousingNumber,
                                    HousingUnitBedNumber = f.HousingBedNumber,
                                    HousingUnitBedLocation = f.HousingBedLocation,
                                    HousingUnitListId = f.HousingUnitListId
                                },
                                AssignConflictType = s.AssignConflictType,
                                KeepSepAssoc1 = s.KeepSepAssoc1,
                                KeepSepAssoc2 = s.KeepSepAssoc2,
                                KeepSepAssocSubset1 = s.KeepSepAssocSubset1,
                                KeepSepAssocSubset2 = s.KeepSepAssocSubset2,
                                PersonLastName = s.PersonLastName,
                                PersonMiddleName = s.PersonMiddleName,
                                PersonFirstName = s.PersonFirstName,
                                PhotoFilePath = s.PhotoFilePath,
                                KeepSepPersonLastName = s.KeepSepPersonLastName,
                                KeepSepPersonFirstName = s.KeepSepPersonFirstName,
                                KeepSepPersonMiddleName = s.KeepSepPersonMiddleName,
                                KeepSepAssoc1Id = s.KeepSepAssoc1Id,
                                KeepSepAssoc2Id = s.KeepSepAssoc2Id,
                                KeepSepAssocSubset1Id = s.KeepSepAssocSubset1Id,
                                KeepSepAssocSubset2Id = s.KeepSepAssocSubset2Id,
                            }).ToList());
                    });
                }
            }
            else
            {
                housingConflictVms = GetHousingKeepSeparate(inmateId, facilityId)
                    .Where(w => w.Housing.HousingUnitLocation == housingUnitLocation).ToList();
            }

            return housingConflictVms;
        }

        private int GetSeparationCount(List<PersonFlag> sepFlag)
        {
            int count = sepFlag.SelectMany(p => _context.Lookup.Where(w =>
                w.LookupInactive == 0 && w.LookupFlag6 == 1 &&
                w.LookupType == LookupConstants.PERSONCAUTION && w.LookupIndex == p.PersonFlagIndex &&
                p.PersonFlagIndex > 0
                || w.LookupType == LookupConstants.TRANSCAUTION
                && w.LookupIndex == p.InmateFlagIndex
                && p.InmateFlagIndex > 0
                && w.LookupFlag6 == 1
            )).Count();
            return count;
        }

        public List<HousingConflictVm> GetHousingConflictNotification(HousingInputVm value)
        {
            // To get Inmate details. 
            IQueryable<Inmate> lstInmate = _context.Inmate
                .Where(f => f.FacilityId == value.FacilityId
                            && f.InmateActive == 1);

            IEnumerable<HousingDetail> lstInmateHousings = lstInmate
                .Where(w => w.HousingUnitId.HasValue && w.HousingUnit.HousingUnitListId == value.HousingUnitListId)
                .Select(s => new HousingDetail
                {
                    HousingUnitLocation = s.HousingUnit.HousingUnitLocation,
                    HousingUnitListId = s.HousingUnit.HousingUnitListId,
                    HousingUnitBedNumber = s.HousingUnit.HousingUnitBedNumber,
                    HousingUnitBedLocation = s.HousingUnit.HousingUnitBedLocation
                });


            IEnumerable<HousingUnit> lstHousingUnit = _context.HousingUnit
                .Where(h => h.FacilityId == value.FacilityId &&
                            (!h.HousingUnitInactive.HasValue || h.HousingUnitInactive == 0));

            List<HousingCapacityVm> housingCapacityVms = new List<HousingCapacityVm>();

            if (value.HousingUnitListId == 0)
            {
                lstInmateHousings = lstInmateHousings
                    .Where(w => w.HousingUnitLocation == value.HousingLocation);

                housingCapacityVms = lstHousingUnit
                    .Where(w => w.HousingUnitLocation == value.HousingLocation)
                    .OrderBy(o => o.HousingUnitNumber)
                    .GroupBy(x => new {x.HousingUnitListId, x.HousingUnitNumber})
                    .Select(s => new HousingCapacityVm
                    {
                        FacilityId = value.FacilityId,
                        HousingLocation = value.HousingLocation,
                        HousingNumber = s.Key.HousingUnitNumber,
                        HousingUnitListId = s.Key.HousingUnitListId,
                        CurrentCapacity = s.Sum(c => c.HousingUnitActualCapacity ?? 0),
                        Assigned = lstInmateHousings.Count(c => c.HousingUnitListId == s.Key.HousingUnitListId),
                        OutofService = s.Sum(c => c.HousingUnitOutOfService ?? 0),
                        HavingNextLevel = s.Any(a => a.HousingUnitLocation == value.HousingLocation
                                                     && a.HousingUnitNumber == s.Key.HousingUnitNumber
                                                     && a.HousingUnitBedNumber != null),
                        ClassifyConflictCheck = true,
                        HousingClassifyString = s.Where(w => w.HousingUnitClassifyRecString != null)
                            .Select(se => se.HousingUnitClassifyRecString.Split(','))
                            .SelectMany(se => se).Distinct().ToList(),
                        HousingFlag = s.Where(se => !string.IsNullOrEmpty(se.HousingUnitFlagString))
                            .SelectMany(se => se.HousingUnitFlagString.Split(',')).ToList(),
                        Gender = s.Select(se => se.HousingUnitSex ?? 0).FirstOrDefault(),
                        HousingUnitBedGroupId = s.Where(w => w.HousingUnitBedGroupKeepsepConflictCheck > 0)
                            .Select(se => se.HousingUnitListBedGroupId ?? 0).FirstOrDefault()
                    }).ToList();
            }
            else if (value.HousingBedNumber is null)
            {

                lstInmateHousings = lstInmateHousings
                    .Where(w => w.HousingUnitListId == value.HousingUnitListId);

                housingCapacityVms = lstHousingUnit
                    .Where(w => w.HousingUnitListId == value.HousingUnitListId)
                    .OrderBy(o => o.HousingUnitBedNumber)
                    .GroupBy(g => new {g.HousingUnitNumber, g.HousingUnitBedNumber})
                    .Select(s => new HousingCapacityVm
                    {
                        FacilityId = value.FacilityId,
                        HousingLocation = value.HousingLocation,
                        HousingNumber = s.Key.HousingUnitNumber,
                        HousingBedNumber = s.Key.HousingUnitBedNumber,
                        HousingUnitListId = value.HousingUnitListId,
                        CurrentCapacity = s.Sum(c => c.HousingUnitActualCapacity ?? 0),
                        Assigned = lstInmateHousings.Count(c => c.HousingUnitListId == value.HousingUnitListId
                                                                && c.HousingUnitBedNumber ==
                                                                s.Key.HousingUnitBedNumber),
                        OutofService = s.Sum(c => c.HousingUnitOutOfService ?? 0),
                        HavingNextLevel = s.Any(a => a.HousingUnitListId == value.HousingUnitListId
                                                     && a.HousingUnitBedNumber == s.Key.HousingUnitBedNumber
                                                     && a.HousingUnitBedLocation != null),
                        HousingClassifyString = s.Where(w => w.HousingUnitClassifyRecString != null)
                            .Select(se => se.HousingUnitClassifyRecString.Split(','))
                            .SelectMany(se => se).Distinct().ToList(),
                        HousingFlag = s.Where(se => !string.IsNullOrEmpty(se.HousingUnitFlagString))
                            .SelectMany(se => se.HousingUnitFlagString.Split(',')).ToList(),
                        Gender = s.Select(se => se.HousingUnitSex ?? 0).FirstOrDefault(),
                        HousingUnitBedGroupId = s.Where(w => w.HousingUnitBedGroupKeepsepConflictCheck > 0)
                            .Select(se => se.HousingUnitListBedGroupId ?? 0).FirstOrDefault()
                    }).ToList();
            }
            else if (value.HousingBedLocation is null)
            {
                // To get Housing Inmate details based on bed location.
                lstInmateHousings = lstInmateHousings.Where(w => w.HousingUnitListId == value.HousingUnitListId
                                                                 && w.HousingUnitBedNumber == value.HousingBedNumber);


                housingCapacityVms = lstHousingUnit
                    .Where(w => w.HousingUnitListId == value.HousingUnitListId &&
                                w.HousingUnitBedNumber == value.HousingBedNumber)
                    .OrderBy(o => o.HousingUnitBedNumber)
                    .GroupBy(g => new {g.HousingUnitNumber, g.HousingUnitBedNumber, g.HousingUnitBedLocation})
                    .Select(s => new HousingCapacityVm
                    {
                        FacilityId = value.FacilityId,
                        HousingLocation = value.HousingLocation,
                        HousingNumber = s.Key.HousingUnitNumber,
                        HousingBedNumber = s.Key.HousingUnitBedNumber,
                        HousingBedLocation = s.Key.HousingUnitBedLocation,
                        HousingUnitListId = value.HousingUnitListId,
                        CurrentCapacity = s.Sum(c => c.HousingUnitActualCapacity ?? 0),
                        Assigned = lstInmateHousings.Count(c => c.HousingUnitListId == value.HousingUnitListId
                                                                && c.HousingUnitBedNumber ==
                                                                s.Key.HousingUnitBedNumber &&
                                                                c.HousingUnitBedLocation ==
                                                                s.Key.HousingUnitBedLocation),
                        OutofService = s.Sum(c => c.HousingUnitOutOfService ?? 0),
                        HavingNextLevel = s.Any(a => a.HousingUnitListId == value.HousingUnitListId
                                                     && a.HousingUnitBedNumber == s.Key.HousingUnitBedNumber
                                                     && a.HousingUnitBedLocation != null),
                        HousingClassifyString = s.Where(w => w.HousingUnitClassifyRecString != null)
                            .Select(se => se.HousingUnitClassifyRecString.Split(','))
                            .SelectMany(se => se).Distinct().ToList(),
                        HousingFlag = s.Where(se => !string.IsNullOrEmpty(se.HousingUnitFlagString))
                            .SelectMany(se => se.HousingUnitFlagString.Split(',')).ToList(),
                        Gender = s.Select(se => se.HousingUnitSex ?? 0).FirstOrDefault(),
                        HousingUnitBedGroupId = s.Where(w => w.HousingUnitBedGroupKeepsepConflictCheck > 0)
                            .Select(se => se.HousingUnitListBedGroupId ?? 0).FirstOrDefault()
                    }).ToList();

            }

            string inmateClassify = _context.Inmate.Where(i => i.InmateId == value.InmateId)
                .Select(s => s.InmateClassification.InmateClassificationReason).SingleOrDefault();

            List<HousingConflictVm> lstHousingConflictDetails = new List<HousingConflictVm>();

            int siteOptionsOverCap = _context.SiteOptions
                .Count(w => w.SiteOptionsName == SiteOptionsConstants.DONOTALLOWOVERCAPACITYHOUSING
                            && w.SiteOptionsStatus == "1" &&
                            w.SiteOptionsValue == SiteOptionsConstants.OFF);

            int currentCapacity = housingCapacityVms.Sum(c => c.CurrentCapacity);
            int assigned = housingCapacityVms.Sum(c => c.Assigned);

            if (assigned >= currentCapacity)
            {
                // To get Housing over capacity details.
                lstHousingConflictDetails.Add(new HousingConflictVm
                {
                    ConflictType = ConflictTypeConstants.HOUSINGOVERCAPACITY,
                    Immovable = siteOptionsOverCap <= 0
                });
            }

            if (housingCapacityVms.Any() && !(inmateClassify is null) &&
                inmateClassify != RequestType.PENDINGREQUEST && housingCapacityVms.Any(w =>
                    w.HousingClassifyString.Any()
                    && !w.HousingClassifyString.Contains(inmateClassify)))
            {
                // To get Housing out of service details.
                lstHousingConflictDetails.Add(new HousingConflictVm
                {
                    ConflictType = ConflictTypeConstants.HOUSINGCLASSIFYCONFLICT,
                    Description = inmateClassify
                });
            }

            // To get the person flag list
            IQueryable<PersonFlag> personFlag = _context.PersonFlag
                .Where(w => w.Person.Inmate.Any(s => s.InmateId == value.InmateId) && w.DeleteFlag == 0
                                                                                   && (w.InmateFlagIndex > 0 ||
                                                                                       w.PersonFlagIndex > 0));

            List<string> housingFlags = personFlag.SelectMany(
                p => _context.Lookup.Where(w => w.LookupInactive == 0 && w.LookupFlag7 == 1 &&
                                                w.LookupType == LookupConstants.PERSONCAUTION
                                                && w.LookupIndex == p.PersonFlagIndex && p.PersonFlagIndex > 0 ||
                                                w.LookupType == LookupConstants.TRANSCAUTION
                                                && w.LookupIndex == p.InmateFlagIndex &&
                                                p.InmateFlagIndex > 0 && w.LookupFlag7 == 1),
                (p, l) => l.LookupDescription.Replace("'", "")).ToList();

            bool hFlag = housingCapacityVms.Any(a => housingFlags.Any() &&
                                                     (!a.HousingFlag.Any() ||
                                                      a.HousingFlag.Any() &&
                                                      !housingFlags.All(b => a.HousingFlag.Any(o => o == b))));

            List<string> housingFlagAll = housingCapacityVms.SelectMany(s => s.HousingFlag).Distinct().ToList();

            //V2-3727 JMS_Recommend housing_Not displaying entire list
            if (hFlag)
            {
                //To get Housing out of service details.
                lstHousingConflictDetails.Add(new HousingConflictVm
                {
                    ConflictType = ConflictTypeConstants.HOUSINGFLAGCONFLICT,
                    ListDescription = housingFlagAll.Any()
                        ? housingFlags.Except(housingFlagAll).Any() ? housingFlags.Except(housingFlagAll).ToList() :
                        housingFlags
                        : housingFlags
                });
            }

            // To get the lookup value.
            Dictionary<int, string> dict = _context.Lookup
                .Where(l => l.LookupInactive == 0 && l.LookupType == LookupConstants.SEX)
                .Select(s => new {s.LookupIndex, s.LookupDescription})
                .ToDictionary(p => p.LookupIndex, p => p.LookupDescription);

            // To get the gender details that are based on housing.
            List<KeyValuePair<int, int>> inmateListSexLast = housingCapacityVms
                .Where(s => s.Gender > 0
                            && dict.Keys.Contains(s.Gender))
                .Select(s => new KeyValuePair<int, int>(s.Gender, 1))
                .ToList();

            if (string.IsNullOrEmpty(value.HousingBedNumber))
            {
                // To get inmate gender details details based on bed number.
                inmateListSexLast.AddRange(lstInmate
                    .Where(h => h.HousingUnitId.HasValue && h.Person.PersonSexLast.HasValue
                                                         && h.Person.PersonSexLast > 0
                                                         && dict.Keys.Contains(h.Person.PersonSexLast.Value)
                                                         && h.HousingUnit.HousingUnitListId == value.HousingUnitListId)
                    .Select(h => new KeyValuePair<int, int>(h.Person.PersonSexLast.Value, 2)).Distinct());
            }
            else if (!string.IsNullOrEmpty(value.HousingBedNumber))
            {
                // To get inmate gender details details based on bed number.
                inmateListSexLast.AddRange(lstInmate
                    .Where(h => h.HousingUnitId.HasValue && h.Person.PersonSexLast.HasValue
                                                         && h.Person.PersonSexLast > 0
                                                         && dict.Keys.Contains(h.Person.PersonSexLast.Value)
                                                         && h.HousingUnit.HousingUnitListId == value.HousingUnitListId
                                                         && h.HousingUnit.HousingUnitBedNumber ==
                                                         value.HousingBedNumber)
                    .Select(h => new KeyValuePair<int, int>(h.Person.PersonSexLast.Value, 2)).Distinct());
            }
            else
            {
                // To get inmate gender details details based on bed location.
                inmateListSexLast.AddRange(lstInmate
                    .Where(h => h.HousingUnitId.HasValue && h.Person.PersonSexLast.HasValue
                                                         && h.Person.PersonSexLast > 0
                                                         && dict.Keys.Contains(h.Person.PersonSexLast.Value)
                                                         && h.HousingUnit.HousingUnitLocation == value.HousingLocation)
                    .Select(h => new KeyValuePair<int, int>(h.Person.PersonSexLast.Value, 2)).Distinct());
            }

            inmateListSexLast = inmateListSexLast.Distinct().ToList();

            // To get the gender details of current inmate.
            int gender = _context.Inmate.Where(w => w.InmateId == value.InmateId
                                                    && w.Person.PersonSexLast.HasValue)
                .Select(s => s.Person.PersonSexLast.Value).SingleOrDefault();

            if (gender > 0 && inmateListSexLast.Count > 0)
            {
                // To get the gender conflict details.
                lstHousingConflictDetails.AddRange(inmateListSexLast.Where(g => gender != g.Key
                                                                                && g.Value == 2)
                    .Select(g => new HousingConflictVm
                        {
                            ConflictType = ConflictTypeConstants.INMATEGENDERHOUSINGCONFLICT,
                            Description = dict.Where(w => w.Key == g.Key)
                                .Select(s => s.Value).FirstOrDefault()
                        }
                    ));

                lstHousingConflictDetails.AddRange(inmateListSexLast.Where(g => gender != g.Key
                                                                                && g.Value == 1)
                    .Select(g => new HousingConflictVm
                        {
                            ConflictType = ConflictTypeConstants.INMATEGENDERCONFLICT
                        }
                    ));
            }

            // Bind housing rule and classify conflicts
            List<HousingConflictVm> lstHousingRuleAndClassifyConflictVms =
                GetHousingRuleAndClassifyFlagConflicts(value, inmateClassify);
            lstHousingConflictDetails.AddRange(lstHousingRuleAndClassifyConflictVms);

            List<HousingConflictVm> podHousingConflict =
                GetHousingKeepSeparate(value.InmateId, value.FacilityId).ToList();

            List<HousingConflictVm> housingConflictVms = podHousingConflict
                .Where(w => w.Housing.HousingUnitListId == value.HousingUnitListId).Distinct().ToList();
            if (!(value.HousingBedNumber is null))
            {
                housingConflictVms = housingConflictVms
                    .Where(w => w.Housing.HousingUnitBedNumber == value.HousingBedNumber).Distinct().ToList();
            }

            if (podHousingConflict.Count > 0)
            {
                if (!(value.HousingBedNumber is null))
                {
                    if (_context.HousingUnit.Any(a => a.HousingUnitBedNumber == value.HousingBedNumber
                                                      && a.HousingUnitListId == value.HousingUnitListId
                                                      && a.HousingUnitListBedGroupId > 0 &&
                                                      a.HousingUnitBedGroupKeepsepConflictCheck == 1))
                    {
                        List<int> housingBedGroupId = _context.HousingUnit.Where(a =>
                                a.HousingUnitBedNumber == value.HousingBedNumber
                                && a.HousingUnitListId == value.HousingUnitListId
                                && a.HousingUnitListBedGroupId > 0 && a.HousingUnitBedGroupKeepsepConflictCheck == 1)
                            .Select(s => s.HousingUnitListBedGroupId ?? 0).ToList();

                        List<HousingConflictVm> bedGroupHousingConflict = podHousingConflict.Where(w =>
                            w.Housing.HousingUnitListId == value.HousingUnitListId &&
                            w.HousingBedGroupId > 0 && housingBedGroupId.Contains(w.HousingBedGroupId)).Select(s =>
                            new HousingConflictVm
                            {
                                ConflictType = s.ConflictType == KeepSepLabel.ASSOCKEEPSEPASSOC
                                    ? KeepSepLabel.ASSOCKEEPSEPASSOCBEDGROUP
                                    : s.ConflictType == KeepSepLabel.ASSOCKEEPSEPINMATE
                                        ? KeepSepLabel.ASSOCKEEPSEPINMATEBEDGROUP
                                        : s.ConflictType == KeepSepLabel.ASSOCKEEPSEPSUBSET
                                            ? KeepSepLabel.ASSOCKEEPSEPSUBSETBEDGROUP
                                            : s.ConflictType == KeepSepLabel.INMATEKEEPSEPASSOC
                                                ? KeepSepLabel.INMATEKEEPSEPASSOCBEDGROUP
                                                : s.ConflictType == KeepSepLabel.INMATEKEEPSEPINMATE
                                                    ? KeepSepLabel.INMATEKEEPSEPINMATEBEDGROUP
                                                    : s.ConflictType == KeepSepLabel.INMATEKEEPSEPSUBSET
                                                        ? KeepSepLabel.INMATEKEEPSEPSUBSETBEDGROUP
                                                        : s.ConflictType == KeepSepLabel.SUBSETKEEPSEPASSOC
                                                            ? KeepSepLabel.SUBSETKEEPSEPASSOCBEDGROUP
                                                            : s.ConflictType == KeepSepLabel.SUBSETKEEPSEPINMATE
                                                                ? KeepSepLabel.SUBSETKEEPSEPINMATEBEDGROUP
                                                                : KeepSepLabel.SUBSETKEEPSEPSUBSETBEDGROUP,
                                KeepSepPersonId = s.KeepSepPersonId,
                                KeepSepInmateId = s.KeepSepInmateId,
                                KeepSepInmateNumber = s.KeepSepInmateNumber,
                                PersonId = s.PersonId,
                                InmateId = s.InmateId,
                                InmateNumber = s.InmateNumber,
                                KeepSepType = s.KeepSepType,
                                ConflictDescription = s.ConflictDescription,
                                Housing = s.Housing,
                                AssignConflictType = s.AssignConflictType,
                                KeepSepAssoc1 = s.KeepSepAssoc1,
                                KeepSepAssoc2 = s.KeepSepAssoc2,
                                KeepSepAssocSubset1 = s.KeepSepAssocSubset1,
                                KeepSepAssocSubset2 = s.KeepSepAssocSubset2,
                                PersonLastName = s.PersonLastName,
                                PersonMiddleName = s.PersonMiddleName,
                                PersonFirstName = s.PersonFirstName,
                                PhotoFilePath = s.PhotoFilePath,
                                KeepSepPersonLastName = s.KeepSepPersonLastName,
                                KeepSepPersonFirstName = s.KeepSepPersonFirstName,
                                KeepSepPersonMiddleName = s.KeepSepPersonMiddleName,
                                KeepSepAssoc1Id = s.KeepSepAssoc1Id,
                                KeepSepAssoc2Id = s.KeepSepAssoc2Id,
                                KeepSepAssocSubset1Id = s.KeepSepAssocSubset1Id,
                                KeepSepAssocSubset2Id = s.KeepSepAssocSubset2Id,
                            }).ToList();

                        housingConflictVms.AddRange(bedGroupHousingConflict.OrderBy(o => o.ConflictType));
                    }
                }
                else
                {
                    if (_context.HousingUnit.Any(a => a.HousingUnitListId == value.HousingUnitListId
                                                      && a.HousingUnitListBedGroupId > 0 &&
                                                      a.HousingUnitBedGroupKeepsepConflictCheck == 1))
                    {
                        List<int> housingBedGroupId = _context.HousingUnit.Where(a =>
                                a.HousingUnitListId == value.HousingUnitListId
                                && a.HousingUnitListBedGroupId > 0 && a.HousingUnitBedGroupKeepsepConflictCheck == 1)
                            .Select(s => s.HousingUnitListBedGroupId ?? 0).ToList();

                        List<HousingConflictVm> bedGroupHousingConflict = podHousingConflict.Where(w =>
                            w.Housing.HousingUnitListId == value.HousingUnitListId
                            && w.HousingBedGroupId > 0 && housingBedGroupId.Contains(w.HousingBedGroupId)).Select(s =>
                            new HousingConflictVm
                            {
                                ConflictType = s.ConflictType == KeepSepLabel.ASSOCKEEPSEPASSOC
                                    ? KeepSepLabel.ASSOCKEEPSEPASSOCBEDGROUP
                                    : s.ConflictType == KeepSepLabel.ASSOCKEEPSEPINMATE
                                        ? KeepSepLabel.ASSOCKEEPSEPINMATEBEDGROUP
                                        : s.ConflictType == KeepSepLabel.ASSOCKEEPSEPSUBSET
                                            ? KeepSepLabel.ASSOCKEEPSEPSUBSETBEDGROUP
                                            : s.ConflictType == KeepSepLabel.INMATEKEEPSEPASSOC
                                                ? KeepSepLabel.INMATEKEEPSEPASSOCBEDGROUP
                                                : s.ConflictType == KeepSepLabel.INMATEKEEPSEPINMATE
                                                    ? KeepSepLabel.INMATEKEEPSEPINMATEBEDGROUP
                                                    : s.ConflictType == KeepSepLabel.INMATEKEEPSEPSUBSET
                                                        ? KeepSepLabel.INMATEKEEPSEPSUBSETBEDGROUP
                                                        : s.ConflictType == KeepSepLabel.SUBSETKEEPSEPASSOC
                                                            ? KeepSepLabel.SUBSETKEEPSEPASSOCBEDGROUP
                                                            : s.ConflictType == KeepSepLabel.SUBSETKEEPSEPINMATE
                                                                ? KeepSepLabel.SUBSETKEEPSEPINMATEBEDGROUP
                                                                : KeepSepLabel.SUBSETKEEPSEPSUBSETBEDGROUP,
                                KeepSepPersonId = s.KeepSepPersonId,
                                KeepSepInmateId = s.KeepSepInmateId,
                                KeepSepInmateNumber = s.KeepSepInmateNumber,
                                PersonId = s.PersonId,
                                InmateId = s.InmateId,
                                InmateNumber = s.InmateNumber,
                                KeepSepType = s.KeepSepType,
                                ConflictDescription = s.ConflictDescription,
                                Housing = s.Housing,
                                AssignConflictType = s.AssignConflictType,
                                KeepSepAssoc1 = s.KeepSepAssoc1,
                                KeepSepAssoc2 = s.KeepSepAssoc2,
                                KeepSepAssocSubset1 = s.KeepSepAssocSubset1,
                                KeepSepAssocSubset2 = s.KeepSepAssocSubset2,
                                PersonLastName = s.PersonLastName,
                                PersonMiddleName = s.PersonMiddleName,
                                PersonFirstName = s.PersonFirstName,
                                PhotoFilePath = s.PhotoFilePath,
                                KeepSepPersonLastName = s.KeepSepPersonLastName,
                                KeepSepPersonFirstName = s.KeepSepPersonFirstName,
                                KeepSepPersonMiddleName = s.KeepSepPersonMiddleName
                            }).ToList();

                        housingConflictVms.AddRange(bedGroupHousingConflict.OrderBy(o => o.ConflictType));
                    }
                }
            }

            if (podHousingConflict.Count > 0)
            {
                if (!(value.HousingBedNumber is null) &&
                    _context.HousingUnit.Any(a =>
                        a.HousingUnitListId == value.HousingUnitListId && a.HousingUnitBedNumber != null
                                                                       && a.HousingUnitBedNumber ==
                                                                       value.HousingBedNumber &&
                                                                       a.HousingUnitNumberKeepsepConflictCheck == 1))
                {
                    List<HousingConflictVm> podConflict = podHousingConflict
                        .Where(w => w.Housing.HousingUnitListId == value.HousingUnitListId).Select(s =>
                            new HousingConflictVm
                            {
                                ConflictType = s.ConflictType == KeepSepLabel.ASSOCKEEPSEPASSOC
                                    ? KeepSepLabel.ASSOCKEEPSEPASSOCBEDPOD
                                    : s.ConflictType == KeepSepLabel.ASSOCKEEPSEPINMATE
                                        ? KeepSepLabel.ASSOCKEEPSEPINMATEBEDPOD
                                        : s.ConflictType == KeepSepLabel.ASSOCKEEPSEPSUBSET
                                            ? KeepSepLabel.ASSOCKEEPSEPSUBSETBEDPOD
                                            : s.ConflictType == KeepSepLabel.INMATEKEEPSEPASSOC
                                                ? KeepSepLabel.INMATEKEEPSEPASSOCBEDPOD
                                                : s.ConflictType == KeepSepLabel.INMATEKEEPSEPINMATE
                                                    ? KeepSepLabel.INMATEKEEPSEPINMATEBEDPOD
                                                    : s.ConflictType == KeepSepLabel.INMATEKEEPSEPSUBSET
                                                        ? KeepSepLabel.INMATEKEEPSEPSUBSETBEDPOD
                                                        : s.ConflictType == KeepSepLabel.SUBSETKEEPSEPASSOC
                                                            ? KeepSepLabel.SUBSETKEEPSEPASSOCBEDPOD
                                                            : s.ConflictType == KeepSepLabel.SUBSETKEEPSEPINMATE
                                                                ? KeepSepLabel.SUBSETKEEPSEPINMATEBEDPOD
                                                                : KeepSepLabel.SUBSETKEEPSEPSUBSETBEDPOD,
                                KeepSepPersonId = s.KeepSepPersonId,
                                KeepSepInmateId = s.KeepSepInmateId,
                                KeepSepInmateNumber = s.KeepSepInmateNumber,
                                PersonId = s.PersonId,
                                InmateId = s.InmateId,
                                InmateNumber = s.InmateNumber,
                                KeepSepType = s.KeepSepType,
                                ConflictDescription = s.ConflictDescription,
                                Housing = s.Housing,
                                AssignConflictType = s.AssignConflictType,
                                KeepSepAssoc1 = s.KeepSepAssoc1,
                                KeepSepAssoc2 = s.KeepSepAssoc2,
                                KeepSepAssocSubset1 = s.KeepSepAssocSubset1,
                                KeepSepAssocSubset2 = s.KeepSepAssocSubset2,
                                PersonLastName = s.PersonLastName,
                                PersonMiddleName = s.PersonMiddleName,
                                PersonFirstName = s.PersonFirstName,
                                PhotoFilePath = s.PhotoFilePath,
                                KeepSepPersonLastName = s.KeepSepPersonLastName,
                                KeepSepPersonFirstName = s.KeepSepPersonFirstName,
                                KeepSepPersonMiddleName = s.KeepSepPersonMiddleName,
                                KeepSepAssoc1Id = s.KeepSepAssoc1Id,
                                KeepSepAssoc2Id = s.KeepSepAssoc2Id,
                                KeepSepAssocSubset1Id = s.KeepSepAssocSubset1Id,
                                KeepSepAssocSubset2Id = s.KeepSepAssocSubset2Id,
                            }).ToList();

                    housingConflictVms.AddRange(podConflict.OrderBy(o => o.ConflictType));

                }
                else if (value.HousingUnitListId > 0 && value.HousingBedNumber is null &&
                         _context.HousingUnit.Any(a =>
                             a.HousingUnitListId == value.HousingUnitListId &&
                             a.HousingUnitNumberKeepsepConflictCheck == 1))
                {
                    List<HousingConflictVm> podConflict = podHousingConflict
                        .Where(w => w.Housing.HousingUnitListId == value.HousingUnitListId).Select(s =>
                            new HousingConflictVm
                            {
                                ConflictType = s.ConflictType == KeepSepLabel.ASSOCKEEPSEPASSOC
                                    ? KeepSepLabel.ASSOCKEEPSEPASSOCBEDPOD
                                    : s.ConflictType == KeepSepLabel.ASSOCKEEPSEPINMATE
                                        ? KeepSepLabel.ASSOCKEEPSEPINMATEBEDPOD
                                        : s.ConflictType == KeepSepLabel.ASSOCKEEPSEPSUBSET
                                            ? KeepSepLabel.ASSOCKEEPSEPSUBSETBEDPOD
                                            : s.ConflictType == KeepSepLabel.INMATEKEEPSEPASSOC
                                                ? KeepSepLabel.INMATEKEEPSEPASSOCBEDPOD
                                                : s.ConflictType == KeepSepLabel.INMATEKEEPSEPINMATE
                                                    ? KeepSepLabel.INMATEKEEPSEPINMATEBEDPOD
                                                    : s.ConflictType == KeepSepLabel.INMATEKEEPSEPSUBSET
                                                        ? KeepSepLabel.INMATEKEEPSEPSUBSETBEDPOD
                                                        : s.ConflictType == KeepSepLabel.SUBSETKEEPSEPASSOC
                                                            ? KeepSepLabel.SUBSETKEEPSEPASSOCBEDPOD
                                                            : s.ConflictType == KeepSepLabel.SUBSETKEEPSEPINMATE
                                                                ? KeepSepLabel.SUBSETKEEPSEPINMATEBEDPOD
                                                                : KeepSepLabel.SUBSETKEEPSEPSUBSETBEDPOD,
                                KeepSepPersonId = s.KeepSepPersonId,
                                KeepSepInmateId = s.KeepSepInmateId,
                                KeepSepInmateNumber = s.KeepSepInmateNumber,
                                PersonId = s.PersonId,
                                InmateId = s.InmateId,
                                InmateNumber = s.InmateNumber,
                                KeepSepType = s.KeepSepType,
                                ConflictDescription = s.ConflictDescription,
                                Housing = s.Housing,
                                AssignConflictType = s.AssignConflictType,
                                KeepSepAssoc1 = s.KeepSepAssoc1,
                                KeepSepAssoc2 = s.KeepSepAssoc2,
                                KeepSepAssocSubset1 = s.KeepSepAssocSubset1,
                                KeepSepAssocSubset2 = s.KeepSepAssocSubset2,
                                PersonLastName = s.PersonLastName,
                                PersonMiddleName = s.PersonMiddleName,
                                PersonFirstName = s.PersonFirstName,
                                PhotoFilePath = s.PhotoFilePath,
                                KeepSepPersonLastName = s.KeepSepPersonLastName,
                                KeepSepPersonFirstName = s.KeepSepPersonFirstName,
                                KeepSepPersonMiddleName = s.KeepSepPersonMiddleName,
                                KeepSepAssoc1Id = s.KeepSepAssoc1Id,
                                KeepSepAssoc2Id = s.KeepSepAssoc2Id,
                                KeepSepAssocSubset1Id = s.KeepSepAssocSubset1Id,
                                KeepSepAssocSubset2Id = s.KeepSepAssocSubset2Id,
                            }).ToList();

                    housingConflictVms.AddRange(podConflict.OrderBy(o => o.ConflictType));

                }
            }

            lstHousingConflictDetails.AddRange(housingConflictVms);
            if (value.InmateId > 0)
            {
                HousingConflictVm housingTransferTasks = _inmateService
                    .GetInmateTasks(value.InmateId, TaskValidateType.HousingAssignFromTransfer)
                    .Select(i => new HousingConflictVm
                    {
                        ConflictType = ConflictTypeConstants.TASKNOTCOMPLETE,
                        ConflictDescription = i.Value
                    }).FirstOrDefault();

                if (!(housingTransferTasks is null))
                {
                    lstHousingConflictDetails.Add(housingTransferTasks);
                }
            }

            List<InmateSearchVm> lstPersonFlag = lstInmate.SelectMany(s => _context.PersonFlag
                .Where(w => s.HousingUnit.HousingUnitListId == value.HousingUnitListId
                            && w.DeleteFlag == 0 && (w.InmateFlagIndex > 0 || w.PersonFlagIndex > 0)
                            && w.PersonId == s.PersonId), (s, p) => new InmateSearchVm
            {
                PersonFlagIndex = p.PersonFlagIndex,
                InmateFlagIndex = p.InmateFlagIndex,
                FacilityId = s.FacilityId,
                PersonId = s.PersonId,
                HousingDetail = new HousingDetail
                {
                    HousingUnitId = s.HousingUnitId ?? 0,
                    HousingUnitListId = s.HousingUnit.HousingUnitListId,
                    HousingUnitLocation = s.HousingUnit.HousingUnitLocation,
                    HousingUnitNumber = s.HousingUnit.HousingUnitNumber,
                    HousingUnitBedNumber = s.HousingUnit.HousingUnitBedNumber,
                    HousingUnitBedLocation = s.HousingUnit.HousingUnitBedLocation
                }
            }).ToList();

            if (!(value.HousingBedNumber is null))
            {
                lstPersonFlag = lstPersonFlag.Where(w => w.HousingDetail.HousingUnitBedNumber == value.HousingBedNumber)
                    .ToList();
            }

            List<Lookup> lookupList = _context.Lookup.Where(w => w.LookupInactive == 0 && w.LookupFlag6 == 1 &&
                                                                 w.LookupType == LookupConstants.PERSONCAUTION
                                                                 || w.LookupType == LookupConstants.TRANSCAUTION
                                                                 && w.LookupFlag6 == 1)
                .OrderByDescending(o => o.LookupOrder).ThenBy(t => t.LookupDescription).ToList();

            List<InmateSearchVm> totalSepDetails = lstPersonFlag.SelectMany(p => lookupList.Where(w =>
                    w.LookupInactive == 0 && w.LookupFlag6 == 1 &&
                    w.LookupType == LookupConstants.PERSONCAUTION && w.LookupIndex == p.PersonFlagIndex &&
                    p.PersonFlagIndex > 0
                    || w.LookupType == LookupConstants.TRANSCAUTION
                    && w.LookupIndex == p.InmateFlagIndex
                    && p.InmateFlagIndex > 0
                    && w.LookupFlag6 == 1
                ), (p, look) => new InmateSearchVm
                {
                    FacilityId = p.FacilityId,
                    PersonId = p.PersonId,
                    HousingDetail = p.HousingDetail
                }
            ).ToList();


            if (totalSepDetails.Any())
            {
                List<HousingConflictVm> conflictTotalSep = totalSepDetails.Select(s => new HousingConflictVm
                {
                    ConflictType = ConflictTypeConstants.TOTALSEPHOUSINGCONFLICT,
                    Housing = s.HousingDetail
                }).ToList();

                lstHousingConflictDetails.AddRange(conflictTotalSep);
            }

            // Total separation inmate conflict
            List<PersonFlag> inmateSepFlag = personFlag.Where(
                w => w.Person.Inmate.Any(a => a.InmateActive == 1)).ToList();

            int inmateSepCount = GetSeparationCount(inmateSepFlag);

            if (inmateSepCount > 0)
            {
                int inmateHouseCount;
                if (value.HousingBedNumber is null)
                {
                    inmateHouseCount = lstInmate.Count(
                        w => w.HousingUnit.HousingUnitListId == value.HousingUnitListId);
                }
                else
                {
                    inmateHouseCount = lstInmate.Count(
                        w => w.HousingUnit.HousingUnitListId == value.HousingUnitListId
                             && w.HousingUnit.HousingUnitBedNumber == value.HousingBedNumber);
                }

                if (inmateHouseCount > 0)
                {
                    lstHousingConflictDetails.Add(new HousingConflictVm
                    {
                        ConflictType = ConflictTypeConstants.TOTALSEPINMATECONFLICT
                    });
                }
                else
                {
                    if (value.ViewerFlag)
                    {
                        lstHousingConflictDetails.Add(new HousingConflictVm
                        {
                            ConflictType = ConflictTypeConstants.TOTALSEPBATCHHOUSINGCONFLICT
                        });
                    }
                }
            }

            lstHousingConflictDetails.ForEach(item =>
            {
                PersonInfoVm inmate = _context.Inmate.Where(w => w.InmateActive == 1 & w.InmateId == value.InmateId)
                    .Select(inm => new PersonInfoVm
                    {
                        InmateId = inm.InmateId,
                        InmateNumber = inm.InmateNumber,
                        PersonLastName = inm.Person.PersonLastName,
                        PersonFirstName = inm.Person.PersonFirstName,
                        PersonMiddleName = inm.Person.PersonMiddleName
                    }).SingleOrDefault();

                if (inmate != null)
                {
                    item.InmateId = value.InmateId;
                    item.InmateNumber = inmate.InmateNumber;
                    item.PersonLastName = inmate.PersonLastName;
                    item.PersonFirstName = inmate.PersonFirstName;
                    item.PersonMiddleName = inmate.PersonMiddleName;
                }
            });
            return lstHousingConflictDetails.OrderBy(inmC => inmC.InmateId).ThenBy(d => d.ConflictType).ToList();
        }

        public List<HousingConflictVm> GetHousingRuleAndClassifyFlagConflicts(HousingInputVm value,
            string inmateClassify)
        {
            List<HousingConflictVm> lstHousingConflictVms = new List<HousingConflictVm>();
            List<HousingConflictVm> lstHousingRuleFlags =
                GetHousingRuleFlags(value.InmateId, value.FacilityId);
            List<HousingConflictVm> lstHousingClassifyFlags =
                GetHousingClassifyFlags(value.InmateId, value.FacilityId);
            // For Cell Group Conflicts
            List<HousingConflictVm> lstOrgHousingRuleFlags = lstHousingRuleFlags;
            List<HousingConflictVm> lstOrgHousingClassifyFlags = lstHousingClassifyFlags;

            string flagName = "", className = "";
            if (string.IsNullOrEmpty(value.HousingBedNumber))
            {
                //// To get inmate gender details details based on bed number.
                lstHousingRuleFlags = lstHousingRuleFlags.Where(h =>
                    h.Housing.HousingUnitListId == value.HousingUnitListId).ToList();
                lstHousingClassifyFlags = lstHousingClassifyFlags.Where(h =>
                    h.Housing.HousingUnitListId == value.HousingUnitListId).ToList();

                flagName = ConflictTypeConstants.INMATEFLAGRULEPODCONFLICT;
                className = ConflictTypeConstants.INMATECLASSIFICATIONRULEPODCONFLICT;
            }
            else if (!string.IsNullOrEmpty(value.HousingBedNumber))
            {
                //// To get inmate gender details details based on bed number.
                lstHousingRuleFlags = lstHousingRuleFlags.Where(h =>
                    h.Housing.HousingUnitListId == value.HousingUnitListId
                    && h.Housing.HousingUnitBedNumber == value.HousingBedNumber).ToList();
                lstHousingClassifyFlags = lstHousingClassifyFlags.Where(h =>
                    h.Housing.HousingUnitListId == value.HousingUnitListId
                    && h.Housing.HousingUnitBedNumber == value.HousingBedNumber).ToList();

                flagName = ConflictTypeConstants.INMATEFLAGRULECELLCONFLICT;
                className = ConflictTypeConstants.INMATECLASSIFICATIONRULECELLCONFLICT;
            }

            if (!string.IsNullOrEmpty(flagName) 
                && (lstHousingRuleFlags.Count > 0 || lstHousingClassifyFlags.Count > 0))
            {
                //// To get the rule conflict details
                lstHousingConflictVms.AddRange(GetRuleClassifyConflictInfo(lstHousingRuleFlags, lstHousingClassifyFlags, 
                    flagName, className, inmateClassify, value.InmateId));
            }

            // To bind Cell Group Conflicts
            if(lstOrgHousingRuleFlags.Count > 0 || lstOrgHousingClassifyFlags.Count > 0)
            {
                if (_context.HousingUnit.Any(a => a.HousingUnitListId == value.HousingUnitListId
                        && a.HousingUnitListBedGroupId > 0 
                        && a.HousingUnitBedGroupKeepsepConflictCheck == 1
                        && (string.IsNullOrEmpty(value.HousingBedNumber) 
                            || a.HousingUnitBedNumber == value.HousingBedNumber)))
                {
                    List<int> housingBedGroupId = _context.HousingUnit.Where(a =>
                            a.HousingUnitListId == value.HousingUnitListId
                            && a.HousingUnitListBedGroupId > 0 && a.HousingUnitBedGroupKeepsepConflictCheck == 1
                            && (string.IsNullOrEmpty(value.HousingBedNumber) 
                            || a.HousingUnitBedNumber == value.HousingBedNumber))
                        .Select(s => s.HousingUnitListBedGroupId ?? 0).ToList();

                    lstOrgHousingRuleFlags = lstOrgHousingRuleFlags.Where(h =>
                        h.Housing.HousingUnitListId == value.HousingUnitListId
                        && h.HousingBedGroupId > 0 
                        && housingBedGroupId.Contains(h.HousingBedGroupId)).ToList();
                    lstOrgHousingClassifyFlags = lstOrgHousingClassifyFlags.Where(h =>
                        h.Housing.HousingUnitListId == value.HousingUnitListId
                        && h.HousingBedGroupId > 0 
                        && housingBedGroupId.Contains(h.HousingBedGroupId)).ToList();

                    flagName = ConflictTypeConstants.INMATEFLAGRULECELLGROUPCONFLICT;
                    className = ConflictTypeConstants.INMATECLASSIFICATIONRULECELLGROUPCONFLICT;

                    lstHousingConflictVms.AddRange(GetRuleClassifyConflictInfo(lstOrgHousingRuleFlags, 
                        lstOrgHousingClassifyFlags, flagName, className, inmateClassify, value.InmateId));
                }
            }

            return lstHousingConflictVms;
        }

        private List<HousingConflictVm> GetRuleClassifyConflictInfo(List<HousingConflictVm> lstHousingRuleFlags, 
            List<HousingConflictVm> lstHousingClassifyFlags, string flagName, string className, 
            string inmateClassify, int inmateId)
        {
            List<HousingConflictVm> lstHousingConflictVms = new List<HousingConflictVm>();
            if (lstHousingClassifyFlags.Any())
            {
                List<InmateHousingRuleClassify> lstInmateHousingRuleClassify = 
                    _context.InmateHousingRuleClassify
                        .Where(i => !i.DeleteFlag).ToList();

                List<string> classifies = lstInmateHousingRuleClassify
                    .Where(h => h.InmateClassificationReason1 == inmateClassify)
                    .Select(a => a.InmateClassificationReason2).ToList();
                classifies.AddRange(lstInmateHousingRuleClassify
                    .Where(h => h.InmateClassificationReason2 == inmateClassify)
                    .Select(a => a.InmateClassificationReason1).ToList());
                classifies = classifies.Distinct().OrderBy(a => a).ToList();

                if (classifies.Count > 0)
                {
                    string keepSepClassify = string.Join(", ", classifies);
                    string inmateHousingRuleClassifyInfo =
                        inmateClassify + "|" + keepSepClassify;

                    lstHousingConflictVms.Add(
                        new HousingConflictVm
                        {
                            ConflictType = className,
                            InmateHousingRuleClassifyInfo = inmateHousingRuleClassifyInfo
                        });
                }
            }

            if (lstHousingRuleFlags.Any())
            {
                //// Inmate housing rule flag 
                List<InmateHousingRuleFlag> lstInmateHousingRuleFlags = _context.InmateHousingRuleFlag
                    .Where(i => !i.DeleteFlag).ToList();

                int personId = _context.Inmate.Where(i => i.InmateId == inmateId).Select(a => a.PersonId)
                    .Single();

                List<Lookup> lstLookup = _context.Lookup
                    .Where(w => w.LookupInactive == 0
                        && (w.LookupType == LookupConstants.PERSONCAUTION
                        || w.LookupType == LookupConstants.TRANSCAUTION
                        || w.LookupType == LookupConstants.DIET))
                    .ToList();

                List<PersonFlag> lstPersonFlags = _context.PersonFlag
                    .Where(p => p.PersonId == personId && p.DeleteFlag == 0
                        && (p.InmateFlagIndex > 0 || p.PersonFlagIndex > 0 ||
                                p.DietFlagIndex > 0)).ToList();

                List<int> inmateHousingRuleFlagIds = new List<int>();
                lstInmateHousingRuleFlags.ForEach(item =>
                {
                    if (item.InmateFlagIndex1 > 0)
                    {
                        if (lstPersonFlags.Count(p => p.InmateFlagIndex == item.InmateFlagIndex1
                                                        || p.InmateFlagIndex == item.InmateFlagIndex2) == 2)
                        {
                            inmateHousingRuleFlagIds.Add(item.InmateHousingRuleFlagId);
                        }
                    }
                    else if (item.PersonFlagIndex1 > 0)
                    {
                        if (lstPersonFlags.Count(p => p.PersonFlagIndex == item.PersonFlagIndex1
                                                        || p.PersonFlagIndex == item.PersonFlagIndex2) == 2)
                        {
                            inmateHousingRuleFlagIds.Add(item.InmateHousingRuleFlagId);
                        }
                    }
                    else
                    {
                        if (lstPersonFlags.Count(p => p.DietFlagIndex == item.DietFlagIndex1
                                                        || p.DietFlagIndex == item.DietFlagIndex2) == 2)
                        {
                            inmateHousingRuleFlagIds.Add(item.InmateHousingRuleFlagId);
                        }
                    }
                });

                if (inmateHousingRuleFlagIds.Count > 0)
                {
                    lstInmateHousingRuleFlags = lstInmateHousingRuleFlags
                        .Where(h => !inmateHousingRuleFlagIds.Contains(h.InmateHousingRuleFlagId)).ToList();
                }

                List<KeyValuePair<string, string>> inmateFlagIndexes = lstInmateHousingRuleFlags.Where(h =>
                        lstPersonFlags.Where(s => s.InmateFlagIndex > 0).Select(d => d.InmateFlagIndex)
                            .Contains(h.InmateFlagIndex1))
                    .Select(s => new KeyValuePair<string, string>(
                        lstLookup.Where(l => l.LookupIndex == s.InmateFlagIndex1
                                                && l.LookupType == LookupConstants.TRANSCAUTION)
                            .Select(d => d.LookupDescription).SingleOrDefault(),
                        lstLookup.Where(l => l.LookupIndex == s.InmateFlagIndex2
                                                && l.LookupType == LookupConstants.TRANSCAUTION)
                            .Select(d => d.LookupDescription).SingleOrDefault()
                    )).ToList();
                inmateFlagIndexes.AddRange(lstInmateHousingRuleFlags.Where(h =>
                        lstPersonFlags.Where(s => s.InmateFlagIndex > 0).Select(d => d.InmateFlagIndex)
                            .Contains(h.InmateFlagIndex2))
                    .Select(s => new KeyValuePair<string, string>(
                        lstLookup.Where(l => l.LookupIndex == s.InmateFlagIndex2
                                                && l.LookupType == LookupConstants.TRANSCAUTION)
                            .Select(d => d.LookupDescription).SingleOrDefault(),
                        lstLookup.Where(l => l.LookupIndex == s.InmateFlagIndex1
                                                && l.LookupType == LookupConstants.TRANSCAUTION)
                            .Select(d => d.LookupDescription).SingleOrDefault()
                    )).ToList());

                if (inmateFlagIndexes.Any())
                {
                    List<KeyValuePair<string, string>> keyValuePairsInmateFlags =
                        inmateFlagIndexes.GroupBy(g => g.Key)
                            .Select(s => new KeyValuePair<string, string>
                            (
                                s.Key,
                                string.Join(", ",
                                    inmateFlagIndexes.Where(d => d.Key == s.Key).Select(d => d.Value).ToArray())
                            )).ToList();

                    foreach (KeyValuePair<string, string> item in keyValuePairsInmateFlags)
                    {
                        string inmateHousingRuleFlagInfo =
                            item.Key + "|" + item.Value;

                        lstHousingConflictVms.Add(
                            new HousingConflictVm
                            {
                                ConflictType = flagName,
                                InmateHousingRuleFlagInfo = inmateHousingRuleFlagInfo
                            });
                    }
                }

                List<KeyValuePair<string, string>> personFlagIndexes = lstInmateHousingRuleFlags.Where(h =>
                        lstPersonFlags.Where(s => s.PersonFlagIndex > 0).Select(d => d.PersonFlagIndex)
                            .Contains(h.PersonFlagIndex1))
                    .Select(s => new KeyValuePair<string, string>(
                        lstLookup.Where(l => l.LookupIndex == s.PersonFlagIndex1
                                                && l.LookupType == LookupConstants.PERSONCAUTION)
                            .Select(d => d.LookupDescription).SingleOrDefault(),
                        lstLookup.Where(l => l.LookupIndex == s.PersonFlagIndex2
                                                && l.LookupType == LookupConstants.PERSONCAUTION)
                            .Select(d => d.LookupDescription).SingleOrDefault()
                    )).ToList();
                personFlagIndexes.AddRange(lstInmateHousingRuleFlags.Where(h =>
                        lstPersonFlags.Where(s => s.PersonFlagIndex > 0).Select(d => d.PersonFlagIndex)
                            .Contains(h.PersonFlagIndex2))
                    .Select(s => new KeyValuePair<string, string>(
                        lstLookup.Where(l => l.LookupIndex == s.PersonFlagIndex2
                                                && l.LookupType == LookupConstants.PERSONCAUTION)
                            .Select(d => d.LookupDescription).SingleOrDefault(),
                        lstLookup.Where(l => l.LookupIndex == s.PersonFlagIndex1
                                                && l.LookupType == LookupConstants.PERSONCAUTION)
                            .Select(d => d.LookupDescription).SingleOrDefault()
                    )).ToList());

                if (personFlagIndexes.Any())
                {
                    List<KeyValuePair<string, string>> keyValuePairsPersonFlags =
                        personFlagIndexes.GroupBy(g => g.Key)
                            .Select(s => new KeyValuePair<string, string>
                            (
                                s.Key,
                                string.Join(", ",
                                    personFlagIndexes.Where(d => d.Key == s.Key).Select(d => d.Value).ToArray())
                            )).ToList();

                    foreach (KeyValuePair<string, string> item in keyValuePairsPersonFlags)
                    {
                        string inmateHousingRuleFlagInfo =
                            item.Key + "|" + item.Value;

                        lstHousingConflictVms.Add(
                            new HousingConflictVm
                            {
                                ConflictType = flagName,
                                InmateHousingRuleFlagInfo = inmateHousingRuleFlagInfo
                            });
                    }
                }

                List<KeyValuePair<string, string>> dietFlagIndexes = lstInmateHousingRuleFlags.Where(h =>
                        lstPersonFlags.Where(s => s.DietFlagIndex > 0).Select(d => d.DietFlagIndex)
                            .Contains(h.DietFlagIndex1))
                    .Select(s => new KeyValuePair<string, string>(
                        lstLookup.Where(l => l.LookupIndex == s.DietFlagIndex1
                                                && l.LookupType == LookupConstants.DIET)
                            .Select(d => d.LookupDescription).SingleOrDefault(),
                        lstLookup.Where(l => l.LookupIndex == s.DietFlagIndex2
                                                && l.LookupType == LookupConstants.DIET)
                            .Select(d => d.LookupDescription).SingleOrDefault()
                    )).ToList();
                dietFlagIndexes.AddRange(lstInmateHousingRuleFlags.Where(h =>
                        lstPersonFlags.Where(s => s.DietFlagIndex > 0).Select(d => d.DietFlagIndex)
                            .Contains(h.DietFlagIndex2))
                    .Select(s => new KeyValuePair<string, string>(
                        lstLookup.Where(l => l.LookupIndex == s.DietFlagIndex2
                                                && l.LookupType == LookupConstants.DIET)
                            .Select(d => d.LookupDescription).SingleOrDefault(),
                        lstLookup.Where(l => l.LookupIndex == s.DietFlagIndex1
                                                && l.LookupType == LookupConstants.DIET)
                            .Select(d => d.LookupDescription).SingleOrDefault()
                    )).ToList());

                if (dietFlagIndexes.Any())
                {
                    List<KeyValuePair<string, string>> keyValuePairsDietFlags =
                        dietFlagIndexes.GroupBy(g => g.Key)
                            .Select(s => new KeyValuePair<string, string>
                            (
                                s.Key,
                                string.Join(", ",
                                    dietFlagIndexes.Where(d => d.Key == s.Key).Select(d => d.Value).ToArray())
                            )).ToList();

                    foreach (KeyValuePair<string, string> item in keyValuePairsDietFlags)
                    {
                        string inmateHousingRuleFlagInfo =
                            item.Key + "|" + item.Value;

                        lstHousingConflictVms.Add(
                            new HousingConflictVm
                            {
                                ConflictType = flagName,
                                InmateHousingRuleFlagInfo = inmateHousingRuleFlagInfo
                            });
                    }
                }
            }

            return lstHousingConflictVms;
        }

        public List<HousingConflictVm> GetHousingRuleAndClassifyFlags(int inmateId, int facilityId)
        {
            List<HousingConflictVm> housingConflictVm = GetHousingClassifyFlags(inmateId, facilityId);

            housingConflictVm.AddRange(GetHousingRuleFlags(inmateId, facilityId));

            return housingConflictVm;
        }

        public List<HousingConflictVm> GetHousingRuleFlags(int inmateId, int facilityId)
        {
            IEnumerable<HousingConflictVm> lstPersonFlag;
            if(inmateId > 0)
            {
                //Active inmate list   
                _inmateList = _inmateList ?? _context.Inmate.Where(p => p.InmateActive == 1 && p.FacilityId == facilityId);

                //// Inmate housing rule flag 
                List<InmateHousingRuleFlag> lstInmateHousingRuleFlags = _context.InmateHousingRuleFlag
                    .Where(i => !i.DeleteFlag).ToList();

                int personId = _context.Inmate.Where(i => i.InmateId == inmateId).Select(a => a.PersonId).Single();

                List<PersonFlag> lstPersonFlags = _context.PersonFlag
                    .Where(p => p.PersonId == personId && p.DeleteFlag == 0
                                                    && (p.InmateFlagIndex > 0 || p.PersonFlagIndex > 0 ||
                                                        p.DietFlagIndex > 0)).ToList();

                List<int> inmateHousingRuleFlagIds = new List<int>();
                lstInmateHousingRuleFlags.ForEach(item =>
                {
                    if (item.InmateFlagIndex1 > 0)
                    {
                        if (lstPersonFlags.Count(p => p.InmateFlagIndex == item.InmateFlagIndex1
                                                    || p.InmateFlagIndex == item.InmateFlagIndex2) == 2)
                        {
                            inmateHousingRuleFlagIds.Add(item.InmateHousingRuleFlagId);
                        }
                    }
                    else if (item.PersonFlagIndex1 > 0)
                    {
                        if (lstPersonFlags.Count(p => p.PersonFlagIndex == item.PersonFlagIndex1
                                                    || p.PersonFlagIndex == item.PersonFlagIndex2) == 2)
                        {
                            inmateHousingRuleFlagIds.Add(item.InmateHousingRuleFlagId);
                        }
                    }
                    else
                    {
                        if (lstPersonFlags.Count(p => p.DietFlagIndex == item.DietFlagIndex1
                                                    || p.DietFlagIndex == item.DietFlagIndex2) == 2)
                        {
                            inmateHousingRuleFlagIds.Add(item.InmateHousingRuleFlagId);
                        }
                    }
                });

                if (inmateHousingRuleFlagIds.Count > 0)
                {
                    lstInmateHousingRuleFlags = lstInmateHousingRuleFlags
                        .Where(h => !inmateHousingRuleFlagIds.Contains(h.InmateHousingRuleFlagId)).ToList();
                }

                List<int?> personFlagIndex = lstInmateHousingRuleFlags.Where(h =>
                        lstPersonFlags.Where(a => a.PersonFlagIndex > 0).Select(d => d.PersonFlagIndex).Contains(h.PersonFlagIndex1))
                    .Select(a => a.PersonFlagIndex2).ToList();
                personFlagIndex.AddRange(lstInmateHousingRuleFlags.Where(h =>
                        lstPersonFlags.Where(a => a.PersonFlagIndex > 0).Select(d => d.PersonFlagIndex).Contains(h.PersonFlagIndex2))
                    .Select(a => a.PersonFlagIndex1).ToList());
                personFlagIndex = personFlagIndex.Distinct().ToList();

                List<int?> inmateFlagIndex = lstInmateHousingRuleFlags.Where(h =>
                        lstPersonFlags.Where(a => a.InmateFlagIndex > 0).Select(d => d.InmateFlagIndex).Contains(h.InmateFlagIndex1))
                    .Select(a => a.InmateFlagIndex2).ToList();
                inmateFlagIndex.AddRange(lstInmateHousingRuleFlags.Where(h =>
                        lstPersonFlags.Where(a => a.InmateFlagIndex > 0).Select(d => d.InmateFlagIndex).Contains(h.InmateFlagIndex2))
                    .Select(a => a.InmateFlagIndex1).ToList());
                inmateFlagIndex = inmateFlagIndex.Distinct().ToList();

                List<int?> dietFlagIndex = lstInmateHousingRuleFlags.Where(h =>
                        lstPersonFlags.Where(a => a.DietFlagIndex > 0).Select(d => d.DietFlagIndex).Contains(h.DietFlagIndex1))
                    .Select(a => a.DietFlagIndex2).ToList();
                dietFlagIndex.AddRange(lstInmateHousingRuleFlags.Where(h =>
                        lstPersonFlags.Where(a => a.DietFlagIndex > 0).Select(d => d.DietFlagIndex).Contains(h.DietFlagIndex2))
                    .Select(a => a.DietFlagIndex1).ToList());
                dietFlagIndex = dietFlagIndex.Distinct().ToList();

                if (personFlagIndex.Any() || inmateFlagIndex.Any() || dietFlagIndex.Any())
                {
                    // To get the person classification     
                    lstPersonFlag = _context.PersonFlag
                        .SelectMany(pf => _inmateList.Where(w => pf.PersonId == w.PersonId
                                                                && pf.DeleteFlag == 0
                                                                && (pf.InmateFlagIndex > 0 || pf.PersonFlagIndex > 0 ||
                                                                    pf.DietFlagIndex > 0)
                                                                && w.InmateActive == 1), (pf, i) => new HousingConflictVm
                        {
                            InmateId = i.InmateId,
                            InmateNumber = i.InmateNumber,
                            PersonId = pf.PersonId,
                            PersonFlagIndex = pf.PersonFlagIndex,
                            InmateFlagIndex = pf.InmateFlagIndex,
                            DietFlagIndex = pf.DietFlagIndex,
                            Housing = new HousingDetail
                            {
                                HousingUnitBedNumber = i.HousingUnit.HousingUnitBedNumber,
                                HousingUnitBedLocation = i.HousingUnit.HousingUnitBedLocation,
                                HousingUnitListId = i.HousingUnit.HousingUnitListId,
                                HousingUnitLocation = i.HousingUnit.HousingUnitLocation,
                                HousingUnitNumber = i.HousingUnit.HousingUnitNumber,
                                HousingUnitId = i.HousingUnitId ?? 0
                            },
                            FacilityId = i.FacilityId,
                            FacilityAbbr = i.Facility.FacilityAbbr,
                            PersonClassificationType = i.InmateClassification.InmateClassificationReason,
                            HousingBedGroupId = i.HousingUnit.HousingUnitListBedGroupId ?? 0
                        });

                    lstPersonFlag = lstPersonFlag
                        .Where(p => (!personFlagIndex.Any() 
                            || (personFlagIndex.Contains(p.PersonFlagIndex) && p.PersonFlagIndex > 0))
                        && (!inmateFlagIndex.Any() 
                            || (inmateFlagIndex.Contains(p.InmateFlagIndex) && p.InmateFlagIndex > 0))
                        && (!dietFlagIndex.Any() 
                            || (dietFlagIndex.Contains(p.DietFlagIndex) && p.DietFlagIndex > 0))).ToList();
                }
                else
                {
                    lstPersonFlag = new List<HousingConflictVm>();
                }
            }
            else
            {
                lstPersonFlag = new List<HousingConflictVm>();
            }

            return lstPersonFlag.ToList();
        }

        public List<HousingConflictVm> GetHousingClassifyFlags(int inmateId, int facilityId)
        {
            List<HousingConflictVm> housingConflictVms = new List<HousingConflictVm>();
            if(inmateId > 0)
            {
                string classify = _context.Inmate.Where(i => i.InmateId == inmateId)
                    .Select(a => a.InmateClassification.InmateClassificationReason).SingleOrDefault();

                _inmateList = _context.Inmate.Where(ii =>
                    ii.InmateActive == 1 && ii.FacilityId == facilityId);

                //// Inmate housing rule classify
                List<InmateHousingRuleClassify> lstInmateHousingRuleClassify = _context.InmateHousingRuleClassify
                    .Where(i => !i.DeleteFlag).ToList();

                if (!string.IsNullOrEmpty(classify))
                {
                    List<string> classifies = lstInmateHousingRuleClassify
                        .Where(h => h.InmateClassificationReason1 == classify)
                        .Select(a => a.InmateClassificationReason2).ToList();
                    classifies.AddRange(lstInmateHousingRuleClassify
                        .Where(h => h.InmateClassificationReason2 == classify)
                        .Select(a => a.InmateClassificationReason1).ToList());
                    classifies = classifies.Distinct().ToList();

                    if (classifies.Count > 0)
                    {
                        _inmateList = _inmateList.Where(ii =>
                            classifies.Any(a => ii.InmateClassification.InmateClassificationReason == a));

                        housingConflictVms = _inmateList
                            .Select(i => new HousingConflictVm
                            {
                                InmateId = i.InmateId,
                                PersonId = i.PersonId,
                                InmateNumber = i.InmateNumber,
                                Housing = new HousingDetail
                                {
                                    HousingUnitBedNumber = i.HousingUnit.HousingUnitBedNumber,
                                    HousingUnitBedLocation = i.HousingUnit.HousingUnitBedLocation,
                                    HousingUnitListId = i.HousingUnit.HousingUnitListId,
                                    HousingUnitLocation = i.HousingUnit.HousingUnitLocation,
                                    HousingUnitNumber = i.HousingUnit.HousingUnitNumber,
                                    HousingUnitId = i.HousingUnitId ?? 0,
                                    HousingUnitBedGroupId = i.HousingUnit.HousingUnitListBedGroupId ?? 0
                                },
                                FacilityId = i.FacilityId,
                                FacilityAbbr = i.Facility.FacilityAbbr,
                                PersonClassificationType = i.InmateClassification.InmateClassificationReason,
                                HousingBedGroupId = i.HousingUnit.HousingUnitListBedGroupId ?? 0
                            }).ToList();
                    }
                }
            }            

            return housingConflictVms;
        }

        public List<HousingConflictVm> GetRecommendHousingConflictVm(int inmateId, int facilityId)
        {
            List<HousingConflictVm> housingConflicts = GetHousingKeepSeparate(inmateId, facilityId);

            List<int> bedGroupIds = housingConflicts.Where(w => w.HousingBedGroupId > 0)
                .Select(s => s.HousingBedGroupId).ToList();

            IQueryable<HousingUnit> housingDetails = _context.HousingUnit.Where(w => w.FacilityId == facilityId);

            housingConflicts.AddRange(housingDetails.Where(w => bedGroupIds.Contains(w.HousingUnitListBedGroupId ?? 0)
                                                                && w.HousingUnitBedGroupKeepsepConflictCheck == 1)
                .Select(s => new HousingConflictVm
                {
                    Housing = new HousingDetail
                    {
                        HousingUnitListId = s.HousingUnitListId,
                        HousingUnitLocation = s.HousingUnitLocation,
                        HousingUnitNumber = s.HousingUnitNumber,
                        HousingUnitBedNumber = s.HousingUnitBedNumber,
                        HousingUnitBedLocation = s.HousingUnitBedLocation
                    }
                }));

            housingConflicts.AddRange(housingDetails.Where(w => w.HousingUnitNumberKeepsepConflictCheck == 1
                                                                && housingConflicts
                                                                    .Select(s => s.Housing.HousingUnitListId)
                                                                    .Contains(w.HousingUnitListId))
                .Select(s => new HousingConflictVm
                {
                    Housing = new HousingDetail
                    {
                        HousingUnitListId = s.HousingUnitListId,
                        HousingUnitLocation = s.HousingUnitLocation,
                        HousingUnitNumber = s.HousingUnitNumber,
                        HousingUnitBedNumber = s.HousingUnitBedNumber,
                        HousingUnitBedLocation = s.HousingUnitBedLocation
                    }
                }));


            return housingConflicts;
        }

    }
}
