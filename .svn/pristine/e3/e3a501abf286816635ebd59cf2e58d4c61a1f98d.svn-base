using System;
using System.Collections.Generic;

namespace ServerAPI.ViewModels
{
    public class GrievanceAppealsVm
    {
        public List<KeyValuePair<string,int>> ElapsedCount { get; set; }
        public List<KeyValuePair<string, int>> CategoryCount { get; set; }
        public List<KeyValuePair<string, int>> TypeCount { get; set; }
        public List<KeyValuePair<string, int>> LocationCount { get; set; }
        public List<GrievanceAppealDetails> GrievanceAppealList { get; set; }
    }

    public class GrievanceCountList
    {
        public string Description { get; set; }
        public DateTime? ReviewDate { get; set; }
        public int? DeleteFlag { get; set; }
    }

    public class GrievanceAppealDetails
    {
        public DateTime? CreateDate { get; set; }
        public string Location { get; set; }
        public int AppealCount { get; set; }
        public int? AppealCategoryLookup { get; set; }
        public string Category { get; set; }
        public string AppealNote { get; set; }
        public PersonInfoVm PersonInfo { get; set; }
        public string GrievanceNumber { get; set; }
        public DateTime? ReviewDate { get; set; }
        public int GrievanceId { get; set; }
        public DateTime? GrievanceCreateDate { get; set; }
        public int? GrievanceType { get; set; }
        public string Type { get; set; }
        public int? AppealBy { get; set; }
        public string AppealByLastName { get; set; }
        public string AppealByBadgeNumber { get; set; }
        public int GrievanceAppealId { get; set; }
        public int? DeleteFlag { get; set; }
        public string GrievanceDisposition { get; set; }
        public DateTime? DateOccured { get; set; }
        public DateTime? AppealDate { get; set; }
        public string GrievanceSummary { get; set; }
        public string InmateResponseNote { get; set; }
        public string ReviewByLastName { get; set; }
        public string ReviewByBadgeNumber { get; set; }
        public string ReviewNote { get; set; }
        public string OverrideReason { get; set; }
        public bool SensitiveMaterial { get; set; }
    }

    public class GrievanceAppealParam
    {
        public int GrievanceId { get; set; }
        public string AppealNote { get; set; }
        public string ReviewNote { get; set; }
        public int AppealCategory { get; set; }
        public string InmateResponse { get; set; }
        public string Disposition { get; set; }
        public string OverrideReason { get; set; }
    }

    public class GrievanceAppealSearch
    {
        public int GrievanceId { get; set; }
        public int InmateId { get; set; }
        public string GrievanceNumber { get; set; }
        public DateTime? DateOccured { get; set; }
        public string Type { get; set; }
        public PersonInfo ReportingInmate { get; set; }
        public int AppealCount { get; set; }
        public int? AppealBy { get; set; }
        public PersonnelVm AppealPersonnel { get; set; }
        public DateTime? AppealDate { get; set; }
    }

    public class GrievanceAppealsReport
    {
        public string ReportName { get; set; }
        public string AgencyName { get; set; }
        public string PhotoFilePath { get; set; }
        public GrievanceVm Grievance { get; set; }
        public GrievanceAppealDetails GrievanceAppeal { get; set; }
        public List<IncarcerationForms> GrievanceForms { get; set; }
        public List<AttachmentDetails> AttachmentDetails { get; set; }
        public string[] GrievanceFlags { get; set; }
        public string[] InmateClassifications { get; set; }
    }
}
