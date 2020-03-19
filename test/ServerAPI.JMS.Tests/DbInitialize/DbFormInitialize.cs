using System;
using GenerateTables.Models;

// ReSharper disable once CheckNamespace
namespace ServerAPI.Tests
{
    public partial class DbInitialize
    {
        private void FormDetails()
        {
            Db.FormTemplates.AddRange(
                new FormTemplates
                {
                    FormTemplatesId = 10,
                    FormCategoryId = 1,
                    CreatedBy = 11,
                    CreateDate = DateTime.Now,
                    DisplayName = "BOOKING",
                    FormCategoryFilterId = 10
                },
                new FormTemplates
                {
                    FormTemplatesId = 11,
                    FormCategoryId = 11,
                    CreatedBy = 12,
                    CreateDate = DateTime.Now.AddDays(-1),
                    DisplayName = "MEDICAL PRE-SCREENING"
                },
                new FormTemplates
                {
                    FormTemplatesId = 12,
                    FormCategoryId = 10,
                    CreateDate = DateTime.Now.AddDays(-1),
                    CreatedBy = 11,
                    DisplayName = "BOOKING FORMS",
                    FormCategoryFilterId = 10
                },
                new FormTemplates
                {
                    FormTemplatesId = 13,
                    FormCategoryId = 15,
                    CreatedBy = 12,
                    CreateDate = DateTime.Now.AddDays(-1),
                    DisplayName = "ALT SENT FORM"
                },
                new FormTemplates
                {
                    FormTemplatesId = 14,
                    FormCategoryId = 2,
                    CreatedBy = 12,
                    CreateDate = DateTime.Now.AddDays(-1),
                    DisplayName = "COURT COMMIT'S"
                },

                new FormTemplates
                {
                    FormTemplatesId = 15,
                    FormCategoryId = 12,
                    CreatedBy = 12,
                    CreateDate = DateTime.Now.AddDays(-1),
                    DisplayName = "MEDICAL PRE-SCREENING"
                },
                new FormTemplates
                {
                    FormTemplatesId = 16,
                    FormCategoryId = 4,
                    CreatedBy = 12,
                    CreateDate = DateTime.Now.AddDays(-1),
                    DisplayName = "ASD",
                    Inactive = 0
                }

            );
            Db.FormCategory.AddRange(
                new FormCategory
                {
                    FormCategoryId = 1,
                    FormCategoryName = "PREBOOK FORMS",
                    FormCategoryUseFieldName = "Form_Record.Inmate_Prebook_id",
                    FormCategoryFolderName = "PREBOOK"
                },
                new FormCategory
                {
                    FormCategoryId = 2,
                    FormCategoryName = "COURT COMMIT FORMS",
                    FormCategoryUseFieldName = "Form_Record.Inmate_Prebook_id",
                    FormCategoryFolderName = "\\FORMTEMPLATES\\COURTCOMMIT"
                },
                new FormCategory
                {
                    FormCategoryId = 3,
                    FormCategoryName = "MEDICAL SCREENING FORM",
                    FormCategoryUseFieldName = "Form_Record.MedScreen_Incarceration_id",
                    FormCategoryFolderName = "MEDICALSCREEN"
                },
                new FormCategory
                {
                    FormCategoryId = 4,
                    FormCategoryName = "MEDICAL FORMS",
                    FormCategoryUseFieldName = "Form_Record.Inmate_id",
                    FormCategoryFolderName = "MEDICAL"
                },
                new FormCategory
                {
                    FormCategoryId = 5,
                    FormCategoryName = "INCIDENT FORMS",
                    FormCategoryUseFieldName = "Form_Record.Disciplinary_control_id",
                    FormCategoryFolderName = "DISCINCIDENT"
                },
                new FormCategory
                {
                    FormCategoryId = 6,
                    FormCategoryName = "INCIDENT INMATE FORMS",
                    FormCategoryUseFieldName = "Form_Record.Disciplinary_Inmate_ID",
                    FormCategoryFolderName = "DISCINVPARTY"
                },
                new FormCategory
                {
                    FormCategoryId = 7,
                    FormCategoryName = "CLASSIFICATION INTIAL FORM",
                    FormCategoryUseFieldName = "Form_Record.Inmate_classification_id",
                    FormCategoryFolderName = "CLASSIFICATION"
                },
                new FormCategory
                {
                    FormCategoryId = 8,
                    FormCategoryName = "CLASSIFICATION RE-CLASS FORM",
                    FormCategoryUseFieldName = "Form_Record.Inmate_classification_id",
                    FormCategoryFolderName = "CLASSIFICATION"
                },
                new FormCategory
                {
                    FormCategoryId = 9,
                    FormCategoryName = "CLASSIFICATION FORMS",
                    FormCategoryUseFieldName = "Form_Record.Inmate_classification_id",
                    FormCategoryFolderName = "CLASSIFICATION"
                },
                new FormCategory
                {
                    FormCategoryId = 10,
                    FormCategoryName = "BOOKING FORMS",
                    FormCategoryUseFieldName = "Form_Record.Incarceration_id",
                    FormCategoryFolderName = "BOOKING",
                    FormCategoryAllow1Only = 1,
                    FormCategoryAllow1OnlyName = null
                },
                new FormCategory
                {
                    FormCategoryId = 11,
                    FormCategoryName = "CASE FORMS",
                    FormCategoryUseFieldName = "Form_Record.Arrest_id",
                    FormCategoryFolderName = "BOOKING"
                },
                new FormCategory
                {
                    FormCategoryId = 12,
                    FormCategoryName = "MEDICAL PRE-SCREENING FORM",
                    FormCategoryUseFieldName = "Form_Record.Med_Inmate_Prebook_id",
                    FormCategoryFolderName = "PREBOOKMEDICAL"
                },
                new FormCategory
                {
                    FormCategoryId = 13,
                    FormCategoryName = "REQUEST FORMS",
                    FormCategoryUseFieldName = "Form_Record.Request_id",
                    FormCategoryFolderName = "REQUEST"
                },
                new FormCategory
                {
                    FormCategoryId = 14,
                    FormCategoryName = "RECORDS CHECK FORM",
                    FormCategoryUseFieldName = "Form_Record.Records_Check_Request_id",
                    FormCategoryFolderName = "RECORDS"
                },
                new FormCategory
                {
                    FormCategoryId = 15,
                    FormCategoryName = "BAIL RECEIPT FORM",
                    FormCategoryUseFieldName = "Form_Record.Bail_Transaction_Id",
                    FormCategoryFolderName = "BAIL"
                },
                new FormCategory
                {
                    FormCategoryId = 16,
                    FormCategoryName = "ALT SENT FORM",
                    FormCategoryUseFieldName = "Form_Record.AltSent_Id",
                    FormCategoryFolderName = "FORMTEMPLATES BOOKING",
                    FormCategoryAllow1Only = 1,
                    FormCategoryAllow1OnlyName = null
                },
                new FormCategory
                {
                    FormCategoryId = 17,
                    FormCategoryName = "GRIEVANCE FORMS",
                    FormCategoryUseFieldName = "Form_Record.Grievance_Id",
                    FormCategoryFolderName = "GRIEVANCE"
                },
                new FormCategory
                {
                    FormCategoryId = 18,
                    FormCategoryName = "PROGRAM FORMS",
                    FormCategoryUseFieldName = "Form_Record.ProgramCaseInmateId",
                    FormCategoryFolderName = "PROGRAM"
                },
                new FormCategory
                {
                    FormCategoryId = 19,
                    FormCategoryName = "CASE SHEET FORM",
                    FormCategoryUseFieldName = "Form_Record.ArrestId",
                    FormCategoryFolderName = "BOOKING"
                },
                new FormCategory
                {
                    FormCategoryId = 20,
                    FormCategoryName = "PROPERTY INCUSTODY SHEET FORM",
                    FormCategoryUseFieldName = "Form_Record.PropertyIncarcerationId",
                    FormCategoryFolderName = "PROPERTY"
                },
                new FormCategory
                {
                    FormCategoryId = 21,
                    FormCategoryName = "PROPERTY RELEASE SHEET FORMBOOKING SHEET FORM",
                    FormCategoryUseFieldName = "Form_Record.PropReleaseInmateId",
                    FormCategoryFolderName = "PROPERTY"
                },
                new FormCategory
                {
                    FormCategoryId = 22,
                    FormCategoryName = "BOOKING FORMS",
                    FormCategoryUseFieldName = "Form_Record.BooksSheetIncarcerationId",
                    FormCategoryFolderName = "BOOKING SHEET FORM"
                }
            );

            Db.FormRecord.AddRange(
                new FormRecord
                {
                    FormRecordId = 5,
                    ArrestId = 5,
                    XmlData = "<Form_Details>< person_name > ABINAYA</ person_name ></ Form_Details > ",
                    CreateDate = DateTime.Now,
                    ClassificationFormInmateId = 101,
                    PREAInmateId = 100,
                    CreateBy = 11,
                    DeleteFlag = 0,
                    FormTemplatesId = 10,
                    InmatePrebookId = 6,
                    IncarcerationId = 10,
                    InmateClassificationId = 6
                },
                new FormRecord
                {
                    FormRecordId = 6,
                    ArrestId = 5,
                    XmlData = "<person_name_lfm>RUBAN</person_name_lfm>  < person_dob > 10 / 10 / 1971 </ person_dob >",
                    CreateDate = DateTime.Now,
                    ClassificationFormInmateId = 103,
                    InmateId = 101,
                    CreateBy = 12,
                    DeleteFlag = 0,
                    FormTemplatesId = 11,
                    InmatePrebookId = 5,
                    IncarcerationId = 11,
                    InmateClassificationId = 7,
                    DisciplinaryControlId = 6
                },
                
                new FormRecord
                {
                    FormRecordId = 7,
                    ArrestId = 5,
                    XmlData = "<Form_Details> < complete_yes > FALSE </ complete_yes ></ Form_Details > ",
                    CreateDate = DateTime.Now,
                    ClassificationFormInmateId = 103,
                    InmateId = 103,
                    CreateBy = 12,
                    DeleteFlag = 0,
                    FormTemplatesId = 10,
                    InmatePrebookId = 7,
                    IncarcerationId = 11,
                    InmateClassificationId = 5,
                    BailTransactionId = 8,
                    InvestigationId = 10
                },
                new FormRecord
                {
                    FormRecordId = 8,
                    ArrestId = 5,
                    XmlData = "<person_name_lfm>DILIP</person_name_lfm>  < person_dob > 12 / 10 / 1980 </ person_dob >",
                    CreateDate = DateTime.Now,
                    ClassificationFormInmateId = 102,
                    CreateBy = 11,
                    DeleteFlag = 0,
                    FormTemplatesId = 11,
                    InmatePrebookId = 6,
                    IncarcerationId = 15,
                    InmateClassificationId = 11,
                    InvestigationId = 10
                },
                new FormRecord
                {
                    FormRecordId = 9,
                    ArrestId = 7,
                    CreateBy = 12,
                    UpdateBy = 11,
                    CreateDate = DateTime.Now.AddDays(-3),
                    UpdateDate = DateTime.Now,
                    DeleteBy = null,
                    DeleteDate = null,
                    IncarcerationId = 24,
                    FormHousingRoute = 1,
                    FormTemplatesId = 11,
                    GrievanceId = 6,
                    BailTransactionId = 7
                },
                new FormRecord
                {
                    FormRecordId = 10,
                    ArrestId = 5,
                    CreateBy = 12,
                    UpdateBy = 11,
                    CreateDate = DateTime.Now.AddDays(-4),
                    DeleteBy = null,
                    FormTemplatesId = 11,
                    FormHousingClear = 0,
                    BailTransactionId = 5,
                    PREAInmateId = 100,
                    DeleteFlag = 0
                },
                new FormRecord
                {
                    FormRecordId = 11,
                    ArrestId = 6,
                    XmlData = null,
                    CreateDate = DateTime.Now,
                    ClassificationFormInmateId = 102,
                    CreateBy = 12,
                    DeleteFlag = 0,
                    FormTemplatesId = 12,
                    InmatePrebookId = 6,
                    IncarcerationId = 15,
                    InmateClassificationId = 11,
                    FormInterfaceBypassed = null,
                    InmateId = null
                },
                new FormRecord
                {
                    FormRecordId = 12,
                    ArrestId = 5,
                    XmlData = "<person_name_lfm>RUBAN</person_name_lfm>  < person_dob > 10 / 10 / 1971 </ person_dob >",
                    CreateDate = DateTime.Now,
                    ClassificationFormInmateId = 104,
                    CreateBy = 12,
                    DeleteFlag = 0,
                    FormTemplatesId = 11,
                    InmatePrebookId = 5,
                    IncarcerationId = 11,
                    InmateClassificationId = 7,
                    DisciplinaryControlId = 6
                },
                new FormRecord
                {
                    FormRecordId = 13,
                    ArrestId = 5,
                    XmlData = "<person_name_lfm>RUBAN</person_name_lfm>  < person_dob > 10 / 10 / 1971 </ person_dob >",
                    CreateDate = DateTime.Now,
                    ClassificationFormInmateId = 104,
                    CreateBy = 12,
                    DeleteFlag = 0,
                    FormTemplatesId = 10,
                    InmatePrebookId = 5,
                    IncarcerationId = 11,
                    InmateClassificationId = 7,
                    DisciplinaryControlId = 5
                },
                new FormRecord
                {
                    FormRecordId = 14,
                    ArrestId = 5,
                    CreateBy = 12,
                    UpdateBy = 11,
                    CreateDate = DateTime.Now.AddDays(-4),
                    DeleteBy = null,
                    FormTemplatesId = 11,
                    FormHousingClear = 0,
                    BailTransactionId = 5,
                    DeleteFlag = 0
                },
                new FormRecord
                {
                    FormRecordId = 15,
                    ArrestId = 5,
                    XmlData = null,
                    CreateDate = DateTime.Now,
                    ClassificationFormInmateId = 105,
                    CreateBy = 12,
                    DeleteFlag = 0,
                    FormTemplatesId = 11,
                    InmatePrebookId = 5,
                    IncarcerationId = 11,
                    InmateClassificationId = 7,
                    DisciplinaryControlId = 6,
                    UpdateBy = 11
                },
                new FormRecord
                {
                    FormRecordId = 16,
                    ArrestId = 9,
                    XmlData = "<person_name_lfm>RUBAN</person_name_lfm>  < person_dob > 10 / 10 / 1971 </ person_dob >",
                    CreateDate = DateTime.Now,
                    ClassificationFormInmateId = 103,
                    CreateBy = 12,
                    DeleteFlag = 0,
                    FormTemplatesId = 11,
                    InmatePrebookId = 5,
                    IncarcerationId = 11,
                    InmateClassificationId = 7,
                    DisciplinaryControlId = 6
                },
                new FormRecord
                {
                    FormRecordId = 17,
                    ArrestId = 5,
                    CreateBy = 12,
                    UpdateBy = 11,
                    CreateDate = DateTime.Now.AddDays(-4),
                    DeleteBy = null,
                    FormTemplatesId = 16,
                    FormHousingClear = 0,
                    BailTransactionId = 5,
                    PREAInmateId = 100,
                    InmateId = 140,
                    DeleteFlag = 0

                },
                new FormRecord
                {
                    FormRecordId = 18,
                    ArrestId = 6,
                    CreateBy = 12,
                    UpdateBy = 11,
                    CreateDate = DateTime.Now.AddDays(-4),
                    DeleteBy = null,
                    FormTemplatesId = 15,
                    FormHousingClear = 0,
                    BailTransactionId = 5,
                    PREAInmateId = 100,
                    InmateId = null,
                    DeleteFlag = 0

                }
        );

            Db.FormRecordSaveHistory.AddRange(
                    new FormRecordSaveHistory
                    {
                        FormRecordSaveHistoryId = 7,
                        FormRecordId = 5,
                        XmlData = "<Form_Details> <address_state>TN</address_state></ Form_Details >",
                        SaveDate = DateTime.Now.AddDays(-1),
                        SaveBy = 13,
                        FormNotes = "COLLECT ADDRESS DETAILS"
                    },
                    new FormRecordSaveHistory
                    {
                        FormRecordSaveHistoryId = 8,
                        FormRecordId = 5,
                        XmlData =
                            "< person_name_lfm > MANI </ person_name_lfm >< person_AKA > YOGI </ person_AKA >< stampdate > 10 / 10 / 2017 </ stampdate > < person_doc > 784754579 </ person_doc > < person_dob > 03 / 06 / 1992 </ person_dob > ",
                        SaveDate = DateTime.Now,
                        SaveBy = 12
                    }
                );
            Db.FormCategoryFilter.AddRange(
                new FormCategoryFilter
                {
                    FormCategoryFilterId = 10,
                    FilterName = "PERSONAL INVENTORY",
                    FormCategoryId = 10
                },
                new FormCategoryFilter
                {
                    FormCategoryFilterId = 11,
                    FilterName = "TRANSPORT RELEASE",
                    FormCategoryId = 10
                },
                new FormCategoryFilter
                {
                    FormCategoryFilterId = 12,
                    FilterName = "CITIZENSHIP",
                    FormCategoryId = 10
                }
                );
            Db.FormBookmark.AddRange(
                new FormBookmark
                {
                    FormBookmarkId = 10,
                    ArrestId = 7,
                    CreateBy = 11,
                    UpdateBy = 13,
                    CreateDate = DateTime.Now.AddDays(-15),
                    PersonId = 50
                },
                new FormBookmark
                {
                    FormBookmarkId = 11,
                    ArrestId =5,
                    CreateBy = 11,
                    UpdateBy = 12,
                    CreateDate = DateTime.Now.AddDays(-20),
                    PersonId = 1,
                    InmatePrebookId = 12
                }
            );
        }
    }
}
