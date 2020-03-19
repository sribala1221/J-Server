using GenerateTables.Models;
using Microsoft.AspNetCore.Http;
using ServerAPI.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ServerAPI.ViewModels;

// ReSharper disable once CheckNamespace
namespace ServerAPI.Services
{
    public class BookingVerifyIdService : IBookingVerifyIdService
    {
        private readonly AAtims _context;
        private readonly int _personnelId;
        private readonly ICommonService _commonService;
        private readonly IPhotosService _photosService;
        public BookingVerifyIdService(AAtims context, IHttpContextAccessor httpContextAccessor, ICommonService commonService, IPhotosService photosService)
        {
            _context = context;
            _personnelId =
                Convert.ToInt32(httpContextAccessor.HttpContext.User.FindFirst(AuthDetailConstants.PERSONNELID)?.Value);
            _commonService = commonService;
            _photosService = photosService;
        }
        public List<BookingVerifyDataVm> GetVerifyInmateDetail(int facilityId)
        {
            var externalPath = _photosService.GetExternalPath();
            var path = _photosService.GetPath();
            List<BookingVerifyDataVm> inmateDetail = _context.Incarceration.Where(w =>
                !w.ReleaseOut.HasValue
                && w.Inmate.InmateActive == 1
                && w.Inmate.FacilityId == facilityId).Select(s => new BookingVerifyDataVm
                {
                    Person = new PersonVm
                    {
                        PersonId = s.Inmate.PersonId,
                        InmateId = s.InmateId,
                        InmateNumber = s.Inmate.InmateNumber,
                        InmateActive = s.Inmate.InmateActive == 1,
                        PersonLastName = s.Inmate.Person.PersonLastName,
                        PersonFirstName = s.Inmate.Person.PersonFirstName,
                        PersonMiddleName = s.Inmate.Person.PersonMiddleName,
                        PersonCii = s.Inmate.Person.PersonCii,
                        PersonFbiNo = s.Inmate.Person.PersonFbiNo,
                        PersonDob = s.Inmate.Person.PersonDob
                    },
                    HousingUnit = new HousingDetail
                    {
                        FacilityAbbr = s.Inmate.Facility.FacilityAbbr,
                        HousingUnitListId = s.Inmate.HousingUnit.HousingUnitListId,
                        HousingUnitLocation = s.Inmate.HousingUnit.HousingUnitLocation,
                        HousingUnitNumber = s.Inmate.HousingUnit.HousingUnitNumber,
                        HousingUnitBedLocation = s.Inmate.HousingUnit.HousingUnitBedLocation,
                        HousingUnitBedNumber = s.Inmate.HousingUnit.HousingUnitBedNumber
                    },
                    BookingNumber = s.BookingNo,
                    IncarcerationId = s.IncarcerationId,
                    LiveScanStateNumber = s.VerifyStateNumber,
                    LiveScanFbiNumber = s.VerifyFBINumber,
                    Payload = s.VerifyPayload,
                    VerifyExternalId = s.VerifyExternalID,
                    VerifyIdFlag = s.VerifyIDFlag,
                    IsCreatePerson = s.VerifyIDFlag == (int?)(BookingVerifyType.Move) && _context.Person.Count(x => x.PersonId != s.Inmate.PersonId && !x.PersonDuplicateId.HasValue &&
                                        String.Equals(
                                            x.PersonCii,
                                            s.VerifyStateNumber,
                                            StringComparison.CurrentCultureIgnoreCase)) == 0,
                    PhotographRelativePath = s.Inmate.Person.Identifiers
                                                .Where(idf => idf.DeleteFlag == 0).Select(x => x.PhotographRelativePath == null ? externalPath + x.PhotographPath : path + x.PhotographRelativePath).FirstOrDefault(),

                    IdentifierId = s.Inmate.Person.Identifiers
                                               .Where(idf => idf.DeleteFlag == 0).Select(x => x.IdentifierId).FirstOrDefault()

                }).ToList();
            return inmateDetail;
        }
        public List<BookingVerifyDataVm> GetDuplicateRecords(BookingVerifyDataVm verifyDataVm)
        {
            IQueryable<Inmate> inmate = _context.Inmate;
            List<BookingVerifyDataVm> duplicates = _context.Person
                .Where(x => x.PersonId != verifyDataVm.PersonId && !x.PersonDuplicateId.HasValue && String.Equals(x.PersonCii,
                                verifyDataVm.LiveScanStateNumber, StringComparison.CurrentCultureIgnoreCase)).Select(
                    s => new BookingVerifyDataVm
                    {
                        Person = new PersonVm
                        {
                            PersonId = s.PersonId,
                            InmateId = inmate.FirstOrDefault(x => x.PersonId == s.PersonId).InmateId,
                            InmateNumber = inmate.FirstOrDefault(x => x.PersonId == s.PersonId).InmateNumber,
                            InmateActive = inmate.FirstOrDefault(x => x.PersonId == s.PersonId).InmateActive == 1,
                            PersonLastName = s.PersonLastName,
                            PersonFirstName = s.PersonFirstName,
                            PersonMiddleName = s.PersonMiddleName,
                            PersonCii = s.PersonCii,
                            PersonFbiNo = s.PersonFbiNo,
                            PersonDob = s.PersonDob
                        }
                    }).ToList();
            int[] personId = duplicates.Select(x => x.Person.PersonId).ToArray();

            List<Identifiers> identifierLst = GetIdentifiers(personId);
            duplicates.ForEach(item =>
            {
                Identifiers identifiers = identifierLst.FirstOrDefault(x => x.PersonId == item.Person.PersonId);
                if (identifiers != null)
                {
                    item.PhotographRelativePath = identifiers.PhotographRelativePath;
                    item.IdentifierId = item.IdentifierId;
                }
            });
            return duplicates;
        }
        public async Task<int> UpdateVerifyDetail(BookingVerifyDataVm verifyDataVm)
        {
            Person person = _context.Person.Single(x => x.PersonId == verifyDataVm.PersonId);
            Inmate inmate = _context.Inmate.Single(x => x.InmateId == verifyDataVm.InmateId);
            Incarceration inc =
                _context.Incarceration.Single(x => x.IncarcerationId == verifyDataVm.IncarcerationId);

            int dupCount = _context.Person.Count(x => x.PersonId != verifyDataVm.PersonId && !x.PersonDuplicateId.HasValue &&
                                             String.Equals(x.PersonCii, verifyDataVm.LiveScanStateNumber, StringComparison.CurrentCultureIgnoreCase));
            int curCount = _context.Person.Count(x => x.PersonId == verifyDataVm.PersonId &&
                                             String.Equals(x.PersonCii, verifyDataVm.LiveScanStateNumber, StringComparison.CurrentCultureIgnoreCase));
            if (verifyDataVm.IsBypass)
            {
                inc.VerifyIDFlag = (int)(BookingVerifyType.ByPass);
                inc.VerifyIDDate = DateTime.Now;
                inc.VerifyIDBy = _personnelId;
                _context.SaveChanges();
            }
            else if (!string.IsNullOrEmpty(verifyDataVm.LiveScanStateNumber) && string.IsNullOrEmpty(verifyDataVm.CurrentStateNumber))
            {
                if (dupCount > 0)
                {
                    inc.VerifyIDFlag = (int)(BookingVerifyType.Merge);
                    inc.VerifyStateNumber = verifyDataVm.LiveScanStateNumber;
                    inc.VerifyFBINumber = verifyDataVm.LiveScanFbiNumber;
                    inc.VerifyIDDate = DateTime.Now;
                    inc.VerifyIDBy = _personnelId;
                    UpdatePersonCii(person, verifyDataVm);
                    InsertAka(person, true);
                    UpdatePerson(person, verifyDataVm, true);
                }
                else
                {
                    inc.VerifyIDFlag = (int)(BookingVerifyType.Verified);
                    inc.VerifyStateNumber = verifyDataVm.LiveScanStateNumber;
                    inc.VerifyFBINumber = verifyDataVm.LiveScanFbiNumber;
                    inc.VerifyIDDate = DateTime.Now;
                    inc.VerifyIDBy = _personnelId;
                    UpdatePersonCii(person, verifyDataVm);
                    InsertAka(person, true);
                    UpdatePerson(person, verifyDataVm, true);
                    UpdateInmateNumber(inmate);
                }
            }
            else if (String.Equals(verifyDataVm.LiveScanStateNumber, verifyDataVm.CurrentStateNumber, StringComparison.CurrentCultureIgnoreCase))
            {
                if (dupCount > 0)
                {
                    inc.VerifyIDFlag = (int)(BookingVerifyType.Merge);
                    inc.VerifyStateNumber = verifyDataVm.LiveScanStateNumber;
                    inc.VerifyFBINumber = verifyDataVm.LiveScanFbiNumber;
                    inc.VerifyIDDate = DateTime.Now;
                    inc.VerifyIDBy = _personnelId;
                    _context.SaveChanges();
                    InsertAka(person, false);
                    UpdatePerson(person, verifyDataVm, false);
                }
                else if (curCount > 0)
                {
                    inc.VerifyIDFlag = (int)(BookingVerifyType.Verified);
                    inc.VerifyStateNumber = verifyDataVm.LiveScanStateNumber;
                    inc.VerifyFBINumber = verifyDataVm.LiveScanFbiNumber;
                    inc.VerifyIDDate = DateTime.Now;
                    inc.VerifyIDBy = _personnelId;
                    _context.SaveChanges();
                    InsertAka(person, false);
                    UpdatePerson(person, verifyDataVm, false);
                    UpdateInmateNumber(inmate);
                }
            }
            else if (!String.Equals(verifyDataVm.LiveScanStateNumber, verifyDataVm.CurrentStateNumber, StringComparison.CurrentCultureIgnoreCase))
            {
                inc.VerifyIDFlag = (int)(BookingVerifyType.Move);
                inc.VerifyStateNumber = verifyDataVm.LiveScanStateNumber;
                inc.VerifyFBINumber = verifyDataVm.LiveScanFbiNumber;
                inc.VerifyIDDate = DateTime.Now;
                inc.VerifyIDBy = _personnelId;
                _context.SaveChanges();
            }
            return await _context.SaveChangesAsync();
        }
        public bool GetVerifyInmate(int incarcerationId)
        {
            bool isVerify = true;
            string siteOption = _commonService.GetSiteOptionValue(string.Empty, SiteOptionsConstants.REQUIREVERIFYIDATBOOKINGCOMPLETE);
            if (siteOption.ToUpper() == SiteOptionsConstants.ON)
            {
                int verifyId = _context.Incarceration.Single(x => x.IncarcerationId == incarcerationId).VerifyIDFlag;
                if (verifyId != 1)
                    isVerify = false;
            }
            return isVerify;
        }

        public async Task<int> CreateInmate(PersonVm person)
        {
            Person per = new Person
            {
                PersonLastName = person.PersonLastName,
                PersonFirstName = person.PersonFirstName,
                PersonMiddleName = person.PersonMiddleName,
                PersonDob = person.PersonDob,
                PersonCii = person.PersonCii,
                PersonFbiNo = person.PersonFbiNo
            };
            _context.Person.Add(per);
            _context.SaveChanges();

            string inmateNumber = _commonService.GetGlobalNumber(2); // get the global numbers

        checkAlreadyExistsInmate:
            if (_context.Inmate.Any(w => w.InmateNumber == inmateNumber))
            {
                inmateNumber = _commonService.GetGlobalNumber(2); // get the global numbers
                goto checkAlreadyExistsInmate;
            }
            int age = _commonService.GetAgeFromDob(per.PersonDob);
            Inmate inmate = new Inmate
            {
                PersonId = per.PersonId,
                InmateNumber = inmateNumber,
                InmateReceivedDate = DateTime.Now,
                InmateOfficerId = _personnelId,
                FacilityId = person.FacilityId,
                InmateJuvenileFlag = age < 18 ? 1 : 0
            };
            _context.Inmate.Add(inmate);

            return await _context.SaveChangesAsync();
        }
        private void InsertAka(Person per, bool flag)
        {
            if (!flag && !string.IsNullOrEmpty(per.PersonFbiNo))
            {
                Aka aka = new Aka
                {
                    PersonId = per.PersonId,
                    AkaFbi = per.PersonFbiNo,
                    CreatedBy = _personnelId,
                    CreateDate = DateTime.Now
                };
                _context.Aka.Add(aka);
                _context.SaveChanges();
            }
            else
            {
                Aka aka = new Aka
                {
                    PersonId = per.PersonId,
                    AkaFbi = per.PersonFbiNo,
                    AkaCii = per.PersonCii,
                    CreatedBy = _personnelId,
                    CreateDate = DateTime.Now
                };
                _context.Aka.Add(aka);
                _context.SaveChanges();
            }
        }
        private void UpdatePerson(Person per, BookingVerifyDataVm verifyDataVm, bool flag)
        {
            if (flag){
                per.PersonCii = verifyDataVm.LiveScanStateNumber;
                per.PersonFbiNo = verifyDataVm.LiveScanFbiNumber;
                _context.SaveChanges();
            }
            else if(!string.IsNullOrEmpty(verifyDataVm.LiveScanFbiNumber)){
                per.PersonFbiNo = verifyDataVm.LiveScanFbiNumber;
                _context.SaveChanges();
            }

        }
        private void UpdatePersonCii(Person person, BookingVerifyDataVm verifyDataVm)
        {
            Person per = _context.Person.Single(x => x.PersonId == person.PersonId);
            per.PersonCii = verifyDataVm.LiveScanStateNumber;
            _context.SaveChanges();
        }
        private void UpdateInmateNumber(Inmate inmate)
        {
            if (inmate.InmateNumber.ToUpper().StartsWith("T"))
            {
                string siteOption = _commonService.GetSiteOptionValue(string.Empty, SiteOptionsConstants.REMOVEINMATENUMPREFIXATVERIFYID);
                if (siteOption.ToUpper() == SiteOptionsConstants.ON)
                {
                    string num = inmate.InmateNumber.Remove(0, 1);
                    inmate.InmateNumber = num;
                    _context.SaveChanges();
                }
            }
        }
        private List<Identifiers> GetIdentifiers(int[] personId)
        {
            List<Identifiers> identifierLst = _context.Identifiers
                .Where(idf => personId.Contains(idf.PersonId ?? 0) && idf.DeleteFlag == 0).Select(x => new Identifiers
                {
                    PersonId = x.PersonId,
                    IdentifierId = x.IdentifierId,
                    PhotographRelativePath = _photosService.GetPhotoByIdentifier(x)
                }).ToList();
            return identifierLst;
        }

        public List<KeyValuePair<PersonVm, bool>> GetParticularPersonnel(string personnelSearch)
        {
            List<KeyValuePair<PersonVm, bool>> lsKeyValuePairs = _context.Personnel
                .Where(a => !a.PersonnelTerminationFlag &&
                            (personnelSearch != OfficerFlag.ARRESTTRANSPORT || a.ArrestTransportOfficerFlag)
                            && (personnelSearch != OfficerFlag.RECEIVESEARCH ||
                                a.ReceiveSearchOfficerFlag)
                            && (personnelSearch != OfficerFlag.PROGRAMINSTRUCTOR ||
                                a.ProgramInstructorFlag)
                            && (personnelSearch != OfficerFlag.PROGRAMCASE ||
                                a.PersonnelProgramCaseFlag))
                .Select(a => new KeyValuePair<PersonVm, bool>(new PersonVm
                {
                    PersonnelId = a.PersonnelId,
                    PersonLastName = a.PersonNavigation.PersonLastName,
                    PersonFirstName = a.PersonNavigation.PersonFirstName,
                    PersonMiddleName = a.PersonNavigation.PersonMiddleName,
                    PersonnelBadgeNumber = a.OfficerBadgeNum
                }, false)).ToList();

            return lsKeyValuePairs;
        }

        public string EditSearchOfficierDetails(int arrestId)
        {
            string lstValues = _context.Arrest.SingleOrDefault(s => s.ArrestId == arrestId)?.ArrestSearchOfficerAddList;

            return lstValues;
        }

    }
}
