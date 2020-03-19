using System.Collections.Generic;
using GenerateTables.Models;

namespace ServerAPI.Services
{
    public interface IPhotosService
    {
        string GetExternalPath();
        string GetPath();
        string GetJmsPath();
        string GetPbpcPath();
        string GetPhotoByPersonId(int personId);
        string GetPhotoByIdentifierId(int identifierId);
        string GetPhotoByIdentifier(Identifiers identifier);
        string GetPhotoByPerson(Person person);
        string GetPhotoByCollectionIdentifier(ICollection<Identifiers> identifiers);
    }
}
