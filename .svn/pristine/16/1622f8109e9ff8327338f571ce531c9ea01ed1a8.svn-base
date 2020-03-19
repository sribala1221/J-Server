using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling MVC for empty projects, visit http://go.microsoft.com/fwlink/?LinkID=397860

namespace ServerAPI.ViewModels
{
    public class PersonCitizenshipVm
    {
        public int PersonCitizenshipId { get; set; }
        public int PersonId { get; set; }
        public string CitizenshipCountry { get; set; }
        public string CitizenshipStatus { get; set; }
        public string CitizenshipNote { get; set; }
        public DateTime CreateDate { get; set; }
        public int CreateBy { get; set; }
        public string CreateByLastName { get; set; }
        public string CreateByFirstName { get; set; }
        public DateTime? UpdateDate { get; set; }
        public int UpdateBy { get; set; }
        public string UpdateByLastName { get; set; }
        public string UpdateByFirstName { get; set; }
        public DateTime? DeleteDate { get; set; }
        public int? DeleteBy { get; set; }
        public bool DeleteFlag { get; set; }
        public PersonCitizenshipHistoryVm[] PersonCitizenshipHistory { get; set; }
        public PersonCitizenshipStatus PersonCitizenshipStatus { get; set; }
        public string PassportNumber { get; set; }
        public bool NotificationAcknowledgement { get; set; }
        public bool NotificationAutomateFlag { get; set; }
        public int? PrimaryLanguage { get; set; }
        public int InmateId { get; set; }
        public int FacilityId { get; set; }
        public bool IsFromUsCitizen { get; set; }
        public string languageDescription { get; set; }
    }

    public class PersonCitizenshipHistoryVm
    {
        public int PersonCitizenshipHistoryId { get; set; }
        public int PersonCitizenshipId { get; set; }
        public int PersonId { get; set; }
        public string CitizenshipCountry { get; set; }
        public string CitizenshipStatus { get; set; }
        public string CitizenshipNote { get; set; }
        public DateTime CreateDate { get; set; }
        public string CreateBy { get; set; }
        public DateTime UpdateDate { get; set; }
        public string UpdateBy { get; set; }
        public DateTime DeleteDate { get; set; }
        public string DeleteBy { get; set; }
        public bool DeleteFlag { get; set; }
    }

    public class CitizenshipCountryDetails
    {
        public string CitizenshipCountry { get; set; }
        public bool NotificationRequired { get; set; }
        public bool NotificationOptional { get; set; }
        public string ConsulateName { get; set; }
        public string ConsulateAddress { get; set; }
        public string ConsulateCity { get; set; }
        public string ConsulateState { get; set; }
        public string ConsulateZip { get; set; }
        public string ConsulatePhone { get; set; }
        public string ConsulateFax { get; set; }
        public string ConsulateEmail { get; set; }
        public string ConsulateContact { get; set; }
        public string ConsulateInstructions { get; set; }
        public bool AutomateFlag { get; set; }
        public string AutomateEmail { get; set; }

    }
    public class PersonCitizenshipDetail
    {
        public List<PersonCitizenshipVm> lstPersonCitizenship { get; set; }
        public List<CitizenshipCountryDetails> lstCitizenshipCountryDetails { get; set; }

    }

    public class IncarcerationFormDetails
    {
        public List<IncarcerationForms> lstIncarcerationForms { get; set; }
        public FormTemplateCount templateCount { get; set; }
    }

    public class IncarcerationForms
    {
        public int FormRecordId { get; set; }
        public string DisplayName { get; set; }
        public string FormNotes { get; set; }
        public DateTime? ReleaseOut { get; set; }
        public DateTime? DateIn { get; set; }
        public int DeleteFlag { get; set; }
        public string XmlData { get; set; }
        public string FormCategoryFolderName { get; set; }
        public string HtmlFileName { get; set; }
        public int FormTemplatesId { get; set; }
        public int? FormInterfaceFlag { get; set; }
        public int? FormInterfaceSent { get; set; }
        public int? FormInterfaceByPassed { get; set; }
        public PersonnelVm CreatedBy { get; set; }
        public PersonnelVm UpdatedBy { get; set; }
        //public string CreatedByPersonLastName { get; set; }
        //public string CreatedByPersonnelNumber { get; set; }
        public int CreateBy { get; set; }
        //public int? UpdateBy { get; set; }
        //public string UpdatedByPersonLastName { get; set; }
        //public string UpdatedByPersonnelNumber { get; set; }
        public DateTime? CreateDate { get; set; }
        public DateTime? UpdateDate { get; set; }
        public int? FormCategoryFilterId { get; set; }
        public string FilterName { get; set; }
        public HousingDetail HousingDetail { get; set; }
        public PersonnelVm PersonnelDetails { get; set; }
        public string InmateNumber { get; set; }
        public string NoSignature { get; set; }
        public bool? NoSignatureFlag { get; set; }
        public int? FacilityId { get; set; }
    }

    public class FormTemplateCount
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; }
        public int Count { get; set; }
        public List<int> LstFormTemplate { get; set; }
    }
}
