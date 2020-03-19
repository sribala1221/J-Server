using ServerAPI.ViewModels;
using System.Threading.Tasks;

namespace ServerAPI.Services
{
    public interface IClassifyClassFileService
    {
        Task<ClassFileOutputs> GetClassFile(ClassFileInputs classFileInputs);
        Task<int> InmateDeleteUndo(DeleteParams deleteParams);
        bool PendingCheck(int inmateId);
    }
}
