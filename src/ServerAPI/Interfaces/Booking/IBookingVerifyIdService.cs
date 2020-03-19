using ServerAPI.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace ServerAPI.Services
{
    public interface IBookingVerifyIdService
    {
        List<BookingVerifyDataVm> GetVerifyInmateDetail(int facilityId);
        Task<int> UpdateVerifyDetail(BookingVerifyDataVm verifyDataVm);
        List<BookingVerifyDataVm> GetDuplicateRecords(BookingVerifyDataVm verifyDataVm);
        bool GetVerifyInmate(int incarcerationId);
        Task<int> CreateInmate(PersonVm personVm);
        List<KeyValuePair<PersonVm, bool>> GetParticularPersonnel(string personnelSearch);
        string EditSearchOfficierDetails(int arrestId);
    }
}
