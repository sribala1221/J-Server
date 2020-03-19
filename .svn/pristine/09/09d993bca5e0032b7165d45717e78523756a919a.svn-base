using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using GenerateTables.Models;

namespace ServerAPI.ViewModels
{
    public class AtimsReportsGetData
    {
        public int ReportId { get; set; }
        public string Parameters { get; set; }
        public string StaticData { get; set; }
    }

    public class AtimsReportsData
    {
        public AtimsReportsTemplate Template { get; set; }
        public JObject Data;
        public JObject StaticData;
    }

    public class AtimsReportsTemplate
    {

        public string ShortId { get; set; }
        public string ProcedureName { get; set; }
    }

    public class AtimsReportsParametersRequest
    {
        public bool Building { get; set; }
        public bool Cell { get; set; }
        public bool Facility { get; set; }
        public bool HousingGroup { get; set; }
        public bool Location { get; set; }
        public bool Number { get; set; }
    }

    public class AtimsReportsParametersData
    {
        public List<HousingUnit> Building { get; set; }
        public List<HousingUnit> Cell { get; set; }
        public List<FacilityVm> Facility { get; set; }
        public List<HousingGroup> HousingGroup { get; set; }
        public List<Privileges> Location { get; set; }
        public List<HousingUnit> Number { get; set; }
        public List<CrimeLookupFlag> ChargeFlag { get; set; }
        public List<ArrestSentenceMethod> SentencingMethod { get; set; }
        public List<AgencyCourtDept> CourtDepartment { get; set; }

    }

    public class AtimsReportsStaticData
    {
        public int PersonnelId { get; set; }
        public string PersonnelNumber { get; set; }
        public string PersonnelNum { get; set;}
        public string BadgeNumber { get; set; }
        public int AgencyId { get; set; }
        public string AgencyName { get; set; }
        public string PersonName { get; set; }
        public int PersonId { get; set; }
        public string PhotoPath { get; set; }
        public string JmsPath { get; set; }
        public string Email { get; set; }
    }

    public class AtimsReportsCommonStaticDataRequest
    {
        public int PersonnelId { get; set; }
    }

    public class AtimsReportsCategoriesList
    {
        public string CategoryName { get; set; }
        public List<AtimsReport> Reports { get; set; }
    }

    public class IncarcerationReleaseReasonVm
    {
        public string ReleaseReason { get; set; }
    }

    public class PersonDescriptorDetailsVm
    {
        public List<string> Category { get; set; }
        public List<string> CategoryMap { get; set; }
        public List<string> ItemLocation { get; set; }
    }

    public class RegistrantVm
    {
        public int RegistrantLookupId { get; set; }
        public string RegistrantName { get; set; }
    }

    public class UserGroupsVm
    {
        public string GroupId { get; set; }
        public string GroupName { get; set; }
    }

}
