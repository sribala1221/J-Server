using GenerateTables.Models;
using ServerAPI.Utilities;
using ServerAPI.ViewModels;
using System.Collections.Generic;
using System.Linq;

namespace ServerAPI.Services
{
	public class InmateNotesService : IInmateNotesService
	{
		private readonly AAtims _context;

		public InmateNotesService(AAtims context)
		{
			_context = context;
		}

		//Get Inmate Notes summary details
		public IEnumerable<FloorNotesVm> GetInmateNotes(int inmateId)
		{
			IQueryable<PersonnelVm> lstPersonnel = _context.Personnel.Select(s => new PersonnelVm
			{
				PersonLastName = s.PersonNavigation.PersonLastName,
				OfficerBadgeNumber = s.OfficerBadgeNum,
				PersonnelId = s.PersonnelId
			});

			List<FloorNotesVm> listInmateNotes = _context.FloorNoteXref
				.Where(x => x.InmateId == inmateId)
				.Select(y => new FloorNotesVm
				{
					FloorNoteId = y.FloorNoteId,
					FloorNoteDate = y.FloorNote.FloorNoteDate,
					FloorNoteNarrative = y.FloorNote.FloorNoteNarrative,
					FloorNoteLocation = y.FloorNote.FloorNoteLocation,
					FloorNoteLocationId = y.FloorNote.FloorNoteLocationId,
					FloorNoteType = y.FloorNote.FloorNoteType,
					FloorNoteOfficerId = y.FloorNote.FloorNoteOfficerId,
					Personnel = lstPersonnel.SingleOrDefault(w => w.PersonnelId == y.FloorNote.FloorNoteOfficerId)
				}).OrderByDescending(f => f.FloorNoteId).ToList();

			return listInmateNotes;
		}

		//Get Inmate Notes summary details based on NoteType
		public IEnumerable<FloorNotesVm> GetInmateNotesByType(int inmateId, string noteType)
		{
			IQueryable<PersonnelVm> lstPersonnel = _context.Personnel.Select(s => new PersonnelVm
			{
				PersonLastName = s.PersonNavigation.PersonLastName,
				OfficerBadgeNumber = s.OfficerBadgeNum,
				PersonnelId = s.PersonnelId
			});

			List<FloorNotesVm> listInmateNotes = _context.FloorNoteXref
				.Where(x => x.InmateId == inmateId && (noteType == FloorNote.ALL || x.FloorNote.FloorNoteType == noteType))
				.Select(y => new FloorNotesVm
				{
					FloorNoteId = y.FloorNoteId,
					FloorNoteDate = y.FloorNote.FloorNoteDate,
					FloorNoteNarrative = y.FloorNote.FloorNoteNarrative,
					FloorNoteLocation = y.FloorNote.FloorNoteLocation,
					FloorNoteLocationId = y.FloorNote.FloorNoteLocationId,
					FloorNoteType = y.FloorNote.FloorNoteType,
					FloorNoteOfficerId = y.FloorNote.FloorNoteOfficerId,
					Personnel = lstPersonnel.SingleOrDefault(w => w.PersonnelId == y.FloorNote.FloorNoteOfficerId)
				}).OrderByDescending(f => f.FloorNoteId).ToList();

			return listInmateNotes;
		}

		//Get NoteType along with Count
		public IEnumerable<FloorNoteTypeCount> GetFloorNoteTypeCount(int inmateId) => _context.FloorNoteXref
					.Where(x => x.InmateId == inmateId)
					.GroupBy(y => y.FloorNote.FloorNoteType)
					.Select(z => new FloorNoteTypeCount
					{
						Name = z.Key,
						Count = z.Count()
					}).ToList();



	}
}