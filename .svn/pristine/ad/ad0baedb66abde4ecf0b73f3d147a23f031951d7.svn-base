using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ServerAPI.Services;
using ServerAPI.ViewModels;
using ServerAPI.Authorization;

namespace ServerAPI.Controllers
{
    [Route("[controller]")]
    public class PersonSearchController : ControllerBase
    {

        private readonly IPersonSearchService _personSearch;
        public PersonSearchController(IPersonSearchService personSearch)
        {
            _personSearch = personSearch;
        }

        [HttpGet("GetPersons")]
        public IActionResult GetPersons(string lastName, string firstName, string middleName, string suffix, DateTime? dob) => 
            !string.IsNullOrWhiteSpace(firstName) || !string.IsNullOrWhiteSpace(lastName)
                ? Ok(_personSearch.GetPersonsWeightedSearch(lastName, firstName, middleName, suffix, dob))
                : (IActionResult)BadRequest("First or Last Name Required!");

        [AllowAnonymous]
        [HttpGet("GetPersonByInmateNo")]//gets PersonSearchResult with an exact match
        public IActionResult GetPersonByInmateNo(string inmateNo) => !string.IsNullOrWhiteSpace(inmateNo) ? 
            Ok(_personSearch.GetPersonByInmateNo(inmateNo)) : (IActionResult)BadRequest("InmateNo Required!");

        [HttpGet("GetPersonsByInmateNo")]//gets list with contains
        public IActionResult GetPersonsByInmateNo(string inmateNo) => !string.IsNullOrWhiteSpace(inmateNo) ? 
            Ok(_personSearch.GetPersonsByInmateNo(inmateNo)) : (IActionResult)BadRequest("InmateNo Required!");

        [HttpGet("GetPersonsByDLN")]
        public IActionResult GetPersonsByDln(string dln, string dlState) => !string.IsNullOrWhiteSpace(dln) ? 
            Ok(_personSearch.GetPersonsByDln(dln, dlState)) : (IActionResult)BadRequest("DLN Required!");

        [HttpGet("GetPersonsAll")]
        public IActionResult GetPersonsAll([FromQuery]PersonSearchVm person) => 
            string.IsNullOrWhiteSpace(person.FirstName) || string.IsNullOrWhiteSpace(person.LastName)
                ? BadRequest("First or Last Name Required!")
                : !string.IsNullOrWhiteSpace(person.FirstName) || person.FirstName.Length < 2
                ? BadRequest("First Name Requires > 2 Characters!")
                : string.IsNullOrWhiteSpace(person.LastName) || person.LastName.Length < 2
                ? BadRequest("Last Name Requires > 2 Characters!")
                : (IActionResult)Ok(_personSearch.GetPersonsAll(person));

        [HttpGet("GetPersonsByCii")]
        public IActionResult GetPersonsByCii(string cii) => Ok(_personSearch.GetPersonsByCii(cii));

        [HttpGet("GetPersonsBySsn")]
        public IActionResult GetPersonsBySsn(string ssn) => Ok(_personSearch.GetPersonsBySsn(ssn));

        [HttpGet("GetPersonsByMoniker")]
        public IActionResult GetPersonsByMoniker(string moniker) => Ok(_personSearch.GetPersonsByMoniker(moniker));

        [HttpGet("GetPersonStates")]
        public IActionResult GetPersonStates() => Ok(_personSearch.GetPersonStates());

        [FuncPermission(601, FuncPermissionType.Access)]
        [HttpGet("GetPersonSearchAccess")]
        public IActionResult GetPersonSearchAccess() => Ok();
    }
}
