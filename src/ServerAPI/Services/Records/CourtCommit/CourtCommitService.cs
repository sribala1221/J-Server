using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GenerateTables.Models;
using Microsoft.AspNetCore.Http;
using ServerAPI.Utilities;
using ServerAPI.ViewModels;

namespace ServerAPI.Services
{
    public class CourtCommitService : ICourtCommitService
    {
        private readonly int _personnelId;
        private readonly AAtims _context;
        private readonly ICommonService _commonService;
        private readonly IWizardService _wizardService;
        private readonly int _facilityId;

        public CourtCommitService(AAtims context, IHttpContextAccessor httpContextAccessor,
            ICommonService commonService, IWizardService wizardService)
        {
            _context = context;
            _commonService = commonService;
            _wizardService = wizardService;
            string facilityId = httpContextAccessor.HttpContext.User.FindFirst(AuthDetailConstants.FACILITYID)?.Value;
            if (!string.IsNullOrEmpty(facilityId))
                _facilityId = Convert.ToInt32(facilityId);
            _personnelId = Convert.ToInt32(httpContextAccessor.HttpContext.User
                .FindFirst(AuthDetailConstants.PERSONNELID)?.Value);
        }

        public List<CourtCommitHistoryVm> CourtCommitHistoryDetails(CourtCommitHistorySearchVm searchvalue)
        {
            IQueryable<InmatePrebook> inmatePrebookData = _context.InmatePrebook.Where(w =>
                (searchvalue.FacilityId <= 0 || w.FacilityId == searchvalue.FacilityId) &&
                (!w.TempHoldId.HasValue || w.TempHoldId == 0) && w.ArrestAgency.AgencyArrestingFlag &&
                w.CourtCommitFlag == 1 && (!w.IncarcerationId.HasValue || w.IncarcerationId == 0));

            if (searchvalue.ActiveFlag)
            {
                inmatePrebookData = inmatePrebookData.Where(w => !w.DeleteFlag.HasValue || w.DeleteFlag == 0);
            }

            if (searchvalue.CompleteFlag)
            {
                inmatePrebookData = inmatePrebookData.Where(w =>!w.CompleteFlag.HasValue || w.CompleteFlag == 1);
            }

            if (searchvalue.OfficerId > 0)
            {
                inmatePrebookData = inmatePrebookData.Where(w => w.ArrestingOfficerId == searchvalue.OfficerId);
            }

            if (searchvalue.InterviewOfficerId > 0)
            {
                inmatePrebookData = inmatePrebookData.Where(w => w.SchedInterviewBy == searchvalue.InterviewOfficerId);
            }

            if (searchvalue.FromDate != null && searchvalue.ToDate != null)
            {
                inmatePrebookData = inmatePrebookData.Where(w =>
                    w.PrebookDate >= searchvalue.FromDate && w.PrebookDate <= searchvalue.ToDate);
            }

            if (searchvalue.InterviewFromDate != null && searchvalue.InterviewToDate != null)
            {
                inmatePrebookData = inmatePrebookData.Where(w =>
                    w.SchedInterviewDate >= searchvalue.InterviewFromDate &&
                    w.SchedInterviewDate <= searchvalue.InterviewToDate);
            }

            if (!string.IsNullOrEmpty(searchvalue.LastName))
            {
                inmatePrebookData = inmatePrebookData.Where(w => !w.PersonId.HasValue
                        ? w.PersonLastName.StartsWith(searchvalue.LastName)
                        : w.Person.PersonLastName.StartsWith(searchvalue.LastName));
            }

            if (!string.IsNullOrEmpty(searchvalue.FirstName))
            {
                inmatePrebookData = inmatePrebookData.Where(w =>
                    !w.PersonId.HasValue
                        ? w.PersonFirstName.StartsWith(searchvalue.FirstName)
                        : w.Person.PersonFirstName.StartsWith(searchvalue.FirstName));
            }

            if (!string.IsNullOrEmpty(searchvalue.CommitStatus))
            {
                inmatePrebookData = inmatePrebookData.Where(w =>
                    !w.PersonId.HasValue
                        ? w.PersonLastName.StartsWith(searchvalue.LastName)
                        : w.Person.PersonLastName.StartsWith(searchvalue.LastName));
            }

            if (searchvalue.Dob != null)
            {
                inmatePrebookData = inmatePrebookData.Where(w =>
                    !w.PersonId.HasValue ? w.PersonDob == searchvalue.Dob : w.Person.PersonDob == searchvalue.Dob);
            }

            if (!string.IsNullOrEmpty(searchvalue.CommitType))
            {
                inmatePrebookData = inmatePrebookData.Where(w => w.CourtCommitType.StartsWith(searchvalue.CommitType));
            }

            if (searchvalue.ScheduleDate != null)
            {
                inmatePrebookData = inmatePrebookData.Where(w => w.PrebookDate == searchvalue.ScheduleDate);
            }

            List<CourtCommitHistoryVm> prebookResult = inmatePrebookData.Select(s => new CourtCommitHistoryVm
            {
                InmateActive = s.Person.Inmate.FirstOrDefault().InmateActive == 1,
                BookedDate = s.SchedBookDate,
                CompleteFlag = s.CompleteFlag == 1,
                PrebookDate = s.PrebookDate,
                DeletedFlag = s.DeleteFlag == 1,
                Dob = !s.PersonId.HasValue ? s.PersonDob : s.Person.PersonDob,
                FacilityId = s.FacilityId,
                InmateId = s.Person.Inmate.FirstOrDefault().InmateId,
                TempHold = s.TemporaryHold == 1,
                InmateName = new PersonInfo
                {
                    PersonLastName = !s.PersonId.HasValue ? s.PersonLastName : s.Person.PersonLastName,
                    PersonFirstName = !s.PersonId.HasValue ? s.PersonFirstName : s.Person.PersonFirstName,
                    PersonMiddleName = !s.PersonId.HasValue ? s.PersonMiddleName : s.Person.PersonMiddleName,
                    InmateNumber = !s.PersonId.HasValue ? string.Empty : s.Person.Inmate.FirstOrDefault().InmateNumber,
                },
                Officer = new PersonnelVm
                {
                    OfficerBadgeNumber = s.ArrestingOfficer.OfficerBadgeNum,
                    PersonLastName = s.ArrestingOfficer.PersonNavigation.PersonLastName,
                    PersonFirstName = s.ArrestingOfficer.PersonNavigation.PersonFirstName,
                },
                InmatePrebookId = s.InmatePrebookId,
                PersonId = s.PersonId ?? 0,
                PersonnelId = s.PersonnelId,
                WizardLastStepId = s.PrebookWizardLastStepId,
                SchedBookDate = s.SchedBookDate,
                ArrestingOfficerId = s.ArrestingOfficerId,
                MedPrescreenStartFlag = s.MedPrescreenStartFlag ?? 0,
                MedPrescreenStatusFlag = s.MedPrescreenStatusFlag ?? 0,
                DocketNumber = !string.IsNullOrEmpty(s.ArrestCourtDocket) 
                                    ? s.ArrestCourtDocket : "",
                PreBookNotes = s.PrebookNotes,
                FacilityName = s.Facility.FacilityAbbr,
                AgencyAbbreviation = s.ArrestAgency.AgencyAbbreviation,
                CaseNumber = string.Join(",", s.InmatePrebookCase.Where(ipc => ipc.InmatePrebookId == s.InmatePrebookId)
                                    .Select(ipc => ipc.CaseNumber)),
                ArresstDate = s.ArrestDate,
                ArrestId = s.ArrestId ?? 0,
                IncarcerationId = s.IncarcerationId,
                PrebookNumber = s.PreBookNumber,
                AoWizardProgressId = s.AoWizardProgressInmatePrebook.FirstOrDefault().AoWizardProgressId,
            }).ToList();

            if (!string.IsNullOrEmpty(searchvalue.CaseNumber))
            {
                prebookResult = prebookResult.Where(w =>
                    (w.CaseNumber.Contains(searchvalue.CaseNumber) ||
                        w.DocketNumber.Contains(searchvalue.CaseNumber)))
                    .ToList();
            }

            List<int> inmatePrebookIds = prebookResult.Select(s => s.InmatePrebookId).ToList();
            List<int> personIds = prebookResult.Select(w =>w.PersonId).ToList();
            List<int> inmateIds =  _context.Inmate.Where(w => personIds.Contains(w.PersonId)).Select(s => s.InmateId).ToList();

            List<KeyValuePair<int,DateTime?>> incarcerationValue = _context.Incarceration.Where(w=> inmateIds.Contains(w.InmateId??0)).
            Select(s => new KeyValuePair<int, DateTime?>(s.Inmate.PersonId,s.OverallFinalReleaseDate)).ToList();

            IQueryable<FormRecord> fromRecordPrebook = _context.FormRecord.Where(w =>
                w.FormTemplates.FormCategoryId == 2 &&
                inmatePrebookIds.Contains(w.InmatePrebookId ?? 0) && w.DeleteFlag == 0 && w.XmlData != null);
            List<InmatePrebookCase> lstInmatePrebookCases = _context.InmatePrebookCase.ToList();

            prebookResult.ForEach(value =>
            {
                value.OverAllFinalReleaseDate = incarcerationValue.FirstOrDefault(w=> w.Key == value.PersonId).Value;
                value.FormCount = fromRecordPrebook.Count(c => c.InmatePrebookId == value.InmatePrebookId);
                if (value.AoWizardProgressId.HasValue)
                {
                    value.WizardProgress = _context.AoWizardStepProgress
                        .Where(a => a.AoWizardProgressId == value.AoWizardProgressId).Select(s => new WizardStep
                        {
                            ComponentId = s.AoComponentId,
                            StepComplete = s.StepComplete,
                            WizardProgressId = s.AoWizardProgressId,
                            WizardStepProgressId = s.AoWizardStepProgressId
                        }).ToList();
                }
                if (lstInmatePrebookCases.Any(i => i.InmatePrebookId == value.InmatePrebookId))
                {
                    value.InmatePrebookCaseId = lstInmatePrebookCases.OrderByDescending(i => i.InmatePrebookCaseId)
                            .First(i => i.InmatePrebookId == value.InmatePrebookId).InmatePrebookCaseId;
                }
            });
            return prebookResult;
        }

        public async Task<int> CourtCommitUpdateDelete(int inmatePreBookId)
        {
            InmatePrebook value = _context.InmatePrebook.Single(s => s.InmatePrebookId == inmatePreBookId);
            value.DeleteFlag = value.DeleteFlag == 1 ? 0 : 1;
            value.DeletedBy = _personnelId;
            value.DeleteDate = DateTime.Now;
            return await _context.SaveChangesAsync();
        }

        private List<AgencyDetailsVm> AgencyDetails(bool checkFlag = false) => _context.Agency.Where(w =>
            (!w.AgencyInactiveFlag.HasValue || w.AgencyInactiveFlag == 0) &&
            (checkFlag && w.AgencyCourtFlag)
        ).Select(s => new AgencyDetailsVm
        {
            AgencyId = s.AgencyId,
            AgencyCourtFlag = s.AgencyCourtFlag,
            AgencyName = s.AgencyName
        }).ToList();

        //wizard steps
        public AoWizardFacilityVm GetIncidentWizard()
        {
            AoWizardVm wizardDetails = _wizardService.GetWizardSteps(10)[0];
            return wizardDetails.WizardFacilities.SingleOrDefault(wf => wf.Facility?.FacilityId == _facilityId);
        }

        //Court Commit Sent Details
        public CourtCommitAgencyVm CourtCommitSentDetails(int incarcerationId, int arrestId, int inmateId,
            int inmatePrebookId)
        {
            List<CourtCommitSentEntryVm> prebookDetails = _context.InmatePrebook
                .Where(w => w.InmatePrebookId == inmatePrebookId).Select(
                    s => new CourtCommitSentEntryVm
                    {
                        ArrestSentenceFineAmount = s.ArrestSentenceFineAmount,
                        ArrestSentenceFinePaid = s.ArrestSentenceFinePaid,
                        ArrestSentenceFineType = s.ArrestSentenceFineType,
                        ArrestSentenceFinePerDay = s.ArrestSentenceFinePerDay,
                        ArrestSentenceFineDays = s.ArrestSentenceFineDays,
                        ArrestCourtJurisdictionId = s.ArrestCourtJurisdictionId
                    }).ToList();

            List<LookupVm> lookupDetails = _commonService.GetLookups(new[]
            {
                LookupConstants.ARRTYPE, LookupConstants.SENTTYPE,
                LookupConstants.SENTFIND, LookupConstants.SENTDURATION,
                LookupConstants.SENTFINETYPE
            });
            prebookDetails.ForEach(item =>
            {
                item.Court = AgencyDetails().Find(f => f.AgencyId == (int?)(item.ArrestCourtJurisdictionId))
                    .AgencyName;
                item.ArrestSentenceFineTypeid = lookupDetails.First(f => f.LookupType == LookupConstants.SENTFINETYPE &&
                    f.LookupDescription == item.ArrestSentenceFineType).LookupIndex;
            });

            return new CourtCommitAgencyVm
            {
                //Agency Details
                AgencyDetails = AgencyDetails(true),

                //Lookup Details
                LookupDetails = lookupDetails,

                //Booking Details
                CourtCommitBooking = _context.IncarcerationArrestXref.Where(w =>
                        w.Incarceration.IncarcerationId == incarcerationId &&
                        (arrestId > 0 && w.Arrest.ArrestId != arrestId &&
                         w.Arrest.ArrestSentenceConsecutiveTo != arrestId))
                    .Select(s => new CourtCommitBookingVm
                    {
                        ArrestBookingNo = s.Arrest.ArrestBookingNo,
                        ArrestCourtDocket = s.Arrest.ArrestCourtDocket,
                        ArrestId = s.Arrest.ArrestId
                    }).ToList(),

                //Judge Details
                PersonnelDetails = _context.Personnel
                    .Where(w => w.PersonnelJudgeFlag).Select(s =>
                        new PersonnelVm
                        {
                            OfficerBadgeNumber = s.OfficerBadgeNum,
                            PersonLastName = s.PersonNavigation.PersonLastName,
                            PersonFirstName = s.PersonNavigation.PersonFirstName,
                            PersonnelId = s.PersonnelId
                        }).ToList(),

                //Inmate Details
                InmateDetails = _context.Inmate.Where(w => w.InmateId == inmateId).Select(s => new PersonVm
                {
                    PersonFirstName = s.Person.PersonFirstName,
                    PersonLastName = s.Person.PersonLastName,
                    InmateNumber = s.InmateNumber
                }).SingleOrDefault(),

                PrebookDetails = prebookDetails
            };
        }

        //wizard Complete step
        public async Task<int> WizardComplete(CourtCommitHistoryVm courtCommitVm)
        {
            InmatePrebook value = _context.InmatePrebook.Single(s => s.InmatePrebookId == courtCommitVm.InmatePrebookId);
            value.CompleteFlag = courtCommitVm.CompleteFlag ? 1 : (int?)null;
            if (courtCommitVm.CompleteFlag)
            {
                value.CompleteDate = DateTime.Now;
                value.CompleteBy = _personnelId;
            }
            return await _context.SaveChangesAsync();
        }
    }
}