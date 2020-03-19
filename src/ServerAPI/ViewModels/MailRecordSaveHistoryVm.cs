using System;
namespace  ServerAPI.ViewModels
{
    public class MailRecordSaveHistoryVm{
       public int MailRecordSaveHistoryid { get; set; }
        public int MailRecordid { get; set; }
        public DateTime SaveDate { get; set; }
        public int SaveBy { get; set; }
        public string JsonSaveData { get; set; }
}
}
