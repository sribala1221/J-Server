using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ServerAPI.ViewModels;
using GenerateTables.Models;
using ServerAPI.Utilities;
using Microsoft.AspNetCore.Http;

namespace ServerAPI.Services
{
    public class PersonContactService : IPersonContactService
    {
        private readonly AAtims _context;
        private readonly int _personnelId;
        private readonly IPersonService _personService;
        public PersonContactService(AAtims context, ICommonService commonService,
            IHttpContextAccessor httpContextAccessor, IPersonService personService)
        {
            _context = context;
            _personnelId = Convert.ToInt32(httpContextAccessor.HttpContext.User
                .FindFirst(AuthDetailConstants.PERSONNELID)?.Value);
            _personService = personService;
        }

        //get contact details
        public ContactDetails GetContactDetails(int typePersonId)
        {
            ContactDetails contactDet = new ContactDetails
            {
                LstContact = _context.Contact.Where(x => x.TypePersonId == typePersonId)
                    .Select(x => new ContactVm
                    {
                        ContactPersonId = x.PersonId,
                        VictimNotify = x.VictimNotify,
                        VictimNotifyNote = x.VictimNotifyNote,
                        ContactId = x.ContactId,
                        ActiveFlag = x.ContactActiveFlag,
                        RelationshipId = x.ContactRelationship,
                        TypePersonId = x.TypePersonId,
                        PersonLastName = x.Person.PersonLastName,
                        PersonFirstName = x.Person.PersonFirstName,
                        PersonMiddleName = x.Person.PersonMiddleName,
                        PersonCellPhone = x.Person.PersonCellPhone,
                        PersonPhone = x.Person.PersonPhone,
                        PersonBusinessPhone = x.Person.PersonBusinessPhone,
                        ContactDescription = x.ContactDescription,
                        InmateId = x.TypeId,
                        IsActiveInmate = _context.Inmate.Count(i =>
                            x.PersonId.HasValue
                                ? (i.PersonId == x.PersonId && i.InmateActive == 1)
                                : i.InmateActive == 1)
                    }).ToList()
            };
            if (contactDet.LstContact.Count <= 0) return contactDet;
            contactDet.LstContact.ForEach(item =>
            {
                item.RelationShip = _context.Lookup.SingleOrDefault(
                    x => x.LookupType == LookupConstants.RELATIONS &&
                         x.LookupIndex == Convert.ToDouble(item.RelationshipId))?.LookupDescription;

            });
            //get contact attempt grid details by contact id
            List<int> contactIds = contactDet.LstContact.Select(x => x.ContactId).ToList();
            contactDet.LstContactAttempt = _context.ContactAttempt.Where(ca => contactIds.Contains(ca.ContactId))
                .OrderBy(o=>o.ContactId)
                .Select(ca => new ContactAttemptVm
                {
                    AttemptDate = ca.ContactAttemptDate,
                    AttemptTypeLookup = ca.AttemptTypeLookup,
                    AttemptDispoLookup = ca.AttemptDispoLookup,
                    AttemptNotes = ca.ContactAttemptNotes,
                    AttemptId = ca.ContactAttemptId,
                    DeleteFlag = ca.DeleteFlag,
                    AttemptCreateBy = ca.CreateBy,
                    ContactId = ca.ContactId
                }).ToList();
            if (contactDet.LstContactAttempt.Count <= 0) return contactDet;
            {
                List<int> personnelId = contactDet.LstContactAttempt.Select(x => x.AttemptCreateBy).ToList();
                List<PersonnelVm> lstPersonDet = _personService.GetPersonNameList(personnelId);
                PersonnelVm personInfo;
                ContactVm contactInfo;

                contactDet.LstContactAttempt.ForEach(item =>
                {
                    personInfo =
                        lstPersonDet.Single(x =>
                            x.PersonnelId == item.AttemptCreateBy); //get personnel details by personnel id
                    contactInfo =
                        contactDet.LstContact.Single(x =>
                            x.ContactId == item.ContactId); //get contact details from lstContact
                    item.CreateByLastName = personInfo.PersonLastName;
                    item.OfficerBadgeNumber = personInfo.OfficerBadgeNumber;
                    item.PersonLastName = contactInfo.PersonLastName;
                    item.PersonFirstName = contactInfo.PersonFirstName;
                    item.PersonMiddleName = contactInfo.PersonMiddleName;
                    item.VictimNotify = contactInfo.VictimNotify;
                });
            }
            return contactDet;
        }

        //Insert and Update person contact attempt details
        public Task<int> InsertUpdateContactAttempt(ContactAttemptVm contactAttempt)
        {
            ContactAttempt dbContactAttempt =
                _context.ContactAttempt.SingleOrDefault(x => x.ContactAttemptId == contactAttempt.AttemptId);
            if (dbContactAttempt == null)
            {
                dbContactAttempt = new ContactAttempt
                {
                    CreateBy = _personnelId,
                    CreateDate = DateTime.Now
                };
            }
            else
            {
                dbContactAttempt.UpdateBy = _personnelId;
                dbContactAttempt.UpdateDate = DateTime.Now;
            }
            dbContactAttempt.ContactId = contactAttempt.ContactId;
            dbContactAttempt.ContactAttemptDate = contactAttempt.AttemptDate;
            dbContactAttempt.AttemptTypeLookup = contactAttempt.AttemptTypeLookup;
            dbContactAttempt.AttemptDispoLookup = contactAttempt.AttemptDispoLookup;
            dbContactAttempt.ContactAttemptNotes = contactAttempt.AttemptNotes;
            if (dbContactAttempt.ContactAttemptId <= 0)
            {
                _context.ContactAttempt.Add(dbContactAttempt);
            }
            return _context.SaveChangesAsync();
        }

        public Task<int> DeleteUndoContactAttempt(int contactAttemptId)
        {
            ContactAttempt dbContactAttempt =
                _context.ContactAttempt.SingleOrDefault(c => c.ContactAttemptId == contactAttemptId);
            if (dbContactAttempt == null) return _context.SaveChangesAsync();
            dbContactAttempt.DeleteBy = _personnelId;
            dbContactAttempt.DeleteDate = DateTime.Now;
            dbContactAttempt.DeleteFlag = dbContactAttempt.DeleteFlag == 1 ? 0 : 1;
            return _context.SaveChangesAsync();
        }

        public Task<int> InsertUpdateContact(ContactVm contact)
        {
            Contact dbContact = _context.Contact.SingleOrDefault(c => c.ContactId == contact.ContactId);
            int? addressId = _context.Address.OrderByDescending(i => i.AddressId)
                .FirstOrDefault(i => i.PersonId == contact.PersonId && i.AddressType == AddressTypeConstants.RES)
                ?.AddressId;
            if (dbContact == null)
            {
                dbContact = new Contact
                {
                    CreateBy = _personnelId,
                    CreateDate = DateTime.Now
                };
            }
            else
            {
                dbContact.UpdateBy = _personnelId;
                dbContact.UpdateDate = DateTime.Now;
            }
            dbContact.PersonId = contact.ContactPersonId;
            dbContact.TypeId = contact.InmateId;
            dbContact.TypePersonId = contact.TypePersonId;
            dbContact.AddressId = addressId;
            dbContact.ContactRelationship = contact.RelationshipId;
            dbContact.ContactType = contact.ContactType;
            dbContact.ContactDescription = contact.ContactDescription;
            dbContact.ContactActiveFlag = contact.ActiveFlag;
            dbContact.VictimNotify = contact.VictimNotify;
            dbContact.VictimNotifyNote = contact.VictimNotifyNote;
            if (dbContact.ContactId <= 0)
            {
                _context.Contact.Add(dbContact);
            }
            return _context.SaveChangesAsync();
        }

        public Task<int> DeleteUndoContact(ContactVm contactDet)
        {
            Contact dbContact = _context.Contact.SingleOrDefault(c => c.ContactId == contactDet.ContactId);
            if (dbContact != null)
            {
                dbContact.ContactActiveFlag = contactDet.ActiveFlag;
            }
            return _context.SaveChangesAsync();
        }

        public ContactVm GetContactCreateUpdateDetails(int contactId)
        {
            ContactVm dbContact = _context.Contact.Where(c => c.ContactId == contactId)
                .Select(c => new ContactVm
                {
                    CreatedDate = c.CreateDate,
                    UpdatedDate = c.UpdateDate,
                    CreateBy = c.CreateBy,
                    UpdateBy = c.UpdateBy,
                    ContactPersonId = c.PersonId
                }).Single();

            List<int> personnelId =
                new[] {dbContact.CreateBy, dbContact.UpdateBy}.Where(x => x.HasValue)
                    .Select(x => x.Value)
                    .ToList();

            List<PersonnelVm> lstPersonDet = _personService.GetPersonNameList(personnelId);
            dbContact.PersonHistoryVm = new PersonHistoryVm();

            PersonnelVm personInfo = lstPersonDet.Single(p => p.PersonnelId == dbContact.CreateBy);
            dbContact.PersonHistoryVm.CreateByPersonLastName = personInfo.PersonLastName;
            dbContact.PersonHistoryVm.CreateByOfficerBadgeNumber = personInfo.OfficerBadgeNumber;
            if (dbContact.UpdateBy.HasValue)
            {
                personInfo = lstPersonDet.Single(p => p.PersonnelId == dbContact.UpdateBy);
                dbContact.PersonHistoryVm.UpdateByPersonLastName = personInfo.PersonLastName;
                dbContact.PersonHistoryVm.UpdateByOfficerBadgeNumber = personInfo.OfficerBadgeNumber;
            }
            return dbContact;
        }
    }
}
