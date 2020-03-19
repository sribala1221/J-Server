using GenerateTables.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace ServerAPI.Tests
{
    public partial class DbInitialize
    {
        private void InvestigationDetails()
        {
            Db.Investigation.AddRange(
                new Investigation
                {
                    InvestigationId = 10,
                    CreateDate = DateTime.Now.AddDays(-10),
                    DeleteFlag = false,
                    DeleteDate = null,
                    DeleteBy = null,
                    CreateBy = 12,
                    UpdateDate = DateTime.Now.AddDays(-10),
                    UpdateBy = 11,
                    OfficerId = 11,
                    StartDate = DateTime.Now.AddDays(-1),
                    CompleteFlag = false,
                    CompleteBy = null
                },
                new Investigation
                {
                    InvestigationId = 11,
                    CreateDate = DateTime.Now.AddDays(-10),
                    DeleteFlag = false,
                    DeleteDate = null,
                    DeleteBy = null,
                    CreateBy = 12,
                    UpdateDate = DateTime.Now.AddDays(-10),
                    UpdateBy = 11,
                    StartDate = DateTime.Now,
                    CompleteFlag = false,
                    CompleteBy = null,
                    OfficerId = 12
                },
                new Investigation
                {
                    InvestigationId = 12,
                    CreateDate = DateTime.Now.AddDays(-10),
                    DeleteFlag = false,
                    DeleteDate = null,
                    DeleteBy = null,
                    CreateBy = 11,
                    UpdateDate = DateTime.Now.AddDays(-10),
                    UpdateBy = 12,
                    StartDate = DateTime.Now,
                    CompleteFlag = false,
                    CompleteBy = null,
                    OfficerId = 12
                },
                new Investigation
                {
                    InvestigationId = 13,
                    CreateDate = DateTime.Now.AddDays(-9),
                    DeleteFlag = false,
                    DeleteDate = null,
                    DeleteBy = null,
                    CreateBy = 11,
                    UpdateDate = DateTime.Now.AddDays(-9),
                    UpdateBy = 12,
                    StartDate = DateTime.Now,
                    CompleteFlag = true,
                    CompleteBy = null,
                    OfficerId = 11,
                    CompleteDisposition = null
                },
                new Investigation
                {
                    InvestigationId = 14,
                    CreateDate = DateTime.Now.AddDays(-9),
                    DeleteFlag = false,
                    DeleteDate = null,
                    DeleteBy = null,
                    CreateBy = 12,
                    UpdateDate = DateTime.Now.AddDays(-8),
                    UpdateBy = 11,
                    StartDate = DateTime.Now,
                    CompleteFlag = true,
                    CompleteBy = null,
                    OfficerId = 11
                }
                );

            Db.InvestigationPersonnel.AddRange(
                new InvestigationPersonnel
                {
                    InvestigationPersonnelId = 11,
                    CreateDate = DateTime.Now.AddDays(-15),
                    DeleteFlag = false,
                    CreateBy = 11,
                    UpdateDate = DateTime.Now,
                    UpdateBy = 12,
                    PersonnelId = 12,
                    DeleteDate = null,
                    DeleteBy = null,
                    ContributerFlag = true,
                    NamedOnlyFlag = true,
                    ViewerFlag = false,
                    InvestigationId = 10
                },
                new InvestigationPersonnel
                {
                    InvestigationPersonnelId = 12,
                    CreateDate = DateTime.Now.AddDays(-15),
                    DeleteFlag = false,
                    CreateBy = 12,
                    UpdateDate = DateTime.Now,
                    UpdateBy = 11,
                    PersonnelId = 11,
                    DeleteDate = null,
                    DeleteBy = null,
                    ContributerFlag = true,
                    NamedOnlyFlag = true,
                    ViewerFlag = false,
                    InvestigationId = 10
                },
                new InvestigationPersonnel
                {
                    InvestigationPersonnelId = 13,
                    CreateDate = DateTime.Now.AddDays(-15),
                    DeleteFlag = false,
                    CreateBy = 11,
                    UpdateDate = DateTime.Now,
                    UpdateBy = 12,
                    PersonnelId = 12,
                    DeleteDate = null,
                    DeleteBy = null,
                    ContributerFlag = true,
                    NamedOnlyFlag = true,
                    ViewerFlag = false,
                    InvestigationId = 11
                }
                );

            Db.InvestigationFlags.AddRange(
                new InvestigationFlags
                {
                    InvestigationFlagsId = 10,
                    CreateDate = DateTime.Now.AddDays(-10),
                    DeleteFlag = false,
                    CreateBy = 12,
                    UpdateDate = DateTime.Now.AddDays(-6),
                    UpdateBy = 11,
                    DeleteDate = null,
                    DeleteBy = null,
                    InvestigationFlagIndex = 5,
                    InvestigationId = 10
                },
                new InvestigationFlags
                {
                    InvestigationFlagsId = 11,
                    CreateDate = DateTime.Now.AddDays(-10),
                    DeleteFlag = false,
                    CreateBy = 12,
                    UpdateDate = DateTime.Now.AddDays(-6),
                    UpdateBy = 11,
                    DeleteDate = null,
                    DeleteBy = null,
                    InvestigationFlagIndex = 6,
                    InvestigationId = 10
                }
                );
            Db.InvestigationNotes.AddRange(
                new InvestigationNotes
                {
                    InvestigationNotesId = 10,
                    CreateDate = DateTime.Now.AddDays(-10),
                    InmateId = 104,
                    DeleteFlag = false,
                    CreateBy = 12,
                    UpdateDate = DateTime.Now,
                    UpdateBy = 11,
                    DeleteDate = null,
                    DeleteBy = null,
                    InvestigationNoteTypeId = 11,
                    InvestigationId = 10
                },
                new InvestigationNotes
                {
                    InvestigationNotesId = 11,
                    CreateDate = DateTime.Now.AddDays(-10),
                    InmateId = 104,
                    CreateBy = 12,
                    UpdateDate = DateTime.Now,
                    UpdateBy = 11,
                    InvestigationNoteTypeId = 12,
                    InvestigationId = 11
                }
                );


            Db.InvestigationToIncident.AddRange(
                new InvestigationToIncident
                {
                    InvestigationToIncidentId = 20,
                    CreateDate = DateTime.Now.AddDays(-10),
                    DeleteFlag = false,
                    CreateBy = 11,
                    UpdateDate = DateTime.Now,
                    UpdateBy = 11,
                    DeleteDate = null,
                    DeleteBy = null,
                    InvestigationId = 11
                },
                new InvestigationToIncident
                {
                    InvestigationToIncidentId = 21,
                    CreateDate = DateTime.Now.AddDays(-10),
                    DeleteFlag = false,
                    CreateBy = 11,
                    UpdateDate = DateTime.Now,
                    UpdateBy = 11,
                    DeleteDate = null,
                    DeleteBy = null,
                    InvestigationId = 11,
                    DisciplinaryIncidentId = 10
                }
                );

            Db.InvestigationLinkXref.AddRange(
                new InvestigationLinkXref
                {
                    InvestigationLinkXrefId = 10,
                    CreateDate = DateTime.Now.AddDays(-10),
                    CreateBy = 11,
                    InmateId = 104,
                    DeleteFlag = false,
                    DeleteDate = null,
                    DeleteBy = null,
                    UpdateDate = DateTime.Now,
                    UpdateBy = 11,
                    InvestigationLinkId = 10
                },
                new InvestigationLinkXref
                {
                    InvestigationLinkXrefId = 11,
                    CreateDate = DateTime.Now.AddDays(-10),
                    CreateBy = 12,
                    InmateId = 104,
                    DeleteFlag = false,
                    DeleteDate = null,
                    DeleteBy = null,
                    UpdateDate = DateTime.Now.AddDays(-9),
                    UpdateBy = 12,
                    InvestigationLinkId = 10
                }
                );
            Db.InvestigationLink.AddRange(
                new InvestigationLink
                {
                    InvestigationLinkId = 10,
                    CreateDate = DateTime.Now.AddDays(-10),
                    DeleteFlag = false,
                    DeleteDate = DateTime.Now,
                    CreateBy = 11,
                    UpdateDate = DateTime.Now.AddDays(-10),
                    UpdateBy = 11,
                    DeleteBy = null,
                    InvestigationId = 10
                },
                new InvestigationLink
                {
                    InvestigationLinkId = 11,
                    CreateDate = DateTime.Now.AddDays(-10),
                    DeleteFlag = false,
                    DeleteDate = DateTime.Now,
                    CreateBy = 11,
                    UpdateDate = DateTime.Now.AddDays(-10),
                    UpdateBy = 11,
                    DeleteBy = null,
                    InvestigationId = 11
                }
                );


            Db.InvestigationToGrievance.AddRange(
                new InvestigationToGrievance
                {
                    InvestigationToGrievanceId = 15,
                    CreateDate = DateTime.Now.AddDays(-5),
                    DeleteFlag = false,
                    DeleteDate = null,
                    CreateBy = 11,
                    UpdateDate = DateTime.Now,
                    UpdateBy = 12,
                    DeleteBy = null,
                    InvestigationId = 10,
                    GrievanceId = 6
                },
                new InvestigationToGrievance
                {
                    InvestigationToGrievanceId = 16,
                    CreateDate = DateTime.Now.AddDays(-5),
                    DeleteFlag = false,
                    DeleteDate = null,
                    CreateBy = 12,
                    UpdateDate = DateTime.Now,
                    UpdateBy = 11,
                    DeleteBy = null,
                    InvestigationId = 10,
                    GrievanceId = 5
                }
                );

            Db.InvestigationHistory.AddRange(
                new InvestigationHistory
                {
                    InvestigationId = 10,
                    InvestigationHistoryId = 15,
                    InvestigationHistoryList =
                        @"{'Investigation name':'Material icons are delightful, beautifully crafted','Source referral':null,'Source department':'ABC','Investigation type':'ADMINITRATIVE','Investigation status':'ACTIVE','Investigation summary':'Material icons are delightful, beautifully crafted symbols for common actions and items. Download on desktop to use them in your digital products for Android, iOS, and webMaterial icons are delightful, beautifully crafted symbols for common actions and items. Download on desktop to use them in your digital products for Android, iOS, and webMaterial icons are delightful, beautifully crafted symbols for common actions and items. Download on desktop to use them in your digital products for Android,','Start date':'2019-10-01T01:00:30','Manager':1,'Complete flag':true,'Complete date':'2019-11-19T15:47:55.2473424','Complete reason':null,'Receive date':'2019-09-04T12:00:37','Delete flag':null,'Delete date':null}",
                    CreateDate = DateTime.Now,
                    PersonnelId = 11
                },
                new InvestigationHistory
                {
                    InvestigationId = 10,
                    InvestigationHistoryId = 16,
                    InvestigationHistoryList =
                        @"{'Investigation name':'Material icons are delightful, beautifully crafted','Source referral':null,'Source department':'ABC','Investigation type':'ADMINITRATIVE','Investigation status':'ACTIVE','Investigation summary':'Material icons are delightful, beautifully crafted symbols for common actions and items. Download on desktop to use them in your digital products for Android, iOS, and webMaterial icons are delightful, beautifully crafted symbols for common actions and items. Download on desktop to use them in your digital products for Android, iOS, and webMaterial icons are delightful, beautifully crafted symbols for common actions and items. Download on desktop to use them in your digital products for Android,','Start date':'2019-10-01T01:00:30','Manager':1,'Complete flag':true,'Complete date':'2019-11-19T15:47:55.2473424','Complete reason':null,'Receive date':'2019-09-04T12:00:37','Delete flag':null,'Delete date':null}",
                    CreateDate = DateTime.Now,
                    PersonnelId = 12
                }
                );

        }

    }
}
