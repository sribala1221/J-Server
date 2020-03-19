using ServerAPI.ViewModels;

namespace ServerAPI.Services
{
    public interface IClassifyAlertsAssociationService
    {
        ClassifyAlertAssociationVm GetClassifyAssociationDetails(KeepSepSearchVm value);
   
    }
}
