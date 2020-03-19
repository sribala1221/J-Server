using GenerateTables.Models;
using JwtDb.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;
using ServerAPI.Utilities;
using ServerAPI.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;

// ReSharper disable once CheckNamespace
namespace ServerAPI.Services
{
    public class SearchInmateService : ISearchInmateService
    {
        private readonly AAtims _context;
        private readonly ICommonService _commonService;
        private readonly int _personnelId;
        private readonly JwtDbContext _jwtDbContext;
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        // ReSharper disable once NotAccessedField.Local
        private readonly IPhotosService _photos;

        public SearchInmateService(AAtims context, IPhotosService photosService, ICommonService commonService,
            IHttpContextAccessor httpContextAccessor, JwtDbContext jwtDbContext,
            UserManager<AppUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _photos = photosService;
            _commonService = commonService;
            _personnelId =
                Convert.ToInt32(httpContextAccessor.HttpContext.User.FindFirst(AuthDetailConstants.PERSONNELID)?.Value);
            _jwtDbContext = jwtDbContext;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        //To get facility housing details
        public List<HousingDetail> GetHousing(int facilityId)
        {
            List<HousingDetail> housingUnit = _context.HousingUnit
                .Where(hu => hu.FacilityId == facilityId)
                .Select(hu => new HousingDetail
                {
                    HousingUnitListId = hu.HousingUnitListId,
                    HousingUnitLocation = hu.HousingUnitLocation,
                    HousingUnitNumber = hu.HousingUnitNumber,
                    HousingUnitBedNumber = hu.HousingUnitBedNumber,
                    HousingUnitBedLocation = hu.HousingUnitBedLocation,
                    Inactive = (hu.HousingUnitInactive ?? 0) == 1
                }).Distinct().ToList();
            return housingUnit;
        }

        public List<HousingUnitList> PodDetails(string buildingId)
        {
            List<HousingUnitList> housingUnit = _context.HousingUnitList
                .Where(hu => hu.HousingUnitLocation == buildingId)
                .Select(hu => new HousingUnitList
                {
                    HousingUnitListId = hu.HousingUnitListId,
                    HousingUnitLocation = hu.HousingUnitLocation,
                    HousingUnitNumber = hu.HousingUnitNumber,

                }).Distinct().ToList();
            return housingUnit;
        }

        public IEnumerable<LookupVm> GetClassificationDetails()
        {
            
                IEnumerable<LookupVm> Lookup = _commonService.GetLookupList(LookupConstants.CLASREAS)
                .Select(x => new LookupVm
                {
                    LookupDescription = x.LookupDescription,
                    LookupName = x.LookupName,
                    LookupIndex = x.LookupIndex,
                    LookupFlag6 = x.LookupFlag6 ?? 0                 
                }).ToList();
            return Lookup;
        }

        public List<PrivilegeDetailsVm> GetLocationDetails()
        {
            List<int?> classificationIds = _context.Inmate.Where(m => m.InmateCurrentTrackId > 0 && m.InmateActive == 1)
                .Select(m => m.InmateCurrentTrackId).Distinct().ToList();

            List<PrivilegeDetailsVm> locationDetails = _context.Privileges
                .Where(m => classificationIds.Contains(m.PrivilegeId))
                .Select(m => new PrivilegeDetailsVm
                {
                    PrivilegeId = m.PrivilegeId,
                    PrivilegeDescription = m.PrivilegeDescription
                }).ToList();
            return locationDetails;
        }

        public List<FlagAlertVm> GetFlagAlertDetails(bool isPermission)
        {
            List<FlagAlertVm> dbFlagAlertLst = _context.PersonFlag
                .Select(prf => new FlagAlertVm
                {
                    // PersonFlagId will be shown only for active records
                    PersonFlagId = prf.DeleteFlag == 0 ? prf.PersonFlagId : 0,
                    PersonFlagIndex = prf.PersonFlagIndex ?? 0,
                    InmateFlagIndex = prf.InmateFlagIndex ?? 0,
                    DietFlagIndex = prf.DietFlagIndex ?? 0,
                    MedicalFlagIndex = prf.MedicalFlagIndex ?? 0,
                    FlagNote = prf.FlagNote,
                    FlagExpire = prf.FlagExpire,
                    DeleteFlag = prf.DeleteFlag == 1,
                    LookupSubListId = prf.LookupSubListId,
                    LookupSubListName = prf.LookupSubList.SubListValue
                }).ToList();

            List<LookupVm> staticFlagLst = _commonService.GetLookups(new[]
            {
                LookupConstants.TRANSCAUTION, LookupConstants.PERSONCAUTION, LookupConstants.DIET,
                LookupConstants.MEDFLAG
            });

            if (isPermission)
            {
                // Get alerts flag permission details
                string userId = _jwtDbContext.UserClaims.Where(w => w.ClaimType == "personnel_id"
                                                                    && w.ClaimValue == _personnelId.ToString())
                    .Select(s => s.UserId).FirstOrDefault();

                AppUser appUser = _userManager.FindByIdAsync(userId).Result;

                List<string> roleIds = _jwtDbContext.UserRoles.Where(w => w.UserId == appUser.Id).Select(s => s.RoleId)
                    .ToList();

                List<IdentityRole> roles = _roleManager.Roles.Where(w => roleIds.Contains(w.Id)).ToList();

                var claimsLst = new List<string>();

                roles.ForEach(f =>
                {
                    claimsLst.AddRange(_roleManager.GetClaimsAsync(f).Result.Select(s => s.Type).ToList());
                });

                List<string> roleClaims = claimsLst.Distinct().ToList();

                staticFlagLst.ToList().ForEach(f =>
                {
                    if (!(roleClaims.Any(c =>
                        c == PermissionTypes.PersonFlag + PermissionTypes.Access + f.LookupIdentity
                        || c == PermissionTypes.InmateFlag + PermissionTypes.Access + f.LookupIdentity
                        || c == PermissionTypes.MedFlag + PermissionTypes.Access + f.LookupIdentity
                        || (c == PermissionTypes.PersonFlag + PermissionTypes.AllAccess &&
                            f.LookupType == LookupConstants.PERSONCAUTION)
                        || (c == PermissionTypes.InmateFlag + PermissionTypes.AllAccess &&
                            f.LookupType == LookupConstants.TRANSCAUTION)
                        || (c == PermissionTypes.MedFlag + PermissionTypes.AllAccess &&
                            (f.LookupType == LookupConstants.DIET || f.LookupType == LookupConstants.MEDFLAG))
                    )))
                    {
                        staticFlagLst.Remove(f);
                    }
                });
            }

            //Convert LookupVm model into FlagAlertVm model using Json Serialize
            List<FlagAlertVm> personFlagAlertLst =
                JsonConvert.DeserializeObject<List<FlagAlertVm>>(JsonConvert.SerializeObject(staticFlagLst));

            // Set PersonFlagIndex from Lookup index
            List<FlagAlertVm> personCautionLst =
                personFlagAlertLst.Where(pfa => pfa.LookupType == LookupConstants.PERSONCAUTION).ToList();

            personCautionLst.ForEach(per => per.PersonFlagIndex = per.LookupIndex);

            // Set InmateFlagIndex from Lookup index
            List<FlagAlertVm> transCautionLst =
                personFlagAlertLst.Where(pfa => pfa.LookupType == LookupConstants.TRANSCAUTION).ToList();

            transCautionLst.ForEach(tra => tra.InmateFlagIndex = tra.LookupIndex);

            // Set DietFlagIndex from Lookup index
            List<FlagAlertVm> dietCautionLst =
                personFlagAlertLst.Where(pfa => pfa.LookupType == LookupConstants.DIET).ToList();

            dietCautionLst.ForEach(det => det.DietFlagIndex = det.LookupIndex);

            // Set MedFlagIndex from Lookup index
            List<FlagAlertVm> medCautionLst =
                personFlagAlertLst.Where(pfa => pfa.LookupType == LookupConstants.MEDFLAG).ToList();

            medCautionLst.ForEach(med => med.MedicalFlagIndex = med.LookupIndex);

            if (dbFlagAlertLst.Any())
            {
                dbFlagAlertLst.ForEach(flagAlert =>
                {
                    if (flagAlert.PersonFlagIndex > 0)
                    {
                        FlagAlertVm personFlagAlert = personFlagAlertLst.SingleOrDefault(pfa =>
                            pfa.LookupType == LookupConstants.PERSONCAUTION 
                            // ReSharper disable once CompareOfFloatsByEqualityOperator
                            && pfa.LookupIndex == (double?)flagAlert.PersonFlagIndex);
                        if (personFlagAlert != null)
                        {
                            personFlagAlert.PersonFlagId = flagAlert.PersonFlagId;
                            personFlagAlert.PersonFlagIndex = flagAlert.PersonFlagIndex;
                            personFlagAlert.FlagNote = flagAlert.FlagNote;
                            personFlagAlert.FlagExpire = flagAlert.FlagExpire;
                            personFlagAlert.LookupSubListId = flagAlert.LookupSubListId;
                            personFlagAlert.LookupSubListName = flagAlert.LookupSubListName;
                        }
                    }
                    else if (flagAlert.InmateFlagIndex > 0)
                    {
                        FlagAlertVm inmateFlagAlert = personFlagAlertLst.SingleOrDefault(pfa =>
                            pfa.LookupType == LookupConstants.TRANSCAUTION 
                            // ReSharper disable once CompareOfFloatsByEqualityOperator
                            && pfa.LookupIndex == (double?)flagAlert.InmateFlagIndex);
                        if (inmateFlagAlert == null) return;
                        inmateFlagAlert.PersonFlagId = flagAlert.PersonFlagId;
                        inmateFlagAlert.InmateFlagIndex = flagAlert.InmateFlagIndex;
                        inmateFlagAlert.FlagNote = flagAlert.FlagNote;
                        inmateFlagAlert.FlagExpire = flagAlert.FlagExpire;
                        inmateFlagAlert.LookupSubListId = flagAlert.LookupSubListId;
                        inmateFlagAlert.LookupSubListName = flagAlert.LookupSubListName;
                    }
                    else if (flagAlert.DietFlagIndex > 0)
                    {
                        FlagAlertVm dietFlagAlert = personFlagAlertLst.SingleOrDefault(pfa =>
                            pfa.LookupType == LookupConstants.DIET 
                            // ReSharper disable once CompareOfFloatsByEqualityOperator
                            && pfa.LookupIndex == (double?)flagAlert.DietFlagIndex);
                        if (dietFlagAlert == null) return;
                        dietFlagAlert.PersonFlagId = flagAlert.PersonFlagId;
                        dietFlagAlert.DietFlagIndex = flagAlert.DietFlagIndex;
                        dietFlagAlert.FlagNote = flagAlert.FlagNote;
                        dietFlagAlert.FlagExpire = flagAlert.FlagExpire;
                        dietFlagAlert.LookupSubListId = flagAlert.LookupSubListId;
                        dietFlagAlert.LookupSubListName = flagAlert.LookupSubListName;
                    }
                    else if (flagAlert.MedicalFlagIndex > 0)
                    {
                        FlagAlertVm medFlagAlert = personFlagAlertLst.SingleOrDefault(med =>
                            med.LookupType == LookupConstants.MEDFLAG 
                            // ReSharper disable once CompareOfFloatsByEqualityOperator
                            && med.LookupIndex == (double?)flagAlert.MedicalFlagIndex);
                        if (medFlagAlert == null) return;
                        medFlagAlert.PersonFlagId = flagAlert.PersonFlagId;
                        medFlagAlert.MedicalFlagIndex = flagAlert.MedicalFlagIndex;
                        medFlagAlert.FlagNote = flagAlert.FlagNote;
                        medFlagAlert.FlagExpire = flagAlert.FlagExpire;
                        medFlagAlert.LookupSubListId = flagAlert.LookupSubListId;
                        medFlagAlert.LookupSubListName = flagAlert.LookupSubListName;
                    }
                });
            }

            return personFlagAlertLst;
        }

        public List<PersonnelVm> GetPersonnel(string type, int agencyId)
        {
            List<PersonnelVm> personnelVms = _context.Personnel
                .Where(w => w.AgencyId == agencyId && !w.PersonnelTerminationFlag &&
                            ((type != "arrest" || w.ArrestTransportOfficerFlag)
                             && (type != "receive" || w.ReceiveSearchOfficerFlag)))
                .Select(s => new PersonnelVm
                {
                    ArrestTransportOfficerFlag = s.ArrestTransportOfficerFlag,
                    PersonLastName = s.PersonNavigation.PersonLastName,
                    PersonFirstName = s.PersonNavigation.PersonFirstName,
                    PersonMiddleName = s.PersonNavigation.PersonMiddleName,
                    PersonId = s.PersonId,
                    PersonnelId = s.PersonnelId,
                    OfficerBadgeNumber = s.OfficerBadgeNum,
                    PersonnelAgencyFlag = s.PersonnelAgencyGroupFlag,
                    AgencyId = s.AgencyId,
                    PersonnelFullDisplayName = GetPersonnelFullName(s.PersonNavigation) +
                                               (!string.IsNullOrEmpty(s.OfficerBadgeNum) ? " " + s.OfficerBadgeNum : "")
                }).OrderBy(s => s.PersonLastName)
                .ToList();
            return personnelVms;
        }

        //TODO remove from this class
        private static string GetPersonnelFullName(Person person)
        {
            string fullName = "";
            if (!string.IsNullOrEmpty(person.PersonLastName))
                fullName += person.PersonLastName;
            if (!string.IsNullOrEmpty(person.PersonFirstName))
                fullName += ", " + person.PersonFirstName;
            return fullName;
        }

        public InmateBookingInfoVm GetInmateBookingInfo()
        {
            //Agency List
            List<Agency> agencyList = _context.Agency.OrderBy(a => a.AgencyName).Select(s => new Agency
            {
                AgencyId = s.AgencyId,
                AgencyName = s.AgencyName,
                AgencyArrestingFlag = s.AgencyArrestingFlag,
                AgencyBillingExceptionFlag = s.AgencyBillingExceptionFlag,
                AgencyInactiveFlag = s.AgencyInactiveFlag,
                AgencyBillingFlag = s.AgencyBillingFlag,
                AgencyBookingFlag = s.AgencyBookingFlag,
                AgencyCourtFlag = s.AgencyCourtFlag
            }).ToList();

            InmateBookingInfoVm bookingInfo = new InmateBookingInfoVm
            {
                //Arresting Agency
                ArrestingAgency = agencyList.Where(a => a.AgencyArrestingFlag)
                    .Select(a => new KeyValuePair<int, string>(a.AgencyId, a.AgencyName)).ToList(),
                //Billing Agency
                BillingAgency = agencyList.Where(a =>
                        !a.AgencyBillingExceptionFlag &&
                        (!a.AgencyInactiveFlag.HasValue || a.AgencyInactiveFlag == 0) &&
                        a.AgencyBillingFlag)
                    .Select(a => new KeyValuePair<int, string>(a.AgencyId, a.AgencyName)).ToList(),
                //Booking Agency
                BookingAgency = agencyList.Where(a => a.AgencyBookingFlag)
                    .Select(a => new KeyValuePair<int, string>(a.AgencyId, a.AgencyName)).ToList(),
                //Originating Agency
                OriginatingAgency = agencyList
                    .Where(a => !a.AgencyCourtFlag)
                    .Select(a => new KeyValuePair<int, string>(a.AgencyId, a.AgencyName)).ToList(),
                //court
                Court = agencyList.Where(a => a.AgencyCourtFlag &&
                        (!a.AgencyInactiveFlag.HasValue || a.AgencyInactiveFlag == 0))
                    .Select(a => new KeyValuePair<int, string>(a.AgencyId, a.AgencyName)).ToList()
            };
            return bookingInfo;
        }

        public List<ArrestSentenceMethod> GetSentenceMethodInfo()
        {
            List<ArrestSentenceMethod> arrestSentenceMethodList =
                _context.ArrestSentenceMethod.Where(w => w.InactiveFlag != 1).ToList();
            return arrestSentenceMethodList;
        }

        // Get Intake Search Details based on requested information
        public List<SearchResult> GetBookingSearchList(SearchRequestVm searchDetails)
        {
            bool isToSearchDob = searchDetails.DateOfBirth.HasValue &&
                                 searchDetails.DateOfBirth > DateTime.MinValue;
            int heightFrom = searchDetails.HSfrom + searchDetails.HPfrom * 12;
            int heightTo = searchDetails.HSto + searchDetails.HPto * 12;

            List<SearchResult> personDetailsLst = _context.Inmate.Where(inm =>
                    (searchDetails.FacilityId == 0
                     || inm.FacilityId == searchDetails.FacilityId)
                    && (!searchDetails.ActiveOnly || inm.InmateActive == 1)
                && (string.IsNullOrEmpty(searchDetails.InmateNumber) || !searchDetails.exactonly
                        || (inm.InmateNumber == searchDetails.InmateNumber
                            || inm.Person.Aka.Any(aka =>
                                aka.AkaInmateNumber == searchDetails.InmateNumber)
                            )
                    )
                    && (string.IsNullOrEmpty(searchDetails.InmateSiteNumber) || searchDetails.exactonly                        
                        || (inm.InmateSiteNumber.StartsWith(searchDetails.InmateSiteNumber)
                            || inm.Person.Aka.Any(aka =>
                                aka.AkaSiteInmateNumber.StartsWith(searchDetails.InmateSiteNumber)
                            ))
                    )
                && (string.IsNullOrEmpty(searchDetails.InmateSiteNumber) || !searchDetails.exactonly
                        || (inm.InmateSiteNumber == searchDetails.InmateSiteNumber
                            || inm.Person.Aka.Any(aka =>
                                aka.AkaSiteInmateNumber == searchDetails.InmateSiteNumber)
                            )
                    )
                    && (string.IsNullOrEmpty(searchDetails.InmateSiteNumber) || searchDetails.exactonly                        
                        || (inm.InmateSiteNumber.StartsWith(searchDetails.InmateSiteNumber)
                            || inm.Person.Aka.Any(aka =>
                                aka.AkaSiteInmateNumber.StartsWith(searchDetails.InmateSiteNumber)
                            ))
                    )
                    && (string.IsNullOrEmpty(searchDetails.Category)
                        || inm.Person.PersonDescriptor.Any(pd => pd.Category == searchDetails.Category))
                    && (string.IsNullOrEmpty(searchDetails.ItemCode)
                        || inm.Person.PersonDescriptor.Any(pd => pd.Code == searchDetails.ItemCode))
                    && (string.IsNullOrEmpty(searchDetails.Location)
                        || inm.Person.PersonDescriptor.Any(pd => pd.ItemLocation == searchDetails.Location))
                    && (string.IsNullOrEmpty(searchDetails.Descriptor)
                        || inm.Person.PersonDescriptor.Any(pd => pd.DescriptorText == searchDetails.Descriptor))
                    && (string.IsNullOrEmpty(searchDetails.FirstName)
                        || (inm.Person.PersonFirstName.StartsWith(searchDetails.FirstName)
                            || inm.Person.Aka.Any(aka =>
                                aka.AkaFirstName.StartsWith(searchDetails.FirstName))))
                    && (string.IsNullOrEmpty(searchDetails.LastName)
                        || (inm.Person.PersonLastName.StartsWith(searchDetails.LastName)
                            || inm.Person.Aka.Any(aka =>
                                aka.AkaLastName.StartsWith(searchDetails.LastName))))
                    && (string.IsNullOrEmpty(searchDetails.MiddleName)
                        || (inm.Person.PersonMiddleName.StartsWith(searchDetails.MiddleName)
                            || inm.Person.Aka.Any(aka =>
                                aka.AkaMiddleName.StartsWith(searchDetails.MiddleName))))
                    && (string.IsNullOrEmpty(searchDetails.cityofBirth)
                        || inm.Person.PersonPlaceOfBirth.StartsWith(searchDetails.cityofBirth))
                    && (string.IsNullOrEmpty(searchDetails.stateorCountryOfBirth)
                        || inm.Person.PersonPlaceOfBirthList.StartsWith(searchDetails.stateorCountryOfBirth))
                && (string.IsNullOrEmpty(searchDetails.OtherId) || !searchDetails.exactonly
                        || (inm.Person.PersonOtherIdNumber.ToUpper() == searchDetails.OtherId.ToUpper()
                            || inm.Person.Aka.Any(aka =>
                                aka.AkaOtherIdNumber.ToUpper() == searchDetails.OtherId.ToUpper())
                            )
                    )
                    && (string.IsNullOrEmpty(searchDetails.OtherId) || searchDetails.exactonly                        
                        || (inm.Person.PersonOtherIdNumber.ToUpper().StartsWith(searchDetails.OtherId.ToUpper())
                            || inm.Person.Aka.Any(aka =>
                                aka.AkaOtherIdNumber.ToUpper().StartsWith(searchDetails.OtherId.ToUpper())
                            ))
                    )
                 && (string.IsNullOrEmpty(searchDetails.DlNumber) || !searchDetails.exactonly
                        || (inm.Person.PersonDlNumber.ToUpper() == searchDetails.DlNumber.ToUpper()
                            || inm.Person.Aka.Any(aka =>
                                aka.AkaDl.ToUpper() == searchDetails.DlNumber.ToUpper())
                            )
                    )
                    && (string.IsNullOrEmpty(searchDetails.DlNumber) || searchDetails.exactonly                        
                        || (inm.Person.PersonDlNumber.ToUpper().StartsWith(searchDetails.DlNumber.ToUpper())
                            || inm.Person.Aka.Any(aka =>
                                aka.AkaDl.ToUpper().StartsWith(searchDetails.DlNumber.ToUpper())
                            ))
                    )
                && (string.IsNullOrEmpty(searchDetails.CiiNumber) || !searchDetails.exactonly
                        || (inm.Person.PersonCii.ToUpper() == searchDetails.CiiNumber.ToUpper()
                            || inm.Person.Aka.Any(aka =>
                                aka.AkaCii.ToUpper() == searchDetails.CiiNumber.ToUpper())
                            )
                    )
                    && (string.IsNullOrEmpty(searchDetails.CiiNumber) || searchDetails.exactonly                        
                        || (inm.Person.PersonCii.ToUpper().StartsWith(searchDetails.CiiNumber.ToUpper())
                            || inm.Person.Aka.Any(aka =>
                                aka.AkaCii.ToUpper().StartsWith(searchDetails.CiiNumber.ToUpper())
                            ))
                    )
                    
                    && (string.IsNullOrEmpty(searchDetails.FbiNumber) || !searchDetails.exactonly
                        || (inm.Person.PersonFbiNo.ToUpper() == searchDetails.FbiNumber.ToUpper()
                            || inm.Person.Aka.Any(aka =>
                                aka.AkaFbi.ToUpper() == searchDetails.FbiNumber.ToUpper())
                            )
                    )
                    && (string.IsNullOrEmpty(searchDetails.FbiNumber) || searchDetails.exactonly                        
                        || (inm.Person.PersonFbiNo.ToUpper().StartsWith(searchDetails.FbiNumber.ToUpper())
                            || inm.Person.Aka.Any(aka =>
                                aka.AkaFbi.ToUpper().StartsWith(searchDetails.FbiNumber.ToUpper())
                            ))
                    )
                    
                    && (string.IsNullOrEmpty(searchDetails.DocNumber) || !searchDetails.exactonly
                        || (inm.Person.PersonDoc.ToUpper() == searchDetails.DocNumber.ToUpper()
                            || inm.Person.Aka.Any(aka =>
                                aka.AkaDoc.ToUpper() == searchDetails.DocNumber.ToUpper())
                            )
                    )
                    && (string.IsNullOrEmpty(searchDetails.DocNumber) || searchDetails.exactonly                        
                        || (inm.Person.PersonDoc.ToUpper().StartsWith(searchDetails.DocNumber.ToUpper())
                            || inm.Person.Aka.Any(aka =>
                                aka.AkaDoc.ToUpper().StartsWith(searchDetails.DocNumber.ToUpper())
                            ))
                    )
                    && (string.IsNullOrEmpty(searchDetails.AfisNumber) || !searchDetails.exactonly
                        || (inm.Person.AfisNumber.ToUpper() == searchDetails.AfisNumber.ToUpper()
                            || inm.Person.Aka.Any(aka =>
                                aka.AkaAfisNumber.ToUpper() == searchDetails.AfisNumber.ToUpper())
                            )
                    )
                    && (string.IsNullOrEmpty(searchDetails.AfisNumber) || searchDetails.exactonly                        
                        || (inm.Person.AfisNumber.ToUpper().StartsWith(searchDetails.AfisNumber.ToUpper())
                            || inm.Person.Aka.Any(aka =>
                                aka.AkaAfisNumber.ToUpper().StartsWith(searchDetails.AfisNumber.ToUpper())
                            ))
                    )
                    // // // ssn will be searched by plain text not masked text
                    // // // application will allow to store ssn as plain text
                    && (string.IsNullOrEmpty(searchDetails.Ssn) || !searchDetails.exactonly
                        || (inm.Person.PersonSsn.ToUpper() == searchDetails.Ssn.ToUpper()
                            || inm.Person.Aka.Any(aka =>
                                aka.AkaSsn.ToUpper() == searchDetails.Ssn.ToUpper())
                            )
                    )
                    && (string.IsNullOrEmpty(searchDetails.Ssn) || searchDetails.exactonly                        
                        || (inm.Person.PersonSsn.ToUpper().StartsWith(searchDetails.Ssn.ToUpper())
                            || inm.Person.Aka.Any(aka =>
                                aka.AkaSsn.ToUpper().StartsWith(searchDetails.Ssn.ToUpper())
                            ))
                    )
                    && (string.IsNullOrEmpty(searchDetails.AlienNumber) || !searchDetails.exactonly
                        || (inm.Person.PersonAlienNo.ToUpper() == searchDetails.AlienNumber.ToUpper()
                            || inm.Person.Aka.Any(aka =>
                                aka.AkaAlienNo.ToUpper() == searchDetails.AlienNumber.ToUpper())
                            )
                    )
                    && (string.IsNullOrEmpty(searchDetails.AlienNumber) || searchDetails.exactonly                        
                        || (inm.Person.PersonAlienNo.ToUpper().StartsWith(searchDetails.AlienNumber.ToUpper())
                            || inm.Person.Aka.Any(aka =>
                                aka.AkaAlienNo.ToUpper().StartsWith(searchDetails.AlienNumber.ToUpper())
                            ))
                    )
                    && (!isToSearchDob
                        || (inm.Person.PersonDob.Value.Date == searchDetails.DateOfBirth.Value.Date
                            || inm.Person.Aka.Any(aka =>
                                aka.AkaDob.Value.Date == searchDetails.DateOfBirth.Value.Date)))
                    // // // phone number will be searched by plain text not masked text
                    // // // application will allow to store phone number as plain text
                    && (string.IsNullOrEmpty(searchDetails.PhoneCode) || !searchDetails.exactonly
                        || (inm.Person.PersonCellPhone == searchDetails.PhoneCode
                        || inm.Person.PersonPhone == searchDetails.PhoneCode
                        || inm.Person.PersonPhone2 == searchDetails.PhoneCode
                        || inm.Person.PersonBusinessPhone == searchDetails.PhoneCode
                        || inm.Person.PersonBusinessFax == searchDetails.PhoneCode
                        || inm.Person.Aka.Any(aka =>
                                aka.AkaOtherPhoneNumber == searchDetails.PhoneCode))
                    )
                    && (string.IsNullOrEmpty(searchDetails.PhoneCode) || searchDetails.exactonly                        
                        || (inm.Person.PersonCellPhone.StartsWith(searchDetails.PhoneCode)
                        || inm.Person.PersonPhone.StartsWith(searchDetails.PhoneCode)
                        || inm.Person.PersonPhone2.StartsWith(searchDetails.PhoneCode)
                        || inm.Person.PersonBusinessPhone.StartsWith(searchDetails.PhoneCode)
                        || inm.Person.PersonBusinessFax.StartsWith(searchDetails.PhoneCode)
                        || inm.Person.Aka.Any(aka =>
                                aka.AkaOtherPhoneNumber.StartsWith(searchDetails.PhoneCode)))
                    )
                    // // // checking characteristic details
                    && (searchDetails.Eyecolor == 0 || inm.Person.PersonEyeColorLast == searchDetails.Eyecolor)
                    && (searchDetails.Haircolor == 0 || inm.Person.PersonHairColorLast == searchDetails.Haircolor)
                    && (searchDetails.Race == 0 || inm.Person.PersonRaceLast == searchDetails.Race)
                    && (searchDetails.gender == 0 || inm.Person.PersonSexLast == searchDetails.gender)
                    && (searchDetails.Weightfrom == 0
                        || (searchDetails.Weightto == 0
                            || (inm.Person.PersonWeightLast >= searchDetails.Weightfrom
                                && inm.Person.PersonWeightLast <= searchDetails.Weightto))
                        || (searchDetails.Weightto > 0
                            || inm.Person.PersonWeightLast == searchDetails.Weightfrom))
                    && (searchDetails.HPfrom == 0
                        || (heightTo == 0
                            || (inm.Person.PersonHeightPrimaryLast * 12 + inm.Person.PersonHeightSecondaryLast <=
                                heightTo
                                && inm.Person.PersonHeightPrimaryLast * 12 + inm.Person.PersonHeightSecondaryLast >=
                                heightFrom))
                        || (heightTo > 0
                            || inm.Person.PersonHeightPrimaryLast * 12 + inm.Person.PersonHeightSecondaryLast ==
                            heightFrom))
                    && (string.IsNullOrEmpty(searchDetails.Occupation)
                        || inm.Person.PersonDescription.Any(pdn =>
                            pdn.PersonOccupation.Contains(searchDetails.Occupation)))
                    // checking address details
                    && (string.IsNullOrEmpty(searchDetails.AddressNumber)
                        || inm.Person.Address.Any(add => add.AddressNumber == searchDetails.AddressNumber))
                    && (string.IsNullOrEmpty(searchDetails.AddressDirection)
                        || inm.Person.Address.Any(add => add.AddressDirection == searchDetails.AddressDirection))
                    && (string.IsNullOrEmpty(searchDetails.DirectionSuffix)
                        || inm.Person.Address.Any(add => add.AddressDirectionSuffix == searchDetails.DirectionSuffix))
                    && (string.IsNullOrEmpty(searchDetails.AddressStreet) // need to implement soundex
                        || inm.Person.Address.Any(add => add.AddressStreet == searchDetails.AddressStreet))
                    && (string.IsNullOrEmpty(searchDetails.AddressCity) // need to implement soundex
                        || inm.Person.Address.Any(add => add.AddressCity == searchDetails.AddressCity))
                    && (string.IsNullOrEmpty(searchDetails.AddressSuffix)
                        || inm.Person.Address.Any(add => add.AddressSuffix == searchDetails.AddressSuffix))
                    && (string.IsNullOrEmpty(searchDetails.AddressUnitType)
                        || inm.Person.Address.Any(add => add.AddressUnitType == searchDetails.AddressUnitType))
                    && (string.IsNullOrEmpty(searchDetails.AddressUnitNumber)
                        || inm.Person.Address.Any(add => add.AddressUnitNumber == searchDetails.AddressUnitNumber))
                    && (string.IsNullOrEmpty(searchDetails.AddressState)
                        || inm.Person.Address.Any(add => add.AddressState == searchDetails.AddressState))
                    && (string.IsNullOrEmpty(searchDetails.AddressZip)
                        || inm.Person.Address.Any(add => add.AddressZip == searchDetails.AddressZip))
                    && (string.IsNullOrEmpty(searchDetails.personOccupation)
                        || inm.Person.Address.Any(add => add.PersonOccupation == searchDetails.personOccupation))
                    && (string.IsNullOrEmpty(searchDetails.employer)
                        || inm.Person.Address.Any(add => add.PersonEmployer == searchDetails.employer))
                    && (string.IsNullOrEmpty(searchDetails.AddressType)
                        || inm.Person.Address.Any(add => add.AddressType == searchDetails.AddressType))
                    && (string.IsNullOrEmpty(searchDetails.AddressLine2)
                        || inm.Person.Address.Any(add =>
                            (string.IsNullOrEmpty(add.AddressLine2)
                             || add.AddressLine2.ToLower().Contains(searchDetails.AddressLine2))))
                    // Below condition has been used in V1 but given document not having. 
                    // Refer JIRA 3503: Move Search Module to Top Level Menu Enhancement
                    // So i commented this. Future, if we need we will used it
                    //&& (!searchDetails.Homeless || inm.Person.Address.Any(add => add.AddressHomeless))
                    //&& (!searchDetails.Transient || inm.Person.Address.Any(add => add.AddressTransient))
                    //&& (!searchDetails.Refused || inm.Person.Address.Any(add => add.AddressRefused))

                    // checking profile search deatils
                    && (searchDetails.maritalStatus == 0
                        || inm.Person.PersonDescription.Any(pro =>
                            pro.PersonMaritalStatus == searchDetails.maritalStatus))
                    && (searchDetails.ethnicity == 0
                        || inm.Person.PersonDescription.Any(pro => pro.PersonEthnicity == searchDetails.ethnicity))
                    && (searchDetails.primLang == 0
                        || inm.Person.PersonDescription.Any(pro => pro.PersonPrimaryLanguage == searchDetails.primLang)
                    )
                    && (string.IsNullOrEmpty(searchDetails.religion)
                        || inm.Person.PersonReligion == searchDetails.religion)
                    && (string.IsNullOrEmpty(searchDetails.genderIdentity)
                        || inm.Person.PersonGenderIdentity == searchDetails.genderIdentity)
                    && (string.IsNullOrEmpty(searchDetails.eduGrade)
                        || inm.Person.PersonEduGrade == searchDetails.eduGrade)
                    && (string.IsNullOrEmpty(searchDetails.eduDegree)
                        || inm.Person.PersonEduDegree == searchDetails.eduDegree)
                    && (string.IsNullOrEmpty(searchDetails.medInsuranceProvider)
                        || inm.Person.PersonMedInsuranceProvider == searchDetails.medInsuranceProvider)
                    && (string.IsNullOrEmpty(searchDetails.eduDiscipline)
                        || inm.Person.PersonEduDiscipline == searchDetails.eduDiscipline)
                    && (string.IsNullOrEmpty(searchDetails.medInsurancePolicyNo)
                        || inm.Person.PersonMedInsurancePolicyNo == searchDetails.medInsurancePolicyNo)
                    && (string.IsNullOrEmpty(searchDetails.skillTrade)
                        || inm.Person.PersonSkillAndTrade.Any(pro => pro.PersonSkillTrade == searchDetails.skillTrade))
                    // checking inmate search details
                    && (searchDetails.InmateSearchFacilityId == 0
                        || (inm.FacilityId == searchDetails.InmateSearchFacilityId))
                    && (string.IsNullOrEmpty(searchDetails.bunkId)
                        || inm.HousingUnitId > 0
                        && inm.HousingUnit.HousingUnitBedLocation == searchDetails.bunkId
                        && (!inm.HousingUnit.HousingUnitInactive.HasValue ||
                            inm.HousingUnit.HousingUnitInactive == 0))
                    && (string.IsNullOrEmpty(searchDetails.cellId)
                        || inm.HousingUnitId > 0
                        && inm.HousingUnit.HousingUnitBedNumber == searchDetails.cellId
                        && (!inm.HousingUnit.HousingUnitInactive.HasValue ||
                            inm.HousingUnit.HousingUnitInactive == 0))
                    && (searchDetails.podId == 0
                        || inm.HousingUnitId > 0
                        && inm.HousingUnit.HousingUnitListId == searchDetails.podId
                        && (!inm.HousingUnit.HousingUnitInactive.HasValue ||
                            inm.HousingUnit.HousingUnitInactive == 0))
                    && (string.IsNullOrEmpty(searchDetails.BuildingId)
                        || inm.HousingUnitId > 0
                        && inm.HousingUnit.HousingUnitLocation == searchDetails.BuildingId
                        && (!inm.HousingUnit.HousingUnitInactive.HasValue ||
                            inm.HousingUnit.HousingUnitInactive == 0))
                    && (string.IsNullOrEmpty(searchDetails.classificationId)
                        || (inm.Incarceration.Any(inc => !inc.ReleaseOut.HasValue)
                        && inm.InmateClassification.InmateClassificationReason == searchDetails.classificationId))
                    && (searchDetails.locationId == 0
                        || (inm.InmateCurrentTrackId == searchDetails.locationId))
                    && (searchDetails.personFlagId == 0
                        || inm.Person.PersonFlag.Any(pf =>
                            pf.PersonFlagIndex == searchDetails.personFlagId && pf.DeleteFlag == 0))
                    && (searchDetails.inmateFlagId == 0
                        || inm.Person.PersonFlag.Any(pf =>
                            pf.InmateFlagIndex == searchDetails.inmateFlagId && pf.DeleteFlag == 0))
                    && (searchDetails.dietFlagId == 0
                        || inm.Person.PersonFlag.Any(
                            pf => pf.DietFlagIndex == searchDetails.dietFlagId && pf.DeleteFlag == 0))
                            && (searchDetails.medFlagId == 0
                        || inm.Person.PersonFlag.Any(
                            pf => pf.MedicalFlagIndex == searchDetails.medFlagId && pf.DeleteFlag == 0))
                ).Distinct().Take(10000)
                .Select(inm => new SearchResult
                {
                    InmateId = inm.InmateId,
                    PersonId = inm.PersonId,
                    InmateNumber = inm.InmateNumber,
                    InmateActive = inm.InmateActive,
                    CaseCount = inm.Incarceration.Count,
                    FirstName = inm.Person.PersonFirstName,
                    MiddleName = inm.Person.PersonMiddleName,
                    LastName = inm.Person.PersonLastName,
                    Dob = inm.Person.PersonDob,
                    DlNumber = inm.Person.PersonDlNumber,
                    Personcii = inm.Person.PersonCii,
                    AfisNumber = inm.Person.AfisNumber,
                    PrimaryHeight = inm.Person.PersonHeightPrimaryLast,
                    SecondaryHeight = inm.Person.PersonHeightSecondaryLast,
                    Weight = inm.Person.PersonWeightLast,
                    Placeofbirth = inm.Person.PersonPlaceOfBirth,
                    SexLast = inm.Person.PersonSexLast,
                    PersonEyeColorLast = inm.Person.PersonEyeColorLast,
                    PersonHairColorLast = inm.Person.PersonHairColorLast,
                    PersonRaceLast = inm.Person.PersonRaceLast,
                    PersonAge = searchDetails.Agefrom != 0 ? GetPersonAge(inm.Person.PersonDob) : 0
                    //Photofilepath = _photos.GetPhotoByIdentifier(inm.Arrest.IncarcerationArrestXref.Select(s =>
                    //        s.Incarceration.Inmate.Person.Identifiers.LastOrDefault(idn =>
                    //            idn.IdentifierType == "1" && idn.DeleteFlag == 0)
                    //    ).FirstOrDefault())
                }).ToList();

            //List<Identifiers> identifierLst = _context.Identifiers
            //    .Where(idf => idf.IdentifierType == "1"
            //                  && idf.DeleteFlag == 0
            //                  && _personDetailsLst.Select(a => a.PersonId).Contains(idf.PersonId ?? 0))
            //    .Select(idf => new Identifiers
            //    {
            //        PersonId = idf.PersonId,
            //        IdentifierId = idf.IdentifierId,
            //        PhotographRelativePath = _photos.GetPhotoByIdentifier(idf)
            //    }).OrderByDescending(o => o.IdentifierId).ToList();

            if (searchDetails.Agefrom != 0)
            {
                personDetailsLst = personDetailsLst
                    .Where(pra => (searchDetails.Ageto == 0
                                   || pra.PersonAge >= searchDetails.Agefrom && pra.PersonAge <= searchDetails.Ageto)
                                  && (searchDetails.Ageto != 0 || pra.PersonAge == searchDetails.Agefrom))
                    .ToList();
            }

            return personDetailsLst;
        }

        public IQueryable<Lookup> GetLookups()
        {
            IQueryable<Lookup> lookups = _context.Lookup
                .Where(w => (w.LookupType == LookupConstants.HAIRCOL || w.LookupType == LookupConstants.EYECOLOR ||
                             w.LookupType == LookupConstants.SEX || w.LookupType == LookupConstants.RACE ||
                             w.LookupType == LookupConstants.ARRTYPE
                             || w.LookupType == LookupConstants.CRIMETYPE));
            return lookups;
        }

        private static int GetPersonAge(DateTime? personDob)
        {
            if (!personDob.HasValue) return 0;
            DateTime todayDate = DateTime.Now.Date;
            int personAge = todayDate.Year - personDob.Value.Year;
            if (personDob > todayDate.AddYears(-personAge))
                personAge--;
            return personAge;
        }

        public List<AgencyVm> GetAgencies()
        {
            List<AgencyVm> agency = _context.Agency.Where(ag =>
                    !ag.AgencyInactiveFlag.HasValue || ag.AgencyInactiveFlag == 0)
                .Select(ag => new AgencyVm
                {
                    AgencyAbbreviation = ag.AgencyAbbreviation,
                    AgencyId = ag.AgencyId,
                    AgencyName = ag.AgencyName,
                    AgencyInactiveFlag = ag.AgencyInactiveFlag,
                    AgencyCourtFlag = ag.AgencyCourtFlag,
                    AgencyArrestingFlag = ag.AgencyArrestingFlag
                }).OrderBy(o => o.AgencyName).ToList();
            return agency;
        }

        public List<CrimeLookupFlag> GetChargeFlag()
        {
            List<CrimeLookupFlag> chargeFlag = _context.CrimeLookupFlag.Select(s => new CrimeLookupFlag()
            {
                FlagName = s.FlagName,
                CrimeLookupFlagId = s.CrimeLookupFlagId
            }).Where(o => o.InactiveFlag != 1).OrderBy(o => o.FlagName).ToList();
            return chargeFlag;
        }

        public string GetCaseType(string arrestType)
        {
            IQueryable<Lookup> lookupsCase = _context.Lookup
                .Where(w => (w.LookupType == LookupConstants.ARRTYPE));
            var caseType = lookupsCase.SingleOrDefault(
                lkp => lkp.LookupIndex.ToString() == arrestType.Trim())?.LookupDescription;
            return caseType;
        }
    }
}