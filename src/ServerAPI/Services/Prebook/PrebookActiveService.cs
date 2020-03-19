using ServerAPI.ViewModels;
using System.Collections.Generic;
using System.Linq;
using GenerateTables.Models;
using System;
using System.Data;
using System.Threading.Tasks;
using System.Globalization;
using Microsoft.AspNetCore.Http;
using ServerAPI.Utilities;
using System.Data.SqlClient;

// ReSharper disable once CheckNamespace
namespace ServerAPI.Services
{
    public class PrebookActiveService : IPrebookActiveService
    {
        private readonly AAtims _context;
        private readonly int _personnelId;
        private string _facilityAbbr;
        private readonly IAtimsHubService _atimsHubService;
        

        public PrebookActiveService(AAtims context,
            IHttpContextAccessor httpContextAccessor,
            IAtimsHubService atimsHubService)
        {
            _context = context;
            _personnelId = Convert.ToInt32(httpContextAccessor.HttpContext.User.FindFirst("personnelId")?.Value);
            _atimsHubService = atimsHubService;
        }

        public List<InmatePrebookVm> GetPrebooks(bool deleteFlag)
        {
            List<InmatePrebookVm> inmatePrebook = (
                from ip in _context.InmatePrebook
                where (ip.IncarcerationId == 0 || !ip.IncarcerationId.HasValue)
                      && (deleteFlag || !ip.DeleteFlag.HasValue)
                      && (ip.TempHoldId == 0 || !ip.TempHoldId.HasValue)
                      && (ip.CourtCommitFlag == 0 || !ip.CourtCommitFlag.HasValue)
                select new InmatePrebookVm
                {
                    InmatePrebookId = ip.InmatePrebookId,
                    PersonId = ip.PersonId ?? 0,
                    PersonLastName = ip.PersonLastName,
                    PersonFirstName = ip.PersonFirstName,
                    PersonMiddleName = ip.PersonMiddleName,
                    PersonSuffix = ip.PersonSuffix,
                    PersonDob = ip.PersonDob,
                    PrebookNumber = ip.PreBookNumber,
                    ArrestDate = ip.ArrestDate,
                    PrebookDate = ip.PrebookDate,
                    //LastStep = ip.AppAOWizardFixedStepsId == null || ip.AppAOWizardFixedStepsId == 110
                    //    ? new WizardStepVm
                    //    {
                    //        StepId = 120,
                    //        StepName = "PRE-BOOK INFO",
                    //        StepOrder = 1
                    //    }
                    //    : new WizardStepVm
                    //    {
                    //        StepId = ip.AppAoWizardFixedSteps.AppAoWizardFixedStepsId,
                    //        StepName = ip.AppAoWizardFixedSteps.StepName,
                    //        StepOrder = ip.AppAoWizardFixedSteps.StepDisplayOrder
                    //    },
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
                    PrebookComplete = ip.CompleteFlag == 1,
                    DeleteFlag = ip.DeleteFlag == 1,
                    TemporaryHold = ip.TemporaryHold == 1,
                    CreateBy = ip.CreateBy,
                    WizardStepProgress = ip.AoWizardProgressInmatePrebook.Count > 0
                        ? ip.AoWizardProgressInmatePrebook.SingleOrDefault().AoWizardStepProgress.Select(s =>
                            new AoWizardStepProgressVm
                            {
                                ComponentId = s.AoComponentId,
                                StepComplete = s.StepComplete,
                            }).ToList()
                        : null
                }).OrderBy(o => o.PrebookDate).ToList();

            return inmatePrebook;
        }

        public InmatePrebookVm GetPrebook(int inmatePrebookId)
        {
            InmatePrebookVm x = _context.InmatePrebook.Where(w=> w.InmatePrebookId == inmatePrebookId)
                .Select(ip=> new InmatePrebookVm
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
                    CaseNumber = string.Join(",",
                        ip.InmatePrebookCase.Where(ipc => ipc.InmatePrebookId == ip.InmatePrebookId)
                            .Select(ipc => ipc.CaseNumber)),
                    ContactNumber = ip.PreBookContactNumber,
                    //LastStep = ip.AppAOWizardFixedStepsId == null || ip.AppAOWizardFixedStepsId == 110
                    //    ? new WizardStepVm
                    //    {
                    //        StepId = 120,
                    //        StepName = "PRE-BOOK INFO",
                    //        StepOrder = 1
                    //    }
                    //    : new WizardStepVm
                    //    {
                    //        StepId = ip.AppAoWizardFixedSteps.AppAoWizardFixedStepsId,
                    //        StepName = ip.AppAoWizardFixedSteps.StepName,
                    //        StepOrder = ip.AppAoWizardFixedSteps.StepDisplayOrder
                    //    },
                    ArrestOfficerId = ip.ArrestingOfficerId,
                    ArrestingOfficer = _context.Personnel.Where(w=>w.PersonnelId == ip.ArrestingOfficerId)
                        .Select(p => new PersonnelVm
                        {
                            PersonLastName = p.PersonNavigation.PersonLastName,
                            PersonFirstName = p.PersonNavigation.PersonFirstName,
                            OfficerBadgeNumber = p.OfficerBadgeNum
                        }).Single(),
                    ArrestOfficerName = ip.ArrestOfficerName,
                    ArrestAgencyId = ip.ArrestAgency.AgencyId,
                    ArrestAgencyAbbr = ip.ArrestAgency.AgencyAbbreviation,
                    ArrestAgencyName = ip.ArrestAgency.AgencyName,
                    FacilityId = ip.FacilityId,
                    FacilityAbbr = ip.Facility.FacilityAbbr,
                    ArrestLocation = ip.ArrestLocation,
                    TemporaryHold = ip.TemporaryHold == 1,
                    TransportOfficerName = ip.TransportOfficerName,
                    TransportOfficerId = ip.TransportingOfficerId,
                    TransportOfficerLastName = ip.TransportingOfficer.PersonNavigation.PersonLastName,
                    TransportOfficerFirstName = ip.TransportingOfficer.PersonNavigation.PersonFirstName,
                    TransportOfficerNumber = ip.TransportingOfficer.PersonnelNumber,
                    PrebookComplete = ip.CompleteFlag == 1,
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
                    VehicleFlagString = ip.VehicleFlagString == null
                        ? null
                        : ip.VehicleFlagString.Split(',', StringSplitOptions.RemoveEmptyEntries),
                    WizardProgressId = ip.AoWizardProgressInmatePrebook.SingleOrDefault().AoWizardProgressId,
                    IntakeReviewComment = ip.IntakeReviewComment,
                    CourtCommitFlag = ip.CourtCommitFlag == 1,
                    CourtCommitType = ip.CourtCommitType,
                    ArrestSentenceAltSentNotAllowed = ip.ArrestSentenceAltSentNotAllowed == 1,
                    ArrestCourtDocket = ip.ArrestCourtDocket,
                    ArrestCourtJurisdictionId = ip.ArrestCourtJurisdictionId,
                    ArrestSentenceDescription = ip.ArrestSentenceDescription,
                    ArrestSentenceAmended = ip.ArrestSentenceAmended == 1,
                    ArrestSentencePenalInstitution = ip.ArrestSentencePenalInstitution == 1,
                    ArrestSentenceOptionsRec = ip.ArrestSentenceOptionsRec == 1,
                    ArrestSentenceNoEarlyRelease = ip.ArrestSentenceNoEarlyRelease == 1,
                    ArrestSentenceNoLocalParole = ip.ArrestSentenceNoLocalParole == 1,
                    ArrestSentenceDateInfo = ip.ArrestSentenceDateInfo,
                    ArrestsentenceType = ip.ArrestsentenceType,
                    ArrestSentenceFindings = ip.ArrestSentenceFindings,
                    ArrestSentenceJudgeId = ip.ArrestSentenceJudgeId ?? 0,
                    ArrestSentenceConsecutiveFlag = ip.ArrestSentenceConsecutiveFlag == 1,
                    ArrestSentenceStartDate = ip.ArrestSentenceStartDate,
                    ArrestSentenceDaysInterval = ip.ArrestSentenceDaysInterval,
                    ArrestSentenceDaysAmount = ip.ArrestSentenceDaysAmount ?? 0,
                    ArrestSentenceFineDays = ip.ArrestSentenceFineDays ?? 0,
                    ArrestSentenceFineAmount = ip.ArrestSentenceFineAmount ?? 0,
                    ArrestSentenceFinePaid = ip.ArrestSentenceFinePaid ?? 0,
                    ArrestSentenceFineType = ip.ArrestSentenceFineType,
                    ArrestSentenceFinePerDay = ip.ArrestSentenceFinePerDay ?? 0,
                    ArrestSentenceDaysStayed = ip.ArrestSentenceDaysStayed ?? 0,
                    ArrestTimeServedDays = ip.ArrestTimeServedDays ?? 0,
                    ArrestSentenceForthwith = ip.ArrestSentenceForthwith == 1,
                    IntakeReviewAccepted = ip.IntakeReviewAccepted,
                    IdentifiedPerson = new PersonVm
                    {
                        PersonLastName = ip.PersonId > 0 ? ip.Person.PersonLastName : default,
                        PersonFirstName = ip.PersonId > 0 ? ip.Person.PersonFirstName : default,
                        PersonMiddleName = ip.PersonId > 0 ? ip.Person.PersonMiddleName : default,
                        PersonSuffix = ip.PersonId > 0 ? ip.Person.PersonSuffix : default,
                        PersonDob = ip.PersonId > 0 ? ip.Person.PersonDob : default
                    }
                }).Single();
            if (x.PersonId.HasValue && x.PersonId != 0)
            {
                x.InmateId = _context.Inmate.SingleOrDefault(w => w.PersonId == x.PersonId)?.InmateId;

                x.InmateDetail = new InmateDetail
                {
                    PersonId = x.PersonId ?? 0,
                    InmateId = x.InmateId ?? 0,
                    Facility = new FacilityVm
                    {
                        FacilityId = x.FacilityId
                    }
                };
                x.IdentifiedPerson = _context.Person.Where(a => a.PersonId == x.PersonId)
                    .Select(a => new PersonVm
                    {
                        PersonLastName = a.PersonLastName,
                        PersonFirstName = a.PersonFirstName,
                        PersonMiddleName = a.PersonMiddleName,
                        PersonSuffix = a.PersonSuffix,
                        PersonDob = a.PersonDob
                    }).Single();
            }

            InmatePrebookCase inmatePrebookCase = _context.InmatePrebookCase
                .FirstOrDefault(ipc => ipc.InmatePrebookId == inmatePrebookId);
            if (inmatePrebookCase != null)
            {
                x.InmatePrebookCaseId = inmatePrebookCase.InmatePrebookCaseId;
                x.ArrestType = inmatePrebookCase.ArrestType != null
                    ? (double?) inmatePrebookCase.ArrestType
                    : default(double?);
                x.CaseNote = inmatePrebookCase.CaseNote;
                x.CaseNumber = inmatePrebookCase.CaseNumber;
            }
            x.ArresteeCondition = _context.InmatePrebookCondition.Where(w =>  w.InmatePrebookId == inmatePrebookId && !w.DeleteFlag)
            .Select(s => new InmatePreBookCondition
            {
                 InmatePrebookId = s.InmatePrebookId,
                 ArresteeConditionLookupIndex = s.ArresteeConditionLookupIndex,
            }).ToList();
             x.ArresteeBehavior = _context.InmatePrebookBehavior.Where(w => w.InmatePrebookId == inmatePrebookId && !w.DeleteFlag)
            .Select(s => new InmatePreBookCondition
            {
                 InmatePrebookId = s.InmatePrebookId,
                 ArresteeBehaviorLookupIndex = s.ArresteeBehaviorLookupIndex,
            }).ToList();
              x.ArresteeBAC = _context.InmatePrebookBACMethod.Where(w =>  w.InmatePrebookId == inmatePrebookId && !w.DeleteFlag)
            .Select(s => new InmatePreBookCondition
            {
                 InmatePrebookId = s.InmatePrebookId,
                 BACMethodLookupIndex = s.BACMethodLookupIndex,
                 BAC1 = s.BAC1,
                 BAC2 = s.BAC2
            }).ToList();
            return x;
            
        }

        //public IQueryable<InmatePrebook> GetInmatePrebooksDeatils()
        //{
        //    IQueryable<InmatePrebook> dbInmatePrebook =
        //        from ip in _context.InmatePrebook
        //        where ip.ArrestAgency.AgencyArrestingFlag == true &&
        //              (facilityId == 0 || ip.FacilityId == facilityId)
        //              && (isSearch
        //                  ? (ip.IncarcerationId.HasValue || !ip.IncarcerationId.HasValue)
        //                  : !ip.IncarcerationId.HasValue)
        //              && ((courtCommitFlag == 1 || courtCommitFlag == 0)
        //                  ? (!ip.CourtCommitFlag.HasValue || ip.CourtCommitFlag == 0)
        //                  : (ip.CourtCommitFlag == 1))
        //              && (tempHold != 1 || ip.TemporaryHold == tempHold || !ip.TemporaryHold.HasValue)
        //              && (ip.PersonId > 0 || !ip.PersonId.HasValue)
        //              && (deleted == 1 || !ip.DeleteFlag.HasValue || ip.DeleteFlag == 0)
        //              && (arrestingOfficer <= 0 || ip.ArrestingOfficerId == arrestingOfficer)
        //              && (myPreBooks != 1 || ip.PersonnelId == _personnelId || ip.CompleteBy == _personnelId)
        //        select ip;

        //    return dbInmatePrebook;

        //}
        public List<InmatePrebookVm> GetPrebookSearch(GetPrebookSearchVm value)
        {

            string programCaseSiteOption = _context.SiteOptions.SingleOrDefault(so =>
                so.SiteOptionsName == SiteOptionsConstants.MULTIPLEPREBOOKCASE)?.SiteOptionsValue;

            DateTime prebookFrom = !string.IsNullOrEmpty(value.PreBookfromDate)
                ? DateTime.Parse(value.PreBookfromDate, CultureInfo.InvariantCulture)
                : DateTime.MinValue;

            DateTime prebookTo = !string.IsNullOrEmpty(value.PreBooktoDate)
                ? DateTime.Parse(value.PreBooktoDate, CultureInfo.InvariantCulture)
                : DateTime.MinValue;

            DateTime arrestFrom = !string.IsNullOrEmpty(value.ArrestfromDate)
                ? DateTime.Parse(value.ArrestfromDate, CultureInfo.InvariantCulture)
                : DateTime.MinValue;

            DateTime arrestTo = !string.IsNullOrEmpty(value.ArresttoDate)
                ? DateTime.Parse(value.ArresttoDate, CultureInfo.InvariantCulture)
                : DateTime.MinValue;

            DateTime inmateDob = !string.IsNullOrEmpty(value.Dob) ? Convert.ToDateTime(value.Dob) : DateTime.MinValue;
            IQueryable<InmatePrebook> dbInmatePrebook;

            value.TempHoldSearch = value.TempHold == "false" ? 0 : 1;
            value.DeletedSearch = value.Deleted == "false" ? 0 : 1;
            value.MyPreBooksSearch = value.MyPreBooks == "false" ? 0 : 1;
            value.ActiveSearch = value.Active == "false" ? 0 : 1;
            if (!string.IsNullOrEmpty(value.CaseNumber) && value.CourtCommitFlag > 0)
            {
                dbInmatePrebook = !string.IsNullOrEmpty(value.CaseNumber)
                    ? from ip in _context.InmatePrebook
                    where ip.InmatePrebookCase.Any(ipc => ipc.CaseNumber.Contains(value.CaseNumber)) && !ip.IncarcerationId.HasValue
                      select ip
                    : from ip in _context.InmatePrebook
                    where ip.ArrestAgency.AgencyArrestingFlag  &&
                          (value.FacilityId == 0 || ip.FacilityId == value.FacilityId)
                          && (value.IsSearch
                              ? (ip.IncarcerationId.HasValue || !ip.IncarcerationId.HasValue)
                              : !ip.IncarcerationId.HasValue)
                          && ((value.CourtCommitFlag == 1 || value.CourtCommitFlag == 0)
                              ? (!ip.CourtCommitFlag.HasValue || ip.CourtCommitFlag == 0)
                              : (ip.CourtCommitFlag == 1))
                          && (value.TempHoldSearch != 1 || ip.TemporaryHold == value.TempHoldSearch ||
                              !ip.TemporaryHold.HasValue)
                          && (ip.PersonId > 0 || !ip.PersonId.HasValue)
                          && (value.DeletedSearch == 1 || !ip.DeleteFlag.HasValue || ip.DeleteFlag == 0)
                          && (value.ArrestingOfficer <= 0 || ip.ArrestingOfficerId == value.ArrestingOfficer)
                          && (value.MyPreBooksSearch != 1 || ip.PersonnelId == _personnelId ||
                              ip.CompleteBy == _personnelId)
                    select ip;

                //dbInmatePrebook = dbInmatePrebook.Where(ip1 =>
                //    ip1.IncarcerationId.HasValue && ip1.Incarceration.InmateId != ip1.Incarceration.Inmate.InmateId);
            }
            else if (value.FacilitySearch == 1)
            {
                value.FacilityId = 0;
                dbInmatePrebook =
                    from ip in _context.InmatePrebook
                    where ip.ArrestAgency.AgencyArrestingFlag 
                          && value.FacilityId == 0
                          && (value.IsSearch
                              ? (ip.IncarcerationId.HasValue || !ip.IncarcerationId.HasValue)
                              : !ip.IncarcerationId.HasValue)
                          && ((value.CourtCommitFlag == 1 || value.CourtCommitFlag == 0)
                              ? (!ip.CourtCommitFlag.HasValue || ip.CourtCommitFlag == 0)
                              : (ip.CourtCommitFlag == 1))
                          && (value.TempHoldSearch != 1 || ip.TemporaryHold == value.TempHoldSearch ||
                              !ip.TemporaryHold.HasValue)
                          && (!ip.TempHoldId.HasValue || ip.TempHoldId == 0)
                          && (ip.PersonId > 0 || !ip.PersonId.HasValue)
                          && (value.DeletedSearch == 1 || !ip.DeleteFlag.HasValue || ip.DeleteFlag == 0)
                          && (value.ArrestingOfficer <= 0 || ip.ArrestingOfficerId == value.ArrestingOfficer)
                          && (value.MyPreBooksSearch != 1 || ip.PersonnelId == _personnelId ||
                              ip.CompleteBy == _personnelId)
                    select ip;
            }
            else
            {
                dbInmatePrebook =
                    from ip in _context.InmatePrebook
                    where ip.ArrestAgency.AgencyArrestingFlag  &&
                          (value.FacilityId == 0 || ip.FacilityId == value.FacilityId)
                          && (value.IsSearch
                              ? (ip.IncarcerationId.HasValue || !ip.IncarcerationId.HasValue)
                              : !ip.IncarcerationId.HasValue)
                          && ((value.CourtCommitFlag == 1 || value.CourtCommitFlag == 0)
                              ? (!ip.CourtCommitFlag.HasValue || ip.CourtCommitFlag == 0)
                              : (ip.CourtCommitFlag == 1))
                          && (value.TempHoldSearch != 1 || ip.TemporaryHold == value.TempHoldSearch ||
                              !ip.TemporaryHold.HasValue)
                          && (ip.PersonId > 0 || !ip.PersonId.HasValue)
                          && (value.DeletedSearch == 1 || !ip.DeleteFlag.HasValue || ip.DeleteFlag == 0)
                          && (value.ArrestingOfficer <= 0 || ip.ArrestingOfficerId == value.ArrestingOfficer)
                          && (value.MyPreBooksSearch != 1 || ip.PersonnelId == _personnelId ||
                              ip.CompleteBy == _personnelId)
                    select ip;
            }

            if (value.TempHoldSearch == 1)
            {
                dbInmatePrebook = dbInmatePrebook.Where(p => p.TemporaryHold == 1);
            }

            if (prebookFrom != DateTime.MinValue && prebookTo != DateTime.MinValue)
            {
                if (value.CourtCommitFlag > 0)
                {
                    dbInmatePrebook = dbInmatePrebook.Where(ip =>
                        (ip.PrebookDate.HasValue && (ip.PrebookDate.Value >= prebookFrom &&
                                                     ip.PrebookDate.Value <= prebookTo)));

                }
                else
                {
                    dbInmatePrebook = from ip in dbInmatePrebook
                        where ip.PrebookDate.HasValue && ip.PrebookDate.Value.Date >= prebookFrom &&
                              ip.PrebookDate.Value.Date <= prebookTo
                        select ip;
                }
            }

            if (arrestFrom != DateTime.MinValue && arrestTo != DateTime.MinValue)
            {
                if (value.CourtCommitFlag > 0)
                {
                    dbInmatePrebook = dbInmatePrebook.Where(ip => ip.ArrestDate.Value >= arrestFrom &&
                                                                  ip.ArrestDate.Value <= arrestTo);
                }
                else
                {
                    dbInmatePrebook = from ip in dbInmatePrebook
                        where ip.ArrestDate.HasValue
                              && ip.ArrestDate.Value >= arrestFrom.Date
                              && ip.ArrestDate.Value <= arrestTo.Date
                        select ip;
                }
            }

            if (value.ActiveSearch == 1)
            {
                dbInmatePrebook = from ip in dbInmatePrebook
                    where value.ActiveSearch != 1 || (!ip.IncarcerationId.HasValue || ip.IncarcerationId == 0)
                          && (!ip.TempHoldId.HasValue || ip.TempHoldId == 0)
                          && (value.DeletedSearch == 1 || (!ip.DeleteFlag.HasValue || ip.DeleteFlag == 0))
                    select ip;
            }

            if (!string.IsNullOrEmpty(value.CaseNumber) && value.CourtCommitFlag == 0)
            {
                
                dbInmatePrebook = !string.IsNullOrEmpty(value.CaseNumber)
                    ? from ip in dbInmatePrebook
                      where ip.InmatePrebookCase.Any(ipc => ipc.CaseNumber.Contains(value.CaseNumber)) && !ip.IncarcerationId.HasValue
                    select ip
                    : from ip in dbInmatePrebook
                      where ip.ArrestAgency.AgencyArrestingFlag  &&
                          (value.FacilityId == 0 || ip.FacilityId == value.FacilityId)
                          && (value.IsSearch
                              ? (ip.IncarcerationId.HasValue || !ip.IncarcerationId.HasValue)
                              : !ip.IncarcerationId.HasValue)
                          && ((value.CourtCommitFlag == 1 || value.CourtCommitFlag == 0)
                              ? (!ip.CourtCommitFlag.HasValue || ip.CourtCommitFlag == 0)
                              : (ip.CourtCommitFlag == 1))
                          && (value.TempHoldSearch != 1 || ip.TemporaryHold == value.TempHoldSearch ||
                              !ip.TemporaryHold.HasValue)
                          && (ip.PersonId > 0 || !ip.PersonId.HasValue)
                          && (value.DeletedSearch == 1 || !ip.DeleteFlag.HasValue || ip.DeleteFlag == 0)
                          && (value.ArrestingOfficer <= 0 || ip.ArrestingOfficerId == value.ArrestingOfficer)
                          && (value.MyPreBooksSearch != 1 || ip.PersonnelId == _personnelId ||
                              ip.CompleteBy == _personnelId)
                    select ip;
            }

            if (!string.IsNullOrEmpty(value.PersonSuffix))
            {
                dbInmatePrebook = from ip in dbInmatePrebook
                    where ip.PersonSuffix == value.PersonSuffix
                    select ip;
            }

            if (value.ArrestingAgencyId > 0)
            {
                dbInmatePrebook = from ip in dbInmatePrebook
                    where ip.ArrestAgencyId == value.ArrestingAgencyId
                    select ip;
            }

            if (value.TransOfficerId > 0)
            {
                dbInmatePrebook = from ip in dbInmatePrebook
                    where ip.TransportingOfficerId == value.TransOfficerId
                    select ip;
            }

            List<InmatePrebookVm> inmatePrebooks =
                (from ip in dbInmatePrebook
                    where (!ip.PersonId.HasValue || ip.PersonId == 0) &&
                          (string.IsNullOrEmpty(value.PersonLastName) ||
                           ip.PersonLastName.StartsWith(value.PersonLastName))
                          && (string.IsNullOrEmpty(value.PersonFirstName) ||
                              ip.PersonFirstName.StartsWith(value.PersonFirstName))
                          //    && (string.IsNullOrEmpty(personSuffix) || ip.PersonFirstName.StartsWith(personSuffix))
                          && (string.IsNullOrEmpty(value.ArrestOfficerName) ||
                              ip.ArrestOfficerName.StartsWith(value.ArrestOfficerName))
                          && (string.IsNullOrEmpty(value.TransportOfficerName) ||
                              ip.TransportOfficerName.StartsWith(value.TransportOfficerName))
                          && (string.IsNullOrEmpty(value.ArrestLocation) ||
                              ip.ArrestLocation.StartsWith(value.ArrestLocation))
                          && (inmateDob == DateTime.MinValue || ip.Person.PersonDob.HasValue &&
                              ip.Person.PersonDob.Value.Date == inmateDob.Date)
                    select new InmatePrebookVm
                    {
                        InmatePrebookId = ip.InmatePrebookId,
                        PersonLastName = ip.PersonLastName,
                        PersonFirstName = ip.PersonFirstName,
                        PersonMiddleName = ip.PersonMiddleName,
                        PersonSuffix = ip.PersonSuffix,
                        PersonDob = ip.PersonDob,
                        PrebookNumber = ip.PreBookNumber,
                        ArrestDate = ip.ArrestDate,
                        PrebookDate = ip.PrebookDate,
                        PrebookComplete = ip.CompleteFlag == 1,
                        DeleteFlag = ip.DeleteFlag == 1,
                        TemporaryHold = ip.TemporaryHold == 1,
                        PersonId = ip.PersonId,
                        CaseNumber = string.Join(",",
                            ip.InmatePrebookCase.Where(ipc => ipc.InmatePrebookId == ip.InmatePrebookId)
                                .Select(ipc => ipc.CaseNumber)),
                        PrebookNotes = ip.PrebookNotes,
                        //LastStep = ip.AppAOWizardFixedStepsId == null || ip.AppAOWizardFixedStepsId == 110
                        //    ? new WizardStepVm
                        //    {
                        //        StepId = 120,
                        //        StepName = "PRE-BOOK INFO",
                        //       StepOrder = 1
                        //    }
                        //    : new WizardStepVm
                        //    {
                        //        StepId = ip.AppAoWizardFixedSteps.AppAoWizardFixedStepsId,
                        //        StepName = ip.AppAoWizardFixedSteps.StepName,
                        //        StepOrder = ip.AppAoWizardFixedSteps.StepDisplayOrder
                        //    },
                        CourtCommitFlag = ip.CourtCommitFlag == 1,
                        ArrestOfficerName = ip.ArrestOfficerName,
                        ArrestOfficerId=ip.ArrestingOfficerId,
                        ArrestAgencyId = ip.ArrestAgency.AgencyId,
                        ArrestAgencyAbbr = ip.ArrestAgency.AgencyAbbreviation,
                        ArrestAgencyName = ip.ArrestAgency.AgencyName,
                        FacilityAbbr = ip.Facility.FacilityAbbr,
                        FacilityId = ip.Facility.FacilityId,
                        WizardFixedStepsId = ip.AppAoWizardFixedStepsId ?? 0,
                        ArrestingOfficer = new PersonnelVm
                        {
                            PersonLastName = ip.ArrestingOfficer.PersonNavigation.PersonLastName,
                            PersonFirstName = ip.ArrestingOfficer.PersonNavigation.PersonFirstName,
                            OfficerBadgeNumber = ip.ArrestingOfficer.OfficerBadgeNum
                        },
                        CreateBy = ip.CreateBy
                    }).ToList();

            inmatePrebooks.AddRange((from ip in dbInmatePrebook
                where ip.PersonId > 0 &&
                      (string.IsNullOrEmpty(value.PersonLastName) ||
                       ip.PersonLastName.StartsWith(value.PersonLastName))
                      && (string.IsNullOrEmpty(value.PersonFirstName) ||
                          ip.PersonFirstName.StartsWith(value.PersonFirstName))
                      //    && (string.IsNullOrEmpty(personSuffix) || ip.PersonFirstName.StartsWith(personSuffix))
                      && (string.IsNullOrEmpty(value.ArrestOfficerName) ||
                          ip.ArrestOfficerName.StartsWith(value.ArrestOfficerName))
                      && (string.IsNullOrEmpty(value.TransportOfficerName) ||
                          ip.TransportOfficerName.StartsWith(value.TransportOfficerName))
                      && (string.IsNullOrEmpty(value.ArrestLocation) ||
                          ip.ArrestLocation.StartsWith(value.ArrestLocation))
                      && (inmateDob == DateTime.MinValue || ip.Person.PersonDob.HasValue &&
                          ip.Person.PersonDob.Value.Date == inmateDob.Date)
                select new InmatePrebookVm
                {
                    InmatePrebookId = ip.InmatePrebookId,
                    PersonId = ip.PersonId ?? 0,
                    PersonLastName = ip.Person.PersonLastName,
                    PersonFirstName = ip.Person.PersonFirstName,
                    PersonMiddleName = ip.Person.PersonMiddleName,
                    PersonSuffix = ip.Person.PersonSuffix,
                    PersonDob = ip.Person.PersonDob,
                    PrebookNumber = ip.PreBookNumber,
                    ArrestDate = ip.ArrestDate,
                    PrebookDate = ip.PrebookDate,
                    PrebookComplete = ip.CompleteFlag == 1,
                    DeleteFlag = ip.DeleteFlag == 1,
                    TemporaryHold = ip.TemporaryHold == 1,
                    CaseNumber = ip.CourtCommitFlag == 0
                        ? programCaseSiteOption == SiteOptionsConstants.OFF
                            ? ip.InmatePrebookCase.OrderByDescending(i => i.InmatePrebookCaseId)
                                .FirstOrDefault(i => i.InmatePrebookId == ip.InmatePrebookId)
                                .CaseNumber
                            : string.Join(",",
                                ip.InmatePrebookCase.Where(ipc =>
                                        ipc.InmatePrebookId == ip.InmatePrebookId)
                                    .Select(ipc => ipc.CaseNumber))
                        : ip.CaseNumber,
                    PrebookNotes = ip.PrebookNotes,
                    //LastStep = ip.AppAOWizardFixedStepsId == null || ip.AppAOWizardFixedStepsId == 110
                    //   ? new WizardStepVm
                    //   {
                    //       StepId = 120,
                    //       StepName = "PRE-BOOK INFO",
                    //       StepOrder = 1
                    //   }
                    //   : new WizardStepVm
                    //   {
                    //       StepId = ip.AppAoWizardFixedSteps.AppAoWizardFixedStepsId,
                    //       StepName = ip.AppAoWizardFixedSteps.StepName,
                    //       StepOrder = ip.AppAoWizardFixedSteps.StepDisplayOrder
                    //   },
                    CourtCommitFlag = ip.CourtCommitFlag == 1,
                    ArrestOfficerName = ip.ArrestOfficerName,
                    ArrestOfficerId=ip.ArrestingOfficerId,
                  //  Arre
                    ArrestAgencyId = ip.ArrestAgency.AgencyId,
                    ArrestAgencyAbbr = ip.ArrestAgency.AgencyAbbreviation,
                    ArrestAgencyName = ip.ArrestAgency.AgencyName,
                    FacilityAbbr = ip.Facility.FacilityAbbr,
                    FacilityId = ip.Facility.FacilityId,
                    WizardFixedStepsId = ip.AppAoWizardFixedStepsId ?? 0,
                    ArrestingOfficer = new PersonnelVm
                    {
                        PersonLastName = ip.ArrestingOfficer.PersonNavigation.PersonLastName,
                        PersonFirstName = ip.ArrestingOfficer.PersonNavigation.PersonFirstName,
                        OfficerBadgeNumber = ip.ArrestingOfficer.OfficerBadgeNum
                    },
                    CreateBy = ip.CreateBy
                }).ToList());

            int[] inmatePrebookIds = inmatePrebooks.Select(ip => ip.InmatePrebookId).ToArray();
            List<KeyValuePair<int, int>> prebookIdAndWizardStepIds = _context.AoWizardProgressInmatePrebook
                .Where(aw => inmatePrebookIds.Contains(aw.InmatePrebookId))
                .Select(awp => new KeyValuePair<int, int>
                (
                    awp.InmatePrebookId,
                    awp.AoWizardProgressId
                )).ToList();

            var wizardStepProgress = _context.AoWizardStepProgress.Where(aws =>
                    prebookIdAndWizardStepIds.Select(p => p.Value).Contains(aws.AoWizardProgressId))
                .Select(a => new
                {
                    InmatePrebookId = prebookIdAndWizardStepIds.SingleOrDefault(p => p.Value == a.AoWizardProgressId)
                        .Key,
                    ComponentId = a.AoComponentId,
                    a.StepComplete
                }).ToList();

            inmatePrebooks.ForEach(item =>
            {
                item.WizardStepProgress = wizardStepProgress.Where(aw => aw.InmatePrebookId == item.InmatePrebookId)
                    .Select(s => new AoWizardStepProgressVm
                    {
                        ComponentId = s.ComponentId,
                        StepComplete = s.StepComplete,
                    }).ToList();
            });

            if (value.CourtCommitFlag == 1)
            {
                inmatePrebooks = inmatePrebooks.Where(w => w.PrebookComplete).Select(s => s).OrderBy(s => s.FacilityId)
                    .ToList();
            }
            else if(value.CourtCommitFlag == 2)
            {
                inmatePrebooks = inmatePrebooks.Where(w => w.PrebookComplete && w.CourtCommitFlag).Select(s => s).OrderBy(s => s.FacilityId)
                    .ToList();
            }

            return inmatePrebooks;
        }
        
        public async Task<int> DeletePrebook(int inmatePrebookId)
        {
            if (inmatePrebookId <= 0)
            {
                return 0;
            }

            InmatePrebook inmatePrebook =
                _context.InmatePrebook.SingleOrDefault(ap => ap.InmatePrebookId == inmatePrebookId);
            if (inmatePrebook != null)
            {
                inmatePrebook.DeleteFlag = 1;
                inmatePrebook.DeleteDate = DateTime.Now;
                inmatePrebook.DeletedBy = 1;
            }

            var res = await _context.SaveChangesAsync();
            await _atimsHubService.GetIntakePrebookCount();
            return res;
        }

        public async Task<int> UndoDeletePrebook(int inmatePrebookId)
        {
            if (inmatePrebookId <= 0)
            {
                return 0;
            }

            InmatePrebook inmatePrebook =
                _context.InmatePrebook.SingleOrDefault(ap => ap.InmatePrebookId == inmatePrebookId);
            if (inmatePrebook != null)
            {
                inmatePrebook.DeleteFlag = null;
                inmatePrebook.DeleteDate = null;
                inmatePrebook.DeletedBy = null;
            }

            var res = await _context.SaveChangesAsync();
            await _atimsHubService.GetIntakePrebookCount();
            return res;
        }

        public void InsertPrebookPerson(int inmatePrebookId, int personId)
        {
            //what part of this method inserts the prebook person??
            InmatePrebook inmatePrebook = _context.InmatePrebook.Single(ap => ap.InmatePrebookId == inmatePrebookId);
            inmatePrebook.PersonId = personId;

            if (personId >= 0)
            {
                // delete functionality
                FormBookmark objformbookmark =
                    _context.FormBookmark.SingleOrDefault(fb =>
                        !fb.InmatePrebookId.HasValue && fb.PersonId == personId);
                if (objformbookmark != null)
                {
                    _context.FormBookmark.Remove(objformbookmark);
                }

                // update functionality
                FormBookmark inmateBookmark =
                    _context.FormBookmark.SingleOrDefault(ap => ap.InmatePrebookId == inmatePrebookId);
                if (objformbookmark != null)
                {
                    if (inmateBookmark != null) inmateBookmark.PersonId = personId;
                }
            }
        }

        // check the validation for the pre book
        public IntakePrebookSelectVm LoadPrebookDetails(int personId)
        {
            int inmatePrebookId = 0;
            List<InmatePrebook> inmatePrebookDetails = _context.InmatePrebook.Where(ip =>
                ip.PersonId == personId && (!ip.IncarcerationId.HasValue || ip.IncarcerationId == 0)
                                        && (!ip.DeleteFlag.HasValue || ip.DeleteFlag == 0) &&
                                        (ip.TempHoldId == 0 || !ip.TempHoldId.HasValue)).ToList();

            if (inmatePrebookDetails.Count > 0)
            {
                inmatePrebookId = inmatePrebookDetails.Select(s => s.InmatePrebookId).Single();
            }

            int courtCommit = inmatePrebookDetails.Select(s => s.CourtCommitFlag ?? 0).SingleOrDefault();

            int value = inmatePrebookDetails
                .Count(ip => (ip.CourtCommitFlag == 0 || !ip.CourtCommitFlag.HasValue) &&
                             (!ip.TempHoldId.HasValue || (ip.TempHoldId == 0 ||
                                                          (ip.TempHoldId > 0 &&
                                                           (ip.TempHold
                                                                .TempHoldCompleteFlag ==
                                                            0 ||
                                                            !ip.TempHold
                                                                .TempHoldCompleteFlag
                                                                .HasValue)))));

            // int? courtCommit = inmatePrebookDetails.Select(s => s.CourtCommitFlag).FirstOrDefault();

            if (value > 0)
            {
                _facilityAbbr = _context.Facility
                    .SingleOrDefault(f =>
                        f.FacilityId == inmatePrebookDetails.Select(s => s.FacilityId).FirstOrDefault())
                    ?.FacilityAbbr;
            }
            else if (value == 0 && courtCommit == 1)
            {
                value = 0;
            }
            else
            {
                value = -1;
            }

            IntakePrebookSelectVm lstIntakePrebookSelectVm = new IntakePrebookSelectVm
            {
                FacilityValueDetail = new KeyValuePair<int, string>(value, _facilityAbbr),
                InmatePrebookId = inmatePrebookId
            };

            return lstIntakePrebookSelectVm;
        }


        public PrebookValidateConfirmData GetPrebookValidateConfirm(int personId)
        {
            PrebookValidateConfirmData prebookValidationConfirmVm = new PrebookValidateConfirmData();

            SqlConnection connection = new SqlConnection(Startup.ConnectionString);
            connection.Open();
            SqlCommand command = new SqlCommand("sp_GetPrebookValidateConfirm", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.Parameters.AddWithValue("@PersonId", personId);
            var resultTable = new DataTable();
            var adapter = new SqlDataAdapter(command);
            adapter.Fill(resultTable);
            connection.Close();

            prebookValidationConfirmVm.PrebookValidateConfirmRecords = (from DataRow r in resultTable.Rows
                select new PrebookValidateConfirmRecord
                {
                    FormRecordId = Int32.Parse(r["FormRecordId"].ToString()),
                    InmatePrebookId = Int32.Parse(r["InmatePrebookId"].ToString()),
                    TagName = r["TagName"].ToString(),
                    FieldMapping = r["FieldMapping"].ToString(),
                    FieldOrder = Int32.Parse(r["FieldOrder"].ToString()),
                    Entered = r["Entered"].ToString(),
                    StoredValue = r["StoredValue"].ToString(),
                    //LookupOptions = r["LookupOptions"].ToString(),
                    ControlType = r["ControlType"].ToString()
                }).ToList();
            return prebookValidationConfirmVm;
        }

        public bool UpdatePrebookValidateConfirm(string values, int personId)
        {
            SqlConnection connection = new SqlConnection(Startup.ConnectionString);
            connection.Open();
            SqlCommand command = new SqlCommand("sp_UpdatePrebookValidateConfirm", connection)
            {
                CommandType = CommandType.StoredProcedure
            };
            command.Parameters.AddWithValue("@PersonId", personId);
            command.Parameters.AddWithValue("@Values", values);
            command.Parameters.AddWithValue("@CreatedBy", _personnelId);
            command.Parameters.Add("@ErrorMessage", SqlDbType.Char, 150);
            command.Parameters["@ErrorMessage"].Direction = ParameterDirection.Output;
            command.Parameters.Add("@IsSuccess", SqlDbType.Bit);
            command.Parameters["@IsSuccess"].Direction = ParameterDirection.Output;
            command.ExecuteNonQuery();
            bool isSuccess = (bool)command.Parameters["@IsSuccess"].Value;
            connection.Close();
            return isSuccess;
        }
    }
}