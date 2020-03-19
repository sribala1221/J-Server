using System;
using ServerAPI.ViewModels;
using System.Collections.Generic;

namespace ServerAPI.Services
{
    public interface IPersonSearchService
    {
        List<PersonPreBookInfo> GetPersons(string lastName, string firstName, string middleName, string suffix, string dob); //TODO: Add Middle, Suffix and DOB as optional parameters
        PersonSearchResult GetPersonByInmateNo(string inmateNo);
        List<PersonPreBookInfo> GetPersonsByInmateNo(string inmateNo);
        List<PersonPreBookInfo> GetPersonsByDln(string dln, string dlState);
        List<PersonSearchResult> GetPersonsAll(PersonSearchVm person);
        List<PersonWeightedSearchResult> GetPersonsWeightedSearch(string lastName, string firstName, string middleName, string suffix, DateTime? dob); 
        List<PersonPreBookInfo> GetPersonsByCii(string cii);
        List<PersonPreBookInfo> GetPersonsBySsn(string ssn);
        List<PersonPreBookInfo> GetPersonsByMoniker(string moniker);
        List<string> GetPersonStates();
    }
}
