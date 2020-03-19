using System.Collections.Generic;
using ServerAPI.ViewModels;
using System.Threading.Tasks;

namespace ServerAPI.Services
{
    public interface IPersonIdentityService
    {
        PersonIdentity GetPersonIdentity(int personId);
        PersonIdentity GetPersonDetails(int personId);
        PersonDetail GetNamePopupDetails(int personId, int inmateId);
        Task<int> InsertNamePopupDetails(PersonDetail personDetail);
        List<PersonHistoryVm> GetPersonSavedHistory(int personId);
        PersonCitizenshipDetail GetCitizenshipList(int personId);
        List<PersonHistoryVm> GetPersonnelCitizenshipHistory(int personId, int personCitizenshipId);
        Task<PersonCitizenshipDetail> InsertUpdatePersonCitizenship(PersonCitizenshipVm citizenship);
        Task<PersonCitizenshipDetail> DeleteUndoPersonCitizenship(PersonCitizenshipVm citizenshipDetail);
        Task<int> InsertUpdatePersonDetails(PersonIdentity person);
        Task<PersonVm> InsertUpdatePerson(PersonVm person);
        void LoadInsertPersonHistory(int personId, string personHistoryList);
        void InsertUpdateCustomField(int personId, List<CustomField> customFields);
        Task<PersonVm> UpdateInmateNumber(int inmateId, string inmateNumber, string akaHistoryList);
        List<CustomField> GetCustomFields(int userControlId, int? PersonId);
    }
}
