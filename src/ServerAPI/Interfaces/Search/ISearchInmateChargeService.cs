using ServerAPI.ViewModels;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace ServerAPI.Services
{
    public interface ISearchInmateChargeService
    {
         List<SearchResult> GetBookingSearchList(SearchRequestVm searchDetails);
    }
}