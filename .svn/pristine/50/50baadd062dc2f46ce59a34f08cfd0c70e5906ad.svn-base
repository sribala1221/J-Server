using System;
using System.Collections.Generic;
using System.Linq;
using GenerateTables.Models;
using ServerAPI.ViewModels;
using ServerAPI.Utilities;

namespace ServerAPI.Services
{
    public class ClassifyAlertsAssociationService : IClassifyAlertsAssociationService
    {
        private readonly AAtims _context;

        public ClassifyAlertsAssociationService(AAtims context)
        {
            _context = context;

        }
        public ClassifyAlertAssociationVm GetClassifyAssociationDetails(KeepSepSearchVm value)
        {
            ClassifyAlertAssociationVm AlertAssociationDetails = GetHousingDetail(value.FacilityId);
            ClassifyAlertAssociationVm detail = GetAssociationDetails(value);
            AlertAssociationDetails.AlertAssociation = detail.AlertAssociation;
            AlertAssociationDetails.AlertAssociationCount = detail.AlertAssociationCount;
            return AlertAssociationDetails;
        }

        private ClassifyAlertAssociationVm GetAssociationDetails(KeepSepSearchVm value)
        {
            ClassifyAlertAssociationVm classifyAlertAssociation = new ClassifyAlertAssociationVm();
            List<int> housingUnitListIds = new List<int>();
            if (value.HousingGroupId > 0)
            {
                housingUnitListIds = _context.HousingGroupAssign
                        .Where(w => w.HousingGroupId == value.HousingGroupId && w.HousingUnitListId.HasValue && w.DeleteFlag != 1)
                        .Select(s => s.HousingUnitListId ?? 0).ToList();
            }

            classifyAlertAssociation.AlertAssociation = _context.PersonClassification
                .Where(w => w.Person.Inmate.Any(a => a.FacilityId == value.FacilityId && a.InmateActive == 1) &&
                    w.InactiveFlag == 0 && w.PersonClassificationDateFrom < DateTime.Now &&
                    (!w.PersonClassificationDateThru.HasValue || w.PersonClassificationDateThru >= DateTime.Now) &&
                    (value.GenderId == 0 || w.Person.PersonSexLast == value.GenderId) &&
                    (value.RaceId == 0 || w.Person.PersonRaceLast == value.RaceId) &&
                    (value.ClassifyId == 0 || w.PersonClassificationTypeId == value.ClassifyId) &&
                    (value.InmateId == 0 || w.Person.Inmate.Any(a => a.InmateId == value.InmateId)) &&
                    (string.IsNullOrEmpty(value.ClassificationReason) || w.Person.Inmate.Any(a =>
                        a.InmateClassification.InmateClassificationReason == value.ClassificationReason)) &&
                    (value.IllegalAlienOnly == false || w.Person.IllegalAlienFlag == value.IllegalAlienOnly) &&
                    (string.IsNullOrEmpty(value.HousingLocation) || w.Person.Inmate.Any(a =>
                        a.HousingUnit.HousingUnitLocation == value.HousingLocation)) &&
                    (value.HousingUnitListId == 0 ||
                        w.Person.Inmate.Any(a => a.HousingUnit.HousingUnitListId == value.HousingUnitListId)) &&
                    (value.HousingGroupId == 0 || w.Person.Inmate.Any(a =>
                        housingUnitListIds.Any(s => s == a.HousingUnit.HousingUnitListId))) &&
                    (string.IsNullOrEmpty(value.ClassificationStatus) ||
                        w.PersonClassificationStatus == value.ClassificationStatus) &&
                    (value.FlagIndex == 0 || w.PersonClassificationFlag.Any(a => a.AssocFlagIndex == value.FlagIndex))
                )
                .Select(s => new PersonClassificationDetails
                {
                    ClassificationType = s.PersonClassificationType,
                    ClassificationSubset = s.PersonClassificationSubset,
                    PersonClassificationId = s.PersonClassificationId,
                    ClassificationNotes = s.PersonClassificationNotes,
                    ClassificationStatus = s.PersonClassificationStatus,
                    PersonId = s.PersonId,
                    DateFrom = s.PersonClassificationDateFrom,
                    DateTo = s.PersonClassificationDateThru,
                    PersonDetails = new PersonVm
                    {
                        PersonLastName = s.Person.PersonLastName,
                        PersonFirstName = s.Person.PersonFirstName,
                        PersonMiddleName = s.Person.PersonMiddleName,
                        PersonIllegalAlien = s.Person.IllegalAlienFlag,
                        PersonSexLast = s.Person.PersonSexLast,
                        PersonRaceLast = s.Person.PersonRaceLast
                    },
                    HousingDetails = s.Person.Inmate.Where(we => we.HousingUnitId == we.HousingUnit.HousingUnitId)
                        .Select(se => new HousingDetail
                        {
                            HousingUnitLocation = se.HousingUnit.HousingUnitLocation,
                            HousingUnitNumber = se.HousingUnit.HousingUnitNumber,
                            HousingUnitBedNumber = se.HousingUnit.HousingUnitBedNumber,
                            HousingUnitBedLocation = se.HousingUnit.HousingUnitBedLocation
                        }).FirstOrDefault(),
                    InmateDetails = s.Person.Inmate.Select(si => new InmateVm
                    {
                        InmateNumber = si.InmateNumber,
                        InmateId = si.InmateId,
                        InmateCurrentTrack = si.InmateCurrentTrack,
                        FacilityId = si.FacilityId,
                    }).FirstOrDefault(),
                    ClassificationTypeId = s.PersonClassificationTypeId,
                    ClassificationSubsetId = s.PersonClassificationSubsetId

                }).OrderBy(o => o.PersonDetails.PersonLastName).ThenBy(t => t.PersonDetails.PersonFirstName).ToList();

            classifyAlertAssociation.AlertAssociationCount = GetClassifyAssociationCount(classifyAlertAssociation.AlertAssociation);
            return classifyAlertAssociation;
        }

        private List<AlertAssociationVm> GetClassifyAssociationCount(List<PersonClassificationDetails> value)
        {
            List<Lookup> lookupslist = _context.Lookup.Where(w => w.LookupType == LookupConstants.CLASSGROUP).ToList();
            List<Lookup> lookupsSublist = _context.Lookup.Where(w => w.LookupType == LookupConstants.CLASSGROUPSUB).ToList();
            List<AlertAssociationVm> alertAssociationCount = value.GroupBy(g => new
            {
                g.ClassificationTypeId,
                g.ClassificationSubsetId
            }).Select(s => new AlertAssociationVm
            {
                ClassificationType = lookupslist.Single(k => k.LookupIndex == s.Key.ClassificationTypeId)
                    .LookupDescription,
                ClassificationSubset = s.Key.ClassificationSubsetId > 0 ?
                    lookupsSublist.Single(k => k.LookupIndex == s.Key.ClassificationSubsetId)
                        .LookupDescription : null,
                SubsetCount = s.Count()
            }).ToList();
            alertAssociationCount.ForEach(item =>
            {
                item.Count = item.ClassificationType.Distinct().Count();
            });
            return alertAssociationCount;
        }

        private ClassifyAlertAssociationVm GetHousingDetail(int facilityId)
        {
            ClassifyAlertAssociationVm classifyAlertAssociation = new ClassifyAlertAssociationVm
            {
                HousingBuilding = _context.HousingUnit
                    .Where(w => (w.HousingUnitInactive == 0 || !w.HousingUnitInactive.HasValue) &&
                        w.FacilityId == facilityId)
                    .GroupBy(g => new {g.HousingUnitLocation, g.Facility.FacilityId})
                    .Select(s => new HousingUnitListVm {HousingUnitLocation = s.Key.HousingUnitLocation}).Distinct()
                    .ToList(),
                HousingNumber = _context.HousingUnit
                    .Where(w => w.HousingUnitInactive == 1 ||
                        !w.HousingUnitInactive.HasValue && w.FacilityId == facilityId)
                    .GroupBy(g => new {g.HousingUnitListId, g.HousingUnitNumber, g.HousingUnitLocation}).Select(s =>
                        new HousingUnitListVm
                        {
                            HousingUnitListId = s.Key.HousingUnitListId,
                            HousingUnitNumber = s.Key.HousingUnitNumber,
                            HousingUnitLocation = s.Key.HousingUnitLocation
                        }).ToList(),
                HousingGroup = _context.HousingGroup.Where(w => w.DeleteFlag == 0 && w.FacilityId == facilityId)
                    .Select(s => new HousingGroupVm
                    {
                        HousingGroupId = s.HousingGroupId,
                        GroupName = s.GroupName,
                        AllowBatchSafetyCheck = s.AllowBatchSafetyCheck ?? 0
                    }).ToList()
            };


            return classifyAlertAssociation;
        }
    }
}

