using System;
using System.Collections.Generic;
using System.Linq;
using ServerAPI.ViewModels;
using GenerateTables.Models;
using ServerAPI.Utilities;

namespace ServerAPI.Services
{
    public class WorkCrewService : IWorkCrewService
    {
        private readonly AAtims _context;

        private readonly ICommonService _commonService;
        private readonly IFacilityPrivilegeService _facilityPrivilegeService;
        private readonly IPhotosService _photos;

        public WorkCrewService(AAtims context, ICommonService commonService,
            IFacilityPrivilegeService facilityPrivilegeService, IPhotosService photosService)
        {
            _context = context;
            _commonService = commonService;
            _facilityPrivilegeService = facilityPrivilegeService;
            _photos = photosService;
        }

        //To get workcrew tracking count
        public List<WorkCrewVm> GetWorkCrewEntriesCount(int facilityId, List<int> housingUnitListId)
        {
            IQueryable<WorkCrew> dbWorkCrew =
                _context.WorkCrew.Where(wc => wc.WorkCrewLookup.DeleteFlag == 0
                                              && (!wc.EndDate.HasValue || wc.EndDate.Value >= DateTime.Now)
                                              && wc.InmateNavigation.InmateActive == 1
                                              && wc.InmateNavigation.WorkCrewId > 0
                                              && (wc.WorkCrewLookup.WorkFurloughFlag == 0 ||
                                                  !wc.WorkCrewLookup.WorkFurloughFlag.HasValue)
                                              && (facilityId == 0 || wc.WorkCrewLookup.FacilityId == facilityId)
                                              && (wc.WorkCrewLookup.CrewSchdAnytime == 1
                                                  || wc.WorkCrewLookup.CrewSchdTodayStart != null &&
                                                  wc.WorkCrewLookup.CrewSchdTodayEnd != null)).OrderBy(o=>o.WorkCrewLookup.CrewName);

            List<WorkCrewVm> workCrewDetails = dbWorkCrew.Where(wcr => wcr.WorkCrewLookup.CrewSchdAnytime == 1 
                && (housingUnitListId.Count == 0||
                housingUnitListId.Contains(wcr.InmateNavigation.HousingUnit.HousingUnitListId) && wcr.InmateNavigation.WorkCrewId.HasValue))
                .Select(wcr => new WorkCrewVm
                {
                    WorkCrewName = wcr.WorkCrewLookup.CrewName,
                    WorkCrewCapacity = wcr.WorkCrewLookup.CrewCapacity ?? 0,
                    WorkCrewLookupId = wcr.WorkCrewLookupId,
                    SchdAnytime=wcr.WorkCrewLookup.CrewSchdAnytime==1
                }).ToList();

            List<WorkCrewVm> details = workCrewDetails;
            workCrewDetails.ForEach(wcd => wcd.WorkCrewCount = details.Count(wc =>
                wc.WorkCrewLookupId == wcd.WorkCrewLookupId));

            List<WorkCrewVm> workCrewDaysDetails = dbWorkCrew
                .Where(wcr => wcr.WorkCrewLookup.CrewSchdAnytime != 1
                              && wcr.WorkCrewLookup.CrewSchdTodayStart != null &&
                              wcr.WorkCrewLookup.CrewSchdTodayEnd != null
                              && (housingUnitListId.Count == 0 ||
                              housingUnitListId.Contains(wcr.InmateNavigation.HousingUnit.HousingUnitListId) && wcr.InmateNavigation.WorkCrewId.HasValue))
                .Select(wcr => new WorkCrewVm
                {
                    WorkCrewName = wcr.WorkCrewLookup.CrewName,
                    WorkCrewCapacity = wcr.WorkCrewLookup.CrewCapacity ?? 0,
                    WorkCrewLookupId = wcr.WorkCrewLookupId,
                    StartDateHours = wcr.WorkCrewLookup.CrewSchdTodayStart.ToString(),
                    EndDateHours = wcr.WorkCrewLookup.CrewSchdTodayEnd.ToString()
                }).ToList();

            workCrewDaysDetails.ForEach(wcd => wcd.WorkCrewCount = workCrewDaysDetails.Count(wc =>
                wc.WorkCrewLookupId == wcd.WorkCrewLookupId));

            workCrewDetails.AddRange(workCrewDaysDetails);

            workCrewDetails = workCrewDetails.GroupBy(wcd => new
            {
                wcd.WorkCrewLookupId,
                wcd.WorkCrewName,
                wcd.WorkCrewCapacity,
                wcd.StartDateHours,
                wcd.EndDateHours,
                wcd.SchdAnytime
            }).Select(wd => wd.FirstOrDefault()).ToList();

            return workCrewDetails;
        }

        //To get workcrew inmate tracking details
        public WorkCrewDetailsVm GetWorkcrewInmateDetails(int facilityId)
        {
            IQueryable<WorkCrewFurlVm> lstWorkCrewTrack =
                _context.WorkCrew.Where(wc => wc.WorkCrewLookup.DeleteFlag == 0
                                              && (!wc.EndDate.HasValue || wc.EndDate.Value >= DateTime.Now)
                                              && wc.InmateNavigation.InmateActive == 1
                                              && wc.InmateNavigation.WorkCrewId > 0
                                              && (wc.WorkCrewLookup.WorkFurloughFlag == 0 ||
                                                  !wc.WorkCrewLookup.WorkFurloughFlag.HasValue)
                                              && (facilityId == 0 || wc.WorkCrewLookup.FacilityId == facilityId)
                                              && (wc.WorkCrewLookup.CrewSchdAnytime == 1
                                                  || wc.WorkCrewLookup.CrewSchdTodayStart != null &&
                                                  wc.WorkCrewLookup.CrewSchdTodayEnd != null)).Select(wc =>
                    new WorkCrewFurlVm
                    {
                        WorkCrewId = wc.WorkCrewId,
                        WorkCrewName = wc.WorkCrewLookup.CrewName,
                        WorkCrewLockerId = wc.WorkCrewLockerId,
                        StartDate = wc.StartDate,
                        EndDate = wc.EndDate,
                        DeleteFlag = wc.DeleteFlag,
                        Comment = wc.Comment,
                        FacilityId = wc.WorkCrewLookup.FacilityId,
                        InmateId = wc.InmateId,
                        InmateNumber = wc.InmateNavigation.InmateNumber,
                        HousingDetail = new HousingDetail
                        {
                            HousingUnitLocation = wc.InmateNavigation.HousingUnit.HousingUnitLocation,
                            HousingUnitNumber = wc.InmateNavigation.HousingUnit.HousingUnitNumber,
                            HousingUnitBedLocation = wc.InmateNavigation.HousingUnit.HousingUnitBedLocation,
                            HousingUnitBedNumber = wc.InmateNavigation.HousingUnit.HousingUnitBedNumber,
                            FacilityAbbr = _context.Facility.Single(f=> f.FacilityId == facilityId).FacilityAbbr
                        },
                        Person = new PersonVm
                        {
                            PersonId = wc.InmateNavigation.PersonId
                        },
                        TodayStart = wc.WorkCrewLookup.CrewSchdTodayStart,
                        TodayEnd = wc.WorkCrewLookup.CrewSchdTodayEnd,
                        SchdAnytime = wc.WorkCrewLookup.CrewSchdAnytime == 1
                    });

            List<int> inmateIdLst = lstWorkCrewTrack.Select(id => id.InmateId).ToList();
            List<int> personIdLst = lstWorkCrewTrack.Select(pid => pid.Person.PersonId).Distinct().ToList();
            List<int> wcLockerIdLst = lstWorkCrewTrack.Select(wcl => wcl.WorkCrewLockerId ?? 0).ToList();

            //Get List of workcrewtrack id by list of workcrew id's
            int[] lstWorkCrewId = lstWorkCrewTrack.Select(w => w.WorkCrewId).ToArray();
            List<WorkCrewTrackXrefVm> lstXrefDet = _context.WorkCrewTrackXref
                .Where(xrf => lstWorkCrewId.Contains(xrf.WorkCrewId))
                .Select(xrf => new WorkCrewTrackXrefVm
                {
                    WorkCrewId = xrf.WorkCrewId,
                    WorkCrewTrackId = xrf.InmateTrakId
                }).ToList();

            //InmateTrackDetails
            List<InmateTrakVm> lstInmateTrack = _context.InmateTrak.Where(it => inmateIdLst.Contains(it.InmateId))
                .Select(it => new InmateTrakVm
                {
                    InmateId = it.InmateId,
                    InmateTrackId = it.InmateTrakId,
                    InmateCurrentTrack = it.Inmate.InmateCurrentTrack,
                    InmateTrackNote = it.InmateTrakNote,
                    InmateCurrentTrackId = it.Inmate.InmateCurrentTrackId,
                    InmateTrackDateIn = it.InmateTrakDateIn
                }).ToList();

            // To get person details
            List<PersonVm> dbPersonLst = _context.Person.Where(pr =>
                personIdLst.Contains(pr.PersonId)).Select(pr =>
                new PersonVm
                {
                    PersonId = pr.PersonId,
                    PersonFirstName = pr.PersonFirstName,
                    PersonLastName = pr.PersonLastName
                }).ToList();

            // To get person Identifiers(Photo path)            
            List<WorkCrewIdentifier> dbIdentifiers = _context.Identifiers.Where(idf =>
                idf.IdentifierType == "1" &&
                idf.DeleteFlag == 0 && personIdLst.Contains(idf.PersonId ?? 0)).Select(idf =>
                new WorkCrewIdentifier
                {
                    PersonId = idf.PersonId ?? 0,
                    PhotoGraphRelativePath = _photos.GetPhotoByIdentifier(idf),
                    IdentifierId = idf.IdentifierId
                }).OrderByDescending(i => i.IdentifierId).ToList();

            // To get workcrew locker details
            List<WorkCrewLocker> dbWcLocker = _context.WorkCrewLocker.Where(wc =>
                wcLockerIdLst.Contains(wc.WorkCrewLockerId)).Select(wc =>
                new WorkCrewLocker
                {
                    WorkCrewLockerId = wc.WorkCrewLockerId,
                    LockerName = wc.LockerName
                }).ToList();

            WorkCrewDetailsVm lstWorkCrew = new WorkCrewDetailsVm {LstWorkCrewFurl = lstWorkCrewTrack.ToList()};
            lstWorkCrew.LstWorkCrewFurl.ForEach(item =>
            {
                item.LockerName = dbWcLocker
                    .SingleOrDefault(w => w.WorkCrewLockerId == item.WorkCrewLockerId)?.LockerName;
                item.Person = dbPersonLst
                    .SingleOrDefault(pId => pId.PersonId == item.Person.PersonId);
                item.Photofilepath = dbIdentifiers.OrderByDescending(o => o.IdentifierId)
                    .FirstOrDefault(po => po.PersonId == item.Person.PersonId)?.PhotoGraphRelativePath; 
                item.InmateTrak = lstInmateTrack.OrderByDescending(i => i.InmateTrackId)
                   .Where(i => i.InmateId == item.InmateId)
                   .Select(i => new InmateTrakVm
                   {
                       InmateCurrentTrack = i.InmateCurrentTrack,
                       InmateTrackNote = i.InmateTrackNote,
                       InmateCurrentTrackId = i.InmateCurrentTrackId,
                       InmateTrackDateIn = i.InmateTrackDateIn
                   }).FirstOrDefault();

                if (lstXrefDet.Count <= 0) return;
                //Assign inmate track id
                WorkCrewTrackXrefVm workCrewTrackXref =
                    lstXrefDet.FirstOrDefault(xrf => xrf.WorkCrewId == item.WorkCrewId);
                item.InmateTrackId = workCrewTrackXref?.WorkCrewTrackId ?? 0;
            });
            lstWorkCrew.LstPrivileges = _facilityPrivilegeService.GetPrivilegeList(facilityId);
            return lstWorkCrew;
        }
    }
}