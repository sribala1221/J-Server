using GenerateTables.Models;
using Microsoft.AspNetCore.Http;
using ServerAPI.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using ServerAPI.ViewModels;
using System.Threading.Tasks;
using System.IO;

namespace ServerAPI.Services
{
    public class PREAService : IPREAService
    {
        private readonly AAtims _context;
        private readonly int _personnelId;
        private readonly ICommonService _commonService;
        private readonly IPersonService _personService;
        private List<Lookup> _lookUp;

        public PREAService(AAtims context, IHttpContextAccessor httpContextAccessor
            , IPersonService personService, ICommonService commonService) {
            _context = context;
            _personnelId =
                Convert.ToInt32(httpContextAccessor.HttpContext.User
                    .FindFirst(AuthDetailConstants.PERSONNELID)?.Value);
            _commonService = commonService;
            _personService = personService;
        }

        public PreaDetailsVm GetClassFileDetails(int personId, int inmateId)
        {
            List<LookupVm> lookupVms = _commonService.GetLookups(new[] { LookupConstants.CLASSGROUP });
            List<string> lookupDescLst = lookupVms.Where(x => x.LookupFlag7 == 1 || x.LookupFlag8 == 1)
                 .Select(x => x.LookupDescription).ToList();

            _lookUp = _context.Lookup.Where(x =>
                x.LookupType == LookupConstants.DISCTYPE
                || x.LookupType == LookupConstants.DISCINTYPE
                || x.LookupType == LookupConstants.GRIEVTYPE
                || x.LookupType == LookupConstants.CLASSLINKTYPE
                || x.LookupType == LookupConstants.INVESTIGATIONNOTETYPE).Select(s => new Lookup
                {
                    LookupType = s.LookupType,
                    LookupDescription = s.LookupDescription,
                    LookupCategory = s.LookupCategory,
                    LookupIndex = s.LookupIndex
                }).ToList();

            PreaDetailsVm PreaCaseFile = new PreaDetailsVm {
                ActiveAssociation = _context.PersonClassification
                .Where(w => w.PersonId == personId && w.PersonClassificationDateFrom <= DateTime.Now
                && (w.PersonClassificationDateThru.HasValue ? w.PersonClassificationDateThru >= DateTime.Now
                    : w.PersonClassificationDateThru == null) && w.PersonClassificationSetComplete != 1
                    && w.InactiveFlag != 1 && lookupDescLst.Contains(w.PersonClassificationType))
                .Select(s => s.PersonClassificationType).ToList(),
                LastReview = _context.PREAReview.LastOrDefault(w => w.InmateId == inmateId 
                && !w.DeleteFlag)?.PREAReviewDate, 
                AssociationsList = GetAssocAlertDetails(personId),
                PreaFlags = GetPreaFlagDetails(personId),
                PreaForms = GetPreaForms(inmateId),
                PreaAttachments = GetAttachments(inmateId),
                PreaReview = GetPreaReviewDetails(inmateId),
                PreaNotes = GetPreaNoteDetails(inmateId),
                InvolvedInvestigations = GetInvestigationDetails(inmateId),
                InvolvedIncidents = GetIncidents(inmateId),
                InvolvedGrievances = GetGrievance(inmateId)
            };

            return PreaCaseFile;
        }
        private List<PersonClassificationDetails> GetAssocAlertDetails(int personId) {
            //Get Prea Classification group
            List<LookupVm> lookupVms = _commonService.GetLookups(new[] { LookupConstants.CLASSGROUP });
            List<int> lookupDescLst = lookupVms.Where(x => x.LookupFlag7 == 1 || x.LookupFlag8 == 1)
                 .Select(x =>Convert.ToInt32(x.LookupIndex)).ToList(); 
            List<PersonClassificationDetails> perClassify = _context.PersonClassification.Where(perClass =>
                perClass.PersonId == personId && perClass.InactiveFlag != 1 
                && lookupDescLst.Contains(perClass.PersonClassificationTypeId ?? 0))
                .Select(perClass =>
                new PersonClassificationDetails {
                    PersonClassificationId = perClass.PersonClassificationId,                  
                    ClassificationType = lookupVms.SingleOrDefault(s => s.LookupIndex == perClass.PersonClassificationTypeId).LookupDescription,
                    ClassificationNotes = perClass.PersonClassificationNotes,
                    PersonId = perClass.PersonId,
                    DateFrom = perClass.PersonClassificationDateFrom,
                    DateTo = perClass.PersonClassificationDateThru,
                    CreateDate = perClass.CreateDate,
                    PersonnelId = perClass.PersonnelId,
                    UpdateDate = perClass.UpdateDate,
                    ClassificationCompleteBy = perClass.PersonClassificationCompleteBy,
                    InactiveFlag = perClass.InactiveFlag == 1,
                    CreateById = perClass.CreatedByPersonnelId,
                    UpdateById = perClass.UpdatedByOfficerId,
                    ClassificationTypeId = perClass.PersonClassificationTypeId,
                }).ToList();

            List<int> createdIds =
                perClassify.Select(i => new[] { i.UpdateById, i.CreateById })
                    .SelectMany(i => i)
                    .Where(i => i.HasValue)
                    .Select(i => i.Value)
                    .ToList();

            List<PersonnelVm> createOfficer = _personService.GetPersonNameList(createdIds.ToList());
            perClassify.ForEach(perClass => {
                perClass.CreatedBy = createOfficer.Single(ao => ao.PersonnelId == perClass.CreateById);
                if (perClass.UpdateById.HasValue) {
                    perClass.UpdatedBy = createOfficer.Single(ao => ao.PersonnelId == perClass.UpdateById.Value);
                }
            });

            return perClassify;
        }
        private List<PreaFlagListVm> GetPreaFlagDetails(int personId) {

            List<LookupVm> lookupList = 
               _commonService.GetLookups(new[] { LookupConstants.PREAFLAGS});

            List<PreaFlagListVm> dbFlagAlertLst = _context.PersonFlag
                .Where(pf => pf.PersonId == personId && pf.DeleteFlag != 1 && pf.PREAFlagIndex > 0)
                .Select(prf => new PreaFlagListVm { 
                    // PersonFlagId will be shown only for active records
                    PersonFlagId = prf.DeleteFlag == 0 ? prf.PersonFlagId : 0,
                    DeleteFlag = prf.DeleteFlag == 1,
                    PreaFlagIndex = prf.PREAFlagIndex,
                    PreaFlagDate = prf.CreateDate,
                    PreaFlagType = lookupList.Where(x => x.LookupType == LookupConstants.PREAFLAGS
                                   &&x.LookupIndex == prf.PREAFlagIndex)
                        .Select(x => x.LookupDescription).FirstOrDefault(),
                    CreateDate = prf.CreateDate,
                    Officerbadgenumber = prf.CreateByNavigation.OfficerBadgeNum,
                    Personlastname = prf.CreateByNavigation.PersonNavigation.PersonLastName
                }).ToList();
            return dbFlagAlertLst;
        }

        // Get incident forms by incident Id.
        private List<IncarcerationForms> GetPreaForms(int inmateId) => _context.FormRecord
            .Where(a => a.PREAInmateId == inmateId && a.DeleteFlag != 1)
            .OrderByDescending(a => a.CreateDate)
            .Select(a => new IncarcerationForms {
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

        private List<PrebookAttachment> GetAttachments(int inmateId) => _context
           .AppletsSaved.Where(a => a.PREAInmateId == inmateId && a.AppletsDeleteFlag == 0 )
           .OrderByDescending(a => a.CreateDate)
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
               PREAInmateId = a.PREAInmateId,
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
        private List<PreaReviewVm> GetPreaReviewDetails(int inmateId) {
            List<LookupVm> lookupList = _commonService.GetLookups(new[] { LookupConstants.PREAFLAGS});

            List<PreaReviewVm> PreaReview = _context.PREAReview
                .Where(rev => rev.InmateId == inmateId && !rev.DeleteFlag)
                .Select(s => new PreaReviewVm {
                    PreaReviewId = s.PREAReviewId,
                    InmateId = s.InmateId,
                    PreaReviewDate = s.PREAReviewDate,
                    PreaReviewFlagId = s.PREAReviewFlagId,
                    PreaFlagType = lookupList.Where(x => x.LookupType == LookupConstants.PREAFLAGS
                                   && x.LookupIndex == s.PREAReviewFlagId)
                        .Select(x => x.LookupDescription).FirstOrDefault(),
                    PreaReviewNote = s.PREAReviewNote,
                    CreateDate = s.CreateDate,
                    CreatedBy = new PersonnelVm {
                        PersonLastName = s.CreateByNavigation.PersonNavigation.PersonLastName,
                        PersonFirstName = s.CreateByNavigation.PersonNavigation.PersonFirstName,
                        PersonMiddleName = s.CreateByNavigation.PersonNavigation.PersonMiddleName,
                        OfficerBadgeNumber = s.CreateByNavigation.OfficerBadgeNum
                    },
                    DeleteFlag = s.DeleteFlag
                }).ToList();
            return PreaReview;
        }
        private List<PreaNotesVm> GetPreaNoteDetails(int inmateId) {            
            List<LookupVm> lookupList =
               _commonService.GetLookups(new[] { LookupConstants.PREANOTETYPE});

            List<PreaNotesVm> PreaNotes = _context.PREANotes
                .Where(rev => rev.InmateId == inmateId && !rev.DeleteFlag)
                .Select(s => new PreaNotesVm {
                    PreaNotesId = s.PREANotesId,
                    InmateId = s.InmateId,
                    InvestigationFlagIndex = s.InvestigationFlagIndex,
                    PreaNoteType = lookupList.Where(x => x.LookupType == LookupConstants.PREANOTETYPE
                                   && x.LookupIndex == s.InvestigationFlagIndex)
                        .Select(x => x.LookupDescription).FirstOrDefault(),
                    PreaNote = s.PREANote,
                    CreateDate = s.CreateDate,
                    CreatedBy = new PersonnelVm {
                        PersonLastName = s.CreateByNavigation.PersonNavigation.PersonLastName,
                        PersonFirstName = s.CreateByNavigation.PersonNavigation.PersonFirstName,
                        PersonMiddleName = s.CreateByNavigation.PersonNavigation.PersonMiddleName,
                        OfficerBadgeNumber = s.CreateByNavigation.OfficerBadgeNum
                    },
                    DeleteFlag = s.DeleteFlag
                }).ToList();
            return PreaNotes;
        }
        private List<PeraInvestigation> GetInvestigationDetails(int inmateId)
        {
            List<PeraInvestigation> investigationLinkLst = _context.InvestigationLinkXref.Where(iXref => iXref.InmateId == inmateId).Select(ii =>
             new PeraInvestigation
             {
                 Type = "LINK: ",
                 StartDate = ii.InvestigationLink.Investigation.StartDate,
                 InvestigationName = ii.InvestigationLink.Investigation.InvestigationName,
                 InvestigationNumber = ii.InvestigationLink.Investigation.InvestigationNumber,
                 Notes = ii.InvestigationLink.LinkNote,
                 NoteType = _lookUp.SingleOrDefault(l =>
                                 l.LookupType == LookupConstants.CLASSLINKTYPE
                                 && l.LookupIndex == ii.InvestigationLink.LinkType).LookupDescription
             }).ToList();

            investigationLinkLst.AddRange(_context.InvestigationNotes.Where(iXref => iXref.InmateId == inmateId).Select(ii =>
            new PeraInvestigation
            {
                Type = "NOTE: ",
                StartDate = ii.Investigation.StartDate,
                InvestigationName = ii.Investigation.InvestigationName,
                InvestigationNumber = ii.Investigation.InvestigationNumber,
                Notes = ii.InvestigationNote,
                NoteType = _lookUp.SingleOrDefault(l =>
                                l.LookupType == LookupConstants.INVESTIGATIONNOTETYPE
                                && l.LookupIndex == ii.InvestigationNoteTypeId).LookupDescription
            }).ToList());

            return investigationLinkLst;
        }
        private List<PeraIncidentsOrGrievance> GetIncidents(int inmateId)
        {
            var lstDisciplinaryInmate = _context.DisciplinaryInmate
                .Where(dis => dis.InmateId == inmateId && dis.DeleteFlag != 1)
                .Select(dis => new
                {
                    dis.DisciplinaryIncidentId,
                    dis.InmateId,
                    dis.DisciplinaryInmateType
                }).ToList();

            int[] disciplinaryIds = lstDisciplinaryInmate.Select(dis => dis.DisciplinaryIncidentId).ToArray();

            return _context.DisciplinaryIncident
                .Where(dis => disciplinaryIds.Contains(dis.DisciplinaryIncidentId)).Select(ii =>
                    new PeraIncidentsOrGrievance
                    {
                        Id = ii.DisciplinaryIncidentId,
                        Number = ii.DisciplinaryNumber,
                        Type = _lookUp.SingleOrDefault(l =>
                            l.LookupType == LookupConstants.DISCTYPE
                            && l.LookupIndex == ii.DisciplinaryType).LookupDescription,
                        Date = ii.DisciplinaryIncidentDate,
                        InvolvedType = _lookUp.SingleOrDefault(l =>
                            l.LookupType == LookupConstants.DISCINTYPE
                            && l.LookupIndex == lstDisciplinaryInmate
                                .Single(inm => inm.DisciplinaryIncidentId == ii.DisciplinaryIncidentId)
                                .DisciplinaryInmateType).LookupDescription
                    }).ToList();
        }
        private List<PeraIncidentsOrGrievance> GetGrievance(int inmateId)
        {
            return _context.Grievance
                .Where(dis => dis.InmateId == inmateId && dis.DeleteFlag != 1).Select(ii =>
             new PeraIncidentsOrGrievance
             {
                 Id = ii.GrievanceId,
                 Number = ii.GrievanceNumber,
                 Date = ii.DateOccured,
                 Type = _lookUp.SingleOrDefault(l =>
                                l.LookupType == LookupConstants.GRIEVTYPE
                                && l.LookupIndex == ii.GrievanceType).LookupDescription,
             }).ToList();
        }

        //PreaQueue
        public PreaQueueDetailsVm GetPreaQueue(int facilityId)
        {
            List<LookupVm> lookupVms = _commonService.GetLookups(new[] { LookupConstants.PREAFLAGS });
            PreaQueueDetailsVm queue = new PreaQueueDetailsVm
            {
                Association = GetAssociations(facilityId),
                Flags = GetPreaFlags(facilityId, lookupVms),
                FlagReview = GetPreaReviewFlags(facilityId, lookupVms),
            };
            return queue;
        }
        private List<QueueAssocVm> GetAssociations(int facilityId)
        {
            List<LookupVm> lookupVms = _commonService.GetLookups(new[] { LookupConstants.CLASSGROUP });
            List<string> lookupDescLst = lookupVms.Where(x => x.LookupFlag7 == 1 || x.LookupFlag8 == 1)
                 .Select(x => x.LookupDescription).ToList();

            int[] inmateIdLst =
                _context.Inmate.Where(inm => inm.InmateActive == 1 && inm.FacilityId == facilityId)
                    .Select(inm => inm.PersonId).ToArray();
            List<QueueAssocVm> assocLst = _context.PersonClassification
                .Where(w=> inmateIdLst.Contains(w.PersonId) && w.InactiveFlag == 0
                           && lookupDescLst.Contains(w.PersonClassificationType))
                .GroupBy(p => new {p.PersonClassificationType})
                .Select(r => new QueueAssocVm {
                    Name = r.Key.PersonClassificationType,
                    Count = r.Count(),
                    PersonIds = r.Select(s=>s.PersonId).ToList()
                }).ToList();
           return assocLst;
        }
        private List<PreaFlagsCountVm> GetPreaReviewFlags(int facilityId, List<LookupVm> lookupVms)
        {
            List<int> lookupDescLst = lookupVms.Where(x => x.LookupCategory != null)
                 .Select(x => x.LookupIndex).ToList();

            int[] personIdLst =
                _context.Inmate.Where(inm => inm.InmateActive == 1 && inm.FacilityId == facilityId)
                    .Select(inm => inm.PersonId).ToArray();
            
            List<PreaFlagsCountVm> flagsLst = _context.PersonFlag
                .Where(w => personIdLst.Contains(w.PersonId) && w.DeleteFlag != 1 && w.PREAFlagIndex > 0
                && lookupDescLst.Contains(w.PREAFlagIndex??0))
                .GroupBy(p => new { p.PREAFlagIndex })
                .Select(r => new PreaFlagsCountVm {
                    PreaFlagIndex = r.Key.PREAFlagIndex,
                    Count = r.Count(),
                    PersonIds = r.Select(s=>s.PersonId).ToList()
                }).ToList();            
            flagsLst.ForEach(item => {                 
                Dictionary<int, DateTime?> keyValuePair = new Dictionary<int, DateTime?>();
                item.PersonIds.ForEach(info => { 
                    PREAReview pREAReview = _context.PREAReview.LastOrDefault(p => p.PREAReviewFlagId == item.PreaFlagIndex
                        && p.Inmate.PersonId == info);
                    if(pREAReview != null)
                    {
                        keyValuePair.Add(info, pREAReview.PREAReviewDate);
                    }
                    else
                    {
                        keyValuePair.Add(info, null);
                    }
                });
                item.ReviewLast = keyValuePair;
            });
            return flagsLst;
        }

        private List<PreaFlagsCountVm> GetPreaFlags(int facilityId, List<LookupVm> lookupVms)
        {            
            List<int> lookupDescLst = lookupVms.Where(w=> !string.IsNullOrEmpty(w.LookupCategory))
                 .Select(x => x.LookupIndex).ToList();

            int[] personIdLst =
                _context.Inmate.Where(inm => inm.InmateActive == 1 && inm.FacilityId == facilityId)
                    .Select(inm => inm.PersonId).ToArray();
            List<PreaFlagsCountVm> flagsLst = _context.PersonFlag
                .Where(w => personIdLst.Contains(w.PersonId) && w.DeleteFlag != 1 && w.PREAFlagIndex > 0
                && lookupDescLst.Contains(w.PREAFlagIndex??0))
                .GroupBy(p => new { p.PREAFlagIndex })
                .Select(r => new PreaFlagsCountVm
                {
                    PreaFlagIndex = r.Key.PREAFlagIndex,
                    Count = r.Count(),
                    PersonIds = r.Select(s => s.PersonId).ToList()
                }).ToList();
            flagsLst.ForEach(item => {
                Dictionary<int, DateTime?> keyValuePair = new Dictionary<int, DateTime?>();
                item.PersonIds.ForEach(info => {
                    PREAReview pREAReview = _context.PREAReview.LastOrDefault(p => p.PREAReviewFlagId == item.PreaFlagIndex
                        && p.Inmate.PersonId == info);
                    if (pREAReview != null)
                    {
                        keyValuePair.Add(info, pREAReview.PREAReviewDate);
                    }
                    else
                    {
                        keyValuePair.Add(info, (DateTime?)null);
                    }
                });
                item.ReviewLast = keyValuePair;
            });
            return flagsLst;
        }

        public List<QueueDetailsVm> GetQueueDetails(PreaQueueSearch input)
        {
            List<QueueDetailsVm> queueDetails = _context.Incarceration
                .Where(i => i.InmateId > 0 && i.Inmate.InmateActive == 1 
                && !i.ReleaseOut.HasValue
                && input.PersonIds.Contains(i.Inmate.PersonId))
                .Select(x => new QueueDetailsVm { 
                    IncarcerationId = x.IncarcerationId,
                    InmateId = x.Inmate.InmateId,
                    InmateNumber = x.Inmate.InmateNumber,
                    FacilityId = x.Inmate.FacilityId,
                    FacilityAbbr = x.Inmate.Facility.FacilityAbbr,
                    InmateClassificationId = x.Inmate.InmateClassificationId ?? 0,
                    ClassificationReason = x.Inmate.InmateClassificationId > 0
                        ? x.Inmate.InmateClassification.InmateClassificationReason
                        : "",
                    PersonDetails = new PersonInfoVm {
                        PersonId = x.Inmate.PersonId,
                        PersonLastName = x.Inmate.Person.PersonLastName,
                        PersonFirstName = x.Inmate.Person.PersonFirstName,
                        PersonMiddleName = x.Inmate.Person.PersonMiddleName,
                        PersonSexLast = x.Inmate.Person.PersonSexLast },
                    HousingDetail = x.Inmate.HousingUnitId > 0 ? new HousingDetail {
                        HousingUnitId = x.Inmate.HousingUnit.HousingUnitId,
                        HousingUnitLocation = x.Inmate.HousingUnit.HousingUnitLocation,
                        HousingUnitNumber = x.Inmate.HousingUnit.HousingUnitNumber,
                        HousingUnitBedNumber = x.Inmate.HousingUnit.HousingUnitBedNumber,
                        HousingUnitBedLocation = x.Inmate.HousingUnit.HousingUnitBedLocation
                    } : new HousingDetail(),
                }).ToList();

            // Person Classification Type
            List<int> personIdList = queueDetails.Select(s => s.PersonDetails.PersonId).ToList();
            List<PersonClassificationDetails> PreaAssociations = _context.PersonClassification
                .Where(w => personIdList.Contains(w.PersonId) && w.InactiveFlag != 1 
                && w.PersonClassificationType == input.AssocType)
                .Select(s => new PersonClassificationDetails {
                ClassificationType = s.PersonClassificationType,
                PersonId = s.PersonId,
            }).ToList();


            //prea Note List
            List<PreaNotesVm> preaNotes = _context.PREANotes.Where(w => !w.DeleteFlag == true)
                .Select(s => new PreaNotesVm {
                    InmateId = s.InmateId,
                    InvestigationFlagIndex = s.InvestigationFlagIndex,
                    DeleteFlag = s.DeleteFlag
                }).ToList();

            //Prea Review
            List<PreaReviewVm> PreaReview = _context.PREAReview.Where(w => !w.DeleteFlag == true)
                .Select(s => new PreaReviewVm {
                    PreaReviewDate = s.PREAReviewDate,
                    InmateId = s.InmateId,
                    PreaReviewFlagId = s.PREAReviewFlagId,
                    DeleteFlag = s.DeleteFlag
                }).ToList();
            queueDetails.ForEach(item => {
                item.ClassificationType = PreaAssociations.Where(x => x.PersonId == item.PersonDetails.PersonId)
                    .Select(s => s.ClassificationType).SingleOrDefault();
                if(input.PreaTypeId > 0) {
                    item.PreaReviewDate = PreaReview.LastOrDefault(x => x.InmateId == item.InmateId
                        && x.PreaReviewFlagId == input.PreaTypeId && !x.DeleteFlag)?.PreaReviewDate;
                }                
                item.PreaAssocList = GetAssocAlertDetails(item.PersonDetails.PersonId).Select(a => a).ToList();
                item.PreaReviewLst = PreaReview.Where(w => w.InmateId == item.InmateId).Select(s => s).ToList();
                item.PreaNotesLst = preaNotes.Where(w => w.InmateId == item.InmateId).Select(s => s).ToList();
            });

            return queueDetails.OrderByDescending(o=> o.PreaReviewDate).ToList();
        }
        public async Task<int> InsertPreaReview (PreaReviewVm review) {
            PREAReview reviewDetails = new PREAReview {
                InmateId = review.InmateId,
                PREAReviewDate = DateTime.Now,
                PREAReviewFlagId = review.PreaReviewFlagId,
                PREAReviewNote = review.PreaReviewNote,
                CreateDate = DateTime.Now,
                CreateBy = _personnelId,
            };
            _context.PREAReview.Add(reviewDetails);
            return await _context.SaveChangesAsync();
        }
        public async Task<int> UpdatePreaReview(PreaReviewVm review) {
            PREAReview PreaReviewDb = _context.PREAReview.Find(review.PreaReviewId);
            PreaReviewDb.UpadateDate = DateTime.Now;
            PreaReviewDb.UpdateBy = _personnelId;
            PreaReviewDb.PREAReviewDate = review.PreaReviewDate;
            PreaReviewDb.PREAReviewNote = review.PreaReviewNote;
            return await _context.SaveChangesAsync();
        }
        public async Task<int> InsertPreaNotes(PreaNotesVm notes)
        {
            PREANotes notesDetails = new PREANotes  {
                InmateId = notes.InmateId,
                InvestigationFlagIndex = notes.InvestigationFlagIndex,
                PREANote = notes.PreaNote,
                CreateDate = DateTime.Now,
                CreateBy = _personnelId,
            };
            _context.PREANotes.Add(notesDetails);
            return await _context.SaveChangesAsync();
        }
        public async Task<int> UpdatePreaNotes(PreaNotesVm review) {
            PREANotes PreaNotesDb = _context.PREANotes.Find(review.PreaNotesId);
            PreaNotesDb.UpadateDate = DateTime.Now;
            PreaNotesDb.UpdateBy = _personnelId;
            PreaNotesDb.PREANote = review.PreaNote;
            return await _context.SaveChangesAsync();
        }

        #region Assoc Alert

        public async Task<AssocAlertValidation> InsertUpdateAssocDetails(PersonClassificationDetails personAssocDetails)
        {
            AssocAlertValidation assValidate = new AssocAlertValidation();
            if (_personService.IsPersonSealed(personAssocDetails.PersonId)) //Validate person is sealed or not
            {
                assValidate.ErrorMessage = AssocErrorMessage.SealedInmate;
                return assValidate;
            }

            assValidate = ValidateAssoc(personAssocDetails);
            if (AssocErrorMessage.None != assValidate.ErrorMessage)
            {
                return assValidate;
            }

            bool isExist = false;
            PersonClassification dbPersonClassification = _context.PersonClassification
                .SingleOrDefault(pa => pa.PersonClassificationId == personAssocDetails.PersonClassificationId);

            if (dbPersonClassification?.PersonClassificationId > 0)
            {
                isExist = true;
                dbPersonClassification.UpdateDate = DateTime.Now;
                dbPersonClassification.UpdatedByOfficerId = _personnelId;
                dbPersonClassification.UpdatedByDate = DateTime.Now;
            }
            else
            {
                dbPersonClassification = new PersonClassification {
                    CreateDate = DateTime.Now,
                    CreatedByPersonnelId = _personnelId,
                    PersonnelId = _personnelId,
                    PersonClassificationDateFrom = DateTime.Now
                };
            }

            dbPersonClassification.PersonId = personAssocDetails.PersonId;
            dbPersonClassification.PersonClassificationType = personAssocDetails.ClassificationType;
            dbPersonClassification.PersonClassificationDateThru = personAssocDetails.DateTo;
            dbPersonClassification.PersonClassificationNotes = personAssocDetails.ClassificationNotes;
            dbPersonClassification.PersonClassificationSubset = personAssocDetails.ClassificationSubset;
            dbPersonClassification.PersonClassificationStatus = personAssocDetails.ClassificationStatus;
            dbPersonClassification.SuspectId = 0;
            dbPersonClassification.PersonClassificationTypeId =personAssocDetails.ClassificationTypeId;           

            if (!isExist) {
                _context.PersonClassification.Add(dbPersonClassification);
            }

            await _context.SaveChangesAsync();
            return assValidate;
        }
        public async Task<AssocAlertValidation> DeleteAssocDetails(PersonClassificationDetails personAssocDetails)
        {
            AssocAlertValidation alValidate = new AssocAlertValidation();
            if (_personService.IsPersonSealed(personAssocDetails.PersonId)) //Validate person is sealed or not
            {
                alValidate.ErrorMessage = AssocErrorMessage.SealedInmate;
                return alValidate;
            }
            if (!personAssocDetails.InactiveFlag)
            {
                alValidate = ValidateAssoc(personAssocDetails);
                if (AssocErrorMessage.None != alValidate.ErrorMessage)
                {
                    return alValidate;
                }
            }

            PersonClassification dbPersonClassification = _context.PersonClassification
                .Single(pa => pa.PersonClassificationId == personAssocDetails.PersonClassificationId);
            dbPersonClassification.UpdatedByOfficerId = _personnelId;
            dbPersonClassification.UpdatedByDate = DateTime.Now;

            if (personAssocDetails.InactiveFlag)
            {
                dbPersonClassification.InactiveFlag = 1;
                dbPersonClassification.PersonClassificationDateThru = DateTime.Now;
            }
            else
            {
                dbPersonClassification.InactiveFlag = 0;
                dbPersonClassification.PersonClassificationDateThru = null;
            }

            await _context.SaveChangesAsync();
            return alValidate;
        }
        private AssocAlertValidation ValidateAssoc(PersonClassificationDetails perAssioDetail)
        {
            AssocAlertValidation assValidate = new AssocAlertValidation();
            int inmateId = _context.Inmate.Single(inm => inm.PersonId == perAssioDetail.PersonId).InmateId;

            IQueryable<PersonClassification> personClassify = _context.PersonClassification.Where(pClass =>
                pClass.PersonId == perAssioDetail.PersonId
                && (perAssioDetail.ClassificationSubset == null ||
                    pClass.PersonClassificationSubsetId == perAssioDetail.ClassificationSubsetId)
                && (pClass.PersonClassificationDateThru == null ||
                    pClass.PersonClassificationDateThru.Value.Date > DateTime.Now.Date)
                && pClass.InactiveFlag == 0
                && pClass.PersonClassificationTypeId == perAssioDetail.ClassificationTypeId
                && (perAssioDetail.PersonClassificationId == 0 ||
                    pClass.PersonClassificationId != perAssioDetail.PersonClassificationId)
                && (perAssioDetail.ClassificationStatus == null ||
                    pClass.PersonClassificationStatus == perAssioDetail.ClassificationStatus));

            if (personClassify.Any())
            {
                assValidate.ErrorMessage = AssocErrorMessage.AssocAssigned;
                return assValidate;
            }

            if (perAssioDetail.ClassificationSubsetId > 0)
            {
                IQueryable<KeepSepSubsetInmate> subsetInmate = _context.KeepSepSubsetInmate.Where(pClass =>
                    pClass.KeepSepInmate2Id == inmateId
                    && pClass.KeepSepAssoc1Id == perAssioDetail.ClassificationTypeId
                    && (perAssioDetail.ClassificationSubsetId != null ||
                        pClass.KeepSepAssoc1SubsetId == perAssioDetail.ClassificationSubsetId));

                if (subsetInmate.Any())
                {
                    assValidate.ErrorMessage = AssocErrorMessage.KeepSepAssigned;
                    return assValidate;
                }
            }
            else
            {
                IQueryable<KeepSepAssocInmate> assocInmate = _context.KeepSepAssocInmate.Where(pClass =>
                    pClass.KeepSepInmate2Id == inmateId
                    && pClass.KeepSepAssoc1Id == perAssioDetail.ClassificationTypeId);

                if (assocInmate.Any())
                {
                    assValidate.ErrorMessage = AssocErrorMessage.KeepSepAssigned;
                    return assValidate;
                }
            }

            IQueryable<PersonClassification> personClassification = _context.PersonClassification.Where(pClass =>
                pClass.PersonId == perAssioDetail.PersonId &&
                (pClass.PersonClassificationDateThru == null || pClass.PersonClassificationDateThru > DateTime.Now) &&
                pClass.InactiveFlag == 0);

            if (!personClassification.Any()) return assValidate;
            IQueryable<KeepSepAssocAssoc> keepSepAssosAssoc1 = personClassification.SelectMany(
                p => _context.KeepSepAssocAssoc.Where(ks =>
                    ks.KeepSepAssoc1Id == p.PersonClassificationTypeId &&
                    ks.KeepSepAssoc2Id == perAssioDetail.ClassificationTypeId &&
                    ks.DeleteFlag == 0));

            IQueryable<KeepSepAssocAssoc> keepSepAssosAssoc2= personClassification.SelectMany(
                p => _context.KeepSepAssocAssoc.Where(ks =>
                    ks.KeepSepAssoc2Id == p.PersonClassificationTypeId &&
                    ks.KeepSepAssoc1Id == perAssioDetail.ClassificationTypeId &&
                    ks.DeleteFlag == 0));
            List<Lookup> lookupslist = _context.Lookup.Where(w => w.LookupType == LookupConstants.CLASSGROUP).ToList();
            List<Lookup> lookupsSublist = _context.Lookup.Where(w => w.LookupType == LookupConstants.CLASSGROUPSUB).ToList();
            if (keepSepAssosAssoc2.Any() || keepSepAssosAssoc1.Any())
            {
                if (keepSepAssosAssoc1.Any())
                {
                    int? keepAssocId =  keepSepAssosAssoc1.Select(kee => kee.KeepSepAssoc1Id).First();
                    assValidate.Association = lookupslist.SingleOrDefault(s => s.LookupIndex == keepAssocId)?.LookupDescription;                   
                }
                else if (keepSepAssosAssoc2.Any())
                {
                    int? keepAssocId =  keepSepAssosAssoc2.Select(kee => kee.KeepSepAssoc2Id).First();
                    assValidate.Association = lookupslist.SingleOrDefault(s => s.LookupIndex == keepAssocId)?.LookupDescription;                
                }
                assValidate.ErrorMessage = AssocErrorMessage.KeepSepAssocAssigned;
                return assValidate;
            }

            IQueryable<KeepSepAssocSubset> keepSepAssosSubset = personClassification.SelectMany(
                p => _context.KeepSepAssocSubset.Where(ks =>
                    ks.KeepSepAssoc1Id == p.PersonClassificationTypeId &&
                    ks.KeepSepAssoc2SubsetId == perAssioDetail.ClassificationSubsetId &&
                    ks.DeleteFlag == 0));

            if (keepSepAssosSubset.Any())
            {
                 int? keepAssocId =  keepSepAssosSubset.Select(kee => kee.KeepSepAssoc1Id).First();
                assValidate.Association = lookupslist.SingleOrDefault(s => s.LookupIndex == keepAssocId)?.LookupDescription;              
                assValidate.ErrorMessage = AssocErrorMessage.SubsetAssigned;
                return assValidate;
            }

            IQueryable<KeepSepSubsetSubset> keepSepSubsetSubset1 = personClassification.SelectMany(
                p => _context.KeepSepSubsetSubset.Where(ks =>
                    ks.KeepSepAssoc1SubsetId == p.PersonClassificationSubsetId &&
                    ks.KeepSepAssoc2SubsetId == perAssioDetail.ClassificationSubsetId &&
                    ks.DeleteFlag == 0));

            IQueryable<KeepSepSubsetSubset> keepSepSubsetSubset2 = personClassification.SelectMany(
                p => _context.KeepSepSubsetSubset.Where(ks =>
                    ks.KeepSepAssoc2SubsetId == p.PersonClassificationSubsetId &&
                    ks.KeepSepAssoc1SubsetId == perAssioDetail.ClassificationSubsetId &&
                    ks.DeleteFlag == 0));

            if (!keepSepSubsetSubset1.Any() && !keepSepSubsetSubset2.Any()) return assValidate;
            if (keepSepSubsetSubset1.Any())
            {
                int? keepAssocId =  keepSepSubsetSubset1.Select(kee => kee.KeepSepAssoc2SubsetId).First();
                assValidate.Subset = lookupsSublist.SingleOrDefault(s => s.LookupIndex == keepAssocId)?.LookupDescription;              
            }
            else if (keepSepSubsetSubset2.Any())
            {
                int? keepAssocId =  keepSepSubsetSubset2.Select(kee => kee.KeepSepAssoc2SubsetId).First();
                assValidate.Subset = lookupsSublist.SingleOrDefault(s => s.LookupIndex == keepAssocId)?.LookupDescription;               
            }
            assValidate.ErrorMessage = AssocErrorMessage.KeepSepSubsetAssigned;
            return assValidate;
        }
        #endregion
    }
}
