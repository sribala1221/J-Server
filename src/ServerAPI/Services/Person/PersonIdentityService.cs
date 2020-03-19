﻿using System;
using System.Collections.Generic;
using System.Linq;
using GenerateTables.Models;
using ServerAPI.ViewModels;
using ServerAPI.Utilities;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Http;

namespace ServerAPI.Services
{
    public class PersonIdentityService : IPersonIdentityService
    {
        private readonly AAtims _context;
        private readonly IPrebookActiveService _iPrebookActiveService;
        private readonly int _personnelId;
        private readonly IPersonAkaService _akaService;
        private readonly IPersonService _personService;
        private readonly IInterfaceEngineService _interfaceEngine;
        private PersonDetail PersonDetail { get; set; }
        private PersonCitizenshipDetail PersonCitizenshipDetail { get; set; }
        private PersonIdentity PersonIdentity { get; set; }
        private PersonVm PersonVm { get; set; }

        public PersonIdentityService(AAtims context,
            IHttpContextAccessor httpContextAccessor,
            IPrebookActiveService iPrebookActiveService
            , IPersonAkaService akaService,
            IPersonService personService,
            IInterfaceEngineService interfaceEngine)
        {
            _context = context;
            _personnelId = Convert.ToInt32(httpContextAccessor.HttpContext.User
                .FindFirst(AuthDetailConstants.PERSONNELID)?.Value);
            _iPrebookActiveService = iPrebookActiveService;
            _akaService = akaService;
            _personService = personService;
            _interfaceEngine = interfaceEngine;
        }

        private void GetPerson(int personId) {
            PersonVm = _context.Person.Where(p => p.PersonId == personId).Select(p => new PersonVm
            {
                PersonFirstName = p.PersonFirstName,
                PersonLastName = p.PersonLastName,
                PersonMiddleName = p.PersonMiddleName,
                PersonSuffix = p.PersonSuffix,
                PersonDob = p.PersonDob,
                PersonPlaceOfBirth = p.PersonPlaceOfBirth,
                PersonPlaceOfBirthList = p.PersonPlaceOfBirthList,
                PersonIllegalAlien = p.IllegalAlienFlag,
                PersonUsCitizen = p.UsCitizenFlag,
                PersonCitizenship = p.CitizenshipString,
                PersonSsn = p.PersonSsn,
                PersonDlNumber = p.PersonDlNumber,
                PersonDlState = p.PersonDlState,
                PersonDlClass = p.PersonDlClass,
                PersonCii = p.PersonCii,
                PersonFbiNo = p.PersonFbiNo,
                PersonDoc = p.PersonDoc,
                PersonAlienNo = p.PersonAlienNo,
                AfisNumber = p.AfisNumber,
                PersonPhone = p.PersonPhone,
                PersonCellPhone = p.PersonCellPhone,
                PersonBusinessPhone = p.PersonBusinessPhone,
                PersonBusinessFax = p.PersonBusinessFax,
                PersonPhone2 = p.PersonPhone2,
                PersonEmail = p.PersonEmail,
                PersonMaidenName = p.PersonMaidenName,
                PersonOtherIdType = p.PersonOtherIdType,
                PersonOtherIdNumber = p.PersonOtherIdNumber,
                PersonOtherIdState = p.PersonOtherIdState,
                CreateBy = p.CreateBy,
                UpdateBy = p.UpdateBy,
                CreatedDate = p.CreateDate,
                UpdatedDate = p.UpdateDate,
                PersonId = p.PersonId,
                PersonSexLast = p.PersonSexLast
            }).Single();
        }

        public PersonIdentity GetPersonDetails(int personId)
        {
            //Get person details
            PersonIdentity = GetPersonIdentity(personId);
            LoadPersonnelDetails();
            return PersonIdentity;
        }

        public PersonIdentity GetPersonIdentity(int personId)
        {
            if (personId > 0)
            {
                string inmateNumber = _context.Inmate.SingleOrDefault(s => s.PersonId == personId)?.InmateNumber;

                PersonIdentity personIdentity = _context.Person.Where(p => p.PersonId == personId)
                    .Select(p => new PersonIdentity
                    {
                        PersonFirstName = p.PersonFirstName,
                        PersonLastName = p.PersonLastName,
                        PersonMiddleName = p.PersonMiddleName,
                        PersonSuffix = p.PersonSuffix,
                        PersonDob = p.PersonDob,
                        PersonPlaceOfBirth = p.PersonPlaceOfBirth,
                        PersonPlaceOfBirthList = p.PersonPlaceOfBirthList,
                        PersonIllegalAlien = p.IllegalAlienFlag,
                        PersonUsCitizen = p.UsCitizenFlag,
                        PersonCitizenship = p.CitizenshipString,
                        PersonSsn = p.PersonSsn,
                        PersonDlNumber = p.PersonDlNumber,
                        PersonDlState = p.PersonDlState,
                        PersonDlClass = p.PersonDlClass,
                        PersonCii = p.PersonCii,
                        PersonFbiNo = p.PersonFbiNo,
                        PersonDoc = p.PersonDoc,
                        PersonAlienNo = p.PersonAlienNo,
                        AfisNumber = p.AfisNumber,
                        PersonPhone = p.PersonPhone,
                        PersonCellPhone = p.PersonCellPhone,
                        PersonBusinessPhone = p.PersonBusinessPhone,
                        PersonBusinessFax = p.PersonBusinessFax,
                        PersonPhone2 = p.PersonPhone2,
                        PersonEmail = p.PersonEmail,
                        PersonMaidenName = p.PersonMaidenName,
                        PersonOtherIdType = p.PersonOtherIdType,
                        PersonOtherIdNumber = p.PersonOtherIdNumber,
                        PersonOtherIdState = p.PersonOtherIdState,
                        PersonOtherIdDescription = p.PersonOtherIdDescription,
                        CreateBy = p.CreateBy,
                        UpdateBy = p.UpdateBy,
                        CreatedDate = p.CreateDate,
                        UpdatedDate = p.UpdateDate,
                        InmateNumber = inmateNumber,
                        PersonId = p.PersonId,
                        PersonSexLast = p.PersonSexLast
                    }).Single();
                return personIdentity;
            }
            else
            {
                PersonIdentity personIdentity = new PersonIdentity();
                return personIdentity;
            }
        }

        private void LoadPersonnelDetails()
        {
            //assign createby and updateby into a list of int     
            List<int> personnelId =
                new[] { PersonIdentity.CreateBy, PersonIdentity.UpdateBy }.Where(x => x.HasValue)
                    .Select(x => x.Value)
                    .ToList();
            // get the personnel details 
            List<PersonnelVm> lstPersonDet = _personService.GetPersonNameList(personnelId);

            PersonnelVm personInfo;

            //assign list of personnel details into appropriate fields
            if (PersonIdentity.CreateBy.HasValue)
            {
                personInfo = lstPersonDet.SingleOrDefault(p => p.PersonnelId == PersonIdentity.CreateBy);
                PersonIdentity.CreateByPersonLastName = personInfo?.PersonLastName;
                PersonIdentity.CreateByOfficerBadgeNumber = personInfo?.OfficerBadgeNumber;
            }
            if (!PersonIdentity.UpdateBy.HasValue) return;
            personInfo = lstPersonDet.SingleOrDefault(p => p.PersonnelId == PersonIdentity.UpdateBy);
            PersonIdentity.UpdateByPersonLastName = personInfo?.PersonLastName;
            PersonIdentity.UpdateByOfficerBadgeNumber = personInfo?.OfficerBadgeNumber;
        }

        //To Get All Details For Indentity Name Popup
        public PersonDetail GetNamePopupDetails(int personId, int inmateId)
        {
            LoadPerson(personId);
            PersonDetail.InmateId = inmateId;
            LoadInmateIncarceration(inmateId);
            LoadAkaList(personId);
            return PersonDetail;
        }

        //To Get Person Details By Person Id
        private void LoadPerson(int personId)
        {
            PersonDetail = _context.Person.Where(p => p.PersonId == personId)
                .Select(p => new PersonDetail
                {
                    PersonId = p.PersonId,
                    PersonLastName = p.PersonLastName,
                    PersonFirstName = p.PersonFirstName,
                    PersonSuffix = p.PersonSuffix,
                    PersonMiddleName = p.PersonMiddleName,
                    FknLastName = p.FknLastName,
                    FknFirstName = p.FknFirstName,
                    FknSuffixName = p.FknSuffixName,
                    FknMiddleName = p.FknMiddleName,
                    PersonDob = p.PersonDob
                }).Single();
        }

        //To Get Incarceration Details By Inmate Id
        private void LoadInmateIncarceration(int inmateId)
        {
            PersonDetail.PersonIncarceration = _context.Incarceration.Where(i => i.InmateId == inmateId)
                .OrderByDescending(i => i.IncarcerationId)
                .Select(i => new PersonIncarceration
                {
                    IncarcerationId = i.IncarcerationId,
                    PersonFirstName = i.UsedPersonFrist,
                    PersonLastName = i.UsedPersonLast,
                    PersonMiddleName = i.UsedPersonMiddle,
                    PersonSuffix = i.UsedPersonSuffix,
                    DateIn = i.DateIn,
                    ReleaseDate = i.ReleaseOut
                }).ToList();
        }

        //To Get Aka List By Person Id
        private void LoadAkaList(int personId)
        {
            PersonDetail.Aka = _context.Aka.Where(a => a.PersonId == personId)
                .GroupBy(g => new { g.AkaLastName, g.AkaFirstName, g.AkaMiddleName, g.AkaSuffix })
                .Select(a => new AkaVm
                {
                    AkaFirstName = a.Key.AkaFirstName,
                    AkaLastName = a.Key.AkaLastName,
                    AkaMiddleName = a.Key.AkaMiddleName,
                    AkaSuffix = a.Key.AkaSuffix,
                    CreateDate = a.Select(se => se.CreateDate).FirstOrDefault(),
                    PersonGangName = a.Select(se => se.PersonGangName).FirstOrDefault()
                }).OrderByDescending(a => a.CreateDate).ToList();
        }

        public async Task<int> InsertNamePopupDetails(PersonDetail personDetail)
        {
            InsertCurrentNameDetails(personDetail);
            InsertPersonIncarcerationDetails(personDetail);
            return await _context.SaveChangesAsync();
        }

        private static bool CheckPersonDetails(PersonDetail dbPersonDetail, PersonDetail personDetail) =>
            (dbPersonDetail.PersonFirstName != personDetail.PersonFirstName ||
            dbPersonDetail.PersonLastName != personDetail.PersonLastName ||
            dbPersonDetail.PersonMiddleName != personDetail.PersonMiddleName ||
            dbPersonDetail.PersonSuffix != personDetail.PersonSuffix ||
            dbPersonDetail.PersonInlineEditStatus != PersonInlineEditStatus.PersonDetail) &&
           (dbPersonDetail.FknFirstName != personDetail.FknFirstName ||
            dbPersonDetail.FknLastName != personDetail.FknLastName ||
            dbPersonDetail.FknMiddleName != personDetail.FknMiddleName ||
            dbPersonDetail.FknSuffixName != personDetail.FknSuffixName ||
            dbPersonDetail.PersonInlineEditStatus != PersonInlineEditStatus.PersonFknDetail);

        //   To Save Current Name Details From Name Popup Inline Edit
        private void InsertCurrentNameDetails(PersonDetail perDetail)
        {
            LoadPerson(perDetail.PersonId);
            //Check Edited Person Values
            bool count = CheckAkaName(perDetail.PersonId, perDetail.PersonFirstName, perDetail.PersonLastName,
                perDetail.PersonMiddleName, perDetail.PersonSuffix);

            if (!count)
            {
                //Check Person Table Values (database values)
                count = CheckAkaName(PersonDetail.PersonId, PersonDetail.PersonFirstName,
                    PersonDetail.PersonLastName, PersonDetail.PersonMiddleName, PersonDetail.PersonSuffix);
                if (!count)
                {
                    PersonDetail.PersonInlineEditStatus = PersonInlineEditStatus.PersonDetail;
                    if (CheckPersonDetails(PersonDetail, perDetail))
                    {
                        LoadInsertAka(PersonDetail);
                    }

                }
                perDetail.PersonAkaHistoryList = perDetail.AkaCurrentNameHistoryList;
                perDetail.PersonInlineEditStatus = PersonInlineEditStatus.PersonCurrentName;
                LoadInsertAka(perDetail);
                _interfaceEngine.Export(new ExportRequestVm
                {
                    EventName = EventNameConstants.CURRENTNAMESAVE,
                    PersonnelId = _personnelId,
                    Param1 = perDetail.PersonId.ToString(),
                    Param2 = perDetail.PersonId.ToString()
                });
            }

            //Check Edited First Known Person Details
            bool akaCount = CheckAkaName(perDetail.PersonId, perDetail.FknFirstName, perDetail.FknLastName,
                perDetail.FknMiddleName, perDetail.FknSuffixName);
            if (!akaCount)
            {
                //Check Person Table First Known Details
                akaCount = CheckAkaName(PersonDetail.PersonId, PersonDetail.FknFirstName, PersonDetail.FknLastName,
                    PersonDetail.FknMiddleName, PersonDetail.FknSuffixName);
                if (!akaCount)
                {
                    PersonDetail.PersonInlineEditStatus = PersonInlineEditStatus.PersonFknDetail;
                    if (CheckPersonDetails(PersonDetail, perDetail))
                    {
                        LoadInsertAka(PersonDetail);
                    }
                }
                perDetail.PersonAkaHistoryList = perDetail.AkaFknHistoryList;
                perDetail.PersonInlineEditStatus = PersonInlineEditStatus.PersonFknName;
                LoadInsertAka(perDetail);

            }

            //To Update Edited Person Values Into Person Table
            Person dbPerson = _context.Person.SingleOrDefault(p => p.PersonId == perDetail.PersonId);
            if (dbPerson != null)
            {
                dbPerson.PersonLastName = perDetail.PersonLastName;
                dbPerson.PersonFirstName = perDetail.PersonFirstName;
                dbPerson.PersonMiddleName = perDetail.PersonMiddleName;
                dbPerson.PersonSuffix = perDetail.PersonSuffix;
                dbPerson.FknLastName = perDetail.FknLastName;
                dbPerson.FknFirstName = perDetail.FknFirstName;
                dbPerson.FknMiddleName = perDetail.FknMiddleName;
                dbPerson.FknSuffixName = perDetail.FknSuffixName;
                _interfaceEngine.Export(new ExportRequestVm
                {
                    EventName = EventNameConstants.FIRSTKNOWNNAMESAVE,
                    PersonnelId = _personnelId,
                    Param1 = perDetail.PersonId.ToString(),
                    Param2 = perDetail.PersonId.ToString()
                });
            }
            LoadInsertPersonHistory(perDetail.PersonId, perDetail.AkaCurrentNameHistoryList);
            LoadInsertPersonHistory(perDetail.PersonId, perDetail.AkaFknHistoryList);
        }

        // To Save Incarceration Details From Name Popup Inline Edit
        private void InsertPersonIncarcerationDetails(PersonDetail perDetail)
        {
            if (perDetail.PersonIncarceration.Count > 0)
            {
                perDetail.PersonIncarceration.ForEach(item =>
                {
                    //Check Edited Incarceration Details
                    bool count = CheckAkaName(perDetail.PersonId, item.PersonFirstName, item.PersonLastName,
                        item.PersonMiddleName, item.PersonSuffix);
                    if (!count)
                    {
                        perDetail.PersonAkaHistoryList = item.IncarcerationHistoryList;
                        perDetail.PersonInlineEditStatus = PersonInlineEditStatus.PresentIncarceration;
                        perDetail.IncarcerationDetail = new PersonIncarceration
                        {
                            PersonLastName = item.PersonLastName,
                            PersonFirstName = item.PersonFirstName,
                            PersonMiddleName = item.PersonMiddleName,
                            PersonSuffix = item.PersonSuffix
                        };
                        LoadInsertAka(perDetail);
                    }
                    //To Update Used Person Details Into Incarceration Table             
                    Incarceration dbIncDetail =
                        _context.Incarceration.SingleOrDefault(
                            ic => ic.IncarcerationId == item.IncarcerationId);
                    if (dbIncDetail != null)
                    {
                        dbIncDetail.UsedPersonFrist = item.PersonFirstName;
                        dbIncDetail.UsedPersonLast = item.PersonLastName;
                        dbIncDetail.UsedPersonMiddle = item.PersonMiddleName;
                        dbIncDetail.UsedPersonSuffix = item.PersonSuffix;
                        _interfaceEngine.Export(new ExportRequestVm
                        {
                            EventName = EventNameConstants.INCARCERATIONNAMESAVE,
                            PersonnelId = _personnelId,
                            Param1 = perDetail.PersonId.ToString(),
                            Param2 = dbIncDetail.IncarcerationId.ToString()
                        });
                    }
                    LoadInsertPersonHistory(perDetail.PersonId, item.IncarcerationHistoryList);
                    _context.SaveChanges();
                });
            }
        }

        //Check Given Values Already In Aka Or Not
        private bool CheckAkaName(int? personId, string firstName, string lastName, string middleName, string suffix) =>
            _context.Aka.Any(a => a.AkaFirstName == firstName && a.AkaLastName == lastName &&
            (a.AkaMiddleName == null || a.AkaMiddleName == string.Empty ? null : a.AkaMiddleName) ==
            (middleName == null || middleName == string.Empty ? null : middleName) &&
            (a.AkaSuffix == null || a.AkaSuffix == string.Empty ? null : a.AkaSuffix) ==
            (suffix == null || suffix == string.Empty ? null : suffix) && a.PersonId == personId);

        //Insert Aka Details
        private void LoadInsertAka(PersonDetail perDetail)
        {
            Aka dbAkaDetail = new Aka
            {
                CreateDate = DateTime.Now,
                PersonId = perDetail.PersonId,
                CreatedBy = _personnelId
            };
            if (PersonInlineEditStatus.PersonDetail == perDetail.PersonInlineEditStatus ||
                PersonInlineEditStatus.PersonCurrentName == perDetail.PersonInlineEditStatus)
            {
                dbAkaDetail.AkaLastName = perDetail.PersonLastName;
                dbAkaDetail.AkaFirstName = perDetail.PersonFirstName;
                dbAkaDetail.AkaMiddleName = perDetail.PersonMiddleName;
                dbAkaDetail.AkaSuffix = perDetail.PersonSuffix;
            }
            else if (PersonInlineEditStatus.PersonFknDetail == perDetail.PersonInlineEditStatus ||
                     PersonInlineEditStatus.PersonFknName == perDetail.PersonInlineEditStatus)
            {
                dbAkaDetail.AkaLastName = perDetail.FknLastName;
                dbAkaDetail.AkaFirstName = perDetail.FknFirstName;
                dbAkaDetail.AkaMiddleName = perDetail.FknMiddleName;
                dbAkaDetail.AkaSuffix = perDetail.FknSuffixName;
            }
            else
            {
                dbAkaDetail.AkaLastName = perDetail.IncarcerationDetail.PersonLastName;
                dbAkaDetail.AkaFirstName = perDetail.IncarcerationDetail.PersonFirstName;
                dbAkaDetail.AkaMiddleName = perDetail.IncarcerationDetail.PersonMiddleName;
                dbAkaDetail.AkaSuffix = perDetail.IncarcerationDetail.PersonSuffix;
            }
            _context.Aka.Add(dbAkaDetail);

            if (PersonInlineEditStatus.PersonDetail == perDetail.PersonInlineEditStatus ||
                PersonInlineEditStatus.PersonFknDetail == perDetail.PersonInlineEditStatus) return;
            //To Save Aka History 
            AkaHistory dbAkaHistoryDetail = new AkaHistory
            {
                CreateDate = DateTime.Now,
                AkaId = dbAkaDetail.AkaId,
                PersonnelId = _personnelId,
                AkaHistoryList = perDetail.PersonAkaHistoryList
            };
            _context.AkaHistory.Add(dbAkaHistoryDetail);
        }

        //Get Person All Saved History
        public List<PersonHistoryVm> GetPersonSavedHistory(int personId)
        {
            List<PersonHistoryVm> lstPersonHistory = _context.PersonHistory.Where(ph => ph.PersonId == personId)
                .OrderByDescending(ph => ph.CreateDate)
                .Select(ph => new PersonHistoryVm
                {
                    PersonHistoryId = ph.PersonHistoryId,
                    CreateDate = ph.CreateDate,
                    PersonId = ph.Personnel.PersonId,
                    OfficerBadgeNumber = ph.Personnel.OfficerBadgeNum,
                    PersonHistoryList = ph.PersonHistoryList
                }).ToList();

            if (lstPersonHistory.Count <= 0) return lstPersonHistory;
            //For Improve Performance All Person Details Loaded By Single Hit Before Looping
            int[] personIds = lstPersonHistory.Select(s => s.PersonId).ToArray();
            //get person list
            List<Person> lstPersonDet = _context.Person.Where(per => personIds.Contains(per.PersonId)).ToList();

            lstPersonHistory.ForEach(item =>
            {
                item.PersonLastName = lstPersonDet.SingleOrDefault(p => p.PersonId == item.PersonId)?.PersonLastName;
                //To GetJson Result Into Dictionary
                if (string.IsNullOrEmpty(item.PersonHistoryList)) return;
                Dictionary<string, string> personHistoryList =
                    JsonConvert.DeserializeObject<Dictionary<string, string>>(item.PersonHistoryList);
                item.Header = personHistoryList.Select(ph => new PersonHeader { Header = ph.Key, Detail = ph.Value })
                        .ToList();
                if (item.Header.Count > 0)
                {
                    //Person history table used in identity and profile pages so FromPage column was added to differenciate from both.
                    item.FromPage = item.Header[0].Header == PersonConstants.FROMPAGE ? item.Header[0].Detail : "";
                }
            });
            return lstPersonHistory;
        }

        //To Get Citizenship List By Person Id
        public PersonCitizenshipDetail GetCitizenshipList(int personId)
        {
            PersonCitizenshipDetail = new PersonCitizenshipDetail
            {
                lstPersonCitizenship = _context.PersonCitizenship.Where(c => c.PersonId == personId)
                    .Select(c => new PersonCitizenshipVm
                    {
                        PersonCitizenshipId = c.PersonCitizenshipId,
                        CitizenshipCountry = c.CitizenshipCountry,
                        CitizenshipStatus = c.CitizenshipStatus,
                        CitizenshipNote = c.CitizenshipNote,
                        DeleteFlag = c.DeleteFlag,
                        PassportNumber = c.PassportNumber,
                        NotificationAcknowledgement = c.NotificationAcknowledgement,
                        NotificationAutomateFlag = c.NotificationAutomateFlag
                    }).ToList()
            };

            if (PersonCitizenshipDetail.lstPersonCitizenship.Count > 0)
            {
                //TODO something is wrong here
                long? perDescriptionLanguage =
                    _context.PersonDescription.OrderByDescending(x => x.PersonDescriptionId)
                        .FirstOrDefault(x => x.PersonId == personId)?.PersonPrimaryLanguage;
                string personLanguage = _context.Lookup.FirstOrDefault(y =>
                            y.LookupIndex == perDescriptionLanguage &&
                            y.LookupType == LookupConstants.LANGUAGE)?.LookupDescription;
                //Add all citizenship primary language
                PersonCitizenshipDetail.lstPersonCitizenship.Select(y =>
                {
                    y.PrimaryLanguage = (int?)(perDescriptionLanguage ?? 0);
                    y.languageDescription = personLanguage;
                    return y;
                }).ToList();
                PersonCitizenshipDetail.lstPersonCitizenship = PersonCitizenshipDetail.lstPersonCitizenship;
            }

            //Get consular notification information for all countries
            PersonCitizenshipDetail.lstCitizenshipCountryDetails = _context.ConsularNotifyLookup
                .Select(x => new CitizenshipCountryDetails
                {
                    CitizenshipCountry = x.CitizenshipCountry,
                    NotificationRequired = x.NotificationRequired,
                    NotificationOptional = x.NotificationOptional,
                    ConsulateName = x.ConsulateName,
                    ConsulateAddress = x.ConsulateAddress,
                    ConsulateCity = x.ConsulateCity,
                    ConsulateState = x.ConsulateState,
                    ConsulateZip = x.ConsulateZip,
                    ConsulatePhone = x.ConsulatePhone,
                    ConsulateFax = x.ConsulateFax,
                    ConsulateEmail = x.ConsulateEmail,
                    ConsulateContact = x.ConsulateContact,
                    ConsulateInstructions = x.ConsulateInstructions,
                    AutomateFlag = x.AutomateFlag,
                    AutomateEmail = x.AutomateEmail
                }).ToList();

            return PersonCitizenshipDetail;
        }

        //To Get Citizenship History Details
        public List<PersonHistoryVm> GetPersonnelCitizenshipHistory(int personId, int personCitizenshipId)
        {
            //get person citizenship history list
            List<PersonHistoryVm> lstPersonCitiHis = _context.PersonCitizenshipHistory
                .Where(ph => ph.PersonCitizenshipId == personCitizenshipId)
                .Select(ph => new PersonHistoryVm
                {
                    PersonCitizenshipHistoryId = ph.PersonCitizenshipHistoryId,
                    CreateDate = ph.CreateDate,
                    DeleteDate = ph.DeleteDate,
                    UpdateDate = ph.UpdateDate,
                    CreateBy = ph.CreateBy,
                    DeleteBy = ph.DeleteBy,
                    UpdateBy = ph.UpdateBy
                }).OrderByDescending(ph => ph.CreateDate).ToList();

            if (lstPersonCitiHis.Count <= 0) return lstPersonCitiHis;
            //To Improve Performance All Person Details Loaded Before Looping by CreateBy,DeleteBy,UpdateBy Values
            //SelectMany Is Used To Convert Multiple Column List Values Into Single Column List
            List<int> personnelId = lstPersonCitiHis.Select(i => new[] { i.CreateBy, i.DeleteBy, i.UpdateBy })
                    .SelectMany(i => i)
                    .Where(i => i.HasValue)
                    .Select(i => i.Value)
                    .ToList();
            List<PersonnelVm> lstPersonDet = _personService.GetPersonNameList(personnelId);
            long? perDescriptionLanguage =
                              _context.PersonDescription.OrderByDescending(x => x.PersonDescriptionId)
                                  .FirstOrDefault(x => x.PersonId == personId)?.PersonPrimaryLanguage;
            string personLanguage = _context.Lookup.FirstOrDefault(y =>
                        y.LookupIndex == perDescriptionLanguage &&
                        y.LookupType == LookupConstants.LANGUAGE)?.LookupDescription;
            lstPersonCitiHis.ForEach(item =>
            {
                PersonnelVm personInfo = lstPersonDet.SingleOrDefault(p => p.PersonnelId == item.CreateBy);
                item.CreateByPersonLastName = personInfo?.PersonLastName;
                item.CreateByOfficerBadgeNumber = personInfo?.OfficerBadgeNumber;

                if (item.DeleteBy.HasValue)
                {
                    personInfo = lstPersonDet.SingleOrDefault(p => p.PersonnelId == item.DeleteBy);
                    item.DeleteByPersonLastName = personInfo?.PersonLastName;
                    item.DeleteByOfficerBadgeNumber = personInfo?.OfficerBadgeNumber;
                }
                if (item.UpdateBy.HasValue)
                {
                    personInfo = lstPersonDet.SingleOrDefault(p => p.PersonnelId == item.UpdateBy);
                    item.UpdateByPersonLastName = personInfo?.PersonLastName;
                    item.UpdateByOfficerBadgeNumber = personInfo?.OfficerBadgeNumber;
                }
                item.Citizenship = _context.PersonCitizenshipHistory
                    .Where(cs => cs.PersonCitizenshipHistoryId == item.PersonCitizenshipHistoryId)
                    .Select(cs => new PersonCitizenshipVm
                    {
                        DeleteDate = cs.DeleteDate,
                        CreateDate = cs.CreateDate,
                        UpdateDate = cs.UpdateDate,
                        DeleteFlag = cs.DeleteFlag,
                        CitizenshipCountry = cs.CitizenshipCountry,
                        CitizenshipStatus = cs.CitizenshipStatus,
                        CitizenshipNote = cs.CitizenshipNote,
                        PassportNumber = cs.PassportNumber,
                        PrimaryLanguage = (int?)(perDescriptionLanguage ?? 0),
                        languageDescription = personLanguage,
                    }).SingleOrDefault();
            });
            return lstPersonCitiHis;
        }

        //Need Clarification For Single Method Insert And Update
        public async Task<PersonCitizenshipDetail> InsertUpdatePersonCitizenship(PersonCitizenshipVm citizenship)
        {
            PersonCitizenship dbPerCs = _context.PersonCitizenship.SingleOrDefault(pcs =>
                pcs.PersonCitizenshipId == citizenship.PersonCitizenshipId);
            int citizenshipCount = _context.PersonCitizenship.Count(p => p.PersonId == citizenship.PersonId &&
                p.CitizenshipCountry == citizenship.CitizenshipCountry && !p.DeleteFlag);

            if (dbPerCs is null)
            {
                if (citizenshipCount == 0)
                {
                    if (citizenship.IsFromUsCitizen)
                    {
                        dbPerCs = new PersonCitizenship
                        {
                            CreateDate = DateTime.Now,
                            PersonId = citizenship.PersonId,
                            CreateBy = _personnelId,
                            CitizenshipCountry = citizenship.CitizenshipCountry
                        };
                    }
                    else
                    {
                        dbPerCs = new PersonCitizenship
                        {
                            CreateDate = DateTime.Now,
                            PersonId = citizenship.PersonId,
                            CreateBy = _personnelId,
                            CitizenshipCountry = citizenship.CitizenshipCountry,
                            CitizenshipStatus = citizenship.CitizenshipStatus,
                            CitizenshipNote = citizenship.CitizenshipNote,
                            PassportNumber = citizenship.PassportNumber,
                            NotificationAcknowledgement = citizenship.NotificationAcknowledgement,
                            NotificationAutomateFlag = citizenship.NotificationAutomateFlag
                        };
                    }
                    _context.PersonCitizenship.Add(dbPerCs);
                    citizenship.PersonCitizenshipId = dbPerCs.PersonCitizenshipId;
                    LoadInsertPerCitizenshipHis(citizenship);
                    await _context.SaveChangesAsync();
                    UpdateCitizenshipString(citizenship.PersonId, citizenship.IsFromUsCitizen);
                }
            }
            else
            {
                if (citizenshipCount == 0 ||
                    dbPerCs.CitizenshipCountry == citizenship.CitizenshipCountry && citizenshipCount != 0)
                {
                    dbPerCs.UpdateDate = DateTime.Now;
                    dbPerCs.UpdateBy = _personnelId;
                    dbPerCs.CitizenshipCountry = citizenship.CitizenshipCountry;
                    dbPerCs.CitizenshipStatus = citizenship.CitizenshipStatus;
                    dbPerCs.CitizenshipNote = citizenship.CitizenshipNote;
                    dbPerCs.PassportNumber = citizenship.PassportNumber;
                    dbPerCs.NotificationAcknowledgement = citizenship.NotificationAcknowledgement;
                    dbPerCs.NotificationAutomateFlag = citizenship.NotificationAutomateFlag;
                    LoadInsertPerCitizenshipHis(citizenship);
                    await _context.SaveChangesAsync();
                    UpdateCitizenshipString(citizenship.PersonId, citizenship.IsFromUsCitizen);
                }
            }
            if (!citizenship.IsFromUsCitizen)
            {
                LoadPersonPrimaryLanguage(citizenship);
            }
            GetCitizenshipList(citizenship.PersonId);
            return PersonCitizenshipDetail;
        }

        //Insert and update the person primary language in person description
        private void LoadPersonPrimaryLanguage(PersonCitizenshipVm personCitizenship)
        {
            PersonDescription dbPersonDesc = _context.PersonDescription.OrderByDescending(p => p.PersonDescriptionId)
                    .FirstOrDefault(p => p.PersonId == personCitizenship.PersonId);
            if (dbPersonDesc != null)
            {
                dbPersonDesc.PersonPrimaryLanguage = personCitizenship.PrimaryLanguage;
                dbPersonDesc.UpdatedBy = _personnelId;
                dbPersonDesc.UpdateDate = DateTime.Now;
            }
            else
            {
                dbPersonDesc = new PersonDescription
                {
                    PersonId = personCitizenship.PersonId,
                    PersonPrimaryLanguage = personCitizenship.PrimaryLanguage,
                    CreatedBy = _personnelId,
                    CreateDate = DateTime.Now
                };
                _context.PersonDescription.Add(dbPersonDesc);
            }
            _context.SaveChanges();
        }

        //Delete And Undo For Person Citizenship Details
        public async Task<PersonCitizenshipDetail> DeleteUndoPersonCitizenship(PersonCitizenshipVm citizenshipDetail)
        {
            IQueryable<PersonCitizenship> qryDbPerCs = _context.PersonCitizenship;
            qryDbPerCs = citizenshipDetail.IsFromUsCitizen ? qryDbPerCs.Where(w =>
                w.CitizenshipCountry == citizenshipDetail.CitizenshipCountry &&
                w.PersonId == citizenshipDetail.PersonId && !w.DeleteFlag)
                : qryDbPerCs.Where(w => w.PersonCitizenshipId == citizenshipDetail.PersonCitizenshipId);
            PersonCitizenship dbPerCs = qryDbPerCs.SingleOrDefault();
            int citizenshipCount = _context.PersonCitizenship.Count(p => p.PersonId == citizenshipDetail.PersonId &&
                    p.CitizenshipCountry == citizenshipDetail.CitizenshipCountry && p.DeleteFlag == false);

            if (dbPerCs != null)
            {
                if (dbPerCs.DeleteFlag && citizenshipCount == 0 || dbPerCs.DeleteFlag == false)
                {
                    //Ternary Operator Takes First Value As Type So Need To Assign Proper Return Type Values
                    DateTime? deleteDate = DateTime.Now;
                    int? personnelId = _personnelId;
                    dbPerCs.DeleteFlag = !dbPerCs.DeleteFlag;
                    dbPerCs.DeleteBy = dbPerCs.DeleteFlag ? personnelId : null;
                    dbPerCs.DeleteDate = dbPerCs.DeleteFlag ? deleteDate : null;

                    PersonCitizenshipVm citizenship = new PersonCitizenshipVm
                    {
                        PersonCitizenshipId = dbPerCs.PersonCitizenshipId,
                        PersonId = dbPerCs.PersonId,
                        DeleteFlag = dbPerCs.DeleteFlag,
                        CitizenshipCountry = dbPerCs.CitizenshipCountry,
                        CitizenshipStatus = dbPerCs.CitizenshipStatus,
                        CitizenshipNote = dbPerCs.CitizenshipNote,
                        PersonCitizenshipStatus = PersonCitizenshipStatus.Delete
                    };
                    LoadInsertPerCitizenshipHis(citizenship);
                    await _context.SaveChangesAsync();
                    UpdateCitizenshipString(citizenship.PersonId, citizenshipDetail.DeleteFlag || citizenshipDetail.IsFromUsCitizen);
                    GetCitizenshipList(citizenship.PersonId);
                }
            }

            if (citizenshipDetail.IsFromUsCitizen && citizenshipCount == 0)
            {
                UpdateCitizenshipString(citizenshipDetail.PersonId, citizenshipDetail.DeleteFlag || citizenshipDetail.IsFromUsCitizen);
            }
            return PersonCitizenshipDetail;
        }

        //To Insert Person Citizenship History
        private void LoadInsertPerCitizenshipHis(PersonCitizenshipVm perCitizenshipHis)
        {
            PersonCitizenshipHistory dbPersCsHis = new PersonCitizenshipHistory();

            if (PersonCitizenshipStatus.Insert == perCitizenshipHis.PersonCitizenshipStatus)
            {
                dbPersCsHis.CreateDate = DateTime.Now;
                dbPersCsHis.CreateBy = _personnelId;
            }
            else if (PersonCitizenshipStatus.Update == perCitizenshipHis.PersonCitizenshipStatus)
            {
                dbPersCsHis.UpdateDate = DateTime.Now;
                dbPersCsHis.UpdateBy = _personnelId;
            }
            else if (PersonCitizenshipStatus.Delete == perCitizenshipHis.PersonCitizenshipStatus)
            {
                dbPersCsHis.DeleteFlag = perCitizenshipHis.DeleteFlag;
                dbPersCsHis.DeleteDate = DateTime.Now;
                dbPersCsHis.DeleteBy = _personnelId;
            }
            if (PersonCitizenshipStatus.Insert != perCitizenshipHis.PersonCitizenshipStatus)
            {
                PersonCitizenshipVm createDet = _context.PersonCitizenship
                    .Where(p => p.PersonCitizenshipId == perCitizenshipHis.PersonCitizenshipId)
                    .Select(p => new PersonCitizenshipVm
                    {
                        CreateDate = p.CreateDate,
                        CreateBy = p.CreateBy
                    }).SingleOrDefault();
                if (createDet != null)
                {
                    dbPersCsHis.CreateDate = createDet.CreateDate;
                    dbPersCsHis.CreateBy = createDet.CreateBy;
                }
            }
            if (perCitizenshipHis.PersonCitizenshipStatus != PersonCitizenshipStatus.Delete)
            {
                dbPersCsHis.PassportNumber = perCitizenshipHis.PassportNumber;
                dbPersCsHis.NotificationAcknowledgement = perCitizenshipHis.NotificationAcknowledgement;
                dbPersCsHis.NotificationAutomateFlag = perCitizenshipHis.NotificationAutomateFlag;
            }
            dbPersCsHis.PersonCitizenshipId = perCitizenshipHis.PersonCitizenshipId;
            dbPersCsHis.PersonId = perCitizenshipHis.PersonId;
            dbPersCsHis.CitizenshipCountry = perCitizenshipHis.CitizenshipCountry;
            dbPersCsHis.CitizenshipStatus = perCitizenshipHis.CitizenshipStatus;
            dbPersCsHis.CitizenshipNote = perCitizenshipHis.CitizenshipNote;
            _context.PersonCitizenshipHistory.Add(dbPersCsHis);
        }

        //The Concatenation Of Person Citizenship String Is Updated Into Person Table
        private void UpdateCitizenshipString(int personId, bool isUsCitizen)
        {
            List<PersonCitizenshipVm> lstCitizenship =
                _context.PersonCitizenship.Where(cs => cs.PersonId == personId && cs.DeleteFlag == false)
                    .Select(cs => new PersonCitizenshipVm
                    {
                        CitizenshipCountry = cs.CitizenshipStatus != "" && cs.CitizenshipStatus != null
                            //In database this table column having both null and empty value, So i checked two condition in future i will tune this code.
                            ? cs.CitizenshipCountry + " [" + cs.CitizenshipStatus + ']' : cs.CitizenshipCountry
                    }).ToList();

            Person dbPerson = _context.Person.SingleOrDefault(p => p.PersonId == personId);

            if (dbPerson != null)
            {
                if (lstCitizenship.Count > 0)
                {
                    dbPerson.CitizenshipString = string.Join("; ",
                        lstCitizenship.Select(c => c.CitizenshipCountry.ToString()));
                    if (isUsCitizen)
                    {
                        dbPerson.UsCitizenFlag = dbPerson.UsCitizenFlag
                            && lstCitizenship.Any(s => s.CitizenshipCountry == PersonConstants.UNITEDSTATES)
                            ;
                    }
                }
                else
                {
                    dbPerson.CitizenshipString = null;
                    dbPerson.UsCitizenFlag = false;
                }
            }
            _context.SaveChanges();
        }

        //To Insert And Update Person Details
        public async Task<int> InsertUpdatePersonDetails(PersonIdentity person)
        {
            Person dbPerson = _context.Person.SingleOrDefault(p => p.PersonId == person.PersonId);
            bool isFromInsert = false;
            if (dbPerson == null)
            {
                isFromInsert = true;
                dbPerson = new Person
                {
                    PersonContactId = person.PersonContactId,
                    PersonContactRelationship = person.PersonContactRelationship,
                    //PersonAge =(Int16)person.PersonAge,
                    PersonSiteId = person.PersonSiteId,
                    PersonSiteBnum = person.PersonSiteBnum,
                    CreateBy = _personnelId,
                    FknFirstName = person.PersonFirstName,
                    FknLastName = person.PersonLastName,
                    FknMiddleName = person.PersonMiddleName,
                    FknSuffixName = person.PersonSuffix
                };
            }
            else
            {
                dbPerson.PersonDlNoExpiration = person.PersonDlNoExpiration;
                dbPerson.UpdateBy = _personnelId;
                dbPerson.UpdateDate = DateTime.Now;
            }
            dbPerson.PersonFirstName = person.PersonFirstName;
            dbPerson.PersonMiddleName = person.PersonMiddleName;
            dbPerson.PersonLastName = person.PersonLastName;
            dbPerson.PersonPhone = person.PersonPhone;
            dbPerson.PersonBusinessPhone = person.PersonBusinessPhone;
            dbPerson.PersonBusinessFax = person.PersonBusinessFax;
            dbPerson.PersonDob = person.PersonDob;
            dbPerson.PersonPlaceOfBirth = person.PersonPlaceOfBirth;
            dbPerson.PersonDlNumber = person.PersonDlNumber;
            dbPerson.PersonDlState = person.PersonDlState;
            dbPerson.PersonDlClass = person.PersonDlClass;
            dbPerson.PersonDlExpiration = person.PersonDlExpiration;
            dbPerson.PersonOtherIdType = person.PersonOtherIdType;
            dbPerson.PersonOtherIdNumber = person.PersonOtherIdNumber;
            dbPerson.PersonOtherIdState = person.PersonOtherIdState;
            dbPerson.PersonOtherIdDescription = person.PersonOtherIdDescription;
            dbPerson.PersonOtherIdExpiration = person.PersonOtherIdExpiration;
            dbPerson.PersonSsn = person.PersonSsn;
            dbPerson.PersonSuffix = person.PersonSuffix;
            dbPerson.PersonFbiNo = person.PersonFbiNo;
            dbPerson.PersonDeceased = person.PersonDeceased;
            dbPerson.PersonDeceasedDate = person.PersonDeceasedDate;
            dbPerson.PersonMissing = person.PersonMissing;
            dbPerson.PersonMissingDate = person.PersonMissingDate;
            dbPerson.PersonPlaceOfBirthList = person.PersonPlaceOfBirthList;
            dbPerson.PersonCii = person.PersonCii;
            dbPerson.PersonDoc = person.PersonDoc;
            dbPerson.PersonPhone2 = person.PersonPhone2;
            dbPerson.PersonCellPhone = person.PersonCellPhone;
            dbPerson.PersonEmail = person.PersonEmail;
            dbPerson.PersonMaidenName = person.PersonMaidenName;
            dbPerson.IllegalAlienFlag = person.PersonIllegalAlien;
            dbPerson.UsCitizenFlag = person.PersonUsCitizen;
            dbPerson.PersonAlienNo = person.PersonAlienNo;
            dbPerson.AfisNumber = person.AfisNumber;
            if (dbPerson.PersonId <= 0)
            {
                _context.Person.Add(dbPerson);
                _context.SaveChanges();
                if (person.InmatePreBookId > 0)
                {
                    _iPrebookActiveService.InsertPrebookPerson(person.InmatePreBookId??0, dbPerson.PersonId);
                }
            }
            LoadInsertPersonHistory(dbPerson.PersonId, person.PersonHistoryList);
            InsertUpdateCustomField(dbPerson.PersonId, person.customFields);
            await _context.SaveChangesAsync();
            if (person.PersonUsCitizen && isFromInsert)
            {
                PersonCitizenshipVm citizenship = new PersonCitizenshipVm
                {
                    PersonId = dbPerson.PersonId,
                    CitizenshipCountry = PersonConstants.UNITEDSTATES,
                    IsFromUsCitizen = true
                };
                await InsertUpdatePersonCitizenship(citizenship);
            }
            _interfaceEngine.Export(new ExportRequestVm
            {
                EventName = EventNameConstants.PERSONIDENTITYSAVE,
                PersonnelId = _personnelId,
                Param1 = dbPerson.PersonId.ToString(),
                Param2 = dbPerson.PersonId.ToString()
            });
            return dbPerson.PersonId;
        }



        public async Task<PersonVm> InsertUpdatePerson(PersonVm person)
        {
            if (!_personService.IsPersonSealed(person.PersonId))
            {
                Person dbPerson = _context.Person.SingleOrDefault(p => p.PersonId == person.PersonId);
                if (dbPerson == null)
                {
                    dbPerson = new Person
                    {
                        //PersonContactId = person.PersonContactId,
                        //PersonContactRelationship = person.PersonContactRelationship,
                        //PersonAge = (Int16)person.PersonAge,
                        //PersonSiteId = person.PersonSiteId,
                        //PersonSiteBnum = person.PersonSiteBnum,
                        CreateBy = person.CreateBy ?? _personnelId, //needed for interface engine
                        FknFirstName = person.PersonFirstName,
                        FknLastName = person.PersonLastName,
                        FknMiddleName = person.PersonMiddleName,
                        FknSuffixName = person.PersonSuffix
                    };
                }
                else
                {
                    dbPerson.PersonDlNoExpiration = person.PersonDlNoExpiration;
                    dbPerson.UpdateBy = person.UpdateBy ?? _personnelId;
                    dbPerson.UpdateDate = DateTime.Now;
                }
                dbPerson.PersonFirstName = person.PersonFirstName;
                dbPerson.PersonMiddleName = person.PersonMiddleName;
                dbPerson.PersonLastName = person.PersonLastName;
                dbPerson.PersonPhone = person.PersonPhone;
                dbPerson.PersonBusinessPhone = person.PersonBusinessPhone;
                dbPerson.PersonBusinessFax = person.PersonBusinessFax;
                dbPerson.PersonDob = person.PersonDob;
                dbPerson.PersonPlaceOfBirth = person.PersonPlaceOfBirth;
                dbPerson.PersonDlNumber = person.PersonDlNumber;
                dbPerson.PersonDlState = person.PersonDlState;
                dbPerson.PersonDlClass = person.PersonDlClass;
                dbPerson.PersonDlExpiration = person.PersonDlExpiration;
                dbPerson.PersonOtherIdType = person.PersonOtherIdType;
                dbPerson.PersonOtherIdNumber = person.PersonOtherIdNumber;
                dbPerson.PersonOtherIdState = person.PersonOtherIdState;
                dbPerson.PersonOtherIdExpiration = person.PersonOtherIdExpiration;
                dbPerson.PersonSsn = person.PersonSsn;
                dbPerson.PersonSuffix = person.PersonSuffix;
                dbPerson.PersonFbiNo = person.PersonFbiNo;
                dbPerson.PersonDeceased = person.PersonDeceased;
                dbPerson.PersonDeceasedDate = person.PersonDeceasedDate;
                dbPerson.PersonMissing = person.PersonMissing;
                dbPerson.PersonMissingDate = person.PersonMissingDate;
                dbPerson.PersonPlaceOfBirthList = person.PersonPlaceOfBirthList;
                dbPerson.PersonCii = person.PersonCii;
                dbPerson.PersonDoc = person.PersonDoc;
                dbPerson.PersonPhone2 = person.PersonPhone2;
                dbPerson.PersonCellPhone = person.PersonCellPhone;
                dbPerson.PersonEmail = person.PersonEmail;
                dbPerson.PersonMaidenName = person.PersonMaidenName;
                dbPerson.IllegalAlienFlag = person.PersonIllegalAlien;
                dbPerson.UsCitizenFlag = person.PersonUsCitizen;
                dbPerson.PersonAlienNo = person.PersonAlienNo;
                dbPerson.AfisNumber = person.AfisNumber;
                if (dbPerson.PersonId <= 0)
                {
                    _context.Person.Add(dbPerson);
                }
                await _context.SaveChangesAsync();
                LoadInsertPersonHistory(dbPerson.PersonId, person.PersonHistoryList,
                    person.UpdateBy ?? person.CreateBy ?? _personnelId);
                GetPerson(dbPerson.PersonId);
            }
            await _context.SaveChangesAsync();
            return PersonVm;
        }

        //To Insert Person History
        public void LoadInsertPersonHistory(int personId, string personHistoryList)
        {
            if (!string.IsNullOrEmpty(personHistoryList))
            {
                LoadInsertPersonHistory(personId, personHistoryList, null);
            }
        }

        public void InsertUpdateCustomField(int personId, List<CustomField> customFields)
        {
            customFields.ForEach(item =>
            {
                CustomFieldSaveData dbSaveData = _context.CustomFieldSaveData.SingleOrDefault(w => w.CustomFieldLookupId == item.CustomFieldLookupId
                  && w.CustomFieldKeyValue == personId);
                if (dbSaveData == null)
                {
                    dbSaveData = new CustomFieldSaveData
                    {
                        CustomFieldLookupId = item.CustomFieldLookupId,
                        CustomFieldKeyValue = personId,
                        CustomFieldEntry = item.CustomfieldEntry,
                        CreateBy = _personnelId,
                        CreateDate = DateTime.Now
                    };

                    _context.CustomFieldSaveData.Add(dbSaveData);
                }
                else
                {
                    dbSaveData.CustomFieldEntry = item.CustomfieldEntry;
                    dbSaveData.UpdateBy = _personnelId;
                    dbSaveData.UpdateDate = DateTime.Now;
                }

                _context.SaveChanges();
            });


        }

        private void LoadInsertPersonHistory(int personId, string personHistoryList, int? personnelId)
        {
            PersonHistory dbPerHisDet = new PersonHistory
            {
                CreateDate = DateTime.Now,
                PersonId = personId,
                PersonnelId = personnelId ?? _personnelId,
                PersonHistoryList = personHistoryList
            };
            _context.PersonHistory.Add(dbPerHisDet);
        }

        //In admin->person screen having the inmate number edit option
        public async Task<PersonVm> UpdateInmateNumber(int inmateId, string inmateNumber, string akaHistoryList)
        {
            //getting new inmate number record and old inmate number record
            List<Inmate> lstDbInmate = _context.Inmate
                .Where(i => i.InmateId == inmateId || i.InmateNumber == inmateNumber).ToList();
            PersonVm personVm = new PersonVm();
            int? personId = lstDbInmate.FirstOrDefault(x => x.InmateNumber == inmateNumber)?.PersonId;
            if (personId is null)
            {
                Inmate dbInmate = lstDbInmate.Single(x => x.InmateId == inmateId);
                Aka dbAka = new Aka
                {
                    PersonId = dbInmate.PersonId,
                    AkaType = PersonConstants.AKA,
                    CreateDate = DateTime.Now,
                    AkaOtherIdType = PersonConstants.INMATENUMBER,
                    AkaOtherIdNumber = dbInmate.InmateNumber,
                    CreatedBy = 1 //hardcoded for session of user_id
                };
                _context.Aka.Add(dbAka);
                _akaService.LoadInsertAkaHistory(dbAka.AkaId, akaHistoryList);
                dbInmate.InmateNumber = inmateNumber;
            }
            else
            {
                personVm = _context.Person.Where(p => p.PersonId == personId).Select(p => new PersonVm
                {
                    PersonLastName = p.PersonLastName,
                    PersonFirstName = p.PersonFirstName,
                    PersonDob = p.PersonDob,
                    PersonId = p.PersonId
                }).SingleOrDefault();
            }
            await _context.SaveChangesAsync();
            return personVm;
        }

        public List<CustomField> GetCustomFields(int userControlId, int? PersonId)
        {
            List<CustomField> customFields = _context.CustomFieldLookup.Where(w => w.DeleteFlag != 1 && w.AppAoUserControlId == userControlId)
                .Select(s => new CustomField
                {
                    CustomFieldLookupId = s.CustomFieldLookupId,
                    UserControlId = s.AppAoUserControlId,
                    FieldLabel = s.FieldLabel,
                    FieldOrder = s.FieldOrder,
                    FieldTypeTextFlag = s.FieldTypeTextFlag,
                    FieldTypeDropDownFlag = s.FieldTypeDropDownFlag,
                    FieldTypeCheckBoxFlag = s.FieldTypeCheckboxFlag,
                    FieldTextEntryMaxLength = s.FieldTextEntryMaxLength,
                    FieldTextentryAlpha = s.FieldTextEntryAlpha,
                    FieldTextentryNumericonly = s.FieldTextEntryNumericOnly,
                    FieldTextentryNumericallowdecimal = s.FieldTextEntryNumericAllowDecimal,
                    FieldSizeSmall = s.FieldSizeSmall,
                    FieldSizeMedium = s.FieldSizeMedium,
                    FieldSizeLarge = s.FieldSizeLarge,
                    FieldRequired = s.FieldRequired,
                    // DropDownValues=_context

                }).OrderBy(o => o.FieldRequired).ToList();

            customFields.ForEach(item =>
            {

                item.CustomfieldEntry = _context.CustomFieldSaveData.SingleOrDefault(w => w.CustomFieldLookupId == item.CustomFieldLookupId
                                            && w.CustomFieldKeyValue == PersonId)?.CustomFieldEntry;

                if (item.FieldTypeDropDownFlag == 1)
                {
                    item.DropDownValues = _context.CustomFieldDropDown.Where(w => w.DeleteFlag != 1 && w.CustomFieldLookupId == item.CustomFieldLookupId)
                .Select(s => new CustomDropDownValues
                {
                    ListEntryID = s.CustomFieldDropDownId,
                    ListEntry = s.DropDownText

                }).ToList();
                }
            });

            return customFields;
        }
    }
}

