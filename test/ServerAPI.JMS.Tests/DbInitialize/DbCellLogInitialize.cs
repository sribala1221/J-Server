using System;
using GenerateTables.Models;

// ReSharper disable once CheckNamespace
namespace ServerAPI.Tests
{
    public partial class DbInitialize
    {
        private void CellLogDetails()
        {
            Db.CellLogHeadcount.AddRange(
                new CellLogHeadcount
                {
                    CellLogHeadcountId = 10,
                    FacilityId = 1,
                    CellLogSchedule = DateTime.Now,
                    StartedBy = 12,
                    StartDate = DateTime.Now.AddDays(-5),
                    CellLogAssignedSum = 140,
                    CellLogCheckoutSum = 12,
                    CellLogActualSum = 128,
                    CellLogCountSum = 2550,
                    CellLogLocationSum = 0,
                    CellLogLocationCount = 0,
                    ClearLocationNote = null,
                    ClearNote = "NOT CLEARED"

                },
                new CellLogHeadcount
                {
                    CellLogHeadcountId = 11,
                    StartDate = DateTime.Now.AddDays(-5),
                    ClearedBy = 12,
                    StartedBy = 11,
                    ClearedDate = DateTime.Now,
                    CellLogAssignedSum = 150,
                    CellLogCheckoutSum = 5,
                    CellLogActualSum = 145,
                    CellLogCountSum = 250,
                    CellLogLocationSum = 2,
                    CellLogLocationCount = 2,
                    FacilityId = 1
                },
                new CellLogHeadcount
                {
                    CellLogHeadcountId = 12,
                    StartDate = DateTime.Now.AddDays(-3),
                    StartedBy = 12,
                    ClearedBy = 11,
                    ClearedDate = DateTime.Now,
                    CellLogAssignedSum = 140,
                    CellLogCheckoutSum = 12,
                    CellLogActualSum = 128,
                    CellLogCountSum = 390,
                    CellLogLocationSum = 10,
                    CellLogLocationCount = 10,
                    FacilityId = 2
                },
                new CellLogHeadcount
                {
                    CellLogHeadcountId = 13,
                    StartDate = DateTime.Now.AddDays(-3),
                    StartedBy = 11,
                    ClearedBy = 12,
                    FacilityId = 1,
                    ClearedDate = DateTime.Now,
                    CellLogAssignedSum = 150,
                    CellLogCheckoutSum = 20,
                    CellLogActualSum = 130,
                    CellLogLocationCount = 5,
                    CellLogLocationSum = 5,
                    CellLogCountSum = 200
                }

            );

            Db.CellLog.AddRange(
                new CellLog
                {
                    CellLogId = 20,
                    CellLogDate = DateTime.Now,
                    CellLogTime = "11:20",
                    CellLogOfficerId = 11,
                    CellLogComments = "HOUSING UNIT COUNT DETAILS",
                    HousingUnitListId = 6,
                    HousingUnitLocation = "FLOOR1",
                    HousingUnitNumber = "UP-B",
                    FacilityId = 1,
                    CreateBy = 11,
                    CellLogHeadcountId = 10,
                    CellLogLocation = "NEW BOOK - MALE",
                    CreateDate = DateTime.Now.AddDays(-6)
                },
                new CellLog
                {
                    CellLogId = 21,
                    CellLogDate = DateTime.Now,
                    CellLogTime = "12:30",
                    CellLogOfficerId = 12,
                    CellLogComments = null,
                    HousingUnitListId = 6,
                    HousingUnitLocation = "FLOOR1",
                    HousingUnitNumber = "UP-B",
                    FacilityId = 1,
                    CreateBy = 12,
                    CreateDate = DateTime.Now.AddDays(-5),
                    CellLogLocation = "NEW BOOK - FEMALE",
                    CellLogHeadcountId = 10,
                    CellLogLocationId = 6
                },
                new CellLog
                {
                    CellLogId = 22,
                    CellLogDate = DateTime.Now,
                    CellLogTime = "12:12",
                    CellLogOfficerId = 11,
                    CellLogComments = "CHNAGED ANOTHER LOCATION FOR CONGESTED",
                    FacilityId = 2,
                    HousingUnitListId = 8,
                    HousingUnitLocation = "FLOOR1",
                    HousingUnitNumber = "DOWN-B",
                    CreateBy = 12,
                    CreateDate = DateTime.Now.AddDays(-5),
                    CellLogLocationId = 7
                },

                new CellLog
                {
                    CellLogId = 23,
                    CellLogDate = DateTime.Now,
                    CellLogTime = "12:30",
                    CellLogOfficerId = 11,
                    CellLogComments = null,
                    HousingUnitListId = 6,
                    HousingUnitLocation = "FLOOR1",
                    HousingUnitNumber = "UP-B",
                    FacilityId = 2,
                    CreateBy = 12,
                    CreateDate = DateTime.Now.AddDays(-5),
                    CellLogLocation = "COURT",
                    CellLogHeadcountId = 12,
                    CellLogLocationId = 10
                },
                new CellLog
                {
                    CellLogId = 24,
                    CellLogDate = DateTime.Now,
                    CellLogTime = "12:30",
                    CellLogOfficerId = 12,
                    CellLogComments = null,
                    HousingUnitListId = 12,
                    HousingUnitLocation = "FLOOR1",
                    HousingUnitNumber = "UP-A",
                    FacilityId = 1,
                    CreateBy = 12,
                    CreateDate = DateTime.Now.AddDays(-5),
                    CellLogLocation = "COURT",
                    CellLogHeadcountId = 10
                }
            );

            Db.CellLogInmate.AddRange(
                new CellLogInmate
                {
                    CellLogInmateId = 200,
                    InmateId = 105,
                    CellLogId = 21,
                    FacilityId = 2
                }
            );

            Db.CellLogSaveHistory.AddRange(
                new CellLogSaveHistory
                {
                    CellLogId = 22,
                    CellLogSaveHistoryId = 20,
                    CreateDate = DateTime.Now.AddDays(-5),
                    PersonnelId = 12,
                    CellLogSaveHistoryList =
                        @"{ 'SCHEDULE':'2018-03-01T11:28:52.1048887+05:30', 'HOUSING':'MCJ', 'ASSIGNED':'29', 'CHECKOUT':'0', 'ACTUAL':'29' }"
                },
                new CellLogSaveHistory
                {
                    CellLogId = 22,
                    CellLogSaveHistoryId = 22,
                    CreateDate = DateTime.Now.AddDays(-4),
                    PersonnelId = 11,
                    CellLogSaveHistoryList =
                        @"{'SCHEDULE':'2018 - 03 - 05T12:28:52.1048887 + 06:30','LOCATION':'COURT','ACTUAL':'2'}"
                }
            );
        }


    }
}
