using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ServerAPI.ViewModels;

namespace ServerAPI.Services
{
    public interface ISearchInmateBookingService
    {
        List<SearchResult> GetBookingSearchList(SearchRequestVm searchDetails);
         
    }
}