using GenerateTables.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using ServerAPI.Utilities;
using ServerAPI.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Web;

// ReSharper disable once CheckNamespace
namespace ServerAPI.Services
{
    public class PrebookWizardService : IPrebookWizardService
    {
        private readonly AAtims _context;
        private readonly IConfiguration _configuration;
        private readonly ICommonService _commonService;
        private readonly IPersonService _personService;
        private readonly IAltSentService _altSentService;
        private readonly ICellService _cellService;
        private readonly IInmateService _inmateService;
        private readonly IBookingReleaseService _bookingReleaseService;
        private readonly int _personnelId;
        private readonly IInterfaceEngineService _interfaceEngineService;

        private readonly IAtimsHubService _atimsHubService;
        private readonly IPhotosService _photoService;
        private readonly IAppletsSavedService _appletsSavedService;


        public PrebookWizardService(AAtims context, IConfiguration configuration, ICommonService commonService,
            ICellService cellService, IHttpContextAccessor httpContextAccessor, IAtimsHubService atimsHubService,
            IPersonService personService, IInmateService inmateService, IBookingReleaseService bookingReleaseService,
            IAltSentService altSentService, IInterfaceEngineService interfaceEngineService, IPhotosService photosService,
            IAppletsSavedService appletsSavedService)
        {
            _context = context;
            _configuration = configuration;
            _commonService = commonService;
            _cellService = cellService;
            _personnelId =
                Convert.ToInt32(httpContextAccessor.HttpContext.User.FindFirst(AuthDetailConstants.PERSONNELID)
                    ?.Value); //Accounts for anonymous call from interface engine do not remove "?"
            _atimsHubService = atimsHubService;
            _personService = personService;
            _inmateService = inmateService;
            _bookingReleaseService = bookingReleaseService;
            _altSentService = altSentService;
            _interfaceEngineService = interfaceEngineService;
            _photoService = photosService;
            _appletsSavedService = appletsSavedService;
        }

        public List<AgencyVm> GetAgencies() =>
            (from ag in _context.Agency
                where !ag.AgencyInactiveFlag.HasValue || ag.AgencyInactiveFlag == 0
                select new AgencyVm
                {
                    AgencyAbbreviation = ag.AgencyAbbreviation,
                    AgencyId = ag.AgencyId,
                    AgencyName = ag.AgencyName,
                    AgencyInactiveFlag = ag.AgencyInactiveFlag,
                    AgencyCourtFlag = ag.AgencyCourtFlag,
                    AgencyArrestingFlag = ag.AgencyArrestingFlag,
                    AgencyOriginatingFlag = ag.AgencyOriginatingFlag
                }).OrderBy(o => o.AgencyName).ToList();

        public List<FacilityVm> GetFacilities() =>
            _context.Facility.Where(w => w.DeleteFlag == 0)
                .Select(s => new FacilityVm
                {
                    FacilityAbbr = s.FacilityAbbr,
                    FacilityId = s.FacilityId,
                    FacilityName = s.FacilityName
                }).OrderBy(o => o.FacilityName).ToList();

        // Create Prebook Record
        public async Task<InmatePrebookVm> InsertPrebook(InmatePrebookVm value)
        {
            InmatePrebook inmatePrebook = new InmatePrebook
            {
                PersonId = value.PersonId,
                PersonLastName = value.PersonLastName,
                PersonFirstName = value.PersonFirstName,
                PersonMiddleName = value.PersonMiddleName,
                PersonSuffix = value.PersonSuffix,
                PersonDob = value.PersonDob,
                PreBookNumber = _commonService.GetGlobalNumber((int)AtimsGlobalNumber.PrebookNumber),
                ArrestDate = value.ArrestDate,
                PrebookDate = value.PrebookDate,
                PrebookType = value.PrebookType,
                ArrestAgencyId = value.ArrestAgencyId == 0 ? 1 : value.ArrestAgencyId, //temporarily hardcoded
                ArrestingOfficerId = value.ArrestOfficerId == 0 ? 1 : value.ArrestOfficerId, //temporarily hardcoded
                ArrestOfficerName = value.ArrestOfficerName,
                ArrestLocation = value.ArrestLocation,
                TransportingOfficerId = value.TransportOfficerId,
                TransportOfficerName = value.TransportOfficerName,
                TemporaryHold = value.TemporaryHold ? 1 : 0,
                CourtCommitFlag = value.CourtCommitFlag ? 1 : 0,
                PreBookContactNumber = value.ContactNumber,
                PrebookNotes = value.PrebookNotes,
                AppAoWizardFixedStepsId = 110,
                FacilityId = value.FacilityId,
                PersonnelId = _personnelId != 0 ? _personnelId : value.CreateBy, //Allow PersonnelId overwrite for interface engine.
                CreateBy = _personnelId != 0 ? _personnelId : value.CreateBy, //Allow PersonnelId overwrite for interface engine.
                CreateDate = DateTime.Now,
                UpdateBy = _personnelId != 0 ? _personnelId : value.UpdateBy != 0 ? value.UpdateBy
                            : value.CreateBy, //Allow PersonnelId overwrite for interface engine.
                UpdateDate = DateTime.Now,
                VehicleColor1 = value.VehicleColor1,
                VehicleColor2 = value.VehicleColor2,
                VehicleDescription = value.VehicleDescription,
                VehicleDisposition = value.VehicleDisposition,
                VehicleInvolvedFlag = value.VehicleInvolvedFlag ? 1 : 0,
                VehicleLicense = value.VehicleLicense,
                VehicleLocation = value.VehicleLocation,
                VehicleMakeId = value.VehicleMakeid,
                VehicleModelId = value.VehicleModelid,
                VehicleState = value.VehicleState,
                VehicleType = value.VehicleType,
                VehicleVin = value.VehicleVIN,
                VehicleYear = value.VehicleYear,
                VehicleFlagString = value.VehicleFlagString == null ? "" : string.Join(",", value.VehicleFlagString),
                //CaseNumber = value.CourtCommitFlag ? value.CaseNumber : "",
                ArrestType = value.ArrestType,
                ArrestCourtDocket = value.ArrestCourtDocket,
                ArrestCourtJurisdictionId = value.ArrestCourtJurisdictionId,
                ArrestSentenceDescription = value.ArrestSentenceDescription,
                ArrestSentenceAmended = value.ArrestSentenceAmended ? 1 : 0,
                ArrestSentencePenalInstitution = value.ArrestSentencePenalInstitution ? 1 : 0,
                ArrestSentenceOptionsRec = value.ArrestSentenceOptionsRec ? 1 : 0,
                ArrestSentenceAltSentNotAllowed = value.ArrestSentenceAltSentNotAllowed ? 1 : 0,
                ArrestSentenceNoEarlyRelease = value.ArrestSentenceNoEarlyRelease ? 1 : 0,
                ArrestSentenceNoLocalParole = value.ArrestSentenceNoLocalParole ? 1 : 0,
                ArrestSentenceDateInfo = value.ArrestSentenceDateInfo,
                ArrestsentenceType = value.ArrestsentenceType,
                ArrestSentenceFindings = value.ArrestSentenceFindings,
                ArrestSentenceJudgeId = value.ArrestSentenceJudgeId,
                ArrestSentenceConsecutiveFlag = value.ArrestSentenceConsecutiveFlag ? 1 : 0,
                ArrestSentenceStartDate = value.ArrestSentenceStartDate,
                ArrestSentenceDaysInterval = value.ArrestSentenceDaysInterval,
                ArrestSentenceDaysAmount = value.ArrestSentenceDaysAmount,
                ArrestSentenceFineDays = value.ArrestSentenceFineDays,
                ArrestSentenceFineAmount = value.ArrestSentenceFineAmount,
                ArrestSentenceFinePaid = value.ArrestSentenceFinePaid,
                ArrestSentenceFineType = value.ArrestSentenceFineType,
                ArrestSentenceFinePerDay = value.ArrestSentenceFinePerDay,
                ArrestSentenceDaysStayed = value.ArrestSentenceDaysStayed,
                ArrestTimeServedDays = value.ArrestTimeServedDays,
                ArrestSentenceForthwith = value.ArrestSentenceForthwith ? 1 : 0,
                CourtCommitType = value.CourtCommitType,
                IntakeReviewAccepted = value.PrebookReviewed,
                IntakeReviewBy = _personnelId,
                IntakeReviewDate = DateTime.Now,
                IdentificationAccepted = value.PersonIdentified,
                IdentificationAcceptedBy = _personnelId,
                IdentificationAcceptedDate = DateTime.Now
            };
            _context.Add(inmatePrebook);
            await _context.SaveChangesAsync();

            int inmatePrebookCaseId = 0;
            if (value.ArrestType.HasValue)
            {
                InmatePrebookCase inmatePrebookCase = new InmatePrebookCase
                {
                    InmatePrebookId = inmatePrebook.InmatePrebookId,
                    ArrestType = (decimal)value.ArrestType,
                    CaseNumber = value.CaseNumber,
                    CaseNote = value.CaseNote,
                    CreateDate = DateTime.Now,
                    CreateBy = _personnelId
                };
                _context.Add(inmatePrebookCase);
                await _context.SaveChangesAsync();
                inmatePrebookCaseId = inmatePrebookCase.InmatePrebookCaseId;
            }
            //Prebook arrestee condition entry
            if (value.DisplayArresteeCondition != null && value.DisplayArresteeCondition.Length > 0)
            {
                value.DisplayArresteeCondition.ToList().ForEach(f =>
               {
                   InmatePrebookCondition arresteeCondition = new InmatePrebookCondition
                   {
                       InmatePrebookId = inmatePrebook.InmatePrebookId,
                       ArresteeConditionLookupIndex = f,
                       CreateBy = _personnelId,
                       CreateDate = DateTime.Now
                   };
                   _context.Add(arresteeCondition);
               });
                await _context.SaveChangesAsync();
            }

            //Prebook arrestee BAC Method entry
            if (value.DisplayArresteeBAC != null && value.DisplayArresteeBAC.Length > 0)
            {
                value.DisplayArresteeBAC.ToList().ForEach(f =>
               {
                   InmatePrebookBACMethod arresteeBAC = new InmatePrebookBACMethod
                   {
                       InmatePrebookId = inmatePrebook.InmatePrebookId,
                       BACMethodLookupIndex = f,
                       BAC1 = value.ArresteeBAC1,
                       BAC2 = value.ArresteeBAC2,
                       CreateBy = _personnelId,
                       CreateDate = DateTime.Now
                   };
                   _context.Add(arresteeBAC);
               });
                await _context.SaveChangesAsync();
            }

            //Prebook arrestee behavior entry
            if (value.DisplayArresteeBehavior != null && value.DisplayArresteeBehavior.Length > 0)
            {
                value.DisplayArresteeBehavior.ToList().ForEach(f =>
               {
                   InmatePrebookBehavior arresteeBehavior = new InmatePrebookBehavior
                   {
                       InmatePrebookId = inmatePrebook.InmatePrebookId,
                       ArresteeBehaviorLookupIndex = f,
                       CreateBy = _personnelId,
                       CreateDate = DateTime.Now
                   };
                   _context.Add(arresteeBehavior);
               });
                await _context.SaveChangesAsync();
            }

            InmatePrebookVm inprebook = _context.InmatePrebook.Where(w =>
                w.InmatePrebookId == inmatePrebook.InmatePrebookId).Select(ip => new InmatePrebookVm
                {
                    InmatePrebookId = ip.InmatePrebookId,
                    IncarcerationId = ip.IncarcerationId,
                    TempHoldId = ip.TempHoldId,
                    PersonId = ip.PersonId ?? 0,
                    PersonLastName = ip.PersonLastName,
                    PersonFirstName = ip.PersonFirstName,
                    PersonMiddleName = ip.PersonMiddleName,
                    PersonSuffix = ip.PersonSuffix,
                    PersonDob = ip.PersonDob,
                    PrebookNumber = ip.PreBookNumber,
                    ArrestDate = ip.ArrestDate,
                    PrebookDate = ip.PrebookDate,
                    PrebookNotes = ip.PrebookNotes,
                    CaseNumber = value.CaseNumber,
                    ContactNumber = ip.PreBookContactNumber,
                    FacilityId = ip.FacilityId,
                    TemporaryHold = ip.TemporaryHold == 1,
                    ArrestOfficerId = ip.ArrestingOfficerId,
                    ArrestingOfficer = new PersonnelVm
                    {
                        PersonLastName = ip.ArrestingOfficer.PersonNavigation.PersonLastName,
                        PersonFirstName = ip.ArrestingOfficer.PersonNavigation.PersonFirstName,
                        OfficerBadgeNumber = ip.ArrestingOfficer.OfficerBadgeNum
                    },
                    ArrestOfficerName = ip.ArrestOfficerName,
                    ArrestAgencyId = ip.ArrestAgency.AgencyId,
                    ArrestAgencyAbbr = ip.ArrestAgency.AgencyAbbreviation,
                    ArrestAgencyName = ip.ArrestAgency.AgencyName,
                    FacilityAbbr = ip.Facility.FacilityAbbr,
                    ArrestLocation = ip.ArrestLocation,
                    TransportOfficerName = ip.TransportOfficerName,
                    TransportOfficerId = ip.TransportingOfficerId,
                    TransportOfficerLastName = ip.TransportingOfficer.PersonNavigation.PersonLastName,
                    TransportOfficerFirstName = ip.TransportingOfficer.PersonNavigation.PersonFirstName,
                    TransportOfficerNumber = ip.TransportingOfficer.PersonnelNumber,
                    VehicleColor1 = ip.VehicleColor1,
                    VehicleColor2 = ip.VehicleColor2,
                    VehicleDescription = ip.VehicleDescription,
                    VehicleDisposition = ip.VehicleDisposition,
                    VehicleInvolvedFlag = ip.VehicleInvolvedFlag == 1,
                    VehicleLicense = ip.VehicleLicense,
                    VehicleLocation = ip.VehicleLocation,
                    VehicleMakeid = ip.VehicleMakeId,
                    VehicleModelid = ip.VehicleModelId,
                    VehicleState = ip.VehicleState,
                    VehicleType = ip.VehicleType,
                    VehicleVIN = ip.VehicleVin,
                    VehicleYear = ip.VehicleYear,
                    VehicleFlagString = ip.VehicleFlagString == null ? null
                    : ip.VehicleFlagString.Split(',', StringSplitOptions.RemoveEmptyEntries),
                    CourtCommitType = ip.CourtCommitType,
                    ArrestType = value.ArrestType,
                    ArrestCourtDocket = ip.ArrestCourtDocket,
                    ArrestCourtJurisdictionId = ip.ArrestCourtJurisdictionId,
                    ArrestSentenceDescription = ip.ArrestSentenceDescription,
                    ArrestSentenceAmended = ip.ArrestSentenceAmended == 1,
                    ArrestSentencePenalInstitution = ip.ArrestSentencePenalInstitution == 1,
                    ArrestSentenceOptionsRec = ip.ArrestSentenceOptionsRec == 1,
                    ArrestSentenceAltSentNotAllowed = ip.ArrestSentenceAltSentNotAllowed == 1,
                    ArrestSentenceNoEarlyRelease = ip.ArrestSentenceNoEarlyRelease == 1,
                    ArrestSentenceNoLocalParole = ip.ArrestSentenceNoLocalParole == 1,
                    ArrestSentenceDateInfo = ip.ArrestSentenceDateInfo,
                    ArrestsentenceType = ip.ArrestsentenceType,
                    ArrestSentenceFindings = ip.ArrestSentenceFindings,
                    ArrestSentenceJudgeId = ip.ArrestSentenceJudgeId,
                    ArrestSentenceConsecutiveFlag = ip.ArrestSentenceConsecutiveFlag == 1,
                    ArrestSentenceStartDate = ip.ArrestSentenceStartDate,
                    ArrestSentenceDaysInterval = ip.ArrestSentenceDaysInterval,
                    ArrestSentenceDaysAmount = ip.ArrestSentenceDaysAmount,
                    ArrestSentenceFineDays = ip.ArrestSentenceFineDays,
                    ArrestSentenceFineAmount = ip.ArrestSentenceFineAmount,
                    ArrestSentenceFinePaid = ip.ArrestSentenceFinePaid,
                    ArrestSentenceFineType = ip.ArrestSentenceFineType,
                    ArrestSentenceFinePerDay = ip.ArrestSentenceFinePerDay,
                    ArrestSentenceDaysStayed = ip.ArrestSentenceDaysStayed,
                    ArrestTimeServedDays = ip.ArrestTimeServedDays,
                    ArrestSentenceForthwith = ip.ArrestSentenceForthwith == 1
                }).Single();
            if (inmatePrebookCaseId > 0)
            {
                inprebook.InmatePrebookCaseId = inmatePrebookCaseId;
            }
            if (inprebook.PersonId.HasValue)
            {
                inprebook.IdentifiedPerson = _personService.GetPersonDetails(inprebook.PersonId.Value);
            }

            inprebook.ArresteeBAC = _context.InmatePrebookBACMethod.Where(w =>
                    w.InmatePrebookId == inmatePrebook.InmatePrebookId && !w.DeleteFlag)
                .Select(s => new InmatePreBookCondition
                {
                    InmatePrebookId = s.InmatePrebookId,
                    BACMethodLookupIndex = s.BACMethodLookupIndex,
                    BAC1 = s.BAC1,
                    BAC2 = s.BAC2
                }).ToList();
            inprebook.ArresteeCondition = _context.InmatePrebookCondition.Where(w =>
                    w.InmatePrebookId == inmatePrebook.InmatePrebookId && !w.DeleteFlag)
                .Select(s => new InmatePreBookCondition
                {
                    InmatePrebookId = s.InmatePrebookId,
                    ArresteeConditionLookupIndex = s.ArresteeConditionLookupIndex,
                }).ToList();
            inprebook.ArresteeBehavior = _context.InmatePrebookBehavior.Where(w =>
                    w.InmatePrebookId == inmatePrebook.InmatePrebookId && !w.DeleteFlag)
                .Select(s => new InmatePreBookCondition
                {
                    InmatePrebookId = s.InmatePrebookId,
                    ArresteeBehaviorLookupIndex = s.ArresteeBehaviorLookupIndex,
                }).ToList();
            return inprebook;
        }

        //Update Prebook Record
        public async Task<int> UpdatePrebook(InmatePrebookVm inmatePrebookVm)
        {
            string programCaseSiteOption = _context.SiteOptions.SingleOrDefault(so =>
                so.SiteOptionsName == SiteOptionsConstants.MULTIPLEPREBOOKCASE)?.SiteOptionsValue;

            InmatePrebook a =
                _context.InmatePrebook.Single(ip => ip.InmatePrebookId == inmatePrebookVm.InmatePrebookId);
            a.PersonLastName = inmatePrebookVm.PersonLastName;
            a.PersonFirstName = inmatePrebookVm.PersonFirstName;
            a.PersonMiddleName = inmatePrebookVm.PersonMiddleName;
            a.PersonSuffix = inmatePrebookVm.PersonSuffix;
            a.PersonDob = inmatePrebookVm.PersonDob;
            a.PersonId = inmatePrebookVm.PersonId == 0 ? null : inmatePrebookVm.PersonId;
            a.ArrestDate = inmatePrebookVm.ArrestDate;
            a.PrebookDate = inmatePrebookVm.PrebookDate;
            a.PrebookType = inmatePrebookVm.PrebookType;
            a.ArrestAgencyId = inmatePrebookVm.ArrestAgencyId == 0 ? 1 : inmatePrebookVm.ArrestAgencyId;
            a.ArrestingOfficerId = inmatePrebookVm.ArrestOfficerId == 0 ? 1 : inmatePrebookVm.ArrestOfficerId;
            a.ArrestOfficerName = inmatePrebookVm.ArrestOfficerName;
            a.ArrestLocation = inmatePrebookVm.ArrestLocation;
            a.TransportingOfficerId = inmatePrebookVm.TransportOfficerId;
            a.TransportOfficerName = inmatePrebookVm.TransportOfficerName;
            a.TemporaryHold = inmatePrebookVm.TemporaryHold ? 1 : 0;
            a.CourtCommitFlag = inmatePrebookVm.CourtCommitFlag ? 1 : 0;
            a.PreBookContactNumber = inmatePrebookVm.ContactNumber;
            a.PrebookNotes = inmatePrebookVm.PrebookNotes;
            a.FacilityId = inmatePrebookVm.FacilityId;
            a.UpdateBy = _personnelId;
            a.UpdateDate = DateTime.Now;
            a.VehicleColor1 = inmatePrebookVm.VehicleColor1;
            a.VehicleColor2 = inmatePrebookVm.VehicleColor2;
            a.VehicleDescription = inmatePrebookVm.VehicleDescription;
            a.VehicleDisposition = inmatePrebookVm.VehicleDisposition;
            a.VehicleInvolvedFlag = inmatePrebookVm.VehicleInvolvedFlag ? 1 : 0;
            a.VehicleLicense = inmatePrebookVm.VehicleLicense;
            a.VehicleLocation = inmatePrebookVm.VehicleLocation;
            a.VehicleMakeId = inmatePrebookVm.VehicleMakeid;
            a.VehicleModelId = inmatePrebookVm.VehicleModelid;
            a.VehicleState = inmatePrebookVm.VehicleState;
            a.VehicleType = inmatePrebookVm.VehicleType;
            a.VehicleVin = inmatePrebookVm.VehicleVIN;
            a.VehicleYear = inmatePrebookVm.VehicleYear;
            a.VehicleFlagString = inmatePrebookVm.VehicleFlagString == null ? ""
                : string.Join(",", inmatePrebookVm.VehicleFlagString);
            a.ArrestType = inmatePrebookVm.ArrestType;
            a.ArrestCourtDocket = inmatePrebookVm.ArrestCourtDocket;
            a.ArrestCourtJurisdictionId = inmatePrebookVm.ArrestCourtJurisdictionId;
            a.ArrestSentenceDescription = inmatePrebookVm.ArrestSentenceDescription;
            a.ArrestSentenceAmended = inmatePrebookVm.ArrestSentenceAmended ? 1 : 0;
            a.ArrestSentencePenalInstitution = inmatePrebookVm.ArrestSentencePenalInstitution ? 1 : 0;
            a.ArrestSentenceOptionsRec = inmatePrebookVm.ArrestSentenceOptionsRec ? 1 : 0;
            a.ArrestSentenceAltSentNotAllowed = inmatePrebookVm.ArrestSentenceAltSentNotAllowed ? 1 : 0;
            a.ArrestSentenceNoEarlyRelease = inmatePrebookVm.ArrestSentenceNoEarlyRelease ? 1 : 0;
            a.ArrestSentenceNoLocalParole = inmatePrebookVm.ArrestSentenceNoLocalParole ? 1 : 0;
            a.ArrestSentenceDateInfo = inmatePrebookVm.ArrestSentenceDateInfo;
            a.ArrestsentenceType = inmatePrebookVm.ArrestsentenceType;
            a.ArrestSentenceFindings = inmatePrebookVm.ArrestSentenceFindings;
            a.ArrestSentenceJudgeId = inmatePrebookVm.ArrestSentenceJudgeId == 0 ? null : inmatePrebookVm.ArrestSentenceJudgeId;
            a.ArrestSentenceConsecutiveFlag = inmatePrebookVm.ArrestSentenceConsecutiveFlag ? 1 : 0;
            a.ArrestSentenceStartDate = inmatePrebookVm.ArrestSentenceStartDate;
            a.ArrestSentenceDaysInterval = inmatePrebookVm.ArrestSentenceDaysInterval;
            a.ArrestSentenceDaysAmount = inmatePrebookVm.ArrestSentenceDaysAmount;
            a.ArrestSentenceFineDays = inmatePrebookVm.ArrestSentenceFineDays;
            a.ArrestSentenceFineAmount = inmatePrebookVm.ArrestSentenceFineAmount;
            a.ArrestSentenceFinePaid = inmatePrebookVm.ArrestSentenceFinePaid;
            a.ArrestSentenceFineType = inmatePrebookVm.ArrestSentenceFineType;
            a.ArrestSentenceFinePerDay = inmatePrebookVm.ArrestSentenceFinePerDay;
            a.ArrestSentenceDaysStayed = inmatePrebookVm.ArrestSentenceDaysStayed;
            a.ArrestTimeServedDays = inmatePrebookVm.ArrestTimeServedDays;
            a.ArrestSentenceForthwith = inmatePrebookVm.ArrestSentenceForthwith ? 1 : 0;
            a.CourtCommitType = inmatePrebookVm.CourtCommitType;

            if (programCaseSiteOption == SiteOptionsConstants.OFF && !inmatePrebookVm.CourtCommitFlag)
            {
                InmatePrebookCase inmatePrebookCase = _context.InmatePrebookCase
                    .OrderByDescending(i => i.InmatePrebookCaseId)
                    .LastOrDefault(i => i.InmatePrebookId == inmatePrebookVm.InmatePrebookId);
                bool isNew = false;
                if (inmatePrebookCase == null)
                {
                    isNew = true;
                    inmatePrebookCase = new InmatePrebookCase
                    {
                        InmatePrebookId = inmatePrebookVm.InmatePrebookId,
                        CreateDate = DateTime.Now,
                        CreateBy = _personnelId
                    };
                }
                inmatePrebookCase.CaseNumber = inmatePrebookVm.CaseNumber;
                if (isNew)
                {
                    _context.Add(inmatePrebookCase);
                }
                await _context.SaveChangesAsync();
            }

            //Update inmate prebook condition
            inmatePrebookVm.DisplayArresteeCondition.ToList().ForEach(f =>
            {
               InmatePrebookCondition arresteePrebookCondition = _context.InmatePrebookCondition
                  .SingleOrDefault(s => s.InmatePrebookId == inmatePrebookVm.InmatePrebookId && s.ArresteeConditionLookupIndex == f );
                   
                   if(arresteePrebookCondition is null)
                   {
                    InmatePrebookCondition arresteeCondition = new InmatePrebookCondition
                    {
                        InmatePrebookId = inmatePrebookVm.InmatePrebookId,
                        ArresteeConditionLookupIndex = f,
                        CreateBy = _personnelId,
                        CreateDate = DateTime.Now
                    };
                    _context.Add(arresteeCondition);
                   }else
                   {
                         arresteePrebookCondition.DeleteFlag = false;
                         arresteePrebookCondition.DeleteDate = null;
                         arresteePrebookCondition.DeleteBy = null;
                   }   
            });

             List<InmatePreBookCondition> arresteeconditionLst =
                _context.InmatePrebookCondition.Where(w => !inmatePrebookVm.DisplayArresteeCondition.Contains(w.ArresteeConditionLookupIndex) && 
                  w.InmatePrebookId == inmatePrebookVm.InmatePrebookId)
                .Select(s => new InmatePreBookCondition
                {
                    ArresteeConditionLookupIndex = s.ArresteeConditionLookupIndex,
                    InmatePrebookId = s.InmatePrebookId,
                    DeleteFlag = s.DeleteFlag
                }).ToList();
             arresteeconditionLst.ForEach(f =>
             {
                 InmatePrebookCondition arresteecondition = _context.InmatePrebookCondition.Single(s =>
                         s.InmatePrebookId == inmatePrebookVm.InmatePrebookId &&
                         s.ArresteeConditionLookupIndex == f.ArresteeConditionLookupIndex);
                 arresteecondition.DeleteFlag = true;
                 arresteecondition.DeleteBy = _personnelId;
                 arresteecondition.DeleteDate = DateTime.Now;
             });

            //Updating Inmate prebook behavior
            inmatePrebookVm.DisplayArresteeBehavior.ToList().ForEach(f =>
            {
                InmatePrebookBehavior arresteeBehavior = _context.InmatePrebookBehavior
                    .SingleOrDefault(s =>
                        s.InmatePrebookId == inmatePrebookVm.InmatePrebookId && s.ArresteeBehaviorLookupIndex == f);

                if (arresteeBehavior is null)
                {
                    InmatePrebookBehavior arrestePrebookBehavior = new InmatePrebookBehavior
                    {
                        InmatePrebookId = inmatePrebookVm.InmatePrebookId,
                        ArresteeBehaviorLookupIndex = f,
                        CreateBy = _personnelId,
                        CreateDate = DateTime.Now
                    };
                    _context.Add(arrestePrebookBehavior);
                }
                else
                {
                    arresteeBehavior.DeleteFlag = false;
                    arresteeBehavior.DeleteDate = null;
                    arresteeBehavior.DeleteBy = null;
                }
            });
               List<InmatePreBookCondition> arresteebehaviorLst =
                _context.InmatePrebookBehavior.Where(w => !inmatePrebookVm.DisplayArresteeBehavior.Contains(w.ArresteeBehaviorLookupIndex) && 
                  w.InmatePrebookId == inmatePrebookVm.InmatePrebookId)
                .Select(s => new InmatePreBookCondition
                {
                    ArresteeBehaviorLookupIndex = s.ArresteeBehaviorLookupIndex,
                    InmatePrebookId = s.InmatePrebookId,
                    DeleteFlag = s.DeleteFlag
                }).ToList();
                arresteebehaviorLst.ForEach(f =>
                {
                    InmatePrebookBehavior arresteebehavior = _context.InmatePrebookBehavior.Single(s =>
                            s.InmatePrebookId == inmatePrebookVm.InmatePrebookId &&
                            s.ArresteeBehaviorLookupIndex == f.ArresteeBehaviorLookupIndex);
                    arresteebehavior.DeleteFlag = true;
                    arresteebehavior.DeleteBy = _personnelId;
                    arresteebehavior.DeleteDate = DateTime.Now;
                });

             //Updating  BACmethod
              inmatePrebookVm.DisplayArresteeBAC.ToList().ForEach(f =>
            {
               InmatePrebookBACMethod arresteeBAC = _context.InmatePrebookBACMethod
                  .SingleOrDefault(s => s.InmatePrebookId == inmatePrebookVm.InmatePrebookId && s.BACMethodLookupIndex == f );
                   
                   if(arresteeBAC is null)
                   {
                    InmatePrebookBACMethod arrestePrebookBac = new InmatePrebookBACMethod
                    {
                        InmatePrebookId = inmatePrebookVm.InmatePrebookId,
                        BACMethodLookupIndex = f,
                        CreateBy = _personnelId,
                        CreateDate = DateTime.Now,
                        BAC1 = inmatePrebookVm.ArresteeBAC1,
                        BAC2 = inmatePrebookVm.ArresteeBAC2
                    };
                    _context.Add(arrestePrebookBac);
                   }else
                   {
                         arresteeBAC.DeleteFlag = false;
                         arresteeBAC.DeleteDate = null;
                         arresteeBAC.DeleteBy = null;
                         arresteeBAC.BAC1 = inmatePrebookVm.ArresteeBAC1;
                         arresteeBAC.BAC2 = inmatePrebookVm.ArresteeBAC2;
                   }   
            });
             
           List<InmatePreBookCondition> arresteeBACMethod =
                _context.InmatePrebookBACMethod.Where(w => !inmatePrebookVm.DisplayArresteeBAC.Contains(w.BACMethodLookupIndex) && 
                  w.InmatePrebookId == inmatePrebookVm.InmatePrebookId)
                .Select(s => new InmatePreBookCondition
                {
                    BACMethodLookupIndex = s.BACMethodLookupIndex,
                    InmatePrebookId = s.InmatePrebookId,
                    DeleteFlag = s.DeleteFlag
                }).ToList();
                arresteeBACMethod.ForEach(f =>
                {
                    InmatePrebookBACMethod arresteeBAC = _context.InmatePrebookBACMethod.Single(s =>
                            s.InmatePrebookId == inmatePrebookVm.InmatePrebookId &&
                            s.BACMethodLookupIndex == f.BACMethodLookupIndex);
                    arresteeBAC.DeleteFlag = true;
                    arresteeBAC.DeleteBy = _personnelId;
                    arresteeBAC.DeleteDate = DateTime.Now;
                });
            return await _context.SaveChangesAsync();
        }

        //Update Prebook Last
        public async Task<int> UpdatePrebookLastStep(InmatePrebookVm inmatePrebookVm)
        {
            InmatePrebook inmatePrebook =
                _context.InmatePrebook.SingleOrDefault(ip => ip.InmatePrebookId == inmatePrebookVm.InmatePrebookId);
            if (inmatePrebook == null)
            {
                return 0;
            }
            return await _context.SaveChangesAsync();
        }

        #region Prebook Charges

        public List<PrebookCharge> GetPrebookCharges(int inmatePrebookId, bool deleteFlag, int prebookWarrantId,
            int incarcerationId = 0, int arrestId = 0)
        {
            List<InmatePrebookCharge> dbInmatePrebookCharge;
            if (arrestId > 0)
            {
                dbInmatePrebookCharge = _context.InmatePrebookCharge.Where(i => i.InmatePrebookId == inmatePrebookId).ToList();
                InmatePrebook inmatePrebook = _context.InmatePrebook.SingleOrDefault(x => x.ArrestId == arrestId);
                if (inmatePrebook != null)
                {
                    inmatePrebookId = inmatePrebook.InmatePrebookId;

                    if (inmatePrebookId > 0)
                    {
                        dbInmatePrebookCharge = _context.InmatePrebookCharge
                                                .Where(pcr => pcr.InmatePrebookId == inmatePrebookId
                                                    && (!pcr.CrimeId.HasValue || pcr.CrimeId == 0)).ToList();
                    }
                    else
                    {
                        dbInmatePrebookCharge = _context.InmatePrebookCharge
                                                .Where(pcr => pcr.InmatePrebook.IncarcerationId == incarcerationId
                                                    && (!pcr.CrimeId.HasValue || pcr.CrimeId == 0)).ToList();
                    }
                }
            }
            else
            {
                dbInmatePrebookCharge = _context.InmatePrebookCharge
                    .Where(pcr => (incarcerationId == 0 || pcr.InmatePrebook.IncarcerationId == incarcerationId
                            && (!pcr.CrimeId.HasValue || pcr.CrimeId == 0))
                       && (incarcerationId > 0 || pcr.InmatePrebookId == inmatePrebookId)).ToList();
            }
            List<PrebookCharge> prebookCharge = dbInmatePrebookCharge
                .Where(x => (prebookWarrantId == 0 || x.InmatePrebookWarrantId == prebookWarrantId)
                    && (prebookWarrantId > 0 || !x.InmatePrebookWarrantId.HasValue)
                    && (deleteFlag || !x.DeleteFlag.HasValue))
                .Select(y => new PrebookCharge
                {
                    InmatePrebookId = y.InmatePrebookId,
                    InmatePrebookChargeId = y.InmatePrebookChargeId,
                    InmatePrebookWarrantId = y.InmatePrebookWarrantId,
                    DeleteFlag = y.DeleteFlag == 1,
                    CrimeLookupId = y.CrimeLookupId,
                    CrimeCount = y.CrimeCount ?? 0,
                    BailAmount = y.BailAmount,
                    BailNoBailFlag = y.BailNoBailFlag == 1,
                    CrimeType = y.CrimeType,
                    CrimeNotes = y.CrimeNotes,
                    ChargeQualifierId = int.Parse(!string.IsNullOrEmpty(y.ChargeQualifierLookup) ? y.ChargeQualifierLookup : "0"),
                    CrimeStatusLookup = y.CrimeStatusLookup,
                    CrimeQualifierLookup = y.ChargeQualifierLookup,
                    CreateBy = y.CreateBy,
                    UpdateBy = y.UpdateBy,
                    CreateDate = y.CreateDate,
                    UpdateDate = y.UpdateDate,
                    OffenceDate = y.OffenceDate,
                    InmatePrebookCaseId = y.InmatePrebookCaseId ?? 0
                }).ToList();

            int?[] crimeLookupIds = prebookCharge.Select(s => s.CrimeLookupId).ToArray();

            List<PrebookCharge> crimeInfo = _context.CrimeLookup.Where(w => crimeLookupIds.Contains(w.CrimeLookupId))
              .Select(s => new PrebookCharge
              {
                  CrimeCodeType = s.CrimeCodeType,
                  CrimeSection = s.CrimeSection,
                  CrimeDescription = s.CrimeDescription,
                  CrimeStatuteId = s.CrimeStatuteId,
                  CrimeStatuteCode = s.CrimeStatuteCode,
                  CrimeSubSection = s.CrimeSubSection,
                  CrimeLookupId = s.CrimeLookupId
              }).ToList();

            int[] ClassOfficerIds = prebookCharge.Select(s => new[] { s.CreateBy, s.UpdateBy })
                .SelectMany(s => s).Where(w => w.HasValue).Select(s => s.Value).ToArray();

            List<PersonnelVm> lstPersonnel = _context.Personnel.Where(w => ClassOfficerIds.Contains(w.PersonnelId))
                .Select(s => new PersonnelVm
                {
                    OfficerBadgeNumber = s.OfficerBadgeNum,
                    PersonnelId = s.PersonnelId,
                    PersonId = s.PersonId
                }).ToList();

            int[] personnelPersonIds = lstPersonnel.Select(p => p.PersonId).ToArray();
            List<Person> lstPersonnelPerson = _context.Person.Where(p => personnelPersonIds.Contains(p.PersonId)).ToList();
            lstPersonnel.ForEach(item =>
            {
                Person person = lstPersonnelPerson.SingleOrDefault(w => w.PersonId == item.PersonId);
                if (person == null) return;
                item.PersonFirstName = person.PersonFirstName;
                item.PersonLastName = person.PersonLastName;
                item.PersonMiddleName = person.PersonMiddleName;
            });

            prebookCharge.ForEach(item =>
            {
                PrebookCharge crimeLookupDetails = crimeInfo.SingleOrDefault(w => w.CrimeLookupId == item.CrimeLookupId);
                if (crimeLookupDetails != null)
                {
                    item.CrimeCodeType = crimeLookupDetails.CrimeCodeType;
                    item.CrimeSection = crimeLookupDetails.CrimeSection;
                    item.CrimeDescription = crimeLookupDetails.CrimeDescription;
                    item.CrimeStatuteId = crimeLookupDetails.CrimeStatuteId;
                    item.CrimeStatuteCode = crimeLookupDetails.CrimeStatuteCode;
                    item.CrimeSubSection = crimeLookupDetails.CrimeSubSection;
                }
                PersonnelVm createBy = lstPersonnel.SingleOrDefault(x => x.PersonnelId == item.CreateBy);
                item.CreatedPersonnel = createBy;

                PersonnelVm updateBy = lstPersonnel.SingleOrDefault(x => x.PersonnelId == item.UpdateBy);
                item.UpdatedPersonnel = updateBy;
            });
            List<CrimeForce> dbCrimeForce = _context.CrimeForce
                .Where(cf => (incarcerationId == 0 || cf.InmatePrebook.IncarcerationId == incarcerationId
                        && (!cf.ArrestId.HasValue || cf.ArrestId == 0))
                    && (incarcerationId > 0 || cf.InmatePrebookId == inmatePrebookId)).ToList();

            prebookCharge.AddRange(dbCrimeForce.Where(x => !x.ForceSupervisorReviewFlag.HasValue
                && (prebookWarrantId == 0 || x.InmatePrebookWarrantId == prebookWarrantId)
                && (prebookWarrantId > 0 || !x.InmatePrebookWarrantId.HasValue)
                && (deleteFlag || x.DeleteFlag == 0))
               .Select(y => new PrebookCharge
               {
                   InmatePrebookId = y.InmatePrebookId ?? 0,
                   CrimeForceId = y.CrimeForceId,
                   InmatePrebookWarrantId = y.InmatePrebookWarrantId,
                   DeleteFlag = y.DeleteFlag == 1,
                   CrimeLookupId = y.ForceCrimeLookupId,
                   CrimeCount = y.TempCrimeCount ?? 0,
                   CrimeCodeType = y.TempCrimeCodeType,
                   CrimeSection = y.TempCrimeSection,
                   CrimeDescription = y.TempCrimeDescription,
                   CrimeStatuteCode = y.TempCrimeStatuteCode,
                   BailAmount = y.BailAmount,
                   BailNoBailFlag = y.BailNoBailFlag == 1,
                   CrimeType = y.TempCrimeType,
                   CrimeQualifierLookup = y.ChargeQualifierLookup,
                   CrimeNotes = y.TempCrimeNotes,
                   ChargeQualifierId = int.Parse(!string.IsNullOrEmpty(y.ChargeQualifierLookup) ? y.ChargeQualifierLookup : "0"),
                   CrimeStatusLookup = y.TempCrimeStatusLookup,
                   CrimeGroupId = int.Parse(!string.IsNullOrEmpty(y.TempCrimeGroup) ? y.TempCrimeGroup : "0"),
                   IsForceCharge = true,
                   InmatePrebookCaseId = y.InmatePrebookCaseId ?? 0,
                   OffenceDate = y.OffenceDate,
               }));

            prebookCharge.ForEach(a =>
            {
                a.CrimeStatus = _commonService.GetLookupList(LookupConstants.CRIMETYPE)
                        .FirstOrDefault(z => z.LookupIndex == Convert.ToInt32(a.CrimeType))?.LookupDescription;

                a.CrimeQualifier = string.IsNullOrEmpty(a.CrimeQualifierLookup) ? null
                    : _commonService.GetLookupList(LookupConstants.CHARGEQUALIFIER)
                        .FirstOrDefault(z => z.LookupIndex == Convert.ToInt32(a.CrimeQualifierLookup))
                        ?.LookupDescription;
            });

            return prebookCharge;
        }

        public async Task<int> InsertInmatePrebookCharge(PrebookCharge prebookCharge)
        {
            InmatePrebookCharge inmatePrebookCharge = new InmatePrebookCharge
            {
                InmatePrebookId = prebookCharge.InmatePrebookId ?? 0,
                InmatePrebookWarrantId = prebookCharge.InmatePrebookWarrantId,
                CrimeLookupId = prebookCharge.CrimeLookupId,
                CrimeCount = prebookCharge.CrimeCount,
                BailAmount = prebookCharge.BailAmount,
                BailNoBailFlag = prebookCharge.BailNoBailFlag ? 1 : 0,
                CrimeType = prebookCharge.CrimeType,
                CrimeNotes = prebookCharge.CrimeNotes,
                ChargeQualifierLookup =
                    prebookCharge.ChargeQualifierId.HasValue ? prebookCharge.ChargeQualifierId.ToString() : default,
                CrimeStatusLookup = prebookCharge.CrimeStatusLookup,
                CreateBy = _personnelId,
                CreateDate = DateTime.Now,
                InmatePrebookCaseId = prebookCharge.InmatePrebookCaseId,
                OffenceDate = prebookCharge.OffenceDate
            };
            _context.Add(inmatePrebookCharge);
            CrimeHistory crimeHistory = new CrimeHistory
            {
                InmatePrebookChargeId = inmatePrebookCharge.InmatePrebookChargeId,
                CrimeId = prebookCharge.CrimeId,
                CrimeForceId = prebookCharge.CrimeForceId,
                CrimeLookupId = prebookCharge.CrimeLookupId,
                CrimeType = prebookCharge.CrimeType,
                CrimeNotes = prebookCharge.CrimeNotes,
                CrimeCount = prebookCharge.CrimeCount,
                CrimeDeleteFlag = prebookCharge.DeleteFlag ? 1 : 0,
                BailAmount = prebookCharge.BailAmount,
                BailNoBailFlag = prebookCharge.BailNoBailFlag ? 1 : 0,
                CreatDate = DateTime.Now,
                CreatedBy = _personnelId,
                ChargeQualifierLookup =
                    prebookCharge.ChargeQualifierId.HasValue ? prebookCharge.ChargeQualifierId.ToString() : default,
                CrimeStatusLookup = prebookCharge.CrimeStatusLookup
            };
            _context.Add(crimeHistory);
            return await _context.SaveChangesAsync();
        }

        public async Task<int> InsertForceCharge(PrebookCharge prebookCharge)
        {
            if (prebookCharge.CrimeId > 0)
            {
                await DeleteCrimeForUpdateForce(prebookCharge);
            }
            if (prebookCharge.InmatePrebookChargeId > 0)
            {
                await DeleteInmatePrebookChargeForUpdateForce(prebookCharge);
            }

            CrimeForce crimeForce = new CrimeForce
            {
                InmatePrebookId = prebookCharge.InmatePrebookId,
                ArrestId = prebookCharge.ArrestId,
                InmatePrebookWarrantId = prebookCharge.InmatePrebookWarrantId,
                WarrantId = prebookCharge.WarrantId,
                TempCrimeCount = prebookCharge.CrimeCount,
                TempCrimeCodeType = prebookCharge.CrimeCodeType,
                TempCrimeSection = prebookCharge.CrimeSection,
                TempCrimeDescription = prebookCharge.CrimeDescription,
                TempCrimeStatuteCode = prebookCharge.CrimeStatuteCode,
                BailAmount = !prebookCharge.BailNoBailFlag ? prebookCharge.BailAmount : 0,
                BailNoBailFlag = prebookCharge.BailNoBailFlag ? 1 : 0,
                TempCrimeType = prebookCharge.CrimeType,
                TempCrimeNotes = prebookCharge.CrimeNotes,
                TempCrimeGroup = prebookCharge.CrimeGroupId.ToString(),
                TempCrimeStatusLookup = prebookCharge.CrimeStatusLookup,
                ChargeQualifierLookup = prebookCharge.ChargeQualifierId.HasValue
                    ? prebookCharge.ChargeQualifierId.ToString() : string.Empty,
                TempCrimeQualifierLookup = prebookCharge.ChargeQualifierId,
                CreateBy = _personnelId,
                CreateDate = DateTime.Now,
                OffenceDate = prebookCharge.OffenceDate
            };
            if (prebookCharge.InmatePrebookCaseId > 0)
            {
                crimeForce.InmatePrebookCaseId = prebookCharge.InmatePrebookCaseId;
            }
            _context.CrimeForce.Add(crimeForce);
            await _context.SaveChangesAsync();
            _interfaceEngineService.Export(new ExportRequestVm
            {
                EventName = EventNameConstants.FORCECHARGE,
                PersonnelId = _personnelId,
                Param1 = _context.InmatePrebook
                .SingleOrDefault(x => x.InmatePrebookId == prebookCharge.InmatePrebookId)?.PersonId.ToString(),
                Param2 = crimeForce.CrimeForceId.ToString()
            });
            CrimeHistory crimeHistory = new CrimeHistory
            {
                InmatePrebookChargeId = prebookCharge.InmatePrebookChargeId,
                CrimeId = prebookCharge.CrimeId,
                CrimeForceId = crimeForce.CrimeForceId,
                CrimeLookupId = prebookCharge.CrimeLookupId,
                CrimeType = prebookCharge.CrimeType,
                CrimeNotes = prebookCharge.CrimeNotes,
                CrimeCount = prebookCharge.CrimeCount,
                CrimeDeleteFlag = prebookCharge.DeleteFlag ? 1 : 0,
                BailAmount = prebookCharge.BailAmount,
                BailNoBailFlag = prebookCharge.BailNoBailFlag ? 1 : 0,
                CreatDate = DateTime.Now,
                OffenceDate = prebookCharge.OffenceDate,
                CreatedBy = _personnelId,
                ChargeQualifierLookup = prebookCharge.ChargeQualifierId.HasValue
                        ? prebookCharge.ChargeQualifierId.ToString() : string.Empty,
                CrimeQualifierLookup = prebookCharge.ChargeQualifierId,
                CrimeStatusLookup = prebookCharge.CrimeStatusLookup,
                CrimeLookupForceString = prebookCharge.CrimeLookupForceString
            };
            _context.CrimeHistory.Add(crimeHistory);
            //Update OverAll ChargeLevel
            if (prebookCharge.ArrestId > 0)
            {
                int? incarcerationId = _context.IncarcerationArrestXref
                    .Where(i => i.Arrest.ArrestId == prebookCharge.ArrestId)
                    .OrderByDescending(i => i.Incarceration.IncarcerationId)
                    .Select(i => i.Incarceration.IncarcerationId).FirstOrDefault();
                if (incarcerationId > 0)
                    _bookingReleaseService.UpdateOverAllChargeLevel((int)incarcerationId);
            }
            return await _context.SaveChangesAsync();
        }

        //Delete Crime Details For Update Crime to Force
        private async Task DeleteCrimeForUpdateForce(PrebookCharge prebookCharge)
        {
            Crime dbCrime = _context.Crime.SingleOrDefault(cr => cr.CrimeId == prebookCharge.CrimeId);
            if (dbCrime != null)
                _context.Crime.Remove(dbCrime);
            IEnumerable<CrimeHistory> dbCrimeHistory = _context.CrimeHistory
                .Where(ch => ch.CrimeId == prebookCharge.CrimeId);
            if (dbCrimeHistory.Any())
                _context.CrimeHistory.RemoveRange(dbCrimeHistory);
            await _context.SaveChangesAsync();
        }


        private async Task DeleteInmatePrebookChargeForUpdateForce(PrebookCharge prebookCharge)
        {
            InmatePrebookCharge dbCrime = _context.InmatePrebookCharge.SingleOrDefault(cr => cr.InmatePrebookChargeId == prebookCharge.InmatePrebookChargeId);
            if (dbCrime != null)
                _context.InmatePrebookCharge.Remove(dbCrime);
            IEnumerable<CrimeHistory> dbCrimeHistory = _context.CrimeHistory
                .Where(ch => ch.InmatePrebookChargeId == prebookCharge.InmatePrebookChargeId);
            if (dbCrimeHistory.Any())
                _context.CrimeHistory.RemoveRange(dbCrimeHistory);
            await _context.SaveChangesAsync();
        }

        public async Task<int> DeleteInmatePrebookCharges(int inmatePrebookChargeId)
        {
            if (inmatePrebookChargeId <= 0)
            {
                return 0;
            }
            InmatePrebookCharge inmatePrebookCharges =
                _context.InmatePrebookCharge.Single(c => c.InmatePrebookChargeId == inmatePrebookChargeId);
            inmatePrebookCharges.DeleteFlag = 1;
            inmatePrebookCharges.DeleteDate = DateTime.Now;
            inmatePrebookCharges.DeletedBy = _personnelId;

            CrimeHistory crimeHistory = new CrimeHistory
            {
                InmatePrebookChargeId = inmatePrebookCharges.InmatePrebookChargeId,
                CrimeLookupId = inmatePrebookCharges.CrimeLookupId,
                CrimeType = inmatePrebookCharges.CrimeType,
                CrimeNotes = inmatePrebookCharges.CrimeNotes,
                CrimeCount = inmatePrebookCharges.CrimeCount,
                CrimeDeleteFlag = 1,
                BailAmount = inmatePrebookCharges.BailAmount,
                BailNoBailFlag = inmatePrebookCharges.BailNoBailFlag,
                CreatDate = DateTime.Now,
                CreatedBy = _personnelId,
                ChargeQualifierLookup = inmatePrebookCharges.ChargeQualifierLookup,
                CrimeStatusLookup = inmatePrebookCharges.CrimeStatusLookup
            };
            _context.CrimeHistory.Add(crimeHistory);

            return await _context.SaveChangesAsync();
        }

        public async Task<int> UndoDeleteInmatePrebookCharges(int inmatePrebookChargeId)
        {
            if (inmatePrebookChargeId <= 0)
            {
                return 0;
            }
            InmatePrebookCharge inmatePrebookCharges =
                _context.InmatePrebookCharge.Single(c => c.InmatePrebookChargeId == inmatePrebookChargeId);
            inmatePrebookCharges.DeleteFlag = null;
            inmatePrebookCharges.DeleteDate = DateTime.Now;
            inmatePrebookCharges.DeletedBy = _personnelId;
            inmatePrebookCharges.UpdateBy = _personnelId;
            inmatePrebookCharges.UpdateDate = DateTime.Now;

            CrimeHistory crimeHistory = new CrimeHistory
            {
                InmatePrebookChargeId = inmatePrebookCharges.InmatePrebookChargeId,
                CrimeLookupId = inmatePrebookCharges.CrimeLookupId,
                CrimeType = inmatePrebookCharges.CrimeType,
                CrimeNotes = inmatePrebookCharges.CrimeNotes,
                CrimeCount = inmatePrebookCharges.CrimeCount,
                CrimeDeleteFlag = 0,
                BailAmount = inmatePrebookCharges.BailAmount,
                BailNoBailFlag = inmatePrebookCharges.BailNoBailFlag,
                CreatDate = DateTime.Now,
                CreatedBy = _personnelId,
                ChargeQualifierLookup = inmatePrebookCharges.ChargeQualifierLookup,
                CrimeStatusLookup = inmatePrebookCharges.CrimeStatusLookup,
                OffenceDate = inmatePrebookCharges.OffenceDate
            };
            _context.CrimeHistory.Add(crimeHistory);

            return await _context.SaveChangesAsync();
        }

        public async Task<int> UpdateInmatePrebookCharge(PrebookCharge value)
        {
            InmatePrebookCharge inmatePrebookCharge = _context.InmatePrebookCharge.SingleOrDefault(
                    i => i.InmatePrebookChargeId == value.InmatePrebookChargeId);
            if (inmatePrebookCharge == null)
            {
                return 0;
            }
            inmatePrebookCharge.InmatePrebookId = value.InmatePrebookId ?? 0;
            inmatePrebookCharge.InmatePrebookWarrantId = value.InmatePrebookWarrantId;
            inmatePrebookCharge.CrimeLookupId = value.CrimeLookupId;
            inmatePrebookCharge.CrimeCount = value.CrimeCount;
            inmatePrebookCharge.BailAmount = value.BailAmount;
            inmatePrebookCharge.BailNoBailFlag = value.BailNoBailFlag ? 1 : 0;
            inmatePrebookCharge.CrimeType = value.CrimeType;
            inmatePrebookCharge.CrimeNotes = value.CrimeNotes;
            inmatePrebookCharge.ChargeQualifierLookup = value.ChargeQualifierId.HasValue
                ? value.ChargeQualifierId.ToString() : string.Empty;
            inmatePrebookCharge.CrimeStatusLookup = value.CrimeStatusLookup;
            inmatePrebookCharge.UpdateBy = _personnelId;
            inmatePrebookCharge.UpdateDate = DateTime.Now;
            inmatePrebookCharge.OffenceDate = value.OffenceDate;
            CrimeHistory crimeHistory = new CrimeHistory
            {
                InmatePrebookChargeId = value.InmatePrebookChargeId,
                CrimeId = value.CrimeId,
                CrimeForceId = value.CrimeForceId,
                CrimeLookupId = value.CrimeLookupId,
                CrimeType = value.CrimeType,
                CrimeNotes = value.CrimeNotes,
                CrimeCount = value.CrimeCount,
                CrimeDeleteFlag = value.DeleteFlag ? 1 : 0,
                BailAmount = value.BailAmount,
                BailNoBailFlag = value.BailNoBailFlag ? 1 : 0,
                CreatDate = DateTime.Now,
                CreatedBy = _personnelId,
                ChargeQualifierLookup =
                    value.ChargeQualifierId.HasValue ? value.ChargeQualifierId.ToString() : string.Empty,
                CrimeStatusLookup = value.CrimeStatusLookup,
                OffenceDate = value.OffenceDate
            };
            _context.Add(crimeHistory);
            return await _context.SaveChangesAsync();
        }

        public async Task<int> UpdateForceCharge(PrebookCharge value)
        {
            CrimeForce crimeForce = _context.CrimeForce.SingleOrDefault(i => i.CrimeForceId == value.CrimeForceId);
            if (crimeForce == null)
            {
                return 0;
            }
            crimeForce.InmatePrebookId = value.InmatePrebookId;
            crimeForce.InmatePrebookWarrantId = value.InmatePrebookWarrantId;
            crimeForce.WarrantId = value.WarrantId;
            crimeForce.TempCrimeCount = value.CrimeCount;
            crimeForce.TempCrimeCodeType = value.CrimeCodeType;
            crimeForce.TempCrimeSection = value.CrimeSection;
            crimeForce.TempCrimeDescription = value.CrimeDescription;
            crimeForce.TempCrimeStatuteCode = value.CrimeStatuteCode;
            crimeForce.BailAmount = value.BailAmount;
            crimeForce.BailNoBailFlag = value.BailNoBailFlag ? 1 : 0;
            crimeForce.TempCrimeType = value.CrimeType;
            crimeForce.TempCrimeNotes = value.CrimeNotes;
            crimeForce.TempCrimeGroup = value.CrimeGroupId.ToString();
            crimeForce.ChargeQualifierLookup = value.ChargeQualifierId.HasValue
                ? value.ChargeQualifierId.ToString() : string.Empty;
            crimeForce.TempCrimeQualifierLookup = value.ChargeQualifierId;
            crimeForce.TempCrimeStatusLookup = value.CrimeStatusLookup;
            crimeForce.UpdateBy = _personnelId;
            crimeForce.UpdateDate = DateTime.Now;
            crimeForce.OffenceDate = value.OffenceDate;
            CrimeHistory crimeHistory = new CrimeHistory
            {
                InmatePrebookChargeId = value.InmatePrebookChargeId,
                CrimeId = value.CrimeId,
                CrimeForceId = value.CrimeForceId,
                CrimeLookupId = value.CrimeLookupId,
                CrimeType = value.CrimeType,
                CrimeNotes = value.CrimeNotes,
                CrimeCount = value.CrimeCount,
                CrimeDeleteFlag = value.DeleteFlag ? 1 : 0,
                BailAmount = value.BailAmount,
                BailNoBailFlag = value.BailNoBailFlag ? 1 : 0,
                CreatDate = DateTime.Now,
                CreatedBy = _personnelId,
                ChargeQualifierLookup =
                    value.ChargeQualifierId.HasValue ? value.ChargeQualifierId.ToString() : string.Empty,
                CrimeQualifierLookup = value.ChargeQualifierId,
                CrimeStatusLookup = value.CrimeStatusLookup,
                CrimeLookupForceString = value.CrimeLookupForceString,
                OffenceDate = value.OffenceDate
            };

            _context.CrimeHistory.Add(crimeHistory);
            //Update OverAll ChargeLevel
            if (value.ArrestId > 0)
            {
                int? incarcerationId = _context.IncarcerationArrestXref
                    .Where(i => i.Arrest.ArrestId == value.ArrestId)
                    .OrderByDescending(i => i.Incarceration.IncarcerationId)
                    .Select(i => i.Incarceration.IncarcerationId).FirstOrDefault();
                if (incarcerationId > 0)
                    _bookingReleaseService.UpdateOverAllChargeLevel((int)incarcerationId);
            }
            return await _context.SaveChangesAsync();
        }


        public IEnumerable<CrimeLookupVm> GetCrimeSearch(CrimeLookupVm crimeDetails) => _context.CrimeLookup.Where(
                x => (string.IsNullOrEmpty(crimeDetails.CrimeCodeType) ||
                        x.CrimeCodeType == crimeDetails.CrimeCodeType)
                    && (string.IsNullOrEmpty(crimeDetails.CrimeSection) || !string.IsNullOrEmpty(x.CrimeSection) &&
                        x.CrimeSection.Contains(crimeDetails.CrimeSection))
                    && (!crimeDetails.Descriptions.Any() || !string.IsNullOrEmpty(x.CrimeDescription)
                        && crimeDetails.Descriptions.Any(w => x.CrimeDescription.ToUpper().Contains(w.ToUpper())))
                    && (string.IsNullOrEmpty(crimeDetails.CrimeStatuteCode) ||
                        x.CrimeStatuteCode == crimeDetails.CrimeStatuteCode)
                    && (!x.CrimeDoNotUse.HasValue || x.CrimeDoNotUse == 0))
            .Select(y => new CrimeLookupVm
            {
                CrimeLookupId = y.CrimeLookupId,
                CrimeCodeType = y.CrimeCodeType,
                CrimeSection = y.CrimeSection,
                CrimeGroupId = y.CrimeGroupId,
                CrimeDescription = y.CrimeDescription,
                CrimeStatuteCode = y.CrimeStatuteCode,
                BailAmount = y.BailAmountDefault,
                BailNoBailFlagDefault = y.BailNoBailFlagDefault == 1
            }).ToList();

        public async Task<int> InsertCrimeHistory(CrimeHistoryVm crime)
        {
            CrimeHistory crimeHistory = new CrimeHistory
            {
                CrimeHistoryId = crime.CrimeHistoryId,
                InmatePrebookChargeId = crime.InmatePrebookChargeId,
                CrimeId = crime.CrimeId,
                CrimeForceId = crime.CrimeForceId,
                CrimeLookupId = crime.CrimeLookupId,
                CrimeLookupForceString = crime.CrimeLookupForceString,
                CrimeType = crime.CrimeType,
                CrimeNotes = crime.CrimeNotes,
                CrimeCount = crime.CrimeCount,
                CrimeDeleteFlag = crime.CrimeDeleteFlag ? 1 : 0,
                BailAmount = crime.BailAmount,
                BailNoBailFlag = crime.BailNoBailFlag ? 1 : 0,
                CreatDate = DateTime.Now,
                CreatedBy = _personnelId,
                ChargeQualifierLookup = crime.ChargeQualifierLookup
            };

            _context.Add(crimeHistory);
            return await _context.SaveChangesAsync();
        }

        public async Task<int> DeleteCrimeForces(int crimeForceId)
        {
            if (crimeForceId <= 0)
            {
                return 0;
            }
            CrimeForce crimeForces = _context.CrimeForce.Single(c => c.CrimeForceId == crimeForceId);
            crimeForces.DeleteFlag = 1;
            crimeForces.DeleteDate = DateTime.Now;
            crimeForces.DeleteBy = _personnelId;

            CrimeHistory crimeHistory = _context.CrimeHistory.LastOrDefault(ch => ch.CrimeForceId == crimeForceId);
            if (crimeHistory != null)
            {
                CrimeHistory newCrimeHistory = new CrimeHistory
                {
                    CrimeForceId = crimeHistory.CrimeForceId,
                    CrimeLookupId = crimeHistory.CrimeLookupId,
                    CrimeType = crimeHistory.CrimeType,
                    CrimeNotes = crimeHistory.CrimeNotes,
                    CrimeCount = crimeHistory.CrimeCount,
                    CrimeDeleteFlag = 1,
                    BailAmount = crimeHistory.BailAmount,
                    BailNoBailFlag = crimeHistory.BailNoBailFlag,
                    CreatDate = DateTime.Now,
                    CreatedBy = _personnelId,
                    ChargeQualifierLookup = crimeHistory.ChargeQualifierLookup,
                    CrimeStatusLookup = crimeHistory.CrimeStatusLookup
                };
                _context.CrimeHistory.Add(newCrimeHistory);
            }

            return await _context.SaveChangesAsync();
        }

        public async Task<int> UndoDeleteCrimeForces(int crimeForceId)
        {
            if (crimeForceId <= 0)
            {
                return 0;
            }
            CrimeForce crimeForces = _context.CrimeForce.Single(c => c.CrimeForceId == crimeForceId);
            crimeForces.DeleteFlag = 0;
            crimeForces.DeleteDate = null;
            crimeForces.DeleteBy = _personnelId;

            CrimeHistory crimeHistory = _context.CrimeHistory.LastOrDefault(ch => ch.CrimeForceId == crimeForceId);
            if (crimeHistory != null)
            {
                CrimeHistory newCrimeHistory = new CrimeHistory
                {
                    CrimeForceId = crimeHistory.CrimeForceId,
                    CrimeLookupId = crimeHistory.CrimeLookupId,
                    CrimeType = crimeHistory.CrimeType,
                    CrimeNotes = crimeHistory.CrimeNotes,
                    CrimeCount = crimeHistory.CrimeCount,
                    CrimeDeleteFlag = 0,
                    BailAmount = crimeHistory.BailAmount,
                    BailNoBailFlag = crimeHistory.BailNoBailFlag,
                    CreatDate = DateTime.Now,
                    CreatedBy = _personnelId,
                    ChargeQualifierLookup = crimeHistory.ChargeQualifierLookup,
                    CrimeStatusLookup = crimeHistory.CrimeStatusLookup
                };
                _context.CrimeHistory.Add(newCrimeHistory);
            }

            return await _context.SaveChangesAsync();
        }

        public IEnumerable<CrimeHistoryVm> GetCrimeHistory(int? inmatePrebookChargeId)
        {
            List<CrimeHistoryVm> crime = _context.CrimeHistory.Where(x =>
                    inmatePrebookChargeId == 0 || x.InmatePrebookChargeId == inmatePrebookChargeId)
                .Select(y => new CrimeHistoryVm
                {
                    CrimeHistoryId = y.CrimeHistoryId,
                    InmatePrebookChargeId = y.InmatePrebookChargeId,
                    CrimeId = y.CrimeId,
                    CrimeForceId = y.CrimeForceId,
                    CrimeLookupId = y.CrimeLookupId,
                    CrimeLookupForceString = y.CrimeLookupForceString,
                    CrimeType = y.CrimeType,
                    CrimeNotes = y.CrimeNotes,
                    CrimeCount = y.CrimeCount,
                    CrimeDeleteFlag = Convert.ToBoolean(y.CrimeDeleteFlag),
                    BailAmount = y.BailAmount,
                    BailNoBailFlag = Convert.ToBoolean(y.BailNoBailFlag),
                    CreatDate = y.CreatDate,
                    CreatedBy = y.CreatedBy,
                    ChargeQualifierLookup = y.ChargeQualifierLookup,
                    CrimeStatusLookup = y.CrimeStatusLookup,
                    CrimeQualifierLookup = y.CrimeQualifierLookup
                }).ToList();
            return crime;
        }

        #endregion

        #region Prebook Warrants

        public IEnumerable<InmatePrebookWarrantVm> GetPrebookWarrant(int inmatePrebookId, bool deleteFlag,
            int incarcerationId = 0, int arrestId = 0)
        {
            List<InmatePrebookWarrant> dbInmatePrebookWarrant;
            if (arrestId > 0)
            {
                dbInmatePrebookWarrant = _context.InmatePrebookWarrant.Where(i => i.InmatePrebookId == inmatePrebookId).ToList();
                InmatePrebook inmatePrebook = _context.InmatePrebook.SingleOrDefault(x => x.ArrestId == arrestId);
                if (inmatePrebook != null)
                {
                    inmatePrebookId = inmatePrebook.InmatePrebookId;
                    if (inmatePrebookId > 0)
                    {
                        dbInmatePrebookWarrant = _context.InmatePrebookWarrant
                        .Where(pw => pw.InmatePrebookId == inmatePrebookId
                            && (!pw.WarrantId.HasValue || pw.WarrantId == 0)).ToList();
                    }
                    else
                    {
                        dbInmatePrebookWarrant = _context.InmatePrebookWarrant
                        .Where(pw => pw.InmatePrebook.IncarcerationId == incarcerationId
                            && (!pw.WarrantId.HasValue || pw.WarrantId == 0)).ToList();
                    }
                }
            }
            else
            {
                dbInmatePrebookWarrant = _context.InmatePrebookWarrant
                .Where(pw => (incarcerationId == 0 || pw.InmatePrebook.IncarcerationId == incarcerationId
                        && (!pw.WarrantId.HasValue || pw.WarrantId == 0))
                       && (incarcerationId > 0 || pw.InmatePrebookId == inmatePrebookId)).ToList();
            }

            List<InmatePrebookWarrantVm> inmatePrebookWarrant =
                dbInmatePrebookWarrant.Where(x => deleteFlag || !x.DeleteFlag.HasValue)
                    .Select(y => new InmatePrebookWarrantVm
                    {
                        InmatePrebookWarrantId = y.InmatePrebookWarrantId,
                        WarrantId = y.WarrantId,
                        DeleteFlag = y.DeleteFlag == 1,
                        WarrantNumber = y.WarrantNumber,
                        WarrantType = y.WarrantType,
                        WarrantIssueDate = y.WarrantIssueDate,
                        WarrantAgencyText = y.WarrantAgencyText,
                        WarrantDescription = y.WarrantDescription,
                        WarrantBailAmount = y.WarrantBailAmount,
                        WarrantChargeType = y.WarrantChargeType,
                        WarrantAgencyId = y.WarrantAgencyId ?? 0,
                        WarrantBailType = y.WarrantBailType,
                        WarrantNoBail = !string.IsNullOrEmpty(y.WarrantBailType),
                        InmatePrebookCaseId = y.InmatePrebookCaseId ?? 0,
                        OriginatingAgency = y.OriginatingAgency,
                        LocalWarrant = y.LocalWarrant
                    }).ToList();

            int?[] warrentAgencyIds = inmatePrebookWarrant.Select(s => s.WarrantAgencyId).ToArray();
            List<Agency> inmatePrebookWarrantList = _context.Agency.Where(w => warrentAgencyIds.Contains(w.AgencyId))
            .Select(s => new Agency
            {
                AgencyId = s.AgencyId,
                AgencyName = s.AgencyName
            }).ToList();


            inmatePrebookWarrant.ForEach(ipw =>
            {
                if (ipw.WarrantAgencyId > 0)
                {
                    ipw.AgencyName = inmatePrebookWarrantList.Single(s => s.AgencyId == ipw.WarrantAgencyId).AgencyName;
                }
                if (ipw.InmatePrebookWarrantId > 0)
                {
                    ipw.WarrantPrebookCharges =
                        GetPrebookCharges(inmatePrebookId, deleteFlag, ipw.InmatePrebookWarrantId, incarcerationId, arrestId);
                }
            });
            return inmatePrebookWarrant;
        }

        public IEnumerable<VehicleMakeVm> GetVehicleMake()
        {
            List<VehicleMakeVm> vehicleMake =
                _context.VehicleMake
                    .Select(y => new VehicleMakeVm
                    {
                        VehicleMakeId = y.VehicleMakeId,
                        VehicleMakeName = y.VehicleMakeName,
                        MakeCode = y.MakeCode,
                        DeleteFlag = y.DeleteFlag
                    }).OrderBy(o => o.VehicleMakeName).ToList();

            return vehicleMake;
        }

        public IEnumerable<VehicleModelVm> GetVehicleModel(int vehicleMakeId) =>
            _context.VehicleModel.Where(x => x.VehicleMakeId == vehicleMakeId)
                .Select(y => new VehicleModelVm
                {
                    VehicleModelId = y.VehicleModelId,
                    VehicleModelName = y.VehicleModelName,
                    ModelCode = y.ModelCode,
                    DeleteFlag = y.DeleteFlag
                }).OrderBy(o => o.VehicleModelName).ToList();

        public InmatePrebookWarrantVm GetPrebookWarrantById(int inmatePrebookWarrantId) =>
            _context.InmatePrebookWarrant.Where(x => x.InmatePrebookWarrantId == inmatePrebookWarrantId)
                .Select(x => new InmatePrebookWarrantVm
                {
                    WarrantId = x.WarrantId,
                    WarrantNumber = x.WarrantNumber,
                    WarrantType = x.WarrantType,
                    WarrantIssueDate = x.WarrantIssueDate,
                    WarrantAgencyText = x.WarrantAgencyText,
                    WarrantAgencyId = x.WarrantAgencyId,
                    WarrantDescription = x.WarrantDescription,
                    WarrantBailAmount = x.WarrantBailAmount,
                    WarrantChargeType = x.WarrantChargeType,
                    InmatePrebookCaseId = x.InmatePrebookCaseId ?? 0,
                    LocalWarrant = x.LocalWarrant
                }).SingleOrDefault();

        public async Task<int> InsertPrebookWarrant(InmatePrebookWarrantVm warrant)
        {
            InmatePrebookWarrant preebookWarrant = new InmatePrebookWarrant
            {
                InmatePrebookWarrantId = warrant.InmatePrebookWarrantId,
                InmatePrebookId = warrant.InmatePrebookId,
                WarrantNumber = warrant.WarrantNumber,
                WarrantType = warrant.WarrantType,
                WarrantDescription = warrant.WarrantDescription,
                WarrantBailAmount = !warrant.WarrantNoBail ? warrant.WarrantBailAmount : 0,
                WarrantAgencyId = warrant.WarrantAgencyId,
                WarrantAgencyText =
                    string.IsNullOrWhiteSpace(warrant.WarrantAgencyText)
                        ? _context.Agency.SingleOrDefault(a => a.AgencyId == warrant.WarrantAgencyId)?.AgencyName
                        : warrant.WarrantAgencyText,
                WarrantIssueDate = warrant.WarrantIssueDate,
                WarrantChargeType = warrant.WarrantChargeType,
                CreateDate = DateTime.Now,
                CreateBy = _personnelId,
                UpdateDate = DateTime.Now,
                UpdateBy = _personnelId,
                WarrantId = warrant.WarrantId,
                WarrantBailType = warrant.WarrantNoBail ? BailType.NOBAIL : string.Empty,
                InmatePrebookCaseId = warrant.InmatePrebookCaseId,
                OriginatingAgency = warrant.OriginatingAgency,
                LocalWarrant = warrant.LocalWarrant
            };
            _context.Add(preebookWarrant);
            return await _context.SaveChangesAsync();
        }

        public async Task<int> UpdatePrebookWarrant(InmatePrebookWarrantVm warrant)
        {
            InmatePrebookWarrant prebookWarrant =
                _context.InmatePrebookWarrant.SingleOrDefault(
                    ipw => ipw.InmatePrebookWarrantId == warrant.InmatePrebookWarrantId);
            if (prebookWarrant == null)
            {
                return 0;
            }
            {
                prebookWarrant.InmatePrebookId = warrant.InmatePrebookId;
                prebookWarrant.WarrantNumber = warrant.WarrantNumber;
                prebookWarrant.WarrantType = warrant.WarrantType;
                prebookWarrant.WarrantDescription = warrant.WarrantDescription;
                prebookWarrant.WarrantBailAmount = !warrant.WarrantNoBail ? warrant.WarrantBailAmount : 0;
                prebookWarrant.WarrantAgencyId = warrant.WarrantAgencyId;
                prebookWarrant.WarrantAgencyText = string.IsNullOrWhiteSpace(warrant.WarrantAgencyText)
                    ? _context.Agency.SingleOrDefault(a => a.AgencyId == warrant.WarrantAgencyId)?.AgencyName
                    : warrant.WarrantAgencyText;
                prebookWarrant.WarrantIssueDate = warrant.WarrantIssueDate;
                prebookWarrant.WarrantChargeType = warrant.WarrantChargeType;
                prebookWarrant.UpdateBy = _personnelId;
                prebookWarrant.UpdateDate = DateTime.Now;
                prebookWarrant.WarrantId = warrant.WarrantId;
                prebookWarrant.WarrantBailType = warrant.WarrantNoBail ? BailType.NOBAIL : string.Empty;
                prebookWarrant.OriginatingAgency = warrant.OriginatingAgency;
            }
            return await _context.SaveChangesAsync();
        }

        public async Task<int> DeletePrebookWarrant(int inmatePrebookWarrantId)
        {
            if (inmatePrebookWarrantId <= 0)
            {
                return 0;
            }
            InmatePrebookWarrant x = _context.InmatePrebookWarrant.Single(
                    ipw => ipw.InmatePrebookWarrantId == inmatePrebookWarrantId);
            x.DeleteFlag = 1;
            x.DeleteDate = DateTime.Now;
            x.DeletedBy = _personnelId;
            return await _context.SaveChangesAsync();
        }

        public async Task<int> UndoDeletePrebookWarrant(int inmatePrebookWarrantId)
        {
            if (inmatePrebookWarrantId <= 0)
            {
                return 0;
            }
            InmatePrebookWarrant inmatePrebookWarrants =
                _context.InmatePrebookWarrant.Single(
                    ipw => ipw.InmatePrebookWarrantId == inmatePrebookWarrantId);
            inmatePrebookWarrants.DeleteFlag = null;
            inmatePrebookWarrants.DeleteDate = DateTime.Now;
            inmatePrebookWarrants.DeletedBy = _personnelId;
            return await _context.SaveChangesAsync();
        }

        #endregion

        public IEnumerable<PersonalInventoryVm> GetPersonalInventoryPrebook(int preBookId, bool deleteFlag)
        {
            List<PersonalInventoryVm> property =
                _context.PersonalInventoryPreBook.Where(
                        x => x.InmatePrebookId == preBookId && (deleteFlag || !x.DeleteFlag.HasValue))
                    .OrderBy(x => x.DeleteFlag).ThenBy(x => x.InventoryArticles)
                    .Select(y => new PersonalInventoryVm
                    {
                        PersonalInventoryPreBookId = y.PersonalInventoryPreBookId,
                        InmatePrebookId = y.InmatePrebookId,
                        InventoryArticles = y.InventoryArticles,
                        InventoryQuantity = y.InventoryQuantity ?? 0,
                        InventoryDescription = y.InventoryDescription,
                        InventoryColor = y.InventoryColor,
                        DeleteFlag = y.DeleteFlag == 1,
                        DeleteBy = y.DeleteBy,
                        ImportFlag = y.ImportFlag,
                        CreateDate = (DateTime)y.CreateDate
                    }).ToList();

            //get article description by lookup index
            int[] listLookupIds = property.Select(s => s.InventoryArticles).ToArray();
            List<LookupVm> listLookupDetails = _context.Lookup
                .Where(w => listLookupIds.Contains(w.LookupIndex) && w.LookupType == LookupConstants.INVARTCL)
                .Select(s => new LookupVm
                {
                    LookupIndex = s.LookupIndex,
                    LookupDescription = s.LookupDescription
                }).ToList();

            property.ForEach(item => item.InventoryArticlesName = listLookupDetails
                    .FirstOrDefault(w => w.LookupIndex == item.InventoryArticles)?.LookupDescription);
            return property;
        }

        public async Task<int> InsertPersonalInventoryPrebook(PersonalInventoryVm[] properties)
        {
            PersonalInventoryPreBook property = new PersonalInventoryPreBook();
            foreach (PersonalInventoryVm i in properties)
            {
                property = new PersonalInventoryPreBook
                {
                    InmatePrebookId = i.InmatePrebookId,
                    InventoryArticles = i.InventoryArticles,
                    InventoryQuantity = i.InventoryQuantity,
                    InventoryDescription = i.InventoryDescription,
                    InventoryColor = i.InventoryColor,
                    CreateDate = DateTime.Now,
                    CreateBy = _personnelId
                };
                _context.Add(property);
            }
            await _context.SaveChangesAsync();
            return property.PersonalInventoryPreBookId;
        }

        public async Task<int> DeletePersonalInventoryPrebook(int personalInventoryPreBookId)
        {
            if (personalInventoryPreBookId <= 0)
            {
                return 0;
            }
            PersonalInventoryPreBook property = _context.PersonalInventoryPreBook.Single(
                    c => c.PersonalInventoryPreBookId == personalInventoryPreBookId);
            property.DeleteFlag = 1;
            property.DeleteDate = DateTime.Now;
            property.DeleteBy = _personnelId;
            return await _context.SaveChangesAsync();
        }

        public async Task<int> UndoDeletePersonalInventoryPrebook(int personalInventoryPreBookId)
        {
            if (personalInventoryPreBookId <= 0)
            {
                return 0;
            }
            PersonalInventoryPreBook property = _context.PersonalInventoryPreBook.Single(
                    c => c.PersonalInventoryPreBookId == personalInventoryPreBookId);
            property.DeleteFlag = null;
            property.DeleteDate = DateTime.Now;
            property.DeleteBy = _personnelId;
            return await _context.SaveChangesAsync();
        }

        #region Prebook Forms

        public IEnumerable<GetFormTemplates> GetPreBookForms(int inmatePrebookId, FormScreen formScreenFlag) =>
            _context.FormTemplates.Where(x =>
                    (!x.Inactive.HasValue || x.Inactive == 0) && (FormScreen.PrebookIntake != formScreenFlag
                        || x.FormCategoryId == (int?)FormCategoryName.PrebookForms) &&
                    (FormScreen.Courtcommit != formScreenFlag ||
                        x.FormCategoryId == (int?)FormCategoryName.CourtCommitForms))
                .Select(ft => new GetFormTemplates
                {
                    DisplayName = ft.DisplayName,
                    HtmlPath = ft.FormCategory.FormCategoryFolderName,
                    HtmlFileName = ft.HtmlFileName,
                    FormTemplatesId = ft.FormTemplatesId,
                    FormInterfaceFlag = ft.FormInterfaceFlag ?? 0,
                    CategoryId = ft.FormCategoryId,
                    RequireUponComplete = ft.RequireUponComplete,
                    Cnt = ft.FormRecord.Count(fr => fr.InmatePrebookId == inmatePrebookId && fr.DeleteFlag == 0)
                }).ToList();

        public IEnumerable<LoadSavedForms> LoadSavedForms(int inmatePrebookId, int formTemplateId,
            FormScreen formScreenFlag, bool deleteFlag) =>
            _context.FormRecord.Where(x => x.InmatePrebookId == inmatePrebookId &&
                    (formScreenFlag == FormScreen.PrebookIntake
                        ? x.FormTemplates.FormCategoryId == (int)FormCategoryName.PrebookForms
                        : x.FormTemplates.FormCategoryId == (int)FormCategoryName.CourtCommitForms) &&
                    (!(formTemplateId > 0) ||
                        x.FormTemplatesId == formTemplateId) &&
                    (deleteFlag ? x.DeleteFlag == 1 || x.DeleteFlag == 0 : x.DeleteFlag == 0))
                .OrderByDescending(y => y.CreateDate)
                .Select(z => new LoadSavedForms
                {
                    DisplayName = z.FormTemplates.DisplayName,
                    Notes = z.FormNotes,
                    FormRecordId = z.FormRecordId,
                    FormTemplatesId = z.FormTemplatesId,
                    InmatePrebookId = inmatePrebookId,
                    //HtmlPath = z.FormTemplates.FormCategory.FormCategoryFolderName,
                    //HtmlFileName = z.FormTemplates.HtmlFileName,
                    //XmlStr = z.XmlData,
                    FormInterfaceSent = z.FormInterfaceSent.Value,
                    InterfaceBypassed = z.FormInterfaceBypassed.Value,
                    InterfaceFlag = z.FormTemplates.FormInterfaceFlag ?? 0,
                    Date = z.CreateDate,
                    DeleteFlag = z.DeleteFlag,
                    CreatedPersonnel = z.CreateBy,
                    CategoryId = z.FormTemplates.FormCategoryId,
                    XmlStr = z.XmlData
                }).ToList();

        public async Task<int> DeletePreBookForm(int formRecordId)
        {
            FormRecord deleteForm = _context.FormRecord.SingleOrDefault(dpf => dpf.FormRecordId == formRecordId);
            if (deleteForm == null) return -1;
            deleteForm.DeleteFlag = deleteForm.DeleteFlag == 1 ? 0 : 1;
            deleteForm.DeleteDate = DateTime.Now;
            deleteForm.DeleteBy = _personnelId;

            return await _context.SaveChangesAsync();
        }

        public AddForm AddForm(int inmatePrebookId, int formTemplatesId)
        {
            AddForm forms = _context.FormTemplates
                .Where(x => x.FormTemplatesId == formTemplatesId)
                .Select(y => new AddForm
                {
                    DisplayName = y.DisplayName,
                    //temporarily commented
                    // File = Convert.ToBase64String(File.ReadAllBytes(path + y.FormCategory.FormCategoryFolderName + "\\" + y.HtmlFileName)),
                })
                .SingleOrDefault();

            AddForm per = _context.InmatePrebook.Where(x => x.InmatePrebookId == inmatePrebookId)
                .Select(y => new AddForm
                {
                    PersonFirstName = y.PersonFirstName,
                    PersonLastName = y.PersonLastName,
                    PersonDob = (DateTime)y.PersonDob,
                }).SingleOrDefault();

            if (forms == null || per == null) return forms;
            forms.PersonDob = per.PersonDob;
            forms.PersonFirstName = per.PersonFirstName;
            forms.PersonLastName = per.PersonLastName;
            return forms;
        }

        public ListForm ListForm(int formRecordId) =>
            _context.FormRecord.Where(x => x.FormRecordId == formRecordId)
                .Select(y => new ListForm
                {
                    DisplayName = y.FormTemplates.DisplayName,
                    Notes = y.FormNotes,
                    FormData = HttpUtility.HtmlDecode(y.XmlData),
                    //Commented temporarily
                    //  File = Convert.ToBase64String(File.ReadAllBytes(path + record.FormTemplates.FormCategory.FormCategoryFolderName + "\\" + record.FormTemplates.HtmlFileName)),
                    PersonFirstName = y.InmatePrebook.Person.PersonFirstName,
                    PersonLastName = y.InmatePrebook.Person.PersonLastName,
                    PersonDob = y.InmatePrebook.Person.PersonDob
                }).SingleOrDefault();

        public async Task<int> UpdateForm(LoadSavedForms formdata)
        {
            //inserting form record
            if (formdata.FormRecordId == 0)
            {
                FormRecord create = new FormRecord
                {
                    InmatePrebookId = formdata.InmatePrebookId,
                    FormTemplatesId = formdata.FormTemplatesId,
                    CreateBy = formdata.CreatedPersonnel,
                    FormNotes = formdata.Notes,
                    XmlData = formdata.XmlStr
                };
                _context.Add(create);
            }
            else //Editing form record
            {
                FormRecord forms = _context.FormRecord.SingleOrDefault(f => f.FormRecordId == formdata.FormRecordId);
                if (forms == null) return -1;
                forms.XmlData = HttpUtility.HtmlDecode(formdata.XmlStr);
                forms.FormNotes = formdata.Notes;
                forms.UpdateBy = formdata.UpdateBy;
                forms.UpdateDate = DateTime.Now;
                forms.FormRecordId = formdata.FormRecordId;
            }
            return await _context.SaveChangesAsync();
        }

        #endregion

        #region Prebook Attachment

        public IEnumerable<PrebookAttachment> GetPrebookAttachment(AttachmentSearch attachmentSearch)
        {
            List<PrebookAttachment> prebookAttachments = _context.AppletsSaved.Where(a =>
                        // filter prebook attachments.
                        (attachmentSearch.InmateprebookId == 0
                         || a.InmatePrebookId == attachmentSearch.InmateprebookId)
                        // filter alt sentence request attachments.
                        && (attachmentSearch.AltSentRequestId == 0
                            || a.AltSentRequestId == attachmentSearch.AltSentRequestId)
                        // filter incarceration attachments.
                        && (attachmentSearch.IncarcerationId == 0 ||
                            a.IncarcerationId == attachmentSearch.IncarcerationId)
                        // filter booking attachments.
                        && (attachmentSearch.ArrestId == 0 ||
                            a.ArrestId == attachmentSearch.ArrestId)
                        // filter grievance attachments.
                        && (attachmentSearch.GrievanceID == 0 || a.GrievanceId == attachmentSearch.GrievanceID)
                        // filter deleted attachments.
                        && (a.AppletsDeleteFlag == attachmentSearch.DeleteFlag ||
                            a.AppletsDeleteFlag == 0)
                        // filter based on booking attach filters.
                        && (attachmentSearch.Flag != AttachmentType.BOOKATTACHTYPE ||
                            a.ArrestId > 0 && (attachmentSearch.ActiveFlag == 0 || a.Arrest.Inmate.InmateActive == 1)
                            && (attachmentSearch.FacilityId == 0 ||
                                a.Arrest.Inmate.Facility.FacilityId == attachmentSearch.FacilityId)
                            && (attachmentSearch.InmateId == 0 ||
                                a.Arrest.Inmate.InmateId == attachmentSearch.InmateId))
                        // filter based on incarceration attach filters.
                        && (attachmentSearch.Flag != AttachmentType.INCARATTACHTYPE ||
                            a.IncarcerationId > 0 &&
                            (attachmentSearch.ActiveFlag == 0 || a.Incarceration.Inmate.InmateActive == 1)
                            && (attachmentSearch.FacilityId == 0 ||
                                a.Incarceration.Inmate.Facility.FacilityId == attachmentSearch.FacilityId)
                            && (attachmentSearch.InmateId == 0 ||
                                a.Incarceration.Inmate.InmateId == attachmentSearch.InmateId))
                        // filter based on classification attach filters.
                        && (attachmentSearch.Flag != AttachmentType.CLASATTACHTYPE ||
                            a.InmateId > 0 && (attachmentSearch.ActiveFlag == 0 || a.Inmate.InmateActive == 1)
                            && (attachmentSearch.FacilityId == 0 ||
                                a.Inmate.Facility.FacilityId == attachmentSearch.FacilityId)
                            && (attachmentSearch.InmateId == 0 || a.InmateId == attachmentSearch.InmateId))
                        // filter based on medical attach filters.
                        && (attachmentSearch.Flag != AttachmentType.MEDATTACHTYPE ||
                            a.MedicalInmateId > 0 &&
                            (attachmentSearch.ActiveFlag == 0 || a.MedicalInmate.InmateActive == 1)
                            && (attachmentSearch.FacilityId == 0 ||
                                a.MedicalInmate.Facility.FacilityId == attachmentSearch.FacilityId)
                            && (attachmentSearch.InmateId == 0 || a.MedicalInmateId == attachmentSearch.InmateId))
                        // filter based on housing attach filters.
                        && (attachmentSearch.Flag != AttachmentType.HOUSATTACHTYPE ||
                            a.FacilityId == attachmentSearch.FacilityId
                            && (string.IsNullOrEmpty(attachmentSearch.Building) ||
                                a.HousingUnitLocation == attachmentSearch.Building)
                            && (string.IsNullOrEmpty(attachmentSearch.Number) ||
                                a.HousingUnitNumber == attachmentSearch.Number)
                            && (attachmentSearch.HousingList.Count == 0 || attachmentSearch.HousingList
                                .Contains(new KeyValuePair<string, string>(a.HousingUnitLocation,
                                    a.HousingUnitNumber))))
                        // filter based on from date and to data.
                        && (!attachmentSearch.FromDate.HasValue || a.CreateDate.HasValue
                            && a.CreateDate.Value.Date >= attachmentSearch.FromDate)
                        && (!attachmentSearch.ToDate.HasValue || a.CreateDate.HasValue
                            && a.CreateDate.Value.Date <= attachmentSearch.ToDate)
                        // filter based on who has created.
                        && (attachmentSearch.PersonnelId == 0 || a.CreatedBy == attachmentSearch.PersonnelId)
                        // filter based on attachment key words.
                        && (string.IsNullOrEmpty(attachmentSearch.Keyword)
                            || !string.IsNullOrEmpty(a.AppletsSavedKeyword1) &&
                            a.AppletsSavedKeyword1.Contains(attachmentSearch.Keyword)
                            || !string.IsNullOrEmpty(a.AppletsSavedKeyword2) &&
                            a.AppletsSavedKeyword2.Contains(attachmentSearch.Keyword)
                            || !string.IsNullOrEmpty(a.AppletsSavedKeyword3) &&
                            a.AppletsSavedKeyword3.Contains(attachmentSearch.Keyword)
                            || !string.IsNullOrEmpty(a.AppletsSavedKeyword4) &&
                            a.AppletsSavedKeyword4.Contains(attachmentSearch.Keyword)
                            || !string.IsNullOrEmpty(a.AppletsSavedKeyword5) &&
                            a.AppletsSavedKeyword5.Contains(attachmentSearch.Keyword)))
                .OrderBy(a => a.CreateDate)
                .Select(y => new PrebookAttachment
                {
                    AttachmentId = y.AppletsSavedId,
                    AttachmentDate = y.CreateDate,
                    AttachmentDeleted = y.AppletsDeleteFlag == 1,
                    AttachmentType = y.AppletsSavedType,
                    AttachmentTitle = y.AppletsSavedTitle,
                    AttachmentDescription = y.AppletsSavedDescription,
                    AttachmentKeyword1 = y.AppletsSavedKeyword1,
                    AttachmentKeyword2 = y.AppletsSavedKeyword2,
                    AttachmentKeyword3 = y.AppletsSavedKeyword3,
                    AttachmentKeyword4 = y.AppletsSavedKeyword4,
                    AttachmentKeyword5 = y.AppletsSavedKeyword5,
                    InmatePrebookId = y.InmatePrebookId,
                    AltSentRequestId = y.AltSentRequestId,
                    InmateId = y.InmateId,
                    DisciplinaryIncidentId = y.DisciplinaryIncidentId,
                    MedicalInmateId = y.MedicalInmateId,
                    ArrestId = y.ArrestId,
                    IncarcerationId = y.IncarcerationId,
                    ProgramCaseInmateId = y.ProgramCaseInmateId,
                    GrievanceId = y.GrievanceId,
                    RegistrantRecordId = y.RegistrantRecordId,
                    FacilityId = y.FacilityId,
                    HousingUnitLocation = y.HousingUnitLocation,
                    HousingUnitNumber = y.HousingUnitNumber,
                    AltSentId = y.AltSentId,
                    ExternalInmateId = y.ExternalInmateId,
                    AttachmentFile = Path.GetFileName(y.AppletsSavedPath),
                    CreatedBy = new PersonnelVm
                    {
                        PersonLastName = y.CreatedByNavigation.PersonNavigation.PersonLastName,
                        PersonFirstName = y.CreatedByNavigation.PersonNavigation.PersonFirstName,
                        PersonMiddleName = y.CreatedByNavigation.PersonNavigation.PersonMiddleName
                    },
                    UpdateDate = y.UpdateDate,
                    UpdatedBy = new PersonnelVm
                    {
                        PersonLastName = y.UpdatedByNavigation.PersonNavigation.PersonLastName,
                        PersonFirstName = y.UpdatedByNavigation.PersonNavigation.PersonFirstName,
                        PersonMiddleName = y.UpdatedByNavigation.PersonNavigation.PersonMiddleName
                    },
                    PersonInfo = new PersonVm
                    {
                        InmateId = attachmentSearch.Flag == AttachmentType.INCARATTACHTYPE
                            ? y.Incarceration.Inmate.InmateId : attachmentSearch.Flag == AttachmentType.BOOKATTACHTYPE
                            ? y.Arrest.Inmate.InmateId : attachmentSearch.Flag == AttachmentType.CLASATTACHTYPE
                            ? y.InmateId : attachmentSearch.Flag == AttachmentType.MEDATTACHTYPE
                            ? y.MedicalInmateId : default
                    }
                }).ToList();
            List<PersonVm> personVms = _inmateService.GetInmateDetails(prebookAttachments
                .Where(a => a.PersonInfo.InmateId.HasValue)
                .Select(a => a.PersonInfo.InmateId ?? 0).ToList());
            prebookAttachments.Where(a => a.PersonInfo.InmateId > 0).ToList().ForEach(a => 
                a.PersonInfo = personVms.SingleOrDefault(x => x.InmateId == a.PersonInfo.InmateId));
            return prebookAttachments;
        }

        public async Task<int> InsertPrebookAttachment(PrebookAttachment applets)
        {
            string savepath = $@"{_configuration.GetSection(PathConstants.SITEVARIABLES)[PathConstants.PHOTOPATH]}",
                dbsavepath =
                    $@"{PathConstants.APPLETS}{_configuration.GetSection(PathConstants.ATTACHMENTPATHS)[applets.AttachType]}";
            string datePath =
                $@"{DateTime.Now.ToString(PathConstants.DATEPATH, System.Globalization.CultureInfo.InvariantCulture)}{
                        PathConstants.BACKWARDSLASH
                    }";
            savepath += $@"{dbsavepath}{datePath}";
            dbsavepath += datePath;
            if (!Directory.Exists(savepath))
            {
                Directory.CreateDirectory(savepath);
            }
            savepath = $@"{savepath}{applets.AttachmentFile}";
            string source = $@"{PathConstants.TEMPFILES}{applets.AttachmentFile}";
            dbsavepath = $@"{dbsavepath}{applets.AttachmentFile}";
            File.Move(source, savepath);
            if (applets.AttachType != AttachmentType.HOUSATTACHTYPE || !applets.HousingGroups.Any())
            {
                AppletsSaved appletSaved = new AppletsSaved
                {
                    AppletsSavedType = applets.AttachmentType,
                    AppletsSavedTitle = applets.AttachmentTitle,
                    AppletsSavedDescription = applets.AttachmentDescription,
                    AppletsSavedKeyword1 = applets.AttachmentKeyword1,
                    AppletsSavedKeyword2 = applets.AttachmentKeyword2,
                    AppletsSavedKeyword3 = applets.AttachmentKeyword3,
                    AppletsSavedKeyword4 = applets.AttachmentKeyword4,
                    AppletsSavedKeyword5 = applets.AttachmentKeyword5,
                    AppletsSavedPath = dbsavepath.Substring(Path.GetPathRoot(dbsavepath).Length),
                    CreateDate = DateTime.Now,
                    CreatedBy = _personnelId,
                    InmatePrebookId = applets.InmatePrebookId,
                    AltSentRequestId = applets.AltSentRequestId,
                    InmateId = applets.InmateId,
                    DisciplinaryIncidentId = applets.DisciplinaryIncidentId,
                    MedicalInmateId = applets.MedicalInmateId,
                    ArrestId = applets.ArrestId,
                    IncarcerationId = applets.IncarcerationId,
                    ProgramCaseInmateId = applets.ProgramCaseInmateId,
                    GrievanceId = applets.GrievanceId,
                    RegistrantRecordId = applets.RegistrantRecordId,
                    FacilityId = applets.FacilityId,
                    HousingUnitLocation = applets.HousingUnitLocation,
                    HousingUnitNumber = applets.HousingUnitNumber,
                    AltSentId = applets.AltSentId,
                    ExternalInmateId = applets.ExternalInmateId,
                    PREAInmateId = applets.PREAInmateId,
                    InvestigationId = applets.InvestigationId,
                    MailRecordid = applets.MailRecordid
                };
                _context.AppletsSaved.Add(appletSaved);
                _context.SaveChanges();
                AppletsSavedHistory history = new AppletsSavedHistory
                {
                    PersonnelId = _personnelId,
                    AppletsSavedId = appletSaved.AppletsSavedId,
                    CreateDate = DateTime.Now,
                    AppletsSavedHistoryList = applets.History
                };
                _context.Add(history);
            }
            else
            {
                List<AppletsSaved> appletsSaveds = applets.HousingGroups.Select(a => new AppletsSaved
                {
                    AppletsSavedType = applets.AttachmentType,
                    AppletsSavedTitle = applets.AttachmentTitle,
                    AppletsSavedDescription = applets.AttachmentDescription,
                    AppletsSavedKeyword1 = applets.AttachmentKeyword1,
                    AppletsSavedKeyword2 = applets.AttachmentKeyword2,
                    AppletsSavedKeyword3 = applets.AttachmentKeyword3,
                    AppletsSavedKeyword4 = applets.AttachmentKeyword4,
                    AppletsSavedKeyword5 = applets.AttachmentKeyword5,
                    AppletsSavedPath = dbsavepath.Substring(Path.GetPathRoot(dbsavepath).Length),
                    CreateDate = DateTime.Now,
                    CreatedBy = _personnelId,
                    FacilityId = applets.FacilityId,
                    HousingUnitLocation = a.Key,
                    HousingUnitNumber = a.Value
                }).ToList();
                _context.AppletsSaved.AddRange(appletsSaveds);
                _context.SaveChanges();
                int i = 0;
                _context.AppletsSavedHistory.AddRange(appletsSaveds
                    .Select(a => new AppletsSavedHistory
                    {
                        PersonnelId = _personnelId,
                        AppletsSavedId = a.AppletsSavedId,
                        CreateDate = DateTime.Now,
                        AppletsSavedHistoryList = applets.Histories[i++]
                    }));
            }
            return await _context.SaveChangesAsync();
        }

        public async Task<int> UpdatePrebookAttachment(PrebookAttachment applets)
        {
            AppletsSaved appletsSaved = _context.AppletsSaved.Single(ap => ap.AppletsSavedId == applets.AttachmentId);
            appletsSaved.AppletsSavedType = applets.AttachmentType;
            appletsSaved.AppletsSavedTitle = applets.AttachmentTitle;
            appletsSaved.AppletsSavedDescription = applets.AttachmentDescription;
            appletsSaved.AppletsSavedKeyword1 = applets.AttachmentKeyword1;
            appletsSaved.AppletsSavedKeyword2 = applets.AttachmentKeyword2;
            appletsSaved.AppletsSavedKeyword3 = applets.AttachmentKeyword3;
            appletsSaved.AppletsSavedKeyword4 = applets.AttachmentKeyword4;
            appletsSaved.AppletsSavedKeyword5 = applets.AttachmentKeyword5;
            appletsSaved.UpdatedBy = _personnelId;
            appletsSaved.UpdateDate = DateTime.Now;
            appletsSaved.InmatePrebookId = applets.InmatePrebookId;
            appletsSaved.AltSentRequestId = applets.AltSentRequestId;
            appletsSaved.InmateId = applets.InmateId;
            appletsSaved.DisciplinaryIncidentId = applets.DisciplinaryIncidentId;
            appletsSaved.MedicalInmateId = applets.MedicalInmateId;
            appletsSaved.ArrestId = applets.ArrestId;
            appletsSaved.IncarcerationId = applets.IncarcerationId;
            appletsSaved.ProgramCaseInmateId = applets.ProgramCaseInmateId;
            appletsSaved.GrievanceId = applets.GrievanceId;
            appletsSaved.RegistrantRecordId = applets.RegistrantRecordId;
            appletsSaved.FacilityId = applets.FacilityId;
            appletsSaved.HousingUnitLocation = applets.HousingUnitLocation;
            appletsSaved.HousingUnitNumber = applets.HousingUnitNumber;
            appletsSaved.AltSentId = applets.AltSentId;
            appletsSaved.ExternalInmateId = applets.ExternalInmateId;
            AppletsSavedHistory history = new AppletsSavedHistory
            {
                PersonnelId = _personnelId,
                AppletsSavedId = applets.AttachmentId,
                CreateDate = DateTime.Now,
                AppletsSavedHistoryList = applets.History
            };
            _context.Add(history);
            if (Path.GetFileName(appletsSaved.AppletsSavedPath) == applets.AttachmentFile)
                return await _context.SaveChangesAsync();
            string deletepath =
                $@"{_configuration.GetSection(PathConstants.SITEVARIABLES)[PathConstants.PHOTOPATH]}{
                        PathConstants.BACKWARDSLASH}{appletsSaved.AppletsSavedPath}";
            if (File.Exists(deletepath))
            {
                File.Delete(deletepath);
            }
            string savepath = _configuration.GetSection(PathConstants.SITEVARIABLES)[PathConstants.PHOTOPATH];
            string dbsavepath =
                $@"{PathConstants.APPLETS}{
                        _configuration.GetSection(PathConstants.ATTACHMENTPATHS)[applets.AttachType]
                    }";
            string datePath = $@"{DateTime.Now.ToString(PathConstants.DATEPATH)}{PathConstants.BACKWARDSLASH}";
            savepath += $@"{dbsavepath}{datePath}";
            dbsavepath += datePath;
            if (!Directory.Exists(savepath))
            {
                Directory.CreateDirectory(savepath);
            }
            savepath = $@"{savepath}{applets.AttachmentFile}";
            string source = $@"{PathConstants.TEMPFILES}{applets.AttachmentFile}";
            dbsavepath = $@"{dbsavepath}{applets.AttachmentFile}";
            File.Move(source, savepath);
            appletsSaved.AppletsSavedPath = dbsavepath.Substring(Path.GetPathRoot(dbsavepath).Length);
            return await _context.SaveChangesAsync();
        }

        public AttachmentComboBoxes LoadPrebookAttachmentEntry(string attachType, int inmateId, int facilityId)
        {
            AttachmentComboBoxes attachmentComboBoxes = new AttachmentComboBoxes
            {
                LookupTypes = _commonService.GetLookups(new[] { attachType })
                    .Where(a => !string.IsNullOrEmpty(a.LookupDescription))
                    .OrderBy(a => a.LookupDescription)
                    .ThenBy(a => a.LookupIndex).ToList()
            };
            if ((attachType == AttachmentType.BOOKATTACHTYPE || attachType == AttachmentType.INCARATTACHTYPE) &&
                inmateId > 0)
            {
                attachmentComboBoxes.BookingNumers = GetBookingNumbers(inmateId);
            }
            else if (attachType == AttachmentType.HOUSATTACHTYPE && facilityId > 0)
            {
                attachmentComboBoxes.Facilities = _commonService.GetFacilities()
                    .Where(a => !string.IsNullOrEmpty(a.FacilityAbbr)).OrderBy(a => a.FacilityAbbr)
                    .Select(a => new KeyValuePair<string, int>(a.FacilityAbbr, a.FacilityId)).ToList();
                attachmentComboBoxes.HousingList = _cellService.GetHousingUnit(facilityId);
                attachmentComboBoxes.HousingGroupList = _cellService.GetHousingGroup(facilityId);
            }
            else if (attachType == AttachmentType.ALTSENTATTACHTYPE && inmateId > 0)
            {
                attachmentComboBoxes.Programs = _altSentService.LoadProgramList(inmateId);
            }
            return attachmentComboBoxes;
        }

        public List<HistoryVm> LoadAttachHistory(int appletSavedId)
        {
            List<HistoryVm> appletsSavedHistory =
                _context.AppletsSavedHistory.Where(a => a.AppletsSavedId == appletSavedId)
                    .OrderByDescending(a => a.CreateDate)
                    .Select(a => new HistoryVm
                    {
                        HistoryId = a.AppletsSavedHistoryId,
                        CreateDate = a.CreateDate,
                        PersonId = a.Personnel.PersonId,
                        PersonLastName = a.Personnel.PersonNavigation.PersonLastName,
                        OfficerBadgeNumber = a.Personnel.OfficerBadgeNum,
                        HistoryList = a.AppletsSavedHistoryList
                    }).ToList();
            appletsSavedHistory.Where(a => !string.IsNullOrEmpty(a.HistoryList)).ToList().ForEach(a =>
            {
                a.Header = JsonConvert.DeserializeObject<Dictionary<string, string>>(a.HistoryList)
                    .Select(x => new PersonHeader
                    {
                        Header = x.Key,
                        Detail = x.Value
                    }).ToList();
            });
            return appletsSavedHistory;
        }

        public string OpenPrebookAttachment(int appletSavedId)
        {
            AppletsSaved applets = _context.AppletsSaved.Single(x => x.AppletsSavedId == appletSavedId);
            return applets.AppletsSavedIsExternal ? _appletsSavedService.GetExternalPath() + applets.AppletsSavedPath
                : _appletsSavedService.GetPath() + applets.AppletsSavedPath;
        }

        public async Task<int> DeletePrebookAttachment(int appletSavedId)
        {
            AppletsSaved appletSaved = _context.AppletsSaved.Single(ap => ap.AppletsSavedId == appletSavedId);
            appletSaved.AppletsDeleteFlag = 1;
            appletSaved.DeleteDate = DateTime.Now;
            appletSaved.DeletedBy = _personnelId;
            _context.AppletsSavedHistory.Add(new AppletsSavedHistory
            {
                AppletsSavedId = appletSavedId,
                PersonnelId = _personnelId,
                CreateDate = DateTime.Now,
                AppletsSavedHistoryList = JsonConvert.SerializeObject(new
                {
                    DELETE = CustomConstants.Yes
                })
            });
            return await _context.SaveChangesAsync();
        }

        public async Task<int> UndoDeletePrebookAttachment(int appletSavedId, string history)
        {
            AppletsSaved appletSaved = _context.AppletsSaved.Single(ap => ap.AppletsSavedId == appletSavedId);
            appletSaved.AppletsDeleteFlag = 0;
            appletSaved.DeleteDate = DateTime.Now;
            appletSaved.DeletedBy = _personnelId;
            _context.AppletsSavedHistory.Add(new AppletsSavedHistory
            {
                AppletsSavedId = appletSavedId,
                PersonnelId = _personnelId,
                CreateDate = DateTime.Now,
                AppletsSavedHistoryList = history
            });
            return await _context.SaveChangesAsync();
        }

        public PrebookAttachment GetAttachmentDetail(int appletsSavedId) => _context.AppletsSaved.Where(a => 
                a.AppletsSavedId == appletsSavedId)
                .Select(a => new PrebookAttachment {
                    AttachmentId = a.AppletsSavedId,
                    AttachmentDate = a.CreateDate,
                    AttachmentDeleted = a.AppletsDeleteFlag == 1,
                    AttachmentType = a.AppletsSavedType,
                    AttachmentTitle = a.AppletsSavedTitle,
                    AttachmentDescription = a.AppletsSavedDescription,
                    AttachmentKeyword1 = a.AppletsSavedKeyword1,
                    AttachmentKeyword2 = a.AppletsSavedKeyword2,
                    AttachmentKeyword3 = a.AppletsSavedKeyword3,
                    AttachmentKeyword4 = a.AppletsSavedKeyword4,
                    AttachmentKeyword5 = a.AppletsSavedKeyword5,
                    InmatePrebookId = a.InmatePrebookId,
                    DisciplinaryIncidentId = a.DisciplinaryIncidentId,
                    InmateId = a.InmateId,
                    AttachmentFile = Path.GetFileName(a.AppletsSavedPath),
                    CreatedBy = new PersonnelVm {
                        PersonLastName = a.CreatedByNavigation.PersonNavigation.PersonLastName,
                        PersonFirstName = a.CreatedByNavigation.PersonNavigation.PersonFirstName,
                        PersonMiddleName = a.CreatedByNavigation.PersonNavigation.PersonMiddleName
                    },
                    UpdateDate = a.UpdateDate,
                    UpdatedBy = new PersonnelVm {
                        PersonLastName = a.UpdatedByNavigation.PersonNavigation.PersonLastName,
                        PersonFirstName = a.UpdatedByNavigation.PersonNavigation.PersonFirstName,
                        PersonMiddleName = a.UpdatedByNavigation.PersonNavigation.PersonMiddleName
                    }
                }).Single();

        #endregion

        public async Task<int> UpdateMedPrescreenStatusStartComplete(MedPrescreenStatus medPrescreenStatus)
        {
            //Update status flag in Inmate Prebook
            InmatePrebook inmatePrebook =
                _context.InmatePrebook.SingleOrDefault(x => x.InmatePrebookId == medPrescreenStatus.PrebookId);
            if (inmatePrebook == null) return await _context.SaveChangesAsync();
            inmatePrebook.MedPrescreenStartFlag = medPrescreenStatus.MedStartFlag;
            inmatePrebook.MedPrescreenStartBy = _personnelId;
            inmatePrebook.MedPrescreenStartDate = DateTime.Now;
            inmatePrebook.CompleteFlag = medPrescreenStatus.MedCompleteFlag;
            return await _context.SaveChangesAsync();
        }

        public async Task<int> UpdateMedPrescreenStatus(MedPrescreenStatus medPrescreenStatus)
        {
            //Update status flag in Inmate Prebook
            InmatePrebook inmatePrebook =
                _context.InmatePrebook.SingleOrDefault(x => x.InmatePrebookId == medPrescreenStatus.PrebookId);
            if (inmatePrebook != null)
            {
                inmatePrebook.MedPrescreenStartFlag = medPrescreenStatus.MedStartFlag;
                inmatePrebook.MedPrescreenStatusNote = medPrescreenStatus.MedPrescreenStatusNote;
                inmatePrebook.MedPrescreenStatusFlag = (int?)medPrescreenStatus.MedPrescreenStatusFlag;
                inmatePrebook.MedPrescreenStatusBy = _personnelId;
                inmatePrebook.CompleteFlag = medPrescreenStatus.MedCompleteFlag;
                inmatePrebook.MedPrescreenStatusDate = DateTime.Now;
                inmatePrebook.UpdateDate = DateTime.Now;
            }

            //Insert into Med Prescreen history
            InmatePrebookMedPrescreenHistory prescreenHistory = new InmatePrebookMedPrescreenHistory
            {
                InmatePrebookId = medPrescreenStatus.PrebookId,
                MedPrescreenStatusFlag = (int)medPrescreenStatus.MedPrescreenStatusFlag,
                MedPrescreenStatusBy = _personnelId,
                MedPrescreenStatusDate = DateTime.Now,
                MedPrescreenStatusNote = medPrescreenStatus.MedPrescreenStatusNote
            };
            _context.InmatePrebookMedPrescreenHistory.Add(prescreenHistory);
            EventVm eventVm = new EventVm
            {
                CorresId = medPrescreenStatus.PrebookId,
                PersonId = inmatePrebook?.PersonId
            };
            // TODO - why we are not using enums?!
            // and this is so bad way to do this!
            if ((int?)medPrescreenStatus.MedPrescreenStatusFlag == 2)
            {
                eventVm.EventName = EventNameConstants.MEDPRESCREENBYPASS;
            }
            else if ((int?)medPrescreenStatus.MedPrescreenStatusFlag == -1)
            {
                eventVm.EventName = EventNameConstants.MEDPRESCREENREJECT;
            }
            else if ((int?)medPrescreenStatus.MedPrescreenStatusFlag == 1)
            {
                eventVm.EventName = EventNameConstants.MEDPRESCREENACCEPT;
            }
            if (!string.IsNullOrEmpty(eventVm.EventName))
            {
                //_commonService.EventHandle(eventVm);
                _interfaceEngineService.Export(new ExportRequestVm
                {
                    EventName = eventVm.EventName,
                    PersonnelId = _personnelId,
                    Param1 = eventVm.PersonId.ToString(),
                    Param2 = eventVm.CorresId.ToString()
                });
            }
            return await _context.SaveChangesAsync();
        }

        //Attachment History
        public IEnumerable<AppletsSavedVm> GetAttachmentHistory(int prebookId)
        {
            List<AppletsSavedVm> lstattachment = _context.AppletsSaved
                .Where(x => x.InmatePrebookId == prebookId)
                .Select(y => new AppletsSavedVm
                {
                    CreateDate = y.CreateDate,
                    CreatedByName = new PersonVm
                    {
                        PersonFirstName = y.CreatedByNavigation.PersonNavigation.PersonFirstName,
                        PersonLastName = y.CreatedByNavigation.PersonNavigation.PersonLastName,
                        PersonMiddleName = y.CreatedByNavigation.PersonNavigation.PersonMiddleName
                    },
                    UpdatedByName = new PersonVm
                    {
                        PersonFirstName = y.UpdatedByNavigation.PersonNavigation.PersonFirstName,
                        PersonLastName = y.UpdatedByNavigation.PersonNavigation.PersonLastName,
                        PersonMiddleName = y.UpdatedByNavigation.PersonNavigation.PersonMiddleName
                    },
                    PersonName = new PersonVm
                    {
                        PersonFirstName = y.InmatePrebook.PersonFirstName,
                        PersonLastName = y.InmatePrebook.PersonLastName,
                        PersonMiddleName = y.InmatePrebook.PersonMiddleName
                    },
                    AppletsSavedType = y.AppletsSavedType,
                    AppletsSavedTitle = y.AppletsSavedTitle,
                    AppletsSavedDescription = y.AppletsSavedDescription,
                    AppletsSavedPath = y.AppletsSavedIsExternal ? _appletsSavedService.GetExternalPath() + y.AppletsSavedPath : y.AppletsSavedPath,
                    AppletsDeleteFlag = y.AppletsDeleteFlag
                }).ToList();
            return lstattachment;
        }

        public async Task<int> UpdatePrebookComplete(int inmatePrebookId)
        {
            InmatePrebook x = _context.InmatePrebook.SingleOrDefault(i => i.InmatePrebookId == inmatePrebookId);
            if (x == null)
            {
                return 0;
            }
            x.CompleteDate = DateTime.Now;
            x.CompleteFlag = 1;
            int res = await _context.SaveChangesAsync();
            await _atimsHubService.GetIntakePrebookCount();
            return res;
        }

        public async Task<int> UpdatePrebookUndoComplete(int inmatePrebookId)
        {
            InmatePrebook x = _context.InmatePrebook.SingleOrDefault(i => i.InmatePrebookId == inmatePrebookId);
            if (x == null)
            {
                return 0;
            }
            x.CompleteDate = DateTime.Now;
            x.CompleteFlag = 0;
            int res = await _context.SaveChangesAsync();
            await _atimsHubService.GetIntakePrebookCount();
            return res;
        }

        public List<KeyValuePair<string, int>> GetBookingNumbers(int inmateId) =>
            _context.Arrest.Where(a => a.InmateId == inmateId).OrderBy(a => a.ArrestId)
                .Select(a => new KeyValuePair<string, int>(a.ArrestBookingNo, a.ArrestId)).ToList();

        public async Task<int> CreateVehicleModel(VehicleModelVm model)
        {
            VehicleModel vehicleModel = new VehicleModel
            {
                VehicleMakeId = model.VehicleMakeId,
                VehicleModelName = model.VehicleModelName,
                ModelCode = model.ModelCode,
                CreateDate = DateTime.Now,
                CreateBy = _personnelId,
                DeleteFlag = 0
            };

            _context.VehicleModel.Add(vehicleModel);
            return await _context.SaveChangesAsync();
        }

        public async Task<int> UpdateVehicleModel(VehicleModelVm vehicleModelVm)
        {
            VehicleModel vehicleModel =
                _context.VehicleModel.SingleOrDefault(s => s.VehicleModelId == vehicleModelVm.VehicleModelId);
            if (vehicleModel != null)
            {
                vehicleModel.DeleteBy = _personnelId;
                vehicleModel.DeleteFlag = vehicleModelVm.DeleteFlag;
                vehicleModel.DeleteDate = DateTime.Now;
            }

            return await _context.SaveChangesAsync();
        }


        public async Task<int> CreateVehicleMake(VehicleMakeVm vehicleMakevm)
        {
            VehicleMake vehicleMake = new VehicleMake
            {
                VehicleMakeName = vehicleMakevm.VehicleMakeName,
                MakeCode = vehicleMakevm.MakeCode,
                CreateDate = DateTime.Now,
                CreateBy = _personnelId,
                DeleteFlag = 0
            };
            _context.VehicleMake.Add(vehicleMake);
            return await _context.SaveChangesAsync();
        }

        public async Task<int> UpdateVehicleMake(VehicleMakeVm vehicleMakevm)
        {
            VehicleMake vehicleMake =
                _context.VehicleMake.SingleOrDefault(s => s.VehicleMakeId == vehicleMakevm.VehicleMakeId);
            if (vehicleMake == null) return -1;
            vehicleMake.DeleteBy = _personnelId;
            vehicleMake.DeleteFlag = vehicleMakevm.DeleteFlag;
            vehicleMake.DeleteDate = DateTime.Now;

            return await _context.SaveChangesAsync();
        }

        #region Inmate Prebook Case

        public List<InmatePrebookCaseVm> GetPrebookCases(int inmatePrebookId, bool deleteFlag)
        {
            List<Lookup> lstLookups = _context.Lookup.Where(lk =>
                lk.LookupType == LookupConstants.ARRTYPE).ToList();

            List<PrebookCharge> lstCharges = GetPrebookCharges(inmatePrebookId, deleteFlag, 0);
            List<InmatePrebookWarrantVm> lstWarrants = GetPrebookWarrant(inmatePrebookId, deleteFlag).ToList();

            List<InmatePrebookCaseVm> lstInmatePrebookCaseVms = _context.InmatePrebookCase
                .Where(ipc => ipc.InmatePrebookId == inmatePrebookId && (deleteFlag || ipc.DeleteFlag == deleteFlag))
                .Select(ipc => new InmatePrebookCaseVm
                {
                    InmatePrebookCaseId = ipc.InmatePrebookCaseId,
                    CaseType = lstLookups.SingleOrDefault(lk => lk.LookupIndex == ipc.ArrestType).LookupDescription,
                    CaseNumber = ipc.CaseNumber,
                    LstCharges = lstCharges.Where(c => c.InmatePrebookCaseId == ipc.InmatePrebookCaseId).ToList(),
                    LstWarrants = lstWarrants.Where(c => c.InmatePrebookCaseId == ipc.InmatePrebookCaseId).ToList(),
                    CaseNote = ipc.CaseNote,
                    CaseTypeId = ipc.ArrestType,
                    DeleteFlag = ipc.DeleteFlag
                }).ToList();

            return lstInmatePrebookCaseVms;
        }

        public List<InmatePrebookCaseVm> InsertUpdatePrebookCase(List<InmatePrebookCaseVm> prebookCaseVms, bool deleteFlag)
        {
            int inmatePrebookId = prebookCaseVms[0].InmatePrebookId;
            InmatePrebookCase inmatePrebookCase;
            prebookCaseVms.ForEach(prebookCase =>
            {
                if (prebookCase.InmatePrebookCaseId > 0)
                {
                    inmatePrebookCase = new InmatePrebookCase();
                    inmatePrebookCase = _context.InmatePrebookCase
                        .Single(ipc => ipc.InmatePrebookCaseId == prebookCase.InmatePrebookCaseId);
                    inmatePrebookCase.ArrestType = prebookCase.CaseTypeId;
                    inmatePrebookCase.CaseNumber = prebookCase.CaseNumber;
                    inmatePrebookCase.CaseNote = prebookCase.CaseNote;
                }
                else
                {
                    inmatePrebookCase = new InmatePrebookCase
                    {
                        InmatePrebookId = prebookCase.InmatePrebookId,
                        ArrestType = prebookCase.CaseTypeId,
                        CaseNumber = prebookCase.CaseNumber,
                        CaseNote = prebookCase.CaseNote,
                        CreateBy = _personnelId,
                        CreateDate = DateTime.Now
                    };
                    _context.Add(inmatePrebookCase);
                }
                _context.SaveChanges();
            });
            return GetPrebookCases(inmatePrebookId, deleteFlag);
        }

        public async Task<int> DeleteUndoPrebookCase(InmatePrebookCaseVm prebookCaseVm)
        {
            InmatePrebookCase inmatePrebookCase =
                _context.InmatePrebookCase.Single(ipw => ipw.InmatePrebookCaseId == prebookCaseVm.InmatePrebookCaseId);
            inmatePrebookCase.DeleteFlag = prebookCaseVm.DeleteFlag;
            inmatePrebookCase.DeleteDate = DateTime.Now;
            inmatePrebookCase.DeleteBy = _personnelId;
            if (prebookCaseVm.DeleteFlag)
            {
                inmatePrebookCase.DeleteReason = prebookCaseVm.DeleteReason;
            }
            return await _context.SaveChangesAsync();
        }

        #endregion

        public PrebookProperty GetPropertyDetails(PrebookProperty propertyDetail)
        {
            propertyDetail.InmateHeaderDetails = new InmatePdfHeader
            {
                AgencyName =
                    _context.Agency.FirstOrDefault(w => w.AgencyBookingFlag)?.AgencyName,
                StampDate = DateTime.Now,
                SummaryHeader = InventoryQueueConstants.PREBOOKPROPERTY,
                PbpcPath = _photoService.GetPbpcPath() + propertyDetail.logoPath,
                ArrestingAgencyName = _context.InmatePrebook.Where(w => w.InmatePrebookId == propertyDetail.prebookId)
                    .Select(x => x.ArrestAgency.AgencyName).FirstOrDefault()
            };

            return propertyDetail;
        }

        public int GetDefaultAgencyId() => _context.Personnel.Single(a => a.PersonnelId == _personnelId).AgencyId;
    }
}
