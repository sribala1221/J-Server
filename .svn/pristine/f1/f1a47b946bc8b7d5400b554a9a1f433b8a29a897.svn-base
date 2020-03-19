using System;
using System.Collections.Generic;
using System.Linq;
using GenerateTables.Models;
using ServerAPI.ViewModels;
using ServerAPI.Utilities;
using System.Globalization;

namespace ServerAPI.Services
{
    public class InmateSummaryService : IInmateSummaryService
    {
        private readonly AAtims _context;
        private readonly ICommonService _commonService;
        private readonly IInmateHeaderService _inmateHeaderService;
        private readonly IKeepSepAlertService _iKeepSepAlertService;
        private readonly IPersonService _personService;
        private readonly IFacilityHousingService _facilityHousingService;

        public InmateSummaryService(AAtims context, ICommonService commonService, IInmateHeaderService inmateHeaderService,
            IKeepSepAlertService iKeepSepAlertService, IPersonService personService, IFacilityHousingService facilityHousingService)
        {
            _context = context;
            _commonService = commonService;
            _inmateHeaderService = inmateHeaderService;
            _iKeepSepAlertService = iKeepSepAlertService;
            _personService = personService;
            _facilityHousingService = facilityHousingService;
        }

        //To Get Inmate Summary Details
        public InmateSummaryDetailsVm GetInmateSummaryDetails(InmateSummaryVm inmateSummary)
        {

            InmateSummaryDetailsVm isvm = new InmateSummaryDetailsVm
            {
                PersonDetails = _personService.GetInmateDetails(inmateSummary.InmateId)
            };


            if (inmateSummary.InmateHeader)
            {
                isvm.InmateHeader = GetInmateHeader(inmateSummary.InmateId);
            }
            if (inmateSummary.InmateAlerts)
            {
                isvm.InmateAlerts = GetInmateAlerts(inmateSummary.InmateId);
            }
            if (inmateSummary.ReleasedBookings || inmateSummary.ActiveBookings)
            {
                LoadBookings(inmateSummary.InmateId, inmateSummary.ActiveBookings, inmateSummary.ReleasedBookings, isvm);
            }
            if (inmateSummary.InOut)
            {
                isvm.InOut = GetInmateIncarceration(inmateSummary.InmateId);
            }
            if (inmateSummary.FloorNote)
            {
                isvm.FloorNotes = GetFloorNotes(inmateSummary.InmateId);
            }
            if (inmateSummary.Incident)
            {
                isvm.Incident = GetDisciplinaryIncident(inmateSummary.InmateId);
            }
            if (inmateSummary.Track)
            {
                isvm.Track = GetInmateTrack(inmateSummary.InmateId);
            }
            if (inmateSummary.Visitors)
            {
                isvm.Visitors = GetVisitorDetails(inmateSummary.InmateId);
            }
            if (inmateSummary.InmateClassification)
            {
                isvm.InmateClassification = GetInmateClassification(inmateSummary.InmateId);
            }
            if (inmateSummary.Appointments)
            {
                isvm.InmateAppointment = GetInmateAppointments(inmateSummary.InmateId);
            }
            if (inmateSummary.Housinghistory)
            {
                isvm.HousingHistory = GetHousingUnitMoveHistory(inmateSummary.InmateId);
            }
            if (inmateSummary.KeepSeparate)
            {
                isvm.KeepSeparate = GetKeepSeparate(inmateSummary.InmateId);
            }
            if (inmateSummary.Association)
            {
                isvm.Association = GetAssociation(isvm.PersonDetails.PersonId);
            }

            return isvm;
        }

        #region GetInmateHeader

        private InmateHeaderVm GetInmateHeader(int inmateId)
        {
            InmateHeaderVm ihvm = _inmateHeaderService.GetInmateBasicInfo(inmateId);

            ihvm.LstPrivilegesAlerts =
            (from ipx in _context.InmatePrivilegeXref
             where ipx.InmateId == inmateId &&
                   ipx.PrivilegeDate.HasValue && ipx.PrivilegeDate < DateTime.Now &&
                   (!ipx.PrivilegeExpires.HasValue || ipx.PrivilegeExpires.Value >= DateTime.Now) &&
                   !ipx.PrivilegeRemoveOfficerId.HasValue
             select new PrivilegeDetailsVm
             {
                 PrivilegeDescription = ipx.Privilege.PrivilegeDescription,
                 PrivilegeType = ipx.Privilege.PrivilegeType,
                 ExpireDate = ipx.PrivilegeExpires
             }).ToList();

            return ihvm;
        }

        #endregion

        #region GetInmateAlerts

        /// <summary>
        /// To get inmate alerts by inmate id
        /// </summary>
        /// <param name="inmateId"></param>
        /// <returns></returns>
        private InmateAlerts GetInmateAlerts(int inmateId)
        {
            Inmate inmate = _context.Inmate.Single(inm => inm.InmateId == inmateId);
            InmateAlerts ih = new InmateAlerts
            {
                PersonId = inmate.PersonId,
                InmateId = inmateId,
                IllegalAlien = inmate.Person.IllegalAlienFlag,
                InmateActive = inmate.InmateActive == 1,
                PersonAlerts = _context.PersonAlert.Where(pa =>
                            pa.PersonId == inmate.PersonId && pa.ActiveAlertFlag.HasValue &&
                            pa.ActiveAlertFlag.Value == 1)
                    .Select(pa => pa.Alert)
                    .ToList()
            };

            IQueryable<PersonFlag> personFlag =
                _context.PersonFlag.Where(pa => pa.PersonId == inmate.PersonId && pa.DeleteFlag == 0);

            ih.PersonFlagAlerts =
                _commonService.GetLookupList(LookupConstants.PERSONCAUTION)
                    .Where(
                        pfa =>
                            personFlag.Select(pf => pf.PersonFlagIndex)
                                .Where(pf => pf.HasValue)
                                .Contains(pfa.LookupIndex))
                    .Select(pfa => pfa.LookupDescription)
                    .ToList();

            ih.PersonClassificationDetails = _iKeepSepAlertService.GetAssociation(inmate.PersonId);

            ih.WarrantDetails =
                _context.Warrant.Count(
                    wa =>
                        wa.PersonId == inmate.PersonId && !wa.WarrantClearedDate.HasValue &&
                        wa.LocalWarrantFlag.HasValue && wa.LocalWarrantFlag.Value == 1);

            ih.WarrantHold =
                _context.WarrantHold.Count(
                    wa =>
                        wa.PersonId == inmate.PersonId && !wa.WarrantHoldRemoved.HasValue);

            ih.License =
                _context.License.Count(
                    li =>
                        li.PersonId == inmate.PersonId && li.LicenseDate <= DateTime.Now &&
                        (!li.LicenseExpires.HasValue ||
                         li.LicenseExpires.Value >= DateTime.Now)) > 0;

            if (!ih.InmateActive) return ih;
            ih.PersonFlagAlerts.AddRange(_commonService.GetLookupList(LookupConstants.TRANSCAUTION).Where(
                pfa => personFlag.Select(pf => pf.InmateFlagIndex)
                    .Where(pf => pf.HasValue)
                    .Contains(pfa.LookupIndex))
                    .Select(pfa => pfa.LookupDescription)
                    .ToList());

            LoadAlertKeepSepDetails(inmateId, ih);

            return ih;
        }

        private void LoadAlertKeepSepDetails(int inmateId, InmateAlerts ih)
        {
            IQueryable<KeepSeparate> keepSep = _context.KeepSeparate.Where(ks => ks.InactiveFlag == 0 &&
                        (ks.KeepSeparateInmate1Id == inmateId || ks.KeepSeparateInmate2Id == inmateId));
            List<Lookup> lookupslist = _context.Lookup.Where(w => w.LookupType == LookupConstants.CLASSGROUP).ToList();
            List<Lookup> lookupSubsetlist = _context.Lookup.Where(w => w.LookupType == LookupConstants.CLASSGROUPSUB).ToList();
            ih.KeepSepDetails = (from ks in keepSep
                                 where ks.KeepSeparateInmate1Id == inmateId && ks.KeepSeparateInmate2.InmateActive == 1
                                 select new KeepSeparateVm
                                 {
                                     Type = KeepSepType.Inmate,
                                     KeepSeparateNote = ks.KeepSeparateNote,
                                     Reason = ks.KeepSeparateReason,
                                     KeepSepType = ks.KeepSeparateType,
                                     KeepSepInmateId = ks.KeepSeparateInmate2Id
                                 }).ToList();

            ih.KeepSepDetails.AddRange((from ks in keepSep
                                        where ks.KeepSeparateInmate2Id == inmateId && ks.KeepSeparateInmate1.InmateActive == 1
                                        select new KeepSeparateVm
                                        {
                                            Type = KeepSepType.Inmate,
                                            KeepSeparateNote = ks.KeepSeparateNote,
                                            Reason = ks.KeepSeparateReason,
                                            KeepSepType = ks.KeepSeparateType,
                                            KeepSepInmateId = ks.KeepSeparateInmate1Id
                                        }).ToList());


            ih.KeepSepDetails.AddRange((from kss in _context.KeepSepSubsetInmate
                where kss.KeepSepInmate2Id == inmateId && kss.DeleteFlag == 0
                select new KeepSeparateVm
                {
                    Type = KeepSepType.Subset,
                    Reason = kss.KeepSepReason,
                    KeepSeparateNote = kss.KeepSeparateNote,
                    KeepSepType = kss.KeepSeparateType,
                    KeepSepAssoc = lookupslist.Single(f => f.LookupIndex == kss.KeepSepAssoc1Id).LookupDescription,
                    KeepSepSubset = lookupSubsetlist.Single(f => f.LookupIndex == kss.KeepSepAssoc1SubsetId)
                        .LookupDescription,
                    KeepSepAssoc1Id = kss.KeepSepAssoc1Id,
                    KeepSepAssoc1SubsetId = kss.KeepSepAssoc1SubsetId,
                    KeepSepInmateId = kss.KeepSepInmate2Id
                }).ToList());

            ih.KeepSepDetails.AddRange((from ksa in _context.KeepSepAssocInmate
                where ksa.KeepSepInmate2Id == inmateId && ksa.DeleteFlag == 0
                select new KeepSeparateVm
                {
                    Type = KeepSepType.Association,
                    Reason = ksa.KeepSepReason,
                    KeepSeparateNote = ksa.KeepSeparateNote,
                    KeepSepType = ksa.KeepSeparateType,
                    KeepSepAssoc = lookupslist.Single(f => f.LookupIndex == ksa.KeepSepAssoc1Id).LookupDescription,
                    KeepSepAssoc1Id = ksa.KeepSepAssoc1Id,
                    KeepSepInmateId = ksa.KeepSepInmate2Id
                }).ToList());

            IQueryable<PersonClassification> personClassification =
                _context.PersonClassification.Where(
                    pc =>
                        pc.InactiveFlag == 0 && pc.PersonId == ih.PersonId &&
                        pc.PersonClassificationDateFrom.HasValue &&
                        pc.PersonClassificationDateFrom.Value <= DateTime.Now.Date &&
                        (!pc.PersonClassificationDateThru.HasValue ||
                         pc.PersonClassificationDateThru.Value <= DateTime.Now.Date) &&
                         pc.PersonClassificationTypeId > 0);

            ih.KeepSepDetails.AddRange((from pc in personClassification
                                        from ksai in _context.KeepSepAssocInmate
                                        where pc.PersonClassificationTypeId == ksai.KeepSepAssoc1Id
                                              && ksai.DeleteFlag == 0
                                        select new KeepSeparateVm
                                        {
                                            Type = KeepSepType.Inmate,
                                            KeepSeparateNote = ksai.KeepSeparateNote,
                                            Reason = ksai.KeepSepReason,
                                            KeepSepType = ksai.KeepSeparateType,
                                            KeepSepInmateId = ksai.KeepSepInmate2Id
                                        }).ToList());

            ih.KeepSepDetails.AddRange((from pc in personClassification
                                        from ksai in _context.KeepSepSubsetInmate
                                        where pc.PersonClassificationSubsetId == ksai.KeepSepAssoc1SubsetId
                                              && ksai.DeleteFlag == 0
                                              && pc.PersonClassificationTypeId == ksai.KeepSepAssoc1Id
                                        select new KeepSeparateVm
                                        {
                                            Type = KeepSepType.Inmate,
                                            KeepSeparateNote = ksai.KeepSeparateNote,
                                            Reason = ksai.KeepSepReason,
                                            KeepSepType = ksai.KeepSeparateType,
                                            KeepSepInmateId = ksai.KeepSepInmate2Id
                                        }).ToList());

            LoadInmatesForKeepSep(ih.KeepSepDetails);
        }

        #endregion

        #region LoadAllBookings

        /// <summary>
        /// To load all booking of inmate by inmate_id and seperated as an active and inactive booking as per the condition.
        /// </summary>
        /// <param name="inmateId"></param>
        /// <param name="activeBookings"></param>
        /// <param name="releasedBookings"></param>
        /// <param name="isvm"></param>
        private void LoadBookings(int inmateId, bool activeBookings, bool releasedBookings, InmateSummaryDetailsVm isvm)
        {
            List<ArrestDetails> arrest = (from a in _context.Arrest
                                          where a.InmateId == inmateId
                                          select new ArrestDetails
                                          {
                                              ArrestId = a.ArrestId,
                                              InmateId = a.InmateId,
                                              BookingNumber = a.ArrestBookingNo,
                                              ArrestActive = a.ArrestActive == 1,
                                              ArrestArraignmentCourtId = a.ArrestArraignmentCourtId,
                                              ArrestArraignmentDate = a.ArrestArraignmentDate,
                                              ArrestBookingDate = a.ArrestBookingDate,
                                              ArrestBookingOfficerId = a.ArrestBookingOfficerId,
                                              ArrestBookingStatusId = a.ArrestBookingStatus,
                                              ArrestCaseNumber = a.ArrestCaseNumber,
                                              ArrestConditionsOfRelease = a.ArrestConditionsOfRelease,
                                              ArrestCourtDocket = a.ArrestCourtDocket,
                                              ArrestDate = a.ArrestDate,
                                              ArrestExamineBooking = a.ArrestExamineBooking,
                                              ArrestLocation = a.ArrestLocation,
                                              ArrestNonCompliance = a.ArrestNonCompliance,
                                              ArrestOfficerId = a.ArrestOfficerId,
                                              ArrestOfficerText = a.ArrestOfficerText,
                                              ArrestPcn = a.ArrestPcn,
                                              ArrestSentenceByHour = a.ArrestSentenceByHour,
                                              ArrestSentenceCode = a.ArrestSentenceCode,
                                              ArrestSentenceDays = a.ArrestSentenceDays,
                                              ArrestSentenceDaysStayed = a.ArrestSentenceDaysStayed,
                                              ArrestSentenceEarlyRelease = a.ArrestSentenceEarlyRelease,
                                              ArrestSentenceFineDays = a.ArrestSentenceFineType,
                                              ArrestSentenceForThwith = a.ArrestSentenceForthwith,
                                              ArrestSentenceGwGtAdjust = a.ArrestSentenceGwGtAdjust,
                                              ArrestSentenceGwGtDays = a.ArrestSentenceGwGtDays,
                                              ArrestSentenceIndefiniteHold = a.ArrestSentenceIndefiniteHold,
                                              ArrestSentenceReleaseDate = a.ArrestSentenceReleaseDate,
                                              ArrestSentenceStartDate = a.ArrestSentenceStartDate,
                                              ArrestSentenceWeekender = a.ArrestSentenceWeekender,
                                              ArrestSentenced = a.ArrestSentenced,
                                              ArrestSiteBookingNumber = a.ArrestSiteBookingNo,
                                              ArrestTimeServedDays = a.ArrestTimeServedDays,
                                              ArrestTypeId = a.ArrestType,
                                              BailAmount = a.BailAmount,
                                              CourtId = a.ArrestArraignmentCourtId,
                                              ArrestAgencyId = a.ArrestingAgencyId,
                                              BookingAgencyId = a.BookingAgencyId
                                          }).ToList();

            string siteOption =
                _context.SiteOptions.SingleOrDefault(
                        so =>
                            so.SiteOptionsStatus == "1" &&
                            so.SiteOptionsName == SiteOptionsConstants.JAILSELINCAR)?
                    .SiteOptionsValue;

            #region OfficerId's

            List<int> officerIds =
                arrest.Select(i => new[] { i.ArrestOfficerId, i.ArrestBookingOfficerId })
                    .SelectMany(i => i)
                    .Where(i => i.HasValue)
                    .Select(i => i.Value)
                    .ToList();

            List<PersonnelVm> arrestOfficer =
                _personService.GetPersonNameList(officerIds.ToList());

            #endregion

            #region AgencyId's

            List<int> agencyIds =
                arrest.Select(i => new[] { i.ArrestAgencyId, i.BookingAgencyId, i.CourtId })
                    .SelectMany(i => i)
                    .Where(i => i.HasValue)
                    .Select(i => i.Value)
                    .ToList();

            List<AgencyVm> agencyList =
                _commonService.GetAgencyNameList(agencyIds.ToList());

            #endregion


            arrest.ForEach(lstArrest =>
            {
                if (lstArrest.CourtId.HasValue)
                {
                    lstArrest.Court = agencyList.Single(ag => ag.AgencyId == lstArrest.CourtId.Value).AgencyName;
                }
                lstArrest.ArrestAgencyName =
                    agencyList.Single(age => age.AgencyId == lstArrest.ArrestAgencyId).AgencyName;

                lstArrest.BookingAgencyName =
                    agencyList.Single(age => age.AgencyId == lstArrest.BookingAgencyId).AgencyName;

                if (lstArrest.ArrestTypeId != null)
                {
                    lstArrest.ArrestType =
                        _commonService.GetLookupList(LookupConstants.ARRTYPE).SingleOrDefault(look =>
                                    Convert.ToString(look.LookupIndex, CultureInfo.InvariantCulture) ==
                                    lstArrest.ArrestTypeId.Trim())
                            ?.LookupDescription;
                }

                if (lstArrest.ArrestBookingStatusId.HasValue)
                {
                    lstArrest.ArrestBookingStatus =
                        _commonService.GetLookupList(LookupConstants.INMSTAT).SingleOrDefault(look =>
                                    (int?)(look.LookupIndex) == lstArrest.ArrestBookingStatusId.Value)
                            ?.LookupDescription;
                }

                IQueryable<IncarcerationArrestXref> lstIncarcerationArrestXref =
                    _context.IncarcerationArrestXref.Where(inc => inc.ArrestId == lstArrest.ArrestId);

                lstArrest.ReactiveHistory = lstIncarcerationArrestXref.Select(
                    iaxe => new BookingHistory
                    {
                        BookingDate = iaxe.BookingDate,
                        ReleaseDate = iaxe.ReleaseDate,
                        BookingReason = iaxe.ReleaseReason
                    }).ToList();

                if (siteOption == SiteOptionsConstants.ON)
                {
                    lstArrest.IncarcerationDetails = (from inc in lstIncarcerationArrestXref
                                                      where inc.ArrestId == lstArrest.ArrestId
                                                      select new IncarcerationDetail
                                                      {
                                                          UsedPersonFirst = inc.Incarceration.UsedPersonFrist,
                                                          UsedPersonLast = inc.Incarceration.UsedPersonLast,
                                                          UsedPersonMiddle = inc.Incarceration.UsedPersonMiddle,
                                                          UsedPersonSuffix = inc.Incarceration.UsedPersonSuffix
                                                      }).First();
                }

                if (lstArrest.ArrestSentenced.HasValue)
                {
                    lstArrest.DisciplinaryDays = _context.DisciplinaryControl.Where(
                            dc =>
                                dc.DisciplinaryArrestId.HasValue && dc.InmateId == lstArrest.InmateId &&
                                dc.DisciplinaryDays.HasValue &&
                                dc.DisciplinaryArrestId.Value == lstArrest.ArrestId)
                        .Select(dc => dc.DisciplinaryDays.Value)
                        .ToList();
                }

                lstArrest.ArrestOfficer =
                    arrestOfficer.Single(arr => arr.PersonnelId == lstArrest.ArrestOfficerId);

                if (lstArrest.ArrestBookingOfficerId.HasValue)
                {
                    lstArrest.BookingOfficer =
                        arrestOfficer.Single(arr => arr.PersonnelId == lstArrest.ArrestBookingOfficerId);
                }

                lstArrest.WarrantDetails = (from wd in _context.Warrant
                                            where wd.ArrestId == lstArrest.ArrestId
                                            select new WarrantDetails
                                            {
                                                Number = wd.WarrantNumber,
                                                Description = wd.WarrantDescription,
                                                Type = wd.WarrantType,
                                                LocalWarrantFlag = wd.LocalWarrantFlag,
                                                Complete = wd.WarrantComplete,
                                                County = wd.WarrantCounty,
                                                WarrantId = wd.WarrantId,
                                                ChargeDetails = (from cr in _context.Crime
                                                                 where cr.WarrantId == wd.WarrantId
                                                                 select new PrebookCharge
                                                                 {
                                                                     CrimeId = cr.CrimeId,
                                                                     CrimeDescription = cr.CrimeLookup.CrimeDescription,
                                                                     CrimeSection = cr.CrimeLookup.CrimeSection,
                                                                     CrimeSubSection = cr.CrimeLookup.CrimeSubSection,
                                                                     CrimeStatuteCode = cr.CrimeLookup.CrimeStatuteCode,
                                                                     CrimeType = cr.CrimeType
                                                                 }).ToList()
                                            }).ToList();

                lstArrest.CrimeDetails = (from cr in _context.Crime
                                          where cr.ArrestId == lstArrest.ArrestId
                                          select new CrimeDetails
                                          {
                                              CrimeId = cr.CrimeId,
                                              CrimeCount = cr.CrimeCount,
                                              CrimeDescription = cr.CrimeLookup.CrimeDescription,
                                              CrimeSection = cr.CrimeLookup.CrimeSection,
                                              CrimeSubSection = cr.CrimeLookup.CrimeSubSection,
                                              CrimeStatueCode = cr.CrimeLookup.CrimeStatuteCode,
                                              CrimeType = cr.CrimeType,
                                              CrimeNcicCode = cr.CrimeLookup.CrimeNcicCode,
                                              CrimeUrcOffenceCode = cr.CrimeLookup.CrimeUcrOffenseCode
                                          }).ToList();
            });

            if (activeBookings)
            {
                isvm.ActiveBookings =
                    arrest.Where(arr => arr.ArrestActive).ToList();
            }

            if (releasedBookings)
            {
                isvm.ReleasedBookings =
                    arrest.Where(arr => !arr.ArrestActive).ToList();
            }
        }

        #endregion

        #region GetIncarcerationDetails

        /// <summary>
        /// To get the incarceration details of the inmate by inmate id.
        /// </summary>
        /// <param name="inmateId"></param>
        /// <returns></returns>
        private List<IncarcerationDetail> GetInmateIncarceration(int inmateId)
        {
            List<IncarcerationDetail> incarcarationList = (from inc in _context.Incarceration
                                                           where inc.InmateId == inmateId
                                                           select new IncarcerationDetail
                                                           {
                                                               IncarcerationId = inc.IncarcerationId,
                                                               ReleaseOut = inc.ReleaseOut,
                                                               ReleaseIn = inc.DateIn,
                                                               TransportHoldName = inc.TransportHoldName,
                                                               TransportHoldTypeId = inc.TransportHoldType,
                                                               TransportInstructions = inc.TransportInstructions,
                                                               TransportScheduleDate = inc.TransportScheduleDate,
                                                               TransportInmateCautions = inc.TransportInmateCautions,
                                                               TransportInmateBail = inc.TransportInmateBail
                                                           }).ToList();

            incarcarationList.ForEach(id =>
            {
                if (id.TransportHoldTypeId.HasValue && id.TransportHoldTypeId.Value != 0)
                {
                    id.TransportHoldType = _commonService.GetLookupList(LookupConstants.TRANSTYPE)
                        .Single(look => 
                        (int?)(look.LookupIndex) == id.TransportHoldTypeId).LookupDescription;
                }
            });

            return incarcarationList;
        }

        #endregion

        #region GetFloorNotesDetails

        /// <summary>
        /// To get floor notes of the inmate by inmate_id
        /// </summary>
        /// <param name="inmateId"></param>
        /// <returns></returns>
        private List<FloorNoteXrefDetails> GetFloorNotes(int inmateId)
        {
            List<FloorNoteXrefDetails> floorNoteXref = (from fnx in _context.FloorNoteXref
                                                        where fnx.InmateId == inmateId
                                                        select new FloorNoteXrefDetails
                                                        {
                                                            FloorNoteDate = fnx.FloorNote.FloorNoteDate,
                                                            FloorNoteTime = fnx.FloorNote.FloorNoteTime,
                                                            FloorNoteDescription = fnx.FloorNote.FloorNoteNarrative
                                                        }).ToList();
            return floorNoteXref;
        }

        #endregion

        #region GetDisciplinaryIncidentDetails

        /// <summary>
        /// to get disciplinary incident list of inmate by inmateid
        /// </summary>
        /// <param name="inmateId"></param>
        /// <returns></returns>
        private List<DisciplinaryIncidentDetails> GetDisciplinaryIncident(int inmateId)
        {
            List<DisciplinaryIncidentDetails> disciplinaryIncident =
            (from fnx in _context.DisciplinaryInmate
             where fnx.InmateId == inmateId
             select new DisciplinaryIncidentDetails
             {
                 DisciplinaryNumber = fnx.DisciplinaryIncident.DisciplinaryNumber,
                 DisciplinaryDate = fnx.DisciplinaryIncident.DisciplinaryIncidentDate,
                 DisciplinaryDescription = fnx.DisciplinaryViolationDescription
             }).ToList();
            return disciplinaryIncident;
        }

        #endregion

        #region GetInmateTrackDetails

        /// <summary>
        /// to get inmate track details(checkin and checkout details) of inmate by inmate id.
        /// </summary>
        /// <param name="inmateId"></param>
        /// <returns></returns>
        private List<InmateTrackDetails> GetInmateTrack(int inmateId)
        {
            List<InmateTrackDetails> inmateTrackDetails = (from it in _context.InmateTrak
                                                           where it.InmateId == inmateId
                                                           select new InmateTrackDetails
                                                           {
                                                               InmateTrackDateIn = it.InmateTrakDateIn,
                                                               //InmateTrackTimeIn = it.InmateTrakTimeIn,
                                                               InmateTrackDateOut = it.InmateTrakDateOut,
                                                               //InmateTrackTimeOut = it.InmateTrakTimeOut,
                                                               InmateTrackLocation = it.InmateTrakLocation
                                                           }).ToList();
            return inmateTrackDetails;
        }

        #endregion

        #region GetVisitorDetails

        /// <summary>
        /// to get the visitor details of inmate by inmate id.
        /// </summary>
        /// <param name="inmateId"></param>
        /// <returns></returns>
        private List<VisitorDetails> GetVisitorDetails(int inmateId)
        {
            List<VisitorDetails> visitDetails = (from vis in _context.VisitToVisitor
                                                 where vis.Visit.InmateId == inmateId
                                                 select new VisitorDetails
                                                 {
                                                     VisitorDateIn = vis.Visit.StartDate,
                                                     //  VisitorTimeIn = vis.VisitorTimeIn,
                                                     VisitorDateOut = vis.Visit.EndDate,
                                                     //VisitorTimeOut = vis.VisitorTimeOut,
                                                     //VisitorReason = vis.Visit.ReasonId,
                                                     VisitorFirstName = vis.Visitor.PersonFirstName,
                                                     VisitorLastName = vis.Visitor.PersonLastName
                                                 }).ToList();
            return visitDetails;
        }

        #endregion

        #region GetInmateClassification

        /// <summary>
        /// To get all active and inactive inmate classification details
        /// </summary>
        /// <param name="inmateId"></param>
        /// <returns></returns>
        private List<InmateClassificationVm> GetInmateClassification(int inmateId)
        {
            List<InmateClassificationVm> inmateClassificationList = (from ic in _context.InmateClassification
                                                                     where ic.InmateId == inmateId
                                                                     select new InmateClassificationVm
                                                                     {
                                                                         DateAssigned = ic.InmateDateAssigned,
                                                                         DateUnAssigned = ic.InmateDateUnassigned,
                                                                         ClassificationType = ic.InmateClassificationType,
                                                                         ClassificationReason = ic.InmateClassificationReason
                                                                     }).ToList();
            return inmateClassificationList;
        }

        #endregion

        #region GetInmateAppointment

        /// <summary>
        /// To get all active and inactive Inmate Appointment Details
        /// </summary>
        /// <param name="inmateId"></param>
        /// <returns></returns>
        private List<InmateAppointmentList> GetInmateAppointments(int inmateId)
        {
            //List<InmateAppointmentList> inmateAppointment = (from app in _context.Appointment
            //    where app.InmateId == inmateId
            //    select new InmateAppointmentList
            //    {
            //        ApptTime = app.AppointmentTime,
            //        ApptDate = app.AppointmentDate.Value,
            //        Duration = app.AppointmentDuration,
            //        ApptEndDate = app.AppointmentEnd,
            //        ApptLocation = app.AppointmentLocation
            //    }).ToList();
            //return inmateAppointment;
            return null;
        }

        #endregion

        #region GetHousingUnitMoveHistory

        /// <summary>
        /// To get all active and inactive inmate housing move history
        /// </summary>
        /// <param name="inmateId"></param>
        /// <returns></returns>
        private List<InmateHousingHistory> GetHousingUnitMoveHistory(int inmateId)
        {
            List<InmateHousingHistory> inmHousingHistory = (from humh in _context.HousingUnitMoveHistory
                                                            where humh.InmateId == inmateId
                                                            select new InmateHousingHistory
                                                            {
                                                                MoveOfficerId = humh.MoveOfficerId,
                                                                FromHousingUnitId = humh.HousingUnitFromId,
                                                                ToHousingUnitId = humh.HousingUnitToId,
                                                                Reason = humh.MoveReason,
                                                                HistoryMoveDate = humh.MoveDate
                                                            }).ToList();

            #region HousingUnitId's

            List<int> housingUnitIds =
                inmHousingHistory.Select(i => new[] { i.FromHousingUnitId, i.ToHousingUnitId })
                    .SelectMany(i => i)
                    .Where(i => i.HasValue)
                    .Select(i => i.Value)
                    .ToList();
            List<HousingDetail> housingLst = _facilityHousingService.GetHousingList(housingUnitIds);

            #endregion

            #region OfficerId's

            List<int> officerIds = inmHousingHistory.Select(i => new[] { i.MoveOfficerId })
                    .SelectMany(i => i)
                    .ToList();

            List<PersonnelVm> arrestOfficer =
                _personService.GetPersonNameList(officerIds.ToList());

            #endregion

            inmHousingHistory.ForEach(ihh =>
            {
                if (ihh.FromHousingUnitId.HasValue)
                {
                    ihh.FromHousing = housingLst.Single(hl => hl.HousingUnitId == ihh.FromHousingUnitId.Value);
                }
                if (ihh.ToHousingUnitId.HasValue)
                {
                    ihh.ToHousing = housingLst.Single(hl => hl.HousingUnitId == ihh.ToHousingUnitId.Value);
                }
                ihh.MoveOfficer = arrestOfficer.Single(arr => arr.PersonnelId == ihh.MoveOfficerId);
            });

            return inmHousingHistory;
        }

        #endregion

        #region GetKeepSeparateDetails

        /// <summary>
        /// To get keep separate details of the Inmate
        /// </summary>
        /// <param name="inmateId"></param>
        /// <returns></returns>
        private List<KeepSeparateVm> GetKeepSeparate(int inmateId)
        {
            IQueryable<KeepSeparate> keepSeparateLst = _context.KeepSeparate.Where(
                ks => ks.KeepSeparateInmate1Id == inmateId || ks.KeepSeparateInmate2Id == inmateId);

            List<KeepSeparateVm> keepSeparateDetails = (from ks in keepSeparateLst
                                                        where ks.KeepSeparateInmate1Id == inmateId
                                                        select new KeepSeparateVm
                                                        {
                                                            KeepSeparateNote = ks.KeepSeparateNote,
                                                            KeepSepType = ks.KeepSeparateType,
                                                            Reason = ks.KeepSeparateReason,
                                                            KeepSepInmateId = ks.KeepSeparateInmate2Id
                                                        }).ToList();

            keepSeparateDetails.AddRange((from ks in keepSeparateLst
                                          where ks.KeepSeparateInmate2Id == inmateId
                                          select new KeepSeparateVm
                                          {
                                              KeepSeparateNote = ks.KeepSeparateNote,
                                              KeepSepType = ks.KeepSeparateType,
                                              Reason = ks.KeepSeparateReason,
                                              KeepSepInmateId = ks.KeepSeparateInmate1Id
                                          }).ToList());

            LoadInmatesForKeepSep(keepSeparateDetails);

            return keepSeparateDetails;
        }


        #endregion

        #region GetAssociationDetails

        /// <summary>
        /// To get association details
        /// </summary>
        /// <param name="personId"></param>
        /// <returns></returns>
        private List<PersonClassificationDetails> GetAssociation(int personId)
        {
            List<Lookup> lookupslist = _context.Lookup.Where(w => w.LookupType == LookupConstants.CLASSGROUP).ToList();
            List<Lookup> lookupSubsetlist = _context.Lookup.Where(w => w.LookupType == LookupConstants.CLASSGROUPSUB).ToList();
            List<PersonClassificationDetails> pcDetails =
                _context.PersonClassification.Where(pc => pc.PersonId == personId).Select(pc =>
                    new PersonClassificationDetails
                    {
                        ClassificationType = lookupslist.Single(f => f.LookupIndex == pc.PersonClassificationTypeId).LookupDescription,
                        ClassificationSubset = lookupSubsetlist.Single(f => f.LookupIndex == pc.PersonClassificationSubsetId).LookupDescription,     
                        ClassificationNotes = pc.PersonClassificationNotes,
                        ClassificationStatus = pc.PersonClassificationStatus,
                        ClassificationTypeId = pc.PersonClassificationTypeId,
                        ClassificationSubsetId = pc.PersonClassificationSubsetId
                    }
                ).ToList();

            return pcDetails;
        }

        #endregion

        //To load the Person details, Housing and Facility Details of the keep separate Inmate
        private void LoadInmatesForKeepSep(List<KeepSeparateVm> keepSeparateDetails)
        {
            keepSeparateDetails.ForEach(ksd =>
                {
                    ksd.KeepSepInmateDetail = _personService.GetInmateDetails(ksd.KeepSepInmateId);

                    ksd.FacilityAbbr =
                        _commonService.GetFacilities().Single(fac => fac.FacilityId == ksd.KeepSepInmateDetail.FacilityId)
                            .FacilityAbbr;

                    ksd.HousingUnitId = ksd.KeepSepInmateDetail.HousingUnitId;

                    if (ksd.HousingUnitId.HasValue)
                    {
                        ksd.HousingDetail = _facilityHousingService.GetHousingDetails(ksd.HousingUnitId.Value);
                    }
                }
            );
        }
    }
}