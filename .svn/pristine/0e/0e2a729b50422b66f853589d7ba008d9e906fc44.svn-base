using System;
using System.Collections.Generic;

namespace ServerAPI.ViewModels
{
    public class MedicalAlertInmateDataVm
    {
        public int InmateId { get; set; }
        public int FacilityId { get; set; }
        public string InmateNumber { get; set; }
        public int HousingUnitId { get; set; }
        public string HousingUnitLocation { get; set; }
        public string HousingUnitNumber { get; set; }
        public string HousingUnitBedNumber { get; set; }
        public string HousingUnitBedLocation { get; set; }
        public PersonInfoVm PersonInfo { get; set; }
        public string InmateName { get; set; }
        public string Housing { get; set; }
        public string MedAlertDescription { get; set; }
        public int DeleteFlag { get; set; }
        public int PersonId { get; set; }
        public int? MedicalFlagIndex { get; set; }
        public int? MedAlert { get; set; }
        public DateTime? FlagExpire{get;set;}
        public string FlagNote {get;set;}
    }
    public class MedicalAlertInmateVm
    {
        public int FacilityId { get; set; }
        public List<LookupVm> MedAlertinfo { get; set; }
        public string[] BuildingInfo { get; set; }
        public List<KeyValuePair<string, string>> BuildingNumberInfo { get; set; }
        public List<MedicalAlertInmateDataVm> MedicalAlertDetails { get; set; }
        public string Building { get; set; }
        public string BuildingNumber { get; set; }
        public int? MedAlert { get; set; }
        public int PersonId { get; set; }
        public string MedAlertDescription { get; set; }
        public bool RefreshFlag { get; set; }

    }
}
