using ServerAPI.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ServerAPI.Services
{
    public interface IPersonDescriptorService
    {
        List<PersonDescriptorVm> GetDescriptorDetails(int personId);
        PersonDescriptorDetails GetIdentifierDetails(int personId, bool isdelete);
        PersonBodyDescriptor GetDescriptorLookupDetails(string[] bodyMap,int personId);
        Task<int> DeleteUndoDescriptor(PersonDescriptorVm descriptor);
        Task<int> InsertUpdateDescriptor(PersonDescriptorVm descriptor);
        List<PersonDescriptorVm> PersonDescriptorLookup();
    }
}
