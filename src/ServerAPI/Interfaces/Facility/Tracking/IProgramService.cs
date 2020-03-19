using ServerAPI.ViewModels;
using System.Collections.Generic;

namespace ServerAPI.Services
{
    public interface IProgramService
    {
        IEnumerable<KeyValuePair<int, string>> GetLocationList(int? facilityId);
        IEnumerable<ProgramVm> GetProgramList(int? facilityId);
        IEnumerable<object> GetProgramAppts();
    }
}
