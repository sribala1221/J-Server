﻿using GenerateTables.Models;
using Microsoft.AspNetCore.Http;
using ServerAPI.Utilities;
using ServerAPI.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace ServerAPI.Services
{
    public class IncidentService : IIncidentService
    {
        private readonly AAtims _context;
        private readonly IWizardService _wizardService;
        private readonly int _personnelId;
        private readonly ICommonService _commonService;
        private readonly IBookingService _bookingService;
        private readonly IPhotosService _photo;

        public IncidentService(AAtims context, IWizardService wizardService, ICommonService commonService,
            IBookingService bookingService, IHttpContextAccessor httpContextAccessor, IPhotosService photoService)
        {
            _context = context;
            _wizardService = wizardService;
            _commonService = commonService;
            _bookingService = bookingService;
            _photo = photoService;
            _personnelId = Convert.ToInt32(httpContextAccessor.HttpContext.User.FindFirst("personnelId")?.Value);
        }

        #region Incident Calendar

        //Get Hearing Location Details
        public List<KeyValuePair<int, string>> GetHearingLocation() => _context.Privileges
            .Where(pr => pr.ShowInHearing == 1 && pr.InactiveFlag == 0)
            .Select(pri => new KeyValuePair<int, string>(pri.PrivilegeId, pri.PrivilegeDescription))
            .OrderBy(pr => pr.Value).ToList();

        //Get Appointment Calendar Details
        public List<IncidentCalendarVm> GetIncidentCalendarDetails(DateTime appointmentDate, int facilityId)
        {
            List<IncidentCalendarVm> dbAppointment = _context.ScheduleIncident
                .Where(ap => !ap.DeleteFlag && ap.StartDate.Date == appointmentDate.Date)
                .Select(ap => new IncidentCalendarVm
                {
                    DisciplinaryInmateId = ap.DisciplinaryInmateId,
                    AppointmentDate = ap.StartDate,
                    AppointmentEnd = ap.EndDate,
                    AppointmentLocation = ap.Location.PrivilegeDescription,
                    AppointmentLocationId = ap.LocationId ?? 0,
                    DeleteFlag = ap.DisciplinaryInmate.DeleteFlag == 1,
                    PersonId = ap.DisciplinaryInmate.Inmate.PersonId,
                    InmateCurrentTrack = ap.DisciplinaryInmate.Inmate.InmateCurrentTrack,
                    InmateNumber = ap.DisciplinaryInmate.Inmate.InmateNumber,
                    InmateActive = ap.DisciplinaryInmate.Inmate.InmateActive == 1
                }).ToList();

            List<int> personIds = dbAppointment.Select(app => app.PersonId).ToList();
            List<PersonInfoVm> dbPersonLst = (from pr in _context.Person
                                              where personIds.Contains(pr.PersonId)
                                              select new PersonInfoVm
                                              {
                                                  PersonId = pr.PersonId,
                                                  PersonFirstName = pr.PersonFirstName,
                                                  PersonLastName = pr.PersonLastName,
                                                  PersonMiddleName = pr.PersonMiddleName
                                              }).ToList();

            dbAppointment.ForEach(details =>
            {
                details.PersonDetails = dbPersonLst.SingleOrDefault(per => per.PersonId == details.PersonId);
            });
            return dbAppointment;
        }

        #endregion

        private static int CalculateElaps(DateTime? requestDate) =>
            requestDate.HasValue ? (int)((DateTime.Now - requestDate.Value).TotalHours) : 0;

        //Get Incident Involved Party Details
        public List<ClassifyInvPartyDetailsVm> GetInvPartyDetails(int facilityId, int filterCategory)
        {
            List<LookupVm> lookups = _commonService
                .GetLookups(new[] { LookupConstants.DISCTYPE, LookupConstants.DISCINTYPE, LookupConstants.INCCAT });
            List<ClassifyInvPartyDetailsVm> classifyInvPartyDetailsVm = _context.DisciplinaryInmate
                .Where(w => w.DisciplinaryIncident.DisciplinaryActive == 1 &&
                w.DisciplinaryIncident.FacilityId == facilityId &&
                (!w.DeleteFlag.HasValue || w.DeleteFlag == 0) &&
                ((int)FilterCategory.ShowMyActiveIncidents != filterCategory ||
                w.DisciplinaryIncident.DisciplinaryOfficerId == _personnelId) &&
                ((int)FilterCategory.ShowMyInvolvedPartyIncidents != filterCategory ||
                w.PersonnelId == _personnelId))
                .Select(s => new ClassifyInvPartyDetailsVm
                {
                    DisciplinaryIncidentId = s.DisciplinaryIncidentId,
                    InmateId = s.InmateId,
                    PersonnelId = s.PersonnelId,
                    DisciplinaryInmateId = s.DisciplinaryInmateId,
                    IncidentNum = s.DisciplinaryIncident.DisciplinaryNumber,
                    HearingComplete = s.HearingComplete,
                    InmateNumber = s.InmateId > 0 ? s.Inmate.InmateNumber : default,
                    ByPassHearing = s.DisciplinaryInmateBypassHearing >= 1,
                    DisciplinaryOtherName = s.DisciplinaryOtherName,
                    PersonFirstName = s.InmateId > 0 ? s.Inmate.Person.PersonFirstName :
                    s.PersonnelId > 0 ? s.Personnel.PersonNavigation.PersonFirstName : default,
                    PersonLastName = s.InmateId > 0 ? s.Inmate.Person.PersonLastName :
                    s.PersonnelId > 0 ? s.Personnel.PersonNavigation.PersonLastName : default,
                    IncidentType = lookups.FirstOrDefault(w => w.LookupType == LookupConstants.DISCTYPE &&
                        w.LookupIndex == s.DisciplinaryIncident.DisciplinaryType).LookupDescription,
                    InvPartyType = lookups.FirstOrDefault(w => w.LookupType == LookupConstants.DISCINTYPE &&
                       w.LookupIndex == s.DisciplinaryInmateType).LookupDescription,
                    Elapsed = CalculateElaps(s.NoticeDate),
                    DisciplinaryHearingHoldReason = s.DisciplinaryHearingHoldReason,
                    DisciplinaryHearingHoldDate = s.DisciplinaryHearingHoldDate,
                    HoldPersonnel = new PersonnelVm
                    {
                        PersonFirstName = s.DisciplinaryHearingHoldByNavigation.PersonNavigation.PersonFirstName,
                        PersonLastName = s.DisciplinaryHearingHoldByNavigation.PersonNavigation.PersonLastName,
                        OfficerBadgeNumber = s.DisciplinaryHearingHoldByNavigation.OfficerBadgeNumber
                    },
                    IncidentWizardStep = s.IncidentWizardStep,
                    DisciplinaryScheduleHearingDate = s.DisciplinaryScheduleHearingDate,
                    NoticeFlag = s.NoticeFlag,
                    DisciplinaryHearingDate = s.DisciplinaryHearingDate,
                    DisciplinaryReviewDate = s.DisciplinaryReviewDate,
                    NoticeDate = s.NoticeDate,
                    HearingHold = s.DisciplinaryHearingHold,
                    CompleteDate = s.DisciplinaryReviewCompleteDate,
                    DisciplinaryReportDate = s.DisciplinaryIncident.DisciplinaryReportDate,
                    AllowHearingFlag = s.DisciplinaryIncident.AllowHearingFlag > 0,
                    DisciplinaryActive = s.DisciplinaryIncident.DisciplinaryActive > 0,
                    SensitiveMaterial = s.DisciplinaryIncident.SensitiveMaterial,
                    PreaOnly = s.DisciplinaryIncident.PreaOnly,
                    Categorization = lookups.FirstOrDefault(w => w.LookupType == LookupConstants.INCCAT &&
                          w.LookupIndex == s.DisciplinaryIncident.IncidentCategorizationIndex).LookupDescription,
                    InitialEntryFlag = s.DisciplinaryIncident.InitialEntryFlag ?? false,
                    SupervisorReviewFlag = s.DisciplinaryIncident.DisciplinaryIncidentNarrative
                    .Count(a => !a.ReadyForReviewFlag.HasValue || a.ReadyForReviewFlag == 0 ||
                    !a.SupervisorReviewFlag.HasValue || a.SupervisorReviewFlag == 0) > 0
                }).ToList();
            List<AoWizardProgressVm> incidentWizardProgresses =
                GetIncidentWizardProgress(classifyInvPartyDetailsVm.Select(a => a.DisciplinaryInmateId).ToArray());
            classifyInvPartyDetailsVm.ForEach(a => a.ActiveIncidentProgress =
                    incidentWizardProgresses.FirstOrDefault(x => x.DisciplinaryInmateId == a.DisciplinaryInmateId));
            return classifyInvPartyDetailsVm;
        }

        //TODO 11?!
        public AoWizardFacilityVm GetIncidentWizard()
        {
            AoWizardVm wizardDetails = _wizardService.GetWizardSteps(11)[0];
            return wizardDetails.WizardFacilities.FirstOrDefault();
        }

        private List<IncidentNarrativeDetailVm> IncidentNarrativeDetails(int facilityId, List<LookupVm> lookups) => _context
            .DisciplinaryIncidentNarrative.Where(w =>
                w.DisciplinaryIncident.FacilityId == facilityId && (!w.DeleteFlag.HasValue || w.DeleteFlag == 0)
                                                                && w.DisciplinaryIncident.DisciplinaryActive == 1)
            .Select(s => new IncidentNarrativeDetailVm
            {
                DisciplinaryIncidentId = s.DisciplinaryIncident.DisciplinaryIncidentId,
                DisciplinaryActive = s.DisciplinaryIncident.DisciplinaryActive == 1,
                IncidentNumber = s.DisciplinaryIncident.DisciplinaryNumber,
                IncDate = s.DisciplinaryIncident.DisciplinaryIncidentDate,
                DisciplinaryReportDate = s.DisciplinaryIncident.DisciplinaryReportDate,
                Type = lookups.Where(l => l.LookupType == LookupConstants.DISCTYPE
                    && l.LookupIndex == s.DisciplinaryIncident.DisciplinaryType)
                    .Select(l => l.LookupDescription).FirstOrDefault(),
                DisciplinaryIncidentNarrative = s.DisciplinaryIncidentNarrative1,
                CreateDate = s.CreateDate,
                DisciplinaryIncidentNarrativeId = s.DisciplinaryIncidentNarrativeId,
                DisciplinaryOfficerId = s.CreateBy,
                SupervisorReviewFlag = s.SupervisorReviewFlag == 1,
                ReadyForReviewFlag = s.ReadyForReviewFlag == 1,
                SupervisorReviewBy = s.SupervisorReviewBy ?? 0,
                SupervisorReviewNote = s.SupervisorReviewNote,
                SensitiveMaterial = s.DisciplinaryIncident.SensitiveMaterial,
                PreaOnly = s.DisciplinaryIncident.PreaOnly,
                CreateByPersonnel = new PersonnelVm
                {
                    PersonnelId = s.CreateBy,
                    PersonLastName = s.CreateByNavigation.PersonNavigation.PersonLastName,
                    PersonFirstName = s.CreateByNavigation.PersonNavigation.PersonFirstName,
                    OfficerBadgeNumber = s.CreateByNavigation.OfficerBadgeNum
                },
                ReviewBy = new PersonnelVm
                {
                    PersonnelId = s.SupervisorReviewBy ?? 0,
                    PersonLastName = s.SupervisorReviewByNavigation.PersonNavigation.PersonLastName,
                    PersonFirstName = s.SupervisorReviewByNavigation.PersonNavigation.PersonFirstName,
                    OfficerBadgeNumber = s.SupervisorReviewByNavigation.OfficerBadgeNum
                },
                Categorization = _context.Lookup.Where(x => x.LookupType == LookupConstants.INCCAT
                    && x.LookupIndex == s.DisciplinaryIncident.IncidentCategorizationIndex)
                    .Select(x => x.LookupDescription).FirstOrDefault(),
                InitialEntryFlag = s.DisciplinaryIncident.InitialEntryFlag ?? false,
                AllowHearingFlag = s.DisciplinaryIncident.AllowHearingFlag > 0
            }).ToList();

        private int NarrativesSupervisorCount(int facilityId) => _context.DisciplinaryIncidentNarrative
                    .Where(w => w.DisciplinaryIncident.DisciplinaryActive == 1
                    && w.DisciplinaryIncident.FacilityId == facilityId &&
                    (!w.DisciplinaryIncident.AllowHearingFlag.HasValue || w.DisciplinaryIncident.AllowHearingFlag == 0
                    || !w.SupervisorReviewFlag.HasValue || w.SupervisorReviewFlag == 0))
            .GroupBy(g => g.DisciplinaryIncidentId).Count();

        public List<IncidentNarrativeDetailVm> SupervisorNarrativesDetails(int facilityId)
        {
            List<LookupVm> lookups =
                _commonService.GetLookups(new[] { LookupConstants.DISCTYPE, LookupConstants.INCCAT });
            return SupervisorStatusDetails(facilityId, lookups);
            
        }

        public NarrativeCommonDetailVm GetNarrativeCommonDetail(int facilityId)
        {
            List<LookupVm> lookups =
                _commonService.GetLookups(new[] { LookupConstants.DISCTYPE, LookupConstants.INCCAT });
            NarrativeCommonDetailVm narrativeCommonDetail = new NarrativeCommonDetailVm
            {
                IncidentNarrativeDetails = SupervisorStatusDetails(facilityId, lookups),
                SuperviorStatusDetails = GetSuperviorDetails(facilityId, lookups),
                AppealReviewCount = _context.DisciplinaryInmateAppeal
                .Where(a => a.DisciplinaryInmate.DisciplinaryIncident.FacilityId == facilityId
                && a.ReviewComplete == 0 && a.SendForReview == 0)
                .Select(a => new IncidentNarrativeDetailVm
                {
                    SensitiveMaterial = a.DisciplinaryInmate.DisciplinaryIncident.SensitiveMaterial,
                    PreaOnly = a.DisciplinaryInmate.DisciplinaryIncident.PreaOnly
                }).ToList()
            };
            return narrativeCommonDetail;
        }

        public List<IncidentNarrativeDetailVm> SupervisorStatusDetails(int facilityId, List<LookupVm> lookups)
        {
            List<IncidentNarrativeDetailVm> narrativeSuperviorDetail = _context.DisciplinaryIncident
                .Where(w => w.DisciplinaryActive == 1 && w.FacilityId == facilityId)
                .Select(a => new IncidentNarrativeDetailVm
                {
                    InitialEntryFlag = a.InitialEntryFlag.HasValue && a.InitialEntryFlag == true,
                    IncidentNumber = a.DisciplinaryNumber,
                    IncDate = a.DisciplinaryIncidentDate,
                    IncidentLocation = a.DisciplinaryLocation,
                    DisciplinaryIncidentId = a.DisciplinaryIncidentId,
                    AllowHearingFlag = a.AllowHearingFlag == 1,
                    DisciplinaryType = a.DisciplinaryType ?? 0,
                    DisciplinaryReportDate = a.DisciplinaryReportDate,
                    ActionNote = a.DisciplinarySupervisorAction,
                    CreateByPersonnel = new PersonnelVm
                    {
                        PersonnelId = a.DisciplinaryOfficerId,
                        PersonId = a.DisciplinaryOfficer.PersonNavigation.PersonId,
                        PersonLastName = a.DisciplinaryOfficer.PersonNavigation.PersonLastName,
                        PersonFirstName = a.DisciplinaryOfficer.PersonNavigation.PersonFirstName,
                        OfficerBadgeNumber = a.DisciplinaryOfficer.OfficerBadgeNum
                    },
                    NoticeDate = a.DisciplinaryInmate.OrderByDescending(n => n.NoticeDate)
                        .Select(n => n.NoticeDate).FirstOrDefault(),
                    ExpectedNarrativeCount = a.ExpectedNarrativeCount ?? 0,
                    Type = lookups.Where(l => l.LookupType == LookupConstants.DISCTYPE
                                              && l.LookupIndex == a.DisciplinaryType)
                        .Select(l => l.LookupDescription).FirstOrDefault(),
                    Categorization = lookups.Where(l => l.LookupType == LookupConstants.INCCAT
                        && l.LookupIndex == a.IncidentCategorizationIndex)
                        .Select(s => s.LookupDescription).FirstOrDefault(),
                    RecordsCount = a.DisciplinaryIncidentNarrative.Count(),
                    Reviwed = a.DisciplinaryIncidentNarrative.All(w =>
                        w.SupervisorReviewFlag == 1 &&
                        (!w.DeleteFlag.HasValue || w.DeleteFlag == 0)),
                    DisciplinaryActive = a.DisciplinaryActive == 1,
                    DispOfficerNarrativeFlag = a.DisciplinaryOfficerNarrativeFlag.HasValue &&
                    a.DisciplinaryOfficerNarrativeFlag == true,
                    DispInmateNarrativeFlag = a.DisciplinaryInmate
                    .Any(x => x.NarrativeFlag.HasValue && x.NarrativeFlag == true),
                    SensitiveMaterial = a.SensitiveMaterial,
                    PreaOnly = a.PreaOnly,
                    ReviewedCount = a.DisciplinaryIncidentNarrative.Count(w =>
                        w.SupervisorReviewFlag == 1 && (!w.DeleteFlag.HasValue || w.DeleteFlag == 0)),
                    NotForReviwed = a.DisciplinaryIncidentNarrative
                        .Count(n => (!n.DeleteFlag.HasValue || n.DeleteFlag == 0)
                                    && (!n.SupervisorReviewFlag.HasValue || n.SupervisorReviewFlag == 0)
                                    && (!n.ReadyForReviewFlag.HasValue || n.ReadyForReviewFlag == 0)),
                    ReadForReviwed = a.DisciplinaryIncidentNarrative
                        .Count(n => (!n.DeleteFlag.HasValue || n.DeleteFlag == 0)
                                    && (!n.SupervisorReviewFlag.HasValue || n.SupervisorReviewFlag == 0)
                                    && n.ReadyForReviewFlag == 1)
                }).OrderByDescending(w => w.IncidentNumber).ToList();

            var lstDisciplinaryIncidentNarrative = 
                _context.DisciplinaryIncidentNarrative.Where(din => din.DisciplinaryIncident.DisciplinaryActive == 1
                    && din.DisciplinaryIncident.FacilityId == facilityId)
                    .Select(a => new { 
                        a.DisciplinaryIncidentId,
                        PersonnelId = a.SupervisorReviewBy ?? 0,
                        a.SupervisorReviewByNavigation.PersonNavigation.PersonLastName,
                        a.SupervisorReviewByNavigation.PersonNavigation.PersonFirstName,
                        a.SupervisorReviewByNavigation.OfficerBadgeNum
                        }).ToList();

            narrativeSuperviorDetail.ForEach(item => {
                item.ReviewBy = lstDisciplinaryIncidentNarrative.Where(a => a.DisciplinaryIncidentId == item.DisciplinaryIncidentId)
                    .Select(s => new PersonnelVm
                    {
                        PersonnelId = s.PersonnelId,
                        PersonLastName = s.PersonLastName,
                        PersonFirstName = s.PersonFirstName,
                        OfficerBadgeNumber = s.OfficerBadgeNum
                    }).FirstOrDefault();
            });

            return narrativeSuperviorDetail;
        }

        private List<IncidentNarrativeDetailVm> GetSuperviorDetails(int facilityId, List<LookupVm> lookups)
        {
            List<IncidentNarrativeDetailVm> SuperviorDetails = _context.DisciplinaryInmate
                .Where(a => a.DisciplinaryIncident.DisciplinaryActive == 1 &&
                (!a.DeleteFlag.HasValue || a.DeleteFlag == 0) &&
                a.DisciplinaryIncident.FacilityId == facilityId &&
                a.DisciplinaryIncident.InitialEntryFlag == true &&
                a.DisciplinaryIncident.AllowHearingFlag == 1)
                .Select(a => new IncidentNarrativeDetailVm
                {
                    IncidentNumber = a.DisciplinaryIncident.DisciplinaryNumber,
                    IncDate = a.DisciplinaryIncident.DisciplinaryIncidentDate,
                    DisciplinaryIncidentId = a.DisciplinaryIncidentId,
                    DisciplinaryInmateId = a.DisciplinaryInmateId,
                    DisciplinaryType = a.DisciplinaryIncident.DisciplinaryType ?? 0,
                    Type = lookups.Where(l => l.LookupType == LookupConstants.DISCTYPE
                                              && l.LookupIndex == a.DisciplinaryIncident.DisciplinaryType)
                        .Select(l => l.LookupDescription).FirstOrDefault(),
                    Categorization = lookups.Where(l => l.LookupType == LookupConstants.INCCAT
                        && l.LookupIndex == a.DisciplinaryIncident.IncidentCategorizationIndex)
                        .Select(s => s.LookupDescription).FirstOrDefault(),
                    DispInmateByPassHearing = a.DisciplinaryInmateBypassHearing > 0,
                    NoticeFlag = a.NoticeFlag,
                    HearingComplete = a.HearingComplete,
                    Reviwed = a.DisciplinaryIncident.DisciplinaryIncidentNarrative.All(w =>
                          w.SupervisorReviewFlag == 1 && (!w.DeleteFlag.HasValue || w.DeleteFlag == 0)),
                    SensitiveMaterial = a.DisciplinaryIncident.SensitiveMaterial,
                    PreaOnly = a.DisciplinaryIncident.PreaOnly
                }).ToList();
            return SuperviorDetails;
        }

        public List<IncidentNarrativeDetailVm> NarrativeDetails(int facilityId)
        {
            List<LookupVm> lookups =
                _commonService.GetLookups(new[] { LookupConstants.DISCTYPE, LookupConstants.INCCAT });
            List<IncidentNarrativeDetailVm> incidentNarratives = _context
            .DisciplinaryIncidentNarrative.Where(w => w.CreateBy == _personnelId &&
                w.DisciplinaryIncident.FacilityId == facilityId &&
                (!w.DeleteFlag.HasValue || w.DeleteFlag == 0) &&
                w.DisciplinaryIncident.DisciplinaryActive == 1)
            .Select(s => new IncidentNarrativeDetailVm
            {
                DisciplinaryIncidentId = s.DisciplinaryIncident.DisciplinaryIncidentId,
                DisciplinaryActive = s.DisciplinaryIncident.DisciplinaryActive == 1,
                IncidentNumber = s.DisciplinaryIncident.DisciplinaryNumber,
                IncDate = s.DisciplinaryIncident.DisciplinaryIncidentDate,
                DisciplinaryReportDate = s.DisciplinaryIncident.DisciplinaryReportDate,
                Type = lookups.Where(l => l.LookupType == LookupConstants.DISCTYPE
                    && l.LookupIndex == s.DisciplinaryIncident.DisciplinaryType)
                    .Select(l => l.LookupDescription).FirstOrDefault(),
                DisciplinaryIncidentNarrative = s.DisciplinaryIncidentNarrative1,
                CreateDate = s.CreateDate,
                DisciplinaryIncidentNarrativeId = s.DisciplinaryIncidentNarrativeId,
                DisciplinaryOfficerId = s.CreateBy,
                SupervisorReviewFlag = s.SupervisorReviewFlag == 1,
                ReadyForReviewFlag = s.ReadyForReviewFlag == 1,
                SupervisorReviewBy = s.SupervisorReviewBy ?? 0,
                SupervisorReviewNote = s.SupervisorReviewNote,
                SensitiveMaterial = s.DisciplinaryIncident.SensitiveMaterial,
                PreaOnly = s.DisciplinaryIncident.PreaOnly,
                CreateByPersonnel = new PersonnelVm
                {
                    PersonnelId = s.CreateBy,
                    PersonLastName = s.CreateByNavigation.PersonNavigation.PersonLastName,
                    PersonFirstName = s.CreateByNavigation.PersonNavigation.PersonFirstName,
                    OfficerBadgeNumber = s.CreateByNavigation.OfficerBadgeNum
                },
                ReviewBy = new PersonnelVm
                {
                    PersonnelId = s.SupervisorReviewBy ?? 0,
                    PersonLastName = s.SupervisorReviewByNavigation.PersonNavigation.PersonLastName,
                    PersonFirstName = s.SupervisorReviewByNavigation.PersonNavigation.PersonFirstName,
                    OfficerBadgeNumber = s.SupervisorReviewByNavigation.OfficerBadgeNum
                },
                Categorization = lookups.Where(x => x.LookupType == LookupConstants.INCCAT
                    && x.LookupIndex == s.DisciplinaryIncident.IncidentCategorizationIndex)
                    .Select(x => x.LookupDescription).FirstOrDefault(),
                InitialEntryFlag = s.DisciplinaryIncident.InitialEntryFlag ?? false,
                AllowHearingFlag = s.DisciplinaryIncident.AllowHearingFlag > 0
            }).ToList();
            // For performance purpose
            int[] obj = _context.DisciplinaryInmate.Where(x => x.PersonnelId == _personnelId &&
            x.NarrativeFlag == true && incidentNarratives.Select(a => a.DisciplinaryIncidentId)
            .Contains(x.DisciplinaryIncidentId)).Select(a => a.DisciplinaryIncidentId).ToArray();

            incidentNarratives.ForEach(a =>
            {
                a.IsExpectedNarrative = obj.Any(x => x == a.DisciplinaryIncidentId);
            });

            return incidentNarratives;
        }

        public NarrativeCommonDetailVm NarrativeReview(int incidentId)
        {
            List<LookupVm> lookups = _commonService.GetLookups(new[] { LookupConstants.DISCINTYPE });
            NarrativeCommonDetailVm narrativeCommonDetail = _context.DisciplinaryIncident
                .Where(a => a.DisciplinaryIncidentId == incidentId)
                .Select(a => new NarrativeCommonDetailVm
                {
                    IncidentNarrativeDetails = a.DisciplinaryIncidentNarrative
                        .Where(x => !x.DeleteFlag.HasValue || x.DeleteFlag == 0)
                        .Select(x => new IncidentNarrativeDetailVm
                        {
                            DisciplinaryIncidentNarrative = x.DisciplinaryIncidentNarrative1,
                            SupervisorReviewNote = x.SupervisorReviewNote,
                            CreateDate = x.CreateDate,
                            DisciplinaryIncidentId = a.DisciplinaryIncidentId,
                            SupervisorReviewFlag = x.SupervisorReviewFlag == 1,
                            ReadyForReviewFlag = x.ReadyForReviewFlag == 1,
                            AllowHearingFlag = a.AllowHearingFlag == 1,
                            DisciplinaryIncidentNarrativeId = x.DisciplinaryIncidentNarrativeId,
                            SensitiveMaterial = a.SensitiveMaterial,
                            PreaOnly = a.PreaOnly,
                            CreateByPersonnel = new PersonnelVm
                            {
                                PersonLastName = x.CreateByNavigation.PersonNavigation.PersonLastName,
                                PersonFirstName = x.CreateByNavigation.PersonNavigation.PersonFirstName,
                                OfficerBadgeNumber = x.CreateByNavigation.OfficerBadgeNum,
                                PersonnelId = x.CreateBy
                            }
                        }).ToList(),
                    ReprtingOfficer = new PersonnelVm
                    {
                        PersonLastName = a.DisciplinaryOfficer.PersonNavigation.PersonLastName,
                        PersonFirstName = a.DisciplinaryOfficer.PersonNavigation.PersonFirstName,
                        OfficerBadgeNumber = a.DisciplinaryOfficer.OfficerBadgeNum,
                        PersonnelId = a.DisciplinaryOfficerId
                    },
                    DispOfficerNarrativeFlag = a.DisciplinaryOfficerNarrativeFlag ?? false,
                    InvolvedParties = a.DisciplinaryInmate.Where(x =>
                            x.PersonnelId > 0 && (!x.DeleteFlag.HasValue || x.DeleteFlag == 0))
                        .Select(x => new IncidentViewer
                        {
                            Personnel = new PersonVm
                            {
                                PersonLastName = x.Personnel.PersonNavigation.PersonLastName,
                                PersonFirstName = x.Personnel.PersonNavigation.PersonFirstName,
                                PersonMiddleName = x.Personnel.PersonNavigation.PersonMiddleName,
                                PersonId = x.PersonnelId.Value,
                                OfficerBadgeNumber = x.Personnel.OfficerBadgeNum
                            },
                            InmateType = lookups.Where(l =>
                                l.LookupIndex == x.DisciplinaryInmateType
                                && l.LookupType == LookupConstants.DISCINTYPE).Select(l => l.LookupDescription)
                                .FirstOrDefault(),
                            NarrativeFlag = x.NarrativeFlag ?? false,
                            NarrativeFlagNote = x.NarrativeFlagNote
                        }).ToList(),
                    IsNeedOfHearing = a.DisciplinaryInmate.Any(x => (!x.DeleteFlag.HasValue || x.DeleteFlag == 0)
                      && (!x.DisciplinaryInmateBypassHearing.HasValue || x.DisciplinaryInmateBypassHearing == 0)),
                    DispInmateIds = a.DisciplinaryInmate.Where(x => !x.DeleteFlag.HasValue || x.DeleteFlag == 0)
                        .Select(x => x.DisciplinaryInmateId).ToArray()
                }).Single();
            return narrativeCommonDetail;
        }

        public async Task<int> UpdateIncident(IncidentNarrativeDetailVm value)
        {
            DisciplinaryIncident updateValue = _context.DisciplinaryIncident.Single(f =>
                f.DisciplinaryIncidentId == value.DisciplinaryIncidentId);
            updateValue.AllowHearingFlag = value.AllowHearingFlag ? 1 : 0;
            updateValue.AllowHearingBy = null;
            updateValue.AllowHearingDate = null;
            updateValue.DisciplinarySupervisorAction = value.ActionNote;
            updateValue.DisciplinaryOfficerNarrativeFlag = value.DispOfficerNarrativeFlag;
            List<DisciplinaryInmate> disciplinaryInmates = _context.DisciplinaryInmate
                .Where(a => a.DisciplinaryIncidentId == value.DisciplinaryIncidentId).ToList();
            disciplinaryInmates.Where(a => a.PersonnelId > 0).ToList()
            .ForEach(a =>
            {
                a.NarrativeFlag = value.InvPartyNarratives.Find(x => x.Key == a.PersonnelId).Value;
                a.NarrativeFlagNote = value.NarrativeFlagNote.Find(x => x.Key == a.PersonnelId).Value;
            });
            int count = value.InvPartyNarratives.Count(a => a.Value);
            updateValue.ExpectedNarrativeCount = updateValue.DisciplinaryOfficerNarrativeFlag == true ? count + 1 : count;
            updateValue.SensitiveMaterial = value.SensitiveMaterial;
            updateValue.PreaOnly = value.PreaOnly;
            if (!value.IsNeedOfHearing && value.AllowHearingFlag && value.SupervisorReviewFlag==true)
            {
                disciplinaryInmates.ForEach(a =>
                {
                    AoWizardProgressDisciplinaryInmate disciplinaryWizard =
                   _context.AoWizardProgressDisciplinaryInmate.FirstOrDefault(d =>
                       d.DisciplinaryInmateId == a.DisciplinaryInmateId &&
                       d.AoWizardId == 11);
                    if ((disciplinaryWizard is null))
                    {
                        disciplinaryWizard = new AoWizardProgressDisciplinaryInmate
                        {
                            DisciplinaryInmateId = a.DisciplinaryInmateId,
                            AoWizardId = 11
                        };
                        _context.AoWizardProgressDisciplinaryInmate.Add(disciplinaryWizard);
                        _context.SaveChanges();
                        if (a.InmateId > 0 && a.DisciplinaryInmateBypassHearing > 0)
                        {
                            List<AoWizardStepProgress> stepProgresses = value.WizardFacilitySteps
                            .Where(w => w.Component.ComponentId == 73 || w.Component.ComponentId == 74 ||
                            w.Component.ComponentId == 79)
                            .Select(w => new AoWizardStepProgress
                            {
                                AoComponentId = w.Component.ComponentId,
                                AoWizardFacilityStepId = w.WizardFacilityStepId,
                                AoWizardProgressId = disciplinaryWizard.AoWizardProgressId,
                                StepComplete = true,
                                StepCompleteById = _personnelId,
                                StepCompleteDate = DateTime.Now
                            }).ToList();
                            _context.AoWizardStepProgress.AddRange(stepProgresses);
                            _context.SaveChanges();
                        }
                        else if ((a.PersonnelId > 0 || !string.IsNullOrEmpty(a.DisciplinaryOtherName))
                         && a.DisciplinaryInmateBypassHearing > 0)
                        {
                            List<AoWizardStepProgress> stepProgresses = value.WizardFacilitySteps
                            .Where(w => w.Component.ComponentId == 73 ||
                            w.Component.ComponentId == 79)
                            .Select(w => new AoWizardStepProgress
                            {
                                AoComponentId = w.Component.ComponentId,
                                AoWizardFacilityStepId = w.WizardFacilityStepId,
                                AoWizardProgressId = disciplinaryWizard.AoWizardProgressId,
                                StepComplete = true,
                                StepCompleteById = _personnelId,
                                StepCompleteDate = DateTime.Now
                            }).ToList();
                            _context.AoWizardStepProgress.AddRange(stepProgresses);
                            _context.SaveChanges();
                        }
                    }
                    else
                    {
                        if (a.InmateId > 0 && a.DisciplinaryInmateBypassHearing > 0)
                        {
                            value.WizardFacilitySteps.Where(w => w.Component.ComponentId == 73 ||
                             w.Component.ComponentId == 74 || w.Component.ComponentId == 79).ToList()
                            .ForEach(w =>
                            {
                                AoWizardStepProgress aoWizardStepProgress = _context.AoWizardStepProgress
                                .Where(x => x.AoWizardProgressId == disciplinaryWizard.AoWizardProgressId
                                && x.AoComponentId == w.Component.ComponentId).SingleOrDefault();
                                if (aoWizardStepProgress is null)
                                {
                                    _context.AoWizardStepProgress.Add(new AoWizardStepProgress
                                    {
                                        AoComponentId = w.Component.ComponentId,
                                        AoWizardFacilityStepId = w.WizardFacilityStepId,
                                        AoWizardProgressId = disciplinaryWizard.AoWizardProgressId,
                                        StepComplete = true,
                                        StepCompleteById = _personnelId,
                                        StepCompleteDate = DateTime.Now
                                    });
                                    _context.SaveChanges();
                                }
                                else
                                {
                                    aoWizardStepProgress.StepComplete = true;
                                    _context.SaveChanges();
                                }
                            });
                        }
                        else if ((a.PersonnelId > 0 || !string.IsNullOrEmpty(a.DisciplinaryOtherName))
                         && a.DisciplinaryInmateBypassHearing > 0)
                        {
                            value.WizardFacilitySteps.Where(w => w.Component.ComponentId == 73 ||
                            w.Component.ComponentId == 79).ToList()
                            .ForEach(w =>
                            {
                                AoWizardStepProgress aoWizardStepProgress = _context.AoWizardStepProgress
                                .Where(x => x.AoWizardProgressId == disciplinaryWizard.AoWizardProgressId
                                && x.AoComponentId == w.Component.ComponentId).SingleOrDefault();
                                if (aoWizardStepProgress is null)
                                {
                                    _context.AoWizardStepProgress.Add(new AoWizardStepProgress
                                    {
                                        AoComponentId = w.Component.ComponentId,
                                        AoWizardFacilityStepId = w.WizardFacilityStepId,
                                        AoWizardProgressId = disciplinaryWizard.AoWizardProgressId,
                                        StepComplete = true,
                                        StepCompleteById = _personnelId,
                                        StepCompleteDate = DateTime.Now
                                    });
                                    _context.SaveChanges();
                                }
                                else
                                {
                                    aoWizardStepProgress.StepComplete = true;
                                    _context.SaveChanges();
                                }
                            });
                        }
                    }
                });
                updateValue.DisciplinaryActive = 0;
            }
            return await _context.SaveChangesAsync();
        }

        public IncidentCountVm GetIncidentLocation(IncidentLocationVm incidentLocation)
        {
            IncidentCountVm incidentCount = new IncidentCountVm();
            GetIncidentByCount(incidentLocation, incidentCount);
            GetIncidentByViolation(incidentLocation, incidentCount);
            GetIncidentByPersonnel(incidentLocation, incidentCount);
            GetIncidentByCategorization(incidentLocation, incidentCount);
            return incidentCount;
        }

        private void GetIncidentByCount(IncidentLocationVm incidentLocation, IncidentCountVm incidentCount)
        {
            IQueryable<DisciplinaryIncident> disciplinaryIncident = _context.DisciplinaryIncident.Where(x =>
                (incidentLocation.FacilityId <= 0 || x.FacilityId == incidentLocation.FacilityId)
                    && (incidentLocation.LastHours <= 0 || x.DisciplinaryIncidentDate >=
                        DateTime.Now.AddHours(-incidentLocation.LastHours)
                        && x.DisciplinaryIncidentDate <= DateTime.Now)
                    && (!incidentLocation.FromDate.HasValue && !incidentLocation.ToDate.HasValue ||
                        x.DisciplinaryIncidentDate >= incidentLocation.FromDate.Value &&
                        x.DisciplinaryIncidentDate <= incidentLocation.ToDate.Value.AddDays(1).AddTicks(-1))
                    );

            if (!string.IsNullOrEmpty(incidentLocation.Building))
            {
                disciplinaryIncident = disciplinaryIncident
                    .Where(w => w.DisciplinaryHousingUnitLocation == incidentLocation.Building);
            }
            if (!string.IsNullOrEmpty(incidentLocation.Number))
            {
                disciplinaryIncident = disciplinaryIncident
                    .Where(w => w.DisciplinaryHousingUnitNumber == incidentLocation.Number);
            }
            if (!string.IsNullOrEmpty(incidentLocation.Bed))
            {
                disciplinaryIncident = disciplinaryIncident
                    .Where(w => w.DisciplinaryHousingUnitBed == incidentLocation.Bed);
            }
            if (!string.IsNullOrEmpty(incidentLocation.Location))
            {
                disciplinaryIncident = disciplinaryIncident.Where(s => s.DisciplinaryLocation == incidentLocation.Location);
            }
            incidentCount.IncidentDescriptionByTypeList = disciplinaryIncident
                .OrderByDescending(o => o.DisciplinaryIncidentDate)
                .Select(a => new IncidentDescription
                {
                    DisciplinaryType = a.DisciplinaryType,
                    DisciplinaryIncidentId = a.DisciplinaryIncidentId,
                    DisciplinaryNumber = a.DisciplinaryNumber,
                    DisciplinaryReportDate = a.DisciplinaryReportDate,
                    DisciplinaryIncidentDate = a.DisciplinaryIncidentDate,
                    DisciplinarySynopsis = a.DisciplinarySynopsis,
                    DisciplinaryOfficerId = a.DisciplinaryOfficerId,
                    IncidentCompelete = a.DisciplinaryActive ?? 0,
                    Categorization = _context.Lookup.Where(l => l.LookupIndex == a.IncidentCategorizationIndex
                        && l.LookupType == LookupConstants.INCCAT).Select(s => s.LookupDescription).SingleOrDefault()
                }).ToList();

            incidentCount.IncidentDescriptionByTypeList.ForEach(f => f.LookupDescription = _context.Lookup.Where(l =>
                l.LookupIndex == f.DisciplinaryType && l.LookupType == LookupConstants.DISCTYPE)
                .Select(s => s.LookupDescription).SingleOrDefault());

            incidentCount.IncidentByTypeCount = incidentCount.IncidentDescriptionByTypeList.GroupBy(g =>
                g.LookupDescription).OrderBy(o => o.Key).Select(s => new GridInfoVm
                {
                    Cnt = s.Count(),
                    Name = s.Key,
                    Id = s.Select(x => x.DisciplinaryType ?? 0).FirstOrDefault()
                }).ToList();
        }

        private void GetIncidentByCategorization(IncidentLocationVm incidentLocation, IncidentCountVm incidentCount)
        {
            IQueryable<DisciplinaryIncident> disciplinaryIncidents = _context.DisciplinaryIncident
                .Where(x => (incidentLocation.FacilityId <= 0 || x.FacilityId == incidentLocation.FacilityId)
                            && (incidentLocation.LastHours <= 0 || x.DisciplinaryIncidentDate >=
                                DateTime.Now.AddHours(-incidentLocation.LastHours)
                                && x.DisciplinaryIncidentDate <= DateTime.Now)
                            && (!incidentLocation.FromDate.HasValue && !incidentLocation.ToDate.HasValue ||
                                x.DisciplinaryIncidentDate >= incidentLocation.FromDate.Value &&
                                x.DisciplinaryIncidentDate <= incidentLocation.ToDate.Value.AddDays(1).AddTicks(-1))
                            && (string.IsNullOrEmpty(incidentLocation.Building)
                                || x.DisciplinaryHousingUnitLocation == incidentLocation.Building)
                            && (string.IsNullOrEmpty(incidentLocation.Number)
                                || x.DisciplinaryHousingUnitNumber == incidentLocation.Number)
                            && (string.IsNullOrEmpty(incidentLocation.Bed)
                                || x.DisciplinaryHousingUnitBed == incidentLocation.Bed)
                            && (string.IsNullOrEmpty(incidentLocation.Location)
                                || x.DisciplinaryHousingUnitLocation == incidentLocation.Location));

            incidentCount.CategorizationTypeList = disciplinaryIncidents
                .OrderByDescending(o => o.DisciplinaryIncidentDate)
                .Select(a => new IncidentDescription
                {
                    DisciplinaryType = a.DisciplinaryType,
                    DisciplinaryIncidentId = a.DisciplinaryIncidentId,
                    DisciplinaryNumber = a.DisciplinaryNumber,
                    DisciplinaryReportDate = a.DisciplinaryReportDate,
                    DisciplinaryIncidentDate = a.DisciplinaryIncidentDate,
                    DisciplinarySynopsis = a.DisciplinarySynopsis,
                    DisciplinaryOfficerId = a.DisciplinaryOfficerId,
                    IncidentCompelete = a.DisciplinaryActive ?? 0,
                    CategorizationIndex = a.IncidentCategorizationIndex,
                    Categorization = _context.Lookup.Where(l =>
                            l.LookupIndex == a.IncidentCategorizationIndex && l.LookupType == LookupConstants.INCCAT)
                        .Select(s => s.LookupDescription).SingleOrDefault(),
                    LookupDescription = _context.Lookup.Where(l => l.LookupIndex == a.DisciplinaryType
                        && l.LookupType == LookupConstants.DISCTYPE).Select(s => s.LookupDescription).SingleOrDefault()
                }).ToList();

            incidentCount.CategorizationCount = incidentCount.CategorizationTypeList.GroupBy(g =>
                g.Categorization).OrderBy(o => o.Key).Select(s => new GridInfoVm
                {
                    Cnt = s.Count(),
                    Name = s.Key,
                    Id = s.Select(x => x.CategorizationIndex).FirstOrDefault()
                }).ToList();
        }

        private void GetIncidentByViolation(IncidentLocationVm incidentLocation, IncidentCountVm incidentCount)
        {
            incidentCount.IncidentDescriptionViolationList = _context.DisciplinaryControlXref
                .Include(dis => dis.DisciplinaryInmate).Where(x =>
                    !x.DisciplinaryInmate.DeleteFlag.HasValue && (incidentLocation.FacilityId <= 0 ||
                          x.DisciplinaryInmate.DisciplinaryIncident.FacilityId == incidentLocation.FacilityId) &&
                    (incidentLocation.LastHours <= 0 ||
                     x.DisciplinaryInmate.DisciplinaryIncident.DisciplinaryIncidentDate
                     >= DateTime.Now.AddHours(-incidentLocation.LastHours) &&
                     x.DisciplinaryInmate.DisciplinaryIncident.DisciplinaryIncidentDate <= DateTime.Now) &&
                    (!incidentLocation.FromDate.HasValue && !incidentLocation.ToDate.HasValue ||
                     x.DisciplinaryInmate.DisciplinaryIncident.DisciplinaryIncidentDate >=
                     incidentLocation.FromDate.Value
                     && x.DisciplinaryInmate.DisciplinaryIncident.DisciplinaryIncidentDate <=
                     incidentLocation.ToDate.Value.AddDays(1).AddTicks(-1)) &&
                    (string.IsNullOrEmpty(incidentLocation.Building) ||
                     x.DisciplinaryInmate.DisciplinaryIncident.DisciplinaryHousingUnitLocation ==
                     incidentLocation.Building)
                    && (string.IsNullOrEmpty(incidentLocation.Number) ||
                        x.DisciplinaryInmate.DisciplinaryIncident.DisciplinaryHousingUnitNumber ==
                        incidentLocation.Number) &&
                    (string.IsNullOrEmpty(incidentLocation.Bed) ||
                     x.DisciplinaryInmate.DisciplinaryIncident.DisciplinaryHousingUnitBed == incidentLocation.Bed) &&
                    (string.IsNullOrEmpty(incidentLocation.Location) ||
                     x.DisciplinaryInmate.DisciplinaryIncident.DisciplinaryHousingUnitLocation ==
                     incidentLocation.Location))
                .Select(s => new IncidentDescription
                {
                    DisciplinaryType = s.DisciplinaryInmate.DisciplinaryIncident.DisciplinaryType,
                    DisciplinaryIncidentId = s.DisciplinaryInmate.DisciplinaryIncident.DisciplinaryIncidentId,
                    DisciplinaryNumber = s.DisciplinaryInmate.DisciplinaryIncident.DisciplinaryNumber,
                    DisciplinaryReportDate = s.DisciplinaryInmate.DisciplinaryIncident.DisciplinaryReportDate,
                    DisciplinaryIncidentDate = s.DisciplinaryInmate.DisciplinaryIncident.DisciplinaryIncidentDate,
                    DisciplinarySynopsis = s.DisciplinaryInmate.DisciplinaryIncident.DisciplinarySynopsis,
                    InmateId = s.DisciplinaryInmate.InmateId ?? 0,
                    IncidentType = _context.Lookup.Where(t =>
                        t.LookupIndex == s.DisciplinaryInmate.DisciplinaryIncident.DisciplinaryType
                        && t.LookupType == LookupConstants.DISCTYPE).Select(z => z.LookupDescription).SingleOrDefault(),
                    Person = new PersonInfoVm
                    {
                        PersonLastName = s.DisciplinaryInmate.Inmate.Person.PersonLastName,
                        PersonFirstName = s.DisciplinaryInmate.Inmate.Person.PersonFirstName,
                        PersonMiddleName = s.DisciplinaryInmate.Inmate.Person.PersonMiddleName,
                        InmateNumber = s.DisciplinaryInmate.Inmate.InmateNumber
                    },
                    PhotoGraphPath = s.DisciplinaryInmate.Inmate != null
                        ? _photo.GetPhotoByPerson(s.DisciplinaryInmate.Inmate.Person)
                        : null,
                    IncidentCompelete = s.DisciplinaryInmate.DisciplinaryIncident.DisciplinaryActive ?? 0,
                    DisciplinaryViolationId = s.DisciplinaryControlViolationId,
                    Categorization = _context.Lookup.Where(l =>
                        l.LookupIndex == s.DisciplinaryInmate.DisciplinaryIncident.IncidentCategorizationIndex &&
                        l.LookupType == LookupConstants.INCCAT).Select(ss => ss.LookupDescription).SingleOrDefault()
                }).ToList();

            incidentCount.IncidentDescriptionViolationList.ForEach(f => f.LookupDescription =
                _context.DisciplinaryControlLookup.Where(w =>
                w.DisciplinaryControlLookupType == (int?)DisciplinaryLookup.DISCVIOL
                && w.DisciplinaryControlLookupId == f.DisciplinaryViolationId)
                    .Select(s => s.DisciplinaryControlLookupDescription).SingleOrDefault());

            incidentCount.IncidentViolationCount = incidentCount.IncidentDescriptionViolationList
                .GroupBy(g => g.LookupDescription).OrderBy(o => o.Key).Select(s => new GridInfoVm
                {
                    Cnt = s.Count(),
                    Name = s.Key,
                    Id = s.Select(x => x.DisciplinaryViolationId ?? 0).FirstOrDefault()
                }).ToList();
        }

        private void GetIncidentByPersonnel(IncidentLocationVm incidentLocation, IncidentCountVm incidentCount)
        {
            IQueryable<DisciplinaryIncident> disciplinaryIncident = _context.DisciplinaryIncident
                .Where(x => (incidentLocation.FacilityId <= 0 || x.FacilityId == incidentLocation.FacilityId)
                            && (incidentLocation.LastHours <= 0 || x.DisciplinaryIncidentDate >=
                                DateTime.Now.AddHours(-incidentLocation.LastHours)
                                && x.DisciplinaryIncidentDate <= DateTime.Now)
                            && (!incidentLocation.FromDate.HasValue && !incidentLocation.ToDate.HasValue ||
                                x.DisciplinaryIncidentDate >= incidentLocation.FromDate.Value &&
                                x.DisciplinaryIncidentDate <= incidentLocation.ToDate.Value.AddDays(1).AddTicks(-1))
                            && (string.IsNullOrEmpty(incidentLocation.Building)
                                || x.DisciplinaryHousingUnitLocation == incidentLocation.Building)
                            && (string.IsNullOrEmpty(incidentLocation.Number)
                                || x.DisciplinaryHousingUnitNumber == incidentLocation.Number)
                            && (string.IsNullOrEmpty(incidentLocation.Bed)
                                || x.DisciplinaryHousingUnitBed == incidentLocation.Bed)
                            && (string.IsNullOrEmpty(incidentLocation.Location)
                                || x.DisciplinaryHousingUnitLocation == incidentLocation.Location));
            switch (incidentLocation.PersonnelType)
            {
                case OfficerType.NARRATIVEBY:
                    incidentCount.IncidentDescriptionPersonnelList = disciplinaryIncident
                        .OrderByDescending(o => o.DisciplinaryIncidentDate).SelectMany(
                            a => a.DisciplinaryIncidentNarrative.Where(x =>
                                x.DisciplinaryIncidentId == a.DisciplinaryIncidentId)
                            , (a, x) => new IncidentDescription
                            {
                                DisciplinaryType = a.DisciplinaryType,
                                DisciplinaryIncidentId = a.DisciplinaryIncidentId,
                                DisciplinaryNumber = a.DisciplinaryNumber,
                                DisciplinaryReportDate = a.DisciplinaryReportDate,
                                DisciplinaryIncidentDate = a.DisciplinaryIncidentDate,
                                DisciplinarySynopsis = a.DisciplinarySynopsis,
                                DisciplinaryOfficerId = a.DisciplinaryOfficerId,
                                IncidentCompelete = a.DisciplinaryActive ?? 0,
                                PersonnelId = x.CreateBy,
                                Personnel = new PersonnelVm
                                {
                                    PersonLastName = x.CreateByNavigation.PersonNavigation.PersonLastName,
                                    PersonFirstName = x.CreateByNavigation.PersonNavigation.PersonLastName,
                                    PersonnelNumber = x.CreateByNavigation.PersonnelNumber,
                                },
                                Categorization = _context.Lookup.Where(l => l.LookupIndex == a.IncidentCategorizationIndex
                                    && l.LookupType == LookupConstants.INCCAT).Select(s => s.LookupDescription).SingleOrDefault()
                            }).ToList();
                    break;
                case OfficerType.REVIEWBY:
                    incidentCount.IncidentDescriptionPersonnelList = disciplinaryIncident
                        .OrderByDescending(o => o.DisciplinaryIncidentDate).SelectMany(
                            a => a.DisciplinaryInmate.Where(x =>
                                x.DisciplinaryIncidentId == a.DisciplinaryIncidentId && x.DisciplinaryReviewOfficer > 0)
                            , (a, x) => new IncidentDescription
                            {
                                DisciplinaryType = a.DisciplinaryType,
                                DisciplinaryIncidentId = a.DisciplinaryIncidentId,
                                DisciplinaryNumber = a.DisciplinaryNumber,
                                DisciplinaryReportDate = a.DisciplinaryReportDate,
                                DisciplinaryIncidentDate = a.DisciplinaryIncidentDate,
                                DisciplinarySynopsis = a.DisciplinarySynopsis,
                                DisciplinaryOfficerId = a.DisciplinaryOfficerId,
                                IncidentCompelete = a.DisciplinaryActive ?? 0,
                                PersonnelId = x.DisciplinaryReviewOfficer.Value,
                                Personnel = _context.Personnel
                                    .Where(d => d.PersonnelId == x.DisciplinaryReviewOfficer.Value).Select(i =>
                                        new PersonnelVm
                                        {
                                            PersonLastName = i.PersonNavigation.PersonLastName,
                                            PersonFirstName = i.PersonNavigation.PersonFirstName,
                                            PersonnelNumber = i.PersonnelNumber
                                        }).SingleOrDefault(),
                                Categorization = _context.Lookup.Where(l => l.LookupIndex == a.IncidentCategorizationIndex
                                    && l.LookupType == LookupConstants.INCCAT).Select(s => s.LookupDescription).SingleOrDefault()
                            }).ToList();
                    break;
                case OfficerType.HEARINGBY:
                    incidentCount.IncidentDescriptionPersonnelList = disciplinaryIncident
                        .OrderByDescending(o => o.DisciplinaryIncidentDate).SelectMany(
                            a => a.DisciplinaryInmate.Where(x =>
                                x.DisciplinaryIncidentId == a.DisciplinaryIncidentId &&
                                x.DisciplinaryHearingOfficer1 > 0)
                            , (a, x) => new IncidentDescription
                            {
                                DisciplinaryType = a.DisciplinaryType,
                                DisciplinaryIncidentId = a.DisciplinaryIncidentId,
                                DisciplinaryNumber = a.DisciplinaryNumber,
                                DisciplinaryReportDate = a.DisciplinaryReportDate,
                                DisciplinaryIncidentDate = a.DisciplinaryIncidentDate,
                                DisciplinarySynopsis = a.DisciplinarySynopsis,
                                DisciplinaryOfficerId = a.DisciplinaryOfficerId,
                                PersonnelId = x.DisciplinaryHearingOfficer1.Value,
                                IncidentCompelete = a.DisciplinaryActive ?? 0,
                                Categorization = _context.Lookup.Where(l => l.LookupIndex == a.IncidentCategorizationIndex
                                    && l.LookupType == LookupConstants.INCCAT).Select(s => s.LookupDescription).SingleOrDefault(),
                                Personnel = _context.Personnel
                                    .Where(d => d.PersonnelId == x.DisciplinaryHearingOfficer1.Value).Select(i =>
                                        new PersonnelVm
                                        {
                                            PersonLastName = i.PersonNavigation.PersonLastName,
                                            PersonFirstName = i.PersonNavigation.PersonFirstName,
                                            PersonnelNumber = i.PersonnelNumber
                                        }).SingleOrDefault()
                            }).ToList();
                    break;
                case OfficerType.COMPLETEBY:
                    incidentCount.IncidentDescriptionPersonnelList = disciplinaryIncident
                        .OrderByDescending(o => o.DisciplinaryIncidentDate).SelectMany(
                            a => a.DisciplinaryInmate.Where(x =>
                                x.DisciplinaryIncidentId == a.DisciplinaryIncidentId &&
                                x.DisciplinaryReviewCompleteOfficer > 0)
                            , (a, x) => new IncidentDescription
                            {
                                DisciplinaryType = a.DisciplinaryType,
                                DisciplinaryIncidentId = a.DisciplinaryIncidentId,
                                DisciplinaryNumber = a.DisciplinaryNumber,
                                DisciplinaryReportDate = a.DisciplinaryReportDate,
                                DisciplinaryIncidentDate = a.DisciplinaryIncidentDate,
                                DisciplinarySynopsis = a.DisciplinarySynopsis,
                                DisciplinaryOfficerId = a.DisciplinaryOfficerId,
                                IncidentCompelete = a.DisciplinaryActive ?? 0,
                                PersonnelId = x.DisciplinaryReviewCompleteOfficer.Value,
                                Categorization = _context.Lookup.Where(l => l.LookupIndex == a.IncidentCategorizationIndex
                                    && l.LookupType == LookupConstants.INCCAT).Select(s => s.LookupDescription).SingleOrDefault(),
                                Personnel = _context.Personnel
                                    .Where(d => d.PersonnelId == x.DisciplinaryReviewCompleteOfficer.Value).Select(i =>
                                        new PersonnelVm
                                        {
                                            PersonLastName = i.PersonNavigation.PersonLastName,
                                            PersonFirstName = i.PersonNavigation.PersonFirstName,
                                            PersonnelNumber = i.PersonnelNumber
                                        }).SingleOrDefault()
                            }).ToList();
                    break;
                default:
                    incidentCount.IncidentDescriptionPersonnelList = disciplinaryIncident
                        .OrderByDescending(o => o.DisciplinaryIncidentDate).Select(a => new IncidentDescription
                        {
                            DisciplinaryType = a.DisciplinaryType,
                            DisciplinaryIncidentId = a.DisciplinaryIncidentId,
                            DisciplinaryNumber = a.DisciplinaryNumber,
                            DisciplinaryReportDate = a.DisciplinaryReportDate,
                            DisciplinaryIncidentDate = a.DisciplinaryIncidentDate,
                            DisciplinarySynopsis = a.DisciplinarySynopsis,
                            DisciplinaryOfficerId = a.DisciplinaryOfficerId,
                            PersonnelId = a.DisciplinaryOfficerId,
                            IncidentCompelete = a.DisciplinaryActive ?? 0,
                            Categorization = _context.Lookup.Where(l => l.LookupIndex == a.IncidentCategorizationIndex
                                && l.LookupType == LookupConstants.INCCAT).Select(s => s.LookupDescription).SingleOrDefault(),
                            Personnel = _context.Personnel.Where(d => d.PersonnelId == a.DisciplinaryOfficerId).Select(
                                i => new PersonnelVm
                                {
                                    PersonLastName = i.PersonNavigation.PersonLastName,
                                    PersonFirstName = i.PersonNavigation.PersonFirstName,
                                    PersonnelNumber = i.PersonnelNumber
                                }).SingleOrDefault()
                        }).ToList();
                    break;
            }

            incidentCount.IncidentDescriptionPersonnelList.ForEach(f =>
            {
                f.LookupDescription = _context.Lookup.Where(l => l.LookupIndex == f.DisciplinaryType
                                                                 && l.LookupType == LookupConstants.DISCTYPE)
                    .Select(s => s.LookupDescription).SingleOrDefault();
            });
            incidentCount.IncidentPersonnelCountList = incidentCount.IncidentDescriptionPersonnelList.GroupBy(g =>
                g.PersonnelId).OrderBy(o => o.Key).Select(s => new GridInfoVm
                {
                    Cnt = s.Count(),
                    Name = s.Select(x => new
                    {
                        personName =
                            $"{x.Personnel.PersonLastName},{x.Personnel.PersonFirstName} {x.Personnel.PersonnelNumber}"
                    }).FirstOrDefault()?.personName,
                    Id = s.Select(x => x.PersonnelId).FirstOrDefault()
                }).ToList();
        }

        public async Task<int> UpdateApproveDeny(IncidentNarrativeDetailVm value)
        {
            DisciplinaryIncidentNarrative updateValue = _context.DisciplinaryIncidentNarrative.Single(f =>
                f.DisciplinaryIncidentId == value.DisciplinaryIncidentId &&
                f.DisciplinaryIncidentNarrativeId == value.DisciplinaryIncidentNarrativeId);
            updateValue.SupervisorReviewBy = _personnelId;
            switch (value.OperationFlag)
            {
                case NarrativeOperations.Approve:
                    {
                        updateValue.SupervisorReviewDate = DateTime.Now;
                        updateValue.SupervisorReviewNote = null;
                        updateValue.SupervisorReviewFlag = 1;
                        break;
                    }
                case NarrativeOperations.Deny:
                    {
                        updateValue.SupervisorReviewDate = DateTime.Now;
                        updateValue.SupervisorReviewNote = value.SupervisorReviewNote;
                        updateValue.ReadyForReviewFlag = null;
                        updateValue.ReadyForReviewDate = null;
                        updateValue.SupervisorReviewFlag = null;
                        break;
                    }
                case NarrativeOperations.Undo:
                    {
                        updateValue.SupervisorReviewFlag = 0;
                        updateValue.SupervisorReviewDate = null;
                        break;
                    }
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return await _context.SaveChangesAsync();
        }

        #region "Incident Wizard"

        public List<AoWizardProgressVm> GetIncidentWizardProgress(int[] dispInmateIds)
        {
            return _context.AoWizardProgressDisciplinaryInmate
                .Where(a => dispInmateIds.Contains(a.DisciplinaryInmateId))
                .Select(wp => new AoWizardProgressVm
                {
                    WizardProgressId = wp.AoWizardProgressId,
                    WizardId = wp.AoWizardId,
                    DisciplinaryInmateId = wp.DisciplinaryInmateId,
                    WizardStepProgress = wp.AoWizardStepProgress
                        .Select(wsp => new AoWizardStepProgressVm
                        {
                            WizardStepProgressId = wsp.AoWizardStepProgressId,
                            WizardProgressId = wsp.AoWizardProgressId,
                            ComponentId = wsp.AoComponentId,
                            Component = new AoComponentVm
                            {
                                AppAoFunctionalityId = wsp.AoComponent.AppAofunctionalityId,
                                CanChangeVisibility = wsp.AoComponent.CanChangeVisibility,
                                ComponentId = wsp.AoComponent.AoComponentId,
                                ComponentName = wsp.AoComponent.ComponentName,
                                CustomFieldAllowed = wsp.AoComponent.CustomFieldAllowed,
                                CustomFieldKeyName = wsp.AoComponent.CustomFieldKeyName,
                                CustomFieldTableName = wsp.AoComponent.CustomFieldTableName,
                                DisplayName = wsp.AoComponent.DisplayName,
                                HasConfigurableFields = wsp.AoComponent.HasConfigurableFields,
                                IsLastScreen = wsp.AoComponent.IsLastScreen
                            },
                            StepComplete = wsp.StepComplete,
                            StepCompleteBy = new PersonnelVm
                            {
                                PersonnelId = wsp.StepCompleteBy.PersonnelId,
                                OfficerBadgeNumber = wsp.StepCompleteBy.OfficerBadgeNum,
                                PersonLastName = wsp.StepCompleteBy.PersonNavigation.PersonLastName,
                                PersonFirstName = wsp.StepCompleteBy.PersonNavigation.PersonFirstName,
                                PersonMiddleName = wsp.StepCompleteBy.PersonNavigation.PersonMiddleName
                            },
                            StepCompleteDate = wsp.StepCompleteDate,
                            StepCompleteNote = wsp.StepCompleteNote
                        }).ToList()
                }).ToList();
        }

        #endregion

        public KeyValuePair<int, int> GetAcceptLogic(int dispInmateId)
        {
            IQueryable<DisciplinaryControlXref> disciplinaryControlXrefs = _context.DisciplinaryControlXref
                .Where(a => a.DisciplinaryInmateId == dispInmateId);
            IQueryable<DisciplinaryControlLookup> dispCtrlLookup1 =
                _context.DisciplinaryControlLookup.Where(a => a.RecommendFlag == 1);
            IQueryable<DisciplinaryControlLookup> dispCtrlLookup2 =
                _context.DisciplinaryControlLookup.Where(a => a.FinalSancFlag == 1);
            var result = disciplinaryControlXrefs.SelectMany(a => dispCtrlLookup1
                .Where(x => x.DisciplinaryControlLookupId == a.DisciplinaryControlViolationId), (a, x) => new
                {
                    a.DisciplinaryControlXrefId,
                    x.RecommendRank,
                    a.DisciplinaryControlSanctionId
                });
            result = result.SelectMany(
                a => dispCtrlLookup2.Where(x => x.DisciplinaryControlLookupId == a.DisciplinaryControlSanctionId),
                (a, x) => a);
            return result.OrderByDescending(a => a.RecommendRank)
                .Select(a => new KeyValuePair<int, int>(a.DisciplinaryControlXrefId, a.RecommendRank ?? 0))
                .FirstOrDefault();
        }

        public ViolationDetails CheckSanction(int dispCtrlXrefId)
        {
            DisciplinaryControlXref dispCtrlXref = _context.DisciplinaryControlXref
                .Single(a => a.DisciplinaryControlXrefId == dispCtrlXrefId);
            DisciplinaryControlLookup controlLookup = _context.DisciplinaryControlLookup
                .Single(a => a.DisciplinaryControlLookupId == dispCtrlXref.DisciplinaryControlSanctionId);
            ViolationDetails violationDetails = new ViolationDetails
            {
                DispControlXrefId = dispCtrlXref.DisciplinaryControlXrefId,
                DispControlSanctionDays = dispCtrlXref.DisciplinaryControlSanctionDays,
                FinalSancFlag = controlLookup.FinalSancFlag,
                FinalSancPrivilegeId = controlLookup.FinalSancPrivilegeId,
                FinalSancSentDays = controlLookup.FinalSancSentDays
            };
            return violationDetails;
        }

        public List<DiscDaysHistory> GetDiscDaysHistories(int inmateId)
        {
            List<DiscDaysHistory> discDaysHistories = _context.Incarceration.Where(a => a.InmateId == inmateId)
                .OrderByDescending(a => a.ReleaseOut)
                .Select(a => new DiscDaysHistory
                {
                    IncarcerationId = a.IncarcerationId,
                    InmateId = a.InmateId,
                    ReleaseOut = a.ReleaseOut ?? DateTime.Now,
                    DateIn = a.DateIn,
                    UsedPersonLast = a.UsedPersonLast,
                    UsedPersonFrist = a.UsedPersonFrist,
                    UsedPersonMiddle = a.UsedPersonMiddle,
                    UsedPersonSuffix = a.UsedPersonSuffix
                }).ToList();
            IQueryable<DisciplinaryInmate> disciplinaryInmates = _context.DisciplinaryInmate
                .Where(a => discDaysHistories.Where(x => x.InmateId > 0).Select(x => x.InmateId).Contains(a.InmateId)
                            && a.DisciplinaryDays > 0);
            List<LookupVm> lookupList = _commonService.GetLookups(new[] { LookupConstants.DISCTYPE });
            List<DiscDays> discDaysList = disciplinaryInmates.SelectMany(a => _context.DisciplinarySentDayXref
                .Where(x => x.DisciplinaryInmateId == a.DisciplinaryInmateId && (!x.DeleteFlag.HasValue
                                                                                 || a.DeleteFlag == 0)), (a, x) =>
                new DiscDays
                {
                    InmateId = a.InmateId,
                    DisciplinaryIncidentDate = a.DisciplinaryIncident.DisciplinaryIncidentDate,
                    CreateDate = x.CreateDate,
                    DisciplinaryNumber = a.DisciplinaryIncident.DisciplinaryNumber,
                    IncidentType = lookupList.Where(l =>
                            l.LookupIndex == a.DisciplinaryIncident.DisciplinaryType)
                        .Select(l => l.LookupDescription).SingleOrDefault(),
                    DisciplinaryDays = a.DisciplinaryDays
                }).ToList();
            discDaysHistories.ForEach(a =>
            {
                a.DiscDays = discDaysList.Where(x =>
                        x.InmateId == a.InmateId && x.CreateDate.HasValue && x.CreateDate >= a.DateIn
                        && x.CreateDate <= a.ReleaseOut).OrderByDescending(x => x.DisciplinaryIncidentDate)
                    .GroupBy(x => x.InmateId).Select(x => x.First()).ToList();
            });
            return discDaysHistories;
        }

        public List<SentenceVm> GetSentenceDetails(int inmateId)
        {
            int incarcerationId = GetIncarcerationId(inmateId);
            List<SentenceVm> sentences = _bookingService.GetBookingSentence(incarcerationId);
            return sentences;
        }

        public async Task<int> SaveAppliedBookings(List<AppliedCharge> applieds)
        {
            _context.DisciplinarySentDayXref.AddRange(applieds.Select(a => new DisciplinarySentDayXref
            {
                DisciplinaryInmateId = a.DispInmateId,
                ArrestId = a.ArrestId,
                CreateDate = DateTime.Now,
                CreateBy = _personnelId,
                CrimeId = a.CrimeId > 0 ? a.CrimeId : (int?)null
            }));
            int[] crimeIds = applieds.Where(a => a.CrimeId > 0).Select(a => a.CrimeId).ToArray();
            List<Crime> crimes = _context.Crime.Where(a => crimeIds.Contains(a.CrimeId)).ToList();
            crimes.ForEach(a => { a.ArrestSentenceDisciplinaryDaysFlag = 1; });
            int[] arrestIds = applieds.Select(x => x.ArrestId ?? 0).ToArray();
            List<Arrest> arrests = _context.Arrest.Where(a => arrestIds.Contains(a.ArrestId)).ToList();
            arrests.ForEach(a => { a.ArrestSentenceDisciplinaryDaysFlag = 1; });
            DisciplinaryInmate disciplinaryInmate = _context.DisciplinaryInmate
                .Single(a => a.DisciplinaryInmateId == applieds.First().DispInmateId);
            disciplinaryInmate.DisciplinaryDaysSentFlag = 0;
            return await _context.SaveChangesAsync();
        }

        public List<ClearChargesVm> GetSentenceCharges(int inmateId)
        {
            int incarcerationId = GetIncarcerationId(inmateId);
            IQueryable<Crime> crimes = _context.Crime.Where(a => a.CrimeDeleteFlag == 0);
            IQueryable<IncarcerationArrestXref> incarcerationArrestXrefs = _context.IncarcerationArrestXref
                .Where(a => a.Incarceration.IncarcerationId == incarcerationId);
            List<LookupVm> lookupList = _commonService.GetLookups(new[]
            {
                LookupConstants.CHARGEQUALIFIER,
                LookupConstants.CRIMETYPE
            });
            List<ClearChargesVm> sentenceCharges = incarcerationArrestXrefs.SelectMany(a => crimes
                .Where(x => x.ArrestId == a.Arrest.ArrestId), (a, x) => new ClearChargesVm
                {
                    ArrestId = a.Arrest.ArrestId,
                    CrimeId = x.CrimeId,
                    ArrestBookingNumber = a.Arrest.ArrestBookingNo,
                    CrimeNumber = x.CrimeNumber,
                    Count = x.CrimeCount,
                    Type = x.CrimeLookup.CrimeCodeType,
                    Qualifier = lookupList.Where(l => l.LookupIndex == x.CrimeQualifierLookup)
                    .Select(l => l.LookupDescription).SingleOrDefault(),
                    Section = x.CrimeLookup.CrimeSection,
                    Description = x.CrimeLookup.CrimeDescription,
                    Statute = x.CrimeLookup.CrimeStatuteCode,
                    Status = lookupList.Where(l => l.LookupIndex == Convert.ToInt32(x.CrimeType))
                    .Select(l => l.LookupDescription).SingleOrDefault(),
                    StartDate = x.ArrestSentenceStartDate,
                    ArrestSentenceConsecutiveFlag = x.ArrestSentenceConsecutiveFlag,
                    UseStartDate = x.ArrestSentenceUseStartDate,
                    ArrestSentenceDays = x.ArrestSentenceDays,
                    ArrestSentenceMethodId = x.ArrestSentenceMethodId,
                    ArrestSentenceDaysToServe = x.ArrestSentenceDaysToServe,
                    ArrestSentenceActualDaysToServe = x.ArrestSentenceActualDaysToServe,
                    ClearDate = x.ArrestSentenceReleaseDate
                }).ToList();
            List<ArrestSentenceMethod> arrestSentenceMethods = _context.ArrestSentenceMethod
                .Where(a => sentenceCharges.Where(x => x.ArrestSentenceMethodId > 0)
                    .Select(x => x.ArrestSentenceMethodId).Contains(a.ArrestSentenceMethodId)).ToList();
            sentenceCharges.ForEach(a =>
            {
                a.Method = arrestSentenceMethods
                    .SingleOrDefault(x => x.ArrestSentenceMethodId == a.ArrestSentenceMethodId)?.MethodName;
            });
            return sentenceCharges.OrderBy(a => a.ArrestId).ThenBy(a => a.CrimeId).ToList();
        }

        private int GetIncarcerationId(int inmateId)
        {
            IQueryable<Incarceration> incarcerations = _context.Incarceration.Where(a => a.InmateId == inmateId);
            int incarcerationId = incarcerations.Any(a => !a.ReleaseOut.HasValue)
                ? incarcerations.First(a => !a.ReleaseOut.HasValue).IncarcerationId
                : incarcerations.OrderByDescending(a => a.IncarcerationId).FirstOrDefault()?.IncarcerationId ?? 0;

            return incarcerationId;
        }

        public async Task<int> DeleteApplyCharges(DeleteRequest deleteParams)
        {
            DisciplinarySentDayXref disciplinarySentDayXref = _context.DisciplinarySentDayXref
                .Single(a => a.DisciplinarySentDayXrefId == deleteParams.DispSentDayXrefId);
            disciplinarySentDayXref.DeleteFlag = 1;
            disciplinarySentDayXref.DeleteDate = DateTime.Now;
            disciplinarySentDayXref.DeleteBy = _personnelId;
            disciplinarySentDayXref.CrimeId = deleteParams.CrimeId;
            Crime crime = _context.Crime.Single(a => a.CrimeId == deleteParams.CrimeId);
            crime.ArrestSentenceDisciplinaryDaysFlag = 1;
            return await _context.SaveChangesAsync();
        }

        public async Task<int> UpdateDispDaySentFlag(ReviewComplete review)
        {
            DisciplinaryInmate disciplinaryInmate = _context.DisciplinaryInmate
                .Find(review.DisciplinaryInmateId);
            disciplinaryInmate.DisciplinaryDaysSentFlag = 1;
            disciplinaryInmate.AppealDueDate = review.AppealDueDate;
            return await _context.SaveChangesAsync();
        }

        public async Task<int> DeleteApplyBookings(DeleteRequest deleteRequest)
        {
            DisciplinarySentDayXref disciplinarySentDayXref = _context.DisciplinarySentDayXref
                .Find(deleteRequest.DispSentDayXrefId);
            disciplinarySentDayXref.DeleteFlag = 1;
            disciplinarySentDayXref.DeleteDate = DateTime.Now;
            disciplinarySentDayXref.DeleteBy = _personnelId;
            disciplinarySentDayXref.CrimeId = deleteRequest.CrimeId > 0 ? deleteRequest.CrimeId : (int?)null;
            Arrest arrest = _context.Arrest.Find(deleteRequest.ArrestId);
            arrest.ArrestSentenceDisciplinaryDaysFlag = 0;
            return await _context.SaveChangesAsync();
        }

        public List<IncidentNarrativeDetailVm> GetHearingQueueNarratives(int facilityId, int queueType)
        {
            List<LookupVm> lookups =
                _commonService.GetLookups(new[] { LookupConstants.DISCTYPE,LookupConstants.DISCINTYPE,
                    LookupConstants.INCCAT });
            IQueryable<DisciplinaryInmate> disciplinaryInmates = _context.DisciplinaryInmate
                .Where(a => a.DisciplinaryIncident.DisciplinaryActive == 1 &&
                (!a.DeleteFlag.HasValue || a.DeleteFlag == 0) &&
                a.DisciplinaryIncident.FacilityId == facilityId &&
                a.DisciplinaryIncident.InitialEntryFlag == true &&
                a.DisciplinaryIncident.AllowHearingFlag == 1 && 
                (!a.DisciplinaryInmateBypassHearing.HasValue ||
                        a.DisciplinaryInmateBypassHearing == 0));
            switch (queueType)
            {
                case 1:
                    disciplinaryInmates = disciplinaryInmates
                        .Where(a => !a.NoticeFlag);
                    break;
                case 2:
                    disciplinaryInmates = disciplinaryInmates
                        .Where(a => a.NoticeFlag && (!a.HearingComplete.HasValue ||
                        a.HearingComplete == 0));
                    break;
                case 3:
                    disciplinaryInmates = disciplinaryInmates
                        .Where(a => a.NoticeFlag && a.HearingComplete == 1 && a.DisciplinaryIncident
                        .DisciplinaryIncidentNarrative.Any(w => (w.SupervisorReviewFlag == 0 ||
                        !w.SupervisorReviewFlag.HasValue) && (!w.DeleteFlag.HasValue || w.DeleteFlag == 0)));
                    break;
                default:
                    disciplinaryInmates = disciplinaryInmates
                        .Where(a => a.NoticeFlag && a.HearingComplete == 1 && a.DisciplinaryIncident
                        .DisciplinaryIncidentNarrative.All(w => w.SupervisorReviewFlag == 1 &&
                        (!w.DeleteFlag.HasValue || w.DeleteFlag == 0)));
                    break;
            }
            List<IncidentNarrativeDetailVm> incidentNarratives = disciplinaryInmates
                .Select(a => new IncidentNarrativeDetailVm
                {
                    DisciplinaryInmateId = a.DisciplinaryInmateId,
                    IncidentNumber = a.DisciplinaryIncident.DisciplinaryNumber,
                    IncDate = a.DisciplinaryIncident.DisciplinaryIncidentDate,
                    DisciplinaryIncidentId = a.DisciplinaryIncidentId,
                    DisciplinaryType = a.DisciplinaryIncident.DisciplinaryType ?? 0,
                    Type = lookups.Where(l => l.LookupType == LookupConstants.DISCTYPE
                                              && l.LookupIndex == a.DisciplinaryIncident.DisciplinaryType)
                        .Select(l => l.LookupDescription).FirstOrDefault(),
                    Categorization = lookups.Where(l => l.LookupType == LookupConstants.INCCAT
                        && l.LookupIndex == a.DisciplinaryIncident.IncidentCategorizationIndex)
                        .Select(s => s.LookupDescription).FirstOrDefault(),
                    InvPartyType = lookups.Where(l => l.LookupType == LookupConstants.DISCINTYPE
                      && l.LookupIndex == a.DisciplinaryInmateType).Select(s => s.LookupDescription)
                    .FirstOrDefault(),
                    DispInmateByPassHearing = a.DisciplinaryInmateBypassHearing > 0,
                    NoticeFlag = a.NoticeFlag,
                    HearingComplete = a.HearingComplete,
                    Elapsed = CalculateElaps(a.NoticeDate),
                    CreateByPersonnel = a.InmateId > 0 ? new PersonnelVm
                    {
                        PersonLastName = a.Inmate.Person.PersonLastName,
                        PersonFirstName = a.Inmate.Person.PersonFirstName,
                        PersonnelNumber = a.Inmate.InmateNumber
                    } : a.PersonnelId > 0 ? new PersonnelVm
                    {
                        PersonLastName = a.Personnel.PersonNavigation.PersonLastName,
                        PersonFirstName = a.Personnel.PersonNavigation.PersonFirstName,
                        PersonnelNumber = a.Personnel.PersonnelNumber
                    } : new PersonnelVm
                    {
                        PersonLastName = a.DisciplinaryOtherName
                    },
                    PersonnelId = a.PersonnelId,
                    DisciplinaryOtherName = a.DisciplinaryOtherName
                }).ToList();
            List<AoWizardProgressVm> incidentWizardProgresses =
                GetIncidentWizardProgress(incidentNarratives.Select(a => a.DisciplinaryInmateId).ToArray());
            incidentNarratives.ForEach(a => a.ActiveIncidentProgress =
                    incidentWizardProgresses.FirstOrDefault(x => x.DisciplinaryInmateId == a.DisciplinaryInmateId));
            return incidentNarratives;
        }

        public List<IncidentNarrativeDetailVm> SupervisorAppealQueue(int facilityId)
        {
            List<LookupVm> lookups =
                _commonService.GetLookups(new[] { LookupConstants.DISCTYPE,LookupConstants.DISCINTYPE,
                    LookupConstants.INCCAT });
            List<IncidentNarrativeDetailVm> incidentNarratives = _context.DisciplinaryInmateAppeal
                .Where(a => a.DisciplinaryInmate.DisciplinaryIncident.FacilityId == facilityId
                && a.ReviewComplete == 0 && a.SendForReview == 0)
                .Select(a => new IncidentNarrativeDetailVm
                {
                    DisciplinaryInmateId = a.DisciplinaryInmateId,
                    IncidentNumber = a.DisciplinaryInmate.DisciplinaryIncident.DisciplinaryNumber,
                    IncDate = a.DisciplinaryInmate.DisciplinaryIncident.DisciplinaryIncidentDate,
                    DisciplinaryIncidentId = a.DisciplinaryInmate.DisciplinaryIncidentId,
                    DisciplinaryType = a.DisciplinaryInmate.DisciplinaryIncident.DisciplinaryType ?? 0,
                    Type = lookups.Where(l => l.LookupType == LookupConstants.DISCTYPE
                                              && l.LookupIndex == a.DisciplinaryInmate.DisciplinaryIncident.DisciplinaryType)
                        .Select(l => l.LookupDescription).FirstOrDefault(),
                    Categorization = lookups.Where(l => l.LookupType == LookupConstants.INCCAT
                        && l.LookupIndex == a.DisciplinaryInmate.DisciplinaryIncident.IncidentCategorizationIndex)
                        .Select(s => s.LookupDescription).FirstOrDefault(),
                    InvPartyType = lookups.Where(l => l.LookupType == LookupConstants.DISCINTYPE
                      && l.LookupIndex == a.DisciplinaryInmate.DisciplinaryInmateType).Select(s => s.LookupDescription)
                    .FirstOrDefault(),
                    DispInmateByPassHearing = a.DisciplinaryInmate.DisciplinaryInmateBypassHearing > 0,
                    NoticeFlag = a.DisciplinaryInmate.NoticeFlag,
                    HearingComplete = a.DisciplinaryInmate.HearingComplete,
                    Elapsed = CalculateElaps(a.DisciplinaryInmate.NoticeDate),
                    CreateByPersonnel = a.DisciplinaryInmate.InmateId > 0 ? new PersonnelVm
                    {
                        PersonLastName = a.DisciplinaryInmate.Inmate.Person.PersonLastName,
                        PersonFirstName = a.DisciplinaryInmate.Inmate.Person.PersonFirstName,
                        PersonnelNumber = a.DisciplinaryInmate.Inmate.InmateNumber
                    } : a.DisciplinaryInmate.PersonnelId > 0 ? new PersonnelVm
                    {
                        PersonLastName = a.DisciplinaryInmate.Personnel.PersonNavigation.PersonLastName,
                        PersonFirstName = a.DisciplinaryInmate.Personnel.PersonNavigation.PersonFirstName,
                        PersonnelNumber = a.DisciplinaryInmate.Personnel.PersonnelNumber
                    } : new PersonnelVm
                    {
                        PersonLastName = a.DisciplinaryInmate.DisciplinaryOtherName
                    },
                    PersonnelId = a.DisciplinaryInmate.PersonnelId,
                    DisciplinaryOtherName = a.DisciplinaryInmate.DisciplinaryOtherName,
                    SensitiveMaterial = a.DisciplinaryInmate.DisciplinaryIncident.SensitiveMaterial,
                    PreaOnly = a.DisciplinaryInmate.DisciplinaryIncident.PreaOnly
                }).ToList();
            List<AoWizardProgressVm> incidentWizardProgresses =
                GetIncidentWizardProgress(incidentNarratives.Select(a => a.DisciplinaryInmateId).ToArray());
            incidentNarratives.ForEach(a => a.ActiveIncidentProgress =
                    incidentWizardProgresses.FirstOrDefault(x => x.DisciplinaryInmateId == a.DisciplinaryInmateId));
            return incidentNarratives;
        }

        public bool IsInvolvedPartyExists(int incidentId, int inmateId, int personnelId)
        {
            List<DisciplinaryInmate> disciplinaryInmates = _context.DisciplinaryInmate
                .Where(i => i.DisciplinaryIncidentId == incidentId).ToList();
            return disciplinaryInmates.Any(i => (!i.DeleteFlag.HasValue || i.DeleteFlag == 0) &&
            (inmateId == 0 || i.InmateId == inmateId) &&
            (personnelId == 0 || i.PersonnelId == personnelId));
        }

        public List<DisciplinaryWTStopVm> GetDisciplinaryWTStops(int dispInmateId, int incarcerationId)
        {
            List<DisciplinaryWTStopVm> disciplinaryWTStops = _context.DisciplinaryWTStop
                .Where(w => w.DisciplinaryInmateId == dispInmateId
                && w.IncarcerationId == incarcerationId && (!w.DeleteFlag.HasValue || w.DeleteFlag == 0))
                .Select(s => new DisciplinaryWTStopVm
                {
                    DisciplinaryWTStopId = s.DisciplinaryWTStopId,
                    DisciplinaryInmateId = s.DisciplinaryInmateId,
                    IncarcerationId = s.IncarcerationId,
                    CalculateFlag = s.CalculateFlag,
                    StartDate = s.StartDate,
                    StopDate = s.StopDate,
                    StopNote = s.StopNote,
                    DeleteFlag = s.DeleteFlag
                }).OrderByDescending(o => o.DisciplinaryWTStopId).ToList();
            return disciplinaryWTStops;
        }

        public IncarcerationDetail GetActiveBooking(int inmateId)
        {
            IncarcerationDetail activeBooking = _context.Incarceration
                .Where(w => w.InmateId == inmateId && !w.ReleaseOut.HasValue)
               .Select(s => new IncarcerationDetail
               {
                   IncarcerationId = s.IncarcerationId,
                   BookingNo = s.BookingNo,
                   DateIn = s.DateIn
               })
              .FirstOrDefault();
            return activeBooking;
        }

        public List<WtStoppageHistory> GetWtStoppageHistories(int dispInmateId, int incarcerationId)
        {
            List<WtStoppageHistory> wtStoppageHistories = _context.DisciplinaryWTStop
                .Where(w => w.DisciplinaryInmateId == dispInmateId && w.IncarcerationId == incarcerationId)
                .Select(s => new WtStoppageHistory
                {
                    DisciplinaryWTStopId = s.DisciplinaryWTStopId,
                    DisciplinaryNumber = s.DisciplinaryInmate.DisciplinaryIncident.DisciplinaryNumber,
                    StopDate = s.StopDate,
                    StartDate = s.StartDate,
                    StopNote = s.StopNote,
                    DeleteFlag = s.DeleteFlag
                }).OrderByDescending(o => o.DisciplinaryWTStopId).ToList();
            return wtStoppageHistories;
        }

        public KeyValuePair<int?, int?> GetGtAndWtFieldVisibility()
        {
            List<ArrestSentenceSetting> arrestSentenceSettings = _context.ArrestSentenceSetting
                .Where(w => w.FieldName == "arrest_sentence_wt_days" ||
                w.FieldName == "arrest_sentence_gt_days").ToList();
            return new KeyValuePair<int?, int?>(arrestSentenceSettings
                .SingleOrDefault(s=>s.FieldName== "arrest_sentence_wt_days")?.InvisibleFlag,
                arrestSentenceSettings
                .SingleOrDefault(s => s.FieldName == "arrest_sentence_gt_days")?.InvisibleFlag);
        }

        public List<DisciplinaryWTStopVm> GetWTStoppageDetails(int inmateId)
        {
            List<DisciplinaryWTStopVm> disciplinaryWTStops = _context.DisciplinaryWTStop
                .Where(w => inmateId <= 0 || w.DisciplinaryInmate.InmateId == inmateId)
                .OrderByDescending(o => o.DisciplinaryWTStopId)
                .Select(s => new DisciplinaryWTStopVm
                {
                    DisciplinaryWTStopId = s.DisciplinaryWTStopId,
                    DisciplinaryInmateId = s.DisciplinaryInmateId,
                    InmateId = s.DisciplinaryInmate.InmateId ?? 0,
                    IncarcerationId = s.IncarcerationId,
                    CalculateFlag = s.CalculateFlag,
                    StopNote = s.StopNote,
                    StopDate = s.StopDate,
                    StartDate = s.StartDate,
                    DeleteFlag = s.DeleteFlag,
                    PersonLastName = s.DisciplinaryInmate.Inmate.Person.PersonLastName,
                    PersonFirstName = s.DisciplinaryInmate.Inmate.Person.PersonFirstName,
                    InmateNumber = s.DisciplinaryInmate.Inmate.InmateNumber,
                    BookingNumber = s.Incarceration.BookingNo,
                    DisciplinaryNumber = s.DisciplinaryInmate.DisciplinaryIncident.DisciplinaryNumber,
                    ReCalculateCount = s.Incarceration.IncarcerationArrestXref
                    .Count(x => x.Arrest.ArrestSentenceStopDaysFlag == true &&
                    (x.Arrest.ArrestSentenceCode == 1 || x.Arrest.ArrestSentenceCode == 4))
                }).ToList();
            return disciplinaryWTStops;
        }

        public async Task<int> UpdateWTStoppage(DisciplinaryWTStopVm disciplinaryWTStop)
        {
            DisciplinaryWTStop disciplinaryWT = _context.DisciplinaryWTStop
                .Single(s => s.DisciplinaryWTStopId == disciplinaryWTStop.DisciplinaryWTStopId);
            disciplinaryWT.StopDate = disciplinaryWTStop.StopDate;
            disciplinaryWT.StartDate = disciplinaryWTStop.StartDate;
            disciplinaryWT.StopNote = disciplinaryWTStop.StopNote;
            disciplinaryWT.CalculateFlag = true;
            disciplinaryWT.UpdateDate = DateTime.Now;
            disciplinaryWT.UpdateBy = _personnelId;
            int[] arrestIds = _context.IncarcerationArrestXref
                        .Where(a => a.IncarcerationId == disciplinaryWTStop.IncarcerationId &&
                        a.ArrestId > 0).Select(s => s.ArrestId.Value).ToArray();
            _context.Arrest.Where(w => arrestIds.Contains(w.ArrestId) && (w.ArrestSentenceCode == 1 ||
            w.ArrestSentenceCode == 4)).ToList().ForEach(f =>
            {
                f.ArrestSentenceStopDaysFlag = true;
            });
            return await _context.SaveChangesAsync();
        }

        public async Task<int> DeleteWTStoppage(int wtStopId)
        {
            DisciplinaryWTStop disciplinaryWTStop = _context.DisciplinaryWTStop
                .Single(s => s.DisciplinaryWTStopId == wtStopId);
            disciplinaryWTStop.DeleteFlag = 1;
            disciplinaryWTStop.DeleteDate = DateTime.Now;
            disciplinaryWTStop.DeleteBy = _personnelId;
            disciplinaryWTStop.CalculateFlag = true;
            int[] arrestIds = _context.IncarcerationArrestXref
                        .Where(a => a.IncarcerationId == disciplinaryWTStop.IncarcerationId &&
                        a.ArrestId > 0).Select(s => s.ArrestId.Value).ToArray();
            _context.Arrest.Where(w => arrestIds.Contains(w.ArrestId) && (w.ArrestSentenceCode == 1 ||
            w.ArrestSentenceCode == 4)).ToList().ForEach(f =>
            {
                f.ArrestSentenceStopDaysFlag = true;
            });
            return await _context.SaveChangesAsync();
        }
    }
}