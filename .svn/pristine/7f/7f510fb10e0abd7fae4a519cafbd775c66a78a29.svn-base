using GenerateTables.Models;
using System;

// ReSharper disable once CheckNamespace
namespace ServerAPI.Tests
{
    public partial class DbInitialize
    {
        private void Workcrewvalues(WorkCrew values)
        {
            values.FurloughSchdTueStart = new TimeSpan(17, 15, 6);
            values.FurloughSchdMonStart = new TimeSpan(5, 15, 2);
            values.FurloughSchdWedStart = new TimeSpan(21, 0, 0);
            values.FurloughSchdThuStart = new TimeSpan(10, 15, 0);
            values.FurloughSchdFriStart = new TimeSpan(13, 30, 0);
            values.FurloughSchdSunStart = new TimeSpan(16, 15, 0);
            values.FurloughSchdSatStart = new TimeSpan(17, 45, 0);
            values.FurloughSchdSunEnd = new TimeSpan(18, 45, 0);
            values.FurloughSchdMonEnd = new TimeSpan(11, 15, 0);
            values.FurloughSchdFriEnd = new TimeSpan(18, 30, 0);
            values.FurloughSchdTueEnd = new TimeSpan(22, 15, 0);
            values.FurloughSchdWedEnd = new TimeSpan(23, 45, 0);
            values.FurloughSchdThuEnd = new TimeSpan(14, 30, 0);
            values.FurloughSchdSatEnd = new TimeSpan(22, 0, 0);
        }
        private void Crewlookupvalues(WorkCrewLookup values)
        {
            values.CrewSchdTueStart = new TimeSpan(14, 15, 6);
            values.CrewSchdMonStart = new TimeSpan(5, 15, 2);
            values.CrewSchdWedStart = new TimeSpan(22, 0, 0);
            values.CrewSchdThuStart = new TimeSpan(11, 15, 0);
            values.CrewSchdFriStart = new TimeSpan(13, 30, 0);
            values.CrewSchdSunStart = new TimeSpan(15, 15, 0);
            values.CrewSchdSatStart = new TimeSpan(20, 45, 0);
            values.CrewSchdSunEnd = new TimeSpan(19, 25, 0);
            values.CrewSchdMonEnd = new TimeSpan(17, 36, 0);
            values.CrewSchdFriEnd = new TimeSpan(23, 15, 0);
            values.CrewSchdTueEnd = new TimeSpan(21, 15, 0);
            values.CrewSchdWedEnd = new TimeSpan(15, 35, 0);
            values.CrewSchdThuEnd = new TimeSpan(15, 40, 0);
            values.CrewSchdSatEnd = new TimeSpan(23, 15, 0);
        }

        private void WorkCrewDetails()
        {
            Db.WorkCrew.AddRange(
                new WorkCrew
                {
                    WorkCrewId = 10,
                    EndDate = DateTime.Now.AddHours(1),
                    DeleteFlag = 0,
                    WorkCrewLookupId = 5,
                    InmateId = 100,
                    CreateDate = DateTime.Now.AddDays(-5),
                    CreatedBy = 12,
                    UpdateDate = DateTime.Now.AddDays(-1),
                    UpdatedBy = 11,
                    StartDate = DateTime.Now.AddDays(-5)
                },
                new WorkCrew
                {
                    WorkCrewId = 11,
                    EndDate = DateTime.Now.AddDays(1),
                    DeleteFlag = 0,
                    WorkCrewLookupId = 6,
                    InmateId = 101,
                    CreateDate = DateTime.Now.AddDays(-5),
                    UpdateDate = DateTime.Now.AddDays(-5),
                    CreatedBy = 11,
                    UpdatedBy = 12,
                    StartDate = DateTime.Now.AddDays(-5),
                },
                new WorkCrew
                {
                    WorkCrewId = 12,
                    EndDate = DateTime.Now.AddDays(1),
                    DeleteFlag = 0,
                    WorkCrewLookupId = 5,
                    InmateId = 101,
                    CreateDate = DateTime.Now.AddDays(-4),
                    CreatedBy = 11,
                    UpdateDate = DateTime.Now.AddDays(-1),
                    UpdatedBy = 11,
                    StartDate = DateTime.Now.AddDays(-4)
                },
                new WorkCrew
                {
                    WorkCrewId = 13,
                    EndDate = DateTime.Now.AddHours(5),
                    DeleteFlag = 0,
                    WorkCrewLookupId = 7,
                    InmateId = 102,
                    StartDate = DateTime.Now,
                    WorkCrewLockerId = 5,
                    UpdateDate = DateTime.Now.AddDays(-4),
                    CreateDate = DateTime.Now.AddDays(-4),
                    CreatedBy = 11,
                    UpdatedBy = 12
                },
                new WorkCrew
                {
                    WorkCrewId = 14,
                    EndDate = DateTime.Now.AddHours(5),
                    DeleteFlag = 0,
                    WorkCrewLookupId = 7,
                    InmateId = 103,
                    StartDate = DateTime.Now,
                    CreateDate = DateTime.Now.AddDays(-3),
                    UpdateDate = DateTime.Now.AddDays(-3),
                    CreatedBy = 11,
                    UpdatedBy = 11
                },
                new WorkCrew
                {
                    WorkCrewId = 15,
                    EndDate = DateTime.Now.AddHours(7),
                    DeleteFlag = 0,
                    WorkCrewLookupId = 8,
                    InmateId = 108,
                    StartDate = DateTime.Now,
                    CreateDate = DateTime.Now.AddDays(-3),
                    UpdateDate = DateTime.Now.AddDays(-2),
                    CreatedBy = 11,
                    UpdatedBy = 12
                });

            Db.WorkCrewRequest.AddRange(
                new WorkCrewRequest
                {
                    WorkCrewRequestId = 11,
                    InmateId = 106,
                    CreateDate = DateTime.Now.AddDays(-5),
                    WorkCrewLookupId = 7,
                    WorkCrewId = null,
                    ClassifyRouteFlag = 0,
                    CreateBy = 11
                },
                new WorkCrewRequest
                {
                    WorkCrewRequestId = 12,
                    InmateId = 100,
                    CreateDate = DateTime.Now.AddDays(-5),
                    CreateBy = 12,
                    WorkCrewLookupId = 5,
                    WorkCrewId = 10,
                    ClassifyRouteFlag = 1
                }
            );

            Db.WorkCrewTrackXref.AddRange(

                new WorkCrewTrackXref
                {
                    InmateWorkcrewXrefId = 10,
                    InmateTrakId = 7,
                    WorkCrewId = 10
                },
                new WorkCrewTrackXref
                {
                    InmateWorkcrewXrefId = 11,
                    InmateTrakId = 6,
                    WorkCrewId = 15
                },
                new WorkCrewTrackXref
                {
                    InmateWorkcrewXrefId = 12,
                    InmateTrakId = 5,
                    WorkCrewId = 13
                });

            Db.WorkCrewLocker.AddRange(
                new WorkCrewLocker
                {
                    WorkCrewLockerId = 5,
                    CreateDate = DateTime.Now.AddDays(-4),
                    UpdateDate = DateTime.Now.AddDays(-4),
                    CreateBy = 11,
                    UpdateBy = 12,
                    LockerName = "KITCHEN",
                    FacilityId = 2
                },
                new WorkCrewLocker
                {
                    WorkCrewLockerId = 6,
                    CreateDate = DateTime.Now.AddDays(-4),
                    UpdateDate = DateTime.Now.AddDays(-4),
                    CreateBy = 12,
                    UpdateBy = 12,
                    LockerName = "MEDICAL",
                    FacilityId = 2
                }
            );

            Db.WorkCrewLookup.AddRange(
                new WorkCrewLookup
                {
                    WorkCrewLookupId = 5,
                    FacilityId = 1,
                    WorkFurloughFlag = 1,
                    CrewName = "KITCHEN CREW",
                    ApplyWorkSentenceCredit = 0
                },
                new WorkCrewLookup
                {
                    WorkCrewLookupId = 6,
                    FacilityId = 2,
                    WorkFurloughFlag = null,
                    CrewName = "OFFICE ROOM CREW",
                    ApplyWorkSentenceCredit = 1
                },
                new WorkCrewLookup
                {
                    WorkCrewLookupId = 7,
                    FacilityId = 2,
                    WorkFurloughFlag = null,
                    CrewName = "PAINT CREW",
                    CrewSchdAnytime = 1,
                    ApplyWorkSentenceCredit =1,
                    ApplyWorkSentenceCheckoutDuration = 1
                },
                new WorkCrewLookup
                {
                    WorkCrewLookupId = 8,
                    FacilityId = 2,
                    WorkFurloughFlag = 1,
                    CrewName = "WAREHOUSE",
                    CrewSchdAnytime = 1,
                    DeleteFlag = 0,
                    ApplyWorkSentenceCredit = 0
                });


            foreach (var crewlookup in Db.WorkCrewLookup.Local)
            {
                Crewlookupvalues(crewlookup);
            }
            foreach (var workcrew in Db.WorkCrew.Local)
            {
                Workcrewvalues(workcrew);
            }
        }
    }
}
