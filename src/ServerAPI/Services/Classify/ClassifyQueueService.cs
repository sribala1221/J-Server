﻿using GenerateTables.Models;
using ServerAPI.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using ServerAPI.Utilities;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Data.SqlClient;
using System.Data;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using ScheduleWidget.Common;
using ScheduleWidget.Schedule;

// ReSharper disable once CheckNamespace
namespace ServerAPI.Services
{
    public class ClassifyQueueService : IClassifyQueueService
    {
        private readonly AAtims _context;
        private IQueryable<Incarceration> _listIncarceration;
        private readonly IRequestService _requestService;
        private readonly IInmateService _inmateService;
        private readonly int _personnelId;
        private readonly IPersonService _personService;
        private readonly IFacilityHousingService _facilityHousingService;
        private readonly ICommonService _commonService;

        public ClassifyQueueService(AAtims context, IRequestService requestService,
            IHttpContextAccessor httpContextAccessor, IPersonService personService, IInmateService inmateService,
            IFacilityHousingService facilityHousingService, ICommonService commonService)
        {
            _context = context;
            _requestService = requestService;
            _personService = personService;
            _personnelId = Convert.ToInt32(httpContextAccessor.HttpContext.User.FindFirst("personnelId")?.Value);
            _inmateService = inmateService;
            _facilityHousingService = facilityHousingService;
            _commonService = commonService;
        }

        //queue page load 
        public ClassifyQueueVm GetClassifyQueue(int facilityId)
        {
            //To get Incarceration Details depend on facility id and active flag
            _listIncarceration = _context.Incarceration.Where(i =>
                i.Inmate.InmateActive == 1 && i.Inmate.FacilityId == facilityId && !i.ReleaseOut.HasValue);

            //getting site option
            string siteOption = _context.SiteOptions
                .SingleOrDefault(s => s.SiteOptionsVariable == SiteOptionsConstants.CLASSREVIEWQUEUEBYCLASSLEVEL)
                ?.SiteOptionsValue;

            ClassifyQueueVm queue = new ClassifyQueueVm
            {
                ClassifyQueue = GetClassifyQueueCount(facilityId),
                InProgressQueue = GetInProgressQueueCount(),
                ReviewQueue = GetReview(siteOption, facilityId),
                PendingQueue = _requestService.GetBookingPendingReq(facilityId, NumericConstants.THREE),
                EligibilityProgramRequestQueue = GetQueueProgramEligibility(facilityId),
                AssignQueue = _requestService.GetBookingAssignedReq(NumericConstants.THREE),
                SpecialQueue = GetSpecialQueue(facilityId),
                EligibilityRequestQueue = GetEligibilityRequest(facilityId),
                //QueueDetails = GetQueueDetails(input),
                SiteOption = siteOption
            };
            return queue;
        }

        public List<QueueDetails> GetQueueDetailsFromFacilityId(int facilityId)
        {
            QueueInputs input = new QueueInputs
            {
                FacilityId = facilityId,
                Flag = ClassifyQueueConstants.READYFORINITIAL,
                LoadFlag = true
            };

            return GetQueueDetails(input);
        }

        private List<KeyValuePair<int, string>> GetClassifyQueueCount(int facilityId) =>
            new List<KeyValuePair<int, string>> {
                new KeyValuePair<int, string>(
                    GetQueueDetailsFromFacilityIdStoredProcedure(facilityId, ClassifyQueueConstants.READYFORINITIAL).Count, ClassifyQueueConstants.READYFORINITIAL),
                new KeyValuePair<int, string>(
                    GetQueueDetailsFromFacilityIdStoredProcedure(facilityId, ClassifyQueueConstants.READYFORHOUSING).Count, ClassifyQueueConstants.READYFORHOUSING),
                new KeyValuePair<int, string>(
                    GetQueueDetailsFromFacilityIdStoredProcedure(facilityId, RequestType.PENDINGREQUEST).Count, RequestType.PENDINGREQUEST)
            };

        private List<KeyValuePair<int, string>> GetInProgressQueueCount() =>
            new List<KeyValuePair<int, string>> {
                new KeyValuePair<int, string>(
                    _listIncarceration.Count(i => !i.BookAndReleaseFlag.HasValue && !i.IntakeCompleteFlag.HasValue),
                    InventoryQueueConstants.INTAKE),
                new KeyValuePair<int, string>(
                    _listIncarceration.Count(i =>
                        !i.BookAndReleaseFlag.HasValue && i.IntakeCompleteFlag.HasValue &&
                        !i.BookCompleteFlag.HasValue),
                    InventoryQueueConstants.BOOKING)
            };

        //review queue list
        public List<QueueReviewDetails> GetReview(string siteOption, int facilityId)
        {
            IQueryable<Incarceration> incarcerationList = _context.Incarceration.Where(i =>
            i.Inmate.InmateActive == 1 && i.Inmate.FacilityId == facilityId && !i.ReleaseOut.HasValue);
            List<QueueReviewDetails> lstCount;
            if (siteOption == SiteOptionsConstants.OFF)
            {
                List<ReviewQueue> lstReview = incarcerationList.Where(w =>
                        !w.BookAndReleaseFlag.HasValue)
                    .Select(i => new ReviewQueue
                    {
                        InmateId = i.Inmate.InmateId,
                        FacilityAbbr = i.Inmate.Facility.FacilityAbbr,
                        ReviewDate = i.Inmate.LastClassReviewDate,
                        HousingUnitId = i.Inmate.HousingUnitId ?? 0
                    }).ToList();
                List<HousingUnit> lstHousing = _context.HousingUnit.Select(h => new HousingUnit
                {
                    HousingUnitId = h.HousingUnitId,
                    HousingUnitLocation = h.HousingUnitLocation,
                    HousingUnitNumber = h.HousingUnitNumber,
                }).ToList();

                //Review List When Site Option Is OFF
                lstCount = lstReview.SelectMany(i =>
                        lstHousing.Where(h => h.HousingUnitId == i.HousingUnitId), (i, h) =>
                        new ReviewQueue
                        {
                            InmateId = i.InmateId,
                            HousingUnitLocation = h.HousingUnitLocation,
                            HousingUnitNumber = h.HousingUnitNumber,
                            FacilityAbbr = i.FacilityAbbr,
                            ReviewDate = i.ReviewDate
                        }).GroupBy(i => new { i.FacilityAbbr, i.HousingUnitLocation, i.HousingUnitNumber })
                    .Select(i => new QueueReviewDetails
                    {
                        Count = i.Count(),
                        ReviewCount = i.Count(j => j.ReviewDate.HasValue)
                    }).ToList();
            }
            else
            {
                List<string> lstSiteOptions = _context.SiteOptions.Where(w =>
                    w.SiteOptionsName == SiteOptionsConstants.CLASSIFYREVIEWBATCH &&
                    w.SiteOptionsStatus == NumericConstants.ONE.ToString()).Select(i => i.SiteOptionsValue).ToList();

                List<ReviewQueue> lstReview = incarcerationList.Where(w =>
                        !w.BookAndReleaseFlag.HasValue && (!w.Inmate.LastClassReviewDate.HasValue 
                        || w.Inmate.LastClassReviewDate <= DateTime.Now)
                        && w.Inmate.InmateClassification.InmateClassificationReason != null)
                    .Select(i => new ReviewQueue
                    {
                        InmateId = i.Inmate.InmateId,
                        ReviewDate = i.Inmate.LastClassReviewDate.Value,
                        Reason = i.Inmate.InmateClassification.InmateClassificationReason
                    }).Distinct().ToList();

                List<Lookup> lstLookup = _context.Lookup.Where(l =>
                    l.LookupType == LookupConstants.CLASREAS && l.LookupInactive != 1).Select(l => new Lookup
                    {
                        LookupIndex = l.LookupIndex,
                        LookupCategory = l.LookupCategory,
                        LookupDescription = l.LookupDescription
                    }).ToList();

                lstReview.ForEach(item =>
                {
                    item.Housing = item.Reason;
                    Lookup lookupDet = lstLookup.SingleOrDefault(w => w.LookupDescription == item.Reason);
                   
                    if (lookupDet != null && !string.IsNullOrEmpty(lookupDet.LookupCategory))
                    {
                        item.Interval = Convert.ToInt32(lookupDet.LookupCategory);
                        item.Batch = lstSiteOptions.Count(w => !string.IsNullOrEmpty(w) && w.Contains(lookupDet.LookupDescription));
                    }
                });

                lstCount = lstReview.GroupBy(i => new { i.Housing, i.Batch, i.Interval }).Select(i => new QueueReviewDetails
                {
                    Batch = i.Key.Batch,
                    ReviewCount = i.Key.Interval > 0 ? i.Count(j =>
                            j.ReviewDate != null && j.ReviewDate.Value.AddDays(i.Key.Interval).Date <= DateTime.Now.Date)
                             : default,
                    Housing = i.Key.Housing,
                    Count = i.Count()
                }).ToList();
            }
            return lstCount;
        }

        //Eligibility Program Request List 
        private List<ProgramEligibility> GetQueueProgramEligibility(int facilityId)
        {
            List<int> lstAppointmentProgramAssign = _context.AppointmentProgramAssign
                .Select(i => i.AppointmentProgramAssignId).ToList();

            // This program table need to change based on programClass
            IQueryable<ProgramRequest> lstProgramReq = _context.ProgramRequest.Where(pro =>
                //pro.Program.ProgramCategory.FacilityId == facilityId &&
                pro.Inmate.InmateActive == 1 && !pro.DeniedFlag.HasValue && pro.DeleteFlag != 1);
            //Program Request List
            List<ProgramEligibility> lstProgramEligibility = lstProgramReq
                .Where(pro => !lstAppointmentProgramAssign.Contains(pro.AppointmentProgramAssignId ?? 0))
                .Select(p => new ProgramEligibility
                {
                    RequestId = p.ProgramRequestId,
                    ProgramId = p.ProgramClassId,
                    InmateId = p.InmateId,
                    PriorityLevel = p.PriorityLevel,
                    Note = p.RequestNote,
                    RequestDate = p.CreateDate,
                    DeleteFlag = p.DeleteFlag == 1,
                    ClassifyRouteFlag = p.ClassifyRouteFlag == 1,
                    DeniedFlag = p.DeniedFlag == 1,
                    //ProgramCategory = p.Program.ProgramCategory.ProgramCategory1,
                    //ClassOrServiceName = p.Program.ClassOrServiceName,
                    AssignedDetails = new QueueEligibilityPerson
                    {
                        PersonnelId = p.AssignedBy,
                        Note = p.AssignedNote,
                        Date = p.AssignedDate
                    },
                    ClassifyRouteDetails = new QueueEligibilityPerson
                    {
                        PersonnelId = p.ClassifyRouteBy,
                        Reason = p.ClassifyRouteReason,
                        Note = p.ClassifyRouteNote,
                        Date = p.ClassifyRouteDate
                    },
                    DeniedDetails = new QueueEligibilityPerson
                    {
                        PersonnelId = p.DeniedBy,
                        Reason = p.DeniedReason,
                        Note = p.DeniedNote,
                        Date = p.DeniedDate
                    },
                    AppointmentProgramAssignId = p.AppointmentProgramAssignId ?? 0,
                    UpdateDetails = new QueueEligibilityPerson
                    {
                        UpdateDate = p.UpdateDate,
                        PersonnelId = p.UpdateBy
                    }
                }).ToList();
            //Person Details List
            List<PersonVm> lstPersonDetails =
                _inmateService.GetInmateDetails(lstProgramEligibility.Select(i => i.InmateId).ToList());
            //List of Housing Details for inmates 
            List<HousingDetail> lstHousingDetail = _facilityHousingService.GetHousingList(lstPersonDetails
                .Where(i => i.HousingUnitId.HasValue).Select(i => i.HousingUnitId.Value).ToList());
            //List of all Officer Ids
            List<int> personnelId =
                lstProgramEligibility.Select(i => new[]
                    {
                        i.AssignedDetails.PersonnelId, i.ClassifyRouteDetails.PersonnelId,
                        i.DeniedDetails.PersonnelId, i.UpdateDetails.PersonnelId
                    })
                    .SelectMany(i => i)
                    .Where(i => i.HasValue)
                    .Select(i => i.Value)
                    .ToList();
            //List of Officer Details
            List<PersonnelVm> lstPersonDet = _personService.GetPersonNameList(personnelId);
            lstProgramEligibility.ForEach(item =>
            {
                item.InmateDetails = lstPersonDetails.Single(inm => inm.InmateId == item.InmateId);
                if (item.InmateDetails.HousingUnitId.HasValue)
                {
                    item.HousingDetails =
                        lstHousingDetail.Single(inm => inm.HousingUnitId == item.InmateDetails.HousingUnitId.Value);
                }
                if (item.AssignedDetails.PersonnelId.HasValue)
                {
                    PersonnelVm personInfo =
                        lstPersonDet.Single(p => p.PersonnelId == item.AssignedDetails.PersonnelId);
                    item.AssignedDetails.PersonLastName = personInfo?.PersonLastName;
                    item.AssignedDetails.OfficerBadgeNumber = personInfo?.OfficerBadgeNumber;
                }
                if (item.ClassifyRouteDetails.PersonnelId.HasValue)
                {
                    PersonnelVm personInfo =
                        lstPersonDet.Single(p => p.PersonnelId == item.ClassifyRouteDetails.PersonnelId);
                    item.ClassifyRouteDetails.PersonLastName = personInfo?.PersonLastName;
                    item.ClassifyRouteDetails.OfficerBadgeNumber = personInfo?.OfficerBadgeNumber;
                }
                if (item.DeniedDetails.PersonnelId.HasValue)
                {
                    PersonnelVm personInfo =
                        lstPersonDet.Single(p => p.PersonnelId == item.DeniedDetails.PersonnelId);
                    item.DeniedDetails.PersonLastName = personInfo?.PersonLastName;
                    item.DeniedDetails.OfficerBadgeNumber = personInfo?.OfficerBadgeNumber;
                }
                if (item.UpdateDetails.PersonnelId.HasValue)
                {
                    PersonnelVm personInfo =
                        lstPersonDet.Single(p => p.PersonnelId == item.UpdateDetails.PersonnelId);
                    item.UpdateDetails.PersonLastName = personInfo?.PersonLastName;
                    item.UpdateDetails.OfficerBadgeNumber = personInfo?.OfficerBadgeNumber;
                }
            });
            return lstProgramEligibility;
        }

        //Special Queue Count
        //In keyValuePair=> key is count and value is review count
        public KeyValuePair<int, int> GetSpecialQueue(int facilityId)
        {
            List<KeyValuePair<int, int>> specialQueueList = _context.Inmate.Where(i => i.FacilityId == facilityId && i.InmateActive == 1
                                        && i.SpecialClassQueueInterval > 0)
                .Select(i => new ReviewQueue
                {
                    InmateId = i.InmateId,
                    LastClassReviewDate = i.LastClassReviewDate,
                    ReviewDate = i.LastReviewDate,
                    Interval = i.SpecialClassQueueInterval ?? 0
                })
                .GroupBy(i => new { i.ReviewDate })
                .Select(i =>
                    new KeyValuePair<int, int>(i.Count(),
                        i.Count(c =>
                            c.LastClassReviewDate.HasValue &&
                            c.LastClassReviewDate.Value.AddDays(c.Interval) <= DateTime.Now)
                    )).ToList();

            return specialQueueList.FirstOrDefault();

        }

        //For Eligibility Work Crew Request
        private List<QueueWorkCrew> GetEligibilityRequest(int facilityId)
        {
            //ifWorkFurloughFlag==false means WORKCREWREQUEST
            //if WorkFurloughFlag==true means WORKFURLOUGHREQUEST
            IQueryable<WorkCrewRequest> lstWrkReq = _context.WorkCrewRequest.Where(w =>
                w.WorkCrewLookup.FacilityId == facilityId && !w.WorkCrewId.HasValue &&
                !w.DeniedFlag.HasValue && !w.DeleteFlag.HasValue && w.ClassifyRouteFlag == 1);

            List<QueueWorkCrew> workCrew = lstWrkReq.Select(w => new QueueWorkCrew
            {
                InmateId = w.InmateId ?? 0,
                WorkCrewRequestId = w.WorkCrewRequestId,
                CrewName = w.WorkCrewLookup.CrewName,
                RouteNote = w.ClassifyRouteNote,
                RouteDate = w.ClassifyRouteDate,
                WorkFurloughFlag = w.WorkCrewLookup.WorkFurloughFlag == 1
            }).ToList();
            //List of Person details
            List<PersonVm> lstPersonDetails =
                _inmateService.GetInmateDetails(workCrew.Select(i => i.InmateId).ToList());
            workCrew.ForEach(w =>
            {
                w.PersonDetails = lstPersonDetails.Single(inm => inm.InmateId == w.InmateId);
            });

            return workCrew;
        }

        //Queue details for left side grid click
        public List<QueueDetails> GetQueueDetails(QueueInputs inputs)
        {
            List<QueueDetails> listQueue = GetClassificationQueueDetails(inputs);
            if (!listQueue.Any()) return listQueue;
            //List of InmateIds
            List<int> inmateIds = listQueue.Select(i => i.InmateId).Distinct().ToList();
            //List<AoAppointmentVm> app = CourtAppDetails(inputs, inmateIds);
            listQueue = _context.Inmate.Where(x => inmateIds.Contains(x.InmateId)).Select(i => new QueueDetails
            {
                InmateId = i.InmateId,
                LastClassReviewDate = i.LastClassReviewDate,
                InmateClassificationId = i.InmateClassificationId ?? 0,
                Interval = listQueue.Where(lq => lq.InmateId == i.InmateId).Select(lq => lq.Interval).FirstOrDefault(),
                InmateClassificationReason = i.InmateClassification.InmateClassificationReason,
                Description = inputs.Flag,
                InmateDetails = new PersonInfo
                {
                    InmateId = i.InmateId,
                    PersonId = i.PersonId,
                    PersonFirstName = i.Person.PersonFirstName,
                    PersonMiddleName = i.Person.PersonMiddleName,
                    PersonLastName = i.Person.PersonLastName,
                    InmateNumber = i.InmateNumber,
                    FacilityId = i.FacilityId,
                    HousingUnitId = i.HousingUnitId ?? 0,
                    LastReviewDate = i.LastReviewDate,
                    InmateCurrentTrack = i.InmateCurrentTrack,
                    InmateClassificationId = i.InmateClassificationId,
                    PersonSexLast = i.Person.PersonSexLast,
                    PersonDob = i.Person.PersonDob,
                    PersonAge = i.Person.PersonAge,
                    sex = _context.Lookup.FirstOrDefault(w => w.LookupIndex == i.Person.PersonSexLast && w.LookupType == LookupConstants.SEX).LookupDescription
                },
                HousingDetails = i.HousingUnit != null ? new HousingDetail
                {
                    HousingUnitListId = i.HousingUnit.HousingUnitListId,
                    HousingUnitId = i.HousingUnit.HousingUnitId,
                    HousingUnitLocation = i.HousingUnit.HousingUnitLocation,
                    HousingUnitNumber = i.HousingUnit.HousingUnitNumber,
                    HousingUnitBedLocation = i.HousingUnit.HousingUnitBedLocation,
                    HousingUnitBedNumber = i.HousingUnit.HousingUnitBedNumber,
                    FacilityId = i.HousingUnit.FacilityId,
                    FacilityAbbr = i.HousingUnit.Facility.FacilityAbbr,
                    FacilityName = i.HousingUnit.Facility.FacilityName
                } : null,
                InmateTrakNote = !string.IsNullOrEmpty(i.InmateCurrentTrack)
                    ? i.InmateTrak.OrderByDescending(it => it.InmateTrakId)
                    .Select(it => it.InmateTrakNote).FirstOrDefault()
                    : null
                //CourtAppointment = app.Any(s => s.InmateId == i.InmateId)? app.Where(s => s.InmateId == i.InmateId).Select(s=>s.StartDate)
                //.FirstOrDefault():default(DateTime?)
            }).ToList();
            return listQueue;
        }

        private List<QueueDetails> GetClassificationQueueDetails(QueueInputs inputs) {
            List<QueueDetails> listQueue = new List<QueueDetails>();
            if (!inputs.LoadFlag
                && inputs.Flag != ClassifyQueueConstants.READYFORINITIAL
                && inputs.Flag != ClassifyQueueConstants.READYFORHOUSING
                && inputs.Flag != RequestType.PENDINGREQUEST)
            {
                _listIncarceration = _context.Incarceration.Where(i =>
                    i.Inmate.InmateActive == 1 && i.Inmate.FacilityId == inputs.FacilityId && !i.ReleaseOut.HasValue);
            }

            if (inputs.Flag is null)
                return listQueue;

            switch (inputs.Flag)
            {
                case ClassifyQueueConstants.READYFORINITIAL:
                    listQueue = GetQueueDetailsFromFacilityIdStoredProcedure(inputs.FacilityId, ClassifyQueueConstants.READYFORINITIAL);
                    break;
                case ClassifyQueueConstants.READYFORHOUSING:
                    listQueue = GetQueueDetailsFromFacilityIdStoredProcedure(inputs.FacilityId, ClassifyQueueConstants.READYFORHOUSING);
                    break;
                case RequestType.PENDINGREQUEST:
                    listQueue = GetQueueDetailsFromFacilityIdStoredProcedure(inputs.FacilityId, RequestType.PENDINGREQUEST);
                    break;
                case InventoryQueueConstants.INTAKE:
                    _listIncarceration =
                        _listIncarceration.Where(i => !i.IntakeCompleteFlag.HasValue && !i.BookAndReleaseFlag.HasValue);
                    break;
                case InventoryQueueConstants.BOOKING:
                    _listIncarceration = _listIncarceration.Where(i =>
                        i.IntakeCompleteFlag.HasValue && !i.BookAndReleaseFlag.HasValue &&
                        !i.BookCompleteFlag.HasValue);
                    break;
                case ClassifyQueueConstants.SPECIALQUEUE:
                    _listIncarceration = _listIncarceration.Where(i => i.Inmate.SpecialClassQueueInterval > 0);
                    break;
                //For Review Queue Details For Main Grid
                //It's Used For Classification Batch Review Popup Count And Details Grids =>
                //For this we want to put Group by & filter this list in client side.
                case ClassifyQueueConstants.REVIEWQUEUE:
                    _listIncarceration = _listIncarceration.Where(i =>
                        !i.BookAndReleaseFlag.HasValue && (!i.Inmate.LastClassReviewDate.HasValue || i.Inmate.LastClassReviewDate <= DateTime.Now)
                        && i.Inmate.InmateClassification.InmateClassificationReason != null);

                    if (inputs.Housing != CommonConstants.ALL.ToString())
                    {
                        _listIncarceration = _listIncarceration.Where(i => i.Inmate.InmateClassification.InmateClassificationReason == inputs.Housing);
                    }
                    List<QueueDetails> queueDetails = _listIncarceration.Select(i => new QueueDetails
                    {
                        InmateId = i.Inmate.InmateId,
                        LastClassReviewDate = i.Inmate.LastClassReviewDate,
                        InmateClassificationId = i.Inmate.InmateClassificationId ?? 0,
                        Description = i.Inmate.InmateClassification.InmateClassificationReason,
                        Interval = _context.Lookup.Where(l => 
                            l.LookupType == LookupConstants.CLASREAS 
                            && l.LookupInactive != 1 
                            && l.LookupDescription == i.Inmate.InmateClassification.InmateClassificationReason
                            && !string.IsNullOrEmpty(l.LookupCategory))
                        .Select(l => Convert.ToInt32(l.LookupCategory))
                        .SingleOrDefault()
                    }).ToList();
                    listQueue = queueDetails;
                    break;
            }

            if (inputs.Flag != ClassifyQueueConstants.REVIEWQUEUE
                && inputs.Flag != ClassifyQueueConstants.READYFORINITIAL
                && inputs.Flag != ClassifyQueueConstants.READYFORHOUSING
                && inputs.Flag != RequestType.PENDINGREQUEST)
            {
                listQueue = _listIncarceration.Select(i => new QueueDetails
                {
                    InmateId = i.Inmate.InmateId,
                    LastClassReviewDate = i.Inmate.LastClassReviewDate,
                    InmateClassificationId = i.Inmate.InmateClassificationId ?? 0,
                    Interval = i.Inmate.SpecialClassQueueInterval ?? 0,
                    InmateClassificationReason = i.Inmate.InmateClassification.InmateClassificationReason,
                    Description = inputs.Flag
                }).ToList();
            }

            return listQueue;
        }
        public List<AoAppointmentVm> CourtAppDetails(QueueInputs inputs, List<int> inmateIds)
        {
            List<AoAppointmentVm> inmateAppList = new List<AoAppointmentVm>();
            List<ScheduleCourt> lstScheduleInmate= new List<ScheduleCourt>();



            switch (inputs.Flag)
            {
                case ClassifyQueueConstants.READYFORINITIAL:
                    lstScheduleInmate = _context.Schedule.OfType<ScheduleCourt>().AsNoTracking()
                        .Where(a => !a.DeleteFlag && a.LocationId.HasValue && (a.Inmate.Incarceration.Any(inc =>
                         !inc.ReleaseOut.HasValue && inc.BookCompleteFlag.HasValue && inc.IntakeCompleteFlag.HasValue &&
                         !inc.BookAndReleaseFlag.HasValue) && a.Inmate.FacilityId == inputs.FacilityId && a.Inmate.InmateActive == 1 &&
                        !a.Inmate.InmateClassificationId.HasValue)).ToList();
                    break;
                case ClassifyQueueConstants.READYFORHOUSING:
                    lstScheduleInmate = _context.Schedule.OfType<ScheduleCourt>().AsNoTracking()
                        .Where(a => !a.DeleteFlag && a.LocationId.HasValue && (a.Inmate.Incarceration.Any(inc =>
                          !inc.ReleaseOut.HasValue && inc.BookCompleteFlag.HasValue && inc.IntakeCompleteFlag.HasValue &&
                          !inc.BookAndReleaseFlag.HasValue) && a.Inmate.FacilityId == inputs.FacilityId && a.Inmate.InmateActive == 1 &&
                          a.Inmate.InmateClassificationId.HasValue && a.Inmate.InmateClassification.InmateClassificationReason
                          != RequestType.PENDINGREQUEST)).ToList();
                    break;
                case RequestType.PENDINGREQUEST:
                    lstScheduleInmate = _context.Schedule.OfType<ScheduleCourt>().AsNoTracking()
                        .Where(a => !a.DeleteFlag && a.LocationId.HasValue && (a.Inmate.Incarceration.Any(inc =>
                             !inc.ReleaseOut.HasValue && inc.BookCompleteFlag.HasValue && inc.IntakeCompleteFlag.HasValue &&
                             !inc.BookAndReleaseFlag.HasValue) && a.Inmate.FacilityId == inputs.FacilityId && a.Inmate.InmateActive == 1 &&
                             a.Inmate.InmateClassificationId.HasValue
                             && a.Inmate.InmateClassification.InmateClassificationReason == RequestType.PENDINGREQUEST)).ToList();
                    break;
                case InventoryQueueConstants.INTAKE:
                    lstScheduleInmate = _context.Schedule.OfType<ScheduleCourt>().AsNoTracking()
                        .Where(a => !a.DeleteFlag && a.LocationId.HasValue && (a.Inmate.Incarceration.Any(inc =>
                            !inc.IntakeCompleteFlag.HasValue && !inc.BookAndReleaseFlag.HasValue &&
                            !inc.ReleaseOut.HasValue) && a.Inmate.FacilityId == inputs.FacilityId && a.Inmate.InmateActive == 1))
                        .ToList();
                    break;
                case InventoryQueueConstants.BOOKING:
                    lstScheduleInmate = _context.Schedule.OfType<ScheduleCourt>().AsNoTracking()
                        .Where(a => !a.DeleteFlag && a.LocationId.HasValue && (a.Inmate.Incarceration.Any(inc =>
                        !inc.IntakeCompleteFlag.HasValue && !inc.BookAndReleaseFlag.HasValue && !inc.BookCompleteFlag.HasValue &&
                        !inc.ReleaseOut.HasValue) && a.Inmate.InmateActive == 1 && a.Inmate.FacilityId == inputs.FacilityId)).ToList();
                    break;
                case ClassifyQueueConstants.SPECIALQUEUE:
                    lstScheduleInmate = _context.Schedule.OfType<ScheduleCourt>().AsNoTracking()
                        .Where(a => !a.DeleteFlag && a.LocationId.HasValue && (a.Inmate.Incarceration.Any(inc =>
                         !inc.ReleaseOut.HasValue) && a.Inmate.InmateActive == 1 && a.Inmate.FacilityId == inputs.FacilityId &&
                          a.Inmate.SpecialClassQueueInterval > 0)).ToList();
                    break;
                case ClassifyQueueConstants.REVIEWQUEUE:
                    lstScheduleInmate = _context.Schedule.OfType<ScheduleCourt>().AsNoTracking()
                        .Where(a => !a.DeleteFlag && a.LocationId.HasValue && (a.Inmate.Incarceration.Any(inc =>
                         !inc.ReleaseOut.HasValue && !inc.BookAndReleaseFlag.HasValue) && a.Inmate.InmateActive == 1 &&
                         a.Inmate.FacilityId == inputs.FacilityId && !a.Inmate.LastClassReviewDate.HasValue ||
                         a.Inmate.LastClassReviewDate <= DateTime.Now) && a.Inmate.InmateClassification.InmateClassificationReason != null)
                        .ToList();
                    break;
            }
            List<AoAppointmentVm> schList = lstScheduleInmate.Select(sch => new AoAppointmentVm
            {
                StartDate = sch.StartDate,
                EndDate = sch.EndDate,
                ScheduleId = sch.ScheduleId,
                InmateId = sch.InmateId ?? 0,
                TypeId = sch.TypeId,
                Duration = sch.Duration,
                IsSingleOccurrence = sch.IsSingleOccurrence,
                DayInterval = sch.DayInterval,
                WeekInterval = sch.WeekInterval,
                FrequencyType = sch.FrequencyType,
                QuarterInterval = sch.QuarterInterval,
                MonthOfQuarterInterval = sch.MonthOfQuarterInterval,
                MonthOfYear = sch.MonthOfYear,
                DayOfMonth = sch.DayOfMonth,
            }).ToList();

            foreach (AoAppointmentVm sch in schList)
            {
                ScheduleBuilder schBuilder = new ScheduleBuilder();
                schBuilder.StartDate(sch.StartDate);
                if (sch.EndDate.HasValue)
                {
                    schBuilder.EndDate(sch.EndDate.Value);
                }

                if (sch.EndDate != null && (sch.IsSingleOccurrence && (sch.StartDate.Date != sch.EndDate.Value.Date)))
                {
                    sch.IsSingleOccurrence = false;
                    sch.FrequencyType = FrequencyType.Daily;
                }
                ISchedule materializedSchedule = schBuilder
                    .Duration(sch.Duration)
                    .SingleOccurrence(sch.IsSingleOccurrence)
                    .OnDaysOfWeek(sch.DayInterval)
                    .DuringMonth(sch.WeekInterval)
                    .HavingFrequency(sch.FrequencyType)
                    .SetMonthOfYear(sch.MonthOfYear)
                    .SetDayOfMonth(sch.DayOfMonth ?? 0)
                    .DuringMonthOfQuarter(sch.MonthOfQuarterInterval)
                    .DuringQuarter(sch.QuarterInterval)
                    .Create();

                Debug.Assert(materializedSchedule.EndDate != null,
                    "AppointmentService: materializedSchedule.EndDate != null");
                DateRange during = new DateRange(materializedSchedule.StartDate,
                    sch.EndDate.HasValue ? materializedSchedule.EndDate.Value : DateTime.Now.AddYears(5));

                if (sch.EndDate != null && (materializedSchedule.IsSingleOccurrence && (sch.StartDate.Date == sch.EndDate.Value.Date)))
                {
                    AoAppointmentVm app = sch;
                    app.StartDate = materializedSchedule.StartDate;
                    app.EndDate = materializedSchedule.EndDate;
                    inmateAppList.Add(app);
                }
                else
                {
                    inmateAppList.AddRange(materializedSchedule.Occurrences(during)
                        .Select(date => new AoAppointmentVm
                        {
                            StartDate = date.Date.Add(during.StartDateTime.TimeOfDay),
                            Duration = sch.Duration,
                            EndDate = sch.EndDate.HasValue
                                ? date.Date.Add(during.EndDateTime.TimeOfDay)
                                : date.Date.Add(during.StartDateTime.TimeOfDay).Add(sch.Duration),
                            EndDateNull = !sch.EndDate.HasValue,
                            ScheduleId = sch.ScheduleId,
                            InmateId = sch.InmateId,
                            TypeId = sch.TypeId,
                            IsSingleOccurrence = sch.IsSingleOccurrence,
                            FacilityId = sch.FacilityId,
                        }));
                }
            }

            inmateAppList = inmateAppList.Where(s => s.StartDate > DateTime.Now).Select(s => new AoAppointmentVm
            {
                StartDate = s.StartDate,
                InmateId = s.InmateId
            }).ToList();

            return inmateAppList;
        }
        private List<QueueDetails> GetQueueDetailsFromFacilityIdStoredProcedure(int facilityId, string inputFlag = "READY FOR INITIAL")
        {
            List<QueueDetails> result = new List<QueueDetails>();
            List<SqlParameter> parameters = new List<SqlParameter>();
            parameters.Add(new SqlParameter("@FacilityId", facilityId));
            parameters.Add(new SqlParameter("@InputFlag", inputFlag));
            DataTable resultTable = _commonService.RunStoredProcedure("getClassificationQueues", parameters);
            foreach (DataRow r in resultTable.Rows)
            {
                result.Add(new QueueDetails
                {
                    InmateId = Convert.ToInt32(r["InmateId"]),
                    LastClassReviewDate = DBNull.Value.Equals(r["LastClassReviewDate"]) ? (DateTime?)null 
                    : Convert.ToDateTime(r["LastClassReviewDate"]),
                    InmateClassificationId = Convert.ToInt32(r["InmateCLassificationId"]),
                    InmateClassificationReason = r["InmateClassificationReason"].ToString(),
                    Description = r["Description"].ToString()
                });
            }
            return result;
        }

        //Manage Special Queue History Details
        public List<SpecialQueueHistory> ManageSpecialQueueHistory(SpecialQueueInputs inputs)
        {
            List<SpecialQueueHistory> lstQueue = new List<SpecialQueueHistory>();
            if (inputs.FacilityId <= 0) return lstQueue;
            //To get SpecialClassQueueSaveHistory details depend on facility Id
            IQueryable<SpecialClassQueueSaveHistory> history =
                _context.SpecialClassQueueSaveHistory.Where(s => s.FacilityId == inputs.FacilityId);
            history = inputs.Count == 100
                ? history.Take(100)
                : history.Where(h => h.SaveDate >= inputs.FromDate && h.SaveDate <= inputs.ToDate.AddDays(1).AddTicks(-1));
            //Filter depend on Inmate Id
            if (inputs.InmateId > 0)
            {
                history = history.Where(h => h.InmateId == inputs.InmateId);
            }
            //Filter depend on Officer Id
            if (inputs.OfficerId > 0)
            {
                history = history.Where(h => h.SaveBy == inputs.OfficerId);
            }
            lstQueue = history.Where(i => i.Inmate.InmateId > 0).Select(i => new SpecialQueueHistory
            {
                InmateId = i.InmateId,
                Interval = i.SpecialClassQueueInterval ?? 0, //SpecialClassQueueInterval
                OfficerId = i.SaveBy,
                SaveDate = i.SaveDate
            }).ToList();
            //To get inmate Details List
            List<PersonVm> lstPersonDetails =
                _inmateService.GetInmateDetails(lstQueue.Select(i => i.InmateId).ToList());
            //To get Officer Details List
            List<PersonnelVm> arrestOfficer =
                _personService.GetPersonNameList(lstQueue.Select(i => i.OfficerId).ToList());
            lstQueue.ForEach(item =>
            {
                item.InmateDetails = lstPersonDetails.Single(inm => inm.InmateId == item.InmateId);
                item.OfficerDetails = arrestOfficer.Single(arr => arr.PersonnelId == item.OfficerId);
            });
            return lstQueue;
        }

        //Insert SpecialClassQueueSaveHistory Table
        public async Task<int> InsertSpecialClassQueue(SpecialQueueInputs inputs)
        {
            //For Delete Functionality SpecialClassQueueInterval Must Be Zero
            SpecialClassQueueSaveHistory queue = new SpecialClassQueueSaveHistory
            {
                InmateId = inputs.InmateId,
                SaveBy = _personnelId,
                SaveDate = DateTime.Now,
                SpecialClassQueueInterval = inputs.Interval,
                FacilityId = inputs.FacilityId
            };
            _context.Add(queue);
            //update inmate SpecialClassQueueInterval
            Inmate inmate = _context.Inmate.Single(i => i.InmateId == inputs.InmateId);
            inmate.SpecialClassQueueInterval = inputs.Interval;
            await _context.SaveChangesAsync();
            return queue.SpecialClassQueueSaveHistoryId;
        }

        //Save Method For Classification Batch Review Popup
        public async Task<int> InsertClassificationNarrative(QueueInputs inputs)
        {
            //insert InmateClassificationNarrative table
            List<InmateClassificationNarrative> inmateClassificationNarrative = inputs.LstInmateIds.Select(s =>
                new InmateClassificationNarrative
                {
                    InmateId = s,
                    ReviewFlag = 1,
                    Narrative = inputs.Narrative,
                    CreatedBy = _personnelId,
                    CreateDate = DateTime.Now
                }
            ).ToList();
            _context.InmateClassificationNarrative.AddRange(inmateClassificationNarrative);
            //update inmate details
            List<Inmate> inmateList = _context.Inmate.Where(inm => inputs.LstInmateIds.Contains(inm.InmateId)).ToList();
            inmateList.ForEach(i =>
            {
                i.LastClassReviewDate = DateTime.Now;
                i.LastClassReviewBy = _personnelId;
            });
            return await _context.SaveChangesAsync();
        }
    }
}
