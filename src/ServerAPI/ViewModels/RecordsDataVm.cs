using System;
using System.Collections.Generic;

namespace ServerAPI.ViewModels
{
    public class RecordsDataVm : PersonInfoVm
    {
        public int? PersonDuplicateId { get; set; }
        public string Dln { get; set; }
        public string Ssn { set; get; }
        public string Cii { set; get; }
        public string Fbi { set; get; }
        public string AlienNo { set; get; }
        public string SiteInmateNo { get; set; }
        public string AfisNumber { get; set; }
        public int Results { get; set; }
        public bool IsGroupByLastName { get; set; }
        public bool IsGroupByFirstName { get; set; }
        public bool IsGroupByMiddleName { get; set; }
        public bool IsGroupByDob { get; set; }
        public bool IsGroupByInmateNumber { get; set; }
        public bool IsGroupBySsn { get; set; }
        public bool IsGroupByDln { get; set; }
        public bool IsGroupByFbi { get; set; }
        public bool IsGroupByCii { get; set; }
        public bool IsGroupByAlienNo { get; set; }
        public bool IsGroupByAfisNo { get; set; }
        public bool IsInclAka { get; set; }
        public bool IsInclPrevMerge { get; set; }
        public int? Juvenile { get; set; }
        public string AkaFirstName { get; set; }
        public string AkaLastName { get; set; }
        public string AkaMiddleName { get; set; }
        public DateTime? AkaDob { get; set; }
        public string AkaDln { get; set; }
        public string AkaSsn { set; get; }
        public string AkaCii { set; get; }
        public string AkaFbi { set; get; }
        public string AkaAlienNo { set; get; }
        public string AkaAfisNumber { get; set; }
        public string AkaInmateNumber { get; set; }
        public List<RecordsDataVm> LstPerson { get; set; }
        public bool IsGroupBy { get; set; }
    }

    public class RecordsDataReferenceVm
    {
        public int DataAoLookUpId { get; set; }
        public string ReferenceName { get; set; }
        public int Count { get; set; }
        public string Date { get; set; }
        public string Number { get; set; }
        public string Description { get; set; }
        public int InmateId { get; set; }
        public int PersonId { get; set; }
        public  int? ReferenceId { get; set; }
        public int? ExcludeInRefMove { get; set; }
    }

    public class RecordsDataTransaction
    {
        public int AccountAoReceivedId { get; set; }
        public int AccountAoDepositoryId { get; set; }
        public int? CashFlag { get; set; }
        public int? InmateId { get; set; }
        public int FundId { get; set; }
        public int AccountAoBankId { get; set; }
        public int? FundInmateOnlyFlag { get; set; }
        public int AccountAoFundId { get; set; }
        public int AccountAoFeeId { get; set; }

    }
    public enum RecordsDataType
    {
        Merge,
        Move,
        Purge,
        Seal
    }

    public enum MoveType
    {
        Incarceration,
        Booking,
        Reference
    }

    public class DoMergeParam
    {
        public RecordsDataVm KeepName { get; set; }
        public List<RecordsDataVm> LstMergeNames { get; set; }
        public List<RecordsDataReferenceVm> DataAoLookup { get; set; }
        public int[] AkaIds { get; set; }
        public List<BookingDataVm> LstBookingData { get; set; }
        public int MergeReasonId { get; set; }
        public string Notes { get; set; }
        public bool ConfirmDelete { get; set; }
        public bool IsVerify { get; set; }
    }
    public class DataHistoryVm : PersonInfoVm
    {
        public int? DataPersonId { get; set; }
        public int? DataInmateId { get; set; }
        public int DataHistoryId { get; set; }
        public string Name { get; set; }
        public string Number { get; set; }
        public int? KeepInmateId { get; set; }
        public int KeepPersonId { get; set; }
        public DateTime? MergeDate { get; set; }
        public DateTime? DataDate { get; set; }
        public int DataBy { get; set; }
        public PersonnelVm DataByPersonnel { get; set; }
        public string DataTitle { get; set; }
        public string DataReason { get; set; }
        public string DataNote { get; set; }
        public string HistoryType { get; set; }
        public int? UndoFlag { get; set; }
        public PersonnelVm UndoByPersonnel { get; set; }
        public DateTime? UndoDate { get; set; }
        public PersonInfoVm KeepPersonInfo { get; set; }
        public PersonInfoVm DataPersonInfo { get; set; }
        public DateTime? MergeDateFrom { get; set; }
        public DateTime? MergeDateTo { get; set; }
        public int Results { get; set; }
        public MoveType MoveType { get; set; }
        public RecordsDataType DataHistoryType { get; set; }
        public int UndoBy { get; set; }
    }

    public class DoMoveParam
    {
        public DataHistoryVm DataHistoryParam { get; set; }
        public List<int> LstDataAoLookupId { get; set; }
        public List<KeyValuePair<int,int>> LstLookupAndRefId { get; set; }
        public int? FromArrestId { get; set; }
        public int? FromIncarcerationId { get; set; }
        public int? ToIncarcerationId { get; set; }
        public int FromInmateId { get; set; }
        public int ToInmateId { get; set; }
        public int FromPersonId { get; set; }
        public int ToPersonId { get; set; }
        public string AppointmetNotes { get; set; }
        public int ScheduleId { get; set; }
        public bool IsVerify { get; set; }
    }

    public class RecordDataSealBookingAndCharges
    {
        public List<RecordDataIncarceration> Incarceration { get; set; }
        public List<RecordDataSealBooking> Booking { get; set; }
        public List<RecordDataSealCharge> Charge { get; set; }
    }

    public class RecordDataIncarceration
    {
        public int IncarcerationId { get; set; }
        public int? InmateId { get; set; }
        public DateTime? ReleaseOut { get; set; }
        public DateTime? DateIn { get; set; }
    }

    public class RecordDataSealBooking
    {
        public int IncarcerationId { get; set; }
        public int? InmateId { get; set; }
        public DateTime? ReleaseOut { get; set; }
        public string BookingNumber { get; set; }
        public string BookingType { get; set; }
        public DateTime? BookingDate { get; set; }
        public int? BookingActive { get; set; }
        public int? ArrestId { get; set; }
        public string ArrestType { get; set; }
        public DateTime? ClearDate { get; set; }
        public string ClearReason { get; set; }
        public string Case { get; set; }
        public string Abbr { get; set; }
        public string Docket { get; set; }
    }

    public class RecordDataSealCharge
    {
        public int? ArrestId { get; set; }
        public int CrimeId { get; set; }
        public int CrimeForceId { get; set; }
        public int WarrantId { get; set; }
        public string WarrantNumber { get; set; }
        public string CrimeSection { get; set; }
        public string CrimeSubSection { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }
        public decimal? BailAmount { get; set; }
        public string BailType { get; set; }
        public string CreateDate { get; set; }
        public string Note { get; set; }
        public string Status { get; set; }
        public string Statute { get; set; }
        public string Qualifier { get; set; }
    }

    public class DataHistoryFieldVm
    {
        public string TableName { get; set; }
        public string PrimaryKey { get; set; }
        public string FieldName { get; set; }
        public int? FromId { get; set; }
        public int? ToId { get; set; }
        public int? PrimaryKeyId { get; set; }
    }

    public class DoSeal
    {
        public string SealType { get; set; }
        public int InmateId { get; set; }
        public int PersonId { get; set; }
        public int PersonnelId { get; set; }
        public string DataTitle { get; set; }
        public string DataReason { get; set; }
        public string DataNote { get; set; }
        public string WarrantId { get; set; }
        public string CrimeId { get; set; }
        public string CrimeForceId { get; set; }
        public string ArrestId { get; set; }
        public string IncarcerationId { get; set; }
        public string IncArrestXrefId { get; set; }
        public int DataHistoryId { get; set; }
    }

}
