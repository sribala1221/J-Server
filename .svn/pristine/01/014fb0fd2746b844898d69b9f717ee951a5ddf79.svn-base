using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;

namespace ServerAPI.ViewModels
{
    public class RosterMaster
    {
        public List<LookupVm> LookupCombos { get; set; }
        public List<RosterDetails> RosterDetails { get; set; }
        public List<LocationDetails> LocationDetails { get; set; }
    }

    public class AccountAoInmateVm
    {
        public int InmateId { get; set; }
        public decimal? BalanceInmate { get; set; }
    }

    public class RosterVm
    {
        public int? FacilityId { get; set; }
        public int InmateId { get; set; }
        public string InmateNumber { get; set; }
        public int? InmateCurrentTrackId { get; set; }
        public string InmateCurrentTrack { get; set; }
        public int PersonId { get; set; }
        public string PersonLastName { get; set; }
        public string PersonMiddleName { get; set; }
        public string PersonFirstName { get; set; }
        public string PersonPhoto { get; set; }
        public int? HousingUnitListId { get; set; }
        public string HousingUnitLocation { get; set; }
        public string HousingUnitNumber { get; set; }
        public string HousingUnitBedNumber { get; set; }
        public DateTime? ReleaseOut { get; set; }
        public DateTime? OverallFinalReleaseDate { get; set; }
    }

    public class RosterFilters
    {
        public bool IsPageInitialize { get; set; }
        public int FacilityId { get; set; }
        public int? Gender { get; set; }
        public int? Race { get; set; }
        public string Association { get; set; }
        public string AlertType { get; set; }
        public double? AlertIndex { get; set; }
        public string Classification { get; set; }
        public string Balance { get; set; }
        public bool IllegalAlien { get; set; }
        public string Status { get; set; }
        public int? HousingUnitListId { get; set; }
        public int? HousingGroupId { get; set; }
        public bool ToLoadInmate { get; set; }
        public string Housing { get; set; }
        public string HousingLocation { get; set; }
        public string HousingNumber { get; set; }
        public string Location { get; set; }
        public bool IsAssigned { get; set; }
        public bool IsCheckOut { get; set; }
        public bool IsActual { get; set; }
        public bool IsFilterByName { get; set; }
        public int? AssociationId { get; set; }
    }

    public class RosterDetails
    {
        public int FacilityId { get; set; }
        public string Housing { get; set; }
        public int Assigned { get; set; }
        public int Checkout { get; set; }
        public string HousingLocation { get; set; }
        public string HousingNumber { get; set; }
        public List<RosterInmateInfo> InmateList { get; set; }
    }

    public class LocationDetails
    {
        public string Location { get; set; }
        public int Assigned { get; set; }
        public int Checkout { get; set; }
    }

    public class PrintOverLay : FormTemplate
    {
        public int ArrestId { get; set; }
        public int PersonId { get; set; }
        public string Flag { get; set; }
        public int PersonnelId { get; set; }
        public List<TemplateControl> TemplateControls { get; set; }
        public Dictionary<string, object> TemplateValue { get; set; }
        public string TemplateValueJsonString { get; set; }
    }

    public class FormTemplate
    {
        public string TemplateName { get; set; }
        public bool? RequireBookingSelect { get; set; }
        public string TemplateSql { get; set; }
        public int PersonFormTemplateId { get; set; }
        public string ShortId { get; set; }
        public FileContentResult LabelPdf { get; set; }

    }

    public class TemplateControl
    {
        public string Type { get; set; }
        public string FieldName { get; set; }
        public string Value { get; set; }
        public int? XPos { get; set; }
        public int? YPos { get; set; }
        public int? Width { get; set; }
        public int? Height { get; set; }
        public string Font { get; set; }
        public int? FontSize { get; set; }
        public string ForeColor { get; set; }
        public string BackColor { get; set; }
    }

    public class PrintAllOverLay : FormTemplate
    {
        public List<TemplateControl> TemplateControls { get; set; }
        public List<Dictionary<string, object>> TemplateValues { get; set; }
    }

    public class InmateBookings
    {
        public int? InmateId { get; set; }
        public int ArrestId { get; set; }
        public string ArrestBookingNo { get; set; }
    }

    public class RosterInmateInfo
    {
        public int InmateId { get; set; }
        public string InmateNumber { get; set; }
        public string InmateCurrentTrack { get; set; }
        public int PersonId { get; set; }
        public string PersonFirstName { get; set; }
        public string PersonMiddleName { get; set; }
        public string PersonLastName { get; set; }
        public string PersonPhoto { get; set; }
        public string HousingUnitLocation { get; set; }
        public string HousingUnitNumber { get; set; }
        public string HousingUnitBedNumber { get; set; }
    }
     public class FacilityFormListVm
    {
        public List<FormTemplateCount> FormTemplateCountList { get; set; }
        public List<IncarcerationForms> FacilityForms { get; set; }
    }
}
