using System;

public class MailSearchRecordVm
{
    public int FacilityId { get; set; }
    public bool IncomingFlag { get; set; }
    public bool OutgoingFlag { get; set; }
    public bool MailDeliveredFlag { get; set; }
    public DateTime? CreatedStartDate { get; set; }
    public DateTime? CreatedEndDate { get; set; }
    public bool IsDate { get; set; }
    public bool ShowDelete { get; set; }
    public int MailDestination { get; set; }
    public int MailType { get; set; }
    public int MailSenderType { get; set; }
    public int MailBinid { get; set; }
    public int MailIdFrom { get; set; }
    public int MailIdTo { get; set; }
    public string SenderNameContains { get; set; }
    public string ReciepientNameContains { get; set; }
    public int? MailVendorid { get; set; }
    public int? RefusalReason { get; set; }
    public int? InmateId { get; set; }
    public int? HousingUnitId { get; set; }
    public int? PersonnelId { get; set; }
    public string MailFlagString { get; set; }
    public bool MailMoneyFlag { get; set; }
    public int AmountFrom { get; set; }
    public int AmountTo { get; set; }
    public int? MailMoneyType { get; set; }
    public string MailMoneyNumber { get; set; }
    public int CreateBy { get; set; }
    public int? UpdateBy { get; set; }
    public int? MailSearchBy { get; set; }
    public int? RefusalBy { get; set; }
    public int? ByPersonId { get; set; }
    // public bool RefusalFlag { get; set; }
    // public bool HoldMailFlag { get; set; }
    // public bool MailSearchFlag { get; set; }

    public bool RefusedHistoryFlag { get; set; }
    public bool NotSearchedHistoryFlag { get; set; }
    public bool MoneyFlagHistoryFlag { get; set; }
    public bool HoldFlagHistoryFlag { get; set; }
    public bool SearchedHistoryFlag { get; set; }
    public bool DeliveredHistoryFlag { get; set; }

}
