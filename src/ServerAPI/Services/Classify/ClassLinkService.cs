using System;
using System.Linq;
using System.Collections.Generic;
using GenerateTables.Models;
using ServerAPI.ViewModels;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;
using ServerAPI.Utilities;

namespace ServerAPI.Services
{
    [UsedImplicitly]
    public class ClassLinkService : IClassLinkService
    {
        private readonly AAtims _context;
        private readonly int _personnelId;
        private readonly ICommonService _commonService;
        private readonly IPersonService _personService;
        private readonly IInmateService _inmateService;
        public ClassLinkService(AAtims context, IHttpContextAccessor httpContextAccessor,
             ICommonService commonService, IPersonService personService, IInmateService inmateService)
        {
            _context = context;
            _personnelId = Convert.ToInt32(httpContextAccessor.HttpContext.User.FindFirst("personnelId")?.Value);
            _commonService = commonService;
            _personService = personService;
            _inmateService = inmateService;
        }
        public List<ClassLinkDetails> GetClassLinkDetails(ClassLinkInputs inputs)
        {
            List<ClassLinkDetails> classLinkDetailsVm = _context.InmateClassificationLink
                .Where(w => (inputs.OfficerId == 0 || w.CreateBy == inputs.OfficerId) &&
                (inputs.LinkType == 0 || w.LinkType == inputs.LinkType) &&
                (inputs.Last12hours == false ||
                DateTime.Now.Subtract((DateTime)w.CreateDate).TotalHours < inputs.Hours) &&
                (inputs.DateRange == false || (w.CreateDate.Value.Date >= inputs.FromDate.Value.Date &&
                w.CreateDate.Value.Date <= inputs.ToDate.Value.Date)) &&
                ((inputs.DeleteFlag ? 1 : 0) == 1 || ((w.DeleteFlag ?? 0) == (inputs.DeleteFlag ? 1 : 0))))
                .Select(s => new ClassLinkDetails
                {
                    InmateClassificationLinkId = s.InmateClassificationLinkId,
                    CreateDate = s.CreateDate,
                    LinkTypeValue = s.LinkType,
                    LinkNote = s.LinkNote,
                    LinkType = _context.Lookup.Where(w => w.LookupType == LookupConstants.CLASSLINKTYPE &&
                        w.LookupIndex == s.LinkType).Select(sl => sl.LookupDescription).SingleOrDefault(),
                    DeleteFlag = s.DeleteFlag ?? 0,
                    OfficerId = s.CreateBy
                }).ToList();
            int[] classOfficerIds = classLinkDetailsVm.Select(s => s.OfficerId).ToArray();
            List<PersonnelVm> lstPersonnel = _context.Personnel
                .Where(w => classOfficerIds.Contains(w.PersonnelId))
                .Select(s => new PersonnelVm
                {
                    OfficerBadgeNumber = s.OfficerBadgeNumber,
                    PersonnelId = s.PersonnelId,
                    PersonId = s.PersonId
                }).ToList();
            int[] personnelPersonIds = lstPersonnel.Select(p => p.PersonId).ToArray();
            List<Person> lstPersonnelPerson = _context.Person
                .Where(p => personnelPersonIds.Contains(p.PersonId)).ToList();
            lstPersonnel.ForEach(item =>
                {
                    Person person = lstPersonnelPerson.SingleOrDefault(w => w.PersonId == item.PersonId);
                    if (person != null)
                    {
                        item.PersonFirstName = person.PersonFirstName;
                        item.PersonLastName = person.PersonLastName;
                    }
                });
            int[] inmateClassificationLinkIds = classLinkDetailsVm
                .Select(s => s.InmateClassificationLinkId).ToArray();
            List<CLassLinkInmateVm> inmateDetail = _context.InmateClassificationLinkXref
                .Where(w => inmateClassificationLinkIds.Contains(w.InmateClassificationLinkId) &&
                (!w.DeleteFlag.HasValue || w.DeleteFlag == 0))
                .Select(s => new CLassLinkInmateVm
                {
                    InmateId = s.InmateId,
                    InmateClassificationLinkId = s.InmateClassificationLinkId
                }).ToList();
            int[] inmateIds = inmateDetail.Select(s => s.InmateId).ToArray();
            List<InmateDetailVm> lstInmateIds = _context.Inmate
                .Where(w => inmateIds.Contains(w.InmateId) && w.FacilityId == inputs.FacilityId)
                .Select(s => new InmateDetailVm
                {
                    InmateId = s.InmateId,
                    InmateNumber = s.InmateNumber,
                    HousingUnitId = s.HousingUnitId,
                    PersonId = s.PersonId,
                    FacilityId = s.FacilityId
                }).ToList();
            int[] inmatePersonIds = lstInmateIds.Select(p => p.PersonId).ToArray();
            List<Person> lstInmatePersonnelPerson = _context.Person
                .Where(p => inmatePersonIds.Contains(p.PersonId)).ToList();
            lstInmateIds.ForEach(item =>
                {
                    Person person = lstInmatePersonnelPerson.SingleOrDefault(w => w.PersonId == item.PersonId);
                    if (person != null)
                    {
                        PersonVm personVm = new PersonVm
                        {
                            PersonFirstName = person.PersonFirstName,
                            PersonLastName = person.PersonLastName,
                            PersonId = person.PersonId
                        };
                        item.PersonDetail = personVm;
                    }
                });
            int?[] housingUnitIds = lstInmateIds.Select(s => s.HousingUnitId).ToArray();
            List<HousingUnit> lstHousingDetails = _context.HousingUnit
                .Where(w => housingUnitIds.Contains(w.HousingUnitId)).ToList();
            lstInmateIds.ForEach(item =>
               {
                   HousingUnit housingUnit = lstHousingDetails
                    .SingleOrDefault(w => w.HousingUnitId == item.HousingUnitId);
                   if (housingUnit != null)
                   {
                       HousingDetail housingUnitVm = new HousingDetail
                       {
                           HousingUnitId = housingUnit.HousingUnitId,
                           HousingUnitLocation = housingUnit.HousingUnitLocation,
                           HousingUnitNumber = housingUnit.HousingUnitNumber,
                           HousingUnitBedNumber = housingUnit.HousingUnitBedNumber,
                           HousingUnitBedLocation = housingUnit.HousingUnitBedLocation
                       };
                       item.HousingDetail = housingUnitVm;
                   }
               });
            int[] lstFacilityinmateIds = lstInmateIds.Select(p => p.InmateId).ToArray();
            inmateDetail = inmateDetail.Where(w => lstFacilityinmateIds.Contains(w.InmateId)).ToList();
            inmateDetail.ForEach(item =>
            {
                item.InmateNumber = lstInmateIds.Single(s => s.InmateId == item.InmateId).InmateNumber;
                PersonVm person = lstInmateIds.Single(s => s.InmateId == item.InmateId).PersonDetail;
                if (person != null)
                {
                    item.PersonDetail = person;
                }
                HousingDetail housingDetail = lstInmateIds
                    .Single(s => s.InmateId == item.InmateId).HousingDetail;
                if (housingDetail != null)
                {
                    item.HousingDetail = housingDetail;
                }
            });
            int[] classLinkIds = inmateDetail.Where(w => (inputs.InmateId == 0 ||
                w.InmateId == inputs.InmateId)).Select(s => s.InmateClassificationLinkId).ToArray();
            classLinkDetailsVm = classLinkDetailsVm
                .Where(w => classLinkIds.Contains(w.InmateClassificationLinkId)).ToList();
            classLinkDetailsVm.ForEach(item =>
            {
                PersonnelVm createBy = lstPersonnel.SingleOrDefault(x => x.PersonnelId == item.OfficerId);
                item.Officer = createBy;
                item.ClassLinkInmateDetails = inmateDetail
                    .Where(w => w.InmateClassificationLinkId == item.InmateClassificationLinkId).ToList();
            });
            return classLinkDetailsVm;
        }
        public async Task<int> InsertClassifyViewerClassLink(ClassLinkAddParam inputs)
        {
            InmateClassificationLink inmateClassificationLink = new InmateClassificationLink
            {
                CreateBy = _personnelId,
                CreateDate = DateTime.Now,
                LinkNote = inputs.LinkNote,
                LinkType = inputs.LinkType
            };
            _context.InmateClassificationLink.Add(inmateClassificationLink);
            await _context.SaveChangesAsync();
            int inmateClassificationLinkId = inmateClassificationLink.InmateClassificationLinkId;
            if (inmateClassificationLinkId > 0)
            {
                foreach (int inmateId in inputs.InmateIds)
                {
                    InmateClassificationLinkXref inmateClassificationLinkXref =
                        new InmateClassificationLinkXref
                        {
                            CreateBy = _personnelId,
                            CreateDate = DateTime.Now,
                            InmateClassificationLinkId = inmateClassificationLinkId,
                            InmateId = inmateId
                        };
                    _context.InmateClassificationLinkXref.Add(inmateClassificationLinkXref);
                }
            }
            return await _context.SaveChangesAsync();
        }
        public async Task<int> UpdateClassifyViewerClassLink(ClassLinkUpdateParam inputs)
        {
            if (inputs.InmateClassificationLinkId > 0)
            {
                InmateClassificationLink inmateClassificationLink = _context.InmateClassificationLink
                    .Find(inputs.InmateClassificationLinkId);
                inmateClassificationLink.LinkType = inputs.LinkType;
                inmateClassificationLink.LinkNote = inputs.LinkNote;
                inmateClassificationLink.UpdateBy = _personnelId;
                inmateClassificationLink.UpdateDate = DateTime.Now;
            }
            List<InmateClassificationLinkXref> lstClassificationLinkXref = _context.InmateClassificationLinkXref
                .Where(w => w.InmateClassificationLinkId == inputs.InmateClassificationLinkId &&
                (!w.DeleteFlag.HasValue || w.DeleteFlag == 0))
                .Select(s => new InmateClassificationLinkXref
                {
                    InmateId = s.InmateId,
                    InmateClassificationLinkXrefId = s.InmateClassificationLinkXrefId
                }).ToList();
            int[] lstiInmateIds = inputs.InmateIds;
            int[] lstClassificationInmateIds = lstClassificationLinkXref.Select(s => s.InmateId).ToArray();
            int[] inmateNewIds = lstiInmateIds.Except(lstClassificationInmateIds).ToArray();
            int[] inmateDelIds = lstClassificationInmateIds.Except(lstiInmateIds).ToArray();
            if (inmateNewIds.Length > 0)
            {
                foreach (int inmateId in inmateNewIds)
                {
                    InmateClassificationLinkXref inmateClassificationLinkXref =
                        new InmateClassificationLinkXref
                        {
                            CreateBy = _personnelId,
                            CreateDate = DateTime.Now,
                            InmateClassificationLinkId = inputs.InmateClassificationLinkId,
                            InmateId = inmateId
                        };
                    _context.InmateClassificationLinkXref.Add(inmateClassificationLinkXref);
                }
            }
            else
            {
                foreach (int inmateId in lstiInmateIds)
                {
                    int inmateClassificationLinkXrefId = lstClassificationLinkXref
                        .Single(s => s.InmateId == inmateId).InmateClassificationLinkXrefId;
                    InmateClassificationLinkXref classificationLinkXref =
                        _context.InmateClassificationLinkXref.Find(inmateClassificationLinkXrefId);
                    classificationLinkXref.DeleteFlag = null;
                    classificationLinkXref.DeleteBy = null;
                    classificationLinkXref.DeleteDate = null;
                }
            }
            if (inmateDelIds.Length > 0)
            {
                foreach (int inmateId in inmateDelIds)
                {
                    int inmateClassificationLinkXrefId = lstClassificationLinkXref
                        .Single(s => s.InmateId == inmateId).InmateClassificationLinkXrefId;
                    InmateClassificationLinkXref classificationLinkXref =
                        _context.InmateClassificationLinkXref.Find(inmateClassificationLinkXrefId);
                    classificationLinkXref.DeleteBy = _personnelId;
                    classificationLinkXref.DeleteFlag = 1;
                    classificationLinkXref.DeleteDate = DateTime.Now;
                }
            }
            return await _context.SaveChangesAsync();
        }
        public int DeleteClassifyViewerClassLink(int inmateClassificationLinkId, bool isUndo)
        {
            InmateClassificationLink inmateClassificationLink =
                _context.InmateClassificationLink.Find(inmateClassificationLinkId);
            inmateClassificationLink.DeleteFlag = isUndo ? 0 : 1;
            inmateClassificationLink.DeleteBy = isUndo ? null : (int?)_personnelId;
            inmateClassificationLink.DeleteDate = isUndo ? null : (DateTime?)DateTime.Now;
            return _context.SaveChanges();
        }
        public List<ClassLinkViewHistoryVm> GetClassLinkViewHistory(int facilityId, int inmateId)
        {
            //Initial
            List<ClassLinkViewHistoryVm> viewHistoryInitialVm =
                _context.InmateClassification.Where(w => w.InmateId == inmateId)
                .Where(w => w.InmateClassificationType == ClassTypeConstants.INITIAL)
                .Select(s => new ClassLinkViewHistoryVm
                {
                    Id = s.InmateClassificationId,
                    InmateId = s.InmateId,
                    ClassType = ClassLinkType.INITIAL,
                    ClassDate = s.InmateDateAssigned,
                    ClassNarrative = s.InmateClassificationReason,
                    ClassOfficerId = s.ClassificationOfficerId,
                    ClassReview = true,
                    DeleteFlag = 0
                }).ToList();
            List<int> initialPersonnelId = viewHistoryInitialVm.Select(i => i.ClassOfficerId).ToList();
            List<PersonnelVm> initialLstPersonnel = _personService.GetPersonNameList(initialPersonnelId);
            int[] initialInmateIds = viewHistoryInitialVm.Select(s => s.InmateId).ToArray();
            List<PersonInfo> lstInitialInmate = _context.Inmate
                .Where(w => initialInmateIds.Contains(w.InmateId) && w.FacilityId == facilityId)
                .Select(s => new PersonInfo
                {
                    InmateId = s.InmateId,
                    InmateNumber = s.InmateNumber,
                    PersonId = s.PersonId,
                    LastReviewDate = s.LastReviewDate
                }).ToList();
            List<int> lstInitialInmateId = lstInitialInmate.Select(wc => wc.InmateId).ToList();
            List<PersonVm> lstInitialPersonDetails = _inmateService.GetInmateDetails(lstInitialInmateId);
            int[] lstInmateIds = lstInitialInmate.Select(s => s.InmateId).ToArray();
            viewHistoryInitialVm = viewHistoryInitialVm.Where(w => lstInmateIds.Contains(w.InmateId)).ToList();
            viewHistoryInitialVm.ForEach(item =>
            {
                item.PersonInfo = lstInitialPersonDetails.SingleOrDefault(s => s.InmateId == item.InmateId);
                PersonnelVm createBy = initialLstPersonnel
                    .SingleOrDefault(x => x.PersonnelId == item.ClassOfficerId);
                item.Personnel = createBy;
                item.LastReviewDate = lstInitialPersonDetails
                        .Single(s => s.InmateId == item.InmateId).LastReviewDate;
            });
            //Reclassify
            List<ClassLinkViewHistoryVm> viewHistoryReclassifyVm =
                _context.InmateClassification.Where(w => w.InmateId == inmateId)
                .Where(w => w.InmateClassificationType == ClassTypeConstants.RECLASSIFICATION)
                .Select(s => new ClassLinkViewHistoryVm
                {
                    Id = s.InmateClassificationId,
                    InmateId = s.InmateId,
                    ClassType = ClassLinkType.RECLASSIFY,
                    ClassDate = s.InmateDateAssigned,
                    ClassNarrative = s.InmateClassificationReason,
                    ClassOfficerId = s.ClassificationOfficerId,
                    ClassReview = true,
                    DeleteFlag = 0
                }).ToList();
            List<int> reClassifyPersonnelId = viewHistoryReclassifyVm.Select(i => i.ClassOfficerId).ToList();
            List<PersonnelVm> reClassifyLstPersonnel = _personService.GetPersonNameList(reClassifyPersonnelId);
            int[] reClassifyInmateIds = viewHistoryReclassifyVm.Select(s => s.InmateId).ToArray();
            List<PersonInfo> lstReclassifyInmate = _context.Inmate
                .Where(w => reClassifyInmateIds.Contains(w.InmateId) && w.FacilityId == facilityId)
                .Select(s => new PersonInfo
                {
                    InmateId = s.InmateId,
                    InmateNumber = s.InmateNumber,
                    PersonId = s.PersonId,
                    LastReviewDate = s.LastReviewDate
                }).ToList();
            List<int> lstReclassifyInmateId = lstReclassifyInmate.Select(wc => wc.InmateId).ToList();
            List<PersonVm> lstReclassifyPersonDetails = _inmateService.GetInmateDetails(lstReclassifyInmateId);
            int[] lstReclassifyInmateIds = lstReclassifyInmate.Select(s => s.InmateId).ToArray();
            viewHistoryReclassifyVm = viewHistoryReclassifyVm
                .Where(w => lstReclassifyInmateIds.Contains(w.InmateId)).ToList();
            viewHistoryReclassifyVm.ForEach(item =>
            {
                item.PersonInfo = lstReclassifyPersonDetails.SingleOrDefault(s => s.InmateId == item.InmateId);
                PersonnelVm createBy = reClassifyLstPersonnel
                    .SingleOrDefault(x => x.PersonnelId == item.ClassOfficerId);
                item.Personnel = createBy;
                item.LastReviewDate = lstReclassifyPersonDetails
                        .Single(s => s.InmateId == item.InmateId).LastReviewDate;
            });
            viewHistoryInitialVm.AddRange(viewHistoryReclassifyVm);
            //Narrative
            List<ClassLinkViewHistoryVm> viewHistoryNarrativeVm = _context.InmateClassificationNarrative
                .Where(w => w.InmateId == inmateId && w.ReviewFlag == 1)
                .Select(s => new ClassLinkViewHistoryVm
                {
                    Id = s.InmateClassificationNarrativeId,
                    InmateId = s.InmateId,
                    ClassType = ClassLinkType.NARRATIVE,
                    ClassDate = s.CreateDate,
                    ClassNarrative = s.Narrative,
                    ClassOfficerId = s.CreatedBy,
                    ClassReview = true,
                    DeleteFlag = 0
                }).ToList();
            List<int> narrativePersonnelId = viewHistoryNarrativeVm.Select(i => i.ClassOfficerId).ToList();
            List<PersonnelVm> narrativeLstPersonnel = _personService.GetPersonNameList(narrativePersonnelId);
            int[] narrativeInmateIds = viewHistoryNarrativeVm.Select(s => s.InmateId).ToArray();
            List<PersonInfo> lstNarrativeInmate = _context.Inmate
                .Where(w => narrativeInmateIds.Contains(w.InmateId) && w.FacilityId == facilityId)
                .Select(s => new PersonInfo
                {
                    InmateId = s.InmateId,
                    InmateNumber = s.InmateNumber,
                    PersonId = s.PersonId,
                    LastReviewDate = s.LastReviewDate
                }).ToList();
            List<int> lstNarrativeInmateId = lstNarrativeInmate.Select(wc => wc.InmateId).ToList();
            List<PersonVm> lstNarrativePersonDetails = _inmateService.GetInmateDetails(lstNarrativeInmateId);
            int[] lstNarrativeInmateIds = lstNarrativeInmate.Select(s => s.InmateId).ToArray();
            viewHistoryNarrativeVm = viewHistoryNarrativeVm
                .Where(w => lstNarrativeInmateIds.Contains(w.InmateId)).ToList();
            viewHistoryNarrativeVm.ForEach(item =>
            {
                item.PersonInfo = lstNarrativePersonDetails.SingleOrDefault(s => s.InmateId == item.InmateId);
                PersonnelVm createBy = narrativeLstPersonnel
                    .SingleOrDefault(x => x.PersonnelId == item.ClassOfficerId);
                item.Personnel = createBy;
                item.LastReviewDate = lstNarrativePersonDetails
                        .Single(s => s.InmateId == item.InmateId).LastReviewDate;
            });
            viewHistoryInitialVm.AddRange(viewHistoryNarrativeVm);
            //Attach
            List<ClassLinkViewHistoryVm> viewHistoryAttachVm = _context.AppletsSaved
                .Where(w => w.InmateId == inmateId && w.RegistrantRecordId == 0)
                .Select(s => new ClassLinkViewHistoryVm
                {
                    Id = s.AppletsSavedId,
                    InmateId = s.InmateId ?? 0,
                    ClassType = ClassLinkType.ATTACH,
                    ClassDate = s.CreateDate,
                    AppletsSavedType = s.AppletsSavedType,
                    AppletsSavedTitle = s.AppletsSavedTitle,
                    ClassOfficerId = s.CreatedBy,
                    ClassReview = true,
                    DeleteFlag = 0
                }).ToList();
            List<int> attachPersonnelId = viewHistoryAttachVm.Select(i => i.ClassOfficerId).ToList();
            List<PersonnelVm> attachLstPersonnel = _personService.GetPersonNameList(attachPersonnelId);
            int[] attachInmateIds = viewHistoryReclassifyVm.Select(s => s.InmateId).ToArray();
            List<PersonInfo> lstAttachInmate = _context.Inmate
                .Where(w => attachInmateIds.Contains(w.InmateId) && w.FacilityId == facilityId)
                .Select(s => new PersonInfo
                {
                    InmateId = s.InmateId,
                    InmateNumber = s.InmateNumber,
                    PersonId = s.PersonId,
                    LastReviewDate = s.LastReviewDate
                }).ToList();
            List<int> lstAttachInmateId = lstAttachInmate.Select(wc => wc.InmateId).ToList();
            List<PersonVm> lstAttachPersonDetails = _inmateService.GetInmateDetails(lstAttachInmateId);
            int[] lstAttachInmateIds = lstAttachInmate.Select(s => s.InmateId).ToArray();
            viewHistoryAttachVm = viewHistoryAttachVm.Where(w => lstAttachInmateIds
                .Contains(w.InmateId)).ToList();
            viewHistoryAttachVm.ForEach(item =>
            {
                item.PersonInfo = lstAttachPersonDetails.SingleOrDefault(s => s.InmateId == item.InmateId);
                PersonnelVm createBy = attachLstPersonnel
                    .SingleOrDefault(x => x.PersonnelId == item.ClassOfficerId);
                item.Personnel = createBy;
                item.LastReviewDate = lstAttachPersonDetails
                        .Single(s => s.InmateId == item.InmateId).LastReviewDate;
            });
            viewHistoryInitialVm.AddRange(viewHistoryAttachVm);
            //Link
            List<Inmate> lstLinkInmateDetail = _context.Inmate.Where(w => w.FacilityId == facilityId &&
                w.InmateId == inmateId)
                .Select(s => new Inmate
                {
                    InmateId = s.InmateId,
                    PersonId = s.PersonId,
                    InmateNumber = s.InmateNumber
                }).ToList();
            List<int> linkInmateIds = lstLinkInmateDetail.Select(s => s.InmateId).ToList();
            List<InmateClassificationLinkXref> inmateDetail = _context.InmateClassificationLinkXref
            .Where(w => linkInmateIds.Contains(w.InmateId) && (!w.DeleteFlag.HasValue || w.DeleteFlag == 0))
            .Select(s => new InmateClassificationLinkXref
            {
                InmateId = s.InmateId,
                InmateClassificationLinkId = s.InmateClassificationLinkId
            }).ToList();
            int[] linkInmateClassificationId = inmateDetail.Select(s => s.InmateClassificationLinkId).ToArray();
            List<ClassLinkViewHistoryVm> viewHistoryLinkVm = _context.InmateClassificationLink
            .Where(w => linkInmateClassificationId.Contains(w.InmateClassificationLinkId) &&
                !w.DeleteFlag.HasValue || w.DeleteFlag == 0)
                .Select(s => new ClassLinkViewHistoryVm
                {
                    Id = s.InmateClassificationLinkId,
                    ClassDate = s.CreateDate,
                    ClassOfficerId = s.CreateBy,
                    ClassReview = true,
                    DeleteFlag = 0,
                    LinkType = s.LinkType,
                    LinkNote = s.LinkNote,
                    ClassType = ClassLinkType.LINK

                }).ToList();
            List<int> linkPersonnelId = viewHistoryLinkVm.Select(i => i.ClassOfficerId).ToList();
            List<PersonnelVm> linkLstPersonnel = _personService.GetPersonNameList(linkPersonnelId);
            int[] linkInmateId = inmateDetail.Select(s => s.InmateId).ToArray();
            List<PersonInfo> lstLinkInmate = _context.Inmate.Where(w => linkInmateId.Contains(w.InmateId) &&
                w.FacilityId == facilityId && w.InmateId == inmateId)
                .Select(s => new PersonInfo
                {
                    InmateId = s.InmateId,
                    InmateNumber = s.InmateNumber,
                    PersonId = s.PersonId,
                    LastReviewDate = s.LastReviewDate
                }).ToList();
            List<int> lstLinkInmateId = lstLinkInmate.Select(wc => wc.InmateId).ToList();
            List<PersonVm> lstLinkPersonDetails = _inmateService.GetInmateDetails(lstLinkInmateId);
            int[] inmIds = lstLinkInmate.Select(s => s.InmateId).ToArray();
            List<InmateClassificationLinkXref> lstXref = inmateDetail
                .Where(w => inmIds.Contains(w.InmateId)).ToList();
            int[] lstLinkInmateIds = lstXref.Select(s => s.InmateClassificationLinkId).ToArray();
            viewHistoryLinkVm = viewHistoryLinkVm.Where(w => lstLinkInmateIds.Contains(w.Id)).ToList();
            viewHistoryLinkVm.ForEach(item =>
            {
                item.InmateId = lstXref.Single(s => s.InmateClassificationLinkId == item.Id).InmateId;
                item.LookupDescription = _context.Lookup.Where(w => w.LookupIndex == item.LinkType
                && w.LookupType == LookupConstants.CLASSLINKTYPE).Select(s => s.LookupDescription).SingleOrDefault();
                item.LinkNote = item.LinkNote;
                item.PersonInfo = lstLinkPersonDetails.SingleOrDefault(s => s.InmateId == item.InmateId);
                PersonnelVm createBy = linkLstPersonnel
                    .SingleOrDefault(x => x.PersonnelId == item.ClassOfficerId);
                item.Personnel = createBy;
                item.LastReviewDate = lstLinkPersonDetails.Single(s => s.InmateId == item.InmateId).LastReviewDate;
            });
            viewHistoryInitialVm.AddRange(viewHistoryLinkVm);
            //Intake
            List<ClassLinkInmateVm> lstIntakeInmate = _context.Inmate.Where(w => w.InmateId == inmateId &&
                w.FacilityId == facilityId)
                .Select(s => new ClassLinkInmateVm
                {
                    InmateId = s.InmateId,
                    InmateNumber = s.InmateNumber,
                    PersonId = s.PersonId,
                    LastReviewDate = s.LastReviewDate
                }).ToList();
            int?[] lstIntakeInmateIds = lstIntakeInmate.Select(s => s.InmateId).ToArray();
            List<ClassLinkViewHistoryVm> viewHistoryIntakeVm = _context.Incarceration
                .Where(w => lstIntakeInmateIds.Contains(w.InmateId) && w.DateIn != null && w.InmateId > 0)
                .Select(s => new ClassLinkViewHistoryVm
                {
                    Id = s.IncarcerationId,
                    InmateId = s.InmateId ?? 0,
                    ClassType = ClassLinkType.INTAKE,
                    ClassDate = s.DateIn,
                    ClassOfficerId = s.InOfficerId,
                    ClassReview = false,
                    DeleteFlag = 0
                }).ToList();
            int[] lstIntakeIncarcIds = viewHistoryIntakeVm.Select(s => s.Id).ToArray();
            List<IncarcerationArrestXref> lstIntakeIncarArrestXref = _context.IncarcerationArrestXref
            .Where(w => lstIntakeIncarcIds.Contains((int)w.IncarcerationId))
            .Select(s => new IncarcerationArrestXref
            {
                IncarcerationId = s.IncarcerationId,
                ArrestId = s.ArrestId,
                ReleaseReason = s.ReleaseReason
            }).ToList();
            int?[] lstIntakeArrestIds = lstIntakeIncarArrestXref.Select(s => s.ArrestId).ToArray();
            List<Arrest> lstIntakearrest = _context.Arrest
                .Where(w => lstIntakeArrestIds.Contains(w.ArrestId))
                .Select(s => new Arrest
                {
                    ArrestId = s.ArrestId,
                    ArrestBookingNo = s.ArrestBookingNo,
                    ArrestType = s.ArrestType
                }).ToList();
            List<int> intakePersonnelId = viewHistoryIntakeVm.Select(i => i.ClassOfficerId).ToList();
            List<PersonnelVm> intakeLstPersonnel = _personService.GetPersonNameList(intakePersonnelId);
            List<int> lstIntakeInmateId = lstIntakeInmate.Select(wc => wc.InmateId.Value).ToList();
            List<PersonVm> lstIntakePersonDetails = _inmateService.GetInmateDetails(lstIntakeInmateId);
            List<LookupVm> IntakeLookups = _commonService.GetLookups(new[] { LookupConstants.ARRTYPE });
            int?[] lstIntakeIncarIds = lstIntakeIncarArrestXref.Select(s => s.IncarcerationId).ToArray();
            lstIntakeIncarArrestXref = lstIntakeIncarArrestXref
                .Where(w => lstIntakeIncarIds.Contains(w.IncarcerationId)).ToList();
            viewHistoryIntakeVm = viewHistoryIntakeVm
                .Where(w => lstIntakeInmateIds.Contains(w.InmateId)).ToList();
            viewHistoryIntakeVm.ForEach(item =>
            {
                List<IncarcerationArrestXref> incarcerationArrestXref =
                   lstIntakeIncarArrestXref.Where(s => s.IncarcerationId == item.Id)
                   .Select(s => new IncarcerationArrestXref
                   {
                       ArrestId = s.ArrestId,
                       IncarcerationId = s.IncarcerationId,
                       ReleaseReason = s.ReleaseReason
                   }).ToList();
                int? incArrestId = incarcerationArrestXref.First(w => w.IncarcerationId == item.Id).ArrestId;
                Arrest arrest = lstIntakearrest.FirstOrDefault(s => s.ArrestId == incArrestId);
                item.Count = incarcerationArrestXref.Count(w => w.IncarcerationId == item.Id);
                if (arrest != null)
                {
                    item.ArrestBookingNo = arrest.ArrestBookingNo;
                    item.LookupDescription = IntakeLookups.Where(l => l.LookupIndex == Convert.ToInt32(arrest.ArrestType)
                            && l.LookupType == LookupConstants.ARRTYPE).Select(l => l.LookupDescription)
                        .FirstOrDefault();
                }

                item.PersonInfo = lstIntakePersonDetails.SingleOrDefault(s => s.InmateId == item.InmateId);
                PersonnelVm createBy = intakeLstPersonnel
                    .SingleOrDefault(x => x.PersonnelId == item.ClassOfficerId);
                item.Personnel = createBy;
                item.LastReviewDate = lstIntakePersonDetails
                        .Single(s => s.InmateId == item.InmateId).LastReviewDate;
            });
            viewHistoryInitialVm.AddRange(viewHistoryIntakeVm);
            //Release
            List<ClassLinkInmateVm> lstReleaseInmate = _context.Inmate.Where(w => w.InmateId == inmateId &&
                w.FacilityId == facilityId)
                .Select(s => new ClassLinkInmateVm
                {
                    InmateId = s.InmateId,
                    InmateNumber = s.InmateNumber,
                    PersonId = s.PersonId,
                    LastReviewDate = s.LastReviewDate
                }).ToList();
            int?[] lstReleaseInmateIds = lstReleaseInmate.Select(s => s.InmateId).ToArray();
            List<ClassLinkViewHistoryVm> viewHistoryReleaseVm = _context.Incarceration
                .Where(w => lstReleaseInmateIds.Contains(w.InmateId) && w.ReleaseOut != null &&
                w.DateIn != null && w.InmateId > 0)
                .Select(s => new ClassLinkViewHistoryVm
                {
                    Id = s.IncarcerationId,
                    InmateId = s.InmateId ?? 0,
                    ClassType = ClassLinkType.RELEASE,
                    ClassDate = s.ReleaseOut,
                    ClassOfficerId = s.OutOfficerId ?? 0,
                    ClassReview = false,
                    DeleteFlag = 0
                }).ToList();
            int[] lstReleaseIncarcIds = viewHistoryReleaseVm.Select(s => s.Id).ToArray();
            List<IncarcerationArrestXref> lstIncarcerationArrestXref = _context.IncarcerationArrestXref
            .Where(w => lstReleaseIncarcIds.Contains((int)w.IncarcerationId))
            .Select(s => new IncarcerationArrestXref
            {
                IncarcerationId = s.IncarcerationId,
                ArrestId = s.ArrestId,
                ReleaseReason = s.ReleaseReason
            }).ToList();
            int?[] lstReleaseArrestIds = lstIncarcerationArrestXref.Select(s => s.ArrestId).ToArray();
            List<Arrest> lstarrest = _context.Arrest.Where(w => lstReleaseArrestIds.Contains(w.ArrestId))
            .Select(s => new Arrest
            {
                ArrestId = s.ArrestId,
                ArrestBookingNo = s.ArrestBookingNo
            }).ToList();
            List<int> releasePersonnelId = viewHistoryReleaseVm.Select(i => i.ClassOfficerId).ToList();
            List<PersonnelVm> releaseLstPersonnel = _personService.GetPersonNameList(releasePersonnelId);
            List<int> lstReleaseInmateId = lstReleaseInmate.Select(wc => wc.InmateId.Value).ToList();
            List<PersonVm> lstReleasePersonDetails = _inmateService.GetInmateDetails(lstReleaseInmateId);
            viewHistoryReleaseVm = viewHistoryReleaseVm
                .Where(w => lstReleaseInmateIds.Contains(w.InmateId)).ToList();
            viewHistoryReleaseVm.ForEach(item =>
            {
                List<IncarcerationArrestXref> incarcerationArrestXref =
                    lstIncarcerationArrestXref.Where(s => s.IncarcerationId == item.Id)
                    .Select(s => new IncarcerationArrestXref
                    {
                        ArrestId = s.ArrestId,
                        IncarcerationId = s.IncarcerationId,
                        ReleaseReason = s.ReleaseReason
                    }).ToList();
                int? incArrestId = incarcerationArrestXref.First(w => w.IncarcerationId == item.Id).ArrestId;
                Arrest arrest = lstarrest.FirstOrDefault(s => s.ArrestId == incArrestId);
                item.Count = incarcerationArrestXref.Count(w => w.IncarcerationId == item.Id);
                if (arrest != null)
                {
                    item.ArrestBookingNo = arrest.ArrestBookingNo;
                    item.ReleaseReason = incarcerationArrestXref.First(w => w.IncarcerationId == item.Id).ReleaseReason;
                }
                item.PersonInfo = lstReleasePersonDetails.SingleOrDefault(s => s.InmateId == item.InmateId);
                PersonnelVm createBy = releaseLstPersonnel
                    .SingleOrDefault(x => x.PersonnelId == item.ClassOfficerId);
                item.Personnel = createBy;
                item.LastReviewDate = lstReleasePersonDetails.Single(s => s.InmateId == item.InmateId).LastReviewDate;
            });
            viewHistoryInitialVm.AddRange(viewHistoryReleaseVm);
            //Housing
            List<ClassLinkViewHistoryVm> viewHistoryHousingVm = _context.HousingUnitMoveHistory
                .Where(w => w.InmateId == inmateId)
                .Select(s => new ClassLinkViewHistoryVm
                {
                    Id = s.HousingUnitMoveHistoryId,
                    InmateId = s.InmateId,
                    ClassType = ClassLinkType.HOUSING,
                    ClassDate = s.MoveDate,
                    ClassOfficerId = s.MoveOfficerId,
                    ClassReview = false,
                    DeleteFlag = 0,
                    Reason = s.MoveReason ?? "",
                    HousingUnitToId = s.HousingUnitToId ?? 0
                }).ToList();
            List<int> housingPersonnelId = viewHistoryHousingVm.Select(i => i.ClassOfficerId).ToList();
            List<PersonnelVm> housingLstPersonnel = _personService.GetPersonNameList(housingPersonnelId);
            int[] housingInmateId = viewHistoryHousingVm.Select(s => s.InmateId).ToArray();
            List<PersonInfo> lstHousingInmate = _context.Inmate
                .Where(w => housingInmateId.Contains(w.InmateId) && w.FacilityId == facilityId)
                .Select(s => new PersonInfo
                {
                    InmateId = s.InmateId,
                    InmateNumber = s.InmateNumber,
                    PersonId = s.PersonId,
                    LastReviewDate = s.LastReviewDate,
                    FacilityId = s.FacilityId
                }).ToList();
            List<int> lstHousingInmateId = lstHousingInmate.Select(wc => wc.InmateId).ToList();
            List<PersonVm> lstHousingPersonDetails = _inmateService.GetInmateDetails(lstHousingInmateId);
            int?[] housingUnitToId = viewHistoryHousingVm.Select(s => s.HousingUnitToId).ToArray();
            List<ClassLinkHousingVm> lstHousingUnit = _context.HousingUnit
                .Where(w => housingUnitToId.Contains(w.HousingUnitId))
                .Select(s => new ClassLinkHousingVm
                {
                    HousingUnitLocation = s.HousingUnitLocation,
                    HousingUnitNumber = s.HousingUnitNumber,
                    FacilityId = s.FacilityId,
                    HousingUnitId = s.HousingUnitId
                }).ToList();
            int[] facilityIds = lstHousingUnit.Select(s => s.FacilityId).ToArray();
            List<ClassLinkHousingVm> lstFacility = _context.Facility
                .Where(w => facilityIds.Contains(w.FacilityId))
                .Select(s => new ClassLinkHousingVm
                {
                    FacilityId = s.FacilityId,
                    FacilityAbbr = s.FacilityAbbr
                }).ToList();
            lstHousingUnit.ForEach(item =>
            {
                item.FacilityAbbr = lstFacility.Single(s => s.FacilityId == item.FacilityId).FacilityAbbr;
            });

            int?[] lstHousingUnitIds = lstHousingUnit.Select(s => s.HousingUnitId).ToArray();
            viewHistoryHousingVm = viewHistoryHousingVm
                .Where(w => lstHousingUnitIds.Contains(w.HousingUnitToId)).ToList();

            int[] lstHousingInmateds = lstHousingInmate.Select(s => s.InmateId).ToArray();
            viewHistoryHousingVm = viewHistoryHousingVm
                .Where(w => lstHousingInmateds.Contains(w.InmateId)).ToList();

            viewHistoryHousingVm.ForEach(item =>
            {
                if (item.HousingUnitToId > 0)
                {
                    item.FacilityAbbr = lstHousingUnit
                        .Single(s => s.HousingUnitId == item.HousingUnitToId).FacilityAbbr;
                    item.HousingUnitLocation = lstHousingUnit
                        .Single(s => s.HousingUnitId == item.HousingUnitToId).HousingUnitLocation;
                    item.HousingUnitNumber = lstHousingUnit
                        .Single(s => s.HousingUnitId == item.HousingUnitToId).HousingUnitNumber;
                }
                item.PersonInfo = lstHousingPersonDetails.SingleOrDefault(s => s.InmateId == item.InmateId);
                PersonnelVm createBy = housingLstPersonnel
                    .SingleOrDefault(x => x.PersonnelId == item.ClassOfficerId);
                item.Personnel = createBy;
                item.LastReviewDate = lstHousingPersonDetails
                        .Single(s => s.InmateId == item.InmateId).LastReviewDate;
            });
            viewHistoryInitialVm.AddRange(viewHistoryHousingVm);
            //Note
            List<ClassLinkViewHistoryVm> viewHistoryNoteVm = _context.FloorNoteXref
                .Where(w => w.InmateId == inmateId)
                .Select(s => new ClassLinkViewHistoryVm
                {
                    Id = s.FloorNoteId,
                    InmateId = s.InmateId,
                    ClassType = ClassLinkType.NOTE,
                    ClassReview = false,
                    DeleteFlag = 0
                }).ToList();
            int[] floorNoteId = viewHistoryNoteVm.Select(s => s.Id).ToArray();
            List<FloorNotes> lstFloorNote = _context.FloorNotes.Where(w => floorNoteId.Contains(w.FloorNoteId))
                .Select(s => new FloorNotes
                {
                    FloorNoteDate = s.FloorNoteDate,
                    FloorNoteType = s.FloorNoteType,
                    FloorNoteNarrative = s.FloorNoteNarrative,
                    FloorNoteOfficerId = s.FloorNoteOfficerId,
                    FloorNoteId = s.FloorNoteId
                }).ToList();
            List<int> notesPersonnelId = lstFloorNote.Select(i => i.FloorNoteOfficerId).ToList();
            List<PersonnelVm> notesLstPersonnel = _personService.GetPersonNameList(notesPersonnelId);
            int[] notesInmateId = viewHistoryNoteVm.Select(s => s.InmateId).ToArray();
            List<PersonInfo> lstNotesInmate = _context.Inmate
                .Where(w => notesInmateId.Contains(w.InmateId) && w.FacilityId == facilityId)
                .Select(s => new PersonInfo
                {
                    InmateId = s.InmateId,
                    InmateNumber = s.InmateNumber,
                    PersonId = s.PersonId,
                    LastReviewDate = s.LastReviewDate
                }).ToList();
            List<int> lstNotesInmateId = lstNotesInmate.Select(wc => wc.InmateId).ToList();
            List<PersonVm> lstNotesPersonDetails = _inmateService.GetInmateDetails(lstNotesInmateId);
            int[] lstNoteInmateIds = lstNotesInmate.Select(s => s.InmateId).ToArray();
            viewHistoryNoteVm = viewHistoryNoteVm.Where(w => lstNoteInmateIds.Contains(w.InmateId)).ToList();
            viewHistoryNoteVm.ForEach(item =>
            {
                FloorNotes floorNotes = lstFloorNote.SingleOrDefault(w => w.FloorNoteId == item.Id);
                if (floorNotes != null)
                {
                    item.ClassOfficerId = floorNotes.FloorNoteOfficerId;
                    item.ClassDate = floorNotes.FloorNoteDate;
                    item.FloorNoteType = floorNotes.FloorNoteType;
                    item.FloorNoteNarrative = floorNotes.FloorNoteNarrative;
                    item.PersonInfo = lstNotesPersonDetails.SingleOrDefault(s => s.InmateId == item.InmateId);
                    PersonnelVm createBy = notesLstPersonnel
                        .SingleOrDefault(x => x.PersonnelId == item.ClassOfficerId);
                    item.Personnel = createBy;
                    item.LastReviewDate = lstNotesPersonDetails
                        .Single(s => s.InmateId == item.InmateId).LastReviewDate;
                }
            });
            viewHistoryInitialVm.AddRange(viewHistoryNoteVm);
            //Incident
            List<LookupVm> discLookups = _commonService.GetLookups(new[] { LookupConstants.DISCINTYPE });
            List<ClassLinkViewHistoryVm> viewHistoryIncidentVm = _context.DisciplinaryInmate
                .Where(w => w.InmateId == inmateId)
                .Select(s => new ClassLinkViewHistoryVm
                {
                    Id = s.DisciplinaryIncidentId,
                    InmateId = s.InmateId ?? 0,
                    ClassType = ClassLinkType.INCIDENT,
                    ClassReview = false,
                    DeleteFlag = s.DeleteFlag ?? 0,
                    DisciplinaryInmateType = discLookups.Where(l =>
                                    (int?)(l.LookupIndex) == s.DisciplinaryInmateType
                                    && l.LookupType == LookupConstants.DISCINTYPE).Select(l => l.LookupDescription)
                                    .FirstOrDefault()
                }).ToList();
            int[] dispIncidentId = viewHistoryIncidentVm.Select(s => s.Id).ToArray();
            List<LookupVm> lookups = _commonService.GetLookups(new[] { LookupConstants.DISCTYPE });
            List<DisciplinaryIncident> lstDispIncident = _context.DisciplinaryIncident
                .Where(w => dispIncidentId.Contains(w.DisciplinaryIncidentId))
                .Select(s => new DisciplinaryIncident
                {
                    DisciplinaryIncidentId = s.DisciplinaryIncidentId,
                    DisciplinaryReportDate = s.DisciplinaryReportDate,
                    DisciplinaryOfficerId = s.DisciplinaryOfficerId,
                    DisciplinarySynopsis = s.DisciplinarySynopsis,
                    DisciplinaryNumber = s.DisciplinaryNumber,
                    DisciplinaryLocation = lookups.Where(l =>
                                    (int?)(l.LookupIndex) == s.DisciplinaryType
                                    && l.LookupType == LookupConstants.DISCTYPE).Select(l => l.LookupDescription)
                                    .FirstOrDefault()
                }).ToList();
            List<int> incidentPersonnelId = lstDispIncident.Select(i => i.DisciplinaryOfficerId).ToList();
            List<PersonnelVm> incidentLstPersonnel = _personService.GetPersonNameList(incidentPersonnelId);
            int[] incidentInmateId = viewHistoryIncidentVm.Select(s => s.InmateId).ToArray();
            List<PersonInfo> lstIncidentInmate = _context.Inmate
                .Where(w => incidentInmateId.Contains(w.InmateId) && w.FacilityId == facilityId)
                .Select(s => new PersonInfo
                {
                    InmateId = s.InmateId,
                    InmateNumber = s.InmateNumber,
                    PersonId = s.PersonId,
                    LastReviewDate = s.LastReviewDate
                }).ToList();
            List<int> lstIncidentInmateId = lstIncidentInmate.Select(wc => wc.InmateId).ToList();
            List<PersonVm> lstIncidentPersonDetails = _inmateService.GetInmateDetails(lstIncidentInmateId);
            int[] lstIncidentInmateIds = lstIncidentInmate.Select(s => s.InmateId).ToArray();
            viewHistoryIncidentVm = viewHistoryIncidentVm
                .Where(w => lstIncidentInmateIds.Contains(w.InmateId)).ToList();
            viewHistoryIncidentVm.ForEach(item =>
            {
                DisciplinaryIncident dispIncident = lstDispIncident
                    .SingleOrDefault(w => w.DisciplinaryIncidentId == item.Id);
                if (dispIncident != null)
                {
                    item.Id = dispIncident.DisciplinaryIncidentId;
                    item.ClassDate = dispIncident.DisciplinaryReportDate;
                    item.ClassOfficerId = dispIncident.DisciplinaryOfficerId;
                    item.DisciplinaryNumber = dispIncident.DisciplinaryNumber;
                    item.DisciplinaryLocation = dispIncident.DisciplinaryLocation;
                    item.DisciplinarySynopsis = dispIncident.DisciplinarySynopsis;
                    item.PersonInfo = lstIncidentPersonDetails
                        .SingleOrDefault(s => s.InmateId == item.InmateId);
                    PersonnelVm createBy = incidentLstPersonnel
                        .SingleOrDefault(x => x.PersonnelId == item.ClassOfficerId);
                    item.Personnel = createBy;
                    item.LastReviewDate = lstIncidentPersonDetails
                        .Single(s => s.InmateId == item.InmateId).LastReviewDate;
                }
            });
            viewHistoryInitialVm.AddRange(viewHistoryIncidentVm);
            //Message
            List<Inmate> lstMsgInmateDetail = _context.Inmate
                .Where(w => w.FacilityId == facilityId && w.InmateId == inmateId)
                .Select(s => new Inmate
                {
                    InmateId = s.InmateId,
                    PersonId = s.PersonId,
                    InmateNumber = s.InmateNumber
                }).ToList();
            int[] personId = lstMsgInmateDetail.Select(s => s.PersonId).ToArray();
            List<ClassLinkViewHistoryVm> viewHistoryMessageVm = _context.PersonAlert
                .Where(w => personId.Contains(w.PersonId))
                .Select(s => new ClassLinkViewHistoryVm
                {
                    Id = s.PersonAlertId,
                    ClassDate = s.CreateDate,
                    ClassNarrative = s.Alert,
                    ClassOfficerId = s.CreatedBy,
                    PersonId = s.PersonId,
                    ClassType = ClassLinkType.MESSAGE,
                    ClassReview = false,
                    DeleteFlag = s.ActiveAlertFlag ?? 0
                }).ToList();
            List<int> messagePersonnelId = viewHistoryMessageVm.Select(i => i.ClassOfficerId).ToList();
            List<PersonnelVm> messageLstPersonnel = _personService.GetPersonNameList(messagePersonnelId);
            int[] messageInmateId = lstMsgInmateDetail.Select(s => s.InmateId).ToArray();
            List<PersonInfo> lstMessageInmate = lstMsgInmateDetail
                .Where(w => messageInmateId.Contains(w.InmateId))
                .Select(s => new PersonInfo
                {
                    InmateId = s.InmateId,
                    InmateNumber = s.InmateNumber,
                    PersonId = s.PersonId,
                    LastReviewDate = s.LastReviewDate
                }).ToList();
            List<int> lstMessageInmateId = lstMessageInmate.Select(wc => wc.InmateId).ToList();
            List<PersonVm> lstMessagePersonDetails = _inmateService.GetInmateDetails(lstMessageInmateId);
            int[] lstMessagePersonId = lstMessageInmate.Select(s => s.PersonId).ToArray();
            viewHistoryMessageVm = viewHistoryMessageVm
                .Where(w => lstMessagePersonId.Contains(w.PersonId)).ToList();
            viewHistoryMessageVm.ForEach(item =>
            {
                Inmate inmate = lstMsgInmateDetail.SingleOrDefault(s => s.PersonId == item.PersonId);
                if (inmate != null)
                {
                    item.InmateId = inmate.InmateId;
                    item.LastReviewDate = inmate.LastReviewDate;
                    item.InmateNumber = inmate.InmateNumber;
                }
                item.DeleteFlag = item.DeleteFlag == 0 ? 1 : 0;
                item.PersonInfo = lstMessagePersonDetails.SingleOrDefault(s => s.InmateId == item.InmateId);
                PersonnelVm createBy = messageLstPersonnel
                    .SingleOrDefault(x => x.PersonnelId == item.ClassOfficerId);
                item.Personnel = createBy;
                item.LastReviewDate = lstMessagePersonDetails
                        .Single(s => s.InmateId == item.InmateId).LastReviewDate;
            });
            viewHistoryInitialVm.AddRange(viewHistoryMessageVm);
            //Flag
            List<Inmate> lstFlagInmateDetail = _context.Inmate
                .Where(w => w.FacilityId == facilityId && w.InmateId == inmateId)
                .Select(s => new Inmate
                {
                    InmateId = s.InmateId,
                    PersonId = s.PersonId,
                    InmateNumber = s.InmateNumber
                }).ToList();
            int[] flagPersonId = lstFlagInmateDetail.Select(s => s.PersonId).ToArray();
            List<ClassLinkViewHistoryVm> viewHistoryFlagVm = _context.PersonFlag
                .Where(w => flagPersonId.Contains(w.PersonId) && w.DeleteFlag == 0 &&
                (w.MedicalFlagIndex ?? 0) == 0 && (w.ProgramCaseFlagIndex ?? 0) == 0)
                .Select(s => new ClassLinkViewHistoryVm
                {
                    Id = s.PersonFlagId,
                    ClassDate = s.CreateDate,
                    ClassOfficerId = s.CreateBy,
                    PersonId = s.PersonId,
                    ClassType = ClassLinkType.FLAG,
                    ClassReview = false,
                    PersonFlagIndex = s.PersonFlagIndex,
                    InmateFlagIndex = s.InmateFlagIndex,
                    DietFlagIndex = s.DietFlagIndex
                }).ToList();
            List<int> flagPersonnelId = viewHistoryFlagVm.Select(i => i.ClassOfficerId).ToList();
            List<PersonnelVm> flagLstPersonnel = _personService.GetPersonNameList(flagPersonnelId);
            int[] flagInmateId = lstFlagInmateDetail.Select(s => s.InmateId).ToArray();
            List<PersonInfo> lstFlagInmate = lstFlagInmateDetail
                .Where(w => flagInmateId.Contains(w.InmateId))
                .Select(s => new PersonInfo
                {
                    InmateId = s.InmateId,
                    InmateNumber = s.InmateNumber,
                    PersonId = s.PersonId,
                    LastReviewDate = s.LastReviewDate
                }).ToList();
            List<int> lstFlagInmateId = lstFlagInmate.Select(wc => wc.InmateId).ToList();
            List<PersonVm> lstFlagPersonDetails = _inmateService.GetInmateDetails(lstFlagInmateId);
            List<LookupVm> personcLookups = _commonService.GetLookups(new[] { LookupConstants.PERSONCAUTION });
            List<LookupVm> inmateLookups = _commonService.GetLookups(new[] { LookupConstants.TRANSCAUTION });
            List<LookupVm> dietLookups = _commonService.GetLookups(new[] { LookupConstants.DIET });
            viewHistoryFlagVm.ForEach(item =>
            {
                Inmate inmate = lstFlagInmateDetail.SingleOrDefault(s => s.PersonId == item.PersonId);
                if (inmate != null)
                {
                    item.InmateId = inmate.InmateId;
                    item.LastReviewDate = inmate.LastReviewDate;
                    item.InmateNumber = inmate.InmateNumber;
                }
                item.PersonInfo = lstFlagPersonDetails.SingleOrDefault(s => s.InmateId == item.InmateId);
                PersonnelVm createBy = flagLstPersonnel
                    .SingleOrDefault(x => x.PersonnelId == item.ClassOfficerId);
                item.Personnel = createBy;
                if (item.PersonFlagIndex > 0)
                    item.ClassNarrative = personcLookups.Where(l =>
                                (int?)(l.LookupIndex) == item.PersonFlagIndex
                                && l.LookupType == LookupConstants.PERSONCAUTION)
                                .Select(l => l.LookupDescription).FirstOrDefault();
                else if (item.InmateFlagIndex > 0)
                    item.ClassNarrative = inmateLookups.Where(l =>
                                (int?)(l.LookupIndex) == item.InmateFlagIndex
                                && l.LookupType == LookupConstants.TRANSCAUTION)
                                .Select(l => l.LookupDescription).FirstOrDefault();
                else if (item.DietFlagIndex > 0)
                    item.ClassNarrative = dietLookups.Where(l =>
                                (int?)(l.LookupIndex) == item.DietFlagIndex
                                && l.LookupType == LookupConstants.DIET).Select(l => l.LookupDescription)
                                .FirstOrDefault();
                else
                    item.ClassNarrative = "";
                item.LastReviewDate = lstFlagPersonDetails
                        .Single(s => s.InmateId == item.InmateId).LastReviewDate;
            });
            viewHistoryInitialVm.AddRange(viewHistoryFlagVm);
            //Keep Sep: Inmate          
            List<ClassLinkViewHistoryVm> viewHistoryKeepInmateVm = _context.KeepSeparate
                .Where(w => w.KeepSeparateInmate1Id == inmateId)
                .Select(s => new ClassLinkViewHistoryVm
                {
                    Id = s.KeepSeparateId,
                    ClassDate = s.KeepSeparateDate,
                    ClassOfficerId = s.KeepSeparateOfficerId,
                    ClassType = ClassLinkType.KEEPSEPINMATE,
                    ClassReview = false,
                    DeleteFlag = s.InactiveFlag,
                    InmateId = s.KeepSeparateInmate1Id,
                    Reason = s.KeepSeparateReason
                }).ToList();
            List<int> keepPersonnelId = viewHistoryKeepInmateVm.Select(i => i.ClassOfficerId).ToList();
            List<PersonnelVm> keepLstPersonnel = _personService.GetPersonNameList(keepPersonnelId);
            int[] keepInmateId = viewHistoryKeepInmateVm.Select(s => s.InmateId).ToArray();
            List<Inmate> lstKeepInmate = _context.Inmate
                .Where(w => keepInmateId.Contains(w.InmateId) && w.FacilityId == facilityId)
                .Select(s => new Inmate
                {
                    InmateId = s.InmateId,
                    InmateNumber = s.InmateNumber,
                    PersonId = s.PersonId,
                    LastReviewDate = s.LastReviewDate,
                    InmateActive = s.InmateActive,
                    HousingUnitId = s.HousingUnitId
                }).ToList();
            int?[] housingUnitIds = lstKeepInmate.Select(s => s.HousingUnitId).ToArray();
            List<HousingUnit> lstHousingDetails = _context.HousingUnit
                .Where(w => housingUnitIds.Contains(w.HousingUnitId))
                .Select(s => new HousingUnit
                {
                    HousingUnitBedLocation = s.HousingUnitBedLocation,
                    HousingUnitBedNumber = s.HousingUnitBedNumber,
                    HousingUnitNumber = s.HousingUnitNumber,
                    HousingUnitLocation = s.HousingUnitLocation,
                    HousingUnitId = s.HousingUnitId
                }).ToList();
            List<int> lstKeepInmateId = lstKeepInmate.Select(wc => wc.InmateId).ToList();
            List<PersonVm> lstKeepPersonDetails = _inmateService.GetInmateDetails(lstKeepInmateId);
            int[] lstKeepSepInmateId = lstKeepInmate.Select(s => s.InmateId).ToArray();
            viewHistoryKeepInmateVm = viewHistoryKeepInmateVm
                .Where(w => lstKeepSepInmateId.Contains(w.InmateId)).ToList();
            viewHistoryKeepInmateVm.ForEach(item =>
            {
                if (lstKeepInmate.Count > 0)
                {
                    item.PersonId = lstKeepInmate.Single(s => s.InmateId == item.InmateId).PersonId;
                    Inmate inmate = lstKeepInmate.SingleOrDefault(s => s.PersonId == item.PersonId);
                    if (inmate != null)
                    {
                        item.InmateId = inmate.InmateId;
                        item.LastReviewDate = inmate.LastReviewDate;
                        item.InmateNumber = inmate.InmateNumber;
                        item.HousingUnitToId = inmate.HousingUnitId;
                        item.InmateActive = inmate.InmateActive;
                    }
                    HousingUnit housingUnit = lstHousingDetails
                        .SingleOrDefault(s => s.HousingUnitId == item.HousingUnitToId);
                    if (housingUnit != null)
                    {
                        item.HousingUnitLocation = housingUnit.HousingUnitLocation;
                        item.HousingUnitNumber = housingUnit.HousingUnitNumber;
                        item.HousingUnitBedNumber = housingUnit.HousingUnitBedNumber;
                        item.HousingUnitBedLocation = housingUnit.HousingUnitBedLocation;
                    }
                }
                item.PersonLastName = lstKeepPersonDetails
                           .Single(s => s.InmateId == item.InmateId).PersonLastName;
                item.PersonFirstName = lstKeepPersonDetails
                    .Single(s => s.InmateId == item.InmateId).PersonFirstName;
                item.PersonMiddleName = lstKeepPersonDetails
                    .Single(s => s.InmateId == item.InmateId).PersonMiddleName ?? "";
                item.PersonInfo = lstKeepPersonDetails.SingleOrDefault(s => s.InmateId == item.InmateId);
                PersonnelVm createBy = keepLstPersonnel
                    .SingleOrDefault(x => x.PersonnelId == item.ClassOfficerId);
                item.Personnel = createBy;
                item.LastReviewDate = lstKeepPersonDetails
                        .Single(s => s.InmateId == item.InmateId).LastReviewDate;
            });
            viewHistoryInitialVm.AddRange(viewHistoryKeepInmateVm);
            //Keep Sep:Inmate
            List<ClassLinkViewHistoryVm> viewHistoryKeepSetInmateVm = _context.KeepSeparate
                .Where(w => w.KeepSeparateInmate2Id == inmateId)
                .Select(s => new ClassLinkViewHistoryVm
                {
                    Id = s.KeepSeparateId,
                    ClassDate = s.KeepSeparateDate,
                    ClassOfficerId = s.KeepSeparateOfficerId,
                    ClassType = ClassLinkType.KEEPSEPINMATENEW,
                    ClassReview = false,
                    DeleteFlag = s.InactiveFlag,
                    InmateId = s.KeepSeparateInmate2Id,
                    Reason = s.KeepSeparateReason
                }).ToList();
            List<int> keepSetPersonnelId = viewHistoryKeepSetInmateVm.Select(i => i.ClassOfficerId).ToList();
            List<PersonnelVm> keepSetLstPersonnel = _personService.GetPersonNameList(keepSetPersonnelId);
            int[] keepSetInmateId = viewHistoryKeepSetInmateVm.Select(s => s.InmateId).ToArray();
            List<Inmate> lstKeepSetInmate = _context.Inmate
                .Where(w => keepSetInmateId.Contains(w.InmateId) && w.FacilityId == facilityId)
                .Select(s => new Inmate
                {
                    InmateId = s.InmateId,
                    InmateNumber = s.InmateNumber,
                    PersonId = s.PersonId,
                    LastReviewDate = s.LastReviewDate,
                    InmateActive = s.InmateActive,
                    HousingUnitId = s.HousingUnitId
                }).ToList();
            int?[] housingUnitSetIds = lstKeepSetInmate.Select(s => s.HousingUnitId).ToArray();
            List<HousingUnit> lstHousingSetDetails = _context.HousingUnit
                .Where(w => housingUnitSetIds.Contains(w.HousingUnitId))
                .Select(s => new HousingUnit
                {
                    HousingUnitBedLocation = s.HousingUnitBedLocation,
                    HousingUnitBedNumber = s.HousingUnitBedNumber,
                    HousingUnitNumber = s.HousingUnitNumber,
                    HousingUnitLocation = s.HousingUnitLocation,
                    HousingUnitId = s.HousingUnitId
                }).ToList();
            List<int> lstKeepSetInmateId = lstKeepSetInmate.Select(wc => wc.InmateId).ToList();
            List<PersonVm> lstKeepSetPersonDetails = _inmateService.GetInmateDetails(lstKeepSetInmateId);
            int[] lstMessageKeepsInmateId = lstKeepSetInmate.Select(s => s.InmateId).ToArray();
            viewHistoryKeepSetInmateVm = viewHistoryKeepSetInmateVm
                .Where(w => lstMessageKeepsInmateId.Contains(w.InmateId)).ToList();
            viewHistoryKeepSetInmateVm.ForEach(item =>
            {
                if (lstKeepSetInmate.Count > 0)
                {
                    item.PersonId = lstKeepSetInmate
                        .Single(s => s.InmateId == item.InmateId).PersonId;
                    Inmate inmate = lstKeepSetInmate.SingleOrDefault(s => s.PersonId == item.PersonId);
                    if (inmate != null)
                    {
                        item.InmateId = inmate.InmateId;
                        item.LastReviewDate = inmate.LastReviewDate;
                        item.InmateNumber = inmate.InmateNumber;
                        item.HousingUnitToId = inmate.HousingUnitId;
                        item.InmateActive = inmate.InmateActive;
                    }
                    HousingUnit housingUnit = lstHousingSetDetails
                        .SingleOrDefault(s => s.HousingUnitId == item.HousingUnitToId);
                    if (housingUnit != null)
                    {
                        item.PersonLastName = lstKeepSetPersonDetails
                            .Single(s => s.InmateId == item.InmateId).PersonLastName;
                        item.PersonFirstName = lstKeepSetPersonDetails
                            .Single(s => s.InmateId == item.InmateId).PersonFirstName;
                        item.PersonMiddleName = lstKeepSetPersonDetails
                            .Single(s => s.InmateId == item.InmateId).PersonMiddleName;
                        item.HousingUnitLocation = housingUnit.HousingUnitLocation;
                        item.HousingUnitNumber = housingUnit.HousingUnitNumber;
                        item.HousingUnitBedNumber = housingUnit.HousingUnitBedNumber;
                        item.HousingUnitBedLocation = housingUnit.HousingUnitBedLocation;
                    }
                }
                item.PersonInfo = lstKeepSetPersonDetails.SingleOrDefault(s => s.InmateId == item.InmateId);
                PersonnelVm createBy = keepSetLstPersonnel
                    .SingleOrDefault(x => x.PersonnelId == item.ClassOfficerId);
                item.Personnel = createBy;
                item.LastReviewDate = lstKeepSetPersonDetails
                        .Single(s => s.InmateId == item.InmateId).LastReviewDate;
            });
            viewHistoryInitialVm.AddRange(viewHistoryKeepSetInmateVm);
            //Keep Sep: Assoc
            List<Lookup> lookupslist = _context.Lookup.Where(w => w.LookupType == LookupConstants.CLASSGROUP).ToList();
            List<Lookup> lookupSubsetlist = _context.Lookup.Where(w => w.LookupType == LookupConstants.CLASSGROUPSUB).ToList();
            List<ClassLinkViewHistoryVm> viewHistoryKeepAssoInmateVm = _context.KeepSepAssocInmate
                .Where(w => w.KeepSepInmate2Id == inmateId)
                .Select(s => new ClassLinkViewHistoryVm
                {
                    Id = s.KeepSepAssocInmateId,
                    ClassDate = s.CreateDate,
                    ClassOfficerId = s.KeepSepOfficerId,
                    KeepSepAssoc1 = lookupslist.SingleOrDefault(d => d.LookupIndex == s.KeepSepAssoc1Id).LookupDescription,
                    KeepSepAssoc1Id = s.KeepSepAssoc1Id,              
                    KeepSepReason = s.KeepSepReason,
                    ClassType = ClassLinkType.KEEPSEPASSOC,
                    ClassReview = false,
                    DeleteFlag = s.DeleteFlag,
                    InmateId = s.KeepSepInmate2Id
                }).ToList();
            List<int> keepAssoPersonnelId = viewHistoryKeepAssoInmateVm.Select(i => i.ClassOfficerId).ToList();
            List<PersonnelVm> keepAssoLstPersonnel = _personService.GetPersonNameList(keepAssoPersonnelId);
            int[] keepAssoInmateId = viewHistoryKeepAssoInmateVm.Select(s => s.InmateId).ToArray();
            List<Inmate> lstKeepAssoInmate = _context.Inmate.Where(w => keepAssoInmateId.Contains(w.InmateId)
            && w.FacilityId == facilityId)
            .Select(s => new Inmate
            {
                InmateId = s.InmateId,
                InmateNumber = s.InmateNumber,
                PersonId = s.PersonId,
                LastReviewDate = s.LastReviewDate
            }).ToList();
            List<int> lstKeepAssoInmateId = lstKeepAssoInmate.Select(wc => wc.InmateId).ToList();
            List<PersonVm> lstKeepAssoPersonDetails = _inmateService.GetInmateDetails(lstKeepAssoInmateId);
            int[] lstKeepAssocInmateId = lstKeepAssoInmate.Select(s => s.InmateId).ToArray();
            viewHistoryKeepAssoInmateVm = viewHistoryKeepAssoInmateVm
                .Where(w => lstKeepAssocInmateId.Contains(w.InmateId)).ToList();
            viewHistoryKeepAssoInmateVm.ForEach(item =>
            {
                if (lstKeepAssoInmate.Count > 0)
                {
                    Inmate inmate = lstKeepAssoInmate.SingleOrDefault(s => s.PersonId == item.PersonId);
                    if (inmate != null)
                    {
                        item.InmateId = inmate.InmateId;
                        item.LastReviewDate = inmate.LastReviewDate;
                        item.InmateNumber = inmate.InmateNumber;
                    }
                }
                item.PersonInfo = lstKeepAssoPersonDetails.SingleOrDefault(s => s.InmateId == item.InmateId);
                PersonnelVm createBy = keepAssoLstPersonnel
                    .SingleOrDefault(x => x.PersonnelId == item.ClassOfficerId);
                item.Personnel = createBy;
                item.LastReviewDate = lstKeepAssoPersonDetails
                        .Single(s => s.InmateId == item.InmateId).LastReviewDate;
            });
            viewHistoryInitialVm.AddRange(viewHistoryKeepAssoInmateVm);
            //Keep Sep: Subset
            List<ClassLinkViewHistoryVm> viewHistoryKeepSubSetInmateVm = _context.KeepSepSubsetInmate
                .Where(w => w.KeepSepInmate2Id == inmateId)
                .Select(s => new ClassLinkViewHistoryVm
                {
                    Id = s.KeepSepSubsetInmateId,
                    ClassDate = s.CreateDate,
                    ClassOfficerId = s.KeepSepOfficerId,
                    KeepSepAssoc1Subset = lookupSubsetlist.SingleOrDefault(f => f.LookupIndex == s.KeepSepAssoc1SubsetId).LookupDescription,
                    KeepSepAssoc1SubsetId = s.KeepSepAssoc1SubsetId,      
                    KeepSepReason = s.KeepSepReason,
                    ClassType = ClassLinkType.KEEPSEPSUBSET,
                    ClassReview = false,
                    DeleteFlag = s.DeleteFlag,
                    InmateId = s.KeepSepInmate2Id
                }).ToList();
            List<int> keepSubSetPersonnelId =
                viewHistoryKeepSubSetInmateVm.Select(i => i.ClassOfficerId).ToList();
            List<PersonnelVm> keepSubSetLstPersonnel = _personService.GetPersonNameList(keepSubSetPersonnelId);
            int[] keepSubSetInmateId = viewHistoryKeepSubSetInmateVm.Select(s => s.InmateId).ToArray();
            List<Inmate> lstKeepSubSetInmate = _context.Inmate
                .Where(w => keepSubSetInmateId.Contains(w.InmateId) && w.FacilityId == facilityId)
                .Select(s => new Inmate
                {
                    InmateId = s.InmateId,
                    InmateNumber = s.InmateNumber,
                    PersonId = s.PersonId,
                    LastReviewDate = s.LastReviewDate
                }).ToList();
            List<int> lstKeepSubSetInmateId = lstKeepSubSetInmate.Select(wc => wc.InmateId).ToList();
            List<PersonVm> lstKeepSubSetPersonDetails = _inmateService.GetInmateDetails(lstKeepSubSetInmateId);
            int[] lstSubsetInmateId = lstKeepSubSetInmate.Select(s => s.InmateId).ToArray();
            viewHistoryKeepSubSetInmateVm = viewHistoryKeepSubSetInmateVm
                .Where(w => lstSubsetInmateId.Contains(w.InmateId)).ToList();
            viewHistoryKeepSubSetInmateVm.ForEach(item =>
            {
                if (lstKeepSubSetInmate.Count > 0)
                {
                    Inmate inmate = lstKeepSubSetInmate.SingleOrDefault(s => s.PersonId == item.PersonId);
                    if (inmate != null)
                    {
                        item.InmateId = inmate.InmateId;
                        item.LastReviewDate = inmate.LastReviewDate;
                        item.InmateNumber = inmate.InmateNumber;
                    }
                }
                item.PersonInfo = lstKeepSubSetPersonDetails.SingleOrDefault(s => s.InmateId == item.InmateId);
                PersonnelVm createBy = keepSubSetLstPersonnel
                    .SingleOrDefault(x => x.PersonnelId == item.ClassOfficerId);
                item.Personnel = createBy;
                item.LastReviewDate = lstKeepSubSetPersonDetails
                        .Single(s => s.InmateId == item.InmateId).LastReviewDate;
            });
            viewHistoryInitialVm.AddRange(viewHistoryKeepSubSetInmateVm);
            //Assoc
            List<Inmate> lstAssocInmateDetail = _context.Inmate
                .Where(w => w.FacilityId == facilityId && w.InmateId == inmateId)
                .Select(s => new Inmate
                {
                    InmateId = s.InmateId,
                    PersonId = s.PersonId,
                    InmateNumber = s.InmateNumber
                }).ToList();
            int[] assocPersonId = lstAssocInmateDetail.Select(s => s.PersonId).ToArray();
            List<ClassLinkViewHistoryVm> viewHistoryAssocVm = _context.PersonClassification
                .Where(w => assocPersonId.Contains(w.PersonId))
                .Select(s => new ClassLinkViewHistoryVm
                {
                    Id = s.PersonClassificationId,
                    ClassDate = s.PersonClassificationDateFrom,
                    ClassOfficerId = s.CreatedByPersonnelId,
                    PersonId = s.PersonId,
                    ClassType = ClassLinkType.ASSOC,
                    ClassReview = false,
                    PersonClassificationTypeId = s.PersonClassificationTypeId,
                    PersonClassificationSubSetId = s.PersonClassificationSubsetId,
                    PersonClassificationType = lookupslist.SingleOrDefault(f => f.LookupIndex == s.PersonClassificationTypeId).LookupDescription,
                    PersonClassificationSubSet = lookupSubsetlist.SingleOrDefault(f => f.LookupIndex == s.PersonClassificationSubsetId).LookupDescription,   
                    PersonClassificationStatus = s.PersonClassificationStatus,
                    PersonClassificationDateThru = s.PersonClassificationDateThru,
                    PersonClassificationNotes = s.PersonClassificationNotes,
                    DeleteFlag = s.InactiveFlag
                }).ToList();
            List<int> assocPersonnelId = viewHistoryAssocVm.Select(i => i.ClassOfficerId).ToList();
            List<PersonnelVm> assocLstPersonnel = _personService.GetPersonNameList(assocPersonnelId);
            int[] assocInmateId = lstAssocInmateDetail.Select(s => s.InmateId).ToArray();
            List<PersonInfo> lstAssocInmate = lstAssocInmateDetail
                .Where(w => assocInmateId.Contains(w.InmateId))
                .Select(s => new PersonInfo
                {
                    InmateId = s.InmateId,
                    InmateNumber = s.InmateNumber,
                    PersonId = s.PersonId,
                    LastReviewDate = s.LastReviewDate
                }).ToList();
            List<int> lstAssocInmateId = lstAssocInmate.Select(wc => wc.InmateId).ToList();
            List<PersonVm> lstAssocPersonDetails = _inmateService.GetInmateDetails(lstAssocInmateId);
            viewHistoryAssocVm.ForEach(item =>
            {
                Inmate inmate = lstAssocInmateDetail.SingleOrDefault(s => s.PersonId == item.PersonId);
                if (inmate != null)
                {
                    item.InmateId = inmate.InmateId;
                    item.LastReviewDate = inmate.LastReviewDate;
                    item.InmateNumber = inmate.InmateNumber;
                }
                item.PersonInfo = lstAssocPersonDetails.SingleOrDefault(s => s.InmateId == item.InmateId);
                PersonnelVm createBy = assocLstPersonnel
                    .SingleOrDefault(x => x.PersonnelId == item.ClassOfficerId);
                item.Personnel = createBy;
                item.LastReviewDate = lstAssocPersonDetails
                        .Single(s => s.InmateId == item.InmateId).LastReviewDate;
            });
            viewHistoryInitialVm.AddRange(viewHistoryAssocVm);
            //Privileges
            List<ClassLinkViewHistoryVm> viewHistoryPrivilegesVm = _context.InmatePrivilegeXref
                .Where(w => w.InmateId == inmateId)
                .Select(s => new ClassLinkViewHistoryVm
                {
                    Id = s.InmatePrivilegeXrefId,
                    InmateId = s.InmateId,
                    ClassDate = s.CreateDate,
                    ClassOfficerId = s.PrivilegeOfficerId,
                    ClassType = ClassLinkType.PRIVILEGES,
                    ClassReview = false,
                    PrivilegeId = s.PrivilegeId,
                    PrivilegeDate = s.PrivilegeDate,
                    PrivilegeNote = s.PrivilegeNote ?? "",
                    PrivilegeExpires = s.PrivilegeExpires
                }).ToList();
            List<int> privilegesPersonnelId = viewHistoryPrivilegesVm.Select(i => i.ClassOfficerId).ToList();
            List<PersonnelVm> privilegesLstPersonnel = _personService.GetPersonNameList(privilegesPersonnelId);
            int[] privilegesInmateId = viewHistoryPrivilegesVm.Select(s => s.InmateId).ToArray();
            List<Inmate> lstPrivilegesInmate = _context.Inmate
                .Where(w => privilegesInmateId.Contains(w.InmateId) &&
            w.FacilityId == facilityId)
            .Select(s => new Inmate
            {
                InmateId = s.InmateId,
                InmateNumber = s.InmateNumber,
                PersonId = s.PersonId,
                LastReviewDate = s.LastReviewDate
            }).ToList();
            List<int> lstPrivilegesInmateId = lstPrivilegesInmate.Select(wc => wc.InmateId).ToList();
            List<PersonVm> lstPrivilegesPersonDetails = _inmateService.GetInmateDetails(lstPrivilegesInmateId);
            int[] privilagesId = viewHistoryPrivilegesVm.Select(s => s.PrivilegeId).ToArray();
            List<Privileges> lstPrivileges = _context.Privileges
                .Where(w => privilagesId.Contains(w.PrivilegeId))
                .Select(s => new Privileges
                {
                    PrivilegeDescription = s.PrivilegeDescription ?? "",
                    PrivilegeId = s.PrivilegeId
                }).ToList();
            viewHistoryPrivilegesVm.ForEach(item =>
            {
                Inmate inmate = lstPrivilegesInmate.SingleOrDefault(s => s.PersonId == item.PersonId);
                if (inmate != null)
                {
                    item.InmateId = inmate.InmateId;
                    item.LastReviewDate = inmate.LastReviewDate;
                    item.InmateNumber = inmate.InmateNumber;
                }
                Privileges privileges = lstPrivileges.SingleOrDefault(s => s.PrivilegeId == item.PrivilegeId);
                if (privileges != null)
                {
                    item.PrivilegeDescription = privileges.PrivilegeDescription;
                }
                item.PersonInfo = lstPrivilegesPersonDetails.SingleOrDefault(s => s.InmateId == item.InmateId);
                PersonnelVm createBy = privilegesLstPersonnel
                .SingleOrDefault(x => x.PersonnelId == item.ClassOfficerId);
                item.Personnel = createBy;
                item.LastReviewDate = lstPrivilegesPersonDetails
                        .Single(s => s.InmateId == item.InmateId).LastReviewDate;
            });
            viewHistoryInitialVm.AddRange(viewHistoryPrivilegesVm);
            viewHistoryInitialVm = viewHistoryInitialVm
                .Where(w => w.DeleteFlag == 0).OrderByDescending(o => o.ClassDate).ToList();

            return viewHistoryInitialVm;
        }

    }
}