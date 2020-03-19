using GenerateTables.Models;
using System;

// ReSharper disable once CheckNamespace
namespace ServerAPI.Tests
{
    public partial class DbInitialize
    {
        private void DisciplinaryDetails()
        {
            Db.DisciplinaryInmate.AddRange(
                new DisciplinaryInmate
                {
                    DisciplinaryInmateId = 101,
                    InmateId = 100,
                    DisciplinaryIncidentId = 5,
                    CreateDate = DateTime.Now.AddDays(-5),
                    UpdateDate = DateTime.Now,
                    DeleteFlag = null,
                    DisciplinaryArrestId = 5,
                    DisciplinaryDays = 10,
                    DisciplinaryDaysRemoveBy = 11,
                    DisciplinaryDaysRemoveDate = DateTime.Now,
                    DisciplinaryInmateType = 5
                },
                new DisciplinaryInmate
                {
                    DisciplinaryInmateId = 102,
                    InmateId = 103,
                    DisciplinaryIncidentId = 6,
                    CreateDate = DateTime.Now.AddDays(-3),
                    UpdateDate = DateTime.Now,
                    DeleteFlag = null,
                    DisciplinaryArrestId = 5,
                    DisciplinaryDays = 15,
                    DisciplinaryDaysRemoveBy = 12,
                    DisciplinaryDaysRemoveDate = DateTime.Now,
                    DisciplinaryDaysRemoveFlag = 0,
                    DisciplinaryDaysSentFlag = 1,
                    DisciplinaryInmateType = 5
                },
                new DisciplinaryInmate
                {
                    DisciplinaryInmateId = 103,
                    InmateId = 101,
                    DisciplinaryIncidentId = 6,
                    CreateDate = DateTime.Now.AddDays(-2),
                    UpdateDate = DateTime.Now,
                    DeleteFlag = null,
                    DisciplinaryArrestId = 5,
                    DisciplinaryDays = 25,
                    DisciplinaryDaysRemoveBy = 11,
                    DisciplinaryDaysRemoveDate = DateTime.Now,
                    DisciplinaryDaysRemoveFlag = 0,
                    DisciplinaryDaysSentFlag = 1,
                    DisciplinaryInmateType = 5,
                    DisciplinaryHearingDate = DateTime.Now.AddDays(-3),
                    DisciplinaryScheduleHearingDate = DateTime.Now.AddDays(-1),
                    DisciplinaryInmateBypassHearing = 1,
                    DisciplinaryReviewDate = DateTime.Now,
                    HearingComplete = 1
                },
                new DisciplinaryInmate
                {
                    DisciplinaryInmateId = 104,
                    InmateId = 101,
                    DisciplinaryIncidentId = 7,
                    CreateDate = DateTime.Now.AddDays(-2),
                    UpdateDate = DateTime.Now,
                    DeleteFlag = null,
                    DisciplinaryArrestId = 5,
                    DisciplinaryDays = 25,
                    DisciplinaryDaysRemoveBy = 11,
                    DisciplinaryDaysRemoveDate = DateTime.Now,
                    DisciplinaryInmateType = 15,
                    DisciplinaryHearingDate = DateTime.Now.AddDays(-2),
                    DisciplinaryScheduleHearingDate = DateTime.Now.AddDays(-2),
                    DisciplinaryInmateBypassHearing = 1,
                    DisciplinaryReviewDate = DateTime.Now,
                    HearingComplete = 1,
                    DisciplinaryOtherName = "WAJBAI"
                },
                new DisciplinaryInmate
                {
                    DisciplinaryInmateId = 105,
                    DisciplinaryHearingOfficer1 = 11,
                    DisciplinaryHearingOfficer2 = 12,
                    CreateDate = DateTime.Now.AddDays(-2),
                    UpdateDate = DateTime.Now,
                    DisciplinaryIncidentId = 6,
                    DeleteFlag = 0,
                    DisciplinaryArrestId = null,
                    HearingComplete = 1,
                    DisciplinaryReviewDate = DateTime.Now,
                    DisciplinaryFindingDate = DateTime.Now,
                    DisciplinaryReviewOfficer = 13,
                    DisciplinaryHearingDate = DateTime.Now.AddDays(1),
                    DisciplinaryScheduleHearingDate = DateTime.Now.AddDays(1)
                },
                new DisciplinaryInmate
                {
                    DisciplinaryInmateId = 109,
                    DisciplinaryHearingOfficer1 = 12,
                    DisciplinaryHearingOfficer2 = 12,
                    CreateDate = DateTime.Now.AddDays(-3),
                    UpdateDate = DateTime.Now,
                    DisciplinaryIncidentId = 6,
                    DeleteFlag = 0,
                    DisciplinaryArrestId = null,
                    HearingComplete = 1,
                    DisciplinaryReviewDate = DateTime.Now,
                    DisciplinaryFindingDate = DateTime.Now,
                    DisciplinaryReviewOfficer = 13,
                    DisciplinaryHearingDate = DateTime.Now.AddDays(1),
                    DisciplinaryScheduleHearingDate = DateTime.Now.AddDays(1)
                },
                new DisciplinaryInmate
                {
                    DisciplinaryInmateId = 120,
                    DisciplinaryHearingOfficer1 = 12,
                    DisciplinaryHearingOfficer2 = 12,
                    CreateDate = DateTime.Now.AddDays(-3),
                    UpdateDate = DateTime.Now,
                    DisciplinaryIncidentId = 6,
                    DeleteFlag = 0,
                    DisciplinaryArrestId = null,
                    HearingComplete = 1,
                    DisciplinaryReviewDate = DateTime.Now,
                    DisciplinaryFindingDate = DateTime.Now,
                    DisciplinaryReviewOfficer = 13,
                    DisciplinaryHearingDate = DateTime.Now.AddDays(1),
                    DisciplinaryScheduleHearingDate = DateTime.Now.AddDays(1)
                },
                new DisciplinaryInmate
                {
                    DisciplinaryInmateId = 121,
                    InmateId = 101,
                    DisciplinaryIncidentId = 7,
                    CreateDate = DateTime.Now.AddDays(-2),
                    UpdateDate = DateTime.Now,
                    DeleteFlag = null,
                    DisciplinaryArrestId = 5,
                    DisciplinaryDays = 25,
                    DisciplinaryDaysRemoveBy = 11,
                    DisciplinaryDaysRemoveDate = DateTime.Now,
                    DisciplinaryInmateType = 15,
                    DisciplinaryHearingDate = DateTime.Now.AddDays(-2),
                    DisciplinaryScheduleHearingDate = DateTime.Now.AddDays(-2),
                    DisciplinaryInmateBypassHearing = 1,
                    DisciplinaryReviewDate = DateTime.Now,
                    HearingComplete = 1,
                    DisciplinaryOtherName = "NOTE",
                    NoticePersonnelId = 11
                },
                  new DisciplinaryInmate
                  {
                      DisciplinaryInmateId = 122,
                      InmateId = 101,
                      DisciplinaryIncidentId = 5,
                      CreateDate = DateTime.Now.AddDays(-2),
                      UpdateDate = DateTime.Now,
                      DeleteFlag = null,
                      DisciplinaryArrestId = 5,
                      DisciplinaryDays = 10,
                      DisciplinaryDaysRemoveBy = 11,
                      DisciplinaryDaysRemoveDate = DateTime.Now,
                      DisciplinaryInmateType = 15,
                      DisciplinaryHearingDate = DateTime.Now.AddDays(-2),
                      DisciplinaryScheduleHearingDate = DateTime.Now.AddDays(-2),
                      DisciplinaryInmateBypassHearing = 1,
                      DisciplinaryReviewDate = DateTime.Now,
                      HearingComplete = 1,
                      DisciplinaryOtherName = "ONLINE PROCESS",
                      NoticePersonnelId = 12,
                      NoticeWavierId = 105,
                      NoticeDate = DateTime.Now,
                      NoticeFlag = true,
                      NoticeNote = "CHANGE INTO NEW CELL",
                      PersonnelId = 11,
                      NarrativeFlag = true

                  },
                   new DisciplinaryInmate
                   {
                       DisciplinaryInmateId = 100,
                       DisciplinaryHearingOfficer1 = 11,
                       DisciplinaryHearingOfficer2 = 12,
                       CreateDate = DateTime.Now.AddDays(-3),
                       UpdateDate = DateTime.Now,
                       DisciplinaryIncidentId = 6,
                       DeleteFlag = 0,
                       DisciplinaryArrestId = null,
                       HearingComplete = 1,
                       DisciplinaryReviewDate = DateTime.Now,
                       DisciplinaryFindingDate = DateTime.Now,
                       DisciplinaryReviewOfficer = 13,
                       DisciplinaryHearingDate = DateTime.Now.AddDays(1),
                       DisciplinaryScheduleHearingDate = DateTime.Now.AddDays(1),
                       NarrativeFlag = true,
                       AppealDueDate = DateTime.Now

                   },
                new DisciplinaryInmate
                {
                    DisciplinaryInmateId = 123,
                    InmateId = 101,
                    DisciplinaryIncidentId = 7,
                    CreateDate = DateTime.Now.AddDays(-2),
                    UpdateDate = DateTime.Now,
                    DeleteFlag = null,
                    DisciplinaryArrestId = 5,
                    DisciplinaryDays = 25,
                    DisciplinaryDaysRemoveBy = 11,
                    DisciplinaryDaysRemoveDate = DateTime.Now,
                    DisciplinaryInmateType = 15,
                    DisciplinaryHearingDate = DateTime.Now.AddDays(-2),
                    DisciplinaryScheduleHearingDate = DateTime.Now.AddDays(-2),
                    DisciplinaryInmateBypassHearing = 1,
                    DisciplinaryReviewDate = DateTime.Now,
                    HearingComplete = 1,
                    DisciplinaryOtherName = "NOTE",
                    NoticePersonnelId = 11

                },
                new DisciplinaryInmate
                {
                    DisciplinaryInmateId = 124,
                    InmateId = 120,
                    DisciplinaryHearingOfficer1 = 11,
                    DisciplinaryHearingOfficer2 = 12,
                    CreateDate = DateTime.Now.AddDays(-3),
                    UpdateDate = DateTime.Now,
                    DisciplinaryIncidentId = 6,
                    DeleteFlag = 0,
                    DisciplinaryArrestId = null,
                    HearingComplete = 1,
                    DisciplinaryReviewDate = DateTime.Now,
                    DisciplinaryFindingDate = DateTime.Now,
                    DisciplinaryReviewOfficer = 13,
                    DisciplinaryHearingDate = DateTime.Now.AddDays(1),
                    DisciplinaryScheduleHearingDate = DateTime.Now.AddDays(1),
                    NarrativeFlag = true,
                    AppealDueDate = DateTime.Now
                },


                new DisciplinaryInmate
                {
                    DisciplinaryInmateId = 125,
                    InmateId = 103,
                    DisciplinaryHearingOfficer1 = 13,
                    DisciplinaryHearingOfficer2 = 12,
                    CreateDate = DateTime.Now.AddDays(-3),
                    UpdateDate = DateTime.Now,
                    DisciplinaryIncidentId = 6,
                    DeleteFlag = 0,
                    DisciplinaryArrestId = null,
                    HearingComplete = 1,
                    DisciplinaryReviewDate = DateTime.Now,
                    DisciplinaryFindingDate = DateTime.Now,
                    DisciplinaryReviewOfficer = 13,
                    DisciplinaryHearingDate = DateTime.Now.AddDays(1),
                    DisciplinaryScheduleHearingDate = DateTime.Now.AddDays(1),
                    NarrativeFlag = true,
                    AppealDueDate = DateTime.Now
                }
                );
            Db.DisciplinaryInmateAppeal.AddRange(
                new DisciplinaryInmateAppeal
                {
                    DisciplinaryInmateAppealId = 10,
                    DisciplinaryInmateId = 101,
                    CreateDate = DateTime.Now.AddDays(-6),
                    UpdateDate = DateTime.Now.AddDays(-5),
                    CreateBy = 12,
                    UpdateBy = 11,
                    ReviewNote = null,
                    ReportedBy = 12
                },
                new DisciplinaryInmateAppeal
                {
                    DisciplinaryInmateAppealId = 11,
                    DisciplinaryInmateId = 100,
                    CreateBy = 12,
                    CreateDate = DateTime.Now.AddDays(-6),
                    UpdateBy = 11,
                    UpdateDate = DateTime.Now.AddDays(-5),
                    ReviewNote = null,
                    ReportedBy = 12
                },
                new DisciplinaryInmateAppeal
                {
                    DisciplinaryInmateId = 123,
                    DisciplinaryInmateAppealId = 12,
                    CreateDate = DateTime.Now.AddDays(-5),
                    CreateBy = 11,
                    UpdateBy = 12,
                    UpdateDate = DateTime.Now,
                    ReviewNote = "OTHERS"
                }
                );
            Db.DisciplinaryIncident.AddRange(
                new DisciplinaryIncident
                {
                    DisciplinaryIncidentId = 5,
                    DisciplinaryOfficerId = 11,
                    FacilityId = 1,
                    DisciplinaryLocationId = 5,
                    DisciplinaryAltSentSiteId = 10,
                    CreateDate = DateTime.Now.AddDays(-5),
                    UpdateDate = DateTime.Now.AddHours(-4),
                    DisciplinaryHousingUnitLocation = "FLOOR1",
                    DisciplinaryHousingUnitBed = "UPA01",
                    DisciplinaryHousingUnitNumber = "UP-A",
                    DisciplinaryNumber = "DIS_100101",
                    DisciplinaryReportDate = DateTime.Now,
                    DisciplinaryType = 10,
                    DisciplinaryIncidentDate = DateTime.Now,
                    DisciplinaryActive = 1,
                    ExpectedNarrativeCount = 4
                },
                new DisciplinaryIncident
                {
                    DisciplinaryIncidentId = 6,
                    DisciplinaryOfficerId = 12,
                    FacilityId = 2,
                    DisciplinaryLocationId = 6,
                    DisciplinaryAltSentSiteId = 10,
                    CreateDate = DateTime.Now.AddDays(-1),
                    UpdateDate = DateTime.Now.AddHours(-1),
                    DisciplinaryHousingUnitLocation = "FLOOR2",
                    DisciplinaryHousingUnitBed = "UPA02",
                    DisciplinaryHousingUnitNumber = "UP-A",
                    DisciplinaryNumber = "DIS_100102",
                    DisciplinaryReportDate = DateTime.Now,
                    DisciplinaryType = 10,
                    DisciplinaryIncidentDate = DateTime.Now,
                    DisciplinaryActive = 1
                },
                new DisciplinaryIncident
                {
                    DisciplinaryIncidentId = 7,
                    DisciplinaryOfficerId = 11,
                    FacilityId = 2,
                    DisciplinaryLocationId = 5,
                    DisciplinaryAltSentSiteId = null,
                    CreateDate = DateTime.Now.AddDays(-5),
                    UpdateDate = DateTime.Now.AddHours(-1),
                    DisciplinaryHousingUnitLocation = "FLOOR1",
                    DisciplinaryHousingUnitBed = "UPA01",
                    DisciplinaryHousingUnitNumber = "UP-B",
                    DisciplinaryNumber = "DIS_100202",
                    DisciplinaryReportDate = DateTime.Now,
                    DisciplinaryType = 15,
                    DisciplinaryIncidentDate = DateTime.Now,
                    DisciplinaryActive = 1
                },
                new DisciplinaryIncident
                {
                    DisciplinaryIncidentId = 8,
                    FacilityId = 1,
                    DisciplinaryLocationId = 6,
                    DisciplinaryAltSentSiteId = null,
                    CreateDate = DateTime.Now.AddDays(-5),
                    UpdateDate = DateTime.Now.AddHours(-1),
                    DisciplinaryHousingUnitLocation = "FLOOR1",
                    DisciplinaryHousingUnitBed = "UPA01",
                    DisciplinaryHousingUnitNumber = "UP-B",
                    DisciplinaryNumber = "DIS_100232",
                    DisciplinaryReportDate = DateTime.Now,
                    DisciplinaryType = 10,
                    DisciplinaryIncidentDate = DateTime.Now,
                    DisciplinaryActive = 1
                },
                 new DisciplinaryIncident
                 {
                     DisciplinaryIncidentId = 9,
                     FacilityId = 1,
                     DisciplinaryLocationId = 7,
                     DisciplinaryAltSentSiteId = null,
                     CreateDate = DateTime.Now.AddDays(-5),
                     UpdateDate = DateTime.Now.AddHours(-1),
                     DisciplinaryHousingUnitLocation = "FLOOR1",
                     DisciplinaryHousingUnitBed = "UPA01",
                     DisciplinaryHousingUnitNumber = "UP-B",
                     DisciplinaryNumber = "DIS_100233",
                     DisciplinaryReportDate = DateTime.Now,
                     DisciplinaryType = 5,
                     DisciplinaryIncidentDate = DateTime.Now,
                     DisciplinaryActive = null
                 },
                new DisciplinaryIncident
                {
                    DisciplinaryIncidentId = 10,
                    FacilityId = 1,
                    DisciplinaryLocationId = 5,
                    CreateDate = DateTime.Now.AddDays(-5),
                    UpdateDate = DateTime.Now.AddHours(-1),
                    DisciplinaryHousingUnitLocation = "FLOOR1",
                    DisciplinaryHousingUnitBed = "UPA01",
                    DisciplinaryHousingUnitNumber = "UP-B",
                    DisciplinaryNumber = "DIS_100232",
                    DisciplinaryReportDate = DateTime.Now,
                    DisciplinaryType = 10,
                    DisciplinaryIncidentDate = DateTime.Now,
                    DisciplinaryActive = 1,
                    DisciplinaryOfficerNarrativeFlag = true

                }
            );

            Db.DisciplinaryIncidentFlag.AddRange(
                new DisciplinaryIncidentFlag
                {
                    DisciplinaryIncidentFlagId = 10,
                    CreateBy = 12,
                    CreateDate = DateTime.Now.AddDays(-5),
                    DeleteBy = null,
                    DeleteDate = null,
                    DeleteFlag = null,
                    IncidentFlagText = "FORCED INMATE",
                    DisciplinaryIncidentId = 5
                },
                new DisciplinaryIncidentFlag
                {
                    DisciplinaryIncidentFlagId = 11,
                    CreateBy = 13,
                    CreateDate = DateTime.Now.AddDays(-4),
                    DeleteBy = 11,
                    DeleteDate = DateTime.Now,
                    DeleteFlag = null,
                    IncidentFlagText = "INMATE INJURY",
                    DisciplinaryIncidentId = 6
                }

            );
            Db.DisciplinaryIncidentNarrative.AddRange(
                new DisciplinaryIncidentNarrative
                {
                    DisciplinaryIncidentNarrativeId = 10,
                    CreateBy = 12,
                    CreateDate = DateTime.Now.AddDays(-5),
                    UpdateBy = 12,
                    UpdateDate = DateTime.Now.AddDays(-3),
                    DisciplinaryIncidentId = 6,
                    DisciplinaryIncidentNarrative1 = null,
                    SupervisorReviewBy = 11
                },
                new DisciplinaryIncidentNarrative
                {
                    DisciplinaryIncidentNarrativeId = 11,
                    CreateBy = 11,
                    CreateDate = DateTime.Now.AddDays(-5),
                    UpdateBy = 12,
                    UpdateDate = DateTime.Now.AddDays(-3),
                    DisciplinaryIncidentId = 7,
                    DisciplinaryIncidentNarrative1 = null,
                    SupervisorReviewBy = 11
                },
                new DisciplinaryIncidentNarrative
                {
                    DisciplinaryIncidentNarrativeId = 12,
                    CreateBy = 11,
                    CreateDate = DateTime.Now.AddDays(-5),
                    UpdateBy = 12,
                    UpdateDate = DateTime.Now.AddDays(-3),
                    DisciplinaryIncidentId = 5,
                    DisciplinaryIncidentNarrative1 = null,
                    SupervisorReviewBy = 11
                }
            );

            Db.DisciplinaryControlXref.AddRange(
            new DisciplinaryControlXref
            {
                DisciplinaryControlXrefId = 15,
                CreateDate = DateTime.Now.AddDays(-15),
                UpdateDate = DateTime.Now.AddDays(-12),
                DisciplinaryControlViolationId = 10,
                DisciplinaryInmateId = 102,
                DisciplinaryControlFindingId = 7,
                DisciplinaryControlNotes = "ALL INMATES WILL RETURN TO THEIR ASSIGNED HOUSING",
                DisciplinaryControlWaiverId = 15,
                DisciplinaryControlLevelId = 13,
                DisciplinaryControlSanctionDays = 5
            },
            new DisciplinaryControlXref
            {
                DisciplinaryControlXrefId = 16,
                CreateDate = DateTime.Now.AddHours(-12),
                UpdateDate = DateTime.Now.AddDays(-10),
                DisciplinaryInmateId = 104,
                DisciplinaryControlNotes = null,
            },
            new DisciplinaryControlXref
            {
                DisciplinaryControlXrefId = 17,
                CreateDate = DateTime.Now.AddHours(-12),
                UpdateDate = DateTime.Now.AddDays(-10),
                DisciplinaryInmateId = 120
            }
            );
            Db.DisciplinarySentDayXref.AddRange(
                new DisciplinarySentDayXref
                {
                    DisciplinarySentDayXrefId = 10,
                    CreateBy = 12,
                    CreateDate = DateTime.Now.AddDays(-10),
                    DeleteDate = null,
                    DeleteFlag = null,
                    DisciplinaryInmateId = 104,
                    ArrestId = 6
                },
                new DisciplinarySentDayXref
                {
                    DisciplinarySentDayXrefId = 11,
                    CreateBy = 11,
                    CreateDate = DateTime.Now.AddDays(-10),
                    DeleteDate = null,
                    DeleteFlag = 0,
                    DisciplinaryInmateId = 102,
                    CrimeId = 5
                },
                new DisciplinarySentDayXref
                {
                    DisciplinarySentDayXrefId = 12,
                    CreateBy = 11,
                    CreateDate = DateTime.Now.AddDays(-10),
                    DeleteDate = null,
                    DeleteFlag = 0,
                    DisciplinaryInmateId = 103,
                    CrimeId = 6

                }
                );
            Db.AoWizardProgressDisciplinaryInmate.AddRange(
                new AoWizardProgressDisciplinaryInmate
                {
                    AoWizardProgressId = 10,
                    AoWizardId = 11,
                    DisciplinaryInmateId = 101
                },
                new AoWizardProgressDisciplinaryInmate
                {
                    AoWizardProgressId = 13,
                    AoWizardId = 11,
                    DisciplinaryInmateId = 122
                }
                );
        }
    }
}
