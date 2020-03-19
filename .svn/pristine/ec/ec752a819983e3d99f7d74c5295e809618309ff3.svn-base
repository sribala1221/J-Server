using GenerateTables.Models;
using System;
using System.Collections.Generic;

namespace ServerAPI.ViewModels
{
    public class InmateBookingDetailsCountVm
    {
        public int Info { get; set; }
        public int Charges { get; set; }
        public int Warrant { get; set; }
        public int Sheet { get; set; }
        public int Bail { get; set; }
        public int Sentence { get; set; }
        public int Note { get; set; }
        public int Clear { get; set; }
        public int Caseforms { get; set; }
        public int Caseattach { get; set; }
        public int Casehistory { get; set; }
    }

    public class InmateBookingNumberDetails
    {
        public string ArrestBookingNumber { get; set; }
        public DateTime? BookingDate { get; set; }
        public int? ArrestId { get; set; }
        public int? IncarcerationId { get; set; }
        public int IncarcerationArrestXrefId { get; set; }
        public string ArrestType { get; set; }
        public IDictionary<SubmoduleDetail, int> InmateBookingDetailsCountVm { get; set; }
        public string BookingType { get; set; }
        public string ArrestAbbrevation { get; set; }
        public string CourtDocket { get; set; }
        public DateTime? ArrestDate { get; set; }
        public string ArrestCaseNumber { get; set; }
        public DateTime? ClearDate { get; set; }
        public string ClearReason { get; set; }
        public string ArrestBookingType { get; set; }
        public string Court { get; set; }
        public string ArrestDisposition { get; set; }
    }

    public class DetailVm
    {
        public int DetailId { get; set; }
        public string DetailName { get; set; }
        public bool DetailVisible { get; set; }
        public string DetailToolTip { get; set; }
    }

    public class InmateFileDetailVm
    {

        public FilePersonVm PersonDetails { get; set; }
        public IDictionary<SubmoduleDetail, int> InmateFileCount { get; set; }

        public List<BookingDetailCountVm> ListBookingNumber { get; set; }

        public List<InmateBookingNumberDetails> ListBookingCasesNumber { get; set; }
        public int CourtCommitCnt { get; set; }
    }

    public class BookingCountVm
    {
        public int Overall { get; set; }
        public int Transport { get; set; }
        public int Bookingforms { get; set; }
        public int Bookingattach { get; set; }
        public int Prebook { get; set; }
        public int Intakecurrency { get; set; }
        public int Bookinghistory { get; set; }
        public int Keeper { get; set; }
        public int Bookingsheetforms { get; set; }
    }


    public class BookingDetailCountVm
    {
        public string ArrestBookingNumber { get; set; }
        public int? IncarcerationId { get; set; }
        public DateTime? ReleaseOut { get; set; }
        public DateTime? DateIn { get; set; }
        public IDictionary<SubmoduleDetail, int> BookingCountVm { get; set; }
        public List<InmateBookingNumberDetails> InmateBookingNumberDetails { get; set; }
        public int VerifyId { get; set; }
    }

    public class BookingHistoryVm
    {
        public List<BookingDetailCountVm> BookingDetailCount { get; set; }

    }
}
