using System.Collections.Generic;
using System.Threading.Tasks;
using ServerAPI.ViewModels;
using GenerateTables.Models;

namespace ServerAPI.Services
{
    public interface IIncidentAppealsService
    {
        List<KeyValuePair<string, int>> GetIncidentAppealsCount(int facilityId);
        List<Appeals> GetIncidentAppeals(AppealsParam objAppealsParam);
        Task<int> InsertDisciplinaryInmateAppeal(DispInmateAppeal inmateAppeal);
        Task<int> UpdateDisciplinaryInmateAppeal(int dispInmateAppealId, DispInmateAppeal inmateAppeal);
		int GetAppealsCountByIncidentId(int incidentId);
	}
}
