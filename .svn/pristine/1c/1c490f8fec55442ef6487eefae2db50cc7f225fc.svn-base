using System;
using System.Collections.Generic;


namespace ServerAPI.ViewModels
{

    public class Form
    {
        public int FormTemplateId { get; set; }
        public int FormRecordId { get; set; }
        public Dictionary<string, object> Values { get; set; }
        public Signature SignValues { get; set; }
        public bool NoSignatureTrack { get; set; }
        public string MedPrescreenStatusNote { get; set; }
    }

    public class FormRecordVm
    {
        public int FormRecordId { get; set; }
        public string XmlData { get; set; }
        public string HtmlFileName { get; set; }
        public int FormRecordSaveHistoryId { get; set; }
        public DateTime? SaveDate { get; set; }
        public int SaveBy { get; set; }
        public string OfficerBadgeNumber { get; set; }
        public string FormNotes { get; set; }

        //prebook validate confirm related property added
        public int? InmatePrebookId { get; set; }
        public int? MedScreenIncarcerationId { get; set; }
        public bool ValidationFlag {get;set;}
    }

    public class FormSaveData
    {
        public int TemplateId { get; set; }
        public int? PrebookId { get; set; }
        public int? MedPrebookId { get; set; }
        public int? RcRequestId { get; set; }
        public string Payload { get; set; }
        public int FormRecordId { get; set; }
        public int? InmateId { get; set; }
        public int? InmateClassificationId { get; set; }
        public int? IncarcerationId { get; set; }
        public int? BailTransactionId { get; set; }
        public int? DisciplinaryInmateId { get; set; }
        public int? DisciplinaryIncidentId { get; set; }
        public Signature SignValues { get; set; }
        public bool EventHandleFlag { get; set; }
        public int? ArrestId { get; set; }
        public int? SheetArrestId { get; set; }
        public int? PropertyIncarcerationId { get; set; }
        public int? PropReleaseInmateId { get; set; }
        public DateTime? PropReleaseDate { get; set; }
        public string FormNotes { get; set; }
        public bool? NoSignatureFlag { get; set; }
        public string NoSignatureReason { get; set; }
        public int? BooksSheetIncarcerationId { get; set; }
        public int? PersonId { get; set; }
        public int? FacilityId { get; set; }
        public int? InvestigationId { get; set; }
        public int? PREAInmateId { get; set; }
        public bool ValidationFlag {get;set;}        
    }

    public class FormHistory
    {
        public int SaveHistoryId { get; set; }
        public int FormRecordId { get; set; }
        public virtual PersonnelVm Officer { get; set; }
        public string FormNotes { get; set; }
        public DateTime? SaveDate { get; set; }
    }

    public class Signature : FormRecordVm
    {
        public string Label1 { get; set; }
        public string Label2 { get; set; }
        public string Label3 { get; set; }
        public string Label4 { get; set; }
        public string Label5 { get; set; }
        public string Label6 { get; set; }
        public string Label7 { get; set; }

        public string Signature1 { get; set; }
        public string Signature2 { get; set; }
        public string Signature3 { get; set; }
        public string Signature4 { get; set; }
        public string Signature5 { get; set; }
        public string Signature6 { get; set; }
        public string Signature7 { get; set; }
        public bool NoSignatureFlag { get; set; }
        public string NoSignatureReason { get; set; }
        public bool? NoSignatureTrack { get; set; }
    }

    //Used in GetFormData method in FormService.cs -- Line No:352
    public class FormFieldName
    {
        public int PrebookId { get; set; }
        public int MedPrebookId { get; set; }
        public int IncarcerationId { get; set; }
        public int InmateId { get; set; }
        public int DisciplinaryControlId { get; set; }
        public int DisciplinaryInmateId { get; set; }
        public int InmateClassificationId { get; set; }
        public int ArrestId { get; set; }
        public int RequestId { get; set; }
        public int RCRequestId { get; set; }
        public int BailId { get; set; }
        public int PersonId { get; set; }
        public int AltSentId { get; set; }
        public int GrivanceId { get; set; }
        public int ProgramCaseInmateId { get; set; }
        public DateTime? ReleaseDate { get; set; }
        public int DeleteFlag { get; set; }
        public int PersonnelId { get; set; }
        public int? FacilityId { get; set; }
        public int? InvestigationId { get; set; }
        public int? PREAInmateId { get; set; }
    }

    public class MedicalPrescreenVm
    {
        public int? IncarcerationId { get; set; }
        public int? InmateId { get; set; }
        public int? MedScreenArrestId { get; set; }
        public int FormRecordId { get; set; }
        public bool DeleteFlag { get; set; }
        public DateTime? Createdate { get; set; }
        public DateTime? Updatedate { get; set; }
        public int CreatedBy { get; set; }
        public PersonnelVm CreatedByInfo { get; set; }
        public int? UpdatedBy { get; set; }
        public PersonnelVm UpdatedByInfo { get; set; }
        public DateTime? PersonDob { get; set; }
        public int? StatusFlag { get; set; }
        public int? StatusBy { get; set; }
        public DateTime? StatusDate { get; set; }
        public PersonnelVm StatusByInfo { get; set; }
        public bool PrebookDeleteFlag { get; set; }
        public DateTime? FormDate { get; set; }
        public string PersonLastName { get; set; }
        public string PersonFirstName { get; set; }
        public string PersonMiddleName { get; set; }
        public string FormNotes { get; set; }
        public string DisplayName { get; set; }
        public int? MedInmatePreBookId { get; set; }
        public bool TempHoldFlag { get; set; }
        public int FacilityId { get; set; }
        public bool ActiveFlag { get; set; }
        public int? TempHoldId { get; set; }
        public string PrebookNumber { get; set; }
        public PersonInfoVm PersonInfo { get; set; }
        public PersonInfoVm PreBookInfo { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int TopSearch { get; set; }
        public int PersonnelId { get; set; }
    }

    public class FormDetail
    {
        public int? TemplateId { get; set; }
        public FormFieldName FieldName { get; set; }
        public int FormRecordId { get; set; }
        public bool Autofill { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public bool IsAdd { get; set; }
        public string ApiUrl { get; set; }
        public Form FormData { get; set; }
    }

    public enum FormCategoryName
    {
        PrebookForms = 1,
        CourtCommitForms = 2
    }

    public class MedicalForms
    {
        public int FormTemplateId { get; set; }
        public int FormRecordId { get; set; }
        public int InmateId { get; set; }
        public DateTime CreateDate { get; set; }
        public int CreateById { get; set; }
        public string DisplayName { get; set; }
        public string FormNotes { get; set; }
        public bool DeleteFlag { get; set; }
        public int? DeleteById { get; set; }
        public InmateHousing InmateDetails { get; set; }
        public PersonnelVm CreateBy { get; set; }
        public PersonnelVm DeleteBy { get; set; }
    }

    public class MedicalFormVm
    {
        public List<MedicalForms> lstMedicalForms { get; set; }
        public List<GetFormTemplates> lstGetFormTemplates { get; set; }
    }

    public class FormSavedHistoryObj
    {
        public Dictionary<string, object> SavedHistoryObj { get; set; }
        public Signature SavedHistorySignature { get; set; }
    }

    public class MedicalNotes
    {
        public int InmateMedicalNoteId { get; set; }
        public int InmateId { get; set; }
        public int PersonId { get; set; }
        public string Note { get; set; }
        public int? DeleteFlag { get; set; }
        public DateTime? CreateDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public DateTime? DeletedDate { get; set; }
        public string OfficerBadgeNumber { get; set; }
        public string MedicalNoteType { get; set; }
        public string LookupDescription { get; set; }
        public double? LookupIndex { get; set; }
        public string LookupColor { get; set; }
        public int? InmateActive { get; set; }
        public string UpdatedOfficerBadgeNumber { get; set; }
        public string DeletedOfficerBadgeNumber { get; set; }
        public int FacilityId { get; set; }
        public string InmateNumber { get; set; }
        public string CreatedByPersonLastName { get; set; }
        public string CreatedByPersonSuffix { get; set; }
        public string CreatedByPersonFirstName { get; set; }
        public string CreatedByPersonMiddleName { get; set; }
        public string PersonLastName { get; set; }
        public string PersonSuffix { get; set; }
        public string PersonFirstName { get; set; }
        public string PersonMiddleName { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int PersonnelId { get; set; }
        public string NoteType { get; set; }
        public int? DeletedOnly { get; set; }
        public int? ActiveOnly { get; set; }
    }
    public class MedicalNotesVm
    {
        public List<MedicalNotes> lstMedicalNotes { get; set; }
    }
}