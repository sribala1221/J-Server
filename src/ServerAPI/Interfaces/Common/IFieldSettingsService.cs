using ServerAPI.ViewModels;
using System.Collections.Generic;


namespace ServerAPI.Services
{
    public interface IFieldSettingsService
    {
        List<AppAoFieldLabelVm> GetFieldLabels(int[] fieldLabelId);
        List<AppAoUserControlFieldsVm> GetFieldSettings();
        AppAoUserControlFieldsVm GetFieldSettings(int ucId);
    }
}
