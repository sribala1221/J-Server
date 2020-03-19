using GenerateTables.Models;
using ServerAPI.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using ServerAPI.Interfaces;
using ServerAPI.ViewModels;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

// ReSharper disable once CheckNamespace
namespace ServerAPI.Services
{
    public class ClassifyAlertsKeepsepService : IClassifyAlertsKeepsepService
    {

        private readonly AAtims _context;
        private List<HousingUnit> _housingUnitLst;
        private readonly IFacilityHousingService _facilityHousingService;
        private readonly int _personnelId;
       
        public ClassifyAlertsKeepsepService(AAtims context, IHttpContextAccessor httpContextAccessor,
            IFacilityHousingService facilityHousingService)
        {
            _context = context;
            _facilityHousingService = facilityHousingService;
            _personnelId = Convert.ToInt32(httpContextAccessor.HttpContext.User
                    .FindFirst(AuthDetailConstants.PERSONNELID)?.Value);
        }

        public HousingUnitListDetailVm GetHousingBuildingDetails(int facilityId)
        {
            HousingUnitListDetailVm housingUnitListDetail = new HousingUnitListDetailVm();

            // Housing Details
            IQueryable<HousingUnit> housingUnit =
                _context.HousingUnit.Where(hu => !hu.HousingUnitInactive.HasValue || hu.HousingUnitInactive == 0);
            _housingUnitLst = facilityId > 0
                 ? housingUnit.Where(hu => hu.FacilityId == facilityId).ToList()
                 : housingUnit.ToList();

            //Housing Building
            housingUnitListDetail.HousingBuilding = _housingUnitLst.Select(s => s.HousingUnitLocation.Trim())
                .Distinct().ToList();

            //Housing Number

            _housingUnitLst = _housingUnitLst.Where(hu =>
                !string.IsNullOrWhiteSpace(hu.HousingUnitLocation)).ToList();

            housingUnitListDetail.HousingNumber = _housingUnitLst
                .GroupBy(g => new { g.HousingUnitListId, g.HousingUnitLocation, g.HousingUnitNumber })
                .Select(s => new HousingUnitListVm
                {
                    HousingUnitListId = s.Key.HousingUnitListId,
                    HousingUnitLocation = s.Key.HousingUnitLocation,
                    HousingUnitNumber = s.Key.HousingUnitNumber
                }).OrderBy(o => o.HousingUnitLocation).ThenBy(t => t.HousingUnitNumber).Distinct().ToList();

            //Housing Group

            housingUnitListDetail.HousingGroups = _context.HousingGroup.Where(h => h.DeleteFlag == 0 &&
            h.FacilityId == facilityId)
               .Select(s => new HousingGroupAssignVm()
               {
                   HousingGroupId = s.HousingGroupId,
                   HousingGroupName = s.GroupName
               }
               ).ToList();
            return housingUnitListDetail;
        }

        public KeepSeparateAlertVm GetAlertKeepInmateList(KeepSepSearchVm keepSepSearch)
        {
            KeepSeparateAlertVm keepSepAlertDetails = new KeepSeparateAlertVm();
            List<int> housingUnitListIds = new List<int>();

            keepSepAlertDetails.HousingDetails = GetHousingBuildingDetails(keepSepSearch.FacilityId);

            if (keepSepSearch.HousingGroupId > 0)
            {
                housingUnitListIds = _context.HousingGroupAssign.Where(h =>
                        h.DeleteFlag == 0 && (keepSepSearch.HousingGroupId > 0
                        || h.HousingGroupId == keepSepSearch.HousingGroupId)
                    && h.HousingGroup.FacilityId == keepSepSearch.FacilityId)
                    .Select(s => s.HousingUnitListId ?? 0).ToList();
            }


            //KeepSeparateInmateList
            //INMATE KEEP SEPARATE To ASSOCIATION
            keepSepAlertDetails.KeepSeparateInmateList = _context.KeepSepAssocInmate.Where(
                k => (keepSepSearch.HousingGroupId == 0 ||
                        housingUnitListIds.Contains(k.KeepSepInmate2.HousingUnit.HousingUnitListId))
                    && (keepSepSearch.GenderId == 0 || k.KeepSepInmate2.Person.PersonSexLast == keepSepSearch.GenderId)
                    && (keepSepSearch.RaceId == 0 || k.KeepSepInmate2.Person.PersonRaceLast == keepSepSearch.RaceId)
                    && (!keepSepSearch.IllegalAlienOnly ||
                        k.KeepSepInmate2.Person.IllegalAlienFlag == keepSepSearch.IllegalAlienOnly)
                    && (string.IsNullOrEmpty(keepSepSearch.Classify)
                        || k.KeepSepInmate2.InmateClassification.InmateClassificationReason == keepSepSearch.Classify)
                    && (keepSepSearch.AssociationId == 0 || k.KeepSepAssoc1Id == keepSepSearch.AssociationId)
                    && (keepSepSearch.InmateId == 0 || k.KeepSepInmate2Id == keepSepSearch.InmateId)
                    && k.KeepSepInmate2.Facility.FacilityId == keepSepSearch.FacilityId
                    && (keepSepSearch.HousingUnitListId == 0 || k.KeepSepInmate2.HousingUnit.HousingUnitListId ==
                        keepSepSearch.HousingUnitListId)
            ).Select(
                ks => new KeepSeparateVm
                {
                    KeepSepAssocInmateId = ks.KeepSepAssocInmateId,
                    KeepSeparateId = ks.KeepSepAssocInmateId,
                    KeepSepInmateId = ks.KeepSepInmate2Id,
                    KeepSepLabel = KeepSepLabel.ASSOC,
                    PersonId = ks.KeepSepInmate2.PersonId,
                    InmateNumber = ks.KeepSepInmate2.InmateNumber,
                    Number = ks.KeepSepInmate2.InmateNumber,
                    KeepSepType = ks.KeepSeparateType,
                    KeepSepReason = ks.KeepSepReason,
                    HousingUnitId = ks.KeepSepInmate2.HousingUnitId,
                    InmateActive = ks.KeepSepInmate2.InmateActive == 1,
                    KeepSeparateNote = ks.KeepSeparateNote,
                    keepsep = ks.KeepSepAssoc1,
                    KeepSeparateDate = ks.KeepSepDate,
                    Deleteflag = ks.DeleteFlag,
                    RaceId = ks.KeepSepInmate2.Person.PersonRaceLast,
                    GenderId = ks.KeepSepInmate2.Person.PersonSexLast,
                    AssocId = ks.KeepSepAssoc1Id,
                    HousingUnitListId = ks.KeepSepInmate2.HousingUnit.HousingUnitListId
                }).ToList();

            //INMATE KEEP SEPARATE To SUBSET
            keepSepAlertDetails.KeepSeparateInmateList.AddRange(_context.KeepSepSubsetInmate.Where(k =>
                (keepSepSearch.HousingGroupId == 0 ||
                    housingUnitListIds.Contains(k.KeepSepInmate2.HousingUnit.HousingUnitListId))
                && (keepSepSearch.GenderId == 0 || k.KeepSepInmate2.Person.PersonSexLast == keepSepSearch.GenderId)
                && (keepSepSearch.RaceId == 0 || k.KeepSepInmate2.Person.PersonRaceLast == keepSepSearch.RaceId)
                && (!keepSepSearch.IllegalAlienOnly ||
                    k.KeepSepInmate2.Person.IllegalAlienFlag == keepSepSearch.IllegalAlienOnly)
                && (string.IsNullOrEmpty(keepSepSearch.Classify)
                    || k.KeepSepInmate2.InmateClassification.InmateClassificationReason == keepSepSearch.Classify)
                && (keepSepSearch.InmateId == 0 || k.KeepSepInmate2Id == keepSepSearch.InmateId)
                && k.KeepSepInmate2.Facility.FacilityId == keepSepSearch.FacilityId
                && (keepSepSearch.HousingUnitListId == 0 ||
                    k.KeepSepInmate2.HousingUnit.HousingUnitListId == keepSepSearch.HousingUnitListId)
            ).Select(
                kss => new KeepSeparateVm
                {
                    KeepSepSubsetInmateId = kss.KeepSepSubsetInmateId,
                    KeepSeparateId = kss.KeepSepSubsetInmateId,
                    KeepSepInmateId = kss.KeepSepInmate2Id,
                    KeepSepLabel = KeepSepLabel.SUBSET,
                    KeepSepAssoc1SubsetId = kss.KeepSepAssoc1SubsetId,
                    KeepSepType = kss.KeepSeparateType,
                    KeepSepReason = kss.KeepSepReason,
                    PersonId = kss.KeepSepInmate2.PersonId,
                    InmateNumber = kss.KeepSepInmate2.InmateNumber,
                    HousingUnitId = kss.KeepSepInmate2.HousingUnitId,
                    InmateActive = kss.KeepSepInmate2.InmateActive == 1,
                    KeepSeparateNote = kss.KeepSeparateNote,
                    keepsep = kss.KeepSepAssoc1,
                    keepSepAssoc1Subset = kss.KeepSepAssoc1Subset,
                    KeepSeparateDate = kss.KeepSepDate,
                    Deleteflag = kss.DeleteFlag,
                }));

            //INMATE KEEP SEPARATE To INMATE :KeepSeparateInmateId 1 
            List<KeepSeparateVm> kepSeparateDetails = _context.KeepSeparate.Where(
                k =>
                    (keepSepSearch.HousingGroupId == 0 ||
                        housingUnitListIds.Contains(k.KeepSeparateInmate2.HousingUnit.HousingUnitListId))
                    && (keepSepSearch.GenderId == 0 ||
                        k.KeepSeparateInmate2.Person.PersonSexLast == keepSepSearch.GenderId)
                    && (keepSepSearch.RaceId == 0 ||
                        k.KeepSeparateInmate2.Person.PersonRaceLast == keepSepSearch.RaceId)
                    && (!keepSepSearch.IllegalAlienOnly || k.KeepSeparateInmate2.Person.IllegalAlienFlag ==
                        keepSepSearch.IllegalAlienOnly)
                    && (keepSepSearch.InmateId == 0 || k.KeepSeparateInmate2Id == keepSepSearch.InmateId || k.KeepSeparateInmate1Id == keepSepSearch.InmateId)
                    && (string.IsNullOrEmpty(keepSepSearch.Classify)
                        || k.KeepSeparateInmate2.InmateClassification.InmateClassificationReason ==
                        keepSepSearch.Classify)
                    && k.KeepSeparateInmate1.InmateActive == 1
                    && k.KeepSeparateInmate2.InmateActive == 1
                    && k.KeepSeparateInmate2.FacilityId == keepSepSearch.FacilityId
                    && (keepSepSearch.HousingUnitListId == 0 || k.KeepSeparateInmate2.HousingUnit.HousingUnitListId ==
                        keepSepSearch.HousingUnitListId)

            ).Select(ks =>

                new KeepSeparateVm
                {
                    KeepSeparateId = ks.KeepSeparateId,
                    KeepSepInmateId = ks.KeepSeparateInmate1Id,
                    InmateNumber = ks.KeepSeparateInmate1.InmateNumber,
                    HousingUnitListId = ks.KeepSeparateInmate1.HousingUnit.HousingUnitListId,
                    PersonId = ks.KeepSeparateInmate1.PersonId,
                    KeepSepInmate2Id = ks.KeepSeparateInmate2Id,
                    KeepSepLabel = KeepSepLabel.INMATE,
                    KeepSepPersonId = ks.KeepSeparateInmate2.PersonId,
                    keepSepHousingUnitId = ks.KeepSeparateInmate2.HousingUnitId,
                    keepSepHousingUnitListId = ks.KeepSeparateInmate2.HousingUnit.HousingUnitListId,
                    KeepSepInmateNumber = ks.KeepSeparateInmate2.InmateNumber,
                    KeepSepType = ks.KeepSeparateType,
                    KeepSepReason = ks.KeepSeparateReason,
                    HousingUnitId = ks.KeepSeparateInmate1.HousingUnitId,
                    InmateActive = ks.KeepSeparateInmate2.InmateActive == 1,
                    KeepSeparateNote = ks.KeepSeparateNote,
                    KeepSeparateDate = ks.KeepSeparateDate,
                    Deleteflag = ks.InactiveFlag

                }).ToList();

            //INMATE KEEP SEPARATE To INMATE :KeepSeparateInmateId 2

            kepSeparateDetails.AddRange(_context.KeepSeparate.Where(k =>
                    (keepSepSearch.HousingGroupId == 0 ||
                        housingUnitListIds.Contains(k.KeepSeparateInmate2.HousingUnit.HousingUnitListId)) &&
                    keepSepSearch.GenderId > 0 && (k.KeepSeparateInmate2.Person.PersonSexLast == keepSepSearch.GenderId
                        || k.KeepSeparateInmate1.Person.PersonSexLast == keepSepSearch.GenderId) &&
                    (keepSepSearch.RaceId == 0 ||
                        k.KeepSeparateInmate2.Person.PersonRaceLast == keepSepSearch.RaceId) &&
                    (!keepSepSearch.IllegalAlienOnly || k.KeepSeparateInmate2.Person.IllegalAlienFlag ==
                        keepSepSearch.IllegalAlienOnly) &&
                    (keepSepSearch.InmateId == 0 || k.KeepSeparateInmate2Id == keepSepSearch.InmateId ||
                        k.KeepSeparateInmate1Id == keepSepSearch.InmateId) && k.KeepSeparateInmate1.InmateActive == 1 &&
                    k.KeepSeparateInmate2.InmateActive == 1 &&
                    k.KeepSeparateInmate2.FacilityId == keepSepSearch.FacilityId &&
                    (string.IsNullOrEmpty(keepSepSearch.Classify)
                        || k.KeepSeparateInmate2.InmateClassification.InmateClassificationReason ==
                        keepSepSearch.Classify) && (keepSepSearch.HousingUnitListId == 0
                        || k.KeepSeparateInmate2.HousingUnit.HousingUnitListId == keepSepSearch.HousingUnitListId)
            ).Select(ks => new KeepSeparateVm
                {
                    KeepSeparateId = ks.KeepSeparateId,
                    KeepSepInmateId = ks.KeepSeparateInmate2Id,
                    KeepSepInmate2Id = ks.KeepSeparateInmate1Id,
                    InmateNumber = ks.KeepSeparateInmate2.InmateNumber,
                    HousingUnitListId = ks.KeepSeparateInmate2.HousingUnit.HousingUnitListId,
                    KeepSepLabel = KeepSepLabel.INMATE,
                    PersonId = ks.KeepSeparateInmate2.PersonId,
                    KeepSepPersonId = ks.KeepSeparateInmate1.PersonId,
                    KeepSepInmateNumber = ks.KeepSeparateInmate1.InmateNumber,
                    keepSepHousingUnitId = ks.KeepSeparateInmate1.HousingUnitId,
                    keepSepHousingUnitListId = ks.KeepSeparateInmate1.HousingUnit.HousingUnitListId,
                    KeepSepType = ks.KeepSeparateType,
                    KeepSepReason = ks.KeepSeparateReason,
                    HousingUnitId = ks.KeepSeparateInmate2.HousingUnitId,
                    InmateActive = ks.KeepSeparateInmate1.InmateActive == 1,
                    KeepSeparateNote = ks.KeepSeparateNote,
                    KeepSeparateDate = ks.KeepSeparateDate,
                    Deleteflag = ks.InactiveFlag
                }).ToList());
            kepSeparateDetails = kepSeparateDetails.GroupBy(g => new
            {
                g.KeepSepAssocInmateId,
                g.KeepSepSubsetInmateId,
                g.KeepSeparateId,
                g.KeepSepInmateId,
                g.KeepSepInmate2Id,
                g.KeepSepLabel,
                g.PersonId,
                g.InmateNumber,
                g.KeepSepInmateNumber,
                g.KeepSepType,
                g.KeepSepReason,
                g.HousingUnitId,
                g.KeepSeparateNote,
                g.KeepSeparateDate,
                g.Deleteflag,
                g.keepsep,
                g.KeepSepAssoc1SubsetId,
                g.keepSepAssoc1Subset,
                g.KeepSepPersonId,
                g.HousingUnitListId,
                g.keepSepHousingUnitId,
                g.keepSepHousingUnitListId
            }).Select(s => new KeepSeparateVm
            {
                KeepSepAssocInmateId = s.Key.KeepSepAssocInmateId,
                KeepSepSubsetInmateId = s.Key.KeepSepSubsetInmateId,
                KeepSeparateId = s.Key.KeepSeparateId,
                KeepSepInmateId = s.Key.KeepSepInmateId,
                KeepSepInmate2Id = s.Key.KeepSepInmate2Id,
                KeepSepLabel = s.Key.KeepSepLabel,
                PersonId = s.Key.PersonId,
                KeepSepInmateNumber = s.Key.KeepSepInmateNumber,
                InmateNumber = s.Key.InmateNumber,
                KeepSepType = s.Key.KeepSepType,
                KeepSepReason = s.Key.KeepSepReason,
                HousingUnitId = s.Key.HousingUnitId,
                KeepSeparateNote = s.Key.KeepSeparateNote,
                KeepSeparateDate = s.Key.KeepSeparateDate,
                Deleteflag = s.Key.Deleteflag,
                keepsep = s.Key.keepsep,
                KeepSepAssoc1SubsetId = s.Key.KeepSepAssoc1SubsetId,
                keepSepAssoc1Subset = s.Key.keepSepAssoc1Subset,
                KeepSepPersonId = s.Key.KeepSepPersonId,
                HousingUnitListId = s.Key.HousingUnitListId,
                keepSepHousingUnitId = s.Key.keepSepHousingUnitId,
                keepSepHousingUnitListId = s.Key.keepSepHousingUnitListId
            }).ToList();

            keepSepAlertDetails.KeepSeparateInmateList.AddRange(kepSeparateDetails);

             List<int> personIds = keepSepAlertDetails.KeepSeparateInmateList.Select(ks => ks.PersonId).Distinct()
               .ToList();
            List<PersonInfo> personDetailLst = _context.Person.Where(pr => personIds.Contains(pr.PersonId))
                .Select(p => new PersonInfo
                {
                    PersonId = p.PersonId,
                    PersonFirstName = p.PersonFirstName,
                    PersonMiddleName = p.PersonMiddleName,
                    PersonLastName = p.PersonLastName,
                }).ToList();

            List<int> keepSepPersonIds = keepSepAlertDetails.KeepSeparateInmateList.Select(ks => ks.KeepSepPersonId).Distinct()
                .ToList();
            List<PersonInfo> keepPersonDetailLst = _context.Person.Where(pr => keepSepPersonIds.Contains(pr.PersonId))
                .Select(p => new PersonInfo
                {
                    PersonId = p.PersonId,
                    PersonFirstName = p.PersonFirstName,
                    PersonMiddleName = p.PersonMiddleName,
                    PersonLastName = p.PersonLastName,
                }).ToList();

            keepSepAlertDetails.KeepSeparateInmateList.ForEach(item =>
            {
                if (item.HousingUnitId.HasValue)
                {
                    item.HousingDetail = _facilityHousingService.GetHousingDetails(item.HousingUnitId.Value);
                    item.HousingUnitListId = item.HousingDetail.HousingUnitListId;
                }
                if (item.keepSepHousingUnitId.HasValue)
                {
                    item.KeepsepHousingDetail = _facilityHousingService.GetHousingDetails(item.keepSepHousingUnitId.Value);
                    item.keepSepHousingUnitListId = item.KeepsepHousingDetail.HousingUnitListId;
                }

                if (item.KeepSepPersonId > 0)
                {
                    PersonInfo person = keepPersonDetailLst.Single(p => p.PersonId == item.KeepSepPersonId);
                    item.KeepSepPersonFirstName = person.PersonFirstName;
                    item.KeepSepPersonMiddleName = person.PersonMiddleName;
                    item.KeepSepPersonLastName = person.PersonLastName;
                }
                if (item.PersonId > 0)
                {
                    PersonInfo person = personDetailLst.Single(p => p.PersonId == item.PersonId);
                    item.PersonFirstName = person.PersonFirstName;
                    item.PersonLastName = person.PersonLastName;
                    item.PersonMiddleName = person.PersonMiddleName;
                }

            });
            return keepSepAlertDetails;

        }

        public KeepSeparateAlertVm GetKeepSeparateAssocSubsetList(KeepSepSearchVm keepSepSearch)
        {
            KeepSeparateAlertVm keepSepAlertDetails = new KeepSeparateAlertVm
            {
                KeepSeparateAssocList = new List<KeepSeparateVm>(),
                KeepSeparateSubsetList = new List<KeepSeparateVm>()
            };

            List<int> housingUnitListIds = new List<int>();

            keepSepAlertDetails.HousingDetails = GetHousingBuildingDetails(keepSepSearch.FacilityId);

            if (keepSepSearch.HousingGroupId > 0)
            {
                housingUnitListIds = _context.HousingGroupAssign.Where(h =>
                        h.DeleteFlag == 0 && (keepSepSearch.HousingGroupId > 0
                            || h.HousingGroupId == keepSepSearch.HousingGroupId)
                        && h.HousingGroup.FacilityId == keepSepSearch.FacilityId)
                    .Select(s => s.HousingUnitListId ?? 0).ToList();
            }
          
            // KeepSeparateAssocList
            //Person Classificatio Details
            DateTime dtTime = DateTime.Now;
            List<PersonClassification> personClassification = _context.PersonClassification.Where(pc =>
                pc.InactiveFlag == 0
                && pc.PersonClassificationDateFrom < DateTime.Now &&
                (pc.PersonClassificationDateThru != null
                    ? pc.PersonClassificationDateThru.Value.AddDays(1) >= DateTime.Now
                    : dtTime.AddDays(1) >= DateTime.Now)
                && (keepSepSearch.AssociationId == 0 || pc.PersonClassificationTypeId == keepSepSearch.AssociationId)
                && (keepSepSearch.SubsetId == 0 || pc.PersonClassificationSubsetId == keepSepSearch.SubsetId)
                && (keepSepSearch.HousingUnitListId == 0 || pc.Person.Inmate.Any(i =>
                    i.HousingUnit.HousingUnitListId == keepSepSearch.HousingUnitListId))
                && (keepSepSearch.HousingGroupId == 0 || pc.Person.Inmate.Any(a =>
                    housingUnitListIds.Any(s => s == a.HousingUnit.HousingUnitListId)))
                && (keepSepSearch.IllegalAlienOnly == false ||
                    pc.Person.IllegalAlienFlag == keepSepSearch.IllegalAlienOnly)
                && pc.Person.Inmate.Any(i => i.InmateActive == 1)
                && (keepSepSearch.FacilityId == 0 ||
                    pc.Person.Inmate.Any(i => i.FacilityId == keepSepSearch.FacilityId))
                && (string.IsNullOrEmpty(keepSepSearch.Classify) ||
                    pc.Person.Inmate.Any(i => _context.InmateClassification.Any(ic => ic.InmateId == i.InmateId &&
                        ic.InmateClassificationReason == keepSepSearch.Classify
                        && !i.InmateClassification.InmateDateUnassigned.HasValue)))).ToList();
            
            List<Lookup> lookupslist = _context.Lookup.Where(w => w.LookupType == LookupConstants.CLASSGROUP).ToList();
            List<Lookup> lookupsSublist = _context.Lookup.Where(w => w.LookupType == LookupConstants.CLASSGROUPSUB).ToList();
            
            // KEEP SEPARATE ASSOCIATION To ASSOCIATION 1
            List<KeepSeparateVm> associationList = _context.KeepSepAssocAssoc
                .SelectMany(ksaa => personClassification.Where(pc =>
                pc.PersonClassificationTypeId == ksaa.KeepSepAssoc1Id)
                .GroupBy(g => g.PersonClassificationTypeId),

                    (ksaa, pc) => new KeepSeparateVm
                    {
                        KeepSepAssocAssocId = ksaa.KeepSepAssocAssocId,
                        KeepSeparateId = ksaa.KeepSepAssocAssocId,
                        KeepSepLabel = KeepSepLabel.ASSOC,
                        keepsep = ksaa.KeepSepAssoc2,
                        Assoc = lookupslist.SingleOrDefault(s => s.LookupIndex == ksaa.KeepSepAssoc1Id).LookupDescription,
                        Deleteflag = ksaa.DeleteFlag,
                        AssocId = ksaa.KeepSepAssoc1Id,
                        KeepSepReason = ksaa.KeepSepReason
                    }).ToList();

            // KEEP SEPARATE ASSOCIATION To ASSOCIATION 2
            associationList.AddRange(_context.KeepSepAssocAssoc
                .SelectMany(ksaa => personClassification.Where(pc =>
                pc.PersonClassificationTypeId == ksaa.KeepSepAssoc2Id)
                .GroupBy(g => g.PersonClassificationTypeId),
                (ksaa, pc) => new KeepSeparateVm
                      {
                          KeepSepAssocAssocId = ksaa.KeepSepAssocAssocId,
                          KeepSeparateId =ksaa.KeepSepAssocAssocId,
                          KeepSepLabel = KeepSepLabel.ASSOC,
                          keepsep = ksaa.KeepSepAssoc1,
                          Assoc = lookupslist.SingleOrDefault(s => s.LookupIndex == ksaa.KeepSepAssoc2Id).LookupDescription,
                          Deleteflag = ksaa.DeleteFlag,
                          AssocId = ksaa.KeepSepAssoc2Id,
                          KeepSepReason = ksaa.KeepSepReason
                      }
              ));

            keepSepAlertDetails.KeepSeparateAssocList.AddRange(associationList);

            // KEEP SEPARATE ASSOCIATION To INMATE
            keepSepAlertDetails.KeepSeparateAssocList.AddRange(_context.KeepSepAssocInmate
                .Where(ksai => ksai.KeepSepInmate2.InmateActive == 1
                    && (keepSepSearch.GenderId == 0 ||
                        ksai.KeepSepInmate2.Person.PersonSexLast == keepSepSearch.GenderId)
                    && (keepSepSearch.RaceId == 0 || ksai.KeepSepInmate2.Person.PersonRaceLast == keepSepSearch.RaceId)
                    && (keepSepSearch.InmateId == 0 || ksai.KeepSepInmate2.InmateId == keepSepSearch.InmateId)
                    && (keepSepSearch.HousingUnitListId == 0 ||
                        ksai.KeepSepInmate2.HousingUnit.HousingUnitListId == keepSepSearch.HousingUnitListId)
                    && (keepSepSearch.HousingGroupId == 0
                        || housingUnitListIds.Contains(ksai.KeepSepInmate2.HousingUnit.HousingUnitListId))
                    && (keepSepSearch.IllegalAlienOnly == false
                        || ksai.KeepSepInmate2.Person.IllegalAlienFlag == keepSepSearch.IllegalAlienOnly)
                    && !ksai.KeepSepInmate2.InmateClassification.InmateDateUnassigned.HasValue
                    && (string.IsNullOrEmpty(keepSepSearch.Classify)
                        || ksai.KeepSepInmate2.InmateClassification.InmateClassificationReason ==
                        keepSepSearch.Classify)
                    && ksai.KeepSepInmate2.Facility.FacilityId == keepSepSearch.FacilityId
                )
                .Select(ksai => new KeepSeparateVm
                {
                    KeepSepAssocInmateId = ksai.KeepSepAssocInmateId,
                    KeepSeparateId = ksai.KeepSepAssocInmateId,
                    AssocId = ksai.KeepSepAssoc1Id,
                    Assoc = lookupslist.SingleOrDefault(s => s.LookupIndex == ksai.KeepSepAssoc1Id).LookupDescription,
                    KeepSepInmateId = ksai.KeepSepInmate2.InmateId,
                    InmateNumber = ksai.KeepSepInmate2.InmateNumber,
                    KeepSepLabel = KeepSepLabel.INMATE,
                    PersonId = ksai.KeepSepInmate2.PersonId,
                    KeepSepType = ksai.KeepSeparateType,
                    KeepSepReason = ksai.KeepSepReason,
                    Deleteflag = ksai.DeleteFlag,
                    FacilityAbbr = ksai.KeepSepInmate2.Facility.FacilityAbbr,
                    HousingUnitId = ksai.KeepSepInmate2.HousingUnitId,
                    HousingUnitListId = ksai.KeepSepInmate2.HousingUnit.HousingUnitListId
                }));

            // KEEP SEPARATE ASSOCIATION To SUBSET
            keepSepAlertDetails.KeepSeparateAssocList.AddRange(_context.KeepSepAssocSubset.SelectMany(ksas =>
                    personClassification.Where(pc =>
                    pc.PersonClassificationTypeId == ksas.KeepSepAssoc1Id &&
                    pc.PersonClassificationSubsetId != null)
                    .GroupBy(g => g.PersonClassificationTypeId),
                (ksas, pc) => new KeepSeparateVm
                {
                    KeepSepAssocSubsetId = ksas.KeepSepAssocSubsetId,
                    KeepSeparateId = ksas.KeepSepAssocSubsetId,
                    KeepSepLabel = KeepSepLabel.SUBSET,
                    KeepSepAssoc2Subset = ksas.KeepSepAssoc2Subset,
                    KeepSepReason = ksas.KeepSepReason,
                    keepsep = ksas.KeepSepAssoc2,
                    Deleteflag = ksas.DeleteFlag,
                    Assoc = lookupslist.SingleOrDefault(s => s.LookupIndex == ksas.KeepSepAssoc1Id).LookupDescription,
                    AssocId = ksas.KeepSepAssoc1Id,
                    SubsetId = ksas.KeepSepAssoc2SubsetId
                }));

            // //Search Filter
            keepSepAlertDetails.KeepSeparateAssocList = keepSepAlertDetails.KeepSeparateAssocList.Where(
                    k => (keepSepSearch.AssociationId == 0 || k.AssocId == keepSepSearch.AssociationId)
                        && (keepSepSearch.SubsetId == 0 || k.SubsetId == keepSepSearch.SubsetId)
                ).Select(kss => new KeepSeparateVm
                {
                    KeepSepAssocInmateId = kss.KeepSepAssocInmateId,
                    KeepSepAssocAssocId = kss.KeepSepAssocAssocId,
                    KeepSepAssocSubsetId = kss.KeepSepAssocSubsetId,
                    KeepSeparateId = kss.KeepSeparateId,
                    KeepSepInmateId = kss.KeepSepInmateId,
                    KeepSepLabel = kss.KeepSepLabel,
                    InmateNumber = kss.InmateNumber,
                    PersonId = kss.PersonId,
                    keepsep = kss.keepsep,
                    KeepSepType = kss.KeepSepType,
                    KeepSepReason = kss.KeepSepReason,
                    Deleteflag = kss.Deleteflag,
                    FacilityAbbr = kss.FacilityAbbr,
                    KeepSepAssoc2Subset = kss.KeepSepAssoc2Subset,
                    Assoc = kss.Assoc,
                    HousingUnitId = kss.HousingUnitId

                }).ToList();

            List<int> personIds = keepSepAlertDetails.KeepSeparateAssocList.Select(ks => ks.PersonId).Distinct()
                .ToList();

            List<PersonInfo> personDetailLst = _context.Person.Where(pr => personIds.Contains(pr.PersonId))
                .Select(p => new PersonInfo
                {
                    PersonId = p.PersonId,
                    PersonFirstName = p.PersonFirstName,
                    PersonMiddleName = p.PersonMiddleName,
                    PersonLastName = p.PersonLastName
                }).ToList();


            keepSepAlertDetails.KeepSeparateAssocList.ForEach(item =>
            {
                if (item.HousingUnitId.HasValue)
                {
                    item.HousingDetail = _facilityHousingService.GetHousingDetails(item.HousingUnitId.Value);
                }

                if (item.PersonId == 0) return;
                PersonInfo person = personDetailLst.Find(p => p.PersonId == item.PersonId);
                item.PersonFirstName = person.PersonFirstName;
                item.PersonMiddleName = person.PersonMiddleName;
                item.PersonLastName = person.PersonLastName;
            });
            
            // KeepSeparate Association Count

             keepSepAlertDetails.KeepSeparateAssocCount = keepSepAlertDetails.KeepSeparateAssocList
                .GroupBy(g => new { g.Assoc }).Select(kss => new AssociationCount
                {
                    Association = kss.Key.Assoc,
                    Count = kss.Count(),
                    Subsetlist = GetLookupCategory(kss.Key.Assoc)                                
                }).ToList();

            //KeepSeparateSubsetList
            // KEEP SEPARATE SUBSET To SUBSET 1
            keepSepAlertDetails.KeepSeparateSubsetList = _context.KeepSepSubsetSubset.SelectMany(ksss =>
                personClassification.Where(pc =>
                      pc.PersonClassificationSubsetId == ksss.KeepSepAssoc1SubsetId)
                    .GroupBy(g => g.PersonClassificationSubsetId),
                  (ksss, pc) => new KeepSeparateVm
                  {
                      KeepSepSubsetSubsetId = ksss.KeepSepSubsetSubsetId,
                      KeepSeparateId = ksss.KeepSepSubsetSubsetId,
                      KeepSepLabel = KeepSepLabel.SUBSET,
                      Assoc = ksss.KeepSepAssoc1,
                      keepsep = ksss.KeepSepAssoc2,
                      KeepSepAssoc2Subset = ksss.KeepSepAssoc2Subset,
                      KeepSepReason = ksss.KeepSepReason,
                      Deleteflag = ksss.DeleteFlag,
                      Subset = lookupsSublist.SingleOrDefault(s => s.LookupIndex == ksss.KeepSepAssoc1SubsetId).LookupDescription,
                      SubsetId = ksss.KeepSepAssoc1SubsetId
                  }).ToList();

            // KEEP SEPARATE SUBSET To SUBSET 2
            keepSepAlertDetails.KeepSeparateSubsetList.AddRange(_context.KeepSepSubsetSubset.SelectMany(ksss =>
                    personClassification.Where(pc =>
                        pc.PersonClassificationSubsetId == ksss.KeepSepAssoc2SubsetId)
                    .GroupBy(g => g.PersonClassificationSubsetId),
                (ksss, pc) => new KeepSeparateVm
                {
                    KeepSepSubsetSubsetId = ksss.KeepSepSubsetSubsetId,
                    KeepSeparateId = ksss.KeepSepSubsetSubsetId,
                    KeepSepLabel = KeepSepLabel.SUBSET,
                    Assoc = ksss.KeepSepAssoc2,
                    keepsep = ksss.KeepSepAssoc1,
                    keepSepAssoc1Subset = ksss.KeepSepAssoc1Subset,
                    Subset = lookupsSublist.SingleOrDefault(s => s.LookupIndex == ksss.KeepSepAssoc2SubsetId).LookupDescription,
                    KeepSepReason = ksss.KeepSepReason,
                    Deleteflag = ksss.DeleteFlag,
                    SubsetId = ksss.KeepSepAssoc2SubsetId,
                }));

            // KEEP SEPARATE SUBSET To ASSOC

            List<KeepSeparateVm> lstAssoc = _context.KeepSepAssocSubset.SelectMany(ksas =>
             _context.KeepSepSubsetSubset.Where(ksss => ksss.KeepSepAssoc2SubsetId == ksas.KeepSepAssoc2SubsetId),
                (ksas, ksss) => new KeepSeparateVm
                {
                    KeepSepAssocSubsetId = ksas.KeepSepAssocSubsetId,
                    KeepSeparateId = ksas.KeepSepAssocSubsetId,
                    KeepSepLabel = KeepSepLabel.ASSOC,
                    Assoc = ksas.KeepSepAssoc2,
                    keepsep = ksas.KeepSepAssoc1,
                    KeepSepReason = ksas.KeepSepReason,
                    Subset = lookupsSublist.SingleOrDefault(s => s.LookupIndex == ksas.KeepSepAssoc2SubsetId).LookupDescription,
                    Deleteflag = ksas.DeleteFlag,
                    SubsetId = ksas.KeepSepAssoc2SubsetId
                }).ToList();


            keepSepAlertDetails.KeepSeparateSubsetList.AddRange(lstAssoc
                .SelectMany(ksas => personClassification.Where(pc =>
                        pc.PersonClassificationSubsetId == ksas.SubsetId)
                        .GroupBy(g => g.PersonClassificationSubsetId),
                    (ksas, pc) => new KeepSeparateVm
                    {
                        KeepSepAssocSubsetId = ksas.KeepSepAssocSubsetId,
                        KeepSeparateId = ksas.KeepSepAssocSubsetId,
                        KeepSepLabel = KeepSepLabel.ASSOC,
                        Assoc = ksas.Assoc,
                        keepsep = ksas.keepsep,
                        KeepSepReason = ksas.KeepSepReason,
                        Subset = lookupsSublist.Single(s => s.LookupIndex == ksas.SubsetId).LookupDescription,
                        Deleteflag = ksas.Deleteflag,
                        SubsetId = ksas.SubsetId
                    }
                    ));

            // KEEP SEPARATE SUBSET To INMATE

            List<KeepSeparateVm> lstInmate = _context.KeepSepSubsetSubset.SelectMany(ksss =>
                    _context.KeepSepSubsetInmate.Where
                    (kssi => kssi.KeepSepAssoc1Id == ksss.KeepSepAssoc1Id
                    && kssi.KeepSepInmate2.Facility.FacilityId == keepSepSearch.FacilityId
                    && kssi.KeepSepInmate2.InmateActive == 1
                    && (keepSepSearch.GenderId == 0 ||kssi.KeepSepInmate2.Person.PersonSexLast == keepSepSearch.GenderId)
                    && (keepSepSearch.RaceId == 0 || kssi.KeepSepInmate2.Person.PersonRaceLast == keepSepSearch.RaceId)
                    && (keepSepSearch.InmateId == 0 ||kssi.KeepSepInmate2.InmateId == keepSepSearch.InmateId)
                    && (keepSepSearch.HousingUnitListId == 0 ||
                     kssi.KeepSepInmate2.HousingUnit.HousingUnitListId == keepSepSearch.HousingUnitListId)
                    && (keepSepSearch.HousingGroupId == 0 
                    || housingUnitListIds.Contains(kssi.KeepSepInmate2.HousingUnit.HousingUnitListId))
                    && (keepSepSearch.IllegalAlienOnly == false 
                    || kssi.KeepSepInmate2.Person.IllegalAlienFlag == keepSepSearch.IllegalAlienOnly)
                    && !kssi.KeepSepInmate2.InmateClassification.InmateDateUnassigned.HasValue
                    && (string.IsNullOrEmpty(keepSepSearch.Classify)
                    || kssi.KeepSepInmate2.InmateClassification.InmateClassificationReason == keepSepSearch.Classify)
                    ),
                (ksss, kssi) => new KeepSeparateVm
                {
                    KeepSepSubsetInmateId = kssi.KeepSepSubsetInmateId,
                    KeepSeparateId = kssi.KeepSepSubsetInmateId,
                    KeepSepInmateId = kssi.KeepSepInmate2Id,
                    KeepSepLabel = KeepSepLabel.INMATE,
                    HousingUnitId = kssi.KeepSepInmate2.HousingUnitId,
                    InmateNumber = kssi.KeepSepInmate2.InmateNumber,
                    PersonId = kssi.KeepSepInmate2.PersonId,
                    Assoc = lookupslist.SingleOrDefault(s => s.LookupIndex == kssi.KeepSepAssoc1Id).LookupDescription,
                    KeepSepType = kssi.KeepSeparateType,
                    Subset = lookupsSublist.SingleOrDefault(s => s.LookupIndex == kssi.KeepSepAssoc1SubsetId).LookupDescription,
                    KeepSepReason = kssi.KeepSepReason,
                    Deleteflag = kssi.DeleteFlag,
                    FacilityAbbr = kssi.KeepSepInmate2.Facility.FacilityAbbr,
                    HousingUnitListId = kssi.KeepSepInmate2.HousingUnit.HousingUnitListId,
                    AssocId = kssi.KeepSepAssoc1Id,
                    SubsetId = kssi.KeepSepAssoc1SubsetId

                }).ToList();
            lstInmate = lstInmate.GroupBy(g => new
            {
                g.KeepSepSubsetInmateId,
                g.KeepSepAssocSubsetId,
                g.KeepSepSubsetSubsetId,
                g.KeepSepInmateId,
                g.KeepSepLabel,
                g.HousingUnitId,
                g.InmateNumber,
                g.PersonId,
                g.Assoc,
                g.KeepSepType,
                g.Subset,
                g.KeepSepReason,
                g.Deleteflag,
                g.HousingUnitListId,
                g.AssocId,
                g.SubsetId,
                g.KeepSeparateId
            }).Select(s => new KeepSeparateVm
            {
                KeepSepSubsetInmateId = s.Key.KeepSepSubsetInmateId,
                KeepSepAssocSubsetId = s.Key.KeepSepAssocSubsetId,
                KeepSepSubsetSubsetId = s.Key.KeepSepSubsetSubsetId,
                KeepSepInmateId = s.Key.KeepSepInmateId,
                KeepSeparateId = s.Key.KeepSeparateId,
                KeepSepLabel = s.Key.KeepSepLabel,
                PersonId = s.Key.PersonId,
                InmateNumber = s.Key.InmateNumber,
                KeepSepType = s.Key.KeepSepType,
                KeepSepReason = s.Key.KeepSepReason,
                HousingUnitId = s.Key.HousingUnitId,
                Deleteflag = s.Key.Deleteflag,
                AssocId = s.Key.AssocId,
                HousingUnitListId = s.Key.HousingUnitListId,
                Subset = s.Key.Subset,
                SubsetId = s.Key.SubsetId,
                Assoc = s.Key.Assoc
            }).ToList();
            keepSepAlertDetails.KeepSeparateSubsetList.AddRange(lstInmate);

             //Search Filter
            keepSepAlertDetails.KeepSeparateSubsetList = keepSepAlertDetails.KeepSeparateSubsetList.Where(
                k => (keepSepSearch.AssociationId == 0 || k.AssocId == keepSepSearch.AssociationId)
                     && (keepSepSearch.SubsetId == 0 || k.SubsetId == keepSepSearch.SubsetId)
            ).Select(kss => new KeepSeparateVm
            {
                Assoc = kss.Assoc,
                Subset = kss.Subset,
                KeepSeparateId = kss.KeepSeparateId,
                KeepSepSubsetInmateId = kss.KeepSepSubsetInmateId,
                KeepSepAssocSubsetId =kss.KeepSepAssocSubsetId,
                KeepSepSubsetSubsetId = kss.KeepSepSubsetSubsetId,
                KeepSepInmateId = kss.KeepSepInmateId,
                KeepSepLabel = kss.KeepSepLabel,
                HousingUnitId = kss.HousingUnitId,
                InmateNumber = kss.InmateNumber,
                PersonId = kss.PersonId,
                keepsep = kss.keepsep,
                KeepSepType = kss.KeepSepType,
                KeepSepReason = kss.KeepSepReason,
                Deleteflag = kss.Deleteflag,
                FacilityAbbr = kss.FacilityAbbr,
                keepSepAssoc1Subset = kss.keepSepAssoc1Subset,
                KeepSepAssoc2Subset = kss.KeepSepAssoc2Subset,
                HousingUnitListId = kss.HousingUnitListId
            }).ToList();

            personIds = keepSepAlertDetails.KeepSeparateSubsetList.Select(ks => ks.PersonId).Distinct()
                .ToList();
            personDetailLst = _context.Person.Where(pr => personIds.Contains(pr.PersonId))
                .Select(p => new PersonInfo
                {
                    PersonId = p.PersonId,
                    PersonFirstName = p.PersonFirstName,
                    PersonMiddleName = p.PersonMiddleName,
                    PersonLastName = p.PersonLastName
                }).ToList();

            keepSepAlertDetails.KeepSeparateSubsetList.ForEach(item =>
            {
                if (item.HousingUnitId.HasValue)
                {
                    item.HousingDetail = _facilityHousingService.GetHousingDetails(item.HousingUnitId.Value);
                }
                if (item.PersonId > 0)
                {
                    PersonInfo person = personDetailLst.Single(p => p.PersonId == item.PersonId);
                    item.PersonFirstName = person.PersonFirstName;
                    item.PersonMiddleName = person.PersonMiddleName;
                    item.PersonLastName = person.PersonLastName;
                }

            });

            //KeepSeparate Subset Count

            keepSepAlertDetails.KeepSeparateSubsetCount = keepSepAlertDetails.KeepSeparateSubsetList
                .GroupBy(g => new { g.Assoc, g.Subset }).Select(kss => new SubsetCount
                {
                    Association = kss.Key.Assoc,
                    Subset = kss.Key.Subset,
                    Count = kss.Count()
                }).ToList();
            return keepSepAlertDetails;
        }

        private List<KeyValuePair<int,string>> GetLookupCategory(string LookupCategory) 
        {

            List<KeyValuePair<int,string>> subsetValues = _context.Lookup.Where(s=>s.LookupCategory ==LookupCategory)
            .GroupBy(g=>new {g.LookupDescription,g.LookupIndex})
            .Select(a => new KeyValuePair<int, string>(a.Count(), a.Key.LookupDescription)).ToList();
            return subsetValues;
         }


        public async Task<bool> DeleteUndoKeepSeparate(KeepSeparateVm keepSepDetails)
        {
            bool isUpdate = keepSepDetails.Type switch
            {
                KeepSepType.Inmate => (keepSepDetails.KeepSepLabel switch
                {
                    KeepSepLabel.INMATE => await DeleteUndoKeepSepInmateDetailsAsync(keepSepDetails),
                    KeepSepLabel.ASSOC => await DeleteUndoKeepSepAssocDetailsAsync(keepSepDetails),
                    KeepSepLabel.SUBSET => await DeleteUndoKeepSepSubsetDetailsAsync(keepSepDetails),
                    _ => false
                }),
                KeepSepType.Association => (keepSepDetails.KeepSepLabel switch
                {
                    KeepSepLabel.INMATE => await DeleteUndoKeepSepAssocDetailsAsync(keepSepDetails),
                    KeepSepLabel.ASSOC => await DeleteUndoKeepSepAssocAssocDetails(keepSepDetails),
                    KeepSepLabel.SUBSET => await DeleteUndoKeepSepAssocSubsetDetails(keepSepDetails),
                    _ => false
                }),
                KeepSepType.Subset => (keepSepDetails.KeepSepLabel switch
                {
                    KeepSepLabel.INMATE => await DeleteUndoKeepSepSubsetDetailsAsync(keepSepDetails),
                    KeepSepLabel.ASSOC => await DeleteUndoKeepSepAssocSubsetDetails(keepSepDetails),
                    KeepSepLabel.SUBSET => await DeleteUndoKeepSepSubsetsubsetDetails(keepSepDetails),
                    _ => false
                }),
                _ => false
            };

            return isUpdate;
        }

        private async Task<bool> DeleteUndoKeepSepInmateDetailsAsync(KeepSeparateVm keepSepDetails)
        {
            KeepSeparate keepSeparate =
                _context.KeepSeparate.SingleOrDefault(ks => ks.KeepSeparateId == keepSepDetails.KeepSeparateId);
            if (keepSeparate == null) return false;
            keepSeparate.UpdatedByDate = DateTime.Now;
            keepSeparate.UpdatedByOfficerId = _personnelId;
            keepSeparate.InactiveFlag = keepSepDetails.IsDeleted ? 1 : 0;
            _context.SaveChanges();

            await InsertKeepSepInmateHistory(keepSepDetails);
            return true;
        }

        private async Task<bool> DeleteUndoKeepSepAssocDetailsAsync(KeepSeparateVm keepSepDetails)
        {
            KeepSepAssocInmate keepSepAssocInmate =
                _context.KeepSepAssocInmate.SingleOrDefault(ksa =>
                    ksa.KeepSepAssocInmateId == keepSepDetails.KeepSepAssocInmateId);
            if (keepSepAssocInmate == null) return false;
            DateTime? currentDate = DateTime.Now;
            keepSepAssocInmate.DeleteFlag = keepSepDetails.IsDeleted ? 1 : 0;
            keepSepAssocInmate.DeleteBy = keepSepDetails.IsDeleted ? _personnelId : new int?();
            keepSepAssocInmate.DeleteDate = keepSepDetails.IsDeleted ? currentDate : null;
            _context.SaveChanges();
            await InsertKeepSepAssocHistory(keepSepDetails);
            return true;
        }

        private async Task<bool> DeleteUndoKeepSepSubsetDetailsAsync(KeepSeparateVm keepSepDetails)
        {
            KeepSepSubsetInmate keepSepSubsetInmate =
                _context.KeepSepSubsetInmate.SingleOrDefault(kssi =>
                    kssi.KeepSepSubsetInmateId == keepSepDetails.KeepSepSubsetInmateId);

            if (keepSepSubsetInmate == null) return false;
            DateTime? currentDate = DateTime.Now;
            keepSepSubsetInmate.DeleteFlag = keepSepDetails.IsDeleted ? 1 : 0;
            keepSepSubsetInmate.DeleteBy = keepSepDetails.IsDeleted ? _personnelId : new int?();
            keepSepSubsetInmate.DeleteDate = keepSepDetails.IsDeleted ? currentDate : null;
            _context.SaveChanges();

            await InsertKeepSepSubsetHistory(keepSepDetails);
            return true;
        }


        private async Task<bool> DeleteUndoKeepSepAssocAssocDetails(KeepSeparateVm keepSepDetails)
        {
            KeepSepAssocAssoc keepSepAssocAssoc =
                _context.KeepSepAssocAssoc.SingleOrDefault(ksa =>
                    ksa.KeepSepAssocAssocId == keepSepDetails.KeepSepAssocAssocId);
            if (keepSepAssocAssoc == null) return false;
            DateTime? currentDate = DateTime.Now;
            keepSepAssocAssoc.DeleteFlag = keepSepDetails.IsDeleted ? 1 : 0;
            keepSepAssocAssoc.DeleteBy = keepSepDetails.IsDeleted ? _personnelId : new int?();
            keepSepAssocAssoc.DeleteDate = keepSepDetails.IsDeleted ? currentDate : null;
            _context.SaveChanges();

             await InsertKeepSepAssocAssocHistory(keepSepDetails);
            return true;
        }

        private async Task<bool> DeleteUndoKeepSepAssocSubsetDetails(KeepSeparateVm keepSepDetails)
        {
            KeepSepAssocSubset keepSepAssocSubset =
                _context.KeepSepAssocSubset.SingleOrDefault(kssi =>
                    kssi.KeepSepAssocSubsetId == keepSepDetails.KeepSepAssocSubsetId);

            if (keepSepAssocSubset == null) return false;
            DateTime? currentDate = DateTime.Now;
            keepSepAssocSubset.DeleteFlag = keepSepDetails.IsDeleted ? 1 : 0;
            keepSepAssocSubset.DeleteBy = keepSepDetails.IsDeleted ? _personnelId : new int?();
            keepSepAssocSubset.DeleteDate = keepSepDetails.IsDeleted ? currentDate : null;
            _context.SaveChanges();

             await InsertKeepSepSAssocSubsetHistory(keepSepDetails);
            return true;
        }

        private async Task<bool> DeleteUndoKeepSepSubsetsubsetDetails(KeepSeparateVm keepSepDetails)
        {
            KeepSepSubsetSubset keepSepSubsetSubset =
                _context.KeepSepSubsetSubset.SingleOrDefault(ksa =>
                    ksa.KeepSepSubsetSubsetId == keepSepDetails.KeepSepSubsetSubsetId);
            if (keepSepSubsetSubset == null) return false;
            DateTime? currentDate = DateTime.Now;
            keepSepSubsetSubset.DeleteFlag = keepSepDetails.IsDeleted ? 1 : 0;
            keepSepSubsetSubset.DeleteBy = keepSepDetails.IsDeleted ? _personnelId : new int?();
            keepSepSubsetSubset.DeleteDate = keepSepDetails.IsDeleted ? currentDate : null;
            _context.SaveChanges();
            await InsertKeepSepSubsetSubsetHistory(keepSepDetails);
            return true;
        }


        private async Task InsertKeepSepInmateHistory(KeepSeparateVm keepSepDetails)
        {
            KeepSeparateHistory keepSeparateHistory = new KeepSeparateHistory
            {
                KeepSeparateId = keepSepDetails.KeepSeparateId,
                KeepSeparateHistoryList = keepSepDetails.KeepSepHistoryList,
                PersonnelId = _personnelId,
                CreateDate = DateTime.Now
            };
            _context.KeepSeparateHistory.Add(keepSeparateHistory);
            await _context.SaveChangesAsync();
        }

        private async Task InsertKeepSepAssocHistory(KeepSeparateVm keepSepDetails)
        {
            KeepSepAssocInmateHistory keepSepAssocInmateHistory = new KeepSepAssocInmateHistory
            {
                KeepSepAssocInmateId = keepSepDetails.KeepSepAssocInmateId,
                KeepSepAssocInmateHistoryList = keepSepDetails.KeepSepHistoryList,
                PersonnelId = _personnelId,
                CreateDate = DateTime.Now
            };
            _context.KeepSepAssocInmateHistory.Add(keepSepAssocInmateHistory);
            await _context.SaveChangesAsync();
        }

        private async Task InsertKeepSepSubsetHistory(KeepSeparateVm keepSepDetails)

               {
            KeepSepSubsetInmateHistory keepSepSubsetInmateHistory = new KeepSepSubsetInmateHistory
            {
                KeepSepSubsetInmateId = keepSepDetails.KeepSepAssocInmateId,
                KeepSepSubsetInmateHistoryList = keepSepDetails.KeepSepHistoryList,
                PersonnelId = _personnelId,
                CreateDate = DateTime.Now
            };
            _context.KeepSepSubsetInmateHistory.Add(keepSepSubsetInmateHistory);
            await _context.SaveChangesAsync();
               }

         private async Task InsertKeepSepAssocAssocHistory(KeepSeparateVm keepSepDetails)
        {
            KeepSepAssocAssocHistory keepSepAssocAssocHistory = new KeepSepAssocAssocHistory
            {
                KeepSepAssocAssocId = keepSepDetails.KeepSepAssocAssocId,
                KeepSepAssocAssocHistoryList = keepSepDetails.KeepSepHistoryList,
                PersonnelId = _personnelId,
                CreateDate = DateTime.Now
            };
            _context.KeepSepAssocAssocHistory.Add(keepSepAssocAssocHistory);
            await _context.SaveChangesAsync();
        }

        private async Task InsertKeepSepSubsetSubsetHistory(KeepSeparateVm keepSepDetails)
        {
            KeepSepSubsetSubsetHistory keepSepSubsetSubsetHistory = new KeepSepSubsetSubsetHistory
            {
                KeepSepSubsetSubsetId = keepSepDetails.KeepSepSubsetSubsetId,
                KeepSepSubsetSubsetHistoryList = keepSepDetails.KeepSepHistoryList,
                PersonnelId = _personnelId,
                CreateDate = DateTime.Now
            };
            _context.KeepSepSubsetSubsetHistory.Add(keepSepSubsetSubsetHistory);
            await _context.SaveChangesAsync();
        }

        private async Task InsertKeepSepSAssocSubsetHistory(KeepSeparateVm keepSepDetails)
        {
            KeepSepAssocSubsetHistory keepSepAssocSubsetHistory = new KeepSepAssocSubsetHistory
            {
                KeepSepAssocSubsetId = keepSepDetails.KeepSepAssocSubsetId,
                KeepSepAssocSubsetHistoryList = keepSepDetails.KeepSepHistoryList,
                PersonnelId = _personnelId,
                CreateDate = DateTime.Now
            };
            _context.KeepSepAssocSubsetHistory.Add(keepSepAssocSubsetHistory);
            await _context.SaveChangesAsync();
        }

    }

}


