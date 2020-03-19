using System;
using GenerateTables.Models;

// ReSharper disable once CheckNamespace
namespace ServerAPI.Tests
{
    public partial class DbInitialize
    {
        private void AltSentdetails()
        {
            Db.AltSent.AddRange(
                new AltSent
                {
                    AltSentId = 5,
                    IncarcerationId = 11,
                    AltSentProgramId = 10,
                    DefaultMonAltSentSiteAssignId = 15,
                    PrimaryAltSentSiteId = 5,
                    DefaultThuAltSentSiteAssignId =15

                },
                new AltSent
                {
                    AltSentId = 6,
                    IncarcerationId = 10,
                    AltSentProgramId = 10,
                    DefaultMonAltSentSiteAssignId = 10,
                    PrimaryAltSentSiteId = 5,
                    DefaultThuAltSentSiteAssignId = 16
                },
                new AltSent
                {
                    AltSentId = 7,
                    IncarcerationId = 12,
                    AltSentProgramId = 11,
                    DefaultMonAltSentSiteAssignId = 13,
                    PrimaryAltSentSiteId = 6
                },
                new AltSent
                {
                    AltSentId = 8,
                    IncarcerationId = 13,
                    AltSentProgramId = 12,
                    DefaultMonAltSentSiteAssignId = 16,
                    PrimaryAltSentSiteId = 6
                },
                new AltSent
                {
                    AltSentId = 9,
                    IncarcerationId = 31,
                    AltSentProgramId = 12,
                    DefaultMonAltSentSiteAssignId = 16,
                    PrimaryAltSentSiteId = 5,
                    AltSentClearFlag = 0
                }
            );

            Db.AltSentProgram.AddRange(
                new AltSentProgram
                {
                    AltSentProgramId = 10,
                    AltSentProgramAbbr = "EMS",
                    AltSentProgramName = "ELECTRONIC MANAGEMENT SERVICE",
                    CreateDate = DateTime.Now.AddDays(-3),
                    CreatedBy = 11,
                    UpdatedBy = 13,
                    UpdateDate = DateTime.Now.AddDays(-3),
                    FacilityId = 1,
                    InactiveFlag = 0

                },
                new AltSentProgram
                {
                    AltSentProgramId = 11,
                    AltSentProgramAbbr = "CELL",
                    AltSentProgramName = "ASUS",
                    CreateDate = DateTime.Now.AddDays(-3),
                    CreatedBy = 11,
                    UpdatedBy = 11,
                    UpdateDate = DateTime.Now.AddDays(-3),
                    FacilityId = 2,
                    InactiveFlag = 1
                },
                new AltSentProgram
                {
                    AltSentProgramId = 12,
                    AltSentProgramAbbr = "SW",
                    AltSentProgramName = "SWIFT WORK",
                    CreateDate = DateTime.Now.AddDays(-2),
                    CreatedBy = 12,
                    UpdatedBy = 11,
                    UpdateDate = DateTime.Now.AddDays(-1),
                    FacilityId = 2,
                    InactiveFlag = 1
                },
                new AltSentProgram
                {
                    AltSentProgramId = 13,
                    AltSentProgramAbbr = "GW",
                    AltSentProgramName = "GARDERN WORK",
                    CreateDate = DateTime.Now.AddDays(-3),
                    CreatedBy = 11,
                    UpdatedBy = 11,
                    UpdateDate = DateTime.Now.AddDays(-3),
                    FacilityId = 1
                }
            );
            Db.AltSentRequestProgram.AddRange(
                new AltSentRequestProgram
                {
                    AltSentRequestProgramId = 15,
                    AltSentProgramId = 12,
                    AltSentRequestId = 10
                }
                );

            Db.AltSentSite.AddRange(
                new AltSentSite
                {
                    AltSentSiteId = 5,
                    AltSentSiteName = "GOODS/M",
                    AltSentInmateInstructions = "ONLY FEMALE",
                    AltSentProgramId = 10,
                    InactiveFlag = 0
                },
                new AltSentSite
                {
                    AltSentSiteId = 6,
                    AltSentSiteName = "GOODS/N",
                    AltSentInmateInstructions = "NEW HOUSE",
                    AltSentProgramId = 12,
                    InactiveFlag = 0
                },
                new AltSentSite
                {
                    AltSentSiteId = 7,
                    AltSentSiteName = "PRAYER",
                    AltSentInmateInstructions = "READY FOR PRAYER",
                    AltSentProgramId = 10,
                    InactiveFlag = 0
                }
            );

            Db.AltSentSiteSchd.AddRange(
                new AltSentSiteSchd
                {
                   AltSentSiteSchdId = 15,
                   AltSentSiteId = 5,
                   InactiveFlag = null,
                   AltSentSiteSchdDayOfWeek = 5,
                   AltSentSiteSchdDescription = "SEND REQUEST"
                },
                new AltSentSiteSchd
                {
                    AltSentSiteSchdId = 16,
                    AltSentSiteId = 5,
                    InactiveFlag = null,
                    AltSentSiteSchdDayOfWeek = 5,
                    AltSentSiteSchdDescription = "KEEP IT SAME REQUEST"
                }
            );

            Db.AltSentRequest.AddRange(

                new AltSentRequest
                {
                    AltSentRequestId = 10,
                    CreateBy = 13,
                    UpdateBy = 12,
                    CreateDate = DateTime.Now.AddDays(-5),
                    UpdateDate = DateTime.Now.AddDays(-4),
                    InmateId = 100,
                    SchedReqBookDate = DateTime.Now,
                    FacilityId = 2,
                    RequestPersonnelId = 1
                },
                new AltSentRequest
                {
                    AltSentRequestId = 15,
                    CreateBy = 11,
                    UpdateBy = 13,
                    CreateDate = DateTime.Now.AddDays(-4),
                    UpdateDate = DateTime.Now.AddDays(-4),
                    InmateId = 110,
                    SchedReqBookDate = DateTime.Now,
                    FacilityId = 1,
                    RequestPersonnelId = 15
                },
                new AltSentRequest
                {
                    AltSentRequestId = 16,
                    CreateBy = 12,
                    UpdateBy = 11,
                    CreateDate = DateTime.Now.AddDays(-4),
                    UpdateDate = DateTime.Now.AddDays(-2),
                    InmateId = 108,
                    SchedReqBookDate = DateTime.Now,
                    FacilityId = 2,
                    RequestPersonnelId = 15,
                    DeleteFlag = 1
                },
                new AltSentRequest
                {
                    AltSentRequestId = 17,
                    CreateBy = 12,
                    UpdateBy = 11,
                    CreateDate = DateTime.Now.AddDays(-3),
                    UpdateDate = DateTime.Now.AddDays(-2),
                    InmateId = 108,
                    SchedReqBookDate = DateTime.Now,
                    FacilityId = 1,
                    RequestPersonnelId = 11
                },
                new AltSentRequest
                {
                    AltSentRequestId = 18,
                    CreateBy = 13,
                    UpdateBy = 12,
                    CreateDate = DateTime.Now.AddDays(-2),
                    UpdateDate = DateTime.Now.AddDays(-1),
                    InmateId = 110,
                    SchedReqBookDate = DateTime.Now,
                    FacilityId = 2,
                    RequestPersonnelId = 12
                },
                new AltSentRequest
                {
                    AltSentRequestId = 19,
                    CreateDate = DateTime.Now.AddDays(-2),
                    UpdateDate = DateTime.Now,
                    ApprovedBy = 12,
                    InmateId = 120,
                    FacilityId = 2,
                    CreateBy = 12,
                    UpdateBy = 11,
                    RequestPersonnelId = 12
                }

                );

            Db.AltSentRequestAppeal.AddRange(
                new AltSentRequestAppeal
                {
                    AltSentRequestAppealId = 10,
                    CreatedBy = 13,
                    UpdateBy = 12,
                    CreateDate = DateTime.Now.AddDays(-5),
                    UpdateDate = DateTime.Now,
                    AltSentRequestId = 10,
                    AppealNumber = 15100,
                    ApprovedBy = 12,
                    ApprovalNote = "WAITING FOR REQUEST"
                },
                 new AltSentRequestAppeal
                 {
                     AltSentRequestAppealId = 11,
                     CreatedBy = 12,
                     UpdateBy = 12,
                     CreateDate = DateTime.Now.AddDays(-4),
                     UpdateDate = DateTime.Now.AddDays(-4),
                     AltSentRequestId = 19,
                     AppealNumber = 15101,
                     ApprovedBy = 13,
                     ApprovalNote = "LAW REQUEST APPROED"
                 }

                );

            Db.AltSentRequestHistory.AddRange(

                new AltSentRequestHistory
                {
                    AltSentRequestHistoryId = 10,
                    CreateDate = DateTime.Now.AddDays(-4),
                    PersonnelId = 11,
                    AltSentRequestId = 10,
                    AltSentHistoryList = "{'FACILITY':'TJ', 'INMATE':'HEMA','REQUEST DATE':'05/02/2017 10:59:36 AM', 'REQUEST NOTE':'BARIA REQUEST CREATION' }"
                },
                new AltSentRequestHistory
                {
                    AltSentRequestHistoryId = 11,
                    CreateDate = DateTime.Now.AddDays(-4),
                    PersonnelId = 13,
                    AltSentRequestId = 15,
                    AltSentHistoryList = "{'FACILITY':'MCJ', 'INMATE':'VINO','REQUEST DATE':'07/15/2017 10:59:36 AM', 'PERSONNEL':'NARESH' }"
                }
                );
            Db.AltSentPrimarySiteSaveHistory.AddRange(
                new AltSentPrimarySiteSaveHistory
                {
                    AltSentPrimarySiteSaveHistoryId = 50,
                    PrimaryAltSentSiteId = 5,
                    AvailableMon = true,
                    AvailableTue = false,
                    AvailableWed = false,
                    AvailableThu = false,
                    AvailableFri = true,
                    AvailableSat = false,
                    AvailableSun = false,
                    SaveBy = 11,
                    SaveDate = DateTime.Now,
                    DefaultFriAltSentSiteAssignId = 1,
                    DefaultMonAltSentSiteAssignId = 0,
                    DefaultTueAltSentSiteAssignId = 0,
                    DefaultWedAltSentSiteAssignId = 0,
                    DefaultThuAltSentSiteAssignId = 0,
                    DefaultSatAltSentSiteAssignId = 0,
                    DefaultSunAltSentSiteAssignId = 0,
                    AltSentId = 10
                }
                );

        }
    }
}
