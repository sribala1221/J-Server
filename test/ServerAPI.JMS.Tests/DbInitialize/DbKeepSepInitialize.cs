using GenerateTables.Models;
using System;

// ReSharper disable once CheckNamespace
namespace ServerAPI.Tests
{
    public partial class DbInitialize
    {
        private void KeepSepDetails()
        {
            Db.KeepSeparate.AddRange(
                new KeepSeparate
                {
                    KeepSeparateId = 5,
                    InactiveFlag = 0,
                    KeepSeparateInmate2Id = 100,
                    KeepSeparateInmate1Id = 102,
                    KeepSeparateType = "CONFIDENTIAL",
                    KeepSeparateReason = "ASSAULT"
                },
                new KeepSeparate
                {
                    KeepSeparateId = 6,
                    InactiveFlag = 0,
                    KeepSeparateInmate2Id = 101,
                    KeepSeparateInmate1Id = 102,
                    KeepSeparateType = "INCIDENT-STREET",
                    KeepSeparateReason = "SAME AGE"
                },
                new KeepSeparate
                {
                    KeepSeparateId = 7,
                    InactiveFlag = 0,
                    KeepSeparateInmate2Id = 100,
                    KeepSeparateInmate1Id = 101,
                    KeepSeparateType = "INCIDENT-STREET",
                    KeepSeparateReason = "ASSAULT"
                },
                new KeepSeparate
                {
                    KeepSeparateId = 8,
                    InactiveFlag = 0,
                    KeepSeparateInmate2Id = 103,
                    KeepSeparateInmate1Id = 100,
                    KeepSeparateType = "ENEMY",
                    KeepSeparateReason = "BOTH ARE ENEMY"
                },
                new KeepSeparate
                {
                    KeepSeparateId = 9,
                    InactiveFlag = 0,
                    KeepSeparateInmate1Id = 106,
                    KeepSeparateInmate2Id = 105,
                    KeepSeparateType = "CONFIDENTIAL",
                    KeepSeparateReason = "CO-DEFENDANT"
                },
                new KeepSeparate
                {
                    KeepSeparateId = 10,
                    InactiveFlag = 0,
                    KeepSeparateInmate1Id = 110,
                    KeepSeparateInmate2Id = 106,
                    KeepSeparateType = "ENEMY",
                    KeepSeparateReason = "ASSAULT"
                },
                new KeepSeparate
                {
                    KeepSeparateId = 11,
                    InactiveFlag = 0,
                    KeepSeparateInmate1Id = 125,
                    KeepSeparateInmate2Id = 110,
                    KeepSeparateType = "ENEMY",
                    KeepSeparateReason = "ENEMY"
                },
                new KeepSeparate
                {
                    KeepSeparateId = 12,
                    InactiveFlag = 0,
                    KeepSeparateInmate1Id = 125,
                    KeepSeparateInmate2Id = 105,
                    KeepSeparateType = "OTHER",
                    KeepSeparateReason = "ASSAULT"
                },
                new KeepSeparate
                {
                    KeepSeparateId = 13,
                    InactiveFlag = 0,
                    KeepSeparateInmate1Id = 102,
                    KeepSeparateInmate2Id = 135,
                    KeepSeparateType = "ASSAULT",
                    KeepSeparateReason = "SAME AGE PERSONS"
                },
                new KeepSeparate
                {
                    KeepSeparateId = 14,
                    InactiveFlag = 1,
                    InactiveDate = DateTime.Now,
                    KeepSeparateInmate1Id = 107,
                    InactiveBy = null
                }
            );

            Db.KeepSeparateHistory.AddRange(
                new KeepSeparateHistory
                {
                    KeepSeparateId = 6,
                    CreateDate = DateTime.Now.AddDays(-4),
                    PersonnelId = 12,
                    KeepSeparateHistoryId = 5
                },
                new KeepSeparateHistory
                {
                    KeepSeparateHistoryId = 6,
                    KeepSeparateId = 7,
                    CreateDate = DateTime.Now.AddDays(-4),
                    PersonnelId = 11
                },
                new KeepSeparateHistory
                {
                    KeepSeparateHistoryId = 7,
                    KeepSeparateId = 5,
                    CreateDate = DateTime.Now.AddDays(-3),
                    PersonnelId = 12
                }
            );

            Db.KeepSepAssocSubset.AddRange(
                new KeepSepAssocSubset
                {
                    KeepSepAssocSubsetId = 10,
                    CreateDate = DateTime.Now.AddDays(-4),
                    UpdateBy = 11,
                    CreateBy = 12,
                    UpdateDate = DateTime.Now,
                    DeleteBy = 12,
                    DeleteDate = DateTime.Now,
                    DeleteFlag = 0
                },
                new KeepSepAssocSubset
                {
                    KeepSepAssocSubsetId = 11,
                    CreateDate = DateTime.Now.AddDays(-3),
                    UpdateBy = 11,
                    CreateBy = 12,
                    UpdateDate = DateTime.Now.AddDays(-1),
                    DeleteFlag = 0
                }
            );
            Db.KeepSepSubsetInmate.AddRange(
                new KeepSepSubsetInmate
                {
                    KeepSepSubsetInmateId = 5,
                    KeepSepInmate2Id = 101,
                    CreateDate = DateTime.Now,
                    CreateBy = 11,
                    UpdateDate = DateTime.Now,
                    UpdateBy = 11,
                    DeleteFlag = 0,
                    KeepSeparateType = "CONFIDENTIAL",
                    KeepSepReason = "ASSAULT",
                    KeepSepAssoc1Subset = "STREET LEVEL",
                    KeepSepAssoc1 = "LOCAL BOYS"
                },
                new KeepSepSubsetInmate
                {
                    KeepSepSubsetInmateId = 6,
                    KeepSepInmate2Id = 100,
                    CreateDate = DateTime.Now,
                    CreateBy = 11,
                    UpdateDate = DateTime.Now,
                    UpdateBy = 12,
                    DeleteFlag = 0,
                    KeepSeparateType = "INCIDENT-STREET",
                    KeepSepReason = "SAME AGE",
                    KeepSepAssoc1Subset = "MEDIUM LEVEL",
                    KeepSepAssoc1 = "RICH BOYS"
                },
                new KeepSepSubsetInmate
                {
                    KeepSepSubsetInmateId = 7,
                    KeepSepInmate2Id = 101,
                    CreateDate = DateTime.Now,
                    CreateBy = 12,
                    UpdateDate = DateTime.Now,
                    UpdateBy = 11,
                    DeleteFlag = 1,
                    KeepSeparateType = "DROP OUT",
                    KeepSepReason = "FAMILY-MEMBERS",
                    KeepSepAssoc1Subset = "CRIMINAL BOYS",
                    KeepSepAssoc1 = "SAME LOCATION"
                });
            Db.KeepSepAssocInmate.AddRange(
                new KeepSepAssocInmate
                {
                    KeepSepAssocInmateId = 15,
                    DeleteFlag = 0,
                    KeepSepInmate2Id = 101,
                    KeepSepAssoc1 = "LOCAL BOYS",
                    KeepSeparateType = "RACIAL",
                    KeepSepReason = "DEFRAUDED GANG",
                    KeepSepAssoc1Id = 5
                },
                new KeepSepAssocInmate
                {
                    DeleteFlag = 0,
                    KeepSepAssocInmateId = 16,
                    KeepSepInmate2Id = 100,
                    KeepSepAssoc1 = "ACORN - WORK",
                    KeepSeparateType = "INCIDENT-JMS",
                    KeepSepReason = "INFORMED ON GANG",
                    KeepSepAssoc1Id = 5
                },
                new KeepSepAssocInmate
                {
                    KeepSepAssocInmateId = 17,
                    DeleteFlag = 0,
                    KeepSepInmate2Id = 102,
                    KeepSepAssoc1 = "NORTENOS",
                    KeepSeparateType = "DROP OUT",
                    KeepSepReason = "ASSAULT BY GANG",
                    KeepSepAssoc1Id = 6
                },
                new KeepSepAssocInmate
                {
                    KeepSepAssocInmateId = 18,
                    DeleteFlag = 0,
                    KeepSepInmate2Id = 125,
                    KeepSepAssoc1 = "ROD TYPE",
                    KeepSepAssoc1Id = 7
                }
            );
            Db.KeepSepAssocAssoc.AddRange(
                new KeepSepAssocAssoc
                {
                    KeepSepAssocAssocId = 10,
                    CreateBy = 11,
                    UpdateBy = 12,
                    CreateDate = DateTime.Now.AddDays(-5),
                    UpdateDate = DateTime.Now.AddDays(-5),
                    DeleteFlag = 0,
                    KeepSepAssoc1 = "SAME LOCATION",
                    KeepSepAssoc1Id = 5,
                    KeepSepAssoc2Id = 9

                },
                new KeepSepAssocAssoc
                {
                    KeepSepAssocAssocId = 11,
                    CreateBy = 12,
                    UpdateBy = 12,
                    CreateDate = DateTime.Now.AddDays(-4),
                    UpdateDate = DateTime.Now.AddDays(-4),
                    DeleteFlag = 0,
                    KeepSepAssoc1 = "ROD TYPE",
                    KeepSepAssoc2 = "MILITARY",
                    KeepSepAssoc1Id = 8,
                    KeepSepAssoc2Id = 10
                },
                new KeepSepAssocAssoc
                {
                    KeepSepAssocAssocId = 12,
                    CreateBy = 11,
                    CreateDate = DateTime.Now.AddDays(-4),
                    UpdateBy = 11,
                    UpdateDate = DateTime.Now,
                    KeepSepAssoc1 = "LOCAL BOYS"
                }
            );

            Db.KeepSepSubsetSubset.AddRange(
                new KeepSepSubsetSubset
                {
                    KeepSepSubsetSubsetId = 10,
                    CreateBy = 12,
                    CreateDate = DateTime.Now,
                    UpdateBy = 11,
                    UpdateDate = DateTime.Now,
                    KeepSepAssoc1 = "LOCAL BOYS",
                    KeepSepAssoc2 = null,
                    KeepSepAssoc1Subset = "STREET LEVEL",
                    KeepSepAssoc2SubsetId = 3
                }
                );


            Db.KeepSepAssocInmateHistory.AddRange(
                new KeepSepAssocInmateHistory
                {
                    KeepSepAssocInmateId = 16,
                    KeepSepAssocInmateHistoryId = 10,
                    PersonnelId = 12,
                    CreateDate = DateTime.Now.AddDays(-4),
                    KeepSepAssocInmateHistoryList =
                        @"{'Inmate':'ANU', 'Keep Sep Type':'CONFIDENTIAL', 'Keep Sep Reason':'ASSAULT'}"
                },
                new KeepSepAssocInmateHistory
                {
                    KeepSepAssocInmateId = 15,
                    KeepSepAssocInmateHistoryId = 11,
                    PersonnelId = 12,
                    CreateDate = DateTime.Now.AddDays(-4),
                    KeepSepAssocInmateHistoryList =
                        @"{'Inmate':'MUKESH', 'Keep Sep Type':'ENEMY', 'Keep Sep Reason':'BOTH ARE ENEMY' }"
                }
            );
        }
    }
}
