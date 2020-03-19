using GenerateTables.Models;
using ServerAPI.Utilities;
using ServerAPI.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ServerAPI.Services
{
    public class HousingStatsService : IHousingStatsService
    {
        // Fields       
        private readonly AAtims _context;
        private IQueryable<Inmate> _inmateList;
        private readonly IPhotosService _photos;

        // Methods
        public HousingStatsService(AAtims context, IPhotosService photosService)
        {
            _context = context;
            _photos = photosService;
        }

        //To get Housing Stats details
        public List<HousingStatsDetails> GetStatsInmateDetails(HousingStatsInputVm value)
        {
            if (value.FacilityId > 0)
                _inmateList = _context.Inmate
                    .Where(w => w.InmateActive == 1 && w.FacilityId == value.FacilityId);
            else
                _inmateList = _context.Inmate.Where(w => w.InmateActive == 1);

            IEnumerable<HousingDetail> listHousingUnit = _context.HousingUnit
                .Where(w => w.FacilityId == value.FacilityId)
                .Select(s => new HousingDetail
                {
                    HousingUnitId = s.HousingUnitId,
                    HousingUnitListId = s.HousingUnitListId,
                    HousingUnitLocation = s.HousingUnitLocation,
                    HousingUnitNumber = s.HousingUnitNumber,
                    HousingUnitBedLocation = s.HousingUnitBedLocation,
                    HousingUnitBedNumber = s.HousingUnitBedNumber
                }).ToList();

            if (value.HousingGroupId > 0)
            {
                List<int> housingUnitListIds = _context.HousingGroupAssign.Where(w => w.HousingGroupId == value.HousingGroupId
                && w.HousingUnitListId.HasValue).Select(s => s.HousingUnitListId ?? 0).ToList();

                _inmateList = _inmateList
                   .Where(w => w.HousingUnitId.HasValue
                               && housingUnitListIds.Contains(w.HousingUnit.HousingUnitListId));
                listHousingUnit = listHousingUnit
                    .Where(w => housingUnitListIds.Contains(w.HousingUnitListId ?? 0));
            }

            switch (value.HousingType)
            {
                case HousingType.HousingLocation:
                    if (value.LocationId == 0)
                    {
                        _inmateList = _inmateList
                            .Where(w => w.HousingUnitId.HasValue
                                        && w.HousingUnit.HousingUnitLocation == value.HousingUnit.HousingUnitLocation);

                        listHousingUnit = listHousingUnit
                            .Where(w => w.HousingUnitLocation == value.HousingUnit.HousingUnitLocation);
                    }
                    else
                    {
                        _inmateList = _inmateList
                            .Where(w => w.InmateCurrentTrackId == value.LocationId);
                    }
                    break;
                case HousingType.Number:
                    if (value.LocationId == 0)
                    {
                        _inmateList = _inmateList
                    .Where(w => w.HousingUnitId.HasValue && w.HousingUnit.HousingUnitListId == value.HousingUnit.HousingUnitListId);

                        listHousingUnit = listHousingUnit
                            .Where(w => w.HousingUnitListId == value.HousingUnit.HousingUnitListId);
                    }
                    else
                    {
                        _inmateList = _inmateList
                            .Where(w =>w.InmateCurrentTrackId == value.LocationId);
                    }
                    break;
                case HousingType.BedNumber:
                    _inmateList = _inmateList
                    .Where(w => w.HousingUnitId.HasValue
                                && w.HousingUnit.HousingUnitBedNumber == value.HousingUnit.HousingUnitBedNumber
                                && w.HousingUnit.HousingUnitListId == value.HousingUnit.HousingUnitListId);

                    listHousingUnit = listHousingUnit
                        .Where(w => w.HousingUnitBedNumber == value.HousingUnit.HousingUnitBedNumber
                                    && w.HousingUnitListId == value.HousingUnit.HousingUnitListId);
                    break;
                case HousingType.BedLocation:
                    _inmateList = _inmateList
                   .Where(w => w.HousingUnitId.HasValue
                               && w.HousingUnit.HousingUnitBedNumber == value.HousingUnit.HousingUnitBedNumber
                               && w.HousingUnit.HousingUnitBedLocation == value.HousingUnit.HousingUnitBedLocation
                               && w.HousingUnit.HousingUnitListId == value.HousingUnit.HousingUnitListId);

                    listHousingUnit = listHousingUnit
                        .Where(w => w.HousingUnitBedNumber == value.HousingUnit.HousingUnitBedNumber
                                    && w.HousingUnitBedLocation == value.HousingUnit.HousingUnitBedLocation
                                    && w.HousingUnitListId == value.HousingUnit.HousingUnitListId);
                    break;
                case HousingType.NoHousing:
                    _inmateList = _inmateList
                    .Where(w => !w.HousingUnitId.HasValue);
                    break;
                case HousingType.Location:
                    _inmateList = _inmateList
                        .Where(w => w.InmateCurrentTrackId == value.LocationId);
                    break;
                case HousingType.Facility:
                    if (value.FacilityId == 0)
                    {
                        _inmateList = _inmateList
                            .Where(w => w.InmateCurrentTrackId == value.LocationId);
                    }
                    break;
            }

            List<HousingStatsDetails> housingStatsList = new List<HousingStatsDetails>();

            switch (value.HousingStatsCount.EventFlag)
            {
                case CellsEventFlag.Flag:
                    // For Flag List
                    housingStatsList = GetFlagDetails(value.HousingStatsCount.FlagId, value.HousingStatsCount.FlagName, value.HousingStatsCount.Type);
                    break;
                case CellsEventFlag.Gender:
                    // For Gender List
                    housingStatsList = GetGenderDetails(value.HousingStatsCount.FlagId);
                    break;
                case CellsEventFlag.Race:
                    // For Race List
                    housingStatsList = GetRaceDetails(value.HousingStatsCount.FlagId);
                    break;
                case CellsEventFlag.Association:
                    // For Association List
                    List<Lookup> lookupslist = _context.Lookup.Where(w => w.LookupType == LookupConstants.CLASSGROUP).ToList();
                    housingStatsList = _context.PersonClassification
                        .SelectMany(p => _inmateList
                                .Where(w => w.PersonId == p.PersonId
                                            && p.PersonClassificationDateFrom <= DateTime.Now
                                            && (!p.PersonClassificationDateThru.HasValue
                                                || p.PersonClassificationDateThru >= DateTime.Now)
                                            && p.PersonClassificationTypeId == value.HousingStatsCount.FlagId),
                            (p, i) => new HousingStatsDetails
                            {
                                FlagId = p.PersonClassificationId,
                                InmateId = i.InmateId,
                                Flags = lookupslist.Single(f => f.LookupIndex == p.PersonClassificationTypeId).LookupDescription,     
                                Location = i.InmateCurrentTrack,
                                HousingUnitId = i.HousingUnitId,
                                PersonId = i.PersonId,
                                InmateNumber = i.InmateNumber,
                                PersonDetail = new PersonInfoVm
                                {
                                    PersonId = i.PersonId,
                                    PersonLastName = i.Person.PersonLastName,
                                    PersonMiddleName = i.Person.PersonMiddleName,
                                    PersonFirstName = i.Person.PersonFirstName
                                }
                            }).ToList();
                    break;
                case CellsEventFlag.Classification:
                    housingStatsList = GetClassifyDetails(value.HousingStatsCount.FlagName);
                    break;
            }

            housingStatsList.ForEach(item =>
            {
                if (item.HousingUnitId > 0)
                    item.HousingDetail = listHousingUnit
                            .SingleOrDefault(w => w.HousingUnitId == item.HousingUnitId);
            });

            return housingStatsList;
        }

        // To get list of inmates based on Alert Flag
        private List<HousingStatsDetails> GetFlagDetails(int flagId, string flagName, string type)
        {
            //To get list of Lookup details
            List<Lookup> lookupList = _context.Lookup
                .Where(l => (l.LookupType == LookupConstants.PERSONCAUTION
                    || l.LookupType == LookupConstants.TRANSCAUTION
                    || l.LookupType == LookupConstants.DIET) && l.LookupInactive == 0).ToList();

            //using ternary operator taking time to load throws exception 
            IEnumerable<HousingStatsDetails> lstPersonflag;
            if (type == LookupConstants.PERSONCAUTION)
            {
                lstPersonflag = _context.PersonFlag
                    .SelectMany(pf => _inmateList
                            .Where(w => w.PersonId == pf.PersonId
                                        && pf.DeleteFlag == 0 &&
                                        pf.PersonFlagIndex == flagId),
                        (pf, inm) => new HousingStatsDetails
                        {
                            PersonFlagIndex = pf.PersonFlagIndex,
                            InmateFlagIndex = pf.InmateFlagIndex,
                            DietFlagIndex = pf.DietFlagIndex,
                            InmateId = inm.InmateId,
                            InmateNumber = inm.InmateNumber,
                            Location = inm.InmateCurrentTrack,
                            HousingUnitId = inm.HousingUnitId,
                            PersonId = pf.PersonId,
                            PersonDetail = new PersonInfoVm
                            {
                                PersonId = inm.PersonId,
                                PersonLastName = inm.Person.PersonLastName,
                                PersonMiddleName = inm.Person.PersonMiddleName,
                                PersonFirstName = inm.Person.PersonFirstName
                            },
                            PhotoFilePath = _photos.GetPhotoByCollectionIdentifier(inm.Person.Identifiers)
                        });
            }
            else if (type == LookupConstants.TRANSCAUTION)
            {
                lstPersonflag = _context.PersonFlag
                    .SelectMany(pf => _inmateList
                            .Where(w => w.PersonId == pf.PersonId
                                        && pf.DeleteFlag == 0
                                        && pf.InmateFlagIndex == flagId),
                        (pf, inm) => new HousingStatsDetails
                        {
                            PersonFlagIndex = pf.PersonFlagIndex,
                            InmateFlagIndex = pf.InmateFlagIndex,
                            DietFlagIndex = pf.DietFlagIndex,
                            InmateId = inm.InmateId,
                            InmateNumber = inm.InmateNumber,
                            Location = inm.InmateCurrentTrack,
                            HousingUnitId = inm.HousingUnitId,
                            PersonId = pf.PersonId,
                            PersonDetail = new PersonInfoVm
                            {
                                PersonId = inm.PersonId,
                                PersonLastName = inm.Person.PersonLastName,
                                PersonMiddleName = inm.Person.PersonMiddleName,
                                PersonFirstName = inm.Person.PersonFirstName
                            },
                            PhotoFilePath = _photos.GetPhotoByCollectionIdentifier(inm.Person.Identifiers)
                        });
            }
            else
            {
                lstPersonflag = _context.PersonFlag
                    .SelectMany(pf => _inmateList
                            .Where(w => w.PersonId == pf.PersonId
                                        && pf.DeleteFlag == 0 &&
                                pf.DietFlagIndex == flagId),
                        (pf, inm) => new HousingStatsDetails
                        {
                            PersonFlagIndex = pf.PersonFlagIndex,
                            InmateFlagIndex = pf.InmateFlagIndex,
                            DietFlagIndex = pf.DietFlagIndex,
                            InmateId = inm.InmateId,
                            InmateNumber = inm.InmateNumber,
                            Location = inm.InmateCurrentTrack,
                            HousingUnitId = inm.HousingUnitId,
                            PersonId = pf.PersonId,
                            PersonDetail = new PersonInfoVm
                            {
                                PersonId = inm.PersonId,
                                PersonLastName = inm.Person.PersonLastName,
                                PersonMiddleName = inm.Person.PersonMiddleName,
                                PersonFirstName = inm.Person.PersonFirstName
                            },
                            PhotoFilePath = _photos.GetPhotoByCollectionIdentifier(inm.Person.Identifiers)
                        });
            }

            List<HousingStatsDetails> housingStatsDetails = lstPersonflag
                .SelectMany(perFlag => lookupList
                        .Where(w => w.LookupIndex == perFlag.PersonFlagIndex
                            && w.LookupType == LookupConstants.PERSONCAUTION &&
                            perFlag.PersonFlagIndex == flagId && w.LookupDescription == flagName),
                    (perFlag, look) => new HousingStatsDetails
                    {
                        FlagId = perFlag.PersonFlagIndex ?? 0,
                        InmateId = perFlag.InmateId,
                        Flags = look.LookupDescription,
                        Location = perFlag.Location,
                        HousingUnitId = perFlag.HousingUnitId,
                        PersonId = perFlag.PersonId,
                        InmateNumber = perFlag.InmateNumber,
                        PersonDetail = perFlag.PersonDetail,
                        PhotoFilePath = perFlag.PhotoFilePath
                    }).ToList();

            housingStatsDetails.AddRange(lstPersonflag.SelectMany(perFlag => lookupList
            .Where(w => w.LookupIndex == perFlag.InmateFlagIndex
                && w.LookupType == LookupConstants.TRANSCAUTION
                && perFlag.InmateFlagIndex == flagId && w.LookupDescription == flagName),
                (perFlag, l) => new HousingStatsDetails
                {
                    FlagId = perFlag.InmateFlagIndex ?? 0,
                    InmateId = perFlag.InmateId,
                    Flags = l.LookupDescription,
                    Location = perFlag.Location,
                    HousingUnitId = perFlag.HousingUnitId,
                    PersonId = perFlag.PersonId,
                    InmateNumber = perFlag.InmateNumber,
                    PersonDetail = perFlag.PersonDetail,
                    PhotoFilePath = perFlag.PhotoFilePath
                }));

            housingStatsDetails.AddRange(lstPersonflag.SelectMany(perFlag => lookupList
                .Where(w => w.LookupIndex == perFlag.DietFlagIndex
                    && w.LookupType == LookupConstants.DIET
                    && perFlag.DietFlagIndex == flagId && w.LookupDescription == flagName),
                (perFlag, l) => new HousingStatsDetails
                {
                    FlagId = perFlag.DietFlagIndex ?? 0,
                    InmateId = perFlag.InmateId,
                    Flags = l.LookupDescription,
                    Location = perFlag.Location,
                    HousingUnitId = perFlag.HousingUnitId,
                    PersonId = perFlag.PersonId,
                    InmateNumber = perFlag.InmateNumber,
                    PersonDetail = perFlag.PersonDetail,
                    PhotoFilePath = perFlag.PhotoFilePath
                }));
            return housingStatsDetails;
        }

        // To get list of inmates based on Gender Flag
        private List<HousingStatsDetails> GetGenderDetails(int flagId)
        {
            List<HousingStatsDetails> housingStatsList;

            if (flagId > 0)
            {
                //To get list of Lookup details
                List<Lookup> lookupList = _context.Lookup
                    .Where(l => l.LookupType == LookupConstants.SEX).ToList();

                housingStatsList = _inmateList.SelectMany(inm => lookupList
                    .Where(w => inm.Person.PersonSexLast.HasValue
                        && w.LookupIndex == inm.Person.PersonSexLast
                        && inm.Person.PersonSexLast == flagId),
                    (inm, l) => new HousingStatsDetails
                    {
                        FlagId = l.LookupIndex,
                        InmateId = inm.InmateId,
                        Flags = l.LookupDescription,
                        Location = inm.InmateCurrentTrack,
                        HousingUnitId = inm.HousingUnitId,
                        PersonId = inm.PersonId,
                        InmateNumber = inm.InmateNumber,
                        PersonDetail = new PersonInfoVm
                        {
                            PersonId = inm.PersonId,
                            PersonLastName = inm.Person.PersonLastName,
                            PersonMiddleName = inm.Person.PersonMiddleName,
                            PersonFirstName = inm.Person.PersonFirstName
                        },
                        PhotoFilePath = _photos.GetPhotoByCollectionIdentifier(inm.Person.Identifiers)
                    }).ToList();
            }
            else
            {
                housingStatsList = _inmateList
                    .Where(w => !w.Person.PersonSexLast.HasValue
                                || w.Person.PersonSexLast == 0)
                    .Select(s => new HousingStatsDetails
                    {
                        FlagId = s.Person.PersonSexLast ?? 0,
                        InmateId = s.InmateId,
                        Location = s.InmateCurrentTrack,
                        HousingUnitId = s.HousingUnitId,
                        PersonId = s.PersonId,
                        InmateNumber = s.InmateNumber,
                        PersonDetail = new PersonInfoVm
                        {
                            PersonId = s.PersonId,
                            PersonLastName = s.Person.PersonLastName,
                            PersonMiddleName = s.Person.PersonMiddleName,
                            PersonFirstName = s.Person.PersonFirstName
                        },
                        PhotoFilePath = _photos.GetPhotoByCollectionIdentifier(s.Person.Identifiers)
                    }).ToList();
            }

            return housingStatsList;
        }

        // To get list of inmates based on Race Flag
        private List<HousingStatsDetails> GetRaceDetails(int flagId)
        {
            List<HousingStatsDetails> housingStatsList;
            if (flagId > 0)
            {
                //To get list of Lookup details
                List<Lookup> lookupList = _context.Lookup
                    .Where(l => l.LookupType == LookupConstants.RACE).ToList();

                housingStatsList = _inmateList.SelectMany(inm => lookupList
                        .Where(w => w.LookupIndex == inm.Person.PersonRaceLast
                         && inm.Person.PersonRaceLast == flagId),
                    (inm, l) => new HousingStatsDetails
                    {
                        FlagId = l.LookupIndex,
                        InmateId = inm.InmateId,
                        Flags = l.LookupDescription,
                        Location = inm.InmateCurrentTrack,
                        HousingUnitId = inm.HousingUnitId,
                        PersonId = inm.PersonId,
                        InmateNumber = inm.InmateNumber,
                        PersonDetail = new PersonInfoVm
                        {
                            PersonId = inm.PersonId,
                            PersonLastName = inm.Person.PersonLastName,
                            PersonMiddleName = inm.Person.PersonMiddleName,
                            PersonFirstName = inm.Person.PersonFirstName
                        },
                        PhotoFilePath = _photos.GetPhotoByCollectionIdentifier(inm.Person.Identifiers)
                    }).ToList();
            }
            else
            {
                housingStatsList = _inmateList
                    .Where(w => !w.Person.PersonRaceLast.HasValue
                     || w.Person.PersonRaceLast == 0)
                    .Select(s => new HousingStatsDetails
                    {
                        FlagId = s.Person.PersonRaceLast ?? 0,
                        InmateId = s.InmateId,
                        Location = s.InmateCurrentTrack,
                        HousingUnitId = s.HousingUnitId,
                        PersonId = s.PersonId,
                        InmateNumber = s.InmateNumber,
                        PersonDetail = new PersonInfoVm
                        {
                            PersonId = s.PersonId,
                            PersonLastName = s.Person.PersonLastName,
                            PersonMiddleName = s.Person.PersonMiddleName,
                            PersonFirstName = s.Person.PersonFirstName
                        },
                        PhotoFilePath = _photos.GetPhotoByCollectionIdentifier(s.Person.Identifiers)
                    }).ToList();
            }
            return housingStatsList;
        }

        // To get list of inmates based on Classify Flag
        private List<HousingStatsDetails> GetClassifyDetails(string flagName)
        {
            List<HousingStatsDetails> housingStatsList;

            if (!(flagName is null))
            {
                // For Classification List
                housingStatsList = _context.InmateClassification.SelectMany(ic => _inmateList
                    .Where(w => w.InmateClassificationId == ic.InmateClassificationId
                    && ic.InmateClassificationReason == flagName),
                    (ic, inm) => new HousingStatsDetails
                    {
                        FlagId = ic.InmateClassificationId,
                        Flags = ic.InmateClassificationReason,
                        InmateId = inm.InmateId,
                        Location = inm.InmateCurrentTrack,
                        HousingUnitId = inm.HousingUnitId,
                        PersonId = inm.PersonId,
                        InmateNumber = inm.InmateNumber,
                        PersonDetail = new PersonInfoVm
                        {
                            PersonId = inm.PersonId,
                            PersonLastName = inm.Person.PersonLastName,
                            PersonMiddleName = inm.Person.PersonMiddleName,
                            PersonFirstName = inm.Person.PersonFirstName
                        },
                        PhotoFilePath = _photos.GetPhotoByCollectionIdentifier(inm.Person.Identifiers)
                    }).ToList();
            }
            else
            {
                housingStatsList = _inmateList
                    .Where(w => !w.InmateClassificationId.HasValue)
                    .Select(inm => new HousingStatsDetails
                    {
                        InmateId = inm.InmateId,
                        Location = inm.InmateCurrentTrack,
                        HousingUnitId = inm.HousingUnitId,
                        PersonId = inm.PersonId,
                        InmateNumber = inm.InmateNumber,
                        PersonDetail = new PersonInfoVm
                        {
                            PersonId = inm.PersonId,
                            PersonLastName = inm.Person.PersonLastName,
                            PersonMiddleName = inm.Person.PersonMiddleName,
                            PersonFirstName = inm.Person.PersonFirstName
                        },
                        PhotoFilePath = _photos.GetPhotoByCollectionIdentifier(inm.Person.Identifiers)
                    }).ToList();
            }
            return housingStatsList;
        }

    }
}
