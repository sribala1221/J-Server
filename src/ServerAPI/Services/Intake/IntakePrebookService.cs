﻿using System;
using System.Linq;
using GenerateTables.Models;
using ServerAPI.ViewModels;
using System.Collections.Generic;
using System.Globalization;
using ServerAPI.Utilities;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore.Storage;

namespace ServerAPI.Services
{
    public class IntakePrebookService : IIntakePrebookService
    {
        private readonly AAtims _context;
        private readonly ICommonService _commonService;
        private readonly IPersonService _personService;
        private readonly IInmateService _inmateService;
        private readonly IWizardService _wizardService;
        private readonly int _personnelId;
        private readonly IAtimsHubService _atimsHubService;
        private readonly IInterfaceEngineService _interfaceEngineService;
        // private int supplementalBookingFacility;

        public IntakePrebookService(AAtims context, ICommonService commonService,
            IHttpContextAccessor httpContextAccessor, IPersonService personService,
            IInmateService inmateService, IAtimsHubService atimsHubService,
            IWizardService wizardService, IInterfaceEngineService interfaceEngineService)
        {
            _context = context;
            _commonService = commonService;
            _personnelId = Convert.ToInt32(httpContextAccessor.HttpContext.User.FindFirst("personnelId")?.Value);
            _personService = personService;
            _inmateService = inmateService;
            _atimsHubService = atimsHubService;
            _wizardService = wizardService;
            _interfaceEngineService = interfaceEngineService;
        }

        #region LoadGrid

        #region IntakePrebookCount

        /// <summary>
        /// To get the intake prebook count based on Facility
        /// </summary>
        /// <param name="facilityId"></param>
        /// <param name="inmatePrebookId"></param>
        /// <param name="queueName"></param>
        /// <param name="active"></param>
        /// <returns></returns>
        public PrebookCountVm GetIntakePrebookCount(int facilityId, int inmatePrebookId, string queueName, bool active)
        {
            Dictionary<string, int> result = new Dictionary<string, int>();
            string intakeCountStatus = null;
            IQueryable<InmateDetail> inmateList = _context.Inmate.Where(w => w.FacilityId == facilityId).Select(s =>
                    new InmateDetail
                    {
                        PersonId = s.PersonId,
                        InmateActive = s.InmateActive == 1
                    });
            //for intake review site option
            string intakeReviewOption = _context.SiteOptions.SingleOrDefault(
                                     x => x.SiteOptionsName == SiteOptionsConstants.INTAKEPREBOOKREVIEW)
                                     ?.SiteOptionsValue;
            IQueryable<InmatePrebookVm> intakePrebookDetails = _context.InmatePrebook
                .Where(ip =>
                    ip.FacilityId == facilityId
                    && (!ip.DeleteFlag.HasValue || ip.DeleteFlag == 0)
                    && ip.CompleteFlag == 1)
                .Select(ip => new InmatePrebookVm
                {
                    InmatePrebookId = ip.InmatePrebookId,
                    IncarcerationId = ip.IncarcerationId,
                    FacilityId = ip.FacilityId,
                    PersonId = ip.PersonId,
                    PrebookDate = ip.PrebookDate.Value,
                    CourtCommitFlag = ip.CourtCommitFlag == 1,
                    MedPrescreenStartFlag = ip.MedPrescreenStartFlag == 1,
                    MedPrescreenStatusFlag = (MedPrescreenStatusFlag?)ip.MedPrescreenStatusFlag,
                    MedPrescreenStatusDate = ip.MedPrescreenStatusDate,
                    TemporaryHold = ip.TemporaryHold == 1,
                    TempHoldId = ip.TempHoldId,
                    CompleteFlag = ip.CompleteFlag ?? 0,
                    IntakeReviewAccepted = ip.IntakeReviewAccepted,
                    IntakeReviewDenied = ip.IntakeReviewDenied,
                    IdentificationAccepted = ip.IdentificationAccepted,
                    ActiveInmate = inmateList.Count(c => c.PersonId == ip.PersonId) > 0 && inmateList.FirstOrDefault(f => f.PersonId == ip.PersonId).InmateActive
                });
            //All in progress
            result.Add(IntakePrebookConstants.allInProgress.ToString(), intakePrebookDetails.Count(w =>
                       (!w.IncarcerationId.HasValue || w.IncarcerationId == 0)
                       && (!w.TempHoldId.HasValue || w.TempHoldId == 0)
                       && (!w.CourtCommitFlag || w.CourtCommitFlag && w.MedPrescreenStartFlag)));
            if (queueName == IntakePrebookConstants.allInProgress.ToString())
            {
                intakeCountStatus = intakePrebookDetails.Count(w =>
                w.InmatePrebookId == inmatePrebookId
                && (!w.IncarcerationId.HasValue || w.IncarcerationId == 0)
                && (!w.TempHoldId.HasValue || w.TempHoldId == 0)
                && (!w.CourtCommitFlag || w.CourtCommitFlag && w.MedPrescreenStartFlag)) > 0
                ? IntakePrebookConstants.allInProgress.ToString() : null;
            }
            // prebookReady
            result.Add(IntakePrebookConstants.prebookReady.ToString(), intakePrebookDetails.Count(w =>
                        !w.CourtCommitFlag
                        && (!w.IncarcerationId.HasValue || w.IncarcerationId == 0)
                        && (!w.TempHoldId.HasValue || w.TempHoldId == 0) && !w.MedPrescreenStartFlag
                        && (!w.MedPrescreenStatusFlag.HasValue || w.MedPrescreenStatusFlag == 0)));
            if (intakeCountStatus is null)
            {
                intakeCountStatus = intakePrebookDetails.Count(w =>
                w.InmatePrebookId == inmatePrebookId
                && !w.CourtCommitFlag
                && (!w.IncarcerationId.HasValue || w.IncarcerationId == 0)
                && (!w.TempHoldId.HasValue || w.TempHoldId == 0)
                && !w.MedPrescreenStartFlag
                && (!w.MedPrescreenStatusFlag.HasValue || w.MedPrescreenStatusFlag == 0)) > 0
                ? IntakePrebookConstants.prebookReady.ToString() : null;
            }
            else
            {
                if (queueName == IntakePrebookConstants.prebookReady.ToString())
                {
                    intakeCountStatus = intakePrebookDetails.Count(w =>
                    w.InmatePrebookId == inmatePrebookId
                    && !w.CourtCommitFlag
                    && (!w.IncarcerationId.HasValue || w.IncarcerationId == 0)
                    && (!w.TempHoldId.HasValue || w.TempHoldId == 0)
                    && !w.MedPrescreenStartFlag
                    && (!w.MedPrescreenStatusFlag.HasValue || w.MedPrescreenStatusFlag == 0)) > 0
                ? IntakePrebookConstants.prebookReady.ToString() : intakeCountStatus;
                }
            }
            //inProgress
            result.Add(IntakePrebookConstants.inProgress.ToString(), intakePrebookDetails.Count(w =>
                        w.MedPrescreenStartFlag
                        && (!w.MedPrescreenStatusFlag.HasValue || w.MedPrescreenStatusFlag == 0)
                        && (!w.IncarcerationId.HasValue || w.IncarcerationId == 0)
                        && (!w.TempHoldId.HasValue || w.TempHoldId == 0)));
            if (intakeCountStatus is null)
            {
                intakeCountStatus = intakePrebookDetails.Count(w =>
                w.InmatePrebookId == inmatePrebookId
                && w.MedPrescreenStartFlag
                && (!w.MedPrescreenStatusFlag.HasValue || w.MedPrescreenStatusFlag == 0)
                && (!w.IncarcerationId.HasValue || w.IncarcerationId == 0) && (!w.TempHoldId.HasValue || w.TempHoldId == 0)) > 0
                ? IntakePrebookConstants.inProgress.ToString() : null;
            }
            else
            {
                if (queueName == IntakePrebookConstants.inProgress.ToString())
                {
                    intakeCountStatus = intakePrebookDetails.Count(w =>
                    w.InmatePrebookId == inmatePrebookId
                    && w.MedPrescreenStartFlag
                    && (!w.MedPrescreenStatusFlag.HasValue || w.MedPrescreenStatusFlag == 0)
                    && (!w.IncarcerationId.HasValue || w.IncarcerationId == 0)
                    && (!w.TempHoldId.HasValue || w.TempHoldId == 0)) > 0
                ? IntakePrebookConstants.inProgress.ToString() : intakeCountStatus;
                }
            }
            //byPassed
            result.Add(IntakePrebookConstants.byPassed.ToString(), intakePrebookDetails.Count(w =>
                        w.MedPrescreenStartFlag
                        && w.MedPrescreenStatusFlag == MedPrescreenStatusFlag.Bypass));
            if (intakeCountStatus is null)
            {
                intakeCountStatus = intakePrebookDetails.Count(w =>
                w.InmatePrebookId == inmatePrebookId
                && w.MedPrescreenStartFlag
                && w.MedPrescreenStatusFlag == MedPrescreenStatusFlag.Bypass) > 0
            ? IntakePrebookConstants.byPassed.ToString() : null;
            }
            else
            {
                if (queueName == IntakePrebookConstants.byPassed.ToString())
                {
                    intakeCountStatus = intakePrebookDetails.Count(w =>
                    w.InmatePrebookId == inmatePrebookId
                    && w.MedPrescreenStartFlag
                    && w.MedPrescreenStatusFlag == MedPrescreenStatusFlag.Bypass) > 0
                    ? IntakePrebookConstants.byPassed.ToString() : intakeCountStatus;
                }
            }
            //medicallyRejected
            result.Add(IntakePrebookConstants.medicallyRejected.ToString(), intakePrebookDetails.Count(w =>
                       (!w.IncarcerationId.HasValue || w.IncarcerationId == 0)
                       && (!w.TempHoldId.HasValue || w.TempHoldId == 0)
                       && w.MedPrescreenStartFlag
                       && w.MedPrescreenStatusFlag == MedPrescreenStatusFlag.Reject
                       && w.MedPrescreenStatusDate.HasValue
                       && w.MedPrescreenStatusDate.Value.Date.AddDays(-2) < w.MedPrescreenStatusDate.Value.Date
                       && w.MedPrescreenStatusDate.Value.Date <= DateTime.Now.Date));
            if (intakeCountStatus is null)
            {
                intakeCountStatus = intakePrebookDetails.Count(w =>
                w.InmatePrebookId == inmatePrebookId
                && (!w.IncarcerationId.HasValue || w.IncarcerationId == 0)
                && (!w.TempHoldId.HasValue || w.TempHoldId == 0)
                && w.MedPrescreenStartFlag
                && w.MedPrescreenStatusFlag == MedPrescreenStatusFlag.Reject
                && w.MedPrescreenStatusDate.HasValue
                && w.MedPrescreenStatusDate.Value.Date.AddDays(-2) < w.MedPrescreenStatusDate.Value.Date
                && w.MedPrescreenStatusDate.Value.Date <= DateTime.Now.Date) > 0
                ? IntakePrebookConstants.medicallyRejected.ToString() : null;
            }
            else
            {
                if (queueName == IntakePrebookConstants.medicallyRejected.ToString())
                {
                    intakeCountStatus = intakePrebookDetails.Count(w =>
                    w.InmatePrebookId == inmatePrebookId
                    && (!w.IncarcerationId.HasValue || w.IncarcerationId == 0)
                    && (!w.TempHoldId.HasValue || w.TempHoldId == 0)
                    && w.MedPrescreenStartFlag
                    && w.MedPrescreenStatusFlag == MedPrescreenStatusFlag.Reject
                    && w.MedPrescreenStatusDate.HasValue
                    && w.MedPrescreenStatusDate.Value.Date.AddDays(-2) < w.MedPrescreenStatusDate.Value.Date
                    && w.MedPrescreenStatusDate.Value.Date <= DateTime.Now.Date) > 0
                ? IntakePrebookConstants.medicallyRejected.ToString() : intakeCountStatus;
                }
            }
            //identification
            result.Add(IntakePrebookConstants.identification.ToString(), intakePrebookDetails.Count(w =>
                      (!w.IncarcerationId.HasValue || w.IncarcerationId == 0)
                      && (!w.TempHoldId.HasValue || w.TempHoldId == 0)
                      && (!w.CourtCommitFlag || w.CourtCommitFlag && w.MedPrescreenStartFlag)
                      && !w.IdentificationAccepted));
            if (intakeCountStatus is null)
            {
                intakeCountStatus = intakePrebookDetails.Count(w => w.InmatePrebookId == inmatePrebookId
                && (!w.IncarcerationId.HasValue || w.IncarcerationId == 0)
                && (!w.TempHoldId.HasValue || w.TempHoldId == 0)
                && (!w.CourtCommitFlag || w.CourtCommitFlag && w.MedPrescreenStartFlag)
                && !w.IdentificationAccepted) > 0
            ? IntakePrebookConstants.identification.ToString() : null;
            }
            else
            {
                if (queueName == IntakePrebookConstants.identification.ToString())
                {
                    intakeCountStatus = intakePrebookDetails.Count(w =>
                    w.InmatePrebookId == inmatePrebookId
                    && (!w.IncarcerationId.HasValue || w.IncarcerationId == 0)
                    && (!w.TempHoldId.HasValue || w.TempHoldId == 0)
                    && (!w.CourtCommitFlag || w.CourtCommitFlag && w.MedPrescreenStartFlag)
                    && !w.IdentificationAccepted) > 0
                ? IntakePrebookConstants.identification.ToString() : intakeCountStatus;
                }
            }
            if (intakeReviewOption == SiteOptionsConstants.ON)
            {
                //notReviewed
                result.Add(IntakePrebookConstants.notReviewed.ToString(), intakePrebookDetails.Count(w =>
                         (!w.IncarcerationId.HasValue || w.IncarcerationId == 0)
                         && (!w.TempHoldId.HasValue || w.TempHoldId == 0)
                         && !w.IntakeReviewAccepted
                         && !w.IntakeReviewDenied
                         && (!w.CourtCommitFlag || w.CourtCommitFlag && w.MedPrescreenStartFlag)));
                if (intakeCountStatus is null)
                {
                    intakeCountStatus = intakePrebookDetails.Count(w =>
                    w.InmatePrebookId == inmatePrebookId
                    && (!w.IncarcerationId.HasValue || w.IncarcerationId == 0)
                    && (!w.TempHoldId.HasValue || w.TempHoldId == 0)
                    && !w.IntakeReviewAccepted && !w.IntakeReviewDenied
                    && (!w.CourtCommitFlag || w.CourtCommitFlag && w.MedPrescreenStartFlag)) > 0
                            ? IntakePrebookConstants.notReviewed.ToString() : null;
                }
                else
                {
                    if (queueName == IntakePrebookConstants.notReviewed.ToString())
                    {
                        intakeCountStatus = intakePrebookDetails.Count(w =>
                          w.InmatePrebookId == inmatePrebookId
                          && (!w.IncarcerationId.HasValue || w.IncarcerationId == 0)
                          && (!w.TempHoldId.HasValue || w.TempHoldId == 0)
                          && !w.IntakeReviewAccepted
                          && !w.IntakeReviewDenied
                          && (!w.CourtCommitFlag || w.CourtCommitFlag && w.MedPrescreenStartFlag)) > 0
                            ? IntakePrebookConstants.notReviewed.ToString() : intakeCountStatus;
                    }
                }
                //deniedReviewAgain
                result.Add(IntakePrebookConstants.deniedReviewAgain.ToString(), intakePrebookDetails.Count(w =>
                        (!w.IncarcerationId.HasValue || w.IncarcerationId == 0)
                        && (!w.TempHoldId.HasValue || w.TempHoldId == 0)
                        && !w.IntakeReviewAccepted
                        && w.IntakeReviewDenied
                        && (!w.CourtCommitFlag || w.CourtCommitFlag && w.MedPrescreenStartFlag)));
                if (intakeCountStatus is null)
                {
                    intakeCountStatus = intakePrebookDetails.Count(w =>
                    w.InmatePrebookId == inmatePrebookId
                    && (!w.IncarcerationId.HasValue || w.IncarcerationId == 0)
                    && (!w.TempHoldId.HasValue || w.TempHoldId == 0)
                    && !w.IntakeReviewAccepted
                    && w.IntakeReviewDenied
                    && (!w.CourtCommitFlag || w.CourtCommitFlag && w.MedPrescreenStartFlag)) > 0
                        ? IntakePrebookConstants.deniedReviewAgain.ToString() : null;
                }
                else
                {
                    if (queueName == IntakePrebookConstants.deniedReviewAgain.ToString())
                    {
                        intakeCountStatus = intakePrebookDetails.Count(w =>
                          w.InmatePrebookId == inmatePrebookId
                          && (!w.IncarcerationId.HasValue || w.IncarcerationId == 0)
                          && (!w.TempHoldId.HasValue || w.TempHoldId == 0)
                          && !w.IntakeReviewAccepted && w.IntakeReviewDenied
                          && (!w.CourtCommitFlag || w.CourtCommitFlag && w.MedPrescreenStartFlag)) > 0
                        ? IntakePrebookConstants.deniedReviewAgain.ToString() : intakeCountStatus;
                    }
                }
            }
            //intake
            result.Add(IntakePrebookConstants.intake.ToString(),
            intakePrebookDetails.Count(w => (
               !w.IncarcerationId.HasValue || w.IncarcerationId == 0)
               && (!w.TempHoldId.HasValue || w.TempHoldId == 0)
               && w.IdentificationAccepted
               && (intakeReviewOption != SiteOptionsConstants.ON || w.IntakeReviewAccepted)
               && w.MedPrescreenStartFlag
               && (w.MedPrescreenStatusFlag == MedPrescreenStatusFlag.Accept
               || w.MedPrescreenStatusFlag == MedPrescreenStatusFlag.Bypass || w.MedPrescreenStatusFlag == MedPrescreenStatusFlag.NotRequired)
               && !w.TemporaryHold));

            if (intakeCountStatus is null)
            {
                intakeCountStatus =
                    intakePrebookDetails.Count(w =>
                   w.InmatePrebookId == inmatePrebookId
                   && (!w.IncarcerationId.HasValue || w.IncarcerationId == 0)
                   && (!w.TempHoldId.HasValue || w.TempHoldId == 0)
                   && w.IdentificationAccepted
                   && (intakeReviewOption != SiteOptionsConstants.ON || w.IntakeReviewAccepted)
                   && w.MedPrescreenStartFlag
                   && (w.MedPrescreenStatusFlag == MedPrescreenStatusFlag.Accept || w.MedPrescreenStatusFlag == MedPrescreenStatusFlag.Bypass
                   || w.MedPrescreenStatusFlag == MedPrescreenStatusFlag.NotRequired) && !w.TemporaryHold) > 0
                        ? IntakePrebookConstants.intake.ToString() : null;
            }
            else
            {
                if (queueName == IntakePrebookConstants.intake.ToString())
                {
                    intakeCountStatus =
                    intakePrebookDetails.Count(w =>
                    w.InmatePrebookId == inmatePrebookId
                    && (!w.IncarcerationId.HasValue || w.IncarcerationId == 0)
                    && (!w.TempHoldId.HasValue || w.TempHoldId == 0)
                    && w.IdentificationAccepted
                    && (intakeReviewOption != SiteOptionsConstants.ON || w.IntakeReviewAccepted)
                    && w.MedPrescreenStartFlag
                    && (w.MedPrescreenStatusFlag == MedPrescreenStatusFlag.Accept
                    || w.MedPrescreenStatusFlag == MedPrescreenStatusFlag.Bypass || w.MedPrescreenStatusFlag == MedPrescreenStatusFlag.NotRequired)
                    && !w.TemporaryHold) > 0
                        ? IntakePrebookConstants.intake.ToString() : intakeCountStatus;
                }
            }
            //temphold
            result.Add(IntakePrebookConstants.tempHold.ToString(),
            intakePrebookDetails.Count(
                    w =>
                    (!w.IncarcerationId.HasValue || w.IncarcerationId == 0)
                    && (!w.TempHoldId.HasValue || w.TempHoldId == 0)
                    && w.IdentificationAccepted
                    && (intakeReviewOption != SiteOptionsConstants.ON || w.IntakeReviewAccepted)
                    && w.MedPrescreenStartFlag
                    && (w.MedPrescreenStatusFlag == MedPrescreenStatusFlag.Accept || w.MedPrescreenStatusFlag == MedPrescreenStatusFlag.Bypass
                    || w.MedPrescreenStatusFlag == MedPrescreenStatusFlag.NotRequired)
                    && w.TemporaryHold
                    ));

            if (intakeCountStatus is null)
            {
                intakeCountStatus =
                    intakePrebookDetails.Count(w =>
                   w.InmatePrebookId == inmatePrebookId
                   && (!w.IncarcerationId.HasValue || w.IncarcerationId == 0)
                   && (!w.TempHoldId.HasValue || w.TempHoldId == 0)
                   && w.IdentificationAccepted
                   && (intakeReviewOption != SiteOptionsConstants.ON || w.IntakeReviewAccepted)
                   && w.MedPrescreenStartFlag
                   && (w.MedPrescreenStatusFlag == MedPrescreenStatusFlag.Accept || w.MedPrescreenStatusFlag == MedPrescreenStatusFlag.Bypass
                   || w.MedPrescreenStatusFlag == MedPrescreenStatusFlag.NotRequired)
                   && w.TemporaryHold) > 0
                        ? IntakePrebookConstants.tempHold.ToString() : null;
            }
            else
            {
                if (queueName == IntakePrebookConstants.tempHold.ToString())
                {
                    intakeCountStatus =
                    intakePrebookDetails.Count(w => w.InmatePrebookId == inmatePrebookId
                        && (!w.IncarcerationId.HasValue || w.IncarcerationId == 0) && (!w.TempHoldId.HasValue || w.TempHoldId == 0)
                        && w.IdentificationAccepted
                        && (intakeReviewOption != SiteOptionsConstants.ON || w.IntakeReviewAccepted)
                        && w.MedPrescreenStartFlag && (w.MedPrescreenStatusFlag == MedPrescreenStatusFlag.Accept
                        || w.MedPrescreenStatusFlag == MedPrescreenStatusFlag.Bypass || w.MedPrescreenStatusFlag == MedPrescreenStatusFlag.NotRequired)
                        && w.TemporaryHold) > 0
                        ? IntakePrebookConstants.tempHold.ToString() : intakeCountStatus;
                }
            }
            //courtCommitToday
            result.Add(IntakePrebookConstants.courtCommitTdy.ToString(), intakePrebookDetails.Count(w =>
                       w.CourtCommitFlag
                       && (!w.IncarcerationId.HasValue || w.IncarcerationId == 0)
                       && (!w.TempHoldId.HasValue || w.TempHoldId == 0)
                       && w.PrebookDate.HasValue
                       && w.PrebookDate.Value.Date == DateTime.Now.Date
                       && (!active || w.ActiveInmate)));
            if (intakeCountStatus is null)
            {
                intakeCountStatus = intakePrebookDetails.Count(w =>
                w.InmatePrebookId == inmatePrebookId
                && w.CourtCommitFlag
                && (!w.IncarcerationId.HasValue || w.IncarcerationId == 0)
                && (!w.TempHoldId.HasValue || w.TempHoldId == 0)
                && w.PrebookDate.HasValue && w.PrebookDate.Value.Date == DateTime.Now.Date
                && (!active || w.ActiveInmate)) > 0
                ? IntakePrebookConstants.courtCommitTdy.ToString() : null;
            }
            else
            {
                if (queueName == IntakePrebookConstants.courtCommitTdy.ToString())
                {
                    intakeCountStatus = intakePrebookDetails.Count(w =>
                    w.InmatePrebookId == inmatePrebookId && w.CourtCommitFlag
                    && (!w.IncarcerationId.HasValue || w.IncarcerationId == 0)
                    && (!w.TempHoldId.HasValue || w.TempHoldId == 0)
                    && w.PrebookDate.HasValue
                    && w.PrebookDate.Value.Date == DateTime.Now.Date
                    && (!active || w.ActiveInmate)) > 0
                ? IntakePrebookConstants.courtCommitTdy.ToString() : intakeCountStatus;
                }
            }
            //courtCommitSchd
            result.Add(IntakePrebookConstants.courtCommitSchd.ToString(),
                intakePrebookDetails.Count(w =>
                    w.CourtCommitFlag
                    && (!w.IncarcerationId.HasValue || w.IncarcerationId == 0)
                    && (!w.TempHoldId.HasValue || w.TempHoldId == 0)
                    && w.PrebookDate.HasValue
                    && w.PrebookDate.Value.Date >= DateTime.Now.Date
                    && (!active || w.ActiveInmate)));
            List<InmatePrebookVm> commitschList = intakePrebookDetails.Where(
                w => w.CourtCommitFlag
                     && (!w.IncarcerationId.HasValue || w.IncarcerationId == 0)
                     && (!w.TempHoldId.HasValue || w.TempHoldId == 0)
                     && w.PrebookDate.HasValue
                     && w.PrebookDate.Value.Date >= DateTime.Now.Date
                     && (!active || w.ActiveInmate)
            ).AsEnumerable().GroupBy(g => g.PrebookDate.Value.Date).Select(s => new InmatePrebookVm
            {
                PrebookDate = s.Key,
                InmateCount = s.Count(),
                CcQueueName = IntakePrebookConstants.courtCommitSchd.ToString()
            }).OrderBy(o => o.PrebookDate).ToList();
            if (intakeCountStatus is null)
            {
                intakeCountStatus = intakePrebookDetails.Count(w =>
                w.InmatePrebookId == inmatePrebookId
                && w.CourtCommitFlag
                && (!w.IncarcerationId.HasValue || w.IncarcerationId == 0)
                && (!w.TempHoldId.HasValue || w.TempHoldId == 0)
                && w.PrebookDate.HasValue
                && w.PrebookDate.Value.Date >= DateTime.Now.Date
                && (!active || w.ActiveInmate)) > 0
                ? IntakePrebookConstants.courtCommitSchd.ToString() : null;
            }
            else
            {
                if (queueName == IntakePrebookConstants.courtCommitSchd.ToString())
                {
                    intakeCountStatus = intakePrebookDetails.Count(w =>
                    w.InmatePrebookId == inmatePrebookId
                    && w.CourtCommitFlag
                    && (!w.IncarcerationId.HasValue || w.IncarcerationId == 0)
                    && (!w.TempHoldId.HasValue || w.TempHoldId == 0)
                    && w.PrebookDate.HasValue
                    && w.PrebookDate.Value.Date >= DateTime.Now.Date
                    && (!active || w.ActiveInmate)) > 0
                ? IntakePrebookConstants.courtCommitSchd.ToString() : intakeCountStatus;
                }
            }
            // courtCommitOverdue
            result.Add(IntakePrebookConstants.courtCommitOverdue.ToString(),
                intakePrebookDetails.Count(w =>
                      w.CourtCommitFlag && (!w.IncarcerationId.HasValue || w.IncarcerationId == 0)
                      && (!w.TempHoldId.HasValue || w.TempHoldId == 0) && w.PrebookDate.HasValue
                      && w.PrebookDate.Value.Date < DateTime.Now.Date &&
                      (!active || w.ActiveInmate)));
            List<InmatePrebookVm> commitoverdueList = intakePrebookDetails.Where(w =>
                w.CourtCommitFlag
                && (!w.IncarcerationId.HasValue || w.IncarcerationId == 0)
                && (!w.TempHoldId.HasValue || w.TempHoldId == 0)
                && w.PrebookDate.HasValue && w.PrebookDate.Value.Date < DateTime.Now.Date
                && (!active || w.ActiveInmate)
            ).AsEnumerable().GroupBy(g => g.PrebookDate.Value.Date).Select(s => new InmatePrebookVm
            {
                PrebookDate = s.Key,
                InmateCount = s.Count(),
                CcQueueName = IntakePrebookConstants.courtCommitOverdue.ToString()
            }).OrderBy(o => o.PrebookDate).ToList();
            if (intakeCountStatus is null)
            {
                intakeCountStatus = intakePrebookDetails.Count(w =>
                w.InmatePrebookId == inmatePrebookId
                && w.CourtCommitFlag
                && (!w.IncarcerationId.HasValue || w.IncarcerationId == 0)
                && (!w.TempHoldId.HasValue || w.TempHoldId == 0)
                && w.PrebookDate.HasValue
                && w.PrebookDate.Value.Date < DateTime.Now.Date
                && (!active || w.ActiveInmate)) > 0
                ? IntakePrebookConstants.courtCommitOverdue.ToString()
                : null;
            }
            else
            {
                if (queueName == IntakePrebookConstants.courtCommitOverdue.ToString())
                {
                    intakeCountStatus = intakePrebookDetails.Count(w =>
                    w.InmatePrebookId == inmatePrebookId
                    && w.CourtCommitFlag
                    && (!w.IncarcerationId.HasValue || w.IncarcerationId == 0)
                    && (!w.TempHoldId.HasValue || w.TempHoldId == 0)
                    && w.PrebookDate.HasValue
                    && w.PrebookDate.Value.Date < DateTime.Now.Date
                    && (!active || w.ActiveInmate)) > 0
                ? IntakePrebookConstants.courtCommitOverdue.ToString() : intakeCountStatus;
                }
            }
            PrebookCountVm prebookCountVm = new PrebookCountVm
            {
                PrebookCounts = result,
                PrebookDetails = intakeCountStatus ?? IntakePrebookConstants.allInProgress.ToString(),
                IntakeReviewOption = intakeReviewOption,
                CommitoverdueList = commitoverdueList,
                CommitschList = commitschList
            };
            return prebookCountVm;
        }

        #endregion

        #region Prebook Grid Loads

        public List<InmatePrebookVm> GetListPrebookDetails(int facilityId, PrebookDetails prebookSelected, string courtcommitoverdueDate)
        {
            List<InmatePrebookVm> lstIntakePrebookDetails = new List<InmatePrebookVm>();
            string intakeReviewOption = _context.SiteOptions.SingleOrDefault(
                    x => x.SiteOptionsName == SiteOptionsConstants.INTAKEPREBOOKREVIEW)
                ?.SiteOptionsValue;
            List<PersonnelVm> personnel = _context.Personnel.Select(s => new PersonnelVm
            {
                PersonnelId = s.PersonnelId,
                PersonLastName = s.PersonNavigation.PersonLastName,
                PersonFirstName = s.PersonNavigation.PersonFirstName,
                OfficerBadgeNumber = s.OfficerBadgeNum

            }).ToList();
            string facilityAbbr = _context.Facility.SingleOrDefault(w => w.FacilityId == facilityId)?.FacilityAbbr;
            List<KeyValuePair<int, string>> agency = _context.Agency.Select(s => new KeyValuePair<int, string>(s.AgencyId, s.AgencyAbbreviation)).ToList();
            IQueryable<InmateDetail> inmateList = _context.Inmate.Where(w => w.FacilityId == facilityId).Select(s =>
                    new InmateDetail
                    {
                        InmateId = s.InmateId,
                        PersonId = s.PersonId,
                        InmateNumber = s.InmateNumber,
                        InmateActive = s.InmateActive == 1
                    });
            List<AoWizardProgressInmatePrebookVm> wizardProgressInmatePrebook = _context.AoWizardProgressInmatePrebook.Select(s =>
                  new AoWizardProgressInmatePrebookVm
                  {
                      InmatePrebookId = s.InmatePrebookId,
                      WizardProgressId = s.AoWizardProgressId,
                      WizardStepProgress = s.AoWizardStepProgress.Select(w =>
                          new AoWizardStepProgressVm
                          {
                              ComponentId = w.AoComponentId,
                              StepComplete = w.StepComplete
                          }).ToList()
                  }).ToList();

            List<KeyValuePair<int, double>> lstInmatePrebookCase = _context.InmatePrebookCase
                .Where(inmPre => !inmPre.DeleteFlag && inmPre.ArrestType.HasValue)
                .Select(look => new KeyValuePair<int, double>(look.InmatePrebookId, (double)look.ArrestType))
                .ToList();

            if (prebookSelected == PrebookDetails.PrebookReady || prebookSelected == PrebookDetails.CourtCommitSch ||
                prebookSelected == PrebookDetails.CourtCommitOverdue || prebookSelected == PrebookDetails.CourtCommitSchToday ||
                prebookSelected == PrebookDetails.AllInProgress)
            {
                List<InmatePrebookVm> lstintakePrebookReady = _context.InmatePrebook.Where(ip =>
                    ip.FacilityId == facilityId && (!ip.DeleteFlag.HasValue || ip.DeleteFlag == 0)
                    && ip.CompleteFlag == 1 && (!ip.IncarcerationId.HasValue || ip.IncarcerationId == 0)
                    && (!ip.TempHoldId.HasValue || ip.TempHoldId == 0))
                    .Select(ip => new InmatePrebookVm
                    {
                        InmatePrebookId = ip.InmatePrebookId,
                        PersonId = ip.PersonId,
                        PersonLastName = ip.PersonLastName,
                        PersonFirstName = ip.PersonFirstName,
                        PersonMiddleName = ip.PersonMiddleName,
                        PersonDob = ip.PersonDob,
                        PersonSuffix = ip.PersonSuffix,
                        ArrestDate = ip.ArrestDate,
                        ArrestLocation = ip.ArrestLocation,
                        PrebookDate = ip.PrebookDate,
                        PrebookNumber = ip.PreBookNumber,
                        ArrestType = ip.ArrestType,
                        ArrestingOfficer = personnel.Find(elm => elm.PersonnelId == ip.ArrestingOfficerId),
                        ArrestAgencyAbbr = agency.Find(elm => elm.Key == ip.ArrestAgencyId).Value,
                        FacilityAbbr = facilityAbbr,
                        CaseNumber = string.Join(",", ip.InmatePrebookCase
                            .Where(ipc => ipc.InmatePrebookId == ip.InmatePrebookId && !ipc.DeleteFlag)
                            .Select(ipc => ipc.CaseNumber)),
                        PrebookNotes = ip.PrebookNotes,
                        TemporaryHold = ip.TemporaryHold == 1,
                        MedPrescreenStartFlag = ip.MedPrescreenStartFlag == 1,
                        MedPrescreenStatusFlag = (MedPrescreenStatusFlag?)ip.MedPrescreenStatusFlag,
                        IncarcerationId = ip.IncarcerationId,
                        TempHoldId = ip.TempHoldId,
                        CourtCommitFlag = ip.CourtCommitFlag == 1,
                        MedPrescreenStatusDate = ip.MedPrescreenStatusDate,
                        ArrestCourtDocket = ip.ArrestCourtDocket,
                        MedPrescreenStatusNote = ip.MedPrescreenStatusNote,
                        WizardProgressId = wizardProgressInmatePrebook.Find(s => s.InmatePrebookId == ip.InmatePrebookId) == null
                            ? (int?)null : wizardProgressInmatePrebook.Find(s => s.InmatePrebookId == ip.InmatePrebookId).WizardProgressId,
                        WizardStepProgress = wizardProgressInmatePrebook.Find(s => s.InmatePrebookId == ip.InmatePrebookId) == null
                            ? null : wizardProgressInmatePrebook.Find(s => s.InmatePrebookId == ip.InmatePrebookId).WizardStepProgress,
                        IntakeReviewAccepted = ip.IntakeReviewAccepted,
                        IntakeReviewDenied = ip.IntakeReviewDenied,
                        IdentificationAccepted = ip.IdentificationAccepted,
                        IdentifiedPerson = new PersonVm
                        {
                            PersonLastName = ip.PersonId > 0 ? ip.Person.PersonLastName : default,
                            PersonFirstName = ip.PersonId > 0 ? ip.Person.PersonFirstName : default,
                            PersonMiddleName = ip.PersonId > 0 ? ip.Person.PersonMiddleName : default,
                            PersonSuffix = ip.PersonId > 0 ? ip.Person.PersonSuffix : default,
                            PersonDob = ip.PersonId > 0 ? ip.Person.PersonDob : default
                        }
                    }).OrderBy(o => o.PrebookDate).ToList();
                lstintakePrebookReady.ForEach(pr =>
                    {
                        pr.CaseType = lstInmatePrebookCase.Where(iax => iax.Key == pr.InmatePrebookId)
                            .Select(iax => iax.Value).ToArray();
                        pr.InmateDetail = (from s in _context.Inmate
                                           where s.PersonId == pr.PersonId
                                           select new InmateDetail
                                           {
                                               InmateId = s.InmateId,
                                               PersonId = s.Person.PersonId,
                                               InmateNumber = s.InmateNumber,
                                               InmateActive = s.InmateActive == 1
                                           }).SingleOrDefault();
                    }
                );

                switch (prebookSelected)
                {
                    case PrebookDetails.PrebookReady:
                        lstIntakePrebookDetails.AddRange(lstintakePrebookReady.Where(s =>
                        !s.CourtCommitFlag && !s.MedPrescreenStartFlag &&
                         (!s.MedPrescreenStatusFlag.HasValue || s.MedPrescreenStatusFlag == 0)));
                        break;
                    case PrebookDetails.CourtCommitSch:
                        lstIntakePrebookDetails.AddRange(lstintakePrebookReady.Where(s =>
                        s.CourtCommitFlag && s.PrebookDate.HasValue && s.PrebookDate.Value.Date >= DateTime.Now.Date &&
                        s.PrebookDate.Value.Date == Convert.ToDateTime(courtcommitoverdueDate)));
                        break;
                    case PrebookDetails.CourtCommitOverdue:
                        lstIntakePrebookDetails.AddRange(lstintakePrebookReady.Where(s =>
                        s.CourtCommitFlag && s.PrebookDate.HasValue && s.PrebookDate.Value.Date < DateTime.Now.Date &&
                        s.PrebookDate.Value.Date == Convert.ToDateTime(courtcommitoverdueDate)));
                        break;
                    case PrebookDetails.CourtCommitSchToday:
                        lstIntakePrebookDetails.AddRange(lstintakePrebookReady.Where(s =>
                        s.CourtCommitFlag && s.PrebookDate.HasValue && s.PrebookDate.Value.Date == DateTime.Now.Date));
                        break;
                    case PrebookDetails.AllInProgress:
                        lstIntakePrebookDetails.AddRange(lstintakePrebookReady.Where(s =>
                        !s.CourtCommitFlag || s.CourtCommitFlag && s.MedPrescreenStartFlag));
                        break;
                }
            }
            if (prebookSelected == PrebookDetails.MedicallyRejected || prebookSelected == PrebookDetails.InProgress)
            {
                //For getting "Medically Rejected" & "Prescreen In Progress".
                IQueryable<InmatePrebookVm> lstintakePrebookInprogress = _context.InmatePrebook.Where(ip =>
                        ip.FacilityId == facilityId
                        && (!ip.DeleteFlag.HasValue || ip.DeleteFlag == 0)
                        && ip.CompleteFlag == 1
                        && (!ip.IncarcerationId.HasValue || ip.IncarcerationId == 0)
                        && (!ip.TempHoldId.HasValue || ip.TempHoldId == 0)
                        && ip.MedPrescreenStartFlag == 1)
                    .Select(ip => new InmatePrebookVm
                    {
                        InmatePrebookId = ip.InmatePrebookId,
                        PersonId = ip.PersonId,
                        PersonLastName = ip.PersonLastName,
                        PersonFirstName = ip.PersonFirstName,
                        PersonMiddleName = ip.PersonMiddleName,
                        ArrestType = ip.ArrestType,
                        CaseType = lstInmatePrebookCase.Where(
                            iax => iax.Key == ip.InmatePrebookId).Select(iax => iax.Value).ToArray(),
                        PersonDob = ip.PersonDob,
                        PersonSuffix = ip.PersonSuffix,
                        ArrestDate = ip.ArrestDate,
                        ArrestLocation = ip.ArrestLocation,
                        PrebookDate = ip.PrebookDate,
                        PrebookNumber = ip.PreBookNumber,
                        ArrestingOfficer = personnel.SingleOrDefault(p => p.PersonnelId == ip.ArrestingOfficerId),
                        ArrestAgencyAbbr = agency.SingleOrDefault(a => a.Key == ip.ArrestAgencyId).Value,
                        FacilityAbbr = facilityAbbr,
                        CaseNumber = string.Join(",", ip.InmatePrebookCase
                        .Where(ipc => ipc.InmatePrebookId == ip.InmatePrebookId && !ipc.DeleteFlag)
                                    .Select(ipc => ipc.CaseNumber)),
                        PrebookNotes = ip.PrebookNotes,
                        TemporaryHold = ip.TemporaryHold == 1,
                        MedPrescreenStartFlag = ip.MedPrescreenStartFlag == 1,
                        MedPrescreenStatusFlag = (MedPrescreenStatusFlag?)ip.MedPrescreenStatusFlag,
                        IncarcerationId = ip.IncarcerationId,
                        TempHoldId = ip.TempHoldId,
                        CourtCommitFlag = ip.CourtCommitFlag == 1,
                        MedPrescreenStatusDate = ip.MedPrescreenStatusDate,
                        ArrestCourtDocket = ip.ArrestCourtDocket,
                        InmateDetail = inmateList.SingleOrDefault(s => s.PersonId == ip.PersonId),
                        MedPrescreenStatusNote = ip.MedPrescreenStatusNote,
                        WizardProgressId = wizardProgressInmatePrebook.FirstOrDefault(s => s.InmatePrebookId == ip.InmatePrebookId).WizardProgressId,
                        WizardStepProgress = wizardProgressInmatePrebook.FirstOrDefault(s => s.InmatePrebookId == ip.InmatePrebookId).WizardStepProgress,
                        IntakeReviewAccepted = ip.IntakeReviewAccepted,
                        IntakeReviewDenied = ip.IntakeReviewDenied,
                        IdentificationAccepted = ip.IdentificationAccepted,
                        IdentifiedPerson = new PersonVm
                        {
                            PersonLastName = ip.PersonId > 0 ? ip.Person.PersonLastName : default,
                            PersonFirstName = ip.PersonId > 0 ? ip.Person.PersonFirstName : default,
                            PersonMiddleName = ip.PersonId > 0 ? ip.Person.PersonMiddleName : default,
                            PersonSuffix = ip.PersonId > 0 ? ip.Person.PersonSuffix : default,
                            PersonDob = ip.PersonId > 0 ? ip.Person.PersonDob : default
                        }
                    }).OrderBy(o => o.PrebookDate);
                switch (prebookSelected)
                {
                    case PrebookDetails.MedicallyRejected:
                        lstIntakePrebookDetails.AddRange(lstintakePrebookInprogress.Where(w =>
                        w.MedPrescreenStatusDate.HasValue
                        && w.MedPrescreenStatusFlag == MedPrescreenStatusFlag.Reject
                        && w.MedPrescreenStatusDate.Value.Date.AddDays(-2) < w.MedPrescreenStatusDate.Value.Date
                        && w.MedPrescreenStatusDate.Value.Date <= DateTime.Now.Date));
                        break;
                    case PrebookDetails.InProgress:
                        lstIntakePrebookDetails.AddRange(lstintakePrebookInprogress.Where(s =>
                        !s.MedPrescreenStatusFlag.HasValue
                        || s.MedPrescreenStatusFlag == 0));
                        break;
                }
            }
            if (prebookSelected == PrebookDetails.ByPassed)
            {
                //For getting "bypassed".
                IQueryable<InmatePrebookVm> lstintakePrebookBypassed = (from ip in _context.InmatePrebook
                                                                        where ip.FacilityId == facilityId
                                                                              && (!ip.DeleteFlag.HasValue || ip.DeleteFlag == 0)
                                                                              && ip.CompleteFlag == 1
                                                                              && ip.MedPrescreenStartFlag == 1
                                                                              && ip.MedPrescreenStatusFlag == (int?)MedPrescreenStatusFlag.Bypass
                                                                        select new InmatePrebookVm
                                                                        {
                                                                            InmatePrebookId = ip.InmatePrebookId,
                                                                            PersonId = ip.PersonId,
                                                                            PersonLastName = ip.PersonLastName,
                                                                            PersonFirstName = ip.PersonFirstName,
                                                                            PersonMiddleName = ip.PersonMiddleName,
                                                                            PersonDob = ip.PersonDob,
                                                                            PersonSuffix = ip.PersonSuffix,
                                                                            ArrestType = ip.ArrestType,
                                                                            CaseType = lstInmatePrebookCase.Where(
                                                                                iax => iax.Key == ip.InmatePrebookId).Select(iax => iax.Value).ToArray(),
                                                                            ArrestDate = ip.ArrestDate,
                                                                            ArrestLocation = ip.ArrestLocation,
                                                                            PrebookDate = ip.PrebookDate,
                                                                            PrebookNumber = ip.PreBookNumber,
                                                                            ArrestingOfficer = personnel.SingleOrDefault(p => p.PersonnelId == ip.ArrestingOfficerId),
                                                                            ArrestAgencyAbbr = agency.Where(a => a.Key == ip.ArrestAgencyId)
                                                                            .Select(a => a.Value).SingleOrDefault(),
                                                                            FacilityAbbr = facilityAbbr,
                                                                            CaseNumber = string.Join(",", ip.InmatePrebookCase
                                                                            .Where(ipc => ipc.InmatePrebookId == ip.InmatePrebookId && !ipc.DeleteFlag)
                                                                                        .Select(ipc => ipc.CaseNumber)),
                                                                            PrebookNotes = ip.PrebookNotes,
                                                                            TemporaryHold = ip.TemporaryHold == 1,
                                                                            MedPrescreenStartFlag = ip.MedPrescreenStartFlag == 1,
                                                                            MedPrescreenStatusFlag = (MedPrescreenStatusFlag?)ip.MedPrescreenStatusFlag,
                                                                            IncarcerationId = ip.IncarcerationId,
                                                                            TempHoldId = ip.TempHoldId,
                                                                            CourtCommitFlag = ip.CourtCommitFlag == 1,
                                                                            MedPrescreenStatusDate = ip.MedPrescreenStatusDate,
                                                                            ArrestCourtDocket = ip.ArrestCourtDocket,
                                                                            InmateDetail = inmateList.FirstOrDefault(s => s.PersonId == ip.PersonId),
                                                                            MedPrescreenStatusNote = ip.MedPrescreenStatusNote,
                                                                            WizardProgressId = wizardProgressInmatePrebook
                                                                            .Where(s => s.InmatePrebookId == ip.InmatePrebookId)
                                                                            .Select(s => s.WizardProgressId).FirstOrDefault(),
                                                                            WizardStepProgress = wizardProgressInmatePrebook
                                                                            .Where(s => s.InmatePrebookId == ip.InmatePrebookId)
                                                                            .Select(s => s.WizardStepProgress).FirstOrDefault(),
                                                                            IntakeReviewAccepted = ip.IntakeReviewAccepted,
                                                                            IntakeReviewDenied = ip.IntakeReviewDenied,
                                                                            IdentificationAccepted = ip.IdentificationAccepted,
                                                                            IdentifiedPerson = new PersonVm
                                                                            {
                                                                                PersonLastName = ip.PersonId > 0 ? ip.Person.PersonLastName : default,
                                                                                PersonFirstName = ip.PersonId > 0 ? ip.Person.PersonFirstName : default,
                                                                                PersonMiddleName = ip.PersonId > 0 ? ip.Person.PersonMiddleName : default,
                                                                                PersonSuffix = ip.PersonId > 0 ? ip.Person.PersonSuffix : default,
                                                                                PersonDob = ip.PersonId > 0 ? ip.Person.PersonDob : default
                                                                            }
                                                                        }).OrderBy(o => o.PrebookDate);
                if (prebookSelected == PrebookDetails.ByPassed)
                {
                    lstIntakePrebookDetails.AddRange(lstintakePrebookBypassed);
                }
            }
            if (prebookSelected == PrebookDetails.Identification || prebookSelected == PrebookDetails.Intake ||
                prebookSelected == PrebookDetails.TempHold)
            {
                //For getting "Identification" & "Intake" & "Temphold".
                IQueryable<InmatePrebookVm> lstintakePrebookIntake = _context.InmatePrebook.Where(ip =>
                        ip.FacilityId == facilityId
                        && (!ip.DeleteFlag.HasValue || ip.DeleteFlag == 0)
                        && ip.CompleteFlag == 1
                        && (!ip.IncarcerationId.HasValue || ip.IncarcerationId == 0)
                        && (!ip.TempHoldId.HasValue || ip.TempHoldId == 0))
                    .Select(ip => new InmatePrebookVm
                    {
                        InmatePrebookId = ip.InmatePrebookId,
                        PersonId = ip.PersonId,
                        PersonLastName = ip.PersonLastName,
                        PersonFirstName = ip.PersonFirstName,
                        PersonMiddleName = ip.PersonMiddleName,
                        PersonDob = ip.PersonDob,
                        PersonSuffix = ip.PersonSuffix,
                        ArrestDate = ip.ArrestDate,
                        ArrestType = ip.ArrestType,
                        CaseType = lstInmatePrebookCase.Where(
                            iax => iax.Key == ip.InmatePrebookId).Select(iax => iax.Value).ToArray(),
                        ArrestLocation = ip.ArrestLocation,
                        PrebookDate = ip.PrebookDate,
                        PrebookNumber = ip.PreBookNumber,
                        ArrestingOfficer = personnel.SingleOrDefault(p => p.PersonnelId == ip.ArrestingOfficerId),
                        ArrestAgencyAbbr = agency.SingleOrDefault(a => a.Key == ip.ArrestAgencyId).Value,
                        FacilityAbbr = facilityAbbr,
                        CaseNumber = string.Join(",", ip.InmatePrebookCase
                        .Where(ipc => ipc.InmatePrebookId == ip.InmatePrebookId && !ipc.DeleteFlag)
                                    .Select(ipc => ipc.CaseNumber)),
                        PrebookNotes = ip.PrebookNotes,
                        TemporaryHold = ip.TemporaryHold == 1,
                        MedPrescreenStartFlag = ip.MedPrescreenStartFlag == 1,
                        MedPrescreenStatusFlag = (MedPrescreenStatusFlag?)ip.MedPrescreenStatusFlag,
                        IncarcerationId = ip.IncarcerationId,
                        TempHoldId = ip.TempHoldId,
                        CourtCommitFlag = ip.CourtCommitFlag == 1,
                        MedPrescreenStatusDate = ip.MedPrescreenStatusDate,
                        ArrestCourtDocket = ip.ArrestCourtDocket,
                        InmateDetail = inmateList.SingleOrDefault(s => s.PersonId == ip.PersonId),
                        MedPrescreenStatusNote = ip.MedPrescreenStatusNote,
                        WizardProgressId = wizardProgressInmatePrebook.FirstOrDefault(s => s.InmatePrebookId == ip.InmatePrebookId).WizardProgressId,
                        WizardStepProgress = wizardProgressInmatePrebook.FirstOrDefault(s => s.InmatePrebookId == ip.InmatePrebookId).WizardStepProgress,
                        IntakeReviewAccepted = ip.IntakeReviewAccepted,
                        IntakeReviewDenied = ip.IntakeReviewDenied,
                        IdentificationAccepted = ip.IdentificationAccepted,
                        IdentifiedPerson = new PersonVm
                        {
                            PersonLastName = ip.PersonId > 0 ? ip.Person.PersonLastName : default,
                            PersonFirstName = ip.PersonId > 0 ? ip.Person.PersonFirstName : default,
                            PersonMiddleName = ip.PersonId > 0 ? ip.Person.PersonMiddleName : default,
                            PersonSuffix = ip.PersonId > 0 ? ip.Person.PersonSuffix : default,
                            PersonDob = ip.PersonId > 0 ? ip.Person.PersonDob : default
                        }
                    }).OrderBy(o => o.PrebookDate);
                switch (prebookSelected)
                {
                    case PrebookDetails.Identification:
                        lstIntakePrebookDetails.AddRange(
                            lstintakePrebookIntake.Where(s => !s.IdentificationAccepted
                            && (!s.CourtCommitFlag || s.CourtCommitFlag && s.MedPrescreenStartFlag)));
                        break;
                    case PrebookDetails.Intake:
                        lstIntakePrebookDetails.AddRange(
                            lstintakePrebookIntake.Where(s => !s.TemporaryHold
                            && s.IdentificationAccepted
                            && (intakeReviewOption != SiteOptionsConstants.ON || s.IntakeReviewAccepted)
                            && s.MedPrescreenStartFlag
                            && (s.MedPrescreenStatusFlag == MedPrescreenStatusFlag.Accept
                            || s.MedPrescreenStatusFlag == MedPrescreenStatusFlag.Bypass
                            || s.MedPrescreenStatusFlag == MedPrescreenStatusFlag.NotRequired)));
                        break;
                    case PrebookDetails.TempHold:
                        lstIntakePrebookDetails.AddRange(
                            lstintakePrebookIntake.Where(w => w.TemporaryHold
                            && w.IdentificationAccepted
                            && (intakeReviewOption != SiteOptionsConstants.ON || w.IntakeReviewAccepted)
                            && w.MedPrescreenStartFlag
                            && (w.MedPrescreenStatusFlag == MedPrescreenStatusFlag.Accept
                            || w.MedPrescreenStatusFlag == MedPrescreenStatusFlag.Bypass
                            || w.MedPrescreenStatusFlag == MedPrescreenStatusFlag.NotRequired)));
                        break;
                }
            }

            if (prebookSelected != PrebookDetails.NotReviewed && prebookSelected != PrebookDetails.DeniedReviewAgain)
                return lstIntakePrebookDetails;
            //For getting "Not reviewed" and "Denied review again"
            IQueryable<InmatePrebookVm> lstPrebookIntakes = _context.InmatePrebook.Where(ip => ip.FacilityId == facilityId
                      && (!ip.DeleteFlag.HasValue || ip.DeleteFlag == 0)
                      && ip.CompleteFlag == 1
                      && (!ip.IncarcerationId.HasValue || ip.IncarcerationId == 0)
                      && (!ip.TempHoldId.HasValue || ip.TempHoldId == 0)
                      && !ip.IntakeReviewAccepted).Select(ip => new InmatePrebookVm
                      {
                          InmatePrebookId = ip.InmatePrebookId,
                          PersonId = ip.PersonId,
                          PersonLastName = ip.PersonLastName,
                          PersonFirstName = ip.PersonFirstName,
                          PersonMiddleName = ip.PersonMiddleName,
                          PersonDob = ip.PersonDob,
                          PersonSuffix = ip.PersonSuffix,
                          ArrestDate = ip.ArrestDate,
                          ArrestLocation = ip.ArrestLocation,
                          ArrestType = ip.ArrestType,
                          CaseType = lstInmatePrebookCase.Where(
                      iax => iax.Key == ip.InmatePrebookId).Select(iax => iax.Value).ToArray(),
                          PrebookDate = ip.PrebookDate,
                          PrebookNumber = ip.PreBookNumber,
                          ArrestingOfficer = personnel.SingleOrDefault(p => p.PersonnelId == ip.ArrestingOfficerId),
                          ArrestAgencyAbbr = agency.SingleOrDefault(a => a.Key == ip.ArrestAgencyId).Value,
                          FacilityAbbr = facilityAbbr,
                          DeleteFlag = ip.DeleteFlag == 1,
                          TemporaryHold = ip.TemporaryHold == 1,
                          MedPrescreenStartFlag = ip.MedPrescreenStartFlag == 1,
                          MedPrescreenStatusFlag = (MedPrescreenStatusFlag?)ip.MedPrescreenStatusFlag,
                          PrebookComplete = ip.CompleteFlag == 1,
                          CourtCommitFlag = ip.CourtCommitFlag == 1,
                          InmateDetail = inmateList.SingleOrDefault(s => s.PersonId == ip.PersonId),
                          WizardProgressId = wizardProgressInmatePrebook.FirstOrDefault(s => s.InmatePrebookId == ip.InmatePrebookId).WizardProgressId,
                          WizardStepProgress = wizardProgressInmatePrebook.FirstOrDefault(s => s.InmatePrebookId == ip.InmatePrebookId).WizardStepProgress,
                          IntakeReviewAccepted = ip.IntakeReviewAccepted,
                          IntakeReviewDenied = ip.IntakeReviewDenied,
                          IdentificationAccepted = ip.IdentificationAccepted,
                          IdentifiedPerson = new PersonVm
                          {
                              PersonLastName = ip.PersonId > 0 ? ip.Person.PersonLastName : default,
                              PersonFirstName = ip.PersonId > 0 ? ip.Person.PersonFirstName : default,
                              PersonMiddleName = ip.PersonId > 0 ? ip.Person.PersonMiddleName : default,
                              PersonSuffix = ip.PersonId > 0 ? ip.Person.PersonSuffix : default,
                              PersonDob = ip.PersonId > 0 ? ip.Person.PersonDob : default
                          }
                      }).OrderBy(o => o.PrebookDate);
            switch (prebookSelected)
            {
                case PrebookDetails.NotReviewed:
                    lstIntakePrebookDetails.AddRange(lstPrebookIntakes.Where(s =>
                        !s.IntakeReviewDenied
                        && (!s.CourtCommitFlag || s.CourtCommitFlag && s.MedPrescreenStartFlag)
                    ));
                    break;
                case PrebookDetails.DeniedReviewAgain:
                    lstIntakePrebookDetails.AddRange(lstPrebookIntakes.Where(s =>
                        s.IntakeReviewDenied
                        && (!s.CourtCommitFlag || s.CourtCommitFlag && s.MedPrescreenStartFlag)
                    ));
                    break;
            }
            return lstIntakePrebookDetails;
        }

        #endregion


        #endregion

        #region GridFunctionality

        //Set Delete Flag in InmatePrebook
        //TODO Why is it hardcoded?
        public async Task<int> DeleteInmatePrebook(int inmatePrebookId)
        {
            InmatePrebook inmatePrebook =
                _context.InmatePrebook.Single(c => c.InmatePrebookId == inmatePrebookId);
            inmatePrebook.DeleteFlag = 1;
            inmatePrebook.DeleteDate = DateTime.Now;
            inmatePrebook.DeletedBy = _personnelId;
            return await _context.SaveChangesAsync();
        }

        //Remove Delete Flag in InmatePrebook
        public async Task<int> UndoInmatePrebook(int inmatePrebookId)
        {
            InmatePrebook inmatePrebook =
                _context.InmatePrebook.Single(c => c.InmatePrebookId == inmatePrebookId);
            inmatePrebook.DeleteFlag = null;
            inmatePrebook.DeleteDate = null;
            inmatePrebook.DeletedBy = null;
            return await _context.SaveChangesAsync();
        }

        //Set Temporary Hold Flag in InmatePrebook
        public async Task<int> UpdateTemporaryHold(int inmatePrebookId)
        {
            InmatePrebook inmatePrebook =
                _context.InmatePrebook.Single(c => c.InmatePrebookId == inmatePrebookId);
            inmatePrebook.TemporaryHold = 1;
            return await _context.SaveChangesAsync();
        }

        //Remove Temporary Hold Flag in InmatePrebook
        public async Task<int> RemoveTemporaryHold(int inmatePrebookId)
        {
            InmatePrebook inmatePrebook =
                _context.InmatePrebook.Single(c => c.InmatePrebookId == inmatePrebookId);
            inmatePrebook.TemporaryHold = 0;
            return await _context.SaveChangesAsync();
        }

        //Set Intake Prebook Person Name From Person
        public async Task<int> UpdateInmatePrebookPersonDetails(int inmatePrebookId, int personId)
        {
            PersonVm personInfo = _personService.GetPersonDetails(personId);
            InmatePrebook inmatePrebook =
                _context.InmatePrebook.Single(c => c.InmatePrebookId == inmatePrebookId);
            inmatePrebook.PersonLastName = personInfo.PersonLastName;
            inmatePrebook.PersonFirstName = personInfo.PersonFirstName;
            inmatePrebook.PersonMiddleName = personInfo.PersonMiddleName;
            inmatePrebook.PersonDob = personInfo.PersonDob;
            inmatePrebook.PersonSuffix = personInfo.PersonSuffix;
            inmatePrebook.PersonId = null;
            return await _context.SaveChangesAsync();
        }

        //Set Intake Prebook Person medical screen
        public async Task<int> UpdateMedicalPrescreenPrebook(int inmatePrebookId)
        {
            // PersonVm personInfo = _commonService.GetPersonDetailsFromPersonId(personId);
            InmatePrebook inmatePrebook =
                _context.InmatePrebook.Single(c => c.InmatePrebookId == inmatePrebookId);
            inmatePrebook.MedPrescreenStartFlag = 1;
            inmatePrebook.MedPrescreenStatusFlag = null;
            inmatePrebook.MedPrescreenStartDate = DateTime.Now;
            inmatePrebook.MedPrescreenStartBy = _personnelId;
            inmatePrebook.MedPrescreenStatusDate = null;
            inmatePrebook.MedPrescreenStatusBy = null;
            return await _context.SaveChangesAsync();
        }

        //Get Intake Prebook
        public InmatePrebookVm GetInmatePrebookDetails(int inmatePrebookId)
        {
            InmatePrebookVm inmatePrebook = _context.InmatePrebook
                .Where(ip => ip.InmatePrebookId == inmatePrebookId)
                .Select(ip => new InmatePrebookVm
                {
                    InmatePrebookId = ip.InmatePrebookId,
                    PersonId = ip.PersonId ?? 0,
                    PersonLastName = ip.PersonLastName,
                    PersonFirstName = ip.PersonFirstName,
                    PersonMiddleName = ip.PersonMiddleName,
                    PersonSuffix = ip.PersonSuffix,
                    PersonDob = ip.PersonDob,
                    PrebookNumber = ip.PreBookNumber,
                    ArrestDate = ip.ArrestDate,
                    PrebookDate = ip.PrebookDate,
                    CaseNumber = string.Join(",", ip.InmatePrebookCase
                    .Where(ipc => ipc.InmatePrebookId == ip.InmatePrebookId && !ipc.DeleteFlag)
                                .Select(ipc => ipc.CaseNumber)),
                    ContactNumber = ip.PreBookContactNumber,
                    ArrestOfficerId = ip.ArrestingOfficerId,
                    ArrestOfficerName = ip.ArrestOfficerName,
                    TransportOfficerId = ip.TransportingOfficerId,
                    TransportOfficerName = ip.TransportOfficerName,
                    ArrestLocation = ip.ArrestLocation,
                    ArrestAgencyId = ip.ArrestAgencyId,
                    FacilityId = ip.FacilityId,
                    PrebookNotes = ip.PrebookNotes,
                    TemporaryHold = ip.TemporaryHold == 1
                }).Single();
            return inmatePrebook;
        }

        #endregion

        #region NewIntake

        //Creating New Intake
        public async Task<int> InsertIntakeProcess(IntakeEntryVm value)
        {
            string inmateNumber;
            string bookNumber = string.Empty;
            int? arrestID = null;
            Inmate inmate;
            bool supplementalBooking = false;
            List<InmatePrebookCase> inmatePrebookCases = new List<InmatePrebookCase>();
            DateTime? dob = _context.Person.SingleOrDefault(pr => pr.PersonId == value.PersonId)?.PersonDob;

            using (IDbContextTransaction transaction = _context.Database.BeginTransaction())
            {
                if (!_context.Inmate.Any(w => w.PersonId == value.PersonId && w.InmateJuvenileFlag != 1))
                {
                    inmateNumber = _commonService.GetGlobalNumber(2); // get the global numbers

                checkAlreadyExistsInmate:
                    if (_context.Inmate.Any(w => w.InmateNumber == inmateNumber))
                    {
                        inmateNumber = _commonService.GetGlobalNumber(2); // get the global numbers
                        goto checkAlreadyExistsInmate;
                    }
                    int age = _commonService.GetAgeFromDob(dob);
                    inmate = new Inmate
                    {
                        PersonId = value.PersonId,
                        InmateNumber = inmateNumber,
                        InmateReceivedDate = DateTime.Now,
                        InmateOfficerId = _personnelId,
                        FacilityId = value.FacilityId,
                        InmateJuvenileFlag = age < 18 ? 1 : 0
                    };

                    _context.Inmate.Add(inmate);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    //no inmateId passed from client
                    inmate = _context.Inmate.Single(a => a.PersonId == value.PersonId);

                    if (value.FacilityId > 0)
                    {
                        inmate.FacilityId = value.FacilityId;
                    }
                }

                value.InmateId = inmate.InmateId;

                Incarceration incarceration;

                if (!_context.Incarceration.Any(w => w.InmateId == value.InmateId && !w.ReleaseOut.HasValue))
                {
                    incarceration = new Incarceration
                    {
                        InmateId = value.InmateId,
                        DateIn = DateTime.Now,
                        FacilityIdIn = value.FacilityId,
                        InOfficerId = _personnelId,
                        ChargeLevel = "O",
                        UsedPersonLast = value.PersonLastName,
                        UsedPersonFrist = value.PersonFirstName,
                        UsedPersonMiddle = value.PersonMiddleName,
                        UsedPersonSuffix = value.PersonSuffix,
                        UsedDob = value.PersonDob?.ToString(CultureInfo.InvariantCulture),
                        ReceiveMethod = value.ReceiveMethod,
                        ExpediteBookingFlag = value.ExpediteBookingFlag,
                        ExpediteBookingDate = DateTime.Now,
                        ExpediteBookingNote = value.ExpediteBookingNote,
                        ExpediteBookingReason = value.ExpediteBookingReason,
                        ExpediteBookingBy = _personnelId,
                        ExpediteBookingByNavigation = _context.Personnel.Single(per => per.PersonnelId == _personnelId)
                    };

                    _context.Incarceration.Add(incarceration);

                    IncarcerationNoKeeperHistory incNoKeep = new IncarcerationNoKeeperHistory
                    {
                        Keeper = true,
                        NoKeeper = false,
                        NoKeeperNote = null,
                        NoKeeperReason = null,
                        IncarcerationId = incarceration.IncarcerationId,
                        CreateDate = DateTime.Now,
                        CreateBy = _personnelId
                    };

                    _context.IncarcerationNoKeeperHistory.Add(incNoKeep);
                    IncarcerationFacility incarcerationFacility = new IncarcerationFacility
                    {
                        IncarcerationId = incarceration.IncarcerationId,
                        FacilityId = value.FacilityId,
                        IncarcerationFrom = DateTime.Now,
                        IncarcerationFromBy = _personnelId
                    };

                    _context.IncarcerationFacility.Add(incarcerationFacility);

                    IncarcerationFacilityHistory incarcerationFacilityHistory = new IncarcerationFacilityHistory
                    {
                        IncarcerationId = incarceration.IncarcerationId,
                        InmateId = value.InmateId.Value,
                        TransferFromFacilityId = 0,
                        FacilityId = value.FacilityId,
                        MoveDate = DateTime.Now,
                        MoveDateBy = _personnelId
                    };

                    _context.IncarcerationFacilityHistory.Add(incarcerationFacilityHistory);

                    inmate.InmateActive = 1;
                    inmate.LastClassReviewDate = null;
                    inmate.LastClassReviewBy = null;
                    await _context.SaveChangesAsync();


                }
                else
                {
                    incarceration =
                        _context.Incarceration.Single(w => w.InmateId == value.InmateId && !w.ReleaseOut.HasValue);
                }

                if (value.BookAndRelease)
                {
                    int appAoWizardStepsId = _context.AppAoWizardSteps
                        .Where(w => w.AppAoWizardId == 1 && w.FacilityId == value.FacilityId)
                        .Select(x => x.AppAoWizardStepsId).FirstOrDefault();
                    incarceration.BookAndReleaseFlag = 1;
                    incarceration.IntakeCompleteFlag = 1;
                    incarceration.IntakeWizardLastStepId = appAoWizardStepsId;
                    incarceration.IntakeCompleteBy = _personnelId;
                    incarceration.IntakeCompleteDate = DateTime.Now;
                }

                if (value.ReactivateArrestId == 0)
                {
                    int bookingCount = _context.IncarcerationArrestXref.Count(w =>
                            w.IncarcerationId == incarceration.IncarcerationId);

                    if (bookingCount > 0)
                    {
                        supplementalBooking = true;
                    }

                    bookNumber =
                        _commonService.GetGlobalNumber(1, supplementalBooking, incarceration.IncarcerationId);

                checkAlreadyExistsBooking:
                    if (_context.Arrest.Any(w => w.ArrestBookingNo == bookNumber))
                    {
                        bookNumber = _commonService.GetGlobalNumber(1, supplementalBooking,
                            incarceration.IncarcerationId); // get the global numbers
                        goto checkAlreadyExistsBooking;
                    }

                    if (!supplementalBooking)
                    {
                        incarceration.BookingNo = bookNumber;
                        await _context.SaveChangesAsync();
                    }

                    if (!supplementalBooking || value.IsCourtCommit)
                    {
                        inmatePrebookCases = _context.InmatePrebookCase
                            .Where(ipc => ipc.InmatePrebookId == value.InmatePreBookId
                                && !ipc.DeleteFlag && ipc.ArrestType.HasValue).ToList();
                    }

                    if (supplementalBooking)
                    {
                        Arrest arrest = new Arrest
                        {
                            ArrestBookingNo = bookNumber,
                            ArrestType = value.IsCourtCommit
                                        ? inmatePrebookCases[0].ArrestType.ToString() : value.BookingType,
                            ArrestDate = value.ArrestDate,
                            ArrestingAgencyId = value.ArrestAgencyId,
                            BookingAgencyId = value.BookingAgencyId,
                            ArrestBookingOfficerId =
                                value.ArrestBookingOfficerId > 0 ? value.ArrestBookingOfficerId : null,
                            ArrestOfficerId = value.ArrestOfficerId,
                            //why is this non nullable if we can set it as not required in field settings?
                            ArrestReceivingOfficerId = value.ReceivingOfficerId > 0 ? value.ReceivingOfficerId : 1,
                            ArrestTransportingOfficerId =
                                value.TransportOfficerId > 0 ? value.TransportOfficerId : null,
                            ArrestSearchOfficerId = value.SearchOfficerId > 0 ? value.SearchOfficerId : null,
                            ArrestBookingDate = DateTime.Now,
                            ArrestOfficerText = value.ArrestOfficerName,
                            InmateId = inmate.InmateId,
                            ArrestTransportingOfficerText = value.TransportOfficerName,
                            ArrestActive = 1,
                            ArrestCaseNumber = value.ArrestCaseNumber,
                            ArrestLocation = value.ArrestLocation,
                            CreateBy = _personnelId,
                            ArrestCourtDocket = value.ArrestCourtDocket,
                            ArrestCourtJurisdictionId = value.ArrestCourtJurisdictionId,
                            ArrestOfficerContactNumber = value.ContactNumber
                        };

                        _context.Arrest.Add(arrest);
                        await _context.SaveChangesAsync();
                        arrestID = arrest.ArrestId;

                        int seqNumber = 0;
                        int seqIndex = bookNumber.IndexOf(".", StringComparison.Ordinal);

                        if (seqIndex > 0)
                        {
                            seqNumber = Convert.ToInt32(bookNumber.Substring(seqIndex + 1,
                                bookNumber.Length - (seqIndex + 1)));
                        }

                        if (seqNumber > 0)
                        {
                            arrest.ArrestSupSeqNumber = seqNumber;
                        }

                        IncarcerationArrestXref incarcerationArrestXref = new IncarcerationArrestXref
                        {
                            IncarcerationId = incarceration.IncarcerationId,
                            ArrestId = arrest.ArrestId,
                            BookingOfficerId = _personnelId,
                            BookingDate = arrest.ArrestBookingDate
                        };

                        _context.IncarcerationArrestXref.Add(incarcerationArrestXref);
                        incarceration.BookBookDataFlag = null;
                        incarceration.BookBookDataBy = null;
                        incarceration.BookBookDataDate = null;

                        AoWizardProgressVm IntakeWizard = new AoWizardProgressVm
                        {
                            IncarcerationId = incarceration.IncarcerationId,
                            WizardId = 1
                        };
                        IntakeWizard.WizardProgressId = await _wizardService.CreateWizardProgress(IntakeWizard);

                        AoWizardProgressVm BookingWizard = new AoWizardProgressVm
                        {
                            IncarcerationId = incarceration.IncarcerationId,
                            WizardId = 2
                        };
                        BookingWizard.WizardProgressId = await _wizardService.CreateWizardProgress(BookingWizard);

                        List<AoWizardStepProgress> intakeWizards = _context.AoWizardStepProgress.Where(aow =>
                            aow.AoComponentId == 35 && (aow.AoWizardProgressId == IntakeWizard.WizardProgressId ||
                                                        aow.AoWizardProgressId == BookingWizard.WizardProgressId)).ToList();

                        foreach (AoWizardStepProgress aoWizardStepProgress in intakeWizards)
                        {
                            aoWizardStepProgress.StepComplete = false;
                            aoWizardStepProgress.StepCompleteBy = null;
                            aoWizardStepProgress.StepCompleteDate = default;
                        }

                        await _context.SaveChangesAsync();
                    }
                }
                else
                {
                    if (value.ReactivateBookingFlag == "allow_weekender_sentence")
                    {
                        IncarcerationArrestXref incarcerationArrestXref = new IncarcerationArrestXref
                        {
                            IncarcerationId = incarceration.IncarcerationId,
                            ArrestId = value.ReactivateArrestId,
                            BookingOfficerId = _personnelId,
                            BookingDate = DateTime.Now,
                            ReactivateFlag = 1
                        };

                        _context.Add(incarcerationArrestXref);
                        Arrest reactivearrest = _context.Arrest.SingleOrDefault(w =>
                            w.ArrestId == value.ReactivateArrestId);
                        if (reactivearrest != null)
                        {
                            reactivearrest.ArrestSentenceReleaseDate = null;
                        }
                        await _context.SaveChangesAsync();
                    }
                    else
                    {
                        Arrest reactivatearrest =
                            _context.Arrest.SingleOrDefault(w => w.ArrestId == value.ReactivateArrestId);
                        if (reactivatearrest != null)
                        {
                            Arrest arrest = new Arrest
                            {
                                ArrestBookingNo = reactivatearrest.ArrestBookingNo,
                                ArrestType = value.BookingType,
                                ArrestDate = value.ArrestDate,
                                ArrestingAgencyId = value.ArrestAgencyId,
                                BookingAgencyId = value.BookingAgencyId,
                                ArrestBookingOfficerId = value.SearchOfficerId > 0 ? value.SearchOfficerId : null,
                                ArrestOfficerId = value.ArrestOfficerId,
                                ArrestReceivingOfficerId = value.ReceivingOfficerId,
                                ArrestTransportingOfficerId =
                                    value.TransportOfficerId > 0 ? value.TransportOfficerId : null,
                                ArrestSearchOfficerId = value.SearchOfficerId > 0 ? value.SearchOfficerId : null,
                                ArrestBookingDate = DateTime.Now,
                                ArrestOfficerText = value.ArrestOfficerName,
                                InmateId = inmate.InmateId,
                                ArrestTransportingOfficerText = value.TransportOfficerName,
                                ArrestActive = 1,
                                ArrestCaseNumber = value.ArrestCaseNumber,
                                ArrestLocation = value.ArrestLocation,
                                CreateBy = _personnelId,
                                ArrestArraignmentCourtId = reactivatearrest.ArrestArraignmentCourtId,
                                ArrestCourtDocket = reactivatearrest.ArrestCourtDocket,
                                ArrestCourtJurisdictionId = reactivatearrest.ArrestCourtJurisdictionId,
                                ArrestSiteBookingNo = reactivatearrest.ArrestSiteBookingNo,
                                ArrestBillingAgencyId = reactivatearrest.ArrestBillingAgencyId,
                                BailAmount = reactivatearrest.BailAmount,
                                BailNoBailFlag = reactivatearrest.BailNoBailFlag,
                                BailType = reactivatearrest.BailType,
                                OriginatingAgencyId = reactivatearrest.OriginatingAgencyId,
                                ArrestSupSeqNumber = reactivatearrest.ArrestSupSeqNumber
                            };
                            _context.Add(arrest);
                            await _context.SaveChangesAsync();
                            arrestID = arrest.ArrestId;
                            IncarcerationArrestXref incarcerationArrestXref = new IncarcerationArrestXref
                            {
                                IncarcerationId = incarceration.IncarcerationId,
                                ArrestId = arrest.ArrestId,
                                BookingOfficerId = _personnelId,
                                BookingDate = arrest.ArrestBookingDate,
                                ReactivateFlag = 1
                            };

                            _context.Add(incarcerationArrestXref);

                            IQueryable<Warrant> warrant =
                                _context.Warrant.Where(w => w.ArrestId == value.ReactivateArrestId);
                            foreach (Warrant row in warrant)
                            {
                                IQueryable<Crime> crime = _context.Crime.Where(w => w.WarrantId == row.WarrantId);
                                IQueryable<CrimeForce> crimeforce =
                                    _context.CrimeForce.Where(w => w.WarrantId == row.WarrantId);
                                Warrant reactivatewarrant = new Warrant
                                {
                                    ArrestId = arrest.ArrestId,
                                    ArrestDispositionId = row.ArrestDispositionId,
                                    ArrestIdSealed = row.ArrestIdSealed,
                                    CaseId = row.CaseId,
                                    CreateDate = row.CreateDate,
                                    LocalWarrantFlag = row.LocalWarrantFlag,
                                    PersonId = row.PersonId,
                                    PreBookingId = row.PreBookingId,
                                    PreviouslyCleared = row.PreviouslyCleared,
                                    RefArrestId = row.RefArrestId,
                                    UnclearDate = row.UnclearDate,
                                    UnclearPersonnelId = row.UnclearPersonnelId,
                                    UpdateDate = row.UpdateDate,
                                    WarrantAgencyId = row.WarrantAgencyId,
                                    WarrantBailAmount = row.WarrantBailAmount,
                                    WarrantBailType = row.WarrantBailType,
                                    WarrantChargeType = row.WarrantChargeType,
                                    WarrantClearedById = row.WarrantClearedById,
                                    WarrantClearedDate = row.WarrantClearedDate,
                                    WarrantClearedReason = row.WarrantClearedReason,
                                    WarrantComplete = row.WarrantComplete,
                                    WarrantCompletedBy = row.WarrantCompletedBy,
                                    WarrantCompletedDate = row.WarrantCompletedDate,
                                    WarrantCounty = row.WarrantCounty,
                                    WarrantCourtDocket = row.WarrantCourtDocket,
                                    WarrantCourtId = row.WarrantCourtId,
                                    WarrantDate = row.WarrantDate,
                                    WarrantDescription = row.WarrantDescription,
                                    WarrantDispo = row.WarrantDispo,
                                    WarrantEntered = row.WarrantEntered,
                                    WarrantEnteredBy = row.WarrantEnteredBy,
                                    WarrantExpDate = row.WarrantExpDate,
                                    WarrantJudgeId = row.WarrantJudgeId,
                                    WarrantLogNumber = row.WarrantNumber,
                                    WarrantState = row.WarrantState,
                                    WarrantType = row.WarrantType,
                                    WarrantNumber = row.WarrantNumber,
                                    WarrantNightService = row.WarrantNightService,
                                    WarrantNotes = row.WarrantNotes,
                                    WarrantUpdated = row.WarrantUpdated,
                                    WarrantUpdatedBy = row.WarrantUpdatedBy,
                                    LocalWarrant = row.LocalWarrant
                                };
                                _context.Add(reactivatewarrant);
                                foreach (Crime crimerow in crime)
                                {
                                    Crime reactiveCrime = new Crime
                                    {
                                        WarrantId = reactivatewarrant.WarrantId,
                                        ArrestId = arrest.ArrestId,
                                        ArrestIdSealed = crimerow.ArrestIdSealed,
                                        ArrestSentenceActualDaysToServe = crimerow.ArrestSentenceActualDaysToServe,
                                        ArrestSentenceConsecutiveFlag = crimerow.ArrestSentenceConsecutiveFlag,
                                        ArrestSentenceConsecutiveTo = crimerow.ArrestSentenceConsecutiveTo,
                                        ArrestSentenceDateInfo = crimerow.ArrestSentenceDateInfo,
                                        ArrestSentenceDayForDayAllowed = crimerow.ArrestSentenceDayForDayAllowed,
                                        ArrestSentenceDayForDayAllowedOverride =
                                            crimerow.ArrestSentenceDayForDayAllowedOverride,
                                        ArrestSentenceDayForDayDays = crimerow.ArrestSentenceDayForDayDays,
                                        ArrestSentenceDayForDayDaysOverride =
                                            crimerow.ArrestSentenceDayForDayDaysOverride,
                                        ArrestSentenceDays = crimerow.ArrestSentenceDays,
                                        ArrestSentenceDaysAmount = crimerow.ArrestSentenceDaysAmount,
                                        ArrestSentenceDaysInterval = crimerow.ArrestSentenceDaysInterval,
                                        ArrestSentenceDaysStayed = crimerow.ArrestSentenceDaysStayed,
                                        ArrestSentenceDaysToServe = crimerow.ArrestSentenceDaysToServe,
                                        ArrestSentenceDisciplinaryDaysFlag =
                                            crimerow.ArrestSentenceDisciplinaryDaysFlag,
                                        ArrestSentenceDisciplinaryDaysSum = crimerow.ArrestSentenceDisciplinaryDaysSum,
                                        ArrestSentenceErcdays = crimerow.ArrestSentenceErcdays,
                                        ArrestSentenceExpirationDate = crimerow.ArrestSentenceExpirationDate,
                                        ArrestSentenceFindings = crimerow.ArrestSentenceFindings,
                                        ArrestSentenceFineAmount = crimerow.ArrestSentenceFineAmount,
                                        ArrestSentenceFineDays = crimerow.ArrestSentenceFineDays,
                                        ArrestSentenceFinePaid = crimerow.ArrestSentenceFinePaid,
                                        ArrestSentenceFinePerDay = crimerow.ArrestSentenceFinePerDay,
                                        ArrestSentenceFineToServe = crimerow.ArrestSentenceFineToServe,
                                        ArrestSentenceFineType = crimerow.ArrestSentenceFineType,
                                        ArrestSentenceForthwith = crimerow.ArrestSentenceForthwith,
                                        ArrestSentenceGtDays = crimerow.ArrestSentenceGtDays,
                                        ArrestSentenceGtDaysOverride = crimerow.ArrestSentenceGtDaysOverride,
                                        CrimeAddedByProbationFlag = crimerow.CrimeAddedByProbationFlag,
                                        ArrestSentenceGwGtAdjust = crimerow.ArrestSentenceGwGtAdjust,
                                        ArrestSentenceHours = crimerow.ArrestSentenceHours,
                                        ArrestSentenceJudgeId = crimerow.ArrestSentenceJudgeId,
                                        ArrestSentenceManual = crimerow.ArrestSentenceManual,
                                        ArrestSentenceMethodId = crimerow.ArrestSentenceMethodId,
                                        ArrestSentenceReleaseDate = crimerow.ArrestSentenceReleaseDate,
                                        ArrestSentenceStartDate = crimerow.ArrestSentenceStartDate,
                                        ArrestSentenceType = crimerow.ArrestSentenceType,
                                        ArrestSentenceUseStartDate = crimerow.ArrestSentenceUseStartDate,
                                        ArrestSentenceWtDays = crimerow.ArrestSentenceWtDays,
                                        ArrestSentenceWtDaysOverride = crimerow.ArrestSentenceWtDaysOverride,
                                        ArrestTimeServedDays = crimerow.ArrestTimeServedDays,
                                        BailAmount = crimerow.BailAmount,
                                        BailNoBailFlag = crimerow.BailNoBailFlag,
                                        BailType = crimerow.BailType,
                                        CaseId = crimerow.CaseId,
                                        ChargeQualifierLookup = crimerow.ChargeQualifierLookup,
                                        CreateDate = crimerow.CreateDate,
                                        CreatedBy = crimerow.CreatedBy,
                                        CrimeAttemptedCompleted = crimerow.CrimeAttemptedCompleted,
                                        CrimeConvictionFlag = crimerow.CrimeConvictionFlag,
                                        CrimeCorrectable = crimerow.CrimeCorrectable,
                                        CrimeCount = crimerow.CrimeCount,
                                        CrimeCourtId = crimerow.CrimeCourtId,
                                        CrimeCriminalActivityType = crimerow.CrimeCriminalActivityType,
                                        CrimeDegree = crimerow.CrimeDegree,
                                        CrimeDeleteFlag = crimerow.CrimeDeleteFlag,
                                        CrimeHistory = crimerow.CrimeHistory,
                                        CrimeLookupId = crimerow.CrimeLookupId,
                                        CrimeNotes = crimerow.CrimeNotes,
                                        CrimeNumber = crimerow.CrimeNumber,
                                        CrimePcWarrant = crimerow.CrimePcWarrant,
                                        CrimePrimaryFlag = crimerow.CrimePrimaryFlag,
                                        CrimeQualifierLookup = crimerow.CrimeQualifierLookup,
                                        CrimeSentenceHistory = crimerow.CrimeSentenceHistory,
                                        CrimeStatusLookup = crimerow.CrimeStatusLookup,
                                        CrimeType = crimerow.CrimeType,
                                        ExternalKey = crimerow.ExternalKey,
                                        IncidentId = crimerow.IncidentId,
                                        InvolvedPartyId = crimerow.InvolvedPartyId,
                                        PersonClassificationId = crimerow.PersonClassificationId,
                                        ProbationId = crimerow.ProbationId,
                                        SuspectId = crimerow.SuspectId,
                                        UpdateBy = crimerow.UpdateBy,
                                        UpdateDate = crimerow.UpdateDate,

                                    };
                                    _context.Add(reactiveCrime);
                                }

                                foreach (CrimeForce crimeforcerow in crimeforce)
                                {
                                    CrimeForce reactivateCrimeForce = new CrimeForce
                                    {
                                        ArrestId = arrest.ArrestId,
                                        WarrantId = reactivatewarrant.WarrantId,
                                        ArrestIdSealed = crimeforcerow.ArrestIdSealed,
                                        BailAmount = crimeforcerow.BailAmount,
                                        BailNoBailFlag = crimeforcerow.BailNoBailFlag,
                                        ChargeQualifierLookup = crimeforcerow.ChargeQualifierLookup,
                                        CreateDate = crimeforcerow.CreateDate,
                                        UpdateBy = crimeforcerow.UpdateBy,
                                        UpdateDate = crimeforcerow.UpdateDate,
                                        CreateBy = crimeforcerow.CreateBy,
                                        DeleteBy = crimeforcerow.DeleteBy,
                                        DeleteDate = crimeforcerow.DeleteDate,
                                        DeleteFlag = crimeforcerow.DeleteFlag,
                                        DropChargeFlag = crimeforcerow.DropChargeFlag,
                                        ForceCrimeLookupId = crimeforcerow.ForceCrimeLookupId,
                                        ForceSupervisorCompleteBy = crimeforcerow.ForceSupervisorCompleteBy,
                                        TempCrimeNotes = crimeforcerow.TempCrimeNotes,
                                        TempCrimeStatusLookup = crimeforcerow.TempCrimeStatusLookup,
                                        TempCrimeType = crimeforcerow.TempCrimeType,
                                        ForceSupervisorCompleteDate = crimeforcerow.ForceSupervisorCompleteDate,
                                        ForceSupervisorReviewFlag = crimeforcerow.ForceSupervisorReviewFlag,
                                        InmatePrebookId = crimeforcerow.InmatePrebookId,
                                        InmatePrebookWarrantId = crimeforcerow.InmatePrebookWarrantId,
                                        SearchCrimeLookupId = crimeforcerow.SearchCrimeLookupId,
                                        TempCrimeCodeType = crimeforcerow.TempCrimeCodeType,
                                        TempCrimeCount = crimeforcerow.TempCrimeCount,
                                        TempCrimeDescription = crimeforcerow.TempCrimeDescription,
                                        TempCrimeGroup = crimeforcerow.TempCrimeGroup,
                                        TempCrimeQualifierLookup = crimeforcerow.TempCrimeQualifierLookup,
                                        TempCrimeSection = crimeforcerow.TempCrimeSection,
                                        TempCrimeStatuteCode = crimeforcerow.TempCrimeStatuteCode,
                                    };
                                    _context.Add(reactivateCrimeForce);
                                }
                            }
                            IQueryable<Crime> crimewowarrant = _context.Crime.Where(w =>
                                !w.WarrantId.HasValue && w.ArrestId == value.ReactivateArrestId);
                            IQueryable<CrimeForce> crimeforcewowarrant = _context.CrimeForce.Where(w =>
                                !w.WarrantId.HasValue && w.ArrestId == value.ReactivateArrestId);
                            foreach (Crime crimerow in crimewowarrant)
                            {
                                Crime reactiveCrime = new Crime
                                {
                                    ArrestId = arrest.ArrestId,
                                    ArrestIdSealed = crimerow.ArrestIdSealed,
                                    ArrestSentenceActualDaysToServe = crimerow.ArrestSentenceActualDaysToServe,
                                    ArrestSentenceConsecutiveFlag = crimerow.ArrestSentenceConsecutiveFlag,
                                    ArrestSentenceConsecutiveTo = crimerow.ArrestSentenceConsecutiveTo,
                                    ArrestSentenceDateInfo = crimerow.ArrestSentenceDateInfo,
                                    ArrestSentenceDayForDayAllowed = crimerow.ArrestSentenceDayForDayAllowed,
                                    ArrestSentenceDayForDayAllowedOverride =
                                        crimerow.ArrestSentenceDayForDayAllowedOverride,
                                    ArrestSentenceDayForDayDays = crimerow.ArrestSentenceDayForDayDays,
                                    ArrestSentenceDayForDayDaysOverride = crimerow.ArrestSentenceDayForDayDaysOverride,
                                    ArrestSentenceDays = crimerow.ArrestSentenceDays,
                                    ArrestSentenceDaysAmount = crimerow.ArrestSentenceDaysAmount,
                                    ArrestSentenceDaysInterval = crimerow.ArrestSentenceDaysInterval,
                                    ArrestSentenceDaysStayed = crimerow.ArrestSentenceDaysStayed,
                                    ArrestSentenceDaysToServe = crimerow.ArrestSentenceDaysToServe,
                                    ArrestSentenceDisciplinaryDaysFlag = crimerow.ArrestSentenceDisciplinaryDaysFlag,
                                    ArrestSentenceDisciplinaryDaysSum = crimerow.ArrestSentenceDisciplinaryDaysSum,
                                    ArrestSentenceErcdays = crimerow.ArrestSentenceErcdays,
                                    ArrestSentenceExpirationDate = crimerow.ArrestSentenceExpirationDate,
                                    ArrestSentenceFindings = crimerow.ArrestSentenceFindings,
                                    ArrestSentenceFineAmount = crimerow.ArrestSentenceFineAmount,
                                    ArrestSentenceFineDays = crimerow.ArrestSentenceFineDays,
                                    ArrestSentenceFinePaid = crimerow.ArrestSentenceFinePaid,
                                    ArrestSentenceFinePerDay = crimerow.ArrestSentenceFinePerDay,
                                    ArrestSentenceFineToServe = crimerow.ArrestSentenceFineToServe,
                                    ArrestSentenceFineType = crimerow.ArrestSentenceFineType,
                                    ArrestSentenceForthwith = crimerow.ArrestSentenceForthwith,
                                    ArrestSentenceGtDays = crimerow.ArrestSentenceGtDays,
                                    ArrestSentenceGtDaysOverride = crimerow.ArrestSentenceGtDaysOverride,
                                    CrimeAddedByProbationFlag = crimerow.CrimeAddedByProbationFlag,
                                    ArrestSentenceGwGtAdjust = crimerow.ArrestSentenceGwGtAdjust,
                                    ArrestSentenceHours = crimerow.ArrestSentenceHours,
                                    ArrestSentenceJudgeId = crimerow.ArrestSentenceJudgeId,
                                    ArrestSentenceManual = crimerow.ArrestSentenceManual,
                                    ArrestSentenceMethodId = crimerow.ArrestSentenceMethodId,
                                    ArrestSentenceReleaseDate = crimerow.ArrestSentenceReleaseDate,
                                    ArrestSentenceStartDate = crimerow.ArrestSentenceStartDate,
                                    ArrestSentenceType = crimerow.ArrestSentenceType,
                                    ArrestSentenceUseStartDate = crimerow.ArrestSentenceUseStartDate,
                                    ArrestSentenceWtDays = crimerow.ArrestSentenceWtDays,
                                    ArrestSentenceWtDaysOverride = crimerow.ArrestSentenceWtDaysOverride,
                                    ArrestTimeServedDays = crimerow.ArrestTimeServedDays,
                                    BailAmount = crimerow.BailAmount,
                                    BailNoBailFlag = crimerow.BailNoBailFlag,
                                    BailType = crimerow.BailType,
                                    CaseId = crimerow.CaseId,
                                    ChargeQualifierLookup = crimerow.ChargeQualifierLookup,
                                    CreateDate = crimerow.CreateDate,
                                    CreatedBy = crimerow.CreatedBy,
                                    CrimeAttemptedCompleted = crimerow.CrimeAttemptedCompleted,
                                    CrimeConvictionFlag = crimerow.CrimeConvictionFlag,
                                    CrimeCorrectable = crimerow.CrimeCorrectable,
                                    CrimeCount = crimerow.CrimeCount,
                                    CrimeCourtId = crimerow.CrimeCourtId,
                                    CrimeCriminalActivityType = crimerow.CrimeCriminalActivityType,
                                    CrimeDegree = crimerow.CrimeDegree,
                                    CrimeDeleteFlag = crimerow.CrimeDeleteFlag,
                                    CrimeHistory = crimerow.CrimeHistory,
                                    CrimeLookupId = crimerow.CrimeLookupId,
                                    CrimeNotes = crimerow.CrimeNotes,
                                    CrimeNumber = crimerow.CrimeNumber,
                                    CrimePcWarrant = crimerow.CrimePcWarrant,
                                    CrimePrimaryFlag = crimerow.CrimePrimaryFlag,
                                    CrimeQualifierLookup = crimerow.CrimeQualifierLookup,
                                    CrimeSentenceHistory = crimerow.CrimeSentenceHistory,
                                    CrimeStatusLookup = crimerow.CrimeStatusLookup,
                                    CrimeType = crimerow.CrimeType,
                                    ExternalKey = crimerow.ExternalKey,
                                    IncidentId = crimerow.IncidentId,
                                    InvolvedPartyId = crimerow.InvolvedPartyId,
                                    PersonClassificationId = crimerow.PersonClassificationId,
                                    ProbationId = crimerow.ProbationId,
                                    SuspectId = crimerow.SuspectId,
                                    UpdateBy = crimerow.UpdateBy,
                                    UpdateDate = crimerow.UpdateDate,

                                };
                                _context.Add(reactiveCrime);
                            }
                            foreach (CrimeForce crimeforcerow in crimeforcewowarrant)
                            {
                                CrimeForce reactivateCrimeForce = new CrimeForce
                                {
                                    ArrestId = arrest.ArrestId,
                                    ArrestIdSealed = crimeforcerow.ArrestIdSealed,
                                    BailAmount = crimeforcerow.BailAmount,
                                    BailNoBailFlag = crimeforcerow.BailNoBailFlag,
                                    ChargeQualifierLookup = crimeforcerow.ChargeQualifierLookup,
                                    CreateDate = crimeforcerow.CreateDate,
                                    UpdateBy = crimeforcerow.UpdateBy,
                                    UpdateDate = crimeforcerow.UpdateDate,
                                    CreateBy = crimeforcerow.CreateBy,
                                    DeleteBy = crimeforcerow.DeleteBy,
                                    DeleteDate = crimeforcerow.DeleteDate,
                                    DeleteFlag = crimeforcerow.DeleteFlag,
                                    DropChargeFlag = crimeforcerow.DropChargeFlag,
                                    ForceCrimeLookupId = crimeforcerow.ForceCrimeLookupId,
                                    ForceSupervisorCompleteBy = crimeforcerow.ForceSupervisorCompleteBy,
                                    TempCrimeNotes = crimeforcerow.TempCrimeNotes,
                                    TempCrimeStatusLookup = crimeforcerow.TempCrimeStatusLookup,
                                    TempCrimeType = crimeforcerow.TempCrimeType,
                                    ForceSupervisorCompleteDate = crimeforcerow.ForceSupervisorCompleteDate,
                                    ForceSupervisorReviewFlag = crimeforcerow.ForceSupervisorReviewFlag,
                                    InmatePrebookId = crimeforcerow.InmatePrebookId,
                                    InmatePrebookWarrantId = crimeforcerow.InmatePrebookWarrantId,
                                    SearchCrimeLookupId = crimeforcerow.SearchCrimeLookupId,
                                    TempCrimeCodeType = crimeforcerow.TempCrimeCodeType,
                                    TempCrimeCount = crimeforcerow.TempCrimeCount,
                                    TempCrimeDescription = crimeforcerow.TempCrimeDescription,
                                    TempCrimeGroup = crimeforcerow.TempCrimeGroup,
                                    TempCrimeQualifierLookup = crimeforcerow.TempCrimeQualifierLookup,
                                    TempCrimeSection = crimeforcerow.TempCrimeSection,
                                    TempCrimeStatuteCode = crimeforcerow.TempCrimeStatuteCode,
                                };
                                _context.Add(reactivateCrimeForce);
                            }
                            await _context.SaveChangesAsync();
                        }
                    }

                }
                if (value.InmatePreBookId > 0)
                {
                    InmatePrebook inmateprebook = _context.InmatePrebook.SingleOrDefault(w =>
                        w.InmatePrebookId == value.InmatePreBookId);
                    if (inmateprebook != null)
                    {
                        inmateprebook.IncarcerationId = incarceration.IncarcerationId;
                    }
                    if (arrestID > 0)
                    {
                        InsertArrestWarrantCharges(arrestID, inmateprebook, value);
                        inmateprebook.ArrestId = arrestID;
                    }
                    else
                    {
                        for (int i = 0; i < inmatePrebookCases.Count; i++)
                        {
                            InsertArrestWarrantCharges(arrestID, inmateprebook, value, bookNumber + "." + (i + 1),
                                inmate.InmateId,
                                supplementalBooking, incarceration.IncarcerationId,
                                inmatePrebookCases[i].InmatePrebookCaseId,
                                inmatePrebookCases[i].ArrestType.ToString(), inmatePrebookCases[i].CaseNumber);
                        }
                    }
                    _inmateService.CreateTask(inmate.InmateId, TaskValidateType.IntakeCreateEvent);
                }

                incarceration.BookBookDataFlag = null;
                incarceration.BookBookDataDate = null;
                incarceration.BookBookDataBy = null;
                incarceration.ReleaseClearFlag = null;
                incarceration.ReleaseClearDate = null;
                incarceration.ReleaseClearBy = null;
                incarceration.ReleaseSupervisorCompleteFlag = null;
                incarceration.ReleaseSupervisorCompleteBy = null;
                incarceration.ReleaseSupervisorWizardLastStepId = null;
                incarceration.ReleaseWizardLastStepId = null;
                incarceration.ReleaseCompleteFlag = null;
                if (value.AltSentFacilityId > 0 && value.InmateId > 0)
                {
                    IQueryable<HousingUnitMoveHistory> housingunitmovehis = _context.HousingUnitMoveHistory.Where(w =>
                        w.InmateId == value.InmateId && !w.MoveDateThru.HasValue);
                    foreach (HousingUnitMoveHistory row in housingunitmovehis)
                    {
                        row.MoveDateThru = DateTime.Now;
                        row.MoveThruBy = _personnelId;
                    }
                    int? housingunitid = inmate.HousingUnitId;
                    if (housingunitid.HasValue && housingunitid > 0)
                    {
                        HousingUnitMoveHistory housunitmovehis = new HousingUnitMoveHistory
                        {
                            InmateId = inmate.InmateId,
                            HousingUnitFromId = housingunitid,
                            MoveOfficerId = _personnelId,
                            MoveDate = DateTime.Now
                        };
                        _context.Add(housunitmovehis);
                        inmate.HousingUnitId = null;
                    }
                    incarceration.IntakeCompleteFlag = null;
                    incarceration.IntakeCompleteBy = null;
                    incarceration.IntakeCompleteDate = null;
                    incarceration.IntakeWizardLastStepId = null;
                    incarceration.BookingWizardLastStepId = null;
                    incarceration.BookAndReleaseWizardLastStepId = null;
                    incarceration.BookCompleteFlag = null;
                    incarceration.BookCompleteBy = null;
                    incarceration.BookCompleteDate = null;
                    incarceration.BookingSupervisorCompleteFlag = null;
                    incarceration.BookingSupervisorCompleteDate = null;
                    incarceration.BookingSupervisorWizardLastStepId = null;
                    InmateClassification inmateclassific = _context.InmateClassification.SingleOrDefault(w =>
                        w.InmateId == inmate.InmateId && !w.InmateDateUnassigned.HasValue);
                    if (inmateclassific != null)
                    {
                        inmateclassific.InmateDateUnassigned = DateTime.Now;
                    }
                    inmate.InmateClassificationId = null;
                    inmate.FacilityId = value.AltSentFacilityId;
                    _context.SaveChanges();
                }
                if (value.ReactivateBookingFlag == "allow_weekender_sentence")
                {
                    int cntBook = _context.IncarcerationArrestXref.Count(w => !w.Incarceration.ReleaseOut.HasValue
                                                                              && w.Incarceration.IncarcerationId ==
                                                                              incarceration.IncarcerationId
                                                                              && w.Incarceration.InmateId.HasValue &&
                                                                              w.ArrestId.HasValue);
                    int cntComp = _context.IncarcerationArrestXref.Count(w => !w.Incarceration.ReleaseOut.HasValue
                                                                              && w.Incarceration.IncarcerationId ==
                                                                              incarceration.IncarcerationId
                                                                              && w.Incarceration.InmateId.HasValue &&
                                                                              w.ArrestId.HasValue &&
                                                                              w.Arrest.BookingCompleteFlag == 1);
                    if (cntBook == cntComp)
                    {
                        incarceration.BookBookDataFlag = 1;
                        incarceration.BookBookDataDate = DateTime.Now;
                        incarceration.BookBookDataBy = _personnelId;
                    }
                }

                //Update flags in Incarceration 
                Incarceration updateIncarceration =
                    _context.Incarceration.Single(i => i.IncarcerationId == incarceration.IncarcerationId);
                updateIncarceration.ExternalToDo = 1;
                updateIncarceration.ExternalToDoAttemptCount = 0;
                updateIncarceration.ExternalToDoExpire =
                    DateTime.Now.AddDays(10); // hardcoded for AfisExternalSiteOption
                updateIncarceration.ExternalToDoFirstRun = DateTime.Now;
                updateIncarceration.ExternalToDoNextRun = DateTime.Now;
                
                _interfaceEngineService.Export(new ExportRequestVm
                {
                    EventName = EventNameConstants.NEWINTAKE,
                    PersonnelId = _personnelId,
                    Param1 = inmate.PersonId.ToString(),
                    Param2 = incarceration.IncarcerationId.ToString()
                });
                transaction.Commit();

                //For File Booking supplemental
                if (value.IsSupplementalFlag)
                {
                    _interfaceEngineService.Export(new ExportRequestVm
                    {
                        EventName = value.IsActiveIncarceration ?
                        EventNameConstants.NEWCASE : EventNameConstants.NEWINTAKE,
                        PersonnelId = _personnelId,
                        Param1 = value.PersonId.ToString(),
                        Param2 = (arrestID ?? 0).ToString()
                    });
                }
            }

            return await _context.SaveChangesAsync();
        }

        private void InsertArrestWarrantCharges(int? arrestID, InmatePrebook inmateprebook, IntakeEntryVm value,
            string bookNumber = "",
            int inmateId = 0, bool supplementalBooking = false, int incarcerationId = 0, int inmatePrebookCaseId = 0,
            string bookingType = "", string caseNumber = "")
        {
            Arrest arrest;
            string programCaseSiteOption = _context.SiteOptions.SingleOrDefault(so =>
                so.SiteOptionsName == SiteOptionsConstants.MULTIPLEPREBOOKCASE)?.SiteOptionsValue;
            if (arrestID > 0)
            {
                arrest = _context.Arrest.SingleOrDefault(w => w.ArrestId == arrestID);
            }
            else
            {
                int seqNumber = 0;
                int seqIndex = bookNumber.IndexOf(".", StringComparison.Ordinal);

                if (seqIndex > 0)
                {
                    seqNumber = Convert.ToInt32(bookNumber.Substring(seqIndex + 1,
                        bookNumber.Length - (seqIndex + 1)));
                }

                arrest = new Arrest
                {
                    ArrestBookingNo = bookNumber,
                    ArrestType = Math.Round(Convert.ToDecimal(bookingType)).ToString(CultureInfo.InvariantCulture),
                    ArrestDate = value.ArrestDate,
                    ArrestingAgencyId = value.ArrestAgencyId,
                    BookingAgencyId = value.BookingAgencyId,
                    ArrestBookingOfficerId = value.ArrestBookingOfficerId > 0 ? value.ArrestBookingOfficerId : null,
                    ArrestOfficerId = value.ArrestOfficerId,
                    //why is this non nullable if we can set it as not required in field settings?
                    ArrestReceivingOfficerId = value.ReceivingOfficerId > 0 ? value.ReceivingOfficerId : 1,
                    ArrestTransportingOfficerId = value.TransportOfficerId > 0 ? value.TransportOfficerId : null,
                    ArrestSearchOfficerId = value.SearchOfficerId > 0 ? value.SearchOfficerId : null,
                    ArrestBookingDate = DateTime.Now,
                    ArrestOfficerText = value.ArrestOfficerName,
                    InmateId = inmateId,
                    ArrestTransportingOfficerText = value.TransportOfficerName,
                    ArrestActive = 1,
                    ArrestCaseNumber = caseNumber,
                    ArrestLocation = value.ArrestLocation,
                    CreateBy = _personnelId,
                    ArrestSupSeqNumber = seqNumber,
                    ArrestOfficerContactNumber = value.ContactNumber
                };

                _context.Arrest.Add(arrest);
                _context.SaveChanges();
                arrestID = arrest.ArrestId;

                if (arrestID > 0 && (value.IsCourtCommit || programCaseSiteOption == SiteOptionsConstants.OFF))
                {
                    inmateprebook.ArrestId = arrestID;
                }

                if (supplementalBooking)
                {
                    seqNumber = 0;
                    seqIndex = bookNumber.IndexOf(".", StringComparison.Ordinal);

                    if (seqIndex > 0)
                    {
                        seqNumber = Convert.ToInt32(bookNumber.Substring(seqIndex + 1,
                            bookNumber.Length - (seqIndex + 1)));
                    }

                    if (seqNumber > 0)
                    {
                        arrest.ArrestSupSeqNumber = seqNumber;
                    }
                }

                IncarcerationArrestXref incarcerationArrestXref = new IncarcerationArrestXref
                {
                    IncarcerationId = incarcerationId,
                    ArrestId = arrest.ArrestId,
                    BookingOfficerId = _personnelId,
                    BookingDate = arrest.ArrestBookingDate
                };

                _context.IncarcerationArrestXref.Add(incarcerationArrestXref);
                _context.SaveChanges();
            }

            if (arrest != null)
            {
                arrest.ArrestSentenceDescription = inmateprebook.ArrestSentenceDescription;
                arrest.ArrestSentenceAmended = inmateprebook.ArrestSentenceAmended;
                arrest.ArrestSentencePenalInstitution = inmateprebook.ArrestSentencePenalInstitution;
                arrest.ArrestSentenceOptionsRec = inmateprebook.ArrestSentenceOptionsRec;
                arrest.ArrestSentenceAltSentNotAllowed = inmateprebook.ArrestSentenceAltSentNotAllowed;
                arrest.ArrestSentenceNoEarlyRelease = inmateprebook.ArrestSentenceNoEarlyRelease;
                arrest.ArrestSentenceNoLocalParole = inmateprebook.ArrestSentenceNoLocalParole;
                arrest.ArrestSentenceDateInfo = inmateprebook.ArrestSentenceDateInfo;
                arrest.ArrestSentenceType = inmateprebook.ArrestsentenceType;
                arrest.ArrestSentenceFindings = inmateprebook.ArrestSentenceFindings;
                arrest.ArrestSentenceJudgeId = inmateprebook.ArrestSentenceJudgeId;
                arrest.ArrestSentenceConsecutiveFlag = inmateprebook.ArrestSentenceConsecutiveFlag;
                arrest.ArrestSentenceStartDate = inmateprebook.ArrestSentenceStartDate;
                arrest.ArrestSentenceDays = inmateprebook.ArrestSentenceDaysAmount;
                arrest.ArrestSentenceDaysInterval = inmateprebook.ArrestSentenceDaysInterval;
                arrest.ArrestSentenceFineDays = inmateprebook.ArrestSentenceFineDays;
                arrest.ArrestSentenceFineAmount = inmateprebook.ArrestSentenceFineAmount;
                arrest.ArrestSentenceFinePaid = inmateprebook.ArrestSentenceFinePaid;
                arrest.ArrestSentenceFineType = inmateprebook.ArrestSentenceFineType;
                arrest.ArrestSentenceFinePerDay = inmateprebook.ArrestSentenceFinePerDay;
                arrest.ArrestSentenceDaysStayed = inmateprebook.ArrestSentenceDaysStayed;
                arrest.ArrestTimeServedDays = inmateprebook.ArrestTimeServedDays;
                arrest.ArrestSentenceForthwith = inmateprebook.ArrestSentenceForthwith;

                int warrantTotalcnt = _context.InmatePrebookWarrant.Count(w =>
                    w.InmatePrebookId == inmateprebook.InmatePrebookId &&
                    (!w.DeleteFlag.HasValue || w.DeleteFlag == 0));

                int crimeTotalcnt = _context.InmatePrebookCharge.Count(w =>
                    w.InmatePrebookId == inmateprebook.InmatePrebookId &&
                    (!w.DeleteFlag.HasValue || w.DeleteFlag == 0) && !w.InmatePrebookWarrantId.HasValue);

                if (warrantTotalcnt > 0 && crimeTotalcnt == 0 &&
                    !string.IsNullOrEmpty(inmateprebook.ArrestCourtDocket) &&
                    inmateprebook.ArrestCourtJurisdictionId > 0)
                {
                    arrest.ArrestCourtDocket = inmateprebook.ArrestCourtDocket;
                    arrest.ArrestCourtJurisdictionId = inmateprebook.ArrestCourtJurisdictionId;
                }
            }
            if (inmateprebook.CourtCommitFlag != 1 && programCaseSiteOption == SiteOptionsConstants.ON)
            {
                List<InmatePrebookWarrant> inmateprebookwarrant = _context.InmatePrebookWarrant.Where(w
                    => w.InmatePrebookId == value.InmatePreBookId && w.InmatePrebookCaseId == inmatePrebookCaseId
                        && !w.DeleteFlag.HasValue).ToList();
                foreach (InmatePrebookWarrant row in inmateprebookwarrant)
                {
                    Warrant prebookwarrant = new Warrant
                    {
                        ArrestId = arrest.ArrestId,
                        PersonId = value.PersonId,
                        CreateDate = row.CreateDate,
                        UpdateDate = row.UpdateDate,
                        WarrantAgencyId = row.WarrantAgencyId,
                        WarrantBailAmount = row.WarrantBailAmount,
                        WarrantBailType = row.WarrantBailType,
                        WarrantChargeType = row.WarrantChargeType,
                        WarrantDescription = row.WarrantDescription,
                        WarrantType = row.WarrantType,
                        WarrantNumber = row.WarrantNumber,
                        WarrantEnteredBy = _personnelId,
                        WarrantDate = row.WarrantIssueDate,
                        LocalWarrant = row.LocalWarrant,
                        OriginatingAgency = row.OriginatingAgency
                    };
                    _context.Add(prebookwarrant);
                    IQueryable<InmatePrebookCharge> inmateprebookcharge = _context.InmatePrebookCharge.Where(w
                        => w.InmatePrebookWarrantId == row.InmatePrebookWarrantId
                            && (!w.DeleteFlag.HasValue || w.DeleteFlag == 0));
                    foreach (InmatePrebookCharge crimerow in inmateprebookcharge)
                    {
                        Crime prebookchargeCrime = new Crime
                        {
                            ArrestId = arrest.ArrestId,
                            CreatedBy = crimerow.CreateBy,
                            WarrantId = prebookwarrant.WarrantId,
                            BailAmount = crimerow.BailAmount,
                            BailNoBailFlag = crimerow.BailNoBailFlag,
                            ChargeQualifierLookup = crimerow.ChargeQualifierLookup,
                            CreateDate = crimerow.CreateDate,
                            CrimeCount = crimerow.CrimeCount,
                            CrimeLookupId = crimerow.CrimeLookupId.Value,
                            CrimeNotes = crimerow.CrimeNotes,
                            CrimeQualifierLookup = crimerow.CrimeQualifierLookup,
                            CrimeStatusLookup = crimerow.CrimeStatusLookup,
                            CrimeType = crimerow.CrimeType,
                            CrimeDeleteFlag = 0,
                            OffenceDate = crimerow.OffenceDate
                        };
                        _context.Crime.Add(prebookchargeCrime);

                        IQueryable<CrimeHistory> crimeHistories = _context.CrimeHistory
                            .Where(c => c.InmatePrebookChargeId == crimerow.InmatePrebookChargeId
                                        && !c.CrimeForceId.HasValue && c.CrimeDeleteFlag == 0);
                        foreach (CrimeHistory item in crimeHistories)
                        {
                            CrimeHistory crimeHistory = new CrimeHistory
                            {
                                BailAmount = item.BailAmount,
                                BailNoBailFlag = item.BailNoBailFlag,
                                ChargeQualifierLookup = item.ChargeQualifierLookup,
                                CreatDate = item.CreatDate,
                                CreatedBy = item.CreatedBy,
                                CrimeCount = item.CrimeCount,
                                CrimeDeleteFlag = item.CrimeDeleteFlag,
                                CrimeLookupForceString = item.CrimeLookupForceString,
                                CrimeLookupId = item.CrimeLookupId,
                                CrimeNotes = item.CrimeNotes,
                                CrimeQualifierLookup = item.CrimeQualifierLookup,
                                CrimeStatusLookup = item.CrimeStatusLookup,
                                CrimeType = item.CrimeType,
                                CrimeId = prebookchargeCrime.CrimeId
                            };
                            _context.CrimeHistory.Add(crimeHistory);
                        }

                        IQueryable<CrimeForce> inmateprebookcrimeforce = _context.CrimeForce.Where(w =>
                            w.InmatePrebookWarrantId == row.InmatePrebookWarrantId
                            && w.DeleteFlag == 0 && !w.DropChargeFlag.HasValue);
                        foreach (CrimeForce crimeforcerow in inmateprebookcrimeforce)
                        {
                            CrimeForce prebookCrimeForce = new CrimeForce
                            {
                                ArrestId = arrest.ArrestId,
                                WarrantId = prebookwarrant.WarrantId,
                                ArrestIdSealed = crimeforcerow.ArrestIdSealed,
                                BailAmount = crimeforcerow.BailAmount,
                                BailNoBailFlag = crimeforcerow.BailNoBailFlag,
                                ChargeQualifierLookup = crimeforcerow.ChargeQualifierLookup,
                                CreateDate = crimeforcerow.CreateDate,
                                UpdateBy = crimeforcerow.UpdateBy,
                                UpdateDate = crimeforcerow.UpdateDate,
                                CreateBy = crimeforcerow.CreateBy,
                                DeleteBy = crimeforcerow.DeleteBy,
                                DeleteDate = crimeforcerow.DeleteDate,
                                DeleteFlag = crimeforcerow.DeleteFlag,
                                DropChargeFlag = crimeforcerow.DropChargeFlag,
                                ForceCrimeLookupId = crimeforcerow.ForceCrimeLookupId,
                                ForceSupervisorCompleteBy = crimeforcerow.ForceSupervisorCompleteBy,
                                TempCrimeNotes = crimeforcerow.TempCrimeNotes,
                                TempCrimeStatusLookup = crimeforcerow.TempCrimeStatusLookup,
                                TempCrimeType = crimeforcerow.TempCrimeType,
                                ForceSupervisorCompleteDate = crimeforcerow.ForceSupervisorCompleteDate,
                                ForceSupervisorReviewFlag = crimeforcerow.ForceSupervisorReviewFlag,
                                InmatePrebookId = crimeforcerow.InmatePrebookId,
                                InmatePrebookWarrantId = crimeforcerow.InmatePrebookWarrantId,
                                SearchCrimeLookupId = crimeforcerow.SearchCrimeLookupId,
                                TempCrimeCodeType = crimeforcerow.TempCrimeCodeType,
                                TempCrimeCount = crimeforcerow.TempCrimeCount,
                                TempCrimeDescription = crimeforcerow.TempCrimeDescription,
                                TempCrimeGroup = crimeforcerow.TempCrimeGroup,
                                TempCrimeQualifierLookup = crimeforcerow.TempCrimeQualifierLookup,
                                TempCrimeSection = crimeforcerow.TempCrimeSection,
                                TempCrimeStatuteCode = crimeforcerow.TempCrimeStatuteCode,
                            };
                            _context.CrimeForce.Add(prebookCrimeForce);

                            IQueryable<CrimeHistory> crimeForceHistories = _context.CrimeHistory
                            .Where(c => c.CrimeForceId == crimeforcerow.CrimeForceId && c.CrimeDeleteFlag == 0);
                            foreach (CrimeHistory item in crimeForceHistories)
                            {
                                CrimeHistory crimeHistory = new CrimeHistory
                                {
                                    BailAmount = item.BailAmount,
                                    BailNoBailFlag = item.BailNoBailFlag,
                                    ChargeQualifierLookup = item.ChargeQualifierLookup,
                                    CreatDate = item.CreatDate,
                                    CreatedBy = item.CreatedBy,
                                    CrimeCount = item.CrimeCount,
                                    CrimeDeleteFlag = item.CrimeDeleteFlag,
                                    CrimeLookupForceString = item.CrimeLookupForceString,
                                    CrimeLookupId = item.CrimeLookupId,
                                    CrimeNotes = item.CrimeNotes,
                                    CrimeQualifierLookup = item.CrimeQualifierLookup,
                                    CrimeStatusLookup = item.CrimeStatusLookup,
                                    CrimeType = item.CrimeType,
                                    CrimeForceId = prebookCrimeForce.CrimeForceId
                                };
                                _context.CrimeHistory.Add(crimeHistory);
                            }
                        }
                    }
                }

                List<InmatePrebookCharge> chargewowarrant = _context.InmatePrebookCharge.Where(w =>
                    w.InmatePrebookId == value.InmatePreBookId && !w.InmatePrebookWarrantId.HasValue
                    && w.InmatePrebookCaseId == inmatePrebookCaseId
                    && (!w.DeleteFlag.HasValue || w.DeleteFlag == 0)).ToList();
                foreach (InmatePrebookCharge crimerow in chargewowarrant)
                {
                    Crime prebookchargeCrime = new Crime
                    {
                        ArrestId = arrest.ArrestId,
                        CreatedBy = crimerow.CreateBy,
                        BailAmount = crimerow.BailAmount,
                        BailNoBailFlag = crimerow.BailNoBailFlag,
                        ChargeQualifierLookup = crimerow.ChargeQualifierLookup,
                        CreateDate = crimerow.CreateDate,
                        CrimeCount = crimerow.CrimeCount,
                        CrimeLookupId = crimerow.CrimeLookupId.Value,
                        CrimeNotes = crimerow.CrimeNotes,
                        CrimeQualifierLookup = crimerow.CrimeQualifierLookup,
                        CrimeStatusLookup = crimerow.CrimeStatusLookup,
                        CrimeType = crimerow.CrimeType,
                        CrimeDeleteFlag = 0,
                        OffenceDate = crimerow.OffenceDate
                    };
                    _context.Crime.Add(prebookchargeCrime);

                    IQueryable<CrimeHistory> crimeHistories = _context.CrimeHistory
                            .Where(c => c.InmatePrebookChargeId == crimerow.InmatePrebookChargeId
                                        && !c.CrimeForceId.HasValue && c.CrimeDeleteFlag == 0);
                    foreach (CrimeHistory item in crimeHistories)
                    {
                        CrimeHistory crimeHistory = new CrimeHistory
                        {
                            BailAmount = item.BailAmount,
                            BailNoBailFlag = item.BailNoBailFlag,
                            ChargeQualifierLookup = item.ChargeQualifierLookup,
                            CreatDate = item.CreatDate,
                            CreatedBy = item.CreatedBy,
                            CrimeCount = item.CrimeCount,
                            CrimeDeleteFlag = item.CrimeDeleteFlag,
                            CrimeLookupForceString = item.CrimeLookupForceString,
                            CrimeLookupId = item.CrimeLookupId,
                            CrimeNotes = item.CrimeNotes,
                            CrimeQualifierLookup = item.CrimeQualifierLookup,
                            CrimeStatusLookup = item.CrimeStatusLookup,
                            CrimeType = item.CrimeType,
                            CrimeId = prebookchargeCrime.CrimeId
                        };
                        _context.CrimeHistory.Add(crimeHistory);
                    }
                }
                List<CrimeForce> crimeforcewowarrant = _context.CrimeForce.Where(w =>
                    !w.InmatePrebookWarrantId.HasValue && w.InmatePrebookId == value.InmatePreBookId
                    && w.DeleteFlag == 0 && w.InmatePrebookCaseId == inmatePrebookCaseId
                    && !w.DropChargeFlag.HasValue).ToList();
                foreach (CrimeForce crimeforcerow in crimeforcewowarrant)
                {
                    CrimeForce prebookCrimeForce = new CrimeForce
                    {
                        ArrestId = arrest.ArrestId,
                        ArrestIdSealed = crimeforcerow.ArrestIdSealed,
                        BailAmount = crimeforcerow.BailAmount,
                        BailNoBailFlag = crimeforcerow.BailNoBailFlag,
                        ChargeQualifierLookup = crimeforcerow.ChargeQualifierLookup,
                        CreateDate = crimeforcerow.CreateDate,
                        UpdateBy = crimeforcerow.UpdateBy,
                        UpdateDate = crimeforcerow.UpdateDate,
                        CreateBy = crimeforcerow.CreateBy,
                        DeleteBy = crimeforcerow.DeleteBy,
                        DeleteDate = crimeforcerow.DeleteDate,
                        DeleteFlag = crimeforcerow.DeleteFlag,
                        DropChargeFlag = crimeforcerow.DropChargeFlag,
                        ForceCrimeLookupId = crimeforcerow.ForceCrimeLookupId,
                        ForceSupervisorCompleteBy = crimeforcerow.ForceSupervisorCompleteBy,
                        TempCrimeNotes = crimeforcerow.TempCrimeNotes,
                        TempCrimeStatusLookup = crimeforcerow.TempCrimeStatusLookup,
                        TempCrimeType = crimeforcerow.TempCrimeType,
                        ForceSupervisorCompleteDate = crimeforcerow.ForceSupervisorCompleteDate,
                        ForceSupervisorReviewFlag = crimeforcerow.ForceSupervisorReviewFlag,
                        InmatePrebookId = crimeforcerow.InmatePrebookId,
                        InmatePrebookWarrantId = crimeforcerow.InmatePrebookWarrantId,
                        SearchCrimeLookupId = crimeforcerow.SearchCrimeLookupId,
                        TempCrimeCodeType = crimeforcerow.TempCrimeCodeType,
                        TempCrimeCount = crimeforcerow.TempCrimeCount,
                        TempCrimeDescription = crimeforcerow.TempCrimeDescription,
                        TempCrimeGroup = crimeforcerow.TempCrimeGroup,
                        TempCrimeQualifierLookup = crimeforcerow.TempCrimeQualifierLookup,
                        TempCrimeSection = crimeforcerow.TempCrimeSection,
                        TempCrimeStatuteCode = crimeforcerow.TempCrimeStatuteCode,
                    };
                    _context.CrimeForce.Add(prebookCrimeForce);

                    IQueryable<CrimeHistory> crimeForceHistories = _context.CrimeHistory
                            .Where(c => c.CrimeForceId == crimeforcerow.CrimeForceId);
                    foreach (CrimeHistory item in crimeForceHistories)
                    {
                        CrimeHistory crimeHistory = new CrimeHistory
                        {
                            BailAmount = item.BailAmount,
                            BailNoBailFlag = item.BailNoBailFlag,
                            ChargeQualifierLookup = item.ChargeQualifierLookup,
                            CreatDate = item.CreatDate,
                            CreatedBy = item.CreatedBy,
                            CrimeCount = item.CrimeCount,
                            CrimeDeleteFlag = item.CrimeDeleteFlag,
                            CrimeLookupForceString = item.CrimeLookupForceString,
                            CrimeLookupId = item.CrimeLookupId,
                            CrimeNotes = item.CrimeNotes,
                            CrimeQualifierLookup = item.CrimeQualifierLookup,
                            CrimeStatusLookup = item.CrimeStatusLookup,
                            CrimeType = item.CrimeType,
                            CrimeForceId = prebookCrimeForce.CrimeForceId
                        };
                        _context.CrimeHistory.Add(crimeHistory);
                    }
                }
                _context.SaveChanges();
            }
        }

        #endregion

        #region Complete Intake

        // Set Intake complete
        public async Task<int> SetIntakeProcessComplete(int incarcerationId)
        {
            Incarceration incar = _context.Incarceration.SingleOrDefault(w => w.IncarcerationId == incarcerationId);

            if (incar == null)
                return 0;

            incar.IntakeCompleteFlag = 1;
            incar.IntakeCompleteBy = _personnelId;
            incar.IntakeCompleteDate = DateTime.Now;

            int personId = _personService.GetInmateDetails(incar.InmateId ?? 0).PersonId;
            //EventVm evenHandle = new EventVm
            //{
            //    CorresId = incarcerationId,
            //    EventName = EventNameConstants.INTAKECOMPLETE,
            //    PersonId = personId
            //};
            //Insert into Web Service Event Type
            //_commonService.EventHandle(evenHandle);
            _interfaceEngineService.Export(new ExportRequestVm
            {
                EventName = EventNameConstants.INTAKECOMPLETE,
                PersonnelId = _personnelId,
                Param1 = personId.ToString(),
                Param2 = incarcerationId.ToString()
            });
            _inmateService.CreateTask(incar.InmateId ?? 0, TaskValidateType.IntakeCompleteEvent);

            return await _context.SaveChangesAsync();
        }

        // Set Intake Incomplete
        public async Task<int> SetIntakeProcessUndoComplete(int incarcerationId)
        {
            Incarceration incar = _context.Incarceration.SingleOrDefault(w => w.IncarcerationId == incarcerationId);

            if (incar == null)
                return 0;

            incar.IntakeCompleteFlag = 0;
            incar.IntakeCompleteBy = _personnelId;
            incar.IntakeCompleteDate = DateTime.Now;

            return await _context.SaveChangesAsync();
        }

        #endregion

        #region InmatePrebookCase

        public List<InmatePrebookCaseVm> GetIntakePrebookCase(int inmatePrebookId)
        {
            List<Lookup> lookups = _context.Lookup.Where(l => l.LookupType == LookupConstants.ARRTYPE).ToList();

            List<InmatePrebookCaseVm> inmatePrebookCases = _context.InmatePrebookCase
                .Where(ipc => ipc.InmatePrebookId == inmatePrebookId && !ipc.DeleteFlag)
                .Select(ipc => new InmatePrebookCaseVm
                {
                    InmatePrebookCaseId = ipc.InmatePrebookCaseId,
                    CaseTypeId = ipc.ArrestType,
                    CaseNumber = ipc.CaseNumber
                }).ToList();

            inmatePrebookCases.ForEach(item =>
            {
                if (item.CaseTypeId != null)
                {
                    item.CaseType = lookups.Single(l => (decimal?)l.LookupIndex == item.CaseTypeId).LookupDescription;
                }
            });
            return inmatePrebookCases;
        }

        #endregion

        #region  ApprovePrebook
        public async Task<int> ApprovePrebook(InmatePrebookReviewVm inmatePrebookReview)
        {
            InmatePrebook inmatePrebook = _context.InmatePrebook.Single(c => c.InmatePrebookId == inmatePrebookReview.InmatePrebookId);
            if (inmatePrebookReview.ApproveStatus)
            {
                inmatePrebook.IntakeReviewAccepted = true;
                inmatePrebook.IntakeReviewDenied = false;
                inmatePrebook.IntakeReviewComment = inmatePrebookReview.Comment;
                inmatePrebook.IntakeReviewDate = DateTime.Now;
                inmatePrebook.IntakeReviewBy = _personnelId;
            }
            else
            {
                inmatePrebook.IntakeReviewAccepted = false;
                inmatePrebook.IntakeReviewDenied = !inmatePrebookReview.UndoFlag;
                inmatePrebook.IntakeReviewComment = inmatePrebookReview.Comment;
                inmatePrebook.IntakeReviewDate = DateTime.Now;
                inmatePrebook.IntakeReviewBy = _personnelId;
                if (!inmatePrebookReview.UndoFlag)
                {
                    inmatePrebook.CompleteFlag = 0;
                }
            }
            var res = await _context.SaveChangesAsync();
            if (!inmatePrebookReview.ApproveStatus)
            {
                await _atimsHubService.GetPrebook();
            }
            return res;
        }
        #endregion

        public async Task<int> SetIdentityAccept(InmatePrebookVm obj)
        {
            InmatePrebook inmatePrebook =
                _context.InmatePrebook.Single(s => s.InmatePrebookId == obj.InmatePrebookId);
            inmatePrebook.PersonId = obj.PersonId;
            inmatePrebook.PersonLastName = obj.PersonLastName;
            inmatePrebook.PersonFirstName = obj.PersonFirstName;
            inmatePrebook.PersonMiddleName = obj.PersonMiddleName;
            inmatePrebook.PersonSuffix = obj.PersonSuffix;
            inmatePrebook.PersonDob = obj.PersonDob;
            inmatePrebook.IdentificationAccepted = true;
            inmatePrebook.IdentificationAcceptedBy = _personnelId;
            inmatePrebook.IdentificationAcceptedDate = DateTime.Now;
            return await _context.SaveChangesAsync();
        }

        public async Task<int> UpdateByPassedAndNotRequiredMedical(int inmatePrebookId, int medPrescreenStatusFlag)
        {
            InmatePrebook inmatePrebook =
                _context.InmatePrebook.Single(c => c.InmatePrebookId == inmatePrebookId);
            inmatePrebook.MedPrescreenStartFlag = 1;
            inmatePrebook.MedPrescreenStartBy = _personnelId;
            inmatePrebook.MedPrescreenStartDate = DateTime.Now;
            inmatePrebook.MedPrescreenStatusFlag = medPrescreenStatusFlag;
            inmatePrebook.MedPrescreenStatusBy = _personnelId;
            inmatePrebook.MedPrescreenStatusDate = DateTime.Now;
            return await _context.SaveChangesAsync();
        }

        public KeyValuePair<int, int> GetPrebookChargesAndWarrantCount(int incarcerationId, int arrestId)
        {
            KeyValuePair<int, int> ChargesWarrant = _context.InmatePrebook.Where(w =>
                w.IncarcerationId == incarcerationId && w.ArrestId == arrestId).Select(s => new KeyValuePair<int, int>(
                s.InmatePrebookCharge.Count(w =>
                    !w.CrimeId.HasValue && (!w.DeleteFlag.HasValue || w.DeleteFlag == 0) &&
                    !w.InmatePrebookWarrantId.HasValue)
                + s.CrimeForce.Count(w => !w.ArrestId.HasValue && w.DeleteFlag == 0),
                s.InmatePrebookWarrant.Count(
                    w => !w.WarrantId.HasValue && (!w.DeleteFlag.HasValue || w.DeleteFlag == 0)))).SingleOrDefault();
            return ChargesWarrant;
        }
    }
}