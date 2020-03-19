﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ServerAPI.ViewModels;
using GenerateTables.Models;
using ServerAPI.Utilities;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace ServerAPI.Services
{
    public class InmateIncidentService : IInmateIncidentService
    {
        private readonly AAtims _context;
        private readonly ICommonService _commonService;
        private readonly IIncidentService _incidentService;
        private readonly int _personnelId;
        private readonly IInterfaceEngineService _interfaceEngine;

        public InmateIncidentService(AAtims context, ICommonService commonService,
            IHttpContextAccessor httpContextAccessor, IIncidentService incidentService,
            IInterfaceEngineService interfaceEngine)
        {
            _context = context;
            _commonService = commonService;
            _incidentService = incidentService;
            _personnelId = Convert.ToInt32(httpContextAccessor.HttpContext.User.FindFirst("personnelId")?.Value);
            Claim facilityId = httpContextAccessor.HttpContext.User.FindFirst(AuthDetailConstants.FACILITYID);
            int.TryParse(facilityId?.Value, out int _);
            _interfaceEngine = interfaceEngine;
        }

        public List<IncidentViewer> GetInmateIncidentList(int inmateId)
        {
            List<IncidentViewer> incidentViewers = _context.DisciplinaryInmate.
                Where(d => d.InmateId == inmateId).
                Select(d => new IncidentViewer
                {
                    DisciplinaryIncidentId = d.DisciplinaryIncidentId,
                    DisciplinaryInmateId = d.DisciplinaryInmateId,
                    IncidentNumber = d.DisciplinaryIncident.DisciplinaryNumber,
                    DisciplinaryTypeId = d.DisciplinaryIncident.DisciplinaryType,
                    IncidentDate = d.DisciplinaryIncident.DisciplinaryIncidentDate,
                    DispLocation = d.DisciplinaryIncident.DisciplinaryLocation,
                    DispHousingUnitLocation = d.DisciplinaryIncident.DisciplinaryHousingUnitLocation,
                    DispHousingUnitNumber = d.DisciplinaryIncident.DisciplinaryHousingUnitNumber,
                    DispHousingUnitBedNumber = d.DisciplinaryIncident.DisciplinaryHousingUnitBed,
                    ReportDate = d.DisciplinaryIncident.DisciplinaryReportDate,
                    BypassHearing = d.DisciplinaryInmateBypassHearing,
                    ScheduleHearingDate = d.DisciplinaryScheduleHearingDate,
                    HearingDate = d.DisciplinaryHearingDate,
                    ReviewDate = d.DisciplinaryReviewDate,
                    HearingComplete = d.HearingComplete,
                    InmateTypeId = d.DisciplinaryInmateType,
                    DisciplinaryActive = d.DisciplinaryIncident.DisciplinaryActive,
                    DispIncidentId = d.DisciplinaryIncident.DisciplinaryIncidentId,
                    DispInmateId = d.DisciplinaryInmateId,
                    AllowHearingFlag = d.DisciplinaryIncident.AllowHearingFlag > 0,
                    DispFreeForm = d.DisciplinaryIncident.DisciplinaryLocationOther,
                    DispOtherLocationId = d.DisciplinaryIncident.OtherLocationID,
                    DispOtherLocationName = d.DisciplinaryIncident.OtherLocation.LocationName,
                    DispAltSentSiteId = d.DisciplinaryIncident.DisciplinaryAltSentSiteId,
                    DispAltSentSiteName = _context.AltSentSite.SingleOrDefault(x => x.AltSentSiteId ==
                    d.DisciplinaryIncident.DisciplinaryAltSentSiteId).AltSentSiteName,
                    SupervisorReviewFlag = d.DisciplinaryIncident.DisciplinaryIncidentNarrative
                    .Select(s => s.SupervisorReviewFlag == 1).FirstOrDefault(),
                    Personnel = d.InmateId > 0 ? new PersonVm
                    {
                        PersonLastName = d.Inmate.Person.PersonLastName,
                        PersonFirstName = d.Inmate.Person.PersonFirstName,
                        InmateNumber = d.Inmate.InmateNumber
                    } : d.PersonnelId > 0 ? new PersonVm
                    {
                        PersonLastName = d.Personnel.PersonNavigation.PersonLastName,
                        PersonFirstName = d.Personnel.PersonNavigation.PersonFirstName,
                        OfficerBadgeNumber = d.Personnel.OfficerBadgeNumber
                    } : default,
                    InmateId = d.InmateId,
                    PersonnelId = d.PersonnelId,
                    DispOtherName = d.DisciplinaryOtherName,
                    DispSynopsis = d.DisciplinaryIncident.DisciplinarySynopsis,
                    CompleteOfficer = new PersonnelVm
                    {
                        PersonLastName = d.DisciplinaryIncident.DisciplinaryOfficer
                        .PersonNavigation.PersonLastName,
                        PersonFirstName = d.DisciplinaryIncident.DisciplinaryOfficer
                        .PersonNavigation.PersonFirstName,
                        OfficerBadgeNumber = d.DisciplinaryIncident.DisciplinaryOfficer.OfficerBadgeNumber
                    },
                    DispOfficerNarrativeFlag = d.DisciplinaryIncident.DisciplinaryOfficerNarrativeFlag ?? false
                }).OrderByDescending(x => x.IncidentDate).ToList();

            //To get Lookupdesc in Lookup
            List<LookupVm> lstLookup = _commonService.GetLookups(new[] { LookupConstants.DISCTYPE,
                LookupConstants.DISCINTYPE });

            List<AoWizardProgressVm> incidentWizardProgresses =
               _incidentService.GetIncidentWizardProgress(incidentViewers
                   .Select(a => a.DispInmateId).ToArray());

            incidentViewers.ForEach(item =>
            {
                item.Count = _context.FormRecord.Count(a =>
                    (a.DisciplinaryInmateId == item.DispInmateId || a.DisciplinaryControlId == item.DispIncidentId)
                    && a.DeleteFlag == 0);

                item.DisciplinaryType = lstLookup.SingleOrDefault(l => l.LookupType == LookupConstants.DISCTYPE
                    && l.LookupIndex == item.DisciplinaryTypeId)?.LookupDescription;

                item.InmateType = lstLookup.SingleOrDefault(l => l.LookupType == LookupConstants.DISCINTYPE
                    && l.LookupIndex == item.InmateTypeId)?.LookupDescription;

                item.ActiveIncidentProgress =
                incidentWizardProgresses.FirstOrDefault(x => x.DisciplinaryInmateId == item.DispInmateId);
            });
            return incidentViewers;
        }

        public InmateIncidentDropdownList GetInmateIncidentDropdownList(bool presetFlag, int facilityId)
        {
            List<LookupVm> lstLookUp = _commonService.GetLookups(new[] {
                LookupConstants.DISCTYPE,
                LookupConstants.DISCINTYPE,
                LookupConstants.INCFLAG,
                LookupConstants.INCCAT
            });
            InmateIncidentDropdownList incidentList = new InmateIncidentDropdownList
            {
                LookupIncidentType = lstLookUp
                    .Where(x => x.LookupType == LookupConstants.DISCTYPE)
                    .OrderByDescending(x => x.LookupOrder)
                    .ThenBy(x => x.LookupDescription).ToList(),

                LookupDiscInmateType = lstLookUp
                    .Where(x => x.LookupType == LookupConstants.DISCINTYPE)
                    .OrderByDescending(x => x.LookupOrder)
                    .ThenBy(x => x.LookupDescription).ToList(),

                LookupCategorizationType = lstLookUp
                    .Where(x => x.LookupType == LookupConstants.INCCAT)
                    .OrderByDescending(x => x.LookupOrder)
                    .ThenBy(x => x.LookupDescription).ToList(),

                IncidentHousingBuildingList =
                    _context.HousingUnit.Where(x => x.HousingUnitInactive == 0 || !x.HousingUnitInactive.HasValue)
                        .Select(x => new KeyValuePair<int, string>(x.FacilityId,
                            x.HousingUnitLocation)).Distinct().OrderBy(s => s.Value).ToList(),

                IncidentHousingLocationList = _context.Privileges.Where(p => p.InactiveFlag == 0 &&
                p.RemoveFromTrackingFlag == 0 && p.RemoveFromPrivilegeFlag == 1 && p.FacilityId == facilityId)
                .OrderBy(a => a.PrivilegeDescription)
                .Select(x => new KeyValuePair<int, string>(x.PrivilegeId, x.PrivilegeDescription)).ToList(),

                IncidentOtherLocationList = _context.OtherLocation.Where(p => p.DeleteFlag == false
                && p.LocationName != null && p.FacilityId == facilityId)
                .OrderBy(a => a.OtherLocationId)
                .Select(x => new KeyValuePair<int, string>(x.OtherLocationId, x.LocationName)).ToList()
            };
            if (presetFlag) return incidentList;
            incidentList.LookupIncidentFlag = lstLookUp
                .Where(x => x.LookupType == LookupConstants.INCFLAG)
                .OrderByDescending(x => x.LookupOrder)
                .ThenBy(x => x.LookupDescription)
                .Select(x => new KeyValuePair<int, string>(x.LookupIndex,
                    x.LookupDescription)).ToList();

            incidentList.IncidentAltSentSiteList = _context.AltSentSite.Select(x => new IncidentAltSentSite
            {
                AltSentSiteId = x.AltSentSiteId,
                AltSentSiteName = x.AltSentSiteName,
                FacilityId = x.AltSentProgram.FacilityId
            }).ToList();
            return incidentList;
        }

        public List<string> GetHousingUnitNumber(int facilityId, string housingUnitLocation) =>
            _context.HousingUnit
                .Where(h => h.FacilityId == facilityId && h.HousingUnitLocation == housingUnitLocation &&
                    !string.IsNullOrEmpty(h.HousingUnitNumber))
                .OrderBy(s => s.HousingUnitNumber)
                .Select(x => x.HousingUnitNumber)
                .Distinct().ToList();

        public List<string> GetHousingUnitBedNumber(int facilityId, string housingUnitLocation,
            string housingUnitNumber) => _context.HousingUnit.Where(h =>
                h.FacilityId == facilityId && h.HousingUnitLocation == housingUnitLocation &&
                h.HousingUnitNumber == housingUnitNumber && h.HousingUnitBedNumber != null)
            .OrderBy(s => TryParseInt(s.HousingUnitBedNumber)).ThenBy(s => s.HousingUnitBedNumber)
            .Select(x => x.HousingUnitBedNumber)
            .Distinct().ToList();

        //TODO ???
        private static int? TryParseInt(string s)
        {
            bool number = int.TryParse(s, out int i);
            return number ? i : (int?)null;
        }

        public List<KeyValuePair<int, string>> GetLocation(int facilityId) => _context.Privileges.Where(p =>
                p.InactiveFlag == 0 && p.RemoveFromTrackingFlag == 0 &&
                (p.FacilityId == 0 || !p.FacilityId.HasValue || p.FacilityId == facilityId))
            .OrderBy(s => TryParseInt(s.PrivilegeDescription)).ThenBy(s => s.PrivilegeDescription)
            .Select(x => new KeyValuePair<int, string>(x.PrivilegeId, x.PrivilegeDescription)).ToList();

        public async Task<int> InsertInmateIncident(InmateIncidentInfo incidentDetails)
        {
            string getIncidentNumber = _commonService.GetGlobalNumber((int)AtimsGlobalNumber.IncidentNumber);
            DisciplinaryIncident discIncident = new DisciplinaryIncident
            {
                DisciplinaryIncidentDate = incidentDetails.IncidentDate,
                DisciplinaryNumber = getIncidentNumber,
                DisciplinaryType = incidentDetails.DisciplinaryTypeId,
                DisciplinaryActive = 1,
                DisciplinaryOfficerId = incidentDetails.DispOfficerId,
                DisciplinaryReportDate = incidentDetails.ReportDate,
                CreateDate = DateTime.Now,
                UpdateDate = DateTime.Now,
                DisciplinarySynopsis = incidentDetails.DispSynopsis,
                FacilityId = incidentDetails.FacilityId,
                DisciplinaryLocation = incidentDetails.DispLocation,
                DisciplinaryHousingUnitLocation = incidentDetails.DispHousingUnitLocation,
                DisciplinaryHousingUnitNumber = incidentDetails.DispHousingUnitNumber,
                DisciplinaryHousingUnitBed = incidentDetails.DispHousingUnitBedNumber,
                //DisciplinaryLocationOther = incidentDetails.DispOtherLocation,
                DisciplinaryLocationOther = incidentDetails.DispFreeForm,
                DisciplinaryAltSentSiteId = incidentDetails.DispAltSentSiteId,
                DisciplinaryLocationId = incidentDetails.DispLocationId,
                SensitiveMaterial = incidentDetails.SensitiveMaterial,
                PreaOnly = incidentDetails.PreaOnly,
                OtherLocationID = incidentDetails.DispOtherLocationId,
                IncidentCategorizationIndex = incidentDetails.CategorizationIndex
            };
            _context.DisciplinaryIncident.Add(discIncident);
            DisciplinaryInmate disInmate = new DisciplinaryInmate
            {
                InmateId = incidentDetails.InmateId,
                DisciplinaryOtherName = incidentDetails.DispOtherName,
                DisciplinaryInmateType = incidentDetails.DispInmateType,
                DisciplinaryInmateBypassHearing = incidentDetails.BypassHearing,
                DisciplinaryIncidentId = discIncident.DisciplinaryIncidentId,
                PersonnelId = incidentDetails.PersonnelId,
                NarrativeFlag = incidentDetails.NarrativeFlag,
                NarrativeFlagNote = incidentDetails.NarrativeFlagNote
            };
            _context.DisciplinaryInmate.Add(disInmate);
            _context.DisciplinaryIncidentFlag.AddRange(incidentDetails.DispIncidentFlag
                    .Select(incidentFlag => new DisciplinaryIncidentFlag
                    {
                        DisciplinaryIncidentId = discIncident.DisciplinaryIncidentId,
                        IncidentFlagText = incidentFlag,
                        CreateDate = DateTime.Now,
                        CreateBy = 1
                    }).ToList());


            if (incidentDetails.LookupFlag == 1)
            {
                HousingSearch housingSearch = new HousingSearch
                {
                    DisciplinaryIncidentID = discIncident.DisciplinaryIncidentId,
                    HousingUnitId = null,
                    OtherLocationId = null,
                    LocationId = null,
                    SearchDate = discIncident.DisciplinaryIncidentDate.Value,
                    ContrabandFound = incidentDetails.ContrabandFound,
                    ContrabandNote = incidentDetails.ContrabandNote,
                    CreateBy = _personnelId,
                    CreateDate = DateTime.Now,
                    DeleteFlag = false
                };
                if (incidentDetails.DispOtherLocationId != null)
                {
                    housingSearch.OtherLocationId = incidentDetails.DispOtherLocationId;
                    _context.HousingSearch.Add(housingSearch);
                }
                else if (incidentDetails.DispLocationId != null)
                {
                    housingSearch.LocationId = incidentDetails.DispLocationId;
                    _context.HousingSearch.Add(housingSearch);
                }
                else if (incidentDetails.CellList.Count > 0)
                {

                    List<HousingUnit> housingUnits = _context.HousingUnit.Where(h => incidentDetails.CellList
                      .Select(c => c.HousingUnitListId).Contains(h.HousingUnitListId)).ToList();
                    incidentDetails.CellList.ForEach(e =>
                    {
                        _context.HousingSearch.AddRange(housingUnits
                            .Where(w => w.HousingUnitListId == e.HousingUnitListId)
                            .Select(x => new HousingSearch
                            {
                                DisciplinaryIncidentID = discIncident.DisciplinaryIncidentId,
                                HousingUnitId = x.HousingUnitId,
                                OtherLocationId = null,
                                LocationId = null,
                                SearchDate = discIncident.DisciplinaryIncidentDate.Value,
                                ContrabandFound = e.ContrabandFound,
                                ContrabandNote = e.ContrabandNote,
                                CreateBy = _personnelId,
                                CreateDate = DateTime.Now,
                                DeleteFlag = false
                            }));
                    });
                }
            }

            await _context.SaveChangesAsync();
            discIncident.DisciplinaryOfficerNarrativeFlag = incidentDetails.DispOfficerNarrativeFlag;
            discIncident.ExpectedNarrativeCount = incidentDetails.DispOfficerNarrativeFlag ?
                incidentDetails.PersonnelId > 0 && incidentDetails.NarrativeFlag ? 2 : 1
                : incidentDetails.PersonnelId > 0 && incidentDetails.NarrativeFlag ? 1 : (int?)null;
            _interfaceEngine.Export(new ExportRequestVm
            {
                EventName = EventNameConstants.INCIDENTCREATE,
                PersonnelId = _personnelId,
                Param1 = _context.Inmate
                        .SingleOrDefault(a => a.InmateId == incidentDetails.InmateId)?.PersonId
                        .ToString() ?? string.Empty,
                Param2 = discIncident.DisciplinaryIncidentId.ToString()
            });
            await _context.SaveChangesAsync();
            return discIncident.DisciplinaryIncidentId;
        }

        public InmateIncidentInfo GetInmateIncident(int incidentId) => _context.DisciplinaryIncident
                .Where(d => d.DisciplinaryIncidentId == incidentId)
                .Select(d => new InmateIncidentInfo
                {
                    DispIncidentId = d.DisciplinaryIncidentId,
                    IncidentDate = d.DisciplinaryIncidentDate,
                    ReportDate = d.DisciplinaryReportDate,
                    DispOfficerId = d.DisciplinaryOfficerId,
                    IncidentNumber = d.DisciplinaryNumber,
                    DisciplinaryActive = d.DisciplinaryActive,
                    DisciplinaryTypeId = d.DisciplinaryType,
                    DispSynopsis = d.DisciplinarySynopsis,
                    FacilityId = d.FacilityId,
                    DispLocation = d.DisciplinaryLocation,
                    DispHousingUnitBedNumber = d.DisciplinaryHousingUnitBed,
                    DispHousingUnitLocation = d.DisciplinaryHousingUnitLocation,
                    DispHousingUnitNumber = d.DisciplinaryHousingUnitNumber,
                    // DispOtherLocation = d.DisciplinaryLocationOther,
                    DispFreeForm = d.DisciplinaryLocationOther,
                    DispOtherLocationId = d.OtherLocationID,
                    DispOtherLocationName = d.OtherLocation.LocationName,
                    DispLocationId = d.DisciplinaryLocationId,
                    DispAltSentSiteId = d.DisciplinaryAltSentSiteId,
                    DispAltSentSiteName = _context.AltSentSite
                        .SingleOrDefault(s => s.AltSentSiteId == d.DisciplinaryAltSentSiteId).AltSentSiteName,
                    SupervisorAction = d.DisciplinarySupervisorAction,
                    DispIncidentFlag = _context.DisciplinaryIncidentFlag
                        .Where(i => i.DisciplinaryIncidentId == incidentId && 
                            (!i.DeleteFlag.HasValue || i.DeleteFlag==0))
                        .Select(i => i.IncidentFlagText).ToArray(),
                    DispOfficerNarrativeFlag = (bool)d.DisciplinaryOfficerNarrativeFlag,
                    CategorizationIndex = d.IncidentCategorizationIndex,
                    ContrabandFound = d.HousingSearch.Select(x => x.ContrabandFound).FirstOrDefault(),
                    ContrabandNote = d.HousingSearch.Select(x => x.ContrabandNote).FirstOrDefault(),
                    SelectedIncidentType = 0,
                    SensitiveMaterial = d.SensitiveMaterial,
                    PreaOnly = d.PreaOnly
                }).Single();

        // Update existing disciplinary incident details.
        public async Task<int> UpdateInmateIncident(InmateIncidentInfo inmateIncidentInfo)
        {
            DisciplinaryIncident disciplinaryIncident = _context.DisciplinaryIncident
                .Single(a => a.DisciplinaryIncidentId == inmateIncidentInfo.DispIncidentId);
            disciplinaryIncident.DisciplinaryIncidentDate = inmateIncidentInfo.IncidentDate;
            disciplinaryIncident.DisciplinaryType = inmateIncidentInfo.DisciplinaryTypeId;
            disciplinaryIncident.DisciplinaryOfficerId = inmateIncidentInfo.DispOfficerId;
            disciplinaryIncident.DisciplinaryReportDate = inmateIncidentInfo.ReportDate;
            disciplinaryIncident.UpdateDate = DateTime.Now.Date;
            disciplinaryIncident.DisciplinarySynopsis = inmateIncidentInfo.DispSynopsis;
            disciplinaryIncident.DisciplinaryLocation = inmateIncidentInfo.DispLocation;
            disciplinaryIncident.DisciplinaryLocationId = inmateIncidentInfo.DispLocationId;
            disciplinaryIncident.DisciplinaryHousingUnitLocation = inmateIncidentInfo.DispHousingUnitLocation;
            disciplinaryIncident.DisciplinaryHousingUnitNumber = inmateIncidentInfo.DispHousingUnitNumber;
            disciplinaryIncident.DisciplinaryHousingUnitBed = inmateIncidentInfo.DispHousingUnitBedNumber;
            // disciplinaryIncident.DisciplinaryLocationOther = inmateIncidentInfo.DispOtherLocation;
            disciplinaryIncident.DisciplinaryAltSentSiteId = inmateIncidentInfo.DispAltSentSiteId;
            disciplinaryIncident.DisciplinarySupervisorAction = inmateIncidentInfo.SupervisorAction;
            disciplinaryIncident.DisciplinaryOfficerNarrativeFlag = inmateIncidentInfo.DispOfficerNarrativeFlag;
            disciplinaryIncident.SensitiveMaterial = inmateIncidentInfo.SensitiveMaterial;
            disciplinaryIncident.PreaOnly = inmateIncidentInfo.PreaOnly;
            disciplinaryIncident.IncidentCategorizationIndex = inmateIncidentInfo.CategorizationIndex;
            disciplinaryIncident.OtherLocationID = inmateIncidentInfo.DispOtherLocationId;
            disciplinaryIncident.DisciplinaryLocationOther = inmateIncidentInfo.DispFreeForm;
            int count= _context.DisciplinaryInmate
                .Count(a => a.DisciplinaryIncidentId == inmateIncidentInfo.DispIncidentId
                            && a.NarrativeFlag == true && a.PersonnelId > 0 &&
                            (!a.DeleteFlag.HasValue || a.DeleteFlag == 0));
            disciplinaryIncident.ExpectedNarrativeCount =
                !inmateIncidentInfo.DispOfficerNarrativeFlag ? count : ++count;
            _context.DisciplinaryIncidentFlag.Where(a =>
                    !a.DeleteFlag.HasValue && a.DisciplinaryIncidentId == inmateIncidentInfo.DispIncidentId)
                .ToList().ForEach(a =>
                {
                    a.DeleteFlag = 1;
                    a.DeleteDate = DateTime.Now;
                    a.DeleteBy = _personnelId;
                });

            _context.DisciplinaryIncidentFlag.AddRange(inmateIncidentInfo.DispIncidentFlag
                .Select(flag => new DisciplinaryIncidentFlag
                {
                    DisciplinaryIncidentId = inmateIncidentInfo.DispIncidentId,
                    IncidentFlagText = flag,
                    CreateDate = DateTime.Now,
                    CreateBy = _personnelId
                }));
                
            HousingSearch housingSearchs = _context.HousingSearch.FirstOrDefault(s => s.DisciplinaryIncidentID == inmateIncidentInfo.DispIncidentId);
            if (housingSearchs != null)
            {
                if (inmateIncidentInfo.LookupFlag == 1)
                {
                    if (inmateIncidentInfo.DispOtherLocationId != null || inmateIncidentInfo.DispLocationId != null)
                    {
                        housingSearchs.DisciplinaryIncidentID = inmateIncidentInfo.DispIncidentId;
                        housingSearchs.HousingUnitId = null;
                        housingSearchs.OtherLocationId = null;
                        housingSearchs.LocationId = null;
                        housingSearchs.SearchDate = inmateIncidentInfo.IncidentDate.Value;
                        housingSearchs.ContrabandFound = inmateIncidentInfo.ContrabandFound;
                        housingSearchs.ContrabandNote = inmateIncidentInfo.ContrabandNote;
                        housingSearchs.UpdateBy = _personnelId;
                        housingSearchs.UpdateDate = DateTime.Now;
                        housingSearchs.DeleteFlag = false;
                    }
                    if (inmateIncidentInfo.DispOtherLocationId != null)
                    {
                        housingSearchs.OtherLocationId = inmateIncidentInfo.DispOtherLocationId;
                    }
                    else if (inmateIncidentInfo.DispLocationId != null)
                    {
                        housingSearchs.LocationId = inmateIncidentInfo.DispLocationId;
                    }

                    else if (inmateIncidentInfo.CellList.Count > 0)
                    {
                        List<HousingUnit> housingUnits = _context.HousingUnit.Where(h => inmateIncidentInfo.CellList
                         .Select(c => c.HousingUnitListId).Contains(h.HousingUnitListId)).ToList();

                        List<HousingSearch> housingSearch = _context.HousingSearch.Where(h => !h.LocationId.HasValue &&
                            !h.OtherLocationId.HasValue && !housingUnits
                       .Select(c => c.HousingUnitId).Contains(h.HousingUnitId ?? 0)).ToList();

                        housingUnits.ForEach(ef =>
                        {
                            HousingSearch housingUnitSearch = _context.HousingSearch.First(s =>
                                s.DisciplinaryIncidentID == inmateIncidentInfo.DispIncidentId
                                && s.HousingUnitId == ef.HousingUnitId);
                            housingUnitSearch.OtherLocationId = null;
                            housingUnitSearch.LocationId = null;
                            housingUnitSearch.SearchDate = inmateIncidentInfo.IncidentDate.Value;
                            housingUnitSearch.ContrabandFound = inmateIncidentInfo.CellList
                                .First(w => w.HousingUnitListId == ef.HousingUnitListId).ContrabandFound;
                            housingUnitSearch.ContrabandNote = inmateIncidentInfo.CellList
                                .First(w => w.HousingUnitListId == ef.HousingUnitListId).ContrabandNote;
                            housingUnitSearch.UpdateBy = _personnelId;
                            housingUnitSearch.UpdateDate = DateTime.Now;
                            housingUnitSearch.DeleteFlag = false;
                        });
                        housingSearch.ForEach(fe =>
                        {
                            HousingSearch housingUnitSearch = _context.HousingSearch.First(s =>
                                s.DisciplinaryIncidentID == inmateIncidentInfo.DispIncidentId
                                && s.HousingUnitId == fe.HousingUnitId);
                            housingUnitSearch.DeleteFlag = true;
                            housingUnitSearch.DeleteBy = _personnelId;
                            housingUnitSearch.DeleteDate = DateTime.Now;
                        });
                    }
                }
                else
                {
                    List<HousingSearch> list = _context.HousingSearch.Where(w => w.DisciplinaryIncidentID == inmateIncidentInfo.DispIncidentId).ToList();
                    list.ForEach(f =>
                    {
                        HousingSearch housingUnitSearch = _context.HousingSearch.First(s =>
                            s.DisciplinaryIncidentID == inmateIncidentInfo.DispIncidentId
                            && s.HousingSearchId == f.HousingSearchId);
                        housingUnitSearch.DeleteFlag = true;
                        housingUnitSearch.DeleteBy = _personnelId;
                        housingUnitSearch.DeleteBy = _personnelId;
                    });
                }
            }
            else
            {
                if (inmateIncidentInfo.LookupFlag == 1)
                {
                    HousingSearch housingSearch = new HousingSearch
                    {
                        DisciplinaryIncidentID = inmateIncidentInfo.DispIncidentId,
                        HousingUnitId = null,
                        OtherLocationId = null,
                        LocationId = null,
                        SearchDate = inmateIncidentInfo.IncidentDate.Value,
                        ContrabandFound = inmateIncidentInfo.ContrabandFound,
                        ContrabandNote = inmateIncidentInfo.ContrabandNote,
                        CreateBy = _personnelId,
                        CreateDate = DateTime.Now,
                        DeleteFlag = false
                    };
                    if (inmateIncidentInfo.DispOtherLocationId != null)
                    {
                        housingSearch.OtherLocationId = inmateIncidentInfo.DispOtherLocationId;
                        _context.HousingSearch.Add(housingSearch);
                    }
                    else if (inmateIncidentInfo.DispLocationId != null)
                    {
                        housingSearch.LocationId = inmateIncidentInfo.DispLocationId;
                        _context.HousingSearch.Add(housingSearch);
                    }
                    else if (inmateIncidentInfo.CellList.Count > 0)
                    {
                        List<HousingUnit> housingUnits = _context.HousingUnit.Where(h => inmateIncidentInfo.CellList
                          .Select(c => c.HousingUnitListId).Contains(h.HousingUnitListId)).ToList();
                        inmateIncidentInfo.CellList.ForEach(e =>
                        {
                            _context.HousingSearch.AddRange(housingUnits
                                .Where(w => w.HousingUnitListId == e.HousingUnitListId)
                                .Select(x => new HousingSearch
                                {
                                    DisciplinaryIncidentID = inmateIncidentInfo.DispIncidentId,
                                    HousingUnitId = x.HousingUnitId,
                                    OtherLocationId = null,
                                    LocationId = null,
                                    SearchDate = inmateIncidentInfo.IncidentDate.Value,
                                    ContrabandFound = e.ContrabandFound,
                                    ContrabandNote = e.ContrabandNote,
                                    CreateBy = _personnelId,
                                    CreateDate = DateTime.Now,
                                    DeleteFlag = false
                                }));
                        });
                    }
                }

            }
            return await _context.SaveChangesAsync();
        }

        //To Get Disciplinary Preset details
        public List<DisciplinaryPresetVm> GetDisciplinaryPresetDetails()
        {
            List<LookupVm> lstLookup = _context.Lookup.Where(w => w.LookupType == LookupConstants.DISCTYPE
                    || w.LookupType == LookupConstants.DISCINTYPE || w.LookupType == LookupConstants.INCCAT)
                .Select(s => new LookupVm
                {
                    LookupType = s.LookupType,
                    LookupIndex = s.LookupIndex,
                    LookupDescription = s.LookupDescription
                }).ToList();
            List<DisciplinaryPresetVm> presetList = _context.DisciplinaryPreset
                .Where(w => !w.DeleteFlag.HasValue || w.DeleteFlag == 0)
                .Select(s => new DisciplinaryPresetVm
                {
                    PresetId = s.DisciplinaryPresetId,
                    PresetName = s.PresetName,
                    HousingFlag = s.DefaultHousingFlag == 1,
                    Narrative = s.DefaultNarrative,
                    DisciplinarySynopsis = s.DefaultDisciplinarySynopsis,
                    AllowHearingFlag = s.DefaultAllowHearingFlag == 1,
                    BypassHearing = s.DefaultDisciplinaryInmateBypassHearing == 1,
                    DisciplinaryTypeId = s.DefaultDisciplinaryType,
                    DisciplinaryType = lstLookup.Single(x => x.LookupType == LookupConstants.DISCTYPE
                        && x.LookupIndex == s.DefaultDisciplinaryType).LookupDescription,
                    DisciplinaryInmateTypeId = s.DefaultDisciplinaryInmateType,
                    DisciplinaryInmateType = lstLookup
                        .SingleOrDefault(x => x.LookupIndex == s.DefaultDisciplinaryInmateType
                            && x.LookupType == LookupConstants.DISCINTYPE).LookupDescription,
                    ViolationId = s.DefaultDisciplinaryControlViolationId,
                    CategorizationIndex = s.IncidentCategorizationIndex,
                    Categorization = lstLookup.Single(x => x.LookupType == LookupConstants.INCCAT
                        && x.LookupIndex == s.IncidentCategorizationIndex).LookupDescription
                }).ToList();
            List<DisciplinaryControlLookup> controlLookup = _context.DisciplinaryControlLookup.Where(a => presetList
                .Select(w => w.ViolationId).Contains(a.DisciplinaryControlLookupId)).Select(a =>
                new DisciplinaryControlLookup
                {
                    DisciplinaryControlLookupId = a.DisciplinaryControlLookupId,
                    DisciplinaryControlLookupName = a.DisciplinaryControlLookupName,
                    DisciplinaryControlLookupDescription = a.DisciplinaryControlLookupDescription,
                }).ToList();
            presetList.ForEach(list =>
            {
                list.Violation = controlLookup.Where(w => w.DisciplinaryControlLookupId == list.ViolationId)
                    .Select(s => new KeyValuePair<string, string>(s.DisciplinaryControlLookupName ?? "",
                        s.DisciplinaryControlLookupDescription ?? ""))
                    .SingleOrDefault();
            });
            return presetList;
        }

        //Insert method for preset incident
        public async Task<int> InsertPresetIncident(DisciplinaryPresetVm presetDetails)
        {
            List<InmateHousing> housing = new List<InmateHousing>();

            if (presetDetails.HousingFlag)
            {
                //Getting Inmates Housing Details 
                housing = _context.Inmate.Where(w => presetDetails.InmateIds.Contains(w.InmateId))
                    .Select(s => new InmateHousing
                    {
                        InmateId = s.InmateId,
                        HousingLocation = s.HousingUnit.HousingUnitLocation,
                        HousingBedNumber = s.HousingUnit.HousingUnitBedNumber,
                        HousingNumber = s.HousingUnit.HousingUnitNumber,
                        HousingBedLocation = s.InmateCurrentTrack,
                        HousingUnitId = s.InmateCurrentTrackId
                    }).ToList();
            }

            foreach (int id in presetDetails.InmateIds)
            {
                string getIncidentNumber = _commonService.GetGlobalNumber((int)AtimsGlobalNumber.IncidentNumber);

                //Insert For DisciplinaryIncident Table
                DisciplinaryIncident incident = new DisciplinaryIncident
                {
                    DisciplinaryNumber = getIncidentNumber,
                    DisciplinaryIncidentDate = presetDetails.IncidentDate,
                    DisciplinaryType = presetDetails.DisciplinaryTypeId,
                    DisciplinaryActive = NumericConstants.ONE,
                    DisciplinaryOfficerId = _personnelId,
                    DisciplinaryReportDate = presetDetails.ReportDate,
                    CreateDate = DateTime.Now,
                    UpdateDate = DateTime.Now,
                    DisciplinarySynopsis = presetDetails.DisciplinarySynopsis,
                    FacilityId = presetDetails.FacilityId,
                    DisciplinaryLocation = presetDetails.HousingFlag
                        ? housing.Single(i => i.InmateId == id).HousingBedLocation
                        : presetDetails.DispLocation,
                    DisciplinaryLocationId = presetDetails.HousingFlag
                        ? housing.Single(i => i.InmateId == id).HousingUnitId
                        : presetDetails.DispLocationId,
                    DisciplinaryHousingUnitLocation = presetDetails.HousingFlag
                        ? housing.Single(i => i.InmateId == id).HousingLocation
                        : presetDetails.DispHousingUnitLocation,
                    DisciplinaryHousingUnitNumber = presetDetails.HousingFlag
                        ? housing.Single(i => i.InmateId == id).HousingNumber
                        : presetDetails.DispHousingUnitNumber,
                    DisciplinaryHousingUnitBed = presetDetails.HousingFlag
                        ? housing.Single(i => i.InmateId == id).HousingBedNumber
                        : presetDetails.DispHousingUnitBedNumber,
                    AllowHearingFlag = presetDetails.AllowHearingFlag ? NumericConstants.ONE : (int?)null,
                    AllowHearingDate = presetDetails.AllowHearingFlag ? DateTime.Now : (DateTime?)null,
                    AllowHearingBy = presetDetails.AllowHearingFlag ? _personnelId : (int?)null,
                    IncidentCategorizationIndex = presetDetails.CategorizationIndex ?? default
                };
                _context.DisciplinaryIncident.Add(incident);

                //Insert For DisciplinaryInmate Table
                DisciplinaryInmate inmate = new DisciplinaryInmate
                {
                    InmateId = id,
                    DisciplinaryIncidentId = incident.DisciplinaryIncidentId,
                    DisciplinaryInmateType = presetDetails.DisciplinaryInmateTypeId,
                    DisciplinaryInmateBypassHearing =
                        presetDetails.BypassHearing ? NumericConstants.ONE : NumericConstants.ZERO,
                    DisciplinaryDamage = "0",
                    CreateDate = DateTime.Now,
                    UpdateDate = DateTime.Now,
                    HearingComplete = NumericConstants.ZERO,
                    DisciplinaryDaysSentFlag = NumericConstants.ZERO,
                    DisciplinaryDaysRemoveFlag = NumericConstants.ZERO,
                    IncidentWizardStep = presetDetails.BypassHearing ? NumericConstants.SEVEN : NumericConstants.THREE
                };
                _context.DisciplinaryInmate.Add(inmate);

                //Insert For DisciplinaryControlXref Table
                DisciplinaryControlXref xref = new DisciplinaryControlXref
                {
                    DisciplinaryControlViolationId = presetDetails.ViolationId,
                    CreateDate = DateTime.Now,
                    UpdateDate = DateTime.Now,
                    DisciplinaryInmateId = inmate.DisciplinaryInmateId,
                    DisciplinaryControlLevelId = presetDetails.ViolationId
                };
                _context.DisciplinaryControlXref.Add(xref);

                //Insert For DisciplinaryIncidentNarrative Table
                DisciplinaryIncidentNarrative narrative = new DisciplinaryIncidentNarrative
                {
                    DisciplinaryIncidentId = incident.DisciplinaryIncidentId,
                    DisciplinaryIncidentNarrative1 = presetDetails.Narrative,
                    CreateDate = DateTime.Now,
                    CreateBy = _personnelId,
                    ReadyForReviewBy = _personnelId,
                    ReadyForReviewFlag = NumericConstants.ONE,
                    ReadyForReviewDate = DateTime.Now,
                    SupervisorReviewFlag = presetDetails.AllowHearingFlag ? 1 : (int?)null,
                    SupervisorReviewDate = presetDetails.AllowHearingFlag ? DateTime.Now : (DateTime?)null,
                    SupervisorReviewBy = presetDetails.AllowHearingFlag ? _personnelId : (int?)null,
                    SupervisorReviewNote = presetDetails.AllowHearingFlag
                        ? IncidentNarrativeConstants.AUTOAPPROVALFORPRESETTEMPLATE : null
                };
                _context.DisciplinaryIncidentNarrative.Add(narrative);
            }

            return await _context.SaveChangesAsync();
        }

        public List<IncarcerationForms> LoadIncidentForms(int incidentId, int dispInmateId) => _context.FormRecord
          .Where(a => (a.DisciplinaryControlId == incidentId || a.DisciplinaryInmateId == dispInmateId) && a.DeleteFlag == 0)
          .OrderByDescending(a => a.CreateDate)
          .Select(a => new IncarcerationForms
          {
              FormRecordId = a.FormRecordId,
              FormNotes = a.FormNotes,
              DeleteFlag = a.DeleteFlag,
              XmlData = a.XmlData,
              FormTemplatesId = a.FormTemplatesId,
              FormCategoryFolderName = a.FormTemplates.FormCategory.FormCategoryFolderName,
              HtmlFileName = a.FormTemplates.HtmlFileName,
              DisplayName = a.FormTemplates.DisplayName,
              CreateDate = a.CreateDate
          }).ToList();

        public IncidentCellGroupVm GetCellGroupDetails(int facilityId, string housingUnitLocation,
        string housingUnitNumber)
        {
            IncidentCellGroupVm incidentCellGroupList = new IncidentCellGroupVm();

            int housingUnitListId = _context.HousingUnit.First(h =>
                    h.FacilityId == facilityId && h.HousingUnitLocation == housingUnitLocation &&
                    h.HousingUnitNumber == housingUnitNumber).HousingUnitListId;

            incidentCellGroupList.CellGroupList = _context.HousingUnitListBedGroup
              .Where(w => w.HousingUnitListId == housingUnitListId)
              .Select(s => new cellGroupListVm
              {
                  HousingUnitListBedGroupId = s.HousingUnitListBedGroupId,
                  HousingUnitListId = s.HousingUnitListId,
                  BedGroupName = s.BedGroupName,
                  Selected = false
              }).ToList();

            incidentCellGroupList.CellList = _context.HousingUnit
                .Where(w => w.FacilityId == facilityId
                    && w.HousingUnitLocation == housingUnitLocation
                    && w.HousingUnitNumber == housingUnitNumber
                    && w.HousingUnitBedNumber != null)
                .OrderBy(o => o.HousingUnitBedNumber)
                .Select(s => new cellListVm
                {
                    // HousingUnitId = s.HousingUnitId,
                    HousingUnitListId = s.HousingUnitListId,
                    HousingUnitBedNumber = s.HousingUnitBedNumber,
                    Selected = false,
                    ContrabandFound = false,
                    ContrabandNote = string.Empty
                }).Distinct().ToList();
            return incidentCellGroupList;
        }

        //TODO: Remove magic numbers
        public List<LastHousingSearchVm> GetLastSearchDetails(int facilityId, string searchFlag)
        {
            List<LastHousingSearchVm> lastSearchList = new List<LastHousingSearchVm>();
            string FacilityAbbr = _context.Facility.First(fe => fe.FacilityId == facilityId).FacilityAbbr;
            if (searchFlag == "1")
            {
                IQueryable<HousingSearch> list = _context.HousingSearch.Where(w => w.HousingUnitId > 0
                    && w.DisciplinaryIncident.FacilityId == facilityId
                    && w.DisciplinaryIncident.DisciplinaryIncidentId == w.DisciplinaryIncidentID
                    && w.DeleteFlag == false);
                lastSearchList = list
                .OrderByDescending(o => o.SearchDate)
                .GroupBy(g => new
                {
                    g.DisciplinaryIncident.DisciplinaryHousingUnitLocation,
                    g.DisciplinaryIncident.DisciplinaryHousingUnitNumber,
                    g.DisciplinaryIncident.DisciplinaryHousingUnitBed,

                }).Select(s => new LastHousingSearchVm
                {
                    HousingUnitLocation = s.Key.DisciplinaryHousingUnitLocation,
                    HousingUnitNumber = s.Key.DisciplinaryHousingUnitNumber,
                    HousingUnitBedNumber = s.Key.DisciplinaryHousingUnitBed,
                    FacilityAbbr = FacilityAbbr,
                    IncidentNumber = s.First().DisciplinaryIncident.DisciplinaryNumber,
                    LastSearchDate = s.First().SearchDate.Date,
                    Elapsed = Math.Round((DateTime.Now - s.First().CreateDate).Value.TotalDays),
                    Contraband = s.Select(se => se.ContrabandFound ? "Y" : "N").Distinct().ToList(),
                    SearchNote = s.Select(se => se.ContrabandNote).Distinct().ToList()
                }).OrderBy(oe => oe.LastSearchDate).ToList();
            }
            else if (searchFlag == "2")
            {
                lastSearchList = _context.HousingSearch.Where(w => w.DisciplinaryIncident.DisciplinaryLocationId == w.LocationId
             && w.LocationId > 0 && w.DisciplinaryIncident.FacilityId == facilityId
             && w.DisciplinaryIncident.DisciplinaryIncidentId == w.DisciplinaryIncidentID
             && w.DeleteFlag == false)
                .OrderByDescending(o => o.SearchDate)
                .GroupBy(g => g.LocationId).Select(s => new LastHousingSearchVm
                {
                    LocationId = s.Key ?? 0,
                    LocationName = s.First().DisciplinaryIncident.DisciplinaryLocation,
                    FacilityAbbr = FacilityAbbr,
                    LastSearchDate = s.First().SearchDate.Date,
                    Elapsed = Math.Round((DateTime.Now - s.First().CreateDate).Value.TotalDays),
                    IncidentNumber = s.First().DisciplinaryIncident.DisciplinaryNumber,
                    LocContraband = s.First().ContrabandFound ? "Y" : "N",
                    LocSearchNote = s.First().ContrabandNote
                }).OrderBy(oe => oe.LastSearchDate).ToList();
            }
            else if (searchFlag == "3")
            {
                lastSearchList = _context.HousingSearch.Where(wc => wc.OtherLocation.OtherLocationId == wc.OtherLocationId
                && wc.DisciplinaryIncident.OtherLocationID == wc.OtherLocationId
                && wc.DisciplinaryIncident.FacilityId == facilityId
                && wc.OtherLocationId > 0 && wc.DisciplinaryIncident.DisciplinaryIncidentId == wc.DisciplinaryIncidentID
                && wc.DeleteFlag == false)
                .OrderByDescending(o => o.SearchDate)
                .GroupBy(g => g.OtherLocationId)
                .Select(s => new LastHousingSearchVm
                {
                    OtherLocationId = s.Key ?? 0,
                    OtherLocationName = s.First().OtherLocation.LocationName,
                    FacilityAbbr = FacilityAbbr,
                    LastSearchDate = s.First().SearchDate.Date,
                    Elapsed = Math.Round((DateTime.Now - s.First().CreateDate).Value.TotalDays),
                    IncidentNumber = s.First().DisciplinaryIncident.DisciplinaryNumber,
                    LocContraband = s.First().ContrabandFound ? "Y" : "N",
                    LocSearchNote = s.First().ContrabandNote
                }).OrderBy(oe => oe.LastSearchDate).ToList();
            }

            return lastSearchList;
        }
        public List<CountHousingSearchVm> GetCountSearchList(int facilityId, string searchFlag,
        DateTime Fromdate, string disposition, string lookupType = "")
        {
             bool contraband = disposition == "1";
            List<CountHousingSearchVm> countSearchList = new List<CountHousingSearchVm>();
            string FacilityAbbr = _context.Facility.First(fe => fe.FacilityId == facilityId).FacilityAbbr;
            IQueryable<HousingSearch> list = _context.HousingSearch.Where(w => w.SearchDate >= Fromdate &&
                w.SearchDate <= DateTime.Now
                && w.DisciplinaryIncident.FacilityId == facilityId && w.DeleteFlag == false
                && (!string.IsNullOrEmpty(disposition) ? w.ContrabandFound == contraband
                    : w.DisciplinaryIncident.FacilityId == facilityId)
                && (!string.IsNullOrEmpty(lookupType) ? w.DisciplinaryIncident.DisciplinaryIncidentFlag.Any(b =>
                    b.DisciplinaryIncidentId == w.DisciplinaryIncident.DisciplinaryIncidentId
                    && b.IncidentFlagText == lookupType) : w.DisciplinaryIncident.FacilityId == facilityId)
            );
            countSearchList = searchFlag switch
            {
                "4" => list
                    .Where(w => w.DisciplinaryIncidentID == w.DisciplinaryIncident.DisciplinaryIncidentId &&
                        !string.IsNullOrEmpty(w.DisciplinaryIncident.DisciplinaryHousingUnitLocation) &&
                        !string.IsNullOrEmpty(w.DisciplinaryIncident.DisciplinaryHousingUnitNumber))
                    .GroupBy(g => new
                    {
                        g.DisciplinaryIncident.DisciplinaryHousingUnitLocation,
                        g.DisciplinaryIncident.DisciplinaryHousingUnitNumber,
                        g.DisciplinaryIncident.DisciplinaryHousingUnitBed,
                    })
                    .Select(x => new CountHousingSearchVm
                    {
                        HousingUnitLocation = x.Key.DisciplinaryHousingUnitLocation,
                        HousingUnitNumber = x.Key.DisciplinaryHousingUnitNumber,
                        HousingUnitBedNumber = x.Key.DisciplinaryHousingUnitBed,
                        FacilityAbbr = FacilityAbbr,
                        IncidentList =
                            x.Select(s =>
                                    new KeyValuePair<int, string>(s.DisciplinaryIncident.DisciplinaryIncidentId,
                                        s.DisciplinaryIncident.DisciplinaryNumber))
                                .Distinct()
                                .ToList(),
                        SearchCount = x.Select(s => s.DisciplinaryIncident.DisciplinaryNumber).Distinct().Count(),
                    })
                    .OrderByDescending(o => o.SearchCount)
                    .ToList(),
                "5" => list
                    .Where(w => w.LocationId == w.DisciplinaryIncident.DisciplinaryLocationId &&
                        w.DisciplinaryIncident.DisciplinaryLocationId > 0 &&
                        w.DisciplinaryIncidentID == w.DisciplinaryIncident.DisciplinaryIncidentId)
                    .GroupBy(g => new
                    {
                        g.DisciplinaryIncident.DisciplinaryLocationId, g.DisciplinaryIncident.DisciplinaryLocation,
                    })
                    .Select(x => new CountHousingSearchVm
                    {
                        LocationId = x.Key.DisciplinaryLocationId ?? 0,
                        LocationName = x.Key.DisciplinaryLocation,
                        FacilityAbbr = FacilityAbbr,
                        IncidentList = x.Select(s =>
                                    new KeyValuePair<int, string>(s.DisciplinaryIncident.DisciplinaryIncidentId,
                                        s.DisciplinaryIncident.DisciplinaryNumber))
                                .Distinct()
                                .ToList(),
                        SearchCount = x.Select(s => s.DisciplinaryIncident.DisciplinaryNumber).Distinct().Count(),
                    })
                    .OrderByDescending(o => o.SearchCount)
                    .ToList(),
                "6" => list
                    .Where(w => w.OtherLocationId == w.OtherLocation.OtherLocationId &&
                        w.DisciplinaryIncident.OtherLocationID == w.OtherLocationId &&
                        w.DisciplinaryIncident.OtherLocationID > 0 &&
                        w.DisciplinaryIncidentID == w.DisciplinaryIncident.DisciplinaryIncidentId)
                    .GroupBy(g => new {g.DisciplinaryIncident.OtherLocationID,})
                    .Select(x => new CountHousingSearchVm
                    {
                        OtherLocationId = x.Key.OtherLocationID ?? 0,
                        OtherLocationName = x.Select(se => se.OtherLocation.LocationName).FirstOrDefault(),
                        FacilityAbbr = FacilityAbbr,
                        IncidentList =
                            x.Select(s =>
                                    new KeyValuePair<int, string>(s.DisciplinaryIncident.DisciplinaryIncidentId,
                                        s.DisciplinaryIncident.DisciplinaryNumber))
                                .Distinct()
                                .ToList(),
                        SearchCount = x.Select(s => s.DisciplinaryIncident.DisciplinaryNumber).Distinct().Count(),
                    })
                    .OrderByDescending(o => o.SearchCount)
                    .ToList(),
                _ => countSearchList
            };

            return countSearchList;
        }
        public List<LastHousingSearchVm> GetLastSearchHistoryList(LastHousingSearchVm values, int facilityId, string searchFlag)
        {
            var searchList = searchFlag switch
            {
                "1" => _context.DisciplinaryIncident
                    .Where(w => w.FacilityId == facilityId &&
                        w.DisciplinaryHousingUnitLocation == values.HousingUnitLocation &&
                        w.DisciplinaryHousingUnitNumber == values.HousingUnitNumber &&
                        w.DisciplinaryHousingUnitBed == values.HousingUnitBedNumber && w.HousingSearch.Any(a =>
                            a.DisciplinaryIncidentID == w.DisciplinaryIncidentId && a.HousingUnitId > 0))
                    .OrderBy(o => o.DisciplinaryIncidentDate)
                    .Select(s => new LastHousingSearchVm
                    {
                        HousingUnitLocation = s.DisciplinaryHousingUnitLocation,
                        HousingUnitNumber = s.DisciplinaryHousingUnitNumber,
                        HousingUnitBedNumber = s.DisciplinaryHousingUnitBed,
                        IncidentNumber = s.DisciplinaryNumber,
                        LastSearchDate = s.HousingSearch.First().SearchDate,
                        Contraband =
                            s.HousingSearch.Select(se => se.ContrabandFound ? "Y" : "N").Distinct().ToList(),
                        SearchNote = s.HousingSearch.Select(se => se.ContrabandNote).Distinct().ToList()
                    })
                    .ToList(),
                "2" => _context.DisciplinaryIncident
                    .Where(w => w.FacilityId == facilityId && w.DisciplinaryLocationId == values.LocationId &&
                        w.HousingSearch.Any(a =>
                            a.DisciplinaryIncidentID == w.DisciplinaryIncidentId && a.LocationId > 0))
                    .OrderBy(o => o.DisciplinaryIncidentDate)
                    .Select(s => new LastHousingSearchVm
                    {
                        LocationId = s.DisciplinaryLocationId ?? 0,
                        LocationName = s.DisciplinaryLocation,
                        IncidentNumber = s.DisciplinaryNumber,
                        LastSearchDate = s.HousingSearch.First().SearchDate,
                        LocContraband = s.HousingSearch.First().ContrabandFound ? "Y" : "N",
                        LocSearchNote = s.HousingSearch.First().ContrabandNote
                    })
                    .ToList(),
                "3" => _context.DisciplinaryIncident
                    .Where(w => w.FacilityId == facilityId && w.OtherLocationID == values.OtherLocationId &&
                        w.HousingSearch.Any(a =>
                            a.DisciplinaryIncidentID == w.DisciplinaryIncidentId && a.OtherLocationId > 0))
                    .OrderBy(o => o.DisciplinaryIncidentDate)
                    .Select(s => new LastHousingSearchVm
                    {
                        OtherLocationId = s.DisciplinaryLocationId ?? 0,
                        OtherLocationName = s.OtherLocation.LocationName,
                        IncidentNumber = s.DisciplinaryNumber,
                        LastSearchDate = s.HousingSearch.First().SearchDate,
                        LocContraband = s.HousingSearch.First().ContrabandFound ? "Y" : "N",
                        LocSearchNote = s.HousingSearch.First().ContrabandNote
                    })
                    .ToList(),
                _ => new List<LastHousingSearchVm>()
            };

            return searchList;
        }

        public List<CountHousingSearchVm> GetCountSearchHistoryList(CountHousingSearchVm values, int facilityId, string searchFlag)
        {

            List<CountHousingSearchVm> countSearchList = new List<CountHousingSearchVm>();
            List<string> list = values.DisplinaryIncidentId.Split(',').ToList();
            List<int> intList = list.ConvertAll(int.Parse);
            List<string> stnList = values.DisplinaryNumber.Split(',').ToList();
            countSearchList = searchFlag switch
            {
                "4" => _context.DisciplinaryIncident
                    .Where(w => w.FacilityId == facilityId && intList.Contains(w.DisciplinaryIncidentId) &&
                        stnList.Contains(w.DisciplinaryNumber))
                    .Select(x => new CountHousingSearchVm
                    {
                        HousingUnitLocation = x.DisciplinaryHousingUnitLocation,
                        HousingUnitNumber = x.DisciplinaryHousingUnitNumber,
                        HousingUnitBedNumber = x.DisciplinaryHousingUnitBed,
                        DisplinaryNumber = x.DisciplinaryNumber,
                    })
                    .ToList(),
                "5" => _context.DisciplinaryIncident
                    .Where(w => w.FacilityId == facilityId && intList.Contains(w.DisciplinaryIncidentId) &&
                        stnList.Contains(w.DisciplinaryNumber))
                    .Select(x => new CountHousingSearchVm
                    {
                        LocationId = x.DisciplinaryLocationId ?? 0,
                        LocationName = x.DisciplinaryLocation,
                        DisplinaryNumber = x.DisciplinaryNumber,
                    })
                    .ToList(),
                "6" => _context.DisciplinaryIncident
                    .Where(w => w.FacilityId == facilityId && intList.Contains(w.DisciplinaryIncidentId) &&
                        stnList.Contains(w.DisciplinaryNumber))
                    .Select(x => new CountHousingSearchVm
                    {
                        OtherLocationId = x.OtherLocationID ?? 0,
                        OtherLocationName = x.OtherLocation.LocationName,
                        DisplinaryNumber = x.DisciplinaryNumber
                    })
                    .ToList(),
                _ => countSearchList
            };

            return countSearchList;
        }

    }
}


