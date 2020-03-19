﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ServerAPI.ViewModels;
using GenerateTables.Models;
using Microsoft.AspNetCore.Http;
using ServerAPI.Utilities;
using System.IO;
using System.Linq.Dynamic.Core;

namespace ServerAPI.Services
{
    public class IncidentActiveService : IIncidentActiveService
    {
        private readonly AAtims _context;
        private readonly ICommonService _commonService;
        private readonly int _personnelId;
        private readonly IIncidentService _incidentService;
        private readonly IPersonService _personService;
        private readonly IPhotosService _photo;

        public IncidentActiveService(AAtims context, ICommonService commonService,
            IHttpContextAccessor httpContextAccessor, IIncidentService incidentService, IPersonService personService, IPhotosService photosService)
        {
            _context = context;
            _commonService = commonService;
            _incidentService = incidentService;
            _personnelId = Convert.ToInt32(httpContextAccessor.HttpContext.User.FindFirst("personnelId")?.Value);
            _personService = personService;
            _photo = photosService;
        }

        // Get active incident list.
        public IncidentViewerDetails LoadActiveIncidentViewerDetails(IncidentFilters incidentFilters)
        {
            IncidentViewerDetails incidentViewerDetails = new IncidentViewerDetails();
            
            // Note: isFirstTime flag is used for identifying initial http request to load all dropdown filters values.
            // For each and every filteration we dont have to load all dropdown filters values again and again.
            if (incidentFilters.isFirstTime)
            {
                incidentViewerDetails.CreateIncidentViewerTypes(_context);
            }

            incidentViewerDetails.SetFilters(incidentFilters);

            incidentViewerDetails
                .SetQuery(_context)
                .FilterIsHistory()
                .FilterDeleteFlag()
                .FilterByCategory(_personnelId)
                .FilterByRestrictions()
                .FilterDisciplinaryType()
                .FilterByHearing()
                .FilterCategorization()
                .FilterKeyword()
                .FilterHours()
                .FilterByDate()
                .FilterOfficer()
                .FilterIncidentNumber()
                .FilterIncidentDate()
                .FilterLocation()
                .Sort();

            incidentViewerDetails.SetTotalRecords();
            incidentViewerDetails.SetIncidentViewerList(_context);

            int[] altSentSiteIds = incidentViewerDetails.IncidentViewerList.Where(x => x.DispAltSentSiteId > 0)
                .Select(x => x.DispAltSentSiteId.Value).ToArray();
            List<KeyValuePair<int, string>> altSentSites = _context.AltSentSite
                .Where(a => altSentSiteIds.Contains(a.AltSentSiteId))
                .Select(a => new KeyValuePair<int, string>(a.AltSentSiteId, a.AltSentSiteName)).ToList();
            incidentViewerDetails.IncidentViewerList.ForEach(a =>
            {
                a.AltSentSiteName = altSentSites.Where(x => x.Key == a.DispAltSentSiteId).Select(x => x.Value)
                    .SingleOrDefault();
            });
            return incidentViewerDetails;
        }

        // This method can be reusable, when we go for incident history screen.
        private IQueryable<DisciplinaryInmate> LoadIncident(IncidentFilters incidentFilters) => _context
            .DisciplinaryInmate
            .Where(a => a.DisciplinaryIncident.FacilityId == incidentFilters.FacilityId
                && (incidentFilters.DisciplinaryType == 0
                    || a.DisciplinaryIncident.DisciplinaryType == incidentFilters.DisciplinaryType)
                && (!IncidentActiveConstants.APPROVED.Equals(incidentFilters.Hearing)
                    || a.DisciplinaryIncident.AllowHearingFlag == 1)
                && (incidentFilters.Categorization == 0 || a.DisciplinaryIncident
                    .IncidentCategorizationIndex == incidentFilters.Categorization)
                && (!IncidentActiveConstants.NOTAPPROVED.Equals(incidentFilters.Hearing)
                    || !a.DisciplinaryIncident.AllowHearingFlag.HasValue)
                && (string.IsNullOrEmpty(incidentFilters.KeyWord) ||
                    !string.IsNullOrEmpty(a.DisciplinaryIncident.DisciplinarySynopsis)
                    && a.DisciplinaryIncident.DisciplinarySynopsis.Contains(incidentFilters.KeyWord) ||
                    a.DisciplinaryIncident.DisciplinaryIncidentNarrative.Any(x =>
                        !string.IsNullOrEmpty(x.DisciplinaryIncidentNarrative1)
                        && x.DisciplinaryIncidentNarrative1.Contains(incidentFilters.KeyWord)) ||
                    !string.IsNullOrEmpty(a.DisciplinaryViolationDescription)
                    && a.DisciplinaryViolationDescription.Contains(incidentFilters.KeyWord) ||
                    !string.IsNullOrEmpty(a.DisciplinaryRecommendations)
                    && a.DisciplinaryRecommendations.Contains(incidentFilters.KeyWord) ||
                    !string.IsNullOrEmpty(a.DisciplinaryReviewNotes)
                    && a.DisciplinaryReviewNotes.Contains(incidentFilters.KeyWord) ||
                    !string.IsNullOrEmpty(a.DisciplinarySanction)
                    && a.DisciplinarySanction.Contains(incidentFilters.KeyWord) ||
                    !string.IsNullOrEmpty(a.DisciplinaryFindingNotes)
                    && a.DisciplinaryFindingNotes.Contains(incidentFilters.KeyWord)));

        // Get incident Details.
        public IncidentDetails LoadIncidentDetails(int incidentId, int dispInmateId, bool deleteFlag)
        {
            IncidentDetails incidentDetails = new IncidentDetails
            {
                NarrativeDetails = LoadDisciplinaryNarrativeDetails(incidentId),
                IncidentAttachments = LoadIncidentAttachments(incidentId, deleteFlag),
                IncidentPhotos = LoadIncidentPhotos(incidentId, deleteFlag),
                IncidentForms = LoadIncidentForms(incidentId),
                InvolvedPartyDetails = LoadInvolvedPartyDetails(incidentId, 0, true),
                ActiveIncidentWizards = _incidentService.GetIncidentWizard(),
                InmateFormList = LoadInmateFormDetails(dispInmateId)
            };
            if (deleteFlag)
            {
                incidentDetails.AppealDetails = GetAppealDetails(incidentId, dispInmateId);
            }
            return incidentDetails;
        }

        // Get disciplinary narrative details by incident Id.
        public List<IncidentNarrativeDetailVm> LoadDisciplinaryNarrativeDetails(int incidentId) => _context
            .DisciplinaryIncidentNarrative.Where(a => a.DisciplinaryIncidentId == incidentId)
            .OrderBy(a => a.DisciplinaryIncidentNarrativeId)
            .Select(a => new IncidentNarrativeDetailVm
            {
                DisciplinaryIncidentNarrativeId = a.DisciplinaryIncidentNarrativeId,
                DisciplinaryIncidentId = a.DisciplinaryIncidentId,
                CreateDate = a.CreateDate,
                DeleteFlag = a.DeleteFlag > 0,
                CreateByPersonnel = new PersonnelVm
                {
                    PersonnelId = a.CreateBy,
                    PersonLastName = a.CreateByNavigation.PersonNavigation.PersonLastName,
                    PersonFirstName = a.CreateByNavigation.PersonNavigation.PersonFirstName,
                    OfficerBadgeNumber = a.CreateByNavigation.OfficerBadgeNum
                },
                DisciplinaryIncidentNarrative = a.DisciplinaryIncidentNarrative1,
                DisciplinaryOfficerId = a.DisciplinaryIncident.DisciplinaryOfficerId,
                ReadyForReviewFlag = a.ReadyForReviewFlag > 0,
                ReadyForReviewBy = a.ReadyForReviewBy,
                ReadyForReviewDate = a.ReadyForReviewDate,
                SupervisorReviewFlag = a.SupervisorReviewFlag > 0,
                SupervisorReviewNote = a.SupervisorReviewNote,
                ReviewBy = new PersonnelVm
                {
                    PersonLastName = a.SupervisorReviewByNavigation.PersonNavigation.PersonLastName,
                    PersonFirstName = a.SupervisorReviewByNavigation.PersonNavigation.PersonFirstName,
                    OfficerBadgeNumber = a.SupervisorReviewByNavigation.OfficerBadgeNum
                },
                SupervisorReviewDate = a.SupervisorReviewDate
            }).ToList();

        // Get incident attachment details by incident Id and delete flag.
        public List<PrebookAttachment> LoadIncidentAttachments(int incidentId, bool deleteFlag) => _context
            .AppletsSaved.Where(a => a.DisciplinaryIncidentId == incidentId
                                     && (deleteFlag || a.AppletsDeleteFlag == 0))
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

        // Get incident photos by incident Id and delete flag.
        public List<IdentifierVm> LoadIncidentPhotos(int incidentId, bool deleteFlag) => _context.Identifiers.Where(
                a => a.DisciplinaryIncidentId == incidentId
                     && (deleteFlag || a.DeleteFlag == 0))
            .OrderByDescending(a => a.IdentifierId)
            .Select(a => new IdentifierVm
            {
                IdentifierId = a.IdentifierId,
                PhotographDate = a.PhotographDate,
                IdentifierTypeName = a.IdentifierDescription,
                DeleteFlag = a.DeleteFlag,
                IdentifierType = a.IdentifierType,
                PhotographRelativePath = _photo.GetPhotoByIdentifier(a),
                CreatedDate = a.CreateDate,
                IdentifierNarrative = a.IdentifierNarrative,
                Officer = new PersonnelVm
                {
                    PersonLastName = a.PhotographTakenByNavigation.PersonNavigation.PersonLastName,
                    PersonFirstName = a.PhotographTakenByNavigation.PersonNavigation.PersonFirstName,
                    OfficerBadgeNumber = a.PhotographTakenByNavigation.OfficerBadgeNum
                }
            }).ToList();

        // Get incident forms by incident Id.
        public List<IncarcerationForms> LoadIncidentForms(int incidentId) => _context.FormRecord
            .Where(a => a.DisciplinaryControlId == incidentId)
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
                CreateDate = a.CreateDate,
                NoSignature = a.NoSignatureReason,
                NoSignatureFlag = a.NoSignatureFlag
            }).ToList();

        // Check incident active status by incident Id.
        public int GetIncidentActiveStatus(int incidentId) =>
            _context.DisciplinaryIncident.Single(a => a.DisciplinaryIncidentId == incidentId)
                .DisciplinaryActive ?? 0;

        // Insert or update narrative details.
        public async Task<int> InsertOrUpdateNarrativeInfo(IncidentNarrativeDetailVm narrative)
        {
            if (narrative.DisciplinaryIncidentNarrativeId > 0)
            {
                DisciplinaryIncidentNarrative narrativeObject = _context.DisciplinaryIncidentNarrative
                    .Single(a => a.DisciplinaryIncidentNarrativeId == narrative.DisciplinaryIncidentNarrativeId);
                narrativeObject.DisciplinaryIncidentNarrative1 = narrative.DisciplinaryIncidentNarrative;
                narrativeObject.UpdateDate = DateTime.Now;
                narrativeObject.UpdateBy = _personnelId;
                narrativeObject.ReadyForReviewFlag = narrative.ReadyForReviewFlag ? 1 : 0;
                narrativeObject.ReadyForReviewDate = DateTime.Now;
                narrativeObject.ReadyForReviewBy = _personnelId;
            }
            else
            {
                _context.DisciplinaryIncidentNarrative.Add(new DisciplinaryIncidentNarrative
                {
                    DisciplinaryIncidentId = narrative.DisciplinaryIncidentId,
                    DisciplinaryIncidentNarrative1 = narrative.DisciplinaryIncidentNarrative,
                    CreateDate = DateTime.Now,
                    CreateBy = _personnelId,
                    ReadyForReviewFlag = narrative.ReadyForReviewFlag ? 1 : 0,
                    ReadyForReviewDate = DateTime.Now,
                    ReadyForReviewBy = _personnelId
                });
            }

            return await _context.SaveChangesAsync();
        }

        // Update incident active status by incident Id.
        public async Task<int> UpdateIncidentActiveStatus(int incidentId, bool status)
        {
            DisciplinaryIncident disciplinaryIncident = _context.DisciplinaryIncident.Find(incidentId);
            disciplinaryIncident.DisciplinaryActive = status ? 1 : 0;
            return await _context.SaveChangesAsync();
        }

        // Get involved property details by incident Id.
        public List<IncidentViewer> LoadInvolvedPartyDetails(int incidentId, int dispInmateId, bool isWizardSteps)
        {
            List<Privileges> privileges = _context.Privileges
            .Where(pr => pr.ShowInHearing == 1 && pr.InactiveFlag == 0).ToList();
            List<LookupVm> lookupList =
                _commonService.GetIncidentLookups(new[] { LookupConstants.DISCINTYPE,LookupConstants.DISCDAM,
                    LookupConstants.DISCTYPE,LookupConstants.INCCAT });
            List<DisciplinaryControlLookup> disciplinaryControlLookup = _context.DisciplinaryControlLookup.Where(w =>
                (!w.InactiveFlag.HasValue || w.InactiveFlag == 0) &&
                w.DisciplinaryControlLookupType == (int?)DisciplinaryLookup.DISCWAIV).ToList();
            List<IncidentViewer> involvedParties = _context.DisciplinaryInmate
                .Where(a => (dispInmateId != 0 || (!a.DeleteFlag.HasValue && a.DisciplinaryIncidentId == incidentId))
                    && (dispInmateId == 0 || a.DisciplinaryInmateId == dispInmateId))
                .Select(a => new IncidentViewer
                {
                    DisciplinaryInmateId = a.DisciplinaryInmateId,
                    DisciplinaryIncidentId = a.DisciplinaryIncidentId,
                    InmateType = lookupList.Where(x => x.LookupType == LookupConstants.DISCINTYPE
                            && x.LookupIndex == a.DisciplinaryInmateType)
                        .Select(x => x.LookupDescription).FirstOrDefault(),
                    DispOtherName = a.DisciplinaryOtherName,
                    Personnel = a.InmateId.HasValue
                        ? new PersonVm
                        {
                            PersonLastName = a.Inmate.Person.PersonLastName,
                            PersonFirstName = a.Inmate.Person.PersonFirstName,
                            PersonMiddleName = a.Inmate.Person.PersonMiddleName,
                            PersonDob = a.Inmate.Person.PersonDob,
                            PersonId = a.Inmate.Person.PersonId,
                            InmateNumber = a.Inmate.InmateNumber,
                            InmateClassificationReason = a.Inmate.InmateClassification
                            .InmateClassificationReason,
                            HousingUnitLocation = a.Inmate.HousingUnit.HousingUnitLocation,
                            HousingUnitNumber = a.Inmate.HousingUnit.HousingUnitNumber,
                            HousingUnitBedLocation = a.Inmate.HousingUnit.HousingUnitBedLocation,
                            HousingUnitBedNumber = a.Inmate.HousingUnit.HousingUnitBedNumber,
                            FacilityAbbr = a.Inmate.Facility.FacilityAbbr
                        }
                        : a.PersonnelId.HasValue
                            ? new PersonVm
                            {
                                PersonLastName = a.Personnel.PersonNavigation.PersonLastName,
                                PersonFirstName = a.Personnel.PersonNavigation.PersonFirstName,
                                PersonMiddleName = a.Personnel.PersonNavigation.PersonMiddleName,
                                PersonDob = a.Personnel.PersonNavigation.PersonDob,
                                PersonId = a.Personnel.PersonNavigation.PersonId,
                                OfficerBadgeNumber = a.Personnel.OfficerBadgeNum
                            }
                            : new PersonVm
                            {
                                PersonLastName = a.DisciplinaryOtherName
                            },
                    DisciplinaryActive = a.DisciplinaryIncident.DisciplinaryActive,
                    InmateId = a.InmateId,
                    PersonnelId = a.PersonnelId,
                    FacilityId = a.InmateId.HasValue ? a.Inmate.FacilityId : default,
                    BypassHearing = a.DisciplinaryInmateBypassHearing,
                    IncidentNumber = a.DisciplinaryIncident.DisciplinaryNumber,
                    DispHousingUnitLocation = a.DisciplinaryIncident.DisciplinaryHousingUnitLocation,
                    DispHousingUnitNumber = a.DisciplinaryIncident.DisciplinaryHousingUnitNumber,
                    DispHousingUnitBedNumber = a.DisciplinaryIncident.DisciplinaryHousingUnitBed,
                    DispLocation = a.DisciplinaryIncident.DisciplinaryLocation,
                    //  DispOtherLocation = a.DisciplinaryIncident.DisciplinaryLocationOther,
                    DispFreeForm = a.DisciplinaryIncident.DisciplinaryLocationOther,
                    DispOtherLocationId = a.DisciplinaryIncident.OtherLocationID,
                    DispOtherLocationName = _context.OtherLocation.SingleOrDefault(x => x.OtherLocationId ==
                        a.DisciplinaryIncident.OtherLocationID).LocationName,
                    DispViolationDescription = a.DisciplinaryViolationDescription,
                    ScheduleHearingDate = a.DisciplinaryScheduleHearingDate,
                    HearingDate = a.DisciplinaryHearingDate,
                    ReviewDate = a.DisciplinaryReviewDate,
                    IncidentFlag = a.DisciplinaryDamage,
                    DispHearingOfficer1 = a.DisciplinaryHearingOfficer1,
                    DispHearingOfficer2 = a.DisciplinaryHearingOfficer2,
                    DispFindingDate = a.DisciplinaryFindingDate,
                    DispFindingNotes = a.DisciplinaryFindingNotes,
                    DiscpRecommendations = a.DisciplinaryRecommendations,
                    DispReviewOfficer = a.DisciplinaryReviewOfficer,
                    ReviewNotes = a.DisciplinaryReviewNotes,
                    DispReviewCompleteDate = a.DisciplinaryReviewCompleteDate,
                    DispReviewCompleteOfficerId = a.DisciplinaryReviewCompleteOfficer,
                    DisciplinarySanction = a.DisciplinarySanction,
                    DispScheduleHearingLocation = !string.IsNullOrEmpty(a
                        .DisciplinaryScheduleHearingLocation) ? privileges
                        .Where(p => p.PrivilegeId == Convert.ToInt32(a
                            .DisciplinaryScheduleHearingLocation))
                        .Select(p => p.PrivilegeDescription).SingleOrDefault() : default,
                    HearingComplete = a.HearingComplete,
                    InmateInterview = a.InmateInterview,
                    DiscDays = a.DisciplinaryDays,
                    DisciplinaryInmatePresent = a.DisciplinaryInmatePresent,
                    AppealDueDate = a.AppealDueDate,
                    NoticeFlag = a.NoticeFlag,
                    ViolationCount = _context.DisciplinaryControlXref
                        .Count(w => w.DisciplinaryInmateId == a.DisciplinaryInmateId),
                    DisciplinaryType = lookupList.Where(x => x.LookupType == LookupConstants.DISCTYPE
                            && x.LookupIndex == a.DisciplinaryIncident.DisciplinaryType)
                        .Select(x => x.LookupDescription).FirstOrDefault(),
                    InmateNotice = new DisciplinaryInmateNotice //for inmate notice 
                    {
                        NoticeDate = a.NoticeDate,
                        NoticeNote = a.NoticeNote,
                        NoticePersonnelId = a.NoticePersonnelId,
                        NoticeWavierId = a.NoticeWavierId,
                        NoticeWavierName = disciplinaryControlLookup.Where(w =>
                                w.DisciplinaryControlLookupId == a.NoticeWavierId)
                            .Select(w => w.DisciplinaryControlLookupDescription).SingleOrDefault(),
                        WavierFlag = disciplinaryControlLookup.Where(w =>
                                w.DisciplinaryControlLookupId == a.NoticeWavierId)
                            .Select(w => w.WavierFlag).SingleOrDefault()
                    },
                    NarrativeFlag = a.NarrativeFlag ?? false,
                    HearingHold = a.DisciplinaryHearingHold,
                    AllowHearingFlag = a.DisciplinaryIncident.AllowHearingFlag == 1,
                    NoticeDate = a.NoticeDate,
                    InvolvedPartyFlags = lookupList.Where(x => x.LookupType == LookupConstants.DISCDAM).ToList(),
                    LookUpIncidentType = lookupList.Where(x => x.LookupType == LookupConstants.DISCTYPE
                            && x.LookupIndex == a.DisciplinaryIncident.DisciplinaryType)
                        .Select(x => x.LookupCategory).FirstOrDefault(),
                    LookUpCategorizationType = lookupList.Where(x => x.LookupType == LookupConstants.INCCAT
                            && x.LookupIndex == a.DisciplinaryIncident.IncidentCategorizationIndex)
                        .Select(x => x.LookupCategory).FirstOrDefault()
                }).ToList();
            if (isWizardSteps)
            {
                List<AoWizardProgressVm> incidentWizardProgresses = _incidentService
                    .GetIncidentWizardProgress(involvedParties.Select(a => a.DisciplinaryInmateId).ToArray());
                involvedParties.ForEach(a => a.ActiveIncidentProgress = incidentWizardProgresses
                        .FirstOrDefault(x => x.DisciplinaryInmateId == a.DisciplinaryInmateId));
            }
            else
            {
                List<PersonnelVm> personnels = _personService.GetPersonNameList(involvedParties.SelectMany(a => new[]
                {
                    a.DispHearingOfficer1,
                    a.DispHearingOfficer2,
                    a.DispReviewOfficer,
                    a.DispReviewCompleteOfficerId,
                    a.InmateNotice.NoticePersonnelId
                }).Where(a => a > 0).Select(a => a.Value).ToList());
                involvedParties.ForEach(a =>
                {
                    a.HearingOfficer1 = personnels.SingleOrDefault(x => x.PersonnelId == a.DispHearingOfficer1);
                    a.HearingOfficer2 = personnels.SingleOrDefault(x => x.PersonnelId == a.DispHearingOfficer2);
                    a.ReviewOfficer = personnels.SingleOrDefault(x => x.PersonnelId == a.DispReviewOfficer);
                    a.CompleteOfficer = personnels.SingleOrDefault(x => x.PersonnelId == a.DispReviewCompleteOfficerId);
                    a.InmateNotice.NoticePersonnelName = personnels
                        .SingleOrDefault(s => s.PersonnelId == a.InmateNotice.NoticePersonnelId)?.PersonLastName;
                    a.InmateNotice.NoticePersonnelNumber = personnels
                        .SingleOrDefault(s => s.PersonnelId == a.InmateNotice.NoticePersonnelId)?.PersonnelNumber;
                });
            }

            return involvedParties;
        }

        // Delete narrative details.
        public async Task<int> DeleteNarrativeInfo(int narrativeId)
        {
            DisciplinaryIncidentNarrative incidentNarrative = _context.DisciplinaryIncidentNarrative
                .Single(a => a.DisciplinaryIncidentNarrativeId == narrativeId);
            incidentNarrative.DeleteFlag = 1;
            incidentNarrative.DeleteDate = DateTime.Now;
            incidentNarrative.DeleteBy = _personnelId;
            return await _context.SaveChangesAsync();
        }

        // Get form templates by form category.
        public List<LoadSavedForms> LoadForms(string categoryName) => _context.FormTemplates
            .Where(a => !a.Inactive.HasValue && a.FormCategory.FormCategoryName == categoryName)
            .Select(a => new LoadSavedForms
            {
                FormTemplatesId = a.FormTemplatesId,
                DisplayName = a.DisplayName,
                FormCategoryFolderName = a.FormCategory.FormCategoryFolderName,
                HtmlFileName = a.HtmlFileName
            }).ToList();

        // Insert form record details.
        public async Task<int> InsertFormRecord(int formTemplateId, int grievanceOrIncidentId, bool isIncident,
            int inmateId)
        {
            FormRecord formRecord = new FormRecord
            {
                FormTemplatesId = formTemplateId,
                DisciplinaryControlId = isIncident ? grievanceOrIncidentId : (int?)null,
                GrievanceId = !isIncident ? grievanceOrIncidentId : (int?)null,
                InmateId = inmateId > 0 ? inmateId :(int?)null,
                CreateBy = _personnelId,
                CreateDate = DateTime.Now                
            };
            _context.FormRecord.Add(formRecord);
            await _context.SaveChangesAsync();
            return formRecord.FormRecordId;
        }

        public IncidentHistoryDetails GetIncidentHistoryDetails(IncidentFilters value)
        {
            IncidentHistoryDetails incidentHistoryDetails = new IncidentHistoryDetails();

            List<Lookup> lookup = _context.Lookup.Where(a => a.LookupType == LookupConstants.DISCTYPE
                            || a.LookupType == LookupConstants.DISCINTYPE || a.LookupType == LookupConstants.INCCAT).ToList();

            if (value.isFirstTime)
            {
                List<HousingUnit> housingUnit = _context.HousingUnit.Where(w =>
                    w.FacilityId == value.FacilityId
                    && (w.HousingUnitInactive == 0 || !w.HousingUnitInactive.HasValue)).ToList();

                incidentHistoryDetails.HousingLocationList = housingUnit
                    .OrderBy(o => o.HousingUnitLocation)
                    .GroupBy(g => g.HousingUnitLocation)
                    .Select(s => s.Key).ToList();

                incidentHistoryDetails.LocationList = _context.Privileges.Where(w => w.InactiveFlag == 0
                     && (w.FacilityId == value.FacilityId || !w.FacilityId.HasValue))
                    .OrderBy(o => o.PrivilegeDescription)
                    .Select(s => new KeyValuePair<int, string>
                        (s.PrivilegeId, s.PrivilegeDescription)).ToList();

                incidentHistoryDetails.AltSentSiteList = _context.AltSentSite
                    .Where(w => w.AltSentProgram.FacilityId == value.FacilityId)
                    .OrderBy(o => o.AltSentSiteName)
                    .Select(s => new KeyValuePair<int, string>(s.AltSentSiteId, s.AltSentSiteName)).ToList();
            }
            IQueryable<DisciplinaryInmate> disciplinaryInmates = LoadIncident(value);
            disciplinaryInmates = disciplinaryInmates
                .Where(a => value.DeleteFlag || !a.DeleteFlag.HasValue || a.DeleteFlag == 0);
            if (value.OfficerId > 0)
            {
                switch (value.OfficerType)
                {
                    case OfficerType.NARRATIVEBY:

                        disciplinaryInmates = disciplinaryInmates.Where(w =>
                            w.DisciplinaryIncident.DisciplinaryOfficerId == value.OfficerId);
                        break;
                    case OfficerType.REVIEWBY:
                        disciplinaryInmates =
                            disciplinaryInmates.Where(w => w.DisciplinaryReviewOfficer == value.OfficerId);
                        break;
                    case OfficerType.HEARINGBY:
                        disciplinaryInmates =
                            disciplinaryInmates.Where(w => w.DisciplinaryHearingOfficer1 == value.OfficerId);
                        break;
                    case OfficerType.COMPLETEBY:
                        disciplinaryInmates =
                            disciplinaryInmates.Where(w => w.DisciplinaryReviewCompleteOfficer == value.OfficerId);
                        break;
                    default:
                        disciplinaryInmates = disciplinaryInmates.Where(w =>
                            w.DisciplinaryIncident.DisciplinaryOfficerId == value.OfficerId);
                        break;
                }
            }

            if (value.FacilityId > 0)
            {
                disciplinaryInmates = disciplinaryInmates.Where(w =>
                    w.DisciplinaryIncident.FacilityId == value.FacilityId);
            }

            if (value.InmateId > 0)
            {
                disciplinaryInmates = disciplinaryInmates.Where(w => w.InmateId == value.InmateId);
            }

            if (!string.IsNullOrEmpty(value.IncidentNumber))
            {
                disciplinaryInmates = disciplinaryInmates.Where(w =>
                    w.DisciplinaryIncident.DisciplinaryNumber.Contains(value.IncidentNumber));
            }

            if (value.Hours > 0)
            {
                disciplinaryInmates = disciplinaryInmates
                    .Where(w => w.DisciplinaryIncident.DisciplinaryIncidentDate >= DateTime.Now.AddHours(-value.Hours)
                                && w.DisciplinaryIncident.DisciplinaryIncidentDate <= DateTime.Now);
            }

            if (value.FromDate.HasValue)
            {
                disciplinaryInmates = disciplinaryInmates
                    .Where(w => w.DisciplinaryIncident.DisciplinaryIncidentDate.Value.Date >= value.FromDate.Value.Date
                                && w.DisciplinaryIncident.DisciplinaryIncidentDate.Value.Date <=
                                value.ToDate.Value.Date);
            }

            if (!string.IsNullOrEmpty(value.HousingLocation))
            {
                disciplinaryInmates = disciplinaryInmates
                    .Where(w => w.DisciplinaryIncident.DisciplinaryHousingUnitLocation == value.HousingLocation);
            }
            if (!string.IsNullOrEmpty(value.HousingNumber))
            {
                disciplinaryInmates = disciplinaryInmates
                    .Where(w => w.DisciplinaryIncident.DisciplinaryHousingUnitNumber == value.HousingNumber);
            }
            if (!string.IsNullOrEmpty(value.HousingBedNumber))
            {
                disciplinaryInmates = disciplinaryInmates
                    .Where(w => w.DisciplinaryIncident.DisciplinaryHousingUnitBed == value.HousingBedNumber);
            }

            if (value.LocationId > 0)
            {
                disciplinaryInmates = disciplinaryInmates
                    .Where(w => w.DisciplinaryIncident.DisciplinaryLocationId == value.LocationId);
            }

            if (value.SiteId > 0)
            {
                disciplinaryInmates = disciplinaryInmates
                    .Where(w => w.DisciplinaryIncident.DisciplinaryAltSentSiteId == value.SiteId);
            }
            incidentHistoryDetails.TotalRecords = disciplinaryInmates.Count();
            incidentHistoryDetails.IncidentViewerList = disciplinaryInmates
                .OrderBy(value.SortColumn + " " + value.SortOrder)
                .Skip(value.Skip).Take(value.RowsPerPage)
                .Select(a => new IncidentViewer
                {
                    DisciplinaryIncidentId = a.DisciplinaryIncident.DisciplinaryIncidentId,
                    DisciplinaryInmateId = a.DisciplinaryInmateId,
                    AllowHearingFlag = a.DisciplinaryIncident.AllowHearingFlag > 0,
                    DispSynopsis = a.DisciplinaryIncident.DisciplinarySynopsis,
                    IncidentNumber = a.DisciplinaryIncident.DisciplinaryNumber,
                    IncidentDate = a.DisciplinaryIncident.DisciplinaryIncidentDate,
                    DispHousingUnitLocation = a.DisciplinaryIncident.DisciplinaryHousingUnitLocation,
                    DispHousingUnitNumber = a.DisciplinaryIncident.DisciplinaryHousingUnitNumber,
                    DispHousingUnitBedNumber = a.DisciplinaryIncident.DisciplinaryHousingUnitBed,
                    DispLocation = a.DisciplinaryIncident.DisciplinaryLocation,
                    DispAltSentSiteId = a.DisciplinaryIncident.DisciplinaryAltSentSiteId,
                    DispAltSentSiteName = _context.AltSentSite
                        .SingleOrDefault(s => s.AltSentSiteId == a.DisciplinaryIncident.DisciplinaryAltSentSiteId).AltSentSiteName,
                    //  DispOtherLocation = a.DisciplinaryIncident.DisciplinaryLocationOther,
                    DispFreeForm = a.DisciplinaryIncident.DisciplinaryLocationOther,
                    DispOtherLocationId = a.DisciplinaryIncident.OtherLocationID,
                    DispOtherLocationName = a.DisciplinaryIncident.OtherLocation.LocationName,
                    ReportDate = a.DisciplinaryIncident.DisciplinaryReportDate,
                    DisciplinaryType = lookup
                        .Where(x => x.LookupIndex == a.DisciplinaryIncident.DisciplinaryType
                                    && x.LookupType == LookupConstants.DISCTYPE)
                        .Select(x => x.LookupDescription)
                        .SingleOrDefault(),
                    DispOfficerId = a.DisciplinaryIncident.DisciplinaryOfficerId,
                    SupervisorAction = a.DisciplinaryIncident.DisciplinarySupervisorAction,
                    InmateId = a.InmateId ?? 0,
                    DispOtherName = a.DisciplinaryOtherName,
                    Person = new PersonInfoVm
                    {
                        PersonId = a.InmateId.HasValue ? a.Inmate.PersonId :
                        a.PersonnelId.HasValue ? a.Personnel.PersonNavigation.PersonId : default,
                        InmateNumber = a.InmateId.HasValue ? a.Inmate.InmateNumber :
                        a.PersonnelId.HasValue ? a.Personnel.OfficerBadgeNumber : default,
                        PersonLastName = a.InmateId.HasValue ? a.Inmate.Person.PersonLastName :
                        a.PersonnelId.HasValue ? a.Personnel.PersonNavigation.PersonLastName : default,
                        PersonFirstName = a.InmateId.HasValue ? a.Inmate.Person.PersonFirstName :
                        a.PersonnelId.HasValue ? a.Personnel.PersonNavigation.PersonFirstName : default
                    },
                    ScheduleHearingDate = a.DisciplinaryScheduleHearingDate,
                    ReviewDate = a.DisciplinaryReviewDate,
                    HearingDate = a.DisciplinaryHearingDate,
                    HearingComplete = a.HearingComplete,
                    DisciplinaryInmateType = lookup
                        .Where(x => x.LookupIndex == a.DisciplinaryInmateType
                                    && x.LookupType == LookupConstants.DISCINTYPE)
                        .Select(x => x.LookupDescription)
                        .SingleOrDefault(),
                    BypassHearing = a.DisciplinaryInmateBypassHearing,
                    DisciplinaryActive = a.DisciplinaryIncident.DisciplinaryActive,
                    Categorization = lookup.Where(x => x.LookupType == LookupConstants.INCCAT
                            && x.LookupIndex == a.DisciplinaryIncident.IncidentCategorizationIndex)
                        .Select(x => x.LookupDescription).SingleOrDefault()
                }).ToList();
            List<KeyValuePair<int, string>> incidentFlags = _context.DisciplinaryIncidentFlag
                .Where(a => incidentHistoryDetails.IncidentViewerList
                        .Select(x => x.DisciplinaryIncidentId).Contains(a.DisciplinaryIncidentId)
                    && !a.DeleteFlag.HasValue)
                .Select(a => new KeyValuePair<int, string>
                    (a.DisciplinaryIncidentId, a.IncidentFlagText)).ToList();

            List<KeyValuePair<int, string>> altSentSite = _context.AltSentSite
                .Where(a => incidentHistoryDetails.IncidentViewerList
                    .Select(x => x.DispAltSentSiteId).Contains(a.AltSentSiteId))
                .Select(a => new KeyValuePair<int, string>
                    (a.AltSentSiteId, a.AltSentSiteName)).ToList();
            List<AoWizardProgressVm> incidentWizardProgresses = _incidentService
                .GetIncidentWizardProgress(incidentHistoryDetails.IncidentViewerList.Select(a => a.DisciplinaryInmateId)
                    .ToArray());
            incidentHistoryDetails.IncidentViewerList.ForEach(a =>
            {
                a.DispIncidentFlag = incidentFlags.Where(x => x.Key == a.DisciplinaryIncidentId)
                    .Select(x => x.Value).ToArray();
                a.AltSentSiteName = altSentSite
                    .Where(w => w.Key == a.DispAltSentSiteId)
                    .Select(s => s.Value).SingleOrDefault();
                a.ActiveIncidentProgress = incidentWizardProgresses
                    .FirstOrDefault(x => x.DisciplinaryInmateId == a.DisciplinaryInmateId);
            });
            return incidentHistoryDetails;
        }

        public async Task<int> DeleteInvolvedPartyDetails(int disciplinaryInmateId)
        {
            DisciplinaryInmate disciplinaryInmate = _context.DisciplinaryInmate
                .Single(a => a.DisciplinaryInmateId == disciplinaryInmateId);
            disciplinaryInmate.DeleteFlag = 1;
            if (disciplinaryInmate.PersonnelId > 0 && disciplinaryInmate.NarrativeFlag == true)
            {
                DisciplinaryIncident dispIncident = _context.DisciplinaryIncident
                    .Single(a => a.DisciplinaryIncidentId == disciplinaryInmate.DisciplinaryIncidentId);
                dispIncident.ExpectedNarrativeCount = dispIncident.ExpectedNarrativeCount - 1;
            }
            return await _context.SaveChangesAsync();
        }

        public IncidentReport GetIncidentReportDetails(int incidentId, int inmateId, int dispInmateId, int reportType)
        {
            IncidentReport incidentReport = new IncidentReport
            {
                IncidentSummary = GetIncidentBasicDetails(incidentId),
                InvolvedPartyDetails = (int?) ReportType.IncidentReport == reportType
                    ? LoadInvolvedPartyDetails(incidentId, 0, false)
                    : LoadInvolvedPartyDetails(incidentId, dispInmateId, false),
                HeaderDetails = _context.Personnel.Where(a => a.PersonnelId == _personnelId)
                    .Select(a => new PersonnelVm
                    {
                        PersonLastName = a.PersonNavigation.PersonLastName,
                        PersonnelNumber = a.PersonnelNumber
                    }).SingleOrDefault()
            };
            incidentReport.HousingLocation = GetHousingLocation(incidentReport.InvolvedPartyDetails
                .Select(a => a.DisciplinaryInmateId).ToArray());
            incidentReport.PhotoGraphs = GetPhotoGraphs(incidentReport.InvolvedPartyDetails
                .Select(a => a.Personnel.PersonId).ToArray()).Where(a => !string.IsNullOrEmpty(a.IdentifierType)
                && a.IdentifierType.Trim() == "1").ToList();
            incidentReport.ViolationDetails = GetViolationDetails(incidentId, incidentReport.InvolvedPartyDetails
                .Select(a => a.DisciplinaryInmateId).ToArray());
            if ((int?)ReportType.HearingNotice != reportType && (int?)ReportType.InvolvedPtyReport != reportType)
            {
                incidentReport.PrivilegeViewes = GetPrivilegeView(inmateId, true, incidentId);
            }
            if ((int?)ReportType.IncidentReport == reportType || (int?)ReportType.InvolvedPtyReport == reportType)
            {
                incidentReport.IncidentPhotos = LoadIncidentPhotos(incidentId, false);
                incidentReport.IncidentAttachments = LoadIncidentAttachments(incidentId, false);
                incidentReport.IncidentForms = LoadIncidentForms(incidentId);
                incidentReport.IncidentNarrativeDetails =
                    LoadDisciplinaryNarrativeDetails(incidentId).Where(a => !a.DeleteFlag).ToList();
            }
            if ((int?)ReportType.HearingNotice != reportType && (int?)ReportType.HearingReport != reportType)
            {
                incidentReport.AppealReports = GetAppealReports(dispInmateId);
            }
            incidentReport.AgencyName = _context.Agency.FirstOrDefault(a => a.AgencyJailFlag)?.AgencyName;
            return incidentReport;
        }

        public IncidentViewer GetIncidentBasicDetails(int incidentId) => _context.DisciplinaryIncident
            .Where(a => a.DisciplinaryIncidentId == incidentId)
            .Select(a => new IncidentViewer
            {
                DisciplinaryIncidentId = a.DisciplinaryIncidentId,
                AllowHearingFlag = a.AllowHearingFlag > 0,
                DispSynopsis = a.DisciplinarySynopsis,
                IncidentNumber = a.DisciplinaryNumber,
                IncidentDate = a.DisciplinaryIncidentDate,
                DispHousingUnitLocation = a.DisciplinaryHousingUnitLocation,
                DispHousingUnitNumber = a.DisciplinaryHousingUnitNumber,
                DispHousingUnitBedNumber = a.DisciplinaryHousingUnitBed,
                DispLocation = a.DisciplinaryLocation,
                DispAltSentSiteId = a.DisciplinaryAltSentSiteId,
                //  DispOtherLocation = a.DisciplinaryLocationOther,
                DispFreeForm = a.DisciplinaryLocationOther,
                DispAltSentSiteName = _context.AltSentSite
                        .Where(s => s.AltSentSiteId == a.DisciplinaryAltSentSiteId)
                .Select(s=>s.AltSentSiteName).SingleOrDefault(),
                DispOtherLocationId = a.OtherLocationID,
                DispOtherLocationName = a.OtherLocation.LocationName,
                ReportDate = a.DisciplinaryReportDate,
                DisciplinaryType = _context.Lookup.Where(x => x.LookupType == LookupConstants.DISCTYPE
                    && x.LookupIndex == a.DisciplinaryType).Select(x => x.LookupDescription)
                    .SingleOrDefault(),
                Personnel = new PersonVm
                {
                    PersonLastName = a.DisciplinaryOfficer.PersonNavigation.PersonLastName,
                    PersonFirstName = a.DisciplinaryOfficer.PersonNavigation.PersonFirstName,
                    OfficerBadgeNumber = a.DisciplinaryOfficer.OfficerBadgeNum
                },
                SupervisorAction = a.DisciplinarySupervisorAction,
                FacilityName = a.Facility.FacilityName,
                DispIncidentFlag = a.DisciplinaryIncidentFlag.Where(x => !x.DeleteFlag.HasValue)
                    .Select(x => x.IncidentFlagText).ToArray(),
                ExpectedCount = a.ExpectedNarrativeCount ?? 0,
                DispOfficerNarrativeFlag = a.DisciplinaryOfficerNarrativeFlag ?? false,
                InitialEntryFlag = a.InitialEntryFlag ?? false,
                DisciplinaryActive = a.DisciplinaryActive,
                AllowBy = new PersonnelVm
                {
                    PersonLastName = a.AllowHearingByNavigation.PersonNavigation.PersonLastName,
                    PersonFirstName = a.AllowHearingByNavigation.PersonNavigation.PersonFirstName,
                    OfficerBadgeNumber = a.AllowHearingByNavigation.OfficerBadgeNum
                },
                AllowHearingBy = a.AllowHearingBy,
                LookUpIncidentType = _context.Lookup.Where(x => x.LookupType == LookupConstants.DISCTYPE
                   && x.LookupIndex == a.DisciplinaryType).Select(x => x.LookupCategory).FirstOrDefault(),
                LookUpCategorizationType = _context.Lookup.Where(x => x.LookupType == LookupConstants.INCCAT
                && x.LookupIndex == a.IncidentCategorizationIndex).Select(x => x.LookupCategory).FirstOrDefault(),
                SensitiveMaterial=a.SensitiveMaterial,
                PreaOnly=a.PreaOnly
            }).Single();

        private List<HousingAssignVm> GetHousingLocation(int[] inmateId) => _context.Inmate
            .Where(a => inmateId.Contains(a.InmateId))
            .Select(a => new HousingAssignVm
            {
                InmateId = a.InmateId,
                HousingDetail = a.HousingUnitId.HasValue
                    ? new HousingDetail
                    {
                        FacilityAbbr = a.Facility.FacilityAbbr,
                        HousingUnitLocation = a.HousingUnit.HousingUnitLocation,
                        HousingUnitNumber = a.HousingUnit.HousingUnitNumber,
                        HousingUnitBedLocation = a.HousingUnit.HousingUnitBedLocation,
                        HousingUnitBedNumber = a.HousingUnit.HousingUnitBedNumber
                    } : default,
                Reason = a.InmateClassificationId.HasValue
                    ? a.InmateClassification.InmateClassificationReason : default
            }).ToList();

        private List<IdentifierVm> GetPhotoGraphs(int[] personId) => _context.Identifiers.Where(a => 
                personId.Contains(a.PersonId ?? 0) && a.DeleteFlag == 0)
                .OrderByDescending(a => a.CreateDate)
                .Select(a => new IdentifierVm {
                    PersonId = a.PersonId ?? 0,
                    PhotographRelativePath = _photo.GetPhotoByIdentifier(a),
                    IdentifierType = a.IdentifierType
                }).ToList();

        public List<ViolationDetails> GetViolationDetails(int incidentId, int[] dispInmateId, bool isActive = false)
        {
            List<ViolationDetails> violationDetails = _context.DisciplinaryControlXref
                .Where(a => (incidentId == 0 ||
                             a.DisciplinaryInmate.DisciplinaryIncident.DisciplinaryIncidentId == incidentId)
                            && dispInmateId.Contains(a.DisciplinaryInmate.DisciplinaryInmateId)
                            && (!isActive || (a.DisciplinaryInmate.DeleteFlag == 0 ||
                                              !a.DisciplinaryInmate.DeleteFlag.HasValue)))
                .Select(a => new ViolationDetails
                {
                    DispInmateId = a.DisciplinaryInmateId,
                    DispControlXrefId = a.DisciplinaryControlXrefId,
                    DispControlViolationId = a.DisciplinaryControlViolationId,
                    DispControlLevelId = a.DisciplinaryControlLevelId,
                    DispControlWaiverId = a.DisciplinaryControlWaiverId,
                    DispControlPleaId = a.DisciplinaryControlPleaId,
                    DispControlFindingId = a.DisciplinaryControlFindingId,
                    DispControlSanctionId = a.DisciplinaryControlSanctionId,
                    DispControlNotes = a.DisciplinaryControlNotes,
                    DispControlSanctionDays = a.DisciplinaryControlSanctionDays
                }).ToList();
            List<int?> lookupId = violationDetails.SelectMany(a => new[]
            {
                a.DispControlViolationId,
                a.DispControlLevelId,
                a.DispControlWaiverId,
                a.DispControlPleaId,
                a.DispControlFindingId,
                a.DispControlSanctionId
            }).Distinct().ToList();
            var lookupDesc = _context.DisciplinaryControlLookup.Where(a => 
                lookupId.Where(x => x.HasValue)
                .Contains(a.DisciplinaryControlLookupId)).Select(a => new
                {
                    DispControlLookupId = a.DisciplinaryControlLookupId,
                    DispControlName = a.DisciplinaryControlLookupName,
                    DispControlLookupDesc = a.DisciplinaryControlLookupDescription,
                    DispControlLookupLevel = a.DisciplinaryControlLookupLevel
                }).ToList();
            violationDetails.ForEach(a =>
            {
                a.Violation = new KeyValuePair<string, string>(
                    lookupDesc.SingleOrDefault(x => x.DispControlLookupId == a.DispControlViolationId)?.DispControlName,
                    lookupDesc.SingleOrDefault(x => x.DispControlLookupId == a.DispControlViolationId)
                        ?.DispControlLookupDesc
                );
                a.Level = lookupDesc.SingleOrDefault(x => x.DispControlLookupId == a.DispControlLevelId)
                    ?.DispControlLookupLevel;
                a.Waiver = lookupDesc.SingleOrDefault(x => x.DispControlLookupId == a.DispControlWaiverId)
                    ?.DispControlLookupDesc;
                a.Plea = lookupDesc.SingleOrDefault(x => x.DispControlLookupId == a.DispControlPleaId)
                    ?.DispControlLookupDesc;
                a.Finding = lookupDesc.SingleOrDefault(x => x.DispControlLookupId == a.DispControlFindingId)
                    ?.DispControlLookupDesc;
                a.Sanction = lookupDesc.SingleOrDefault(x => x.DispControlLookupId == a.DispControlSanctionId)
                    ?.DispControlLookupDesc;
            });
            return violationDetails;
        }

        private List<PrivilegeAlertVm> GetPrivilegeView(int inmateId, bool active = false, int privilegeDiskId = 0,
            bool activeRecordsFlag = false)
        {
            List<PrivilegeAlertVm> privilegeXrefs = _context.InmatePrivilegeXref
                .Where(a => a.Privilege.InactiveFlag == 0 && a.Inmate.InmateId == inmateId
                    && (!active || !a.PrivilegeRemoveDatetime.HasValue &&
                        !a.PrivilegeExpires.HasValue || a.PrivilegeExpires.HasValue &&
                        a.PrivilegeExpires >= DateTime.Now)
                    && (privilegeDiskId == 0 || a.PrivilegeDiscLinkId == privilegeDiskId)
                    && (!activeRecordsFlag || !a.PrivilegeRemoveDatetime.HasValue))
                .Select(a => new PrivilegeAlertVm
                {
                    InmateId = a.InmateId,
                    InmatePrivilegeXrefId = a.InmatePrivilegeXrefId,
                    PrivilegeId = a.Privilege.PrivilegeId,
                    PrivilegeDescription = a.Privilege.PrivilegeDescription,
                    PrivilegeType = a.Privilege.PrivilegeType,
                    PrivilegeDate = a.PrivilegeDate,
                    PrivilegeExpires = a.PrivilegeExpires,
                    PrivilegeRemoveDateTime = a.PrivilegeRemoveDatetime,
                    PrivilegeNote = a.PrivilegeNote,
                    PrivilegeDiscLinkId = a.PrivilegeDiscLinkId
                }).ToList();
            var incidentNumbers = _context.DisciplinaryIncident
                .Where(a => privilegeXrefs.Select(x => x.PrivilegeDiscLinkId).Contains(a.DisciplinaryIncidentId))
                .Select(a => new { a.DisciplinaryNumber, a.DisciplinaryIncidentId }).ToList();
            privilegeXrefs.ForEach(a =>
            {
                a.DisciplinaryNumber = incidentNumbers
                    .SingleOrDefault(x => x.DisciplinaryIncidentId == a.PrivilegeDiscLinkId)?.DisciplinaryNumber;
            });
            return privilegeXrefs;
        }

        private List<AppealDetailsVm> GetAppealDetails(int incidentId, int dispInmateId)
        {

            List<AppealDetailsVm> appealDetails = new List<AppealDetailsVm>();

            DisciplinaryInmateAppeal disciplinaryInmateAppeal =
                _context.DisciplinaryInmateAppeal.SingleOrDefault(w => w.DisciplinaryInmateId == dispInmateId &&
                    w.DisciplinaryInmate.DisciplinaryIncidentId == incidentId && w.ReviewComplete == 0);

            if (disciplinaryInmateAppeal is null)
                return appealDetails;

            appealDetails.Add(new AppealDetailsVm
            {
                AppealHeader = AppealConstants.APPEALDATE,
                AppealDate = disciplinaryInmateAppeal.AppealDate
            });

            appealDetails.Add(new AppealDetailsVm
            {
                AppealHeader = AppealConstants.REPORTOFFICER,
                Personnel = _context.Personnel.Where(w => w.PersonnelId == disciplinaryInmateAppeal.ReportedBy)
                    .Select(s =>
                        new PersonnelVm
                        {
                            PersonLastName = s.PersonNavigation.PersonLastName,
                            PersonFirstName = s.PersonNavigation.PersonFirstName,
                            OfficerBadgeNumber = s.OfficerBadgeNum
                        }).SingleOrDefault()
            });

            appealDetails.Add(new AppealDetailsVm
            {
                AppealHeader = AppealConstants.REASON,
                AppealDetail = _commonService.GetLookups(new[] { LookupConstants.INCAPPEALREAS })
                    .FirstOrDefault(a => a.LookupIndex == disciplinaryInmateAppeal.AppealReason)
                    ?.LookupDescription
            });

            appealDetails.Add(new AppealDetailsVm
            {
                AppealHeader = AppealConstants.APPEALNOTE,
                AppealDetail = disciplinaryInmateAppeal.AppealNote
            });

            return appealDetails;

        }

        private AppealReport GetAppealReports(int dispInmateId)
        {
            List<LookupVm> lookups = _commonService.GetLookups(new[]
                {LookupConstants.INCAPPEALREAS, LookupConstants.INCAPPEALDISPO});
            AppealReport appealReport = _context.DisciplinaryInmateAppeal
                .Where(a => a.DisciplinaryInmate.DisciplinaryInmateId == dispInmateId)
                .Select(a => new AppealReport
                {
                    AppealDate = a.AppealDate,
                    AppealReason = lookups.Where(x => x.LookupIndex == a.AppealReason
                            && x.LookupType == LookupConstants.INCAPPEALREAS)
                        .Select(x => x.LookupDescription)
                        .FirstOrDefault(),
                    ReportedBy = new PersonnelVm
                    {
                        PersonLastName = a.ReportedByNavigation.PersonNavigation.PersonLastName,
                        PersonFirstName = a.ReportedByNavigation.PersonNavigation.PersonFirstName,
                        OfficerBadgeNumber = a.ReportedByNavigation.OfficerBadgeNum
                    },
                    ReviewDiscDaysNew = a.ReviewDiscDaysNew,
                    Reviewedby = new PersonnelVm
                    {
                        PersonLastName = a.ReviewByNavigation.PersonNavigation.PersonLastName,
                        PersonFirstName = a.ReviewByNavigation.PersonNavigation.PersonFirstName,
                        OfficerBadgeNumber = a.ReviewByNavigation.OfficerBadgeNum
                    },
                    ReviewDispo = lookups.Where(x => x.LookupIndex == a.ReviewDispo
                            && x.LookupType == LookupConstants.INCAPPEALDISPO)
                        .Select(x => x.LookupDescription)
                        .FirstOrDefault(),
                    ReviewDate = a.ReviewDate,
                    AppealNote = a.AppealNote,
                    ReviewDiscDaysPrior = a.ReviewDiscDaysPrior,
                    ReviewInmateResponse = a.ReviewInmateResponse,
                    ReviewNote = a.ReviewNote
                }).FirstOrDefault();
            return appealReport;
        }

        public List<InmateFormDetails> LoadInmateFormDetails(int dispInmateId) => _context.FormRecord
                .Where(f => f.DisciplinaryInmateId == dispInmateId).Select(n => new InmateFormDetails {
                    FormName = n.FormTemplates.DisplayName,
                    FormRecordId = n.FormRecordId,
                    CreatedDate = n.CreateDate,
                    DeleteFlag = n.DeleteFlag,
                    FormTemplateId = n.FormTemplatesId
                }).ToList();

        public async Task<int> DeleteUndoInmateForm(KeyValuePair<int, bool> inmateFormDetail)
        {
            FormRecord formRecord = _context.FormRecord.Single(f => f.FormRecordId == inmateFormDetail.Key);

            formRecord.DeleteFlag = inmateFormDetail.Value ? 1 : 0;
            formRecord.DeleteDate = inmateFormDetail.Value ? DateTime.Now : (DateTime?)null;
            formRecord.DeleteBy = _personnelId;

            return await _context.SaveChangesAsync();
        }

        public List<KeyValuePair<int, string>> GetIncidnetLocations(int facilityId) =>
            _context.Privileges.Where(p => p.InactiveFlag == 0 &&
                    p.RemoveFromTrackingFlag == 0 && p.RemoveFromPrivilegeFlag == 1 && p.FacilityId == facilityId)
                .OrderBy(a => a.PrivilegeDescription)
                .Select(x => new KeyValuePair<int, string>(x.PrivilegeId, x.PrivilegeDescription)).ToList();

        public List<KeyValuePair<int, string>> GetIncidentOtherLocations(int facilityId) =>
            _context.OtherLocation.Where(p => !p.DeleteFlag
                    && !string.IsNullOrEmpty(p.LocationName) && p.FacilityId == facilityId)
                .OrderBy(a => a.OtherLocationId)
                .Select(x => new KeyValuePair<int, string>(x.OtherLocationId, x.LocationName)).ToList();

        public KeyValuePair<int, int> GetOverAllGtandDiscDays(int inmateId, DateTime incidentDate)
        {
            IQueryable<IncarcerationArrestXref> incarcerationArrestXref = _context.IncarcerationArrestXref
                .Where(a => a.Incarceration.InmateId == inmateId &&
                a.Incarceration.DateIn < incidentDate);
            int concurrentGoodTime = 0, consecutiveGoodTime = 0;
            if(incarcerationArrestXref
                .Any(a => !a.Arrest.ArrestSentenceConsecutiveFlag.HasValue ||
                a.Arrest.ArrestSentenceConsecutiveFlag == 0))
            {
                concurrentGoodTime = incarcerationArrestXref
                .Where(a => !a.Arrest.ArrestSentenceConsecutiveFlag.HasValue ||
                a.Arrest.ArrestSentenceConsecutiveFlag == 0)
                .Max(a => a.Arrest.ArrestSentenceGtDays ?? 0);
            }
            if(incarcerationArrestXref
                .Any(a => a.Arrest.ArrestSentenceConsecutiveFlag == 1))
            {
                consecutiveGoodTime = incarcerationArrestXref
                .Where(a => a.Arrest.ArrestSentenceConsecutiveFlag == 1)
                .Sum(a => a.Arrest.ArrestSentenceGtDays ?? 0);
            }
            int overAllGoodTime = concurrentGoodTime + consecutiveGoodTime;
            int discDays = incarcerationArrestXref.Sum(a=>a.Arrest.ArrestSentenceDisciplinaryDaysSum);
            return new KeyValuePair<int, int>(overAllGoodTime, discDays);
        }
    }
}

