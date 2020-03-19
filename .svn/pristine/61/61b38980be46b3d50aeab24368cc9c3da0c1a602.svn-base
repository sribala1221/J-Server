using System.Collections.Generic;
using System.Threading.Tasks;
using ServerAPI.ViewModels;

// ReSharper disable once CheckNamespace
namespace ServerAPI.Services
{
	public interface IKeepSepAlertService
	{
		Task<string> InsertUpdateKeepSepInmateDetails(KeepSeparateVm keepSepDetails);
		Task<string> InsertUpdateKeepSepAssocDetails(KeepSeparateVm keepSepDetails);
		Task<string> InsertUpdateKeepSepSubsetDetails(KeepSeparateVm keepSepDetails);
		Task<bool> DeleteUndoKeepSeparateDetails(KeepSeparateVm keepSepDetails);
		KeepSeparateAlertVm GetKeepSeparateAlertDetails(int inmateId);
		List<HistoryVm> GetKeepSeparateHistory(int keepSeparateId, KeepSepType keepSepType);
		List<KeepSepInmateDetailsVm> GetKeepSepAssocSubsetDetails(string keepSepType, int? subset, KeepSepType type);
		List<KeepSepInmateDetailsVm> GetKeepSepInmateConflictDetails(int inmateId);
	    List<KeepSeparateVm> GetInmateKeepSep(int inmateId, bool isNotFromAppt = true);
	    List<PersonClassificationDetails> GetAssociation(int personId);
    }
}
