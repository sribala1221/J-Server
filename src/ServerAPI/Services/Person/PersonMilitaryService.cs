using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using ServerAPI.ViewModels;
using GenerateTables.Models;
using ServerAPI.Utilities;
using Newtonsoft.Json;

namespace ServerAPI.Services
{
    public class PersonMilitaryService : IPersonMilitaryService
    {
        private readonly AAtims _context;
        private readonly ICommonService _commonService;
        private readonly int _personnelId;
        private readonly IInterfaceEngineService _interfaceEngine;

        public PersonMilitaryService(AAtims context,
            IHttpContextAccessor httpContextAccessor,
            ICommonService commonService,
            IInterfaceEngineService interfaceEngine)
        {
            _context = context;
            _personnelId = Convert.ToInt32(httpContextAccessor.HttpContext.User
                .FindFirst(AuthDetailConstants.PERSONNELID)?.Value);
            _commonService = commonService;
            _interfaceEngine = interfaceEngine;
        }

        public List<PersonMilitaryVm> GetPersonMilitaryDetails(int personId)
        {
            List<KeyValuePair<int, string>> milBranch = _commonService.GetLookupKeyValuePairs(LookupConstants.MILBRANCH);
            List<PersonMilitaryVm> lstPersonMilitary = _context.PersonMilitary.Where(w => w.PersonId == personId)
                .Select(s => new PersonMilitaryVm
                {
                    PersonMilitaryId = s.PersonMilitaryId,
                    StatusDate = s.StatusDate,
                    ActiveMilitary = s.ActiveMilitary,
                    MilitaryVeteran = s.MilitaryVeteran,
                    BranchId = s.Branch,                    
                    Branch = s.Branch.HasValue ? milBranch.Find(elm => elm.Key == s.Branch).Value : default,
                    Notes = s.Notes,
                    MilitaryStatus = s.MilitaryStatus,
                    MilitaryRank = s.MilitaryRank,
                    MilitaryId = s.MilitaryId,
                    CreateById = s.PersonnelId,
                    CreateByLastName = s.Personnel.PersonNavigation.PersonLastName,
                    CreateByFirstName = s.Personnel.PersonNavigation.PersonFirstName,
                    CreateByMiddleName = s.Personnel.PersonNavigation.PersonMiddleName,
                    CreateByOfficerBadgeNo = s.Personnel.OfficerBadgeNum,
                    CreateDate = s.CreateDate,
                    UpdateById = s.UpdatePersonnelId,
                    UpdateByLastName = s.UpdatePersonnel.PersonNavigation.PersonLastName,
                    UpdateByFirstName = s.UpdatePersonnel.PersonNavigation.PersonFirstName,
                    UpdateByMiddleName = s.UpdatePersonnel.PersonNavigation.PersonMiddleName,
                    UpdateByOfficerBadgeNo = s.UpdatePersonnel.OfficerBadgeNum,
                    UpdateDate = s.UpdateDate,
                    DeleteBy = s.DeleteBy,
                    DeleteDate = s.DeleteDate,
                    DeleteFlag = s.DeleteFlag    
                }).ToList();

            return lstPersonMilitary;
        }

        public Task<int> InsertUpdatePersonMilitary(PersonMilitaryVm personMilitary)
        {
            PersonMilitary dbPersonMilitary = _context.PersonMilitary
                    .SingleOrDefault(s => s.PersonMilitaryId == personMilitary.PersonMilitaryId) ??
                new PersonMilitary
                {
                    PersonnelId = _personnelId,
                    CreateDate = DateTime.Now
                };

            dbPersonMilitary.PersonId = personMilitary.PersonId;
            dbPersonMilitary.StatusDate = personMilitary.StatusDate;
            dbPersonMilitary.ActiveMilitary = personMilitary.ActiveMilitary;
            dbPersonMilitary.MilitaryVeteran = personMilitary.MilitaryVeteran;
            dbPersonMilitary.Branch = personMilitary.BranchId;
            dbPersonMilitary.Notes = personMilitary.Notes;
            dbPersonMilitary.UpdateDate = DateTime.Now;
            dbPersonMilitary.UpdatePersonnelId = _personnelId;
            dbPersonMilitary.MilitaryStatus = personMilitary.MilitaryStatus;
            dbPersonMilitary.MilitaryRank = personMilitary.MilitaryRank;
            dbPersonMilitary.MilitaryId = personMilitary.MilitaryId;

            if (dbPersonMilitary.PersonMilitaryId <= 0)
            {
                _context.PersonMilitary.Add(dbPersonMilitary);
                _context.SaveChanges();
                _interfaceEngine.Export(new ExportRequestVm
                {
                    EventName = EventNameConstants.MILITARYSAVE,
                    PersonnelId = _personnelId,
                    Param1 = personMilitary.PersonId.ToString(),
                    Param2 = dbPersonMilitary.PersonMilitaryId.ToString()
                });
            }

            InsertPersonMilitaryHistory(dbPersonMilitary.PersonMilitaryId, personMilitary.PersonMilitaryHistoryList);

            return _context.SaveChangesAsync();
        }

        private void InsertPersonMilitaryHistory(int personMilitaryId, string personMilitaryHistoryList)
        {
            PersonMilitaryHistory dbPersonMilitaryHis = new PersonMilitaryHistory
            {
                PersonMilitaryId = personMilitaryId,
                PersonnelId = _personnelId,
                CreateDate = DateTime.Now,
                PersonMilitaryHistoryList = personMilitaryHistoryList
            };
            _context.PersonMilitaryHistory.Add(dbPersonMilitaryHis);
        }
        
        public Task<int> DeletePersonMilitary(PersonMilitaryVm value)
        {
           PersonMilitary dbPersonMilitary = _context.PersonMilitary.Find(value.PersonMilitaryId);
           dbPersonMilitary.DeleteBy = !value.DeleteFlag?_personnelId : default(int?);
           dbPersonMilitary.DeleteFlag = !value.DeleteFlag;
           dbPersonMilitary.DeleteDate = !value.DeleteFlag ? DateTime.Now:default(DateTime?);

            return _context.SaveChangesAsync();
        }

        public List<HistoryVm> GetMilitaryHistoryDetails(int militaryId)
        {
            List<HistoryVm> lstMilitary = _context.PersonMilitaryHistory.Where(w => w.PersonMilitaryId == militaryId)
                .OrderByDescending(o => o.CreateDate)
                .Select(s => new HistoryVm
                {
                    HistoryId = s.PersonMilitaryHistoryId,
                    CreateDate = s.CreateDate,
                    PersonId = s.Personnel.PersonId,
                    OfficerBadgeNumber = s.Personnel.OfficerBadgeNum,
                    HistoryList = s.PersonMilitaryHistoryList
                }).ToList();

            if (lstMilitary.Count == 0) return lstMilitary;

            //To Improve Performance All Person Details Loaded By Single Hit Before Looping
            int[] personIds = lstMilitary.Select(s => s.PersonId).ToArray();

            //Getting person list
            List<Person> lstPersonDetails = _context.Person.Where(w => personIds.Contains(w.PersonId)).ToList();

            lstMilitary.ForEach(item =>
            {
                item.PersonLastName =
                    lstPersonDetails.SingleOrDefault(s => s.PersonId == item.PersonId)?.PersonLastName;

                //To get Json result into Dictionary
                if (item.HistoryList == null) return;
                Dictionary<string, string> personHistoryList =
                    JsonConvert.DeserializeObject<Dictionary<string, string>>(item.HistoryList);
                item.Header =
                    personHistoryList.Select(ph => new PersonHeader {Header = ph.Key, Detail = ph.Value}).ToList();
            });

            return lstMilitary;
        }
    }
}
