using System;
using GenerateTables.Models;
using ServerAPI.ViewModels;
using System.Linq;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using ServerAPI.Utilities;
using System.IO;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Data.SqlClient;
using Newtonsoft.Json.Linq;

// ReSharper disable once CheckNamespace
namespace ServerAPI.Services
{
    public class CommonService : ICommonService
    {
        private readonly AAtims _context;
        private IConfiguration Configuration { get; }
        private readonly int _personnelId;
        private readonly IPersonService _personService;
        private readonly IAppletsSavedService _appletsSavedService;
        private readonly IInterfaceEngineService _interfaceEngineService;

        public CommonService(AAtims context, IConfiguration configuration,
            IHttpContextAccessor ihHttpContextAccessor,
            IPersonService personService, IAppletsSavedService appletsSavedService,
            IInterfaceEngineService interfaceEngineService)
        {
            _context = context;
            Configuration = configuration;
            _personnelId = Convert.ToInt32(ihHttpContextAccessor.HttpContext.User
                .FindFirst(AuthDetailConstants.PERSONNELID)?.Value);
            _personService = personService;
            _appletsSavedService = appletsSavedService;
            _interfaceEngineService = interfaceEngineService;
        }

        //Get Agency List by Agency Id's
        public List<AgencyVm> GetAgencyNameList(List<int> agencyIds) =>
            _context.Agency.Where(agg => agencyIds.Contains(agg.AgencyId)).Select(agg => new AgencyVm
            {
                AgencyId = agg.AgencyId,
                AgencyName = agg.AgencyName,
                AgencyAbbreviation = agg.AgencyAbbreviation
            }).ToList();

        public List<Lookup> GetLookupList(string lookupType) => _context.Lookup.Where(look =>
            look.LookupInactive == 0 && look.LookupType == lookupType).ToList();

        //To get Facility details
        public List<Facility> GetFacilities() => _context.Facility.Where(f => f.DeleteFlag == 0).ToList();

        //To get Site Options
        public string GetSiteOptionValue(string optionName, string optionVariable)
        {
            IQueryable<SiteOptions> siteOptions = _context.SiteOptions.Where(so => so.SiteOptionsStatus == "1");

            if (!string.IsNullOrEmpty(optionName))
            {
                siteOptions = siteOptions.Where(so => so.SiteOptionsName == optionName);
            }

            if (!string.IsNullOrEmpty(optionVariable))
            {
                siteOptions = siteOptions.Where(so => so.SiteOptionsVariable == optionVariable);
            }

            return (from so in siteOptions select so.SiteOptionsValue).FirstOrDefault();
        }

        public List<SiteOptions> GetSiteOptions(string[] siteOptionNames)
        {
            List<SiteOptions> siteOptions = _context.SiteOptions
                .Where(a => siteOptionNames.Contains(a.SiteOptionsName)).ToList();
            return siteOptions;
        }

        //Get SiteOptionValue from SiteOption without check SiteOptionsStatus
        public string GetSiteOptionValue(string optionName) => !string.IsNullOrEmpty(optionName)
            ? _context.SiteOptions.SingleOrDefault(sop => sop.SiteOptionsName == optionName.ToUpper())?.SiteOptionsValue
            : string.Empty;

        //To get lookup description from lookup table
        public List<KeyValuePair<int, string>> GetLookupKeyValuePairs(string lookupType) =>
            _context.Lookup.Where(look => look.LookupInactive == 0 && look.LookupType == lookupType)
                .Select(look => new KeyValuePair<int, string>
                (
                    look.LookupIndex,
                    look.LookupDescription
                )).ToList();

        //Log into Web Service Event Type and History
        public void EventHandle(EventVm evenHandle)
        {
            bool exists = _context.WebServiceEventSetting.Any(x => x.EventQueueFlag == 1);

            WebServiceEventType webServiceEventType = _context.WebServiceEventType
                .SingleOrDefault(a => a.WebServiceEventName == evenHandle.EventName.Trim());

            if (!exists || webServiceEventType == null) return;
            _context.WebServiceEventAssign.Where(x =>
                    x.WebServiceEventTypeId == webServiceEventType.WebServiceEventTypeId &&
                    (string.IsNullOrEmpty(evenHandle.PersonFlagLookupType) ||
                     x.PersonFlagLookupType == evenHandle.PersonFlagLookupType ||
                     string.IsNullOrEmpty(x.PersonFlagLookupType))
                    && (evenHandle.PersonFlagLookupIndex == 0 ||
                        x.PersonFlagLookupIndex == evenHandle.PersonFlagLookupIndex ||
                        x.PersonFlagLookupIndex == 0)
                    && (string.IsNullOrEmpty(evenHandle.FormTagName) || x.FormTagName == evenHandle.FormTagName)
                    && (evenHandle.FormTemplateId == 0 || x.FormTemplatesId == evenHandle.FormTemplateId)
                    && (evenHandle.WebServiceEventAssignId == 0 ||
                        x.WebServiceEventAssignId == evenHandle.WebServiceEventAssignId)
                    && x.WebServiceEventInactive == 0)
                .Select(y => new EventAssign
                {
                    WebServiceEventAssignId = y.WebServiceEventAssignId,
                    WebServiceEventAuthId = y.WebServiceEventAuthId,
                    WebServiceEventExportId = y.WebServiceEventExportId
                }).ToList().ForEach(evt =>
                {
                    WebServiceEventQueue webServiceEventQueue = new WebServiceEventQueue
                    {
                        WebServiceEventAssignId = evt.WebServiceEventAssignId,
                        CreateDate = DateTime.Now,
                        CreateBy = 1,
                        WebServiceEventParameter1 = evenHandle.PersonId.ToString(),
                        WebServiceEventParameter2 = evenHandle.CorresId.ToString()
                    };
                    _context.WebServiceEventQueue.Add(webServiceEventQueue);
                });

            if (!webServiceEventType.WebServiceEventRunHistory.HasValue ||
                webServiceEventType.WebServiceEventRunHistory.Value != 1) return;
            WebServiceEventTypeHistory webServiceEventTypeHistory = new WebServiceEventTypeHistory
            {
                WebServiceEventTypeId = webServiceEventType.WebServiceEventTypeId,
                CreateBy = 1,
                CreateDate = DateTime.Now,
                WebServiceEventParameter1 = evenHandle.PersonId.ToString(),
                WebServiceEventParameter2 = evenHandle.CorresId.ToString()
            };
            _context.Add(webServiceEventTypeHistory);
        }

        //To get Global Number for booking,pre-book etc,.
        public string GetGlobalNumber(int atimsNumberId, bool suppBooking = false, int incarcerationId = 0)
        {
            AtimsNumber atimsNumber = _context.AtimsNumber.Single(w => w.AtimsNumberId == atimsNumberId);
            if (atimsNumber == null) return null;

            int counter = atimsNumber.AtimsNumberCounter ?? 0;

            if (suppBooking && atimsNumber.AtimsNumberAllowSequence == 0 || !suppBooking)
            {
                counter++;
            }

            string globalNumber = suppBooking && atimsNumber.AtimsNumberAllowSequence == 1
                ? GetGlobalNumberSuppBookingWithAllowSequence(incarcerationId)
                : GetGlobalNumberGeneric(atimsNumber, counter);

            return globalNumber;
        }

        private string GetGlobalNumberSuppBookingWithAllowSequence(int incarcerationId = 0)
        {
            int seqNumber = 0;

            string originalBooking = _context.IncarcerationArrestXref
                .OrderByDescending(y => y.Arrest.ArrestId)
                .Where(x => x.IncarcerationId == incarcerationId).Select(s => s.Arrest.ArrestBookingNo)
                .FirstOrDefault();

            if (!string.IsNullOrEmpty(originalBooking))
            {
                int sequenceNo = 1;
                if (originalBooking.IndexOf('.') > 0)
                {
                    int seqIndex = originalBooking.IndexOf(".", StringComparison.Ordinal) + 1;
                    sequenceNo = Convert.ToInt32(originalBooking.Substring(seqIndex)) + 1;
                    originalBooking = $"{originalBooking.Substring(0, originalBooking.IndexOf('.'))}.{sequenceNo}";
                }
                else
                {
                    originalBooking = $"{originalBooking}.{sequenceNo}";
                }

                //Check for Duplicate Entry in Arrest table
                if (_context.Arrest.Any(x => x.ArrestBookingNo == $"{originalBooking}"))
                {
                    originalBooking = $"{originalBooking.Substring(1, originalBooking.IndexOf('.'))}{sequenceNo + 1}";
                }

                seqNumber = Convert.ToInt32(originalBooking.Split('.').ElementAt(1));
            }

            return !string.IsNullOrEmpty(originalBooking)
                ? originalBooking.Replace(string.Format(originalBooking.Split('.')[1]), seqNumber.ToString())
                : "";
        }

        private string GetGlobalNumberGeneric(AtimsNumber atimsNumber, int counter)
        {
            string strCounter = atimsNumber.AtimsNumberPadding == 1
                                && counter.ToString().Length < atimsNumber.AtimsNumberPaddingLen
                ? counter.ToString("D" + atimsNumber.AtimsNumberPaddingLen)
                : counter.ToString();

            string checkYear = atimsNumber.AtimsNumberUseYear == 1
                ? DateTime.Now.Year.ToString().Substring(2, 2)
                : null;

            if (atimsNumber.AtimsNumberUseYear == 1
                && !atimsNumber.AtimsNumberYearCompare.Equals(DateTime.Now.Year.ToString())
                && atimsNumber.AtimsNumberYearResetCounter == 1)
            {
                atimsNumber.AtimsNumberCurrentYear = checkYear;
                atimsNumber.AtimsNumberYearCompare = checkYear;
            }

            //concatenation using string interpolation
            string globalNumber =
                $"{atimsNumber.AtimsNumberPrefix?.Trim()}{checkYear}{strCounter}{atimsNumber.AtimsNumberSuffix?.Trim()}";

            // Check for Duplicate Entry in Arrest table
            bool exists = _context.Arrest.Any(x => x.ArrestBookingNo == globalNumber);

            // Check for Duplicate Entry in Incarceration table
            if (exists)
            {
                exists = _context.Incarceration.Any(x => x.BookingNo == globalNumber);
            }

            while (exists)
            {
                counter += 1;

                string strSeqCounter = atimsNumber.AtimsNumberPadding == 1
                                       && counter.ToString().Length < atimsNumber.AtimsNumberPaddingLen
                    ? counter.ToString("D" + atimsNumber.AtimsNumberPaddingLen)
                    : counter.ToString();

                //concatenation using string interpolation
                globalNumber =
                    $"{atimsNumber.AtimsNumberPrefix}{checkYear}{strSeqCounter}{atimsNumber.AtimsNumberSuffix}";

                exists = _context.Arrest.Any(x => x.ArrestBookingNo == globalNumber);
            }

            atimsNumber.AtimsNumberCounter = counter;
            _context.SaveChanges();
            return globalNumber;
        }

        // Get Age From Dob
        public int GetAgeFromDob(DateTime? personDob)
        {
            if (!personDob.HasValue) return 0;
            DateTime todayDate = DateTime.Now.Date;
            int personAge = todayDate.Year - personDob.Value.Year;
            if (personDob > todayDate.AddYears(-personAge))
                personAge--;
            return personAge;
        }

        public List<PersonnelVm> GetPersonnel(string type)
        {
            List<PersonnelVm> personnelVms = _context.Personnel
                .Where(w => !w.PersonnelTerminationFlag && (type != OfficerFlag.ARRESTTRANSPORT || w.ArrestTransportOfficerFlag)
                     && (type != OfficerFlag.RECEIVESEARCH || w.ReceiveSearchOfficerFlag))
                .Select(s => new PersonnelVm
                {
                    ArrestTransportOfficerFlag = s.ArrestTransportOfficerFlag,
                    PersonId = s.PersonId,
                    PersonnelId = s.PersonnelId,
                    OfficerBadgeNumber = s.OfficerBadgeNum,
                    PersonnelAgencyFlag = s.PersonnelAgencyGroupFlag,
                    AgencyId = s.AgencyId
                }).AsEnumerable().OrderBy(s => s.PersonLastName).ToList();
            int[] personIds = personnelVms.Select(a => a.PersonId).ToArray();
            List<PersonInfoVm> lstPersonInfoVms = _context.Person.Where(a => personIds.Contains(a.PersonId))
                .Select(s => new PersonInfoVm
                {
                    PersonId = s.PersonId,
                    PersonLastName = s.PersonLastName,
                    PersonFirstName = s.PersonFirstName,
                    PersonMiddleName = s.PersonMiddleName
                }).ToList();
            personnelVms.ForEach(item =>
            {
                PersonInfoVm personInfoVm = lstPersonInfoVms.Single(a => a.PersonId == item.PersonId);
                item.PersonLastName = personInfoVm.PersonLastName;
                item.PersonFirstName = personInfoVm.PersonFirstName;
                item.PersonMiddleName = personInfoVm.PersonMiddleName;
                item.PersonnelFullDisplayName = GetPersonnelFullName(personInfoVm) +
                     (!string.IsNullOrEmpty(item.OfficerBadgeNumber) ? " " + item.OfficerBadgeNumber : "");
            });

            return personnelVms;
        }

        private string GetPersonnelFullName(PersonInfoVm person)
        {
            string fullName = "";
            if (!string.IsNullOrEmpty(person.PersonLastName))
                fullName += person.PersonLastName;
            if (!string.IsNullOrEmpty(person.PersonFirstName))
                fullName += ", " + person.PersonFirstName;
            return fullName;
        }

        public string UploadFile(IFormFile uploadedFile)
        {
            string savePath = "TempFiles";
            if (!Directory.Exists(savePath))
            {
                Directory.CreateDirectory(savePath);
            }

            savePath =
                $@"{savePath}\{uploadedFile.FileName.Split('.')[0]}_{DateTime.Now:yyyyMMddHHss}.{
                        uploadedFile.FileName.Split('.')[1]
                    }";
            FileStream fs = new FileStream(savePath, FileMode.Create);
            uploadedFile.CopyTo(fs);
            fs.Dispose();
            return savePath;
        }

        public void DeleteFile(string fileName)
        {
            string path = $@"TempFiles\{fileName}";
            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }

        public PrebookAttachment GetAttachment(int attachmentId) => _context.AppletsSaved
            .Where(x => x.AppletsSavedId == attachmentId && x.AppletsDeleteFlag == 0)
            .Select(y => new PrebookAttachment
            {
                AppletsSavedIsExternal = y.AppletsSavedIsExternal,
                AttachmentFile = y.AppletsSavedIsExternal ? _appletsSavedService.GetExternalPath() +
                y.AppletsSavedPath : _appletsSavedService.GetPath() + y.AppletsSavedPath,
            }).SingleOrDefault();

        public List<LookupVm> GetLookups(string[] values, bool includeInactive = false)
        {
            List<LookupVm> lookUp =
                (from lu in _context.Lookup
                 where values.Contains(lu.LookupType) && (includeInactive || lu.LookupInactive == 0)
                 select new LookupVm
                 {
                     LookupIdentity = lu.LookupIdentity,
                     LookupIndex = lu.LookupIndex,
                     LookupType = lu.LookupType,
                     LookupCategory = lu.LookupCategory,
                     LookupDescription = lu.LookupDescription,
                     LookupInactive = lu.LookupInactive == 1,
                     LookupName = lu.LookupName,
                     LookupColor = lu.LookupColor,
                     AlertAllowExpire = lu.LookupAlertAllowExpire == 1,
                     LookupNoAlert = lu.LookupNoAlert ?? 0,
                     AlertOrder = lu.LookupAlertOrder ?? 0,
                     LookupFlag6 = lu.LookupFlag6 ?? 0,
                     LookupFlag7 = lu.LookupFlag7 ?? 0,
                     LookupOrder = lu.LookupOrder,
                     LookupFlag8 = lu.LookupFlag8 ?? 0,
                     LookupFlag9 = lu.LookupFlag9 ?? 0,
                     LookupFlag10 = lu.LookupFlag10 ?? 0
                 }).OrderByDescending(lu => lu.LookupOrder).ThenBy(luk => luk.LookupDescription).ToList();
            return lookUp;
        }

        public List<LookupDropdownListVm> GetDropdownValues(string[] lookupTypes, string[] lookupReferences)
        {
            List<LookupDropdownListVm> lookUp =
                (from lu in _context.Lookup
                 where lookupTypes.Contains(lu.LookupType) && lu.LookupInactive == 0 &&
                     lookupReferences.Contains(lu.LookupReference1)
                 select new LookupDropdownListVm
                 {
                     Description = lu.LookupDescription,
                     Values = _context.LookupSubList
                         .Where(su => su.LookupIdentity == lu.LookupIdentity && su.DeleteFlag == false).Select(su =>
                             new LookupValues
                             {
                                 Value = su.SubListValue,
                                 Display = su.SubListValue
                             }).OrderBy(su => su.Display).ThenBy(su => su.Value).ToList()
                 }).OrderBy(lu => lu.Description).ToList();
            return lookUp;
        }

        public List<AgencyVm> GetAgenciesByType(string type) =>
            (from a in _context.Agency
             where type == "arrest" && a.AgencyArrestingFlag
                   || type == "bill" && a.AgencyBillingFlag
                   || type == "book" && a.AgencyBookingFlag
                   || type == "court" && a.AgencyCourtFlag

             select new AgencyVm
             {
                 AgencyId = a.AgencyId,
                 AgencyName = a.AgencyName,
                 AgencyAbbreviation = a.AgencyAbbreviation,
                 AgencyInactiveFlag = a.AgencyInactiveFlag,
                 AgencyArrestingFlag = a.AgencyArrestingFlag,
                 AgencyCourtFlag = a.AgencyCourtFlag
             }).ToList();

        public AgencyVm GetAgencyByOri(string oriNumber) =>
            (from a in _context.Agency
             where a.AgencyOriNumber == oriNumber
             select new AgencyVm
             {
                 AgencyId = a.AgencyId,
                 AgencyName = a.AgencyName,
                 AgencyAbbreviation = a.AgencyAbbreviation
             }).Single();

        // evidence agency 
        public List<AgencyVm> GetEvidenceAgency(bool arrestFlag, bool courtFlag)
        {
            IQueryable<AgencyVm> lstAgencyList =
                from a in _context.Agency
                where !a.AgencyInactiveFlag.HasValue
                select new AgencyVm
                {
                    AgencyId = a.AgencyId,
                    AgencyName = a.AgencyName,
                    AgencyArrestingFlag = a.AgencyArrestingFlag,
                    AgencyCourtFlag = a.AgencyCourtFlag
                };

            List<AgencyVm> lstAgencyArrest = (
                from s in lstAgencyList
                where s.AgencyArrestingFlag == arrestFlag && s.AgencyCourtFlag == courtFlag
                select new AgencyVm
                {
                    AgencyId = s.AgencyId,
                    AgencyName = s.AgencyName
                }).OrderBy(s => s.AgencyName).ToList();

            return lstAgencyArrest;
        }

        public List<PersonnelVm> GetPersonnelDetails() =>
            _context.Personnel.Where(w => !w.PersonnelTerminationFlag)
                .Select(s => new PersonnelVm
                {
                    ArrestTransportOfficerFlag = s.ArrestTransportOfficerFlag,
                    PersonFirstName = s.PersonNavigation.PersonFirstName,
                    PersonLastName = s.PersonNavigation.PersonLastName,
                    PersonMiddleName = s.PersonNavigation.PersonMiddleName,
                    PersonId = s.PersonId,
                    PersonnelId = s.PersonnelId,
                    OfficerBadgeNumber = s.OfficerBadgeNum,
                    PersonnelAgencyFlag = s.PersonnelAgencyGroupFlag
                }).ToList();

        public PersonnelVm GetPersonnelByUsername(string username) =>
            (from u in _context.UserAccess
             where !u.Personnel.PersonnelTerminationFlag && u.UserName == username
             select new PersonnelVm
             {
                 ArrestTransportOfficerFlag = u.Personnel.ArrestTransportOfficerFlag,
                 PersonFirstName = u.Personnel.PersonNavigation.PersonFirstName,
                 PersonLastName = u.Personnel.PersonNavigation.PersonLastName,
                 PersonMiddleName = u.Personnel.PersonNavigation.PersonMiddleName,
                 PersonId = u.Personnel.PersonId,
                 PersonnelId = u.PersonnelId,
                 OfficerBadgeNumber = u.Personnel.OfficerBadgeNum,
                 PersonnelAgencyFlag = u.Personnel.PersonnelAgencyGroupFlag
             }).Single();

        // In Admin(Global->Field) user can customize FieldLabel. And in Jms we displayed the  Field Name based on that.
        public List<UserControlFieldTags> GetFieldNames(List<UserControlFieldTags> userField)
        {
            List<UserControlFieldTags> customizedFieldNames = new List<UserControlFieldTags>();
            if (!userField.Any()) return customizedFieldNames;
            customizedFieldNames = _context.AppAoUserControlFields.SelectMany(usc => userField.Where(ctr =>
                    ctr.ControlId == usc.AppAoUserControlId && ctr.FieldTag == usc.FieldTagId),
                (usc, f) => new UserControlFieldTags
                {
                    FieldTag = f.FieldTag,
                    FieldLabel = usc.FieldLabel
                }).ToList();

            return customizedFieldNames;
        }

        // Personnel search details
        //TODO Requires deep code review!
        public List<PersonnelSearchVm> GetPersonnelSearchDetails(PersonnelSearchVm personnelSearchVm)
        {
            personnelSearchVm.PersonnelSearchText = personnelSearchVm
            .PersonnelSearchText.Where(w => !string.IsNullOrEmpty(w)).ToArray();

            IQueryable<Personnel> personnels = _context.Personnel.Where(w => (!personnelSearchVm.PersonActive
              || !w.PersonnelTerminationFlag) && (personnelSearchVm.AgencyId <= 0 ||
              w.AgencyId == personnelSearchVm.AgencyId) && (personnelSearchVm.OfficerFlag != OfficerFlag.ARRESTTRANSPORT
              || w.ArrestTransportOfficerFlag) && (personnelSearchVm.OfficerFlag != OfficerFlag.RECEIVESEARCH
              || w.ReceiveSearchOfficerFlag) && (personnelSearchVm.OfficerFlag != OfficerFlag.PROGRAMINSTRUCTOR
              || w.ProgramInstructorFlag) && (personnelSearchVm.OfficerFlag != OfficerFlag.PROGRAMCASE
              || w.PersonnelProgramCaseFlag) && (personnelSearchVm.PersonnelId <= 0
              || w.PersonnelId == personnelSearchVm.PersonnelId));

            personnels = personnelSearchVm.PersonnelSearchText
            .Aggregate(personnels, (current, search) => current
            .Where(w => w.PersonNavigation.PersonLastName.Contains(search) ||
            w.PersonNavigation.Aka.Any(a => a.AkaLastName.Contains(search)) ||
            w.PersonNavigation.PersonFirstName.Contains(search) ||
            w.PersonNavigation.Aka.Any(a => a.AkaFirstName.Contains(search)) ||
            w.PersonNavigation.PersonMiddleName.Contains(search) ||
            w.PersonNavigation.Aka.Any(a => a.AkaMiddleName.Contains(search)) ||
            w.OfficerBadgeNum.Replace("-", string.Empty).Contains(search) ||
            w.OfficerBadgeNumber.Replace("-", string.Empty).Contains(search)));

            List<PersonnelSearchVm> personnelSearches = personnels
                .Select(s => new PersonnelSearchVm
                {
                    PersonnelId = s.PersonnelId,
                    PersonDetail = new PersonVm
                    {
                        PersonnelId = s.PersonnelId,
                        PersonId = s.PersonNavigation.PersonId,
                        PersonLastName = s.PersonNavigation.PersonLastName,
                        PersonFirstName = s.PersonNavigation.PersonFirstName,
                        PersonMiddleName = s.PersonNavigation.PersonMiddleName,
                        PersonSuffix = s.PersonNavigation.PersonSuffix,
                        OfficerBadgeNumber = s.OfficerBadgeNum, // officerbadge number
                        PersonnelBadgeNumber = s.OfficerBadgeNumber, // personnelbadge number
                        PersonDob = s.PersonNavigation.PersonDob
                    },
                    PersonActive = !s.PersonnelTerminationFlag,
                    AgencyGroupFlag = s.PersonnelAgencyGroupFlag
                })
                .OrderByDescending(x => x.PersonDetail.OfficerBadgeNumber == "1" ? 1 : 0)
                .OrderBy(o => o.PersonDetail.PersonLastName)
                .ThenBy(o => o.PersonDetail.PersonFirstName)
                .ThenBy(o => o.PersonDetail.PersonMiddleName)
                .ThenBy(o => o.PersonDetail.PersonSuffix)
                .ToList();
            return personnelSearches;
        }

        public string GetDbDetails() =>
            Configuration.GetValue<string>("ConnectionStrings:DefaultConnection").Split(';')[1]
                .Split('=')[1];

        public int InsertInmateTracking(InmateTrackingVm obInsertTrackingVm)
        {
            if (obInsertTrackingVm == null || !obInsertTrackingVm.TrackingHistory.Any()) return 0;
            foreach (InmateTrackingHistroyVm item in obInsertTrackingVm.TrackingHistory)
            {
                int inmateTrackId = _context.InmateTrak.LastOrDefault(i =>
                                        i.InmateId == item.InmateId && !i.InmateTrakDateIn.HasValue)?.InmateTrakId ?? 0;
                int oldInmateTrackId = inmateTrackId;
                int inmateDestinationId = 0;
                DateTime EnrouteStartOut = DateTime.Now;
                int eventInmateTrackId = 0;
                InmateTrak oldInmateTrack = new InmateTrak();
                if (inmateTrackId > 0)
                {
                    int id = inmateTrackId;
                    oldInmateTrack = _context.InmateTrak.Single(i => i.InmateTrakId == id);

                    if(TrackingFlag.Checkin == obInsertTrackingVm.TrackingStatus)
                    {
                        oldInmateTrack.InmateTrakNote = item.InmateTrackNote;
                    }

                    inmateDestinationId = oldInmateTrack.EnrouteFinalLocationId ?? 0;

                    EnrouteStartOut = inmateDestinationId > 0 && obInsertTrackingVm.TrackingStatus == TrackingFlag.Move
                            ? oldInmateTrack.EnrouteStartOut ?? DateTime.Now
                            : DateTime.Now;
                }

                //skipped same location moving
                if(obInsertTrackingVm.TrackingStatus == TrackingFlag.Move)
                {
                    if(oldInmateTrack.InmateTrakLocationId == obInsertTrackingVm.SelectedLocationId)
                    {
                        if(obInsertTrackingVm.EnrouteFinalLocationId == 0)
                        {
                            continue;
                        }
                        else if(oldInmateTrack.EnrouteFinalLocationId > 0)
                        {
                            continue;
                        }
                    }
                }

                if(inmateTrackId > 0)
                {
                    if (!item.InmateRefused)
                    {
                        oldInmateTrack.InmateTrakDateIn = DateTime.Now;
                        oldInmateTrack.InPersonnelId = _personnelId;
                    }
                }

                Inmate obInmate = _context.Inmate.Single(it => it.InmateId == item.InmateId);

                if (TrackingFlag.Checkin == obInsertTrackingVm.TrackingStatus)
                {
                    obInmate.InmateCurrentTrack = null;
                    obInmate.InmateCurrentTrackId = null;
                }
                else
                {
                    int reqEnrouteFinalLocId = obInsertTrackingVm.EnrouteFinalLocationId ?? 0;
                    int finalLocId = reqEnrouteFinalLocId > 0
                        ? reqEnrouteFinalLocId
                        : obInsertTrackingVm.SelectedLocationId ?? 0;
                    if (TrackingFlag.Checkout != obInsertTrackingVm.TrackingStatus)
                    {
                        finalLocId = inmateDestinationId > 0 ? inmateDestinationId
                            : reqEnrouteFinalLocId > 0 ? reqEnrouteFinalLocId
                            : obInsertTrackingVm.SelectedLocationId ?? 0;
                    }
                    else
                    {
                        EnrouteStartOut = DateTime.Now;
                    }

                    Privileges privileges =
                        _context.Privileges.Single(w => w.PrivilegeId == finalLocId);

                    if (obInsertTrackingVm.ConflictDetails != null &&
                        obInsertTrackingVm.ConflictDetails.Count > 0 && !item.InmateRefused)
                    {
                        InsertConflict(item.ConflictDetails, item.InmateId,item.InmateTrakConflictNote);
                    }

                    InmateTrak objTrack = new InmateTrak();
                    if (item.HistoryScheduleId > 0 && !item.InmateRefused)
                    {
                        objTrack = new InmateAppointmentTrack();
                    }

                    objTrack.InmateId = item.InmateId;
                    objTrack.InmateTrakLocation = obInsertTrackingVm.SelectedLocation;
                    objTrack.InmateTrakLocationId = obInsertTrackingVm.SelectedLocationId;
                    objTrack.OutPersonnelId = _personnelId;
                    objTrack.InmateTrakDateOut = DateTime.Now;
                    objTrack.FacilityId = obInsertTrackingVm.FacilityId;
                    objTrack.InmateTrakNote = item.InmateTrackNote;
                    objTrack.InmateTrakConflictNote = item.InmateTrakConflictNote;
                    objTrack.InmateTrakFromHousingUnitId = obInmate.HousingUnitId;
                    objTrack.InmateTrakFromLocationId = obInmate.InmateCurrentTrackId;
                    objTrack.EnrouteStartOut = EnrouteStartOut;
                    //Enroute checkout
                    if (privileges.TrackEnrouteFlag == 1 || inmateDestinationId > 0)
                    {
                        bool enrouteInFlag = false, enrouteFinalFlag = false;

                        if (inmateDestinationId > 0 || obInsertTrackingVm.EnrouteFinalLocationId > 0)
                        {
                            objTrack.EnrouteFinalLocationId = inmateDestinationId > 0
                                ? inmateDestinationId : obInsertTrackingVm.EnrouteFinalLocationId;
                        }

                        if (inmateTrackId > 0 && objTrack.EnrouteFinalLocationId != objTrack.InmateTrakLocationId)
                        {
                            int inmTrackId = inmateTrackId;
                            InmateTrak inmateTrack = _context.InmateTrak.Single(i => i.InmateTrakId == inmTrackId);
                            enrouteInFlag = inmateTrack.EnrouteInFlag;
                            enrouteFinalFlag = inmateTrack.EnrouteFinalFlag;
                        }

                        if (objTrack.EnrouteFinalLocationId == objTrack.InmateTrakLocationId)
                        {
                            objTrack.EnrouteFinalFlag = true;
                        }
                        else if (enrouteInFlag || enrouteFinalFlag)
                        {
                            objTrack.EnrouteInFlag = true;
                            objTrack.EnrouteOutFlag = false;
                        }
                        else
                        {
                            objTrack.EnrouteOutFlag = true;
                            objTrack.EnrouteInFlag = false;
                        }
                    }

                    if (!item.InmateRefused)
                    {
                        obInmate.InmateCurrentTrack = obInsertTrackingVm.SelectedLocation;
                        obInmate.InmateCurrentTrackId = obInsertTrackingVm.SelectedLocationId;
                        objTrack.InmateTrakDateOut = DateTime.Now;
                        objTrack.InmateRefused = item.InmateRefused;
                    }
                    else
                    {
                        objTrack.InmateRefusedReason = item.InmateRefusedReason;
                        objTrack.InmateRefusedNote = item.InmateRefusedNote;
                        objTrack.InmateRefused = item.InmateRefused;
                        objTrack.InmateTrakDateIn = DateTime.Now;
                        objTrack.InPersonnelId = _personnelId;
                    }

                    if (item.HistoryScheduleId > 0 && !item.InmateRefused)
                    {
                        InmateAppointmentTrack obInmateAppointmentTrack = (InmateAppointmentTrack)objTrack;
                        obInmateAppointmentTrack.ScheduleId = item.HistoryScheduleId;
                        obInmateAppointmentTrack.OccurenceDate = DateTime.Now;
                        _context.InmateAppointmentTrack.Add(obInmateAppointmentTrack);
                        _context.SaveChanges();
                        eventInmateTrackId = obInmateAppointmentTrack.InmateTrakId;
                    }
                    else
                    {
                        _context.InmateTrak.Add(objTrack);
                        _context.SaveChanges();
                        eventInmateTrackId = objTrack.InmateTrakId;
                    }

                    _context.SaveChanges();
                    inmateTrackId = objTrack.InmateTrakId;

                    int count = _context.InmateTrak.Count(i =>
                        i.InmateTrakLocationId == obInsertTrackingVm.SelectedLocationId &&
                        !i.InmateTrakDateIn.HasValue);
                    bool externalFacility = _context.Privileges.Any(x =>
                        x.PrivilegeDescription == obInsertTrackingVm.SelectedLocation);
                    int inmateCount = _context.Inmate.Count(it =>
                        it.InmateId == item.InmateId &&
                        it.InmateCurrentTrack ==
                        obInsertTrackingVm.InmateTrakLocation &&
                        externalFacility &&
                        it.FacilityId == obInsertTrackingVm.FacilityId.Value);

                    if (count == 1 && inmateCount == 1)
                    {
                        privileges.SafetyCheckLastEntry = DateTime.Now;
                        privileges.SafetyCheckLastEntryBy = _personnelId;
                    }

                    if (ScheduleEventFlag.WorkFurlough == obInsertTrackingVm.ScheduleFlag
                        || ScheduleEventFlag.WorkCrew == obInsertTrackingVm.ScheduleFlag)
                    {
                        WorkCrewTrackXref dbWorkCrewTrackXref = new WorkCrewTrackXref
                        {
                            WorkCrewId = item.WorkCrewId,
                            InmateTrakId = inmateTrackId
                        };
                        _context.WorkCrewTrackXref.Add(dbWorkCrewTrackXref);
                        _context.SaveChanges();
                    }
                }

                _interfaceEngineService.Export(new ExportRequestVm
                {
                    EventName = EventNameConstants.LOCATIONTRACKING,
                    PersonnelId = _personnelId,
                    Param1 = _context.Inmate.Single(i => i.InmateId == item.InmateId).PersonId.ToString(),
                    Param2 = eventInmateTrackId.ToString()
                });

                _context.SaveChanges();

                if (ScheduleEventFlag.WorkFurlough == obInsertTrackingVm.ScheduleFlag
                    || ScheduleEventFlag.WorkCrew == obInsertTrackingVm.ScheduleFlag)
                {
                    GetWorkCrewHistoryValueDetails(item.WorkCrewId, item.InmateId);
                }

                _context.SaveChanges();
            }

            return _context.SaveChanges();
        }

        private void InsertWorkCrewHistory(int workCrewId, string workCrewHistoryList)
        {
            WorkCrewHistory dbCrewHistory = new WorkCrewHistory
            {
                WorkCrewId = workCrewId,
                PersonnelId = _personnelId,
                CreateDate = DateTime.Now,
                WorkCrewHistoryList = workCrewHistoryList
            };
            _context.WorkCrewHistory.Add(dbCrewHistory);
        }

        public void InsertConflict(List<TrackingConflictVm> obTrackingConflictDetails, int inmateId, string note)
        {
            FloorNotes floorNoteIns1 = new FloorNotes();
            foreach (TrackingConflictVm ob in obTrackingConflictDetails)
            {
                floorNoteIns1.FloorNoteNarrative +=FloorNotesConflictConstants.NOTE + note + " "+
                 FloorNotesConflictConstants.TYPE + ob.ConflictType +" "+ FloorNotesConflictConstants.DESCRIPTION +
                                                    ob.ConflictDescription;
            }

            floorNoteIns1.FloorNoteOfficerId = _personnelId;

            floorNoteIns1.FloorNoteDate = DateTime.Now;
            floorNoteIns1.FloorNoteType = FloorNotesConflictConstants.CONFLICTCHECK;
            floorNoteIns1.FloorNoteTime = DateTime.Now.ToString("HH:mm:ss");
            _context.FloorNotes.Add(floorNoteIns1);

            FloorNoteXref floorNoteXref = new FloorNoteXref
            {
                FloorNoteId = floorNoteIns1.FloorNoteId,
                InmateId = inmateId,
                CreateDate = DateTime.Now
            };

            _context.FloorNoteXref.Add(floorNoteXref);

            _context.SaveChanges();
        }

        //TODO Total rewrite using Schedule infrastructure
        private WorkCrewFurlVm GetWorkCrewHistoryValue(int workCrewId, int inmateId)
        {
            //Get WorkCrew details
            WorkCrewFurlVm lstHistoryVal = _context.WorkCrew.Where(wc =>
                    wc.WorkCrewId == workCrewId && wc.InmateNavigation.InmateActive == 1 && wc.InmateId == inmateId)
                .Select(wc => new WorkCrewFurlVm
                {
                    WorkCrewId = wc.WorkCrewId,
                    WorkCrewName = wc.WorkCrewLookup.CrewName,
                    StartDate = wc.StartDate,
                    EndDate = wc.EndDate,
                    WorkCrewLockerId = wc.WorkCrewLockerId,
                    Comment = wc.Comment,
                    ContactName = wc.FurloughContactName,
                    ContactNumber = wc.FurloughContactNumber,
                    ContactInfo = wc.FurloughContactInfo,
                    TodayStart = wc.FurloughSchdTodayStart,
                    TodayEnd = wc.FurloughSchdTodayEnd,
                    SunStart = wc.FurloughSchdSunStart,
                    SunEnd = wc.FurloughSchdSunEnd,
                    MonStart = wc.FurloughSchdMonStart,
                    MonEnd = wc.FurloughSchdMonEnd,
                    TueStart = wc.FurloughSchdTueStart,
                    TueEnd = wc.FurloughSchdTueEnd,
                    WedStart = wc.FurloughSchdWedStart,
                    WedEnd = wc.FurloughSchdWedEnd,
                    ThuStart = wc.FurloughSchdThuStart,
                    ThuEnd = wc.FurloughSchdThuEnd,
                    FriStart = wc.FurloughSchdFriStart,
                    FriEnd = wc.FurloughSchdFriEnd,
                    SatStart = wc.FurloughSchdSatStart,
                    SatEnd = wc.FurloughSchdSatEnd,
                    InmateId = wc.InmateId,
                    InmateTrak = new InmateTrakVm
                    {
                        InmateCurrentTrack = wc.InmateNavigation.InmateCurrentTrack
                    },
                    Person = new PersonVm
                    {
                        PersonId = wc.InmateNavigation.PersonId,
                        CreatedDate = wc.CreateDate,
                        UpdatedDate = wc.UpdateDate,
                        CreateBy = wc.CreatedBy,
                        UpdateBy = wc.UpdatedBy,
                        InmateNumber = wc.InmateNavigation.InmateNumber
                    },
                    WorkCrewLookupId = wc.WorkCrewLookupId
                }).SingleOrDefault();

            if (lstHistoryVal == null) return null;
            int lstPersonId = lstHistoryVal.Person.PersonId;
            int personnelId = lstHistoryVal.Person.CreateBy ?? (lstHistoryVal.Person.UpdateBy ?? 0);
            List<int> lstPersonnelId = new List<int> { personnelId };

            List<PersonnelVm> lstPersonDet = _personService.GetPersonNameList(lstPersonnelId);

            PersonVm lstPersonDetails = _context.Person.Where(p => p.PersonId == lstPersonId)
                .Select(p => new PersonVm
                {
                    PersonId = p.PersonId,
                    PersonLastName = p.PersonLastName,
                    PersonFirstName = p.PersonFirstName,
                    PersonMiddleName = p.PersonMiddleName,
                    PersonSuffix = p.PersonSuffix,
                    PersonBusinessPhone = p.PersonBusinessPhone,
                    PersonCellPhone = p.PersonCellPhone,
                    PersonPhone = p.PersonPhone
                }).SingleOrDefault();

            lstHistoryVal.LockerName = _context.WorkCrewLocker
                .SingleOrDefault(w => w.WorkCrewLockerId == lstHistoryVal.WorkCrewLockerId)?.LockerName;

            if (lstPersonDetails != null)
            {
                lstHistoryVal.Person.PersonLastName = lstPersonDetails.PersonLastName;
                lstHistoryVal.Person.PersonFirstName = lstPersonDetails.PersonFirstName;
                lstHistoryVal.Person.PersonMiddleName = lstPersonDetails.PersonMiddleName;
                lstHistoryVal.Person.PersonSuffix = lstPersonDetails.PersonSuffix;
                lstHistoryVal.Person.PersonBusinessPhone = lstPersonDetails.PersonBusinessPhone;
                lstHistoryVal.Person.PersonCellPhone = lstPersonDetails.PersonCellPhone;
                lstHistoryVal.Person.PersonPhone = lstPersonDetails.PersonPhone;
            }

            PersonnelVm personInfo;
            lstHistoryVal.TrackingHistory = new TrackingHistory();
            if (lstHistoryVal.Person.CreateBy.HasValue)
            {
                personInfo = lstPersonDet.Single(p => p.PersonnelId == lstHistoryVal.Person.CreateBy);
                lstHistoryVal.TrackingHistory.CreateByLastName = personInfo.PersonLastName;
                lstHistoryVal.TrackingHistory.CreateByFirstName = personInfo.PersonFirstName;
                lstHistoryVal.TrackingHistory.CreateByOfficerBadgeNumber = personInfo.OfficerBadgeNumber;
            }

            if (lstHistoryVal.Person.UpdateBy.HasValue)
            {
                personInfo = lstPersonDet.Single(p => p.PersonnelId == lstHistoryVal.Person.UpdateBy);
                lstHistoryVal.TrackingHistory.UpdateByLastName = personInfo.PersonLastName;
                lstHistoryVal.TrackingHistory.UpdateByFirstName = personInfo.PersonFirstName;
                lstHistoryVal.TrackingHistory.UpdateByOfficerBadgeNumber = personInfo.OfficerBadgeNumber;
            }

            //Get person Address
            lstHistoryVal.PersonAddress = GetAddressDetails(lstPersonId);

            PersonAddressVm lstPersonDescriptionDetails = _context.PersonDescription
                .Where(pd => pd.PersonId == lstHistoryVal.Person.PersonId)
                .Select(pd => new PersonAddressVm
                {
                    PersonEmployer = pd.PersonEmployer,
                    PersonDescriptionId = pd.PersonDescriptionId,
                    PersonOccupation = pd.PersonOccupation,
                    PersonId = pd.PersonId ?? 0
                }).OrderByDescending(pd => pd.PersonDescriptionId)
                .FirstOrDefault();

            if (lstPersonDescriptionDetails == null) return lstHistoryVal;
            lstHistoryVal.PersonEmployer = lstPersonDescriptionDetails.PersonEmployer;
            lstHistoryVal.PersonOccupation = lstPersonDescriptionDetails.PersonOccupation;

            return lstHistoryVal;
        }

        private List<PersonAddressVm> GetAddressDetails(int personId)
        {
            //referred in v1 -for track furlough and work crew tracking, only residential and business address needed for workcrewhistorylist column.
            string[] addressType = { AddressTypeConstants.RES, AddressTypeConstants.BUS };

            //Getting the address for person in Address table
            return addressType.Select(addType => _context.Address
                    .Where(a => a.PersonId == personId && a.AddressType == addType)
                    .Select(a => new PersonAddressVm
                    {
                        AddressId = a.AddressId,
                        PersonId = a.PersonId ?? 0,
                        Number = a.AddressNumber,
                        Street = a.AddressStreet,
                        Suffix = a.AddressSuffix,
                        UnitType = a.AddressUnitType,
                        UnitNo = a.AddressUnitNumber,
                        AddressType = a.AddressType,
                        AddressZone = a.AddressZone,
                        AddressGridLocation = a.AddressGridLocation,
                        AddressBeat = a.AddressBeat,
                        City = a.AddressCity,
                        State = a.AddressState,
                        Zip = a.AddressZip,
                        Direction = a.AddressDirection,
                        Line2 = a.AddressLine2,
                        DirectionSuffix = a.AddressDirectionSuffix,
                        AddressLookupId = a.AddressLookupId,
                        CreateDate = a.CreateDate,
                        UpdateDate = a.UpdateDate,
                        BusinessFax = a.PersonBusinessFax,
                        AddressOtherNote = a.AddressOtherNote,
                        IsHomeless = a.AddressHomeless,
                        IsTransient = a.AddressTransient,
                        IsRefused = a.AddressRefused,
                        CreateBy = a.CreatedBy,
                        UpdateBy = a.UpdateBy
                    })
                    .OrderByDescending(a => a.AddressId)
                    .FirstOrDefault())
                .Where(lstAddressDet => lstAddressDet != null)
                .ToList();
        }

        //TODO: Should be totally rewritten
        private void GetWorkCrewHistoryValueDetails(int workCrewId, int inmateId)
        {
            WorkCrewFurlHistoryVm hstValue = new WorkCrewFurlHistoryVm();
            StringBuilder address = new StringBuilder();
            WorkCrewFurlVm lstHistoryVal = GetWorkCrewHistoryValue(workCrewId, inmateId);

            if (lstHistoryVal != null)
            {
                hstValue.WorkCrew = lstHistoryVal.WorkCrewName;
                hstValue.StartDate = lstHistoryVal.StartDate.HasValue
                    ? lstHistoryVal.StartDate.ToString()
                    : "";
                hstValue.StartTime = lstHistoryVal.StartDate.HasValue
                    ? lstHistoryVal.StartDate.Value.TimeOfDay.ToString()
                    : ""; //Get time from StartDate
                hstValue.EndDate = lstHistoryVal.EndDate.HasValue ? lstHistoryVal.EndDate.ToString() : "";
                hstValue.EndTime = lstHistoryVal.EndDate.HasValue
                    ? lstHistoryVal.EndDate.Value.TimeOfDay.ToString()
                    : ""; //Get time from EndDate
                hstValue.LockerName = lstHistoryVal.LockerName ?? "";
                hstValue.Comment = lstHistoryVal.Comment ?? "";
                hstValue.Employer = lstHistoryVal.PersonEmployer ?? "";
                hstValue.Occupation = lstHistoryVal.PersonOccupation ?? "";
                hstValue.BusPhone = lstHistoryVal.Person.PersonBusinessPhone ?? "";
                hstValue.CellPhone = lstHistoryVal.Person.PersonCellPhone ?? "";
                hstValue.HomePhone = lstHistoryVal.Person.PersonPhone ?? "";
                lstHistoryVal.PersonAddress.ForEach(a =>
                {
                    //Concatenate the address details for Insert
                    address.Append(a.AddressType == AddressTypeConstants.RES
                        ? AddressTypeConstants.RES
                        : AddressTypeConstants.BUS).Append(": ");
                    if (!string.IsNullOrEmpty(a.Number))
                        address.Append(a.Number).Append(" ");
                    if (!string.IsNullOrEmpty(a.Direction))
                        address.Append(a.Direction).Append(" ");
                    if (!string.IsNullOrEmpty(a.Street))
                        address.Append(a.Street).Append(" ");
                    if (!string.IsNullOrEmpty(a.Suffix))
                        address.Append(a.Suffix).Append(" ");
                    if (!string.IsNullOrEmpty(a.DirectionSuffix))
                        address.Append(a.DirectionSuffix).Append(" ");
                    if (!string.IsNullOrEmpty(a.UnitType))
                        address.Append(a.UnitType).Append(" ");
                    if (!string.IsNullOrEmpty(a.UnitNo))
                        address.Append(a.UnitNo).Append(" ");
                    if (!string.IsNullOrEmpty(a.Line2))
                        address.Append(a.Line2).Append(" ");
                    if (!string.IsNullOrEmpty(a.City))
                        address.Append(a.City).Append(",");
                    if (!string.IsNullOrEmpty(a.State))
                        address.Append(a.State).Append(" ");
                    if (!string.IsNullOrEmpty(a.Zip))
                        address.Append(a.Zip).Append(" ");
                    if (!string.IsNullOrEmpty(a.AddressOtherNote))
                        address.Append(a.AddressOtherNote);
                });
                hstValue.Address = address.ToString();
                hstValue.ContactName = lstHistoryVal.ContactName ?? "";
                hstValue.ContactNumber = lstHistoryVal.ContactNumber ?? "";
                hstValue.ContactInfo = lstHistoryVal.ContactInfo ?? "";

                //Concatenate the date details for Insert
                hstValue.Sun = lstHistoryVal.SunStart.HasValue
                    ? new StringBuilder().Append(lstHistoryVal.SunStart).Append("-")
                        .Append(lstHistoryVal.SunEnd).ToString()
                    : "";

                hstValue.Mon = lstHistoryVal.MonStart.HasValue
                    ? new StringBuilder().Append(lstHistoryVal.MonStart).Append("-")
                        .Append(lstHistoryVal.MonEnd).ToString()
                    : "";

                hstValue.Tue = lstHistoryVal.TueStart.HasValue
                    ? new StringBuilder().Append(lstHistoryVal.TueStart).Append("-")
                        .Append(lstHistoryVal.TueEnd).ToString()
                    : "";

                hstValue.Wed = lstHistoryVal.WedStart.HasValue
                    ? new StringBuilder().Append(lstHistoryVal.WedStart).Append("-")
                        .Append(lstHistoryVal.WedEnd).ToString()
                    : "";

                hstValue.Thu = lstHistoryVal.ThuStart.HasValue
                    ? new StringBuilder().Append(lstHistoryVal.ThuStart).Append("-")
                        .Append(lstHistoryVal.ThuEnd).ToString()
                    : "";

                hstValue.Fri = lstHistoryVal.FriStart.HasValue
                    ? new StringBuilder().Append(lstHistoryVal.FriStart).Append("-")
                        .Append(lstHistoryVal.FriEnd).ToString()
                    : "";

                hstValue.Sat = lstHistoryVal.SatStart.HasValue
                    ? new StringBuilder().Append(lstHistoryVal.SatStart).Append("-")
                        .Append(lstHistoryVal.SatEnd).ToString()
                    : "";
                hstValue.TrackLocation = lstHistoryVal.InmateTrak.InmateCurrentTrack ?? "";
            }

            InsertWorkCrewHistory(workCrewId, JsonConvert.SerializeObject(hstValue));
        }

        //Convert File Stream int on Byte[](temporarily done in here)
        public byte[] ConvertStreamToByte(Stream input)
        {
            byte[] buffer = new byte[16 * 1024];
            MemoryStream ms = new MemoryStream();
            int read;
            while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
            {
                ms.Write(buffer, 0, read);
            }

            return ms.ToArray();
        }

        public string GetExecSp(string spName, KeyValuePair<string, string>[] parameters)
        {
            string result = "";
            try
            {
                using (DbCommand command = _context.Database.GetDbConnection().CreateCommand())
                {
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandText = spName;

                    foreach (KeyValuePair<string, string> param in parameters)
                    {
                        DbParameter parameter = command.CreateParameter();
                        parameter.ParameterName = param.Key;
                        parameter.DbType = DbType.String;
                        parameter.Direction = ParameterDirection.Input;
                        parameter.Value = param.Value;
                        command.Parameters.Add(parameter);
                    }

                    _context.Database.OpenConnection();
                    DbDataReader reader = command.ExecuteReader();
                    if (!reader.HasRows || !reader.Read()) return result;
                    result = reader.IsDBNull(0) ? null : reader.GetValue(0).ToString();
                }

                return result;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        public string GetValidationType(TaskValidateType validationNumber)
        {
            switch (validationNumber)
            {
                case TaskValidateType.IntakeCreateEvent:
                    return "INTAKE CREATE EVENT";
                case TaskValidateType.IntakeCompleteEvent:
                    return "INTAKE COMPLETE EVENT";
                case TaskValidateType.BookingCompleteKeeperEvent:
                    return "BOOKING COMPLETE KEEPER EVENT";
                case TaskValidateType.AssessmentCompleteKeeperEvent:
                    return "ASSESSMENT COMPLETE KEEPER EVENT";
                case TaskValidateType.FacilityTransferEvent:
                    return "FACILITY TRANSFER EVENT";
                case TaskValidateType.ClearedForRelease:
                    return "CLEARED FOR RELEASE";
                case TaskValidateType.IntakeComplete:
                    return "INTAKE COMPLETE";
                case TaskValidateType.BookingComplete:
                    return "BOOKING COMPLETE";
                case TaskValidateType.AssessmentComplete:
                    return "ASSESSMENT COMPLETE";
                case TaskValidateType.BookingCompleteNonKeeperEvent:
                    return "BOOKING COMPLETE NON KEEPER EVENT";
                case TaskValidateType.AssessmentCompleteNonKeeperEvent:
                    return "ASSESSMENT COMPLETE NON KEEPER EVENT";
                case TaskValidateType.DoRelease:
                    return "DO RELEASE";
                case TaskValidateType.HousingAssignFromTransfer:
                    return "HOUSING ASSIGN FROM TRANSFER";
                default: return null;
            }
        }

        public bool GetCourtCommitFlag(int arrestId)
        {
            List<InmatePrebook> inmatePreBooks = _context.InmatePrebook.Where(ip => ip.ArrestId == arrestId).ToList();
            if (inmatePreBooks.Count > 0)
            {
                return inmatePreBooks[0].CourtCommitFlag == 1;
            }

            int? incarcerationId = _context.IncarcerationArrestXref.SingleOrDefault(ia => ia.ArrestId == arrestId)
                ?.IncarcerationId;
            inmatePreBooks = _context.InmatePrebook.Where(ip => ip.IncarcerationId == incarcerationId).ToList();
            return inmatePreBooks.Count > 0 && inmatePreBooks[0].CourtCommitFlag == 1;
        }

        public List<string> GetLookupGangSubSet(string gangName)
        {
            List<string> subsets = _context.Lookup
                .Where(w => w.LookupCategory == gangName && w.LookupType == "CLASSGROUPSUB" && w.LookupFlag9 == 1)
                .Select(s => s.LookupDescription).ToList();
            return subsets;
        }

        public int GetIdleTimeOut()
        {
            return Configuration.GetValue<int?>("SiteVariables:IdleTimeOut") != null
                ? Configuration.GetValue<int>("SiteVariables:IdleTimeOut") : 0;
        }

        public void atimsReportsContentLog(string reportName, string jsonString)
        {
            if (Configuration.GetValue<bool?>("SiteVariables:JSReportLog") == null ||
                !Configuration.GetValue<bool>("SiteVariables:JSReportLog")) return;
            string path = @".\logs\JsReport" + DateTime.Now.ToString("yyyy/MM/dd").Replace('/', '_') + ".txt";

            if (!File.Exists(path))
            {
                File.WriteAllText(path, reportName + " " + DateTime.Now + ": " + jsonString + Environment.NewLine);
            }
            else
            {
                File.AppendAllText(path, reportName + " " + DateTime.Now + ": " + jsonString + Environment.NewLine);
            }
        }

        public List<int> GetHousingUnitListIds(int housingGroupId) =>
            _context.HousingGroupAssign.Where(w => w.HousingGroupId == housingGroupId
                && w.HousingUnitListId.HasValue).Select(s => s.HousingUnitListId ?? 0).ToList();

        public void WriteToFile(string fileName, string content)
        {
            string path = @".\logs\" + fileName + DateTime.Now.ToString("yyyy/MM/dd").Replace('/', '_') + ".txt";
            if (!File.Exists(path))
            {
                File.WriteAllText(path, DateTime.Now + ": " + content + Environment.NewLine);
            }
            else
            {
                File.AppendAllText(path, DateTime.Now + ": " + content + Environment.NewLine);
            }
        }

        public DataTable RunStoredProcedure(string storeProcedureName, List<SqlParameter> parameters)
        {
            SqlCommand command =
                new SqlCommand(storeProcedureName, (SqlConnection)_context.Database.GetDbConnection())
                {
                    CommandType = CommandType.StoredProcedure
                };
            parameters.ForEach(p => { command.Parameters.Add(p); });
            DataTable resultTable = new DataTable();
            SqlDataAdapter adapter = new SqlDataAdapter(command);
            adapter.Fill(resultTable);
            return resultTable;
        }

        //Report
        public JObject GetCustomMapping()
        {
            List<CustomMappingVm> lstCustomMappings = _context.CustomMapping.Select(s => new CustomMappingVm
            {
                CustomMappingId = s.CustomMappingId,
                DisplayValue = string.IsNullOrEmpty(s.CustomValue) ? s.DefaultValue : s.CustomValue,
                FieldMapping = s.FieldMapping
            }).ToList();

            // below var type used to add root node of an custom mapping table element
            var customMapping = new
            {
                CustomMapping = lstCustomMappings
            };
            string customData = JsonConvert.SerializeObject(customMapping);
            JObject data3 = JObject.Parse(customData);
            return data3;
        }

        public List<LookupVm> GetIncidentLookups(string[] types)
        {
            List<LookupVm> lookUp =
                (from lu in _context.Lookup
                 where types.Contains(lu.LookupType)
                 select new LookupVm
                 {
                     LookupIdentity = lu.LookupIdentity,
                     LookupIndex = lu.LookupIndex,
                     LookupType = lu.LookupType,
                     LookupCategory = lu.LookupCategory,
                     LookupDescription = lu.LookupDescription,
                     LookupInactive = lu.LookupInactive == 1,
                     LookupName = lu.LookupName,
                     LookupColor = lu.LookupColor,
                     AlertAllowExpire = lu.LookupAlertAllowExpire == 1,
                     LookupNoAlert = lu.LookupNoAlert ?? 0,
                     AlertOrder = lu.LookupAlertOrder ?? 0,
                     LookupFlag6 = lu.LookupFlag6 ?? 0,
                     LookupFlag7 = lu.LookupFlag7 ?? 0,
                     LookupOrder = lu.LookupOrder,
                     LookupFlag8 = lu.LookupFlag8 ?? 0,
                     LookupFlag9 = lu.LookupFlag9 ?? 0,
                     LookupFlag10 = lu.LookupFlag10 ?? 0
                 }).OrderByDescending(lu => lu.LookupOrder).ThenBy(luk => luk.LookupDescription).ToList();
            return lookUp;
        }
    }
}