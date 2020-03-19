using System.Collections.Generic;
using ServerAPI.ViewModels;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace ServerAPI.Services
{
    public interface ICellService
    {
        List<HousingUnitGroupVm> GetHousingGroup(int facilityId);
        List<HousingUnitListVm> GetHousingUnit(int facilityId);
        MyLogCountDetailsVm GetMyLogDetailsCount(MyLogRequestVm mylog);
        MylogDetailsVm GetMyLogDetails(MyLogRequestVm logReqDetails);
        Task<int> SetCurrentStatus(MyLogRequestVm statusReq);
        Task<int> AddLogDetails(MyLogRequestVm logDetails);
        Task<int> ClearAttendanceDetails(MyLogRequestVm attendance);
        Task<int> SetHousingDetails(MyLogRequestVm setHousingReq);
        Task<LogSettingDetails> GetMyLogSettings();
        Task<int> DeleteUndoMyLog(CellLogDetailsVm log);
        Task<IdentityResult> UpdateUserSettings(LogSettingDetails objfilter);
    }
}
