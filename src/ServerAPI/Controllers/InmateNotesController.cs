using Microsoft.AspNetCore.Mvc;
using ServerAPI.Services;
using ServerAPI.Utilities;
using ServerAPI.ViewModels;

namespace ServerAPI.Controllers
{
    [Route("[controller]")]
    public class InmateNotesController : ControllerBase
    {
        #region Properties

        private readonly IInmateNotesService _inmateNotesService;
        private readonly ICommonService _commonService;

        #endregion

        #region Constructor

        public InmateNotesController(IInmateNotesService inmateNotesService, ICommonService commonService)
        {
            _inmateNotesService = inmateNotesService;
            _commonService = commonService;
        }

        #endregion

        #region InmateNotesSummary       

        //Get Inmate Note summary details
        [HttpGet("GetInmateNotes")]
        public IActionResult GetInmateNotesSummary(int inmateId)
        {
            return Ok(_inmateNotesService.GetInmateNotes(inmateId));
        }

        //Get Inmate Note summary details based on NoteType
        [HttpGet("GetInmateNotesByType")]
        public IActionResult GetInmateNotesByNoteType(int inmateId, string noteType)
        {
            return Ok(_inmateNotesService.GetInmateNotesByType(inmateId, noteType));
        }

        //Get Lookup details for Inmate Notesdrop down in Add Screen
        [HttpGet("GetNoteType")]
        public IActionResult GetNoteType()
        {
            return Ok(_commonService.GetLookupKeyValuePairs(LookupConstants.NOTETYPEINMATE));
        }

        //Get NoteType Count Details
        [HttpGet("GetNoteTypeCountDetails")]
        public IActionResult GetNoteTypeCount(int inmateId)
        {
            return Ok(_inmateNotesService.GetFloorNoteTypeCount(inmateId));
        }

        #endregion

    }

}