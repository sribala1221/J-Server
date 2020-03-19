using GenerateTables.Models;
using ServerAPI.Utilities;
using System;

// ReSharper disable once CheckNamespace
namespace ServerAPI.Tests
{
    public partial class DbInitialize
    {
        private void SiteDetails()
        {
            Db.SiteOptions.AddRange(
                new SiteOptions
                {
                    SiteOptionsId = 10,
                    SiteOptionsName = SiteOptionsConstants.JAILSELINCAR,
                    SiteOptionsStatus = "1",
                    SiteOptionsValue = "ON"
                },
                new SiteOptions
                {
                    SiteOptionsId = 11,
                    SiteOptionsName = SiteOptionsConstants.BOOKINGCOMREQCHAR,
                    SiteOptionsStatus = "1",
                    SiteOptionsValue = "ON"
                },
                new SiteOptions
                {
                    SiteOptionsId = 12,
                    SiteOptionsName = SiteOptionsConstants.BOOKINGCOMPREQWAR,
                    SiteOptionsStatus = "1",
                    SiteOptionsValue = "OFF"
                },
                new SiteOptions
                {
                    SiteOptionsId = 13,
                    SiteOptionsName = SiteOptionsConstants.BOOKING1WAR,
                    SiteOptionsValue = "ON",
                    SiteOptionsStatus = "1"
                },
                new SiteOptions
                {
                    SiteOptionsId = 14,
                    SiteOptionsName = SiteOptionsConstants.BAILSUMCHARNO,
                    SiteOptionsValue = "ON",
                    SiteOptionsStatus = "1",
                    SiteOptionsVariable = "BAIL_SUMMARY_CHARGES_NO_STATUS"
                },
                new SiteOptions
                {
                    SiteOptionsId = 15,
                    SiteOptionsName = SiteOptionsConstants.NCIC_RUN,
                    SiteOptionsValue = "ON",
                    SiteOptionsStatus = "1",
                    SiteOptionsVariable = "NCIC_RUN"
                },
                new SiteOptions
                {
                    SiteOptionsId = 16,
                    SiteOptionsName = "1",
                    SiteOptionsValue = "ON",
                    SiteOptionsStatus = "1"
                },
                new SiteOptions
                {
                    SiteOptionsId = 17,
                    SiteOptionsName = SiteOptionsConstants.SENTENCEBYCHARGE,
                    SiteOptionsValue = "ON",
                    SiteOptionsStatus = "1",
                    SiteOptionsVariable = "SENTENCE_BY_CHARGE"

                },
                new SiteOptions
                {
                    SiteOptionsId = 18,
                    SiteOptionsName = SiteOptionsConstants.HIGHESTBAILPERBOOKING,
                    SiteOptionsValue = "ON",
                    SiteOptionsStatus = "1"
                },
                new SiteOptions
                {
                    SiteOptionsId = 19,
                    SiteOptionsName = SiteOptionsConstants.ALLOWWEEKENDERSENTENCE,
                    SiteOptionsValue = "OFF",
                    SiteOptionsStatus = "1",
                    SiteOptionsVariable = "ALLOW_WEEKENDER_SENTENCE"
                },
                new SiteOptions
                {
                    SiteOptionsId = 20,
                    SiteOptionsName = SiteOptionsConstants.ALLOWCLASSIFYEDIT,
                    SiteOptionsValue = "ON",
                    SiteOptionsStatus = "1",
                    SiteOptionsVariable = "ALLOW_CLASSIFY_EDIT"
                },
                new SiteOptions
                {
                    SiteOptionsId = 21,
                    SiteOptionsName = SiteOptionsConstants.FORCETRANSFERDURINGFACILITYMOVE,
                    SiteOptionsValue = "ON",
                    SiteOptionsStatus = "1",
                    SiteOptionsVariable = "FORCE_TRANSFER_DURING_FACILITY_MOVE"
                },
                new SiteOptions
                {
                    SiteOptionsId = 22,
                    SiteOptionsName = SiteOptionsConstants.DONOTALLOWOVERCAPACITYHOUSING,
                    SiteOptionsValue = "OFF",
                    SiteOptionsStatus = "1",
                    SiteOptionsVariable = "DO_NOT_ALLOW_OVER_CAPACITY_HOUSING"
                },
                new SiteOptions
                {
                    SiteOptionsId = 23,
                    SiteOptionsName = SiteOptionsConstants.CLASSREVIEWQUEUEBYCLASSLEVEL,
                    SiteOptionsValue = "ON",
                    SiteOptionsStatus = "1",
                    SiteOptionsVariable = "CLASS_REVIEW_QUEUE_BYCLASSLEVEL"
                },
                new SiteOptions
                {
                    SiteOptionsId = 24,
                    SiteOptionsName = SiteOptionsConstants.CLASSIFYREVIEWBATCH,
                    SiteOptionsValue = "ON",
                    SiteOptionsStatus = "1",
                    SiteOptionsVariable = "CLASSIFY_REVIEW_BATCH"
                },
                new SiteOptions
                {
                    SiteOptionsId = 25,
                    SiteOptionsName = SiteOptionsConstants.NOHOUSECLASS,
                    SiteOptionsValue = "ON",
                    SiteOptionsStatus = "1",
                    SiteOptionsVariable = "NO_HOUSE_CLASS"
                },
                new SiteOptions
                {
                    SiteOptionsId = 26,
                    SiteOptionsName = SiteOptionsConstants.MULTIPLEPREBOOKCASE,
                    SiteOptionsValue = "OFF",
                    SiteOptionsStatus = "1",
                    SiteOptionsVariable = "MULTIPLEPREBOOKCASE"
                },
                new SiteOptions
                {
                    SiteOptionsId = 27,
                    SiteOptionsName = SiteOptionsConstants.INTAKEPREBOOKREVIEW,
                    SiteOptionsValue = "ON",
                    SiteOptionsStatus = "1",
                    SiteOptionsVariable = "INTAKE_PREBOOK_REVIEW"
                },
                new SiteOptions
                {
                    SiteOptionsId = 28,
                    SiteOptionsName = SiteOptionsConstants.INVENTORYDAYSAFTERRELEASEDEFAULT,
                    SiteOptionsValue = "ON",
                    SiteOptionsStatus = "1",
                    SiteOptionsVariable = "INVENTORY_DAYS_AFTER_RELEASE_DEFAULT"
                },
                new SiteOptions
                {
                    SiteOptionsId = 29,
                    SiteOptionsName = SiteOptionsConstants.BYPASSASSESSMENTFORNONKEEPER,
                    SiteOptionsValue = "ON",
                    SiteOptionsStatus = "1",
                    SiteOptionsVariable = "BYPASS_ASSESSMENT_FOR_NON_KEEPER"
                }
            );
            Db.SpecialClassQueueSaveHistory.AddRange(
                new SpecialClassQueueSaveHistory
                {
                    SpecialClassQueueSaveHistoryId = 10,
                    InmateId = 100,
                    FacilityId = 1,
                    SaveDate = DateTime.Now,
                    SaveBy = 12,
                    SpecialClassQueueInterval = 5
                },
                new SpecialClassQueueSaveHistory
                {
                    SpecialClassQueueSaveHistoryId = 11,
                    InmateId = 104,
                    FacilityId = 2,
                    SaveDate = DateTime.Now,
                    SaveBy = 11,
                    SpecialClassQueueInterval = 4
                },
                new SpecialClassQueueSaveHistory
                {
                    SpecialClassQueueSaveHistoryId = 12,
                    InmateId = 106,
                    FacilityId = 2,
                    SaveDate = DateTime.Now,
                    SaveBy = 11,
                    SpecialClassQueueInterval = null
                }
            );
        }
    }
}
