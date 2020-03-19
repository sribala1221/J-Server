
using System;
using System.Collections.Generic;

namespace ServerAPI.ViewModels
{
    public class MailRecordVm
    {
        public int MailRecordid { get; set; }
        public int FacilityId { get; set; }
        public DateTime CreateDate { get; set; }
        public int CreateBy { get; set; }
        public DateTime? UpdateDate { get; set; }
        public int? UpdateBy { get; set; }
        public bool DeleteFlag { get; set; }
        public int? DeleteReason { get; set; }
        public DateTime? DeleteDate { get; set; }
        public int? DeleteBy { get; set; }
        public bool IncomingFlag { get; set; }
        public bool OutgoingFlag { get; set; }
        public int MailDestination { get; set; }
        public int? InmateId { get; set; }
        public int? HousingUnitId { get; set; }
        public int? PersonnelId { get; set; }
        public string DestinationOther { get; set; }
        public string SenderOther { get; set; }
        public int MailType { get; set; }
        public int MailSenderType { get; set; }
        public int? MailSenderid { get; set; }
        public int? MailVendorid { get; set; }
        public int? MailRecipientid { get; set; }
        public bool RefusalFlag { get; set; }
        public int? RefusalReason { get; set; }
        public DateTime? RefusalDate { get; set; }
        public int? RefusalBy { get; set; }
        public bool RefusalPrintFlag { get; set; }
        public bool HoldMailFlag { get; set; }
        public int MailBinid { get; set; }
        public string MailNote { get; set; }
        public bool MailSearchFlag { get; set; }
        public DateTime? MailSearchDate { get; set; }
        public int? MailSearchBy { get; set; }
        public bool MailSearchNotAllowed { get; set; }
        public string MailSearchNote { get; set; }
        public string MailFlagString { get; set; }
        public bool MailMoneyFlag { get; set; }
        public decimal? MailMoneyAmount { get; set; }
        public int? MailMoneyType { get; set; }
        public string MailMoneyNumber { get; set; }
        public bool MailMoneyPrintFlag { get; set; }
        public bool MailDeliveredFlag { get; set; }
        public DateTime? MailDeliveredDate { get; set; }
        public int? MailDeliveredBy { get; set; }
        public bool MailCoverFlag { get; set; }
        public MailSenderVm MailSender { get; set; }
        public MailVendorVm MailVendor { get; set; }

        public MailRecipientVm MailRecipient { get; set; }
        public string DestinationName { get; set; }
        public string MailTypeName { get; set; }
        public string SenderTypeName { get; set; }
        public string RefusalName { get; set; }
        public PersonInfoVm PersonInfo { get; set; }
        public PersonnelVm PersonnelVm { get; set; }
        public string MailBinName { get; set; }

        public int[] SearchListId { get; set; }
        public string MailMoneyAmountTypeName { get; set; }
        // public inmateVm Inmatevm { get;  set; }
    }
    public class InmateMailVm
    {
        public List<MailBinVm> MailBinList { get; set; }
        public List<MailVendorVm> MailVendorList { get; set; }
        public List<MailSenderVm> MailSenderList { get; set; }
        public List<MailRecordVm> MailRecordList { get; set; }
        public List<MailRecipientVm> MailRecipientList { get; set; }
        public List<MailVendorSubscribeVm> MailVendorSubscribeList { get; set; }
    }

    public class InmateMailHousingDetailVm : HousingDetail
    {
        public int? LocationId { get; set; }
        public int? MailBinid { get; set; }
        public bool MailPrivilegeFlag { get; set; }
        public bool MailCoverFlag { get; set; }
        public PersonInfo PersonInfo { get; set; }
    }


}