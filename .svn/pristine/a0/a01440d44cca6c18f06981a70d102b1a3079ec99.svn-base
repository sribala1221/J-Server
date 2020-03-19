using System;
using System.Collections.Generic;
using System.Linq;
using GenerateTables.Models;
using ServerAPI.ViewModels;
using ServerAPI.Utilities;

// ReSharper disable once CheckNamespace
namespace ServerAPI.Services
{
    // ReSharper disable once UnusedMember.Global
    public class SearchInmateChargeService : ISearchInmateChargeService
    {
        private readonly AAtims _context;

        // ReSharper disable once NotAccessedField.Local
        private readonly IPhotosService _photos;

        public SearchInmateChargeService(AAtims context, IPhotosService photosService)
        {
            _context = context;
            _photos = photosService;
        }

        // Get Intake Search Details based on requested information
        public List<SearchResult> GetBookingSearchList(SearchRequestVm searchDetails)
        {
            var externalPath = _photos.GetExternalPath();
            var path = _photos.GetPath();
            bool isToSearchDob = searchDetails.DateOfBirth.HasValue &&
                                 searchDetails.DateOfBirth > DateTime.MinValue;

            List<Lookup> lstLookupDetails = _context.Lookup
                .Where(lkp =>
                    (lkp.LookupType == LookupConstants.ARRTYPE || lkp.LookupType == LookupConstants.CRIMETYPE)
                    && !string.IsNullOrEmpty(lkp.LookupDescription))
                .Select(lkp => new Lookup
                {
                    LookupDescription = lkp.LookupDescription,
                    LookupIndex = lkp.LookupIndex,
                    LookupType = lkp.LookupType
                }).Distinct().ToList();

            List<SearchResult> personDetailsLst = _context.Crime.Where(cr =>
                    (searchDetails.FacilityId == 0 || cr.Arrest.Inmate.FacilityId == searchDetails.FacilityId)
                    && (!searchDetails.ActiveOnly || cr.Arrest.Inmate.InmateActive == 1)
                    && (string.IsNullOrEmpty(searchDetails.bunkId)
                        || cr.Arrest.Inmate.HousingUnitId > 0
                        && cr.Arrest.Inmate.HousingUnit.HousingUnitBedLocation == searchDetails.bunkId
                        && (!cr.Arrest.Inmate.HousingUnit.HousingUnitInactive.HasValue ||
                            cr.Arrest.Inmate.HousingUnit.HousingUnitInactive == 0))
                    && (string.IsNullOrEmpty(searchDetails.cellId)
                        || cr.Arrest.Inmate.HousingUnitId > 0
                        && cr.Arrest.Inmate.HousingUnit.HousingUnitBedNumber == searchDetails.cellId
                        && (!cr.Arrest.Inmate.HousingUnit.HousingUnitInactive.HasValue ||
                            cr.Arrest.Inmate.HousingUnit.HousingUnitInactive == 0))
                    && (searchDetails.podId == 0
                        || cr.Arrest.Inmate.HousingUnitId > 0
                        && cr.Arrest.Inmate.HousingUnit.HousingUnitListId == searchDetails.podId
                        && (!cr.Arrest.Inmate.HousingUnit.HousingUnitInactive.HasValue ||
                            cr.Arrest.Inmate.HousingUnit.HousingUnitInactive == 0))
                    && (string.IsNullOrEmpty(searchDetails.BuildingId)
                        || cr.Arrest.Inmate.HousingUnitId > 0
                        && cr.Arrest.Inmate.HousingUnit.HousingUnitLocation == searchDetails.BuildingId
                        && (!cr.Arrest.Inmate.HousingUnit.HousingUnitInactive.HasValue ||
                            cr.Arrest.Inmate.HousingUnit.HousingUnitInactive == 0))
                    && (searchDetails.InmateSearchFacilityId == 0
                        || (cr.Arrest.Inmate.FacilityId == searchDetails.InmateSearchFacilityId))
                    && (string.IsNullOrEmpty(searchDetails.classificationId)
                        || cr.Arrest.Inmate.InmateClassification.InmateClassificationReason == searchDetails.classificationId)
                    && (searchDetails.locationId == 0
                        || (cr.Arrest.Inmate.InmateCurrentTrackId == searchDetails.locationId))
                    && (searchDetails.personFlagId == 0
                        || cr.Arrest.Inmate.Person.PersonFlag.Any(pf =>
                            pf.PersonFlagIndex == searchDetails.personFlagId && pf.DeleteFlag == 0))
                    && (searchDetails.inmateFlagId == 0
                        || cr.Arrest.Inmate.Person.PersonFlag.Any(pf =>
                            pf.InmateFlagIndex == searchDetails.inmateFlagId && pf.DeleteFlag == 0))
                    && (searchDetails.medFlagId == 0
                        || cr.Arrest.Inmate.Person.PersonFlag.Any(
                            pf => pf.MedicalFlagIndex == searchDetails.medFlagId && pf.DeleteFlag == 0))
                            && (searchDetails.dietFlagId == 0
                        || cr.Arrest.Inmate.Person.PersonFlag.Any(
                            pf => pf.DietFlagIndex == searchDetails.dietFlagId && pf.DeleteFlag == 0))
                    && (string.IsNullOrEmpty(searchDetails.InmateNumber)
                        || cr.Arrest.Inmate.InmateNumber.StartsWith(searchDetails.InmateNumber)
                        || cr.Arrest.Inmate.Person.Aka.Any(p =>
                            p.AkaInmateNumber.StartsWith(searchDetails.InmateNumber)))
                    && (string.IsNullOrEmpty(searchDetails.InmateSiteNumber)
                        || cr.Arrest.Inmate.InmateSiteNumber.StartsWith(searchDetails.InmateSiteNumber)
                        || cr.Arrest.Inmate.Person.Aka.Any(
                            p => p.AkaSiteInmateNumber.StartsWith(searchDetails.InmateSiteNumber)))
                    && (!searchDetails.IncarcerationSearch
                        || !searchDetails.ActiveOnly
                        || cr.Arrest.Inmate.Incarceration.Any(inc =>
                            (!searchDetails.activeBookingOnly ||
                             !inc.ReleaseOut.HasValue)
                            && ((!searchDetails.dateSearchFrom.HasValue &&
                                 !searchDetails.dateSearchTo.HasValue)
                                || inc.DateIn >=
                                searchDetails.dateSearchFrom &&
                                inc.DateIn <= searchDetails.dateSearchTo)
                            && ((!searchDetails.dateReleaseFrom.HasValue &&
                                 !searchDetails.dateReleaseTo.HasValue)
                                || inc.ReleaseOut >=
                                searchDetails.dateReleaseFrom &&
                                inc.ReleaseOut <= searchDetails.dateReleaseTo)
                            && (searchDetails.ClearByOfficer == 0 ||
                                inc.ReleaseClearBy ==
                                searchDetails.ClearByOfficer)
                            && (searchDetails.IntakeOfficer == 0 ||
                                inc.InOfficerId ==
                                searchDetails.IntakeOfficer)))
                    && (string.IsNullOrEmpty(searchDetails.Moniker)
                        || cr.Arrest.Inmate.Person.Aka.Any(aka => aka.PersonGangName.StartsWith(searchDetails.Moniker))
                    )
                    && (string.IsNullOrEmpty(searchDetails.FirstName)
                        || (cr.Arrest.Inmate.Person.PersonFirstName.StartsWith(searchDetails.FirstName)
                            || cr.Arrest.Inmate.Person.Aka.Any(aka =>
                                aka.AkaFirstName.StartsWith(searchDetails.FirstName))))
                    && (string.IsNullOrEmpty(searchDetails.LastName)
                        || (cr.Arrest.Inmate.Person.PersonLastName.StartsWith(searchDetails.LastName)
                            || cr.Arrest.Inmate.Person.Aka.Any(
                                aka => aka.AkaLastName.StartsWith(searchDetails.LastName))))
                    && (string.IsNullOrEmpty(searchDetails.MiddleName)
                        || (cr.Arrest.Inmate.Person.PersonMiddleName.StartsWith(searchDetails.MiddleName)
                            || cr.Arrest.Inmate.Person.Aka.Any(aka =>
                                aka.AkaMiddleName.StartsWith(searchDetails.MiddleName))))
                    && (string.IsNullOrEmpty(searchDetails.AfisNumber)
                        || (cr.Arrest.Inmate.Person.AfisNumber.StartsWith(searchDetails.AfisNumber)
                            || cr.Arrest.Inmate.Person.Aka.Any(aka =>
                                aka.AkaAfisNumber.StartsWith(searchDetails.AfisNumber))))
                    && (!isToSearchDob
                        || (cr.Arrest.Inmate.Person.PersonDob.Value.Date == searchDetails.DateOfBirth.Value.Date
                            || cr.Arrest.Inmate.Person.Aka.Any(aka =>
                                aka.AkaDob.Value.Date == searchDetails.DateOfBirth.Value.Date)))
                    // binding arrest details
                    && (!searchDetails.IsArrestSearch || cr.Arrest.ArrestActive == 1)
                    && (searchDetails.IsArrestSearch
                        || !searchDetails.ActiveOnly
                        || !searchDetails.IsBookingSearch
                        || cr.Arrest.ArrestActive == 1)
                    && (!searchDetails.activeCasesOnly
                        || cr.Arrest.IncarcerationArrestXref.Any(iax =>
                            !iax.ReleaseDate.HasValue))
                    && (string.IsNullOrEmpty(
                            searchDetails.BookingNumber)
                        || cr.Arrest.ArrestBookingNo.Contains(searchDetails
                            .BookingNumber))
                    && (string.IsNullOrEmpty(searchDetails.CourtDocket)
                        || cr.Arrest.ArrestCourtDocket.Contains(searchDetails
                            .CourtDocket))
                    && (searchDetails.BookingType == 0
                        || cr.Arrest.ArrestType == searchDetails.BookingType
                            .ToString())
                    && (searchDetails.BillingAgency == 0
                        || cr.Arrest.ArrestBillingAgencyId ==
                        searchDetails.BillingAgency)
                    && (searchDetails.Court == 0
                        || cr.Arrest.ArrestCourtJurisdictionId ==
                        searchDetails.Court)
                    && (searchDetails.ArrestingAgency == 0
                        || cr.Arrest.ArrestingAgencyId ==
                        searchDetails.ArrestingAgency)
                    && (searchDetails.ArrestingOfficer == 0
                        || cr.Arrest.ArrestOfficerId ==
                        searchDetails.ArrestingOfficer)
                    && (searchDetails.arrestingOfficerName == 0
                        || cr.Arrest.ArrestOfficerId ==
                        searchDetails.arrestingOfficerName)
                    && (searchDetails.BookingOfficer == 0
                        || cr.Arrest.ArrestBookingOfficerId ==
                        searchDetails.BookingOfficer)
                    && (string.IsNullOrEmpty(searchDetails.bailType)
                        || cr.Arrest.BailType == searchDetails.bailType)
                    && (!searchDetails.noBail || cr.Arrest.BailNoBailFlag == 0)
                    && (searchDetails.arrestConvictionStatus == 0
                        || cr.Arrest.ArrestBookingStatus ==
                        searchDetails.arrestConvictionStatus)
                    && (string.IsNullOrEmpty(searchDetails
                            .lawEnforcementDispositionId)
                        || cr.Arrest.ArrestLawEnforcementDisposition ==
                        searchDetails.lawEnforcementDispositionId)
                    && (searchDetails.sentCode == 0
                        || cr.Arrest.ArrestSentenceCode ==
                        searchDetails.sentCode)
                    && (searchDetails.sentMethod == 0
                        || cr.Arrest.ArrestSentenceMethodId ==
                        searchDetails.sentMethod)
                    && (searchDetails.bailFrom == 0
                        || cr.Arrest.BailAmount >= searchDetails.bailFrom &&
                        cr.Arrest.BailAmount <= searchDetails.bailTo)
                    && (searchDetails.sentDaysFrom == 0
                        || cr.Arrest.ArrestSentenceActualDaysToServe >=
                        searchDetails.sentDaysFrom
                        && cr.Arrest.ArrestSentenceActualDaysToServe <=
                        searchDetails.sentDaysTo)
                    && (string.IsNullOrEmpty(
                            searchDetails.WarrantNumber)
                        || cr.Arrest.Warrant.Any(war =>
                            war.WarrantNumber.StartsWith(searchDetails
                                .WarrantNumber)))
                    && (string.IsNullOrEmpty(searchDetails.warrentType)
                        || cr.Arrest.Warrant.Any(war =>
                            war.WarrantChargeType ==
                            searchDetails.warrentType))
                    && (searchDetails.jurisdictionId == 0
                        || cr.Arrest.Warrant.Any(war =>
                            war.WarrantAgencyId ==
                            searchDetails.jurisdictionId))
                    && (!searchDetails.freeFormJus
                        || cr.Arrest.Warrant.Any(
                            war => war.WarrantCounty != ""))
                    && (string.IsNullOrEmpty(searchDetails.clearReason)
                        || cr.Arrest.IncarcerationArrestXref.Any(iax =>
                            iax.ArrestId > 0
                            && iax.ReleaseReason.ToUpper() ==
                            searchDetails.clearReason.ToUpper()))
                    && (string.IsNullOrEmpty(searchDetails.ChargeType)
                        || cr.CrimeLookup.CrimeCodeType.ToUpper() ==
                                                      searchDetails.ChargeType.ToUpper())
                    && (searchDetails.ChargeGroup == 0
                        || cr.CrimeLookup.CrimeGroupId == searchDetails.ChargeGroup)
                    && (string.IsNullOrEmpty(searchDetails.ChargeSection)
                        || cr.CrimeLookup.CrimeSection.ToUpper()
                            .Contains(searchDetails.ChargeSection.ToUpper()))
                    && (string.IsNullOrEmpty(searchDetails.ChargeDescription)
                        || cr.CrimeLookup.CrimeDescription.ToUpper()
                            .Contains(searchDetails.ChargeDescription.ToUpper()))
                    && (string.IsNullOrEmpty(searchDetails.bookingCaseType)
                        || cr.CrimeLookup.CrimeStatuteCode == searchDetails.bookingCaseType)
                    && (string.IsNullOrEmpty(searchDetails.CrimeType)
                        || cr.CrimeType == searchDetails.CrimeType)
                    && (string.IsNullOrEmpty(searchDetails.Classify)
                        || cr.Arrest.Inmate.InmateClassification.InmateClassificationReason == searchDetails.Classify)
                ).Take(10000)
                .Select(cr => new SearchResult
                {
                    ArrestId = cr.ArrestId ?? 0,
                    BookingDate = cr.Arrest.IncarcerationArrestXref.Select(s => s.BookingDate).FirstOrDefault(),
                    BookingNumber = cr.Arrest.ArrestBookingNo,
                    CourtDocket = cr.Arrest.ArrestCourtDocket,
                    ReleaseDate = cr.Arrest.IncarcerationArrestXref.Select(s => s.ReleaseDate).FirstOrDefault(),
                    ReleaseReason = cr.Arrest.IncarcerationArrestXref.Select(s => s.ReleaseReason).FirstOrDefault(),
                    IncarArrestXrefId = cr.Arrest.IncarcerationArrestXref.Select(s => s.IncarcerationArrestXrefId)
                        .FirstOrDefault(),
                    IncarcerationId = cr.Arrest.IncarcerationArrestXref.Select(s => s.IncarcerationId)
                                          .FirstOrDefault() ?? 0,
                    InmateActive = cr.Arrest.IncarcerationArrestXref.Select(s => s.Incarceration.Inmate.InmateActive)
                        .FirstOrDefault(),
                    InmateNumber = cr.Arrest.IncarcerationArrestXref.Select(s => s.Incarceration.Inmate.InmateNumber)
                        .FirstOrDefault(),
                    IncarbookingNumber = cr.Arrest.IncarcerationArrestXref.Select(s => s.Incarceration.BookingNo)
                        .FirstOrDefault(),
                    ArrestReleaseClearedDate = cr.Arrest.ArrestReleaseClearedDate,
                    ArrestSupSeqNumber = cr.Arrest.ArrestSupSeqNumber,
                    WarrantNumber = cr.Warrant.WarrantNumber,
                    CrimeType = cr.CrimeType,
                    CrimeCodeType = cr.CrimeLookup.CrimeCodeType,
                    CrimeSection = cr.CrimeLookup.CrimeSection,
                    CrimeStatuteCode = cr.CrimeLookup.CrimeStatuteCode,
                    CrimeDescription = cr.CrimeLookup.CrimeDescription,
                    BookingType = lstLookupDetails.FirstOrDefault(
                        lkp => lkp.LookupType == LookupConstants.ARRTYPE
                               && !string.IsNullOrEmpty(cr.Arrest.ArrestType)
                               && lkp.LookupIndex.ToString() == cr.Arrest.ArrestType.Trim()).LookupDescription,
                    PersonId = cr.Arrest.IncarcerationArrestXref.Select(s => s.Incarceration.Inmate.PersonId)
                        .FirstOrDefault(),
                    InmateId =
                        cr.Arrest.IncarcerationArrestXref.Select(s => s.Incarceration.InmateId).FirstOrDefault() ?? 0,
                    ArrestType = cr.Arrest.ArrestType,
                    ArrestBillingAgencyId = cr.Arrest.ArrestBillingAgencyId,
                    ArrestCourtJurisdictionId = cr.Arrest.ArrestCourtJurisdictionId,
                    FirstName = cr.Arrest.IncarcerationArrestXref
                        .Select(s => s.Incarceration.Inmate.Person.PersonFirstName).FirstOrDefault(),
                    MiddleName = cr.Arrest.IncarcerationArrestXref
                        .Select(s => s.Incarceration.Inmate.Person.PersonMiddleName).FirstOrDefault(),
                    LastName = cr.Arrest.IncarcerationArrestXref
                        .Select(s => s.Incarceration.Inmate.Person.PersonLastName).FirstOrDefault(),
                    ChargeStatus = lstLookupDetails.SingleOrDefault(lkp =>
                        lkp.LookupType == LookupConstants.CRIMETYPE
                        && !string.IsNullOrEmpty(cr.CrimeType)
                        && lkp.LookupIndex.ToString() == cr.CrimeType.Trim()).LookupDescription,
                    // below code commented due to performance issue
                    // Photofilepath = _context.Identifiers.Where(idf => 
                    //     idf.IdentifierType == "1" 
                    //     && idf.DeleteFlag == 0 
                    //     && idf.PersonId == cr.Arrest.IncarcerationArrestXref.Select(s => s.Incarceration.Inmate.PersonId).FirstOrDefault())
                    //     .Select(x =>
                    //         x.PhotographRelativePath == null
                    //             ? externalPath + x.PhotographPath
                    //             : path + x.PhotographRelativePath).FirstOrDefault()
                    //Photofilepath = cr.Arrest.IncarcerationArrestXref
                    //    .Select(inc => inc.Incarceration).FirstOrDefault()
                    //    .Inmate.Person.Identifiers.Where(idf => idf.IdentifierType == "1" && idf.DeleteFlag == 0)
                    //    .Select(x =>
                    //        x.PhotographRelativePath == null
                    //            ? externalPath + x.PhotographPath
                    //            : path + x.PhotographRelativePath).FirstOrDefault()

                    //Photofilepath = _photos.GetPhotoByIdentifier(cr.Arrest.IncarcerationArrestXref.Select(s =>
                    //        s.Incarceration.Inmate.Person.Identifiers.LastOrDefault(idn =>
                    //            idn.IdentifierType == "1" && idn.DeleteFlag == 0)
                    //    ).FirstOrDefault())
                }).ToList();

            //List<Identifiers> identifierLst = _context.Identifiers
            //    .Where(idf => idf.IdentifierType == "1"
            //                  && idf.DeleteFlag == 0
            //                  && _personDetailsLst.Select(a => a.PersonId).Contains(idf.PersonId ?? 0))
            //    .Select(idf => new Identifiers
            //    {
            //        PersonId = idf.PersonId,
            //        IdentifierId = idf.IdentifierId,
            //        PhotographRelativePath = _photos.GetPhotoByIdentifier(idf)
            //    }).OrderByDescending(o => o.IdentifierId).ToList();

            ////Booking Result
            //Parallel.ForEach(_personDetailsLst, item =>
            //{
            //    item.Photofilepath =
            //        identifierLst.FirstOrDefault(idn => idn.PersonId == item.PersonId)?.PhotographRelativePath;
            //});
            return personDetailsLst;
        }
    }
}