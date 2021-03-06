﻿using System;
using System.Collections.Generic;

namespace ServerAPI.ViewModels
{
    public class ClassFileInputs
    {
        public int InmateId { get; set; }
        public bool DeleteFlag { get; set; }
        public bool Refresh { get; set; }
        public LogParameters LogParameters { set; get; }
    }

    public class ClassFileOutputs
    {
        public ClassFileReview InmateReviewDetails { get; set; }
        public ClassifyCount GridCounts { set; get; }
        // TODO Grid Values? from V1
        public List<ClassLog> GridValues { get; set; }
        public LogParameters GetClassifySettings { set; get; }
        public List<SiteOptionProp> SiteOption { get; set; }
    }

    public class SiteOptionProp
    {
        public string SiteOptionVariable { get; set; }
        public string SiteOptionValue { get; set; }
    }

    public class ClassFileReview
    {
        public DateTime LastReviewDate { get; set; }
        public string LookupCategory { get; set; }
        public PersonDetailVM Name { get; set; }
    }

    public class PersonDetailVM
    {
        public int PersonnelId { get; set; }
        public int? PersonId { get; set; }
        public int InmateId { get; set; }
        public string LastName { get; set; }
        public string FirstName { get; set; }
        public string MiddleName { get; set; }
        public string Suffix { get; set; }
        public string Number { get; set; }
        public string OfficerBadgeNumber { get; set; }
        public DateTime? Dob { get; set; }
    }

    public class ClassFileInmateDetail : InmateDetail
    {
        public int FacilityId { get; set; }
        public int HousingUnitId { get; set; }
        public int LastReviewBy { get; set; }
        public DateTime LastReviewDate { get; set; }
        public int InmateClassificationId { get; set; }
        public string InmateClassificationReason { get; set; }
    }

    public class DeleteParams
    {
        public string Type { get; set; }
        public int InmateId { get; set; }
        public int Id { get; set; }
        public int DeleteFlag { get; set; }
    }

    public class ClassifyAlerts: PersonInfoVm
    {
        public string Location { get; set; }
        public int AlertId { get; set; }
        public string AlertMessage { get; set; }
        public DateTime? ClassDate { get; set; }
        public PersonnelVm Personnel { get; set; }
    }

    public class ClassifyAlertsCount
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public int Count { get; set; }
        public List<ClassifyAlertsCount> ClassifyCount { get; set; }
    }

    public class ClassifyAlertsVm
    {
        public List<ClassifyAlertsCount> ClassifyAlertsCount { get; set; }
        public List<ClassifyAlerts> ClassifyAlerts { get; set; }
    }
}





