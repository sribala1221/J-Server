using ServerAPI.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace ServerAPI.Services
{
    public interface IClassifyViewerService
    {
       ClassLogDetails GetClassLog(ClassLogInputs classLogInput);      
       //Task<int> UpdateClassifySettings(LogParameters inputs);
       List<ClassLog> GetInmateClassify(int inmateId);
       Task<ClassLogDetails> GetClassifyLog(int facilityId);
       Task<LogParameters> GetClassifySettings();
       Task<IdentityResult> UpdateClassify(LogParameters logParameter);
    }
}
