﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ServerAPI.ViewModels
{
    public class PersonInfoVm
    {
        public int? InmateId { get; set; }
        public int PersonId { get; set; }
        public int AkaId { get; set; }
        public string PersonFirstName { get; set; }
        public string PersonLastName { get; set; }
        public string PersonMiddleName { get; set; }
        public string PersonSuffix { get; set; }
        public string InmateNumber { get; set; }
        public DateTime? PersonDob { get; set; }
        public int? PersonSexLast { get; set; }
        public int? InmateClassificationId { get; set; }
        public int? GrvInmateId { get; set; }
        public bool InmateActive { get; set; }
        public int InmatePersonId { get; set; }
        public string PhotoPath { get; set; }
        public int FacilityId {get;set;}
        public int HousingUnitId {get;set;}
        public string Facility { get; set; }
        public string HousingLocation { get; set; }
        public string HousingUnitNumber { get; set; }
        public string HousingUnitBed { get; set; }
        public string Classification { get; set; }
        public string Sex { get; set; }
      
    }

    public class FilePersonVm {
        public string PersonFirstName { get; set; }
        public string PersonLastName { get; set; }
        public string PersonMiddleName { get; set; }
        public string PersonSuffix { get; set; }
        public DateTime? PersonDob { get; set; }
        public bool InmateActive { get; set; }
        public int? InmateId { get; set; }
        public int PersonId { get; set; }
        public int FacilityId { get; set; }
        public string InmateNumber { get; set; }
        public int InmatePreBookId { get; set; }
        public bool NoKeeper { get; set; }

    }
}
