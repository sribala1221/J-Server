using GenerateTables.Models;
using System;

// ReSharper disable once CheckNamespace
namespace ServerAPI.Tests
{
    public partial class DbInitialize
    {
        private void GrievanceDetails()
        {

            Db.Grievance.AddRange(
                new Grievance
                {
                    GrievanceId = 5,
                    CreateDate = DateTime.Now.AddDays(-7),
                    UpdateDate = DateTime.Now,
                    InmateId = 100,
                    DeleteFlag = 0,
                    CreatedBy = 11,
                    DateOccured = DateTime.Now,
                    Department = "HEALTH",
                    FacilityId = 1,
                    GrievanceNumber = "GR10"
                },
                new Grievance
                {
                    GrievanceId = 6,
                    CreateDate = DateTime.Now.AddDays(-5),
                    UpdateDate = DateTime.Now,
                    InmateId = 101,
                    DeleteFlag = 0,
                    CreatedBy = 11,
                    DateOccured = DateTime.Now,
                    Department = "CBI",
                    FacilityId = 2,
                    GrievanceNumber = "GR20"
                },
                new Grievance
                {
                    GrievanceId = 7,
                    CreateDate = DateTime.Now.AddDays(-5),
                    DeleteFlag = 0,
                    FacilityId = 1,
                    InmateId = 105,
                    GrievanceType = 3,
                    CreatedBy = 12,
                    DateOccured = DateTime.Now,
                    Department = "ELECTRIC",
                    GrievanceNumber = "GR30"

                },
                new Grievance
                {
                    GrievanceId = 8,
                    CreateDate = DateTime.Now.AddDays(-5),
                    DeleteFlag = 0,
                    FacilityId = 1,
                    InmateId = 108,
                    Department = null,
                    CreatedBy = 11,
                    DateOccured = DateTime.Now,
                    GrievanceNumber = "GR40"
                },
                new Grievance
                {
                    GrievanceId = 9,
                    CreateDate = DateTime.Now.AddDays(-4),
                    DeleteFlag = 0,
                    FacilityId = 2,
                    CreatedBy = 12,
                    Department = "MEDICAL",
                    DateOccured = DateTime.Now,
                    InmateId = 102,
                    GrievanceNumber = "GR50"
                },
                new Grievance
                {
                    GrievanceId = 10,
                    CreateDate = DateTime.Now.AddDays(-4),
                    DeleteFlag = 0,
                    SetReview = 1,
                    Department = null,
                    DateOccured = DateTime.Now,
                    CreatedBy = 11,
                    UpdateBy = 11,
                    UpdateDate = DateTime.Now,
                    GrievanceNumber = "GR60",
                    InmateId = 125,
                    FacilityId = 2
                  
                },
                new Grievance
                {
                    GrievanceId = 11,
                    CreateDate = DateTime.Now.AddDays(-5),
                    UpdateDate = DateTime.Now,
                    InmateId = 101,
                    DeleteFlag = 0,
                    CreatedBy = 11,
                    DateOccured = DateTime.Now,
                    Department = "CBI",
                    FacilityId = 2,
                    SetReview = 1,
                    GrievanceType = 22,
                    GrievanceLocation = "TRICHY",
                    GrievanceLocationId = 7,
                    GrievanceNumber = "GR70"
                },
                new Grievance
                {
                    GrievanceId = 12,
                    CreateDate = DateTime.Now.AddDays(-4),
                    DeleteFlag = 0,
                    Department = null,
                    DateOccured = DateTime.Now,
                    CreatedBy = 12,
                    UpdateBy = 11,
                    UpdateDate = DateTime.Now,
                    GrievanceNumber = "GR80",
                    InmateId = 125,
                    FacilityId = 2

                }
            );



            Db.GrievancePersonnel.AddRange(
                new GrievancePersonnel
                {
                    GrievancePersonnelId = 10,
                    GrievanceId = 5,
                    PersonnelId = 11
                },
                 new GrievancePersonnel
                 {
                     GrievancePersonnelId = 11,
                     GrievanceId = 7,
                     PersonnelId = 12
                 },
                 new GrievancePersonnel
                 {
                     GrievancePersonnelId = 12,
                     GrievanceId = 6,
                     PersonnelId = 11
                 }
                );
            Db.GrievanceInmate.AddRange(
                new GrievanceInmate
                {
                    GrievanceInmateId = 10,
                    GrievanceId = 5,
                    InmateId = 105
                },
                new GrievanceInmate
                {
                    GrievanceInmateId = 11,
                    GrievanceId = 5,
                    InmateId = 106
                },
                new GrievanceInmate
                {
                    GrievanceInmateId = 12,
                    InmateId = 105,
                    GrievanceId = 6
                }
                );
            Db.GrievanceFlag.AddRange(
                new GrievanceFlag
                {
                    GrievanceFlagId = 10,
                    CreateBy = 12,
                    CreateDate = DateTime.Now.AddDays(-5),
                    DeleteBy = null,
                    DeleteDate = null,
                    DeleteFlag = 0,
                    GrievanceId = 5
                },
                new GrievanceFlag
                {
                    GrievanceFlagId = 11,
                    CreateBy = 12,
                    CreateDate = DateTime.Now.AddDays(-5),
                    DeleteBy = 11,
                    DeleteDate = DateTime.Now,
                    DeleteFlag = 1,
                    GrievanceId = 9,
                    GrievanceFlagText = "INTERPRETER REQUIRED"
                },
                new GrievanceFlag
                {
                    GrievanceFlagId = 12,
                    CreateDate = DateTime.Now.AddDays(-5),
                    CreateBy = 12,
                    DeleteBy = 12,
                    DeleteDate = DateTime.Now,
                    GrievanceId = 6,
                    GrievanceFlagText = "RELIGIOUS REASON"
                }
                );
            Db.GrievanceAppeal.AddRange(
                new GrievanceAppeal
                {
                    GrievanceAppealId = 10,
                    GrievanceId = 6,
                    CreateDate = DateTime.Now.AddDays(-5),
                    CreatedBy = 12,
                    UpdateBy = 11,
                    UpdateDate = DateTime.Now,
                    DeleteDate = null,
                    DeleteBy = null,
                    DeleteFlag = 0,
                    AppealDisposition = "DECISION TO BE REVERSED",
                    AppealCategoryLookup = 15,
                    ReviewDate = DateTime.Now
                },
                new GrievanceAppeal
                {
                    GrievanceAppealId = 11,
                    GrievanceId = 5,
                    CreateDate = DateTime.Now.AddDays(-5),
                    CreatedBy = 12,
                    UpdateBy = 11,
                    UpdateDate = DateTime.Now,
                    DeleteDate = null,
                    DeleteBy = null,
                    DeleteFlag = 0,
                    AppealDisposition = "APPLICATION TO A HIGHER COURT"
                },
                new GrievanceAppeal
                {
                    GrievanceAppealId = 12,
                    GrievanceId = 11,
                    CreateDate = DateTime.Now.AddDays(-4),
                    CreatedBy = 12,
                    UpdateBy = 11,
                    UpdateDate = DateTime.Now,
                    DeleteDate = DateTime.Now,
                    DeleteBy = 12,
                    DeleteFlag = 1,
                    AppealDisposition = "FOUNDED",
                    ReviewDate = DateTime.Now,
                    AppealCategoryLookup = 40
                },
                new GrievanceAppeal
                {
                    GrievanceAppealId = 13,
                    GrievanceId = 5,
                    CreateDate = DateTime.Now.AddDays(-4),
                    CreatedBy = 11,
                    UpdateBy = 12,
                    DeleteFlag = 0,
                    DeleteDate = null,
                    DeleteBy = null,
                    AppealDisposition = null
                },
                new GrievanceAppeal
                {
                    GrievanceAppealId = 14,
                    GrievanceId = 6,
                    CreateDate = DateTime.Now.AddDays(-4),
                    CreatedBy = 12,
                    UpdateBy = 12,
                    DeleteFlag = 0,
                    DeleteDate = null,
                    DeleteBy = null,
                    AppealDisposition = "CASE IS NOT COMPLETED",
                    AppealCategoryLookup = 10
                },
                new GrievanceAppeal
                {
                    GrievanceAppealId = 15,
                    GrievanceId = 11,
                    CreateDate = DateTime.Now.AddDays(-4),
                    CreatedBy = 11,
                    UpdateBy = 11,
                    UpdateDate = DateTime.Now,
                    DeleteDate = DateTime.Now,
                    DeleteBy = 12,
                    DeleteFlag = 1,
                    AppealDisposition = "FOUNDED",
                    ReviewDate = DateTime.Now,
                    AppealCategoryLookup = 40
                }
                );

        }
    }
}
