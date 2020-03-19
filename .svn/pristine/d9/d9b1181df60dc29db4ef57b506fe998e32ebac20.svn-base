using GenerateTables.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using ServerAPI.ViewModels;
using ServerAPI.Utilities;

namespace ServerAPI.Services
{
    public class StatusBoardOverviewService : IStatusBoardOverviewService
    {
        private readonly AAtims _context;
        private IQueryable<Inmate> _inmateList;
        private IQueryable<InmatePrebook> _inmatePrebook;
        private IQueryable<Incarceration> _incarceration;
        private List<Lookup> _lookups;
        private readonly IPhotosService _photos;
        private readonly IAppointmentViewerService _appointmentViewerService;

        public StatusBoardOverviewService(AAtims context, IPhotosService photosService, IAppointmentViewerService appointmentViewerService)
        {
            _context = context;
            _photos = photosService;
            _appointmentViewerService = appointmentViewerService;
        }

        public StatusBoardOveriviewVm GetStatusBoardOverview(int facilityId)
        {
            _inmatePrebook = _context.InmatePrebook.Where(w => (!w.DeleteFlag.HasValue || w.DeleteFlag == 0)
               && w.CompleteFlag == 1 && w.FacilityId == facilityId);

            _inmateList = _context.Inmate.Where(w => w.InmateActive == 1 && w.FacilityId == facilityId);
            
            _incarceration = _context.Incarceration
                .Where(w => !w.ReleaseOut.HasValue && w.Inmate.FacilityId == facilityId && w.Inmate.InmateActive == 1);

            _lookups = _context.Lookup
                .Where(w => w.LookupType == LookupConstants.DISCTYPE || w.LookupType == LookupConstants.DISCINTYPE
                || w.LookupType == LookupConstants.GRIEVTYPE).ToList();

            StatusBoardOveriviewVm statusBoard = new StatusBoardOveriviewVm
            {
                PreScreening = GetPreScreening(),
                PreScreenInProgress = GetPreScreenInProgress(),
                PreScreenComplete = GetPreScreenComplete(facilityId),
                WizardQueue = GetWizardQueue(),
                TaskQueue = GetTaskQueue(facilityId),
                SupervisorQueue = GetSupervisorQueue(facilityId),
                ActiveIncident = GetActiveIncident(facilityId),
                IncidentInmate = GetIncidentInmate(facilityId),
                ActiveGrievance = GetActiveGrievance(facilityId),
                GrievanceInmates = GetGrievanceInmate(facilityId),
                GrievancePersonnel = GetGrievancePersonnel(facilityId),
                InOutCount = GetInOutCount(facilityId),
                FacilityCount = GetFacilityCount(),
                BuildingCount = GetBuildingCount(),
                ExternalCheckOut = GetExternalCheckOut(),
                CheckedOut = GetCheckedOut(facilityId),
                AppointmentToday = GetAppointmentToday(facilityId),
                Sentence = GetSentence(),
                Charge = GetCharge(facilityId),
                ActiveBooking = GetActiveBooking(facilityId),
                Attendance = GetAttendance(facilityId)
            };

            List<OverviewVm> statsCount = LoadStatsCount(_inmateList);

            statusBoard.Flag = statsCount.Where(w => w.OverviewFlag == OverviewType.Flag).ToList();
            statusBoard.Gender = statsCount.Where(w => w.OverviewFlag == OverviewType.Gender).ToList();
            statusBoard.Race = statsCount.Where(w => w.OverviewFlag == OverviewType.Race).ToList();
            statusBoard.Association = statsCount.Where(w => w.OverviewFlag == OverviewType.Association).ToList();
            statusBoard.Classify = statsCount.Where(w => w.OverviewFlag == OverviewType.Classify).ToList();

            return statusBoard;
        }

        public StatusBoardOveriviewVm GetPreScreeningCount(int facilityId)
        {
            _inmatePrebook = _context.InmatePrebook.Where(w => (!w.DeleteFlag.HasValue || w.DeleteFlag == 0)
                && w.CompleteFlag == 1 && w.FacilityId == facilityId);

            StatusBoardOveriviewVm statusBoard = new StatusBoardOveriviewVm
            {
                PreScreening = GetPreScreening(),
                PreScreenInProgress = GetPreScreenInProgress(),
                PreScreenComplete = GetPreScreenComplete(facilityId)
            };

            return statusBoard;
        }

        public StatusBoardOveriviewVm GetWizardSentenceCount(int facilityId)
        {
            _incarceration = _context.Incarceration
                .Where(w => !w.ReleaseOut.HasValue && w.Inmate.FacilityId == facilityId && w.Inmate.InmateActive == 1);
            _inmateList = _context.Inmate.Where(w => w.InmateActive == 1 && w.FacilityId == facilityId);
            StatusBoardOveriviewVm statusBoard = new StatusBoardOveriviewVm
            {
                WizardQueue = GetWizardQueue(),
                SupervisorQueue = GetSupervisorQueue(facilityId),
                Sentence = GetSentence(),
                Charge = GetCharge(facilityId)
            };

            return statusBoard;
        }

        public StatusBoardOveriviewVm GetIncidentCount(int facilityId)
        {
            _lookups = _context.Lookup
                .Where(w => w.LookupType == LookupConstants.DISCTYPE || w.LookupType == LookupConstants.DISCINTYPE
                    || w.LookupType == LookupConstants.GRIEVTYPE).ToList();
            StatusBoardOveriviewVm statusBoard = new StatusBoardOveriviewVm
            {
                ActiveIncident = GetActiveIncident(facilityId),
                IncidentInmate = GetIncidentInmate(facilityId)
            };

            return statusBoard;
        }

        public StatusBoardOveriviewVm GetInmateCount(int facilityId)
        {
            _inmateList = _context.Inmate.Where(w => w.InmateActive == 1 && w.FacilityId == facilityId);

            StatusBoardOveriviewVm statusBoard = new StatusBoardOveriviewVm
            {
                TaskQueue = GetTaskQueue(facilityId),
                InOutCount = GetInOutCount(facilityId),
                FacilityCount = GetFacilityCount(),
                BuildingCount = GetBuildingCount(),
                ExternalCheckOut = GetExternalCheckOut(),
                CheckedOut = GetCheckedOut(facilityId),
                AppointmentToday = GetAppointmentToday(facilityId),
                ActiveBooking = GetActiveBooking(facilityId),
                Attendance = GetAttendance(facilityId)
            };

            return statusBoard;
        }

        public StatusBoardOveriviewVm GetStatsCount(int facilityId)
        {
            _inmateList = _context.Inmate.Where(w => w.InmateActive == 1 && w.FacilityId == facilityId);

            List<OverviewVm> statsCount = LoadStatsCount(_inmateList);

            StatusBoardOveriviewVm statusBoard = new StatusBoardOveriviewVm
            {
                Flag = statsCount.Where(w => w.OverviewFlag == OverviewType.Flag).ToList(),
                Gender = statsCount.Where(w => w.OverviewFlag == OverviewType.Gender).ToList(),
                Race = statsCount.Where(w => w.OverviewFlag == OverviewType.Race).ToList(),
                Association = statsCount.Where(w => w.OverviewFlag == OverviewType.Association).ToList(),
                Classify = statsCount.Where(w => w.OverviewFlag == OverviewType.Classify).ToList()
            };

            return statusBoard;
        }

        public StatusBoardOveriviewVm GetGrievanceCount(int facilityId)
        {

            _lookups = _context.Lookup
                .Where(w => w.LookupType == LookupConstants.DISCTYPE || w.LookupType == LookupConstants.DISCINTYPE
                    || w.LookupType == LookupConstants.GRIEVTYPE).ToList();

            StatusBoardOveriviewVm statusBoard = new StatusBoardOveriviewVm
            {
                ActiveGrievance = GetActiveGrievance(facilityId),
                GrievanceInmates = GetGrievanceInmate(facilityId),
                GrievancePersonnel = GetGrievancePersonnel(facilityId)
            };

            return statusBoard;
        }

        private List<OverviewVm> GetPreScreening()
        {
            List<OverviewVm> listPreScreen = new List<OverviewVm>
            {
                new OverviewVm
                {
                    Count = _inmatePrebook.Count(w =>
                        (!w.CourtCommitFlag.HasValue || w.CourtCommitFlag == 0)
                        && (!w.IncarcerationId.HasValue || w.IncarcerationId == 0)
                        && (!w.TempHoldId.HasValue || w.TempHoldId == 0) &&
                        (!w.MedPrescreenStartFlag.HasValue || w.MedPrescreenStartFlag == 0)
                        && (!w.MedPrescreenStatusFlag.HasValue || w.MedPrescreenStatusFlag == 0)),
                    Description = StatusBoardOverviewConstants.READYFORMEDICALPRESCREEN,
                    OverviewFlag = OverviewType.MedPreScreening
                },
                new OverviewVm
                {
                    Count = _inmatePrebook.Count(c => c.CourtCommitFlag == 1
                        && !c.MedPrescreenStartFlag.HasValue &&
                        (!c.MedPrescreenStatusFlag.HasValue || c.MedPrescreenStatusFlag == 0)
                        && c.PrebookDate.HasValue && (!c.TemporaryHold.HasValue || c.TemporaryHold == 0) &&
                        (!c.IncarcerationId.HasValue || c.IncarcerationId == 0)
                        && c.PrebookDate.Value.Date >= DateTime.Now.Date),
                    Description = StatusBoardOverviewConstants.COURTCOMMITSCHEDULED,
                    OverviewFlag = OverviewType.MedPreScreening
                },
                new OverviewVm
                {
                    Count = _inmatePrebook.Count(c => c.CourtCommitFlag == 1
                        && !c.MedPrescreenStartFlag.HasValue &&
                        (!c.MedPrescreenStatusFlag.HasValue || c.MedPrescreenStatusFlag == 0)
                        && c.PrebookDate.HasValue && (!c.TemporaryHold.HasValue || c.TemporaryHold == 0)
                        && (!c.IncarcerationId.HasValue || c.IncarcerationId == 0) &&
                        c.PrebookDate.Value.Date < DateTime.Now.Date),
                    Description = StatusBoardOverviewConstants.COURTCOMMITOVERDUE,
                    OverviewFlag = OverviewType.MedPreScreening
                },
                new OverviewVm
                {
                    Count = _inmatePrebook.Count(c => c.MedPrescreenStartFlag == 1 && c.MedPrescreenStatusFlag == -1
                        && c.MedPrescreenStatusDate.HasValue && (!c.TemporaryHold.HasValue || c.TemporaryHold == 0)
                        && (!c.IncarcerationId.HasValue || c.IncarcerationId == 0)
                        && c.MedPrescreenStatusDate.Value.Date.AddDays(-2) < c.MedPrescreenStatusDate.Value.Date
                        && c.MedPrescreenStatusDate.Value.Date <= DateTime.Now.Date),
                    Description = StatusBoardOverviewConstants.MEDICALLYREJECTEDLAST48HOURS,
                    OverviewFlag = OverviewType.MedPreScreening
                },
            };

            return listPreScreen;
        }

        private List<OverviewVm> GetPreScreenInProgress()
        {
            List<OverviewVm> listPreScreen = new List<OverviewVm>
            {
                new OverviewVm
                {
                    Count = _inmatePrebook.Count(c =>
                        (!c.MedPrescreenStatusFlag.HasValue || c.MedPrescreenStatusFlag == 0)
                        && !c.IncarcerationId.HasValue && c.MedPrescreenStartFlag == 1 && (!c.TemporaryHold.HasValue ||
                            c.TemporaryHold == 0)),
                    Description = StatusBoardOverviewConstants.MEDICALPRESCREENINPROGRESS,
                    OverviewFlag = OverviewType.MedPreScreenInProgress
                },

                new OverviewVm
                {
                    Count = _inmatePrebook.Count(c => c.MedPrescreenStatusFlag == 2 && c.MedPrescreenStartFlag == 1),
                    Description = StatusBoardOverviewConstants.BYPASSEDMEDICALPRESCREEN,
                    OverviewFlag = OverviewType.MedPreScreenInProgress
                }
            };

            return listPreScreen;
        }

        private List<OverviewVm> GetPreScreenComplete(int facilityId)
        {
            IQueryable<InmatePrebook> inmatePrebooks3 = _context.InmatePrebook
                .Where(w => (!w.DeleteFlag.HasValue || w.DeleteFlag == 0) && w.CompleteFlag == 1
                    && !w.IncarcerationId.HasValue && (!w.TempHoldId.HasValue || w.TempHoldId == 0)
                    && w.MedPrescreenStatusFlag > 0 && w.FacilityId == facilityId);
            List<OverviewVm> listPreScreen = new List<OverviewVm>
            {
                new OverviewVm{Count=inmatePrebooks3.Count(c => !c.TemporaryHold.HasValue ||
                                           c.TemporaryHold == 0 && (c.PersonId.HasValue || !c.PersonId.HasValue)),
                    Description =StatusBoardOverviewConstants.READYFORINTAKE,OverviewFlag=OverviewType.MedPrescreenComplete },
                new OverviewVm
                {
                    Count = inmatePrebooks3.Count(c => (c.PersonId.HasValue || !c.PersonId.HasValue) && c.TemporaryHold == 1),
                    Description = StatusBoardOverviewConstants.READYFORTEMPHOLD,OverviewFlag=OverviewType.MedPrescreenComplete
                }
            };

            return listPreScreen;
        }

        private List<OverviewVm> GetWizardQueue()
        {
            List<OverviewVm> listPreScreen = new List<OverviewVm>
            {
                new OverviewVm
                {
                    Count = _inmateList
                        .Count(c => c.Incarceration.Any(a => !a.IntakeCompleteFlag.HasValue && !a.ReleaseOut.HasValue)),
                    Description = StatusBoardOverviewConstants.INTAKE,
                    OverviewFlag = OverviewType.WizardQueue
                },
                new OverviewVm
                {
                    Count = _incarceration
                        .Count(c => c.IntakeCompleteFlag == 1 && !c.BookCompleteFlag.HasValue
                            || c.BookAndReleaseFlag == 1),
                    Description = StatusBoardOverviewConstants.BOOKING, OverviewFlag = OverviewType.WizardQueue
                },
                new OverviewVm
                {
                    Count = _incarceration
                        .Count(c => c.OverallFinalReleaseDate <= DateTime.Now &&
                            c.OverallFinalReleaseDate.HasValue ||
                            c.ReleaseClearFlag == 1 || c.BookAndReleaseFlag == 1),
                    Description = StatusBoardOverviewConstants.RELEASE, OverviewFlag = OverviewType.WizardQueue
                },
                new OverviewVm
                {
                    Count = _incarceration.Count(c => c.BookAndReleaseFlag == 1),
                    Description = StatusBoardOverviewConstants.BOOKANDRELEASE,
                    OverviewFlag = OverviewType.WizardQueue
                }
            };

            return listPreScreen;
        }

        private List<OverviewVm> GetTaskQueue(int facilityId)
        {
            List<OverviewVm> listTask = _context.AoTaskQueue
                .Where(w => !w.CompleteFlag && !w.AoTaskLookup.DeleteFlag && w.Inmate.FacilityId == facilityId && w.Inmate.InmateActive == 1)
                .GroupBy(g => g.AoTaskLookup.TaskName).Select(s => new OverviewVm
                { Count = s.Count(), Description = s.Key, OverviewFlag = OverviewType.Task }).ToList();

            return listTask;
        }

        private List<OverviewVm> GetSupervisorQueue(int facilityId)
        {
            IQueryable<IncarcerationArrestXref> incarcerations = _context.IncarcerationArrestXref
                .Where(w => !w.Incarceration.ReleaseOut.HasValue && w.Incarceration.Inmate.FacilityId == facilityId
                                                                 && w.Incarceration.Inmate.InmateActive == 1);

            List<OverviewVm> listSupervisor = new List<OverviewVm>
            {
                new OverviewVm
                {
                    Count = _incarceration
                        .Count(c => c.BookCompleteFlag == 1 && !c.BookingSupervisorCompleteFlag.HasValue),
                    Description = StatusBoardOverviewConstants.SUPERVISORREVIEWOVERALL,
                    OverviewFlag = OverviewType.SupervisorQueues
                },
                new OverviewVm
                {
                    Count = incarcerations
                        .Count(c => c.Incarceration.BookCompleteFlag == 1 && !c.BookingSupervisorCompleteFlag.HasValue
                            && c.Arrest.BookingCompleteFlag == 1),
                    Description = StatusBoardOverviewConstants.SUPERVISORREVIEWBOOKING,
                    OverviewFlag = OverviewType.SupervisorQueues
                },
                new OverviewVm
                {
                    Count = _incarceration
                        .Count(c => c.ReleaseClearFlag == 1 && (!c.ReleaseSupervisorCompleteFlag.HasValue ||
                            c.ReleaseSupervisorCompleteFlag == 0)),
                    Description = StatusBoardOverviewConstants.REVIEWRELEASE,
                    OverviewFlag = OverviewType.SupervisorQueues
                },
                new OverviewVm
                {
                    Count = incarcerations.Count(c => c.ReleaseDate.HasValue
                        && !c.ReleaseSupervisorCompleteFlag.HasValue),
                    Description = StatusBoardOverviewConstants.REVIEWCLEAR, OverviewFlag = OverviewType.SupervisorQueues
                }
            };

            return listSupervisor;
        }

        private List<OverviewVm> GetActiveIncident(int facilityId)
        {
            List<OverviewVm> listAciveIncident = _context.DisciplinaryIncident
                .Where(w => w.FacilityId == facilityId && w.DisciplinaryActive == 1)
                .GroupBy(g => g.DisciplinaryType)
                .Select(s => new OverviewVm
                {
                    Count = s.Count(),
                    OverviewId = (int)s.Key,
                    OverviewFlag = OverviewType.Incident
                }).ToList();

            listAciveIncident.ForEach(f => f.Description = _lookups.Where(w => w.LookupIndex == f.OverviewId && 
                w.LookupType == LookupConstants.DISCTYPE).Select(se => se.LookupDescription).FirstOrDefault());

            return listAciveIncident.OrderByDescending(o => o.Description).ToList();
        }

        private List<OverviewVm> GetIncidentInmate(int facilityId)
        {
            List<OverviewIncidentVm> overviewIncident = _context.DisciplinaryInmate.Where(inc =>
                    inc.DisciplinaryIncident.DisciplinaryActive == 1
                    && inc.DisciplinaryIncident.FacilityId == facilityId && inc.DisciplinaryInmateType > 0
                    && (!inc.DeleteFlag.HasValue || inc.DeleteFlag == 0))
                .Select(s => new OverviewIncidentVm
                    {
                        DiscTypeIndex = _lookups.FirstOrDefault(w => w.LookupType == LookupConstants.DISCTYPE &&
                            w.LookupIndex == s.DisciplinaryIncident.DisciplinaryType).LookupIndex,
                        DiscTypeDescription = _lookups.FirstOrDefault(w => w.LookupType == LookupConstants.DISCTYPE &&
                            w.LookupIndex == s.DisciplinaryIncident.DisciplinaryType).LookupDescription,
                        DiscInTypeIndex = _lookups.FirstOrDefault(w => w.LookupType == LookupConstants.DISCINTYPE &&
                            w.LookupIndex == s.DisciplinaryInmateType).LookupIndex,
                        DiscInTypeDescription = _lookups.FirstOrDefault(w =>
                            w.LookupType == LookupConstants.DISCINTYPE &&
                            w.LookupIndex == s.DisciplinaryInmateType).LookupDescription
                    }
                ).ToList();

            List<OverviewVm> listAciveIncident = overviewIncident
                .Where(w => w.DiscInTypeIndex > 0 && w.DiscTypeIndex > 0)
                .OrderBy(o => o.DiscTypeDescription).ThenBy(t => t.DiscInTypeDescription)
                .GroupBy(g => new
                {
                    g.DiscInTypeIndex,
                    g.DiscInTypeDescription,
                    g.DiscTypeDescription
                }).Select(s => new OverviewVm
                {
                    Count = s.Count(),
                    OverviewId = (int) s.Key.DiscInTypeIndex,
                    Description = s.Key.DiscInTypeDescription,
                    Type = s.Key.DiscTypeDescription,
                    OverviewFlag = OverviewType.IncidentInmates
                }).ToList();

            return listAciveIncident;
        }

        private List<MonitorAttendanceVm> GetActiveGrievance(int facilityId)
        {
            List<MonitorAttendanceVm> listAciveGrievance = _context.Grievance
                .SelectMany(gr => _lookups.Where(w => w.LookupIndex == gr.GrievanceType
                   && w.LookupType == LookupConstants.GRIEVTYPE && gr.DeleteFlag == 0 && gr.FacilityId == facilityId
                   && !gr.SetReview.HasValue && gr.GrievanceType > 0),
                    (gr, look) => new { gr, look })
                .OrderBy(o => o.look.LookupOrder).ThenBy(t => t.look.LookupDescription)
                .GroupBy(g => new
                {
                    g.look.LookupIndex,
                    g.look.LookupDescription
                }).Select(s => new MonitorAttendanceVm
                {
                    Count = s.Count(),
                    OverviewId = s.Key.LookupIndex,
                    Department = s.Key.LookupDescription,
                    OverviewFlag = OverviewType.Grievance
                }).ToList();

            return listAciveGrievance;
        }

        private List<OverviewVm> GetGrievanceInmate(int facilityId) => _context.Grievance
            .SelectMany(gr => _lookups.Where(w => w.LookupIndex == gr.GrievanceType && 
                    w.LookupType == LookupConstants.GRIEVTYPE && gr.DeleteFlag == 0 && gr.FacilityId == facilityId && 
                    !gr.SetReview.HasValue || w.LookupIndex == gr.GrievanceType && 
                    w.LookupType == LookupConstants.GRIEVTYPE && gr.DeleteFlag == 0 && gr.FacilityId == facilityId
                    && !gr.SetReview.HasValue && gr.GrievanceInmate.Any()),
                (gr, look) => new {gr, look})
            .OrderBy(o => o.look.LookupOrder).ThenBy(t => t.look.LookupDescription)
            .GroupBy(g => new
            {
                g.look.LookupIndex,
                g.look.LookupDescription
            }).Select(s => new OverviewVm
            {
                Count = s.Count(),
                OverviewId = s.Key.LookupIndex,
                Description = s.Key.LookupDescription,
                OverviewFlag = OverviewType.GrievanceInmates
            }).ToList();

        private List<MonitorAttendanceVm> GetGrievancePersonnel(int facilityId) => _context.GrievancePersonnel
            .SelectMany(gr => _lookups.Where(w => w.LookupIndex == gr.Grievance.GrievanceType && 
                    w.LookupType == LookupConstants.GRIEVTYPE && gr.Grievance.DeleteFlag == 0 && 
                    gr.Grievance.FacilityId == facilityId && !gr.Grievance.SetReview.HasValue),
                    (gr, look) => new { gr, look })
                .OrderBy(o => o.look.LookupOrder).ThenBy(t => t.look.LookupDescription)
                .GroupBy(g => new
                {
                    g.look.LookupIndex,
                    g.look.LookupDescription
                }).Select(s => new MonitorAttendanceVm
                {
                    Count = s.Count(),
                    OverviewId = s.Key.LookupIndex,
                    Department = s.Key.LookupDescription,
                    OverviewFlag = OverviewType.GrievancePersonnel
                }).ToList();

        private List<OverviewVm> GetInOutCount(int facilityId)
        {
            IQueryable<Incarceration> incarcerations = _context.Incarceration
                .Where(w => w.Inmate.FacilityId == facilityId);

            List<OverviewVm> listInOutCount = new List<OverviewVm>
            {
                new OverviewVm
                {
                    Count = incarcerations.Count(c => c.DateIn.Value.Date == DateTime.Now.Date),
                    Description = StatusBoardOverviewConstants.BOOKEDTODAY, OverviewFlag = OverviewType.InOut
                },
                new OverviewVm
                {
                    Count = incarcerations.Count(c => c.ReleaseOut.Value.Date == DateTime.Now.Date),
                    Description = StatusBoardOverviewConstants.RELEASEDTODAY, OverviewFlag = OverviewType.InOut
                }
            };

            return listInOutCount;
        }

        private List<OverviewVm> GetFacilityCount() => _context.Inmate.Where(w => w.InmateActive == 1)
            .OrderBy(o => o.Facility.FacilityAbbr)
            .GroupBy(g => new
            {
                g.FacilityId,
                g.Facility.FacilityAbbr
            }).Select(s => new OverviewVm
            {
                Count = s.Count(),
                OverviewId = s.Key.FacilityId,
                Description = s.Key.FacilityAbbr,
                OverviewFlag = OverviewType.Facility
            }).ToList();

        private List<OverviewVm> GetBuildingCount() => _inmateList.OrderBy(o => o.FacilityId)
            .GroupBy(g => new
            {
                g.HousingUnit.HousingUnitLocation
            }).Select(s => new OverviewVm
            {
                Count = s.Count(),
                Description = s.Key.HousingUnitLocation,
                OverviewFlag = OverviewType.Housing
            }).ToList();

        private List<OverviewVm> GetExternalCheckOut() => _context.Inmate.Where(w => 
            !w.InmateCurrentTrackNavigation.FacilityId.HasValue && w.InmateCurrentTrackId > 0 && w.InmateActive == 1)
                .Select(s => new OverviewVm
                {
                    OverviewId = s.InmateCurrentTrackId ?? 0,
                    Description = s.InmateCurrentTrackNavigation.PrivilegeDescription
                }).GroupBy(g => new
                {
                    g.OverviewId,
                    g.Description
                }).Select(x => new OverviewVm
                {
                    OverviewId = x.Key.OverviewId,
                    Description = x.Key.Description,
                    Count = x.Count(),
                    OverviewFlag = OverviewType.ExternalLocation
                }).OrderBy(o => o.Description).ToList();

        private List<OverviewVm> GetCheckedOut(int facilityId) => _inmateList
                .Where(w => w.InmateCurrentTrackNavigation.FacilityId == facilityId)
                .Select(s => new OverviewVm
                {
                    OverviewId = s.InmateCurrentTrackId ?? 0,
                    Description = s.InmateCurrentTrack
                }).GroupBy(g => new
                {
                    g.OverviewId,
                    g.Description
                }).Select(x => new OverviewVm
                {
                    OverviewId = x.Key.OverviewId,
                    Description = x.Key.Description,
                    Count = x.Count(),
                    OverviewFlag = OverviewType.Location
                }).OrderBy(o => o.Description).ToList();

        private List<OverviewVm> LoadStatsCount(IQueryable<Inmate> inmateList)
        {
            //To get list of Lookup details
            List<Lookup> lookupList = _context.Lookup.Where(l => (l.LookupType == LookupConstants.PERSONCAUTION
                    || l.LookupType == LookupConstants.TRANSCAUTION || l.LookupType == LookupConstants.DIET
                    || l.LookupType == LookupConstants.SEX
                    || l.LookupType == LookupConstants.RACE) && l.LookupInactive == 0)
                .OrderByDescending(o => o.LookupOrder).ThenBy(t => t.LookupDescription).ToList();

            //To get the Person Flag details
            IEnumerable<PersonFlag> lstPersonFlag = inmateList
                .Where(w => w.Person.PersonFlag.Any(a => a.DeleteFlag == 0)).SelectMany(s => s.Person.PersonFlag);

            List<OverviewVm> overview = lstPersonFlag.SelectMany(perFlag => lookupList.Where(w => 
                (w.LookupIndex == perFlag.PersonFlagIndex && w.LookupType == LookupConstants.PERSONCAUTION
                    || w.LookupIndex == perFlag.InmateFlagIndex && w.LookupType == LookupConstants.TRANSCAUTION
                    || w.LookupIndex == perFlag.DietFlagIndex && w.LookupType == LookupConstants.DIET) 
                && perFlag.DeleteFlag == 0),
                    (perFlag, look) => new {perFlag, look})
                .OrderBy(o => o.look.LookupOrder).ThenBy(t => t.look.LookupDescription)
                .GroupBy(g => new
                {
                    g.look.LookupIndex,
                    g.look.LookupDescription
                }).Select(x => new OverviewVm
                {
                    Count = x.Count(),
                    Description = x.Key.LookupDescription,
                    OverviewId = x.Key.LookupIndex,
                    OverviewFlag = OverviewType.Flag,
                    Type = x.Select(se => se.look.LookupType).FirstOrDefault(),
                }).ToList();

            //Gender list
            // TODO: QueryClientEvaluationWarning
            List<OverviewVm> genderFlagList = inmateList.SelectMany(inm => lookupList.Where(w =>
                inm.Person.PersonSexLast.HasValue &&
                w.LookupIndex == inm.Person.PersonSexLast.Value && w.LookupType == LookupConstants.SEX),
                    (inm, l) => new OverviewVm
                    {
                        OverviewId = l.LookupIndex,
                        Description = l.LookupDescription
                    }).ToList();

            genderFlagList.AddRange(inmateList
                .Where(w => !w.Person.PersonSexLast.HasValue
                            || w.Person.PersonSexLast == 0).Select(s =>
                    new OverviewVm
                    {
                        OverviewId = 0
                    }));

            overview.AddRange(genderFlagList.OrderBy(o => o.Description)
                .GroupBy(g => new
                {
                    g.Description,
                    g.OverviewId
                }).Select(x => new OverviewVm
                {
                    Count = x.Count(),
                    Description = x.Key.Description,
                    OverviewFlag = OverviewType.Gender,
                    OverviewId = x.Key.OverviewId
                }));

            //Race list
            List<OverviewVm> raceFlagList = inmateList
                .SelectMany(inm => lookupList
                        .Where(w => w.LookupIndex == inm.Person.PersonRaceLast
                                    && w.LookupType == LookupConstants.RACE),
                    (inm, l) => new OverviewVm
                    {
                        OverviewId = l.LookupIndex,
                        Description = l.LookupDescription
                    }).ToList();

            raceFlagList.AddRange(inmateList
                .Where(w => !w.Person.PersonRaceLast.HasValue
                            || w.Person.PersonRaceLast == 0).Select(s =>
                    new OverviewVm
                    {
                        OverviewId = 0
                    }));

            overview.AddRange(raceFlagList.GroupBy(g => new
            {
                g.Description,
                g.OverviewId
            }).Select(x => new OverviewVm
            {
                Count = x.Count(),
                Description = x.Key.Description,
                OverviewFlag = OverviewType.Race,
                OverviewId = x.Key.OverviewId
            }).OrderBy(o => o.Description));

            // For Association List
            IEnumerable<PersonClassification> lstPersonClassification = inmateList
                .Where(w => w.Person.PersonClassification
                    .Any(p => p.PersonClassificationDateFrom <= DateTime.Now
                        && (!p.PersonClassificationDateThru.HasValue
                            || p.PersonClassificationDateThru >= DateTime.Now)))
                .SelectMany(s => s.Person.PersonClassification);

            overview.AddRange(lstPersonClassification
                .GroupBy(g => new { g.PersonClassificationType, g.PersonClassificationTypeId }).Select(x => new OverviewVm
                {
                    Count = x.Count(),
                    Description = x.Key.PersonClassificationType,
                    OverviewFlag = OverviewType.Association,
                    OverviewId = x.Key.PersonClassificationTypeId ?? 0,
                }).OrderBy(o => o.Description));

            // For Classification List
            List<OverviewVm> classifyFlagList = inmateList
                 .Where(w => w.InmateClassificationId.HasValue)
               .Select(ic => new OverviewVm
               {
                   Description = ic.InmateClassification.InmateClassificationReason
               }).ToList();

            classifyFlagList.AddRange(inmateList
                .Where(w => !w.InmateClassificationId.HasValue)
                .Select(inm => new OverviewVm
                {
                    OverviewId = inm.InmateId
                }));

            overview.AddRange(classifyFlagList
                .GroupBy(g => g.Description).Select(s =>
                    new OverviewVm
                    {
                        Description = s.Key,
                        Count = s.Count(),
                        OverviewFlag = OverviewType.Classify
                    }).OrderBy(o => o.Description));

            return overview;
        }

        private List<OverviewVm> GetAppointmentToday(int facilityId)
        {
            DateTime startDate = DateTime.Now.Date;
            DateTime endDate = DateTime.Now.Date.AddHours(23).AddMinutes(59).AddSeconds(59);

            CalendarInputs inputs = new CalendarInputs
            {
                CalendarTypes = "1,2,3,4,5,6,7,8",
                FromDate = startDate,
                ToDate = endDate,
                FacilityId = facilityId
            };

            List<OverviewVm> listAppointment = _appointmentViewerService.AppointmentViewer(inputs)
                .GroupBy(g => new { g.Location, g.LocationId })
                .Select(s => new OverviewVm
                {
                    Count = s.Count(),
                    Description = s.Key.Location,
                    OverviewId = s.Key.LocationId,
                    OverviewFlag = OverviewType.TodaysAppointment
                }).ToList();

            return listAppointment;
        }

        private List<OverviewVm> GetSentence() =>
            new List<OverviewVm>
            {
                new OverviewVm
                {
                    Count = _incarceration.Count(c => c.OverallSentStartDate.HasValue && 
                        c.OverallSentStartDate.Value <= DateTime.Now && !c.ReleaseOut.HasValue),
                    Description = StatusBoardOverviewConstants.SENTENCED, OverviewFlag = OverviewType.Sentence
                },

                new OverviewVm
                {
                    Count = _incarceration.Count(c => 
                        (!c.OverallSentStartDate.HasValue || c.OverallSentStartDate > DateTime.Now) && !c.ReleaseOut.HasValue),
                    Description = StatusBoardOverviewConstants.UNSENTENCED, OverviewFlag = OverviewType.Sentence
                }
            };

        private List<OverviewVm> GetCharge(int facilityId)
        {

            List<KeyValuePair<int, string>> crime = _context.Crime.Where(c => c.CrimeLookupId > 0 &&
                    c.CrimeDeleteFlag == 0
                    && !c.WarrantId.HasValue && c.Arrest.IncarcerationArrestXref.Any(a =>
                        !a.Incarceration.ReleaseOut.HasValue && a.Incarceration.Inmate.FacilityId == facilityId
                        && a.Incarceration.Inmate.InmateActive == 1))
                .Select(s => new KeyValuePair<int, string>(s.CrimeLookupId, s.CrimeLookup.CrimeCodeType)).ToList();

            crime.AddRange(_context.CrimeForce.Where(
                    c => c.DeleteFlag == 0 && !c.DropChargeFlag.HasValue && !c.ForceCrimeLookupId.HasValue &&
                        !c.WarrantId.HasValue && !c.ForceSupervisorReviewFlag.HasValue
                        && c.Arrest.IncarcerationArrestXref.Any(a => !a.Incarceration.ReleaseOut.HasValue &&
                            a.Incarceration.Inmate.FacilityId == facilityId
                            && a.Incarceration.Inmate.InmateActive == 1))
                .Select(s => new KeyValuePair<int, string>(s.ForceCrimeLookupId ?? 0, s.TempCrimeCodeType)));

            List<OverviewVm> listCharge = crime
            .GroupBy(g => new
            {
                g.Key,
                g.Value
            })
            .Select(s => new OverviewVm
            {
                Count = s.Count(),
                OverviewId = s.Key.Key,
                Description = s.Key.Value,
                OverviewFlag = OverviewType.Charge
            }).ToList();


            return listCharge;
        }

        private List<OverviewVm> GetActiveBooking(int facilityId) => _context.Arrest
            .Where(w => w.Inmate.InmateActive == 1 && w.Inmate.FacilityId == facilityId
                && w.ArrestActive == 1 && w.ArrestingAgencyId > 0)
            .GroupBy(g => new
            {
                g.ArrestingAgencyId,
                g.ArrestingAgency.AgencyName
            }).Select(s => new OverviewVm
            {
                Count = s.Count(), OverviewId = s.Key.ArrestingAgencyId, Description = s.Key.AgencyName,
                OverviewFlag = OverviewType.ArrestingAgency
            }).OrderBy(o => o.Description).ToList();

        private List<MonitorAttendanceVm> GetAttendance(int facilityId) => _context.AttendanceAo
                .Where(w => w.PersonnelId > 0 && w.FacilityId == facilityId && !w.ClockOutEnter.HasValue
                    && w.ClockInEnter.Value.Date == DateTime.Now.Date)
                .GroupBy(g => new
                {
                    g.Personnel.PersonnelDepartment,
                    g.Personnel.PersonnelPosition
                }).Select(s => new MonitorAttendanceVm
                {
                    Count = s.Count(),
                    Department = s.Key.PersonnelDepartment,
                    Position = s.Key.PersonnelPosition,
                    OverviewFlag = OverviewType.Attendance
                }).OrderBy(o => o.Department).ThenBy(t => t.Position).ToList();

        public List<MonitorPreScreenVm> GetMonitorPreScreenDetail(OverviewDetailInputVm value)
        {
            List<MonitorPreScreenVm> monitorPreScreen = new List<MonitorPreScreenVm>();

            switch (value.OverviewType)
            {
                case OverviewType.MedPreScreening:
                    IQueryable<InmatePrebook> inmatePrebooks = _context.InmatePrebook
                        .Where(w => (!w.DeleteFlag.HasValue || w.DeleteFlag == 0) && w.CompleteFlag == 1
                            && (!w.TemporaryHold.HasValue || w.TemporaryHold == 0) && w.FacilityId == value.FacilityId);

                    monitorPreScreen = value.Flag switch
                    {
                        StatusBoardOverviewConstants.READYFORMEDICALPRESCREEN => inmatePrebooks
                            .Where(w => (!w.CourtCommitFlag.HasValue || w.CourtCommitFlag == 0) &&
                                (!w.IncarcerationId.HasValue || w.IncarcerationId == 0) &&
                                (!w.MedPrescreenStartFlag.HasValue || w.MedPrescreenStartFlag == 0) &&
                                (!w.MedPrescreenStatusFlag.HasValue || w.MedPrescreenStatusFlag == 0) &&
                                (w.PersonId.HasValue || !w.PersonId.HasValue))
                            .Select(s => new MonitorPreScreenVm
                            {
                                PersonId = s.PersonId,
                                PersonLastName = s.PersonLastName,
                                PersonFirstName = s.PersonFirstName,
                                PersonDob = s.PersonId.HasValue ? s.Person.PersonDob : s.PersonDob,
                                ArrestDate = s.ArrestDate,
                                PrebookDate = s.PrebookDate,
                                PhotoFilePath = _photos.GetPhotoByCollectionIdentifier(s.Person.Identifiers)
                            })
                            .ToList(),
                        StatusBoardOverviewConstants.COURTCOMMITSCHEDULED => inmatePrebooks
                            .Where(c => c.CourtCommitFlag == 1 && !c.MedPrescreenStartFlag.HasValue &&
                                (!c.MedPrescreenStatusFlag.HasValue || c.MedPrescreenStatusFlag == 0) &&
                                c.PrebookDate.HasValue && (!c.IncarcerationId.HasValue || c.IncarcerationId == 0) &&
                                c.PrebookDate.Value.Date >= DateTime.Now.Date &&
                                (c.PersonId.HasValue || !c.PersonId.HasValue))
                            .Select(s => new MonitorPreScreenVm
                            {
                                PersonId = s.PersonId,
                                PersonLastName = s.PersonLastName,
                                PersonFirstName = s.PersonFirstName,
                                PersonDob = s.PersonId.HasValue ? s.Person.PersonDob : s.PersonDob,
                                ArrestDate = s.ArrestDate,
                                PrebookDate = s.PrebookDate,
                                PhotoFilePath = _photos.GetPhotoByCollectionIdentifier(s.Person.Identifiers)
                            })
                            .ToList(),
                        StatusBoardOverviewConstants.COURTCOMMITOVERDUE => inmatePrebooks
                            .Where(c => c.CourtCommitFlag == 1 && !c.MedPrescreenStartFlag.HasValue &&
                                (!c.MedPrescreenStatusFlag.HasValue || c.MedPrescreenStatusFlag == 0) &&
                                c.PrebookDate.HasValue && (!c.IncarcerationId.HasValue || c.IncarcerationId == 0) &&
                                c.PrebookDate.Value.Date < DateTime.Now.Date &&
                                (c.PersonId.HasValue || !c.PersonId.HasValue))
                            .Select(s => new MonitorPreScreenVm
                            {
                                PersonId = s.PersonId,
                                PersonLastName = s.PersonLastName,
                                PersonFirstName = s.PersonFirstName,
                                PersonDob = s.PersonId.HasValue ? s.Person.PersonDob : s.PersonDob,
                                ArrestDate = s.ArrestDate,
                                PrebookDate = s.PrebookDate,
                                PhotoFilePath = _photos.GetPhotoByCollectionIdentifier(s.Person.Identifiers)
                            })
                            .ToList(),
                        StatusBoardOverviewConstants.MEDICALLYREJECTEDLAST48HOURS => inmatePrebooks
                            .Where(c => c.MedPrescreenStartFlag == 1 && c.MedPrescreenStatusFlag == -1 &&
                                c.MedPrescreenStatusDate.HasValue &&
                                (!c.IncarcerationId.HasValue || c.IncarcerationId == 0) &&
                                (!c.TempHoldId.HasValue || c.TempHoldId == 0) &&
                                c.MedPrescreenStatusDate.Value.Date.AddDays(-2) < c.MedPrescreenStatusDate.Value.Date &&
                                c.MedPrescreenStatusDate.Value.Date <= DateTime.Now.Date &&
                                (c.PersonId.HasValue || !c.PersonId.HasValue))
                            .Select(s => new MonitorPreScreenVm
                            {
                                PersonId = s.PersonId,
                                PersonLastName = s.PersonLastName,
                                PersonFirstName = s.PersonFirstName,
                                PersonDob = s.PersonId.HasValue ? s.Person.PersonDob : s.PersonDob,
                                ArrestDate = s.ArrestDate,
                                PrebookDate = s.PrebookDate,
                                PhotoFilePath = _photos.GetPhotoByCollectionIdentifier(s.Person.Identifiers)
                            })
                            .ToList(),
                        _ => monitorPreScreen
                    };
                    break;
                case OverviewType.MedPreScreenInProgress:
                    IQueryable<InmatePrebook> inmatePrebooks2 = _context.InmatePrebook
                        .Where(w => (!w.DeleteFlag.HasValue || w.DeleteFlag == 0)
                            && w.CompleteFlag == 1 && w.MedPrescreenStartFlag == 1
                            && w.FacilityId == value.FacilityId);
                    monitorPreScreen = value.Flag switch
                    {
                        StatusBoardOverviewConstants.MEDICALPRESCREENINPROGRESS => inmatePrebooks2
                            .Where(c => (!c.MedPrescreenStatusFlag.HasValue || c.MedPrescreenStatusFlag == 0) &&
                                !c.IncarcerationId.HasValue && (!c.TemporaryHold.HasValue || c.TemporaryHold == 0) &&
                                (c.PersonId.HasValue || !c.PersonId.HasValue))
                            .OrderByDescending(o => o.PrebookDate)
                            .Select(s => new MonitorPreScreenVm
                            {
                                PersonId = s.Person.Inmate.Select(se => se.InmateId).FirstOrDefault(),
                                PersonLastName = s.PersonLastName,
                                PersonFirstName = s.PersonFirstName,
                                PersonDob = s.PersonId.HasValue ? s.Person.PersonDob : s.PersonDob,
                                ArrestDate = s.ArrestDate,
                                PrebookDate = s.PrebookDate,
                                PhotoFilePath = _photos.GetPhotoByCollectionIdentifier(s.Person.Identifiers)
                            })
                            .ToList(),
                        StatusBoardOverviewConstants.BYPASSEDMEDICALPRESCREEN => inmatePrebooks2
                            .Where(c => c.MedPrescreenStatusFlag == 2 && (c.PersonId.HasValue || !c.PersonId.HasValue))
                            .OrderByDescending(o => o.PrebookDate)
                            .Select(s => new MonitorPreScreenVm
                            {
                                PersonId = s.Person.Inmate.Select(se => se.InmateId).FirstOrDefault(),
                                PersonLastName = s.PersonLastName,
                                PersonFirstName = s.PersonFirstName,
                                PersonDob = s.PersonId.HasValue ? s.Person.PersonDob : s.PersonDob,
                                ArrestDate = s.ArrestDate,
                                PrebookDate = s.PrebookDate
                            })
                            .ToList(),
                        _ => monitorPreScreen
                    };
                    break;
                case OverviewType.MedPrescreenComplete:
                    IQueryable<InmatePrebook> inmatePrebooks3 = _context.InmatePrebook
                        .Where(w => (!w.DeleteFlag.HasValue || w.DeleteFlag == 0) && w.CompleteFlag == 1
                            && !w.IncarcerationId.HasValue && (!w.TempHoldId.HasValue || w.TempHoldId == 0)
                            && w.MedPrescreenStatusFlag > 0 && w.FacilityId == value.FacilityId);
                    monitorPreScreen = value.Flag switch
                    {
                        StatusBoardOverviewConstants.READYFORINTAKE => inmatePrebooks3
                            .Where(c => !c.TemporaryHold.HasValue ||
                                c.TemporaryHold == 0 && (c.PersonId.HasValue || !c.PersonId.HasValue))
                            .OrderByDescending(o => o.PrebookDate)
                            .Select(s => new MonitorPreScreenVm
                            {
                                PersonId = s.PersonId,
                                PersonLastName = s.PersonLastName,
                                PersonFirstName = s.PersonFirstName,
                                PersonDob = s.PersonId.HasValue ? s.Person.PersonDob : s.PersonDob,
                                ArrestDate = s.ArrestDate,
                                PrebookDate = s.PrebookDate,
                                PhotoFilePath = _photos.GetPhotoByCollectionIdentifier(s.Person.Identifiers)
                            })
                            .ToList(),
                        StatusBoardOverviewConstants.READYFORTEMPHOLD => inmatePrebooks3
                            .Where(c => c.TemporaryHold == 1 && (c.PersonId.HasValue || !c.PersonId.HasValue))
                            .OrderByDescending(o => o.PrebookDate)
                            .Select(s => new MonitorPreScreenVm
                            {
                                PersonId = s.PersonId,
                                PersonLastName = s.PersonLastName,
                                PersonFirstName = s.PersonFirstName,
                                PersonDob = s.PersonId.HasValue ? s.Person.PersonDob : s.PersonDob,
                                ArrestDate = s.ArrestDate,
                                PrebookDate = s.PrebookDate,
                                PhotoFilePath = _photos.GetPhotoByCollectionIdentifier(s.Person.Identifiers)
                            })
                            .ToList(),
                        _ => monitorPreScreen
                    };
                    break;
            }

            return monitorPreScreen;
        }

        public List<HousingStatsDetails> GetInmateDetail(OverviewDetailInputVm value)
        {
            List<HousingStatsDetails> monitorPreScreen = new List<HousingStatsDetails>();

            IQueryable<Inmate> inmate = _context.Inmate.Where(w => w.InmateActive == 1 && w.FacilityId == value.FacilityId);

            IQueryable<Incarceration> incarcerations = _context.Incarceration
                .Where(w => !w.ReleaseOut.HasValue && w.Inmate.FacilityId == value.FacilityId
                   && w.Inmate.InmateActive == 1);
            switch (value.OverviewType)
            {
                case OverviewType.WizardQueue:
                    if (value.Flag == StatusBoardOverviewConstants.INTAKE)
                    {
                        monitorPreScreen = inmate.Where(c => c.Incarceration.Any(a =>
                                !a.IntakeCompleteFlag.HasValue && !a.BookAndReleaseFlag.HasValue && !a.ReleaseOut.HasValue))
                            .Select(s => new HousingStatsDetails
                            {
                                InmateId = s.InmateId,
                                Location = s.InmateCurrentTrack,
                                HousingUnitId = s.HousingUnitId,
                                PersonId = s.PersonId,
                                InmateNumber = s.InmateNumber,
                                PersonDetail = new PersonInfoVm
                                {
                                    PersonId = s.PersonId,
                                    PersonLastName = s.Person.PersonLastName,
                                    PersonMiddleName = s.Person.PersonMiddleName,
                                    PersonFirstName = s.Person.PersonFirstName
                                },
                                HousingDetail = new HousingDetail
                                {
                                    HousingUnitLocation = s.HousingUnitId > 0 ? s.HousingUnit.HousingUnitLocation : null,
                                    HousingUnitNumber = s.HousingUnitId > 0 ? s.HousingUnit.HousingUnitNumber : null,
                                    HousingUnitBedNumber = s.HousingUnitId > 0 ? s.HousingUnit.HousingUnitBedNumber : null,
                                    HousingUnitBedLocation = s.HousingUnitId > 0 ? s.HousingUnit.HousingUnitBedLocation : null
                                },
                                DateValue = s.Incarceration.Select(f => f.DateIn).FirstOrDefault(),
                                PhotoFilePath = _photos.GetPhotoByCollectionIdentifier(s.Person.Identifiers)
                            }).OrderBy(o => o.DateValue).ToList();
                    }
                    else if (value.Flag == StatusBoardOverviewConstants.BOOKING)
                    {
                        monitorPreScreen = incarcerations.Where(c => c.IntakeCompleteFlag == 1 && !c.BookCompleteFlag.HasValue
                        || c.BookAndReleaseFlag == 1).OrderBy(o => o.DateIn)
                            .Select(s => new HousingStatsDetails
                            {
                                InmateId = s.InmateId ?? 0,
                                Location = s.Inmate.InmateCurrentTrack,
                                HousingUnitId = s.Inmate.HousingUnitId,
                                PersonId = s.Inmate.PersonId,
                                InmateNumber = s.Inmate.InmateNumber,
                                PersonDetail = new PersonInfoVm
                                {
                                    PersonId = s.Inmate.PersonId,
                                    PersonLastName = s.Inmate.Person.PersonLastName,
                                    PersonMiddleName = s.Inmate.Person.PersonMiddleName,
                                    PersonFirstName = s.Inmate.Person.PersonFirstName
                                },
                                HousingDetail = new HousingDetail
                                {
                                    HousingUnitLocation = s.Inmate.HousingUnitId > 0 ? s.Inmate.HousingUnit.HousingUnitLocation : null,
                                    HousingUnitNumber = s.Inmate.HousingUnitId > 0 ? s.Inmate.HousingUnit.HousingUnitNumber : null,
                                    HousingUnitBedNumber = s.Inmate.HousingUnitId > 0 ? s.Inmate.HousingUnit.HousingUnitBedNumber : null,
                                    HousingUnitBedLocation = s.Inmate.HousingUnitId > 0 ? s.Inmate.HousingUnit.HousingUnitBedLocation : null
                                },
                                PhotoFilePath = _photos.GetPhotoByCollectionIdentifier(s.Inmate.Person.Identifiers)

                            }).ToList();
                    }
                    else if (value.Flag == StatusBoardOverviewConstants.RELEASE)
                    {
                        monitorPreScreen = incarcerations.Where(c => c.OverallFinalReleaseDate <= DateTime.Now &&
                                c.OverallFinalReleaseDate.HasValue ||
                        c.ReleaseClearFlag == 1 || c.BookAndReleaseFlag == 1)
                            .Select(s => new HousingStatsDetails
                            {
                                InmateId = s.InmateId ?? 0,
                                Location = s.Inmate.InmateCurrentTrack,
                                HousingUnitId = s.Inmate.HousingUnitId,
                                PersonId = s.Inmate.PersonId,
                                InmateNumber = s.Inmate.InmateNumber,
                                PersonDetail = new PersonInfoVm
                                {
                                    PersonId = s.Inmate.PersonId,
                                    PersonLastName = s.Inmate.Person.PersonLastName,
                                    PersonMiddleName = s.Inmate.Person.PersonMiddleName,
                                    PersonFirstName = s.Inmate.Person.PersonFirstName
                                },
                                HousingDetail = new HousingDetail
                                {
                                    HousingUnitLocation = s.Inmate.HousingUnitId > 0 ? s.Inmate.HousingUnit.HousingUnitLocation : null,
                                    HousingUnitNumber = s.Inmate.HousingUnitId > 0 ? s.Inmate.HousingUnit.HousingUnitNumber : null,
                                    HousingUnitBedNumber = s.Inmate.HousingUnitId > 0 ? s.Inmate.HousingUnit.HousingUnitBedNumber : null,
                                    HousingUnitBedLocation = s.Inmate.HousingUnitId > 0 ? s.Inmate.HousingUnit.HousingUnitBedLocation : null
                                },
                                PhotoFilePath = _photos.GetPhotoByCollectionIdentifier(s.Inmate.Person.Identifiers)

                            }).OrderBy(o => o.PersonDetail.PersonLastName).ThenBy(t => t.PersonDetail.PersonFirstName).ToList();
                    }
                    else if (value.Flag == StatusBoardOverviewConstants.BOOKANDRELEASE)
                    {
                        monitorPreScreen = incarcerations.Where(c => c.BookAndReleaseFlag == 1)
                            .Select(s => new HousingStatsDetails
                            {
                                InmateId = s.InmateId ?? 0,
                                Location = s.Inmate.InmateCurrentTrack,
                                HousingUnitId = s.Inmate.HousingUnitId,
                                PersonId = s.Inmate.PersonId,
                                InmateNumber = s.Inmate.InmateNumber,
                                PersonDetail = new PersonInfoVm
                                {
                                    PersonId = s.Inmate.PersonId,
                                    PersonLastName = s.Inmate.Person.PersonLastName,
                                    PersonMiddleName = s.Inmate.Person.PersonMiddleName,
                                    PersonFirstName = s.Inmate.Person.PersonFirstName
                                },
                                HousingDetail = new HousingDetail
                                {
                                    HousingUnitLocation = s.Inmate.HousingUnitId > 0 ? s.Inmate.HousingUnit.HousingUnitLocation : null,
                                    HousingUnitNumber = s.Inmate.HousingUnitId > 0 ? s.Inmate.HousingUnit.HousingUnitNumber : null,
                                    HousingUnitBedNumber = s.Inmate.HousingUnitId > 0 ? s.Inmate.HousingUnit.HousingUnitBedNumber : null,
                                    HousingUnitBedLocation = s.Inmate.HousingUnitId > 0 ? s.Inmate.HousingUnit.HousingUnitBedLocation : null
                                },
                                PhotoFilePath = _photos.GetPhotoByCollectionIdentifier(s.Inmate.Person.Identifiers)

                            }).OrderBy(o => o.PersonDetail.PersonLastName).ThenBy(t => t.PersonDetail.PersonFirstName).ToList();
                    }
                    break;
                case OverviewType.Task:
                    monitorPreScreen = _context.AoTaskQueue.Where(w => !w.CompleteFlag && !w.AoTaskLookup.DeleteFlag &&
                            w.Inmate.InmateActive == 1
                            && w.AoTaskLookup.TaskName == value.Flag
                            && w.Inmate.FacilityId == value.FacilityId)
                        .Select(s => new HousingStatsDetails
                        {
                            InmateId = s.InmateId,
                            Location = s.Inmate.InmateCurrentTrack,
                            HousingUnitId = s.Inmate.HousingUnitId,
                            PersonId = s.Inmate.PersonId,
                            InmateNumber = s.Inmate.InmateNumber,
                            PersonDetail = new PersonInfoVm
                            {
                                PersonId = s.Inmate.PersonId,
                                PersonLastName = s.Inmate.Person.PersonLastName,
                                PersonMiddleName = s.Inmate.Person.PersonMiddleName,
                                PersonFirstName = s.Inmate.Person.PersonFirstName
                            },
                            HousingDetail = new HousingDetail
                            {
                                HousingUnitLocation = s.Inmate.HousingUnitId > 0
                                    ? s.Inmate.HousingUnit.HousingUnitLocation : null,
                                HousingUnitNumber = s.Inmate.HousingUnitId > 0 ? s.Inmate.HousingUnit.HousingUnitNumber
                                    : null,
                                HousingUnitBedNumber = s.Inmate.HousingUnitId > 0
                                    ? s.Inmate.HousingUnit.HousingUnitBedNumber : null,
                                HousingUnitBedLocation = s.Inmate.HousingUnitId > 0
                                    ? s.Inmate.HousingUnit.HousingUnitBedLocation : null
                            },
                            PhotoFilePath = _photos.GetPhotoByCollectionIdentifier(s.Inmate.Person.Identifiers)

                        }).OrderBy(o => o.PersonDetail.PersonLastName).ThenBy(t => t.PersonDetail.PersonFirstName)
                        .ToList();
                    break;
                case OverviewType.IncidentInmates:
                    IQueryable<Lookup> lookup = _context.Lookup.Where(w =>
                        w.LookupType == LookupConstants.DISCTYPE || w.LookupType == LookupConstants.DISCINTYPE);

                    int discTypeIndex = lookup.First(w =>
                            w.LookupType == LookupConstants.DISCTYPE && w.LookupDescription == value.Description)
                        .LookupIndex;

                    int discInTypeIndex = lookup.First(w =>
                            w.LookupType == LookupConstants.DISCINTYPE && w.LookupDescription == value.Flag)
                        .LookupIndex;

                    monitorPreScreen = _context.DisciplinaryInmate
                    .Where(inc => inc.DisciplinaryIncident.DisciplinaryActive == 1
                            && inc.DisciplinaryIncident.FacilityId == value.FacilityId
                            && !inc.DeleteFlag.HasValue
                            && inc.DisciplinaryInmateType > 0
                            && inc.DisciplinaryIncident.DisciplinaryType == discTypeIndex
                            && inc.DisciplinaryInmateType == discInTypeIndex)
                        .Select(inc => new HousingStatsDetails
                        {
                            InmateId = inc.InmateId ?? 0,
                            Location = inc.InmateId != null ? inc.Inmate.InmateCurrentTrack : null,
                            HousingUnitId = inc.InmateId != null ? inc.Inmate.HousingUnitId : 0,
                            PersonId = inc.InmateId != null ? inc.Inmate.PersonId : 0,
                            InmateNumber = inc.InmateId != null ? inc.Inmate.InmateNumber : null,
                            PersonDetail = new PersonInfoVm
                            {
                                PersonId = inc.InmateId > 0 ? inc.Inmate.PersonId : inc.PersonnelId > 0 ? inc.Personnel.PersonId : default,
                                PersonLastName = inc.InmateId > 0 ? inc.Inmate.Person.PersonLastName :
                                inc.PersonnelId > 0 ? inc.Personnel.PersonNavigation.PersonLastName : default,
                                PersonMiddleName = inc.InmateId > 0 ? inc.Inmate.Person.PersonMiddleName :
                                inc.PersonnelId > 0 ? inc.Personnel.PersonNavigation.PersonMiddleName : default,
                                PersonFirstName = inc.InmateId > 0 ? inc.Inmate.Person.PersonFirstName :
                                inc.PersonnelId > 0 ? inc.Personnel.PersonNavigation.PersonFirstName : default
                            },
                            HousingDetail = new HousingDetail
                            {
                                HousingUnitLocation = inc.Inmate.HousingUnitId > 0 ? inc.Inmate.HousingUnit.HousingUnitLocation : null,
                                HousingUnitNumber = inc.Inmate.HousingUnitId > 0 ? inc.Inmate.HousingUnit.HousingUnitNumber : null,
                                HousingUnitBedNumber = inc.Inmate.HousingUnitId > 0 ? inc.Inmate.HousingUnit.HousingUnitBedNumber : null,
                                HousingUnitBedLocation = inc.Inmate.HousingUnitId > 0 ? inc.Inmate.HousingUnit.HousingUnitBedLocation : null
                            },
                            PhotoFilePath = inc.InmateId != null ? _photos.GetPhotoByCollectionIdentifier(inc.Inmate.Person.Identifiers) : null,
                            DisciplinaryOtherName = inc.DisciplinaryOtherName
                        }).OrderBy(o => o.PersonDetail != null ? o.PersonDetail.PersonLastName : null)
                        .ThenBy(t => t.PersonDetail != null ? t.PersonDetail.PersonFirstName : null).ToList();
                    break;
                case OverviewType.GrievanceInmates:
                    monitorPreScreen = _context.Grievance
                        .Where(gr => gr.GrievanceType == value.FlagId && gr.DeleteFlag == 0 &&
                            gr.FacilityId == value.FacilityId && !gr.SetReview.HasValue ||
                            gr.GrievanceType == value.FlagId && gr.DeleteFlag == 0
                            && gr.FacilityId == value.FacilityId
                            && !gr.SetReview.HasValue && gr.GrievanceInmate.Any())
                        .Select(gr => new HousingStatsDetails
                        {
                            InmateId = gr.InmateId,
                            Location = gr.Inmate.InmateCurrentTrack,
                            HousingUnitId = gr.Inmate.HousingUnitId,
                            PersonId = gr.Inmate.PersonId,
                            InmateNumber = gr.Inmate.InmateNumber,
                            PersonDetail = new PersonInfoVm
                            {
                                PersonId = gr.Inmate.PersonId,
                                PersonLastName = gr.Inmate.Person.PersonLastName,
                                PersonMiddleName = gr.Inmate.Person.PersonMiddleName,
                                PersonFirstName = gr.Inmate.Person.PersonFirstName
                            },
                            HousingDetail = new HousingDetail
                            {
                                HousingUnitLocation = gr.Inmate.HousingUnitId > 0
                                    ? gr.Inmate.HousingUnit.HousingUnitLocation : null,
                                HousingUnitNumber = gr.Inmate.HousingUnitId > 0
                                    ? gr.Inmate.HousingUnit.HousingUnitNumber : null,
                                HousingUnitBedNumber = gr.Inmate.HousingUnitId > 0
                                    ? gr.Inmate.HousingUnit.HousingUnitBedNumber : null,
                                HousingUnitBedLocation = gr.Inmate.HousingUnitId > 0
                                    ? gr.Inmate.HousingUnit.HousingUnitBedLocation : null
                            },
                            PhotoFilePath = _photos.GetPhotoByCollectionIdentifier(gr.Inmate.Person.Identifiers)

                        }).OrderBy(o => o.PersonDetail.PersonLastName).ThenBy(t => t.PersonDetail.PersonFirstName)
                        .ToList();
                    break;
                case OverviewType.Facility:
                    monitorPreScreen = _context.Inmate.Where(w => w.InmateActive == 1 && w.FacilityId == value.FlagId)
                        .Select(s => new HousingStatsDetails
                        {
                            InmateId = s.InmateId,
                            Location = s.InmateCurrentTrack,
                            HousingUnitId = s.HousingUnitId,
                            PersonId = s.PersonId,
                            InmateNumber = s.InmateNumber,
                            PersonDetail = new PersonInfoVm
                            {
                                PersonId = s.PersonId,
                                PersonLastName = s.Person.PersonLastName,
                                PersonMiddleName = s.Person.PersonMiddleName,
                                PersonFirstName = s.Person.PersonFirstName
                            },
                            HousingDetail = new HousingDetail
                            {
                                HousingUnitLocation = s.HousingUnitId > 0 ? s.HousingUnit.HousingUnitLocation : null,
                                HousingUnitNumber = s.HousingUnitId > 0 ? s.HousingUnit.HousingUnitNumber : null,
                                HousingUnitBedNumber = s.HousingUnitId > 0 ? s.HousingUnit.HousingUnitBedNumber : null,
                                HousingUnitBedLocation = s.HousingUnitId > 0 ? s.HousingUnit.HousingUnitBedLocation : null
                            },
                            PhotoFilePath = _photos.GetPhotoByCollectionIdentifier(s.Person.Identifiers)

                        }).OrderBy(o => o.PersonDetail.PersonLastName).ThenBy(t => t.PersonDetail.PersonFirstName).ToList();
                    break;
                case OverviewType.Housing:
                    //TODO we shouldn't be passing "null" string for Flag
                    inmate = value.Flag == "null" ? inmate.Where(w => !w.HousingUnitId.HasValue)
                        : inmate.Where(w => w.HousingUnit.HousingUnitLocation == value.Flag);

                    monitorPreScreen = inmate
                        .Select(s => new HousingStatsDetails
                        {
                            InmateId = s.InmateId,
                            Location = s.InmateCurrentTrack,
                            HousingUnitId = s.HousingUnitId,
                            PersonId = s.PersonId,
                            InmateNumber = s.InmateNumber,
                            PersonDetail = new PersonInfoVm
                            {
                                PersonId = s.PersonId,
                                PersonLastName = s.Person.PersonLastName,
                                PersonMiddleName = s.Person.PersonMiddleName,
                                PersonFirstName = s.Person.PersonFirstName
                            },
                            HousingDetail = new HousingDetail
                            {
                                HousingUnitLocation = s.HousingUnitId > 0 ? s.HousingUnit.HousingUnitLocation : null,
                                HousingUnitNumber = s.HousingUnitId > 0 ? s.HousingUnit.HousingUnitNumber : null,
                                HousingUnitBedNumber = s.HousingUnitId > 0 ? s.HousingUnit.HousingUnitBedNumber : null,
                                HousingUnitBedLocation =
                                    s.HousingUnitId > 0 ? s.HousingUnit.HousingUnitBedLocation : null
                            },
                            PhotoFilePath = _photos.GetPhotoByCollectionIdentifier(s.Person.Identifiers)

                        }).OrderBy(o => o.PersonDetail.PersonLastName).ThenBy(t => t.PersonDetail.PersonFirstName)
                        .ToList();
                    break;
                case OverviewType.ExternalLocation:
                    monitorPreScreen = _context.Inmate.Where(w => w.InmateCurrentTrackId == value.FlagId &&
                            !w.InmateCurrentTrackNavigation.FacilityId.HasValue && w.InmateActive == 1)
                        .Select(s => new HousingStatsDetails
                        {
                            InmateId = s.InmateId,
                            Location = s.InmateCurrentTrack,
                            HousingUnitId = s.HousingUnitId,
                            PersonId = s.PersonId,
                            InmateNumber = s.InmateNumber,
                            PersonDetail = new PersonInfoVm
                            {
                                PersonId = s.PersonId,
                                PersonLastName = s.Person.PersonLastName,
                                PersonMiddleName = s.Person.PersonMiddleName,
                                PersonFirstName = s.Person.PersonFirstName
                            },
                            HousingDetail = new HousingDetail
                            {
                                HousingUnitLocation = s.HousingUnitId > 0 ? s.HousingUnit.HousingUnitLocation : null,
                                HousingUnitNumber = s.HousingUnitId > 0 ? s.HousingUnit.HousingUnitNumber : null,
                                HousingUnitBedNumber = s.HousingUnitId > 0 ? s.HousingUnit.HousingUnitBedNumber : null,
                                HousingUnitBedLocation =
                                    s.HousingUnitId > 0 ? s.HousingUnit.HousingUnitBedLocation : null
                            },
                            PhotoFilePath = _photos.GetPhotoByCollectionIdentifier(s.Person.Identifiers)

                        }).OrderBy(o => o.PersonDetail.PersonLastName).ThenBy(t => t.PersonDetail.PersonFirstName)
                        .ToList();
                    break;
                case OverviewType.Location:
                    monitorPreScreen = inmate
                        .Where(w => w.InmateCurrentTrackId == value.FlagId)
                        .Select(s => new HousingStatsDetails
                        {
                            InmateId = s.InmateId,
                            Location = s.InmateCurrentTrack,
                            HousingUnitId = s.HousingUnitId,
                            PersonId = s.PersonId,
                            InmateNumber = s.InmateNumber,
                            PersonDetail = new PersonInfoVm
                            {
                                PersonId = s.PersonId,
                                PersonLastName = s.Person.PersonLastName,
                                PersonMiddleName = s.Person.PersonMiddleName,
                                PersonFirstName = s.Person.PersonFirstName
                            },
                            HousingDetail = new HousingDetail
                            {
                                HousingUnitLocation = s.HousingUnitId > 0 ? s.HousingUnit.HousingUnitLocation : null,
                                HousingUnitNumber = s.HousingUnitId > 0 ? s.HousingUnit.HousingUnitNumber : null,
                                HousingUnitBedNumber = s.HousingUnitId > 0 ? s.HousingUnit.HousingUnitBedNumber : null,
                                HousingUnitBedLocation = s.HousingUnitId > 0 ? s.HousingUnit.HousingUnitBedLocation : null
                            },
                            PhotoFilePath = _photos.GetPhotoByCollectionIdentifier(s.Person.Identifiers)

                        }).OrderBy(o => o.PersonDetail.PersonLastName).ThenBy(t => t.PersonDetail.PersonFirstName).ToList();
                    break;
                case OverviewType.SupervisorQueues:
                    monitorPreScreen = value.Flag switch
                    {
                        StatusBoardOverviewConstants.SUPERVISORREVIEWOVERALL => incarcerations
                            .Where(c => c.BookCompleteFlag == 1 && !c.BookingSupervisorCompleteFlag.HasValue)
                            .Select(s => new HousingStatsDetails
                            {
                                InmateId = s.Inmate.InmateId,
                                Location = s.Inmate.InmateCurrentTrack,
                                HousingUnitId = s.Inmate.HousingUnitId,
                                PersonId = s.Inmate.PersonId,
                                InmateNumber = s.Inmate.InmateNumber,
                                PersonDetail =
                                    new PersonInfoVm
                                    {
                                        PersonId = s.Inmate.PersonId,
                                        PersonLastName = s.Inmate.Person.PersonLastName,
                                        PersonMiddleName = s.Inmate.Person.PersonMiddleName,
                                        PersonFirstName = s.Inmate.Person.PersonFirstName
                                    },
                                HousingDetail = new HousingDetail
                                {
                                    HousingUnitLocation = s.Inmate.HousingUnitId > 0 ? 
                                        s.Inmate.HousingUnit.HousingUnitLocation : null,
                                    HousingUnitNumber = s.Inmate.HousingUnitId > 0 ? 
                                        s.Inmate.HousingUnit.HousingUnitNumber : null,
                                    HousingUnitBedNumber = s.Inmate.HousingUnitId > 0 
                                        ? s.Inmate.HousingUnit.HousingUnitBedNumber : null,
                                    HousingUnitBedLocation = s.Inmate.HousingUnitId > 0 ? 
                                        s.Inmate.HousingUnit.HousingUnitBedLocation : null
                                },
                                DateValue = s.OverallFinalReleaseDate,
                                Classify = s.Inmate.InmateClassification.InmateClassificationReason,
                                PhotoFilePath = _photos.GetPhotoByCollectionIdentifier(s.Inmate.Person.Identifiers)
                            })
                            .OrderBy(o => o.PersonDetail.PersonLastName)
                            .ThenBy(t => t.PersonDetail.PersonFirstName)
                            .ToList(),
                        StatusBoardOverviewConstants.REVIEWRELEASE => incarcerations
                            .Where(c => c.ReleaseClearFlag == 1 && (!c.ReleaseSupervisorCompleteFlag.HasValue ||
                                c.ReleaseSupervisorCompleteFlag == 0))
                            .Select(s => new HousingStatsDetails
                            {
                                InmateId = s.Inmate.InmateId,
                                Location = s.Inmate.InmateCurrentTrack,
                                HousingUnitId = s.Inmate.HousingUnitId,
                                PersonId = s.Inmate.PersonId,
                                InmateNumber = s.Inmate.InmateNumber,
                                PersonDetail =
                                    new PersonInfoVm
                                    {
                                        PersonId = s.Inmate.PersonId,
                                        PersonLastName = s.Inmate.Person.PersonLastName,
                                        PersonMiddleName = s.Inmate.Person.PersonMiddleName,
                                        PersonFirstName = s.Inmate.Person.PersonFirstName
                                    },
                                HousingDetail = new HousingDetail
                                {
                                    HousingUnitLocation =
                                        s.Inmate.HousingUnitId > 0 ? s.Inmate.HousingUnit.HousingUnitLocation
                                            : null,
                                    HousingUnitNumber =
                                        s.Inmate.HousingUnitId > 0 ? s.Inmate.HousingUnit.HousingUnitNumber
                                            : null,
                                    HousingUnitBedNumber =
                                        s.Inmate.HousingUnitId > 0 ? s.Inmate.HousingUnit.HousingUnitBedNumber
                                            : null,
                                    HousingUnitBedLocation = s.Inmate.HousingUnitId > 0
                                        ? s.Inmate.HousingUnit.HousingUnitBedLocation : null
                                },
                                DateValue = s.OverallFinalReleaseDate,
                                Classify = s.Inmate.InmateClassification.InmateClassificationReason,
                                PhotoFilePath = _photos.GetPhotoByCollectionIdentifier(s.Inmate.Person.Identifiers)
                            })
                            .OrderBy(o => o.PersonDetail.PersonLastName)
                            .ThenBy(t => t.PersonDetail.PersonFirstName)
                            .ToList(),
                        _ => monitorPreScreen
                    };
                    break;
                case OverviewType.ArrestingAgency:
                    monitorPreScreen = _context.Arrest.Where(w => w.Inmate.InmateActive == 1 &&
                            w.Inmate.FacilityId == value.FacilityId
                            && w.ArrestActive == 1 && w.ArrestingAgencyId > 0 && w.ArrestingAgencyId == value.FlagId)
                        .Select(s => new HousingStatsDetails
                        {
                            InmateId = s.Inmate.InmateId,
                            Location = s.Inmate.InmateCurrentTrack,
                            HousingUnitId = s.Inmate.HousingUnitId,
                            PersonId = s.Inmate.PersonId,
                            InmateNumber = s.ArrestBookingNo,
                            PersonDetail = new PersonInfoVm
                            {
                                PersonId = s.Inmate.PersonId,
                                PersonLastName = s.Inmate.Person.PersonLastName,
                                PersonMiddleName = s.Inmate.Person.PersonMiddleName,
                                PersonFirstName = s.Inmate.Person.PersonFirstName
                            },
                            HousingDetail = new HousingDetail
                            {
                                HousingUnitLocation = s.Inmate.HousingUnitId > 0
                                    ? s.Inmate.HousingUnit.HousingUnitLocation : null,
                                HousingUnitNumber = s.Inmate.HousingUnitId > 0 ? s.Inmate.HousingUnit.HousingUnitNumber
                                    : null,
                                HousingUnitBedNumber = s.Inmate.HousingUnitId > 0
                                    ? s.Inmate.HousingUnit.HousingUnitBedNumber : null,
                                HousingUnitBedLocation = s.Inmate.HousingUnitId > 0
                                    ? s.Inmate.HousingUnit.HousingUnitBedLocation : null
                            },
                            PhotoFilePath = _photos.GetPhotoByCollectionIdentifier(s.Inmate.Person.Identifiers)
                        }).OrderBy(o => o.PersonDetail.PersonLastName).ThenBy(t => t.PersonDetail.PersonFirstName)
                        .ToList();
                    break;
                case OverviewType.InOut:
                    IQueryable<Incarceration> incarcerations1 = _context.Incarceration
                        .Where(w => w.Inmate.FacilityId == value.FacilityId);

                    monitorPreScreen = value.Flag switch
                    {
                        StatusBoardOverviewConstants.BOOKEDTODAY => incarcerations1
                            .Where(w => w.DateIn.Value.Date == DateTime.Now.Date)
                            .Select(s => new HousingStatsDetails
                            {
                                InmateId = s.Inmate.InmateId,
                                Location = s.Inmate.InmateCurrentTrack,
                                HousingUnitId = s.Inmate.HousingUnitId,
                                PersonId = s.Inmate.PersonId,
                                InmateNumber = s.Inmate.InmateNumber,
                                PersonDetail = new PersonInfoVm
                                    {
                                        PersonId = s.Inmate.PersonId,
                                        PersonLastName = s.Inmate.Person.PersonLastName,
                                        PersonMiddleName = s.Inmate.Person.PersonMiddleName,
                                        PersonFirstName = s.Inmate.Person.PersonFirstName
                                    },
                                HousingDetail = new HousingDetail
                                {
                                    HousingUnitLocation = s.Inmate.HousingUnitId > 0 ? 
                                        s.Inmate.HousingUnit.HousingUnitLocation : null,
                                    HousingUnitNumber = s.Inmate.HousingUnitId > 0 ? 
                                        s.Inmate.HousingUnit.HousingUnitNumber : null,
                                    HousingUnitBedNumber = s.Inmate.HousingUnitId > 0 ? 
                                        s.Inmate.HousingUnit.HousingUnitBedNumber : null,
                                    HousingUnitBedLocation = s.Inmate.HousingUnitId > 0 ? 
                                        s.Inmate.HousingUnit.HousingUnitBedLocation : null
                                },
                                PhotoFilePath = _photos.GetPhotoByCollectionIdentifier(s.Inmate.Person.Identifiers)
                            })
                            .OrderBy(o => o.PersonDetail.PersonLastName)
                            .ThenBy(t => t.PersonDetail.PersonFirstName)
                            .ToList(),
                        StatusBoardOverviewConstants.RELEASEDTODAY => incarcerations1
                            .Where(w => w.ReleaseOut.Value.Date == DateTime.Now.Date)
                            .Select(s => new HousingStatsDetails
                            {
                                InmateId = s.Inmate.InmateId,
                                Location = s.Inmate.InmateCurrentTrack,
                                HousingUnitId = s.Inmate.HousingUnitId,
                                PersonId = s.Inmate.PersonId,
                                InmateNumber = s.Inmate.InmateNumber,
                                PersonDetail =
                                    new PersonInfoVm
                                    {
                                        PersonId = s.Inmate.PersonId,
                                        PersonLastName = s.Inmate.Person.PersonLastName,
                                        PersonMiddleName = s.Inmate.Person.PersonMiddleName,
                                        PersonFirstName = s.Inmate.Person.PersonFirstName
                                    },
                                HousingDetail = new HousingDetail
                                {
                                    HousingUnitLocation = s.Inmate.HousingUnitId > 0 ? 
                                        s.Inmate.HousingUnit.HousingUnitLocation : null,
                                    HousingUnitNumber = s.Inmate.HousingUnitId > 0 ? 
                                        s.Inmate.HousingUnit.HousingUnitNumber : null,
                                    HousingUnitBedNumber = s.Inmate.HousingUnitId > 0 ? 
                                        s.Inmate.HousingUnit.HousingUnitBedNumber : null,
                                    HousingUnitBedLocation = s.Inmate.HousingUnitId > 0 ? 
                                        s.Inmate.HousingUnit.HousingUnitBedLocation : null
                                },
                                PhotoFilePath = _photos.GetPhotoByCollectionIdentifier(s.Inmate.Person.Identifiers)
                            })
                            .OrderBy(o => o.PersonDetail.PersonLastName)
                            .ThenBy(t => t.PersonDetail.PersonFirstName)
                            .ToList(),
                        _ => monitorPreScreen
                    };

                    break;
                case OverviewType.Sentence:
                    IQueryable<Incarceration> incarcerations4 = _context.Incarceration
                        .Where(w => w.Inmate.FacilityId == value.FacilityId && w.Inmate.InmateActive == 1);

                    monitorPreScreen = value.Flag switch
                    {
                        StatusBoardOverviewConstants.SENTENCED => incarcerations4
                            .Where(w => w.OverallSentStartDate.HasValue && w.OverallSentStartDate <= DateTime.Now &&
                                !w.ReleaseOut.HasValue)
                            .Select(s => new HousingStatsDetails
                            {
                                InmateId = s.Inmate.InmateId,
                                Location = s.Inmate.InmateCurrentTrack,
                                HousingUnitId = s.Inmate.HousingUnitId,
                                PersonId = s.Inmate.PersonId,
                                InmateNumber = s.Inmate.InmateNumber,
                                PersonDetail = new PersonInfoVm
                                    {
                                        PersonId = s.Inmate.PersonId,
                                        PersonLastName = s.Inmate.Person.PersonLastName,
                                        PersonMiddleName = s.Inmate.Person.PersonMiddleName,
                                        PersonFirstName = s.Inmate.Person.PersonFirstName
                                    },
                                HousingDetail = new HousingDetail
                                {
                                    HousingUnitLocation = s.Inmate.HousingUnitId > 0 ? s.Inmate.HousingUnit.HousingUnitLocation : null,
                                    HousingUnitNumber = s.Inmate.HousingUnitId > 0 ? s.Inmate.HousingUnit.HousingUnitNumber : null,
                                    HousingUnitBedNumber = s.Inmate.HousingUnitId > 0 ? s.Inmate.HousingUnit.HousingUnitBedNumber : null,
                                    HousingUnitBedLocation = s.Inmate.HousingUnitId > 0 ? s.Inmate.HousingUnit.HousingUnitBedLocation : null
                                },
                                PhotoFilePath = _photos.GetPhotoByCollectionIdentifier(s.Inmate.Person.Identifiers)
                            })
                            .OrderBy(o => o.PersonDetail.PersonLastName)
                            .ThenBy(t => t.PersonDetail.PersonFirstName)
                            .ToList(),

                        StatusBoardOverviewConstants.UNSENTENCED => incarcerations4
                            .Where(w => (!w.OverallSentStartDate.HasValue || w.OverallSentStartDate > DateTime.Now) &&
                                !w.ReleaseOut.HasValue)
                            .Select(s => new HousingStatsDetails
                            {
                                InmateId = s.Inmate.InmateId,
                                Location = s.Inmate.InmateCurrentTrack,
                                HousingUnitId = s.Inmate.HousingUnitId,
                                PersonId = s.Inmate.PersonId,
                                InmateNumber = s.Inmate.InmateNumber,
                                PersonDetail = new PersonInfoVm
                                    {
                                        PersonId = s.Inmate.PersonId,
                                        PersonLastName = s.Inmate.Person.PersonLastName,
                                        PersonMiddleName = s.Inmate.Person.PersonMiddleName,
                                        PersonFirstName = s.Inmate.Person.PersonFirstName
                                    },
                                HousingDetail = new HousingDetail
                                {
                                    HousingUnitLocation = s.Inmate.HousingUnitId > 0 ? s.Inmate.HousingUnit.HousingUnitLocation : null,
                                    HousingUnitNumber = s.Inmate.HousingUnitId > 0 ? s.Inmate.HousingUnit.HousingUnitNumber : null,
                                    HousingUnitBedNumber = s.Inmate.HousingUnitId > 0 ? s.Inmate.HousingUnit.HousingUnitBedNumber : null,
                                    HousingUnitBedLocation = s.Inmate.HousingUnitId > 0 ? s.Inmate.HousingUnit.HousingUnitBedLocation : null
                                },
                                PhotoFilePath = _photos.GetPhotoByCollectionIdentifier(s.Inmate.Person.Identifiers)
                            })
                            .OrderBy(o => o.PersonDetail.PersonLastName)
                            .ThenBy(t => t.PersonDetail.PersonFirstName)
                            .ToList(),
                        _ => monitorPreScreen
                    };
                    break;
                case OverviewType.Charge:
                    List<int> crime = _context.Crime.Where(c => c.CrimeLookupId > 0 && c.CrimeDeleteFlag == 0
                            && !c.WarrantId.HasValue && c.Arrest.IncarcerationArrestXref.Any(a =>
                                !a.Incarceration.ReleaseOut.HasValue &&
                                a.Incarceration.Inmate.FacilityId == value.FacilityId
                                && a.Incarceration.Inmate.InmateActive == 1) && c.CrimeLookupId == value.FlagId)
                        .Select(s => s.Arrest.InmateId ?? 0).ToList();

                    crime.AddRange(_context.CrimeForce.Where(
                            c => c.DeleteFlag == 0 && !c.DropChargeFlag.HasValue && !c.ForceCrimeLookupId.HasValue &&
                                !c.WarrantId.HasValue && !c.ForceSupervisorReviewFlag.HasValue
                                && c.Arrest.IncarcerationArrestXref.Any(a => !a.Incarceration.ReleaseOut.HasValue &&
                                    a.Incarceration.Inmate.FacilityId == value.FacilityId
                                    && a.Incarceration.Inmate.InmateActive == 1) &&
                                c.ForceCrimeLookupId == value.FlagId)
                        .Select(s => s.Arrest.InmateId ?? 0));

                    monitorPreScreen=_context.Inmate.Where(w=>w.FacilityId==value.FacilityId && w.InmateActive==1
                    && crime.Contains(w.InmateId))
                    .Select(s => new HousingStatsDetails
                     {
                         InmateId = s.InmateId,
                         Location = s.InmateCurrentTrack,
                         HousingUnitId = s.HousingUnitId,
                         PersonId = s.PersonId,
                         InmateNumber = s.InmateNumber,
                         PersonDetail = new PersonInfoVm
                         {
                             PersonId = s.PersonId,
                             PersonLastName = s.Person.PersonLastName,
                             PersonMiddleName = s.Person.PersonMiddleName,
                             PersonFirstName = s.Person.PersonFirstName
                         },
                         HousingDetail = new HousingDetail
                         {
                             HousingUnitLocation = s.HousingUnitId > 0 ? s.HousingUnit.HousingUnitLocation : null,
                             HousingUnitNumber = s.HousingUnitId > 0 ? s.HousingUnit.HousingUnitNumber : null,
                             HousingUnitBedNumber = s.HousingUnitId > 0 ? s.HousingUnit.HousingUnitBedNumber : null,
                             HousingUnitBedLocation = s.HousingUnitId > 0 ? s.HousingUnit.HousingUnitBedLocation : null
                         },
                         PhotoFilePath = _photos.GetPhotoByCollectionIdentifier(s.Person.Identifiers)
                     }).ToList();                   
                    break;
                case OverviewType.TodaysAppointment:

                    DateTime startDate = DateTime.Now.Date;
                    DateTime endDate = DateTime.Now.Date.AddHours(23).AddMinutes(59).AddSeconds(59);

                    CalendarInputs inputs = new CalendarInputs
                    {
                        CalendarTypes = "1,2,3",
                        FromDate = startDate,
                        ToDate = endDate,
                        FacilityId = value.FacilityId,
                        LocationId = value.FlagId
                    };

                    monitorPreScreen = _appointmentViewerService.AppointmentViewer(inputs)
                        .Select(s => new HousingStatsDetails
                        {
                            InmateId = s.InmateId ?? 0,
                            Location = s.Location,
                            PersonId = s.InmateDetails.PersonId,
                            InmateNumber = s.InmateDetails.InmateNumber,
                            PersonDetail = new PersonInfoVm
                            {
                                PersonId = s.InmateDetails.PersonId,
                                PersonLastName = s.InmateDetails.PersonLastName,
                                PersonMiddleName = s.InmateDetails.PersonMiddleName,
                                PersonFirstName = s.InmateDetails.PersonFirstName
                            },
                            HousingDetail = new HousingDetail
                            {
                                HousingUnitLocation = s.InmateDetails.HousingLocation,
                                HousingUnitNumber = s.InmateDetails.HousingNumber,
                                HousingUnitBedNumber = s.InmateDetails.HousingUnitBedNumber,
                                HousingUnitBedLocation = s.InmateDetails.HousingUnitBedLocation
                            },
                            PhotoFilePath = s.InmateDetails.PhotoPath

                        }).OrderBy(o => o.PersonDetail.PersonLastName).ThenBy(t => t.PersonDetail.PersonFirstName)
                        .ToList();
                    break;
            }
            return monitorPreScreen;
        }

        public List<MonitorGrivanceVm> GetGrivanceDetail(OverviewDetailInputVm value) =>
            value.OverviewType switch
            {
                OverviewType.Grievance => _context.Grievance.SelectMany(
                        gr => _context.Lookup.Where(w =>
                            w.LookupIndex == gr.GrievanceType && w.LookupType == LookupConstants.GRIEVTYPE &&
                            gr.DeleteFlag == 0 && gr.FacilityId == value.FacilityId && !gr.SetReview.HasValue &&
                            gr.GrievanceType > 0 && w.LookupDescription == value.Flag),
                        (gr, look) => new MonitorGrivanceVm
                        {
                            GrivanceType = look.LookupDescription,
                            GrivanceDate = gr.DateOccured,
                            Department = gr.Department,
                            ReviewDate = gr.ReviewedDate
                        })
                    .ToList(),
                OverviewType.GrievancePersonnel => _context.GrievancePersonnel
                    .Where(gr =>
                        value.FlagId == gr.Grievance.GrievanceType && gr.Grievance.DeleteFlag == 0 &&
                        gr.Grievance.FacilityId == value.FacilityId && !gr.Grievance.SetReview.HasValue)
                    .Select(gr => new MonitorGrivanceVm
                    {
                        PersonLastName = gr.Personnel.PersonNavigation.PersonLastName,
                        PersonFirstName = gr.Personnel.PersonNavigation.PersonFirstName,
                        Number = gr.Personnel.OfficerBadgeNum,
                        Department = gr.Personnel.PersonnelDepartment,
                        Position = gr.Personnel.PersonnelPosition
                    })
                    .ToList(),
                OverviewType.Attendance => _context.AttendanceAo
                    .Where(w => w.PersonnelId > 0 && w.FacilityId == value.FacilityId && !w.ClockOutEnter.HasValue &&
                        w.ClockInEnter.Value.Date == DateTime.Now.Date &&
                        w.Personnel.PersonnelDepartment == value.Flag &&
                        w.Personnel.PersonnelPosition == value.Description)
                    .Select(s => new MonitorGrivanceVm
                    {
                        PersonLastName = s.Personnel.PersonNavigation.PersonLastName,
                        PersonFirstName = s.Personnel.PersonNavigation.PersonFirstName,
                        Number = s.Personnel.OfficerBadgeNum,
                        Department = s.Personnel.PersonnelDepartment,
                        Position = s.Personnel.PersonnelPosition
                    })
                    .OrderBy(o => o.PersonLastName)
                    .ThenBy(t => t.PersonFirstName)
                    .ToList(),
                _ => new List<MonitorGrivanceVm>()
            };

        public List<IncidentDescription> GetIncidentDetail(OverviewDetailInputVm value)
        {
            List<Lookup> lookup = _context.Lookup.Where(w => w.LookupType == LookupConstants.DISCTYPE && w.LookupIndex == value.FlagId)
                .ToList();

            List<IncidentDescription> monitorPreScreen = _context.DisciplinaryIncident
                .Where(inc => inc.DisciplinaryType == value.FlagId
                && inc.DisciplinaryActive == 1
                 && inc.FacilityId == value.FacilityId
                ).Select(inc => new IncidentDescription
                {
                    DisciplinaryIncidentDate = inc.DisciplinaryReportDate,
                    DisciplinaryNumber = inc.DisciplinaryNumber,
                    IncidentType = lookup.First(f => f.LookupIndex == inc.DisciplinaryType).LookupDescription,
                    DisciplinarySynopsis = inc.DisciplinarySynopsis
                })
                .ToList();

            return monitorPreScreen;
        }

        public List<SupervisiorReviewVm> GetSupervisorReviewDetail(OverviewDetailInputVm value)
        {
            List<SupervisiorReviewVm> supervisiorReview = new List<SupervisiorReviewVm>();

            IQueryable<IncarcerationArrestXref> incarcerations = _context.IncarcerationArrestXref
                .Where(w => !w.Incarceration.ReleaseOut.HasValue && w.Incarceration.Inmate.FacilityId == value.FacilityId
                   && w.Incarceration.Inmate.InmateActive == 1);

            List<Lookup> lookups = _context.Lookup.Where(w => w.LookupType == LookupConstants.ARRTYPE).ToList();

            supervisiorReview = value.Flag switch
            {
                StatusBoardOverviewConstants.SUPERVISORREVIEWBOOKING => incarcerations
                    .Where(c => c.Incarceration.BookCompleteFlag == 1 && !c.BookingSupervisorCompleteFlag.HasValue &&
                        c.Arrest.BookingCompleteFlag == 1)
                    .Select(s => new SupervisiorReviewVm
                    {
                        BookingNumber = s.Arrest.ArrestBookingNo,
                        InmateNumber = s.Incarceration.Inmate.InmateNumber,
                        PersonDetail = new PersonInfoVm
                        {
                            PersonId = s.Incarceration.Inmate.PersonId,
                            PersonLastName = s.Incarceration.Inmate.Person.PersonLastName,
                            PersonMiddleName = s.Incarceration.Inmate.Person.PersonMiddleName,
                            PersonFirstName = s.Incarceration.Inmate.Person.PersonFirstName
                        },
                        BookingType =
                            lookups.FirstOrDefault(f => f.LookupBinary == Convert.ToDouble(s.Arrest.ArrestType))
                                .LookupDescription,
                        CompleteDate = s.Incarceration.BookCompleteDate
                    })
                    .OrderBy(o => o.PersonDetail.PersonLastName)
                    .ThenBy(t => t.PersonDetail.PersonFirstName)
                    .ToList(),
                StatusBoardOverviewConstants.REVIEWCLEAR => incarcerations
                    .Where(c => c.ReleaseDate.HasValue && !c.ReleaseSupervisorCompleteFlag.HasValue)
                    .Select(s => new SupervisiorReviewVm
                    {
                        BookingNumber = s.Arrest.ArrestBookingNo,
                        InmateNumber = s.Incarceration.Inmate.InmateNumber,
                        PersonDetail = new PersonInfoVm
                        {
                            PersonId = s.Incarceration.Inmate.PersonId,
                            PersonLastName = s.Incarceration.Inmate.Person.PersonLastName,
                            PersonMiddleName = s.Incarceration.Inmate.Person.PersonMiddleName,
                            PersonFirstName = s.Incarceration.Inmate.Person.PersonFirstName
                        },
                        BookingType =
                            lookups.FirstOrDefault(f => f.LookupBinary == Convert.ToDouble(s.Arrest.ArrestType))
                                .LookupDescription,
                        CompleteDate = s.Incarceration.ReleaseClearDate
                    })
                    .OrderBy(o => o.PersonDetail.PersonLastName)
                    .ThenBy(t => t.PersonDetail.PersonFirstName)
                    .ToList(),
                _ => supervisiorReview
            };
            return supervisiorReview;
        }
    }
}
