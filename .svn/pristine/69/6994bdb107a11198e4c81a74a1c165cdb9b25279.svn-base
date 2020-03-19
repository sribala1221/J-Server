using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ServerAPI.ViewModels;

namespace ServerAPI.Services
{
    public interface IPersonAddressService
    {
        PersonAddressDetails GetPersonAddressDetails(int personId);
        Task<PersonAddressDetails> InsertUpdateAddressDetails(PersonAddressDetails personAddressDetails);
        Task<int> InsertUpdateAddress(PersonAddressVm personAddress, bool personAddressSave);
    }
}
