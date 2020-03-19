using System;


namespace ServerAPI.ViewModels
{
    public class MailRecipientVm
{
        public int MailRecipientid { get; set; }
        public DateTime CreateDate { get; set; }
        public int CreateBy { get; set; }
        public DateTime? UpdateDate { get; set; }
        public int? UpdateBy { get; set; }
        public DateTime? DeleteDate { get; set; }
        public int? DeleteBy { get; set; }
        public bool DeleteFlag { get; set; }
        public string RecipientName { get; set; }
        public string RecipientAddress { get; set; }
        public string RecipientCity { get; set; }
        public string RecipientState { get; set; }
        public string RecipientZip { get; set; }
        public string RecipientAlertNote { get; set; }
        public bool NotAllowedFlag { get; set; }
}

}