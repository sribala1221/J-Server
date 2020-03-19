using GenerateTables.Models;
using System;

// ReSharper disable once CheckNamespace
namespace ServerAPI.Tests
{
    public partial class DbInitialize
    {
        private void AppletsDetails()
        {
            Db.AppletsSaved.AddRange(
                new AppletsSaved
                {
                    AppletsSavedId = 15,
                    AppletsNumber = 10,
                    AppletsSavedTitle = "INMATE PHOTOS",
                    AppletsSavedKeyword1 = "PHOTO",
                    AppletsSavedKeyword2 = null,
                    AppletsDeleteFlag = 0,
                    InmatePrebookId = 5,
                    AppletsSavedType = "GALLERY",
                    AppletsSavedDescription = "COLLECTED ALL PHOTOS FROM INMATES",
                    ExternalInmateId = 101,
                    CreateDate = DateTime.Now,
                    CreatedBy = 11,
                    UpdatedBy = 12,
                    UpdateDate = DateTime.Now,
                    ArrestId = 5,
                    IncarcerationId = 10,
                    FacilityId = 2,
                    DisciplinaryIncidentId = 6,
                    InvestigationId = 10
                },
                new AppletsSaved
                {
                    AppletsSavedId = 16,
                    AppletsNumber = 11,
                    AppletsSavedTitle = "THINGS",
                    AppletsSavedKeyword1 = "BELT",
                    AppletsSavedKeyword2 = null,
                    AppletsSavedKeyword4 = "CHAIN",
                    AppletsSavedKeyword5 = null,
                    AppletsDeleteFlag = 0,
                    InmatePrebookId = 6,
                    AppletsSavedType = "STORE ALL THINGS",
                    ExternalInmateId = 100,
                    CreateDate = DateTime.Now,
                    CreatedBy = 12,
                    UpdateDate = DateTime.Now,
                    ArrestId = 6,
                    IncarcerationId = 11,
                    DisciplinaryIncidentId = 5,
                    GrievanceId = 7

                },

                new AppletsSaved
                {
                    AppletsSavedId = 17,
                    AppletsNumber = 11,
                    AppletsSavedTitle = "THINGS",
                    AppletsSavedKeyword1 = "BELT",
                    AppletsSavedKeyword2 = "MOBILE PHONE",
                    AppletsSavedKeyword4 = null,
                    AppletsSavedKeyword5 = "PHANTS",
                    AppletsDeleteFlag = 0,
                    InmatePrebookId = 6,
                    AppletsSavedType = "STORE ALL THINGS",
                    ExternalInmateId = 100,
                    CreateDate = DateTime.Now,
                    CreatedBy = 11,
                    UpdateDate = DateTime.Now,
                    ArrestId = 7,
                    IncarcerationId = 12,
                    InvestigationId = 11

                },
                new AppletsSaved
                {
                    AppletsSavedId = 18,
                    AppletsNumber = 11,
                    AppletsSavedTitle = "THINGS",
                    AppletsSavedKeyword1 = "BELT",
                    AppletsSavedKeyword2 = null,
                    AppletsSavedKeyword4 = "CHAIN",
                    AppletsSavedKeyword5 = null,
                    AppletsDeleteFlag = 0,
                    InmatePrebookId = 6,
                    AppletsSavedType = "STORE ALL THINGS",
                    InmateId = 105,
                    CreateDate = DateTime.Now,
                    CreatedBy = 12,
                    UpdateDate = DateTime.Now,
                    ArrestId = 6,
                    IncarcerationId = 11
                },
                new AppletsSaved
                {
                    AppletsSavedId = 19,
                    AppletsNumber = null,
                    AppletsSavedTitle = "VEHICLE",
                    AppletsSavedKeyword1 = "SPAR SPARTS",
                    AppletsSavedKeyword2 = "ENGINE",
                    AppletsSavedKeyword4 = null,
                    AppletsSavedKeyword5 = null,
                    AppletsDeleteFlag = 0,
                    InmatePrebookId = 6,
                    AppletsSavedType = "CAR",
                    InmateId = 141,
                    CreateDate = DateTime.Now,
                    CreatedBy = 12,
                    UpdateDate = DateTime.Now,
                    ArrestId = 7,
                    IncarcerationId = 11
                },
                new AppletsSaved
                {
                    AppletsSavedId = 20,
                    AppletsNumber = 12,
                    AppletsSavedTitle = "VEHICLE",
                    AppletsSavedKeyword1 = " SEAT BELT",
                    AppletsSavedKeyword2 = null,
                    AppletsSavedKeyword4 = "GLASS",
                    AppletsSavedKeyword5 = null,
                    AppletsDeleteFlag = 1,
                    InmatePrebookId = 6,
                    AppletsSavedType = "STORE ALL THINGS",
                    InmateId = 102,
                    CreateDate = DateTime.Now.AddDays(-2),
                    CreatedBy = 11,
                    UpdateDate = DateTime.Now,
                    IncarcerationId = 11
                },
                new AppletsSaved
                {
                    AppletsSavedId = 21,
                    AppletsNumber = 12,
                    AppletsSavedTitle = null,
                    AppletsSavedKeyword1 = null,
                    AppletsSavedKeyword2 = null,
                    AppletsSavedKeyword4 = null,
                    AppletsSavedKeyword5 = null,
                    AppletsDeleteFlag = 0,
                    InmatePrebookId = 6,
                    AppletsSavedType = "OFFENDER PADF RULES",
                    InmateId = 102,
                    CreateDate = DateTime.Now.AddDays(-2),
                    CreatedBy = 11,
                    UpdateDate = DateTime.Now,
                    IncarcerationId = 11
                });

            Db.AppletsSavedHistory.AddRange(
                new AppletsSavedHistory
                {
                    AppletsSavedId = 15,
                    AppletsSavedHistoryId = 5,
                    PersonnelId = 12,
                    CreateDate = DateTime.Now.AddDays(-3),
                    AppletsSavedHistoryList =
                        @"{'InmateId':'100', 'InmateSiteNumber':'IS1000', 'InmateCurrentTrack':'CHENNAI', 'Type':'GALLERY', 'AppletsSavedKeyword1' : 'PHOTO'}"
                },
                new AppletsSavedHistory
                {
                    AppletsSavedId = 16,
                    AppletsSavedHistoryId = 6,
                    PersonnelId = 11,
                    CreateDate = DateTime.Now.AddDays(-2),
                    AppletsSavedHistoryList =
                        @"{'InmateId':'103', 'InmateSiteNumber':'CHS000', 'InmateCurrentTrack':'CHENNAI', 'Type':'STORE ROOM', 'AppletsSavedKeyword1' : 'BELT','AppletsSavedKeyword4' : 'CHAIN'}"
                },
                new AppletsSavedHistory
                {
                    AppletsSavedId = 15,
                    AppletsSavedHistoryId = 7,
                    PersonnelId = 11,
                    CreateDate = DateTime.Now,
                    AppletsSavedHistoryList =
                        @"{'InmateId':'102', 'InmateSiteNumber':'PKS000', 'InmateCurrentTrack':'TRICHY', 'Type':null,'AppletsSavedKeyword1' :'PHOTO','AppletsSavedKeyword2' :null}"
                }
            );
        }

    }
}
