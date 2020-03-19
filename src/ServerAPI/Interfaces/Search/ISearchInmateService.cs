using GenerateTables.Models;
using ServerAPI.ViewModels;
using System.Collections.Generic;
using System.Linq;

// ReSharper disable once CheckNamespace
namespace ServerAPI.Services
{

    public interface ISearchInmateService
    {
        List<HousingDetail> GetHousing(int facilityId);

        List<HousingUnitList> PodDetails(string buildingId);

        // List<HousingUnitListVm> GetPodDetails(int facilityId,string building);
        List<FlagAlertVm> GetFlagAlertDetails(bool isPermission);
        IEnumerable<LookupVm> GetClassificationDetails();

        List<PrivilegeDetailsVm> GetLocationDetails();
        List<PersonnelVm> GetPersonnel(string type, int agencyId);
        InmateBookingInfoVm GetInmateBookingInfo();

        List<ArrestSentenceMethod> GetSentenceMethodInfo();
        List<AgencyVm> GetAgencies();
        List<CrimeLookupFlag> GetChargeFlag();
        string GetCaseType(string arrestType);
        List<SearchResult> GetBookingSearchList(SearchRequestVm searchDetails);
        IQueryable<Lookup> GetLookups();
    }
}