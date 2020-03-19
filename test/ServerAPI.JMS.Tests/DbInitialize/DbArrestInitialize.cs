using GenerateTables.Models;
using System;

// ReSharper disable once CheckNamespace
namespace ServerAPI.Tests
{
    public partial class DbInitialize
    {
        private void ArrestDetails()
        {
            Db.Arrest.AddRange(
                new Arrest
                {
                    ArrestId = 5,
                    ArrestType = "4",
                    ArrestBookingNo = "160001194",
                    ArrestSentenceCode = 1,
                    BailNoBailFlag = 0,
                    BailAmount = 1000,
                    ArrestSentenceReleaseDate = DateTime.Now,
                    InmateId = 110,
                    ArrestActive = 1,
                    ArrestOfficerId = 11,
                    ArrestBookingOfficerId = 11,
                    ArrestingAgencyId = 5,
                    BookingAgencyId = 5,
                    ArrestArraignmentCourtId = 5,
                    ArrestCaseNumber = "CASE_01",
                    ArrestCourtDocket = "DOCKET_100100",
                    ArrestDate = DateTime.Now.Date,
                    ArrestBillingAgencyId = 5,
                    ArrestSiteBookingNo = "45000410",
                    ArrestSentenceDayForDayAllowed = 1,
                    ArrestSentenceMethodId = 15
                },
                new Arrest
                {
                    ArrestId = 6,
                    ArrestType = "5",
                    ArrestBookingNo = "160001191",
                    ArrestSentenceCode = 5,
                    BailNoBailFlag = 1,
                    BailAmount = 5000,
                    ArrestSentenceReleaseDate = null,
                    InmateId = 100,
                    ArrestOfficerId = 12,
                    ArrestBookingOfficerId = 11,
                    ArrestingAgencyId = 6,
                    BookingAgencyId = 6,
                    ArrestArraignmentCourtId = 5,
                    ArrestCaseNumber = "CASE_02",
                    ArrestCourtDocket = "DOCKET_100101",
                    ArrestDate = DateTime.Now.AddDays(-1),
                    ArrestBillingAgencyId = 6,
                    ArrestSiteBookingNo = "45000200",
                    ArrestSentenceDisciplinaryDaysFlag = 1,
                    ArrestSentenceDayForDayAllowed = 1
                },
                new Arrest
                {
                    ArrestId = 7,
                    ArrestType = "5",
                    ArrestBookingNo = "160001192",
                    ArrestSentenceCode = 4,
                    BailNoBailFlag = 1,
                    BailAmount = 2000,
                    ArrestSentenceReleaseDate = DateTime.Now,
                    InmateId = 103,
                    ArrestOfficerId = 13,
                    ArrestBookingOfficerId = 12,
                    ArrestingAgencyId = 5,
                    BookingAgencyId = 5,
                    ArrestArraignmentCourtId = 6,
                    ArrestCaseNumber = "CASE_03",
                    ArrestCourtDocket = "DOCKET_100102",
                    ArrestDate = DateTime.Now.AddDays(-6),
                    ArrestBillingAgencyId = 5,
                    ArrestSiteBookingNo = "45001400"
                },
                new Arrest
                {
                    ArrestId = 8,
                    ArrestType = "5",
                    ArrestBookingNo = "160001132",
                    BailAmount = 2000,
                    ArrestSentenceReleaseDate = DateTime.Now,
                    InmateId = 102,
                    ArrestOfficerId = 12,
                    ArrestBookingOfficerId = 11,
                    ArrestingAgencyId = 6,
                    BookingAgencyId = 5,
                    ArrestArraignmentCourtId = 6,
                    ArrestCaseNumber = "CASE_04",
                    ArrestCourtDocket = "DOCKET_100132",
                    ArrestDate = DateTime.Now.AddDays(-5),
                    ArrestBillingAgencyId = 6,
                    ArrestSentenceWeekender = 1,
                    ArrestSiteBookingNo = "45001520",
                    ArrestSentenceNoEarlyRelease = 0,
                    ArrestSentenceNoDayForDay = 1,
                    ArrestSentenceNoLocalParole = 1,
                    ArrestSentenceAltSentNotAllowed = 0,
                    ArrestConditionsOfRelease = "NEW ARREST CASE"

                },
                new Arrest
                {
                    ArrestId = 9,
                    ArrestType = "4",
                    ArrestBookingNo = "160001160",
                    BailAmount = 1500,
                    ArrestSentenceReleaseDate = DateTime.Now,
                    InmateId = 105,
                    ArrestOfficerId = 13,
                    ArrestBookingOfficerId = 11,
                    ArrestingAgencyId = 6,
                    BookingAgencyId = 5,
                    ArrestArraignmentCourtId = 6,
                    ArrestCaseNumber = "CASE_05",
                    ArrestCourtDocket = "DOCKET_100147",
                    ArrestDate = DateTime.Now.AddDays(-2),
                    ArrestBillingAgencyId = 5,
                    ArrestSentenceWeekender = 2,
                    ArrestSiteBookingNo = "45008522"
                },
                new Arrest
                {
                    ArrestId = 10,
                    ArrestType = "5",
                    ArrestBookingNo = "160001162",
                    ArrestSentenceCode = 4,
                    BailNoBailFlag = 1,
                    BailAmount = 4500,
                    ArrestSentenceReleaseDate = null,
                    InmateId = 107,
                    ArrestOfficerId = 13,
                    ArrestBookingOfficerId = 11,
                    ArrestingAgencyId = 6,
                    BookingAgencyId = 5,
                    ArrestArraignmentCourtId = 5,
                    ArrestCaseNumber = "CASE_08",
                    ArrestCourtDocket = "DOCKET_100200",
                    ArrestDate = DateTime.Now.AddDays(-2),
                    ArrestBillingAgencyId = 6,
                    ArrestSiteBookingNo = "45000350"
                },
                new Arrest
                {
                    ArrestId = 11,
                    ArrestType = "5",
                    ArrestBookingNo = "160001172",
                    ArrestSentenceCode = 5,
                    BailNoBailFlag = 1,
                    BailAmount = 7500,
                    ArrestSentenceReleaseDate = null,
                    InmateId = 104,
                    ArrestOfficerId = 13,
                    ArrestBookingOfficerId = 11,
                    ArrestingAgencyId = 6,
                    BookingAgencyId = 5,
                    ArrestArraignmentCourtId = 5,
                    ArrestCaseNumber = "CASE_09",
                    ArrestCourtDocket = "DOCKET_100120",
                    ArrestDate = DateTime.Now.AddDays(-2),
                    ArrestBillingAgencyId = 7,
                    ArrestSiteBookingNo = "45000700"
                },
                new Arrest
                {
                    ArrestId = 12,
                    ArrestType = "7",
                    ArrestBookingNo = "160001172",
                    ArrestSentenceCode = 4,
                    BailNoBailFlag = 1,
                    BailAmount = 5500,
                    InmateId = 120,
                    ArrestOfficerId = 13,
                    ArrestBookingOfficerId = 11,
                    ArrestingAgencyId = 6,
                    BookingAgencyId = 5,
                    ArrestArraignmentCourtId = 5,
                    ArrestCaseNumber = "CASE_09",
                    ArrestCourtDocket = "DOCKET_100121",
                    ArrestDate = DateTime.Now.AddDays(-2),
                    ArrestBillingAgencyId = 7,
                    ArrestSiteBookingNo = "45000710"
                },
                new Arrest
                {
                    ArrestId = 13,
                    ArrestType = "5",
                    ArrestBookingNo = "160001180",
                    BailAmount = 4500,
                    InmateId = 125,
                    ArrestingAgencyId = 6,
                    BookingAgencyId = 7,
                    ArrestArraignmentCourtId = 5,
                    ArrestCaseNumber = "CASE_10",
                    ArrestCourtDocket = "DOCKET_100125",
                    ArrestSiteBookingNo = "45000711"
                },
                new Arrest
                {
                    ArrestId = 14,
                    ArrestType = "6",
                    BailType = "NO BAIL",
                    ArrestBookingNo = "160001162",
                    ArrestSentenceCode = 4,
                    BailNoBailFlag = 1,
                    BailAmount = 5000,
                    ArrestSentenceReleaseDate = null,
                    InmateId = 107,
                    ArrestOfficerId = 13,
                    ArrestBookingOfficerId = 11,
                    ArrestingAgencyId = 6,
                    BookingAgencyId = 5,
                    ArrestArraignmentCourtId = 5,
                    ArrestCaseNumber = "CASE_08",
                    ArrestCourtDocket = "DOCKET_100210",
                    ArrestDate = DateTime.Now.AddDays(-3),
                    ArrestBillingAgencyId = 6,
                    ArrestSiteBookingNo = "45000712"
                },
                new Arrest
                {
                    ArrestId = 15,
                    ArrestType = "5",
                    ArrestBookingNo = "160001163",
                    BailAmount = 1500,
                    ArrestSentenceReleaseDate = DateTime.Now,
                    InmateId = 105,
                    ArrestOfficerId = 13,
                    ArrestBookingOfficerId = 12,
                    ArrestingAgencyId = 6,
                    BookingAgencyId = 5,
                    ArrestArraignmentCourtId = 6,
                    ArrestCaseNumber = "CASE_09",
                    ArrestCourtDocket = "DOCKET_100215",
                    ArrestDate = DateTime.Now.AddDays(-2),
                    ArrestBillingAgencyId = 6,
                    ArrestSentenceWeekender = 2,
                    ArrestSiteBookingNo = "45008713",
                    BailType = "NO BAIL"
                },
                new Arrest
                {
                    ArrestId = 16,
                    ArrestType = "5",
                    ArrestBookingNo = "160001164",
                    BailAmount = 1500,
                    ArrestSentenceReleaseDate = DateTime.Now,
                    InmateId = 110,
                    ArrestOfficerId = 11,
                    ArrestBookingOfficerId = 12,
                    ArrestingAgencyId = 6,
                    BookingAgencyId = 5,
                    ArrestArraignmentCourtId = 7,
                    ArrestCaseNumber = "CASE_10",
                    ArrestCourtDocket = "DOCKET_100216",
                    ArrestDate = DateTime.Now.AddDays(-2),
                    ArrestBillingAgencyId = 6,
                    ArrestSentenceWeekender = 2,
                    ArrestSiteBookingNo = "45008714",
                    BailType = "NO BAIL"
                },
                new Arrest
                {
                    ArrestId = 17,
                    ArrestType = "8",
                    ArrestBookingNo = "160001165",
                    BailAmount = 2000,
                    ArrestSentenceReleaseDate = DateTime.Now,
                    InmateId = 105,
                    ArrestOfficerId = 13,
                    ArrestBookingOfficerId = 12,
                    ArrestingAgencyId = 6,
                    BookingAgencyId = 5,
                    ArrestArraignmentCourtId = 6,
                    ArrestCaseNumber = "CASE_11",
                    ArrestCourtDocket = "DOCKET_100215",
                    ArrestDate = DateTime.Now.AddDays(-2),
                    ArrestBillingAgencyId = 6,
                    ArrestSentenceWeekender = 2,
                    ArrestSiteBookingNo = "45008713",
                    BailType = "CASH BAIL OTHER"
                }

            );
            Db.ArrestClearHistory.AddRange(
                new ArrestClearHistory
                {
                    ArrestClearHistoryId = 10,
                    CreateDate = DateTime.Now.AddDays(-4),
                    ArrestId = 7,
                    PersonnelId = 12,
                    ArrestHistoryList = null
                },
                new ArrestClearHistory
                {
                    ArrestClearHistoryId = 11,
                    CreateDate = DateTime.Now.AddDays(-4),
                    ArrestId = 6,
                    PersonnelId = 11,
                    ArrestHistoryList =
                        @"{ 'BOOKING NUMBER':'45000200','COURT DOCKET':'DOCKET_100200',  'SCHEDULED CLEAR':'UNSENT', 'CLEAR REASON':'DIED WHILE IN CUSTODY', 'CLEAR OFFICER':'SANGEETHA', 'CLEAR DATE':'11/11/2017 12:26:00 PM', 'CLEAR NOTE':null, 'WARNING':'None' }"
                }
            );
            Db.ArrestSentenceMethodOerc.AddRange(
                new ArrestSentenceMethodOerc
                {

                    ArrestSentenceMethodOercid = 1,
                    Oerccredit = 1,
                    OercdaysRange = 2,
                    OercdaysRangeUseMaxD = 3,
                    OercdaysRangeUseMaxDts = 0,
                    Oercvisible = 1
                },
                new ArrestSentenceMethodOerc
                {

                    ArrestSentenceMethodOercid = 2,
                    Oerccredit = 0,
                    OercdaysRange = 1,
                    OercdaysRangeUseMaxD = 0,
                    OercdaysRangeUseMaxDts = 2,
                    Oercvisible =2
                }


                );


            Db.ArrestSentenceAttendance.AddRange(
                new ArrestSentenceAttendance
                {
                    ArrestSentenceAttendanceId = 20,
                    ArrestId = 5,
                    DeleteFlag = 0,
                    InmateId = 100,
                    ArrestSentenceAttendanceDayId = 10,
                    CreateBy = 11,
                    CreateDate = DateTime.Now.AddDays(-20),
                    UpdateDate = DateTime.Now,
                    UpdateBy = 12
                },
                new ArrestSentenceAttendance
                {
                    ArrestSentenceAttendanceId = 21,
                    ArrestId = 6,
                    DeleteFlag = 0,
                    InmateId = 101,
                    ArrestSentenceAttendanceDayId = 11,
                    CreateBy = 12,
                    CreateDate = DateTime.Now.AddDays(-18),
                    UpdateDate = DateTime.Now,
                    UpdateBy = 11
                },
                new ArrestSentenceAttendance
                {
                    ArrestSentenceAttendanceId = 22,
                    ArrestId = null,
                    DeleteFlag = 0,
                    InmateId = 125,
                    ArrestAppliedDate = DateTime.Now.Date,
                    ArrestSentenceAttendanceDayId = 12,
                    CreateBy = 11,
                    CreateDate = DateTime.Now.AddDays(-10)
                },
                new ArrestSentenceAttendance
                {
                    ArrestSentenceAttendanceId = 23,
                    ArrestId = null,
                    DeleteFlag = 0,
                    InmateId = 105,
                    ArrestAppliedDate = DateTime.Now.Date,
                    ArrestSentenceAttendanceDayId = 13,
                    CreateBy = 12,
                    CreateDate = DateTime.Now.AddDays(-10)
                },
                new ArrestSentenceAttendance
                {
                    ArrestSentenceAttendanceId = 24,
                    ArrestId = null,
                    DeleteFlag = 1,
                    InmateId = 106,
                    ArrestAppliedDate = DateTime.Now.Date,
                    ArrestSentenceAttendanceDayId = 13,
                    CreateBy = 12,
                    CreateDate = DateTime.Now.AddDays(-10)
                },
                new ArrestSentenceAttendance
                {
                    ArrestSentenceAttendanceId = 25,
                    ArrestId = null,
                    DeleteFlag = 0,
                    InmateId = 107,
                    ArrestAppliedDate = DateTime.Now.Date,
                    ArrestSentenceAttendanceDayId = 13,
                    CreateBy = 12,
                    CreateDate = DateTime.Now.AddDays(-10)
                },
                new ArrestSentenceAttendance
                {
                    ArrestSentenceAttendanceId = 26,
                    ArrestId = null,
                    DeleteFlag = 0,
                    InmateId = 110,
                    ArrestAppliedDate = DateTime.Now.Date,
                    ArrestSentenceAttendanceDayId = 11,
                    CreateBy = 11,
                    AttendCredit = 2,
                    CreateDate = DateTime.Now.AddDays(-10)
                },
                new ArrestSentenceAttendance
                {
                    ArrestSentenceAttendanceId = 27,
                    ArrestId = 16,
                    DeleteFlag = 0,
                    InmateId = 111,
                    ArrestAppliedDate = DateTime.Now.Date,
                    ArrestSentenceAttendanceDayId = 12,
                    CreateBy = 11,
                    AttendCredit = 2,
                    CreateDate = DateTime.Now.AddDays(-5)
                }
            );
            Db.ArrestSentenceAttendanceDay.AddRange(
                new ArrestSentenceAttendanceDay
                {
                    ArrestSentenceAttendanceDayId = 10,
                    AttendanceDate = DateTime.Now.Date,
                    CreateDate = DateTime.Now,
                    CreateBy = 11
                },
                new ArrestSentenceAttendanceDay
                {
                    ArrestSentenceAttendanceDayId = 11,
                    AttendanceDate = DateTime.Now.Date.AddDays(-5),
                    CreateDate = DateTime.Now,
                    CreateBy = 12

                },
                new ArrestSentenceAttendanceDay
                {
                    ArrestSentenceAttendanceDayId = 12,
                    AttendanceDate = DateTime.Now.Date.AddDays(-2),
                    CreateDate = DateTime.Now,
                    CreateBy = 12

                },
                new ArrestSentenceAttendanceDay
                {
                    ArrestSentenceAttendanceDayId = 13,
                    AttendanceDate = DateTime.Now.Date.AddDays(-1),
                    CreateDate = DateTime.Now,
                    CreateBy = 11

                },
                new ArrestSentenceAttendanceDay
                {
                    ArrestSentenceAttendanceDayId = 14,
                    AttendanceDate = DateTime.Now.Date.AddDays(-3),
                    CreateDate = DateTime.Now,
                    CreateBy = 11

                });

            Db.ArrestCondClear.AddRange(
                new ArrestCondClear
                {
                    ArrestCondClearId = 10,
                    CreateDate = DateTime.Now.AddDays(-15),
                    DeleteFlag = null,
                    CreateBy = 11,
                    DeleteDate = null,
                    DeleteBy = null,
                    CondOfClearance = "CONTACT AGENCY",
                    CondOfClearanceNote = "AGENCY IS CLEARED",
                    ArrestId = 5
                },
                new ArrestCondClear
                {
                    ArrestCondClearId = 11,
                    CreateDate = DateTime.Now.AddDays(-14),
                    DeleteFlag = null,
                    CreateBy = 12,
                    DeleteDate = null,
                    DeleteBy = null,
                    CondOfClearance = "CONFIRM BAIL REQUIREMENTS",
                    CondOfClearanceNote = "CONFORM BAIL WITH ARREST OFFICER",
                    ArrestId = 8
                },
                new ArrestCondClear
                {
                    ArrestCondClearId = 12,
                    CreateDate = DateTime.Now.AddDays(-13),
                    DeleteFlag = 1,
                    CreateBy = 11,
                    DeleteDate = DateTime.Now,
                    DeleteBy = 12,
                    CondOfClearance = "CONTACT PROBATION OFFICER",
                    CondOfClearanceNote = null,
                    ArrestId = 6
                }
                );

            Db.ArrestSentFlag.AddRange(
                new ArrestSentFlag
                {
                    ArrestSentflagId = 15,
                    CreateDate = DateTime.Now.AddDays(-10),
                    DeleteFlag = 0,
                    CreateBy = 12,
                    UpdateBy = 12,
                    DeleteDate = null,
                    Notes = "SET FLAG",
                    SentflagLookupIndex = 1,
                    ArrestId = 7
                },
                new ArrestSentFlag
                {
                    ArrestSentflagId = 16,
                    CreateDate = DateTime.Now.AddDays(-10),
                    DeleteFlag = null,
                    CreateBy = 12,
                    UpdateBy = 12,
                    DeleteDate = null,
                    Notes = null,
                    SentflagLookupIndex = 1,
                    ArrestId = 8
                },
                new ArrestSentFlag
                {
                    ArrestSentflagId = 17,
                    CreateDate = DateTime.Now.AddDays(-10),
                    DeleteFlag = null,
                    CreateBy = 11,
                    UpdateBy = 12,
                    DeleteDate = null,
                    Notes = null,
                    SentflagLookupIndex = 2,
                    ArrestId = 8
                }
                );

            Db.ArrestSentenceAttendanceArrestXref.AddRange(
                new ArrestSentenceAttendanceArrestXref
                {
                    ArrestSentenceAttendanceArrestXrefId = 10,
                    ArrestSentenceAttendanceId = 26,
                    ArrestId = 15,
                    ReCalcBy = 11,
                    ReCalcDate = DateTime.Now,
                    ArrestAppliedDate = null,
                    ArrestAppliedBy = null
                }

                );
            Db.ArrestSentenceMethod.AddRange(
                new ArrestSentenceMethod
                {
                    ArrestSentenceMethodId = 15,
                    CreateDate = DateTime.Now.AddDays(-10),
                    UpdateDate = DateTime.Now.AddDays(-10),
                    CreateBy = 11,
                    UpdateBy = 12,
                    InactiveFlag = 0,
                    ArrestSentenceDdFactor = 0,
                    ArrestSentenceDdFixed = 1,
                    ArrestSentenceDdaDFactor = 0,
                    ArrestSentenceDdaFixed = 1,
                    MethodName = "HALF TIME"
                },
                new ArrestSentenceMethod
                {
                    ArrestSentenceMethodId = 16,
                    CreateDate = DateTime.Now.AddDays(-10),
                    UpdateDate = DateTime.Now.AddDays(-10),
                    CreateBy = 11,
                    UpdateBy = 12,
                    InactiveFlag = 1,
                    ArrestSentenceDdFactor = 0,
                    ArrestSentenceDdFixed = 1,
                    ArrestSentenceDdaDFactor = 0,
                    ArrestSentenceDdaFixed = 1,
                    MethodName = "FULL TIME"
                }



                );

            Db.ArrestNote.AddRange(
                new ArrestNote
                {
                    ArrestId = 5,
                    ArrestNoteId = 5,
                    DeleteDate = null,
                    DeleteFlag = 0,
                    NoteType = "MISC",
                    NoteBy = 11,
                    NoteDate = DateTime.Now.Date
                },
                new ArrestNote
                {
                    ArrestId = 7,
                    ArrestNoteId = 6,
                    DeleteDate = null,
                    DeleteFlag = 0,
                    NoteType = "MISC",
                    NoteBy = 12,
                    NoteDate = DateTime.Now
                },
                new ArrestNote
                {
                    ArrestId = 5,
                    ArrestNoteId = 7,
                    DeleteDate = null,
                    DeleteFlag = 0,
                    NoteType = null,
                    NoteBy = 12,
                    NoteDate = DateTime.Now
                },
                new ArrestNote
                {
                    ArrestNoteId = 8,
                    DeleteDate = null,
                    DeleteFlag = 0,
                    NoteType = null,
                    NoteBy = 11,
                    NoteDate = DateTime.Now,
                    ArrestId = 15
                },
                new ArrestNote
                {
                    ArrestNoteId = 9,
                    DeleteDate = null,
                    DeleteFlag = 0,
                    NoteType = null,
                    NoteBy = 12,
                    NoteDate = DateTime.Now,
                    ArrestId = 14
                }

                );
        }
    }
}
