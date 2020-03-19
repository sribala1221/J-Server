using GenerateTables.Models;
using System;

// ReSharper disable once CheckNamespace
namespace ServerAPI.Tests
{
    public partial class DbInitialize
    {
        private void CrimeDetails()
        {
            Db.Crime.AddRange(
                new Crime
                {
                    CreateDate = DateTime.Now.AddDays(-3),
                    UpdateDate = DateTime.Now.AddDays(-2),
                    CreatedBy = 11,
                    UpdateBy = 11,
                    CrimeId = 5,
                    CrimeLookupId = 5,
                    ArrestId = 5,
                    CrimeDeleteFlag = 0,
                    WarrantId = 6,
                    CrimeQualifierLookup = 10,
                    CrimeType = "4"
                },
                new Crime
                {
                    CreateDate = DateTime.Now.AddDays(-3),
                    UpdateDate = DateTime.Now.AddDays(-3),
                    CreatedBy = 11,
                    UpdateBy = 11,
                    CrimeId = 6,
                    CrimeLookupId = 6,
                    ArrestId = 5,
                    CrimeDeleteFlag = 1,
                    WarrantId = 5,
                    CrimeQualifierLookup = 10,
                    CrimeType = "5",
                    ArrestSentenceDisciplinaryDaysFlag = 1
                },
                new Crime
                {
                    CreateDate = DateTime.Now.AddDays(-3),
                    UpdateDate = DateTime.Now.AddDays(-2),
                    CreatedBy = 11,
                    UpdateBy = 12,
                    CrimeId = 7,
                    CrimeLookupId = 5,
                    ArrestId = 7,
                    CrimeDeleteFlag = 0,
                    WarrantId = 6,
                    CrimeQualifierLookup = 6,
                    CrimeType = "6",
                    ChargeQualifierLookup = "7",
                    ArrestSentenceMethodId = 15
                   
                },
                new Crime
                {
                    CreateDate = DateTime.Now.AddDays(-2),
                    UpdateDate = DateTime.Now,
                    CreatedBy = 12,
                    UpdateBy = 11,
                    CrimeId = 8,
                    CrimeLookupId = 6,
                    ArrestId = 7,
                    CrimeDeleteFlag = 0,
                    WarrantId = 5,
                    CrimeQualifierLookup = 6,
                    CrimeType = "4",
                    ArrestSentenceMethodId = 16
                },
                new Crime
                {
                    CreateDate = DateTime.Now.AddDays(-3),
                    UpdateDate = DateTime.Now,
                    CreatedBy = 12,
                    UpdateBy = 11,
                    CrimeId = 9,
                    CrimeLookupId = 6,
                    ArrestId = 6,
                    CrimeDeleteFlag = 0,
                    WarrantId = 5,
                    CrimeQualifierLookup = 5,
                    CrimeType = "5",
                    BailType = "NOBAIL"
                },
                new Crime
                {
                    CreateDate = DateTime.Now.AddDays(-3),
                    UpdateDate = DateTime.Now,
                    CreatedBy = 11,
                    UpdateBy = 11,
                    CrimeId = 10,
                    CrimeLookupId = 6,
                    ArrestId = 8,
                    CrimeDeleteFlag = 0,
                    WarrantId = 6,
                    CrimeQualifierLookup = 3,
                    CrimeType = "6",
                    ArrestSentenceMethodId = 15
                },
                new Crime
                {
                    CreateDate = DateTime.Now.AddDays(-2),
                    UpdateDate = DateTime.Now,
                    CreatedBy = 12,
                    UpdateBy = 12,
                    CrimeId = 11,
                    CrimeLookupId = 6,
                    ArrestId = 6,
                    CrimeDeleteFlag = 0,
                    CrimeQualifierLookup = 5,
                    CrimeType = "6",
                    BailType = "ARREST",
                    BailAmount = 10000
                },
                new Crime
                {
                    CreateDate = DateTime.Now.AddDays(-1),
                    UpdateDate = DateTime.Now,
                    CreatedBy = 13,
                    UpdateBy = 11,
                    CrimeId = 12,
                    CrimeLookupId = 6,
                    ArrestId = 10,
                    CrimeQualifierLookup = 6,
                    CrimeType = "6",
                    BailType = "BAIL",
                    BailAmount = 10100
                },
                new Crime
                {
                    CreateDate = DateTime.Now,
                    UpdateDate = DateTime.Now,
                    CreatedBy = 13,
                    UpdateBy = 11,
                    CrimeId = 13,
                    CrimeLookupId = 6,
                    ArrestId = 10,
                    CrimeQualifierLookup = 5,
                    CrimeType = "5",
                    BailType = "ARREST",
                    BailAmount = 15000
                    
                },
                new Crime
                {
                    CreateDate = DateTime.Now,
                    UpdateDate = DateTime.Now,
                    CreatedBy = 12,
                    UpdateBy = 12,
                    CrimeId = 14,
                    CrimeLookupId = 6,
                    ArrestId = 8,
                    CrimeQualifierLookup = 5,
                    CrimeType = "6",
                    BailAmount = 45000,
                    BailType = "NOBAIL",
                    ArrestSentenceMethodId = 16

                },
                new Crime
                {
                    CreateDate = DateTime.Now,
                    UpdateDate = DateTime.Now,
                    CreatedBy = 11,
                    UpdateBy = 12,
                    CrimeId = 15,
                    CrimeLookupId = 6,
                    ArrestId = 10,
                    CrimeQualifierLookup = 5,
                    CrimeType = "5",
                    BailType = "DRINK AND DRIVE",
                    BailAmount = 10000
                }
            );

            Db.CrimeHistory.AddRange(
                new CrimeHistory
                {
                    CrimeId = 11,
                    CrimeHistoryId = 1500,
                    CreatedBy = 11,
                    BailAmount = 4000,
                    CrimeDeleteFlag = 0,
                    CrimeCount = 1,
                    CreatDate = DateTime.Now.AddDays(-4),
                    CrimeLookupId = 11,
                    CrimeForceId = 10
                },
                new CrimeHistory
                {
                    CrimeId = 15,
                    CrimeHistoryId = 1501,
                    CreatedBy = 11,
                    BailAmount = 4500,
                    CrimeDeleteFlag = 0,
                    CrimeCount = 1,
                    CreatDate = DateTime.Now.AddDays(-4),
                    CrimeLookupId = 11,
                    CrimeForceId = 10
                },
                new CrimeHistory
                {
                    CrimeId = 13,
                    CrimeForceId = 12,
                    BailAmount = 4000,
                    CrimeCount = 2,
                    CrimeDeleteFlag = 0,
                    CrimeHistoryId = 1502,
                    CreatDate = DateTime.Now.AddDays(-4),
                    CreatedBy = 12,
                    InmatePrebookChargeId = 12
                },
                new CrimeHistory
                {
                    CrimeId = 13,
                    CrimeForceId = 8,
                    BailAmount = 4000,
                    CrimeCount = 5,
                    CrimeDeleteFlag = 0,
                    CrimeHistoryId = 1503,
                    CreatDate = DateTime.Now.AddDays(-4),
                    CreatedBy = 11,
                    InmatePrebookChargeId = 12
                }
                );

            Db.CrimeLookupFlag.AddRange(
                new CrimeLookupFlag
                {
                    CrimeLookupFlagId = 20,
                    CreateDate = DateTime.Now.AddDays(-10),
                    UpdateDate = DateTime.Now.AddDays(-10),
                    CreateBy = 11,
                    UpdateBy = 12,
                    InactiveFlag = 0,
                    FlagDescription = "USED FOR CLASSIFICATION TREE",
                    FlagName = "VIOLENT ASSAULTIVE"
                },
                new CrimeLookupFlag
                {
                    CrimeLookupFlagId = 21,
                    CreateDate = DateTime.Now.AddDays(-10),
                    UpdateDate = DateTime.Now.AddDays(-10),
                    CreateBy = 11,
                    UpdateBy = 12,
                    InactiveFlag = 0,
                    FlagDescription = "USED FOR CLASSIFICATION TREE",
                    FlagName = "FEDERAL/STATE HOLD"
                }
                );

            Db.CrimeForce.AddRange(
                new CrimeForce
                {
                    ArrestId = 5,
                    CreateBy = 11,
                    UpdateBy = 11,
                    CreateDate = DateTime.Now.AddDays(-5),
                    UpdateDate = DateTime.Now,
                    CrimeForceId = 5,
                    DeleteFlag = 0,
                    TempCrimeCodeType = "M",
                    TempCrimeType = "5",
                    TempCrimeGroup = "6",
                    TempCrimeDescription = "COLLECTION OF DOCUMENTS FOR CRIME CASE"
                },
                new CrimeForce
                {
                    ArrestId = 7,
                    CreateBy = 12,
                    UpdateBy = 11,
                    CreateDate = DateTime.Now.AddDays(-5),
                    UpdateDate = DateTime.Now,
                    CrimeForceId = 6,
                    DeleteFlag = 0,
                    DropChargeFlag = 0,
                    ForceCrimeLookupId = 0,
                    SearchCrimeLookupId = 0,
                    WarrantId = 7,
                    BailAmount = 5000,
                    BailNoBailFlag = 1,
                    TempCrimeSection = "4",
                    TempCrimeDescription = "PULLED PKG 'CRACK' OUT OF POCKET",
                    TempCrimeCodeType = "I",
                    TempCrimeType = "6",
                    TempCrimeGroup = "7",
                },
                new CrimeForce
                {
                    ArrestId = 8,
                    CreateBy = 12,
                    UpdateBy = 11,
                    CreateDate = DateTime.Now.AddDays(-5),
                    UpdateDate = DateTime.Now,
                    CrimeForceId = 7,
                    DeleteFlag = 0,
                    TempCrimeCodeType = "M",
                    TempCrimeDescription = "AGE IS NOT APLICABLE FOR LICENSE",
                    TempCrimeType = "5",
                    BailAmount = 5000,
                    BailNoBailFlag = 1,
                    TempCrimeGroup = "5"
                },
                new CrimeForce
                {
                    ArrestId = 6,
                    CreateBy = 12,
                    UpdateBy = 11,
                    CreateDate = DateTime.Now.AddDays(-4),
                    UpdateDate = DateTime.Now,
                    CrimeForceId = 8,
                    DeleteFlag = 0,
                    TempCrimeCodeType = "X",
                    TempCrimeDescription = "CHAIN SNATCHERS",
                    WarrantId = 0,
                    ForceSupervisorReviewFlag = 0,
                    BailAmount = 4500,
                    BailNoBailFlag = 1,
                    TempCrimeType = "6",
                    TempCrimeGroup = "5"

                },
                new CrimeForce
                {
                    ArrestId = 7,
                    CreateBy = 12,
                    UpdateBy = 11,
                    CreateDate = DateTime.Now.AddDays(-4),
                    UpdateDate = DateTime.Now,
                    CrimeForceId = 9,
                    DeleteFlag = 0,
                    TempCrimeCodeType = "Y",
                    TempCrimeDescription = "DRUG ADDICTION",
                    WarrantId = 0,
                    ForceSupervisorReviewFlag = 0,
                    BailAmount = 4500,
                    BailNoBailFlag = 0,
                    TempCrimeType = "7",
                    InmatePrebookId = 10,
                    TempCrimeGroup = "5"
                },
                new CrimeForce
                {
                    CrimeForceId = 10,
                    UpdateBy = 12,
                    CreateDate = DateTime.Now.AddDays(-4),
                    UpdateDate = DateTime.Now,
                    DeleteFlag = 0,
                    BailAmount = 15000,
                    DeleteDate = null,
                    DeleteBy = null,
                    ArrestId = null,
                    InmatePrebookId = null,
                    BailNoBailFlag = 0,
                    TempCrimeType = "7",
                    TempCrimeCodeType = "M",
                    TempCrimeDescription = "DRUG ADDICTION",
                    TempCrimeGroup = "6"
                },
                new CrimeForce
                {
                    CrimeForceId = 11,
                    UpdateBy = 12,
                    CreateDate = DateTime.Now.AddDays(-4),
                    UpdateDate = DateTime.Now,
                    DeleteFlag = 0,
                    BailAmount = 100000,
                    DeleteDate = null,
                    DeleteBy = null,
                    ArrestId = null,
                    InmatePrebookId = null,
                    BailNoBailFlag = 0,
                    TempCrimeType = "10",
                    TempCrimeCodeType = "M",
                    TempCrimeDescription = "MURDER CASE",
                    TempCrimeGroup = "6"
                },
                new CrimeForce
                {
                    ArrestId = 7,
                    CreateBy = 12,
                    UpdateBy = 11,
                    CreateDate = DateTime.Now.AddDays(-4),
                    UpdateDate = DateTime.Now,
                    CrimeForceId = 12,
                    DeleteFlag = 0,
                    TempCrimeCodeType = "M",
                    TempCrimeDescription = "DRUG ADDICTION",
                    WarrantId = 0,
                    ForceSupervisorReviewFlag = 0,
                    BailAmount = 4500,
                    BailNoBailFlag = 0,
                    TempCrimeType = "8",
                    InmatePrebookId = 10,
                    TempCrimeGroup = "8"
                }

            );
            Db.CrimeLookup.AddRange(
                new CrimeLookup
                {
                    CrimeLookupId = 5,
                    CrimeCodeType = "F",
                    CrimeSection = "EPKO320",
                    CrimeDescription = "EP CASE(1AC)"
                    
                },
                new CrimeLookup
                {
                    CrimeLookupId = 6,
                    CrimeCodeType = "M",
                    CrimeSection = "IRSR100",
                    CrimeDescription = "IS CASE(1AS)"
                },
                new CrimeLookup
                {
                    CrimeLookupId = 7,
                    CrimeCodeType = "F",
                    CrimeSection = "IRSR101",
                    CrimeDescription = "MEDICINE"
                },
                new CrimeLookup
                {
                    CrimeLookupId = 8,
                    CrimeCodeType = "F",
                    CrimeSection = "IRSR101",
                    CrimeDescription = "ROOM VIOLATION"
                }
            );
        }
    }
}
