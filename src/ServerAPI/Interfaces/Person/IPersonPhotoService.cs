using Microsoft.AspNetCore.Http;
using ServerAPI.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ServerAPI.Services
{
    public interface IPersonPhotoService
    {
        List<PersonPhoto> GetPersonPhoto(int personId, bool showDeleted = false);
        Task<int> InsertUpdatePersonPhoto(PersonPhoto personPhoto);
        List<PersonDescriptorVm> GetPersonPhotoDescriptor(int personId);
        List<string> GetTempImages(string facilityName);
        string UploadTempPhoto(IFormFile uploadedFile,string facilityName);
        void DeleteTempPhoto(string fileName);
        Task<int> DeleteOrUndoPhoto(DeleteParams deleteParams);
        List<IdentifierVm> GetIdentifier(int personId);
    }
}
