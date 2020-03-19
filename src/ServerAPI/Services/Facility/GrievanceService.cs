﻿using GenerateTables.Models;
using Microsoft.AspNetCore.Http;
using ServerAPI.Utilities;
using ServerAPI.ViewModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ServerAPI.Services
{
    public class GrievanceService : IGrievanceService
    {
        private readonly AAtims _context;
        private readonly ICommonService _commonService;
        private readonly int _personnelId;
        private readonly IWizardService _wizardService;
        private readonly IPersonService _personService;
        private readonly IPhotosService _photoService;

        public GrievanceService(AAtims context, ICommonService commonService,
            IHttpContextAccessor ihHttpContextAccessor, IWizardService wizardService, IPersonService personService, IPhotosService photosService)
        {
            _context = context;
            _commonService = commonService;
            _personnelId =
               Convert.ToInt32(ihHttpContextAccessor.HttpContext.User.FindFirst(AuthDetailConstants.PERSONNELID)?.Value);
            _wizardService = wizardService;
            _personService = personService;
            _photoService = photosService;
        }

        public async Task<int> InsertUpdateGrievance(GrievanceVm grievanceVm)
        {
            Grievance grievance = _context.Grievance
                .SingleOrDefault(g => g.GrievanceId == grievanceVm.GrievanceId);
            if (grievance is null)
            {
                string getIncidentNumber = _commonService
                    .GetGlobalNumber((int)AtimsGlobalNumber.GrievanceNumber);
                grievance = new Grievance
                {
                    CreateDate = DateTime.Now,
                    CreatedBy = _personnelId,
                    InmateId = grievanceVm.InmateId,
                    DateOccured = grievanceVm.DateOccured,
                    FacilityId = grievanceVm.FacilityId,
                    GrievanceNumber = getIncidentNumber,
                    SensitiveMaterial = grievanceVm.SensitiveMaterial
                };
            }
            else
            {
                grievance.UpdateBy = _personnelId;
                grievance.UpdateDate = DateTime.Now;
            }
            grievance.ReceiveDate = grievanceVm.ReceiveDate;
            grievance.GrievanceSummary = grievanceVm.GrievanceSummary;
            grievance.GrievanceLocation = grievanceVm.GrievanceLocation;
            grievance.GrievanceLocationId = grievanceVm.GrievanceLocationId > 0 ?
                grievanceVm.GrievanceLocationId : default(int?);
            grievance.GrievanceHousingUnitLocation = grievanceVm.HousingUnitLocation;
            grievance.GrievanceHousingUnitNumber = grievanceVm.HousingUnitNumber;
            grievance.GrievanceHousingUnitBed = grievanceVm.HousingUnitBed;
            grievance.GrievanceLocationOther = grievanceVm.OtherLocation;
            grievance.GrievanceType = grievanceVm.GrievanceType;
            grievance.SensitiveMaterial = grievanceVm.SensitiveMaterial;
            if (grievance.GrievanceId <= 0)
            {
                _context.Grievance.Add(grievance);
                _context.SaveChanges();
                AoWizardProgressVm wizardProgress = new AoWizardProgressVm
                {
                    GrievanceId = grievance.GrievanceId,
                    WizardId = 12
                };
                wizardProgress.WizardProgressId = await _wizardService.CreateWizardProgress(wizardProgress);
                int aoWizardFacilityId = _context.AoWizardFacility
                    .Single(wiz => wiz.AoWizardId == (int?)Wizards.grievance 
                    && wiz.FacilityId == grievanceVm.FacilityId).AoWizardFacilityId;
                int aoWizardFacilityStepId = _context.AoWizardFacilityStep
                    .First(wiz => wiz.AoComponentId == (int?)AoComponents.GrvDetailsComponent
                    && wiz.AoWizardFacilityId == aoWizardFacilityId
                    && wiz.IsActive).AoWizardFacilityStepId;
                AoWizardStepProgressVm wizardStep = new AoWizardStepProgressVm
                {
                    ComponentId = (int)AoComponents.GrvDetailsComponent,
                    AoWizardFacilityStepId = aoWizardFacilityStepId,
                    WizardProgressId = wizardProgress.WizardProgressId,
                    StepComplete = true,
                    StepCompleteById = _personnelId,
                    StepCompleteDate = DateTime.Now
                };
                await _wizardService.SetWizardStepComplete(wizardStep);
            }
            await _context.SaveChangesAsync();
            return grievance.GrievanceId;
        }

        public GrievanceCountVm GetGrievanceCount(GrievanceCountVm grievanceCountVm)
        {
            if (grievanceCountVm.IsFirst)
            {
                grievanceCountVm.LstLocation = _context.Privileges
                    .Where(p => p.FacilityId == grievanceCountVm.FacilityId
                                && p.InactiveFlag == 0)
                    .OrderBy(p => p.PrivilegeDescription)
                    .Select(p => new KeyValuePair<int, string>(p.PrivilegeId, p.PrivilegeDescription))
                    .Distinct().ToList();

                grievanceCountVm.LstDispo = _context.Lookup
                    .Where(lk => lk.LookupType == LookupConstants.GRIEVDISPO
                                 && lk.LookupInactive == 0)
                    .OrderByDescending(lk => lk.LookupOrder).ThenBy(lk => lk.LookupDescription)
                    .Select(lk => new KeyValuePair<int, string>(lk.LookupIndex, lk.LookupName))
                    .ToList();

                grievanceCountVm.LstGrievanceFlag = _context.Lookup
                    .Where(lk => lk.LookupType == LookupConstants.GRIEVFLAG
                                 && lk.LookupInactive == 0)
                    .OrderByDescending(lk => lk.LookupOrder).ThenBy(lk => lk.LookupDescription)
                    .Select(lk =>
                        new KeyValuePair<int, string>(lk.LookupIndex, lk.LookupDescription))
                    .ToList();

                grievanceCountVm.LstHousingDetails = _context.HousingUnit
                    .Where(hu => hu.FacilityId == grievanceCountVm.FacilityId)
                    .Select(hu => new HousingDetail
                    {
                        HousingUnitListId = hu.HousingUnitListId,
                        HousingUnitLocation = hu.HousingUnitLocation,
                        HousingUnitNumber = hu.HousingUnitNumber,
                        HousingUnitBedNumber = hu.HousingUnitBedNumber,
                        Inactive = (hu.HousingUnitInactive ?? 0) == 1
                    }).Distinct().ToList();
            }

            GetGrievanceCountDetails(ref grievanceCountVm);

            return grievanceCountVm;
        }

        void GetGrievanceCountDetails(ref GrievanceCountVm grievanceCountVm)
        {
            GrievanceCountVm grievanceCount = grievanceCountVm;

            List<Lookup> lsLookups = _context.Lookup.Where(lk =>
                lk.LookupType == LookupConstants.GRIEVTYPE).ToList();

            // to get grievance type details
            grievanceCountVm.LstType = _context.Grievance
                .Where(gr => gr.DeleteFlag == 0 && gr.GrievanceType > 0
                                                && gr.FacilityId == grievanceCount.FacilityId
                                                && (grievanceCount.Hours > 0 && gr.DateOccured.Value >=
                                                    DateTime.Now.AddHours(-grievanceCount.Hours) && gr.DateOccured.Value <= DateTime.Now
                                                    || grievanceCount.Hours == 0 && (!grievanceCount.FromDate.HasValue ||
                                                        gr.DateOccured.Value.Date >=
                                                        grievanceCount.FromDate.Value.Date) && (!grievanceCount.ToDate.HasValue ||
                                                        gr.DateOccured.Value.Date <=
                                                        grievanceCount.ToDate.Value.Date))
                                                && (grievanceCount.LocationId == 0 ||
                                                    gr.GrievanceLocationId == grievanceCount.LocationId)
                                                && (string.IsNullOrEmpty(grievanceCount.Building) ||
                                                    gr.GrievanceHousingUnitLocation == grievanceCount.Building)
                                                && (string.IsNullOrEmpty(grievanceCount.Num) ||
                                                    gr.GrievanceHousingUnitNumber == grievanceCount.Num)
                                                && (string.IsNullOrEmpty(grievanceCount.Bed) ||
                                                    gr.GrievanceHousingUnitBed == grievanceCount.Bed)
                                                && (string.IsNullOrEmpty(grievanceCount.Dispo) ||
                                                    gr.GrievanceDispositionLookup == grievanceCount.Dispo)
                                                && (string.IsNullOrEmpty(grievanceCount.Flag)
                                                    || gr.GrievanceFlag.Where(gf => (gf.DeleteFlag ?? 0) == 0
                                                                                    && gf.GrievanceFlagText ==
                                                                                    grievanceCount.Flag)
                                                        .Select(s => s.GrievanceId)
                                                        .Contains(gr.GrievanceId)))
                .Select(gr => new
                {
                    Name = lsLookups.SingleOrDefault(lk =>
                            lk.LookupIndex == gr.GrievanceType &&
                            lk.LookupType == LookupConstants.GRIEVTYPE)
                        .LookupDescription
                })
                .GroupBy(gr => gr).Select(gr => new
                {
                    gr.Key.Name,
                    Count = gr.Count()
                }).Select(gr => new GridInfoVm
                {
                    Name = gr.Name,
                    Cnt = gr.Count
                }).OrderBy(gr => gr.Name).ToList();

            // to get grievance against details
            List<GrievancePersonnelOrAgainstCount> grievanceAgainst =
                _context.Grievance
                    .Where(gr => gr.DeleteFlag == 0
                        && gr.FacilityId == grievanceCount.FacilityId
                        && !string.IsNullOrEmpty(gr.Department)
                        && (grievanceCount.Hours > 0 &&
                            gr.DateOccured.Value >= DateTime.Now.AddHours(-grievanceCount.Hours) &&
                            gr.DateOccured.Value <= DateTime.Now
                            || grievanceCount.Hours == 0 && (!grievanceCount.FromDate.HasValue ||
                                gr.DateOccured.Value.Date >=
                                grievanceCount.FromDate.Value.Date) && (!grievanceCount.ToDate.HasValue ||
                                gr.DateOccured.Value.Date <=
                                grievanceCount.ToDate.Value.Date))
                        && (grievanceCount.LocationId == 0 ||
                            gr.GrievanceLocationId == grievanceCount.LocationId)
                        && (string.IsNullOrEmpty(grievanceCount.Building) ||
                            gr.GrievanceHousingUnitLocation == grievanceCount.Building)
                        && (string.IsNullOrEmpty(grievanceCount.Num) ||
                            gr.GrievanceHousingUnitNumber == grievanceCount.Num)
                        && (string.IsNullOrEmpty(grievanceCount.Bed) ||
                            gr.GrievanceHousingUnitBed == grievanceCount.Bed)
                        && (string.IsNullOrEmpty(grievanceCount.Dispo) ||
                            gr.GrievanceDispositionLookup == grievanceCount.Dispo)
                        && (string.IsNullOrEmpty(grievanceCount.Flag)
                            || gr.GrievanceFlag.Where(gf => (gf.DeleteFlag ?? 0) == 0
                                    && gf.GrievanceFlagText == grievanceCount.Flag)
                                .Select(s => s.GrievanceId)
                                .Contains(gr.GrievanceId)))
                    .Select(gr => new
                    {
                        gr.Department
                    })
                    .GroupBy(gr => gr).Select(gr => new GrievancePersonnelOrAgainstCount
                    {
                        Name = gr.Key.Department,
                        Cnt = gr.Count()
                    })
                    .ToList();

            List<GrievancePersonnelOrAgainstCount> grievancePersonnel =
                _context.GrievancePersonnel
                    .Where(gr => gr.Grievance.DeleteFlag == 0
                        && gr.Grievance.FacilityId == grievanceCount.FacilityId
                        && (grievanceCount.Hours > 0 &&
                            gr.Grievance.DateOccured.Value >= DateTime.Now.AddHours(-grievanceCount.Hours) &&
                            gr.Grievance.DateOccured.Value <= DateTime.Now
                            || grievanceCount.Hours == 0 && (!grievanceCount.FromDate.HasValue ||
                                gr.Grievance.DateOccured.Value.Date >=
                                grievanceCount.FromDate.Value.Date) && (!grievanceCount.ToDate.HasValue ||
                                gr.Grievance.DateOccured.Value.Date <=
                                grievanceCount.ToDate.Value.Date))
                        && (grievanceCount.LocationId == 0 ||
                            gr.Grievance.GrievanceLocationId == grievanceCount.LocationId)
                        && (string.IsNullOrEmpty(grievanceCount.Building) ||
                            gr.Grievance.GrievanceHousingUnitLocation == grievanceCount.Building)
                        && (string.IsNullOrEmpty(grievanceCount.Num) ||
                            gr.Grievance.GrievanceHousingUnitNumber == grievanceCount.Num)
                        && (string.IsNullOrEmpty(grievanceCount.Bed) ||
                            gr.Grievance.GrievanceHousingUnitBed == grievanceCount.Bed)
                        && (string.IsNullOrEmpty(grievanceCount.Dispo) ||
                            gr.Grievance.GrievanceDispositionLookup == grievanceCount.Dispo)
                        && (string.IsNullOrEmpty(grievanceCount.Flag)
                            || gr.Grievance.GrievanceFlag.Where(gf => (gf.DeleteFlag ?? 0) == 0
                                    && gf.GrievanceFlagText ==
                                    grievanceCount.Flag)
                                .Select(s => s.GrievanceId)
                                .Contains(gr.GrievanceId)))
                    .Select(gr => new
                    {
                        gr.PersonnelId
                    })
                    .GroupBy(gr => gr).Select(gr => new GrievancePersonnelOrAgainstCount
                    {
                        PersonnelId = gr.Key.PersonnelId,
                        Cnt = gr.Count()
                    })
                    .ToList();

            // to get grievance personnel details
            grievanceCountVm.LstPersonnel = _context.Grievance
                .Where(gr => gr.DeleteFlag == 0
                    && gr.FacilityId == grievanceCount.FacilityId
                    && (grievanceCount.Hours > 0 &&
                        gr.DateOccured.Value >= DateTime.Now.AddHours(-grievanceCount.Hours) &&
                        gr.DateOccured.Value <= DateTime.Now
                        || grievanceCount.Hours == 0 && (!grievanceCount.FromDate.HasValue ||
                            gr.DateOccured.Value.Date >=
                            grievanceCount.FromDate.Value.Date) && (!grievanceCount.ToDate.HasValue ||
                            gr.DateOccured.Value.Date <=
                            grievanceCount.ToDate.Value.Date))
                    && (grievanceCount.LocationId == 0 || gr.GrievanceLocationId == grievanceCount.LocationId)
                    && (string.IsNullOrEmpty(grievanceCount.Building) ||
                        gr.GrievanceHousingUnitLocation == grievanceCount.Building)
                    && (string.IsNullOrEmpty(grievanceCount.Num) ||
                        gr.GrievanceHousingUnitNumber == grievanceCount.Num)
                    && (string.IsNullOrEmpty(grievanceCount.Bed) ||
                        gr.GrievanceHousingUnitBed == grievanceCount.Bed)
                    && (string.IsNullOrEmpty(grievanceCount.Dispo) ||
                        gr.GrievanceDispositionLookup == grievanceCount.Dispo)
                    && (string.IsNullOrEmpty(grievanceCount.Flag)
                        || gr.GrievanceFlag.Where(gf => (gf.DeleteFlag ?? 0) == 0
                                && gf.GrievanceFlagText == grievanceCount.Flag)
                            .Select(s => s.GrievanceId)
                            .Contains(gr.GrievanceId)))
                .Select(gr => new
                {
                    PersonnelId = grievanceCount.PersonnelType == CommonConstants.CREATEDBY.ToString()
                        ? gr.CreatedBy
                        : gr.ReviewedBy ?? 0
                })
                .GroupBy(gr => gr).Select(gr => new GrievancePersonnelOrAgainstCount
                {
                    PersonnelId = gr.Key.PersonnelId,
                    Cnt = gr.Count()
                })
                .ToList();

            grievanceCountVm.LstPersonnel = grievanceCountVm.LstPersonnel.Where(gr => gr.PersonnelId > 0).ToList();

            List<int> lstPersonnelIds = grievanceCountVm.LstPersonnel
                .Select(per => per.PersonnelId).ToList();

            // adding personnelids of LstAgainst list
            lstPersonnelIds.AddRange(grievancePersonnel.Select(per => per.PersonnelId).ToList());

            // getting the person details (1ms)
            List<PersonnelVm> lstPersonDetails = _personService.GetPersonNameList(lstPersonnelIds);

            // grievancepersonnel list
            grievanceCountVm.LstPersonnel.ForEach(item =>
            {
                item.PersonLastName = lstPersonDetails.Single(p => p.PersonnelId == item.PersonnelId).PersonLastName;
                item.PersonFirstName = lstPersonDetails.Single(p => p.PersonnelId == item.PersonnelId).PersonFirstName;
                item.PersonnelNumber = lstPersonDetails.Single(p => p.PersonnelId == item.PersonnelId).PersonnelNumber;
            });

            // LstAgainst list
            grievancePersonnel.ForEach(item =>
            {
                item.PersonLastName = lstPersonDetails.Single(p => p.PersonnelId == item.PersonnelId).PersonLastName;
                item.PersonFirstName = lstPersonDetails.Single(p => p.PersonnelId == item.PersonnelId).PersonFirstName;
                item.PersonnelNumber = lstPersonDetails.Single(p => p.PersonnelId == item.PersonnelId).PersonnelNumber;
            });

            grievanceCountVm.LstAgainst = grievanceAgainst;
            grievanceCountVm.LstAgainst.AddRange(grievancePersonnel);
        }

        public List<GrievanceCountDetails> GetGrievanceCountPopupDetails(GrievanceCountVm grievanceCountVm)
        {
            List<Lookup> lstLookups = _context.Lookup.Where(lk =>
                lk.LookupType == LookupConstants.GRIEVTYPE).ToList();

            List<GrievanceCountDetails> lstGrievanceCountDetails = new List<GrievanceCountDetails>();

            switch (grievanceCountVm.Type)
            {
                // to get grievance type / against details
                case GrievanceType.Type:
                case GrievanceType.Against:
                    if (grievanceCountVm.PersonnelId == 0)
                    {
                        lstGrievanceCountDetails = _context.Grievance
                            .Where(gr => gr.DeleteFlag == 0
                                && gr.FacilityId == grievanceCountVm.FacilityId
                                && (grievanceCountVm.Hours > 0 &&
                                    gr.DateOccured.Value >= DateTime.Now.AddHours(-grievanceCountVm.Hours) &&
                                    gr.DateOccured.Value <= DateTime.Now
                                    || grievanceCountVm.Hours == 0 && (!grievanceCountVm.FromDate.HasValue ||
                                        gr.DateOccured.Value.Date >=
                                        grievanceCountVm.FromDate.Value.Date) && (!grievanceCountVm.ToDate.HasValue ||
                                        gr.DateOccured.Value.Date <=
                                        grievanceCountVm.ToDate.Value.Date))
                                        && (grievanceCountVm.LocationId == 0 ||
                                                    gr.GrievanceLocationId == grievanceCountVm.LocationId)
                                                && (string.IsNullOrEmpty(grievanceCountVm.Building) ||
                                                    gr.GrievanceHousingUnitLocation == grievanceCountVm.Building)
                                                && (string.IsNullOrEmpty(grievanceCountVm.Num) ||
                                                    gr.GrievanceHousingUnitNumber == grievanceCountVm.Num)
                                                && (string.IsNullOrEmpty(grievanceCountVm.Bed) ||
                                                    gr.GrievanceHousingUnitBed == grievanceCountVm.Bed)
                                                && (string.IsNullOrEmpty(grievanceCountVm.Dispo) ||
                                                    gr.GrievanceDispositionLookup == grievanceCountVm.Dispo)
                                && (string.IsNullOrEmpty(grievanceCountVm.Flag)
                                    || gr.GrievanceFlag.Where(gf => (gf.DeleteFlag ?? 0) == 0
                                            && gf.GrievanceFlagText ==
                                            grievanceCountVm.Flag)
                                        .Select(s => s.GrievanceId)
                                        .Contains(gr.GrievanceId))
                                && (grievanceCountVm.Type != GrievanceType.Against ||
                                    gr.Department == grievanceCountVm.TypeValue))
                            .OrderByDescending(gr => gr.GrievanceId)
                            .Select(gr => new GrievanceCountDetails
                            {
                                Type = lstLookups.SingleOrDefault(lk =>
                                        lk.LookupIndex == gr.GrievanceType &&
                                        lk.LookupType == LookupConstants.GRIEVTYPE)
                                    .LookupDescription,
                                GrievanceNumber = gr.GrievanceNumber,
                                Date = gr.DateOccured,
                                InmateId = gr.InmateId,
                                Against = gr.Department,
                                Summary = gr.GrievanceSummary,
                                SetReview = gr.SetReview ?? 0
                            })
                            .Where(gr =>
                                grievanceCountVm.Type != GrievanceType.Type || gr.Type == grievanceCountVm.TypeValue)
                            .ToList();
                    }

                    break;
                case GrievanceType.Personnel:
                    // to get grievance personnel details
                    lstGrievanceCountDetails = _context.Grievance
                        .Where(gr => gr.DeleteFlag == 0
                            && gr.FacilityId == grievanceCountVm.FacilityId
                            && (grievanceCountVm.Hours > 0 &&
                                gr.DateOccured.Value >= DateTime.Now.AddHours(-grievanceCountVm.Hours) &&
                                gr.DateOccured.Value <= DateTime.Now
                                || grievanceCountVm.Hours == 0 && (!grievanceCountVm.FromDate.HasValue ||
                                    gr.DateOccured.Value.Date >=
                                    grievanceCountVm.FromDate.Value.Date) && (!grievanceCountVm.ToDate.HasValue ||
                                    gr.DateOccured.Value.Date <=
                                    grievanceCountVm.ToDate.Value.Date))
                                    && (grievanceCountVm.LocationId == 0 ||
                                                    gr.GrievanceLocationId == grievanceCountVm.LocationId)
                                                && (string.IsNullOrEmpty(grievanceCountVm.Building) ||
                                                    gr.GrievanceHousingUnitLocation == grievanceCountVm.Building)
                                                && (string.IsNullOrEmpty(grievanceCountVm.Num) ||
                                                    gr.GrievanceHousingUnitNumber == grievanceCountVm.Num)
                                                && (string.IsNullOrEmpty(grievanceCountVm.Bed) ||
                                                    gr.GrievanceHousingUnitBed == grievanceCountVm.Bed)
                                                && (string.IsNullOrEmpty(grievanceCountVm.Dispo) ||
                                                    gr.GrievanceDispositionLookup == grievanceCountVm.Dispo)
                            && (grievanceCountVm.PersonnelType != CommonConstants.CREATEDBY.ToString() ||
                                gr.CreatedBy == grievanceCountVm.PersonnelId)
                            && (grievanceCountVm.PersonnelType != CommonConstants.REVIEWEDBY.ToString() ||
                                (gr.ReviewedBy ?? 0) == grievanceCountVm.PersonnelId)
                            && (string.IsNullOrEmpty(grievanceCountVm.Flag)
                                || gr.GrievanceFlag.Where(gf => (gf.DeleteFlag ?? 0) == 0
                                        && gf.GrievanceFlagText ==
                                        grievanceCountVm.Flag)
                                    .Select(s => s.GrievanceId)
                                    .Contains(gr.GrievanceId)))
                        .OrderByDescending(gr => gr.GrievanceId)
                        .Select(gr => new GrievanceCountDetails
                        {
                            Type = lstLookups.SingleOrDefault(lk =>
                                    lk.LookupIndex == gr.GrievanceType &&
                                    lk.LookupType == LookupConstants.GRIEVTYPE)
                                .LookupDescription,
                            GrievanceNumber = gr.GrievanceNumber,
                            Date = gr.DateOccured,
                            InmateId = gr.InmateId,
                            Against = gr.Department,
                            Summary = gr.GrievanceSummary,
                            SetReview = gr.SetReview ?? 0
                        })
                        .ToList();
                    break;
            }

            // to get grievance against details
            if (grievanceCountVm.Type == GrievanceType.Against && grievanceCountVm.PersonnelId > 0)
            {
                lstGrievanceCountDetails = _context.GrievancePersonnel
                    .Where(gr => gr.Grievance.DeleteFlag == 0
                        && gr.Grievance.FacilityId == grievanceCountVm.FacilityId
                        && (grievanceCountVm.Hours > 0 && gr.Grievance.DateOccured.Value >=
                            DateTime.Now.AddHours(-grievanceCountVm.Hours) &&
                            gr.Grievance.DateOccured.Value <= DateTime.Now
                            || grievanceCountVm.Hours == 0 && (!grievanceCountVm.FromDate.HasValue ||
                                gr.Grievance.DateOccured.Value.Date >=
                                grievanceCountVm.FromDate.Value.Date) && (!grievanceCountVm.ToDate.HasValue ||
                                gr.Grievance.DateOccured.Value.Date <=
                                grievanceCountVm.ToDate.Value.Date))
                                && (grievanceCountVm.LocationId == 0 ||
                                                    gr.Grievance.GrievanceLocationId == grievanceCountVm.LocationId)
                                                && (string.IsNullOrEmpty(grievanceCountVm.Building) ||
                                                    gr.Grievance.GrievanceHousingUnitLocation == grievanceCountVm.Building)
                                                && (string.IsNullOrEmpty(grievanceCountVm.Num) ||
                                                    gr.Grievance.GrievanceHousingUnitNumber == grievanceCountVm.Num)
                                                && (string.IsNullOrEmpty(grievanceCountVm.Bed) ||
                                                    gr.Grievance.GrievanceHousingUnitBed == grievanceCountVm.Bed)
                                                && (string.IsNullOrEmpty(grievanceCountVm.Dispo) ||
                                                    gr.Grievance.GrievanceDispositionLookup == grievanceCountVm.Dispo)
                        && gr.PersonnelId == grievanceCountVm.PersonnelId
                        && (string.IsNullOrEmpty(grievanceCountVm.Flag)
                            || gr.Grievance.GrievanceFlag.Where(gf => (gf.DeleteFlag ?? 0) == 0
                                    && gf.GrievanceFlagText ==
                                    grievanceCountVm.Flag)
                                .Select(s => s.GrievanceId)
                                .Contains(gr.GrievanceId)))
                    .OrderByDescending(gr => gr.GrievanceId)
                    .Select(gr => new GrievanceCountDetails
                    {
                        Type = lstLookups.SingleOrDefault(lk =>
                                lk.LookupIndex == gr.Grievance.GrievanceType &&
                                lk.LookupType == LookupConstants.GRIEVTYPE)
                            .LookupDescription,
                        GrievanceNumber = gr.Grievance.GrievanceNumber,
                        Date = gr.Grievance.DateOccured,
                        InmateId = gr.Grievance.InmateId,
                        Against = gr.Grievance.Department,
                        Summary = gr.Grievance.GrievanceSummary,
                        SetReview = gr.Grievance.SetReview ?? 0
                    })
                    .ToList();
            }

            int[] lstInmateIds = lstGrievanceCountDetails.Select(g => g.InmateId).ToArray();

            List<PersonDetailVM> lstPerson = _context.Inmate
                .Where(p => lstInmateIds.Contains(p.InmateId))
                .Select(p => new PersonDetailVM
                {
                    LastName = p.Person.PersonLastName,
                    FirstName = p.Person.PersonFirstName,
                    Number = p.InmateNumber,
                    InmateId = p.InmateId
                }).ToList();

            lstGrievanceCountDetails.ForEach((item) =>
            {
                item.LastName = lstPerson.Single(p => p.InmateId == item.InmateId).LastName;
                item.FirstName = lstPerson.Single(p => p.InmateId == item.InmateId).FirstName;
                item.Number = lstPerson.Single(p => p.InmateId == item.InmateId).Number;
            });

            return lstGrievanceCountDetails;
        }

        public GrievanceDetailsVm GetGrievanceDetails(GrievanceInputs inputs)
        {
            GrievanceDetailsVm grievance = new GrievanceDetailsVm { GrievanceList = GetGrievances(inputs) };

            grievance.GrievanceElapsed = new List<GrievanceActiveHistoryCount>()
            {
                new GrievanceActiveHistoryCount{
                   GrievanceName = CommonConstants.ALL.ToString(),
                    GrievanceElapsedCount = grievance.GrievanceList.Count,
                   PastDueCount =  grievance.GrievanceList.Count(c=>c.PastDue)
                },

                new GrievanceActiveHistoryCount{
                    GrievanceName = ElapsedConstants.HOURS73PLUS, 
                    GrievanceElapsedCount = grievance.GrievanceList
                    .Count(c => DateTime.Now.Subtract(c.ReceiveDate ?? DateTime.Now).TotalHours >= 73),
                    PastDueCount =  grievance.GrievanceList.Count(c=>
                    DateTime.Now.Subtract(c.ReceiveDate ?? DateTime.Now).TotalHours >= 73 && c.PastDue)
                    },

                new GrievanceActiveHistoryCount{
                   GrievanceName =  ElapsedConstants.HOURS49_72, 
                  GrievanceElapsedCount =  grievance.GrievanceList
                    .Count(c => DateTime.Now.Subtract(c.ReceiveDate ?? DateTime.Now).TotalHours < 73
                                && DateTime.Now.Subtract(c.ReceiveDate ?? DateTime.Now).TotalHours >= 49) ,
                    PastDueCount = grievance.GrievanceList.Count(c=>DateTime.Now.Subtract(c.ReceiveDate ?? DateTime.Now).TotalHours < 73
                                && DateTime.Now.Subtract(c.ReceiveDate ?? DateTime.Now).TotalHours >= 49 && c.PastDue)},

                new GrievanceActiveHistoryCount{
                  GrievanceName =  ElapsedConstants.HOURS25_48, 
                    GrievanceElapsedCount =   grievance.GrievanceList
                    .Count(c => DateTime.Now.Subtract(c.ReceiveDate ?? DateTime.Now).TotalHours < 49
                                && DateTime.Now.Subtract(c.ReceiveDate ?? DateTime.Now).TotalHours >= 25),
                                PastDueCount =  grievance.GrievanceList.Count(c=>DateTime.Now.Subtract(c.ReceiveDate ?? DateTime.Now).TotalHours < 49
                                && DateTime.Now.Subtract(c.ReceiveDate ?? DateTime.Now).TotalHours >= 25 && c.PastDue)},

                new GrievanceActiveHistoryCount{
                    GrievanceName = ElapsedConstants.HOURS00_24,
                GrievanceElapsedCount = grievance.GrievanceList
                    .Count(c => DateTime.Now.Subtract(c.ReceiveDate ?? DateTime.Now).TotalHours < 25
                                && (c.ReceiveDate ?? DateTime.Now) <= DateTime.Now),
                 PastDueCount =  grievance.GrievanceList.Count(c=>DateTime.Now.Subtract(c.ReceiveDate ?? DateTime.Now).TotalHours < 25
                                && (c.ReceiveDate ?? DateTime.Now) <= DateTime.Now && c.PastDue)},
            };

            grievance.GrievanceStatus = new List<GrievanceActiveHistoryCount>()
            {
                new GrievanceActiveHistoryCount{
                  GrievanceName =   CommonConstants.ALL.ToString(),
                   GrievanceStatusCount= grievance.GrievanceList.Count,
                    PastDueCount =  grievance.GrievanceList.Count(c=>c.PastDue)},

                new GrievanceActiveHistoryCount{
                      GrievanceName =  IncidentNarrativeConstants.READYFORREVIEW,
                GrievanceStatusCount=    grievance.GrievanceList.Count(c => c.ReadyForReview == 1),
                     PastDueCount =  grievance.GrievanceList.Count(c=>c.ReadyForReview == 1 && c.PastDue)},

                new GrievanceActiveHistoryCount{
                GrievanceName =   IncidentNarrativeConstants.NOTREADYFORREVIEW,
                 GrievanceStatusCount=   grievance.GrievanceList.Count(c => !c.ReadyForReview.HasValue),
                     PastDueCount =  grievance.GrievanceList.Count(c=>!c.ReadyForReview.HasValue && c.PastDue)},
                 
            };
            if(inputs.IsComplete){
                   grievance.GrievanceStatus.AddRange(new List<GrievanceActiveHistoryCount>()
               {
                new GrievanceActiveHistoryCount{
                   GrievanceName =  IncidentNarrativeConstants.COMPLETED,
                   GrievanceStatusCount =  grievance.GrievanceList.Count(c => c.SetReview == 1),
                    }
                 });
            }
            List<Lookup> lookups = _context.Lookup
                .Where(w => w.LookupType == LookupConstants.GRIEVTYPE).ToList();

            grievance.GrievanceType = new List<GrievanceActiveHistoryCount>()
            {
                new GrievanceActiveHistoryCount{
                   GrievanceName =  CommonConstants.ALL.ToString(),
                  GrievanceTypeCount=     grievance.GrievanceList.Count(c => c.GrievanceType > 0),
                    PastDueCount =  grievance.GrievanceList.Count(c=>c.GrievanceType > 0 && c.PastDue)
                    }
            };

            grievance.GrievanceType.AddRange(grievance.GrievanceList
                .GroupBy(g => g.GrievanceType)
                .SelectMany(g => lookups.Where(w => w.LookupIndex == g.Key),
                    (g, l) => new GrievanceActiveHistoryCount
                    {
                        GrievanceName = l.LookupDescription,
                        GrievanceTypeCount = g.Count(),
                        PastDueCount = g.Count(c=>c.PastDue)
                    }).OrderBy(o=>o.GrievanceName));

            grievance.GrievanceIncidentLoc = new List<GrievanceActiveHistoryCount>()
            {
                new GrievanceActiveHistoryCount{
                GrievanceName= CommonConstants.ALL.ToString(),
                 GrievanceLocationCount= grievance.GrievanceList.Count(c => c.GrievanceLocation != null)
                    }
            };

            grievance.GrievanceIncidentLoc.AddRange(grievance.GrievanceList
                .Where(w => !string.IsNullOrEmpty(w.GrievanceLocation))
                .GroupBy(g => g.GrievanceLocation)
                .Select(s => new GrievanceActiveHistoryCount{
                  GrievanceName= s.Key,
                  GrievanceLocationCount= s.Count()
                  }).OrderBy(o=>o.GrievanceName));

            grievance.GrievanceList.ForEach(item =>
            {
                item.WizardProgressId = _context.AoWizardProgressGrievance.FirstOrDefault(awp =>
                     awp.GrievanceId == item.GrievanceId &&
                     awp.AoWizardId == 12)?.AoWizardProgressId;

                if (item.WizardProgressId.HasValue)
                {
                    item.LastStep = _context.AoWizardStepProgress
                        .Where(aws => aws.AoWizardProgressId == item.WizardProgressId.Value).Select(aw => new WizardStep
                        {
                            ComponentId = aw.AoComponentId,
                            WizardProgressId = aw.AoWizardProgressId,
                            WizardStepProgressId = aw.AoWizardStepProgressId,
                            StepComplete = aw.StepComplete
                        }).ToList();
                }
                item.Person = (from i in _context.Inmate
                               where i.InmateId == item.InmateId
                               select new PersonInfoVm
                               {
                                   PersonLastName = i.Person.PersonLastName,
                                   PersonFirstName = i.Person.PersonFirstName,
                                   PersonMiddleName = i.Person.PersonMiddleName,
                                   PersonSuffix = i.Person.PersonSuffix,
                                   InmateNumber = i.InmateNumber,
                                   PersonDob = i.Person.PersonDob
                               }).SingleOrDefault();


            });
            return grievance;
        }

        private List<GrievanceVm> GetGrievances(GrievanceInputs inputs)
        {
            List<LookupVm> lookups =
                   _commonService.GetLookups(new[] { LookupConstants.GRIEVTYPE }, true);

            IQueryable<Grievance> grievance = _context.Grievance
                .Where(w => !inputs.IsSensitiveMaterial || w.SensitiveMaterial);
            if (inputs.isDeleted && !inputs.ActivePage) //Check deleted 
                grievance = _context.Grievance;
            else if (!inputs.isDeleted && !inputs.ActivePage)
                grievance = grievance.Where(w => w.DeleteFlag == 0);
            else
            {
                grievance = grievance.Where(w => w.DeleteFlag == 0 && w.FacilityId == inputs.FacilityId);
            }
            switch (inputs.ActionFlag)
            {
                case 1: //Hours
                    grievance = grievance.Where(w =>
                        inputs.GrievanceAppealFlag > 0
                            ? w.AppealDate >= DateTime.Now.AddHours(inputs.Hours) && w.AppealDate <= DateTime.Now
                            : w.DateOccured >= DateTime.Now.AddHours(inputs.Hours) &&
                              w.DateOccured <= DateTime.Now);
                    break;
                case 2: //Active
                    grievance = grievance.Where(w => w.SetReview != 1);
                    break;
                case 3: //By Date Range
                    grievance = grievance.Where(w =>
                        inputs.GrievanceAppealFlag > 0
                            ? w.AppealDate >= inputs.FromDate &&
                              w.AppealDate <= inputs.ToDate.AddDays(1).AddTicks(-1)
                            : w.DateOccured >= inputs.FromDate &&
                              w.DateOccured <= inputs.ToDate.AddDays(1).AddTicks(-1));
                    break;
                case 4: //All Dates
                    break;
            }

            if (inputs.GrievanceType > 0)
            {
                grievance = grievance.Where(w => w.GrievanceType == inputs.GrievanceType);
            }

            if (inputs.Grievancenumber != null)
            {
                grievance = grievance.Where(w => w.GrievanceNumber == inputs.Grievancenumber);
            }

            if (inputs.GrievanceDepartment != null)
            {
                grievance = grievance.Where(w => w.Department == inputs.GrievanceDepartment);
            }

            if (inputs.Building != null)
            {
                grievance =
                    grievance.Where(w => w.GrievanceHousingUnitLocation == inputs.Building);
            }

            if (inputs.Number != null)
            {
                grievance =
                    grievance.Where(w => w.GrievanceHousingUnitNumber == inputs.Number);
            }

            if (inputs.Bed != null)
            {
                grievance = grievance.Where(w => w.GrievanceHousingUnitBed == inputs.Bed);
            }

            if (inputs.Location != null)
            {
                grievance = grievance.Where(w => w.GrievanceLocation == inputs.Location);
            }

            if (inputs.Disposition != null)
            {
                grievance =
                    grievance.Where(w => w.GrievanceDispositionLookup == inputs.Disposition);
            }

            if (inputs.PersonnelId > 0)
            {
                grievance = grievance.Where(w => w.CreatedBy == inputs.PersonnelId);
            }

            if (inputs.InmateId > 0)
            {
                int inmateCount = grievance.Count(w => w.InmateId == inputs.InmateId);

                if (inmateCount > 0)
                {
                    grievance = grievance.Where(w => w.InmateId == inputs.InmateId);
                }
                else
                {
                    int[] grievanceInmateList = _context.GrievanceInmate.Where(w => w.InmateId == inputs.InmateId)
                        .Select(s => s.GrievanceId).ToArray();
                    grievance = grievance.Where(w => grievanceInmateList.Contains(w.GrievanceId));
                }
            }

            if (inputs.GrievanceFlag != null)
            {
                int[] grievanceFlagInmate = _context.GrievanceFlag
                    .Where(w => w.GrievanceFlagText == inputs.GrievanceFlag &&
                                (!w.DeleteFlag.HasValue || w.DeleteFlag == 0)).Select(s => s.GrievanceId).ToArray();
                grievance = grievance.Where(w => grievanceFlagInmate.Contains(w.GrievanceId));
            }

            if (inputs.GrievanceAppealFlag > 0)
            {
                grievance = grievance.Where(w => w.GrievanceAppeal.Any(a => a.DeleteFlag != 1 && a.GrievanceId == w.GrievanceId));

                switch (inputs.GrievanceAppealFlag)
                {
                    case 2:
                        grievance =
                            grievance.Where(w => w.GrievanceAppeal.Any(a => !a.ReviewDate.HasValue));
                        break;
                    default:
                        grievance =
                           grievance.Where(w => w.GrievanceAppeal.Any(a => a.ReviewDate.HasValue));
                        break;
                }
                if (inputs.GrivenceAppealDispo != null)
                {
                    grievance = grievance.Where(w => w.GrievanceAppeal.Any(a => a.AppealDisposition == inputs.GrivenceAppealDispo));
                }

            }

            List<GrievanceVm> grievanceDetails = grievance.Select(s => new GrievanceVm
            {
                DateOccured = s.DateOccured,
                InmateId = s.InmateId,
                FacilityId=s.FacilityId,
                GrievanceId = s.GrievanceId,
                GrievanceSummary = s.GrievanceSummary,
                GrievanceWizardStep = s.GrievanceWizardStep,
                ReadyForReview = s.ReadyForReview,
                SetReview = s.SetReview,
                GrievanceLocation = s.GrievanceLocation,
                GrievanceLocationId = s.GrievanceLocationId ?? 0,
                HousingUnitLocation = s.GrievanceHousingUnitLocation,
                HousingUnitNumber = s.GrievanceHousingUnitNumber,
                HousingUnitBed = s.GrievanceHousingUnitBed,
                GrievanceDispositionLookup = s.GrievanceDispositionLookup,
                GrievanceNumber = s.GrievanceNumber,
                GrievanceType = s.GrievanceType,
                AppealDate = s.AppealDate,
                OtherLocation = s.GrievanceLocationOther,
                CreatedBy = s.CreatedBy,
                CreatedByPerson = new PersonnelVm
                {
                    PersonLastName = s.CreatedByNavigation.PersonNavigation.PersonLastName,
                    PersonFirstName = s.CreatedByNavigation.PersonNavigation.PersonFirstName,
                    OfficerBadgeNumber = s.CreatedByNavigation.OfficerBadgeNum
                },
                Department = s.Department,
                ReviewedDate = s.ReviewedDate,
                ReviewedByPerson = new PersonnelVm
                {
                    PersonLastName = s.ReviewedByNavigation.PersonNavigation.PersonLastName,
                    PersonFirstName = s.ReviewedByNavigation.PersonNavigation.PersonFirstName,
                    OfficerBadgeNumber = s.ReviewedByNavigation.OfficerBadgeNum
                },
                DeleteFlag = s.DeleteFlag,
                ReceiveDate = s.ReceiveDate,
                SensitiveMaterial = s.SensitiveMaterial,
                AppealCt = s.GrievanceAppeal.Count(),
                PastDue = s.ReceiveDate.HasValue ? 
                s.ReceiveDate.Value.AddDays(
                    Convert.ToDouble(lookups.Single(si=>si.LookupIndex == s.GrievanceType).LookupCategory)) < DateTime.Now
                : false
            }).OrderBy(o => o.DateOccured).ToList();
            return grievanceDetails;
        }

        public GrievanceHistoryDetails GetGrievanceHistory(GrievanceInputs inputs)
        {
            GrievanceHistoryDetails history = new GrievanceHistoryDetails
            {
                GrievanceDetails = GetGrievanceDetails(inputs),
                Location = _context.Privileges.Where(w => !w.FacilityId.HasValue || w.FacilityId == inputs.FacilityId)
                    .Select(s => s.PrivilegeDescription).ToArray(),
                HousingDetails = _context.HousingUnit
                    .Where(hu => hu.FacilityId == inputs.FacilityId)
                    .Select(hu => new HousingDetail
                    {
                        HousingUnitListId = hu.HousingUnitListId,
                        HousingUnitLocation = hu.HousingUnitLocation,
                        HousingUnitNumber = hu.HousingUnitNumber,
                        HousingUnitBedNumber = hu.HousingUnitBedNumber,
                        Inactive = (hu.HousingUnitInactive ?? 0) == 1
                    }).Distinct().ToList()
            };
            
            return history;
        }

        public async Task<int> InsertAdditionalInmate(GrievanceVm grievance)
        {
            GrievanceInmate grievanceInmate = new GrievanceInmate
            {
                GrievanceId = grievance.GrievanceId,
                InmateId = grievance.InmateId
            };
            _context.GrievanceInmate.Add(grievanceInmate);

            return await _context.SaveChangesAsync();
        }

        public async Task<int> DeleteAdditionalInmate(int grievanceInmateId)
        {
            GrievanceInmate grievanceInmate = _context.GrievanceInmate.Single(g => g.GrievanceInmateId == grievanceInmateId);

            _context.GrievanceInmate.Remove(grievanceInmate);

            return await _context.SaveChangesAsync();
        }

        public async Task<int> InsertAdditionalPersonnel(GrievanceVm grievance)
        {
            GrievancePersonnel grievancePersonnel = new GrievancePersonnel
            {
                GrievanceId = grievance.GrievanceId,
                PersonnelId = grievance.PersonnelId ?? 0
            };
            _context.GrievancePersonnel.Add(grievancePersonnel);
            return await _context.SaveChangesAsync();
        }

        public async Task<int> DeleteAdditionalPersonnel(int grievancePersonnelId)
        {
            GrievancePersonnel grievancePersonnel = _context.GrievancePersonnel.Single(p => p.GrievancePersonnelId == grievancePersonnelId);
            _context.GrievancePersonnel.Remove(grievancePersonnel);
            return await _context.SaveChangesAsync();
        }

        public async Task<int> UpdateGrievanceDepartment(GrievanceVm grv)
        {
            Grievance grievance = _context.Grievance.Single(g => g.GrievanceId == grv.GrievanceId);
            grievance.Department = grv.Department;
            grievance.ReadyForReview = grv.ReadyForReview;
            return await _context.SaveChangesAsync();
        }

        public async Task<int> UpdateGrievanceReview(GrievanceReview review)
        {

            Grievance grievance = _context.Grievance.Single(g => g.GrievanceId == review.GrievanceId);
            grievance.ReviewedDate = review.ReviewedDate;
            grievance.ReviewedBy = review.ReviewedBy;
            grievance.GrievanceDispositionLookup = review.DispositionLookup;
            grievance.GrievanceDisposition = review.Disposition;
            grievance.InmateResponseNote = review.InmateResponseNote;
            grievance.SensitiveMaterial = review.SensitiveMaterial;

            List<Lookup> lookupLst = _context.Lookup.Where(l => l.LookupType == "GRIEVFLAG" && l.LookupInactive != 1).ToList();

            lookupLst.ForEach(item =>
            {

                InsertUpdateGrievanceFlag(item, review);
            });

            return await _context.SaveChangesAsync();

        }

        public void InsertUpdateGrievanceFlag(Lookup lookup, GrievanceReview review)
        {
            GrievanceFlag grievanceFlag = _context.GrievanceFlag
                                              .FirstOrDefault(g => g.GrievanceId == review.GrievanceId
                                                                   && g.GrievanceFlagText ==
                                                                   lookup.LookupDescription) ??
                                          new GrievanceFlag
                                          {
                                              GrievanceId = review.GrievanceId,
                                              GrievanceFlagText = lookup.LookupDescription,
                                              CreateDate = DateTime.Now,
                                              CreateBy = 1
                                          };
            if (review.LstGrievanceFlag.Any(lk => lk == lookup.LookupDescription))
            {
                grievanceFlag.DeleteFlag = 0;
            }
            else
            {
                grievanceFlag.DeleteDate = DateTime.Now;
                grievanceFlag.DeleteBy = 1;
                grievanceFlag.DeleteFlag = 1;
            }
            if (grievanceFlag.GrievanceFlagId <= 0)
            {
                _context.Add(grievanceFlag);
            }
            _context.SaveChanges();

        }

        public GrievanceDetails GetAdditionalDetails(int grievanceId)
        {

            GrievanceDetails grievanceDetails = new GrievanceDetails();

            Grievance grievance = _context.Grievance.Single(g => g.GrievanceId == grievanceId);
            grievanceDetails.Department = grievance.Department;

            grievanceDetails.CreatePersonnel = _context.Personnel.Where(p => p.PersonnelId == grievance.CreatedBy).
                   Select(g => new PersonnelVm
                   {
                       PersonFirstName = g.PersonNavigation.PersonFirstName,
                       PersonLastName = g.PersonNavigation.PersonLastName,
                       PersonMiddleName = g.PersonNavigation.PersonMiddleName,
                       OfficerBadgeNumber = g.OfficerBadgeNum
                   }).SingleOrDefault();

            List<KeyValuePair<int, int>> inmateLstId = _context.GrievanceInmate.Where(i => i.GrievanceId == grievanceId).
                 Select(i => new KeyValuePair<int, int>(i.GrievanceInmateId, i.InmateId)).ToList();

            int[] inmLst = inmateLstId.Select(s => s.Value).ToArray();

            grievanceDetails.AdditionalInmateLst = _context.Inmate.Where(i =>inmLst.Contains(i.InmateId)).
                 Select(i => new PersonInfoVm
                 {
                     PersonLastName = i.Person.PersonLastName,
                     PersonFirstName = i.Person.PersonFirstName,
                     PersonMiddleName = i.Person.PersonMiddleName,
                     InmateNumber = i.InmateNumber,
                     Facility = i.Facility.FacilityAbbr,
                     HousingLocation =i.HousingUnit.HousingUnitLocation,
                     HousingUnitNumber = i.HousingUnit.HousingUnitNumber,
                     HousingUnitBed= i.HousingUnit.HousingUnitBedNumber,
                     GrvInmateId = inmateLstId.Single(de => de.Value == i.InmateId).Key,
                     InmateId = i.InmateId
                 }).ToList();

            //List<int> personnelIdLst = _context.GrievancePersonnel.Where(g => g.GrievanceId == grievanceId).
            //    Select(i => i.PersonnelId).ToList();
            List<KeyValuePair<int, int>> personnelIdLst = _context.GrievancePersonnel.Where(g => g.GrievanceId == grievanceId).
            Select(i => new KeyValuePair<int, int>(i.GrievancePersonnelId, i.PersonnelId)).ToList();

            //List<int> lstPersonnel = personnelIdLst.Select(p => p.Value).ToList();

            List<PersonnelVm> lstPersonDetails = _personService.GetPersonNameList(personnelIdLst.Select(p => p.Value).ToList());

            grievanceDetails.PersonnelLst = lstPersonDetails.Select(p => new PersonnelVm
            {
                PersonFirstName = p.PersonFirstName,
                PersonLastName = p.PersonLastName,
                PersonMiddleName = p.PersonMiddleName,
                PersonnelNumber = p.PersonnelNumber,
                GrvPersonnelId = personnelIdLst.Single(psl => psl.Value == p.PersonnelId).Key,
                PersonnelId = p.PersonnelId
            }).ToList();

            grievanceDetails.ReviewDetails = new GrievanceReview();

            if (grievance.ReviewedBy.HasValue)
            {
                grievanceDetails.ReviewDetails.ReviewOfficer = _context.Personnel.Where(p => p.PersonnelId == grievance.ReviewedBy).
                   Select(g => new PersonnelVm
                   {
                       PersonFirstName = g.PersonNavigation.PersonFirstName,
                       PersonLastName = g.PersonNavigation.PersonLastName,
                       PersonMiddleName = g.PersonNavigation.PersonMiddleName,
                       PersonnelId = g.PersonnelId,
                       OfficerBadgeNumber = g.OfficerBadgeNum
                   }).SingleOrDefault();
            }
            grievanceDetails.ReviewDetails.ReviewedBy = grievance.ReviewedBy > 0 ? grievance.ReviewedBy : _personnelId;
            grievanceDetails.ReviewDetails.ReviewedDate = grievance.ReviewedDate;
            grievanceDetails.ReviewDetails.Disposition = grievance.GrievanceDisposition;
            grievanceDetails.ReviewDetails.DispositionLookup = grievance.GrievanceDispositionLookup;
            grievanceDetails.ReviewDetails.InmateResponseNote = grievance.InmateResponseNote;

            //grievanceDetails.ReviewDetails.LstGrievanceFlag = _context.GrievanceFlag
            //     .Where(g => g.GrievanceId == grievanceId && g.DeleteFlag != 1)
            //     .Select(g => new KeyValuePair<int, string>(g.GrievanceFlagId, g.GrievanceFlagText)).ToList();

            grievanceDetails.ReviewDetails.LstGrievanceFlag = _context.GrievanceFlag
                 .Where(g => g.GrievanceId == grievanceId && g.DeleteFlag != 1)
                 .Select(g => g.GrievanceFlagText).ToList();

            grievanceDetails.AppealDetails = _context.GrievanceAppeal.Where(a => a.GrievanceId == grievanceId)
                .Select(a => new GrievanceAppealVm
                {
                    AppealId = a.GrievanceAppealId,
                    AppealDate = a.AppealDate,
                    AppealNote = a.AppealNote,
                    ReviewDate = a.ReviewDate,
                    DeleteFlag = a.DeleteFlag ?? 0
                }).ToList();

            grievanceDetails.lstGrievanceForms = GetGrievanceForms(grievanceId);

            grievanceDetails.AttachmentLst = _context.AppletsSaved.Where(a => a.GrievanceId == grievanceId)
                .Select(a => new AttachmentDetails
                {
                    CreateDate = a.CreateDate,
                    Title = a.AppletsSavedTitle,
                    SavedId = a.AppletsSavedId,
                    DeleteFlag = a.AppletsDeleteFlag,
                    GrievanceId = a.GrievanceId,
                    FilePath = a.AppletsSavedPath,
                    Type = a.AppletsSavedType,
                    CreatedBy = new PersonnelVm
                    {
                        PersonFirstName = a.CreatedByNavigation.PersonNavigation.PersonFirstName,
                        PersonLastName = a.CreatedByNavigation.PersonNavigation.PersonLastName,
                        OfficerBadgeNumber = a.CreatedByNavigation.OfficerBadgeNum
                    },
                }).ToList();

            return grievanceDetails;

        }

        public List<IncarcerationForms> GetGrievanceForms(int grievanceId)
        {
            List<IncarcerationForms> forms = _context.FormRecord.Where(w => w.GrievanceId == grievanceId)
                .Select(s => new IncarcerationForms
                {
                    FormRecordId = s.FormRecordId,
                    DisplayName = s.FormTemplates.DisplayName,
                    FormNotes = s.FormNotes,
                    DeleteFlag = s.DeleteFlag,
                    XmlData = s.XmlData,
                    FormCategoryFolderName = s.FormTemplates.FormCategory.FormCategoryFolderName,
                    HtmlFileName = s.FormTemplates.HtmlFileName
                }).OrderByDescending(f => f.CreateDate).ToList();
            return forms;
        }

        public GrievanceHousingDetails GetHousingDetails(int facilityId)
        {
            GrievanceHousingDetails housingDetails = new GrievanceHousingDetails();

            List<HousingUnit> housingUnit = _context.HousingUnit.Where(w =>
                  w.FacilityId == facilityId && (!w.HousingUnitInactive.HasValue || w.HousingUnitInactive==0)).ToList();

            housingDetails.HousingLocationList = housingUnit
                    .OrderBy(o => o.HousingUnitLocation)
                    .GroupBy(g => g.HousingUnitLocation)
                    .Select(s => s.Key).ToList();
            housingDetails.HousingNumberList = housingUnit.Where(h=>!string.IsNullOrEmpty(h.HousingUnitNumber))
                    .OrderBy(o => o.HousingUnitNumber).GroupBy(g =>
                        new { g.HousingUnitListId, g.HousingUnitLocation, g.HousingUnitNumber })
                    .Select(s => new HousingDetail
                    {
                        HousingUnitListId = s.Key.HousingUnitListId,
                        HousingUnitLocation = s.Key.HousingUnitLocation,
                        HousingUnitNumber = s.Key.HousingUnitNumber
                    }).ToList();

            housingDetails.HousingBedNumberList = housingUnit
                .GroupBy(g => new { g.HousingUnitListId, g.HousingUnitBedNumber })
                .Select(s => new KeyValuePair<int, string>
                    (s.Key.HousingUnitListId, s.Key.HousingUnitBedNumber))
                .ToList();

            housingDetails.LocationList = _context.Privileges.Where(w => w.InactiveFlag == 0
                    && (w.FacilityId == facilityId || !w.FacilityId.HasValue))
                .OrderBy(o => o.PrivilegeDescription)
                .Select(s => new KeyValuePair<int, string>
                    (s.PrivilegeId, s.PrivilegeDescription)).ToList();
            return housingDetails;
        }

        public async Task<int> UpdateGrvSetReview(GrievanceVm grvcDetail)
        {
            Grievance grievance = _context.Grievance.Single(g => g.GrievanceId == grvcDetail.GrievanceId);
            grievance.SetReview = grvcDetail.SetReview;
            return await _context.SaveChangesAsync();

        }

        public List<PrebookAttachment> LoadGrievanceAttachments(int grievanceId) => _context
          .AppletsSaved.Where(a => a.GrievanceId == grievanceId
                                   && a.AppletsDeleteFlag == 0)
          .OrderByDescending(a => a.CreateDate)
          .Select(a => new PrebookAttachment
          {
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
              CreatedBy = new PersonnelVm
              {
                  PersonLastName = a.CreatedByNavigation.PersonNavigation.PersonLastName,
                  PersonFirstName = a.CreatedByNavigation.PersonNavigation.PersonFirstName,
                  PersonMiddleName = a.CreatedByNavigation.PersonNavigation.PersonMiddleName
              },
              UpdateDate = a.UpdateDate,
              UpdatedBy = new PersonnelVm
              {
                  PersonLastName = a.UpdatedByNavigation.PersonNavigation.PersonLastName,
                  PersonFirstName = a.UpdatedByNavigation.PersonNavigation.PersonFirstName,
                  PersonMiddleName = a.UpdatedByNavigation.PersonNavigation.PersonMiddleName
              }
          }).ToList();

        public GrievanceReport GetGrievanceReport(int grievanceId)
        {
            GrievanceReport grievanceRpt = new GrievanceReport
            {
                Grievance = GetGrievance(grievanceId)
            };
            grievanceRpt.PhotoPath = _photoService.GetPhotoByPersonId(grievanceRpt.Grievance.PersonId);
            grievanceRpt.InmateDetails = _context.Inmate
                .Where(i => i.InmateId == grievanceRpt.Grievance.InmateId)
                .Select(s => new PersonInfoVm
                {
                    PersonLastName = s.Person.PersonLastName,
                    PersonFirstName = s.Person.PersonFirstName,
                    PersonMiddleName = s.Person.PersonMiddleName,
                    Facility = s.Facility.FacilityAbbr,
                    HousingLocation = s.HousingUnit.HousingUnitLocation,
                    HousingUnitNumber = s.HousingUnit.HousingUnitNumber,
                    HousingUnitBed = s.HousingUnit.HousingUnitBedNumber,
                    InmateNumber =  s.InmateNumber,
                    Classification = s.InmateClassification.InmateClassificationReason
                }).Single();
            grievanceRpt.GrievanceDetail = GetAdditionalDetails(grievanceId);
            grievanceRpt.PersonnelDetails = _context.Personnel.Where(w=>w.PersonnelId == _personnelId).Select(s => new PersonnelVm
            {
                PersonLastName = s.PersonNavigation.PersonLastName,
                PersonFirstName =  s.PersonNavigation.PersonFirstName,
                AgencyId = s.AgencyId,
                PersonnelNumber = s.OfficerBadgeNum
            }).FirstOrDefault();
            if (grievanceRpt.PersonnelDetails!= null)
            {
                grievanceRpt.PersonnelDetails.AgencyName = _context.Agency
                    .FirstOrDefault(w => w.AgencyBookingFlag)?.AgencyName;
            }

            grievanceRpt.GrievanceAppealDetails = _context.GrievanceAppeal.Where(a => a.GrievanceId == grievanceId)
                .Select(a => new GrievanceAppealDetails
                {
                    GrievanceAppealId = a.GrievanceAppealId,
                    AppealDate = a.AppealDate,
                    AppealNote = a.AppealNote,
                    AppealCategoryLookup = a.AppealCategoryLookup,
                    Category = _context.Lookup.SingleOrDefault(w =>
                            w.LookupInactive == 0 && w.LookupType == LookupConstants.GRVAPPEALTYPE
                                                  && w.LookupIndex == a.AppealCategoryLookup)
                        .LookupDescription,
                    ReviewDate = a.ReviewDate,
                    InmateResponseNote = a.InmateResponseNote,
                    ReviewByLastName = a.ReviewByNavigation.PersonNavigation.PersonLastName,
                    ReviewByBadgeNumber = a.ReviewByNavigation.OfficerBadgeNum,
                    ReviewNote = a.ReviewNote,
                    CreateDate = a.CreateDate,
                    AppealByLastName = a.AppealByNavigation.PersonNavigation.PersonLastName,
                    AppealByBadgeNumber = a.AppealByNavigation.OfficerBadgeNum
                }).FirstOrDefault();
            return grievanceRpt;
        }

        public async Task<int> DeleteUndoByGrievanceId(GrievanceVm grvcDetail)
        {
            if (grvcDetail.DeleteFlag == 1)
            {
                Grievance deletegrievance = _context.Grievance.Single(w => w.GrievanceId == grvcDetail.GrievanceId);
                deletegrievance.DeleteFlag = 1;
            }
            else
            {
                Grievance deletegrievance = _context.Grievance.Single(w => w.GrievanceId == grvcDetail.GrievanceId);
                deletegrievance.DeleteFlag = 0;
            }
            return await _context.SaveChangesAsync();
        }

        // public async Task<int> DeleteGrievance(int grievanceId, string disposition, string dispositionNote)
        // {
        //     Grievance grievance = _context.Grievance.Single(a => a.GrievanceId == grievanceId);
        //     grievance.DeleteFlag = 1;
        //     grievance.GrievanceDispositionLookup = disposition;
        //     grievance.GrievanceDisposition = dispositionNote;
        //     return await _context.SaveChangesAsync();
        // }
        public async Task<int> DeleteGrievance(CancelGrievance cancelGrievance)
        {
            Grievance grievance = _context.Grievance.Single(a => a.GrievanceId == cancelGrievance.GrievanceId);
            grievance.DeleteFlag = 1;
            grievance.GrievanceDispositionLookup = cancelGrievance.Disposition;
            grievance.GrievanceDisposition = cancelGrievance.DispositionNote;
            return await _context.SaveChangesAsync();
        }

        public GrievanceVm GetGrievance(int grievanceId)
        {
            List<Lookup> lsLookups = _context.Lookup.Where(lk =>
                lk.LookupType == LookupConstants.GRIEVTYPE).ToList();
            GrievanceVm grievance = _context.Grievance.Where(g => g.GrievanceId == grievanceId)
                    .Select(s => new GrievanceVm
                    {
                        DateOccured = s.DateOccured,
                        InmateId = s.InmateId,
                        GrievanceId = s.GrievanceId,
                        GrievanceSummary = s.GrievanceSummary,
                        GrievanceWizardStep = s.GrievanceWizardStep,
                        ReadyForReview = s.ReadyForReview,
                        SetReview = s.SetReview,
                        GrievanceLocation = s.GrievanceLocation,
                        GrievanceLocationId = s.GrievanceLocationId ?? 0,
                        HousingUnitLocation = s.GrievanceHousingUnitLocation,
                        HousingUnitNumber = s.GrievanceHousingUnitNumber,
                        HousingUnitBed = s.GrievanceHousingUnitBed,
                        GrievanceDispositionLookup = s.GrievanceDispositionLookup,
                        GrievanceNumber = s.GrievanceNumber,
                        GrievanceType = s.GrievanceType,
                        GrievanceTypeName = lsLookups.SingleOrDefault(lk =>
                                lk.LookupIndex == s.GrievanceType &&
                                lk.LookupType == LookupConstants.GRIEVTYPE)
                            .LookupDescription,
                        AppealDate = s.AppealDate,
                        OtherLocation = s.GrievanceLocationOther,
                        CreatedBy = s.CreatedBy,
                        Department = s.Department,
                        FacilityAbbr = s.Facility.FacilityAbbr,
                        CreateDate = s.CreateDate,
                        ReviewedDate = s.ReviewedDate,
                        PersonId = s.Inmate.PersonId,
                        Person = new PersonInfoVm
                        {
                            PersonLastName = s.Inmate.Person.PersonLastName,
                            PersonFirstName = s.Inmate.Person.PersonFirstName,
                            InmateNumber = s.Inmate.InmateNumber
                        },
                        CreatedByPerson = new PersonnelVm
                        {
                            PersonLastName = s.CreatedByNavigation.PersonNavigation.PersonLastName,
                            OfficerBadgeNumber = s.CreatedByNavigation.OfficerBadgeNumber
                        },
                        ReviewedByPerson = new PersonnelVm
                        {
                            PersonLastName = s.ReviewedByNavigation.PersonNavigation.PersonLastName,
                            OfficerBadgeNumber = s.ReviewedByNavigation.OfficerBadgeNum
                        },
                        ReceiveDate = s.ReceiveDate
                    }).Single();
            return grievance;
        }
    }
}
