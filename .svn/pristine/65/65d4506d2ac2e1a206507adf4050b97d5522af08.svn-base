using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ServerAPI.ViewModels;

namespace ServerAPI.Services
{
    public interface IInmatePhoneService
    {
        InmatePhoneHistoryVm GetCallLogHistroy(int inmateId);
        Task<int> InsertUpdateCallLog(PhoneDetailsVm objCallDetails);
        InmatePhoneHistoryVm GetPinHistroy(int inmateId);
        Task<int> InsertDeletePhonePin(PhoneDetailsVm objPhonePin);
    }
}
