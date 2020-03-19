using GenerateTables.Models;
using System;

// ReSharper disable once CheckNamespace
namespace ServerAPI.Tests
{
    public partial class DbInitialize
    {
        private void ObservationDetails()
        {
            Db.ObservationSchedule.AddRange(
                new ObservationSchedule
                {
                    ObservationScheduleId = 5,
                    ObservationType = 9,
                    Note = "SUICIDE ATTEMPT",
                    InmateId = 100,
                    StartDate = DateTime.Now.AddDays(-1),
                    EndDate = DateTime.Now.AddDays(1)
                },
                new ObservationSchedule
                {
                    ObservationScheduleId = 6,
                    ObservationType = 9,
                    Note = "FOR MEDICAL CHECKUP",
                    InmateId = 101,
                    StartDate = DateTime.Now,
                    EndDate = DateTime.Now.AddDays(1)
                },
                new ObservationSchedule
                {
                    ObservationScheduleId = 7,
                    ObservationType = 9,
                    Note = "NOT FIT",
                    InmateId = 102,
                    StartDate = DateTime.Now,
                    EndDate = null
                },
                new ObservationSchedule
                {
                    ObservationScheduleId = 8,
                    ObservationType = 8,
                    Note = null,
                    InmateId = 110,
                    StartBy = 12,
                    StartDate = DateTime.Now,
                    EndDate = DateTime.Now
                },
                new ObservationSchedule
                {
                    ObservationScheduleId = 9,
                    ObservationType = 8,
                    Note = null,
                    InmateId = 110,
                    StartBy = 12,
                    StartDate = DateTime.Now,
                    EndDate = DateTime.Now
                },
                new ObservationSchedule
                {
                    ObservationScheduleId = 10,
                    ObservationType = 9,
                    Note = "RECHECK THE MEDICAL REPORT",
                    InmateId = 120,
                    DeleteFlag = 0,
                    StartBy = 11,
                    StartDate = DateTime.Now.AddDays(-1),
                    EndDate = DateTime.Now.AddDays(1)
                },
                new ObservationSchedule
                {
                    ObservationScheduleId = 11,
                    ObservationType = 9,
                    Note = "INACTIVE",
                    InmateId = 120,
                    DeleteFlag = 0,
                    StartBy = 11,
                    StartDate = DateTime.Now.AddDays(-3),
                    EndDate = DateTime.Now.AddDays(2)
                },
                new ObservationSchedule
                {
                    ObservationScheduleId = 12,
                    ObservationType = 9,
                    Note = "REPORT",
                    InmateId = 120,
                    DeleteFlag = 1,
                    StartBy = 11,
                    StartDate = DateTime.Now,
                    EndDate =null
                }
               );

            Db.ObservationPolicy.AddRange(
                new ObservationPolicy
                {
                    ObservationPolicyId = 15,
                    CreateDate = DateTime.Now.AddDays(-15),
                    UpdateDate = DateTime.Now,
                    DeleteFlag = 0,
                    CreateBy = 11,
                    UpdateBy = 12,
                    DeleteDate = null,
                    DeleteBy = null,
                    ObservationType = 9,
                    ObservationActionId = 10
                },
                new ObservationPolicy
                {
                    ObservationPolicyId = 16,
                    CreateDate = DateTime.Now.AddDays(-15),
                    UpdateDate = DateTime.Now,
                    DeleteFlag = 0,
                    CreateBy = 11,
                    UpdateBy = 12,
                    DeleteDate = null,
                    DeleteBy = null,
                    ObservationType = 5,
                    ObservationActionId = 10
                }
                );


            Db.ObservationAction.AddRange(
                new ObservationAction
                {
                    ObservationActionId = 10,
                    DeleteDate = null,
                    CreateBy = 11,
                    DeleteBy = null,
                    CreateDate = DateTime.Now.AddDays(-5),
                    UpdateBy = 11,
                    DeleteFlag = 0,
                    UpdateDate = DateTime.Now.AddDays(-2),
                    ActionName = "HEALTH CHECK",
                    ActionAbbr = "HE"
                },
                new ObservationAction
                {
                    ObservationActionId = 11,
                    DeleteDate = null,
                    CreateBy = 12,
                    DeleteBy = null,
                    CreateDate = DateTime.Now.AddDays(-5),
                    UpdateBy = 11,
                    DeleteFlag = 0,
                    UpdateDate = DateTime.Now.AddDays(-2),
                    ActionName = "WATER CHECK",
                    ActionAbbr = "WAT"
                }

                );
            Db.ObservationScheduleAction.AddRange(
                new ObservationScheduleAction
                {
                    ObservationScheduleActionId = 10,
                    CreateBy = 11,
                    UpdateBy = 11,
                    CreateDate = DateTime.Now.AddDays(-7),
                    UpdateDate = DateTime.Now.AddDays(-3),
                    CreateByNavigation = null,
                    DeleteBy = 11,
                    DeleteFlag = 1,
                    LastReminderEntryBy = 11,
                    ObservationScheduleNote = "SCHEDULE NOTE",
                    DeleteDate = null,
                    ObservationScheduleId = 6,
                    ObservationScheduleInterval = 120,
                    LastReminderEntry = DateTime.Now,
                    ObservationActionId = 10,
                    ObservationLateEntryMax = 20
                },
                 new ObservationScheduleAction
                 {
                     ObservationScheduleActionId = 11,
                     CreateBy = 12,
                     UpdateBy = 11,
                     CreateDate = DateTime.Now.AddDays(-5),
                     UpdateDate = DateTime.Now.AddDays(-3),
                     CreateByNavigation = null,
                     DeleteBy = null,
                     DeleteFlag = 0,
                     LastReminderEntryBy = 12,
                     ObservationScheduleNote = "SCHEDULE NOTE",
                     DeleteDate = null,
                     ObservationScheduleId = 5,
                     ObservationActionId = 10,
                     LastReminderEntry = DateTime.Now.AddDays(-1),
                     ObservationLateEntryMax = 15
                 },
                new ObservationScheduleAction
                {
                    ObservationScheduleActionId = 12,
                    CreateBy = 12,
                    UpdateBy = 11,
                    CreateDate = DateTime.Now.AddDays(-5),
                    UpdateDate = DateTime.Now.AddDays(-3),
                    CreateByNavigation = null,
                    DeleteBy = null,
                    DeleteFlag = 0,
                    LastReminderEntryBy = 12,
                    ObservationScheduleNote = "NEW SCHEDULE",
                    DeleteDate = null,
                    ObservationScheduleId = 11,
                    ObservationActionId = 10,
                    LastReminderEntry = DateTime.Now.AddDays(-1),
                    ObservationLateEntryMax = 15
                },
                new ObservationScheduleAction
                {
                    ObservationScheduleActionId = 13,
                    CreateBy = 12,
                    UpdateBy = 11,
                    CreateDate = DateTime.Now.AddDays(-5),
                    UpdateDate = DateTime.Now.AddDays(-3),
                    CreateByNavigation = null,
                    DeleteBy = null,
                    DeleteFlag = 0,
                    LastReminderEntryBy = 12,
                    ObservationScheduleNote = "OBSERVATION LOG",
                    DeleteDate = null,
                    ObservationScheduleId = 12,
                    ObservationActionId = 10,
                    LastReminderEntry = DateTime.Now.AddDays(-1),
                    ObservationLateEntryMax = 15
                }
                );

            Db.ObservationScheduleHistory.AddRange(
                new ObservationScheduleHistory
                {
                    ObservationScheduleHistoryId = 10,
                    CreateDate = DateTime.Now.AddDays(-15),
                    PersonnelId = 12,
                    ObservationScheduleHistoryList = @"{ 'Inmate':'SALVETTI, SALVETTI JOSEPH AUX597', 'Type':'MENTAL HEALTH', 'Start Date':'2018-07-16T13:30:00', 'End Date':null, 'Note':'TWO RESET' }",
                    ObservationScheduleId = 10
                },
                new ObservationScheduleHistory
                {
                    ObservationScheduleHistoryId = 11,
                    CreateDate = DateTime.Now.AddDays(-15),
                    PersonnelId = 11,
                    ObservationScheduleHistoryList = @"{'Inmate':'ALEXANDER, ALEXANDER ULY927','Type':'MEDICAL','Start Date':'09/10/2018 14:12'}",
                    ObservationScheduleId = 5
                },
                new ObservationScheduleHistory
                {
                    ObservationScheduleHistoryId = 12,
                    CreateDate = DateTime.Now.AddDays(-15),
                    PersonnelId = 11,
                    ObservationScheduleHistoryList = @"{ 'Inmate':'SALVETTI, SALVETTI JOSEPH AUX597', 'Type':'MENTAL HEALTH', 'Start Date':'2018-07-16T13:30:00', 'End Date':null, 'Note':'TWO RESET' }",
                    ObservationScheduleId = 10
                }


                );

            Db.ObservationLog.AddRange(
                new ObservationLog
                {
                    ObservationLogId = 7,
                    ObservationScheduleActionId = 10,
                    CreateBy = 12,
                    CreateDate = DateTime.Now.AddDays(-6),
                    UpdateBy = 11,
                    UpdateDate = DateTime.Now,
                    HousingUnitId = 10
                },
                new ObservationLog
                {
                    ObservationLogId = 8,
                    ObservationScheduleActionId = 10,
                    CreateBy = 11,
                    CreateDate = DateTime.Now.AddDays(-3),
                    UpdateBy = 11,
                    UpdateDate = DateTime.Now,
                    HousingUnitId = 10
                },
                new ObservationLog
                {
                    ObservationLogId = 9,
                    ObservationScheduleActionId = 13,
                    CreateBy = 11,
                    CreateDate = DateTime.Now.AddDays(-3),
                    UpdateBy = 11,
                    UpdateDate = DateTime.Now,
                    HousingUnitId = 10
                }
                );
        }
    }
}
