using GenerateTables.Models;
using ServerAPI.ViewModels;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace ServerAPI.Services
{
    public interface ISearchInmateCaseService
    {
         List<SearchResult> GetBookingSearchList(SearchRequestVm searchDetails);
         List<Facility> GetFacilityDetails();
    }
}