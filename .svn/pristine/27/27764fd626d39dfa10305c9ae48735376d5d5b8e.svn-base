using ServerAPI.ViewModels;
using System.Collections.Generic;


namespace ServerAPI.Services
{
	public interface IInmateNotesService
	{
		IEnumerable<FloorNotesVm> GetInmateNotes(int inmateId);
		IEnumerable<FloorNotesVm> GetInmateNotesByType(int inmateId, string noteType);
		IEnumerable<FloorNoteTypeCount> GetFloorNoteTypeCount(int inmateId);
	}
}