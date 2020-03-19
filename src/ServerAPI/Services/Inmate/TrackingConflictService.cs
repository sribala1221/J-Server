﻿using System;
using System.Collections.Generic;
using System.Linq;
using ServerAPI.ViewModels;
using ServerAPI.Utilities;
using GenerateTables.Models;

// ReSharper disable once CheckNamespace
namespace ServerAPI.Services
{
    public class TrackingConflictService : ITrackingConflictService
    {
        private readonly AAtims _context;
        private readonly ICommonService _commonService;
        private readonly List<TrackingConflictVm> _trackingConflictDetails = new List<TrackingConflictVm>();

        public TrackingConflictService(AAtims context, ICommonService commonService)
        {
            _context = context;
            _commonService = commonService;
        }

        //Tracking Conflict 
        public List<TrackingConflictVm> GetTrackingConflict(TrackingVm obj)
        {
            //To Get Visit Conflict
            // LoadVisitorConflict(obj.MoveInmateList, obj.MoveLocationId);
            //To Get Privilege Conflict Details
            LoadPrivilegeConflict(obj.MoveInmateList, obj.MoveLocationId);
            //To Get Location OOS & Capacity
            LoadLocationOverCapacity(obj.MoveInmateList, obj.MoveLocationId, obj.MoveInmateList.Count);
            // To Get Gender Conflict 
            LoadGenderConflict(obj.MoveInmateList, obj.MoveLocationId);

            if (obj.IsVisit)
            {
                IQueryable<AppointmentVm> lstTodayAppointment = GetTodayAppointment(obj.MoveInmateList, obj.MoveLocationId, obj.StartDate, obj.EndDate, obj.ScheduleId);
                LoadAppointmentConflict(lstTodayAppointment, obj.MoveInmateList);
                List<AppointmentVm> lstVisitSchedule = GetVisitSchedule(obj.MoveInmateList, obj.MoveLocationId, obj.StartDate, obj.EndDate, obj.ScheduleId);
                LoadVisitationConflict(lstVisitSchedule, obj.MoveInmateList);
            }
            else
            {
                // To get Appointment Conflict
                IQueryable<AppointmentVm> lstTodayAppointment = GetTodaysAppointment(obj.MoveInmateList, obj.MoveLocationId);
                LoadAppointmentConflict(lstTodayAppointment, obj.MoveInmateList);
            }
            //To Get Program Appointment
            //List<AppointmentVm> lstTodayProgramAppt = GetProgramAppointment(obj.MoveLocationId);
            //LoadProgramAppointmentConflict(lstTodayProgramAppt, obj.MoveInmateList);
            //To Get Keepsep Conflict 
            LoadKeepSeparateConflict(obj.MoveInmateList, obj.MoveLocationId);
            //To get total seperation conflict
            LoadTotalSeperationConflict(obj.MoveInmateList, obj.MoveLocationId);
            //To get housing supply conflict
            LoadHousingSupplyConflict(obj.MoveInmateList);
            //To Get housing lockdown conflict
            LoadHousingLockdownConflict(obj);
            //To Get Person Name Details
            LoadPersonDetails();

            return _trackingConflictDetails;
        }

        private List<AppointmentVm> GetVisitSchedule(ICollection<int> inmateList, int moveLocationId,
            DateTime? startDate, DateTime? endDate, int scheduleId)
        {
            List<AppointmentVm> visitSchedule = new List<AppointmentVm>();
            int[] lst = inmateList.ToArray();
            List<VisitToVisitor> schedule = _context.VisitToVisitor.Where(v =>
                    v.Visit.LocationId != moveLocationId &&
                    v.Visit.StartDate.Date == startDate.Value.Date && lst.Contains(v.Visit.InmateId ?? 0) &&
                    v.Visit.EndDate.Value.Date == endDate.Value.Date && startDate < v.Visit.EndDate &&
                    endDate >= v.Visit.EndDate ||
                    startDate <= v.Visit.StartDate && endDate > v.Visit.StartDate ||
                    startDate == v.Visit.StartDate && endDate == v.Visit.EndDate ||
                    startDate > v.Visit.StartDate && endDate < v.Visit.EndDate)
                .Select(x => new VisitToVisitor
                {
                    PersonId = x.PersonId,
                    ScheduleId = x.ScheduleId,
                    Visit = x.Visit
                }).ToList();
            schedule = schedule.Where(s => s.ScheduleId != scheduleId && s.Visit.InmateId != null &&
                                           s.Visit.CompleteRegistration && !s.Visit.CompleteVisitFlag &&
            s.Visit.VisitDenyFlag != 1 && !s.Visit.DeleteFlag).ToList();
            int[] lstId = schedule.Select(s => s.Visit.InmateId ?? 0).ToArray();
            int[] priId = schedule.Select(s => s.Visit.LocationId ?? 0).ToArray();
            List<Inmate> inmate = _context.Inmate.Where(s => lstId.Contains(s.InmateId)).ToList();
            List<Privileges> privilege = _context.Privileges.Where(x => priId.Contains(x.PrivilegeId)).ToList();
            schedule.ForEach(item =>
            {
                AppointmentVm app = new AppointmentVm();
                //VisitorPersonal visitorPersonal = _context.VisitorPersonal.SingleOrDefault(w => w.PersonId == item.PersonId);
                //VisitorProfessional visitorProfessional = _context.VisitorProfessional.SingleOrDefault(w => w.PersonId == item.PersonId);
                if (!item.Visit.VisitorType.HasValue)
                {
                    app.ApptId = item.ScheduleId;
                    app.InmateId = item.Visit.InmateId;
                    app.PersonId = inmate.First(s => s.InmateId == item.Visit.InmateId).PersonId;
                    app.ApptLocation = privilege.First(s => s.PrivilegeId == item.Visit.LocationId).PrivilegeDescription;
                    app.ApptLocationId = item.Visit.LocationId;
                    app.ApptDate = item.Visit.StartDate;
                    app.InmateNumber = inmate.First(s => s.InmateId == item.Visit.InmateId).InmateNumber;
                    app.VisitType = false;
                    visitSchedule.Add(app);
                }
                else if (item.Visit.VisitorType == 1)
                {
                    app.ApptId = item.ScheduleId;
                    app.InmateId = item.Visit.InmateId;
                    app.PersonId = inmate.First(s => s.InmateId == item.Visit.InmateId).PersonId;
                    app.ApptLocation = privilege.First(s => s.PrivilegeId == item.Visit.LocationId).PrivilegeDescription;
                    app.ApptLocationId = item.Visit.LocationId;
                    app.ApptDate = item.Visit.StartDate;
                    app.InmateNumber = inmate.First(s => s.InmateId == item.Visit.InmateId).InmateNumber;
                    app.VisitType = true;
                    visitSchedule.Add(app);
                }
            });
            return visitSchedule;
        }
        private IQueryable<AppointmentVm> GetTodayAppointment(ICollection<int> inmateList, int moveLocationId, DateTime? startDate, DateTime? endDate, int scheduleId)
        {
            IQueryable<ScheduleInmate> appointment = _context.Schedule.OfType<ScheduleInmate>().Where(s => !(s is Visit));

            IQueryable<ScheduleInmate> lstTodayAppsFilter = appointment.Where(a =>
                a.StartDate != null && a.EndDate != null && a.StartDate.Date == startDate.Value.Date &&
                a.EndDate.Value.Date == endDate.Value.Date && startDate < a.EndDate && endDate >= a.EndDate ||
                startDate <= a.StartDate && endDate > a.StartDate || startDate == a.StartDate && endDate == a.EndDate ||
                startDate > a.StartDate && endDate < a.EndDate && a.LocationId != moveLocationId && inmateList.Contains(a.Inmate.InmateId));

            lstTodayAppsFilter = lstTodayAppsFilter.Where(s => s.ScheduleId != scheduleId && s.InmateId != null);

            IQueryable<AppointmentVm> lstTodayApps = lstTodayAppsFilter.Select(a => new
            {
                ApptId = a.ScheduleId,
                a.InmateId,
                a.Inmate.PersonId,
                ApptLocation = a.Location.PrivilegeDescription,
                ApptLocationId = a.LocationId,
                ApptDate = a.StartDate,
                a.Inmate.InmateNumber
            }).Distinct().Select(a => new AppointmentVm
            {
                ApptId = a.ApptId,
                InmateId = a.InmateId,
                PersonId = a.PersonId,
                ApptLocation = a.ApptLocation,
                ApptLocationId = a.ApptLocationId,
                ApptDate = a.ApptDate,
                InmateNumber = a.InmateNumber
            });

            return lstTodayApps;
        }
        //To Get Privilege Conflict Details
        private void LoadPrivilegeConflict(ICollection<int> inmateList, int moveLocationId)
        {
            DateTime privilegeDate = DateTime.Now;
            _trackingConflictDetails.AddRange(_context.InmatePrivilegeXref.Where(
                ipx => ipx.PrivilegeId == moveLocationId
                && ipx.Privilege.PrivilegeType == ConflictTypeConstants.REVOKE &&
                inmateList.Contains(ipx.InmateId)
                && ipx.Privilege.InactiveFlag == 0 &&
                !ipx.PrivilegeRemoveOfficerId.HasValue &&
                ipx.PrivilegeDate <= privilegeDate
                && (ipx.PrivilegeExpires > privilegeDate || !ipx.PrivilegeExpires.HasValue))
                .Select(ipx => new TrackingConflictVm
                {
                    InmateId = ipx.Inmate.InmateId,
                    ConflictType = ClassTypeConstants.PRIVILEGE,
                    PersonId = ipx.Inmate.PersonId,
                    ConflictDescription = ipx.Privilege.PrivilegeDescription,
                    Note = ipx.PrivilegeNote,
                    InmateNumber = ipx.Inmate.InmateNumber

                }).ToList());
        }

        //To Get Location OOS & Capacity
        private void LoadLocationOverCapacity(ICollection<int> inmateList, int moveLocationId, int inmateCount)
        {
            int existingInmateCount =
                _context.Inmate.Count(a => a.InmateActive == 1 && a.InmateCurrentTrackId == moveLocationId);

            Privileges selLocation = _context.Privileges.
                Single(p =>p.PrivilegeId == moveLocationId);

            if (selLocation.Capacity < existingInmateCount + inmateCount)
            {
                _trackingConflictDetails.Add(new TrackingConflictVm
                {
                    ConflictType = ConflictTypeConstants.LOCATIONOVERCAPACITY,
                    Location = selLocation.PrivilegeDescription,
                    InmateCount = existingInmateCount,
                    Capacity = selLocation.Capacity
                });
            }

            if (selLocation.LocationOutOfService.HasValue)
            {
                _trackingConflictDetails.Add(new TrackingConflictVm
                {
                    ConflictType = ConflictTypeConstants.LOCATIONOUTOFSERVICE,
                    Location = selLocation.PrivilegeDescription,
                    ConflictDescription = selLocation.LocationOutOfServiceReason,
                    OutofService = selLocation.LocationOutOfService
                });
            }

            IQueryable<InmatePrivilegeXref> lstInmateLockQueryable = _context.InmatePrivilegeXref.Where(ipx =>
                inmateList.Contains(ipx.InmateId) && !ipx.PrivilegeRemoveOfficerId.HasValue);

            _trackingConflictDetails.AddRange(lstInmateLockQueryable.Where(w =>
                    w.PrivilegeDate < DateTime.Now
                    && (!w.PrivilegeExpires.HasValue ||
                        w.PrivilegeExpires >= DateTime.Now)
                    && (w.PrivilegeRemoveDatetime ?? DateTime.Now.AddDays(1)) > DateTime.Now)
                .OrderByDescending(o => o.InmatePrivilegeXrefId)
                .Select(ipx => new TrackingConflictVm
                {
                    ConflictType = ConflictTypeConstants.INMATELOCKDOWNCONFLICT,
                    InmateNumber = ipx.Inmate.InmateNumber,
                    InmateId = ipx.InmateId,
                    PersonId = ipx.Inmate.PersonId,
                    LocationId = ipx.PrivilegeId,
                    Location = ipx.Privilege.PrivilegeDescription,
                    FromDate = ipx.PrivilegeDate,
                    ConflictDate = ipx.PrivilegeExpires,
                    Note = ipx.Privilege.PrivilegeType
                }));

            if (selLocation.ConflictCheckRoomPrivilege != 1)
            {
                return;
            }
            IQueryable<TrackingConflictVm> lstInmatePrivilegexref =
                from ipx in _context.InmatePrivilegeXref
                where inmateList.Contains(ipx.InmateId) && ipx.PrivilegeExpires >= DateTime.Now &&
                !ipx.PrivilegeExpires.HasValue && !ipx.PrivilegeRemoveDatetime.HasValue
                && ipx.Privilege.RoomPrivilegeFlag == 1
                select new TrackingConflictVm
                {
                    LocationId = ipx.PrivilegeId,
                    InmateId = ipx.InmateId,
                    PersonId = ipx.Inmate.PersonId,
                    InmateNumber = ipx.Inmate.InmateNumber
                };

            _trackingConflictDetails.AddRange(
                from ipx in lstInmatePrivilegexref
                from priv in _context.Privileges
                where ipx.LocationId == priv.PrivilegeId && priv.InactiveFlag == 0
                orderby ipx.InmateId
                select new TrackingConflictVm
                {
                    ConflictType = ConflictTypeConstants.ROOMPRIVILEGECONFLICT,
                    Location = "[" + priv.PrivilegeDescription + "] " +
                        selLocation.PrivilegeDescription,
                    LocationId = ipx.LocationId,
                    InmateId = ipx.InmateId,
                    PersonId = ipx.PersonId,
                    InmateNumber = ipx.InmateNumber
                });
        }

        //To get Gender Conflict Details
        private void LoadGenderConflict(ICollection<int> inmateList, int moveLocationId)
        {
            List<TrackingConflictVm> inmateLsts =
            (from inm in _context.Inmate
             where inm.InmateActive == 1
             select new TrackingConflictVm
             {
                 PersonSexLast = inm.Person.PersonSexLast,
                 InmateId = inm.InmateId,
                 PersonId = inm.PersonId,
                 InmateCurrentTrack = inm.InmateCurrentTrack,
                 InmateCurrentTrackId = inm.InmateCurrentTrackId,
                 InmateNumber = inm.InmateNumber,
                 FacilityId = inm.FacilityId
             }).ToList();

            bool genderConflict = _context.Privileges.First(w => w.PrivilegeId == moveLocationId).RemoveCoflictCheckGender == 1;

            if (genderConflict) return;
            //Select Inmate List 
            IEnumerable<TrackingConflictVm> selInmateList = inmateLsts.Where(i => inmateList.Contains(i.InmateId));

            //Move Location Inmate List
            IEnumerable<TrackingConflictVm> locationInmateList = inmateLsts.Where(
                i => i.InmateCurrentTrackId == moveLocationId);

            IEnumerable<Lookup> lookUpList = _commonService.GetLookupList(LookupConstants.SEX);

            List<TrackingConflictVm> sInmate = (from inm in selInmateList
                                                select new TrackingConflictVm
                                                {
                                                    PersonSexLast = inm.PersonSexLast,
                                                    InmateId = inm.InmateId,
                                                    PersonId = inm.PersonId,
                                                    InmateCurrentTrack = inm.InmateCurrentTrack,
                                                    InmateCurrentTrackId = inm.InmateCurrentTrackId,
                                                    InmateNumber = inm.InmateNumber,
                                                    LookupDescription = lookUpList.FirstOrDefault(lk =>
                                                        lk.LookupIndex == inm.PersonSexLast)?.LookupDescription
                                                }).ToList();

            _trackingConflictDetails.AddRange((
                from in1 in sInmate
                from in2 in sInmate
                where in1.PersonSexLast != in2.PersonSexLast
                      && in1.PersonSexLast.HasValue && in1.PersonSexLast > 0
                      && in2.PersonSexLast.HasValue && in2.PersonSexLast > 0
                select new
                {
                    in1.PersonId,
                    in1.InmateNumber,
                    in1.InmateId,
                    Reason = in1.LookupDescription,
                }).Distinct().Select(inm => new TrackingConflictVm
                {
                    ConflictType = ConflictTypeConstants.INMATEGENDERCONFLICT,
                    PersonId = inm.PersonId,
                    InmateNumber = inm.InmateNumber,
                    InmateId = inm.InmateId,
                    Reason = inm.Reason,
                    ConflictDescription = ConflictTypeConstants.DIFFERENTGENDER
                }));

            List<TrackingConflictVm> inmateLocList = (from i in locationInmateList
                                                      from look in lookUpList
                                                      where i.PersonSexLast == look.LookupIndex && i.PersonSexLast > 0
                                                      select new TrackingConflictVm
                                                      {
                                                          InmateId = i.InmateId,
                                                          PersonId = i.PersonId,
                                                          InmateCurrentTrack = i.InmateCurrentTrack,
                                                          InmateCurrentTrackId = i.InmateCurrentTrackId,
                                                          InmateNumber = i.InmateNumber,
                                                          LookupDescription = look.LookupDescription,
                                                          LookupIndex = look.LookupIndex
                                                      }).ToList();

            if (inmateLocList.Count > 0)
            {
                _trackingConflictDetails.AddRange((from iLocList1 in inmateLocList
                                                   from iLocList2 in sInmate
                                                   where (int?)iLocList1.LookupIndex != iLocList2.PersonSexLast
                                                         && iLocList2.PersonSexLast.HasValue && iLocList2.PersonSexLast > 0
                                                   select new
                                                   {
                                                       iLocList2.InmateId,
                                                       iLocList2.InmateNumber,
                                                       Reason = iLocList1.LookupDescription,
                                                       iLocList2.PersonId,
                                                       Location = iLocList1.InmateCurrentTrack
                                                   }).Distinct().Select(inm => new TrackingConflictVm
                                                   {
                                                       ConflictType = ConflictTypeConstants.LOCATIONGENDERCONFLICT,
                                                       InmateId = inm.InmateId,
                                                       InmateNumber = inm.InmateNumber,
                                                       Reason = inm.Reason,
                                                       PersonId = inm.PersonId,
                                                       Location = inm.Location
                                                   }));
            }
        }

        private IQueryable<AppointmentVm> GetTodaysAppointment(ICollection<int> inmateList, int moveLocationId)
        {
            IQueryable<ScheduleInmate> lstTodayApptsFilter = _context.ScheduleInmate.Where(a =>
                a.LocationId != moveLocationId
                && inmateList.Contains(a.Inmate.InmateId));

            IQueryable<ScheduleInmate> lstTodayApptsFilter1 = lstTodayApptsFilter.Where(a =>
                a.IsSingleOccurrence || a.EndDate.HasValue &&
                a.EndDate.Value >= DateTime.Now);

            IQueryable<ScheduleInmate> lstTodayApptsFilter2 =
                lstTodayApptsFilter1.Where(a => a.EndDate.HasValue);

            IQueryable<AppointmentVm> lstTodayAppts = lstTodayApptsFilter2.Where(a => !a.IsSingleOccurrence
                || a.FrequencyType != 0)
                .Select(a => new
                {
                    ApptId = a.ScheduleId,
                    a.InmateId,
                    a.Inmate.PersonId,
                    ApptLocation = a.Location.PrivilegeDescription,
                    ApptLocationId = a.LocationId,
                    ApptDate = a.IsSingleOccurrence ? DateTime.Now.Date.AddHours(a.StartDate.Hour)
                        .AddMinutes(a.StartDate.Minute)
                        : a.StartDate,
                    a.Inmate.InmateNumber
                }).Distinct().Select(a => new AppointmentVm
                {
                    ApptId = a.ApptId,
                    InmateId = a.InmateId,
                    PersonId = a.PersonId,
                    ApptLocation = a.ApptLocation,
                    ApptLocationId = a.ApptLocationId,
                    ApptDate = a.ApptDate,
                    InmateNumber = a.InmateNumber
                });

            return lstTodayAppts;
        }

        private void LoadVisitationConflict(List<AppointmentVm> lstTodayAppts, ICollection<int> inmateList) =>
            _trackingConflictDetails.AddRange(lstTodayAppts
                .Where(s => s.InmateId.HasValue && inmateList.Contains(s.InmateId.Value)).Select(t =>
                    new TrackingConflictVm
                    {
                        ConflictType = t.VisitType ? ConflictTypeConstants.VISITPROFESSIONALCONFLICT :
                            ConflictTypeConstants.VISITSOCIALCONFLICT,
                        PersonId = t.PersonId,
                        InmateId = t.InmateId ?? 0,
                        Location = t.ApptLocation,
                        InmateNumber = t.InmateNumber,
                        ConflictDate = t.ApptDate
                    }).ToList());
        private void LoadAppointmentConflict(IQueryable<AppointmentVm> lstTodayAppts, ICollection<int> inmateList) =>
            _trackingConflictDetails.AddRange((from lsapp in lstTodayAppts
                                               where lsapp.InmateId.HasValue && inmateList.Contains(lsapp.InmateId.Value)
                                               select new TrackingConflictVm
                                               {
                                                   ConflictType = ConflictTypeConstants.POSSIBLEAPPOINTMENTCONFLICT,
                                                   PersonId = lsapp.PersonId,
                                                   InmateId = lsapp.InmateId ?? 0,
                                                   Location = lsapp.ApptLocation,
                                                   InmateNumber = lsapp.InmateNumber,
                                                   ConflictDate = lsapp.ApptDate
                                               }).ToList());

        private void LoadKeepSeparateConflict(ICollection<int> inmateList, int moveLocatoinId)
        {
            List<TrackingConflictVm> lstKeepSeps = new List<TrackingConflictVm>();

            IEnumerable<Inmate> lstInmate =
                _context.Inmate.Where(i => i.InmateActive == 1 &&
                (i.InmateCurrentTrackId == moveLocatoinId || inmateList.Contains(i.InmateId)))
                .Select(i => new Inmate
                {
                    InmateId = i.InmateId,
                    InmateNumber = i.InmateNumber,
                    InmateCurrentTrackId = i.InmateCurrentTrackId,
                    PersonId = i.PersonId
                }).ToList();

            bool genderConflict = _context.Privileges.First(w => w.PrivilegeId == moveLocatoinId).RemoveCoflictCheckKeepSep == 1;
            if (genderConflict) return;
            IQueryable<PersonClassification> dbPersonClassifications = _context.PersonClassification
                .Where(pc => pc.InactiveFlag == 0 && pc.PersonClassificationDateFrom < DateTime.Now
                    && (!pc.PersonClassificationDateThru.HasValue || pc.PersonClassificationDateThru >= DateTime.Now));

            //TODO: Move evaluation from server to client... why do we need SelectMany?
            List<TrackingConflictVm> lstPersonClassify =
                dbPersonClassifications.SelectMany(pc => lstInmate.Where(i => pc.PersonId == i.PersonId),
                    (pc, i) => new TrackingConflictVm
                    {
                        InmateId = i.InmateId,
                        InmateNumber = i.InmateNumber,
                        PersonId = pc.PersonId,
                        InmateCurrentTrackId = i.InmateCurrentTrackId,
                        PersonClassificationType = pc.PersonClassificationType,
                        PersonClassificationSubset = pc.PersonClassificationSubset,
                        PersonClassificationTypeId = pc.PersonClassificationTypeId,
                        PersonClassificationSubsetId = pc.PersonClassificationSubsetId,

                    }).ToList();

            //Move Location Inmate List
            List<TrackingConflictVm> lstPerClassifyLocInmates =
                lstPersonClassify.Where(p => p.InmateCurrentTrackId == moveLocatoinId).ToList();

            //Selected Inmate List
            List<TrackingConflictVm> lstPerClassifySelectedInmates =
                lstPersonClassify.Where(i => inmateList.Contains(i.InmateId)).ToList();

            List<int> locationInmateList = lstInmate.Where(p => p.InmateCurrentTrackId == moveLocatoinId)
                .Select(p => p.InmateId).ToList();

            #region Assoc To Assoc
            List<Lookup> lookupslist = _context.Lookup.Where(w => w.LookupType == LookupConstants.CLASSGROUP).ToList();
            List<Lookup> lookupsSublist = _context.Lookup.Where(w => w.LookupType == LookupConstants.CLASSGROUPSUB).ToList();
            //Keep Sep Details for Assoc - Assoc
            IQueryable<KeepSepAssocAssoc> tkeepSepAssocAssoc =
                _context.KeepSepAssocAssoc.Where(ksaa => ksaa.DeleteFlag == 0);

            if (lstPerClassifyLocInmates.Count > 0)
            {
                List<TrackingConflictVm> keepSepAssocAssoc =
                    (from ksaa in tkeepSepAssocAssoc
                     from lpcl in lstPerClassifyLocInmates
                     where lpcl.PersonClassificationTypeId == ksaa.KeepSepAssoc1Id
                     select new TrackingConflictVm
                     {
                         PersonId = lpcl.PersonId,
                         InmateId = lpcl.InmateId,
                         InmateNumber = lpcl.InmateNumber,
                         KeepSepReason = ksaa.KeepSepReason,
                         KeepSepAssoc2 = lookupslist.SingleOrDefault(k => k.LookupIndex == ksaa.KeepSepAssoc2Id).LookupDescription,
                         KeepSepAssoc1 = lookupslist.SingleOrDefault(k => k.LookupIndex == ksaa.KeepSepAssoc1Id).LookupDescription,
                         KeepSepAssoc1Id = ksaa.KeepSepAssoc1Id,
                         KeepSepAssoc2Id = ksaa.KeepSepAssoc2Id
                     }).ToList();

                List<TrackingConflictVm> tKeepSepAssocAssoc = (from ksaa in tkeepSepAssocAssoc
                                                               from lpcl in lstPerClassifyLocInmates
                                                               where lpcl.PersonClassificationTypeId == ksaa.KeepSepAssoc2Id
                                                               select new TrackingConflictVm
                                                               {
                                                                   PersonId = lpcl.PersonId,
                                                                   InmateId = lpcl.InmateId,
                                                                   InmateNumber = lpcl.InmateNumber,
                                                                   KeepSepReason = ksaa.KeepSepReason,
                                                                   KeepSepAssoc2 = lookupslist.SingleOrDefault(k => k.LookupIndex == ksaa.KeepSepAssoc2Id)
                                                                       .LookupDescription,
                                                                   KeepSepAssoc1 = lookupslist.SingleOrDefault(k => k.LookupIndex == ksaa.KeepSepAssoc1Id)
                                                                       .LookupDescription,
                                                                   KeepSepAssoc1Id = ksaa.KeepSepAssoc1Id,
                                                                   KeepSepAssoc2Id = ksaa.KeepSepAssoc2Id
                                                               }).ToList();

                lstKeepSeps = (from ksaa in keepSepAssocAssoc
                               from lksaas in lstPerClassifySelectedInmates
                               where lksaas.PersonClassificationTypeId == ksaa.KeepSepAssoc2Id
                                     && ksaa.PersonId != lksaas.PersonId
                               select new TrackingConflictVm
                               {
                                   ConflictType = KeepSepLabel.ASSOCKEEPSEPASSOC,
                                   KeepSepPersonId = ksaa.PersonId,
                                   KeepSepInmateId = ksaa.InmateId,
                                   KeepSepInmateNumber = ksaa.InmateNumber,
                                   ConflictDescription = ksaa.KeepSepReason,
                                   KeepSepAssoc1 = ksaa.KeepSepAssoc2,
                                   KeepSepAssoc2 = ksaa.KeepSepAssoc1,
                                   PersonId = lksaas.PersonId,
                                   InmateId = lksaas.InmateId,
                                   InmateNumber = lksaas.InmateNumber,
                                   KeepSepAssoc1Id = ksaa.KeepSepAssoc2Id,
                                   KeepSepAssoc2Id = ksaa.KeepSepAssoc1Id
                               }).ToList();

                lstKeepSeps.AddRange(from ksaa in tKeepSepAssocAssoc
                                     from lksaas in lstPerClassifySelectedInmates
                                     where lksaas.PersonClassificationTypeId == ksaa.KeepSepAssoc1Id
                                           && ksaa.PersonId != lksaas.PersonId
                                     select new TrackingConflictVm
                                     {
                                         ConflictType = KeepSepLabel.ASSOCKEEPSEPASSOC,
                                         ConflictDescription = ksaa.KeepSepReason,
                                         KeepSepAssoc1 = ksaa.KeepSepAssoc1,
                                         KeepSepAssoc2 = ksaa.KeepSepAssoc2,
                                         PersonId = lksaas.PersonId,
                                         InmateId = lksaas.InmateId,
                                         InmateNumber = lksaas.InmateNumber,
                                         KeepSepPersonId = ksaa.PersonId,
                                         KeepSepInmateId = ksaa.InmateId,
                                         KeepSepInmateNumber = ksaa.InmateNumber,
                                         KeepSepAssoc1Id = ksaa.KeepSepAssoc1Id,
                                         KeepSepAssoc2Id = ksaa.KeepSepAssoc2Id
                                     });
            }

            if (lstPerClassifySelectedInmates.Count > 0)
            {
                List<TrackingConflictVm> tLstKeepSepAssocAssoc = (from ksaa in tkeepSepAssocAssoc
                                                                  from lpcl in lstPerClassifySelectedInmates
                                                                  where lpcl.PersonClassificationTypeId == ksaa.KeepSepAssoc1Id
                                                                  select new TrackingConflictVm
                                                                  {
                                                                      PersonId = lpcl.PersonId,
                                                                      InmateId = lpcl.InmateId,
                                                                      InmateNumber = lpcl.InmateNumber,
                                                                      KeepSepReason = ksaa.KeepSepReason,
                                                                      KeepSepAssoc2 = lookupslist.SingleOrDefault(k => k.LookupIndex == ksaa.KeepSepAssoc2Id)
                                                                          .LookupDescription,
                                                                      KeepSepAssoc1 = lookupslist.SingleOrDefault(k => k.LookupIndex == ksaa.KeepSepAssoc1Id)
                                                                          .LookupDescription,
                                                                      KeepSepAssoc1Id = ksaa.KeepSepAssoc1Id,
                                                                      KeepSepAssoc2Id = ksaa.KeepSepAssoc2Id
                                                                  }).ToList();

                List<TrackingConflictVm> keepSepAssocAssocS = (from ksaa in tkeepSepAssocAssoc
                                                               from lpcl in lstPerClassifySelectedInmates
                                                               where lpcl.PersonClassificationTypeId == ksaa.KeepSepAssoc2Id
                                                               select new TrackingConflictVm
                                                               {
                                                                   PersonId = lpcl.PersonId,
                                                                   InmateId = lpcl.InmateId,
                                                                   InmateNumber = lpcl.InmateNumber,
                                                                   KeepSepReason = ksaa.KeepSepReason,
                                                                   KeepSepAssoc2 = lookupslist.SingleOrDefault(k => k.LookupIndex == ksaa.KeepSepAssoc2Id)
                                                                       .LookupDescription,
                                                                   KeepSepAssoc1 = lookupslist.SingleOrDefault(k => k.LookupIndex == ksaa.KeepSepAssoc1Id)
                                                                       .LookupDescription,
                                                                   KeepSepAssoc2Id = ksaa.KeepSepAssoc2Id,
                                                                   KeepSepAssoc1Id = ksaa.KeepSepAssoc1Id
                                                               }).ToList();

                lstKeepSeps.AddRange(from ksaa in tLstKeepSepAssocAssoc
                                     from lksaas in lstPerClassifySelectedInmates
                                     where lksaas.PersonClassificationTypeId == ksaa.KeepSepAssoc2Id
                                           && ksaa.PersonId != lksaas.PersonId
                                     select new TrackingConflictVm
                                     {
                                         ConflictType = KeepSepLabel.ASSOCKEEPSEPASSOC,
                                         ConflictDescription = ksaa.KeepSepReason,
                                         KeepSepAssoc1 = ksaa.KeepSepAssoc1,
                                         KeepSepAssoc2 = ksaa.KeepSepAssoc2,
                                         PersonId = lksaas.PersonId,
                                         InmateId = lksaas.InmateId,
                                         InmateNumber = lksaas.InmateNumber,
                                         KeepSepPersonId = ksaa.PersonId,
                                         KeepSepInmateId = ksaa.InmateId,
                                         KeepSepInmateNumber = ksaa.InmateNumber,
                                         KeepSepAssoc2Id = ksaa.KeepSepAssoc2Id,
                                         KeepSepAssoc1Id = ksaa.KeepSepAssoc1Id
                                     });

                lstKeepSeps.AddRange(from ksaa in keepSepAssocAssocS
                                     from lksaas in lstPerClassifySelectedInmates
                                     where lksaas.PersonClassificationTypeId == ksaa.KeepSepAssoc1Id
                                           && ksaa.PersonId != lksaas.PersonId
                                     select new TrackingConflictVm
                                     {
                                         ConflictType = KeepSepLabel.ASSOCKEEPSEPASSOC,
                                         ConflictDescription = ksaa.KeepSepReason,
                                         KeepSepAssoc1 = ksaa.KeepSepAssoc1,
                                         KeepSepAssoc2 = ksaa.KeepSepAssoc2,
                                         PersonId = lksaas.PersonId,
                                         InmateId = lksaas.InmateId,
                                         InmateNumber = lksaas.InmateNumber,
                                         KeepSepPersonId = ksaa.PersonId,
                                         KeepSepInmateId = ksaa.InmateId,
                                         KeepSepInmateNumber = ksaa.InmateNumber,
                                         KeepSepAssoc2Id = ksaa.KeepSepAssoc2Id,
                                         KeepSepAssoc1Id = ksaa.KeepSepAssoc1Id
                                     });
            }
            #endregion

            #region Assoc To Inmate

            //Keep Sep Details for Assoc - Inmate && Inmate - Assoc
            IQueryable<KeepSepAssocInmate> tKeepSepAssocInmate =
                _context.KeepSepAssocInmate.Where(ksa =>
                    ksa.DeleteFlag == 0 && ksa.KeepSepInmate2.InmateCurrentTrackId == moveLocatoinId &&
                    ksa.KeepSepInmate2.InmateActive == 1);

            IQueryable<KeepSepAssocInmate> tKeepSepInmateAssoc = _context.KeepSepAssocInmate.Where(ksa =>
                ksa.DeleteFlag == 0 && inmateList.Contains(ksa.KeepSepInmate2Id));

            if (lstPerClassifySelectedInmates.Count > 0)
            {
                lstKeepSeps.AddRange(from ksa in tKeepSepAssocInmate
                                     from lksa in lstPerClassifySelectedInmates
                                     where lksa.PersonClassificationTypeId == ksa.KeepSepAssoc1Id
                                     select new TrackingConflictVm
                                     {
                                         ConflictType = KeepSepLabel.ASSOCKEEPSEPINMATE,
                                         KeepSepPersonId = ksa.KeepSepInmate2.PersonId,
                                         KeepSepInmateId = ksa.KeepSepInmate2Id,
                                         KeepSepInmateNumber = ksa.KeepSepInmate2.InmateNumber,
                                         PersonId = lksa.PersonId,
                                         InmateId = lksa.InmateId,
                                         InmateNumber = lksa.InmateNumber,
                                         KeepSepAssoc1 = lookupslist.SingleOrDefault(k => k.LookupIndex == ksa.KeepSepAssoc1Id)
                                             .LookupDescription,
                                         KeepSepAssoc1Id = ksa.KeepSepAssoc1Id,
                                         KeepSepType = ksa.KeepSeparateType,
                                         ConflictDescription = ksa.KeepSepReason
                                     });


                lstKeepSeps.AddRange(from ksa in tKeepSepInmateAssoc
                                     from lksa in lstPerClassifySelectedInmates
                                     where lksa.PersonClassificationTypeId == ksa.KeepSepAssoc1Id
                                     select new TrackingConflictVm
                                     {
                                         ConflictType = KeepSepLabel.ASSOCKEEPSEPINMATE,
                                         KeepSepPersonId = ksa.KeepSepInmate2.PersonId,
                                         KeepSepInmateId = ksa.KeepSepInmate2Id,
                                         KeepSepInmateNumber = ksa.KeepSepInmate2.InmateNumber,
                                         PersonId = lksa.PersonId,
                                         InmateId = lksa.InmateId,
                                         InmateNumber = lksa.InmateNumber,
                                         KeepSepType = ksa.KeepSeparateType,
                                         ConflictDescription = ksa.KeepSepReason,
                                         KeepSepAssoc1 = lookupslist.SingleOrDefault(k => k.LookupIndex == ksa.KeepSepAssoc1Id)
                                             .LookupDescription,
                                         KeepSepAssoc1Id = ksa.KeepSepAssoc1Id,
                                     });
            }
            #endregion

            #region Assoc To Subset

            //Keep Sep Details for Assoc - Subset && Subset - Assoc
            IQueryable<KeepSepAssocSubset> tKeepSepAssocSubset =
                _context.KeepSepAssocSubset.Where(ksas => ksas.DeleteFlag == 0);

            if (lstPerClassifySelectedInmates.Count > 0)
            {
                IEnumerable<TrackingConflictVm> lstKeepSepAssocSubset = (from ksas in tKeepSepAssocSubset
                                                                         from ksaas in lstPerClassifySelectedInmates
                                                                         where ksas.KeepSepAssoc1Id == ksaas.PersonClassificationTypeId
                                                                         select new TrackingConflictVm
                                                                         {
                                                                             PersonId = ksaas.PersonId,
                                                                             InmateId = ksaas.InmateId,
                                                                             InmateNumber = ksaas.InmateNumber,
                                                                             KeepSepAssoc1 = lookupslist.SingleOrDefault(s => s.LookupIndex == ksas.KeepSepAssoc1Id)
                                                                                 .LookupDescription,
                                                                             KeepSepAssoc2 = lookupslist.SingleOrDefault(s => s.LookupIndex == ksas.KeepSepAssoc2Id)
                                                                                 .LookupDescription,
                                                                             KeepSepAssoc2Subset = lookupsSublist
                                                                                 .SingleOrDefault(s => s.LookupIndex == ksas.KeepSepAssoc2SubsetId).LookupDescription,
                                                                             KeepSepAssoc1Id = ksas.KeepSepAssoc1Id,
                                                                             KeepSepAssoc2Id = ksas.KeepSepAssoc2Id,
                                                                             KeepSepAssoc2SubsetId = ksas.KeepSepAssoc2SubsetId,
                                                                         }).ToList();

                lstKeepSeps.AddRange(from ksas in lstKeepSepAssocSubset
                                     from lksa in lstPerClassifyLocInmates
                                     where ksas.KeepSepAssoc2Id == lksa.PersonClassificationTypeId
                                           && ksas.KeepSepAssoc2SubsetId == lksa.PersonClassificationSubsetId
                                           && ksas.PersonId != lksa.PersonId
                                     select new TrackingConflictVm
                                     {
                                         ConflictType = KeepSepLabel.ASSOCKEEPSEPSUBSET,
                                         PersonId = ksas.PersonId,
                                         InmateId = ksas.InmateId,
                                         InmateNumber = ksas.InmateNumber,
                                         KeepSepPersonId = lksa.PersonId,
                                         KeepSepInmateId = lksa.InmateId,
                                         KeepSepInmateNumber = lksa.InmateNumber,
                                         KeepSepAssoc1 = ksas.KeepSepAssoc1,
                                         KeepSepAssoc2 = ksas.KeepSepAssoc2,
                                         KeepSepAssocSubset2 = ksas.KeepSepAssoc2Subset,
                                         KeepSepAssoc1Id = ksas.KeepSepAssoc1Id,
                                         KeepSepAssoc2Id = ksas.KeepSepAssoc2Id,
                                         KeepSepAssocSubset2Id = ksas.KeepSepAssoc2SubsetId,
                                     });

                lstKeepSeps.AddRange(from ksas in lstKeepSepAssocSubset
                                     from lksa in lstPerClassifySelectedInmates
                                     where ksas.KeepSepAssoc2Id == lksa.PersonClassificationTypeId
                                           && ksas.KeepSepAssoc2SubsetId == lksa.PersonClassificationSubsetId
                                           && ksas.PersonId != lksa.PersonId
                                     select new TrackingConflictVm
                                     {
                                         ConflictType = KeepSepLabel.ASSOCKEEPSEPSUBSET,
                                         PersonId = ksas.PersonId,
                                         InmateId = ksas.InmateId,
                                         InmateNumber = ksas.InmateNumber,
                                         KeepSepPersonId = lksa.PersonId,
                                         KeepSepInmateId = lksa.InmateId,
                                         KeepSepInmateNumber = lksa.InmateNumber,
                                         KeepSepAssoc1 = ksas.KeepSepAssoc1,
                                         KeepSepAssoc2 = ksas.KeepSepAssoc2,
                                         KeepSepAssocSubset2 = ksas.KeepSepAssoc2Subset,
                                         KeepSepAssoc1Id = ksas.KeepSepAssoc1Id,
                                         KeepSepAssoc2Id = ksas.KeepSepAssoc2Id,
                                         KeepSepAssocSubset2Id = ksas.KeepSepAssoc2SubsetId,
                                     });
            }
            #endregion

            #region Inmate To Assoc
            if (tKeepSepInmateAssoc.Any())
            {
                //Keep Seperate for Inmate to Association Check MoveLocation Inmates
                if (lstPerClassifyLocInmates.Any())
                {
                    lstKeepSeps.AddRange(from ksa in tKeepSepInmateAssoc
                                         from lksa in lstPerClassifyLocInmates
                                         where lksa.PersonClassificationTypeId == ksa.KeepSepAssoc1Id
                                         select new TrackingConflictVm
                                         {
                                             ConflictType = KeepSepLabel.INMATEKEEPSEPASSOC,
                                             PersonId = ksa.KeepSepInmate2.PersonId,
                                             InmateId = ksa.KeepSepInmate2.InmateId,
                                             InmateNumber = ksa.KeepSepInmate2.InmateNumber,
                                             KeepSepPersonId = lksa.PersonId,
                                             KeepSepInmateNumber = lksa.InmateNumber,
                                             KeepSepAssoc1 = lookupslist.SingleOrDefault(s => s.LookupIndex == ksa.KeepSepAssoc1Id)
                                                 .LookupDescription,
                                             KeepSepAssoc1Id = ksa.KeepSepAssoc1Id,
                                             KeepSepType = ksa.KeepSeparateType,
                                             ConflictDescription = ksa.KeepSepReason
                                         });
                }

                //Keep Separate for Inmate to Association Check MoveInmates
                if (lstPerClassifySelectedInmates.Any())
                {
                    lstKeepSeps.AddRange(from ksa in tKeepSepInmateAssoc
                                         from lksa in lstPerClassifySelectedInmates
                                         where lksa.PersonClassificationTypeId == ksa.KeepSepAssoc1Id
                                         select new TrackingConflictVm
                                         {
                                             ConflictType = KeepSepLabel.INMATEKEEPSEPASSOC,
                                             PersonId = ksa.KeepSepInmate2.PersonId,
                                             InmateId = ksa.KeepSepInmate2.InmateId,
                                             InmateNumber = ksa.KeepSepInmate2.InmateNumber,
                                             KeepSepPersonId = lksa.PersonId,
                                             KeepSepInmateNumber = lksa.InmateNumber,
                                             KeepSepAssoc1 = lookupslist.SingleOrDefault(s => s.LookupIndex == ksa.KeepSepAssoc1Id)
                                                 .LookupDescription,
                                             KeepSepAssoc1Id = ksa.KeepSepAssoc1Id,
                                             KeepSepType = ksa.KeepSeparateType,
                                             ConflictDescription = ksa.KeepSepReason
                                         });
                }
            }
            #endregion

            #region Inmate To Inmate

            //Keep Sep Inmate 1 List
            IQueryable<KeepSeparate> kepSeparateDetails = from ks in _context.KeepSeparate
                                                          where ks.InactiveFlag == 0 &&
                                                                (inmateList.Contains(ks.KeepSeparateInmate1Id) || inmateList.Contains(ks.KeepSeparateInmate2Id))
                                                          select ks;

            if (kepSeparateDetails.Any())
            {
                lstKeepSeps.AddRange(from ks in kepSeparateDetails
                                     from i in locationInmateList
                                     where ks.KeepSeparateInmate1Id == i
                                     select new TrackingConflictVm
                                     {
                                         ConflictType = KeepSepLabel.INMATEKEEPSEPINMATE,
                                         KeepSepPersonId = ks.KeepSeparateInmate1.PersonId,
                                         KeepSepInmateId = ks.KeepSeparateInmate1Id,
                                         KeepSepInmateNumber = ks.KeepSeparateInmate1.InmateNumber,
                                         PersonId = ks.KeepSeparateInmate2.PersonId,
                                         InmateId = ks.KeepSeparateInmate2Id,
                                         InmateNumber = ks.KeepSeparateInmate2.InmateNumber,
                                         KeepSepType = ks.KeepSeparateType,
                                         ConflictDescription = ks.KeepSeparateReason
                                     });

                lstKeepSeps.AddRange(from ks in kepSeparateDetails
                                     from i in locationInmateList
                                     where ks.KeepSeparateInmate2Id == i
                                     select new TrackingConflictVm
                                     {
                                         ConflictType = KeepSepLabel.INMATEKEEPSEPINMATE,
                                         KeepSepPersonId = ks.KeepSeparateInmate2.PersonId,
                                         KeepSepInmateId = ks.KeepSeparateInmate2Id,
                                         KeepSepInmateNumber = ks.KeepSeparateInmate2.InmateNumber,
                                         PersonId = ks.KeepSeparateInmate1.PersonId,
                                         InmateId = ks.KeepSeparateInmate1Id,
                                         InmateNumber = ks.KeepSeparateInmate1.InmateNumber,
                                         KeepSepType = ks.KeepSeparateType,
                                         ConflictDescription = ks.KeepSeparateReason
                                     });


                lstKeepSeps.AddRange(from ks in kepSeparateDetails
                                     from i in locationInmateList
                                     where ks.KeepSeparateInmate1Id == i
                                           && ks.KeepSeparateInmate2.InmateCurrentTrackId == moveLocatoinId
                                     select new TrackingConflictVm
                                     {
                                         ConflictType = KeepSepLabel.INMATEKEEPSEPINMATE,
                                         KeepSepPersonId = ks.KeepSeparateInmate1.PersonId,
                                         KeepSepInmateId = ks.KeepSeparateInmate1Id,
                                         KeepSepInmateNumber = ks.KeepSeparateInmate1.InmateNumber,
                                         PersonId = ks.KeepSeparateInmate2.PersonId,
                                         InmateId = ks.KeepSeparateInmate2Id,
                                         InmateNumber = ks.KeepSeparateInmate2.InmateNumber,
                                         KeepSepType = ks.KeepSeparateType,
                                         ConflictDescription = ks.KeepSeparateReason
                                     });

                lstKeepSeps.AddRange(from ks in kepSeparateDetails
                                     from i in locationInmateList
                                     where ks.KeepSeparateInmate2Id == i
                                           && ks.KeepSeparateInmate1.InmateCurrentTrackId == moveLocatoinId
                                     select new TrackingConflictVm
                                     {
                                         ConflictType = KeepSepLabel.INMATEKEEPSEPINMATE,
                                         KeepSepPersonId = ks.KeepSeparateInmate2.PersonId,
                                         KeepSepInmateId = ks.KeepSeparateInmate2Id,
                                         KeepSepInmateNumber = ks.KeepSeparateInmate2.InmateNumber,
                                         PersonId = ks.KeepSeparateInmate1.PersonId,
                                         InmateId = ks.KeepSeparateInmate1Id,
                                         InmateNumber = ks.KeepSeparateInmate1.InmateNumber,
                                         KeepSepType = ks.KeepSeparateType,
                                         ConflictDescription = ks.KeepSeparateReason
                                     });

                lstKeepSeps.AddRange(from ks in kepSeparateDetails
                                     from i in inmateList
                                     where ks.KeepSeparateInmate2Id == i
                                           && inmateList.Contains(ks.KeepSeparateInmate1Id)
                                     select new TrackingConflictVm
                                     {
                                         ConflictType = KeepSepLabel.INMATEKEEPSEPINMATE,
                                         KeepSepPersonId = ks.KeepSeparateInmate2.PersonId,
                                         KeepSepInmateId = ks.KeepSeparateInmate2Id,
                                         KeepSepInmateNumber = ks.KeepSeparateInmate2.InmateNumber,
                                         PersonId = ks.KeepSeparateInmate1.PersonId,
                                         InmateId = ks.KeepSeparateInmate1Id,
                                         InmateNumber = ks.KeepSeparateInmate1.InmateNumber,
                                         KeepSepType = ks.KeepSeparateType,
                                         ConflictDescription = ks.KeepSeparateReason
                                     });

                lstKeepSeps.AddRange(from ks in kepSeparateDetails
                                     from i in inmateList
                                     where ks.KeepSeparateInmate1Id == i
                                           && inmateList.Contains(ks.KeepSeparateInmate2Id)
                                     select new TrackingConflictVm
                                     {
                                         ConflictType = KeepSepLabel.INMATEKEEPSEPINMATE,
                                         KeepSepPersonId = ks.KeepSeparateInmate1.PersonId,
                                         KeepSepInmateId = ks.KeepSeparateInmate1Id,
                                         KeepSepInmateNumber = ks.KeepSeparateInmate1.InmateNumber,
                                         PersonId = ks.KeepSeparateInmate2.PersonId,
                                         InmateId = ks.KeepSeparateInmate2Id,
                                         InmateNumber = ks.KeepSeparateInmate2.InmateNumber,
                                         KeepSepType = ks.KeepSeparateType,
                                         ConflictDescription = ks.KeepSeparateReason
                                     });
            }
            #endregion

            #region Inmate To Subset

            IQueryable<KeepSepSubsetInmate> tKeepSepInmateSubset =
                _context.KeepSepSubsetInmate.Where(
                    ksa => ksa.DeleteFlag == 0 && inmateList.Contains(ksa.KeepSepInmate2Id));

            if (tKeepSepInmateSubset.Any())
            {
                //Keep Sep : SUBSET KEEP SEPARATE To INMATE
                if (lstPerClassifyLocInmates.Count > 0)
                {
                    lstKeepSeps.AddRange(from ksa in tKeepSepInmateSubset
                                         from lpcl in lstPerClassifyLocInmates
                                         where lpcl.PersonClassificationTypeId == ksa.KeepSepAssoc1Id
                                             && lpcl.PersonClassificationSubsetId == ksa.KeepSepAssoc1SubsetId
                                         select new TrackingConflictVm
                                         {
                                             ConflictType = KeepSepLabel.INMATEKEEPSEPSUBSET,
                                             PersonId = ksa.KeepSepInmate2.PersonId,
                                             InmateId = ksa.KeepSepInmate2Id,
                                             InmateNumber = ksa.KeepSepInmate2.InmateNumber,
                                             KeepSepPersonId = lpcl.PersonId,
                                             KeepSepInmateId = lpcl.InmateId,
                                             KeepSepInmateNumber = lpcl.InmateNumber,
                                             KeepSepAssoc1 = lookupslist.SingleOrDefault(s => s.LookupIndex == ksa.KeepSepAssoc1Id)
                                                 .LookupDescription,
                                             KeepSepAssocSubset1 = lookupsSublist
                                                 .SingleOrDefault(s => s.LookupIndex == ksa.KeepSepAssoc1SubsetId).LookupDescription,
                                             KeepSepAssocSubset1Id = ksa.KeepSepAssoc1SubsetId,
                                             KeepSepAssoc1Id = ksa.KeepSepAssoc1Id,
                                             KeepSepType = ksa.KeepSeparateType,
                                             ConflictDescription = ksa.KeepSepReason
                                         });
                }

                if (lstPerClassifySelectedInmates.Count > 0)
                {
                    lstKeepSeps.AddRange(from ksa in tKeepSepInmateSubset
                                         from lpcl in lstPerClassifySelectedInmates
                                         where lpcl.PersonClassificationTypeId == ksa.KeepSepAssoc1Id
                                             && lpcl.PersonClassificationSubsetId == ksa.KeepSepAssoc1SubsetId
                                             && lpcl.InmateId != ksa.KeepSepInmate2Id
                                         select new TrackingConflictVm
                                         {
                                             ConflictType = KeepSepLabel.INMATEKEEPSEPSUBSET,
                                             PersonId = ksa.KeepSepInmate2.PersonId,
                                             InmateId = ksa.KeepSepInmate2Id,
                                             InmateNumber = ksa.KeepSepInmate2.InmateNumber,
                                             KeepSepPersonId = lpcl.PersonId,
                                             KeepSepInmateId = lpcl.InmateId,
                                             KeepSepInmateNumber = lpcl.InmateNumber,
                                             KeepSepAssoc1 = lookupslist.SingleOrDefault(s => s.LookupIndex == ksa.KeepSepAssoc1Id)
                                                 .LookupDescription,
                                             KeepSepAssocSubset1 = lookupsSublist
                                                 .SingleOrDefault(s => s.LookupIndex == ksa.KeepSepAssoc1SubsetId).LookupDescription,
                                             KeepSepAssocSubset1Id = ksa.KeepSepAssoc1SubsetId,
                                             KeepSepAssoc1Id = ksa.KeepSepAssoc1Id,
                                             KeepSepType = ksa.KeepSeparateType,
                                             ConflictDescription = ksa.KeepSepReason
                                         });
                }
            }
            #endregion

            #region Subset To Assoc

            List<TrackingConflictVm> selLstKeepSepAssocSubset =
                (from ksas in tKeepSepAssocSubset
                 from ksaas in lstPerClassifySelectedInmates
                 where ksas.KeepSepAssoc2Id == ksaas.PersonClassificationTypeId
                     && ksas.KeepSepAssoc2SubsetId == ksaas.PersonClassificationSubsetId
                 select new TrackingConflictVm
                 {
                     PersonId = ksaas.PersonId,
                     InmateId = ksaas.InmateId,
                     InmateNumber = ksaas.InmateNumber,
                     KeepSepReason = ksas.KeepSepReason,
                     KeepSepAssoc1 = lookupslist.SingleOrDefault(s => s.LookupIndex == ksas.KeepSepAssoc1Id)
                         .LookupDescription,
                     KeepSepAssoc2 = lookupslist.SingleOrDefault(s => s.LookupIndex == ksas.KeepSepAssoc2Id)
                         .LookupDescription,
                     KeepSepAssoc2Subset = lookupsSublist
                         .SingleOrDefault(s => s.LookupIndex == ksas.KeepSepAssoc2SubsetId).LookupDescription,
                     KeepSepAssoc1Id = ksas.KeepSepAssoc1Id,
                     KeepSepAssoc2Id = ksas.KeepSepAssoc2Id,
                     KeepSepAssoc2SubsetId = ksas.KeepSepAssoc2SubsetId
                 }).ToList();

            if (selLstKeepSepAssocSubset.Count > 0)
            {
                lstKeepSeps.AddRange(from ksas in selLstKeepSepAssocSubset
                                     from lpcl in lstPerClassifySelectedInmates
                                     where lpcl.PersonClassificationTypeId == ksas.KeepSepAssoc1Id
                                           && ksas.PersonId != lpcl.PersonId
                                     select new TrackingConflictVm
                                     {
                                         ConflictType = KeepSepLabel.SUBSETKEEPSEPASSOC,

                                         PersonId = ksas.PersonId,
                                         InmateId = ksas.InmateId,
                                         InmateNumber = ksas.InmateNumber,
                                         KeepSepPersonId = lpcl.PersonId,
                                         KeepSepInmateId = lpcl.InmateId,
                                         KeepSepInmateNumber = lpcl.InmateNumber,
                                         KeepSepAssoc1 = ksas.KeepSepAssoc1,
                                         KeepSepAssocSubset1 = ksas.KeepSepAssoc2,
                                         KeepSepAssoc2 = ksas.KeepSepAssoc2Subset,
                                         KeepSepAssoc1Id = ksas.KeepSepAssoc1Id,
                                         KeepSepAssocSubset1Id = ksas.KeepSepAssoc2Id,
                                         KeepSepAssoc2Id = ksas.KeepSepAssoc2SubsetId
                                     });

                lstKeepSeps.AddRange(from ksas in selLstKeepSepAssocSubset
                                     from lpcl in lstPerClassifyLocInmates
                                     where lpcl.PersonClassificationTypeId == ksas.KeepSepAssoc1Id
                                           && ksas.PersonId != lpcl.PersonId
                                     select new TrackingConflictVm
                                     {
                                         ConflictType = KeepSepLabel.SUBSETKEEPSEPASSOC,
                                         PersonId = ksas.PersonId,
                                         InmateId = ksas.InmateId,
                                         InmateNumber = ksas.InmateNumber,
                                         KeepSepPersonId = lpcl.PersonId,
                                         KeepSepInmateId = lpcl.InmateId,
                                         KeepSepInmateNumber = lpcl.InmateNumber,
                                         KeepSepAssoc1 = ksas.KeepSepAssoc1,
                                         KeepSepAssocSubset1 = ksas.KeepSepAssoc2,
                                         KeepSepAssoc2 = ksas.KeepSepAssoc2Subset,
                                         KeepSepAssoc1Id = ksas.KeepSepAssoc1Id,
                                         KeepSepAssocSubset1Id = ksas.KeepSepAssoc2Id,
                                         KeepSepAssoc2Id = ksas.KeepSepAssoc2SubsetId
                                     });
            }
            #endregion

            #region Subset To Inmate

            IQueryable<KeepSepSubsetInmate> tKeepSepSubsetInmate = _context.KeepSepSubsetInmate.Where(ksa =>
                ksa.DeleteFlag == 0 && ksa.KeepSepInmate2.InmateActive == 1 &&
                ksa.KeepSepInmate2.InmateCurrentTrackId == moveLocatoinId);

            if (lstPerClassifySelectedInmates.Count > 0)
            {
                lstKeepSeps.AddRange(from ksa in tKeepSepSubsetInmate
                                     from lksa in lstPerClassifySelectedInmates
                                     where lksa.PersonClassificationTypeId == ksa.KeepSepAssoc1Id
                                         && lksa.PersonClassificationSubsetId == ksa.KeepSepAssoc1SubsetId
                                     select new TrackingConflictVm
                                     {
                                         ConflictType = KeepSepLabel.SUBSETKEEPSEPINMATE,
                                         KeepSepPersonId = ksa.KeepSepInmate2.PersonId,
                                         KeepSepInmateId = ksa.KeepSepInmate2Id,
                                         KeepSepInmateNumber = ksa.KeepSepInmate2.InmateNumber,
                                         PersonId = lksa.PersonId,
                                         InmateId = lksa.InmateId,
                                         InmateNumber = lksa.InmateNumber,
                                         KeepSepType = ksa.KeepSeparateType,
                                         ConflictDescription = ksa.KeepSepReason,
                                         KeepSepAssoc1 = lookupslist.SingleOrDefault(s => s.LookupIndex == ksa.KeepSepAssoc1Id)
                                             .LookupDescription,
                                         KeepSepAssocSubset1 = lookupsSublist
                                             .SingleOrDefault(s => s.LookupIndex == ksa.KeepSepAssoc1SubsetId).LookupDescription,
                                         KeepSepAssoc1Id = ksa.KeepSepAssoc1Id,
                                         KeepSepAssocSubset1Id = ksa.KeepSepAssoc1SubsetId,
                                     });

                lstKeepSeps.AddRange(from ksa in tKeepSepInmateSubset
                                     from lpcl in lstPerClassifySelectedInmates
                                     where lpcl.PersonClassificationTypeId == ksa.KeepSepAssoc1Id
                                         && lpcl.PersonClassificationSubsetId == ksa.KeepSepAssoc1SubsetId
                                         && lpcl.InmateId != ksa.KeepSepInmate2Id
                                     select new TrackingConflictVm
                                     {
                                         ConflictType = KeepSepLabel.SUBSETKEEPSEPINMATE,
                                         KeepSepPersonId = ksa.KeepSepInmate2.PersonId,
                                         KeepSepInmateId = ksa.KeepSepInmate2Id,
                                         KeepSepInmateNumber = ksa.KeepSepInmate2.InmateNumber,
                                         PersonId = lpcl.PersonId,
                                         InmateId = lpcl.InmateId,
                                         InmateNumber = lpcl.InmateNumber,
                                         KeepSepType = ksa.KeepSeparateType,
                                         ConflictDescription = ksa.KeepSepReason,
                                         KeepSepAssoc1 = lookupslist.SingleOrDefault(s => s.LookupIndex == ksa.KeepSepAssoc1Id)
                                             .LookupDescription,
                                         KeepSepAssocSubset1 = lookupsSublist
                                             .SingleOrDefault(s => s.LookupIndex == ksa.KeepSepAssoc1SubsetId).LookupDescription,
                                         KeepSepAssoc1Id = ksa.KeepSepAssoc1Id,
                                         KeepSepAssocSubset1Id = ksa.KeepSepAssoc1SubsetId,
                                     });
            }
            #endregion

            #region Subset To Subset

            if (lstPerClassifyLocInmates.Count > 0)
            {
                IQueryable<TrackingConflictVm> keepSepSubsetSubset = from ksss in _context.KeepSepSubsetSubset
                                                                     from ksaas in lstPerClassifyLocInmates
                                                                     where ksss.KeepSepAssoc1Id == ksaas.PersonClassificationTypeId
                                                                         && ksss.KeepSepAssoc1SubsetId == ksaas.PersonClassificationSubsetId
                                                                         && ksss.DeleteFlag == 0
                                                                     select new TrackingConflictVm
                                                                     {
                                                                         PersonId = ksaas.PersonId,
                                                                         InmateId = ksaas.InmateId,
                                                                         InmateNumber = ksaas.InmateNumber,
                                                                         KeepSepReason = ksss.KeepSepReason,
                                                                         KeepSepAssoc2Id = ksss.KeepSepAssoc2Id,
                                                                         KeepSepAssoc2SubsetId = ksss.KeepSepAssoc2SubsetId,
                                                                         KeepSepAssoc1Id = ksss.KeepSepAssoc1Id,
                                                                         KeepSepAssoc1SubsetId = ksss.KeepSepAssoc1SubsetId,
                                                                         KeepSepAssoc2 = lookupslist.SingleOrDefault(s => s.LookupIndex == ksss.KeepSepAssoc2Id)
                                                                             .LookupDescription,
                                                                         KeepSepAssoc2Subset = lookupsSublist
                                                                             .SingleOrDefault(s => s.LookupIndex == ksss.KeepSepAssoc2SubsetId).LookupDescription,
                                                                         KeepSepAssoc1 = lookupslist.SingleOrDefault(s => s.LookupIndex == ksss.KeepSepAssoc1Id)
                                                                             .LookupDescription,
                                                                         KeepSepAssoc1Subset = lookupsSublist
                                                                             .SingleOrDefault(s => s.LookupIndex == ksss.KeepSepAssoc1SubsetId).LookupDescription,
                                                                     };

                IQueryable<TrackingConflictVm> tKeepSepSubsetSubset = from ksss in _context.KeepSepSubsetSubset
                                                                      from ksaas in lstPerClassifyLocInmates
                                                                      where ksss.KeepSepAssoc2Id == ksaas.PersonClassificationTypeId
                                                                          && ksss.KeepSepAssoc2SubsetId == ksaas.PersonClassificationSubsetId
                                                                          && ksss.DeleteFlag == 0
                                                                      select new TrackingConflictVm
                                                                      {
                                                                          PersonId = ksaas.PersonId,
                                                                          InmateId = ksaas.InmateId,
                                                                          InmateNumber = ksaas.InmateNumber,
                                                                          KeepSepReason = ksss.KeepSepReason,
                                                                          KeepSepAssoc2Id = ksss.KeepSepAssoc2Id,
                                                                          KeepSepAssoc2SubsetId = ksss.KeepSepAssoc2SubsetId,
                                                                          KeepSepAssoc1Id = ksss.KeepSepAssoc1Id,
                                                                          KeepSepAssoc1SubsetId = ksss.KeepSepAssoc1SubsetId,
                                                                          KeepSepAssoc2 = lookupslist.SingleOrDefault(s => s.LookupIndex == ksss.KeepSepAssoc2Id)
                                                                              .LookupDescription,
                                                                          KeepSepAssoc2Subset = lookupsSublist
                                                                              .SingleOrDefault(s => s.LookupIndex == ksss.KeepSepAssoc2SubsetId).LookupDescription,
                                                                          KeepSepAssoc1 = lookupslist.SingleOrDefault(s => s.LookupIndex == ksss.KeepSepAssoc1Id)
                                                                              .LookupDescription,
                                                                          KeepSepAssoc1Subset = lookupsSublist
                                                                              .SingleOrDefault(s => s.LookupIndex == ksss.KeepSepAssoc1SubsetId).LookupDescription,
                                                                      };


                lstKeepSeps.AddRange(from ksss in keepSepSubsetSubset
                                     from lksaas in lstPerClassifySelectedInmates
                                     where ksss.KeepSepAssoc2Id == lksaas.PersonClassificationTypeId
                                           && ksss.KeepSepAssoc2SubsetId == lksaas.PersonClassificationSubsetId
                                           && ksss.PersonId != lksaas.PersonId
                                     select new TrackingConflictVm
                                     {
                                         ConflictType = KeepSepLabel.SUBSETKEEPSEPSUBSET,
                                         KeepSepPersonId = ksss.PersonId,
                                         KeepSepInmateId = ksss.InmateId,
                                         KeepSepInmateNumber = ksss.InmateNumber,
                                         ConflictDescription = ksss.KeepSepReason,
                                         KeepSepAssoc1 = ksss.KeepSepAssoc2,
                                         KeepSepAssocSubset1 = ksss.KeepSepAssoc2Subset,
                                         KeepSepAssoc2 = ksss.KeepSepAssoc1,
                                         KeepSepAssocSubset2 = ksss.KeepSepAssoc1Subset,
                                         KeepSepAssoc1Id = ksss.KeepSepAssoc2Id,
                                         KeepSepAssocSubset1Id = ksss.KeepSepAssoc2SubsetId,
                                         KeepSepAssoc2Id = ksss.KeepSepAssoc1Id,
                                         KeepSepAssocSubset2Id = ksss.KeepSepAssoc1SubsetId,
                                         PersonId = lksaas.PersonId,
                                         InmateId = lksaas.InmateId,
                                         InmateNumber = lksaas.InmateNumber
                                     });

                lstKeepSeps.AddRange(from ksss in tKeepSepSubsetSubset
                                     from lksaas in lstPerClassifySelectedInmates
                                     where ksss.KeepSepAssoc1Id == lksaas.PersonClassificationTypeId
                                           && ksss.KeepSepAssoc1SubsetId == lksaas.PersonClassificationSubsetId
                                           && ksss.PersonId != lksaas.PersonId
                                     select new TrackingConflictVm
                                     {
                                         ConflictType = KeepSepLabel.SUBSETKEEPSEPSUBSET,
                                         KeepSepPersonId = ksss.PersonId,
                                         KeepSepInmateId = ksss.InmateId,
                                         KeepSepInmateNumber = ksss.InmateNumber,
                                         ConflictDescription = ksss.KeepSepReason,
                                         KeepSepAssoc1 = ksss.KeepSepAssoc1,
                                         KeepSepAssocSubset1 = ksss.KeepSepAssoc1Subset,
                                         KeepSepAssoc2 = ksss.KeepSepAssoc2,
                                         KeepSepAssocSubset2 = ksss.KeepSepAssoc2Subset,
                                         KeepSepAssoc2Id = ksss.KeepSepAssoc2Id,
                                         KeepSepAssoc1Id = ksss.KeepSepAssoc1Id,
                                         KeepSepAssocSubset1Id = ksss.KeepSepAssoc1SubsetId,
                                         KeepSepAssocSubset2Id = ksss.KeepSepAssoc2SubsetId,
                                         PersonId = lksaas.PersonId,
                                         InmateId = lksaas.InmateId,
                                         InmateNumber = lksaas.InmateNumber
                                     });
            }

            if (lstPerClassifySelectedInmates.Count > 0)
            {
                IQueryable<TrackingConflictVm> keepSepSubsetSubsetT = from ksss in _context.KeepSepSubsetSubset
                                                                      from ksaas in lstPerClassifySelectedInmates
                                                                      where ksss.KeepSepAssoc1Id == ksaas.PersonClassificationTypeId
                                                                          && ksss.KeepSepAssoc1SubsetId == ksaas.PersonClassificationSubsetId
                                                                          && ksss.DeleteFlag == 0
                                                                      select new TrackingConflictVm
                                                                      {

                                                                          PersonId = ksaas.PersonId,
                                                                          InmateId = ksaas.InmateId,
                                                                          InmateNumber = ksaas.InmateNumber,
                                                                          KeepSepReason = ksss.KeepSepReason,
                                                                          KeepSepAssoc2Id = ksss.KeepSepAssoc2Id,
                                                                          KeepSepAssoc2SubsetId = ksss.KeepSepAssoc2SubsetId,
                                                                          KeepSepAssoc1Id = ksss.KeepSepAssoc1Id,
                                                                          KeepSepAssoc1SubsetId = ksss.KeepSepAssoc1SubsetId,
                                                                          KeepSepAssoc2 = lookupslist.SingleOrDefault(s => s.LookupIndex == ksss.KeepSepAssoc2Id)
                                                                              .LookupDescription,
                                                                          KeepSepAssoc2Subset = lookupsSublist
                                                                              .SingleOrDefault(s => s.LookupIndex == ksss.KeepSepAssoc2SubsetId).LookupDescription,
                                                                          KeepSepAssoc1 = lookupslist.SingleOrDefault(s => s.LookupIndex == ksss.KeepSepAssoc1Id)
                                                                              .LookupDescription,
                                                                          KeepSepAssoc1Subset = lookupsSublist
                                                                              .SingleOrDefault(s => s.LookupIndex == ksss.KeepSepAssoc1SubsetId).LookupDescription,
                                                                      };

                IQueryable<TrackingConflictVm> tLstKeepSepSubsetSubset = from ksss in _context.KeepSepSubsetSubset
                                                                         from ksaas in lstPerClassifySelectedInmates
                                                                         where ksss.KeepSepAssoc2Id == ksaas.PersonClassificationTypeId
                                                                             && ksss.KeepSepAssoc2SubsetId == ksaas.PersonClassificationSubsetId
                                                                             && ksss.DeleteFlag == 0
                                                                         select new TrackingConflictVm
                                                                         {
                                                                             PersonId = ksaas.PersonId,
                                                                             InmateId = ksaas.InmateId,
                                                                             InmateNumber = ksaas.InmateNumber,
                                                                             KeepSepReason = ksss.KeepSepReason,
                                                                             KeepSepAssoc2Id = ksss.KeepSepAssoc2Id,
                                                                             KeepSepAssoc2SubsetId = ksss.KeepSepAssoc2SubsetId,
                                                                             KeepSepAssoc1Id = ksss.KeepSepAssoc1Id,
                                                                             KeepSepAssoc1SubsetId = ksss.KeepSepAssoc1SubsetId,
                                                                             KeepSepAssoc2 = lookupslist.SingleOrDefault(s => s.LookupIndex == ksss.KeepSepAssoc2Id)
                                                                                 .LookupDescription,
                                                                             KeepSepAssoc2Subset = lookupsSublist
                                                                                 .SingleOrDefault(s => s.LookupIndex == ksss.KeepSepAssoc2SubsetId).LookupDescription,
                                                                             KeepSepAssoc1 = lookupslist.SingleOrDefault(s => s.LookupIndex == ksss.KeepSepAssoc1Id)
                                                                                 .LookupDescription,
                                                                             KeepSepAssoc1Subset = lookupsSublist
                                                                                 .SingleOrDefault(s => s.LookupIndex == ksss.KeepSepAssoc1SubsetId).LookupDescription,
                                                                         };


                lstKeepSeps.AddRange(from ksaa in keepSepSubsetSubsetT
                                     from lksaas in lstPerClassifySelectedInmates
                                     where lksaas.PersonClassificationTypeId == ksaa.KeepSepAssoc2Id
                                           && lksaas.PersonClassificationSubsetId == ksaa.KeepSepAssoc2SubsetId
                                           && ksaa.PersonId != lksaas.PersonId
                                     select new TrackingConflictVm
                                     {
                                         ConflictType = KeepSepLabel.SUBSETKEEPSEPSUBSET,
                                         KeepSepPersonId = ksaa.PersonId,
                                         KeepSepInmateId = ksaa.InmateId,
                                         KeepSepInmateNumber = ksaa.InmateNumber,
                                         ConflictDescription = ksaa.KeepSepReason,
                                         KeepSepAssoc1 = ksaa.KeepSepAssoc2,
                                         KeepSepAssocSubset1 = ksaa.KeepSepAssoc2Subset,
                                         KeepSepAssoc2 = ksaa.KeepSepAssoc1,
                                         KeepSepAssocSubset2 = ksaa.KeepSepAssoc1Subset,
                                         KeepSepAssoc2Id = ksaa.KeepSepAssoc1Id,
                                         KeepSepAssocSubset2Id = ksaa.KeepSepAssoc1SubsetId,
                                         KeepSepAssoc1Id = ksaa.KeepSepAssoc2Id,
                                         KeepSepAssocSubset1Id = ksaa.KeepSepAssoc2SubsetId,
                                         PersonId = lksaas.PersonId,
                                         InmateId = lksaas.InmateId,
                                         InmateNumber = lksaas.InmateNumber
                                     });

                lstKeepSeps.AddRange(from ksaa in tLstKeepSepSubsetSubset
                                     from lksaas in lstPerClassifySelectedInmates
                                     where lksaas.PersonClassificationTypeId == ksaa.KeepSepAssoc1Id
                                           && lksaas.PersonClassificationSubsetId == ksaa.KeepSepAssoc1SubsetId
                                           && ksaa.PersonId != lksaas.PersonId
                                     select new TrackingConflictVm
                                     {
                                         ConflictType = KeepSepLabel.SUBSETKEEPSEPSUBSET,
                                         KeepSepPersonId = ksaa.PersonId,
                                         KeepSepInmateId = ksaa.InmateId,
                                         KeepSepInmateNumber = ksaa.InmateNumber,
                                         ConflictDescription = ksaa.KeepSepReason,
                                         KeepSepAssoc1 = ksaa.KeepSepAssoc2,
                                         KeepSepAssocSubset1 = ksaa.KeepSepAssoc2Subset,
                                         KeepSepAssoc2 = ksaa.KeepSepAssoc1,
                                         KeepSepAssocSubset2 = ksaa.KeepSepAssoc1Subset,
                                         KeepSepAssoc2Id = ksaa.KeepSepAssoc1Id,
                                         KeepSepAssocSubset2Id = ksaa.KeepSepAssoc1SubsetId,
                                         KeepSepAssoc1Id = ksaa.KeepSepAssoc2Id,
                                         KeepSepAssocSubset1Id = ksaa.KeepSepAssoc2SubsetId,
                                         PersonId = lksaas.PersonId,
                                         InmateId = lksaas.InmateId,
                                         InmateNumber = lksaas.InmateNumber
                                     });
            }
            #endregion

            _trackingConflictDetails.AddRange(lstKeepSeps);
        }


        private void LoadPersonDetails() => _trackingConflictDetails.ForEach(item =>
        {
            if (item.PersonId > 0)
            {
                Person person = _context.Person.Single(p => p.PersonId == item.PersonId);

                item.PersonLastName = person.PersonLastName;
                item.PersonFirstName = person.PersonFirstName;
                item.PersonMiddleName = person.PersonMiddleName;
            }
            if (item.KeepSepPersonId > 0)
            {
                Person person = _context.Person.Single(p => p.PersonId == item.KeepSepPersonId);

                item.KeepSepPersonLastName = person.PersonLastName;
                item.KeepSepPersonFirstName = person.PersonFirstName;
            }
            if (item.VisitorId > 0)
            {
                Person person = _context.Person.Single(p => p.PersonId == item.VisitorId);

                item.VisitorLastName = person.PersonLastName;
                item.VisitorFirstName = person.PersonFirstName;
                item.VisitorMiddleName = person.PersonMiddleName;
            }
        });

        public KeyValuePair<int, string> GetFacilityLocation(int facilityId)
        {
            KeyValuePair<int, string> keyValuePairs = new KeyValuePair<int, string>();
            int? facilityLocationId = _context.Facility.Single(s => s.FacilityId == facilityId).DefaultBookingLocationId;

            if (facilityLocationId > 0)
            {
                keyValuePairs = _context.Privileges.Where(s => s.PrivilegeId == facilityLocationId && s.FacilityId == facilityId).
                Select(s => new KeyValuePair<int, string>
                   (s.PrivilegeId,
                       s.PrivilegeDescription)).SingleOrDefault();

            }
            return keyValuePairs;
        }

        private void LoadTotalSeperationConflict(ICollection<int> inmateList, int moveLocationId)
        {
            List<TrackingConflictVm> inmateLsts = (from inm in _context.Inmate
                                                   where inm.InmateActive == 1
                                                   select new TrackingConflictVm
                                                   {
                                                       InmateId = inm.InmateId,
                                                       PersonId = inm.PersonId,
                                                       InmateNumber = inm.InmateNumber,
                                                       FacilityId = inm.FacilityId,
                                                       InmateCurrentTrack = inm.InmateCurrentTrack,
                                                       InmateCurrentTrackId = inm.InmateCurrentTrackId
                                                   }).ToList();
            List<TrackingConflictVm> selInmateList = inmateLsts.Where(i => inmateList.Contains(i.InmateId)).ToList();
            List<TrackingConflictVm> locationInmateList = inmateLsts.Where(i => i.InmateCurrentTrackId == moveLocationId).ToList();

            //Location conflict check
            List<PersonFlag> sepFlag = _context.PersonFlag.Where(w => w.DeleteFlag == 0
                                          && (w.InmateFlagIndex > 0 || w.PersonFlagIndex > 0)).ToList();
            List<PersonFlag> locSepFlag = sepFlag.Where(w => locationInmateList.Select(a => a.PersonId).Contains(w.PersonId)).ToList();

            List<int> locSepCount = GetSeparationCount(locSepFlag);
            if (locSepCount.Count > 0)
            {
                _trackingConflictDetails.AddRange((from in1 in selInmateList
                                                   from in2 in locationInmateList
                                                   select new
                                                   {
                                                       in1.PersonId,
                                                       in1.InmateNumber,
                                                       in1.InmateId,
                                                       Location = in2.InmateCurrentTrack
                                                   }).Distinct().Select(inm => new TrackingConflictVm
                                                   {
                                                       ConflictType = ConflictTypeConstants.TOTALSEPLOCCONFLICT,
                                                       PersonId = inm.PersonId,
                                                       InmateNumber = inm.InmateNumber,
                                                       InmateId = inm.InmateId,
                                                       ConflictDescription = ConflictTypeConstants.INMATEALREADYINLOCATION
                                                          + inm.Location + ConflictTypeConstants.HAVETOTALSEPERATIONFLAG
                                                   }));
            }

            List<PersonFlag> locInmSepFlag = sepFlag.Where(w => selInmateList.Select(a => a.PersonId).Contains(w.PersonId)).ToList();


            List<int> locInmSepCount = GetSeparationCount(locInmSepFlag);
            if (locInmSepCount.Count == 0) return;
            IEnumerable<TrackingConflictVm> conflictInmateList = selInmateList.Where(w => locInmSepCount.Contains(w.PersonId));
            List<TrackingConflictVm> conflictBatchInmateList = selInmateList.Where(w => !locInmSepCount.Contains(w.PersonId)).ToList();
            if (locationInmateList.Count > 0)
            {
                _trackingConflictDetails.AddRange((from in1 in conflictInmateList
                                                   from in2 in locationInmateList
                                                   select new
                                                   {
                                                       in1.PersonId,
                                                       in1.InmateNumber,
                                                       in1.InmateId,
                                                       Location = in2.InmateCurrentTrack
                                                   }).Distinct().Select(inm => new TrackingConflictVm
                                                   {
                                                       ConflictType = ConflictTypeConstants.TOTALSEPINMLOCCONFLICT,
                                                       PersonId = inm.PersonId,
                                                       InmateNumber = inm.InmateNumber,
                                                       InmateId = inm.InmateId,
                                                       ConflictDescription = ConflictTypeConstants.INMATEISFLAGGEDFORTOTALSEP + inm.Location
                                                   }));
            }
            else
            {
                if (conflictBatchInmateList.Count > 0)
                {
                    _trackingConflictDetails.AddRange(from inm in conflictInmateList
                                                      select new TrackingConflictVm
                                                      {
                                                          ConflictType = ConflictTypeConstants.TOTALSEPINMLOCCONFLICT,
                                                          PersonId = inm.PersonId,
                                                          InmateNumber = inm.InmateNumber,
                                                          InmateId = inm.InmateId,
                                                          ConflictDescription = ConflictTypeConstants.INMATETOTALSEP
                                                      });
                }
            }
            if (conflictBatchInmateList.Count > 0)
            {
                _trackingConflictDetails.AddRange(from inm in conflictBatchInmateList
                                                  select new TrackingConflictVm
                                                  {
                                                      ConflictType = ConflictTypeConstants.TOTALSEPINMLOCCONFLICT,
                                                      PersonId = inm.PersonId,
                                                      InmateNumber = inm.InmateNumber,
                                                      InmateId = inm.InmateId,
                                                      ConflictDescription = ConflictTypeConstants.INMATETOTALSEPLIST
                                                  });
            }
        }

        private List<int> GetSeparationCount(List<PersonFlag> sepFlag)
        {
            List<int> count = sepFlag.SelectMany(
                p => _context.Lookup.Where(w => w.LookupInactive == 0 && w.LookupFlag6 == 1 &&
                    w.LookupType == LookupConstants.PERSONCAUTION &&
                    w.LookupIndex == p.PersonFlagIndex && p.PersonFlagIndex > 0
                || w.LookupType == LookupConstants.TRANSCAUTION
                && w.LookupIndex == p.InmateFlagIndex
                && p.InmateFlagIndex > 0
                    && w.LookupFlag6 == 1
                ), (p, l) => p.PersonId).ToList();
            return count;
        }

        private void LoadHousingSupplyConflict(ICollection<int> inmateList)
        {
            _trackingConflictDetails.AddRange(_context.HousingSupplyItem.Where(w =>
                    w.HousingSupplyItemLookup.ConflictCheckFlag == 1 &&
                    inmateList.Contains(w.CurrentCheckoutInmateId ?? 0))
                .Select(s => new TrackingConflictVm
                {
                    InmateId = s.CurrentCheckoutInmateId ?? 0,
                    ConflictType = ConflictTypeConstants.HOUSINGSUPPLYCHECKOUT,
                    PersonId = s.CurrentCheckoutInmate.PersonId,
                    ConflictDescription = s.HousingSupplyItemLookup.ItemName,
                    InmateNumber = s.CurrentCheckoutInmate.InmateNumber
                }).ToList());
        }

        private void LoadHousingLockdownConflict(TrackingVm obj)
        {
            //Check location lockdown flag
            if (_context.Privileges.Find(obj.MoveLocationId).ConflictCheckRoomPrivilege != 1 &&
                (obj.MoveDestinationId <= 0 ||
                 _context.Privileges.Find(obj.MoveDestinationId).ConflictCheckRoomPrivilege != 1)) return;

            //Housing lockdown list
            List<HousingLockdown> lockDownDetails = _context.HousingLockdown.Where(w =>
                w.StartLockdown <= DateTime.Now && w.EndLockdown >= DateTime.Now && !w.DeleteFlag).ToList();
            //Housing unit list
            IQueryable<HousingUnit> housingUnits = _context.HousingUnit;
            //Housing group assign list
            List<HousingGroupAssign> housingAssign = _context.HousingGroupAssign.Where(w => w.HousingUnitListId.HasValue).ToList();
            //Inmate list
            List<Inmate> inmates = _context.Inmate.Where(w => w.InmateActive == 1).ToList();
            obj.MoveInmateList.ForEach(f =>
            {
                Inmate inmate = inmates.SingleOrDefault(si => si.InmateId == f);
                if(inmate == null) return;
                if (inmate.HousingUnitId == 0 || inmate.HousingUnitId == null) return;
                HousingDetail housingInfo = housingUnits.Where(w => w.HousingUnitId == inmate.HousingUnitId)
                    .Select(s => new HousingDetail
                    {
                        HousingUnitId = s.HousingUnitId,
                        FacilityId = s.FacilityId,
                        FacilityAbbr = s.Facility.FacilityAbbr,
                        HousingUnitListId = s.HousingUnitListId,
                        HousingUnitLocation = s.HousingUnitLocation,
                        HousingUnitNumber = s.HousingUnitNumber,
                        HousingUnitBedLocation = s.HousingUnitBedLocation,
                        HousingUnitBedNumber = s.HousingUnitBedNumber,
                        HousingUnitBedGroupId = s.HousingUnitListBedGroupId ?? 0
                    }).Single();
                lockDownDetails.ForEach(lockDown =>
                {
                    TrackingConflictVm trackingConflict = new TrackingConflictVm()
                    {
                        ConflictType = ConflictTypeConstants.HOUSINGLOCKDOWNCONFLICT,
                        PersonId = inmate.PersonId,
                        InmateNumber = inmate.InmateNumber,
                        InmateId = inmate.InmateId,
                        HousingInfo = housingInfo,
                        StartLockdown = lockDown.StartLockdown,
                        EndLockdown = lockDown.EndLockdown
                    };
                    switch (lockDown.LockdownSource)
                    {
                        case "Facility":
                            if (lockDown.SourceId == housingInfo.FacilityId)
                            {
                                //Facility Lockdown
                                trackingConflict.LockDown = LockDownType.Facility;
                                _trackingConflictDetails.Add(trackingConflict);
                            }
                            break;
                        case "Building":
                            KeyValuePair<string, int> buildingInfo = housingUnits.Where(w =>
                                 w.HousingUnitListId == lockDown.SourceId).Select(se =>
                                 new KeyValuePair<string, int>(se.HousingUnitLocation, se.FacilityId)).First();
                            if (buildingInfo.Key == housingInfo.HousingUnitLocation && buildingInfo.Value == housingInfo.FacilityId)
                            {
                                //Building Lockdown
                                trackingConflict.LockDown = LockDownType.Building;
                                _trackingConflictDetails.Add(trackingConflict);
                            }
                            break;
                        case "Pod":
                            if (lockDown.SourceId == housingInfo.HousingUnitListId)
                            {
                                //Pod Lockdown
                                trackingConflict.LockDown = LockDownType.Pod;
                                _trackingConflictDetails.Add(trackingConflict);
                            }
                            break;
                        case "Cell":
                            KeyValuePair<string, int> cellInfo = housingUnits.Where(si =>
                                 si.HousingUnitId == lockDown.SourceId).Select(se =>
                                 new KeyValuePair<string, int>(se.HousingUnitBedNumber, se.HousingUnitListId)).Single();
                            if (cellInfo.Key == housingInfo.HousingUnitBedNumber && cellInfo.Value == housingInfo.HousingUnitListId)
                            {
                                //Cell Lockdown
                                trackingConflict.LockDown = LockDownType.Cell;
                                _trackingConflictDetails.Add(trackingConflict);
                            }
                            break;
                        case "CellGroup":
                            if (lockDown.SourceId == housingInfo.HousingUnitBedGroupId)
                            {
                                //CellGroup Lockdown
                                trackingConflict.LockDown = LockDownType.CellGroup;
                                _trackingConflictDetails.Add(trackingConflict);
                            }
                            break;
                        case "HousingGroup":
                            List<int?> housingUnitListIds = housingAssign.Where(w =>
                                    w.HousingGroupId == lockDown.SourceId && w.HousingUnitListId.HasValue)
                                .Select(sa => sa.HousingUnitListId).ToList();
                            if (housingUnitListIds.Contains(housingInfo.HousingUnitListId))
                            {
                                //Group Lockdown
                                trackingConflict.LockDown = LockDownType.CellGroup;
                                _trackingConflictDetails.Add(trackingConflict);
                            }

                            break;
                    }
                });
            });
        }

    }
}
