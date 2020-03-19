using ServerAPI.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ServerAPI.Services
{
    public interface IClassLinkService
    {
        List<ClassLinkDetails> GetClassLinkDetails(ClassLinkInputs inputs);
        Task<int> InsertClassifyViewerClassLink(ClassLinkAddParam inputs);
        Task<int> UpdateClassifyViewerClassLink(ClassLinkUpdateParam inputs);
        int DeleteClassifyViewerClassLink(int inmateClassificationLinkId, bool isUndo);
        List<ClassLinkViewHistoryVm> GetClassLinkViewHistory(int facilityId, int inmateId);
    }
}
