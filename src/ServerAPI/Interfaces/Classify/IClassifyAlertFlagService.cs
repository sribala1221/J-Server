using ServerAPI.ViewModels;
using System.Collections.Generic;

namespace ServerAPI.Services
{
    public interface IClassifyAlertFlagService
    {
        ClassifyAlertVm GetClassifyAlert(KeepSepSearchVm keepSepSearch);
        List<HousingUnitVm> GetClassifyAlertHousingCount(List<ClassifyAlertVm> classifyAlerts);
        List<LookupVm> GetFlagCount(List<ClassifyAlertVm> classifyAlerts);

    }
}
