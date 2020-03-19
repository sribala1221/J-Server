﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using GenerateTables.Models;
using Microsoft.AspNetCore.Http;
using ServerAPI.Utilities;
using ServerAPI.ViewModels;

namespace ServerAPI.Services
{
    public class BookingReleaseService : IBookingReleaseService
    {
        private readonly AAtims _context;
        private readonly ICommonService _commonService;
        private IQueryable<Incarceration> _lstIncarcerations;
        private bool _altSentFlag;
        private readonly int _personnelId;
        private readonly IPersonService _iPersonService;
        private readonly IInmateService _inmateService;
        private readonly IRecordsCheckService _recordsCheckService;
        private readonly IFacilityHousingService _facilityHousingService;
        private readonly IInterfaceEngineService _interfaceEngine;

        public BookingReleaseService(AAtims context, ICommonService commonService,
            IHttpContextAccessor httpContextAccessor, IPersonService personService,
            IInmateService inmateService, IRecordsCheckService recordsCheckService,
            IFacilityHousingService facilityHousingService,
            IInterfaceEngineService interfaceEngine)
        {
            _context = context;
            _commonService = commonService;
            _personnelId = Convert.ToInt32(httpContextAccessor.HttpContext.User.FindFirst("personnelId")?.Value);
            _iPersonService = personService;
            _inmateService = inmateService;
            _recordsCheckService = recordsCheckService;
            _facilityHousingService = facilityHousingService;
            _interfaceEngine = interfaceEngine;
        }

        #region Left Side Grid

        public BookingReleaseVm GetBookingRelease(BookingInputsVm iRelease)
        {
            _lstIncarcerations = _context.Incarceration.Where(inc =>
                inc.Inmate.InmateActive == 1 &&
                !inc.ReleaseOut.HasValue &&
                inc.Inmate.FacilityId == iRelease.FacilityId);

            _altSentFlag = _context.Facility.Where(fac =>
                    fac.FacilityId == iRelease.FacilityId && fac.AltSentFlag.HasValue && fac.AltSentFlag == 1)
                .Select(fac => fac.AltSentFlag == 1).SingleOrDefault();

            int[] lstIncId;
            if (_altSentFlag)
            {
                lstIncId = _context.AltSent.Select(alt => alt.IncarcerationId.Value).ToArray();
                _lstIncarcerations = _lstIncarcerations.Where(inc => lstIncId.Contains(inc.IncarcerationId));
            }

            BookingReleaseVm brcVm = new BookingReleaseVm
            {
                ReleaseCount = new List<KeyValuePair<ReleaseTypeEnum, int>>(),
                TransportCount = new List<KeyValuePair<string, int>>()
            };

            List<Incarceration> lstIncarcerations = GetOverallReleaseProgress(iRelease, false).ToList();

            brcVm.ReleaseCount.Add(new KeyValuePair<ReleaseTypeEnum, int>(ReleaseTypeEnum.OverallReleaseProgress,
                lstIncarcerations.Count));
            brcVm.ReleaseCount.Add(new KeyValuePair<ReleaseTypeEnum, int>(ReleaseTypeEnum.OverallReleaseProgressTransport,
                GetOverallReleaseProgress(iRelease, true).Count()));

            int[] incIds = _lstIncarcerations.Select(inc => inc.IncarcerationId).ToArray();
            int sentenceClearCount = _context.IncarcerationArrestXref.Count(incXref => incIds.Contains(incXref.IncarcerationId ?? 0)
            && !incXref.ReleaseDate.HasValue && incXref.ArrestId.HasValue &&
                incXref.Arrest.ArrestSentenceCode.HasValue && incXref.Arrest.ArrestSentenceCode.Value != 4 &&
                (iRelease.ActiveOnly || incXref.Arrest.ArrestSentenceReleaseDate.HasValue &&
                    incXref.Arrest.ArrestSentenceReleaseDate.Value.Date <= DateTime.Now.Date ||
                    incXref.Incarceration.ReleaseClearFlag == 1 || incXref.Incarceration.BookAndReleaseFlag == 1));


            brcVm.ReleaseCount.Add(new KeyValuePair<ReleaseTypeEnum, int>(ReleaseTypeEnum.SentenceClear,
                sentenceClearCount));
            brcVm.ReleaseCount.Add(new KeyValuePair<ReleaseTypeEnum, int>(ReleaseTypeEnum.HoldClear,
                GetHoldClearOrInfinity(iRelease, false).Count()));
            brcVm.ReleaseCount.Add(new KeyValuePair<ReleaseTypeEnum, int>(ReleaseTypeEnum.HoldIndefinite,
                GetHoldClearOrInfinity(iRelease, true).Count()));

            string weekenderSiteOption = _context.SiteOptions.SingleOrDefault(so => so.SiteOptionsStatus == "1" &&
                so.SiteOptionsName == SiteOptionsConstants.ALLOWWEEKENDERSENTENCE &&
                so.SiteOptionsVariable == SiteOptionsConstants.ALLOWWEEKENDERSENTENCE)?.SiteOptionsValue;

            if (weekenderSiteOption == SiteOptionsConstants.ON)
            {
                brcVm.ReleaseCount.Add(new KeyValuePair<ReleaseTypeEnum, int>(ReleaseTypeEnum.Weekender, GetWeekender().Count()));
            }

            //42057 - Records Check submodule
            bool bRecordCheck = _context.AppAoSubModule.SingleOrDefault(app =>
                                    app.AppAoSubModuleId == 42057)?.AppAoSubModuleVisible == 1;
            if (bRecordCheck)
            {
                brcVm.ReleaseCount.Add(new KeyValuePair<ReleaseTypeEnum, int>(ReleaseTypeEnum.RecordsCheckResponses,
                    _recordsCheckService.GetRecordsCheckResponseCount(iRelease.FacilityId).Count()));
            }

            brcVm.TransportCount.Add(new KeyValuePair<string, int>(ReleaseTypeEnum.AllTransports.ToString(),
                _lstIncarcerations.Count(inc => inc.TransportFlag == 1)));

            brcVm.TransportCount.Add(new KeyValuePair<string, int>(ReleaseTypeEnum.NoTransportDate.ToString(),
                _lstIncarcerations.Count(inc =>
                    inc.TransportFlag == 1 && !inc.TransportScheduleDate.HasValue)));

            Dictionary<string, int> lstAdd = _lstIncarcerations.Where(
                    inc => inc.TransportFlag == 1 && inc.TransportScheduleDate.HasValue)
                .Select(inc => inc.TransportScheduleDate.Value).AsEnumerable()
                .GroupBy(inc => inc.Date).ToList()
                .ToDictionary(inx => inx.Key.Date.ToString(CultureInfo.InvariantCulture), inx => inx.Count());

            foreach (KeyValuePair<string, int> keyValuePair in lstAdd)
            {
                brcVm.TransportCount.Add(new KeyValuePair<string, int>(keyValuePair.Key, keyValuePair.Value));
            }

            brcVm.TasksCount = GetTasksDetails(iRelease.FacilityId,
                lstIncarcerations.Select(inc => inc.InmateId ?? 0).ToArray());

            return brcVm;
        }

        private List<TasksCountVm> GetTasksDetails(int facilityId, int[] inmateIds)
        {
            List<KeyValuePair<bool, int>> lstAoTaskQueue = _context.AoTaskQueue
                .Where(queue =>
                    !queue.CompleteFlag && !queue.AoTaskLookup.DeleteFlag && inmateIds.Contains(queue.InmateId))
                .Select(queue =>
                    new KeyValuePair<bool, int>(queue.PriorityFlag, queue.AoTaskLookupId)).ToList();

            List<TasksCountVm> tasksCount = _context.AoTaskLookupAssign.Where(lookAssign =>
                    lookAssign.TaskValidateLookup == "DO RELEASE"
                    && !lookAssign.DeleteFlag && lookAssign.FacilityId == facilityId &&
                    !lookAssign.AoTaskLookup.DeleteFlag)
                .Select(
                    look => new TasksCountVm
                    {
                        TaskLookupId = look.AoTaskLookupId,
                        TaskName = look.AoTaskLookup.TaskName,
                        InmateCount = lstAoTaskQueue.Count(queue => queue.Value == look.AoTaskLookupId),
                        TaskPriorityCount =
                            lstAoTaskQueue.Count(queue => queue.Value == look.AoTaskLookupId && queue.Key)
                    }).Distinct().ToList();

            return tasksCount;
        }

        #endregion

        #region Right Side Grid

        public BookingReleaseDetailsVm GetBookingReleaseDetails(BookingInputsVm iRelease)
        {
            _lstIncarcerations = _context.Incarceration.Where(inc =>
                inc.Inmate.InmateActive == 1 &&
                !inc.ReleaseOut.HasValue &&
                inc.Inmate.FacilityId == iRelease.FacilityId);

            _altSentFlag = _context.Facility.Where(fac => fac.FacilityId == iRelease.FacilityId &&
                                                          fac.AltSentFlag.HasValue && fac.AltSentFlag == 1)
                .Select(fac => fac.AltSentFlag == 1).SingleOrDefault();

            if (_altSentFlag)
            {
                int?[] lstIncId = _context.AltSent.Select(alt => alt.IncarcerationId).ToArray();

                _lstIncarcerations = _lstIncarcerations.Where(inc => lstIncId.Contains(inc.IncarcerationId));
            }

            BookingReleaseDetailsVm releaseDetail = new BookingReleaseDetailsVm();

            switch (iRelease.ReleaseType)
            {
                case ReleaseTypeEnum.OverallReleaseProgress:
                    releaseDetail.ReleaseDetails =
                        GetReleaseDetails(GetOverallReleaseProgress(iRelease, false), iRelease.FacilityId);
                    break;
                case ReleaseTypeEnum.OverallReleaseProgressTransport:
                    releaseDetail.ReleaseDetails =
                        GetReleaseDetails(GetOverallReleaseProgress(iRelease, true), iRelease.FacilityId);
                    break;
                case ReleaseTypeEnum.SentenceClear:
                    releaseDetail.ReleaseClearDetails = GetArrestXrefDetails(GetSentenceClear(iRelease));
                    break;
                case ReleaseTypeEnum.HoldClear:
                    releaseDetail.ReleaseClearDetails =
                        GetArrestXrefDetails(GetHoldClearOrInfinity(iRelease, false));
                    break;
                case ReleaseTypeEnum.HoldIndefinite:
                    releaseDetail.ReleaseClearDetails =
                        GetArrestXrefDetails(GetHoldClearOrInfinity(iRelease, true));
                    break;
                case ReleaseTypeEnum.RecordsCheckResponses:
                    releaseDetail.RecordsCheckResponse =
                        _recordsCheckService.GetRecordsCheckResponse(iRelease.FacilityId);
                    break;
                case ReleaseTypeEnum.AllTransports:
                    releaseDetail.ReleaseDetails =
                        GetReleaseDetails(
                            _lstIncarcerations.Where(inc => inc.TransportFlag == 1),
                            iRelease.FacilityId);
                    break;
                case ReleaseTypeEnum.NoTransportDate:
                    releaseDetail.ReleaseDetails = GetReleaseDetails(_lstIncarcerations.Where(inc =>
                            inc.TransportFlag == 1 && !inc.TransportScheduleDate.HasValue),
                        iRelease.FacilityId);
                    break;
                case ReleaseTypeEnum.Weekender:
                    releaseDetail.ReleaseClearDetails = GetArrestXrefDetails(GetWeekender());
                    break;
                default:
                    iRelease.TransportDate = iRelease.TransportDate.HasValue?iRelease.TransportDate:DateTime.Now;
                    releaseDetail.ReleaseDetails = GetReleaseDetails(_lstIncarcerations.Where(inc =>
                        inc.TransportFlag == 1 && inc.TransportScheduleDate.HasValue &&
                        inc.TransportScheduleDate.Value.Date == iRelease.TransportDate.Value.Date
                        && inc.AssessmentCompleteFlag), iRelease.FacilityId);
                    break;
            }

            return releaseDetail;
        }

        private List<InmateHousing> GetInmateDetails(List<int> inmateIds) =>
            _context.Inmate.Where(x => inmateIds.Contains(x.InmateId))
                .Select(a => new InmateHousing
                {
                    InmateId = a.InmateId,
                    PersonId = a.PersonId,
                    PersonFirstName = a.Person.PersonFirstName,
                    PersonMiddleName = a.Person.PersonMiddleName,
                    PersonLastName = a.Person.PersonLastName,
                    PersonSuffix = a.Person.PersonSuffix,
                    InmateNumber = a.InmateNumber,
                    FacilityId = a.FacilityId,
                    LastReviewDate = a.LastReviewDate,
                    InmateCurrentTrack = a.InmateCurrentTrack,
                    HousingUnitId = a.HousingUnitId,
                    HousingLocation = a.HousingUnit.HousingUnitLocation,
                    HousingNumber = a.HousingUnit.HousingUnitNumber,
                    HousingBedLocation = a.HousingUnit.HousingUnitBedLocation,
                    HousingBedNumber = a.HousingUnit.HousingUnitBedNumber
                }).ToList();

        private List<ReleaseVm> GetReleaseDetails(IQueryable<Incarceration> iIncarceration, int facilityId)
        {
            int[] inmateIds = iIncarceration.Select(i => i.InmateId.Value).ToArray();

            List<InmateHousing> lstPersonDetails =
                GetInmateDetails(inmateIds.ToList());

            List<Lookup> lstLookups = _context.Lookup.Where(look => (look.LookupType == LookupConstants.TRANSTYPE ||
                    look.LookupType == LookupConstants.TRANSROUTE)
                && look.LookupInactive == 0).ToList();

            List<AoWizardProgressIncarceration> lstAoWizardProgress = _context.AoWizardProgressIncarceration.Where(
                awp => iIncarceration.Select(inc => inc.IncarcerationId).Contains(awp.IncarcerationId)).ToList();

            List<ReleaseVm> lstRelease = iIncarceration.Select(lstInc => new ReleaseVm
            {
                IncarcerationId = lstInc.IncarcerationId,
                InmateId = lstInc.InmateId.Value,
                OverallFinalReleaseDate = lstInc.OverallFinalReleaseDate,
                ReleaseCompleteFlag = lstInc.ReleaseCompleteFlag == 1,
                TransportScheduleDate = lstInc.TransportScheduleDate,
                TransportHoldName = lstInc.TransportHoldName,
                BookAndReleaseFlag = lstInc.BookAndReleaseFlag == 1,
                TransportFlag = lstInc.TransportFlag == 1,
                PersonDetails = lstPersonDetails.Single(per => per.InmateId == lstInc.InmateId.Value),
                AgencyName = lstInc.ReleaseToOtherAgency.AgencyName,
                TransportInmateCaution = lstInc.TransportInmateCautions,
                TransportInstructions = lstInc.TransportInstructions,
                TransRoute = lstInc.TransportCoOpRoute.HasValue
                    ? lstLookups
                        .Where(l =>
                            l.LookupType == LookupConstants.TRANSROUTE &&
                            l.LookupIndex == lstInc.TransportCoOpRoute.Value)
                        .Select(l => l.LookupDescription)
                        .SingleOrDefault()
                    : null,
                TransType = lstInc.TransportHoldType.HasValue
                    ? lstLookups
                        .Where(l =>
                            l.LookupType == LookupConstants.TRANSTYPE &&
                            l.LookupIndex == lstInc.TransportHoldType.Value)
                        .Select(l => l.LookupDescription)
                        .SingleOrDefault()
                    : null,

                LastClearReason = lstInc.ReleaseClearFlag == 1
                    ? lstInc.IncarcerationArrestXref.OrderByDescending(a => a.ReleaseDate).First().ReleaseReason
                    : null,
                IncarcerationArrestXrefId = lstInc.IncarcerationArrestXref.OrderByDescending(a => a.ReleaseDate)
                    .FirstOrDefault().IncarcerationArrestXrefId,
                ExpediteBookingReason = lstInc.ExpediteBookingReason,
                ExpediteBookingFlag = lstInc.ExpediteBookingFlag,
                ExpediteBookingNote = lstInc.ExpediteBookingNote,
                ExpediteDate = lstInc.ExpediteBookingDate,
                ExpediteById = lstInc.ExpediteBookingBy
            }).ToList();

            int[] lstIncIds = lstRelease.Select(inc => inc.IncarcerationId).ToArray();

            var lstIncarcerationArrestXrefDs =
                _context.IncarcerationArrestXref.Where(iax => lstIncIds.Contains(iax.IncarcerationId.Value) &&
                                                              iax.ArrestId.HasValue).Select(iax => new
                {
                    IncarcerationId = iax.IncarcerationId.Value,
                    iax.Arrest.ArrestBookingNo,
                    iax.Arrest.ArrestType
                }).ToList();

            List<AoComponent> lstAoComponent = _context.AoComponent.ToList();


            int[] arrTaskIds = _context.AoTaskLookupAssign.Where(lookAssign =>
                    lookAssign.TaskValidateLookup == _commonService.GetValidationType(TaskValidateType.DoRelease) &&
                    !lookAssign.DeleteFlag &&
                    lookAssign.FacilityId == facilityId &&
                    !lookAssign.AoTaskLookup.DeleteFlag)
                .Select(task => task.AoTaskLookupId).ToArray();

            List<TasksCountVm> lstAoTaskQueue = _context.AoTaskQueue.Where(inm =>
                    inmateIds.Contains(inm.InmateId) && !inm.CompleteFlag && arrTaskIds.Contains(inm.AoTaskLookupId))
                .Select(que => new TasksCountVm
                {
                    InmateId = que.InmateId,
                    ComponentId = que.AoTaskLookup.AoComponentId,
                    TaskLookupId = que.AoTaskLookupId,
                    ComponentName = que.AoTaskLookup.AoComponentId.HasValue
                        ? lstAoComponent.Single(look => look.AoComponentId == que.AoTaskLookup.AoComponentId)
                            .ComponentName
                        : null,
                    TaskName = que.AoTaskLookup.TaskName,
                    TaskIconPath = que.AoTaskLookup.AoComponentId.HasValue
                        ? lstAoComponent.Single(look => look.AoComponentId == que.AoTaskLookup.AoComponentId).StepIcon
                        : null,
                    TaskInstruction = que.AoTaskLookup.TaskInstructions
                }).ToList();

            List<int> officerIds = lstRelease.Where(i => i.ExpediteById.HasValue)
                .Select(i => i.ExpediteById.Value).ToList();

            List<PersonnelVm> expediteOfficer = _iPersonService.GetPersonNameList(officerIds.ToList());

            lstRelease.ForEach(rel =>
            {
                Wizards wizardId = rel.BookAndReleaseFlag ? Wizards.bookAndRelease : Wizards.release;
                rel.WizardProgressId = lstAoWizardProgress.FirstOrDefault(awp =>
                    awp.IncarcerationId == rel.IncarcerationId && awp.AoWizardId == (int)wizardId)?.AoWizardProgressId;

                if (rel.WizardProgressId.HasValue)
                {
                    rel.LastStep = _context.AoWizardStepProgress
                        .Where(aws => aws.AoWizardProgressId == rel.WizardProgressId.Value).Select(aw => new WizardStep
                        {
                            ComponentId = aw.AoComponentId,
                            WizardProgressId = aw.AoWizardProgressId,
                            AoWizardFacilityStepId = aw.AoWizardFacilityStepId,
                            WizardStepProgressId = aw.AoWizardStepProgressId,
                            StepComplete = aw.StepComplete
                        }).ToList();
                }

                if (rel.ExpediteById.HasValue)
                {
                    rel.ExpediteBy = expediteOfficer.Single(ao => ao.PersonnelId == rel.ExpediteById.Value);
                }

                rel.BookingTasks = lstAoTaskQueue.Where(qu => qu.InmateId == rel.InmateId).ToList();

                rel.CaseType = lstIncarcerationArrestXrefDs.Where(
                    iax => rel.IncarcerationId == iax.IncarcerationId).Select(iax => iax.ArrestType).ToArray();
            });

            return lstRelease;
        }

        private List<SentenceClearVm> GetArrestXrefDetails(
            IQueryable<IncarcerationArrestXref> lstIncarcerationArrestXref)
        {
            List<Lookup> lstLookUp = _context.Lookup
                .Where(look =>
                    (look.LookupType == LookupConstants.ARRTYPE || look.LookupType == LookupConstants.BOOKSTAT) &&
                    look.LookupInactive == 0)
                .ToList();

            List<int> clearedOffIds =
                lstIncarcerationArrestXref.Where(i => i.Arrest.ArrestReleaseClearedBy.HasValue)
                    .Select(i => i.Arrest.ArrestReleaseClearedBy.Value).ToList();

            List<PersonnelVm> clearedOfficer =
                _iPersonService.GetPersonNameList(clearedOffIds.ToList());

            List<SentenceClearVm> lstSentenceClear = lstIncarcerationArrestXref.Select(i =>
                new SentenceClearVm
                {
                    InmateId = i.Arrest.InmateId.Value,
                    IncarcerationId = i.IncarcerationId.Value,
                    IncarcerationArrestXrefId = i.IncarcerationArrestXrefId,
                    ReleaseReason = i.ReleaseReason,
                    ReleaseNotes = i.ReleaseNotes,
                    ArrestId = i.ArrestId.Value,
                    ArrestCourtJurisdictionId = i.Arrest.ArrestCourtJurisdictionId,
                    OverallFinalReleaseDate = i.Incarceration.OverallFinalReleaseDate
                }).ToList();

            List<KeyValuePair<int, string>> lstAgency = _context.Agency.Where(age =>
                    lstSentenceClear.Where(lax => lax.ArrestCourtJurisdictionId.HasValue)
                        .Select(lax => lax.ArrestCourtJurisdictionId.Value).Contains(age.AgencyId)).Select(age =>
                    new KeyValuePair<int, string>(age.AgencyId, age.AgencyAbbreviation))
                .ToList();

            int[] arrestId = lstSentenceClear.Where(iax => iax.ArrestId.HasValue).Select(iax => iax.ArrestId.Value)
                .ToArray();

            List<Arrest> lstArrest = _context.Arrest.Where(arr => arrestId.Contains(arr.ArrestId)).ToList();

            List<InmateHousing> lstPersonDetails =
                GetInmateDetails(lstSentenceClear.Select(rel => rel.InmateId).ToList());

            lstSentenceClear.ForEach(lsc =>
            {
                Arrest arr = lstArrest.Single(larr => larr.ArrestId == lsc.ArrestId);
                lsc.ReleaseDate = arr.ArrestSentenceReleaseDate;
                lsc.CourtDocket = arr.ArrestCourtDocket;
                lsc.ArrestActive = arr.ArrestActive == 1;
                lsc.BookingNo = arr.ArrestBookingNo;
                lsc.ClearDate = arr.ArrestReleaseClearedDate;
                lsc.ArrestReleaseClearedById = arr.ArrestReleaseClearedBy;
                lsc.ArrestSentenceDescription = arr.ArrestSentenceDescription;
                lsc.ArrestSentenceCode = arr.ArrestSentenceCode;
                lsc.BailNoBailFlag = arr.BailNoBailFlag;
                lsc.BailAmount = arr.BailAmount;
                lsc.ArrestBookingStatus = arr.ArrestBookingStatus;
                lsc.ArrestType = Convert.ToInt32(arr.ArrestType);

                lsc.CourtName = lsc.ArrestCourtJurisdictionId.HasValue
                    ? lstAgency.SingleOrDefault(age => age.Key == lsc.ArrestCourtJurisdictionId.Value).Value
                    : null;

                lsc.ArrestReleaseClearedBy = lsc.ArrestReleaseClearedById.HasValue
                    ? clearedOfficer.Single(per => per.PersonnelId == lsc.ArrestReleaseClearedById)
                    : null;

                lsc.BookingType = lstLookUp.SingleOrDefault(look =>
                    look.LookupType == LookupConstants.ARRTYPE && look.LookupIndex == lsc.ArrestType)?.LookupDescription;

                lsc.PersonDetails = lstPersonDetails.Single(inm => inm.InmateId == lsc.InmateId);

                if (lsc.ArrestBookingStatus.HasValue)
                {
                    lsc.Abbr = lsc.ArrestBookingStatus != null ? lstLookUp.SingleOrDefault(look =>
                        look.LookupType == LookupConstants.BOOKSTAT &&
                        look.LookupIndex == lsc.ArrestBookingStatus)?.LookupDescription
                        : null;
                }
            });

            return lstSentenceClear;
        }

        #endregion

        #region Common IQueryables

        private IQueryable<Incarceration> GetOverallReleaseProgress(BookingInputsVm iRelease, bool transportFlag)
        {
            IQueryable<Incarceration> lstIncarceration = _altSentFlag
                ? _lstIncarcerations.Where(inc => transportFlag
                    ? inc.TransportFlag == 1
                    : (!inc.TransportFlag.HasValue || inc.TransportFlag == 0) &&
                      (iRelease.ActiveOnly || inc.OverallFinalReleaseDate.HasValue &&
                       inc.OverallFinalReleaseDate.Value.Date <= DateTime.Now.Date || inc.ReleaseClearFlag == 1))
                : _lstIncarcerations.Where(inc => transportFlag
                    ? inc.TransportFlag == 1 && inc.AssessmentCompleteFlag
                    : (!inc.TransportFlag.HasValue || inc.TransportFlag == 0) &&
                      (iRelease.ActiveOnly || inc.OverallFinalReleaseDate.HasValue &&
                       inc.OverallFinalReleaseDate.Value.Date <= DateTime.Now.Date || inc.ReleaseClearFlag == 1 ||
                       inc.BookAndReleaseFlag == 1 || inc.NoKeeper)
                      && (inc.AssessmentCompleteFlag || inc.BookAndReleaseFlag == 1));

            return lstIncarceration;
        }

        private IQueryable<IncarcerationArrestXref> GetSentenceClear(BookingInputsVm iRelease) =>
            _lstIncarcerations.SelectMany(inc => inc.IncarcerationArrestXref.Where(iax =>
                iax.IncarcerationId == inc.IncarcerationId && !iax.ReleaseDate.HasValue && iax.ArrestId.HasValue &&
                iax.Arrest.ArrestSentenceCode.HasValue && iax.Arrest.ArrestSentenceCode.Value != 4 &&
                (iRelease.ActiveOnly || iax.Arrest.ArrestSentenceReleaseDate.HasValue &&
                    iax.Arrest.ArrestSentenceReleaseDate.Value.Date <= DateTime.Now.Date ||
                    inc.ReleaseClearFlag == 1 || inc.BookAndReleaseFlag == 1)));

        private IQueryable<IncarcerationArrestXref> GetHoldClearOrInfinity(BookingInputsVm iRelease, bool infinityFlag) =>
            _lstIncarcerations.SelectMany(inc => inc.IncarcerationArrestXref.Where(iax =>
                iax.IncarcerationId == inc.IncarcerationId && iax.ArrestId.HasValue &&
                iax.Arrest.ArrestSentenceCode == 4 && !iax.Arrest.ArrestReleaseClearedDate.HasValue &&
                (infinityFlag ? iax.Arrest.ArrestSentenceIndefiniteHold.HasValue &&
                    iax.Arrest.ArrestSentenceIndefiniteHold == 1
                    : !iax.Arrest.ArrestSentenceIndefiniteHold.HasValue || iax.Arrest.ArrestSentenceIndefiniteHold == 0) &&
                (infinityFlag || iRelease.ActiveOnly || iax.Arrest.ArrestSentenceReleaseDate.HasValue &&
                   iax.Arrest.ArrestSentenceReleaseDate.Value.Date <= DateTime.Now.Date ||
                   inc.ReleaseClearFlag == 1 || inc.BookAndReleaseFlag == 1)));

        private IQueryable<IncarcerationArrestXref> GetWeekender()
        {
            IQueryable<IncarcerationArrestXref> incArrXref = _lstIncarcerations.SelectMany(inc =>
                inc.IncarcerationArrestXref.Where(iax =>
                    iax.IncarcerationId == inc.IncarcerationId && iax.ArrestId.HasValue &&
                    !iax.ReleaseDate.HasValue && iax.Arrest.ArrestSentenceWeekender == 1));

            if (_altSentFlag)
            {
                incArrXref = incArrXref.Where(inc => inc.Incarceration.AltSent != null);
            }

            return incArrXref;
        }

        #endregion

        public BookingOverviewDetails GetInmateReleaseValidation(int inmateId) =>
            _context.Incarceration
                .Where(inc => inc.InmateId == inmateId &&
                              !inc.ReleaseOut.HasValue).Select(inc => new BookingOverviewDetails
                              {
                                  IntakeCompleteFlag = inc.IntakeCompleteFlag == 1,
                                  BookCompleteFlag = inc.BookCompleteFlag == 1,
                                  AssessmentCompleteFlag = inc.AssessmentCompleteFlag,
                                  NoKeeper = inc.NoKeeper
                              }).Single();

        //Get inmate release details.
        public InmateReleaseVm GetInmateRelease(int inmateId, int incarcerationId, int personId)
        {
            IQueryable<Lookup> lstLookup = _context.Lookup.Where(w => (w.LookupType == LookupConstants.PERSONCAUTION ||
                                                                       w.LookupType == LookupConstants.TRANSCAUTION ||
                                                                       w.LookupType == LookupConstants.DIET ||
                                                                       w.LookupType == LookupConstants.MEDFLAG ||
                                                                       w.LookupType == LookupConstants.ALTSENTFLAG ||
                                                                       w.LookupType == LookupConstants.TRANSROUTE) &&
                                                                      w.LookupAlertRemoveRelease == 1
                                                                      && w.LookupInactive == 0);

            Incarceration qIncarceration = _context.Incarceration.Single(inc => inc.IncarcerationId == incarcerationId);

            InmateReleaseVm inmRelease = new InmateReleaseVm
            {
                Privilege = _context.InmatePrivilegeXref.Where(ipx =>
                    ipx.InmateId == inmateId && ipx.PrivilegeDate < DateTime.Now &&
                    (!ipx.PrivilegeExpires.HasValue || ipx.PrivilegeExpires >= DateTime.Now)).Select(ipx =>
                    new KeyValuePair<string, string>
                    (
                        ipx.Privilege.PrivilegeDescription,
                        ipx.Privilege.PrivilegeType
                    )).ToList(),
                IncarcerationId = qIncarceration.IncarcerationId,
                ReleaseClearFlag = qIncarceration.ReleaseClearFlag == 1,
                OverallConditionOfRelease = qIncarceration.OverallConditionOfRelease,
                OverallFinalReleaseDate = qIncarceration.OverallFinalReleaseDate,
                TransportFlag = qIncarceration.TransportFlag == 1,
                AgencyName = qIncarceration.ReleaseToOtherAgencyName,
                TransportScheduleDate = qIncarceration.TransportScheduleDate,
                InmateId = qIncarceration.InmateId ?? 0,
                AltSentFlag = qIncarceration.AltSent != null,
                VerifyId=qIncarceration.VerifyIDFlag,
                InmateClassification = _context.InmateClassification.FirstOrDefault(inmclass =>
                        inmclass.InmateId == inmateId && !inmclass.InmateDateUnassigned.HasValue)
                    ?.InmateClassificationReason,
                WorkCrewName = _context.WorkCrew
                    .SingleOrDefault(wc => wc.InmateId == inmateId && wc.DeleteFlag == 0)?.WorkCrewLookup?
                    .CrewName,
                WorkCrewRequest = _context.WorkCrewRequest
                    .Where(wcr =>
                        wcr.InmateId == inmateId && !wcr.DeleteFlag.HasValue && !wcr.DeniedFlag.HasValue &&
                        !wcr.WorkCrewLookup.WorkFurloughFlag.HasValue).Select(wcr => wcr.WorkCrewLookup.CrewName)
                    .ToArray(),
                WorkFurloughRequest = _context.WorkCrewRequest
                    .Where(wcr =>
                        wcr.InmateId == inmateId && !wcr.DeleteFlag.HasValue && !wcr.DeniedFlag.HasValue &&
                        wcr.WorkCrewLookup.WorkFurloughFlag.HasValue).Select(wcr => wcr.WorkCrewLookup.CrewName)
                    .ToArray(),
                Contact = _context.Contact.Count(con => con.TypePersonId == personId && con.VictimNotify == 1),
                AltSentRequest = _context.AltSentRequestProgram
                    .Where(asp =>
                        asp.AltSentRequest.InmateId == inmateId && !asp.AltSentRequest.RejectFlag.HasValue &&
                        !asp.AltSentRequest.ApproveFlag.HasValue && !asp.AltSentRequest.DeleteFlag.HasValue)
                    .Select(asp => asp.AltSentProgram.AltSentProgramAbbr).ToArray(),
                AltSentProgram = _context.AltSentRequestProgram
                    .Where(asp =>
                        asp.AltSentRequest.InmateId == inmateId && !asp.AltSentRequest.RejectFlag.HasValue &&
                        !asp.AltSentRequest.DeleteFlag.HasValue)
                    .Select(asp => asp.AltSentProgram.AltSentProgramAbbr).ToArray(),
                IssuedProperty = _context.IssuedProperty.Count(ip =>
                    ip.InmateId == inmateId && ip.ActiveFlag == 1 && ip.DeleteFlag == 0 &&
                    ip.IssuedPropertyLookup.ExpireUponRelease == 1),
                CondOfRelFlags = _context.IncarcerationCondRelease
                    .Where(icr => icr.IncarcerationId == incarcerationId && icr.DeleteFlag != 1).Select(icr =>
                        new KeyValuePair<string, string>
                        (
                            icr.CondOfRelease,
                            icr.CondOfReleaseNote
                        )).ToList(),
                DNA = _context.PersonDna.Count(ip =>
                    ip.PersonId == personId && ip.DnaRequested == 1 && ip.DnaDateGathered.HasValue),
                Vehicle = _context.InmatePrebook
                    .Where(inmPre => inmPre.PersonId == personId && inmPre.VehicleInvolvedFlag.HasValue &&
                                     inmPre.VehicleInvolvedFlag.Value == 1 && inmPre.IncarcerationId == incarcerationId)
                    .Select(inmPre => new VechileVm
                    {
                        License = inmPre.VehicleLicense,
                        Disposition = inmPre.VehicleDisposition,
                        Location = inmPre.VehicleLocation,
                        MakeName = inmPre.VehicleMake.VehicleMakeName,
                        ModelName = inmPre.VehicleModel.VehicleModelName
                    }).SingleOrDefault()
            };

            int? arrestId = _context.IncarcerationArrestXref
                .FirstOrDefault(arr => arr.IncarcerationId == incarcerationId)?.ArrestId;

            if (arrestId.HasValue)
            {
                inmRelease.ArrestActive = _context.Arrest.Count(ar =>
                                              ar.ArrestId == arrestId &&
                                              (ar.ArrestActive.HasValue || ar.ArrestActive.Value > 0)) > 0;
            }

            if (qIncarceration.TransportCoOpRoute != null)
            {
                inmRelease.CoOpRoute = _context.Lookup.SingleOrDefault(look =>
                    look.LookupType == LookupConstants.TRANSROUTE &&
                    look.LookupIndex == qIncarceration.TransportCoOpRoute
                    && look.LookupInactive == 0)?.LookupDescription;
            }

            Inmate qInmate = _context.Inmate.Single(inc => inc.InmateId == inmRelease.InmateId);

            inmRelease.DebtAmt = qInmate.InmateDebt;
            inmRelease.PersonalInventory = qInmate.InmatePersonalInventory;
            inmRelease.InmateSupply = _context.InmateSupply.Count(inmSupp =>
                inmSupp.InmateId == inmateId && !inmSupp.DeleteFlag.HasValue && !inmSupp.CheckinDate.HasValue);
            inmRelease.InmateLocation = qInmate.InmateCurrentTrack;
            inmRelease.HousingFlag = qInmate.HousingUnitId > 0;
            inmRelease.InmateActive = qInmate.InmateActive == 1;
            if (qInmate.HousingUnitId.HasValue)
            {
                inmRelease.HousingDetails = _facilityHousingService.GetHousingDetails(qInmate.HousingUnitId.Value);
            }

            inmRelease.WorkCrewFlag = qInmate.WorkCrewId > 0;

            IQueryable<PersonFlag> personFlag = _context.PersonFlag.Where(w =>
                w.PersonId == personId && w.DeleteFlag == 0 &&
                (w.InmateFlagIndex.HasValue || w.PersonFlagIndex.HasValue ||
                 w.DietFlagIndex.HasValue));

            inmRelease.PersonAlert = personFlag.SelectMany(
                p => lstLookup.Where(w => w.LookupType == LookupConstants.PERSONCAUTION &&
                    w.LookupIndex == p.PersonFlagIndex &&
                    p.PersonFlagIndex > 0 ||
                    w.LookupType == LookupConstants.TRANSCAUTION &&
                    w.LookupIndex == p.InmateFlagIndex &&
                    p.InmateFlagIndex > 0 ||
                    w.LookupType == LookupConstants.DIET &&
                    w.LookupIndex == p.DietFlagIndex &&
                    p.DietFlagIndex > 0 ||
                    w.LookupType == LookupConstants.MEDFLAG &&
                    w.LookupIndex == p.MedicalFlagIndex &&
                    p.MedicalFlagIndex > 0 ||
                    w.LookupType == LookupConstants.DIET &&
                    w.LookupIndex == p.ProgramAltSentFlagIndex &&
                    p.ProgramAltSentFlagIndex > 0 &&
                    p.DeleteFlag == 0
                )).Count();

            IQueryable<AltSent> lstAltSent = _context.AltSent
                .Where(alt => alt.IncarcerationId == incarcerationId && !alt.AltSentClearFlag.HasValue);

            inmRelease.AltSentCount = lstAltSent.Count();

            inmRelease.AltSentAttend = lstAltSent
                .Where(alt => alt.IncarcerationId == incarcerationId && !alt.AltSentClearFlag.HasValue).Select(alt =>
                    new KeyValuePair<int?, int?>
                    (
                        alt.AltSentTotalAttend,
                        alt.AltSentAdts
                    )).LastOrDefault();

            int[] programId = new int[0];

            // This program table need to change based on programClass
            string[] classOrServiceName = _context.Program
                .Where(w => programId.Contains(w.ProgramId) && w.DeleteFlag == 0).Select(s => s.ClassOrServiceName)
                .ToArray();
            inmRelease.ProgramAssign = string.Join(',', classOrServiceName);

            programId = _context.ProgramRequest.Where(w => w.InmateId == inmateId && w.DeleteFlag == 0)
                .Select(s => s.ProgramClassId).ToArray();

            classOrServiceName = _context.Program.Where(w => programId.Contains(w.ProgramId) && w.DeleteFlag == 0)
                .Select(s => s.ClassOrServiceName).ToArray();

            inmRelease.ProgramRequest = string.Join(',', classOrServiceName);

            inmRelease.TaskDetails = _inmateService.GetInmateTasks(inmateId, TaskValidateType.DoRelease);

            return inmRelease;
        }

        public List<BookingNumberList> GetInmateByBooking(string bookingNumber, int inmateId)
        {
            int? incarcerationId = null;
            if (!(inmateId is 0))
            {
                incarcerationId = _context.Incarceration
                    .Single(inc => inc.InmateId == inmateId && !inc.ReleaseOut.HasValue).IncarcerationId;
            }

            List<BookingNumberList> bookingNumberList;

            if (incarcerationId.HasValue)
            {
                List<BookingNumberList> arrestXrefDet = _context.IncarcerationArrestXref.Where(incArr =>
                        incArr.IncarcerationId.Value == incarcerationId)
                    .Select(incArr => new BookingNumberList
                    {
                        BookAndReleaseFlag = incArr.Incarceration.BookAndReleaseFlag == 1,
                        InmateId = incArr.Incarceration.InmateId.Value,
                        IncarcerationId = incArr.IncarcerationId.Value,
                        ArrestId = incArr.ArrestId.Value
                    }).ToList();

                bookingNumberList = arrestXrefDet.SelectMany(incArr =>
                    _context.Arrest.Where(arr => arr.ArrestId == incArr.ArrestId), (cl, cr) => new BookingNumberList
                    {
                        BookAndReleaseFlag = cl.BookAndReleaseFlag,
                        BookingNumber = cr.ArrestBookingNo,
                        InmateId = cl.InmateId,
                        IncarcerationId = cl.IncarcerationId,
                        CaseType = cr.ArrestType
                    }).OrderBy(or => or.BookingNumber).ToList();
            }
            else
            {
                List<BookingNumberList> arrestLst = _context.Arrest.Where(arr =>
                        arr.ArrestBookingNo.StartsWith(bookingNumber))
                    .Select(incArr => new BookingNumberList
                    {
                        BookingNumber = incArr.ArrestBookingNo,
                        CaseType = incArr.ArrestType,
                        ArrestId = incArr.ArrestId
                    }).ToList();

                bookingNumberList = arrestLst.SelectMany(incArr =>
                    _context.IncarcerationArrestXref.Where(arr => arr.ArrestId == incArr.ArrestId), (cl, cr) =>
                    new BookingNumberList
                    {
                        BookingNumber = cl.BookingNumber,
                        IncarcerationId = cr.IncarcerationId ?? 0,
                        CaseType = cl.CaseType
                    }).ToList();

                //Incarceration not get in previous step
                bookingNumberList = bookingNumberList.SelectMany(incArr =>
                    _context.Incarceration.Where(arr => arr.IncarcerationId == incArr.IncarcerationId), (cl, cr) =>
                    new BookingNumberList
                    {
                        BookAndReleaseFlag = cr.BookAndReleaseFlag == 1,
                        InmateId = cr.InmateId ?? 0,
                        BookingNumber = cl.BookingNumber,
                        IncarcerationId = cl.IncarcerationId,
                        CaseType = cl.CaseType
                    }).OrderBy(or => or.BookingNumber).ToList();
            }

            int[] incIds = bookingNumberList.Select(book => book.IncarcerationId).ToArray();

            List<AoWizardProgressIncarceration> lstAoWizardProgress = _context.AoWizardProgressIncarceration.Where(
                awp => incIds.Contains(awp.IncarcerationId) && awp.AoWizardId == (int)Wizards.release).ToList();

            int[] lstWizardIds = lstAoWizardProgress.Select(lli => lli.AoWizardProgressId).ToArray();

            List<AoWizardStepProgress> lstAoWizardStepProgress = _context.AoWizardStepProgress
                .Where(aow => lstWizardIds.Contains(aow.AoWizardProgressId)).ToList();

            bookingNumberList.ForEach(rel =>
            {
                rel.WizardProgressId = lstAoWizardProgress.FirstOrDefault(awp =>
                    awp.IncarcerationId == rel.IncarcerationId &&
                    awp.AoWizardId == (int)Wizards.release)?.AoWizardProgressId;

                if (rel.WizardProgressId.HasValue)
                {
                    rel.LastStep = lstAoWizardStepProgress.Select(aw => new WizardStep
                    {
                        ComponentId = aw.AoComponentId,
                        WizardProgressId = aw.AoWizardProgressId,
                        WizardStepProgressId = aw.AoWizardStepProgressId,
                        StepComplete = aw.StepComplete
                    }).ToList();
                }
            });
            return bookingNumberList;
        }

        public async Task<int> UpdateDoReleaseAsync(int inmateId, int incarcerationId)
        {
            // workcrew Request unassign
            List<WorkCrewRequest> workCrewReq = _context.WorkCrewRequest
                .Where(s => s.InmateId == inmateId && s.DeleteFlag == 0).ToList();
            if (workCrewReq.Count > 0)
            {
                workCrewReq.ForEach(item =>
                {
                    item.DeniedFlag = 1;
                    item.DeniedBy = _personnelId;
                    item.DeniedDate = DateTime.Now;
                    item.DeniedReason = "RELEASE";
                });
            }

            // workcrew unassign
            List<WorkCrew> workCrew = _context.WorkCrew.Where(s => s.InmateId == inmateId).ToList();
            if (workCrew.Count > 0)
            {
                workCrew.ForEach(item =>
                {
                    item.UnassignReason = "1";
                    item.DeletedBy = _personnelId;
                    item.DeleteDate = DateTime.Now;
                    item.UnassignNote = "RELEASE";
                    item.EndDate = DateTime.Now;
                });
            }

            // inmateactive 
            Inmate inmate = _context.Inmate.Single(s => s.InmateId == inmateId);

            if(inmate.InmateCurrentTrackId.HasValue)
            {
                //Update Inmate Track table
                InmateTrak inmateTrak = _context.InmateTrak.FirstOrDefault(w =>
                    w.InmateId == inmate.InmateId && !w.InmateTrakDateIn.HasValue);

                if (inmateTrak != null)
                {
                    inmateTrak.InmateTrakDateIn = DateTime.Now;
                    inmateTrak.InPersonnelId = _personnelId;
                }
            }

            inmate.InmateActive = 0;
            inmate.InmateCurrentTrack = null;
            inmate.InmateCurrentTrackId = null;
            inmate.HousingUnitId = null;
            inmate.InmateClassificationId = null;

            HousingUnitMoveHistory housingUnitMoveHistory = _context.HousingUnitMoveHistory
                .SingleOrDefault(hou => hou.InmateId == inmateId && !hou.MoveDateThru.HasValue);

            if (!(housingUnitMoveHistory is null))
            {
                housingUnitMoveHistory.MoveDateThru = DateTime.Now;
                housingUnitMoveHistory.MoveThruBy = _personnelId;
            }


            // AltSent Request
            List<AltSentRequest> altSentRequests = _context.AltSentRequest.Where(w => w.InmateId == inmateId).ToList();
            if (altSentRequests.Count > 0)
            {
                altSentRequests.ForEach(item => { item.DeleteFlag = 1; });

                // AltSent Request Appeal
                int[] altSentRequestId = altSentRequests.Select(s => s.AltSentRequestId).ToArray();
                List<AltSentRequestAppeal> altSentRequestAppeal = _context.AltSentRequestAppeal
                    .Where(w => altSentRequestId.Contains(w.AltSentRequestId)).ToList();
                if (altSentRequestAppeal.Count > 0)
                {
                    altSentRequestAppeal.ForEach(item => { item.DeleteFlag = 1; });
                }
            }

            // Appointment Program unassign
            List<AppointmentProgramAssign> appointmentProgramAssigns =
                _context.AppointmentProgramAssign.Where(w => w.InmateId == inmateId).ToList();
            if (appointmentProgramAssigns.Count > 0)
            {
                appointmentProgramAssigns.ForEach(item =>
                {
                    item.DeleteDate = DateTime.Now;
                    item.DeleteBy = _personnelId;
                    item.DeleteFlag = 1;
                });
            }

            // unassign altsent
            List<AltSent> altSent = _context.AltSent
                .Where(w => w.IncarcerationId == incarcerationId && !w.AltSentThru.HasValue).ToList();
            if (altSent.Count > 0)
            {
                if (altSent.First().AltSentClearFlag == 1)
                {
                    altSent.ForEach(item => { item.AltSentThru = DateTime.Now; });
                }
                else
                {
                    altSent.ForEach(item =>
                    {
                        item.AltSentThru = DateTime.Now;
                        item.AltSentClearFlag = 1;
                        item.AltSentClearBy = _personnelId;
                        item.AltSentClearDate = DateTime.Now;
                    });
                }
            }

            // incarceration 
            Incarceration incarceration = _context.Incarceration.Single(s => s.IncarcerationId == incarcerationId);
            incarceration.CurrentAltSentId = null;
            // altsentrequest again
            altSentRequests = _context.AltSentRequest.Where(w => w.InmateId == inmateId && !w.AltSentId.HasValue &&
                                                                 !w.DeleteFlag.HasValue && !w.RejectFlag.HasValue)
                .ToList();
            if (altSentRequests.Count > 0)
            {
                altSentRequests.ForEach(item =>
                {
                    item.DeleteFlag = 1;
                    item.DeleteDate = DateTime.Now;
                    item.DeletedBy = _personnelId;
                });
            }

            // Ao_release_inmate sp
            List<IncarcerationFacility> incarcerationFacilities =
                _context.IncarcerationFacility.Where(w => w.IncarcerationId == incarcerationId).ToList();
            if (incarcerationFacilities.Count > 0)
            {
                incarcerationFacilities.ForEach(item =>
                {
                    item.IncarcerationTo = DateTime.Now;
                    item.IncarcerationToBy = _personnelId;
                });
            }

            IncarcerationFacilityHistory incarcerationFacilityHistory = _context.IncarcerationFacilityHistory
                .OrderByDescending(o => o.MoveDate)
                .FirstOrDefault(s => s.IncarcerationId == incarcerationId && s.InmateId == inmateId);
            if (incarcerationFacilityHistory != null)
            {
                incarcerationFacilityHistory.MoveDateThru = DateTime.Now;
                incarcerationFacilityHistory.MoveDateThruBy = _personnelId;
            }

            incarceration.ReleaseCompleteFlag = 1;
            incarceration.ReleaseCompleteBy = _personnelId;
            incarceration.ReleaseCompleteDate = DateTime.Now;
            incarceration.ReleaseOut = DateTime.Now;
            incarceration.FacilityIdOut = incarceration.Inmate.FacilityId;
            incarceration.OutOfficerId = _personnelId;

            // person flag 
            int[] lookupList = _commonService.GetLookupList(LookupConstants.PERSONCAUTION)
                .Where(w => w.LookupAlertRemoveRelease == 1).Select(s => s.LookupIndex).ToArray();

            List<PersonFlag> personFlags = _context.PersonFlag.Where(w => w.PersonId == incarceration.Inmate.PersonId &&
                    lookupList.Contains(w.PersonFlagIndex.Value))
                .ToList();

            lookupList = _commonService.GetLookupList(LookupConstants.TRANSCAUTION)
                .Where(w => w.LookupAlertRemoveRelease == 1).Select(s => s.LookupIndex).ToArray();
            personFlags.AddRange(_context.PersonFlag.Where(w => w.PersonId == incarceration.Inmate.PersonId &&
                lookupList.Contains(w.InmateFlagIndex.Value)).ToList());

            lookupList = _commonService.GetLookupList(LookupConstants.DIET).Where(w => w.LookupAlertRemoveRelease == 1)
                .Select(s => s.LookupIndex).ToArray();
            personFlags.AddRange(_context.PersonFlag.Where(w => w.PersonId == incarceration.Inmate.PersonId &&
                lookupList.Contains(w.DietFlagIndex.Value)).ToList());

            lookupList = _commonService.GetLookupList(LookupConstants.MEDFLAG)
                .Where(w => w.LookupAlertRemoveRelease == 1).Select(s => s.LookupIndex).ToArray();
            personFlags.AddRange(_context.PersonFlag.Where(w => w.PersonId == incarceration.Inmate.PersonId &&
                lookupList.Contains(w.MedicalFlagIndex.Value)).ToList());

            lookupList = _commonService.GetLookupList(LookupConstants.ALTSENTFLAG)
                .Where(w => w.LookupAlertRemoveRelease == 1).Select(s => s.LookupIndex).ToArray();
            personFlags.AddRange(_context.PersonFlag.Where(w => w.PersonId == incarceration.Inmate.PersonId &&
                lookupList.Contains(w.ProgramAltSentFlagIndex.Value)).ToList());
            if (personFlags.Count > 0)
            {
                personFlags.ForEach(item => { item.DeleteFlag = 1; });

                _context.PersonFlagHistory.AddRange(personFlags.Select(s =>
                    new PersonFlagHistory
                    {
                        CreateBy = s.CreateBy,
                        CreateDate = s.CreateDate,
                        DeleteBy = s.DeleteBy,
                        DeleteDate = s.DeleteDate,
                        DeleteFlag = s.DeleteFlag,
                        DietFlagIndex = s.DietFlagIndex,
                        FlagExpire = s.FlagExpire,
                        FlagNote = s.FlagNote,
                        InmateFlagIndex = s.InmateFlagIndex,
                        MedicalFlagIndex = s.MedicalFlagIndex,
                        PersonFlagIndex = s.PersonFlagIndex,
                        ProgramAltSentIndex = s.ProgramAltSentFlagIndex,
                        PersonId = s.PersonId,
                        ProgramCaseIndex = s.ProgramCaseFlagIndex
                    }).ToList());
            }

            /* TODO Insert audit for released inmate with modules */

            /* Remove Finger Print */

            _interfaceEngine.Export(new ExportRequestVm
            {
                EventName = "RELEASE",
                PersonnelId = _personnelId,
                Param1 = incarceration.Inmate.PersonId.ToString(),
                Param2 = incarcerationId.ToString()
            });

            if (incarceration.Inmate.InmateActive == 0)
            {
                List<PersonCitizenship> lstPersonCitizenship =
                 _context.PersonCitizenship.Where(w => w.PersonId == incarceration.Inmate.PersonId
                 && w.NotificationAcknowledgement).ToList();
                lstPersonCitizenship.ForEach(fe =>
                {
                    fe.NotificationAcknowledgement = false;
                });
            }


            List<InmatePrivilegeXref> lstPrivilege = _context.InmatePrivilegeXref.Where(ipx =>
                ipx.InmateId == inmateId && ipx.PrivilegeDate < DateTime.Now &&
                (!ipx.PrivilegeExpires.HasValue || ipx.PrivilegeExpires >= DateTime.Now)).ToList();

            lstPrivilege.ForEach(step =>
            {
                step.PrivilegeRemoveDatetime = DateTime.Now;
                step.PrivilegeRemoveOfficerId = _personnelId;
                step.PrivilegeRemoveNote = "RELEASE";
                _context.SaveChanges();
            });


            return await _context.SaveChangesAsync();
        }

        public async Task<int> UpdateUndoReleaseAsync(int inmateId, int incarcerationId)
        {
            Incarceration incarceration = _context.Incarceration.Single(s => s.IncarcerationId == incarcerationId);

            int[] arrestIds = _context.IncarcerationArrestXref
                .Where(xref => xref.IncarcerationId == incarcerationId && xref.ArrestId.HasValue)
                .Select(xref => xref.ArrestId.Value).ToArray();

            incarceration.ReleaseOut = null;
            incarceration.ReleaseSupervisorCompleteFlag = null;
            incarceration.ReleaseCompleteFlag = null;
            incarceration.ReleaseCompleteDate = null;
            incarceration.ReleaseCompleteBy = null;
            incarceration.ReleaseClearFlag = null;
            incarceration.OutOfficerId = null;
            Inmate inmate = _context.Inmate.Single(s => s.InmateId == inmateId);
            inmate.InmateActive = 1;

            int[] wizardProgressIds =
                _context.AoWizardProgressIncarceration.Where(awp =>
                        awp.IncarcerationId == incarceration.IncarcerationId &&
                        (awp.AoWizardId == (int) Wizards.release || awp.AoWizardId == (int) Wizards.releaseSupervisor))
                    .Select(wizard => wizard.AoWizardProgressId).ToArray();

            wizardProgressIds = wizardProgressIds.Concat(_context.AoWizardProgressArrest.Where(awp =>
                        arrestIds.Contains(awp.ArrestId) && awp.AoWizardId == (int)Wizards.bookingDataSupervisorClear)
                    .Select(wizard => wizard.AoWizardProgressId).ToArray()).ToArray();

            List<AoWizardStepProgress> lstAoWizardStepProgress = _context.AoWizardStepProgress.Where(step =>
                wizardProgressIds.Contains(step.AoWizardProgressId)).ToList();

            lstAoWizardStepProgress.ForEach(step =>
            {
                step.StepComplete = false;
                step.StepCompleteBy = null;
                step.StepCompleteNote = null;
                _context.SaveChanges();
            });
            
            return await _context.SaveChangesAsync();
        }

        #region Calculate BailTotalAmount

        public bool UpdateOverAllChargeLevel(int incarcerationId)
        {
            //To get ArrestIds for Incarceration Charge level
            int[] arrestIds = _context.IncarcerationArrestXref
                .Where(i => i.ArrestId.HasValue && i.IncarcerationId == incarcerationId && !i.ReleaseDate.HasValue)
                .Select(i => i.ArrestId ?? 0).ToArray();

            if (arrestIds.Length <= 0)
                return false;

            //To get current-charge-level Details
            KeyValuePair<int, string> chargeLevelDetails = _context.Incarceration
                .Where(inc => inc.IncarcerationId == incarcerationId)
                .Select(inc => new KeyValuePair<int, string>(inc.ChargeLevelOverrideFlag ?? 0, inc.ChargeLevel))
                .SingleOrDefault();

            if (chargeLevelDetails.Key != 0) return false;
            //Get Highest Charge Level
            List<int> lookupIndexLst = _context.Lookup.Where(lkp => lkp.LookupType == LookupConstants.CRIMETYPE
                    && (!lkp.LookupFlag6.HasValue || lkp.LookupFlag6 == 0))
                .Select(l => l.LookupIndex).ToList();

            //Get charge level based on 'Warrant' details
            List<string> chargeLevelLst = _context.Warrant
                .Where(war => arrestIds.Contains(war.ArrestId ?? 0) &&
                              !string.IsNullOrEmpty(war.WarrantChargeType) &&
                              war.Crime.Count == 0).Select(w => w.WarrantChargeType).ToList();

            //Get charge level based on 'Charges & Warrant_Charges'
            List<string> crimeCodeTypes = _context.CrimeLookup.SelectMany(crl => _context.Crime
                        .Where(cr => cr.CrimeDeleteFlag == 0 && (string.IsNullOrEmpty(cr.CrimeType) ||
                                lookupIndexLst.Contains(Convert.ToInt32(cr.CrimeType)))
                            && cr.CrimeLookupId == crl.CrimeLookupId &&
                            arrestIds.Contains(cr.ArrestId ?? 0)
                            && !string.IsNullOrEmpty(crl.CrimeCodeType)),
                    (cl, cr) => cl.CrimeCodeType)
                .Distinct().ToList();

            if (crimeCodeTypes.Any())
            {
                chargeLevelLst.AddRange(crimeCodeTypes);
            }

            //Get charge level based on 'Force Charges'
            chargeLevelLst.AddRange(_context.CrimeForce.Where(cfr => cfr.DeleteFlag == 0 &&
                    !cfr.ForceCrimeLookupId.HasValue &&
                    !cfr.DropChargeFlag.HasValue &&
                    !cfr.SearchCrimeLookupId.HasValue &&
                    arrestIds.Contains(cfr.ArrestId ?? 0) &&
                    !string.IsNullOrEmpty(cfr.TempCrimeCodeType) &&
                    (string.IsNullOrEmpty(cfr.TempCrimeType) ||
                        lookupIndexLst.Contains(
                            Convert.ToInt32(cfr.TempCrimeType))))
                .Select(cf => cf.TempCrimeCodeType).ToList());

            chargeLevelLst = chargeLevelLst.Distinct().ToList();
            if (!chargeLevelLst.Any()) return true;
            //To get charges crime type for crime code
            //'Enum.IsDefined()' -> Identify crime type is available in particular Enum or not(true/false)
            string highestChargeLevel = chargeLevelLst.Select(crimeType =>
                    new KeyValuePair<int, string>(Enum.IsDefined(typeof(CrimeCode), crimeType)
                        ? (int) Enum.Parse(typeof(CrimeCode), crimeType)
                        : 7, crimeType))
                .OrderBy(cc => cc.Key).First().Value;

            //Update Charge level in 'Incarceration' table
            if (highestChargeLevel == chargeLevelDetails.Value) return true;
            Incarceration incarceration =
                _context.Incarceration.SingleOrDefault(inca => inca.IncarcerationId == incarcerationId);
            if (incarceration == null) return true;
            incarceration.ChargeLevel = highestChargeLevel;
            _context.SaveChanges();

            return true;
        }

        public void CalculateBailTotalAmount(int arrestId, int personId, bool doNotDoSaveHistory, bool bailTransaction)
        {
            //SiteOptions value for CrimeId
            int siteOptionId = _context.SiteOptions
                .Where(s => s.SiteOptionsName == SiteOptionsConstants.HIGHESTBAILPERBOOKING &&
                            s.SiteOptionsValue == SiteOptionsConstants.ON &&
                            s.SiteOptionsStatus == "1")
                .Select(s => s.SiteOptionsId).FirstOrDefault();

            //Taking incarcerationId based on arrestId
            int incarcerationId = _context.IncarcerationArrestXref
                .Where(i => i.Arrest.ArrestId == arrestId)
                .OrderByDescending(i => i.Incarceration.IncarcerationId)
                .Select(i => i.Incarceration.IncarcerationId)
                .FirstOrDefault();

            if (siteOptionId > 0)
            {
                List<Crime> lstCrime = _context.Crime.Where(c => c.ArrestId == arrestId).ToList();

                //Updating CrimePrimaryFlag
                lstCrime.ForEach(item => item.CrimePrimaryFlag = null);

                if (lstCrime.Any(a => a.BailType == BailType.NOBAIL))
                {
                    Crime updateCrime = lstCrime.First(s => s.BailType == BailType.NOBAIL);
                    updateCrime.CrimePrimaryFlag = 1;
                }
                else if (lstCrime.Any(a => !a.WarrantId.HasValue))
                {
                    Crime updateCrime = lstCrime.OrderByDescending(c => c.BailAmount)
                        .First(s => !s.WarrantId.HasValue);
                    updateCrime.CrimePrimaryFlag = 1;
                }
            }

            //getting BailType and BailAmount value for updating BailTransaction
            UpdateBailTransaction(arrestId, personId, bailTransaction, siteOptionId);

            //updating BailFlag
            UpdateIncarcerationFlagDetails(incarcerationId);

            //Update the Chargelevel to Incarceration 
            UpdateOverAllChargeLevel(incarcerationId);

            //Save BailHistory table
            SaveBailHistoryTable(arrestId, doNotDoSaveHistory);
        }

        public BailDetails BailAmount(int arrestId, int personId, int siteOptionId)
        {
            //To get Bail details from Crime table
            List<BailDetails> lstInmateBailDetails = _context.Crime
                .Where(c => (!c.WarrantId.HasValue || c.WarrantId == 0) &&
                            c.CrimeDeleteFlag == 0 && c.ArrestId == arrestId)
                .Select(c => new BailDetails
                {
                    BailAmount = c.BailAmount,
                    BailType = c.BailType
                }).ToList();

            //To get Bail details from CrimeForce table
            lstInmateBailDetails.AddRange(_context.CrimeForce
                .Where(c => (!c.ForceSupervisorReviewFlag.HasValue || c.ForceSupervisorReviewFlag == 0) &&
                            c.DeleteFlag == 0 && (!c.WarrantId.HasValue || c.WarrantId == 0) &&
                            c.ArrestId == arrestId)
                .Select(c => new BailDetails
                {
                    BailAmount = c.BailAmount,
                    BailType = c.BailNoBailFlag == 1
                        ? BailType.NOBAIL
                        : string.Empty // BailNoBailFlag 1 means 'No Bail'
                }).ToList());

            //To get Bail details from Warrant table
            List<BailDetails> lstWarrantBailDetails = _context.Warrant
                .Where(w => w.ArrestId == arrestId && w.LocalWarrantFlag == 1 &&
                            w.PersonId == personId)
                .Select(w => new BailDetails
                {
                    BailType = w.WarrantBailType,
                    BailAmount = w.WarrantBailAmount
                }).ToList();

            //getting BailType and BailAmount value for updating BailTransaction
            BailDetails bail = new BailDetails();

            if (lstInmateBailDetails.All(b => b.BailType != BailType.NOBAIL) && lstInmateBailDetails.Count > 0 ||
                lstWarrantBailDetails.All(w => w.BailType != BailType.NOBAIL) && lstWarrantBailDetails.Count > 0)
            {
                if (lstInmateBailDetails.Count > 0)
                {
                    bail.BailAmount = siteOptionId > 0
                        ? lstInmateBailDetails.Max(b => b.BailAmount)
                        : lstInmateBailDetails.Sum(b => b.BailAmount);
                }

                if (lstWarrantBailDetails.Count > 0)
                {
                    bail.BailAmount = bail.BailAmount + lstWarrantBailDetails.Sum(w => w.BailAmount);
                }

                bail.BailType = lstInmateBailDetails.LastOrDefault()?.BailType == BailType.CASHONLY ||
                                lstWarrantBailDetails.LastOrDefault()?.BailType == BailType.CASHONLY
                    ? BailType.CASHONLY
                    : BailType.BONDABLE;
            }
            else
            {
                bail.BailAmount = 0;
                bail.BailType = BailType.NOBAIL;
            }

            return bail;
        }

        private void UpdateBailTransaction(int arrestId, int personId, bool bailTransaction, int siteOptionId)
        {
            BailDetails bail = BailAmount(arrestId, personId, siteOptionId);

            //updating the Arrest Bail amount based on Bailtype
            if (bailTransaction) return;
            Arrest updateArrest = _context.Arrest.Single(a => a.ArrestId == arrestId);
            updateArrest.BailAmount = bail.BailAmount;
            updateArrest.BailType = bail.BailType;
            updateArrest.BailNoBailFlag = bail.BailType == BailType.NOBAIL ? 1 : 0;
        }

        private void SaveBailHistoryTable(int arrestId, bool doNotDoSaveHistory)
        {
            if (doNotDoSaveHistory) return;
            //Getting list from Arrestdetails to save BailHistory table
            Arrest arrestBailDetails = _context.Arrest.Single(a => a.ArrestId == arrestId);

            BailSaveHistory bailSave = new BailSaveHistory
            {
                ArrestId = arrestBailDetails.ArrestId,
                BailOfficerId = _personnelId,
                BailAmount = arrestBailDetails.BailAmount ?? 0,
                BailPercentPosted = arrestBailDetails.BailPercentPosted ?? 0,
                BailPostedBy = arrestBailDetails.BailPostedBy,
                BailReceiptNumber = arrestBailDetails.BailReceiptNumber,
                BailNoBailFlag = Convert.ToString(arrestBailDetails.BailNoBailFlag ?? 0)
            };
            _context.BailSaveHistory.Add(bailSave);
        }

        private void UpdateIncarcerationFlagDetails(int incarcerationId)
        {
            //Taking IncarcerationArrestXref details for updating BailFlag
            List<BailDetails> lstArrestBail = _context.IncarcerationArrestXref
                .Where(i => !i.ReleaseDate.HasValue && i.Incarceration.IncarcerationId == incarcerationId)
                .Select(i => new BailDetails
                {
                    BailAmount = i.Arrest.BailAmount,
                    BailFlag = i.Arrest.BailNoBailFlag == 1
                }).ToList();

            //To get Incarceration details
            Incarceration updateIncarceration =
                _context.Incarceration.Single(i => i.IncarcerationId == incarcerationId);

            //updating the Incarceration based on BailFlag
            if (lstArrestBail.Any(a => a.BailFlag))
            {
                updateIncarceration.BailNoBailFlagTotal = 1;
                updateIncarceration.BailAmountTotal = 0;
            }
            else
            {
                updateIncarceration.BailNoBailFlagTotal = null;
                updateIncarceration.BailAmountTotal = lstArrestBail.Sum(a => a.BailAmount);
            }
        }

        public OverallIncarceration getOverallIncarceration(int incarcerationId, int userControlId)
        {
            IQueryable<PersonnelVm> lstPersonnel = _context.Personnel.Select(s => new PersonnelVm
            {
                PersonLastName = s.PersonNavigation.PersonLastName,
                PersonFirstName = s.PersonNavigation.PersonFirstName,
                OfficerBadgeNumber = s.OfficerBadgeNum,
                PersonnelId = s.PersonnelId
            });

            OverallIncarceration overallIncarceration = _context.Incarceration
                .Where(w => w.IncarcerationId == incarcerationId)
                .Select(s => new OverallIncarceration
                {
                    DateIn = s.DateIn,
                    DateOut = s.ReleaseOut,
                    ChargeType = s.ChargeLevel,
                    ChargeFlag = s.ChargeLevelOverrideFlag == 1,
                    IntakeCompleteFlag = s.IntakeCompleteFlag > 0,
                    FacilityAbbr = s.FacilityIdInNavigation.FacilityAbbr,
                    BailAmountTotal = s.BailAmountTotal,
                    BailNoBailFlagTotal = s.BailNoBailFlagTotal,
                    ReceiveMethod = s.ReceiveMethod,
                    OverallSentStartDate = s.OverallSentStartDate,
                    OverallFinalReleaseDate = s.OverallFinalReleaseDate,
                    OverallConditionOfRelease = s.OverallConditionOfRelease,
                    BookAndReleaseFlag = s.BookAndReleaseFlag == 1,
                    ExpediteBookingReason = s.ExpediteBookingReason,
                    ExpediteBookingFlag = s.ExpediteBookingFlag,
                    ExpediteBookingNote = s.ExpediteBookingNote
                }).Single();

            overallIncarceration.ConditionRelease = _context.Lookup
                .Where(w => w.LookupType == LookupConstants.OVERALLCONDREL && w.LookupInactive == 0)
                .Select(s => new KeyValuePair<string, string>(s.LookupDescription, "")).Distinct().ToList();

            overallIncarceration.RealeaseCondtions = _context.IncarcerationCondRelease
                .Where(i => i.IncarcerationId == incarcerationId).Select(i =>
                    new KeyValuePair<string, string>(i.CondOfRelease, i.CondOfReleaseNote)).ToList();

            overallIncarceration.ListOverallWizard = _context.Incarceration
                .Where(w => w.IncarcerationId == incarcerationId)
                .Select(s => new OverallWizard
                {
                    WizardName = EventNameConstants.INTAKECOMPLETED,
                    CompleteDate = s.IntakeCompleteDate,
                    Personnel = lstPersonnel.SingleOrDefault(w => w.PersonnelId == s.IntakeCompleteBy),
                    Order = (int) Order.INTAKECOMPLETED
                }).ToList();

            overallIncarceration.ListOverallWizard.AddRange(_context.Incarceration
                .Where(w => w.IncarcerationId == incarcerationId)
                .Select(s => new OverallWizard
                {
                    WizardName = EventNameConstants.BOOKINGCOMPLETED,
                    CompleteDate = s.BookCompleteDate,
                    Personnel = lstPersonnel.SingleOrDefault(w => w.PersonnelId == s.BookCompleteBy),
                    Order = (int) Order.BOOKINGCOMPLETED
                }).ToList());

            overallIncarceration.ListOverallWizard.AddRange(_context.Incarceration
                .Where(w => w.IncarcerationId == incarcerationId)
                .Select(s => new OverallWizard
                {
                    WizardName = EventNameConstants.SUPERVISORCOMPLETED,
                    CompleteDate = s.BookingSupervisorCompleteDate,
                    Personnel = lstPersonnel.SingleOrDefault(w => w.PersonnelId == s.BookingSupervisorCompleteBy),
                    Order = (int)Order.SUPERVISORCOMPLETED
                }).ToList());
            overallIncarceration.ListOverallWizard.AddRange(_context.Incarceration
                .Where(w => w.IncarcerationId == incarcerationId)
                .Select(s => new OverallWizard
                {
                    WizardName = EventNameConstants.CLEARCOMPLETED,
                    CompleteDate = s.ReleaseClearDate,
                    Personnel = lstPersonnel.SingleOrDefault(w => w.PersonnelId == s.ReleaseClearBy),
                    Order = (int)Order.CLEARCOMPLETED
                }).ToList());
            overallIncarceration.ListOverallWizard.AddRange(_context.Incarceration
                .Where(w => w.IncarcerationId == incarcerationId)
                .Select(s => new OverallWizard
                {
                    WizardName = EventNameConstants.RELEASECOMPLETED,
                    CompleteDate = s.ReleaseCompleteDate,
                    Personnel = lstPersonnel.SingleOrDefault(w => w.PersonnelId == s.ReleaseCompleteBy),
                    Order = (int)Order.RELEASECOMPLETED
                }).OrderBy(o => o.Order).ToList());

            overallIncarceration.OverallUserControls = _context.AppAoUserControlFields.Where(w =>
                    w.AppAoUserControlId == userControlId && w.FieldTagId == "DdlReceiveMethod")
                .Select(s => new OverallUserControls
                {
                    FieldName = s.FieldLabel,
                    FieldRequired = s.FieldRequired,
                    FieldVisible = s.FieldVisible
                }).SingleOrDefault();

            return overallIncarceration;
        }

        public async Task<int> UpdateOverallIncarceration(OverallIncarceration overallIncarceration)
        {
            Incarceration incarceration = _context.Incarceration.Single(
                s => s.IncarcerationId == overallIncarceration.IncarcerationId);
            incarceration.OverallConditionOfRelease = overallIncarceration.OverallConditionOfRelease;
            incarceration.ChargeLevel = overallIncarceration.ChargeType;
            incarceration.ChargeLevelOverrideFlag = overallIncarceration.ChargeFlag ? 1 : 0;
            if (overallIncarceration.BookAndReleaseFlag)
            {
                incarceration.BookAndReleaseFlag = 1;
            }
            else
            {
                incarceration.BookAndReleaseFlag = null;
            }
            incarceration.ReceiveMethod = overallIncarceration.ReceiveMethod;
            incarceration.ExpediteBookingFlag = overallIncarceration.ExpediteBookingFlag;
            incarceration.ExpediteBookingDate = DateTime.Now;
            incarceration.ExpediteBookingNote = overallIncarceration.ExpediteBookingNote;
            incarceration.ExpediteBookingReason = overallIncarceration.ExpediteBookingReason;
            incarceration.ExpediteBookingBy = _personnelId;
            incarceration.ExpediteBookingByNavigation =
                _context.Personnel.Single(per => per.PersonnelId == _personnelId);

            _context.IncarcerationCondRelease.RemoveRange(_context.IncarcerationCondRelease.Where(
                s => s.IncarcerationId == overallIncarceration.IncarcerationId));

            _context.IncarcerationCondRelease.AddRange(overallIncarceration.ConditionRelease
                .Select(s => new IncarcerationCondRelease
                {
                    IncarcerationId = overallIncarceration.IncarcerationId ?? 0,
                    CondOfRelease = s.Key,
                    CondOfReleaseNote = s.Value,
                    CreateDate = DateTime.Now,
                    CreateBy = _personnelId
                }));

            return await _context.SaveChangesAsync();
        }

        public BookingTransportDetailsVm GetBookingTransportDetails(int incarcerationId, bool deleteFlag)
        {
            BookingTransportDetailsVm bookingTransportDetailsVm = new BookingTransportDetailsVm();

            IQueryable<Lookup> lookuplst = _context.Lookup
                .Where(w => (w.LookupType == LookupConstants.TRANSROUTE || w.LookupType == LookupConstants.TRANSTYPE) &&
                            w.LookupInactive == 0);

            IQueryable<PersonnelVm> lstPersonnel = _context.Personnel.Select(s => new PersonnelVm
            {
                PersonLastName = s.PersonNavigation.PersonLastName,
                PersonFirstName = s.PersonNavigation.PersonFirstName,
                PersonnelNumber = s.PersonnelNumber,
                PersonnelId = s.PersonnelId
            });

            bookingTransportDetailsVm.TransportInfo = _context.Incarceration
                .Where(w => w.IncarcerationId == incarcerationId)
                .Select(s => new TransportInfo
                {
                    TransportFlag = s.TransportFlag,
                    OtherAgencyId = s.ReleaseToOtherAgencyId,
                    AgencyName =
                        _context.Agency.SingleOrDefault(w => w.AgencyId == s.ReleaseToOtherAgencyId).AgencyName,
                    OtherAgencyName = s.ReleaseToOtherAgencyName,
                    CoOperativeFlag = s.TransportCoOpFlag,
                    HoldFlag = s.TransportHoldFlag,
                    TransportRoute = lookuplst.SingleOrDefault(w => w.LookupType == LookupConstants.TRANSROUTE &&
                                                                    w.LookupIndex == s.TransportCoOpRoute)
                        .LookupDescription,
                    HoldType = lookuplst.SingleOrDefault(w => w.LookupType == LookupConstants.TRANSTYPE &&
                                                              w.LookupIndex == s.TransportHoldType)
                        .LookupDescription,
                    HoldName = s.TransportHoldName,
                    ScheduleDate = s.TransportScheduleDate,
                    Instruction = s.TransportInstructions,
                    Caution = s.TransportInmateCautions,
                    Bail = s.TransportInmateBail,
                    ReturnAgency = s.TransportInmateReturn
                }).SingleOrDefault();

            bookingTransportDetailsVm.TransportNote = _context.IncarcerationTransportNote.Where(w =>
                    w.IncarcerationId == incarcerationId && (deleteFlag ? w.DeleteFlag >= 0 : w.DeleteFlag == 0))
                .Select(s => new TransportNote
                {
                    NoteId = s.IncarcerationTransportNoteId,
                    NoteDate = s.TranportNoteDate,
                    NoteType = s.TranportNoteType,
                    Note = s.TranportNote,
                    NoteBy = s.TranportNoteBy,
                    ReleaseOfficer = lstPersonnel.SingleOrDefault(w => w.PersonnelId == s.TranportNoteBy),
                    DeleteFlag = s.DeleteFlag
                }).OrderByDescending(o => o.NoteDate).ToList();

            return bookingTransportDetailsVm;
        }

        public TransportManageDetailVm GetTransportManageDetails(int incarcerationId)
        {
            TransportManageDetailVm transportManageDetailVm = new TransportManageDetailVm
            {
                Agency = _context.Agency.Select(s => new AgencyVm
                {
                    AgencyName = s.AgencyName,
                    AgencyId = s.AgencyId
                }).OrderBy(o => o.AgencyName).ToList()
            };

            IQueryable<Lookup> lookuplst = _context.Lookup
                .Where(w => (w.LookupType == LookupConstants.TRANSROUTE || w.LookupType == LookupConstants.TRANSTYPE) &&
                            w.LookupInactive == 0);

            IQueryable<PersonnelVm> lstPersonnel = _context.Personnel.Select(s => new PersonnelVm
            {
                PersonLastName = s.PersonNavigation.PersonLastName,
                OfficerBadgeNumber = s.OfficerBadgeNum,
                PersonnelId = s.PersonnelId
            });

            transportManageDetailVm.TransportManage = _context.Incarceration
                .Where(w => w.IncarcerationId == incarcerationId)
                .Select(s => new TransportManageVm
                {
                    IntakeCompleteFlag = s.IntakeCompleteFlag,
                    BailAmountTotal = s.BailAmountTotal,
                    BailNoBailFlagTotal = s.BailNoBailFlagTotal,
                    ChargeLevel = s.ChargeLevel,
                    OverallCondition = s.OverallConditionOfRelease,
                    FinalReleaseDate = s.OverallFinalReleaseDate,
                    TransportScheduleDate = s.TransportScheduleDate,
                    LevelOverrideFlag = s.ChargeLevelOverrideFlag,
                    HoldName = s.TransportHoldName,
                    Instructions = s.TransportInstructions,
                    InmateCautions = s.TransportInmateCautions,
                    InmateBail = s.TransportInmateBail,
                    InmateReturn = s.TransportInmateReturn,
                    CoopFlag = s.TransportCoOpFlag,
                    HoldFlag = s.TransportHoldFlag,
                    CoOpRouteId = s.TransportCoOpRoute,
                    HoldTypeId = s.TransportHoldType,
                    Route = lookuplst.SingleOrDefault(w => w.LookupType == LookupConstants.TRANSROUTE &&
                                                           w.LookupIndex == s.TransportCoOpRoute).LookupDescription,
                    Type = lookuplst.SingleOrDefault(w => w.LookupType == LookupConstants.TRANSTYPE &&
                                                          w.LookupIndex == s.TransportHoldType).LookupDescription,
                    PersonInName = lstPersonnel.SingleOrDefault(w => w.PersonnelId == s.InOfficerId),
                    PersonOutName = lstPersonnel.SingleOrDefault(w => w.PersonnelId == s.OutOfficerId),
                    AgencyName = transportManageDetailVm.Agency
                        .SingleOrDefault(w => w.AgencyId == s.ReleaseToOtherAgencyId).AgencyName,
                    AgencyId = s.ReleaseToOtherAgencyId,
                    OtherAgencyName = s.ReleaseToOtherAgencyName,
                    TransportFlag = s.TransportFlag,
                    BookAndReleaseFlag = s.BookAndReleaseFlag,
                    DateIn = s.DateIn,
                    ReleaseOut = s.ReleaseOut,
                    TransportUpdateDate = s.TransportUpdateDate,
                    TransportUpdateById = s.TransportUpdateBy,
                    TransportUpdateBy = lstPersonnel.SingleOrDefault(w => w.PersonnelId == s.TransportUpdateBy),
                    ReceiveMethod = s.ReceiveMethod
                }).SingleOrDefault();

            transportManageDetailVm.ConditionRelease = _context.IncarcerationCondRelease
                .Where(w => w.IncarcerationId == incarcerationId)
                .Select(s => new ConditionRelease
                {
                    Note = s.CondOfReleaseNote,
                    ReleaseFlag = s.CondOfRelease,
                    ConReleaseId = s.IncarcerationCondReleaseId,
                    DeleteFlag = s.DeleteFlag
                }).ToList();

            return transportManageDetailVm;
        }

        public int UpdateManageDetails(TransportManageVm transportManageVm)
        {
            Incarceration incarceration = _context.Incarceration.Single(
                s => s.IncarcerationId == transportManageVm.IncarcerationId);
            {
                incarceration.OverallConditionOfRelease = transportManageVm.OverallCondition;
                incarceration.ChargeLevel = transportManageVm.ChargeLevel;
                incarceration.TransportHoldName = transportManageVm.HoldName;
                incarceration.TransportInmateCautions = transportManageVm.InmateCautions;
                incarceration.TransportInstructions = transportManageVm.Instructions;
                incarceration.TransportCoOpFlag = transportManageVm.CoopFlag;
                incarceration.TransportHoldFlag = transportManageVm.HoldFlag;
                incarceration.TransportFlag = transportManageVm.TransportFlag;
                incarceration.TransportInmateBail = transportManageVm.InmateBail;
                incarceration.TransportInmateReturn = transportManageVm.InmateReturn;
                incarceration.TransportScheduleDate = transportManageVm.TransportScheduleDate;
                incarceration.ReleaseToOtherAgencyId = transportManageVm.AgencyId;
                incarceration.ReleaseToOtherAgencyName = transportManageVm.OtherAgencyName;
                incarceration.BookAndReleaseFlag = transportManageVm.BookAndReleaseFlag;
                incarceration.ChargeLevelOverrideFlag = transportManageVm.LevelOverrideFlag;
                incarceration.TransportUpdateDate = DateTime.Now;
                incarceration.TransportUpdateBy = _personnelId;
                incarceration.TransportCoOpRoute = transportManageVm.CoOpRouteId;
                incarceration.TransportHoldType = transportManageVm.HoldTypeId;
            }
            return _context.SaveChanges();
        }

        public int InsertTransportNote(TransportNote transportNote)
        {
            IncarcerationTransportNote incarcerationTransportNote = new IncarcerationTransportNote();
            {
                incarcerationTransportNote.IncarcerationId = transportNote.IncarcerationId;
                incarcerationTransportNote.TranportNoteDate = DateTime.Now;
                incarcerationTransportNote.TranportNoteBy = _personnelId;
                incarcerationTransportNote.TranportNoteType = transportNote.NoteType;
                incarcerationTransportNote.TranportNote = transportNote.Note ?? "";
            }

            _context.Add(incarcerationTransportNote);

            return _context.SaveChanges();
        }

        public int UpdateTransportNote(TransportNote transportNote)
        {
            IncarcerationTransportNote incarcerationTransportNote =
                _context.IncarcerationTransportNote.Single(w => w.IncarcerationTransportNoteId == transportNote.NoteId);
            {
                incarcerationTransportNote.DeleteFlag = transportNote.DeleteFlag == 0 ? 1 : default;
                incarcerationTransportNote.DeleteBy = _personnelId;
                incarcerationTransportNote.DeleteDate = DateTime.Now;
            }

            return _context.SaveChanges();
        }

        #endregion

        #region Supervisor Validate

        public bool GetSupervisorValidate(int incarcerationId) =>
            _context.IncarcerationArrestXref.Count(iax =>
                iax.IncarcerationId == incarcerationId && iax.ReleaseSupervisorCompleteFlag == 0) > 0;

        #endregion

        public int UpdateClearFlag(int incarcerationId)
        {
            Incarceration incarceration = _context.Incarceration.Single(s => s.IncarcerationId == incarcerationId);
            incarceration.ReleaseClearFlag = 1;
            return _context.SaveChanges();
        }
    }
}
