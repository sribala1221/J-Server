using ServerAPI.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace ServerAPI.Services
{
    public interface IPrebookActiveService
    {
        List<InmatePrebookVm> GetPrebooks(bool deleteFlag);

        InmatePrebookVm GetPrebook(int inmatePrebookId);

        List<InmatePrebookVm> GetPrebookSearch(GetPrebookSearchVm value);
        Task<int> DeletePrebook(int inmatePrebookId);
        Task<int> UndoDeletePrebook(int inmatePrebookId);
        void InsertPrebookPerson(int inmatePrebookId, int personId);
        IntakePrebookSelectVm LoadPrebookDetails(int personId);
        PrebookValidateConfirmData GetPrebookValidateConfirm(int personId);
        bool UpdatePrebookValidateConfirm(string values, int personId);
    }
}