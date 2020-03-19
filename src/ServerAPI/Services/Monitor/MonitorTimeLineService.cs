﻿using GenerateTables.Models;
using ServerAPI.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using ServerAPI.Utilities;

namespace ServerAPI.Services
{
    public class MonitorTimeLineService : IMonitorTimeLineService
    {
        private readonly AAtims _context;
        private readonly ICommonService _commonService;
        private readonly IAppointmentViewerService _appointmentViewerService;

        public MonitorTimeLineService(AAtims context, IAppointmentViewerService appointmentViewerService, ICommonService commonService)
        {
            _context = context;
            _commonService = commonService;
            _appointmentViewerService = appointmentViewerService;
        }

        #region Monitor => TimeLine
        //Load monitor timeline grid details
        public List<MonitorTimeLineDetailsVm> GetMonitorTimeLine(MonitorTimeLineSearchVm searchValues)
        {
            List<LookupVm> lstLookup = _commonService.GetLookups(new[]
            {
                LookupConstants.ARRTYPE, LookupConstants.GRIEVTYPE, LookupConstants.DISCTYPE,
                LookupConstants.RELATIONS, LookupConstants.APPTREAS
            });
            List<MonitorTimeLineDetailsVm> monitorTimeLineDetail = new List<MonitorTimeLineDetailsVm>();
            List<PersonnelVm> personnelVm = new List<PersonnelVm>();

            if (searchValues.IntakeFlag || searchValues.AppointmentsFlag || searchValues.VisitationFlag)
            {
                personnelVm = _context.Personnel.Select(a => new PersonnelVm
                {
                    PersonId = a.PersonId,
                    PersonFirstName = a.PersonNavigation.PersonFirstName,
                    PersonLastName = a.PersonNavigation.PersonLastName,
                    PersonMiddleName = a.PersonNavigation.PersonMiddleName,
                    OfficerBadgeNumber = a.OfficerBadgeNum,
                    PersonnelId = a.PersonnelId
                }).ToList();
            }

            if (searchValues.PreBookFlag) { PreBookFlagList(searchValues, monitorTimeLineDetail); }

            if (searchValues.IntakeFlag) { IntakeFlagList(searchValues, monitorTimeLineDetail, personnelVm); }

            if (searchValues.BookingFlag) { BookingFlagList(searchValues, monitorTimeLineDetail, lstLookup); }

            if (searchValues.IncarcerationFlag) { IncarcerationFlagList(searchValues, monitorTimeLineDetail); }

            if (searchValues.ReleaseFlag) { ReleaseFlagList(searchValues, monitorTimeLineDetail); }

            if (searchValues.ClassificationFlag) { ClassificationFlagList(searchValues, monitorTimeLineDetail); }

            if (searchValues.HousingFlag) { HousingFlagList(searchValues, monitorTimeLineDetail); }

            if (searchValues.GrievanceFlag) { GrievanceFlagList(searchValues, monitorTimeLineDetail, lstLookup); }

            if (searchValues.IncidentFlag) { IncidentFlagList(searchValues, monitorTimeLineDetail, lstLookup); }

            if (searchValues.NotesFlag) { NotesFlagList(searchValues, monitorTimeLineDetail); }

            if (searchValues.AppointmentsFlag) { AppointmentsFlagList(searchValues, monitorTimeLineDetail, personnelVm); }

            if (searchValues.CellLogFlag) { CellLogFlagList(searchValues, monitorTimeLineDetail); }

            if (searchValues.VisitationFlag) { VisitationFlagList(searchValues, monitorTimeLineDetail, lstLookup, personnelVm); }

            if (searchValues.TrackingFacilityFlag) { TrackingFacilityFlagList(searchValues, monitorTimeLineDetail); }

            if (searchValues.TrackingNoFacilityFlag) { TrackingNoFacilityFlagList(searchValues, monitorTimeLineDetail); }

            if (!searchValues.MedDistributeFlag) return monitorTimeLineDetail;

            MedDistributeFlagList(searchValues, monitorTimeLineDetail);

            return monitorTimeLineDetail;
        }

        private void PreBookFlagList(MonitorTimeLineSearchVm searchValues, List<MonitorTimeLineDetailsVm> monitorTimeLineDetail)
        {
            int inmatePersonId = searchValues.InmateId > 0 ?
    _context.Inmate.Find(searchValues.InmateId).PersonId : 0;

            IQueryable<MonitorTimeLineDetailsVm> prebookList = _context.InmatePrebook.Where(w =>
            (!w.DeleteFlag.HasValue || w.DeleteFlag == 0)
             && (w.PrebookDate.HasValue && w.PrebookDate.Value.Date == searchValues.DateOfRecord.Date)
             && (searchValues.FacilityId == 0 || w.FacilityId == searchValues.FacilityId)
             && (searchValues.PersonnelId == 0 || w.PersonnelId == searchValues.PersonnelId)
             && (inmatePersonId == 0 || w.PersonId == inmatePersonId))
             .Select(s => new MonitorTimeLineDetailsVm
             {
                 DateOfRecord = s.PrebookDate,
                 Category = MonitorTimeLineConstants.PREBOOK,
                 DetailOne = s.PrebookType,
                 InmateInfo = s.PersonId > 0 ? new PersonVm
                 {
                     PersonLastName = s.Person.PersonLastName,
                     PersonMiddleName = s.Person.PersonMiddleName,
                     PersonFirstName = s.Person.PersonFirstName,
                     PersonId = s.PersonId ?? 0
                 } : new PersonVm
                 {
                     PersonLastName = s.PersonLastName,
                     PersonMiddleName = s.PersonMiddleName,
                     PersonFirstName = s.PersonFirstName,
                     PersonId = s.PersonId ?? 0
                 },
                 Personnel = new PersonnelVm
                 {
                     PersonFirstName = s.Personnel.PersonNavigation.PersonFirstName,
                     PersonLastName = s.Personnel.PersonNavigation.PersonLastName,
                     OfficerBadgeNumber = s.Personnel.OfficerBadgeNum,
                 }
             });
            monitorTimeLineDetail.AddRange(prebookList);
        }

        private void IntakeFlagList(MonitorTimeLineSearchVm searchValues,
            List<MonitorTimeLineDetailsVm> monitorTimeLineDetail,
            List<PersonnelVm> personnelVm)
        {
            List<MonitorTimeLineDetailsVm> incarceration = _context.IncarcerationArrestXref.Where(inc =>
               (inc.Incarceration.DateIn.HasValue && inc.Incarceration.DateIn.Value.Date == searchValues.DateOfRecord.Date)
               && (searchValues.FacilityId == 0 || inc.Incarceration.Inmate.FacilityId == searchValues.FacilityId))
               .Select(m => new MonitorTimeLineDetailsVm
               {
                   InmateId = m.Incarceration.InmateId,
                   Category = MonitorTimeLineConstants.INTAKE,
                   DateOfRecord = m.Incarceration.DateIn,
                   InmateInfo = new PersonVm
                   {
                       PersonLastName = m.Incarceration.Inmate.Person.PersonLastName,
                       PersonMiddleName = m.Incarceration.Inmate.Person.PersonMiddleName,
                       PersonFirstName = m.Incarceration.Inmate.Person.PersonFirstName,
                       FacilityId = m.Incarceration.Inmate.FacilityId
                   },
                   ArrestOfficerId = m.Arrest.ArrestOfficerId,
                   DetailInDate = m.Arrest.ArrestDate
               }).ToList();

            incarceration.ForEach(item =>
                item.Personnel = personnelVm.SingleOrDefault(b => b.PersonnelId == item.ArrestOfficerId));

            incarceration = incarceration.Where(s => (searchValues.PersonnelId == 0 ||
            s.ArrestOfficerId == searchValues.PersonnelId) &&
            (searchValues.InmateId == 0 || s.InmateId == searchValues.InmateId)).ToList();
            monitorTimeLineDetail.AddRange(incarceration);
        }

        private void BookingFlagList(MonitorTimeLineSearchVm searchValues,
            List<MonitorTimeLineDetailsVm> monitorTimeLineDetail, List<LookupVm> lstLookup)
        {
            IQueryable<MonitorTimeLineDetailsVm> arrest = _context.Arrest.Where(aa =>
               (aa.ArrestBookingDate.HasValue &&
               aa.ArrestBookingDate.Value.Date == searchValues.DateOfRecord.Date) &&
                (searchValues.FacilityId == 0 || aa.Inmate.FacilityId == searchValues.FacilityId) &&
                (searchValues.PersonnelId == 0 || aa.ArrestBookingOfficerId == searchValues.PersonnelId) &&
                (searchValues.InmateId == 0 || aa.InmateId == searchValues.InmateId)
            ).Select(aa => new MonitorTimeLineDetailsVm
            {
                DateOfRecord = aa.ArrestBookingDate,
                Category = MonitorTimeLineConstants.BOOKING,
                DetailOne = aa.ArrestBookingNo,
                DetailTwo = lstLookup.SingleOrDefault(ll => Convert.ToString(ll.LookupIndex) == aa.ArrestType
                    && ll.LookupType == LookupConstants.ARRTYPE).LookupDescription,
                InmateInfo = new PersonVm
                {
                    PersonFirstName = aa.Inmate.Person.PersonFirstName,
                    PersonLastName = aa.Inmate.Person.PersonLastName,
                    PersonMiddleName = aa.Inmate.Person.PersonMiddleName
                },
                Personnel = new PersonnelVm
                {
                    PersonFirstName = aa.ArrestBookingOfficer.PersonNavigation.PersonFirstName,
                    PersonLastName = aa.ArrestBookingOfficer.PersonNavigation.PersonLastName,
                    OfficerBadgeNumber = aa.ArrestBookingOfficer.OfficerBadgeNumber
                }
            });
            monitorTimeLineDetail.AddRange(arrest);
        }

        private void IncarcerationFlagList(MonitorTimeLineSearchVm searchValues,
            List<MonitorTimeLineDetailsVm> monitorTimeLineDetail)
        {

            IQueryable<MonitorTimeLineDetailsVm> incarcerationInList = _context.Incarceration.Where
                (inc => (inc.DateIn.HasValue && inc.DateIn.Value.Date == searchValues.DateOfRecord.Date)
                && (searchValues.FacilityId == 0 || inc.Inmate.FacilityId == searchValues.FacilityId) && (searchValues.PersonnelId == 0 ||
                inc.InOfficerId == searchValues.PersonnelId) && (searchValues.InmateId == 0
                || inc.InmateId == searchValues.InmateId)
                ).Select(s => new MonitorTimeLineDetailsVm
                {
                    Category = MonitorTimeLineConstants.INCARCERATION,
                    DateOfRecord = s.DateIn,
                    DetailInDate = s.ReleaseOut,
                    InmateInfo = new PersonVm
                    {
                        PersonLastName = s.Inmate.Person.PersonLastName,
                        PersonMiddleName = s.Inmate.Person.PersonMiddleName,
                        PersonFirstName = s.Inmate.Person.PersonFirstName,
                    },
                    Personnel = new PersonnelVm
                    {
                        PersonFirstName = s.InOfficer.PersonNavigation.PersonFirstName,
                        PersonLastName = s.InOfficer.PersonNavigation.PersonLastName,
                        OfficerBadgeNumber = s.InOfficer.OfficerBadgeNum
                    }

                });

            monitorTimeLineDetail.AddRange(incarcerationInList);

            IQueryable<MonitorTimeLineDetailsVm> releaseOutList = _context.Incarceration.Where
                (inc => (inc.ReleaseOut.HasValue && inc.ReleaseOut.Value.Date == searchValues.DateOfRecord.Date)
                 && (searchValues.FacilityId == 0 || inc.Inmate.FacilityId == searchValues.FacilityId)
                 && (searchValues.PersonnelId == 0 || inc.OutOfficerId == searchValues.PersonnelId)
                 && (searchValues.InmateId == 0
                || inc.InmateId == searchValues.InmateId)).Select(s => new MonitorTimeLineDetailsVm
                {
                    Category = MonitorTimeLineConstants.INCARCERATION,
                    DateOfRecord = s.ReleaseOut,
                    DetailInDate = s.ReleaseOut,
                    DetailOne = s.ReleaseToOtherAgencyName,
                    InmateInfo = new PersonVm
                    {
                        PersonLastName = s.Inmate.Person.PersonLastName,
                        PersonMiddleName = s.Inmate.Person.PersonMiddleName,
                        PersonFirstName = s.Inmate.Person.PersonFirstName,
                    },
                    Personnel = new PersonnelVm
                    {
                        PersonFirstName = s.OutOfficer.PersonNavigation.PersonFirstName,
                        PersonLastName = s.OutOfficer.PersonNavigation.PersonLastName,
                        OfficerBadgeNumber = s.OutOfficer.OfficerBadgeNum
                    }
                });
            monitorTimeLineDetail.AddRange(releaseOutList);
        }
        private void ReleaseFlagList(MonitorTimeLineSearchVm searchValues,
              List<MonitorTimeLineDetailsVm> monitorTimeLineDetail)
        {

            List<MonitorTimeLineDetailsVm> incarcerationArrestXrefList = _context.
                IncarcerationArrestXref.Where(xr => xr.ReleaseDate.HasValue && xr.ReleaseDate.Value.Date ==
                searchValues.DateOfRecord.Date).Select(s => new MonitorTimeLineDetailsVm
                {
                    Category = MonitorTimeLineConstants.RELEASE,
                    DateOfRecord = s.ReleaseDate,
                    DetailOne = s.ReleaseReason,
                    DetailTwo = s.ReleaseNotes,
                    IncarcerationId = s.IncarcerationId
                }).ToList();

            IQueryable<MonitorTimeLineDetailsVm> releaseList = _context.Incarceration.Where(inc =>
            (searchValues.FacilityId == 0 || inc.Inmate.FacilityId == searchValues.FacilityId) &&
            (searchValues.PersonnelId == 0 || inc.OutOfficerId == searchValues.PersonnelId)
            && (searchValues.InmateId == 0 || inc.InmateId == searchValues.InmateId))
            .Select(s => new MonitorTimeLineDetailsVm
            {
                IncarcerationId = s.IncarcerationId,
                InmateInfo = new PersonVm
                {
                    PersonLastName = s.Inmate.Person.PersonLastName,
                    PersonMiddleName = s.Inmate.Person.PersonMiddleName,
                    PersonFirstName = s.Inmate.Person.PersonFirstName,
                },
                Personnel = new PersonnelVm
                {
                    PersonFirstName = s.OutOfficer.PersonNavigation.PersonFirstName,
                    PersonLastName = s.OutOfficer.PersonNavigation.PersonLastName,
                    OfficerBadgeNumber = s.OutOfficer.OfficerBadgeNum
                }
            });

            incarcerationArrestXrefList.ForEach(item =>
                    {
                        item.InmateInfo = releaseList.Single
                        (ss => ss.IncarcerationId == item.IncarcerationId).InmateInfo;
                        item.Personnel = releaseList.Single
                        (ss => ss.IncarcerationId == item.IncarcerationId).Personnel;
                    });
            monitorTimeLineDetail.AddRange(incarcerationArrestXrefList);
        }

        private void ClassificationFlagList(MonitorTimeLineSearchVm searchValues,
      List<MonitorTimeLineDetailsVm> monitorTimeLineDetail)
        {
            IQueryable<MonitorTimeLineDetailsVm> classifyList = _context.InmateClassification.Where
            (cl => (cl.CreateDate.HasValue && cl.CreateDate.Value.Date == searchValues.DateOfRecord.Date) &&
                (searchValues.FacilityId == 0 || cl.InmateNavigation.FacilityId == searchValues.FacilityId) &&
                (searchValues.PersonnelId == 0 || cl.ClassificationOfficerId == searchValues.PersonnelId)
                && (searchValues.InmateId == 0 || cl.InmateId == searchValues.InmateId)).Select(s =>
                new MonitorTimeLineDetailsVm
                {
                    Category = MonitorTimeLineConstants.CLASSIFICATION,
                    DateOfRecord = s.CreateDate,
                    DetailOne = s.InmateClassificationType,
                    DetailTwo = s.InmateClassificationReason,
                    InmateInfo = new PersonVm
                    {
                        PersonLastName = s.InmateNavigation.Person.PersonLastName,
                        PersonMiddleName = s.InmateNavigation.Person.PersonMiddleName,
                        PersonFirstName = s.InmateNavigation.Person.PersonFirstName
                    },
                    Personnel = new PersonnelVm
                    {
                        PersonFirstName = s.ClassificationOfficer.PersonNavigation.PersonFirstName,
                        PersonLastName = s.ClassificationOfficer.PersonNavigation.PersonLastName,
                        OfficerBadgeNumber = s.ClassificationOfficer.OfficerBadgeNum
                    }
                });
            monitorTimeLineDetail.AddRange(classifyList);
        }

        private void HousingFlagList(MonitorTimeLineSearchVm searchValues,
      List<MonitorTimeLineDetailsVm> monitorTimeLineDetail)
        {
            IQueryable<MonitorTimeLineDetailsVm> housingList = _context.HousingUnitMoveHistory.Where
                (hs => (hs.MoveDate.HasValue && hs.MoveDate.Value.Date == searchValues.DateOfRecord.Date)
                && (searchValues.FacilityId == 0 || hs.Inmate.FacilityId == searchValues.FacilityId) && (searchValues.PersonnelId == 0
                || hs.MoveOfficerId == searchValues.PersonnelId) &&
                (searchValues.InmateId == 0 || hs.InmateId == searchValues.InmateId)).Select(s => new MonitorTimeLineDetailsVm
                {
                    Category = MonitorTimeLineConstants.HOUSING,
                    DateOfRecord = s.MoveDate,
                    DetailOne = s.MoveReason,
                    InmateInfo = new PersonVm
                    {
                        PersonLastName = s.Inmate.Person.PersonLastName,
                        PersonMiddleName = s.Inmate.Person.PersonMiddleName,
                        PersonFirstName = s.Inmate.Person.PersonFirstName
                    },
                    Personnel = new PersonnelVm
                    {
                        PersonFirstName = s.MoveOfficer.PersonNavigation.PersonFirstName,
                        PersonLastName = s.MoveOfficer.PersonNavigation.PersonLastName,
                        OfficerBadgeNumber = s.MoveOfficer.OfficerBadgeNum
                    },
                    HousingUnitLoation = s.HousingUnitToId > 0 ? new HousingDetail
                    {
                        HousingUnitId = s.HousingUnitTo.HousingUnitId,
                        HousingUnitLocation = s.HousingUnitTo.HousingUnitLocation,
                        HousingUnitNumber = s.HousingUnitTo.HousingUnitNumber,
                        HousingUnitBedNumber = s.HousingUnitTo.HousingUnitBedNumber,
                        HousingUnitBedLocation = s.HousingUnitTo.HousingUnitBedLocation,
                    } : new HousingDetail(),

                });

            monitorTimeLineDetail.AddRange(housingList);
        }

        private void GrievanceFlagList(MonitorTimeLineSearchVm searchValues,
     List<MonitorTimeLineDetailsVm> monitorTimeLineDetail, List<LookupVm> lstLookup)
        {
            IQueryable<MonitorTimeLineDetailsVm> grievanceList = _context.Grievance.Where(gr =>
            gr.DeleteFlag == 0 &&
            (gr.DateOccured.HasValue && gr.DateOccured.Value.Date == searchValues.DateOfRecord.Date)
            && (searchValues.FacilityId == 0 || gr.Inmate.FacilityId == searchValues.FacilityId)
            && (searchValues.PersonnelId == 0 || gr.CreatedBy == searchValues.PersonnelId) &&
            (searchValues.InmateId == 0 || gr.InmateId == searchValues.InmateId)).Select(s => new MonitorTimeLineDetailsVm
            {
                Category = MonitorTimeLineConstants.GRIEVANCE,
                DateOfRecord = s.DateOccured,
                DetailOne = lstLookup.SingleOrDefault(ll => ll.LookupIndex == s.GrievanceType
                && ll.LookupType == LookupConstants.GRIEVTYPE).LookupDescription,
                DetailTwo = s.Department,
                DetailInDate = s.ReviewedDate,
                InmateInfo = new PersonVm
                {
                    PersonLastName = s.Inmate.Person.PersonLastName,
                    PersonMiddleName = s.Inmate.Person.PersonMiddleName,
                    PersonFirstName = s.Inmate.Person.PersonFirstName
                },
                Personnel = new PersonnelVm
                {
                    PersonFirstName = s.CreatedByNavigation.PersonNavigation.PersonFirstName,
                    PersonLastName = s.CreatedByNavigation.PersonNavigation.PersonLastName,
                    OfficerBadgeNumber = s.CreatedByNavigation.OfficerBadgeNum
                },
            });
            monitorTimeLineDetail.AddRange(grievanceList);
        }

        private void IncidentFlagList(MonitorTimeLineSearchVm searchValues,
    List<MonitorTimeLineDetailsVm> monitorTimeLineDetail, List<LookupVm> lstLookup)
        {
            List<MonitorTimeLineDetailsVm> incidentList = _context.DisciplinaryIncident.
                Where(dc => (dc.DisciplinaryActive.HasValue && dc.DisciplinaryActive == 1)
                && (dc.DisciplinaryIncidentDate.HasValue &&
                dc.DisciplinaryIncidentDate.Value.Date == searchValues.DateOfRecord.Date)
                && (searchValues.FacilityId == 0 ||
                dc.DisciplinaryInmate.Any(ii => ii.Inmate.FacilityId == searchValues.FacilityId)) &&
                (searchValues.PersonnelId == 0 || dc.DisciplinaryOfficerId == searchValues.PersonnelId) &&
                (searchValues.InmateId == 0 || dc.DisciplinaryInmate.Any(ii =>
                ii.InmateId == searchValues.InmateId))).Select(
                s => new MonitorTimeLineDetailsVm
                {
                    Category = MonitorTimeLineConstants.INCIDENT,
                    DisciplinaryIncidentId = s.DisciplinaryIncidentId,
                    DetailOne = lstLookup.SingleOrDefault(ll => ll.LookupType ==
                    LookupConstants.DISCTYPE && ll.LookupIndex == s.DisciplinaryType).LookupDescription,
                    Personnel = new PersonnelVm
                    {
                        PersonFirstName = s.DisciplinaryOfficer.PersonNavigation.PersonFirstName,
                        PersonLastName = s.DisciplinaryOfficer.PersonNavigation.PersonLastName,
                        OfficerBadgeNumber = s.DisciplinaryOfficer.OfficerBadgeNum
                    }
                }).ToList();

            List<int> dispIncIds = incidentList.Select(a => a.DisciplinaryIncidentId).ToList();

            IQueryable<MonitorTimeLineDetailsVm> disciplinaryInmate = _context.DisciplinaryInmate.
                Where(d => d.InmateId > 0 && (searchValues.FacilityId == 0 ||
                d.Inmate.FacilityId == searchValues.FacilityId)
                    && (searchValues.InmateId == 0 || d.InmateId == searchValues.InmateId)
                    && dispIncIds.Any(a => a == d.DisciplinaryIncidentId))
              .Select(s => new MonitorTimeLineDetailsVm
              {
                  DisciplinaryIncidentId = s.DisciplinaryIncidentId,
                  InmateInfo = new PersonVm
                  {
                      PersonLastName = s.Inmate.Person.PersonLastName,
                      PersonMiddleName = s.Inmate.Person.PersonMiddleName,
                      PersonFirstName = s.Inmate.Person.PersonFirstName,
                  }
              });
            incidentList.ForEach(item =>
                    {
                        item.InmateInfo = disciplinaryInmate.Single
                        (ii => ii.DisciplinaryIncidentId == item.DisciplinaryIncidentId).InmateInfo;
                    });
            monitorTimeLineDetail.AddRange(incidentList);
        }

        private void NotesFlagList(MonitorTimeLineSearchVm searchValues,
     List<MonitorTimeLineDetailsVm> monitorTimeLineDetail)
        {
            List<MonitorTimeLineDetailsVm> notesDetails = _context.FloorNotes.Where
                (fl => (fl.FloorNoteDate.HasValue && fl.FloorNoteDate.Value.Date ==
                searchValues.DateOfRecord.Date) &&
                (searchValues.PersonnelId == 0 || fl.FloorNoteOfficerId == searchValues.PersonnelId))
                .Select(s => new MonitorTimeLineDetailsVm
                {
                    Category = MonitorTimeLineConstants.NOTES,
                    DateOfRecord = s.FloorNoteDate,
                    FloorNotesId = s.FloorNoteId,
                    DetailOne = s.FloorNoteLocation,
                    DetailTwo = s.FloorNoteNarrative,
                    Personnel = new PersonnelVm
                    {
                        PersonFirstName = s.FloorNoteOfficer.PersonNavigation.PersonFirstName,
                        PersonLastName = s.FloorNoteOfficer.PersonNavigation.PersonLastName,
                        OfficerBadgeNumber = s.FloorNoteOfficer.OfficerBadgeNum
                    },
                }).ToList();

            List<int> notesIds = notesDetails.Select(a => a.FloorNotesId).ToList();

            IQueryable<MonitorTimeLineDetailsVm> floorNoteXref = _context.FloorNoteXref.
                Where(fl => notesIds.Any(n => n == fl.FloorNoteId) &&
                (searchValues.InmateId == 0 || fl.InmateId == searchValues.InmateId)).
                Select(s => new MonitorTimeLineDetailsVm
                {
                    FloorNotesId = s.FloorNoteId,
                    InmateInfo = new PersonVm
                    {
                        PersonLastName = s.Inmate.Person.PersonLastName,
                        PersonMiddleName = s.Inmate.Person.PersonMiddleName,
                        PersonFirstName = s.Inmate.Person.PersonFirstName,
                    }
                });

            notesDetails.ForEach(item => item.InmateInfo = floorNoteXref.
                SingleOrDefault(s => s.FloorNotesId == item.FloorNotesId)?.InmateInfo);
            monitorTimeLineDetail.AddRange(notesDetails);
        }

        private void AppointmentsFlagList(MonitorTimeLineSearchVm searchValues,
     List<MonitorTimeLineDetailsVm> monitorTimeLineDetail, List<PersonnelVm> personnelVm)
        {
            CalendarInputs calendarInputs = new CalendarInputs
            {
                CalendarTypes = "1,2,4,5,6,7,8",
                FacilityId = searchValues.FacilityId,
                InmateId = searchValues.InmateId > 0 ? searchValues.InmateId : 0,
                FromDate = searchValues.DateOfRecord,
                ToDate = searchValues.DateOfRecord.Date.AddHours(23).AddMinutes(59).AddSeconds(59),
                IsFromViewer = true
            };
            List<AoAppointmentVm> getAppointmentList = _appointmentViewerService.
                AppointmentViewer(calendarInputs);

            List<MonitorTimeLineDetailsVm> appointmentList = getAppointmentList.Where(w =>
                    searchValues.PersonnelId == 0 || w.CreateBy == searchValues.PersonnelId)
                .Select(aa => new MonitorTimeLineDetailsVm
                {
                    Category = MonitorTimeLineConstants.APPOINTMENT,
                    DateOfRecord = aa.StartDate,
                    DetailOne = aa.Location,
                    DetailTwo = aa.Reason,
                    CreateBy = aa.CreateBy,
                    InmateInfo = aa.InmateId > 0 ? new PersonVm
                    {
                        PersonLastName = aa.InmateDetails.PersonLastName,
                        PersonFirstName = aa.InmateDetails.PersonFirstName,
                        PersonMiddleName = aa.InmateDetails.PersonMiddleName
                    } : new PersonVm(),
                }).ToList();
            appointmentList.ForEach(appt => appt.Personnel = personnelVm.SingleOrDefault
                          (c => c.PersonnelId == appt.CreateBy));

            monitorTimeLineDetail.AddRange(appointmentList);
        }

        private void CellLogFlagList(MonitorTimeLineSearchVm searchValues,
     List<MonitorTimeLineDetailsVm> monitorTimeLineDetail)
        {
            IQueryable<MonitorTimeLineDetailsVm> cellLogList = _context.CellLog.Where(cl =>
            (cl.CellLogDate.HasValue && cl.CellLogDate.Value.Date == searchValues.DateOfRecord.Date)
            && (searchValues.PersonnelId == 0 ||
            cl.CellLogOfficerId == searchValues.PersonnelId)).
            Select(s => new MonitorTimeLineDetailsVm
            {
                Category = MonitorTimeLineConstants.CELLLOG,
                DateOfRecord = s.CellLogDate,
                DetailFour = s.CellLogCount ?? 0,
                DetailOne = s.CellLogObservation,
                DetailTwo = s.CellLogComments,
                HousingUnitLoation = s.HousingUnitListId > 0 ? new HousingDetail
                {
                    HousingUnitLocation = s.HousingUnitLocation,
                    HousingUnitNumber = s.HousingUnitNumber
                } : new HousingDetail(),
                Personnel = new PersonnelVm
                {
                    PersonFirstName = s.CellLogOfficer.PersonNavigation.PersonFirstName,
                    PersonLastName = s.CellLogOfficer.PersonNavigation.PersonLastName,
                    OfficerBadgeNumber = s.CellLogOfficer.OfficerBadgeNum
                }
            });

            monitorTimeLineDetail.AddRange(cellLogList);
        }
        private void VisitationFlagList(MonitorTimeLineSearchVm searchValues,
    List<MonitorTimeLineDetailsVm> monitorTimeLineDetail,
    List<LookupVm> lstLookup, List<PersonnelVm> personnelVm)
        {

            DateTime endDate = searchValues.DateOfRecord.Date.AddHours(23).AddMinutes(59).AddSeconds(59);
            List<AoAppointmentVm> lstAppointment = _context.Visit.Where(a =>
                !a.DeleteFlag &&
                a.LocationId.HasValue &&
                a.InmateId.HasValue &&
                a.EndDate.HasValue &&
                searchValues.DateOfRecord <= a.StartDate &&
                endDate >= a.EndDate &&
                a.CompleteRegistration &&
                a.RegFacilityId == searchValues.FacilityId &&
                a.VisitDenyFlag != 1 &&
                !a.CompleteVisitFlag &&
                (searchValues.InmateId == 0 || searchValues.InmateId == a.InmateId)
                && (searchValues.PersonnelId == 0 || a.CreateBy == searchValues.PersonnelId)).Select(ii =>
                new AoAppointmentVm
                {
                    StartDate = ii.StartDate,
                    InmateId = ii.InmateId ?? 0,
                    ReasonId = ii.ReasonId ?? 0,
                    LocationId = ii.LocationId ?? 0,
                    ScheduleId = ii.ScheduleId,
                    VisitType = ii.VisitorType ?? 0,
                    CreateBy = ii.CreateBy,
                    InmateDetails = new InmateDetailsList
                    {
                        PersonFirstName = ii.Inmate.Person.PersonFirstName,
                        PersonMiddleName = ii.Inmate.Person.PersonMiddleName,
                        PersonLastName = ii.Inmate.Person.PersonLastName,
                    }
                }).ToList();
            if (lstAppointment.Count > 0)
            {
                List<int> scheduleIds = lstAppointment.Select(ii => ii.ScheduleId).ToList();

                IQueryable<VisitorToInmate> visitorToInmate = _context.VisitorToInmate.
                          Where(a => !a.DeleteFlag.HasValue || a.DeleteFlag == 0);

                IQueryable<Privileges> lstPrivileges = _context.Privileges
                 .Where(pri => lstAppointment.Select(ii => ii.LocationId).Contains(pri.PrivilegeId));

                IQueryable<VisitDetails> lstVisitToVisitor = _context.VisitToVisitor.Where(ii =>
                    scheduleIds.Any(sId => sId == ii.ScheduleId) &&
                    !ii.PrimaryVisitorId.HasValue).Select(s => new VisitDetails
                    {
                        VisitToVisitorId = s.VisitToVisitorId,
                        ScheduleId = s.ScheduleId,
                        PrimaryVisitor = new PersonInfoVm
                        {
                            PersonLastName = s.Visitor.PersonLastName,
                            PersonFirstName = s.Visitor.PersonFirstName,
                            PersonMiddleName = s.Visitor.PersonMiddleName
                        },
                        PersonId = s.PersonId
                    });

                lstAppointment.ForEach(appt =>
                            {

                                appt.PrimaryVisitorDetails = lstVisitToVisitor.Single(a => a.ScheduleId == appt.ScheduleId);

                                int? relationship = visitorToInmate.Last(v => v.VisitorId == appt.PrimaryVisitorDetails.PersonId
                                    && (searchValues.InmateId == 0 || v.InmateId == searchValues.InmateId)).VisitorRelationship;

                                if (relationship > 0)
                                {
                                    appt.PrimaryVisitorDetails.Relationship = lstLookup.SingleOrDefault(l =>
                                        l.LookupIndex == relationship
                                        && l.LookupType == LookupConstants.RELATIONS)?.LookupDescription;
                                }
                                appt.Location = lstPrivileges.Single(ll => ll.PrivilegeId == appt.LocationId)
                                            .PrivilegeDescription;

                                appt.Reason = lstLookup.SingleOrDefault(rr => rr.LookupIndex == appt.ReasonId
                                && rr.LookupType == LookupConstants.APPTREAS)?.LookupDescription;

                                appt.personnelInfo = personnelVm.SingleOrDefault(s => s.PersonnelId == appt.CreateBy);
                            });

                List<MonitorTimeLineDetailsVm> finlaResult = lstAppointment.
                    Select(s => new MonitorTimeLineDetailsVm
                    {
                        DateOfRecord = s.StartDate,
                        VisitBy = new PersonVm
                        {
                            PersonLastName = s.PrimaryVisitorDetails.PrimaryVisitor.PersonLastName,
                            PersonMiddleName = s.PrimaryVisitorDetails.PrimaryVisitor.PersonMiddleName,
                            PersonFirstName = s.PrimaryVisitorDetails.PrimaryVisitor.PersonFirstName
                        },
                        Personnel = new PersonnelVm
                        {
                            PersonFirstName = s.personnelInfo.PersonFirstName,
                            PersonLastName = s.personnelInfo.PersonLastName,
                            PersonMiddleName = s.personnelInfo.PersonMiddleName
                        },
                        InmateInfo = new PersonVm
                        {
                            PersonFirstName = s.InmateDetails.PersonFirstName,
                            PersonLastName = s.InmateDetails.PersonLastName,
                            PersonMiddleName = s.InmateDetails.PersonMiddleName
                        },
                        DetailOne = s.PrimaryVisitorDetails.Relationship,
                        DetailTwo = s.Reason,
                        DetailThree = s.Location
                    }).ToList();

                monitorTimeLineDetail.AddRange(finlaResult);
            }

        }

        private void TrackingFacilityFlagList(MonitorTimeLineSearchVm searchValues,
     List<MonitorTimeLineDetailsVm> monitorTimeLineDetail)
        {
            IQueryable<MonitorTimeLineDetailsVm> checkOutList = _context.InmateTrak.Where(t =>
                  (t.InmateTrakDateOut.HasValue &&
                  t.InmateTrakDateOut.Value.Date == searchValues.DateOfRecord.Date) &&
                  (searchValues.FacilityId == 0 || t.FacilityId.HasValue &&
                    t.FacilityId == searchValues.FacilityId) &&
                    (searchValues.PersonnelId == 0 || t.OutPersonnelId == searchValues.PersonnelId) &&
                    (searchValues.InmateId == 0 || t.InmateId == searchValues.InmateId))
                .Select(s => new MonitorTimeLineDetailsVm
                {
                    Category = MonitorTimeLineConstants.TRACKINGFACILITY,
                    DateOfRecord = s.InmateTrakDateOut,
                    DetailInDate = s.InmateTrakDateOut,
                    DetailOne = s.Facility.FacilityAbbr,
                    DetailTwo = s.InmateTrakLocation,
                    InmateInfo = s.InmateId > 0 ? new PersonVm
                    {
                        PersonLastName = s.Inmate.Person.PersonLastName,
                        PersonFirstName = s.Inmate.Person.PersonFirstName,
                        PersonMiddleName = s.Inmate.Person.PersonMiddleName
                    } : new PersonVm(),
                    Personnel = new PersonnelVm
                    {
                        PersonFirstName = s.OutPersonnel.PersonNavigation.PersonFirstName,
                        PersonLastName = s.OutPersonnel.PersonNavigation.PersonLastName,
                        OfficerBadgeNumber = s.OutPersonnel.OfficerBadgeNum
                    }
                });

            monitorTimeLineDetail.AddRange(checkOutList);

            IQueryable<MonitorTimeLineDetailsVm> checkInList = _context.InmateTrak.Where
            (t => (t.InmateTrakDateIn.HasValue && t.InmateTrakDateIn.Value.Date
                == searchValues.DateOfRecord.Date) &&
                (searchValues.FacilityId == 0 || t.FacilityId.HasValue &&
                t.FacilityId == searchValues.FacilityId) &&
                (searchValues.PersonnelId == 0 || t.InPersonnelId == searchValues.PersonnelId) &&
                (searchValues.InmateId == 0 || t.InmateId == searchValues.InmateId)).Select(s =>
                new MonitorTimeLineDetailsVm
                {
                    Category = MonitorTimeLineConstants.TRACKINGFACILITY,
                    DateOfRecord = s.InmateTrakDateIn,
                    DetailInDate = s.InmateTrakDateOut,
                    DetailOne = s.Facility.FacilityAbbr,
                    DetailTwo = s.InmateTrakLocation,
                    InmateInfo = s.InmateId > 0 ? new PersonVm
                    {
                        PersonLastName = s.Inmate.Person.PersonLastName,
                        PersonFirstName = s.Inmate.Person.PersonFirstName,
                        PersonMiddleName = s.Inmate.Person.PersonMiddleName
                    } : new PersonVm(),
                    Personnel = new PersonnelVm
                    {
                        PersonFirstName = s.InPersonnel.PersonNavigation.PersonFirstName,
                        PersonLastName = s.InPersonnel.PersonNavigation.PersonLastName,
                        OfficerBadgeNumber = s.InPersonnel.OfficerBadgeNumber
                    }
                });
            monitorTimeLineDetail.AddRange(checkInList);
        }

        private void TrackingNoFacilityFlagList(MonitorTimeLineSearchVm searchValues,
     List<MonitorTimeLineDetailsVm> monitorTimeLineDetail)
        {
            IQueryable<MonitorTimeLineDetailsVm> outList = _context.InmateTrak.Where(ii =>
            (!ii.FacilityId.HasValue || ii.FacilityId == 0) &&
            (ii.InmateTrakDateOut.HasValue && ii.InmateTrakDateOut.Value.Date == searchValues.DateOfRecord.Date) &&
            (searchValues.PersonnelId == 0 || ii.OutPersonnelId == searchValues.PersonnelId) &&
            (searchValues.InmateId == 0 || ii.InmateId == searchValues.InmateId))
            .Select(s => new MonitorTimeLineDetailsVm
            {
                Category = MonitorTimeLineConstants.TRACKINGNOFACILITY,
                DateOfRecord = s.InmateTrakDateOut,
                DetailInDate = s.InmateTrakDateOut,
                DetailOne = s.Facility.FacilityAbbr,
                DetailTwo = s.InmateTrakLocation,
                InmateInfo = new PersonVm
                {
                    PersonLastName = s.Inmate.Person.PersonLastName,
                    PersonFirstName = s.Inmate.Person.PersonFirstName,
                    PersonMiddleName = s.Inmate.Person.PersonMiddleName,
                },
                Personnel = new PersonnelVm
                {
                    PersonFirstName = s.OutPersonnel.PersonNavigation.PersonFirstName,
                    PersonLastName = s.OutPersonnel.PersonNavigation.PersonLastName,
                    OfficerBadgeNumber = s.OutPersonnel.OfficerBadgeNum
                }
            });
            monitorTimeLineDetail.AddRange(outList);

            IQueryable<MonitorTimeLineDetailsVm> inList = _context.InmateTrak.Where(ii =>
                (!ii.FacilityId.HasValue || ii.FacilityId == 0) &&
                ii.InmateTrakDateIn.Value.Date == searchValues.DateOfRecord.Date &&
                (searchValues.PersonnelId == 0 || ii.InPersonnelId == searchValues.PersonnelId) &&
                (searchValues.InmateId == 0 || ii.InmateId == searchValues.InmateId)).Select(s =>
                new MonitorTimeLineDetailsVm
                {
                    Category = MonitorTimeLineConstants.TRACKINGNOFACILITY,
                    DateOfRecord = s.InmateTrakDateIn,
                    DetailInDate = s.InmateTrakDateOut,
                    DetailOne = s.Facility.FacilityAbbr,
                    DetailTwo = s.InmateTrakLocation,
                    InmateInfo = new PersonVm
                    {
                        PersonLastName = s.Inmate.Person.PersonLastName,
                        PersonFirstName = s.Inmate.Person.PersonFirstName,
                        PersonMiddleName = s.Inmate.Person.PersonMiddleName,
                    },
                    Personnel = new PersonnelVm
                    {
                        PersonFirstName = s.InPersonnel.PersonNavigation.PersonFirstName,
                        PersonLastName = s.InPersonnel.PersonNavigation.PersonLastName,
                        OfficerBadgeNumber = s.InPersonnel.OfficerBadgeNumber
                    }
                });
            monitorTimeLineDetail.AddRange(inList);
        }

        private void MedDistributeFlagList(MonitorTimeLineSearchVm searchValues,
     List<MonitorTimeLineDetailsVm> monitorTimeLineDetail)
        {
            IQueryable<MonitorTimeLineDetailsVm> medList = _context.PrescriptionDistribution.Where
              (pr => (pr.CreateDate.HasValue && pr.CreateDate.Value.Date == searchValues.DateOfRecord.Date) &&
                  (searchValues.FacilityId == 0 || pr.Inmate.FacilityId == searchValues.FacilityId) &&
                  (searchValues.PersonnelId == 0 ||
                      pr.PrescriptionDistributionOfficerId == searchValues.PersonnelId) &&
                  (searchValues.InmateId == 0 ||
                      pr.InmateId == searchValues.InmateId)).Select(s => new MonitorTimeLineDetailsVm
                      {
                          Category = MonitorTimeLineConstants.MEDDISTRIBUTE,
                          DateOfRecord = s.CreateDate,
                          DetailOne = s.PrescriptionOrder.PrescriptionDrugName,
                          DetailTwo = s.PrescriptionOrder.PrescriptionDosage,
                          DetailThree = s.PrescriptionOrder.PrescriptionDosageUom,
                          DetailFour = s.PrescriptionOrder.PrescriptionQtyPerDist ?? 0,
                          AcceptedFlag = s.AcceptedFlag,
                          RejectedFlag = s.RejectedFlag,
                          InmateInfo = new PersonVm
                          {
                              PersonLastName = s.Inmate.Person.PersonLastName,
                              PersonFirstName = s.Inmate.Person.PersonFirstName,
                              PersonMiddleName = s.Inmate.Person.PersonMiddleName,
                          },
                          Personnel = new PersonnelVm
                          {
                              PersonFirstName = s.PrescriptionDistributionOfficer.PersonNavigation.PersonFirstName,
                              PersonLastName = s.PrescriptionDistributionOfficer.PersonNavigation.PersonLastName,
                              OfficerBadgeNumber = s.PrescriptionDistributionOfficer.OfficerBadgeNum
                          }
                      });

            monitorTimeLineDetail.AddRange(medList);
        }

        #endregion
    }
}
