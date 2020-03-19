using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;

namespace ServerAPI.ViewModels
{
	public class NcicVm
	{
		public string SiteOption { get; set; }
		public List<KeyValuePair<int, string>> LstNcicType { get; set; }
		public PersonIdentity PersonDetails { get; set; }
		public PersonCharVm PersonChar { get; set; }
		public List<KeyValuePair<int, string>> LstState { get; set; }
		public List<KeyValuePair<int, string>> LstGender { get; set; }
		public List<IncarcerationDetail> LstIncarcerationAndBooking { get; set; }
        public int InmateActive { get; set; }


    }

	public class AttendenceVm : PersonVm
	{
		public int AttendanceId { get; set; }
		public int AttendanceDayId { get; set; }

		public bool ProgramFlag { get; set; }
		public bool WorkCrewFlag { get; set; }
		public bool AttendFlag { get; set; }
		public double? AttendCredit { get; set; }
		public int? ArrestId { get; set; }
		public bool NoDayDayFlag { get; set; }
		public int? DeleteFlag { get; set; }
		public bool ReCalcComplete { get; set; }
		public string AttendNote { get; set; }
		public DateTime? AttendanceDate { get; set; }
		public DateTime? CreateDate { get; set; }
		public int? AppliedBy { get; set; }
		public PersonnelVm CreatedDetails { get; set; }
		public PersonnelVm AppliedDetails { get; set; }
        public PersonnelVm UndoDetails { get; set; }
        public int? DeleteBy { get; set; }
        public DateTime? DeleteDate { get; set; }
        public DateTime? AppliedDate { get; set; }
        public string CourtCase { get; set; }
        public int? Personneld { get; set; }
        public bool ManualFlag { get; set; }     
        public bool InmateFlag { get; set; }
    }  

    public class AttendeeFlagVm
    {
        public int InmateId { get; set; }
        public DateTime? AttedanceDate { get; set; }
        public int Count { get; set; }  
    }

    public class FormTemplateVm
    {
        public string TemplateName { get; set; }
        public bool RequireBookingSelect { get; set; }
        public string TemplateSql { get; set; }
        public int PersonFormTemplateId { get; set; }
        public bool DeleteFlag { get; set; }
        public bool ShowInLabel { get; set; }
        public bool ShowInPersonnel { get; set; }
        public FormTemplateDetailVm PersonFormTemplateDetail { get; set; }
        public List<KeyValuePair<int, string>> ArrestBookingNo { get; set; }
        public string ShortId { get; set; }
		public string ParamName { get; set; }
		public FileContentResult LabelPdf { get; set; }
    }

    public class FormTemplateDetailVm
    {
        public int PersonFormTemplateId { get; set; }   
        public List<FormTemplateCt1Vm> LstPersonFormTemplateCtrl { get; set; }
        public Dictionary<string, object> TemplateData { get; set; }
        public string PhotoFilePath { get; set; }
        public List<KeyValuePair<int,string>> PersonColorFlag { get; set; }
    }

   
    public class FormTemplateCt1Vm
    {
        public string Type { get; set; }
        public string FieldName { get; set; }
        public string Value { get; set; }
        public int XPos { get; set; }
        public int YPos { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public string Font { get; set; }
        public int FontSize { get; set; }
        public string ForeColor { get; set; }
        public string Backcolor { get; set; }
    }
}
