using ServerAPI.ViewModels;
using System.Collections.Generic;

namespace ServerAPI.Interfaces
{
    public interface IHeadCountService
    {
        List<HeadCountVm> GetHeadCountList(ConsoleInputVm value);
        List<HeadCountVm> LoadHeadCountHousingList(ConsoleInputVm value);
        List<HeadCountVm> LoadHeadCountLocationList(ConsoleInputVm value);
    }
}
