using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;

namespace ServerAPI.ViewModels
{

    public class IssuedPropertyLookupVm
    {
        public string PropertyName { get; set; }

        public string PropertyDescription { get; set; }

        public int? ExpireUponRelease { get; set; }
        public int IssuedPropertyLookupId { get; set; }

    }

    public class IssuedPropertyLookupFacilityVm
    {
        public int IssuedPropertyLookupFacilityId { get; set; }
        public int FacilityId { get; set; }
        public int IssuedPropertyLookupId { get; set; }

        public string PropertyName { get; set; }

        public string PropertyDescription { get; set; }

        public int? ExpireUponRelease { get; set; }
    }

    public class IssuedPropertyMethod : IssuedPropertyLookupFacilityVm
    {

        public int IssuedCount { get; set; }
        public string IssueNumber { get; set; }
        public string IssueNote { get; set; }
        public int? InmateId { get; set; }
        public DateTime? InactiveDate { get; set; }
        public bool DeleteFlag { get; set; }
        public DateTime? DeleteDate { get; set; }
        public bool ActiveFlag { get; set; }
        public int IssuedPropertyId { get; set; }
        public DateTime? IssueDate { get; set; }
	    public PersonnelVm IssuedDetails { get; set; }
        public PersonnelVm InactiveDetails { get; set; }     
       public string IssuedPropertyHistoryList { get; set; }
        public IssuedType IssuedType { get; set; }
    }


    public enum IssuedType
    {
        Active,
        InActive,

        Delete,

        UndoDelete

    }
}