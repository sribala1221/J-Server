﻿using GenerateTables.Models;
using ServerAPI.Utilities;
using ServerAPI.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;

// ReSharper disable once CheckNamespace
namespace ServerAPI.Services
{
    public class TrackHousingService : ITrackHousingService
    {
        private readonly AAtims _context;
        private readonly ICommonService _commonService;
        private readonly IInmateService _inmateService;
        private readonly IPhotosService _photo;
        private readonly ILockdownService _lockdownService;

        public TrackHousingService(AAtims context, ICommonService commonService, IInmateService inmateService,
            IPhotosService photosService, ILockdownService lockdownService)
        {
            _context = context;
            _commonService = commonService;
            _inmateService = inmateService;
            _photo = photosService;
            _lockdownService = lockdownService;
        }

        public TrackHousingDetailVm GetTrackingLocation(int facilityId, int housingNumber = 0, int housingGroup = 0)
        {
            IQueryable<Inmate> lstInmate =
                _context.Inmate.Where(w => w.InmateActive == 1 && w.InmateCurrentTrackId.HasValue && w.FacilityId == facilityId);

            if (housingGroup > 0)
            {
                List<int> housingUnitListIds = _context.HousingGroupAssign
                    .Where(w => w.HousingGroupId == housingGroup && w.HousingUnitListId > 0)
                    .Select(s => s.HousingUnitListId ?? 0).ToList();

                List<int> locationIds = _context.HousingGroupAssign
                    .Where(w => w.HousingGroupId == housingGroup && w.LocationId > 0)
                    .Select(s => s.LocationId ?? 0).ToList();

                IQueryable<Inmate> lstHousingInmates = lstInmate.Where(w => housingUnitListIds.Contains(w.HousingUnit.HousingUnitListId));
                
                IQueryable<Inmate> lstLocationInmates = lstInmate.Where(w => locationIds.Contains(w.InmateCurrentTrackId??0));
               
                lstInmate = lstHousingInmates.Concat(lstLocationInmates);
            }
            else if (housingNumber > 0)
            {
                lstInmate = lstInmate.Where(w => w.HousingUnit.HousingUnitListId == housingNumber);
            }
            List<AppointmentTracking> appointmentTrakList = _context.InmateAppointmentTrack
                .Where(w => w.InmateTrakDateIn == null).Select(s =>
                    new AppointmentTracking
                    {
                        AppointmentEndDate = s.Schedule.EndDate,
                        ScheduleId = s.Schedule.ScheduleId,
                        ApptLocationId = s.Schedule.LocationId,
                        ApptStartDate = s.Schedule.StartDate.ToString("HH:mm"),
                        ReoccurFlag = s.Schedule.IsSingleOccurrence,
                        InmateId = s.InmateId
                    }).ToList();

            List<int> aoScheduleIds = appointmentTrakList.Select(s => s.ScheduleId).ToList();

            List<InmateTrak> lstinmateTraks = _context.InmateTrak.Where(w => !w.InmateTrakDateIn.HasValue)
                .OrderByDescending(w => w.InmateTrakId).ToList();
            List<InmateAppointmentTrack> lstInmateApptTraks = _context.InmateAppointmentTrack
                .Where(w => !w.InmateTrakDateIn.HasValue).ToList();

            TrackHousingDetailVm trackHousingDetailVm = new TrackHousingDetailVm
            {
                LstTrackCheckedOut = lstInmate.Where(w =>
                        w.InmateCurrentTrackNavigation.InactiveFlag == 0 &&
                        w.InmateCurrentTrackNavigation.FacilityId == facilityId
                        && !w.InmateCurrentTrackNavigation.TransferFlag.HasValue
                        && !w.InmateCurrentTrackNavigation.HousingUnitListId.HasValue)
                    .OrderBy(o => o.HousingUnit.HousingUnitLocation).ThenBy(t => t.HousingUnit.HousingUnitNumber)
                    .Select(
                        s => new TrackHousingLocationVm
                        {
                            InmateId = s.InmateId,
                            LocationId = s.InmateCurrentTrackId ?? 0,
                            Location = s.InmateCurrentTrack,
                            DateOut = lstinmateTraks.Where(w => w.InmateId == s.InmateId)
                                .Select(se => se.InmateTrakDateOut).FirstOrDefault(),
                            EnrouteStartOut = lstinmateTraks.Where(w => w.InmateId == s.InmateId)
                            .Select(se => se.EnrouteStartOut)
                            .FirstOrDefault(),
                            TrakNotes = lstinmateTraks.Where(w => w.InmateId == s.InmateId)
                                .Select(se => se.InmateTrakNote).FirstOrDefault(),
                            AppointmentTracking = appointmentTrakList.Where(a => a.InmateId == s.InmateId)
                                .Select(se => new AppointmentTracking
                                {
                                    AppointmentEndDate = se.AppointmentEndDate,
                                    ScheduleId = se.ScheduleId,
                                    ApptLocationId = se.ApptLocationId,
                                    ApptStartDate = se.ApptStartDate,
                                    ReoccurFlag = se.ReoccurFlag
                                }).FirstOrDefault(),
                            //VisitFlag = s.ScheduleInmate.Any(se => aoScheduleIds.Contains(se.ScheduleId)),
                            VisitFlag = _context.Visit.Any(w => aoScheduleIds.Contains(w.ScheduleId) 
                                && w.InmateId == s.InmateId),
                            HousingDetail =
                                new HousingCapacityVm
                                {
                                    HousingLocation = s.HousingUnit.HousingUnitLocation,
                                    HousingNumber = s.HousingUnit.HousingUnitNumber,
                                    HousingBedLocation = s.HousingUnit.HousingUnitBedLocation,
                                    HousingBedNumber = s.HousingUnit.HousingUnitBedNumber
                                },
                            Person = new PersonInfoVm
                            {
                                PersonId = s.Person.PersonId,
                                PersonFirstName = s.Person.PersonFirstName,
                                PersonLastName = s.Person.PersonLastName,
                                PersonMiddleName = s.Person.PersonMiddleName,
                                PersonSuffix = s.Person.PersonSuffix
                            },
                            FacilityAbbr = s.Facility.FacilityAbbr,
                            FacilityId = s.Facility.FacilityId,
                            InmateNumber = s.InmateNumber,
                            TransferFlag = s.InmateCurrentTrackNavigation.TransferFlag,
                            ApptAlertEndDate = s.InmateCurrentTrackNavigation.AppointmentAlertApptEndDate == 1,
                            ImageName = _photo.GetPhotoByCollectionIdentifier(s.Person.Identifiers),
                            EnrouteLocationId = lstinmateTraks.Where(w => w.InmateId == s.InmateId)
                                .Select(se => se.EnrouteFinalLocationId).FirstOrDefault(),
                            ScheduleId = lstInmateApptTraks.Where(w => w.InmateId == s.InmateId)
                                .Select(se => se.ScheduleId).FirstOrDefault()
                        }).ToList()
            };

            // TO Do
            trackHousingDetailVm.LstTrackCheckedOutCount = trackHousingDetailVm.LstTrackCheckedOut.GroupBy(g =>
                new
                {
                    g.LocationId,
                    g.Location
                }).OrderBy(o => o.Key.Location).Select(s => new TrackHousingCountVm
                {
                    LocationId = s.Key.LocationId,
                    TrakLocation = s.Key.Location,
                    Count = s.Count(),
                    DisplayFlag = s.Count(w =>
                                      w.LocationId == s.Key.LocationId && w.ApptAlertEndDate &&
                                      w.AppointmentTracking != null && !w.VisitFlag
                                      && w.AppointmentTracking.AppointmentEndDate.HasValue
                                      && (w.AppointmentTracking.AppointmentEndDate.Value.TimeOfDay.TotalMinutes > 0
                                          ? w.AppointmentTracking.AppointmentEndDate.Value < DateTime.Now
                                          : w.AppointmentTracking.AppointmentEndDate.Value.Date <
                                            DateTime.Now.Date)) > 0
                }).ToList();
            trackHousingDetailVm.LstTrackCheckedOutCount.ForEach(item => {
                Privileges privileges = _context.Privileges.Find(item.LocationId);
                item.EnrouteFlag = privileges.TrackEnrouteFlag == 1;
                item.CourtFlag = privileges.AppointmentRequireCourtLink;
            });

            trackHousingDetailVm.LstTrackCheckedOut.ForEach(track =>
            {
                track.IsIncompleteTasks =
                    _inmateService.GetInmateTasks(track.InmateId, TaskValidateType.HousingAssignFromTransfer)
                        .Count > 0;
            });

            trackHousingDetailVm.LstTrackTransfer = lstInmate.Where(w =>
                    w.InmateCurrentTrackNavigation.InactiveFlag == 0 
                    && w.InmateCurrentTrackNavigation.FacilityId == facilityId
                    && w.InmateCurrentTrackNavigation.TransferFlag == 1)
                .OrderBy(o => o.HousingUnit.HousingUnitLocation).ThenBy(t => t.HousingUnit.HousingUnitNumber)
                .Select(
                    s => new TrackHousingLocationVm
                    {
                        InmateId = s.InmateId,
                        LocationId = s.InmateCurrentTrackId ?? 0,
                        Location = s.InmateCurrentTrack,
                        DateOut = lstinmateTraks.Where(w => w.InmateId == s.InmateId)
                            .Select(se => se.InmateTrakDateOut)
                            .FirstOrDefault(),
                        EnrouteStartOut = lstinmateTraks.Where(w => w.InmateId == s.InmateId)
                            .Select(se => se.EnrouteStartOut)
                            .FirstOrDefault(),
                        TrakNotes = lstinmateTraks.Where(w => w.InmateId == s.InmateId)
                            .Select(se => se.InmateTrakNote).FirstOrDefault(),
                        AppointmentTracking = appointmentTrakList.Where(a => a.InmateId == s.InmateId)
                            .Select(se => new AppointmentTracking
                            {
                                AppointmentEndDate = se.AppointmentEndDate,
                                ScheduleId = se.ScheduleId,
                                ApptLocationId = se.ApptLocationId,
                                ApptStartDate = se.ApptStartDate,
                                ReoccurFlag = se.ReoccurFlag
                            }).FirstOrDefault(),
                        //  VisitFlag = s.ScheduleInmate.Any(se => aoScheduleIds.Contains(se.ScheduleId)),
                        VisitFlag = _context.Visit.Any(w => aoScheduleIds.Contains(w.ScheduleId) 
                            && w.InmateId == s.InmateId),
                        HousingDetail = new HousingCapacityVm
                        {
                            HousingLocation = s.HousingUnit.HousingUnitLocation,
                            HousingNumber = s.HousingUnit.HousingUnitNumber,
                            HousingBedLocation = s.HousingUnit.HousingUnitBedLocation,
                            HousingBedNumber = s.HousingUnit.HousingUnitBedNumber
                        },
                        Person = new PersonInfoVm
                        {
                            PersonId = s.Person.PersonId,
                            PersonFirstName = s.Person.PersonFirstName,
                            PersonLastName = s.Person.PersonLastName,
                            PersonMiddleName = s.Person.PersonMiddleName,
                            PersonSuffix = s.Person.PersonSuffix
                        },
                        FacilityAbbr = s.Facility.FacilityAbbr,
                        FacilityId=s.Facility.FacilityId,
                        InmateNumber = s.InmateNumber,
                        TransferFlag = s.InmateCurrentTrackNavigation.TransferFlag,
                        ImageName = _photo.GetPhotoByCollectionIdentifier(s.Person.Identifiers),
                        TransferFacilityId = s.InmateCurrentTrackNavigation.FacilityId,
                        ApptAlertEndDate = s.InmateCurrentTrackNavigation.AppointmentAlertApptEndDate == 1,
                        EnrouteLocationId = lstinmateTraks.Where(w => w.InmateId == s.InmateId)
                            .Select(se => se.EnrouteFinalLocationId).FirstOrDefault(),
                        ScheduleId = lstInmateApptTraks.Where(w => w.InmateId == s.InmateId)
                            .Select(se => se.ScheduleId).FirstOrDefault()
                    }).ToList();

            trackHousingDetailVm.LstTrackTransfer.ForEach(track =>
            {
                track.IsIncompleteTasks =
                    _inmateService.GetInmateTasks(track.InmateId, TaskValidateType.HousingAssignFromTransfer)
                        .Count > 0;
            });

            trackHousingDetailVm.LstTrackTransferCount = trackHousingDetailVm.LstTrackTransfer
                .GroupBy(g =>
                new
                {
                    g.LocationId,
                    g.Location
                }).OrderBy(o => o.Key.Location).Select(s => new TrackHousingCountVm
                {
                    LocationId = s.Key.LocationId,
                    TrakLocation = s.Key.Location,
                    Count = s.Count(),
                    DisplayFlag = s.Count(w =>
                                      w.LocationId == s.Key.LocationId && w.ApptAlertEndDate &&
                                      w.AppointmentTracking != null && !w.VisitFlag
                                      && w.AppointmentTracking.AppointmentEndDate.HasValue
                                      && (w.AppointmentTracking.AppointmentEndDate.Value.TimeOfDay.TotalMinutes > 0
                                          ? w.AppointmentTracking.AppointmentEndDate.Value < DateTime.Now
                                          : w.AppointmentTracking.AppointmentEndDate.Value.Date < DateTime.Now.Date
                                      )) > 0
                }).ToList();
            trackHousingDetailVm.LstTrackTransferCount.ForEach(item => {
                Privileges privileges = _context.Privileges.Find(item.LocationId);
                item.EnrouteFlag = privileges.TrackEnrouteFlag == 1;
                item.CourtFlag = privileges.AppointmentRequireCourtLink;
            });

            trackHousingDetailVm.LstExternalCheckedOut = _context.Inmate.Where(w =>
                    w.InmateActive == 1 && w.InmateCurrentTrackId.HasValue
                    && !w.InmateCurrentTrackNavigation.FacilityId.HasValue
                    && !w.InmateCurrentTrackNavigation.TransferFlag.HasValue &&
                    w.InmateCurrentTrackNavigation.TransferFlag == null)
                .OrderBy(o => o.HousingUnit.HousingUnitLocation).ThenBy(t => t.HousingUnit.HousingUnitNumber)
                .Select(s => new TrackHousingLocationVm
                {
                    InmateId = s.InmateId,
                    LocationId = s.InmateCurrentTrackId ?? 0,
                    Location = s.InmateCurrentTrack,
                    TrakLocation = s.InmateTrak.Any() ? s.InmateCurrentTrack : "",
                    DateOut = lstinmateTraks.Where(w => w.InmateId == s.InmateId).Select(se => se.InmateTrakDateOut)
                        .FirstOrDefault(),
                    EnrouteStartOut = lstinmateTraks.Where(w => w.InmateId == s.InmateId)
                            .Select(se => se.EnrouteStartOut)
                            .FirstOrDefault(),
                    TrakNotes = lstinmateTraks.Where(w => w.InmateId == s.InmateId)
                                .Select(se => se.InmateTrakNote).FirstOrDefault(),
                    AppointmentTracking = appointmentTrakList.Where(a => a.InmateId == s.InmateId)
                        .Select(se => new AppointmentTracking
                        {
                            AppointmentEndDate = se.AppointmentEndDate,
                            ScheduleId = se.ScheduleId,
                            ApptLocationId = se.ApptLocationId,
                            ApptStartDate = se.ApptStartDate,
                            ReoccurFlag = se.ReoccurFlag
                        }).FirstOrDefault(),
                    // VisitFlag = s.ScheduleInmate.Any(se => aoScheduleIds.Contains(se.ScheduleId)),
                    VisitFlag = _context.Visit.Any(w => aoScheduleIds.Contains(w.ScheduleId) 
                        && w.InmateId == s.InmateId),
                    ApptAlertEndDate = s.InmateCurrentTrackNavigation.AppointmentAlertApptEndDate == 1,
                    HousingDetail =
                        new HousingCapacityVm
                        {
                            HousingBedNumber = s.HousingUnit.HousingUnitBedNumber,
                            HousingBedLocation = s.HousingUnit.HousingUnitBedLocation,
                            HousingLocation = s.HousingUnit.HousingUnitLocation,
                            HousingNumber = s.HousingUnit.HousingUnitNumber
                        },
                    InmateNumber = s.InmateNumber,
                    TransferFlag = s.InmateCurrentTrackNavigation.TransferFlag,
                    Person = new PersonInfoVm
                    {
                        PersonId = s.Person.PersonId,
                        PersonLastName = s.Person.PersonLastName,
                        PersonFirstName = s.Person.PersonFirstName,
                        PersonMiddleName = s.Person.PersonMiddleName,
                        PersonSuffix = s.Person.PersonSuffix
                    },
                    FacilityAbbr = s.Facility.FacilityAbbr,
                    FacilityId=s.Facility.FacilityId,
                    ImageName = _photo.GetPhotoByCollectionIdentifier(s.Person.Identifiers),
                    EnrouteLocationId = lstinmateTraks.Where(w => w.InmateId == s.InmateId)
                        .Select(se => se.EnrouteFinalLocationId).FirstOrDefault(),
                    ScheduleId = lstInmateApptTraks.Where(w => w.InmateId == s.InmateId)
                        .Select(se => se.ScheduleId).FirstOrDefault()
                }).ToList();

            trackHousingDetailVm.LstExternalCheckedOut.ForEach(track =>
            {
                track.IsIncompleteTasks =
                    _inmateService.GetInmateTasks(track.InmateId, TaskValidateType.HousingAssignFromTransfer)
                        .Count > 0;
            });

            trackHousingDetailVm.LstExternalCheckedOutCount = trackHousingDetailVm.LstExternalCheckedOut
                .GroupBy(g =>
                    new
                    {
                        g.LocationId,
                        g.Location
                    }).OrderBy(o => o.Key.Location).Select(s => new TrackHousingCountVm
                    {
                        LocationId = s.Key.LocationId,
                        TrakLocation = s.Key.Location,
                        Count = s.Count(),
                        DisplayFlag = s.Count(w => w.TrakLocation == s.Key.Location && w.ApptAlertEndDate &&
                                                   w.AppointmentTracking != null && !w.VisitFlag
                                                   && w.AppointmentTracking.AppointmentEndDate.HasValue
                                                   && (w.AppointmentTracking.AppointmentEndDate.Value.TimeOfDay
                                                           .TotalMinutes > 0
                                                       ? w.AppointmentTracking.AppointmentEndDate.Value < DateTime.Now
                                                       : w.AppointmentTracking.AppointmentEndDate.Value.Date <
                                                         DateTime.Now.Date
                                                   )) > 0
                    }).ToList();
            trackHousingDetailVm.LstExternalCheckedOutCount.ForEach(item => {
                Privileges privileges = _context.Privileges.Find(item.LocationId);
                item.EnrouteFlag = privileges.TrackEnrouteFlag == 1;
                item.CourtFlag = privileges.AppointmentRequireCourtLink;
            });

            trackHousingDetailVm.LstInternalPod = lstInmate.Where(w =>
                    w.InmateCurrentTrackNavigation.InactiveFlag == 0 &&
                    w.InmateCurrentTrackNavigation.FacilityId == facilityId
                    && !w.InmateCurrentTrackNavigation.TransferFlag.HasValue
                    && w.InmateCurrentTrackNavigation.HousingUnitListId.HasValue)
                .OrderBy(o => o.HousingUnit.HousingUnitLocation).ThenBy(t => t.HousingUnit.HousingUnitNumber)
                .Select(
                    s => new TrackHousingLocationVm
                    {
                        InmateId = s.InmateId,
                        LocationId = s.InmateCurrentTrackId ?? 0,
                        Location = s.InmateCurrentTrack,
                        DateOut = lstinmateTraks.Where(w => w.InmateId == s.InmateId)
                            .Select(se => se.InmateTrakDateOut)
                            .FirstOrDefault(),
                        EnrouteStartOut = lstinmateTraks.Where(w => w.InmateId == s.InmateId)
                            .Select(se => se.EnrouteStartOut)
                            .FirstOrDefault(),
                        TrakNotes = lstinmateTraks.Where(w => w.InmateId == s.InmateId)
                                .Select(se => se.InmateTrakNote).FirstOrDefault(),
                        AppointmentTracking = appointmentTrakList.Where(a => a.InmateId == s.InmateId)
                            .Select(se => new AppointmentTracking
                            {
                                AppointmentEndDate = se.AppointmentEndDate,
                                ScheduleId = se.ScheduleId,
                                ApptLocationId = se.ApptLocationId,
                                ApptStartDate = se.ApptStartDate,
                                ReoccurFlag = se.ReoccurFlag
                            }).FirstOrDefault(),
                        //  VisitFlag = s.ScheduleInmate.Any(se => aoScheduleIds.Contains(se.ScheduleId)),
                        VisitFlag = _context.Visit.Any(w => aoScheduleIds.Contains(w.ScheduleId) 
                            && w.InmateId == s.InmateId),
                        HousingDetail =
                            new HousingCapacityVm
                            {
                                HousingLocation = s.HousingUnit.HousingUnitLocation,
                                HousingNumber = s.HousingUnit.HousingUnitNumber,
                                HousingBedLocation = s.HousingUnit.HousingUnitBedLocation,
                                HousingBedNumber = s.HousingUnit.HousingUnitBedNumber
                            },
                        Person = new PersonInfoVm
                        {
                            PersonId = s.Person.PersonId,
                            PersonFirstName = s.Person.PersonFirstName,
                            PersonLastName = s.Person.PersonLastName,
                            PersonMiddleName = s.Person.PersonMiddleName,
                            PersonSuffix = s.Person.PersonSuffix
                        },
                        FacilityAbbr = s.Facility.FacilityAbbr,
                        FacilityId=s.Facility.FacilityId,
                        InmateNumber = s.InmateNumber,
                        TransferFlag = s.InmateCurrentTrackNavigation.TransferFlag,
                        ApptAlertEndDate = s.InmateCurrentTrackNavigation.AppointmentAlertApptEndDate == 1,
                        ImageName = _photo.GetPhotoByCollectionIdentifier(s.Person.Identifiers),
                        EnrouteLocationId = lstinmateTraks.Where(w => w.InmateId == s.InmateId)
                            .Select(se => se.EnrouteFinalLocationId).FirstOrDefault(),
                        ScheduleId = lstInmateApptTraks.Where(w => w.InmateId == s.InmateId)
                            .Select(se => se.ScheduleId).FirstOrDefault()
                    }).ToList();

            trackHousingDetailVm.LstInternalPodCount = trackHousingDetailVm.LstInternalPod.GroupBy(g =>
                new
                {
                    g.LocationId,
                    g.Location
                }).OrderBy(o => o.Key.Location).Select(s => new TrackHousingCountVm
                {
                    LocationId = s.Key.LocationId,
                    TrakLocation = s.Key.Location,
                    Count = s.Count(),
                    DisplayFlag = s.Count(w =>
                                      w.LocationId == s.Key.LocationId && w.AppointmentTracking != null &&
                                      w.ApptAlertEndDate && !w.VisitFlag &&
                                      w.AppointmentTracking.AppointmentEndDate.HasValue
                                      && (w.AppointmentTracking.AppointmentEndDate.Value.TimeOfDay.TotalMinutes >
                                          0
                                          ? w.AppointmentTracking.AppointmentEndDate.Value < DateTime.Now
                                          : w.AppointmentTracking.AppointmentEndDate.Value.Date < DateTime.Now.Date
                                      )) > 0
                }).ToList();
            trackHousingDetailVm.LstInternalPodCount.ForEach(item => {
                Privileges privileges = _context.Privileges.Find(item.LocationId);
                item.EnrouteFlag = privileges.TrackEnrouteFlag == 1;
                item.CourtFlag = privileges.AppointmentRequireCourtLink;
            });

            trackHousingDetailVm.LstProgramOnly = lstInmate.Where(w =>
                    w.InmateCurrentTrackNavigation.InactiveFlag == 0
                    && w.InmateCurrentTrackNavigation.FacilityId == facilityId
                    && !w.InmateCurrentTrackNavigation.TransferFlag.HasValue
                    && w.InmateCurrentTrackNavigation.ShowInProgram == 1)
                .OrderBy(o => o.HousingUnit.HousingUnitLocation).ThenBy(t => t.HousingUnit.HousingUnitNumber)
                .Select(
                    s => new TrackHousingLocationVm
                    {
                        InmateId = s.InmateId,
                        LocationId = s.InmateCurrentTrackId ?? 0,
                        Location = s.InmateCurrentTrack,
                        DateOut = lstinmateTraks.Where(w => w.InmateId == s.InmateId)
                            .Select(se => se.InmateTrakDateOut)
                            .FirstOrDefault(),
                        EnrouteStartOut = lstinmateTraks.Where(w => w.InmateId == s.InmateId)
                            .Select(se => se.EnrouteStartOut)
                            .FirstOrDefault(),
                        TrakNotes = lstinmateTraks.Where(w => w.InmateId == s.InmateId)
                                .Select(se => se.InmateTrakNote).FirstOrDefault(),
                        AppointmentTracking = appointmentTrakList.Where(a => a.InmateId == s.InmateId)
                            .Select(se => new AppointmentTracking
                            {
                                AppointmentEndDate = se.AppointmentEndDate,
                                ScheduleId = se.ScheduleId,
                                ApptLocationId = se.ApptLocationId,
                                ApptStartDate = se.ApptStartDate,
                                ReoccurFlag = se.ReoccurFlag
                            }).FirstOrDefault(),
                        //   VisitFlag = s.ScheduleInmate.Any(se => aoScheduleIds.Contains(se.ScheduleId)),
                        VisitFlag = _context.Visit.Any(w => aoScheduleIds.Contains(w.ScheduleId) 
                            && w.InmateId == s.InmateId),
                        HousingDetail =
                            new HousingCapacityVm
                            {
                                HousingLocation = s.HousingUnit.HousingUnitLocation,
                                HousingNumber = s.HousingUnit.HousingUnitNumber,
                                HousingBedLocation = s.HousingUnit.HousingUnitBedLocation,
                                HousingBedNumber = s.HousingUnit.HousingUnitBedNumber
                            },
                        Person = new PersonInfoVm
                        {
                            PersonId = s.Person.PersonId,
                            PersonFirstName = s.Person.PersonFirstName,
                            PersonLastName = s.Person.PersonLastName,
                            PersonMiddleName = s.Person.PersonMiddleName,
                            PersonSuffix = s.Person.PersonSuffix
                        },
                        FacilityAbbr = s.Facility.FacilityAbbr,
                        FacilityId=s.Facility.FacilityId,
                        InmateNumber = s.InmateNumber,
                        TransferFlag = s.InmateCurrentTrackNavigation.TransferFlag,
                        ApptAlertEndDate = s.InmateCurrentTrackNavigation.AppointmentAlertApptEndDate == 1,
                        ImageName = _photo.GetPhotoByCollectionIdentifier(s.Person.Identifiers),
                        EnrouteLocationId = lstinmateTraks.Where(w => w.InmateId == s.InmateId)
                            .Select(se => se.EnrouteFinalLocationId).FirstOrDefault(),
                        ScheduleId = lstInmateApptTraks.Where(w => w.InmateId == s.InmateId)
                            .Select(se => se.ScheduleId).FirstOrDefault()
                    }).ToList();

            trackHousingDetailVm.LstProgramOnlyCount = trackHousingDetailVm.LstProgramOnly.GroupBy(g =>
                new
                {
                    g.LocationId,
                    g.Location
                }).OrderBy(o => o.Key.Location).Select(s => new TrackHousingCountVm
                {
                    LocationId = s.Key.LocationId,
                    TrakLocation = s.Key.Location,
                    Count = s.Count(),
                    DisplayFlag = s.Count(w =>
                                      w.LocationId == s.Key.LocationId && w.AppointmentTracking != null &&
                                      w.ApptAlertEndDate && !w.VisitFlag &&
                                      w.AppointmentTracking.AppointmentEndDate.HasValue
                                      && (w.AppointmentTracking.AppointmentEndDate.Value.TimeOfDay.TotalMinutes >
                                          0
                                          ? w.AppointmentTracking.AppointmentEndDate.Value < DateTime.Now
                                          : w.AppointmentTracking.AppointmentEndDate.Value.Date < DateTime.Now.Date
                                      )) > 0
                }).ToList();
            trackHousingDetailVm.LstProgramOnlyCount.ForEach(item => {
                Privileges privileges = _context.Privileges.Find(item.LocationId);
                item.EnrouteFlag = privileges.TrackEnrouteFlag == 1;
                item.CourtFlag = privileges.AppointmentRequireCourtLink;
            });

            trackHousingDetailVm.LstHearingOnly = lstInmate.Where(w =>
                    w.InmateCurrentTrackNavigation.InactiveFlag == 0
                    && w.InmateCurrentTrackNavigation.FacilityId == facilityId
                    && !w.InmateCurrentTrackNavigation.TransferFlag.HasValue
                    && w.InmateCurrentTrackNavigation.ShowInHearing == 1)
                .OrderBy(o => o.HousingUnit.HousingUnitLocation).ThenBy(t => t.HousingUnit.HousingUnitNumber)
                .Select(
                    s => new TrackHousingLocationVm
                    {
                        InmateId = s.InmateId,
                        LocationId = s.InmateCurrentTrackId ?? 0,
                        Location = s.InmateCurrentTrack,
                        DateOut = lstinmateTraks.Where(w => w.InmateId == s.InmateId)
                            .Select(se => se.InmateTrakDateOut)
                            .FirstOrDefault(),
                        EnrouteStartOut = lstinmateTraks.Where(w => w.InmateId == s.InmateId)
                            .Select(se => se.EnrouteStartOut)
                            .FirstOrDefault(),
                        TrakNotes = lstinmateTraks.Where(w => w.InmateId == s.InmateId)
                                .Select(se => se.InmateTrakNote).FirstOrDefault(),
                        AppointmentTracking = appointmentTrakList.Where(a => a.InmateId == s.InmateId)
                            .Select(se => new AppointmentTracking
                            {
                                AppointmentEndDate = se.AppointmentEndDate,
                                ScheduleId = se.ScheduleId,
                                ApptLocationId = se.ApptLocationId,
                                ApptStartDate = se.ApptStartDate,
                                ReoccurFlag = se.ReoccurFlag
                            }).FirstOrDefault(),
                        // VisitFlag = s.ScheduleInmate.Any(se => aoScheduleIds.Contains(se.ScheduleId)),
                        VisitFlag = _context.Visit.Any(w => aoScheduleIds.Contains(w.ScheduleId) 
                            && w.InmateId == s.InmateId),
                        HousingDetail =
                            new HousingCapacityVm
                            {
                                HousingLocation = s.HousingUnit.HousingUnitLocation,
                                HousingNumber = s.HousingUnit.HousingUnitNumber,
                                HousingBedLocation = s.HousingUnit.HousingUnitBedLocation,
                                HousingBedNumber = s.HousingUnit.HousingUnitBedNumber
                            },
                        Person = new PersonInfoVm
                        {
                            PersonId = s.Person.PersonId,
                            PersonFirstName = s.Person.PersonFirstName,
                            PersonLastName = s.Person.PersonLastName,
                            PersonMiddleName = s.Person.PersonMiddleName,
                            PersonSuffix = s.Person.PersonSuffix
                        },
                        FacilityAbbr = s.Facility.FacilityAbbr,
                        FacilityId=s.Facility.FacilityId,
                        InmateNumber = s.InmateNumber,
                        TransferFlag = s.InmateCurrentTrackNavigation.TransferFlag,
                        ApptAlertEndDate = s.InmateCurrentTrackNavigation.AppointmentAlertApptEndDate == 1,
                        ImageName = _photo.GetPhotoByCollectionIdentifier(s.Person.Identifiers),
                        EnrouteLocationId = lstinmateTraks.Where(w => w.InmateId == s.InmateId)
                            .Select(se => se.EnrouteFinalLocationId).FirstOrDefault(),
                        ScheduleId = lstInmateApptTraks.Where(w => w.InmateId == s.InmateId)
                            .Select(se => se.ScheduleId).FirstOrDefault()
                    }).ToList();

            trackHousingDetailVm.LstHearingOnlyCount = trackHousingDetailVm.LstHearingOnly.GroupBy(g =>
                new
                {
                    g.LocationId,
                    g.Location
                }).OrderBy(o => o.Key.Location).Select(s => new TrackHousingCountVm
                {
                    LocationId = s.Key.LocationId,
                    TrakLocation = s.Key.Location,
                    Count = s.Count(),
                    DisplayFlag = s.Count(w =>
                                      w.LocationId == s.Key.LocationId && w.AppointmentTracking != null &&
                                      w.ApptAlertEndDate && !w.VisitFlag &&
                                      w.AppointmentTracking.AppointmentEndDate.HasValue
                                      && (w.AppointmentTracking.AppointmentEndDate.Value.TimeOfDay.TotalMinutes >
                                          0
                                          ? w.AppointmentTracking.AppointmentEndDate.Value < DateTime.Now
                                          : w.AppointmentTracking.AppointmentEndDate.Value.Date < DateTime.Now.Date
                                      )) > 0
                }).ToList();
            trackHousingDetailVm.LstHearingOnlyCount.ForEach(item => {
                Privileges privileges = _context.Privileges.Find(item.LocationId);
                item.EnrouteFlag = privileges.TrackEnrouteFlag == 1;
                item.CourtFlag = privileges.AppointmentRequireCourtLink;
            });

            IQueryable<PrivilegeDetailsVm> lstAllprivilege = _context.Privileges
                .Where(w => w.RemoveFromTrackingFlag == 0 && w.InactiveFlag == 0)
                .Select(s => new PrivilegeDetailsVm
                {
                    PrivilegeId = s.PrivilegeId,
                    PrivilegeDescription = s.PrivilegeDescription,
                    FacilityId = s.FacilityId,
                    TrackingAllowRefusal = s.TrackingAllowRefusal,
                    TrackEnrouteFlag = s.TrackEnrouteFlag,
                    ApptAlertEndDate = s.AppointmentAlertApptEndDate == 1
                });

            List<InmateTrak> lstInmateTrakElapsed =
                _context.InmateTrak.Where(w => w.EnrouteFinalLocationId.HasValue).ToList();

            trackHousingDetailVm.LstExternalCheckedOut.ForEach(item =>
            {
                if (item.EnrouteLocationId > 0)
                {
                    item.Destination = lstAllprivilege.Single(w => w.PrivilegeId == item.EnrouteLocationId)
                        .PrivilegeDescription;

                    item.DateOut = lstInmateTrakElapsed.Where(w => w.EnrouteFinalLocationId == item.EnrouteLocationId
                                                                   && w.InmateId == item.InmateId)
                        .OrderBy(o => o.InmateTrakId).FirstOrDefault()?.InmateTrakDateOut;
                        
                    item.EnrouteStartOut = lstInmateTrakElapsed.Where(w => w.EnrouteFinalLocationId == item.EnrouteLocationId
                                                                   && w.InmateId == item.InmateId)
                        .OrderBy(o => o.InmateTrakId).LastOrDefault()?.EnrouteStartOut;

                }
            });

            trackHousingDetailVm.LstInternalPod.ForEach(item =>
            {
                if (item.EnrouteLocationId > 0)
                {
                    item.Destination = lstAllprivilege.Single(w => w.PrivilegeId == item.EnrouteLocationId)
                        .PrivilegeDescription;

                    item.DateOut = lstInmateTrakElapsed.Where(w => w.EnrouteFinalLocationId == item.EnrouteLocationId
                                                                   && w.InmateId == item.InmateId)
                        .OrderBy(o => o.InmateTrakId).FirstOrDefault()?.InmateTrakDateOut;
                            
                    item.EnrouteStartOut = lstInmateTrakElapsed.Where(w => w.EnrouteFinalLocationId == item.EnrouteLocationId
                                                                   && w.InmateId == item.InmateId)
                        .OrderBy(o => o.InmateTrakId).LastOrDefault()?.EnrouteStartOut;

                }
            });

            trackHousingDetailVm.LstTrackCheckedOut.ForEach(item =>
            {
                if (item.EnrouteLocationId > 0)
                {
                    item.Destination = lstAllprivilege.Single(w => w.PrivilegeId == item.EnrouteLocationId)
                        .PrivilegeDescription;

                    item.DateOut = lstInmateTrakElapsed.Where(w => w.EnrouteFinalLocationId == item.EnrouteLocationId
                                                                   && w.InmateId == item.InmateId)
                        .OrderBy(o => o.InmateTrakId).FirstOrDefault()?.InmateTrakDateOut;
                            
                    item.EnrouteStartOut = lstInmateTrakElapsed.Where(w => w.EnrouteFinalLocationId == item.EnrouteLocationId
                                                                   && w.InmateId == item.InmateId)
                        .OrderBy(o => o.InmateTrakId).LastOrDefault()?.EnrouteStartOut;
                }
            });

            trackHousingDetailVm.LstTrackTransfer.ForEach(item =>
            {
                if (item.EnrouteLocationId > 0)
                {
                    item.Destination = lstAllprivilege.Single(w => w.PrivilegeId == item.EnrouteLocationId)
                        .PrivilegeDescription;

                    item.DateOut = lstInmateTrakElapsed.Where(w => w.EnrouteFinalLocationId == item.EnrouteLocationId
                                                                   && w.InmateId == item.InmateId)
                        .OrderBy(o => o.InmateTrakId).FirstOrDefault()?.InmateTrakDateOut;
                            
                    item.EnrouteStartOut = lstInmateTrakElapsed.Where(w => w.EnrouteFinalLocationId == item.EnrouteLocationId
                                                                   && w.InmateId == item.InmateId)
                        .OrderBy(o => o.InmateTrakId).LastOrDefault()?.EnrouteStartOut;
                }
            });

            trackHousingDetailVm.LstProgramOnly.ForEach(item =>
            {
                if (item.EnrouteLocationId > 0)
                {
                    item.Destination = lstAllprivilege.Single(w => w.PrivilegeId == item.EnrouteLocationId)
                        .PrivilegeDescription;

                    item.DateOut = lstInmateTrakElapsed.Where(w => w.EnrouteFinalLocationId == item.EnrouteLocationId
                                                                   && w.InmateId == item.InmateId)
                        .OrderBy(o => o.InmateTrakId).FirstOrDefault()?.InmateTrakDateOut;
                            
                    item.EnrouteStartOut = lstInmateTrakElapsed.Where(w => w.EnrouteFinalLocationId == item.EnrouteLocationId
                                                                   && w.InmateId == item.InmateId)
                        .OrderBy(o => o.InmateTrakId).LastOrDefault()?.EnrouteStartOut;
                }
            });

            trackHousingDetailVm.LstHearingOnly.ForEach(item =>
            {
                if (item.EnrouteLocationId > 0)
                {
                    item.Destination = lstAllprivilege.Single(w => w.PrivilegeId == item.EnrouteLocationId)
                        .PrivilegeDescription;

                    item.DateOut = lstInmateTrakElapsed.Where(w => w.EnrouteFinalLocationId == item.EnrouteLocationId
                                                                   && w.InmateId == item.InmateId)
                        .OrderBy(o => o.InmateTrakId).FirstOrDefault()?.InmateTrakDateOut;
                            
                    item.EnrouteStartOut = lstInmateTrakElapsed.Where(w => w.EnrouteFinalLocationId == item.EnrouteLocationId
                                                                   && w.InmateId == item.InmateId)
                        .OrderBy(o => o.InmateTrakId).LastOrDefault()?.EnrouteStartOut;
                }
            });

            trackHousingDetailVm.LstPrivileges = lstAllprivilege.Where(w => w.FacilityId == facilityId).ToList();
            trackHousingDetailVm.LstPrivileges.AddRange(lstAllprivilege.Where(w => w.FacilityId == 0 ||
                                                                                   !w.FacilityId.HasValue).ToList());
            trackHousingDetailVm.LstPrivileges =
                trackHousingDetailVm.LstPrivileges.OrderBy(o => o.PrivilegeDescription).ToList();

            trackHousingDetailVm.RefusalReasonList =
                _commonService.GetLookupKeyValuePairs(LookupConstants.TRACKREFUSALREAS);

            return trackHousingDetailVm;
        }

        public TrackingFacilityVm GetTrackingHousing(int facilityId, int housingNumber = 0, int housingGroup = 0)
        {
            IQueryable<Inmate> lstInmate =
                _context.Inmate.Where(w => w.InmateActive == 1
                                           && w.FacilityId == facilityId
                                           && (!w.InmateCurrentTrackId.HasValue ||
                                               w.InmateCurrentTrackNavigation.InactiveFlag == 0));

            if (housingGroup > 0)
            {
                List<int> housingUnitListIds = _context.HousingGroupAssign
                    .Where(w => w.HousingGroupId == housingGroup && w.HousingUnitListId > 0)
                    .Select(s => s.HousingUnitListId ?? 0).ToList();

                lstInmate = lstInmate.Where(w => housingUnitListIds.Contains(w.HousingUnit.HousingUnitListId));
            }
            else if (housingNumber > 0)
            {
                lstInmate = lstInmate.Where(w => w.HousingUnit.HousingUnitListId == housingNumber);
            }

            List<AppointmentTracking> appointmentTrakList = _context.InmateAppointmentTrack
                .Where(w => !w.InmateTrakDateIn.HasValue).Select(s =>
                    new AppointmentTracking
                    {
                        AppointmentEndDate = s.Schedule.EndDate,
                        ScheduleId = s.Schedule.ScheduleId,
                        ApptLocationId = s.Schedule.LocationId,
                        ApptStartDate = s.Schedule.StartDate.ToString("HH:mm"),
                        ReoccurFlag = s.Schedule.IsSingleOccurrence,
                        InmateId = s.InmateId
                    }).ToList();

            List<int> aoScheduleIds = appointmentTrakList.Select(s => s.ScheduleId).ToList();

            List<InmateTrak> lstInmateTraks = _context.InmateTrak
                .Where(w => !w.InmateTrakDateIn.HasValue && w.FacilityId == facilityId)
                .OrderByDescending(w => w.InmateTrakId).ToList();
            List<InmateAppointmentTrack> lstInmateApptTraks = _context.InmateAppointmentTrack
                .Where(w => !w.InmateTrakDateIn.HasValue && w.FacilityId == facilityId).ToList();

            TrackingFacilityVm trackHousingDetail = new TrackingFacilityVm
            {
                LstTrackFacility = lstInmate.Select(
                    s => new TrackHousingLocationVm
                    {
                        InmateId = s.InmateId,
                        LocationId = s.InmateCurrentTrackId ?? 0,
                        Location = s.InmateCurrentTrack,
                        TrakNotes = lstInmateTraks.Where(w => w.InmateId == s.InmateId)
                            .Select(se => se.InmateTrakNote)
                            .FirstOrDefault(),
                        DateOut = lstInmateTraks.Where(w => w.InmateId == s.InmateId)
                            .Select(se => se.InmateTrakDateOut)
                            .FirstOrDefault(),
                        EnrouteStartOut = lstInmateTraks.Where(w => w.InmateId == s.InmateId)
                            .Select(se => se.EnrouteStartOut)
                            .FirstOrDefault(),
                        AppointmentTracking = appointmentTrakList.Where(a => a.InmateId == s.InmateId)
                            .Select(se => new AppointmentTracking
                            {
                                AppointmentEndDate = se.AppointmentEndDate,
                                ScheduleId = se.ScheduleId,
                                ApptLocationId = se.ApptLocationId,
                                ApptStartDate = se.ApptStartDate,
                                ReoccurFlag = se.ReoccurFlag
                            }).FirstOrDefault(),
                        // VisitFlag = s.ScheduleInmate.Any(se => aoScheduleIds.Contains(se.ScheduleId)),
                        VisitFlag = _context.Visit.Any(w => aoScheduleIds.Contains(w.ScheduleId) 
                            && w.InmateId == s.InmateId),
                        HousingDetail = new HousingCapacityVm
                        {
                            HousingLocation = s.HousingUnit.HousingUnitLocation,
                            HousingNumber = s.HousingUnit.HousingUnitNumber,
                            HousingBedLocation = s.HousingUnit.HousingUnitBedLocation,
                            HousingBedNumber = s.HousingUnit.HousingUnitBedNumber,
                            HousingUnitListId = s.HousingUnitId.HasValue ? s.HousingUnit.HousingUnitListId : 0
                        },
                        FacilityAbbr = s.Facility.FacilityAbbr,
                        FacilityId=s.Facility.FacilityId,
                        Person = new PersonInfoVm
                        {
                            PersonId = s.Person.PersonId,
                            PersonFirstName = s.Person.PersonFirstName,
                            PersonLastName = s.Person.PersonLastName,
                            PersonMiddleName = s.Person.PersonMiddleName,
                            PersonSuffix = s.Person.PersonSuffix
                        },
                        InmateNumber = s.InmateNumber,
                        TransferFlag = s.InmateCurrentTrackNavigation.TransferFlag ?? 0,
                        ApptAlertEndDate = s.InmateCurrentTrackNavigation.AppointmentAlertApptEndDate == 1,
                        ImageName = _photo.GetPhotoByCollectionIdentifier(s.Person.Identifiers),
                        EnrouteLocationId = lstInmateTraks.Where(w => w.InmateId == s.InmateId)
                            .Select(se => se.EnrouteFinalLocationId).FirstOrDefault(),
                        ScheduleId = lstInmateApptTraks.Where(w => w.InmateId == s.InmateId)
                            .Select(se => se.ScheduleId).FirstOrDefault()
                    }).ToList()
            };

            List<HousingCapacityVm> housingCapacities = _context.HousingUnit
                .Where(w => trackHousingDetail.LstTrackFacility.Select(s => s.HousingDetail.HousingUnitListId)
                    .Contains(w.HousingUnitListId))
                .GroupBy(g => g.HousingUnitListId)
                .Select(s => new HousingCapacityVm
                {
                    HousingUnitListId = s.Key,
                    CurrentCapacity = s.Sum(se => se.HousingUnitActualCapacity ?? 0),
                    OutofService = s.Sum(se => se.HousingUnitOutOfService)
                }).ToList();

            trackHousingDetail.LstTrackFacilityCount = trackHousingDetail.LstTrackFacility.GroupBy(g => new
            {
                g.HousingDetail.HousingLocation,
                g.HousingDetail.HousingNumber,
                g.HousingDetail.HousingUnitListId
            }).OrderBy(o => o.Key.HousingLocation).ThenBy(t => t.Key.HousingNumber).Select(s =>
                new TrackHousingCountVm
                {
                    Count = s.Count(w => w.LocationId > 0),
                    HousingUnitLocation = s.Key.HousingLocation,
                    HousingUnitNumber = s.Key.HousingNumber,
                    HousingUnitListId = s.Key.HousingUnitListId,
                    Assigned = s.Count(),
                    CurrentCapacity = housingCapacities.Where(w => w.HousingUnitListId == s.Key.HousingUnitListId)
                        .Select(se => se.CurrentCapacity).FirstOrDefault(),
                    OutofService = housingCapacities.Where(w => w.HousingUnitListId == s.Key.HousingUnitListId)
                        .Select(se => se.OutofService ?? 0).FirstOrDefault(),
                    DisplayFlag = s.Count(w => w.ApptAlertEndDate && w.AppointmentTracking != null && !w.VisitFlag &&
                                               w.AppointmentTracking.AppointmentEndDate.HasValue &&
                                               (w.AppointmentTracking.AppointmentEndDate.Value.TimeOfDay.TotalMinutes >
                                                0
                                                   ? w.AppointmentTracking.AppointmentEndDate.Value < DateTime.Now
                                                   : w.AppointmentTracking.AppointmentEndDate.Value.Date <
                                                     DateTime.Now.Date
                                               )) > 0
                }).ToList();

            IQueryable<PrivilegeDetailsVm> lstAllprivilege = _context.Privileges
                .Where(w => w.RemoveFromTrackingFlag == 0 && w.InactiveFlag == 0)
                .Select(s => new PrivilegeDetailsVm
                {
                    PrivilegeId = s.PrivilegeId,
                    PrivilegeDescription = s.PrivilegeDescription,
                    FacilityId = s.FacilityId,
                    TrackingAllowRefusal = s.TrackingAllowRefusal,
                    TrackEnrouteFlag = s.TrackEnrouteFlag,
                    ApptAlertEndDate = s.AppointmentAlertApptEndDate == 1
                });

            List<int> finalLocationIds = trackHousingDetail.LstTrackFacility.Where(w => w.EnrouteLocationId.HasValue)
                .Select(s => s.EnrouteLocationId ?? 0).ToList();

            List<InmateTrak> lstInmateTrakElapsed = _context.InmateTrak.Where(w =>
                finalLocationIds.Contains(w.EnrouteFinalLocationId ?? 0)).ToList();

            trackHousingDetail.LstTrackFacility.ForEach(item =>
            {
                if (item.EnrouteLocationId > 0)
                {
                    item.Destination = lstAllprivilege.Single(w => w.PrivilegeId == item.EnrouteLocationId)
                        .PrivilegeDescription;

                    item.DateOut = lstInmateTrakElapsed.Where(w => w.EnrouteFinalLocationId == item.EnrouteLocationId
                                                                   && w.InmateId == item.InmateId)
                        .OrderBy(o => o.InmateTrakId).FirstOrDefault()?.InmateTrakDateOut;
                            
                    item.EnrouteStartOut = lstInmateTrakElapsed.Where(w => w.EnrouteFinalLocationId == item.EnrouteLocationId
                                                                   && w.InmateId == item.InmateId)
                        .OrderBy(o => o.InmateTrakId).LastOrDefault()?.EnrouteStartOut;

                }
            });

            trackHousingDetail.LstPrivileges = lstAllprivilege.Where(w => w.FacilityId == facilityId).ToList();
            trackHousingDetail.LstPrivileges.AddRange(lstAllprivilege.Where(w => w.FacilityId == 0 ||
                                                                                 !w.FacilityId.HasValue).ToList());
            trackHousingDetail.LstPrivileges =
                trackHousingDetail.LstPrivileges.OrderBy(o => o.PrivilegeDescription).ToList();

            trackHousingDetail.RefusalReasonList =
                _commonService.GetLookupKeyValuePairs(LookupConstants.TRACKREFUSALREAS);

            trackHousingDetail.HousingLockdownList = GetHousingLockdownNotifications(facilityId).Distinct().ToList();

            HousingLockInputVm lockInputVm = new HousingLockInputVm()
            {
                HousingType = HousingType.HousingLocation,
                FacilityId = facilityId
            };

            trackHousingDetail.HousingBuildingLockdownList =
                _lockdownService.GetHousingLockdownNotifications(lockInputVm).ToList();

            List<HousingGroupAssignVm> lstHousingGroupAssignVm = _context.HousingGroup
                .Where(h => h.FacilityId == facilityId && h.DeleteFlag != 1)
                .Select(g => new HousingGroupAssignVm
                {
                    HousingGroupId = g.HousingGroupId,
                    HousingGroupName = g.GroupName
                })
                .OrderBy(hg => hg.HousingGroupName)
                .ToList();

            List<HousingGroupAssign> lstHousingGroupAssign = _context.HousingGroupAssign.ToList();
            lstHousingGroupAssignVm.ForEach(item =>
            {
                item.LstHousingUnitIds = lstHousingGroupAssign.Where(h => h.HousingGroupId == item.HousingGroupId
                        && h.DeleteFlag == 0 && h.HousingUnitListId.HasValue)
                    .Select(g => g.HousingUnitListId ?? 0)
                    .Distinct().ToArray();
            });

            trackHousingDetail.HousingUnitGroup = lstHousingGroupAssignVm;
            return trackHousingDetail;
        }

        public List<HousingDetail> GetHousingLockdownNotifications(int facilityId)
        {
            List<HousingDetail> lockHousingDetails = new List<HousingDetail>();

            List<LockdownVm> housingLockdown = _context.HousingLockdown.Where(w =>
                    w.StartLockdown <= DateTime.Now && w.EndLockdown >= DateTime.Now && !w.DeleteFlag)
                .Select(s => new LockdownVm
                {
                    Region = s.LockdownSource,
                    SourceId = s.SourceId
                }).ToList();

            if (housingLockdown.Count(w =>
                    w.Region == LockDownType.Facility.ToString() && w.SourceId == facilityId) > 0)
            {
                lockHousingDetails.Add(new HousingDetail()
                {
                    FacilityId = facilityId
                });
            }
            else
            {
                List<HousingUnitList> housingUnit = _context.HousingUnitList
                    .Where(w => w.FacilityId == facilityId).ToList();
                List<HousingGroupAssign> housingGroupAssign = _context.HousingGroupAssign
                    .Where(w => w.HousingGroup.FacilityId == facilityId && w.HousingUnitLocation != null && w.DeleteFlag == 0).ToList();
                housingLockdown.ForEach(lockDown =>
                {
                    if (lockDown.Region == LockDownType.Building.ToString() && housingUnit.Count(f =>
                            f.FacilityId == facilityId
                            && f.HousingUnitListId == lockDown.SourceId) > 0)
                    {
                        string housingUnitLocation = housingUnit
                            .First(f => f.HousingUnitListId == lockDown.SourceId).HousingUnitLocation;

                        lockHousingDetails.AddRange(housingUnit.Where(w => w.HousingUnitLocation == housingUnitLocation)
                            .Select(s => new HousingDetail
                            {
                                HousingUnitListId = s.HousingUnitListId
                            }));
                    }

                    if (lockDown.Region == LockDownType.Pod.ToString() && housingUnit.Count(f =>
                            f.FacilityId == facilityId
                            && f.HousingUnitListId == lockDown.SourceId) > 0)
                    {
                        lockHousingDetails.Add(new HousingDetail()
                        {
                            HousingUnitListId = lockDown.SourceId
                        });
                    }

                    if (lockDown.Region == LockDownType.HousingGroup.ToString() && housingGroupAssign.Count(w =>
                            w.HousingGroupId == lockDown.SourceId) > 0)
                    {
                        lockHousingDetails.AddRange(housingGroupAssign.Where(w => w.HousingGroupId == lockDown.SourceId)
                            .Select(s => new HousingDetail
                            {
                                HousingUnitListId = s.HousingUnitListId
                            }));
                    }
                });
            }

            return lockHousingDetails;
        }
    }
}
