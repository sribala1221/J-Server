using System;
using System.Collections.Generic;

namespace ServerAPI.ViewModels
{
    public class MedicalFormsParam
    {
        public int? InmateId { get; set; }
        public int? PersonnelId { get; set; }
        public int? FacilityId { get; set; }
        public DateTime? DateFrom { get; set; }
        public DateTime? DateTo { get; set; }
        public int FormRecordId { get; set; }
    }

    public class MedicalFormsVm
    {
        public int FormRecordId { get; set; }
        public int FormTemplatesId { get; set; }
        public DateTime? CreateDate { get; set; }
        public string FormType { get; set; }
        public string FormNotes { get; set; }
        public PersonInfoVm PersonInfo { get; set; }
        public PersonnelVm PersonnelInfo { get; set; }
        public int DeleteFlag { get; set; }
        public int IncarcerationId { get; set; }
        public int InmateId { get; set; }
        public string XmlData { get; set; }
    }
}
