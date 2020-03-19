using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using ServerAPI.ViewModels;
using GenerateTables.Models;
using ServerAPI.Utilities;

namespace ServerAPI.Services
{
    public class TempHoldService : ITempHoldService
    {
        private readonly AAtims _context;
        private readonly int _personnelId;
        private readonly ICommonService _commonService;

        public TempHoldService(AAtims context, ICommonService commonService, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _commonService = commonService;
            _personnelId = Convert.ToInt32(httpContextAccessor.HttpContext.User
                .FindFirst(AuthDetailConstants.PERSONNELID)?.Value);
        }

        public IntakeTempHoldVm GetIntakeTempHoldDetails(PersonnelSearchVm personnelSearchVm)
        {
            personnelSearchVm.PersonDetail = new PersonVm();
            personnelSearchVm.PersonActive = true;
            IntakeTempHoldVm intakeTempHoldVm = new IntakeTempHoldVm
            {
                //To get Personnel details
                LstPersonnel = _commonService.GetPersonnelSearchDetails(personnelSearchVm),

                //To get Temp Hold Type details
                LstTempHoldType = GetTempHoldType(),

                //To get Temp Hold Location details
                LstTempHoldLocation = GetTempHoldLocation()
            };

            return intakeTempHoldVm;
        }

        public TempHoldCompleteStepLookup GetTempHoldCompleteStepLookup(int tempHoldId)
        {
            //get temp hold complete step lookup details
            TempHoldCompleteStepLookup tempHoldCompleteStepLookup = new TempHoldCompleteStepLookup
            {
                ListType = GetTempHoldType(),
                ListLocation = GetTempHoldLocation(),
                ListDisposition = _commonService.GetLookupList(LookupConstants.TEMPHOLDDISPO)
                    .Select(s => new KeyValuePair<int, string>
                        (Convert.ToInt32(s.LookupIndex), s.LookupDescription)).ToList(),
                PrebookComplete = GetTempHoldPrebookCompleteDetails(tempHoldId)
            };
            return tempHoldCompleteStepLookup;
        }

        private List<KeyValuePair<int, string>> GetTempHoldType() => _commonService
            .GetLookupList(LookupConstants.TEMPHOLDTYPE)
            .Select(s => new KeyValuePair<int, string>
                (s.LookupIndex, s.LookupDescription)).ToList();

        private List<KeyValuePair<int, string>> GetTempHoldLocation() => _context.Privileges
            .Where(p => p.InactiveFlag == 0 && p.HoldLocation == 1)
            .OrderBy(o => o.PrivilegeDescription)
            .Select(s => new KeyValuePair<int, string>(s.PrivilegeId, s.PrivilegeDescription))
            .ToList();

        private PrebookCompleteVm GetTempHoldPrebookCompleteDetails(int tempHoldId) =>
            _context.TempHold.Where(w => w.TempHoldId == tempHoldId)
                .Select(s => new PrebookCompleteVm
                {
                    TempHoldTypeId = s.TempHoldType,
                    TempHoldLocationId = s.TempHoldLocationId,
                    TempHoldNote = s.TempHoldNote,
                    TempHoldCompleteFlag = s.TempHoldCompleteFlag,
                    TempHoldDisposition = s.TempHoldDisposition,
                    TempHoldCompleteNote = s.TempHoldCompleteNote,
                    TempHoldCompleteDate = s.TempHoldCompleteDate,
                    TempHoldCompleteBy = s.TempHoldCompleteBy,
                    TempHoldCompleteByLast = s.TempHoldCompleteByNavigation.PersonNavigation.PersonLastName,
                    TempHoldCompleteByFirst = s.TempHoldCompleteByNavigation.PersonNavigation.PersonFirstName,
                    TempHoldCompleteByBadgeNumber = s.TempHoldCompleteByNavigation.OfficerBadgeNum
                }).Single();

        public async Task<TempHoldDetailsVm> SaveIntakeTempHold(IntakeTempHoldParam objTempHoldParam)
        {
            TempHold tempHold = new TempHold
            {
                FacilityId = objTempHoldParam.FacilityId,
                TempHoldDateIn = DateTime.Now,
                TempHoldInOfficerId = objTempHoldParam.ReceivingOfficerId,
                TempHoldType = objTempHoldParam.TempHoldTypeId,
                TempHoldLocationId = objTempHoldParam.TempHoldLocationId,
                TempHoldLocation = objTempHoldParam.TempHoldLocation,
                TempHoldNote = objTempHoldParam.TempHoldNote,
                CreateDate = DateTime.Now,
                CreateBy = _personnelId
            };
            _context.TempHold.Add(tempHold);

            //Update Inmate Prebook table TempHoldId
            InmatePrebook inmatePrebook =
                _context.InmatePrebook.Single(s => s.InmatePrebookId == objTempHoldParam.InmatePrebookId);

            if (inmatePrebook != null)
            {
                inmatePrebook.TempHoldId = tempHold.TempHoldId;
            }
            await _context.SaveChangesAsync();

            List<AoWizardProgressVm> tempwizardProgresses =
                    GetTempHoldWizardProgress(new[] { tempHold.TempHoldId });
            TempHoldDetailsVm tempHoldDetails = _context.TempHold.Where(w => w.TempHoldId == tempHold.TempHoldId)
                .Select(s => new TempHoldDetailsVm
                {
                    TempHoldId = s.TempHoldId,
                    TempHoldProgress = tempwizardProgresses.SingleOrDefault(x => x.TempHoldId == tempHold.TempHoldId)
                }).Single();

            return tempHoldDetails;
        }

        public async Task<int> UpdateTempHold(PrebookCompleteVm objParam)
        {
            TempHold tempHold = _context.TempHold.Single(s => s.TempHoldId == objParam.TempHoldId);

            if (tempHold != null)
            {
                tempHold.TempHoldType = objParam.TempHoldTypeId;
                tempHold.TempHoldLocation = objParam.TempHoldLocation;
                tempHold.TempHoldLocationId = objParam.TempHoldLocationId;
                tempHold.TempHoldNote = objParam.TempHoldNote;
                tempHold.TempHoldDisposition = objParam.TempHoldDisposition;
                tempHold.TempHoldCompleteNote = objParam.TempHoldCompleteNote;
                tempHold.TempHoldCompleteFlag = objParam.TempHoldCompleteFlag;
                tempHold.TempHoldCompleteDate = tempHold.TempHoldCompleteFlag == 1 ? DateTime.Now : (DateTime?)null;
                tempHold.TempHoldCompleteBy = tempHold.TempHoldCompleteFlag == 1 ? _personnelId : (int?)null;
            }

            if (objParam.BookingRequired)
            {
                InmatePrebook inmatePrebook =
                    _context.InmatePrebook.Single(s => s.InmatePrebookId == objParam.InmatePrebookId);

                if (inmatePrebook != null)
                {
                    inmatePrebook.TemporaryHold = null;
                    inmatePrebook.TempHoldId = null;
                }
            }

            return await _context.SaveChangesAsync();
        }

        public TempHoldVm GetTempHoldDetails(TempHoldDetailsVm tempHoldReq)
        {
            TempHoldVm tempHolds = new TempHoldVm();

            List<TempHoldDetailsVm> dbTempHold = _context.TempHold.Where(tho =>
                    (tho.TempHoldDisposition > 0 || tho.TempHoldType > 0) &&
                    (!tempHoldReq.IsActive || tho.TempHoldCompleteFlag != 1) &&
                    (tempHoldReq.FacilityId == 0 || tho.FacilityId == tempHoldReq.FacilityId)
                    && (tempHoldReq.TempHoldType == 0 || tho.TempHoldType == tempHoldReq.TempHoldType)
                    && (tempHoldReq.Disposition == 0 || tho.TempHoldDisposition == tempHoldReq.Disposition)
                    && (!tempHoldReq.FromDate.HasValue ||
                        tho.TempHoldDateIn.HasValue &&
                        tho.TempHoldDateIn.Value.Date >= tempHoldReq.FromDate.Value.Date &&
                        tho.TempHoldDateIn.Value.Date <=
                        tempHoldReq.ToDate.Value.Date)
                    && (tempHoldReq.OfficerId == 0 || tho.TempHoldInOfficerId == tempHoldReq.OfficerId))
                .Select(tho => new TempHoldDetailsVm
                {
                    TempHoldId = tho.TempHoldId,
                    OfficerId = tho.TempHoldInOfficerId,
                    Disposition = tho.TempHoldDisposition ?? 0,
                    TempHoldType = tho.TempHoldType ?? 0,
                    TempHoldDateIn = tho.TempHoldDateIn,
                    Location = tho.TempHoldLocation,
                    LocationId = tho.TempHoldLocationId ?? 0,
                    TempHoldNote = tho.TempHoldNote,
                    TempHoldCompleteNote = tho.TempHoldCompleteNote,
                    TempHoldCompleteDate = tho.TempHoldCompleteDate,
                    CompleteFlag = tho.TempHoldCompleteFlag,
                    CompleteBy = tho.TempHoldCompleteBy ?? 0,
                    WizardLastStepId = tho.TempHoldWizardLastStepId ?? 0,
                }).ToList();

			List<int> ipTempHoldIds = dbTempHold.Select(i => i.TempHoldId).ToList();

            List<TempHoldDetailsVm> inmatePrebooks = _context.InmatePrebook.Where(ip =>
                    (tempHoldReq.IsDeleted || ip.DeleteFlag != 1)
                    && ipTempHoldIds.Contains(ip.TempHoldId ?? 0)
                    && (tempHoldReq.PersonDetails == null ||
                        (string.IsNullOrEmpty(tempHoldReq.PersonDetails.LastName) ||
                            ip.Person.PersonLastName.StartsWith(tempHoldReq.PersonDetails.LastName))
                        && (string.IsNullOrEmpty(tempHoldReq.PersonDetails.FirstName) ||
                            ip.Person.PersonFirstName.StartsWith(tempHoldReq.PersonDetails
                                .FirstName))
                        && (tempHoldReq.PersonDetails.Dob == null ||
                            ip.Person.PersonDob == tempHoldReq.PersonDetails.Dob)))
                .Select(ip => new TempHoldDetailsVm
                {
                    TempHoldId = ip.TempHoldId ?? 0,
                    PrebookId = ip.InmatePrebookId,
                    PersonnelId = ip.PersonnelId,
                    IsDeleted = ip.DeleteFlag == 1,
                    PersonDetails = new PersonDetailVM
                    {
                        PersonId = ip.PersonId ?? 0,
                        LastName = ip.Person.PersonLastName,
                        FirstName = ip.Person.PersonFirstName
                    }
                }).ToList();

			tempHolds.TempHoldDetails = inmatePrebooks.SelectMany(ip =>
			dbTempHold.Where(tho => ip.TempHoldId == tho.TempHoldId),
                (ip, tho) => new TempHoldDetailsVm
                {
					
                    TempHoldId = tho.TempHoldId,
                    PrebookId = ip.PrebookId,
                    PersonnelId = ip.PersonnelId,
                    OfficerId = tho.OfficerId,
                    Disposition = tho.Disposition,
                    TempHoldType = tho.TempHoldType,
                    TempHoldDateIn = tho.TempHoldDateIn,
                    Location = tho.Location,
                    LocationId = tho.LocationId,
                    TempHoldNote = tho.TempHoldNote,
                    TempHoldCompleteNote = tho.TempHoldCompleteNote,
                    TempHoldCompleteDate = tho.TempHoldCompleteDate,
                    CompleteFlag = tho.CompleteFlag,
                    CompleteBy = tho.CompleteBy,
                    WizardLastStepId = tho.WizardLastStepId,
                    IsDeleted = ip.IsDeleted,
					PersonDetails = ip.PersonDetails
				}).ToList();

            if (tempHolds.TempHoldDetails.Any())
            {
                List<Lookup> lookupLst = _context.Lookup.Where(lk =>
                        (lk.LookupType == LookupConstants.TEMPHOLDDISPO ||
                         lk.LookupType == LookupConstants.TEMPHOLDTYPE) &&
                        lk.LookupIndex > 0)
                    .Select(lk => new Lookup
                    {
                        LookupIndex = lk.LookupIndex,
                        LookupType = lk.LookupType,
                        LookupDescription = lk.LookupDescription
                    }).ToList();

                List<int> officerIds = tempHolds.TempHoldDetails.Select(t => t.OfficerId).ToList();
                officerIds.AddRange(tempHolds.TempHoldDetails.Select(t => t.CompleteBy).ToList());

                List<PersonDetailVM> personnel = _context.Personnel.Where(
                        per => officerIds.Contains(per.PersonnelId))
                    .Select(per => new PersonDetailVM
                    {
                        PersonnelId = per.PersonnelId,
                        OfficerBadgeNumber = per.OfficerBadgeNum,
                        FirstName = per.PersonNavigation.PersonFirstName,
                        LastName = per.PersonNavigation.PersonLastName,
                        MiddleName = per.PersonNavigation.PersonMiddleName,
                    }).Distinct().ToList();

                List<KeyValuePair<int, int>> fromRecords =
                    _context.FormRecord.Where(fr =>
                            fr.FormTemplates.FormCategoryId == (int?)FormCategoryName.PrebookForms
                            && fr.InmatePrebookId > 0 && fr.DeleteFlag == 0)
                        .Select(fr => new KeyValuePair<int, int>(fr.FormRecordId, fr.InmatePrebookId ?? 0)).ToList();

                List<AoWizardProgressVm> wizardProgresses =
                    GetTempHoldWizardProgress(tempHolds.TempHoldDetails.Select(a => a.TempHoldId).ToArray());

                tempHolds.TempHoldDetails.ForEach(th =>
                {
					th.FormCount = fromRecords.Count(fr => fr.Value == th.PrebookId);
                    th.OfficerDetails = personnel.FirstOrDefault(p => p.PersonnelId == th.OfficerId);
                    th.CompletedByDetails = personnel.FirstOrDefault(p => p.PersonnelId == th.CompleteBy);
                    th.TempHoldProgress = wizardProgresses.SingleOrDefault(x => x.TempHoldId == th.TempHoldId);

                    if (th.Disposition > 0)
                        th.DispositionDesc = lookupLst
                            .FirstOrDefault(lk =>
                                lk.LookupIndex == th.Disposition && lk.LookupType == LookupConstants.TEMPHOLDDISPO)
                            ?.LookupDescription;
                    if (th.TempHoldType > 0)
                        th.TempHoldTypeDesc = lookupLst
                            .FirstOrDefault(lk =>
                                lk.LookupIndex == th.TempHoldType && lk.LookupType == LookupConstants.TEMPHOLDTYPE)
                            ?.LookupDescription;
                });

                tempHolds.TempHoldTypeCnt = tempHolds.TempHoldDetails.Where(t => t.TempHoldType > 0)
                    .Select(th => new LookupVm
                    {
                        LookupIndex = th.TempHoldType,
                        LookupDescription = th.TempHoldTypeDesc
                    }).GroupBy(res => new { res.LookupIndex, res.LookupDescription }).Select(thold => new RequestTypes
                    {
                        RequestCount = thold.Count(),
                        RequestLookupId = thold.Key.LookupIndex,
                        RequestLookupName = thold.Key.LookupDescription
                    }).ToList();

                tempHolds.DispositionCnt = tempHolds.TempHoldDetails.Where(t => t.Disposition > 0)
                    .Select(th => new LookupVm
                    {
                        LookupIndex = th.Disposition,
                        LookupDescription = th.DispositionDesc
                    }).GroupBy(res => new { res.LookupIndex, res.LookupDescription }).Select(thold => new RequestTypes
                    {
                        RequestCount = thold.Count(),
                        RequestLookupId = thold.Key.LookupIndex,
                        RequestLookupName = thold.Key.LookupDescription
                    }).ToList();
            }

            return tempHolds;
        }


        private List<AoWizardProgressVm> GetTempHoldWizardProgress(int[] tempHoldIds)
        {
            return _context.AoWizardProgressTempHold
                .Where(a => tempHoldIds.Contains(a.TempHoldId))
                .Select(wp => new AoWizardProgressVm
                {
                    WizardProgressId = wp.AoWizardProgressId,
                    WizardId = wp.AoWizardId,
                    TempHoldId = wp.TempHoldId,
                    WizardStepProgress = wp.AoWizardStepProgress
                        .Select(wsp => new AoWizardStepProgressVm
                        {
                            WizardStepProgressId = wsp.AoWizardStepProgressId,
                            WizardProgressId = wsp.AoWizardProgressId,
                            ComponentId = wsp.AoComponentId,
                            Component = new AoComponentVm
                            {
                                AppAoFunctionalityId = wsp.AoComponent.AppAofunctionalityId,
                                CanChangeVisibility = wsp.AoComponent.CanChangeVisibility,
                                ComponentId = wsp.AoComponent.AoComponentId,
                                ComponentName = wsp.AoComponent.ComponentName,
                                CustomFieldAllowed = wsp.AoComponent.CustomFieldAllowed,
                                CustomFieldKeyName = wsp.AoComponent.CustomFieldKeyName,
                                CustomFieldTableName = wsp.AoComponent.CustomFieldTableName,
                                DisplayName = wsp.AoComponent.DisplayName,
                                HasConfigurableFields = wsp.AoComponent.HasConfigurableFields,
                                IsLastScreen = wsp.AoComponent.IsLastScreen
                            },
                            StepComplete = wsp.StepComplete,
                            StepCompleteBy = new PersonnelVm
                            {
                                PersonnelId = wsp.StepCompleteBy.PersonnelId,
                                OfficerBadgeNumber = wsp.StepCompleteBy.OfficerBadgeNum,
                                PersonLastName = wsp.StepCompleteBy.PersonNavigation.PersonLastName,
                                PersonFirstName = wsp.StepCompleteBy.PersonNavigation.PersonFirstName,
                                PersonMiddleName = wsp.StepCompleteBy.PersonNavigation.PersonMiddleName
                            },
                            StepCompleteDate = wsp.StepCompleteDate,
                            StepCompleteNote=wsp.StepCompleteNote
                        }).ToList()
                }).ToList();
        }
    }
}