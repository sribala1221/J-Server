using GenerateTables.Models;
using System;

// ReSharper disable once CheckNamespace
namespace ServerAPI.Tests
{
    public partial class DbInitialize
    {
        private void IncarcerationDetails()
        {
            Db.Incarceration.AddRange(
                new Incarceration
                {
                    IncarcerationId = 10,
                    InmateId = 100,
                    UsedPersonFrist = "SANGEETHA",
                    UsedPersonLast = "VIJAYA",
                    UsedPersonSuffix = "V",
                    DateIn = DateTime.Now.AddDays(-1),
                    InOfficerId = 12,
                    TransportFlag = 1,
                    TransportHoldName = "SRI SASTHA",
                    TransportHoldType = 0,
                    TransportInstructions = "MORE PEOPLE STAY IN SINGLE HALL",
                    TransportScheduleDate = DateTime.Now,
                    TransportInmateCautions = "PROPER COUNT",
                    TransportInmateBail = "TEMPORARY INMATE",
                    ReleaseClearBy = 12,
                    ReleaseClearDate = DateTime.Now,
                    OverallSentStartDate = DateTime.Now,
                    ExpediteBookingBy = 12,
                    FacilityIdIn = 1
                },
                new Incarceration
                {
                    IncarcerationId = 11,
                    InmateId = 101,
                    UsedPersonFrist = "SUGIR",
                    UsedPersonLast = "KRISHNA",
                    UsedPersonMiddle = "SURUTHI",
                    OverallFinalReleaseDate = DateTime.Now,
                    DateIn = DateTime.Now.AddDays(-2),
                    InOfficerId = 11,
                    TransportHoldName = "MASTERO VA",
                    TransportHoldType = 6,
                    TransportInstructions = "PROTEST PERSON",
                    TransportScheduleDate = DateTime.Now,
                    TransportInmateCautions = "MANY PEOPLES",
                    TransportInmateBail = "NO JAIL",
                    ReleaseClearBy = 11,
                    ReleaseCompleteBy = 1,
                    ReleaseClearDate = DateTime.Now,
                    OverallSentStartDate = DateTime.Now,
                    BookingWizardLastStepId = 7,
                    BookCompleteFlag = 1,
                    ChargeLevel = "F",
                    ChargeLevelOverrideFlag = 0,
                    FacilityIdIn = 1
                },
                new Incarceration
                {
                    IncarcerationId = 12,
                    InmateId = 103,
                    UsedPersonFrist = "VIJAI",
                    UsedPersonLast = "KUMAR",
                    UsedPersonMiddle = "SATHEES",
                    OverallFinalReleaseDate = DateTime.Now,
                    DateIn = DateTime.Now,
                    ReleaseOut = DateTime.Now,
                    InOfficerId = 12,
                    TransportFlag = 0,
                    ReleaseClearBy = 12,
                    ReleaseClearDate = DateTime.Now,
                    OverallSentStartDate = DateTime.Now,
                    FacilityIdIn = 1
                },
                new Incarceration
                {
                    IncarcerationId = 13,
                    InmateId = 102,
                    UsedPersonFrist = "SANKAR",
                    UsedPersonLast = "GEETHA",
                    UsedPersonMiddle = "PRAGATHISVARAN",
                    OverallFinalReleaseDate = DateTime.Now,
                    DateIn = DateTime.Now,
                    ReleaseOut = DateTime.Now,
                    InOfficerId = 11,
                    TransportFlag = 0,
                    ReleaseClearBy = 12,
                    ReleaseClearDate = DateTime.Now,
                    OverallSentStartDate = DateTime.Now,
                    FacilityIdIn = 1
                },
                new Incarceration
                {
                    IncarcerationId = 14,
                    InmateId = 104,
                    UsedPersonFrist = "HAYATHBASHA",
                    UsedPersonLast = "RAHIMA",
                    UsedPersonMiddle = "NASRIN",
                    OverallFinalReleaseDate = DateTime.Now,
                    DateIn = DateTime.Now,
                    ReleaseOut = null,
                    InOfficerId = 11,
                    TransportFlag = 0,
                    ReleaseClearBy = 12,
                    ReleaseClearDate = DateTime.Now,
                    OverallSentStartDate = DateTime.Now,
                    FacilityIdIn = 1
                },
                new Incarceration
                {
                    IncarcerationId = 15,
                    InmateId = 108,
                    UsedPersonFrist = "MARSHAL",
                    UsedPersonLast = "RAAV",
                    UsedPersonMiddle = "KIRAN",
                    OverallFinalReleaseDate = DateTime.Now,
                    DateIn = DateTime.Now,
                    InOfficerId = 12,
                    TransportFlag = 0,
                    ReleaseClearBy = 12,
                    BookingWizardLastStepId = 7,
                    ReleaseCompleteFlag = 0,
                    ReleaseClearDate = null,
                    ReleaseClearFlag = 1,
                    OverallSentManual = 1,
                    OverallSentExpDate = DateTime.Now,
                    OverallSentGtDays = 1,
                    FacilityIdIn = 1,
                    BookCompleteFlag = 1,
                    BookAndReleaseFlag = 1
                },
                new Incarceration
                {
                    IncarcerationId = 16,
                    InmateId = 110,
                    UsedPersonFrist = "PRABHU",
                    UsedPersonLast = "DEVA",
                    UsedPersonMiddle = null,
                    DateIn = DateTime.Now,
                    InOfficerId = 11,
                    TransportFlag = 0,
                    ReleaseClearBy = 12,
                    ReleaseOut = null,
                    FacilityIdIn = 1
                },
                new Incarceration
                {
                    IncarcerationId = 17,
                    InmateId = 106,
                    UsedPersonFrist = "NARESH",
                    UsedPersonLast = "RAJ",
                    UsedPersonMiddle = "ABI",
                    DateIn = DateTime.Now,
                    InOfficerId = 12,
                    TransportHoldName = "DIO 150",
                    TransportHoldType = 2,
                    TransportInstructions = "SUPER FAST",
                    TransportScheduleDate = DateTime.Now,
                    TransportInmateCautions = null,
                    TransportInmateBail = "BAIL HAVE",
                    ReleaseClearBy = 11,
                    OverallSentStartDate = DateTime.Now,
                    FacilityIdIn = 2
                },
                new Incarceration
                {
                    IncarcerationId = 18,
                    InmateId = 107,
                    UsedPersonFrist = "SENTHIL",
                    UsedPersonLast = "MURUGAN",
                    UsedPersonMiddle = null,
                    DateIn = DateTime.Now,
                    InOfficerId = 11,
                    TransportFlag = 0,
                    ReleaseClearBy = 11,
                    ReleaseOut = DateTime.Now,
                    IntakeCompleteFlag = 1,
                    IntakeCompleteDate = DateTime.Now,
                    ChargeLevel = "F",
                    ChargeLevelOverrideFlag = 0,
                    FacilityIdIn = 1
                },
                new Incarceration
                {
                    IncarcerationId = 19,
                    InmateId = 107,
                    UsedPersonFrist = "ARUN",
                    UsedPersonLast = "RAJ",
                    UsedPersonMiddle = "BAI",
                    DateIn = DateTime.Now.AddDays(-1),
                    InOfficerId = 12,
                    TransportFlag = 0,
                    ReleaseClearBy = 11,
                    ReleaseOut = null,
                    BookingSupervisorCompleteFlag = 1,
                    IntakeCompleteDate = DateTime.Now,
                    FacilityIdIn = 2
                },
                new Incarceration
                {
                    IncarcerationId = 20,
                    InmateId = 120,
                    UsedPersonFrist = "REVATHI",
                    UsedPersonLast = "MOORTHY",
                    UsedPersonMiddle = null,
                    DateIn = DateTime.Now.AddDays(-2),
                    InOfficerId = 11,
                    TransportFlag = 0,
                    ReleaseClearBy = 12,
                    ReleaseOut = DateTime.Now,
                    ReleaseCompleteBy = 13,
                    FacilityIdIn = 1
                },
                new Incarceration
                {
                    IncarcerationId = 21,
                    InmateId = 102,
                    UsedPersonFrist = "SANKAR",
                    UsedPersonLast = "GEETHA",
                    UsedPersonMiddle = "PRAGATHISVARAN",
                    ReleaseOut = DateTime.Now.AddDays(-2),
                    DateIn = DateTime.Now,
                    InOfficerId = 11,
                    TransportFlag = 0,
                    ReleaseClearBy = 12,
                    BookingWizardLastStepId = 6,
                    FacilityIdIn = 1
                },
                new Incarceration
                {
                    IncarcerationId = 22,
                    InmateId = 141,
                    UsedPersonFrist = "BISHMA",
                    UsedPersonLast = null,
                    UsedPersonMiddle = "GANGA",
                    DateIn = DateTime.Now,
                    InOfficerId = 12,
                    BookingWizardLastStepId = 7,
                    DesireClasses = true,
                    DesireWorkCrew = true,
                    DesireFurlough = false,
                    FacilityIdIn = 1
                },
                new Incarceration
                {
                    IncarcerationId = 23,
                    UsedPersonFrist = "ANU",
                    UsedPersonMiddle = "RAGAV",
                    DateIn = DateTime.Now.AddDays(-1),
                    InmateId = 140,
                    InOfficerId = 13,
                    ReleaseOut = DateTime.Now,
                    FacilityIdIn = 2
                },
                new Incarceration
                {
                    IncarcerationId = 24,
                    InmateId = 141,
                    UsedPersonFrist = "GOKUL",
                    UsedPersonLast = null,
                    UsedPersonMiddle = "RAJ",
                    DateIn = DateTime.Now,
                    InOfficerId = 13,
                    BookingWizardLastStepId = 7,
                    DesireClasses = true,
                    DesireWorkCrew = true,
                    DesireFurlough = false,
                    BookCompleteFlag = 1,
                    IntakeCompleteFlag = 1,
                    BookAndReleaseFlag = null,
                    ReleaseOut = DateTime.Now,
                    FacilityIdIn = 1
                },
                new Incarceration
                {
                    IncarcerationId = 25,
                    InmateId = 125,
                    UsedPersonFrist = "STRUTHI",
                    UsedPersonLast = "HASAN",
                    UsedPersonMiddle = null,
                    DateIn = DateTime.Now,
                    InOfficerId = 13,
                    BookingWizardLastStepId = 7,
                    DesireClasses = true,
                    DesireWorkCrew = true,
                    DesireFurlough = false,
                    BookCompleteFlag = 1,
                    IntakeCompleteFlag = 1,
                    BookAndReleaseFlag = null,
                    OverallFinalReleaseDate = DateTime.Now,
                    ReleaseOut = null,
                    FacilityIdIn = 1
                },
                new Incarceration
                {
                    IncarcerationId = 26,
                    InmateId = 105,
                    UsedPersonLast = null,
                    DateIn = DateTime.Now,
                    InOfficerId = 13,
                    BookingWizardLastStepId = 7,
                    DesireClasses = true,
                    DesireWorkCrew = true,
                    DesireFurlough = false,
                    BookCompleteFlag = 1,
                    IntakeCompleteFlag = 1,
                    BookAndReleaseFlag = null,
                    ReleaseOut = DateTime.Now,
                    ChargeLevel = "AS",
                    ChargeLevelOverrideFlag = 1,
                    ExpediteBookingBy = 11,
                    FacilityIdIn = 1
                },
                new Incarceration
                {
                    IncarcerationId = 27,
                    InmateId = 105,
                    UsedPersonLast = null,
                    DateIn = DateTime.Now,
                    InOfficerId = 12,
                    BookingWizardLastStepId = 7,
                    OverallFinalReleaseDate = DateTime.Now,
                    DesireClasses = true,
                    DesireWorkCrew = true,
                    DesireFurlough = false,
                    BookCompleteFlag = 1,
                    IntakeCompleteFlag = 1,
                    BookAndReleaseFlag = null,
                    ReleaseOut = null,
                    ChargeLevel = "VALUE",
                    ChargeLevelOverrideFlag = 1,
                    TransferApprovalSaveBy = 12,
                    TransferEligibleSaveBy = 12,
                    ExpediteBookingBy = 12,
                    FacilityIdIn = 1
                },
                new Incarceration
                {
                    IncarcerationId = 28,
                    InmateId = 125,
                    UsedPersonFrist = "STRUTHI",
                    UsedPersonLast = "HASAN",
                    UsedPersonMiddle = null,
                    DateIn = DateTime.Now,
                    InOfficerId = 13,
                    BookingWizardLastStepId = 7,
                    OverallFinalReleaseDate = DateTime.Now,
                    DesireClasses = true,
                    DesireWorkCrew = true,
                    DesireFurlough = false,
                    BookCompleteFlag = 1,
                    IntakeCompleteFlag = 1,
                    BookAndReleaseFlag = null,
                    ReleaseOut = DateTime.Now,
                    FacilityIdIn = 1,
                    FacilityIdOut = 1
                },
                new Incarceration
                {
                    IncarcerationId = 29,
                    InmateId = 130,
                    UsedPersonFrist = "NARESH",
                    UsedPersonLast = "RAJ",
                    UsedPersonMiddle = "ABI",
                    DateIn = DateTime.Now,
                    InOfficerId = 12,
                    TransportHoldName = "DIO 160",
                    TransportHoldType = 2,
                    TransportInstructions = "SUPER FAST",
                    TransportScheduleDate = DateTime.Now,
                    TransportInmateCautions = null,
                    TransportInmateBail = "NO BAIL",
                    ReleaseClearBy = 11,
                    OverallSentStartDate = DateTime.Now,
                    FacilityIdIn = 1
                },
                new Incarceration
                {
                    IncarcerationId = 30,
                    InmateId = 103,
                    UsedPersonFrist = "VIJAI",
                    UsedPersonLast = "KUMAR",
                    UsedPersonMiddle = "SATHEES",
                    OverallFinalReleaseDate = DateTime.Now,
                    DateIn = DateTime.Now,
                    ReleaseOut = null,
                    InOfficerId = 12,
                    TransportFlag = 0,
                    ReleaseClearBy = 12,
                    ReleaseClearDate = DateTime.Now,
                    OverallSentStartDate = DateTime.Now,
                    FacilityIdIn = 1
                },
                new Incarceration
                {
                    IncarcerationId = 31,
                    InmateId = 130,
                    UsedPersonFrist = "RAVI",
                    UsedPersonLast = null,
                    UsedPersonMiddle = "KUMAR",
                    OverallFinalReleaseDate = DateTime.Now.AddDays(-10),
                    DateIn = DateTime.Now,
                    ReleaseOut = DateTime.Now,
                    InOfficerId = 12,
                    TransportFlag = 0,
                    ReleaseClearBy = 11,
                    ReleaseClearDate = DateTime.Now,
                    OverallSentStartDate = DateTime.Now,
                    FacilityIdIn = 1,
                    FacilityIdOut = 1
                }
            );

            Db.IncarcerationIntakeCurrency.AddRange(
                new IncarcerationIntakeCurrency
                {
                    IncarcerationIntakeCurrencyId = 15,
                    CreateDate = DateTime.Now.AddDays(-15),
                    CreateBy = 12,
                    IncarcerationId = 15,
                    Currency10000Count = 50,
                    Currency1000Count = 10,
                    Currency100Count = 5,
                    Currency25Count = 20,
                    ModifyReason = "NEW AMOUNT"
                },
                new IncarcerationIntakeCurrency
                {
                    IncarcerationIntakeCurrencyId = 16,
                    CreateDate = DateTime.Now.AddDays(-15),
                    CreateBy = 12,
                    IncarcerationId = 15,
                    Currency10000Count = 50,
                    Currency1000Count = 10,
                    Currency100Count = 5,
                    Currency25Count = 20,
                    ModifyReason = "BALANCE AMOUNT"
                },
                new IncarcerationIntakeCurrency
                {
                    IncarcerationIntakeCurrencyId = 17,
                    CreateDate = DateTime.Now.AddDays(-15),
                    CreateBy = 12,
                    IncarcerationId = 18,
                    Currency01Count = 50,
                    Currency100Count = 10,
                    Currency50Count = 5,
                    Currency5000Count = 15,
                    Currency25Count = 20,
                    ModifyReason = "NEW CURRENCY"
                }
                );

            Db.IncarcerationFacilityHistory.AddRange(
                new IncarcerationFacilityHistory
                {
                    InmateId = 104,
                    FacilityId = 1,
                    IncarcerationId = 15,
                    IncarcerationFacilityHistoryId = 5,
                    MoveDate = DateTime.Now,
                    MoveDateBy = 1
                },
                new IncarcerationFacilityHistory
                {
                    FacilityId = 1,
                    IncarcerationId = 23,
                    IncarcerationFacilityHistoryId = 6,
                    MoveDate = DateTime.Now,
                    MoveDateBy = null,
                    MoveDateThru = DateTime.Now
                }
            );
            Db.IncarcerationArrestXref.AddRange(
                new IncarcerationArrestXref
                {
                    IncarcerationArrestXrefId = 5,
                    ArrestId = 5,
                    BookingOfficerId = 5,
                    BookingDate = DateTime.Now.AddDays(-3),
                    IncarcerationId = 10,
                    ReleaseReason = "BAIL"
                },
                new IncarcerationArrestXref
                {
                    IncarcerationArrestXrefId = 6,
                    ArrestId = 10,
                    BookingOfficerId = 5,
                    BookingDate = DateTime.Now.AddDays(-3),
                    IncarcerationId = 12,
                    ReleaseReason = "CASE COMPLETED"
                },
                new IncarcerationArrestXref
                {
                    IncarcerationArrestXrefId = 7,
                    ArrestId = 6,
                    BookingOfficerId = 11,
                    BookingDate = DateTime.Now,
                    IncarcerationId = 11,
                    ReleaseReason = "NO COMPLAINT"
                },
                new IncarcerationArrestXref
                {
                    IncarcerationArrestXrefId = 8,
                    ArrestId = 7,
                    BookingOfficerId = 12,
                    BookingDate = DateTime.Now.AddDays(-1),
                    IncarcerationId = 13,
                    ReleaseReason = "WORK TIME COMPLETE"
                },
                new IncarcerationArrestXref
                {
                    IncarcerationArrestXrefId = 9,
                    ArrestId = 8,
                    BookingOfficerId = 12,
                    BookingDate = DateTime.Now,
                    IncarcerationId = 13,
                    ReleaseReason = "CASE DISMISSED",
                    ReleaseDate = DateTime.Now
                },
                new IncarcerationArrestXref
                {
                    IncarcerationArrestXrefId = 10,
                    ArrestId = 9,
                    BookingOfficerId = 11,
                    BookingDate = DateTime.Now,
                    IncarcerationId = 15
                },
                new IncarcerationArrestXref
                {
                    IncarcerationArrestXrefId = 11,
                    ArrestId = 8,
                    BookingOfficerId = 12,
                    BookingDate = DateTime.Now,
                    IncarcerationId = 18
                },
                new IncarcerationArrestXref
                {
                    IncarcerationArrestXrefId = 12,
                    BookingOfficerId = 11,
                    IncarcerationId = 14,
                    ArrestId = 11,
                    BookingDate = DateTime.Now,
                    ReleaseReason = "NO COMPLAINT"
                },
                new IncarcerationArrestXref
                {
                    IncarcerationArrestXrefId = 13,
                    BookingOfficerId = 12,
                    IncarcerationId = 10,
                    ArrestId = 5,
                    BookingDate = DateTime.Now
                },
                new IncarcerationArrestXref
                {
                    IncarcerationArrestXrefId = 14,
                    BookingOfficerId = 11,
                    IncarcerationId = 12,
                    ArrestId = 8,
                    BookingDate = DateTime.Now
                },
                new IncarcerationArrestXref
                {
                    IncarcerationArrestXrefId = 15,
                    BookingOfficerId = 12,
                    IncarcerationId = 20,
                    ArrestId = 10,
                    BookingDate = DateTime.Now
                },
                new IncarcerationArrestXref
                {
                    IncarcerationArrestXrefId = 16,
                    IncarcerationId = 26,
                    BookingOfficerId = 11,
                    ArrestId = 15,
                    BookingDate = DateTime.Now
                },
                new IncarcerationArrestXref
                {
                    IncarcerationArrestXrefId = 17,
                    IncarcerationId = 29,
                    BookingOfficerId = 13,
                    ArrestId = 5,
                    BookingDate = DateTime.Now
                },
                new IncarcerationArrestXref
                {
                    IncarcerationArrestXrefId = 18,
                    IncarcerationId = 27,
                    BookingOfficerId = 12,
                    ArrestId = 5,
                    BookingDate = DateTime.Now
                },
                new IncarcerationArrestXref
                {
                    IncarcerationArrestXrefId = 19,
                    IncarcerationId = 16,
                    BookingOfficerId = 11,
                    ArrestId = 6,
                    BookingDate = DateTime.Now
                },
                new IncarcerationArrestXref
                {
                    IncarcerationArrestXrefId = 20,
                    IncarcerationId = 19,
                    BookingOfficerId = 12,
                    ArrestId = 6,
                    BookingDate = DateTime.Now
                },
                new IncarcerationArrestXref
                {
                    IncarcerationArrestXrefId = 21,
                    IncarcerationId = 30,
                    BookingOfficerId = 11,
                    ArrestId = 7,
                    BookingDate = DateTime.Now
                },
                new IncarcerationArrestXref
                {
                    IncarcerationArrestXrefId = 22,
                    IncarcerationId = 22,
                    BookingOfficerId = 12,
                    ArrestId = 9,
                    BookingDate = DateTime.Now
                },
                new IncarcerationArrestXref
                {
                    IncarcerationArrestXrefId = 23,
                    IncarcerationId = 22,
                    BookingOfficerId = 12,
                    ArrestId = 17,
                    BookingDate = DateTime.Now
                },
                new IncarcerationArrestXref
                {
                    IncarcerationArrestXrefId = 24,
                    IncarcerationId = 21,
                    BookingOfficerId = 11,
                    ArrestId = 16,
                    BookingDate = DateTime.Now
                }
            );

            Db.IncarcerationTransferSaveHistory.AddRange(
                new IncarcerationTransferSaveHistory
                {
                    IncarcerationTransferSaveHistoryid = 15,
                    Incarcerationid = 15,
                    TransferEligibleLookup = 1,
                    TransferApprovalLookup = 0,
                    TransferEligibleDate = DateTime.Now,
                    TransferEligibleNote = "CHANGED",
                    TransferApprovalSaveBy = 12
                },
                new IncarcerationTransferSaveHistory
                {
                    IncarcerationTransferSaveHistoryid = 16,
                    Incarcerationid = 15,
                    TransferEligibleLookup = 1,
                    TransferApprovalLookup = 1,
                    TransferEligibleDate = DateTime.Now,
                    TransferEligibleNote = "MOVED TO NEW ROOM",
                    TransferApprovalSaveBy = 11,
                    TransferEligibleSaveBy = 13
                }

                );
        }
    }
}
