﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ServerAPI.ViewModels;
using GenerateTables.Models;
using ServerAPI.Utilities;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using ScheduleWidget.Schedule;
using ScheduleWidget.Common;
using Microsoft.EntityFrameworkCore;

namespace ServerAPI.Services
{
    public class TransferService : ITransferService
    {
        private readonly AAtims _context;
        private readonly int _personnelId;
        private readonly ICommonService _commonService;
        private readonly IAppointmentService _appointmentService;
        private readonly IPhotosService _photosService;
        private readonly IHousingService _housingService;
        private readonly IKeepSepAlertService _keepSepAlertService;
        private readonly IInmateService _inmateService;
        private readonly IInterfaceEngineService _interfaceEngineService;

        public TransferService(AAtims context, IHttpContextAccessor httpContextAccessor, ICommonService commonService,
            IPhotosService photosService, IAppointmentService appointmentService, IInmateService inmateService,
            IHousingService housingService, IKeepSepAlertService keepSepAlertService,
            IInterfaceEngineService interfaceEngineService)
        {
            _context = context;
            _personnelId = Convert.ToInt32(httpContextAccessor.HttpContext.User
                .FindFirst(AuthDetailConstants.PERSONNELID)?.Value);
            _commonService = commonService;
            _appointmentService = appointmentService;
            _photosService = photosService;
            _housingService = housingService;
            _keepSepAlertService = keepSepAlertService;
            _inmateService = inmateService;
            _interfaceEngineService = interfaceEngineService;
        }

        #region Transfer Eligibles

        public TransEligibleDetailsVm GetTransferEligibles(EligibleSearchVm eligibleSearch)
        {
            TransEligibleDetailsVm transEligibleVm = new TransEligibleDetailsVm
            {
                TransEligibleDetails = GetTransEligibleDetails(eligibleSearch)
            };
            if (eligibleSearch.IsFlag)
            {
                transEligibleVm.LstLocation = _context.Privileges
                    .Where(p => p.TransferFlag == 1
                                && p.InactiveFlag == 0)
                    .OrderBy(p => p.PrivilegeDescription)
                    .Select(p => new PrivilegeDetailsVm
                    {
                        PrivilegeId = p.PrivilegeId,
                        PrivilegeDescription = p.PrivilegeDescription,
                        FacilityId = p.FacilityId
                    }).ToList();
                transEligibleVm.LstHousingDetails = _context.HousingUnit
                    .Where(h => h.FacilityId == eligibleSearch.FacilityId &&
                                (h.HousingUnitInactive == 0 || h.HousingUnitInactive == null))
                    .GroupBy(g => new
                    {
                        g.HousingUnitLocation,
                        g.HousingUnitNumber,
                        g.HousingUnitListId
                    })
                    .Select(h => new HousingDetail
                    {
                        HousingUnitLocation = h.Key.HousingUnitLocation,
                        HousingUnitNumber = h.Key.HousingUnitNumber,
                        HousingUnitListId = h.Key.HousingUnitListId
                    }).ToList();
            }
            return transEligibleVm;
        }

        //List of transfer Eligibles
        private List<TransferEligibleVm> GetTransEligibleDetails(EligibleSearchVm eligibleSearch)
        {
            List<TransferEligibleVm> lstTransEligible = _context.Incarceration
                 .Where(i => i.InmateId > 0 && i.Inmate.InmateActive == 1
                             && i.Inmate.FacilityId == eligibleSearch.FacilityId
                             && !i.ReleaseOut.HasValue
                             && (!eligibleSearch.Eligiblility.HasValue
                                 || i.TransferEligibleLookup == eligibleSearch.Eligiblility)
                             && (!eligibleSearch.Approval.HasValue
                                 || i.TransferApprovalLookup == eligibleSearch.Approval)
                             && (!eligibleSearch.Housing.HasValue
                                 || i.Inmate.HousingUnit.HousingUnitListId == eligibleSearch.Housing)
                             && (string.IsNullOrEmpty(eligibleSearch.Classify)
                                 || i.Inmate.InmateClassification.InmateClassificationReason == eligibleSearch.Classify)
                             && (!eligibleSearch.Gender.HasValue
                                 || i.Inmate.Person.PersonSexLast == eligibleSearch.Gender)
                             && (eligibleSearch.InmateId == 0
                                 || i.InmateId == eligibleSearch.InmateId)
                             && (eligibleSearch.PersonnelId == 0
                                 || i.TransferEligibleSaveBy == eligibleSearch.PersonnelId
                                 || i.TransferApprovalSaveBy == eligibleSearch.PersonnelId))
                 .Select(x => new TransferEligibleVm
                 {
                     IncarcerationId = x.IncarcerationId,
                     InmateId = x.Inmate.InmateId,
                     InmateNumber = x.Inmate.InmateNumber,
                     FacilityId = x.Inmate.FacilityId,
                     FacilityAbbr = x.Inmate.Facility.FacilityAbbr,
                     OverallSentStartDate = x.OverallSentStartDate,
                     OverallFinalReleaseDate = x.OverallFinalReleaseDate,
                     DateIn = x.DateIn,
                     EligibleLookup = x.TransferEligibleLookup,
                     EligibleDate = x.TransferEligibleDate,
                     EligibleNote = x.TransferEligibleNote,
                     ApprovalLookup = x.TransferApprovalLookup,
                     ApprovalDate = x.TransferApprovalDate,
                     ApprovalNote = x.TransferApprovalNote,
                     InmateClassificationId = x.Inmate.InmateClassificationId ?? 0,
                     ClassificationReason = x.Inmate.InmateClassificationId > 0
                         ? x.Inmate.InmateClassification.InmateClassificationReason
                         : default,
                     PersonDetails = new PersonInfoVm
                     {
                         PersonId = x.Inmate.PersonId,
                         PersonLastName = x.Inmate.Person.PersonLastName,
                         PersonFirstName = x.Inmate.Person.PersonFirstName,
                         PersonMiddleName = x.Inmate.Person.PersonMiddleName,
                         PersonSexLast = x.Inmate.Person.PersonSexLast
                     },
                     HousingDetail = x.Inmate.HousingUnitId > 0
                         ? new HousingDetail
                         {
                             HousingUnitId = x.Inmate.HousingUnit.HousingUnitId,
                             HousingUnitLocation = x.Inmate.HousingUnit.HousingUnitLocation,
                             HousingUnitNumber = x.Inmate.HousingUnit.HousingUnitNumber,
                             HousingUnitBedNumber = x.Inmate.HousingUnit.HousingUnitBedNumber,
                             HousingUnitBedLocation = x.Inmate.HousingUnit.HousingUnitBedLocation
                         } : default
                 }).OrderByDescending(i => i.EligibleLookup)
                 .ThenByDescending(a => a.ApprovalLookup)
                 .ThenByDescending(d => d.DateIn).ToList();

            //Appointment details
            List<AppointmentClass> lstAoSchedule = _context.Privileges.SelectMany(p => _context.ScheduleInmate
                .Where(a => a.LocationId == p.PrivilegeId && p.TransferFlag == 1 &&
                            (eligibleSearch.FutureAppt == FutureAppt.Today
                                ? a.StartDate.Date == DateTime.Now.Date
                                : eligibleSearch.FutureAppt != FutureAppt.Tomorrow ||
                                  a.StartDate.Date == DateTime.Now.Date.AddDays(1))
                            && (eligibleSearch.InternalId == 0
                                || a.LocationId == eligibleSearch.InternalId)
                            && (eligibleSearch.ExternalId == 0
                                || a.LocationId == eligibleSearch.ExternalId)
                            && (eligibleSearch.InternalId <= 0 && eligibleSearch.ExternalId <= 0 ||
                                a.StartDate.Date >= DateTime.Now.Date)
                ), (pr, ao) => new AppointmentClass
                {
                    InmateId = ao.InmateId,
                    ApptId = ao.ScheduleId,
                    ApptDate = ao.StartDate,
                    ApptEndDate = ao.EndDate ?? null,
                    ApptLocation = pr.PrivilegeDescription,
                    Duration = ao.Duration,
                    DayInterval = ao.DayInterval,
                    WeekInterval = ao.WeekInterval,
                    FrequencyType = ao.FrequencyType,
                    QuarterInterval = ao.QuarterInterval,
                    MonthOfQuarterInterval = ao.MonthOfQuarterInterval,
                    MonthOfYear = ao.MonthOfYear,
                    DayOfMonth = ao.DayOfMonth,
                    IsSingleOccurrence = ao.IsSingleOccurrence,
                    DeleteFlag = ao.DeleteFlag,

                }).ToList();

            if (eligibleSearch.FutureAppt != FutureAppt.All
                || eligibleSearch.ExternalId > 0
                || eligibleSearch.InternalId > 0
                || eligibleSearch.None)
            {
                List<int> inmateIdLst = lstAoSchedule.Select(s => s.InmateId ?? 0).ToList();
                lstTransEligible = lstTransEligible.Where(w => eligibleSearch.None
                        ? !inmateIdLst.Contains(w.InmateId)
                        : inmateIdLst.Contains(w.InmateId)).ToList();
            }

            //Flag Alert details
            //List<LookupVm> lstLookup = _commonService.GetLookups(new[]
            //    {LookupConstants.PERSONCAUTION, LookupConstants.TRANSCAUTION,LookupConstants.DIET,LookupConstants.MEDFLAG});

            if (eligibleSearch.DietFlagId.HasValue || eligibleSearch.PersonFlagId.HasValue ||
                eligibleSearch.InmateFlagId.HasValue)
            {
                List<FlagAlertVm> dbFlagAlertLst = _context.PersonFlag
                    .Where(w => w.DeleteFlag == 0)
                    .Select(prf => new FlagAlertVm
                    {
                        // PersonFlagId will be shown only for active records
                        PersonId = prf.PersonId,
                        PersonFlagId = prf.DeleteFlag == 0 ? prf.PersonFlagId : 0,
                        PersonFlagIndex = prf.PersonFlagIndex ?? 0,
                        InmateFlagIndex = prf.InmateFlagIndex ?? 0,
                        DietFlagIndex = prf.DietFlagIndex ?? 0,
                        MedicalFlagIndex = prf.MedicalFlagIndex ?? 0
                    }).ToList();
                List<int> personIds = dbFlagAlertLst.Where(w =>
                        (eligibleSearch.DietFlagId.HasValue ? w.DietFlagIndex == eligibleSearch.DietFlagId : default)
                        || (eligibleSearch.MedFlagId.HasValue ? w.MedicalFlagIndex == eligibleSearch.MedFlagId : default)
                        || (eligibleSearch.PersonFlagId.HasValue ? w.PersonFlagIndex == eligibleSearch.PersonFlagId : default)
                        || (eligibleSearch.InmateFlagId.HasValue ? w.InmateFlagIndex == eligibleSearch.InmateFlagId : default))
                    .Select(s => s.PersonId).ToList();

                lstTransEligible = lstTransEligible
                    .Where(w => personIds.Contains(w.PersonDetails.PersonId)).ToList();
            }
            List<AppointmentClass> inmateApptList = new List<AppointmentClass>();

            if (!eligibleSearch.None)
                foreach (AppointmentClass sch in lstAoSchedule)
                {
                    ScheduleBuilder schBuilder = new ScheduleBuilder();
                    schBuilder.StartDate(sch.ApptDate);
                    if (sch.ApptEndDate.HasValue)
                    {
                        schBuilder.EndDate(sch.ApptEndDate.Value);
                    }
                    else if (!sch.ApptEndDate.HasValue)
                    {
                        schBuilder.EndDate(sch.ApptDate.Date.AddHours((sch.ApptDate.TimeOfDay + sch.Duration).Hours)
                            .AddMinutes((sch.ApptDate.TimeOfDay + sch.Duration).Minutes));
                    }

                    ISchedule materializedSchedule = schBuilder
                        .Duration(sch.Duration)
                        .SingleOccurrence(sch.IsSingleOccurrence)
                        .OnDaysOfWeek(sch.DayInterval)
                        .DuringMonth(sch.WeekInterval)
                        .DuringMonthOfQuarter(sch.MonthOfQuarterInterval)
                        .DuringQuarter(sch.QuarterInterval)
                        .HavingFrequency(sch.FrequencyType)
                        .Create();


                    DateRange during = new DateRange(materializedSchedule.StartDate,
                        sch.ApptEndDate.HasValue ? materializedSchedule.EndDate.Value : DateTime.Now.AddYears(5));

                    if (materializedSchedule.IsSingleOccurrence)
                    {
                        inmateApptList.Add(new AppointmentClass
                        {
                            ApptDate = materializedSchedule.StartDate,
                            ApptEndDate = materializedSchedule.EndDate,
                            ApptId = sch.ApptId,
                            InmateId = sch.InmateId,
                            Duration = materializedSchedule.EndDate - materializedSchedule.StartDate ??
                                       new TimeSpan(0, 0, 0),
                            IsSingleOccurrence = sch.IsSingleOccurrence,
                            DeleteFlag = sch.DeleteFlag,

                        });
                    }
                    else
                    {
                        inmateApptList.AddRange(materializedSchedule.Occurrences(during)
                            .Select(date => new AppointmentClass
                            {
                                ApptDate = date.Date.Add(during.StartDateTime.TimeOfDay),
                                ApptEndDate = date.Date.Add(during.EndDateTime.TimeOfDay),
                                ApptId = sch.ApptId,
                                InmateId = sch.InmateId,
                                Duration = materializedSchedule.Duration,
                                IsSingleOccurrence = sch.IsSingleOccurrence,
                                DeleteFlag = sch.DeleteFlag,
                            }));
                    }
                }
            lstTransEligible.ForEach(item =>
            {
                item.Appointment = inmateApptList.Where(x => x.InmateId == item.InmateId && !x.DeleteFlag).ToList();
            }
            );
            return lstTransEligible;
        }

        public Task<int> UpdateIncarceration(TransferEligibleVm eligible)
        {
            Incarceration dbIncar = _context.Incarceration
                .Single(a => a.IncarcerationId == eligible.IncarcerationId);
            if (eligible.EligibleLookup > 0)
            {
                dbIncar.TransferEligibleLookup = eligible.EligibleLookup;
                dbIncar.TransferEligibleDate = eligible.EligibleDate;
                dbIncar.TransferEligibleNote = eligible.EligibleNote;
                dbIncar.TransferEligibleSaveDate = DateTime.Now;
                dbIncar.TransferEligibleSaveBy = _personnelId;
            }
            if (eligible.ApprovalLookup > 0)
            {
                dbIncar.TransferApprovalLookup = eligible.ApprovalLookup;
                dbIncar.TransferApprovalDate = eligible.ApprovalDate;
                dbIncar.TransferApprovalNote = eligible.ApprovalNote;
                dbIncar.TransferApprovalSaveDate = DateTime.Now;
                dbIncar.TransferApprovalSaveBy = _personnelId;
            }
            InsertTransferSaveHistory(eligible);
            return _context.SaveChangesAsync();
        }

        private void InsertTransferSaveHistory(TransferEligibleVm eligible)
        {
            IncarcerationTransferSaveHistory tranHistoryDb = new IncarcerationTransferSaveHistory
            {
                Incarcerationid = eligible.IncarcerationId,
            };
            if (eligible.EligibleLookup > 0)
            {
                tranHistoryDb.TransferEligibleLookup = eligible.EligibleLookup;
                tranHistoryDb.TransferEligibleDate = eligible.EligibleDate;
                tranHistoryDb.TransferEligibleNote = eligible.EligibleNote;
                tranHistoryDb.TransferEligibleSaveDate = DateTime.Now;
                tranHistoryDb.TransferEligibleSaveBy = _personnelId;
            }

            if (eligible.ApprovalLookup > 0)
            {
                tranHistoryDb.TransferApprovalLookup = eligible.ApprovalLookup;
                tranHistoryDb.TransferApprovalDate = eligible.ApprovalDate;
                tranHistoryDb.TransferApprovalNote = eligible.ApprovalNote;
                tranHistoryDb.TransferApprovalSaveDate = DateTime.Now;
                tranHistoryDb.TransferApprovalSaveBy = _personnelId;
            }
            _context.IncarcerationTransferSaveHistory.Add(tranHistoryDb);
        }

        public List<TransferHistoryVm> GetTransferHistoryDetails(int incarcerationId)
        {
            List<TransferHistoryVm> lstTransfer = _context.IncarcerationTransferSaveHistory
                .OrderByDescending(i => i.IncarcerationTransferSaveHistoryid)
                .Where(i => i.Incarcerationid == incarcerationId)
                .Select(i => new TransferHistoryVm
                {
                    IncarcerationId = i.Incarcerationid,
                    EligibleLookup = i.TransferEligibleLookup,
                    ApprovalLookup = i.TransferApprovalLookup,
                    EligibleDate = i.TransferEligibleDate,
                    EligibleNote = i.TransferEligibleNote,
                    ApprovalDate = i.TransferApprovalDate,
                    EligibleSaveDate = i.TransferEligibleSaveDate,
                    EligibleSaveBy = i.TransferEligibleSaveByNavigation.PersonId,
                    ApprovalNote = i.TransferApprovalNote,
                    ApprovalSaveDate = i.TransferApprovalSaveDate,
                    ApprovalSaveBy = i.TransferApprovalSaveByNavigation.PersonId
                }).ToList();

            List<int?> lstApprovBy = lstTransfer.Select(x => x.ApprovalSaveBy).ToList();
            lstApprovBy.AddRange(lstTransfer.Select(x => x.EligibleSaveBy).ToList());
            IQueryable<PersonnelVm> lstPersonDetails = _context.Person.Where(x => lstApprovBy.Contains(x.PersonId))
                .Select(x => new PersonnelVm
                {
                    PersonId = x.PersonId,
                    PersonLastName = x.PersonLastName,
                    PersonFirstName = x.PersonFirstName,
                    PersonMiddleName = x.PersonMiddleName
                });

            lstTransfer.ForEach(item =>
            {
                item.ApprovalDetails = lstPersonDetails.SingleOrDefault(x => x.PersonId == item.ApprovalSaveBy);
                item.EligibleDetails = lstPersonDetails.SingleOrDefault(x => x.PersonId == item.EligibleSaveBy);
            });

            return lstTransfer;
        }

        public EligibleInmateCountVm GetInmateCount(int inmateId, int personId)
        {
            EligibleInmateCountVm inmateCountVm = new EligibleInmateCountVm
            {
                Counts = GetCounts(inmateId, personId)
            };
            return inmateCountVm;
        }

        private List<KeyValuePair<int, string>> GetCounts(int inmateId, int personId)
        {
            List<KeyValuePair<int, string>> inmateCount = new List<KeyValuePair<int, string>>();

            //CHARGES
            int charges = 0;

            //To get Inmate BookingNo details for Booking dropdown
            List<InmateBookingNumberDetails> lstArrestId =
                _context.IncarcerationArrestXref.Where(i => i.Incarceration.InmateId == inmateId)
                    .OrderByDescending(i => i.Incarceration.DateIn)
                    .Select(i => new InmateBookingNumberDetails
                    {
                        ArrestId = i.ArrestId
                    }).ToList();
            lstArrestId.ForEach(a =>
            {
                //To get list from Crime for Charges count
                int crimeCnt = _context.Crime
                    .Count(c => c.CrimeLookupId > 0 && c.CrimeDeleteFlag == 0 && !c.WarrantId.HasValue &&
                                c.Arrest.ArrestId == a.ArrestId);

                //To get list from CrimeForce for Charges count
                int crimeForceCnt =
                    _context.CrimeForce.Count(
                        c => c.DeleteFlag == 0 && !c.DropChargeFlag.HasValue && !c.ForceCrimeLookupId.HasValue &&
                             !c.WarrantId.HasValue && !c.ForceSupervisorReviewFlag.HasValue &&
                             c.ArrestId == a.ArrestId);

                charges += crimeCnt + crimeForceCnt;
            });

            inmateCount.Add(new KeyValuePair<int, string>(charges, ClassTypeConstants.CHARGES));

            //INCIDENT
            inmateCount.Add(new KeyValuePair<int, string>(
                _context.DisciplinaryInmate.Count(d => d.InmateId == inmateId), ClassTypeConstants.INCIDENT));

            //PRIVILEGE
            inmateCount.Add(new KeyValuePair<int, string>(_context.InmatePrivilegeXref.Count(
                ip => ip.Privilege.InactiveFlag == 0 && !ip.PrivilegeRemoveOfficerId.HasValue &&
                      (ip.PrivilegeExpires >= DateTime.Now || !ip.PrivilegeExpires.HasValue)
                      && ip.InmateId == inmateId), ClassTypeConstants.PRIVILEGE));

            //KEEPSEP
            //To get keepsepInmate count for KeepSeparation
            int keepsepInmate = _context.KeepSeparate
                .Count(ks => (ks.KeepSeparateInmate1Id == inmateId
                              || ks.KeepSeparateInmate2Id == inmateId)
                             && ks.InactiveFlag == 0 && ks.KeepSeparateInmate1.InmateActive == 1 &&
                             ks.KeepSeparateInmate2.InmateActive == 1);

            //To get keepsepAssoc count for KeepSeparation
            int keepsepAssoc =
                _context.KeepSepAssocInmate.Count(a => a.KeepSepInmate2Id == inmateId && a.DeleteFlag == 0);

            //To get keepsepSubset count for KeepSeparation
            int keepsepSubset =
                _context.KeepSepSubsetInmate.Count(s => s.KeepSepInmate2Id == inmateId && s.DeleteFlag == 0);

            inmateCount.Add(new KeyValuePair<int, string>(keepsepInmate + keepsepAssoc + keepsepSubset,
                ClassTypeConstants.KEEPSEP));

            //ASSOCIATION
            //To get LookupDescription in Lookup for Association count
            int[] lookupClassGroup = _commonService.GetLookupList(LookupConstants.CLASSGROUP)
                .Select(a => a.LookupIndex).ToArray();

            inmateCount.Add(new KeyValuePair<int, string>(_context.PersonClassification.Count(pc =>
                pc.InactiveFlag == 0 && pc.PersonId == personId &&
                (!pc.PersonClassificationDateThru.HasValue ||
                 pc.PersonClassificationDateThru.Value.Date >= DateTime.Now.Date) &&
                lookupClassGroup.Contains(pc.PersonClassificationTypeId ?? 0)), ClassTypeConstants.ASSOC));

            //HOUSING
            inmateCount.Add(new KeyValuePair<int, string>(
                _context.HousingUnitMoveHistory.Count(h => h.InmateId == inmateId), ClassTypeConstants.HOUSING));

            //CLASSIFY
            inmateCount.Add(new KeyValuePair<int, string>(_context.InmateClassification
                .Count(i => i.InmateId == inmateId), ClassifyQueueConstants.CLASSIFY));

            //FLAG
            inmateCount.Add(new KeyValuePair<int, string>(
                _context.PersonFlag.Count(a => a.PersonId == personId && a.DeleteFlag == 0 &&
                                               (a.PersonFlagIndex > 0 || a.InmateFlagIndex > 0 ||
                                                a.DietFlagIndex > 0))
                , ClassTypeConstants.FLAG));

            return inmateCount;
        }

        #endregion

        #region Internal Transfer

        public InternalTransferVm GetInternalTransfer(DateTime transferDate, int facilityId, bool inProgress)
        {
            InternalTransferVm internalTransfer = new InternalTransferVm
            {
                ScheduleDetails =
                    _appointmentService.ListAoAppointments(facilityId, null, transferDate, transferDate, false,
                        inProgress)
            };
            if (!internalTransfer.ScheduleDetails.Any()) return internalTransfer;
            int[] locationId = internalTransfer.ScheduleDetails.Select(aos => aos.LocationId).ToArray();

            List<int> transLocationIds = _context.Privileges
                .Where(pri => locationId.Contains(pri.PrivilegeId) && pri.FacilityId.HasValue && pri.TransferFlag == 1)
                .Select(pri => pri.PrivilegeId).ToList();

            internalTransfer.ScheduleDetails = internalTransfer.ScheduleDetails.Where(app =>
                    facilityId > 0
                        ? app.InmateDetails.FacilityId == facilityId && transLocationIds.Contains(app.LocationId)
                        : transLocationIds.Contains(app.LocationId))
                .ToList();


            List<int> selectedPersonIds = internalTransfer.ScheduleDetails.Select(ina => ina.InmateDetails.PersonId)
                .Distinct().ToList();
            List<Identifiers> identifierLst = (from idf in _context.Identifiers
                                               where idf.IdentifierType == "1" && idf.DeleteFlag == 0
                                                     && selectedPersonIds.Contains(idf.PersonId ?? 0)
                                               select new Identifiers
                                               {
                                                   PersonId = idf.PersonId,
                                                   IdentifierId = idf.IdentifierId,
                                                   PhotographRelativePath = _photosService.GetPhotoByIdentifier(idf)
                                               }).OrderByDescending(o => o.IdentifierId).ToList();
            internalTransfer.ScheduleDetails.ForEach(item =>
            {
                item.InmateDetails.PhotoPath = identifierLst.FirstOrDefault(idn =>
                    idn.PersonId == item.InmateDetails.PersonId)?.PhotographRelativePath;
                item.KeepSepDetails = _keepSepAlertService.GetInmateKeepSep(item.InmateId ?? 0);
            });

            internalTransfer.LocationList = internalTransfer.ScheduleDetails
                .GroupBy(app => new { app.LocationId, app.StartDate, app.Location })
                .Select(ap => new TransferCountDetails
                {
                    LocationId = ap.Key.LocationId,
                    Count = ap.Count(),
                    Description = ap.Key.Location,
                    AppointmentDate = ap.Key.StartDate
                }).ToList();

            internalTransfer.HousingList = internalTransfer.ScheduleDetails.GroupBy(app =>
                    new { app.InmateDetails.HousingUnitId, app.InmateDetails.HousingUnitBedLocation, app.StartDate })
                .Select(ap => new TransferCountDetails
                {
                    HousingUnitId = ap.Key.HousingUnitId,
                    Count = ap.Count(),
                    FacilityAbbr = ap.Select(s => s.InmateDetails.FacilityAbbr).FirstOrDefault(),
                    HousingLocation = ap.Select(s => s.InmateDetails.HousingLocation).FirstOrDefault(),
                    HousingNumber = ap.Select(s => s.InmateDetails.HousingNumber).FirstOrDefault(),
                    AppointmentDate = ap.Key.StartDate
                }).ToList();
            return internalTransfer;
        }

        #endregion

        #region  External Transfer

        public ExternalTransferVm GetLocationCountDetails(int facilityId, DateTime startDate)
        {
            ExternalTransferVm externalDetails = new ExternalTransferVm();
            List<TransferCountDetails> locationDetails = new List<TransferCountDetails>();
            List<TransferCountDetails> scheduleDetails = new List<TransferCountDetails>();
            List<InmateVm> inmates = _context.Inmate.Where(w => 
                    w.FacilityId == facilityId
                    && w.InmateActive == 1).Select(s =>
                    new InmateVm
                    {
                        InmateId = s.InmateId,
                        InmateCurrentTrack = s.InmateCurrentTrackNavigation.PrivilegeDescription,
                        InmateCurrentTrackId = s.InmateCurrentTrackId,
                        HousingUnitId = s.HousingUnitId ?? 0
                    }).ToList();
            locationDetails.Add(
                new TransferCountDetails
                {
                    Description = string.Empty,
                    Count = inmates.Count(c => c.HousingUnitId == 0),
                    LocationId = 0
                });
            locationDetails.Add(
                new TransferCountDetails
                {
                    Description = string.Empty,
                    Count = inmates.Count(c => c.HousingUnitId == 0 && !c.InmateCurrentTrackId.HasValue),
                    LocationId = -1
                });
            locationDetails.AddRange(inmates.Where(w => w.HousingUnitId == 0 && w.InmateCurrentTrackId.HasValue).GroupBy(
                x => x.InmateCurrentTrack
            ).Select(s =>
                new TransferCountDetails
                {
                    Description = s.Select(x => x.InmateCurrentTrack).FirstOrDefault(),
                    Count = s.Count(),
                    LocationId = s.Select(x => x.InmateCurrentTrackId).FirstOrDefault() ?? 0
                }));
            List<AoAppointmentVm> appointmentDetails = _appointmentService.ListAoAppointments(facilityId,
               null, startDate, startDate, false);
            if (appointmentDetails.Any())
            {
                int[] locationId = appointmentDetails.Select(s => s.LocationId).Distinct().ToArray();
                List<int> transLocationIds = _context.Privileges.Where(w =>
                        locationId.Contains(w.PrivilegeId)
                        && w.TransferFlag == 1
                        && (!w.FacilityId.HasValue || w.FacilityId == 0)
                        && w.InactiveFlag == 0)
                    .Select(s => s.PrivilegeId)
                    .ToList();
                appointmentDetails = appointmentDetails.Where(w => transLocationIds.Contains(w.LocationId)
                                                        && w.InmateDetails.FacilityId == facilityId
                                                        && w.InmateDetails.InmateActive == 1)
                                                        .ToList();
                scheduleDetails = appointmentDetails.GroupBy(g => new { g.LocationId, g.StartDate, g.Location })
                    .Select(ap => new TransferCountDetails
                    {
                        LocationId = ap.Key.LocationId,
                        //Count = ap.Count(c => c.TrackId == null),
                        Description = ap.Key.Location,
                        AppointmentDate = ap.Key.StartDate
                    }).ToList();
            }

            externalDetails.LocationDetails = locationDetails;
            externalDetails.ScheduleDetails = scheduleDetails;
            return externalDetails;
        }

        public ExternalTransferVm GetLocationInmateDetails(int locationId, int facilityId, bool isAppointment, DateTime startDate)
        {

            List<Lookup> lookupDetails = _context.Lookup
                .Where(w => w.LookupInactive == 0 
                && (w.LookupType == LookUpType.LIBBOOKTYPE.ToString() 
                || w.LookupType == LookUpType.LIBBOOKCAT.ToString() 
                || w.LookupType == LookUpType.LIBBOOKCOND.ToString())).ToList();
            List<LibraryBook> libraryDetails = _context.LibraryBook
                .Where(w => w.DeleteFlag == 0 
                && w.LibraryRoom.DeleteFlag == 0
                && (!w.LibraryRoomLocationId.HasValue || w.LibraryRoomLocation.DeleteFlag == 0)).ToList();
            libraryDetails = libraryDetails.SelectMany(s => lookupDetails.Where(w =>
               w.LookupType == LookUpType.LIBBOOKTYPE.ToString() && w.LookupIndex == s.BookTypeId
               || w.LookupType == LookUpType.LIBBOOKCAT.ToString() && w.LookupIndex == s.BookCategoryId
               || w.LookupType == LookUpType.LIBBOOKCOND.ToString() && w.LookupIndex == s.CurrentConditionId),
               (s, w) => s).Distinct().ToList();
            List<InmateSupplyLookupDetails> inmateSupplyLookupDetails = _context.InmateSupplySizeLookup.Where(w =>
                    w.InmateSupplyLookup.FacilityId == facilityId).GroupBy(g =>new
                    {
                        g.InmateSupplyLookup.ItemDescription,
                        g.InmateSupplyLookup.ShirtFlag,
                        g.InmateSupplyLookup.ShoesFlag,
                        g.InmateSupplyLookup.OtherFlag,
                        g.InmateSupplyLookup.BraFlag,
                        g.InmateSupplyLookup.PantsFlag,
                        g.InmateSupplyLookup.UnderwearFlag,
                        g.InmateSupplySizeLookupId,
                        g.InmateSupplyLookup.DoNotTransferFlag
                    }).Select(s =>
                    new InmateSupplyLookupDetails
                    {
                        InmateSupplySizelookupId = s.Key.InmateSupplySizeLookupId,
                        ItemDescription = s.Key.ItemDescription,
                        ShirtFlag = s.Key.ShirtFlag,
                        ShoeFlag = s.Key.ShoesFlag,
                        OtherFlag = s.Key.OtherFlag,
                        BraFlag = s.Key.BraFlag,
                        PantFlag = s.Key.PantsFlag,
                        UnderwearFlag = s.Key.UnderwearFlag,
                        DoNotTransferFlag = s.Key.DoNotTransferFlag
                    }).ToList();
            inmateSupplyLookupDetails.ForEach(f =>
            {
                f.ItemDescription = f.ShirtFlag == 1 ? InmateSupplyLookUp.SHIRTS.ToString() :
                    f.ShoeFlag == 1 ? InmateSupplyLookUp.SHOES.ToString() :
                    f.BraFlag == 1 ? InmateSupplyLookUp.BRAS.ToString() :
                    f.PantFlag == 1 ? InmateSupplyLookUp.PANTS.ToString() :
                    f.UnderwearFlag == 1 ? InmateSupplyLookUp.UNDERWEAR.ToString() : string.Empty;
            });
            List<InmateSupply> supplyDetails = _context.InmateSupply
                .Where(w => !w.DeleteFlag.HasValue || w.DeleteFlag == 0).ToList();
            List<InmateSupplyDetails> inmateSupplyDetails = inmateSupplyLookupDetails.SelectMany(p =>
                                        supplyDetails.Where(w => w.InmateSupplySizeLookupId == p.InmateSupplySizelookupId
                                        && (!w.CheckinCount.HasValue || w.CheckinCount == 0)),
                                            (p, w) => new InmateSupplyDetails
                                            {
                                                InmateId = w.InmateId,
                                                CheckOutCount = w.CheckoutCount ?? 0,
                                                DoNotTransferFlag = p.DoNotTransferFlag,
                                                ItemDescription = p.ItemDescription
                                            }).ToList();
            List<int> issuedPropertyDetails = _context.IssuedProperty
                .Where(w => w.ActiveFlag == 1 && w.DeleteFlag == 0).Select(s=>s.InmateId ?? 0).ToList();
            List<AoAppointmentVm> appointmentDetails = new List<AoAppointmentVm>();
            if (isAppointment)
            {
                appointmentDetails = _appointmentService.ListAoAppointments(facilityId,
                    null, startDate, startDate, false);
                if (appointmentDetails.Any())
                {
                    appointmentDetails = appointmentDetails
                    .Where(w => w.LocationId == locationId && w.InmateDetails.FacilityId == facilityId
                    && w.StartDate.Hour == startDate.Hour && w.StartDate.Minute == startDate.Minute).ToList();
                }
            }
            List<InmateTransferDetails> inmateDetails = _context.Inmate.Where(w =>
                    w.InmateActive == 1
                    && (isAppointment 
                        ? appointmentDetails.Any(c => c.InmateId == w.InmateId)
                        : (!w.HousingUnitId.HasValue || w.HousingUnitId == 0) && w.FacilityId == facilityId)
                    ).Select(s => new InmateTransferDetails
                    {
                    InmateId = s.InmateId,
                    InmateNumber = s.InmateNumber.Trim(),
                    FacilityId = s.FacilityId,
                    PersonId = s.PersonId,
                    InmLastName = s.Person.PersonLastName,
                    InmMiddleName = s.Person.PersonMiddleName,
                    InmFirstName = s.Person.PersonFirstName,
                    PersonalInventory = s.InmatePersonalInventory,
                    CurrentTrack = s.InmateCurrentTrack,
                    InmateCurrentTrackId = s.InmateCurrentTrackId,
                    Housing = isAppointment && s.HousingUnitId > 0 ? new HousingDetail
                    {
                        HousingUnitId = s.HousingUnitId ?? 0,
                        HousingUnitLocation = s.HousingUnit.HousingUnitLocation,
                        HousingUnitNumber = s.HousingUnit.HousingUnitNumber,
                        HousingUnitBedLocation = s.HousingUnit.HousingUnitBedLocation,
                        HousingUnitBedNumber = s.HousingUnit.HousingUnitBedNumber,
                        HousingUnitListId = s.HousingUnit.HousingUnitListId
                    } : new HousingDetail()
                    }).OrderBy(o =>
                    o.InmLastName + ", " + o.InmMiddleName + " " + o.InmFirstName)
                .ToList();
            //Library Track Details
            List<LibraryBook> libraryBookLst = GetCounts();

            //For IsIncompleteTasks 
            List<KeyValuePair<int, int>> lstAoTaskLookups = _context.AoTaskLookupAssign.Where(lookAssign =>
                    lookAssign.TaskValidateLookup == _commonService.GetValidationType(TaskValidateType.HousingAssignFromTransfer)
                    && !lookAssign.DeleteFlag
                    && !lookAssign.AoTaskLookup.DeleteFlag)
                .Select(lookAssign => new KeyValuePair<int, int>(lookAssign.AoTaskLookupId,
                    lookAssign.FacilityId)).ToList();

            List<AoTaskQueue> lstAoTaskQueue = _context.AoTaskQueue.Where(w=> !w.CompleteFlag).ToList();
            AoAppointmentVm appointment;
            inmateDetails.ForEach(item =>
            {
                if (isAppointment)
                {
                    appointment = appointmentDetails.FirstOrDefault(f => f.InmateId == item.InmateId);
                    if (appointment != null)
                    {
                        item.ScheduleTime = appointment.StartDate;
                        item.ScheduleLocation = appointment.Location;
                        item.ScheduleLocationId = appointment.LocationId;
                        item.ScheduleId = appointment.ScheduleId;
                    }
                }
                inmateSupplyDetails = inmateSupplyDetails.Where(w => w.InmateId == item.InmateId).ToList();
                item.TransferSupply = inmateSupplyDetails.Where(w =>w.DoNotTransferFlag == 0).GroupBy(g =>
                    new {g.CheckOutCount, g.ItemDescription}).Select(se => new KeyValuePair<int?, string>(se
                    .Sum(x => x.CheckOutCount), se.Key.ItemDescription)).OrderBy(o =>
                    new {o.Key, o.Value}).ToList();
                item.NoTransferSupply = inmateSupplyDetails.Where(w =>w.DoNotTransferFlag == 1).GroupBy(g =>
                    new {g.CheckOutCount, g.ItemDescription}).Select(se =>new KeyValuePair<int?, string>(se
                    .Sum(x => x.CheckOutCount), se.Key.ItemDescription)).OrderBy(o => 
                    new {o.Key, o.Value}).ToList();
                libraryBookLst = libraryBookLst.Where(w=>w.CurrentCheckoutInmateId == item.InmateId).ToList();
                item.SupplyCnt = supplyDetails.Where(w => w.InmateId == item.InmateId && !w.CheckinDate.HasValue)
                    .Sum(su => su.CheckoutCount);
                item.IssuedPropertyCnt = issuedPropertyDetails.Count(c => c == item.InmateId);
                item.BookCnt = libraryDetails.Count(c => c.CurrentCheckoutInmateId == item.InmateId);
                item.Count = libraryBookLst.Count();
                item.LateCount = libraryBookLst.Count(x => x.CurrentSchdReturn <= DateTime.Now);
                int[] arrTaskIds = lstAoTaskLookups
                    .Where(w => w.Value == item.FacilityId).Select(task => task.Key).ToArray();
                item.IsIncompleteTasks = lstAoTaskQueue.Count(c =>
                    c.InmateId == item.InmateId && arrTaskIds.Contains(c.AoTaskLookupId)) > 0;
            });
            ExternalTransferVm lstInmateDetails = new ExternalTransferVm();
            lstInmateDetails.AllInmateDetails = inmateDetails;
            if (locationId == -1)
            {
                inmateDetails = inmateDetails
                    .Where(w => !w.InmateCurrentTrackId.HasValue || w.InmateCurrentTrackId == 0).ToList();
            }
            else if (locationId != 0 && !isAppointment)
            {
                inmateDetails = inmateDetails.Where(w => w.InmateCurrentTrackId == locationId).ToList();
            }
            lstInmateDetails.InmateDetails = inmateDetails;
            return lstInmateDetails;
        }

        public List<KeyValuePair<string, int>> GetExternalLocations()
        {
            List<KeyValuePair<string, int>> externalLocationList = _context.Privileges
                .Where(w => w.InactiveFlag == 0 && w.TransferFlag == 1 && (!w.FacilityId.HasValue || w.FacilityId == 0))
                .Select(s => new KeyValuePair<string, int>(s.PrivilegeDescription, s.PrivilegeId))
                .ToList();
            return externalLocationList;
        }

        public List<KeyValuePair<string, int>> GetInventoryBinList()
        {
            List<KeyValuePair<string, int>> inventoryBinList = _context.PersonalInventoryBin
                .Where(w => (!w.InActiveFlag.HasValue || w.InActiveFlag == 0) && w.FacilityTransferFlag == 1)
                .Select(s => new KeyValuePair<string, int>(s.BinName, s.PersonalInventoryBinId))
                .Distinct()
                .ToList();
            return inventoryBinList;
        }

        public async Task<int> IssuedProperty(List<ExternalSearchVm> externalSearch, int isFlag)
        {
            switch (isFlag)
            {
                case 1:
                    //Supply Details  
                    externalSearch.ForEach(item =>
                    {
                        List<InmateSupplyVm> lstInmateSupply = _context.InmateSupply
                            .Where(x => x.InmateId == item.InmateId && !x.CheckinDate.HasValue && x.DeleteFlag != 1)
                            .Select(s => new InmateSupplyVm
                            {
                                InmateSupplySizeLookupId = s.InmateSupplySizeLookupId,
                                CheckoutCount = s.CheckoutCount,
                                CheckInCount = s.CheckinCount,
                                CheckBy = s.CheckBy,
                                CheckInDate = s.CheckinDate,
                                UpdatedBy = s.UpdatedBy,
                                InmateId = s.InmateId
                            }).ToList();

                        // Update InmateSupplySizeLookup from InmateSupply
                        lstInmateSupply.ForEach(a =>
                        {
                            InmateSupplySizeLookup sizeLookup = _context.InmateSupplySizeLookup.Single(s =>
                                s.InmateSupplySizeLookupId == a.InmateSupplySizeLookupId);
                            sizeLookup.QtyCheckedOut = sizeLookup.QtyCheckedOut - a.CheckoutCount;
                            sizeLookup.QtyOnHand = sizeLookup.QtyTotal - (sizeLookup.QtyCheckedOut - a.CheckoutCount);
                            sizeLookup.UpdateDate = DateTime.Now;
                            sizeLookup.UpdatedBy = _personnelId;

                            // Update InmateSupply
                            a.CheckInCount = a.CheckoutCount;
                            a.CheckBy = a.UpdatedBy;
                            a.CheckInDate = DateTime.Now;
                        }
                        );
                    });
                    break;
                case 2:
                    //Inventory Details
                    externalSearch.ForEach(item =>
                    {
                        List<InventoryBinVm> lstInventoryBin = _context.PersonalInventory
                            .Where(w => w.InmateId == item.InmateId)
                            .Select(s => new InventoryBinVm
                            {
                                PersonalInventoryId = s.PersonalInventoryId,
                                DoNotMoveDuringTransfer = s.PersonalInventoryBinId.HasValue
                                    ? s.PersonalInventoryBin.DoNotMoveDuringTransfer
                                    : 0
                            }).ToList();
                        int rowCount = lstInventoryBin.Count(c => c.DoNotMoveDuringTransfer != 1);
                        if (rowCount == 0) return;
                        //Update Personal Inventory
                        //TODO Remove magic numbers
                        List<PersonalInventory> dbInventory = _context.PersonalInventory
                            .Where(a => a.InventoryDispositionCode == 4
                                && a.InmateId == item.InmateId
                                && (!a.PersonalInventoryBinId.HasValue
                                    || a.PersonalInventoryBin.DoNotMoveDuringTransfer != 1)).ToList();

                        dbInventory.ForEach(inv =>
                        {
                            inv.PersonalInventoryBinId = item.BinId;
                            inv.InventoryBinNumber = item.BinNumber;
                            inv.UpdateDate = DateTime.Now;
                            inv.UpdatedBy = _personnelId;
                        });

                        //Insert Inventory History
                        List<PersonalInventory> dbInventoryHistory = _context.PersonalInventory
                            .Where(a => a.InventoryDispositionCode == 4
                                && a.InmateId == item.InmateId
                                && a.PersonalInventoryBinId.HasValue &&
                                a.PersonalInventoryBin.DoNotMoveDuringTransfer == 1).ToList();

                        dbInventoryHistory.ForEach(hst =>
                        {
                            PersonalInventoryHistory inventoryHistoryDb = new PersonalInventoryHistory
                            {
                                PersonalInventoryId = hst.PersonalInventoryId,
                                InmateId = hst.InmateId,
                                InventoryDate = hst.InventoryDate,
                                InventoryArticles = hst.InventoryArticles,
                                InventoryQuantity = hst.InventoryQuantity,
                                InventoryUom = hst.InventoryUom,
                                InventoryDescription = hst.InventoryDescription,
                                InventoryDispositionCode = hst.InventoryDispositionCode,
                                InventoryValue = hst.InventoryValue,
                                InventoryDestroyed = hst.InventoryDestroyed,
                                InventoryMailed = hst.InventoryMailed,
                                InventoryMailPersonId = hst.InventoryMailPersonId,
                                InventoryMailAddressId = hst.InventoryMailAddressId,
                                InventoryOfficerId = hst.InventoryOfficerId,
                                InventoryColor = hst.InventoryColor,
                                InventoryBinNumber = hst.InventoryBinNumber,
                                InventoryReturnDate = hst.InventoryReturnDate,
                                CreateDate = hst.CreateDate,
                                UpdateDate = hst.UpdateDate,
                                PersonalInventoryBinId = hst.PersonalInventoryBinId ?? 0,
                                PersonName = hst.PersonName,
                                PersonIdType = hst.PersonIdType,
                                PersonAddress = hst.PersonAddress,
                                DispoNotes = hst.DispoNotes,
                                CreatedBy = hst.CreatedBy,
                                UpdatedBy = hst.UpdatedBy,
                                DeleteFlag = hst.DeleteFlag,
                                DeleteDate = hst.DeleteDate,
                                DeletedBy = hst.DeletedBy,
                                CityStateZip = hst.CityStateZip,
                                PersonalInventoryGroupId = hst.PersonalInventoryGroupId,
                                DeleteReason = hst.DeleteReason,
                                DeleteReasonNote = hst.DeleteReasonNote,
                                InventoryEvidencePersonnelId = hst.InventoryEvidencePersonnelId,
                                InventoryEvidenceAgencyId = hst.InventoryEvidenceAgencyId,
                                InventoryEvidenceCaseNumber = hst.InventoryEvidenceCaseNumber
                            };
                            _context.PersonalInventoryHistory.Add(inventoryHistoryDb);
                        });

                        //Do not Move BIn
                        List<string> nameLst = _context.PersonalInventory
                            .Where(w => w.InmateId == item.InmateId
                                && !string.IsNullOrEmpty(w.PersonalInventoryBin.BinName)
                                && w.PersonalInventoryBinId.HasValue &&
                                w.PersonalInventoryBin.DoNotMoveDuringTransfer == 1)
                            .Select(s => s.PersonalInventoryBin.BinName).Distinct().ToList();
                        string binName = null;
                        if (nameLst.Count > 1)
                        {
                            binName = string.Join(',', nameLst);
                        }
                        else if (nameLst.Count == 1)
                        {
                            binName = nameLst.Select(s => s).Single().ToString();
                        }
                        binName = binName != null ? binName + ',' + item.BinNumber : item.BinNumber;
                        Inmate inmate = _context.Inmate.Single(w => w.InmateId == item.InmateId);
                        inmate.InmatePersonalInventory = binName;
                        inmate.UpdateDate = DateTime.Now;
                    });
                    break;
            }

            return await _context.SaveChangesAsync();
        }

        public async Task<int> UpdateFacilityTransfer(ExtTransferFacilityVm extTransfer)
        {
            List<KeyValuePair<int, int>> inmateIncIds = _context.Incarceration.Where(a => !a.ReleaseOut.HasValue)
                .Select(s => new KeyValuePair<int, int>(s.IncarcerationId, Convert.ToInt32(s.InmateId))).ToList();

            //Keep Classification
            bool duration = _context.Facility
                .Where(w => w.DeleteFlag == 0 && w.FacilityId == extTransfer.NewFacilityId)
                .Select(s => s.KeepClassDuringTransfer).Single();

            extTransfer.TransferInmateDetails.ForEach(item =>
            {
                // Update Inmate Last Review Details to null
                if (!duration)
                {
                    Inmate dbInmate = _context.Inmate.Single(s => s.InmateId == item.InmateId);
                    dbInmate.LastClassReviewDate = null;
                    dbInmate.LastClassReviewBy = null;
                    dbInmate.UpdateDate = DateTime.Now;
                }

                //UpdateIncarcerationFacilityHistory

                //GetIncarcerationId
                int incarcerationId = inmateIncIds.Where(w => w.Value == item.InmateId).Select(s => s.Key).Single();

                //Get Incarceration Facility History & Incarceration Facility
                KeyValuePair<int, int> incFacilityHistory = _context.IncarcerationFacilityHistory
                    .Where(w => w.InmateId == item.InmateId && w.IncarcerationId == incarcerationId)
                    .OrderByDescending(o => o.MoveDate)
                    .Select(s => new KeyValuePair<int, int>(s.IncarcerationFacilityHistoryId, s.FacilityId))
                    .Take(1).SingleOrDefault();

                KeyValuePair<int, int> incFacility = _context.IncarcerationFacility
                    .Where(w => w.IncarcerationId == incarcerationId)
                    .OrderByDescending(o => o.IncarcerationFrom)
                    .Select(s => new KeyValuePair<int, int>(s.IncarcerationFacilityId, s.FacilityId))
                    .Take(1).SingleOrDefault();

                //Update Or Insert Incarceration Facility History
                if (incFacilityHistory.Key > 0)
                {
                    IncarcerationFacilityHistory facilityHistory = _context.IncarcerationFacilityHistory
                        .Single(a => a.IncarcerationFacilityHistoryId == incFacilityHistory.Key);
                    facilityHistory.MoveDateThru = DateTime.Now;
                    facilityHistory.MoveDateThruBy = _personnelId;
                }
                else
                {
                    IncarcerationFacilityHistory facilityHistory = new IncarcerationFacilityHistory
                    {
                        IncarcerationId = incarcerationId,
                        InmateId = item.InmateId,
                        TransferFromFacilityId = incFacilityHistory.Value,
                        FacilityId = extTransfer.NewFacilityId,
                        MoveDate = DateTime.Now,
                        MoveDateBy = _personnelId
                    };
                    _context.IncarcerationFacilityHistory.Add(facilityHistory);
                }

                //Update Or Insert Incarceration Facility
                if (incFacility.Key > 0)
                {
                    IncarcerationFacility dbIncFacility = _context.IncarcerationFacility
                        .Single(a => a.IncarcerationFacilityId == incFacility.Key);
                    dbIncFacility.IncarcerationTo = DateTime.Now;
                    dbIncFacility.IncarcerationToBy = _personnelId;
                }
                else
                {
                    IncarcerationFacility facility = new IncarcerationFacility
                    {
                        IncarcerationId = incarcerationId,
                        FacilityId = extTransfer.NewFacilityId,
                        IncarcerationFrom = DateTime.Now,
                        IncarcerationFromBy = _personnelId,
                        FacilityIdFrom = incFacility.Value
                    };
                    _context.IncarcerationFacility.Add(facility);
                }

                extTransfer.TransferDetail = item;

                if (!extTransfer.IsExtenalLocation)
                {
                    DoTracking(extTransfer);
                }

                //DoFacilityTracking
                DoFacilityTransfer(extTransfer);
            });

            return await _context.SaveChangesAsync();
        }

        private void DoFacilityTransfer(ExtTransferFacilityVm extTransfer)
        {
            // DoFacilityTracking
            //TransferInmateHousing
            Inmate inmate = _context.Inmate.Single(w => w.InmateId == extTransfer.TransferDetail.InmateId);
            inmate.FacilityId = extTransfer.NewFacilityId;
            inmate.HousingUnitId = null;
            inmate.InmateCurrentTrack = extTransfer.ExtLocation;
            inmate.InmateCurrentTrackId = extTransfer.ExtLocationId;

            //Insert into HousingUnitMoveHistory
            HousingUnitMoveHistory moveHistory = new HousingUnitMoveHistory()
            {
                InmateId = extTransfer.TransferDetail.InmateId,
                MoveOfficerId = _personnelId,
                MoveReason = AlertsConstants.FACILITYTRANSFER,
                MoveDate = DateTime.Now,
                CreateDate = DateTime.Now,
                UpdateDate = DateTime.Now,
                MoveDateThru = DateTime.Now,
                MoveThruBy = _personnelId
            };
            _context.HousingUnitMoveHistory.Add(moveHistory);

            //Get Max HousingUnitMoveHistoryId
            int housingUnitMoveHistoryId = _context.HousingUnitMoveHistory
                .OrderByDescending(o => o.HousingUnitMoveHistoryId)
                .Select(s => s.HousingUnitMoveHistoryId).DefaultIfEmpty().Max();

            //Transfer_InmateTrack
            InmateTrak inmateTrack = _context.InmateTrak
                .SingleOrDefault(a => a.InmateTrakDateIn == null && a.InmateId == extTransfer.TransferDetail.InmateId);
            if (inmateTrack != null)
            {
                inmateTrack.InmateTrakDateIn = DateTime.Now;
                inmateTrack.InPersonnelId = _personnelId;
            }

            // Insert Inmate Track Details
            InmateTrak newTrack = new InmateTrak()
            {
                InmateId = extTransfer.TransferDetail.InmateId,
                InmateTrakLocation = extTransfer.ExtLocation,
                InmateTrakDateOut = DateTime.Now,
                OutPersonnelId = _personnelId,
                InmateTrakLocationId = extTransfer.ExtLocationId
            };
            _context.InmateTrak.Add(newTrack);

            //Transfer Inmate Housing with Supply items StoreProcedure
            InmateSupplyItemsTransferSupply(extTransfer.TransferDetail.InmateId, extTransfer.NewFacilityId);

            //Delete Program Requests ByFacility
            List<int> programId = _context.Program
                .Where(w => w.ProgramCategory.ProgramCategoryId == w.ProgramCategoryId
                            && w.DeleteFlag != 1 && w.ProgramCategory.DeleteFlag != 1
                            && w.ProgramCategory.FacilityId == extTransfer.FacilityId)
                .Select(s => s.ProgramId).ToList();

            programId.ForEach(p =>
            {
                // This program table need to change based on programClass
                ProgramRequest dbRequest =
                    _context.ProgramRequest.
                        SingleOrDefault(s => s.InmateId == extTransfer.TransferDetail.InmateId && s.ProgramClassId == p);
                if (dbRequest != null)
                {
                    dbRequest.DeleteFlag = 1;
                    dbRequest.UpdateBy = _personnelId;
                    dbRequest.UpdateDate = DateTime.Now;
                }
            });
            _interfaceEngineService.Export(new ExportRequestVm
            {
                EventName = EventNameConstants.HOUSINGMOVE,
                PersonnelId = _personnelId,
                Param1 = extTransfer.TransferDetail.PersonId.ToString(),
                Param2 = housingUnitMoveHistoryId.ToString()
            });
            _context.SaveChanges();
            _inmateService.CreateTask(extTransfer.TransferDetail.InmateId, TaskValidateType.FacilityTransferEvent);
        }

        private void DoTracking(ExtTransferFacilityVm extTransfer)
        {
            if (string.IsNullOrEmpty(extTransfer.TransferDetail.CurrentTrack))
            {
                //UpdateTrackHistory
                UpdateTrackHistory(extTransfer.TransferDetail.InmateId,
                    extTransfer.TransferDetail.InmateTrakId, extTransfer.TransferDetail.ConflictNote);
                //UpdateInmateTrackHistory
                UpdateInmateTrakHistory(extTransfer.TransferDetail.InmateId, null, null);

                //InsertTrackHistory
                InmateTrak objInmateTrak = _context.InmateTrak
                    .SingleOrDefault(s => s.InmateTrakDateIn == null && s.InmateId == extTransfer.TransferDetail.InmateId);
                if (objInmateTrak != null)
                {
                    objInmateTrak.InmateTrakDateIn = DateTime.Now;
                    objInmateTrak.InPersonnelId = _personnelId;
                    objInmateTrak.InmateTrakConflictNote = extTransfer.TransferDetail.ConflictNote;
                }

                InmateTrak objTrack = new InmateTrak
                {
                    InmateId = extTransfer.TransferDetail.InmateId,
                    InmateTrakLocation = extTransfer.ExtLocation,
                    InmateTrakLocationId = extTransfer.ExtLocationId,
                    OutPersonnelId = _personnelId,
                    InmateTrakDateOut = DateTime.Now,
                    InmateTrakConflictNote = extTransfer.TransferDetail.ConflictNote,
                };

                _context.InmateTrak.Add(objTrack);
                Privileges privileges =
                    _context.Privileges.Single(w =>
                        w.PrivilegeId == extTransfer.ExtLocationId);

                int count = _context.InmateTrak.Count(i =>
                    i.InmateTrakLocationId == extTransfer.ExtLocationId &&
                    !i.InmateTrakDateIn.HasValue);

                if (privileges.SafetyCheckLastEntry == null || count > 0)
                {
                    privileges.SafetyCheckLastEntry = DateTime.Now;
                    privileges.SafetyCheckLastEntryBy = _personnelId;
                }

                //UpdateInmateTrackHistory
                UpdateInmateTrakHistory(extTransfer.TransferDetail.InmateId,
                    extTransfer.ExtLocationId, extTransfer.ExtLocation);

                _context.SaveChanges();
                _interfaceEngineService.Export(new ExportRequestVm
                {
                    EventName = EventNameConstants.LOCATIONTRACKING,
                    PersonnelId = _personnelId,
                    Param1 = extTransfer.TransferDetail.PersonId.ToString(),
                    Param2 = objTrack.InmateTrakId.ToString()
                });
            }
            else
            {
                //UpdateTrackHistory
                UpdateTrackHistory(extTransfer.TransferDetail.InmateId,
                    extTransfer.TransferDetail.InmateTrakId, extTransfer.TransferDetail.ConflictNote);
                //UpdateInmateTrackHistory
                UpdateInmateTrakHistory(extTransfer.TransferDetail.InmateId, null, null);
                _interfaceEngineService.Export(new ExportRequestVm
                {
                    EventName = EventNameConstants.LOCATIONTRACKING,
                    PersonnelId = _personnelId,
                    Param1 = extTransfer.TransferDetail.PersonId.ToString(),
                    Param2 = extTransfer.TransferDetail.InmateTrakId.ToString()
                });
            }
            _context.SaveChanges();
        }

        private void UpdateTrackHistory(int inmateId, int trackId, string conflictNote)
        {
            //UpdateTrackHistory
            InmateTrak track = _context.InmateTrak
                .SingleOrDefault(w => w.InmateTrakDateIn == null && w.InmateId == inmateId
                                      || w.InmateTrakId == trackId);
            if (track != null)
            {
                track.InmateTrakDateIn = DateTime.Now;
                track.InPersonnelId = _personnelId;
                track.InmateTrakConflictNote = conflictNote;
            }
            _context.SaveChanges();
        }

        private void InmateSupplyItemsTransferSupply(int inmateId, int facilityId)
        {
            List<InmateSupplyVm> inmateSupplyDetails = _context.InmateSupply.SelectMany(p => _context.InmateSupplyLookup
                    .Where(w => w.InmateSupplyLookupId == p.InmateSupplySizeLookup.InmateSupplyLookupId
                                && p.InmateId == inmateId
                                && !p.DeleteFlag.HasValue
                                && !p.CheckinDate.HasValue),
                (p, w) => new InmateSupplyVm
                {
                    InmateSupplySizeLookupId = p.InmateSupplySizeLookupId,
                    InmateSupplyLookupId = p.InmateSupplySizeLookup.InmateSupplyLookupId,
                    CheckoutCount = p.CheckoutCount,
                    DoNotTransferFlag = w.DoNotTransferFlag,
                    QtyTotal = p.InmateSupplySizeLookup.QtyTotal,
                    QtyCheckedOut = p.InmateSupplySizeLookup.QtyCheckedOut,
                    QtyOnHand = p.InmateSupplySizeLookup.QtyOnHand
                }).ToList();

            inmateSupplyDetails.ForEach(item =>
            {
                int inmateSupplyLookupId = item.InmateSupplyLookupId;

                InmateSupplyLookup inmateSupply = _context.InmateSupplyLookup
                    .SingleOrDefault(w => w.FacilityId == facilityId && w.InmateSupplyLookupId == item.InmateSupplyLookupId);

                if (inmateSupply == null)
                {
                    InmateSupplyLookup dbSupplyLookup = _context.InmateSupplyLookup.SingleOrDefault(w =>
                        w.InmateSupplyLookupId == item.InmateSupplyLookupId && w.DoNotTransferFlag != 1);

                    if (dbSupplyLookup != null)
                    {
                        InmateSupplyLookup newSupplyLookup = new InmateSupplyLookup()
                        {
                            FacilityId = facilityId,
                            ItemDescription = dbSupplyLookup.ItemDescription,
                            ItemColor = dbSupplyLookup.ItemColor,
                            OtherFlag = dbSupplyLookup.OtherFlag,
                            ShirtFlag = dbSupplyLookup.ShirtFlag,
                            PantsFlag = dbSupplyLookup.PantsFlag,
                            ShoesFlag = dbSupplyLookup.ShoesFlag,
                            UnderwearFlag = dbSupplyLookup.UnderwearFlag,
                            BraFlag = dbSupplyLookup.BraFlag,
                            CreateDate = DateTime.Now,
                            CreatedBy = _personnelId
                        };
                        _context.InmateSupplyLookup.Add(newSupplyLookup);
                        _context.SaveChanges();

                        inmateSupplyLookupId = newSupplyLookup.InmateSupplyLookupId;
                    }
                }
                else
                {
                    InmateSupplyLookup dbInmateSupplyLookup = _context.InmateSupplyLookup
                        .Single(a => a.InmateSupplyLookupId == inmateSupply.InmateSupplyLookupId);
                    dbInmateSupplyLookup.InactiveFlag = null;
                    dbInmateSupplyLookup.InactiveDate = DateTime.Now;
                    dbInmateSupplyLookup.InactivatedBy = _personnelId;
                    dbInmateSupplyLookup.UpdateDate = DateTime.Now;
                    dbInmateSupplyLookup.UpdatedBy = _personnelId;
                }

                //For Items size insert/update
                string currentItemSize = _context.InmateSupplySizeLookup
                    .SingleOrDefault(de => de.InmateSupplySizeLookupId == item.InmateSupplySizeLookupId)?.ItemSize;

                int? inmSupplySizeId = _context.InmateSupplySizeLookup.FirstOrDefault(ss =>
                    ss.InmateSupplyLookup.FacilityId == facilityId &&
                    ss.InmateSupplyLookupId == item.InmateSupplyLookupId &&
                    ss.InmateSupplyLookup.DoNotTransferFlag != 1 &&
                    ss.ItemSize == currentItemSize)?.InmateSupplySizeLookupId;

                if (inmSupplySizeId is null)
                {
                    int tempInmateSuLookupId = _context.InmateSupplyLookup.SelectMany(de =>
                            de.InmateSupplySizeLookup.Where(sl => de.FacilityId == facilityId &&
                                sl.InmateSupplySizeLookupId == item.InmateSupplySizeLookupId
                                && sl.InmateSupplyLookup.DoNotTransferFlag != 1 &&
                                sl.InmateSupplyLookupId == de.InmateSupplyLookupId),
                        (ilk, sz) => ilk.InmateSupplyLookupId).SingleOrDefault();

                    InmateSupplySizeLookup inmateSupplySizeLookup = new InmateSupplySizeLookup()
                    {
                        CreateDate = DateTime.Now,
                        CreatedBy = _personnelId,
                        ItemSize = _context.InmateSupplySizeLookup
                            .SingleOrDefault(de => de.InmateSupplySizeLookupId == item.InmateSupplySizeLookupId)?.ItemSize,
                        InmateSupplyLookupId = tempInmateSuLookupId
                    };
                    _context.InmateSupplySizeLookup.Add(inmateSupplySizeLookup);
                }
                else
                {
                    currentItemSize = _context.InmateSupplySizeLookup
                        .SingleOrDefault(de => de.InmateSupplySizeLookupId == item.InmateSupplySizeLookupId
                                            && de.InmateSupplyLookupId == item.InmateSupplyLookupId)?.ItemSize;

                    List<InmateSupplySizeLookup> dbInmateSupplySizeLookup = _context.InmateSupplySizeLookup
                        .Where(s => s.InactiveFlag == 1 && s.ItemSize == currentItemSize).ToList();
                    if (dbInmateSupplySizeLookup.Count > 0)
                    {
                        dbInmateSupplySizeLookup.ForEach(sl =>
                        {
                            sl.InactiveFlag = null;
                            sl.InactivatedBy = null;
                            sl.InactiveDate = null;
                            sl.UpdateDate = DateTime.Now;
                            sl.UpdatedBy = _personnelId;
                        });
                    }
                }
                _context.SaveChanges();

                // @TempTransferFlag =0
                if (item.DoNotTransferFlag == 0)
                {
                    InmateSupplySizeQty sizeQty = new InmateSupplySizeQty()
                    {
                        InmateSupplySizeLookupId = item.InmateSupplySizeLookupId,
                        InmateId = item.InmateId,
                        QtyAdjusted = item.QtyTotal - item.CheckoutCount,
                        AdjustNote = AlertsConstants.FACILITYTRANSFER,
                        AdjustDate = DateTime.Now,
                        AdjustedBy = _personnelId
                    };
                    _context.InmateSupplySizeQty.Add(sizeQty);

                    int qtyTotal = _context.InmateSupplySizeLookup
                        .Where(w => w.InmateSupplyLookupId == inmateSupplyLookupId)
                        .Select(s => s.QtyTotal ?? 0).SingleOrDefault();

                    InmateSupplySizeQty addSizeQty = new InmateSupplySizeQty()
                    {
                        InmateSupplySizeLookupId = inmateSupplyLookupId,
                        InmateId = item.InmateId,
                        QtyAdjusted = qtyTotal - item.CheckoutCount,
                        AdjustNote = AlertsConstants.FACILITYTRANSFER,
                        AdjustDate = DateTime.Now,
                        AdjustedBy = _personnelId
                    };
                    _context.InmateSupplySizeQty.Add(addSizeQty);

                    //Subtract from current
                    InmateSupplySizeLookup inmateSupplySizeLookup = _context.InmateSupplySizeLookup
                        .Single(ss => ss.InmateSupplySizeLookupId == item.InmateSupplySizeLookupId);

                    var dbDetails = _context.InmateSupplySizeLookup.SelectMany(inm =>
                            _context.InmateSupply.Where(de =>
                                inm.InmateSupplySizeLookupId == de.InmateSupplySizeLookupId &&
                                inm.InmateSupplySizeLookupId == item.InmateSupplySizeLookupId &&
                                !de.CheckinDate.HasValue &&
                                de.DeleteFlag != 1 &&
                                inm.InmateSupplyLookupId == item.InmateSupplyLookupId && de.InmateId == item.InmateId),
                        (sz, ins) => new
                        {
                            sz.QtyCheckedOut,
                            sz.QtyTotal,
                            ins.CheckoutCount
                        }).SingleOrDefault();

                    if (dbDetails != null)
                    {
                        inmateSupplySizeLookup.QtyTotal = dbDetails.QtyTotal - dbDetails.CheckoutCount;
                        inmateSupplySizeLookup.QtyCheckedOut = dbDetails.QtyCheckedOut - dbDetails.CheckoutCount;
                        inmateSupplySizeLookup.QtyOnHand =
                            dbDetails.QtyTotal - dbDetails.CheckoutCount -
                            (dbDetails.QtyCheckedOut - dbDetails.CheckoutCount);
                    }

                    //Add supply item into moved facility  
                    InmateSupplySizeLookup dbInmateSupplySizeLkp = _context.InmateSupplySizeLookup
                        .SingleOrDefault(isl => isl.InmateSupplySizeLookupId == inmSupplySizeId);
                    if (dbInmateSupplySizeLkp != null)
                    {
                        dbInmateSupplySizeLkp.QtyTotal = dbInmateSupplySizeLkp.QtyTotal + item.CheckoutCount;
                        dbInmateSupplySizeLkp.QtyCheckedOut = dbInmateSupplySizeLkp.QtyCheckedOut + item.CheckoutCount;
                        dbInmateSupplySizeLkp.QtyOnHand = dbInmateSupplySizeLkp.QtyTotal - dbInmateSupplySizeLkp.QtyCheckedOut;
                    }
                    _context.SaveChanges();
                }

                //Update InmateSupplySizeLookup
                if (item.DoNotTransferFlag == 1)
                {
                    InmateSupplySizeLookup supplySizeLookup = _context.InmateSupplySizeLookup
                        .Single(a => a.InmateSupplySizeLookupId == item.InmateSupplySizeLookupId);
                    supplySizeLookup.QtyCheckedOut = item.QtyCheckedOut - item.CheckoutCount;
                    supplySizeLookup.QtyOnHand = item.QtyOnHand + item.CheckoutCount;
                }

                //Update InmateSupply Check in details

                InmateSupply supply = _context.InmateSupply.Single();
                supply.CheckinCount = item.CheckoutCount;
                supply.CheckinDate = DateTime.Now;
                supply.CheckBy = _personnelId;

                //Checkout to proposed facility
                InmateSupply newSupply = new InmateSupply()
                {
                    InmateSupplySizeLookupId = inmateSupplyLookupId,
                    InmateId = item.InmateId,
                    CheckoutCount = item.CheckoutCount,
                    CreateDate = DateTime.Now,
                    CreatedBy = _personnelId,
                    CheckoutDate = DateTime.Now,
                    CheckoutBy = _personnelId,
                };
                _context.InmateSupply.Add(newSupply);
                _context.SaveChanges();
            });
        }

        private void UpdateInmateTrakHistory(int inmateId, int? locationId, string location)
        {
            Inmate inmateHistory = _context.Inmate.Single(a => a.InmateId == inmateId);
            inmateHistory.InmateCurrentTrack = location;
            inmateHistory.InmateCurrentTrackId = locationId;
            _context.SaveChanges();
        }

        public async Task<int> UpdateCheckInLibBook(List<int> inmateIds)
        {
            inmateIds.ForEach(item =>
            {
                //Update Library Track Details
                LibraryTrack dbLibTrack = _context.LibraryTrack
                    .Single(a => a.CheckoutInmateId == item);

                dbLibTrack.CheckInBy = _personnelId;
                dbLibTrack.UpdateBy = _personnelId;
                dbLibTrack.CheckInDate = DateTime.Now;
                dbLibTrack.UpdateDate = DateTime.Now;

                //Update Library Book
                LibraryBook dbLibBook = _context.LibraryBook
                    .SingleOrDefault(a => a.CurrentCheckoutInmateId == item);
                if (dbLibBook != null)
                {
                    dbLibBook.CurrentCheckoutInmateId = null;
                    dbLibBook.CurrentCheckoutDate = null;
                    dbLibBook.CurrentSchdReturn = null;
                    dbLibBook.CurrentCheckoutBy = null;
                    dbLibBook.CurrentCheckoutNote = null;
                }
            });

            return await _context.SaveChangesAsync();
        }

        public List<LibraryBook> GetCounts()
        {
            List<int> lookupTypeId = _context.Lookup
                .Where(l => l.LookupType == LookupConstants.LIBBOOKTYPE && l.LookupInactive == 0)
                .Select(l => l.LookupIndex).ToList();
            List<int> lookupCategoryId = _context.Lookup
                .Where(l => l.LookupType == LookupConstants.LIBBOOKCAT && l.LookupInactive == 0)
                .Select(l => l.LookupIndex).ToList();
            List<int> lookupConditionId = _context.Lookup
                .Where(l => l.LookupType == LookupConstants.LIBBOOKCOND && l.LookupInactive == 0)
                .Select(l => l.LookupIndex).ToList();

            List<LibraryBook> libraryBook = _context.LibraryBook
                .Where(w => w.DeleteFlag == 0
                            && w.LibraryRoom.DeleteFlag == 0
                            && w.LibraryRoomLocation.DeleteFlag == 0
                            && lookupTypeId.Contains(w.BookTypeId)
                            && lookupCategoryId.Contains(w.BookCategoryId)
                            && lookupConditionId.Contains(w.CurrentConditionId ?? 0)).Select(s => s).ToList();

            return libraryBook;
        }

        #endregion

        #region Schedule Transfer

        public List<AoAppointmentVm> GetScheduleTransfer(DateTime fromDate, int facilityId)
        {
            fromDate = DateTime.Now;
            IList<int> inmateIds = _context.Inmate.Where(x => x.InmateActive == 1 &&
            x.FacilityId == facilityId).Select(s => s.InmateId).ToList();

            List<AoAppointmentVm> aoAppointment = _appointmentService.
                ListAoAppointments(facilityId, null, fromDate, fromDate, true, false, true);
            //aoAppointment = aoAppointment.SelectMany(a => _context.Inmate.Where(x =>
            //        x.InmateId == a.InmateId && x.InmateActive == 1 && x.FacilityId == facilityId &&
            //        a.StartDate >= fromDate.Date),
            //    (a, x) => a).ToList();
            aoAppointment = aoAppointment.Where(s => s.StartDate >= fromDate.Date && inmateIds.Contains(s.InmateId ?? 0)).ToList();
            aoAppointment = aoAppointment.GroupBy(a => new { a.ScheduleId, a.StartDate }).Select(a => a.First()).Take(10000).ToList();
            if (!aoAppointment.Any()) return aoAppointment;
            aoAppointment = aoAppointment.Where(app => app.InmateDetails.FacilityId == facilityId).ToList();
            List<HousingCapacityVm> hosuingDetails =
                GetHousingFacility(facilityId);
            if (hosuingDetails.Any(s => s.HousingUnitListId > 0))
            {
                aoAppointment.ForEach(item =>
                {
                    item.HousingVmDetails =
                        hosuingDetails.Where(s => s.HousingUnitListId == item.InmateDetails.HousingUnitListId)
                            .ToList();
                    if (item.ApptOccurance != null)
                    {
                        item.DeleteFlag = item.ApptOccurance.DeleteFlag == 1;
                    }
                });
            }
            return aoAppointment;
        }

        private List<HousingCapacityVm> GetHousingFacility(int facilityId)
        {
            IQueryable<Inmate> inmateList = _context.Inmate.Where(w => w.InmateActive == 1 && w.FacilityId == facilityId);

            //To get Housing Unit details
            IEnumerable<HousingUnit> listHousingUnit = _context.HousingUnit.Where(w => w.Facility.DeleteFlag == 0
                && w.HousingUnitInactive != 1).ToList();

            List<KeyValuePair<int, string>> lstFacilityAbbr = _context.Facility.Where(w => w.DeleteFlag == 0)
                .Select(s => new KeyValuePair<int, string>(s.FacilityId, s.FacilityAbbr)).ToList();

            //To get housing location details
            List<HousingCapacityVm> housingBuildingCapacityList = listHousingUnit.Where(w => w.FacilityId == facilityId)
                .GroupBy(g => new { g.HousingUnitListId, g.HousingUnitNumber, g.HousingUnitLocation })
                .Select(s => new HousingCapacityVm
                {
                    HousingLocation = s.Key.HousingUnitLocation,
                    HousingNumber = s.Key.HousingUnitNumber,
                    HousingUnitListId = s.Key.HousingUnitListId,
                    FacilityId = facilityId,
                    FacilityAbbr = lstFacilityAbbr.SingleOrDefault(w => w.Key == facilityId).Value,
                    CurrentCapacity = s.Sum(c => c.HousingUnitActualCapacity ?? 0),
                    OutofService = s.Sum(c => c.HousingUnitOutOfService ?? 0),
                    Assigned = inmateList.Count(c => c.HousingUnit.HousingUnitListId == s.Key.HousingUnitListId)
                }
                ).ToList();

            return housingBuildingCapacityList;
        }

        public List<AoAppointmentVm> HousingDetails(int facilityId, int inmateId)
        {
            List<AoAppointmentVm> aoAppointments = new List<AoAppointmentVm>();
            List<HousingCapacityVm> housingCapacityVms = _housingService.GetRecommendHousing(facilityId, inmateId).HousingCapacityList;
            aoAppointments.ForEach(s =>
            {
                s.HousingVmDetails = housingCapacityVms.Where(t => t.HousingUnitListId == s.InmateDetails.HousingUnitListId).ToList();
            });
            return aoAppointments;
        }
        public async Task<AppointmentConflictCheck> InsertAddInmateAsync(InmateApptDetailsVm inmateApptDetails)
        {
            AppointmentConflictCheck apptConflictCheck = new AppointmentConflictCheck();
            ////To Check Appointment Conflict 
            if (inmateApptDetails.AoScheduleDetails.IsSingleOccurrence && !inmateApptDetails.ApptRecheckConflict)
            {
                ScheduleVm aoSchedule = inmateApptDetails.AoScheduleDetails;
                List<AppointmentConflictDetails> lstofReoccurAppt = new List<AppointmentConflictDetails>();
                if (aoSchedule != null)
                {
                    //To Check Appointment Conflict 
                    DateTime? endDate = aoSchedule.EndDate;
                    endDate = inmateApptDetails.AllDayAppt ? endDate?.Date.AddHours(23).AddMinutes(59) : endDate;

                    //Taking the list of appointment details from appointment service
                    List<AoAppointmentVm> lstApptDetails = GetScheduleTransfer(fromDate: DateTime.Now,
                        inmateApptDetails.FacilityId ?? 0);
                    IQueryable<Privileges> privilegesList = _context.Privileges;
                    if (endDate.HasValue)
                    {
                        foreach (DateTime day in EachDay(aoSchedule.StartDate, endDate.Value))
                        {
                            inmateApptDetails.InmateId.ForEach(item =>
                            {
                                //Appointment Conflict Check for Reoccurence
                                lstofReoccurAppt.AddRange(lstApptDetails.Where(app => app.InmateId == item
                                        && app.EndDate.HasValue
                                        && (app.EndDate == null || app.EndDate.Value.Date >= day.Date) &&
                                        (aoSchedule.StartDate < day.Date.Add(app.EndDate.Value.TimeOfDay) &&
                                            aoSchedule.EndDate > day.Date.Add(app.StartDate.TimeOfDay) ||
                                            app.StartDate.ToString(DateConstants.HOURSMINUTES)
                                            == DateConstants.STARTHRSMINUTES
                                            && app.EndDate.Value.ToString(DateConstants.HOURSMINUTES)
                                            == DateConstants.STARTHRSMINUTES)
                                    )
                                    .Select(app => new AppointmentConflictDetails
                                    {
                                        InmateId = item,
                                        ApptId = app.ScheduleId,
                                        ConflictType = ConflictTypeConstants.APPOINTMENTOVERLAPCONFLICT,
                                        ConflictApptLocation = privilegesList
                                            .SingleOrDefault(x => x.PrivilegeId == app.LocationId)
                                            ?.PrivilegeDescription,
                                        ApptStartDate = app.StartDate,
                                        ApptReoccurFlag = app.IsSingleOccurrence ? 0 : 1
                                    }).ToList());

                                lstofReoccurAppt.AddRange(lstApptDetails.Where(app =>
                                        app.InmateId == item &&
                                        (app.EndDate == null ||
                                            app.EndDate.Value.Date >= day.Date) &&
                                        app.EndDate.HasValue &&
                                        (day.Date.Add(app.EndDate.Value.TimeOfDay) <= aoSchedule.StartDate &&
                                            day.Date.Add(app.EndDate.Value.TimeOfDay).AddHours(1) >=
                                            aoSchedule.StartDate && app.StartDate.ToString(DateConstants.HOURSMINUTES)
                                            != DateConstants.STARTHRSMINUTES &&
                                            app.EndDate.Value.ToString(DateConstants.HOURSMINUTES)
                                            != DateConstants.STARTHRSMINUTES ||
                                            aoSchedule.EndDate <= day.Date.Add(app.StartDate.TimeOfDay) &&
                                            day.Date.Add(app.StartDate.TimeOfDay).AddHours(-1) <= endDate &&
                                            app.StartDate.ToString(DateConstants.HOURSMINUTES)
                                            != DateConstants.STARTHRSMINUTES &&
                                            app.EndDate.Value.ToString(DateConstants.HOURSMINUTES)
                                            != DateConstants.STARTHRSMINUTES)
                                    )
                                    .Select(app => new AppointmentConflictDetails
                                    {
                                        InmateId = item,
                                        ApptId = app.ScheduleId,
                                        ConflictType = ConflictTypeConstants.APPOINTMENTWARNINGBACKTOBACK,
                                        ApptStartDate = app.StartDate,
                                        ConflictApptLocation = privilegesList
                                            .SingleOrDefault(x => x.PrivilegeId == app.LocationId)
                                            ?.PrivilegeDescription,
                                        ApptReoccurFlag = app.IsSingleOccurrence ? 0 : 1
                                    }));
                            });
                        }
                    }
                    string appointmentLocation =
                        privilegesList.Single(p => p.PrivilegeId == aoSchedule.LocationId).PrivilegeDescription;

                    apptConflictCheck = new AppointmentConflictCheck
                    {
                        ApptLocation = appointmentLocation,
                        ApptLocationId = aoSchedule.LocationId,
                        EndDate = inmateApptDetails.AllDayAppt ? aoSchedule.StartDate : endDate,
                        ApptDate = aoSchedule.StartDate,
                        ApptConflictDetails = lstofReoccurAppt
                    };

                    if (inmateApptDetails.ScheduleId > 0)
                    {
                        apptConflictCheck.ApptConflictDetails = lstofReoccurAppt
                         .Where(a => a.ApptId != inmateApptDetails.ScheduleId).ToList();
                    }
                }
            }
            if (apptConflictCheck.ApptConflictDetails.Count > 0)
            {
                return apptConflictCheck;
            }

            await _appointmentService.InsertMultiInmateAppt(inmateApptDetails, inmateApptDetails.InmateId);
            return apptConflictCheck;
        }

        private static IEnumerable<DateTime> EachDay(DateTime from, DateTime thru)
        {
            for (DateTime day = from.Date; day.Date <= thru.Date; day = day.AddDays(1))
            {
                yield return day;
            }
        }

        public List<AppointmentHistoryList> GetAppointmentSavedHistory(int scheduledId)
        {
            List<AppointmentHistoryList> lstAppointmentHistory = new List<AppointmentHistoryList>();
            //List<AppointmentHistoryList> lstAppointmentHistory = _context.AoscheduleSaveHistory
            //    .Where(w => w.AoscheduleId == scheduledId)
            //    .OrderByDescending(ph => ph.CreateDate)
            //    .Select(s => new AppointmentHistoryList
            //    {
            //        AppointmentHistoryId = s.AoscheduleSaveHistoryId,
            //        CreateDate = s.CreateDate,
            //        PersonId = s.Personnel.PersonId,
            //        OfficerBadgeNumber = s.Personnel.OfficerBadgeNum,
            //        ApptHistoryList = s.AppointmentHistoryList
            //    }).ToList();

            if (lstAppointmentHistory.Count == 0) return lstAppointmentHistory;
            //For Improve Performance All Person Details Loaded By Single Hit Before Looping
            int[] personIds = lstAppointmentHistory.Select(s => s.PersonId).ToArray();
            //get person list
            List<Person> lstPersonDet = _context.Person.Where(per => personIds.Contains(per.PersonId)).ToList();

            lstAppointmentHistory.ForEach(item =>
            {
                item.PersonLastName = lstPersonDet.SingleOrDefault(p => p.PersonId == item.PersonId)?.PersonLastName;
                //To GetJson Result Into Dictionary
                if (string.IsNullOrEmpty(item.ApptHistoryList)) return;
                Dictionary<string, string> apptHistoryList =
                    JsonConvert.DeserializeObject<Dictionary<string, string>>(item.ApptHistoryList);
                item.Header = apptHistoryList.Select(ph => new AppointmentHeader { Header = ph.Key, Detail = ph.Value })
                    .ToList();
            });
            return lstAppointmentHistory;
        }

        #endregion
    }
}
