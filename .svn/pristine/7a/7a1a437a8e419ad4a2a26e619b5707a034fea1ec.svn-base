using System.Collections.Generic;
using System.Threading.Tasks;
using ServerAPI.ViewModels;

// ReSharper disable once CheckNamespace
namespace ServerAPI.Services
{
    public interface IWhosInCustodyService
    {
        List<WhosInCustodyRemoveVm> GetWhosInCustody();
        Task<int> InsertWhosInCustody(WhosInCustodyRemoveVm model);
        Task<int> UpdateWhosInCustody(WhosInCustodyRemoveVm model);
        List<WhosInCustodyRemoveVm> DeleteWhosInCustody(WhosInCustodyRemoveVm model);
        List<WhosInCustodyRemoveVm> UndoWhosInCustody(WhosInCustodyRemoveVm model);
    }
}