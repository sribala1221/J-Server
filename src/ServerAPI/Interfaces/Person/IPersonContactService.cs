using System.Threading.Tasks;
using ServerAPI.ViewModels;

namespace ServerAPI.Services
{
    public interface IPersonContactService
    {
        ContactDetails GetContactDetails(int typePersonId);
        Task<int> InsertUpdateContactAttempt(ContactAttemptVm contactAttempt);
        Task<int> DeleteUndoContactAttempt(int contactAttemptId);
        Task<int> InsertUpdateContact(ContactVm contact);
        Task<int> DeleteUndoContact(ContactVm contactDet);
        ContactVm GetContactCreateUpdateDetails(int contactId);
    }
}
