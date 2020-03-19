using GenerateTables.Models;
using System;


// ReSharper disable once CheckNamespace
namespace ServerAPI.Tests
{
    public partial class DbInitialize
    {
        private void HousingDetails()
        {
            Db.HousingUnit.AddRange(
                new HousingUnit
                {
                    HousingUnitId = 10,
                    HousingUnitLocation = "FLOOR1",
                    HousingUnitNumber = "UP-A",
                    HousingUnitBedNumber = "UPA01",
                    HousingUnitBedLocation = null,
                    HousingUnitListId = 5,
                    FacilityId = 1,
                    HousingUnitActualCapacity = 5,
                    HousingUnitOutOfService = 1,
                    HousingUnitSex = 1
                },
                new HousingUnit
                {
                    HousingUnitId = 11,
                    HousingUnitLocation = "FLOOR2",
                    HousingUnitNumber = "UP-A",
                    HousingUnitBedNumber = "UPA02",
                    HousingUnitBedLocation = "UPPER BED LOC2",
                    FacilityId = 1,
                    HousingUnitActualCapacity = 0,
                    HousingUnitOutOfService = 1,
                    HousingUnitSex = 1,
                    HousingUnitListId = 9,
                    HousingUnitClassConflictCheck = 1
                },
                new HousingUnit
                {
                    HousingUnitId = 12,
                    HousingUnitLocation = "FLOOR1",
                    HousingUnitNumber = "UP-B",
                    HousingUnitBedNumber = "UPB01",
                    HousingUnitBedLocation = null,
                    FacilityId = 1,
                    HousingUnitActualCapacity = 0,
                    HousingUnitOutOfService = 1,
                    HousingUnitListId = 6
                },
                new HousingUnit
                {
                    HousingUnitId = 13,
                    HousingUnitLocation = "FLOOR2",
                    HousingUnitNumber = "UP-B",
                    HousingUnitBedNumber = "UPB02",
                    HousingUnitBedLocation = null,
                    FacilityId = 1,
                    HousingUnitActualCapacity = 5,
                    HousingUnitOutOfService = 1,
                    HousingUnitListId = 6,
                    HousingUnitSex = 2
                },
                new HousingUnit
                {
                    HousingUnitId = 14,
                    HousingUnitLocation = "FLOOR2",
                    HousingUnitNumber = "DOWN-A",
                    HousingUnitBedNumber = "DOWNA01",
                    HousingUnitBedLocation = "DOWN BED LOC1",
                    FacilityId = 2,
                    HousingUnitActualCapacity = 4,
                    HousingUnitOutOfService = 0,
                    HousingUnitListId = 11,
                    HousingUnitFlagString = "ARSON"
                },
                new HousingUnit
                {
                    HousingUnitId = 15,
                    HousingUnitLocation = "FLOOR2",
                    HousingUnitNumber = "UP-B",
                    HousingUnitBedNumber = "UPA011",
                    HousingUnitBedLocation = null,
                    FacilityId = 1,
                    HousingUnitActualCapacity = 2,
                    HousingUnitListId = 10,
                    HousingUnitInactive = 0,
                    HousingUnitVisitAllow = 1
                },
                new HousingUnit
                {
                    HousingUnitId = 16,
                    HousingUnitLocation = "FLOOR2",
                    HousingUnitNumber = "DOWN-B",
                    HousingUnitBedNumber = "DOWN0014",
                    HousingUnitBedLocation = "LOC1",
                    FacilityId = 1,
                    HousingUnitActualCapacity = 2,
                    HousingUnitListId = 12,
                    HousingUnitInactive = null
                },

                new HousingUnit
                {
                    HousingUnitId = 17,
                    HousingUnitLocation = "FLOOR2",
                    HousingUnitNumber = "DOWN-UP-A",
                    HousingUnitBedLocation = "DOWNUP01",
                    HousingUnitBedNumber = "LOC2",
                    FacilityId = 2,
                    HousingUnitActualCapacity = 4,
                    HousingUnitListId = 12,
                    HousingUnitDoNotRecommend = 1,
                    HousingUnitInactive = null,
                    HousingUnitClassConflictCheck = 1,
                    HousingUnitClassifyRecString = "MIN MENTAL",
                    HousingUnitFlagString = "WHEELCHAIR",
                    HousingUnitOutOfService = 5,
                    HousingUnitSex = 2
                }
            );
            Db.HousingUnitList.AddRange(
                new HousingUnitList
                {
                    HousingUnitListId = 5,
                    HousingUnitLocation = "FLOOR1",
                    HousingUnitNumber = "UP-A",
                    FacilityId = 1,
                    LocationIdList = "5,6,7"
                },
                new HousingUnitList
                {
                    HousingUnitListId = 6,
                    HousingUnitLocation = "FLOOR1",
                    HousingUnitNumber = "UP-B",
                    FacilityId = 1,
                    LocationIdList = "5,6,7"
                },
                new HousingUnitList
                {
                    HousingUnitListId = 7,
                    HousingUnitLocation = "FLOOR1",
                    HousingUnitNumber = "DOWN-A",
                    FacilityId = 1,
                    LocationIdList = "5,7,9"
                },
                new HousingUnitList
                {
                    HousingUnitListId = 8,
                    HousingUnitLocation = "FLOOR1",
                    HousingUnitNumber = "DOWN-B",
                    FacilityId = 2
                },
                new HousingUnitList
                {
                    HousingUnitListId = 9,
                    HousingUnitLocation = "FLOOR2",
                    HousingUnitNumber = "UP-A",
                    FacilityId = 1
                },
                new HousingUnitList
                {
                    HousingUnitListId = 10,
                    HousingUnitLocation = "FLOOR2",
                    HousingUnitNumber = "UP-B",
                    FacilityId = 2,
                    LocationIdList = "6,7,8"
                },
                new HousingUnitList
                {
                    HousingUnitListId = 11,
                    HousingUnitLocation = "FLOOR2",
                    HousingUnitNumber = "DOWN-A",
                    FacilityId = 1
                },
                new HousingUnitList
                {
                    HousingUnitListId = 12,
                    HousingUnitLocation = "FLOOR2",
                    HousingUnitNumber = "DOWN-B",
                    FacilityId = 1,
                    LocationIdList = "5,6,7,8"
                }
            );

            Db.HousingUnitVisitationClassRule.AddRange(
                new HousingUnitVisitationClassRule
                {
                    HousingUnitVisitationClassRuleId = 20,
                    CreateDate = DateTime.Now.AddDays(-20),
                    DeleteFlag = null,
                    DeleteDate = null,
                    DeleteBy = null,
                    CreateBy = 12,
                    HousingUnitListId = 5,
                    ClassificationReason = "MINIMUM"
                },
                new HousingUnitVisitationClassRule
                {
                    HousingUnitVisitationClassRuleId = 21,
                    CreateDate = DateTime.Now.AddDays(-20),
                    DeleteFlag = null,
                    DeleteDate = null,
                    DeleteBy = null,
                    CreateBy = 12,
                    HousingUnitListId = 6,
                    ClassificationReason = "ACUTE MEDICAL"
                },
                new HousingUnitVisitationClassRule
                {
                    HousingUnitVisitationClassRuleId = 22,
                    CreateDate = DateTime.Now.AddDays(-20),
                    DeleteFlag = null,
                    DeleteDate = null,
                    DeleteBy = null,
                    CreateBy = 12,
                    HousingUnitListId = 5,
                    ClassificationReason = "ADMINISTRATIVE SEPARATION 1"
                }



                );
            Db.HousingUnitScheduleCount.AddRange(

                new HousingUnitScheduleCount
                {
                    FacilityId = 1,
                    HousingUnitScheduleCountId = 13,
                    CountTime = new TimeSpan(DateTime.Now.Hour, DateTime.Now.Minute, 0),
                    HousingUnitListId = 5,
                    HousingUnitLocation = "FLOOR1",
                    HousingUnitNumber = "UP-A"
                },
                new HousingUnitScheduleCount
                {
                    FacilityId = 1,
                    HousingUnitScheduleCountId = 14,
                    CountTime = new TimeSpan(DateTime.Now.Hour, DateTime.Now.Minute, 0),
                    HousingUnitListId = 6,
                    HousingUnitLocation = "FLOOR1",
                    HousingUnitNumber = "UP-B"
                },
                new HousingUnitScheduleCount
                {
                    FacilityId = 1,
                    HousingUnitScheduleCountId = 15,
                    CountTime = new TimeSpan(DateTime.Now.Hour, DateTime.Now.Minute, 0),
                    HousingUnitListId = 7,
                    HousingUnitLocation = "FLOOR1",
                    HousingUnitNumber = "DOWN-A"
                }
            );

            Db.HousingUnitOosHistory.AddRange(
                new HousingUnitOosHistory
                {
                    HousingUnitOosHistoryId = 5,
                    HousingUnitId = 10,
                    OutOfServiceCount = 5,
                    OutOfServiceReason = "MISC",
                    OutOfServiceDate = DateTime.Now
                },
                new HousingUnitOosHistory
                {
                    HousingUnitOosHistoryId = 6,
                    HousingUnitId = 11,
                    OutOfServiceCount = 10,
                    OutOfServiceReason = "MISC",
                    OutOfServiceDate = DateTime.Now
                });

            Db.HousingUnitMoveHistory.AddRange(

                new HousingUnitMoveHistory
                {
                    HousingUnitMoveHistoryId = 10,
                    InmateId = 101,
                    MoveDate = DateTime.Now,
                    CreateDate = DateTime.Now.AddDays(-5),
                    UpdateDate = DateTime.Now.AddHours(-1),
                    MoveReason = "SMALL CELL",
                    MoveOfficerId = 11,
                    HousingUnitFromId = 11,
                    HousingUnitToId = 12
                },
                new HousingUnitMoveHistory
                {
                    HousingUnitMoveHistoryId = 11,
                    InmateId = 103,
                    MoveDate = DateTime.Now,
                    CreateDate = DateTime.Now.AddDays(4),
                    UpdateDate = DateTime.Now,
                    MoveReason = "OLD ROOM",
                    MoveOfficerId = 12,
                    HousingUnitFromId = 11,
                    HousingUnitToId = 10
                },
                new HousingUnitMoveHistory
                {
                    HousingUnitMoveHistoryId = 12,
                    InmateId = 102,
                    MoveDate = DateTime.Now.AddDays(-1),
                    CreateDate = DateTime.Now.AddDays(-4),
                    UpdateDate = DateTime.Now.AddDays(-1),
                    MoveReason = "SAME AGE",
                    MoveOfficerId = 12,
                    HousingUnitToId = 10
                },
                new HousingUnitMoveHistory
                {
                    HousingUnitMoveHistoryId = 13,
                    InmateId = 140,
                    MoveDate = DateTime.Now,
                    CreateDate = DateTime.Now,
                    UpdateDate = DateTime.Now,
                    MoveReason = "CHANGE DIFFERENT LOCATION",
                    MoveOfficerId = 11,
                    HousingUnitToId = 12,
                    MoveDateThru = DateTime.Now.AddDays(-1)
                },
                new HousingUnitMoveHistory
                {
                    HousingUnitMoveHistoryId = 14,
                    InmateId = 110,
                    MoveDate = DateTime.Now,
                    CreateDate = DateTime.Now,
                    UpdateDate = DateTime.Now,
                    MoveReason = "NEW INMATE",
                    MoveOfficerId = 12,
                    HousingUnitToId = 10,
                    MoveDateThru = DateTime.Now.AddDays(-1),
                    HousingUnitFromId = 12
                });

            Db.HousingGroup.AddRange(
                new HousingGroup
                {
                    HousingGroupId = 10,
                    CreateBy = 11,
                    CreateDate = DateTime.Now.AddDays(-10),
                    UpdateBy = 12,
                    GroupString = "'FLOOR1 DOWN-A','FLOOR1 UP-A','FLOOR2 DOWN-A','FLOOR1 UP-B'",
                    LocationString = "'CHENNAI','MADURAI','TRICHY','COIMBATORE','PUDUKKOTTAI'",
                    UpdateDate = DateTime.Now.AddDays(-3),
                    FacilityId = 1,
                    GroupName = "HOUSING A BLOCK"
                },
                new HousingGroup
                {
                    HousingGroupId = 11,
                    CreateBy = 12,
                    CreateDate = DateTime.Now.AddDays(-9),
                    UpdateBy = 12,
                    UpdateDate = DateTime.Now.AddDays(-3),
                    FacilityId = 1,
                    GroupString = "'FLOOR2 DOWN-B','FLOOR2 UP-B','FLOOR2 DOWN-A','FLOOR1 UP-A'",
                    GroupName = "HOUSING B BLOCK",
                    LocationString = "'MADURAI','TRICHY','COIMBATORE','VELLUR'"
                },
                new HousingGroup
                {
                    HousingGroupId = 12,
                    CreateBy = 11,
                    CreateDate = DateTime.Now.AddDays(-10),
                    UpdateBy = 12,
                    UpdateDate = DateTime.Now.AddDays(-2),
                    FacilityId = 2,
                    GroupName = "HOUSING C BLOCK",
                    GroupString = "'FLOOR2 DOWN-B','FLOOR2 UP-B','FLOOR2 DOWN-A','FLOOR1 UP-A'",
                    LocationString = "'CHENNAI','COIMBATORE','PUDUKKOTTAI','VELLUR'"
                }
            );

            Db.HousingGroupAssign.AddRange(
                new HousingGroupAssign
                {
                    HousingGroupAssignId = 10,
                    CreateBy = 12,
                    CreateDate = DateTime.Now.AddDays(-6),
                    UpdateBy = 12,
                    UpdateDate = DateTime.Now.AddDays(-3),
                    HousingGroupId = 10,
                    HousingUnitListId = 9
                },
                new HousingGroupAssign
                {
                    HousingGroupAssignId = 11,
                    CreateBy = 12,
                    CreateDate = DateTime.Now.AddDays(-6),
                    UpdateDate = DateTime.Now.AddDays(-4),
                    UpdateBy = 12,
                    DeleteBy = null,
                    HousingGroupId = 11,
                    HousingUnitListId = 6
                },

                new HousingGroupAssign
                {
                    HousingGroupAssignId = 12,
                    CreateBy = 11,
                    CreateDate = DateTime.Now.AddDays(-5),
                    UpdateDate = DateTime.Now.AddDays(-4),
                    UpdateBy = 12,
                    DeleteBy = null,
                    HousingGroupId = 12,
                    HousingUnitListId = 6
                },
                new HousingGroupAssign
                {
                    HousingGroupAssignId = 13,
                    CreateBy = 12,
                    CreateDate = DateTime.Now.AddDays(-5),
                    UpdateDate = DateTime.Now.AddDays(-4),
                    UpdateBy = 11,
                    DeleteBy = null,
                    HousingGroupId = 11,
                    HousingUnitListId = 9
                });

            Db.HousingUnitScheduleSafteyCheck.AddRange(
                new HousingUnitScheduleSafteyCheck
                {
                    HousingUnitScheduleSafteyCheckId = 10,
                    HousingUnitListId = 5,
                    FacilityId = 1,
                    HousingUnitLocation = "FLOOR1",
                    HousingUnitNumber = "UP-A",
                    LastEntryBy = 11,
                    LastEntry = DateTime.Now,
                    DeleteDate = null,
                    DeleteFlag = 0,
                    LateEntryMaxMin = 6
                },
                new HousingUnitScheduleSafteyCheck
                {
                    HousingUnitScheduleSafteyCheckId = 11,
                    HousingUnitListId = 11,
                    FacilityId = 1,
                    HousingUnitLocation = "FLOOR2",
                    HousingUnitNumber = "DOWN-A",
                    LastEntryBy = 12,
                    LastEntry = DateTime.Now,
                    DeleteDate = null,
                    DeleteFlag = 0,
                    LateEntryMaxMin = 10
                },
                new HousingUnitScheduleSafteyCheck
                {
                    HousingUnitScheduleSafteyCheckId = 12,
                    HousingUnitListId = 12,
                    FacilityId = 1,
                    HousingUnitNumber = "UP-B",
                    HousingUnitLocation = "FLOOR2",
                    DeleteDate = null,
                    DeletedBy = null,
                    DeleteFlag = null,
                    LastEntryBy = 11,
                    LateEntryMaxMin = 30
                }
            );
            Db.HousingUnitVisitation.AddRange(
                new HousingUnitVisitation
                {
                    HousingUnitVisitationId = 10,
                    FacilityId = 1,
                    DeleteFlag = 0,
                    DeleteBy = null,
                    HousingUnitLocation = "FLOOR2",
                    HousingUnitNumber = "DOWN-A",
                    VisitationDay = DateTime.Now.AddDays(-1).Date.DayOfWeek.ToString()
                },
                new HousingUnitVisitation
                {
                    HousingUnitVisitationId = 11,
                    FacilityId = 1,
                    DeleteFlag = 0,
                    DeleteBy = null,
                    HousingUnitLocation = "FLOOR1",
                    HousingUnitNumber = "UP-A"
                },
                new HousingUnitVisitation
                {
                    HousingUnitVisitationId = 12,
                    FacilityId = 2,
                    DeleteFlag = 0,
                    DeleteBy = null,
                    HousingUnitLocation = "FLOOR1",
                    HousingUnitNumber = "UP-B",
                    VisitationDay = DateTime.Now.Date.DayOfWeek.ToString(),
                    VisitationFrom = DateTime.Now.AddHours(-1),
                    VisitationTo = DateTime.Now,
                    RegistrationFrom = DateTime.Now.AddHours(-3),
                    RegistrationTo = DateTime.Now
                },
                new HousingUnitVisitation
                {
                    HousingUnitVisitationId = 13,
                    FacilityId = 1,
                    DeleteFlag = null,
                    DeleteBy = null,
                    HousingUnitLocation = "FLOOR1",
                    HousingUnitNumber = "UP-B",
                    VisitationDay = DateTime.Now.Date.DayOfWeek.ToString(),
                    VisitationFrom = DateTime.Now.AddHours(-2),
                    VisitationTo = DateTime.Now,
                    RegistrationFrom = DateTime.Now.AddHours(-3),
                    RegistrationTo = DateTime.Now,
                    HousingUnitListId = 6
                }
            );

            Db.HousingUnitVisitationCell.AddRange(
                new HousingUnitVisitationCell
                {
                    HousingUnitVisitationCellId = 10,
                    FacilityId = 1,
                    DeleteFlag = 0,
                    DeleteDate = null,
                    DeleteBy = null,
                    HousingUnitId = 11,
                    VisitationDay = "MONDAY",
                    RegistrationFrom = null,
                    RegistrationTo = null
                },
                new HousingUnitVisitationCell
                {
                    HousingUnitVisitationCellId = 11,
                    FacilityId = 1,
                    DeleteFlag = 0,
                    DeleteDate = null,
                    DeleteBy = null,
                    HousingUnitId = 12,
                    VisitationDay = "WEDNESDAY",
                    RegistrationFrom = null,
                    RegistrationTo = null
                }
                );

            Db.HousingSupplyItem.AddRange(
                new HousingSupplyItem
                {
                    HousingSupplyItemId = 10,
                    CreateDate = DateTime.Now.AddDays(-15),
                    DeleteDate = null,
                    CreatedBy = 12,
                    DeleteFlag = null,
                    DeletedBy = null,
                    UpdateDate = DateTime.Now,
                    UpdatedBy = 11,
                    HousingSupplyModuleId = 10,
                    AllowCheckoutToHousing = 1,
                    AllowCheckoutToInmate = 1,
                    AllowCheckoutToLocation = 1,
                    AllowMove = 0,
                    CurrentCheckoutInmateId = null,
                    CurrentCheckoutHousing = null,
                    CurrentCheckoutInmate = null,
                    CurrentCheckoutLocation = null,
                    HousingSupplyItemLookupId = 10,
                    CurrentCheckoutModuleId = 11,
                    SupplyNumber = "SP_1400"
                },
                new HousingSupplyItem
                {
                    HousingSupplyItemId = 11,
                    CreateDate = DateTime.Now.AddDays(-10),
                    DeleteDate = null,
                    CreatedBy = 12,
                    DeleteFlag = null,
                    DeletedBy = null,
                    UpdateDate = DateTime.Now.AddDays(-10),
                    UpdatedBy = 11,
                    HousingSupplyModuleId = 11,
                    AllowCheckoutToHousing = 1,
                    AllowCheckoutToInmate = 1,
                    AllowCheckoutToLocation = 1,
                    AllowMove = 0,
                    CurrentCheckoutInmateId = 100,
                    HousingSupplyItemLookupId = 10,
                    CurrentCheckoutModuleId = 11,
                    SupplyNumber = "SP_1400"
                },
                new HousingSupplyItem
                {
                    HousingSupplyItemId = 12,
                    CreateDate = DateTime.Now.AddDays(-10),
                    DeleteDate = null,
                    CreatedBy = 11,
                    DeleteFlag = null,
                    DeletedBy = null,
                    UpdateDate = DateTime.Now.AddDays(-10),
                    UpdatedBy = 11,
                    HousingSupplyModuleId = 12,
                    AllowCheckoutToHousing = 1,
                    AllowCheckoutToInmate = 1,
                    AllowCheckoutToLocation = 1,
                    AllowMove = 0,
                    CurrentCheckoutInmateId = null,
                    HousingSupplyItemLookupId = 11,
                    CurrentCheckoutModuleId = 11,
                    SupplyNumber = "SP_1400"
                },
                new HousingSupplyItem
                {
                    HousingSupplyItemId = 13,
                    CreateDate = DateTime.Now.AddDays(-10),
                    DeleteDate = null,
                    CreatedBy = 11,
                    DeleteFlag = null,
                    DeletedBy = null,
                    UpdateDate = DateTime.Now.AddDays(-10),
                    UpdatedBy = 11,
                    HousingSupplyModuleId = 12,
                    AllowCheckoutToHousing = 0,
                    AllowCheckoutToInmate = null,
                    AllowCheckoutToLocation = null,
                    AllowMove = 0,
                    CurrentCheckoutInmateId = null,
                    HousingSupplyItemLookupId = 11,
                    CurrentCheckoutModuleId = 11,
                    SupplyNumber = "SP_1401"
                },
                new HousingSupplyItem
                {
                    HousingSupplyItemId = 14,
                    CreateDate = DateTime.Now.AddDays(-10),
                    DeleteDate = null,
                    CreatedBy = 12,
                    DeleteFlag = null,
                    DeletedBy = null,
                    UpdateDate = DateTime.Now.AddDays(-10),
                    UpdatedBy = 11,
                    HousingSupplyModuleId = 11,
                    AllowCheckoutToHousing = 1,
                    AllowCheckoutToInmate = 1,
                    AllowCheckoutToLocation = 1,
                    AllowMove = 0,
                    CurrentCheckoutInmateId = 100,
                    HousingSupplyItemLookupId = 10,
                    CurrentCheckoutModuleId = 11,
                    SupplyNumber = "SP_1400"
                },
                new HousingSupplyItem
                {
                    HousingSupplyItemId = 15,
                    CreateDate = DateTime.Now.AddDays(-10),
                    DeleteDate = null,
                    CreatedBy = 12,
                    DeleteFlag = null,
                    DeletedBy = null,
                    UpdateDate = DateTime.Now.AddDays(-10),
                    UpdatedBy = 11,
                    HousingSupplyModuleId = 11,
                    AllowCheckoutToHousing = 1,
                    AllowCheckoutToInmate = 1,
                    AllowCheckoutToLocation = 1,
                    AllowMove = 0,
                    CurrentCheckoutInmateId = 100,
                    HousingSupplyItemLookupId = 10,
                    CurrentCheckoutModuleId = 11,
                    SupplyNumber = "SP_1401",
                    ConsumedFlag = 1
                },
                new HousingSupplyItem
                {
                    HousingSupplyItemId = 16,
                    CreateDate = DateTime.Now.AddDays(-10),
                    CreatedBy = 11,
                    UpdateDate = DateTime.Now.AddDays(-10),
                    UpdatedBy = 11,
                    HousingSupplyModuleId = 11,
                    AllowCheckoutToHousing = 1,
                    AllowCheckoutToInmate = 1,
                    AllowCheckoutToLocation = 1,
                    AllowMove = 0,
                    CurrentCheckoutInmateId = 100,
                    HousingSupplyItemLookupId = 11,
                    CurrentCheckoutModuleId = 11,
                    SupplyNumber = "SP_1400",
                    ConsumedFlag = 0
                },
                new HousingSupplyItem
                {
                    HousingSupplyItemId = 17,
                    CreateDate = DateTime.Now.AddDays(-10),
                    CreatedBy = 11,
                    UpdateDate = DateTime.Now.AddDays(-10),
                    UpdatedBy = 11,
                    HousingSupplyModuleId = 11,
                    AllowCheckoutToHousing = 1,
                    AllowCheckoutToInmate = 1,
                    AllowCheckoutToLocation = 1,
                    AllowMove = 0,
                    CurrentCheckoutInmateId = 100,
                    HousingSupplyItemLookupId = 11,
                    CurrentCheckoutModuleId = 11,
                    SupplyNumber = "SP_1400",
                    ConsumedFlag = 0
                },
                new HousingSupplyItem
                {
                    HousingSupplyItemId = 18,
                    CreateDate = DateTime.Now.AddDays(-10),
                    CreatedBy = 11,
                    UpdateDate = DateTime.Now.AddDays(-10),
                    UpdatedBy = 11,
                    HousingSupplyModuleId = 11,
                    AllowCheckoutToHousing = 1,
                    AllowCheckoutToInmate = 1,
                    AllowCheckoutToLocation = 1,
                    AllowMove = 0,
                    CurrentCheckoutInmateId = 100,
                    HousingSupplyItemLookupId = 11,
                    CurrentCheckoutModuleId = 11,
                    SupplyNumber = "SP_1400",
                    ConsumedFlag = 0
                },
                new HousingSupplyItem
                {
                    HousingSupplyItemId = 19,
                    CreateDate = DateTime.Now.AddDays(-10),
                    CreatedBy = 11,
                    UpdateDate = DateTime.Now.AddDays(-10),
                    UpdatedBy = 11,
                    HousingSupplyModuleId = 11,
                    AllowCheckoutToHousing = 1,
                    AllowCheckoutToInmate = 1,
                    AllowCheckoutToLocation = 1,
                    AllowMove = 0,
                    CurrentCheckoutInmateId = 100,
                    HousingSupplyItemLookupId = 11,
                    CurrentCheckoutModuleId = 11,
                    SupplyNumber = "SP_1400",
                    ConsumedFlag = 0
                }

            );
            Db.HousingSupplyItemHistory.AddRange(
                new HousingSupplyItemHistory
                {
                    HousingSupplyItemHistoryId = 10,
                    HousingSupplyItemId = 16,
                    CreateDate = DateTime.Now.AddDays(-10),
                    PersonnelId = 12,
                    HousingSupplyItemHistoryList = @"{ 'DeleteFlag':'1','DeleteBy':'11','Deletedate':'02-26-2019 14:00'}"
                },
                new HousingSupplyItemHistory
                {

                    HousingSupplyItemHistoryId = 11,
                    HousingSupplyItemId = 17,
                    CreateDate = DateTime.Now.AddDays(-10),
                    PersonnelId = 12,
                    HousingSupplyItemHistoryList = @"{ 'DeleteFlag':'1','DeleteBy':'11','Deletedate':'02-26-2019 14:00'}"
                },
                new HousingSupplyItemHistory
                {

                    HousingSupplyItemHistoryId = 12,
                    HousingSupplyItemId = 18,
                    CreateDate = DateTime.Now.AddDays(-10),
                    PersonnelId = 12,
                    HousingSupplyItemHistoryList = @"{ 'DeleteFlag':'1','DeleteBy':'12','Deletedate':'02-26-2019 14:00'}"
                },
                new HousingSupplyItemHistory
                {

                    HousingSupplyItemHistoryId = 13,
                    HousingSupplyItemId = 19,
                    CreateDate = DateTime.Now.AddDays(-10),
                    PersonnelId = 12,
                    HousingSupplyItemHistoryList = @"{ 'DeleteFlag':'1','DeleteBy':'11','Deletedate':'02-26-2019 14:00'}"
                }
                );
            Db.HousingSupplyCheckout.AddRange(


                new HousingSupplyCheckout
                {
                    HousingSupplyCheckoutId = 10,
                    HousingSupplyItemId = 11,
                    CheckinBy = null,
                    CheckinDate = DateTime.Now,
                    CheckoutDate = DateTime.Now.AddDays(-10),
                    CheckinNote = null,
                    CheckoutInmateId = "100",
                    CheckoutBy = 11

                },
                new HousingSupplyCheckout
                {
                    HousingSupplyCheckoutId = 11,
                    HousingSupplyItemId = 10,
                    CheckinBy = null,
                    CheckinNote = null,
                    CheckoutBy = 11,
                    CheckinDate = null,
                    CheckoutDate = DateTime.Now,
                    CheckoutInmateId = "101"
                },
                new HousingSupplyCheckout
                {
                    HousingSupplyCheckoutId = 12,
                    HousingSupplyItemId = 15,
                    CheckinBy = null,
                    CheckinNote = null,
                    CheckoutBy = 11,
                    CheckinDate = DateTime.Now,
                    CheckoutDate = DateTime.Now,
                    CheckoutInmateId = "100"

                }

            );

            Db.HousingSupplyItemLookup.AddRange(
                new HousingSupplyItemLookup
                {
                    HousingSupplyItemLookupId = 10,
                    CreateDate = DateTime.Now.AddDays(-10),
                    DeleteFlag = null,
                    DeleteDate = null,
                    UpdateDate = DateTime.Now,
                    UpdatedBy = 12,
                    CreatedBy = 12,
                    DeletedBy = null,
                    ItemName = "VOLLEYBALL"

                },
                new HousingSupplyItemLookup
                {
                    HousingSupplyItemLookupId = 11,
                    CreateDate = DateTime.Now.AddDays(-10),
                    DeleteFlag = null,
                    DeleteDate = null,
                    UpdateDate = DateTime.Now,
                    UpdatedBy = 11,
                    CreatedBy = 12,
                    DeletedBy = null,
                    ItemName = "SOFTBALL EQUIPMENT"

                },
                new HousingSupplyItemLookup
                {
                    HousingSupplyItemLookupId = 12,
                    CreateDate = DateTime.Now.AddDays(-10),
                    DeleteFlag = null,
                    DeleteDate = null,
                    UpdateDate = DateTime.Now,
                    UpdatedBy = 12,
                    CreatedBy = 11,
                    DeletedBy = null,
                    ItemName = "BASKETBALL"

                }

            );

            Db.HousingSupplyModule.AddRange(
                new HousingSupplyModule
                {
                    HousingSupplyModuleId = 10,
                    FacilityId = 1,
                    CreatedBy = 11,
                    CreateDate = DateTime.Now.AddDays(-10),
                    UpdateDate = DateTime.Now.AddDays(-4),
                    UpdatedBy = 12,
                    DeleteDate = null,
                    DeleteFlag = null,
                    DeletedBy = null,
                    ClosetName = "CLOSET A",
                    ClosetBelongsTo = "'FLOOR1 UP-A','FLOOR1 UP-B'"
                },
                new HousingSupplyModule
                {
                    HousingSupplyModuleId = 11,
                    FacilityId = 1,
                    CreatedBy = 11,
                    CreateDate = DateTime.Now.AddDays(-10),
                    UpdateDate = DateTime.Now.AddDays(-4),
                    UpdatedBy = 12,
                    DeleteDate = null,
                    DeleteFlag = null,
                    DeletedBy = null,
                    ClosetName = "CLOSET B",
                    ClosetBelongsTo = "'FLOOR1 UP-A'"
                },
                new HousingSupplyModule
                {
                    HousingSupplyModuleId = 12,
                    CreatedBy = 12,
                    FacilityId = 2,
                    CreateDate = DateTime.Now.AddDays(-10),
                    UpdateDate = DateTime.Now.AddDays(-4),
                    UpdatedBy = 12,
                    DeleteDate = null,
                    DeleteFlag = null,
                    DeletedBy = null,
                    ClosetName = "CLOSET C",
                    ClosetBelongsTo = "'FLOOR2 UP-A','FLOOR1 UP-A'"
                }

            );
            Db.HousingSupplyCheckList.AddRange(

                new HousingSupplyCheckList
                {
                    HousingSupplyCheckListId = 10,
                    CreateDate = DateTime.Now.AddDays(-10),
                    CreatedBy = 12,
                    DeleteFlag = null,
                    DeleteDate = null,
                    DeletedBy = null,
                    UpdateDate = DateTime.Now,
                    UpdatedBy = 12,
                    CheckListBy = 11,
                    CheckListDate = DateTime.Now,
                    CheckListNote = "PANT SIZE MIS MATCHED"
                },
                new HousingSupplyCheckList
                {
                    HousingSupplyCheckListId = 11,
                    CreateDate = DateTime.Now.AddDays(-10),
                    CreatedBy = 12,
                    UpdateDate = null,
                    UpdatedBy = 12,
                    CheckListBy = 11,
                    CheckListDate = DateTime.Now,
                    CheckListNote = "DAMAGED PIECE IS RETUNED "
                },
                new HousingSupplyCheckList
                {
                    HousingSupplyCheckListId = 12,
                    CreateDate = DateTime.Now.AddDays(-10),
                    CreatedBy = 11,
                    DeleteFlag = null,
                    DeleteDate = null,
                    DeletedBy = null,
                    UpdateDate = DateTime.Now,
                    UpdatedBy = 12,
                    CheckListBy = 11,
                    CheckListDate = DateTime.Now,
                    CheckListNote = "DAMAGED PIECE ARE PROVIDED"
                }
            );
            Db.HousingSupplyCheckListXref.AddRange(
                new HousingSupplyCheckListXref
                {
                    HousingSupplyCheckListXrefId = 15,
                    CreateDate = DateTime.Now.AddDays(-10),
                    CreatedBy = 11,
                    UpdateDate = DateTime.Now,
                    UpdatedBy = 12,
                    CheckListFlag = 1,
                    HousingSupplyCheckListId = 10,
                    CheckListNote = "PANT SIZE MIS MATCHED",
                    HousingSupplyItemId = 10
                },
                new HousingSupplyCheckListXref
                {
                    HousingSupplyCheckListXrefId = 16,
                    CreateDate = DateTime.Now.AddDays(-10),
                    CreatedBy = 11,
                    UpdateDate = DateTime.Now,
                    UpdatedBy = 12,
                    CheckListFlag = 1,
                    HousingSupplyCheckListId = 10,
                    CheckListNote = "QUANTITY COUNT IS CHECKED",
                    HousingSupplyItemId = 10
                },
                new HousingSupplyCheckListXref
                {
                    HousingSupplyCheckListXrefId = 17,
                    CreateDate = DateTime.Now.AddDays(-10),
                    CreatedBy = 11,
                    UpdateDate = DateTime.Now,
                    UpdatedBy = 12,
                    CheckListFlag = 1,
                    HousingSupplyCheckListId = 10,
                    CheckListNote = "1 QUANTITY GIVEN TO THE INMATE",
                    HousingSupplyItemId = 11
                }

            );

            Db.Privileges.AddRange(
                new Privileges
                {
                    PrivilegeId = 5,
                    InactiveFlag = 0,
                    FacilityId = 1,
                    RemoveFromPrivilegeFlag = 1,
                    CommisssaryPrivilegeFlag = 0,
                    VisitationPrivilegeFlag = 0,
                    VisitationPhoneFlag = 0,
                    RemoveFromTrackingFlag = 0,
                    PrivilegeDescription = "CHENNAI",
                    PrivilegeType = "REVOKE",
                    SafetyCheckLastEntryBy = 1,
                    SafetyCheckLastEntry = DateTime.Now,
                    SafetyCheckIntervalMinutes = 10,
                    HousingUnitLocation = "FLOOR1",
                    HousingUnitNumber = "UP-A",
                    ShowInAppointments = true,
                    ShowInProgram = 1,
                    SafetyCheckFlag = 1,
                    LocationOutOfService = 1,
                    LocationOutOfServiceReason = "ELECTRICAL WORK IS GOING ON",
                    ShowInVisitation = true,
                    AppointmentHierarchyId = 5,
                    ShowInHearing = 1,
                    VisitOpenScheduleFlag = true
                },
                new Privileges
                {
                    PrivilegeId = 6,
                    InactiveFlag = 0,
                    FacilityId = 2,
                    CommisssaryPrivilegeFlag = 0,
                    VisitationPrivilegeFlag = 0,
                    VisitationPhoneFlag = 0,
                    RemoveFromTrackingFlag = 0,
                    RemoveFromPrivilegeFlag = 1,
                    PrivilegeDescription = "MADURAI",
                    PrivilegeType = "REVOKE",
                    SafetyCheckLastEntryBy = 1,
                    SafetyCheckLastEntry = null,
                    SafetyCheckIntervalMinutes = 10,
                    HousingUnitLocation = "FLOOR2",
                    HousingUnitNumber = "UP-B",
                    AppointmentRequireCourtLink = true,
                    ShowInAppointments = true,
                    ShowInProgram = 1,
                    SafetyCheckFlag = 1,
                    Capacity = 5,
                    ShowInVisitation = true,
                    AppointmentHierarchyId = 6,
                    TrackingAllowRefusal = 1,
                    ShowInHearing = 0
                },
                new Privileges
                {
                    PrivilegeId = 7,
                    InactiveFlag = 0,
                    FacilityId = 1,
                    CommisssaryPrivilegeFlag = 0,
                    VisitationPrivilegeFlag = 0,
                    VisitationPhoneFlag = 0,
                    RemoveFromTrackingFlag = 1,
                    RemoveFromPrivilegeFlag = 0,
                    PrivilegeDescription = "TRICHY",
                    PrivilegeType = "AUTH",
                    SafetyCheckLastEntryBy = 1,
                    SafetyCheckLastEntry = DateTime.Now,
                    SafetyCheckIntervalMinutes = 10,
                    HousingUnitLocation = "FLOOR2",
                    HousingUnitNumber = "DOWN-A",
                    ShowInAppointments = true,
                    ShowInProgram = 1,
                    SafetyCheckFlag = 1,
                    AppointmentHierarchyId = 5,
                    TrackingAllowRefusal = 1,
                    ShowInHearing = 1,
                    TransferFlag = 1,
                    ShowInVisitation = true
                },
                new Privileges
                {
                    PrivilegeId = 8,
                    InactiveFlag = 0,
                    FacilityId = 2,
                    CommisssaryPrivilegeFlag = 0,
                    VisitationPrivilegeFlag = 0,
                    VisitationPhoneFlag = 0,
                    RemoveFromTrackingFlag = 0,
                    PrivilegeDescription = "COIMBATORE",
                    PrivilegeType = "REVOKE",
                    SafetyCheckLastEntryBy = 1,
                    SafetyCheckLastEntry = DateTime.Now.AddHours(-2),
                    SafetyCheckIntervalMinutes = 10,
                    HousingUnitLocation = null,
                    HousingUnitNumber = null,
                    ShowInAppointments = true,
                    ShowInProgram = 1,
                    SafetyCheckFlag = 1,
                    AppointmentHierarchyId = 7,
                    TrackingAllowRefusal = 0,
                    ShowInHearing = null,
                    ShowInVisitation = false
                },
                new Privileges
                {
                    PrivilegeId = 9,
                    InactiveFlag = 0,
                    FacilityId = 1,
                    CommisssaryPrivilegeFlag = 0,
                    VisitationPrivilegeFlag = 0,
                    VisitationPhoneFlag = 0,
                    RemoveFromTrackingFlag = 0,
                    PrivilegeDescription = "PUDUKKOTTAI",
                    PrivilegeType = "REVOKE",
                    SafetyCheckLastEntryBy = 1,
                    SafetyCheckLastEntry = DateTime.Now,
                    SafetyCheckIntervalMinutes = 10,
                    HousingUnitLocation = "FLOOR1",
                    HousingUnitNumber = "DOWN-B",
                    ShowInAppointments = true,
                    AppointmentHierarchyId = 7,
                    TrackingAllowRefusal = 0,
                    ShowInVisitation = false

                },
                new Privileges
                {
                    PrivilegeId = 10,
                    FacilityId = null,
                    CommisssaryPrivilegeFlag = 0,
                    VisitationPrivilegeFlag = 0,
                    VisitationPhoneFlag = 0,
                    RemoveFromTrackingFlag = 0,
                    PrivilegeDescription = "VELLORE",
                    PrivilegeType = "AUTH",
                    SafetyCheckLastEntryBy = 1,
                    SafetyCheckLastEntry = DateTime.Now.AddHours(-2),
                    SafetyCheckIntervalMinutes = 10,
                    HousingUnitLocation = "FLOOR1",
                    HousingUnitNumber = "UP-B",
                    ShowInAppointments = true,
                    ShowInProgram = 1,
                    SafetyCheckFlag = 1,
                    AppointmentHierarchyId = 5,
                    TrackingAllowRefusal = 1,
                    TransferFlag = 1,
                    ShowInVisitation = false
                },
                new Privileges
                {
                    PrivilegeId = 11,
                    FacilityId = null,
                    CommisssaryPrivilegeFlag = 0,
                    VisitationPrivilegeFlag = 0,
                    VisitationPhoneFlag = 0,
                    RemoveFromTrackingFlag = 0,
                    PrivilegeDescription = "NAMAKKAL",
                    PrivilegeType = "AUTH",
                    SafetyCheckLastEntryBy = 1,
                    SafetyCheckLastEntry = DateTime.Now.AddHours(-2),
                    SafetyCheckIntervalMinutes = 10,
                    HousingUnitLocation = "FLOOR1",
                    HousingUnitNumber = "UP-B",
                    ShowInAppointments = true,
                    ShowInProgram = 1,
                    SafetyCheckFlag = 1,
                    AppointmentHierarchyId = 5,
                    TrackingAllowRefusal = 1,
                    TransferFlag = null,
                    HoldLocation = 1,
                    InactiveFlag = 0,
                    ShowInVisitation = false
                }
            );


            Db.PrivilegeFlagLookup.AddRange(
                new PrivilegeFlagLookup
                {
                    PrivilegeFlagLookupId = 15,
                    PrivilegeId = 6,
                    CreateDate = DateTime.Now.AddDays(-10),
                    CreateBy = 12,
                    InactiveFlag = 0,
                    FlagName = "Status Active"
                },
                new PrivilegeFlagLookup
                {
                    PrivilegeFlagLookupId = 16,
                    PrivilegeId = 6,
                    CreateDate = DateTime.Now.AddDays(-10),
                    CreateBy = 11,
                    InactiveFlag = 0,
                    FlagName = "Status Inactive"
                }

                );


            Db.TempHold.AddRange(
                new TempHold
                {
                    TempHoldId = 10,
                    CreateDate = DateTime.Now.AddDays(-10),
                    UpdateDate = DateTime.Now.AddDays(-1),
                    CreateBy = 11,
                    FacilityId = 2,
                    UpdateBy = 12,
                    TempHoldLocationId = 6,
                    TempHoldCompleteBy = null
                },
                new TempHold
                {
                    TempHoldId = 11,
                    CreateDate = DateTime.Now.AddDays(-10),
                    UpdateDate = DateTime.Now.AddDays(-1),
                    CreateBy = 12,
                    FacilityId = 1,
                    UpdateBy = 12,
                    TempHoldLocationId = 5,
                    TempHoldCompleteBy = 12,
                    TempHoldCompleteNote = "RELEASE OUT"
                },
                new TempHold
                {
                    TempHoldId = 12,
                    CreateDate = DateTime.Now.AddDays(-10),
                    UpdateDate = DateTime.Now.AddDays(-1),
                    CreateBy = 12,
                    FacilityId = 1,
                    UpdateBy = 12,
                    TempHoldLocationId = 6,
                    TempHoldCompleteBy = 11,
                    TempHoldCompleteNote = null
                },
                new TempHold
                {
                    TempHoldId = 13,
                    CreateDate = DateTime.Now.AddDays(-10),
                    UpdateDate = DateTime.Now.AddDays(-1),
                    CreateBy = 11,
                    FacilityId = 1,
                    UpdateBy = 11,
                    TempHoldLocationId = 7,
                    TempHoldCompleteBy = 11,
                    TempHoldCompleteNote = null,
                    TempHoldType = 5,
                    TempHoldDisposition = 4,
                    TempHoldDateIn = DateTime.Now

                });

            Db.WarehouseItem.AddRange(
                new WarehouseItem
                {
                    WarehouseItemId = 10,
                    CreateDate = DateTime.Now.AddDays(-10),
                    DeleteFlag = null,
                    DeleteDate = null,
                    DeletedBy = null,
                    FacilityId = 1,
                    CreatedBy = 12,
                    ItemCategory = "MISC",
                    ItemName = "BANGLE",
                    ItemDescription = "MISC"

                },
                new WarehouseItem
                {
                    WarehouseItemId = 11,
                    CreateDate = DateTime.Now.AddDays(-10),
                    DeleteFlag = null,
                    DeleteDate = null,
                    DeletedBy = null,
                    FacilityId = 1,
                    CreatedBy = 12,
                    ItemCategory = "MISC",
                    ItemName = "RING",
                    ItemDescription = "RING"
                }
            );

            Db.WarehouseRequest.AddRange(
                new WarehouseRequest
                {
                    CreateDate = DateTime.Now.AddDays(-10),
                    DeleteFlag = null,
                    FacilityId = 1,
                    WarehouseItemId = 11,
                    WarehouseRequestId = 10,
                    UpdateDate = DateTime.Now,
                    UpdatedBy = 12,
                    DeleteDate = null,
                    DeletedBy = null,
                    CreatedBy = 12,
                    HousingBuilding = "FLOOR2",
                    HousingNumber = "UP-B",
                    CompleteBy = 11,
                    RequestedBy = 12
                },
                new WarehouseRequest
                {
                    CreateDate = DateTime.Now.AddDays(-10),
                    DeleteFlag = 0,
                    DeleteDate = null,
                    DeletedBy = null,
                    FacilityId = 2,
                    WarehouseItemId = 10,
                    WarehouseRequestId = 11,
                    UpdateDate = DateTime.Now,
                    UpdatedBy = 12,
                    CreatedBy = 12,
                    HousingBuilding = "FLOOR1",
                    HousingNumber = "UP-B"
                }
            );

            Db.FloorNotes.AddRange(
                new FloorNotes
                {
                    FloorNoteId = 10,
                    FloorNoteDate = DateTime.Now,
                    FloorNoteType = "GENDER CHECK",
                    FloorNoteNarrative = "INMATE GENDER CONFLICT",
                    FloorNoteLocation = "MADURAI",
                    FloorNoteLocationId = 6,
                    FloorNoteOfficerId = 11,
                    FloorNoteTime = "12.30"
                },
                new FloorNotes
                {
                    FloorNoteId = 20,
                    FloorNoteDate = DateTime.Now,
                    FloorNoteType = "VISITOR CONFLICT CHECK",
                    FloorNoteNarrative = "VISITOR FOR JAIL INMATE",
                    FloorNoteLocation = "CHENNAI",
                    FloorNoteLocationId = null,
                    FloorNoteOfficerId = 11,
                    FloorNoteTime = "5.10"
                },
                new FloorNotes
                {
                    FloorNoteId = 25,
                    FloorNoteDate = DateTime.Now,
                    FloorNoteType = "NO TYPE",
                    FloorNoteNarrative = "KEEPSEPERATE THE INMATE",
                    FloorNoteLocation = "CHENNAI",
                    FloorNoteLocationId = 5,
                    FloorNoteOfficerId = 12,
                    FloorNoteTime = "2.43"
                },
                new FloorNotes
                {
                    FloorNoteId = 26,
                    FloorNoteDate = DateTime.Now,
                    FloorNoteType = "ALL",
                    FloorNoteNarrative = "ALL TYPES",
                    FloorNoteLocation = "TRICHY",
                    FloorNoteLocationId = 7,
                    FloorNoteOfficerId = 13,
                    FloorNoteTime = "12.20"
                },
                new FloorNotes
                {
                    FloorNoteId = 27,
                    FloorDeleteFlag = 0,
                    FloorNoteDate = DateTime.Now,
                    FloorNoteType = "LOCATION NOTE",
                    FloorNoteLocationId = null,
                    FloorNoteTime = "08.40",
                    FloorNoteOfficerId = 12
                }
            );
            Db.FloorNoteXref.AddRange(
                new FloorNoteXref
                {
                    FloorNoteXrefId = 5,
                    FloorNoteId = 10,
                    CreateDate = DateTime.Now.AddDays(-3),
                    UpdateDate = DateTime.Now,
                    InmateId = 100
                },
                new FloorNoteXref
                {
                    FloorNoteXrefId = 10,
                    FloorNoteId = 20,
                    CreateDate = DateTime.Now.AddDays(-2),
                    UpdateDate = DateTime.Now,
                    InmateId = 101
                },
                new FloorNoteXref
                {
                    FloorNoteXrefId = 11,
                    FloorNoteId = 25,
                    CreateDate = DateTime.Now.AddDays(-2),
                    UpdateDate = DateTime.Now,
                    InmateId = 130
                },
                new FloorNoteXref
                {
                    FloorNoteXrefId = 12,
                    FloorNoteId = 20,
                    CreateDate = DateTime.Now.AddDays(-1),
                    UpdateDate = DateTime.Now.AddDays(-1),
                    InmateId = 130
                },
                new FloorNoteXref
                {
                    FloorNoteXrefId = 13,
                    FloorNoteId = 20,
                    CreateDate = DateTime.Now,
                    UpdateDate = DateTime.Now,
                    InmateId = 110
                },
                new FloorNoteXref
                {
                    FloorNoteXrefId = 14,
                    FloorNoteId = 10,
                    CreateDate = DateTime.Now,
                    UpdateDate = DateTime.Now,
                    InmateId = 110
                },
                new FloorNoteXref
                {
                    FloorNoteXrefId = 15,
                    FloorNoteId = 26,
                    CreateDate = DateTime.Now,
                    UpdateDate = DateTime.Now,
                    InmateId = 120
                }
            );
            Db.Address.AddRange(
                new Address
                {
                    AddressId = 10,
                    PersonId = 50,
                    AddressNumber = "ADDS_4000",
                    AddressStreet = "VARA STREET",
                    AddressSuffix = "VS",
                    AddressState = "ANDHRA PRADESH",
                    AddressType = "RES",
                    AddressZip = "045212235",
                    AddressDirection = "EAST",
                    AddressDirectionSuffix = "E",
                    AddressCity = "KADAPA",
                    CreatedBy = 12,
                    CreateDate = DateTime.Now,
                    UpdateBy = 11,
                    UpdateDate = DateTime.Now
                },
                new Address
                {
                    AddressId = 11,
                    PersonId = 55,
                    AddressNumber = "ADDS_5321",
                    AddressStreet = "THIRUVALLUVAR NAGAR",
                    AddressSuffix = "TN",
                    AddressState = "TAMIL NADU",
                    AddressType = "BUS",
                    AddressZip = "6000028"
                },
                new Address
                {
                    AddressId = 12,
                    PersonId = 60,
                    AddressNumber = "ADDS_741",
                    AddressStreet = "PALANIYAPPA NAGAR",
                    AddressSuffix = "PAN",
                    AddressState = "TAMIL NADU",
                    AddressType = "MAIL",
                    AddressZip = "6000028",
                    CreateDate = DateTime.Now.AddDays(-1),
                    UpdateDate = DateTime.Now,
                    CreatedBy = 11,
                    UpdateBy = 11
                },
                new Address
                {
                    AddressId = 13,
                    PersonId = 60,
                    AddressNumber = "ADDS_4145",
                    AddressStreet = "NANMAGALAM",
                    AddressSuffix = "RES",
                    AddressState = "ANDRA PRADESH",
                    AddressType = "RES",
                    AddressZip = "6000028",
                    CreateDate = DateTime.Now,
                    UpdateDate = DateTime.Now,
                    CreatedBy = 12,
                    UpdateBy = 11
                },
                new Address
                {
                    AddressId = 14,
                    PersonId = 50,
                    AddressNumber = "ADDS_4178",
                    AddressStreet = "CHANDRA NAGAR",
                    AddressSuffix = "CN",
                    AddressState = "TAMIL NADU",
                    AddressType = "BUS",
                    AddressZip = "6000078",
                    CreateDate = DateTime.Now,
                    UpdateDate = DateTime.Now,
                    CreatedBy = 11,
                    UpdateBy = 11,
                    AddressDirection = "EAST",
                    AddressDirectionSuffix = "E",
                    AddressCity = "RAMESHWARAM"
                });
        }


    }
}
