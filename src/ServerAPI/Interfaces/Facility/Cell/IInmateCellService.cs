using ServerAPI.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ServerAPI.Services
{
    public interface IInmateCellService
    {
        InmateCellVm GetInmateCell(CellInmateInputs input);
        List<HousingInmateHistory> GetInmateCellHistory(CellInmateInputs input);
        CellLogVm GetCellLog(int cellLodId,int facilityId);
        Task<int> InmateCellLog(InmateCellLogDetailsVm  values);
    }
}
