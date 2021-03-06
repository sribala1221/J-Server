using GenerateTables.Models;
using ServerAPI.Utilities;
using ServerAPI.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;

// ReSharper disable once CheckNamespace
namespace ServerAPI.Services
{
    // ReSharper disable once UnusedMember.Global
    public class SearchInmateCaseService : ISearchInmateCaseService
    {
        private readonly AAtims _context;

        // ReSharper disable once NotAccessedField.Local
        private readonly IPhotosService _photos;

        public SearchInmateCaseService(AAtims context, IPhotosService photosService)
        {
            _context = context;
            _photos = photosService;
        }

        // Get Intake Search Details based on requested information
        public List<SearchResult> GetBookingSearchList(SearchRequestVm searchDetails)
        {
            bool isToSearchDob = searchDetails.DateOfBirth.HasValue &&
                                 searchDetails.DateOfBirth > DateTime.MinValue;
            bool isToSearchByBooking = searchDetails.bookingCaseFlag != 5
                                       && searchDetails.caseBookingFrom.HasValue &&
                                       searchDetails.caseBookingTo.HasValue;
            bool isToSearchByClear = searchDetails.bookingCaseFlag != 5
                                     && searchDetails.caseClearFrom.HasValue &&
                                     searchDetails.caseClearTo.HasValue;
            bool isToSearchBySent = searchDetails.bookingCaseFlag != 5
                                    && searchDetails.casesentStartFrom.HasValue &&
                                    searchDetails.casesentStartTo.HasValue;
            bool isToSearchBySchedule = searchDetails.bookingCaseFlag != 5
                                        && searchDetails.caseSchStartFrom.HasValue &&
                                        searchDetails.caseSchStartTo.HasValue;
            bool isToSearchByOverallSent =
                searchDetails.overallSentDaysFrom > 0 || searchDetails.overallSentDaysTo > 0;

            List<Lookup> lstLookupDetails = _context.Lookup
                .Where(lkp =>
                    (lkp.LookupType == LookupConstants.ARRTYPE)
                    && !string.IsNullOrEmpty(lkp.LookupDescription))
                .Select(lkp => new Lookup
                {
                    LookupDescription = lkp.LookupDescription,
                    LookupIndex = lkp.LookupIndex,
                    LookupType = lkp.LookupType
                }).Distinct().ToList();

            List<SearchResult> personDetailsLst = _context.IncarcerationArrestXref.Where(iax =>
                    (searchDetails.FacilityId == 0 || iax.Arrest.Inmate.FacilityId == searchDetails.FacilityId)
                    && (!searchDetails.ActiveOnly || iax.Arrest.Inmate.InmateActive == 1)
                    && (string.IsNullOrEmpty(searchDetails.bunkId)
                        || iax.Arrest.Inmate.HousingUnitId > 0
                        && iax.Arrest.Inmate.HousingUnit.HousingUnitBedLocation == searchDetails.bunkId
                        && (!iax.Arrest.Inmate.HousingUnit.HousingUnitInactive.HasValue ||
                            iax.Arrest.Inmate.HousingUnit.HousingUnitInactive == 0))
                    && (string.IsNullOrEmpty(searchDetails.cellId)
                        || iax.Arrest.Inmate.HousingUnitId > 0
                        && iax.Arrest.Inmate.HousingUnit.HousingUnitBedNumber == searchDetails.cellId
                        && (!iax.Arrest.Inmate.HousingUnit.HousingUnitInactive.HasValue ||
                            iax.Arrest.Inmate.HousingUnit.HousingUnitInactive == 0))
                    && (searchDetails.podId == 0
                        || iax.Arrest.Inmate.HousingUnitId > 0
                        && iax.Arrest.Inmate.HousingUnit.HousingUnitListId == searchDetails.podId
                        && (!iax.Arrest.Inmate.HousingUnit.HousingUnitInactive.HasValue ||
                            iax.Arrest.Inmate.HousingUnit.HousingUnitInactive == 0))
                    && (string.IsNullOrEmpty(searchDetails.BuildingId)
                        || iax.Arrest.Inmate.HousingUnitId > 0
                        && iax.Arrest.Inmate.HousingUnit.HousingUnitLocation == searchDetails.BuildingId
                        && (!iax.Arrest.Inmate.HousingUnit.HousingUnitInactive.HasValue ||
                            iax.Arrest.Inmate.HousingUnit.HousingUnitInactive == 0))
                    && (searchDetails.InmateSearchFacilityId == 0
                        || (iax.Arrest.Inmate.FacilityId == searchDetails.InmateSearchFacilityId))
                    && (string.IsNullOrEmpty(searchDetails.classificationId)
                        || iax.Arrest.Inmate.InmateClassification.InmateClassificationReason == searchDetails.classificationId)
                    && (searchDetails.locationId == 0
                        || (iax.Arrest.Inmate.InmateCurrentTrackId == searchDetails.locationId))
                    && (searchDetails.personFlagId == 0
                        || iax.Arrest.Inmate.Person.PersonFlag.Any(pf =>
                            pf.PersonFlagIndex == searchDetails.personFlagId && pf.DeleteFlag == 0))
                    && (searchDetails.inmateFlagId == 0
                        || iax.Arrest.Inmate.Person.PersonFlag.Any(pf =>
                            pf.InmateFlagIndex == searchDetails.inmateFlagId && pf.DeleteFlag == 0))
                    && (searchDetails.medFlagId == 0
                        || iax.Arrest.Inmate.Person.PersonFlag.Any(
                            pf => pf.MedicalFlagIndex == searchDetails.medFlagId && pf.DeleteFlag == 0))
                            && (searchDetails.dietFlagId == 0
                        || iax.Arrest.Inmate.Person.PersonFlag.Any(
                            pf => pf.DietFlagIndex == searchDetails.dietFlagId && pf.DeleteFlag == 0))
                    && (string.IsNullOrEmpty(searchDetails.InmateNumber)
                        || iax.Arrest.Inmate.InmateNumber.StartsWith(searchDetails.InmateNumber)
                        || iax.Arrest.Inmate.Person.Aka.Any(p =>
                            p.AkaInmateNumber.StartsWith(searchDetails.InmateNumber)))
                    && (string.IsNullOrEmpty(searchDetails.InmateSiteNumber)
                        || iax.Arrest.Inmate.InmateSiteNumber.StartsWith(searchDetails.InmateSiteNumber)
                        || iax.Arrest.Inmate.Person.Aka.Any(
                            p => p.AkaSiteInmateNumber.StartsWith(searchDetails.InmateSiteNumber)))
                    && (!searchDetails.IncarcerationSearch
                        || !searchDetails.ActiveOnly
                        || iax.Arrest.Inmate.Incarceration.Any(inc =>
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
                            && (!isToSearchByOverallSent
                                || inc.TotSentDays <= searchDetails.overallSentDaysFrom
                                && inc.TotSentDays >= searchDetails.overallSentDaysTo)
                            && (searchDetails.ClearByOfficer == 0 ||
                                inc.ReleaseClearBy ==
                                searchDetails.ClearByOfficer)
                            && (searchDetails.IntakeOfficer == 0 ||
                                inc.InOfficerId ==
                                searchDetails.IntakeOfficer)))
                    && (string.IsNullOrEmpty(searchDetails.Moniker)
                        || iax.Arrest.Inmate.Person.Aka.Any(aka => aka.PersonGangName.StartsWith(searchDetails.Moniker))
                    )
                    && (string.IsNullOrEmpty(searchDetails.FirstName)
                        || (iax.Arrest.Inmate.Person.PersonFirstName.StartsWith(searchDetails.FirstName)
                            || iax.Arrest.Inmate.Person.Aka.Any(aka =>
                                aka.AkaFirstName.StartsWith(searchDetails.FirstName))))
                    && (string.IsNullOrEmpty(searchDetails.LastName)
                        || (iax.Arrest.Inmate.Person.PersonLastName.StartsWith(searchDetails.LastName)
                            || iax.Arrest.Inmate.Person.Aka.Any(aka =>
                                aka.AkaLastName.StartsWith(searchDetails.LastName))))
                    && (string.IsNullOrEmpty(searchDetails.MiddleName)
                        || (iax.Arrest.Inmate.Person.PersonMiddleName.StartsWith(searchDetails.MiddleName)
                            || iax.Arrest.Inmate.Person.Aka.Any(aka =>
                                aka.AkaMiddleName.StartsWith(searchDetails.MiddleName))))
                    && (string.IsNullOrEmpty(searchDetails.AfisNumber)
                        || (iax.Arrest.Inmate.Person.AfisNumber.StartsWith(searchDetails.AfisNumber)
                            || iax.Arrest.Inmate.Person.Aka.Any(aka =>
                                aka.AkaAfisNumber.StartsWith(searchDetails.AfisNumber))))
                    && (!isToSearchDob
                        || (iax.Arrest.Inmate.Person.PersonDob.Value.Date == searchDetails.DateOfBirth.Value.Date
                            || iax.Arrest.Inmate.Person.Aka.Any(aka =>
                                aka.AkaDob.Value.Date == searchDetails.DateOfBirth.Value.Date)))
                    && (searchDetails.InmateSearchFacilityId == 0
                        || iax.Arrest.Inmate.FacilityId == searchDetails.InmateSearchFacilityId)
                    // binding arrest details
                    && (!searchDetails.IsArrestSearch || iax.Arrest.ArrestActive == 1)
                    && (searchDetails.IsArrestSearch
                        || !searchDetails.ActiveOnly
                        || !searchDetails.IsBookingSearch
                        || iax.Arrest.ArrestActive == 1)
                    && (!searchDetails.activeCasesOnly || !iax.ReleaseDate.HasValue)
                    && (string.IsNullOrEmpty(
                            searchDetails.BookingNumber)
                        || iax.Arrest.ArrestBookingNo.Contains(searchDetails
                            .BookingNumber))
                    && (string.IsNullOrEmpty(searchDetails.CourtDocket)
                        || iax.Arrest.ArrestCourtDocket.Contains(searchDetails
                            .CourtDocket))
                    && (searchDetails.BookingType == 0
                        || iax.Arrest.ArrestType == searchDetails.BookingType
                            .ToString())
                    && (searchDetails.BillingAgency == 0
                        || iax.Arrest.ArrestBillingAgencyId ==
                        searchDetails.BillingAgency)
                    && (searchDetails.Court == 0
                        || iax.Arrest.ArrestCourtJurisdictionId ==
                        searchDetails.Court)
                    && (searchDetails.ArrestingAgency == 0
                        || iax.Arrest.ArrestingAgencyId ==
                        searchDetails.ArrestingAgency)
                    && (searchDetails.ArrestingOfficer == 0
                        || iax.Arrest.ArrestOfficerId ==
                        searchDetails.ArrestingOfficer)
                    && (searchDetails.arrestingOfficerName == 0
                        || iax.Arrest.ArrestOfficerId ==
                        searchDetails.arrestingOfficerName)
                    && (searchDetails.BookingOfficer == 0
                        || iax.Arrest.ArrestBookingOfficerId ==
                        searchDetails.BookingOfficer)
                    && (string.IsNullOrEmpty(searchDetails.bailType)
                        || iax.Arrest.BailType == searchDetails.bailType)
                    && (!searchDetails.noBail || iax.Arrest.BailNoBailFlag == 0)
                    && (searchDetails.arrestConvictionStatus == 0
                        || iax.Arrest.ArrestBookingStatus ==
                        searchDetails.arrestConvictionStatus)
                    && (string.IsNullOrEmpty(searchDetails
                            .lawEnforcementDispositionId)
                        || iax.Arrest.ArrestLawEnforcementDisposition ==
                        searchDetails.lawEnforcementDispositionId)
                    && (searchDetails.sentCode == 0
                        || iax.Arrest.ArrestSentenceCode ==
                        searchDetails.sentCode)
                    && (searchDetails.sentMethod == 0
                        || iax.Arrest.ArrestSentenceMethodId ==
                        searchDetails.sentMethod)
                    && (searchDetails.bailFrom == 0
                        || iax.Arrest.BailAmount >= searchDetails.bailFrom &&
                        iax.Arrest.BailAmount <= searchDetails.bailTo)
                    && (searchDetails.sentDaysFrom == 0
                        || iax.Arrest.ArrestSentenceActualDaysToServe >=
                        searchDetails.sentDaysFrom
                        && iax.Arrest.ArrestSentenceActualDaysToServe <=
                        searchDetails.sentDaysTo)
                    && (!isToSearchByBooking
                        || iax.Arrest.ArrestBookingDate >= searchDetails.caseBookingFrom
                        && iax.Arrest.ArrestBookingDate <= searchDetails.caseBookingTo)
                    && (!isToSearchByClear
                        || iax.Arrest.ArrestReleaseClearedDate >= searchDetails.caseClearFrom
                        && iax.Arrest.ArrestReleaseClearedDate <= searchDetails.caseClearTo)
                    && (!isToSearchBySent
                        || iax.Arrest.ArrestSentenceStartDate >= searchDetails.casesentStartFrom
                        && iax.Arrest.ArrestSentenceStartDate <= searchDetails.casesentStartTo)
                    && (!isToSearchBySchedule
                        || iax.Arrest.ArrestSentenceReleaseDate >= searchDetails.caseSchStartFrom
                        && iax.Arrest.ArrestSentenceReleaseDate <= searchDetails.caseSchStartTo)
                    && (string.IsNullOrEmpty(searchDetails.clearReason)
                        || iax.ArrestId > 0
                        && iax.ReleaseReason.ToUpper() ==
                        searchDetails.clearReason.ToUpper())
                    && (string.IsNullOrEmpty(searchDetails.ChargeType)
                        || iax.Arrest.Crime.Any(crr => crr.CrimeLookup.CrimeCodeType.ToUpper() ==
                                                       searchDetails.ChargeType.ToUpper()))
                    && (searchDetails.ChargeGroup == 0
                        || iax.Arrest.Crime.Any(crr => crr.CrimeLookup.CrimeGroupId == searchDetails.ChargeGroup))
                    && (string.IsNullOrEmpty(searchDetails.ChargeSection)
                        || iax.Arrest.Crime.Any(crr => crr.CrimeLookup.CrimeSection.ToUpper()
                            .Contains(searchDetails.ChargeSection.ToUpper())))
                    && (string.IsNullOrEmpty(searchDetails.ChargeDescription)
                        || iax.Arrest.Crime.Any(crr => crr.CrimeLookup.CrimeDescription.ToUpper()
                            .Contains(searchDetails.ChargeDescription.ToUpper())))
                    && (string.IsNullOrEmpty(searchDetails.bookingCaseType)
                        || iax.Arrest.Crime.Any(
                            crr => crr.CrimeLookup.CrimeStatuteCode == searchDetails.bookingCaseType))
                    && (string.IsNullOrEmpty(searchDetails.CrimeType)
                        || iax.Arrest.Crime.Any(crr => crr.CrimeType == searchDetails.CrimeType))
                    && (string.IsNullOrEmpty(searchDetails.Classify)
                        || iax.Arrest.Inmate.InmateClassification.InmateClassificationReason == searchDetails.Classify)
                ).Take(10000)
                .Select(iax => new SearchResult
                {
                    ArrestId = iax.ArrestId ?? 0,
                    IncarArrestXrefId = iax.IncarcerationArrestXrefId,
                    BookingDate = iax.BookingDate,
                    ReleaseDate = iax.ReleaseDate,
                    ReleaseReason = iax.ReleaseReason,
                    ArrestSentenceCode = iax.Arrest.ArrestSentenceCode ?? 3,
                    ArrestSentenceReleaseDate = iax.Arrest.ArrestSentenceReleaseDate,
                    ArrestReleaseClearedDate = iax.Arrest.ArrestReleaseClearedDate,
                    ArrestSupSeqNumber = iax.Arrest.ArrestSupSeqNumber,
                    WarrantCount = iax.Arrest.Warrant.Count(),
                    WarrantCrimeCount = iax.Arrest.Crime.Count(ar => ar.WarrantId != null),
                    CrimeCount = iax.Arrest.Crime.Count(ar => ar.WarrantId == null),
                    CrimeForceCount = iax.Arrest.CrimeForce.Count(ar => ar.WarrantId == null),
                    WarrantCrimeForceCount = iax.Arrest.CrimeForce.Count(ar => ar.WarrantId != null),
                    IncarbookingNumber = iax.Incarceration.BookingNo,
                    IncarcerationId = iax.Incarceration.IncarcerationId,
                    PersonId = iax.Incarceration.Inmate.PersonId,
                    InmateId = iax.Incarceration.Inmate.InmateId,
                    ArrestType = iax.Arrest.ArrestType,
                    ArrestBillingAgencyId = iax.Arrest.ArrestBillingAgencyId,
                    ArrestCourtJurisdictionId = iax.Arrest.ArrestCourtJurisdictionId,
                    FirstName = iax.Incarceration.Inmate.Person.PersonFirstName,
                    MiddleName = iax.Incarceration.Inmate.Person.PersonMiddleName,
                    LastName = iax.Incarceration.Inmate.Person.PersonLastName,
                    //Photofilepath = _photos.GetPhotoByIdentifier(iax.Arrest.IncarcerationArrestXref.Select(s =>
                    //        s.Incarceration.Inmate.Person.Identifiers.LastOrDefault(idn =>
                    //            idn.IdentifierType == "1" && idn.DeleteFlag == 0)
                    //    ).FirstOrDefault())
                    CaseNo = iax.Arrest.ArrestCaseNumber,
                    BookingNumber = iax.Arrest.ArrestBookingNo,
                    CourtDocket = iax.Arrest.ArrestCourtDocket,
                    BookingType = lstLookupDetails.FirstOrDefault(
                        lkp => lkp.LookupType == LookupConstants.ARRTYPE
                               && !string.IsNullOrEmpty(iax.Arrest.ArrestType)
                               && lkp.LookupIndex.ToString() == iax.Arrest.ArrestType.Trim()).LookupDescription,
                    InmateActive = iax.Incarceration.Inmate.InmateActive,
                    InmateNumber = iax.Incarceration.Inmate.InmateNumber,
                    Classify = iax.Incarceration.Inmate.InmateClassification.InmateClassificationReason
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

        public List<Facility> GetFacilityDetails()
        {
            List<Facility> lstFacilityAbbr = _context.Facility.Where(w => w.DeleteFlag == 0)
                .ToList();
            return lstFacilityAbbr;
        }
    }
}