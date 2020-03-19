using System;

namespace ServerAPI.ViewModels
{
    public class InboundRequestVM
    {
        public string MethodName { get; set; }
        public Object Data { get; set; }
        public string UserName { get; set; }
        public string Pwd { get; set; }
    }

    public class ExportRequestVm
    {
        public string UserName { get; set; }
        public string Pwd { get; set; }
        public int? EventType { get; set; }
        public string EventName { get; set; }
        public string Param1 { get; set; }
        public string Param2 { get; set; }
        public Object Data { get; set; }
        public int? PersonnelId { get; set; }
        public int? WebServiceEventAssignId { get; set; }
    }



    public class InmatePrebookSacramento
    {
        public string AtimsApiKey { get; set; }
        public InmatePrebookVm Prebook { get; set; }
        public string InmateNumber { get; set; }
    }

    public enum ExportTypeEnum
    {
        NetworkDrive = 1,
        Ftp = 2,
        Soap = 3,
        Rest = 4
    }

    public class AtimsOnlineServiceRunExport
    {
        public string UserName { get; set; }
        public string Pwd { get; set; }
        public int ExportId { get; set; }
        public string CallingIp { get; set; }
        public string Parameter1 { get; set; }
        public string Parameter2 { get; set; }
        public string Parameter3 { get; set; }
    }
}
