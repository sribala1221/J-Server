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
    public class PersonDnaService : IPersonDnaService
    {
        private readonly AAtims _context;
        private readonly int _personnelId;
        private readonly IPersonService _personService;

        public PersonDnaService(AAtims context, IHttpContextAccessor httpContextAccessor,
            IPersonService personService)
        {
            _context = context;
            _personnelId = Convert.ToInt32(httpContextAccessor.HttpContext.User
                .FindFirst(AuthDetailConstants.PERSONNELID)?.Value);
            _personService = personService;
        }

        //get dna details for grid list
        public List<DnaVm> GetDnaDetails(int personId)
        {
            List<DnaVm> dnaVm = _context.PersonDna.Where(dna => dna.PersonId == personId)
                .Select(dna => new DnaVm
                {
                    PersonId = dna.PersonId,
                    DnaPersonnelId = dna.PersonnelId,
                    DnaId = dna.PersonDnaId,
                    RequestDate = dna.DnaDateRequired,
                    GatheredDate = dna.DnaDateGathered,
                    Disposition = dna.DnaDisposition,
                    PerformedBy = dna.DnaTestBy,
                    Notes = dna.DnaNotes,
                    Requested = dna.DnaRequested,
                    UpdateDate = dna.UpdateDate,
                    CreateDate = dna.CreateDate,
                    DateProcessed = dna.DnaDateProcessed,
                    ProcessedBy = dna.DnaProcessedBy,
                    ProcessedDisposition = dna.DnaProcessedDisposition
                }).ToList();

            if (dnaVm.Count <= 0) return dnaVm;
            //get all dna list personnel id into single list
            List<int> personnelId = dnaVm.Select(i => i.DnaPersonnelId).ToList();
            //get all person details for list of personnel id
            List<PersonnelVm> lstPersonDet = _personService.GetPersonNameList(personnelId);
            PersonnelVm personInfo;

            int[] arrDispo = dnaVm.Where(i => i.Disposition.HasValue).Select(i => i.Disposition.Value).ToArray();
            List<LookupVm> lstLookup = _context.Lookup.Where(l => l.LookupType == LookupConstants.DNADISPO
                                                                  && arrDispo.Contains(l.LookupIndex))
                .Select(l => new LookupVm
                {
                    LookupIndex = l.LookupIndex,
                    LookupDescription = l.LookupDescription
                }).ToList();

            dnaVm.ForEach(item =>
            {
                item.DispositionText = lstLookup.SingleOrDefault(
                    x => x.LookupIndex == item.Disposition)?.LookupDescription;
                personInfo = lstPersonDet.Single(p => p.PersonnelId == item.DnaPersonnelId);
                item.CreateByPersonLastName = personInfo.PersonLastName;
                item.CreateByPersonFirstName = personInfo.PersonFirstName;
                item.OfficerBadgeNumber = personInfo.OfficerBadgeNumber;
            });
            return dnaVm;
        }

        //insert and update for person dna details
        public Task<int> InsertUpdatePersonDna(DnaVm dna)
        {
            PersonDna dbPerDna = _context.PersonDna
                                     .SingleOrDefault(a => a.PersonDnaId == dna.DnaId) ?? new PersonDna
                                 {
                                     PersonId = dna.PersonId,
                                     PersonnelId = _personnelId,
                                     CreateDate = DateTime.Now
                                 };
            dbPerDna.DnaRequested = dna.Requested;
            dbPerDna.DnaDateRequired = dna.RequestDate;
            dbPerDna.DnaDateGathered = dna.GatheredDate;
            dbPerDna.DnaTestBy = dna.PerformedBy;
            dbPerDna.DnaNotes = dna.Notes;
            dbPerDna.DnaDisposition = dna.Disposition;
            dbPerDna.UpdateDate = DateTime.Now;
            dbPerDna.DnaDateProcessed = dna.DateProcessed;
            dbPerDna.DnaProcessedBy = dna.ProcessedBy;
            dbPerDna.DnaProcessedDisposition = dna.ProcessedDisposition;

            if (dbPerDna.PersonDnaId <= 0)
            {
                _context.PersonDna.Add(dbPerDna);
            }
            InsertPersonDnaHistory(dbPerDna.PersonDnaId, dna.PersonDnaHistoryList);
            return _context.SaveChangesAsync();
        }

        private void InsertPersonDnaHistory(int personDnaId, string personDnaHistoryList)
        {
            PersonDnaHistory dbPerDnaHis = new PersonDnaHistory
            {
                PersonDnaId = personDnaId,
                PersonnelId = _personnelId,
                CreateDate = DateTime.Now,
                PersonDnaHistoryList = personDnaHistoryList
            };
            _context.PersonDnaHistory.Add(dbPerDnaHis);
        }

        public List<HistoryVm> GetDnaHistoryDetails(int dnaId)
        {
            List<HistoryVm> lstDna = _context.PersonDnaHistory.Where(ph => ph.PersonDnaId == dnaId)
                .OrderByDescending(ph => ph.CreateDate)
                .Select(ph => new HistoryVm
                {
                    HistoryId = ph.PersonDnaHistoryId,
                    CreateDate = ph.CreateDate,
                    PersonId = ph.Personnel.PersonId,
                    OfficerBadgeNumber = ph.Personnel.OfficerBadgeNum,
                    HistoryList = ph.PersonDnaHistoryList
                }).ToList();
            if (lstDna.Count <= 0) return lstDna;
            //To Improve Performence All Person Details Loaded By Single Hit Before Looping
            int[] personIds = lstDna.Select(x => x.PersonId).ToArray();
            //get person list
            List<Person> lstPersonDet = _context.Person.Where(per => personIds.Contains(per.PersonId)).ToList();

            lstDna.ForEach(item =>
            {
                item.PersonLastName = lstPersonDet.SingleOrDefault(p => p.PersonId == item.PersonId)?.PersonLastName;
                //To GetJson Result Into Dictionary
                if (item.HistoryList == null) return;
                Dictionary<string, string> personHistoryList =
                    JsonConvert.DeserializeObject<Dictionary<string, string>>(item.HistoryList);
                item.Header =
                    personHistoryList.Select(ph => new PersonHeader {Header = ph.Key, Detail = ph.Value})
                        .ToList();
            });
            return lstDna;
        }
    }
}