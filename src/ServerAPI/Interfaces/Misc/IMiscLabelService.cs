using System.Collections.Generic;
using System.Threading.Tasks;
using ServerAPI.ViewModels;

namespace ServerAPI.Services
{
    public interface IMiscLabelService
    {
        List<FormTemplateVm> GetMisclabel(int inmateId, string fromScreen, int wizardStepId, int arrestId,
            int personnelId);
        Task<List<FormTemplateVm>> GetMiscPdfLabel(int inmateId, string fromScreen, int wizardStepId, int arrestId,
            int personnelId);

        string GetTemplateSqlDataJsonString(string StoredProcedureName, object ParameterId, int? personId = 0,
            bool isPersonnel = false);
    }
}
