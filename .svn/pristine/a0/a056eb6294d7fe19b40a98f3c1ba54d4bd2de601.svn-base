using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ServerAPI.ViewModels;
using GenerateTables.Models;
using ServerAPI.Utilities;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;

namespace ServerAPI.Services
{
    public class PersonTestingService : IPersonTestingService
    {
        private readonly AAtims _context;
        private readonly IPersonService _personService;
        private readonly int _personnelId;
        private readonly IInterfaceEngineService _interfaceEngine;

        public PersonTestingService(AAtims context, IPersonService personService,
            IHttpContextAccessor httpContextAccessor, IInterfaceEngineService interfaceEngine)
        {
            _context = context;
            _personService = personService;
            _personnelId = Convert.ToInt32(httpContextAccessor.HttpContext.User
                .FindFirst(AuthDetailConstants.PERSONNELID)?.Value);
            _interfaceEngine = interfaceEngine;
        }

        //get testing grid details
        public List<TestingVm> GetTestingDetails(int personId)
        {
            List<TestingVm> lstTestingDet = _context.PersonTesting
                .Where(x => x.PersonId == personId)
                .Select(x => new TestingVm
                {
                    TestingId = x.PersonTestingId,
                    Type = x.TestingType,
                    Requested = x.TestingRequested,
                    RequestDate = x.TestingDateRequired,
                    GatheredDate = x.TestingDateGathered,
                    PerformedBy = x.TestingTestBy,
                    DispositionText = x.TestingDisposition,
                    DateProcessed = x.TestingDateProcessed,
                    ProcessedBy = x.TestingProcessBy,
                    ProcessedDisposition = x.TestingProcessDisposition,
                    Notes = x.TestingNotes,
                    CreateDate = x.CreateDate,
                    UpdateDate = x.UpdateDate,
                    PersonnelId = x.PersonnelId,
                    UpdatePersonnelId = x.UpdatePersonnelId
                }).ToList();

            if (lstTestingDet.Count <= 0) return lstTestingDet;
            List<int> personnelId =
                lstTestingDet.Select(i => new[] {i.PersonnelId, i.UpdatePersonnelId})
                    .SelectMany(i => i)
                    .Where(i => i.HasValue)
                    .Select(i => i.Value)
                    .ToList();

            //get all person details for list of personnel id
            List<PersonnelVm> lstPersonDet = _personService.GetPersonNameList(personnelId);
            PersonnelVm personInfo;

            int[] arrTypeText = lstTestingDet.Where(i => i.Type.HasValue).Select(i => i.Type.Value).ToArray();
            List<LookupVm> lstLookup = _context.Lookup.Where(l => l.LookupType == LookupConstants.TESTTYPE
                                                                  && arrTypeText.Contains(l.LookupIndex))
                .Select(l => new LookupVm
                {
                    LookupIndex = l.LookupIndex,
                    LookupDescription = l.LookupDescription
                }).ToList();

            lstTestingDet.ForEach(item =>
            {
                item.TypeText = lstLookup.SingleOrDefault(
                    x => (int?)(x.LookupIndex) == item.Type)?.LookupDescription;
                personInfo = lstPersonDet.Single(p => p.PersonnelId == item.PersonnelId);
                item.CreateByPersonLastName = personInfo.PersonLastName;
                item.CreateByPersonFirstName = personInfo.PersonFirstName;
                item.OfficerBadgeNumber = personInfo.OfficerBadgeNumber;

                if (!(item.UpdatePersonnelId > 0)) return;
                {
                    personInfo = lstPersonDet.Single(p => p.PersonnelId == item.UpdatePersonnelId);
                    item.UpdateByPersonLastName = personInfo.PersonLastName;
                    item.UpdateByPersonFirstName = personInfo.PersonFirstName;
                    item.UpdateOfficerBadgeNumber = personInfo.OfficerBadgeNumber;
                }
            });
            return lstTestingDet;
        }

        public Task<int> InsertUpdateTestingDetails(TestingVm testingDet)
        {
            PersonTesting dbTesting =
                _context.PersonTesting.SingleOrDefault(x => x.PersonTestingId == testingDet.TestingId);
            if (dbTesting == null)
            {
                dbTesting = new PersonTesting
                {
                    PersonId = testingDet.PersonId,
                    CreateDate = DateTime.Now,
                    PersonnelId = _personnelId
                };
            }
            else
            {
                dbTesting.UpdateDate = DateTime.Now;
                dbTesting.UpdatePersonnelId = _personnelId;
            }
            dbTesting.TestingType = testingDet.Type;
            dbTesting.TestingRequested = testingDet.Requested;
            dbTesting.TestingDateRequired = testingDet.RequestDate;
            dbTesting.TestingDateGathered = testingDet.GatheredDate;
            dbTesting.TestingTestBy = testingDet.PerformedBy;
            dbTesting.TestingNotes = testingDet.Notes;
            dbTesting.TestingDisposition = testingDet.DispositionText;
            dbTesting.TestingDateProcessed = testingDet.DateProcessed;
            dbTesting.TestingProcessBy = testingDet.ProcessedBy;
            dbTesting.TestingProcessDisposition = testingDet.ProcessedDisposition;

            if (dbTesting.PersonTestingId <= 0)
            {
                _context.PersonTesting.Add(dbTesting);
                _context.SaveChanges();
                _interfaceEngine.Export(new ExportRequestVm
                {
                    EventName = EventNameConstants.PERSONTESTING,
                    PersonnelId = _personnelId,
                    Param1 = dbTesting.PersonId.ToString(),
                    Param2 = dbTesting.PersonTestingId.ToString()
                });
            }
            InsertPersonTestingHistory(dbTesting.PersonTestingId, testingDet.TestingHistoryList);
            return _context.SaveChangesAsync();
        }

        private void InsertPersonTestingHistory(int personTestingId, string testingHistoryList)
        {
            PersonTestingHistory dbPerTestingHis = new PersonTestingHistory
            {
                PersonTestingId = personTestingId,
                PersonnelId = _personnelId,
                CreateDate = DateTime.Now,
                TestingHistoryList = testingHistoryList
            };
            _context.PersonTestingHistory.Add(dbPerTestingHis);
        }

        public List<HistoryVm> GetTestingHistoryDetails(int testingId)
        {
            List<HistoryVm> lstTesting = _context.PersonTestingHistory
                .Where(ph => ph.PersonTestingId == testingId)
                .OrderByDescending(ph => ph.CreateDate)
                .Select(ph => new HistoryVm
                {
                    HistoryId = ph.PersonTestingHistoryId,
                    CreateDate = ph.CreateDate,
                    PersonId = ph.Personnel.PersonId,
                    OfficerBadgeNumber = ph.Personnel.OfficerBadgeNum,
                    HistoryList = ph.TestingHistoryList
                }).ToList();
            if (lstTesting.Count <= 0) return lstTesting;
            //To Improve Performence All Person Details Loaded By Single Hit Before Looping
            int[] personIds = lstTesting.Select(x => x.PersonId).ToArray();
            //get person list
            List<Person> lstPersonDet = _context.Person.Where(per => personIds.Contains(per.PersonId)).ToList();

            lstTesting.ForEach(item =>
            {
                item.PersonLastName =
                    lstPersonDet.SingleOrDefault(p => p.PersonId == item.PersonId)?.PersonLastName;
                //To GetJson Result Into Dictionary
                if (item.HistoryList == null) return;
                Dictionary<string, string> personHistoryList =
                    JsonConvert.DeserializeObject<Dictionary<string, string>>(item.HistoryList);
                item.Header =
                    personHistoryList.Select(ph => new PersonHeader {Header = ph.Key, Detail = ph.Value})
                        .ToList();
            });
            return lstTesting;
        }
    }
}
