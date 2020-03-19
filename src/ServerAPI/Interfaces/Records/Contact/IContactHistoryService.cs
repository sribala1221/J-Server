using ServerAPI.ViewModels;
using System.Collections.Generic;

// ReSharper disable once CheckNamespace
namespace ServerAPI.Services
{
    public interface IContactHistoryService
    {
        List<ContactHistoryDetailVm> GetContacts(ContactHistoryVm searchValue);
        List<ContactHistoryDetailVm> GetContactDetail(ContactHistoryModelVm modelVm);
        List<ContactHistoryDetailVm> GetPersonnelContacts(ContactHistoryVm searchValue);
        List<ContactHistoryDetailVm> GetPersonnelDetail(ContactHistoryDetailVm contacts);
    }
}
