using GenerateTables.Models;
using Microsoft.AspNetCore.Http;
using ServerAPI.Utilities;
using ServerAPI.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace ServerAPI.Services
{
    public class HousingService : IHousingService
    {
        // Fields       
        private readonly AAtims _context;
        private readonly IHousingConflictService _housingConflictService;
        private readonly IFacilityPrivilegeService _facilityPrivilegeService;
        private readonly ILockdownService _lockdownService;
        private IQueryable<Inmate> _inmateList;
        private IEnumerable<HousingUnit> _listHousingUnit;
        private readonly int _personnelId;
        private readonly IPhotosService _photos;
        private readonly IInterfaceEngineService _interfaceEngineService;

        // Methods
        public HousingService(AAtims context,
            IHousingConflictService housingConflictService, IHttpContextAccessor httpContextAccessor, IPhotosService photosService,
            IFacilityPrivilegeService facilityPrivilegeService,ILockdownService lockdownService,
            IInterfaceEngineService interfaceEngineService)
        {
            _context = context;
            _housingConflictService = housingConflictService;
            _personnelId = Convert.ToInt32(httpContextAccessor.HttpContext.User
               .FindFirst(AuthDetailConstants.PERSONNELID)?.Value);
            _photos = photosService;
            _facilityPrivilegeService = facilityPrivilegeService;
            _lockdownService = lockdownService;
            _interfaceEngineService = interfaceEngineService;
        }

        //To get the Inmate Housing Details
        public InmateHousingVm GetInmateHousingDetails(int inmateId)
        {
            InmateHousingVm inmateHousingVm = new InmateHousingVm
            {
                //To get the Inmate Current Details
                InmateCurrentDetails = GetInmateCurrentDetails(inmateId)
            };

            //To get the Inmate Housing History Details
            List<InmateHousingMoveHistory> housingMoveHistory = _context.HousingUnitMoveHistory
               .Where(w => w.InmateId == inmateId)
               .OrderByDescending(o => o.MoveDate)
               .Select(s => new InmateHousingMoveHistory
               {
                   HousingDetail = new HousingDetail
                   {
                       HousingUnitId = s.HousingUnitTo != null ? s.HousingUnitTo.HousingUnitId : 0,
                       HousingUnitLocation = s.HousingUnitTo.HousingUnitLocation ?? string.Empty,
                       HousingUnitNumber = s.HousingUnitTo.HousingUnitNumber ?? string.Empty,
                       HousingUnitBedNumber = s.HousingUnitTo.HousingUnitBedNumber ?? string.Empty,
                       HousingUnitBedLocation = s.HousingUnitTo.HousingUnitBedLocation ?? string.Empty,
                       FacilityAbbr = s.HousingUnitTo != null ? s.HousingUnitTo.Facility.FacilityAbbr : s.Inmate.Facility.FacilityAbbr
                   },
                   DateIn = s.MoveDate,
                   DateOut = s.MoveDateThru,
                   OfficerIn = new PersonnelVm
                   {
                       PersonFirstName = s.MoveOfficer.PersonNavigation.PersonFirstName,
                       PersonLastName = s.MoveOfficer.PersonNavigation.PersonLastName,
                       OfficerBadgeNumber = s.MoveOfficer.OfficerBadgeNum
                   },
                   OfficerOut = new PersonnelVm
                   {
                       PersonFirstName = s.MoveThruByNavigation.PersonNavigation.PersonFirstName,
                       PersonLastName = s.MoveThruByNavigation.PersonNavigation.PersonLastName,
                       OfficerBadgeNumber = s.MoveThruByNavigation.OfficerBadgeNum
                   },
                   Reason = s.MoveReason
               }).ToList();

            DateTime dtTime = DateTime.Now;

            //To get the Housing Move Incarceration History Details
            inmateHousingVm.IncarcerationHistoryList = _context.Incarceration
                .Where(w => w.InmateId == inmateId).OrderByDescending(o => o.IncarcerationId)
                    .Select(s => new HousingIncarcerationHistory
                    {
                        IncarcerationId = s.IncarcerationId,
                        DateIn = s.DateIn,
                        ReleaseOut = s.ReleaseOut
                    }).ToList();

            inmateHousingVm.IncarcerationHistoryList.ForEach(item =>
                {
                    item.InmateMoveCount = housingMoveHistory
                        .Count(c => item.DateIn <= c.DateIn &&
                        (item.ReleaseOut ?? dtTime) >= (c.DateOut ?? dtTime));
                });

            //To get the Facility based Incarceration History Details
            inmateHousingVm.IncarcerationFacilityHistoryList = _context.IncarcerationFacilityHistory
                .Where(w => w.Incarceration.InmateId == inmateId)
                .OrderByDescending(o => o.MoveDate)
                .Select(s => new HousingIncarcerationHistory
                {
                    DateIn = s.MoveDate,
                    ReleaseOut = s.MoveDateThru,
                    FacilityName = s.Facility.FacilityAbbr,
                    IncarcerationId = s.IncarcerationId
                }).ToList();

            inmateHousingVm.IncarcerationFacilityHistoryList.ForEach(item => 
                item.InmateMoveCount = housingMoveHistory.Count(c => item.DateIn != null &&
                (c.HousingDetail != null ? (c.DateIn ?? dtTime) >= item.DateIn
                    && (c.DateIn ?? dtTime) < (item.ReleaseOut ?? dtTime)
                : (c.DateIn ?? dtTime) >= item.DateIn.Value && (c.DateIn ?? dtTime) <= (item.ReleaseOut ?? dtTime))));

            inmateHousingVm.InmateHousingMoveHistoryList = housingMoveHistory.ToList();

            return inmateHousingVm;
        }

        // To get Inmate current housing, location, classify and flag details
        public InmateCurrentDetails GetInmateCurrentDetails(int inmateId)
        {
            if(inmateId == 0) return new InmateCurrentDetails();
            InmateCurrentDetails inmateCurrentDetails = _context.Inmate
                .Where(w => w.InmateId == inmateId)
                .Select(s => new InmateCurrentDetails
                {
                    HousingDetails = new HousingDetail
                    {
                        HousingUnitListId = s.HousingUnit.HousingUnitListId,
                        HousingUnitId = s.HousingUnitId ?? 0,
                        HousingUnitLocation = s.HousingUnit.HousingUnitLocation,
                        HousingUnitNumber = s.HousingUnit.HousingUnitNumber,
                        HousingUnitBedLocation = s.HousingUnit.HousingUnitBedLocation,
                        HousingUnitBedNumber = s.HousingUnit.HousingUnitBedNumber,
                        FacilityId = s.FacilityId
                    },
                    Person = new PersonInfoVm
                    {
                        PersonId = s.PersonId,
                        PersonFirstName = s.Person.PersonFirstName,
                        PersonLastName = s.Person.PersonLastName,
                        PersonMiddleName = s.Person.PersonMiddleName,
                        InmateNumber = s.InmateNumber
                    },
                    Location = s.InmateCurrentTrack,
                    InmateClassification = s.InmateClassification.InmateClassificationReason,
                    FacilityId = s.FacilityId,
                    FacilityAbbr = s.Facility.FacilityAbbr
                }).Single();

            // To get the person flag list
            IQueryable<PersonFlag> personFlag = _context.PersonFlag
                .Where(w => w.PersonId == inmateCurrentDetails.Person.PersonId && w.DeleteFlag == 0
                && (w.InmateFlagIndex > 0 || w.PersonFlagIndex > 0));

            inmateCurrentDetails.HousingFlags = personFlag.SelectMany(p => _context.Lookup.OrderByDescending(o => o.LookupOrder).
                ThenBy(t => t.LookupDescription).Where(w =>
                  w.LookupInactive == 0 && w.LookupFlag7 == 1 && w.LookupType == LookupConstants.PERSONCAUTION &&
                  w.LookupIndex == p.PersonFlagIndex && p.PersonFlagIndex > 0 ||
                  w.LookupType == LookupConstants.TRANSCAUTION && w.LookupIndex == p.InmateFlagIndex &&
                  p.InmateFlagIndex > 0 && w.LookupFlag7 == 1 && w.LookupInactive == 0), (p, l) => l.LookupDescription).ToList();

            return inmateCurrentDetails;
        }

        //To get facility housing details
        public HousingVm GetFacilityHousingDetails(int inmateId, int facilityId)
        {
            HousingVm housing = new HousingVm();

            //To get Housing Unit details
            _listHousingUnit = _context.HousingUnit
                .Where(w => w.FacilityId == facilityId && (w.HousingUnitInactive == 0 || !w.HousingUnitInactive.HasValue)).ToList();

            if (!_listHousingUnit.Any()) return housing;

            if (facilityId > 0) {
                housing.FacilityAbbr = _context.Facility.Find(facilityId).FacilityAbbr;
            }

            if (_context.SiteOptions.Any(w => w.SiteOptionsStatus == "1"
                && w.SiteOptionsName == SiteOptionsConstants.FORCETRANSFERDURINGFACILITYMOVE
                && w.SiteOptionsValue == SiteOptionsConstants.ON))
            {
                housing.ShowFacility = true;
            }

            _inmateList = _context.Inmate
                .Where(w => w.InmateActive == 1 && w.FacilityId == facilityId);

            //To get Housing stats count
            housing.HousingStatsDetails = LoadHousingStatsCount(_inmateList);

            //To get Inmate details
            housing.InmateDetailsList = GetInmateList(facilityId, false)
                .Where(w => w.HousingDetail != null).ToList();

            housing.InmateCurrentDetails = inmateId > 0 ? GetInmateCurrentDetails(inmateId) : new InmateCurrentDetails();

            //To get Housing Location details
            housing.HousingCapacityList = _listHousingUnit.OrderBy(o => o.HousingUnitLocation)
                .GroupBy(g => g.HousingUnitLocation)
                .Select(s => new HousingCapacityVm
                {
                    HousingLocation = s.Key,
                    HousingBedNumber = s.FirstOrDefault().HousingUnitBedNumber,
                    HousingBedLocation = s.FirstOrDefault().HousingUnitBedLocation,
                    CurrentCapacity = s.Sum(c => c.HousingUnitActualCapacity ?? 0),
                    OutofService = s.Sum(c => c.HousingUnitOutOfService ?? 0)
                }).ToList();

            //To get housing header details
            housing.HousingHeaderDetails = _listHousingUnit.Select(s => new HousingHeaderVm
            {
                Status = s.Facility?.DeleteFlag ?? 0,
                //Count = HousingConstants.Of,
                Floor = s.HousingUnitFloor,
                Offsite = s.HousingUnitOffsite,
                Medical = s.HousingUnitMedical,
                Mental = s.HousingUnitMental,
                Visitation = s.HousingUnitVisitAllow,
                Commission = s.HousingUnitCommAllow
            }).FirstOrDefault();

            housing.LocationList = _facilityPrivilegeService.GetTrackingLocationList(facilityId).ToList();

            HousingLockInputVm lockInputVm = new HousingLockInputVm
            {
                HousingType = HousingType.HousingLocation,
                FacilityId = facilityId
            };
            housing.HousingLockdownList = _lockdownService.GetHousingLockdownNotifications(lockInputVm).ToList();

            return housing;
        }

        //To get Housing stats count
        public HousingStatsVm LoadHousingStatsCount(IQueryable<Inmate> inmateList)
        {
            HousingStatsVm housingStatsVm = new HousingStatsVm();

            //To get list of Lookup details
            List<Lookup> lookupList = _context.Lookup
                .Where(l => (l.LookupType == LookupConstants.PERSONCAUTION
                             || l.LookupType == LookupConstants.TRANSCAUTION
                             || l.LookupType == LookupConstants.DIET
                             || l.LookupType == LookupConstants.SEX
                             || l.LookupType == LookupConstants.RACE) && l.LookupInactive == 0)
                             .OrderByDescending(o => o.LookupOrder).ThenBy(t => t.LookupDescription).ToList();

            //To get the Person Flag details
            IEnumerable<PersonFlag> lstPersonFlag = inmateList
                        .Where(w => w.Person.PersonFlag.Any(a => a.DeleteFlag == 0)).SelectMany(s => s.Person.PersonFlag);

            int inmateCount = inmateList.Count();

            housingStatsVm.FlagCountList = lstPersonFlag
               .SelectMany(perFlag => lookupList
                       .Where(w => ( w.LookupIndex == perFlag.PersonFlagIndex
                                   && w.LookupType == LookupConstants.PERSONCAUTION
                                   || w.LookupIndex == perFlag.InmateFlagIndex
                                   && w.LookupType == LookupConstants.TRANSCAUTION
                               || w.LookupIndex == perFlag.DietFlagIndex
                                   && w.LookupType == LookupConstants.DIET) && perFlag.DeleteFlag == 0),
                               (perFlag, look) => new { perFlag, look })
                               .OrderBy(o => o.look.LookupOrder).ThenBy(t => t.look.LookupDescription)
                               .GroupBy(g => new
                               {
                                   g.look.LookupIndex,
                                   g.look.LookupDescription
                               }).Select(x => new HousingStatsCount
                               {
                                   InmateCount = x.Count(),
                                   FlagName = x.Key.LookupDescription,
                                   FlagId = x.Key.LookupIndex,
                                   EventFlag = CellsEventFlag.Flag,
                                   Type = x.Select(se => se.look.LookupType).FirstOrDefault(),
                                   Percentage = inmateCount > 0 ? x.Count() * 100.0 / inmateCount : 0.0
                               }).ToList();

            //Gender list
            // TODO: QueryClientEvaluationWarning
            List<HousingStatsDetails> genderFlagList = inmateList.SelectMany(inm => lookupList.Where(w =>
                inm.Person.PersonSexLast.HasValue && 
                w.LookupIndex == inm.Person.PersonSexLast.Value && w.LookupType == LookupConstants.SEX),
                    (inm, l) => new HousingStatsDetails
                    {
                        FlagId = l.LookupIndex,
                        Flags = l.LookupDescription
                    }).ToList();

            genderFlagList.AddRange(inmateList
                .Where(w => !w.Person.PersonSexLast.HasValue
                            || w.Person.PersonSexLast == 0).Select(s =>
                    new HousingStatsDetails
                    {
                        FlagId = 0
                    }));

            housingStatsVm.GenderCountList = genderFlagList.OrderBy(o => o.Flags)
                .GroupBy(g => new
                {
                    g.Flags,
                    g.FlagId
                }).Select(x => new HousingStatsCount
                {
                    InmateCount = x.Count(),
                    FlagName = x.Key.Flags,
                    EventFlag = CellsEventFlag.Gender,
                    FlagId = x.Key.FlagId,
                    Percentage = inmateCount > 0 ? x.Count() * 100.0 / inmateCount : 0.0
                }).ToList();

            //Race list
            List<HousingStatsDetails> raceFlagList = inmateList
                .SelectMany(inm => lookupList
                        .Where(w => 
                        w.LookupIndex == inm.Person.PersonRaceLast
                                    && w.LookupType == LookupConstants.RACE),
                    (inm, l) => new HousingStatsDetails
                    {
                        FlagId = l.LookupIndex,
                        Flags = l.LookupDescription
                    }).ToList();

            raceFlagList.AddRange(inmateList
                .Where(w => !w.Person.PersonRaceLast.HasValue
                            || w.Person.PersonRaceLast == 0).Select(s =>
                    new HousingStatsDetails
                    {
                        FlagId = 0
                    }));

            housingStatsVm.RaceCountList = raceFlagList.GroupBy(g => new
            {
                g.Flags,
                g.FlagId
            }).Select(x => new HousingStatsCount
            {
                InmateCount = x.Count(),
                FlagName = x.Key.Flags,
                EventFlag = CellsEventFlag.Race,
                FlagId = x.Key.FlagId,
                Percentage = inmateCount > 0 ? x.Count() * 100.0 / inmateCount : 0.0
            }).OrderBy(o => o.FlagName).ToList();

            // For Association List
            IEnumerable<PersonClassification> lstPersonClassification = inmateList
                .Where(w => w.Person.PersonClassification
                .Any(p => p.PersonClassificationDateFrom <= DateTime.Now
                             && (!p.PersonClassificationDateThru.HasValue
                                 || p.PersonClassificationDateThru >= DateTime.Now) 
                            && p.PersonClassificationTypeId > 0))
                                 .SelectMany(s => s.Person.PersonClassification);

            List<Lookup> lookupsList = _context.Lookup.Where(w => w.LookupType == LookupConstants.CLASSGROUP).ToList();
            housingStatsVm.AssociationCountList = lstPersonClassification
                .GroupBy(g => g.PersonClassificationTypeId).Select(x => new HousingStatsCount
                {
                    InmateCount = x.Count(),
                    FlagId = x.Key ?? 0,
                    FlagName = lookupsList.Single(f => f.LookupIndex == x.Key).LookupDescription,
                    EventFlag = CellsEventFlag.Association,
                    Percentage = inmateCount > 0 ? x.Count() * 100.0 / inmateCount : 0.0
                }).OrderBy(o => o.FlagName).ToList();

            // For Classification List
            List<HousingStatsDetails> classifyFlagList = inmateList
                 .Where(w => w.InmateClassificationId.HasValue)
               .Select(ic => new HousingStatsDetails
               {
                   Flags = ic.InmateClassification.InmateClassificationReason
               }).ToList();

            classifyFlagList.AddRange(inmateList
                .Where(w => !w.InmateClassificationId.HasValue)
                .Select(inm => new HousingStatsDetails
                {
                    InmateId = inm.InmateId
                }));

            housingStatsVm.ClassifyCountList = classifyFlagList
                .GroupBy(g => g.Flags).Select(s =>
                    new HousingStatsCount
                    {
                        FlagName = s.Key,
                        InmateCount = s.Count(),
                        EventFlag = CellsEventFlag.Classification,
                        Percentage = inmateCount > 0 ? s.Count() * 100.0 / inmateCount : 0.0
                    }).OrderBy(o => o.FlagName).ToList();

            return housingStatsVm;
        }

        // To get housing details based on housing level
        public HousingVm GetHousingDetails(HousingInputVm value)
        {
            HousingVm housing = new HousingVm();
            List<InmateSearchVm> totalSepDetails = new List<InmateSearchVm>();
            //To get HousingUnit details
            IEnumerable<HousingUnit> listHousingUnit = _context.HousingUnit
                .Where(w => w.FacilityId == value.FacilityId && (w.HousingUnitInactive == 0 || !w.HousingUnitInactive.HasValue));

            //To get inmate details
            _inmateList = _context.Inmate.Where(w => w.InmateActive == 1 && w.FacilityId == value.FacilityId);
            // To get the person flag list
            IQueryable<PersonFlag> personFlag = _context.PersonFlag
                .Where(w => w.Person.Inmate.Any(s => s.InmateActive == 1) && w.DeleteFlag == 0
                && (w.InmateFlagIndex > 0 || w.PersonFlagIndex > 0));          

            switch (value.HousingType)
            {
                case HousingType.HousingLocation:

                    //Internal Location
                    List<HousingPrivilegesVm> internalLocationList = _inmateList
                        .Where(w => w.InmateCurrentTrackNavigation.FacilityId == value.FacilityId
                        && w.InmateCurrentTrackNavigation.HousingUnitLocation == value.HousingLocation)
                        .Select(s => new HousingPrivilegesVm
                        {
                            PrivilegesId = s.InmateCurrentTrackId ?? 0,
                            PrivilegesDescription = s.InmateCurrentTrack
                        }).ToList();

                    housing.InternalLocationList = internalLocationList.GroupBy(g => new
                    {
                        g.PrivilegesId,
                        g.PrivilegesDescription
                    }).Select(x => new HousingPrivilegesVm
                    {
                        PrivilegesId = x.Key.PrivilegesId,
                        PrivilegesDescription = x.Key.PrivilegesDescription,
                        Count = x.Count()
                    }).OrderBy(o => o.PrivilegesDescription).ToList();

                    _inmateList = _inmateList
                   .Where(w => w.HousingUnitId.HasValue
                               && w.HousingUnit.HousingUnitLocation == value.HousingLocation);

                    _listHousingUnit = listHousingUnit
                        .Where(w => w.HousingUnitLocation == value.HousingLocation).ToList();

                    housing.InmateDetailsList = GetInmateList(value.FacilityId, true).ToList();

                    //To get HousingUnt Location details
                    housing.HousingCapacityList = _listHousingUnit
                     .OrderBy(o => o.HousingUnitNumber)
                     .GroupBy(x => new { x.HousingUnitListId, x.HousingUnitNumber })
                     .Select(s => new HousingCapacityVm
                     {
                         FacilityId = value.FacilityId,
                         HousingLocation = value.HousingLocation,
                         HousingNumber = s.Key.HousingUnitNumber,
                         HousingUnitListId = s.Key.HousingUnitListId,
                         HousingBedNumber = s.FirstOrDefault().HousingUnitBedNumber,
                         HousingBedLocation = s.FirstOrDefault().HousingUnitBedLocation,
                         CurrentCapacity = s.Sum(c => c.HousingUnitActualCapacity ?? 0),
                         OutofService = s.Sum(c => c.HousingUnitOutOfService ?? 0),
                         HavingNextLevel = s.Any(a => a.HousingUnitLocation == value.HousingLocation
                                       && a.HousingUnitNumber == s.Key.HousingUnitNumber
                                       && a.HousingUnitBedNumber != null),
                         ClassifyConflictCheck = true,
                         HousingClassifyString = s.Where(w => w.HousingUnitClassifyRecString != null)
                             .Select(se => se.HousingUnitClassifyRecString.Split(','))
                             .SelectMany(se => se).Distinct().ToList(),
                         HousingFlagString = s.Where(se => !string.IsNullOrEmpty(se.HousingUnitFlagString))
                             .Select(se => se.HousingUnitFlagString).FirstOrDefault(),
                         HousingAttributeString = s.Where(se => se.HousingUnitAttributeString != "")
                            .Select(se => se.HousingUnitAttributeString).FirstOrDefault()
                     }).ToList();

                    housing.HousingFlag = GetHousingFlagList();
                    housing.HousingGenderDetails = LoadHousingGender(value.InmateId, value.FacilityId)
                        .Where(w => w.HousingDetail.HousingUnitLocation == value.HousingLocation).ToList();


                    // Total separation housing conflict
                    totalSepDetails.AddRange(GetTotalSeparation(personFlag.Where(
                        w => w.Person.Inmate.Any(a => a.FacilityId == value.FacilityId
                            && a.HousingUnit.HousingUnitLocation == value.HousingLocation)).ToList(), 1));
                    if (value.InmateId > 0)
                    {
                        // Total separation inmate conflict
                        totalSepDetails.AddRange(GetTotalSeparation(personFlag.Where(w => w.Person.Inmate.Any(a =>
                            a.InmateId == value.InmateId)).ToList(), 2)
                            .Where(w => w.HousingDetail.HousingUnitLocation == value.HousingLocation));
                    }

                    housing.HousingRuleAndClassifyFlags = 
                        _housingConflictService.GetHousingRuleAndClassifyFlags(value.InmateId, value.FacilityId)
                            .Where(w => w.Housing.HousingUnitLocation == value.HousingLocation).ToList();

                    HousingLockInputVm lockInputVm = new HousingLockInputVm
                    {
                        HousingType = HousingType.Number,
                        FacilityId = value.FacilityId,
                        HousingLocation = value.HousingLocation
                    };
                    housing.HousingLockdownList = _lockdownService.GetHousingLockdownNotifications(lockInputVm).ToList();

                    break;
                case HousingType.Number:
                    _inmateList = _inmateList
                    .Where(w => w.HousingUnitId.HasValue
                                && w.HousingUnit.HousingUnitListId == value.HousingUnitListId);

                    _listHousingUnit = listHousingUnit.Where(w => w.HousingUnitListId == value.HousingUnitListId).ToList();

                    housing.InmateDetailsList = GetInmateList(value.FacilityId, true).ToList();

                    //To get HousingUnt Location details
                    housing.HousingCapacityList = _listHousingUnit
                       .Where(w => w.HousingUnitBedNumber != null)
                       .OrderBy(o => o.HousingUnitBedNumber)
                       .GroupBy(g => new { g.HousingUnitNumber, g.HousingUnitBedNumber, g.HousingUnitListBedGroupId })
                       .Select(s => new HousingCapacityVm
                       {
                           FacilityId = value.FacilityId,
                           HousingLocation = value.HousingLocation,
                           HousingNumber = s.Key.HousingUnitNumber,
                           HousingBedNumber = s.Key.HousingUnitBedNumber,
                           HousingUnitBedGroupId = s.Key.HousingUnitListBedGroupId ?? 0,
                           HousingUnitListId = value.HousingUnitListId,
                           CurrentCapacity = s.Sum(c => c.HousingUnitActualCapacity ?? 0),
                           OutofService = s.Sum(c => c.HousingUnitOutOfService ?? 0),
                           HavingNextLevel = s.Any(a => a.HousingUnitListId == value.HousingUnitListId
                                             && a.HousingUnitBedNumber == s.Key.HousingUnitBedNumber
                                             && a.HousingUnitBedLocation != null),
                           ClassifyConflictCheck = true,
                           HousingClassifyString = s.Where(w => w.HousingUnitClassifyRecString != null)
                               .Select(se => se.HousingUnitClassifyRecString.Split(','))
                               .SelectMany(se => se).Distinct().ToList(),
                           HousingFlagString = s.Where(se => !string.IsNullOrEmpty(se.HousingUnitFlagString))
                               .Select(se => se.HousingUnitFlagString).FirstOrDefault(),
                           HousingAttributeString = s.Where(se => se.HousingUnitAttributeString != "")
                            .Select(se => se.HousingUnitAttributeString).FirstOrDefault()
                       }).ToList();

                    if (_listHousingUnit.All(a => a.HousingUnitBedNumber == null))
                    {
                        housing.HousingAttribute = GetHousingAttribute();
                    }
                    housing.HousingFlag = GetHousingFlagList();
                    housing.HousingGenderDetails = LoadHousingGender(value.InmateId, value.FacilityId)
                        .Where(w => w.HousingDetail.HousingUnitListId == value.HousingUnitListId).ToList();
                    housing.HousingRuleAndClassifyFlags = 
                        _housingConflictService.GetHousingRuleAndClassifyFlags(value.InmateId, value.FacilityId)
                            .Where(w => w.Housing.HousingUnitListId == value.HousingUnitListId).ToList();
                    if (value.InmateId > 0 && _listHousingUnit.All(a => a.HousingUnitBedNumber == null))
                    {
                        //To get Keep Separate details
                        housing.HousingConflictList = _housingConflictService.GetInmateHousingConflictVm(value);
                        //housing.HousingAttribute = GetHousingAttribute();
                    }
                    else if (_listHousingUnit.All(a => a.HousingUnitBedNumber == null))
                    {
                        //To get Keep Separate details
                        housing.HousingConflictList = _housingConflictService.GetHousingConflictVm(value);
                    }

                    // Total separation housing conflict
                    totalSepDetails.AddRange(GetTotalSeparation(personFlag.Where(w => w.Person.Inmate.Any(
                        a => a.FacilityId == value.FacilityId
                            && a.HousingUnit.HousingUnitListId == value.HousingUnitListId)).ToList(), 1));
                    if (value.InmateId > 0)
                    {
                        // Total separation inmate conflict
                        totalSepDetails.AddRange(GetTotalSeparation(personFlag.Where(w => w.Person.Inmate.Any(
                            a => a.InmateId == value.InmateId)).ToList(), 2).Where(w =>
                            w.HousingDetail.HousingUnitListId == value.HousingUnitListId));
                    }

                    //To get HousingUnt Location details
                    housing.BunkCapacityList = _listHousingUnit.Where(w => w.HousingUnitBedLocation != null)
                        .OrderBy(o => o.HousingUnitBedLocation).ThenBy(t => t.HousingUnitBedNumber)
                        .GroupBy(g => new { g.HousingUnitNumber, g.HousingUnitBedNumber, g.HousingUnitBedLocation })
                        .Select(s => new HousingCapacityVm
                        {
                            FacilityId = value.FacilityId,
                            HousingLocation = value.HousingLocation,
                            HousingNumber = s.Key.HousingUnitNumber,
                            HousingUnitListId = value.HousingUnitListId,
                            HousingBedNumber = s.Key.HousingUnitBedNumber,
                            HousingBedLocation = s.Key.HousingUnitBedLocation,
                            CurrentCapacity = s.Sum(c => c.HousingUnitActualCapacity ?? 0),
                            OutofService = s.Sum(c => c.HousingUnitOutOfService ?? 0),
                            ClassifyConflictCheck = true,
                            HousingClassifyString = s.Where(w => w.HousingUnitClassifyRecString != null)
                                .Select(se => se.HousingUnitClassifyRecString.Split(','))
                                .SelectMany(se => se).Distinct().ToList(),
                            HousingFlagString = s.Where(se => !string.IsNullOrEmpty(se.HousingUnitFlagString))
                                .Select(se => se.HousingUnitFlagString).FirstOrDefault(),
                        }).ToList();

                    HousingLockInputVm podLockInputVm = new HousingLockInputVm
                    {
                        HousingType = HousingType.BedNumber,
                        FacilityId = value.FacilityId,
                        HousingLocation = value.HousingLocation,
                        HousingUnitListId = value.HousingUnitListId
                    };
                    housing.HousingLockdownList = _lockdownService.GetHousingLockdownNotifications(podLockInputVm).ToList();

                    break;
                case HousingType.BedNumber:

                    //To get inmate details
                    _inmateList = _inmateList
                        .Where(w => w.HousingUnitId.HasValue
                                    && w.HousingUnit.HousingUnitBedNumber == value.HousingBedNumber
                                    && w.HousingUnit.HousingUnitListId == value.HousingUnitListId);

                    //To get HousingUnit details
                    _listHousingUnit = listHousingUnit
                        .Where(w => w.HousingUnitBedNumber == value.HousingBedNumber
                                    && w.HousingUnitListId == value.HousingUnitListId).ToList();

                    housing.InmateDetailsList = GetInmateList(value.FacilityId, true).ToList();

                    if (value.InmateId > 0 && _listHousingUnit.All(a => a.HousingUnitBedLocation == null))
                    {
                        //To get Keep Separate details
                        housing.HousingConflictList = _housingConflictService.GetInmateHousingConflictVm(value);
                        housing.HousingAttribute = GetHousingAttribute();
                    }
                    else if (_listHousingUnit.All(a => a.HousingUnitBedLocation == null))
                    {
                        housing.HousingAttribute = GetHousingAttribute();
                        //To get Keep Separate details
                        housing.HousingConflictList = _housingConflictService.GetHousingConflictVm(value);
                    }

                    housing.HousingRuleAndClassifyFlags = 
                        _housingConflictService.GetHousingRuleAndClassifyFlags(value.InmateId, value.FacilityId)
                            .Where(w => w.Housing.HousingUnitBedNumber == value.HousingBedNumber
                                && w.Housing.HousingUnitListId == value.HousingUnitListId).ToList();
                    break;
                case HousingType.BedLocation:

                    //To get inmate details
                    _inmateList = _inmateList
                        .Where(w => w.HousingUnitId.HasValue
                                    && w.HousingUnit.HousingUnitBedNumber == value.HousingBedNumber
                                    && w.HousingUnit.HousingUnitBedLocation == value.HousingBedLocation
                                    && w.HousingUnit.HousingUnitListId == value.HousingUnitListId);

                    //To get HousingUnit details
                    _listHousingUnit = listHousingUnit
                        .Where(w => w.HousingUnitBedNumber == value.HousingBedNumber
                                    && w.HousingUnitBedLocation == value.HousingBedLocation
                                    && w.HousingUnitListId == value.HousingUnitListId).ToList();

                    housing.InmateDetailsList = GetInmateList(value.FacilityId, true).ToList();

                    housing.HousingConflictList = value.InmateId > 0 ? _housingConflictService.GetInmateHousingConflictVm(value)
                        : _housingConflictService.GetHousingConflictVm(value);
                    housing.HousingAttribute = GetHousingAttribute();
                    break;
            }
            housing.TotalSepDetails = totalSepDetails;
            if (value.InmateId > 0)
            {
                housing.InmateCurrentDetails = GetInmateCurrentDetails(value.InmateId);
            }

            if (_context.SiteOptions.Any(w => w.SiteOptionsStatus == "1"
                                              && w.SiteOptionsName == SiteOptionsConstants.FORCETRANSFERDURINGFACILITYMOVE
                                              && w.SiteOptionsValue == SiteOptionsConstants.ON))
            {
                housing.ShowFacility = true;
            }

            //To get Housing Stats details
            housing.HousingStatsDetails = LoadHousingStatsCount(_inmateList);
           
            housing.LocationList = _facilityPrivilegeService.GetTrackingLocationList(value.FacilityId).ToList();

            return housing;
        }

        public HousingVm GetHousingData(int housingUnitListId)
        {
            HousingVm housing = new HousingVm
            {
                HousingHeaderDetails = _context.HousingUnit
                    .Where(w => w.HousingUnitListId == housingUnitListId)
                    .Select(s => new HousingHeaderVm
                    {
                        Status = s.Facility.DeleteFlag,
                        Gender = _context.Lookup
                            .Where(w => w.LookupIndex == s.HousingUnitSex
                                        && w.LookupType == LookupConstants.SEX && w.LookupInactive == 0)
                            .Select(se => se.LookupDescription).FirstOrDefault(),
                        Floor = s.HousingUnitFloor,
                        Offsite = s.HousingUnitOffsite,
                        Medical = s.HousingUnitMedical,
                        Mental = s.HousingUnitMental,
                        Visitation = s.HousingUnitVisitAllow,
                        Commission = s.HousingUnitCommAllow
                    }).FirstOrDefault()
            };


            if (housing.HousingHeaderDetails?.Visitation != null)
            {
                housing.HousingVisitationDetails = _context.HousingUnitVisitation
                .Where(w => !w.DeleteFlag.HasValue && w.HousingUnitListId == housingUnitListId)
               .Select(s => new HousingVisitationVm
               {
                   VisitDay = s.VisitationDay,
                   VisitFrom = s.VisitationFrom,
                   VisitTo = s.VisitationTo
               }).ToList();
            }

            return housing;
        }

        private List<HousingFlags> GetHousingFlagList() => _listHousingUnit

            .Select(s => new HousingFlags
            {
                HousingDetail = new HousingDetail
                {
                    HousingUnitId = s.HousingUnitId,
                    HousingUnitListId = s.HousingUnitListId,
                    HousingUnitLocation = s.HousingUnitLocation,
                    HousingUnitNumber = s.HousingUnitNumber,
                    HousingUnitBedNumber = s.HousingUnitBedNumber,
                    HousingUnitBedLocation = s.HousingUnitBedLocation
                },
                Flags = s.HousingUnitFlagString != "" ? s.HousingUnitFlagString?.Split(',').ToList() : null
            }).ToList();

        //To get Housing Attribute details
        private HousingAttributeVm GetHousingAttribute() => _listHousingUnit
            .Select(s => new HousingAttributeVm
            {
                AttributeString = s.HousingUnitAttributeString,
                DisplayNote = s.HousingUnitDisplayNote
            }).FirstOrDefault();

        //To get inmate list
        private IQueryable<InmateSearchVm> GetInmateList(int facilityId, bool sentencedFlag) => _inmateList
                  .Where(w => w.HousingUnitId.HasValue && w.FacilityId == facilityId)
                  .Select(s => new InmateSearchVm
                  {
                      InmateId = s.InmateId,
                      InmateNumber = s.InmateNumber,
                      FacilityId = facilityId,
                      FacilityAbbr = s.Facility.FacilityAbbr,
                      LocationId = s.InmateCurrentTrackId,
                      Location = s.InmateCurrentTrack,
                      HousingDetail = new HousingDetail
                      {
                          HousingUnitId = s.HousingUnit.HousingUnitId,
                          HousingUnitListId = s.HousingUnit.HousingUnitListId,
                          HousingUnitLocation = s.HousingUnit.HousingUnitLocation,
                          HousingUnitNumber = s.HousingUnit.HousingUnitNumber,
                          HousingUnitBedNumber = s.HousingUnit.HousingUnitBedNumber,
                          HousingUnitBedLocation = s.HousingUnit.HousingUnitBedLocation
                      },
                      PersonDetail = new PersonInfoVm
                      {
                          PersonId = s.PersonId,
                          PersonLastName = s.Person.PersonLastName,
                          PersonMiddleName = s.Person.PersonMiddleName,
                          PersonFirstName = s.Person.PersonFirstName
                      },
                      PhotoFilePath = _photos.GetPhotoByCollectionIdentifier(s.Person.Identifiers),
                      Classify = s.InmateClassification.InmateClassificationReason,
                      Sentenced = sentencedFlag ? s.Incarceration.OrderByDescending(o => o.IncarcerationId)
                       .Where(w => w.InmateId == s.InmateId)
                       .Select(se => se.OverallFinalReleaseDate).FirstOrDefault() : null
                  }).OrderBy(o => o.PersonDetail.PersonLastName)
                   .ThenBy(t => t.PersonDetail.PersonLastName)
                   .ThenBy(t => t.PersonDetail.PersonMiddleName);
        private List<InmateSearchVm> LoadHousingGender(int inmateId, int facilityId)
        {
            //To get Opposite gender for GenderSeparation
            int gender = _context.Inmate.Where(w => w.InmateId == inmateId)
                .Select(s => s.Person.PersonSexLast ?? 0).SingleOrDefault();

            List<InmateSearchVm> lstHousingGenderDetails = new List<InmateSearchVm>();

            lstHousingGenderDetails.AddRange(_listHousingUnit.Where(w => !w.Inmate.Any()
            && w.HousingUnitSex != gender && w.HousingUnitSex > 0 && gender > 0
            ).Select(
                s => new InmateSearchVm
                {
                    FacilityId = s.FacilityId,
                    HousingDetail = new HousingDetail
                    {
                        HousingUnitId = s.HousingUnitId,
                        HousingUnitListId = s.HousingUnitListId,
                        HousingUnitLocation = s.HousingUnitLocation,
                        HousingUnitNumber = s.HousingUnitNumber,
                        HousingUnitBedNumber = s.HousingUnitBedNumber,
                        HousingUnitBedLocation = s.HousingUnitBedLocation
                    }
                }));

            IQueryable<Inmate> lstInmates = _context.Inmate
                .Where(il => il.InmateActive == 1 && il.FacilityId == facilityId);

            //To get Gender conflict count
            lstHousingGenderDetails.AddRange(lstInmates
                .Where(w => w.HousingUnitId.HasValue && (w.HousingUnit.HousingUnitInactive == 0 || !w.HousingUnit.HousingUnitInactive.HasValue)                   
                    && (gender != w.Person.PersonSexLast && w.Person.PersonSexLast.HasValue
                                                         && w.Person.PersonSexLast > 0 && gender > 0
                    || w.HousingUnit.HousingUnitSex != gender
                    && w.HousingUnit.HousingUnitSex > 0 && gender > 0))
                .Select(s => new InmateSearchVm
                {
                    InmateId = s.InmateId,
                    InmateNumber = s.InmateNumber,
                    LocationId = s.InmateCurrentTrackId,
                    FacilityId = facilityId,
                    HousingDetail = new HousingDetail
                    {
                        HousingUnitId = s.HousingUnit.HousingUnitId,
                        HousingUnitListId = s.HousingUnit.HousingUnitListId,
                        HousingUnitLocation = s.HousingUnit.HousingUnitLocation,
                        HousingUnitNumber = s.HousingUnit.HousingUnitNumber,
                        HousingUnitBedNumber = s.HousingUnit.HousingUnitBedNumber,
                        HousingUnitBedLocation = s.HousingUnit.HousingUnitBedLocation
                    },
                    PersonDetail = new PersonInfoVm
                    {
                        PersonFirstName = s.Person.PersonFirstName,
                        PersonLastName = s.Person.PersonLastName,
                        PersonMiddleName = s.Person.PersonMiddleName,
                        PersonId = s.PersonId
                    }
                }));

            return lstHousingGenderDetails;
        }

        //To get details inmate Recommend Housing
        public HousingVm GetRecommendHousing(int facilityId, int inmateId)
        {
            HousingVm housing = new HousingVm();

            if (inmateId == 0)
                return housing;

            int gender = _context.Inmate
                .Where(w => w.InmateId == inmateId)
                .Select(s => s.Person.PersonSexLast ?? 0).SingleOrDefault();

            _inmateList = _context.Inmate
                .Where(w => w.InmateActive == 1
                            && w.FacilityId == facilityId && w.HousingUnitId.HasValue);

            InmateHousingVm inmateHousingVm = new InmateHousingVm
            {
                InmateCurrentDetails = GetInmateCurrentDetails(inmateId)
            };

            string facilityAbbr = _context.Facility.Single(s => s.FacilityId == facilityId).FacilityAbbr;
            string classify = inmateHousingVm.InmateCurrentDetails.InmateClassification;

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

                _inmateList = _inmateList.Where(i => 
                    classifies.All(a => i.InmateClassification.InmateClassificationReason != a));
            }

            //To get list of Lookup details
            List<Lookup> lookupList = _context.Lookup
                .Where(l => (l.LookupType == LookupConstants.PERSONCAUTION
                             || l.LookupType == LookupConstants.TRANSCAUTION) && l.LookupInactive == 0 && l.LookupFlag6 == 1)
                .OrderByDescending(o => o.LookupOrder).ThenBy(t => t.LookupDescription).ToList();


            List<InmateSearchVm> lstPersonFlag = _inmateList.SelectMany(s => _context.PersonFlag
            .Where(w => w.DeleteFlag == 0 && (w.InmateFlagIndex > 0 || w.PersonFlagIndex > 0)
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

            List<InmateSearchVm> totalSepDetails = lstPersonFlag
                .SelectMany(perFlag => lookupList.Where(w => 
                    w.LookupIndex == perFlag.PersonFlagIndex && w.LookupType == LookupConstants.PERSONCAUTION
                    || w.LookupIndex == perFlag.InmateFlagIndex
                    && w.LookupType == LookupConstants.TRANSCAUTION),
                    (perFlag, look) => new InmateSearchVm
                    {
                        FacilityId = perFlag.FacilityId,
                        PersonId = perFlag.PersonId,
                        HousingDetail = perFlag.HousingDetail
                    }).ToList();


            int inmateSepFlag = _context.PersonFlag.Where(w => w.Person.Inmate.Any(s => 
                w.Person.Inmate.Any(a => a.InmateId == inmateId) && s.InmateActive == 1)
                && w.DeleteFlag == 0 && (w.InmateFlagIndex > 0 || w.PersonFlagIndex > 0)).SelectMany(
                p => _context.Lookup.Where(w => w.LookupInactive == 0 && w.LookupFlag6 == 1 && 
                w.LookupType == LookupConstants.PERSONCAUTION && w.LookupIndex == p.PersonFlagIndex && 
                p.PersonFlagIndex > 0 || w.LookupType == LookupConstants.TRANSCAUTION
                && w.LookupIndex == p.InmateFlagIndex && p.InmateFlagIndex > 0
                && w.LookupFlag6 == 1), (p, l) => p.PersonId).Count();

            //To Get keep separate details
            housing.HousingConflictList = _housingConflictService.GetRecommendHousingConflictVm(inmateId, facilityId);


            _listHousingUnit = _context.HousingUnit.Where(w => w.FacilityId == facilityId
                && w.Facility.DeleteFlag == 0 && (w.HousingUnitInactive == 0 || !w.HousingUnitInactive.HasValue) 
                //&& w.HousingUnitBedNumber != null
                && (gender == 0 || !w.HousingUnitSex.HasValue || (w.HousingUnitSex ?? 0) == gender)
                 && (w.HousingUnitClassifyRecString == null ||
                     inmateHousingVm.InmateCurrentDetails.InmateClassification == null ||
                     inmateHousingVm.InmateCurrentDetails.InmateClassification != RequestType.PENDINGREQUEST
                     && w.HousingUnitClassifyRecString.Contains(classify))
                ).ToList();


            if (totalSepDetails.Any())
            {
                _listHousingUnit = _listHousingUnit
                    .Where(w => totalSepDetails.All(a => a.HousingDetail.HousingUnitId != w.HousingUnitId)).ToList();
            }

            //// Inmate housing rule flag 
            List<InmateHousingRuleFlag> lstInmateHousingRuleFlags = _context.InmateHousingRuleFlag
                .Where(i => !i.DeleteFlag).ToList();

            List<PersonFlag> lstPersonFlags = _context.PersonFlag
                .Where(p => p.PersonId == inmateHousingVm.InmateCurrentDetails.Person.PersonId
                    && (p.InmateFlagIndex > 0 || p.PersonFlagIndex > 0 || p.DietFlagIndex > 0)).ToList();

            List<int?> personFlagIndex = lstInmateHousingRuleFlags.Where(h => 
                lstPersonFlags.Select(d => d.PersonFlagIndex).Contains(h.PersonFlagIndex1))
                .Select(a => a.PersonFlagIndex2).ToList();            
            personFlagIndex.AddRange(lstInmateHousingRuleFlags.Where(h => 
                lstPersonFlags.Select(d => d.PersonFlagIndex).Contains(h.PersonFlagIndex2))
                .Select(a => a.PersonFlagIndex1).ToList());
            personFlagIndex = personFlagIndex.Distinct().ToList();

            List<int?> inmateFlagIndex = lstInmateHousingRuleFlags.Where(h => 
                lstPersonFlags.Select(d => d.InmateFlagIndex).Contains(h.InmateFlagIndex1))
                .Select(a => a.InmateFlagIndex2).ToList();            
            inmateFlagIndex.AddRange(lstInmateHousingRuleFlags.Where(h => 
                lstPersonFlags.Select(d => d.InmateFlagIndex).Contains(h.InmateFlagIndex2))
                .Select(a => a.InmateFlagIndex1).ToList());
            inmateFlagIndex = inmateFlagIndex.Distinct().ToList();

            List<int?> dietFlagIndex = lstInmateHousingRuleFlags.Where(h => 
                lstPersonFlags.Select(d => d.DietFlagIndex).Contains(h.DietFlagIndex1))
                .Select(a => a.DietFlagIndex2).ToList();            
            dietFlagIndex.AddRange(lstInmateHousingRuleFlags.Where(h => 
                lstPersonFlags.Select(d => d.DietFlagIndex).Contains(h.DietFlagIndex2))
                .Select(a => a.DietFlagIndex1).ToList());
            dietFlagIndex = dietFlagIndex.Distinct().ToList();

            if(personFlagIndex.Any() || inmateFlagIndex.Any() || dietFlagIndex.Any())
            {
                //To get list of Lookup details
                List<Lookup> lookupFlagList = _context.Lookup
                    .Where(l => (l.LookupType == LookupConstants.PERSONCAUTION
                                 || l.LookupType == LookupConstants.TRANSCAUTION
                                 || l.LookupType == LookupConstants.DIET) 
                                 && l.LookupInactive == 0)
                    .OrderByDescending(o => o.LookupOrder).ThenBy(t => t.LookupDescription).ToList();

                List<InmateSearchVm> lstPersonRuleFlag = _inmateList.SelectMany(s => _context.PersonFlag
                .Where(w => w.DeleteFlag == 0 && (w.InmateFlagIndex > 0 || w.PersonFlagIndex > 0 || w.DietFlagIndex > 0)
                    && w.PersonId == s.PersonId), (s, p) => new InmateSearchVm
                    {
                        PersonFlagIndex = p.PersonFlagIndex,
                        InmateFlagIndex = p.InmateFlagIndex,
                        DietFlagIndex = p.DietFlagIndex,
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

                lstPersonRuleFlag = lstPersonRuleFlag.Where(p => personFlagIndex.Contains(p.PersonFlagIndex)
                    || inmateFlagIndex.Contains(p.InmateFlagIndex) || dietFlagIndex.Contains(p.DietFlagIndex)).ToList();

                List<InmateSearchVm> lstRuleDetails = lstPersonRuleFlag
                    .SelectMany(perFlag => lookupFlagList.Where(w =>
                        w.LookupIndex == perFlag.PersonFlagIndex && w.LookupType == LookupConstants.PERSONCAUTION
                        || w.LookupIndex == perFlag.InmateFlagIndex
                        && w.LookupType == LookupConstants.TRANSCAUTION
                        || w.LookupIndex == perFlag.DietFlagIndex
                        && w.LookupType == LookupConstants.DIET),
                        (perFlag, look) => new InmateSearchVm
                        {
                            FacilityId = perFlag.FacilityId,
                            PersonId = perFlag.PersonId,
                            HousingDetail = perFlag.HousingDetail
                        }).ToList();

                if (lstRuleDetails.Any())
                {
                    _listHousingUnit = _listHousingUnit
                        .Where(w => lstRuleDetails.Any(a => a.HousingDetail.HousingUnitId != w.HousingUnitId)).ToList();
                }
            }

            if (housing.HousingConflictList.Any())
            {
                _listHousingUnit = _listHousingUnit.Where(w => !housing.HousingConflictList.Any(a =>
                    a.Housing.HousingUnitListId == w.HousingUnitListId
                    && (a.Housing.HousingUnitBedNumber == null && w.HousingUnitBedNumber == null
                         || a.Housing.HousingUnitBedNumber == w.HousingUnitBedNumber)
                    && (a.Housing.HousingUnitBedLocation == null && w.HousingUnitBedLocation == null
                        || a.Housing.HousingUnitBedLocation == w.HousingUnitBedLocation))).ToList();
            }

            //To get Inmate details
            housing.InmateDetailsList = GetInmateList(facilityId, false)
                .Where(w => w.HousingDetail != null).ToList();


            //To get HousingUnt Location details
            housing.HousingCapacityList = _listHousingUnit.OrderBy(o => o.HousingUnitListId)
                .GroupBy(s => new
                {
                    s.HousingUnitListId,
                    s.HousingUnitLocation,
                    s.HousingUnitNumber,
                    s.HousingUnitBedNumber,
                    s.HousingUnitBedLocation,
                    s.FacilityId,
                    s.HousingUnitActualCapacity,
                    s.HousingUnitOutOfService
                })
                .Select(s => new HousingCapacityVm
                {
                    HousingUnitListId = s.Key.HousingUnitListId,
                    HousingLocation = s.Key.HousingUnitLocation,
                    HousingNumber = s.Key.HousingUnitNumber,
                    HousingBedNumber = s.Key.HousingUnitBedNumber,
                    HousingBedLocation = s.Key.HousingUnitBedLocation,
                    FacilityAbbr = facilityAbbr,
                    FacilityId = s.Key.FacilityId,
                    CurrentCapacity = (s.Key.HousingUnitActualCapacity ?? 0) - (s.Key.HousingUnitOutOfService ?? 0),
                    Assigned = housing.InmateDetailsList.Count(c =>
                         s.Key.HousingUnitLocation == c.HousingDetail.HousingUnitLocation
                        && s.Key.HousingUnitNumber == c.HousingDetail.HousingUnitNumber
                        && s.Key.HousingUnitBedNumber == c.HousingDetail.HousingUnitBedNumber
                        && s.Key.HousingUnitBedLocation == c.HousingDetail.HousingUnitBedLocation),
                    OutOfServiceReason = s.Select(se => se.HousingUnitOutOfServiceReason).FirstOrDefault(),
                    OutofService = s.Select(se => se.HousingUnitOutOfService ?? 0).FirstOrDefault(),
                    HavingNextLevel = _listHousingUnit.Any(a => a.HousingUnitListId == s.Key.HousingUnitListId
                                       && a.HousingUnitBedNumber == s.Key.HousingUnitBedNumber
                                       && a.HousingUnitBedLocation != null),
                    HousingClassifyString = s.Where(w => !string.IsNullOrEmpty(w.HousingUnitFlagString))
                        .SelectMany(se => se.HousingUnitFlagString.Split(',')).ToList(),
                    Actual = housing.InmateDetailsList.Count(c =>
                           s.Key.HousingUnitLocation == c.HousingDetail.HousingUnitLocation
                          && s.Key.HousingUnitNumber == c.HousingDetail.HousingUnitNumber
                          && s.Key.HousingUnitBedNumber == c.HousingDetail.HousingUnitBedNumber)
                }).OrderBy(o => o.HousingLocation)
                .ThenBy(t => t.HousingNumber)
                .ThenBy(t => t.HousingBedNumber).ToList();

            //V2-3727 JMS_Recommend housing_Not displaying entire list
            housing.HousingCapacityList = housing.HousingCapacityList
               .Where(w => w.Assigned < w.CurrentCapacity
               && w.OutofService == 0
               && (!inmateHousingVm.InmateCurrentDetails.HousingFlags.Any()
                   || w.HousingClassifyString.Any() && inmateHousingVm.InmateCurrentDetails.HousingFlags.Any()
                    && inmateHousingVm.InmateCurrentDetails.HousingFlags.All(o => w.HousingClassifyString.Any(a => a == o)))
               && (inmateSepFlag == 0 || inmateSepFlag > 0 && w.Actual == 0)
                ).ToList();

            return housing;
        }

        public async Task<int> InsertHousingAssign(HousingAssignVm value)
        {
            Inmate inmate = _context.Inmate.Single(s => s.InmateId == value.InmateId);

            InmateTrak inmateTrack = _context.InmateTrak
                .FirstOrDefault(w => w.InmateId == value.InmateId && !w.InmateTrakDateIn.HasValue);

            value.HousingUnitId = !string.IsNullOrEmpty(value.HousingDetail.HousingUnitBedLocation)
                ? _context.HousingUnit
                    .Where(w => w.HousingUnitListId == value.HousingDetail.HousingUnitListId
                                && w.HousingUnitBedNumber == value.HousingDetail.HousingUnitBedNumber
                                && w.HousingUnitBedLocation == value.HousingDetail.HousingUnitBedLocation)
                    .Select(s => s.HousingUnitId).FirstOrDefault()
                : !string.IsNullOrEmpty(value.HousingDetail.HousingUnitBedNumber)
                  && string.IsNullOrEmpty(value.HousingDetail.HousingUnitBedLocation)
                    ? _context.HousingUnit
                        .Where(w => w.HousingUnitListId == value.HousingDetail.HousingUnitListId
                                    && w.HousingUnitBedNumber == value.HousingDetail.HousingUnitBedNumber)
                        .Select(s => s.HousingUnitId).FirstOrDefault()
                    : _context.HousingUnit
                        .Where(w => w.HousingUnitListId == value.HousingDetail.HousingUnitListId)
                        .Select(s => s.HousingUnitId).FirstOrDefault();

            //Un assign housing 
            if (value.UnAssign)
            {
                inmate.HousingUnitId = null;

                HousingUnitMoveHistory updateHousingUnitMoveHistory = _context.HousingUnitMoveHistory
                    .SingleOrDefault(w => w.InmateId == value.InmateId && !w.MoveDateThru.HasValue);

                HousingUnitMoveHistory housingUnitMoveHistory = new HousingUnitMoveHistory
                {
                    InmateClassificationId = inmate.InmateClassificationId,
                    InmateId = value.InmateId,
                    HousingUnitFromId = inmate.HousingUnitId,
                    HousingUnitToId = value.HousingUnitId > 0 ? value.HousingUnitId : default(int?),
                    MoveOfficerId = _personnelId,
                    MoveReason = value.Reason,
                    MoveDate = DateTime.Now  //mm/dd/yyyy hh.mm.ss
                };
                _context.Add(housingUnitMoveHistory);

                if (!(updateHousingUnitMoveHistory is null))
                {
                    updateHousingUnitMoveHistory.MoveDateThru = DateTime.Now;
                    updateHousingUnitMoveHistory.MoveThruBy = _personnelId;
                }

                if (!(inmateTrack is null))
                {
                    inmateTrack.InmateTrakDateIn = DateTime.Now;
                    inmateTrack.InPersonnelId = _personnelId;
                }

                InmateTrak insertInmateTrack = new InmateTrak
                {
                    InmateId = value.InmateId,
                    InmateTrakLocation = value.PrivilegeDetails.PrivilegeDescription,
                    InmateTrakLocationId = value.PrivilegeDetails.PrivilegeId,
                    InmateTrakDateOut = DateTime.Now,
                    OutPersonnelId = _personnelId,
                    FacilityId = value.FacilityId
                };
                _context.Add(insertInmateTrack);

                Privileges privileges = _context.Privileges
                        .Single(s => s.PrivilegeId == value.PrivilegeDetails.PrivilegeId);

                int isExternalFacility = privileges.FacilityId ?? 0;

                int inmateLocationCnt = isExternalFacility > 0
                    ? _context.Inmate
                        .Count(c => c.InmateCurrentTrackId == value.PrivilegeDetails.PrivilegeId
                                    && c.InmateActive == 1 && c.FacilityId == value.FacilityId)
                    : _context.Inmate
                        .Count(c => c.InmateCurrentTrackId == value.PrivilegeDetails.PrivilegeId &&
                                    c.InmateActive == 1);

                if (inmateLocationCnt == 0)
                {
                    privileges.SafetyCheckLastEntry = DateTime.Now;
                    privileges.SafetyCheckLastEntryBy = _personnelId;
                }

                inmate.InmateCurrentTrack = value.PrivilegeDetails.PrivilegeDescription;
                inmate.InmateCurrentTrackId = value.PrivilegeDetails.PrivilegeId;

                await _context.SaveChangesAsync();
                _interfaceEngineService.Export(new ExportRequestVm
                {
                    EventName = EventNameConstants.HOUSINGMOVE,
                    PersonnelId = _personnelId,
                    Param1 = inmate.PersonId.ToString(),
                    Param2 = housingUnitMoveHistory.HousingUnitMoveHistoryId.ToString()
                });

                //Event Handle
                if (!(inmateTrack is null))
                {
                    //// INSERT  EVENT HANDLE
                    _interfaceEngineService.Export(new ExportRequestVm
                    {
                        EventName = EventNameConstants.LOCATIONTRACKING,
                        PersonnelId = _personnelId,
                        Param1 = inmate.PersonId.ToString(),
                        Param2 = inmateTrack.InmateTrakId.ToString()
                    });
                }
            }
            else
            {
                //Assign housing 
                value.InmateClassificationId = inmate.InmateClassificationId;
                value.PersonId = inmate.PersonId;
                value.HousingUnitFromId = inmate.HousingUnitId;

                if (value.FacilityId != inmate.FacilityId)
                {
                    //Inactivate Program Schedule
                    AppointmentProgramAssign appointmentProgramAssign = _context.AppointmentProgramAssign
                        .SingleOrDefault(s => s.InmateId == value.InmateId);

                    if (!(appointmentProgramAssign is null))
                    {
                        appointmentProgramAssign.DeleteFlag = 1;
                        appointmentProgramAssign.DeleteDate = DateTime.Now;
                        appointmentProgramAssign.DeleteBy = _personnelId;
                    }

                    // This program table need to change based on programClass
                    //Delete Program Requests in the old facility
                    List<ProgramRequest> programRequestList = _context.ProgramRequest
                        .Where(w => w.InmateId == value.InmateId
                      //&& w.Program.DeleteFlag == 0
                      //&& w.Program.ProgramCategory.FacilityId == value.FacilityId
                      //&& w.Program.ProgramCategory.DeleteFlag == 0
                      ).ToList();

                    programRequestList.ForEach(item =>
                    {
                        item.DeleteFlag = 1;
                        item.UpdateDate = DateTime.Now;
                        item.UpdateBy = _personnelId;
                    });

                    //Update Inmate Classification
                    inmate.LastClassReviewDate = null;
                    inmate.LastClassReviewBy = null;
                    inmate.FacilityId = value.FacilityId;
                    inmate.HousingUnitId = value.HousingUnitId;

                    SaveHousingAssign(value);

                    int incarcerationId = _context.Incarceration.OrderByDescending(o => o.IncarcerationId)
                        .Where(w => w.InmateId == value.InmateId && !w.ReleaseOut.HasValue)
                        .Select(s => s.IncarcerationId).FirstOrDefault();

                    IncarcerationFacilityHistory incarcerationFacilityHistory =
                        _context.IncarcerationFacilityHistory
                        .OrderByDescending(o => o.MoveDate)
                        .FirstOrDefault(w => w.InmateId == value.InmateId
                        && w.IncarcerationId == incarcerationId);

                    IncarcerationFacility incarcerationFacility = _context.IncarcerationFacility
                        .OrderByDescending(o => o.IncarcerationFrom)
                        .FirstOrDefault(w => w.IncarcerationId == incarcerationId);

                    if (!(incarcerationFacilityHistory is null)
                        && incarcerationFacilityHistory.IncarcerationFacilityHistoryId > 0)
                    {
                        incarcerationFacilityHistory.MoveDateThru = DateTime.Now;
                        incarcerationFacilityHistory.MoveDateThruBy = _personnelId;
                    }

                    IncarcerationFacilityHistory insertIncarcerationFacilityHistory = new IncarcerationFacilityHistory
                    {
                        IncarcerationId = incarcerationId,
                        InmateId = value.InmateId,
                        TransferFromFacilityId = incarcerationFacilityHistory?.FacilityId,
                        FacilityId = value.FacilityId,
                        MoveDate = DateTime.Now,
                        MoveDateBy = _personnelId
                    };
                    _context.Add(insertIncarcerationFacilityHistory);

                    if (!(incarcerationFacility is null))
                    {
                        int facilityIdFrom = incarcerationFacility.FacilityId;

                        if (incarcerationFacility.IncarcerationFacilityId > 0)
                        {
                            incarcerationFacility.IncarcerationTo = DateTime.Now;
                            incarcerationFacility.IncarcerationToBy = _personnelId;

                            if (!(incarcerationFacilityHistory is null))
                                facilityIdFrom = incarcerationFacilityHistory.FacilityId;
                        }

                        IncarcerationFacility insertIncarcerationFacility = new IncarcerationFacility
                        {
                            IncarcerationId = incarcerationId,
                            FacilityId = value.FacilityId,
                            IncarcerationFrom = DateTime.Now,
                            IncarcerationFromBy = _personnelId,
                            FacilityIdFrom = facilityIdFrom
                        };
                        _context.Add(insertIncarcerationFacility);
                    }
                }
                else
                {
                    inmate.FacilityId = value.FacilityId;
                    inmate.HousingUnitId = value.HousingUnitId;

                    SaveHousingAssign(value);
                }
                HousingUnitMoveHistory housingUnitMoveHistory = _context.HousingUnitMoveHistory.SingleOrDefault(s =>
                    s.InmateId == value.InmateId && !s.MoveDateThru.HasValue);

                if (!(housingUnitMoveHistory is null))
                {
                    housingUnitMoveHistory.MoveDateThru = DateTime.Now;
                    housingUnitMoveHistory.MoveThruBy = _personnelId;
                }
                HousingUnitMoveHistory insertHousingUnitMoveHistory = new HousingUnitMoveHistory
                {
                    InmateClassificationId = value.InmateClassificationId,
                    InmateId = value.InmateId,
                    HousingUnitFromId = value.HousingUnitFromId,
                    HousingUnitToId = value.HousingUnitId,
                    MoveOfficerId = _personnelId,
                    MoveReason = value.Reason,
                    MoveDate = DateTime.Now // mm/dd/yyyy hh.mm.ss
                };

                _context.Add(insertHousingUnitMoveHistory);

                await _context.SaveChangesAsync();
                //Event Handle
                //EventVm housingEventHandle = new EventVm
                //{
                //    PersonId = value.PersonId,
                //    InmateTrackingId = insertHousingUnitMoveHistory.HousingUnitMoveHistoryId,
                //    EventName = EventNameConstants.HOUSINGMOVE
                //};

                //// INSERT  EVENT HANDLE
                //_commonService.EventHandle(housingEventHandle);

                _interfaceEngineService.Export(new ExportRequestVm
                {
                    EventName = EventNameConstants.HOUSINGMOVE,
                    PersonnelId = _personnelId,
                    Param1 = value.PersonId.ToString(),
                    Param2 = insertHousingUnitMoveHistory.HousingUnitMoveHistoryId.ToString()
                });
                if (value.CheckIn)
                {
                    if (!(inmateTrack is null))
                    {
                        inmateTrack.InmateTrakDateIn = DateTime.Now;
                        inmateTrack.InPersonnelId = _personnelId;
                    }

                    inmate.InmateCurrentTrack = null;
                    inmate.InmateCurrentTrackId = null;
                }
            }
            return await _context.SaveChangesAsync();
        }

        private void SaveHousingAssign(HousingAssignVm value)
        {
            if (value.HousingConflictList != null)
            {
                List<HousingConflictVm> lstHousingConflict = value.HousingConflictList
                    .Where(w => w.KeepSepInmateId > 0).Distinct().ToList();

                lstHousingConflict.ForEach(item =>
                {
                    string sNote = null;
                    List<HousingConflictVm> lstHousingConflictInmate = _housingConflictService
                        .GetHousingKeepSeparate(item.KeepSepInmateId, value.FacilityId)
                        .Where(w => w.Housing.HousingUnitListId == value.HousingDetail.HousingUnitListId
                                    && w.KeepSepInmateId == value.InmateId).ToList();
                    lstHousingConflictInmate.ForEach(i =>
                    {
                        sNote += value.ConflictNotes + FloorNotesConflictConstants.TYPE +
                                 i.AssignConflictType + FloorNotesConflictConstants.DESCRIPTION +
                                 i.ConflictDescription;
                    });

                    if (!(sNote is null))
                    {
                        InsertFloorNote(sNote, item.KeepSepInmateId);
                    }
                });
                string floorNote = string.Empty;
                value.HousingConflictList.ForEach(ob =>
                {
                    floorNote += FloorNotesConflictConstants.TYPE +
                                 ob.ConflictType + FloorNotesConflictConstants.DESCRIPTION +
                                 ob.ConflictDescription;
                });
                if (!string.IsNullOrEmpty(floorNote))
                    InsertFloorNote(floorNote, value.InmateId);
            }
        }

        private void InsertFloorNote(string note, int inmateId)
        {
            FloorNotes floorNotes = new FloorNotes
            {
                FloorNoteNarrative = note,
                FloorNoteOfficerId = _personnelId,
                FloorNoteDate = DateTime.Now,
                FloorNoteType = FloorNotesConflictConstants.CONFLICTCHECK,
                FloorNoteTime = DateTime.Now.ToString(DateConstants.TIME)
            };
            _context.FloorNotes.Add(floorNotes);

            FloorNoteXref floorNoteXref = new FloorNoteXref
            {
                FloorNoteId = floorNotes.FloorNoteId,
                InmateId = inmateId,
                CreateDate = DateTime.Now
            };

            _context.FloorNoteXref.Add(floorNoteXref);
        }

        public HousingVm GetHousingFacility(int facilityId)
        {
            HousingVm housing = new HousingVm();

            _inmateList = _context.Inmate.Where(w => w.InmateActive == 1);

            //To get Housing Unit details
            _listHousingUnit = _context.HousingUnit.Where(w => w.Facility.DeleteFlag == 0
            && (w.HousingUnitInactive == 0 || !w.HousingUnitInactive.HasValue)).ToList();

            List<KeyValuePair<int, string>> lstFacilityAbbr = _context.Facility.Where(w => w.DeleteFlag == 0)
               .Select(s => new KeyValuePair<int, string>(s.FacilityId, s.FacilityAbbr)).ToList();

            if (_context.SiteOptions.Any(w => w.SiteOptionsStatus == "1"
                   && w.SiteOptionsName == SiteOptionsConstants.FORCETRANSFERDURINGFACILITYMOVE
                   && w.SiteOptionsValue == SiteOptionsConstants.ON))
            {
                housing.ShowFacility = true;
            }
            //198
            //To get housing facility details
            housing.HousingCapacityList = _listHousingUnit.GroupBy(g => g.FacilityId)
                 .Select(s => new HousingCapacityVm
                 {
                     FacilityId = s.Key,
                     FacilityAbbr = lstFacilityAbbr.SingleOrDefault(w => w.Key == s.Key).Value,
                     CurrentCapacity = s.Sum(c => c.HousingUnitActualCapacity ?? 0 - c.HousingUnitOutOfService ?? 0),
                     Assigned = _inmateList.Count(c => c.FacilityId == s.Key),
                     Out = _inmateList.Count(c => c.FacilityId == s.Key && c.InmateCurrentTrackId.HasValue),
                     OutofService = s.Sum(c => c.HousingUnitOutOfService ?? 0)
                 }).OrderBy(o => o.FacilityAbbr).ToList();
            if (housing.HousingCapacityList.All(a => a.FacilityId != facilityId))
            { facilityId = housing.HousingCapacityList[0].FacilityId; }

            housing.FacilityAbbr = lstFacilityAbbr
                .SingleOrDefault(s => s.Key == facilityId).Value;

            housing.FacilityId = facilityId;

            //To get Inmate details
            IQueryable<InmateSearchVm> inmateDetailsList = _inmateList
                   .Where(w => w.FacilityId == facilityId)
                   .OrderBy(o => o.Person.PersonLastName)
                   .ThenBy(t => t.Person.PersonMiddleName)
                   .Select(s => new InmateSearchVm
                   {
                       InmateId = s.InmateId,
                       FacilityId = s.FacilityId,
                       FacilityAbbr = s.Facility.FacilityAbbr,
                       PersonId = s.PersonId,
                       InmateNumber = s.InmateNumber,
                       LocationId = s.InmateCurrentTrackId,
                       Location = s.InmateCurrentTrack,
                       HousingDetail = new HousingDetail
                       {
                           HousingUnitId = s.HousingUnitId ?? 0,
                           HousingUnitListId = s.HousingUnit.HousingUnitListId,
                           HousingUnitLocation = s.HousingUnit.HousingUnitLocation,
                           HousingUnitNumber = s.HousingUnit.HousingUnitNumber,
                           HousingUnitBedNumber = s.HousingUnit.HousingUnitBedNumber,
                           HousingUnitBedLocation = s.HousingUnit.HousingUnitBedLocation
                       },
                       PersonDetail = new PersonInfoVm
                       {
                           PersonId = s.PersonId,
                           PersonLastName = s.Person.PersonLastName,
                           PersonMiddleName = s.Person.PersonMiddleName,
                           PersonFirstName = s.Person.PersonFirstName
                       },
                       PhotoFilePath = _photos.GetPhotoByCollectionIdentifier(s.Person.Identifiers),
                       Classify = s.InmateClassification.InmateClassificationReason,
                       Sentenced = s.Incarceration.OrderByDescending(o => o.IncarcerationId).Where(w => w.InmateId == s.InmateId)
                       .Select(se => se.OverallFinalReleaseDate).FirstOrDefault()
                   });

            housing.InmateDetailsList = inmateDetailsList.ToList();

            if (facilityId > 0)
            {
                //To get no housing details
                housing.HousingBuildingCapacityList = new List<HousingCapacityVm>
                        {
                            new HousingCapacityVm
                            {
                                FacilityId = facilityId,
                                FacilityAbbr = lstFacilityAbbr.SingleOrDefault(w => w.Key == facilityId).Value,
                                Assigned = _inmateList
                                    .Count(c => !c.HousingUnitId.HasValue && c.FacilityId==facilityId),
                                Out = _inmateList.Count(c => !c.HousingUnitId.HasValue
                                                && c.InmateCurrentTrackId.HasValue && c.FacilityId==facilityId)
                            }
                        };

                //To get housing location details
                housing.HousingBuildingCapacityList.AddRange(_listHousingUnit.Where(w => w.FacilityId == facilityId)
                        .OrderBy(o => o.HousingUnitLocation).GroupBy(g => g.HousingUnitLocation)
                        .Select(s => new HousingCapacityVm
                        {
                            HousingNumber = s.First().HousingUnitNumber,
                            HousingUnitListId = s.First().HousingUnitListId,
                            FacilityId = facilityId,
                            FacilityAbbr = lstFacilityAbbr.SingleOrDefault(w => w.Key == facilityId).Value,
                            HousingLocation = s.Key,
                            CurrentCapacity = s.Sum(c => c.HousingUnitActualCapacity ?? 0),
                            OutofService = s.Sum(c => c.HousingUnitOutOfService ?? 0)
                        }));
            }

            //To get housing header details
            housing.HousingHeaderDetails = _listHousingUnit
                .Where(w => w.FacilityId == facilityId)
                .Select(s => new HousingHeaderVm
                {
                    Status = s.Facility?.DeleteFlag ?? 0,
                    Floor = s.HousingUnitFloor,
                    Offsite = s.HousingUnitOffsite,
                    Medical = s.HousingUnitMedical,
                    Mental = s.HousingUnitMental,
                    Visitation = s.HousingUnitVisitAllow,
                    Commission = s.HousingUnitCommAllow
                }).FirstOrDefault();

            if (facilityId > 0)
                _inmateList = _inmateList.Where(w => w.FacilityId == facilityId);

            //To get Housing stats count
            housing.HousingStatsDetails = LoadHousingStatsCount(_inmateList);

            //External Location
            List<HousingPrivilegesVm> externalLocationList = _context.Inmate
                .Where(w => !w.InmateCurrentTrackNavigation.FacilityId.HasValue
                && w.InmateCurrentTrackId > 0)
                .Select(s => new HousingPrivilegesVm
                {
                    PrivilegesId = s.InmateCurrentTrackId ?? 0,
                    PrivilegesDescription = s.InmateCurrentTrackNavigation.PrivilegeDescription
                }).ToList();

            housing.ExternalLocationList = externalLocationList.GroupBy(g => new
            {
                g.PrivilegesId,
                g.PrivilegesDescription
            }).Select(x => new HousingPrivilegesVm
            {
                PrivilegesId = x.Key.PrivilegesId,
                PrivilegesDescription = x.Key.PrivilegesDescription,
                Count = x.Count()
            }).OrderBy(o => o.PrivilegesDescription).ToList();

            //Internal Location
            List<HousingPrivilegesVm> internalLocationList = _inmateList
                .Where(w => w.InmateCurrentTrackNavigation.FacilityId == facilityId
                && string.IsNullOrEmpty(w.InmateCurrentTrackNavigation.HousingUnitLocation))
                .Select(s => new HousingPrivilegesVm
                {
                    PrivilegesId = s.InmateCurrentTrackId ?? 0,
                    PrivilegesDescription = s.InmateCurrentTrack
                }).ToList();

            housing.InternalLocationList = internalLocationList.GroupBy(g => new
            {
                g.PrivilegesId,
                g.PrivilegesDescription
            }).Select(x => new HousingPrivilegesVm
            {
                PrivilegesId = x.Key.PrivilegesId,
                PrivilegesDescription = x.Key.PrivilegesDescription,
                Count = x.Count()
            }).OrderBy(o => o.PrivilegesDescription).ToList();

            return housing;
        }

        public HousingVm GetHousingFacilityDetailsForFacility(int facilityId)
        {
            HousingVm housing = new HousingVm();
            List<Inmate> inmateList = _context.Inmate.Where(w => w.InmateActive == 1).ToList();

            List<KeyValuePair<int, string>> lstFacilityAbbr = _context.Facility.Where(w => w.DeleteFlag == 0)
                .Select(s => new KeyValuePair<int, string>(s.FacilityId, s.FacilityAbbr)).ToList();

            if (_context.SiteOptions.Any(w => w.SiteOptionsStatus == "1"
                                              && w.SiteOptionsName == SiteOptionsConstants.FORCETRANSFERDURINGFACILITYMOVE
                                              && w.SiteOptionsValue == SiteOptionsConstants.ON))
            {
                housing.ShowFacility = true;
            }

            housing.HousingCapacityList = _context.HousingUnit.Where(w => w.Facility.DeleteFlag == 0
                && (w.HousingUnitInactive == 0 || !w.HousingUnitInactive.HasValue)).AsEnumerable().GroupBy(g => g.FacilityId)
                .Select(s => new HousingCapacityVm
                {
                    FacilityId = s.Key,
                    FacilityAbbr = lstFacilityAbbr.SingleOrDefault(w => w.Key == s.Key).Value,
                    CurrentCapacity = s.Sum(c => c.HousingUnitActualCapacity ?? 0 - c.HousingUnitOutOfService ?? 0),
                    Assigned = inmateList.Count(c => c.FacilityId == s.Key),
                    Out = inmateList.Count(c => c.FacilityId == s.Key
                                                 && c.InmateCurrentTrackId.HasValue),
                    OutofService = s.Sum(c => c.HousingUnitOutOfService ?? 0)
                }).OrderBy(o => o.FacilityAbbr).ToList();
            if (housing.HousingCapacityList.All(a => a.FacilityId != facilityId))
            { facilityId = housing.HousingCapacityList[0].FacilityId; }

            housing.FacilityAbbr = lstFacilityAbbr
                .SingleOrDefault(s => s.Key == facilityId).Value;
            HousingLockInputVm lockInputVm = new HousingLockInputVm
            {
                HousingType = HousingType.Facility
            };

            housing.HousingLockdownList = _lockdownService.GetHousingLockdownNotifications(lockInputVm).ToList();

            return housing;
        }

        public HousingVm GetHousingFacilityDetailsForBuilding(int facilityId)
        {
            HousingVm housing = new HousingVm();
            IQueryable<Inmate> inmateList = _context.Inmate.Where(w => w.InmateActive == 1);
            List<KeyValuePair<int, string>> lstFacilityAbbr = _context.Facility.Where(w => w.DeleteFlag == 0)
                .Select(s => new KeyValuePair<int, string>(s.FacilityId, s.FacilityAbbr)).ToList();
            IEnumerable<HousingUnit> listHousingUnit = _context.HousingUnit.Where(w => w.Facility.DeleteFlag == 0
                                                                                       && (w.HousingUnitInactive == 0 || !w.HousingUnitInactive.HasValue)).ToList();

            //To get no housing details
            housing.HousingBuildingCapacityList = new List<HousingCapacityVm>
                    {
                        new HousingCapacityVm
                        {
                            FacilityId = facilityId,
                            FacilityAbbr = lstFacilityAbbr.SingleOrDefault(w => w.Key == facilityId).Value,
                            Assigned = inmateList
                                .Count(c => !c.HousingUnitId.HasValue && c.FacilityId==facilityId),
                            Out = inmateList.Count(c => !c.HousingUnitId.HasValue
                                            && c.InmateCurrentTrackId.HasValue && c.FacilityId==facilityId)
                        }
                    };

            //To get housing location details
            housing.HousingBuildingCapacityList.AddRange(listHousingUnit.Where(w => w.FacilityId == facilityId)
                    .OrderBy(o => o.HousingUnitLocation).GroupBy(g => g.HousingUnitLocation)
                    .Select(s => new HousingCapacityVm
                    {
                        HousingNumber = s.First().HousingUnitNumber,
                        HousingUnitListId = s.First().HousingUnitListId,
                        FacilityId = facilityId,
                        FacilityAbbr = lstFacilityAbbr.SingleOrDefault(w => w.Key == facilityId).Value,
                        HousingLocation = s.Key,
                        CurrentCapacity = s.Sum(c => c.HousingUnitActualCapacity ?? 0),
                        OutofService = s.Sum(c => c.HousingUnitOutOfService ?? 0)
                    }));

            //To get housing header details
            housing.HousingHeaderDetails = listHousingUnit
                .Where(w => w.FacilityId == facilityId)
                .Select(s => new HousingHeaderVm
                {
                    Status = s.Facility?.DeleteFlag ?? 0,
                    Floor = s.HousingUnitFloor,
                    Offsite = s.HousingUnitOffsite,
                    Medical = s.HousingUnitMedical,
                    Mental = s.HousingUnitMental,
                    Visitation = s.HousingUnitVisitAllow,
                    Commission = s.HousingUnitCommAllow
                }).FirstOrDefault();
            HousingLockInputVm lockInputVm = new HousingLockInputVm
            {
                HousingType = HousingType.HousingLocation,
                FacilityId = facilityId
            };
            housing.HousingBuildingLockdownList = _lockdownService.GetHousingLockdownNotifications(lockInputVm).ToList();
            return housing;
        }

        public HousingVm GetHousingFacilityDetailsForInternalExternalLocation(int facilityId)
        {
            HousingVm housing = new HousingVm();
            IQueryable<Inmate> inmateList =
                _context.Inmate.Where(w => w.InmateActive == 1 && w.FacilityId == facilityId);

            //To get Housing stats count
            housing.HousingStatsDetails = LoadHousingStatsCount(inmateList);

            //External Location
            List<HousingPrivilegesVm> externalLocationList = _context.Inmate
                .Where(w => !w.InmateCurrentTrackNavigation.FacilityId.HasValue
                && w.InmateCurrentTrackId > 0)
                .Select(s => new HousingPrivilegesVm
                {
                    PrivilegesId = s.InmateCurrentTrackId ?? 0,
                    PrivilegesDescription = s.InmateCurrentTrackNavigation.PrivilegeDescription
                }).ToList();

            housing.ExternalLocationList = externalLocationList.GroupBy(g => new
            {
                g.PrivilegesId,
                g.PrivilegesDescription
            }).Select(x => new HousingPrivilegesVm
            {
                PrivilegesId = x.Key.PrivilegesId,
                PrivilegesDescription = x.Key.PrivilegesDescription,
                Count = x.Count()
            }).OrderBy(o => o.PrivilegesDescription).ToList();

            //Internal Location
            List<HousingPrivilegesVm> internalLocationList = inmateList
                .Where(w => w.InmateCurrentTrackNavigation.FacilityId == facilityId
                && string.IsNullOrEmpty(w.InmateCurrentTrackNavigation.HousingUnitLocation))
                .Select(s => new HousingPrivilegesVm
                {
                    PrivilegesId = s.InmateCurrentTrackId ?? 0,
                    PrivilegesDescription = s.InmateCurrentTrackNavigation.PrivilegeDescription
                }).ToList();

            housing.InternalLocationList = internalLocationList.GroupBy(g => new
            {
                g.PrivilegesId,
                g.PrivilegesDescription
            }).Select(x => new HousingPrivilegesVm
            {
                PrivilegesId = x.Key.PrivilegesId,
                PrivilegesDescription = x.Key.PrivilegesDescription,
                Count = x.Count()
            }).OrderBy(o => o.PrivilegesDescription).ToList();
            return housing;
        }

        public HousingVm GetHousingFacilityDetailsForInmates(int facilityId)
        {
            HousingVm housing = new HousingVm();
            IQueryable<Inmate> inmateList = _context.Inmate.Where(w => w.InmateActive == 1);
            housing.InmateDetailsList = inmateList
                   .Where(w => w.FacilityId == facilityId)
                   .OrderBy(o => o.Person.PersonLastName)
                   .ThenBy(t => t.Person.PersonMiddleName)
                   .Select(s => new InmateSearchVm
                   {
                       InmateId = s.InmateId,
                       FacilityId = s.FacilityId,
                       FacilityAbbr = s.Facility.FacilityAbbr,
                       PersonId = s.PersonId,
                       InmateNumber = s.InmateNumber,
                       LocationId = s.InmateCurrentTrackId,
                       Location = s.InmateCurrentTrack,
                       HousingDetail = new HousingDetail
                       {
                           HousingUnitId = s.HousingUnitId ?? 0,
                           HousingUnitListId = s.HousingUnit.HousingUnitListId,
                           HousingUnitLocation = s.HousingUnit.HousingUnitLocation,
                           HousingUnitNumber = s.HousingUnit.HousingUnitNumber,
                           HousingUnitBedNumber = s.HousingUnit.HousingUnitBedNumber,
                           HousingUnitBedLocation = s.HousingUnit.HousingUnitBedLocation
                       },
                       PersonDetail = new PersonInfoVm
                       {
                           PersonId = s.PersonId,
                           PersonLastName = s.Person.PersonLastName,
                           PersonMiddleName = s.Person.PersonMiddleName,
                           PersonFirstName = s.Person.PersonFirstName
                       },
                       Classify = s.InmateClassification.InmateClassificationReason,
                       Sentenced = s.Incarceration.OrderByDescending(o => o.IncarcerationId).Where(w => w.InmateId == s.InmateId)
                       .Select(se => se.OverallFinalReleaseDate).FirstOrDefault()
                   }).ToList();
            return housing;
        }

        public List<InmateSearchVm> GetHousingInmateHistory(HousingInputVm value)
        {
            if (value.HousingType == HousingType.NoHousing)
                return null;

            IQueryable<HousingUnitMoveHistory> housingUnitMoveHistory = _context.HousingUnitMoveHistory
                .Where(w => w.HousingUnitToId.HasValue && w.MoveDate <= (value.ThruDate ?? DateTime.Now)
                            && (w.MoveDateThru ?? DateTime.Now) >= (value.FromDate ?? DateTime.Now));

            if (value.FacilityId > 0)
            {
                housingUnitMoveHistory = housingUnitMoveHistory
                    .Where(w => w.HousingUnitTo.FacilityId == value.FacilityId);
            }
            if (value.HousingBedLocation != null)
            {
                housingUnitMoveHistory = housingUnitMoveHistory
                    .Where(w => w.HousingUnitTo.HousingUnitBedLocation == value.HousingBedLocation
                    && w.HousingUnitTo.HousingUnitListId == value.HousingUnitListId);
            }
            else if (value.HousingBedNumber != null && value.HousingBedLocation == null)
            {
                housingUnitMoveHistory = housingUnitMoveHistory
                    .Where(w => w.HousingUnitTo.HousingUnitBedNumber == value.HousingBedNumber
                    && w.HousingUnitTo.HousingUnitListId == value.HousingUnitListId);
            }
            else if (value.HousingBedNumber == null && value.HousingUnitListId > 0)
            {
                housingUnitMoveHistory = housingUnitMoveHistory
                    .Where(w => w.Inmate.HousingUnit.HousingUnitListId == value.HousingUnitListId);
            }
            else if (value.HousingUnitListId == 0 && value.HousingLocation != null)
            {
                housingUnitMoveHistory = housingUnitMoveHistory
                    .Where(w => w.HousingUnitTo.HousingUnitLocation == value.HousingLocation);
            }

            List<InmateSearchVm> housingInmateHistory = housingUnitMoveHistory
                .OrderByDescending(o => o.MoveDate).Select(s =>
                  new InmateSearchVm
                  {
                      InmateId = s.Inmate.InmateId,
                      PersonDetail = new PersonInfoVm
                      {
                          PersonLastName = s.Inmate.Person.PersonLastName,
                          PersonFirstName = s.Inmate.Person.PersonFirstName,
                          PersonMiddleName = s.Inmate.Person.PersonMiddleName,
                          PersonId = s.Inmate.PersonId
                      },
                      InmateNumber = s.Inmate.InmateNumber,
                      HousingUnitId = s.HousingUnitToId ?? 0,
                      Location = s.Inmate.InmateTrak.OrderByDescending(o => o.InmateTrakId)
                      .Select(se => se.InmateTrakLocation).FirstOrDefault(),
                      FromDate = s.MoveDate,
                      ThruDate = s.MoveDateThru,
                      Active = s.Inmate.InmateActive == 1,
                      PhotoFilePath = _photos.GetPhotoByCollectionIdentifier(s.Inmate.Person.Identifiers),
                      HousingDetail = new HousingDetail
                      {
                          HousingUnitListId = s.Inmate.HousingUnitId != null ? s.Inmate.HousingUnit.HousingUnitListId : default,
                          HousingUnitLocation = s.Inmate.HousingUnitId != null ? s.Inmate.HousingUnit.HousingUnitLocation : null,
                          HousingUnitNumber = s.Inmate.HousingUnitId != null ? s.Inmate.HousingUnit.HousingUnitNumber : null,
                          HousingUnitBedNumber = s.Inmate.HousingUnitId != null ? s.Inmate.HousingUnit.HousingUnitBedNumber : null,
                          HousingUnitBedLocation = s.Inmate.HousingUnitId != null ? s.Inmate.HousingUnit.HousingUnitBedLocation : null
                      }

                  }).OrderByDescending(o => o.FromDate).ToList();

            return housingInmateHistory;
        }

        public HousingVm GetFacilityLocationDetails(int locationId, int facilityId)
        {
            HousingVm housing = new HousingVm();

            //To get Housing Unit details
            _listHousingUnit = _context.HousingUnit.ToList();

            _inmateList = _context.Inmate
                .Where(w => w.InmateActive == 1 && w.InmateCurrentTrackId == locationId);

            if (facilityId > 0)
            {
                _inmateList = _inmateList.Where(w => w.FacilityId == facilityId);
                _listHousingUnit = _listHousingUnit.Where(w => w.FacilityId == facilityId).ToList();
            }

            //To get Housing stats count
            housing.HousingStatsDetails = LoadHousingStatsCount(_inmateList);

            // List<InmateSearchVm> inmateList
            housing.InmateDetailsList = _inmateList
                  .Select(s => new InmateSearchVm
                  {
                      FacilityId = s.FacilityId,
                      FacilityAbbr = s.Facility.FacilityAbbr,
                      InmateId = s.InmateId,
                      InmateNumber = s.InmateNumber,
                      LocationId = s.InmateCurrentTrackId,
                      Location = s.InmateCurrentTrack,
                      HousingDetail = new HousingDetail
                      {
                          HousingUnitId = s.HousingUnitId ?? 0,
                          HousingUnitListId = s.HousingUnit.HousingUnitListId,
                          HousingUnitLocation = s.HousingUnit.HousingUnitLocation ?? string.Empty,
                          HousingUnitNumber = s.HousingUnit.HousingUnitNumber ?? string.Empty,
                          HousingUnitBedNumber = s.HousingUnit.HousingUnitBedNumber ?? string.Empty,
                          HousingUnitBedLocation = s.HousingUnit.HousingUnitBedLocation ?? string.Empty
                      },
                      PersonDetail = new PersonInfoVm
                      {
                          PersonId = s.PersonId,
                          PersonLastName = s.Person.PersonLastName,
                          PersonMiddleName = s.Person.PersonMiddleName,
                          PersonFirstName = s.Person.PersonFirstName
                      },
                      PhotoFilePath = _photos.GetPhotoByCollectionIdentifier(s.Person.Identifiers),
                      Classify = s.InmateClassification.InmateClassificationReason,
                      Sentenced = s.Incarceration.OrderByDescending(o => o.IncarcerationId).Where(w => w.InmateId == s.InmateId)
                          .Select(se => se.OverallFinalReleaseDate).FirstOrDefault()
                  }).OrderBy(o => o.PersonDetail.PersonLastName)
                   .ThenBy(t => t.PersonDetail.PersonLastName)
                   .ThenBy(t => t.PersonDetail.PersonMiddleName).ToList();

            return housing;
        }

        // To get housing details based on housing level
        public HousingVm GetNoHousingDetails(int facilityId)
        {
            HousingVm housing = new HousingVm();

            //To get inmate details
            _inmateList = _context.Inmate.Where(w => w.InmateActive == 1
                            && w.FacilityId == facilityId && !w.HousingUnitId.HasValue);

            housing.InmateDetailsList = _inmateList
                  .Where(w => !w.HousingUnitId.HasValue && w.FacilityId == facilityId)
                  .Select(s => new InmateSearchVm
                  {
                      FacilityId = s.FacilityId,
                      FacilityAbbr = s.Facility.FacilityAbbr,
                      InmateId = s.InmateId,
                      InmateNumber = s.InmateNumber,
                      LocationId = s.InmateCurrentTrackId,
                      Location = s.InmateCurrentTrack,
                      PersonDetail = new PersonInfoVm
                      {
                          PersonId = s.PersonId,
                          PersonLastName = s.Person.PersonLastName,
                          PersonMiddleName = s.Person.PersonMiddleName,
                          PersonFirstName = s.Person.PersonFirstName
                      },
                      PhotoFilePath = _photos.GetPhotoByCollectionIdentifier(s.Person.Identifiers),
                      Classify = s.InmateClassification.InmateClassificationReason,
                      Sentenced = s.Incarceration.OrderByDescending(o => o.IncarcerationId).Where(w => w.InmateId == s.InmateId)
                      .Select(se => se.OverallFinalReleaseDate).FirstOrDefault()
                  }).OrderBy(o => o.PersonDetail.PersonLastName)
                   .ThenBy(t => t.PersonDetail.PersonLastName)
                   .ThenBy(t => t.PersonDetail.PersonMiddleName).ToList();

            //To get Housing Stats details
            housing.HousingStatsDetails = LoadHousingStatsCount(_inmateList);

            return housing;
        }

        //To get facility housing details
        public HousingVm GetBuildingDetails(int facilityId)
        {
            HousingVm housing = new HousingVm();

            //To get Housing Unit details
            _listHousingUnit = _context.HousingUnit
                .Where(w => w.FacilityId == facilityId && (w.HousingUnitInactive == 0 || !w.HousingUnitInactive.HasValue)).ToList();

            housing.FacilityAbbr = _context.Facility.Single(s => s.FacilityId == facilityId).FacilityAbbr;

            string siteOptionsValue = _context.SiteOptions.Single(w => w.SiteOptionsStatus == "1"
                && w.SiteOptionsName == SiteOptionsConstants.FORCETRANSFERDURINGFACILITYMOVE).SiteOptionsValue;

            if (siteOptionsValue == SiteOptionsConstants.ON)
            {
                housing.ShowFacility = true;
            }

            _inmateList = _context.Inmate
                .Where(w => w.InmateActive == 1 && w.FacilityId == facilityId);

            //To get Housing stats count
            housing.HousingStatsDetails = LoadHousingStatsCount(_inmateList);


            //To get Inmate details
            housing.InmateDetailsList = _inmateList
                   .Where(w => w.FacilityId == facilityId)
                   .Select(s => new InmateSearchVm
                   {
                       InmateId = s.InmateId,
                       FacilityId = s.FacilityId,
                       FacilityAbbr = s.Facility.FacilityAbbr,
                       PersonId = s.PersonId,
                       InmateNumber = s.InmateNumber,
                       LocationId = s.InmateCurrentTrackId,
                       Location = s.InmateCurrentTrack,
                       HousingDetail = new HousingDetail
                       {
                           HousingUnitId = s.HousingUnitId ?? 0,
                           HousingUnitListId = s.HousingUnit.HousingUnitListId,
                           HousingUnitLocation = s.HousingUnit.HousingUnitLocation,
                           HousingUnitNumber = s.HousingUnit.HousingUnitNumber,
                           HousingUnitBedNumber = s.HousingUnit.HousingUnitBedNumber,
                           HousingUnitBedLocation = s.HousingUnit.HousingUnitBedLocation
                       },
                       PersonDetail = new PersonInfoVm
                       {
                           PersonId = s.PersonId,
                           PersonLastName = s.Person.PersonLastName,
                           PersonMiddleName = s.Person.PersonMiddleName,
                           PersonFirstName = s.Person.PersonFirstName
                       },
                       PhotoFilePath = _photos.GetPhotoByCollectionIdentifier(s.Person.Identifiers),
                       Classify = s.InmateClassification.InmateClassificationReason,
                       Sentenced = s.Incarceration.OrderByDescending(o => o.IncarcerationId).Where(w => w.InmateId == s.InmateId)
                       .Select(se => se.OverallFinalReleaseDate).FirstOrDefault()
                   }).OrderBy(o => o.PersonDetail.PersonLastName)
                   .ThenBy(t => t.PersonDetail.PersonLastName)
                   .ThenBy(t => t.PersonDetail.PersonMiddleName).ToList();


            housing.HousingCapacityList = new List<HousingCapacityVm>
            {
                new HousingCapacityVm
                {
                    Assigned =_inmateList
                    .Count(c => !c.HousingUnitId.HasValue && c.FacilityId==facilityId),
                    Out = _inmateList
                    .Count(c => !c.HousingUnitId.HasValue
                                && c.InmateCurrentTrackId.HasValue && c.FacilityId==facilityId)
                }
            };

            //To get housing location details
            housing.HousingCapacityList.AddRange(_listHousingUnit.OrderBy(o => o.HousingUnitLocation)
                .GroupBy(g => g.HousingUnitLocation)
                .Select(s => new HousingCapacityVm
                {
                    HousingLocation = s.Key,
                    CurrentCapacity = s.Sum(c => c.HousingUnitActualCapacity ?? 0),
                    OutofService = s.Sum(c => c.HousingUnitOutOfService ?? 0)
                }));

            //To get housing header details
            housing.HousingHeaderDetails = _listHousingUnit.Select(s => new HousingHeaderVm
            {
                Status = s.Facility?.DeleteFlag ?? 0,
                Floor = s.HousingUnitFloor,
                Offsite = s.HousingUnitOffsite,
                Medical = s.HousingUnitMedical,
                Mental = s.HousingUnitMental,
                Visitation = s.HousingUnitVisitAllow,
                Commission = s.HousingUnitCommAllow
            }).FirstOrDefault();

            //External Location
            List<HousingPrivilegesVm> externalLocationList = _context.Inmate
                .Where(w => !w.InmateCurrentTrackNavigation.FacilityId.HasValue
                            && w.InmateCurrentTrackId > 0)
                .Select(s => new HousingPrivilegesVm
                {
                    PrivilegesId = s.InmateCurrentTrackId ?? 0,
                    PrivilegesDescription = s.InmateCurrentTrackNavigation.PrivilegeDescription
                }).ToList();

            housing.ExternalLocationList = externalLocationList.GroupBy(g => new
            {
                g.PrivilegesId,
                g.PrivilegesDescription
            }).Select(x => new HousingPrivilegesVm
            {
                PrivilegesId = x.Key.PrivilegesId,
                PrivilegesDescription = x.Key.PrivilegesDescription,
                Count = x.Count()
            }).OrderBy(o => o.PrivilegesDescription).ToList();

            //Internal Location
            List<HousingPrivilegesVm> internalLocationList = _inmateList
                .Where(w => w.InmateCurrentTrackNavigation.FacilityId == facilityId
                 && string.IsNullOrEmpty(w.InmateCurrentTrackNavigation.HousingUnitLocation))
                .Select(s => new HousingPrivilegesVm
                {
                    PrivilegesId = s.InmateCurrentTrackId ?? 0,
                    PrivilegesDescription = s.InmateCurrentTrackNavigation.PrivilegeDescription
                }).ToList();

            housing.InternalLocationList = internalLocationList.GroupBy(g => new
            {
                g.PrivilegesId,
                g.PrivilegesDescription
            }).Select(x => new HousingPrivilegesVm
            {
                PrivilegesId = x.Key.PrivilegesId,
                PrivilegesDescription = x.Key.PrivilegesDescription,
                Count = x.Count()
            }).OrderBy(o => o.PrivilegesDescription).ToList();

            HousingLockInputVm lockInputVm = new HousingLockInputVm
            {
                HousingType = HousingType.HousingLocation,
                FacilityId = facilityId
            };

            housing.HousingBuildingLockdownList = _lockdownService.GetHousingLockdownNotifications(lockInputVm).ToList();
            return housing;
        }

        public HousingVm GetLocationInmateDetails(HousingInputVm value)
        {
            HousingVm housing = new HousingVm();

            //To get inmate details
            _inmateList = _context.Inmate
                .Where(w => w.InmateActive == 1
                            && w.FacilityId == value.FacilityId && w.HousingUnitId.HasValue);

            switch (value.HousingType)
            {
                case HousingType.Facility:

                    IQueryable<InmateSearchVm> inmateDetailsList = _inmateList
                   .OrderBy(o => o.Person.PersonLastName)
                   .Select(s => new InmateSearchVm
                   {
                       InmateId = s.InmateId,
                       FacilityId = s.FacilityId,
                       FacilityAbbr = s.Facility.FacilityAbbr,
                       PersonId = s.PersonId,
                       InmateNumber = s.InmateNumber,
                       LocationId = s.InmateCurrentTrackId,
                       Location = s.InmateCurrentTrack,
                       HousingDetail = new HousingDetail
                       {
                           HousingUnitId = s.HousingUnitId ?? 0,
                           HousingUnitListId = s.HousingUnit.HousingUnitListId,
                           HousingUnitLocation = s.HousingUnit.HousingUnitLocation,
                           HousingUnitNumber = s.HousingUnit.HousingUnitNumber,
                           HousingUnitBedNumber = s.HousingUnit.HousingUnitBedNumber,
                           HousingUnitBedLocation = s.HousingUnit.HousingUnitBedLocation
                       },
                       PersonDetail = new PersonInfoVm
                       {
                           PersonId = s.PersonId,
                           PersonLastName = s.Person.PersonLastName,
                           PersonMiddleName = s.Person.PersonMiddleName,
                           PersonFirstName = s.Person.PersonFirstName
                       },
                       PhotoFilePath = _photos.GetPhotoByCollectionIdentifier(s.Person.Identifiers),
                       Classify = s.InmateClassification.InmateClassificationReason,
                       Sentenced = s.Incarceration.OrderByDescending(o => o.IncarcerationId).Where(w => w.InmateId == s.InmateId)
                       .Select(se => se.OverallFinalReleaseDate).FirstOrDefault()
                   });

                    housing.InmateDetailsList = inmateDetailsList.OrderBy(o => o.PersonDetail.PersonLastName)
                           .ThenBy(t => t.PersonDetail.PersonLastName)
                           .ThenBy(t => t.PersonDetail.PersonMiddleName).ToList();

                    break;
                case HousingType.HousingLocation:

                    _inmateList = _inmateList
                   .Where(w => w.HousingUnitId.HasValue
                               && w.HousingUnit.HousingUnitLocation == value.HousingLocation);

                    housing.InmateDetailsList = GetInmateList(value.FacilityId, false).ToList();

                    break;
                case HousingType.Number:
                    _inmateList = _inmateList
                    .Where(w => w.HousingUnitId.HasValue
                                && w.HousingUnit.HousingUnitListId == value.HousingUnitListId);

                    housing.InmateDetailsList = GetInmateList(value.FacilityId, false).ToList();

                    break;
                case HousingType.BedNumber:

                    //To get inmate details
                    _inmateList = _inmateList
                        .Where(w => w.HousingUnitId.HasValue
                                    && w.HousingUnit.HousingUnitBedNumber == value.HousingBedNumber
                                    && w.HousingUnit.HousingUnitListId == value.HousingUnitListId);

                    housing.InmateDetailsList = GetInmateList(value.FacilityId, false).ToList();

                    break;
                case HousingType.BedLocation:
                    //To get inmate details
                    _inmateList = _inmateList
                        .Where(w => w.HousingUnitId.HasValue
                                    && w.HousingUnit.HousingUnitBedNumber == value.HousingBedNumber
                                    && w.HousingUnit.HousingUnitBedLocation == value.HousingBedLocation
                                    && w.HousingUnit.HousingUnitListId == value.HousingUnitListId);

                    housing.InmateDetailsList = GetInmateList(value.FacilityId, false).ToList();

                    break;
            }

            return housing;
        }

        public HousingVm GetFacilityInmateDetails(int facilityId)
        {
            HousingVm housing = new HousingVm();

            //To get inmate details
            _inmateList = _context.Inmate.Where(w => w.InmateActive == 1 && w.FacilityId == facilityId);

            IQueryable<InmateSearchVm> inmateDetailsList = _inmateList
           .OrderBy(o => o.Person.PersonLastName)
           .Select(s => new InmateSearchVm
           {
               InmateId = s.InmateId,
               FacilityId = s.FacilityId,
               FacilityAbbr = s.Facility.FacilityAbbr,
               PersonId = s.PersonId,
               InmateNumber = s.InmateNumber,
               LocationId = s.InmateCurrentTrackId,
               Location = s.InmateCurrentTrack,
               HousingDetail = new HousingDetail
               {
                   HousingUnitId = s.HousingUnitId ?? 0,
                   HousingUnitListId = s.HousingUnit.HousingUnitListId,
                   HousingUnitLocation = s.HousingUnit.HousingUnitLocation,
                   HousingUnitNumber = s.HousingUnit.HousingUnitNumber,
                   HousingUnitBedNumber = s.HousingUnit.HousingUnitBedNumber,
                   HousingUnitBedLocation = s.HousingUnit.HousingUnitBedLocation
               },
               PersonDetail = new PersonInfoVm
               {
                   PersonId = s.PersonId,
                   PersonLastName = s.Person.PersonLastName,
                   PersonMiddleName = s.Person.PersonMiddleName,
                   PersonFirstName = s.Person.PersonFirstName
               },
               PhotoFilePath = _photos.GetPhotoByCollectionIdentifier(s.Person.Identifiers),
               Classify = s.InmateClassification.InmateClassificationReason,
               Sentenced = s.Incarceration.OrderByDescending(o => o.IncarcerationId).Where(w => w.InmateId == s.InmateId)
               .Select(se => se.OverallFinalReleaseDate).FirstOrDefault()
           });

            housing.InmateDetailsList = inmateDetailsList.OrderBy(o => o.PersonDetail.PersonLastName)
                   .ThenBy(t => t.PersonDetail.PersonLastName)
                   .ThenBy(t => t.PersonDetail.PersonMiddleName).ToList();

            //External Location
            List<HousingPrivilegesVm> externalLocationList = _context.Inmate
                .Where(w => !w.InmateCurrentTrackNavigation.FacilityId.HasValue
                            && w.InmateCurrentTrackId > 0)
                .Select(s => new HousingPrivilegesVm
                {
                    PrivilegesId = s.InmateCurrentTrackId ?? 0,
                    PrivilegesDescription = s.InmateCurrentTrackNavigation.PrivilegeDescription
                }).ToList();

            housing.ExternalLocationList = externalLocationList.GroupBy(g => new
            {
                g.PrivilegesId,
                g.PrivilegesDescription
            }).Select(x => new HousingPrivilegesVm
            {
                PrivilegesId = x.Key.PrivilegesId,
                PrivilegesDescription = x.Key.PrivilegesDescription,
                Count = x.Count()
            }).OrderBy(o => o.PrivilegesDescription).ToList();

            //Internal Location
            List<HousingPrivilegesVm> internalLocationList = _inmateList
                .Where(w => w.InmateCurrentTrackNavigation.FacilityId == facilityId
                && string.IsNullOrEmpty(w.InmateCurrentTrackNavigation.HousingUnitLocation))
                .Select(s => new HousingPrivilegesVm
                {
                    PrivilegesId = s.InmateCurrentTrackId ?? 0,
                    PrivilegesDescription = s.InmateCurrentTrackNavigation.PrivilegeDescription
                }).ToList();

            housing.InternalLocationList = internalLocationList.GroupBy(g => new
            {
                g.PrivilegesId,
                g.PrivilegesDescription
            }).Select(x => new HousingPrivilegesVm
            {
                PrivilegesId = x.Key.PrivilegesId,
                PrivilegesDescription = x.Key.PrivilegesDescription,
                Count = x.Count()
            }).OrderBy(o => o.PrivilegesDescription).ToList();

            return housing;
        }

        private List<InmateSearchVm> GetTotalSeparation(List<PersonFlag> flag, int conflictType)
        {
            List<InmateSearchVm> inmateDetailsList = new List<InmateSearchVm>();
            List<int> sepFlag = flag.SelectMany(p => _context.Lookup.Where(w =>
                w.LookupInactive == 0 && w.LookupFlag6 == 1 && w.LookupType == LookupConstants.PERSONCAUTION
                && w.LookupIndex == p.PersonFlagIndex && p.PersonFlagIndex > 0
                || w.LookupType == LookupConstants.TRANSCAUTION
                && w.LookupIndex == p.InmateFlagIndex && p.InmateFlagIndex > 0 && w.LookupFlag6 == 1), 
                (p, l) => p.PersonId).ToList();
            if (sepFlag.Count <= 0) return inmateDetailsList;

            inmateDetailsList = _inmateList.Select(s => new InmateSearchVm
            {
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
            if (conflictType == 1)
            {
                inmateDetailsList = inmateDetailsList.Where(w => sepFlag.Contains(w.PersonId)).ToList();
            }
            return inmateDetailsList;
        }
        public HousingVm GetPodHousingDetails(HousingInputVm value)
        {
            HousingVm housing = new HousingVm();
            List<InmateSearchVm> totalSepDetails = new List<InmateSearchVm>();
            //To get HousingUnit details
            IEnumerable<HousingUnit> listHousingUnit = _context.HousingUnit
                .Where(w => w.FacilityId == value.FacilityId && (w.HousingUnitInactive == 0 || !w.HousingUnitInactive.HasValue)
                && w.HousingUnitListId == value.HousingUnitListId);

            //To get inmate details
            _inmateList = _context.Inmate.Where(w => w.InmateActive == 1
                            && w.FacilityId == value.FacilityId);
            // To get the person flag list
            IQueryable<PersonFlag> personFlag = _context.PersonFlag
                .Where(w => w.Person.Inmate.Any(s => s.InmateActive == 1) && w.DeleteFlag == 0
                && (w.InmateFlagIndex > 0 || w.PersonFlagIndex > 0));

            housing.InmateDetailsList = GetInmateList(value.FacilityId, true).ToList();

            //To get HousingUnt Location details
            IEnumerable<HousingUnit> housingUnits = listHousingUnit.ToList();

            switch (value.HousingType)
            {
                case HousingType.BedNumber:

                    //To get inmate details
                    _inmateList = _inmateList
                        .Where(w => w.HousingUnitId.HasValue
                                    && w.HousingUnit.HousingUnitBedNumber == value.HousingBedNumber
                                    && w.HousingUnit.HousingUnitListId == value.HousingUnitListId);

                    housing.HousingCapacityList = housingUnits
                       .Where(w => w.HousingUnitBedNumber != null)
                       .OrderBy(o => o.HousingUnitBedNumber)
                       .GroupBy(g => new { g.HousingUnitNumber, g.HousingUnitBedNumber })
                       .Select(s => new HousingCapacityVm
                       {
                           FacilityId = value.FacilityId,
                           HousingLocation = value.HousingLocation,
                           HousingNumber = s.Key.HousingUnitNumber,
                           HousingBedNumber = s.Key.HousingUnitBedNumber,
                           HousingUnitListId = value.HousingUnitListId,
                           CurrentCapacity = s.Sum(c => c.HousingUnitActualCapacity ?? 0),
                           OutofService = s.Sum(c => c.HousingUnitOutOfService ?? 0),
                           HavingNextLevel = s.Any(a => a.HousingUnitListId == value.HousingUnitListId
                                             && a.HousingUnitBedNumber == s.Key.HousingUnitBedNumber
                                             && a.HousingUnitBedLocation != null),
                           ClassifyConflictCheck = true,
                           HousingClassifyString = s.Where(w => w.HousingUnitClassifyRecString != null)
                               .Select(se => se.HousingUnitClassifyRecString.Split(','))
                               .SelectMany(se => se).Distinct().ToList(),
                           HousingFlagString = s.Where(se => !string.IsNullOrEmpty(se.HousingUnitFlagString))
                               .Select(se => se.HousingUnitFlagString).FirstOrDefault(),
                           HousingAttributeString = s.Where(se => se.HousingUnitAttributeString != "")
                            .Select(se => se.HousingUnitAttributeString).FirstOrDefault()
                       }).ToList();

                    //To get HousingUnt Location details
                    housing.BunkCapacityList = listHousingUnit.Where(w => w.HousingUnitBedLocation != null)
                        .OrderBy(o => o.HousingUnitBedLocation).ThenBy(t => t.HousingUnitBedNumber)
                        .GroupBy(g => new { g.HousingUnitNumber, g.HousingUnitBedNumber, g.HousingUnitBedLocation })
                        .Select(s => new HousingCapacityVm
                        {
                            FacilityId = value.FacilityId,
                            HousingLocation = value.HousingLocation,
                            HousingNumber = s.Key.HousingUnitNumber,
                            HousingUnitListId = value.HousingUnitListId,
                            HousingBedNumber = s.Key.HousingUnitBedNumber,
                            HousingBedLocation = s.Key.HousingUnitBedLocation,
                            CurrentCapacity = s.Sum(c => c.HousingUnitActualCapacity ?? 0),
                            OutofService = s.Sum(c => c.HousingUnitOutOfService ?? 0),
                            ClassifyConflictCheck = true,
                            HousingClassifyString = s.Where(w => w.HousingUnitClassifyRecString != null)
                                .Select(se => se.HousingUnitClassifyRecString.Split(','))
                                .SelectMany(se => se).Distinct().ToList(),
                            HousingFlagString = s.Where(se => !string.IsNullOrEmpty(se.HousingUnitFlagString))
                                .Select(se => se.HousingUnitFlagString).FirstOrDefault(),
                            HousingAttributeString = s.Where(se => se.HousingUnitAttributeString != "")
                            .Select(se => se.HousingUnitAttributeString).FirstOrDefault()
                        }).ToList();

                    //To get HousingUnit details
                    _listHousingUnit = listHousingUnit
                        .Where(w => w.HousingUnitBedNumber == value.HousingBedNumber
                                    && w.HousingUnitListId == value.HousingUnitListId).ToList();

                    //housing.InmateDetailsList = GetInmateList(value.FacilityId, true).ToList();

                    housing.HousingFlag = GetHousingFlagList();
                    housing.HousingGenderDetails = LoadHousingGender(value.InmateId, value.FacilityId)
                        .Where(w => w.HousingDetail.HousingUnitListId == value.HousingUnitListId).ToList();
                    // Total separation housing conflict
                    totalSepDetails.AddRange(GetTotalSeparation(personFlag.Where(w => w.Person.Inmate.Any(
                            a => a.FacilityId == value.FacilityId
                                && a.HousingUnit.HousingUnitListId == value.HousingUnitListId)).ToList(), 1));

                    if (value.InmateId > 0)
                    {
                        // Total separation inmate conflict
                        totalSepDetails.AddRange(GetTotalSeparation(personFlag.Where(w => w.Person.Inmate.Any(
                        a => a.InmateId == value.InmateId)).ToList(), 2).Where(w =>
                        w.HousingDetail.HousingUnitListId == value.HousingUnitListId));
                    }

                    if (value.InmateId > 0 && _listHousingUnit.All(a => a.HousingUnitBedLocation == null))
                    {
                        //To get Keep Separate details
                        housing.HousingConflictList = _housingConflictService.GetInmateHousingConflictVm(value);
                        housing.HousingAttribute = GetHousingAttribute();
                    }
                    else if (_listHousingUnit.All(a => a.HousingUnitBedLocation == null))
                    {
                        housing.HousingAttribute = GetHousingAttribute();
                        //To get Keep Separate details
                        housing.HousingConflictList = _housingConflictService.GetHousingConflictVm(value);
                    }
                   
                    break;
                case HousingType.BedLocation:

                    //To get inmate details
                    _inmateList = _inmateList
                        .Where(w => w.HousingUnitId.HasValue
                                    && w.HousingUnit.HousingUnitBedNumber == value.HousingBedNumber
                                    && w.HousingUnit.HousingUnitBedLocation == value.HousingBedLocation
                                    && w.HousingUnit.HousingUnitListId == value.HousingUnitListId);

                    housing.HousingCapacityList = housingUnits
                      .Where(w => w.HousingUnitBedNumber != null)
                      .OrderBy(o => o.HousingUnitBedNumber)
                      .GroupBy(g => new { g.HousingUnitNumber, g.HousingUnitBedNumber })
                      .Select(s => new HousingCapacityVm
                      {
                          FacilityId = value.FacilityId,
                          HousingLocation = value.HousingLocation,
                          HousingNumber = s.Key.HousingUnitNumber,
                          HousingBedNumber = s.Key.HousingUnitBedNumber,
                          HousingUnitListId = value.HousingUnitListId,
                          CurrentCapacity = s.Sum(c => c.HousingUnitActualCapacity ?? 0),
                          OutofService = s.Sum(c => c.HousingUnitOutOfService ?? 0),
                          HavingNextLevel = s.Any(a => a.HousingUnitListId == value.HousingUnitListId
                                            && a.HousingUnitBedNumber == s.Key.HousingUnitBedNumber
                                            && a.HousingUnitBedLocation != null),
                          ClassifyConflictCheck = true,
                          HousingClassifyString = s.Where(w => w.HousingUnitClassifyRecString != null)
                              .Select(se => se.HousingUnitClassifyRecString.Split(','))
                              .SelectMany(se => se).Distinct().ToList(),
                          HousingFlagString = s.Where(se => !string.IsNullOrEmpty(se.HousingUnitFlagString))
                              .Select(se => se.HousingUnitFlagString).FirstOrDefault(),
                          HousingAttributeString = s.Where(se => se.HousingUnitAttributeString != "")
                            .Select(se => se.HousingUnitAttributeString).FirstOrDefault()
                      }).ToList();

                    //To get HousingUnt Location details
                    housing.BunkCapacityList = listHousingUnit.Where(w => w.HousingUnitBedLocation != null)
                        .OrderBy(o => o.HousingUnitBedLocation).ThenBy(t => t.HousingUnitBedNumber)
                        .GroupBy(g => new { g.HousingUnitNumber, g.HousingUnitBedNumber, g.HousingUnitBedLocation })
                        .Select(s => new HousingCapacityVm
                        {
                            FacilityId = value.FacilityId,
                            HousingLocation = value.HousingLocation,
                            HousingNumber = s.Key.HousingUnitNumber,
                            HousingUnitListId = value.HousingUnitListId,
                            HousingBedNumber = s.Key.HousingUnitBedNumber,
                            HousingBedLocation = s.Key.HousingUnitBedLocation,
                            CurrentCapacity = s.Sum(c => c.HousingUnitActualCapacity ?? 0),
                            OutofService = s.Sum(c => c.HousingUnitOutOfService ?? 0),
                            ClassifyConflictCheck = true,
                            HousingClassifyString = s.Where(w => w.HousingUnitClassifyRecString != null)
                                .Select(se => se.HousingUnitClassifyRecString.Split(','))
                                .SelectMany(se => se).Distinct().ToList(),
                            HousingFlagString = s.Where(se => !string.IsNullOrEmpty(se.HousingUnitFlagString))
                                .Select(se => se.HousingUnitFlagString).FirstOrDefault(),
                            HousingAttributeString = s.Where(se => se.HousingUnitAttributeString != "")
                            .Select(se => se.HousingUnitAttributeString).FirstOrDefault()
                        }).ToList();

                    //To get HousingUnit details
                    _listHousingUnit = listHousingUnit
                        .Where(w => w.HousingUnitBedNumber == value.HousingBedNumber
                                    && w.HousingUnitBedLocation == value.HousingBedLocation
                                    && w.HousingUnitListId == value.HousingUnitListId).ToList();

                    //housing.InmateDetailsList = GetInmateList(value.FacilityId, true).ToList();

                    housing.HousingConflictList = value.InmateId > 0 ? _housingConflictService.GetInmateHousingConflictVm(value)
                        : _housingConflictService.GetHousingConflictVm(value);
                    housing.HousingAttribute = GetHousingAttribute();
                    break;
            }

            HousingLockInputVm podLockInputVm = new HousingLockInputVm
            {
                HousingType = HousingType.BedNumber,
                FacilityId = value.FacilityId,
                HousingLocation = value.HousingLocation,
                HousingUnitListId = value.HousingUnitListId
            };
            housing.HousingLockdownList = _lockdownService.GetHousingLockdownNotifications(podLockInputVm).ToList();
            housing.TotalSepDetails = totalSepDetails;
            if (_context.SiteOptions.Any(w => w.SiteOptionsStatus == "1"
                                              && w.SiteOptionsName == SiteOptionsConstants.FORCETRANSFERDURINGFACILITYMOVE
                                              && w.SiteOptionsValue == SiteOptionsConstants.ON))
            {
                housing.ShowFacility = true;
            }
            if (value.InmateId > 0)
            {
                housing.InmateCurrentDetails = GetInmateCurrentDetails(value.InmateId);
            }

            //To get Housing Stats details
            housing.HousingStatsDetails = LoadHousingStatsCount(_inmateList);
           
            return housing;
        }

        public bool GetHousingNextLevel(int housingUnitListId)
        {
            bool housingNextLevel = _context.HousingUnit.Any(w => w.HousingUnitListId == housingUnitListId
            && w.HousingUnitBedNumber == null);

            return housingNextLevel;
        }

        public HousingVm GetHousingNumberDetails(HousingInputVm value)
        {
            HousingVm housing = new HousingVm();
            List<InmateSearchVm> totalSepDetails = new List<InmateSearchVm>();
            //To get HousingUnit details
            IEnumerable<HousingUnit> listHousingUnit = _context.HousingUnit
                .Where(w => w.FacilityId == value.FacilityId && (w.HousingUnitInactive == 0 || !w.HousingUnitInactive.HasValue));

            //To get inmate details
            _inmateList = _context.Inmate
                .Where(w => w.InmateActive == 1
                            && w.FacilityId == value.FacilityId);
            // To get the person flag list
            IQueryable<PersonFlag> personFlag = _context.PersonFlag
                .Where(w => w.Person.Inmate.Any(s => s.InmateActive == 1) && w.DeleteFlag == 0
                && (w.InmateFlagIndex > 0 || w.PersonFlagIndex > 0));

            _inmateList = _inmateList
                 .Where(w => w.HousingUnitId.HasValue
                             && w.HousingUnit.HousingUnitLocation == value.HousingLocation);

            _listHousingUnit = listHousingUnit
                        .Where(w => w.HousingUnitLocation == value.HousingLocation).ToList();

            housing.InmateDetailsList = GetInmateList(value.FacilityId, true).ToList();

            //To get HousingUnt Location details
            housing.HousingCapacityList = _listHousingUnit
             .OrderBy(o => o.HousingUnitNumber)
             .GroupBy(x => new { x.HousingUnitListId, x.HousingUnitNumber })
             .Select(s => new HousingCapacityVm
             {
                 FacilityId = value.FacilityId,
                 HousingLocation = value.HousingLocation,
                 HousingNumber = s.Key.HousingUnitNumber,
                 HousingUnitListId = s.Key.HousingUnitListId,
                 CurrentCapacity = s.Sum(c => c.HousingUnitActualCapacity ?? 0),
                 OutofService = s.Sum(c => c.HousingUnitOutOfService ?? 0),
                 HavingNextLevel = s.Any(a => a.HousingUnitLocation == value.HousingLocation
                               && a.HousingUnitNumber == s.Key.HousingUnitNumber
                               && a.HousingUnitBedNumber != null),
                 ClassifyConflictCheck = true,
                 HousingClassifyString = s.Where(w => w.HousingUnitClassifyRecString != null)
                     .Select(se => se.HousingUnitClassifyRecString.Split(','))
                     .SelectMany(se => se).Distinct().ToList(),
                 HousingFlagString = s.Where(se => !string.IsNullOrEmpty(se.HousingUnitFlagString))
                     .Select(se => se.HousingUnitFlagString).FirstOrDefault(),
                 HousingAttributeString = s.Where(se => se.HousingUnitAttributeString != "")
                    .Select(se => se.HousingUnitAttributeString).FirstOrDefault()
             }).ToList();

            housing.HousingFlag = GetHousingFlagList();
            housing.HousingGenderDetails = LoadHousingGender(value.InmateId, value.FacilityId)
                .Where(w => w.HousingDetail.HousingUnitLocation == value.HousingLocation).ToList();


            // Total separation housing conflict
            totalSepDetails.AddRange(GetTotalSeparation(personFlag.Where(
                w => w.Person.Inmate.Any(
                    a => a.FacilityId == value.FacilityId
                        && a.HousingUnit.HousingUnitLocation == value.HousingLocation)).ToList(), 1));
            if (value.InmateId > 0)
            {
                // Total separation inmate conflict
                totalSepDetails.AddRange(GetTotalSeparation(personFlag.Where(w => w.Person.Inmate.Any(a =>
                    a.InmateId == value.InmateId)).ToList(), 2)
                    .Where(w => w.HousingDetail.HousingUnitLocation == value.HousingLocation));
            }
            if (_listHousingUnit.All(a => a.HousingUnitBedNumber == null))
            {
                housing.HousingAttribute = GetHousingAttribute();
            }

            if (value.InmateId > 0 && _listHousingUnit.All(a => a.HousingUnitListId==value.HousingUnitListId && a.HousingUnitBedNumber == null))
            {
                //To get Keep Separate details
                housing.HousingConflictList = _housingConflictService.GetInmateHousingConflictVm(value);
            }
            else if (_listHousingUnit.All(a => a.HousingUnitListId == value.HousingUnitListId && a.HousingUnitBedNumber == null))
            {
                //To get Keep Separate details
                housing.HousingConflictList = _housingConflictService.GetHousingConflictVm(value);
            }

            housing.TotalSepDetails = totalSepDetails;
            if (value.InmateId > 0)
            {
                housing.InmateCurrentDetails = GetInmateCurrentDetails(value.InmateId);
            }
           
            housing.HousingAttribute =  _listHousingUnit.Where(w=>w.HousingUnitListId==value.HousingUnitListId).Select(s => new HousingAttributeVm
            {
                AttributeString = s.HousingUnitAttributeString,
                DisplayNote = s.HousingUnitDisplayNote
            }).FirstOrDefault();

            //To get Housing Stats details
            housing.HousingStatsDetails = LoadHousingStatsCount(_inmateList.Where(w=>w.HousingUnit.HousingUnitListId==value.HousingUnitListId));
            if (_context.SiteOptions.Any(w => w.SiteOptionsStatus == "1"
                                              && w.SiteOptionsName == SiteOptionsConstants.FORCETRANSFERDURINGFACILITYMOVE
                                              && w.SiteOptionsValue == SiteOptionsConstants.ON))
            {
                housing.ShowFacility = true;
            }
            HousingLockInputVm podLockInputVm = new HousingLockInputVm
            {
                HousingType = HousingType.BedNumber,
                FacilityId = value.FacilityId,
                HousingLocation = value.HousingLocation,
                HousingUnitListId = value.HousingUnitListId
            };
            housing.HousingLockdownList = _lockdownService.GetHousingLockdownNotifications(podLockInputVm).ToList();

            return housing;
        }
    }
}

