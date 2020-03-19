using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GenerateTables.Models;
using ServerAPI.ViewModels;
using ServerAPI.Utilities;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace ServerAPI.Services
{
    public class KeepSepAlertService : IKeepSepAlertService
    {

        private readonly AAtims _context;
        private readonly int _personnelId;
        private readonly IFacilityHousingService _facilityHousingService;
        private readonly IInterfaceEngineService _interfaceEngine;

        public KeepSepAlertService(AAtims context, IHttpContextAccessor httpContextAccessor,
            IFacilityHousingService facilityHousingService, IInterfaceEngineService interfaceEngine)
        {
            _context = context;
            _personnelId =
                Convert.ToInt32(httpContextAccessor.HttpContext.User
                .FindFirst(AuthDetailConstants.PERSONNELID)?.Value);
            _facilityHousingService = facilityHousingService;
            _interfaceEngine = interfaceEngine;
        }

        #region Get KeepSeparate Details 

        //Get KeepSeparate Alert Details based on Inmate
        public KeepSeparateAlertVm GetKeepSeparateAlertDetails(int inmateId)
        {
            KeepSeparateAlertVm keepSepAlertDetails = new KeepSeparateAlertVm
            {
                KeepSeparateInmateList = new List<KeepSeparateVm>(),
                KeepSeparateAssocList = new List<KeepSeparateVm>()
            };

            keepSepAlertDetails.KeepSeparateInmateList = (from ks in _context.KeepSeparate
                                                          where ks.KeepSeparateInmate1Id == inmateId
                                                          select new KeepSeparateVm
                                                          {
                                                              KeepSeparateId = ks.KeepSeparateId,
                                                              KeepSepInmateId = ks.KeepSeparateInmate1Id,
                                                              KeepSepInmate2Id = ks.KeepSeparateInmate2Id,
                                                              KeepSepLabel = KeepSepLabel.INMATE,
                                                              PersonId = ks.KeepSeparateInmate2.PersonId,
                                                              InmateNumber = ks.KeepSeparateInmate2.InmateNumber,
                                                              KeepSepType = ks.KeepSeparateType,
                                                              KeepSepReason = ks.KeepSeparateReason,
                                                              HousingUnitId = ks.KeepSeparateInmate2.HousingUnitId,
                                                              IsDeleted = ks.InactiveFlag == 1,
                                                              InmateActive = ks.KeepSeparateInmate2.InmateActive == 1,
                                                              KeepSepTypeName = KeepSepTypeName.II, // "I1-I2"
                                                              FacilityAbbr = ks.KeepSeparateInmate2.Facility.FacilityAbbr,
                                                              KeepSeparateNote = ks.KeepSeparateNote
                                                          }).ToList();

            //Keep Sep Inmate 2 List
            keepSepAlertDetails.KeepSeparateInmateList.AddRange(from ks in _context.KeepSeparate
                                                                where ks.KeepSeparateInmate2Id == inmateId
                                                                select new KeepSeparateVm
                                                                {
                                                                    KeepSeparateId = ks.KeepSeparateId,
                                                                    KeepSepInmateId = ks.KeepSeparateInmate2Id,
                                                                    KeepSepInmate2Id = ks.KeepSeparateInmate1Id,
                                                                    KeepSepLabel = KeepSepLabel.INMATE,
                                                                    PersonId = ks.KeepSeparateInmate1.PersonId,
                                                                    InmateNumber = ks.KeepSeparateInmate1.InmateNumber,
                                                                    KeepSepType = ks.KeepSeparateType,
                                                                    KeepSepReason = ks.KeepSeparateReason,
                                                                    HousingUnitId = ks.KeepSeparateInmate1.HousingUnitId,
                                                                    IsDeleted = ks.InactiveFlag == 1,
                                                                    InmateActive = ks.KeepSeparateInmate1.InmateActive == 1,
                                                                    KeepSepTypeName = KeepSepTypeName.II, // "I2-I1"
                                                                    FacilityAbbr = ks.KeepSeparateInmate1.Facility.FacilityAbbr,
                                                                    KeepSeparateNote = ks.KeepSeparateNote
                                                                });
            List<int> personIds = keepSepAlertDetails.KeepSeparateInmateList.Select(ks => ks.PersonId).Distinct()
                .ToList();
            List<PersonInfo> personDetailLst = _context.Person.Where(pr => personIds.Contains(pr.PersonId))
                .Select(p => new PersonInfo
                {
                    PersonId = p.PersonId,
                    PersonLastName = p.PersonLastName,
                    PersonFirstName = p.PersonFirstName,
                    PersonMiddleName = p.PersonMiddleName
                }).ToList();

            keepSepAlertDetails.KeepSeparateInmateList.ForEach(item =>
            {
                if (item.HousingUnitId.HasValue)
                {
                    item.HousingDetail = _facilityHousingService.GetHousingDetails(item.HousingUnitId.Value);
                    item.HousingUnitListId = item.HousingDetail.HousingUnitListId;
                }

                if (item.PersonId > 0)
                {
                    PersonInfo person = personDetailLst.Single(p => p.PersonId == item.PersonId);
                    item.PersonLastName = person.PersonLastName;
                    item.PersonFirstName = person.PersonFirstName;
                    item.PersonMiddleName = person.PersonMiddleName;
                }
            });
            List<Lookup> lookupslist = _context.Lookup.Where(w => w.LookupType == LookupConstants.CLASSGROUP).ToList();
            List<Lookup> lookupsSublist = _context.Lookup.Where(w => w.LookupType == LookupConstants.CLASSGROUPSUB).ToList();
            //Keep Sep Assoc List: INMATE KEEP SEPARATE To ASSOCIATION
            keepSepAlertDetails.KeepSeparateInmateList.AddRange(from ksa in _context.KeepSepAssocInmate
                where ksa.KeepSepInmate2Id == inmateId
                select new KeepSeparateVm
                {
                    KeepSepAssocInmateId = ksa.KeepSepAssocInmateId,
                    KeepSeparateId = ksa.KeepSepAssocInmateId,
                    KeepSepLabel = KeepSepLabel.ASSOC,
                    KeepSepAssoc = lookupslist.SingleOrDefault(s => s.LookupIndex == ksa.KeepSepAssoc1Id)
                        .LookupDescription,
                    KeepSepAssoc1Id = ksa.KeepSepAssoc1Id,
                    KeepSepType = ksa.KeepSeparateType,
                    KeepSepReason = ksa.KeepSepReason,
                    KeepSepInmateId = ksa.KeepSepInmate2Id,
                    PersonId = ksa.KeepSepInmate2.PersonId,
                    InmateNumber = ksa.KeepSepInmate2.InmateNumber,
                    HousingUnitId = ksa.KeepSepInmate2.HousingUnitId,
                    IsDeleted = ksa.DeleteFlag == 1,
                    InmateActive = ksa.KeepSepInmate2.InmateActive == 1,
                    KeepSepTypeName = KeepSepTypeName.IA, // "I-A"
                    FacilityAbbr = ksa.KeepSepInmate2.Facility.FacilityAbbr,
                    KeepSeparateNote = ksa.KeepSeparateNote
                });

            //Keep Sep Subset List: INMATE KEEP SEPARATE To SUBSET
            keepSepAlertDetails.KeepSeparateInmateList.AddRange(from kss in _context.KeepSepSubsetInmate
                where kss.KeepSepInmate2Id == inmateId
                select new KeepSeparateVm
                {
                    KeepSepSubsetInmateId = kss.KeepSepSubsetInmateId,
                    KeepSeparateId = kss.KeepSepSubsetInmateId,
                    KeepSepInmateId = kss.KeepSepInmate2Id,
                    KeepSepLabel = KeepSepLabel.SUBSET,
                    KeepSepAssoc = lookupslist.SingleOrDefault(s => s.LookupIndex == kss.KeepSepAssoc1Id)
                        .LookupDescription,
                    KeepSepAssocSubset = lookupsSublist.SingleOrDefault(s => s.LookupIndex == kss.KeepSepAssoc1SubsetId)
                        .LookupDescription,
                    KeepSepAssoc1Id = kss.KeepSepAssoc1Id,
                    KeepSepAssoc1SubsetId = kss.KeepSepAssoc1SubsetId,
                    KeepSepType = kss.KeepSeparateType,
                    KeepSepReason = kss.KeepSepReason,
                    PersonId = kss.KeepSepInmate2.PersonId,
                    InmateNumber = kss.KeepSepInmate2.InmateNumber,
                    HousingUnitId = kss.KeepSepInmate2.HousingUnitId,
                    IsDeleted = kss.DeleteFlag == 1,
                    InmateActive = kss.KeepSepInmate2.InmateActive == 1,
                    KeepSepTypeName = KeepSepTypeName.IS, // "I-S"
                    FacilityAbbr = kss.KeepSepInmate2.Facility.FacilityAbbr,
                    KeepSeparateNote = kss.KeepSeparateNote
                });
            int personId =
                _context.Inmate.SingleOrDefault(i => i.InmateId == inmateId && i.InmateActive == 1)?.PersonId ?? 0;
            //Keep Separates based on Inmate Association Status
            var lstKeepSepActiveAssocSubset = _context.PersonClassification
                .Where(pc => pc.PersonId == personId && pc.InactiveFlag == 0 &&
                             pc.PersonClassificationDateFrom < DateTime.Now
                             && (!pc.PersonClassificationDateThru.HasValue
                                 || pc.PersonClassificationDateThru >= DateTime.Now)).Select(pc =>
                    new
                    {
                        Assoc =  lookupslist.SingleOrDefault(s => s.LookupIndex == pc.PersonClassificationTypeId).LookupDescription,
                        Subset = lookupsSublist.SingleOrDefault(s => s.LookupIndex == pc.PersonClassificationSubsetId).LookupDescription,
                        AssocId = pc.PersonClassificationTypeId,
                        SubsetId = pc.PersonClassificationSubsetId,
                    }).ToList();

            //Assoc Keep Sep List : ASSOCIATION KEEP SEPARATE To ASSOCIATION
            keepSepAlertDetails.KeepSeparateAssocList.AddRange(_context.KeepSepAssocInmate.SelectMany(ksai =>
                    lstKeepSepActiveAssocSubset.Where(ksaas =>
                        ksai.DeleteFlag == 0 && ksaas.AssocId == ksai.KeepSepAssoc1Id),
                (ksai, ksaas) => new KeepSeparateVm
                {
                    KeepSepLabel = KeepSepLabel.ASSOCKEEPSEP,
                    PersonId = ksai.KeepSepInmate2.PersonId,
                    KeepSepInmateId = ksai.KeepSepInmate2Id,  
                    Assoc = lookupslist.SingleOrDefault(s =>s.LookupIndex == ksaas.AssocId).LookupDescription,
                    KeepSepReason = ksai.KeepSepReason
                }).GroupBy(n => new
                {
                    n.KeepSepInmateId,
                    n.Assoc,
                    n.KeepSepReason
                }).Select(s => s.FirstOrDefault()));

            // Subset Keep Sep List : ASSOCIATION KEEP SEPARATE To SUBSET
            keepSepAlertDetails.KeepSeparateAssocList.AddRange(_context.KeepSepSubsetInmate.SelectMany(kssi =>
                lstKeepSepActiveAssocSubset.Where(ksaas =>
                    ksaas.AssocId == kssi.KeepSepAssoc1Id && ksaas.SubsetId == kssi.KeepSepAssoc1SubsetId
                    && kssi.DeleteFlag == 0), (kssi, ksaas) => new KeepSeparateVm
            {
                KeepSepLabel = KeepSepLabel.SUBSETKEEPSEP,
                PersonId = kssi.KeepSepInmate2.PersonId,
                KeepSepInmateId = kssi.KeepSepInmate2Id,
                Assoc = lookupslist.SingleOrDefault(s => s.LookupIndex == ksaas.AssocId).LookupDescription,
                Subset = lookupsSublist.SingleOrDefault(s => s.LookupIndex == ksaas.SubsetId).LookupDescription,
                KeepSepReason = kssi.KeepSepReason
            }).GroupBy(n => new
            {
                n.KeepSepInmateId,
                n.Assoc,
                n.Subset,
                n.KeepSepReason
            }).Select(s => s.FirstOrDefault()));

            personIds = keepSepAlertDetails.KeepSeparateAssocList.Select(ks => ks.PersonId).Distinct().ToList();
            personDetailLst = _context.Person.Where(pr => personIds.Contains(pr.PersonId))
                .Select(p => new PersonInfo
                {
                    PersonId = p.PersonId,
                    PersonLastName = p.PersonLastName,
                    PersonFirstName = p.PersonFirstName,
                    PersonMiddleName = p.PersonMiddleName
                }).ToList();
            keepSepAlertDetails.KeepSeparateAssocList.ForEach(item =>
            {
                if (item.PersonId > 0)
                {
                    PersonInfo person = personDetailLst.Single(p => p.PersonId == item.PersonId);
                    item.PersonLastName = person.PersonLastName;
                    item.PersonFirstName = person.PersonFirstName;
                    item.PersonMiddleName = person.PersonMiddleName;
                }
            });

            //Keep Separates based on Inmate Association Status for ASSOCIATION
            keepSepAlertDetails.KeepSeparateAssocList.AddRange(_context.KeepSepAssocAssoc.SelectMany(ksaa =>
                    lstKeepSepActiveAssocSubset.Where(ksaas =>
                        ksaa.DeleteFlag == 0 && ksaas.AssocId == ksaa.KeepSepAssoc1Id),
                (ksaa, ksaas) => new KeepSeparateVm
                {
                    KeepSepLabel = KeepSepLabel.ASSOC,
                    KeepSepInmateId = ksaa.KeepSepAssocAssocId,                 
                    Assoc = lookupslist.SingleOrDefault(s => s.LookupIndex == ksaas.AssocId).LookupDescription,                  
                    KeepSepAssoc = lookupslist.SingleOrDefault(s => s.LookupIndex == ksaa.KeepSepAssoc2Id).LookupDescription,
                    KeepSepReason = ksaa.KeepSepReason
                }).ToList());

            keepSepAlertDetails.KeepSeparateAssocList.AddRange(_context.KeepSepAssocAssoc.SelectMany(ksaa =>
                    lstKeepSepActiveAssocSubset.Where(ksaas =>
                        ksaa.DeleteFlag == 0 && ksaas.AssocId == ksaa.KeepSepAssoc2Id),
                (ksaa, ksaas) => new KeepSeparateVm
                {
                    KeepSepLabel = KeepSepLabel.ASSOC,
                    KeepSepInmateId = ksaa.KeepSepAssocAssocId,            
                    Assoc = lookupslist.SingleOrDefault(s => s.LookupIndex == ksaas.AssocId).LookupDescription,       
                    KeepSepAssoc = lookupslist.SingleOrDefault(s => s.LookupIndex == ksaa.KeepSepAssoc1Id).LookupDescription,
                    KeepSepReason = ksaa.KeepSepReason
                }).ToList());

            keepSepAlertDetails.KeepSeparateAssocList.AddRange(_context.KeepSepAssocSubset.SelectMany(ksas =>
                    lstKeepSepActiveAssocSubset.Where(ksaas =>
                        ksas.DeleteFlag == 0 && ksaas.AssocId == ksas.KeepSepAssoc2Id
                                             && ksas.KeepSepAssoc2SubsetId == ksaas.SubsetId),
                (ksas, ksaas) => new KeepSeparateVm
                {
                    KeepSepLabel = KeepSepLabel.ASSOC,
                    KeepSepInmateId = ksas.KeepSepAssocSubsetId,                  
                    Assoc = lookupslist.SingleOrDefault(k => k.LookupIndex == ksaas.AssocId).LookupDescription,
                    Subset = lookupsSublist.SingleOrDefault(k => k.LookupIndex == ksaas.SubsetId).LookupDescription,                 
                    KeepSepAssoc = lookupslist.SingleOrDefault(k => k.LookupIndex == ksas.KeepSepAssoc1Id).LookupDescription,
                    KeepSepReason = ksas.KeepSepReason
                }).ToList());

            //Keep Separates based on Inmate Association Status for ASSOCIATION SUBSET
            keepSepAlertDetails.KeepSeparateAssocList.AddRange(_context.KeepSepAssocSubset.SelectMany(ksas =>
                    lstKeepSepActiveAssocSubset.Where(ksaas =>
                        ksas.DeleteFlag == 0 && ksas.KeepSepAssoc1Id == ksaas.AssocId),
                (ksas, ksaas) => new KeepSeparateVm
                {
                    KeepSepLabel = KeepSepLabel.SUBSET,
                    KeepSepInmateId = ksas.KeepSepAssocSubsetId,     
                    Assoc = lookupslist.SingleOrDefault(k => k.LookupIndex == ksaas.AssocId).LookupDescription,
                    KeepSepAssoc = lookupslist.SingleOrDefault(k => k.LookupIndex == ksas.KeepSepAssoc2Id).LookupDescription,
                    KeepSepAssocSubset = lookupsSublist.SingleOrDefault(k => k.LookupIndex == ksas.KeepSepAssoc2SubsetId).LookupDescription,
                    KeepSepReason = ksas.KeepSepReason
                }));

            keepSepAlertDetails.KeepSeparateAssocList.AddRange(_context.KeepSepSubsetSubset.SelectMany(kss =>
                    lstKeepSepActiveAssocSubset.Where(ksaas =>
                        kss.DeleteFlag == 0 && kss.KeepSepAssoc2Id == ksaas.AssocId
                                            && kss.KeepSepAssoc2SubsetId == ksaas.SubsetId),
                (kss, ksaas) => new KeepSeparateVm
                {
                    KeepSepLabel = KeepSepLabel.SUBSET,
                    Assoc = lookupslist.SingleOrDefault(k => k.LookupIndex == ksaas.AssocId).LookupDescription,
                    Subset = lookupsSublist.SingleOrDefault(k => k.LookupIndex == ksaas.SubsetId).LookupDescription,
                    KeepSepAssoc = lookupslist.SingleOrDefault(k => k.LookupIndex == kss.KeepSepAssoc1Id).LookupDescription,
                    KeepSepAssocSubset = lookupsSublist.SingleOrDefault(k => k.LookupIndex == kss.KeepSepAssoc1SubsetId).LookupDescription,
                    KeepSepReason = kss.KeepSepReason
                }));


            keepSepAlertDetails.KeepSeparateAssocList.AddRange(_context.KeepSepSubsetSubset.SelectMany(kss =>
                    lstKeepSepActiveAssocSubset.Where(ksaas =>
                        kss.DeleteFlag == 0 && kss.KeepSepAssoc1Id == ksaas.AssocId
                                            && kss.KeepSepAssoc1SubsetId != ksaas.SubsetId),
                (kss, ksaas) => new KeepSeparateVm
                {
                    KeepSepLabel = KeepSepLabel.SUBSET,
                    KeepSepInmateId = kss.KeepSepSubsetSubsetId,           
                    Assoc = lookupslist.SingleOrDefault(k => k.LookupIndex == ksaas.AssocId).LookupDescription,
                    Subset = lookupsSublist.SingleOrDefault(k => k.LookupIndex == ksaas.SubsetId).LookupDescription,
                    KeepSepAssoc = lookupslist.SingleOrDefault(k => k.LookupIndex == kss.KeepSepAssoc1Id).LookupDescription,
                    KeepSepAssocSubset = lookupsSublist.SingleOrDefault(k => k.LookupIndex == kss.KeepSepAssoc1SubsetId).LookupDescription,               
                    KeepSepReason = kss.KeepSepReason
                }));

            keepSepAlertDetails.KeepSeparateAssocList.AddRange(_context.KeepSepSubsetSubset.SelectMany(kss =>
                    lstKeepSepActiveAssocSubset.Where(ksaas =>
                        kss.DeleteFlag == 0 && kss.KeepSepAssoc1Id == ksaas.AssocId
                                            && (kss.KeepSepAssoc1SubsetId == ksaas.SubsetId ||
                                                kss.KeepSepAssoc2SubsetId != ksaas.SubsetId)),
                (kss, ksaas) => new KeepSeparateVm
                {
                    KeepSepLabel = KeepSepLabel.SUBSET,
                    KeepSepInmateId = kss.KeepSepSubsetSubsetId,
                    Assoc = lookupslist.SingleOrDefault(k => k.LookupIndex == ksaas.AssocId).LookupDescription,
                    Subset = lookupsSublist.SingleOrDefault(k => k.LookupIndex == ksaas.SubsetId).LookupDescription,
                    KeepSepAssoc = lookupslist.SingleOrDefault(k => k.LookupIndex == kss.KeepSepAssoc2Id).LookupDescription,
                    KeepSepAssocSubset = lookupsSublist.SingleOrDefault(k => k.LookupIndex == kss.KeepSepAssoc2SubsetId).LookupDescription,
                    KeepSepReason = kss.KeepSepReason
                }));

            return keepSepAlertDetails;
        }

        //Get KeepSeparate History Details List		
        public List<HistoryVm> GetKeepSeparateHistory(int keepSeparateId, KeepSepType keepSepType)
        {
            List<HistoryVm> keepSepHistoryLst = new List<HistoryVm>();

            switch (keepSepType)
            {
                case KeepSepType.Inmate:
                    keepSepHistoryLst = _context.KeepSeparateHistory.Where(ksh => ksh.KeepSeparateId == keepSeparateId)
                        .Select(ksh => new HistoryVm
                        {
                            PersonId = ksh.Personnel.PersonId,
                            OfficerBadgeNumber = ksh.Personnel.OfficerBadgeNum,
                            CreateDate = ksh.CreateDate,
                            HistoryList = ksh.KeepSeparateHistoryList
                        }).ToList();
                    break;
                case KeepSepType.Association:
                    keepSepHistoryLst = _context.KeepSepAssocInmateHistory
                        .Where(ksah => ksah.KeepSepAssocInmateId == keepSeparateId)
                        .Select(ksah => new HistoryVm
                        {
                            PersonId = ksah.Personnel.PersonId,
                            OfficerBadgeNumber = ksah.Personnel.OfficerBadgeNum,
                            CreateDate = ksah.CreateDate,
                            HistoryList = ksah.KeepSepAssocInmateHistoryList
                        }).ToList();
                    break;
                case KeepSepType.Subset:
                    keepSepHistoryLst = _context.KeepSepSubsetInmateHistory
                        .Where(kssh => kssh.KeepSepSubsetInmateId == keepSeparateId)
                        .Select(kssh => new HistoryVm
                        {
                            PersonId = kssh.Personnel.PersonId,
                            OfficerBadgeNumber = kssh.Personnel.OfficerBadgeNum,
                            CreateDate = kssh.CreateDate,
                            HistoryList = kssh.KeepSepSubsetInmateHistoryList
                        }).ToList();
                    break;
            }

            if (keepSepHistoryLst.Any())
            {
                //Get person details from keepsep history list
                int[] personIds = keepSepHistoryLst.Select(p => p.PersonId).ToArray();

                List<Person> lstPersonDet = (from per in _context.Person
                                             where personIds.Contains(per.PersonId)
                                             select new Person
                                             {
                                                 PersonId = per.PersonId,
                                                 PersonLastName = per.PersonLastName
                                             }).ToList();

                keepSepHistoryLst.ForEach(item =>
                {
                    item.PersonLastName =
                        lstPersonDet.SingleOrDefault(p => p.PersonId == item.PersonId)?.PersonLastName;
                    //To GetJson Result Into Dictionary
                    if (!string.IsNullOrEmpty(item.HistoryList))
                    {
                        Dictionary<string, string> personHistoryList =
                            JsonConvert.DeserializeObject<Dictionary<string, string>>(item.HistoryList);
                        item.Header = personHistoryList
                            .Select(ph => new PersonHeader { Header = ph.Key, Detail = ph.Value })
                            .ToList();
                    }
                });
            }

            return keepSepHistoryLst;
        }

        //Get KeepSeparate Assoc or Subset Inmate Details based on PersonClassification
        public List<KeepSepInmateDetailsVm> GetKeepSepAssocSubsetDetails(string keepSepType, int? subset,
            KeepSepType type)
        {
            List<KeepSepInmateDetailsVm> keepSepInmateDetailsLst = new List<KeepSepInmateDetailsVm>();

            List<int> personClassificationLst = _context.PersonClassification
                .Where(pc => pc.PersonClassificationType == keepSepType &&
                             (type != KeepSepType.Subset || pc.PersonClassificationSubsetId == subset))
                .Select(pc => pc.PersonId).ToList();

            if (personClassificationLst.Count > 0)
            {
                keepSepInmateDetailsLst = _context.Inmate
                    .Where(inm => inm.InmateActive == 1 && personClassificationLst.Contains(inm.PersonId))
                    .Select(inm => new KeepSepInmateDetailsVm
                    {
                        InmateId = inm.InmateId,
                        PeronId = inm.PersonId,
                        Location = inm.InmateCurrentTrack,
                        InmateNumber = inm.InmateNumber,
                        HousingUnitId = inm.HousingUnitId ?? 0,
                        Housing = new HousingDetail
                        {
                            FacilityAbbr = inm.Facility.FacilityAbbr,
                            HousingUnitNumber = inm.HousingUnit.HousingUnitNumber,
                            HousingUnitLocation = inm.HousingUnit.HousingUnitLocation,
                            HousingUnitBedLocation = inm.HousingUnit.HousingUnitBedLocation,
                            HousingUnitBedNumber = inm.HousingUnit.HousingUnitBedNumber
                        }
                    }).ToList();

                if (keepSepInmateDetailsLst.Any())
                {
                    IQueryable<Person> dbPersonDetails =
                        _context.Person.Where(per => personClassificationLst.Contains(per.PersonId));

                    keepSepInmateDetailsLst.ForEach(ks =>
                    {
                        Person personDetails = dbPersonDetails.SingleOrDefault(per => per.PersonId == ks.PeronId);
                        ks.FirstName = personDetails?.PersonFirstName;
                        ks.LastName = personDetails?.PersonLastName;
                    });
                }
            }

            return keepSepInmateDetailsLst;
        }

        //Get KeepSeparate Inmate Conflict Details
        public List<KeepSepInmateDetailsVm> GetKeepSepInmateConflictDetails(int inmateId)
        {
            List<KeyValuePair<int, string>> ksConflictedInmateLst = new List<KeyValuePair<int, string>>();

            IQueryable<Inmate> dbInmateDetails = _context.Inmate.Where(inm => inm.InmateActive == 1);

            //I1-I2
            List<KeepSepInmateDetailsVm> keepSepInmateLst = (from ks in _context.KeepSeparate
                                                             where ks.InactiveFlag == 0 && ks.KeepSeparateInmate1.InmateActive == 1
                                                                                        && ks.KeepSeparateInmate2.InmateActive == 1
                                                                                        && ks.KeepSeparateInmate1Id == inmateId
                                                             select new KeepSepInmateDetailsVm
                                                             {
                                                                 ConflictType = KeepSepTypeName.II,
                                                                 InmateId = ks.KeepSeparateInmate2Id,
                                                                 PeronId = ks.KeepSeparateInmate2.PersonId,
                                                                 Location = ks.KeepSeparateInmate2.InmateCurrentTrack,
                                                                 InmateNumber = ks.KeepSeparateInmate2.InmateNumber,
                                                                 FacilityId = ks.KeepSeparateInmate2.FacilityId,
                                                                 HousingUnitId = ks.KeepSeparateInmate2.HousingUnitId ?? 0
                                                             }).ToList();

            //I2-I1
            keepSepInmateLst.AddRange(from ks in _context.KeepSeparate
                                      where ks.InactiveFlag == 0 && ks.KeepSeparateInmate1.InmateActive == 1
                                                                 && ks.KeepSeparateInmate2.InmateActive == 1
                                                                 && ks.KeepSeparateInmate2Id == inmateId
                                      select new KeepSepInmateDetailsVm
                                      {
                                          ConflictType = KeepSepTypeName.II,
                                          InmateId = ks.KeepSeparateInmate1Id,
                                          PeronId = ks.KeepSeparateInmate1.PersonId,
                                          Location = ks.KeepSeparateInmate1.InmateCurrentTrack,
                                          InmateNumber = ks.KeepSeparateInmate1.InmateNumber,
                                          FacilityId = ks.KeepSeparateInmate1.FacilityId,
                                          HousingUnitId = ks.KeepSeparateInmate1.HousingUnitId ?? 0
                                      });

            var lstKeepSepActiveAssocSubset = _context.PersonClassification
                .SelectMany(pc => dbInmateDetails
                        .Where(w => pc.PersonId == w.PersonId && pc.InactiveFlag == 0
                                                              && pc.PersonClassificationDateFrom < DateTime.Now
                                                              && (!pc.PersonClassificationDateThru.HasValue
                                                                  || pc.PersonClassificationDateThru >= DateTime.Now)),
                    (pc, i) =>
                        new
                        {
                            i.InmateId,
                            i.PersonId,
                            i.InmateCurrentTrack,
                            i.InmateNumber,
                            i.FacilityId,
                            i.HousingUnitId,                            
                            Assoc = pc.PersonClassificationType,
                            Subset = pc.PersonClassificationSubset,
                            AssocId = pc.PersonClassificationTypeId,
                            SubsetId = pc.PersonClassificationSubsetId,
                        }).ToList();

            //I-A
            keepSepInmateLst.AddRange(lstKeepSepActiveAssocSubset.SelectMany(lsas => _context.KeepSepAssocInmate.Where(
                    ksa =>
                        ksa.KeepSepInmate2Id == inmateId && ksa.DeleteFlag == 0 && lsas.AssocId == ksa.KeepSepAssoc1Id),
                (lsas, ksa) => new KeepSepInmateDetailsVm
                {
                    ConflictType = KeepSepTypeName.IA,
                    InmateId = lsas.InmateId,
                    PeronId = lsas.PersonId,
                    Location = lsas.InmateCurrentTrack,
                    InmateNumber = lsas.InmateNumber,
                    FacilityId = lsas.FacilityId,
                    HousingUnitId = lsas.HousingUnitId ?? 0
                }).ToList());

            var filteredAssocSub = lstKeepSepActiveAssocSubset.Where(asi => asi.InmateId == inmateId);

            //A-I 
            ksConflictedInmateLst.AddRange(filteredAssocSub.SelectMany(lsas =>
                    _context.KeepSepAssocInmate
                        .Where(ksai => lsas.InmateId == inmateId && ksai.DeleteFlag == 0 &&
                                       lsas.AssocId == ksai.KeepSepAssoc1Id && ksai.KeepSepInmate2.InmateActive == 1),
                (lsas, ksai) => new KeyValuePair<int, string>(ksai.KeepSepInmate2Id, KeepSepTypeName.AI)).ToList());

            //S-I
            ksConflictedInmateLst.AddRange(filteredAssocSub.SelectMany(ksaas =>
                    _context.KeepSepSubsetInmate.Where(kssi => ksaas.InmateId == inmateId && kssi.DeleteFlag == 0 &&
                                                               kssi.KeepSepAssoc1Id == ksaas.AssocId &&
                                                               kssi.KeepSepAssoc1SubsetId == ksaas.SubsetId),
                (ksaas, kssi) => new KeyValuePair<int, string>(kssi.KeepSepInmate2Id, KeepSepTypeName.SI)).ToList());

            //I-S
            keepSepInmateLst.AddRange(lstKeepSepActiveAssocSubset.SelectMany(ksaas =>
                _context.KeepSepSubsetInmate.Where(kssi => kssi.KeepSepInmate2Id == inmateId && kssi.DeleteFlag == 0 &&
                                                           kssi.KeepSepAssoc1Id == ksaas.AssocId &&
                                                           kssi.KeepSepAssoc1SubsetId == ksaas.SubsetId), (ksaas, kssi) =>
                new KeepSepInmateDetailsVm
                {
                    ConflictType = KeepSepTypeName.IS,
                    InmateId = ksaas.InmateId,
                    PeronId = ksaas.PersonId,
                    Location = ksaas.InmateCurrentTrack,
                    InmateNumber = ksaas.InmateNumber,
                    FacilityId = ksaas.FacilityId,
                    HousingUnitId = ksaas.HousingUnitId ?? 0
                }).ToList());

            //A1-A2
            List<int> filteredKeepSepA1A2 = _context.KeepSepAssocAssoc
                .SelectMany(
                    ksaa => lstKeepSepActiveAssocSubset.Where(ks =>
                        ks.AssocId == ksaa.KeepSepAssoc1Id && ks.InmateId == inmateId && ksaa.DeleteFlag == 0),
                    (ksaa, ks) => ksaa.KeepSepAssocAssocId).ToList();

            keepSepInmateLst.AddRange(lstKeepSepActiveAssocSubset.SelectMany(kaa => _context.KeepSepAssocAssoc.Where(
                ks =>
                    filteredKeepSepA1A2.Contains(ks.KeepSepAssocAssocId) &&
                    ks.KeepSepAssoc2Id == kaa.AssocId), (kaa, ks) => new KeepSepInmateDetailsVm
                    {
                        ConflictType = KeepSepTypeName.AA, //"A1-A2"
                        InmateId = kaa.InmateId,
                        PeronId = kaa.PersonId,
                        Location = kaa.InmateCurrentTrack,
                        InmateNumber = kaa.InmateNumber,
                        FacilityId = kaa.FacilityId,
                        HousingUnitId = kaa.HousingUnitId ?? 0
                    }).ToList());

            //A2-A1
            List<int> filteredKeepSepA2A1 = _context.KeepSepAssocAssoc
                .SelectMany(
                    ksaa => lstKeepSepActiveAssocSubset.Where(ks =>
                        ks.AssocId == ksaa.KeepSepAssoc2Id && ks.InmateId == inmateId && ksaa.DeleteFlag == 0),
                    (ksaa, ks) => ksaa.KeepSepAssocAssocId).ToList();

            keepSepInmateLst.AddRange(lstKeepSepActiveAssocSubset.SelectMany(kaa => _context.KeepSepAssocAssoc.Where(
                ks =>
                    filteredKeepSepA2A1.Contains(ks.KeepSepAssocAssocId)
                    && ks.KeepSepAssoc1Id == kaa.AssocId), (kaa, ks) => new KeepSepInmateDetailsVm
                    {
                        ConflictType = KeepSepTypeName.AA, //"A2-A1"
                        InmateId = kaa.InmateId,
                        PeronId = kaa.PersonId,
                        Location = kaa.InmateCurrentTrack,
                        InmateNumber = kaa.InmateNumber,
                        FacilityId = kaa.FacilityId,
                        HousingUnitId = kaa.HousingUnitId ?? 0
                    }).ToList());

            //A1-S2
            List<int> filteredA1S2 = _context.KeepSepAssocSubset
                .SelectMany(
                    ksas => lstKeepSepActiveAssocSubset.Where(lks =>
                        lks.InmateId == inmateId && lks.AssocId == ksas.KeepSepAssoc1Id && ksas.DeleteFlag == 0),
                    (ksas, lks) => ksas.KeepSepAssocSubsetId).ToList();

            keepSepInmateLst.AddRange(_context.KeepSepAssocSubset
                .SelectMany(ksas => lstKeepSepActiveAssocSubset.Where(lks =>
                        lks.SubsetId == ksas.KeepSepAssoc2SubsetId && lks.AssocId == ksas.KeepSepAssoc2Id
                                                               && filteredA1S2.Contains(ksas.KeepSepAssocSubsetId)),
                    (ksas, lks) => new KeepSepInmateDetailsVm
                    {
                        ConflictType = KeepSepTypeName.AS, // "A1-S2"
                        InmateId = lks.InmateId,
                        PeronId = lks.PersonId,
                        Location = lks.InmateCurrentTrack,
                        InmateNumber = lks.InmateNumber,
                        FacilityId = lks.FacilityId,
                        HousingUnitId = lks.HousingUnitId ?? 0
                    }).ToList());

            List<int> filteredA2S1 = _context.KeepSepAssocSubset
                .SelectMany(ksas => lstKeepSepActiveAssocSubset
                        .Where(lks => lks.InmateId == inmateId && ksas.DeleteFlag == 0
                                                               && lks.SubsetId == ksas.KeepSepAssoc2SubsetId &&
                                                               lks.AssocId == ksas.KeepSepAssoc2Id),
                    (ksas, lks) => ksas.KeepSepAssocSubsetId).ToList();
            //A2-S1
            keepSepInmateLst.AddRange(_context.KeepSepAssocSubset
                .SelectMany(ksas => lstKeepSepActiveAssocSubset.Where(lks =>
                        string.IsNullOrEmpty(lks.Subset) && lks.AssocId == ksas.KeepSepAssoc1Id
                                                         && filteredA2S1.Contains(ksas.KeepSepAssocSubsetId)),
                    (ksas, lks) => new KeepSepInmateDetailsVm
                    {
                        ConflictType = KeepSepTypeName.AS, // "A2-S1"
                        InmateId = lks.InmateId,
                        PeronId = lks.PersonId,
                        Location = lks.InmateCurrentTrack,
                        InmateNumber = lks.InmateNumber,
                        FacilityId = lks.FacilityId,
                        HousingUnitId = lks.HousingUnitId ?? 0
                    }).ToList());

            //S1-S2
            List<int> filteredS1S2Ids = _context.KeepSepSubsetSubset
                .SelectMany(ksss => lstKeepSepActiveAssocSubset
                        .Where(lks => lks.InmateId == inmateId && ksss.DeleteFlag == 0
                                                               && ksss.KeepSepAssoc1Id == lks.AssocId &&
                                                               ksss.KeepSepAssoc1SubsetId == lks.SubsetId),
                    (ksss, lks) => ksss.KeepSepSubsetSubsetId).ToList();

            keepSepInmateLst.AddRange(_context.KeepSepSubsetSubset
                .SelectMany(ksss => lstKeepSepActiveAssocSubset
                        .Where(lks => filteredS1S2Ids.Contains(ksss.KeepSepSubsetSubsetId)
                                      && ksss.KeepSepAssoc2Id == lks.AssocId && ksss.KeepSepAssoc2SubsetId == lks.SubsetId),
                    (ksss, lks) => new KeepSepInmateDetailsVm
                    {
                        ConflictType = KeepSepTypeName.SS, //"S1-S2"
                        InmateId = lks.InmateId,
                        PeronId = lks.PersonId,
                        Location = lks.InmateCurrentTrack,
                        InmateNumber = lks.InmateNumber,
                        FacilityId = lks.FacilityId,
                        HousingUnitId = lks.HousingUnitId ?? 0
                    }).ToList());

            //S2-S1
            List<int> filteredS2S1Ids = _context.KeepSepSubsetSubset
                .SelectMany(ksss => lstKeepSepActiveAssocSubset
                        .Where(lks => lks.InmateId == inmateId && ksss.DeleteFlag == 0
                                                               && ksss.KeepSepAssoc2Id == lks.AssocId &&
                                                               ksss.KeepSepAssoc2SubsetId == lks.SubsetId),
                    (ksss, lks) => ksss.KeepSepSubsetSubsetId).ToList();

            keepSepInmateLst.AddRange(_context.KeepSepSubsetSubset
                .SelectMany(ksss => lstKeepSepActiveAssocSubset
                        .Where(lks => filteredS2S1Ids.Contains(ksss.KeepSepSubsetSubsetId)
                                      && ksss.KeepSepAssoc1Id == lks.AssocId && ksss.KeepSepAssoc1SubsetId == lks.SubsetId),
                    (ksss, lks) => new KeepSepInmateDetailsVm
                    {
                        ConflictType = KeepSepTypeName.SS, // "S2-S1"
                        InmateId = lks.InmateId,
                        PeronId = lks.PersonId,
                        Location = lks.InmateCurrentTrack,
                        InmateNumber = lks.InmateNumber,
                        FacilityId = lks.FacilityId,
                        HousingUnitId = lks.HousingUnitId ?? 0
                    }).ToList());

            //Add A-I and S-I Conflicts 
            if (ksConflictedInmateLst.Count > 0)
            {
                keepSepInmateLst.AddRange(dbInmateDetails.SelectMany(inm =>
                        ksConflictedInmateLst.Where(cf => inm.InmateId == cf.Key),
                    (inm, cfl) => new KeepSepInmateDetailsVm
                    {
                        ConflictType = cfl.Value,
                        InmateId = inm.InmateId,
                        PeronId = inm.PersonId,
                        Location = inm.InmateCurrentTrack,
                        InmateNumber = inm.InmateNumber,
                        FacilityId = inm.FacilityId,
                        HousingUnitId = inm.HousingUnitId ?? 0
                    }));
            }

            //To Add Housing and Person Details
            if (keepSepInmateLst.Any())
            {
                List<int> filteredHousingUnitId = keepSepInmateLst.Select(ks => ks.HousingUnitId).ToList();
                List<int> personIdLst = keepSepInmateLst.Select(ks => ks.PeronId).Distinct().ToList();

                List<HousingDetail> dbHousingUnitDetails =
                    _context.HousingUnit.Where(hu => filteredHousingUnitId.Contains(hu.HousingUnitId))
                        .Select(hu => new HousingDetail
                        {
                            HousingUnitId = hu.HousingUnitId,
                            HousingUnitNumber = hu.HousingUnitNumber,
                            HousingUnitLocation = hu.HousingUnitLocation,
                            HousingUnitBedLocation = hu.HousingUnitBedLocation,
                            HousingUnitBedNumber = hu.HousingUnitBedNumber
                        }).ToList();
                IQueryable<Facility> dbFacilityDetails = _context.Facility.Where(fac => fac.DeleteFlag == 0);

                List<PersonInfoVm> dbPersonDetails = _context.Person.Where(per => personIdLst.Contains(per.PersonId))
                    .Select(p => new PersonInfoVm
                    {
                        PersonId = p.PersonId,
                        PersonFirstName = p.PersonFirstName,
                        PersonLastName = p.PersonLastName
                    }).ToList();

                keepSepInmateLst.ForEach(ksi =>
                {
                    ksi.Housing = dbHousingUnitDetails.SingleOrDefault(fc => fc.HousingUnitId == ksi.HousingUnitId) ??
                                  new HousingDetail();
                    ksi.Housing.FacilityAbbr = dbFacilityDetails.SingleOrDefault(fc => fc.FacilityId == ksi.FacilityId)
                        ?.FacilityAbbr;

                    PersonInfoVm personDetails = dbPersonDetails.SingleOrDefault(per => per.PersonId == ksi.PeronId);
                    ksi.FirstName = personDetails?.PersonFirstName;
                    ksi.LastName = personDetails?.PersonLastName;
                });
            }

            return keepSepInmateLst.OrderBy(ks => ks.LastName).ToList();
        }

        #endregion

        #region Check Duplicate/Existing KeepSep

        private string CheckDupKeepSepInmate(int keepSeparateId, int inmateId, int kepSepInmateId)
        {
            string errorMsg = string.Empty;

            KeepSeparate dbKeepSepDetails = _context.KeepSeparate.Where(ksp => ksp.KeepSeparateId != keepSeparateId &&
               (ksp.KeepSeparateInmate1Id == inmateId && ksp.KeepSeparateInmate2Id == kepSepInmateId
                || ksp.KeepSeparateInmate1Id == kepSepInmateId && ksp.KeepSeparateInmate2Id == inmateId))
                .Select(ks => new KeepSeparate
                {
                    InactiveFlag = ks.InactiveFlag,
                    KeepSeparateId = ks.KeepSeparateId
                }).FirstOrDefault();

            Inmate inactiveInmate = _context.Inmate.FirstOrDefault(inm => inm.InmateActive == 0
                  && (inm.InmateId == inmateId || inm.InmateId == kepSepInmateId));

            if (dbKeepSepDetails != null)
            {
                errorMsg = dbKeepSepDetails.InactiveFlag == 1 && inactiveInmate != null
                    ? KeepSepErrorMessage.RELEASEDANDINACTIVEKEEPSEPEXISTS
                    : dbKeepSepDetails.InactiveFlag == 1
                        ? KeepSepErrorMessage.INACTIVEKEEPSEPEXISTS
                        : KeepSepErrorMessage.ALREADYEXISTS;
            }

            return errorMsg;
        }

        private string CheckDupKeepSepAssoc(int keepSepAssocId, int inmateId, int? kepSepAssoc)
        {
            string errorMsg = string.Empty;
            KeepSepAssocInmate dbKeepSepDetails = _context.KeepSepAssocInmate
                .SingleOrDefault(ksai =>
                    ksai.KeepSepInmate2Id == inmateId && ksai.KeepSepAssoc1Id == kepSepAssoc &&
                    ksai.KeepSepAssocInmateId != keepSepAssocId);
            if (dbKeepSepDetails != null)
            {
                errorMsg = dbKeepSepDetails.DeleteFlag == 1 ? KeepSepErrorMessage.INACTIVEKEEPSEPEXISTS : KeepSepErrorMessage.ALREADYEXISTS;
            }

            return errorMsg;
        }

        private string CheckDupKeepSepSubset(int keepSepSubsetId, int inmateId, int? kepSepAssoc, int? kepSepSubset)
        {
            string errorMsg = string.Empty;
            KeepSepSubsetInmate dbKeepSepDetails = _context.KeepSepSubsetInmate.SingleOrDefault(kssi =>
                kssi.KeepSepInmate2Id == inmateId
                && kssi.KeepSepAssoc1Id == kepSepAssoc && kssi.KeepSepAssoc1SubsetId == kepSepSubset &&
                kssi.KeepSepSubsetInmateId != keepSepSubsetId);
            if (dbKeepSepDetails != null)
            {
                errorMsg = dbKeepSepDetails.DeleteFlag == 1 ? KeepSepErrorMessage.INACTIVEKEEPSEPEXISTS : KeepSepErrorMessage.ALREADYEXISTS;
            }

            return errorMsg;
        }

        #endregion

        #region Insert or Update KeepSeparate Details

        //Insert or Update KeepSeparate Inmate Details
        public async Task<string> InsertUpdateKeepSepInmateDetails(KeepSeparateVm keepSepDetails)
        {
            bool isExist = true;
            string dupRecErrorMsg = CheckDupKeepSepInmate(keepSepDetails.KeepSeparateId, keepSepDetails.KeepSepInmateId,
                keepSepDetails.KeepSepInmate2Id);
            if (dupRecErrorMsg != string.Empty) return dupRecErrorMsg;
            KeepSeparate dbKeepSepDetails =
                _context.KeepSeparate.SingleOrDefault(asi => asi.KeepSeparateId == keepSepDetails.KeepSeparateId);

            if (dbKeepSepDetails is null)
            {
                dbKeepSepDetails = new KeepSeparate
                {
                    KeepSeparateDate = DateTime.Now,
                    KeepSeparateOfficerId = _personnelId,
                    KeepSeparateInmate1Id = keepSepDetails.KeepSepInmateId,
                    KeepSeparateInmate2Id = keepSepDetails.KeepSepInmate2Id
                };
                isExist = false;
            }

            dbKeepSepDetails.KeepSeparateReason = keepSepDetails.KeepSepReason;
            dbKeepSepDetails.KeepSeparateType = keepSepDetails.KeepSepType;
            dbKeepSepDetails.KeepSeparateNote = keepSepDetails.KeepSeparateNote;
            dbKeepSepDetails.InactiveFlag = 0;
            dbKeepSepDetails.UpdatedByOfficerId = _personnelId;
            dbKeepSepDetails.UpdatedByDate = DateTime.Now;

            if (!isExist)
            {
                _context.KeepSeparate.Add(dbKeepSepDetails);
            }
            
            await _context.SaveChangesAsync();

            _interfaceEngine.Export(new ExportRequestVm
            {
                EventName = EventNameConstants.KEEPSEPINMATE,
                PersonnelId = _personnelId,
                Param1 = _context.Inmate.Single(a => a.InmateId == keepSepDetails.KeepSepInmateId)
                .PersonId.ToString(),
                Param2 = dbKeepSepDetails.KeepSeparateId.ToString()
            });

            keepSepDetails.KeepSeparateId = dbKeepSepDetails.KeepSeparateId;
            await InsertKeepSepInmateHistory(keepSepDetails);

            return dupRecErrorMsg;
        }

        //Insert or Update KeepSepAssoc Inmate Details
        public async Task<string> InsertUpdateKeepSepAssocDetails(KeepSeparateVm keepSepDetails)
        {
            bool isExist = true;
            string dupRecErrorMsg = CheckDupKeepSepAssoc(keepSepDetails.KeepSepAssocInmateId,
                keepSepDetails.KeepSepInmateId, keepSepDetails.KeepSepAssoc1Id);

            if (dupRecErrorMsg != string.Empty) return dupRecErrorMsg;
            KeepSepAssocInmate dbKeepSepAssocInmate =
                _context.KeepSepAssocInmate.SingleOrDefault(asi =>
                    asi.KeepSepAssocInmateId == keepSepDetails.KeepSepAssocInmateId);
            if (dbKeepSepAssocInmate is null)
            {
                dbKeepSepAssocInmate = new KeepSepAssocInmate
                {
                    CreateBy = _personnelId,
                    CreateDate = DateTime.Now,
                    KeepSepDate = DateTime.Now,
                    KeepSepInmate2Id = keepSepDetails.KeepSepInmateId,
                    KeepSepOfficerId = _personnelId
                };
                isExist = false;
            }

            dbKeepSepAssocInmate.KeepSepAssoc1 = keepSepDetails.KeepSepAssoc;
            dbKeepSepAssocInmate.KeepSepReason = keepSepDetails.KeepSepReason;
            dbKeepSepAssocInmate.KeepSeparateType = keepSepDetails.KeepSepType;
            dbKeepSepAssocInmate.KeepSeparateNote = keepSepDetails.KeepSeparateNote;
            dbKeepSepAssocInmate.UpdateDate = DateTime.Now;
            dbKeepSepAssocInmate.UpdateBy = _personnelId;
            dbKeepSepAssocInmate.KeepSepAssoc1Id = keepSepDetails.KeepSepAssoc1Id;

            if (!isExist)
            {
                _context.KeepSepAssocInmate.Add(dbKeepSepAssocInmate);
            }

            await _context.SaveChangesAsync();

            _interfaceEngine.Export(new ExportRequestVm
            {
                EventName = EventNameConstants.KEEPSEPASSOC,
                PersonnelId = _personnelId,
                Param1 = _context.Inmate.Single(a => a.InmateId == dbKeepSepAssocInmate.KeepSepInmate2Id)
                .PersonId.ToString(),
                Param2 = dbKeepSepAssocInmate.KeepSepAssocInmateId.ToString()
            });

            keepSepDetails.KeepSepAssocInmateId = dbKeepSepAssocInmate.KeepSepAssocInmateId;
            await InsertKeepSepAssocHistory(keepSepDetails);

            return dupRecErrorMsg;
        }

        //Insert or Update KeepSepSubset Inmate Details
        public async Task<string> InsertUpdateKeepSepSubsetDetails(KeepSeparateVm keepSepDetails)
        {
            bool isExist = true;
            string dupRecErrorMsg = CheckDupKeepSepSubset(keepSepDetails.KeepSepSubsetInmateId,
                keepSepDetails.KeepSepInmateId, keepSepDetails.KeepSepAssoc1Id, keepSepDetails.KeepSepAssoc1SubsetId);

            if (dupRecErrorMsg != string.Empty) return dupRecErrorMsg;
            KeepSepSubsetInmate dbKeepSepSubsetInmate =
                _context.KeepSepSubsetInmate.SingleOrDefault(asi =>
                    asi.KeepSepSubsetInmateId == keepSepDetails.KeepSepSubsetInmateId);

            if (dbKeepSepSubsetInmate is null)
            {
                dbKeepSepSubsetInmate = new KeepSepSubsetInmate
                {
                    CreateBy = _personnelId,
                    CreateDate = DateTime.Now,
                    KeepSepDate = DateTime.Now,
                    KeepSepOfficerId = _personnelId,
                    KeepSepInmate2Id = keepSepDetails.KeepSepInmateId
                };
                isExist = false;
            }

            dbKeepSepSubsetInmate.KeepSepReason = keepSepDetails.KeepSepReason;
            dbKeepSepSubsetInmate.KeepSepAssoc1 = keepSepDetails.KeepSepAssoc;
            dbKeepSepSubsetInmate.KeepSepAssoc1Subset = keepSepDetails.KeepSepAssocSubset;
            dbKeepSepSubsetInmate.KeepSeparateType = keepSepDetails.KeepSepType;
            dbKeepSepSubsetInmate.KeepSeparateNote = keepSepDetails.KeepSeparateNote;
            dbKeepSepSubsetInmate.UpdateBy = _personnelId;
            dbKeepSepSubsetInmate.UpdateDate = DateTime.Now;
            dbKeepSepSubsetInmate.KeepSepAssoc1SubsetId = keepSepDetails.KeepSepAssoc1SubsetId;
            dbKeepSepSubsetInmate.KeepSepAssoc1Id = keepSepDetails.KeepSepAssoc1Id;
            if (!isExist)
            {
                _context.KeepSepSubsetInmate.Add(dbKeepSepSubsetInmate);
            }

            await _context.SaveChangesAsync();

            _interfaceEngine.Export(new ExportRequestVm
            {
                EventName = EventNameConstants.KEEPSEPSUBSET,
                PersonnelId = _personnelId,
                Param1 = _context.Inmate.Single(a => a.InmateId == dbKeepSepSubsetInmate.KeepSepInmate2Id)
                .PersonId.ToString(),
                Param2 = dbKeepSepSubsetInmate.KeepSepSubsetInmateId.ToString()
            });

            keepSepDetails.KeepSepSubsetInmateId = dbKeepSepSubsetInmate.KeepSepSubsetInmateId;
            await InsertKeepSepSubsetHistory(keepSepDetails);

            return dupRecErrorMsg;
        }

        #endregion

        #region Insert KeepSeparate History Details

        //Insert KeepSeparate Inmate History
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

        //Insert KeepSeparate Assoc History
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

        //Insert KeepSeparate Subset History
        private async Task InsertKeepSepSubsetHistory(KeepSeparateVm keepSepDetails)
        {
            KeepSepSubsetInmateHistory keepSepSubsetInmateHistory = new KeepSepSubsetInmateHistory
            {
                KeepSepSubsetInmateId = keepSepDetails.KeepSepSubsetInmateId,
                KeepSepSubsetInmateHistoryList = keepSepDetails.KeepSepHistoryList,
                PersonnelId = _personnelId,
                CreateDate = DateTime.Now
            };
            _context.KeepSepSubsetInmateHistory.Add(keepSepSubsetInmateHistory);
            await _context.SaveChangesAsync();
        }

        #endregion

        #region Delete or Undo KeepSeparate Details List

        public async Task<bool> DeleteUndoKeepSeparateDetails(KeepSeparateVm keepSepDetails)
        {
            bool isUpdate = false;
            switch (keepSepDetails.KeepSepLabel)
            {
                case KeepSepLabel.INMATE:
                    isUpdate = await DeleteUndoKeepSepInmateDetailsAsync(keepSepDetails);
                    break;
                case KeepSepLabel.ASSOC:
                    isUpdate = await DeleteUndoKeepSepAssocDetailsAsync(keepSepDetails);
                    break;
                case KeepSepLabel.SUBSET:
                    isUpdate = await DeleteUndoKeepSepSubsetDetailsAsync(keepSepDetails);
                    break;
            }

            return isUpdate;
        }

        private async Task<bool> DeleteUndoKeepSepInmateDetailsAsync(KeepSeparateVm keepSepDetails)
        {
            KeepSeparate dbKeepSeparate =
                _context.KeepSeparate.SingleOrDefault(ks => ks.KeepSeparateId == keepSepDetails.KeepSeparateId);
            if (dbKeepSeparate == null) return false;
            dbKeepSeparate.UpdatedByDate = DateTime.Now;
            dbKeepSeparate.UpdatedByOfficerId = _personnelId;
            dbKeepSeparate.InactiveFlag = keepSepDetails.IsDeleted ? 1 : 0;
            _context.SaveChanges();
            if (keepSepDetails.IsDeleted)
            {
                _interfaceEngine.Export(new ExportRequestVm
                {
                    EventName = EventNameConstants.KEEPSEPREMOVEINMATE,
                    PersonnelId = _personnelId,
                    Param1 = _context.Inmate.Single(a => a.InmateId == dbKeepSeparate.KeepSeparateInmate1Id)
                .PersonId.ToString(),
                    Param2 = dbKeepSeparate.KeepSeparateId.ToString()
                });
            }
            await InsertKeepSepInmateHistory(keepSepDetails);
            return true;
        }

        private async Task<bool> DeleteUndoKeepSepAssocDetailsAsync(KeepSeparateVm keepSepDetails)
        {
            KeepSepAssocInmate dbkeepSepAssocInmate =
                _context.KeepSepAssocInmate.SingleOrDefault(ksa =>
                    ksa.KeepSepAssocInmateId == keepSepDetails.KeepSepAssocInmateId);
            if (dbkeepSepAssocInmate == null) return false;
            DateTime? currentDate = DateTime.Now;
            dbkeepSepAssocInmate.DeleteFlag = keepSepDetails.IsDeleted ? 1 : 0;
            dbkeepSepAssocInmate.DeleteBy = keepSepDetails.IsDeleted ? _personnelId : new int?();
            dbkeepSepAssocInmate.DeleteDate = keepSepDetails.IsDeleted ? currentDate : null;
            _context.SaveChanges();
            if (keepSepDetails.IsDeleted)
            {
                _interfaceEngine.Export(new ExportRequestVm
                {
                    EventName = EventNameConstants.KEEPSEPREMOVEASSOC,
                    PersonnelId = _personnelId,
                    Param1 = _context.Inmate.Single(a => a.InmateId == dbkeepSepAssocInmate.KeepSepInmate2Id)
                .PersonId.ToString(),
                    Param2 = dbkeepSepAssocInmate.KeepSepAssocInmateId.ToString()
                });
            }
            await InsertKeepSepAssocHistory(keepSepDetails);
            return true;
        }

        private async Task<bool> DeleteUndoKeepSepSubsetDetailsAsync(KeepSeparateVm keepSepDetails)
        {
            KeepSepSubsetInmate dbKeepSepSubsetInmate =
                _context.KeepSepSubsetInmate.SingleOrDefault(kssi =>
                    kssi.KeepSepSubsetInmateId == keepSepDetails.KeepSepSubsetInmateId);

            if (dbKeepSepSubsetInmate == null) return false;
            DateTime? currentDate = DateTime.Now;
            dbKeepSepSubsetInmate.DeleteFlag = keepSepDetails.IsDeleted ? 1 : 0;
            dbKeepSepSubsetInmate.DeleteBy = keepSepDetails.IsDeleted ? _personnelId : new int?();
            dbKeepSepSubsetInmate.DeleteDate = keepSepDetails.IsDeleted ? currentDate : null;
            _context.SaveChanges();
            if (keepSepDetails.IsDeleted)
            {
                _interfaceEngine.Export(new ExportRequestVm
                {
                    EventName = EventNameConstants.KEEPSEPREMOVESUBSET,
                    PersonnelId = _personnelId,
                    Param1 = _context.Inmate.Single(a => a.InmateId == dbKeepSepSubsetInmate.KeepSepInmate2Id)
                    .PersonId.ToString(),
                    Param2 = dbKeepSepSubsetInmate.KeepSepSubsetInmateId.ToString()
                });
            }
            await InsertKeepSepSubsetHistory(keepSepDetails);
            return true;
        }

        #endregion

        //Get Inmate Keep Separate Details
        public List<KeepSeparateVm> GetInmateKeepSep(int inmateId, bool isNotFromAppt = true)
        {
            //KeepSeparateVm KeepSep = new KeepSeparateVm();
            //Keep Sep Inmate 1 List
            List<KeepSeparateVm> lstKeepSeparate = (from ks in _context.KeepSeparate
                where ks.InactiveFlag == 0 && ks.KeepSeparateInmate1Id == inmateId
                    && (from i in _context.Inmate where i.InmateActive == 1 select i.InmateId)
                    .Contains(ks.KeepSeparateInmate2Id)
                select new KeepSeparateVm
                {
                    KeepSepLabel = KeepSepLabel.KEEPSEPINMATE, // "KEEP SEP INMATE"
                    PersonId = ks.KeepSeparateInmate2.PersonId,
                    InmateNumber = ks.KeepSeparateInmate2.InmateNumber,
                    KeepSepType = ks.KeepSeparateType,
                    KeepSepReason = ks.KeepSeparateReason,
                    HousingUnitId = ks.KeepSeparateInmate2.HousingUnitId,
                    KeepSepTypeName = KeepSepTypeName.II, // "I-I"
                    KeepSepInmateId = ks.KeepSeparateInmate2Id
                }).ToList();

            //Keep Sep Inmate 2 List
            lstKeepSeparate.AddRange(from ks in _context.KeepSeparate
                                     where ks.InactiveFlag == 0 && ks.KeepSeparateInmate2Id == inmateId
                                                                && (from i in _context.Inmate where i.InmateActive == 1 select i.InmateId)
                                                                .Contains(ks.KeepSeparateInmate1Id)
                                     select new KeepSeparateVm
                                     {
                                         KeepSepLabel = KeepSepLabel.KEEPSEPINMATE, // "KEEP SEP INMATE"
                                         PersonId = ks.KeepSeparateInmate1.PersonId,
                                         InmateNumber = ks.KeepSeparateInmate1.InmateNumber,
                                         KeepSepType = ks.KeepSeparateType,
                                         KeepSepReason = ks.KeepSeparateReason,
                                         HousingUnitId = ks.KeepSeparateInmate1.HousingUnitId,
                                         KeepSepTypeName = KeepSepTypeName.II, // "I-I"
                                         KeepSepInmateId = ks.KeepSeparateInmate1Id
                                     });

            if (isNotFromAppt)
            {
                List<Lookup> lookupslist = _context.Lookup.Where(w => w.LookupType == LookupConstants.CLASSGROUP).ToList();
                List<Lookup> lookupsSublist = _context.Lookup.Where(w => w.LookupType == LookupConstants.CLASSGROUPSUB).ToList();
                //Keep Sep Assoc List: INMATE KEEP SEPARATE To ASSOCIATION
                lstKeepSeparate.AddRange(from ksa in _context.KeepSepAssocInmate
                    where ksa.DeleteFlag == 0 && ksa.KeepSepInmate2Id == inmateId
                    select new KeepSeparateVm
                    {
                        KeepSepLabel = KeepSepLabel.KEEPSEPASSOC, // "KEEP SEP ASSOC"
                        KeepSepAssoc = lookupslist.SingleOrDefault(k => k.LookupIndex == ksa.KeepSepAssoc1Id)
                            .LookupDescription,
                        KeepSepAssoc1Id = ksa.KeepSepAssoc1Id,
                        KeepSepType = ksa.KeepSeparateType,
                        KeepSepReason = ksa.KeepSepReason,
                        KeepSepTypeName = KeepSepTypeName.IA // "I-A"
                    });

                //Keep Sep Subset List: INMATE KEEP SEPARATE To SUBSET
                lstKeepSeparate.AddRange(from kss in _context.KeepSepSubsetInmate
                    where kss.DeleteFlag == 0 && kss.KeepSepInmate2Id == inmateId
                    select new KeepSeparateVm
                    {
                        KeepSepLabel =
                            KeepSepLabel
                                .KEEPSEPSUBSET, // "KEEP SEP SUBSET"                                                                                 
                        KeepSepAssoc1Id = kss.KeepSepAssoc1Id,
                        KeepSepAssoc1SubsetId = kss.KeepSepAssoc1SubsetId,
                        KeepSepAssoc = lookupslist.SingleOrDefault(s => s.LookupIndex == kss.KeepSepAssoc1Id)
                            .LookupDescription,
                        KeepSepAssocSubset = lookupsSublist
                            .SingleOrDefault(s => s.LookupIndex == kss.KeepSepAssoc1SubsetId).LookupDescription,
                        KeepSepType = kss.KeepSeparateType,
                        KeepSepReason = kss.KeepSepReason,
                        KeepSepTypeName = KeepSepTypeName.IS // "I-S"
                    });

                //Keep Sep Active Assoc Subset List
                var lstKeepSepActiveAssocSubset = (from pc in _context.PersonClassification
                    from i in _context.Inmate
                    where pc.PersonId == i.PersonId && pc.InactiveFlag == 0
                        && pc.PersonClassificationDateFrom < DateTime.Now &&
                        (!pc.PersonClassificationDateThru.HasValue || pc.PersonClassificationDateThru >= DateTime.Now)
                        && i.InmateId == inmateId
                    select new
                    {
                        i.InmateId,
                        Assoc = pc.PersonClassificationType,
                        Subset = pc.PersonClassificationSubset,
                        AssocId = pc.PersonClassificationTypeId,
                        SubsetId = pc.PersonClassificationSubsetId,
                    }).ToList();

                //TODO: Move evaluation to the server. What are we trying to do here, anyway?
                //Assoc Keep Sep List : ASSOCIATION KEEP SEPARATE To ASSOCIATION 
                var ksaList = (from ksa in _context.KeepSepAssocInmate
                               from kss in lstKeepSepActiveAssocSubset
                               where ksa.DeleteFlag == 0 && kss.AssocId == ksa.KeepSepAssoc1Id
                               select new KeepSeparateVm
                               {
                                   KeepSepLabel = KeepSepLabel.ASSOCKEEPSEP, // "ASSOC KEEP SEP"
                                   PersonId = ksa.KeepSepInmate2.PersonId,
                                   InmateNumber = ksa.KeepSepInmate2.InmateNumber,
                                   KeepSepReason = ksa.KeepSepReason,
                                   KeepSepType = ksa.KeepSeparateType,
                                   HousingUnitId = ksa.KeepSepInmate2.HousingUnitId,
                                   KeepSepTypeName = KeepSepTypeName.AA // "A-A"
                               }).GroupBy(n => new
                               {
                                   n.InmateNumber,
                                   n.KeepSepType,
                                   n.KeepSepReason
                               }).FirstOrDefault();
                if (ksaList != null)
                {
                    lstKeepSeparate.AddRange(ksaList);
                }

                // Subset Keep Sep List : ASSOCIATION KEEP SEPARATE To SUBSET
                var ksiList = (from kss in _context.KeepSepSubsetInmate
                               from kas in lstKeepSepActiveAssocSubset
                               where kas.AssocId == kss.KeepSepAssoc1Id
                                     && kas.SubsetId == kss.KeepSepAssoc1SubsetId && kss.DeleteFlag == 0
                               select new KeepSeparateVm
                               {
                                   KeepSepLabel = KeepSepLabel.SUBSETKEEPSEP, // "SUBSET KEEP SEP"
                                   PersonId = kss.KeepSepInmate2.PersonId,
                                   InmateNumber = kss.KeepSepInmate2.InmateNumber,
                                   KeepSepType = kss.KeepSeparateType,
                                   KeepSepReason = kss.KeepSepReason,
                                   HousingUnitId = kss.KeepSepInmate2.HousingUnitId,
                                   KeepSepTypeName = KeepSepTypeName.AS // "A-S"
                               }).GroupBy(n => new
                               {
                                   n.InmateNumber,
                                   n.KeepSepType,
                                   n.KeepSepReason
                               }).FirstOrDefault();
                if (ksiList != null)
                {
                    lstKeepSeparate.AddRange(ksiList);
                }
            }

            lstKeepSeparate.ForEach(item =>
            {
                if (item.HousingUnitId.HasValue)
                {
                    item.HousingDetail = _facilityHousingService.GetHousingDetails(item.HousingUnitId.Value);
                    item.HousingUnitListId = item.HousingDetail.HousingUnitListId;
                }

                if (item.PersonId <= 0)
                {
                    return;
                }

                var person = (from p in _context.Person
                              where p.PersonId == item.PersonId
                              select new
                              {
                                  p.PersonLastName,
                                  p.PersonFirstName,
                                  p.PersonMiddleName
                              }).Single();

                item.PersonLastName = person.PersonLastName;
                item.PersonFirstName = person.PersonFirstName;
                item.PersonMiddleName = person.PersonMiddleName;
            });

            return lstKeepSeparate.OrderBy(ln => ln.PersonLastName).ToList();
        }

        //Get Association Alert Details
        public List<PersonClassificationDetails> GetAssociation(int personId)
        {
            //Both IQueryable<> and List<> query will take 1ms
            //So i used list query due to code simplification
            List<Lookup> lookupslist = _context.Lookup.Where(w => w.LookupType == LookupConstants.CLASSGROUP).ToList();
            List<Lookup> lookupSubsetlist = _context.Lookup.Where(w => w.LookupType == LookupConstants.CLASSGROUPSUB).ToList();
            List<PersonClassificationDetails> pcDetails = (from pc in _context.PersonClassification
                where pc.InactiveFlag == 0 && pc.PersonClassificationDateFrom.HasValue
                    && pc.PersonClassificationDateFrom <= DateTime.Now
                    && (!pc.PersonClassificationDateThru.HasValue
                        || pc.PersonClassificationDateThru >= DateTime.Now)
                    && pc.PersonId == personId
                orderby pc.PersonClassificationId descending
                select new PersonClassificationDetails
                {
                    ClassificationType = lookupslist
                        .SingleOrDefault(f => f.LookupIndex == pc.PersonClassificationTypeId).LookupDescription,
                    ClassificationSubset = lookupSubsetlist
                        .SingleOrDefault(f => f.LookupIndex == pc.PersonClassificationSubsetId).LookupDescription,
                    ClassificationNotes = pc.PersonClassificationNotes,
                    ClassificationStatus = pc.PersonClassificationStatus,
                    ClassificationTypeId = pc.PersonClassificationTypeId,
                    ClassificationSubsetId = pc.PersonClassificationSubsetId
                }).ToList();
            return pcDetails;
        }
    }
}
