using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GenerateTables.Models;
using Microsoft.AspNetCore.Http;
using ServerAPI.Utilities;
using ServerAPI.ViewModels;

// ReSharper disable once CheckNamespace
namespace ServerAPI.Services
{
    public class WhosInCustodyService : IWhosInCustodyService
    {
        private readonly AAtims _context;
        private readonly int _personnelId;

        public WhosInCustodyService(AAtims context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _personnelId = Convert.ToInt32(httpContextAccessor.HttpContext.User
                .FindFirst(AuthDetailConstants.PERSONNELID)?.Value);
        }
        
        public List<WhosInCustodyRemoveVm> GetWhosInCustody() 
        { 
            List<WhosInCustodyRemoveVm> lstWhosInCustodyRemove = 
                _context.WhosInCustodyRemove
                    .Select(w => new WhosInCustodyRemoveVm{
                        WhosInCustodyRemoveId = w.WhosInCustodyRemoveId,
                        ConfidentialDisplayFlag = w.ConfidentialDisplayFlag,
                        InmateId = w.InmateId ?? 0,
                        RemoveReason = w.RemovalReason,
                        RemoveNote = w.RemovalNote,
                        CreateDate = w.CreateDate,
                        DeleteFlag = w.DeleteFlag == 1,
                        PersonId = w.Inmate.PersonId,
                        InmateNumber = w.Inmate.InmateNumber
                    }).ToList();
            
            int[] personIds = lstWhosInCustodyRemove.Select(l => l.PersonId).ToArray();
            List<Person> lstPerson = _context.Person.Where(p => personIds.Any(a=> a == p.PersonId))
                .Select(s => new Person{
                    PersonId = s.PersonId,
                    PersonLastName = s.PersonLastName,
                    PersonFirstName = s.PersonFirstName,
                    PersonMiddleName = s.PersonMiddleName,
                    PersonDob = s.PersonDob
                }).ToList();

            lstWhosInCustodyRemove.ForEach(item => {
                Person person = lstPerson.Where(p => p.PersonId == item.PersonId).SingleOrDefault();
                if(person != null)
                {
                    item.PersonLastName = person.PersonLastName;
                    item.PersonFirstName = person.PersonFirstName;
                    item.PersonMiddleName = person.PersonMiddleName;
                    item.PersonDob = person.PersonDob;
                }
            });

            return lstWhosInCustodyRemove;
        }

        public async Task<int> InsertWhosInCustody(WhosInCustodyRemoveVm model)
        {
            WhosInCustodyRemove whosInCustodyRemove = new WhosInCustodyRemove();
            whosInCustodyRemove.InmateId = model.InmateId;
            whosInCustodyRemove.RemovalReason = model.RemoveReason;
            whosInCustodyRemove.ConfidentialDisplayFlag = model.ConfidentialDisplayFlag; 
            whosInCustodyRemove.RemovalNote = model.RemoveNote;
            whosInCustodyRemove.CreateBy = _personnelId;
            whosInCustodyRemove.CreateDate = DateTime.Now;

            _context.Add(whosInCustodyRemove);
            return await _context.SaveChangesAsync();
        }

        public async Task<int> UpdateWhosInCustody(WhosInCustodyRemoveVm model)
        {
            WhosInCustodyRemove whosInCustodyRemove = _context.WhosInCustodyRemove
                .Where(w => w.WhosInCustodyRemoveId == model.WhosInCustodyRemoveId).Single();
            whosInCustodyRemove.RemovalReason = model.RemoveReason;
            whosInCustodyRemove.ConfidentialDisplayFlag = model.ConfidentialDisplayFlag; 
            whosInCustodyRemove.RemovalNote = model.RemoveNote;
            whosInCustodyRemove.UpdateBy = _personnelId;
            whosInCustodyRemove.UpdateDate = DateTime.Now;

            return await _context.SaveChangesAsync();
        }

        public List<WhosInCustodyRemoveVm> DeleteWhosInCustody(WhosInCustodyRemoveVm model)
        {
            DeleteUndoWhosInCustody(model);
            return GetWhosInCustody();
        }

        public List<WhosInCustodyRemoveVm> UndoWhosInCustody(WhosInCustodyRemoveVm model)
        {
            DeleteUndoWhosInCustody(model);
            return GetWhosInCustody();
        }

        private void DeleteUndoWhosInCustody(WhosInCustodyRemoveVm model)
        {
            WhosInCustodyRemove whosInCustodyRemove = _context.WhosInCustodyRemove
                .Where(w => w.WhosInCustodyRemoveId == model.WhosInCustodyRemoveId).Single();
            if(model.DeleteFlag)
            {
                whosInCustodyRemove.DeleteBy = _personnelId;
                whosInCustodyRemove.DeleteDate = DateTime.Now;
            }
            else
            {
                whosInCustodyRemove.DeleteBy = null;
                whosInCustodyRemove.DeleteDate = null;
            }
            whosInCustodyRemove.DeleteFlag = model.DeleteFlag ? 1 : 0;
            _context.SaveChanges();
        }
    }
}