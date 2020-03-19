using GenerateTables.Models;
using System;

// ReSharper disable once CheckNamespace
namespace ServerAPI.Tests
{
    public partial class DbInitialize
    {
        private void RequestDetails()
        {
            Db.Request.AddRange(
                new Request
                {
                    RequestId = 5,
                    RequestActionLookupId = 10,
                    CreateDate = DateTime.Now.AddDays(-20),
                    UpdateDate = DateTime.Now.AddDays(-19),
                    CreatedBy = 12,
                    UpdatedBy = 11,
                    InmateId = 100,
                    RequestedBy = 11,
                    RequestDate = DateTime.Now,
                    RequestNote = "MOVE HOUSING REQUEST"
                },
                new Request
                {
                    RequestId = 6,
                    RequestActionLookupId = 15,
                    CreateDate = DateTime.Now.AddDays(-20),
                    UpdateDate = DateTime.Now.AddDays(-19),
                    CreatedBy = 11,
                    UpdatedBy = 11,
                    InmateId = 101,
                    RequestNote = "CLASSIFICATION REQUEST",
                    PendingBy = 11,
                    RequestedBy = 11
                },
                new Request
                {
                    RequestId = 7,
                    RequestActionLookupId = 16,
                    CreateDate = DateTime.Now.AddDays(-20),
                    UpdateDate = DateTime.Now.AddDays(-19),
                    CreatedBy = 12,
                    UpdatedBy = 11,
                    InmateId = 106,
                    RequestDate = DateTime.Now,
                    RequestNote = "INMATE REQUEST",
                    RequestedBy = 12
                },
                new Request
                {
                    RequestId = 8,
                    RequestActionLookupId = 17,
                    CreateDate = DateTime.Now.AddDays(-20),
                    UpdateDate = DateTime.Now.AddDays(-19),
                    CreatedBy = 1,
                    UpdatedBy = 11,
                    InmateId = 106,
                    RequestDate = DateTime.Now.AddDays(-1),
                    RequestNote = "PROGRAM REQUEST",
                    RequestedBy = 11
                },
                new Request
                {
                    RequestId = 9,
                    RequestActionLookupId = 17,
                    CreateDate = DateTime.Now.AddDays(-20),
                    UpdateDate = DateTime.Now.AddDays(-19),
                    CreatedBy = 11,
                    UpdatedBy = 11,
                    InmateId = 103,
                    RequestDate = DateTime.Now.AddDays(-1),
                    RequestNote = "HOUSING REQUEST",
                    RequestedBy = 12
                },
                new Request
                {
                    RequestId = 10,
                    RequestActionLookupId = 18,
                    CreateDate = DateTime.Now.AddDays(-25),
                    UpdateDate = DateTime.Now.AddDays(-19),
                    CreatedBy = 12,
                    UpdatedBy = 11,
                    InmateId = 105,
                    RequestDate = DateTime.Now.AddDays(-1),
                    RequestNote = "INMATE REQUEST",
                    RequestedBy = 12
                },
                new Request
                {
                    RequestId = 11,
                    RequestActionLookupId = 15,
                    CreateDate = DateTime.Now.AddDays(-21),
                    UpdateDate = DateTime.Now.AddDays(-17),
                    CreatedBy = 12,
                    UpdatedBy = 11,
                    InmateId = 108,
                    RequestDate = DateTime.Now.AddDays(-1),
                    RequestNote = "PROGRAM REQUEST",
                    RequestedBy = 12,
                    PendingBy = 11
                },
                new Request
                {
                    RequestId = 12,
                    RequestActionLookupId = 19,
                    CreateDate = DateTime.Now.AddDays(-22),
                    UpdateDate = DateTime.Now.AddDays(-18),
                    CreatedBy = 12,
                    InmateId = 106,
                    RequestDate = DateTime.Now,
                    RequestNote = "INMATE REQUEST",
                    RequestedBy = 11
                },
                new Request
                {
                    RequestId = 13,
                    RequestActionLookupId = 19,
                    CreatedBy = 12,
                    CreateDate = DateTime.Now.AddDays(-20),
                    UpdateDate = DateTime.Now.AddDays(-10),
                    UpdatedBy = 11,
                    PendingBy = 11,
                    RequestedBy = 11,
                    InmateId = 110
                },
                new Request
                {
                    RequestId = 14,
                    RequestActionLookupId = 18,
                    CreatedBy = 12,
                    CreateDate = DateTime.Now.AddDays(-20),
                    UpdateDate = DateTime.Now.AddDays(-10),
                    UpdatedBy = 13,
                    PendingBy = 11,
                    RequestedBy = 11,
                    InmateId = 120
                },
                new Request
                {
                    RequestId = 15,
                    RequestActionLookupId = 10,
                    CreatedBy = 11,
                    CreateDate = DateTime.Now.AddDays(-20),
                    UpdateDate = DateTime.Now.AddDays(-9),
                    UpdatedBy = 11,
                    PendingBy = 11,
                    RequestedBy = 11,
                    InmateId = 110
                },
                new Request
                {
                    RequestId = 16,
                    RequestActionLookupId = 16,
                    CreatedBy = 11,
                    CreateDate = DateTime.Now,
                    UpdateDate = DateTime.Now.AddDays(-10),
                    UpdatedBy = 12,
                    ClearedBy = null,
                    InmateId = 110,
                    HousingUnitListId = 12,
                    RequestedBy = 12,
                    RequestNote = "INMATE REQUEST"
                },
                new Request
                {
                    RequestId = 17,
                    RequestActionLookupId = 18,
                    CreatedBy = 11,
                    CreateDate = DateTime.Now,
                    UpdateDate = DateTime.Now.AddDays(-10),
                    UpdatedBy = 12,
                    ClearedBy = null,
                    InmateId = 110,
                    HousingUnitListId = 12,
                    RequestedBy = 12,
                    PendingBy = 11,
                    RequestNote = null
                },
                new Request
                {
                    RequestId = 18,
                    RequestActionLookupId = 16,
                    CreatedBy = 11,
                    CreateDate = DateTime.Now,
                    UpdateDate = DateTime.Now.AddDays(-10),
                    UpdatedBy = 12,
                    ClearedBy = null,
                    InmateId = 110,
                    HousingUnitListId = 12,
                    RequestedBy = 12,
                    RequestNote = "INMATE REQUEST"
                },
                new Request
                {
                    RequestId = 19,
                    RequestActionLookupId = 20,
                    CreatedBy = 11,
                    CreateDate = DateTime.Now,
                    UpdateDate = DateTime.Now.AddDays(-10),
                    UpdatedBy = 12,
                    ClearedBy = null,
                    InmateId = 110,
                    HousingUnitListId = 12,
                    RequestedBy = 12,
                    PendingBy = 11,
                    RequestNote = "BROKEN WINDOW REQUEST"
                });

            Db.RequestActionLookup.AddRange(
                new RequestActionLookup
                {
                    RequestActionLookupId = 10,
                    CreatedBy = 12,
                    UpdatedBy = 12,
                    CreateDate = DateTime.Now.AddDays(-2),
                    UpdateDate = DateTime.Now,
                    ActionLookup = "VISITOR REQUEST",
                    InactiveFlag = false,
                    RequestByInmate = true,
                    ShowInFlag = 7,
                    PendingAllFacilityFlag = true,
                    FacilityId = 2
                },
                new RequestActionLookup
                {
                    RequestActionLookupId = 15,
                    CreatedBy = 11,
                    UpdatedBy = 12,
                    CreateDate = DateTime.Now.AddDays(-5),
                    UpdateDate = DateTime.Now.AddDays(-1),
                    ActionLookup = "PROGRAM REQUEST",
                    InactiveFlag = false,
                    RequestByInmate = true,
                    ShowInFlag = 7,
                    PendingAllFacilityFlag = true,
                    FacilityId = 1
                },
                new RequestActionLookup
                {
                    RequestActionLookupId = 16,
                    CreatedBy = 12,
                    UpdatedBy = 12,
                    CreateDate = DateTime.Now.AddDays(-2),
                    UpdateDate = DateTime.Now,
                    ActionLookup = "HOUSING MOVE",
                    InactiveFlag = false,
                    RequestByInmate = true,
                    FacilityId = 2,
                    ShowInFlag = 5
                },
                new RequestActionLookup
                {
                    RequestActionLookupId = 17,
                    CreatedBy = 11,
                    UpdatedBy = 12,
                    CreateDate = DateTime.Now.AddDays(-3),
                    UpdateDate = DateTime.Now,
                    ActionLookup = "CLASSIFICATION REQUEST",
                    InactiveFlag = false,
                    RequestByInmate = true,
                    FacilityId = 1,
                    PendingAllFacilityFlag = true,
                    RequestFacilityId = 1,
                    ShowInFlag = 0
                },
                new RequestActionLookup
                {
                    RequestActionLookupId = 18,
                    CreatedBy = 12,
                    UpdatedBy = 12,
                    CreateDate = DateTime.Now.AddDays(-2),
                    UpdateDate = DateTime.Now,
                    ActionLookup = "VISITATION REQUEST",
                    InactiveFlag = false,
                    RequestByInmate = true,
                    FacilityId = 1,
                    PendingAllFacilityFlag = true,
                    RequestFacilityId = 1,
                    ShowInFlag = 1
                },
                new RequestActionLookup
                {
                    RequestActionLookupId = 19,
                    CreatedBy = 13,
                    UpdatedBy = 12,
                    CreateDate = DateTime.Now.AddDays(-1),
                    UpdateDate = DateTime.Now,
                    ActionLookup = "INITIAL PROGRAM",
                    InactiveFlag = false,
                    RequestByInmate = true,
                    FacilityId = 2,
                    PendingAllFacilityFlag = true,
                    RequestFacilityId = 1,
                    ShowInFlag = 3
                },
                new RequestActionLookup
                {
                    RequestActionLookupId = 20,
                    CreatedBy = 12,
                    UpdatedBy = 11,
                    CreateDate = DateTime.Now.AddDays(-1),
                    UpdateDate = DateTime.Now,
                    ActionLookup = "BROKEN WINDOW",
                    InactiveFlag = false,
                    RequestByInmate = true,
                    FacilityId = 1,
                    PendingAllFacilityFlag = true,
                    RequestFacilityId = 1,
                    ShowInFlag = 2
                }
            );
            Db.RequestTrack.AddRange(
                new RequestTrack
                {
                    RequestTrackId = 15,
                    RequestActionLookupId = 16,
                    RequestId = 5,
                    ResponseInmateFlag = 1,
                    ResponseInmateReadFlag = 0,
                    RequestTrackCategory = "PROGRAM REQUEST",
                    RequestTrackDate = DateTime.Now,
                    ResponseDisposition = "ACCESS DENIED",
                    RequestTrackBy = 11
                },
                new RequestTrack
                {
                    RequestTrackId = 16,
                    RequestActionLookupId = 17,
                    RequestId = 6,
                    ResponseInmateFlag = 1,
                    ResponseInmateReadFlag = 0,
                    RequestTrackCategory = "TRANSFER APPROVED",
                    RequestTrackDate = DateTime.Now,
                    ResponseDisposition = "REQUEST CANCELLED",
                    RequestTrackBy = 12
                },
                new RequestTrack
                {
                    RequestTrackId = 17,
                    RequestActionLookupId = 17,
                    RequestId = 10,
                    ResponseInmateFlag = 1,
                    ResponseInmateReadFlag = 0,
                    RequestTrackCategory = "TRANSFER APPROVED",
                    RequestTrackDate = DateTime.Now,
                    ResponseDisposition = "REQUEST CANCELLED",
                    RequestTrackBy = 11
                   
                }
            );

            Db.RequestActionUserGroup.AddRange(
                new RequestActionUserGroup
                {
                    RequestActionUserGroupId = 5,
                    GroupId = 1,
                    CreateDate = DateTime.Now.AddDays(-2),
                    RequestActionLookupId = 10
                },
                new RequestActionUserGroup
                {
                    RequestActionUserGroupId = 6,
                    GroupId = 1,
                    CreateDate = DateTime.Now.AddDays(-2),
                    RequestActionLookupId = 15
                },
                new RequestActionUserGroup
                {
                    RequestActionUserGroupId = 7,
                    GroupId = 1,
                    CreateDate = DateTime.Now.AddDays(-1),
                    RequestActionLookupId = 19

                },
                new RequestActionUserGroup
                {
                    RequestActionUserGroupId = 8,
                    GroupId = 5,
                    CreateDate = DateTime.Now.AddDays(-1),
                    RequestActionLookupId = 10,
                    RoleId = "11",
                    DeleteFlag = 0
                },
                new RequestActionUserGroup
                {
                    RequestActionUserGroupId = 9,
                    GroupId = 5,
                    CreateDate = DateTime.Now.AddDays(-1),
                    RequestActionLookupId = 19,
                    RoleId = "12",
                    DeleteFlag = 0
                },
                new RequestActionUserGroup
                {
                    RequestActionUserGroupId = 10,
                    GroupId = 5,
                    CreateDate = DateTime.Now.AddDays(-1),
                    RequestActionLookupId = 15,
                    RoleId = "11",
                    DeleteFlag = 0
                }
            );
        }
    }
}
