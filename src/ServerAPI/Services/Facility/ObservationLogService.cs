
using GenerateTables.Models;
using Microsoft.AspNetCore.Http;
using ServerAPI.Utilities;
using ServerAPI.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServerAPI.Services
{
    public class ObservationLogService : IObservationLogService
    {
        private readonly AAtims _context;      
        private readonly int _personnelId;

        public ObservationLogService(AAtims context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _personnelId = Convert.ToInt32(httpContextAccessor.HttpContext.User.FindFirst("personnelId")?.Value);
        }

        //refresh the observation Log list
        public ObservationLogsVm GetObservationLogList(ConsoleInputVm value)
        {   
            if (!(value.ListConsoleHousingId is null))
            {
                ObservationLogsVm lstLoadObservationHousingList = LoadObservationLogList(value);
                return lstLoadObservationHousingList;
            }
            else
            {
                ObservationLogsVm lstLoadObservationLocationList = LoadObservationLogList(value);
                return lstLoadObservationLocationList;
            }
        }

        //observation log list
        public ObservationLogsVm LoadObservationLogList(ConsoleInputVm value)
        {
            List<Lookup> lstLookup = _context.Lookup
                .Where(lk => lk.LookupType == LookupConstants.OBSTYPE).ToList();

            // main grid
            IQueryable<ObservationLogDetails> observationschedule = _context.ObservationSchedule
                .Where(ob => ob.DeleteFlag == 0
                             && value.FacilityId == ob.Inmate.FacilityId
                             && (ob.StartDate <= DateTime.Now) && (ob.EndDate >= DateTime.Now || !ob.EndDate.HasValue)
                             && (value.ListConsoleHousingId.Contains(
                                     ob.Inmate.HousingUnit.HousingUnitListId)
                                 || value.ListConsoleLocationId.Contains(ob.Inmate.InmateCurrentTrackId ?? 0)
                                 || value.ListConsoleMyLocationId.Contains(ob.Inmate.InmateCurrentTrackId ?? 0)))
                .Select(ob => new ObservationLogDetails
                {
                    ObservationScheduleId = ob.ObservationScheduleId,
                    InmateId = ob.InmateId,
                    ObservationStartDate = ob.StartDate,
                    ObservationEndDate = ob.EndDate,
                    ObservationNotes = ob.Note,
                    ObservationTypeId = ob.ObservationType,
                    ObservationType = lstLookup.Single(l => l.LookupIndex == ob.ObservationType).LookupDescription,
                    InmateCurrentTrack = ob.Inmate.InmateCurrentTrack,
                    PersonDetail = new PersonInfoVm
                    {
                        PersonLastName = ob.Inmate.Person.PersonLastName,
                        PersonFirstName = ob.Inmate.Person.PersonFirstName
                    },
                    HousingDetail = new HousingDetail
                    {
                        HousingUnitLocation = !string.IsNullOrEmpty(ob.Inmate.HousingUnit.HousingUnitLocation)
                            ? ob.Inmate.HousingUnit.HousingUnitLocation
                            : HousingConstants.NOHOUSING,
                        HousingUnitNumber = ob.Inmate.HousingUnit.HousingUnitNumber,
                        HousingUnitBedNumber = ob.Inmate.HousingUnit.HousingUnitBedNumber,
                        HousingUnitBedLocation = ob.Inmate.HousingUnit.HousingUnitBedLocation
                    },
                    LookupDetails = lstLookup.Where(lk => lk.LookupType == LookupConstants.OBSTYPE
                                                          && lk.LookupInactive == 0 &&
                                                          lk.LookupIndex == ob.ObservationType)
                        .OrderByDescending(o => o.LookupIndex)
                        .Select(s => new KeyValuePair<int, string>(s.LookupIndex, s.LookupDescription)).ToList()
                });

            int[] lstObservationIds = observationschedule.Select(s => s.ObservationScheduleId ?? 0).ToArray();

            //sub grid
            List<ObservationLogItemDetails> lstObservationScheduleAction = _context.ObservationScheduleAction.Where(
                    os => os.DeleteFlag == 0
                          && lstObservationIds.Contains(os.ObservationScheduleId))
                .Select(s => new ObservationLogItemDetails
                {
                    ObservationScheduleActionId = s.ObservationScheduleActionId,
                    ObservationActionName = s.ObservationAction.ActionName,
                    ObservationActionId = s.ObservationActionId,
                    ObservationScheduleNote = s.ObservationSchedule.Note, // schedule notes
                    ObservationActionNote = s.ObservationScheduleNote, // action notes
                    ObservationScheduleInterval = s.ObservationScheduleInterval,
                    ObservationScheduleId = s.ObservationScheduleId,
                    LastReminderEntry = s.LastReminderEntry,
                    LastReminderEntryBy = s.LastReminderEntryBy,
                    ObservationLateEntryMax = s.ObservationLateEntryMax,
                    InmateId = s.ObservationSchedule.InmateId,
                    HousingUnitId = s.ObservationSchedule.Inmate.HousingUnitId
                }).ToList();

            ObservationLogsVm listObservationLogs = new ObservationLogsVm
            {
                ObservationLogDetails = observationschedule,
                ObservationLogItemDetails = lstObservationScheduleAction
            };

            return listObservationLogs;
        }

        //load observation history list
        public List<ObservationLogItemDetails> LoadObservationHistoryList(int observationActionId, int inmateId)
        {
            List<Lookup> lstLookup = _context.Lookup
                .Where(lk => lk.LookupType == LookupConstants.OBSTYPE).ToList();

            List<ObservationLogItemDetails> loadObservationHistoryList = _context.ObservationLog
                .Where(ol => ol.ObservationScheduleActionId == observationActionId
                             && ol.ObservationScheduleAction.ObservationSchedule.InmateId == inmateId)
                .Select(ol => new ObservationLogItemDetails
                {
                    ObservationLogId = ol.ObservationLogId,
                    ObservationScheduleId = ol.ObservationScheduleActionId ?? 0,
                    ObservationDate = ol.ObservationDateTime,
                    ObservationActionName = ol.ObservationScheduleAction.ObservationAction.ActionAbbr,
                    ObservationLateEntryFlag = ol.ObservationLateEntryFlag == 1,
                    CreatedDate = ol.CreateDate,
                    LastReminderEntryBy = ol.ObservationScheduleAction.LastReminderEntryBy,
                    PersonDetails = new PersonnelVm
                    {
                        PersonLastName = ol.CreateByNavigation.PersonNavigation.PersonLastName,
                        PersonFirstName = ol.CreateByNavigation.PersonNavigation.PersonFirstName,
                        OfficerBadgeNumber = ol.CreateByNavigation.OfficerBadgeNum
                    },
                    ObservationTypeId = ol.ObservationScheduleAction.ObservationSchedule.ObservationType,
                    ObservationScheduleNote = ol.ObservationScheduleAction.ObservationScheduleNote, //schedule note     
                    ObservationActionNote = ol.ObservationScheduleAction.ObservationSchedule.Note, //action note
                    ObservationScheduleActionId = ol.ObservationScheduleActionId,
                    ObservationScheduleInterval = ol.ObservationScheduleAction.ObservationScheduleInterval,
                    LastReminderEntry = ol.ObservationScheduleAction.LastReminderEntry.Value.Date,
                    ObservationLateEntryMax = ol.ObservationScheduleAction.ObservationLateEntryMax,
                    InmateId = ol.ObservationScheduleAction.ObservationSchedule.InmateId,
                    PersonDetail = new PersonInfoVm
                    {
                        PersonLastName = ol.ObservationScheduleAction.ObservationSchedule.Inmate.Person.PersonLastName,
                        PersonFirstName =
                            ol.ObservationScheduleAction.ObservationSchedule.Inmate.Person.PersonFirstName,
                        PersonMiddleName =
                            ol.ObservationScheduleAction.ObservationSchedule.Inmate.Person.PersonMiddleName,
                        InmateNumber = ol.ObservationScheduleAction.ObservationSchedule.Inmate.InmateNumber
                    },
                    ObservationNotes=ol.ObservationNote
                }).OrderByDescending(o=>o.ObservationDate).ToList();
            loadObservationHistoryList.ForEach(item =>
            {
                item.ObservationType = lstLookup.Single(l => (int?)(l.LookupIndex) == item.ObservationTypeId)
                    .LookupDescription;
            });

            return loadObservationHistoryList;

        }

        //Observation entry
        public ObservationScheduleVm LoadObservationScheduleEntry(int observationScheduleId)
        {
            ObservationScheduleVm observationschedule = _context.ObservationSchedule
                .Where(ob => ob.ObservationScheduleId == observationScheduleId)
                .Select(ob => new ObservationScheduleVm
                {
                    ObservationScheduleId = ob.ObservationScheduleId,
                    InmateId = ob.InmateId,
                    ObservationType = ob.ObservationType ?? 0,
                    StartDate = ob.StartDate,
                    EndDate = ob.EndDate,
                    Note = ob.Note
                }).Single();

            return observationschedule;
        }

        //Observation action entry
        public ObservationScheduleActionVm LoadObservationActionEntry(int observationScheduleActionId)
        {
            ObservationScheduleActionVm observationschedule = _context.ObservationScheduleAction
                .Where(ol => ol.ObservationScheduleActionId == observationScheduleActionId)
                .Select(ol => new ObservationScheduleActionVm
                {
                    ObservationScheduleInterval = ol.ObservationScheduleInterval,
                    ObservationActionId = ol.ObservationActionId,
                    ObservationLateEntryMax = ol.ObservationLateEntryMax,
                    ObservationScheduleNote = ol.ObservationScheduleNote,
                    ObservationScheduleActionId=ol.ObservationScheduleActionId,
                    ActionList = _context.ObservationAction.Where(w => w.ObservationActionId == ol.ObservationActionId)
                        .Select(s => new KeyValuePair<int, string>(s.ObservationActionId, s.ActionName)).ToList()
                }).SingleOrDefault();

            return observationschedule;
        }

        //insert observation log
        public async Task<int> InsertObservationLog(ObservationLogItemDetails logItem)
        {
            //insert the observation logs details
            ObservationLog observationLog = new ObservationLog
            {
                ObservationScheduleActionId = logItem.ObservationScheduleActionId,
                HousingUnitId = logItem.HousingUnitId,
                ObservationDateTime = logItem.ObservationDate,
                ObservationLateEntryFlag = logItem.ObservationLateEntryFlag ? 1 : 0,
                ObservationNote = logItem.ObservationNotes,
                CreateDate = DateTime.Now,
                CreateBy = _personnelId
            };

            _context.ObservationLog.Add(observationLog);

            //update 
            ObservationScheduleAction observationScheduleAction = _context.ObservationScheduleAction
                .SingleOrDefault(c => c.ObservationScheduleActionId == logItem.ObservationScheduleActionId);
            if (!(observationScheduleAction is null))
            {
                observationScheduleAction.LastReminderEntry = logItem.ObservationDate;
                observationScheduleAction.LastReminderEntryBy = _personnelId;
            }

            return await _context.SaveChangesAsync();
        }
    }
}
