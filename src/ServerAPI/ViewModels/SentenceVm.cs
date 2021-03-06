﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json.Linq;

namespace ServerAPI.ViewModels
{
    public class SentenceViewerVm
    {
        //public BookingStatusVm BookingStatus { get; set; }
        public List<SentenceVm> SentenceList { get; set; }
        public List<ChargeSentenceVm> ChargeSentence { get; set; }
        //public OverallSentenceVm OverallSentence { get; set; }
        public bool SentenceByCharge { get; set; }
    }

    public class ChargeSentenceViewerVm
    {
        public SentenceDetailsVm SentenceDetails { get; set; }
        public List<ChargeSentenceVm> ChargeSentence { get; set; }
    }

    public class BookingStatusVm
    {
        public int ArrestId { get; set; }
        public int? InmateId { get; set; }
        public string ArrestBookingStatus { get; set; }
        public int? ArrestBookingStatusId { get; set; }
        public List<KeyValuePair<int, string>> ArrestBookingStatusList { get; set; }
        public string ArrestBookingAbbr { get; set; }
        public DateTime? ArrestConvictionDate { get; set; }
        public string ArrestConvictionNote { get; set; }
        public string ArrestBookingNumber { get; set; }
        public string ArrestBookingType { get; set; }
        public string CourtDocket { get; set; }
        public string Court { get; set; }
        public int PersonId { get; set; }
    }

    public class BookingStatusHistoryVm
    {
        public string ArrestBookingStatus { get; set; }
        public string ArrestBookingAbbr { get; set; }
        public int? SavedBy { get; set; }
        public string SavedPersonFirstName { get; set; }
        public string SavedPersonLastName { get; set; }
        public string SavedPersonMiddleName { get; set; }
        public string OfficerBadgeNumber { get; set; }
        public DateTime? SavedDate { get; set; }
        public DateTime? ArrestConvictionDate { get; set; }
        public string ArrestConvictionNote { get; set; }
    }

    public class SentenceDetailsVm
    {
        public int? ArrestId { get; set; }
        public int? ArrestSentenceCode { get; set; }
        public string ArrestSentenceDescription { get; set; }
        public string ArrestBookingNo { get; set; }
        public string Type { get; set; }
        public string ArrestCourtDocket { get; set; }
        public string ArrestBookingNo1 { get; set; }
        public int? ArrestSentenceGroup { get; set; }
        public string Abbr { get; set; }
        public SentenceGapFound SentenceGapFound { get; set; }
        public DateTime? ArrestSentenceStartDate { get; set; }
        public bool ArrestSentenceConsecutiveFlag { get; set; }
        public int? ArrestSentenceConsecutiveTo { get; set; }
        public DateTime? ArrestSentenceUseStartDate { get; set; }
        public DateTime? ArrestSentenceUseStartDateStr { get; set; }
        public int? ArrestSentenceDays { get; set; }
        public string MethodName { get; set; }
        public int? ArrestSentenceDaysToServe { get; set; }
        public int? ArrestSentenceActualDaysToServe { get; set; }
        public DateTime? ArrestSentenceReleaseDate { get; set; }
        public DateTime? ArrestSentenceReleaseDateStr { get; set; }
        public bool ArrestSentenceManual { get; set; }
        public bool ArrestSentenceForthwith { get; set; }
        public int? ArrestSentenceFlatTime { get; set; }
        public int? ArrestSentenceDaysAmount { get; set; }
        public DateTime? ArrestSentenceExpirationDate { get; set; }
        public DateTime? ArrestSentenceExpirationDateStr { get; set; }
        public int? WeekEnder { get; set; }
        public DateTime? ArrestDate { get; set; }
        public bool ArrestSentenceAmended { get; set; }
        public bool ArrestSentencePenalInstitution { get; set; }
        public bool ArrestSentenceOptionsRec { get; set; }
        public bool ArrestSentenceAltSentNotAllowed { get; set; }
        public bool ArrestSentenceNoEarlyRelease { get; set; }
        public bool ArrestSentenceNoLocalParole { get; set; }
        public bool ArrestSentenceNoDayForDay { get; set; }
        public bool ArrestSentenceWeekender { get; set; }
        public List<SentenceAdditionalFlagsVm> AdditionalFlagsList { get; set; }
        public DateTime? ArrestSentenceDateInfo { get; set; }
        public string ArrestSentenceType { get; set; }
        public List<KeyValuePair<int, string>> ArrestSentenceTypeList { get; set; }
        public string ArrestSentenceFindings { get; set; }
        public List<KeyValuePair<int, string>> ArrestSentenceFindingsList { get; set; }
        public int? ArrestSentenceJudgeId { get; set; }
        public string ArrestSentenceJudgeName { get; set; }
        public List<PersonnelVm> ArrestSentenceJudgeList { get; set; }
        public string ArrestSentenceDaysInterval { get; set; }
        public List<KeyValuePair<int, string>> ArrestSentenceDaysIntervalList { get; set; }
        public int? ArrestSentenceFineDays { get; set; }
        public int? ArrestSentenceDaysStayed { get; set; }
        public int? ArrestTimeServedDays { get; set; }
        public int? ArrestSentenceMethodId { get; set; }
        public List<KeyValuePair<int, string>> ArrestSentenceMethodList { get; set; }
        public int? DefaultSentenceMethodId { get; set; }
        public int? ArrestSentenceHours { get; set; }
        public bool ArrestSentenceDayForDayAllowedOverride { get; set; }
        public bool ArrestSentenceGtDaysOverride { get; set; }
        public bool ArrestSentenceWtDaysOverride { get; set; }
        public int? ArrestSentenceErcDays { get; set; }
        public bool ArrestSentenceDayForDayDaysOverride { get; set; }
        public int? ArrestSentenceDayForDayAllowed { get; set; }
        public int? ArrestSentenceGtDays { get; set; }
        public int? ArrestSentenceWtDays { get; set; }
        public int? ArrestSentenceGwGtAdjust { get; set; }
        public int? ArrestSentenceDayForDayDays { get; set; }
        public int? ArrestSentenceDisciplinaryDaysSum { get; set; }
        public bool ArrestSentenceIndefiniteHold { get; set; }
        public SentenceFineDaysVm SentenceFineDays { get; set; }
        public SentenceMethodVm SentenceMethod { get; set; }
        public SentenceChargeVm SentenceCharge { get; set; }
        public int FacilityId { get; set; }
        public bool AllowWeekEnderSentence { get; set; }
        public bool NoDayForDayVisible { get; set; }
        public DateTime? ArrestClearScheduleDate { get; set; }
        public int InmateId { get; set; }
        public SentenceAltSentDetailsVm SentenceAltSentDetails { get; set; }
        public bool IsAltSentFacility { get; set; }
        public string HistoryList { get; set; }
        public int? IncarcerationId { get; set; }
        public bool SameDayUseStart { get; set; }
        public List<LookupVm> ListLookup { get; set; }
        public bool NoBailSiteOptionValue { get; set; }
        public int? IsNoBail { get; set; }
        public int ArrestSupSeqNumber { get; set; }
        public int ConsecutiveArrestSupSeqNumber { get; set; }
        public string ConsecutiveArrestCourtDocket { get; set; }
        public string ArrestSentenceDaysStayedInterval { get; set; }
        public int? ArrestSentenceDaysStayedAmount { get; set; }
        public int? ConsecutiveArrestSentenceCode { get; set; }
        public DateTime? ConsecutiveArrestSentenceReleaseDate { get; set; }
        public bool ConsecutiveArrestSentenceIndefiniteHold { get; set; }
        public bool IsCalculated { get; set; }
        public int ArrestSentenceAttendanceId { get; set; }
        public bool ArrestSentenceErcFlag { get; set; }
        public int ArrestSentenceStopWT { get; set; }
    }

    public class SentenceAltSentDetailsVm
    {
        public int? AltSentId { get; set; }
        public int? AltSentProgramId { get; set; }
        public int? AltSentProgramSentCode { get; set; }
    }

    public class ChargeSentenceVm : SentenceDetailsVm
    {
        public int CrimeId { get; set; }
        public int? CrimeNumber { get; set; }
        public int? CrimeCount { get; set; }
        public string Qualifier { get; set; }
        public DateTime? CreateDate { get; set; }
        public int? CrimeLookupId { get; set; }
        public string CrimeCodeType { get; set; }
        public string CrimeSection { get; set; }
        public string CrimeDescription { get; set; }
        public string CrimeStatute { get; set; }
        public string CrimeStatus { get; set; }

        public int? CrimeStatusId { get; set; }
        //public bool IsChecked { get; set; }
    }

    public class SentenceFineDaysVm
    {
        public decimal? ArrestSentenceFinePaid { get; set; }
        public decimal? ArrestSentenceFineToServe { get; set; }
        public string ArrestSentenceFineType { get; set; }
        public decimal? ArrestSentenceFineAmount { get; set; }
        public decimal? ArrestSentenceFinePerDay { get; set; }
        public List<LookupVm> ArrestSentenceFineTypeList { get; set; }
    }

    public class SentenceCreditServedVm
    {
        public SentenceCreditServed CurrentDetails { get; set; }
        public List<SentenceCreditServed> SameDocketDetails { get; set; }
        public List<SentenceCreditServed> OtherDocketDetails { get; set; }
    }

    public class SentenceCreditServed
    {
        public DateTime? BookingDate { get; set; }
        public DateTime? ReleaseDate { get; set; }
        public string ArrestBookingNo { get; set; }
        public string ArrestType { get; set; }
        public string ArrestCourtDocket { get; set; }
        public string ArrestBookingStatus { get; set; }
        public DateTime? ArrestConvictionDate { get; set; }
        public string ArrestCourtJurisdictionAbbr { get; set; }
        public bool IsDayDifference { get; set; }
        public int OverlapDays { get; set; }
        public int DaysDifference { get; set; }
    }

    public class OverallSentenceVm
    {
        public int? InmateId { get; set; }
        public int IncarcerationId { get; set; }
        public DateTime? OverallSentStartDate { get; set; }
        public DateTime? OverallFinalReleaseDate { get; set; }
        public int? ActualDaysToServe { get; set; }
        //public PersonInfoVm PersonInfoDetails { get; set; }
        public bool SentenceByCharge { get; set; }
        //public List<LookupVm> ListLookup { get; set; }
        public bool Manual { get; set; }
        public int? Erc { get; set; }
        public bool ErcClear { get; set; }
        public SentenceAltSentProgramVm AltSentProgramList { get; set; }
        public OercDetailsVm OercDetails { get; set; }
        public int? TotalSentDays { get; set; }
        public bool BookCompleteFlag { get; set; }
        public bool TransportFlag { get; set; }
        public int? SavedBy { get; set; }
        public DateTime? SavedDate { get; set; }
        public string SavedPersonFirstName { get; set; }
        public string SavedPersonLastName { get; set; }
        public string SavedPersonMiddleName { get; set; }
        public string OfficerBadgeNumber { get; set; }
        public bool OverAllErcFlag { get; set; }
        public bool OverAllErcClearFlag { get; set; }
        public DateTime? ArrestDate { get; set; }
    }

    public class SentenceAltSentProgramVm
    {
        public int? IncarcerationId { get; set; }
        public int AltSentId { get; set; }
        public int AltSentProgramId { get; set; }
        public DateTime? AltSentStart { get; set; }
        public DateTime? AltSentThru { get; set; }
        public string AltSentProgramAbbr { get; set; }
        public string FacilityAbbr { get; set; }
        public int? Adts { get; set; }
        public int? DaysAttend { get; set; }
    }

    public class OercDetailsVm
    {
        public bool Visible { get; set; }
        public int DaysRange { get; set; }
        public int Credit { get; set; }
        public int UseMaxDts { get; set; }
        public int UseMaxD { get; set; }
    }

    public class SentenceAdditionalFlagsVm
    {
        public int ArrestSentFlagId { get; set; }
        public int ArrestId { get; set; }
        public int LookupIndex { get; set; }
        public string LookupDescription { get; set; }
        public bool Selected { get; set; }
        public bool DeleteFlag { get; set; }
    }

    public class SentenceMethodVm
    {
        public int ArrestSentenceMethodId { get; set; }
        public string MethodName { get; set; }
        public string MethodDescription { get; set; }
        public int? GtFixed { get; set; }
        public int? GtDFactor { get; set; }
        public int? GtDtsFactor { get; set; }
        public string GtSql { get; set; }
        public double? GtDtsPercent { get; set; }
        public double? GtDPercent { get; set; }
        public bool GtAllowOverride { get; set; }
        public int? GtPostMultiply { get; set; }
        public int? GtTable { get; set; }
        public string GtTableDays { get; set; }
        public List<TabularLookupVm> GtTabularLookup { get; set; }
        public int? DdaFixed { get; set; }
        public int? DdaDFactor { get; set; }
        public int? DdaDtsFactor { get; set; }
        public string DdaSql { get; set; }
        public double? DdaDtsPercent { get; set; }
        public double? DdaDPercent { get; set; }
        public bool DdaAllowOverride { get; set; }
        public bool DdaSubtractGt { get; set; }
        public int? WtFixed { get; set; }
        public int? WtDFactor { get; set; }
        public int? WtDtsFactor { get; set; }
        public string WtSql { get; set; }
        public double? WtDtsPercent { get; set; }
        public double? WtDPercent { get; set; }
        public bool WtAllowOverride { get; set; }
        public int? WtPostMultiply { get; set; }
        public int? WtTable { get; set; }
        public string WtTableDays { get; set; }
        public List<TabularLookupVm> WtTabularLookup { get; set; }
        public int? DdFixed { get; set; }
        public int? DdDFactor { get; set; }
        public int? DdDtsFactor { get; set; }
        public string DdSql { get; set; }
        public double? DdDtsPercent { get; set; }
        public double? DdDPercent { get; set; }
        public bool DdAllowOverride { get; set; }
        public bool InactiveFlag { get; set; }
        public bool ErcTable { get; set; }
        public string ErcTableDays { get; set; }
        public List<TabularLookupVm> ErcTabularLookup { get; set; }
        public string SiteOptionValue { get; set; }
        public string NoBailSiteOptionValue { get; set; }
    }

    public class TabularLookupVm
    {
        public int ArrestSentenceMethodId { get; set; }
        public int? CreditDays { get; set; }
        public int? DaysFrom { get; set; }
        public int? DaysThru { get; set; }
    }

    public class DisciplinaryDays
    {
        public string IncidentNumber { get; set; }
        public string IncidentType { get; set; }
        public DateTime? IncidentDate { get; set; }
        public int? DiscDays { get; set; }
        public PersonVm InmateDetail { get; set; }
        public int DiscInmateId { get; set; }
        public int DiscIncidentId { get; set; }
        public int RemoveFlag { get; set; }
        public string InmateNumber { get; set; }
        public string IncInvolvedParty { get; set; }
        public PersonnelVm RemovedBy { get; set; }
        public DateTime? RemoveDate { get; set; }
        public DateTime? DateIn { get; set; }
        public DateTime? CreateDate { get; set; }
        public int InmateId { get; set; }
        public int? DisciplinaryType { get; set; }
    }
    public class DiscDaysVm
    {
        public int ApplyListCount { get; set; }
        public int RecalcListCount { get; set; }
    }

    public class SentenceGapFound
    {
        public bool ProcessFlag { get; set; }
        public bool GapFoundFlag { get; set; }
        public int GapFoundDays { get; set; }
        public DateTime? NewUseStartDate { get; set; }
        public DateTime? NewUseExpirationDate { get; set; }
        public DateTime? NewUseReleaseClearDate { get; set; }
    }

    public class SentenceChargeVm
    {
        public int CrimeId { get; set; }
        public int? CrimeNumber { get; set; }
        public int? CrimeCount { get; set; }
        public string Qualifier { get; set; }
        public DateTime? CreateDate { get; set; }
        public int? CrimeLookupId { get; set; }
        public string CrimeCodeType { get; set; }
        public string CrimeSection { get; set; }
        public string CrimeDescription { get; set; }
        public string CrimeStatute { get; set; }
        public string CrimeStatus { get; set; }
        public int? CrimeStatusId { get; set; }
        public List<KeyValuePair<int, string>> CrimeStatusList { get; set; }
    }

    //public class SentenceDropDownVm
    //{
    //    public List<KeyValuePair<int, string>> SentenceType { get; set; }
    //    public List<KeyValuePair<int, string>> SentenceFind { get; set; }
    //    public List<KeyValuePair<int, string>> SentenceDuration { get; set; }
    //    public List<KeyValuePair<int, string>> SentenceMethod { get; set; }
    //    public List<PersonnelVm> JudgeDetails { get; set; }
    //}

    //public class SentenceFieldsVm
    //{
    //    public int ArrestSentenceSettingId { get; set; }
    //    public string FieldName { get; set; }
    //    public string DisplayOverride { get; set; }
    //    public bool DisableField { get; set; }
    //    public bool InvisibleField { get; set; }
    //    public bool RequiredBeforeSave { get; set; }
    //    public bool RequiredBeforeCalculation { get; set; }
    //}

    //public class SentenceMinMaxDate
    //{
    //    public DateTime? ArrestSentenceStartDate { get; set; }
    //    public DateTime? ArrestSentenceReleaseDate { get; set; }
    //}

    //public enum ConsecutiveFlag
    //{
    //    Concurrent = 0,
    //    Consecutive = 1
    //}

    //public enum HoldType
    //{
    //    ClearHold = 0,
    //    ClearHoldDate = 1,
    //    IndefiniteHold = 2
    //}

    public class ArrestSentenceSettingVm
    {
        public int ArrestSentenceSettingId { get; set; }
        public string FieldTable { get; set; }
        public string FieldName { get; set; }
        public int? FieldOrder { get; set; }
        public string FieldDisplay { get; set; }
        public string FieldDescription { get; set; }
        public int? FieldSettingMethodFlag { get; set; }
        public bool FieldCalcInputRequired { get; set; }
        public int? FieldCalcFlag { get; set; }
        public int? FieldEntryFlag { get; set; }
        public int? FieldAllowDefault { get; set; }
        public string DisplayOverride { get; set; }
        public bool DisableFlag { get; set; }
        public bool InvisibleFlag { get; set; }
        public bool RequiredForCalc { get; set; }
        public bool RequiredForSave { get; set; }
        public int? DefaultValue { get; set; }
        public bool RequireForCalc { get; set; }
        public string FieldLabel { get; set; }
    }

    public class SentencePdfViewerVm
    {
        public List<BookingStatusVm> BookingStatus { get; set; }
        public List<SentenceDetailsVm> SentenceList { get; set; }
        public List<ChargeSentenceVm> ChargeSentence { get; set; }
        public OverallSentenceVm OverallSentence { get; set; }
        public SentencePdfDetailsVm SentencePdfDetails { get; set; }
        public List<ArrestSentenceSettingVm> ArrestSentenceSetting { get; set; }
        public JObject CustomLabel { get; set; }
    }

    //public class SentencePdfDetailsVm
    //{
    //    public InmatePdfHeader InmatePdfHeaderDetails { get; set; }
    //    public PersonVm PersonDetails { get; set; }
    //}

    public class SentencePdfDetailsVm
    {
        public string SummaryHeader { get; set; }
        public string AgencyName { get; set; }
        public string OfficerName { get; set; }
        public string PersonnelNumber { get; set; }
        public string InmateNumber { get; set; }
        public DateTime StampDate { get; set; }
        public PersonVm PersonDetails { get; set; }
        public bool DisplayOverall { get; set; }
    }

    public enum SentenceSummaryType
    {
        [Display(Name = "Arrest Only")] ARRESTONLY,
        [Display(Name = "Active")] ACTIVE,
        [Display(Name = "All")] ALL
    }

    public class ChargeReplicateSentenceDetails
    {
        public SentenceDetailsVm SentenceDetails { get; set; }
        public List<int> CrimeIds { get; set; }
    }
}