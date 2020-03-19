using System;
using System.Collections.Generic;
using System.Linq;
using GenerateTables.Models;
using ServerAPI.ViewModels;

namespace ServerAPI.Services
{
    public class TrackFurlService : ITrackFurlService
    {
        private readonly AAtims _context;

        private readonly ICommonService _commonService;
        private readonly IInmateService _inmateService;
        private readonly IFacilityPrivilegeService _facilityPrivilegeService;
        private readonly IPhotosService _photoService;
        private readonly IFacilityHousingService _facilityHousingService;

        public TrackFurlService(AAtims context, ICommonService commonService, IInmateService inmateService,
            IFacilityPrivilegeService facilityPrivilegeService,IPhotosService photoService,
            IFacilityHousingService facilityHousingService)
        {
            _context = context;
            _commonService = commonService;
            _inmateService = inmateService;
            _facilityPrivilegeService = facilityPrivilegeService;
            _photoService = photoService;
            _facilityHousingService = facilityHousingService;
        }

        public WorkCrewFurloughTracking GetFurloughDetails(int facilityId)
        {
            WorkCrewFurloughTracking workCrewFurloughTracking = new WorkCrewFurloughTracking
            {
                LstFurloughDetails = _context.WorkCrew
                    .Where(w => w.WorkCrewId > 0 && w.WorkCrewLookup.DeleteFlag == 0 &&
                                w.WorkCrewLookup.WorkFurloughFlag.HasValue &&
                                (w.EndDate == null || w.EndDate >= DateTime.Now)
                                && w.InmateNavigation.InmateActive == 1 && w.InmateNavigation.WorkCrewId > 0
                                && w.WorkCrewLookup.FacilityId == facilityId)
                    .Select(w => new WorkCrewFurlVm
                    {
                        WorkCrewId = w.WorkCrewId,
                        WorkCrewName = w.WorkCrewLookup.CrewName,
                        StartDate = w.StartDate,
                        EndDate = w.EndDate,
                        DeleteFlag = w.DeleteFlag,
                        Comment = w.Comment,
                        InmateId = w.InmateNavigation.InmateId,
                        InmateNumber = w.InmateNavigation.InmateNumber,
                        WorkCrewLockerId = w.WorkCrewLockerId,
                        TodayStart = w.FurloughSchdTodayStart,
                        TodayEnd = w.FurloughSchdTodayEnd,
                        PersonId=w.InmateNavigation.PersonId
                    }).ToList()
            };

            List<int> lstInmateId = workCrewFurloughTracking.LstFurloughDetails.Select(wc => wc.InmateId).ToList();

            //InmateTrackDetails
            List<InmateTrakVm> lstInmateTrack = _context.InmateTrak.Where(it => lstInmateId.Contains(it.InmateId))
                .Select(it => new InmateTrakVm
                {
                    InmateId = it.InmateId,
                    InmateTrackId = it.InmateTrakId,
                    InmateCurrentTrack = it.Inmate.InmateCurrentTrack,
                    InmateTrackNote = it.InmateTrakNote,
                    InmateCurrentTrackId = it.Inmate.InmateCurrentTrackId,
                    InmateTrackDateIn=it.InmateTrakDateIn
                }).ToList();

            //Get Person and Housing details by list of inmate id
            List<PersonVm> lstPersonDetails = _inmateService.GetInmateDetails(lstInmateId);
            List<HousingDetail> lstHousingDetail = _facilityHousingService.GetHousingList(lstPersonDetails
                .Where(i => i.HousingUnitId.HasValue).Select(i => i.HousingUnitId.Value).ToList());

            //Get list of personId and get photo path details
            int[] lstPersonId = lstPersonDetails.Select(p => p.PersonId).ToArray();
            List<WorkCrewIdentifier> lstIdentifier = _context.Identifiers
                .Where(i => lstPersonId.Contains(i.PersonId.Value))
                .Select(i => new WorkCrewIdentifier
                {
                    PhotoGraphPathAbsolute = i.PhotographPathAbsolute,
                    PhotoGraphPath = i.PhotographPath,
                    PhotoGraphRelativePath = i.PhotographRelativePath,
                    IdentifierId = i.IdentifierId,
                    PersonId=i.PersonId??0
                }).ToList();

            //Get List of workcrewtrack id by list of workcrew id's
            int[] lstWorkCrewId = workCrewFurloughTracking.LstFurloughDetails.Select(w => w.WorkCrewId).ToArray();

            List<WorkCrewTrackXrefVm> lstXrefDet = _context.WorkCrewTrackXref
                .Where(xrf => lstWorkCrewId.Contains(xrf.WorkCrewId))
                .OrderByDescending(o=>o.InmateWorkcrewXrefId)
                .Select(xrf => new WorkCrewTrackXrefVm
                {
                    WorkCrewId = xrf.WorkCrewId,
                    WorkCrewTrackId = xrf.InmateTrakId
                }).ToList();

            workCrewFurloughTracking.LstFurloughDetails.ForEach(item =>
            {
                //To get location details              
                item.InmateTrak = lstInmateTrack.OrderByDescending(i => i.InmateTrackId)
                    .Where(i => i.InmateId == item.InmateId)
                    .Select(i => new InmateTrakVm
                    {
                        InmateCurrentTrack = i.InmateCurrentTrack,
                        InmateTrackNote = i.InmateTrackNote,
                        InmateCurrentTrackId = i.InmateCurrentTrackId,
                        InmateTrackDateIn=i.InmateTrackDateIn
                    }).FirstOrDefault();

                //Person and Housing Details
                item.Person = lstPersonDetails.Single(p => p.InmateId == item.InmateId);
                if (item.Person.HousingUnitId.HasValue)
                {
                    item.HousingDetail =
                        lstHousingDetail.Single(hu => hu.HousingUnitId == item.Person.HousingUnitId.Value);
                }
                else
                {
                    item.HousingDetail = new HousingDetail
                    {
                        FacilityAbbr = _context.Facility.Single(f => f.FacilityId == facilityId).FacilityAbbr
                    };
                }

                //get photo path               
                if (lstIdentifier.Count > 0)
                {
                    item.WorkCrewIdentifier = lstIdentifier
                        .Where(w => w.PersonId == item.PersonId).OrderByDescending(i => i.IdentifierId)
                        .Select(i => new WorkCrewIdentifier
                        {
                            PhotoGraphPathAbsolute = i.PhotoGraphPathAbsolute,
                            PhotoGraphPath = i.PhotoGraphPath,
                            PhotoGraphRelativePath = _photoService.GetPhotoByIdentifierId(i.IdentifierId),
                            IdentifierId = i.IdentifierId
                        }).FirstOrDefault();
                }

                item.LockerName = _context.WorkCrewLocker
                    .SingleOrDefault(w => w.WorkCrewLockerId == item.WorkCrewLockerId)?.LockerName;

                if (lstXrefDet.Count <= 0) return;
                //Assign inmate track id
                WorkCrewTrackXrefVm workCrewTrackXref =
                    lstXrefDet.FirstOrDefault(xrf => xrf.WorkCrewId == item.WorkCrewId);
                item.InmateTrackId = workCrewTrackXref is null ? 0 : workCrewTrackXref.WorkCrewTrackId;
            });

            
            workCrewFurloughTracking.LstFurloughCountDetails = GetFurloughCount(facilityId, null);
            workCrewFurloughTracking.LstPrivileges = _facilityPrivilegeService.GetPrivilegeList(facilityId);
            return workCrewFurloughTracking;
        }

        public List<WorkCrewFurloughCountVm> GetFurloughCount(int facilityId, List<int> housingListId)
        {
            List<WorkCrewFurloughCountVm> lstFurloughDetails = _context.WorkCrew
                .Where(w => w.WorkCrewLookup.DeleteFlag == 0 && w.InmateNavigation.InmateActive == 1 &&
                            w.InmateNavigation.WorkCrewId > 0 && w.WorkCrewLookup.WorkFurloughFlag == 1 &&
                            (w.EndDate == null || w.EndDate >= DateTime.Now) &&
                            (facilityId == 0 || w.WorkCrewLookup.FacilityId == facilityId)
                            && (housingListId == null ||
                            (housingListId.Contains(w.InmateNavigation.HousingUnit.HousingUnitListId))))
                .Select(w => new WorkCrewFurloughCountVm
                {
                    WorkCrewName = w.WorkCrewLookup.CrewName,
                    WorkCrewLookupId = w.WorkCrewLookupId,
                    TodayStart = w.FurloughSchdTodayStart,
                    TodayEnd = w.FurloughSchdTodayEnd,
                    CrewCapacity = w.WorkCrewLookup.CrewCapacity
                }).ToList();
            
            List<WorkCrewFurloughCountVm> lstFurlCount = lstFurloughDetails
                .GroupBy(g => new {g.WorkCrewLookupId, g.TodayStart, g.TodayEnd})
                .Select(g => new WorkCrewFurloughCountVm
                {
                    WorkCrewFurloughCount = g.Count(),
                    WorkCrewLookupId = g.Key.WorkCrewLookupId,
                    TodayStart = g.Key.TodayStart
                }).ToList();

            lstFurloughDetails.ForEach(item =>
            {
                item.WorkCrewFurloughCount = lstFurlCount.Single(cnt =>
                        cnt.WorkCrewLookupId == item.WorkCrewLookupId && cnt.TodayStart == item.TodayStart)
                    .WorkCrewFurloughCount;
                List<WorkFurlRequestVm> lstFurlRequest = _context.WorkCrewRequest
                    .Where(r => r.WorkCrewLookupId == item.WorkCrewLookupId)
                    .Select(r => new WorkFurlRequestVm
                    {
                        DeleteFlag = r.DeleteFlag,
                        WorkCrewRequestId = r.WorkCrewRequestId
                    }).ToList();
                item.WorkCrewRequestCount = lstFurlRequest.Count;
            });

            lstFurloughDetails = lstFurloughDetails.GroupBy(g => new {g.WorkCrewLookupId, g.TodayStart, g.TodayEnd})
                .Select(g => g.FirstOrDefault()).ToList();

            int[] workCrewLookupIds = lstFurloughDetails.Select(wc => wc.WorkCrewLookupId).ToArray();

            List<WorkCrewFurloughCountVm> lstFurloughDetails1 = _context.WorkCrewRequest
                .Where(wcr => wcr.WorkCrewLookup.DeleteFlag == 0
                              && wcr.WorkCrewLookup.WorkFurloughFlag == 1 &&
                              !workCrewLookupIds.Contains(wcr.WorkCrewLookupId ?? 0)
                              && (facilityId == 0 ||
                                  wcr.WorkCrewLookup.FacilityId == facilityId)
                              && (housingListId==null ||
                            (housingListId.Contains(wcr.WorkCrew.InmateNavigation.HousingUnit.HousingUnitListId))))
                .Select(wcr => new WorkCrewFurloughCountVm
                {
                    WorkCrewName = wcr.WorkCrewLookup.CrewName,
                    WorkCrewLookupId = wcr.WorkCrewLookupId ?? 0,
                    CrewCapacity = wcr.WorkCrewLookup.CrewCapacity
                }).ToList();

            List<WorkCrewFurloughCountVm> lstFurlCount1 = lstFurloughDetails1
                .GroupBy(g => new {g.WorkCrewName, g.TodayStart, g.TodayEnd}).Select(
                    g => new WorkCrewFurloughCountVm
                    {
                        WorkCrewName = g.Key.WorkCrewName,
                        WorkCrewRequestCount = g.Count()
                    }).ToList();
            lstFurloughDetails1 = lstFurloughDetails1.GroupBy(g => new {g.WorkCrewName, g.TodayStart, g.TodayEnd})
                .Select(g => g.FirstOrDefault())
                .ToList();

            lstFurloughDetails1.ForEach(item =>
            {
                item.WorkCrewRequestCount =
                    lstFurlCount1.Single(c => c.WorkCrewName == item.WorkCrewName).WorkCrewRequestCount;
            });
            lstFurloughDetails.AddRange(lstFurloughDetails1);
            return lstFurloughDetails;
        }
    }
}
